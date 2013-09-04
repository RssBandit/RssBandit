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
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Win;
using Infragistics.Win.UltraWinToolbars;
using NewsComponents;
using NewsComponents.Utils;
using RssBandit.Common;
using RssBandit.Common.Logging;
using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Utility;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Forms;

using NewsComponents.Threading;
using Appearance=Infragistics.Win.Appearance;

namespace RssBandit.WinGui
{
	/// <summary>
	/// AutoDiscoveredFeedsMenuHandler manages the menu/tool dropdown list of 
	/// autodiscovered feeds for available HTML content.
	/// </summary>
	public class AutoDiscoveredFeedsMenuHandler
	{
		/// <summary>
		/// Callback delegate used when adding an auto discovered feed to the toolbar
		/// </summary>
		public delegate void AddAutoDiscoveredFeedCallback(DiscoveredFeedsInfo info);
		

		#region private variables
		
		private readonly RssBanditApplication app;
		internal CommandMediator mediator;
		
		internal AppPopupMenuCommand itemDropdown;
		private AppButtonToolCommand clearListButton;
		private Appearance[] discoveredAppearance;

		private Dictionary<AppButtonToolCommand, DiscoveredFeedsInfo> discoveredFeeds;
		private Queue newDiscoveredFeeds;
		private readonly object SyncRoot = new Object();

		private readonly PriorityThread worker;
		private int workerPriorityCounter;
		internal static long cmdKeyPostfix;
		#endregion

		#region ctor's
		private AutoDiscoveredFeedsMenuHandler() {
			worker = new PriorityThread();
			
			newDiscoveredFeeds = new Queue(1);
			discoveredFeeds = new Dictionary<AppButtonToolCommand, DiscoveredFeedsInfo>(7);
			mediator = new CommandMediator();
		}

		internal AutoDiscoveredFeedsMenuHandler(RssBanditApplication app):this()
		{
			this.app = app;
		}
		#endregion

		#region public methods/properties

		/// <summary>
		/// Gets fired, if a user click/select a discovered entry from the dropdown control.
		/// If you do not cancel the event via DiscoveredFeedsInfoCancelEventArgs Cancel property,
		/// this entry will be removed from the dropdown list.
		/// </summary>
		public event EventHandler<DiscoveredFeedsInfoCancelEventArgs> DiscoveredFeedsSubscribe;

		/// <summary>
		/// Occurs when new feeds discovered.
		/// </summary>
		public event EventHandler<DiscoveredFeedsInfoEventArgs> NewFeedsDiscovered;

		public void SetControls (AppPopupMenuCommand dropDown, AppButtonToolCommand clearList) 
		{
			this.itemDropdown = dropDown; 
			this.clearListButton = clearList;
			this.discoveredAppearance = new Appearance[4];
			
			// index 0 and 1: non-discovered small (0) and large (1)
			this.discoveredAppearance[0] = new Appearance();
			this.discoveredAppearance[0].Image = Properties.Resources.no_feed_discovered_16;
			this.discoveredAppearance[1] = new Appearance();
			this.discoveredAppearance[1].Image = Properties.Resources.no_feed_discovered_32;
			
			// index 2 and 3: discovered small (2) and large (3)
			this.discoveredAppearance[2] = new Appearance();
			this.discoveredAppearance[2].Image = Properties.Resources.feed_discovered_16;
			this.discoveredAppearance[3] = new Appearance();
			this.discoveredAppearance[3].Image = Properties.Resources.feed_discovered_32;
			
			// init:
			Reset();
			this.itemDropdown.ToolbarsManager.ToolClick -= OnToolbarsManager_ToolClick;
			this.itemDropdown.ToolbarsManager.ToolClick += OnToolbarsManager_ToolClick;

		}
		
		
		
