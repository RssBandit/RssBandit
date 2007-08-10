#region CVS Version Header
/*
 * $Id: RssBanditApplication.cs,v 1.385 2007/07/28 20:11:50 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007/07/28 20:11:50 $
 * $Revision: 1.385 $
 */
#endregion

//#undef USEAUTOUPDATE
#define USEAUTOUPDATE

// Uncomment the next line to enable specific UI lang. tests.
// Then modify the returned culture ISO code within I18NTestCulture struct.
// Alternativly you can also add a commandline param '-culture:"ru-RU" to
// the project properties...

//#define TEST_I18N_THISCULTURE

#region framework namespaces
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Soap;
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Configuration;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Xml.Xsl;
using Microsoft.Win32;
#endregion

#region external namespaces

using RssBandit.WinGui.Controls;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
using Logger = RssBandit.Common.Logging;
using iTunesLib;
using WMPLib;
#endregion

#region project namespaces

using NewsComponents;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.Search;
using NewsComponents.Net;
using NewsComponents.Utils;
using NewsComponents.Storage;

using RssBandit.AppServices;
using RssBandit.Exceptions;
using RssBandit.Resources;
using RssBandit.SpecialFeeds;
using RssBandit.UIServices;
using RssBandit.Utility;
using RssBandit.WebSearch;
using RssBandit.WinGui;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Interfaces;
using System.Windows.Forms.ThListView;
#endregion

#if DEBUG && TEST_I18N_THISCULTURE			
internal struct I18NTestCulture {  public string Culture { get { return  "ru-RU"; } } };
#endif

namespace RssBandit {

