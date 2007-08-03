#region CVS Version Header
/*
 * $Id: PrefetchFeedThreadHandler.cs,v 1.12 2005/09/06 20:07:11 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/09/06 20:07:11 $
 * $Revision: 1.12 $
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
		private feedsFeed discoveredFeed = null;
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

		public feedsFeed DiscoveredFeed {
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
			
			discoveredFeed = new feedsFeed();

			try {
				
				//feedInfo = feedHandler.GetFeedInfo(this.feedUrl, this.credentials);
				using (Stream mem = AsyncWebRequest.GetSyncResponseStream(this.feedUrl, this.credentials, RssBanditApplication.UserAgent, this.Proxy)) {
					feedsFeed f = new feedsFeed();
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
