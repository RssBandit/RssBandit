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
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Infragistics.Win.UltraWinTree;
using NewsComponents.Collections;
using NewsComponents.Threading;
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
			AnonymousDelegate,
			LoadAllFeedSourcesSubscriptions,
			LoadFeedSourceSubscriptions,
			//LoadSpecialFeeds,
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

			//if ((Task)task.TaskID == Task.AnonymousDelegate)
			//{
			//    Action action = (Action)task.Arguments[0];
			//    if (action == null)
			//        throw new InvalidOperationException("no Action delegate specified");
				
			//    action();
			//    // default event if no result(s) from processing:
			//    RaiseBackgroundTaskFinished(task, 1, 1, null, null);
			//    return null;
			//}

			RssBanditApplication app = ((ThreadWorkerTask) task).Application;
			int maxTasks = 0, currentTask = 0;

			bool force;			
			UltraTreeNode feedNode;
			string stylesheet, html;

			switch ((Task)task.TaskID) {

				case Task.LoadAllFeedSourcesSubscriptions:
					List<FeedSourceEntry> entries = (List<FeedSourceEntry>)task.Arguments[0];
					var finished = new ManualResetEvent(false);
					int max = entries.Count;
					int current = 0;

					for (int i = 0; i < max; i++ )
					{
						IndexedFeedSourceEntry e = new IndexedFeedSourceEntry(entries[i], i);
						
						PriorityThreadPool.QueueUserWorkItem(
							delegate(object state)
							{
								IndexedFeedSourceEntry fs = (IndexedFeedSourceEntry)state;
								int threadCurrent = fs.Index+1;
								
								try
								{
									app.LoadFeedSourceSubscriptions(fs.Entry, true);
									this.RaiseBackroundTaskProgress(task, max, threadCurrent, null, fs.Entry);
								}
								catch (Exception loadEx)
								{
									this.RaiseBackroundTaskProgress(task, max, threadCurrent, loadEx, fs.Entry);
								}
								finally
								{
									if (Interlocked.Increment(ref current) >= max)
									{
										if (finished != null)
											finished.Set();
									}
								}
							}, e, 1);
					}
					
					if (max > 0)
						finished.WaitOne(Timeout.Infinite, true);
					
					break;

				case Task.LoadFeedSourceSubscriptions:
					app.LoadFeedSourceSubscriptions((FeedSourceEntry)task.Arguments[0], true);
				    break;

				// code mocved to FlaggedItemsFeed migrate method:
				//case Task.LoadSpecialFeeds:
				//    app.InitializeFlaggedItems();
				//    break;

				case Task.RefreshFeeds:
					force = (bool)task.Arguments[0];
                    app.FeedSources.ForEach(s => s.RefreshFeeds(force)); 
					break;

				case Task.RefreshCommentFeeds:
					force = (bool)task.Arguments[0];
					app.CommentFeedsHandler.RefreshFeeds(force);
					break;

				case Task.RefreshCategoryFeeds:
					FeedSourceEntry entry = (FeedSourceEntry)task.Arguments[0];
					string category = (string)task.Arguments[1];
					force = (bool)task.Arguments[2];
					entry.Source.RefreshFeeds(category, force);
					break;

				case Task.LoadCommentFeed:
					INewsItem item = (INewsItem)task.Arguments[0];
					if (item == null)
						throw new InvalidOperationException("Non-Null task argument 'item' expected.");
                   
                    object result = null;

                    if ((item.Feed != null) && (item.Feed.owner is IFacebookFeedSource))
                    {
                        IFacebookFeedSource fbSource = item.Feed.owner as IFacebookFeedSource;
                        result = fbSource.GetCommentsForItem(item); 
                    }else{
                        NewsFeed cmtFeed = new NewsFeed();
                        cmtFeed.link = item.CommentRssUrl;
                        cmtFeed.title = item.Feed.title;

                        if (!string.IsNullOrEmpty(item.Feed.authUser))
                        {	// take over credential settings
                            string u = null, p = null;
                            FeedSource.GetFeedCredentials(item.Feed, ref u, ref p);
                            FeedSource.SetFeedCredentials(cmtFeed, u, p);
                        }

                        result = RssParser.DownloadItemsFromFeed(cmtFeed, app.Proxy, FeedSource.Offline);
                    }
					RaiseBackgroundTaskFinished(task, maxTasks, currentTask, null, new object[]{result, item, task.Arguments[1], task.Arguments[2]});
					return null;

				case Task.TransformFeed:
                    IFeedDetails feed = (IFeedDetails)task.Arguments[0];
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

		struct IndexedFeedSourceEntry
		{
			public FeedSourceEntry Entry;
			public int Index;
			public IndexedFeedSourceEntry(FeedSourceEntry e, int i)
			{
				Entry = e;
				Index = i;
			}
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

            this.owner.FeedSources.ForEach(delegate(FeedSource f) { ConnectFeedSourceEvents(f); });
									
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
		//				if (t.Args is FeedSource.UpdateFeedEventArgs) {
		//					this.owner.OnUpdateFeedStarted(t.sender, (FeedSource.UpdateFeedEventArgs)t.Args);
		//				} else if (t.Args is FeedSource.UpdatedFeedEventArgs) {
		//					this.owner.OnUpdatedFeed(t.sender, (FeedSource.UpdatedFeedEventArgs)t.Args);
		//				} else if (t.Args is FeedSource.UpdateFeedExceptionEventArgs) {
		//					this.owner.OnUpdateFeedException(t.sender, (FeedSource.UpdateFeedExceptionEventArgs)t.Args);
		//				}
		//			}
		//		}

		private void OnProcessResultTick(object sender, EventArgs e) {
			// get them out of the resultInfos list and deliver
			if (resultInfos.Count > 0) {
				ThreadResultInfo t = (ThreadResultInfo)resultInfos.Dequeue();
				if (t.Args is FeedSource.UpdateFeedEventArgs) {
					this.owner.OnUpdateFeedStarted(t.sender, (FeedSource.UpdateFeedEventArgs)t.Args);
				} else if (t.Args is FeedSource.UpdatedFeedEventArgs) {
					this.owner.OnUpdatedFeed(t.sender, (FeedSource.UpdatedFeedEventArgs)t.Args);
				} else if (t.Args is FeedSource.UpdateFeedExceptionEventArgs) {
					this.owner.OnUpdateFeedException(t.sender, (FeedSource.UpdateFeedExceptionEventArgs)t.Args);
				}
			}
		}

		private void OnUpdateFeedStarted(object sender, FeedSource.UpdateFeedEventArgs e) {
			this.resultInfos.Enqueue(e.Priority, new ThreadResultInfo(sender, e));
		}
		private void OnUpdatedFeed(object sender, FeedSource.UpdatedFeedEventArgs e) {
			this.resultInfos.Enqueue(e.Priority, new ThreadResultInfo(sender, e));
		}
		private void OnUpdateFeedException(object sender, FeedSource.UpdateFeedExceptionEventArgs e) {
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

        #region private methods 

        internal void ConnectFeedSourceEvents(FeedSource source)
        {
            source.UpdateFeedStarted += new FeedSource.UpdateFeedStartedHandler(this.OnUpdateFeedStarted);
            source.OnUpdatedFeed += new FeedSource.UpdatedFeedCallback(this.OnUpdatedFeed);
            source.OnUpdateFeedException += new FeedSource.UpdateFeedExceptionCallback(this.OnUpdateFeedException);
        }

        #endregion 

    }
	#endregion

}
