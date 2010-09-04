#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using Lucene.Net.Index;
using NewsComponents.Feed;
using RssBandit.Common.Logging;

namespace NewsComponents.Search
{
	
	/// <summary>
	/// LuceneIndexer helps to process/index NewsItems and feedFeeds.
	/// </summary>
	internal class LuceneIndexer {
		
		public event EventHandler IndexingFinished;
		public event LuceneIndexingProgressEventHandler IndexingProgress;

		private readonly LuceneIndexModifier indexModifier;
		private bool indexingAllThreadRunning;
		private readonly object indexingAllSyncLock = new object();
		private readonly IList<FeedSource> newsHandlers;
		
		public LuceneIndexer(LuceneIndexModifier indexModifier, IList<FeedSource> newsHandlers) {
		
			this.indexModifier = indexModifier;
			if (this.indexModifier == null)
				throw new ArgumentNullException("indexModifier");

            this.newsHandlers = newsHandlers;
            if (this.newsHandlers == null || this.newsHandlers.Count == 0)
                throw new ArgumentException("newsHandlers");
		}

		/// <summary>
		/// Indexes all feeds with all items.
		/// </summary>
		/// <param name="restartInfos">The restart infos.</param>
		/// <param name="lastIndexedEntry">The last indexed entry.</param>
		public void IndexAll(IDictionary restartInfos, DictionaryEntry lastIndexedEntry) 
		{
			lock (indexingAllSyncLock)
			{
				if (indexingAllThreadRunning)
					return; // yet running

				if (restartInfos == null)
					restartInfos = new HybridDictionary();

				ThreadPool.QueueUserWorkItem(ThreadRun,
					new object[] {restartInfos, lastIndexedEntry});
				
				indexingAllThreadRunning = true;
			}
		}
		
	
		private void ThreadRun(object state) 
		{
			DateTime start = DateTime.Now;
			Thread.CurrentThread.Name = "BanditIndexAllThread";

			object[] stateArray = (object[])state;

			IDictionary restartInfo = (IDictionary)stateArray[0];
			DictionaryEntry lastIndexed = (DictionaryEntry)stateArray[1];
			
			DefaultLog.Info(String.Format("Lucene Indexing {0}started at {1}", restartInfo.Count == 0 ? String.Empty : "re-", start));

			if (lastIndexed.Key != null && lastIndexed.Value != null) 
			{	// always remove the last possibly not fully indexed feed 
				// (e.g. app.exit while initial indexing was in progress):
				this.RemoveNewsItems(lastIndexed.Value as string);
				restartInfo.Remove(lastIndexed.Key);
				DefaultLog.Info(String.Format("Lucene Indexing removed possible incomplete indexed documents of feed {0}", lastIndexed.Key) ); 
			}

			try {

                int feedCount = 0, itemCount = 0;

                foreach(FeedSource newsHandler in newsHandlers)
                {			   

                if (newsHandler.FeedsListOK &&
                    0 < newsHandler.GetFeeds().Count)
                {
                    int maxCount = newsHandler.GetFeeds().Count;
                    // we are working with a copy of the feed list to avoid 
                    // exceptions, if the original feedlist was modified while indexing:
                    string[] feedLinks = new string[maxCount];
                    newsHandler.GetFeeds().Keys.CopyTo(feedLinks, 0);
                    for (int i = 0; i < maxCount; i++)
                    {
                        string feedlink = feedLinks[i];

                        if (!newsHandler.IsSubscribed(feedlink))
                            continue;

                        feedCount++;
                        if (!restartInfo.Contains(feedlink))
                        {
                            // the ID gets generated on request:
                            string feedID = newsHandler.GetFeeds()[feedlink].id;
                            if (!this.RaiseIndexingProgress(feedCount, maxCount, feedlink, feedID))
                            {
                                // if there was a feed refresh meanwhile, we must ensure to get
                                // already indexed feeds not indexed again:
                                RemoveNewsItems(feedID);
                                // now add items to index:
                                itemCount += IndexNewsItems(newsHandler.GetCachedItemsForFeed(feedlink));
                                DefaultLog.Info("IndexAll() progress: " + feedCount + " feeds, " + itemCount + " items processed.");
                            }
                        }
                        else
                        {
                            // reset the feed ID, because it may not get saved 
                            // along with the subscription list on a initial indexing break:                           
                            INewsFeed f;
                            if (newsHandler.GetFeeds().TryGetValue(feedlink, out f))
                            {
                                f.id = restartInfo[feedlink] as string;
                            }
                        }
                    }//for(int i = 0; i < maxCount; i++)

                }//if (newsHandler.FeedsListOK &&

                }//foreach
					
					DateTime end = DateTime.Now;
					TimeSpan timeRequired = TimeSpan.FromTicks(end.Ticks - start.Ticks);
					DefaultLog.Info(String.Format("Lucene Indexing all items finished at {0}. Time required: {1}h:{2} min for {3} feeds with {4} items.", end, timeRequired.TotalHours, timeRequired.TotalMinutes, feedCount, itemCount ) ); 
				

				// are these statistics above also important for an end-user???
				this.RaiseIndexingFinished(/* statistics */);
				
				if (feedCount != 0 || itemCount != 0)
					indexModifier.Optimize();

			} 
			catch (ThreadAbortException) {
				// ignore
			} 
			catch (Exception e) {
				DefaultLog.Error("Failure while indexing items.", e);
			} 
			finally 
			{
				lock (indexingAllSyncLock)
				{
					indexingAllThreadRunning = false;
				}
			}
		}

