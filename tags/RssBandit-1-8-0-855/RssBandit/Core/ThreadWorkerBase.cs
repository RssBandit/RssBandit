#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.Threading;
using System.ComponentModel;
using NewsComponents.Utils;

namespace RssBandit {

	#region ThreadWorkerTaskBase
	/// <summary>
	/// Describes Tasks to work on in background
	/// </summary>
	public class ThreadWorkerTaskBase {
		protected ThreadWorkerTaskBase(Enum taskID, ThreadWorkerProgressHandler progressHandler, object[] args) {
			this.TaskID = taskID;
			this.Arguments = args;
			this.ProgressHandler = progressHandler;
		}

		/// <summary>
		/// Override to return your own implementation worker.
		/// </summary>
		/// <returns>ThreadWorkerBase</returns>
		public virtual ThreadWorkerBase GetWorkerInstance() {
			return new ThreadWorkerBase(this);
		}

		/// <summary>
		/// Task Arguments
		/// </summary>
		public object[] Arguments;
		/// <summary>
		/// Task ID required to manage tasks
		/// </summary>
		public Enum TaskID;
		/// <summary>
		/// The delegate method (callback) on the sender to call
		/// </summary>
		protected internal ThreadWorkerProgressHandler ProgressHandler;

		/// <summary>
		/// The thread running the task
		/// </summary>
		protected internal Thread WorkerThread;

		private ThreadWorkerTaskBase() {}
	}

	#endregion

	#region ThreadWorkerBase class
	/// <summary>
	/// ThreadWorkerBase can be used to start a task in the 
	/// background and report progress to the UI.
	/// </summary>
	/// <remarks>You have to inherit that class and implement 
	/// DoTaskWork() method with your very own long running tasks.
	/// </remarks>
	public class ThreadWorkerBase {

		#region DuplicateTaskQueued enum
		/// <summary>
		/// Controls the action ThreadWorker should apply,
		/// if the same task was yet queued.
		/// </summary>
		public enum DuplicateTaskQueued {
			/// <summary>
			/// Let the yet running task as is and ignore the new task (not started again)
			/// </summary>
			Ignore,
			/// <summary>
			/// Let the yet running task as is and start it again
			/// </summary>
			Allowed,
			/// <summary>
			/// Wait for the queued task to finish, then start the new one
			/// </summary>
			Wait,
			/// <summary>
			/// Abort the queued/running task, then start the new one
			/// </summary>
			Abort,
		}
		#endregion

		/// <summary>
		/// Local Method for the actual work.
		/// </summary>
		/// <returns>Exception object on any failure for a task, 
		/// or null if task was cancelled or succeeds.</returns>
		private Exception LocalRunProcess() {
		
			if (RaiseBackroundTaskStarted(this.task))
				return null;	//cancelled

			try {			
				return DoTaskWork(this.task);

			} catch (ThreadAbortException) { /* ignore */ 
			} catch (Exception ex) {
                ex.PreserveExceptionStackTrace();
				RaiseBackgroundTaskFinished(this.task, 1, 1, ex, null);
				return ex;
			}
			return null;
		}

		/// <summary>
		/// Have to be overridden to impl. the real work a background task
		/// have to fulfill
		/// </summary>
		/// <param name="task"></param>
		protected virtual Exception DoTaskWork(ThreadWorkerTaskBase task) {
			return new NotImplementedException("Inherit the class and override DoTaskWork() to work on tasks in background");
		}


		#region ivar's
		
		/// <summary>
		/// Current task to work on
		/// </summary>
		private ThreadWorkerTaskBase task;
		
		#endregion

		#region ctor's
		static ThreadWorkerBase() {
			queuedTasks = Hashtable.Synchronized(new Hashtable());
			taskStartInfos = Queue.Synchronized(new Queue());
			taskResultInfos = Queue.Synchronized(new Queue());
			waitForGlobalThreadResource = false;
			taskTimer = new System.Timers.Timer(100);
			taskTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTaskTimerElapsed);
			taskTimer.Start();
		}
		/// <summary>
		/// Constructor used by caller using ThreadPool
		/// </summary>
		public ThreadWorkerBase() {	}

