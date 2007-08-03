#region CVS Version Header
/*
 * $Id: RssBanditPreferences.cs,v 1.43 2007/05/18 22:18:00 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2007/05/18 22:18:00 $
 * $Revision: 1.43 $
 */
#endregion

#region usings
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Collections;
using NewsComponents.Utils;
using Logger = RssBandit.Common.Logging;
using RssBandit.WinGui.Utility;
using RssBandit.AppServices;
#endregion

namespace RssBandit {
	
	/// <summary>
	/// RssBanditPreferences manages 
	/// all the Bandit specific user preferences.
	/// </summary>
	[Serializable]
	public class RssBanditPreferences: ISerializable, IUserPreferences {
		
		#region bool instance variables
		/// <summary>
		/// To get rid of all the bool variables,
		/// we now use one store to track the bool states.
		/// </summary>
		[Flags,Serializable]
		private enum OptionalFlags
		{
			AllOff = 0,
			CustomProxy = 0x1,
			TakeIEProxySettings = 0x2,
			ByPassProxyOnLocal = 0x4,
			ProxyCustomCredentials = 0x8,
			UseRemoteStorage = 0x10,
			ReUseFirstBrowserTab = 0x20,
			NewsItemOpenLinkInDetailWindow = 0x40,
			MarkFeedItemsReadOnExit = 0x80,
			RefreshFeedsOnStartup = 0x100,
			AllowJavascriptInBrowser = 0x200,
			AllowJavaInBrowser = 0x400, 
			AllowActiveXInBrowser = 0x800,
			AllowBGSoundInBrowser = 0x1000, 
			AllowVideoInBrowser = 0x2000, 
			AllowImagesInBrowser = 0x4000,
			ShowNewItemsReceivedBalloon = 0x8000,
			BuildRelationCosmos = 0x10000,
			OpenNewTabsInBackground = 0x20000,
			DisableFavicons = 0x40000,
			AddPodcasts2ITunes = 0x80000,
			AddPodcasts2WMP    = 0x100000,
			AddPodcasts2Folder = 0x200000,
			SinglePodcastPlaylist = 0x400000,
			AllowAppEventSounds = 0x800000,
			ShowAllNewsItemsPerPage = 0x1000000,
			DisableAutoMarkItemsRead = 0x2000000
		}

		private OptionalFlags allOptionalFlags;
		#endregion

		#region other instance variables

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(RssBanditPreferences));
		
		//new 1.5.x
		private decimal numNewsItemsPerPage = 10;

		// new: 1.3.x
		private string userIdentityForComments = String.Empty;
		private string ngosSyncToken = String.Empty;

		// old: 1.2.x; see RssBanditApplication.CheckAndMigrateSettingsAndPreferences() 
		private string referer = String.Empty;
		private string userName = String.Empty;
		private string userMailAddress = String.Empty;

		private string[] proxyBypassList = new string[]{};
		private string proxyAddress = String.Empty;
		private int proxyPort = 0;
		private string proxyUser = String.Empty;
		private string proxyPassword = String.Empty;

		private string remoteStorageUserName = String.Empty;
		private string remoteStoragePassword = String.Empty;
		private RemoteStorageProtocolType remoteStorageProtocol = RemoteStorageProtocolType.UNC;
		private string remoteStorageLocation = String.Empty;

		private string singlePlaylistName    = String.Empty;

		private string newsItemStylesheetFile = String.Empty;
		private HideToTray hideToTrayAction = HideToTray.OnMinimize;
		// autoupdate properties
		private AutoUpdateMode autoUpdateFrequency = AutoUpdateMode.OnceIn14Days;
		//private DateTime lastAutoUpdateCheck = DateTime.MinValue;

		private Font normalFont;
		private Font unreadFont;
		private Font flagFont;
		private Font errorFont;
		private Font refererFont;
		private Font newCommentsFont;
		private Color normalFontColor = FontColorHelper.DefaultNormalColor;
		private Color unreadFontColor = FontColorHelper.DefaultUnreadColor;
		private Color flagFontColor = FontColorHelper.DefaultHighlightColor;
		private Color errorFontColor = FontColorHelper.DefaultFailureColor;
		private Color refererFontColor = FontColorHelper.DefaultReferenceColor;
		private Color newCommentsColor = FontColorHelper.DefaultNewCommentsColor;

		// general max item age: 90 days:
		private TimeSpan maxItemAge = TimeSpan.FromDays(90);	

		private BrowserBehaviorOnNewWindow browserBehaviorOnNewWindow = BrowserBehaviorOnNewWindow.OpenNewTab;
		private string browserCustomExecOnNewWindow = String.Empty;

		private DisplayFeedAlertWindow feedAlertWindow = DisplayFeedAlertWindow.AsConfiguredPerFeed;
		#endregion

		#region public properties

		
		/// <summary>
		/// Gets/Sets the number of news items to display per page in the newspaper view
		/// </summary>
		public decimal NumNewsItemsPerPage{
			[DebuggerStepThrough()]
			get { return numNewsItemsPerPage; }
			set { 
				numNewsItemsPerPage = value; 
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NumNewsItemsPerPage"));
			}		
		}


		/// <summary>
		/// Gets/Sets the Newsgator Online sync token.
		/// </summary>
		public string NgosSyncToken {
			[DebuggerStepThrough()]
			get { return ngosSyncToken; }
			set { 
				ngosSyncToken = value; 
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NgosSyncToken"));
			}
		}

		/// <summary>
		/// Gets/Sets the user identity used to post feed comments.
		/// </summary>
		public string UserIdentityForComments {
			[DebuggerStepThrough()]
			get { return userIdentityForComments; }
			set { 
				userIdentityForComments = value; 
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UserIdentityForComments"));
			}
		}

		#region kept for migration reasons only
		/// <summary>
		/// Obsolete. Do not use it anymore!
		/// Used only to migrate old values to the new structure UserIdentity.
		/// </summary>
		public string Referer 
		{
			[DebuggerStepThrough()]
			get {	return referer;		}
			set {	referer = value;	}
		}

