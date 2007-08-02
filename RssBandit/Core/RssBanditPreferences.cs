#region CVS Version Header
/*
 * $Id: RssBanditPreferences.cs,v 1.21 2005/06/04 15:15:19 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/06/04 15:15:19 $
 * $Revision: 1.21 $
 */
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Collections;

using Logger = RssBandit.Common.Logging;
using RssBandit.WinGui.Utility;
using RssBandit.AppServices;

namespace RssBandit {
	
	/// <summary>
	/// RssBanditPreferences stores all the Bandit specific user preferences.
	/// </summary>
	[Serializable]
	public class RssBanditPreferences: ISerializable, IUserPreferences {
		
		/// <summary>
		/// Gets fired on a change of any preference property.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(RssBanditPreferences));

		// new: 1.3.x
		private string userIdentityForComments = String.Empty;

		// old: 1.2.x; see RssBanditApplication.CheckAndMigrateSettingsAndPreferences() 
		private string referer = String.Empty;
		private string userName = String.Empty;
		private string userMailAddress = String.Empty;

		private bool useProxy = false;
		private bool useIEProxySettings = false;
		private bool bypassProxyOnLocal = true;
		private string[] proxyBypassList = new string[]{};
		private bool proxyCustomCredentials = false;
		private string proxyAddress = String.Empty;
		private int proxyPort = 0;
		private string proxyUser = String.Empty;
		private string proxyPassword = String.Empty;

		private bool useRemoteStorage = false;
		private string remoteStorageUserName = String.Empty;
		private string remoteStoragePassword = String.Empty;
		private RemoteStorageProtocolType remoteStorageProtocol = RemoteStorageProtocolType.UNC;
		private string remoteStorageLocation = String.Empty;

		private string newsItemStylesheetFile = String.Empty;
		private bool reuseFirstBrowserTab = true;
		private bool newsItemOpenLinkInDetailWindow = true;
		private bool markItemsReadOnExit = false;
		private HideToTray hideToTrayAction = HideToTray.OnMinimize;
		// autoupdate properties
		private AutoUpdateMode autoUpdateFrequency = AutoUpdateMode.OnceIn14Days;
		private DateTime lastAutoUpdateCheck = DateTime.MinValue;

		private Font normalFont;
		private Font highlightFont;
		private Font flagFont;
		private Font errorFont;
		private Font refererFont;
		private Color normalFontColor = SystemColors.ControlText;
		private Color highlightFontColor = SystemColors.ControlText;
		private Color flagFontColor = Color.Green;
		private Color errorFontColor = Color.Red;
		private Color refererFontColor = Color.Blue;

		// general max item age: 30 days:
		private TimeSpan maxItemAge = TimeSpan.FromDays(30);	

		private BrowserBehaviorOnNewWindow browserBehaviorOnNewWindow = BrowserBehaviorOnNewWindow.OpenNewTab;
		private string browserCustomExecOnNewWindow = String.Empty;

		private bool feedRefreshOnStartup = false;

		bool allowJavascriptInBrowser, allowJavaInBrowser, allowActiveXInBrowser,
			allowBGSoundInBrowser, allowVideoInBrowser, allowImagesInBrowser = true;

		//private bool showConfiguredAlertWindows = true;	// old. Now a enum:DisplayFeedAlertWindow
		private bool showNewItemsReceivedBalloon = true;

		private DisplayFeedAlertWindow feedAlertWindow = DisplayFeedAlertWindow.AsConfiguredPerFeed;

		#region public properties

		/// <summary>
		/// Gets/Sets the user identity used to post feed comments.
		/// </summary>
		public string UserIdentityForComments {
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
			get {	return referer;		}
			set {	referer = value;	}
		}

		/// <summary>
		/// Obsolete. Do not use it anymore!
		/// Used only to migrate old values to the new structure UserIdentity.
		/// </summary>
		public string UserName {
			get {	return userName;	}
			set {	userName = value;	}
		}

		/// <summary>
		/// Obsolete. Do not use it anymore!
		/// Used only to migrate old values to the new structure UserIdentity.
		/// </summary>
		public string UserMailAddress {
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
			get {	return feedRefreshOnStartup; }
			set {	
				feedRefreshOnStartup = value;		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("FeedRefreshOnStartup"));
			}
		}

