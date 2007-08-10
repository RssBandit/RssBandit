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
using System.Windows.Forms;
using Infragistics.Win.UltraWinTree;
using NewsComponents.Collections;
using NewsComponents.Utils;
using RssBandit;
using NewsComponents;
using NewsComponents.Feed;
using Logger = RssBandit.Common.Logging;

namespace RssBandit
{

	#region ThreadWorker
	/// <summary>
	/// ThreadWorker can be used to start a task in the background and report
	/// progress to the UI.
	/// </summary>
	internal class ThreadWorker:ThreadWorkerBase {

		#region Task enum
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
			TransformCategory,
			RefreshCommentFeeds
		}
		#endregion

		/// <summary>
		/// The worker method.
		/// </summary>
		/// <param name="task"></param>
		protected override Exception DoTaskWork(ThreadWorkerTaskBase task) {

			RssBanditApplication app = ((ThreadWorkerTask) task).Application;
			int maxTasks = 0, currentTask = 0;

			bool force = false;			
			UltraTreeNode feedNode;
			string stylesheet, html;

			switch ((Task)task.TaskID) {

				case Task.LoadFeedlist:
					app.LoadFeedList();
					break;
			
				case Task.LoadSpecialFeeds:
					app.InitializeFlaggedItems();
					break;

				case Task.RefreshFeeds:
					force = (bool)task.Arguments[0];
					app.FeedHandler.RefreshFeeds(force);
					break;

				case Task.RefreshCommentFeeds:
					force = (bool)task.Arguments[0];
					app.CommentFeedsHandler.RefreshFeeds(force);
					break;

				case Task.RefreshCategoryFeeds:
					string category = (string)task.Arguments[0];
					force = (bool)task.Arguments[1];
					app.FeedHandler.RefreshFeeds(category, force);
					break;

				case Task.LoadCommentFeed:
					NewsItem item = (NewsItem)task.Arguments[0];
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
					RaiseBackgroundTaskFinished(task, maxTasks, currentTask, null, new object[]{result, item, task.Arguments[1], task.Arguments[2]});
					return null;

				case Task.TransformFeed:
					FeedInfo feed = (FeedInfo)task.Arguments[0];
					feedNode = (UltraTreeNode)task.Arguments[1];
					stylesheet = (string)task.Arguments[2];					
					html = app.FormatFeed(stylesheet, feed); 
					
					RaiseBackgroundTaskFinished(task, maxTasks, currentTask, null, 
						new object[]{feedNode, html});
					return null;

				case Task.TransformCategory:
					FeedInfoList feeds = (FeedInfoList)task.Arguments[0];
					feedNode = (UltraTreeNode)task.Arguments[1];
					stylesheet = (string)task.Arguments[2];
					
					html = app.FormatFeeds(stylesheet, feeds); 

					RaiseBackgroundTaskFinished(task, maxTasks, currentTask, null, 
						new object[]{feedNode, html});
					return null;

				default:
					throw new InvalidOperationException("Unhandled ThreadWorker Task: " + task.TaskID);
			}

			// default event if no result(s) from processing:
			RaiseBackgroundTaskFinished(task, maxTasks, currentTask, null, null);
			return null;
		}

		#region ctor's
		/// <summary>
		/// Constructor used by caller using ThreadPool
		/// </summary>
		public ThreadWorker():
			base() {
		}
		/// <summary>
		/// Constructor called by callee using ThreadPool OR ThreadStart
		/// </summary>
		/// <param name="task"></param>
		public ThreadWorker (ThreadWorkerTask task):
			base(task) {
		}