		/// <summary>
		/// Discover feeds in the provided content.
		/// </summary>
		/// <param name="htmlContent">string content to lookup</param>
		/// <param name="pageUrl">string Url of the content. Used to calculate relative link targets.</param>
		/// <param name="pageTitle">string Title of the content. Used as the menu entry Text on the managed dropdown control</param>
		public void DiscoverFeedInContent(string htmlContent, string pageUrl, string pageTitle)
		{
			if (String.IsNullOrEmpty(htmlContent))
				return;

			if (workerPriorityCounter == Int32.MaxValue)	// new requests will get the highest prio.
				workerPriorityCounter = 0;	// reset, if Bandit runs for a long time
			
			if (string.IsNullOrEmpty(pageTitle))
			{
				string def = pageUrl;
				try { def = new Uri(pageUrl).Host; }
				catch (Exception) { /* ignore all */ }
				pageTitle = HtmlHelper.FindTitle(htmlContent, def);
			}
			CallbackState state = new CallbackState(htmlContent, pageUrl, pageTitle);
			worker.QueueUserWorkItem(this.ThreadRun, state, ++workerPriorityCounter);
		}

		/// <summary>
		/// Will add a DiscoveredFeedsInfo entry to the managed tool dropdown.
		/// </summary>
		/// <param name="info">DiscoveredFeedsInfo instance</param>
		public void Add(DiscoveredFeedsInfo info) {
			if (info == null)
				return;
			
			// detect duplicates:
			AppButtonToolCommand duplicateItem = FindYetDiscoveredFeedMenuItem(info);

			if (duplicateItem != null) 
			{	
				// update title/desc:
				duplicateItem.SharedProps.Caption = StripAndShorten(info.Title);
				duplicateItem.SharedProps.StatusText = info.FeedLinks[0];

				lock(SyncRoot) {	
					// refresh the existing info item to the new one
					discoveredFeeds.Remove(duplicateItem);
					discoveredFeeds.Add(duplicateItem, info);
				}

			} 
			else 
			{
				// new entry:
				WinGuiMain guiMain = (WinGuiMain)app.MainForm; 

				GuiInvoker.InvokeAsync(guiMain, delegate
                {
					//guiMain.AddAutoDiscoveredUrl(info); 
					
					AppButtonToolCommand newItem = new AppButtonToolCommand(
					String.Concat("cmdDiscoveredFeed_", ++(cmdKeyPostfix)),
					mediator,
					OnDiscoveredItemClick,
					StripAndShorten(info.Title), info.FeedLinks[0]);

					if (itemDropdown.ToolbarsManager.Tools.Exists(newItem.Key))
						itemDropdown.ToolbarsManager.Tools.Remove(newItem);

					itemDropdown.ToolbarsManager.Tools.Add(newItem);
					newItem.SharedProps.StatusText = info.SiteBaseUrl;
					newItem.SharedProps.ShowInCustomizer = false;

					lock (SyncRoot)
					{
						// add a fresh version of info
						discoveredFeeds.Add(newItem, info);
					}

					lock (newDiscoveredFeeds.SyncRoot)
					{
						// re-order to top of list, in RefreshItemContainer()
						newDiscoveredFeeds.Enqueue(newItem);
					}

                	RaiseNewFeedsDiscovered(info);
				});

			}

			RefreshDiscoveredItemContainer();
			
		}
		#endregion

		#region private methods/properties

		/// <summary>
		/// Resets the control. Should get called after Toolbar.LoadFromXml(), 
		/// because the items maintained dynamically.
		/// </summary>
		private void Reset()
		{
			this.itemDropdown.Tools.Clear();
			this.itemDropdown.Tools.Add(this.clearListButton);
			this.itemDropdown.Tools["cmdDiscoveredFeedsListClear"].InstanceProps.IsFirstInGroup = true;
			// init to non-dicovered:
			this.itemDropdown.SharedProps.AppearancesSmall.Appearance = discoveredAppearance[0];
			this.itemDropdown.SharedProps.AppearancesLarge.Appearance = discoveredAppearance[1];
		}

		private static string StripAndShorten(string s)
		{
			return Utilities.StripMnemonics(StringHelper.ShortenByEllipsis(s, 40));
		}
		
		/// <summary>
		/// Thread entry point
		/// </summary>
		/// <param name="state"></param>
		private void ThreadRun(object state)
		{
			CallbackState cbs = (CallbackState)state;
			this.AsyncDiscoverFeedsInContent(cbs.HtmlContent, cbs.PageUrl, cbs.PageTitle);
		}