		/// <summary>
		/// Constructor called by callee using ThreadPool OR ThreadStart
		/// </summary>
		/// <param name="task">ThreadWorkerTaskBase</param>
		public ThreadWorkerBase(ThreadWorkerTaskBase task) {
			this.task = task;
		}

		#endregion

		#region public methods

		/// <summary>
		/// Returns true, if a task is yet queued to work on
		/// </summary>
		/// <param name="task">Task</param>
		/// <returns>bool</returns>
		public static bool IsTaskQueued(Enum task) {
			return queuedTasks.ContainsKey(task);
		}

		/// <summary>
		/// Returns true, if a task is waiting to get queued for run
		/// </summary>
		/// <param name="task">Task</param>
		/// <returns>bool</returns>
		public static bool IsTaskWaitingForGlobalThreadResource(Enum task) {
			IEnumerator myEnumerator = taskStartInfos.GetEnumerator();
			while ( myEnumerator.MoveNext() ) {
				TaskStartInfo info = (TaskStartInfo)myEnumerator.Current;
				if (info.Task.TaskID.Equals(task))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the Thread of a queued/running task.
		/// </summary>
		/// <param name="task">Task</param>
		/// <returns>Thread. If the task is not queued/running, it returns null</returns>
		public static Thread GetTaskThread(Enum task) {
			if (!queuedTasks.ContainsKey(task))
				return null;

			int its = 0, maxIts = 10;
			ThreadWorkerTaskBase tw = (ThreadWorkerTaskBase)queuedTasks[task];
			Thread t = tw.WorkerThread;
			while (t == null) {
				// ThreadPool task: queued but not yet started, wait...
				Thread.Sleep(100);

				if (!queuedTasks.ContainsKey(task))
					return null;

				tw = (ThreadWorkerTaskBase)queuedTasks[task];
				t = tw.WorkerThread;
				if (++its > maxIts)
					break;	// do not iterate forever
			}
			return t;
		}

		/// <summary>
		/// Abort a running task, if queued.
		/// </summary>
		/// <param name="task">Task to abort</param>
		public static void AbortTask(Enum task) {
			if (queuedTasks.ContainsKey(task)) {
				Thread t = GetTaskThread(task);
				if (null != t) {
					try {
						t.Abort();
					} catch {}
				}
				queuedTasks.Remove(task);
				RaiseBackroundTasksRunning(queuedTasks.Count);
			}
		}

		/// <summary>
		/// Waits until a queued/running task finishes execution
		/// </summary>
		/// <param name="task">Task to wait for</param>
		/// <returns>True, if the thread terminates, false if the wait gets a timeout</returns>
		public static bool WaitForTask(Enum task) {
			bool terminated = true;
			if (queuedTasks.ContainsKey(task)) {
				
				Thread t = GetTaskThread(task);
				if (null != t) {
					if (t.IsAlive) {
						try {
							terminated = t.Join(15000);
						} catch {}
					}
				}

				if (terminated) {
					queuedTasks.Remove(task);
					RaiseBackroundTasksRunning(queuedTasks.Count);
				}
				
			}
			return terminated;
		}

		/// <summary>
		/// Abort all running/queued tasks 
		/// </summary>
		public static void AbortAllTasks() {
			foreach (ThreadWorkerTaskBase t in queuedTasks.Values) {
				if (null != t && t.WorkerThread != null) {
					try {
						t.WorkerThread.Abort();
					} catch {}
				}
			}
			queuedTasks.Clear();
			RaiseBackroundTasksRunning(0);
		}

		/// <summary>
		/// Clears all tasks that are waiting for the global resource. 
		/// </summary>
		public static void ClearTasksWaitingForGlobalThreadResource() {
			taskStartInfos.Clear();
		}

		/// <summary>
		/// Queue a task using ThreadPool. If the same task is yet running,
		/// the call will wait for the first one to finish work (DuplicateTaskQueued.Wait).
		/// </summary>
		/// <param name="task">Task to start (ThreadWorkerTaskBase)</param>
		/// <returns>True, if the task was successfully queued up
		/// to the ThreadPool, else false.</returns>
		public static bool QueueTask(ThreadWorkerTaskBase task) {
			return QueueTask(task , DuplicateTaskQueued.Ignore);
		}

		/// <summary>
		/// Queue a task using ThreadPool.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <param name="action">Duplicate task action</param>
		/// <returns>True, if the task was successfully queued up
		/// to the ThreadPool, else false.</returns>
		public static bool QueueTask(ThreadWorkerTaskBase task, DuplicateTaskQueued action) {
			
			if (waitForGlobalThreadResource) {
				// delay startup of the thread
				if (IsTaskWaitingForGlobalThreadResource(task.TaskID)) {
					if (action == DuplicateTaskQueued.Ignore)
						return false;	// yet waiting
					if (action != DuplicateTaskQueued.Allowed) {
						return false;	// we do not have to wait/abort, they are not yet even started anyway
					}
				}

				TaskStartInfo startInfo = new TaskStartInfo(TaskStartInfo.StartMethod.ThreadPool, task, action);
				taskStartInfos.Enqueue(startInfo);
				return true;
			
			}

			if (IsTaskQueued(task.TaskID)) {
				
				if (action == DuplicateTaskQueued.Ignore)
					return false;

				if (action != DuplicateTaskQueued.Allowed) {	// wait, or abort running task thread
					if (action == DuplicateTaskQueued.Abort) {
						AbortTask(task.TaskID);
					}
					if (action != DuplicateTaskQueued.Wait) {
						WaitForTask(task.TaskID);
					}
				}
			}

			queuedTasks.Add(task.TaskID, task);
			ThreadWorkerBase wc = task.GetWorkerInstance();
			return ThreadPool.QueueUserWorkItem( new WaitCallback (wc.RunProcess));

		}

		/// <summary>
		/// Queue a task using a freshly created Thread. If the same task is yet running,
		/// the call will wait for the first one to finish work (DuplicateTaskQueued.Wait).
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <returns>True, if the task was successfully started, else false.</returns>
		public static bool StartTask(ThreadWorkerTaskBase task) {
			return StartTask(task, DuplicateTaskQueued.Ignore);
		}

		/// <summary>
		/// Queue a task using a freshly created Thread.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <param name="action">DuplicateTaskQueued</param>
		/// <returns>True, if the task was successfully started, else false.</returns>
		public static bool StartTask(ThreadWorkerTaskBase task, DuplicateTaskQueued action) {
			
			if (waitForGlobalThreadResource) {
				// delay startup of the thread
				if (IsTaskWaitingForGlobalThreadResource(task.TaskID)) {
					if (action == DuplicateTaskQueued.Ignore)
						return false;
					if (action != DuplicateTaskQueued.Allowed) {
						return false;	// we do not have to wait/abort, they are not yet even started anyway
					}
				}

				TaskStartInfo startInfo = new TaskStartInfo(TaskStartInfo.StartMethod.ThreadStart, task, action);
				taskStartInfos.Enqueue(startInfo);
				return true;
			
			}

			if (IsTaskQueued(task.TaskID)) {
				
				if (action == DuplicateTaskQueued.Ignore)
					return false;

				if (action != DuplicateTaskQueued.Allowed) {	// wait, or abort running task thread
					if (action == DuplicateTaskQueued.Abort) {
						AbortTask(task.TaskID);
					}
					if (action != DuplicateTaskQueued.Wait) {
						WaitForTask(task.TaskID);
					}
				}
			}

			ThreadWorkerBase wc = task.GetWorkerInstance();
			Thread t = new Thread( new ThreadStart(wc.RunProcess));
			t.IsBackground = true; //make them a daemon - prevent thread callback issues
			task.WorkerThread = t;
			t.Start();
			queuedTasks.Add(task.TaskID, task);
			return true;
		}

		/// <summary>
		/// Execute a task in synchronized manner.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <returns>An Exception object on any failure within the task, 
		/// or null if the task was successfully finished or cancelled.</returns>
		public static Exception RunTaskSynchronized(ThreadWorkerTaskBase task) {
			return RunTaskSynchronized(task, DuplicateTaskQueued.Ignore);
		}

		/// <summary>
		/// Execute a task in synchronized manner.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <param name="action">DuplicateTaskQueued</param>
		/// <returns>An Exception object on any failure within the task, 
		/// or null if the task was successfully finished or cancelled.</returns>
		public static Exception RunTaskSynchronized(ThreadWorkerTaskBase task, DuplicateTaskQueued action) {

			if (IsTaskQueued(task.TaskID)) {
				
				if (action == DuplicateTaskQueued.Ignore)
					return null;

				if (action != DuplicateTaskQueued.Allowed) {	// wait, or abort running task thread
					if (action == DuplicateTaskQueued.Abort) {
						AbortTask(task.TaskID);
					}
					if (action != DuplicateTaskQueued.Wait) {
						WaitForTask(task.TaskID);
					}
				}
			}

			ThreadWorkerBase wc = task.GetWorkerInstance();
			queuedTasks.Add(task.TaskID, task);
			try {
				return wc.LocalRunProcess();
			} finally {
				queuedTasks.Remove(task);
			}
		}

		/// <summary>
		/// Method for ThreadPool QueueWorkerItem
		/// </summary>
		/// <param name="obj"></param>
		public void RunProcess ( object obj ) {
			Thread.CurrentThread.IsBackground = true; //make them a daemon
			
			if (queuedTasks.ContainsKey(task.TaskID)) {	// add Thread ref. to enable cancel/suspend, etc.
				ThreadWorkerTaskBase tw = (ThreadWorkerTaskBase)queuedTasks[task.TaskID];
				tw.WorkerThread = Thread.CurrentThread;
			}

			RaiseBackroundTasksRunning(queuedTasks.Count);
			try {
				LocalRunProcess();
			} finally {
				queuedTasks.Remove(task.TaskID);
				RaiseBackroundTasksRunning(queuedTasks.Count);
			}

		}

		/// <summary>
		/// Method for ThreadStart delegate
		/// </summary>
		internal void RunProcess() {
			Thread.CurrentThread.IsBackground = true; //make them a daemon
			RaiseBackroundTasksRunning(queuedTasks.Count);
			try {
				LocalRunProcess();
			} finally {
				queuedTasks.Remove(task.TaskID);
				RaiseBackroundTasksRunning(queuedTasks.Count);
			}
		}
		#endregion

		#region private helpers
		
		private bool RaiseBackroundTaskStarted(ThreadWorkerTaskBase task) {
			if (OnBackgroundTaskStarted != null) {
				ThreadWorkerProgressArgs args = new ThreadWorkerProgressArgs(task.TaskID, 1, 0, null, false, null); 
				OnBackgroundTaskStarted(task.WorkerThread, args);
			}
			return false;
		}

		protected void RaiseBackroundTaskProgress(ThreadWorkerTaskBase task, int total, int current, Exception error, object result) {
			ThreadWorkerProgressArgs args = new ThreadWorkerProgressArgs(task.TaskID, total, current, error, false, result); 
			taskResultInfos.Enqueue(new TaskResultInfo(task, args));
		}

		protected void RaiseBackgroundTaskFinished(ThreadWorkerTaskBase task, int total, int current, Exception error, object result) {
			ThreadWorkerProgressArgs args = new ThreadWorkerProgressArgs(task.TaskID, total, current, error, true, result); 
			taskResultInfos.Enqueue(new TaskResultInfo(task, args));
		}

		private static void RaiseBackroundTasksRunning(int current) {
			if (OnBackgroundTaskRunning != null) {
				BackgroundTaskRunningArgs args = new BackgroundTaskRunningArgs(current);
				object sender = System.Threading.Thread.CurrentThread;
				try {
					OnBackgroundTaskRunning(sender, args);
				} catch {}
				if (args.Cancel) {
					AbortAllTasks();
				}
			}
		}

		public static event BackgroundTaskRunningHandler OnBackgroundTaskRunning;
		public static event ThreadWorkerProgressHandler OnBackgroundTaskStarted;
		private static Hashtable queuedTasks;
		private static Queue taskStartInfos;
		private static Queue taskResultInfos;
		private static bool waitForGlobalThreadResource;
		
		/// <summary>
		/// Set this to true, if all calls to StartTask/QueueTask
		/// should enqueue the requests as long the global resource
		/// required by the threaded task is not available.
		/// If it gets set to false, all queued thread tasks will start
		/// as they get queued for wait.
		/// 
		/// A global resource could be e.g. the Internet connection.
		/// </summary>
		public static bool WaitForGlobalThreadResource {
			get { return waitForGlobalThreadResource; }
			set { waitForGlobalThreadResource = value; }
		}

		/// <summary>
		/// object implementing ISynchronizeInvoke to sync. result
		/// delivery. Usually a form or a control.
		/// </summary>
		public static ISynchronizeInvoke SynchronizingObject {
			get { return synchronizingObject; }
			set { synchronizingObject = value; }
		}
		private static System.Timers.Timer taskTimer;
		private static ISynchronizeInvoke synchronizingObject;
		
		/// <summary>
		/// Called when taskTimer elapsed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
		private static void OnTaskTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
			// get them out of the tasksInfos lists
			if (taskResultInfos.Count > 0) {
				TaskResultInfo t = (TaskResultInfo)taskResultInfos.Dequeue();
				//DispatchInvocationToGuiThread(new MethodInvocation(t.Task.ProgressHandler, new object[]{t.Task, t.Args}));

                GuiInvoker.Invoke(null,
                    () => t.Task.ProgressHandler(t.Task, t.Args));
				//t.Task.ProgressHandler(t.Task, t.Args);
			}
			
			// we do not start all waiting task thread at once:
			if (! waitForGlobalThreadResource && taskStartInfos.Count > 0) {
				
				TaskStartInfo tInfo = (TaskStartInfo)taskStartInfos.Dequeue();
				if (TaskStartInfo.StartMethod.ThreadStart == tInfo.ThreadStartMethod) {
					
					StartTask(tInfo.Task, tInfo.Action);

				} else if (TaskStartInfo.StartMethod.ThreadPool == tInfo.ThreadStartMethod ) {
					
					QueueTask(tInfo.Task, tInfo.Action);
				
				}
			}
		}

		#endregion

		#region TaskStartInfo class

		private class TaskStartInfo {
			public enum StartMethod {
				/// <summary>
				/// Use the Thread.Start() method to start the task
				/// </summary>
				ThreadStart,
				/// <summary>
				/// Use the ThreadPool to enqueue the new thread
				/// </summary>
				ThreadPool,
			}

			public TaskStartInfo(StartMethod startMethod, ThreadWorkerTaskBase task, DuplicateTaskQueued action) {
				this.startMethod = startMethod;
				this.task = task;
				this.action = action;
			}
			private readonly StartMethod startMethod;
			public StartMethod ThreadStartMethod {
				get { return startMethod; }	
			}

			private readonly ThreadWorkerTaskBase task;
			public ThreadWorkerTaskBase Task {
				get { return task; }	
			}
			private readonly DuplicateTaskQueued action;
			public DuplicateTaskQueued Action {
				get { return action; }	
			}
		}
		#endregion

		#region TaskResultInfo class

		private class TaskResultInfo {
			public TaskResultInfo(ThreadWorkerTaskBase task, ThreadWorkerProgressArgs args) {
				this.Task = task;
				this.Args = args;
			}
			public readonly ThreadWorkerTaskBase Task;
			public readonly ThreadWorkerProgressArgs Args;
		}
		#endregion

	}
	#endregion

