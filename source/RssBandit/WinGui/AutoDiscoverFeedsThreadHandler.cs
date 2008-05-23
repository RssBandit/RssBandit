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
using System.Collections;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.Utils;

using RssBandit.Resources;

namespace RssBandit.WinGui
{
	/// <summary>
    /// Thread handler for AddSubscriptionWizard.
	/// </summary>
	public class AutoDiscoverFeedsThreadHandler: EntertainmentThreadHandlerBase
	{
		private string webPageUrl = String.Empty;
		private string searchTerms = String.Empty;
		private FeedLocationMethod locationMethod = FeedLocationMethod.AutoDiscoverUrl;
		private Hashtable discoveredFeeds = null;
		private IWebProxy proxy;
		private ICredentials credentials = CredentialCache.DefaultCredentials;

		public AutoDiscoverFeedsThreadHandler():base() {;}

		public IWebProxy Proxy {
			get {	return proxy;		}
			set {	proxy = value;	}
		}
		public ICredentials Credentials {
			get {	return credentials;		}
			set {	credentials = value;	}
		}
		public string WebPageUrl {
			get {	return webPageUrl;	}
			set {	webPageUrl = value;	}
		}

		public string SearchTerms {
			get {	return searchTerms;	}
			set {	searchTerms = value;	}
		}
		public FeedLocationMethod LocationMethod {
			get {	return locationMethod;	}
			set {	locationMethod = value;	}
		}

		public Hashtable DiscoveredFeeds {
			get {	return discoveredFeeds;	}
		}

		protected override void Run() {
			RssLocater locator = new RssLocater(proxy, RssBanditApplication.UserAgent, this.Credentials); 
			ArrayList arrFeedUrls = null;
			Hashtable htFeedUrls = null;
			
			// can raise System.Net.WebException: The remote server returned an error: (403) Forbidden
			try {
				if (locationMethod == FeedLocationMethod.AutoDiscoverUrl) {

					arrFeedUrls = locator.GetRssFeedsForUrl(webPageUrl, true);
					htFeedUrls = new Hashtable(arrFeedUrls.Count);
					
					foreach (string rssurl in arrFeedUrls) {
						NewsFeed discoveredFeed = new NewsFeed();
						discoveredFeed.link = rssurl;
						IFeedDetails feedInfo = null;
						try {
							// can raise System.Net.WebException: The remote server returned an error: (403) Forbidden
							feedInfo = RssParser.GetItemsForFeed(discoveredFeed, this.GetWebResponseStream(rssurl), false);
						} catch (Exception) {	// fatal errors
							feedInfo = FeedInfo.Empty;
						}

						htFeedUrls.Add (rssurl, new string[]{(!string.IsNullOrEmpty(feedInfo.Title) ? feedInfo.Title: SR.AutoDiscoveredDefaultTitle), feedInfo.Description, feedInfo.Link, rssurl} );
					}

				} else {	// Syndic8SearchType.Keyword

					htFeedUrls = locator.GetFeedsFromSyndic8(searchTerms, locationMethod);

				}
			} catch (ThreadAbortException) {
				// eat up
			} catch (System.Net.WebException wex) {
				p_operationException = wex;
				// Would it make sense, to display a credentials dialog for error (403) here?
//				MessageBox.Show(
//					SR.WebExceptionOnUrlAccess(webPageUrl, wex.Message), 
//					SR.GUIAutoDiscoverFeedFailedCaption, MessageBoxButtons.OK,MessageBoxIcon.Error);
				htFeedUrls = new Hashtable();

			} catch (Exception e) {	// fatal errors				
				p_operationException = e;
//				MessageBox.Show(
//					SR.WebExceptionOnUrlAccess(webPageUrl, e.Message), 
//					SR.GUIAutoDiscoverFeedFailedCaption, MessageBoxButtons.OK,MessageBoxIcon.Error);
				htFeedUrls = new Hashtable();
			} finally {
				discoveredFeeds = htFeedUrls;
				WorkDone.Set();
			}
		}

		private Stream GetWebResponseStream(string url) {
			return GetWebResponseStream(url, this.Credentials);			
		}
		private Stream GetWebResponseStream(string url, ICredentials credentials) {
			return AsyncWebRequest.GetSyncResponseStream(url, credentials, RssBanditApplication.UserAgent, this.Proxy);			
		}

	
	}

}