		/// <summary>
		/// Thread worker procedure.
		/// </summary>
		/// <param name="htmlContent"></param>
		/// <param name="pageUrl"></param>
		/// <param name="pageTitle"></param>
		private void AsyncDiscoverFeedsInContent(string htmlContent, string pageUrl, string pageTitle)
		{

			if (string.IsNullOrEmpty(pageUrl))
				return;

			string baseUrl = GetBaseUrlOf(pageUrl);
			AppButtonToolCommand foundItem = null;

			lock (SyncRoot)
			{
				// simple search for baseUrl, so we may prevent lookup of the content
				foreach (AppButtonToolCommand item in discoveredFeeds.Keys)
				{
					DiscoveredFeedsInfo info;
					if (discoveredFeeds.TryGetValue(item, out info))
					{
						string url = info.SiteBaseUrl;
						if (String.Equals(baseUrl, url, StringComparison.OrdinalIgnoreCase))
						{
							foundItem = item;
						}
					}
				}
			}

			if (foundItem != null)
			{

				foundItem.SharedProps.Caption = StripAndShorten(pageTitle);

			}
			else
			{

				NewsComponents.Feed.RssLocater locator = new NewsComponents.Feed.RssLocater(Proxy, RssBanditApplication.UserAgent);	//we did not really need the proxy. Content is allready loaded
				List<string> feeds = null;
				try
				{
					// You can use this to simplify debugging IEControl HTML output.
					// That is slightly different than the HTML we would get from a direct request!
					//					using(System.IO.StreamWriter writer = System.IO.File.CreateText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "IEContent.htm"))) {
					//						writer.Write(htmlContent);
					//						writer.Flush();
					//					}
					feeds = locator.GetRssFeedsForUrlContent(pageUrl, htmlContent, false);
				}
				catch (Exception)
				{
					//catch up all, if it fails, it returns an empty list
				}
				if (feeds != null && feeds.Count > 0)
				{
					feeds = this.CheckAndRemoveSubscribedFeeds(feeds, baseUrl);
				}

				if (feeds != null && feeds.Count > 0)
				{

					DiscoveredFeedsInfo info = new DiscoveredFeedsInfo(feeds, pageTitle, baseUrl);
					this.Add(info);
					return;
				}

			}

			RefreshDiscoveredItemContainer();

		}


		private static string GetBaseUrlOf(string pageUrl)
		{
			Uri uri = null;
			if (pageUrl != null)
				Uri.TryCreate(pageUrl, UriKind.Absolute, out uri);

			if (uri == null)
				return pageUrl;
			string leftPart = uri.GetLeftPart(UriPartial.Path);
			return leftPart.Substring(0, leftPart.LastIndexOf("/"));
		}
		
		
		internal void CmdClearFeedsList(ICommand sender) 
		{
			lock(SyncRoot) 
				discoveredFeeds.Clear();
			
			lock (itemDropdown.Tools)
			{
				itemDropdown.Tools.Clear();
				itemDropdown.Tools.Add(clearListButton);
				itemDropdown.Tools["cmdDiscoveredFeedsListClear"].InstanceProps.IsFirstInGroup = true;
			}

			RefreshDiscoveredItemContainer();
		}
		