	/// <summary>
	/// Summary description for WinGuiMainMediator.
	/// </summary>
	internal class RssBanditApplication: ApplicationContext, 
		ICoreApplication, IInternetService, IServiceProvider
	{

		/// <summary>
		/// additional string appended to Assembly version info
		/// </summary>
		/// <remarks>Next Final Release: remove the temp. preferences file 
		/// reading/writing before publishing!</remarks>
		private static string versionPostfix =  "ShadowCat beta"; /* String.Empty; */	// e.g. 'beta 1' or '(CVS)'
		
		private static bool validationErrorOccured = false; 
		private static RssBanditPreferences defaultPrefs = new RssBanditPreferences();
		private static Settings guiSettings = null;
		private static ServiceContainer Services = new ServiceContainer(/* no other parent, we are at top */);

		/// <summary>
		/// This is the default name for the playlist created by RSS Bandit in Windows Media Player
		/// or iTunes. 
		/// </summary>
		private const string DefaultPlaylistName = "RSS Bandit Podcasts";

		private const string DefaultPodcastFileExts = "mp3;mov;mp4;aac;aa;m4a;m4b;wma;wmv";

		private CommandMediator cmdMediator;
		private NewsHandler feedHandler;
		private NewsHandler commentFeedsHandler; 

		private WinGuiMain guiMain;
		private PostReplyForm postReplyForm;
		private RssBanditPreferences currentPrefs = null;
		private SearchEngineHandler searchEngines = null;
		private ThreadResultManager threadResultManager = null;
		private IdentityNewsServerManager identityNewsServerManager = null;
		private IAddInManager addInManager = null;

		/// <summary>
		/// Manage the channel processors working on displaying items and feeds
		/// (before we render them in the detail pane)
		/// </summary>
		private static NewsChannelServices displayingNewsChannel = new NewsChannelServices();

		/// <summary>
		/// used to share the current UI thread's UI culture on various threads
		/// </summary>
		private static CultureInfo sharedUICulture;
		/// <summary>
		/// used to share the current thread's culture on various threads
		/// </summary>
		private static CultureInfo sharedCulture;

		private static FeedColumnLayout DefaultFeedColumnLayout = new FeedColumnLayout(new string[]{"Title", "Flag", "Enclosure", "Date", "Subject"}, new int[]{ 250, 22, 22, 100, 120}, "Date", NewsComponents.SortOrder.Descending, LayoutType.GlobalFeedLayout);
		private static FeedColumnLayout DefaultCategoryColumnLayout = new FeedColumnLayout(new string[]{"Title", "Subject", "Date", "FeedTitle"}, new int[]{ 250, 120, 100, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.GlobalCategoryLayout);
		private static FeedColumnLayout DefaultSearchFolderColumnLayout = new FeedColumnLayout(new string[]{"Title", "Subject", "Date", "FeedTitle"}, new int[]{ 250, 120, 100, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.SearchFolderLayout);
		private static FeedColumnLayout DefaultSpecialFolderColumnLayout = new FeedColumnLayout(new string[]{"Title", "Subject", "Date", "FeedTitle"}, new int[]{ 250, 120, 100, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.SpecialFeedsLayout);

		private static string defaultFeedColumnLayoutKey;
		private static string defaultCategoryColumnLayoutKey;
		private static string defaultSearchFolderColumnLayoutKey;
		private static string defaultSpecialFolderColumnLayoutKey;

		private System.Threading.Timer autoSaveTimer = null;
		private bool feedlistModified = false;
		private bool commentFeedlistModified = false;
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
		private LocalFeedsFeed watchedItemsFeed; 
		private LocalFeedsFeed flaggedItemsFeed;
		private LocalFeedsFeed sentItemsFeed;
		private LocalFeedsFeed deletedItemsFeed;
		private LocalFeedsFeed unreadItemsFeed;

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
		/// <summary>
		/// make Bandit running from a stick
		/// </summary>
		private static bool portableApplicationMode;

		private static Version appVersion;	
		private static string appDataFolderPath;	
		private static string appCacheFolderPath;	
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(RssBanditApplication));

		public event EventHandler PreferencesChanged;
		public event EventHandler FeedlistLoaded;
		public event FeedDeletedHandler FeedDeleted;
		

		#region constructors and startup
		static RssBanditApplication() 
		{
			// according to http://blogs.msdn.com/shawnste/archive/2007/07/11/security-patch-breakes-some-culture-names-for-net-2-0-on-windows-xp-2003-2000.aspx
			ApplyResourceNameFix();

			// read app.config If a key was not found, take defaults from the embedded resources
			validationUrlBase  = (string)ReadAppSettingsEntry("validationUrlBase", typeof(string), SR.URL_FeedValidationBase);
			linkCosmosUrlBase  = (string)ReadAppSettingsEntry("linkCosmosUrlBase", typeof(string), SR.URL_FeedLinkCosmosUrlBase);
			bugReportUrl       = (string)ReadAppSettingsEntry("bugReportUrl", typeof(string), SR.URL_BugReport);
			webHelpUrl         = (string)ReadAppSettingsEntry("webHelpUrl", typeof(string), SR.URL_WebHelp);
			workspaceNewsUrl   = (string)ReadAppSettingsEntry("projectNewsUrl", typeof(string), SR.URL_ProjectNews);
			wikiNewsUrl        = (string)ReadAppSettingsEntry("wikiWebUrl", typeof(string), SR.URL_WikiWebNews);
			forumUrl           = (string)ReadAppSettingsEntry("userForumUrl", typeof(string), SR.URL_UserForum);
			projectDonationUrl = (string)ReadAppSettingsEntry("projectDonationUrl", typeof(string), SR.URL_ProjectDonation);
			projectDownloadUrl = (string)ReadAppSettingsEntry("projectDownloadUrl", typeof(string), SR.URL_ProjectDownload);

			// read advanced settings:
			unconditionalCommentRss = (bool)ReadAppSettingsEntry("UnconditionalCommentRss", typeof(bool), false);
			automaticColorSchemes   = (bool)ReadAppSettingsEntry("AutomaticColorSchemes", typeof(bool), true);
			NewsHandler.SetCookies  = (bool)ReadAppSettingsEntry("UseCookiesFromIE", typeof(bool), true);
			portableApplicationMode = (bool)ReadAppSettingsEntry("PortableApplicationMode", typeof(bool), false);

			// Gui Settings (Form position, layouts,...)
			guiSettings = new Settings(String.Empty);

		}

		public RssBanditApplication():base() {
			this.commandLineOptions = new RssBanditApplication.CommandLineOptions();
		}

		public void Init() {

			this.LoadTrustedCertificateIssues();
			AsyncWebRequest.OnCertificateIssue += new CertificateIssueHandler(this.OnRequestCertificateIssue);
			
//			this.feedHandler = new NewsHandler(applicationId, 
//				new FileCacheManager(RssBanditApplication.GetFeedFileCachePath() ),
//				RssBanditApplication.GetSearchIndexPath() );

			INewsComponentsConfiguration myConfig = this.CreateFeedHandlerConfiguration();
			this.feedHandler = new NewsHandler(myConfig);
			this.feedHandler.UserAgent = RssBanditApplication.UserAgent; 
			this.feedHandler.PodcastFileExtensionsAsString = DefaultPodcastFileExts;
			//this.feedHandler.PodcastFolder = this.feedHandler.EnclosureFolder = RssBanditApplication.GetEnclosuresPath();
			
			this.feedHandler.BeforeDownloadFeedStarted += new NewsHandler.DownloadFeedStartedCallback(this.BeforeDownloadFeedStarted);
			this.feedHandler.UpdateFeedsStarted += new NewsHandler.UpdateFeedsStartedHandler(this.OnUpdateFeedsStarted);
			this.feedHandler.OnUpdatedFavicon  += new NewsHandler.UpdatedFaviconCallback(this.OnUpdatedFavicon);
			this.feedHandler.OnDownloadedEnclosure += new NewsHandler.DownloadedEnclosureCallback(this.OnDownloadedEnclosure); 								

			// now handled by the ThreadResultManager:
//          this.feedHandler.UpdateFeedStarted += new NewsHandler.UpdateFeedStartedHandler(this.OnUpdateFeedStarted);
//			this.feedHandler.OnUpdatedFeed += new NewsHandler.UpdatedFeedCallback(this.OnUpdatedFeed);
//			this.feedHandler.OnUpdateFeedException += new NewsHandler.UpdateFeedExceptionCallback(this.OnUpdateFeedException);
			this.feedHandler.OnAllAsyncRequestsCompleted += new EventHandler(this.OnAllRequestsCompleted);


			/* NewsHandler for comment feeds for watched comments */ 
//			this.commentFeedsHandler = new NewsHandler(applicationId, 
//				new FileCacheManager(RssBanditApplication.GetFeedFileCachePath() ),
//				null );

			this.commentFeedsHandler = new NewsHandler(this.CreateCommentFeedHandlerConfiguration(myConfig));
			this.commentFeedsHandler.UserAgent = RssBanditApplication.UserAgent; 
			// not really needed here, but init:
			this.commentFeedsHandler.PodcastFileExtensionsAsString = DefaultPodcastFileExts;
			this.commentFeedsHandler.OnAllAsyncRequestsCompleted += new EventHandler(this.OnAllCommentFeedRequestsCompleted);			
			this.commentFeedsHandler.OnUpdatedFeed += new NewsHandler.UpdatedFeedCallback(this.OnUpdatedCommentFeed);
						
			NewsHandler.UnconditionalCommentRss = UnconditionalCommentRss;
#if DEBUG
			NewsHandler.TraceMode = true;
#endif

			this.searchEngines = new SearchEngineHandler();
			this.identityNewsServerManager = new IdentityNewsServerManager(this);
			this.addInManager = new AppInteropServices.ServiceManager();

			// Gui command handling
			this.cmdMediator = new CommandMediator();
			
			// Gui State handling (switch buttons, icons, etc.)
			this.stateManager = new GuiStateManager();
			this.stateManager.InternetConnectionStateMoved += new GuiStateManager.InternetConnectionStateMovedHandler(this.OnInternetConnectionStateChanged);
			this.stateManager.NewsHandlerBeforeStateMove += new GuiStateManager.NewsHandlerBeforeStateMoveHandler(this.OnRssParserBeforeStateChange);
			this.stateManager.NewsHandlerStateMoved += new GuiStateManager.NewsHandlerStateMovedHandler(this.OnNewsHandlerStateChanged);

			this.Preferences = DefaultPreferences;
			this.NewsItemFormatter = new NewsItemFormatter();
			this.NewsItemFormatter.TransformError += new FeedExceptionEventArgs.EventHandler(this.OnNewsItemTransformationError);
			this.NewsItemFormatter.StylesheetError += new ExceptionEventArgs.EventHandler(this.OnNewsItemFormatterStylesheetError);
			this.NewsItemFormatter.StylesheetValidationError += new ExceptionEventArgs.EventHandler(this.OnNewsItemFormatterStylesheetValidationError);
			
			this.LoadPreferences();
			this.ApplyPreferences();

			this.flaggedItemsFeed = new LocalFeedsFeed(
				RssBanditApplication.GetFlagItemsFileName(),
				SR.FeedNodeFlaggedFeedsCaption, 
				SR.FeedNodeFlaggedFeedsDesc);

			this.watchedItemsFeed = new LocalFeedsFeed(
				RssBanditApplication.GetWatchedItemsFileName(),
				SR.FeedNodeWatchedItemsCaption,
				SR.FeedNodeWatchedItemsDesc);

			this.sentItemsFeed = new LocalFeedsFeed(
				RssBanditApplication.GetSentItemsFileName(),
				SR.FeedNodeSentItemsCaption, 
				SR.FeedNodeSentItemsDesc);

			this.deletedItemsFeed = new LocalFeedsFeed(
				RssBanditApplication.GetDeletedItemsFileName(),
				SR.FeedNodeDeletedItemsCaption, 
				SR.FeedNodeDeletedItemsDesc);

			this.unreadItemsFeed = new LocalFeedsFeed(
				"virtualfeed://rssbandit.org/local/unreaditems",
				SR.FeedNodeUnreadItemsCaption, 
				SR.FeedNodeUnreadItemsDesc, 
				false);
			
			this.findersSearchRoot = this.LoadSearchFolders();

			defaultCategory = SR.FeedDefaultCategory;
			
			backgroundDiscoverFeedsHandler = new AutoDiscoveredFeedsMenuHandler(this);
			backgroundDiscoverFeedsHandler.OnDiscoveredFeedsSubscribe += new AutoDiscoveredFeedsMenuHandler.DiscoveredFeedsSubscribeCallback(this.OnBackgroundDiscoveredFeedsSubscribe);

			this.modifiedFeeds = new Queue(this.feedHandler.FeedsTable.Count + 11);

			// Create a timer that waits three minutes , then invokes every five minutes.
			autoSaveTimer = new System.Threading.Timer(new TimerCallback(this.OnAutoSave), this, 3 * MilliSecsMultiplier, 5 * MilliSecsMultiplier);
			// handle/listen to power save modes
			SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(this.OnPowerModeChanged);

			// App Update Management
			RssBanditUpdateManager.OnUpdateAvailable += new RssBanditUpdateManager.UpdateAvailableEventHandler(this.OnApplicationUpdateAvailable);

			//specify 'nntp' and 'news' URI handler
			NewsComponents.News.NntpWebRequest creator = new NewsComponents.News.NntpWebRequest(new Uri("http://www.example.com"));
			WebRequest.RegisterPrefix("nntp", creator);
			WebRequest.RegisterPrefix("news", creator);

			InitApplicationServices();

			// register build in channel processors:
			this.CoreServices.RegisterDisplayingNewsChannelProcessor(new DisplayingNewsChannelProcessor());			
		}
		
		INewsComponentsConfiguration CreateFeedHandlerConfiguration() {
			NewsComponentsConfiguration cfg = new NewsComponentsConfiguration();
			cfg.ApplicationID = RssBanditApplication.Name;
			try {
				cfg.SearchIndexBehavior = (SearchIndexBehavior)ReadAppSettingsEntry("Lucene.SearchIndexBehavior", typeof(SearchIndexBehavior), SearchIndexBehavior.Default);
			} catch (Exception configException) {
				_log.Error("Invalid Value for SearchIndexBehavior in app.config", configException);
				cfg.SearchIndexBehavior = SearchIndexBehavior.Default;
			}
			cfg.UserApplicationDataPath = ApplicationDataFolderFromEnv;
			cfg.UserLocalApplicationDataPath = ApplicationLocalDataFolderFromEnv;
			cfg.DownloadedFilesDataPath = GetEnclosuresPath();
			cfg.CacheManager = new FileCacheManager(RssBanditApplication.GetFeedFileCachePath());
			cfg.PersistedSettings = this.GuiSettings;
			return cfg;
		}
		
		INewsComponentsConfiguration CreateCommentFeedHandlerConfiguration(INewsComponentsConfiguration configTemplate) {
			NewsComponentsConfiguration cfg = new NewsComponentsConfiguration();
			cfg.ApplicationID = configTemplate.ApplicationID;
			cfg.SearchIndexBehavior = SearchIndexBehavior.NoIndexing;
			cfg.UserApplicationDataPath = configTemplate.UserApplicationDataPath;
			cfg.UserLocalApplicationDataPath = configTemplate.UserLocalApplicationDataPath;
			cfg.DownloadedFilesDataPath = null;	// no background downloads
			cfg.CacheManager = new FileCacheManager(RssBanditApplication.GetFeedFileCachePath());
			cfg.PersistedSettings = configTemplate.PersistedSettings;
			return cfg;
		}
		
		#region IServiceProvider Members and Init
		
		private void InitApplicationServices() {
			IServiceContainer top = Services;
			// allow other services to add themself to the chain:
			top.AddService(typeof(IServiceContainer), top);
			// our default topmost services:
			top.AddService(typeof(IInternetService), this);
			top.AddService(typeof(IUserPreferences), this.Preferences);
			top.AddService(typeof(ICoreApplication), this);
			top.AddService(typeof(IAddInManager), this.addInManager);
			//TODO: add all the other services we provide...
		}
		
		/// <summary>
		/// Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType">An object that specifies the type of service object to get.</param>
		/// <returns>
		/// 	<para>A service object of type <paramref name="serviceType"/>.</para>
		/// 	<para>-or-</para>
		/// 	<para>
		/// 		<see langword="null"/> if there is no service object of type <paramref name="serviceType"/>.</para>
		/// </returns>
		public object GetService(Type serviceType) {
			
			object o = Services.GetService(serviceType);
			if (o != null) return o;

			if (serviceType == typeof(IServiceContainer))
				return Services;

			return null;
		}

		#endregion
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
			Splash.Status = SR.AppLoadStateGuiLoading;
			base.MainForm = guiMain = new WinGuiMain(this, initialStartupState); // interconnect

			// thread results to UI serialization/sync.:
			this.threadResultManager = new ThreadResultManager(this, guiMain.ResultDispatcher);
			ThreadWorker.SynchronizingObject  = this.guiMain;

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
	
		internal void CheckAndLoadAddIns() {

			IAddInCollection addIns = this.addInManager.AddIns;
			if (addIns == null || addIns.Count == 0)
				return;
			foreach (IAddIn addIn in addIns) {
				if (addIn.AddInPackages == null || addIn.AddInPackages.Count == 0)
					continue;
				foreach (IAddInPackage package in addIn.AddInPackages) {
					try
					{
						package.Load(this);
					}
					catch (Exception ex)
					{
						string error = SR.AddInGeneralFailure(ex.Message, addIn.Name);
						_log.Fatal( "Failed to load IAddInPackage from AddIn: " + addIn.Name +" from '" + addIn.Location + "'", ex);
						this.MessageError(error);
					}
				}
			}
		}

		/// <summary>
		/// Cleanup task for addins
		/// </summary>
		internal void UnloadAddIns() {
			IAddInCollection addIns = this.addInManager.AddIns;
			if (addIns == null || addIns.Count == 0)
				return;
			foreach (IAddIn addIn in addIns) {
				foreach (IAddInPackage package in addIn.AddInPackages) {
					try {
						package.Unload();
					}
					catch (Exception ex)
					{
						string error = SR.AddInUnloadFailure(ex.Message, addIn.Name);
						_log.Fatal("Failed to unload IAddInPackage from AddIn: " + addIn.Name + " from '" + addIn.Location + "'", ex);
						this.MessageError(error);
					}
				}
			}
		}

//		internal IAddIn AddAndLoadAddIn(string fileName) {
//			try {
//				IAddIn newAddIn = this.addInManager.Load(fileName);
//				if (newAddIn != null) {
//					foreach (IAddInPackage package in newAddIn.AddInPackages) {
//						package.Load(this);
//					}
//					return newAddIn;
//				}
//			} catch (Exception ex) {
//				this.MessageError("RES_AddInLoadFailure", ex.Message,  fileName);
//			}
//			return null;
//		}
//
//		internal void RemoveAndUnloadAddIn(IAddIn addIn) {
//			if (addIn == null) return;
//			try {
//				foreach (IAddInPackage package in addIn.AddInPackages) {
//					package.Unload();
//				}
//				this.addInManager.Unload(addIn);
//			} catch (Exception ex) {
//				this.MessageError("RES_AddInUnloadFailure", ex.Message, addIn.Name);
//			}
//		}


		#endregion

		#region static class routines
		
		/// <summary>
		/// Gets or sets the shared UI culture.
		/// </summary>
		/// <value>The shared UI culture.</value>
		public static CultureInfo SharedUICulture {
			get { return sharedUICulture; }	
			set {
				lock(typeof(RssBanditApplication)) {
					sharedUICulture = value;
				}
			}	
		}

		/// <summary>
		/// Gets or sets the shared culture.
		/// </summary>
		/// <value>The shared culture.</value>
		public static CultureInfo SharedCulture {
			get { return sharedCulture; }	
			set {
				lock(typeof(RssBanditApplication)) {
					sharedCulture = value;
				}
			}	
		}

		/// <summary>
		/// Gets the version (no version postfix).
		/// </summary>
		/// <value>The version.</value>
		public static Version Version { 
			get {
				if (appVersion == null) {
					try {
						appVersion = Assembly.GetEntryAssembly().GetName().Version;
					} catch {
						appVersion = new Version(Application.ProductVersion);
					}
				}
				return appVersion;
			}
		}
		
		/// <summary>
		/// Gets the version (long format, incl. version postfix).
		/// </summary>
		/// <value>The version string.</value>
		public static string VersionLong { 
			get {
				Version verInfo = RssBanditApplication.Version;
				string versionStr = String.Format("{0}.{1}.{2}.{3}", 
					verInfo.Major, verInfo.Minor,
					verInfo.Build, verInfo.Revision);
				
				if (!StringHelper.EmptyOrNull(versionPostfix)) 
					return String.Format("{0} {1}", versionStr, versionPostfix);
				return versionStr;
			} 
		}

		/// <summary>
		/// Gets the version (short format, incl. version postfix).
		/// </summary>
		/// <value>The version</value>
		public static string VersionShort { 
			get {
				Version verInfo = RssBanditApplication.Version;
				string versionStr = String.Format("{0}.{1}", 
					verInfo.Major, verInfo.Minor);
				
				if (!StringHelper.EmptyOrNull(versionPostfix)) 
					return String.Format("{0} {1}", versionStr, versionPostfix);
				return versionStr;
			} 
		}
		
		
		/// <summary>
		/// Gets the application infos.
		/// </summary>
		/// <value>The application infos.</value>
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

		/// <summary>
		/// Gets the update service URL.
		/// </summary>
		/// <value>The update service URL.</value>
		public static string UpdateServiceUrl { 
			get { 
				int idx = DateTime.Now.Second % applicationUpdateServiceUrls.Length;
				return applicationUpdateServiceUrls[idx]; 
			} 
		}

		/// <summary>
		/// Gets the feed validation URL base.
		/// </summary>
		/// <value>The feed validation URL base.</value>
		public static string FeedValidationUrlBase {
			get { return validationUrlBase; }	
		}

		/// <summary>
		/// Gets the app GUID. Used by update web-service.
		/// </summary>
		/// <value>The app GUID.</value>
		public static string AppGuid { get { return applicationGuid; } }
		
		public static string Name { get { return applicationId; } }
		public static string Caption { get { return String.Format("{0} {1}", applicationName, VersionLong); } }
		public static string CaptionOnly { get { return applicationName; } }
		public static string DefaultCategory { get { return defaultCategory; } }
		
		/// <summary>
		/// Gets the user agent. Used for web-access.
		/// </summary>
		/// <value>The user agent.</value>
		public static string UserAgent { get { return String.Format("{0}/{1}", applicationId , Version); } }

		public static string GetUserPath() {
			return ApplicationDataFolderFromEnv;
		}

		public static string GetSearchesPath() {
			
			string s = Path.Combine(ApplicationDataFolderFromEnv, "searches");
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			return s;
		}

		public static string GetTemplatesPath() {

			string s = Path.Combine(Application.StartupPath, "templates");
			if(!Directory.Exists(s)) return null;
			return s;
		}

		
		public static string GetEnclosuresPath(){
			string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString();
			string s = Path.Combine(mydocs, "RSS Bandit\\My Downloaded Files");			
			return s;
		}


		public static string GetPlugInPath() {

			string s = Path.Combine(Application.StartupPath, "plugins");
			if(!Directory.Exists(s)) return null;
			return s;
		}
		
		public static string GetSearchIndexPath() {
			return Path.Combine(ApplicationLocalDataFolderFromEnv, "index");
		}

		public static string GetFeedFileCachePath() {
			
			#region old behavior

//		        // old behavior:
//				string s = ApplicationDataFolderFromEnv;
//				if(!Directory.Exists(s)) Directory.CreateDirectory(s);
//				s = Path.Combine(s, @"Cache");
//				if(!Directory.Exists(s)) 
//					Directory.CreateDirectory(s);
//				return s;

			#endregion

			if (appCacheFolderPath == null) {
				// We activated this in general to use 
				// Environment.SpecialFolder.LocalApplicationData for cache
				// to support better roaming profile performance
				string s = Path.Combine(ApplicationLocalDataFolderFromEnv, "Cache");
				
				if(!Directory.Exists(s)) {
					
					string old_cache = Path.Combine(GetUserPath(), "Cache");
					// move old content:
					if (Directory.Exists(old_cache)) 
					{
						if (s.StartsWith(old_cache)) 
						{
							_log.Error("GetFeedFileCachePath(): " + SR.CacheFolderInvalid_CannotBeMoved(s));
							Splash.Close();
							MessageBox.Show(
								SR.CacheFolderInvalid_CannotBeMoved(s), 
								RssBanditApplication.Caption, 
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
							s = old_cache;
						} else {

							try {
								string s_root_old = Directory.GetDirectoryRoot(old_cache);
								string s_root = Directory.GetDirectoryRoot(s);
								if (s_root_old == s_root) 
								{
									// fast move possible on the same drive:
									Directory.Move(old_cache, s);
								} else {
									// slower action (source on network/oher drive):
									if (!Directory.Exists(s))
										Directory.CreateDirectory(s);
									// copy files:
									foreach (string f in Directory.GetFiles(old_cache)) {
										File.Copy(f, Path.Combine(s, Path.GetFileName(f)), true);
									}
									// delete source(s):
									Directory.Delete(old_cache, true);
								}
							} catch (Exception ex) {
								_log.Error("GetFeedFileCachePath()error while moving cache folder." , ex);
								Splash.Close();
								MessageBox.Show(
									SR.CacheFolderInvalid_CannotBeMovedException(s, ex.Message), 
									RssBanditApplication.Caption, 
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
								s = old_cache;
							}
						}
					} else {
						Directory.CreateDirectory(s);
					}
				}
				appCacheFolderPath = s;
			}
			return appCacheFolderPath;
		}

		
		/// <summary>
		/// Gets the error log path.
		/// </summary>
		/// <returns></returns>
		public static string GetErrorLogPath() {
			string s = Path.Combine(ApplicationDataFolderFromEnv, "errorlog");
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			return s;
		}

		/// <summary>
		/// Gets the name of the feed error file.
		/// </summary>
		/// <returns></returns>
		public static string GetFeedErrorFileName() {
			return Path.Combine(GetErrorLogPath(), "feederrors.xml");
		}

		/// <summary>
		/// Gets the name of the flag items file.
		/// </summary>
		/// <returns></returns>
		public static string GetFlagItemsFileName() {
			return Path.Combine(GetUserPath(), "flagitems.xml");
		}
		/// <summary>
		/// Gets the name of the watched items file.
		/// </summary>
		/// <returns></returns>
		public static string GetWatchedItemsFileName() {
			return Path.Combine(GetUserPath(), "watcheditems.xml");
		}
		/// <summary>
		/// Gets the name of the sent items file.
		/// </summary>
		/// <returns></returns>
		public static string GetSentItemsFileName() {
			return Path.Combine(GetUserPath(), "replyitems.xml");
		}
		/// <summary>
		/// Gets the name of the deleted items file.
		/// </summary>
		/// <returns></returns>
		public static string GetDeletedItemsFileName() {
			return Path.Combine(GetUserPath(), "deleteditems.xml");
		}
		/// <summary>
		/// Gets the name of the search folder file.
		/// </summary>
		/// <returns></returns>
		public static string GetSearchFolderFileName() {
			return Path.Combine(GetUserPath(), "searchfolders.xml");
		}
		/// <summary>
		/// Gets the name of the shortcut settings file.
		/// </summary>
		/// <returns></returns>
		public static string GetShortcutSettingsFileName() {
			return Path.Combine(GetUserPath(), "shortcutsettings.xml");
		}
		/// <summary>
		/// Gets the name of the settings file.
		/// </summary>
		/// <returns></returns>
		public static string GetSettingsFileName() {
			string clr = String.Empty;
			if (NewsComponents.Utils.Common.ClrVersion.Major > 1)
				clr = NewsComponents.Utils.Common.ClrVersion.Major.ToString();
			return Path.Combine(GetUserPath(), ".settings"+clr+".xml");
		}
		/// <summary>
		/// Gets the name of the feed list file.
		/// </summary>
		/// <returns></returns>
		public static string GetFeedListFileName() {
			return Path.Combine(GetUserPath(), "subscriptions.xml");
		}

		/// <summary>
		/// Gets the name of the comments feed list file.
		/// </summary>
		/// <returns></returns>
		public static string GetCommentsFeedListFileName() {
			return Path.Combine(GetUserPath(), "comment-subscriptions.xml");
		}

		/// <summary>
		/// Gets the old name of the feed list file.
		/// </summary>
		/// <returns></returns>
		public static string GetOldFeedListFileName() {
			return Path.Combine(GetUserPath(), "feedlist.xml");
		}

		/// <summary>
		/// Gets the name of the trusted certificate issues file.
		/// </summary>
		/// <returns></returns>
		public static string GetTrustedCertIssuesFileName() {
			return Path.Combine(GetUserPath(), "certificates.config.xml");
		}

		/// <summary>
		/// Gets the name of the log file.
		/// </summary>
		/// <returns></returns>
		public static string GetLogFileName() {
			return Path.Combine(GetUserPath(), "error.log");
		}
		
		/// <summary>
		/// Gets the name of the file containing the information about open browser tabs
		/// when the application was last closed 
		/// </summary>
		/// <returns></returns>
		public static string GetBrowserTabStateFileName() {
			return Path.Combine(GetUserPath(), ".openbrowsertabs.xml");
		}

		/// <summary>
		/// Gets the name of the subscription tree state file.
		/// </summary>
		/// <returns></returns>
		public static string GetSubscriptionTreeStateFileName() {
			return Path.Combine(GetUserPath(), ".treestate.xml");
		}
		/// <summary>
		/// Gets the preferences file name (old binary format).
		/// </summary>
		/// <returns></returns>
		public static string GetPreferencesFileNameOldBinary() {
			return Path.Combine(GetUserPath(), ".preferences");
		}
		/// <summary>
		/// Gets the name of the preferences file.
		/// </summary>
		/// <returns></returns>
		public static string GetPreferencesFileName() {
			return Path.Combine(GetUserPath(), ".preferences.xml");
		}
		/// <summary>
		/// Gets the default preferences.
		/// </summary>
		/// <value>The default preferences.</value>
		public static RssBanditPreferences DefaultPreferences {
			get { return defaultPrefs; }
		}
		
		/// <summary>
		/// Gets a value indicating whether to use unconditional comment RSS.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [unconditional comment RSS]; otherwise, <c>false</c>.
		/// </value>
		public static bool UnconditionalCommentRss {
			get { return unconditionalCommentRss; }
		}
		/// <summary>
		/// Gets a value indicating whether to use automatic color schemes.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [automatic color schemes]; otherwise, <c>false</c>.
		/// </value>
		public static bool AutomaticColorSchemes {
			get { return automaticColorSchemes; }
		}

		/// <summary>
		/// Gets a value indicating whether [portable application mode].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [portable application mode]; otherwise, <c>false</c>.
		/// </value>
		public static bool PortableApplicationMode {
			get { return portableApplicationMode; }
		}
		
		private static string ApplicationDataFolderFromEnv {
			get {
				if (StringHelper.EmptyOrNull(appDataFolderPath)) {
					
					appDataFolderPath = ConfigurationManager.AppSettings["AppDataFolder"];
					if (!StringHelper.EmptyOrNull(appDataFolderPath)) {
						appDataFolderPath = Environment.ExpandEnvironmentVariables(appDataFolderPath);
					} else {
						try {	// once
							appDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), RssBanditApplication.Name);
							if (!Directory.Exists(appDataFolderPath))
								Directory.CreateDirectory(appDataFolderPath);
						} catch (SecurityException secEx) {
							MessageBox.Show ("Cannot query for Environment.SpecialFolder.ApplicationData:\n"+secEx.Message, "Security violation");
							Application.Exit();
						}
					}
					// expand a relative path to be relative to the executable:
					if (!Path.IsPathRooted(appDataFolderPath))
						appDataFolderPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), appDataFolderPath);
					if (-1 == Path.GetPathRoot(appDataFolderPath).IndexOf(":"))	// we have to cut the leading slash off (Path.Combine don't like it):
						appDataFolderPath = Path.Combine(Path.GetPathRoot(Application.ExecutablePath), appDataFolderPath.Substring(1));
					if (!Directory.Exists(appDataFolderPath))
						Directory.CreateDirectory(appDataFolderPath);
				}

				return appDataFolderPath;
			}
		}

		private static string ApplicationLocalDataFolderFromEnv {
			get {
				string s = ConfigurationManager.AppSettings["AppCacheFolder"];
				if (!StringHelper.EmptyOrNull(s)) {
					s = Environment.ExpandEnvironmentVariables(s);
				} else {
					// We changed this in general to Environment.SpecialFolder.LocalApplicationData
					// to support better roaming perf. for windows roaming profiles.
					// but reqires a upgrade path to move existing cache content to the new location...
					try {	// once
						s = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), RssBanditApplication.Name);
					} catch (SecurityException secEx) {
						MessageBox.Show ("Cannot query for Environment.SpecialFolder.LocalApplicationData:\n"+secEx.Message, "Security violation");
						Application.Exit();
					}
				}

				// expand a relative path to be relative to the executable:
				if (!Path.IsPathRooted(s))
					s = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), s);
				if (-1 == Path.GetPathRoot(s).IndexOf(":"))
					appDataFolderPath = Path.Combine(Path.GetPathRoot(Application.ExecutablePath), s.Substring(1));
				if(!Directory.Exists(s)) 
					Directory.CreateDirectory(s);
				
				return s;
			}
		}


		/// <summary>
		/// Handles errors that occur during schema validation of RSS feed list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void CommentFeedListValidationCallback(object sender,
			System.Xml.Schema.ValidationEventArgs args) {

			if(args.Severity == System.Xml.Schema.XmlSeverityType.Warning) {
				_log.Info(RssBanditApplication.GetCommentsFeedListFileName() + " validation warning: " + args.Message);
			} 
			else if(args.Severity == System.Xml.Schema.XmlSeverityType.Error) {				
				_log.Error(RssBanditApplication.GetCommentsFeedListFileName() + " validation error: " + args.Message);
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
		/// Ensures that there is a 'Subscribe in RSS Bandit' menu option. Also if we 
		/// are the default aggregator, we remove the option to subscribe in the default 
		/// aggregator.			
		/// </summary>
		public static void CheckAndRegisterIEMenuExtensions() {
			try {

				//if we are the default aggregator then remove that menu option since it is redundant
				if (Win32.Registry.IsInternetExplorerExtensionRegistered(Win32.IEMenuExtension.DefaultFeedAggregator))
					Win32.Registry.UnRegisterInternetExplorerExtension(Win32.IEMenuExtension.DefaultFeedAggregator);
				
				if (!Win32.Registry.IsInternetExplorerExtensionRegistered(Win32.IEMenuExtension.Bandit))
					Win32.Registry.RegisterInternetExplorerExtension(Win32.IEMenuExtension.Bandit);
		
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
			[DebuggerStepThrough]
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
			if (!IsFormAvailable(guiMain)) 
				return;

			if (guiMain.InvokeRequired) {
				// call synchronized (dialog shown, and we need the result in the cancel event args:
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
			if (!IsFormAvailable(guiMain)) 
				return;

			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.UpdateFeedsStartedHandler(this.OnUpdateFeedsStarted), new object[]{sender, e});
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
			if (!IsFormAvailable(guiMain)) 
				return;
			
			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.DownloadFeedStartedCallback(this.BeforeDownloadFeedStarted), new object[]{sender, e});
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
		internal void OnUpdateFeedStarted(object sender, NewsHandler.UpdateFeedEventArgs e) {
			if (!IsFormAvailable(guiMain)) 
				return;
			
			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.UpdateFeedStartedHandler(this.OnUpdateFeedStarted), new object[]{sender, e});
			} else {
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOne);
			}
		}

		/// <summary>
		/// Called by RssParser, after a feed was updated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnUpdatedFeed(object sender, NewsHandler.UpdatedFeedEventArgs e) {
			if (!IsFormAvailable(guiMain)) 
				return;
			
			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.UpdatedFeedCallback(this.OnUpdatedFeed), new object[]{sender, e});
			} else {
				guiMain.UpdateFeed(e.UpdatedFeedUri, e.NewFeedUri, e.UpdateState == NewsComponents.Net.RequestResult.OK);
				
				if(e.FirstSuccessfulDownload){
					//new <cacheurl> entry in subscriptions.xml 
					this.SubscriptionModified(NewsFeedProperty.FeedCacheUrl);
					//this.FeedlistModified = true; 
				} 
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
			}
		}

		/// <summary>
		/// Called by RssParser, after a comment feed was updated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnUpdatedCommentFeed(object sender, NewsHandler.UpdatedFeedEventArgs e) {
			if (!IsFormAvailable(guiMain)) 
				return;
			
			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.UpdatedFeedCallback(this.OnUpdatedCommentFeed), new object[]{sender, e});
			} else {
				if(e.UpdateState == NewsComponents.Net.RequestResult.OK){
					guiMain.UpdateCommentFeed(e.UpdatedFeedUri, e.NewFeedUri);
				}
			}
		}

		/// <summary>
		/// Called by NewsHandler, after a favicon was updated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnUpdatedFavicon(object sender, NewsHandler.UpdatedFaviconEventArgs e) {
			if (!IsFormAvailable(guiMain)) 
				return;
			
			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.UpdatedFaviconCallback(this.OnUpdatedFavicon), new object[]{sender, e});
			} else {
				guiMain.UpdateFavicon(e.Favicon, e.FeedUrls);
			}
		}

		/// <summary>
		/// Called by NewsHandler, after an enclosure has been downloaded.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnDownloadedEnclosure(object sender, DownloadItemEventArgs e) {

			/* create playlists in media players if that option is selected */ 
			if(this.Preferences.AddPodcasts2WMP){
				this.AddPodcastToWMP(e.DownloadItem);
			}

			if(this.Preferences.AddPodcasts2ITunes){
				this.AddPodcastToITunes(e.DownloadItem);
			}			

			/* update GUI if needed */ 
			if (!IsFormAvailable(guiMain)) 
				return;
			
			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.DownloadedEnclosureCallback(guiMain.OnEnclosureReceived), new object[]{sender, e});
			} else {								
				guiMain.OnEnclosureReceived(sender, e); 			
			}
		}

		/// <summary>
		/// Called by RssParser, if update of a feed caused an exception
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnUpdateFeedException(object sender, NewsHandler.UpdateFeedExceptionEventArgs e) {
			if (!IsFormAvailable(guiMain)) 
				return;

			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new NewsHandler.UpdateFeedExceptionCallback(this.OnUpdateFeedException), new object[]{sender, e});
			} else {
				WebException webex = e.ExceptionThrown as WebException;
				if (webex != null) {	// yes, WebException
					if (webex.Status == WebExceptionStatus.NameResolutionFailure || 
						webex.Status == WebExceptionStatus.ProxyNameResolutionFailure) {// connection lost?
						this.UpdateInternetConnectionState(true);	// update connect state
						if (!this.InternetAccessAllowed) {							
							guiMain.UpdateFeed(e.FeedUri, null, false);
							stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
							return;
						}
					}
				}
				Trace.WriteLine(e.ExceptionThrown.StackTrace); 
				this.UpdateXmlFeedErrorFeed(e.ExceptionThrown, e.FeedUri, true);
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
			}
		}

		/// <summary>
		/// Called by RssParser, if all pending comment feed updates are done.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAllCommentFeedRequestsCompleted(object sender, EventArgs e) {
			if (!IsFormAvailable(guiMain)) 
				return;

			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new EventHandler(this.OnAllCommentFeedRequestsCompleted), new object[]{sender, e});
			} else {				
				guiMain.OnAllAsyncUpdateCommentFeedsFinished();
				GC.Collect();
			}
		}

		/// <summary>
		/// Called by RssParser, if all pending feed updates are done.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAllRequestsCompleted(object sender, EventArgs e) {
			if (!IsFormAvailable(guiMain)) 
				return;

			if (guiMain.InvokeRequired) {
				guiMain.BeginInvoke(new EventHandler(this.OnAllRequestsCompleted), new object[]{sender, e});
			} else {
				stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshAllDone);
				guiMain.TriggerGUIStateOnNewFeeds(true);
				guiMain.OnAllAsyncUpdateFeedsFinished();
				GC.Collect();
			}
		}
		#endregion

		#region Task management
		
		ThreadWorkerTask MakeTask(ThreadWorker.Task task, ThreadWorkerProgressHandler handler, params object[] args) {
			return new ThreadWorkerTask(task, handler, this, args);
		}
		internal void MakeAndQueueTask(ThreadWorker.Task task, ThreadWorkerProgressHandler handler, params object[] args) {
			QueueTask(MakeTask(task, handler, args));
		}
		internal void MakeAndQueueTask(ThreadWorker.Task task, ThreadWorkerProgressHandler handler, ThreadWorkerBase.DuplicateTaskQueued duplicate, params object[] args) {
			QueueTask(MakeTask(task, handler, args), duplicate);
		}
		void QueueTask(ThreadWorkerTask task) {
			ThreadWorker.QueueTask(task);
		}
		void QueueTask(ThreadWorkerTask task, ThreadWorkerBase.DuplicateTaskQueued duplicate) {
			ThreadWorker.QueueTask(task, duplicate);
		}

		public void BeginLoadingFeedlist() {
			MakeAndQueueTask(ThreadWorker.Task.LoadFeedlist, new ThreadWorkerProgressHandler(this.OnLoadingFeedlistProgress));
//			ThreadWorkerProgressHandler handler = new ThreadWorkerProgressHandler(this.OnLoadingFeedlistProgress);
//			ThreadWorker.StartTask(ThreadWorker.Task.LoadFeedlist, this.guiMain, handler, this, null);
		}

		private void OnLoadingFeedlistProgress(object sender, ThreadWorkerProgressArgs args) {
			if (args.Exception != null) {
				// failure(s)
				args.Cancel = true;
				BanditApplicationException ex = args.Exception as BanditApplicationException;
				if (ex != null) {
					if (ex.Number == ApplicationExceptions.FeedlistOldFormat) {
						Application.Exit();
					} else if (ex.Number == ApplicationExceptions.FeedlistOnRead) {
						AppExceptions.ExceptionManager.Publish(ex.InnerException);
						this.MessageError(SR.ExceptionReadingFeedlistFile(ex.InnerException.Message, RssBanditApplication.GetLogFileName()));
						this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
					} else if (ex.Number == ApplicationExceptions.FeedlistOnProcessContent) {
						this.MessageError(SR.InvalidFeedlistFileMessage(RssBanditApplication.GetLogFileName()));
						this.SetGuiStateFeedbackText(SR.GUIStatusValidationErrorReadingFeedlistFile);
					} else if (ex.Number == ApplicationExceptions.FeedlistNA) {
						this.refreshRate = feedHandler.RefreshRate;
						this.SetGuiStateFeedbackText(SR.GUIStatusNoFeedlistFile);
					} else {
						RssBanditApplication.PublishException(args.Exception);
						this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
					}
				} else {	// unhandled
					RssBanditApplication.PublishException(args.Exception);
					this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
				}
			} else if (!args.Done) {
				// in progress
				if (!IsFormAvailable(guiMain)) { args.Cancel = true; return; }
				this.SetGuiStateFeedbackText(SR.GUIStatusLoadingFeedlist);
			} else if (args.Done) {
				// done
				this.refreshRate = feedHandler.RefreshRate;	// loaded from feedlist
				
				this.CheckAndMigrateSettingsAndPreferences();		// needs the feedlist to be loaded
				this.CheckAndMigrateListViewLayouts();

				//resume pending enclosure downloads
				this.feedHandler.ResumePendingDownloads();

				if (!IsFormAvailable(guiMain)) { args.Cancel = true; return; }

				try {
					this.guiMain.PopulateFeedSubscriptions(feedHandler.Categories, feedHandler.FeedsTable, RssBanditApplication.DefaultCategory);
				} catch (Exception ex) {
					RssBanditApplication.PublishException(ex);
				}
				
				if (FeedlistLoaded != null)
					FeedlistLoaded(this, EventArgs.Empty);

				this.SetGuiStateFeedbackText(SR.GUIStatusDone);

				//TODO: move this out of the Form code to allow dynamic create/dispose of the main form from the system tray menu
				foreach (string newFeedUrl in this.commandLineOptions.SubscribeTo) {
					if (IsFormAvailable(guiMain)) 
						this.guiMain.AddFeedUrlSynchronized(newFeedUrl);	 
				}

				// start load items and refresh from web, if we have to refresh on startup:
				guiMain.UpdateAllFeeds(this.Preferences.FeedRefreshOnStartup);

			}
		}

		public void BeginLoadingSpecialFeeds() {
			MakeAndQueueTask(ThreadWorker.Task.LoadSpecialFeeds, new ThreadWorkerProgressHandler(this.OnLoadingSpecialFeedsProgress));
//			ThreadWorkerProgressHandler handler = new ThreadWorkerProgressHandler(this.OnLoadingSpecialFeedsProgress);
//			ThreadWorker.StartTask(ThreadWorker.Task.LoadSpecialFeeds, this.guiMain, handler, this);
		}

		private void OnLoadingSpecialFeedsProgress(object sender, ThreadWorkerProgressArgs args) {
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
			//if (this.InternetAccessAllowed) {
				// handled via NewsHander.Offline flag
				this.StateHandler.MoveNewsHandlerStateTo(forceDownload ? NewsHandlerState.RefreshAllForced: NewsHandlerState.RefreshAllAuto);
				MakeAndQueueTask(ThreadWorker.Task.RefreshFeeds, new ThreadWorkerProgressHandler(this.OnRefreshFeedsProgress), forceDownload);
			//}
		}

		public void BeginRefreshCommentFeeds(bool forceDownload) {
			//if (this.InternetAccessAllowed) {
			// handled via NewsHander.Offline flag
			MakeAndQueueTask(ThreadWorker.Task.RefreshCommentFeeds, new ThreadWorkerProgressHandler(this.OnRefreshFeedsProgress), forceDownload);
			//}
		}

		public void BeginRefreshCategoryFeeds(string category, bool forceDownload) {
			//if (this.InternetAccessAllowed) {
				// handled via NewsHander.Offline flag
				this.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshCategory);
				MakeAndQueueTask(ThreadWorker.Task.RefreshCategoryFeeds, new ThreadWorkerProgressHandler(this.OnRefreshFeedsProgress), category, forceDownload);
			//}
		}

		private void OnRefreshFeedsProgress(object sender, ThreadWorkerProgressArgs args) {
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

		#region Podcast related routines 

		/// <summary>
		/// Gets the current Enclosure folder
		/// </summary>		
		public string EnclosureFolder{
		
			get { return this.feedHandler.EnclosureFolder; }
		}		


		/// <summary>
		/// Gets the current Podcast folder
		/// </summary>		
		public string PodcastFolder{
		
			get { return this.feedHandler.PodcastFolder; }
		}	

		/// <summary>
		/// Indicates the number of enclosures which should be downloaded automatically from a newly subscribed feed.
		/// </summary>
		public  int NumEnclosuresToDownloadOnNewFeed { 
			get{ return this.feedHandler.NumEnclosuresToDownloadOnNewFeed;}  
			 
		}


		/// <summary>
		/// Indicates the maximum amount of space that enclosures and podcasts can use on disk.
		/// </summary>
		public  int EnclosureCacheSize { 
			get{ return this.feedHandler.EnclosureCacheSize;}  
		}

		/// <summary>
		/// Gets a semi-colon delimited list of file extensions of enclosures that 
		/// should be treated as podcasts
		/// </summary>
		public string PodcastFileExtensions { 
			get{ return this.feedHandler.PodcastFileExtensionsAsString; }
		}

		/// <summary>
		/// Gets whether enclosures should be created in a subfolder named after the feed. 
		/// </summary>
		public bool DownloadCreateFolderPerFeed  {
		
			get { return this.feedHandler.CreateSubfoldersForEnclosures;}		
		}
		
		/// <summary>
		/// Gets whether alert Windows should be displayed for enclosures or not. 
		/// </summary>
		public bool EnableEnclosureAlerts  {
		
			get { return this.feedHandler.EnclosureAlert;}		
		}
		
		/// <summary>
		/// Gets whether enclosures should be downloaded automatically or not.
		/// </summary>
		public bool DownloadEnclosures { 
		
			get { return this.feedHandler.DownloadEnclosures;}		
		}

		/// <summary>
		/// Tests whether a file type is supported by Windows Media Player by checking the 
		/// file extension. 
		/// </summary>
		/// <param name="fileExt">The file extension to test</param>
		/// <returns>True if the file extension is supported by Windows Media Player</returns>
		private static bool IsWMPFile(string fileExt){		

			if(fileExt.Equals(".asf") || fileExt.Equals(".wma") || fileExt.Equals(".avi")
				|| fileExt.Equals(".mpg") || fileExt.Equals(".mpeg") || fileExt.Equals(".m1v") 
				|| fileExt.Equals(".wmv") || fileExt.Equals(".wm") || fileExt.Equals(".asx") 
				|| fileExt.Equals(".wax") || fileExt.Equals(".wpl") || fileExt.Equals(".wvx") 
				|| fileExt.Equals(".wmd") || fileExt.Equals(".dvr-ms") || fileExt.Equals(".m3u") 
				|| fileExt.Equals(".mp3") || fileExt.Equals(".mp2") || fileExt.Equals(".mpa") 
				|| fileExt.Equals(".mpe") || fileExt.Equals(".mpv2") || fileExt.Equals(".wms") 
				|| fileExt.Equals(".mid") || fileExt.Equals(".midi") || fileExt.Equals(".rmi")
				|| fileExt.Equals(".aif") || fileExt.Equals(".aifc") || fileExt.Equals(".aiff")
				|| fileExt.Equals(".wav") || fileExt.Equals(".au") || fileExt.Equals(".snd")
				|| fileExt.Equals(".ivf") || fileExt.Equals(".wmz")){
				return true;
			}else{
				return false;
			}
		}

		/// <summary>
		/// Adds the downloaded item to a playlist in Windows Media Player. 
		/// </summary>
		/// <remarks>The title of the playlist is the name of the feed in RSS Bandit.</remarks>
		/// <param name="podcast"></param>
		private void AddPodcastToWMP(DownloadItem podcast){
			try{
			
				if(!IsWMPFile(Path.GetExtension(podcast.File.LocalName))){
					return; 
				}

				string playlistName = this.Preferences.SinglePlaylistName; 

				if(!this.Preferences.SinglePodcastPlaylist && this.feedHandler.FeedsTable.Contains(podcast.OwnerFeedId)){
					playlistName = this.feedHandler.FeedsTable[podcast.OwnerFeedId].title;
				}

				WindowsMediaPlayer wmp = new WindowsMediaPlayer();			

				//get a handle to the playlist if it exists or create it if it doesn't				
				IWMPPlaylist podcastPlaylist = null; 
				IWMPPlaylistArray playlists = wmp.playlistCollection.getAll();

				for(int i = 0; i < playlists.count; i++){					
					IWMPPlaylist pl = playlists.Item(i); 

					if(pl.name.Equals(playlistName)){
						podcastPlaylist =  pl; 
					}
				}

				if(podcastPlaylist == null){
					podcastPlaylist = wmp.playlistCollection.newPlaylist(playlistName); 
				}

				IWMPMedia wm = wmp.newMedia(Path.Combine(podcast.TargetFolder, podcast.File.LocalName)); 				
				podcastPlaylist.appendItem(wm);				
								
			}catch(Exception e){
				_log.Error("The following error occured in AddPodcastToWMP(): ", e);
			}		
		}

		/// <summary>
		/// Tests whether a file type is supported by iTunesby checking the 
		/// file extension. 
		/// </summary>
		/// <param name="fileExt">The file extension to test</param>
		/// <returns>True if the file extension is supported by iTunes</returns>
		private static bool IsITunesFile(string fileExt){
		
			if(fileExt.Equals(".mov") || fileExt.Equals(".mp4") || fileExt.Equals(".mp3")
				|| fileExt.Equals(".m4v") || fileExt.Equals(".m4a") || fileExt.Equals(".m4b") 
				|| fileExt.Equals(".m4p") || fileExt.Equals(".wav") || fileExt.Equals(".aiff")
				|| fileExt.Equals(".aif") || fileExt.Equals(".aifc")|| fileExt.Equals(".aa")){
				return true;
			}else{
				return false;
			}
		}

		/// <summary>
		/// Adds the downloaded item to a playlist in iTunes. 
		/// </summary>
		/// <remarks>The title of the playlist is the name of the feed in RSS Bandit.</remarks>
		/// <param name="podcast"></param>
		private void AddPodcastToITunes(DownloadItem podcast){
			try{

				if(!IsITunesFile(Path.GetExtension(podcast.File.LocalName))){
					return; 
				}

				string playlistName = this.Preferences.SinglePlaylistName; 

				if(!this.Preferences.SinglePodcastPlaylist && this.feedHandler.FeedsTable.Contains(podcast.OwnerFeedId)){
					playlistName = this.feedHandler.FeedsTable[podcast.OwnerFeedId].title;
				}

				// initialize iTunes application connection
				iTunesApp itunes = new iTunesLib.iTunesApp();		
							
				//get a handle to the playlist if it exists or create it if it doesn't				
				IITUserPlaylist podcastPlaylist = null; 
			
				foreach (IITPlaylist pl in itunes.LibrarySource.Playlists) {
					if(pl.Name.Equals(playlistName)){
						podcastPlaylist =  (IITUserPlaylist) pl; 
					}
				}

				if(podcastPlaylist == null){
					podcastPlaylist = (IITUserPlaylist) itunes.CreatePlaylist(playlistName); 
				}
				
				//add podcast to our playlist for this feed							
				podcastPlaylist.AddFile(Path.Combine(podcast.TargetFolder, podcast.File.LocalName)); 				
				
			}catch(Exception e){
				_log.Error("The following error occured in AddPodcastToITunes(): ", e);
			}
		}

		#endregion

		public NewsHandler FeedHandler { get { return this.feedHandler; } }

		public NewsHandler CommentFeedsHandler { get { return this.commentFeedsHandler; } }


		public RssBanditPreferences Preferences { 
			get { return currentPrefs;  } 
			set { currentPrefs = value; } 
		}

		public IdentityNewsServerManager IdentityManager { 
			get { return identityNewsServerManager;  } 
		}
		public IdentityNewsServerManager NntpServerManager { 
			get { return identityNewsServerManager;  } 
		}

		/// <summary>
		/// Notification method about a setting that was modified,
		/// relevant to the subscriptions.
		/// </summary>
		/// <param name="property">The property.</param>
		public void SubscriptionModified(NewsFeedProperty property) { 
			HandleSubscriptionRelevantChange(property);
		}

		/// <summary>
		/// Notification method about a feed that was modified.
		/// </summary>
		/// <param name="feed">The feed.</param>
		/// <param name="property">The property.</param>
		public void FeedWasModified(feedsFeed feed, NewsFeedProperty property) 
		{ 
			HandleSubscriptionRelevantChange(property);

			if (feed == null)
				return;

			HandleFeedCacheRelevantChange(feed.link, property);
			HandleIndexRelevantChange(feed, property);
		}

		/// <summary>
		/// Notification method about a feed that was modified.
		/// </summary>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="property">The property.</param>
		public void FeedWasModified(string feedUrl, NewsFeedProperty property) 
		{ 
			HandleSubscriptionRelevantChange(property);
			
			if (StringHelper.EmptyOrNull(feedUrl))
				return;
			
			HandleFeedCacheRelevantChange(feedUrl, property);
			HandleIndexRelevantChange(feedUrl, property);
		}

		private void HandleSubscriptionRelevantChange(NewsFeedProperty property) {
			if (this.feedHandler.IsSubscriptionRelevantChange(property)) 
				this.feedlistModified = true;
		}

		private void HandleFeedCacheRelevantChange(string feedUrl, NewsFeedProperty property) {
			if (StringHelper.EmptyOrNull(feedUrl))
				return;
			if (this.feedHandler.IsCacheRelevantChange(property)) {
				// we queue up the cache file refresh requests:
				lock (modifiedFeeds) {
					if (!modifiedFeeds.Contains(feedUrl))
						modifiedFeeds.Enqueue(feedUrl);
				}
			}
		}
		
		private void HandleIndexRelevantChange(string feedUrl, NewsFeedProperty property) {
			if (StringHelper.EmptyOrNull(feedUrl))
				return;
			if (this.feedHandler.SearchHandler.IsIndexRelevantChange(property)) 
				HandleIndexRelevantChange(GetFeed(feedUrl), property);
		}

		private void HandleIndexRelevantChange(feedsFeed feed, NewsFeedProperty property) {
			if (feed == null)
				return;
			if (this.feedHandler.SearchHandler.IsIndexRelevantChange(property)) 
			{
				if (NewsFeedProperty.FeedRemoved == (property & NewsFeedProperty.FeedRemoved)) 
				{
					this.feedHandler.SearchHandler.IndexRemove(feed.id);
					// feed added change is handled after first sucessful request of the feed:
				} else if (NewsFeedProperty.FeedAdded == (property & NewsFeedProperty.FeedAdded)) {
					this.feedHandler.SearchHandler.ReIndex(feed);
				}
			}
		}

		/// <summary>
		/// Gets the feedsFeed from FeedHandler.
		/// </summary>
		/// <param name="feedUrl">The feed URL (can be null).</param>
		/// <returns>feedsFeed if found, else null</returns>
		public feedsFeed GetFeed(string feedUrl) {
			if (StringHelper.EmptyOrNull(feedUrl)) 
				return null;
			if (this.feedHandler.FeedsTable.ContainsKey(feedUrl))
				return this.feedHandler.FeedsTable[feedUrl];
			return null;
		}

		/// <summary>
		/// Gets the feed info (IFeedDetails).
		/// </summary>
		/// <param name="feedUrl">The feed URL (can be null).</param>
		/// <returns>IFeedDetails if found, else null</returns>
		public IFeedDetails GetFeedInfo(string feedUrl) {
			if (StringHelper.EmptyOrNull(feedUrl)) 
				return null;
			if (this.feedHandler.FeedsTable.ContainsKey(feedUrl))
				return this.feedHandler.GetFeedInfo(feedUrl);
			return null;
		}

		public SearchEngineHandler SearchEngineHandler {
			get { return searchEngines; }
		}

		public Settings GuiSettings { 
			get { return guiSettings;  } 
		}

		/// <summary>
		/// Gets or sets the last auto update check date.
		/// </summary>
		/// <value>Date of the last auto update check.</value>
		public DateTime LastAutoUpdateCheck {
			get {
				return (DateTime)GuiSettings.GetProperty("Application.LastAutoUpdateCheck", DateTime.MinValue, typeof(DateTime));
			}
			set {
				GuiSettings.SetProperty("Application.LastAutoUpdateCheck", value);
				GuiSettings.Flush();
			}
//			get { return this.Preferences.LastAutoUpdateCheck; }
//			set { this.Preferences.LastAutoUpdateCheck = value; SavePreferences(); }
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

		public event AppServices.InternetConnectionStateChangeHandler InternetConnectionStateChange;

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
		/// Property CommentFeedlistModified (bool)
		/// </summary>
		public bool CommentFeedlistModified {
			get { return this.commentFeedlistModified;  }
			set { this.commentFeedlistModified = value; }
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
				
				//NewsComponents.Collections.FeedColumnLayoutCollection myLayouts = new NewsComponents.Collections.FeedColumnLayoutCollection();
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
			internal List<string> _columns;
			internal List<int> _columnWidths;
			private bool _modified;

			public ListViewLayout():this(null, null, null, NewsComponents.SortOrder.None) {	}
			public ListViewLayout(ICollection<string> columns, ICollection<int> columnWidths, string sortByColumn, NewsComponents.SortOrder sortOrder) {
				if (columns != null)
					_columns = new List<string>(columns);
				else
					_columns = new List<string>();
				if (columnWidths != null)
					_columnWidths = new List<int>(columnWidths);
				else
					_columnWidths = new List<int>();
				_sortByColumn = sortByColumn;
				_sortOrder = sortOrder;
			}

			public static ListViewLayout CreateFromXML(string xmlString) {
				if (xmlString != null && xmlString.Length > 0) {
					XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof(ListViewLayout));
					StringReader reader = new StringReader(xmlString);
					return (ListViewLayout)formatter.Deserialize(reader);
				}
				return null;
			}

			public static string SaveAsXML(ListViewLayout layout) {
				if (layout == null)
					return null;
				try {
					XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof(ListViewLayout));
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
			public IList<string> Columns {
				get {	return _columns;	}
				set { 
					if (value != null)
						_columns = new List<string>(value); 
					else
						_columns = new List<string>();
				}
			}

			[XmlIgnore]
			public IList<int> ColumnWidths {
				get {	return _columnWidths;	}
				set { 
					if (value != null)
						_columnWidths = new List<int>(value); 
					else
						_columnWidths = new List<int>();
				}
			}

			[XmlIgnore]
			public bool Modified {
				get {	return _modified;	}
				set {	_modified = value;	}
			}

			#endregion

			[XmlArrayItem(typeof(string))]
			public List<string> ColumnList {
				get {	return _columns;	}
				set { 
					if (value != null)
						_columns = value; 
					else
						_columns = new List<string>();
				}
			}
			[XmlArrayItem(typeof(int))]
			public List<int> ColumnWidthList {
				get {	return _columnWidths;	}
				set { 
					if (value != null)
						_columnWidths = value; 
					else
						_columnWidths = new List<int>();
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

		#region MessageQuestion()/-Info()/-Error()

		/// <summary>
		/// Displays a Message box as a question.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		/// <remarks>Ensures, it gets displayed in foreground and 
		/// use the main form as the parent</remarks>
		public DialogResult MessageQuestion(string text) {
			return MessageQuestion(text, null);
		}

		/// <summary>
		/// Displays a Message box as a question.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="captionPostfix">The caption postfix, appended to the appl. caption.</param>
		/// <returns></returns>
		/// <remarks>Ensures, it gets displayed in foreground and
		/// use the main form as the parent</remarks>
		public DialogResult MessageQuestion(string text, string captionPostfix) {
			if (MainForm != null && MainForm.IsHandleCreated)
				Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm, text,
				RssBanditApplication.CaptionOnly + captionPostfix, 
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);
		}
		
		/// <summary>
		/// Displays a informational Message box.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		/// <remarks>Ensures, it gets displayed in foreground and 
		/// use the main form as the parent</remarks>
		public DialogResult MessageInfo(string text) {
			if (MainForm != null && MainForm.IsHandleCreated)
				Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm, text,
				RssBanditApplication.CaptionOnly, 
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		/// <summary>
		/// Displays a warning Message box.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		/// <remarks>Ensures, it gets displayed in foreground and 
		/// use the main form as the parent</remarks>
		public DialogResult MessageWarn(string text) {
			if (MainForm != null && MainForm.IsHandleCreated)
				Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm, text,
				RssBanditApplication.CaptionOnly, 
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}

		/// <summary>
		/// Displays a error Message box.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		/// <remarks>Ensures, it gets displayed in foreground and 
		/// use the main form as the parent</remarks>
		public DialogResult MessageError(string text) {
			if (MainForm != null && MainForm.IsHandleCreated)
				Win32.SetForegroundWindow(MainForm.Handle);
			return MessageBox.Show(MainForm, text, 
				RssBanditApplication.Caption, 
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}
		#endregion

		
		/// <summary>
		/// Determines whether is the form available and safe to get called.
		/// </summary>
		/// <param name="f">The form.</param>
		/// <returns>
		/// 	<c>true</c> if the form can be called; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsFormAvailable(Form f) {
			return (f != null && !f.Disposing && !f.IsDisposed);
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
							this.MessageError(SR.ExceptionLoadingSearchEnginesMessage(e.Message, errorLog));
						}
					}
			
					if(this.SearchEngineHandler.EnginesOK) {		
						// Build the menues, below
					}
					else if(validationErrorOccured) {					
						this.MessageError(SR.ExceptionInvalidSearchEnginesMessage(errorLog));
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
				this.MessageError(SR.ExceptionWebSearchEnginesSave(ioe.InnerException.Message));
			}
			catch(Exception ex) {
				_log.Error("Unexpected Error on saving SearchEngineSettings.", ex);
				this.MessageError(SR.ExceptionWebSearchEnginesSave(ex.Message));
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
					this.SubscriptionModified(NewsFeedProperty.FeedRefreshRate);
					//this.FeedlistModified = true;
				}

			}
			catch(FormatException) {
				MessageBox.Show (propertiesDialog, 
					SR.FormatExceptionRefreshRate,
					SR.PreferencesExceptionMessageTitle, 
					MessageBoxButtons.OK, MessageBoxIcon.Error);						
			}
			catch(OverflowException) {
				MessageBox.Show (propertiesDialog, 
					SR.OverflowExceptionRefreshRate,
					SR.PreferencesExceptionMessageTitle, 
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
					SR.FormatExceptionProxyPort, 
					SR.PreferencesExceptionMessageTitle, 
					MessageBoxButtons.OK, MessageBoxIcon.Error);						
			}
			catch(OverflowException) {
				MessageBox.Show (propertiesDialog, 
					SR.ExceptionProxyPortRange,
					SR.PreferencesExceptionMessageTitle, 
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

			if(Preferences.UseFavicons != propertiesDialog.checkUseFavicons.Checked){
				
				Preferences.UseFavicons = propertiesDialog.checkUseFavicons.Checked; 				
				try { //reload tree view					 
					this.guiMain.ApplyFavicons();
				} catch (Exception ex) {
					RssBanditApplication.PublishException(ex);
				}				
			}

			Preferences.NumNewsItemsPerPage   = propertiesDialog.numNewsItemsPerPage.Value;

			Preferences.MarkItemsAsReadWhenViewed = propertiesDialog.checkMarkItemsAsReadWhenViewed.Checked;
			Preferences.LimitNewsItemsPerPage = propertiesDialog.checkLimitNewsItemsPerPage.Checked;
			Preferences.BuildRelationCosmos = propertiesDialog.checkBuildRelationCosmos.Checked;
			Preferences.ReuseFirstBrowserTab = propertiesDialog.checkReuseFirstBrowserTab.Checked;
			Preferences.OpenNewTabsInBackground = propertiesDialog.checkOpenTabsInBackground.Checked;
			Preferences.FeedRefreshOnStartup = propertiesDialog.checkRefreshFeedsOnStartup.Checked;
			Preferences.AllowAppEventSounds = propertiesDialog.checkAllowAppEventSounds.Checked;
			Preferences.RunBanditAsWindowsUserLogon = propertiesDialog.checkRunAtStartup.Checked;
			
			Preferences.UserIdentityForComments = propertiesDialog.cboUserIdentityForComments.Text;

			if (propertiesDialog.radioTrayActionMinimize.Checked)
				Preferences.HideToTrayAction = HideToTray.OnMinimize;
			if (propertiesDialog.radioTrayActionClose.Checked)
				Preferences.HideToTrayAction = HideToTray.OnClose;
			if (propertiesDialog.radioTrayActionNone.Checked)
				Preferences.HideToTrayAction = HideToTray.None;

			Preferences.AutoUpdateFrequency = (AutoUpdateMode)propertiesDialog.comboAppUpdateFrequency.SelectedIndex;

			Preferences.NormalFont = (Font)propertiesDialog.FontForState(FontStates.Read).Clone();
			Preferences.UnreadFont = (Font)propertiesDialog.FontForState(FontStates.Unread).Clone();
			Preferences.FlagFont = (Font)propertiesDialog.FontForState(FontStates.Flag).Clone();
			Preferences.RefererFont = (Font)propertiesDialog.FontForState(FontStates.Referrer).Clone();
			Preferences.ErrorFont = (Font)propertiesDialog.FontForState(FontStates.Error).Clone();
			Preferences.NewCommentsFont = (Font)propertiesDialog.FontForState(FontStates.NewComments).Clone();
			
			Preferences.NormalFontColor = propertiesDialog.ColorForState(FontStates.Read);
			Preferences.UnreadFontColor = propertiesDialog.ColorForState(FontStates.Unread);
			Preferences.FlagFontColor = propertiesDialog.ColorForState(FontStates.Flag);
			Preferences.RefererFontColor = propertiesDialog.ColorForState(FontStates.Referrer);
			Preferences.ErrorFontColor = propertiesDialog.ColorForState(FontStates.Error);
			Preferences.NewCommentsFontColor = propertiesDialog.ColorForState(FontStates.NewComments);

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
				case 3: //"NewsgatorOnline"
					Preferences.RemoteStorageProtocol = RemoteStorageProtocolType.NewsgatorOnline;
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

			bool browserPrefsChanged = false; 

			if(Preferences.BrowserJavascriptAllowed != propertiesDialog.checkBrowserJavascriptAllowed.Checked ||
				Preferences.BrowserJavaAllowed != propertiesDialog.checkBrowserJavaAllowed.Checked ||
				Preferences.BrowserActiveXAllowed != propertiesDialog.checkBrowserActiveXAllowed.Checked ||
				Preferences.BrowserBGSoundAllowed != propertiesDialog.checkBrowserBGSoundAllowed.Checked ||
				Preferences.BrowserVideoAllowed != propertiesDialog.checkBrowserVideoAllowed.Checked ||
				Preferences.BrowserImagesAllowed != propertiesDialog.checkBrowserImagesAllowed.Checked
			){
				browserPrefsChanged = true; 
			}

			Preferences.BrowserJavascriptAllowed = propertiesDialog.checkBrowserJavascriptAllowed.Checked;
			Preferences.BrowserJavaAllowed = propertiesDialog.checkBrowserJavaAllowed.Checked;
			Preferences.BrowserActiveXAllowed = propertiesDialog.checkBrowserActiveXAllowed.Checked;
			Preferences.BrowserBGSoundAllowed = propertiesDialog.checkBrowserBGSoundAllowed.Checked;
			Preferences.BrowserVideoAllowed = propertiesDialog.checkBrowserVideoAllowed.Checked;
			Preferences.BrowserImagesAllowed = propertiesDialog.checkBrowserImagesAllowed.Checked;

			if(browserPrefsChanged){
				guiMain.ResetHtmlDetail(); 			
			}

			if(!this.feedHandler.EnclosureFolder.Equals(propertiesDialog.textEnclosureDirectory.Text)){
				this.feedHandler.EnclosureFolder = propertiesDialog.textEnclosureDirectory.Text;
				this.feedlistModified = true;
			}

			if(this.feedHandler.DownloadEnclosures != propertiesDialog.checkDownloadEnclosures.Checked){
				this.feedHandler.DownloadEnclosures = propertiesDialog.checkDownloadEnclosures.Checked;
				this.feedlistModified = true;
			}

			if(this.feedHandler.EnclosureAlert != propertiesDialog.checkEnableEnclosureAlerts.Checked){
				this.feedHandler.EnclosureAlert     = propertiesDialog.checkEnableEnclosureAlerts.Checked;
				this.feedlistModified = true;
			}

			if(this.feedHandler.CreateSubfoldersForEnclosures != propertiesDialog.checkDownloadCreateFolderPerFeed.Checked){			
				this.feedHandler.CreateSubfoldersForEnclosures = propertiesDialog.checkDownloadCreateFolderPerFeed.Checked;
				this.feedlistModified = true;
			}

		
			if((this.feedHandler.NumEnclosuresToDownloadOnNewFeed == Int32.MaxValue) &&  
				propertiesDialog.checkOnlyDownloadLastXAttachments.Checked){
				this.feedlistModified = true;
			}
			if(propertiesDialog.checkOnlyDownloadLastXAttachments.Checked){
				this.feedHandler.NumEnclosuresToDownloadOnNewFeed = Convert.ToInt32(propertiesDialog.numOnlyDownloadLastXAttachments.Value);			
			}else{
				this.feedHandler.NumEnclosuresToDownloadOnNewFeed = Int32.MaxValue;
			}

			if((this.feedHandler.EnclosureCacheSize == Int32.MaxValue) &&  
				propertiesDialog.checkEnclosureSizeOnDiskLimited.Checked){
				this.feedlistModified = true;
			}
			if(propertiesDialog.checkEnclosureSizeOnDiskLimited.Checked){
				this.feedHandler.EnclosureCacheSize = Convert.ToInt32(propertiesDialog.numEnclosureCacheSize.Value);
			}else{
				this.feedHandler.EnclosureCacheSize = Int32.MaxValue;
			}


			this.ApplyPreferences();
			this.SavePreferences();
		}
		
		private string[] ParseProxyBypassList(string proxyBypassString)
		{
			return ListHelper.StripEmptyEntries(proxyBypassString.Split(';', ' ', ','));
		}

		// called, to apply preferences to the NewsComponents and Gui
		internal void ApplyPreferences() {

			this.FeedHandler.MaxItemAge = Preferences.MaxItemAge;
			this.Proxy = this.CreateProxyFrom(Preferences);
			NewsHandler.BuildRelationCosmos = Preferences.BuildRelationCosmos;

			try {
				this.NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile, GetNewsItemFormatterTemplate());
			}
			catch {
				
				//this.NewsItemFormatter.XslStyleSheet = NewsItemFormatter.DefaultNewsItemTemplate;				
				Preferences.NewsItemStylesheetFile = String.Empty;	
				this.NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile, GetNewsItemFormatterTemplate());
			
			}
			this.FeedHandler.Stylesheet = Preferences.NewsItemStylesheetFile;
			Win32.ApplicationSoundsAllowed = Preferences.AllowAppEventSounds;
		}

		private IWebProxy CreateProxyFrom(RssBanditPreferences p) {
			
			// default proxy init:
			IWebProxy proxy = WebRequest.DefaultWebProxy; 
			proxy.Credentials = CredentialCache.DefaultCredentials;

			if(p.UseProxy) {	// private proxy settings

				if (p.ProxyPort > 0)
					proxy = new WebProxy(p.ProxyAddress, p.ProxyPort); 
				else
					proxy = new WebProxy(p.ProxyAddress); 

				proxy.Credentials = CredentialCache.DefaultCredentials;
				((WebProxy)proxy).BypassProxyOnLocal = p.BypassProxyOnLocal;
				//Get rid of String.Empty in by pass list because it means bypass on all URLs
				((WebProxy)proxy).BypassList = ListHelper.StripEmptyEntries(p.ProxyBypassList);

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
					_log.Error("Apply Preferences.UseIEProxySettings caused exception", ex);
					this.MessageError(SR.ExceptionProxyConfiguration(ex.Message));
					p.UseIEProxySettings = false;
				}

			} /* endif UseIEProxySettings */

			return proxy;
		
		}

		internal void LoadPreferences() {
			
			string pName = RssBanditApplication.GetPreferencesFileName();
			bool migrate = false;
			IFormatter formatter = new SoapFormatter();
			
			if(! File.Exists(pName)) {	// migrate from binary to XML
				string pOldName = RssBanditApplication.GetPreferencesFileNameOldBinary();
				string pTempNew = pOldName + ".v13";	// in between temp prefs

				if(File.Exists(pTempNew)) {
					pName = pTempNew;	// migrate from in-between
					migrate = true;
					formatter = new BinaryFormatter();
				} else if (File.Exists(pOldName)) {
					pName = pOldName;
					migrate = true;
					formatter = new BinaryFormatter();
				}
			}

			if(File.Exists(pName)) {
				using(Stream stream = FileHelper.OpenForRead(pName)) {
					try {
						// to provide backward compat.:
						formatter.Binder = new RssBanditPreferences.DeserializationTypeBinder();
						RssBanditPreferences p = (RssBanditPreferences)formatter.Deserialize(stream);
						Preferences = p;
					} catch (Exception e) {
						_log.Error("Preferences DeserializationException", e);
					}
				}

				if (migrate) {
					this.SavePreferences();
				}
			}
		}

		internal void SavePreferences() {
			
			using (MemoryStream stream = new MemoryStream()) {
				IFormatter formatter = new SoapFormatter();
				try {
					formatter.Serialize(stream, Preferences);
					string pName = RssBanditApplication.GetPreferencesFileName();
					if (FileHelper.WriteStreamWithBackup(pName, stream)) {
						// on success, raise event:
						if (PreferencesChanged != null)
							PreferencesChanged(this, EventArgs.Empty);
					}
				} catch (Exception ex) {
					_log.Error("SavePreferences() failed.", ex);
				}
			}

		}


		private void CheckAndMigrateSettingsAndPreferences() {
			// check, if any migration task have to be applied:
			
			// v.1.2.x to 1.3.x:
			// The old (one) username, mail and referer 
			// have to be migrated from Preferences to the default UserIdentity

			// Obsolete() warnings can be ignored for that function.
			if (!StringHelper.EmptyOrNull(this.Preferences.UserName) &&
				StringHelper.EmptyOrNull(this.Preferences.UserIdentityForComments)) {
				
				if (!this.feedHandler.UserIdentity.ContainsKey(this.Preferences.UserName)) {
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

		#endregion

		#region save/load SearchFolders

		public FinderSearchNodes FindersSearchRoot{	
			get {return this.findersSearchRoot;}
			set {this.findersSearchRoot = value;}
		}

		/// <summary>
		/// Removes default "Unread Items" search folder since this has now been replaced by a Special Folder. 
		/// </summary>
		/// <remarks>This code used to be in the custom action for the installer but was moved once we got rid 
		/// of custom actions due to Vista install issues</remarks>
		private void RemoveUnreadItemsSearchFolders(){
		
			string searchfolders = RssBanditApplication.GetSearchFolderFileName(); 

			try{

				XmlDocument doc      = new XmlDocument(); 

				if(File.Exists(searchfolders)){ //there should be an 'Unread Items' there
					doc.Load(searchfolders); 
				}else{
					return;
				}
			 
				XmlElement unreadItems = (XmlElement) doc.SelectSingleNode("/FinderSearchNodes/RssFinderNodes/RssFinder[FullPath = 'Unread Items']"); 
			
				if(unreadItems!=null){
					unreadItems.ParentNode.RemoveChild(unreadItems); 				
				}			
		
				doc.Save(searchfolders); 
			}catch (Exception ex) {
				_log.Error("RemoveUnreadItemsSearchFolders() Exception (reading/saving file).", ex);						
			}

		}
      		

		// For backward compatibility (read previous defined search folder settings)
		// we read the old defs. from GuiSettings 
		// If we get something from there, we save it immediatly to the new file/location used for
		// search folders, then removing the entry from settings.
		// If we did not found something in settings, we test for the saerch folder defs. file and read it.
		public FinderSearchNodes LoadSearchFolders() {
					
			this.RemoveUnreadItemsSearchFolders(); 

			FinderSearchNodes fsn = null;
			bool needs2saveNew = false;
			
			string s = this.GuiSettings.GetString("FinderNodes", null);

			if (s == null) {	// no old defs found. Read from search folder file
				
				if (File.Exists(RssBanditApplication.GetSearchFolderFileName())) {
					using (Stream stream = FileHelper.OpenForRead(RssBanditApplication.GetSearchFolderFileName())) {
						try {
							XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof(FinderSearchNodes));
							fsn = (FinderSearchNodes) ser.Deserialize(stream);
						} catch (Exception ex) {
							_log.Error("LoadSearchFolders::Load Exception (reading/deserialize file).", ex);						
						}
					}
				}
			} else {
				try {
					XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof(FinderSearchNodes));
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
					XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof(FinderSearchNodes));
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
			
			lock (AsyncWebRequest.TrustedCertificateIssues) {
				IList<CertificateIssue> issues = null;
				if (AsyncWebRequest.TrustedCertificateIssues.ContainsKey(site)) {
					issues = AsyncWebRequest.TrustedCertificateIssues[site];
					AsyncWebRequest.TrustedCertificateIssues.Remove(site);
				}
				if (issues == null)
					issues = new List<CertificateIssue>(1);

				if (!issues.Contains(issue))
					issues.Add(issue);

				AsyncWebRequest.TrustedCertificateIssues.Add(site, issues);
			}

			this.trustedCertIssuesModified = true;
		}

		internal void LoadTrustedCertificateIssues() {
			
			if (File.Exists(RssBanditApplication.GetTrustedCertIssuesFileName())) {
				
				lock(AsyncWebRequest.TrustedCertificateIssues) {
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

							List<CertificateIssue> issues = new List<CertificateIssue>();

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
								lock(AsyncWebRequest.TrustedCertificateIssues) {
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

					lock (AsyncWebRequest.TrustedCertificateIssues) {
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
			if (feedUrl == null)
				return null;

			string layout = feedHandler.GetFeedColumnLayout(feedUrl);

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
				if (!feedHandler.ColumnLayouts[known].Value.Equals(layout)){	// check if layout modified
					if (!feedHandler.ColumnLayouts[known].Value.Equals(layout, true)){	// check if just a simple resizing of columns						
						
						if (!layout.Equals(global, true)){ //check if new layout is equivalent to current default
							int otherKnownSimilar = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
						
							if (otherKnownSimilar >= 0){ //check if this layout is similar to an existing layout
								//feedHandler.ColumnLayouts[otherKnownSimilar] = new FeedColumnLayoutEntry(key, layout);	// refresh layout info
								feedHandler.SetFeedColumnLayout(feedUrl, feedHandler.ColumnLayouts.GetKey(otherKnownSimilar));	// set new key
							} else{ //this is a brand new layout
								key = Guid.NewGuid().ToString("N");
								feedHandler.ColumnLayouts.Add(key, layout);
								feedHandler.SetFeedColumnLayout(feedUrl, key);
							}

						}else{ //new layout is equivalent to the current default
							feedHandler.SetFeedColumnLayout(feedUrl, null);
							feedHandler.ColumnLayouts.RemoveAt(known);
						}
					} else {	// this was a simple column resizing
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
		/// Creates the default feed list if it doesn't exist. 
		/// </summary>
		/// <remarks>This code used to be in the custom action for the installer but was moved once we got rid 
		/// of custom actions due to Vista install issues</remarks>
		private void InstallDefaultFeedList(){
		
			string oldSubslist   = RssBanditApplication.GetOldFeedListFileName(); 
			string subslist      = RssBanditApplication.GetFeedListFileName(); 

			if(!File.Exists(oldSubslist) && !File.Exists(subslist)){
				
				using(Stream s = Resource.GetStream("Resources.default-feedlist.xml")){
					FileHelper.WriteStreamWithRename(subslist, s); 				
				}//using
			}		
		}


		/// <summary>
		/// Load the feedlist. To have exception handling and UI feedback,
		/// please call StartLoadingFeedlist().
		/// </summary>
		/// <exception cref="BanditApplicationException">On any failure</exception>
		internal void LoadFeedList() {

			//Load new feed file if exists 
			string p = RssBanditApplication.GetFeedListFileName();
			string pOld = RssBanditApplication.GetOldFeedListFileName();

			if(!File.Exists(p) && File.Exists(pOld)) {
				if (this.MessageQuestion(SR.UpgradeFeedlistInfoText(RssBanditApplication.Caption)) == DialogResult.No) {
					throw new BanditApplicationException(ApplicationExceptions.FeedlistOldFormat);
				}
				File.Copy(pOld , p); // copy to be used to load from
			
			}else{ //create default feed list 			
				this.InstallDefaultFeedList(); 			
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

			/* Also load feedlist for watched comments */ 

			p = RssBanditApplication.GetCommentsFeedListFileName();			

			if(File.Exists(p)) {				
				try {
					commentFeedsHandler.LoadFeedlist(p, new System.Xml.Schema.ValidationEventHandler(CommentFeedListValidationCallback));
					
					foreach(feedsFeed f in commentFeedsHandler.FeedsTable.Values){
						if((f.Any!= null) && (f.Any.Length > 0)){
							XmlElement origin = f.Any[0];
							string sourceFeedUrl  = origin.InnerText; 
							feedsFeed sourceFeed = feedHandler.FeedsTable[sourceFeedUrl];
							if(sourceFeed != null){
								f.Tag = sourceFeed; 
							}
						}
					}
				}
				catch (Exception e) {				
					_log.Error("Exception on loading '"+p+"'.", e);						
				}							
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
								this.SubscriptionModified(NewsFeedProperty.General);
								//this.FeedlistModified = true;
							}
							catch(Exception ex) {
								this.MessageError(SR.ExceptionImportFeedlist(s, ex.Message));
								return; 
							}
							guiMain.SaveSubscriptionTreeState();
							guiMain.InitiatePopulateTreeFeeds();
							guiMain.LoadAndRestoreSubscriptionTreeState();
						}					
					} else {
						Uri uri = null;
						try {
							uri = new Uri(s);
						} catch {}
						if (uri != null) {

							HttpRequestFileThreadHandler fileHandler = new HttpRequestFileThreadHandler(uri.AbsoluteUri, this.feedHandler.Proxy);
							DialogResult result = fileHandler.Start( guiMain, SR.GUIStatusWaitMessageRequestFile(uri.AbsoluteUri));
							
							if (result != DialogResult.OK)
								return;

							if (!fileHandler.OperationSucceeds) {
								this.MessageError(SR.WebExceptionOnUrlAccess( 
									uri.AbsoluteUri, fileHandler.OperationException.Message)); 
								return;
							}
                    
							myStream = fileHandler.ResponseStream;
							if (myStream != null) {
								using (myStream) {
									try { 
										this.feedHandler.ImportFeedlist(myStream, cat); 
										this.SubscriptionModified(NewsFeedProperty.General);
										//this.FeedlistModified = true;									}
									} catch(Exception ex) {
										this.MessageError(SR.ExceptionImportFeedlist(s, ex.Message));
										return; 
									}
									guiMain.SaveSubscriptionTreeState();
									guiMain.InitiatePopulateTreeFeeds();
									guiMain.LoadAndRestoreSubscriptionTreeState();
								}					
							}
						}
					}
					
				}
			}
			
		}

		internal void DeleteFeed(string url) {
			
			if (StringHelper.EmptyOrNull(url))
				return;
			
			// was possibly an error causing feed:
			SpecialFeeds.ExceptionManager.GetInstance().RemoveFeed(url);

			feedsFeed f = this.GetFeed(url);
			if (f != null) {
				RaiseFeedDeleted(url, f.title);
				f.Tag = null;
				try {
					this.FeedHandler.DeleteFeed(url);
				} catch (ApplicationException ex) {
					_log.Error(String.Format("DeleteFeed({0})", url), ex);
				}
				this.FeedWasModified(f, NewsFeedProperty.FeedRemoved);
				//this.FeedlistModified = true;
			}
			

		}

		private void RaiseFeedDeleted(string feedUrl, string feedTitle) {
			if (FeedDeleted != null) {
				try {
					FeedDeleted(this, new FeedDeletedEventArgs(feedUrl, feedTitle));
				} catch (Exception ex) {
					_log.Error("RaiseFeedDeleted() error", ex);
				}
			}
		}

		/// <summary>
		/// Disable a feed (with UI update)
		/// </summary>
		/// <param name="feedUrl">string</param>
		public void DisableFeed(string feedUrl) {
			if (feedUrl != null) {
				this.DisableFeed(this.FeedHandler.FeedsTable[feedUrl], 
					TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.MyFeeds), feedUrl));
			}
		}
		/// <summary>
		/// Disable a feed (with UI update)
		/// </summary>
		/// <param name="f">feedsFeed</param>
		/// <param name="feedsNode">FeedTreeNodeBase</param>
		internal void DisableFeed(feedsFeed f, TreeFeedsNodeBase feedsNode) {
			if (f != null) {
				this.FeedHandler.DisableFeed(f.link); 
				guiMain.SetSubscriptionNodeState(f, feedsNode, FeedProcessingState.Normal);
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
				
				item.FlagStatus = Flagged.None;
				this.ReFlagNewsItem(item);			

			}else if(folder is WatchedItemsNode){ 
				item.WatchComments = false; 
				this.ReWatchNewsItem(item); 				
			}

			//we always remove it from the smart folder regardless of type
			folder.Remove(item); 
		}

		public LocalFeedsFeed FlaggedItemsFeed {
			get { return flaggedItemsFeed; }
			set { flaggedItemsFeed = value; }
		}

		public void ClearFlaggedItems() {

			foreach(NewsItem ri in this.flaggedItemsFeed.Items){
			
				try{
				
					XmlQualifiedName key = RssHelper.GetOptionalElementKey(ri.OptionalElements, AdditionalFeedElements.OriginalFeedOfFlaggedItem); 

					XmlElement elem = null;
					//find bandit:feed-url element 
					if (null != key)
						elem  = RssHelper.GetOptionalElement(ri, key);

					if(elem != null){
							
						string feedUrl = elem.InnerText; 

						if(this.feedHandler.FeedsTable.Contains(feedUrl)){ //check if feed exists 

							IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

							//find this item 
							int itemIndex = itemsForFeed.IndexOf(ri); 


							if(itemIndex != -1){ //check if item still exists 

								NewsItem item = itemsForFeed[itemIndex]; 									
								item.FlagStatus = Flagged.None; 
								item.OptionalElements.Remove(AdditionalFeedElements.OriginalFeedOfFlaggedItem); 
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
				XmlElement elem = RssHelper.GetOptionalElement(theItem, AdditionalFeedElements.OriginalFeedOfFlaggedItem);
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

				IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

				//find this item 
				int itemIndex = itemsForFeed.IndexOf(theItem); 

				if(itemIndex != -1){ //check if item still exists 

					NewsItem item = itemsForFeed[itemIndex]; 									
					item.FlagStatus = theItem.FlagStatus; 

					this.FeedWasModified(feedUrl, NewsFeedProperty.FeedItemFlag);

				}

			}//if(this.feedHan...)

			this.flaggedItemsFeed.Modified = true;
							
		}


		/// <summary>
		/// Get a NewsItem to unwatch: usually called on an already watched item. It try to
		/// find the corresponding feed containing the item and unwatches them.
		/// </summary>
		/// <param name="theItem">NewsItem to re-flag</param>
		public void ReWatchNewsItem(NewsItem theItem) {

			if (theItem == null) 
				return; 

			string feedUrl = null;	// the corresponding feed Url

			try{
				XmlElement elem = RssHelper.GetOptionalElement(theItem, AdditionalFeedElements.OriginalFeedOfWatchedItem);
				if (elem != null) {
					feedUrl = elem.InnerText;
				}
			} catch {}

		
			//find this item in watched feeds and set watched state
			int index = this.watchedItemsFeed.Items.IndexOf(theItem); 

			if(index != -1){ //check if item exists 

				NewsItem item = (NewsItem)this.watchedItemsFeed.Items[index];
				item.WatchComments = theItem.WatchComments; 
			}
			

			//find this item in main feed list and set watched state
			if(feedUrl != null && this.feedHandler.FeedsTable.Contains(feedUrl)){ //check if feed exists 

				IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

				//find this item 
				int itemIndex = itemsForFeed.IndexOf(theItem); 

				if(itemIndex != -1){ //check if item still exists 

					NewsItem item = itemsForFeed[itemIndex]; 									
					item.WatchComments = theItem.WatchComments; 

					this.FeedWasModified(feedUrl, NewsFeedProperty.FeedItemWatchComments);

				}

			}//if(this.feedHan...)

			this.watchedItemsFeed.Modified = true;
							
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
						
					// now create a full copy (including item content)
					NewsItem flagItem = this.feedHandler.CopyNewsItemTo(theItem, flaggedItemsFeed);

					//take over flag status
					flagItem.FlagStatus = theItem.FlagStatus; 

					if (null == RssHelper.GetOptionalElementKey(flagItem.OptionalElements, AdditionalFeedElements.OriginalFeedOfFlaggedItem)){
						XmlElement originalFeed = RssHelper.CreateXmlElement(
							AdditionalFeedElements.ElementPrefix, 
							AdditionalFeedElements.OriginalFeedOfFlaggedItem.Name, 
							AdditionalFeedElements.OriginalFeedOfFlaggedItem.Namespace, 
							theItem.Feed.link); 
						flagItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfFlaggedItem, originalFeed.OuterXml);
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
			this.FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemFlag);
		}
		
		/// <summary>
		/// Get/Sets the unread items local feed
		/// </summary>
		public LocalFeedsFeed UnreadItemsFeed {
			get { return unreadItemsFeed; }
			set { unreadItemsFeed = value; }
		}

		/// <summary>
		/// Gets or sets the watched items feed.
		/// </summary>
		/// <value>The watched items feed.</value>
		public LocalFeedsFeed WatchedItemsFeed{
			get {return watchedItemsFeed; }
			set { watchedItemsFeed = value;} 
		}


		/// <summary>
		/// Updates the state of the WatchedItems based on the state of the items passed in
		/// </summary>
		/// <param name="items"></param>
        public void UpdateWatchedItems(IList<NewsItem> items) {

			if((items == null) || (items.Count == 0))
				return; 
		
			foreach(NewsItem ni in items){
			
				if(this.watchedItemsFeed.Items.Contains(ni)){
					this.watchedItemsFeed.Items.Remove(ni); //remove old copy of the NewsItem 
			
					XmlElement originalFeed = RssHelper.CreateXmlElement(
						AdditionalFeedElements.ElementPrefix, 
						AdditionalFeedElements.OriginalFeedOfWatchedItem.Name, 
						AdditionalFeedElements.OriginalFeedOfWatchedItem.Namespace, 
						ni.Feed.link);

					NewsItem watchedItem =  this.feedHandler.CopyNewsItemTo(ni, watchedItemsFeed);
					
					if (null == RssHelper.GetOptionalElementKey(watchedItem.OptionalElements, AdditionalFeedElements.OriginalFeedOfWatchedItem)){
					
						watchedItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfWatchedItem, originalFeed.OuterXml);
					}
									
					this.watchedItemsFeed.Add(watchedItem);
				}
			}
		
		}

		/// <summary>
		/// Gets a NewsItem to Watch and adds it (Clone) to the watched item node
		/// </summary>
		/// <param name="theItem">NewsItem to watch</param>
		public void WatchNewsItem(NewsItem theItem) {
			 
			if (theItem == null) 
				return; 

			if (theItem.WatchComments == false){
				// remove
				if (this.watchedItemsFeed.Items.Contains(theItem)) {
					this.watchedItemsFeed.Items.Remove(theItem);
				}

				if(!StringHelper.EmptyOrNull(theItem.CommentRssUrl) && 
					this.commentFeedsHandler.FeedsTable.ContainsKey(theItem.CommentRssUrl)){
						this.commentFeedsHandler.DeleteFeed(theItem.CommentRssUrl); 
						this.commentFeedlistModified   = true;
				}
			
			} else {

				XmlElement originalFeed = RssHelper.CreateXmlElement(
					AdditionalFeedElements.ElementPrefix, 
					AdditionalFeedElements.OriginalFeedOfWatchedItem.Name, 
					AdditionalFeedElements.OriginalFeedOfWatchedItem.Namespace, 
					theItem.Feed.link); 

				if (!this.watchedItemsFeed.Items.Contains(theItem)) {
					
					NewsItem watchedItem =  this.feedHandler.CopyNewsItemTo(theItem, watchedItemsFeed);
					
					if (null == RssHelper.GetOptionalElementKey(watchedItem.OptionalElements, AdditionalFeedElements.OriginalFeedOfWatchedItem)){
					
						watchedItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfWatchedItem, originalFeed.OuterXml);
					}
									
					this.watchedItemsFeed.Add(watchedItem);
				
				}

				if(!StringHelper.EmptyOrNull(theItem.CommentRssUrl) && 
					!commentFeedsHandler.FeedsTable.ContainsKey(theItem.CommentRssUrl)){					
					
					feedsFeed f = new feedsFeed(); 

					f.link  = theItem.CommentRssUrl; 					
					f.title = theItem.Title;	
					f.Tag   = theItem.Feed;					
 					f.Any   = new XmlElement[1]; 
					f.Any[0] = originalFeed; 

					//Always replace newsitems on disk with contents from feed. 
					//This prevents issues when comments are deleted.
					f.replaceitemsonrefresh = f.replaceitemsonrefreshSpecified = true; 

					// set feedsFeed new credentials
					if ( !StringHelper.EmptyOrNull( theItem.Feed.authUser) ) {	
						string u = null, p = null;

						NewsHandler.GetFeedCredentials(theItem.Feed, ref u, ref p);
						NewsHandler.SetFeedCredentials(f, u, p);

					} else {
						NewsHandler.SetFeedCredentials(f, null, null);
					}

					// add feed to backend					
					commentFeedsHandler.FeedsTable.Add(f.link, f); 				

					// set properties the backend requires the feed just added
					int intIn = feedHandler.GetRefreshRate(theItem.Feed.link)/2 * MilliSecsMultiplier; //fetch comments twice as often as feed
					commentFeedsHandler.SetRefreshRate(f.link, intIn);
					commentFeedsHandler.SetMaxItemAge(f.link, new TimeSpan(365,0,0,0)); //max item age is 1 year so we don't risk filtering out comments					
					this.commentFeedlistModified   = true;		
				}
			}

			this.watchedItemsFeed.Modified = true;
		
			if (!StringHelper.EmptyOrNull(theItem.Feed.link)) {
				this.FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemWatchComments);
			}
		}

		public LocalFeedsFeed SentItemsFeed {
			get { return sentItemsFeed; }
			set { sentItemsFeed = value; }
		}

		/// <summary>
		/// Adds the replyItem to the sent news item feed.
		/// </summary>
		/// <param name="inResponse2item">The item responded to.</param>
		/// <param name="replyItem">The reply item itself.</param>
		public void AddSentNewsItem(NewsItem inResponse2item, NewsItem replyItem) {
			 
			//TODO: do use a different approach and do not overwrite the flag!	
			if (inResponse2item != null)
				inResponse2item.FlagStatus = Flagged.Reply; 

			if (inResponse2item != null && replyItem != null) {
				// create a new one, because we could not modify the replyItem.link :(
				NewsItem newItem = new NewsItem(this.sentItemsFeed, replyItem.Title, inResponse2item.Link, replyItem.Content, replyItem.Date, inResponse2item.Feed.title); 
				newItem.OptionalElements = (Hashtable) replyItem.OptionalElements.Clone(); 

				if (null == RssHelper.GetOptionalElementKey(newItem.OptionalElements, AdditionalFeedElements.OriginalFeedOfFlaggedItem)){
					XmlElement originalFeed = RssHelper.CreateXmlElement(AdditionalFeedElements.ElementPrefix, 
						AdditionalFeedElements.OriginalFeedOfFlaggedItem.Name, 
						AdditionalFeedElements.OriginalFeedOfFlaggedItem.Namespace, 
						inResponse2item.Feed.link); 
					newItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfFlaggedItem, originalFeed.OuterXml);
				}
			
				newItem.BeenRead = false;
				this.sentItemsFeed.Add(newItem);
			
				guiMain.SentItemsNode.UpdateReadStatus();
			}
		}

		/// <summary>
		/// Adds the replyItem to the sent news item feed.
		/// </summary>
		/// <param name="postTarget">The feedsFeed posted to.</param>
		/// <param name="replyItem">The reply item itself.</param>
		public void AddSentNewsItem(feedsFeed postTarget, NewsItem replyItem) {
			 
			if (postTarget != null && replyItem != null) {
				// create a new one, because we could not modify the replyItem.link :(
				NewsItem newItem = new NewsItem(this.sentItemsFeed, replyItem.Title, Guid.NewGuid().ToString(), replyItem.Content, replyItem.Date, postTarget.title); 
				newItem.OptionalElements = (Hashtable) replyItem.OptionalElements.Clone(); 

				newItem.BeenRead = false;
				this.sentItemsFeed.Add(newItem);
			
				guiMain.SentItemsNode.UpdateReadStatus();
			}
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
			if (null != theItem.Feed && null == RssHelper.GetOptionalElementKey(theItem.OptionalElements, AdditionalFeedElements.OriginalFeedOfDeletedItem)) {
				XmlElement originalFeed = RssHelper.CreateXmlElement(
					AdditionalFeedElements.ElementPrefix, 
					AdditionalFeedElements.OriginalFeedOfDeletedItem.Name, 
					AdditionalFeedElements.OriginalFeedOfDeletedItem.Namespace, 
					theItem.Feed.link); 
				theItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfDeletedItem, originalFeed.OuterXml);
			}

			bool yetDeleted = false;
			if (!this.deletedItemsFeed.Items.Contains(theItem)) {
				// add new deleted item
				this.deletedItemsFeed.Add(theItem);
				yetDeleted = true;
			} 

			this.feedHandler.DeleteItem(theItem);

			this.deletedItemsFeed.Modified = true;
			this.FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemsDeleteUndelete);

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
		public TreeFeedsNodeBase RestoreNewsItem(NewsItem item) {
		
			if (item == null) 
				return null; 

			string containerFeedUrl = null;
			XmlElement elem = RssHelper.GetOptionalElement(item, AdditionalFeedElements.OriginalFeedOfDeletedItem);
			if (null != elem) {
				containerFeedUrl = elem.InnerText;
				item.OptionalElements.Remove(AdditionalFeedElements.OriginalFeedOfDeletedItem);
			}

			if (StringHelper.EmptyOrNull(containerFeedUrl)) {
				containerFeedUrl = item.Feed.link;
			}

			if (StringHelper.EmptyOrNull(containerFeedUrl)) {
				_log.Error("Cannot restore item: feed link missing.");
				return null;
			}

			bool foundAndRestored = false;
			TreeFeedsNodeBase feedsNode = null;

				
			if (null != RssHelper.GetOptionalElementKey(item.OptionalElements, AdditionalFeedElements.OriginalFeedOfFlaggedItem)) {
			
				// it was a flagged item
				this.flaggedItemsFeed.Add(item);
				feedsNode = (TreeFeedsNodeBase)guiMain.FlaggedFeedsNode(item.FlagStatus);
				foundAndRestored = true;
			
			} else if (this.FeedHandler.FeedsTable.Contains(containerFeedUrl)) {

				this.FeedHandler.RestoreDeletedItem(item);
				feedsNode = TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.MyFeeds), containerFeedUrl);
				foundAndRestored = true;
			
			} else {

				ISmartFolder isFolder = TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.SmartFolders), containerFeedUrl) as ISmartFolder;
				if (null != isFolder) {
					isFolder.Add(item);
					feedsNode = (TreeFeedsNodeBase)isFolder;
					foundAndRestored = true;
				}

			}

			if (foundAndRestored) {
			
				this.deletedItemsFeed.Remove(item);
				this.deletedItemsFeed.Modified = true;
				this.FeedWasModified(containerFeedUrl, NewsFeedProperty.FeedItemsDeleteUndelete);
			
			} else {
				_log.Error("Cannot restore item: container feed not found. Url was '"+containerFeedUrl +"'.");
			}

			return feedsNode;
		}

		public static void SetWorkingSet() { 

			try {
				System.Diagnostics.Process loProcess =  System.Diagnostics.Process.GetCurrentProcess(); 
				loProcess.MaxWorkingSet = new IntPtr(0x400000);
				loProcess.MinWorkingSet = new IntPtr(0x100000);
				//long lnValue = loProcess.WorkingSet; // see what the actual value 
			} catch (Exception ex) {
				_log.Error("SetWorkingSet caused exception.", ex);
			}

		} 


		/// <summary>
		/// Publish an XML Feed error.
		/// </summary>
		/// <param name="e">XmlException to publish</param>
		/// <param name="feedLink">The errornous feed url</param>
		/// <param name="updateNodeIcon">Set to true, if you want to get the node icon reflecting the errornous state</param>
		public void PublishXmlFeedError(System.Exception e, string feedLink, bool updateNodeIcon) {
			if (feedLink != null) {
			/*	Uri uri = null;
				try {
					uri = new Uri(feedLink);
				} catch (UriFormatException) {} */
				this.UpdateXmlFeedErrorFeed(this.CreateLocalFeedRequestException(e, feedLink), feedLink, updateNodeIcon);
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
		private void UpdateXmlFeedErrorFeed(System.Exception e, string resourceUri, bool updateNodeIcon) {

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
				guiMain.PopulateSmartFolder((TreeFeedsNodeBase)guiMain.ExceptionNode, false);
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
					XmlElement e = RssHelper.GetOptionalElement(ri, AdditionalFeedElements.OriginalFeedOfFlaggedItem);
					if (e != null) {
						feedUrl = e.InnerText;
					}
				} catch {}

				if(feedUrl != null && this.feedHandler.FeedsTable.Contains(feedUrl)){ //check if feed exists 

					IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false); 

					//find this item 
					int itemIndex = itemsForFeed.IndexOf(ri); 


					if(itemIndex != -1){ //check if item still exists 

						NewsItem item = itemsForFeed[itemIndex]; 									
						if (item.FlagStatus != ri.FlagStatus)	{// correction: older Bandit versions are not able to store flagStatus
							item.FlagStatus = ri.FlagStatus;
							this.flaggedItemsFeed.Modified = true;
							this.FeedWasModified(feedUrl, NewsFeedProperty.FeedItemFlag);	// self-healing
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
			
			autoSaveTimer.Dispose();

			if (guiMain != null && !guiMain.IsDisposed) { 
				if (guiMain.InvokeRequired) {
					guiMain.Invoke(new WinGuiMain.CloseMainForm(guiMain.Close), new object[]{true});
				} else {
					guiMain.Close(true);
				}
			}

			SaveApplicationState(true);
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
			this.MessageError(SR.ExceptionNewsItemFormatterStylesheetMessage(e.ErrorMessage, e.FailureException.Message));
		}
		
		private void OnNewsItemFormatterStylesheetValidationError(object sender, ExceptionEventArgs e) {
			_log.Error("OnNewsItemFormatterStylesheetValidationError() called", e.FailureException);
			this.MessageError(SR.ExceptionNewsItemFormatterStylesheetMessage(e.ErrorMessage, e.FailureException.Message));
		}


		private void OnInternetConnectionStateChanged(INetState oldState, INetState newState) {

			bool offline = ((newState & INetState.Offline) > 0);
			bool connected = ((newState & INetState.Connected) > 0);
			bool internet_allowed = connected && (newState & INetState.Online) > 0;
			
			this.feedHandler.Offline = !internet_allowed;
			
			if (oldState != newState) 
			{
				if (guiMain != null && !guiMain.IsDisposed) {
					guiMain.SetGuiStateINetConnected(internet_allowed);
					guiMain.SetTitleText(null);
					Mediator.SetEnabled(connected, "cmdToggleOfflineMode");
					if (connected)
						Mediator.SetChecked(offline, "cmdToggleOfflineMode");
					Mediator.SetEnabled(internet_allowed, "cmdAutoDiscoverFeed");
				}
			
				// notify service consumers:
				EventsHelper.Fire(InternetConnectionStateChange, 
					this, new InternetConnectionStateChangeEventArgs(oldState, newState));
			}
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
				this.SetGuiStateFeedbackText(SR.GUIStatusRefreshFeedsMessage, ApplicationTrayState.BusyRefreshFeeds);
			} else
				if (newState == NewsHandlerState.RefreshOne) {
				this.SetGuiStateFeedbackText(SR.GUIStatusLoadingFeed, ApplicationTrayState.BusyRefreshFeeds);
			} else
				if (newState == NewsHandlerState.RefreshAllAuto || newState == NewsHandlerState.RefreshAllForced) {
				this.SetGuiStateFeedbackText(SR.GUIStatusRefreshFeedsMessage, ApplicationTrayState.BusyRefreshFeeds);
			}
		}

		/// <summary>
		/// Called from the autoSaveTimer. It is re-used to probe for a 
		/// valid open internet connection...
		/// </summary>
		/// <param name="theStateObject"></param>
		private void OnAutoSave(object theStateObject) {
			if (IsFormAvailable(guiMain)) {
				if (!guiMain.ShutdownInProgress)
					guiMain.DelayTask(DelayedTasks.SaveUIConfiguration);
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
			//this.feedHandler.Offline = !stateManager.InternetAccessAllowed;		// reset feedHandler
		}

		/// <summary>
		/// Saves Application State: the feedlist, changed cached files, search folders, flagged items and sent items
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void SaveApplicationState() {
			SaveApplicationState(false);
		}

		/// <summary>
		/// Saves Application State: the feedlist, changed cached files, search folders, flagged items and sent items
		/// </summary>
		/// <param name="appIsClosing">if set to <c>true</c> [app is closing].</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void SaveApplicationState(bool appIsClosing) {
			
			if (guiMain == null) return;
			
			/* 
			 * we handle the exit error here, because it does not make sense
			 * to provide a "Resume", "Ignore" as the global exception handler
			 * offers on exiting the program
			 * */
			try { 
				if(this.feedlistModified && this.feedHandler != null && this.feedHandler.FeedsTable != null && 
					this.feedHandler.FeedsListOK ) 
				{ 

					using (MemoryStream stream = new MemoryStream()) {
						try {
							feedHandler.SaveFeedList(stream); 
							if (FileHelper.WriteStreamWithBackup( RssBanditApplication.GetFeedListFileName(), stream)) {
								this.feedlistModified = false;	// reset state flag
							}
						} catch (Exception ex) {
							_log.Error("feedHandler::SaveFeedList() failed.", ex);
						}
					}
				}

				if (this.flaggedItemsFeed.Modified)
					this.flaggedItemsFeed.Save();

				if (this.sentItemsFeed.Modified)
					this.sentItemsFeed.Save();

				if (this.deletedItemsFeed.Modified)
					this.deletedItemsFeed.Save();

				if(this.watchedItemsFeed.Modified)
					this.watchedItemsFeed.Save(); 

				if(this.commentFeedlistModified && this.commentFeedsHandler != null && this.commentFeedsHandler.FeedsTable != null && 
					this.commentFeedsHandler.FeedsListOK ) { 

					using (MemoryStream stream = new MemoryStream()) {
						try {
							commentFeedsHandler.SaveFeedList(stream); 
							FileHelper.WriteStreamWithBackup( RssBanditApplication.GetCommentsFeedListFileName(), stream);									
						} catch (Exception ex) {
							_log.Error("commentFeedsHandler::SaveFeedList() failed.", ex);
						}
					}
				}
				

				if (this.trustedCertIssuesModified)
					this.SaveTrustedCertificateIssues();

				this.SaveModifiedFeeds();

				//save search folders
				this.SaveSearchFolders();

				// Last operation: write all changes to the search index to disk
				if (appIsClosing)
					this.FeedHandler.SearchHandler.StopIndexer();

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
			guiMain.MarkSelectedNodeRead(guiMain.CurrentSelectedFeedsNode);
			if (guiMain.CurrentSelectedFeedsNode != null)
				this.FeedWasModified(guiMain.CurrentSelectedFeedsNode.DataKey, NewsFeedProperty.FeedItemReadState);
			//this.FeedlistModified = true;
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// Called only from subscriptions tree on SmartFolder(s)
		/// </summary>
		/// <param name="sender"></param>
		public void CmdDeleteAllFeedItems(ICommand sender) {
			
			TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;
			
			if (tn != null) {
				
				ISmartFolder isFolder = tn as ISmartFolder;
				
				if (isFolder != null) {

					if (isFolder is FlaggedItemsNode || isFolder is WatchedItemsNode) {
						// we need to unflag the items within each subscribed feed:
						for (int i = 0, j = isFolder.Items.Count; i < j; i++) {
							NewsItem item = (NewsItem)isFolder.Items[0];
							this.RemoveItemFromSmartFolder(isFolder, item);
						}
					} else {	// simply clr all
						isFolder.Items.Clear();
					}
					isFolder.Modified = true;
					guiMain.PopulateSmartFolder(tn, true);
					guiMain.UpdateTreeNodeUnreadStatus(tn, 0);
					return;
				}

			}

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;

		}

		/// <summary>
		/// Called from listview context menu or Edit|Delete menu
		/// </summary>
		/// <param name="sender"></param>
		public void CmdDeleteSelectedFeedItems(ICommand sender) {
			if (guiMain.CurrentSelectedFeedsNode != null) {
				guiMain.RemoveSelectedFeedItems();
			}
		}

		/// <summary>
		/// Called from listview context menu or Edit|Restore items menu
		/// </summary>
		/// <param name="sender"></param>
		public void CmdRestoreSelectedFeedItems(ICommand sender) {
			if (guiMain.CurrentSelectedFeedsNode != null) {
				guiMain.RestoreSelectedFeedItems();
			}
		}

		/// <summary>
		/// Pops up the NewFeedDialog class and adds a new feed to the list 
		/// of subscribed feeds. The category will be preselected/added.
		/// </summary>
		/// <param name="category">Feed category</param>
		/// <returns>true, if the dialog succeeds (feed subscribed), else false</returns>
//		public bool CmdNewFeed(string category) {
//			return this.CmdNewFeed(category, null, null);
//		}
		/// <summary>
		/// Pops up the NewFeedDialog class and adds a new feed to the list 
		/// of subscribed feeds.
		/// </summary>
		/// <param name="category">Feed category</param>
		/// <param name="feedLink">Feed link</param>
		/// <param name="feedTitle">Feed title</param>
		/// <returns>true, if the dialog succeeds (feed subscribed), else false</returns>
		public bool CmdNewFeed(string category, string feedLink, string feedTitle) {
			
			return this.SubscribeToFeed(feedLink, category, feedTitle);

			#region not reached (there for ref.)
/*
			bool success = true;
			NewFeedDialog newFeedDialog = new NewFeedDialog(category, defaultCategory, feedHandler.Categories, feedLink, feedTitle); 
			
			newFeedDialog.btnLookupTitle.Enabled = this.InternetAccessAllowed;
			newFeedDialog.Proxy = this.Proxy; 

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
*/
			#endregion
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
		/// fdaction:?action=toggleread&postid=id-of-post
		/// fdaction:?action=toggleflag&postid=id-of-post
		/// fdaction:?action=previouspage&currpage=current-page-number
		/// fdaction:?action=nextpage&currpage=current-page-number
		/// From within error detail reports:
		/// fdaction:?action=navigatetofeed&feedid=id-of-feed
		/// fdaction:?action=unsubscribefeed&feedid=id-of-feed
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

			}else if(url.Scheme.Equals("fdaction")){

				/* TODO: Toggle envelope and flag in newspaper on click if javascript is OFF (by user)
				 * 1. Fetch <area> whose href attribute contains URL
				 * 2. Get name of parent <map> of the <area>
				 * 3. Fetch <img> whose usemap attribute is '#' + value from Step 2
				 * 4. Swap out value of src attribute. 
				 */

				int idIndex = webUrl.IndexOf("postid=") + 7; 
				int feedIdIndex = webUrl.IndexOf("feedid=") + 7; 
				int typeIndex = webUrl.IndexOf("pagetype=") + 9;
				 
				if(webUrl.IndexOf("toggleread")!= -1){
					guiMain.ToggleItemReadState(webUrl.Substring(idIndex)); 
				}else if(webUrl.IndexOf("toggleflag")!= -1){
					guiMain.ToggleItemFlagState(webUrl.Substring(idIndex)); 
				}else if(webUrl.IndexOf("togglewatch")!= -1){
					guiMain.ToggleItemWatchState(webUrl.Substring(idIndex)); 
				}else if(webUrl.IndexOf("markread")!= -1){
					guiMain.ToggleItemReadState(webUrl.Substring(idIndex), true); 
				}else if(webUrl.IndexOf("previouspage")!= -1){
					guiMain.SwitchPage(webUrl.Substring(typeIndex), false);										
				}else if(webUrl.IndexOf("nextpage")!= -1){
					guiMain.SwitchPage(webUrl.Substring(typeIndex), true);										
				}else if(webUrl.IndexOf("navigatetofeed")!= -1) {
					string normalizedUrl = HtmlHelper.UrlDecode(webUrl.Substring(feedIdIndex));
					feedsFeed f = GetFeed(normalizedUrl);
					if (f != null) 
						guiMain.NavigateToFeed(f);
				}else if(webUrl.IndexOf("unsubscribefeed")!= -1) {
					string normalizedUrl = HtmlHelper.UrlDecode(webUrl.Substring(feedIdIndex));
					feedsFeed f = GetFeed(normalizedUrl);
					if (f != null) 
						this.UnsubscribeFeed(f, false);
				}
				return true; 

			} else if (url.Scheme.Equals("feed")) {
				this.CmdNewFeed(defaultCategory, RssLocater.UrlFromFeedProtocolUrl(url.ToString()), null );
				return true;
			} else if (url.ToString().EndsWith(".opml")) {
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
				if (!RssBanditApplication.IsDefaultAggregator() && 
					RssBanditApplication.ShouldAskForDefaultAggregator &&
					!UACManager.Denied(ElevationRequiredAction.MakeDefaultAggregator)) 
				{
					using (AskForDefaultAggregator dialog = new AskForDefaultAggregator()) {
						if (dialog.ShowDialog(guiMain) == DialogResult.OK) {
							try {
								RssBanditApplication.MakeDefaultAggregator();
							} catch (System.Security.SecurityException) {
								this.MessageInfo(SR.SecurityExceptionCausedByRegistryAccess("HKEY_CLASSES_ROOT\feed"));
							} catch (Exception ex) {
								this.MessageError(SR.ExceptionSettingDefaultAggregator(ex.Message));
							}
						}
						RssBanditApplication.ShouldAskForDefaultAggregator = !dialog.checkBoxDoNotAskAnymore.Checked;
					}
				}
			} catch (Exception e) {
				_log.Error("Unexpected error on checking for default aggregator.", e);
			}

			CheckAndRegisterIEMenuExtensions();
		}

		/// <summary>
		/// Checks and init sounds events.
		/// </summary>
		internal void CheckAndInitSoundEvents() {
			RssBandit.Win32.Registry.CheckAndInitSounds(
				Path.GetFileNameWithoutExtension(Application.ExecutablePath));
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
		/// Used to initialize parameters to the XSLT template that formats the feeds as HTMl. 
		/// </summary>
		/// <returns></returns>
		internal XsltArgumentList PrepareXsltArgs(){
			XsltArgumentList xslArgs = new XsltArgumentList();
			xslArgs.AddParam("AppStartupPath", String.Empty, Application.StartupPath);
			xslArgs.AddParam("AppUserDataPath", String.Empty, RssBanditApplication.GetUserPath());
			xslArgs.AddParam("MarkItemsAsReadWhenViewed", String.Empty, this.Preferences.MarkItemsAsReadWhenViewed);
			xslArgs.AddParam("LimitNewsItemsPerPage", String.Empty, this.Preferences.LimitNewsItemsPerPage);
			xslArgs.AddParam("LastPageNumber", String.Empty, guiMain.LastPageNumber);
			xslArgs.AddParam("CurrentPageNumber", String.Empty, guiMain.CurrentPageNumber);
			
			return xslArgs; 
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

			// display channel processing:
			foreach (FeedInfo fi in feeds) {
				foreach (NewsItem n in fi.ItemsList) {
					RssBanditApplication.DisplayingNewsChannelServices.ProcessItem(n);
				}
			}

			return this.NewsItemFormatter.ToHtml(stylesheet, feeds, this.PrepareXsltArgs());			
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

			// display channel processing:
			foreach (NewsItem item in feed.ItemsList) {
				RssBanditApplication.DisplayingNewsChannelServices.ProcessItem(item);
			}

			return this.NewsItemFormatter.ToHtml(stylesheet, feed, this.PrepareXsltArgs());			
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
			
			// display channel processing:
			item = (NewsItem)RssBanditApplication.DisplayingNewsChannelServices.ProcessItem(item);

			if (toHighlight == null) {
				return this.NewsItemFormatter.ToHtml(stylesheet, item, this.PrepareXsltArgs());
			} else {
				List<SearchCriteriaString> criterias = new List<SearchCriteriaString>();
				for (int i = 0; i < toHighlight.Count; i++) {
					// only String matches are interesting for highlighting
					SearchCriteriaString scs = toHighlight[i] as SearchCriteriaString;
					if (scs != null && scs.Match(item)) {
						criterias.Add(scs);
					}
				}
				if (criterias.Count > 0) {		
					SearchHitNewsItem shitem = item as SearchHitNewsItem; 
					NewsItem clone = new NewsItem(item.Feed, item.Title, item.Link, 
						this.ApplyHighlightingTo(item.Content, criterias),item.Date, item.Subject,
						item.ContentType, item.OptionalElements, item.Id, item.ParentId); 	
					clone.FeedDetails = item.FeedDetails;	
					clone.BeenRead    = item.BeenRead; 					
					return NewsItemFormatter.ToHtml(stylesheet,clone, this.PrepareXsltArgs());				
				} else {
					return this.NewsItemFormatter.ToHtml(stylesheet,item, this.PrepareXsltArgs());
				}
			}
		}	


		private string ApplyHighlightingTo(string xhtml, IList<SearchCriteriaString> searchCriteriaStrings){
		
			for (int i = 0; i < searchCriteriaStrings.Count; i++){
			
					
				// only String matches are interesting for highlighting here
				SearchCriteriaString scs = searchCriteriaStrings[i];
					
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

		/// <summary>
		/// Reads an app settings entry.
		/// </summary>
		/// <param name="name">The name of the entry.</param>
		/// <param name="entryType">Expected Type of the entry.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>Value read or defaultValue</returns>
		public static object ReadAppSettingsEntry(string name, Type entryType, object defaultValue) {
			if (StringHelper.EmptyOrNull(name))
				return defaultValue;

			string value = ConfigurationManager.AppSettings[name];
			if (!StringHelper.EmptyOrNull(value)) {
				
				if (entryType == typeof(bool)) {
					try { return Boolean.Parse(value); } 
					catch (FormatException){}
				} else if (entryType == typeof(string)) {
					return value;
				} else if (entryType.IsEnum) {
					try { return Enum.Parse(entryType, value, true); }
					catch (ArgumentException){}
				} else if (entryType == typeof(Color)) {
					Color c = Color.FromName(value); 
					// If name is not the valid name of a pre-defined color, 
					// the FromName method creates a Color structure that has
					// an ARGB value of zero (that is, all ARGB components are 0).
					if (c.ToArgb() != 0)
						return c;
				} else {
					if (entryType != null)
						Trace.WriteLine("ReadAppSettingsEntry() unsupported type: " + entryType.FullName);
				}
				
			}
			return defaultValue;
		}

		/// <summary>
		/// Retrives the assembly informational version (from the AssemblyInformationalVersionAttribute).
		/// </summary>
		/// <param name="assembly">Assembly</param>
		/// <returns>String. It is empty if no description was found.</returns>
		public static string GetAssemblyInformationalVersion(Assembly assembly) {
			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
			if (attributes.Length > 0) {
				string ad = ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
				if (!StringHelper.EmptyOrNull(ad) )
					return ad;
			}
			return String.Empty;
		}

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
				currentChecked = Mediator.IsChecked("cmdShowNewItemsReceivedBalloon");

			Preferences.ShowNewItemsReceivedBalloon = !currentChecked;
			Mediator.SetChecked(!currentChecked, "cmdShowNewItemsReceivedBalloon");
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
				currentChecked = Mediator.IsChecked("cmdToggleOfflineMode");
			
			Utils.SetIEOffline(currentChecked);			// update IE
			this.UpdateInternetConnectionState(true);	// get new network state, takes a few msecs
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
				"  * NetAdvantage for Windows Forms (c) 2006 by Infragistics, http://www.infragistics.com\n" +
				"  * SandBar, SandDock (c) 2005 by Divelements Limited, http://www.divil.co.uk/net/\n"+
				"  * Portions Copyright ©2002-2004 The Genghis Group (www.genghisgroup.com)\n"+
				"  * sourceforge.net team (Project hosting)", SR.WindowAboutCaption(RssBanditApplication.CaptionOnly), 
				MessageBoxButtons.OK, MessageBoxIcon.Asterisk); 

		}

		/// <summary>
		/// Display the Help Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdWebHelp(ICommand sender)	{
			this.NavigateToUrlInExternalBrowser(webHelpUrl);
		}

		/// <summary>
		/// Display the Bug Report Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdReportAppBug(ICommand sender) {
			this.NavigateToUrlAsUserPreferred(bugReportUrl, RssBanditApplication.CaptionOnly + ": Bug Tracker", true, true);
		}

		/// <summary>
		/// Display the Workspace News Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdWorkspaceNews(ICommand sender)	{
			this.NavigateToUrlAsUserPreferred(workspaceNewsUrl, RssBanditApplication.CaptionOnly + ": Project News", true, true);
		}

		/// <summary>
		/// Display the Wiki News Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdWikiNews(ICommand sender) {
			this.NavigateToUrlAsUserPreferred(wikiNewsUrl, RssBanditApplication.CaptionOnly + ": Wiki", true, true);
		}

		/// <summary>
		/// Display the Wiki News Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdVisitForum(ICommand sender) {
			this.NavigateToUrlAsUserPreferred(forumUrl, RssBanditApplication.CaptionOnly + ": Forum", true, true);
		}

		/// <summary>
		/// Display Donate to project Web-Page.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDonateToProject(ICommand sender) {
			this.NavigateToUrlAsUserPreferred(projectDonationUrl, RssBanditApplication.CaptionOnly + ": Donate", true, true);
		}
		

		/// <summary>
		/// Send logs by mail.
		/// </summary>
		/// <param name="sender">The sender.</param>
		public void CmdSendLogsByMail(ICommand sender) {
			List<string> files = new List<string>();
			
			try {
				// log files are configured in the RssBandit.exe.log4net.config
				// as to be ${APPDATA}\\RssBandit\\trace.log, NOT always at the possibly reconfigured
				// RssBanditApplciation.GetUserPath() location:
				string logFilesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), RssBanditApplication.Name);
				if (Directory.Exists(logFilesFolder)) {
				
					string[] matches = Directory.GetFiles(logFilesFolder, "trace.log*");

					if (matches.Length > 0) {
						files.AddRange(matches);
						if (Preferences.UseProxy) {	
							// include preferences to get proxy infos 
							// (pwds are encrypted, so we don't sniff sensitive infos here)
							files.Add(RssBanditApplication.GetPreferencesFileName());
						}

						string zipDest = Path.Combine(
							Environment.GetFolderPath(Environment.SpecialFolder.Personal),
							"RssBandit.logs." + RssBanditApplication.VersionLong + ".zip");
					
						if (File.Exists(zipDest))
							FileHelper.Delete(zipDest);

						FileHelper.ZipFiles(files.ToArray(), zipDest);
						// open a mailto:contact@rssbandit.org mail window with
						// hints how to attach the zip

						Process.Start(CreateMailUrlMessage(
							"contact@rssbandit.org", 
							"Log files -- RSS Bandit v" + RssBanditApplication.VersionLong, 
							"Please attach this file from your My Documents folder:\r\n\n" + zipDest + "\r\n\r\n" +
							"and replace this text with some more useful informations about: \r\n" +
							"\t* Your system environment and OS version\r\n" +
							"\t* Description of the issue to report\r\n" +
							"\t* Any hints/links that may help,\r\nplease!"));
					} else {
						MessageInfo("No log files at " + logFilesFolder);
					}
				
				} else {
					MessageInfo("No log files at " + logFilesFolder);
				}
			} catch (Exception ex) {
				_log.Error("Failed to send log files", ex);
				MessageError("Failed to send log files: \r\n" + ex.Message);
			}
		}

		private string CreateMailUrlMessage(string to, string subject, string text) {
			subject = HtmlHelper.UrlEncode( subject );
			string body = HtmlHelper.UrlEncode( text );
			if( body.Length + subject.Length > 900 ) {
				if( subject.Length > 400 ) {
					subject = subject.Substring( 0, 400 ) + "...";

				}
				body = body.Substring( 0, 897 - subject.Length ) + "...";
			}

			return  "mailto:" + to + "?subject=" + subject  +"&body=" + body;
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
				DateTime t = this.LastAutoUpdateCheck;
				if (this.Preferences.AutoUpdateFrequency == AutoUpdateMode.OnceIn14Days)
					t = t.AddDays(14);
				else
					t = t.AddMonths(1);
				if (DateTime.Compare(t, DateTime.Now) < 0) 
					this.CheckForUpdates(mode);
			}

		}

		private void OnApplicationUpdateAvailable(object sender, UpdateAvailableEventArgs e) {
			AutoUpdateMode mode = (AutoUpdateMode)RssBanditUpdateManager.Tag;
			bool hasUpdates = e.NewVersionAvailable;

			if (hasUpdates) {
			
				if (DialogResult.No == MessageQuestion(SR.DialogQuestionNewAppVersionAvailable)) {
					this.LastAutoUpdateCheck = DateTime.Now;
				} else 	{
					
					//RssBanditUpdateManager.BeginDownload(updateManager.AvailableUpdates);	// Async. Preferences updated in OnUpdateComplete event
					
					// for now we do not download anything, just display the SF download page:
					this.NavigateToUrlAsUserPreferred(projectDownloadUrl, RssBanditApplication.CaptionOnly + ": Download", true, true);
					this.LastAutoUpdateCheck = DateTime.Now;
				}

			} else {
				this.LastAutoUpdateCheck = DateTime.Now;

				if (mode == AutoUpdateMode.Manually)
					MessageInfo(SR.DialogMessageNoNewAppVersionAvailable);
			}

		}

		private void CheckForUpdates(AutoUpdateMode mode) {
			try 
			{
				RssBanditUpdateManager.Tag = mode;
				if (mode == AutoUpdateMode.Manually)
					RssBanditUpdateManager.BeginCheckForUpdates(this.guiMain, this.Proxy); 
				else
					RssBanditUpdateManager.BeginCheckForUpdates(null, this.Proxy); 
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
			if (IsFormAvailable(guiMain)) {
				guiMain.DoShow();
			}
		}

		/// <summary>
		/// Refresh all feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdRefreshFeeds(ICommand sender) {
			guiMain.UpdateAllFeeds(true);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// Pops up the SubscriptionWizard and adds a new feed to the list 
		/// of subscribed feeds. It uses WizardMode.SubscribeURLDirect.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNewFeed(ICommand sender) {
			string category = guiMain.CategoryOfSelectedNode();

			if(category == null) {
				category = DefaultCategory; 
			}

			this.SubscribeToFeed(null, category.Trim(), null, null, WizardMode.SubscribeURLDirect);

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;

		}

		/// <summary>
		/// Pops up the SubscriptionWizard and adds a new feed to the list 
		/// of subscribed feeds. WizardMode.Default
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNewSubscription(ICommand sender) {
			string category = guiMain.CategoryOfSelectedNode();

			if(category == null) {
				category = DefaultCategory; 
			}

			this.SubscribeToFeed(null, category.Trim(), null, null, WizardMode.Default);

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;

		}

		/// <summary>
		/// Pops up the SubscriptionWizard and adds a new feed to the list 
		/// of subscribed feeds. WizardMode.SubscribeNNTPDirect
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNewNntpFeed(ICommand sender) {
			string category = guiMain.CategoryOfSelectedNode();

			if(category == null) {
				category = DefaultCategory; 
			}

			this.SubscribeToFeed(null, category.Trim(), null, null, WizardMode.SubscribeNNTPDirect);

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;

		}

		/// <summary>
		/// Moves the focus to the next unread feed item if available.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNextUnreadFeedItem(ICommand sender) {
			guiMain.MoveToNextUnreadItem();
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}
		/// <summary>
		/// Display a dialog to manage AddIns.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdOpenManageAddInsDialog(ICommand sender) {
			ManageAddInDialog dialog = new ManageAddInDialog(this);
			dialog.ShowDialog(MainForm);
			dialog.Dispose();
		}
		
		/// <summary>
		/// Display the wizard dialog to autodiscover feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdAutoDiscoverFeed(ICommand sender) {
			
			if (this.SearchForFeeds(null)) {
				//success
			}
			return;