		#endregion
	}

	#endregion

	#region ThreadWorkerTask
	
	/// <summary>
	/// ThreadWorkerTask connects the application with the 
	/// concrete task worker instance.
	/// </summary>
	internal class ThreadWorkerTask: ThreadWorkerTaskBase {
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ThreadWorkerTask"/> class.
		/// </summary>
		/// <param name="task">The task.</param>
		/// <param name="progressHandler">The progress handler.</param>
		/// <param name="application">The RssBanditApplication.</param>
		/// <param name="args">The ask args.</param>
		public ThreadWorkerTask(ThreadWorker.Task task, ThreadWorkerProgressHandler progressHandler, RssBanditApplication application, params object[] args):
			base(task, progressHandler, args) {
			this.app = application;
		}

		/// <summary>
		/// Gets the application instance.
		/// </summary>
		public RssBanditApplication Application { get { return this.app; } }
		private readonly RssBanditApplication app;

		/// <summary>
		/// Override to return our own implementation worker.
		/// </summary>
		/// <returns>ThreadWorkerBase</returns>
		public override ThreadWorkerBase GetWorkerInstance() {
			return new ThreadWorker(this);
		}

	}
	#endregion
	
	#region ThreadResultManager
	/// <summary>
	/// Serialize the results we get from threads to the be
	/// processed one by one and sync. to the provided SynchronizingObject
	/// </summary>
	internal sealed class ThreadResultManager {
		#region private ivars
		private RssBanditApplication owner;
		private PriorityQueue resultInfos;
		private System.Windows.Forms.Timer processResult;
		#endregion

		#region ctor's
		public ThreadResultManager(RssBanditApplication owner, System.Windows.Forms.Timer uiDispatcher) {
			this.owner = owner;
			this.processResult = uiDispatcher;
			resultInfos = PriorityQueue.Synchronize(new PriorityQueue());
			
			// what we catch on:
			this.owner.FeedHandler.UpdateFeedStarted += new NewsHandler.UpdateFeedStartedHandler(this.OnUpdateFeedStarted);
			this.owner.FeedHandler.OnUpdatedFeed += new NewsHandler.UpdatedFeedCallback(this.OnUpdatedFeed);
			this.owner.FeedHandler.OnUpdateFeedException += new NewsHandler.UpdateFeedExceptionCallback(this.OnUpdateFeedException);

			//processResult = new System.Timers.Timer(250);
			processResult.Tick += new EventHandler(OnProcessResultTick); //new System.Timers.ElapsedEventHandler(OnProcessResultElapsed);
			processResult.Interval = 250;
			processResult.Start();
		}
		#endregion

		//		/// <summary>
		//		/// The timer synchronization object. Usually the main form.
		//		/// </summary>
		//		public ISynchronizeInvoke SynchronizingObject {
		//			get { return processResult.SynchronizingObject; }
		//			set { processResult.SynchronizingObject = value; }
		//		}

		#region private members
		//		private void OnProcessResultElapsed(object sender, System.Timers.ElapsedEventArgs e) {
		//			// get them out of the resultInfos list and deliver
		//			if (resultInfos.Count > 0) {
		//				ThreadResultInfo t = (ThreadResultInfo)resultInfos.Dequeue();
		//				if (t.Args is NewsHandler.UpdateFeedEventArgs) {
		//					this.owner.OnUpdateFeedStarted(t.sender, (NewsHandler.UpdateFeedEventArgs)t.Args);
		//				} else if (t.Args is NewsHandler.UpdatedFeedEventArgs) {
		//					this.owner.OnUpdatedFeed(t.sender, (NewsHandler.UpdatedFeedEventArgs)t.Args);
		//				} else if (t.Args is NewsHandler.UpdateFeedExceptionEventArgs) {
		//					this.owner.OnUpdateFeedException(t.sender, (NewsHandler.UpdateFeedExceptionEventArgs)t.Args);
		//				}
		//			}
		//		}

		private void OnProcessResultTick(object sender, EventArgs e) {
			// get them out of the resultInfos list and deliver
			if (resultInfos.Count > 0) {
				ThreadResultInfo t = (ThreadResultInfo)resultInfos.Dequeue();
				if (t.Args is NewsHandler.UpdateFeedEventArgs) {
					this.owner.OnUpdateFeedStarted(t.sender, (NewsHandler.UpdateFeedEventArgs)t.Args);
				} else if (t.Args is NewsHandler.UpdatedFeedEventArgs) {
					this.owner.OnUpdatedFeed(t.sender, (NewsHandler.UpdatedFeedEventArgs)t.Args);
				} else if (t.Args is NewsHandler.UpdateFeedExceptionEventArgs) {
					this.owner.OnUpdateFeedException(t.sender, (NewsHandler.UpdateFeedExceptionEventArgs)t.Args);
				}
			}
		}

		private void OnUpdateFeedStarted(object sender, NewsHandler.UpdateFeedEventArgs e) {
			this.resultInfos.Enqueue(e.Priority, new ThreadResultInfo(sender, e));
		}
		private void OnUpdatedFeed(object sender, NewsHandler.UpdatedFeedEventArgs e) {
			this.resultInfos.Enqueue(e.Priority, new ThreadResultInfo(sender, e));
		}
		private void OnUpdateFeedException(object sender, NewsHandler.UpdateFeedExceptionEventArgs e) {
			this.resultInfos.Enqueue(e.Priority, new ThreadResultInfo(sender, e));
		}

		#region queued container
		/// <summary>
		/// Result container store
		/// </summary>
		class ThreadResultInfo {
			public ThreadResultInfo(object sender, EventArgs args) {
				this.sender = sender;
				this.Args = args;
			}
			public readonly object sender;
			public readonly EventArgs Args;
		}
		#endregion

		#endregion

	}
	#endregion

}