	#region ThreadWorkerProgressArgs helper class

	/// <summary>
	/// ThreadWorkerProgressArgs class transport the ThreadWorker progress arguments
	/// to the callee. 
	/// </summary>
	public class ThreadWorkerProgressArgs : CancelEventArgs {
		/// <summary>
		/// Task ID that is currently in progress
		/// </summary>
		public readonly Enum TaskID;
		/// <summary>
		/// The exception (if any) while processing.
		/// </summary>
		public readonly Exception Exception;
		/// <summary>
		/// Maximum number of sub-tasks of a thread task.
		/// </summary>
		public readonly int MaxProgress;
		/// <summary>
		/// Current index of sub-tasks of a thread task.
		/// </summary>
		public readonly int CurrentProgress;
		/// <summary>
		/// Is true, if the task finish.
		/// </summary>
		public readonly bool Done;
		/// <summary>
		/// The result container.
		/// </summary>
		public readonly object Result;

		public ThreadWorkerProgressArgs(int total, int current, Exception exception, bool done, object result):
			base(false) {
			this.Exception = exception;
			this.Done = done;
			this.MaxProgress = total;
			this.CurrentProgress = current;
			this.Result = result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ThreadWorkerProgressArgs"/> class.
		/// </summary>
		/// <param name="taskID">The task ID.</param>
		/// <param name="total">The total.</param>
		/// <param name="current">The current.</param>
		public ThreadWorkerProgressArgs(Enum taskID, int total, int current):
			this(taskID, total, current, null, false, null) {}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ThreadWorkerProgressArgs"/> class.
		/// </summary>
		/// <param name="taskID">The task ID.</param>
		/// <param name="total">The total.</param>
		/// <param name="current">The current.</param>
		/// <param name="exception">The exception.</param>
		public ThreadWorkerProgressArgs(Enum taskID, int total, int current, Exception exception):
			this(taskID, total, current, exception, false, null) {}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ThreadWorkerProgressArgs"/> class.
		/// </summary>
		/// <param name="taskID">The task ID.</param>
		/// <param name="total">The total.</param>
		/// <param name="current">The current.</param>
		/// <param name="exception">The exception.</param>
		/// <param name="done">if set to <c>true</c> the thread has finished the work.</param>
		/// <param name="result">The result.</param>
		public ThreadWorkerProgressArgs(Enum taskID, int total, int current, Exception exception, bool done, object result):
			base(false) {
			this.TaskID = taskID;
			this.Exception = exception;
			this.Done = done;
			this.MaxProgress = total;
			this.CurrentProgress = current;
			this.Result = result;
		}
	}

	/// <summary>
	/// The callback routine using the ShowProgressArgs class
	/// </summary>
	public delegate void ThreadWorkerProgressHandler(object sender, ThreadWorkerProgressArgs e);

	#endregion
	
	#region BackgroundTaskRunningArgs helper class

	/// <summary>
	/// BackgroundTaskRunningArgs class transport the ThreadWorker current state as arguments
	/// to the subscriber. If you set Cancel to true, ALL background tasks are cancelled!
	/// </summary>
	public class BackgroundTaskRunningArgs : CancelEventArgs {
		public int CurrentRunning;

		public BackgroundTaskRunningArgs():
			this(0) {}
		public BackgroundTaskRunningArgs(int currentRunning):base(false) {
			this.CurrentRunning = currentRunning;
		}
	}

	/// <summary>
	/// The callback routine using the BackgroundTaskRunningArgs class
	/// </summary>
	public delegate void BackgroundTaskRunningHandler(object sender, BackgroundTaskRunningArgs e);
	
	#endregion

}
