#region CVS Version Header
/*
 * $Id: RssBanditApplication.cs,v 1.217 2005/06/13 14:09:43 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2005/06/13 14:09:43 $
 * $Revision: 1.217 $
 */
#endregion

//#undef USEAUTOUPDATE
#define USEAUTOUPDATE

// Uncomment the next line to enable specific UI lang. tests.
// Then modify the returned culture ISO code within I18NTestCulture struct.
// Alternativly you can also add a commandline param '-culture:"ru-RU" to
// the project properties...

//#define TEST_I18N_THISCULTURE

// activates the new features branches for the Nightcrawler release
// also may need some references set!
#undef NIGHTCRAWLER

#region framework namespaces
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.IO; 
using System.Net;
using System.Web;
using System.Text; 
using System.Text.RegularExpressions;
using System.Xml; 
using System.Xml.Serialization;
using System.Diagnostics; 
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Configuration;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
#endregion

#region external namespaces
using EnterpriseDT.Net.Ftp;
using NewsComponents.Collections;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
using Logger = RssBandit.Common.Logging;
#endregion

#region project namespaces

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Search;
using NewsComponents.Net;
using NewsComponents.Utils;
using NewsComponents.Storage;

using RssBandit.AppServices;
using RssBandit.Exceptions;
using RssBandit.SpecialFeeds;
using RssBandit.Utility;
using RssBandit.WebSearch;
using RssBandit.WinGui;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Interfaces;
#endregion

#if DEBUG && TEST_I18N_THISCULTURE			
internal struct I18NTestCulture {  public string Culture { get { return  "ru-RU"; } } };
#endif

namespace RssBandit {

	/// <summary>
	/// Summary description for WinGuiMainMediator.
	/// </summary>
	internal class RssBanditApplication: ServiceContainerBase, ICoreApplication, IInternetService
	{

		/// <summary>
		/// additional string appended to Assembly version info
		/// </summary>
		private static string versionExtension =  "(Gnomedex Edition)";	// e.g. 'beta 1' or '(CVS)'
		private static bool validationErrorOccured = false; 
		private static RssBanditPreferences defaultPrefs = new RssBanditPreferences();
		private static Settings guiSettings = null;

		private CommandMediator cmdMediator;
		private NewsHandler feedHandler;

		private WinGuiMain guiMain;
		private PostReplyForm postReplyForm;
		private RssBanditPreferences currentPrefs = null;
		private SearchEngineHandler searchEngines = null;

		private static XmlQualifiedName flaggedItemOriginalFeedQName = new XmlQualifiedName("feed-url", "http://www.25hoursaday.com/2003/RSSBandit/feeds/"); 
		private static XmlQualifiedName deletedItemContainerFeedQName = new XmlQualifiedName("container-url", "http://www.25hoursaday.com/2003/RSSBandit/feeds/"); 
		

		private static FeedColumnLayout DefaultFeedColumnLayout = new FeedColumnLayout(new string[]{"Title", "Subject", "Date"}, new int[]{ 250, 120, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.GlobalFeedLayout);
		private static FeedColumnLayout DefaultCategoryColumnLayout = new FeedColumnLayout(new string[]{"Title", "Subject", "Date", "FeedTitle"}, new int[]{ 250, 120, 100, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.GlobalCategoryLayout);
		private static FeedColumnLayout DefaultSearchFolderColumnLayout = new FeedColumnLayout(new string[]{"Title", "Subject", "Date", "FeedTitle"}, new int[]{ 250, 120, 100, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.SearchFolderLayout);
		private static FeedColumnLayout DefaultSpecialFolderColumnLayout = new FeedColumnLayout(new string[]{"Title", "Subject", "Date", "FeedTitle"}, new int[]{ 250, 120, 100, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.SpecialFeedsLayout);

		private static string defaultFeedColumnLayoutKey;
		private static string defaultCategoryColumnLayoutKey;
		private static string defaultSearchFolderColumnLayoutKey;
		private static string defaultSpecialFolderColumnLayoutKey;

		private System.Threading.Timer autoSaveTimer = null;
		private bool feedlistModified = false;
		private bool trustedCertIssuesModified = false;
		private Queue modifiedFeeds = null;

		//private INetState connectionState = INetState.Invalid;	// moved to GuiStateManager

		private static int MilliSecsMultiplier = 60 * 1000;
		private static int DefaultRefreshRate = 60 * MilliSecsMultiplier;
		// option stored within the feedlist.xml:
		private int refreshRate = DefaultRefreshRate;

		// other options:
		// TODO: include it in options dialog 
		private bool interceptUrlNavigation = true;

		// private feeds
		private LocalFeedsFeed flaggedItemsFeed;
		private LocalFeedsFeed sentItemsFeed;
		private LocalFeedsFeed deletedItemsFeed;

		private RssBanditApplication.CommandLineOptions commandLineOptions;
		private GuiStateManager stateManager = null;
		private AutoDiscoveredFeedsMenuHandler backgroundDiscoverFeedsHandler = null;

		//private feedsFeed flagItemsFeed;
		//private ArrayList flagItemsList;

		private FinderSearchNodes findersSearchRoot;
		private NewsItemFormatter NewsItemFormatter;

		// as defined in the installer for Product ID
		private const string applicationGuid	= "9DDCC9CA-DFCD-4BF3-B069-C9660BB28848";
		private const string applicationId		= "RssBandit";
		private const string applicationName	= "RSS Bandit";

		private static string[] applicationUpdateServiceUrls = 
			new string[] {"http://www.rssbandit.org/services/UpdateService.asmx"}; 
		
		private static string defaultCategory;
		
		private static string validationUrlBase;
		private static string linkCosmosUrlBase;
		private static string bugReportUrl;
		private static string workspaceNewsUrl;
		private static string webHelpUrl;
		private static string wikiNewsUrl;
		private static string forumUrl;
		private static string projectDonationUrl;
		private static string projectDownloadUrl;

		// advanced .config options:
		private static bool unconditionalCommentRss;
		private static bool automaticColorSchemes;

		private static string appVersionInfo;	
		private static string appDataFolderPath;	
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(RssBanditApplication));

		public event EventHandler PreferencesChanged;
		public event EventHandler FeedlistLoaded;



		#region constructors and startup
		static RssBanditApplication() {

			// read app.config If a key was not found, take it from the resources
			validationUrlBase = ConfigurationSettings.AppSettings["validationUrlBase"];
			if (StringHelper.EmptyOrNull(validationUrlBase))
				validationUrlBase = Resource.Manager["URL_FeedValidationBase"];

			linkCosmosUrlBase = ConfigurationSettings.AppSettings["linkCosmosUrlBase"];
			if (StringHelper.EmptyOrNull(linkCosmosUrlBase))
				linkCosmosUrlBase = Resource.Manager["URL_FeedLinkCosmosUrlBase"];

			bugReportUrl = ConfigurationSettings.AppSettings["bugReportUrl"];
			if (StringHelper.EmptyOrNull(bugReportUrl))
				bugReportUrl = Resource.Manager["URL_BugReport"];

			webHelpUrl = ConfigurationSettings.AppSettings["webHelpUrl"];
			if (StringHelper.EmptyOrNull(webHelpUrl))
				webHelpUrl = Resource.Manager["URL_WebHelp"];

			workspaceNewsUrl = ConfigurationSettings.AppSettings["projectNewsUrl"];
			if (StringHelper.EmptyOrNull(workspaceNewsUrl))
				workspaceNewsUrl = Resource.Manager["URL_ProjectNews"];

			wikiNewsUrl = ConfigurationSettings.AppSettings["wikiWebUrl"];
			if (StringHelper.EmptyOrNull(wikiNewsUrl))
				wikiNewsUrl = Resource.Manager["URL_WikiWebNews"];

			forumUrl = ConfigurationSettings.AppSettings["userForumUrl"];
			if (StringHelper.EmptyOrNull(forumUrl))
				forumUrl = Resource.Manager["URL_UserForum"];

			projectDonationUrl = ConfigurationSettings.AppSettings["projectDonationUrl"];
			if (StringHelper.EmptyOrNull(projectDonationUrl))
				projectDonationUrl = Resource.Manager["URL_ProjectDonation"];

			projectDownloadUrl = ConfigurationSettings.AppSettings["projectDownloadUrl"];
			if (StringHelper.EmptyOrNull(projectDownloadUrl))
				projectDownloadUrl = Resource.Manager["URL_ProjectDownload"];

			// read advanced settings:
			unconditionalCommentRss = false;
			string s = ConfigurationSettings.AppSettings["UnconditionalCommentRss"];
			if (!StringHelper.EmptyOrNull(s)) {
				try {
					unconditionalCommentRss = Boolean.Parse(s);
				} catch (FormatException){}
			}

			automaticColorSchemes = true;
			s = ConfigurationSettings.AppSettings["AutomaticColorSchemes"];
			if (!StringHelper.EmptyOrNull(s)) {
				try {
					automaticColorSchemes = Boolean.Parse(s);
				} catch (FormatException){}
			}

			// Gui Settings (Form position, layouts,...)
			guiSettings = new Settings(String.Empty);

		}

		public RssBanditApplication():base() {
			this.commandLineOptions = new RssBanditApplication.CommandLineOptions();
		}

		public void Init() {

			this.LoadTrustedCertificateIssues();
			AsyncWebRequest.OnCertificateIssue += new CertificateIssueHandler(this.OnRequestCertificateIssue);
			
			this.feedHandler = new NewsHandler(applicationId, new FileCacheManager(RssBanditApplication.GetFeedFileCachePath() ));
			this.feedHandler.UserAgent = RssBanditApplication.UserAgent; 
			this.feedHandler.UpdateFeedStarted += new NewsHandler.UpdateFeedStartedHandler(this.OnUpdateFeedStarted);
			this.feedHandler.BeforeDownloadFeedStarted += new NewsHandler.DownloadFeedStartedCallback(this.BeforeDownloadFeedStarted);
			this.feedHandler.UpdateFeedsStarted += new NewsHandler.UpdateFeedsStartedHandler(this.OnUpdateFeedsStarted);
			this.feedHandler.OnUpdatedFeed += new NewsHandler.UpdatedFeedCallback(this.OnUpdatedFeed);
			this.feedHandler.OnUpdateFeedException += new NewsHandler.UpdateFeedExceptionCallback(this.OnUpdateFeedException);
			this.feedHandler.OnAllAsyncRequestsCompleted += new EventHandler(this.OnAllRequestsCompleted);

			this.feedHandler.NewsItemSearchResult += new NewsHandler.NewsItemSearchResultEventHandler(this.OnNewsItemSearchResult);
			this.feedHandler.SearchFinished += new NewsHandler.SearchFinishedEventHandler(OnSearchFinished);

			NewsHandler.UnconditionalCommentRss = UnconditionalCommentRss;
#if DEBUG
			NewsHandler.TraceMode = true;
#endif

			this.searchEngines = new SearchEngineHandler();

			// Gui command handling
			this.cmdMediator = new CommandMediator();
			
			// Gui State handling (switch buttons, icons, etc.)
			this.stateManager = new GuiStateManager();
			this.stateManager.InternetConnectionStateMoved += new GuiStateManager.InternetConnectionStateMovedHandler(this.OnInternetConnectionStateChanged);
			this.stateManager.NewsHandlerBeforeStateMove += new GuiStateManager.NewsHandlerBeforeStateMoveHandler(this.OnRssParserBeforeStateChange);
			this.stateManager.NewsHandlerStateMoved += new GuiStateManager.NewsHandlerStateMovedHandler(this.OnNewsHandlerStateChanged);

			this.Preferences = new RssBanditPreferences();
			this.NewsItemFormatter = new NewsItemFormatter();
			this.NewsItemFormatter.TransformError += new FeedExceptionEventArgs.EventHandler(this.OnNewsItemTransformationError);
			this.NewsItemFormatter.StylesheetError += new ExceptionEventArgs.EventHandler(this.OnNewsItemFormatterStylesheetError);
			this.NewsItemFormatter.StylesheetValidationError += new ExceptionEventArgs.EventHandler(this.OnNewsItemFormatterStylesheetValidationError);
			
			this.LoadPreferences();
			this.ApplyPreferences();

			this.flaggedItemsFeed = new LocalFeedsFeed(
				RssBanditApplication.GetFlagItemsFileName(),
				Resource.Manager["RES_FeedNodeFlaggedFeedsCaption"], 
				Resource.Manager["RES_FeedNodeFlaggedFeedsDesc"]);

			this.sentItemsFeed = new LocalFeedsFeed(
				RssBanditApplication.GetSentItemsFileName(),
				Resource.Manager["RES_FeedNodeSentItemsCaption"], 
				Resource.Manager["RES_FeedNodeSentItemsDesc"]);

			this.deletedItemsFeed = new LocalFeedsFeed(
				RssBanditApplication.GetDeletedItemsFileName(),
				Resource.Manager["RES_FeedNodeDeletedItemsCaption"], 
				Resource.Manager["RES_FeedNodeDeletedItemsDesc"]);

			this.findersSearchRoot = this.LoadSearchFolders();

			defaultCategory = Resource.Manager["RES_FeedDefaultCategory"];
			
			backgroundDiscoverFeedsHandler = new AutoDiscoveredFeedsMenuHandler(this);
			backgroundDiscoverFeedsHandler.OnDiscoveredFeedsSubscribe += new RssBandit.WinGui.AutoDiscoveredFeedsMenuHandler.DiscoveredFeedsSubscribeCallback(this.OnBackgroundDiscoveredFeedsSubscribe);

			this.modifiedFeeds = new Queue(this.feedHandler.FeedsTable.Count + 11);

			// Create a timer that waits three minutes , then invokes every five minutes.
			autoSaveTimer = new System.Threading.Timer(new TimerCallback(this.OnAutoSave), this, 3 * MilliSecsMultiplier, 5 * MilliSecsMultiplier);
			// handle/listen to power save modes
			SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(this.OnPowerModeChanged);

			// App Update Management
#if USEAUTOUPDATE
			RssBanditUpdateManager.OnUpdateAvailable += new RssBanditUpdateManager.UpdateAvailableEventHandler(this.OnApplicationUpdateAvailable);
#endif
			//specify 'nntp' and 'news' URI handler
			NewsComponents.News.NntpWebRequest creator = new NewsComponents.News.NntpWebRequest(new Uri("http://www.example.com"));
			WebRequest.RegisterPrefix("nntp", creator);
			WebRequest.RegisterPrefix("news", creator);

			InitApplicationServices();

			CheckForPlugins();
		}

		private void InitApplicationServices() {
			IServiceContainer me = this as IServiceContainer;
			me.AddService(typeof(IInternetService), this);
			me.AddService(typeof(IUserPreferences), this.Preferences);
			me.AddService(typeof(ICoreApplication), this);
			//TODO: add all the other services we provide...
		}

		/// <summary>
		/// Startup the Main GUI Interface.
		/// </summary>
		public void StartMainGui(FormWindowState initialStartupState) {
			ApplicationExceptionHandler eh = new ApplicationExceptionHandler();
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(eh.OnAppDomainException);
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(this.OnAppDomainUnload);
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(this.OnAppDomainUnload);
	
			Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
			Application.ThreadException += new ThreadExceptionEventHandler(this.OnThreadException);
			Splash.Status = Resource.Manager["RES_AppLoadStateGuiLoading"];
			base.MainForm = guiMain = new WinGuiMain(this, initialStartupState); // interconnect

			enter_mainevent_loop:			
				try {
					Application.Run(this);
				} catch (Exception thirdPartyComponentsExceptions) {
					Splash.Close(); // if occured on load
					if (DialogResult.Retry == RssBanditApplication.PublishException(new BanditApplicationException("StartMainGui() exiting main event loop on exception.", thirdPartyComponentsExceptions)))
						goto enter_mainevent_loop;
					Application.Exit();
				}
		}
	
		private void CheckForPlugins() {
#if NIGHTCRAWLER

			ArrayList plugIns = AppInteropServices.ServiceManager.SearchForIChannelProcessors(RssBanditApplication.GetPlugInPath());
			if (plugIns == null || plugIns.Count == 0)
				return;
			foreach (NewsComponents.IChannelProcessor processor in plugIns) {
				INewsChannel[] channels = processor.GetChannels() ;
				if (channels == null || channels.Length==0)
					continue;
				foreach (INewsChannel channel in channels) {
					feedHandler.RegisterNewsChannel(channel);
				}
			}
#endif
		}

		#endregion

		#region static class routines
		
		public static string Version { 
			get { 
				if (!StringHelper.EmptyOrNull(versionExtension)) 
					  return VersionOnly + " " + versionExtension;
				return VersionOnly;
			}
		}

		public static string VersionOnly { 
			get {
				if (appVersionInfo != null)
					return appVersionInfo;

				string versionStr = "1.1"; 
				foreach (ProcessModule module in Process.GetCurrentProcess().Modules) {
					if (module.ModuleName == "RSSBandit.exe") {
						FileVersionInfo verInfo = module.FileVersionInfo;
						versionStr = String.Format("{0}.{1}.{2}.{3}", 
							verInfo.FileMajorPart,
							verInfo.FileMinorPart,
							verInfo.FileBuildPart,
							verInfo.FilePrivatePart);
						break;
					}
				}
				appVersionInfo = versionStr;
				return appVersionInfo;
			} 
		}
		
		public static string ApplicationInfos {
			get {	
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0};UI:{1};", Name, Thread.CurrentThread.CurrentUICulture.Name);
				try {		sb.AppendFormat("OS:{0},",Environment.OSVersion.ToString());} 
				catch {	sb.Append("OS:n/a,");	}
				sb.AppendFormat("{0};",System.Globalization.CultureInfo.InstalledUICulture.Name);
				try {		sb.AppendFormat(".NET CLR:{0};",System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion());	} 
				catch {	sb.Append(".NET CLR:n/a;");		}
				return sb.ToString();
			}
		}

		public static string UpdateServiceUrl { 
			get { 
				int idx = DateTime.Now.Second % applicationUpdateServiceUrls.Length;
				return applicationUpdateServiceUrls[idx]; 
			} 
		}
		public static string AppGuid { get { return applicationGuid; } }
		public static string Name { get { return applicationId; } }
		public static string Caption { get { return applicationName + " " + Version; } }
		public static string CaptionOnly { get { return applicationName; } }
		public static string DefaultCategory { get { return defaultCategory; } }
		public static string UserAgent { get { return applicationId + "/" + Version; } }

		public static string GetUserPath() {
			
			string s = ApplicationDataFolderFromEnv;
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			return s;
		}

		public static string GetSearchesPath() {
			
			string s = ApplicationDataFolderFromEnv;
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			s = Path.Combine(s, "searches");
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			return s;
		}

		public static string GetTemplatesPath() {

			string s = Path.Combine(Application.StartupPath, "templates");
			if(!Directory.Exists(s)) return null;
			return s;
		}

		public static string GetPlugInPath() {

			string s = Path.Combine(Application.StartupPath, "plugins");
			if(!Directory.Exists(s)) return null;
			return s;
		}

		public static string GetFeedFileCachePath() {
			string s = ApplicationDataFolderFromEnv;
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			s = Path.Combine(s, @"Cache");
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			return s;
		}

		
		public static string GetErrorLogPath() {
		
			string s = ApplicationDataFolderFromEnv;
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			s = Path.Combine(s, "errorlog");
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			return s;
		}
		public static string GetFeedErrorFileName() {
			return Path.Combine(RssBanditApplication.GetErrorLogPath(), "feederrors.xml");
		}

		public static string GetFlagItemsFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "flagitems.xml");
		}

