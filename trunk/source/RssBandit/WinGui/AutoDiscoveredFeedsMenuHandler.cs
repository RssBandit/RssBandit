#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinToolbars;
using NewsComponents.Utils;
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
		//private delegate void AsyncDiscoverFeedInContentCallback(string htmlContent, string pageUrl, string pageTitle);
		private RssBanditApplication app = null;
		internal CommandMediator mediator = null;
		
		internal AppPopupMenuCommand itemDropdown = null;
		private AppButtonToolCommand clearListButton = null;
		private Infragistics.Win.Appearance[] discoveredAppearance;
		
		internal Hashtable discoveredFeeds = null;
		internal Queue newDiscoveredFeeds = null;
		
		private Hashtable discoveredItems = null;
		private Queue newItems = null;
		private PriorityThread worker;
		private int workerPriorityCounter = 0;
		internal static long cmdKeyPostfix = 0;
		#endregion

		#region ctor's
		private AutoDiscoveredFeedsMenuHandler() {
			worker = new PriorityThread();
			//TODO:remove
			newItems = new Queue(1);
			discoveredItems = new Hashtable(11);
			//end
			newDiscoveredFeeds = new Queue(1);
			discoveredFeeds = new Hashtable(9);
			mediator = new CommandMediator();
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

		internal void SetControls (AppPopupMenuCommand dropDown, AppButtonToolCommand clearList) 
		{
			this.itemDropdown = dropDown; 
			this.clearListButton = clearList;
			this.discoveredAppearance = new Appearance[4];
			
			// index 0 and 1: non-discovered small (0) and large (1)
			this.discoveredAppearance[0] = new Infragistics.Win.Appearance();
			this.discoveredAppearance[0].Image = Resource.LoadBitmap("Resources.no.feed.discovered.16.png");
			this.discoveredAppearance[1] = new Infragistics.Win.Appearance();
			this.discoveredAppearance[1].Image = Resource.LoadBitmap("Resources.no.feed.discovered.32.png");
			
			// index 2 and 3: discovered small (2) and large (3)
			this.discoveredAppearance[2] = new Infragistics.Win.Appearance();
			this.discoveredAppearance[2].Image = Resource.LoadBitmap("Resources.feed.discovered.16.png");
			this.discoveredAppearance[3] = new Infragistics.Win.Appearance();
			this.discoveredAppearance[3].Image = Resource.LoadBitmap("Resources.feed.discovered.32.png");
			
			// init:
			Reset();
			this.itemDropdown.ToolbarsManager.ToolClick -= new ToolClickEventHandler(OnToolbarsManager_ToolClick);
			this.itemDropdown.ToolbarsManager.ToolClick += new ToolClickEventHandler(OnToolbarsManager_ToolClick);

		}
		
		/// <summary>
		/// Resets the control. Should get called after Toolbar.LoadFromXml(), 
		/// because the items maintained dynamically.
		/// </summary>
		internal void Reset() {
			this.itemDropdown.Tools.Clear();
			this.itemDropdown.Tools.Add(this.clearListButton);
			this.itemDropdown.Tools["cmdDiscoveredFeedsListClear"].InstanceProps.IsFirstInGroup = true;
			// init to non-dicovered:
			this.itemDropdown.SharedProps.AppearancesSmall.Appearance = discoveredAppearance[0];
			this.itemDropdown.SharedProps.AppearancesLarge.Appearance = discoveredAppearance[1];
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
			AppButtonToolCommand duplicateItem = FindYetDiscoveredFeedMenuItem(info);

			if (duplicateItem != null) {	

				// update title:
				duplicateItem.SharedProps.Caption = StripAndShorten(info.Title);

				lock(discoveredFeeds) {	// we will refresh the existing info item to the new one
					discoveredFeeds.Remove(duplicateItem);
				}

			} else {
				// new entry:
				WinGuiMain guiMain = (WinGuiMain)app.MainForm; 

				if(guiMain.InvokeRequired){
					guiMain.BeginInvoke(new AddAutoDiscoveredFeedCallback(guiMain.AddAutoDiscoveredUrl), new object[]{info}); 
				}else{
					guiMain.AddAutoDiscoveredUrl(info); 
				}

				/* 
				duplicateItem = new AppButtonToolCommand(String.Concat("cmdDiscoveredFeed_", ++cmdKeyPostfix), mediator, 
					new ExecuteCommandHandler(this.OnDiscoveredItemClick), 
					StripAndShorten(info.Title), (string)info.FeedLinks[0]);
				if (this.itemDropdown.ToolbarsManager.Tools.Exists(duplicateItem.Key))
					this.itemDropdown.ToolbarsManager.Tools.Remove(duplicateItem);
				this.itemDropdown.ToolbarsManager.Tools.Add(duplicateItem);
				duplicateItem.SharedProps.StatusText = info.SiteBaseUrl;
				duplicateItem.SharedProps.ShowInCustomizer = false;
				Win32.PlaySound(Resource.ApplicationSound.FeedDiscovered);
			
				lock(discoveredFeeds) {	// add a fresh version of info
					discoveredFeeds.Add(duplicateItem, info);
				}
			
				lock(newDiscoveredFeeds) {// re-order to top of list, in RefreshItemContainer()
					newDiscoveredFeeds.Enqueue(duplicateItem);
				}
			
				*/
			}

			RefreshDiscoveredItemContainer();
			
		}
		#endregion

		#region private methods/properties
		
		internal string StripAndShorten(string s) {
			return Utilities.StripMnemonics(StringHelper.ShortenByEllipsis(s, 40));
		}
		
		/// <summary>
		/// Thread entry point
		/// </summary>
		/// <param name="state"></param>
		private void ThreadRun(object state) {
			CallbackState cbs = (CallbackState)state;
			this.AsyncDiscoverFeedsInContent(cbs.HtmlContent, cbs.PageUrl, cbs.PageTitle);
		}

		/// <summary>
		/// Thread worker procedure.
		/// </summary>
		/// <param name="htmlContent"></param>
		/// <param name="pageUrl"></param>
		/// <param name="pageTitle"></param>
		private void AsyncDiscoverFeedsInContent(string htmlContent, string pageUrl, string pageTitle) {

			if (StringHelper.EmptyOrNull( pageUrl) )
				return;

			string baseUrl = GetBaseUrlOf(pageUrl);
			AppButtonToolCommand foundItem = null;
			
			lock(discoveredFeeds) {		// simple search for baseUrl, so we may prevent lookup of the content
				foreach (AppButtonToolCommand item in discoveredFeeds.Keys) {
					string url = ((DiscoveredFeedsInfo)discoveredFeeds[item]).SiteBaseUrl;
					if (0 == String.Compare(baseUrl, url, true)) {
						foundItem = item;
					}
				}
			}

			if (foundItem != null) {	

				foundItem.SharedProps.Caption = StripAndShorten(pageTitle);
				lock(newItems) { // enqueue for re-order to top of list
					newItems.Enqueue(foundItem);
				}

			} else {

				NewsComponents.Feed.RssLocater locator = new NewsComponents.Feed.RssLocater(Proxy, RssBanditApplication.UserAgent);	//we did not really need the proxy. Content is allready loaded
				ArrayList feeds = null;
				try {
					// You can use this to simplify debugging IEControl HTML output.
					// That is slightly different than the HTML we would get from a direct request!
					//					using(System.IO.StreamWriter writer = System.IO.File.CreateText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "IEContent.htm"))) {
					//						writer.Write(htmlContent);
					//						writer.Flush();
					//					}
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
			
			RefreshDiscoveredItemContainer();

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
		
		
		internal void CmdClearFeedsList(ICommand sender) {
			this.itemDropdown.Tools.Clear();
			this.discoveredFeeds.Clear();
			this.itemDropdown.Tools.Add(clearListButton);
			this.itemDropdown.Tools["cmdDiscoveredFeedsListClear"].InstanceProps.IsFirstInGroup = true;
			RefreshDiscoveredItemContainer();
		}
		
		
		internal void OnDiscoveredItemClick(ICommand sender) {
			AppButtonToolCommand itemClicked = sender as AppButtonToolCommand;
			Debug.Assert(itemClicked != null);
			DiscoveredFeedsInfo info = discoveredFeeds[itemClicked] as DiscoveredFeedsInfo;
			Debug.Assert(info != null);
			bool cancel = this.RaiseOnDiscoveredFeedsSubscribe(info);
			if (! cancel) {
				
				//remove divider
				foreach (AppButtonToolCommand item in discoveredFeeds.Keys) {
					if (item.InstanceProps != null)
						item.InstanceProps.IsFirstInGroup = false;
				}

				//remove entry
				discoveredFeeds.Remove(itemClicked);
				itemDropdown.Tools.Remove(itemClicked);
				if (itemDropdown.Tools.Count > 1)
					itemDropdown.Tools[1].InstanceProps.IsFirstInGroup = true;
				
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

			if (app.MainForm.InvokeRequired) {	// snyc. wih main UI thread:
				app.MainForm.BeginInvoke(new MethodInvoker(this.RefreshDiscoveredItemContainer));
				return;
			}

			AppButtonToolCommand item = null;
			while (newDiscoveredFeeds.Count > 0) {
				lock(newDiscoveredFeeds) {
					item = (AppButtonToolCommand)newDiscoveredFeeds.Dequeue();
				}
				lock(itemDropdown.Tools) {
					if (itemDropdown.Tools.Contains(item))
						itemDropdown.Tools.Remove(item);
					itemDropdown.Tools.Insert(1, item);
				}

			}

			lock(itemDropdown.Tools) {
				foreach (AppButtonToolCommand m in itemDropdown.Tools) {
					if (m.InstanceProps != null)
						m.InstanceProps.IsFirstInGroup = false;
				}
			}

			lock(itemDropdown.Tools) {
				if (itemDropdown.Tools.Count > 1)
					itemDropdown.Tools[1].InstanceProps.IsFirstInGroup = true;

				// refresh appearances/images:
				int cnt = itemDropdown.Tools.Count;
				if (cnt <= 1) {
					itemDropdown.SharedProps.AppearancesSmall.Appearance = this.discoveredAppearance[0];
					itemDropdown.SharedProps.AppearancesLarge.Appearance = this.discoveredAppearance[1];
				} else {
					itemDropdown.SharedProps.AppearancesSmall.Appearance = this.discoveredAppearance[2];
					itemDropdown.SharedProps.AppearancesLarge.Appearance = this.discoveredAppearance[3];
					if (!itemDropdown.Enabled)
						itemDropdown.Enabled = true;
				}
			}
		}
		
		private void OnToolbarsManager_ToolClick(object sender, ToolClickEventArgs e) {
			this.mediator.Execute(e.Tool.Key);
		}
		
		private bool RaiseOnDiscoveredFeedsSubscribe(DiscoveredFeedsInfo feedInfo) {
			bool cancel = false;
			if (OnDiscoveredFeedsSubscribe != null) try {
				DiscoveredFeedsSubscribeCancelEventArgs ea = new DiscoveredFeedsSubscribeCancelEventArgs(feedInfo, cancel);
				OnDiscoveredFeedsSubscribe(this, ea);
				cancel = ea.Cancel;
			} catch (Exception ex) {
				Log.Error("OnDiscoveredFeedsSubscribe() event causes an exception", ex);
			}
			return cancel;
		}

		
		private AppButtonToolCommand FindYetDiscoveredFeedMenuItem(DiscoveredFeedsInfo info) {
			if (info == null)
				return null;

			AppButtonToolCommand foundItem = null;
			
			lock(discoveredItems) {
				foreach (AppButtonToolCommand item in discoveredFeeds.Keys) {
					if (null == foundItem) {
						DiscoveredFeedsInfo itemInfo = (DiscoveredFeedsInfo) discoveredFeeds[item];
						if (0 == String.Compare(itemInfo.SiteBaseUrl, info.SiteBaseUrl, true)) {
							foundItem = item;
						} else {
							ArrayList knownFeeds = itemInfo.FeedLinks;
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
						ret.Add(uri.AbsoluteUri);
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

#region CVS Version Log
/*
 * $Log: AutoDiscoveredFeedsMenuHandler.cs,v $
 * Revision 1.34  2007/03/13 16:50:49  t_rendelmann
 * fixed: new feed source dialog is now modal (key events are badly processed by parent window)
 *
 * Revision 1.33  2007/03/11 16:17:33  t_rendelmann
 * fixed: [ 1678119 ] Bandit sends out weird User Agent string (https://sourceforge.net/tracker/?func=detail&atid=615248&aid=1678119&group_id=96589)
 *
 * Revision 1.32  2007/01/30 23:06:16  carnage4life
 * Fixed threading issue with AutoDiscovered feeds handler that caused toolbar to be replaced with red X
 *
 * Revision 1.31  2007/01/21 12:54:09  t_rendelmann
 * fixed: HtmlDecoding/Mnemonics issue caused by feed titles using "&" and the like in discovered feeds dropdown and subscription wizard; as a test Url you can use e.g.  http://www.codeplex.com/entlib/Release/ProjectReleases.aspx?ReleaseId=1649
 *
 * Revision 1.30  2007/01/16 19:40:03  t_rendelmann
 * fixed: race condition caused exception
 *
 * Revision 1.29  2006/12/19 17:00:40  t_rendelmann
 * added: CVS log sections
 *
 */
#endregion