		internal void OnDiscoveredItemClick(ICommand sender) {
			AppButtonToolCommand itemClicked = sender as AppButtonToolCommand;
			Debug.Assert(itemClicked != null, "sender is not a AppButtonToolCommand");
			DiscoveredFeedsInfo info;
			lock(SyncRoot)
				discoveredFeeds.TryGetValue(itemClicked, out info);
			Debug.Assert(info != null, "discoveredFeeds has no matching key");
			
			bool cancel = this.RaiseDiscoveredFeedsSubscribe(info);
			if (! cancel) {
				
				//remove divider
				foreach (AppButtonToolCommand item in discoveredFeeds.Keys) {
					if (item.InstanceProps != null)
						item.InstanceProps.IsFirstInGroup = false;
				}

				//remove entry
				lock (SyncRoot)
					discoveredFeeds.Remove(itemClicked);
				lock (itemDropdown.Tools)
				{
					itemDropdown.Tools.Remove(itemClicked);
					if (itemDropdown.Tools.Count > 1)
						itemDropdown.Tools[1].InstanceProps.IsFirstInGroup = true;
				}

				RefreshDiscoveredItemContainer();
			}
		}
		
		
		/// <summary>
		/// Refresh the discovered feeds menu item container. It sync. automatically with the 
		/// main UI thread, if required.
		/// </summary>
		private void RefreshDiscoveredItemContainer() {
			
			if (app == null || app.MainForm == null || app.MainForm.Disposing)
				return;

            GuiInvoker.InvokeAsync(app.MainForm,
                delegate
                {
                    while (newDiscoveredFeeds.Count > 0)
                    {
                        AppButtonToolCommand item;
                        lock (newDiscoveredFeeds.SyncRoot)
                        {
                            item = (AppButtonToolCommand)newDiscoveredFeeds.Dequeue();
                        }
                        lock (itemDropdown.Tools)
                        {
                            if (itemDropdown.Tools.Contains(item))
                                itemDropdown.Tools.Remove(item);
                            itemDropdown.Tools.Insert(1, item);
                        }
                    }

                    lock (itemDropdown.Tools)
                    {
                        foreach (AppButtonToolCommand m in itemDropdown.Tools)
                        {
                            if (m.InstanceProps != null)
                                m.InstanceProps.IsFirstInGroup = false;
                        }
                    }

                    lock (itemDropdown.Tools)
                    {
                        if (itemDropdown.Tools.Count > 1)
                            itemDropdown.Tools[1].InstanceProps.IsFirstInGroup = true;

                        // refresh appearances/images:
                        int cnt = itemDropdown.Tools.Count;
                        if (cnt <= 1)
                        {
                            itemDropdown.SharedProps.AppearancesSmall.Appearance = this.discoveredAppearance[0];
                            itemDropdown.SharedProps.AppearancesLarge.Appearance = this.discoveredAppearance[1];
                        }
                        else
                        {
                            itemDropdown.SharedProps.AppearancesSmall.Appearance = this.discoveredAppearance[2];
                            itemDropdown.SharedProps.AppearancesLarge.Appearance = this.discoveredAppearance[3];
                            if (!itemDropdown.Enabled)
                                itemDropdown.Enabled = true;
                        }
                    }
                });
		}
		
		private void OnToolbarsManager_ToolClick(object sender, ToolClickEventArgs e) {
			this.mediator.Execute(e.Tool.Key);
		}
		
		private bool RaiseDiscoveredFeedsSubscribe(DiscoveredFeedsInfo feedInfo) {
			bool cancel = false;
			if (DiscoveredFeedsSubscribe != null) try {
				DiscoveredFeedsInfoCancelEventArgs ea = new DiscoveredFeedsInfoCancelEventArgs(feedInfo, cancel);
				DiscoveredFeedsSubscribe(this, ea);
				cancel = ea.Cancel;
			} catch (Exception ex) {
				Log.Error("DiscoveredFeedsSubscribe() event causes an exception", ex);
			}
			return cancel;
		}
		private void RaiseNewFeedsDiscovered(DiscoveredFeedsInfo feedInfo)
		{
			if (NewFeedsDiscovered != null) try {
				NewFeedsDiscovered(this, new DiscoveredFeedsInfoEventArgs(feedInfo));
			} catch (Exception ex) {
				Log.Error("NewFeedsDiscovered() event causes an exception", ex);
			}
		}
		
		private AppButtonToolCommand FindYetDiscoveredFeedMenuItem(DiscoveredFeedsInfo info) {
			if (info == null)
				return null;

			AppButtonToolCommand foundItem = null;

			lock (SyncRoot)
			{
				foreach (AppButtonToolCommand item in discoveredFeeds.Keys)
				{
					DiscoveredFeedsInfo itemInfo;
					if (!discoveredFeeds.TryGetValue(item, out itemInfo))
						continue;

					if (0 == String.Compare(itemInfo.SiteBaseUrl, info.SiteBaseUrl, true))
					{
						foundItem = item;
					}
					else
					{
						List<string> knownFeeds = itemInfo.FeedLinks;
						foreach (string feedLink in knownFeeds)
						{
							if (info.FeedLinks.Contains(feedLink))
							{
								foundItem = item;
								break;
							}
						}
					}

					if (null != foundItem)
						break;
				}
			}
			return foundItem;
		}

