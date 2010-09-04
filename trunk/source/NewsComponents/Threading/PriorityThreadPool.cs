#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

// Stephen Toub
// stoub@microsoft.com
// 
// PriorityThreadPool.cs
// C# ThreadPool that executes waiting delegates in order of priority as supplied
// to QueueUserWorkItem.

#region Namespaces
using System;
using System.Threading;
using System.Collections;
using System.Configuration;
using NewsComponents.Collections;
#endregion

namespace NewsComponents.Threading
{
	/// <summary>Managed thread pool.</summary>
	public sealed class PriorityThreadPool
	{
		#region Constants
		/// <summary>Maximum number of threads the thread pool has at its disposal.</summary>
		private static int _maxWorkerThreads = 4;
		#endregion

		#region Member Variables
		/// <summary>Queue of all the callbacks waiting to be executed.</summary>
		static PriorityQueue _waitingCallbacks;
		/// <summary>
		/// Used to signal that a worker thread is needed for processing.  Note that multiple
		/// threads may be needed simultaneously and as such we use a semaphore instead of
		/// an auto reset event.
		/// </summary>
		static Semaphore _workerThreadNeeded;
		/// <summary>List of all worker threads at the disposal of the thread pool.</summary>
		static ArrayList _workerThreads;
		/// <summary>Number of threads currently active.</summary>
		static int _inUseThreads;
		#endregion

		#region Construction
		private PriorityThreadPool() {}

		/// <summary>Initialize the thread pool.</summary>
		static PriorityThreadPool()
		{
			// Create our thread stores; we handle synchronization ourself
			// as we may run into situtations where multiple operations need to be atomic.
			// We keep track of the threads we've created just for good measure; not actually
			// needed for any core functionality.
			_waitingCallbacks = new PriorityQueue();
			_workerThreads = new ArrayList();
			_inUseThreads = 0;

			string maxWorkerThreadsFromConfig = ConfigurationManager.AppSettings["MaxDownloadThreads"];
			if (maxWorkerThreadsFromConfig != null && maxWorkerThreadsFromConfig.Length > 0) {
				try {
					int newMax = Convert.ToInt32(maxWorkerThreadsFromConfig);
					if (newMax > 0) {
						_maxWorkerThreads = newMax;
					}
				} catch {}
			}

			// Create our "thread needed" event
			_workerThreadNeeded = new Semaphore(0);
			
			// Create all of the worker threads
			for(int i=0; i<_maxWorkerThreads; i++)
			{
				// Create a new thread and add it to the list of threads.
				Thread newThread = new Thread(new ThreadStart(ProcessQueuedItems));
				newThread.Name = "ManagedPriorityPooledThread #" + i.ToString();
				_workerThreads.Add(newThread);

				// Configure the new thread and start it
				newThread.IsBackground = true;
				newThread.TrySetApartmentState(ApartmentState.MTA);
				newThread.Start();
			}
		}
		#endregion

		#region Public Methods
		/// <summary>Queues a user work item to the thread pool.</summary>
		/// <param name="callback">
		/// A WaitCallback representing the delegate to invoke when the thread in the 
		/// thread pool picks up the work item.
		/// </param>
		/// <param name="priority">The priority for this callback.</param>
		public static void QueueUserWorkItem(WaitCallback callback, int priority)
		{
			// Queue the delegate with no state
			QueueUserWorkItem(callback, null, priority);
		}

		/// <summary>Queues a user work item to the thread pool.</summary>
		/// <param name="callback">
		/// A WaitCallback representing the delegate to invoke when the thread in the 
		/// thread pool picks up the work item.
		/// </param>
		/// <param name="state">
		/// The object that is passed to the delegate when serviced from the thread pool.
		/// </param>
		/// <param name="priority">The priority for this callback.</param>
		public static void QueueUserWorkItem(WaitCallback callback, object state, int priority)
		{
			// Create a waiting callback that contains the delegate and its state.
			// At it to the processing queue, and signal that data is waiting.
			WaitingCallback waiting = new WaitingCallback(callback, state);
			lock(_waitingCallbacks.SyncRoot) { _waitingCallbacks.Enqueue(priority, waiting); }
			_workerThreadNeeded.AddOne();
		}