		public static string GetSentItemsFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "replyitems.xml");
		}
		public static string GetDeletedItemsFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "deleteditems.xml");
		}
		public static string GetSearchFolderFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "searchfolders.xml");
		}
		public static string GetShortcutSettingsFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "shortcutsettings.xml");
		}
		public static string GetFeedListFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "subscriptions.xml");
		}

		public static string GetOldFeedListFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "feedlist.xml");
		}

		public static string GetTrustedCertIssuesFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "certificates.config.xml");
		}

		public static string GetLogFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), "error.log");
		}
		public static string GetPreferencesFileName() {
			return Path.Combine(RssBanditApplication.GetUserPath(), ".preferences");
		}
		public static RssBanditPreferences DefaultPreferences {
			get { return defaultPrefs; }
		}
		public static bool UnconditionalCommentRss {
			get { return unconditionalCommentRss; }
		}
		public static bool AutomaticColorSchemes {
			get { return automaticColorSchemes; }
		}
		
		private static string ApplicationDataFolderFromEnv {
			get {
				if (StringHelper.EmptyOrNull(appDataFolderPath)) {
					appDataFolderPath = ConfigurationSettings.AppSettings["AppDataFolder"];
				}

				if (StringHelper.EmptyOrNull(appDataFolderPath)) {
					try {	// once
						appDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), RssBanditApplication.Name);
					} catch (SecurityException secEx) {
						//TODO: what should we do, if we cannot query for the appData folder?
						MessageBox.Show ("Cannot query for Environment.SpecialFolder.ApplicationData:\n"+secEx.Message, "Security violation");
						Application.Exit();
					}
				}
				return appDataFolderPath;
			}
		}

		/// <summary>
		/// Handles errors that occur during schema validation of RSS feed list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void FeedListValidationCallback(object sender,
			System.Xml.Schema.ValidationEventArgs args) {

			if(args.Severity == System.Xml.Schema.XmlSeverityType.Warning) {
				_log.Info(RssBanditApplication.GetFeedListFileName() + " validation warning: " + args.Message);

			}
			else if(args.Severity == System.Xml.Schema.XmlSeverityType.Error) {

				validationErrorOccured = true; 
				
				_log.Error(RssBanditApplication.GetFeedListFileName() + " validation error: " + args.Message);
				AppExceptions.ExceptionManager.Publish(args.Exception);
				
			}

		}

		/// <summary>
		/// Method install Bandit as the "feed:" url scheme handler
		/// </summary>
		public static void MakeDefaultAggregator() {
			string appPath = Application.ExecutablePath;
			try {
				Win32.Registry.CurrentFeedProtocolHandler = appPath;
				// on success, ask the next startup time, if we are not anymore the default handler:
				ShouldAskForDefaultAggregator = true;
			} catch (Exception ex) {
				_log.Debug("Unable to set CurrentFeedProtocolHandler", ex);
				throw;
			}
			
			CheckAndRegisterIEMenuExtensions();
		}
	
		/// <summary>
		/// Checks and register IE Menu Extensions.
		/// If we are the default aggregator, we only ensure to have one context menu entry
		/// delegating subscription to the registered default aggregator (feed: protocol handler).
		/// If we are not the default aggregator, we add an entry to enable subscription in
		/// Rss Bandit, ignoring any existing entry for the default aggregator.
		/// </summary>
		public static void CheckAndRegisterIEMenuExtensions() {
			try {
				if (IsDefaultAggregator()) {
					
					if (!Win32.Registry.IsInternetExplorerExtensionRegistered(Win32.Registry.IEMenuExtension.DefaultFeedAggregator))
						Win32.Registry.RegisterInternetExplorerExtension(Win32.Registry.IEMenuExtension.DefaultFeedAggregator);

					if (Win32.Registry.IsInternetExplorerExtensionRegistered(Win32.Registry.IEMenuExtension.Bandit))
						Win32.Registry.UnRegisterInternetExplorerExtension(Win32.Registry.IEMenuExtension.Bandit);

				} else {

					if (!Win32.Registry.IsInternetExplorerExtensionRegistered(Win32.Registry.IEMenuExtension.Bandit))
						Win32.Registry.RegisterInternetExplorerExtension(Win32.Registry.IEMenuExtension.Bandit);

				}
			} catch (Exception ex) {
				_log.Debug("CheckAndRegisterIEMenuExtensions(): Unable to modify InternetExplorerExtension", ex);
			}

		}

		/// <summary>
		/// Returns true, if a Yes/No dialog should be displayed on startup (asking for
		/// to make Bandit the default "feed:" scheme protocol handler)
		/// </summary>
		public static bool ShouldAskForDefaultAggregator {
			get { return (bool)guiSettings.GetProperty("AskForMakeDefaultAggregator", true);	}
			set { guiSettings.SetProperty("AskForMakeDefaultAggregator", value);}
		}

		/// <summary>
		/// Method test the running application, if it is registered as the
		/// default "feed:" protocol scheme handler.
		/// </summary>
		/// <returns>true, if registered, else false</returns>
		public static bool IsDefaultAggregator() {
			return IsDefaultAggregator(Application.ExecutablePath);
		}
		/// <summary>
		/// Method test the provided appPath (incl. .exe name!), if it is registered as the
		/// default "feed:" protocol scheme handler.
		/// </summary>
		/// <param name="appPath">Full path name incl. executable name</param>
		/// <returns>true, if registered, else false</returns>
		public static bool IsDefaultAggregator(string appPath) {
			bool isDefault = false;
			try {
				string currentHandler = Win32.Registry.CurrentFeedProtocolHandler;
				if (StringHelper.EmptyOrNull(currentHandler)) {	
					// we just take over the control, if it is not yet set
					RssBanditApplication.MakeDefaultAggregator();
					isDefault = true;
				}
				isDefault = (String.Concat(appPath, " ", "\"",  "%1", "\"").CompareTo(currentHandler) == 0);
			} catch (System.Security.SecurityException secex) {
				_log.Warn("Security exception error on make default aggregator.", secex);
			} catch (Exception e) {
				_log.Error("Unexpected Error while check for default aggregator", e);
			}
			return isDefault;
		}

		/// <summary>
		/// Publish a unexpected exception to the user (simple OK dialog is displayed)
		/// </summary>
		/// <param name="ex">Exception to report</param>
		/// <returns>OK DialogResult</returns>
		public static DialogResult PublishException(Exception ex) {
			return PublishException(ex, false);
		}
		/// <summary>
		/// Publish a unexpected exception to the user. 
		/// Retry/Ignore/Cancel dialog is displayed, if <c>resumable</c> is true.
		/// </summary>
		/// <param name="ex">Exception to report</param>
		/// <param name="resumable">Set this to true, if the exception is resumable and react
		/// to the DialogResult returned.</param>
		/// <returns>Retry/Ignore/Cancel DialogResults</returns>
		public static DialogResult PublishException(Exception ex, bool resumable) {
			return RssBanditApplication.ApplicationExceptionHandler.ShowExceptionDialog(ex, resumable);
		}

		/// <summary>
		/// Helper to create a wrapped Exception, that provides more error infos for a feed
		/// </summary>
		/// <param name="e">Exception</param>
		/// <param name="f">feedsFeed</param>
		/// <param name="fi">IFeedDetails</param>
		/// <returns></returns>
		public static FeedRequestException CreateLocalFeedRequestException(Exception e, feedsFeed f, IFeedDetails fi) {
			return new FeedRequestException(e.Message, e, NewsHandler.GetFailureContext(f, fi));
		}

		#endregion

		#region ICommandComponent related routines

		public CommandMediator Mediator {
			get { return this.cmdMediator; }
		}

		#endregion

		#region AsynWebRequest events
		/// <summary>
		/// Called by AsynWebRequest, if a web request caused a certificate problem.
		/// Used to sync. the async. call to the UI thread.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">CertificateIssueCancelEventArgs</param>
		private void OnRequestCertificateIssue(object sender, CertificateIssueCancelEventArgs e) {
			if (guiMain == null || guiMain.Disposing || guiMain.IsDisposed) return;

			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new CertificateIssueHandler(this.OnRequestCertificateIssue), new object[]{sender, e});
			} else {
				guiMain.OnRequestCertificateIssue(sender, e);
			}
		}
		#endregion

		#region NewsHandler events
		/// <summary>
		/// Called by RssParser, if a RefreshFeeds() was initiated (all feeds)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUpdateFeedsStarted(object sender, NewsHandler.UpdateFeedsEventArgs e) {
			if (guiMain == null || guiMain.Disposing || guiMain.IsDisposed) return;

			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new NewsHandler.UpdateFeedsStartedHandler(this.OnUpdateFeedsStarted), new object[]{sender, e});
			} else {
				if (e.ForcedRefresh)
					stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshAllForced);
				else
					stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshAllAuto);
			}
		}

		/// <summary>
		/// Called by RssParser, if a feed start's to download.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BeforeDownloadFeedStarted(object sender, NewsHandler.DownloadFeedCancelEventArgs e) {
			if (guiMain == null || guiMain.Disposing || guiMain.IsDisposed) return;

			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new NewsHandler.DownloadFeedStartedCallback(this.BeforeDownloadFeedStarted), new object[]{sender, e});
			} else {
				bool cancel = e.Cancel;
				guiMain.OnFeedUpdateStart(e.FeedUri, ref cancel);
				e.Cancel = cancel;
			}
		}

		/// <summary>
		/// Called by RssParser, if a Refresh of a individual Feed was initiated
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUpdateFeedStarted(object sender, NewsHandler.UpdateFeedEventArgs e) {
			if (guiMain == null || guiMain.Disposing || guiMain.IsDisposed) return;

			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new NewsHandler.UpdateFeedStartedHandler(this.OnUpdateFeedStarted), new object[]{sender, e});
			} else {
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOne);
			}
		}

		/// <summary>
		/// Called by RssParser, after a feed was updated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUpdatedFeed(object sender, NewsHandler.UpdatedFeedEventArgs e) {
			if (guiMain == null || guiMain.Disposing || guiMain.IsDisposed) return;

			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.UpdatedFeedCallback(this.OnUpdatedFeed), new object[]{sender, e});
			} else {
				guiMain.UpdateFeed(e.UpdatedFeedUri, e.NewFeedUri, e.FeedItems, e.UpdateState == NewsComponents.Net.RequestResult.OK);
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
			}
		}

		/// <summary>
		/// Called by RssParser, if update of a feed caused an exception
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUpdateFeedException(object sender, NewsHandler.UpdateFeedExceptionEventArgs e) {
			if (guiMain == null || guiMain.Disposing || guiMain.IsDisposed) return;

			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new NewsHandler.UpdateFeedExceptionCallback(this.OnUpdateFeedException), new object[]{sender, e});
			} else {
				WebException webex = e.ExceptionThrown as WebException;
				if (webex != null) {	// yes, WebException
					if (webex.Status == WebExceptionStatus.NameResolutionFailure || 
						webex.Status == WebExceptionStatus.ProxyNameResolutionFailure) {// connection lost?
						this.UpdateInternetConnectionState(true);	// update connect state
						if (!this.InternetAccessAllowed) {
							guiMain.UpdateFeed(e.UpdatedFeedUri, null, new ArrayList(), false);
							stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
							return;
						}
					}
				}
				Trace.WriteLine(e.ExceptionThrown.StackTrace); 
				this.UpdateXmlFeedErrorFeed(e.ExceptionThrown, e.UpdatedFeedUri, true);
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
			}
		}

		/// <summary>
		/// Called by RssParser, if all pending feed updates are done.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAllRequestsCompleted(object sender, EventArgs e) {
			if (guiMain == null || guiMain.Disposing || guiMain.IsDisposed) return;

			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new EventHandler(this.OnAllRequestsCompleted), new object[]{sender, e});
			} else {
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshAllDone);
				guiMain.TriggerGUIStateOnNewFeeds(true);
				guiMain.OnAllAsyncUpdateFeedsFinished();
				GC.Collect();
				RssBanditApplication.SetWorkingSet(750000,300000);
			}
		}
		#endregion

		#region Task management
		public void BeginLoadingFeedlist() {
			ShowProgressHandler handler = new ShowProgressHandler(this.OnLoadingFeedlistProgress);
			ThreadWorker.StartTask(ThreadWorker.Task.LoadFeedlist, this.guiMain, handler, this, null);
		}

		private void OnLoadingFeedlistProgress(object sender, ShowProgressArgs args) {
			if (args.Exception != null) {
				// failure(s)
				args.Cancel = true;
				BanditApplicationException ex = args.Exception as BanditApplicationException;
				if (ex != null) {
					if (ex.Number == ApplicationExceptions.FeedlistOldFormat) {
						Application.Exit();
					} else if (ex.Number == ApplicationExceptions.FeedlistOnRead) {
						AppExceptions.ExceptionManager.Publish(ex.InnerException);
						this.MessageError("RES_ExceptionReadingFeedlistFile",  ex.InnerException.Message, RssBanditApplication.GetLogFileName());
						this.SetGuiStateFeedback("RES_GUIStatusErrorReadingFeedlistFile");
					} else if (ex.Number == ApplicationExceptions.FeedlistOnProcessContent) {
						this.MessageError("RES_InvalidFeedlistFileMessage", RssBanditApplication.GetLogFileName());
						this.SetGuiStateFeedback("RES_GUIStatusValidationErrorReadingFeedlistFile");
					} else if (ex.Number == ApplicationExceptions.FeedlistNA) {
						this.refreshRate = feedHandler.RefreshRate;
						this.SetGuiStateFeedback("RES_GUIStatusNoFeedlistFile");
					} else {
						RssBanditApplication.PublishException(args.Exception);
						this.SetGuiStateFeedback("RES_GUIStatusErrorReadingFeedlistFile");
					}
				} else {	// unhandled
					RssBanditApplication.PublishException(args.Exception);
					this.SetGuiStateFeedback("RES_GUIStatusErrorReadingFeedlistFile");
				}
			} else if (!args.Done) {
				// in progress
				if (this.guiMain == null || this.guiMain.Disposing) { args.Cancel = true; return; }
				this.SetGuiStateFeedback("RES_GUIStatusLoadingFeedlist");
			} else if (args.Done) {
				// done
				this.refreshRate = feedHandler.RefreshRate;	// loaded from feedlist
				
				this.CheckAndMigrateSettingsAndPreferences();		// needs the feedlist to be loaded
				this.CheckAndMigrateListViewLayouts();

				if (this.guiMain == null || this.guiMain.Disposing) { args.Cancel = true; return; }

				try {
					this.guiMain.PopulateFeedSubscriptions(feedHandler.Categories, feedHandler.FeedsTable, RssBanditApplication.DefaultCategory);
				} catch (Exception ex) {
					RssBanditApplication.PublishException(ex);
				}
				
				if (FeedlistLoaded != null)
					FeedlistLoaded(this, EventArgs.Empty);

				this.SetGuiStateFeedback("RES_GUIStatusDone");

				foreach (string newFeedUrl in this.commandLineOptions.SubscribeTo) {
					if (this.guiMain != null && !this.guiMain.Disposing)
						this.guiMain.AddFeedUrlSynchronized(newFeedUrl);	 
				}

				// start load items and refresh from web, if we have to refresh on startup:
				guiMain.UpdateAllFeeds(this.Preferences.FeedRefreshOnStartup);

			}
		}

		public void BeginLoadingSpecialFeeds() {
			ShowProgressHandler handler = new ShowProgressHandler(this.OnLoadingSpecialFeedsProgress);
			ThreadWorker.StartTask(ThreadWorker.Task.LoadSpecialFeeds, this.guiMain, handler, this);
		}

		private void OnLoadingSpecialFeedsProgress(object sender, ShowProgressArgs args) {
			if (args.Exception != null) {
				// failure(s)
				args.Cancel = true;
				RssBanditApplication.PublishException(args.Exception);
			} else if (!args.Done) {
				// in progress
			} else if (args.Done) {
				// done
				this.guiMain.PopulateTreeSpecialFeeds();
			}
		}

		public void BeginRefreshFeeds(bool forceDownload) {
			if (this.InternetAccessAllowed) {
				this.StateHandler.MoveNewsHandlerStateTo(forceDownload ? NewsHandlerState.RefreshAllForced: NewsHandlerState.RefreshAllAuto);
				ShowProgressHandler handler = new ShowProgressHandler(this.OnRefreshFeedsProgress);
				ThreadWorker.StartTask(ThreadWorker.Task.RefreshFeeds, this.guiMain, handler, this, forceDownload);
			}
		}

		public void BeginRefreshCategoryFeeds(string category, bool forceDownload) {
			if (this.InternetAccessAllowed) {
				this.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshCategory);
				ShowProgressHandler handler = new ShowProgressHandler(this.OnRefreshFeedsProgress);
				ThreadWorker.StartTask(ThreadWorker.Task.RefreshCategoryFeeds, this.guiMain, handler, this, category, forceDownload);
			}
		}

		private void OnRefreshFeedsProgress(object sender, ShowProgressArgs args) {
			if (args.Exception != null) {
				// failure(s)
				args.Cancel = true;
				RssBanditApplication.PublishException(args.Exception);
			} else if (!args.Done) {
				// handled via separate events
			} else if (args.Done) {
				// done
				// handled via separate events
			}
		}

		#endregion

		public NewsHandler FeedHandler { get { return this.feedHandler; } }

		public RssBanditPreferences Preferences { 
			get { return currentPrefs;  } 
			set { currentPrefs = value; } 
		}

		public void FeedWasModified(string feedUrl) { 
			if (StringHelper.EmptyOrNull(feedUrl))
				return;
			lock (modifiedFeeds) {
				if (!modifiedFeeds.Contains(feedUrl))
					modifiedFeeds.Enqueue(feedUrl);
			}
		}

		public SearchEngineHandler SearchEngineHandler {
			get { return searchEngines; }
		}

		public Settings GuiSettings { 
			get { return guiSettings;  } 
		}

		public GuiStateManager StateHandler {
			get { return stateManager; }
		}

		public IWebProxy Proxy { 
			get { return this.feedHandler.Proxy;  } 
			set { this.feedHandler.Proxy = value; } 
		}

		public ArrayList FinderList {
			get { return findersSearchRoot.RssFinderNodes; }
			set { this.findersSearchRoot.RssFinderNodes = value; }
		}

		#region IInternetService Members

		public event RssBandit.AppServices.InternetConnectionStateChangeHandler InternetConnectionStateChange;

		public bool InternetAccessAllowed {
			get { return stateManager.InternetAccessAllowed; }
		}

		public bool InternetConnectionOffline {
			get { return stateManager.InternetConnectionOffline; }
		}
		
		public INetState InternetConnectionState {
			get { return stateManager.InternetConnectionState; 	}
		}

		#endregion


		/// <summary>
		/// Property FeedlistModified (bool)
		/// </summary>
		public bool FeedlistModified {
			get { return this.feedlistModified;  }
			set { this.feedlistModified = value; }
		}
		
		public AutoDiscoveredFeedsMenuHandler BackgroundDiscoverFeedsHandler {
			get { return this.backgroundDiscoverFeedsHandler; }
		}

		private void OnBackgroundDiscoveredFeedsSubscribe(object sender, DiscoveredFeedsSubscribeCancelEventArgs e) {
			if (e.FeedsInfo.FeedLinks.Count == 1) {
				e.Cancel = !this.CmdNewFeed(DefaultCategory, (string)e.FeedsInfo.FeedLinks[0], e.FeedsInfo.Title);
			} else if (e.FeedsInfo.FeedLinks.Count > 1) {

				Hashtable feedUrls = new Hashtable(e.FeedsInfo.FeedLinks.Count);
				foreach (string feedUrl in e.FeedsInfo.FeedLinks) {
					feedUrls.Add(feedUrl, new string[]{e.FeedsInfo.Title, String.Empty, e.FeedsInfo.SiteBaseUrl, feedUrl});
				}
                    
				DiscoveredFeedsDialog discoveredFeedsDialog = new DiscoveredFeedsDialog(feedUrls); 
				discoveredFeedsDialog.ShowDialog(guiMain);

				if(discoveredFeedsDialog.DialogResult == DialogResult.OK) {
					e.Cancel = true;
					foreach( ListViewItem feedItem in discoveredFeedsDialog.listFeeds.SelectedItems ) {
						if (this.CmdNewFeed(defaultCategory, (string)feedItem.Tag, feedItem.SubItems[0].Text) && e.Cancel)
							e.Cancel = false;	// at least one dialog succeeds
					}
				} else {
					e.Cancel = true;
				}
			}
		}
		
		private void CheckAndMigrateListViewLayouts() {
			// check, if any migration task have to be applied:

			// v.1.3.x beta to 1.3.x.release:
			// layouts was serialized directly to feed/category elements
			// now they live in a separate collection

			// we assume we should have always at least the default layouts:
			if (feedHandler.ColumnLayouts.Count == 0) {
				
				NewsComponents.Collections.FeedColumnLayoutCollection myLayouts = new NewsComponents.Collections.FeedColumnLayoutCollection();
				ListViewLayout oldLayout = new ListViewLayout();
				foreach (feedsFeed f in feedHandler.FeedsTable.Values) {
					if (StringHelper.EmptyOrNull(f.listviewlayout) || f.listviewlayout.IndexOf("<") < 0 )
						continue;
					try {
						oldLayout = ListViewLayout.CreateFromXML(f.listviewlayout);
						FeedColumnLayout fc = new FeedColumnLayout(oldLayout.ColumnList,
							oldLayout.ColumnWidthList, oldLayout.SortByColumn, oldLayout.SortOrder, LayoutType.IndividualLayout);
						
						if (!fc.Equals(DefaultFeedColumnLayout, true))
						{
							int found = feedHandler.ColumnLayouts.IndexOfSimilar(fc);
							if (found >= 0) {	// assign key
								f.listviewlayout = feedHandler.ColumnLayouts.GetKey(found);
							} else {	// add and assign key
								f.listviewlayout = Guid.NewGuid().ToString("N");
								feedHandler.ColumnLayouts.Add(f.listviewlayout, fc);
							}
						} else {
							f.listviewlayout = null;	// same as default: reset
						}

					} catch (Exception ex) { _log.Error(ex.Message, ex); /* ignore deserialization failures */ }
				}

				foreach (category c in feedHandler.Categories.Values) {
					if (StringHelper.EmptyOrNull(c.listviewlayout) || c.listviewlayout.IndexOf("<") < 0 )
						continue;
					try {
						oldLayout = ListViewLayout.CreateFromXML(c.listviewlayout);
						FeedColumnLayout fc = new FeedColumnLayout(oldLayout.ColumnList,
							oldLayout.ColumnWidthList, oldLayout.SortByColumn, oldLayout.SortOrder, LayoutType.IndividualLayout);

						if (!fc.Equals(DefaultCategoryColumnLayout, true)) {
							int found = feedHandler.ColumnLayouts.IndexOfSimilar(fc);
							if (found >= 0) {	// assign key
								c.listviewlayout = feedHandler.ColumnLayouts.GetKey(found);
							} else {	// add and assign key
								c.listviewlayout = Guid.NewGuid().ToString("N");
								feedHandler.ColumnLayouts.Add(c.listviewlayout, fc);
							}
						} else {
							c.listviewlayout = null;	// same as default: reset
						}

					} catch (Exception ex) { _log.Error(ex.Message, ex); /* ignore deserialization failures */ }

				}

				// now add the default layouts
				feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultFeedColumnLayout);
				feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultCategoryColumnLayout);
				feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultSearchFolderColumnLayout);
				feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultSpecialFolderColumnLayout);

				if (!StringHelper.EmptyOrNull(feedHandler.FeedColumnLayout)) try {
					oldLayout = ListViewLayout.CreateFromXML(feedHandler.FeedColumnLayout);
					FeedColumnLayout fc = new FeedColumnLayout(oldLayout.ColumnList,
						oldLayout.ColumnWidthList, oldLayout.SortByColumn, oldLayout.SortOrder, LayoutType.IndividualLayout);

					if (!fc.Equals(DefaultCategoryColumnLayout, true)) {
						int found = feedHandler.ColumnLayouts.IndexOfSimilar(fc);
						if (found >= 0) {	// assign key
							feedHandler.FeedColumnLayout = feedHandler.ColumnLayouts.GetKey(found);
						} else {	// add and assign key
							feedHandler.FeedColumnLayout = Guid.NewGuid().ToString("N");
							feedHandler.ColumnLayouts.Add(feedHandler.FeedColumnLayout, fc);
						}
					} else {
						feedHandler.FeedColumnLayout = defaultCategoryColumnLayoutKey;	// same as default
					}

				} catch (Exception ex) { _log.Error(ex.Message, ex); /* ignore deserialization failures */ }

				this.feedlistModified = true;	// trigger auto-save
			}
		}

		#region class kept for migration purpose:
		[Serializable]
		public class ListViewLayout: ICloneable {
			private string _sortByColumn;
			private NewsComponents.SortOrder _sortOrder;
			internal ArrayList _columns;
			internal ArrayList _columnWidths;
			private bool _modified;

			public ListViewLayout():this(null, null, null, NewsComponents.SortOrder.None) {	}
			public ListViewLayout(ICollection columns, ICollection columnWidths, string sortByColumn, NewsComponents.SortOrder sortOrder) {
				if (columns != null)
					_columns = new ArrayList(columns);
				else
					_columns = new ArrayList();
				if (columnWidths != null)
					_columnWidths = new ArrayList(columnWidths);
				else
					_columnWidths = new ArrayList();
				_sortByColumn = sortByColumn;
				_sortOrder = sortOrder;
			}

			public static ListViewLayout CreateFromXML(string xmlString) {
				if (xmlString != null && xmlString.Length > 0) {
					XmlSerializer formatter = new XmlSerializer(typeof(ListViewLayout));
					StringReader reader = new StringReader(xmlString);
					return (ListViewLayout)formatter.Deserialize(reader);
				}
				return null;
			}

			public static string SaveAsXML(ListViewLayout layout) {
				if (layout == null)
					return null;
				try {
					XmlSerializer formatter = new XmlSerializer(typeof(ListViewLayout));
					StringWriter writer = new StringWriter();
					formatter.Serialize(writer, layout);
					return writer.ToString();
				} catch (Exception ex) {
					Trace.WriteLine("SaveAsXML() failed.", ex.Message);
				}
				return null;
			}

			#region IListViewLayout Members

			public string SortByColumn {
				get {	return _sortByColumn;	}
				set {	_sortByColumn = value;	}
			}

			public NewsComponents.SortOrder SortOrder {
				get {	return _sortOrder; }
				set { _sortOrder = value; }
			}

			[XmlIgnore]
			public IList Columns {
				get {	return _columns;	}
				set { 
					if (value != null)
						_columns = new ArrayList(value); 
					else
						_columns = new ArrayList();
				}
			}

			[XmlIgnore]
			public IList ColumnWidths {
				get {	return _columnWidths;	}
				set { 
					if (value != null)
						_columnWidths = new ArrayList(value); 
					else
						_columnWidths = new ArrayList();
				}
			}

			[XmlIgnore]
			public bool Modified {
				get {	return _modified;	}
				set {	_modified = value;	}
			}

			#endregion

			[XmlArrayItem(typeof(string))]
			public ArrayList ColumnList {
				get {	return _columns;	}
				set { 
					if (value != null)
						_columns = value; 
					else
						_columns = new ArrayList();
				}
			}
			[XmlArrayItem(typeof(int))]
			public ArrayList ColumnWidthList {
				get {	return _columnWidths;	}
				set { 
					if (value != null)
						_columnWidths = value; 
					else
						_columnWidths = new ArrayList();
				}
			}

			public override bool Equals(object obj) {
				if (obj == null)
					return false;
				ListViewLayout o = obj as ListViewLayout;
				if (o== null)
					return false;
				if (this.SortOrder != o.SortOrder)
					return false;
				if (this.SortByColumn != o.SortByColumn)
					return false;
				if (this._columns == null && o._columns == null)
					return true;
				if (this._columns == null || o._columns == null)
					return false;
				if (this._columns.Count != o._columns.Count)
					return false;
				for (int i = 0; i < this._columns.Count; i++) {
					if (String.Compare((string)this._columns[i], (string)o._columns[i]) != 0 || 
						(int)this._columnWidths[i] != (int)o._columnWidths[i])
						return false;
				}
				return true;
			}

			public override int GetHashCode() {	// just to hide the compiler warning
				return base.GetHashCode ();
			}

			#region ICloneable Members

			public object Clone() {
				return new ListViewLayout(_columns, _columnWidths, _sortByColumn, _sortOrder);
			}

			#endregion
		}
		#endregion

		private void CheckAndMigrateSettingsAndPreferences() {
			// check, if any migration task have to be applied:
			
			// v.1.2.x to 1.3.x:
			// The old (one) username, mail and referer 
			// have to be migrated from Preferences to the default UserIdentity

			// Obsolete() warnings can be ignored for that function.
			if (!StringHelper.EmptyOrNull(this.Preferences.UserName) &&
				StringHelper.EmptyOrNull(this.Preferences.UserIdentityForComments)) {
				
				if (!this.feedHandler.UserIdentity.Contains(this.Preferences.UserName)) {
					//create a UserIdentity from Prefs. properties
					UserIdentity ui = new UserIdentity();
					ui.Name = ui.RealName = this.Preferences.UserName;
					ui.ResponseAddress = ui.MailAddress = this.Preferences.UserMailAddress;
					ui.ReferrerUrl = this.Preferences.Referer;
					this.feedHandler.UserIdentity.Add(ui.Name, ui);
					this.feedlistModified = true;
				} else {
					// yet contain the ID, nothing to do here
				}

				// set/reset values:
				this.Preferences.UserIdentityForComments = this.Preferences.UserName;
				this.Preferences.UserName = String.Empty;
				this.Preferences.UserMailAddress = String.Empty;
				this.Preferences.Referer = String.Empty;
				this.SavePreferences();
			}
		}

		public DialogResult MessageQuestion(string messageResId) {
			Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm,
				Resource.Manager[messageResId], 
				RssBanditApplication.CaptionOnly, 
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
		}

		public DialogResult MessageQuestion(string messageResId, params object[] formatArgs ) {
			Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm,
				Resource.Manager.FormatMessage(messageResId, formatArgs), 
				RssBanditApplication.CaptionOnly, 
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
		}

		public DialogResult MessageInfo(string messageResId) {
			Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm,
				Resource.Manager[messageResId], 
				RssBanditApplication.CaptionOnly, 
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		public DialogResult MessageInfo(string messageResId, params object[] formatArgs ) {
			Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm,
				Resource.Manager.FormatMessage(messageResId, formatArgs), 
				RssBanditApplication.CaptionOnly, 
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		public DialogResult MessageError(string messageResId) {
			Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm,
				Resource.Manager[messageResId], 
				RssBanditApplication.Caption, 
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}

		public DialogResult MessageError(string messageResId, params object[] formatArgs ) {
			Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm,
				Resource.Manager.FormatMessage(messageResId, formatArgs), 
				RssBanditApplication.Caption, 
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}

		private void SaveModifiedFeeds() {
			while (modifiedFeeds.Count > 0) {
				string feedUrl = null;
				lock (modifiedFeeds) {
					feedUrl = (string)modifiedFeeds.Dequeue();
				}
				try {
					this.feedHandler.ApplyFeedModifications(feedUrl);
				} catch {}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void LoadSearchEngines() {

			string p = Path.Combine(RssBanditApplication.GetSearchesPath(), @"config.xml");
			if (File.Exists(p)) {
				string errorLog = RssBanditApplication.GetLogFileName();
				using (FileStream myFile =  FileHelper.OpenForWriteAppend(errorLog)) {
 
					/* Create a new text writer using the output stream, and add it to
					 * the trace listeners. */
					TextWriterTraceListener myTextListener = new 
						TextWriterTraceListener(myFile);
					Trace.Listeners.Add(myTextListener);

					try {
						this.SearchEngineHandler.LoadEngines(p, new System.Xml.Schema.ValidationEventHandler(this.SearchConfigValidationCallback));
					}
					catch (Exception e) {
						if (!validationErrorOccured) {
							this.MessageError("RES_ExceptionLoadingSearchEnginesMessage", e.Message, errorLog);
						}
					}
			
					if(this.SearchEngineHandler.EnginesOK) {		
						// Build the menues, below
					}
					else if(validationErrorOccured) {					
						this.MessageError("RES_ExceptionInvalidSearchEnginesMessage", errorLog);
						validationErrorOccured = false;  
					}
					else {
						// no search engines
					}

					// Flush and close the trace output.
					Trace.Listeners.Remove(myTextListener); 
					myTextListener.Close();
				}
			} 
			else { //if (File.Exists(p))
				this.SearchEngineHandler.GenerateDefaultEngines();
				this.SaveSearchEngines();
			}
		}

		internal void SaveSearchEngines() {
			try { 
				if(this.SearchEngineHandler != null && this.SearchEngineHandler.Engines != null && 
					this.SearchEngineHandler.EnginesOK && this.SearchEngineHandler.Engines.Count > 0) { 
					string p = Path.Combine(RssBanditApplication.GetSearchesPath(), @"config.xml");
					this.SearchEngineHandler.SaveEngines(new FileStream(p, FileMode.Create)); 
				}
			}
			catch(InvalidOperationException ioe) {
				_log.Error("Unexpected Error on saving SearchEngineSettings.", ioe);
				this.MessageError("RES_ExceptionWebSearchEnginesSave", ioe.InnerException.Message);
			}
			catch(Exception ex) {
				_log.Error("Unexpected Error on saving SearchEngineSettings.", ex);
				this.MessageError("RES_ExceptionWebSearchEnginesSave", ex.Message);
			}

		}

		/// <summary>
		/// Handles errors that occur during schema validation of search engines list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SearchConfigValidationCallback(object sender,
			System.Xml.Schema.ValidationEventArgs args) {
			if(args.Severity == System.Xml.Schema.XmlSeverityType.Warning) {
				_log.Info("Validation Warning on search engines list: " + args.Message);
			}
			else if(args.Severity == System.Xml.Schema.XmlSeverityType.Error) {
				validationErrorOccured = true; 
				_log.Error("Validation Error on search engines list: " + args.Message);
			}
		}

		#region Preferences handling
		internal void LoadPreferences() {
			try {
				Stream stream = new FileStream(RssBanditApplication.GetPreferencesFileName(), FileMode.Open,
					FileAccess.Read, FileShare.Read);
				try {
					IFormatter formatter = new BinaryFormatter();
					// to provide backward compat.:
					formatter.Binder = new RssBanditPreferences.DeserializationTypeBinder();
					RssBanditPreferences p = (RssBanditPreferences)formatter.Deserialize(stream);
					Preferences = p;
				} catch (SerializationException se) {
					_log.Error("Preferences DeserializationException", se);
				}
				finally {
					stream.Close();
				}
			}
			catch {}
		}

		// called from Preferences dialog via "Apply" button
		// takes over the settings
		private void OnApplyPreferences(object sender, EventArgs e) {
			
			PreferencesDialog	 propertiesDialog = sender as PreferencesDialog;
			if (propertiesDialog == null)
				return;

			//validate  refresh rate before setting
			try { 

				if(!StringHelper.EmptyOrNull(propertiesDialog.comboRefreshRate.Text)) {
					this.refreshRate = System.Int32.Parse(propertiesDialog.comboRefreshRate.Text) * MilliSecsMultiplier; 
					feedHandler.RefreshRate = this.refreshRate;
					this.FeedlistModified = true;
				}

			}
			catch(FormatException) {
				MessageBox.Show (propertiesDialog, 
					Resource.Manager["RES_FormatExceptionRefreshRate"],
					Resource.Manager["RES_PreferencesExceptionMessageTitle"], 
					MessageBoxButtons.OK, MessageBoxIcon.Error);						
			}
			catch(OverflowException) {
				MessageBox.Show (propertiesDialog, 
					Resource.Manager["RES_OverflowExceptionRefreshRate"],
					Resource.Manager["RES_PreferencesExceptionMessageTitle"], 
					MessageBoxButtons.OK, MessageBoxIcon.Error);						
			}

			//validate proxy port before before setting proxy info
			try {

				Preferences.ProxyPort  = System.UInt16.Parse("0"+propertiesDialog.textProxyPort.Text); 
				Preferences.UseIEProxySettings = propertiesDialog.checkUseIEProxySettings.Checked; 
				Preferences.UseProxy = propertiesDialog.checkUseProxy.Checked; 
				Preferences.ProxyAddress  = propertiesDialog.textProxyAddress.Text;
				Preferences.ProxyCustomCredentials = propertiesDialog.checkProxyAuth.Checked;
				Preferences.BypassProxyOnLocal = propertiesDialog.checkProxyBypassLocal.Checked;
				Preferences.ProxyUser = propertiesDialog.textProxyCredentialUser.Text;
				Preferences.ProxyPassword = propertiesDialog.textProxyCredentialPassword.Text;
				Preferences.ProxyBypassList = ParseProxyBypassList(propertiesDialog.textProxyBypassList.Text);

			}
			catch(FormatException) {
				MessageBox.Show (propertiesDialog,
					Resource.Manager["RES_FormatExceptionProxyPort"], 
					Resource.Manager["RES_PreferencesExceptionMessageTitle"], 
					MessageBoxButtons.OK, MessageBoxIcon.Error);						
			}
			catch(OverflowException) {
				MessageBox.Show (propertiesDialog, 
					Resource.Manager["RES_ExceptionProxyPortRange"],
					Resource.Manager["RES_PreferencesExceptionMessageTitle"], 
					MessageBoxButtons.OK, MessageBoxIcon.Error);						
			}

			
			if (propertiesDialog.checkCustomFormatter.Checked) {
				this.feedHandler.Stylesheet = Preferences.NewsItemStylesheetFile = propertiesDialog.comboFormatters.Text;				
			} else {
				this.feedHandler.Stylesheet = Preferences.NewsItemStylesheetFile = String.Empty;
			}

			if (Preferences.MaxItemAge != propertiesDialog.MaxItemAge) {
				Preferences.MaxItemAge = propertiesDialog.MaxItemAge;
				this.feedHandler.MarkForDownload();	// all
			}

			if(Preferences.MarkItemsReadOnExit != propertiesDialog.checkMarkItemsReadOnExit.Checked){
				this.feedHandler.MarkItemsReadOnExit = Preferences.MarkItemsReadOnExit = propertiesDialog.checkMarkItemsReadOnExit.Checked;
			}

			Preferences.ReuseFirstBrowserTab = propertiesDialog.checkReuseFirstBrowserTab.Checked;
			Preferences.FeedRefreshOnStartup = propertiesDialog.checkRefreshFeedsOnStartup.Checked;

			Preferences.UserIdentityForComments = propertiesDialog.cboUserIdentityForComments.Text;

			if (propertiesDialog.radioTrayActionMinimize.Checked)
				Preferences.HideToTrayAction = HideToTray.OnMinimize;
			if (propertiesDialog.radioTrayActionClose.Checked)
				Preferences.HideToTrayAction = HideToTray.OnClose;
			if (propertiesDialog.radioTrayActionNone.Checked)
				Preferences.HideToTrayAction = HideToTray.None;

			Preferences.AutoUpdateFrequency = (AutoUpdateMode)propertiesDialog.comboAppUpdateFrequency.SelectedIndex;

			Preferences.NormalFont = (Font)propertiesDialog.FontForState(FontStates.Read).Clone();
			Preferences.HighlightFont = (Font)propertiesDialog.FontForState(FontStates.Unread).Clone();
			Preferences.FlagFont = (Font)propertiesDialog.FontForState(FontStates.Flag).Clone();
			Preferences.RefererFont = (Font)propertiesDialog.FontForState(FontStates.Referrer).Clone();
			Preferences.ErrorFont = (Font)propertiesDialog.FontForState(FontStates.Error).Clone();
			Preferences.NormalFontColor = propertiesDialog.ColorForState(FontStates.Read);
			Preferences.HighlightFontColor = propertiesDialog.ColorForState(FontStates.Unread);
			Preferences.FlagFontColor = propertiesDialog.ColorForState(FontStates.Flag);
			Preferences.RefererFontColor = propertiesDialog.ColorForState(FontStates.Referrer);
			Preferences.ErrorFontColor = propertiesDialog.ColorForState(FontStates.Error);

			Preferences.UseRemoteStorage = propertiesDialog.checkUseRemoteStorage.Checked;
			Preferences.RemoteStorageLocation = propertiesDialog.textRemoteStorageLocation.Text;
			Preferences.RemoteStorageUserName = propertiesDialog.textRemoteStorageUserName.Text;
			Preferences.RemoteStoragePassword = propertiesDialog.textRemoteStoragePassword.Text;

			switch (propertiesDialog.comboRemoteStorageProtocol.SelectedIndex) {
				case 0: // now using index, names are to be localized 
					Preferences.RemoteStorageProtocol = RemoteStorageProtocolType.UNC;
					break;
				case 1: //"FTP"
					Preferences.RemoteStorageProtocol = RemoteStorageProtocolType.FTP;
					break;
				case 2: //"dasBlog"
					Preferences.RemoteStorageProtocol = RemoteStorageProtocolType.dasBlog;
					break;
				case 3: //"dasBlog"
					Preferences.RemoteStorageProtocol = RemoteStorageProtocolType.dasBlog_1_3;
					break;
				case 4: //"WebDAV"
					Preferences.RemoteStorageProtocol = RemoteStorageProtocolType.WebDAV;
					break;
			}

			if (propertiesDialog.optNewWindowOnTab.Checked)
				Preferences.BrowserOnNewWindow = BrowserBehaviorOnNewWindow.OpenNewTab;
			if (propertiesDialog.optNewWindowDefaultWebBrowser.Checked)
				Preferences.BrowserOnNewWindow = BrowserBehaviorOnNewWindow.OpenDefaultBrowser;
			if (propertiesDialog.optNewWindowCustomExec.Checked)
				Preferences.BrowserOnNewWindow = BrowserBehaviorOnNewWindow.OpenWithCustomExecutable;

			Preferences.BrowserCustomExecOnNewWindow = propertiesDialog.txtBrowserStartExecutable.Text;
			Preferences.NewsItemOpenLinkInDetailWindow = propertiesDialog.chkNewsItemOpenLinkInDetailWindow.Checked;

			if (propertiesDialog.searchEnginesModified) {
				// take over the new settings and move the images to the search folder 
				
				this.SearchEngineHandler.Clear();				// reset handler, if it was wrong from a initial load failure

				foreach (SearchEngine engine in propertiesDialog.searchEngines) {
					if (engine.ImageName != null && engine.ImageName.IndexOf(Path.DirectorySeparatorChar) > 0)	{
						// absolute path, copy the image to our search config folder
						try {
							if (File.Exists(engine.ImageName))
								File.Copy(engine.ImageName, Path.Combine(RssBanditApplication.GetSearchesPath(), Path.GetFileName(engine.ImageName) ), true);
							engine.ImageName = Path.GetFileName(engine.ImageName);	// reduce to "name.ext" only
						} catch (Exception ex) {
							_log.Error("SearchEngine Image FileCopy exception", ex);
							engine.ImageName = String.Empty;
						}
					}
					this.SearchEngineHandler.Engines.Add(engine);
				}

				this.SaveSearchEngines();
				guiMain.InitSearchEngines();	// rebuild menu(s)/toolbar entries
			}

			Preferences.BrowserJavascriptAllowed = propertiesDialog.checkBrowserJavascriptAllowed.Checked;
			Preferences.BrowserJavaAllowed = propertiesDialog.checkBrowserJavaAllowed.Checked;
			Preferences.BrowserActiveXAllowed = propertiesDialog.checkBrowserActiveXAllowed.Checked;
			Preferences.BrowserBGSoundAllowed = propertiesDialog.checkBrowserBGSoundAllowed.Checked;
			Preferences.BrowserVideoAllowed = propertiesDialog.checkBrowserVdieoAllowed.Checked;
			Preferences.BrowserImagesAllowed = propertiesDialog.checkBrowserImagesAllowed.Checked;

			this.ApplyPreferences();
			this.SavePreferences();
		}
		
		private string[] ParseProxyBypassList(string proxyBypassString)
		{
			return proxyBypassString.Split(';', ' ', ',');
		}

		// called, to apply preferences to the NewsComponents and Gui
		internal void ApplyPreferences() {

			this.feedHandler.MaxItemAge = Preferences.MaxItemAge;
			this.Proxy = this.CreateProxyFrom(Preferences);
			//this.NewsItemFormatter.DesignMode = Preferences.NewsItemDesignMode;

			try {
				this.NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile, GetNewsItemFormatterTemplate());
			}
			catch {
				
				//this.NewsItemFormatter.XslStyleSheet = NewsItemFormatter.DefaultNewsItemTemplate;				
				Preferences.NewsItemStylesheetFile = String.Empty;	
				this.NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile, GetNewsItemFormatterTemplate());
			
			}
			this.feedHandler.Stylesheet = Preferences.NewsItemStylesheetFile;

			if (guiMain != null && guiMain.Visible)	// initiate a refresh of the NewsItem detail pane
				guiMain.OnFeedListItemActivate(this, null);

			if (guiMain != null && guiMain.Visible && guiMain.CurrentSelectedNode != null) {
				guiMain.CurrentSelectedNode.TreeView.SelectedNode = guiMain.CurrentSelectedNode;				
				
				if(guiMain.NumSelectedListViewItems == 0){
					guiMain.RefreshFeedDisplay(guiMain.CurrentSelectedNode, false);				
				}
			}

			if (guiMain != null) {
				guiMain.SetFontAndColor(
					Preferences.NormalFont, Preferences.NormalFontColor,
					Preferences.HighlightFont, Preferences.HighlightFontColor,
					Preferences.FlagFont, Preferences.FlagFontColor,
					Preferences.ErrorFont, Preferences.ErrorFontColor,
					Preferences.RefererFont, Preferences.RefererFontColor
					);
				Mediator.SetEnable(Preferences.UseRemoteStorage, "cmdUploadFeeds", "cmdDownloadFeeds");
			}

		}


		internal void SavePreferences() {
			
			using (MemoryStream stream = new MemoryStream()) {
				IFormatter formatter = new BinaryFormatter();
				try {
					formatter.Serialize(stream, Preferences);
					if (FileHelper.WriteStreamWithBackup(RssBanditApplication.GetPreferencesFileName(), stream)) {
						// on success, raise event:
						if (PreferencesChanged != null)
							PreferencesChanged(this, new EventArgs());
					}
				} catch (Exception ex) {
					_log.Error("SavePreferences() failed.", ex);
				}
			}

		}

		private IWebProxy CreateProxyFrom(RssBanditPreferences p) {
			
			// default proxy init:
			IWebProxy proxy = GlobalProxySelection.GetEmptyWebProxy() as IWebProxy; 
			proxy.Credentials = CredentialCache.DefaultCredentials;

			if(p.UseProxy) {	// private proxy settings

				if (p.ProxyPort > 0)
					proxy = new WebProxy(p.ProxyAddress, p.ProxyPort); 
				else
					proxy = new WebProxy(p.ProxyAddress); 

				proxy.Credentials = CredentialCache.DefaultCredentials;
				((WebProxy)proxy).BypassProxyOnLocal = p.BypassProxyOnLocal;
				((WebProxy)proxy).BypassList = p.ProxyBypassList;

				//Get rid of String.Empty in by pass list because it means bypass on all URLs
				for(int i = 0; i < ((WebProxy)proxy).BypassArrayList.Count; i++){
					
					string s = (string) ((WebProxy)proxy).BypassArrayList[i];
					
					if(s.Equals(String.Empty)){
						((WebProxy)proxy).BypassArrayList.RemoveAt(i);
						i--;
					}
				
				}

				if (p.ProxyCustomCredentials) {

					if (!StringHelper.EmptyOrNull(p.ProxyUser)) {
							
						proxy.Credentials = NewsHandler.CreateCredentialsFrom(p.ProxyUser, p.ProxyPassword);

						#region experimental
						//CredentialCache credCache = new CredentialCache();
						//credCache.Add(proxy.Address, "Basic", credentials);
						//credCache.Add(proxy.Address, "Digest", credentials);
						//proxy.Credentials = credCache;
						#endregion
					}
				}

			} /* endif UseProxy */ else
			
			if (p.UseIEProxySettings) {

				try {

					IWebProxy ieProxy = AutomaticProxy.GetProxyFromIESettings();	
					
					if (ieProxy != null) {
						proxy = ieProxy;
					}

				} catch (Exception ex){
					// happens on systems with IE release below 5.5
					_log.Error("Apply Preferences.UseIEProxySettings caused exception", ex);
					p.UseIEProxySettings = false;
				}

			} /* endif UseIEProxySettings */

			return proxy;
		
		}

		#endregion

		#region save/load SearchFolders

		public FinderSearchNodes FindersSearchRoot{	
			get {return this.findersSearchRoot;}
			set {this.findersSearchRoot = value;}
		}

		// For backward compatibility (read previous defined search folder settings)
		// we read the old defs. from GuiSettings 
		// If we get something from there, we save it immediatly to the new file/location used for
		// search folders, then removing the entry from settings.
		// If we did not found something in settings, we test for the saerch folder defs. file and read it.
		public FinderSearchNodes LoadSearchFolders() {
			
			FinderSearchNodes fsn = null;
			bool needs2saveNew = false;
			
			string s = this.GuiSettings.GetString("FinderNodes", null);

			if (s == null) {	// no old defs found. Read from search folder file
				
				if (File.Exists(RssBanditApplication.GetSearchFolderFileName())) {
					using (Stream stream = FileHelper.OpenForRead(RssBanditApplication.GetSearchFolderFileName())) {
						try {
							XmlSerializer ser = new XmlSerializer(typeof(FinderSearchNodes));
							fsn = (FinderSearchNodes) ser.Deserialize(stream);
						} catch (Exception ex) {
							_log.Error("LoadSearchFolders::Load Exception (reading/deserialize file).", ex);						
						}
					}
				}
			} else {
				try {
					XmlSerializer ser = new XmlSerializer(typeof(FinderSearchNodes));
					fsn = (FinderSearchNodes) ser.Deserialize(new StringReader(s));
					needs2saveNew = true;
				} catch (Exception ex) {
					_log.Error("LoadSearchFolders::Load Exception (reading/deserialize string.", ex);					
				}
			}

			if(fsn == null){ //exception occured or xsi:nil = true in searchfolders.xml
			  fsn = new FinderSearchNodes(); 
			}

			if (needs2saveNew) {
				this.SaveSearchFolders();	// save to new file
				this.GuiSettings.SetProperty("FinderNodes", null); // remove from .settings.xml
			}


			return fsn;
		}

		public void SaveSearchFolders() {
			using (MemoryStream stream = new MemoryStream()) {
				try {
					XmlSerializer ser = new XmlSerializer(typeof(FinderSearchNodes));
					ser.Serialize(stream, this.findersSearchRoot);
					if (FileHelper.WriteStreamWithBackup( RssBanditApplication.GetSearchFolderFileName(), stream)) {
						// OK
					}
				} catch (Exception ex) {
					_log.Error("SaveSearchFolders::Save Exception.", ex);
				}
			}
		}

		#endregion

		#region Manage trusted CertificateIssues

		public void AddTrustedCertificateIssue(string site, CertificateIssue issue) {
			
			lock (AsyncWebRequest.TrustedCertificateIssues.SyncRoot) {
				IList issues = null;
				if (AsyncWebRequest.TrustedCertificateIssues.ContainsKey(site)) {
					issues = (IList)AsyncWebRequest.TrustedCertificateIssues[site];
					AsyncWebRequest.TrustedCertificateIssues.Remove(site);
				}
				if (issues == null)
					issues = new ArrayList(1);

				if (!issues.Contains(issue))
					issues.Add(issue);

				AsyncWebRequest.TrustedCertificateIssues.Add(site, issues);
			}

			this.trustedCertIssuesModified = true;
		}

		internal void LoadTrustedCertificateIssues() {
			
			if (File.Exists(RssBanditApplication.GetTrustedCertIssuesFileName())) {
				
				lock(AsyncWebRequest.TrustedCertificateIssues.SyncRoot) {
					AsyncWebRequest.TrustedCertificateIssues.Clear();
				}

				using (Stream stream = FileHelper.OpenForRead(RssBanditApplication.GetTrustedCertIssuesFileName())) {

					System.Xml.XPath.XPathDocument doc = new System.Xml.XPath.XPathDocument(stream);
					System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
					
					try {
						System.Xml.XPath.XPathNodeIterator siteIssues = nav.Select("descendant::issues");

						while (siteIssues.MoveNext()){
	
							string url =  siteIssues.Current.GetAttribute("site", String.Empty);
							if (url == null) { continue; }

							ArrayList issues = new ArrayList();

							System.Xml.XPath.XPathNodeIterator theIssues = siteIssues.Current.Select("issue");
							while (theIssues.MoveNext()) {
								if (theIssues.Current.IsEmptyElement) { continue; }
								string issue = theIssues.Current.Value;
								try  {
									CertificateIssue ci = (CertificateIssue)Enum.Parse(typeof(CertificateIssue), issue);
									issues.Add(ci);
								} catch { /* ignore parse errors */ }
							}

							if (issues.Count > 0) {
								lock(AsyncWebRequest.TrustedCertificateIssues.SyncRoot) {
									AsyncWebRequest.TrustedCertificateIssues.Add(url, issues);
								}
							}
						}
                
					}
					catch (Exception e) {
						// Report exception.
						_log.Debug("LoadTrustedCertificateIssues: There was an error while deserializing from Settings Storage.  Ignoring.");
						_log.Debug("LoadTrustedCertificateIssues: The exception was:", e);
					}
				}
			}
		}		

		internal void SaveTrustedCertificateIssues() {
			using (MemoryStream stream = new MemoryStream()) {

				XmlTextWriter writer = null;
				try {
					writer = new XmlTextWriter(stream , null);
					// Use indentation for readability.
					writer.Formatting = Formatting.Indented;
					writer.Indentation = 2;
					writer.WriteStartDocument(true);
					writer.WriteStartElement("trustedCertificateIssues");

					lock (AsyncWebRequest.TrustedCertificateIssues.SyncRoot) {
						foreach (string url in AsyncWebRequest.TrustedCertificateIssues.Keys) {
							ICollection trusted = (ICollection)AsyncWebRequest.TrustedCertificateIssues[url];
							if (trusted != null && trusted.Count > 0) {
								writer.WriteStartElement("issues");
								writer.WriteAttributeString("site", url);
								foreach (CertificateIssue issue in trusted) {
									writer.WriteStartElement("issue");
									writer.WriteString(issue.ToString());
									writer.WriteEndElement();	//issue
								}
								writer.WriteEndElement();	//issues
							}
						}
					}

					writer.WriteEndElement();	//trustedCertificateIssues
					writer.WriteEndDocument();
					writer.Flush();

					try {
						if (FileHelper.WriteStreamWithBackup(RssBanditApplication.GetTrustedCertIssuesFileName(), stream)) {
							// success
							this.trustedCertIssuesModified = false;
						}
					} catch (Exception ex) {
						_log.Error("SaveTrustedCertificateIssues() failed.", ex);
					}

				} catch (Exception e) {
					// Release all resources held by the XmlTextWriter.
					if (writer != null)
						writer.Close();
                
					// Report exception.
					_log.Debug("SaveTrustedCertificateIssues: There was an error while serializing to Storage.  Ignoring.");
					_log.Debug("SaveTrustedCertificateIssues: The exception was:", e);
				}

			}

		}

		#endregion

		#region Manage FeedColumnLayouts

		private void ValidateGlobalFeedColumnLayout()
		{
			if (!feedHandler.FeedsListOK)
				return;

			if (defaultFeedColumnLayoutKey == null){
				foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts){
					if (e.Value.LayoutType == LayoutType.GlobalFeedLayout){
						defaultFeedColumnLayoutKey = e.Key;
						break;
					}
				}
				if (defaultFeedColumnLayoutKey == null){
					defaultFeedColumnLayoutKey = Guid.NewGuid().ToString("N");
					feedHandler.ColumnLayouts.Add(defaultFeedColumnLayoutKey, DefaultFeedColumnLayout);
					feedlistModified = true;
				}
			}
		}
		private void ValidateGlobalCategoryColumnLayout() {
			if (!feedHandler.FeedsListOK)
				return;

			if (defaultCategoryColumnLayoutKey == null){
				foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts){
					if (e.Value.LayoutType == LayoutType.GlobalCategoryLayout){
						defaultCategoryColumnLayoutKey = e.Key;
						break;
					}
				}
				if (defaultCategoryColumnLayoutKey == null){
					defaultCategoryColumnLayoutKey = Guid.NewGuid().ToString("N");
					feedHandler.ColumnLayouts.Add(defaultCategoryColumnLayoutKey, DefaultCategoryColumnLayout);
					feedlistModified = true;
				}
			}
		}
		private void ValidateGlobalSearchFolderColumnLayout() {
			if (!feedHandler.FeedsListOK)
				return;

			if (defaultSearchFolderColumnLayoutKey == null){
				foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts){
					if (e.Value.LayoutType == LayoutType.SearchFolderLayout){
						defaultSearchFolderColumnLayoutKey = e.Key;
						break;
					}
				}
				if (defaultSearchFolderColumnLayoutKey == null){
					defaultSearchFolderColumnLayoutKey = Guid.NewGuid().ToString("N");
					feedHandler.ColumnLayouts.Add(defaultSearchFolderColumnLayoutKey, DefaultSearchFolderColumnLayout);
					feedlistModified = true;
				}
			}
		}
		private void ValidateGlobalSpecialFolderColumnLayout() {
			if (!feedHandler.FeedsListOK)
				return;

			if (defaultSpecialFolderColumnLayoutKey == null){
				foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts){
					if (e.Value.LayoutType == LayoutType.SpecialFeedsLayout){
						defaultSpecialFolderColumnLayoutKey = e.Key;
						break;
					}
				}
				if (defaultSpecialFolderColumnLayoutKey == null){
					defaultSpecialFolderColumnLayoutKey = Guid.NewGuid().ToString("N");
					feedHandler.ColumnLayouts.Add(defaultSpecialFolderColumnLayoutKey, DefaultSpecialFolderColumnLayout);
					feedlistModified = true;
				}
			}
		}

		private void RemoveSimilarColumnLayouts(FeedColumnLayout layout) {
			if (layout == null) return;
			for (int i = 0; i < feedHandler.ColumnLayouts.Count; i++) {
				if (feedHandler.ColumnLayouts[i].Value.LayoutType == LayoutType.IndividualLayout) {
					if (feedHandler.ColumnLayouts[i].Value.Equals(layout, true)) {
						feedHandler.ColumnLayouts.RemoveAt(i);
						i--;
					}
				}
			}
		}

		public FeedColumnLayout GlobalFeedColumnLayout {
			get{
				ValidateGlobalFeedColumnLayout();
				return feedHandler.ColumnLayouts.GetByKey(defaultFeedColumnLayoutKey);
			} 
			set{
				ValidateGlobalFeedColumnLayout();
				if (value == null) return;
				value.LayoutType = LayoutType.GlobalFeedLayout;
				int index = feedHandler.ColumnLayouts.IndexOfKey(defaultFeedColumnLayoutKey);
				if ((index == -1) || (!feedHandler.ColumnLayouts[index].Value.Equals(value))) {
					feedHandler.ColumnLayouts[index] = new FeedColumnLayoutEntry(defaultFeedColumnLayoutKey, value);
					RemoveSimilarColumnLayouts(value);
					feedlistModified = true;
				}
			}
		}

		public FeedColumnLayout GlobalCategoryColumnLayout {
			get{
				ValidateGlobalCategoryColumnLayout();
				return feedHandler.ColumnLayouts.GetByKey(defaultCategoryColumnLayoutKey);
			} 
			set{
				ValidateGlobalCategoryColumnLayout();
				if (value == null) return;
				value.LayoutType = LayoutType.GlobalCategoryLayout;
				int index = feedHandler.ColumnLayouts.IndexOfKey(defaultCategoryColumnLayoutKey);
				if (!feedHandler.ColumnLayouts[index].Value.Equals(value)) {
					feedHandler.ColumnLayouts[index] = new FeedColumnLayoutEntry(defaultCategoryColumnLayoutKey, value);
					RemoveSimilarColumnLayouts(value);
					feedlistModified = true;
				}
			}
		}

		public FeedColumnLayout GlobalSearchFolderColumnLayout {
			get{
				ValidateGlobalSearchFolderColumnLayout();
				return feedHandler.ColumnLayouts.GetByKey(defaultSearchFolderColumnLayoutKey);
			} 
			set{
				ValidateGlobalSearchFolderColumnLayout();
				if (value == null) return;
				value.LayoutType = LayoutType.SearchFolderLayout;
				int index = feedHandler.ColumnLayouts.IndexOfKey(defaultSearchFolderColumnLayoutKey);
				if (!feedHandler.ColumnLayouts[index].Value.Equals(value)) {
					feedHandler.ColumnLayouts[index] = new FeedColumnLayoutEntry(defaultSearchFolderColumnLayoutKey, value);
					feedlistModified = true;
				}
			}
		}

		public FeedColumnLayout GlobalSpecialFolderColumnLayout {
			get{
				ValidateGlobalSpecialFolderColumnLayout();
				return feedHandler.ColumnLayouts.GetByKey(defaultSpecialFolderColumnLayoutKey);
			} 
			set{
				ValidateGlobalSpecialFolderColumnLayout();
				if (value == null) return;
				value.LayoutType = LayoutType.SpecialFeedsLayout;
				int index = feedHandler.ColumnLayouts.IndexOfKey(defaultSpecialFolderColumnLayoutKey);
				if (!feedHandler.ColumnLayouts[index].Value.Equals(value)) {
					feedHandler.ColumnLayouts[index] = new FeedColumnLayoutEntry(defaultSpecialFolderColumnLayoutKey, value);
					feedlistModified = true;
				}
			}
		}

		/// <summary>
		/// Returns the individual FeedColumnLayout for a feed, or
		/// the global one.
		/// </summary>
		/// <param name="feedUrl"></param>
		/// <returns>FeedColumnLayout</returns>
		public FeedColumnLayout GetFeedColumnLayout(string feedUrl) {
			string layout = null;
			layout = feedHandler.GetFeedColumnLayout(feedUrl);

			if (StringHelper.EmptyOrNull(layout)) 
				return GlobalFeedColumnLayout;
			else if (feedHandler.ColumnLayouts.ContainsKey(layout))
				return feedHandler.ColumnLayouts.GetByKey(layout);
			else { // invalid key: cleanup
				feedHandler.SetFeedColumnLayout(feedUrl, null);
				feedlistModified = true;
				return GlobalFeedColumnLayout;
			}
		}

		public void SetFeedColumnLayout(string feedUrl, FeedColumnLayout layout) {
			if (StringHelper.EmptyOrNull(feedUrl))
				return;
			
			if (layout == null) {	// reset
				feedHandler.SetFeedColumnLayout(feedUrl, null);
				feedlistModified = true;
				return;
			}

			if (layout.LayoutType != LayoutType.IndividualLayout &&  layout.LayoutType != LayoutType.GlobalFeedLayout)
				return;	// not a layout format we have to store for a feed

			string key = feedHandler.GetFeedColumnLayout(feedUrl);
			FeedColumnLayout global = GlobalFeedColumnLayout;

			if (StringHelper.EmptyOrNull(key) || false == feedHandler.ColumnLayouts.ContainsKey(key)) {
				if (!layout.Equals(global, true)){
					int known = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
					if (known >= 0){
						feedHandler.SetFeedColumnLayout(feedUrl, feedHandler.ColumnLayouts.GetKey(known));
					} else {
						key = Guid.NewGuid().ToString("N");
						feedHandler.ColumnLayouts.Add(key, layout);
						feedHandler.SetFeedColumnLayout(feedUrl, key);
					}
					feedlistModified = true;
				} else {	// similar to global: store there
					GlobalFeedColumnLayout = layout;
				}
			} else {
				int known = feedHandler.ColumnLayouts.IndexOfKey(key);
				if (!feedHandler.ColumnLayouts[known].Value.Equals(layout)){	// modified
					if (!feedHandler.ColumnLayouts[known].Value.Equals(layout, true)){	// not anymore similar
						feedHandler.SetFeedColumnLayout(feedUrl, null);
						feedHandler.ColumnLayouts.RemoveAt(known);
						if (!layout.Equals(global, true)){
							int otherKnownSimilar = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
							if (otherKnownSimilar >= 0){
								feedHandler.ColumnLayouts[otherKnownSimilar] = new FeedColumnLayoutEntry(key, layout);	// refresh layout info
								feedHandler.SetFeedColumnLayout(feedUrl, feedHandler.ColumnLayouts.GetKey(otherKnownSimilar));	// set new key
							} else{
								key = Guid.NewGuid().ToString("N");
								feedHandler.ColumnLayouts.Add(key, layout);
								feedHandler.SetFeedColumnLayout(feedUrl, key);
							}
						}
					} else {	// still similar:
						feedHandler.ColumnLayouts[known]= new FeedColumnLayoutEntry(key, layout);	// refresh layout info
					}
					feedlistModified = true;
				}
			}

		}

		/// <summary>
		/// Returns the FeedColumnLayout
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		public FeedColumnLayout GetCategoryColumnLayout(string category) {
			string layout = null;
			layout = feedHandler.GetCategoryFeedColumnLayout(category);

			if (StringHelper.EmptyOrNull(layout)) 
				return GlobalCategoryColumnLayout;
			else if (feedHandler.ColumnLayouts.ContainsKey(layout))
				return feedHandler.ColumnLayouts.GetByKey(layout);
			else { // invalid key: cleanup
				feedHandler.SetCategoryFeedColumnLayout(category, null);
				feedlistModified = true;
				return GlobalCategoryColumnLayout;
			}
		}

		public void SetCategoryColumnLayout(string category, FeedColumnLayout layout) {
			if (StringHelper.EmptyOrNull(category))
				return;
			
			if (layout == null) {	// reset
				feedHandler.SetCategoryFeedColumnLayout(category, null);
				feedlistModified = true;
				return;
			}

			if (layout.LayoutType != LayoutType.IndividualLayout && layout.LayoutType != LayoutType.GlobalCategoryLayout)
				return;	// not a layout format we have to store for a category

			string key = feedHandler.GetCategoryFeedColumnLayout(category);
			FeedColumnLayout global = GlobalCategoryColumnLayout;

			if (StringHelper.EmptyOrNull(key) || false == feedHandler.ColumnLayouts.ContainsKey(key)) {
				if (!layout.Equals(global, true)){
					int known = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
					if (known >= 0){
						feedHandler.SetCategoryFeedColumnLayout(category, feedHandler.ColumnLayouts.GetKey(known));
					} else {
						key = Guid.NewGuid().ToString("N");
						feedHandler.ColumnLayouts.Add(key, layout);
						feedHandler.SetCategoryFeedColumnLayout(category, key);
					}
					feedlistModified = true;
				} else {
					GlobalCategoryColumnLayout = layout;
				}
			} else {
				int known = feedHandler.ColumnLayouts.IndexOfKey(key);
				if (!feedHandler.ColumnLayouts[known].Value.Equals(layout)){	// modified
					if (!feedHandler.ColumnLayouts[known].Value.Equals(layout, true)){	// not anymore similar
						feedHandler.SetCategoryFeedColumnLayout(category, null);
						feedHandler.ColumnLayouts.RemoveAt(known);
						if (!layout.Equals(global, true)){
							int otherKnownSimilar = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
							if (otherKnownSimilar >= 0){
								feedHandler.ColumnLayouts[otherKnownSimilar] = new FeedColumnLayoutEntry(key, layout);	// refresh layout info
								feedHandler.SetCategoryFeedColumnLayout(category, feedHandler.ColumnLayouts.GetKey(otherKnownSimilar));	// set new key
							} else{
								key = Guid.NewGuid().ToString("N");
								feedHandler.ColumnLayouts.Add(key, layout);
								feedHandler.SetCategoryFeedColumnLayout(category, key);
							}
						}
					} else {	// still similar:
						feedHandler.ColumnLayouts[known]= new FeedColumnLayoutEntry(key, layout);	// refresh layout info
					}
					feedlistModified = true;
				}
			}

		}

		#endregion

		/// <summary>
		/// Load the feedlist. To have exception handling and UI feedback,
		/// please call StartLoadingFeedlist().
		/// </summary>
		/// <exception cref="BanditApplicationException">On any failure</exception>
		internal void LoadFeedList() {

			//Load new feed file if exists 
			string p = RssBanditApplication.GetFeedListFileName();
			string pOld = Path.Combine(GetUserPath(), "feedlist.xml");

			if(!File.Exists(p) && File.Exists(pOld)) {
				if (this.MessageQuestion("RES_UpgradeFeedlistInfoText", RssBanditApplication.Caption) == DialogResult.No) {
					throw new BanditApplicationException(ApplicationExceptions.FeedlistOldFormat);
				}
				File.Copy(pOld , p); // copy to be used to load from
			}

			if(File.Exists(p)) {
				
				try {
					feedHandler.LoadFeedlist(p, new System.Xml.Schema.ValidationEventHandler(FeedListValidationCallback));
				}
				catch (Exception e) {
					if (!validationErrorOccured) {	// set by validation callback handler
						_log.Error("Exception on loading '"+p+"'.", e);
						throw new BanditApplicationException(ApplicationExceptions.FeedlistOnRead, e);
					}
				}
			
				if(feedHandler.FeedsListOK && guiMain != null) {		
					/* All right here... 	*/ 
				}
				else if(validationErrorOccured) {				
					validationErrorOccured = false;  
					throw new BanditApplicationException(ApplicationExceptions.FeedlistOnProcessContent);
				}
				else {
					throw new BanditApplicationException(ApplicationExceptions.FeedlistNA);
				}
			
			} else {
				// no feedlist file exists:
				throw new BanditApplicationException(ApplicationExceptions.FeedlistNA);
			}

		}

		public void ImportFeeds(string fromFileOrUrl) {
			this.ImportFeeds(fromFileOrUrl, String.Empty);
		}

		public void ImportFeeds(string fromFileOrUrl, string selectedCategory) {

			ImportFeedsDialog dialog = new ImportFeedsDialog(fromFileOrUrl, selectedCategory, defaultCategory, feedHandler.Categories);
			try {
				dialog.ShowDialog(guiMain);
			} catch {}

			Application.DoEvents();		// give time to visualize dismiss the dialog andredraw the UI

			Stream myStream = null;

			if(dialog.DialogResult == DialogResult.OK) {
				string s = dialog.FeedsUrlOrFile;
				string cat = (dialog.FeedCategory == null ? String.Empty: dialog.FeedCategory);
				if (!StringHelper.EmptyOrNull(s)) {
					if (File.Exists(s)) {
						using (myStream = FileHelper.OpenForRead(s)) {
							try { 
								this.feedHandler.ImportFeedlist(myStream,cat); 
								this.FeedlistModified = true;
							}
							catch(Exception ex) {
								this.MessageError("RES_ExceptionImportFeedlist", s, ex.Message);
								return; 
							}
							guiMain.InitiatePopulateTreeFeeds();
						}					
					} else {
						Uri uri = null;
						try {
							uri = new Uri(s);
						} catch {}
						if (uri != null) {

							HttpRequestFileThreadHandler fileHandler = new HttpRequestFileThreadHandler(uri.ToString(), this.feedHandler.Proxy);
							DialogResult result = fileHandler.Start( guiMain, Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageRequestFile", uri.ToString()));
							
							if (result != DialogResult.OK)
								return;

							if (!fileHandler.OperationSucceeds) {
								this.MessageError("RES_WebExceptionOnUrlAccess", 
									uri.ToString(), fileHandler.OperationException.Message); 
								return;
							}
                    
							myStream = fileHandler.ResponseStream;
							if (myStream != null) {
								using (myStream) {
									try { 
										this.feedHandler.ImportFeedlist(myStream, cat); 
										this.FeedlistModified = true;
									}
									catch(Exception ex) {
										this.MessageError("RES_ExceptionImportFeedlist", s, ex.Message);
										return; 
									}
									guiMain.InitiatePopulateTreeFeeds();
								}					
							}
						}
					}
					
				}
			}
			
		}

		/// <summary>
		/// Disable a feed (with UI update)
		/// </summary>
		/// <param name="feedUrl">string</param>
		public void DisableFeed(string feedUrl) {
			if (feedUrl != null) {
				this.FeedHandler.DisableFeed(feedUrl);
				FeedTreeNodeBase n = guiMain.GetTreeNodeForItem(guiMain.GetRoot(RootFolderType.MyFeeds), feedUrl);
				if (n != null) {
					n.ImageIndex = n.SelectedImageIndex = 5;			
				}
			}
		}

		/// <summary>
		/// UI thread save navigation to an Url.
		/// </summary>
		/// <param name="url">Url to navigate to</param>
		/// <param name="tabCaption">The suggested tab caption (maybe replaced by the url's html page title)</param>
		/// <param name="forceNewTabOrWindow">Force to open a new Tab/Window</param>
		/// <param name="setFocus">Force to set the focus to the new Tab/Window</param>
		public void NavigateToUrl(string url, string tabCaption, bool forceNewTabOrWindow, bool setFocus) {
			if (guiMain.InvokeRequired) {
				WinGuiMain.NavigateToURLDelegate d = new WinGuiMain.NavigateToURLDelegate(guiMain.DetailTabNavigateToUrl);
				guiMain.Invoke(d, new object[]{url, tabCaption, forceNewTabOrWindow, setFocus});
			} else {
				guiMain.DetailTabNavigateToUrl(url, tabCaption, forceNewTabOrWindow, setFocus);
			}
		}

		public void PreferredNavigateToUrl(string url, string tabCaption, bool forceNewTabOrWindow, bool setFocus) {
			if (BrowserBehaviorOnNewWindow.OpenNewTab == Preferences.BrowserOnNewWindow)  {
				NavigateToUrl(url, tabCaption, forceNewTabOrWindow, setFocus);
			} else if (BrowserBehaviorOnNewWindow.OpenDefaultBrowser == Preferences.BrowserOnNewWindow) {
				OpenUrlInExternalBrowser(url);
			}
		}

		/// <summary>
		/// Open a link in the default System Browser
		/// </summary>
		/// <param name="url">string</param>
		public void OpenUrlInExternalBrowser(string url) {
			if (StringHelper.EmptyOrNull(url))
				url = "about:blank";
			try {
				Process.Start(url);
			} catch (Exception  ex) {
				if (this.MessageQuestion("RES_ExceptionStartDefaultBrowserMessage", ex.Message, url) == DialogResult.Yes) {
					this.NavigateToUrl(url, "Web", true, true);
				} 
			}
		}


		/// <summary>
		/// Removes a NewsItem from a SmartFolder.
		/// </summary>
		/// <param name="folder"></param>
		/// <param name="item"></param>
		public void RemoveItemFromSmartFolder(ISmartFolder folder, NewsItem item){
			
			if (folder == null || item == null)
				return;

			if (folder is FlaggedItemsNode) {

				try{	// remove the flagged state from original item
				
					XmlQualifiedName key = RssHelper.GetOptionalElementKey(item.OptionalElements, flaggedItemOriginalFeedQName); 
					XmlElement elem  = null;

					//find bandit:feed-url element 
					if (null != key)
						elem  = (XmlElement) item.OptionalElements[key];

					if(elem != null){
							
						string feedUrl = elem.InnerText; 

						if(this.feedHandler.FeedsTable.Contains(feedUrl)){ //check if feed exists 

							ArrayList itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

							//find this item 
							int itemIndex = itemsForFeed.IndexOf(item); 


							if(itemIndex != -1){ //check if item still exists 

								NewsItem foundItem = (NewsItem) itemsForFeed[itemIndex]; 									
								foundItem.FlagStatus = Flagged.None; 
							}

						}//if(this.feedHan...)
							
							
					}//if(elem.Equals...)						

				}catch(Exception e) { 
					_log.Error("RemoveItemFromSmartFolder() exception", e);
				}					

				folder.Remove(item);

			} else {	// other SmartFolders

				folder.Remove(item);

			}

		}

		public LocalFeedsFeed FlaggedItemsFeed {
			get { return flaggedItemsFeed; }
			set { flaggedItemsFeed = value; }
		}

		public void ClearFlaggedItems() {

			foreach(NewsItem ri in this.flaggedItemsFeed.Items){
			
				try{
				
					XmlQualifiedName key = RssHelper.GetOptionalElementKey(ri.OptionalElements, flaggedItemOriginalFeedQName); 

					XmlElement elem = null;
					//find bandit:feed-url element 
					if (null != key)
						elem  = (XmlElement) ri.OptionalElements[key];

					if(elem != null){
							
						string feedUrl = elem.InnerText; 

						if(this.feedHandler.FeedsTable.Contains(feedUrl)){ //check if feed exists 

							ArrayList itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

							//find this item 
							int itemIndex = itemsForFeed.IndexOf(ri); 


							if(itemIndex != -1){ //check if item still exists 

								NewsItem item = (NewsItem) itemsForFeed[itemIndex]; 									
								item.FlagStatus = Flagged.None; 
								item.OptionalElements.Remove(flaggedItemOriginalFeedQName); 
								break; 									
							}

						}//if(this.feedHan...)
							
							
					}//if(elem.Equals...)						

				}catch(Exception e) { 
					_log.Error("ClearFlaggedItems() exception", e);
				}					
			
			}//foreach(NewsItem ri...)

			this.flaggedItemsFeed.Items.Clear();
		}

		/// <summary>
		/// Get a NewsItem to re-flag: usually called on a already flagged item. It try to
		/// find the corresponding feed containing the item and re-flag them.
		/// </summary>
		/// <param name="theItem">NewsItem to re-flag</param>
		public void ReFlagNewsItem(NewsItem theItem) {

			if (theItem == null) 
				return; 

			string feedUrl = null;	// the corresponding feed Url

			try{
				XmlElement elem = RssHelper.GetOptionalElement(theItem, flaggedItemOriginalFeedQName);
				if (elem != null) {
					feedUrl = elem.InnerText;
				}
			} catch {}

			if (theItem.FlagStatus == Flagged.None || theItem.FlagStatus == Flagged.Complete) {
				// remove from collection
				if (this.flaggedItemsFeed.Items.Contains(theItem)) {
					this.flaggedItemsFeed.Items.Remove(theItem);
				}
			
			} else {

				//find this item 
				int itemIndex = this.flaggedItemsFeed.Items.IndexOf(theItem); 

				if(itemIndex != -1){ //check if item exists 

					NewsItem item = (NewsItem)this.flaggedItemsFeed.Items[itemIndex];
					item.FlagStatus = theItem.FlagStatus; 
				}

			}

			if(feedUrl != null && this.feedHandler.FeedsTable.Contains(feedUrl)){ //check if feed exists 

				ArrayList itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

				//find this item 
				int itemIndex = itemsForFeed.IndexOf(theItem); 

				if(itemIndex != -1){ //check if item still exists 

					NewsItem item = (NewsItem) itemsForFeed[itemIndex]; 									
					item.FlagStatus = theItem.FlagStatus; 

					this.FeedWasModified(feedUrl);

				}

			}//if(this.feedHan...)

			this.flaggedItemsFeed.Modified = true;
							
		}

		
		/// <summary>
		/// Get a NewsItem to flag and add them (Clone) to the flagged item node collection
		/// </summary>
		/// <param name="theItem">NewsItem to flag</param>
		public void FlagNewsItem(NewsItem theItem) {
			 
			if (theItem == null) 
				return; 

			if (theItem.FlagStatus == Flagged.None || theItem.FlagStatus == Flagged.Complete) {
				// remove
				if (this.flaggedItemsFeed.Items.Contains(theItem)) {
					this.flaggedItemsFeed.Items.Remove(theItem);
				}
			
			} else {

				if (!this.flaggedItemsFeed.Items.Contains(theItem)) {
					// new flagged item
					NewsItem flagItem = theItem.CopyTo(flaggedItemsFeed);

					//take over flag status
					flagItem.FlagStatus = theItem.FlagStatus; 

					if (null == RssHelper.GetOptionalElementKey(flagItem.OptionalElements, flaggedItemOriginalFeedQName)){
						XmlElement originalFeed = RssHelper.CreateXmlElement("bandit", flaggedItemOriginalFeedQName.Name, flaggedItemOriginalFeedQName.Namespace, theItem.Feed.link); 
						flagItem.OptionalElements.Add(flaggedItemOriginalFeedQName, originalFeed);
					}
				
					flagItem.BeenRead = theItem.BeenRead;
					this.flaggedItemsFeed.Add(flagItem);
				
				} else {
					// re-flag:
					//find this item 
					int itemIndex = this.flaggedItemsFeed.Items.IndexOf(theItem); 
					if(itemIndex != -1){ //check if item still exists 
						NewsItem flagItem = (NewsItem)this.flaggedItemsFeed.Items[itemIndex];
						flagItem.FlagStatus = theItem.FlagStatus;
					}
				}
			}

			this.flaggedItemsFeed.Modified = true;
			if (!StringHelper.EmptyOrNull(theItem.Feed.link)) {
				this.FeedWasModified(theItem.Feed.link);
			}
		}
		
		public LocalFeedsFeed SentItemsFeed {
			get { return sentItemsFeed; }
			set { sentItemsFeed = value; }
		}

		public void AddSentNewsItem(NewsItem theItem, NewsItem replyItem) {
			 
			if (theItem == null || replyItem == null)
				return;

				
			theItem.FlagStatus = Flagged.Reply; 

			// create a new one, because we could not modify the replyItem.link :(
			NewsItem newItem = new NewsItem(this.sentItemsFeed, replyItem.Title, theItem.Link, replyItem.Content, replyItem.Date, theItem.Feed.title); 
			newItem.OptionalElements = (Hashtable) replyItem.OptionalElements.Clone(); 

			if (null == RssHelper.GetOptionalElementKey(newItem.OptionalElements, flaggedItemOriginalFeedQName)){
				XmlElement originalFeed = RssHelper.CreateXmlElement("bandit", flaggedItemOriginalFeedQName.Name, flaggedItemOriginalFeedQName.Namespace, theItem.Feed.link); 
				newItem.OptionalElements.Add(flaggedItemOriginalFeedQName, originalFeed);
			}
			
			newItem.BeenRead = false;
			this.sentItemsFeed.Add(newItem);
			
			guiMain.SentItemsNode.UpdateReadStatus();

		}


		/// <summary>
		/// Get/Sets the deleted items local feed
		/// </summary>
		public LocalFeedsFeed DeletedItemsFeed {
			get { return deletedItemsFeed; }
			set { deletedItemsFeed = value; }
		}

		/// <summary>
		/// Gets a NewsItem to delete and add them to the deleted items feed
		/// </summary>
		/// <param name="theItem">NewsItem to delete</param>
		public void DeleteNewsItem(NewsItem theItem) {
			 
			if (theItem == null) 
				return; 

			// remove from flagged local feed (there are copies of NewsItems)
			if (this.flaggedItemsFeed.Items.Contains(theItem)) {
				this.flaggedItemsFeed.Items.Remove(theItem);
			}
						
			
			// add a optional element to remember the original feed container (for later restore)
			if (null != theItem.Feed && null == RssHelper.GetOptionalElementKey(theItem.OptionalElements, deletedItemContainerFeedQName)) {
				XmlElement originalFeed = RssHelper.CreateXmlElement("bandit", deletedItemContainerFeedQName.Name, deletedItemContainerFeedQName.Namespace, theItem.Feed.link); 
				theItem.OptionalElements.Add(deletedItemContainerFeedQName, originalFeed);
			}

			bool yetDeleted = false;
			if (!this.deletedItemsFeed.Items.Contains(theItem)) {
				// add new deleted item
				this.deletedItemsFeed.Add(theItem);
				yetDeleted = true;
			} 

			this.feedHandler.DeleteItem(theItem);

			this.deletedItemsFeed.Modified = true;
			if (!StringHelper.EmptyOrNull(theItem.Feed.link)) {
				this.FeedWasModified(theItem.Feed.link);
			}

			// remove from deleted local feed (if already there or deleted directly from the 
			// node container 'Waste basket' itself)
			if (!yetDeleted && this.deletedItemsFeed.Items.Contains(theItem)) {
				this.deletedItemsFeed.Items.Remove(theItem);		
			}
		}
		
		/// <summary>
		/// Gets a NewsItem and restore it. It will be removed from the deleted items feed
		/// and added back to the original container feed. 
		/// It returns the original container tree node if found and restored, else null.
		/// </summary>
		/// <param name="item">NewsItem</param>
		/// <returns>FeedTreeNodeBase</returns>
		public FeedTreeNodeBase RestoreNewsItem(NewsItem item) {
		
			if (item == null) 
				return null; 

			string containerFeedUrl = null;
			XmlElement elem = RssHelper.GetOptionalElement(item, deletedItemContainerFeedQName);
			if (null != elem) {
				containerFeedUrl = elem.InnerText;
				item.OptionalElements.Remove(deletedItemContainerFeedQName);
			}

			if (StringHelper.EmptyOrNull(containerFeedUrl)) {
				containerFeedUrl = item.Feed.link;
			}

			if (StringHelper.EmptyOrNull(containerFeedUrl)) {
				_log.Error("Cannot restore item: feed link missing.");
				return null;
			}

			bool foundAndRestored = false;
			FeedTreeNodeBase node = null;

				
			if (null != RssHelper.GetOptionalElementKey(item.OptionalElements, flaggedItemOriginalFeedQName)) {
			
				// it was a flagged item
				this.flaggedItemsFeed.Add(item);
				node = (FeedTreeNodeBase)guiMain.FlaggedFeedsNode(item.FlagStatus);
				foundAndRestored = true;
			
			} else if (this.FeedHandler.FeedsTable.Contains(containerFeedUrl)) {

				this.FeedHandler.RestoreDeletedItem(item);
				node = guiMain.GetTreeNodeForItem(guiMain.GetRoot(RootFolderType.MyFeeds), containerFeedUrl);
				foundAndRestored = true;
			
			} else {

				ISmartFolder isFolder = guiMain.GetTreeNodeForItem(guiMain.GetRoot(RootFolderType.SmartFolders), containerFeedUrl) as ISmartFolder;
				if (null != isFolder) {
					isFolder.Add(item);
					node = (FeedTreeNodeBase)isFolder;
					foundAndRestored = true;
				}

			}

			if (foundAndRestored) {
			
				this.deletedItemsFeed.Remove(item);
				this.deletedItemsFeed.Modified = true;
				this.FeedWasModified(containerFeedUrl);
			
			} else {
				_log.Error("Cannot restore item: container feed not found. Url was '"+containerFeedUrl +"'.");
			}

			return node;
		}

		public static void SetWorkingSet(int lnMaxSize,int lnMinSize) { 

			System.Diagnostics.Process loProcess =  System.Diagnostics.Process.GetCurrentProcess(); 
			loProcess.MaxWorkingSet = (IntPtr) lnMaxSize; 
			loProcess.MinWorkingSet = (IntPtr) lnMinSize; 
			//long lnValue = loProcess.WorkingSet; // see what the actual value 

		} 


		/// <summary>
		/// Publish an XML Feed error.
		/// </summary>
		/// <param name="e">XmlException to publish</param>
		/// <param name="feedLink">The errornous feed url</param>
		/// <param name="updateNodeIcon">Set to true, if you want to get the node icon reflecting the errornous state</param>
		public void PublishXmlFeedError(System.Exception e, string feedLink, bool updateNodeIcon) {
			if (feedLink != null) {
				Uri uri = null;
				try {
					uri = new Uri(feedLink);
				} catch (UriFormatException) {}
				this.UpdateXmlFeedErrorFeed(this.CreateLocalFeedRequestException(e, feedLink), uri, updateNodeIcon);
			}
		}

		/// <summary>
		/// Publish an XML Feed error.
		/// </summary>
		/// <param name="e">XmlException to publish</param>
		/// <param name="f">The errornous feedsFeed</param>
		/// <param name="updateNodeIcon">Set to true, if you want to get the node icon reflecting the errornous state</param>
		public void PublishXmlFeedError(System.Exception e, feedsFeed f, bool updateNodeIcon) {
			if (f != null && !StringHelper.EmptyOrNull(f.link)) {
				this.PublishXmlFeedError(e, f.link, updateNodeIcon);
			}
		}

		/// <summary>
		/// Helper to create a wrapped Exception, that provides more error infos for a feed
		/// </summary>
		/// <param name="e">Exception</param>
		/// <param name="f">feedsFeed</param>
		/// <returns></returns>
		private FeedRequestException CreateLocalFeedRequestException(Exception e, feedsFeed f) {
			return new FeedRequestException(e.Message, e, this.feedHandler.GetFailureContext(f));
		}
		/// <summary>
		/// Helper to create a wrapped Exception, that provides more error infos for a feed
		/// </summary>
		/// <param name="e">Exception</param>
		/// <param name="feedUrl">feed Url</param>
		/// <returns></returns>
		private FeedRequestException CreateLocalFeedRequestException(Exception e, string feedUrl) {
			if (feedUrl != null) {
				Uri uri = null;
				try {
					uri = new Uri(feedUrl);
				} catch (UriFormatException) {}
				return new FeedRequestException(e.Message, e, this.feedHandler.GetFailureContext(uri));
			} else {
				return new FeedRequestException(e.Message, e, new Hashtable());
			}
		}

		/// <summary>
		/// Add the exception to the local feedError feed. 
		/// Rewrite/Recreate the file on demand.
		/// </summary>
		/// <param name="e">An Exception to publish</param>
		/// <param name="resourceUri">The resource Uri that raise the exception.</param>
		/// <param name="updateNodeIcon">Set this to true, if you want to get the icon state reflecting the reported exception</param>
		private void UpdateXmlFeedErrorFeed(System.Exception e, Uri resourceUri, bool updateNodeIcon) {

			if (e != null) {

				ExceptionManager.GetInstance().Add(e);
				
				ResourceGoneException goneex = e as ResourceGoneException;
				
				if (goneex == null) 
					goneex = e.InnerException as ResourceGoneException;

				if (goneex != null) {	
					feedsFeed f = this.feedHandler.FeedsTable[resourceUri];
					if (f != null)
						this.DisableFeed(f.link);
				} else if (updateNodeIcon && resourceUri != null) {
					guiMain.OnFeedUpdateFinishedWithException(resourceUri, e);
				}

				guiMain.ExceptionNode.UpdateReadStatus();
				guiMain.PopulateSmartFolder((FeedTreeNodeBase)guiMain.ExceptionNode, false);
			}
		}

		/// <summary>
		/// Flags the NewsItems in the regular feeds that are currently in the flagItemList.
		/// </summary>
		public void InitializeFlaggedItems(){
		
			// as long the FlagStatus of NewsItem's wasn't persisted all the time, 
			// we have to re-init the feed item's FlagStatus from the flagged items collection:
			bool runSelfHealingFlagStatus = this.GuiSettings.GetBoolean("RunSelfHealing.FlagStatus", true);
			
			foreach(NewsItem ri in this.flaggedItemsFeed.Items){
			
				if (ri.FlagStatus == Flagged.None)	{ // correction: older Bandit versions are not able to store flagStatus
					ri.FlagStatus = Flagged.FollowUp;
					this.flaggedItemsFeed.Modified = true;
				} else {
					if (!runSelfHealingFlagStatus)
						continue;	// done
				}

				// self-healing processing:

				string feedUrl = null;	// the corresponding feed Url

				try{
					XmlElement e = RssHelper.GetOptionalElement(ri, flaggedItemOriginalFeedQName);
					if (e != null) {
						feedUrl = e.InnerText;
					}
				} catch {}

				if(feedUrl != null && this.feedHandler.FeedsTable.Contains(feedUrl)){ //check if feed exists 

					ArrayList itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

					//find this item 
					int itemIndex = itemsForFeed.IndexOf(ri); 


					if(itemIndex != -1){ //check if item still exists 

						NewsItem item = (NewsItem) itemsForFeed[itemIndex]; 									
						if (item.FlagStatus != ri.FlagStatus)	{// correction: older Bandit versions are not able to store flagStatus
							item.FlagStatus = ri.FlagStatus;
							this.flaggedItemsFeed.Modified = true;
							this.FeedWasModified(feedUrl);			// self-healing
						}
						
					}

				}//if(this.feedHan...)
							
			}//foreach(NewsItem ri...)

			if (runSelfHealingFlagStatus) {	// remember the state:
				this.GuiSettings.SetProperty("RunSelfHealing.FlagStatus", false);
			}

		}

		/// <summary>
		/// Called on Application Exit. Close the main form and save application state (RSS Feeds).
		/// </summary>
		/// <param name="e">Event args.</param>
		private void OnApplicationExit(object sender, EventArgs e) {
			if (guiMain != null && !guiMain.IsDisposed) 
				guiMain.Close(true);
			autoSaveTimer.Dispose();

			SaveApplicationState();
			guiMain = null;
		}

		private void OnAppDomainUnload(object sender, EventArgs e) {
			// forward to:
			OnApplicationExit(sender, e);
		}

		private void OnThreadException(object sender, ThreadExceptionEventArgs e) {
			_log.Error("OnThreadException() called", e.Exception);
		}

		private void OnNewsItemTransformationError(object sender, FeedExceptionEventArgs e) {
			this.PublishXmlFeedError(e.FailureException, e.FeedLink, false);
		}

		private void OnNewsItemFormatterStylesheetError(object sender, ExceptionEventArgs e) {
			_log.Error("OnNewsItemFormatterStylesheetError() called", e.FailureException);
			this.MessageError("RES_ExceptionNewsItemFormatterStylesheetMessage", e.ErrorMessage, e.FailureException.Message);
		}
		
		private void OnNewsItemFormatterStylesheetValidationError(object sender, ExceptionEventArgs e) {
			_log.Error("OnNewsItemFormatterStylesheetValidationError() called", e.FailureException);
			this.MessageError("RES_ExceptionNewsItemFormatterStylesheetMessage", e.ErrorMessage, e.FailureException.Message);
		}


		private void OnInternetConnectionStateChanged(INetState oldState, INetState newState) {
			if (guiMain != null && !guiMain.IsDisposed && oldState != newState) {
				guiMain.SetGuiStateINetConnected((newState & INetState.Connected) > 0 && (newState & INetState.Online) > 0);
				guiMain.SetTitleText(null);
				bool offline = ((newState & INetState.Offline) > 0);
				Mediator.SetChecked(offline, "cmdToggleOfflineMode");
				Mediator.SetEnable(!offline, "cmdAutoDiscoverFeed");
			}
			// notify service consumers:
			EventsHelper.Fire(InternetConnectionStateChange, 
				this, new InternetConnectionStateChangeEventArgs(oldState, newState));
		}

		private void OnRssParserBeforeStateChange(NewsHandlerState oldState, NewsHandlerState newState, ref bool cancel){
			// move to idle states
			if (newState == NewsHandlerState.RefreshOneDone) {
				if (oldState >= NewsHandlerState.RefreshCategory) {
					cancel = true;	// not allowed. Only RefreshAllDone can switch to idle
				}
			} else 
				if (newState < NewsHandlerState.RefreshCategory &&
				newState != NewsHandlerState.Idle &&
				oldState >= NewsHandlerState.RefreshCategory) {
				cancel = true; 	// not allowed. RefreshAll or Categories in progress
			}
		}

		private void OnNewsHandlerStateChanged (NewsHandlerState oldState, NewsHandlerState newState) {
			// move to idle states
			if (newState == NewsHandlerState.RefreshOneDone) {
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.Idle);
			} else 
				if (newState == NewsHandlerState.RefreshAllDone) {
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.Idle);
			} else 
				if (newState == NewsHandlerState.Idle) {
				this.SetGuiStateFeedbackText(String.Empty, ApplicationTrayState.NormalIdle);	
			} else
				if (newState == NewsHandlerState.RefreshCategory) {
				this.SetGuiStateFeedback("RES_GUIStatusRefreshFeedsMessage", ApplicationTrayState.BusyRefreshFeeds);
			} else
				if (newState == NewsHandlerState.RefreshOne) {
				this.SetGuiStateFeedback("RES_GUIStatusLoadingFeed", ApplicationTrayState.BusyRefreshFeeds);
			} else
				if (newState == NewsHandlerState.RefreshAllAuto || newState == NewsHandlerState.RefreshAllForced) {
				this.SetGuiStateFeedback("RES_GUIStatusRefreshFeedsMessage", ApplicationTrayState.BusyRefreshFeeds);
			}
		}

		/// <summary>
		/// Called from the autoSaveTimer. It is re-used to probe for a 
		/// valid open internet connection...
		/// </summary>
		/// <param name="theStateObject"></param>
		private void OnAutoSave(object theStateObject) {
			if (guiMain != null && !guiMain.Disposing) {
				guiMain.SaveUIConfiguration(true);
			}
			this.SaveApplicationState();
			this.UpdateInternetConnectionState();
		}


		private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e) {
			if (e.Mode == PowerModes.Resume) {
				
				// re-create a timer that waits three minutes , then invokes every five minutes.
				autoSaveTimer = new System.Threading.Timer(new TimerCallback(this.OnAutoSave), this, 3 * MilliSecsMultiplier, 5 * MilliSecsMultiplier);
				this.UpdateInternetConnectionState(true);											// get current state, takes a few msecs
			
			} else if (e.Mode == PowerModes.Suspend) {
				
				this.OnAutoSave(null);
				
				if (autoSaveTimer != null) {
					autoSaveTimer.Dispose();
					autoSaveTimer = null;
				}

				this.feedHandler.Offline = true;
			}
		}

		public void UpdateInternetConnectionState() {
			this.UpdateInternetConnectionState(false);
		}
		
		public void UpdateInternetConnectionState(bool forceFullTest) {
			
			INetState state = Utils.CurrentINetState(this.Proxy, forceFullTest);
			stateManager.MoveInternetConnectionStateTo(state);					// raises OnInternetConnectionStateChanged() event
			this.feedHandler.Offline = !stateManager.InternetAccessAllowed;		// reset feedHandler
			
		}

		/// <summary>
		/// Saves Application State: the feedlist, changed cached files, search folders, flagged items and sent items
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void SaveApplicationState() {
			
			if (guiMain == null) return;
			
			/* 
			 * we handle the exit error here, because it does not make sense
			 * to provide a "Resume", "Ignore" as the global exception handler
			 * offers on exiting the program
			 * */
			try { 
				if(this.FeedlistModified && this.feedHandler != null && this.feedHandler.FeedsTable != null && 
					this.feedHandler.FeedsListOK ) { 

					using (MemoryStream stream = new MemoryStream()) {
						try {
							feedHandler.SaveFeedList(stream); 
							if (FileHelper.WriteStreamWithBackup( RssBanditApplication.GetFeedListFileName(), stream)) {
								this.FeedlistModified = false;	// reset state flag
							}
						} catch (Exception ex) {
							_log.Error("SaveFeedList() failed.", ex);
						}
					}
				}

				if (this.flaggedItemsFeed.Modified)
					this.flaggedItemsFeed.Save();

				if (this.sentItemsFeed.Modified)
					this.sentItemsFeed.Save();

				if (this.deletedItemsFeed.Modified)
					this.deletedItemsFeed.Save();

				if (this.trustedCertIssuesModified)
					this.SaveTrustedCertificateIssues();

				this.SaveModifiedFeeds();

				//save search folders
				this.SaveSearchFolders();

			}
			catch(InvalidOperationException ioe) {
				RssBanditApplication.PublishException(new BanditApplicationException("Unexpected InvalidOperationException on SaveApplicationState()", ioe)); 				
			}
			catch(Exception ex) {
				RssBanditApplication.PublishException(new BanditApplicationException("Unexpected Exception on SaveApplicationState()", ex)); 				
			}
		}

		/// <summary>
		/// Catch Up the current selected node in subscriptions treeview.
		/// Works for all subscription types (feed, category, all).
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdCatchUpCurrentSelectedNode(ICommand sender) {
			guiMain.MarkSelectedNodeRead(guiMain.CurrentSelectedNode);
			this.FeedlistModified = true;
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// Called only from subscriptions tree on SmartFolder(s)
		/// </summary>
		/// <param name="sender"></param>
		public void CmdDeleteAllFeedItems(ICommand sender) {
			
			FeedTreeNodeBase tn = guiMain.CurrentSelectedNode;
			
			if (tn != null) {
				
				ISmartFolder isFolder = tn as ISmartFolder;
				
				if (isFolder != null) {

					if (isFolder is FlaggedItemsNode) {
						// we need to unflag the items within each subscribed feed:
						for (int i = 0; i < isFolder.Items.Count; i++) {
							NewsItem item = (NewsItem)isFolder.Items[0];
							this.RemoveItemFromSmartFolder(isFolder, item);
						}
					} else {	// simply clr all
						isFolder.Items.Clear();
					}
					isFolder.Modified = true;
					guiMain.PopulateSmartFolder(tn, true);
					tn.UpdateReadStatus(tn, 0);
					return;
				}

			}

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;

		}

		/// <summary>
		/// Called from listview context menu or Edit|Delete menu
		/// </summary>
		/// <param name="sender"></param>
		public void CmdDeleteSelectedFeedItems(ICommand sender) {
			if (guiMain.CurrentSelectedNode != null) {
				guiMain.RemoveSelectedFeedItems();
			}
		}

		/// <summary>
		/// Called from listview context menu or Edit|Restore items menu
		/// </summary>
		/// <param name="sender"></param>
		public void CmdRestoreSelectedFeedItems(ICommand sender) {
			if (guiMain.CurrentSelectedNode != null) {
				guiMain.RestoreSelectedFeedItems();
			}
		}

		/// <summary>
		/// Pops up the NewFeedDialog class and adds a new feed to the list 
		/// of subscribed feeds. The category will be preselected/added.
		/// </summary>
		/// <param name="category">Feed category</param>
		/// <returns>true, if the dialog succeeds (feed subscribed), else false</returns>
		public bool CmdNewFeed(string category) {
			return this.CmdNewFeed(category, null, null);
		}
		/// <summary>
		/// Pops up the NewFeedDialog class and adds a new feed to the list 
		/// of subscribed feeds.
		/// </summary>
		/// <param name="category">Feed category</param>
		/// <param name="feedLink">Feed link</param>
		/// <param name="feedTitle">Feed title</param>
		/// <returns>true, if the dialog succeeds (feed subscribed), else false</returns>
		public bool CmdNewFeed(string category, string feedLink, string feedTitle) {
			bool success = true;
			NewFeedDialog newFeedDialog = new NewFeedDialog(category, defaultCategory, feedHandler.Categories, feedLink, feedTitle); 
			
			newFeedDialog.btnLookupTitle.Enabled = this.InternetAccessAllowed;
			newFeedDialog.FeedHandler = this.feedHandler; 

			try {
				Win32.SetForegroundWindow(MainForm.Handle);
				newFeedDialog.ShowDialog(guiMain);
			} catch {}

			try{ 

				if(newFeedDialog.DialogResult == DialogResult.OK) {
				
					feedsFeed f = new feedsFeed(); 
					f.link  = newFeedDialog.FeedUrl; 
					f.title = newFeedDialog.FeedTitle;
					f.refreshrate = 60; 
					//f.storiesrecentlyviewed = new ArrayList(); 				
					//f.deletedstories = new ArrayList(); 				

					//handle the common case of feed URI not beginning with HTTP 
					try{ 
						Uri reqUri = new Uri(f.link);
						f.link     = reqUri.ToString().Replace("\r\n", String.Empty); //some weird URLs have newlines						
					}catch(UriFormatException){

						if(!f.link.ToLower().StartsWith("http://")){							
							Uri reqUri = new Uri("http://" + f.link); 
							f.link     = reqUri.ToString().Replace("\r\n", String.Empty); 					
						}
				
					}

					if(feedHandler.FeedsTable.Contains(f.link)) {
						feedsFeed f2 = feedHandler.FeedsTable[f.link]; 
						this.MessageInfo("RES_GUIFieldLinkRedundantInfo", 
							(f2.category == null? String.Empty : category + "\\") + f2.title, f2.link );
						newFeedDialog.Close(); 
						return success; 
					}

					if((newFeedDialog.FeedCategory != null) && 
						(!newFeedDialog.FeedCategory.Equals(String.Empty)) && 
						(!newFeedDialog.FeedCategory.Equals(defaultCategory))) {
						f.category = newFeedDialog.FeedCategory; 

						if(!feedHandler.Categories.ContainsKey(f.category)) {
							feedHandler.Categories.Add(f.category); 
						}
					}

					if (newFeedDialog.textUser.Text != null && newFeedDialog.textUser.Text.Trim().Length != 0 ) {	// set feedsFeed new credentials
						string u = newFeedDialog.textUser.Text.Trim(), p = null;
						if (newFeedDialog.textPwd.Text != null && newFeedDialog.textPwd.Text.Trim().Length != 0)
							p = newFeedDialog.textPwd.Text.Trim();
						NewsHandler.SetFeedCredentials(f, u, p);
					} else {
						NewsHandler.SetFeedCredentials(f, null, null);
					}

					f.alertEnabled = f.alertEnabledSpecified = newFeedDialog.checkEnableAlerts.Checked;

					feedHandler.FeedsTable.Add(f.link, f); 
					this.FeedlistModified = true;

					feedHandler.AddFeed2CommonFeedList(f, f.category); 

					guiMain.AddNewFeedNode(f.category, f);
					guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);
					success = true;

				} else {
					success = false;
				}

			}catch(UriFormatException){
				this.MessageError("RES_InvalidFeedURIBoxText");
			}
			
			newFeedDialog.Dispose();
			return success;
		}

		/// <summary>
		/// Link navigation interception: checks the URL for 'subscribe to' commands.
		/// </summary>
		/// <param name="webUrl"></param>
		/// <returns>True, if no further processing/navigation is needed, else False.</returns>
		/// <remarks>
		/// Link navigation interception:
		/// we try to find out if someone clicks on a link provided to import
		/// a feed or feedlist to Userland, AmphetaDesk, Awasu etc.
		/// A good reference about all these is:
		/// http://xml.mfd-consult.dk/syn-sub/?rss=http://www22.brinkster.com/rendelmann/db/net.rss.xml
		/// (with my blog feed as an example ;-).
		/// These links are of the form (Userland):
		/// <code>http://127.0.0.1:5335/system/pages/subscriptions?url=&lt;FEED_URL></code>
		/// or (Amphetadesk, see also http://www.disobey.com/amphetadesk/website_integration.html - Website integration):
		/// <code> 
		/// http://127.0.0.1:8888/index.html?add_url=&lt;FEED_URL>
		/// http://127.0.0.1:8888/index.html?add_urls=&lt;FEED_URL1>,&lt;FEED_URL2>,...
		/// http://127.0.0.1:8888/index.html?add_url=&lt;OPML_URL>
		/// </code>
		/// </remarks>
		public bool InterceptUrlNavigation(string webUrl) {
			if (!interceptUrlNavigation) return false;

			Uri url = null;

			try {
				url = new Uri(webUrl);
			} catch {
				// catch invalid url formats
				return false;
			}

			// first look for localhost
			if (url.IsLoopback) {

				bool   captured = false;
				ArrayList feedurls = RssLocater.UrlsFromWellknownListener(webUrl);
				
				foreach (string feedurl in feedurls) {
					if (feedurl.ToLower().EndsWith(".opml")) {
						this.ImportFeeds(feedurl);	//displays a dialog also
						captured = true;
					} 
					else {	// assume, it is a valid feed link url
						this.CmdNewFeed(defaultCategory , feedurl, null);
						captured = true;
					}
				}

				return captured;

			} else if (url.Scheme.Equals("feed")) {
				this.CmdNewFeed(defaultCategory, RssLocater.UrlFromFeedProtocolUrl(url.ToString()), null );
				return true;
			} else if (url.ToString().EndsWith("opml")) {
				this.ImportFeeds(url.ToString());
				return true;
			}
			
			return false;
		}

		/// <summary>
		/// Check for current default aggregator. If we are not the default, then display
		/// the question dialog, if we have to ask.
		/// </summary>
		public void AskAndCheckForDefaultAggregator() {
			try {
				if (!RssBanditApplication.IsDefaultAggregator() && RssBanditApplication.ShouldAskForDefaultAggregator) {
					AskForDefaultAggregator dialog = new AskForDefaultAggregator();
					if (dialog.ShowDialog(guiMain) == DialogResult.OK) {
						try {
							RssBanditApplication.MakeDefaultAggregator();
						} catch (System.Security.SecurityException) {
							this.MessageInfo("RES_SecurityExceptionCausedByRegistryAccess", "HKEY_CLASSES_ROOT\feed");
						} catch (Exception ex) {
							this.MessageError("RES_ExceptionSettingDefaultAggregator", ex.Message);
						}
					}
					RssBanditApplication.ShouldAskForDefaultAggregator = !dialog.checkBoxDoNotAskAnymore.Checked;
					dialog.Dispose();
				}
			} catch (Exception e) {
				_log.Error("Unexpected error on checking for default aggregator.", e);
			}

			CheckAndRegisterIEMenuExtensions();
		}

		/// <summary>
		/// Detect, if the url contains the 'feed:' protocol. If so, it just remove it
		/// to prepare a valid web url.
		/// </summary>
		/// <param name="feedUrl">Url to mangle</param>
		/// <returns>Mangled Url</returns>
		public string HandleUrlFeedProtocol(string feedUrl) {
			//code moved to:
			return RssLocater.UrlFromFeedProtocolUrl(feedUrl);
		}



		/// <summary>
		/// Uses the current defined XSLT template to format the feeds to HTML.
		/// </summary>
		/// <param name="feeds">The list of feeds to transform</param>
		/// <returns>The feeds formatted as a HTML string</returns>
		public string FormatFeeds(string stylesheet, FeedInfoList feeds) {

			if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet)){
				this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
			}
			
			return this.NewsItemFormatter.ToHtml(stylesheet, feeds);			
		}
	

		/// <summary>
		/// Uses the current defined XSLT template to
		/// format the feed  to HTML.
		/// </summary>
		/// <param name="feed">The feed to transform</param>
		/// <returns>The feed formatted as a HTML string</returns>
		public string FormatFeed(string stylesheet, FeedInfo feed) {

			if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet)){
				this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
			}
			
			return this.NewsItemFormatter.ToHtml(stylesheet, feed);			
		}


		/// <summary>
		/// Uses the current defined XSLT template to
		/// format the item to HTML.
		/// </summary>
		/// <param name="item">The NewsItem</param>
		/// <returns>The NewsItem formatted as a HTML string</returns>
		public string FormatNewsItem(string stylesheet, NewsItem item, SearchCriteriaCollection toHighlight) {

			if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet)){
				this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
			}
			
			if (toHighlight == null) {
				return this.NewsItemFormatter.ToHtml(stylesheet, item);
			} else {
				ArrayList criterias = new ArrayList();
				for (int i = 0; i < toHighlight.Count; i++) {
					// only String matches are interesting for highlighting
					SearchCriteriaString scs = toHighlight[i] as SearchCriteriaString;
					if (scs != null && scs.Match(item)) {
						criterias.Add(scs);
					}
				}
				if (criterias.Count > 0) {		
					NewsItem clone = new NewsItem(item.Feed, item.Title, item.Link, 
						this.ApplyHighlightingTo(item.Content, criterias),item.Date, item.Subject,
						item.ContentType, item.OptionalElements, item.Id, item.ParentId); 	
					clone.FeedDetails = item.FeedDetails;					
					return NewsItemFormatter.ToHtml(stylesheet,clone);				
				} else {
					return this.NewsItemFormatter.ToHtml(stylesheet,item);
				}
			}
		}	


		private string ApplyHighlightingTo(string xhtml, ArrayList searchCriteriaStrings){
		
			for (int i = 0; i < searchCriteriaStrings.Count; i++){
			
					
				// only String matches are interesting for highlighting here
				SearchCriteriaString scs = searchCriteriaStrings[i] as SearchCriteriaString;
					
				if (scs != null){
				
					switch(scs.WhatKind){
					
						case StringExpressionKind.Text: 													

							//match html tags
							Match m = SearchCriteriaString.htmlRegex.Match(xhtml);							
    
							//strip markup 
							string strippedxhtml = SearchCriteriaString.htmlRegex.Replace(xhtml, "$!$"); 

							//replace search words     							
							Regex replaceRegex = new Regex ("(" + EscapeRegexSpecialChars(scs.What) + ")", RegexOptions.IgnoreCase);
							string highlightedxhtml = replaceRegex.Replace( strippedxhtml, "<span style='color:highlighttext;background:highlight'>$1</span>"); 

							//Reinsert HTML
							StringBuilder sb = new StringBuilder(); 							
							string[] splitxhtml =  SearchCriteriaString.placeholderRegex.Split(highlightedxhtml); 

							foreach(string s in splitxhtml) {
								sb.Append(s); 

								if(m.Success) {
									sb.Append(m.Value); 
									m = m.NextMatch();     
								}
							}  
							xhtml = sb.ToString(); 
							break; 

						case StringExpressionKind.RegularExpression:						

							//match html tags
							Match m2 = SearchCriteriaString.htmlRegex.Match(xhtml);							
    
							//strip markup 
							string strippedxhtml2 = SearchCriteriaString.htmlRegex.Replace(xhtml, "$!$"); 

							//replace search words     
							Regex replaceRegex2 = new Regex ("(" + scs.What + ")");
							string highlightedxhtml2 = replaceRegex2.Replace( strippedxhtml2, "<span style='color:highlighttext;background:highlight'>$1</span>"); 
    							

							//Reinsert HTML
							StringBuilder sb2 = new StringBuilder(); 							
							string[] splitxhtml2 =  SearchCriteriaString.placeholderRegex.Split(highlightedxhtml2); 

							foreach(string s in splitxhtml2) {
								sb2.Append(s); 

								if(m2.Success) {
									sb2.Append(m2.Value); 
									m2 = m2.NextMatch();     
								}
							}  		
							xhtml = sb2.ToString(); 
							break; 

						case StringExpressionKind.XPathExpression:	/* NOTHING TO DO HERE */ 
							break;
						default: 
							break; 
					}
				}
			}

			return xhtml; 
		}

		private static string EscapeRegexSpecialChars(string input){		
			return input.Replace("\\", "\\\\").Replace(".", "\\.").Replace("$", "\\?").Replace("*", "\\*").Replace("+","\\+").Replace("^","\\^").Replace("|","\\|").Replace("?","\\?").Replace("(","\\(").Replace(")","\\)").Replace("[","\\[").Replace("]","\\]");
		}

		/*
		// recursivly called
		private void ApplyHighlightingTo(XmlNode node, ArrayList searchCriteriaStrings) {
			if (node == null) return;			

			if (node.ChildNodes.Count > 0) {
				foreach (XmlNode n in node.ChildNodes) {
					ApplyHighlightingTo(n, searchCriteriaStrings);
				}
			} else {
								

				//apply highlighting to:
				string content = node.InnerText;
				if (StringHelper.EmptyOrNull(content))
					return;

				StringBuilder newContent = new StringBuilder(content.Length);
				for (int i = 0; i < searchCriteriaStrings.Count; i++) {
					
					// only String matches are interesting for highlighting here
					SearchCriteriaString scs = searchCriteriaStrings[i] as SearchCriteriaString;
					
					if (scs != null) {
						switch(scs.WhatKind){
				
								//BUGBUG: Text on markup bounderies won't be found, e.g. search for 
								//        'Hello World' won't match 'Hello <i>World</i>' 

							case StringExpressionKind.Text: 
								bool anyMatch = false;
								int startIndex = 0, foundIndex = content.ToLower().IndexOf(scs.What.ToLower()), lenWhat = scs.What.Length;
								while (foundIndex >= 0) {
									anyMatch = true;
									newContent.Append(content.Substring(startIndex, foundIndex-startIndex));
									newContent.Append("<span style=\"color:highlighttext;background:highlight\">");
									newContent.Append(scs.What);
									newContent.Append("</span>");
									startIndex = foundIndex + lenWhat;
									foundIndex = content.ToLower().IndexOf(scs.What.ToLower(), startIndex);
								}
								if (anyMatch) {
									newContent.Append(content.Substring(startIndex,content.Length-startIndex));
									XmlElement newNode = (XmlElement) node.OwnerDocument.CreateNode(XmlNodeType.Element, "span", null);									
									AddOfficeNamespaces(newNode); // This is here because so many people cut & paste into blogs from Microsoft Word								
									
										newNode.InnerXml = newContent.ToString();
										XmlNode parent = node.ParentNode;
										parent.ReplaceChild(newNode, node);
									
								}
								break;

							case StringExpressionKind.RegularExpression:	//TODO

								Regex regex = new Regex(scs.What, RegexOptions.IgnoreCase); 
					
								if(regex.Match(content).Success)   {
									string s = regex.Replace(content, new MatchEvaluator(this.RegexHighlightEvaluate));
									XmlElement newNode = (XmlElement)node.OwnerDocument.CreateNode(XmlNodeType.Element, "span", null);
									AddOfficeNamespaces(newNode); // This is here because so many people cut & paste into blogs from Microsoft Word								
									try {
										newNode.InnerXml = s;
										XmlNode parent = node.ParentNode;
										parent.ReplaceChild(newNode, node);
									} catch (Exception ex) {
										Trace.WriteLine("Highlighting Exception: " + ex.Message);
										Trace.WriteLine("INNERXML: " + newContent.ToString());
									}
								}
								break;

							case StringExpressionKind.XPathExpression:	//HOWTO?

//								XPathDocument doc  = new XPathDocument(new System.IO.StringReader(item.ToString(true))); 
//								XPathNavigator nav = doc.CreateNavigator(); 
//
//								if((bool)nav.Evaluate("boolean(" + What + ")")){
//									return true;
//								}else{
//									return false; 
//								}
								break;

							default: 
								break; 
						}

					}
				}
			}
		}


		private void AddOfficeNamespaces(XmlElement elem){
		
			XmlAttribute attr = elem.OwnerDocument.CreateAttribute("xmlns", "o", "http://www.w3.org/2000/xmlns/");
			attr.Value        = "urn:schemas-microsoft-com:office:office"; 
			elem.Attributes.Append(attr); 			

			attr = elem.OwnerDocument.CreateAttribute("xmlns", "w", "http://www.w3.org/2000/xmlns/");
			attr.Value        = "urn:schemas-microsoft-com:office:word"; 
			elem.Attributes.Append(attr); 

			attr = elem.OwnerDocument.CreateAttribute("xmlns", "st1", "http://www.w3.org/2000/xmlns/");
			attr.Value        = "urn:schemas-microsoft-com:office:smarttags"; 
			elem.Attributes.Append(attr); 

			attr = elem.OwnerDocument.CreateAttribute("xmlns", "st2", "http://www.w3.org/2000/xmlns/");
			attr.Value        = "urn:schemas-microsoft-com:office:smarttags";
			elem.Attributes.Append(attr); 
			
			attr = elem.OwnerDocument.CreateAttribute("xmlns", "asp", "http://www.w3.org/2000/xmlns/");
			attr.Value        = "http://www.example.com/asp";
			elem.Attributes.Append(attr); 		
		}


		// delegate called from ApplyHighlightingTo()
		private string RegexHighlightEvaluate(Match m) {
			string x = m.ToString();
			return String.Format("<span style=\"color:highlighttext;background:highlight\">{0}</span>", x);
		}
		*/

		/// <summary>
		/// Loads the default stylesheet from disk and returns it as a string
		/// </summary>
		/// <returns>The XSLT stylesheet</returns>
		protected string GetNewsItemFormatterTemplate() {
			return this.GetNewsItemFormatterTemplate(Preferences.NewsItemStylesheetFile); 
		}


		/// <summary>
		/// Loads the stylesheet from disk and returns it as a string
		/// </summary>
		/// <param name="stylesheet">The stylesheet name</param>
		/// <returns>The XSLT stylesheet</returns>
		protected string GetNewsItemFormatterTemplate(string stylesheet) {
			string s = RssBanditApplication.GetTemplatesPath();
			string t = NewsItemFormatter.DefaultNewsItemTemplate;

			if (stylesheet == null || stylesheet.Length == 0)
				return t;

			if (Directory.Exists(s)) {
				string filename = Path.Combine(s, stylesheet + ".fdxsl");
				if (File.Exists(filename)) {	
					try {
						using (StreamReader sr = new StreamReader(filename, true)) {
							t = sr.ReadToEnd();
							sr.Close();
						}
					}
					catch (Exception) { //stylesheet probably not found
						if (Preferences.NewsItemStylesheetFile.Equals(stylesheet)) {						
							Preferences.NewsItemStylesheetFile = String.Empty;
						}
					}
				}
				else { // not file.exists
					if (Preferences.NewsItemStylesheetFile.Equals(stylesheet)) {						
						Preferences.NewsItemStylesheetFile = String.Empty;
					}
				}
			}
			else { // not dir.exists
				if (Preferences.NewsItemStylesheetFile.Equals(stylesheet)) {						
					Preferences.NewsItemStylesheetFile = String.Empty;
				}
			}

			return t;
		}

		#region Commands listened

		/// <summary>
		/// Exiting the Application.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdExitApp(ICommand sender) {
			// really exit app.
			if (guiMain != null) {
				guiMain.Close(true);
			}
			Application.Exit();
		}

		/// <summary>
		/// Show Alert Windows Mode: None.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowAlertWindowNone(ICommand sender) {
			Mediator.SetChecked(
				"+cmdShowAlertWindowNone", 
				"-cmdShowAlertWindowConfiguredFeeds", 
				"-cmdShowAlertWindowAll");
			Preferences.ShowAlertWindow = DisplayFeedAlertWindow.None;
			this.SavePreferences();
		}

		/// <summary>
		/// Show Alert Windows Mode: Configured Feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowAlertWindowConfigPerFeed(ICommand sender) {
			Mediator.SetChecked(
				"-cmdShowAlertWindowNone", 
				"+cmdShowAlertWindowConfiguredFeeds", 
				"-cmdShowAlertWindowAll");
			Preferences.ShowAlertWindow = DisplayFeedAlertWindow.AsConfiguredPerFeed;
			this.SavePreferences();
		}

		/// <summary>
		/// Show Alert Windows Mode: All.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowAlertWindowAll(ICommand sender) {
			Mediator.SetChecked(
				"-cmdShowAlertWindowNone", 
				"-cmdShowAlertWindowConfiguredFeeds", 
				"+cmdShowAlertWindowAll");
			Preferences.ShowAlertWindow = DisplayFeedAlertWindow.All;
			this.SavePreferences();
		}

		/// <summary>
		/// Toggle the Show New Items Received Balloon Mode.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdToggleShowNewItemsReceivedBalloon(ICommand sender) {
			bool currentChecked;
			
			if (sender != null) 	// really sent by the Gui Component
				currentChecked = ((ICommandComponent)sender).Checked;
			else
				currentChecked = Mediator.IsCommandComponentChecked("cmdShowNewItemsReceivedBalloon");

			Preferences.ShowNewItemsReceivedBalloon = !currentChecked;
			Mediator.SetCommandComponentChecked("cmdShowNewItemsReceivedBalloon", !currentChecked);
			this.SavePreferences();
		}

		/// <summary>
		/// Toggle the Internet Connection Mode.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdToggleInternetConnectionMode(ICommand sender) {
			bool currentChecked;
			
			if (sender != null) 	// really sent by the Gui Component
				currentChecked = ((ICommandComponent)sender).Checked;
			else
				currentChecked = Mediator.IsCommandComponentChecked("cmdToggleOfflineMode");

			this.UpdateInternetConnectionState(true);		// get current state, takes a few msecs
			if (this.InternetConnectionOffline != currentChecked) {
				// only the current Gui state is not up-to-date, done
			} else {	// force toggle
				Utils.SetIEOffline(!currentChecked);		// update IE
				this.UpdateInternetConnectionState(true);	// get new state, takes a few msecs
			}
		}

		/// <summary>
		/// Display the about box.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdAboutApp(ICommand sender) {
			// view about box. 
			// ToDo: add advertising links:
			// ... and to the project workspace/bug report section, license etc.
			
			MessageBox.Show (RssBanditApplication.Caption + " written by\n\n"+
				"  * Dare Obasanjo (DareObasanjo, www.25hoursaday.com/weblog/)\n"+
				"  * Torsten Rendelmann (TorstenR, www.rendelmann.info/blog/)\n"+
				"  * Phil Haack (haacked.com)\n"+
				"  * and all the active members of RSS Bandit community.\n"+
				"\nCredits:\n\n"+
				"  * Mike Krueger (#ZipLib)\n"+ 
				"  * Jack Palevich (NntpClient)\n"+				
				"  * SandBar, SandDock Copyright (c) 2005 by Divelements Limited, http://www.divil.co.uk/net/\n"+
				"  * Portions Copyright ©2002-2004 The Genghis Group (www.genghisgroup.com)\n"+
				"  * sourceforge.net team (Project hosting)", Resource.Manager["RES_WindowAboutCaption", RssBanditApplication.CaptionOnly], 
				MessageBoxButtons.OK, MessageBoxIcon.Asterisk); 

		}

		/// <summary>
		/// Display the Help Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdWebHelp(ICommand sender)	{
			this.OpenUrlInExternalBrowser(webHelpUrl);
		}

		/// <summary>
		/// Display the Bug Report Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdReportAppBug(ICommand sender) {
			this.PreferredNavigateToUrl(bugReportUrl, RssBanditApplication.CaptionOnly + ": Bug Tracker", true, true);
		}

		/// <summary>
		/// Display the Workspace News Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdWorkspaceNews(ICommand sender)	{
			this.PreferredNavigateToUrl(workspaceNewsUrl, RssBanditApplication.CaptionOnly + ": Project News", true, true);
		}

		/// <summary>
		/// Display the Wiki News Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdWikiNews(ICommand sender) {
			this.PreferredNavigateToUrl(wikiNewsUrl, RssBanditApplication.CaptionOnly + ": Wiki", true, true);
		}

		/// <summary>
		/// Display the Wiki News Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdVisitForum(ICommand sender) {
			this.PreferredNavigateToUrl(forumUrl, RssBanditApplication.CaptionOnly + ": Forum", true, true);
		}

		/// <summary>
		/// Display Donate to project Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDonateToProject(ICommand sender) {
			this.PreferredNavigateToUrl(projectDonationUrl, RssBanditApplication.CaptionOnly + ": Donate", true, true);
		}
		

		/// <summary>
		/// Check for program updates.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdCheckForUpdates(ICommand sender) {
			this.CmdCheckForUpdates(AutoUpdateMode.Manually);
		}

		/// <summary>
		/// Check for program updates.
		/// </summary>
		/// <param name="mode">Update Mode</param>
		public void CmdCheckForUpdates(AutoUpdateMode mode) {
#if USEAUTOUPDATE
			if (mode == AutoUpdateMode.Manually)
				this.CheckForUpdates(mode);
			else {		// called on App Startup.
				
				if (!InternetAccessAllowed)
					return;
				
				// consider preferences settings
				if (this.Preferences.AutoUpdateFrequency == AutoUpdateMode.Manually)
					return;
				
				if (this.Preferences.AutoUpdateFrequency == AutoUpdateMode.OnApplicationStart &&
					mode == AutoUpdateMode.OnApplicationStart) {
					this.CheckForUpdates(mode);
					return;
				}
				// check, if it is time to check for updates
				DateTime t = this.Preferences.LastAutoUpdateCheck;
				if (this.Preferences.AutoUpdateFrequency == AutoUpdateMode.OnceIn14Days)
					t = t.AddDays(14);
				else
					t = t.AddMonths(1);
				if (DateTime.Compare(t, DateTime.Now) < 0) 
					this.CheckForUpdates(mode);
			}
#endif
		}

		private void OnApplicationUpdateAvailable(object sender, UpdateAvailableEventArgs e) {
			AutoUpdateMode mode = (AutoUpdateMode)RssBanditUpdateManager.Tag;
			bool hasUpdates = e.NewVersionAvailable;

			if (hasUpdates) {
			
				if (DialogResult.No == MessageQuestion("RES_DialogQuestionNewAppVersionAvailable")) {
					Preferences.LastAutoUpdateCheck = DateTime.Now;
					SavePreferences();
				} else 	{
					
					//RssBanditUpdateManager.BeginDownload(updateManager.AvailableUpdates);	// Async. Preferences updated in OnUpdateComplete event
					
					// for now we do not download anything, just display the SF download page:
					this.PreferredNavigateToUrl(projectDownloadUrl, RssBanditApplication.CaptionOnly + ": Download", true, true);
					Preferences.LastAutoUpdateCheck = DateTime.Now;
					SavePreferences();
				}

			} else {
				Preferences.LastAutoUpdateCheck = DateTime.Now;
				SavePreferences();

				if (mode == AutoUpdateMode.Manually)
					MessageInfo("RES_DialogMessageNoNewAppVersionAvailable");
			}

		}

		private void CheckForUpdates(AutoUpdateMode mode) {
			try 
			{
				RssBanditUpdateManager.Tag = mode;
				if (mode == AutoUpdateMode.Manually)
					RssBanditUpdateManager.BeginCheckForUpdates(this.guiMain); 
				else
					RssBanditUpdateManager.BeginCheckForUpdates(null); 
			}
			catch (Exception ex)
			{
				_log.Error("RssBanditUpdateManager.BeginCheckForUpdates() failed", ex);
			}
		}

		/// <summary>
		/// Re-Display/Open the main GUI.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowMainGui(ICommand sender)	{
			if (guiMain != null && !guiMain.Disposing) {
				if (!guiMain.Visible){
					//BUGBUG
					guiMain.Show(); // was before if ()..., now SandBar does not crash our app
					if (this.Preferences.HideToTrayAction == HideToTray.OnMinimize)
						Win32.ShowWindow(guiMain.Handle, Win32.ShowWindowStyles.SW_RESTORE);
					//End BUGBUG: proove after Tim Dawson provided a fix
					Win32.SetForegroundWindow(guiMain.Handle);
				} 
				else
					guiMain.Activate();
			}
		}

		/// <summary>
		/// Refresh all feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdRefreshFeeds(ICommand sender) {
			guiMain.UpdateAllFeeds(true);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// Pops up the NewFeedDialog class and adds a new feed to the list 
		/// of subscribed feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNewFeed(ICommand sender) {
			string category = guiMain.CategoryOfSelectedNode();

			if(category == null) {
				category = DefaultCategory; 
			}

			this.CmdNewFeed(category.Trim());

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;

		}

		/// <summary>
		/// Moves the focus to the next unread feed item if available.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNextUnreadFeedItem(ICommand sender) {
			guiMain.MoveToNextUnreadItem();
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}
		
		/// <summary>
		/// Display a dialog to handle autodiscovery of feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdAutoDiscoverFeed(ICommand sender) {
			AutoDiscoverFeedsDialog autoDiscoverFeedsDialog = new AutoDiscoverFeedsDialog(); 
			autoDiscoverFeedsDialog.WebpageUrl = guiMain.UrlText;
			autoDiscoverFeedsDialog.ShowDialog(guiMain);

			if(autoDiscoverFeedsDialog.DialogResult == DialogResult.OK) {														

				if(!autoDiscoverFeedsDialog.IsKeywordSearch && StringHelper.EmptyOrNull(autoDiscoverFeedsDialog.WebpageUrl)) {
					this.MessageError("RES_GUIFieldWebUrlInvalid");
					autoDiscoverFeedsDialog.Close(); 
					return; 
				}

				if(autoDiscoverFeedsDialog.IsKeywordSearch && StringHelper.EmptyOrNull(autoDiscoverFeedsDialog.Keywords)) {
					this.MessageError("RES_GUIFieldKeywordsInvalid");
					autoDiscoverFeedsDialog.Close(); 
					return; 
				}

				try { 

					AutoDiscoverFeedsThreadHandler autoDiscover = new AutoDiscoverFeedsThreadHandler();
					autoDiscover.Proxy = this.Proxy;
					if (autoDiscoverFeedsDialog.IsKeywordSearch) {
						autoDiscover.SearchTerms = autoDiscoverFeedsDialog.Keywords;
						autoDiscover.SearchType = Syndic8SearchType.Keyword;
						autoDiscover.OperationMessage = Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageDetectingFeedsWithKeywords", autoDiscoverFeedsDialog.Keywords);
					} else {
						autoDiscover.WebPageUrl = autoDiscoverFeedsDialog.WebpageUrl;
						autoDiscover.SearchType = Syndic8SearchType.Url;
						autoDiscover.OperationMessage = Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageDetectingFeeds", autoDiscoverFeedsDialog.WebpageUrl);
					}
					
					if (DialogResult.OK != autoDiscover.Start( guiMain ))
						return;	// cancelled
                    
					if (!autoDiscover.OperationSucceeds)
						return;

					Hashtable feedUrls = autoDiscover.DiscoveredFeeds;
                    
					if(feedUrls.Count == 0) {
						this.MessageInfo("RES_GUIStatusInfoMessageNoFeedsFound");
						return; 
					}

					DiscoveredFeedsDialog discoveredFeedsDialog = new DiscoveredFeedsDialog(feedUrls); 
					discoveredFeedsDialog.ShowDialog(guiMain);

					if(discoveredFeedsDialog.DialogResult == DialogResult.OK) {
						foreach( ListViewItem feedItem in discoveredFeedsDialog.listFeeds.SelectedItems ) {
							this.CmdNewFeed(defaultCategory, (string)feedItem.Tag, feedItem.SubItems[0].Text); 
						}
					}

				}
				catch(Exception e) {
					_log.Error("AutoDiscoverFeed exception.", e);
					this.MessageError("RES_ExceptionGeneral", e.Message);
				}
			}		
			
		}
		
		/// <summary>
		/// Pops up the NewCategoryDialog class and adds a new category to the list 
		/// of subscribed feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNewCategory(ICommand sender) {
			guiMain.NewCategory();
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// Delete all Feeds subscribed to.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDeleteAll(ICommand sender) {
			if (this.MessageQuestion("RES_MessageBoxDeleteAllFeedsQuestion") == DialogResult.Yes) {
				this.feedHandler.FeedsTable.Clear(); 
				this.feedHandler.Categories.Clear(); 
				this.FeedHandler.ClearItemsCache();
				this.FeedlistModified = true;
				guiMain.InitiatePopulateTreeFeeds(); 
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdUpdateCategory(ICommand sender) {
			guiMain.UpdateCategory(true);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdRenameCategory(ICommand sender) {
			guiMain.InitiateRenameFeedOrCategory();
			this.FeedlistModified = true;
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDeleteCategory(ICommand sender) {
			if (guiMain.NodeEditingActive)
				return;
			if (this.MessageQuestion("RES_MessageBoxDeleteAllFeedsInCategoryQuestion") == DialogResult.Yes) {
				guiMain.DeleteCategory();
				this.FeedlistModified = true;
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdUpdateFeed(ICommand sender) {
			string feedUrl = (string)guiMain.CurrentSelectedNode.Tag;
			if (!StringHelper.EmptyOrNull(feedUrl)) {
				this.feedHandler.AsyncGetItemsForFeed(feedUrl, true, true);
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdRenameFeed(ICommand sender) {
			guiMain.InitiateRenameFeedOrCategory();
			this.FeedlistModified = true;
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}


		/// <summary>
		/// Reload the feedlist from the common feedlist
		/// </summary>
		/// <param name="sender"></param>
		public void CmdReloadCFL(ICommand sender) {
			feedHandler.ReplaceFeedlist(null); 
			this.FeedlistModified = true;

			guiMain.InitiatePopulateTreeFeeds();
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDeleteFeed(ICommand sender) {
			
			if (guiMain.NodeEditingActive)
				return;

			if (this.MessageQuestion("RES_MessageBoxDeleteThisFeedQuestion") == DialogResult.Yes) {
				guiMain.DeleteFeed();
				this.FeedlistModified = true;
			}

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdViewSourceOfFeed(ICommand sender) {
			if(guiMain.CurrentSelectedNode!= null && guiMain.CurrentSelectedNode.Tag != null) {
				this.PreferredNavigateToUrl((string)guiMain.CurrentSelectedNode.Tag, Resource.Manager.FormatMessage("RES_TabFeedSourceCaption", guiMain.CurrentSelectedNode.Text),true, true);
			}								

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// Overloaded. Validates a feed link.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdValidateFeed(ICommand sender) {
			this.CmdValidateFeed((string)guiMain.CurrentSelectedNode.Tag);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// Overloaded. Validates a feed link.
		/// </summary>
		/// <param name="feedLink">Feed link</param>
		public void CmdValidateFeed(string feedLink) {
			if (!StringHelper.EmptyOrNull(feedLink)) {
				this.PreferredNavigateToUrl(validationUrlBase+HttpUtility.UrlEncode(feedLink), Resource.Manager["RES_TabValidationResultCaption"], true, true);
			}
		}

		/// <summary>
		/// Overloaded. Navigates to feed home page (feed link).
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNavigateFeedHome(ICommand sender) {
			this.CmdNavigateFeedHome((string)guiMain.CurrentSelectedNode.Tag);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// Overloaded. Navigates to feed home page (feed link).
		/// </summary>
		/// <param name="feedLink">Feed link</param>
		public void CmdNavigateFeedHome(string feedLink) {
			if (!StringHelper.EmptyOrNull(feedLink)) {
				
				IFeedDetails feedInfo = feedHandler.GetFeedInfo(feedLink);

				if (feedInfo != null) {
					this.PreferredNavigateToUrl(feedInfo.Link, Resource.Manager.FormatMessage("RES_TabFeedHomeCaption", feedInfo.Title), true, true);
				}
			}
		}

		/// <summary>
		/// Overloaded. Display technorati link cosmos of the feed.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNavigateFeedLinkCosmos(ICommand sender) {
			this.CmdNavigateFeedLinkCosmos((string)guiMain.CurrentSelectedNode.Tag);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// Overloaded. Display technorati link cosmos of the feed.
		/// </summary>
		/// <param name="feedLink">Feed link</param>
		public void CmdNavigateFeedLinkCosmos(string feedLink) {
			if (!StringHelper.EmptyOrNull(feedLink)) {
				IFeedDetails feedInfo = feedHandler.GetFeedInfo(feedLink);
				if (feedInfo != null) {
					this.PreferredNavigateToUrl(linkCosmosUrlBase+HttpUtility.UrlEncode(feedInfo.Link),Resource.Manager.FormatMessage("RES_TabLinkCosmosCaption", feedInfo.Title), true, true);
				}
			}
		}

		/// <summary>
		/// Uses SaveFileDialog to save the feed file either as a file conforming 
		///	to feeds.xsd or an OPML file in the format used by Radio Userland, AmphetaDesk, 
		///	and other news aggregators. 
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdExportFeeds(ICommand sender) {
			
			ExportFeedsDialog dialog = new ExportFeedsDialog(guiMain.GetRoot(RootFolderType.MyFeeds), Preferences.NormalFont, guiMain.TreeImageList);
			if (DialogResult.OK == dialog.ShowDialog(guiMain)) {
			
				Stream myStream ;
				SaveFileDialog sfd = new SaveFileDialog();
 
				ArrayList selections = dialog.GetSelectedFeedUrls();
				NewsComponents.Collections.FeedsCollection fc = new NewsComponents.Collections.FeedsCollection(selections.Count);
				foreach (string url in selections) {
					if (this.feedHandler.FeedsTable.Contains(url))
						fc.Add(url, this.feedHandler.FeedsTable[url]);
				}
				
				if (fc.Count == 0)
					fc = feedHandler.FeedsTable;

				bool includeEmptyCategories = false;
				FeedListFormat format = FeedListFormat.OPML;

				String.Format("{0} (*.*)|*.*", Resource.Manager["RES_FileDialogFilterAllFiles"]);

				if (dialog.radioFormatOPML.Checked) {
					format = FeedListFormat.OPML;
					sfd.Filter = String.Format("{0} (*.opml)|*.opml|{1} (*.*)|*.*", Resource.Manager["RES_FileDialogFilterOPMLFiles"], Resource.Manager["RES_FileDialogFilterAllFiles"]);
					includeEmptyCategories = dialog.checkFormatOPMLIncludeCats.Checked;
				} else if (dialog.radioFormatNative.Checked) {
					format = FeedListFormat.NewsHandler;
					sfd.Filter = String.Format("{0} (*.xml)|*.xml|{1} (*.*)|*.*", Resource.Manager["RES_FileDialogFilterXMLFiles"], Resource.Manager["RES_FileDialogFilterAllFiles"]);
					if (!dialog.checkFormatNativeFull.Checked)
						format = FeedListFormat.NewsHandlerLite;
				}

				sfd.FilterIndex = 1 ;
				sfd.RestoreDirectory = true ;
 
				if(sfd.ShowDialog() == DialogResult.OK) {
					try {
						if((myStream = sfd.OpenFile()) != null) {

							this.feedHandler.SaveFeedList(myStream, format, fc, includeEmptyCategories); 
							myStream.Close();
						}
					} catch (Exception ex) {
						this.MessageError("RES_ExceptionSaveFileMessage", ex.Message);
					}
				}
			}
			
			dialog.Dispose();
		}

		/// <summary>
		/// Loads a feed list using the open file dialog. Either feed lists conforming 
		/// to the feeds.xsd schema or OPML files in the format used by Radio UserLand, 
		/// AmphetaDesk and other news aggregators. 
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdImportFeeds(ICommand sender) {
			string category = String.Empty;
			FeedTreeNodeBase n = guiMain.CurrentSelectedNode;
			if(n!= null){
				if (n.Type == FeedNodeType.Category || n.Type == FeedNodeType.Feed)
					category = guiMain.BuildCategoryStoreName(n);
			}
			this.ImportFeeds(String.Empty, category);
		}

		/// <summary>
		/// Sends the feed list to the location configured on the 
		/// Remote Storage tab of the Options dialog.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdUploadFeeds(ICommand sender) {			

			if (!Preferences.UseRemoteStorage) {
				this.MessageInfo("RES_RemoteStorageFeature.Info");
				return;
			}

			// Make sure this is what the user wants to do
			if (this.MessageQuestion("RES_RemoteStorageUpload.Question")  == DialogResult.No) {
				return;
			}


			RemoteFeedlistThreadHandler rh = new RemoteFeedlistThreadHandler(
				RemoteFeedlistThreadHandler.Operation.Upload, this,
				Preferences.RemoteStorageProtocol, Preferences.RemoteStorageLocation,
				Preferences.RemoteStorageUserName, Preferences.RemoteStoragePassword, this.GuiSettings);

			DialogResult result = rh.Start(guiMain, Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageUpLoadingFeedlist", Preferences.RemoteStorageProtocol.ToString()), false);

			if (result != DialogResult.OK)
				return;

			if (rh.OperationSucceeds) {
				this.MessageInfo("RES_RemoteStorageUploadSucceeds.Info");
			} else {
				this.MessageError("RES_GUIFeedlistUploadExceptionMessage", rh.OperationException.Message);
			}

		}

		/// <summary>
		/// Loads the feed list from the location configured on the 
		/// Remote Storage tab of the Options dialog.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDownloadFeeds(ICommand sender) {
			if (!Preferences.UseRemoteStorage) {
				this.MessageInfo("RES_RemoteStorageFeature.Info");
				return;
			}

			if (this.MessageQuestion("RES_RemoteStorageDownload.Question") == DialogResult.No) {
				return;
			}

			RemoteFeedlistThreadHandler rh = new RemoteFeedlistThreadHandler(
				RemoteFeedlistThreadHandler.Operation.Download, this,
				Preferences.RemoteStorageProtocol, Preferences.RemoteStorageLocation,
				Preferences.RemoteStorageUserName, Preferences.RemoteStoragePassword, this.GuiSettings);

			DialogResult result = rh.Start(guiMain, Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageDownLoadingFeedlist", Preferences.RemoteStorageProtocol.ToString()), false);

			if (result != DialogResult.OK)
				return;

			if (rh.OperationSucceeds) {
				guiMain.SyncFinderNodes();
				guiMain.InitiatePopulateTreeFeeds();
			} else {
				this.MessageError("RES_GUIFeedlistDownloadExceptionMessage", rh.OperationException.Message);
			}

		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowOptions(ICommand sender) {

			if (!this.SearchEngineHandler.EnginesLoaded || !this.SearchEngineHandler.EnginesOK)
				this.LoadSearchEngines();

			PreferencesDialog propertiesDialog = new PreferencesDialog(refreshRate/60000, Preferences, this.searchEngines, new IdentityNewsServerManager(this)); 
			propertiesDialog.OnApplyPreferences += new EventHandler(this.OnApplyPreferences);
			propertiesDialog.ShowDialog(guiMain);

			if(propertiesDialog.DialogResult == DialogResult.OK) {
				this.OnApplyPreferences(propertiesDialog, new EventArgs());
			}

			//close dialog box
			propertiesDialog.Close(); 

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowFeedProperties(ICommand sender) {
			if(guiMain.CurrentSelectedNode!= null && guiMain.CurrentSelectedNode.Tag != null) {
				FeedTreeNodeBase tn = guiMain.CurrentSelectedNode;

				feedsFeed f = null;
				int refreshrate = this.refreshRate;
				TimeSpan feedMaxItemAge = TimeSpan.Zero;
				bool feedDisabled = false;
				bool feedMarkItemsReadOnExit = false; 

				try {
					f = feedHandler.FeedsTable[(string)tn.Tag]; 
					//refreshrate = (f.refreshrateSpecified ? f.refreshrate : this.refreshRate); 							
					try 
					{
						refreshrate = feedHandler.GetRefreshRate(f.link); 
						feedMaxItemAge = feedHandler.GetMaxItemAge(f.link);
						feedMarkItemsReadOnExit = feedHandler.GetMarkItemsReadOnExit(f.link); 
					} catch {/* ignore this */}

				} catch (Exception e) {
					this.MessageError("RES_GUIStatusErrorGeneralFeedMessage", (string)tn.Tag, e.Message);
					return;
				}

				FeedProperties propertiesDialog = new FeedProperties(f.title, f.link, refreshrate/60000, feedMaxItemAge, (f.category == null ? defaultCategory: f.category), defaultCategory, feedHandler.Categories, this.feedHandler.GetStyleSheet(f.link)); 
				propertiesDialog.comboMaxItemAge.Enabled = !feedMaxItemAge.Equals(TimeSpan.Zero);
				propertiesDialog.checkEnableAlerts.Checked = f.alertEnabled;
				propertiesDialog.checkMarkItemsReadOnExit.Checked = feedMarkItemsReadOnExit;


				if (f.authUser != null) {	// feedsFeed has credentials
					string u = null, p = null;
					NewsHandler.GetFeedCredentials(f, ref u, ref p);
					propertiesDialog.textUser.Text = u;
					propertiesDialog.textPwd.Text = p;
				}

				propertiesDialog.ShowDialog(guiMain);

				if(propertiesDialog.DialogResult == DialogResult.OK) {
					
					bool refreshThisFeed = false;

					if((propertiesDialog.textBox1.Text == null) ||
						(propertiesDialog.textBox2.Text == null)||
						propertiesDialog.textBox1.Text.Trim().Equals(String.Empty) ||
						propertiesDialog.textBox2.Text.Trim().Equals(String.Empty)) {
					
						this.MessageError("RES_GUIFieldLinkTitleInvalid");
					
					}	else {
						
						if(!f.link.Equals(propertiesDialog.textBox2.Text.Trim())) {
							// link was changed						   						  
							this.feedHandler.FeedsTable.Remove(f.link); 

							string newLink = propertiesDialog.textBox2.Text.Trim();
							//handle the common case of feed URI not beginning with HTTP 
							try{ 
								Uri reqUri = new Uri(newLink);
								newLink = reqUri.ToString();
							}catch(UriFormatException){

								if(!newLink.ToLower().StartsWith("http://")){
									newLink = "http://" + newLink; 
									Uri reqUri = new Uri(newLink); 
									newLink = reqUri.ToString();
								}
				
							}

							f.link = newLink; 
							this.feedHandler.FeedsTable.Add(f.link, f); 
							tn.Tag = f.link; 

							/*

							if (this.feedHandler.FeedsTable.ContainsKey(f.link)) {	// already there?
								f = (feedsFeed)feedHandler.FeedsTable[f.link];	// get this
								tn.Remove();					// remove the duplicate node
								// lookup the corresponding node
								tn = guiMain.GetTreeNodeForItem(guiMain.GetRoot(RootFolderType.MyFeeds), f);
								tn.EnsureVisible();
								tn.Tag = f.link;

							}	else {
								this.feedHandler.FeedsTable.Add(f.link, f); 					// add the changed one
							}
							*/

							refreshThisFeed = true;
						}

						if(!f.title.Equals(propertiesDialog.textBox1.Text.Trim())) {
							f.title = propertiesDialog.textBox1.Text.Trim();
							tn.Key = f.title;	// does also refresh the text
						}

					}
					
					try { 

						if((!StringHelper.EmptyOrNull(propertiesDialog.comboBox1.Text.Trim()))) {
							Int32 intIn = System.Int32.Parse(propertiesDialog.comboBox1.Text.Trim());
							if (intIn <= 0) {
								this.DisableFeed(f.link);
								feedDisabled = true;
							} else {
								tn.ImageIndex = tn.SelectedImageIndex = 4;
								intIn = intIn * MilliSecsMultiplier;
								this.feedHandler.SetRefreshRate(f.link, intIn);
								/*
								f.refreshrate = intIn;
								f.refreshrateSpecified = (this.refreshRate != intIn);// default refresh rate?
								*/
							}
						}

					}
					catch(FormatException) {
						this.MessageError("RES_FormatExceptionRefreshRate");
					}
					catch(OverflowException) {
						this.MessageError("RES_OverflowExceptionRefreshRate");
					}						

					string category = null;

					if((propertiesDialog.comboBox2.Text != null) && 
						(!propertiesDialog.comboBox2.Text.Equals(String.Empty)) && 
						(!propertiesDialog.comboBox2.Text.Equals(defaultCategory))) {

						category = propertiesDialog.comboBox2.Text.Trim();
					}

					if (category != null && !category.Equals(f.category)) {
						
						f.category = category; 
						if(!feedHandler.Categories.ContainsKey(category)) {
							feedHandler.Categories.Add(category); 
						}
						// find/create the target node:
						FeedTreeNodeBase target =	guiMain.CreateCategoryHive(guiMain.GetRoot(RootFolderType.MyFeeds), category);
						// move to new location:
						guiMain.MoveNode(tn, target);
					}
					
					if (propertiesDialog.comboMaxItemAge.Enabled) {
						if (feedMaxItemAge.CompareTo(propertiesDialog.MaxItemAge) != 0) {
							refreshThisFeed = true;
							this.feedHandler.SetMaxItemAge(f.link, propertiesDialog.MaxItemAge);
							this.FeedWasModified(f.link);
						}
					}

					if (propertiesDialog.textUser.Text != null && propertiesDialog.textUser.Text.Trim().Length != 0 ) {	// set feedsFeed new credentials
						string u = propertiesDialog.textUser.Text.Trim(), p = null;
						if (!StringHelper.EmptyOrNull(propertiesDialog.textPwd.Text))
							p = propertiesDialog.textPwd.Text.Trim();
						NewsHandler.SetFeedCredentials(f, u, p);
						refreshThisFeed = true;
					} else {
						NewsHandler.SetFeedCredentials(f, null, null);
					}

					if (!feedDisabled && (f.authUser != null || f.link.StartsWith("https")))
						tn.ImageIndex = tn.SelectedImageIndex = 9;	// image with lock 

					f.alertEnabledSpecified = f.alertEnabled = propertiesDialog.checkEnableAlerts.Checked ;
					
					if(propertiesDialog.checkMarkItemsReadOnExit.Checked != feedHandler.GetMarkItemsReadOnExit(f.link)){
						this.feedHandler.SetMarkItemsReadOnExit(f.link, propertiesDialog.checkMarkItemsReadOnExit.Checked); 
					}

					if (refreshThisFeed && !feedDisabled) {
						this.feedHandler.MarkForDownload(f);
						//guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);
					}

					if (propertiesDialog.checkCustomFormatter.Checked) {
						string stylesheet = propertiesDialog.comboFormatters.Text;

						if(!stylesheet.Equals(this.feedHandler.GetStyleSheet(f.link))){
							this.feedHandler.SetStyleSheet(f.link,stylesheet ); 						
						
							if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet)){
								this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
							}
						}
					}
					else {
						if(!String.Empty.Equals(this.feedHandler.GetStyleSheet(f.link))){
							this.feedHandler.SetStyleSheet(f.link, String.Empty);
						}
					}


					this.FeedlistModified = true;
				}


				//close dialog 
				propertiesDialog.Close(); 
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;

		}


		/// <summary>Displays the properties dialog for a category </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowCategoryProperties(ICommand sender) {


			if(guiMain.CurrentSelectedNode!= null && (guiMain.CurrentSelectedNode.Type == FeedNodeType.Category) )
			{
				FeedTreeNodeBase tn = guiMain.CurrentSelectedNode;

				string category = null, catPlusSep = null, categoryName; 
				int refreshrate = this.refreshRate;
				TimeSpan feedMaxItemAge = TimeSpan.Zero;			
				bool feedMarkItemsReadOnExit = false; 

				try 
				{

					category = guiMain.BuildCategoryStoreName(tn);				
					catPlusSep = category + tn.TreeView.PathSeparator;
					categoryName = tn.Key; 
				
					try 
					{
						refreshrate = feedHandler.GetCategoryRefreshRate(category); 
						feedMaxItemAge = feedHandler.GetCategoryMaxItemAge(category);
						feedMarkItemsReadOnExit = feedHandler.GetCategoryMarkItemsReadOnExit(category); 
					} 
					catch {/* ignore this */}

				} 
				catch (Exception e) 
				{
					this.MessageError("RES_GUIStatusErrorGeneralFeedMessage", category, e.Message);
					return;
				}

				CategoryProperties propertiesDialog = new CategoryProperties(tn.Key, refreshrate/60000, feedMaxItemAge, this.feedHandler.GetCategoryStyleSheet(category)); 
				propertiesDialog.comboMaxItemAge.Enabled = !feedMaxItemAge.Equals(TimeSpan.Zero);
				propertiesDialog.checkMarkItemsReadOnExit.Checked = feedMarkItemsReadOnExit;

				propertiesDialog.ShowDialog(guiMain);

				if(propertiesDialog.DialogResult == DialogResult.OK) 
				{
										
					if((propertiesDialog.textBox2.Text == null)||					
						propertiesDialog.textBox2.Text.Trim().Equals(String.Empty)) 
					{					
						this.MessageError("RES_GUIFieldTitleInvalid");					
					}	
					else 
					{
						
						if(!categoryName.Equals(propertiesDialog.textBox2.Text.Trim())) 
						{		
							//string oldCategory = category;
							categoryName = propertiesDialog.textBox2.Text.Trim();
							guiMain.RenameTreeNode(tn, categoryName); 
							category c = feedHandler.Categories.GetByKey(category);
							feedHandler.Categories.Remove(category);
							category = guiMain.BuildCategoryStoreName(tn);
							feedHandler.Categories.Add(category, c);
						}

					}
					
					try 
					{ 

						if((!StringHelper.EmptyOrNull(propertiesDialog.comboBox1.Text.Trim()))) 
						{							
							
							Int32 intIn = System.Int32.Parse(propertiesDialog.comboBox1.Text.Trim());
							if (intIn <= 0) 
							{																
								foreach(feedsFeed f in feedHandler.FeedsTable.Values){
									if( (f.category != null) &&  (f.category.Equals(category) || f.category.StartsWith(catPlusSep))){
										f.refreshrateSpecified = false;
										this.DisableFeed(f.link);	
									}
								}
								feedHandler.SetCategoryRefreshRate(category, 0); 
							} 
							else 
							{
								//tn.ImageIndex = tn.SelectedImageIndex = 4;
								foreach(feedsFeed f in feedHandler.FeedsTable.Values){
									if( (f.category != null) &&  (f.category.Equals(category) || f.category.StartsWith(catPlusSep))){
										f.refreshrateSpecified = false; 
										FeedTreeNodeBase t = guiMain.GetTreeNodeForItem(tn, f);
										t.ImageIndex = t.SelectedImageIndex = 4;
									}
								}

								intIn = intIn * MilliSecsMultiplier;
								feedHandler.SetCategoryRefreshRate(category, intIn); 								
							}
						}

					}
					catch(FormatException) 
					{
						this.MessageError("RES_FormatExceptionRefreshRate");
					}
					catch(OverflowException) 
					{
						this.MessageError("RES_OverflowExceptionRefreshRate");
					}						

					 								
					//TODO: Merge this loop with the one for refresh rate 
					if (propertiesDialog.comboMaxItemAge.Enabled) 
					{
						if (feedMaxItemAge.CompareTo(propertiesDialog.MaxItemAge) != 0) 
						{
							
							foreach(feedsFeed f in feedHandler.FeedsTable.Values){
								if((f.category != null) &&  (f.category.Equals(category) || f.category.StartsWith(catPlusSep))){
									f.maxitemage = null; 
								}
							}

							this.feedHandler.SetCategoryMaxItemAge(category, propertiesDialog.MaxItemAge);
							//this.FeedWasModified(f.link);
						}
					}					
				
					this.feedHandler.SetCategoryMarkItemsReadOnExit(category, propertiesDialog.checkMarkItemsReadOnExit.Checked); 					

					if (propertiesDialog.checkCustomFormatter.Checked) 
					{
						string stylesheet = propertiesDialog.comboFormatters.Text;
						this.feedHandler.SetCategoryStyleSheet(category,stylesheet ); 						
						
						if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
						{
							this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
						}
					}
					else 
					{
						this.feedHandler.SetCategoryStyleSheet(category, String.Empty);
					}

					this.FeedlistModified = true; 
				}

				//close dialog 
				propertiesDialog.Close(); 
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedNode = null;

		}

		/// <summary>
		/// Listview context menu command
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdMarkFeedItemsUnread(ICommand sender) {
			guiMain.MarkSelectedItemsLVUnread();
		}

		/// <summary>
		/// Listview context menu command
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdMarkFeedItemsRead(ICommand sender) {
			guiMain.MarkSelectedItemsLVRead();
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdPostReplyToItem(ICommand sender) {
			NewsItem item2reply = guiMain.CurrentSelectedFeedItem; 
			
			if (item2reply == null) {
				this.MessageInfo("RES_GuiStateNoFeedItemSelectedMessage");
				return;
			}

			if ((postReplyForm == null) || (postReplyForm.IsDisposed)){
				postReplyForm = new PostReplyForm(Preferences.UserIdentityForComments, new IdentityNewsServerManager(this)); 
				postReplyForm.OnPostReply += new DoPostReplyHandler(OnPostReplyFormPostReply);
			}
			
			postReplyForm.ReplyToItem = item2reply;
			
			postReplyForm.Show();	// open non-modal
			Win32.SetForegroundWindow(postReplyForm.Handle);

		}

		public void CmdBrowserGoBack(ICommand sender) {
			guiMain.RequestBrowseAction(BrowseAction.NavigateBack);
		}
		public void CmdBrowserGoForward(ICommand sender) {
			guiMain.RequestBrowseAction(BrowseAction.NavigateForward);
		}
		public void CmdBrowserCancelNavigation(ICommand sender) {
			guiMain.RequestBrowseAction(BrowseAction.NavigateCancel);
		}
		public void CmdBrowserNavigate(ICommand sender) {
			this.NavigateToUrl(guiMain.UrlText, "Web", (Control.ModifierKeys & Keys.Control) == Keys.Control, true);
		}
		public void CmdBrowserRefresh(ICommand sender) {
			guiMain.RequestBrowseAction(BrowseAction.DoRefresh);
		}
		public void CmdBrowserCreateNewTab(ICommand sender) {
			this.NavigateToUrl("about:blank", "New Browser", true, true);
		}


		/// <summary>
		/// Calling a generic listview context menu item command used 
		/// e.g. for plugin's supporting IBlogExtension
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdGenericListviewCommand(ICommand sender) {
			AppContextMenuCommand cmd = (AppContextMenuCommand)sender;
			string s = (string)cmd.Tag;
			guiMain.OnGenericListviewCommand(Int32.Parse(s.Substring(s.IndexOf(".")+1)), false);
		}
		/// <summary>
		/// Calling a generic listview context menu config item command, 
		/// e.g. for plugin's supporting IBlogExtension.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdGenericListviewCommandConfig(ICommand sender) {
			AppContextMenuCommand cmd = (AppContextMenuCommand)sender;
			string s = (string)cmd.Tag;
			guiMain.OnGenericListviewCommand(Int32.Parse(s.Substring(s.IndexOf(".")+1)), true);
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args) {
			// test only some code...
//			string s = HtmlHelper.HtmlDecode("Does it work now&#133; ?");	// Should result in "Does it work now... ?"
//		DateTime[] dta = RssHelper.InitialLastRetrievedSettings(1234, 30*60*1000);
//			foreach (DateTime d in dta)
//				Trace.WriteLine(d.ToString("u"));
			bool running = false;

			/* fails to exec on .NET 1.0
			if (!File.Exists(Path.Combine(Application.StartupPath, "RssBandit.exe.manifest" )) && Win32.IsOSAtLeastWindowsXP) {
				Application.EnableVisualStyles();
				Application.DoEvents();
			}
			*/
			
#if DEBUG && TEST_I18N_THISCULTURE			
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(new I18NTestCulture().Culture);
			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
#endif

			FormWindowState initialStartupState = Win32.GetStartupWindowState();
			// if you want to debug the minimzed startup (cannot be configured in VS.IDE),
			// comment out the line above and uncomment the next one:
			//FormWindowState initialStartupState =  FormWindowState.Minimized;

			RssBanditApplication appInstance = new RssBanditApplication();
			OtherInstanceCallback callback = new OtherInstanceCallback(appInstance.OnOtherInstance);

			running = InitialInstanceActivator.Activate(appInstance, callback, args);
			_log.Info("Application start, running instance is " + running.ToString());

			if (!running) {
				
				if (appInstance.HandleCommandLineArgs(args)) {
					if (appInstance.commandLineOptions.LocalCulture.Length > 0) {
						System.Globalization.CultureInfo current = Thread.CurrentThread.CurrentUICulture;
						try {
							Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(appInstance.commandLineOptions.LocalCulture);
							Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
						} catch (Exception ex) {
							_log.Error("Cannot switch application culture to '" + appInstance.commandLineOptions.LocalCulture + "'", ex);
							// reset
							Thread.CurrentThread.CurrentUICulture = current;
							Thread.CurrentThread.CurrentCulture = current;
						}
					}
					if (!appInstance.commandLineOptions.StartInTaskbarNotificationAreaOnly &&
						initialStartupState != FormWindowState.Minimized) {
						// no splash, if start option is tray only or minimized
						Splash.Show();
						Splash.Version = "v" + RssBanditApplication.Version;
						Splash.Status = Resource.Manager["RES_AppLoadStateLoading"];
					}

					appInstance.Init();
					appInstance.StartMainGui(initialStartupState);
					Splash.Close();

					return 0;	// OK
				} else {
					return 2;	// CommandLine error
				}

			}	else {
				return 1;		// other running instance
			}
			
		}

		// Called from other instances of the app on startup
		public void OnOtherInstance(string[] args) {
			this.commandLineOptions.SubscribeTo.Clear();
			// parse command line...
			if (this.HandleCommandLineArgs(args)) {
				
				this.CmdShowMainGui(null);

				foreach (string newFeedUrl in this.commandLineOptions.SubscribeTo) {
					if (this.guiMain != null & !this.guiMain.Disposing)
						this.guiMain.AddFeedUrlSynchronized(newFeedUrl);	 
				}
			}
		}

		public RssBanditApplication.CommandLineOptions CommandLineArgs {
			get { return this.commandLineOptions; }
		}

		/// <summary>
		/// Handle command line arguments.
		/// </summary>
		/// <param name="args">Arguments string list</param>
		/// <returns>True, if all is OK, False if further processing should stop.</returns>
		public bool HandleCommandLineArgs(string[] args) {
			
			bool retVal = true;
			CommandLineParser commandLineParser = new CommandLineParser(typeof(RssBanditApplication.CommandLineOptions));
			try {
				commandLineParser.Parse(args, this.commandLineOptions);
				if (this.commandLineOptions.ShowHelp) {
					// show Help commandline options messagebox
					MessageBox.Show (RssBanditApplication.CaptionOnly + "\n\n"+
						commandLineParser.Usage,
						RssBanditApplication.Caption + " " + "Commandline options", 
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					return false;		// display only the help, then exit
				}
				
			}
			catch (CommandLineArgumentException e) {
				Splash.Close();
				// Write logo banner if parser was created successfully
				MessageBox.Show((commandLineParser != null ? commandLineParser.LogoBanner : RssBanditApplication.CaptionOnly) + e.Message,
					RssBanditApplication.Caption, 
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				retVal = false;		// something failed
			} catch (ApplicationException e) {
				Splash.Close();
				if (e.InnerException != null && e.InnerException.Message != null) {
					MessageBox.Show(e.Message + "\n\t" + e.InnerException.Message,
						RssBanditApplication.Caption, 
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				} else {
					MessageBox.Show(e.Message,
						RssBanditApplication.Caption, 
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				retVal = false;		// something failed
			} catch (Exception e) {
				Splash.Close();
				// all other exceptions should have been caught
				MessageBox.Show("INTERNAL ERROR\n\t" + e.Message,
					RssBanditApplication.Caption, 
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				retVal = false;		// something failed
			}		
			return retVal;
		}

		private void SetGuiStateFeedback(string resourceID) {
			this.SetGuiStateFeedbackText(Resource.Manager[resourceID]);
		}
		private void SetGuiStateFeedback(string resourceID, ApplicationTrayState state) {
			this.SetGuiStateFeedbackText(Resource.Manager[resourceID], state);
		}
		private void SetGuiStateFeedbackText(string message) {
			if (this.guiMain == null || this.guiMain.Disposing) { return; }
			if (this.guiMain.InvokeRequired)
				guiMain.Invoke(new WinGuiMain.SetGuiMessageFeedbackDelegate(guiMain.SetGuiStateFeedback), new object[]{message});
			else
				this.guiMain.SetGuiStateFeedback(message);
		}
		private void SetGuiStateFeedbackText(string message, ApplicationTrayState state) {
			if (this.guiMain == null || this.guiMain.Disposing) { return; }
			if (this.guiMain.InvokeRequired)
				guiMain.Invoke(new WinGuiMain.SetGuiMessageStateFeedbackDelegate(guiMain.SetGuiStateFeedback), new object[]{message, state});
			else
				this.guiMain.SetGuiStateFeedback(message, state);
		}

		private void OnNewsItemSearchResult(object sender, NewsHandler.NewsItemSearchResultEventArgs e) {
			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new NewsHandler.NewsItemSearchResultEventHandler(this.OnNewsItemSearchResult), new object[]{sender, e});
			} else {
				bool cancel = false;
				guiMain.SearchResultAction(e.Tag, e.NewsItems, ref cancel);
				e.Cancel = cancel;
			}
		}

		private void OnSearchFinished(object sender, NewsHandler.SearchFinishedEventArgs e) {
			if (guiMain.InvokeRequired) {
				guiMain.Invoke(new NewsHandler.SearchFinishedEventHandler(this.OnSearchFinished), new object[]{sender, e});
			} else {
				guiMain.SearchFinishedAction(e.Tag, e.MatchingFeeds, e.MatchingFeedsCount, e.MatchingItemsCount);
			}
		}

		/// <summary>
		/// PostReplyForm callback
		/// </summary>
		/// <param name="postInfos"></param>
		private void OnPostReplyFormPostReply(PostReplyEventArgs postInfos) {
			
			bool success = false;

			if(postInfos.ReplyToItem != null) {

				NewsItem item2reply = postInfos.ReplyToItem;
				NewsItem item2post = null;

				string title = postInfos.Title;
				string name  = postInfos.FromName; 
				string url   = postInfos.FromUrl; 
				string email = postInfos.FromEMail; 

				string comment = String.Empty;

				XmlDocument tempDoc = new XmlDocument(); 							

				
				if (postInfos.Beautify) {// not yet active (but in next release):
					comment = postInfos.Comment.Replace("\r\n", "<br />") ;
					item2post = new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, ContentType.Html, new Hashtable(), url, item2reply.Id); 
				} else {
					comment = postInfos.Comment; 			
					item2post = new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, null, item2reply.Id); 
				}

				string commentUrl     = item2reply.CommentUrl;
				item2post.FeedDetails = item2reply.FeedDetails; 
				item2post.Author = (email == null) || (email.Trim().Length == 0) ? name : email + " (" + name + ")"; 			
				
				/* redundancy here, because Joe Gregorio changed spec now must support both <author> and <dc:creator> */				
				XmlElement emailNode = tempDoc.CreateElement("author"); 
				emailNode.InnerText  = item2post.Author; 							

				item2post.OptionalElements.Add(new XmlQualifiedName("author"), emailNode); 
				item2post.ContentType = ContentType.Html; 


				PostReplyThreadHandler prth = new PostReplyThreadHandler(this.feedHandler,commentUrl, item2post, item2reply);
				DialogResult result = prth.Start(postReplyForm, Resource.Manager["RES_GUIStatusPostReplyToItem"]);

				if (result != DialogResult.OK)
					return;

				if (!prth.OperationSucceeds) {
					this.MessageError("RES_ExceptionPostReplyToNewsItem", 
						(StringHelper.EmptyOrNull(item2reply.Title) ? item2reply.Link: item2reply.Title) , 
						prth.OperationException.Message);
					return;
				}

				this.AddSentNewsItem(item2reply, item2post);
				success = true;
			}

			if (success) {
				
				if (this.postReplyForm != null ) {
					
					this.postReplyForm.Hide();

					if (!this.postReplyForm.IsDisposed) {
						this.postReplyForm.Dispose();
					}
					this.postReplyForm = null;
				}
			} else {

				if (this.postReplyForm != null ) {
					this.postReplyForm.Show();
					Win32.SetForegroundWindow(this.postReplyForm.Handle);
				}

			}
		}

		#region global app helper

		public class CommandLineOptions  {
			public CommandLineOptions() {
			
			}
			private bool startInTaskbarNotificationAreaOnly = false;
			/// <summary>
			/// Have a look to http://blogs.gotdotnet.com/raymondc/permalink.aspx/5a811e6f-cd12-48de-8994-23409290faea,
			/// that is why we does not name it "StartInSystemTray" or such.
			/// </summary>
			[CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "taskbar", ShortName="t", Description = "RES_CmdLineStartInTaskbarDesc", DescriptionIsResourceId = true)]
			public bool StartInTaskbarNotificationAreaOnly {
				get { return startInTaskbarNotificationAreaOnly; }
				set { startInTaskbarNotificationAreaOnly = value; }
			}

			private StringCollection subscribeTo = new StringCollection();
			[DefaultCommandLineArgument(CommandLineArgumentTypes.Multiple, Name="feedUrl", Description = "RES_CmdLineSubscribeToDesc", DescriptionIsResourceId = true)]
			public StringCollection SubscribeTo {
				get { return subscribeTo; }
				set { subscribeTo = value; }
			}

			private bool showHelp;
			[CommandLineArgument(CommandLineArgumentTypes.Exclusive, Name = "help", ShortName = "h", Description = "RES_CmdLineHelpDesc", DescriptionIsResourceId = true)]
			public bool ShowHelp {
				get { return showHelp; }
				set { showHelp = value; }
			}

			private string localCulture = String.Empty;
			[CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "culture", ShortName = "c", Description = "RES_CmdLineCultureDesc", DescriptionIsResourceId = true)]
			public string LocalCulture {
				get { return localCulture; }
				set {
					localCulture = value; 
					if (StringHelper.EmptyOrNull(localCulture)) {
						localCulture = String.Empty;
					}
				}
			}
		}


		
		internal class ApplicationHelper {
			// code adopted from
			// http://www.codeproject.com/csharp/oneProcessOnly.asp
			// and used by ActivateRunningProcessInstance()

			private static IntPtr _foundWnd = IntPtr.Zero;

			/// <summary>
			/// Detect if there is an instance of your application already running.
			/// If detected, it will bring that application to the foreground 
			/// (restoring its window state if iconic), and then terminating the 
			/// current application. This is useful in instances where you want to 
			/// ensure that only one instance of your application is running.
			/// </summary>
			/// <returns>true, if another instance was found and get activated.
			/// Else false (you should continue to initialize)</returns>
			[Obsolete]public static bool ActivateRunningProcessInstance() {
				try {
					Process running = RunningInstance();	// may return null on security violation
					if (running != null) {
						
						// get the window handle 
						// (Windows 98 Platform Note:  This property is not available
						// on this platform if you started the process with 
						// ProcessStartInfo.UseShellExecute set to true
						IntPtr hWnd = running.MainWindowHandle;

						if (hWnd == IntPtr.Zero) { 
							// no window handle if minimized to tray (hidden, no main window)
							// we try the native Win32 find features
							hWnd = Win32.FindWindow(null, RssBanditApplication.CaptionOnly);
							if (hWnd == IntPtr.Zero) {
								hWnd = FindProcessWindow(running.Id);
							}
						}
						// if iconic, we need to restore the window
						if (Win32.IsIconic(hWnd)) {
							Win32.ShowWindowAsync(hWnd, (int)Win32.ShowWindowStyles.SW_RESTORE);
						}
						// bring it to the foreground
						Win32.SetForegroundWindow(hWnd);
						// state: exit our process
						return true;
					}
				} catch {;}
				// ... continue with your application initialization here.
				return false;
			}

			/// <summary>
			/// Try to find a window with a window text that looks like 
			/// the provided caption the old API way (calls EnumWindows)
			/// </summary>
			/// <param name="processId">The process id the window should belongs to. Optional.</param>
			/// <returns>The found window handle, else IntPtr.Zero</returns>
			private static IntPtr FindProcessWindow(int processId) {
				//Declare a callback delegate for EnumWindows() API call
				Win32.EnumWindowsProc ewp = new Win32.EnumWindowsProc(EvalWindow);
				// reset var modified by EnumWindows
				_foundWnd = IntPtr.Zero;
				//Enumerate all Windows
				Win32.EnumWindows(ewp, processId);
				return _foundWnd;
			}

			/// <summary>
			/// Callback routine to enum windows
			/// </summary>
			/// <param name="hWnd">Window handles</param>
			/// <param name="lParam">Here: used to contain the process Id</param>
			/// <returns></returns>
			private static bool EvalWindow(IntPtr hWnd, int lParam) {

				StringBuilder title = new StringBuilder(256);

				IntPtr pId = IntPtr.Zero;
				Win32.GetWindowThreadProcessId(hWnd, ref pId);
				if (pId.ToInt32() == lParam) {
					Win32.GetWindowText(hWnd, title, 255);
					if (RssBanditApplication.CaptionOnly.CompareTo(title.ToString()) <= 0) {
						_foundWnd = hWnd;
						return (false);
					}
				}

				return(true);

			}

			private static Process RunningInstance() {

				// Here we need System.Security.SecurityPermission for calling 
				// any members of System.Diagnostic.Process with full trust: 
				SecurityPermission secPer = new SecurityPermission(PermissionState.Unrestricted);
				try {
					secPer.Demand();
				} catch (SecurityException se) {
					_log.Debug("SecurityException occurs. We need FullTrust to access any Process class member", se);
					return null;
				}

				Process current = Process.GetCurrentProcess();
				Process[] processes = Process.GetProcessesByName (current.ProcessName);

				//Loop through the running processes in with the same name
				foreach (Process process in processes) {
					//Ignore the current process
					if (process.Id != current.Id) {
						//Make sure that the process is running from the exe file.
						if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") ==
							current.MainModule.FileName) {
							//Return the other process instance.
							return process;
						}
					}
				}
				return null;
			}
		}


		#endregion

		#region global app exception handler
		
		internal class ApplicationExceptionHandler {
			public void OnAppDomainException(object sender, UnhandledExceptionEventArgs e) {

				if (e.ExceptionObject is ThreadAbortException) {
					return;	// ignore. We catch them already in the apropriate places
				}

				// this seems to be the only place to "handle" the 
				//		System.NullReferenceException: Object reference not set to an instance of an object.
				//			at System.Net.OSSOCK.WSAGetOverlappedResult(IntPtr socketHandle, IntPtr overlapped, UInt32& bytesTransferred, Boolean wait, IntPtr ignored)
				//			at System.Net.Sockets.OverlappedAsyncResult.CompletionPortCallback(UInt32 errorCode, UInt32 numBytes,NativeOverlapped* nativeOverlapped)
				// that occurs on some systems running behind a NAT/Router/Dialer network connection.
				// See also the discussions here: 
				// http://groups.google.com/groups?hl=de&ie=UTF-8&oe=UTF-8&q=WSAGetOverlappedResult+%22Object+reference+not+set%22&sa=N&tab=wg&lr=
				// http://groups.google.com/groups?hl=de&lr=&ie=UTF-8&oe=UTF-8&threadm=7P-cnbOVWf_pEtKiXTWc-g%40speakeasy.net&rnum=4&prev=/groups%3Fhl%3Dde%26ie%3DUTF-8%26oe%3DUTF-8%26q%3DWSAGetOverlappedResult%2B%2522Object%2Breference%2Bnot%2Bset%2522%26sa%3DN%26tab%3Dwg%26lr%3D
				// http://groups.google.com/groups?hl=de&lr=&ie=UTF-8&oe=UTF-8&threadm=3fd6eba3.432257543%40news.microsoft.com&rnum=3&prev=/groups%3Fhl%3Dde%26ie%3DUTF-8%26oe%3DUTF-8%26q%3DWSAGetOverlappedResult%2B%2522Object%2Breference%2Bnot%2Bset%2522%26sa%3DN%26tab%3Dwg%26lr%3D

				if (e.ExceptionObject is NullReferenceException) {
					string message = ((Exception)e.ExceptionObject).ToString();
					if (message.IndexOf("WSAGetOverlappedResult") >= 0 && message.IndexOf("CompletionPortCallback") >= 0 )
						_log.Debug("Unhandled exception ignored: ", (Exception)e.ExceptionObject);
					return;	// ignore. See comment above :-(
				}

				DialogResult result = DialogResult.Cancel;
				Exception        ex = null;

				// The log is an extra backup in case the stack trace doesn't
				// get fully included in the exception.
				string logName = RssBanditApplication.GetLogFileName();
				try {
					ex = (Exception)e.ExceptionObject;
					result = ShowExceptionDialog(ex);
				}
				catch (Exception fatal){
					try {
						Logger.Log.Fatal("Exception on publish AppDomainException.", fatal);
						MessageBox.Show("Fatal Error: "+fatal.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}
					finally {
						Application.Exit();
					}
				}

				// Exits the program when the user clicks Abort.
				if (result == DialogResult.Abort) 
					Application.Exit();

			}

			// Creates the error message and displays it 
			public static DialogResult ShowExceptionDialog(Exception e) {
				return ShowExceptionDialog(e, false);
			}
			public static DialogResult ShowExceptionDialog(Exception e, bool resumable) {
				AppExceptions.ExceptionManager.Publish(e);
				try {
					StringBuilder errorMsg = new StringBuilder(Resource.Manager["RES_ExceptionGeneralCritical", RssBanditApplication.GetLogFileName()]);
					errorMsg.Append("\n"+e.Message);
					if (Application.MessageLoop && e.Source != null)
						errorMsg.Append("\n@:" + e.Source);
					return MessageBox.Show(errorMsg.ToString(), Resource.Manager["RES_GUIErrorMessageBoxCaption"] + " " + RssBanditApplication.Caption, (resumable ? MessageBoxButtons.AbortRetryIgnore: MessageBoxButtons.OK), MessageBoxIcon.Stop);
				} catch (Exception ex){ 
					_log.Error("Critical exception in ShowExceptionDialog() ", ex);
					/* */ 
				}
				return DialogResult.Abort;
			}

		}

		#endregion

		#region ICoreApplication Members

		string ICoreApplication.DefaultCategory
		{
			get{ return defaultCategory; }
		}
		public string[] GetCategories()
		{
			string[] cats = new string[1 + this.FeedHandler.Categories.Count];
			cats[0] = DefaultCategory;
			if (cats.Length > 1)
				this.FeedHandler.Categories.Keys.CopyTo(cats, 1);
			return cats;
		}

		public int CurrentGlobalRefreshRate{
			get {
				return this.refreshRate / MilliSecsMultiplier;
			}
		}

		public void AddCategory(string category)
		{
			if (category != null){
				category = category.Trim();
				if (category.Length > 0 && ! this.FeedHandler.Categories.ContainsKey(category))
				{
					category c = new category();
					c.Value = category;
					this.FeedHandler.Categories.Add(new CategoryEntry(category, c));
					this.guiMain.CreateCategoryHive(this.guiMain.GetRoot(RootFolderType.MyFeeds), category);
				}
					
			}
		}

		public bool SubscribeToFeed(string url, string category, string title)
		{
			AddSubscriptionWizard wiz = new AddSubscriptionWizard(this);
			wiz.FeedUrl = url;
			wiz.FeedCategory = category;
			wiz.FeedTitle = title;
			if (wiz.ShowDialog(this.guiMain) != DialogResult.Cancel)
				return true;

			return false;
		}

		bool RssBandit.AppServices.ICoreApplication.SubscribeToFeed(string url, string category)
		{
			return this.SubscribeToFeed(url, category, null);
		}

		bool RssBandit.AppServices.ICoreApplication.SubscribeToFeed(string url)
		{
			return this.SubscribeToFeed(url, DefaultCategory, null);
		}

		#endregion
	}//end class RssBanditApplication

}//end namespace RssBandit

namespace RssBandit.Exceptions {

	#region Feed-/ExceptionEventArgs classes

	/// <summary>
	/// Used to populate exceptions without throwing them. 
	/// </summary>
	public class ExceptionEventArgs: EventArgs {

		/// <summary>
		/// We define also the delegate here.
		/// </summary>
		public delegate void EventHandler(object sender, ExceptionEventArgs e);

		public ExceptionEventArgs() {;}
		public ExceptionEventArgs(Exception exception, string theErrorMessage) {
			failureException = exception;
			errorMessage		 = theErrorMessage;
		}
		private Exception failureException;
		public Exception FailureException { 
			get { return failureException; } set { failureException = value; } 
		}

		private string errorMessage;
		public string ErrorMessage { 
			get { return errorMessage; } set { errorMessage = value; } 
		}
	
	}

	/// <summary>
	/// Used to populate feed exceptions without throwing them. 
	/// </summary>
	public class FeedExceptionEventArgs: ExceptionEventArgs {

		/// <summary>
		/// We define also the delegate here.
		/// </summary>
		public new delegate void EventHandler(object sender, FeedExceptionEventArgs e);

		public FeedExceptionEventArgs() {;}
		public FeedExceptionEventArgs(Exception exception, string link, string theErrorMessage):base(exception, theErrorMessage) {
			feedLink = link;
		}

		private string feedLink;
		public string FeedLink { 
			get { return feedLink; } set { feedLink = value; } 
		}

	}
	#endregion

}
