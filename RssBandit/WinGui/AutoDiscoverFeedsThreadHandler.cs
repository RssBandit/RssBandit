#region CVS Version Header
/*
 * $Id: AutoDiscoverFeedsThreadHandler.cs,v 1.11 2005/04/08 15:00:18 t_rendelmann Exp $
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
using NewsComponents.Utils;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Thread handler for AutoDiscoverFeedsDialog.
	/// </summary>
	public class AutoDiscoverFeedsThreadHandler: EntertainmentThreadHandlerBase
	{
		private string webPageUrl = String.Empty;
		private string searchTerms = String.Empty;
		private Syndic8SearchType searchType = Syndic8SearchType.Url;
		private Hashtable discoveredFeeds = null;
		private IWebProxy proxy;

		public AutoDiscoverFeedsThreadHandler():base() {;}

		public IWebProxy Proxy {
			get {	return proxy;		}
			set {	proxy = value;	}
		}
		public string WebPageUrl {
			get {	return webPageUrl;	}
			set {	webPageUrl = value;	}
		}

		public string SearchTerms {
			get {	return searchTerms;	}
			set {	searchTerms = value;	}
		}
		public Syndic8SearchType SearchType {
			get {	return searchType;	}
			set {	searchType = value;	}
		}

		public Hashtable DiscoveredFeeds {
			get {	return discoveredFeeds;	}
		}

		protected override void Run() {
			RssLocater locator = new RssLocater(proxy); 
			ArrayList arrFeedUrls = null;
			Hashtable htFeedUrls = null;
			
			// can raise System.Net.WebException: The remote server returned an error: (403) Forbidden
			try {
				if (searchType == Syndic8SearchType.Url) {

					arrFeedUrls = locator.GetRssFeedsForUrl(webPageUrl);
					htFeedUrls = new Hashtable(arrFeedUrls.Count);
					
					foreach (string rssurl in arrFeedUrls) {
						feedsFeed discoveredFeed = new feedsFeed();
						IFeedDetails feedInfo = null;
						try {
							// can raise System.Net.WebException: The remote server returned an error: (403) Forbidden
							feedInfo = NewsHandler.GetItemsForFeed(discoveredFeed, new XmlTextReader(rssurl), false);
						} catch (Exception) {	// fatal errors
							feedInfo = FeedInfo.Empty;
						}

						htFeedUrls.Add (rssurl, new string[]{(!StringHelper.EmptyOrNull(feedInfo.Title) ? feedInfo.Title: Resource.Manager["RES_AutoDiscoveredDefaultTitle"]), feedInfo.Description, feedInfo.Link, rssurl} );
					}

				} else {	// Syndic8SearchType.Keyword

					htFeedUrls = locator.GetFeedsFromSyndic8(searchTerms, searchType);

				}
			} catch (ThreadAbortException) {
				// eat up
			} catch (System.Net.WebException wex) {
				p_operationException = wex;
				// Would it make sense, to display a credentials dialog for error (403) here?
				MessageBox.Show(
					Resource.Manager.FormatMessage("RES_WebExceptionOnUrlAccess", webPageUrl, wex.Message), 
					Resource.Manager["RES_GUIAutoDiscoverFeedFailedCaption"], MessageBoxButtons.OK,MessageBoxIcon.Error);
				htFeedUrls = new Hashtable();

			} catch (Exception e) {	// fatal errors				
				p_operationException = e;
				MessageBox.Show(
					Resource.Manager.FormatMessage("RES_WebExceptionOnUrlAccess", webPageUrl, e.Message), 
					Resource.Manager["RES_GUIAutoDiscoverFeedFailedCaption"], MessageBoxButtons.OK,MessageBoxIcon.Error);
				htFeedUrls = new Hashtable();
			} finally {
				discoveredFeeds = htFeedUrls;
				WorkDone.Set();
			}
		}
	
	}

}