		/// <summary>Empties the work queue of any queued work items.</summary>
		public static void EmptyQueue()
		{
			lock(_waitingCallbacks.SyncRoot) 
			{ 
				try 
				{
					// Try to dispose of all remaining state
					foreach(object obj in _waitingCallbacks)
					{
						((WaitingCallback)obj).Dispose();
					}
				} 
				catch
				{
					// Make sure an error isn't thrown.
				}

				// Clear all waiting items and reset the number of worker threads currently needed
				// to be 0 (there is nothing for threads to do)
				_waitingCallbacks.Clear();
				_workerThreadNeeded.Reset(0);
			}
		}
		#endregion

		#region Properties
		/// <summary>Gets the number of threads at the disposal of the thread pool.</summary>
		public static int MaxThreads { get { return _maxWorkerThreads; } }
		/// <summary>Gets the number of currently active threads in the thread pool.</summary>
		public static int ActiveThreads { get { return _inUseThreads; } }
		/// <summary>Gets the number of callback delegates currently waiting in the thread pool.</summary>
		public static int WaitingCallbacks { get { lock(_waitingCallbacks.SyncRoot) { return _waitingCallbacks.Count; } } }
		#endregion

		#region Thread Processing
		/// <summary>A thread worker function that processes items from the work queue.</summary>
		private static void ProcessQueuedItems()
		{
			// Process indefinitely
			while(true)
			{
				// Get the next item in the queue.  If there is nothing there, go to sleep
				// for a while until we're woken up when a callback is waiting.
				WaitingCallback callback = null;
				while (callback == null)
				{
					// Try to get the next callback available.  We need to lock on the 
					// queue in order to make our count check and retrieval atomic.
					lock(_waitingCallbacks.SyncRoot)
					{
						if (_waitingCallbacks.Count > 0)
						{
							callback = (WaitingCallback)_waitingCallbacks.Dequeue();
						}
					}

					// If we can't get one, go to sleep.
					if (callback == null) _workerThreadNeeded.WaitOne();
				}

				// We now have a callback.  Execute it.  Make sure to accurately
				// record how many callbacks are currently executing.
				try 
				{
					Interlocked.Increment(ref _inUseThreads);
					callback.Callback(callback.State);
				} 
				catch
				{
					// Ignore any errors; not our problem.
				}
				finally
				{
					Interlocked.Decrement(ref _inUseThreads);
				}
			}
		}
		#endregion

		/// <summary>Used to hold a callback delegate and the state for that delegate.</summary>
		internal class WaitingCallback : IDisposable
		{
			#region Member Variables
			/// <summary>Callback delegate for the callback.</summary>
			private WaitCallback _callback;
			/// <summary>State with which to call the callback delegate.</summary>
			private object _state;
			#endregion

			#region Construction
			/// <summary>Initialize the callback holding object.</summary>
			/// <param name="callback">Callback delegate for the callback.</param>
			/// <param name="state">State with which to call the callback delegate.</param>
			public WaitingCallback(WaitCallback callback, object state)
			{
				_callback = callback;
				_state = state;
			}

			// NOTE: Even though this implements IDisposable and it's good practice
			// to also implement a finalizer when implementing Dispose, we're not going 
			// to, as there is no real need in this case.
			#endregion

			#region Properties
			/// <summary>Gets the callback delegate for the callback.</summary>
			public WaitCallback Callback { get { return _callback; } }
			/// <summary>Gets the state with which to call the callback delegate.</summary>
			public object State { get { return _state; } }
			#endregion

			#region Implementation of IDisposable
			/// <summary>Disposes of the contained state if it is disposable.</summary>
			public void Dispose()
			{
				if (State is IDisposable) ((IDisposable)State).Dispose();
			}
			#endregion
		}
	}
}
