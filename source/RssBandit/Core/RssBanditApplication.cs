#region Version Info Header

/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

//#undef USEAUTOUPDATE
#define USEAUTOUPDATE

#define TEST_PERSISTED_FEEDSOURCES
//#define TEST_NEWSGATOR_ONLINE

// Uncomment the next line to enable specific UI lang. tests.
// Then modify the returned culture ISO code within I18NTestCulture struct.
// Alternativly you can also add a commandline param '-culture:"ru-RU" to
// the project properties...

//#define TEST_I18N_THISCULTURE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq; 
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using RssBandit.WinGui.Controls.ThListView;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using AppInteropServices;
using log4net;
using Microsoft.Win32;
using NewsComponents;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.News;
using NewsComponents.Search;
using NewsComponents.Storage;
using NewsComponents.Utils;
using RssBandit.AppServices;
using RssBandit.Common;
using RssBandit.Common.Logging;
using RssBandit.Exceptions;
using RssBandit.Resources;
using RssBandit.SpecialFeeds;
using RssBandit.UIServices;
using RssBandit.Utility;
using RssBandit.WebSearch;
using RssBandit.WinGui;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
using Logger = RssBandit.Common.Logging;
using SortOrder=NewsComponents.SortOrder;
using Timer=System.Threading.Timer;
using System.Windows.Threading;
using System.Windows.Forms.Integration;
using System.Configuration;

#if DEBUG && TEST_I18N_THISCULTURE			
internal struct I18NTestCulture {  public string Culture { get { return  "ru-RU"; } } };
#endif

namespace RssBandit
{
    /// <summary>
    /// Summary description for WinGuiMainMediator.
    /// </summary>
    internal partial class RssBanditApplication : ApplicationContext,
                                                  ICoreApplication, IInternetService, IServiceProvider
    {
        #region Fields

        /// <summary>
        /// additional string appended to Assembly version info
        /// </summary>
        /// <remarks>Next Final Release: remove the temp. preferences file 
        /// reading/writing before publishing!</remarks>
        public static readonly string versionPostfix = "(Phoenix - Alpha release)"; // String.Empty; // e.g. 'beta 1' or '(CVS)'

        private static bool validationErrorOccured;
        private static readonly RssBanditPreferences defaultPrefs = new RssBanditPreferences();
        private static Settings guiSettings;
        private static readonly ServiceContainer Services = new ServiceContainer( /* no other parent, we are at top */);

        internal const string DefaultPodcastFileExts = "mp3;mov;mp4;aac;aa;m4a;m4b;wma;wmv";

        private CommandMediator cmdMediator;
        private FeedSourceManager sourceManager;
        [Obsolete("use FeedSources")]
		private FeedSource feedHandler;
        private FeedSource commentFeedsHandler;

        private WinGuiMain guiMain;
        private PostReplyForm postReplyForm;
        private SearchEngineHandler searchEngines;
        private ThreadResultManager threadResultManager;
        private IdentityNewsServerManager identityNewsServerManager;
        private IAddInManager addInManager;

        /// <summary>
        /// Manage the channel processors working on displaying items and feeds
        /// (before we render them in the detail pane)
        /// </summary>
        private static readonly NewsChannelServices displayingNewsChannel = new NewsChannelServices();

        /// <summary>
        /// used to share the current UI thread's UI culture on various threads
        /// </summary>
        private static CultureInfo sharedUICulture;

        /// <summary>
        /// used to share the current thread's culture on various threads
        /// </summary>
        private static CultureInfo sharedCulture;

        private static readonly FeedColumnLayout DefaultFeedColumnLayout =
            new FeedColumnLayout(new[] {"Title", "Flag", "Enclosure", "Date", "Subject"},
                                 new[] {250, 22, 22, 100, 120}, "Date", SortOrder.Descending,
                                 LayoutType.GlobalFeedLayout);

        private static readonly FeedColumnLayout DefaultCategoryColumnLayout =
            new FeedColumnLayout(new[] {"Title", "Subject", "Date", "FeedTitle"}, new[] {250, 120, 100, 100},
                                 "Date", SortOrder.Descending, LayoutType.GlobalCategoryLayout);

        private static readonly FeedColumnLayout DefaultSearchFolderColumnLayout =
            new FeedColumnLayout(new[] {"Title", "Subject", "Date", "FeedTitle"}, new[] {250, 120, 100, 100},
                                 "Date", SortOrder.Descending, LayoutType.SearchFolderLayout);

        private static readonly FeedColumnLayout DefaultSpecialFolderColumnLayout =
            new FeedColumnLayout(new[] {"Title", "Subject", "Date", "FeedTitle"}, new[] {250, 120, 100, 100},
                                 "Date", SortOrder.Descending, LayoutType.SpecialFeedsLayout);

        private static string defaultFeedColumnLayoutKey;
        private static string defaultCategoryColumnLayoutKey;
        private static string defaultSearchFolderColumnLayoutKey;
        private static string defaultSpecialFolderColumnLayoutKey;

        private Timer autoSaveTimer;
        private bool feedlistModified;
        private bool commentFeedlistModified;
        private bool trustedCertIssuesModified;
        private Dictionary<int, List<string>> modifiedFeeds;

        private Thread _dispatcherThread;
        private Dispatcher _dispatcher;

        //private INetState connectionState = INetState.Invalid;	// moved to GuiStateManager

        internal static readonly int MilliSecsMultiplier = 60*1000;

        // other options:
        // TODO: include it in options dialog 
        private readonly bool interceptUrlNavigation = true;

        // private feeds
        private LocalFeedsFeed watchedItemsFeed;
        private LocalFeedsFeed flaggedItemsFeed;
        private LocalFeedsFeed sentItemsFeed;
        private LocalFeedsFeed deletedItemsFeed;
        private LocalFeedsFeed unreadItemsFeed;

        private readonly CommandLineOptions commandLineOptions;
        private GuiStateManager stateManager;

        //private NewsFeed flagItemsFeed;
        //private ArrayList flagItemsList;

        private FinderSearchNodes findersSearchRoot;
        private NewsItemFormatter NewsItemFormatter;

        // as defined in the installer for Product ID
        private const string applicationGuid = "9DDCC9CA-DFCD-4BF3-B069-C9660BB28848";
        private const string applicationId = "RssBandit";
        private const string applicationName = "RSS Bandit";

        private static readonly string[] applicationUpdateServiceUrls =
            new[] {"http://www.rssbandit.org/services/UpdateService.asmx"};

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

		private static SearchIndexBehavior searchIndexBehavior;

        private static Version appVersion;
        private static string appDataFolderPath;
        private static string appCacheFolderPath;
        private static readonly ILog _log = Log.GetLogger(typeof (RssBanditApplication));

        public event EventHandler PreferencesChanged;

		public event EventHandler AllFeedSourcesLoaded;
		public event EventHandler<FeedSourceEventArgs> FeedSourceLoaded;
		public event EventHandler AllFeedSourceSubscriptionsLoaded;
		public event EventHandler<FeedSourceEventArgs> FeedSourceSubscriptionsLoaded;
		public event EventHandler<FeedSourceFeedUrlTitleEventArgs> FeedSourceFeedDeleted;
        
		// old:
		public event EventHandler FeedlistLoaded;
        public event FeedDeletedHandler FeedDeleted;

        /// <summary>
        /// Async invoke on UI thread
        /// </summary>
        private readonly Action<Action> InvokeOnGui;

        /// <summary>
        /// Sync invoke on UI thread
        /// </summary>
        private readonly Action<Action> InvokeOnGuiSync;

        #endregion

        #region constructors and startup

        internal static void StaticInit()
        {
            // set initial defaults:
			validationUrlBase = SR.URL_FeedValidationBase;
			linkCosmosUrlBase = SR.URL_FeedLinkCosmosUrlBase;
			bugReportUrl = SR.URL_BugReport;
			webHelpUrl = SR.URL_WebHelp;
			workspaceNewsUrl = SR.URL_ProjectNews;
			wikiNewsUrl = SR.URL_WikiWebNews;
			forumUrl = SR.URL_UserForum;
			projectDonationUrl = SR.URL_ProjectDonation;
			projectDownloadUrl = SR.URL_ProjectDownload;

			// advanced settings:
			unconditionalCommentRss = false;
			automaticColorSchemes =  true;
			FeedSource.SetCookies = true;
			portableApplicationMode = false;
        	searchIndexBehavior = SearchIndexBehavior.Default;

            // read app.config If a key was not found, take defaults from the embedded resources
            List<ConfigurationErrorsException> configErrors = new List<ConfigurationErrorsException>();

			CollectConfigurationException(() => validationUrlBase = ReadAppSettingsEntry("validationUrlBase", SR.URL_FeedValidationBase), configErrors);
            CollectConfigurationException(() => linkCosmosUrlBase = ReadAppSettingsEntry("linkCosmosUrlBase", SR.URL_FeedLinkCosmosUrlBase), configErrors);
            CollectConfigurationException(() => bugReportUrl = ReadAppSettingsEntry("bugReportUrl", SR.URL_BugReport), configErrors);
            CollectConfigurationException(() => webHelpUrl = ReadAppSettingsEntry("webHelpUrl", SR.URL_WebHelp), configErrors);
            CollectConfigurationException(() => workspaceNewsUrl = ReadAppSettingsEntry("projectNewsUrl", SR.URL_ProjectNews), configErrors);
            CollectConfigurationException(() => wikiNewsUrl = ReadAppSettingsEntry("wikiWebUrl", SR.URL_WikiWebNews), configErrors);
            CollectConfigurationException(() => forumUrl = ReadAppSettingsEntry("userForumUrl", SR.URL_UserForum), configErrors);
            CollectConfigurationException(() => projectDonationUrl = ReadAppSettingsEntry("projectDonationUrl", SR.URL_ProjectDonation), configErrors);
            CollectConfigurationException(() => projectDownloadUrl = ReadAppSettingsEntry("projectDownloadUrl", SR.URL_ProjectDownload), configErrors);

            // read advanced settings:
            CollectConfigurationException(() => unconditionalCommentRss = ReadAppSettingsEntry("UnconditionalCommentRss", false), configErrors);
            CollectConfigurationException(() => automaticColorSchemes = ReadAppSettingsEntry("AutomaticColorSchemes", true), configErrors);
            CollectConfigurationException(() => FeedSource.SetCookies = ReadAppSettingsEntry("UseCookiesFromIE", true), configErrors);
			CollectConfigurationException(() => portableApplicationMode = ReadAppSettingsEntry("PortableApplicationMode", false), configErrors);
			CollectConfigurationException(() => searchIndexBehavior = ReadAppSettingsEntry("Lucene.SearchIndexBehavior",
				                            	SearchIndexBehavior.Default), configErrors);

			if (configErrors.Count > 0)
			{
				StringBuilder b = new StringBuilder();
				foreach (var ex in configErrors)
					b.AppendFormat("- {0}{1}", ex.Message, Environment.NewLine);

				b.AppendFormat("{0}{0}Default value(s) will be used.", Environment.NewLine);
				MessageBox.Show(b.ToString(), "Configuration error(s) detected", MessageBoxButtons.OK);	
			}

            // Gui Settings (Form position, layouts,...)
            guiSettings = new Settings(String.Empty);
        }

		private static void CollectConfigurationException(Action action, ICollection<ConfigurationErrorsException> exceptions)
		{
			ExceptionHelper.CatchAndCollectExceptions(action, exceptions);
		}

        public RssBanditApplication()
        {
            commandLineOptions = new CommandLineOptions();

            InvokeOnGuiSync = a => GuiInvoker.Invoke(guiMain, a);

            InvokeOnGui = a => GuiInvoker.InvokeAsync(guiMain, a);
        }

        /// <summary>
        /// Inits this instance. Returns false, if an issue happened and/or the user
        /// do not like to continue startup.
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            //specify 'nntp' and 'news' URI handler
            var creator = new NntpWebRequest(new Uri("http://www.example.com"));
            WebRequest.RegisterPrefix("nntp", creator);
            WebRequest.RegisterPrefix("news", creator);

            defaultCategory = SR.FeedDefaultCategory;

            // first: load user preferences (proxy, maxitemage, etc.)
            LoadPreferences();

            // fix/workaround for topstories feature:
            Preferences.BuildRelationCosmos = true;

            // may already use preference settings:
            FeedSource.DefaultConfiguration = CreateFeedHandlerConfiguration();

            // set static properties:
            FeedSource.EnclosureFolder = Preferences.EnclosureFolder;
            FeedSource.Stylesheet = Preferences.NewsItemStylesheetFile;
        	FeedSource.PodcastFolder = Preferences.PodcastFolder;
			FeedSource.PodcastFileExtensionsAsString = Preferences.PodcastFileExtensions;

            LoadTrustedCertificateIssues();
            AsyncWebRequest.OnCertificateIssue += OnRequestCertificateIssue;

            // init feed source manager and sources:
            sourceManager = new FeedSourceManager();

            try
            {
                // load feedsources from file/db and init
                sourceManager.LoadFeedSources(GetFeedSourcesFileName());
            }
            catch (Exception loadEx)
            {
                _log.Error(
                    String.Format("Failed to load feed sources from file:{0}: {1}.", GetFeedSourcesFileName(),
                                  loadEx.Message), loadEx);
                if (DialogResult.No ==
					MessageQuestion(String.Format("Failed to load feed sources from file:{0}{1}.{0}{0}Error was: {2}{0}{0}Continue?",
                                                  Environment.NewLine, GetFeedSourcesFileName(), loadEx.Message)))
                    return false;
            }

            //make sure we have a direct access feed source
			//DISCUSS: should we do so in the final release?
			if (!sourceManager.Contains(SR.FeedNodeMyFeedsCaption))
			{
				sourceManager.Add(SR.FeedNodeMyFeedsCaption, FeedSourceType.DirectAccess, null);
				// save the test enviroment:
				sourceManager.SaveFeedSources(GetFeedSourcesFileName());
			}

			//TODO: remove, if all references got replaced:
        	feedHandler = sourceManager[SR.FeedNodeMyFeedsCaption].Source;
            
            commentFeedsHandler = FeedSource.CreateFeedSource(FeedSourceType.DirectAccess,
                                                              new SubscriptionLocation(GetCommentsFeedListFileName()),
                                                              CreateCommentFeedHandlerConfiguration(
                                                                  FeedSource.DefaultConfiguration));
            // not really needed here, but init:
            commentFeedsHandler.OnAllAsyncRequestsCompleted += OnAllCommentFeedRequestsCompleted;
            commentFeedsHandler.OnUpdatedFeed += OnUpdatedCommentFeed;

            FeedSource.UnconditionalCommentRss = UnconditionalCommentRss;
#if DEBUG
            FeedSource.TraceMode = true;
#endif

            searchEngines = new SearchEngineHandler();
            identityNewsServerManager = new IdentityNewsServerManager(this);
            addInManager = new ServiceManager();

            // Gui command handling
            cmdMediator = new CommandMediator();

            // Gui State handling (switch buttons, icons, etc.)
            stateManager = new GuiStateManager();

            stateManager.InternetConnectionStateMoved += OnInternetConnectionStateChanged;
            stateManager.NewsHandlerBeforeStateMove += OnRssParserBeforeStateChange;
            stateManager.NewsHandlerStateMoved += OnNewsHandlerStateChanged;


            NewsItemFormatter = new NewsItemFormatter();
            NewsItemFormatter.TransformError += OnNewsItemTransformationError;
            NewsItemFormatter.StylesheetError += OnNewsItemFormatterStylesheetError;
            NewsItemFormatter.StylesheetValidationError += OnNewsItemFormatterStylesheetValidationError;

            // init all common components with the current preferences. 
            // also apply some settings to each feed source instance, so they have
            // have to be loaded before calling:
            ApplyPreferences();

            InitLocalFeeds();

            BackgroundDiscoverFeedsHandler = new AutoDiscoveredFeedsMenuHandler(this);
            BackgroundDiscoverFeedsHandler.DiscoveredFeedsSubscribe += OnBackgroundDiscoveredFeedsSubscribe;
			BackgroundDiscoverFeedsHandler.NewFeedsDiscovered += OnBackgroundNewFeedsDiscovered;


            modifiedFeeds = new Dictionary<int,List<string>>();

            // Create a timer that waits three minutes , then invokes every five minutes.
            autoSaveTimer = new Timer(OnAutoSave, this, 3*MilliSecsMultiplier, 5*MilliSecsMultiplier);
            // handle/listen to power save modes
            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            // App Update Management
            RssBanditUpdateManager.OnUpdateAvailable += OnApplicationUpdateAvailable;

            InitApplicationServices();

            // register build in channel processors:
            CoreServices.RegisterDisplayingNewsChannelProcessor(new DisplayingNewsChannelProcessor());

            // indicates OK:
            return true;
        }

        private void InitLocalFeeds()
        {
			FeedSourceEntry migrationEntry = BanditFeedSourceEntry;

			flaggedItemsFeed = new FlaggedItemsFeed(migrationEntry);
			watchedItemsFeed = new WatchedItemsFeed(migrationEntry);
			sentItemsFeed = new SentItemsFeed(migrationEntry);
			deletedItemsFeed = new DeletedItemsFeed(migrationEntry);
			unreadItemsFeed = new UnreadItemsFeed(migrationEntry);

            findersSearchRoot = LoadSearchFolders();
			
			// flush possible migration changes:
			if (flaggedItemsFeed.Modified)
				flaggedItemsFeed.Save();
			if (watchedItemsFeed.Modified)
				watchedItemsFeed.Save();
			if (sentItemsFeed.Modified)
				sentItemsFeed.Save();
			if (deletedItemsFeed.Modified)
				deletedItemsFeed.Save();
        }

		
        
		private INewsComponentsConfiguration CreateFeedHandlerConfiguration()
        {
            var cfg = new NewsComponentsConfiguration
                          {
                              ApplicationID = Name,
                              ApplicationVersion = Version
                          };
            try
            {
            	cfg.SearchIndexBehavior = SearchIndexBehavior;
                    
            }
            catch (Exception configException)
            {
                _log.Error("Invalid Value for SearchIndexBehavior in app.config", configException);
                cfg.SearchIndexBehavior = SearchIndexBehavior.Default;
            }
            cfg.UserApplicationDataPath = ApplicationDataFolderFromEnv;
            cfg.UserLocalApplicationDataPath = ApplicationLocalDataFolderFromEnv;

            if (String.IsNullOrEmpty(Preferences.EnclosureFolder))
                cfg.DownloadedFilesDataPath = Preferences.EnclosureFolder;
            else
                cfg.DownloadedFilesDataPath = GetDefaultEnclosuresPath();

            cfg.CacheManager = new FileCacheManager(GetFeedFileCachePath());
            cfg.PersistedSettings = GuiSettings;

            // once written a valid value:
            if (Preferences.RefreshRate >= 0)
                cfg.RefreshRate = Preferences.RefreshRate;

            return cfg;
        }

        private static INewsComponentsConfiguration CreateCommentFeedHandlerConfiguration(
            INewsComponentsConfiguration configTemplate)
        {
            var cfg = new NewsComponentsConfiguration
                          {
                              ApplicationID = configTemplate.ApplicationID,
                              ApplicationVersion = configTemplate.ApplicationVersion,
                              SearchIndexBehavior = SearchIndexBehavior.NoIndexing,
                              UserApplicationDataPath = configTemplate.UserApplicationDataPath,
                              UserLocalApplicationDataPath = configTemplate.UserLocalApplicationDataPath,
                              DownloadedFilesDataPath = null,
                              CacheManager = new FileCacheManager(GetFeedFileCachePath()),
                              PersistedSettings = configTemplate.PersistedSettings,
                              RefreshRate = configTemplate.RefreshRate
                          };
            return cfg;
        }

        #region IServiceProvider Members and Init

        private void InitApplicationServices()
        {
            IServiceContainer top = Services;
            // allow other services to add themself to the chain:
            top.AddService(typeof (IServiceContainer), top);
            // our default topmost services:
            top.AddService(typeof (IInternetService), this);
            top.AddService(typeof (IUserPreferences), Preferences);
            top.AddService(typeof (ICoreApplication), this);
            top.AddService(typeof (IAddInManager), addInManager);
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
        public object GetService(Type serviceType)
        {
            object o = Services.GetService(serviceType);
            if (o != null) return o;

            if (serviceType == typeof (IServiceContainer))
                return Services;

            return null;
        }

        #endregion

        /// <summary>
        /// Startup the Main GUI Interface.
        /// </summary>
        public void StartMainGui(FormWindowState initialStartupState)
        {
#if !DEBUG
    // Allow exceptions to be unhandled so they break in the debugger
            ApplicationExceptionHandler eh = new ApplicationExceptionHandler();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(eh.OnAppDomainException);
#endif
            //AppDomain.CurrentDomain.DomainUnload += OnAppDomainUnload;
            //AppDomain.CurrentDomain.ProcessExit += OnAppDomainUnload;

            Application.ApplicationExit += OnApplicationExit;
            // Allow exceptions to be unhandled so they break in the debugger
#if !DEBUG
			Application.ThreadException += this.OnThreadException;
#endif
            Splash.Status = SR.AppLoadStateGuiLoading;

            _dispatcherThread = new Thread(DispatcherThread);
            _dispatcherThread.TrySetApartmentState(ApartmentState.STA);
            _dispatcherThread.Name = "Dispatcher Thread";
            _dispatcherThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            _dispatcherThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            _dispatcherThread.Start();

            MainForm = guiMain = new WinGuiMain(this, initialStartupState); // interconnect

            GuiInvoker.Initialize();
			
			// now we are ready to receive events from the backend:
			sourceManager.ForEach(delegate(FeedSource f)
			                      {
			                      	ConnectFeedSourceEvents(f);
			                      });

            // thread results to UI serialization/sync.:
            threadResultManager = new ThreadResultManager(this, guiMain.ResultDispatcher);
            ThreadWorkerBase.SynchronizingObject = guiMain;

#if !DEBUG
            enter_mainevent_loop:
            try
            {
                Application.Run(this);
            }
            catch (Exception thirdPartyComponentsExceptions)
            {
                Splash.Close(); // if occured on load
                if (DialogResult.Retry ==
                    PublishException(
                        new BanditApplicationException("StartMainGui() exiting main event loop on exception.",
                                                       thirdPartyComponentsExceptions)))
                    goto enter_mainevent_loop;
                Application.Exit();
            }
#else
            Application.Run(this);
#endif

        }

        private void DispatcherThread()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
            DownloadRegistryManager.Current.Initialize();

            // create this here to pre-load the WPF libraries
            var dm = new DownloadManagerWindow();
            dm.Close();

            Dispatcher.Run();
        }

        internal void CheckAndLoadAddIns()
        {
            IEnumerable<IAddIn> addIns = addInManager.AddIns;

            if (addIns == null)
                return;
            foreach (var addIn in addIns)
            {
                if (addIn.AddInPackages == null || addIn.AddInPackages.Count == 0)
                    continue;
                foreach (var package in addIn.AddInPackages)
                {
                    try
                    {
                        package.Load(this);
                    }
                    catch (Exception ex)
                    {
                        string error = String.Format(SR.AddInGeneralFailure, ex.Message, addIn.Name);
                        _log.Fatal(
                            "Failed to load IAddInPackage from AddIn: " + addIn.Name + " from '" + addIn.Location + "'",
                            ex);
                        MessageError(error);
                    }
                }
            }
        }

        /// <summary>
        /// Cleanup task for addins
        /// </summary>
        internal void UnloadAddIns()
        {
            IEnumerable<IAddIn> addIns = addInManager.AddIns;
            if (addIns == null)
                return;
            foreach (var addIn in addIns)
            {
                foreach (var package in addIn.AddInPackages)
                {
                    try
                    {
                        package.Unload();
                    }
                    catch (Exception ex)
                    {
                        string error = String.Format(SR.AddInUnloadFailure, ex.Message, addIn.Name);
                        _log.Fatal(
                            "Failed to unload IAddInPackage from AddIn: " + addIn.Name + " from '" + addIn.Location +
                            "'", ex);
                        MessageError(error);
                    }
                }
            }
        }

        #endregion

        #region ICommandComponent related routines

        public CommandMediator Mediator
        {
            [DebuggerStepThrough]
            get { return cmdMediator; }
        }

        #endregion

        #region Task management

        private ThreadWorkerTask MakeTask(ThreadWorker.Task task, ThreadWorkerProgressHandler handler,
                                          params object[] args)
        {
            return new ThreadWorkerTask(task, handler, this, args);
        }

        internal void MakeAndQueueTask(ThreadWorker.Task task, ThreadWorkerProgressHandler handler, params object[] args)
        {
            QueueTask(MakeTask(task, handler, args));
        }

        internal void MakeAndQueueTask(ThreadWorker.Task task, ThreadWorkerProgressHandler handler,
                                       ThreadWorkerBase.DuplicateTaskQueued duplicate, params object[] args)
        {
            QueueTask(MakeTask(task, handler, args), duplicate);
        }

        private static void QueueTask(ThreadWorkerTaskBase task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            ThreadWorkerBase.QueueTask(task);
        }

