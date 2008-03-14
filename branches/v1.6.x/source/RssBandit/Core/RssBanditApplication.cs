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

// Uncomment the next line to enable specific UI lang. tests.
// Then modify the returned culture ISO code within I18NTestCulture struct.
// Alternativly you can also add a commandline param '-culture:"ru-RU" to
// the project properties...

//#define TEST_I18N_THISCULTURE

#region framework namespaces

#endregion

    #region external namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.ThListView;
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

    #endregion

    #region project namespaces

using RssBandit.Common;
#endregion

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
        private static readonly string versionPostfix = String.Empty; // e.g. 'beta 1' or '(CVS)'

        private static bool validationErrorOccured = false;
        private static readonly RssBanditPreferences defaultPrefs = new RssBanditPreferences();
        private static Settings guiSettings = null;
        private static readonly ServiceContainer Services = new ServiceContainer( /* no other parent, we are at top */);

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
            new FeedColumnLayout(new string[] {"Title", "Flag", "Enclosure", "Date", "Subject"},
                                 new int[] {250, 22, 22, 100, 120}, "Date", SortOrder.Descending,
                                 LayoutType.GlobalFeedLayout);

        private static readonly FeedColumnLayout DefaultCategoryColumnLayout =
            new FeedColumnLayout(new string[] {"Title", "Subject", "Date", "FeedTitle"}, new int[] {250, 120, 100, 100},
                                 "Date", SortOrder.Descending, LayoutType.GlobalCategoryLayout);

        private static readonly FeedColumnLayout DefaultSearchFolderColumnLayout =
            new FeedColumnLayout(new string[] {"Title", "Subject", "Date", "FeedTitle"}, new int[] {250, 120, 100, 100},
                                 "Date", SortOrder.Descending, LayoutType.SearchFolderLayout);

        private static readonly FeedColumnLayout DefaultSpecialFolderColumnLayout =
            new FeedColumnLayout(new string[] {"Title", "Subject", "Date", "FeedTitle"}, new int[] {250, 120, 100, 100},
                                 "Date", SortOrder.Descending, LayoutType.SpecialFeedsLayout);

        private static string defaultFeedColumnLayoutKey;
        private static string defaultCategoryColumnLayoutKey;
        private static string defaultSearchFolderColumnLayoutKey;
        private static string defaultSpecialFolderColumnLayoutKey;

        private Timer autoSaveTimer = null;
        private bool feedlistModified = false;
        private bool commentFeedlistModified = false;
        private bool trustedCertIssuesModified = false;
        private Queue modifiedFeeds = null;

        //private INetState connectionState = INetState.Invalid;	// moved to GuiStateManager

        private static readonly int MilliSecsMultiplier = 60*1000;
        private static readonly int DefaultRefreshRate = 60*MilliSecsMultiplier;
        // option stored within the feedlist.xml:
        private int refreshRate = DefaultRefreshRate;

        // other options:
        // TODO: include it in options dialog 
        private readonly bool interceptUrlNavigation = true;

        // private feeds
        private LocalFeedsFeed watchedItemsFeed;
        private LocalFeedsFeed flaggedItemsFeed;
        private LocalFeedsFeed sentItemsFeed;
        private LocalFeedsFeed deletedItemsFeed;
        private LocalFeedsFeed unreadItemsFeed;

        private CommandLineOptions commandLineOptions;
        private GuiStateManager stateManager = null;
        private AutoDiscoveredFeedsMenuHandler backgroundDiscoverFeedsHandler = null;

        //private NewsFeed flagItemsFeed;
        //private ArrayList flagItemsList;

        private FinderSearchNodes findersSearchRoot;
        private NewsItemFormatter NewsItemFormatter;

        // as defined in the installer for Product ID
        private const string applicationGuid = "9DDCC9CA-DFCD-4BF3-B069-C9660BB28848";
        private const string applicationId = "RssBandit";
        private const string applicationName = "RSS Bandit";

        private static readonly string[] applicationUpdateServiceUrls =
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
        private static readonly ILog _log = Log.GetLogger(typeof (RssBanditApplication));

        public event EventHandler PreferencesChanged;
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
            // according to http://blogs.msdn.com/shawnste/archive/2007/07/11/security-patch-breakes-some-culture-names-for-net-2-0-on-windows-xp-2003-2000.aspx
            //TR 30-Aug-2007: we do not make use of that fix. We assume,
            //the user will have installed the framework security hotfix
            //provided via windows update!
            //ApplyResourceNameFix();

            // read app.config If a key was not found, take defaults from the embedded resources
            validationUrlBase =
                (string) ReadAppSettingsEntry("validationUrlBase", typeof (string), SR.URL_FeedValidationBase);
            linkCosmosUrlBase =
                (string) ReadAppSettingsEntry("linkCosmosUrlBase", typeof (string), SR.URL_FeedLinkCosmosUrlBase);
            bugReportUrl = (string) ReadAppSettingsEntry("bugReportUrl", typeof (string), SR.URL_BugReport);
            webHelpUrl = (string) ReadAppSettingsEntry("webHelpUrl", typeof (string), SR.URL_WebHelp);
            workspaceNewsUrl = (string) ReadAppSettingsEntry("projectNewsUrl", typeof (string), SR.URL_ProjectNews);
            wikiNewsUrl = (string) ReadAppSettingsEntry("wikiWebUrl", typeof (string), SR.URL_WikiWebNews);
            forumUrl = (string) ReadAppSettingsEntry("userForumUrl", typeof (string), SR.URL_UserForum);
            projectDonationUrl =
                (string) ReadAppSettingsEntry("projectDonationUrl", typeof (string), SR.URL_ProjectDonation);
            projectDownloadUrl =
                (string) ReadAppSettingsEntry("projectDownloadUrl", typeof (string), SR.URL_ProjectDownload);

            // read advanced settings:
            unconditionalCommentRss = (bool) ReadAppSettingsEntry("UnconditionalCommentRss", typeof (bool), false);
            automaticColorSchemes = (bool) ReadAppSettingsEntry("AutomaticColorSchemes", typeof (bool), true);
            NewsHandler.SetCookies = (bool) ReadAppSettingsEntry("UseCookiesFromIE", typeof (bool), true);
            portableApplicationMode = (bool) ReadAppSettingsEntry("PortableApplicationMode", typeof (bool), false);

            // Gui Settings (Form position, layouts,...)
            guiSettings = new Settings(String.Empty);
        }

        public RssBanditApplication() 
        {
            this.commandLineOptions = new CommandLineOptions();

            InvokeOnGuiSync = delegate(Action a)
            {
                GuiInvoker.Invoke(guiMain, a);
            };

            InvokeOnGui = delegate(Action a)
            {
                GuiInvoker.InvokeAsync(guiMain, a);
            };

        }

        public void Init()
        {
            

            LoadTrustedCertificateIssues();
            AsyncWebRequest.OnCertificateIssue += this.OnRequestCertificateIssue;


            INewsComponentsConfiguration myConfig = this.CreateFeedHandlerConfiguration();
            this.feedHandler = new NewsHandler(myConfig);
            this.feedHandler.UserAgent = UserAgent;
            this.feedHandler.PodcastFileExtensionsAsString = DefaultPodcastFileExts;
            

            this.feedHandler.BeforeDownloadFeedStarted += this.BeforeDownloadFeedStarted;
            this.feedHandler.UpdateFeedsStarted += this.OnUpdateFeedsStarted;
            this.feedHandler.OnUpdatedFavicon += this.OnUpdatedFavicon;
            this.feedHandler.OnDownloadedEnclosure += this.OnDownloadedEnclosure;

            
            this.feedHandler.OnAllAsyncRequestsCompleted += this.OnAllRequestsCompleted;


            this.commentFeedsHandler = new NewsHandler(this.CreateCommentFeedHandlerConfiguration(myConfig));
            this.commentFeedsHandler.UserAgent = UserAgent;
            // not really needed here, but init:
            this.commentFeedsHandler.PodcastFileExtensionsAsString = DefaultPodcastFileExts;
            this.commentFeedsHandler.OnAllAsyncRequestsCompleted += this.OnAllCommentFeedRequestsCompleted;
            this.commentFeedsHandler.OnUpdatedFeed += this.OnUpdatedCommentFeed;

            NewsHandler.UnconditionalCommentRss = UnconditionalCommentRss;
#if DEBUG
            NewsHandler.TraceMode = true;
#endif

            this.searchEngines = new SearchEngineHandler();
            this.identityNewsServerManager = new IdentityNewsServerManager(this);
            this.addInManager = new ServiceManager();

            // Gui command handling
            this.cmdMediator = new CommandMediator();

            // Gui State handling (switch buttons, icons, etc.)
            this.stateManager = new GuiStateManager();

            this.stateManager.InternetConnectionStateMoved += this.OnInternetConnectionStateChanged;
            this.stateManager.NewsHandlerBeforeStateMove += OnRssParserBeforeStateChange;
            this.stateManager.NewsHandlerStateMoved += this.OnNewsHandlerStateChanged;

            this.Preferences = DefaultPreferences;
            this.NewsItemFormatter = new NewsItemFormatter();
            this.NewsItemFormatter.TransformError += this.OnNewsItemTransformationError;
            this.NewsItemFormatter.StylesheetError += this.OnNewsItemFormatterStylesheetError;
            this.NewsItemFormatter.StylesheetValidationError += this.OnNewsItemFormatterStylesheetValidationError;

            this.LoadPreferences();
            this.ApplyPreferences();

            this.flaggedItemsFeed = new LocalFeedsFeed(
                GetFlagItemsFileName(),
                SR.FeedNodeFlaggedFeedsCaption,
                SR.FeedNodeFlaggedFeedsDesc);

            this.watchedItemsFeed = new LocalFeedsFeed(
                GetWatchedItemsFileName(),
                SR.FeedNodeWatchedItemsCaption,
                SR.FeedNodeWatchedItemsDesc);

            this.sentItemsFeed = new LocalFeedsFeed(
                GetSentItemsFileName(),
                SR.FeedNodeSentItemsCaption,
                SR.FeedNodeSentItemsDesc);

            this.deletedItemsFeed = new LocalFeedsFeed(
                GetDeletedItemsFileName(),
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
            backgroundDiscoverFeedsHandler.OnDiscoveredFeedsSubscribe += this.OnBackgroundDiscoveredFeedsSubscribe;

            this.modifiedFeeds = new Queue(this.feedHandler.FeedsTable.Count + 11);

            // Create a timer that waits three minutes , then invokes every five minutes.
            autoSaveTimer = new Timer(this.OnAutoSave, this, 3*MilliSecsMultiplier, 5*MilliSecsMultiplier);
            // handle/listen to power save modes
            SystemEvents.PowerModeChanged += this.OnPowerModeChanged;

            // App Update Management
            RssBanditUpdateManager.OnUpdateAvailable += this.OnApplicationUpdateAvailable;

            //specify 'nntp' and 'news' URI handler
            NntpWebRequest creator = new NntpWebRequest(new Uri("http://www.example.com"));
            WebRequest.RegisterPrefix("nntp", creator);
            WebRequest.RegisterPrefix("news", creator);

            InitApplicationServices();

            // register build in channel processors:
            this.CoreServices.RegisterDisplayingNewsChannelProcessor(new DisplayingNewsChannelProcessor());
        }

        private INewsComponentsConfiguration CreateFeedHandlerConfiguration()
        {
            NewsComponentsConfiguration cfg = new NewsComponentsConfiguration();
            cfg.ApplicationID = Name;
            try
            {
                cfg.SearchIndexBehavior =
                    (SearchIndexBehavior)
                    ReadAppSettingsEntry("Lucene.SearchIndexBehavior", typeof (SearchIndexBehavior),
                                         SearchIndexBehavior.Default);
            }
            catch (Exception configException)
            {
                _log.Error("Invalid Value for SearchIndexBehavior in app.config", configException);
                cfg.SearchIndexBehavior = SearchIndexBehavior.Default;
            }
            cfg.UserApplicationDataPath = ApplicationDataFolderFromEnv;
            cfg.UserLocalApplicationDataPath = ApplicationLocalDataFolderFromEnv;
            cfg.DownloadedFilesDataPath = GetEnclosuresPath();
            cfg.CacheManager = new FileCacheManager(GetFeedFileCachePath());
            cfg.PersistedSettings = this.GuiSettings;
            return cfg;
        }

        private INewsComponentsConfiguration CreateCommentFeedHandlerConfiguration(
            INewsComponentsConfiguration configTemplate)
        {
            NewsComponentsConfiguration cfg = new NewsComponentsConfiguration();
            cfg.ApplicationID = configTemplate.ApplicationID;
            cfg.SearchIndexBehavior = SearchIndexBehavior.NoIndexing;
            cfg.UserApplicationDataPath = configTemplate.UserApplicationDataPath;
            cfg.UserLocalApplicationDataPath = configTemplate.UserLocalApplicationDataPath;
            cfg.DownloadedFilesDataPath = null; // no background downloads
            cfg.CacheManager = new FileCacheManager(GetFeedFileCachePath());
            cfg.PersistedSettings = configTemplate.PersistedSettings;
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
            top.AddService(typeof (IUserPreferences), this.Preferences);
            top.AddService(typeof (ICoreApplication), this);
            top.AddService(typeof (IAddInManager), this.addInManager);
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
          //  AppDomain.CurrentDomain.DomainUnload += OnAppDomainUnload;
           //AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;

            Application.ApplicationExit += OnApplicationExit;
            // Allow exceptions to be unhandled so they break in the debugger
#if !DEBUG
			Application.ThreadException += new ThreadExceptionEventHandler(this.OnThreadException);
#endif
            Splash.Status = SR.AppLoadStateGuiLoading;
            MainForm = guiMain = new WinGuiMain(this, initialStartupState); // interconnect

            GuiInvoker.Initialize();

            // thread results to UI serialization/sync.:
            this.threadResultManager = new ThreadResultManager(this, guiMain.ResultDispatcher);
            ThreadWorker.SynchronizingObject = this.guiMain;

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
        }

        internal void CheckAndLoadAddIns()
        {
            IEnumerable<IAddIn> addIns = this.addInManager.AddIns;

            if (addIns == null)
                return;
            foreach (IAddIn addIn in addIns)
            {
                if (addIn.AddInPackages == null || addIn.AddInPackages.Count == 0)
                    continue;
                foreach (IAddInPackage package in addIn.AddInPackages)
                {
                    try
                    {
                        package.Load(this);
                    }
                    catch (Exception ex)
                    {
                        string error = SR.AddInGeneralFailure(ex.Message, addIn.Name);
                        _log.Fatal(
                            "Failed to load IAddInPackage from AddIn: " + addIn.Name + " from '" + addIn.Location + "'",
                            ex);
                        this.MessageError(error);
                    }
                }
            }
        }

        /// <summary>
        /// Cleanup task for addins
        /// </summary>
        internal void UnloadAddIns()
        {
            IEnumerable<IAddIn> addIns = this.addInManager.AddIns;
            if (addIns == null)
                return;
            foreach (IAddIn addIn in addIns)
            {
                foreach (IAddInPackage package in addIn.AddInPackages)
                {
                    try
                    {
                        package.Unload();
                    }
                    catch (Exception ex)
                    {
                        string error = SR.AddInUnloadFailure(ex.Message, addIn.Name);
                        _log.Fatal(
                            "Failed to unload IAddInPackage from AddIn: " + addIn.Name + " from '" + addIn.Location +
                            "'", ex);
                        this.MessageError(error);
                    }
                }
            }
        }


        #endregion

        

        #region ICommandComponent related routines

        public CommandMediator Mediator
        {
            [DebuggerStepThrough]
            get
            {
                return this.cmdMediator;
            }
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

            ThreadWorker.QueueTask(task);
        }

        private static void QueueTask(ThreadWorkerTaskBase task, ThreadWorkerBase.DuplicateTaskQueued duplicate)
        {
            if (task == null) 
                throw new ArgumentNullException("task");

            ThreadWorker.QueueTask(task, duplicate);
        }

        public void BeginLoadingFeedlist()
        {
            MakeAndQueueTask(ThreadWorker.Task.LoadFeedlist, OnLoadingFeedlistProgress);

        }

        public void BeginLoadingSpecialFeeds()
        {
            MakeAndQueueTask(ThreadWorker.Task.LoadSpecialFeeds,
                             new ThreadWorkerProgressHandler(this.OnLoadingSpecialFeedsProgress));
//			ThreadWorkerProgressHandler handler = new ThreadWorkerProgressHandler(this.OnLoadingSpecialFeedsProgress);
//			ThreadWorker.StartTask(ThreadWorker.Task.LoadSpecialFeeds, this.guiMain, handler, this);
        }

        private void OnLoadingSpecialFeedsProgress(object sender, ThreadWorkerProgressArgs args)
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
                // done
                this.guiMain.PopulateTreeSpecialFeeds();
            }
        }

        public void BeginRefreshFeeds(bool forceDownload)
        {
            //if (this.InternetAccessAllowed) {
            // handled via NewsHander.Offline flag
            this.StateHandler.MoveNewsHandlerStateTo(forceDownload
                                                         ? NewsHandlerState.RefreshAllForced
                                                         : NewsHandlerState.RefreshAllAuto);
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

        public void BeginRefreshCategoryFeeds(string category, bool forceDownload)
        {
            //if (this.InternetAccessAllowed) {
            // handled via NewsHander.Offline flag
            this.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshCategory);
            MakeAndQueueTask(ThreadWorker.Task.RefreshCategoryFeeds, OnRefreshFeedsProgress, category,
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

        

        public NewsHandler FeedHandler
        {
            get
            {
                return this.feedHandler;
            }
        }

        public NewsHandler CommentFeedsHandler
        {
            get
            {
                return this.commentFeedsHandler;
            }
        }


        public RssBanditPreferences Preferences
        {
            get
            {
                return currentPrefs;
            }
            set
            {
                currentPrefs = value;
            }
        }

        public IdentityNewsServerManager IdentityManager
        {
            get
            {
                return identityNewsServerManager;
            }
        }

        public IdentityNewsServerManager NntpServerManager
        {
            get
            {
                return identityNewsServerManager;
            }
        }

        /// <summary>
        /// Notification method about a setting that was modified,
        /// relevant to the subscriptions.
        /// </summary>
        /// <param name="property">The property.</param>
        public void SubscriptionModified(NewsFeedProperty property)
        {
            HandleSubscriptionRelevantChange(property);
        }

        /// <summary>
        /// Notification method about a feed that was modified.
        /// </summary>
        /// <param name="feed">The feed.</param>
        /// <param name="property">The property.</param>
        public void FeedWasModified(NewsFeed feed, NewsFeedProperty property)
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

            if (string.IsNullOrEmpty(feedUrl))
                return;

            HandleFeedCacheRelevantChange(feedUrl, property);
            HandleIndexRelevantChange(feedUrl, property);
        }

        private void HandleSubscriptionRelevantChange(NewsFeedProperty property)
        {
            if (this.feedHandler.IsSubscriptionRelevantChange(property))
                this.feedlistModified = true;
        }

        private void HandleFeedCacheRelevantChange(string feedUrl, NewsFeedProperty property)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return;
            if (this.feedHandler.IsCacheRelevantChange(property))
            {
                // we queue up the cache file refresh requests:
                lock (modifiedFeeds)
                {
                    if (!modifiedFeeds.Contains(feedUrl))
                        modifiedFeeds.Enqueue(feedUrl);
                }
            }
        }

        private void HandleIndexRelevantChange(string feedUrl, NewsFeedProperty property)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return;
            if (this.feedHandler.SearchHandler.IsIndexRelevantChange(property))
                HandleIndexRelevantChange(GetFeed(feedUrl), property);
        }

        private void HandleIndexRelevantChange(NewsFeed feed, NewsFeedProperty property)
        {
            if (feed == null)
                return;
            if (this.feedHandler.SearchHandler.IsIndexRelevantChange(property))
            {
                if (NewsFeedProperty.FeedRemoved == (property & NewsFeedProperty.FeedRemoved))
                {
                    this.feedHandler.SearchHandler.IndexRemove(feed.id);
                    // feed added change is handled after first sucessful request of the feed:
                }
                else if (NewsFeedProperty.FeedAdded == (property & NewsFeedProperty.FeedAdded))
                {
                    this.feedHandler.SearchHandler.ReIndex(feed);
                }
            }
        }

        /// <summary>
        /// Gets the NewsFeed from FeedHandler.
        /// </summary>
        /// <param name="feedUrl">The feed URL (can be null).</param>
        /// <returns>NewsFeed if found, else null</returns>
        public NewsFeed GetFeed(string feedUrl)
        {
            if (string.IsNullOrEmpty(feedUrl))
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
        public IFeedDetails GetFeedInfo(string feedUrl)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return null;
            if (this.feedHandler.FeedsTable.ContainsKey(feedUrl))
                return this.feedHandler.GetFeedInfo(feedUrl);
            return null;
        }

        public SearchEngineHandler SearchEngineHandler
        {
            get
            {
                return searchEngines;
            }
        }

        public Settings GuiSettings
        {
            get
            {
                return guiSettings;
            }
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
            get
            {
                return stateManager;
            }
        }

        public IWebProxy Proxy
        {
            get
            {
                return this.feedHandler.Proxy;
            }
            set
            {
                this.feedHandler.Proxy = value;
            }
        }

        public ArrayList FinderList
        {
            get
            {
                return findersSearchRoot.RssFinderNodes;
            }
            set
            {
                this.findersSearchRoot.RssFinderNodes = value;
            }
        }

        #region IInternetService Members

        public event InternetConnectionStateChangeHandler InternetConnectionStateChange;

        public bool InternetAccessAllowed
        {
            get
            {
                return stateManager.InternetAccessAllowed;
            }
        }

        public bool InternetConnectionOffline
        {
            get
            {
                return stateManager.InternetConnectionOffline;
            }
        }

        public INetState InternetConnectionState
        {
            get
            {
                return stateManager.InternetConnectionState;
            }
        }

        #endregion

        /// <summary>
        /// Property CommentFeedlistModified (bool)
        /// </summary>
        public bool CommentFeedlistModified
        {
            get
            {
                return this.commentFeedlistModified;
            }
            set
            {
                this.commentFeedlistModified = value;
            }
        }

        public AutoDiscoveredFeedsMenuHandler BackgroundDiscoverFeedsHandler
        {
            get
            {
                return this.backgroundDiscoverFeedsHandler;
            }
        }

        private void OnBackgroundDiscoveredFeedsSubscribe(object sender, DiscoveredFeedsSubscribeCancelEventArgs e)
        {
            if (e.FeedsInfo.FeedLinks.Count == 1)
            {
                e.Cancel = !this.CmdNewFeed(DefaultCategory, (string) e.FeedsInfo.FeedLinks[0], e.FeedsInfo.Title);
            }
            else if (e.FeedsInfo.FeedLinks.Count > 1)
            {
                Hashtable feedUrls = new Hashtable(e.FeedsInfo.FeedLinks.Count);
                foreach (string feedUrl in e.FeedsInfo.FeedLinks)
                {
                    feedUrls.Add(feedUrl,
                                 new string[] {e.FeedsInfo.Title, String.Empty, e.FeedsInfo.SiteBaseUrl, feedUrl});
                }

                DiscoveredFeedsDialog discoveredFeedsDialog = new DiscoveredFeedsDialog(feedUrls);
                discoveredFeedsDialog.ShowDialog(guiMain);

                if (discoveredFeedsDialog.DialogResult == DialogResult.OK)
                {
                    e.Cancel = true;
                    foreach (ListViewItem feedItem in discoveredFeedsDialog.listFeeds.SelectedItems)
                    {
                        if (this.CmdNewFeed(defaultCategory, (string) feedItem.Tag, feedItem.SubItems[0].Text) &&
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
            if (feedHandler.ColumnLayouts.Count == 0)
            {
                ListViewLayout oldLayout;
                foreach (NewsFeed f in feedHandler.FeedsTable.Values)
                {
                    if (string.IsNullOrEmpty(f.listviewlayout) || f.listviewlayout.IndexOf("<") < 0)
                        continue;
                    try
                    {
                        oldLayout = ListViewLayout.CreateFromXML(f.listviewlayout);
                        FeedColumnLayout fc = new FeedColumnLayout(oldLayout.ColumnList,
                                                                   oldLayout.ColumnWidthList, oldLayout.SortByColumn,
                                                                   oldLayout.SortOrder, LayoutType.IndividualLayout);

                        if (!fc.Equals(DefaultFeedColumnLayout, true))
                        {
                            int found = feedHandler.ColumnLayouts.IndexOfSimilar(fc);
                            if (found >= 0)
                            {
                                // assign key
                                f.listviewlayout = feedHandler.ColumnLayouts.GetKey(found);
                            }
                            else
                            {
                                // add and assign key
                                f.listviewlayout = Guid.NewGuid().ToString("N");
                                feedHandler.ColumnLayouts.Add(f.listviewlayout, fc);
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

                foreach (category c in feedHandler.Categories.Values)
                {
                    if (string.IsNullOrEmpty(c.listviewlayout) || c.listviewlayout.IndexOf("<") < 0)
                        continue;
                    try
                    {
                        oldLayout = ListViewLayout.CreateFromXML(c.listviewlayout);
                        FeedColumnLayout fc = new FeedColumnLayout(oldLayout.ColumnList,
                                                                   oldLayout.ColumnWidthList, oldLayout.SortByColumn,
                                                                   oldLayout.SortOrder, LayoutType.IndividualLayout);

                        if (!fc.Equals(DefaultCategoryColumnLayout, true))
                        {
                            int found = feedHandler.ColumnLayouts.IndexOfSimilar(fc);
                            if (found >= 0)
                            {
                                // assign key
                                c.listviewlayout = feedHandler.ColumnLayouts.GetKey(found);
                            }
                            else
                            {
                                // add and assign key
                                c.listviewlayout = Guid.NewGuid().ToString("N");
                                feedHandler.ColumnLayouts.Add(c.listviewlayout, fc);
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
                feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultFeedColumnLayout);
                feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultCategoryColumnLayout);
                feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultSearchFolderColumnLayout);
                feedHandler.ColumnLayouts.Add(Guid.NewGuid().ToString("N"), DefaultSpecialFolderColumnLayout);

                if (!string.IsNullOrEmpty(feedHandler.FeedColumnLayout))
                    try
                    {
                        oldLayout = ListViewLayout.CreateFromXML(feedHandler.FeedColumnLayout);
                        FeedColumnLayout fc = new FeedColumnLayout(oldLayout.ColumnList,
                                                                   oldLayout.ColumnWidthList, oldLayout.SortByColumn,
                                                                   oldLayout.SortOrder, LayoutType.IndividualLayout);

                        if (!fc.Equals(DefaultCategoryColumnLayout, true))
                        {
                            int found = feedHandler.ColumnLayouts.IndexOfSimilar(fc);
                            if (found >= 0)
                            {
                                // assign key
                                feedHandler.FeedColumnLayout = feedHandler.ColumnLayouts.GetKey(found);
                            }
                            else
                            {
                                // add and assign key
                                feedHandler.FeedColumnLayout = Guid.NewGuid().ToString("N");
                                feedHandler.ColumnLayouts.Add(feedHandler.FeedColumnLayout, fc);
                            }
                        }
                        else
                        {
                            feedHandler.FeedColumnLayout = defaultCategoryColumnLayoutKey; // same as default
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex); /* ignore deserialization failures */
                    }

                this.feedlistModified = true; // trigger auto-save
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
            private bool _modified;

            public ListViewLayout() : this(null, null, null, SortOrder.None)
            {
            }

            public ListViewLayout(IEnumerable<string> columns, IEnumerable<int> columnWidths, string sortByColumn,
                                  SortOrder sortOrder)
            {
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

            public static ListViewLayout CreateFromXML(string xmlString)
            {
                if (xmlString != null && xmlString.Length > 0)
                {
                    XmlSerializer formatter = XmlHelper.SerializerCache.GetSerializer(typeof (ListViewLayout));
                    StringReader reader = new StringReader(xmlString);
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
                    StringWriter writer = new StringWriter();
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
                get
                {
                    return _sortByColumn;
                }
                set
                {
                    _sortByColumn = value;
                }
            }

            public SortOrder SortOrder
            {
                get
                {
                    return _sortOrder;
                }
                set
                {
                    _sortOrder = value;
                }
            }

            [XmlIgnore]
            public IList<string> Columns
            {
                get
                {
                    return _columns;
                }
                set
                {
                    if (value != null)
                        _columns = new List<string>(value);
                    else
                        _columns = new List<string>();
                }
            }

            [XmlIgnore]
            public IList<int> ColumnWidths
            {
                get
                {
                    return _columnWidths;
                }
                set
                {
                    if (value != null)
                        _columnWidths = new List<int>(value);
                    else
                        _columnWidths = new List<int>();
                }
            }

            [XmlIgnore]
            public bool Modified
            {
                get
                {
                    return _modified;
                }
                set
                {
                    _modified = value;
                }
            }

            #endregion

            [XmlArrayItem(typeof (string))]
            public List<string> ColumnList
            {
                get
                {
                    return _columns;
                }
                set
                {
                    if (value != null)
                        _columns = value;
                    else
                        _columns = new List<string>();
                }
            }

            [XmlArrayItem(typeof (int))]
            public List<int> ColumnWidthList
            {
                get
                {
                    return _columnWidths;
                }
                set
                {
                    if (value != null)
                        _columnWidths = value;
                    else
                        _columnWidths = new List<int>();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                ListViewLayout o = obj as ListViewLayout;
                if (o == null)
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
                for (int i = 0; i < this._columns.Count; i++)
                {
                    if (String.Compare(this._columns[i], o._columns[i]) != 0 ||
                        this._columnWidths[i] != o._columnWidths[i])
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
            return MessageBox.Show(MainForm, text,
                                   CaptionOnly + captionPostfix,
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
                string feedUrl;
                lock (modifiedFeeds)
                {
                    feedUrl = (string) modifiedFeeds.Dequeue();
                }
                try
                {
                    this.feedHandler.ApplyFeedModifications(feedUrl);
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
                    TextWriterTraceListener myTextListener = new
                        TextWriterTraceListener(myFile);
                    Trace.Listeners.Add(myTextListener);

                    try
                    {
                        this.SearchEngineHandler.LoadEngines(p, SearchConfigValidationCallback);
                    }
                    catch (Exception e)
                    {
                        if (!validationErrorOccured)
                        {
                            this.MessageError(SR.ExceptionLoadingSearchEnginesMessage(e.Message, errorLog));
                        }
                    }

                    if (this.SearchEngineHandler.EnginesOK)
                    {
                        // Build the menues, below
                    }
                    else if (validationErrorOccured)
                    {
                        this.MessageError(SR.ExceptionInvalidSearchEnginesMessage(errorLog));
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
                this.SearchEngineHandler.GenerateDefaultEngines();
                this.SaveSearchEngines();
            }
        }

        internal void SaveSearchEngines()
        {
            try
            {
                if (this.SearchEngineHandler != null && this.SearchEngineHandler.Engines != null &&
                    this.SearchEngineHandler.EnginesOK && this.SearchEngineHandler.Engines.Count > 0)
                {
                    string p = Path.Combine(GetSearchesPath(), @"config.xml");
                    this.SearchEngineHandler.SaveEngines(new FileStream(p, FileMode.Create));
                }
            }
            catch (InvalidOperationException ioe)
            {
                _log.Error("Unexpected Error on saving SearchEngineSettings.", ioe);
                this.MessageError(SR.ExceptionWebSearchEnginesSave(ioe.InnerException.Message));
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected Error on saving SearchEngineSettings.", ex);
                this.MessageError(SR.ExceptionWebSearchEnginesSave(ex.Message));
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
            PreferencesDialog propertiesDialog = sender as PreferencesDialog;
            if (propertiesDialog == null)
                return;

            //validate  refresh rate before setting
            try
            {
                if (!string.IsNullOrEmpty(propertiesDialog.comboRefreshRate.Text))
                {
                    this.refreshRate = Int32.Parse(propertiesDialog.comboRefreshRate.Text)*MilliSecsMultiplier;
                    feedHandler.RefreshRate = this.refreshRate;
                    this.SubscriptionModified(NewsFeedProperty.FeedRefreshRate);
                    //this.FeedlistModified = true;
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
                this.feedHandler.Stylesheet = Preferences.NewsItemStylesheetFile = propertiesDialog.comboFormatters.Text;
            }
            else
            {
                this.feedHandler.Stylesheet = Preferences.NewsItemStylesheetFile = String.Empty;
            }

            if (Preferences.MaxItemAge != propertiesDialog.MaxItemAge)
            {
                Preferences.MaxItemAge = propertiesDialog.MaxItemAge;
                this.feedHandler.MarkForDownload(); // all
				this.feedHandler.ClearAllMaxItemAgeSettings(); // all on feed/category
            }

            if (Preferences.MarkItemsReadOnExit != propertiesDialog.checkMarkItemsReadOnExit.Checked)
            {
                this.feedHandler.MarkItemsReadOnExit =
                    Preferences.MarkItemsReadOnExit = propertiesDialog.checkMarkItemsReadOnExit.Checked;
            }

            if (Preferences.UseFavicons != propertiesDialog.checkUseFavicons.Checked)
            {
                Preferences.UseFavicons = propertiesDialog.checkUseFavicons.Checked;
                try
                {
                    //reload tree view					 
                    this.guiMain.ApplyFavicons();
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

                this.SearchEngineHandler.Clear(); // reset handler, if it was wrong from a initial load failure

                foreach (SearchEngine engine in propertiesDialog.searchEngines)
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
                    this.SearchEngineHandler.Engines.Add(engine);
                }

                this.SaveSearchEngines();
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

            if (!this.feedHandler.EnclosureFolder.Equals(propertiesDialog.textEnclosureDirectory.Text))
            {
                this.feedHandler.EnclosureFolder = propertiesDialog.textEnclosureDirectory.Text;
                this.feedlistModified = true;
            }

            if (this.feedHandler.DownloadEnclosures != propertiesDialog.checkDownloadEnclosures.Checked)
            {
                this.feedHandler.DownloadEnclosures = propertiesDialog.checkDownloadEnclosures.Checked;
                this.feedlistModified = true;
            }

            if (this.feedHandler.EnclosureAlert != propertiesDialog.checkEnableEnclosureAlerts.Checked)
            {
                this.feedHandler.EnclosureAlert = propertiesDialog.checkEnableEnclosureAlerts.Checked;
                this.feedlistModified = true;
            }

            if (this.feedHandler.CreateSubfoldersForEnclosures !=
                propertiesDialog.checkDownloadCreateFolderPerFeed.Checked)
            {
                this.feedHandler.CreateSubfoldersForEnclosures =
                    propertiesDialog.checkDownloadCreateFolderPerFeed.Checked;
                this.feedlistModified = true;
            }


            if ((this.feedHandler.NumEnclosuresToDownloadOnNewFeed == Int32.MaxValue) &&
                propertiesDialog.checkOnlyDownloadLastXAttachments.Checked)
            {
                this.feedlistModified = true;
            }
            if (propertiesDialog.checkOnlyDownloadLastXAttachments.Checked)
            {
                this.feedHandler.NumEnclosuresToDownloadOnNewFeed =
                    Convert.ToInt32(propertiesDialog.numOnlyDownloadLastXAttachments.Value);
            }
            else
            {
                this.feedHandler.NumEnclosuresToDownloadOnNewFeed = Int32.MaxValue;
            }

            if ((this.feedHandler.EnclosureCacheSize == Int32.MaxValue) &&
                propertiesDialog.checkEnclosureSizeOnDiskLimited.Checked)
            {
                this.feedlistModified = true;
            }
            if (propertiesDialog.checkEnclosureSizeOnDiskLimited.Checked)
            {
                this.feedHandler.EnclosureCacheSize = Convert.ToInt32(propertiesDialog.numEnclosureCacheSize.Value);
            }
            else
            {
                this.feedHandler.EnclosureCacheSize = Int32.MaxValue;
            }


            this.ApplyPreferences();
            this.SavePreferences();
        }

        private static string[] ParseProxyBypassList(string proxyBypassString)
        {
            return ListHelper.StripEmptyEntries(proxyBypassString.Split(';', ' ', ','));
        }

        // called, to apply preferences to the NewsComponents and Gui
        internal void ApplyPreferences()
        {
            this.FeedHandler.MaxItemAge = Preferences.MaxItemAge;
            this.Proxy = CreateProxyFrom(Preferences);
            NewsHandler.BuildRelationCosmos = Preferences.BuildRelationCosmos;

            try
            {
                this.NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile,
                                                        GetNewsItemFormatterTemplate());
            }
            catch
            {
                //this.NewsItemFormatter.XslStyleSheet = NewsItemFormatter.DefaultNewsItemTemplate;				
                Preferences.NewsItemStylesheetFile = String.Empty;
                this.NewsItemFormatter.AddXslStyleSheet(Preferences.NewsItemStylesheetFile,
                                                        GetNewsItemFormatterTemplate());
            }
            this.FeedHandler.Stylesheet = Preferences.NewsItemStylesheetFile;
            Win32.ApplicationSoundsAllowed = Preferences.AllowAppEventSounds;
        }

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

                proxy.Credentials = CredentialCache.DefaultCredentials;
                (proxy).BypassProxyOnLocal = p.BypassProxyOnLocal;
                //Get rid of String.Empty in by pass list because it means bypass on all URLs
                (proxy).BypassList = ListHelper.StripEmptyEntries(p.ProxyBypassList);

                if (p.ProxyCustomCredentials)
                {
                    if (!string.IsNullOrEmpty(p.ProxyUser))
                    {
                        proxy.Credentials = NewsHandler.CreateCredentialsFrom(p.ProxyUser, p.ProxyPassword);

                        #region experimental

                        //CredentialCache credCache = new CredentialCache();
                        //credCache.Add(proxy.Address, "Basic", credentials);
                        //credCache.Add(proxy.Address, "Digest", credentials);
                        //proxy.Credentials = credCache;

                        #endregion
                    }
                }

                return proxy;
            } /* endif UseProxy */

            // default proxy init:

            // No need to do anything special for .NET 2.0:
            // http://msdn.microsoft.com/msdnmag/issues/05/08/AutomaticProxyDetection/default.aspx#S3

            return WebRequest.DefaultWebProxy;
        }

        internal void LoadPreferences()
        {
            string pName = GetPreferencesFileName();
            bool migrate = false;
            IFormatter formatter = new SoapFormatter();

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
                        RssBanditPreferences p = (RssBanditPreferences) formatter.Deserialize(stream);
                        Preferences = p;
                    }
                    catch (Exception e)
                    {
                        _log.Error("Preferences DeserializationException", e);
                    }
                }

                if (migrate)
                {
                    this.SavePreferences();
                }
            }
        }

        internal void SavePreferences()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new SoapFormatter();
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
            // check, if any migration task have to be applied:

            // v.1.2.x to 1.3.x:
            // The old (one) username, mail and referer 
            // have to be migrated from Preferences to the default UserIdentity

            // Obsolete() warnings can be ignored for that function.
            if (!string.IsNullOrEmpty(this.Preferences.UserName) &&
                string.IsNullOrEmpty(this.Preferences.UserIdentityForComments))
            {
                if (!this.feedHandler.UserIdentity.ContainsKey(this.Preferences.UserName))
                {
                    //create a UserIdentity from Prefs. properties
                    UserIdentity ui = new UserIdentity();
                    ui.Name = ui.RealName = this.Preferences.UserName;
                    ui.ResponseAddress = ui.MailAddress = this.Preferences.UserMailAddress;
                    ui.ReferrerUrl = this.Preferences.Referer;
                    this.feedHandler.UserIdentity.Add(ui.Name, ui);
                    this.feedlistModified = true;
                }
                else
                {
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

        public FinderSearchNodes FindersSearchRoot
        {
            get
            {
                return this.findersSearchRoot;
            }
            set
            {
                this.findersSearchRoot = value;
            }
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
                XmlDocument doc = new XmlDocument();

                if (File.Exists(searchfolders))
                {
                    //there should be an 'Unread Items' there
                    doc.Load(searchfolders);
                }
                else
                {
                    return;
                }

                XmlElement unreadItems =
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

            string s = this.GuiSettings.GetString("FinderNodes", null);

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
                this.SaveSearchFolders(); // save to new file
                this.GuiSettings.SetProperty("FinderNodes", null); // remove from .settings.xml
            }


            return fsn;
        }

        public void SaveSearchFolders()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof (FinderSearchNodes));
                    ser.Serialize(stream, this.findersSearchRoot);
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

            this.trustedCertIssuesModified = true;
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
                    XPathDocument doc = new XPathDocument(stream);
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

                            List<CertificateIssue> issues = new List<CertificateIssue>();

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
                                    CertificateIssue ci =
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
            using (MemoryStream stream = new MemoryStream())
            {
                XmlTextWriter writer = null;
                try
                {
                    writer = new XmlTextWriter(stream, null);
                    // Use indentation for readability.
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 2;
                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("trustedCertificateIssues");

                    lock (AsyncWebRequest.TrustedCertificateIssues)
                    {
                        foreach (string url in AsyncWebRequest.TrustedCertificateIssues.Keys)
                        {
                            ICollection trusted = (ICollection) AsyncWebRequest.TrustedCertificateIssues[url];
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
                            this.trustedCertIssuesModified = false;
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
                foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts)
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
                foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts)
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
                foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts)
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
                foreach (FeedColumnLayoutEntry e in feedHandler.ColumnLayouts)
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
            else if (feedHandler.ColumnLayouts.ContainsKey(layout))
                return feedHandler.ColumnLayouts.GetByKey(layout);
            else
            {
                // invalid key: cleanup
                feedHandler.SetFeedColumnLayout(feedUrl, null);
                feedlistModified = true;
                return GlobalFeedColumnLayout;
            }
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
            string layout;
            layout = feedHandler.GetCategoryFeedColumnLayout(category);

            if (string.IsNullOrEmpty(layout))
                return GlobalCategoryColumnLayout;
            else if (feedHandler.ColumnLayouts.ContainsKey(layout))
                return feedHandler.ColumnLayouts.GetByKey(layout);
            else
            {
                // invalid key: cleanup
                feedHandler.SetCategoryFeedColumnLayout(category, null);
                feedlistModified = true;
                return GlobalCategoryColumnLayout;
            }
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
        private static void InstallDefaultFeedList()
        {
            string oldSubslist = GetOldFeedListFileName();
            string subslist = GetFeedListFileName();

            if (!File.Exists(oldSubslist) && !File.Exists(subslist))
            {
                using (Stream s = Resource.GetStream("Resources.default-feedlist.xml"))
                {
                    FileHelper.WriteStreamWithRename(subslist, s);
                } //using
            }
        }


        /// <summary>
        /// Load the feedlist. To have exception handling and UI feedback,
        /// please call StartLoadingFeedlist().
        /// </summary>
        /// <exception cref="BanditApplicationException">On any failure</exception>
        internal void LoadFeedList()
        {
            //Load new feed file if exists 
            string p = GetFeedListFileName();
            string pOld = GetOldFeedListFileName();

            if (!File.Exists(p) && File.Exists(pOld))
            {
                if (this.MessageQuestion(SR.UpgradeFeedlistInfoText(Caption)) == DialogResult.No)
                {
                    throw new BanditApplicationException(ApplicationExceptions.FeedlistOldFormat);
                }
                File.Copy(pOld, p); // copy to be used to load from
            }
            else
            {
                //create default feed list 			
                InstallDefaultFeedList();
            }

            if (File.Exists(p))
            {
                try
                {
                    feedHandler.LoadFeedlist(p, FeedListValidationCallback);
                }
                catch (Exception e)
                {
                    if (!validationErrorOccured)
                    {
                        // set by validation callback handler
                        _log.Error("Exception on loading '" + p + "'.", e);
                        throw new BanditApplicationException(ApplicationExceptions.FeedlistOnRead, e);
                    }
                }

                if (feedHandler.FeedsListOK && guiMain != null)
                {
                    /* All right here... 	*/
                }
                else if (validationErrorOccured)
                {
                    validationErrorOccured = false;
                    throw new BanditApplicationException(ApplicationExceptions.FeedlistOnProcessContent);
                }
                else
                {
                    throw new BanditApplicationException(ApplicationExceptions.FeedlistNA);
                }
            }
            else
            {
                // no feedlist file exists:
                throw new BanditApplicationException(ApplicationExceptions.FeedlistNA);
            }

            /* Also load feedlist for watched comments */

            p = GetCommentsFeedListFileName();

            if (File.Exists(p))
            {
                try
                {
                    commentFeedsHandler.LoadFeedlist(p, CommentFeedListValidationCallback);

                    foreach (NewsFeed f in commentFeedsHandler.FeedsTable.Values)
                    {
                        if ((f.Any != null) && (f.Any.Length > 0))
                        {
                            XmlElement origin = f.Any[0];
                            string sourceFeedUrl = origin.InnerText;
                            NewsFeed sourceFeed = null;  
                            if (feedHandler.FeedsTable.TryGetValue(sourceFeedUrl, out sourceFeed))
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


        public void ImportFeeds(string fromFileOrUrl)
        {
            this.ImportFeeds(fromFileOrUrl, String.Empty);
        }

        public void ImportFeeds(string fromFileOrUrl, string selectedCategory)
        {
            ImportFeedsDialog dialog =
                new ImportFeedsDialog(fromFileOrUrl, selectedCategory, defaultCategory, feedHandler.Categories);
            try
            {
                dialog.ShowDialog(guiMain);
            }
            catch
            {
            }

            Application.DoEvents(); // give time to visualize dismiss the dialog andredraw the UI

            if (dialog.DialogResult == DialogResult.OK)
            {
                string s = dialog.FeedsUrlOrFile;
                string cat = (dialog.FeedCategory ?? String.Empty);
                if (!string.IsNullOrEmpty(s))
                {
                    Stream myStream;
                    if (File.Exists(s))
                    {
                        using (myStream = FileHelper.OpenForRead(s))
                        {
                            try
                            {
                                this.feedHandler.ImportFeedlist(myStream, cat);
                                this.SubscriptionModified(NewsFeedProperty.General);
                            }
                            catch (Exception ex)
                            {
                                this.MessageError(SR.ExceptionImportFeedlist(s, ex.Message));
                                return;
                            }
                            guiMain.SaveSubscriptionTreeState();
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
                            HttpRequestFileThreadHandler fileHandler =
                                new HttpRequestFileThreadHandler(uri.CanonicalizedUri(), this.feedHandler.Proxy);
                            DialogResult result =
                                fileHandler.Start(guiMain, SR.GUIStatusWaitMessageRequestFile(uri.CanonicalizedUri()));

                            if (result != DialogResult.OK)
                                return;

                            if (!fileHandler.OperationSucceeds)
                            {
                                this.MessageError(SR.WebExceptionOnUrlAccess(
                                                      uri.CanonicalizedUri(), fileHandler.OperationException.Message));
                                return;
                            }

                            myStream = fileHandler.ResponseStream;
                            if (myStream != null)
                            {
                                using (myStream)
                                {
                                    try
                                    {
                                        this.feedHandler.ImportFeedlist(myStream, cat);
                                        this.SubscriptionModified(NewsFeedProperty.General);
                                        //this.FeedlistModified = true;									}
                                    }
                                    catch (Exception ex)
                                    {
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

        internal void DeleteFeed(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            // was possibly an error causing feed:
            SpecialFeeds.ExceptionManager.GetInstance().RemoveFeed(url);

            NewsFeed f = this.GetFeed(url);
            if (f != null)
            {
                RaiseFeedDeleted(url, f.title);
                f.Tag = null;
                try
                {
                    this.FeedHandler.DeleteFeed(url);
                }
                catch (ApplicationException ex)
                {
                    _log.Error(String.Format("DeleteFeed({0})", url), ex);
                }
                this.FeedWasModified(f, NewsFeedProperty.FeedRemoved);
                //this.FeedlistModified = true;
            }
        }

        private void RaiseFeedDeleted(string feedUrl, string feedTitle)
        {
            if (FeedDeleted != null)
            {
                try
                {
                    FeedDeleted(this, new FeedDeletedEventArgs(feedUrl, feedTitle));
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
        public void DisableFeed(string feedUrl)
        {
            NewsFeed f = null;
            if (feedUrl != null && this.FeedHandler.FeedsTable.TryGetValue(feedUrl, out f))
            {                
                this.DisableFeed(f,TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.MyFeeds), feedUrl));
            }
        }

        /// <summary>
        /// Disable a feed (with UI update)
        /// </summary>
        /// <param name="f">NewsFeed</param>
        /// <param name="feedsNode">FeedTreeNodeBase</param>
        internal void DisableFeed(NewsFeed f, TreeFeedsNodeBase feedsNode)
        {
            if (f != null)
            {
                this.FeedHandler.DisableFeed(f.link);
                guiMain.SetSubscriptionNodeState(f, feedsNode, FeedProcessingState.Normal);
            }
        }

        /// <summary>
        /// Removes a NewsItem from a SmartFolder.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="item"></param>
        public void RemoveItemFromSmartFolder(ISmartFolder folder, NewsItem item)
        {
            if (folder == null || item == null)
                return;

            if (folder is FlaggedItemsNode)
            {
                item.FlagStatus = Flagged.None;
                this.ReFlagNewsItem(item);
            }
            else if (folder is WatchedItemsNode)
            {
                item.WatchComments = false;
                this.ReWatchNewsItem(item);
            }

            //we always remove it from the smart folder regardless of type
            folder.Remove(item);
        }

        public LocalFeedsFeed FlaggedItemsFeed
        {
            get
            {
                return flaggedItemsFeed;
            }
            set
            {
                flaggedItemsFeed = value;
            }
        }

        public void ClearFlaggedItems()
        {
            foreach (NewsItem ri in this.flaggedItemsFeed.Items)
            {
                try
                {
                    XmlQualifiedName key =
                        RssHelper.GetOptionalElementKey(ri.OptionalElements,
                                                        AdditionalFeedElements.OriginalFeedOfFlaggedItem);

                    XmlElement elem = null;
                    //find bandit:feed-url element 
                    if (null != key)
                        elem = RssHelper.GetOptionalElement(ri, key);

                    if (elem != null)
                    {
                        string feedUrl = elem.InnerText;

                        if (this.feedHandler.FeedsTable.ContainsKey(feedUrl))
                        {
                            //check if feed exists 

                            IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false);

                            //find this item 
                            int itemIndex = itemsForFeed.IndexOf(ri);


                            if (itemIndex != -1)
                            {
                                //check if item still exists 

                                NewsItem item = itemsForFeed[itemIndex];
                                item.FlagStatus = Flagged.None;
                                item.OptionalElements.Remove(AdditionalFeedElements.OriginalFeedOfFlaggedItem);
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

            this.flaggedItemsFeed.Items.Clear();
        }

        /// <summary>
        /// Get a NewsItem to re-flag: usually called on a already flagged item. It try to
        /// find the corresponding feed containing the item and re-flag them.
        /// </summary>
        /// <param name="theItem">NewsItem to re-flag</param>
        public void ReFlagNewsItem(NewsItem theItem)
        {
            if (theItem == null)
                return;

            string feedUrl = null; // the corresponding feed Url

            try
            {
                XmlElement elem =
                    RssHelper.GetOptionalElement(theItem, AdditionalFeedElements.OriginalFeedOfFlaggedItem);
                if (elem != null)
                {
                    feedUrl = elem.InnerText;
                }
            }
            catch
            {
            }

            if (theItem.FlagStatus == Flagged.None || theItem.FlagStatus == Flagged.Complete)
            {
                // remove from collection
                if (this.flaggedItemsFeed.Items.Contains(theItem))
                {
                    this.flaggedItemsFeed.Items.Remove(theItem);
                }
            }
            else
            {
                //find this item 
                int itemIndex = this.flaggedItemsFeed.Items.IndexOf(theItem);

                if (itemIndex != -1)
                {
                    //check if item exists 

                    NewsItem item = (NewsItem) this.flaggedItemsFeed.Items[itemIndex];
                    item.FlagStatus = theItem.FlagStatus;
                }
            }

            if (feedUrl != null && this.feedHandler.FeedsTable.ContainsKey(feedUrl))
            {
                //check if feed exists 

                IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false);

                //find this item 
                int itemIndex = itemsForFeed.IndexOf(theItem);

                if (itemIndex != -1)
                {
                    //check if item still exists 

                    NewsItem item = itemsForFeed[itemIndex];
                    item.FlagStatus = theItem.FlagStatus;

                    this.FeedWasModified(feedUrl, NewsFeedProperty.FeedItemFlag);
                }
            } //if(this.feedHan...)

            this.flaggedItemsFeed.Modified = true;
        }


        /// <summary>
        /// Get a NewsItem to unwatch: usually called on an already watched item. It try to
        /// find the corresponding feed containing the item and unwatches them.
        /// </summary>
        /// <param name="theItem">NewsItem to re-flag</param>
        public void ReWatchNewsItem(NewsItem theItem)
        {
            if (theItem == null)
                return;

            string feedUrl = null; // the corresponding feed Url

            try
            {
                XmlElement elem =
                    RssHelper.GetOptionalElement(theItem, AdditionalFeedElements.OriginalFeedOfWatchedItem);
                if (elem != null)
                {
                    feedUrl = elem.InnerText;
                }
            }
            catch
            {
            }


            //find this item in watched feeds and set watched state
            int index = this.watchedItemsFeed.Items.IndexOf(theItem);

            if (index != -1)
            {
                //check if item exists 

                NewsItem item = this.watchedItemsFeed.Items[index];
                item.WatchComments = theItem.WatchComments;
            }


            //find this item in main feed list and set watched state
            if (feedUrl != null && this.feedHandler.FeedsTable.ContainsKey(feedUrl))
            {
                //check if feed exists 

                IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false);

                //find this item 
                int itemIndex = itemsForFeed.IndexOf(theItem);

                if (itemIndex != -1)
                {
                    //check if item still exists 

                    NewsItem item = itemsForFeed[itemIndex];
                    item.WatchComments = theItem.WatchComments;

                    this.FeedWasModified(feedUrl, NewsFeedProperty.FeedItemWatchComments);
                }
            } //if(this.feedHan...)

            this.watchedItemsFeed.Modified = true;
        }

        /// <summary>
        /// Get a NewsItem to flag and add them (Clone) to the flagged item node collection
        /// </summary>
        /// <param name="theItem">NewsItem to flag</param>
        public void FlagNewsItem(NewsItem theItem)
        {
            if (theItem == null)
                return;

            if (theItem.FlagStatus == Flagged.None || theItem.FlagStatus == Flagged.Complete)
            {
                // remove
                if (this.flaggedItemsFeed.Items.Contains(theItem))
                {
                    this.flaggedItemsFeed.Items.Remove(theItem);
                }
            }
            else
            {
                if (!this.flaggedItemsFeed.Items.Contains(theItem))
                {
                    // now create a full copy (including item content)
                    NewsItem flagItem = this.feedHandler.CopyNewsItemTo(theItem, flaggedItemsFeed);

                    //take over flag status
                    flagItem.FlagStatus = theItem.FlagStatus;

                    if (null ==
                        RssHelper.GetOptionalElementKey(flagItem.OptionalElements,
                                                        AdditionalFeedElements.OriginalFeedOfFlaggedItem))
                    {
                        XmlElement originalFeed = RssHelper.CreateXmlElement(
                            AdditionalFeedElements.ElementPrefix,
                            AdditionalFeedElements.OriginalFeedOfFlaggedItem.Name,
                            AdditionalFeedElements.OriginalFeedOfFlaggedItem.Namespace,
                            theItem.Feed.link);
                        flagItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfFlaggedItem,
                                                      originalFeed.OuterXml);
                    }

                    flagItem.BeenRead = theItem.BeenRead;
                    this.flaggedItemsFeed.Add(flagItem);
                }
                else
                {
                    // re-flag:
                    //find this item 
                    int itemIndex = this.flaggedItemsFeed.Items.IndexOf(theItem);
                    if (itemIndex != -1)
                    {
                        //check if item still exists 
                        NewsItem flagItem = this.flaggedItemsFeed.Items[itemIndex];
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
        public LocalFeedsFeed UnreadItemsFeed
        {
            get
            {
                return unreadItemsFeed;
            }
            set
            {
                unreadItemsFeed = value;
            }
        }

        /// <summary>
        /// Gets or sets the watched items feed.
        /// </summary>
        /// <value>The watched items feed.</value>
        public LocalFeedsFeed WatchedItemsFeed
        {
            get
            {
                return watchedItemsFeed;
            }
            set
            {
                watchedItemsFeed = value;
            }
        }


        /// <summary>
        /// Updates the state of the WatchedItems based on the state of the items passed in
        /// </summary>
        /// <param name="items"></param>
        public void UpdateWatchedItems(IList<NewsItem> items)
        {
            if ((items == null) || (items.Count == 0))
                return;

            foreach (NewsItem ni in items)
            {
                if (this.watchedItemsFeed.Items.Contains(ni))
                {
                    this.watchedItemsFeed.Items.Remove(ni); //remove old copy of the NewsItem 

                    XmlElement originalFeed = RssHelper.CreateXmlElement(
                        AdditionalFeedElements.ElementPrefix,
                        AdditionalFeedElements.OriginalFeedOfWatchedItem.Name,
                        AdditionalFeedElements.OriginalFeedOfWatchedItem.Namespace,
                        ni.Feed.link);

                    NewsItem watchedItem = this.feedHandler.CopyNewsItemTo(ni, watchedItemsFeed);

                    if (null ==
                        RssHelper.GetOptionalElementKey(watchedItem.OptionalElements,
                                                        AdditionalFeedElements.OriginalFeedOfWatchedItem))
                    {
                        watchedItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfWatchedItem,
                                                         originalFeed.OuterXml);
                    }

                    this.watchedItemsFeed.Add(watchedItem);
                }
            }
        }

        /// <summary>
        /// Gets a NewsItem to Watch and adds it (Clone) to the watched item node
        /// </summary>
        /// <param name="theItem">NewsItem to watch</param>
        public void WatchNewsItem(NewsItem theItem)
        {
            if (theItem == null)
                return;

            if (theItem.WatchComments == false)
            {
                // remove
                if (this.watchedItemsFeed.Items.Contains(theItem))
                {
                    this.watchedItemsFeed.Items.Remove(theItem);
                }

                if (!string.IsNullOrEmpty(theItem.CommentRssUrl) &&
                    this.commentFeedsHandler.FeedsTable.ContainsKey(theItem.CommentRssUrl))
                {
                    this.commentFeedsHandler.DeleteFeed(theItem.CommentRssUrl);
                    this.commentFeedlistModified = true;
                }
            }
            else
            {
                XmlElement originalFeed = RssHelper.CreateXmlElement(
                    AdditionalFeedElements.ElementPrefix,
                    AdditionalFeedElements.OriginalFeedOfWatchedItem.Name,
                    AdditionalFeedElements.OriginalFeedOfWatchedItem.Namespace,
                    theItem.Feed.link);

                if (!this.watchedItemsFeed.Items.Contains(theItem))
                {
                    NewsItem watchedItem = this.feedHandler.CopyNewsItemTo(theItem, watchedItemsFeed);

                    if (null ==
                        RssHelper.GetOptionalElementKey(watchedItem.OptionalElements,
                                                        AdditionalFeedElements.OriginalFeedOfWatchedItem))
                    {
                        watchedItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfWatchedItem,
                                                         originalFeed.OuterXml);
                    }

                    this.watchedItemsFeed.Add(watchedItem);
                }

                if (!string.IsNullOrEmpty(theItem.CommentRssUrl) &&
                    !commentFeedsHandler.FeedsTable.ContainsKey(theItem.CommentRssUrl))
                {
                    NewsFeed f = new NewsFeed();

                    f.link = theItem.CommentRssUrl;
                    f.title = theItem.Title;
                    f.Tag = theItem.Feed;
                    f.Any = new XmlElement[1];
                    f.Any[0] = originalFeed;

                    //Always replace newsitems on disk with contents from feed. 
                    //This prevents issues when comments are deleted.
                    f.replaceitemsonrefresh = f.replaceitemsonrefreshSpecified = true;

                    // set NewsFeed new credentials
                    if (!string.IsNullOrEmpty(theItem.Feed.authUser))
                    {
                        string u = null, p = null;

                        NewsHandler.GetFeedCredentials(theItem.Feed, ref u, ref p);
                        NewsHandler.SetFeedCredentials(f, u, p);
                    }
                    else
                    {
                        NewsHandler.SetFeedCredentials(f, null, null);
                    }

                    // add feed to backend					
                    commentFeedsHandler.FeedsTable.Add(f.link, f);

                    // set properties the backend requires the feed just added
                    int intIn = feedHandler.GetRefreshRate(theItem.Feed.link)/2*MilliSecsMultiplier;
                        //fetch comments twice as often as feed
                    commentFeedsHandler.SetRefreshRate(f.link, intIn);
                    commentFeedsHandler.SetMaxItemAge(f.link, new TimeSpan(365, 0, 0, 0));
                        //max item age is 1 year so we don't risk filtering out comments					
                    this.commentFeedlistModified = true;
                }
            }

            this.watchedItemsFeed.Modified = true;

            if (!string.IsNullOrEmpty(theItem.Feed.link))
            {
                this.FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemWatchComments);
            }
        }

        public LocalFeedsFeed SentItemsFeed
        {
            get
            {
                return sentItemsFeed;
            }
            set
            {
                sentItemsFeed = value;
            }
        }

        /// <summary>
        /// Adds the replyItem to the sent news item feed.
        /// </summary>
        /// <param name="inResponse2item">The item responded to.</param>
        /// <param name="replyItem">The reply item itself.</param>
        public void AddSentNewsItem(NewsItem inResponse2item, NewsItem replyItem)
        {
            //TODO: do use a different approach and do not overwrite the flag!	
            if (inResponse2item != null)
                inResponse2item.FlagStatus = Flagged.Reply;

            if (inResponse2item != null && replyItem != null)
            {
                // create a new one, because we could not modify the replyItem.link :(
                NewsItem newItem =
                    new NewsItem(this.sentItemsFeed, replyItem.Title, inResponse2item.Link, replyItem.Content,
                                 replyItem.Date, inResponse2item.Feed.title);
                newItem.OptionalElements = (Hashtable) replyItem.OptionalElements.Clone();

                if (null ==
                    RssHelper.GetOptionalElementKey(newItem.OptionalElements,
                                                    AdditionalFeedElements.OriginalFeedOfFlaggedItem))
                {
                    XmlElement originalFeed = RssHelper.CreateXmlElement(AdditionalFeedElements.ElementPrefix,
                                                                         AdditionalFeedElements.
                                                                             OriginalFeedOfFlaggedItem.Name,
                                                                         AdditionalFeedElements.
                                                                             OriginalFeedOfFlaggedItem.Namespace,
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
        /// <param name="postTarget">The NewsFeed posted to.</param>
        /// <param name="replyItem">The reply item itself.</param>
        public void AddSentNewsItem(NewsFeed postTarget, NewsItem replyItem)
        {
            if (postTarget != null && replyItem != null)
            {
                // create a new one, because we could not modify the replyItem.link :(
                NewsItem newItem =
                    new NewsItem(this.sentItemsFeed, replyItem.Title, Guid.NewGuid().ToString(), replyItem.Content,
                                 replyItem.Date, postTarget.title);
                newItem.OptionalElements = (Hashtable) replyItem.OptionalElements.Clone();

                newItem.BeenRead = false;
                this.sentItemsFeed.Add(newItem);

                guiMain.SentItemsNode.UpdateReadStatus();
            }
        }

        /// <summary>
        /// Get/Sets the deleted items local feed
        /// </summary>
        public LocalFeedsFeed DeletedItemsFeed
        {
            get
            {
                return deletedItemsFeed;
            }
            set
            {
                deletedItemsFeed = value;
            }
        }

        /// <summary>
        /// Gets a NewsItem to delete and add them to the deleted items feed
        /// </summary>
        /// <param name="theItem">NewsItem to delete</param>
        public void DeleteNewsItem(NewsItem theItem)
        {
            if (theItem == null)
                return;

            // remove from flagged local feed (there are copies of NewsItems)
            if (this.flaggedItemsFeed.Items.Contains(theItem))
            {
                this.flaggedItemsFeed.Items.Remove(theItem);
            }


            // add a optional element to remember the original feed container (for later restore)
            if (null != theItem.Feed &&
                null ==
                RssHelper.GetOptionalElementKey(theItem.OptionalElements,
                                                AdditionalFeedElements.OriginalFeedOfDeletedItem))
            {
                XmlElement originalFeed = RssHelper.CreateXmlElement(
                    AdditionalFeedElements.ElementPrefix,
                    AdditionalFeedElements.OriginalFeedOfDeletedItem.Name,
                    AdditionalFeedElements.OriginalFeedOfDeletedItem.Namespace,
                    theItem.Feed.link);
                theItem.OptionalElements.Add(AdditionalFeedElements.OriginalFeedOfDeletedItem, originalFeed.OuterXml);
            }

            bool yetDeleted = false;
            if (!this.deletedItemsFeed.Items.Contains(theItem))
            {
                // add new deleted item
                this.deletedItemsFeed.Add(theItem);
                yetDeleted = true;
            }

            this.feedHandler.DeleteItem(theItem);

            this.deletedItemsFeed.Modified = true;
            this.FeedWasModified(theItem.Feed, NewsFeedProperty.FeedItemsDeleteUndelete);

            // remove from deleted local feed (if already there or deleted directly from the 
            // node container 'Waste basket' itself)
            if (!yetDeleted && this.deletedItemsFeed.Items.Contains(theItem))
            {
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
        public TreeFeedsNodeBase RestoreNewsItem(NewsItem item)
        {
            if (item == null)
                return null;

            string containerFeedUrl = null;
            XmlElement elem = RssHelper.GetOptionalElement(item, AdditionalFeedElements.OriginalFeedOfDeletedItem);
            if (null != elem)
            {
                containerFeedUrl = elem.InnerText;
                item.OptionalElements.Remove(AdditionalFeedElements.OriginalFeedOfDeletedItem);
            }

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


            if (null !=
                RssHelper.GetOptionalElementKey(item.OptionalElements, AdditionalFeedElements.OriginalFeedOfFlaggedItem))
            {
                // it was a flagged item
                this.flaggedItemsFeed.Add(item);
                feedsNode = (TreeFeedsNodeBase) guiMain.FlaggedFeedsNode(item.FlagStatus);
                foundAndRestored = true;
            }
            else if (this.FeedHandler.FeedsTable.ContainsKey(containerFeedUrl))
            {
                this.FeedHandler.RestoreDeletedItem(item);
                feedsNode = TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.MyFeeds), containerFeedUrl);
                foundAndRestored = true;
            }
            else
            {
                ISmartFolder isFolder =
                    TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.SmartFolders), containerFeedUrl) as ISmartFolder;
                if (null != isFolder)
                {
                    isFolder.Add(item);
                    feedsNode = (TreeFeedsNodeBase) isFolder;
                    foundAndRestored = true;
                }
            }

            if (foundAndRestored)
            {
                this.deletedItemsFeed.Remove(item);
                this.deletedItemsFeed.Modified = true;
                this.FeedWasModified(containerFeedUrl, NewsFeedProperty.FeedItemsDeleteUndelete);
            }
            else
            {
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
        public void PublishXmlFeedError(Exception e, string feedLink, bool updateNodeIcon)
        {
            if (feedLink != null)
            {
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
        /// <param name="f">The errornous NewsFeed</param>
        /// <param name="updateNodeIcon">Set to true, if you want to get the node icon reflecting the errornous state</param>
        public void PublishXmlFeedError(Exception e, NewsFeed f, bool updateNodeIcon)
        {
            if (f != null && !string.IsNullOrEmpty(f.link))
            {
                this.PublishXmlFeedError(e, f.link, updateNodeIcon);
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
                return new FeedRequestException(e.Message, e, this.feedHandler.GetFailureContext(uri));
            }
            else
            {
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
        private void UpdateXmlFeedErrorFeed(Exception e, string resourceUri, bool updateNodeIcon)
        {
            if (e != null)
            {
                SpecialFeeds.ExceptionManager.GetInstance().Add(e);

                ResourceGoneException goneex = e as ResourceGoneException;

                if (goneex == null)
                    goneex = e.InnerException as ResourceGoneException;

                if (goneex != null)
                {
                    NewsFeed f = null;
                    if (this.feedHandler.FeedsTable.TryGetValue(resourceUri,out f))
                        this.DisableFeed(f.link);
                }
                else if (updateNodeIcon && resourceUri != null)
                {
                    guiMain.OnFeedUpdateFinishedWithException(resourceUri, e);
                }

                guiMain.ExceptionNode.UpdateReadStatus();
                guiMain.PopulateSmartFolder((TreeFeedsNodeBase) guiMain.ExceptionNode, false);
            }
        }

        /// <summary>
        /// Flags the NewsItems in the regular feeds that are currently in the flagItemList.
        /// </summary>
        public void InitializeFlaggedItems()
        {
            // as long the FlagStatus of NewsItem's wasn't persisted all the time, 
            // we have to re-init the feed item's FlagStatus from the flagged items collection:
            bool runSelfHealingFlagStatus = this.GuiSettings.GetBoolean("RunSelfHealing.FlagStatus", true);

            foreach (NewsItem ri in this.flaggedItemsFeed.Items)
            {
                if (ri.FlagStatus == Flagged.None)
                {
                    // correction: older Bandit versions are not able to store flagStatus
                    ri.FlagStatus = Flagged.FollowUp;
                    this.flaggedItemsFeed.Modified = true;
                }
                else
                {
                    if (!runSelfHealingFlagStatus)
                        continue; // done
                }

                // self-healing processing:

                string feedUrl = null; // the corresponding feed Url

                try
                {
                    XmlElement e = RssHelper.GetOptionalElement(ri, AdditionalFeedElements.OriginalFeedOfFlaggedItem);
                    if (e != null)
                    {
                        feedUrl = e.InnerText;
                    }
                }
                catch
                {
                }

                if (feedUrl != null && this.feedHandler.FeedsTable.ContainsKey(feedUrl))
                {
                    //check if feed exists 

                    IList<NewsItem> itemsForFeed = this.feedHandler.GetItemsForFeed(feedUrl, false);

                    //find this item 
                    int itemIndex = itemsForFeed.IndexOf(ri);


                    if (itemIndex != -1)
                    {
                        //check if item still exists 

                        NewsItem item = itemsForFeed[itemIndex];
                        if (item.FlagStatus != ri.FlagStatus)
                        {
// correction: older Bandit versions are not able to store flagStatus
                            item.FlagStatus = ri.FlagStatus;
                            this.flaggedItemsFeed.Modified = true;
                            this.FeedWasModified(feedUrl, NewsFeedProperty.FeedItemFlag); // self-healing
                        }
                    }
                } //if(this.feedHan...)
            } //foreach(NewsItem ri...)

            if (runSelfHealingFlagStatus)
            {
                // remember the state:
                this.GuiSettings.SetProperty("RunSelfHealing.FlagStatus", false);
            }
        }

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

                InvokeOnGuiSync(delegate
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
            this.PublishXmlFeedError(e.FailureException, e.FeedLink, false);
        }

        private void OnNewsItemFormatterStylesheetError(object sender, ExceptionEventArgs e)
        {
            _log.Error("OnNewsItemFormatterStylesheetError() called", e.FailureException);
            this.MessageError(SR.ExceptionNewsItemFormatterStylesheetMessage(e.ErrorMessage, e.FailureException.Message));
        }

        private void OnNewsItemFormatterStylesheetValidationError(object sender, ExceptionEventArgs e)
        {
            _log.Error("OnNewsItemFormatterStylesheetValidationError() called", e.FailureException);
            this.MessageError(SR.ExceptionNewsItemFormatterStylesheetMessage(e.ErrorMessage, e.FailureException.Message));
        }


        private void OnInternetConnectionStateChanged(INetState oldState, INetState newState)
        {
            bool offline = ((newState & INetState.Offline) > 0);
            bool connected = ((newState & INetState.Connected) > 0);
            bool internet_allowed = connected && (newState & INetState.Online) > 0;

            this.feedHandler.Offline = !internet_allowed;

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

        private static void OnRssParserBeforeStateChange(NewsHandlerState oldState, NewsHandlerState newState, ref bool cancel)
        {
            // move to idle states
            if (newState == NewsHandlerState.RefreshOneDone)
            {
                if (oldState >= NewsHandlerState.RefreshCategory)
                {
                    cancel = true; // not allowed. Only RefreshAllDone can switch to idle
                }
            }
            else if (newState < NewsHandlerState.RefreshCategory &&
                     newState != NewsHandlerState.Idle &&
                     oldState >= NewsHandlerState.RefreshCategory)
            {
                cancel = true; // not allowed. RefreshAll or Categories in progress
            }
        }

        private void OnNewsHandlerStateChanged(NewsHandlerState oldState, NewsHandlerState newState)
        {
            // move to idle states
            if (newState == NewsHandlerState.RefreshOneDone)
            {
                stateManager.MoveNewsHandlerStateTo(NewsHandlerState.Idle);
            }
            else if (newState == NewsHandlerState.RefreshAllDone)
            {
                stateManager.MoveNewsHandlerStateTo(NewsHandlerState.Idle);
            }
            else if (newState == NewsHandlerState.Idle)
            {
                this.SetGuiStateFeedbackText(String.Empty, ApplicationTrayState.NormalIdle);
            }
            else if (newState == NewsHandlerState.RefreshCategory)
            {
                this.SetGuiStateFeedbackText(SR.GUIStatusRefreshFeedsMessage, ApplicationTrayState.BusyRefreshFeeds);
            }
            else if (newState == NewsHandlerState.RefreshOne)
            {
                this.SetGuiStateFeedbackText(SR.GUIStatusLoadingFeed, ApplicationTrayState.BusyRefreshFeeds);
            }
            else if (newState == NewsHandlerState.RefreshAllAuto || newState == NewsHandlerState.RefreshAllForced)
            {
                this.SetGuiStateFeedbackText(SR.GUIStatusRefreshFeedsMessage, ApplicationTrayState.BusyRefreshFeeds);
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

            this.SaveApplicationState();
            this.UpdateInternetConnectionState();
        }


        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                // re-create a timer that waits three minutes , then invokes every five minutes.
                autoSaveTimer =
                    new Timer(OnAutoSave, this, 3*MilliSecsMultiplier, 5*MilliSecsMultiplier);
                this.UpdateInternetConnectionState(true); // get current state, takes a few msecs
            }
            else if (e.Mode == PowerModes.Suspend)
            {
                this.OnAutoSave(null);

                if (autoSaveTimer != null)
                {
                    autoSaveTimer.Dispose();
                    autoSaveTimer = null;
                }

                this.feedHandler.Offline = true;
            }
        }

        public void UpdateInternetConnectionState()
        {
            this.UpdateInternetConnectionState(false);
        }

        public void UpdateInternetConnectionState(bool forceFullTest)
        {
            INetState state = Utils.CurrentINetState(this.Proxy, forceFullTest);
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
                if (this.feedlistModified && this.feedHandler != null && this.feedHandler.FeedsTable != null &&
                    this.feedHandler.FeedsListOK)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        try
                        {
                            feedHandler.SaveFeedList(stream);
                            if (FileHelper.WriteStreamWithBackup(GetFeedListFileName(), stream))
                            {
                                this.feedlistModified = false; // reset state flag
                            }
                        }
                        catch (Exception ex)
                        {
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

                if (this.watchedItemsFeed.Modified)
                    this.watchedItemsFeed.Save();

                if (this.commentFeedlistModified && this.commentFeedsHandler != null &&
                    this.commentFeedsHandler.FeedsTable != null &&
                    this.commentFeedsHandler.FeedsListOK)
                {
                    using (MemoryStream stream = new MemoryStream())
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


                if (this.trustedCertIssuesModified)
                    this.SaveTrustedCertificateIssues();

                this.SaveModifiedFeeds();

                //save search folders
                this.SaveSearchFolders();

                if (NewsHandler.TopStoriesModified)
                    NewsHandler.SaveCachedTopStoryTitles();

                // Last operation: write all changes to the search index to disk
                if (appIsClosing)
                    this.FeedHandler.SearchHandler.StopIndexer();
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
                ArrayList feedurls = RssLocater.UrlsFromWellknownListener(webUrl);

                foreach (string feedurl in feedurls)
                {
                    if (feedurl.ToLower().EndsWith(".opml"))
                    {
                        this.ImportFeeds(feedurl); //displays a dialog also
                        captured = true;
                    }
                    else
                    {
                        // assume, it is a valid feed link url
                        this.CmdNewFeed(defaultCategory, feedurl, null);
                        captured = true;
                    }
                }

                return captured;
            }
            else if (url.Scheme.Equals("fdaction"))
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
                    NewsFeed f = GetFeed(normalizedUrl);
                    if (f != null)
                        guiMain.NavigateToFeed(f);
                }
                else if (webUrl.IndexOf("unsubscribefeed") != -1)
                {
                    string normalizedUrl = HtmlHelper.UrlDecode(webUrl.Substring(feedIdIndex));
                    NewsFeed f = GetFeed(normalizedUrl);
                    if (f != null)
                        this.UnsubscribeFeed(f, false);
                }
                return true;
            }
            else if (url.Scheme.Equals("feed"))
            {
                this.CmdNewFeed(defaultCategory, RssLocater.UrlFromFeedProtocolUrl(url.ToString()), null);
                return true;
            }
            else if (url.ToString().EndsWith(".opml"))
            {
                this.ImportFeeds(url.ToString());
                return true;
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
                    using (AskForDefaultAggregator dialog = new AskForDefaultAggregator())
                    {
                        if (dialog.ShowDialog(guiMain) == DialogResult.OK)
                        {
                            try
                            {
                                MakeDefaultAggregator();
                            }
                            catch (SecurityException)
                            {
                                this.MessageInfo(SR.SecurityExceptionCausedByRegistryAccess("HKEY_CLASSES_ROOT\feed"));
                            }
                            catch (Exception ex)
                            {
                                this.MessageError(SR.ExceptionSettingDefaultAggregator(ex.Message));
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
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("AppStartupPath", String.Empty, Application.StartupPath);
            xslArgs.AddParam("AppUserDataPath", String.Empty, GetUserPath());
            xslArgs.AddParam("MarkItemsAsReadWhenViewed", String.Empty, this.Preferences.MarkItemsAsReadWhenViewed);
            xslArgs.AddParam("LimitNewsItemsPerPage", String.Empty, this.Preferences.LimitNewsItemsPerPage);
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
            if (!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
            {
                this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
            }

            // display channel processing:
            foreach (FeedInfo fi in feeds)
            {
                foreach (NewsItem n in fi.ItemsList)
                {
                    DisplayingNewsChannelServices.ProcessItem(n);
                }
            }

            return this.NewsItemFormatter.ToHtml(stylesheet, feeds, this.PrepareXsltArgs());
        }


        /// <summary>
        /// Uses the current defined XSLT template to
        /// format the feed  to HTML.
        /// </summary>
        /// <param name="stylesheet">The stylesheet.</param>
        /// <param name="feed">The feed to transform</param>
        /// <returns>The feed formatted as a HTML string</returns>
        public string FormatFeed(string stylesheet, FeedInfo feed)
        {
            if (!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
            {
                this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
            }

            // display channel processing:
            foreach (NewsItem item in feed.ItemsList)
            {
                DisplayingNewsChannelServices.ProcessItem(item);
            }

            return this.NewsItemFormatter.ToHtml(stylesheet, feed, this.PrepareXsltArgs());
        }


        /// <summary>
        /// Uses the current defined XSLT template to
        /// format the item to HTML.
        /// </summary>
        /// <param name="stylesheet">The stylesheet.</param>
        /// <param name="item">The NewsItem</param>
        /// <param name="toHighlight">To highlight.</param>
        /// <returns>The NewsItem formatted as a HTML string</returns>
        public string FormatNewsItem(string stylesheet, NewsItem item, SearchCriteriaCollection toHighlight)
        {
            if (!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
            {
                this.NewsItemFormatter.AddXslStyleSheet(stylesheet, this.GetNewsItemFormatterTemplate(stylesheet));
            }

            // display channel processing:
            item = (NewsItem) DisplayingNewsChannelServices.ProcessItem(item);

            if (toHighlight == null)
            {
                return this.NewsItemFormatter.ToHtml(stylesheet, item, this.PrepareXsltArgs());
            }
            else
            {
                List<SearchCriteriaString> criterias = new List<SearchCriteriaString>();
                for (int i = 0; i < toHighlight.Count; i++)
                {
                    // only String matches are interesting for highlighting
                    SearchCriteriaString scs = toHighlight[i] as SearchCriteriaString;
                    if (scs != null && scs.Match(item))
                    {
                        criterias.Add(scs);
                    }
                }
                if (criterias.Count > 0)
                {
                    //SearchHitNewsItem shitem = item as SearchHitNewsItem; 
                    NewsItem clone = new NewsItem(item.Feed, item.Title, item.Link,
                                                  ApplyHighlightingTo(item.Content, criterias), item.Date,
                                                  item.Subject,
                                                  item.ContentType, item.OptionalElements, item.Id, item.ParentId);
                    clone.FeedDetails = item.FeedDetails;
                    clone.BeenRead = item.BeenRead;
                    return NewsItemFormatter.ToHtml(stylesheet, clone, this.PrepareXsltArgs());
                }
                else
                {
                    return this.NewsItemFormatter.ToHtml(stylesheet, item, this.PrepareXsltArgs());
                }
            }
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
                            Regex replaceRegex =
                                new Regex("(" + EscapeRegexSpecialChars(scs.What) + ")", RegexOptions.IgnoreCase);
                            string highlightedxhtml =
                                replaceRegex.Replace(strippedxhtml,
                                                     "<span style='color:highlighttext;background:highlight'>$1</span>");

                            //Reinsert HTML
                            StringBuilder sb = new StringBuilder();
                            string[] splitxhtml = SearchCriteriaString.placeholderRegex.Split(highlightedxhtml);

                            foreach (string s in splitxhtml)
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
                            Regex replaceRegex2 = new Regex("(" + scs.What + ")");
                            string highlightedxhtml2 =
                                replaceRegex2.Replace(strippedxhtml2,
                                                      "<span style='color:highlighttext;background:highlight'>$1</span>");


                            //Reinsert HTML
                            StringBuilder sb2 = new StringBuilder();
                            string[] splitxhtml2 = SearchCriteriaString.placeholderRegex.Split(highlightedxhtml2);

                            foreach (string s in splitxhtml2)
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

        /// <summary>
        /// Reads an app settings entry.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        /// <param name="entryType">Expected Type of the entry.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Value read or defaultValue</returns>
        public static object ReadAppSettingsEntry(string name, Type entryType, object defaultValue)
        {
            if (string.IsNullOrEmpty(name))
                return defaultValue;

            if (entryType == null)
                throw new ArgumentNullException("entryType");

            string value = ConfigurationManager.AppSettings[name];
            if (!string.IsNullOrEmpty(value))
            {
                if (entryType == typeof (bool))
                {
                    try
                    {
                        return Boolean.Parse(value);
                    }
                    catch (FormatException)
                    {
                    }
                }
                else if (entryType == typeof (string))
                {
                    return value;
                }
                else if (entryType.IsEnum)
                {
                    try
                    {
                        return Enum.Parse(entryType, value, true);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                else if (entryType == typeof (Color))
                {
                    Color c = Color.FromName(value);
                    // If name is not the valid name of a pre-defined color, 
                    // the FromName method creates a Color structure that has
                    // an ARGB value of zero (that is, all ARGB components are 0).
                    if (c.ToArgb() != 0)
                        return c;
                }
                else
                {
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
        public static string GetAssemblyInformationalVersion(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false);
            if (attributes.Length > 0)
            {
                string ad = ((AssemblyInformationalVersionAttribute) attributes[0]).InformationalVersion;
                if (!string.IsNullOrEmpty(ad))
                    return ad;
            }
            return String.Empty;
        }

        /// <summary>
        /// Loads the default stylesheet from disk and returns it as a string
        /// </summary>
        /// <returns>The XSLT stylesheet</returns>
        protected string GetNewsItemFormatterTemplate()
        {
            return this.GetNewsItemFormatterTemplate(Preferences.NewsItemStylesheetFile);
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

            if (stylesheet == null || stylesheet.Length == 0)
                return t;

            if (Directory.Exists(s))
            {
                string filename = Path.Combine(s, stylesheet + ".fdxsl");
                if (File.Exists(filename))
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(filename, true))
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
            this.commandLineOptions.SubscribeTo.Clear();
            // parse command line...
            if (this.HandleCommandLineArgs(args))
            {
                this.CmdShowMainGui(null);

                // fix of issue https://sourceforge.net/tracker/?func=detail&atid=615248&aid=1404778&group_id=96589
                // we use now a copy of the .SubscribeTo collection to allow users clicking twice or more to
                // a "feed:uri" link while a subscription wizard window is still yet open:
                foreach (string newFeedUrl in new ArrayList(this.commandLineOptions.SubscribeTo))
                {
                    if (IsFormAvailable(this.guiMain))
                        this.guiMain.AddFeedUrlSynchronized(newFeedUrl);
                }
            }
        }

        public CommandLineOptions CommandLineArgs
        {
            get
            {
                return this.commandLineOptions;
            }
        }

        /// <summary>
        /// Handle command line arguments.
        /// </summary>
        /// <param name="args">Arguments string list</param>
        /// <returns>True, if all is OK, False if further processing should stop.</returns>
        public bool HandleCommandLineArgs(string[] args)
        {
            bool retVal = true;
            CommandLineParser commandLineParser = new CommandLineParser(typeof (CommandLineOptions));
            try
            {
                commandLineParser.Parse(args, this.commandLineOptions);
                if (this.commandLineOptions.ShowHelp)
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
            InvokeOnGui(delegate
                            {
                                this.guiMain.SetGuiStateFeedback(message);
                            });
        }

        private void SetGuiStateFeedbackText(string message, ApplicationTrayState state)
        {
            InvokeOnGui(delegate
                            {
                                this.guiMain.SetGuiStateFeedback(message, state);
                            });
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

            NewsItem item2post, item2reply;
            PostReplyThreadHandler prth;

            if (replyEventArgs.ReplyToItem != null)
            {
                item2reply = replyEventArgs.ReplyToItem;
                string parentID = item2reply.Id;

                XmlDocument tempDoc = new XmlDocument();

                if (replyEventArgs.Beautify)
                {
// not yet active (but in next release):
                    comment = replyEventArgs.Comment.Replace("\r\n", "<br />");
                    item2post =
                        new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, ContentType.Html,
                                     new Hashtable(), url, parentID);
                }
                else
                {
                    comment = replyEventArgs.Comment;
                    item2post =
                        new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, null, parentID);
                }

                string commentUrl = item2reply.CommentUrl;
                item2post.FeedDetails = item2reply.FeedDetails;
                item2post.Author = (email == null) || (email.Trim().Length == 0) ? name : email + " (" + name + ")";

                /* redundancy here, because Joe Gregorio changed spec now must support both <author> and <dc:creator> */
                XmlElement emailNode = tempDoc.CreateElement("author");
                emailNode.InnerText = item2post.Author;

                item2post.OptionalElements.Add(new XmlQualifiedName("author"), emailNode.OuterXml);
                item2post.ContentType = ContentType.Html;

                prth = new PostReplyThreadHandler(this.feedHandler, commentUrl, item2post, item2reply);
                DialogResult result = prth.Start(postReplyForm, SR.GUIStatusPostReplyToItem);

                if (result != DialogResult.OK)
                    return;

                if (!prth.OperationSucceeds)
                {
                    this.MessageError(SR.ExceptionPostReplyToNewsItem(
                                          (string.IsNullOrEmpty(item2reply.Title)
                                               ? item2reply.Link
                                               : item2reply.Title),
                                          prth.OperationException.Message));
                    return;
                }

                this.AddSentNewsItem(item2reply, item2post);
                success = true;
            }
            else if (replyEventArgs.PostToFeed != null)
            {
                NewsFeed f = replyEventArgs.PostToFeed;
                XmlDocument tempDoc = new XmlDocument();

                if (replyEventArgs.Beautify)
                {
// not yet active (but in next release):
                    comment = replyEventArgs.Comment.Replace("\r\n", "<br />");
                    item2post =
                        new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, ContentType.Html,
                                     new Hashtable(), url, null);
                }
                else
                {
                    comment = replyEventArgs.Comment;
                    item2post = new NewsItem(this.sentItemsFeed, title, url, comment, DateTime.Now, null, null, null);
                }

                item2post.CommentStyle = SupportedCommentStyle.NNTP;
                // in case the feed does not yet have downloaded items, we may get null here:
                item2post.FeedDetails = this.feedHandler.GetFeedInfo(f.link);
                if (item2post.FeedDetails == null)
                    item2post.FeedDetails =
                        new FeedInfo(f.id, f.cacheurl, new List<NewsItem>(0), f.title, f.link, f.title);
                item2post.Author = (email == null) || (email.Trim().Length == 0) ? name : email + " (" + name + ")";

                /* redundancy here, because Joe Gregorio changed spec now must support both <author> and <dc:creator> */
                XmlElement emailNode = tempDoc.CreateElement("author");
                emailNode.InnerText = item2post.Author;

                item2post.OptionalElements.Add(new XmlQualifiedName("author"), emailNode.OuterXml);
                item2post.ContentType = ContentType.Html;

                prth = new PostReplyThreadHandler(this.feedHandler, item2post, f);
                DialogResult result = prth.Start(postReplyForm, SR.GUIStatusPostNewFeedItem);

                if (result != DialogResult.OK)
                    return;

                if (!prth.OperationSucceeds)
                {
                    this.MessageError(SR.ExceptionPostNewFeedItem(
                                          (string.IsNullOrEmpty(item2post.Title) ? f.link : item2post.Title),
                                          prth.OperationException.Message));
                    return;
                }

                this.AddSentNewsItem(f, item2post);
                success = true;
            }

            if (success)
            {
                if (this.postReplyForm != null)
                {
                    this.postReplyForm.Hide();

                    if (!this.postReplyForm.IsDisposed)
                    {
                        this.postReplyForm.Dispose();
                    }
                    this.postReplyForm = null;
                }
            }
            else
            {
                if (this.postReplyForm != null)
                {
                    this.postReplyForm.Show();
                    Win32.SetForegroundWindow(this.postReplyForm.Handle);
                }
            }
        }

        #region global app helper

        public class CommandLineOptions
        {
            private bool startInTaskbarNotificationAreaOnly = false;

            /// <summary>
            /// Have a look to http://blogs.gotdotnet.com/raymondc/permalink.aspx/5a811e6f-cd12-48de-8994-23409290faea,
            /// that is why we does not name it "StartInSystemTray" or such.
            /// </summary>
            [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "taskbar", ShortName = "t",
                Description = SR.Keys.CmdLineStartInTaskbarDesc, DescriptionIsResourceId = true)]
            public bool StartInTaskbarNotificationAreaOnly
            {
                get
                {
                    return startInTaskbarNotificationAreaOnly;
                }
                set
                {
                    startInTaskbarNotificationAreaOnly = value;
                }
            }

            private StringCollection subscribeTo = new StringCollection();

            [DefaultCommandLineArgument(CommandLineArgumentTypes.Multiple, Name = "feedUrl",
                Description = SR.Keys.CmdLineSubscribeToDesc, DescriptionIsResourceId = true)]
            public StringCollection SubscribeTo
            {
                get
                {
                    return subscribeTo;
                }
                set
                {
                    subscribeTo = value;
                }
            }

            private bool showHelp;

            [CommandLineArgument(CommandLineArgumentTypes.Exclusive, Name = "help", ShortName = "h",
                Description = SR.Keys.CmdLineHelpDesc, DescriptionIsResourceId = true)]
            public bool ShowHelp
            {
                get
                {
                    return showHelp;
                }
                set
                {
                    showHelp = value;
                }
            }

            private string localCulture = String.Empty;

            [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "culture", ShortName = "c",
                Description = SR.Keys.CmdLineCultureDesc, DescriptionIsResourceId = true)]
            public string LocalCulture
            {
                get
                {
                    return localCulture;
                }
                set
                {
                    localCulture = value;
                    if (string.IsNullOrEmpty(localCulture))
                    {
                        localCulture = String.Empty;
                    }
                }
            }

            private bool resetUi;

            [CommandLineArgument(CommandLineArgumentTypes.AtMostOnce, Name = "resetUI", ShortName = "r",
                Description = SR.Keys.CmdLineResetUIDesc, DescriptionIsResourceId = true)]
            public bool ResetUserInterface
            {
                get
                {
                    return resetUi;
                }
                set
                {
                    resetUi = value;
                }
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
            using (PodcastOptionsDialog optionDialog = new PodcastOptionsDialog(Preferences, this))
            {
                optionDialog.ShowDialog(owner ?? guiMain);
                if (optionDialog.DialogResult == DialogResult.OK)
                {
                    //modify preferences with data from dialog
                    this.feedHandler.PodcastFileExtensionsAsString = optionDialog.textPodcastFilesExtensions.Text;

                    if (optionDialog.chkCopyPodcastToFolder.Checked)
                    {
                        this.feedHandler.PodcastFolder = optionDialog.txtCopyPodcastToFolder.Text;
                    }
                    else
                    {
                        this.feedHandler.PodcastFolder = this.feedHandler.EnclosureFolder;
                    }

                    this.Preferences.AddPodcasts2Folder = optionDialog.chkCopyPodcastToFolder.Checked;
                    this.Preferences.AddPodcasts2ITunes = optionDialog.chkCopyPodcastToITunesPlaylist.Checked;
                    this.Preferences.AddPodcasts2WMP = optionDialog.chkCopyPodcastToWMPlaylist.Checked;

                    this.Preferences.SinglePodcastPlaylist = optionDialog.optSinglePlaylistName.Checked;
                    this.Preferences.SinglePlaylistName = optionDialog.textSinglePlaylistName.Text;


                    // apply to backend, UI etc. and save:
                    this.ApplyPreferences();
                    this.SavePreferences();

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
            if (!this.SearchEngineHandler.EnginesLoaded || !this.SearchEngineHandler.EnginesOK)
                this.LoadSearchEngines();

            PreferencesDialog propertiesDialog =
                new PreferencesDialog(this, refreshRate/60000, Preferences, this.searchEngines, this.IdentityManager);
            propertiesDialog.OnApplyPreferences += this.OnApplyPreferences;
            if (optionsChangedHandler != null)
                propertiesDialog.OnApplyPreferences += optionsChangedHandler;

            // just to check..
            //propertiesDialog.Disposed += new EventHandler(OnDialogDisposed);

            propertiesDialog.SelectedSection = selectedSection;
            propertiesDialog.ShowDialog(owner ?? guiMain);

            if (propertiesDialog.DialogResult == DialogResult.OK)
            {
                this.OnApplyPreferences(propertiesDialog, new EventArgs());
                if (optionsChangedHandler != null)
                    optionsChangedHandler(propertiesDialog, new EventArgs());
            }

            // detach event(s) to get the dialog garbage collected:
            propertiesDialog.OnApplyPreferences -= this.OnApplyPreferences;
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
        public void ShowNntpServerManagementDialog(IWin32Window owner, EventHandler definitionChangeEventHandler)
        {
            if (definitionChangeEventHandler != null)
                this.NntpServerManager.NewsServerDefinitionsModified += definitionChangeEventHandler;
            this.NntpServerManager.ShowNewsServerSubscriptionsDialog(owner ?? guiMain);
            if (definitionChangeEventHandler != null)
                this.NntpServerManager.NewsServerDefinitionsModified -= definitionChangeEventHandler;
        }

        /// <summary>
        /// Shows the user identity management dialog.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="definitionChangeEventHandler">The definition change event handler.</param>
        public void ShowUserIdentityManagementDialog(IWin32Window owner, EventHandler definitionChangeEventHandler)
        {
            if (definitionChangeEventHandler != null)
                this.IdentityManager.IdentityDefinitionsModified += definitionChangeEventHandler;
            this.IdentityManager.ShowIdentityDialog(owner ?? guiMain);
            if (definitionChangeEventHandler != null)
                this.IdentityManager.IdentityDefinitionsModified -= definitionChangeEventHandler;
        }

        string ICoreApplication.DefaultCategory
        {
            get
            {
                return defaultCategory;
            }
        }

        public string[] GetCategories()
        {
            string[] cats = new string[1 + this.FeedHandler.Categories.Count];
            cats[0] = DefaultCategory;
            if (cats.Length > 1)
                this.FeedHandler.Categories.Keys.CopyTo(cats, 1);
            return cats;
        }
       

        public int CurrentGlobalRefreshRate
        {
            get
            {
                return this.refreshRate/MilliSecsMultiplier;
            }
        }

        public void AddCategory(string category)
        {
            if (category != null)
            {
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

        public bool SearchForFeeds(string searchTerm)
        {
            return SubscribeToFeed(null, null, null, searchTerm, WizardMode.SubscribeSearchDirect);
        }

        public bool SubscribeToFeed(string url, string category, string title)
        {
            WizardMode mode = WizardMode.Default;
            if (! string.IsNullOrEmpty(url))
            {
                mode = WizardMode.SubscribeURLDirect;
                if (RssHelper.IsNntpUrl(url))
                    mode = WizardMode.SubscribeNNTPGroupDirect;
            }
            return SubscribeToFeed(url, category, title, null, mode);
        }

        public bool SubscribeToFeed(string url, string category, string title, string searchTerms, WizardMode mode)
        {
            AddSubscriptionWizard wiz = new AddSubscriptionWizard(this, mode);
            wiz.FeedUrl = (url ?? String.Empty);
            if (category != null) // does remember the last category:
                wiz.FeedCategory = category;
            wiz.FeedTitle = (title ?? String.Empty);
            wiz.SearchTerms = (searchTerms ?? String.Empty);

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
                NewsFeed f;

                if (wiz.MultipleFeedsToSubscribe)
                {
                    bool anySubscription = false;

                    for (int i = 0; i < wiz.MultipleFeedsToSubscribeCount; i++)
                    {
                        f = this.CreateFeedFromWizard(wiz, i);
                        if (f == null)
                        {
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
                }
                else
                {
                    f = this.CreateFeedFromWizard(wiz, 0);

                    if (f == null)
                    {
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

            }

            wiz.Dispose();
            return false;
        }

        private NewsFeed CreateFeedFromWizard(AddSubscriptionWizard wiz, int index)
        {
            NewsFeed f = new NewsFeed();

            f.link = wiz.FeedUrls(index);
            if (feedHandler.FeedsTable.ContainsKey(f.link))
            {
                NewsFeed f2 = feedHandler.FeedsTable[f.link];
                this.MessageInfo(SR.GUIFieldLinkRedundantInfo(
                                     (f2.category == null ? String.Empty : f2.category + NewsHandler.CategorySeparator) +
                                     f2.title, f2.link));

                return null;
            }

            f.title = wiz.FeedTitles(index);
            f.category = wiz.FeedCategory;
            if ((f.category != null) && (!feedHandler.Categories.ContainsKey(f.category)))
            {
                feedHandler.Categories.Add(f.category);
            }

            if (!string.IsNullOrEmpty(wiz.FeedCredentialUser))
            {
                // set NewsFeed new credentials
                string u = wiz.FeedCredentialUser, p = null;
                if (!string.IsNullOrEmpty(wiz.FeedCredentialPwd))
                    p = wiz.FeedCredentialPwd;
                NewsHandler.SetFeedCredentials(f, u, p);
            }
            else
            {
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

            if (stylesheet != null && !stylesheet.Equals(this.feedHandler.GetStyleSheet(f.link)))
            {
                this.feedHandler.SetStyleSheet(f.link, stylesheet);

                if (!this.NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
                {
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
        public void UnsubscribeFeed(string feedUrl, bool askUser)
        {
            UnsubscribeFeed(GetFeed(feedUrl), askUser);
        }

        /// <summary>
        /// Unsubscribes the feed.
        /// </summary>
        /// <param name="feed">The feed.</param>
        /// <param name="askUser">if set to <c>true</c> [ask user].</param>
        public void UnsubscribeFeed(NewsFeed feed, bool askUser)
        {
            if (feed == null) return;

            TreeFeedsNodeBase tn = (TreeFeedsNodeBase) feed.Tag;
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
                    this.DeleteFeed(feed.link);
                    guiMain.CurrentSelectedFeedsNode = null;
                }
            }
        }

        public bool ContainsFeed(string address)
        {
            return this.feedHandler.FeedsTable.ContainsKey(address);
        }

        public bool TryGetFeedDetails(string url, out string category, out string title, out string link)
        {
            NewsFeed f;
            if (feedHandler.FeedsTable.TryGetValue(url, out f))
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
            get
            {
                return new ReadOnlyDictionary((IDictionary) this.IdentityManager.CurrentIdentities);
            }
        }

        IDictionary<string, INntpServerDefinition> ICoreApplication.NntpServerDefinitions
        {
            get
            {
                return this.NntpServerManager.CurrentNntpServers;
            }
        }

        IList ICoreApplication.GetNntpNewsGroups(string nntpServerName, bool forceReloadFromServer)
        {
            if (! string.IsNullOrEmpty(nntpServerName) &&
                this.NntpServerManager.CurrentNntpServers.ContainsKey(nntpServerName))
            {
                INntpServerDefinition sd = this.NntpServerManager.CurrentNntpServers[nntpServerName];
                if (sd != null)
                    return (IList) this.NntpServerManager.LoadNntpNewsGroups(guiMain, sd, forceReloadFromServer);
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
                List<string> formatters = new List<string>(tmplFiles.GetLength(0));
                foreach (string filename in tmplFiles)
                {
                    formatters.Add(Path.GetFileNameWithoutExtension(filename));
                }
                return formatters;
            }
            else
            {
                return new List<string>(0);
            }
        }

        /// <summary>
        /// Gets the defined web search engines. 
        /// Items are of type ISearchEngine, keys are the corresponding Title.
        /// </summary>
        IList ICoreApplication.WebSearchEngines
        {
            get
            {
                return ArrayList.ReadOnly(this.SearchEngineHandler.Engines);
            }
        }

        bool ICoreApplication.SubscribeToFeed(string url, string category)
        {
            return this.SubscribeToFeed(url, category, null);
        }

        bool ICoreApplication.SubscribeToFeed(string url)
        {
            return this.SubscribeToFeed(url, DefaultCategory, null);
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
            InvokeOnGui(delegate
            {
                guiMain.DetailTabNavigateToUrl(url, tabCaption, forceNewTabOrWindow, setFocus);
            });
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
                if (this.MessageQuestion(SR.ExceptionStartDefaultBrowserMessage(ex.Message, url)) == DialogResult.Yes)
                {
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
        void ICoreApplication.RegisterReceivingNewsChannelProcessor(IChannelProcessor channelProcessor)
        {
            if (channelProcessor == null)
                return;

            INewsChannel[] channels = channelProcessor.GetChannels();
            if (channels == null || channels.Length == 0)
                return;
            foreach (INewsChannel channel in channels)
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
            foreach (INewsChannel channel in channels)
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
            foreach (INewsChannel channel in channels)
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
            foreach (INewsChannel channel in channels)
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
            get
            {
                return displayingNewsChannel;
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Internal accessor to the ICoreApplication interface services.
        /// </summary>
        internal ICoreApplication CoreServices
        {
            get
            {
                return this;
            }
        }


/*
		private void OnDialogDisposed(object sender, EventArgs e) {
			Trace.WriteLine("Dialog Disposed('" + sender.GetType().Name.ToString() + "')");
		}
*/
    } //end class RssBanditApplication
} //end namespace RssBandit
