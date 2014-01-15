//--------------------------------------------------------------------------
// 
//  Some Parts Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: PrioritizingTaskScheduler.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using log4net;
using NewsComponents.Utils;
using RssBandit.Common.Logging;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>
    ///   Provides a task scheduler that supports reprioritizing previously queued tasks.
    /// </summary>
    public sealed class PrioritizingTaskScheduler : TaskScheduler
    {
       // private static readonly ILog Log = DefaultLog.GetLogger(typeof (PrioritizingTaskScheduler));

        /// <summary>
        ///   Whether the current thread is processing work items.
        /// </summary>
        [ThreadStatic] private static bool _currentThreadIsProcessingItems;

        /// <summary>
        ///   Whether the scheduler is currently processing work items.
        /// </summary>
        private int _delegatesQueuedOrRunning; // protected by lock(_tasks)       

        /// <summary>
        ///   The maximum concurrency level allowed by this scheduler.
        /// </summary>
        private readonly int _maxDegreeOfParallelism = 4;

        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        /// <summary>
        ///   Initializes a new instance of this class whose max degree of parallelism is read from the RSSBandit.exe.config file. Default value is 4 if no 
        ///   configured value found.
        /// </summary>
        public PrioritizingTaskScheduler()
        {
            try
            {
                int newMax = Common.Configuration.ReadAppSettingsEntry("MaxDownloadThreads", _maxDegreeOfParallelism);
                if (newMax > 0 && newMax < 50)
                {
                    _maxDegreeOfParallelism = newMax;
                }
            }
            catch (ConfigurationErrorsException cex)
            {
                Log.Error("Failed to read 'MaxDownloadThreads' from .config", cex);
            }
        }

        /// <summary>
        ///   Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the
        ///   specified degree of parallelism.
        /// </summary>
        /// <param name = "maxDegreeOfParallelism">The maximum degree of parallelism provided by this scheduler.</param>
        public PrioritizingTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        ///   Gets the maximum concurrency level supported by this scheduler.
        /// </summary>
        public override sealed int MaximumConcurrencyLevel
        {
            get { return _maxDegreeOfParallelism; }
        }

        /// <summary>
        ///   Reprioritizes a previously queued task to the back of the queue.
        /// </summary>
        /// <param name = "task">The task to be reprioritized.</param>
        /// <returns>Whether the task could be found and moved to the back of the queue.</returns>
        public bool Deprioritize(Task task)
        {
            lock (_tasks)
            {
                LinkedListNode<Task> node = _tasks.Find(task);
                if (node != null)
                {
                    _tasks.Remove(node);
                    _tasks.AddLast(node);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///   Reprioritizes a previously queued task to the front of the queue.
        /// </summary>
        /// <param name = "task">The task to be reprioritized.</param>
        /// <returns>Whether the task could be found and moved to the front of the queue.</returns>
        public bool Prioritize(Task task)
        {
            lock (_tasks)
            {
                LinkedListNode<Task> node = _tasks.Find(task);
                if (node != null)
                {
                    _tasks.Remove(node);
                    _tasks.AddFirst(node);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///   Gets all of the tasks currently queued to the scheduler.
        /// </summary>
        /// <returns>An enumerable of the tasks currently queued to the scheduler.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(_tasks);
                if (lockTaken) return _tasks.ToArray();
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }

        /// <summary>
        ///   Queues a task to the scheduler.
        /// </summary>
        /// <param name = "task">The task to be queued.</param>
        protected override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        /// <summary>
        ///   Removes a previously queued item from the scheduler.
        /// </summary>
        /// <param name = "task">The task to be removed.</param>
        /// <returns>Whether the task could be removed from the scheduler.</returns>
        protected override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        /// <summary>
        ///   Attempts to execute the specified task on the current thread.
        /// </summary>
        /// <param name = "task">The task to be executed.</param>
        /// <param name = "taskWasPreviouslyQueued"></param>
        /// <returns>Whether the task could be executed on the current thread.</returns>
        protected override sealed bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued) TryDequeue(task);

            // Try to run the task.
            return base.TryExecuteTask(task);
        }

        /// <summary>
        ///   Informs the ThreadPool that there's work to be executed for this scheduler.
        /// </summary>
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
                                                   {
                                                       // Note that the current thread is now processing work items.
                                                       // This is necessary to enable inlining of tasks into this thread.
                                                       _currentThreadIsProcessingItems = true;
                                                       try
                                                       {
                                                           // Process all available items in the queue.
                                                           while (true)
                                                           {
                                                               Task item;
                                                               lock (_tasks)
                                                               {
                                                                   // When there are no more items to be processed,
                                                                   // note that we're done processing, and get out.
                                                                   if (_tasks.Count == 0)
                                                                   {
                                                                       --_delegatesQueuedOrRunning;
                                                                       break;
                                                                   }

                                                                   // Get the next item from the queue
                                                                   item = _tasks.First.Value;
                                                                   _tasks.RemoveFirst();
                                                               }

                                                               // Execute the task we pulled out of the queue
                                                               base.TryExecuteTask(item);
                                                           }
                                                       }
                                                           // We're done processing items on the current thread
                                                       finally
                                                       {
                                                           _currentThreadIsProcessingItems = false;
                                                       }
                                                   }, null);
        }

        /// <summary>
        ///   Picks up and executes the next item in the queue.
        /// </summary>
        /// <param name = "ignored">Ignored.</param>
        private void ProcessNextQueuedItem(object ignored)
        {
            Task t;
            lock (_tasks)
            {
                if (_tasks.Count > 0)
                {
                    t = _tasks.First.Value;
                    _tasks.RemoveFirst();
                }
                else return;
            }
            base.TryExecuteTask(t);
        }
    }
}