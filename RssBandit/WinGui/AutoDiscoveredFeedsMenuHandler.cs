#region CVS Version Header
/*
 * $Id: AutoDiscoveredFeedsMenuHandler.cs,v 1.15 2005/05/08 17:03:07 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/08 17:03:07 $
 * $Revision: 1.15 $
 */
#endregion

using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NewsComponents.Utils;
using TD.SandBar;

using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;
using RssBandit.WinGui.Interfaces;

using NewsComponents.Threading;

namespace RssBandit.WinGui
{
	/// <summary>
	/// AutoDiscoveredFeedsMenuHandler manages the menu/tool dropdown list of 
	/// autodiscovered feeds for available HTML content.
	/// </summary>
	/// <remarks>Not jet finished!</remarks>
	public class AutoDiscoveredFeedsMenuHandler
	{

		#region private variables
		private delegate void AsyncDiscoverFeedInContentCallback(string htmlContent, string pageUrl, string pageTitle);
		private RssBanditApplication app = null;
		private CommandMediator mediator = null;
		private AppToolMenuCommand itemContainer = null;
		private AppMenuCommand clearList = null;
		private Hashtable discoveredItems = null;
		private Queue newItems = null;
		private PriorityThread worker;
		private int workerPriorityCounter = 0;
		#endregion

		#region ctor's
		private AutoDiscoveredFeedsMenuHandler() {
			worker = new PriorityThread();
			newItems = new Queue(1);
			discoveredItems = new Hashtable(11);
			mediator = new CommandMediator();

			itemContainer= new AppToolMenuCommand("cmdDiscoveredFeedsDropDown", mediator, 
				new ExecuteCommandHandler(this.OnItemContainerClick),
				Resource.Manager["RES_MenuAutodiscoveredFeedsDropdownCaption"], 
				Resource.Manager["RES_MenuAutodiscoveredFeedsDropdownDesc"]);

			itemContainer.BeginGroup = true;

			clearList = new AppMenuCommand("cmdDiscoveredFeedsListClear", mediator, 
				new ExecuteCommandHandler(this.OnClearFeedsList), 
				Resource.Manager["RES_MenuClearAutodiscoveredFeedsListCaption"], 
				Resource.Manager["RES_MenuClearAutodiscoveredFeedsListDesc"]);

			itemContainer.Items.AddRange(new TD.SandBar.MenuButtonItem[]{clearList});
			itemContainer.Icon = Resource.Manager.LoadIcon("Resources.RssDiscovered0.ico", new Size(16,16));
		}

		internal AutoDiscoveredFeedsMenuHandler(RssBanditApplication app):this()
		{
			this.app = app;
		}
		#endregion

		#region public methods/properties

		/// <summary>
		/// Callback used for OnDiscoveredFeedsSubscribe event
		/// </summary>
		public delegate void DiscoveredFeedsSubscribeCallback(object sender, DiscoveredFeedsSubscribeCancelEventArgs e);
		/// <summary>
		/// Gets fired, if a user click/select a discovered entry from the dropdown control.
		/// If you do not cancel the event via DiscoveredFeedsSubscribeCancelEventArgs Cancel property,
		/// this entry will be removed from the dropdown list.
		/// </summary>
		public event DiscoveredFeedsSubscribeCallback OnDiscoveredFeedsSubscribe;

		/// <summary>
		/// Gets the AppToolMenuCommand control to be inserted into the ToolMenuItems
		/// collection of the caller.
		/// </summary>
		public AppToolMenuCommand Control {
			get { return itemContainer; }
		}

