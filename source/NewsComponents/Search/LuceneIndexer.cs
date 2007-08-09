#region CVS Version Header
/*
 * $Id: LuceneIndexer.cs,v 1.7 2007/07/21 12:26:21 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007/07/21 12:26:21 $
 * $Revision: 1.7 $
 */
#endregion

#region CVS Version Log
/*
 * $Log: LuceneIndexer.cs,v $
 * Revision 1.7  2007/07/21 12:26:21  t_rendelmann
 * added support for "portable Bandit" version
 *
 * Revision 1.6  2006/11/05 01:23:55  carnage4life
 * Reduced time consuming locks in indexing code
 *
 * Revision 1.5  2006/09/29 18:11:59  t_rendelmann
 * a) integrated lucene index refreshs;
 * b) now using a centralized defined category separator;
 * c) unified decision about storage relevant changes to feed, feed and feeditem properties;
 *
 * Revision 1.4  2006/09/12 19:06:52  t_rendelmann
 * next iteration on lucense integration - using a fixed lucene.dll with more localized analyzers
 *
 * Revision 1.3  2006/08/13 17:01:18  t_rendelmann
 * further progress on lucene search (not yet finished)
 *
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using Lucene.Net.Index;
using NewsComponents.Utils;
using RssBandit.Common.Logging;

namespace NewsComponents.Search
{
	
	/// <summary>
	/// LuceneIndexer helps to process/index NewsItems and feedFeeds.
	/// </summary>
	internal class LuceneIndexer {
		
		public event EventHandler IndexingFinished;
		public event LuceneIndexingProgressEventHandler IndexingProgress;

		private LuceneIndexModifier indexModifier;
		private Thread IndexingThread;
		private NewsHandler newsHandler;
		private IDictionary restartInfo;
		private DictionaryEntry lastIndexed;
		
		public LuceneIndexer(NewsHandler newsHandler, LuceneIndexModifier indexModifier) {
			this.newsHandler = newsHandler;
			if (this.newsHandler == null)
				throw new ArgumentNullException("newsHandler");
		
			this.indexModifier = indexModifier;
			if (this.indexModifier == null)
				throw new ArgumentNullException("indexModifier");
		}

		/// <summary>
		/// Indexes all feeds with all items.
		/// </summary>
		/// <param name="restartInfo">The restart info.</param>
		/// <param name="lastIndexed">The last indexed feed.</param>
		public void IndexAll(IDictionary restartInfo, DictionaryEntry lastIndexed) {
			
			if (IndexingThread != null && IndexingThread.IsAlive)
				return;	// yet running
			IndexingThread = new Thread(new ThreadStart(this.ThreadRun));
			IndexingThread.Name = "BanditIndexerThread";
			IndexingThread.IsBackground = true;
			
			if (restartInfo == null)
				restartInfo = new HybridDictionary();
			
			this.lastIndexed = lastIndexed;
			this.restartInfo = restartInfo;
			
			IndexingThread.Start();
		}
		
	
		private void ThreadRun() {
			
			System.DateTime start = System.DateTime.Now;
			Log.Info(String.Format("Lucene Indexing {0}started at {1}", restartInfo.Count == 0 ? String.Empty: "re-", start) ); 
			
			if (lastIndexed.Key != null && lastIndexed.Value != null) 
			{	// always remove the last possibly not fully indexed feed 
				// (e.g. app.exit while initial indexing was in progress):
				this.RemoveNewsItems(lastIndexed.Value as string);
				restartInfo.Remove(lastIndexed.Key);
				Log.Info(String.Format("Lucene Indexing removed possible incomplete indexed documents of feed {0}", lastIndexed.Key) ); 
			}

			try {
				
				int feedCount = 0, itemCount = 0, maxCount = 0;

				if (newsHandler.FeedsListOK && 
					0 < newsHandler.FeedsTable.Count) 
				{
					maxCount = newsHandler.FeedsTable.Count; 
					// we are working with a copy of the feed list to avoid 
					// exceptions, if the original feedlist was modified while indexing:
					string[] feedLinks = new string[maxCount];
					newsHandler.FeedsTable.GetKeyList().CopyTo(feedLinks, 0);
					for (int i=0; i < maxCount; i++) 
					{
						string feedlink = feedLinks[i];
						
						if (!newsHandler.FeedsTable.ContainsKey(feedlink))
							continue;

						feedCount++;
						if (!restartInfo.Contains(feedlink)) 
						{
							// the ID gets generated on request:
							string feedID = newsHandler.FeedsTable[feedlink].id;
							if (!this.RaiseIndexingProgress(feedCount, maxCount, feedlink, feedID)) 
							{
								// if there was a feed refresh meanwhile, we must ensure to get
								// already indexed feeds not indexed again:
								RemoveNewsItems(feedID);
								// now add items to index:
								itemCount += IndexNewsItems(newsHandler.GetCachedItemsForFeed(feedlink));
								Log.Info("IndexAll() progress: " + feedCount + " feeds, " + itemCount + " items processed.");
							}
						} else {
							// reset the feed ID, because it may not get saved 
							// along with the subscription list on a initial indexing break:
							newsHandler.FeedsTable[feedlink].id = restartInfo[feedlink] as string;
						}
					}
					
					System.DateTime end = System.DateTime.Now;
					TimeSpan timeRequired = TimeSpan.FromTicks(end.Ticks - start.Ticks);
					Log.Info(String.Format("Lucene Indexing all items finished at {0}. Time required: {1}h:{2} min for {3} feeds with {4} items.", end, timeRequired.TotalHours, timeRequired.TotalMinutes, feedCount, itemCount ) ); 
				}

				// are these statistics above also important for an end-user???
				this.RaiseIndexingFinished(/* statistics */);
				
				if (feedCount != 0 || itemCount != 0)
					indexModifier.Optimize();

			} catch (ThreadAbortException) {
				// ignore
			} catch (Exception e) {
				Log.Error("Failure while indexing items.", e);
			} finally {
				this.IndexingThread = null;
			}
		}

		internal int IndexNewsItems(IList<NewsItem> newsItems) {
			
			if (newsItems != null) 
			{
				for (int i=0; i < newsItems.Count; i++) {
					NewsItem item = newsItems[i] as NewsItem;
					if (item != null) {
						try {
							// we do not always have the content loaded:
							if (item.ContentType == ContentType.None)
								newsHandler.GetCachedContentForItem(item);
							indexModifier.Add(LuceneNewsItemSearch.Document(item), item.Language);
						} catch (Exception ex) {
							Log.Error("IndexNewsItems() error", ex);
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
				Term term = new Term(LuceneSearch.IndexDocument.ItemID, null);
				for (int i=0; i < newsItems.Count; i++) {
					NewsItem item = newsItems[i] as NewsItem;
					if (item != null) {
						term.text = LuceneNewsItemSearch.UID(item);
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
			if (!StringHelper.EmptyOrNull(feedID)) {
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
			if (!StringHelper.EmptyOrNull(feedID)) {
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