		internal int IndexNewsItems(IList<INewsItem> newsItems) {
			
			if (newsItems != null) 
			{
				for (int i=0; i < newsItems.Count; i++) {
					INewsItem item = newsItems[i];
					if (item != null) {
						try {
							// we do not always have the content loaded:
                            if (item.ContentType == ContentType.None && item.Feed != null && item.Feed.owner != null)
                            {
                            	if (item.Feed.owner != null)
                            	{
                            		FeedSource handler = (FeedSource)item.Feed.owner;
                            		handler.GetCachedContentForItem(item);
                            	}
                            }
							indexModifier.Add(LuceneNewsItemSearch.Document(item), item.Language);
						} catch (Exception ex) {
							DefaultLog.Error("IndexNewsItems() error", ex);
						}
					}
				}
				return newsItems.Count;
			}
			return 0;
		}
	
		/// <summary>
		/// Removes the index documents of the news items.
		/// </summary>
		/// <param name="newsItems">The news items.</param>
		/// <returns>Number of index documents removed</returns>
		internal void RemoveNewsItems(IList newsItems) {
			if (newsItems != null && newsItems.Count > 0) {
				//int removed = 0;
				for (int i=0; i < newsItems.Count; i++) {
					INewsItem item = newsItems[i] as INewsItem;
					if (item != null) 
					{
						Term term = new Term(LuceneSearch.IndexDocument.ItemID, 
							LuceneNewsItemSearch.UID(item));
						indexModifier.Delete(term);
					}
				}
				//return removed;
			}
			//return 0;
		}
		
		/// <summary>
		/// Removes the index documents for news items of a feed.
		/// </summary>
		/// <param name="feedID">The feed ID.</param>
		/// <returns>Number of index documents removed</returns>
		internal void RemoveNewsItems(string feedID) {
			if (!string.IsNullOrEmpty(feedID)) {
				Term term = new Term(LuceneSearch.IndexDocument.FeedID, feedID);
				 indexModifier.Delete(term);
			}
			//return 0;
		}
		
		/// <summary>
		/// Removes the index documents for news items of a feed and
		/// the feed itself.
		/// </summary>
		/// <param name="feedID">The feed ID.</param>
		internal void RemoveFeed(string feedID) {
			if (!string.IsNullOrEmpty(feedID)) {
				Term term = new Term(LuceneSearch.IndexDocument.FeedID, feedID);
				indexModifier.DeleteFeed(term);
			}
			//return 0;
		}
		
		private void RaiseIndexingFinished() {
			if (IndexingFinished != null)
				try { IndexingFinished(this, EventArgs.Empty); } catch {}
		}

		private bool RaiseIndexingProgress(int current, int max, string yetIndexedFeedUrl, string yetIndexedFeedID) {
			bool toReturn = false;
			if (IndexingProgress != null) {
				LuceneIndexingProgressCancelEventArgs cancelEventArgs = new LuceneIndexingProgressCancelEventArgs(toReturn, current, max, yetIndexedFeedUrl, yetIndexedFeedID);
				try { IndexingProgress(this, cancelEventArgs); } catch{}
				return cancelEventArgs.Cancel;
			}
			return toReturn;
		}
	}


	public delegate void LuceneIndexingProgressEventHandler(object sender, LuceneIndexingProgressCancelEventArgs e);

	public class LuceneIndexingProgressCancelEventArgs: CancelEventArgs
	{
		public readonly int Current;
		public readonly int Max;
		public readonly string YetIndexedFeedUrl;
		public readonly string YetIndexedFeedID;

		public LuceneIndexingProgressCancelEventArgs(bool cancel, int current, int max, string yetIndexedFeedUrl, string yetIndexedFeedID):
			base(cancel) 
		{
			this.Current = current;
			this.Max = max;
			this.YetIndexedFeedUrl = yetIndexedFeedUrl;
			this.YetIndexedFeedID = yetIndexedFeedID;
		}
	}

}
