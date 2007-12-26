#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

// Original provided by Stephen Toub (stoub@microsoft.com)
// Modified to be used as a instance.
// PriorityThread.cs
// C# Thread Pool that executes waiting delegates in order of priority as supplied
// to QueueUserWorkItem. It can be used as a instance to hold multiple pools with 
// multiple threads. Default is one thread per instance.

#region Namespaces
using System.Threading;
using System.Collections;
using NewsComponents.Collections;
#endregion

namespace NewsComponents.Threading {
	
	/// <summary>
	/// Managed thread(s), that handles all requests posted to the worker queue.
	/// It executes waiting delegates in order of priority as supplied
	/// to QueueUserWorkItem. Instances can be created to hold multiple pools with 
	/// multiple threads. Default is one thread per instance.
	/// </summary>
	public class PriorityThread {
		#region Constants
		/// <summary>Default maximum number of threads the thread 
		/// pool has at its disposal.</summary>
		private static int _defaultMaxWorkerThreads = 1;
		#endregion

		#region Member Variables
		/// <summary>Queue of all the callbacks waiting to be executed.</summary>
		private PriorityQueue _waitingCallbacks;
		/// <summary>
		/// Used to signal that a worker thread is needed for processing.  Note that multiple
		/// threads may be needed simultaneously and as such we use a semaphore instead of
		/// an auto reset event.
		/// </summary>
		private Semaphore _workerThreadNeeded;
		/// <summary>List of all worker threads at the disposal of the thread pool.</summary>
		private ArrayList _workerThreads;
		/// <summary>Number of threads currently active.</summary>
		private int _inUseThreads;
		#endregion

		#region Construction
		/// <summary>Initialize the thread pool with one worker thread and
		/// ThreadPriority.Normal.</summary>
		public PriorityThread():this(_defaultMaxWorkerThreads, ThreadPriority.Normal) {}
		/// <summary>Initialize the thread pool.</summary>
		/// <param name="workerThreadCount">Amount of worker threads to use</param>
		/// <param name="priority">ThreadPriority of the worker threads</param>
		public PriorityThread(int workerThreadCount, ThreadPriority priority) {
			
			if (workerThreadCount < _defaultMaxWorkerThreads)
				workerThreadCount = _defaultMaxWorkerThreads;

			// Create our thread stores; we handle synchronization ourself
			// as we may run into situtations where multiple operations need to be atomic.
			// We keep track of the threads we've created just for good measure; not actually
			// needed for any core functionality.
			_waitingCallbacks = new PriorityQueue();
			_workerThreads = new ArrayList(workerThreadCount);
			_inUseThreads = 0;

			// Create our "thread needed" event
			_workerThreadNeeded = new Semaphore(0);
			
			// Create all of the worker threads
			for(int i=0; i<workerThreadCount; i++) {
				// Create a new thread and add it to the list of threads.
				Thread newThread = new Thread(new ThreadStart(ProcessQueuedItems));
				newThread.Name = "PriorityThread #" + i.ToString();
				newThread.Priority = priority;
				_workerThreads.Add(newThread);

				// Configure the new thread and start it
				newThread.IsBackground = true;
				newThread.ApartmentState = ApartmentState.MTA;
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
		public void QueueUserWorkItem(WaitCallback callback, int priority) {
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
		public void QueueUserWorkItem(WaitCallback callback, object state, int priority) {
			// Create a waiting callback that contains the delegate and its state.
			// At it to the processing queue, and signal that data is waiting.
			PriorityThreadPool.WaitingCallback waiting = new PriorityThreadPool.WaitingCallback(callback, state);
			lock(_waitingCallbacks.SyncRoot) { _waitingCallbacks.Enqueue(priority, waiting); }
			_workerThreadNeeded.AddOne();
		}

		/// <summary>Empties the work queue of any queued work items.</summary>
		public void EmptyQueue() {
			lock(_waitingCallbacks.SyncRoot) { 
				try {
					// Try to dispose of all remaining state
					foreach(object obj in _waitingCallbacks) {
						((PriorityThreadPool.WaitingCallback)obj).Dispose();
					}
				} 
				catch {
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
		/// <summary>Gets the number of currently active threads in the thread pool.</summary>
		public int ActiveThreads { get { return _inUseThreads; } }
		/// <summary>Gets the number of callback delegates currently waiting in the thread pool.</summary>
		public int WaitingCallbacks { get { lock(_waitingCallbacks.SyncRoot) { return _waitingCallbacks.Count; } } }
		#endregion

		#region Thread Processing
		/// <summary>A thread worker function that processes items from the work queue.</summary>
		private void ProcessQueuedItems() {
			// Process indefinitely
			while(true) {
				// Get the next item in the queue.  If there is nothing there, go to sleep
				// for a while until we're woken up when a callback is waiting.
				PriorityThreadPool.WaitingCallback callback = null;
				while (callback == null) {
					// Try to get the next callback available.  We need to lock on the 
					// queue in order to make our count check and retrieval atomic.
					lock(_waitingCallbacks.SyncRoot) {
						if (_waitingCallbacks.Count > 0) {
							callback = (PriorityThreadPool.WaitingCallback)_waitingCallbacks.Dequeue();
						}
					}

					// If we can't get one, go to sleep.
					if (callback == null) _workerThreadNeeded.WaitOne();
				}

				// We now have a callback.  Execute it.  Make sure to accurately
				// record how many callbacks are currently executing.
				try {
					Interlocked.Increment(ref _inUseThreads);
					callback.Callback(callback.State);
				} 
				catch (System.Exception e){ e.ToString();
					// Ignore any errors; not our problem.
				}
				finally {
					Interlocked.Decrement(ref _inUseThreads);
				}
			}
		}
		#endregion

	}
}
