#region CVS Version Header
/*
 * $Id: ThreadWorker.cs,v 1.16 2005/05/10 18:57:15 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/10 18:57:15 $
 * $Revision: 1.16 $
 */
#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using NewsComponents.Utils;
using RssBandit;
using NewsComponents;
using NewsComponents.Feed;
using Logger = RssBandit.Common.Logging;

namespace RssBandit
{

	#region ThreadWorker class
	/// <summary>
	/// ThreadWorker can be used to start a task in the background and report
	/// progress to the UI.
	/// </summary>
	internal class ThreadWorker {

		/// <summary>
		/// Our tasks to work on in background
		/// </summary>
		public enum Task {
			LoadFeedlist,
			LoadSpecialFeeds,
			RefreshFeeds,
			RefreshCategoryFeeds,
			TransformFeed,			
			LoadCommentFeed,
			TransformCategory
		}

		/// <summary>
		/// Controls the action ThreadWorker should apply,
		/// if the same task was yet queued/running.
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

		/// <summary>
		/// Local Method for the actual work.
		/// </summary>
		private void LocalRunProcess() {
			switch (m_task) {
				
					#region LoadFeedlist
				case Task.LoadFeedlist:
					if (OnTaskProgress(1, 0) )
						return;	// cancelled
					try {
						RssBanditApplication app = (RssBanditApplication)this.m_taskArguments[0];
						app.LoadFeedList();
						OnTaskFinished(1, 1, null, null);
					} catch (Exception ex) {
						OnTaskFinished(1, 0, ex, null);
					}
					break;
					#endregion

					#region LoadSpecialFeeds
				case Task.LoadSpecialFeeds:
					if (OnTaskProgress(1, 0) )
						return;	// cancelled
					try {
						RssBanditApplication app = (RssBanditApplication)this.m_taskArguments[0];
						app.InitializeFlaggedItems();
						OnTaskFinished(1, 1, null, null);
					} catch (Exception ex) {
						OnTaskFinished(1, 0, ex, null);
					}
					break;
					#endregion

					#region RefreshFeeds
				case Task.RefreshFeeds:
					if (OnTaskProgress(1, 0) )
						return;	// cancelled
					try {
						RssBanditApplication app = (RssBanditApplication)this.m_taskArguments[0];
						bool force = (bool)this.m_taskArguments[1];
						app.FeedHandler.RefreshFeeds(force);
						OnTaskFinished(1, 1, null, null);
					} catch (Exception ex) {
						OnTaskFinished(1, 0, ex, null);
					}
					break;
					#endregion

					#region RefreshCategoryFeeds
				case Task.RefreshCategoryFeeds:
					if (OnTaskProgress(1, 0) )
						return;	// cancelled
					try {
						RssBanditApplication app = (RssBanditApplication)this.m_taskArguments[0];
						string category = (string)this.m_taskArguments[1];
						bool force = (bool)this.m_taskArguments[2];
						app.FeedHandler.RefreshFeeds(category, force);
						OnTaskFinished(1, 1, null, null);
					} catch (Exception ex) {
						OnTaskFinished(1, 0, ex, null);
					}
					break;
					#endregion

					#region LoadCommentFeed
				case Task.LoadCommentFeed:
					if (OnTaskProgress(1, 0) )
						return;	// cancelled
					try {
						RssBanditApplication app = (RssBanditApplication)this.m_taskArguments[0];
						NewsItem item = (NewsItem)this.m_taskArguments[1];
						if (item == null)
							throw new InvalidOperationException("Non-Null task argument 'item' expected.");

						feedsFeed cmtFeed = new feedsFeed();
						cmtFeed.link = item.CommentRssUrl;
						cmtFeed.title = item.Feed.title;

						if (!StringHelper.EmptyOrNull(item.Feed.authUser)) {	// take over credential settings
							string u = null, p = null;
							NewsHandler.GetFeedCredentials(item.Feed, ref u, ref p);
							NewsHandler.SetFeedCredentials(cmtFeed, u, p);
						}
						
						object result = app.FeedHandler.GetItemsForFeed(cmtFeed);
						OnTaskFinished(1, 1, null, new object[]{result, item, this.m_taskArguments[2], this.m_taskArguments[3]});

					} catch (Exception ex) {
						OnTaskFinished(1, 0, ex, new object[]{null, this.m_taskArguments[1], this.m_taskArguments[2], this.m_taskArguments[3]});
					}

					break;
					#endregion

					#region TransformFeed
				case Task.TransformFeed:					
					try {
						RssBanditApplication app = (RssBanditApplication)this.m_taskArguments[0];
						FeedInfo feed = (FeedInfo)this.m_taskArguments[1];
						TreeNode feedNode = (TreeNode)this.m_taskArguments[2];
						string stylesheet = (string) this.m_taskArguments[3];
						IComparer newsItemSorter = (IComparer) this.m_taskArguments[4];
						feed.ItemsList.Sort(newsItemSorter); 
						string html = app.FormatFeed(stylesheet, feed); 
						OnTaskFinished(1, 1, null, new object[]{feedNode, html});
					} catch (Exception ex) {
						OnTaskFinished(1, 0, ex, null);
					}
					break;
					#endregion

					#region TransformCategory
				case Task.TransformCategory:					
					try {
						RssBanditApplication app = (RssBanditApplication)this.m_taskArguments[0];
						FeedInfoList feeds = (FeedInfoList)this.m_taskArguments[1];
						TreeNode feedNode = (TreeNode)this.m_taskArguments[2];
						string stylesheet = (string) this.m_taskArguments[3];
						IComparer newsItemSorter = (IComparer) this.m_taskArguments[4];
						
						foreach(FeedInfo feed in feeds){
							feed.ItemsList.Sort(newsItemSorter); 
						}
						
						string html = app.FormatFeeds(stylesheet, feeds); 
						
						OnTaskFinished(1, 1, null, new object[]{feedNode, html});
					} catch (Exception ex) {
						OnTaskFinished(1, 0, ex, null);
					}
					break;
					#endregion
				default:
					Debug.Assert(false, "Unknown ThreadWorker task: " + m_task.ToString());
					break;
			}
		}