		/// <summary>
		/// Discover feeds in the provided content.
		/// </summary>
		/// <param name="htmlContent">string content to lookup</param>
		/// <param name="pageUrl">string Url of the content. Used to calculate relative link targets.</param>
		/// <param name="pageTitle">string Title of the content. Used as the menu entry Text on the managed dropdown control</param>
		public void DiscoverFeedInContent(string htmlContent, string pageUrl, string pageTitle) {
			if (workerPriorityCounter == Int32.MaxValue )	// new requests will get the highest prio.
				workerPriorityCounter = 0;	// reset, if Bandit runs for a long time
			if (StringHelper.EmptyOrNull(pageTitle)) {
				string def = pageUrl;
				try { def = new Uri(pageUrl).Host; } catch (UriFormatException) { /* ignore */ }
				pageTitle = HtmlHelper.FindTitle(htmlContent, def);
			}
			CallbackState state = new CallbackState(htmlContent, pageUrl, pageTitle);
			worker.QueueUserWorkItem(new WaitCallback(this.ThreadRun), state, ++workerPriorityCounter);
		}

		/// <summary>
		/// Will add a DiscoveredFeedsInfo entry to the managed tool dropdown.
		/// </summary>
		/// <param name="info">DiscoveredFeedsInfo instance</param>
		public void Add(DiscoveredFeedsInfo info) {
			if (info == null)
				return;

			// detect duplicates:
			TD.SandBar.MenuButtonItem foundItem = FindInfoMenuItem(info);

			if (foundItem != null) {	

				// update title:
				foundItem.Text = StringHelper.ShortenByEllipsis(info.Title, 40);

				lock(discoveredItems) {	// we will refresh the existing info item to the new one
					discoveredItems.Remove(foundItem);
				}

			} else {
				// new entry:
				foundItem = new AppMenuCommand("cmdDiscoveredFeed"+info.SiteBaseUrl, mediator, 
					new ExecuteCommandHandler(this.OnItemClick), 
					StringHelper.ShortenByEllipsis(info.Title, 40), (string)info.FeedLinks[0]);
					
			}

			lock(discoveredItems) {	// add a fresh version of info
				discoveredItems.Add(foundItem, info);
			}
			lock(newItems) {// re-order to top of list, in RefreshItemContainer()
				newItems.Enqueue(foundItem);
			}

			RefreshItemContainer();
		}
		#endregion

		#region private methods/properties
		/// <summary>
		/// Thread entry point
		/// </summary>
		/// <param name="state"></param>
		private void ThreadRun(object state) {
			CallbackState cbs = (CallbackState)state;
			this.AsyncDiscoverFeedInContent(cbs.HtmlContent, cbs.PageUrl, cbs.PageTitle);
		}

		/// <summary>
		/// Thread worker procedure.
		/// </summary>
		/// <param name="htmlContent"></param>
		/// <param name="pageUrl"></param>
		/// <param name="pageTitle"></param>
		private void AsyncDiscoverFeedInContent(string htmlContent, string pageUrl, string pageTitle) {

			if (StringHelper.EmptyOrNull( pageUrl) )
				return;

			string baseUrl = GetBaseUrlOf(pageUrl);
			MenuButtonItem foundItem = null;
			
			lock(discoveredItems) {		// simple search for baseUrl, so we may prevent lookup of the content
				foreach (MenuButtonItem item in discoveredItems.Keys) {
					string url = ((DiscoveredFeedsInfo)discoveredItems[item]).SiteBaseUrl;
					if (baseUrl == url) {
						foundItem = item;
					}
				}
			}

			if (foundItem != null) {	

				foundItem.Text = StringHelper.ShortenByEllipsis(pageTitle, 40);
				lock(newItems) { // enqueue for re-order to top of list
					newItems.Enqueue(foundItem);
				}

			} else {

				NewsComponents.Feed.RssLocater locator = new NewsComponents.Feed.RssLocater(Proxy);	//we did not really need the proxy. Content is allready loaded
				ArrayList feeds = null;
				try {
					feeds = locator.GetRssFeedsForUrlContent(pageUrl, htmlContent, false);	
				} catch (Exception){
					//catch up all, if it fails, it returns an empty list
				}
				if (feeds != null && feeds.Count > 0) {
					feeds = this.CheckAndRemoveSubscribedFeeds(feeds, baseUrl);
				}
				
				if (feeds != null && feeds.Count > 0) {
				
					DiscoveredFeedsInfo info = new DiscoveredFeedsInfo(feeds, pageTitle, baseUrl);
					this.Add(info);
					return;
				} 

			}
			
			RefreshItemContainer();

		}