		/// <summary>
		/// Gets/Set a value to control if the application have to use a proxy to
		/// request feeds.
		/// </summary>
		public bool UseProxy {
			get {	return useProxy;		}
			set {	
				useProxy = value;		
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
			get { return useIEProxySettings;	}
			set { 
				useIEProxySettings = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("UseIEProxySettings"));
			}
		}

		/// <summary>
		/// Gets/Set the value if the used proxy should bypass requests
		/// for local (intranet) servers.
		/// </summary>
		public bool BypassProxyOnLocal {
			get {	return bypassProxyOnLocal;		}
			set {	
				bypassProxyOnLocal = value;		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("BypassProxyOnLocal"));
			}
		}

		/// <summary>
		/// Sets/Get a list of servers/web addresses to bypass by the used proxy.
		/// </summary>
		public string[] ProxyBypassList 
		{
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
			get {	return proxyCustomCredentials;		}
			set {	
				proxyCustomCredentials = value;		
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ProxyCustomCredentials"));
			}
		}

		/// <summary>
		/// Sets/Get the proxy custom credential user name.
		/// </summary>
		public string ProxyUser {
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
			get {	return newsItemStylesheetFile;		}
			set {	
				newsItemStylesheetFile = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NewsItemStylesheetFile"));
			}
		}

		/// <summary>
		/// Sets/Get a value to control if the first opened web browser Tab should
		/// be reused or not.
		/// </summary>
		public bool ReuseFirstBrowserTab {
			get {	return reuseFirstBrowserTab;		}
			set {	
				reuseFirstBrowserTab = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ReuseFirstBrowserTab"));
			}
		}	

		/// <summary>
		/// Sets/Get a value to control if unread items should be marked as read
		/// while leaving the feed through UI navigation (to another feed/category)
		/// </summary>
		public bool MarkItemsReadOnExit {
			get { return markItemsReadOnExit;	}
			set { 
				markItemsReadOnExit = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("MarkItemsReadOnExit"));
			}
		}

		/// <summary>
		/// Sets/Get a value to control if an news item without a description
		/// should display the (web page) content of the link target instead (if true).
		/// </summary>
		public bool NewsItemOpenLinkInDetailWindow {
			get { return newsItemOpenLinkInDetailWindow;	}
			set { 
				newsItemOpenLinkInDetailWindow = value;	
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
			get {	return autoUpdateFrequency;		}
			set {	
				autoUpdateFrequency = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("AutoUpdateFrequency"));
			}
		}

		/// <summary>
		/// Sets/Get the DateTime of the last check for updates. 
		/// <see cref="AutoUpdateFrequency">AutoUpdateFrequency</see>
		/// </summary>
		public DateTime LastAutoUpdateCheck {
			get {	return lastAutoUpdateCheck;		}
			set {	
				lastAutoUpdateCheck = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("AutoUpdateFrequency"));
			}
		} 

		/// <summary>
		/// Normal font used to render items (listview) 
		/// and feeds (treeview)
		/// </summary>
		public Font NormalFont {
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
		public Color NormalFontColor 
		{
			get {	return normalFontColor;		}
			set
			{
				normalFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("NormalFontColor"));
			}
		}

		/// <summary>
		/// Font used to highlight items (listview) 
		/// and feeds (treeview)
		/// </summary>
		public Font HighlightFont 
		{
			get {	return highlightFont;		}
			set
			{
				highlightFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("HighlightFont"));
			}
		}

		/// <summary>
		/// Color used to highlight items (listview) 
		/// and feeds (treeview)
		/// </summary>
		public Color HighlightFontColor 
		{
			get {	return highlightFontColor;		}
			set
			{
				highlightFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("HighlightFontColor"));
			}
		}

		/// <summary>
		/// Font used to render flagged items (listview) 
		/// </summary>
		public Font FlagFont 
		{
			get {	return flagFont;		}
			set
			{
				flagFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("FlagFont"));
			}
		}
		
		/// <summary>
		/// Color used to render flagged items (listview) 
		/// </summary>
		public Color FlagFontColor 
		{
			get {	return flagFontColor;		}
			set
			{
				flagFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("FlagFontColor"));
			}
		}

		/// <summary>
		/// Font used to render items that refer back to the users 
		/// default identity (listview) 
		/// </summary>
		public Font RefererFont 
		{
			get {	return refererFont;		}
			set
			{
				refererFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RefererFont"));
			}
		}

		/// <summary>
		/// Color used to render items that refer back to the users 
		/// default identity (listview) 
		/// </summary>
		public Color RefererFontColor 
		{
			get {	return refererFontColor;	}
			set
			{
				refererFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("RefererFontColor"));
			}
		}

		/// <summary>
		/// Font used to render items that display an error message (listview) 
		/// </summary>
		public Font ErrorFont 
		{
			get {	return errorFont;		}
			set
			{
				errorFont = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ErrorFont"));
			}
		}

		/// <summary>
		/// Color used to render items that display an error message (listview) 
		/// </summary>
		public Color ErrorFontColor 
		{
			get {	return errorFontColor;		}
			set
			{
				errorFontColor = value;
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("ErrorFontColor"));
			}
		}

		/// <summary>
		/// Sets/Get the TimeSpan for the global maximum news item age.
		/// You have to use TimeSpan.MinValue for the unlimited item age.
		/// </summary>
		public TimeSpan MaxItemAge {
			get {	return maxItemAge;	}
			set {	
				maxItemAge = value;	
				EventsHelper.Fire(PropertyChanged, this, 
					new PropertyChangedEventArgs("MaxItemAge"));
			}
		}

		public bool UseRemoteStorage {
			get { return useRemoteStorage; }
			set { useRemoteStorage = value; }
		}

		public string RemoteStorageUserName {
			get { return remoteStorageUserName; }
			set { remoteStorageUserName = value; }
		}

		public string RemoteStoragePassword {
			get { return remoteStoragePassword; }
			set { remoteStoragePassword = value; }
		}

		public RemoteStorageProtocolType RemoteStorageProtocol {
			get { return remoteStorageProtocol; }
			set { remoteStorageProtocol = value; }
		}

		public string RemoteStorageLocation {
			get { return remoteStorageLocation; }
			set { remoteStorageLocation = value; }
		}

		public BrowserBehaviorOnNewWindow BrowserOnNewWindow {
			get { return browserBehaviorOnNewWindow; }
			set { browserBehaviorOnNewWindow = value; }
		}

		public string BrowserCustomExecOnNewWindow  {
			get { return browserCustomExecOnNewWindow; }
			set { 
				if (value == null) 
					browserCustomExecOnNewWindow = String.Empty;
				 else	  
					browserCustomExecOnNewWindow = value; 
			}
		}

		public bool BrowserJavascriptAllowed { 
			get { return allowJavascriptInBrowser; }
			set {  allowJavascriptInBrowser = value; }
		}
		public bool BrowserJavaAllowed { 
			get { return allowJavaInBrowser; }
			set {  allowJavaInBrowser = value; }
		}
		public bool BrowserActiveXAllowed { 
			get { return allowActiveXInBrowser; }
			set {  allowActiveXInBrowser = value; }
		}
		public bool BrowserBGSoundAllowed { 
			get { return allowBGSoundInBrowser; }
			set {  allowBGSoundInBrowser = value; }
		}
		public bool BrowserVideoAllowed { 
			get { return allowVideoInBrowser; }
			set {  allowVideoInBrowser = value; }
		}
		public bool BrowserImagesAllowed { 
			get { return allowImagesInBrowser; }
			set {  allowImagesInBrowser = value; }
		}

		public DisplayFeedAlertWindow ShowAlertWindow { 
			get { return feedAlertWindow; }
			set {  feedAlertWindow = value; }
		}
		public bool ShowNewItemsReceivedBalloon { 
			get { return showNewItemsReceivedBalloon; }
			set {  showNewItemsReceivedBalloon = value; }
		}
		#endregion

		public RssBanditPreferences()	{
			normalFont = new Font(FontFamily.GenericSansSerif,9.75f ,FontStyle.Regular);
			highlightFont = new Font(normalFont,FontStyle.Bold);
			flagFont = new Font(normalFont,FontStyle.Regular);
			errorFont = new Font(normalFont,FontStyle.Regular);
			refererFont = new Font(normalFont,FontStyle.Regular);
		}

		protected RssBanditPreferences(SerializationInfo info, StreamingContext context) {
			int version = info.GetInt32("_PrefsVersion");

			UseProxy = info.GetBoolean("UseProxy");
			ProxyAddress = info.GetString("ProxyAddress");
			ProxyPort = info.GetInt32("ProxyPort");
			if (version >= 3) {	// uses encryption
				ProxyUser = EncryptionHelper.Decrypt(info.GetString("ProxyUser"));
				ProxyPassword = EncryptionHelper.Decrypt(info.GetString("ProxyPassword"));
			}
			else {
				// old: clear text
				ProxyUser = info.GetString("ProxyUser");
				ProxyPassword = info.GetString("ProxyPassword");
			}
			BypassProxyOnLocal = info.GetBoolean("BypassProxyOnLocal");
			ProxyCustomCredentials = info.GetBoolean("ProxyCustomCredentials");
			NewsItemStylesheetFile = info.GetString("NewsItemStylesheetFile");

			// see also version >= 16 below...
			// we still read them to enable migration
			UserName = info.GetString("UserName");
			UserMailAddress = info.GetString("UserMailAddress");
			Referer = info.GetString("Referer");

			if (version >= 2) {
				HideToTrayAction = (HideToTray)info.GetValue("HideToTrayAction",typeof(HideToTray));
			}
			if (version >= 4) {
				AutoUpdateFrequency = (AutoUpdateMode)info.GetValue("AutoUpdateFrequency",typeof(AutoUpdateMode));
				LastAutoUpdateCheck = info.GetDateTime("LastAutoUpdateCheck");
			}
			if (version >= 5) {
				NormalFont = (Font)info.GetValue("NormalFont",typeof(Font));
				NormalFontColor = (Color)info.GetValue("NormalFontColor",typeof(Color));
				HighlightFont = (Font)info.GetValue("HighlightFont",typeof(Font));
				HighlightFontColor = (Color)info.GetValue("HighlightFontColor",typeof(Color));
				FlagFont = (Font)info.GetValue("FlagFont",typeof(Font));
				FlagFontColor = (Color)info.GetValue("FlagFontColor",typeof(Color));
				// see also version >= 16 below...
			}
			if (version >= 6) {
				MaxItemAge = TimeSpan.FromTicks(info.GetInt64("MaxItemAge"));
			}
			if (version >= 7) { 
				UseRemoteStorage = info.GetBoolean("UseRemoteStorage");
				RemoteStorageUserName = info.GetString("RemoteStorageUserName");
				RemoteStoragePassword = info.GetString("RemoteStoragePassword");
				RemoteStorageProtocol = (RemoteStorageProtocolType)info.GetValue("RemoteStorageProtocol", RemoteStorageProtocol.GetType());
				RemoteStorageLocation = info.GetString("RemoteStorageLocation");
				// small workaround because of the enum index change in build 1.2.0.65 to support multiple dasBlog versions (moved to the end of the enum):
				if (UseRemoteStorage) {
					if (RemoteStorageProtocol == RemoteStorageProtocolType.FTP && !RemoteStorageLocation.StartsWith("ftp:"))
						RemoteStorageProtocol =  RemoteStorageProtocolType.dasBlog_1_3;	// old index mapped to dasBlog enum, before we added the dasBlog (1.5) support
				}

			}
			if (version >= 8) {
				BrowserOnNewWindow = (BrowserBehaviorOnNewWindow)info.GetValue("BrowserOnNewWindow", typeof(BrowserBehaviorOnNewWindow));
				BrowserCustomExecOnNewWindow = info.GetString("BrowserCustomExecOnNewWindow");
			}
			if (version >= 9) {
				NewsItemOpenLinkInDetailWindow = info.GetBoolean("NewsItemOpenLinkInDetailWindow");
			}
			if (version >= 10) {
				UseIEProxySettings = info.GetBoolean("UseIEProxySettings");
				FeedRefreshOnStartup = info.GetBoolean("FeedRefreshOnStartup");
			}
			if (version >= 11) {
				BrowserJavascriptAllowed = info.GetBoolean("BrowserJavascriptAllowed");
				BrowserJavaAllowed = info.GetBoolean("BrowserJavaAllowed");
				BrowserActiveXAllowed = info.GetBoolean("BrowserActiveXAllowed");
				BrowserBGSoundAllowed = info.GetBoolean("BrowserBGSoundAllowed");
				BrowserVideoAllowed = info.GetBoolean("BrowserVideoAllowed");
				BrowserImagesAllowed = info.GetBoolean("BrowserImagesAllowed");
			}
			if (version == 12) {
				bool showConfiguredAlertWindows = info.GetBoolean("ShowConfiguredAlertWindows");
				// migrate the old bool value to the new enum:
				if (showConfiguredAlertWindows) {
					ShowAlertWindow = DisplayFeedAlertWindow.AsConfiguredPerFeed;
				} else {
					ShowAlertWindow = DisplayFeedAlertWindow.None;
				}
			}
			if (version >= 12) {
				ShowNewItemsReceivedBalloon = info.GetBoolean("ShowNewItemsReceivedBalloon");
			}
			if (version >= 13) {
				ShowAlertWindow = (DisplayFeedAlertWindow)info.GetValue("ShowAlertWindow", typeof(DisplayFeedAlertWindow));
			}
			if (version >= 14){
				ProxyBypassList = (string[])info.GetValue("ProxyBypassList", typeof(string[]));
			}
			
			if (ProxyBypassList == null)
				ProxyBypassList = new string[]{};

			if(version >= 15){
				MarkItemsReadOnExit = info.GetBoolean("MarkItemsReadOnExit");
			}

			if (version >= 16) {
				UserIdentityForComments = info.GetString("UserIdentityForComments");
				ErrorFont = (Font)info.GetValue("ErrorFont",typeof(Font));
				ErrorFontColor = (Color)info.GetValue("ErrorFontColor",typeof(Color));
				RefererFont = (Font)info.GetValue("RefererFont",typeof(Font));
				RefererFontColor = (Color)info.GetValue("RefererFontColor",typeof(Color));
			} else {
				ErrorFont = new Font(this.NormalFont, FontStyle.Regular);
				RefererFont = new Font(this.NormalFont, FontStyle.Regular);
			}

			if (version >= 17) {
				ReuseFirstBrowserTab = info.GetBoolean("ReuseFirstBrowserTab");
			}
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true),
		 SecurityPermissionAttribute(SecurityAction.LinkDemand)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)	{
		
			info.AddValue("_PrefsVersion", 17);	// additional fontstyles/colors; default identity for feed comments, ReuseFirstBrowserTab
			info.AddValue("UseProxy", UseProxy);
			info.AddValue("ProxyAddress", ProxyAddress);
			info.AddValue("ProxyPort", ProxyPort);
			// v3 and higher use Encryption
			info.AddValue("ProxyUser", EncryptionHelper.Encrypt(ProxyUser));
			info.AddValue("ProxyPassword", EncryptionHelper.Encrypt(ProxyPassword));
			info.AddValue("BypassProxyOnLocal", BypassProxyOnLocal);
			info.AddValue("ProxyCustomCredentials", ProxyCustomCredentials);
			info.AddValue("NewsItemStylesheetFile", NewsItemStylesheetFile);
			info.AddValue("Referer", Referer);
			info.AddValue("UserName", UserName);
			info.AddValue("UserMailAddress", UserMailAddress);
			// v2 and higher
			info.AddValue("HideToTrayAction", HideToTrayAction);
			// v4 and higher use autoupdate
			info.AddValue("AutoUpdateFrequency", AutoUpdateFrequency);
			info.AddValue("LastAutoUpdateCheck", LastAutoUpdateCheck);
			// v5 and higher use font/colors
			info.AddValue("NormalFont", NormalFont);
			info.AddValue("NormalFontColor", NormalFontColor);
			info.AddValue("HighlightFont", HighlightFont);
			info.AddValue("HighlightFontColor", HighlightFontColor);
			info.AddValue("FlagFont", FlagFont);
			info.AddValue("FlagFontColor", FlagFontColor);
			// v6 and higher use the general MaxItemAge (we save the ticks as long)
			info.AddValue("MaxItemAge", MaxItemAge.Ticks);
			// v7 remote storage setting
			info.AddValue("UseRemoteStorage", UseRemoteStorage);
			info.AddValue("RemoteStorageUserName", RemoteStorageUserName);
			info.AddValue("RemoteStoragePassword", RemoteStoragePassword);
			info.AddValue("RemoteStorageProtocol", RemoteStorageProtocol);
			info.AddValue("RemoteStorageLocation", RemoteStorageLocation);
			// v8 custom BrowserOnNewWindow settings
			info.AddValue("BrowserOnNewWindow", BrowserOnNewWindow);
			info.AddValue("BrowserCustomExecOnNewWindow", BrowserCustomExecOnNewWindow);
			// v9 custom NewsItemOpenLinkInDetailWindow setting
			info.AddValue("NewsItemOpenLinkInDetailWindow", NewsItemOpenLinkInDetailWindow);
			// v10 UseIEProxySettings, FeedRefreshOnStartup
			info.AddValue("UseIEProxySettings", UseIEProxySettings);
			info.AddValue("FeedRefreshOnStartup", FeedRefreshOnStartup);
			// v11 with BrowserSecuritySettings
			info.AddValue("BrowserJavascriptAllowed", BrowserJavascriptAllowed);
			info.AddValue("BrowserJavaAllowed", BrowserJavaAllowed);
			info.AddValue("BrowserActiveXAllowed", BrowserActiveXAllowed);
			info.AddValue("BrowserBGSoundAllowed", BrowserBGSoundAllowed);
			info.AddValue("BrowserVideoAllowed", BrowserVideoAllowed);
			info.AddValue("BrowserImagesAllowed", BrowserImagesAllowed);
			// v12 with silent options (Alerts, Balloon)
			//info.AddValue("ShowConfiguredAlertWindows", ShowConfiguredAlertWindows); now new: ShowAlertWindow with enum
			info.AddValue("ShowNewItemsReceivedBalloon", ShowNewItemsReceivedBalloon);
			// v13 with fine grain silent options on Alerts
			info.AddValue("ShowAlertWindow", ShowAlertWindow);
			// v14 
			info.AddValue("ProxyBypassList", ProxyBypassList);
			//v15
			info.AddValue("MarkItemsReadOnExit", MarkItemsReadOnExit); 
			//v16
			info.AddValue("UserIdentityForComments", UserIdentityForComments); 
			info.AddValue("ErrorFont", ErrorFont);
			info.AddValue("ErrorFontColor", ErrorFontColor);
			info.AddValue("RefererFont", RefererFont);
			info.AddValue("RefererFontColor", RefererFontColor);
			//v17
			info.AddValue("ReuseFirstBrowserTab", ReuseFirstBrowserTab); 
		}

		private class EncryptionHelper {
			private static TripleDESCryptoServiceProvider _des;

			private EncryptionHelper(){}

			static EncryptionHelper() {
				_des = new TripleDESCryptoServiceProvider();
				_des.Key = _calcHash();
				_des.Mode = CipherMode.ECB;
//				for (int i = 0; i < _des.Key.GetLength(0); i++)
//					Trace.Write(_des.Key[i].ToString("x"));
//
//				Trace.WriteLine(" Mode: "+((int)_des.Mode).ToString());
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
				string salt = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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
			
			public override System.Type BindToType(string assemblyName, string typeName) {
				System.Type typeToDeserialize = null;

				// For each assemblyName/typeName that you wish to deserialize to
				// to a different type, set typeToDeserialize to the desired type
				

				if (movedTypes.Contains(typeName)) {
					if (assemblyName.IndexOf("AppServices") < 0) {
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

	}
}