		#region ivar's
		/// <summary>
		/// Usually a form or a winform control that implements "Invoke/BeginInvoke".
		/// Used a the synchronization object for callbacks.
		/// </summary>
		ContainerControl m_sender = null;

		/// <summary>
		/// Task to work on
		/// </summary>
		Task m_task = 0;

		/// <summary>
		/// Task arguments (optional)
		/// </summary>
		object[] m_taskArguments;

		/// <summary>
		/// The delegate method (callback) on the sender to call
		/// </summary>
		ShowProgressHandler m_showProgressDelegate = null;

		#endregion

		#region ctor's
		/// <summary>
		/// Constructor used by caller using ThreadPool
		/// </summary>
		public ThreadWorker() {
		}
		/// <summary>
		/// Constructor called by callee using ThreadPool OR ThreadStart
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="senderDelegate"></param>
		/// <param name="task"></param>
		/// <param name="taskArgs"></param>
		public ThreadWorker ( ContainerControl sender, ShowProgressHandler senderDelegate, Task task, params object[] taskArgs) {
			m_sender = sender;
			m_showProgressDelegate = senderDelegate;
			m_task = task;
			m_taskArguments = taskArgs;
		}

		/// <summary>
		/// Constructor called by callee using ThreadPool OR ThreadStart
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="senderDelegate"></param>
		/// <param name="task"></param>
		public ThreadWorker ( ContainerControl sender, ShowProgressHandler senderDelegate, Task task){
			m_sender = sender;
			m_showProgressDelegate = senderDelegate;
			m_task = task;
			m_taskArguments = new object[]{};
		}

		#endregion

		#region public methods

		/// <summary>
		/// Returns true, if a task is yet queued to work on
		/// </summary>
		/// <param name="task">Task</param>
		/// <returns>bool</returns>
		public static bool IsTaskQueued(Task task) {
			return queuedTasks.ContainsKey(task);
		}