		private string GetBaseUrlOf(string pageUrl) {
			Uri uri = null;
			try {
				uri = new Uri(pageUrl);
			} catch {}
			if (uri == null)
				return pageUrl;
			string leftPart = uri.GetLeftPart(UriPartial.Path);
			return leftPart.Substring(0, leftPart.LastIndexOf("/"));
		}
		
		private void OnItemContainerClick(ICommand sender) {
			itemContainer.Show();
		}

		private void OnClearFeedsList(ICommand sender) {
			itemContainer.Items.Clear();
			discoveredItems.Clear();
			itemContainer.Items.AddRange(new TD.SandBar.MenuButtonItem[]{clearList});
			RefreshItemContainer();
		}

		private void OnItemClick(ICommand sender) {
			TD.SandBar.MenuButtonItem foundItem = sender as TD.SandBar.MenuButtonItem;
			DiscoveredFeedsInfo info = discoveredItems[foundItem] as DiscoveredFeedsInfo;
			string baseUrl = info.SiteBaseUrl;
			bool cancel = this.RaiseOnDiscoveredFeedsSubscribe(info);
			if (! cancel) {
				//remove entry
				foreach (TD.SandBar.MenuButtonItem item in discoveredItems.Keys) {
					item.BeginGroup = false;
				}

				discoveredItems.Remove(foundItem);
				itemContainer.Items.Remove(foundItem);
				if (itemContainer.Items.Count > 1)
					itemContainer.Items[1].BeginGroup = true;
				RefreshItemContainer();

			}
		}

		/// <summary>
		/// Refresh the item container. It sync. automatically with the main UI thread,
		/// if required.
		/// </summary>
		private void RefreshItemContainer() {
			
			if (app == null || app.MainForm == null || app.MainForm.Disposing)
				return;

			if (app.MainForm.InvokeRequired) {	// snyc. wih main UI thread:
				app.MainForm.Invoke(new MethodInvoker(this.RefreshItemContainer));
				return;
			}

			MenuButtonItem item = null;
			while (newItems.Count > 0) {
				lock(newItems) {
					item = (MenuButtonItem)newItems.Dequeue();
				}
				lock(itemContainer.Items) {
					if (itemContainer.Items.Contains(item))
						itemContainer.Items.Remove(item);
					itemContainer.Items.Insert(1, item);
				}

			}

			lock(itemContainer.Items) {
				foreach (MenuButtonItem m in itemContainer.Items) {
					m.BeginGroup = false;
				}
			}

			if (itemContainer.Items.Count > 1)
				itemContainer.Items[1].BeginGroup = true;

			// refresh icon info:
			int cnt = itemContainer.Items.Count;
			if (cnt == 1) {
				itemContainer.Icon = Resource.Manager.LoadIcon("Resources.RssDiscovered0.ico", new Size(16,16));
				//itemContainer.Enabled = false;
			} else if (cnt > 1 && cnt < 11) {
				cnt--;
				itemContainer.Icon = Resource.Manager.LoadIcon("Resources.RssDiscovered"+cnt.ToString()+".ico", new Size(16,16));
				if (!itemContainer.Enabled)
					itemContainer.Enabled = true;
			} else if (cnt >= 11) {
				itemContainer.Icon = Resource.Manager.LoadIcon("Resources.RssDiscoveredXX.ico", new Size(16,16));
				if (!itemContainer.Enabled)
					itemContainer.Enabled = true;
			} else {
				System.Diagnostics.Debug.Assert(cnt > 0, "invalid subitems count");
			}
		}

		private bool RaiseOnDiscoveredFeedsSubscribe(DiscoveredFeedsInfo feedInfo) {
			bool cancel = false;
			if (OnDiscoveredFeedsSubscribe != null) {
				DiscoveredFeedsSubscribeCancelEventArgs ea = new DiscoveredFeedsSubscribeCancelEventArgs(feedInfo, cancel);
				OnDiscoveredFeedsSubscribe(itemContainer, ea);
				cancel = ea.Cancel;
			}
			return cancel;
		}

