#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using System.Net;
using System.Threading;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Thread handler for Prefetching a Feed to discover some properties
	/// without subscribing.
	/// </summary>
	public class PrefetchFeedThreadHandler: EntertainmentThreadHandlerBase
	{
		private string feedUrl = String.Empty;
		private NewsFeed discoveredFeed = null;
		private FeedInfo feedInfo = null;
		private IWebProxy proxy; 
		private ICredentials credentials = null;
		
		private PrefetchFeedThreadHandler() {;}
		public PrefetchFeedThreadHandler(string feedUrl, IWebProxy proxy) {
			this.feedUrl = feedUrl;
			this.proxy = proxy; 
		}
	
		public string FeedUrl {
			get {	return this.feedUrl;	}
			set {	this.feedUrl = value;	}
		}

		public NewsFeed DiscoveredFeed {
			get {	return discoveredFeed;	}
		}

		public IFeedDetails DiscoveredDetails {
			get {	return feedInfo;	}
		}

		internal FeedInfo FeedInfo {
			get {	return feedInfo;	}
		}

		public ICredentials Credentials {
			get {	return this.credentials;	}
			set {	this.credentials = value;	}
		}

		public IWebProxy Proxy {
			get {	return this.proxy;	}
			set {	this.proxy = value;	}
		}

		protected override void Run() {
			
			discoveredFeed = new NewsFeed();

			try {
				
				//feedInfo = feedHandler.GetFeedInfo(this.feedUrl, this.credentials);
				using (Stream mem = AsyncWebRequest.GetSyncResponseStream(this.feedUrl, this.credentials, RssBanditApplication.UserAgent, this.Proxy)) {
					NewsFeed f = new NewsFeed();
					f.link = feedUrl;
					if (RssParser.CanProcessUrl(feedUrl)) {
						feedInfo = RssParser.GetItemsForFeed(f, mem, false); 
						if (feedInfo.ItemsList != null && feedInfo.ItemsList.Count > 0)
							f.containsNewMessages = true;
					}
				}

			} catch (ThreadAbortException) {
				// eat up
			} catch (Exception e) {	// fatal errors
				p_operationException = e;
			} finally {
				WorkDone.Set();
			}
		}
	
	}
}