		/// <summary>
		/// Returns the Thread of a queued/running task.
		/// </summary>
		/// <param name="task">Task</param>
		/// <returns>Thread. If the task is not queued/running, it returns null</returns>
		public static Thread GetTaskThread(Task task) {
			if (!queuedTasks.ContainsKey(task))
				return null;

			int its = 0, maxIts = 10;
			Thread t = queuedTasks[task] as Thread;
			while (t == null) {
				// ThreadPool task: queued but not yet started, wait...
				Thread.Sleep(100);

				if (!queuedTasks.ContainsKey(task))
					return null;

				t = queuedTasks[task] as Thread;
				if (++its > maxIts)
					break;	// do not iterate forever
			}
			return t;
		}

		/// <summary>
		/// Abort a running task, if queued.
		/// </summary>
		/// <param name="task">Task to abort</param>
		public static void AbortTask(Task task) {
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
		public static bool WaitForTask(Task task) {
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
			foreach (Thread t in queuedTasks.Values) {
				if (null != t) {
					try {
						t.Abort();
					} catch {}
				}
			}
			queuedTasks.Clear();
			RaiseBackroundTasksRunning(0);
		}


		/// <summary>
		/// Queue a task using ThreadPool.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <param name="sender">ContainerControl</param>
		/// <param name="callback">ShowProgressHandler</param>
		/// <param name="taskArgs">optional task parameters</param>
		/// <returns>True, if the task was successfully queued up
		/// to the ThreadPool, else false.</returns>
		public static bool QueueTask(Task task, ContainerControl sender, ShowProgressHandler callback, params object[] taskArgs) {
			return QueueTask(task, sender, callback, DuplicateTaskQueued.Ignore, taskArgs);
		}
		/// <summary>
		/// Queue a task using ThreadPool.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <param name="sender">ContainerControl</param>
		/// <param name="callback">ShowProgressHandler</param>
		/// <param name="action">Duplicate task action</param>
		/// <param name="taskArgs">optional task parameters</param>
		/// <returns>True, if the task was successfully queued up
		/// to the ThreadPool, else false.</returns>
		public static bool QueueTask(Task task, ContainerControl sender, ShowProgressHandler callback, DuplicateTaskQueued action, params object[] taskArgs) {
			
			if (IsTaskQueued(task)) {
				
				if (action == DuplicateTaskQueued.Ignore)
					return false;

				if (action != DuplicateTaskQueued.Allowed) {	// wait, or abort running task thread
					if (action == DuplicateTaskQueued.Abort) {
						AbortTask(task);
					}
					if (action != DuplicateTaskQueued.Wait) {
						WaitForTask(task);
					}
				}
			}

			queuedTasks.Add(task, null);	// thread worker thread added/set later

			ThreadWorker wc = new ThreadWorker(sender, callback, task, taskArgs);
			bool rc = ThreadPool.QueueUserWorkItem( new WaitCallback (wc.RunProcess));
			return rc ;
		}

		/// <summary>
		/// Queue a task using a freshly created Thread. Duplicate Tasks are ignored.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <param name="sender">ContainerControl</param>
		/// <param name="callback">ShowProgressHandler</param>
		/// <param name="taskArgs">optional task parameters</param>
		/// <returns>True, if the task was successfully started, else false.</returns>
		public static bool StartTask(Task task, ContainerControl sender, ShowProgressHandler callback, params object[] taskArgs) {
			return StartTask(task, sender, callback, DuplicateTaskQueued.Ignore, taskArgs);
		}
		/// <summary>
		/// Queue a task using a freshly created Thread.
		/// </summary>
		/// <param name="task">Task to start</param>
		/// <param name="sender">ContainerControl</param>
		/// <param name="callback">ShowProgressHandler</param>
		/// <param name="action">DuplicateTaskQueued</param>
		/// <param name="taskArgs">optional task parameters</param>
		/// <returns>True, if the task was successfully started, else false.</returns>
		public static bool StartTask(Task task, ContainerControl sender, ShowProgressHandler callback, DuplicateTaskQueued action, params object[] taskArgs) {
			
			if (IsTaskQueued(task)) {
				
				if (action == DuplicateTaskQueued.Ignore)
					return false;

				if (action != DuplicateTaskQueued.Allowed) {	// wait, or abort running task thread
					if (action == DuplicateTaskQueued.Abort) {
						AbortTask(task);
					}
					if (action != DuplicateTaskQueued.Wait) {
						WaitForTask(task);
					}
				}
			}

			ThreadWorker wc = new ThreadWorker(sender, callback, task, taskArgs);
			Thread t = new Thread( new ThreadStart(wc.RunProcess));			
			t.Priority = ThreadPriority.AboveNormal;
			t.IsBackground = true; //make them a daemon - prevent thread callback issues
			t.Start();
			queuedTasks.Add(task, t);
			return true;
		}

		/// <summary>
		/// Method for ThreadPool QueueWorkerItem
		/// </summary>
		/// <param name="obj"></param>
		public void RunProcess ( object obj ) {
			Thread.CurrentThread.IsBackground = true; //make them a daemon

			if (queuedTasks.ContainsKey(this.m_task))	// add Thread ref. to enable cancel/suspend, etc.
				queuedTasks[this.m_task] = Thread.CurrentThread;

			RaiseBackroundTasksRunning(queuedTasks.Count);

			if (obj != null) {
				object[] objArray = (object[]) obj;
				m_sender = (ContainerControl) objArray[0];
				m_showProgressDelegate = (ShowProgressHandler) objArray[1];
				m_task = (Task) objArray[2];
			}

			try {
				LocalRunProcess();
			} finally {
				queuedTasks.Remove(this.m_task);
				RaiseBackroundTasksRunning(queuedTasks.Count);
			}
		}

		/// <summary>
		/// Method for ThreadStart delegate
		/// </summary>
		public void RunProcess() {
			Thread.CurrentThread.IsBackground = true; //make them a daemon
			RaiseBackroundTasksRunning(queuedTasks.Count);
			try {
				LocalRunProcess();
			} finally {
				queuedTasks.Remove(this.m_task);
				RaiseBackroundTasksRunning(queuedTasks.Count);
			}
		}
		#endregion

		#region private helpers
		private bool OnTaskProgress(int total, int current) {
			return this.OnTaskProgress(total, current, null, null);
		}
		private bool OnTaskProgress(int total, int current, Exception error) {
			return this.OnTaskProgress(total, current, error, null);
		}
		private bool OnTaskProgress(int total, int current, Exception error, object result) {
			ShowProgressArgs args = new ShowProgressArgs(total, current, error, false, result); 
			object sender = System.Threading.Thread.CurrentThread;
			if (m_sender.InvokeRequired) {
				m_sender.Invoke(m_showProgressDelegate,  new object[] { sender, args } );
			} else {
				m_showProgressDelegate(sender, args);
			}
			return args.Cancel;
		}

//		private void OnTaskFinished(TreeNode feedNode, string result, Exception error) {
//			TransformedTreeArgs args = new TransformedTreeArgs(feedNode, result, error); 
//			object sender = System.Threading.Thread.CurrentThread;
//			if (m_sender.InvokeRequired) {
//				m_sender.Invoke(m_transformedFeedsDelegate,  new object[] { sender, args } );
//			} else {
//				m_transformedFeedsDelegate(sender, args);
//			}
//		}

		private void OnTaskFinished(int total, int current, Exception error, object result) {
			ShowProgressArgs args = new ShowProgressArgs(total, current, error, true, result); 
			object sender = System.Threading.Thread.CurrentThread;
			if (m_sender.InvokeRequired) {
				m_sender.Invoke(m_showProgressDelegate,  new object[] { sender, args } );
			} else {
				m_showProgressDelegate(sender, args);
			}
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
		private static Hashtable queuedTasks = Hashtable.Synchronized(new Hashtable(11));

		#endregion

	}
	#endregion
	
	#region ShowProgressArgs helper class

	/// <summary>
	/// ShowProgressArgs class transport the ThreadWorker progress arguments
	/// to the callee. 
	/// </summary>
	public class ShowProgressArgs : CancelEventArgs {
		public Exception Exception;
		public int MaxProgress;
		public int CurrentProgress;
		public bool Done;
		public object Result;

		public ShowProgressArgs(int total, int current):
			this(total, current, null, false, null) {}
		public ShowProgressArgs(int total, int current, Exception exception):
			this(total, current, exception, false, null) {}
		public ShowProgressArgs(int total, int current, Exception exception, bool done, object result):base(false) {
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
	public delegate void ShowProgressHandler(object sender, ShowProgressArgs e);
	
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