		private TD.SandBar.MenuButtonItem FindInfoMenuItem(DiscoveredFeedsInfo info) {
			if (info == null)
				return null;

			TD.SandBar.MenuButtonItem foundItem = null;
			
			lock(discoveredItems) {
				foreach (TD.SandBar.MenuButtonItem item in discoveredItems.Keys) {
					if (null == foundItem) {
						string url = ((DiscoveredFeedsInfo)discoveredItems[item]).SiteBaseUrl;
						if (url == info.SiteBaseUrl) {
							foundItem = item;
						} else {
							ArrayList knownFeeds = ((DiscoveredFeedsInfo)discoveredItems[item]).FeedLinks;
							foreach (string feedLink in knownFeeds) {
								if (info.FeedLinks.Contains(feedLink)) {
									foundItem = item;
									break;
								}
							}
						}
					}
				}
			}
			return foundItem;
		}

		private ArrayList CheckAndRemoveSubscribedFeeds(ArrayList feeds, string baseUrl) {
			ArrayList ret = new ArrayList(feeds.Count);
			Uri baseUri = null;
			
			try { 
				if (baseUrl != null && baseUrl.Length > 0)
					baseUri = new Uri(baseUrl);
			} catch (UriFormatException) {}

			foreach (string url in feeds) {
				Uri uri = null;
				try { 
					if (baseUri != null) {
						uri = new Uri(baseUri, url);
					} else {
						uri = new Uri(url);
					}
					if (!this.app.FeedHandler.FeedsTable.ContainsKey(uri)) {
						ret.Add(uri.ToString());
					}
				} catch (UriFormatException) { /* ignore invalid urls */ }
			}
			return ret;
		}

		private System.Net.IWebProxy Proxy {
			get { return this.app.Proxy; }
		}

		#endregion

		#region private class
		/// <summary>
		/// Async. State container.
		/// </summary>
		private class CallbackState {
			public string HtmlContent, PageUrl, PageTitle;
			public CallbackState(string htmlContent, string pageUrl, string pageTitle) {
				this.HtmlContent = htmlContent;
				this.PageUrl = pageUrl;
				this.PageTitle = pageTitle;
			}
		}
		#endregion
	} 

	#region Helper classes
	
	/// <summary>
	/// Used as a parameter on the
	/// AutoDiscoveredFeedsMenuHandler.DiscoveredFeedsSubscribeCallback delegate
	/// </summary>
	public class DiscoveredFeedsSubscribeCancelEventArgs: System.ComponentModel.CancelEventArgs {
		
		#region ctor's
		public DiscoveredFeedsSubscribeCancelEventArgs():base(false) {
			feedsInfo = new DiscoveredFeedsInfo();
		}
		public DiscoveredFeedsSubscribeCancelEventArgs(DiscoveredFeedsInfo feedsInfo, bool cancel) {
			base.Cancel = cancel;
			this.feedsInfo = feedsInfo;
		}
		#endregion

		#region public properties
		public DiscoveredFeedsInfo FeedsInfo {
			get { return this.feedsInfo; }
		}
		private DiscoveredFeedsInfo feedsInfo;
		#endregion

	}

	/// <summary>
	/// Container class to store all the informations needed to describe autodiscovered feeds for a website.
	/// </summary>
	public class DiscoveredFeedsInfo {
		public ArrayList FeedLinks;
		public string Title;
		public string SiteBaseUrl;

		public DiscoveredFeedsInfo() {
			this.FeedLinks = new ArrayList(1);
			this.Title = String.Empty;
			this.SiteBaseUrl = String.Empty;
		}
		public DiscoveredFeedsInfo(string feedLink, string title, string baseUrl):this() {
			this.FeedLinks.Add(feedLink);
			this.Title = title;
			this.SiteBaseUrl = baseUrl;
		}
		public DiscoveredFeedsInfo(ArrayList feedLinks, string title, string baseUrl) {
			this.FeedLinks = feedLinks;
			this.Title = title;
			this.SiteBaseUrl = baseUrl;
		}
	}
	#endregion
}