		private List<string> CheckAndRemoveSubscribedFeeds(ICollection<string> feeds, string baseUrl)
		{
			List<string> ret = new List<string>(feeds.Count);
			Uri baseUri = null;
			
			if (!string.IsNullOrEmpty(baseUrl))
				Uri.TryCreate(baseUrl, UriKind.Absolute, out baseUri);

			foreach (string feedUrl in feeds)
			{
				Uri uri;

				if (baseUri != null)
				{
					Uri.TryCreate(baseUri, feedUrl, out uri);
				}
				else
				{
					Uri.TryCreate(feedUrl, UriKind.Absolute, out uri);
				}

				if (uri != null)
				{
					string key = uri.CanonicalizedUri();
					bool anySourceSubscribedThis = false;
					foreach (FeedSourceEntry entry in this.app.FeedSources.Sources)
					{
						if (entry.Source.IsSubscribed(key))
						{
							anySourceSubscribedThis = true;
							break;
						}
					}
					if (!anySourceSubscribedThis)
					{
						ret.Add(key);
					}
				}
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
	/// Parameter envent class
	/// </summary>
	public class DiscoveredFeedsInfoEventArgs : EventArgs
	{

		#region ctor's
		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveredFeedsInfoEventArgs"/> class.
		/// </summary>
		internal DiscoveredFeedsInfoEventArgs()
			: this(new DiscoveredFeedsInfo())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveredFeedsInfoEventArgs"/> class.
		/// </summary>
		/// <param name="feedsInfo">The feeds info.</param>
		internal DiscoveredFeedsInfoEventArgs(DiscoveredFeedsInfo feedsInfo)
		{
			this.feedsInfo = feedsInfo;
		}
		#endregion

		#region public properties
		/// <summary>
		/// Gets the feeds info.
		/// </summary>
		/// <value>The feeds info.</value>
		public DiscoveredFeedsInfo FeedsInfo
		{
			get { return this.feedsInfo; }
		}
		private DiscoveredFeedsInfo feedsInfo;
		#endregion

	}
	/// <summary>
	/// Used as event parameters
	/// </summary>
	public class DiscoveredFeedsInfoCancelEventArgs: CancelEventArgs {
		
		#region ctor's
		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveredFeedsInfoCancelEventArgs"/> class.
		/// </summary>
		internal DiscoveredFeedsInfoCancelEventArgs():base(false) {
			feedsInfo = new DiscoveredFeedsInfo();
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveredFeedsInfoCancelEventArgs"/> class.
		/// </summary>
		/// <param name="feedsInfo">The feeds info.</param>
		/// <param name="cancel">if set to <c>true</c> [cancel].</param>
		internal DiscoveredFeedsInfoCancelEventArgs(DiscoveredFeedsInfo feedsInfo, bool cancel)
		{
			Cancel = cancel;
			this.feedsInfo = feedsInfo;
		}
		#endregion

		#region public properties
		/// <summary>
		/// Gets the feeds info.
		/// </summary>
		/// <value>The feeds info.</value>
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
		/// <summary>
		/// Gets the list of feed links
		/// </summary>
		public List<string> FeedLinks;
		/// <summary>
		/// Gets the feed title
		/// </summary>
		public string Title;
		/// <summary>
		/// Gets the site base url
		/// </summary>
		public string SiteBaseUrl;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveredFeedsInfo"/> class.
		/// </summary>
		public DiscoveredFeedsInfo() {
			this.FeedLinks = new List<string>(1);
			this.Title = String.Empty;
			this.SiteBaseUrl = String.Empty;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveredFeedsInfo"/> class.
		/// </summary>
		/// <param name="feedLink">The feed link.</param>
		/// <param name="title">The title.</param>
		/// <param name="baseUrl">The base URL.</param>
		public DiscoveredFeedsInfo(string feedLink, string title, string baseUrl):this() {
			this.FeedLinks.Add(feedLink);
			this.Title = title;
			this.SiteBaseUrl = baseUrl;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveredFeedsInfo"/> class.
		/// </summary>
		/// <param name="feedLinks">The feed links.</param>
		/// <param name="title">The title.</param>
		/// <param name="baseUrl">The base URL.</param>
		public DiscoveredFeedsInfo(List<string> feedLinks, string title, string baseUrl)
		{
			this.FeedLinks = feedLinks;
			this.Title = title;
			this.SiteBaseUrl = baseUrl;
		}
	}
	#endregion
}