        private static void QueueTask(ThreadWorkerTaskBase task, ThreadWorkerBase.DuplicateTaskQueued duplicate)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            ThreadWorkerBase.QueueTask(task, duplicate);
        }

		//public void BeginLoadingFeedlists()
		//{
		//    MakeAndQueueTask(ThreadWorker.Task.LoadFeedlists, OnLoadingFeedlistProgress);
		//}

		/// <summary>
		/// Begins the load of feed source subscriptions for one source in async. manner.
		/// </summary>
		/// <param name="sourceEntry">The source entry.</param>
		public void BeginLoadFeedSourceSubscriptions(FeedSourceEntry sourceEntry)
		{
			MakeAndQueueTask(ThreadWorker.Task.LoadFeedSourceSubscriptions,
							 (sender, args) =>
							 {
							 	 if (args.Exception != null)
								 {
									 // failure(s)
									 args.Cancel = true;
									 PublishException(args.Exception);
								 }
								 else if (!args.Done)
								 {
									 // in progress
								 }
								 else if (args.Done)
								 {
									 guiMain.PopulateFeedSubscriptions(sourceEntry, DefaultCategory);
								 }
							 }, sourceEntry);
		}

		/// <summary>
		/// Begins the load of all feed source subscriptions in async. manner.
		/// </summary>
		public void BeginLoadAllFeedSourcesSubscriptions()
		{
			MakeAndQueueTask(
				ThreadWorker.Task.LoadAllFeedSourcesSubscriptions,
				(sender, args) =>
				{
					FeedSourceEntry current = (FeedSourceEntry)args.Result;
						
					if (args.Exception != null)
					{
						// failure(s)
						PublishException(args.Exception);
						// always call (callee can detect an error using current.Source.FeedListOK):
						RaiseFeedSourceSubscriptionsLoaded(current);
					
					}
					else if (!args.Done)
					{
						// notify current progress:
						RaiseFeedSourceSubscriptionsLoaded(current);
						// in progress counters:
						//TODO: we should have our own ApplicationTrayState  (not Idle) !
						guiMain.SetGuiStateFeedback(String.Format(SR.GUIStatusXofYFeedSourceSubscriptionsLoaded, args.CurrentProgress, args.MaxProgress), ApplicationTrayState.NormalIdle);
					}
					else if (args.Done)
					{
						guiMain.SetGuiStateFeedback(SR.GUIStatusAllSubscriptionListsLoaded, ApplicationTrayState.NormalIdle);
						RaiseAllFeedSourcesSubscriptionsLoaded();
					}

				}, FeedSources.GetOrderedFeedSources());
		}

        public void BeginRefreshFeeds(bool forceDownload)
        {
            //if (this.InternetAccessAllowed) {
            // handled via NewsHander.Offline flag
            StateHandler.MoveNewsHandlerStateTo(forceDownload
                                                    ? FeedSourceBusyState.RefreshAllForced
                                                    : FeedSourceBusyState.RefreshAllAuto);
            MakeAndQueueTask(ThreadWorker.Task.RefreshFeeds, OnRefreshFeedsProgress, forceDownload);
            //}
        }

        public void BeginRefreshCommentFeeds(bool forceDownload)
        {
            //if (this.InternetAccessAllowed) {
            // handled via NewsHander.Offline flag
            MakeAndQueueTask(ThreadWorker.Task.RefreshCommentFeeds, OnRefreshFeedsProgress, forceDownload);
            //}
        }

        public void BeginRefreshCategoryFeeds(FeedSourceEntry entry, string category, bool forceDownload)
        {
            //if (this.InternetAccessAllowed) {
            // handled via NewsHander.Offline flag
            StateHandler.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshCategory);
            MakeAndQueueTask(ThreadWorker.Task.RefreshCategoryFeeds, OnRefreshFeedsProgress, entry, category,
                             forceDownload);
            //}
        }

        private static void OnRefreshFeedsProgress(object sender, ThreadWorkerProgressArgs args)
        {
            if (args.Exception != null)
            {
                // failure(s)
                args.Cancel = true;
                PublishException(args.Exception);
            }
            else if (!args.Done)
            {
                // handled via separate events
            }
            else if (args.Done)
            {
                // done
                // handled via separate events
            }
        }

        #endregion


        public FeedSourceEntry CurrentFeedSource
        {
            get
            {
                return guiMain.CurrentSelectedFeedSource;              
            }
        }

        public FeedSourceManager FeedSources
        {
            get { return sourceManager; }
        }

        public FeedSource BanditFeedSource
        {
			get { return BanditFeedSourceEntry.Source; }
        }

		/// <summary>
		/// Gets the migration feed source entry. (BanditFeedSource: used to read/get/migrate old 1.6.x settings)
		/// </summary>
		/// <returns></returns>
		public FeedSourceEntry BanditFeedSourceEntry
		{
			get
			{
				return FeedSources.Sources.FirstOrDefault(
					entry =>
					{
						return (entry.SourceType == FeedSourceType.DirectAccess);
					});
			}
		}

		[Obsolete("Please call FeedSources property")]
        public FeedSource FeedHandler
        {
            get { return feedHandler; }        }

        public FeedSource CommentFeedsHandler
        {
            get { return commentFeedsHandler; }
        }


        public RssBanditPreferences Preferences { get; set; }

        public IdentityNewsServerManager IdentityManager
        {
            get { return identityNewsServerManager; }
        }

        public IdentityNewsServerManager NntpServerManager
        {
            get { return identityNewsServerManager; }
        }

		/// <summary>
		/// Notification method about a setting that was modified,
		/// relevant to the subscriptions.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="property">The property.</param>
		public void SubscriptionModified(FeedSourceEntry entry, NewsFeedProperty property)
		{
			HandleSubscriptionRelevantChange(property);
		}

		[Obsolete("call SubscriptionModified(FeedSourceEntry, NewsFeedProperty)")]
		public void SubscriptionModified(NewsFeedProperty property)
        {
            HandleSubscriptionRelevantChange(property);
        }

        /// <summary>
        /// Notification method about a feed that was modified.
        /// </summary>
        /// <param name="feed">The feed.</param>
        /// <param name="property">The property.</param>
        public void FeedWasModified(INewsFeed feed, NewsFeedProperty property)
        {
            HandleSubscriptionRelevantChange(property);

            if (feed == null)
                return;

            HandleFeedCacheRelevantChange(sourceManager.SourceOf(feed.owner as FeedSource), feed.link, property);
            HandleIndexRelevantChange(feed, property);
        }

        /// <summary>
        /// Notification method about a feed that was modified.
        /// </summary>
        /// <param name="feedUrl">The feed URL.</param>
        /// <param name="property">The property.</param>
        public void FeedWasModified(FeedSourceEntry entry, string feedUrl, NewsFeedProperty property)
        {
            HandleSubscriptionRelevantChange(property);

            if (string.IsNullOrEmpty(feedUrl))
                return;

            HandleFeedCacheRelevantChange(entry, feedUrl, property);
            HandleIndexRelevantChange(feedUrl, property);
        }

        private void HandleSubscriptionRelevantChange(NewsFeedProperty property)
        {
			//TODO: move save state handling to FeedSourceManager
            if (FeedSource.IsSubscriptionRelevantChange(property))
                feedlistModified = true;
        }

        private void HandleFeedCacheRelevantChange(FeedSourceEntry entry, string feedUrl, NewsFeedProperty property)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return;
            if (FeedSource.IsCacheRelevantChange(property))
            {
                // we queue up the cache file refresh requests:
                lock (modifiedFeeds)
                {
                    if (modifiedFeeds.ContainsKey(entry.ID))
                    {
                        if (!modifiedFeeds[entry.ID].Contains(feedUrl))
                        {
                            modifiedFeeds[entry.ID].Add(feedUrl); 
                        }
                    }
                    else
                    {
                        var modified_list = new List<string> { feedUrl }; 
                        modifiedFeeds.Add(entry.ID, modified_list);
                    }
                }
            }
        }

        private void HandleIndexRelevantChange(string feedUrl, NewsFeedProperty property)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return;
            if (FeedSource.SearchHandler.IsIndexRelevantChange(property))
                HandleIndexRelevantChange(GetFeed(feedUrl), property);
        }

        private void HandleIndexRelevantChange(INewsFeed feed, NewsFeedProperty property)
        {
            if (feed == null)
                return;
            if (FeedSource.SearchHandler.IsIndexRelevantChange(property))
            {
                if (NewsFeedProperty.FeedRemoved == (property & NewsFeedProperty.FeedRemoved))
                {
                    FeedSource.SearchHandler.IndexRemove(feed.id);
                    // feed added change is handled after first sucessful request of the feed:
                }
                else if (NewsFeedProperty.FeedAdded == (property & NewsFeedProperty.FeedAdded))
                {
                    FeedSource source = feed.owner as FeedSource;
                    if (source != null)
                    {
                        FeedSource.SearchHandler.ReIndex(feed, source.GetCachedItemsForFeed(feed.link));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the NewsFeed from FeedHandler.
        /// </summary>
        /// <param name="feedUrl">The feed URL (can be null).</param>
        /// <returns>NewsFeed if found, else null</returns>
		[Obsolete("Please call GetFeed(FeedSourceEntry, string)")]
		public INewsFeed GetFeed(string feedUrl)
        {
			if (string.IsNullOrEmpty(feedUrl))
                return null;
            if (feedHandler.IsSubscribed(feedUrl))
                return feedHandler.GetFeeds()[feedUrl];
            return null;
        }

		/// <summary>
		/// Gets the NewsFeed from FeedHandler.
		/// </summary>
		/// <param name="entry">The feed source entry.</param>
		/// <param name="feedUrl">The feed URL (can be null).</param>
		/// <returns>NewsFeed if found, else null</returns>
		public INewsFeed GetFeed(FeedSourceEntry entry, string feedUrl)
		{
			if (entry == null || string.IsNullOrEmpty(feedUrl))
				return null;
			if (entry.Source.IsSubscribed(feedUrl))
				return entry.Source.GetFeeds()[feedUrl];
			return null;
		}

        /// <summary>
        /// Gets the feed info (<see cref="IFeedDetails"/>).
        /// </summary>
        /// <param name="entry">The feed source entry.</param>
        /// <param name="feedUrl">The feed URL (can be null).</param>
        /// <returns>IFeedDetails if found, else null</returns>
		public IFeedDetails GetFeedDetails(FeedSourceEntry entry, string feedUrl)
        {
			if (entry == null || string.IsNullOrEmpty(feedUrl))
                return null;
			if (entry.Source.IsSubscribed(feedUrl))
				return entry.Source.GetFeedDetails(feedUrl);
            return null;
        }

        public SearchEngineHandler SearchEngineHandler
        {
            get { return searchEngines; }
        }

        public Settings GuiSettings
        {
            get { return guiSettings; }
        }

		public static IPersistedSettings PersistedSettings
		{
			get { return guiSettings; }
		}

        /// <summary>
        /// Gets or sets the last auto update check date.
        /// </summary>
        /// <value>Date of the last auto update check.</value>
        public DateTime LastAutoUpdateCheck
        {
            get
            {
                return
                    (DateTime)
                    GuiSettings.GetProperty("Application.LastAutoUpdateCheck", DateTime.MinValue, typeof (DateTime));
            }
            set
            {
                GuiSettings.SetProperty("Application.LastAutoUpdateCheck", value);
                GuiSettings.Flush();
            }
//			get { return this.Preferences.LastAutoUpdateCheck; }
//			set { this.Preferences.LastAutoUpdateCheck = value; SavePreferences(); }
        }

        public GuiStateManager StateHandler
        {
            get { return stateManager; }
        }

        public string Stylesheet
        {
            get { return Preferences.NewsItemStylesheetFile; }
        }

        public IWebProxy Proxy
        {
            get { return FeedSource.GlobalProxy; }
            set
            {
                if (value == null)
                    FeedSource.UseDefaultProxy();
                else
                    FeedSource.GlobalProxy = value;
            }
        }

        public void ApplyMaxItemAge(TimeSpan value)
        {
            sourceManager.ForEach(
                delegate(FeedSource fs) { fs.MaxItemAge = value; });
        }

        public void ApplyRefreshRate(int value)
        {
            // not yet a valid setting, keep the defaults:
            if (value < 0)
                return;

            // apply setting to feed sources:
            sourceManager.ForEach(
                fs =>
                    {
                        var cfg = (NewsComponentsConfiguration) fs.Configuration;
                        cfg.RefreshRate = value;
                    });

            // apply setting to comment feed handler:
            var c = (NewsComponentsConfiguration) commentFeedsHandler.Configuration;
            c.RefreshRate = value;
        }

        public void ApplyDownloadEnclosures(bool value)
        {
            // apply setting to feed sources:
            sourceManager.ForEach(
                fs =>
                    {
                        var cfg = (NewsComponentsConfiguration) fs.Configuration;
                        cfg.DownloadEnclosures = value;
                    });

            // apply setting to comment feed handler:
            var c = (NewsComponentsConfiguration) commentFeedsHandler.Configuration;
            c.DownloadEnclosures = value;
        }

        public ArrayList FinderList
        {
            get { return findersSearchRoot.RssFinderNodes; }
            set { findersSearchRoot.RssFinderNodes = value; }
        }

        #region IInternetService Members

        public event InternetConnectionStateChangeHandler InternetConnectionStateChange;

        public bool InternetAccessAllowed
        {
            get { return stateManager.InternetAccessAllowed; }
        }

        public bool InternetConnectionOffline
        {
            get { return stateManager.InternetConnectionOffline; }
        }

        public INetState InternetConnectionState
        {
            get { return stateManager.InternetConnectionState; }
        }

        #endregion

        /// <summary>
        /// Property CommentFeedlistModified (bool)
        /// </summary>
        public bool CommentFeedlistModified
        {
            get { return commentFeedlistModified; }
            set { commentFeedlistModified = value; }
        }

        public AutoDiscoveredFeedsMenuHandler BackgroundDiscoverFeedsHandler { get; private set; }

		private void OnBackgroundNewFeedsDiscovered(object sender, DiscoveredFeedsInfoEventArgs e)
		{
			// currently we just play a sound.
			// But we could also display a tooltip at the command button
			// to indicate new feeds detected here
			Win32.PlaySound(Resource.ApplicationSound.FeedDiscovered);
		}

    	private void OnBackgroundDiscoveredFeedsSubscribe(object sender, DiscoveredFeedsInfoCancelEventArgs e)
        {
            if (e.FeedsInfo.FeedLinks.Count == 1)
            {
                e.Cancel = !CmdNewFeed(DefaultCategory, e.FeedsInfo.FeedLinks[0], e.FeedsInfo.Title);
            }
            else if (e.FeedsInfo.FeedLinks.Count > 1)
            {
                var feedUrls = new Hashtable(e.FeedsInfo.FeedLinks.Count);
                foreach (string feedUrl in e.FeedsInfo.FeedLinks)
                {
                    feedUrls.Add(feedUrl,
                                 new[] {e.FeedsInfo.Title, String.Empty, e.FeedsInfo.SiteBaseUrl, feedUrl});
                }

                var discoveredFeedsDialog = new DiscoveredFeedsDialog(feedUrls);
                discoveredFeedsDialog.ShowDialog(guiMain);

                if (discoveredFeedsDialog.DialogResult == DialogResult.OK)
                {
                    e.Cancel = true;
                    foreach (ListViewItem feedItem in discoveredFeedsDialog.listFeeds.SelectedItems)
                    {
                        if (CmdNewFeed(defaultCategory, (string) feedItem.Tag, feedItem.SubItems[0].Text) &&
                            e.Cancel)
                            e.Cancel = false; // at least one dialog succeeds
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void CheckAndMigrateListViewLayouts()
        {
            // check, if any migration task have to be applied:

            // v.1.3.x beta to 1.3.x.release:
            // layouts was serialized directly to feed/category elements
            // now they live in a separate collection

            // we assume we should have always at least the default layouts:
            if (this.BanditFeedSource.ColumnLayouts.Count == 0)
            {
                ListViewLayout oldLayout;
                foreach (var f in this.BanditFeedSource.GetFeeds().Values)
                {
                    if (string.IsNullOrEmpty(f.listviewlayout) || f.listviewlayout.IndexOf("<") < 0)
                        continue;
                    try
                    {
                        oldLayout = ListViewLayout.CreateFromXML(f.listviewlayout);
                        var fc = new FeedColumnLayout(oldLayout.ColumnList,
                                                      oldLayout.ColumnWidthList, oldLayout.SortByColumn,
                                                      oldLayout.SortOrder, LayoutType.IndividualLayout);

                        if (!fc.Equals(DefaultFeedColumnLayout, true))
                        {
                            int found = this.BanditFeedSource.ColumnLayouts.IndexOfSimilar(fc);
                            if (found >= 0)
                            {
                                // assign key
                                f.listviewlayout = this.BanditFeedSource.ColumnLayouts.GetKey(found);
                            }
                            else
                            {
                                // add and assign key
                                f.listviewlayout = Guid.NewGuid().ToString("N");
                                this.BanditFeedSource.ColumnLayouts.Add(f.listviewlayout, fc);
                            }
                        }
                        else
                        {
                            f.listviewlayout = null; // same as default: reset
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex); /* ignore deserialization failures */
                    }
                }

                foreach (var c in this.BanditFeedSource.GetCategories().Values)
                {
                    if (string.IsNullOrEmpty(c.listviewlayout) || c.listviewlayout.IndexOf("<") < 0)
                        continue;
                    try
                    {
                        oldLayout = ListViewLayout.CreateFromXML(c.listviewlayout);
                        var fc = new FeedColumnLayout(oldLayout.ColumnList,
                                                      oldLayout.ColumnWidthList, oldLayout.SortByColumn,
                                                      oldLayout.SortOrder, LayoutType.IndividualLayout);

                        if (!fc.Equals(DefaultCategoryColumnLayout, true))
                        {
                            int found = this.BanditFeedSource.ColumnLayouts.IndexOfSimilar(fc);
                            if (found >= 0)
                            {
                                // assign key
                                c.listviewlayout = this.BanditFeedSource.ColumnLayouts.GetKey(found);
                            }
                            else
                            {
                                // add and assign key
                                c.listviewlayout = Guid.NewGuid().ToString("N");
                                this.BanditFeedSource.ColumnLayouts.Add(c.listviewlayout, fc);
                            }
                        }
                        else
                        {
                            c.listviewlayout = null; // same as default: reset
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex); /* ignore deserialization failures */
                    }
                }

                // now add the default layouts
                this.BanditFeedSource.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultFeedColumnLayout);
                this.BanditFeedSource.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultCategoryColumnLayout);
                this.BanditFeedSource.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultSearchFolderColumnLayout);
                this.BanditFeedSource.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultSpecialFolderColumnLayout);

                if (!string.IsNullOrEmpty(this.BanditFeedSource.FeedColumnLayout))
                    try
                    {
                        oldLayout = ListViewLayout.CreateFromXML(this.BanditFeedSource.FeedColumnLayout);
                        var fc = new FeedColumnLayout(oldLayout.ColumnList,
                                                      oldLayout.ColumnWidthList, oldLayout.SortByColumn,
                                                      oldLayout.SortOrder, LayoutType.IndividualLayout);

                        if (!fc.Equals(DefaultCategoryColumnLayout, true))
                        {
                            int found = this.BanditFeedSource.ColumnLayouts.IndexOfSimilar(fc);
                            if (found >= 0)
                            {
                                // assign key
                                this.BanditFeedSource.FeedColumnLayout = this.BanditFeedSource.ColumnLayouts.GetKey(found);
                            }
                            else
                            {
                                // add and assign key
                                this.BanditFeedSource.FeedColumnLayout = Guid.NewGuid().ToString("N");
                                this.BanditFeedSource.ColumnLayouts.Add(feedHandler.FeedColumnLayout, fc);
                            }
                        }
                        else
                        {
                            this.BanditFeedSource.FeedColumnLayout = defaultCategoryColumnLayoutKey; // same as default
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex); /* ignore deserialization failures */
                    }

                feedlistModified = true; // trigger auto-save
            }
        }

        #region class kept for migration purpose:

        [Serializable]
        public class ListViewLayout : ICloneable
        {
            private string _sortByColumn;
            private SortOrder _sortOrder;
            internal List<string> _columns;
            internal List<int> _columnWidths;

            public ListViewLayout() : this(null, null, null, SortOrder.None)
            {
            }

            public ListViewLayout(IEnumerable<string> columns, IEnumerable<int> columnWidths, string sortByColumn,
                                  SortOrder sortOrder)
            {
                _columns = columns != null ? new List<string>(columns) : new List<string>();
                _columnWidths = columnWidths != null ? new List<int>(columnWidths) : new List<int>();
                _sortByColumn = sortByColumn;
                _sortOrder = sortOrder;
            }

            public static ListViewLayout CreateFromXML(string xmlString)
            {
                if (!string.IsNullOrEmpty(xmlString))
                {
                    XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof (ListViewLayout));
                    var reader = new StringReader(xmlString);
                    return (ListViewLayout) formatter.Deserialize(reader);
                }
                return null;
            }

            public static string SaveAsXML(ListViewLayout layout)
            {
                if (layout == null)
                    return null;
                try
                {
                    XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof (ListViewLayout));
                    var writer = new StringWriter();
                    formatter.Serialize(writer, layout);
                    return writer.ToString();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("SaveAsXML() failed.", ex.Message);
                }
                return null;
            }

            #region IListViewLayout Members

            public string SortByColumn
            {
                get { return _sortByColumn; }
                set { _sortByColumn = value; }
            }

            public SortOrder SortOrder
            {
                get { return _sortOrder; }
                set { _sortOrder = value; }
            }

            [XmlIgnore]
            public IList<string> Columns
            {
                get { return _columns; }
                set { _columns = value != null ? new List<string>(value) : new List<string>(); }
            }

            [XmlIgnore]
            public IList<int> ColumnWidths
            {
                get { return _columnWidths; }
                set { _columnWidths = value != null ? new List<int>(value) : new List<int>(); }
            }

            [XmlIgnore]
            public bool Modified { get; set; }

            #endregion

            [XmlArrayItem(typeof (string))]
            public List<string> ColumnList
            {
                get { return _columns; }
                set { _columns = value ?? new List<string>(); }
            }

            [XmlArrayItem(typeof (int))]
            public List<int> ColumnWidthList
            {
                get { return _columnWidths; }
                set { _columnWidths = value ?? new List<int>(); }
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                var o = obj as ListViewLayout;
                if (o == null)
                    return false;
                if (SortOrder != o.SortOrder)
                    return false;
                if (SortByColumn != o.SortByColumn)
                    return false;
                if (_columns == null && o._columns == null)
                    return true;
                if (_columns == null || o._columns == null)
                    return false;
                if (_columns.Count != o._columns.Count)
                    return false;
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (String.Compare(_columns[i], o._columns[i]) != 0 ||
                        _columnWidths[i] != o._columnWidths[i])
                        return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                // just to hide the compiler warning
                return base.GetHashCode();
            }

            #region ICloneable Members

            public object Clone()
            {
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
        public DialogResult MessageQuestion(string text)
        {
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
		public DialogResult MessageQuestion(string text, string captionPostfix)
		{
			if (MainForm != null && MainForm.IsHandleCreated)
				Win32.SetForegroundWindow(MainForm.Handle);
			DialogResult res = MessageBox.Show(
				MainForm, text,
				CaptionOnly + " " + captionPostfix,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			return res;
		}

        /// <summary>
        /// Displays a informational Message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        /// <remarks>Ensures, it gets displayed in foreground and 
        /// use the main form as the parent</remarks>
        public DialogResult MessageInfo(string text)
        {
            if (MainForm != null && MainForm.IsHandleCreated)
                Win32.SetForegroundWindow(MainForm.Handle);
            return MessageBox.Show(MainForm, text,
                                   CaptionOnly,
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
        public DialogResult MessageWarn(string text)
        {
            if (MainForm != null && MainForm.IsHandleCreated)
                Win32.SetForegroundWindow(MainForm.Handle);
            return MessageBox.Show(MainForm, text,
                                   CaptionOnly,
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
        public DialogResult MessageError(string text)
        {
            if (MainForm != null && MainForm.IsHandleCreated)
                Win32.SetForegroundWindow(MainForm.Handle);
            return MessageBox.Show(MainForm, text,
                                   Caption,
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
        private static bool IsFormAvailable(Control f)
        {
            return (f != null && !f.Disposing && !f.IsDisposed);
        }

        private void SaveModifiedFeeds()
        {
            while (modifiedFeeds.Count > 0)
            {
                List<string> modifiedUrls = null;
                FeedSourceEntry entry = null; 

                lock (modifiedFeeds)
                {
                    entry = sourceManager[modifiedFeeds.ElementAt(0).Key];                     
                    modifiedUrls = modifiedFeeds.ElementAt(0).Value;
                    modifiedFeeds.Remove(entry.ID); 
                }
                try
                {
                    foreach (string feedUrl in modifiedUrls)
                    {
                        entry.Source.ApplyFeedModifications(feedUrl);
                    }
                }
                catch
                {
                }
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void LoadSearchEngines()
        {
            string p = Path.Combine(GetSearchesPath(), @"config.xml");
            if (File.Exists(p))
            {
                string errorLog = GetLogFileName();
                using (FileStream myFile = FileHelper.OpenForWriteAppend(errorLog))
                {
                    /* Create a new text writer using the output stream, and add it to
					 * the trace listeners. */
                    var myTextListener = new
                        TextWriterTraceListener(myFile);
                    Trace.Listeners.Add(myTextListener);

                    try
                    {
                        SearchEngineHandler.LoadEngines(p, SearchConfigValidationCallback);
                    }
                    catch (Exception e)
                    {
                        if (!validationErrorOccured)
                        {
                            MessageError(String.Format(SR.ExceptionLoadingSearchEnginesMessage, e.Message, errorLog));
                        }
                    }

                    if (SearchEngineHandler.EnginesOK)
                    {
                        // Build the menues, below
                    }
                    else if (validationErrorOccured)
                    {
                        MessageError(String.Format(SR.ExceptionInvalidSearchEnginesMessage, errorLog));
                        validationErrorOccured = false;
                    }
                    else
                    {
                        // no search engines
                    }

                    // Flush and close the trace output.
                    Trace.Listeners.Remove(myTextListener);
                    myTextListener.Close();
                }
            }
            else
            {
                //if (File.Exists(p))
                SearchEngineHandler.GenerateDefaultEngines();
                SaveSearchEngines();
            }
        }

        internal void SaveSearchEngines()
        {
            try
            {
                if (SearchEngineHandler != null && SearchEngineHandler.Engines != null &&
                    SearchEngineHandler.EnginesOK && SearchEngineHandler.Engines.Count > 0)
                {
                    string p = Path.Combine(GetSearchesPath(), @"config.xml");
                    SearchEngineHandler.SaveEngines(new FileStream(p, FileMode.Create));
                }
            }
            catch (InvalidOperationException ioe)
            {
                _log.Error("Unexpected Error on saving SearchEngineSettings.", ioe);
                MessageError(String.Format(SR.ExceptionWebSearchEnginesSave, ioe.InnerException.Message));
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected Error on saving SearchEngineSettings.", ex);
                MessageError(String.Format(SR.ExceptionWebSearchEnginesSave, ex.Message));
            }
        }

        /// <summary>
        /// Handles errors that occur during schema validation of search engines list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void SearchConfigValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                _log.Info("Validation Warning on search engines list: " + args.Message);
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                validationErrorOccured = true;
                _log.Error("Validation Error on search engines list: " + args.Message);
            }
        }

        #region Preferences handling

        // called from Preferences dialog via "Apply" button
        // takes over the settings
        private void OnApplyPreferences(object sender, EventArgs e)
        {
            var propertiesDialog = sender as PreferencesDialog;
            if (propertiesDialog == null)
                return;

            //validate  refresh rate before setting
            try
            {
                if (!string.IsNullOrEmpty(propertiesDialog.comboRefreshRate.Text))
                {
                    Preferences.RefreshRate = Int32.Parse(propertiesDialog.comboRefreshRate.Text)*MilliSecsMultiplier;

                    if (propertiesDialog.checkResetIndividualRefreshRates.Checked)
                    {
                        sourceManager.ForEach(fs => fs.ResetAllRefreshRateSettings());
                        feedlistModified = true;
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show(propertiesDialog,
                                SR.FormatExceptionRefreshRate,
                                SR.PreferencesExceptionMessageTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show(propertiesDialog,
                                SR.OverflowExceptionRefreshRate,
                                SR.PreferencesExceptionMessageTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //validate proxy port before before setting proxy info
            try
            {
                Preferences.ProxyPort = UInt16.Parse("0" + propertiesDialog.textProxyPort.Text);
                Preferences.UseIEProxySettings = propertiesDialog.checkUseIEProxySettings.Checked;
                Preferences.UseProxy = propertiesDialog.checkUseProxy.Checked;
                Preferences.ProxyAddress = propertiesDialog.textProxyAddress.Text;
                Preferences.ProxyCustomCredentials = propertiesDialog.checkProxyAuth.Checked;
                Preferences.BypassProxyOnLocal = propertiesDialog.checkProxyBypassLocal.Checked;
                Preferences.ProxyUser = propertiesDialog.textProxyCredentialUser.Text;
                Preferences.ProxyPassword = propertiesDialog.textProxyCredentialPassword.Text;
                Preferences.ProxyBypassList = ParseProxyBypassList(propertiesDialog.textProxyBypassList.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show(propertiesDialog,
                                SR.FormatExceptionProxyPort,
                                SR.PreferencesExceptionMessageTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show(propertiesDialog,
                                SR.ExceptionProxyPortRange,
                                SR.PreferencesExceptionMessageTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (propertiesDialog.checkCustomFormatter.Checked)
            {
                Preferences.NewsItemStylesheetFile = propertiesDialog.comboFormatters.Text;
            }
            else
            {
                Preferences.NewsItemStylesheetFile = String.Empty;
            }

            bool markedForDownloadCalled = false;
            if (Preferences.MaxItemAge != propertiesDialog.MaxItemAge)
            {
                Preferences.MaxItemAge = propertiesDialog.MaxItemAge;
                sourceManager.ForEach(fs => fs.MarkForDownload());
                markedForDownloadCalled = true;
            }

            if (propertiesDialog.checkClearFeedCategoryItemAgeSettings.Checked)
            {
                sourceManager.ForEach(fs => fs.ResetAllMaxItemAgeSettings());
                if (!markedForDownloadCalled)
                {
                    sourceManager.ForEach(fs => fs.MarkForDownload());
                    //markedForDownloadCalled = true;
                }
            }

            if (Preferences.MarkItemsReadOnExit != propertiesDialog.checkMarkItemsReadOnExit.Checked)
            {
                Preferences.MarkItemsReadOnExit = propertiesDialog.checkMarkItemsReadOnExit.Checked;
            }

            if (Preferences.UseFavicons != propertiesDialog.checkUseFavicons.Checked)
            {
                Preferences.UseFavicons = propertiesDialog.checkUseFavicons.Checked;
                try
                {
                    //reload tree view					 
					guiMain.ApplyFavicons(Preferences.UseFavicons);
                }
                catch (Exception ex)
                {
                    PublishException(ex);
                }
            }

            Preferences.NumNewsItemsPerPage = propertiesDialog.numNewsItemsPerPage.Value;

            Preferences.MarkItemsAsReadWhenViewed = propertiesDialog.checkMarkItemsAsReadWhenViewed.Checked;
            Preferences.LimitNewsItemsPerPage = propertiesDialog.checkLimitNewsItemsPerPage.Checked;
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

            Preferences.AutoUpdateFrequency = (AutoUpdateMode) propertiesDialog.comboAppUpdateFrequency.SelectedIndex;

            Preferences.NormalFont = (Font) propertiesDialog.FontForState(FontStates.Read).Clone();
            Preferences.UnreadFont = (Font) propertiesDialog.FontForState(FontStates.Unread).Clone();
            Preferences.FlagFont = (Font) propertiesDialog.FontForState(FontStates.Flag).Clone();
            Preferences.RefererFont = (Font) propertiesDialog.FontForState(FontStates.Referrer).Clone();
            Preferences.ErrorFont = (Font) propertiesDialog.FontForState(FontStates.Error).Clone();
            Preferences.NewCommentsFont = (Font) propertiesDialog.FontForState(FontStates.NewComments).Clone();

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

            switch (propertiesDialog.comboRemoteStorageProtocol.SelectedIndex)
            {
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

            if (propertiesDialog.searchEnginesModified)
            {
                // take over the new settings and move the images to the search folder 

                SearchEngineHandler.Clear(); // reset handler, if it was wrong from a initial load failure

                foreach (var engine in propertiesDialog.searchEngines)
                {
                    if (engine.ImageName != null && engine.ImageName.IndexOf(Path.DirectorySeparatorChar) > 0)
                    {
                        // absolute path, copy the image to our search config folder
                        try
                        {
                            if (File.Exists(engine.ImageName))
                                File.Copy(engine.ImageName,
                                          Path.Combine(GetSearchesPath(), Path.GetFileName(engine.ImageName)), true);
                            engine.ImageName = Path.GetFileName(engine.ImageName); // reduce to "name.ext" only
                        }
                        catch (Exception ex)
                        {
                            _log.Error("SearchEngine Image FileCopy exception", ex);
                            engine.ImageName = String.Empty;
                        }
                    }
                    SearchEngineHandler.Engines.Add(engine);
                }

                SaveSearchEngines();
                guiMain.InitSearchEngines(); // rebuild menu(s)/toolbar entries
            }

            bool browserPrefsChanged = false;

            if (Preferences.BrowserJavascriptAllowed != propertiesDialog.checkBrowserJavascriptAllowed.Checked ||
                Preferences.BrowserJavaAllowed != propertiesDialog.checkBrowserJavaAllowed.Checked ||
                Preferences.BrowserActiveXAllowed != propertiesDialog.checkBrowserActiveXAllowed.Checked ||
                Preferences.BrowserBGSoundAllowed != propertiesDialog.checkBrowserBGSoundAllowed.Checked ||
                Preferences.BrowserVideoAllowed != propertiesDialog.checkBrowserVideoAllowed.Checked ||
                Preferences.BrowserImagesAllowed != propertiesDialog.checkBrowserImagesAllowed.Checked
                )
            {
                browserPrefsChanged = true;
            }

            Preferences.BrowserJavascriptAllowed = propertiesDialog.checkBrowserJavascriptAllowed.Checked;
            Preferences.BrowserJavaAllowed = propertiesDialog.checkBrowserJavaAllowed.Checked;
            Preferences.BrowserActiveXAllowed = propertiesDialog.checkBrowserActiveXAllowed.Checked;
            Preferences.BrowserBGSoundAllowed = propertiesDialog.checkBrowserBGSoundAllowed.Checked;
            Preferences.BrowserVideoAllowed = propertiesDialog.checkBrowserVideoAllowed.Checked;
            Preferences.BrowserImagesAllowed = propertiesDialog.checkBrowserImagesAllowed.Checked;

            if (browserPrefsChanged)
            {
                guiMain.ResetHtmlDetail();
            }

            if (
                !String.Equals(Preferences.EnclosureFolder, propertiesDialog.textEnclosureDirectory.Text,
                               StringComparison.OrdinalIgnoreCase))
            {
                Preferences.EnclosureFolder = propertiesDialog.textEnclosureDirectory.Text;
            }

            if (Preferences.DownloadEnclosures != propertiesDialog.checkDownloadEnclosures.Checked)
            {
                Preferences.DownloadEnclosures = propertiesDialog.checkDownloadEnclosures.Checked;
            }

            if (Preferences.EnclosureAlert != propertiesDialog.checkEnableEnclosureAlerts.Checked)
            {
                Preferences.EnclosureAlert = propertiesDialog.checkEnableEnclosureAlerts.Checked;
            }

            if (Preferences.CreateSubfoldersForEnclosures !=
                propertiesDialog.checkDownloadCreateFolderPerFeed.Checked)
            {
                Preferences.CreateSubfoldersForEnclosures =
                    propertiesDialog.checkDownloadCreateFolderPerFeed.Checked;
            }

            if (propertiesDialog.checkOnlyDownloadLastXAttachments.Checked)
            {
                Preferences.NumEnclosuresToDownloadOnNewFeed =
                    Convert.ToInt32(propertiesDialog.numOnlyDownloadLastXAttachments.Value);
            }
            else
            {
                Preferences.NumEnclosuresToDownloadOnNewFeed =
                    FeedSource.DefaultNumEnclosuresToDownloadOnNewFeed;
            }


            if (propertiesDialog.checkEnclosureSizeOnDiskLimited.Checked)
            {
                Preferences.EnclosureCacheSize =
                    Convert.ToInt32(propertiesDialog.numEnclosureCacheSize.Value);
            }
            else
            {
                Preferences.EnclosureCacheSize =
                    FeedSource.DefaultEnclosureCacheSize;
            }


            ApplyPreferences();
            SavePreferences();
        }

        private static string[] ParseProxyBypassList(string proxyBypassString)
        {
            return ListHelper.StripEmptyEntries(proxyBypassString.Split(';', ' ', ','));
        }

        // called, to apply preferences to the NewsComponents and Gui
        internal void ApplyPreferences()
        {
            ApplyMaxItemAge(Preferences.MaxItemAge);
            ApplyRefreshRate(Preferences.RefreshRate);
            ApplyDownloadEnclosures(Preferences.DownloadEnclosures);

            // assigns the globally used proxy:
            Proxy = CreateProxyFrom(Preferences);

            //TODO: same as in Init() ?
            FeedSource.BuildRelationCosmos = Preferences.BuildRelationCosmos;

            try
            {
                NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile,
                                                   GetNewsItemFormatterTemplate());
            }
            catch
            {
                //this.NewsItemFormatter.XslStyleSheet = NewsItemFormatter.DefaultNewsItemTemplate;				
                Preferences.NewsItemStylesheetFile = String.Empty;
                NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile,
                                                   GetNewsItemFormatterTemplate());
            }

            FeedSource.Stylesheet = Preferences.NewsItemStylesheetFile;
            FeedSource.EnclosureFolder = Preferences.EnclosureFolder;
            FeedSource.EnclosureAlert = Preferences.EnclosureAlert;
            FeedSource.CreateSubfoldersForEnclosures = Preferences.CreateSubfoldersForEnclosures;
            FeedSource.NumEnclosuresToDownloadOnNewFeed = Preferences.NumEnclosuresToDownloadOnNewFeed;
            FeedSource.EnclosureCacheSize = Preferences.EnclosureCacheSize;
            FeedSource.MarkItemsReadOnExit = Preferences.MarkItemsReadOnExit;

            Win32.ApplicationSoundsAllowed = Preferences.AllowAppEventSounds;
        }

        /// <summary>
        /// Creates the proxy to be used from preferences.
        /// </summary>
        /// <param name="p">The preferences</param>
        /// <remarks>
        /// It works this way:
        /// 1. If "Don't use a proxy" is set, we do nothing specific here
        ///    and use the by default the settings from machine/app.config
        ///    returning 
        /// 2. If "take over settings from IE" is set, we call 
        ///    <see cref="WebRequest.GetSystemWebProxy">GetSystemWebProxy</see>
        ///	   to retrive return that specific proxy.
        /// </remarks>
        /// <returns></returns>
        private static IWebProxy CreateProxyFrom(IUserPreferences p)
        {
            if (p.UseProxy)
            {
                WebProxy proxy;
                // private proxy settings

                if (p.ProxyPort > 0)
                    proxy = new WebProxy(p.ProxyAddress, p.ProxyPort);
                else
                    proxy = new WebProxy(p.ProxyAddress);

                (proxy).BypassProxyOnLocal = p.BypassProxyOnLocal;
                //Get rid of String.Empty in by pass list because it means bypass on all URLs
                (proxy).BypassList = ListHelper.StripEmptyEntries(p.ProxyBypassList);

                if (p.ProxyCustomCredentials)
                {
					proxy.Credentials = CredentialCache.DefaultCredentials;
                    if (!string.IsNullOrEmpty(p.ProxyUser))
                    {
                        proxy.Credentials = FeedSource.CreateCredentialsFrom(p.ProxyUser, p.ProxyPassword);
                    }
                }

                return proxy;
            } /* endif UseProxy */

			if (p.UseIEProxySettings)
			{
				// IE proxy settings do not include proxy credentials!
				IWebProxy proxy= WebRequest.GetSystemWebProxy();
				if (p.ProxyCustomCredentials)
				{
					proxy.Credentials = CredentialCache.DefaultCredentials;
					if (!string.IsNullOrEmpty(p.ProxyUser))
					{
						proxy.Credentials = FeedSource.CreateCredentialsFrom(p.ProxyUser, p.ProxyPassword);
					}
				}
				return proxy;
			}

        	// default proxy init -

            // No need to do anything special for .NET 2.0:
            // http://msdn.microsoft.com/msdnmag/issues/05/08/AutomaticProxyDetection/default.aspx#S3

            // force switch to system default proxy:
            return null;
        }

        internal void LoadPreferences()
        {
            string pName = GetPreferencesFileName();
            bool migrate = false;
			SoapFormatter sf = new SoapFormatter();
        	// we don't rely on strong assembly names for prefs.:
			sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
			sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
            
			IFormatter formatter = sf;

            if (! File.Exists(pName))
            {
                // migrate from binary to XML
                string pOldName = GetPreferencesFileNameOldBinary();
                string pTempNew = pOldName + ".v13"; // in between temp prefs

                if (File.Exists(pTempNew))
                {
                    pName = pTempNew; // migrate from in-between
                    migrate = true;
                    formatter = new BinaryFormatter();
                }
                else if (File.Exists(pOldName))
                {
                    pName = pOldName;
                    migrate = true;
                    formatter = new BinaryFormatter();
                }
            }

            if (File.Exists(pName))
            {
                using (Stream stream = FileHelper.OpenForRead(pName))
                {
                    try
                    {
                        // to provide backward compat.:
                        formatter.Binder = new RssBanditPreferences.DeserializationTypeBinder();
                        var p = (RssBanditPreferences) formatter.Deserialize(stream);
                        Preferences = p;
                    }
                    catch (Exception e)
                    {
                        _log.Error("Preferences DeserializationException", e);
                        Preferences = DefaultPreferences;
                        migrate = false;
                    }
                }

                if (migrate)
                {
                    SavePreferences();
                }
            }
            else
            {
                // no preferences saved yet, use default:
                Preferences = DefaultPreferences;
            }
        }

        internal void SavePreferences()
        {
            using (var stream = new MemoryStream())
            {
				SoapFormatter sf = new SoapFormatter();
				// we don't rely on strong assembly names for prefs.:
				sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
				sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;

				IFormatter formatter = sf;
                try
                {
                    formatter.Serialize(stream, Preferences);
                    string pName = GetPreferencesFileName();
                    if (FileHelper.WriteStreamWithBackup(pName, stream))
                    {
                        // on success, raise event:
                        if (PreferencesChanged != null)
                            PreferencesChanged(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("SavePreferences() failed.", ex);
                }
            }
        }


        private void CheckAndMigrateSettingsAndPreferences()
        {
            // check, if any migration task have been applied. If so, we have to:
            bool saveChanges = false;

            // v.1.2.x to 1.3.x:
            // The old (one) username, mail and referer 
            // have to be migrated from Preferences to the default UserIdentity

            // Obsolete() warnings can be ignored for that function.

            if (!string.IsNullOrEmpty(Preferences.UserName) &&
                string.IsNullOrEmpty(Preferences.UserIdentityForComments))
            {
				FeedSourceEntry entry4migration = BanditFeedSourceEntry;
				if (entry4migration != null &&
					!entry4migration.Source.UserIdentity.ContainsKey(Preferences.UserName))
                {
                    //create a UserIdentity from Prefs. properties
                    var ui = new UserIdentity();
                    ui.Name = ui.RealName = Preferences.UserName;
                    ui.ResponseAddress = ui.MailAddress = Preferences.UserMailAddress;
                    ui.ReferrerUrl = Preferences.Referer;
					entry4migration.Source.UserIdentity.Add(ui.Name, ui);
                    feedlistModified = true;
                }
                else
                {
                    // yet contain the ID, nothing to do here
                }

                // set/reset values:
                Preferences.UserIdentityForComments = Preferences.UserName;
                Preferences.UserName = String.Empty;
                Preferences.UserMailAddress = String.Empty;
                Preferences.Referer = String.Empty;
                saveChanges = true;
            }

            // 1.6.x migrations to phoenix (2.0):

            // migrate stylesheet:
            if (FeedSource.MigrationProperties.ContainsKey("Stylesheet"))
            {
                // take over from previous version, as we stored that in feedlist:
                Preferences.NewsItemStylesheetFile = (string) FeedSource.MigrationProperties["Stylesheet"];
                // loaded from feedlist or default setting
                try
                {
                    NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile,
                                                       GetNewsItemFormatterTemplate());
                }
                catch
                {
                    Preferences.NewsItemStylesheetFile = String.Empty;
                    NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile,
                                                       GetNewsItemFormatterTemplate());
                }

                FeedSource.Stylesheet = Preferences.NewsItemStylesheetFile;
                saveChanges = true;
            }            

            // migrate refreshrate:
            if (FeedSource.MigrationProperties.ContainsKey("RefreshRate"))
            {
                // take over from previous version, as we stored that in feedlist:
                Preferences.RefreshRate = (int) FeedSource.MigrationProperties["RefreshRate"];
                // loaded from feedlist or default setting
                ApplyRefreshRate(Preferences.RefreshRate);

                saveChanges = true;
            }

            // migrate maxitemage:
            if (FeedSource.MigrationProperties.ContainsKey("MaxItemAge"))
            {
                // take over from previous version, as we stored that in feedlist:
                Preferences.MaxItemAge = XmlConvert.ToTimeSpan((string) FeedSource.MigrationProperties["MaxItemAge"]);
                // loaded from feedlist or default setting
                ApplyMaxItemAge(Preferences.MaxItemAge);

                saveChanges = true;
            }

            // migrate EnclosureFolder saved previously at feedlist to preferences:
            if (FeedSource.MigrationProperties.ContainsKey("EnclosureFolder"))
            {
                Preferences.EnclosureFolder = (string) FeedSource.MigrationProperties["EnclosureFolder"];
                FeedSource.EnclosureFolder = Preferences.EnclosureFolder;
                saveChanges = true;
            }

            // migrate EnclosureFolder saved previously at feedlist to preferences:
            if (FeedSource.MigrationProperties.ContainsKey("DownloadEnclosures"))
            {
                Preferences.DownloadEnclosures = (bool) FeedSource.MigrationProperties["DownloadEnclosures"];
                ApplyDownloadEnclosures(Preferences.DownloadEnclosures);
                saveChanges = true;
            }

            if (FeedSource.MigrationProperties.ContainsKey("EnclosureAlert"))
            {
                Preferences.EnclosureAlert = (bool) FeedSource.MigrationProperties["EnclosureAlert"];
                FeedSource.EnclosureAlert = Preferences.EnclosureAlert;
                saveChanges = true;
            }

			// migrate PodcastFolder saved previously at feedlist to preferences:
			if (FeedSource.MigrationProperties.ContainsKey("PodcastFolder"))
			{
				Preferences.PodcastFolder = (string)FeedSource.MigrationProperties["PodcastFolder"];
				FeedSource.PodcastFolder = Preferences.PodcastFolder;
				saveChanges = true;
			}
			// migrate PodcastFileExtensions saved previously at feedlist to preferences:
			if (FeedSource.MigrationProperties.ContainsKey("PodcastFileExtensions"))
			{
				Preferences.PodcastFileExtensions = (string)FeedSource.MigrationProperties["PodcastFileExtensions"];
				FeedSource.PodcastFileExtensionsAsString = Preferences.PodcastFileExtensions;
				saveChanges = true;
			}

            if (FeedSource.MigrationProperties.ContainsKey("MarkItemsReadOnExit"))
            {
                Preferences.MarkItemsReadOnExit = (bool) FeedSource.MigrationProperties["MarkItemsReadOnExit"];
                FeedSource.MarkItemsReadOnExit = Preferences.MarkItemsReadOnExit;
                saveChanges = true;
            }

            if (FeedSource.MigrationProperties.ContainsKey("NumEnclosuresToDownloadOnNewFeed"))
            {
                Preferences.NumEnclosuresToDownloadOnNewFeed =
                    (int) FeedSource.MigrationProperties["NumEnclosuresToDownloadOnNewFeed"];
                FeedSource.NumEnclosuresToDownloadOnNewFeed = Preferences.NumEnclosuresToDownloadOnNewFeed;
                saveChanges = true;
            }

            if (FeedSource.MigrationProperties.ContainsKey("CreateSubfoldersForEnclosures"))
            {
                Preferences.CreateSubfoldersForEnclosures =
                    (bool) FeedSource.MigrationProperties["CreateSubfoldersForEnclosures"];
                FeedSource.CreateSubfoldersForEnclosures = Preferences.CreateSubfoldersForEnclosures;
                saveChanges = true;
            }

            if (saveChanges)
                SavePreferences();
        }

        #endregion

        #region save/load SearchFolders

        public FinderSearchNodes FindersSearchRoot
        {
            get { return findersSearchRoot; }
            set { findersSearchRoot = value; }
        }

        /// <summary>
        /// Removes default "Unread Items" search folder since this has now been replaced by a Special Folder. 
        /// </summary>
        /// <remarks>This code used to be in the custom action for the installer but was moved once we got rid 
        /// of custom actions due to Vista install issues</remarks>
        private static void RemoveUnreadItemsSearchFolders()
        {
            string searchfolders = GetSearchFolderFileName();

            try
            {
                var doc = new XmlDocument();

                if (File.Exists(searchfolders))
                {
                    //there should be an 'Unread Items' there
                    doc.Load(searchfolders);
                }
                else
                {
                    return;
                }

                var unreadItems =
                    (XmlElement)
                    doc.SelectSingleNode("/FinderSearchNodes/RssFinderNodes/RssFinder[FullPath = 'Unread Items']");

                if (unreadItems != null)
                {
                    unreadItems.ParentNode.RemoveChild(unreadItems);
                }

                doc.Save(searchfolders);
            }
            catch (Exception ex)
            {
                _log.Error("RemoveUnreadItemsSearchFolders() Exception (reading/saving file).", ex);
            }
        }


        // For backward compatibility (read previous defined search folder settings)
        // we read the old defs. from GuiSettings 
        // If we get something from there, we save it immediatly to the new file/location used for
        // search folders, then removing the entry from settings.
        // If we did not found something in settings, we test for the saerch folder defs. file and read it.
        public FinderSearchNodes LoadSearchFolders()
        {
            RemoveUnreadItemsSearchFolders();

            FinderSearchNodes fsn = null;
            bool needs2saveNew = false;

            string s = GuiSettings.GetString("FinderNodes", null);

            if (s == null)
            {
                // no old defs found. Read from search folder file

                if (File.Exists(GetSearchFolderFileName()))
                {
                    using (Stream stream = FileHelper.OpenForRead(GetSearchFolderFileName()))
                    {
                        try
                        {
                            XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof (FinderSearchNodes));
                            fsn = (FinderSearchNodes) ser.Deserialize(stream);
                        }
                        catch (Exception ex)
                        {
                            _log.Error("LoadSearchFolders::Load Exception (reading/deserialize file).", ex);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof (FinderSearchNodes));
                    fsn = (FinderSearchNodes) ser.Deserialize(new StringReader(s));
                    needs2saveNew = true;
                }
                catch (Exception ex)
                {
                    _log.Error("LoadSearchFolders::Load Exception (reading/deserialize string.", ex);
                }
            }

            if (fsn == null)
            {
                //exception occured or xsi:nil = true in searchfolders.xml
                fsn = new FinderSearchNodes();
            }

            if (needs2saveNew)
            {
                SaveSearchFolders(); // save to new file
                GuiSettings.SetProperty("FinderNodes", null); // remove from .settings.xml
            }


            return fsn;
        }

        public void SaveSearchFolders()
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof (FinderSearchNodes));
                    ser.Serialize(stream, findersSearchRoot);
                    if (FileHelper.WriteStreamWithBackup(GetSearchFolderFileName(), stream))
                    {
                        // OK
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("SaveSearchFolders::Save Exception.", ex);
                }
            }
        }

        #endregion

        #region Manage trusted CertificateIssues

        public void AddTrustedCertificateIssue(string site, CertificateIssue issue)
        {
            lock (AsyncWebRequest.TrustedCertificateIssues)
            {
                IList<CertificateIssue> issues = null;
                if (AsyncWebRequest.TrustedCertificateIssues.ContainsKey(site))
                {
                    issues = AsyncWebRequest.TrustedCertificateIssues[site];
                    AsyncWebRequest.TrustedCertificateIssues.Remove(site);
                }
                if (issues == null)
                    issues = new List<CertificateIssue>(1);

                if (!issues.Contains(issue))
                    issues.Add(issue);

                AsyncWebRequest.TrustedCertificateIssues.Add(site, issues);
            }

            trustedCertIssuesModified = true;
        }

        internal static void LoadTrustedCertificateIssues()
        {
            if (File.Exists(GetTrustedCertIssuesFileName()))
            {
                lock (AsyncWebRequest.TrustedCertificateIssues)
                {
                    AsyncWebRequest.TrustedCertificateIssues.Clear();
                }

                using (Stream stream = FileHelper.OpenForRead(GetTrustedCertIssuesFileName()))
                {
                    var doc = new XPathDocument(stream);
                    XPathNavigator nav = doc.CreateNavigator();

                    try
                    {
                        XPathNodeIterator siteIssues = nav.Select("descendant::issues");

                        while (siteIssues.MoveNext())
                        {
                            string url = siteIssues.Current.GetAttribute("site", String.Empty);
                            if (url == null)
                            {
                                continue;
                            }

                            var issues = new List<CertificateIssue>();

                            XPathNodeIterator theIssues = siteIssues.Current.Select("issue");
                            while (theIssues.MoveNext())
                            {
                                if (theIssues.Current.IsEmptyElement)
                                {
                                    continue;
                                }
                                string issue = theIssues.Current.Value;
                                try
                                {
                                    var ci =
                                        (CertificateIssue) Enum.Parse(typeof (CertificateIssue), issue);
                                    issues.Add(ci);
                                }
                                catch
                                {
                                    /* ignore parse errors */
                                }
                            }

                            if (issues.Count > 0)
                            {
                                lock (AsyncWebRequest.TrustedCertificateIssues)
                                {
                                    AsyncWebRequest.TrustedCertificateIssues.Add(url, issues);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Report exception.
                        _log.Debug(
                            "LoadTrustedCertificateIssues: There was an error while deserializing from Settings Storage.  Ignoring.");
                        _log.Debug("LoadTrustedCertificateIssues: The exception was:", e);
                    }
                }
            }
        }

        internal void SaveTrustedCertificateIssues()
        {
            using (var stream = new MemoryStream())
            {
                XmlTextWriter writer = null;
                try
                {
                    writer = new XmlTextWriter(stream, null)
                                 {
                                     Formatting = Formatting.Indented,
                                     Indentation = 2
                                 };
                    // Use indentation for readability.
                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("trustedCertificateIssues");

                    lock (AsyncWebRequest.TrustedCertificateIssues)
                    {
                        foreach (var url in AsyncWebRequest.TrustedCertificateIssues.Keys)
                        {
                            var trusted = (ICollection) AsyncWebRequest.TrustedCertificateIssues[url];
                            if (trusted != null && trusted.Count > 0)
                            {
                                writer.WriteStartElement("issues");
                                writer.WriteAttributeString("site", url);
                                foreach (CertificateIssue issue in trusted)
                                {
                                    writer.WriteStartElement("issue");
                                    writer.WriteString(issue.ToString());
                                    writer.WriteEndElement(); //issue
                                }
                                writer.WriteEndElement(); //issues
                            }
                        }
                    }

                    writer.WriteEndElement(); //trustedCertificateIssues
                    writer.WriteEndDocument();
                    writer.Flush();

                    try
                    {
                        if (FileHelper.WriteStreamWithBackup(GetTrustedCertIssuesFileName(), stream))
                        {
                            // success
                            trustedCertIssuesModified = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("SaveTrustedCertificateIssues() failed.", ex);
                    }
                }
                catch (Exception e)
                {
                    // Release all resources held by the XmlTextWriter.
                    if (writer != null)
                        writer.Close();

                    // Report exception.
                    _log.Debug(
                        "SaveTrustedCertificateIssues: There was an error while serializing to Storage.  Ignoring.");
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

            if (defaultFeedColumnLayoutKey == null)
            {
                foreach (var e in feedHandler.ColumnLayouts)
                {
                    if (e.Value.LayoutType == LayoutType.GlobalFeedLayout)
                    {
                        defaultFeedColumnLayoutKey = e.Key;
                        break;
                    }
                }
                if (defaultFeedColumnLayoutKey == null)
                {
                    defaultFeedColumnLayoutKey = Guid.NewGuid().ToString("N");
                    feedHandler.ColumnLayouts.Add(defaultFeedColumnLayoutKey, DefaultFeedColumnLayout);
                    feedlistModified = true;
                }
            }
        }

        private void ValidateGlobalCategoryColumnLayout()
        {
            if (!feedHandler.FeedsListOK)
                return;

            if (defaultCategoryColumnLayoutKey == null)
            {
                foreach (var e in feedHandler.ColumnLayouts)
                {
                    if (e.Value.LayoutType == LayoutType.GlobalCategoryLayout)
                    {
                        defaultCategoryColumnLayoutKey = e.Key;
                        break;
                    }
                }
                if (defaultCategoryColumnLayoutKey == null)
                {
                    defaultCategoryColumnLayoutKey = Guid.NewGuid().ToString("N");
                    feedHandler.ColumnLayouts.Add(defaultCategoryColumnLayoutKey, DefaultCategoryColumnLayout);
                    feedlistModified = true;
                }
            }
        }

        private void ValidateGlobalSearchFolderColumnLayout()
        {
            if (!feedHandler.FeedsListOK)
                return;

            if (defaultSearchFolderColumnLayoutKey == null)
            {
                foreach (var e in feedHandler.ColumnLayouts)
                {
                    if (e.Value.LayoutType == LayoutType.SearchFolderLayout)
                    {
                        defaultSearchFolderColumnLayoutKey = e.Key;
                        break;
                    }
                }
                if (defaultSearchFolderColumnLayoutKey == null)
                {
                    defaultSearchFolderColumnLayoutKey = Guid.NewGuid().ToString("N");
                    feedHandler.ColumnLayouts.Add(defaultSearchFolderColumnLayoutKey, DefaultSearchFolderColumnLayout);
                    feedlistModified = true;
                }
            }
        }

        private void ValidateGlobalSpecialFolderColumnLayout()
        {
            if (!feedHandler.FeedsListOK)
                return;

            if (defaultSpecialFolderColumnLayoutKey == null)
            {
                foreach (var e in feedHandler.ColumnLayouts)
                {
                    if (e.Value.LayoutType == LayoutType.SpecialFeedsLayout)
                    {
                        defaultSpecialFolderColumnLayoutKey = e.Key;
                        break;
                    }
                }
                if (defaultSpecialFolderColumnLayoutKey == null)
                {
                    defaultSpecialFolderColumnLayoutKey = Guid.NewGuid().ToString("N");
                    feedHandler.ColumnLayouts.Add(defaultSpecialFolderColumnLayoutKey, DefaultSpecialFolderColumnLayout);
                    feedlistModified = true;
                }
            }
        }

        private void RemoveSimilarColumnLayouts(FeedColumnLayout layout)
        {
            if (layout == null) return;
            for (int i = 0; i < feedHandler.ColumnLayouts.Count; i++)
            {
                if (feedHandler.ColumnLayouts[i].Value.LayoutType == LayoutType.IndividualLayout)
                {
                    if (feedHandler.ColumnLayouts[i].Value.Equals(layout, true))
                    {
                        feedHandler.ColumnLayouts.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public FeedColumnLayout GlobalFeedColumnLayout
        {
            get
            {
                ValidateGlobalFeedColumnLayout();
                return feedHandler.ColumnLayouts.GetByKey(defaultFeedColumnLayoutKey);
            }
            set
            {
                ValidateGlobalFeedColumnLayout();
                if (value == null) return;
                value.LayoutType = LayoutType.GlobalFeedLayout;
                int index = feedHandler.ColumnLayouts.IndexOfKey(defaultFeedColumnLayoutKey);
                if ((index == -1) || (!feedHandler.ColumnLayouts[index].Value.Equals(value)))
                {
                    feedHandler.ColumnLayouts[index] = new FeedColumnLayoutEntry(defaultFeedColumnLayoutKey, value);
                    RemoveSimilarColumnLayouts(value);
                    feedlistModified = true;
                }
            }
        }

        public FeedColumnLayout GlobalCategoryColumnLayout
        {
            get
            {
                ValidateGlobalCategoryColumnLayout();
                return feedHandler.ColumnLayouts.GetByKey(defaultCategoryColumnLayoutKey);
            }
            set
            {
                ValidateGlobalCategoryColumnLayout();
                if (value == null) return;
                value.LayoutType = LayoutType.GlobalCategoryLayout;
                int index = feedHandler.ColumnLayouts.IndexOfKey(defaultCategoryColumnLayoutKey);
                if (!feedHandler.ColumnLayouts[index].Value.Equals(value))
                {
                    feedHandler.ColumnLayouts[index] = new FeedColumnLayoutEntry(defaultCategoryColumnLayoutKey, value);
                    RemoveSimilarColumnLayouts(value);
                    feedlistModified = true;
                }
            }
        }

        public FeedColumnLayout GlobalSearchFolderColumnLayout
        {
            get
            {
                ValidateGlobalSearchFolderColumnLayout();
                return feedHandler.ColumnLayouts.GetByKey(defaultSearchFolderColumnLayoutKey);
            }
            set
            {
                ValidateGlobalSearchFolderColumnLayout();
                if (value == null) return;
                value.LayoutType = LayoutType.SearchFolderLayout;
                int index = feedHandler.ColumnLayouts.IndexOfKey(defaultSearchFolderColumnLayoutKey);
                if (!feedHandler.ColumnLayouts[index].Value.Equals(value))
                {
                    feedHandler.ColumnLayouts[index] =
                        new FeedColumnLayoutEntry(defaultSearchFolderColumnLayoutKey, value);
                    feedlistModified = true;
                }
            }
        }

        public FeedColumnLayout GlobalSpecialFolderColumnLayout
        {
            get
            {
                ValidateGlobalSpecialFolderColumnLayout();
                return feedHandler.ColumnLayouts.GetByKey(defaultSpecialFolderColumnLayoutKey);
            }
            set
            {
                ValidateGlobalSpecialFolderColumnLayout();
                if (value == null) return;
                value.LayoutType = LayoutType.SpecialFeedsLayout;
                int index = feedHandler.ColumnLayouts.IndexOfKey(defaultSpecialFolderColumnLayoutKey);
                if (!feedHandler.ColumnLayouts[index].Value.Equals(value))
                {
                    feedHandler.ColumnLayouts[index] =
                        new FeedColumnLayoutEntry(defaultSpecialFolderColumnLayoutKey, value);
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
        public FeedColumnLayout GetFeedColumnLayout(string feedUrl)
        {
            if (feedUrl == null)
                return null;

            string layout = feedHandler.GetFeedColumnLayout(feedUrl);

            if (string.IsNullOrEmpty(layout))
                return GlobalFeedColumnLayout;

            if (feedHandler.ColumnLayouts.ContainsKey(layout))
                return feedHandler.ColumnLayouts.GetByKey(layout);

            // invalid key: cleanup
            feedHandler.SetFeedColumnLayout(feedUrl, null);
            feedlistModified = true;
            return GlobalFeedColumnLayout;
        }

        public void SetFeedColumnLayout(string feedUrl, FeedColumnLayout layout)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return;

            if (layout == null)
            {
                // reset
                feedHandler.SetFeedColumnLayout(feedUrl, null);
                feedlistModified = true;
                return;
            }

            if (layout.LayoutType != LayoutType.IndividualLayout && layout.LayoutType != LayoutType.GlobalFeedLayout)
                return; // not a layout format we have to store for a feed

            string key = feedHandler.GetFeedColumnLayout(feedUrl);
            FeedColumnLayout global = GlobalFeedColumnLayout;

            if (string.IsNullOrEmpty(key) || false == feedHandler.ColumnLayouts.ContainsKey(key))
            {
                if (!layout.Equals(global, true))
                {
                    int known = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
                    if (known >= 0)
                    {
                        feedHandler.SetFeedColumnLayout(feedUrl, feedHandler.ColumnLayouts.GetKey(known));
                    }
                    else
                    {
                        key = Guid.NewGuid().ToString("N");
                        feedHandler.ColumnLayouts.Add(key, layout);
                        feedHandler.SetFeedColumnLayout(feedUrl, key);
                    }
                    feedlistModified = true;
                }
                else
                {
                    // similar to global: store there
                    GlobalFeedColumnLayout = layout;
                }
            }
            else
            {
                int known = feedHandler.ColumnLayouts.IndexOfKey(key);
                if (!feedHandler.ColumnLayouts[known].Value.Equals(layout))
                {
                    // check if layout modified
                    if (!feedHandler.ColumnLayouts[known].Value.Equals(layout, true))
                    {
                        // check if just a simple resizing of columns						

                        if (!layout.Equals(global, true))
                        {
                            //check if new layout is equivalent to current default
                            int otherKnownSimilar = feedHandler.ColumnLayouts.IndexOfSimilar(layout);

                            if (otherKnownSimilar >= 0)
                            {
                                //check if this layout is similar to an existing layout
                                //feedHandler.ColumnLayouts[otherKnownSimilar] = new FeedColumnLayoutEntry(key, layout);	// refresh layout info
                                feedHandler.SetFeedColumnLayout(feedUrl,
                                                                feedHandler.ColumnLayouts.GetKey(otherKnownSimilar));
                                // set new key
                            }
                            else
                            {
                                //this is a brand new layout
                                key = Guid.NewGuid().ToString("N");
                                feedHandler.ColumnLayouts.Add(key, layout);
                                feedHandler.SetFeedColumnLayout(feedUrl, key);
                            }
                        }
                        else
                        {
                            //new layout is equivalent to the current default
                            feedHandler.SetFeedColumnLayout(feedUrl, null);
                            feedHandler.ColumnLayouts.RemoveAt(known);
                        }
                    }
                    else
                    {
                        // this was a simple column resizing
                        feedHandler.ColumnLayouts[known] = new FeedColumnLayoutEntry(key, layout);
                        // refresh layout info
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
        public FeedColumnLayout GetCategoryColumnLayout(string category)
        {
            string layout = feedHandler.GetCategoryFeedColumnLayout(category);

            if (string.IsNullOrEmpty(layout))
                return GlobalCategoryColumnLayout;
            if (feedHandler.ColumnLayouts.ContainsKey(layout))
                return feedHandler.ColumnLayouts.GetByKey(layout);

            // invalid key: cleanup
            feedHandler.SetCategoryFeedColumnLayout(category, null);
            feedlistModified = true;
            return GlobalCategoryColumnLayout;
        }

        public void SetCategoryColumnLayout(string category, FeedColumnLayout layout)
        {
            if (string.IsNullOrEmpty(category))
                return;

            if (layout == null)
            {
                // reset
                feedHandler.SetCategoryFeedColumnLayout(category, null);
                feedlistModified = true;
                return;
            }

            if (layout.LayoutType != LayoutType.IndividualLayout && layout.LayoutType != LayoutType.GlobalCategoryLayout)
                return; // not a layout format we have to store for a category

            string key = feedHandler.GetCategoryFeedColumnLayout(category);
            FeedColumnLayout global = GlobalCategoryColumnLayout;

            if (string.IsNullOrEmpty(key) || false == feedHandler.ColumnLayouts.ContainsKey(key))
            {
                if (!layout.Equals(global, true))
                {
                    int known = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
                    if (known >= 0)
                    {
                        feedHandler.SetCategoryFeedColumnLayout(category, feedHandler.ColumnLayouts.GetKey(known));
                    }
                    else
                    {
                        key = Guid.NewGuid().ToString("N");
                        feedHandler.ColumnLayouts.Add(key, layout);
                        feedHandler.SetCategoryFeedColumnLayout(category, key);
                    }
                    feedlistModified = true;
                }
                else
                {
                    GlobalCategoryColumnLayout = layout;
                }
            }
            else
            {
                int known = feedHandler.ColumnLayouts.IndexOfKey(key);
                if (!feedHandler.ColumnLayouts[known].Value.Equals(layout))
                {
                    // modified
                    if (!feedHandler.ColumnLayouts[known].Value.Equals(layout, true))
                    {
                        // not anymore similar
                        feedHandler.SetCategoryFeedColumnLayout(category, null);
                        feedHandler.ColumnLayouts.RemoveAt(known);
                        if (!layout.Equals(global, true))
                        {
                            int otherKnownSimilar = feedHandler.ColumnLayouts.IndexOfSimilar(layout);
                            if (otherKnownSimilar >= 0)
                            {
                                feedHandler.ColumnLayouts[otherKnownSimilar] = new FeedColumnLayoutEntry(key, layout);
                                // refresh layout info
                                feedHandler.SetCategoryFeedColumnLayout(category,
                                                                        feedHandler.ColumnLayouts.GetKey(
                                                                            otherKnownSimilar)); // set new key
                            }
                            else
                            {
                                key = Guid.NewGuid().ToString("N");
                                feedHandler.ColumnLayouts.Add(key, layout);
                                feedHandler.SetCategoryFeedColumnLayout(category, key);
                            }
                        }
                    }
                    else
                    {
                        // still similar:
                        feedHandler.ColumnLayouts[known] = new FeedColumnLayoutEntry(key, layout);
                        // refresh layout info
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
        private static void InstallDefaultFeedList(string currentFeedListFileName)
        {
            if (!File.Exists(currentFeedListFileName))
            {
                using (Stream s = Resource.GetStream("Resources.default-feedlist.xml"))
                {
                    FileHelper.WriteStreamWithRename(currentFeedListFileName, s);
                } //using
            }
        }

        /// <summary>
        /// Migrate NewsGator settings from Remote Feed List Storage to Feed Source. 
        /// </summary>
        internal void CheckAndMigrateNewsGatorSettings()
        {
            //migrate newsgator feed source
            if (Preferences.RemoteStorageProtocol == RemoteStorageProtocolType.NewsgatorOnline)
            {
                if (!String.IsNullOrEmpty(Preferences.RemoteStorageUserName) && !String.IsNullOrEmpty(Preferences.RemoteStoragePassword))
                {
                    var creds = new NetworkCredential(Preferences.RemoteStorageUserName, Preferences.RemoteStoragePassword);
                    var loc = new SubscriptionLocation(FeedSourceManager.BuildSubscriptionName(sourceManager.UniqueKey, FeedSourceType.NewsGator), creds);
                    FeedSource fs = FeedSource.CreateFeedSource(FeedSourceType.NewsGator, loc);
                    FeedSourceEntry entry = sourceManager.Add(fs, SR.FeedNodeMyNewsGatorFeedsCaption);                   
                }
                Preferences.RemoteStorageProtocol = RemoteStorageProtocolType.UNC; //default 
                Preferences.RemoteStorageUserName = Preferences.RemoteStoragePassword = Preferences.RemoteStorageLocation = null;
            }
        }


        /// <summary>
        /// Load the various subscription lists. 
        /// </summary>
        internal void LoadAllFeedSourcesSubscriptions()
        {
			// can always run:
            CheckAndMigrateNewsGatorSettings();

			// run only at first startup after installation.
			// we load synchronized, to get migration done:

			if (Win32.Registry.ThisVersionExecutesFirstTimeAfterInstallation)
			{
				foreach (FeedSourceEntry fs in sourceManager.Sources)
				{
					if (fs.SourceType == FeedSourceType.DirectAccess)
					{
						// migrate old subscriptions or install a default one:
						MigrateOrInstallDefaultFeedList(fs.Source.SubscriptionLocation.Location);
						LoadFeedSourceSubscriptions(fs, false);
						// needs the feedlist to be loaded:
						CheckAndMigrateSettingsAndPreferences();
						CheckAndMigrateListViewLayouts();
					}
					else
					{
						LoadFeedSourceSubscriptions(fs, false);
					}

					//resume pending enclosure downloads
					fs.Source.ResumePendingDownloads();

					RaiseFeedSourceSubscriptionsLoaded(fs);
				
				}//foreach

				//reset first app start flag:
				Win32.Registry.ThisVersionExecutesFirstTimeAfterInstallation = false;
							
				RaiseAllFeedSourcesSubscriptionsLoaded();
			} 
			else
			{
				BeginLoadAllFeedSourcesSubscriptions();
			}

            LoadWatchedCommentsFeedlist();
        }

        /// <summary>
        /// Migrates old used feedlists to DirectAccess.
        /// </summary>
        /// <param name="currentFeedListFileName">Name of the current feed list file.</param>
        /// <exception cref="BanditApplicationException">On any failure</exception>
        internal static void MigrateOrInstallDefaultFeedList(string currentFeedListFileName)
        {
            if (File.Exists(currentFeedListFileName))
                return;

            //checks if new feed file exists and if we can migrate some older:
            string oldSubscriptionFile = GetFeedListFileName();
            string veryOldSubscriptionFile = GetOldFeedListFileName();

            if (!File.Exists(currentFeedListFileName))
            {
                if (!File.Exists(oldSubscriptionFile) && File.Exists(veryOldSubscriptionFile))
                {                   
                    File.Copy(veryOldSubscriptionFile, currentFeedListFileName); // copy to be used to load from
                }
                else if (File.Exists(oldSubscriptionFile))
                {                 
                    File.Copy(oldSubscriptionFile, currentFeedListFileName); // copy to be used to load from
                }
                else
                {
                    //create default feed list 			
                    InstallDefaultFeedList(currentFeedListFileName);
                }
            }

            // now we should have that file:
            if (!File.Exists(currentFeedListFileName))
            {
                // no feedlist file exists:
                throw new BanditApplicationException(ApplicationExceptions.FeedlistNA);
            }
        }

		private void RaiseFeedSourceSubscriptionsLoaded(FeedSourceEntry entry)
		{
			if (FeedSourceSubscriptionsLoaded != null)
			{
				try
				{
					FeedSourceSubscriptionsLoaded(this, new FeedSourceEventArgs(entry));
				} catch (Exception ex)
				{
					_log.Error("FeedSourceSubscriptionLoaded event call caused error", ex);
				}
			}
		}

		private void RaiseAllFeedSourcesSubscriptionsLoaded()
		{
			if (AllFeedSourceSubscriptionsLoaded != null)
			{
				try
				{
					AllFeedSourceSubscriptionsLoaded(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					_log.Error("FeedSourceSubscriptionLoaded event call caused error", ex);
				}
			}
		}
		internal void LoadFeedSourceSubscriptions(FeedSourceEntry sourceEntry)
		{
			if (sourceEntry == null)
				return;
			
			LoadFeedSourceSubscriptions(sourceEntry, false);
		}

		
    	internal void LoadFeedSourceSubscriptions(FeedSourceEntry sourceEntry, bool IsAsyncCall)
        {
			if (sourceEntry == null)
                return;

            try
            {
				sourceEntry.Source.LoadFeedlist();

				if (sourceEntry.Source.FeedsListOK )
				{
					/* All right here... 	*/
				}
				else if (FeedSource.validationErrorOccured)
				{
					FeedSource.validationErrorOccured = false;
					throw new BanditApplicationException(ApplicationExceptions.FeedlistOnProcessContent);
				}
				else
				{
					throw new BanditApplicationException(ApplicationExceptions.FeedlistNA);
				}
            }
            catch (Exception e)
            {
				if (IsAsyncCall)
					throw;

				//TODO: change/add error messages to include source name/file!
				e.PreserveExceptionStackTrace();
				BanditApplicationException ex = e as BanditApplicationException;
				if (ex != null)
				{
					if (ex.Number == ApplicationExceptions.FeedlistOldFormat)
					{
						Application.Exit();
					}
					else if (ex.Number == ApplicationExceptions.FeedlistOnRead)
					{
						PublishException(ex);
						this.MessageError(String.Format(SR.ExceptionReadingFeedlistFile, ex.InnerException.Message, GetLogFileName()));
						this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
					}
					else if (ex.Number == ApplicationExceptions.FeedlistOnProcessContent)
					{
						this.MessageError(String.Format(SR.InvalidFeedlistFileMessage, GetLogFileName()));
						this.SetGuiStateFeedbackText(SR.GUIStatusValidationErrorReadingFeedlistFile);
					}
					else if (ex.Number == ApplicationExceptions.FeedlistNA)
					{
						if (this.Preferences.RefreshRate < 0)
							this.Preferences.RefreshRate = FeedSource.DefaultRefreshRate;
						this.SetGuiStateFeedbackText(SR.GUIStatusNoFeedlistFile);
					}
					else
					{
						PublishException(ex);
						this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
					}
				}
				else
				{
					// unhandled
					PublishException(e);
					this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
				}
                
            }
        }

        internal void LoadWatchedCommentsFeedlist()
        {
            string p = GetCommentsFeedListFileName();

            if (File.Exists(p))
            {
                try
                {
                    //we don't want properties from comment feed handler to override our actual settings
                    FeedSource.MigrateProperties = false;

                    commentFeedsHandler.LoadFeedlist();

                    foreach (var f in commentFeedsHandler.GetFeeds().Values)
                    {
                        if ((f.Any != null) && (f.Any.Length > 0))
                        {
                            XmlElement origin = f.Any[0];
                            string sourceFeedUrl = origin.InnerText;
                            INewsFeed sourceFeed;
                            if (feedHandler.GetFeeds().TryGetValue(sourceFeedUrl, out sourceFeed))
                            {
                                f.Tag = sourceFeed;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error("Exception on loading '" + p + "'.", e);
                }
            }
        }

        public void SynchronizeFeeds()
        {
            var wiz = new SynchronizeFeedsWizard(this);

            try
            {
                if (MainForm.IsHandleCreated)
                    Win32.SetForegroundWindow(MainForm.Handle);
                wiz.ShowDialog(guiMain);
            }
            catch (Exception ex)
            {
                _log.Error("SynchronizeFeeds caused exception.", ex);
                wiz.DialogResult = DialogResult.Cancel;
            }

            if (wiz.DialogResult == DialogResult.OK)
            {
                FeedSourceEntry entry = null;
                FeedSource fs; 
                
                if (wiz.SelectedFeedSource == FeedSourceType.WindowsRSS)
                {
                    fs = FeedSource.CreateFeedSource(FeedSourceType.WindowsRSS, new SubscriptionLocation(FeedSourceManager.BuildSubscriptionName(sourceManager.UniqueKey, FeedSourceType.WindowsRSS), null));
                    entry = sourceManager.Add(fs, wiz.FeedSourceName);
                }
                else if (wiz.SelectedFeedSource == FeedSourceType.Google)
                {
                    SubscriptionLocation loc = new SubscriptionLocation(FeedSourceManager.BuildSubscriptionName(sourceManager.UniqueKey, FeedSourceType.Google), new NetworkCredential(wiz.UserName, wiz.Password));
                    fs = FeedSource.CreateFeedSource(FeedSourceType.Google, loc); 
                    entry = sourceManager.Add(fs, wiz.FeedSourceName);

                }
                else if (wiz.SelectedFeedSource == FeedSourceType.NewsGator)
                {

                    SubscriptionLocation loc = new SubscriptionLocation(FeedSourceManager.BuildSubscriptionName(sourceManager.UniqueKey, FeedSourceType.NewsGator), new NetworkCredential(wiz.UserName, wiz.Password));
                    fs = FeedSource.CreateFeedSource(FeedSourceType.NewsGator, loc);
                    entry = sourceManager.Add(fs, wiz.FeedSourceName);
                }

                if (entry != null)
                {
                    AddFeedSourceToUserInterface(entry); 
                }
            }
        }


        private void AddFeedSourceToUserInterface(FeedSourceEntry entry)
        {
            this.guiMain.CreateFeedSourceView(entry, true);
            this.guiMain.AddToSubscriptionTree(entry);
            this.guiMain.SelectFeedSource(entry);
            this.guiMain.MaximizeNavigatorSelectedGroup();
            this.LoadFeedSourceSubscriptions(entry, false);
            this.guiMain.PopulateFeedSubscriptions(entry, DefaultCategory);
            TreeFeedsNodeBase root = this.guiMain.GetSubscriptionRootNode(entry);
            if (root != null) root.Expanded = true;
            ConnectFeedSourceEvents(entry.Source);
            threadResultManager.ConnectFeedSourceEvents(entry.Source); //for updated feeds
            SaveFeedSources();
        }

		public void SaveFeedSources()
		{
			try
			{
				guiMain.ApplyOrderToFeedSources(sourceManager.Sources);
				sourceManager.SaveFeedSources(GetFeedSourcesFileName());
			} catch (Exception saveException)
			{
				_log.Error("Error saving feed sources", saveException);
				MessageError("Error saving your feed sources: " + saveException.Message);
			}
		}

		public void RemoveFeedSource(FeedSourceEntry entry)
		{
			if (entry == null)
				return;
			
			DisconnectFeedSourceEvents(entry.Source);
			sourceManager.Remove(entry);
			SaveFeedSources();

            entry.Source.DeleteAllFeedsAndCategories(false); 

			//TODO: handle case where all sources removed!
            if (sourceManager.Count > 0)
            {
                FeedSourceEntry next = sourceManager.Sources.First();
                guiMain.SelectFeedSource(next);
                guiMain.GetSubscriptionRootNode(next).Selected = true; 
            }
			guiMain.RemoveFromSubscriptionTree(entry);
			guiMain.RemoveFeedSourceView(entry);
            
		}

		private void ConnectFeedSourceEvents(FeedSource source)
		{
			source.BeforeDownloadFeedStarted += BeforeDownloadFeedStarted;
			source.UpdateFeedsStarted += OnUpdateFeedsStarted;
			source.OnUpdatedFavicon += OnUpdatedFavicon;
			source.OnDownloadedEnclosure += OnDownloadedEnclosure;

			source.OnAllAsyncRequestsCompleted += OnAllRequestsCompleted;
			source.OnAddedCategory += OnAddedCategory;
			source.OnDeletedCategory += OnDeletedCategory;
			source.OnMovedCategory += OnMovedCategory;
			source.OnRenamedCategory += OnRenamedCategory;
			source.OnAddedFeed += OnAddedFeed;
			source.OnDeletedFeed += OnDeletedFeed;
			source.OnRenamedFeed += OnRenamedFeed;
			source.OnMovedFeed += OnMovedFeed;
		}

		// to be called on a manual remove of the user!
		private void DisconnectFeedSourceEvents(FeedSource source)
		{
			source.BeforeDownloadFeedStarted -= BeforeDownloadFeedStarted;
			source.UpdateFeedsStarted -= OnUpdateFeedsStarted;
			source.OnUpdatedFavicon -= OnUpdatedFavicon;
			source.OnDownloadedEnclosure -= OnDownloadedEnclosure;

			source.OnAllAsyncRequestsCompleted -= OnAllRequestsCompleted;
			source.OnAddedCategory -= OnAddedCategory;
			source.OnDeletedCategory -= OnDeletedCategory;
			source.OnMovedCategory -= OnMovedCategory;
			source.OnRenamedCategory -= OnRenamedCategory;
			source.OnAddedFeed -= OnAddedFeed;
			source.OnDeletedFeed -= OnDeletedFeed;
			source.OnRenamedFeed -= OnRenamedFeed;
			source.OnMovedFeed -= OnMovedFeed;
		}

        public void ImportFeeds(string fromFileOrUrl)
        {
            ImportFeeds(fromFileOrUrl, String.Empty, String.Empty);
        }

        public void ImportFeeds(string fromFileOrUrl, string selectedCategory, string selectedFeedSource)
        {
            var dialog =
                new ImportFeedsDialog(fromFileOrUrl, selectedCategory, defaultCategory, selectedFeedSource,
                                      FeedSources);
            try
            {
                dialog.ShowDialog(guiMain);
            }
            catch (Exception e)
            {
                _log.Error("Error on opening Import Feeds Dialog:" + e);
            }

            Application.DoEvents(); // give time to visualize dismiss the dialog andredraw the UI

            if (dialog.DialogResult == DialogResult.OK)
            {
                string s = dialog.FeedsUrlOrFile;
                string cat = (dialog.FeedCategory == DefaultCategory ? null : dialog.FeedCategory);
                FeedSourceEntry entry = sourceManager[dialog.FeedSource]; 

                if (!string.IsNullOrEmpty(s))
                {
                    Stream myStream;
                    if (File.Exists(s))
                    {
                        using (myStream = FileHelper.OpenForRead(s))
                        {
                            try
                            {
                                entry.Source.ImportFeedlist(myStream, cat);
                                SubscriptionModified(entry, NewsFeedProperty.General);
                            }
                            catch (Exception ex)
                            {
                                MessageError(String.Format(SR.ExceptionImportFeedlist, s, ex.Message));
                                return;
                            }
                            guiMain.SaveSubscriptionTreeState();
							//TODO: we should reload only the imported source:
                            guiMain.InitiatePopulateTreeFeeds();
                            guiMain.LoadAndRestoreSubscriptionTreeState();
                        }
                    }
                    else
                    {
                        Uri uri = null;
                        try
                        {
                            uri = new Uri(s);
                        }
                        catch
                        {
                        }
                        if (uri != null)
                        {
                            var fileHandler =
                                new HttpRequestFileThreadHandler(uri.CanonicalizedUri(), this.Proxy);
                            DialogResult result =
                                fileHandler.Start(guiMain,
                                                  String.Format(SR.GUIStatusWaitMessageRequestFile,
                                                                uri.CanonicalizedUri()));

                            if (result != DialogResult.OK)
                                return;

                            if (!fileHandler.OperationSucceeds)
                            {
                                MessageError(String.Format(SR.WebExceptionOnUrlAccess,
                                                           uri.CanonicalizedUri(),
                                                           fileHandler.OperationException.Message));
                                return;
                            }

                            myStream = fileHandler.ResponseStream;
                            if (myStream != null)
                            {
                                using (myStream)
                                {
                                    try
                                    {
                                        entry.Source.ImportFeedlist(myStream, cat);
                                        SubscriptionModified(entry, NewsFeedProperty.General);
                                        //this.FeedlistModified = true;									}
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageError(String.Format(SR.ExceptionImportFeedlist, s, ex.Message));
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

        internal void DeleteFeed(FeedSourceEntry entry, string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            // was possibly an error causing feed:
            ExceptionManager.GetInstance().RemoveFeed(entry, url);

            INewsFeed f = GetFeed(entry, url);
			    
			if (f != null)
            {
                f.Tag = null;
                try
                {
                    entry.Source.DeleteFeed(url);
                }
                catch (ApplicationException ex)
                {
                    _log.Error(String.Format("DeleteFeed({0})", url), ex);
                }
                FeedWasModified(f, NewsFeedProperty.FeedRemoved);
                //this.FeedlistModified = true;
            }
			
			// behave the same as FeedSource event(s): feed is already removed from source
			// before we raise/get the event:
			RaiseFeedDeleted(entry, url, f != null ? f.title : null);
        }

		private void RaiseFeedDeleted(FeedSourceEntry entry, string feedUrl, string feedTitle)
        {
            if (FeedSourceFeedDeleted != null)
            {
                try
                {
					FeedSourceFeedDeleted(this, new FeedSourceFeedUrlTitleEventArgs(entry, feedUrl, feedTitle));
                }
                catch (Exception ex)
                {
                    _log.Error("RaiseFeedDeleted() error", ex);
                }
            }
        }

		/// <summary>
		/// Disable a feed (with UI update)
		/// </summary>
		/// <param name="feedUrl">string</param>
		/// <param name="entry">The entry.</param>
        public void DisableFeed(string feedUrl, FeedSourceEntry entry)
        {
            INewsFeed f;
            if (feedUrl != null && entry.Source.GetFeeds().TryGetValue(feedUrl, out f))
            {
                DisableFeed(f, TreeHelper.FindNode(guiMain.GetSubscriptionRootNode(entry), feedUrl));
            }
        }

        /// <summary>
        /// Disable a feed (with UI update)
        /// </summary>
        /// <param name="f">NewsFeed</param>
        /// <param name="feedsNode">FeedTreeNodeBase</param>
        internal void DisableFeed(INewsFeed f, TreeFeedsNodeBase feedsNode)
        {
            if (f != null)
            {
                FeedSource source = guiMain.FeedSourceOf(feedsNode).Source;
                source.DisableFeed(f.link);
                guiMain.SetSubscriptionNodeState(f, feedsNode, FeedProcessingState.Normal);
            }
        }

        /// <summary>
        /// Removes a NewsItem from a SmartFolder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="item"></param>
        public void RemoveItemFromSmartFolder(ISmartFolder folder, INewsItem item)
        {
            if (folder == null || item == null)
                return;

            if (folder is FlaggedItemsNode)
            {
                item.FlagStatus = Flagged.None;
                ReFlagNewsItem(item);
            }
            else if (folder is WatchedItemsNode)
            {
                item.WatchComments = false;
                ReWatchNewsItem(item);
            }

            //we always remove it from the smart folder regardless of type
            folder.Remove(item);
        }

        public LocalFeedsFeed FlaggedItemsFeed
        {
            get { return flaggedItemsFeed; }
            set { flaggedItemsFeed = value; }
        }

        public void ClearFlaggedItems()
        {
            foreach (var ri in flaggedItemsFeed.Items)
            {
                try
                {
                	int sourceID;
                	string feedUrl = OptionalItemElement.GetOriginalFeedReference(ri, out sourceID);
					
					if (feedUrl != null) 
					{
						FeedSourceEntry entry = null;
						if (FeedSources.ContainsKey(sourceID))
							entry = FeedSources[sourceID];
						
						if (entry != null && entry.Source.IsSubscribed(feedUrl))
                        {
                            //check if feed exists 

							IList<INewsItem> itemsForFeed = entry.Source.GetItemsForFeed(feedUrl, false);

                            //find this item 
                            int itemIndex = itemsForFeed.IndexOf(ri);


                            if (itemIndex != -1)
                            {
                                //check if item still exists 

                                INewsItem item = itemsForFeed[itemIndex];
                                item.FlagStatus = Flagged.None;
								OptionalItemElement.RemoveOriginalFeedReference(item);
                                
								break;
                            }
                        } //if(this.feedHan...)
                    } //if(elem.Equals...)						
                }
                catch (Exception e)
                {
                    _log.Error("ClearFlaggedItems() exception", e);
                }
            } //foreach(NewsItem ri...)

            flaggedItemsFeed.Items.Clear();
        }

        /// <summary>
        /// Get a NewsItem to re-flag: usually called on a already flagged item. It try to
        /// find the corresponding feed containing the item and re-flag them.
        /// </summary>
        /// <param name="theItem">NewsItem to re-flag</param>
        public void ReFlagNewsItem(INewsItem theItem)
        {
            if (theItem == null)
                return;
			
			int sourceID;
            string feedUrl = OptionalItemElement.GetOriginalFeedReference(theItem, out sourceID); // the corresponding feed Url

			FeedSourceEntry entry = null;
			if (FeedSources.ContainsKey(sourceID))
				entry = FeedSources[sourceID];

			//try
			//{
			//    XmlElement elem =
			//        RssHelper.GetOptionalElement(theItem, AdditionalElements.OriginalFeedOfFlaggedItem);
			//    if (elem != null)
			//    {
			//        feedUrl = elem.InnerText;
			//    }
			//}
			//catch
			//{
			//}

            if (theItem.FlagStatus == Flagged.None || theItem.FlagStatus == Flagged.Complete)
            {
                // remove from collection
                if (flaggedItemsFeed.Items.Contains(theItem))
                {
                    flaggedItemsFeed.Items.Remove(theItem);
                }
            }
            else
            {
                //find this item 
                int itemIndex = flaggedItemsFeed.Items.IndexOf(theItem);

                if (itemIndex != -1)
                {
                    //check if item exists 

                    INewsItem item = flaggedItemsFeed.Items[itemIndex];
                    item.FlagStatus = theItem.FlagStatus;
                }
            }

        	if (feedUrl != null && entry!= null && entry.Source.IsSubscribed(feedUrl))
            {
                //check if feed exists 
				IList<INewsItem> itemsForFeed = entry.Source.GetItemsForFeed(feedUrl, false);

                //find this item 
                int itemIndex = itemsForFeed.IndexOf(theItem);

                if (itemIndex != -1)
                {
                    //check if item still exists 

                    INewsItem item = itemsForFeed[itemIndex];
                    item.FlagStatus = theItem.FlagStatus;

                    FeedWasModified(entry, feedUrl, NewsFeedProperty.FeedItemFlag);
                }
            } //if(this.feedHan...)

            flaggedItemsFeed.Modified = true;
        }


        /// <summary>
        /// Get a NewsItem to unwatch: usually called on an already watched item. It try to
        /// find the corresponding feed containing the item and unwatches them.
        /// </summary>
        /// <param name="theItem">NewsItem to re-flag</param>
        public void ReWatchNewsItem(INewsItem theItem)
        {
            if (theItem == null)
                return;

			int sourceID;
			string feedUrl = OptionalItemElement.GetOriginalFeedReference(theItem, out sourceID); // the corresponding feed Url

			FeedSourceEntry entry = null;
			if (FeedSources.ContainsKey(sourceID))
				entry = FeedSources[sourceID];

			//try
			//{
			//    XmlElement elem =
			//        RssHelper.GetOptionalElement(theItem, AdditionalElements.OriginalFeedOfWatchedItem);
			//    if (elem != null)
			//    {
			//        feedUrl = elem.InnerText;
			//    }
			//}
			//catch
			//{
			//}


            //find this item in watched feeds and set watched state
            int index = watchedItemsFeed.Items.IndexOf(theItem);

            if (index != -1)
            {
                //check if item exists 

                INewsItem item = watchedItemsFeed.Items[index];
                item.WatchComments = theItem.WatchComments;
            }


            //find this item in main feed list and set watched state
            if (feedUrl != null && entry != null && entry.Source.IsSubscribed(feedUrl))
            {
                //check if feed exists 
				IList<INewsItem> itemsForFeed = entry.Source.GetItemsForFeed(feedUrl, false);

                //find this item 
                int itemIndex = itemsForFeed.IndexOf(theItem);

                if (itemIndex != -1)
                {
                    //check if item still exists 

                    INewsItem item = itemsForFeed[itemIndex];
                    item.WatchComments = theItem.WatchComments;

                    FeedWasModified(entry, feedUrl, NewsFeedProperty.FeedItemWatchComments);
                }
            } //if(this.feedHan...)

            watchedItemsFeed.Modified = true;
        }

        /// <summary>
        /// Get a NewsItem to flag and add them (Clone) to the flagged item node collection
        /// </summary>
        /// <param name="theItem">NewsItem to flag</param>
        public void FlagNewsItem(INewsItem theItem)
        {
            if (theItem == null)
                return;

            if (theItem.FlagStatus == Flagged.None || theItem.FlagStatus == Flagged.Complete)
            {
                // remove
                if (flaggedItemsFeed.Items.Contains(theItem))
                {
                    flaggedItemsFeed.Items.Remove(theItem);
                }
            }
            else
            {
                if (!flaggedItemsFeed.Items.Contains(theItem))
                {
                	FeedSourceEntry entry = FeedSources.SourceOf(theItem.Feed);
					// can we flag items in non-source'd feeds?
					if (entry == null)
						return;

					// now create a full copy (including item content)
					INewsItem flagItem = entry.Source.CopyNewsItemTo(theItem, flaggedItemsFeed);

                    //take over flag status
                    flagItem.FlagStatus = theItem.FlagStatus;

                	OptionalItemElement.AddOrReplaceOriginalFeedReference(
                		flagItem, theItem.Feed.link, entry.ID);
                	
                    flagItem.BeenRead = theItem.BeenRead;
                    flaggedItemsFeed.Add(flagItem);
                }
                else
                {
                    // re-flag:
                    //find this item 
                    int itemIndex = flaggedItemsFeed.Items.IndexOf(theItem);
                    if (itemIndex != -1)
                    {
                        //check if item still exists 
                        INewsItem flagItem = flaggedItemsFeed.Items[itemIndex];
                        flagItem.FlagStatus = theItem.FlagStatus;
                    }
                }
            }

            flaggedItemsFeed.Modified = true;
            FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemFlag);
        }

        /// <summary>
        /// Get/Sets the unread items local feed
        /// </summary>
        public LocalFeedsFeed UnreadItemsFeed
        {
            get { return unreadItemsFeed; }
            set { unreadItemsFeed = value; }
        }

        /// <summary>
        /// Gets or sets the watched items feed.
        /// </summary>
        /// <value>The watched items feed.</value>
        public LocalFeedsFeed WatchedItemsFeed
        {
            get { return watchedItemsFeed; }
            set { watchedItemsFeed = value; }
        }


        /// <summary>
        /// Updates the state of the WatchedItems based on the state of the items passed in
        /// </summary>
        /// <param name="items"></param>
        public void UpdateWatchedItems(IList<INewsItem> items)
        {
            if ((items == null) || (items.Count == 0))
                return;

            foreach (var ni in items)
            {
                if (watchedItemsFeed.Items.Contains(ni))
                {
                	int sourceID;
                	string feedUrlOfWatchedItem = OptionalItemElement.GetOriginalFeedReference(
                		ni, out sourceID);

					if (!String.IsNullOrEmpty(feedUrlOfWatchedItem) && FeedSources.ContainsKey(sourceID))
					{
						// still there, so we can update the item.
						
						watchedItemsFeed.Items.Remove(ni); //remove old copy of the NewsItem 
						FeedSourceEntry entry = FeedSources[sourceID];
						INewsItem watchedItem = entry.Source.CopyNewsItemTo(ni, watchedItemsFeed);
						OptionalItemElement.AddOrReplaceOriginalFeedReference(watchedItem, ni.Feed.link, sourceID);
						watchedItemsFeed.Add(watchedItem);
					}

					//watchedItemsFeed.Items.Remove(ni); //remove old copy of the NewsItem 

					//XmlElement originalFeed = RssHelper.CreateXmlElement(
					//    OptionalItemElement.Prefix,
					//    AdditionalElements.OriginalFeedOfWatchedItem,
					//    ni.Feed.link);

					//INewsItem watchedItem = feedHandler.CopyNewsItemTo(ni, watchedItemsFeed);

					//if (null ==
					//    RssHelper.GetOptionalElementKey(watchedItem.OptionalElements,
					//                                    AdditionalElements.OriginalFeedOfWatchedItem))
					//{
					//    watchedItem.OptionalElements.Add(AdditionalElements.OriginalFeedOfWatchedItem,
					//                                     originalFeed.OuterXml);
					//}

					//watchedItemsFeed.Add(watchedItem);
                }
            }
        }

        /// <summary>
        /// Gets a NewsItem to Watch and adds it (Clone) to the watched item node
        /// </summary>
        /// <param name="theItem">NewsItem to watch</param>
        public void WatchNewsItem(INewsItem theItem)
        {
            if (theItem == null)
                return;

            if (theItem.WatchComments == false)
            {
                // remove
                if (watchedItemsFeed.Items.Contains(theItem))
                {
                    watchedItemsFeed.Items.Remove(theItem);
                }

                if (!string.IsNullOrEmpty(theItem.CommentRssUrl) &&
                    commentFeedsHandler.IsSubscribed(theItem.CommentRssUrl))
                {
                    commentFeedsHandler.DeleteFeed(theItem.CommentRssUrl);
                    commentFeedlistModified = true;
                }
            }
            else
            {
				FeedSourceEntry entry = FeedSources.SourceOf(theItem.Feed);
				// can we watch items in non-source'd feeds?
				if (entry == null)
					return;

            	XmlElement originalSource = null;
            	XmlElement originalFeed = null; //RssHelper.CreateXmlElement(
				//    AdditionalFeedElements.CurrentPrefix,
				//    AdditionalFeedElements.OriginalFeedOfWatchedItem,
				//    theItem.Feed.link);

                if (!watchedItemsFeed.Items.Contains(theItem))
                {
                    INewsItem watchedItem = entry.Source.CopyNewsItemTo(theItem, watchedItemsFeed);

					originalFeed = OptionalItemElement.AddOrReplaceOriginalFeedReference(
						watchedItem, theItem.Feed.link, entry.ID);
                	
					
					//if (null ==
					//    RssHelper.GetOptionalElementKey(watchedItem.OptionalElements,
					//                                    AdditionalFeedElements.OriginalFeedOfWatchedItem))
					//{
					//    watchedItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfWatchedItem,
					//                                     originalFeed.OuterXml);
					//}

                    watchedItemsFeed.Add(watchedItem);
                }

                if (!string.IsNullOrEmpty(theItem.CommentRssUrl) &&
                    !commentFeedsHandler.IsSubscribed(theItem.CommentRssUrl))
                {
                    INewsFeed f = new NewsFeed
                                      {
                                          link = theItem.CommentRssUrl,
                                          title = theItem.Title,
                                          Tag = theItem.Feed,
                                          Any = new XmlElement[1]
                                      };

                    f.Any[0] = originalFeed;

                    //Always replace newsitems on disk with contents from feed. 
                    //This prevents issues when comments are deleted.
                    f.replaceitemsonrefresh = f.replaceitemsonrefreshSpecified = true;

                    // set NewsFeed new credentials
                    if (!string.IsNullOrEmpty(theItem.Feed.authUser))
                    {
                        string u = null, p = null;

                        FeedSource.GetFeedCredentials(theItem.Feed, ref u, ref p);
                        FeedSource.SetFeedCredentials(f, u, p);
                    }
                    else
                    {
                        FeedSource.SetFeedCredentials(f, null, null);
                    }

                    // add feed to backend					
                    f = commentFeedsHandler.AddFeed(f);

                    // set properties the backend requires the feed just added
                    int intIn = entry.Source.GetRefreshRate(theItem.Feed.link)/2*MilliSecsMultiplier;
                    //fetch comments twice as often as feed
                    commentFeedsHandler.SetRefreshRate(f.link, intIn);
                    commentFeedsHandler.SetMaxItemAge(f.link, new TimeSpan(365, 0, 0, 0));
                    //max item age is 1 year so we don't risk filtering out comments					
                    commentFeedlistModified = true;
                }
            }

            watchedItemsFeed.Modified = true;

            if (!string.IsNullOrEmpty(theItem.Feed.link))
            {
                FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemWatchComments);
            }
        }

        public LocalFeedsFeed SentItemsFeed
        {
            get { return sentItemsFeed; }
            set { sentItemsFeed = value; }
        }

        /// <summary>
        /// Adds the replyItem to the sent news item feed.
        /// </summary>
        /// <param name="inResponse2item">The item responded to.</param>
        /// <param name="replyItem">The reply item itself.</param>
        public void AddSentNewsItem(INewsItem inResponse2item, INewsItem replyItem)
        {
            //TODO: do use a different approach and do not overwrite the flag!	
            if (inResponse2item != null)
                inResponse2item.FlagStatus = Flagged.Reply;

            if (inResponse2item != null && replyItem != null)
            {
                // create a new one, because we could not modify the replyItem.link :(
                var newItem =
                    new NewsItem(sentItemsFeed, replyItem.Title, inResponse2item.Link, replyItem.Content,
                                 replyItem.Date, inResponse2item.Feed.title)
                        {
                            OptionalElements = (new Dictionary<XmlQualifiedName, string>(replyItem.OptionalElements))
                        };

				int sourceID = -1;
				FeedSourceEntry entry = FeedSources.SourceOf(inResponse2item.Feed);
				if (entry != null)
						sourceID = entry.ID;

            	OptionalItemElement.AddOrReplaceOriginalFeedReference(
            		newItem, inResponse2item.Feed.link, sourceID);

				//XmlQualifiedName key = AdditionalElements.OriginalFeedOfSentItem;
				//if (null == RssHelper.GetOptionalElementKey(newItem.OptionalElements, key))
				//{
				//    XmlElement element = RssHelper.CreateXmlElement(OptionalItemElement.Prefix, key,
				//                                                         inResponse2item.Feed.link);
				//    newItem.OptionalElements.Add(key, element.OuterXml);
				//}

				//key = AdditionalElements.OriginalSourceOfItem;
				//if (null == RssHelper.GetOptionalElementKey(newItem.OptionalElements, key))
				//{
				//    string sourceID = "-1";
				//    FeedSourceEntry entry = FeedSources.SourceOf(inResponse2item.Feed);
				//    if (entry != null)
				//        sourceID = entry.ID.ToString();
				//    XmlElement element = RssHelper.CreateXmlElement(OptionalItemElement.Prefix, key,
				//                                                         sourceID);
				//    newItem.OptionalElements.Add(key, element.OuterXml);
				//}

                newItem.BeenRead = false;
                sentItemsFeed.Add(newItem);

                guiMain.SentItemsNode.UpdateReadStatus();
            }
        }

        /// <summary>
        /// Adds the replyItem to the sent news item feed.
        /// </summary>
        /// <param name="postTarget">The NewsFeed posted to.</param>
        /// <param name="replyItem">The reply item itself.</param>
        public void AddSentNewsItem(INewsFeed postTarget, INewsItem replyItem)
        {
            if (postTarget != null && replyItem != null)
            {
                // create a new one, because we could not modify the replyItem.link :(
                var newItem =
                    new NewsItem(sentItemsFeed, replyItem.Title, Guid.NewGuid().ToString(), replyItem.Content,
                                 replyItem.Date, postTarget.title)
                        {
                            OptionalElements = (new Dictionary<XmlQualifiedName, string>(replyItem.OptionalElements)),
                            BeenRead = false
                        };

                sentItemsFeed.Add(newItem);

                guiMain.SentItemsNode.UpdateReadStatus();
            }
        }

        /// <summary>
        /// Get/Sets the deleted items local feed
        /// </summary>
        public LocalFeedsFeed DeletedItemsFeed
        {
            get { return deletedItemsFeed; }
            set { deletedItemsFeed = value; }
        }

        /// <summary>
        /// Gets a NewsItem to delete and add them to the deleted items feed
        /// </summary>
        /// <param name="theItem">NewsItem to delete</param>
        public void DeleteNewsItem(INewsItem theItem)
        {
            if (theItem == null)
                return;

            // remove from flagged local feed (there are copies of NewsItems)
            if (flaggedItemsFeed.Items.Contains(theItem))
            {
                flaggedItemsFeed.Items.Remove(theItem);
            }


            // add a optional element to remember the original feed container (for later restore)
        	FeedSourceEntry entry = FeedSources.SourceOf(theItem.Feed);
			if (null != theItem.Feed && entry != null)
				OptionalItemElement.AddOrReplaceOriginalFeedReference(theItem, theItem.Feed.link, entry.ID);

			//if (null != theItem.Feed &&
			//    null ==
			//    RssHelper.GetOptionalElementKey(theItem.OptionalElements,
			//                                    AdditionalElements.OriginalFeedOfDeletedItem))
			//{
			//    XmlElement originalFeed = RssHelper.CreateXmlElement(
			//        OptionalItemElement.Prefix,
			//        AdditionalElements.OriginalFeedOfDeletedItem,
			//        theItem.Feed.link);
			//    theItem.OptionalElements.Add(AdditionalElements.OriginalFeedOfDeletedItem, originalFeed.OuterXml);
			//}

            bool yetDeleted = false;
            if (!deletedItemsFeed.Items.Contains(theItem))
            {
                // add new deleted item
                deletedItemsFeed.Add(theItem);
                yetDeleted = true;
            }

        	if (entry != null)
				entry.Source.DeleteItem(theItem);

            deletedItemsFeed.Modified = true;
            FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemsDeleteUndelete);

            // remove from deleted local feed (if already there or deleted directly from the 
            // node container 'Waste basket' itself)
            if (!yetDeleted && deletedItemsFeed.Items.Contains(theItem))
            {
                deletedItemsFeed.Items.Remove(theItem);
            }
        }

        /// <summary>
        /// Gets a NewsItem and restore it. It will be removed from the deleted items feed
        /// and added back to the original container feed. 
        /// It returns the original container tree node if found and restored, else null.
        /// </summary>
        /// <param name="item">NewsItem</param>
        /// <returns>FeedTreeNodeBase</returns>
        public TreeFeedsNodeBase RestoreNewsItem(INewsItem item)
        {
            if (item == null)
                return null;

            int ownerSourceID ;
			string containerFeedUrl = OptionalItemElement.GetOriginalFeedReference(item, out ownerSourceID);
        	
        	FeedSourceEntry entry = null;
			if (FeedSources.ContainsKey(ownerSourceID))
				entry = FeedSources[ownerSourceID];

			OptionalItemElement.RemoveOriginalFeedReference(item);
			
            if (string.IsNullOrEmpty(containerFeedUrl))
            {
                containerFeedUrl = item.Feed.link;
            }

            if (string.IsNullOrEmpty(containerFeedUrl))
            {
                _log.Error("Cannot restore item: feed link missing.");
                return null;
            }

            bool foundAndRestored = false;
            TreeFeedsNodeBase feedsNode = null;

			if (item.FlagStatus != Flagged.None && item.FlagStatus != Flagged.Complete)
			{
                // it was a flagged item
                flaggedItemsFeed.Add(item);
                feedsNode = (TreeFeedsNodeBase) guiMain.FlaggedFeedsNode(item.FlagStatus);
                foundAndRestored = true;
            }
			else if (entry != null && entry.Source.IsSubscribed(containerFeedUrl))
            {
				entry.Source.RestoreDeletedItem(item);
                feedsNode = TreeHelper.FindNode(guiMain.GetSubscriptionRootNode(entry), containerFeedUrl);
                foundAndRestored = true;
            }
            else
            {
                var isFolder = TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.SmartFolders), containerFeedUrl) as ISmartFolder;
                if (null != isFolder)
                {
                    isFolder.Add(item);
                    feedsNode = (TreeFeedsNodeBase) isFolder;
                    foundAndRestored = true;
                }
            }

            if (foundAndRestored)
            {
                deletedItemsFeed.Remove(item);
                deletedItemsFeed.Modified = true;
                FeedWasModified(entry, containerFeedUrl, NewsFeedProperty.FeedItemsDeleteUndelete);
            }
            else
            {
				// deletedItemsFeed still contains item, so add previously removed reference back:
				OptionalItemElement.AddOrReplaceOriginalFeedReference(item, containerFeedUrl, ownerSourceID);
				_log.Error("Cannot restore item: container feed not found. Url was '" + containerFeedUrl + "'.");
            }

            return feedsNode;
        }

        public static void SetWorkingSet()
        {
            try
            {
                Process loProcess = Process.GetCurrentProcess();
                loProcess.MaxWorkingSet = new IntPtr(0x400000);
                loProcess.MinWorkingSet = new IntPtr(0x100000);
                //long lnValue = loProcess.WorkingSet; // see what the actual value 
            }
            catch (Exception ex)
            {
                _log.Error("SetWorkingSet caused exception.", ex);
            }
        }


        /// <summary>
        /// Publish an XML Feed error.
        /// </summary>
        /// <param name="e">XmlException to publish</param>
        /// <param name="feedLink">The errornous feed url</param>
        /// <param name="updateNodeIcon">Set to true, if you want to get the node icon reflecting the errornous state</param>
        /// <param name="entry">The feed source the feed belongs to</param>
        public void PublishXmlFeedError(Exception e, string feedLink, bool updateNodeIcon, FeedSourceEntry entry)
        {
            if (feedLink != null)
            {
                /*	Uri uri = null;
				try {
					uri = new Uri(feedLink);
				} catch (UriFormatException) {} */
                UpdateXmlFeedErrorFeed(CreateLocalFeedRequestException(e, feedLink), feedLink, updateNodeIcon, entry);
            }
        }

        /// <summary>
        /// Publish an XML Feed error.
        /// </summary>
        /// <param name="e">XmlException to publish</param>
        /// <param name="f">The errornous NewsFeed</param>
        /// <param name="updateNodeIcon">Set to true, if you want to get the node icon reflecting the errornous state</param>
        /// <param name="entry">The feed source the NewsFeed belongs to</param>
        public void PublishXmlFeedError(Exception e, NewsFeed f, bool updateNodeIcon, FeedSourceEntry entry)
        {
            if (f != null && !string.IsNullOrEmpty(f.link))
            {
                PublishXmlFeedError(e, f.link, updateNodeIcon, entry);
            }
        }

        /// <summary>
        /// Helper to create a wrapped Exception, that provides more error infos for a feed
        /// </summary>
        /// <param name="e">Exception</param>
        /// <param name="feedUrl">feed Url</param>
        /// <returns></returns>
        private FeedRequestException CreateLocalFeedRequestException(Exception e, string feedUrl)
        {
            if (feedUrl != null)
            {
                Uri uri = null;
                try
                {
                    uri = new Uri(feedUrl);
                }
                catch (UriFormatException)
                {
                }
                return new FeedRequestException(e.Message, e, feedHandler.GetFailureContext(uri));
            }

            return new FeedRequestException(e.Message, e, new Hashtable());
        }

        /// <summary>
        /// Add the exception to the local feedError feed. 
        /// Rewrite/Recreate the file on demand.
        /// </summary>
        /// <param name="e">An Exception to publish</param>
        /// <param name="resourceUri">The resource Uri that raise the exception.</param>
        /// <param name="updateNodeIcon">Set this to true, if you want to get the icon state reflecting the reported exception</param>
        /// <param name="entry">The Feed Source which the feed belongs to</param>
        private void UpdateXmlFeedErrorFeed(Exception e, string resourceUri, bool updateNodeIcon, FeedSourceEntry entry)
        {
            if (e != null)
            {
                ExceptionManager.GetInstance().Add(e, entry);

                ResourceGoneException goneex = e as ResourceGoneException ?? e.InnerException as ResourceGoneException;

                if (goneex != null && entry != null)
                {
                    INewsFeed f;
                    if (entry.Source.GetFeeds().TryGetValue(resourceUri, out f))
                        DisableFeed(f.link, entry);
                }
                else if (updateNodeIcon && resourceUri != null)
                {
                    guiMain.OnFeedUpdateFinishedWithException(resourceUri, e);
                }

                guiMain.ExceptionNode.UpdateReadStatus();
                guiMain.PopulateSmartFolder((TreeFeedsNodeBase) guiMain.ExceptionNode, false);
            }
        }

		// code mocved to FlaggedItemsFeed migrate method:
//        /// <summary>
//        /// Flags the NewsItems in the regular feeds that are currently in the flagItemList.
//        /// </summary>
//        public void InitializeFlaggedItems()
//        {
//            // as long the FlagStatus of NewsItem's wasn't persisted all the time, 
//            // we have to re-init the feed item's FlagStatus from the flagged items collection:
//            bool runSelfHealingFlagStatus = GuiSettings.GetBoolean("RunSelfHealing.FlagStatus", true);

//            foreach (NewsItem ri in flaggedItemsFeed.Items)
//            {
//                if (ri.FlagStatus == Flagged.None)
//                {
//                    // correction: older Bandit versions are not able to store flagStatus
//                    ri.FlagStatus = Flagged.FollowUp;
//                    flaggedItemsFeed.Modified = true;
//                }
//                else
//                {
//                    if (!runSelfHealingFlagStatus)
//                        continue; // done
//                }

//                // self-healing processing:

//                string feedUrl = null; // the corresponding feed Url

//                try
//                {
//                    XmlElement e = RssHelper.GetOptionalElement(ri, AdditionalElements.OriginalFeedOfFlaggedItem);
//                    if (e != null)
//                    {
//                        feedUrl = e.InnerText;
//                    }
//                }
//                catch
//                {
//                }

//                if (feedUrl != null && feedHandler.IsSubscribed(feedUrl))
//                {
//                    //check if feed exists 

//                    IList<INewsItem> itemsForFeed = feedHandler.GetItemsForFeed(feedUrl, false);

//                    //find this item 
//                    int itemIndex = itemsForFeed.IndexOf(ri);


//                    if (itemIndex != -1)
//                    {
//                        //check if item still exists 

//                        INewsItem item = itemsForFeed[itemIndex];
//                        if (item.FlagStatus != ri.FlagStatus)
//                        {
//// correction: older Bandit versions are not able to store flagStatus
//                            item.FlagStatus = ri.FlagStatus;
//                            flaggedItemsFeed.Modified = true;
//                            FeedWasModified(feedUrl, NewsFeedProperty.FeedItemFlag); // self-healing
//                        }
//                    }
//                } //if(this.feedHan...)
//            } //foreach(NewsItem ri...)

//            if (runSelfHealingFlagStatus)
//            {
//                // remember the state:
//                GuiSettings.SetProperty("RunSelfHealing.FlagStatus", false);
//            }
//        }

        /// <summary>
        /// Called on Application Exit. Close the main form and save application state (RSS Feeds).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event args.</param>
        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (guiMain != null)
            {
                autoSaveTimer.Dispose();


                _dispatcher.InvokeShutdown();

                InvokeOnGuiSync(() =>
                                    {
                                        
                                        guiMain.Close(true);
                                    });

                SaveApplicationState(true);
                guiMain = null;
            }
        }

#if !DEBUG
        private void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            _log.Error("OnThreadException() called", e.Exception);
        }
#endif

        private void OnNewsItemTransformationError(object sender, FeedExceptionEventArgs e)
        {
            PublishXmlFeedError(e.FailureException, e.FeedLink, false, null);
        }

        private void OnNewsItemFormatterStylesheetError(object sender, ExceptionEventArgs e)
        {
            _log.Error("OnNewsItemFormatterStylesheetError() called", e.FailureException);
            MessageError(String.Format(SR.ExceptionNewsItemFormatterStylesheetMessage, e.ErrorMessage,
                                       e.FailureException.Message));
        }

        private void OnNewsItemFormatterStylesheetValidationError(object sender, ExceptionEventArgs e)
        {
            _log.Error("OnNewsItemFormatterStylesheetValidationError() called", e.FailureException);
            MessageError(String.Format(SR.ExceptionNewsItemFormatterStylesheetMessage, e.ErrorMessage,
                                       e.FailureException.Message));
        }


        private void OnInternetConnectionStateChanged(INetState oldState, INetState newState)
        {
            bool offline = ((newState & INetState.Offline) > 0);
            bool connected = ((newState & INetState.Connected) > 0);
            bool internet_allowed = connected && (newState & INetState.Online) > 0;

            FeedSource.Offline = !internet_allowed;

            if (oldState != newState)
            {
                InvokeOnGui(delegate
                                {
                                    if (guiMain != null && !guiMain.IsDisposed)
                                    {
                                        guiMain.SetGuiStateINetConnected(internet_allowed);
                                        guiMain.SetTitleText(null);
                                        Mediator.SetEnabled(connected, "cmdToggleOfflineMode");
                                        if (connected)
                                            Mediator.SetChecked(offline, "cmdToggleOfflineMode");
                                        Mediator.SetEnabled(internet_allowed, "cmdAutoDiscoverFeed");
                                    }
                                });

                // notify service consumers:
                EventsHelper.Fire(InternetConnectionStateChange,
                                  this, new InternetConnectionStateChangeEventArgs(oldState, newState));
            }
        }

        private static void OnRssParserBeforeStateChange(FeedSourceBusyState oldState, FeedSourceBusyState newState,
                                                         ref bool cancel)
        {
            // move to idle states
            if (newState == FeedSourceBusyState.RefreshOneDone)
            {
                if (oldState >= FeedSourceBusyState.RefreshCategory)
                {
                    cancel = true; // not allowed. Only RefreshAllDone can switch to idle
                }
            }
            else if (newState < FeedSourceBusyState.RefreshCategory &&
                     newState != FeedSourceBusyState.Idle &&
                     oldState >= FeedSourceBusyState.RefreshCategory)
            {
                cancel = true; // not allowed. RefreshAll or Categories in progress
            }
        }

        private void OnNewsHandlerStateChanged(FeedSourceBusyState oldState, FeedSourceBusyState newState)
        {
            // move to idle states
            if (newState == FeedSourceBusyState.RefreshOneDone)
            {
                stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.Idle);
            }
            else if (newState == FeedSourceBusyState.RefreshAllDone)
            {
                stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.Idle);
            }
            else if (newState == FeedSourceBusyState.Idle)
            {
                SetGuiStateFeedbackText(String.Empty, ApplicationTrayState.NormalIdle);
            }
            else if (newState == FeedSourceBusyState.RefreshCategory)
            {
                SetGuiStateFeedbackText(SR.GUIStatusRefreshFeedsMessage, ApplicationTrayState.BusyRefreshFeeds);
            }
            else if (newState == FeedSourceBusyState.RefreshOne)
            {
                SetGuiStateFeedbackText(SR.GUIStatusLoadingFeed, ApplicationTrayState.BusyRefreshFeeds);
            }
            else if (newState == FeedSourceBusyState.RefreshAllAuto || newState == FeedSourceBusyState.RefreshAllForced)
            {
                SetGuiStateFeedbackText(SR.GUIStatusRefreshFeedsMessage, ApplicationTrayState.BusyRefreshFeeds);
            }
        }

        /// <summary>
        /// Called from the autoSaveTimer. It is re-used to probe for a 
        /// valid open internet connection...
        /// </summary>
        /// <param name="theStateObject"></param>
        private void OnAutoSave(object theStateObject)
        {
            InvokeOnGui(delegate
                            {
                                if (!guiMain.ShutdownInProgress)
                                    guiMain.DelayTask(DelayedTasks.SaveUIConfiguration);
                            });

            SaveApplicationState();
            UpdateInternetConnectionState();
        }


        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                // re-create a timer that waits three minutes , then invokes every five minutes.
                autoSaveTimer =
                    new Timer(OnAutoSave, this, 3*MilliSecsMultiplier, 5*MilliSecsMultiplier);
                UpdateInternetConnectionState(true); // get current state, takes a few msecs
            }
            else if (e.Mode == PowerModes.Suspend)
            {
                OnAutoSave(null);

                if (autoSaveTimer != null)
                {
                    autoSaveTimer.Dispose();
                    autoSaveTimer = null;
                }

                FeedSource.Offline = true;
            }
        }

        public void UpdateInternetConnectionState()
        {
            UpdateInternetConnectionState(false);
        }

        public void UpdateInternetConnectionState(bool forceFullTest)
        {
            INetState state = Utils.CurrentINetState(Proxy, forceFullTest);
            stateManager.MoveInternetConnectionStateTo(state); // raises OnInternetConnectionStateChanged() event
            //this.feedHandler.Offline = !stateManager.InternetAccessAllowed;		// reset feedHandler
        }

        /// <summary>
        /// Saves Application State: the feedlist, changed cached files, search folders, flagged items and sent items
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveApplicationState()
        {
            SaveApplicationState(false);
        }

        /// <summary>
        /// Saves Application State: the feedlist, changed cached files, search folders, flagged items and sent items
        /// </summary>
        /// <param name="appIsClosing">if set to <c>true</c> [app is closing].</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveApplicationState(bool appIsClosing)
        {
            if (guiMain == null) return;           

            /* 
			 * we handle the exit error here, because it does not make sense
			 * to provide a "Resume", "Ignore" as the global exception handler
			 * offers on exiting the program
			 * */
            try
            {
                foreach (FeedSourceEntry fse in sourceManager.Sources)
                {
                    FeedSource source = fse.Source;

                    if (feedlistModified && source != null && source.GetFeeds() != null &&
                        source.FeedsListOK)
                    {
                        try
                        {
                            source.SaveFeedList();
                        }
                        catch (Exception ex)
                        {
                            _log.Error("source::SaveFeedList() failed.", ex);
                        }
                    }
                }//foreach

                feedlistModified = false; // reset state flag

                if (flaggedItemsFeed.Modified)
                    flaggedItemsFeed.Save();

                if (sentItemsFeed.Modified)
                    sentItemsFeed.Save();

                if (deletedItemsFeed.Modified)
                    deletedItemsFeed.Save();

                if (watchedItemsFeed.Modified)
                    watchedItemsFeed.Save();

                if (commentFeedlistModified && commentFeedsHandler != null &&
                    commentFeedsHandler.GetFeeds() != null &&
                    commentFeedsHandler.FeedsListOK)
                {
                    using (var stream = new MemoryStream())
                    {
                        try
                        {
                            commentFeedsHandler.SaveFeedList(stream);
                            FileHelper.WriteStreamWithBackup(GetCommentsFeedListFileName(), stream);
                        }
                        catch (Exception ex)
                        {
                            _log.Error("commentFeedsHandler::SaveFeedList() failed.", ex);
                        }
                    }
                }


                if (trustedCertIssuesModified)
                    SaveTrustedCertificateIssues();

                SaveModifiedFeeds();

                //save search folders
                SaveSearchFolders();

                if (FeedSource.TopStoriesModified)
                    FeedSource.SaveCachedTopStoryTitles();

                // Last operation: write all changes to the search index to disk
                if (appIsClosing)
                    FeedSource.SearchHandler.StopIndexer();
            }
            catch (InvalidOperationException ioe)
            {
                PublishException(
                    new BanditApplicationException("Unexpected InvalidOperationException on SaveApplicationState()", ioe));
            }
            catch (Exception ex)
            {
                PublishException(new BanditApplicationException("Unexpected Exception on SaveApplicationState()", ex));
            }
           
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
        /// fdaction:?action=toggleread&amp;postid=id-of-post
        /// fdaction:?action=toggleflag&amp;postid=id-of-post
        /// fdaction:?action=previouspage&amp;currpage=current-page-number
        /// fdaction:?action=nextpage&amp;currpage=current-page-number
        /// From within error detail reports:
        /// fdaction:?action=navigatetofeed&amp;feedid=id-of-feed
        /// fdaction:?action=unsubscribefeed&amp;feedid=id-of-feed
        /// From within top story page:
        /// fdaction?action=markdiscussionread&amp;storyid=id-of-story
        /// </code>
        /// </remarks>
        public bool InterceptUrlNavigation(string webUrl)
        {
            if (!interceptUrlNavigation) return false;

            Uri url;

            try
            {
                url = new Uri(webUrl);
            }
            catch
            {
                // catch invalid url formats
                return false;
            }

            // first look for localhost
            if (url.IsLoopback)
            {
                bool captured = false;
				List<string> feedurls = RssLocater.UrlsFromWellknownListener(webUrl);

                foreach (string feedurl in feedurls)
                {
                    if (feedurl.ToLower().EndsWith(".opml"))
                    {
                        ImportFeeds(feedurl); //displays a dialog also
                        captured = true;
                    }
                    else
                    {
                        // assume, it is a valid feed link url
                        CmdNewFeed(defaultCategory, feedurl, null);
                        captured = true;
                    }
                }

                return captured;
            }
            else
            {
                if (url.Scheme.Equals("fdaction"))
                {
                    /* TODO: Toggle envelope and flag in newspaper on click if javascript is OFF (by user)
				 * 1. Fetch <area> whose href attribute contains URL
				 * 2. Get name of parent <map> of the <area>
				 * 3. Fetch <img> whose usemap attribute is '#' + value from Step 2
				 * 4. Swap out value of src attribute. 
				 */

                    int idIndex = webUrl.IndexOf("postid=") + 7;
                    int feedIdIndex = webUrl.IndexOf("feedid=") + 7;
                    int typeIndex = webUrl.IndexOf("pagetype=") + 9;
                    int storyIdIndex = webUrl.IndexOf("storyid=") + 8;

                    if (webUrl.IndexOf("toggleread") != -1)
                    {
                        guiMain.ToggleItemReadState(webUrl.Substring(idIndex));
                    }
                    else if (webUrl.IndexOf("toggleflag") != -1)
                    {
                        guiMain.ToggleItemFlagState(webUrl.Substring(idIndex));
                    }
                    else if (webUrl.IndexOf("toggleshare") != -1)
                    {
                        guiMain.ToggleItemShareState(webUrl.Substring(idIndex));
                    }
                    else if (webUrl.IndexOf("toggleclip") != -1)
                    {
                        guiMain.ToggleItemClipState(webUrl.Substring(idIndex));
                    }
                    else if (webUrl.IndexOf("togglewatch") != -1)
                    {
                        guiMain.ToggleItemWatchState(webUrl.Substring(idIndex));
                    }
                    else if (webUrl.IndexOf("markread") != -1)
                    {
                        guiMain.ToggleItemReadState(webUrl.Substring(idIndex), true);
                    }
                    else if (webUrl.IndexOf("previouspage") != -1)
                    {
                        guiMain.SwitchPage(webUrl.Substring(typeIndex), false);
                    }
                    else if (webUrl.IndexOf("nextpage") != -1)
                    {
                        guiMain.SwitchPage(webUrl.Substring(typeIndex), true);
                    }
                    else if (webUrl.IndexOf("markdiscussionread") != -1)
                    {
                        guiMain.MarkDiscussionAsRead(webUrl.Substring(storyIdIndex));
                    }
                    else if (webUrl.IndexOf("navigatetofeed") != -1)
                    {
                        string normalizedUrl = HtmlHelper.UrlDecode(webUrl.Substring(feedIdIndex));
                        INewsFeed f = GetFeed(normalizedUrl);
                        if (f != null)
                            guiMain.NavigateToFeed(f);
                    }
                    else if (webUrl.IndexOf("unsubscribefeed") != -1)
                    {
                        string normalizedUrl = HtmlHelper.UrlDecode(webUrl.Substring(feedIdIndex));
                        INewsFeed f = GetFeed(normalizedUrl);
                        if (f != null)
                            UnsubscribeFeed(f, false);
                    }
                    return true;
                }

                if (url.Scheme.Equals("feed"))
                {
                    CmdNewFeed(defaultCategory, RssLocater.UrlFromFeedProtocolUrl(url.ToString()), null);
                    return true;
                }

                if (url.ToString().EndsWith(".opml"))
                {
                    ImportFeeds(url.ToString());
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check for current default aggregator. If we are not the default, then display
        /// the question dialog, if we have to ask.
        /// </summary>
        public void AskAndCheckForDefaultAggregator()
        {
            try
            {
                if (!IsDefaultAggregator() &&
                    ShouldAskForDefaultAggregator &&
                    !UACManager.Denied(ElevationRequiredAction.MakeDefaultAggregator))
                {
                    using (var dialog = new AskForDefaultAggregator())
                    {
                        if (dialog.ShowDialog(guiMain) == DialogResult.OK)
                        {
                            try
                            {
                                MakeDefaultAggregator();
                            }
                            catch (SecurityException)
                            {
                                MessageInfo(String.Format(SR.SecurityExceptionCausedByRegistryAccess,
                                                          "HKEY_CLASSES_ROOT\feed"));
                            }
                            catch (Exception ex)
                            {
                                MessageError(String.Format(SR.ExceptionSettingDefaultAggregator, ex.Message));
                            }
                        }
                        ShouldAskForDefaultAggregator = !dialog.checkBoxDoNotAskAnymore.Checked;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Unexpected error on checking for default aggregator.", e);
            }

            CheckAndRegisterIEMenuExtensions();
        }

        /// <summary>
        /// Checks and init sounds events.
        /// </summary>
        internal static void CheckAndInitSoundEvents()
        {
            Win32.Registry.CheckAndInitSounds(
                Path.GetFileNameWithoutExtension(Application.ExecutablePath));
        }

        /// <summary>
        /// Detect, if the url contains the 'feed:' protocol. If so, it just remove it
        /// to prepare a valid web url.
        /// </summary>
        /// <param name="feedUrl">Url to mangle</param>
        /// <returns>Mangled Url</returns>
        public string HandleUrlFeedProtocol(string feedUrl)
        {
            //code moved to:
            return RssLocater.UrlFromFeedProtocolUrl(feedUrl);
        }


        /// <summary>
        /// Used to initialize parameters to the XSLT template that formats the feeds as HTMl. 
        /// </summary>
        /// <returns></returns>
        internal XsltArgumentList PrepareXsltArgs()
        {
            var xslArgs = new XsltArgumentList();
            xslArgs.AddParam("AppStartupPath", String.Empty, Application.StartupPath);
            xslArgs.AddParam("AppUserDataPath", String.Empty, GetUserPath());
            xslArgs.AddParam("MarkItemsAsReadWhenViewed", String.Empty, Preferences.MarkItemsAsReadWhenViewed);
            xslArgs.AddParam("LimitNewsItemsPerPage", String.Empty, Preferences.LimitNewsItemsPerPage);
            xslArgs.AddParam("LastPageNumber", String.Empty, guiMain.LastPageNumber);
            xslArgs.AddParam("CurrentPageNumber", String.Empty, guiMain.CurrentPageNumber);

            return xslArgs;
        }


        /// <summary>
        /// Uses the current defined XSLT template to format the feeds to HTML.
        /// </summary>
        /// <param name="stylesheet">The stylesheet.</param>
        /// <param name="feeds">The list of feeds to transform</param>
        /// <returns>The feeds formatted as a HTML string</returns>
        public string FormatFeeds(string stylesheet, FeedInfoList feeds)
        {
            if (!NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
            {
                NewsItemFormatter.AddXslStyleSheet(stylesheet, GetNewsItemFormatterTemplate(stylesheet));
            }

            // display channel processing:
            foreach (IFeedDetails fi in feeds)
            {
                foreach (var n in fi.ItemsList)
                {
                    DisplayingNewsChannelServices.ProcessItem(n);
                }
            }

            return NewsItemFormatter.ToHtml(stylesheet, feeds, PrepareXsltArgs());
        }


        /// <summary>
        /// Uses the current defined XSLT template to
        /// format the feed  to HTML.
        /// </summary>
        /// <param name="stylesheet">The stylesheet.</param>
        /// <param name="feed">The feed to transform</param>
        /// <returns>The feed formatted as a HTML string</returns>
        public string FormatFeed(string stylesheet, IFeedDetails feed)
        {
            if (!NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
            {
                NewsItemFormatter.AddXslStyleSheet(stylesheet, GetNewsItemFormatterTemplate(stylesheet));
            }

            // display channel processing:
            foreach (var item in feed.ItemsList)
            {
                DisplayingNewsChannelServices.ProcessItem(item);
            }

            return NewsItemFormatter.ToHtml(stylesheet, feed, PrepareXsltArgs());
        }


        /// <summary>
        /// Uses the current defined XSLT template to
        /// format the item to HTML.
        /// </summary>
        /// <param name="stylesheet">The stylesheet.</param>
        /// <param name="item">The NewsItem</param>
        /// <param name="toHighlight">To highlight.</param>
        /// <returns>The NewsItem formatted as a HTML string</returns>
        public string FormatNewsItem(string stylesheet, INewsItem item, SearchCriteriaCollection toHighlight)
        {
            if (!NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
            {
                NewsItemFormatter.AddXslStyleSheet(stylesheet, GetNewsItemFormatterTemplate(stylesheet));
            }

            // display channel processing:
            item = DisplayingNewsChannelServices.ProcessItem(item);

            if (toHighlight == null)
            {
                return NewsItemFormatter.ToHtml(stylesheet, item, PrepareXsltArgs());
            }

            var criterias = new List<SearchCriteriaString>();
            for (int i = 0; i < toHighlight.Count; i++)
            {
                // only String matches are interesting for highlighting
                var scs = toHighlight[i] as SearchCriteriaString;
                if (scs != null && scs.Match(item))
                {
                    criterias.Add(scs);
                }
            }
            if (criterias.Count > 0)
            {
                //SearchHitNewsItem shitem = item as SearchHitNewsItem; 
                var clone = new NewsItem(item.Feed, item.Title, item.Link,
                                         ApplyHighlightingTo(item.Content, criterias), item.Date,
                                         item.Subject,
                                         item.ContentType, item.OptionalElements, item.Id, item.ParentId)
                                {
                                    FeedDetails = item.FeedDetails,
                                    BeenRead = item.BeenRead
                                };
                return NewsItemFormatter.ToHtml(stylesheet, clone, PrepareXsltArgs());
            }

            return NewsItemFormatter.ToHtml(stylesheet, item, PrepareXsltArgs());
        }


        private static string ApplyHighlightingTo(string xhtml, IList<SearchCriteriaString> searchCriteriaStrings)
        {
            for (int i = 0; i < searchCriteriaStrings.Count; i++)
            {
                // only String matches are interesting for highlighting here
                SearchCriteriaString scs = searchCriteriaStrings[i];

                if (scs != null)
                {
                    switch (scs.WhatKind)
                    {
                        case StringExpressionKind.Text:

                            //match html tags
                            Match m = SearchCriteriaString.htmlRegex.Match(xhtml);

                            //strip markup 
                            string strippedxhtml = SearchCriteriaString.htmlRegex.Replace(xhtml, "$!$");

                            //replace search words     							
                            var replaceRegex =
                                new Regex("(" + EscapeRegexSpecialChars(scs.What) + ")", RegexOptions.IgnoreCase);
                            string highlightedxhtml =
                                replaceRegex.Replace(strippedxhtml,
                                                     "<span style='color:highlighttext;background:highlight'>$1</span>");

                            //Reinsert HTML
                            var sb = new StringBuilder();
                            string[] splitxhtml = SearchCriteriaString.placeholderRegex.Split(highlightedxhtml);

                            foreach (var s in splitxhtml)
                            {
                                sb.Append(s);

                                if (m.Success)
                                {
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
                            var replaceRegex2 = new Regex("(" + scs.What + ")");
                            string highlightedxhtml2 =
                                replaceRegex2.Replace(strippedxhtml2,
                                                      "<span style='color:highlighttext;background:highlight'>$1</span>");


                            //Reinsert HTML
                            var sb2 = new StringBuilder();
                            string[] splitxhtml2 = SearchCriteriaString.placeholderRegex.Split(highlightedxhtml2);

                            foreach (var s in splitxhtml2)
                            {
                                sb2.Append(s);

                                if (m2.Success)
                                {
                                    sb2.Append(m2.Value);
                                    m2 = m2.NextMatch();
                                }
                            }
                            xhtml = sb2.ToString();
                            break;

                        case StringExpressionKind.XPathExpression: /* NOTHING TO DO HERE */
                            break;
                        default:
                            break;
                    }
                }
            }

            return xhtml;
        }

        private static string EscapeRegexSpecialChars(string input)
        {
            return
                input.Replace("\\", "\\\\").Replace(".", "\\.").Replace("$", "\\?").Replace("*", "\\*").Replace("+",
                                                                                                                "\\+").
                    Replace("^", "\\^").Replace("|", "\\|").Replace("?", "\\?").Replace("(", "\\(").Replace(")", "\\)").
                    Replace("[", "\\[").Replace("]", "\\]");
        }

        ///// <summary>
        ///// Reads an app settings entry.
        ///// </summary>
        ///// <param name="name">The name of the entry.</param>
        ///// <param name="entryType">Expected Type of the entry.</param>
        ///// <param name="defaultValue">The default value.</param>
        ///// <returns>Value read or defaultValue</returns>
        //public static object ReadAppSettingsEntry(string name, Type entryType, object defaultValue)
        //{
        //    if (string.IsNullOrEmpty(name))
        //        return defaultValue;

        //    if (entryType == null)
        //        throw new ArgumentNullException("entryType");

        //    string value = ConfigurationManager.AppSettings[name];
        //    if (!string.IsNullOrEmpty(value))
        //    {
        //        if (entryType == typeof (bool))
        //        {
        //            try
        //            {
        //                return Boolean.Parse(value);
        //            }
        //            catch (FormatException)
        //            {
        //            }
        //        }
        //        else if (entryType == typeof (string))
        //        {
        //            return value;
        //        }
        //        else if (entryType.IsEnum)
        //        {
        //            try
        //            {
        //                return Enum.Parse(entryType, value, true);
        //            }
        //            catch (ArgumentException)
        //            {
        //            }
        //        }
        //        else if (entryType == typeof (Color))
        //        {
        //            Color c = Color.FromName(value);
        //            // If name is not the valid name of a pre-defined color, 
        //            // the FromName method creates a Color structure that has
        //            // an ARGB value of zero (that is, all ARGB components are 0).
        //            if (c.ToArgb() != 0)
        //                return c;
        //        }
        //        else
        //        {
        //            Trace.WriteLine("ReadAppSettingsEntry() unsupported type: " + entryType.FullName);
        //        }
        //    }
        //    return defaultValue;
        //}


        /// <summary>
        /// Loads the default stylesheet from disk and returns it as a string
        /// </summary>
        /// <returns>The XSLT stylesheet</returns>
        protected string GetNewsItemFormatterTemplate()
        {
            return GetNewsItemFormatterTemplate(Preferences.NewsItemStylesheetFile);
        }


        /// <summary>
        /// Loads the stylesheet from disk and returns it as a string
        /// </summary>
        /// <param name="stylesheet">The stylesheet name</param>
        /// <returns>The XSLT stylesheet</returns>
        protected string GetNewsItemFormatterTemplate(string stylesheet)
        {
            string s = GetTemplatesPath();
            string t = NewsItemFormatter.DefaultNewsItemTemplate;

            if (string.IsNullOrEmpty(stylesheet))
                return t;

            if (Directory.Exists(s))
            {
                string filename = Path.Combine(s, stylesheet + ".fdxsl");
                if (File.Exists(filename))
                {
                    try
                    {
                        using (var sr = new StreamReader(filename, true))
                        {
                            t = sr.ReadToEnd();
                            sr.Close();
                        }
                    }
                    catch (Exception)
                    {
                        //stylesheet probably not found
                        if (Preferences.NewsItemStylesheetFile.Equals(stylesheet))
                        {
                            Preferences.NewsItemStylesheetFile = String.Empty;
                        }
                    }
                }
                else
                {
                    // not file.exists
                    if (Preferences.NewsItemStylesheetFile.Equals(stylesheet))
                    {
                        Preferences.NewsItemStylesheetFile = String.Empty;
                    }
                }
            }
            else
            {
                // not dir.exists
                if (Preferences.NewsItemStylesheetFile.Equals(stylesheet))
                {
                    Preferences.NewsItemStylesheetFile = String.Empty;
                }
            }

            return t;
        }

        // Called from other instances of the app on startup
        public void OnOtherInstance(string[] args)
        {
            commandLineOptions.SubscribeTo.Clear();
            // parse command line...
            if (HandleCommandLineArgs(args))
            {
                CmdShowMainGui(null);

                // fix of issue https://sourceforge.net/tracker/?func=detail&atid=615248&aid=1404778&group_id=96589
                // we use now a copy of the .SubscribeTo collection to allow users clicking twice or more to
                // a "feed:uri" link while a subscription wizard window is still yet open:
                foreach (string newFeedUrl in new ArrayList(commandLineOptions.SubscribeTo))
                {
                    if (IsFormAvailable(guiMain))
                        guiMain.AddFeedUrlSynchronized(newFeedUrl);
                }
            }
        }

        public CommandLineOptions CommandLineArgs
        {
            get { return commandLineOptions; }
        }

        /// <summary>
        /// Handle command line arguments.
        /// </summary>
        /// <param name="args">Arguments string list</param>
        /// <returns>True, if all is OK, False if further processing should stop.</returns>
        public bool HandleCommandLineArgs(string[] args)
        {
            bool retVal = true;
            var commandLineParser = new CommandLineParser(typeof (CommandLineOptions));
            try
            {
                commandLineParser.Parse(args, commandLineOptions);
                if (commandLineOptions.ShowHelp)
                {
                    // show Help commandline options messagebox
                    MessageBox.Show(CaptionOnly + "\n\n" +
                                    commandLineParser.Usage,
                                    Caption + " " + "Commandline options",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false; // display only the help, then exit
                }
            }
            catch (CommandLineArgumentException e)
            {
                Splash.Close();
                // Write logo banner if parser was created successfully
                MessageBox.Show(commandLineParser.LogoBanner + e.Message,
                                Caption,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                retVal = false; // something failed
            }
            catch (ApplicationException e)
            {
                Splash.Close();
                if (e.InnerException != null && e.InnerException.Message != null)
                {
                    MessageBox.Show(e.Message + "\n\t" + e.InnerException.Message,
                                    Caption,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(e.Message,
                                    Caption,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                retVal = false; // something failed
            }
            catch (Exception e)
            {
                Splash.Close();
                // all other exceptions should have been caught
                MessageBox.Show("INTERNAL ERROR\n\t" + e.Message,
                                Caption,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                retVal = false; // something failed
            }
            return retVal;
        }

        private void SetGuiStateFeedbackText(string message)
        {
            InvokeOnGui(() => guiMain.SetGuiStateFeedback(message));
        }

        private void SetGuiStateFeedbackText(string message, ApplicationTrayState state)
        {
            InvokeOnGui(() => guiMain.SetGuiStateFeedback(message, state));
        }

        /// <summary>
        /// PostReplyForm callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="replyEventArgs"></param>
        private void OnPostReplyFormPostReply(object sender, PostReplyEventArgs replyEventArgs)
        {
            bool success = false;

            string title = replyEventArgs.Title;
            string name = replyEventArgs.FromName;
            string url = replyEventArgs.FromUrl;
            string email = replyEventArgs.FromEMail;
            string comment;

            INewsItem item2post, item2reply;
            PostReplyThreadHandler prth;

            if (replyEventArgs.ReplyToItem != null)
            {
                item2reply = replyEventArgs.ReplyToItem;
                string parentID = item2reply.Id;

                var tempDoc = new XmlDocument();

                if (replyEventArgs.Beautify)
                {
// not yet active (but in next release):
                    comment = replyEventArgs.Comment.Replace("\r\n", "<br />");
                    item2post =
                        new NewsItem(sentItemsFeed, title, url, comment, DateTime.Now, null, ContentType.Html,
                                     new Dictionary<XmlQualifiedName, string>(), url, parentID);
                }
                else
                {
                    comment = replyEventArgs.Comment;
                    item2post =
                        new NewsItem(sentItemsFeed, title, url, comment, DateTime.Now, null, null, parentID);
                }

                string commentUrl = item2reply.CommentUrl;
                item2post.FeedDetails = item2reply.FeedDetails;
                item2post.Author = (email == null) || (email.Trim().Length == 0) ? name : email + " (" + name + ")";

                /* redundancy here, because Joe Gregorio changed spec now must support both <author> and <dc:creator> */
                XmlElement emailNode = tempDoc.CreateElement("author");
                emailNode.InnerText = item2post.Author;

                item2post.OptionalElements.Add(new XmlQualifiedName("author"), emailNode.OuterXml);
                item2post.ContentType = ContentType.Html;

                prth = new PostReplyThreadHandler(feedHandler, commentUrl, item2post, item2reply);
                DialogResult result = prth.Start(postReplyForm, SR.GUIStatusPostReplyToItem);

                if (result != DialogResult.OK)
                    return;

                if (!prth.OperationSucceeds)
                {
                    MessageError(String.Format(SR.ExceptionPostReplyToNewsItem,
                                               (string.IsNullOrEmpty(item2reply.Title)
                                                    ? item2reply.Link
                                                    : item2reply.Title),
                                               prth.OperationException.Message));
                    return;
                }

                AddSentNewsItem(item2reply, item2post);
                success = true;
            }
            else if (replyEventArgs.PostToFeed != null)
            {
                INewsFeed f = replyEventArgs.PostToFeed;
                var tempDoc = new XmlDocument();

                if (replyEventArgs.Beautify)
                {
// not yet active (but in next release):
                    comment = replyEventArgs.Comment.Replace("\r\n", "<br />");
                    item2post =
                        new NewsItem(sentItemsFeed, title, url, comment, DateTime.Now, null, ContentType.Html,
                                     new Dictionary<XmlQualifiedName, string>(), url, null);
                }
                else
                {
                    comment = replyEventArgs.Comment;
                    item2post = new NewsItem(sentItemsFeed, title, url, comment, DateTime.Now, null, null, null);
                }

                item2post.CommentStyle = SupportedCommentStyle.NNTP;
                // in case the feed does not yet have downloaded items, we may get null here:
                item2post.FeedDetails = feedHandler.GetFeedDetails(f.link);
                if (item2post.FeedDetails == null)
                    item2post.FeedDetails =
                        new FeedInfo(f.id, f.cacheurl, new List<INewsItem>(0), f.title, f.link, f.title);
                item2post.Author = (email == null) || (email.Trim().Length == 0) ? name : email + " (" + name + ")";

                /* redundancy here, because Joe Gregorio changed spec now must support both <author> and <dc:creator> */
                XmlElement emailNode = tempDoc.CreateElement("author");
                emailNode.InnerText = item2post.Author;

                item2post.OptionalElements.Add(new XmlQualifiedName("author"), emailNode.OuterXml);
                item2post.ContentType = ContentType.Html;

                prth = new PostReplyThreadHandler(feedHandler, item2post, f);
                DialogResult result = prth.Start(postReplyForm, SR.GUIStatusPostNewFeedItem);

                if (result != DialogResult.OK)
                    return;

                if (!prth.OperationSucceeds)
                {
                    MessageError(String.Format(SR.ExceptionPostNewFeedItem,
                                               (string.IsNullOrEmpty(item2post.Title) ? f.link : item2post.Title),
                                               prth.OperationException.Message));
                    return;
                }

                AddSentNewsItem(f, item2post);
                success = true;
            }

            if (success)
            {
                if (postReplyForm != null)
                {
                    postReplyForm.Hide();

                    if (!postReplyForm.IsDisposed)
                    {
                        postReplyForm.Dispose();
                    }
                    postReplyForm = null;
                }
            }
            else
            {
                if (postReplyForm != null)
                {
                    postReplyForm.Show();
                    Win32.SetForegroundWindow(postReplyForm.Handle);
                }
            }
        }

        #region global app helper

		/// <summary>
		/// RSS Bandit command line parameter class
		/// </summary>
        public class CommandLineOptions
        {
			/// <summary>
			/// Initializes a new instance of the <see cref="CommandLineOptions"/> class.
			/// </summary>
			public CommandLineOptions()
			{
				// allow users to specify commandline options via app.config:
				StartInTaskbarNotificationAreaOnly = ReadAppSettingsEntry("ui.display.taskbar", false);
				LocalCulture = ReadAppSettingsEntry("ui.display.culture", String.Empty);
			}

            /// <summary>
            /// Have a look to http://blogs.gotdotnet.com/raymondc/permalink.aspx/5a811e6f-cd12-48de-8994-23409290faea,
            /// that is why we does not name it "StartInSystemTray" or such.
            /// </summary>
            [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "taskbar", ShortName = "t",
                Description = "CmdLineStartInTaskbarDesc", DescriptionIsResourceId = true)]
            public bool StartInTaskbarNotificationAreaOnly { get; set; }

            private StringCollection subscribeTo = new StringCollection();

            [DefaultCommandLineArgument(CommandLineArgumentTypes.Multiple, Name = "feedUrl",
                Description = "CmdLineSubscribeToDesc", DescriptionIsResourceId = true)]
            public StringCollection SubscribeTo
            {
                get { return subscribeTo; }
                set { subscribeTo = value; }
            }

            [CommandLineArgument(CommandLineArgumentTypes.Exclusive, Name = "help", ShortName = "h",
                Description = "CmdLineHelpDesc", DescriptionIsResourceId = true)]
            public bool ShowHelp { get; set; }

            private string localCulture = String.Empty;

            [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "culture", ShortName = "c",
                Description = "CmdLineCultureDesc", DescriptionIsResourceId = true)]
            public string LocalCulture
            {
                get { return localCulture; }
                set
                {
                    localCulture = value;
                    if (string.IsNullOrEmpty(localCulture))
                    {
                        localCulture = String.Empty;
                    }
                }
            }

            [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "resetUI", ShortName = "r",
                Description = "CmdLineResetUIDesc", DescriptionIsResourceId = true)]
            public bool ResetUserInterface { get; set; }
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
            using (var optionDialog = new PodcastOptionsDialog(Preferences, this))
            {
                optionDialog.ShowDialog(owner ?? guiMain);
                if (optionDialog.DialogResult == DialogResult.OK)
                {
                    //modify preferences with data from dialog
                    Preferences.PodcastFileExtensions = 
						FeedSource.PodcastFileExtensionsAsString = optionDialog.textPodcastFilesExtensions.Text;
					 
                    if (optionDialog.chkCopyPodcastToFolder.Checked)
                    {
						FeedSource.PodcastFolder = optionDialog.txtCopyPodcastToFolder.Text;
                    }
                    else
                    {
						FeedSource.PodcastFolder = FeedSource.EnclosureFolder;
                    }

                    Preferences.AddPodcasts2Folder = optionDialog.chkCopyPodcastToFolder.Checked;
                    Preferences.AddPodcasts2ITunes = optionDialog.chkCopyPodcastToITunesPlaylist.Checked;
                    Preferences.AddPodcasts2WMP = optionDialog.chkCopyPodcastToWMPlaylist.Checked;

                    Preferences.SinglePodcastPlaylist = optionDialog.optSinglePlaylistName.Checked;
                    Preferences.SinglePlaylistName = optionDialog.textSinglePlaylistName.Text;

                    // apply to backend, UI etc. and save:
                    ApplyPreferences();
                    SavePreferences();

                    // notify service callbacks:
                    if (optionsChangedHandler != null)
                    {
                        try
                        {
                            optionsChangedHandler.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
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
        public void ShowOptions(OptionDialogSection selectedSection, IWin32Window owner,
                                EventHandler optionsChangedHandler)
        {
            if (!SearchEngineHandler.EnginesLoaded || !SearchEngineHandler.EnginesOK)
                LoadSearchEngines();

            using (var propertiesDialog =
                new PreferencesDialog(this, CurrentGlobalRefreshRateMinutes, Preferences, searchEngines, IdentityManager)
                )
            {
                propertiesDialog.OnApplyPreferences += OnApplyPreferences;
                if (optionsChangedHandler != null)
                    propertiesDialog.OnApplyPreferences += optionsChangedHandler;

                propertiesDialog.SelectedSection = selectedSection;
                propertiesDialog.ShowDialog(owner ?? guiMain);

                if (propertiesDialog.DialogResult == DialogResult.OK)
                {
                    OnApplyPreferences(propertiesDialog, new EventArgs());
                    if (optionsChangedHandler != null)
                        optionsChangedHandler(propertiesDialog, new EventArgs());
                }

                // detach event(s) to get the dialog garbage collected:
                propertiesDialog.OnApplyPreferences -= OnApplyPreferences;
                if (optionsChangedHandler != null)
                    propertiesDialog.OnApplyPreferences -= optionsChangedHandler;

                //cleanup
            }
        }

        /// <summary>
        /// Shows the NNTP server management dialog.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="definitionChangeEventHandler">The definition change event handler.</param>
        public void ShowNntpServerManagementDialog(IWin32Window owner, EventHandler definitionChangeEventHandler)
        {
            if (definitionChangeEventHandler != null)
                NntpServerManager.NewsServerDefinitionsModified += definitionChangeEventHandler;
            NntpServerManager.ShowNewsServerSubscriptionsDialog(owner ?? guiMain);
            if (definitionChangeEventHandler != null)
                NntpServerManager.NewsServerDefinitionsModified -= definitionChangeEventHandler;
        }

        /// <summary>
        /// Shows the user identity management dialog.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="definitionChangeEventHandler">The definition change event handler.</param>
        public void ShowUserIdentityManagementDialog(IWin32Window owner, EventHandler definitionChangeEventHandler)
        {
            if (definitionChangeEventHandler != null)
                IdentityManager.IdentityDefinitionsModified += definitionChangeEventHandler;
            IdentityManager.ShowIdentityDialog(owner ?? guiMain);
            if (definitionChangeEventHandler != null)
                IdentityManager.IdentityDefinitionsModified -= definitionChangeEventHandler;
        }

        string ICoreApplication.DefaultCategory
        {
            get { return defaultCategory; }
        }

        public IEnumerable<string> GetCategories()
        {
            //list containing default category used for bootstrapping the Aggregate function
            var c = new List<string>();
            c.Add(DefaultCategory);
            IEnumerable<string> all_categories = c; 

            //get a list of the distinct categories used across all feed sources
            all_categories = FeedSources.Sources.Aggregate(all_categories, 
                                 (list, s) => list.Union(s.Source.GetCategories().Keys, StringComparer.InvariantCultureIgnoreCase));                        
            return all_categories;
        }

        /// <summary>
        /// Returns the default global Feed Refresh Rate in minutes.
        /// </summary>
        public static int DefaultGlobalRefreshRateMinutes
        {
            [DebuggerStepThrough]
            get { return FeedSource.DefaultRefreshRate/MilliSecsMultiplier; }
        }

        /// <summary>
        /// Returns the current global (specified via options)
        /// Feed Refresh Rate in minutes.
        /// </summary>
        /// <value></value>
        int ICoreApplication.CurrentGlobalRefreshRate
        {
            [DebuggerStepThrough]
            get { return CurrentGlobalRefreshRateMinutes; }
        }

        /// <summary>
        /// Returns the current global (specified via options)
        /// Feed Refresh Rate in minutes.
        /// </summary>
        /// <value></value>
        public int CurrentGlobalRefreshRateMinutes
        {
            get
            {
                if (Preferences.RefreshRate >= 0)
                    return Preferences.RefreshRate/MilliSecsMultiplier;
                return DefaultGlobalRefreshRateMinutes;
            }
            internal set
            {
                Preferences.RefreshRate = value*MilliSecsMultiplier;
                ApplyRefreshRate(Preferences.RefreshRate);
            }
        }

		void ICoreApplication.AddCategory(string category)
		{
			//TODO: change interface or map to a "current source" ?
		}

        public void AddCategory(FeedSourceEntry entry, string category)
        {
            if (category != null)
            {
                category = category.Trim();
                if (category.Length > 0 && ! entry.Source.HasCategory(category))
                {
                    var c = new category(category);
					entry.Source.AddCategory(c);
                    guiMain.CreateSubscriptionsCategoryHive(guiMain.GetSubscriptionRootNode(entry), category);
                }
            }
        }

        public bool SearchForFeeds(string searchTerm)
        {
            return SubscribeToFeed(null, null, null, searchTerm, AddSubscriptionWizardMode.SubscribeSearchDirect);
        }

        public bool SubscribeToFeed(string url, string category, string title)
        {
            AddSubscriptionWizardMode mode = AddSubscriptionWizardMode.Default;
            if (! string.IsNullOrEmpty(url))
            {
                mode = AddSubscriptionWizardMode.SubscribeURLDirect;
                if (RssHelper.IsNntpUrl(url))
                    mode = AddSubscriptionWizardMode.SubscribeNNTPGroupDirect;
            }
            return SubscribeToFeed(url, category, title, null, mode);
        }

        public bool SubscribeToFeed(string url, string category, string title, string searchTerms,
                                    AddSubscriptionWizardMode mode)
        {
            var wiz = new AddSubscriptionWizard(this, mode)
                          {
                              FeedUrl = (url ?? String.Empty),
                              FeedTitle = (title ?? String.Empty),
                              SearchTerms = (searchTerms ?? String.Empty)
                          };

            if (category != null) // does remember the last category:
                wiz.FeedCategory = category;


        	List<SubscriptionRootNode> visibleRoots = guiMain.GetVisibleSubscriptionRootNodes();
			if (visibleRoots.Count > 0)
				wiz.FeedSourceName = guiMain.FeedSourceOf(visibleRoots[0]).Name;

			try
            {
                if (MainForm.IsHandleCreated)
                    Win32.SetForegroundWindow(MainForm.Handle);
                wiz.ShowDialog(guiMain);
            }
            catch (Exception ex)
            {
                _log.Error("SubscribeToFeed caused exception.", ex);
                wiz.DialogResult = DialogResult.Cancel;
            }

            if (wiz.DialogResult == DialogResult.OK)
            {
                INewsFeed f;
				FeedSourceEntry entry = sourceManager[wiz.FeedSourceName];

                if (wiz.MultipleFeedsToSubscribe)
                {
                    bool anySubscription = false;

                    for (int i = 0; i < wiz.MultipleFeedsToSubscribeCount; i++)
                    {
                        f = CreateFeedFromWizard(wiz, entry, i);
                        if (f == null)
                        {
                            continue;
                        }

                        // add feed visually
                        guiMain.AddNewFeedNode(entry, f.category, f);

                        if (wiz.FeedInfo == null)
                            guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

                        anySubscription = true;
                    }

                    wiz.Dispose();
                    return anySubscription;
                }

                f = CreateFeedFromWizard(wiz, entry, 0);

                if (f == null)
                {
                    wiz.Dispose();
                    return false;
                }

                // add feed visually
                guiMain.AddNewFeedNode(entry, f.category, f);

                if (wiz.FeedInfo == null)
                    guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

                wiz.Dispose();
                return true;
            }

            wiz.Dispose();
            return false;
        }

        private INewsFeed CreateFeedFromWizard(AddSubscriptionWizard wiz, FeedSourceEntry entry, int index)
        {
            INewsFeed f = new NewsFeed
                              {
                                  link = wiz.FeedUrls(index)
                              };

        	
			if (entry.Source.IsSubscribed(f.link))
            {
				INewsFeed f2 = entry.Source.GetFeeds()[f.link];
                MessageInfo(String.Format(SR.GUIFieldLinkRedundantInfo,
                                          (f2.category == null
                                               ? String.Empty
                                               : f2.category + FeedSource.CategorySeparator) +
                                          f2.title, f2.link));

                return null;
            }

            f.title = wiz.FeedTitles(index);
            f.category = wiz.FeedCategory;
			if ((f.category != null) && (!entry.Source.HasCategory(f.category)))
            {
				entry.Source.AddCategory(f.category);
            }

            if (!string.IsNullOrEmpty(wiz.FeedCredentialUser))
            {
                // set NewsFeed new credentials
                string u = wiz.FeedCredentialUser, p = null;
                if (!string.IsNullOrEmpty(wiz.FeedCredentialPwd))
                    p = wiz.FeedCredentialPwd;
                FeedSource.SetFeedCredentials(f, u, p);
            }
            else
            {
                FeedSource.SetFeedCredentials(f, null, null);
            }

            f.alertEnabled = f.alertEnabledSpecified = wiz.AlertEnabled;

            // add feed to backend
			entry.Source.AddFeed(f, wiz.FeedInfo);

            FeedWasModified(f, NewsFeedProperty.FeedAdded);
            //this.FeedlistModified = true;

            // set properties the backend requires the feed yet added
            if (wiz.RefreshRate != CurrentGlobalRefreshRateMinutes)
				entry.Source.SetRefreshRate(f.link, wiz.RefreshRate);
			entry.Source.SetMaxItemAge(f.link, wiz.MaxItemAge);
			entry.Source.SetMarkItemsReadOnExit(f.link, wiz.MarkItemsReadOnExit);

            string stylesheet = wiz.FeedStylesheet;

			if (stylesheet != null && !stylesheet.Equals(entry.Source.GetStyleSheet(f.link)))
            {
				entry.Source.SetStyleSheet(f.link, stylesheet);

                if (!NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
                {
                    NewsItemFormatter.AddXslStyleSheet(stylesheet, GetNewsItemFormatterTemplate(stylesheet));
                }
            }

            return f;
        }

        /// <summary>
        /// Unsubscribes the feed by feedUrl.
        /// </summary>
        /// <param name="feedUrl">The feed URL.</param>
        /// <param name="askUser">if set to <c>true</c> [ask user].</param>
        public void UnsubscribeFeed(string feedUrl, bool askUser)
        {
            UnsubscribeFeed(GetFeed(feedUrl), askUser);
        }

        /// <summary>
        /// Unsubscribes the feed.
        /// </summary>
        /// <param name="feed">The feed.</param>
        /// <param name="askUser">if set to <c>true</c> [ask user].</param>
        public void UnsubscribeFeed(INewsFeed feed, bool askUser)
        {
            if (feed == null) return;

            var tn = (TreeFeedsNodeBase) feed.Tag;
            if (tn != null)
            {
                if (askUser)
                {
                    guiMain.CurrentSelectedFeedsNode = tn;
                    guiMain.CmdDeleteFeed(null);
                    guiMain.CurrentSelectedFeedsNode = null;
                }
                else
                {
                    guiMain.CurrentSelectedFeedsNode = tn;
                    DeleteFeed(this.sourceManager.SourceOf(feed), feed.link);
                    guiMain.CurrentSelectedFeedsNode = null;
                }
            }
        }

        public bool ContainsFeed(string address)
        {
            return feedHandler.IsSubscribed(address);
        }

        public bool TryGetFeedDetails(string url, out string category, out string title, out string link)
        {
            INewsFeed f;
            if (feedHandler.GetFeeds().TryGetValue(url, out f))
            {
                category = f.category ?? string.Empty;
                title = f.title;
                link = f.link;
                return true;
            }

            category = null;
            title = null;
            link = null;
            return false;
        }

        IDictionary ICoreApplication.Identities
        {
            get { return new ReadOnlyDictionary((IDictionary) IdentityManager.CurrentIdentities); }
        }

        IDictionary<string, INntpServerDefinition> ICoreApplication.NntpServerDefinitions
        {
            get { return NntpServerManager.CurrentNntpServers; }
        }

        IList ICoreApplication.GetNntpNewsGroups(string nntpServerName, bool forceReloadFromServer)
        {
            if (! string.IsNullOrEmpty(nntpServerName) &&
                NntpServerManager.CurrentNntpServers.ContainsKey(nntpServerName))
            {
                INntpServerDefinition sd = NntpServerManager.CurrentNntpServers[nntpServerName];
                if (sd != null)
                    return (IList) NntpServerManager.LoadNntpNewsGroups(guiMain, sd, forceReloadFromServer);
            }
            return new string[] {};
        }

        /// <summary>
        /// Gets the News Item Formatter Stylesheet list.
        /// </summary>
        /// <returns></returns>
        public IList GetItemFormatterStylesheets()
        {
            string tmplFolder = GetTemplatesPath();

            if (Directory.Exists(tmplFolder))
            {
                string[] tmplFiles = Directory.GetFiles(tmplFolder, "*.fdxsl");
                var formatters = new List<string>(tmplFiles.GetLength(0));
                foreach (var filename in tmplFiles)
                {
                    formatters.Add(Path.GetFileNameWithoutExtension(filename));
                }
                return formatters;
            }

            return new List<string>(0);
        }

        /// <summary>
        /// Gets the defined web search engines. 
        /// Items are of type ISearchEngine, keys are the corresponding Title.
        /// </summary>
        IList ICoreApplication.WebSearchEngines
        {
            get { return ArrayList.ReadOnly(SearchEngineHandler.Engines); }
        }

        bool ICoreApplication.SubscribeToFeed(string url, string category)
        {
            return SubscribeToFeed(url, category, null);
        }

        bool ICoreApplication.SubscribeToFeed(string url)
        {
            return SubscribeToFeed(url, DefaultCategory, null);
        }

        /// <summary>
        /// UI thread save navigation to an Url.
        /// </summary>
        /// <param name="url">Url to navigate to</param>
        /// <param name="tabCaption">The suggested tab caption (maybe replaced by the url's html page title)</param>
        /// <param name="forceNewTabOrWindow">Force to open a new Tab/Window</param>
        /// <param name="setFocus">Force to set the focus to the new Tab/Window</param>
        public void NavigateToUrl(string url, string tabCaption, bool forceNewTabOrWindow, bool setFocus)
        {
            InvokeOnGui(() => guiMain.DetailTabNavigateToUrl(url, tabCaption, forceNewTabOrWindow, setFocus));
        }

        /// <summary>
        /// Navigates to an provided Url on the user preferred Web Browser.
        /// So it may be the external OS Web Browser, or the internal one.
        /// </summary>
        /// <param name="url">Url to navigate to</param>
        /// <param name="tabCaption">The suggested tab caption (maybe replaced by the url's html page title)</param>
        /// <param name="forceNewTabOrWindow">Force to open a new Browser Window (Tab)</param>
        /// <param name="setFocus">Force to set the focus to the new Window (Tab)</param>
        public void NavigateToUrlAsUserPreferred(string url, string tabCaption, bool forceNewTabOrWindow, bool setFocus)
        {
            if (BrowserBehaviorOnNewWindow.OpenNewTab == Preferences.BrowserOnNewWindow)
            {
                NavigateToUrl(url, tabCaption, forceNewTabOrWindow, setFocus);
            }
            else if (BrowserBehaviorOnNewWindow.OpenDefaultBrowser == Preferences.BrowserOnNewWindow)
            {
                NavigateToUrlInExternalBrowser(url);
            }
        }

        /// <summary>
        /// Navigates to an provided Url with help of the OS system preferred Web Browser.
        /// If it fails to navigate with that browser, it falls back to internal tabbed browsing.
        /// </summary>
        /// <param name="url">Url to navigate to</param>
        public void NavigateToUrlInExternalBrowser(string url)
        {
            if (string.IsNullOrEmpty(url))
                url = "about:blank";
            try
            {
                Process.Start(url);
            }
            catch (Exception  ex)
            {
                if (MessageQuestion(String.Format(SR.ExceptionStartDefaultBrowserMessage, ex.Message, url)) ==
                    DialogResult.Yes)
                {
                    NavigateToUrl(url, "Web", true, true);
                }
            }
        }

        public void LaunchDownloadManagerWindow()
        {
            //var coords = 

            _dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
                {
                    if (_downloadManager == null)
                    {

                        _downloadManager = new DownloadManagerWindow() { Visibility = System.Windows.Visibility.Visible };
                        _downloadManager.Closed += delegate
                                                       {
                                                           _downloadManager = null;
                                                       };

                        //ElementHost.EnableModelessKeyboardInterop(_downloadManager);
                        _downloadManager.Show();
                    }
                    else
                    {
                        _downloadManager.Activate();
                    }
                }, null);
        }

        private DownloadManagerWindow _downloadManager;

        #region NewsChannel Manangement		

        /// <summary>
        /// Register a IChannelProcessor services, that works
        /// in the receiving news channel chain: the moment we requested new feeds
        /// or update feeds from the original sources. 
        /// </summary>
        /// <param name="channelProcessor">IChannelProcessor</param>
        void ICoreApplication.RegisterReceivingNewsChannelProcessor(IChannelProcessor channelProcessor)
        {
            if (channelProcessor == null)
                return;

            INewsChannel[] channels = channelProcessor.GetChannels();
            if (channels == null || channels.Length == 0)
                return;
            foreach (var channel in channels)
            {
                feedHandler.RegisterReceivingNewsChannel(channel);
            }
        }

        /// <summary>
        /// Unregister a previously registered IChannelProcessor services 
        /// and removes it from the receiving news channel processing chain.
        /// </summary>
        /// <param name="channelProcessor">IChannelProcessor</param>
        void ICoreApplication.UnregisterReceivingNewsChannelProcessor(IChannelProcessor channelProcessor)
        {
            if (channelProcessor == null)
                return;

            INewsChannel[] channels = channelProcessor.GetChannels();
            if (channels == null || channels.Length == 0)
                return;
            foreach (var channel in channels)
            {
                feedHandler.UnregisterReceivingNewsChannel(channel);
            }
        }


        /// <summary>
        /// Register a IChannelProcessor services, that works
        /// in the displaying news channel chain: the moment before we render feeds
        /// or newsitems in the detail display pane. 
        /// </summary>
        /// <param name="channelProcessor">IChannelProcessor</param>
        public void RegisterDisplayingNewsChannelProcessor(IChannelProcessor channelProcessor)
        {
            if (channelProcessor == null)
                return;

            INewsChannel[] channels = channelProcessor.GetChannels();
            if (channels == null || channels.Length == 0)
                return;
            foreach (var channel in channels)
            {
                displayingNewsChannel.RegisterNewsChannel(channel);
            }
        }

        /// <summary>
        /// Unregister a previously registered IChannelProcessor services 
        /// and removes it from the receiving news channel processing chain.
        /// </summary>
        /// <param name="channelProcessor">IChannelProcessor</param>
        public void UnregisterDisplayingNewsChannelProcessor(IChannelProcessor channelProcessor)
        {
            if (channelProcessor == null)
                return;

            INewsChannel[] channels = channelProcessor.GetChannels();
            if (channels == null || channels.Length == 0)
                return;
            foreach (var channel in channels)
            {
                displayingNewsChannel.UnregisterNewsChannel(channel);
            }
        }

        /// <summary>
        /// Gets the receiving news channel.
        /// </summary>
        /// <value>The displaying news channel services.</value>
        internal static NewsChannelServices DisplayingNewsChannelServices
        {
            get { return displayingNewsChannel; }
        }

        #endregion

        #endregion

        /// <summary>
        /// Internal accessor to the ICoreApplication interface services.
        /// </summary>
        internal ICoreApplication CoreServices
        {
            get { return this; }
        }


/*
		private void OnDialogDisposed(object sender, EventArgs e) {
			Trace.WriteLine("Dialog Disposed('" + sender.GetType().Name.ToString() + "')");
		}
*/
    } //end class RssBanditApplication

	#region to be moved to AppServices project later on!

	
	#region event args

	
	/// <summary>
	/// Event arguments class to transport feed source and simple feed infos.
	/// </summary>
	public class FeedSourceFeedUrlTitleEventArgs : FeedSourceEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedSourceFeedUrlTitleEventArgs"/> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="feedTitle">The feed title.</param>
		public FeedSourceFeedUrlTitleEventArgs(FeedSourceEntry entry, string feedUrl, string feedTitle):
			base(entry)
		{
			FeedUrl = feedUrl;
			FeedTitle = feedTitle;
		}

		/// <summary>
		/// Gets the feed URL.
		/// </summary>
		/// <value>The feed URL.</value>
		public string FeedUrl { get; private set; }
		/// <summary>
		/// Gets the feed title.
		/// </summary>
		/// <value>The feed title.</value>
		public string FeedTitle { get; private set; }
	}

	/// <summary>
	/// Event arguments class to transport feed source and feed infos.
	/// </summary>
	public class FeedSourceFeedEventArgs : FeedSourceEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedDeletedEventArgs"/> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="feed">The feed.</param>
		public FeedSourceFeedEventArgs(FeedSourceEntry entry, INewsFeed feed) :
			base(entry)
		{
			_feed = feed;
		}

		/// <summary>
		/// Gets the feed .
		/// </summary>
		public INewsFeed Feed
		{
			get { return _feed; }
		}

		private readonly INewsFeed _feed;
	}
	
	#endregion

	/// <summary>
	/// Event arguments class to inform about a feed event
	/// </summary>
	public class FeedSourceEventArgs : EventArgs
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedSourceEventArgs"/> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		public FeedSourceEventArgs(FeedSourceEntry entry)
		{
			this._feedSource = entry;
		}

		/// <summary>
		/// Gets the feed source entry.
		/// </summary>
		public FeedSourceEntry Entry
		{
			get { return _feedSource; }
		}

		private readonly FeedSourceEntry _feedSource;

	}
	

	#endregion
} //end namespace RssBandit