		/// <summary>
		/// Obsolete. Do not use it anymore!
		/// Used only to migrate old values to the new structure UserIdentity.
		/// </summary>
		public string UserName {
			[DebuggerStepThrough()]
			get {	return userName;	}
			set {	userName = value;	}
		}

		/// <summary>
		/// Obsolete. Do not use it anymore!
		/// Used only to migrate old values to the new structure UserIdentity.
		/// </summary>
		public string UserMailAddress {
			[DebuggerStepThrough()]
			get {	return userMailAddress;		}
			set {	userMailAddress = value;	}
		}
		#endregion

		/// <summary>
		/// Sets/Get a value that control if feeds should be refreshed from the original
		/// source on startup of the application.
		/// </summary>
		public bool FeedRefreshOnStartup 
		{			
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.RefreshFeedsOnStartup); }
			set {	
				SetOption(OptionalFlags.RefreshFeedsOnStartup, value);		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("FeedRefreshOnStartup"));
			}
		}

		/// <summary>
		/// Gets/Set a value to control if the application have to use a proxy to
		/// request feeds.
		/// </summary>
		public bool UseProxy {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.CustomProxy);		}
			set {	
				SetOption(OptionalFlags.CustomProxy, value);		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UseProxy"));
			}
		}

		/// <summary>
		/// If <see cref="UseProxy">UseProxy</see> is set to true, this option is used
		/// to force a take over the proxy settings from and installed Internet Explorer.
		/// (Including automatic proxy configuration).
		/// </summary>
		public bool UseIEProxySettings {
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.TakeIEProxySettings);	}
			set { 
				SetOption(OptionalFlags.TakeIEProxySettings, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UseIEProxySettings"));
			}
		}

		/// <summary>
		/// Gets/Set the value if the used proxy should bypass requests
		/// for local (intranet) servers.
		/// </summary>
		public bool BypassProxyOnLocal {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.ByPassProxyOnLocal);		}
			set {	
				SetOption(OptionalFlags.ByPassProxyOnLocal, value);		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BypassProxyOnLocal"));
			}
		}


		/// <summary>
		/// Gets/Sets the value that indicates whether a news item should be automatically 
		/// marked as read when viewed in the newspaper view
		/// </summary>
		public bool MarkItemsAsReadWhenViewed { 
			[DebuggerStepThrough()]
			get {	return !GetOption(OptionalFlags.DisableAutoMarkItemsRead);		}
			set {	
				SetOption(OptionalFlags.DisableAutoMarkItemsRead, !value);		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("MarkItemsAsReadWhenViewed"));
			}
		}

		/// <summary>
		/// Gets/Set the value that indicates whether a limited number of news items
		/// should be displayed per page in the newspaper view. 
		/// </summary>
		public bool LimitNewsItemsPerPage {
			[DebuggerStepThrough()]
			get {	return !GetOption(OptionalFlags.ShowAllNewsItemsPerPage);		}
			set {	
				SetOption(OptionalFlags.ShowAllNewsItemsPerPage, !value);		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("LimitNewsItemsPerPage"));
			}
		}

		/// <summary>
		/// Sets/Get a list of servers/web addresses to bypass by the used proxy.
		/// </summary>
		public string[] ProxyBypassList 
		{
			[DebuggerStepThrough()]
			get {	return proxyBypassList;			}
			set {	
				proxyBypassList = value;		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ProxyBypassList"));
			}
		}

		/// <summary>
		/// Sets/Get the proxy address.
		/// </summary>
		public string ProxyAddress {
			[DebuggerStepThrough()]
			get {	return proxyAddress;	}
			set {	
				proxyAddress = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ProxyAddress"));
			}
		}

		/// <summary>
		/// Sets/Get the proxy port number.
		/// </summary>
		public int ProxyPort {
			[DebuggerStepThrough()]
			get {	return proxyPort;		}
			set {	
				proxyPort = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ProxyPort"));
			}
		}

		/// <summary>
		/// Sets/Get a value indicating if the proxy have to use 
		/// custom credentials (proxy needs authentication).
		/// </summary>
		public bool ProxyCustomCredentials {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.ProxyCustomCredentials);		}
			set {	
				SetOption(OptionalFlags.ProxyCustomCredentials, value);		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ProxyCustomCredentials"));
			}
		}

		/// <summary>
		/// Sets/Get the proxy custom credential user name.
		/// </summary>
		public string ProxyUser {
			[DebuggerStepThrough()]
			get {	return proxyUser;		}
			set {	
				proxyUser = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ProxyUser"));
			}
		}

		/// <summary>
		/// Sets/Get the proxy custom credential user password.
		/// </summary>
		public string ProxyPassword {
			[DebuggerStepThrough()]
			get {	return proxyPassword;		}
			set {	
				proxyPassword = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ProxyPassword"));
			}
		}
		
		/// <summary>
		/// Sets/Get the global news item formatter stylesheet 
		/// (filename exluding path name)
		/// </summary>
		public string NewsItemStylesheetFile {
			[DebuggerStepThrough()]
			get {	return newsItemStylesheetFile;		}
			set {	
				newsItemStylesheetFile = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NewsItemStylesheetFile"));
			}
		}


		/// <summary>
		/// Sets/Get the user-specified name for the WMP or iTunes playlist that will 
		/// contain all podcasts from RSS Bandit. 
		/// </summary>
		public string SinglePlaylistName {
			[DebuggerStepThrough()]
			get {	return singlePlaylistName;		}
			set {	
				singlePlaylistName = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("SinglePlaylistName"));
			}
		}
		

		/// <summary>
		/// Sets/Get a value to control if the first opened web browser Tab should
		/// be reused or not.
		/// </summary>
		public bool ReuseFirstBrowserTab {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.ReUseFirstBrowserTab);		}
			set {	
				SetOption(OptionalFlags.ReUseFirstBrowserTab, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ReuseFirstBrowserTab"));
			}
		}	

		/// <summary>
		/// Sets/Get a value to control if the new browser tabs should be opened 
		/// in the background.
		/// </summary>
		public bool OpenNewTabsInBackground {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.OpenNewTabsInBackground);		}
			set {	
				SetOption(OptionalFlags.OpenNewTabsInBackground, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("OpenNewTabsInBackground"));
			}
		}	

		/// <summary>
		/// Gets or sets a value indicating whether to allow application
		/// event sounds.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [allow app event sounds]; otherwise, <c>false</c>.
		/// </value>
		public bool AllowAppEventSounds {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.AllowAppEventSounds);		}
			set {	
				SetOption(OptionalFlags.AllowAppEventSounds, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("AllowAppEventSounds"));
			}
		}	

		/// <summary>
		/// Gets or sets a value indicating whether to run bandit as windows user logon.
		/// It directly modifies the registry value within the "Run" section and
		/// don't get persisted into preferences file.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [run bandit as windows user logon]; otherwise, <c>false</c>.
		/// </value>
		public bool RunBanditAsWindowsUserLogon {
			get { return Win32.Registry.RunAtStartup; }
			set {
				if (Win32.Registry.RunAtStartup != value)
					Win32.Registry.RunAtStartup = value;
			}
		}
		
		/// <summary>
		/// Sets/Get a value to control whether there should be a single playlist 
		/// for podcast files. If this value is false, then podcasts are added to 
		/// a playlist with the same name as the feed. 
		/// </summary>
		public bool SinglePodcastPlaylist {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.SinglePodcastPlaylist);		}
			set {	
				SetOption(OptionalFlags.SinglePodcastPlaylist, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("SinglePodcastPlaylist"));
			}
		}	

		/// <summary>
		/// Sets/Get a value to control if podcasts should be moved to a specified 
		/// podcasts folder. 
		/// </summary>
		public bool AddPodcasts2Folder {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.AddPodcasts2Folder);		}
			set {	
				SetOption(OptionalFlags.AddPodcasts2Folder, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("AddPodcasts2Folder"));
			}
		}	

		/// <summary>
		/// Sets/Get a value to control if a playlist in Windows Media Player should be 
		/// created when an WMP-compatible podcast is successfully downloaded
		/// </summary>
		public bool AddPodcasts2WMP {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.AddPodcasts2WMP);		}
			set {	
				SetOption(OptionalFlags.AddPodcasts2WMP, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("AddPodcasts2WMP"));
			}
		}	

		/// <summary>
		/// Sets/Get a value to control if a playlist in iTunes should be 
		/// created when an iTunes-compatible podcast is successfully downloaded
		/// </summary>
		public bool AddPodcasts2ITunes {
			[DebuggerStepThrough()]
			get {	return GetOption(OptionalFlags.AddPodcasts2ITunes);		}
			set {	
				SetOption(OptionalFlags.AddPodcasts2ITunes, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("AddPodcasts2ITunes"));
			}
		}	

		/// <summary>
		/// Sets/Get a value to control if the favicons are used as feed icons 
		/// in the tree view.
		/// </summary>
		public bool UseFavicons {
			[DebuggerStepThrough()]
			get {	return !GetOption(OptionalFlags.DisableFavicons);		}
			set {	
				SetOption(OptionalFlags.DisableFavicons, !value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UseFavicons"));
			}
		}	
		/// <summary>
		/// Sets/Get a value to control if unread feed items should be marked as read
		/// while leaving the feed through UI navigation (to another feed/category)
		/// </summary>
		public bool MarkItemsReadOnExit {
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.MarkFeedItemsReadOnExit);	}
			set { 
				SetOption(OptionalFlags.MarkFeedItemsReadOnExit, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("MarkItemsReadOnExit"));
			}
		}

		/// <summary>
		/// Sets/Get a value to control if an news item without a description
		/// should display the (web page) content of the link target instead (if true).
		/// </summary>
		public bool NewsItemOpenLinkInDetailWindow {
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.NewsItemOpenLinkInDetailWindow);	}
			set { 
				SetOption(OptionalFlags.NewsItemOpenLinkInDetailWindow, value);	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NewsItemOpenLinkInDetailWindow"));
			}
		}

		/// <summary>
		/// Sets/Get the user action <see cref="HideToTray">HideToTray</see> 
		/// when the application should minimize to the
		/// system tray area.
		/// </summary>
		public HideToTray HideToTrayAction {
			[DebuggerStepThrough()]
			get {	return hideToTrayAction;		}
			set {	
				hideToTrayAction = value;		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("HideToTrayAction"));
			}
		}

		/// <summary>
		/// Sets/Get the frequency defined in <see cref="AutoUpdateMode">AutoUpdateMode</see>
		/// the application should check for available updates (internet access required).
		/// </summary>
		public AutoUpdateMode AutoUpdateFrequency {
			[DebuggerStepThrough()]
			get {	return autoUpdateFrequency;		}
			set {	
				autoUpdateFrequency = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("AutoUpdateFrequency"));
			}
		}

		//TODO: remove in Phoenix release!
		/// <summary>
		/// Deprecated (cause NotSupportedException).
		/// Only kept for AppServices compatibility in version 1.5.0x,
		/// will be removed in Phoenix release!
		/// Please use the propery DateTime ICoreApplication:LastAutoUpdateCheck instead! 
		/// </summary>
		[Obsolete("Please use the propery DateTime ICoreApplication:LastAutoUpdateCheck instead!")]
		public DateTime LastAutoUpdateCheck {
			get {
				throw new NotSupportedException(
					"Obsolete: Please use the propery DateTime ICoreApplication:LastAutoUpdateCheck instead!");
			} 
			set {
				throw new NotSupportedException(
					"Obsolete: Please use the propery DateTime ICoreApplication:LastAutoUpdateCheck instead!");
			}
//			[DebuggerStepThrough()]
//			get {	return lastAutoUpdateCheck;		}
//			set {	
//				lastAutoUpdateCheck = value;	
//				EventsHelper.Fire(PropertyChanged, this, 
//					new PropertyChangedEventArgs("LastAutoUpdateCheck"));
//			}
		} 

		/// <summary>
		/// Normal font used to render items (listview) 
		/// and feeds (treeview)
		/// </summary>
		public Font NormalFont {
			[DebuggerStepThrough()]
			get {	return normalFont;		}
			set {	
				normalFont = value;		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NormalFont"));
			}
		}

		/// <summary>
		/// Normal font color used to render items (listview) 
		/// and feeds (treeview)
		/// </summary>
		public Color NormalFontColor {
			[DebuggerStepThrough()]
			get {	return normalFontColor;		}
			set {
				normalFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NormalFontColor"));
			}
		}

		/// <summary>
		/// Font used to highlight unread items (listview) 
		/// and feeds (treeview)
		/// </summary>
		public Font UnreadFont {
			[DebuggerStepThrough()]
			get {	return unreadFont;		}
			set {
				unreadFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UnreadFont"));
			}
		}

		/// <summary>
		/// Color used to highlight unread items (listview) 
		/// and feeds (treeview)
		/// </summary>
		public Color UnreadFontColor {
			[DebuggerStepThrough()]
			get {	return unreadFontColor;		}
			set {
				unreadFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UnreadFontColor"));
			}
		}

		/// <summary>
		/// Font used to render flagged items (listview) 
		/// </summary>
		public Font FlagFont {
			[DebuggerStepThrough()]
			get {	return flagFont;		}
			set {
				flagFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("FlagFont"));
			}
		}
		
		/// <summary>
		/// Color used to render flagged items (listview) 
		/// </summary>
		public Color FlagFontColor {
			[DebuggerStepThrough()]
			get {	return flagFontColor;		}
			set {
				flagFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("FlagFontColor"));
			}
		}

		/// <summary>
		/// Font used to render items that refer back to the users 
		/// default identity (listview) 
		/// </summary>
		public Font RefererFont {
			[DebuggerStepThrough()]
			get {	return refererFont;		}
			set {
				refererFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RefererFont"));
			}
		}

		/// <summary>
		/// Color used to render items that refer back to the users 
		/// default identity (listview) 
		/// </summary>
		public Color RefererFontColor {
			[DebuggerStepThrough()]
			get {	return refererFontColor;	}
			set {
				refererFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RefererFontColor"));
			}
		}

		/// <summary>
		/// Font used to render items that display an error message (listview) 
		/// </summary>
		public Font ErrorFont {
			[DebuggerStepThrough()]
			get {	return errorFont;		}
			set {
				errorFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ErrorFont"));
			}
		}

		/// <summary>
		/// Color used to render items that display an error message (listview) 
		/// </summary>
		public Color ErrorFontColor {
			[DebuggerStepThrough()]
			get {	return errorFontColor;		}
			set {
				errorFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ErrorFontColor"));
			}
		}

		/// <summary>
		/// Font used to render items that received new comments (watched) 
		/// </summary>
		public Font NewCommentsFont {
			[DebuggerStepThrough()]
			get {	return newCommentsFont;		}
			set {
				newCommentsFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NewCommentsFont"));
			}
		}

		/// <summary>
		/// Color used to render items that received new comments (watched) 
		/// </summary>
		public Color NewCommentsFontColor {
			[DebuggerStepThrough()]
			get {	return newCommentsColor;		}
			set {
				newCommentsColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NewCommentsFontColor"));
			}
		}
		
		/// <summary>
		/// Sets/Get the TimeSpan for the global maximum news item age.
		/// You have to use TimeSpan.MinValue for the unlimited item age.
		/// </summary>
		public TimeSpan MaxItemAge {
			[DebuggerStepThrough()]
			get {	return maxItemAge;	}
			set {	
				maxItemAge = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("MaxItemAge"));
			}
		}

		/// <summary>
		/// Sets/Get the value indicating if we have to use a remote storage
		/// for sync. states.
		/// </summary>
		public bool UseRemoteStorage {
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.UseRemoteStorage); }
			set {
				SetOption(OptionalFlags.UseRemoteStorage, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UseRemoteStorage"));

			}
		}

		/// <summary>
		/// Sets/Get the username that may be required to access
		/// the remote storage location.
		/// </summary>
		public string RemoteStorageUserName {
			[DebuggerStepThrough()]
			get { return remoteStorageUserName; }
			set {
				remoteStorageUserName = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RemoteStorageUserName"));
			}
		}

		/// <summary>
		/// Sets/Get the password that may be required to access the remote
		/// storage location.
		/// </summary>
		public string RemoteStoragePassword {
			[DebuggerStepThrough()]
			get { return remoteStoragePassword; }
			set {
				remoteStoragePassword = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RemoteStoragePassword"));
			}
		}

		/// <summary>
		/// Sets/Get the type of remote storage to use. <see cref="RemoteStorageProtocolType"/>
		/// </summary>
		public RemoteStorageProtocolType RemoteStorageProtocol {
			[DebuggerStepThrough()]
			get { return remoteStorageProtocol; }
			set {
				remoteStorageProtocol = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RemoteStorageProtocol"));
			}
		}

		/// <summary>
		/// Sets/Get the remote storage location. Can vary dep. on
		/// the location type (ftp, share,...)
		/// </summary>
		public string RemoteStorageLocation {
			[DebuggerStepThrough()]
			get { return remoteStorageLocation; }
			set {
				remoteStorageLocation = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RemoteStorageLocation"));
			}
		}

		/// <summary>
		/// Sets/Get the behavior how to handle requests to open new
		/// window(s) while browsing
		/// </summary>
		public BrowserBehaviorOnNewWindow BrowserOnNewWindow {
			[DebuggerStepThrough()]
			get { return browserBehaviorOnNewWindow; }
			set {
				browserBehaviorOnNewWindow = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserOnNewWindow"));
			}
		}

		/// <summary>
		/// Gets/Set the executable application to start if
		/// browser requires to open a new window.
		/// </summary>
		public string BrowserCustomExecOnNewWindow  {
			[DebuggerStepThrough()]
			get { return browserCustomExecOnNewWindow; }
			set { 
				if (value == null) 
					browserCustomExecOnNewWindow = String.Empty;
				 else	  
					browserCustomExecOnNewWindow = value; 
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserCustomExecOnNewWindow"));
			}
		}

		/// <summary>
		/// Sets/Get if Javascript should be allowed to execute
		/// </summary>
		public bool BrowserJavascriptAllowed { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.AllowJavascriptInBrowser); }
			set {
				SetOption(OptionalFlags.AllowJavascriptInBrowser, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserJavascriptAllowed"));
			}
		}

		/// <summary>
		/// Sets/Get if Java should be allowed to execute
		/// </summary>
		public bool BrowserJavaAllowed { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.AllowJavaInBrowser); }
			set {
				SetOption(OptionalFlags.AllowJavaInBrowser, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserJavaAllowed"));
			}
		}

		/// <summary>
		/// Sets/Get if ActiveX controls should be allowed to execute
		/// </summary>
		public bool BrowserActiveXAllowed { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.AllowActiveXInBrowser); }
			set {
				SetOption(OptionalFlags.AllowActiveXInBrowser, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserActiveXAllowed"));
			}
		}

		/// <summary>
		/// Sets/Get if background sounds are allowed to be played
		/// </summary>
		public bool BrowserBGSoundAllowed { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.AllowBGSoundInBrowser); }
			set {
				SetOption(OptionalFlags.AllowBGSoundInBrowser, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserBGSoundAllowed"));
			}
		}

		/// <summary>
		/// Sets/Get if video can be played
		/// </summary>
		public bool BrowserVideoAllowed { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.AllowVideoInBrowser); }
			set {
				SetOption(OptionalFlags.AllowVideoInBrowser, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserVideoAllowed"));
			}
		}

		/// <summary>
		/// Sets/Get if images should be loaded
		/// </summary>
		public bool BrowserImagesAllowed { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.AllowImagesInBrowser); }
			set {
				SetOption(OptionalFlags.AllowImagesInBrowser, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BrowserImagesAllowed"));
			}
		}

		/// <summary>
		/// Sets/Get the DisplayFeedAlertWindow enumeration value
		/// </summary>
		public DisplayFeedAlertWindow ShowAlertWindow { 
			[DebuggerStepThrough()]
			get { return feedAlertWindow; }
			set {
				feedAlertWindow = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ShowAlertWindow"));
			}
		}

		/// <summary>
		/// Sets/Get if the system tray balloon tip should be displayed
		/// if new news items are received.
		/// </summary>
		public bool ShowNewItemsReceivedBalloon { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.ShowNewItemsReceivedBalloon); }
			set {
				SetOption(OptionalFlags.ShowNewItemsReceivedBalloon, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ShowNewItemsReceivedBalloon"));
			}
		}

		/// <summary>
		/// Sets/Get if we build the relation cosmos (interlinkage of news items).
		/// </summary>
		public bool BuildRelationCosmos { 
			[DebuggerStepThrough()]
			get { return GetOption(OptionalFlags.BuildRelationCosmos); }
			set {
				SetOption(OptionalFlags.BuildRelationCosmos, value);
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BuildRelationCosmos"));
			}
		}
		#endregion

		#region private OptionalFlags handling
		private bool GetOption(OptionalFlags flag) {
			return ( (this.allOptionalFlags & flag) == flag );	
		}

		private void SetOption(OptionalFlags flag, bool value) {
			if (value)
				this.allOptionalFlags |= flag;
			else
				this.allOptionalFlags = this.allOptionalFlags & ~flag;
		}
		#endregion

		#region ctor's
		public RssBanditPreferences()	{
			InitDefaults();
		}
		#endregion

		#region Init
		private void InitDefaults() {
			normalFont = FontColorHelper.DefaultNormalFont;
			unreadFont = FontColorHelper.DefaultUnreadFont;
			flagFont = FontColorHelper.DefaultHighlightFont;
			errorFont = FontColorHelper.DefaultFailureFont;
			refererFont = FontColorHelper.DefaultReferenceFont;
			newCommentsFont = FontColorHelper.DefaultNewCommentsFont;

			// init default options to true:
			this.allOptionalFlags = DefaultOptionalFlags;
		}

		private OptionalFlags DefaultOptionalFlags {
			get {
				OptionalFlags f = OptionalFlags.AllOff;
				f |= OptionalFlags.ByPassProxyOnLocal |
					OptionalFlags.ShowNewItemsReceivedBalloon |
					OptionalFlags.AllowImagesInBrowser |
					OptionalFlags.NewsItemOpenLinkInDetailWindow |
					OptionalFlags.ReUseFirstBrowserTab |
					OptionalFlags.AllowAppEventSounds  | 
					OptionalFlags.AllowJavascriptInBrowser;
				return f;
			}
		}
		
		#endregion

		#region Serializing
		/// <summary>
		/// Initializes a new instance of the <see cref="RssBanditPreferences"/> class.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <param name="context">The context.</param>
		protected RssBanditPreferences(SerializationInfo info, StreamingContext context) {
			
			InitDefaults();
			
			SerializationInfoReader reader = new SerializationInfoReader(info);

			int version = reader.GetInt("_PrefsVersion", 0);
			// new encryption key with version 21 and higher:
			EncryptionHelper.CompatibilityMode = (version <= 20);
			//bool xmlFormat = (version >= 20);

			this.allOptionalFlags = (OptionalFlags)reader.GetValue("AllOptionalFlags", typeof(OptionalFlags), DefaultOptionalFlags);
			
			// all the following if (reader.Contains() calls are for migration from binary format
			// and because booleans are stored now in a flagged enum (OptionalFlags), that gets read once above.
			if (reader.Contains("UseProxy"))
				UseProxy = reader.GetBoolean("UseProxy", false);

			ProxyAddress = reader.GetString("ProxyAddress", String.Empty);
			ProxyPort = reader.GetInt("ProxyPort", 8080);
			ProxyUser = EncryptionHelper.Decrypt(reader.GetString("ProxyUser", String.Empty));
			ProxyPassword = EncryptionHelper.Decrypt(reader.GetString("ProxyPassword", String.Empty));
			
			if (reader.Contains("BypassProxyOnLocal"))
				BypassProxyOnLocal = reader.GetBoolean("BypassProxyOnLocal", true);
			if (reader.Contains("ProxyCustomCredentials"))
				ProxyCustomCredentials = reader.GetBoolean("ProxyCustomCredentials", false);
			
			NewsItemStylesheetFile = reader.GetString("NewsItemStylesheetFile", String.Empty);

			// see also version >= 16 below...
			// we still read them to enable migration
			// but newer formats do not store that anymore
			if (version < 18) {
				UserName = reader.GetString("UserName", String.Empty);
				UserMailAddress = reader.GetString("UserMailAddress", String.Empty);
				Referer = reader.GetString("Referer", String.Empty);
			}  

			HideToTrayAction = (HideToTray)reader.GetValue("HideToTrayAction",typeof(HideToTray), HideToTray.OnMinimize);

			AutoUpdateFrequency = (AutoUpdateMode)reader.GetValue("AutoUpdateFrequency",typeof(AutoUpdateMode), AutoUpdateMode.OnceIn14Days);
			//LastAutoUpdateCheck = reader.GetDateTime("LastAutoUpdateCheck", DateTime.Now);

			#region read Fonts

			if (reader.Contains("NormalFontString"))	// current
				NormalFont = reader.GetFont("NormalFontString", FontColorHelper.DefaultNormalFont);
			else	// older versions may contain:
				NormalFont = (Font)reader.GetValue("NormalFont",typeof(Font),FontColorHelper.DefaultNormalFont);
			
			if (reader.Contains("UnreadFontString"))	// current
				UnreadFont = reader.GetFont("UnreadFontString", FontColorHelper.DefaultUnreadFont);
			else if (reader.Contains("HighlightFontString"))	// older than v1.5.0.8
				UnreadFont = reader.GetFont("HighlightFontString", FontColorHelper.DefaultUnreadFont);
			else	// older then v1.4.x:
				UnreadFont = (Font)reader.GetValue("HighlightFont", typeof(Font), FontColorHelper.DefaultUnreadFont);

			if (reader.Contains("FlagFontString"))	// current
				FlagFont = reader.GetFont("FlagFontString", FontColorHelper.DefaultHighlightFont);
			else	// older versions may contain:
				FlagFont = (Font)reader.GetValue("FlagFont",typeof(Font), FontColorHelper.DefaultHighlightFont);
			
			if (reader.Contains("ErrorFontString"))	// current
				ErrorFont = reader.GetFont("ErrorFontString", FontColorHelper.DefaultFailureFont);
			else	// older versions may contain:
				ErrorFont = (Font)reader.GetValue("ErrorFont",typeof(Font), FontColorHelper.DefaultFailureFont);
			
			if (reader.Contains("RefererFontString"))	// current
				RefererFont = reader.GetFont("RefererFontString", FontColorHelper.DefaultReferenceFont);
			else	// older versions may contain:
				RefererFont = (Font)reader.GetValue("RefererFont",typeof(Font), FontColorHelper.DefaultReferenceFont);
			
			// new with 1.5.0.x:
			NewCommentsFont = reader.GetFont("NewCommentsFontString", FontColorHelper.DefaultNewCommentsFont);
			#endregion

			#region read colors

			NormalFontColor = (Color)reader.GetValue("NormalFontColor",typeof(Color), FontColorHelper.DefaultNormalColor);
			if (reader.Contains("UnreadFontColor"))	// current
				UnreadFontColor = (Color)reader.GetValue("UnreadFontColor",typeof(Color), FontColorHelper.DefaultUnreadColor);
			else	// older versions may contain the old key:
				UnreadFontColor = (Color)reader.GetValue("HighlightFontColor",typeof(Color), FontColorHelper.DefaultUnreadColor);
			
			FlagFontColor = (Color)reader.GetValue("FlagFontColor",typeof(Color), FontColorHelper.DefaultHighlightColor);
			ErrorFontColor = (Color)reader.GetValue("ErrorFontColor",typeof(Color), FontColorHelper.DefaultFailureColor);
			RefererFontColor = (Color)reader.GetValue("RefererFontColor",typeof(Color), FontColorHelper.DefaultReferenceColor);
			
			// new with 1.5.0.x:
			NewCommentsFontColor = (Color)reader.GetValue("NewCommentsFontColor",typeof(Color), FontColorHelper.DefaultNewCommentsColor);
			
			#endregion

			MaxItemAge = TimeSpan.FromTicks(reader.GetLong("MaxItemAge", TimeSpan.FromDays(90).Ticks));
			
			if (reader.Contains("UseRemoteStorage"))
				UseRemoteStorage = reader.GetBoolean("UseRemoteStorage", false);

			if (reader.Contains("RemoteStorageUserName"))	{
				RemoteStorageUserName = reader.GetString("RemoteStorageUserName", String.Empty);
			} else {
				RemoteStorageUserName = EncryptionHelper.Decrypt(reader.GetString("RemoteStorageUserNameCrypted", String.Empty));
			}
			if (reader.Contains("RemoteStoragePassword")) {	
				RemoteStoragePassword = reader.GetString("RemoteStoragePassword", String.Empty);
			} else {
				RemoteStoragePassword = EncryptionHelper.Decrypt(reader.GetString("RemoteStoragePasswordCrypted", String.Empty));
			}
			
			RemoteStorageProtocol = (RemoteStorageProtocolType)reader.GetValue("RemoteStorageProtocol", RemoteStorageProtocol.GetType(), RemoteStorageProtocolType.Unknown);
			RemoteStorageLocation = reader.GetString("RemoteStorageLocation", String.Empty);
				// dasBlog_1_3 is not anymore supported:
			if (UseRemoteStorage && RemoteStorageProtocol == RemoteStorageProtocolType.dasBlog_1_3) {
				UseRemoteStorage = false;	
			}

			BrowserOnNewWindow = (BrowserBehaviorOnNewWindow)reader.GetValue("BrowserOnNewWindow", typeof(BrowserBehaviorOnNewWindow), BrowserBehaviorOnNewWindow.OpenNewTab);
			BrowserCustomExecOnNewWindow = reader.GetString("BrowserCustomExecOnNewWindow", String.Empty);

			if (reader.Contains("NewsItemOpenLinkInDetailWindow")) {
				NewsItemOpenLinkInDetailWindow = reader.GetBoolean("NewsItemOpenLinkInDetailWindow", true);
			}
			if (reader.Contains("UseIEProxySettings")) {
				UseIEProxySettings = reader.GetBoolean("UseIEProxySettings", false);
			}
			if (reader.Contains("FeedRefreshOnStartup")) {
				FeedRefreshOnStartup = reader.GetBoolean("FeedRefreshOnStartup", false);
			}
			if (reader.Contains("BrowserJavascriptAllowed")) {
				BrowserJavascriptAllowed = reader.GetBoolean("BrowserJavascriptAllowed", true);
			}
			if (reader.Contains("BrowserJavaAllowed")) {
				BrowserJavaAllowed = reader.GetBoolean("BrowserJavaAllowed", false);
			}
			if (reader.Contains("BrowserActiveXAllowed")) {
				BrowserActiveXAllowed = reader.GetBoolean("BrowserActiveXAllowed", false);
			}
			if (reader.Contains("BrowserBGSoundAllowed")) {
				BrowserBGSoundAllowed = reader.GetBoolean("BrowserBGSoundAllowed", false);
			}
			if (reader.Contains("BrowserVideoAllowed")) {
				BrowserVideoAllowed = reader.GetBoolean("BrowserVideoAllowed", false);
			}
			if (reader.Contains("BrowserImagesAllowed")) {
				BrowserImagesAllowed = reader.GetBoolean("BrowserImagesAllowed", true);
			}
			
			if (reader.Contains("ShowConfiguredAlertWindows")) {
				bool showConfiguredAlertWindows = reader.GetBoolean("ShowConfiguredAlertWindows", false);
				// migrate the old bool value to the new enum:
				if (showConfiguredAlertWindows) {
					ShowAlertWindow = DisplayFeedAlertWindow.AsConfiguredPerFeed;
				} else {
					ShowAlertWindow = DisplayFeedAlertWindow.None;
				}
			} else {
				ShowAlertWindow = (DisplayFeedAlertWindow)reader.GetValue("ShowAlertWindow", typeof(DisplayFeedAlertWindow), DisplayFeedAlertWindow.AsConfiguredPerFeed);
			}

			if (reader.Contains("ShowNewItemsReceivedBalloon")) {
				ShowNewItemsReceivedBalloon = reader.GetBoolean("ShowNewItemsReceivedBalloon", true);
			}
			
			ProxyBypassList = (string[])reader.GetValue("ProxyBypassList", typeof(string[]), new string[]{});
			if (ProxyBypassList == null)
				ProxyBypassList = new string[]{};

			if (reader.Contains("MarkItemsReadOnExit")) {
				MarkItemsReadOnExit = reader.GetBoolean("MarkItemsReadOnExit", false);
			}

			UserIdentityForComments = reader.GetString("UserIdentityForComments", String.Empty);
			
			if (reader.Contains("ReuseFirstBrowserTab")) {
				ReuseFirstBrowserTab = reader.GetBoolean("ReuseFirstBrowserTab", true);
			}

			this.NgosSyncToken = reader.GetString("NgosSyncToken", String.Empty); 

			this.NumNewsItemsPerPage = reader.GetDecimal("NumNewsItemsPerPage", 10); 
		}

		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/>
		/// with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true),
		 SecurityPermissionAttribute(SecurityAction.LinkDemand)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)	{
		
			info.AddValue("_PrefsVersion", 23);	// added settings for limiting number of news items per page
			EncryptionHelper.CompatibilityMode = false;
			info.AddValue("ProxyAddress", ProxyAddress);
			info.AddValue("ProxyPort", ProxyPort);
			info.AddValue("ProxyUser", EncryptionHelper.Encrypt(ProxyUser));
			info.AddValue("ProxyPassword", EncryptionHelper.Encrypt(ProxyPassword));
			info.AddValue("ProxyBypassList", ProxyBypassList);
			info.AddValue("NewsItemStylesheetFile", NewsItemStylesheetFile);
			info.AddValue("HideToTrayAction", HideToTrayAction);
			info.AddValue("AutoUpdateFrequency", AutoUpdateFrequency);
			//info.AddValue("LastAutoUpdateCheck", LastAutoUpdateCheck);
			info.AddValue("NormalFontString", SerializationInfoReader.ConvertFont(NormalFont));
			info.AddValue("HighlightFontString", SerializationInfoReader.ConvertFont(UnreadFont));
			info.AddValue("FlagFontString", SerializationInfoReader.ConvertFont(FlagFont));
			info.AddValue("ErrorFontString", SerializationInfoReader.ConvertFont(ErrorFont));
			info.AddValue("RefererFontString", SerializationInfoReader.ConvertFont(RefererFont));
			info.AddValue("NewCommentsFontString", SerializationInfoReader.ConvertFont(NewCommentsFont));
			info.AddValue("NormalFontColor", NormalFontColor);
			info.AddValue("UnreadFontColor", UnreadFontColor);
			info.AddValue("FlagFontColor", FlagFontColor);
			info.AddValue("ErrorFontColor", ErrorFontColor);
			info.AddValue("RefererFontColor", RefererFontColor);
			info.AddValue("NewCommentsFontColor", NewCommentsFontColor);
			info.AddValue("MaxItemAge", MaxItemAge.Ticks);
			info.AddValue("RemoteStorageUserNameCrypted", EncryptionHelper.Encrypt(RemoteStorageUserName));
			info.AddValue("RemoteStoragePasswordCrypted", EncryptionHelper.Encrypt(RemoteStoragePassword));
			info.AddValue("RemoteStorageProtocol", RemoteStorageProtocol);
			info.AddValue("RemoteStorageLocation", RemoteStorageLocation);
			info.AddValue("BrowserOnNewWindow", BrowserOnNewWindow);
			info.AddValue("BrowserCustomExecOnNewWindow", BrowserCustomExecOnNewWindow);
			info.AddValue("ShowAlertWindow", ShowAlertWindow);
			info.AddValue("UserIdentityForComments", UserIdentityForComments); 
			info.AddValue("AllOptionalFlags", this.allOptionalFlags);
			info.AddValue("NgosSyncToken", this.NgosSyncToken); 
			info.AddValue("NumNewsItemsPerPage", this.NumNewsItemsPerPage);
		}
		#endregion

		#region IPropertyChanged interface
		/// <summary>
		/// Gets fired on a change of any preference property.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
		
		#region helper classes
		private class EncryptionHelper {
			private static TripleDESCryptoServiceProvider _des;
			private static bool _compatibilityMode = false;

			private EncryptionHelper(){}

			static EncryptionHelper() {
				_des = new TripleDESCryptoServiceProvider();
				_des.Key = _calcHash();
				_des.Mode = CipherMode.ECB;
			}

			/// <summary>
			/// Just to enable read of old encrypted values by 
			/// providing the value 'true'.
			/// </summary>
			internal static bool CompatibilityMode { 
				get { return _compatibilityMode; }
				set {
					if (value != _compatibilityMode)
						_des.Key = _calcHash();
					_compatibilityMode = value;
				}
			}

			public static string Decrypt(string str) {
				byte[] base64;
				byte[] bytes;
				string ret;

				if (str == null)
					ret = null;
				else {
					if (str.Length == 0)
						ret = String.Empty;
					else {
						try {
							base64 = Convert.FromBase64String(str);
							bytes = _des.CreateDecryptor().TransformFinalBlock(base64, 0, base64.GetLength(0));
							ret = Encoding.Unicode.GetString(bytes);
						}
						catch (Exception e) {
							_log.Debug("Exception in Decrypt", e);
							ret = String.Empty;
						}
					}
				}
				return ret;
			}

			public static string Encrypt(string str) {
				byte[] inBytes;
				byte[] bytes;
				string ret;

				if (str == null)
					ret = null;
				else {
					if (str.Length == 0)
						ret = String.Empty;
					else {
						try {
							inBytes = Encoding.Unicode.GetBytes(str);
							bytes = _des.CreateEncryptor().TransformFinalBlock(inBytes, 0, inBytes.GetLength(0));
							ret = Convert.ToBase64String(bytes);
						}
						catch (Exception e) {
							_log.Debug("Exception in Encrypt", e);
							ret = String.Empty;
						}
					}
				}
				return ret;
			}

			private static byte[] _calcHash() {
				string salt = null;
				if (_compatibilityMode) {
					// use the old days salt string.
					// this is not just a query: it will also create the folder :-(
					salt = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				} else {
					// so here is a possibly better way to init the salt without
					// query the file system:
					salt = "B*A!N_D:I;T,P1E0P%P$E+R";
				}

				byte[] b = Encoding.Unicode.GetBytes(salt);
				int bLen = b.GetLength(0);
				
				// just to make the key somewhat "invisible" in Anakrino, we use the random class.
				// the seed (a prime number) makes it repro
				Random r = new Random(1500450271);	
				// result array
				byte[] res = new Byte[500];
				int i = 0;
				
				for (i = 0; i < bLen && i < 500; i++)
					res[i] = (byte)(b[i] ^ r.Next(30, 127));
				
				// padding:
				while (i < 500) {
					res[i] = (byte)r.Next(30, 127);
					i++;
				}

				MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider();
				return csp.ComputeHash(res);
			}



		}
		
		/// <summary>
		/// Helps to deserialize the old RSSBandit.XXXTypes classes/types.
		/// It maps the type to the new namespace and same class.
		/// </summary>
		internal class DeserializationTypeBinder: SerializationBinder {
			
			//private static string assemblyRunning = Assembly.GetExecutingAssembly().FullName;
			// here are the enums moved from RssBandit assembly to AppServices:
			private static ArrayList movedTypes = new ArrayList(
				new string[]{"RssBandit.HideToTray", 
								"RssBandit.AutoUpdateMode", 
								"RssBandit.RemoteStorageProtocolType", 
								"RssBandit.BrowserBehaviorOnNewWindow", 
								"RssBandit.DisplayFeedAlertWindow"});

			/// <summary>
			/// When overridden in a derived class, controls the binding of a serialized object to a type.
			/// </summary>
			/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"></see> name of the serialized object.</param>
			/// <param name="typeName">Specifies the <see cref="T:System.Type"></see> name of the serialized object.</param>
			/// <returns>
			/// The type of the object the formatter creates a new instance of.
			/// </returns>
			public override System.Type BindToType(string assemblyName, string typeName) 
			{
				System.Type typeToDeserialize = null;

				// For each assemblyName/typeName that you wish to deserialize 
				// to a different type, set typeToDeserialize to the desired type
				
				if (movedTypes.Contains(typeName)) {
					// moved types (from RssBandit to AppServices assembly):
					int index = assemblyName.IndexOf("AppServices");
					if (index < 0) {
						typeToDeserialize = Type.GetType(String.Format("{0}, {1}", 
							typeName, "RssBandit.AppServices"));
					}
					else if (index > 0)
					{ 	// version incorrect types (AppServices assembly):
						typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
							typeName, "RssBandit.AppServices"));
					}
				}

				// very old: namespace name changed (now mixed case)
				string typeVer1 = "RSSBandit.";	

				if (typeName.IndexOf(typeVer1) == 0 ) {
					// old namespace found
					typeName = typeName.Replace(typeVer1, "RssBandit.");
					typeToDeserialize = Type.GetType(String.Format("{0}, {1}", 
						typeName, assemblyName));
				}

				return typeToDeserialize;
			}

		}
		#endregion
	}
}