/*			
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
						autoDiscover.LocationMethod = FeedLocationMethod.Syndic8Search;
						autoDiscover.OperationMessage = Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageDetectingFeedsWithKeywords", autoDiscoverFeedsDialog.Keywords);
					} else {
						autoDiscover.WebPageUrl = autoDiscoverFeedsDialog.WebpageUrl;
						autoDiscover.LocationMethod = FeedLocationMethod.AutoDiscoverUrl;
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
*/			
		}
		
		/// <summary>
		/// Pops up the NewCategoryDialog class and adds a new category to the list 
		/// of subscribed feeds.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNewCategory(ICommand sender) {
			guiMain.NewCategory();
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// Delete all Feeds subscribed to.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDeleteAll(ICommand sender) {
			if (this.MessageQuestion(SR.MessageBoxDeleteAllFeedsQuestion) == DialogResult.Yes) {
				this.feedHandler.FeedsTable.Clear(); 
				this.feedHandler.Categories.Clear(); 
				this.feedHandler.ClearItemsCache();
				this.feedHandler.SearchHandler.IndexRemoveAll();
				this.SubscriptionModified(NewsFeedProperty.General);
				//this.FeedlistModified = true;
				guiMain.InitiatePopulateTreeFeeds(); 
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdUpdateCategory(ICommand sender) {
			guiMain.UpdateCategory(true);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdRenameCategory(ICommand sender) {
			guiMain.InitiateRenameFeedOrCategory();
			// way too early to call this: treeview stays in edit mode
			//this.FeedlistModified = true;

			//We need to know which node is being edited in PreFilterMessage so we 
			//don't set it to null. 
			/* if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null; */
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDeleteCategory(ICommand sender) {
			if (guiMain.NodeEditingActive)
				return;
			// right-click selected:
			TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;
			if (tn == null) return;
			if(tn.Type != FeedNodeType.Category) return;

			if (DialogResult.Yes == this.MessageQuestion(
			                        	SR.MessageBoxDeleteAllFeedsInCategoryQuestion,
			                        	String.Format(" - {0} ({1})",SR.MenuDeleteCategoryCaption, guiMain.CurrentSelectedFeedsNode.Text))) {
				// walks down the hierarchy and delete each child feed,
				// removes the node:
				guiMain.DeleteCategory(tn);
				this.SubscriptionModified(NewsFeedProperty.FeedCategoryRemoved);
				//this.FeedlistModified = true;
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdUpdateFeed(ICommand sender) {
			string feedUrl = guiMain.CurrentSelectedFeedsNode.DataKey;
			if (!StringHelper.EmptyOrNull(feedUrl)) {
				this.feedHandler.AsyncGetItemsForFeed(feedUrl, true, true);
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdRenameFeed(ICommand sender) {
			guiMain.InitiateRenameFeedOrCategory();
			// way too early to call this, treeview stays in edit mode:
			//this.FeedlistModified = true;

			//We need to know which node is being edited in PreFilterMessage so we 
			//don't set it to null. 
			/* if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null; */
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdViewSourceOfFeed(ICommand sender) {
			if(guiMain.CurrentSelectedFeedsNode!= null && guiMain.CurrentSelectedFeedsNode.DataKey != null) {
				string feedUrl = guiMain.CurrentSelectedFeedsNode.DataKey;
				string title = SR.TabFeedSourceCaption(guiMain.CurrentSelectedFeedsNode.Text);
				
				using (FeedSourceDialog dialog = new FeedSourceDialog(this.Proxy, this.feedHandler.GetFeedCredentials(feedUrl), feedUrl, title)) {
					dialog.ShowDialog(this.guiMain);
				}
//				this.NavigateToUrlAsUserPreferred(tmpFile, SR.TabFeedSourceCaption(guiMain.CurrentSelectedFeedsNode.Text),true, true);
			}								

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// Overloaded. Validates a feed link.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdValidateFeed(ICommand sender) {
			this.CmdValidateFeed(guiMain.CurrentSelectedFeedsNode.DataKey);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// Overloaded. Validates a feed link.
		/// </summary>
		/// <param name="feedLink">Feed link</param>
		public void CmdValidateFeed(string feedLink) {
			if (!StringHelper.EmptyOrNull(feedLink)) {
				this.NavigateToUrlAsUserPreferred(validationUrlBase+HttpUtility.UrlEncode(feedLink), SR.TabValidationResultCaption, true, true);
			}
		}

		/// <summary>
		/// Overloaded. Navigates to feed home page (feed link).
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNavigateFeedHome(ICommand sender) {
			this.CmdNavigateFeedHome(guiMain.CurrentSelectedFeedsNode.DataKey);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// Overloaded. Navigates to feed home page (feed link).
		/// </summary>
		/// <param name="feedLink">Feed link</param>
		public void CmdNavigateFeedHome(string feedLink) {
			if (!StringHelper.EmptyOrNull(feedLink)) {
				
				IFeedDetails feedInfo = feedHandler.GetFeedInfo(feedLink);

				if (feedInfo != null) {
					this.NavigateToUrlAsUserPreferred(feedInfo.Link, SR.TabFeedHomeCaption(feedInfo.Title), true, true);
				}
			}
		}

		/// <summary>
		/// Overloaded. Display technorati link cosmos of the feed.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdNavigateFeedLinkCosmos(ICommand sender) {
			this.CmdNavigateFeedLinkCosmos(guiMain.CurrentSelectedFeedsNode.DataKey);
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// Overloaded. Display technorati link cosmos of the feed.
		/// </summary>
		/// <param name="feedLink">Feed link</param>
		public void CmdNavigateFeedLinkCosmos(string feedLink) {
			if (!StringHelper.EmptyOrNull(feedLink)) {
				IFeedDetails feedInfo = feedHandler.GetFeedInfo(feedLink);
				if (feedInfo != null) {
					this.NavigateToUrlAsUserPreferred(linkCosmosUrlBase+HttpUtility.UrlEncode(feedInfo.Link),SR.TabLinkCosmosCaption(feedInfo.Title), true, true);
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

				String.Format("{0} (*.*)|*.*", SR.FileDialogFilterAllFiles);

				if (dialog.radioFormatOPML.Checked) {
					format = FeedListFormat.OPML;
					sfd.Filter = String.Format("{0} (*.opml)|*.opml|{1} (*.*)|*.*", SR.FileDialogFilterOPMLFiles, SR.FileDialogFilterAllFiles);
					includeEmptyCategories = dialog.checkFormatOPMLIncludeCats.Checked;
				} else if (dialog.radioFormatNative.Checked) {
					format = FeedListFormat.NewsHandler;
					sfd.Filter = String.Format("{0} (*.xml)|*.xml|{1} (*.*)|*.*", SR.FileDialogFilterXMLFiles, SR.FileDialogFilterAllFiles);
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
						this.MessageError(SR.ExceptionSaveFileMessage(sfd.FileName, ex.Message));
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
			TreeFeedsNodeBase n = guiMain.CurrentSelectedFeedsNode;
			if(n!= null){
				if (n.Type == FeedNodeType.Category || n.Type == FeedNodeType.Feed)
					category = n.CategoryStoreName;
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
				this.MessageInfo(SR.RemoteStorageFeature_Info);
				return;
			}

			// Make sure this is what the user wants to do
			if (this.MessageQuestion(SR.RemoteStorageUpload_Question)  == DialogResult.No) {
				return;
			}


			RemoteFeedlistThreadHandler rh = new RemoteFeedlistThreadHandler(
				RemoteFeedlistThreadHandler.Operation.Upload, this,
				Preferences.RemoteStorageProtocol, Preferences.RemoteStorageLocation,
				Preferences.RemoteStorageUserName, Preferences.RemoteStoragePassword, this.GuiSettings);

			DialogResult result = rh.Start(guiMain, SR.GUIStatusWaitMessageUpLoadingFeedlist(Preferences.RemoteStorageProtocol.ToString()), false);

			if (result != DialogResult.OK)
				return;

			if (!rh.OperationSucceeds) {			
				this.MessageError(SR.GUIFeedlistUploadExceptionMessage(rh.OperationException.Message));
			}

		}

		/// <summary>
		/// Loads the feed list from the location configured on the 
		/// Remote Storage tab of the Options dialog.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdDownloadFeeds(ICommand sender) {
			if (!Preferences.UseRemoteStorage) {
				this.MessageInfo(SR.Keys.RemoteStorageFeature_Info);
				return;
			}

			if (this.MessageQuestion(SR.RemoteStorageDownload_Question) == DialogResult.No) {
				return;
			}

			RemoteFeedlistThreadHandler rh = new RemoteFeedlistThreadHandler(
				RemoteFeedlistThreadHandler.Operation.Download, this,
				Preferences.RemoteStorageProtocol, Preferences.RemoteStorageLocation,
				Preferences.RemoteStorageUserName, Preferences.RemoteStoragePassword, this.GuiSettings);

			DialogResult result = rh.Start(guiMain, SR.GUIStatusWaitMessageDownLoadingFeedlist(Preferences.RemoteStorageProtocol.ToString()), false);

			if (result != DialogResult.OK)
				return;

			if (rh.OperationSucceeds) {
				guiMain.SaveSubscriptionTreeState();
				guiMain.SyncFinderNodes();
				guiMain.InitiatePopulateTreeFeeds();
				guiMain.LoadAndRestoreSubscriptionTreeState();
			} else {
				this.MessageError(SR.GUIFeedlistDownloadExceptionMessage(rh.OperationException.Message));
			}

		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowOptions(ICommand sender) {

			ShowOptions(OptionDialogSection.Default, guiMain, null);

			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowFeedProperties(ICommand sender) {
			if(guiMain.CurrentSelectedFeedsNode!= null && guiMain.CurrentSelectedFeedsNode.DataKey != null) {
				TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;

				feedsFeed f = null;
				int refreshrate = this.refreshRate;
				TimeSpan feedMaxItemAge = TimeSpan.Zero;
				bool feedDisabled = false;
				bool feedMarkItemsReadOnExit = false; 

				try {
					f = feedHandler.FeedsTable[tn.DataKey]; 
					//refreshrate = (f.refreshrateSpecified ? f.refreshrate : this.refreshRate); 							
					try 
					{
						refreshrate = feedHandler.GetRefreshRate(f.link); 
						feedMaxItemAge = feedHandler.GetMaxItemAge(f.link);
						feedMarkItemsReadOnExit = feedHandler.GetMarkItemsReadOnExit(f.link); 
					} catch {/* ignore this */}

				} catch (Exception e) {
					this.MessageError(SR.GUIStatusErrorGeneralFeedMessage(tn.DataKey, e.Message));
					return;
				}

				FeedProperties propertiesDialog = new FeedProperties(f.title, f.link, refreshrate/60000, feedMaxItemAge, (f.category == null ? defaultCategory: f.category), defaultCategory, feedHandler.Categories, this.feedHandler.GetStyleSheet(f.link)); 
				propertiesDialog.comboMaxItemAge.Enabled = !feedMaxItemAge.Equals(TimeSpan.Zero);
				propertiesDialog.checkEnableAlerts.Checked = f.alertEnabled;
				propertiesDialog.checkMarkItemsReadOnExit.Checked = feedMarkItemsReadOnExit;
				propertiesDialog.checkDownloadEnclosures.Checked  = this.feedHandler.GetDownloadEnclosures(f.link);
				propertiesDialog.checkEnableEnclosureAlerts.Checked  = this.feedHandler.GetEnclosureAlert(f.link);

				if (f.authUser != null) {	// feedsFeed has credentials
					string u = null, p = null;
					NewsHandler.GetFeedCredentials(f, ref u, ref p);
					propertiesDialog.textUser.Text = u;
					propertiesDialog.textPwd.Text = p;
				}

				propertiesDialog.ShowDialog(guiMain);

				if(propertiesDialog.DialogResult == DialogResult.OK) 
				{
					
					NewsFeedProperty changes = NewsFeedProperty.None;
					bool refreshThisFeed = false;

					if((propertiesDialog.textBox1.Text == null) ||
						(propertiesDialog.textBox2.Text == null)||
						propertiesDialog.textBox1.Text.Trim().Equals(String.Empty) ||
						propertiesDialog.textBox2.Text.Trim().Equals(String.Empty)) {
					
						this.MessageError(SR.GUIFieldLinkTitleInvalid);
					
					}	else {
						
						if(!f.link.Equals(propertiesDialog.textBox2.Text.Trim())) {
							// link was changed						   						  
							this.feedHandler.FeedsTable.Remove(f.link); 
							changes |= NewsFeedProperty.FeedLink;

							string newLink = propertiesDialog.textBox2.Text.Trim();
							//handle the common case of feed URI not beginning with HTTP 
							try{ 
								Uri reqUri = new Uri(newLink);
								newLink = reqUri.AbsoluteUri;
							}catch(UriFormatException){

								if(!newLink.ToLower().StartsWith("http://")){
									newLink = "http://" + newLink; 
									Uri reqUri = new Uri(newLink); 
									newLink = reqUri.AbsoluteUri;
								}
				
							}

							f.link = newLink; 
							this.feedHandler.FeedsTable.Add(f.link, f); 
							tn.DataKey = f.link; 

							refreshThisFeed = true;
						}

						if(!f.title.Equals(propertiesDialog.textBox1.Text.Trim())) {
							f.title = propertiesDialog.textBox1.Text.Trim();
							changes |= NewsFeedProperty.FeedTitle;
							tn.Text = f.title;	
						}

					}
					
					try { 

						if((!StringHelper.EmptyOrNull(propertiesDialog.comboBox1.Text.Trim()))) {
							Int32 intIn = System.Int32.Parse(propertiesDialog.comboBox1.Text.Trim());
							changes |= NewsFeedProperty.FeedRefreshRate;
							if (intIn <= 0) {
								this.DisableFeed(f, tn);
								feedDisabled = true;
							} else {
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
						this.MessageError(SR.FormatExceptionRefreshRate);
					}
					catch(OverflowException) {
						this.MessageError(SR.OverflowExceptionRefreshRate);
					}						

					string category = null;

					if((propertiesDialog.comboBox2.Text != null) && 
						(!propertiesDialog.comboBox2.Text.Equals(String.Empty)) && 
						(!propertiesDialog.comboBox2.Text.Equals(defaultCategory))) {

						category = propertiesDialog.comboBox2.Text.Trim();
					}

					if (category != null && !category.Equals(f.category)) {
						
						f.category = category; 
						changes |= NewsFeedProperty.FeedCategory;
						if(!feedHandler.Categories.ContainsKey(category)) {
							feedHandler.Categories.Add(category); 
						}
						// find/create the target node:
						TreeFeedsNodeBase target =	guiMain.CreateSubscriptionsCategoryHive(guiMain.GetRoot(RootFolderType.MyFeeds), category);
						// move to new location:
						guiMain.MoveNode(tn, target);
					}
					
					if (propertiesDialog.comboMaxItemAge.Enabled) {
						if (feedMaxItemAge.CompareTo(propertiesDialog.MaxItemAge) != 0) {
							refreshThisFeed = true;
							this.feedHandler.SetMaxItemAge(f.link, propertiesDialog.MaxItemAge);
							changes |= NewsFeedProperty.FeedMaxItemAge;
						}
					}

					if (propertiesDialog.textUser.Text != null && propertiesDialog.textUser.Text.Trim().Length != 0 ) {	// set feedsFeed new credentials
						string u = propertiesDialog.textUser.Text.Trim(), p = null;
						if (!StringHelper.EmptyOrNull(propertiesDialog.textPwd.Text))
							p = propertiesDialog.textPwd.Text.Trim();
						NewsHandler.SetFeedCredentials(f, u, p);
						changes |= NewsFeedProperty.FeedCredentials;
						refreshThisFeed = true;
					} else {
						NewsHandler.SetFeedCredentials(f, null, null);
						changes |= NewsFeedProperty.FeedCredentials;
					}

					if (f.alertEnabled != propertiesDialog.checkEnableAlerts.Checked)
						changes |= NewsFeedProperty.FeedAlertOnNewItemsReceived;
					f.alertEnabledSpecified = f.alertEnabled = propertiesDialog.checkEnableAlerts.Checked ;
									

					if(propertiesDialog.checkMarkItemsReadOnExit.Checked != feedHandler.GetMarkItemsReadOnExit(f.link)){
						this.feedHandler.SetMarkItemsReadOnExit(f.link, propertiesDialog.checkMarkItemsReadOnExit.Checked); 
						changes |= NewsFeedProperty.FeedMarkItemsReadOnExit;
					}

					if (refreshThisFeed && !feedDisabled) {
						this.feedHandler.MarkForDownload(f);
					}

					if(feedHandler.GetDownloadEnclosures(f.link) != propertiesDialog.checkDownloadEnclosures.Checked){
						feedHandler.SetDownloadEnclosures(f.link, propertiesDialog.checkDownloadEnclosures.Checked);
					}

					if(feedHandler.GetEnclosureAlert(f.link) != propertiesDialog.checkEnableEnclosureAlerts.Checked){
						feedHandler.SetEnclosureAlert(f.link, propertiesDialog.checkEnableEnclosureAlerts.Checked);
					}
					
								
					if (propertiesDialog.checkCustomFormatter.Checked) {
						string stylesheet = propertiesDialog.comboFormatters.Text;

						if(!stylesheet.Equals(this.feedHandler.GetStyleSheet(f.link))){
							this.feedHandler.SetStyleSheet(f.link,stylesheet ); 						
							changes |= NewsFeedProperty.FeedStylesheet;

							if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet)){
								this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
							}
						}
					}
					else {
						if(!String.Empty.Equals(this.feedHandler.GetStyleSheet(f.link))){
							this.feedHandler.SetStyleSheet(f.link, String.Empty);
							changes |= NewsFeedProperty.FeedStylesheet;
						}
					}

					guiMain.SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);
					this.FeedWasModified(f, changes);
					//this.FeedlistModified = true;
				}


				//cleanup 
				propertiesDialog.Dispose(); 
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;

		}


		/// <summary>Displays the properties dialog for a category </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdShowCategoryProperties(ICommand sender) {


			if(guiMain.CurrentSelectedFeedsNode!= null && (guiMain.CurrentSelectedFeedsNode.Type == FeedNodeType.Category) )
			{
				TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;

				string category = null, catPlusSep = null, categoryName; 
				int refreshrate = this.refreshRate;
				TimeSpan feedMaxItemAge = TimeSpan.Zero;			
				bool feedMarkItemsReadOnExit = false; 

				try 
				{
					category = tn.CategoryStoreName;				
					catPlusSep = category + NewsHandler.CategorySeparator;
					categoryName = tn.Text; 
				
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
					this.MessageError(SR.GUIStatusErrorGeneralFeedMessage(category, e.Message));
					return;
				}

				CategoryProperties propertiesDialog = new CategoryProperties(tn.Text, refreshrate/60000, feedMaxItemAge, this.feedHandler.GetCategoryStyleSheet(category)); 
				propertiesDialog.comboMaxItemAge.Enabled = !feedMaxItemAge.Equals(TimeSpan.Zero);
				propertiesDialog.checkMarkItemsReadOnExit.Checked = feedMarkItemsReadOnExit;
				propertiesDialog.checkDownloadEnclosures.Checked = this.feedHandler.GetCategoryDownloadEnclosures(category);
				propertiesDialog.checkEnableEnclosureAlerts.Checked = this.feedHandler.GetCategoryEnclosureAlert(category);
				

				propertiesDialog.ShowDialog(guiMain);

				if(propertiesDialog.DialogResult == DialogResult.OK) 
				{
						
					NewsFeedProperty changes = NewsFeedProperty.General;

					if((propertiesDialog.textBox2.Text == null)||					
						propertiesDialog.textBox2.Text.Trim().Equals(String.Empty)) 
					{					
						this.MessageError(SR.GUIFieldTitleInvalid);					
					}	
					else 
					{
						
						if(!categoryName.Equals(propertiesDialog.textBox2.Text.Trim())) 
						{		
							//string oldCategory = category;
							categoryName = propertiesDialog.textBox2.Text.Trim();
							// this call yet cause a FeedModified() notification:
							guiMain.RenameTreeNode(tn, categoryName); 
							category c = feedHandler.Categories.GetByKey(category);
							feedHandler.Categories.Remove(category);
							category = tn.CategoryStoreName;
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
								foreach(feedsFeed f in feedHandler.FeedsTable.Values){
									if( (f.category != null) &&  (f.category.Equals(category) || f.category.StartsWith(catPlusSep))){
										f.refreshrateSpecified = false; 
										guiMain.SetSubscriptionNodeState(f, TreeHelper.FindNode(tn, f), FeedProcessingState.Normal);
									}
								}

								intIn = intIn * MilliSecsMultiplier;
								feedHandler.SetCategoryRefreshRate(category, intIn); 
							}
						}

					}
					catch(FormatException) 
					{
						this.MessageError(SR.FormatExceptionRefreshRate);
					}
					catch(OverflowException) 
					{
						this.MessageError(SR.OverflowExceptionRefreshRate);
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
							changes |= NewsFeedProperty.General;
							//this.FeedWasModified(f.link);
						}
					}					
				
					this.feedHandler.SetCategoryMarkItemsReadOnExit(category, propertiesDialog.checkMarkItemsReadOnExit.Checked); 					
					this.feedHandler.SetCategoryDownloadEnclosures(category, propertiesDialog.checkDownloadEnclosures.Checked);
					this.feedHandler.SetCategoryEnclosureAlert(category, propertiesDialog.checkEnableEnclosureAlerts.Checked);					

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

					this.SubscriptionModified(changes);
					//this.FeedlistModified = true; 
				}

				//cleanup
				propertiesDialog.Dispose(); 
			}
			if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null;

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
		/// Opens the reply post window to allow a user to
		/// answer to an post (send a comment to a feed item) 
		/// or reply to NNTP group post.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdPostReplyToItem(ICommand sender) {
			NewsItem item2reply = guiMain.CurrentSelectedFeedItem; 
			
			if (item2reply == null) {
				this.MessageInfo(SR.GuiStateNoFeedItemSelectedMessage);
				return;
			}

			if ((postReplyForm == null) || (postReplyForm.IsDisposed)){
				postReplyForm = new PostReplyForm(Preferences.UserIdentityForComments, this.IdentityManager); 
				postReplyForm.PostReply += new PostReplyEventHandler(OnPostReplyFormPostReply);
			}
			
			postReplyForm.ReplyToItem = item2reply;
			
			postReplyForm.Show();	// open non-modal
			Win32.SetForegroundWindow(postReplyForm.Handle);

		}

		/// <summary>
		/// Opens the new post window to allow a user to
		/// create a new post to send to a NNTP group.
		/// </summary>
		/// <param name="sender">Object that initiates the call</param>
		public void CmdPostNewItem(ICommand sender) {
			TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode; 
			if (tn == null || tn.Type != FeedNodeType.Feed) 
			{
				this.Mediator.SetEnabled("-cmdFeedItemNewPost");
				return;
			} 

			string feedUrl = tn.DataKey;
			if (feedUrl == null || 
				!RssHelper.IsNntpUrl(feedUrl) ||
				!this.FeedHandler.FeedsTable.Contains(feedUrl)) 
			{
				this.Mediator.SetEnabled("-cmdFeedItemNewPost");
				return;
			}

			if ((postReplyForm == null) || (postReplyForm.IsDisposed)){
				postReplyForm = new PostReplyForm(Preferences.UserIdentityForComments, this.IdentityManager); 
				postReplyForm.PostReply += new PostReplyEventHandler(OnPostReplyFormPostReply);
			}

			postReplyForm.PostToFeed = this.FeedHandler.FeedsTable[feedUrl];
			
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
			bool running = true;

			/* fails to exec on .NET 1.0
			if (!File.Exists(Path.Combine(Application.StartupPath, "RssBandit.exe.manifest" )) && Win32.IsOSAtLeastWindowsXP) {
				Application.EnableVisualStyles();
				Application.DoEvents();
			}
			*/

			/* setup handler for unhandled exceptions */ 
			ApplicationExceptionHandler eh = new ApplicationExceptionHandler();
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(eh.OnAppDomainException);			
			
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
            try {
                running = InitialInstanceActivator.Activate(appInstance, callback, args);
            } catch (Exception ex) {
                _log.Error(ex); /* other instance is probably still running */ 
            }
			_log.Info("Application v" + RssBanditApplication.VersionLong + " started, running instance is " + running.ToString());

			if (!running) 
			{
				// init to system default:
				SharedCulture = CultureInfo.CurrentCulture;
				SharedUICulture = CultureInfo.CurrentUICulture;

				if (appInstance.HandleCommandLineArgs(args)) 
				{
					if (! StringHelper.EmptyOrNull(appInstance.commandLineOptions.LocalCulture)) 
					{
						try {
							SharedUICulture = CultureInfo.CreateSpecificCulture(appInstance.commandLineOptions.LocalCulture);
							SharedCulture = SharedUICulture;
						} catch (Exception ex) {
							appInstance.MessageError(SR.ExceptionProcessCommandlineCulture(appInstance.commandLineOptions.LocalCulture, ex.Message));
						}
					}
					
					// take over customized cultures to current main thread:
					Thread.CurrentThread.CurrentCulture = SharedCulture;
					Thread.CurrentThread.CurrentUICulture = SharedUICulture;
					
					if (!appInstance.commandLineOptions.StartInTaskbarNotificationAreaOnly &&
						initialStartupState != FormWindowState.Minimized) {
						// no splash, if start option is tray only or minimized
						Splash.Show();
						Splash.Version = String.Format("v{0}", RssBanditApplication.Version);
						Splash.Status = SR.AppLoadStateLoading;
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

				// fix of issue https://sourceforge.net/tracker/?func=detail&atid=615248&aid=1404778&group_id=96589
				// we use now a copy of the .SubscribeTo collection to allow users clicking twice or more to
				// a "feed:uri" link while a subscription wizard window is still yet open:
				foreach (string newFeedUrl in new ArrayList(this.commandLineOptions.SubscribeTo)) {
					if (IsFormAvailable(this.guiMain))
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

		private void SetGuiStateFeedbackText(string message) {
			if (!IsFormAvailable(guiMain)) 
				return;
			if (this.guiMain.InvokeRequired)
				guiMain.BeginInvoke(new WinGuiMain.SetGuiMessageFeedbackDelegate(guiMain.SetGuiStateFeedback), new object[]{message});
			else
				this.guiMain.SetGuiStateFeedback(message);
		}
		private void SetGuiStateFeedbackText(string message, ApplicationTrayState state) {
			if (!IsFormAvailable(guiMain)) 
				return;
			if (this.guiMain.InvokeRequired)
				guiMain.BeginInvoke(new WinGuiMain.SetGuiMessageStateFeedbackDelegate(guiMain.SetGuiStateFeedback), new object[]{message, state});
			else
				this.guiMain.SetGuiStateFeedback(message, state);
		}

		/// <summary>
		/// PostReplyForm callback
		/// </summary>
		/// <param name="replyEventArgs"></param>
		private void OnPostReplyFormPostReply(object sender, PostReplyEventArgs replyEventArgs) {
			
			bool success = false;

			string parentID = null;
			string title = replyEventArgs.Title;
			string name  = replyEventArgs.FromName; 
			string url   = replyEventArgs.FromUrl; 
			string email = replyEventArgs.FromEMail; 
			string comment = String.Empty;
			
			NewsItem item2post = null, item2reply = null;
			PostReplyThreadHandler prth;

			if(replyEventArgs.ReplyToItem != null) {

				item2reply = replyEventArgs.ReplyToItem;
				parentID = item2reply.Id;

				XmlDocument tempDoc = new XmlDocument(); 							

				if (replyEventArgs.Beautify) {// not yet active (but in next release):
					comment = replyEventArgs.Comment.Replace("\r\n", "<br />") ;
					item2post = new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, ContentType.Html, new Hashtable(), url, parentID); 
				} else {
					comment = replyEventArgs.Comment; 			
					item2post = new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, null, parentID); 
				}

				string commentUrl     = item2reply.CommentUrl;
				item2post.FeedDetails = item2reply.FeedDetails; 
				item2post.Author = (email == null) || (email.Trim().Length == 0) ? name : email + " (" + name + ")"; 			
				
				/* redundancy here, because Joe Gregorio changed spec now must support both <author> and <dc:creator> */				
				XmlElement emailNode = tempDoc.CreateElement("author"); 
				emailNode.InnerText  = item2post.Author; 							

				item2post.OptionalElements.Add(new XmlQualifiedName("author"), emailNode.OuterXml); 
				item2post.ContentType = ContentType.Html; 

				prth = new PostReplyThreadHandler(this.feedHandler, commentUrl, item2post, item2reply);
				DialogResult result = prth.Start(postReplyForm, SR.GUIStatusPostReplyToItem);

				if (result != DialogResult.OK)
					return;

				if (!prth.OperationSucceeds) {
					this.MessageError(SR.ExceptionPostReplyToNewsItem( 
						(StringHelper.EmptyOrNull(item2reply.Title) ? item2reply.Link: item2reply.Title) , 
						prth.OperationException.Message));
					return;
				}

				this.AddSentNewsItem(item2reply, item2post);
				success = true;
				
			
			} else if (replyEventArgs.PostToFeed != null) {
				
				feedsFeed f = replyEventArgs.PostToFeed;
				XmlDocument tempDoc = new XmlDocument(); 							

				if (replyEventArgs.Beautify) {// not yet active (but in next release):
					comment = replyEventArgs.Comment.Replace("\r\n", "<br />") ;
					item2post = new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, ContentType.Html, new Hashtable(), url, null); 
				} else {
					comment = replyEventArgs.Comment; 			
					item2post = new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, null, null); 
				}

				item2post.CommentStyle = SupportedCommentStyle.NNTP;
				// in case the feed does not yet have downloaded items, we may get null here:
				item2post.FeedDetails = this.feedHandler.GetFeedInfo(f.link); 
				if (item2post.FeedDetails == null)
					item2post.FeedDetails = new FeedInfo(f.id, f.cacheurl, new List<NewsItem>(0), f.title, f.link, f.title);
				item2post.Author = (email == null) || (email.Trim().Length == 0) ? name : email + " (" + name + ")"; 			
				
				/* redundancy here, because Joe Gregorio changed spec now must support both <author> and <dc:creator> */				
				XmlElement emailNode = tempDoc.CreateElement("author"); 
				emailNode.InnerText  = item2post.Author; 							

				item2post.OptionalElements.Add(new XmlQualifiedName("author"), emailNode.OuterXml); 
				item2post.ContentType = ContentType.Html; 

				prth = new PostReplyThreadHandler(this.feedHandler, item2post, f);
				DialogResult result = prth.Start(postReplyForm, SR.GUIStatusPostNewFeedItem);

				if (result != DialogResult.OK)
					return;

				if (!prth.OperationSucceeds) {
					this.MessageError(SR.ExceptionPostNewFeedItem( 
						(StringHelper.EmptyOrNull(item2post.Title) ? f.link: item2post.Title) , 
						prth.OperationException.Message));
					return;
				}

				this.AddSentNewsItem(f, item2post);
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
			
			private bool startInTaskbarNotificationAreaOnly = false;
			/// <summary>
			/// Have a look to http://blogs.gotdotnet.com/raymondc/permalink.aspx/5a811e6f-cd12-48de-8994-23409290faea,
			/// that is why we does not name it "StartInSystemTray" or such.
			/// </summary>
			[CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "taskbar", ShortName="t", Description = SR.Keys.CmdLineStartInTaskbarDesc, DescriptionIsResourceId = true)]
			public bool StartInTaskbarNotificationAreaOnly {
				get { return startInTaskbarNotificationAreaOnly; }
				set { startInTaskbarNotificationAreaOnly = value; }
			}

			private StringCollection subscribeTo = new StringCollection();
			[DefaultCommandLineArgument(CommandLineArgumentTypes.Multiple, Name="feedUrl", Description = SR.Keys.CmdLineSubscribeToDesc, DescriptionIsResourceId = true)]
			public StringCollection SubscribeTo {
				get { return subscribeTo; }
				set { subscribeTo = value; }
			}

			private bool showHelp;
			[CommandLineArgument(CommandLineArgumentTypes.Exclusive, Name = "help", ShortName = "h", Description = SR.Keys.CmdLineHelpDesc, DescriptionIsResourceId = true)]
			public bool ShowHelp {
				get { return showHelp; }
				set { showHelp = value; }
			}

			private string localCulture = String.Empty;
			[CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "culture", ShortName = "c", Description = SR.Keys.CmdLineCultureDesc, DescriptionIsResourceId = true)]
			public string LocalCulture {
				get { return localCulture; }
				set {
					localCulture = value; 
					if (StringHelper.EmptyOrNull(localCulture)) {
						localCulture = String.Empty;
					}
				}
			}
			
			private bool resetUi;
			[CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name= "resetUI", ShortName="r", Description = SR.Keys.CmdLineResetUIDesc, DescriptionIsResourceId = true)]
			public bool ResetUserInterface {
				get { return resetUi; }
				set { resetUi = value; }
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
					string message = e.ExceptionObject.ToString();
					if (message.IndexOf("WSAGetOverlappedResult") >= 0 && message.IndexOf("CompletionPortCallback") >= 0 )
						_log.Debug("Unhandled exception ignored: ", (Exception)e.ExceptionObject);
					return;	// ignore. See comment above :-(
				}

				DialogResult result = DialogResult.Cancel;
				Exception        ex = null;

				// The log is an extra backup in case the stack trace doesn't
				// get fully included in the exception.
				//string logName = RssBanditApplication.GetLogFileName();
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
					StringBuilder errorMsg = new StringBuilder(SR.ExceptionGeneralCritical(RssBanditApplication.GetLogFileName()));
					errorMsg.Append("\n"+e.Message);
					if (Application.MessageLoop && e.Source != null)
						errorMsg.Append("\n@:" + e.Source);
					return MessageBox.Show(errorMsg.ToString(), SR.GUIErrorMessageBoxCaption + " " + RssBanditApplication.Caption, (resumable ? MessageBoxButtons.AbortRetryIgnore: MessageBoxButtons.OK), MessageBoxIcon.Stop);
				} catch (Exception ex){ 
					_log.Error("Critical exception in ShowExceptionDialog() ", ex);
					/* */ 
				}
				return DialogResult.Abort;
			}

		}

		#endregion

		#region ICoreApplication Members

		/// <summary>
		/// Shows the podcast options.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="optionsChangedHandler">The options changed handler.</param>
		public void ShowPodcastOptionsDialog(IWin32Window owner, EventHandler optionsChangedHandler) 
		{
			using(PodcastOptionsDialog optionDialog = new PodcastOptionsDialog(Preferences, this)) 
			{
				optionDialog.ShowDialog(owner == null ? guiMain: owner);
				if(optionDialog.DialogResult == DialogResult.OK) 
				{
					
					//modify preferences with data from dialog
					this.feedHandler.PodcastFileExtensionsAsString  = optionDialog.textPodcastFilesExtensions.Text;
					
					if(optionDialog.chkCopyPodcastToFolder.Checked){
						this.feedHandler.PodcastFolder          =  optionDialog.txtCopyPodcastToFolder.Text;
					}else{
						this.feedHandler.PodcastFolder = this.feedHandler.EnclosureFolder;
					}

					this.Preferences.AddPodcasts2Folder		= optionDialog.chkCopyPodcastToFolder.Checked;
					this.Preferences.AddPodcasts2ITunes		= optionDialog.chkCopyPodcastToITunesPlaylist.Checked;
					this.Preferences.AddPodcasts2WMP		= optionDialog.chkCopyPodcastToWMPlaylist.Checked;

					this.Preferences.SinglePodcastPlaylist  = optionDialog.optSinglePlaylistName.Checked;
					this.Preferences.SinglePlaylistName		= optionDialog.textSinglePlaylistName.Text;
				
				
					// apply to backend, UI etc. and save:
					this.ApplyPreferences();
					this.SavePreferences();
				
					// notify service callbacks:
					if (optionsChangedHandler != null) 
					{
						try {
							optionsChangedHandler.Invoke(this, EventArgs.Empty);
						} catch (Exception ex) {
							_log.Error("ShowPodcastOptions() change handler caused exception", ex);
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Display the options dialog and select the desired detail section
		/// </summary>
		/// <param name="selectedSection">OptionDialogSection</param>
		/// <param name="owner">The owner.</param>
		/// <param name="optionsChangedHandler">The options changed handler.</param>
		public void ShowOptions(OptionDialogSection selectedSection, IWin32Window owner, EventHandler optionsChangedHandler) {
			
			if (!this.SearchEngineHandler.EnginesLoaded || !this.SearchEngineHandler.EnginesOK)
				this.LoadSearchEngines();

			PreferencesDialog propertiesDialog = new PreferencesDialog(this, refreshRate/60000, Preferences, this.searchEngines, this.IdentityManager); 
			propertiesDialog.OnApplyPreferences += new EventHandler(this.OnApplyPreferences);
			if (optionsChangedHandler != null)
				propertiesDialog.OnApplyPreferences += optionsChangedHandler;

			// just to check..
			//propertiesDialog.Disposed += new EventHandler(OnDialogDisposed);

			propertiesDialog.SelectedSection = selectedSection;
			propertiesDialog.ShowDialog(owner == null ? guiMain: owner);

			if(propertiesDialog.DialogResult == DialogResult.OK) {
				this.OnApplyPreferences(propertiesDialog, new EventArgs());
				if (optionsChangedHandler != null)
					optionsChangedHandler(propertiesDialog, new EventArgs());
			}

			// detach event(s) to get the dialog garbage collected:
			propertiesDialog.OnApplyPreferences -= new EventHandler(this.OnApplyPreferences);
			if (optionsChangedHandler != null)
				propertiesDialog.OnApplyPreferences -= optionsChangedHandler;

			//cleanup
			propertiesDialog.Dispose(); 
		}

		/// <summary>
		/// Shows the NNTP server management dialog.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="definitionChangeEventHandler">The definition change event handler.</param>
		public void ShowNntpServerManagementDialog(IWin32Window owner, EventHandler definitionChangeEventHandler) {
			if (definitionChangeEventHandler != null)
				this.NntpServerManager.NewsServerDefinitionsModified += definitionChangeEventHandler;
			this.NntpServerManager.ShowNewsServerSubscriptionsDialog(owner == null ? guiMain: owner);
			if (definitionChangeEventHandler != null)
				this.NntpServerManager.NewsServerDefinitionsModified -= definitionChangeEventHandler;
		}

		/// <summary>
		/// Shows the user identity management dialog.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="definitionChangeEventHandler">The definition change event handler.</param>
		public void ShowUserIdentityManagementDialog(IWin32Window owner, EventHandler definitionChangeEventHandler) {
			if (definitionChangeEventHandler != null)
				this.IdentityManager.IdentityDefinitionsModified += definitionChangeEventHandler;
			this.IdentityManager.ShowIdentityDialog(owner == null ? guiMain: owner);
			if (definitionChangeEventHandler != null)
				this.IdentityManager.IdentityDefinitionsModified -= definitionChangeEventHandler;
		}

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
					this.guiMain.CreateSubscriptionsCategoryHive(this.guiMain.GetRoot(RootFolderType.MyFeeds), category);
				}
					
			}
		}

		public bool SearchForFeeds(string searchTerm) {
			return SubscribeToFeed(null, null, null, searchTerm, WizardMode.SubscribeSearchDirect);
		}

		public bool SubscribeToFeed(string url, string category, string title)
		{
			WizardMode mode = WizardMode.Default;
			if ( ! StringHelper.EmptyOrNull(url) ) {
				mode = WizardMode.SubscribeURLDirect;
				if (RssHelper.IsNntpUrl(url))
					mode = WizardMode.SubscribeNNTPGroupDirect;
			}
			return SubscribeToFeed(url, category, title, null, mode);
		}
		
		public bool SubscribeToFeed(string url, string category, string title, string searchTerms, WizardMode mode) {
			
			AddSubscriptionWizard wiz = new AddSubscriptionWizard(this, mode);
			wiz.FeedUrl = (url == null ? String.Empty: url);
			if (category != null)	// does remember the last category:
				wiz.FeedCategory = category;
			wiz.FeedTitle = (title == null ? String.Empty: title);
			wiz.SearchTerms = (searchTerms == null ? String.Empty: searchTerms);
			
			try {
				if (MainForm.IsHandleCreated)
					Win32.SetForegroundWindow(MainForm.Handle);
				wiz.ShowDialog(guiMain);
			} catch (Exception ex){
				_log.Error("SubscribeToFeed caused exception.", ex);
				wiz.DialogResult = DialogResult.Cancel;
			}

			if (wiz.DialogResult == DialogResult.OK) {
				
				feedsFeed f = null; 

				if (wiz.MultipleFeedsToSubscribe) {
					
					bool anySubscription = false;
					
					for (int i = 0; i < wiz.MultipleFeedsToSubscribeCount; i++) {

						f = this.CreateFeedFromWizard(wiz, i);
						if(f == null) {
							continue;
						}
							
						// add feed visually
						guiMain.AddNewFeedNode(f.category, f);

						if (wiz.FeedInfo == null)
							guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

						anySubscription = true;
						
					}
					
					wiz.Dispose(); 
					return anySubscription;

				} else {
					
					f = this.CreateFeedFromWizard(wiz, 0);
					
					if(f == null) {
						wiz.Dispose(); 
						return false;
					}

					// add feed visually
					guiMain.AddNewFeedNode(f.category, f);

					if (wiz.FeedInfo == null)
						guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

					wiz.Dispose(); 
					return true;
				}

//				f.link  = wiz.FeedUrl; 
//				if(feedHandler.FeedsTable.Contains(f.link)) {
//					feedsFeed f2 = feedHandler.FeedsTable[f.link]; 
//					this.MessageInfo("RES_GUIFieldLinkRedundantInfo", 
//						(f2.category == null? String.Empty : category + "\\") + f2.title, f2.link );
//					wiz.Dispose(); 
//					return false; 
//				}
//
//				f.title = wiz.FeedTitle;
//				f.category = wiz.FeedCategory;
//				if((f.category != null) && (!feedHandler.Categories.ContainsKey(f.category))) {
//					feedHandler.Categories.Add(f.category); 
//				}
//
//				if ( !StringHelper.EmptyOrNull( wiz.FeedCredentialUser) ) {	// set feedsFeed new credentials
//					string u = wiz.FeedCredentialUser, p = null;
//					if (!StringHelper.EmptyOrNull( wiz.FeedCredentialPwd) )
//						p = wiz.FeedCredentialPwd;
//					NewsHandler.SetFeedCredentials(f, u, p);
//				} else {
//					NewsHandler.SetFeedCredentials(f, null, null);
//				}
//
//				f.alertEnabled = f.alertEnabledSpecified = wiz.AlertEnabled;
//
//				// add feed to backend
//				if (wiz.FeedInfo != null)
//					feedHandler.AddFeed(f, wiz.FeedInfo);
//				else
//					feedHandler.FeedsTable.Add(f.link, f); 
//				this.FeedlistModified = true;
//
//				// set properties the backend requires the feed yet added
//				f.refreshrate = 60;	// init a default
//				int intIn = wiz.RefreshRate * MilliSecsMultiplier;
//				this.feedHandler.SetRefreshRate(f.link, intIn);
//				this.feedHandler.SetMaxItemAge(f.link, wiz.MaxItemAge);
//				this.feedHandler.SetMarkItemsReadOnExit(f.link, wiz.MarkItemsReadOnExit); 
//
//				string stylesheet = wiz.FeedStylesheet;
//
//				if(stylesheet != null && !stylesheet.Equals(this.feedHandler.GetStyleSheet(f.link))){
//					this.feedHandler.SetStyleSheet(f.link,stylesheet ); 						
//						
//					if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet)){
//						this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
//					}
//				}
//
//				// add feed visually
//				guiMain.AddNewFeedNode(f.category, f);
//
//				if (wiz.FeedInfo == null)
//					guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);
//
//				wiz.Dispose(); 
//				return true;
			}

			wiz.Dispose(); 
			return false;
		}

		private feedsFeed CreateFeedFromWizard(AddSubscriptionWizard wiz, int index) {
			feedsFeed f = new feedsFeed(); 

			f.link  = wiz.FeedUrls(index); 
			if(feedHandler.FeedsTable.Contains(f.link)) {
				feedsFeed f2 = feedHandler.FeedsTable[f.link]; 
				this.MessageInfo(SR.GUIFieldLinkRedundantInfo( 
					(f2.category == null? String.Empty : f2.category + NewsHandler.CategorySeparator) + f2.title, f2.link ));
				
				return null; 
			}

			f.title = wiz.FeedTitles(index);
			f.category = wiz.FeedCategory;
			if((f.category != null) && (!feedHandler.Categories.ContainsKey(f.category))) {
				feedHandler.Categories.Add(f.category); 
			}

			if ( !StringHelper.EmptyOrNull( wiz.FeedCredentialUser) ) {	// set feedsFeed new credentials
				string u = wiz.FeedCredentialUser, p = null;
				if (!StringHelper.EmptyOrNull( wiz.FeedCredentialPwd) )
					p = wiz.FeedCredentialPwd;
				NewsHandler.SetFeedCredentials(f, u, p);
			} else {
				NewsHandler.SetFeedCredentials(f, null, null);
			}

			f.alertEnabled = f.alertEnabledSpecified = wiz.AlertEnabled;

			// add feed to backend
			if (wiz.FeedInfo != null)
				feedHandler.AddFeed(f, wiz.FeedInfo);
			else
				feedHandler.FeedsTable.Add(f.link, f); 
			this.FeedWasModified(f, NewsFeedProperty.FeedAdded);
			//this.FeedlistModified = true;
			
			/* DON'T NEED THIS, IT WILL INHERIT FROM NewsHandler.RefreshRate
				f.refreshrate = 60;	
				int intIn = wiz.RefreshRate * MilliSecsMultiplier;
				this.feedHandler.SetRefreshRate(f.link, intIn); 
			*/ 
			// set properties the backend requires the feed yet added
			this.feedHandler.SetMaxItemAge(f.link, wiz.MaxItemAge);
			this.feedHandler.SetMarkItemsReadOnExit(f.link, wiz.MarkItemsReadOnExit); 

			string stylesheet = wiz.FeedStylesheet;

			if(stylesheet != null && !stylesheet.Equals(this.feedHandler.GetStyleSheet(f.link))){
				this.feedHandler.SetStyleSheet(f.link,stylesheet ); 						
						
				if(!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet)){
					this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
				}
			}

			return f;
		}

		/// <summary>
		/// Unsubscribes the feed by feedUrl.
		/// </summary>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="askUser">if set to <c>true</c> [ask user].</param>
		public void UnsubscribeFeed(string feedUrl, bool askUser) {
			UnsubscribeFeed(GetFeed(feedUrl), askUser);
		}
		/// <summary>
		/// Unsubscribes the feed.
		/// </summary>
		/// <param name="feed">The feed.</param>
		/// <param name="askUser">if set to <c>true</c> [ask user].</param>
		public void UnsubscribeFeed(feedsFeed feed, bool askUser) {
			if (feed == null) return;
				
			TreeFeedsNodeBase tn = (TreeFeedsNodeBase)feed.Tag; 
			if (tn != null) {
				if (askUser) {
					guiMain.CurrentSelectedFeedsNode = tn;
					guiMain.CmdDeleteFeed(null);
					guiMain.CurrentSelectedFeedsNode = null;
				} else {
					guiMain.CurrentSelectedFeedsNode = tn;
					this.DeleteFeed(feed.link);
					guiMain.CurrentSelectedFeedsNode = null;
				}
			}

		}
		
		public IDictionary Subscriptions {
			get { return new ReadOnlyDictionary(this.feedHandler.FeedsTable);}
		}
		IDictionary AppServices.ICoreApplication.Identities {
            get { return new ReadOnlyDictionary((IDictionary) this.IdentityManager.CurrentIdentities); }	
		}
		IDictionary AppServices.ICoreApplication.NntpServerDefinitions {
			get { return new ReadOnlyDictionary((IDictionary)this.NntpServerManager.CurrentNntpServers); }	
		}

		IList AppServices.ICoreApplication.GetNntpNewsGroups(string nntpServerName, bool forceReloadFromServer) {
			if (! StringHelper.EmptyOrNull(nntpServerName) &&
				this.NntpServerManager.CurrentNntpServers.ContainsKey(nntpServerName)) {
				NntpServerDefinition sd = (NntpServerDefinition)this.NntpServerManager.CurrentNntpServers[nntpServerName];
				if (sd != null)
					return (IList) this.NntpServerManager.LoadNntpNewsGroups(guiMain, sd, forceReloadFromServer);
			}
			return new string[]{};
		}

		/// <summary>
		/// Gets the News Item Formatter Stylesheet list.
		/// </summary>
		/// <returns></returns>
		public IList GetItemFormatterStylesheets() {
			string tmplFolder = RssBanditApplication.GetTemplatesPath();
			
			if (Directory.Exists(tmplFolder)) {
				string[] tmplFiles = Directory.GetFiles(tmplFolder, "*.fdxsl");
                List<string> formatters = new List<string>(tmplFiles.GetLength(0));
				foreach (string filename in tmplFiles) {
					formatters.Add(Path.GetFileNameWithoutExtension(filename)); 
				}
				return formatters;
			
			}else {
                return new List<string>(0);
			}	
		}

		/// <summary>
		/// Gets the defined web search engines. 
		/// Items are of type ISearchEngine, keys are the corresponding Title.
		/// </summary>
		IList AppServices.ICoreApplication.WebSearchEngines {
			get { return ArrayList.ReadOnly(this.SearchEngineHandler.Engines); }
		}

		bool AppServices.ICoreApplication.SubscribeToFeed(string url, string category){
			return this.SubscribeToFeed(url, category, null);
		}

		bool AppServices.ICoreApplication.SubscribeToFeed(string url){
			return this.SubscribeToFeed(url, DefaultCategory, null);
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

		/// <summary>
		/// Navigates to an provided Url on the user preferred Web Browser.
		/// So it may be the external OS Web Browser, or the internal one.
		/// </summary>
		/// <param name="url">Url to navigate to</param>
		/// <param name="tabCaption">The suggested tab caption (maybe replaced by the url's html page title)</param>
		/// <param name="forceNewTabOrWindow">Force to open a new Browser Window (Tab)</param>
		/// <param name="setFocus">Force to set the focus to the new Window (Tab)</param>
		public void NavigateToUrlAsUserPreferred(string url, string tabCaption, bool forceNewTabOrWindow, bool setFocus) {
			if (BrowserBehaviorOnNewWindow.OpenNewTab == Preferences.BrowserOnNewWindow)  {
				NavigateToUrl(url, tabCaption, forceNewTabOrWindow, setFocus);
			} else if (BrowserBehaviorOnNewWindow.OpenDefaultBrowser == Preferences.BrowserOnNewWindow) {
				NavigateToUrlInExternalBrowser(url);
			}
		}

		/// <summary>
		/// Navigates to an provided Url with help of the OS system preferred Web Browser.
		/// If it fails to navigate with that browser, it falls back to internal tabbed browsing.
		/// </summary>
		/// <param name="url">Url to navigate to</param>
		public void NavigateToUrlInExternalBrowser(string url) {
			if (StringHelper.EmptyOrNull(url))
				url = "about:blank";
			try {
				Process.Start(url);
			} catch (Exception  ex) {
				if (this.MessageQuestion(SR.ExceptionStartDefaultBrowserMessage(ex.Message, url)) == DialogResult.Yes) {
					this.NavigateToUrl(url, "Web", true, true);
				} 
			}
		}

		#region NewsChannel Manangement		

		/// <summary>
		/// Register a IChannelProcessor services, that works
		/// in the receiving news channel chain: the moment we requested new feeds
		/// or update feeds from the original sources. 
		/// </summary>
		/// <param name="channelProcessor">IChannelProcessor</param>
		void ICoreApplication.RegisterReceivingNewsChannelProcessor(IChannelProcessor channelProcessor) {
			if (channelProcessor == null)
				return;

			INewsChannel[] channels = channelProcessor.GetChannels();
			if (channels == null || channels.Length==0)
				return;
			foreach (INewsChannel channel in channels) {
				feedHandler.RegisterReceivingNewsChannel(channel);
			}
			
		}

		/// <summary>
		/// Unregister a previously registered IChannelProcessor services 
		/// and removes it from the receiving news channel processing chain.
		/// </summary>
		/// <param name="channelProcessor">IChannelProcessor</param>
		void ICoreApplication.UnregisterReceivingNewsChannelProcessor(IChannelProcessor channelProcessor) {
			if (channelProcessor == null)
				return;

			INewsChannel[] channels = channelProcessor.GetChannels();
			if (channels == null || channels.Length==0)
				return;
			foreach (INewsChannel channel in channels) {
				feedHandler.UnregisterReceivingNewsChannel(channel);
			}
			
		}

		
		/// <summary>
		/// Register a IChannelProcessor services, that works
		/// in the displaying news channel chain: the moment before we render feeds
		/// or newsitems in the detail display pane. 
		/// </summary>
		/// <param name="channelProcessor">IChannelProcessor</param>
		public void RegisterDisplayingNewsChannelProcessor (IChannelProcessor channelProcessor) {
			if (channelProcessor == null)
				return;

			INewsChannel[] channels = channelProcessor.GetChannels();
			if (channels == null || channels.Length==0)
				return;
			foreach (INewsChannel channel in channels) {
				displayingNewsChannel.RegisterNewsChannel(channel);
			}
		}

		/// <summary>
		/// Unregister a previously registered IChannelProcessor services 
		/// and removes it from the receiving news channel processing chain.
		/// </summary>
		/// <param name="channelProcessor">IChannelProcessor</param>
		public void UnregisterDisplayingNewsChannelProcessor (IChannelProcessor channelProcessor) {
			if (channelProcessor == null)
				return;

			INewsChannel[] channels = channelProcessor.GetChannels();
			if (channels == null || channels.Length==0)
				return;
			foreach (INewsChannel channel in channels) {
				displayingNewsChannel.UnregisterNewsChannel(channel);
			}
		}

		/// <summary>
		/// Gets the receiving news channel.
		/// </summary>
		/// <value>The displaying news channel services.</value>
		internal static NewsChannelServices DisplayingNewsChannelServices {
			get { return displayingNewsChannel; }
		}

		#endregion

		#endregion

		/// <summary>
		/// Internal accessor to the ICoreApplication interface services.
		/// </summary>
		internal ICoreApplication CoreServices {
			get { return this; }
		}

		private static void ApplyResourceNameFix() {
			CreateCopyCulture("en-029", "en-CB");
			CreateCopyCulture("az-Latn-AZ", "az-AZ-Latn");
			CreateCopyCulture("uz-Latn-UZ", "uz-UZ-Latn");
			CreateCopyCulture("sr-Latn-CS", "sr-SP-Latn");
			CreateCopyCulture("az-Cyrl-AZ", "az-AZ-Cyrl");
			CreateCopyCulture("uz-Cyrl-UZ", "uz-UZ-Cyrl");
			CreateCopyCulture("sr-Cyrl-CS", "sr-SP-Cyrl");
			CreateCopyCulture("bs-Cyrl-BA", "bs-BA-Cyrl");
			CreateCopyCulture("sr-Latn-BA", "sr-BA-Latn");
			CreateCopyCulture("sr-Cyrl-BA", "sr-BA-Cyrl");
			CreateCopyCulture("bs-Latn-BA", "bs-BA-Latn");
			CreateCopyCulture("iu-Latn-CA", "iu-CA-Latn");
			CreateCopyCulture("dv-MV", "div-MV");
			// zh-Hant & zh-Hans (zh-CHS/zh-CHT) are already aliased.
		}
		static void CreateCopyCulture(string strRealName, string strAliasName)
		{
			try
			{
				// Create a new culture based on the old name
				CultureAndRegionInfoBuilder carib = new CultureAndRegionInfoBuilder(
					strAliasName, CultureAndRegionModifiers.None);

				carib.LoadDataFromCultureInfo(new CultureInfo(strRealName));
				carib.LoadDataFromRegionInfo(new RegionInfo(strRealName));

				carib.Register();

				// Change the existing culture's parent to the old culture
				carib = new CultureAndRegionInfoBuilder(strRealName,
								CultureAndRegionModifiers.Replacement);

				carib.Parent = new CultureInfo(strAliasName);
				carib.Register();

				// Verify they're registered...
				CultureInfo ci = new CultureInfo(strAliasName);
				Trace.WriteLine(String.Format("Aliased culture {0} has parent of {1}.", ci, ci.Parent));
				ci = new CultureInfo(strRealName);
				Trace.WriteLine(String.Format("\"Real\" culture {0} has parent of {1}.", ci, ci.Parent));
			}
			catch (Exception e)
			{
				Trace.WriteLine("Unable to create custom culture " + strAliasName);
				Trace.WriteLine(e);
			}
		}

/*
		private void OnDialogDisposed(object sender, EventArgs e) {
			Trace.WriteLine("Dialog Disposed('" + sender.GetType().Name.ToString() + "')");
		}
*/
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

#region CVS Version Log
/*
 * $Log: RssBanditApplication.cs,v $
 * Revision 1.385  2007/07/28 20:11:50  t_rendelmann
 * small fix to make AppDataFolder and LocalDataFolder work with rooted paths NOT containing the drive letter
 *
 * Revision 1.384  2007/07/27 19:08:09  carnage4life
 * Moved unhandled exception handler
 *
 * Revision 1.383  2007/07/26 02:50:56  carnage4life
 * Fixed issue where global refresh rate is not applied to newly subscribed feeds
 *
 * Revision 1.382  2007/07/21 12:26:55  t_rendelmann
 * added support for "portable Bandit" version
 *
 * Revision 1.381  2007/07/01 17:59:54  t_rendelmann
 * feature: support for portable application mode (running Bandit from a stick)
 *
 * Revision 1.380  2007/06/19 17:51:53  t_rendelmann
 * fixed: [ 1736842 ] Crash when clicking on enclosure link in toast window
 *
 * Revision 1.379  2007/06/19 15:01:22  t_rendelmann
 * fixed: [ 1702811 ] Cannot access .treestate.xml (now report a read error as a warning once, but log only at save time during autosave)
 *
 * Revision 1.378  2007/06/15 11:02:00  t_rendelmann
 * new: enabled user failure actions within feed error reports (validate, navigate to, delete subscription)
 *
 * Revision 1.377  2007/06/14 01:02:27  carnage4life
 * Fixed issue where Changes to feed-specific enclosure/attachment download settings are not remembered
 *
 * Revision 1.376  2007/06/13 22:53:17  carnage4life
 * Fixed one place where malformed searchfolders.xml could cause application crash
 *
 * Revision 1.375  2007/06/12 18:55:56  t_rendelmann
 * changed: added the xslt param "LimitNewsItemsPerPage" to conditionally page items
 *
 * Revision 1.374  2007/06/12 17:34:22  t_rendelmann
 * fixed: Cache location error (http://sourceforge.net/tracker/index.php?func=detail&aid=1667884&group_id=96589&atid=615248)
 *
 * Revision 1.373  2007/06/09 16:06:02  carnage4life
 * Raised version number to 1.5.0.14
 *
 * Revision 1.372  2007/06/07 02:04:17  carnage4life
 * Added pages in the newspaper view when displaying unread items for a feed
 *
 * Revision 1.371  2007/06/03 17:03:08  carnage4life
 * no message
 *
 * Revision 1.370  2007/05/29 13:20:40  carnage4life
 * no message
 *
 * Revision 1.369  2007/05/20 00:54:42  carnage4life
 * Added option to disable automatically marking posts as read when viewed in the default stylsheet
 *
 * Revision 1.368  2007/05/18 22:18:00  carnage4life
 * Added UI option for disabling automatically marking items as read on default newspaper stylesheet
 *
 * Revision 1.367  2007/05/18 15:10:26  t_rendelmann
 * fixed: node selection after feed/category deletion behavior. Now the new node to select after a deletion is controlled by the new method TreeHelper.GetNewNodeToActivate()
 *
 * Revision 1.366  2007/05/18 11:46:45  t_rendelmann
 * fixed: no category context menus displayed after OPML import or remote sync.
 *
 * Revision 1.365  2007/05/15 23:38:47  carnage4life
 * Added UI options for limiting number of news items displayed per page in the newspaper view
 *
 * Revision 1.364  2007/05/12 18:15:18  carnage4life
 * Changed a number of APIs to treat feed URLs as System.String instead of System.Uri because some feed URLs such as those containing unicode cannot be used to create instances of System.Uri
 *
 * Revision 1.363  2007/05/05 10:45:43  t_rendelmann
 * fixed: lucene indexing issues caused by thread race condition
 *
 * Revision 1.362  2007/05/03 15:58:06  t_rendelmann
 * fixed: toggle read state from within html detail pane (javascript initiated) not always toggle the read state within the listview and subscription tree (caused if item ID is Url-encoded)
 *
 * Revision 1.361  2007/04/30 10:02:08  t_rendelmann
 * fixed: null-ref-exception in user notification messageboxes if no UI was yet active
 *
 * Revision 1.360  2007/03/29 10:44:33  t_rendelmann
 * new: detail header title now also display the unread counter of the selected node (additional to caption)
 *
 * Revision 1.359  2007/03/19 12:02:37  t_rendelmann
 * changed: better error reporting on AddIn's
 *
 * Revision 1.358  2007/03/19 10:43:03  t_rendelmann
 * changed: better handling of favicon's (driven by extension now); we are now looking for the smallest and smoothest icon image to use (if ICO)
 *
 * Revision 1.357  2007/03/13 16:50:49  t_rendelmann
 * fixed: new feed source dialog is now modal (key events are badly processed by parent window)
 *
 * Revision 1.356  2007/03/10 18:26:12  t_rendelmann
 * feature: separate dialog to view feed sources
 *
 * Revision 1.355  2007/03/10 15:01:21  t_rendelmann
 * changed: use our own xslt to display XML sources in IE
 *
 * Revision 1.354  2007/03/05 16:37:28  t_rendelmann
 * fixed: check for updates is always disabled
 *
 * Revision 1.353  2007/03/04 20:53:19  carnage4life
 * Changes related to shipping the v1.5.0.10 release
 *
 * Revision 1.352  2007/02/18 14:43:17  t_rendelmann
 * some cleanup in the code installing the default feedlist
 *
 * Revision 1.351  2007/02/17 22:43:20  carnage4life
 * Clarified text and behavior around showing full item text in search folders
 *
 * Revision 1.350  2007/02/13 21:29:00  t_rendelmann
 * fixed: Ask for default aggregator should not be displayed in case of a UAC violation
 *
 * Revision 1.349  2007/02/11 17:03:12  carnage4life
 * Made following changes to default newspaper view
 * 1.) Alt text now shows up for action icons in newspaper view
 * 2.) Items marked as unread by the user aren't toggled to read automatically by scrolling over them
 * 3.) Scrolling only marks items as read instead of toggling read state
 *
 * Revision 1.348  2007/02/09 15:44:19  t_rendelmann
 * changed: refactored LastAutoUpdateCheck property (moved from Preferences to GuiSettings storage)
 *
 * Revision 1.347  2007/02/09 14:54:08  t_rendelmann
 * fixed: added missing configuration option for newComments font style and color;
 * changed: some refactoring in FontColorHelper;
 *
 * Revision 1.346  2007/02/01 16:00:42  t_rendelmann
 * fixed: option "Initiate download feeds at startup" was not taken over to the Options UI checkbox
 * fixed: Deserialization issue with Preferences types of wrong AppServices assembly version
 * fixed: OnPreferencesChanged() event was not executed at the main thread
 * changed: prevent execptions while deserialize DownloadTask
 *
 * Revision 1.345  2007/01/30 21:17:43  carnage4life
 * Added support for remembering browser tab state on restart
 *
 * Revision 1.344  2007/01/22 16:42:10  carnage4life
 * Changes to fix issues before shipping Jubilee release candidate
 *
 * Revision 1.343  2007/01/20 18:11:47  carnage4life
 * Added support for watching comments from the Newspaper view
 *
 * Revision 1.342  2007/01/18 18:21:31  t_rendelmann
 * code cleanup (old search controls and code removed)
 *
 * Revision 1.341  2007/01/18 04:03:08  carnage4life
 * Completed support for custom newspaper view for search results
 *
 * Revision 1.340  2007/01/14 19:30:47  t_rendelmann
 * cont. SearchPanel: first main form integration and search working (scope/populate search scope tree is still a TODO)
 *
 * Revision 1.339  2006/12/26 17:18:04  carnage4life
 * Fixed issue that hitting [space] while editing nodes moved to the next unread item
 *
 * Revision 1.338  2006/12/24 14:03:23  carnage4life
 * Fixed issue where IE security band is displayed even when ActiveX is explicitly enabled by the user.
 *
 * Revision 1.337  2006/12/17 14:55:44  t_rendelmann
 * added: consider sound configuration setting (allowed, not allowed)
 *
 * Revision 1.336  2006/12/17 14:07:00  t_rendelmann
 * added: option to control application sounds and configuration;
 * added option to control Bandit startup as windows user logon;
 *
 * Revision 1.335  2006/12/16 23:15:51  carnage4life
 * Fixed issue where comment feeds get confused when a comment is deleted from the feed,
 *
 * Revision 1.334  2006/12/16 22:26:51  carnage4life
 * Added CopyItemTo method that copies a NewsItem to a specific feedsFeed and does the logic to load item content from disk if needed
 *
 * Revision 1.333  2006/12/16 15:52:34  t_rendelmann
 * removed unused image strips;
 * now calling a Windows.Forms timer to apply UI configuration save to prevent cross-thread exceptions;
 *
 * Revision 1.332  2006/12/16 15:09:36  t_rendelmann
 * feature: application sound support (configurable via Windows Sounds Control Panel)
 *
 * Revision 1.331  2006/12/14 18:52:20  carnage4life
 * Removed redundant 'Subscribe in defautl aggregator' option added to IE right-click menu
 *
 * Revision 1.330  2006/12/14 16:34:06  t_rendelmann
 * finished: all toolbar migrations; removed Sandbar toolbars from MainUI
 *
 * Revision 1.329  2006/12/12 16:20:56  carnage4life
 * Fixed issue where Attachments/Podcasts option "Enable alert window for new downl. attachments" did not get persisted
 *
 * Revision 1.328  2006/12/09 22:57:03  carnage4life
 * Added support for specifying how many podcasts downloaded from new feeds
 *
 * Revision 1.327  2006/12/08 17:00:22  t_rendelmann
 * fixed: flag a item with no content did not show content anymore in the flagged item view (linked) (was regression because of the dynamic load of item content from cache);
 * fixed: "View Outlook Reading Pane" was not working correctly (regression from the toolbars migration);
 *
 * Revision 1.326  2006/12/05 04:06:25  carnage4life
 * Made changes so that when comments for an item are viewed from Watched Items folder, the actual feed is updated and vice versa
 *
 * Revision 1.325  2006/12/03 01:20:13  carnage4life
 * Made changes to support Watched Items feed showing when new comments found
 *
 * Revision 1.324  2006/11/28 18:08:40  t_rendelmann
 * changed; first version with the new menubar and the main toolbar migrated to IG - still work in progress
 *
 * Revision 1.323  2006/11/27 19:11:49  carnage4life
 * Made changes to get around issues with installer for Jubilee beta 1
 *
 * Revision 1.322  2006/11/25 18:38:16  carnage4life
 * Changes for JUBILEE beta 1 release
 *
 * Revision 1.321  2006/11/22 00:14:03  carnage4life
 * Added support for last of Podcast options
 *
 * Revision 1.320  2006/11/21 17:25:53  carnage4life
 * Made changes to support options for Podcasts
 *
 * Revision 1.319  2006/11/20 22:26:20  carnage4life
 * Added support for most of the Podcast and Attachment options except for podcast file extensions and copying podcasts to a specified folder
 *
 * Revision 1.318  2006/11/19 03:11:10  carnage4life
 * Added support for persisting podcast settings when changed in the Preferences dialog
 *
 * Revision 1.317  2006/11/14 17:30:49  t_rendelmann
 * fixed: SharedUICulture not always initialized and cause an exception later on using SR.strings generated class
 *
 * Revision 1.316  2006/11/12 16:24:35  carnage4life
 * 1.) Removed ability to override enclosure folder per feed or category
 * 2.) Moved code to resume pending enclosure downloads until after feedlist is loaded to avoid race condition
 *
 * Revision 1.315  2006/11/12 01:25:01  carnage4life
 * 1.) Added Support for Alert windows on received podcasts.
 * 2.) Fixed feed mixup issues
 *
 * Revision 1.314  2006/11/11 14:42:41  t_rendelmann
 * added: DialogBase base Form to be able to inherit simple OK/Cancel dialogs;
 * added new PodcastOptionsDialog (inherits DialogBase)
 *
 * Revision 1.313  2006/11/05 15:33:07  t_rendelmann
 * added: Attachments/Podcasts tab to options dialog; some dialog widget names changed
 *
 * Revision 1.312  2006/11/05 01:23:55  carnage4life
 * Reduced time consuming locks in indexing code
 *
 * Revision 1.311  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 * Revision 1.310  2006/10/28 23:10:00  carnage4life
 * Added "Attachments/Podcasts" to Feed Properties and Category properties dialogs.
 *
 * Revision 1.309  2006/10/28 16:38:25  t_rendelmann
 * added: new "Unread Items" folder, not anymore based on search, but populated directly with the unread items
 *
 * Revision 1.308  2006/10/27 01:57:54  carnage4life
 * Added support for adding newly downloaded podcasts to playlists in Windows Media Player or iTunes
 *
 * Revision 1.307  2006/10/24 15:15:13  carnage4life
 * Changed the default folders for podcasts
 *
 * Revision 1.306  2006/10/21 23:34:16  carnage4life
 * Changes related to adding the "Download Attachment" right-click menu option in the list view
 *
 * Revision 1.305  2006/10/17 15:23:26  carnage4life
 * Integrated BITS code for downloading enclosures
 *
 * Revision 1.304  2006/10/10 17:43:28  t_rendelmann
 * feature: added a commandline option to allow users to reset the UI (don't init from .settings.xml);
 * fixed: explorer bar state was not saved/restored, corresponding menu entries hold the wrong state on explorer group change
 *
 * Revision 1.303  2006/10/05 17:58:31  t_rendelmann
 * fixed: last selected node activated on startup after restore the treestate did not populated the listview/detail pane
 *
 * Revision 1.302  2006/10/05 15:46:29  t_rendelmann
 * rework: now using XmlSerializerCache everywhere to get the XmlSerializer instance
 *
 * Revision 1.301  2006/10/05 14:59:03  t_rendelmann
 * feature: restore the subscription tree state (expansion, selection) also after remote download of the subscriptions
 *
 * Revision 1.300  2006/10/05 14:45:04  t_rendelmann
 * added usage of the XmlSerializerCache to prevent the Xml Serializer leak for the new
 * feature: persist the subscription tree state (expansion, selection)
 *
 * Revision 1.299  2006/09/29 18:14:36  t_rendelmann
 * a) integrated lucene index refreshs;
 * b) now using a centralized defined category separator;
 * c) unified decision about storage relevant changes to feed, feed and feeditem properties;
 * d) fixed: issue [ 1546921 ] Extra Category Folders Created
 * e) fixed: issue [ 1550083 ] Problem when renaming categories
 *
 * Revision 1.298  2006/09/15 19:03:33  t_rendelmann
 * added version info to log on application startup
 *
 * Revision 1.297  2006/09/12 10:53:45  t_rendelmann
 * changed: MainForm.Invoke calles replaced by .BeginInvoke to avoid thread locks (places, where we expect to receive results from threads)
 *
 * Revision 1.296  2006/09/07 16:47:44  carnage4life
 * Fixed two issues
 * 1. Added SelectedImageIndex and ImageIndex to FeedsTreeNodeBase
 * 2. Fixed issue where watched comments always were treated as having new comments on HTTP 304
 *
 * Revision 1.295  2006/09/07 00:48:36  carnage4life
 * Fixed even more bugs with comment watching and comment feeds
 *
 * Revision 1.294  2006/09/05 05:26:27  carnage4life
 * Fixed a number of bugs in comment watching code for comment feeds
 *
 * Revision 1.293  2006/09/03 19:08:50  carnage4life
 * Added support for favicons
 *
 * Revision 1.292  2006/09/01 02:01:41  carnage4life
 * Added "Load new browser tabs in background"
 *
 * Revision 1.291  2006/08/31 21:52:31  carnage4life
 * Fixed issue with subscribing to watched comment feeds
 *
 * Revision 1.290  2006/08/18 19:10:57  t_rendelmann
 * added an "id" XML attribute to the feedsFeed. We need it to make the feed items (feeditem.id + feed.id) unique to enable progressive indexing (lucene)
 *
 * Revision 1.289  2006/08/13 17:01:18  t_rendelmann
 * further progress on lucene search (not yet finished)
 *
 * Revision 1.288  2006/08/08 14:24:45  t_rendelmann
 * fixed: nullref. exception on "Move to next unread" (if it turns back to treeview top node)
 * fixed: nullref. exception (assertion) on delete feeds/category node
 * changed: refactored usage of node.Tag (object type) to use node.DataKey (string type)
 *
 */
#endregion
