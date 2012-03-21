#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections.Generic;
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
		private Dictionary<string, string[]> discoveredFeeds;
		private IWebProxy proxy;
		private ICredentials credentials = CredentialCache.DefaultCredentials;

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoDiscoverFeedsThreadHandler"/> class.
		/// </summary>
		public AutoDiscoverFeedsThreadHandler()
		{;}

		/// <summary>
		/// Gets or sets the proxy.
		/// </summary>
		/// <value>The proxy.</value>
		public IWebProxy Proxy {
			get {	return proxy;		}
			set {	proxy = value;	}
		}
		/// <summary>
		/// Gets or sets the credentials.
		/// </summary>
		/// <value>The credentials.</value>
		public ICredentials Credentials {
			get {	return credentials;		}
			set {	credentials = value;	}
		}
		/// <summary>
		/// Gets or sets the web page URL.
		/// </summary>
		/// <value>The web page URL.</value>
		public string WebPageUrl {
			get {	return webPageUrl;	}
			set {	webPageUrl = value;	}
		}

		/// <summary>
		/// Gets or sets the search terms.
		/// </summary>
		/// <value>The search terms.</value>
		public string SearchTerms {
			get {	return searchTerms;	}
			set {	searchTerms = value;	}
		}
		/// <summary>
		/// Gets or sets the location method.
		/// </summary>
		/// <value>The location method.</value>
		public FeedLocationMethod LocationMethod {
			get {	return locationMethod;	}
			set {	locationMethod = value;	}
		}

		/// <summary>
		/// Gets the discovered feeds.
		/// </summary>
		/// <value>The discovered feeds.</value>
		public Dictionary<string, string[]> DiscoveredFeeds
		{
			get {	return discoveredFeeds;	}
		}

		/// <summary>
		/// Implentation required for the Thread start call
		/// </summary>
		/// <example>
		/// Here is the impl. recommendation:
		/// <code>
		/// try {
		/// // long running task
		/// } catch (System.Threading.ThreadAbortException) {
		/// // eat up: op. cancelled
		/// } catch(Exception ex) {
		/// // handle them, or publish:
		/// p_operationException = ex;
		/// } finally {
		/// this.WorkDone.Set();	// signal end of operation to dismiss the dialog
		/// }
		/// </code>
		/// </example>
		protected override void Run() {
			RssLocater locator = new RssLocater(proxy, RssBanditApplication.UserAgent, this.Credentials); 
			List<string> arrFeedUrls;
			Dictionary<string,string[]> htFeedUrls = null;
			
			// can raise System.Net.WebException: The remote server returned an error: (403) Forbidden
			try {
				if (locationMethod == FeedLocationMethod.AutoDiscoverUrl) {

					arrFeedUrls = locator.GetRssFeedsForUrl(webPageUrl, true);
					htFeedUrls = new Dictionary<string, string[]>(arrFeedUrls.Count);
					
					foreach (string rssurl in arrFeedUrls) {
						NewsFeed discoveredFeed = new NewsFeed();
						discoveredFeed.link = rssurl;
						IFeedDetails feedInfo;
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
				htFeedUrls = new Dictionary<string, string[]>();

			} catch (Exception e) {	// fatal errors				
				p_operationException = e;
//				MessageBox.Show(
//					SR.WebExceptionOnUrlAccess(webPageUrl, e.Message), 
//					SR.GUIAutoDiscoverFeedFailedCaption, MessageBoxButtons.OK,MessageBoxIcon.Error);
				htFeedUrls = new Dictionary<string, string[]>();
			} finally {
				discoveredFeeds = htFeedUrls;
				WorkDone.Set();
			}
		}

		private Stream GetWebResponseStream(string url) {
			return GetWebResponseStream(url, this.Credentials);			
		}
		private Stream GetWebResponseStream(string url, ICredentials credentials) {
			return SyncWebRequest.GetResponseStream(url, credentials, RssBanditApplication.UserAgent, this.Proxy);			
		}

	
	}

}
