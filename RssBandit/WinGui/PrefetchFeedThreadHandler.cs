#region CVS Version Header
/*
 * $Id: PrefetchFeedThreadHandler.cs,v 1.11 2005/04/08 15:00:18 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/08 15:00:18 $
 * $Revision: 1.11 $
 */
#endregion

using System;
using System.Net;
using System.Xml;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;

using NewsComponents;
using NewsComponents.Feed;
using RssBandit.WinGui.Utility;

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
		private IFeedDetails feedInfo = null;
		private NewsHandler feedHandler; 
		private ICredentials credentials = null;
		
		private PrefetchFeedThreadHandler() {;}
		public PrefetchFeedThreadHandler(string feedUrl, NewsHandler handler) {
			this.feedUrl = feedUrl;
			this.feedHandler = handler; 
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

		public ICredentials Credentials {
			get {	return this.credentials;	}
			set {	this.credentials = value;	}
		}

		protected override void Run() {
			
			discoveredFeed = new feedsFeed();

			try {
				
				feedInfo = feedHandler.GetFeedInfo(this.feedUrl, this.credentials);

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
