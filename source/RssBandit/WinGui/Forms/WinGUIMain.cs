#region Version Header

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

#region framework namespaces

#endregion

    #region third party namespaces

    // docking panels, docked tabs
    // external webbrowser control
    // related interfaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.ThListView;
using System.Windows.Forms.ThListView.Sorting;
using AppInteropServices;
using IEControl;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinToolTip;
using Infragistics.Win.UltraWinTree;
using log4net;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.News;
using NewsComponents.RelationCosmos;
using NewsComponents.Search;
using NewsComponents.Utils;
using RssBandit.AppServices;
using RssBandit.Common;
using RssBandit.Common.Logging;
using RssBandit.Filter;
using RssBandit.Resources;
using RssBandit.Utility.Keyboard;
using RssBandit.WebSearch;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Forms.ControlHelpers;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Utility;
using SHDocVw;
using Syndication.Extensibility;
using TD.SandDock;
using TD.SandDock.Rendering;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
using ExecuteCommandHandler=RssBandit.WinGui.Interfaces.ExecuteCommandHandler;

    #endregion

    #region project namespaces

using K = RssBandit.Utility.Keyboard;
using SortOrder=System.Windows.Forms.SortOrder;
using Timer=System.Windows.Forms.Timer;
using ToolTip=System.Windows.Forms.ToolTip;

#endregion

namespace RssBandit.WinGui.Forms
{

    #region Enum defs.

    /// <summary>
    /// Enumeration that defines the possible embedded web browser actions
    /// to perform from the main application.
    /// </summary>
    internal enum BrowseAction
    {
        NavigateCancel,
        NavigateBack,
        NavigateForward,
        DoRefresh
    }

    /// <summary>
    /// Enumeration that defines the type of the known root folders
    /// of Bandit displayed within the treeview.
    /// </summary>
    internal enum RootFolderType
    {
        MyFeeds,
        SmartFolders,
        Finder
    }

    /// <summary>
    /// Defines the subscription tree node processing states
    /// </summary>
    internal enum FeedProcessingState
    {
        Normal,
        Updating,
        Failure,
    }

    /// <summary>
    /// Used to delay execution of some UI tasks by a timer
    /// </summary>
    [Flags]
    internal enum DelayedTasks
    {
        None = 0,
        NavigateToWebUrl = 1,
        StartRefreshOneFeed = 2,
        StartRefreshAllFeeds = 4,
        ShowFeedPropertiesDialog = 8,
        NavigateToFeedNewsItem = 16,
        AutoSubscribeFeedUrl = 32,
        ClearBrowserStatusInfo = 64,
        RefreshTreeUnreadStatus = 128,
        SyncRssSearchTree = 256,
        InitOnFinishLoading = 512,
        SaveUIConfiguration = 1024,
        NavigateToFeed = 2048,
        RefreshTreeCommentStatus = 4096,
    }

    internal enum NavigationPaneView
    {
        Subscriptions,
        RssSearch,
    }

    #endregion

    /// <summary>
    /// Summary description for WinGuiMain.
    /// </summary>
    //[CLSCompliant(true)]
    internal class WinGuiMain : Form,
                                ITabState, IMessageFilter, ICommandBarImplementationSupport
    {
        #region delegates and events declarations

        /// <summary>
        /// Delegate used for calling UpdateTreeStatus(Hashtable) in the correct thread.
        /// </summary>
        /// <remarks>Read the article at 
        /// http://msdn.microsoft.com/library/en-us/dnforms/html/winforms06112002.asp
        /// for an explanation of why this delegate is needed.</remarks>
        public delegate void UpdateTreeStatusDelegate(IDictionary<string, NewsFeed> theFeeds, RootFolderType rootFolder);

        private delegate void PopulateTreeFeedsDelegate(
            ICollection<category> categories, IDictionary<string, NewsFeed> feedsTable, string defaultCategory);

        /// <summary>
        /// Delegate used for calling PopulateListView() in the correct thread.
        /// </summary>
        private delegate void PopulateListViewDelegate(
            TreeFeedsNodeBase associatedFeedsNode, IList<NewsItem> list, bool forceReload, bool categorizedView,
            TreeFeedsNodeBase initialFeedsNode);

        /// <summary>
        /// Delegate used to invoke SetGuiStateFeedback() from other UI threads 
        /// </summary>
        public delegate void SetGuiMessageFeedbackDelegate(string message);

        /// <summary>
        /// Delegate used to invoke SetGuiStateFeedback() from other UI threads 
        /// </summary>
        public delegate void SetGuiMessageStateFeedbackDelegate(string message, ApplicationTrayState state);

        /// <summary>
        /// Delegate used to invoke GetItemsForFeed() on RssParser class in a 
        /// background thread so as not to tie up the UI. 
        /// </summary>
        public delegate void GetCommentNewsItemsDelegate(NewsItem item, ThreadedListViewItem listViewItem);

        /// <summary>
        /// used to start NewsSearch asynchron
        /// </summary>
        private delegate void StartNewsSearchDelegate(FinderNode node);

        /// <summary>
        /// used to start RssRemoteSearch asynchron
        /// </summary>
        private delegate void StartRssRemoteSearchDelegate(string searchUrl, FinderNode resultContainer);

        /// <summary>
        /// Called from within a toast window
        /// </summary>
        public delegate void NavigateToURLDelegate(string url, string tab, bool createNewTab, bool setFocus);

        /// <summary>
        /// Command line param
        /// </summary>
        private delegate void SubscribeToFeedUrlDelegate(string newFeedUrl);


        /// <summary>
        /// Used to queue a UI task
        /// </summary>
        private delegate void DelayTaskDelegate(DelayedTasks task, object data, int interval);

        /// <summary>
        /// Enable close window, if called from another thread
        /// </summary>
        internal delegate void CloseMainForm(bool force);

        // there is really no such thing on the native form interface :-(
        public event EventHandler OnMinimize;

        #endregion

        #region private variables
       
        /// <summary>
        /// Async invoke on UI thread
        /// </summary>
        private readonly Action<Action> InvokeOnGui;

        /// <summary>
        /// Sync invoke on UI thread
        /// </summary>
        private readonly Action<Action> InvokeOnGuiSync;

        private static readonly TimeSpan SevenDays = new TimeSpan(7, 0, 0, 0);

        private const int BrowserProgressBarWidth = 120;
        private const int MaxHeadlineWidth = 133;

        /// <summary>
        /// To be raised by one on every Toolbars modification like new tools or menus!
        /// </summary>
        /// <remarks>
        /// If you forget this, you will always get your old toolbars layout
        /// restored from the users local machine.
        /// </remarks>
        private const int _currentToolbarsVersion = 9;

        /// <summary>
        /// To be raised by one on every UltraExplorerBar docks modification like new groups!
        /// </summary>
        /// <remarks>
        /// If you forget this, you will always get your old groups layout
        /// restored from the users local machine.
        /// </remarks>
        private const int _currentExplorerBarVersion = 1;

        private static readonly ILog _log = Log.GetLogger(typeof (WinGuiMain));

        private readonly History _feedItemImpressionHistory;
        private bool _navigationActionInProgress = false;

        private bool _faviconsDownloaded = false;
        private bool _browserTabsRestored = false;

        private ToastNotifier toastNotifier;
        private WheelSupport wheelSupport;
        internal UrlCompletionExtender urlExtender;
        private NavigatorHeaderHelper navigatorHeaderHelper;
        private ToolbarHelper toolbarHelper;
        internal HistoryMenuManager historyMenuManager;

        private readonly TreeFeedsNodeBase[] _roots = new TreeFeedsNodeBase[3];
                                    // store refs to root folders (they order within the treeview may be resorted depending on the languages)

        private TreeFeedsNodeBase _unreadItemsFeedsNode = null;
        private TreeFeedsNodeBase _feedExceptionsFeedsNode = null;
        private TreeFeedsNodeBase _sentItemsFeedsNode = null;
        private TreeFeedsNodeBase _watchedItemsFeedsNode = null;
        private TreeFeedsNodeBase _deletedItemsFeedsNode = null;
        private TreeFeedsNodeBase _flaggedFeedsNodeRoot = null;
        private TreeFeedsNodeBase _flaggedFeedsNodeFollowUp = null;
        private TreeFeedsNodeBase _flaggedFeedsNodeRead = null;
        private TreeFeedsNodeBase _flaggedFeedsNodeReview = null;
        private TreeFeedsNodeBase _flaggedFeedsNodeForward = null;
        private TreeFeedsNodeBase _flaggedFeedsNodeReply = null;
        private FinderNode _searchResultNode = null;

        //private TreeFeedsPainter _treeFeedsPainter = null;

        //private ListViewSortHelper _lvSortHelper = null;
        private NewsItemFilterManager _filterManager = null;

        private SearchPanel searchPanel;

        private IList<IBlogExtension> blogExtensions = null;

        // store the HashCodes of temp. NewsItems, that have bean read.
        // Used for commentRss implementation
        private readonly Dictionary<string, object> tempFeedItemsRead = new Dictionary<string, object>();

        //Stores Image object for favicons so we can reuse the same object if used by
        //multiple feeds. 
        private readonly Dictionary<string, Image> _favicons = new Dictionary<string, Image>();

        // used to store temp. the currently yet populated feeds to speedup category population
        private readonly Dictionary<string, object> feedsCurrentlyPopulated = new Dictionary<string, object>();

        private ShortcutHandler _shortcutHandler;

        private INewsItem _currentNewsItem = null;

        private TreeFeedsNodeBase _currentSelectedFeedsNode = null;
                                  // currently selected node at the treeView (could be also temp. change on Right-Click, so it is different from treeView.SelectedNode )

        private TreeFeedsNodeBase _currentDragFeedsNode = null;
        private TreeFeedsNodeBase _currentDragHighlightFeedsNode = null;

        //used for storing current news items to display in reading pane. We store them 
        //in member variables for quick access as part of newspaper paging implementation.
        private FeedInfo _currentFeedNewsItems = null;
        private FeedInfoList _currentCategoryNewsItems = null;
        private int _currentPageNumber = 1;
        private int _lastPageNumber = 1;

        // used to save last used window size to be restored after System Tray Mode:
        private Rectangle _formRestoreBounds = Rectangle.Empty;

        //these are here, because we only want to display the balloon popup,
        //if there are really new items received:
        private int _lastUnreadFeedItemCountBeforeRefresh = 0;

        private int _beSilentOnBalloonPopupCounter = 0;
                    // if a user explicitly close the balloon, we are silent for the next 12 retries (we refresh all 5 minutes, so this equals at least to one hour)

        private bool _forceShutdown = false;
        private readonly FormWindowState initialStartupState = FormWindowState.Normal;

        private int _webTabCounter = 0;

        // variables set in PreFilterMessage() to indicate the user clicked an url; reset also there on mouse-move, or within WebBeforeNavigate event
        private bool _webUserNavigated = false;
        private bool _webForceNewTab = false;
        private Point _lastMousePosition = Point.Empty;

        // GUI main components:
        private ImageList _allToolImages = null;
        private ImageList _treeImages = null;
        private ImageList _listImages = null;

        private MenuItem _feedInfoContextMenu = null;
        private ContextMenu _treeRootContextMenu = null;
        private ContextMenu _treeCategoryContextMenu = null;
        private ContextMenu _treeFeedContextMenu = null;
        private ContextMenu _notifyContextMenu = null;
        private ContextMenu _listContextMenu = null;
        private ContextMenu _treeLocalFeedContextMenu = null;
        private ContextMenu _treeSearchFolderRootContextMenu = null;
        private ContextMenu _treeSearchFolderContextMenu = null;
        private ContextMenu _treeTempSearchFolderContextMenu = null;
        private ContextMenu _docTabContextMenu = null;

        // Used to temp. store the context menu position. Processed later within
        // ICommand event receiver (in Screen-Coordinates).
        private Point _contextMenuCalledAt = Point.Empty;

        private AppContextMenuCommand _listContextMenuDownloadAttachment = null;
        private MenuItem _listContextMenuDeleteItemsSeparator = null;
        private MenuItem _listContextMenuDownloadAttachmentsSeparator = null;

        private TrayStateManager _trayManager = null;
        private NotifyIconAnimation _trayAni = null;

        private string _tabStateUrl;

        // the GUI owner and Form controller
        private RssBanditApplication owner;

        private Panel panelFeedDetails;
        private Panel panelWebDetail;
        private ThreadedListView listFeedItems;

        private UltraToolbarsDockArea _Main_Toolbars_Dock_Area_Left;
        private UltraToolbarsDockArea _Main_Toolbars_Dock_Area_Right;
        private UltraToolbarsDockArea _Main_Toolbars_Dock_Area_Top;
        private UltraToolbarsDockArea _Main_Toolbars_Dock_Area_Bottom;
        private UltraToolbarsManager ultraToolbarsManager;

        internal ToolTip toolTip;
        private Timer _timerResetStatus;
        private Timer _startupTimer;
        private System.Timers.Timer _timerTreeNodeExpand;
        private System.Timers.Timer _timerRefreshFeeds;
        private System.Timers.Timer _timerRefreshCommentFeeds;
        private HtmlControl htmlDetail;
        private StatusBar _status;
        private StatusBarPanel statusBarBrowser;
        private StatusBarPanel statusBarBrowserProgress;
        private StatusBarPanel statusBarConnectionState;
        private StatusBarPanel statusBarRssParser;
        private ProgressBar progressBrowser;
        private Panel panelRssSearch;
        private UITaskTimer _uiTasksTimer;
        private HelpProvider helpProvider1;
        private SandDockManager sandDockManager;
        private DockContainer rightSandDock;
        private DockContainer bottomSandDock;
        private DockContainer topSandDock;
        private DocumentContainer _docContainer;
        private DockControl _docFeedDetails;
        private ThreadedListViewColumnHeader colHeadline;
        private ThreadedListViewColumnHeader colDate;
        private ThreadedListViewColumnHeader colTopic;
        private CollapsibleSplitter detailsPaneSplitter;
        private Timer _timerDispatchResultsToUI;
        private UltraTree treeFeeds;
        private UltraToolTipManager ultraToolTipManager;
        private UltraTreeExtended listFeedItemsO;
        private UltraExplorerBar Navigator;
        private UltraExplorerBarContainerControl NavigatorFeedSubscriptions;
        private UltraExplorerBarContainerControl NavigatorSearch;
        private Panel panelFeedItems;
        private Splitter splitterNavigator;
        private Panel pNavigatorCollapsed;
        private Panel panelClientAreaContainer;
        private Panel panelFeedDetailsContainer;
        private UltraLabel detailHeaderCaption;
        private VerticalHeaderLabel navigatorHiddenCaption;


        private IContainer components;

        #endregion

        #region Class initialize

        public WinGuiMain(RssBanditApplication theGuiOwner, FormWindowState initialFormState)
        {
            InvokeOnGuiSync = delegate(Action a)
            {
                GuiInvoker.Invoke(this, a);
            };

            InvokeOnGui = delegate(Action a)
            {
                GuiInvoker.InvokeAsync(this, a);
            };

            GuiOwner = theGuiOwner;
            initialStartupState = initialFormState;

            urlExtender = new UrlCompletionExtender(this);
            _feedItemImpressionHistory = new History( /* TODO: get maxEntries from .config */);
            _feedItemImpressionHistory.StateChanged += OnFeedItemImpressionHistoryStateChanged;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Init();
            ApplyComponentTranslation();
        }

        protected void Init()
        {
            OnMinimize += OnFormMinimize;
            MouseDown += OnFormMouseDown;
            Resize += OnFormResize;
            Closing += OnFormClosing;
            Load += OnLoad;
            HandleCreated += OnFormHandleCreated;
            Move += OnFormMove;
            Activated += OnFormActivated;
            Deactivate += OnFormDeactivate;

            owner.PreferencesChanged += OnPreferencesChanged;
            owner.FeedDeleted += OnFeedDeleted;

            wheelSupport = new WheelSupport(this);
            wheelSupport.OnGetChildControl += OnWheelSupportGetChildControl;

            SetFontAndColor(
                owner.Preferences.NormalFont, owner.Preferences.NormalFontColor,
                owner.Preferences.UnreadFont, owner.Preferences.UnreadFontColor,
                owner.Preferences.FlagFont, owner.Preferences.FlagFontColor,
                owner.Preferences.ErrorFont, owner.Preferences.ErrorFontColor,
                owner.Preferences.RefererFont, owner.Preferences.RefererFontColor,
                owner.Preferences.NewCommentsFont, owner.Preferences.NewCommentsFontColor
                );

            InitColors();
            InitResources();
            InitShortcutManager();
            // requires resources, shortcuts, etc.:
            CreateIGToolbars();

            InitStatusBar();
            InitDockHosts();
            InitDocumentManager();
            InitContextMenus();
            InitTrayIcon();
            InitWidgets();
        }

        private void ApplyComponentTranslation()
        {
            Text = RssBanditApplication.CaptionOnly; // dynamically changed!
            detailHeaderCaption.Text = SR.MainForm_DetailHeaderCaption_AtStartup; // dynamically changed!
            navigatorHiddenCaption.Text = SR.MainForm_SubscriptionNavigatorCaption;
            Navigator.Groups[Resource.NavigatorGroup.Subscriptions].Text = SR.MainForm_SubscriptionNavigatorCaption;
            Navigator.Groups[Resource.NavigatorGroup.RssSearch].Text = SR.MainForm_SearchNavigatorCaption;
            _docFeedDetails.Text = SR.FeedDetailDocumentTabCaption;
            statusBarBrowser.ToolTipText = SR.MainForm_StatusBarBrowser_ToolTipText;
            statusBarBrowserProgress.ToolTipText = SR.MainForm_StatusBarBrowserProgress_ToolTipText;
            statusBarConnectionState.ToolTipText = SR.MainForm_StatusBarConnectionState_ToolTipText;
            statusBarRssParser.ToolTipText = SR.MainForm_StatusBarRssParser_ToolTipText;
        }

        // set/reset major UI colors
        protected void InitColors()
        {
            BackColor = FontColorHelper.UiColorScheme.FloatingControlContainerToolbar;
            panelClientAreaContainer.BackColor = BackColor;
            splitterNavigator.BackColor = BackColor;
        }

        protected void InitFilter()
        {
            _filterManager = new NewsItemFilterManager();
            _filterManager.Add("NewsItemReferrerFilter", new NewsItemReferrerFilter(owner));
            _filterManager.Add("NewsItemFlagFilter", new NewsItemFlagFilter());
        }

        #endregion

        #region public properties/accessor routines

        /// <summary>
        /// Returns the current page number in the reading pane. 
        /// </summary>
        public int CurrentPageNumber
        {
            get
            {
                return _currentPageNumber;
            }
        }

        /// <summary>
        /// Returns the page number for the last page in the reading pane. 
        /// </summary>
        public int LastPageNumber
        {
            get
            {
                return _lastPageNumber;
            }
        }

        /// <summary>
        /// Gets and sets the GUI owner application
        /// </summary>
        public RssBanditApplication GuiOwner
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
            }
        }

        /// <summary>
        /// Initialized on Class init with the initial Form state (usually defined on a Shortcut)
        /// </summary>
        public FormWindowState InitialStartupState
        {
            get
            {
                return initialStartupState;
            }
        }

        /// <summary>
        /// Provide access to the current entry text within the navigation dropdown
        /// </summary>
        public string UrlText
        {
            get
            {
                if (UrlComboBox.Text.Trim().Length > 0)
                    return UrlComboBox.Text.Trim();
                else
                    return _urlText;
            }
            set
            {
                _urlText = (value ?? String.Empty);
                if (_urlText.Equals("about:blank"))
                {
                    _urlText = String.Empty;
                }
                UrlComboBox.Text = _urlText;
            }
        }

        private string _urlText = String.Empty;
                       // don't know why navigateComboBox.ComboBox.Text returns an empty string after dragging a url to it, so we use this as a workaround

        /// <summary>
        /// Provide access to the current entry text within the web search dropdown
        /// </summary>
        public string WebSearchText
        {
            get
            {
                if (SearchComboBox.Text.Trim().Length > 0)
                    return SearchComboBox.Text.Trim();
                else
                    return _webSearchText;
            }
            set
            {
                _webSearchText = (value ?? String.Empty);
                SearchComboBox.Text = _webSearchText;
            }
        }

        private string _webSearchText = String.Empty;
                       // don't know why searchComboBox.ComboBox.Text returns an empty string after dragging a url to it, so we use this as a workaround

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the FeedExceptions.
        /// </summary>
        public ISmartFolder ExceptionNode
        {
            get
            {
                return _feedExceptionsFeedsNode as ISmartFolder;
            }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the FlaggedFeeds. Because they all share the item list, it is enough
        /// to return one of them
        /// </summary>
        public ISmartFolder FlaggedFeedsNode(Flagged flag)
        {
            switch (flag)
            {
                case Flagged.FollowUp:
                    return _flaggedFeedsNodeFollowUp as ISmartFolder;
                case Flagged.Read:
                    return _flaggedFeedsNodeRead as ISmartFolder;
                case Flagged.Review:
                    return _flaggedFeedsNodeReview as ISmartFolder;
                case Flagged.Reply:
                    return _flaggedFeedsNodeReply as ISmartFolder;
                case Flagged.Forward:
                    return _flaggedFeedsNodeForward as ISmartFolder;
                default:
                    return _flaggedFeedsNodeFollowUp as ISmartFolder;
            }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the Sent Items.
        /// </summary>
        public ISmartFolder SentItemsNode
        {
            get
            {
                return _sentItemsFeedsNode as ISmartFolder;
            }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the Watched Items.
        /// </summary>
        public ISmartFolder WatchedItemsNode
        {
            get
            {
                return _watchedItemsFeedsNode as ISmartFolder;
            }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the Unread Items.
        /// </summary>
        /// <value>The unread items node.</value>
        public ISmartFolder UnreadItemsNode
        {
            get
            {
                return _unreadItemsFeedsNode as ISmartFolder;
            }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the Deleted Items.
        /// </summary>
        public ISmartFolder DeletedItemsNode
        {
            get
            {
                return _deletedItemsFeedsNode as ISmartFolder;
            }
        }

        /// <summary>
        /// Gets the TreeNode instance representing/store of
        /// a non persistent search result (temp. result)
        /// </summary>
        public ISmartFolder SearchResultNode
        {
            get
            {
                return _searchResultNode;
            }
        }

        public void SetFontAndColor(
            Font normalFont, Color normalColor,
            Font unreadFont, Color unreadColor,
            Font highlightFont, Color highlightColor,
            Font errorFont, Color errorColor,
            Font refererFont, Color refererColor,
            Font newCommentsFont, Color newCommentsColor)
        {
            // really do the work
            FontColorHelper.DefaultFont = normalFont;
            FontColorHelper.NormalStyle = normalFont.Style;
            FontColorHelper.NormalColor = normalColor;
            FontColorHelper.UnreadStyle = unreadFont.Style;
            FontColorHelper.UnreadColor = unreadColor;
            FontColorHelper.HighlightStyle = highlightFont.Style;
            FontColorHelper.HighlightColor = highlightColor;
            FontColorHelper.ReferenceStyle = refererFont.Style;
            FontColorHelper.ReferenceColor = refererColor;
            FontColorHelper.FailureStyle = errorFont.Style;
            FontColorHelper.FailureColor = errorColor;
            FontColorHelper.NewCommentsStyle = newCommentsFont.Style;
            FontColorHelper.NewCommentsColor = newCommentsColor;

            ResetTreeViewFontAndColor();
            ResetListViewFontAndColor();
            ResetListViewOutlookFontAndColor();
        }

        // helper
        private void ResetTreeViewFontAndColor()
        {
            try
            {
                treeFeeds.BeginUpdate();
                //if (Utils.VisualStylesEnabled) {
                treeFeeds.Font = FontColorHelper.NormalFont;
                //this.treeFeeds.Appearance.FontData.Name = FontColorHelper.NormalFont.Name;
                //	} else {
                //		this.treeFeeds.Font = FontColorHelper.FontWithMaxWidth();
                //	}

                // we measure for a bigger sized text, because we need a fixed global right images size
                // for all right images to extend the clickable area
                using (Graphics g = CreateGraphics())
                {
                    SizeF sz = g.MeasureString("(99999)", FontColorHelper.UnreadFont);
                    Size gz = new Size((int) sz.Width, (int) sz.Height);

                    // adjust global sizes
                    if (! treeFeeds.RightImagesSize.Equals(gz))
                        treeFeeds.RightImagesSize = gz;
                }

                treeFeeds.Override.NodeAppearance.ForeColor = FontColorHelper.NormalColor;

                // now iterate and update the single nodes
                if (treeFeeds.Nodes.Count > 0)
                {
                    for (int i = 0; i < _roots.Length; i++)
                    {
                        TreeFeedsNodeBase startNode = _roots[i];
                        if (null != startNode)
                            WalkdownThenRefreshFontColor(startNode);
                    }
                }

                //	PopulateTreeRssSearchScope();	causes threading issues since not on UI thread
            }
            finally
            {
                treeFeeds.EndUpdate();
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then reset all font/colors.
        /// </summary>
        /// <param name="startNode">Node to start with.</param>
        private static void WalkdownThenRefreshFontColor(TreeFeedsNodeBase startNode)
        {
            if (startNode == null) return;

            if (startNode.UnreadCount > 0)
            {
                FontColorHelper.CopyFromFont(startNode.Override.NodeAppearance.FontData, FontColorHelper.UnreadFont);
                startNode.ForeColor = FontColorHelper.UnreadColor;
            }
            else if (startNode.HighlightCount > 0)
            {
                FontColorHelper.CopyFromFont(startNode.Override.NodeAppearance.FontData, FontColorHelper.HighlightFont);
                startNode.ForeColor = FontColorHelper.HighlightColor;
            }
            else if (startNode.ItemsWithNewCommentsCount > 0)
            {
                FontColorHelper.CopyFromFont(startNode.Override.NodeAppearance.FontData, FontColorHelper.NewCommentsFont);
                startNode.ForeColor = FontColorHelper.NewCommentsColor;
            }
            else
            {
                FontColorHelper.CopyFromFont(startNode.Override.NodeAppearance.FontData, FontColorHelper.NormalFont);
                startNode.ForeColor = FontColorHelper.NormalColor;
            }

            for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
            {
                WalkdownThenRefreshFontColor(child);
            }
        }

        private void ResetListViewFontAndColor()
        {
            listFeedItems.BeginUpdate();

            listFeedItems.Font = FontColorHelper.NormalFont;
            listFeedItems.ForeColor = FontColorHelper.NormalColor;

            // now iterate and update the single items
            for (int i = 0; i < listFeedItems.Items.Count; i++)
            {
                ThreadedListViewItem lvi = listFeedItems.Items[i];
                // apply leading fonts/colors
                ApplyStyles(lvi);
            }
            listFeedItems.EndUpdate();
        }

        private void ResetListViewOutlookFontAndColor()
        {
            listFeedItemsO.BeginUpdate();

            listFeedItemsO.Font = FontColorHelper.NormalFont;
            int hh = (int) listFeedItemsO.CreateGraphics().MeasureString("W", listFeedItemsO.Font).Height;
            int hh2 = 2 + hh + 3 + hh + 2;
            listFeedItemsO.Override.ItemHeight = hh2%2 == 0 ? hh2 + 1 : hh2;
                //BUGBUG: if the height is an even number, it uses a bad rectangle
            listFeedItemsO.ColumnSettings.ColumnSets[0].Columns[0].LayoutInfo.PreferredCellSize =
                new Size(listFeedItemsO.Width, listFeedItemsO.Override.ItemHeight);
            hh2 = 5 + hh;
            UltraTreeExtended.COMMENT_HEIGHT = hh2%2 == 0 ? hh2 + 1 : hh2;
            hh2 = hh + 9;
            UltraTreeExtended.DATETIME_GROUP_HEIGHT = hh2%2 == 0 ? hh2 + 1 : hh2;

            // now iterate and update the single items
            //Root DateTime Nodes
            for (int i = 0; i < listFeedItemsO.Nodes.Count; i++)
            {
                listFeedItemsO.Nodes[i].Override.ItemHeight = UltraTreeExtended.DATETIME_GROUP_HEIGHT;
                //NewsItem Nodes
                for (int j = 0; j < listFeedItemsO.Nodes[i].Nodes.Count; j++)
                {
                    //Already done

                    //Comments
                    for (int k = 0; k < listFeedItemsO.Nodes[i].Nodes[j].Nodes.Count; k++)
                    {
                        //Comments
                        listFeedItemsO.Nodes[i].Nodes[j].Nodes[k].Override.ItemHeight = UltraTreeExtended.COMMENT_HEIGHT;
                    }
                }
            }
            listFeedItemsO.EndUpdate();
        }

        public void CmdExecuteSearchEngine(ICommand sender)
        {
            if (sender is AppButtonToolCommand)
            {
                AppButtonToolCommand cmd = (AppButtonToolCommand) sender;
                SearchEngine se = (SearchEngine) cmd.Tag;
                StartSearch(se);
            }
        }

        public void CmdSearchGo(ICommand sender)
        {
            StartSearch(null);
        }

        /// <summary>
        /// Initiate the search process
        /// </summary>
        /// <param name="thisEngine">A specific engine to use. If null, all
        /// active engines are started.</param>
        public void StartSearch(SearchEngine thisEngine)
        {
            string phrase = WebSearchText;

            if (phrase.Length > 0)
            {
                if (!SearchComboBox.Items.Contains(phrase))
                    SearchComboBox.Items.Add(phrase);

                if (thisEngine != null)
                {
                    string s = thisEngine.SearchLink;
                    if (s != null && s.Length > 0)
                    {
                        try
                        {
                            s = String.Format(new UrlFormatter(), s, phrase);
                        }
                        catch (Exception fmtEx)
                        {
                            _log.Error("Invalid search phrase placeholder, or no placeholder '{0}'", fmtEx);
                            return;
                        }
                        if (thisEngine.ReturnRssResult)
                        {
                            owner.BackgroundDiscoverFeedsHandler.Add(
                                new DiscoveredFeedsInfo(s, thisEngine.Title + ": '" + phrase + "'", s));
                            AsyncStartRssRemoteSearch(phrase, s, thisEngine.MergeRssResult, true);
                        }
                        else
                        {
                            DetailTabNavigateToUrl(s, thisEngine.Title, owner.SearchEngineHandler.NewTabRequired, true);
                        }
                    }
                }
                else
                {
                    // all
                    bool isFirstItem = true;
                    int engineCount = 0;
                    foreach (SearchEngine engine in owner.SearchEngineHandler.Engines)
                    {
                        if (engine.IsActive)
                        {
                            engineCount++;
                            string s = engine.SearchLink;
                            if (s != null && s.Length > 0)
                            {
                                try
                                {
                                    s = String.Format(new UrlFormatter(), s, phrase);
                                }
                                catch (Exception fmtEx)
                                {
                                    _log.Error("Invalid search phrase placeholder, or no placeholder '{0}'", fmtEx);
                                    continue;
                                }
                                if (engine.ReturnRssResult)
                                {
                                    owner.BackgroundDiscoverFeedsHandler.Add(
                                        new DiscoveredFeedsInfo(s, engine.Title + ": '" + phrase + "'", s));
                                    AsyncStartRssRemoteSearch(phrase, s, engine.MergeRssResult, isFirstItem);
                                    isFirstItem = false;
                                }
                                else
                                {
                                    DetailTabNavigateToUrl(s, engine.Title, true, engineCount > 1);
                                    Application.DoEvents();
                                }
                            }
                        }
                    }
                } // end all active engines
            }
        }

        public void CmdOpenConfigIdentitiesDialog(ICommand sender)
        {
            IdentityNewsServerManager imng = new IdentityNewsServerManager(owner);
            imng.ShowIdentityDialog(this);
        }

        public void CmdOpenConfigNntpServerDialog(ICommand sender)
        {
            IdentityNewsServerManager imng = new IdentityNewsServerManager(owner);
            imng.ShowNewsServerSubscriptionsDialog(this);
        }

        /// <summary>
        /// Gets or sets the current internal selected tree node (FeedTreeNodeBase).
        /// </summary>
        /// <value>an FeedTreeNodeBase instance</value>
        /// <remarks>
        /// If the internal current selected tree node is null, it
        /// try to return the selected node of the TreeView (property SelectedNode).
        /// If it is not null, it will be returned not regarding the current TreeView
        /// selection. This behavior enables to have a context menu related to the current
        /// clicked node item or (if set to null) a context menu related to current TreeView
        /// selection (highlighted).
        /// </remarks>
        public TreeFeedsNodeBase CurrentSelectedFeedsNode
        {
            get
            {
                if (_currentSelectedFeedsNode != null)
                    return _currentSelectedFeedsNode;
                // this may also return null
                return TreeSelectedFeedsNode;
            }
            set
            {
                _currentSelectedFeedsNode = value;
            }
        }

        public TreeFeedsNodeBase TreeSelectedFeedsNode
        {
            get
            {
                if (treeFeeds.SelectedNodes.Count == 0)
                    return null;
                else
                    return (TreeFeedsNodeBase) treeFeeds.SelectedNodes[0];
            }
            set
            {
                if (value.Control != null)
                {
                    listFeedItems.CheckForLayoutModifications();
                    treeFeeds.BeforeSelect -= OnTreeFeedBeforeSelect;
                    treeFeeds.AfterSelect -= OnTreeFeedAfterSelect;
                    treeFeeds.SelectedNodes.Clear();
                    value.Selected = true;
                    value.Control.ActiveNode = value;
                    treeFeeds.BeforeSelect += OnTreeFeedBeforeSelect;
                    treeFeeds.AfterSelect += OnTreeFeedAfterSelect;
                    listFeedItems.FeedColumnLayout = GetFeedColumnLayout(value);
                }
            }
        }


        /// <summary>
        /// Returns the number of selected list view items
        /// </summary>
        public int NumSelectedListViewItems
        {
            get
            {
                return listFeedItems.SelectedItems.Count;
            }
        }

        /// <summary>
        /// Returns the Subscription Tree Image List.
        /// </summary>
        public ImageList TreeImageList
        {
            get
            {
                return _treeImages;
            }
        }

        /// <summary>
        /// Gets or sets the current internal selected feed story item (NewsItem)
        /// </summary>
        /// <value>an NewsItem instance</value>
        /// <remarks>If the internal current NewsItem is null, it
        /// try to return the first item found on the ListView selctedItems list.
        /// If it is not null, it will be returned not regarding the current ListView
        /// selection. This behavior enables to have a context menu related to the current
        /// clicked item row or (if set to null) a context menu related to current ListView
        /// selection (highlighted).</remarks>
        public INewsItem CurrentSelectedFeedItem
        {
            get
            {
                if (_currentNewsItem != null)
                    return _currentNewsItem;
                if (listFeedItems.Visible && (listFeedItems.SelectedItems.Count > 0))
                {
                    return (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key;
                }
                if (listFeedItemsO.Visible && (listFeedItemsO.SelectedItems.Count > 0))
                {
                    return (INewsItem) ((ThreadedListViewItem) listFeedItemsO.SelectedItems[0]).Key;
                }
                return null;
            }
            set
            {
                _currentNewsItem = value;
            }
        }

        /// <summary>
        /// Gets the UI Result Dispatcher timer.
        /// </summary>
        internal Timer ResultDispatcher
        {
            get
            {
                return _timerDispatchResultsToUI;
            }
        }

        /// <summary>
        /// Gets a value indicating whether shutdown is in progress.
        /// </summary>
        /// <value><c>true</c> if [shutdown in progress]; otherwise, <c>false</c>.</value>
        internal bool ShutdownInProgress
        {
            get
            {
                return _forceShutdown;
            }
        }

        #endregion

        #region private helper routines

        private static string CurrentToolbarsVersion
        {
            get
            {
                return
                    String.Format("{0}.{1}", StateSerializationHelper.InfragisticsToolbarVersion,
                                  _currentToolbarsVersion);
            }
        }

#if USE_UltraDockManager
		private string CurrentDockingVersion {
			get { 
				return String.Format("{0}.{1}",  StateSerializationHelper.InfragisticsDockingVersion, _currentDockingVersion);
			}
		}
#endif

        private static string CurrentExplorerBarVersion
        {
            get
            {
                return
                    String.Format("{0}.{1}", StateSerializationHelper.InfragisticsExplorerBarVersion,
                                  _currentExplorerBarVersion);
            }
        }

        private ComboBox _urlComboBox = null;

        internal ComboBox UrlComboBox
        {
            get
            {
                if (_urlComboBox == null)
                {
                    Debug.Assert(false, "UrlComboBox control not yet initialized (by ToolbarHelper)");
                }
                return _urlComboBox;
            }
            set
            {
                _urlComboBox = value;
            }
        }

        private ComboBox _searchComboBox = null;

        internal ComboBox SearchComboBox
        {
            get
            {
                if (_searchComboBox == null)
                {
                    Debug.Assert(false, "SearchComboBox control not yet initialized (by ToolbarHelper)");
                }
                return _searchComboBox;
            }
            set
            {
                _searchComboBox = value;
            }
        }

        internal void SetTitleText(string newTitle)
        {
            if (newTitle != null && newTitle.Trim().Length != 0)
                Text = RssBanditApplication.CaptionOnly + " - " + newTitle;
            else
                Text = RssBanditApplication.CaptionOnly;

            if (0 != (owner.InternetConnectionState & INetState.Offline))
                Text += " " + SR.MenuAppInternetConnectionModeOffline;
        }

        internal void SetDetailHeaderText(TreeFeedsNodeBase node)
        {
            if (node != null && ! string.IsNullOrEmpty(node.Text))
            {
                if (node.UnreadCount > 0)
                    detailHeaderCaption.Text = String.Format("{0} ({1})", node.Text, node.UnreadCount);
                else
                    detailHeaderCaption.Text = node.Text;
                detailHeaderCaption.Appearance.Image = node.ImageResolved;
            }
            else
            {
                detailHeaderCaption.Text = SR.DetailHeaderCaptionWelcome;
                detailHeaderCaption.Appearance.Image = null;
            }
        }

        protected void AddUrlToHistory(string newUrl)
        {
            if (!newUrl.Equals("about:blank"))
            {
                UrlComboBox.Items.Remove(newUrl);
                UrlComboBox.Items.Insert(0, newUrl);
            }
        }

        internal TreeFeedsNodeBase GetRoot(RootFolderType rootFolder)
        {
            if (treeFeeds.Nodes.Count > 0)
            {
                return _roots[(int) rootFolder];
            }
            return null;
        }

        internal RootFolderType GetRoot(TreeFeedsNodeBase feedsNode)
        {
            if (feedsNode == null)
                throw new ArgumentNullException("feedsNode");

            if (feedsNode.Type == FeedNodeType.Root || feedsNode.Parent == null)
            {
                for (int i = 0; i < _roots.GetLength(0); i++)
                {
                    if (feedsNode == _roots[i])
                        return (RootFolderType) i;
                }
            }
            else if (feedsNode.Parent != null)
            {
                return GetRoot(feedsNode.Parent);
            }
            return RootFolderType.MyFeeds;
        }

        protected TreeFeedsNodeBase CurrentDragNode
        {
            get
            {
                return _currentDragFeedsNode;
            }
            set
            {
                _currentDragFeedsNode = value;
            }
        }

        protected TreeFeedsNodeBase CurrentDragHighlightNode
        {
            get
            {
                return _currentDragHighlightFeedsNode;
            }
            set
            {
                if (_currentDragHighlightFeedsNode != null && _currentDragHighlightFeedsNode != value)
                {
                    // unhighlight old one
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.ResetBackColor();
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.ResetForeColor();
                    //if (_timerTreeNodeExpand.Enabled) 
                    //	_timerTreeNodeExpand.Stop();
                }
                _currentDragHighlightFeedsNode = value;
                if (_currentDragHighlightFeedsNode != null)
                {
                    // highlight new one
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.BackColor = SystemColors.Highlight;
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.ForeColor = SystemColors.HighlightText;
                    //if (_currentDragHighlightFeedsNode.Nodes.Count > 0 && !_currentDragHighlightFeedsNode.Expanded)
                    //	_timerTreeNodeExpand.Start();
                }
            }
        }

        private static void SetFocus2WebBrowser(HtmlControl theBrowser)
        {
            if (theBrowser == null)
                return;
            theBrowser.Focus();
        }

        internal void SetSubscriptionNodeState(INewsFeed f, TreeFeedsNodeBase feedsNode, FeedProcessingState state)
        {
            if (f == null || feedsNode == null) return;
            if (RssHelper.IsNntpUrl(f.link))
            {
                SetNntpNodeState(f, feedsNode, state);
            }
            else
            {
                SetFeedNodeState(f, feedsNode, state);
            }
        }

        private static void SetNntpNodeState(INewsFeed f, TreeFeedsNodeBase feedsNode, FeedProcessingState state)
        {
            if (f == null || feedsNode == null) return;
            switch (state)
            {
                case FeedProcessingState.Normal:
                    if (f.refreshrateSpecified && f.refreshrate <= 0)
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpDisabled;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.NntpDisabledSelected;
                    }
                    else if (f.authUser != null || f.link.StartsWith(NntpWebRequest.NntpsUriScheme))
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpSecured;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.NntpSecuredSelected;
                    }
                    else
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.Nntp;
                        feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.NntpSelected;
                    }
                    break;
                case FeedProcessingState.Failure:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpFailure;
                    feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.NntpFailureSelected;
                    break;
                case FeedProcessingState.Updating:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpUpdating;
                    feedsNode.Override.SelectedNodeAppearance.Image =
                        Resource.SubscriptionTreeImage.NntpUpdatingSelected;
                    break;
                default:
                    Trace.WriteLine("Unhandled/unknown FeedProcessingState: " + state);
                    break;
            }
        }

        private void SetFeedNodeState(INewsFeed f, TreeFeedsNodeBase feedsNode, FeedProcessingState state)
        {
            if (f == null || feedsNode == null) return;
            switch (state)
            {
                case FeedProcessingState.Normal:
                    if (!string.IsNullOrEmpty(f.favicon) && feedsNode.HasCustomIcon && owner.Preferences.UseFavicons)
                    {
                        feedsNode.SetIndividualImage(null); //revert to original images
                    }
                    else if (f.refreshrateSpecified && f.refreshrate <= 0)
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedDisabled;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.FeedDisabledSelected;
                    }
                    else if (f.authUser != null || f.link.StartsWith("https"))
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedSecured;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.FeedSecuredSelected;
                    }
                    else
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.Feed;
                        feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.FeedSelected;
                    }
                    break;
                case FeedProcessingState.Failure:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedFailure;
                    feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.FeedFailureSelected;
                    break;
                case FeedProcessingState.Updating:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedUpdating;
                    feedsNode.Override.SelectedNodeAppearance.Image =
                        Resource.SubscriptionTreeImage.FeedUpdatingSelected;
                    break;
                default:
                    Trace.WriteLine("Unhandled/unknown FeedProcessingState: " + state);
                    break;
            }
        }

        /// <summary>
        /// Populates the list view with the items for the feed represented by 
        /// the tree node then checks to see if any are unread. If this is the 
        /// case then the unread item is given focus.  
        /// </summary>
        /// <param name="tn"></param>
        /// <returns>True if an unread item exists for this feed and false otherwise</returns>
        private bool FindNextUnreadItem(TreeFeedsNodeBase tn)
        {
            INewsFeed f = null;
            bool repopulated = false, isTopLevel = true;
            ListViewItem foundLVItem = null;

            //long measure = 0;	// used only for profiling...

            if (tn.Type == FeedNodeType.Feed)
                f = owner.GetFeed(tn.DataKey);

            bool containsUnread = ((f != null && f.containsNewMessages) ||
                                   (tn == TreeSelectedFeedsNode && tn.UnreadCount > 0));

            if (containsUnread)
            {
                if (tn != TreeSelectedFeedsNode && f != null)
                {
                    containsUnread = false;
                    IList<INewsItem> items = owner.FeedHandler.GetCachedItemsForFeed(f.link);

                    for (int i = 0; i < items.Count; i++)
                    {
                        INewsItem item = items[i];
                        if (!item.BeenRead)
                        {
                            containsUnread = true;
                            break;
                        }
                    }

                    if (containsUnread)
                    {
                        TreeFeedsNodeBase tnSelected = TreeSelectedFeedsNode;
                        if (tnSelected == null)
                            tnSelected = GetRoot(RootFolderType.MyFeeds);

                        if (tnSelected.Type == FeedNodeType.SmartFolder || tnSelected.Type == FeedNodeType.Finder ||
                            tnSelected.Type == FeedNodeType.Root ||
                            (tnSelected != tn && tnSelected.Type == FeedNodeType.Feed) ||
                            (tnSelected.Type == FeedNodeType.Category && !NodeIsChildOf(tn, tnSelected)))
                        {
                            //ProfilerHelper.StartMeasure(ref measure);

                            //re-populate list view with items for feed with unread messages, if it is not
                            // the current displayed:
                            //							this.treeFeeds.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
                            //							this.treeFeeds.BeforeSelect -= new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);									
                            TreeSelectedFeedsNode = tn;
                            CurrentSelectedFeedsNode = null; // reset
                            //							this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);
                            //							this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																
                            PopulateListView(tn, items, true);
                            repopulated = true;

                            //_log.Info("Profile FindNextUnreadItem() Re-Populate listview took: "+ProfilerHelper.StopMeasureString(measure));
                        }
                    }
                    else
                    {
                        f.containsNewMessages = false; // correct the property value
                    }
                }
            } //if(f.containsNewMessages)

            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                ThreadedListViewItem tlvi = (ThreadedListViewItem) listFeedItems.SelectedItems[i];
                if ((tlvi != null) && (tlvi.IndentLevel != 0))
                {
                    isTopLevel = false;
                    break;
                }
            }

            //select a list item that hasn't been read. As an optimization, we don't 
            //walk the list view if we are on a top level listview item and there are no
            //unread posts. 
            if ((!isTopLevel) || containsUnread)
            {
                foundLVItem = FindUnreadListViewItem();
            }

            if (foundLVItem != null)
            {
                MoveFeedDetailsToFront();

                listFeedItems.BeginUpdate();
                listFeedItems.SelectedItems.Clear();
                foundLVItem.Selected = true;
                foundLVItem.Focused = true;
                htmlDetail.Activate(); // set focus to html after doc is loaded
                OnFeedListItemActivate(null, EventArgs.Empty); //pass nulls because I don't use params
                SetTitleText(tn.Text);
                SetDetailHeaderText(tn);
                foundLVItem.Selected = true;
                foundLVItem.Focused = true;
                listFeedItems.Focus();
                listFeedItems.EnsureVisible(foundLVItem.Index);
                listFeedItems.EndUpdate();

                //select new position in tree view based on feed with unread messages.
                if (TreeSelectedFeedsNode != tn && repopulated)
                {
                    //we unregister event here to avoid OnTreeFeedAfterSelect() being invoked
                    //					this.treeFeeds.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
                    //					this.treeFeeds.BeforeSelect -= new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																							
                    //					treeFeeds.BeginUpdate();
                    SelectNode(tn);
                    //					treeFeeds.EndUpdate();
                    //					this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);
                    //					this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																
                }
                return true;
            }

            return false;
        }

        private ThreadedListViewItem FindUnreadListViewItem()
        {
            bool inComments = false;

            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                ThreadedListViewItem tlvi = (ThreadedListViewItem) listFeedItems.SelectedItems[i];
                if ((tlvi != null) && (tlvi.IsComment))
                {
                    inComments = true;
                    break;
                }
            }


            if (listFeedItems.Items.Count == 0)
                return null;

            NewsItem compareItem = null;
            ThreadedListViewItem foundLVItem = null;

            int pos = 0, incrementor = 1;

            if ((!inComments) && (listFeedItems.SortManager.SortOrder == SortOrder.Descending))
            {
                pos = listFeedItems.Items.Count - 1; // at the end
                incrementor = -1; // decrement
            }

            while (pos >= 0 && pos < listFeedItems.Items.Count)
            {
                // in correct range

                ThreadedListViewItem lvi = listFeedItems.Items[pos];
                NewsItem item = lvi.Key as NewsItem;

                // find the oldest unread item
                if (item != null && !item.BeenRead)
                {
                    // item can be null for temp entries like "Load comments,..."
                    if (compareItem == null) compareItem = item;
                    if (foundLVItem == null) foundLVItem = lvi;

                    if (!(listFeedItems.SortManager.GetComparer() is ThreadedListViewDateTimeItemComparer))
                    {
                        if (DateTime.Compare(item.Date, compareItem.Date) < 0)
                        {
                            // worst case: compare all unread
                            // instance item.Date smaller than compareItem.Date. Found one:
                            compareItem = item; // item to compare to 
                            foundLVItem = lvi; // corresponding ListViewItem
                        }
                    }
                    else
                    {
                        // simply the next
                        foundLVItem = lvi; // corresponding ListViewItem
                        break;
                    }
                }

                pos += incrementor; // decrement or increment
            }

            return foundLVItem;
        }

        private void SelectNode(TreeFeedsNodeBase feedsNode)
        {
            TreeSelectedFeedsNode = feedsNode;
            feedsNode.BringIntoView();
            if (feedsNode.Parent != null) feedsNode.Parent.BringIntoView();
        }

        /// <summary>
        /// From the startNode, this function returns the next
        /// FeedNode with UnreadCount > 0 that is hirarchically below the startNode.
        /// </summary>
        /// <param name="startNode">the Node to start with</param>
        /// <returns>FeedTreeNodeBase found or null</returns>
        /// <param name="ignoreStartNode"></param>
        private static TreeFeedsNodeBase NextNearFeedNode(TreeFeedsNodeBase startNode, bool ignoreStartNode)
        {
            TreeFeedsNodeBase found = null;

            if (!ignoreStartNode)
            {
                if (startNode.Type == FeedNodeType.Feed) return startNode;
            }

            // walk childs, go down
            for (TreeFeedsNodeBase sibling = startNode.FirstNode;
                 sibling != null && found == null;
                 sibling = sibling.NextNode)
            {
                if (sibling.Type == FeedNodeType.Feed) return sibling;
                if (sibling.FirstNode != null) // childs?
                    found = NextNearFeedNode(sibling.FirstNode, false);
            }
            if (found != null) return found;

            // walk next siblings. If they have childs, go down
            for (TreeFeedsNodeBase sibling = (ignoreStartNode ? startNode.NextNode : startNode.FirstNode);
                 sibling != null && found == null;
                 sibling = sibling.NextNode)
            {
                if (sibling.Type == FeedNodeType.Feed) return sibling;
                if (sibling.FirstNode != null) // childs?
                    found = NextNearFeedNode(sibling.FirstNode, false);
            }
            if (found != null) return found;
            if (startNode.Parent == null) return null; // top of tree

            // no sibling, no Feed childs.
            // go upwards, as long as the parent itself is lastNode
            for (startNode = startNode.Parent;
                 startNode != null && startNode.NextNode == null;
                 startNode = startNode.Parent)
            {
                // nix to do here
            }
            if (startNode == null) return null;

            // no walk next parent siblings. 
            for (TreeFeedsNodeBase parentSibling = startNode.NextNode;
                 parentSibling != null && found == null;
                 parentSibling = parentSibling.NextNode)
            {
                if (parentSibling.Type == FeedNodeType.Feed) return parentSibling;
                if (parentSibling.FirstNode != null) // childs?
                    found = NextNearFeedNode(parentSibling.FirstNode, false);
            }

            return found;
        }


        /// <summary>
        /// Moves from the currently selected item to the next unread item. 
        /// If no unread item is left then this method does nothing.
        /// </summary>
        public void MoveToNextUnreadItem()
        {
            TreeFeedsNodeBase startNode = null, foundFeedsNode, rootNode = GetRoot(RootFolderType.MyFeeds);
            bool unreadFound = false;

            if (listFeedItems.Items.Count > 0)
            {
                startNode = TreeSelectedFeedsNode;
                if (startNode != null && startNode.UnreadCount > 0)
                {
                    unreadFound = FindNextUnreadItem(startNode);
                    if (!unreadFound)
                    {
                        startNode = null;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (startNode == null)
                startNode = CurrentSelectedFeedsNode;

            if (startNode != null && !NodeIsChildOf(startNode, rootNode))
                startNode = null;

            if (startNode == null)
                startNode = rootNode;


            if (startNode.Type == FeedNodeType.Feed)
            {
                MoveFeedDetailsToFront();

                if (FindNextUnreadItem(startNode))
                {
                    unreadFound = true;
                }
            }


            if (!unreadFound)
            {
                // look for next near down feed node
                foundFeedsNode = NextNearFeedNode(startNode, true);
                while (foundFeedsNode != null && !unreadFound)
                {
                    if (FindNextUnreadItem(foundFeedsNode))
                    {
                        unreadFound = true;
                    }
                    foundFeedsNode = NextNearFeedNode(foundFeedsNode, true);
                }
            }

            if (!unreadFound && startNode != GetRoot(RootFolderType.MyFeeds))
            {
                // if not already applied,
                // look for next near down feed node from top of tree
                foundFeedsNode = NextNearFeedNode(GetRoot(RootFolderType.MyFeeds), true);
                while (foundFeedsNode != null && !unreadFound)
                {
                    if (FindNextUnreadItem(foundFeedsNode))
                    {
                        unreadFound = true;
                    }
                    foundFeedsNode = NextNearFeedNode(foundFeedsNode, true);
                }
            }

            if (!unreadFound)
            {
                if (owner.StateHandler.NewsHandlerState == NewsHandlerState.Idle)
                    SetGuiStateFeedback(SR.GUIStatusNoUnreadFeedItemsLeft, ApplicationTrayState.NormalIdle);
            }
        }

        /// <summary>
        /// Help to simply serialize a bounds rect.
        /// </summary>
        /// <param name="b"></param>
        /// <returns>A ';' separated string: "X;Y;Width;Height".</returns>
        private static string BoundsToString(Rectangle b)
        {
            return string.Format("{0};{1};{2};{3}", b.X, b.Y, b.Width, b.Height);
        }

        private static Rectangle StringToBounds(string b)
        {
            string[] ba = b.Split(new char[] {';'});
            Rectangle r = Rectangle.Empty;
            if (ba.GetLength(0) == 4)
            {
                try
                {
                    r = new Rectangle(Int32.Parse(ba[0]), Int32.Parse(ba[1]), Int32.Parse(ba[2]), Int32.Parse(ba[3]));
                }
                catch
                {
                }
            }
            return r;
        }

        /// <summary>
        /// Helper method to populate SmartFolders.
        /// </summary>
        /// <param name="feedNode">The tree node which represents the feed in the tree view</param>
        /// <param name="updateGui">Indicates whether the UI should be altered when the download is completed 
        /// or not. Basically if this flag is true then the list view and browser pane are updated while 
        /// they remain unchanged if this flag is false. </param>
        public void PopulateSmartFolder(TreeFeedsNodeBase feedNode, bool updateGui)
        {
            IList<INewsItem> items;
            ISmartFolder isFolder = feedNode as ISmartFolder;

            if (isFolder == null)
                return;

            items = isFolder.Items;

            //Ensure we update the UI in the correct thread. Since this method is likely 
            //to have been called from a thread that is not the UI thread we should ensure 
            //that calls to UI components are actually made from the UI thread or marshalled
            //accordingly. 			

            if (updateGui || TreeSelectedFeedsNode == feedNode)
            {
                INewsItem itemSelected = null;
                if (listFeedItems.SelectedItems.Count > 0)
                    itemSelected = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key;

                // call them sync., because we want to re-set the previous selected item
                InvokeOnGuiSync(delegate
                {
                    PopulateListView(feedNode, items, true, false, feedNode);
                });

                if (updateGui)
                {
                    htmlDetail.Clear(); //clear browser pane 
                    if (itemSelected == null || listFeedItems.Items.Count == 0)
                    {
                        CurrentSelectedFeedItem = null;
                    }
                    else
                        ReSelectListViewItem(itemSelected);
                }
            }
        }

        /// <summary>
        /// Helper method to populate Aggregated Folders.
        /// </summary>
        /// <param name="node">The tree node which represents the feed in the tree view</param>
        /// <param name="updateGui">Indicates whether the UI should be altered when the download is completed 
        /// or not. Basically if this flag is true then the list view and browser pane are updated while 
        /// they remain unchanged if this flag is false. </param>
        public void PopulateFinderNode(FinderNode node, bool updateGui)
        {
            if (node == null)
                return;

            //Ensure we update the UI in the correct thread. Since this method is likely 
            //to have been called from a thread that is not the UI thread we should ensure 
            //that calls to UI components are actually made from the UI thread or marshalled
            //accordingly. 
            if (updateGui || TreeSelectedFeedsNode == node)
            {
                INewsItem itemSelected = null;
                if (listFeedItems.SelectedItems.Count > 0)
                    itemSelected = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key;

                // now the FinderNode handle refresh of the read state only, so we need to initiate a new search again...:
                node.AnyUnread = false;
                node.Clear();

                //check if Web search or local search
                if (!string.IsNullOrEmpty(node.Finder.ExternalSearchUrl))
                {
                    StartRssRemoteSearch(node.Finder.ExternalSearchUrl, node);
                }
                else
                {
                    AsyncStartNewsSearch(node);
                }


                IList<INewsItem> items = node.Items;

                // call them sync., because we want to re-set the previous selected item
                InvokeOnGuiSync(delegate
                {
                    PopulateListView(node, items, true, true, node);
                });

                if (updateGui)
                {
                    if (itemSelected != null)
                    {
                        //clear browser pane 
                        CurrentSelectedFeedItem = null;
                        htmlDetail.Clear();
                    }
                    else
                        ReSelectListViewItem(itemSelected);
                }
            }
        }

        private ThreadedListViewItem CreateThreadedLVItem(INewsItem newsItem, bool hasChilds, int imgOffset,
                                                          ColumnKeyIndexMap colIndex, bool authorInTopicColumn)
        {
            string[] lvItems = new string[colIndex.Count];

            foreach (string colKey in colIndex.Keys)
            {
                lvItems[colIndex[colKey]] = String.Empty; // init
                switch ((NewsItemSortField) Enum.Parse(typeof (NewsItemSortField), colKey, true))
                {
                    case NewsItemSortField.Title:
                        lvItems[colIndex[colKey]] = StringHelper.ShortenByEllipsis(newsItem.Title, MaxHeadlineWidth);
                        break;
                    case NewsItemSortField.Subject:
                        if (authorInTopicColumn && !colIndex.ContainsKey("Author"))
                        {
                            lvItems[colIndex[colKey]] = newsItem.Author;
                        }
                        else
                        {
                            lvItems[colIndex[colKey]] = newsItem.Subject;
                        }
                        break;
                    case NewsItemSortField.FeedTitle:
                        INewsFeed f = newsItem.Feed;
                        //if we are in a Smart Folder then use the original title of the feed 
                        string feedUrl = GetOriginalFeedUrl(newsItem);
                        if ((feedUrl != null) && owner.FeedHandler.IsSubscribed(feedUrl))
                        {
                            f = owner.FeedHandler.GetFeeds()[feedUrl];
                        }

                        lvItems[colIndex[colKey]] = HtmlHelper.HtmlDecode(f.title);
                        break;
                    case NewsItemSortField.Author:
                        lvItems[colIndex[colKey]] = newsItem.Author;
                        break;
                    case NewsItemSortField.Date:
                        lvItems[colIndex[colKey]] = newsItem.Date.ToLocalTime().ToString();
                        break;
                    case NewsItemSortField.CommentCount:
                        if (newsItem.CommentCount != NewsItem.NoComments)
                            lvItems[colIndex[colKey]] = newsItem.CommentCount.ToString();
                        break;
                    case NewsItemSortField.Enclosure: //TODO: use states. Now it is simply a counter
                        if (null != newsItem.Enclosures && newsItem.Enclosures.Count > 0)
                            lvItems[colIndex[colKey]] = newsItem.Enclosures.Count.ToString();
                                // state should be ("None", "Available", "Scheduled", "Downloaded")
                        break;
                    case NewsItemSortField.Flag:
                        if (newsItem.FlagStatus != Flagged.None)
                            lvItems[colIndex[colKey]] = newsItem.FlagStatus.ToString(); //TODO: localize
                        break;
                    default:
                        Trace.Assert(false, "CreateThreadedLVItem::NewsItemSortField NOT handled: " + colKey);
                        break;
                }
            }

            ThreadedListViewItem lvi = new ThreadedListViewItem(newsItem, lvItems);

            if (!newsItem.BeenRead)
                imgOffset++;
            lvi.ImageIndex = imgOffset;

            // apply leading fonts/colors
            ApplyStyles(lvi, newsItem.BeenRead, newsItem.HasNewComments);

            lvi.HasChilds = hasChilds;
            lvi.IsComment = authorInTopicColumn;

            return lvi;
        }

        /// <summary>call it if items are added to the listview only!</summary>
        /// <param name="items"></param>
        private void ApplyNewsItemPropertyImages(IEnumerable<ThreadedListViewItem> items)
        {
            ColumnKeyIndexMap indexMap = listFeedItems.Columns.GetColumnIndexMap();

            bool applyFlags = indexMap.ContainsKey(NewsItemSortField.Flag.ToString());
            bool applyAttachments = indexMap.ContainsKey(NewsItemSortField.Enclosure.ToString());

            if (!applyFlags && !applyAttachments)
                return;

            foreach (ThreadedListViewItem lvi in items)
            {
                NewsItem item = lvi.Key as NewsItem;
                if (item == null) continue;
                if (applyFlags && item.FlagStatus != Flagged.None)
                    ApplyFlagStateTo(lvi, item.FlagStatus, indexMap);
                if (applyAttachments && item.Enclosures != null && item.Enclosures.Count > 0)
                    ApplyAttachmentImageTo(lvi, item.Enclosures.Count, indexMap);
            }
        }

        /// <summary>call it if items are added to the listview only!</summary>
        /// <param name="lvi"></param>
        /// <param name="attachemtCount"></param>
        /// <param name="indexMap"></param>
        private static void ApplyAttachmentImageTo(ThreadedListViewItem lvi, int attachemtCount, ColumnKeyIndexMap indexMap)
        {
            if (lvi == null || lvi.ListView == null)
                return;

            string key = NewsItemSortField.Enclosure.ToString();
            if (! indexMap.ContainsKey(key))
                return;

            string text = (attachemtCount > 0 ? attachemtCount.ToString() : String.Empty);

            if (indexMap[key] > 0)
            {
                lvi.SubItems[indexMap[key]].Text = text;
                if (attachemtCount > 0)
                    lvi.SetSubItemImage(indexMap[key], Resource.NewsItemRelatedImage.Attachment);
            }
            else
            {
                lvi.SubItems[indexMap[key]].Text = text;
                //lvi.SetSubItemImage(indexMap[key], imgIndex);
            }
        }

        /// <summary>call it if items are added to the listview only!</summary>
        /// <param name="lvi"></param>
        /// <param name="flagStatus"></param>
        /// <param name="indexMap"></param>
        private static void ApplyFlagStateTo(ThreadedListViewItem lvi, Flagged flagStatus, ColumnKeyIndexMap indexMap)
        {
            if (lvi == null || lvi.ListView == null)
                return;

            string key = NewsItemSortField.Flag.ToString();
            if (! indexMap.ContainsKey(key))
                return;

            int imgIndex = -1;
            Color bkColor = lvi.BackColor;
            string text = flagStatus.ToString(); //TODO: localize!!!
            switch (flagStatus)
            {
                case Flagged.Complete:
                    imgIndex = Resource.FlagImage.Complete;
                    break;
                case Flagged.FollowUp:
                    imgIndex = Resource.FlagImage.Red;
                    bkColor = Resource.ItemFlagBackground.Red;
                    break;
                case Flagged.Forward:
                    imgIndex = Resource.FlagImage.Blue;
                    bkColor = Resource.ItemFlagBackground.Blue;
                    break;
                case Flagged.Read:
                    imgIndex = Resource.FlagImage.Green;
                    bkColor = Resource.ItemFlagBackground.Green;
                    break;
                case Flagged.Review:
                    imgIndex = Resource.FlagImage.Yellow;
                    bkColor = Resource.ItemFlagBackground.Yellow;
                    break;
                case Flagged.Reply:
                    imgIndex = Resource.FlagImage.Purple;
                    bkColor = Resource.ItemFlagBackground.Purple;
                    break;
                case Flagged.None:
                    //imgIndex is already setup, as is bkColor
                    text = String.Empty;
                    break;
            }

            if (indexMap[key] > 0)
            {
                lvi.SubItems[indexMap[key]].Text = text;
                lvi.SetSubItemImage(indexMap[key], imgIndex);
                lvi.SubItems[indexMap[key]].BackColor = bkColor; // no effect :-( - BUGBUG???
            }
            else
            {
                lvi.SubItems[indexMap[key]].Text = text;
                //lvi.SetSubItemImage(indexMap[key], imgIndex);
                //lvi.SubItems[indexMap[key]].BackColor = bkColor;	// no effect :-( - BUGBUG???
            }
        }

        private static ThreadedListViewItem CreateThreadedLVItemInfo(string infoMessage, bool isError)
        {
            ThreadedListViewItem lvi = new ThreadedListViewItemPlaceHolder(infoMessage);
            if (isError)
            {
                lvi.Font = FontColorHelper.FailureFont;
                lvi.ForeColor = FontColorHelper.FailureColor;
                lvi.ImageIndex = Resource.NewsItemRelatedImage.Failure;
            }
            else
            {
                lvi.Font = FontColorHelper.NormalFont;
                lvi.ForeColor = FontColorHelper.NormalColor;
            }
            lvi.HasChilds = false;

            return lvi;
        }

        /// <summary>
        /// Populates the list view with NewsItem's from the ArrayList. 
        /// </summary>
        /// <param name="associatedFeedsNode">The accociated tree Node</param>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <param name="forceReload">Force reload of the listview</param>
        private void PopulateListView(TreeFeedsNodeBase associatedFeedsNode, IList<INewsItem> list, bool forceReload)
        {
            PopulateListView(associatedFeedsNode, list, forceReload, false, associatedFeedsNode);
        }

        /// <summary>
        /// Populates the list view with NewsItem's from the ArrayList. 
        /// </summary>
        /// <param name="associatedFeedsNode">The accociated tree Node to populate</param>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <param name="forceReload">Force reload of the listview</param>
        /// <param name="categorizedView">True, if the feed title should be appended to
        /// each RSS Item title: "...rss item title... (feed title)"</param>
        /// <param name="initialFeedsNode"></param>
        private void PopulateListView(TreeFeedsNodeBase associatedFeedsNode, IList<INewsItem> list, bool forceReload,
                                      bool categorizedView, TreeFeedsNodeBase initialFeedsNode)
        {
            try
            {
                lock (listFeedItems.Items)
                {
                    if ((initialFeedsNode != null) && TreeSelectedFeedsNode != initialFeedsNode)
                    {
                        return;
                    }
                }

                IList<INewsItem> unread;

                // detect, if we should do a smartUpdate

                lock (listFeedItems.Items)
                {
                    //since this is a multithreaded app there could have been a change since the last 
                    //time we checked this at the beginning of the method due to context switching. 
                    if (TreeSelectedFeedsNode != initialFeedsNode)
                    {
                        return;
                    }

                    if (initialFeedsNode.Type == FeedNodeType.Category)
                    {
                        if (NodeIsChildOf(associatedFeedsNode, initialFeedsNode))
                        {
                            if (forceReload)
                            {
                                EmptyListView();
                                feedsCurrentlyPopulated.Clear();
                            }

                            bool checkForDuplicates = feedsCurrentlyPopulated.ContainsKey(associatedFeedsNode.DataKey);
                            unread = PopulateSmartListView(list, categorizedView, checkForDuplicates);
                            if (!checkForDuplicates)
                                feedsCurrentlyPopulated.Add(associatedFeedsNode.DataKey, null);

                            if (unread.Count != associatedFeedsNode.UnreadCount)
                                UpdateTreeNodeUnreadStatus(associatedFeedsNode, unread.Count);
                        }
                        else if (associatedFeedsNode == initialFeedsNode)
                        {
                            feedsCurrentlyPopulated.Clear();
                            PopulateFullListView(list);
                            if (associatedFeedsNode.DataKey != null)
                                feedsCurrentlyPopulated.Add(associatedFeedsNode.DataKey, null);
                        }
                    }
                    else if (TreeSelectedFeedsNode is UnreadItemsNode)
                    {
                        if (forceReload)
                        {
                            EmptyListView();
                        }

                        PopulateSmartListView(list, categorizedView, true);
                    }
                    else if (TreeSelectedFeedsNode == associatedFeedsNode)
                    {
                        if (forceReload)
                        {
                            unread = PopulateFullListView(list);
                            if (unread.Count != associatedFeedsNode.UnreadCount)
                                UpdateTreeNodeUnreadStatus(associatedFeedsNode, unread.Count);
                        }
                        else
                        {
                            unread = PopulateSmartListView(list, categorizedView, true);
                            if (unread.Count > 0)
                            {
                                int unreadItems = unread.Count;
                                if (categorizedView) // e.g. AggregatedNodes
                                    unreadItems += associatedFeedsNode.UnreadCount;
                                UpdateTreeNodeUnreadStatus(associatedFeedsNode, unreadItems);
                            }
                        }
                    }
                } //lock

                SetGuiStateFeedback(SR.StatisticsItemsDisplayedMessage(listFeedItems.Items.Count));
            }
            catch (Exception ex)
            {
                _log.Error("PopulateListView() failed.", ex);
            }
        }

        /// <summary>
        /// Can be called from another thread to populate the listview in the Gui thread.
        /// </summary>
        /// <param name="associatedFeedsNode"></param>
        /// <param name="list"></param>
        /// <param name="forceReload"></param>
        /// <param name="categorizedView"></param>
        /// <param name="initialFeedsNode"></param>
        public void AsyncPopulateListView(TreeFeedsNodeBase associatedFeedsNode, IList<INewsItem> list, bool forceReload,
                                          bool categorizedView, TreeFeedsNodeBase initialFeedsNode)
        {
            InvokeOnGui(delegate
            {
                PopulateListView(associatedFeedsNode, list, forceReload, categorizedView, initialFeedsNode);
            });
        }

        /// <summary>
        /// Fully populates the list view with NewsItem's from the ArrayList 
        /// (forced reload).
        /// </summary>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <returns>unread items</returns>
        private IList<INewsItem> PopulateFullListView(IList<INewsItem> list)
        {
            ThreadedListViewItem[] aNew = new ThreadedListViewItem[list.Count];

            List<INewsItem> unread = new List<INewsItem>(list.Count);

            ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();
            INewsItemFilter flagFilter = null;

            if (CurrentSelectedFeedsNode is FlaggedItemsNode)
            {
                // do not apply flag filter on Flagged items node(s)
                flagFilter = _filterManager["NewsItemFlagFilter"];
                _filterManager.Remove("NewsItemFlagFilter");
            }

            EmptyListView();

            listFeedItems.BeginUpdate();

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    INewsItem item = list[i];

                    if (!item.BeenRead)
                        unread.Add(item);

                    bool hasRelations;
                    hasRelations = NewsItemHasRelations(item); // here is the bottleneck :-(

                    ThreadedListViewItem newItem = CreateThreadedLVItem(item, hasRelations, Resource.NewsItemImage.DefaultRead, colIndex, false);
                    _filterManager.Apply(newItem);

                    aNew[i] = newItem;
                }

                Array.Sort(aNew, listFeedItems.SortManager.GetComparer());
                listFeedItems.Items.AddRange(aNew);
                ApplyNewsItemPropertyImages(aNew);

                //listFeedItems.EndUpdate();
                if (listFeedItemsO.Visible)
                {
                    listFeedItemsO.AddRange(aNew);
                }
                return unread;
            }
            catch (Exception ex)
            {
                _log.Error("PopulateFullListView exception", ex);
                return unread;
            }
            finally
            {
                listFeedItems.EndUpdate();

                if (flagFilter != null)
                {
                    // add back
                    //flagFilter = _filterManager.Add("NewsItemFlagFilter", flagFilter);
                    _filterManager.Add("NewsItemFlagFilter", flagFilter);
                }
            }
        }

        /// <summary>
        /// Add NewsItem's from the ArrayList to the current displayed ListView. 
        /// This contains usually some items, so we have to insert the new items 
        /// at the correct position(s).
        /// </summary>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <param name="categorizedView">True, if the feed title should be appended to
        /// each RSS Item title: "...rss item title... (feed title)"</param>
        /// <param name="checkDuplicates">If true, we check if a NewsItem is allready populated.
        /// This has a perf. impact, if true!</param>
        /// <returns>unread items</returns>
        public IList<INewsItem> PopulateSmartListView(IList<INewsItem> list, bool categorizedView, bool checkDuplicates)
        {
            List<ThreadedListViewItem> items = new List<ThreadedListViewItem>(listFeedItems.Items.Count);
            List<ThreadedListViewItem> newItems = new List<ThreadedListViewItem>(list.Count);
            List<INewsItem> unread = new List<INewsItem>(list.Count);

            lock (listFeedItems.Items)
            {
                items.AddRange(listFeedItems.Items);
            }

            // column index map
            ColumnKeyIndexMap colIndexes = listFeedItems.Columns.GetColumnIndexMap();

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    INewsItem item = list[i];
                    bool hasRelations = NewsItemHasRelations(item);
                    bool isDuplicate = false;
                    ThreadedListViewItem tlvi = null;

                    if (checkDuplicates)
                    {
                        //lock(listFeedItems.Items) {
                        // look, if it is already there
                        for (int j = 0; j < items.Count; j++)
                        {
                            tlvi = items[j];
                            if (item.Equals(tlvi.Key) && tlvi.IndentLevel == 0)
                            {
                                tlvi.Key = item; // update ref
                                isDuplicate = true;
                                break;
                            }
                        }
                        //}
                    }

                    if (isDuplicate)
                    {
                        // do not create a new one, but check if it has new childs 
                        if (!tlvi.HasChilds && hasRelations)
                            tlvi.HasChilds = hasRelations;
                        ApplyStyles(tlvi); //highlight item if it has new comments						
                    }
                    else
                    {
                        ThreadedListViewItem newItem = CreateThreadedLVItem(item, hasRelations, Resource.NewsItemImage.DefaultRead, colIndexes,
                                                                            false);

                        _filterManager.Apply(newItem);
                        newItems.Add(newItem);
                    }


                    if (!item.BeenRead)
                        unread.Add(item);
                } //for(int i)

                if (newItems.Count > 0)
                {
                    try
                    {
                        listFeedItems.BeginUpdate();

                        lock (listFeedItems.Items)
                        {
                            ThreadedListViewItem[] a = new ThreadedListViewItem[newItems.Count];
                            newItems.CopyTo(a);
                            listFeedItems.ListViewItemSorter = listFeedItems.SortManager.GetComparer();
                            listFeedItems.Items.AddRange(a);
                            if (listFeedItemsO.Visible)
                                listFeedItemsO.AddRange(a);
                            ApplyNewsItemPropertyImages(a);
                            listFeedItems.ListViewItemSorter = null;

                            if (listFeedItems.SelectedItems.Count > 0)
                            {
                                listFeedItems.EnsureVisible(listFeedItems.SelectedItems[0].Index);
                                if (listFeedItemsO.Visible)
                                    listFeedItemsO.GetFromLVI((ThreadedListViewItem) listFeedItems.SelectedItems[0]).
                                        BringIntoView();
                            }
                        }
                    }
                    finally
                    {
                        listFeedItems.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("PopulateSmartListView exception", ex);
            }

            return unread;
        }

        //		private int GetInsertIndexOfItem(ThreadedListViewItem item) {
        //			
        //			if (this._lvSortHelper.Sorting == SortOrder.Ascending) {
        //				for (int i = 0; i < listFeedItems.Items.Count; i++) {
        //					ThreadedListViewItem tlv = (ThreadedListViewItem) listFeedItems.Items[i];
        //					if (tlv.IndentLevel == 0 && this._lvSortHelper.Compare(item, tlv) >= 0)
        //						return i;
        //				} 
        //
        //				return 0;
        //
        //			} else {
        //				for (int i = 0; i < listFeedItems.Items.Count; i++) {
        //					ThreadedListViewItem tlv = (ThreadedListViewItem) listFeedItems.Items[i];
        //					if (tlv.IndentLevel == 0 && this._lvSortHelper.Compare(item, tlv) <= 0)
        //						return i;
        //				}			
        //
        //			}
        //			// mean: the caller should append the item
        //			return listFeedItems.Items.Count;
        //		}

        private bool NewsItemHasRelations(INewsItem item)
        {
            return NewsItemHasRelations(item, new INewsItem[] {});
        }

        private bool NewsItemHasRelations(INewsItem item, IList<INewsItem> itemKeyPath)
        {
            bool hasRelations = false;
            if (item.Feed != null & owner.FeedHandler.IsSubscribed(item.Feed.link))
            {
                hasRelations = owner.FeedHandler.HasItemAnyRelations(item, itemKeyPath);
            }
            if (!hasRelations) hasRelations = (item.HasExternalRelations && owner.InternetAccessAllowed);
            return hasRelations;
        }

        public void BeginLoadCommentFeed(INewsItem item, string ticket, IList<INewsItem> itemKeyPath)
        {
            owner.MakeAndQueueTask(ThreadWorker.Task.LoadCommentFeed,
                                   OnLoadCommentFeedProgress,
                                   item, ticket, itemKeyPath);
        }

        private void OnLoadCommentFeedProgress(object sender, ThreadWorkerProgressArgs args)
        {
            if (args.Exception != null)
            {
                // failure(s)
                args.Cancel = true;
                ExceptionManager.Publish(args.Exception);
                object[] results = (object[]) args.Result;
                string insertionPointTicket = (string) results[2];
                ThreadedListViewItem[] newChildItems =
                    new ThreadedListViewItem[] {CreateThreadedLVItemInfo(args.Exception.Message, true)};
                listFeedItems.InsertItemsForPlaceHolder(insertionPointTicket, newChildItems, false);
                if (listFeedItemsO.Visible && newChildItems.Length > 0)
                {
                    listFeedItemsO.AddRangeComments(newChildItems[0].Parent, newChildItems);
                }
            }
            else if (!args.Done)
            {
                // in progress
                // we already have a "loading ..." text listview item
            }
            else if (args.Done)
            {
                // done
                object[] results = (object[]) args.Result;
                List<INewsItem> commentItems = (List<INewsItem>) results[0];
                INewsItem item = (INewsItem) results[1];
                string insertionPointTicket = (string) results[2];
                IList<INewsItem> itemKeyPath = (IList<INewsItem>) results[3];


                if (item.CommentCount != commentItems.Count)
                {
                    item.CommentCount = commentItems.Count;
                    owner.FeedWasModified(item.Feed, NewsFeedProperty.FeedItemCommentCount);
                }

                commentItems.Sort(RssHelper.GetComparer(false, NewsItemSortField.Date));
                item.SetExternalRelations(commentItems);

                ThreadedListViewItem[] newChildItems = null;

                if (commentItems.Count > 0)
                {
                    ArrayList newChildItemsArray = new ArrayList(commentItems.Count);

                    // column index map
                    ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();

                    for (int i = 0; i < commentItems.Count; i++)
                    {
                        INewsItem o = commentItems[i];
                        if (itemKeyPath != null && itemKeyPath.Contains(o))
                            continue;


                        bool hasRelations = NewsItemHasRelations(o, itemKeyPath);

                        o.BeenRead = tempFeedItemsRead.ContainsKey(RssHelper.GetHashCode(o));
                        ThreadedListViewItem newListItem =
                            CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.CommentRead, colIndex, true);
                        _filterManager.Apply(newListItem);
                        newChildItemsArray.Add(newListItem);
                    } //iterator.MoveNext

                    if (newChildItemsArray.Count > 0)
                    {
                        newChildItems = new ThreadedListViewItem[newChildItemsArray.Count];
                        newChildItemsArray.CopyTo(newChildItems);
                    }
                }

                listFeedItems.InsertItemsForPlaceHolder(insertionPointTicket, newChildItems, false);
                if (listFeedItemsO.Visible && newChildItems.Length > 0)
                {
                    listFeedItemsO.AddRangeComments(newChildItems[0].Parent, newChildItems);
                }
            }
        }

        /// <summary>
        /// Called to refresh the GUI state after refresh of feeds/feed items
        /// </summary>
        public void TriggerGUIStateOnNewFeeds(bool handleNewReceived)
        {
            int unreadFeeds, unreadMessages;
            CountUnread(out unreadFeeds, out unreadMessages);

            if (unreadMessages != 0)
            {
                _timerResetStatus.Stop();
                if (handleNewReceived && unreadMessages > _lastUnreadFeedItemCountBeforeRefresh)
                {
                    string message = SR.GUIStatusNewFeedItemsReceivedMessage(unreadFeeds, unreadMessages);
                    if (Visible)
                    {
                        SetGuiStateFeedback(message, ApplicationTrayState.NewUnreadFeeds);
                    }
                    else
                    {
                        // if invisible (tray only): animate
                        SetGuiStateFeedback(message, ApplicationTrayState.NewUnreadFeedsReceived);
                    }
                    if (owner.Preferences.ShowNewItemsReceivedBalloon &&
                        (SystemTrayOnlyVisible || WindowState == FormWindowState.Minimized))
                    {
                        if (_beSilentOnBalloonPopupCounter <= 0)
                        {
                            message = SR.GUIStatusNewFeedItemsReceivedMessage(
                                unreadFeeds,
                                unreadMessages);
                            _trayAni.ShowBalloon(NotifyIconAnimation.EBalloonIcon.Info, message,
                                                 RssBanditApplication.CaptionOnly + " - " +
                                                 SR.GUIStatusNewFeedItemsReceived);
                        }
                        else
                        {
                            _beSilentOnBalloonPopupCounter--;
                        }
                    }
                }
                else
                {
                    SetGuiStateFeedback(String.Empty, ApplicationTrayState.NewUnreadFeeds);
                }
            }
            else
            {
                SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
            }
        }


        /// <summary>
        /// Updates the comment status for the specified tree node and any related search folders that may 
        /// contain items from this node. 
        /// </summary>
        /// <param name="tn">The tree node whose comment status is being updated</param>
        /// <param name="f">The feed associated with the tree node</param>
        private void UpdateCommentStatus(TreeFeedsNodeBase tn, INewsFeed f)
        {
            IList<INewsItem> itemsWithNewComments = GetFeedItemsWithNewComments(f);
            tn.UpdateCommentStatus(tn, itemsWithNewComments.Count);
            owner.UpdateWatchedItems(itemsWithNewComments);
            WatchedItemsNode.UpdateCommentStatus();
        }

        /// <summary>
        /// Updates the comment status for the specified tree node and any related search folders that may
        /// contain items from this node.
        /// </summary>
        /// <param name="tn">The tree node whose comment status is being updated</param>
        /// <param name="items">The items.</param>
        /// <param name="commentsRead">Indicates that these are new comments or whether the comments were just read</param>
        private void UpdateCommentStatus(TreeFeedsNodeBase tn, IList<INewsItem> items, bool commentsRead)
        {
            ;

            int multiplier = (commentsRead ? -1 : 1);

            if (commentsRead)
            {
                tn.UpdateCommentStatus(tn, items.Count*multiplier);
                owner.UpdateWatchedItems(items);
            }
            else
            {
                IList<INewsItem> itemsWithNewComments = GetFeedItemsWithNewComments(items);
                tn.UpdateCommentStatus(tn, itemsWithNewComments.Count*multiplier);
                owner.UpdateWatchedItems(itemsWithNewComments);
            }
            WatchedItemsNode.UpdateCommentStatus();
        }

        /// <summary>
        /// Returns the number of items with new comments in a particular list of items
        /// </summary>
        /// <param name="items">The list of items</param>
        /// <returns>The number of items with new comments</returns>
        private static IList<INewsItem> GetFeedItemsWithNewComments(IList<INewsItem> items)
        {
            List<INewsItem> itemsWithNewComments = new List<INewsItem>();

            if (items == null) return itemsWithNewComments;
            if (items.Count == 0) return itemsWithNewComments;

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];
                if (item.HasNewComments) itemsWithNewComments.Add(item);
            }

            return itemsWithNewComments;
        }

        /// <summary>
        /// Remove unread items of the feed f from the unread item tree node container.
        /// </summary>
        /// <param name="feedLink">The feed link.</param>
        private void UnreadItemsNodeRemoveItems(string feedLink)
        {
            if (string.IsNullOrEmpty(feedLink) ||
                ! owner.FeedHandler.IsSubscribed(feedLink))
                return;
            IList<INewsItem> items = owner.FeedHandler.GetCachedItemsForFeed(feedLink);
            UnreadItemsNodeRemoveItems(FilterUnreadFeedItems(items));
        }

        /// <summary>
        /// Remove unread items of the feed f from the unread item tree node container.
        /// </summary>
        /// <param name="f">The feed.</param>
        private void UnreadItemsNodeRemoveItems(INewsFeed f)
        {
            if (f == null) return;
            UnreadItemsNodeRemoveItems(FilterUnreadFeedItems(f));
        }

        /// <summary>
        /// Remove items from the unread item tree node container.
        /// The NewsItems in unread list are NOT checked again if they are
        /// unread!
        /// </summary>
        /// <param name="unread">The unread item list.</param>
        private void UnreadItemsNodeRemoveItems(IList<INewsItem> unread)
        {
            if (unread == null) return;
            for (int i = 0; i < unread.Count; i++)
                UnreadItemsNode.Remove(unread[i]);
            UnreadItemsNode.UpdateReadStatus();
        }

        /// <summary>
        /// Gets the list of unread item only from the provided feed.
        /// </summary>
        /// <param name="f">The feed.</param>
        /// <returns></returns>
        private IList<INewsItem> FilterUnreadFeedItems(INewsFeed f)
        {
            List<INewsItem> result = new List<INewsItem>();

            if (f == null)
                return result;

            if (f.containsNewMessages)
            {
                IList<INewsItem> items = null;
                try
                {
                    items = owner.FeedHandler.GetCachedItemsForFeed(f.link);
                }
                catch
                {
                    /* ignore cache errors here. On error, it returns always empty list */
                }

                return FilterUnreadFeedItems(items);
            }
            return result;
        }
		private static IList<INewsItem> FilterUnreadFeedItems(IList<INewsItem> items) {
			return FilterUnreadFeedItems(items, false);
		}
		/// <summary>
		/// Gets the unread items out of the provided list.
		/// </summary>
		/// <param name="items">The items.</param>
		/// <param name="sorted">if set to <c>true</c> it returns a sorted list (descending by item date,
		/// means newest first).</param>
		/// <returns></returns>
        private static IList<INewsItem> FilterUnreadFeedItems(IList<INewsItem> items, bool sorted)
        {
            List<INewsItem> result = new List<INewsItem>();
			
            if (items == null || items.Count == 0)
                return result;

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];
                if (!item.BeenRead)
                    result.Add(item);
            }
			
			if (sorted)
				result.Sort(RssHelper.GetComparer(true)); 
            return result;
        }

        /// <summary>
        /// Returns the number of unread items in a particular feed
        /// </summary>
        /// <param name="f">The target feed</param>
        /// <returns>The number of unread items</returns>
        private int CountUnreadFeedItems(INewsFeed f)
        {
            if (f == null) return 0;
            return FilterUnreadFeedItems(f).Count;
        }

        /// <summary>
        /// Returns the number of items with unread comments for this feed
        /// </summary>
        /// <param name="f">The target feed</param>
        /// <returns>The number of items with unread comments </returns>
        private IList<INewsItem> GetFeedItemsWithNewComments(INewsFeed f)
        {
            List<INewsItem> itemsWithNewComments = new List<INewsItem>();

            if (f == null) return itemsWithNewComments;

            if (f.containsNewComments)
            {
                IList<INewsItem> items = null;
                try
                {
                    items = owner.FeedHandler.GetCachedItemsForFeed(f.link);
                }
                catch
                {
                    /* ignore cache errors here. On error, it returns always zero */
                }
                if (items == null) return itemsWithNewComments;

                for (int i = 0; i < items.Count; i++)
                {
                    INewsItem item = items[i];
                    if (item.HasNewComments) itemsWithNewComments.Add(item);
                }
            }
            return itemsWithNewComments;
        }


        /// <summary>
        /// Obtains the number of unread RSS feeds and total unread RSS items
        /// </summary>
        /// <param name="unreadFeeds">Total RSS feeds with at least one unread item</param>
        /// <param name="unreadMessages">Total unread items</param>
        private void CountUnread(out int unreadFeeds, out int unreadMessages)
        {
            /* this code is inefficient because we loop through feeds and items even though 
			 * we probably just did that in a RefreshFeeds(). At least this should happen only
			 * when the app is minimized so this doesn't delay response time to user input. 
			 */

            unreadFeeds = unreadMessages = 0;

            foreach (INewsFeed f in owner.FeedHandler.GetFeeds().Values)
            {
                if (f.containsNewMessages)
                {
                    unreadFeeds++;
                    int urm = CountUnreadFeedItems(f);
                    unreadMessages += urm;
                }
            }
        }

        private void CheckForAddIns()
        {
            owner.CheckAndLoadAddIns();
            IBlogExtension ibe = null;

            try
            {
                blogExtensions = ServiceManager.SearchForIBlogExtensions(RssBanditApplication.GetPlugInPath());
                if (blogExtensions == null || blogExtensions.Count == 0)
                    return;

                // separator
                _listContextMenu.MenuItems.Add(new MenuItem("-"));

                for (int i = 0; i < blogExtensions.Count; i++)
                {
                    ibe = blogExtensions[i];
                    AppContextMenuCommand m = new AppContextMenuCommand("cmdIBlogExt." + i,
                                                                        owner.Mediator,
                                                                        new ExecuteCommandHandler(
                                                                            owner.CmdGenericListviewCommand),
                                                                        ibe.DisplayName,
                                                                        SR.MenuIBlogExtensionCommandDesc);
                    _listContextMenu.MenuItems.Add(m);
                    if (ibe.HasConfiguration)
                    {
                        AppContextMenuCommand mc = new AppContextMenuCommand("cmdIBlogExtConfig." + i,
                                                                             owner.Mediator,
                                                                             new ExecuteCommandHandler(
                                                                                 owner.CmdGenericListviewCommandConfig),
                                                                             ibe.DisplayName + " - " +
                                                                             SR.MenuConfigCommandCaption,
                                                                             SR.MenuIBlogExtensionConfigCommandDesc);
                        _listContextMenu.MenuItems.Add(mc);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Fatal(
                    "Failed to load IBlogExtension plugin: " + (ibe == null ? String.Empty : ibe.GetType().FullName), ex);
                ExceptionManager.Publish(ex);
            }
            finally
            {
                //unload AppDomain used to load add-ins
                ServiceManager.UnloadLoaderAppDomain();
            }
        }

        private void OnFeedTransformed(object sender, ThreadWorkerProgressArgs args)
        {
            if (args.Exception != null)
            {
                // failure(s)
                args.Cancel = true;
                RssBanditApplication.PublishException(args.Exception);
            }
            else if (!args.Done)
            {
                // in progress
            }
            else if (args.Done)
            {
                // done
                object[] results = (object[]) args.Result;
                UltraTreeNode node = (UltraTreeNode) results[0];
                string html = (string) results[1];
                if ((listFeedItems.SelectedItems.Count == 0) && treeFeeds.SelectedNodes.Count > 0 &&
                    ReferenceEquals(treeFeeds.SelectedNodes[0], node))
                {
                    htmlDetail.Html = html;
                    htmlDetail.Navigate(null);
                }
            }
        }


        /// <summary>
        /// Invoked by RssBanditApplication when an enclosure has been successfully dowbloaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnEnclosureReceived(object sender, DownloadItemEventArgs e)
        {
            /* display alert window on new download available */
            if (owner.FeedHandler.IsSubscribed(e.DownloadItem.OwnerFeedId))
            {
                INewsFeed f = owner.FeedHandler.GetFeeds()[e.DownloadItem.OwnerFeedId];

                if (owner.FeedHandler.GetEnclosureAlert(f.link))
                {
                    e.DownloadItem.OwnerFeed = f;
                    List<DownloadItem> items = new List<DownloadItem>();
                    items.Add(e.DownloadItem);
                    toastNotifier.Alert(f.title, 1, items);
                }
            } //if(feedHandler.GetFeeds().Contains(..))
        }


        private void BeginTransformFeed(IFeedDetails feed, UltraTreeNode feedNode, string stylesheet)
        {
            /* perform XSLT transformation in a background thread */
            owner.MakeAndQueueTask(ThreadWorker.Task.TransformFeed, OnFeedTransformed,
                                   ThreadWorkerBase.DuplicateTaskQueued.Abort, feed, feedNode, stylesheet);
        }


        private void BeginTransformFeedList(FeedInfoList feeds, UltraTreeNode feedNode, string stylesheet)
        {
            /* perform XSLT transformation in a background thread */
            owner.MakeAndQueueTask(ThreadWorker.Task.TransformCategory,
                                   OnFeedTransformed,
                                   ThreadWorkerBase.DuplicateTaskQueued.Abort, feeds, feedNode, stylesheet);
        }


        /// <summary>
        /// Returns a FeedInfoList containing the news items which should be displayed on the 
        /// specified page if in the newspaper view. 
        /// </summary>
        /// <param name="pageNum">The page number. If the page number is outside the range 
        /// of valid values then the first page is returned. </param>
        /// <returns>A FeedInfoList containing all the news items that should be displayed on 
        /// the specified page</returns>
        private FeedInfoList GetCategoryItemsAtPage(int pageNum)
        {
            if (_currentCategoryNewsItems == null)
            {
                return null;
            }

            int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);

            bool validPageNum = (pageNum >= 1) && (pageNum <= _lastPageNumber);

            if (owner.Preferences.LimitNewsItemsPerPage && validPageNum)
            {
                FeedInfoList fil = new FeedInfoList(_currentCategoryNewsItems.Title);

                int endindex = pageNum*itemsPerPage;
                int startindex = endindex - itemsPerPage;
                int counter = 0;
                int numLeft = itemsPerPage;

                foreach (FeedInfo fi in _currentCategoryNewsItems)
                {
                    if (numLeft <= 0)
                    {
                        break;
                    }

                    FeedInfo ficlone = fi.Clone(false);

                    if ((fi.ItemsList.Count + counter) > startindex)
                    {
                        //is this feed on the page?
                        int actualstart = startindex - counter;
                        int actualend = actualstart + numLeft;

                        if (actualend > fi.ItemsList.Count)
                        {
                            //handle case where this feed isn't the last one on the page							
                            int numAdded = fi.ItemsList.Count - actualstart;
                            ficlone.ItemsList.AddRange(fi.ItemsList.GetRange(actualstart, numAdded));
                            numLeft -= numAdded;
                            startindex += numAdded;
                        }
                        else
                        {
                            ficlone.ItemsList.AddRange(fi.ItemsList.GetRange(actualstart, numLeft));
                            numLeft -= numLeft;
                        }
                        fil.Add(ficlone);
                    }
                    counter += fi.ItemsList.Count;
                } //foreach


                return fil;
            }
            else
            {
                return _currentCategoryNewsItems;
            }
        }

        /// <summary>
        /// Returns a FeedInfo containing the news items which should be displayed on the 
        /// specified page if in the newspaper view. 
        /// </summary>
        /// <param name="pageNum">The page number. If the page number is outside the range 
        /// of valid values then the first page is returned. </param>
        /// <returns>A FeedInfo containing all the news items that should be displayed on 
        /// the specified page</returns>
        private FeedInfo GetFeedItemsAtPage(int pageNum)
        {
            if (_currentFeedNewsItems == null)
            {
                return null;
            }

            int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);
            int numItems = _currentFeedNewsItems.ItemsList.Count;

            bool validPageNum = (pageNum >= 1) && (pageNum <= _lastPageNumber);

            if (owner.Preferences.LimitNewsItemsPerPage && validPageNum)
            {
                FeedInfo fi = _currentFeedNewsItems.Clone(false);

                int endindex = pageNum*itemsPerPage;
                int startindex = endindex - itemsPerPage;

                if (endindex > numItems)
                {
                    //handle if we are on last page and numItems % itemsPerPage != 0
                    fi.ItemsList.AddRange(_currentFeedNewsItems.ItemsList.GetRange(startindex, numItems - startindex));
                }
                else
                {
                    fi.ItemsList.AddRange(_currentFeedNewsItems.ItemsList.GetRange(startindex, itemsPerPage));
                }

                return fi;
            }
            else
            {
                return _currentFeedNewsItems;
            }
        }

        /// <summary>
        /// Reloads the list view if the feed node is selected and renders the newspaper view
        /// </summary>
        /// <param name="tn">the tree node</param>
        /// <param name="populateListview">indicates whether the list view should be repopulated or not</param>
        internal void RefreshFeedDisplay(TreeFeedsNodeBase tn, bool populateListview)
        {
            if (tn == null)
                tn = CurrentSelectedFeedsNode;
            if (tn == null)
                return;
            if (!tn.Selected || tn.Type != FeedNodeType.Feed)
                return;

            INewsFeed f = owner.GetFeed(tn.DataKey);

            if (f != null)
            {
                owner.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOne);
                try
                {
                    htmlDetail.Clear();
                    // Old: call may initiate a web request, if eTag/last retrived is too old:
                    //ArrayList items = owner.FeedHandler.GetItemsForFeed(tn.DataKey, false);
                    // this will just get the items from cache:
                    IList<INewsItem> items = owner.FeedHandler.GetCachedItemsForFeed(tn.DataKey);
                    IList<INewsItem> unread = FilterUnreadFeedItems(items, true);

                    if ((DisplayFeedAlertWindow.All == owner.Preferences.ShowAlertWindow ||
                         (DisplayFeedAlertWindow.AsConfiguredPerFeed == owner.Preferences.ShowAlertWindow &&
                          f.alertEnabled)) &&
                        tn.UnreadCount < unread.Count)
                    {
                        //test flag on feed, if toast enabled
                        toastNotifier.Alert(tn.Text, unread.Count, unread);
                    }

                    if (tn.UnreadCount != unread.Count)
                    {
                        UnreadItemsNodeRemoveItems(items);
                        UnreadItemsNode.Items.AddRange(unread);
                        UnreadItemsNode.UpdateReadStatus();
                    }

                    //we don't need to populate the listview if this called from 
                    //RssBanditApplication.ApplyPreferences() since it is already populated
                    if (populateListview)
                    {
                        PopulateListView(tn, items, true, false, tn);
                    }

                    IFeedDetails fi = owner.GetFeedDetails(tn.DataKey);

                    if (fi != null)
                    {
                        FeedDetailTabState.Url = fi.Link;

                        //we use a clone of the FeedInfo because it isn't 
                        //necessarily true that everything in the main FeedInfo is being rendered
                        FeedInfo fi2 = new FeedInfo(fi);
                        fi2.ItemsList.Clear();

                        fi2.ItemsList.AddRange(unread);

                        //sort news items
                        //TODO: Ensure that there is no chance the code below can throw ArgumentOutOfRangeException 
                        ThreadedListViewColumnHeader colHeader =
                            listFeedItems.Columns[listFeedItems.SortManager.SortColumnIndex];
                        IComparer<INewsItem> newsItemSorter =
                            RssHelper.GetComparer(listFeedItems.SortManager.SortOrder == SortOrder.Descending,
                                                  (NewsItemSortField)
                                                  Enum.Parse(typeof (NewsItemSortField), colHeader.Key));

                        fi2.ItemsList.Sort(newsItemSorter);

                        //store list of unread items then only send one page of results 
                        //to newspaper view. 						
                        _currentFeedNewsItems = fi2;
                        _currentCategoryNewsItems = null;
                        _currentPageNumber = _lastPageNumber = 1;
                        int numItems = _currentFeedNewsItems.ItemsList.Count;
                        string stylesheet = owner.FeedHandler.GetStyleSheet(tn.DataKey);

                        if (numItems > 0)
                        {
                            int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);
                            _lastPageNumber = (numItems/itemsPerPage) + (numItems%itemsPerPage == 0 ? 0 : 1);

                            //default stylesheet: get first page of items
                            if (string.IsNullOrEmpty(stylesheet))
                            {
                                fi2 = GetFeedItemsAtPage(1);
                                ;
                            }
                        }

                        //check to see if we still have focus 
                        if (tn.Selected)
                        {
                            BeginTransformFeed(fi2, tn, stylesheet);
                        }
                    }
                }
                catch (Exception e)
                {
                    EmptyListView();
                    owner.PublishXmlFeedError(e, tn.DataKey, true);
                }
                owner.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
            }
        }


        /// <summary>
        ///  Reloads the list view if the category node is selected and renders the newspaper view
        /// </summary>
        /// <param name="tn">the tree node</param>
        private void RefreshCategoryDisplay(TreeFeedsNodeBase tn)
        {
            listFeedItems.BeginUpdate();
            string category = tn.CategoryStoreName;
            FeedInfoList fil2, unreadItems = new FeedInfoList(category);

            PopulateListView(tn, new List<INewsItem>(), true);
            htmlDetail.Clear();
            WalkdownThenRefreshFeed(tn, false, true, tn, unreadItems);


            listFeedItems.EndUpdate();

            //sort news items
            //TODO: Ensure that there is no chance the code below can throw ArgumentOutOfRangeException 
            ThreadedListViewColumnHeader colHeader = listFeedItems.Columns[listFeedItems.SortManager.SortColumnIndex];
            IComparer<INewsItem> newsItemSorter =
                RssHelper.GetComparer(listFeedItems.SortManager.SortOrder == SortOrder.Descending,
                                      (NewsItemSortField) Enum.Parse(typeof (NewsItemSortField), colHeader.Key));

            foreach (FeedInfo f in unreadItems)
            {
                f.ItemsList.Sort(newsItemSorter);
            }



            //store list of unread items then only send one page of results 
            //to newspaper view. 						
            _currentFeedNewsItems = null;
            fil2 = _currentCategoryNewsItems = unreadItems;
            _currentPageNumber = _lastPageNumber = 1;
            int numItems = _currentCategoryNewsItems.NewsItemCount;
            string stylesheet = owner.FeedHandler.GetCategoryStyleSheet(category);

            if (numItems > 0)
            {
                int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);
                _lastPageNumber = (numItems/itemsPerPage) + (numItems%itemsPerPage == 0 ? 0 : 1);

                //default stylesheet: get first page of items
                if (string.IsNullOrEmpty(stylesheet))
                {
                    fil2 = GetCategoryItemsAtPage(1);
                }
            }


            if (tn.Selected)
            {
                FeedDetailTabState.Url = String.Empty;
                BeginTransformFeedList(fil2, tn, stylesheet);
            }
            
        }


        internal void DeleteCategory(TreeFeedsNodeBase categoryFeedsNode)
        {
            if (categoryFeedsNode == null) categoryFeedsNode = CurrentSelectedFeedsNode;
            if (categoryFeedsNode == null) return;
            if (categoryFeedsNode.Type != FeedNodeType.Category) return;

            TreeFeedsNodeBase cnf = null;

            // if there are feed items displayed, we may have to delete the content
            // if rss items are of a feed with the category to delete
            if (listFeedItems.Items.Count > 0)
                cnf = TreeHelper.FindNode(categoryFeedsNode, (INewsItem) (listFeedItems.Items[0]).Key);
            if (cnf != null)
            {
                EmptyListView();
                htmlDetail.Clear();
            }

            if (categoryFeedsNode.Selected ||
                TreeHelper.IsChildNode(categoryFeedsNode, TreeSelectedFeedsNode))
            {
                TreeSelectedFeedsNode = TreeHelper.GetNewNodeToActivate(categoryFeedsNode);
                RefreshFeedDisplay(TreeSelectedFeedsNode, true);
            }

            WalkdownThenDeleteFeedsOrCategories(categoryFeedsNode);
            string catName = TreeFeedsNodeBase.BuildCategoryStoreName(categoryFeedsNode);           
            owner.FeedHandler.DeleteCategory(catName);             
            UpdateTreeNodeUnreadStatus(categoryFeedsNode, 0);

            try
            {
                categoryFeedsNode.Parent.Nodes.Remove(categoryFeedsNode);
            }
            finally
            {
                DelayTask(DelayedTasks.SyncRssSearchTree);
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then call AsyncGetItemsForFeed() for each of them.
        /// </summary>
        /// <param name="startNode">Node to start with</param>
        /// <param name="forceRefresh">true, if refresh should be forced</param>
        /// <param name="categorized">indicates whether this is part of the refresh or click of a category node</param>
        /// <param name="initialFeedsNode">This is the node where the refresh began from</param>
        /// <param name="unreadItems">an array list to place the unread items in the category into. This is needed to render them afterwards 
        /// in a newspaper view</param>
        private void WalkdownThenRefreshFeed(TreeFeedsNodeBase startNode, bool forceRefresh, bool categorized,
                                             TreeFeedsNodeBase initialFeedsNode, FeedInfoList unreadItems)
        {
            if (startNode == null) return;

            if (TreeSelectedFeedsNode != initialFeedsNode)
                return; // do not continue, if selection was changed

            try
            {
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    if (Disposing)
                        return;

                    if (child.Type != FeedNodeType.Feed && child.FirstNode != null)
                    {
                        //if (forceRefresh) {
                        WalkdownThenRefreshFeed(child, forceRefresh, categorized, initialFeedsNode, unreadItems);
                        //}
                    }
                    else
                    {
                        string feedUrl = child.DataKey;

                        if (feedUrl == null || !owner.FeedHandler.IsSubscribed(feedUrl))
                            continue;

                        try
                        {
                            if (forceRefresh)
                            {
                                //owner.FeedHandler.AsyncGetItemsForFeed(feedUrl, forceRefresh);
                                DelayTask(DelayedTasks.StartRefreshOneFeed, feedUrl);
                            }
                            else if (categorized)
                            {
                                IList<INewsItem> items = owner.FeedHandler.GetCachedItemsForFeed(feedUrl);
                                INewsFeed f = owner.GetFeed(feedUrl);
                                FeedInfo fi;

                                if (f != null)
                                {
                                    IFeedDetails ifd = (IFeedDetails) owner.GetFeedDetails(f.link);

                                    if (ifd == null) // with with an error, and the like: ignore
                                        continue;

                                    fi = new FeedInfo(ifd);
                                    fi.ItemsList.Clear();
                                }
                                else
                                {
                                    fi = FeedInfo.Empty;
                                }

                                foreach (INewsItem i in items)
                                {
                                    if (!i.BeenRead)
                                        fi.ItemsList.Add(i);
                                }

                                if (fi.ItemsList.Count > 0)
                                {
                                    unreadItems.Add(fi);
                                    if (fi.ItemsList.Count != child.UnreadCount)
                                    {
                                        UpdateTreeNodeUnreadStatus(child, fi.ItemsList.Count);
                                        UnreadItemsNodeRemoveItems(items);
                                        UnreadItemsNode.Items.AddRange(fi.ItemsList);
                                        UnreadItemsNode.UpdateReadStatus();
                                    }
                                }
                                else
                                {
                                }

                                //todo -- build list to add at end
                                PopulateListView(child, items, false, true, initialFeedsNode);
                                Application.DoEvents();
                            }
                        }
                        catch (Exception e)
                        {
                            owner.PublishXmlFeedError(e, feedUrl, true);
                        }
                    }
                } //for
            }
            catch (Exception ex)
            {
                _log.Error("WalkdownThenRefreshFeed() failed.", ex);
            }
        }


        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then catchup categories on any child FeedNode. Does not work on
        /// the root Node (there we call FeedHandler.MarkAllCachedItemsAsRead) !
        /// </summary>
        /// <param name="startNode">Node to start with. The startNode itself is 
        /// considered on catchup.</param>
        private void WalkdownAndCatchupCategory(TreeFeedsNodeBase startNode)
        {
            if (startNode == null) return;

            if (startNode.Type == FeedNodeType.Category)
            {
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    if (child.Type == FeedNodeType.Category)
                        WalkdownAndCatchupCategory(child);
                    else
                    {
                        // rely on unread cached items:
                        UnreadItemsNodeRemoveItems(child.DataKey);
                        // and now mark cached items read:
                        owner.FeedHandler.MarkAllCachedItemsAsRead(child.DataKey);
                        UpdateTreeNodeUnreadStatus(child, 0);
                    }
                }
            }
            else
            {
                owner.FeedHandler.MarkAllCachedItemsAsRead(startNode.DataKey);
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then rename categories on any FeedNode within owner.GetFeeds().
        /// </summary>
        /// <param name="startNode">Node to start with. The startNode itself is 
        /// not considered on renaming.</param>
        /// <param name="newCategory">new full category name (long name, with all the '\').</param>
        private void WalkdownThenRenameFeedCategory(TreeFeedsNodeBase startNode, string newCategory)
        {
            if (startNode == null) return;
            INewsFeed f;

            if (startNode.Type == FeedNodeType.Feed)
            {
                f = owner.GetFeed(startNode.DataKey);
                if (f != null)
                {
                    f.category = newCategory; // may be null: then it is the default category "[Unassigned feeds]"
                    owner.FeedWasModified(f, NewsFeedProperty.FeedCategory);
                    //owner.FeedlistModified = true;
                }
                if (newCategory != null && !owner.FeedHandler.HasCategory(newCategory))
                    owner.FeedHandler.AddCategory(newCategory);
            }
            else
            {
                // other
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    if (child.Type == FeedNodeType.Feed)
                        WalkdownThenRenameFeedCategory(child, child.Parent.CategoryStoreName);
                    else
                        WalkdownThenRenameFeedCategory(child, null /* BuildCategoryStoreName(child) */);
                            // catname will be recalculated on each CategoryNode
                }
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then delete all child categories and FeedNode refs in owner.FeedHandler.
        /// </summary>
        /// <param name="startNode">new full category name (long name, with all the '\').</param>
        private void WalkdownThenDeleteFeedsOrCategories(TreeFeedsNodeBase startNode)
        {
            if (startNode == null) return;

            if (startNode.Type == FeedNodeType.Feed)
            {
                if (owner.FeedHandler.IsSubscribed(startNode.DataKey))
                {
                    INewsFeed f = owner.GetFeed(startNode.DataKey);
                    if (f != null)
                    {
                        UnreadItemsNodeRemoveItems(f);
                        f.Tag = null; // remove tree node ref.                  
                    }
                }              
            }
            else
            {
                // other
                string catName = startNode.CategoryStoreName;
              
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    WalkdownThenDeleteFeedsOrCategories(child);
                }
            }
        }

        //		bool StoreFeedColumnLayout(FeedTreeNodeBase startNode, string layout) {
        //			if (layout == null) throw new ArgumentNullException("layout");
        //			if (startNode == null) return false;
        //
        //			if (startNode.Type == FeedNodeType.Feed) {
        //				if (!string.IsNullOrEmpty(owner.FeedHandler.GetFeedColumnLayout(startNode.DataKey)))
        //					owner.FeedHandler.SetFeedColumnLayout(startNode.DataKey, layout);
        //				else
        //					CurrentFeedFeedColumnLayout = layout;
        //			} else if(startNode.Type == FeedNodeType.Category) {
        //				if (!string.IsNullOrEmpty(owner.FeedHandler.GetCategoryFeedColumnLayout(startNode.DataKey)))
        //					owner.FeedHandler.SetCategoryFeedColumnLayout(startNode.DataKey, layout);
        //				else
        //					CurrentCategoryFeedColumnLayout = layout;
        //			} else {
        //				CurrentSmartFolderFeedColumnLayout = layout;
        //			}
        //			
        //			return true;
        //		}


        /// <summary>
        /// Gets the feed column layout.
        /// </summary>
        /// <param name="startNode">The start node.</param>
        /// <returns></returns>
        private FeedColumnLayout GetFeedColumnLayout(TreeFeedsNodeBase startNode)
        {
            if (startNode == null)
                startNode = TreeSelectedFeedsNode;
            if (startNode == null)
                return listFeedItems.FeedColumnLayout;

            FeedColumnLayout layout = listFeedItems.FeedColumnLayout;
            if (startNode.Type == FeedNodeType.Feed)
            {
                layout = owner.GetFeedColumnLayout(startNode.DataKey);
                if (layout == null) layout = owner.GlobalFeedColumnLayout;
            }
            else if (startNode.Type == FeedNodeType.Category)
            {
                layout = owner.GetCategoryColumnLayout(startNode.CategoryStoreName);
                if (layout == null) layout = owner.GlobalCategoryColumnLayout;
            }
            else if (startNode.Type == FeedNodeType.Finder)
            {
                layout = owner.GlobalSearchFolderColumnLayout;
            }
            else if (startNode.Type == FeedNodeType.SmartFolder)
            {
                layout = owner.GlobalSpecialFolderColumnLayout;
            }
            return layout;
        }

        /// <summary>
        /// Sets the feed handler feed column layout.
        /// </summary>
        /// <param name="feedsNode">The feeds node.</param>
        /// <param name="layout">The layout.</param>
        private void SetFeedHandlerFeedColumnLayout(TreeFeedsNodeBase feedsNode, FeedColumnLayout layout)
        {
            if (feedsNode == null) feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null)
            {
                if (feedsNode.Type == FeedNodeType.Feed)
                {
                    owner.SetFeedColumnLayout(feedsNode.DataKey, layout);
                }
                else if (feedsNode.Type == FeedNodeType.Category)
                {
                    owner.SetCategoryColumnLayout(feedsNode.CategoryStoreName, layout);
                }
                else if (feedsNode.Type == FeedNodeType.Finder)
                {
                    owner.GlobalSearchFolderColumnLayout = layout;
                }
                else if (feedsNode.Type == FeedNodeType.SmartFolder)
                {
                    owner.GlobalSpecialFolderColumnLayout = layout;
                }
            }
        }

        /// <summary>
        /// Sets the global feed column layout.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="layout">The layout.</param>
        private void SetGlobalFeedColumnLayout(FeedNodeType type, FeedColumnLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");

            if (type == FeedNodeType.Feed)
            {
                owner.GlobalFeedColumnLayout = layout;
            }
            else if (type == FeedNodeType.Category)
            {
                owner.GlobalCategoryColumnLayout = layout;
            }
            else
            {
                //CurrentCategoryFeedColumnLayout = layout;
            }
        }


        /// <summary>
        /// A helper method that locates the ThreadedListViewItem representing
        /// the NewsItem object. 
        /// </summary>
        /// <param name="item">The RSS item</param>
        /// <returns>The ThreadedListViewItem or null if 
        /// it can't be found</returns>
        public ThreadedListViewItem GetListViewItem(NewsItem item)
        {
            ThreadedListViewItem theItem = null;
            for (int i = 0; i < listFeedItems.Items.Count; i++)
            {
                ThreadedListViewItem currentItem = listFeedItems.Items[i];
                if (item.Equals(currentItem.Key))
                {
                    theItem = currentItem;
                    break;
                }
            }
            return theItem;
        }

        /// <summary>
        /// A helper method that locates the ThreadedListViewItem representing
        /// the NewsItem object with the given ID. 
        /// </summary>
        /// <param name="id">The RSS item's ID</param>
        /// <returns>The ThreadedListViewItem or null if 
        /// it can't be found</returns>
        public ThreadedListViewItem GetListViewItem(string id)
        {
            //TR: fix (2007/05/03) provided id can be Url Encoded:
            string normalizedId = HtmlHelper.UrlDecode(id);
            ThreadedListViewItem theItem = null;
            for (int i = 0; i < listFeedItems.Items.Count; i++)
            {
                ThreadedListViewItem currentItem = listFeedItems.Items[i];
                INewsItem item = (INewsItem) currentItem.Key;

                if (item.Id.Equals(id) || item.Id.Equals(normalizedId))
                {
                    theItem = currentItem;
                    break;
                }
            }
            return theItem;
        }

        /// <summary>
        /// Traverse down the tree on the path defined by 'category' 
        /// starting with 'startNode'.
        /// </summary>
        /// <param name="startNode">FeedTreeNodeBase to start with</param>
        /// <param name="category">A category path, e.g. 'Category1\SubCategory1'.</param>
        /// <returns>The leave category node.</returns>
        /// <remarks>If one category in the path is not found, it will be created.</remarks>
        internal TreeFeedsNodeBase CreateSubscriptionsCategoryHive(TreeFeedsNodeBase startNode, string category)
        {
            return TreeHelper.CreateCategoryHive(startNode, category, _treeCategoryContextMenu);
        }

        private void DoEditTreeNodeLabel()
        {
            if (CurrentSelectedFeedsNode != null)
            {
                CurrentSelectedFeedsNode.BeginEdit();
            }
        }

        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (components != null)
                    {
                        components.Dispose();
                    }
                }
                base.Dispose(disposing);
            }
            catch
            {
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinGuiMain));
            Infragistics.Win.UltraWinTree.Override _override1 = new Infragistics.Win.UltraWinTree.Override();
            Infragistics.Win.UltraWinTree.UltraTreeColumnSet ultraTreeColumnSet1 = new Infragistics.Win.UltraWinTree.UltraTreeColumnSet();
            Infragistics.Win.UltraWinTree.UltraTreeNodeColumn ultraTreeNodeColumn1 = new Infragistics.Win.UltraWinTree.UltraTreeNodeColumn();
            Infragistics.Win.UltraWinTree.Override _override2 = new Infragistics.Win.UltraWinTree.Override();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup ultraExplorerBarGroup1 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup ultraExplorerBarGroup2 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            this.NavigatorFeedSubscriptions = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
            this.treeFeeds = new Infragistics.Win.UltraWinTree.UltraTree();
            this.NavigatorSearch = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
            this.panelRssSearch = new System.Windows.Forms.Panel();
            this.ultraToolTipManager = new Infragistics.Win.UltraWinToolTip.UltraToolTipManager(this.components);
            this.panelFeedDetails = new System.Windows.Forms.Panel();
            this.panelWebDetail = new System.Windows.Forms.Panel();
            this.htmlDetail = new IEControl.HtmlControl();
            this.detailsPaneSplitter = new RssBandit.WinGui.Controls.CollapsibleSplitter();
            this.panelFeedItems = new System.Windows.Forms.Panel();
            this.listFeedItemsO = new RssBandit.WinGui.Controls.UltraTreeExtended();
            this.listFeedItems = new System.Windows.Forms.ThListView.ThreadedListView();
            this.colHeadline = new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader();
            this.colDate = new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader();
            this.colTopic = new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._status = new System.Windows.Forms.StatusBar();
            this.statusBarBrowser = new System.Windows.Forms.StatusBarPanel();
            this.statusBarBrowserProgress = new System.Windows.Forms.StatusBarPanel();
            this.statusBarConnectionState = new System.Windows.Forms.StatusBarPanel();
            this.statusBarRssParser = new System.Windows.Forms.StatusBarPanel();
            this.progressBrowser = new System.Windows.Forms.ProgressBar();
            this.rightSandDock = new TD.SandDock.DockContainer();
            this.sandDockManager = new TD.SandDock.SandDockManager();
            this.bottomSandDock = new TD.SandDock.DockContainer();
            this.topSandDock = new TD.SandDock.DockContainer();
            this._docContainer = new TD.SandDock.DocumentContainer();
            this._docFeedDetails = new TD.SandDock.DockControl();
            this.panelClientAreaContainer = new System.Windows.Forms.Panel();
            this.panelFeedDetailsContainer = new System.Windows.Forms.Panel();
            this.detailHeaderCaption = new Infragistics.Win.Misc.UltraLabel();
            this.splitterNavigator = new System.Windows.Forms.Splitter();
            this.Navigator = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBar();
            this.pNavigatorCollapsed = new System.Windows.Forms.Panel();
            this.navigatorHiddenCaption = new RssBandit.WinGui.Controls.VerticalHeaderLabel();
            this._startupTimer = new System.Windows.Forms.Timer(this.components);
            this._timerTreeNodeExpand = new System.Timers.Timer();
            this._timerRefreshFeeds = new System.Timers.Timer();
            this._timerRefreshCommentFeeds = new System.Timers.Timer();
            this._timerResetStatus = new System.Windows.Forms.Timer(this.components);
            this._uiTasksTimer = new RssBandit.WinGui.Forms.WinGuiMain.UITaskTimer(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this._timerDispatchResultsToUI = new System.Windows.Forms.Timer(this.components);
            this.NavigatorFeedSubscriptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeFeeds)).BeginInit();
            this.NavigatorSearch.SuspendLayout();
            this.panelFeedDetails.SuspendLayout();
            this.panelWebDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.htmlDetail)).BeginInit();
            this.panelFeedItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listFeedItemsO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarBrowser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarBrowserProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarConnectionState)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarRssParser)).BeginInit();
            this._docContainer.SuspendLayout();
            this._docFeedDetails.SuspendLayout();
            this.panelClientAreaContainer.SuspendLayout();
            this.panelFeedDetailsContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Navigator)).BeginInit();
            this.Navigator.SuspendLayout();
            this.pNavigatorCollapsed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._timerTreeNodeExpand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._timerRefreshFeeds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._timerRefreshCommentFeeds)).BeginInit();
            this.SuspendLayout();
            // 
            // NavigatorFeedSubscriptions
            // 
            this.NavigatorFeedSubscriptions.Controls.Add(this.treeFeeds);
            resources.ApplyResources(this.NavigatorFeedSubscriptions, "NavigatorFeedSubscriptions");
            this.NavigatorFeedSubscriptions.Name = "NavigatorFeedSubscriptions";
            this.helpProvider1.SetShowHelp(this.NavigatorFeedSubscriptions, ((bool)(resources.GetObject("NavigatorFeedSubscriptions.ShowHelp"))));
            // 
            // treeFeeds
            // 
            this.treeFeeds.AllowDrop = true;
            this.treeFeeds.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
            resources.ApplyResources(this.treeFeeds, "treeFeeds");
            this.treeFeeds.HideSelection = false;
            this.treeFeeds.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.treeFeeds.Name = "treeFeeds";
            this.treeFeeds.NodeConnectorColor = System.Drawing.SystemColors.ControlDark;
            _override1.LabelEdit = Infragistics.Win.DefaultableBoolean.True;
            _override1.Sort = Infragistics.Win.UltraWinTree.SortType.Ascending;
            this.treeFeeds.Override = _override1;
            this.treeFeeds.SettingsKey = "WinGuiMain.treeFeeds";
            this.helpProvider1.SetShowHelp(this.treeFeeds, ((bool)(resources.GetObject("treeFeeds.ShowHelp"))));
            // 
            // NavigatorSearch
            // 
            this.NavigatorSearch.Controls.Add(this.panelRssSearch);
            resources.ApplyResources(this.NavigatorSearch, "NavigatorSearch");
            this.NavigatorSearch.Name = "NavigatorSearch";
            this.helpProvider1.SetShowHelp(this.NavigatorSearch, ((bool)(resources.GetObject("NavigatorSearch.ShowHelp"))));
            // 
            // panelRssSearch
            // 
            this.panelRssSearch.BackColor = System.Drawing.SystemColors.InactiveCaption;
            resources.ApplyResources(this.panelRssSearch, "panelRssSearch");
            this.panelRssSearch.Name = "panelRssSearch";
            this.helpProvider1.SetShowHelp(this.panelRssSearch, ((bool)(resources.GetObject("panelRssSearch.ShowHelp"))));
            // 
            // ultraToolTipManager
            // 
            this.ultraToolTipManager.ContainingControl = this;
            this.ultraToolTipManager.DisplayStyle = Infragistics.Win.ToolTipDisplayStyle.Office2007;
            // 
            // panelFeedDetails
            // 
            this.panelFeedDetails.Controls.Add(this.panelWebDetail);
            this.panelFeedDetails.Controls.Add(this.detailsPaneSplitter);
            this.panelFeedDetails.Controls.Add(this.panelFeedItems);
            resources.ApplyResources(this.panelFeedDetails, "panelFeedDetails");
            this.panelFeedDetails.Name = "panelFeedDetails";
            this.helpProvider1.SetShowHelp(this.panelFeedDetails, ((bool)(resources.GetObject("panelFeedDetails.ShowHelp"))));
            // 
            // panelWebDetail
            // 
            this.panelWebDetail.Controls.Add(this.htmlDetail);
            resources.ApplyResources(this.panelWebDetail, "panelWebDetail");
            this.panelWebDetail.Name = "panelWebDetail";
            this.helpProvider1.SetShowHelp(this.panelWebDetail, ((bool)(resources.GetObject("panelWebDetail.ShowHelp"))));
            // 
            // htmlDetail
            // 
            this.htmlDetail.AllowDrop = true;
            resources.ApplyResources(this.htmlDetail, "htmlDetail");
            this.htmlDetail.Name = "htmlDetail";
            this.htmlDetail.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("htmlDetail.OcxState")));
            this.helpProvider1.SetShowHelp(this.htmlDetail, ((bool)(resources.GetObject("htmlDetail.ShowHelp"))));
            // 
            // detailsPaneSplitter
            // 
            this.detailsPaneSplitter.AnimationDelay = 20;
            this.detailsPaneSplitter.AnimationStep = 20;
            this.detailsPaneSplitter.BackColor = System.Drawing.SystemColors.Control;
            this.detailsPaneSplitter.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
            this.detailsPaneSplitter.ControlToHide = this.panelFeedItems;
            this.detailsPaneSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
            resources.ApplyResources(this.detailsPaneSplitter, "detailsPaneSplitter");
            this.detailsPaneSplitter.ExpandParentForm = false;
            this.detailsPaneSplitter.Name = "detailsPaneSplitter";
            this.helpProvider1.SetShowHelp(this.detailsPaneSplitter, ((bool)(resources.GetObject("detailsPaneSplitter.ShowHelp"))));
            this.detailsPaneSplitter.TabStop = false;
            this.detailsPaneSplitter.UseAnimations = false;
            this.detailsPaneSplitter.VisualStyle = RssBandit.WinGui.Controls.VisualStyles.XP;
            // 
            // panelFeedItems
            // 
            this.panelFeedItems.Controls.Add(this.listFeedItemsO);
            this.panelFeedItems.Controls.Add(this.listFeedItems);
            resources.ApplyResources(this.panelFeedItems, "panelFeedItems");
            this.panelFeedItems.Name = "panelFeedItems";
            this.helpProvider1.SetShowHelp(this.panelFeedItems, ((bool)(resources.GetObject("panelFeedItems.ShowHelp"))));
            // 
            // listFeedItemsO
            // 
            this.listFeedItemsO.ColumnSettings.AllowCellEdit = Infragistics.Win.UltraWinTree.AllowCellEdit.Disabled;
            this.listFeedItemsO.ColumnSettings.AutoFitColumns = Infragistics.Win.UltraWinTree.AutoFitColumns.ResizeAllColumns;
            ultraTreeColumnSet1.AllowCellEdit = Infragistics.Win.UltraWinTree.AllowCellEdit.Disabled;
            ultraTreeNodeColumn1.AllowCellEdit = Infragistics.Win.UltraWinTree.AllowCellEdit.Disabled;
            ultraTreeNodeColumn1.Key = "Arranged by: Date";
            ultraTreeColumnSet1.Columns.Add(ultraTreeNodeColumn1);
            ultraTreeColumnSet1.Key = "csOutlook";
            this.listFeedItemsO.ColumnSettings.ColumnSets.Add(ultraTreeColumnSet1);
            this.listFeedItemsO.ColumnSettings.HeaderStyle = Infragistics.Win.HeaderStyle.XPThemed;
            resources.ApplyResources(this.listFeedItemsO, "listFeedItemsO");
            this.listFeedItemsO.FullRowSelect = true;
            this.listFeedItemsO.HideSelection = false;
            this.listFeedItemsO.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.listFeedItemsO.IsUpdatingSelection = false;
            this.listFeedItemsO.Name = "listFeedItemsO";
            this.listFeedItemsO.NodeConnectorColor = System.Drawing.SystemColors.ControlDark;
            _override2.ColumnSetIndex = 0;
            _override2.ItemHeight = 35;
            _override2.SelectionType = Infragistics.Win.UltraWinTree.SelectType.Extended;
            this.listFeedItemsO.Override = _override2;
            this.listFeedItemsO.SettingsKey = "WinGuiMain.listFeedItemsO";
            this.helpProvider1.SetShowHelp(this.listFeedItemsO, ((bool)(resources.GetObject("listFeedItemsO.ShowHelp"))));
            // 
            // listFeedItems
            // 
            this.listFeedItems.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listFeedItems.AllowColumnReorder = true;
            this.listFeedItems.Columns.AddRange(new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader[] {
            this.colHeadline,
            this.colDate,
            this.colTopic});
            resources.ApplyResources(this.listFeedItems, "listFeedItems");
            this.listFeedItems.FullRowSelect = true;
            this.listFeedItems.HideSelection = false;
            this.listFeedItems.Name = "listFeedItems";
            this.listFeedItems.NoThreadChildsPlaceHolder = null;
            this.helpProvider1.SetShowHelp(this.listFeedItems, ((bool)(resources.GetObject("listFeedItems.ShowHelp"))));
            this.listFeedItems.UseCompatibleStateImageBehavior = false;
            this.listFeedItems.View = System.Windows.Forms.View.Details;
            this.listFeedItems.ListLayoutModified += new System.Windows.Forms.ThListView.ThreadedListView.OnListLayoutModifiedEventHandler(this.OnFeedListLayoutModified);
            this.listFeedItems.ItemActivate += new System.EventHandler(this.OnFeedListItemActivate);
            this.listFeedItems.ExpandThread += new System.Windows.Forms.ThListView.ThreadedListView.OnExpandThreadEventHandler(this.OnFeedListExpandThread);
            this.listFeedItems.ListLayoutChanged += new System.Windows.Forms.ThListView.ThreadedListView.OnListLayoutChangedEventHandler(this.OnFeedListLayoutChanged);
            this.listFeedItems.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnFeedListMouseDown);
            this.listFeedItems.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnFeedListItemDrag);
            this.listFeedItems.AfterExpandThread += new System.Windows.Forms.ThListView.ThreadedListView.OnAfterExpandThreadEventHandler(this.OnFeedListAfterExpandThread);
            this.listFeedItems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFeedListItemKeyUp);
            // 
            // colHeadline
            // 
            this.colHeadline.ColumnValueType = typeof(string);
            this.colHeadline.Key = "Title";
            resources.ApplyResources(this.colHeadline, "colHeadline");
            // 
            // colDate
            // 
            this.colDate.ColumnValueType = typeof(System.DateTime);
            this.colDate.Key = "Date";
            resources.ApplyResources(this.colDate, "colDate");
            // 
            // colTopic
            // 
            this.colTopic.ColumnValueType = typeof(string);
            this.colTopic.Key = "Subject";
            resources.ApplyResources(this.colTopic, "colTopic");
            // 
            // _status
            // 
            resources.ApplyResources(this._status, "_status");
            this._status.Name = "_status";
            this._status.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarBrowser,
            this.statusBarBrowserProgress,
            this.statusBarConnectionState,
            this.statusBarRssParser});
            this.helpProvider1.SetShowHelp(this._status, ((bool)(resources.GetObject("_status.ShowHelp"))));
            this._status.ShowPanels = true;
            // 
            // statusBarBrowser
            // 
            this.statusBarBrowser.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            resources.ApplyResources(this.statusBarBrowser, "statusBarBrowser");
            // 
            // statusBarBrowserProgress
            // 
            resources.ApplyResources(this.statusBarBrowserProgress, "statusBarBrowserProgress");
            // 
            // statusBarConnectionState
            // 
            resources.ApplyResources(this.statusBarConnectionState, "statusBarConnectionState");
            // 
            // statusBarRssParser
            // 
            resources.ApplyResources(this.statusBarRssParser, "statusBarRssParser");
            // 
            // progressBrowser
            // 
            resources.ApplyResources(this.progressBrowser, "progressBrowser");
            this.progressBrowser.Name = "progressBrowser";
            this.helpProvider1.SetShowHelp(this.progressBrowser, ((bool)(resources.GetObject("progressBrowser.ShowHelp"))));
            // 
            // rightSandDock
            // 
            resources.ApplyResources(this.rightSandDock, "rightSandDock");
            this.rightSandDock.Guid = new System.Guid("c6e4c477-596c-4e8c-9d35-840718d4c40d");
            this.rightSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
            this.rightSandDock.Manager = this.sandDockManager;
            this.rightSandDock.Name = "rightSandDock";
            this.helpProvider1.SetShowHelp(this.rightSandDock, ((bool)(resources.GetObject("rightSandDock.ShowHelp"))));
            // 
            // sandDockManager
            // 
            this.sandDockManager.DockingManager = TD.SandDock.DockingManager.Whidbey;
            this.sandDockManager.OwnerForm = this;
            this.sandDockManager.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
            // 
            // bottomSandDock
            // 
            resources.ApplyResources(this.bottomSandDock, "bottomSandDock");
            this.bottomSandDock.Guid = new System.Guid("9ffc7b96-a550-4e79-a533-8eee52ac0da1");
            this.bottomSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
            this.bottomSandDock.Manager = this.sandDockManager;
            this.bottomSandDock.Name = "bottomSandDock";
            this.helpProvider1.SetShowHelp(this.bottomSandDock, ((bool)(resources.GetObject("bottomSandDock.ShowHelp"))));
            // 
            // topSandDock
            // 
            resources.ApplyResources(this.topSandDock, "topSandDock");
            this.topSandDock.Guid = new System.Guid("e1c62abd-0e7a-4bb6-aded-a74f27027165");
            this.topSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
            this.topSandDock.Manager = this.sandDockManager;
            this.topSandDock.Name = "topSandDock";
            this.helpProvider1.SetShowHelp(this.topSandDock, ((bool)(resources.GetObject("topSandDock.ShowHelp"))));
            // 
            // _docContainer
            // 
            this._docContainer.Controls.Add(this._docFeedDetails);
            this._docContainer.Cursor = System.Windows.Forms.Cursors.Default;
            this._docContainer.DockingManager = TD.SandDock.DockingManager.Whidbey;
            this._docContainer.Guid = new System.Guid("f032a648-4262-4312-ab2b-abe5094272bd");
            this._docContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400, System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.DocumentLayoutSystem(392, 414, new TD.SandDock.DockControl[] {
                        this._docFeedDetails}, this._docFeedDetails)))});
            resources.ApplyResources(this._docContainer, "_docContainer");
            this._docContainer.Manager = null;
            this._docContainer.Name = "_docContainer";
            this._docContainer.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
            this.helpProvider1.SetShowHelp(this._docContainer, ((bool)(resources.GetObject("_docContainer.ShowHelp"))));
            // 
            // _docFeedDetails
            // 
            this._docFeedDetails.Closable = false;
            this._docFeedDetails.Controls.Add(this.panelFeedDetails);
            this._docFeedDetails.Guid = new System.Guid("9c7b7643-2ed3-402c-9e86-3c958341c81f");
            resources.ApplyResources(this._docFeedDetails, "_docFeedDetails");
            this._docFeedDetails.Name = "_docFeedDetails";
            this.helpProvider1.SetShowHelp(this._docFeedDetails, ((bool)(resources.GetObject("_docFeedDetails.ShowHelp"))));
            // 
            // panelClientAreaContainer
            // 
            this.panelClientAreaContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(247)))));
            this.panelClientAreaContainer.Controls.Add(this.panelFeedDetailsContainer);
            this.panelClientAreaContainer.Controls.Add(this.splitterNavigator);
            this.panelClientAreaContainer.Controls.Add(this.Navigator);
            this.panelClientAreaContainer.Controls.Add(this.pNavigatorCollapsed);
            resources.ApplyResources(this.panelClientAreaContainer, "panelClientAreaContainer");
            this.panelClientAreaContainer.Name = "panelClientAreaContainer";
            this.helpProvider1.SetShowHelp(this.panelClientAreaContainer, ((bool)(resources.GetObject("panelClientAreaContainer.ShowHelp"))));
            // 
            // panelFeedDetailsContainer
            // 
            this.panelFeedDetailsContainer.Controls.Add(this._docContainer);
            this.panelFeedDetailsContainer.Controls.Add(this.detailHeaderCaption);
            resources.ApplyResources(this.panelFeedDetailsContainer, "panelFeedDetailsContainer");
            this.panelFeedDetailsContainer.Name = "panelFeedDetailsContainer";
            this.helpProvider1.SetShowHelp(this.panelFeedDetailsContainer, ((bool)(resources.GetObject("panelFeedDetailsContainer.ShowHelp"))));
            // 
            // detailHeaderCaption
            // 
            appearance7.BackColor = System.Drawing.Color.CornflowerBlue;
            appearance7.BackColor2 = System.Drawing.Color.MidnightBlue;
            appearance7.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance7.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            appearance7.ImageHAlign = Infragistics.Win.HAlign.Right;
            appearance7.ImageVAlign = Infragistics.Win.VAlign.Middle;
            resources.ApplyResources(appearance7, "appearance7");
            appearance7.TextTrimming = Infragistics.Win.TextTrimming.EllipsisWord;
            this.detailHeaderCaption.Appearance = appearance7;
            resources.ApplyResources(this.detailHeaderCaption, "detailHeaderCaption");
            this.detailHeaderCaption.Name = "detailHeaderCaption";
            this.detailHeaderCaption.Padding = new System.Drawing.Size(5, 0);
            this.helpProvider1.SetShowHelp(this.detailHeaderCaption, ((bool)(resources.GetObject("detailHeaderCaption.ShowHelp"))));
            this.detailHeaderCaption.WrapText = false;
            // 
            // splitterNavigator
            // 
            resources.ApplyResources(this.splitterNavigator, "splitterNavigator");
            this.splitterNavigator.Name = "splitterNavigator";
            this.helpProvider1.SetShowHelp(this.splitterNavigator, ((bool)(resources.GetObject("splitterNavigator.ShowHelp"))));
            this.splitterNavigator.TabStop = false;
            // 
            // Navigator
            // 
            this.Navigator.Controls.Add(this.NavigatorFeedSubscriptions);
            this.Navigator.Controls.Add(this.NavigatorSearch);
            resources.ApplyResources(this.Navigator, "Navigator");
            ultraExplorerBarGroup1.Container = this.NavigatorFeedSubscriptions;
            ultraExplorerBarGroup1.Key = "groupFeedsTree";
            appearance2.Image = ((object)(resources.GetObject("appearance2.Image")));
            ultraExplorerBarGroup1.Settings.AppearancesLarge.HeaderAppearance = appearance2;
            appearance3.Image = ((object)(resources.GetObject("appearance3.Image")));
            ultraExplorerBarGroup1.Settings.AppearancesSmall.HeaderAppearance = appearance3;
            resources.ApplyResources(ultraExplorerBarGroup1, "ultraExplorerBarGroup1");
            ultraExplorerBarGroup2.Container = this.NavigatorSearch;
            ultraExplorerBarGroup2.Key = "groupFeedsSearch";
            appearance4.Image = ((object)(resources.GetObject("appearance4.Image")));
            ultraExplorerBarGroup2.Settings.AppearancesLarge.HeaderAppearance = appearance4;
            appearance5.Image = ((object)(resources.GetObject("appearance5.Image")));
            ultraExplorerBarGroup2.Settings.AppearancesSmall.HeaderAppearance = appearance5;
            resources.ApplyResources(ultraExplorerBarGroup2, "ultraExplorerBarGroup2");
            this.Navigator.Groups.AddRange(new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup[] {
            ultraExplorerBarGroup1,
            ultraExplorerBarGroup2});
            this.Navigator.GroupSettings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;
            this.Navigator.Name = "Navigator";
            this.Navigator.NavigationMaxGroupHeaders = 0;
            this.helpProvider1.SetShowHelp(this.Navigator, ((bool)(resources.GetObject("Navigator.ShowHelp"))));
            this.Navigator.Style = Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarStyle.OutlookNavigationPane;
            // 
            // pNavigatorCollapsed
            // 
            this.pNavigatorCollapsed.BackColor = System.Drawing.Color.Transparent;
            this.pNavigatorCollapsed.Controls.Add(this.navigatorHiddenCaption);
            resources.ApplyResources(this.pNavigatorCollapsed, "pNavigatorCollapsed");
            this.pNavigatorCollapsed.Name = "pNavigatorCollapsed";
            this.helpProvider1.SetShowHelp(this.pNavigatorCollapsed, ((bool)(resources.GetObject("pNavigatorCollapsed.ShowHelp"))));
            // 
            // navigatorHiddenCaption
            // 
            appearance8.BackColor = System.Drawing.Color.CornflowerBlue;
            appearance8.BackColor2 = System.Drawing.Color.MidnightBlue;
            appearance8.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            appearance8.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            appearance8.Image = ((object)(resources.GetObject("appearance8.Image")));
            appearance8.ImageHAlign = Infragistics.Win.HAlign.Center;
            appearance8.ImageVAlign = Infragistics.Win.VAlign.Top;
            resources.ApplyResources(appearance8, "appearance8");
            appearance8.TextTrimming = Infragistics.Win.TextTrimming.EllipsisWord;
            this.navigatorHiddenCaption.Appearance = appearance8;
            resources.ApplyResources(this.navigatorHiddenCaption, "navigatorHiddenCaption");
            this.navigatorHiddenCaption.Name = "navigatorHiddenCaption";
            this.navigatorHiddenCaption.Padding = new System.Drawing.Size(0, 5);
            this.helpProvider1.SetShowHelp(this.navigatorHiddenCaption, ((bool)(resources.GetObject("navigatorHiddenCaption.ShowHelp"))));
            this.navigatorHiddenCaption.WrapText = false;
            // 
            // _startupTimer
            // 
            this._startupTimer.Interval = 45000;
            this._startupTimer.Tick += new System.EventHandler(this.OnTimerStartupTick);
            // 
            // _timerTreeNodeExpand
            // 
            this._timerTreeNodeExpand.Interval = 1000;
            this._timerTreeNodeExpand.SynchronizingObject = this;
            this._timerTreeNodeExpand.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerTreeNodeExpandElapsed);
            // 
            // _timerRefreshFeeds
            // 
            this._timerRefreshFeeds.Interval = 600000;
            this._timerRefreshFeeds.SynchronizingObject = this;
            this._timerRefreshFeeds.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerFeedsRefreshElapsed);
            // 
            // _timerRefreshCommentFeeds
            // 
            this._timerRefreshCommentFeeds.Interval = 600000;
            this._timerRefreshCommentFeeds.SynchronizingObject = this;
            this._timerRefreshCommentFeeds.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerCommentFeedsRefreshElapsed);
            // 
            // _timerResetStatus
            // 
            this._timerResetStatus.Interval = 5000;
            this._timerResetStatus.Tick += new System.EventHandler(this.OnTimerResetStatusTick);
            // 
            // _uiTasksTimer
            // 
            this._uiTasksTimer.Enabled = true;
            // 
            // helpProvider1
            // 
            resources.ApplyResources(this.helpProvider1, "helpProvider1");
            // 
            // _timerDispatchResultsToUI
            // 
            this._timerDispatchResultsToUI.Interval = 250;
            // 
            // WinGuiMain
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panelClientAreaContainer);
            this.Controls.Add(this.rightSandDock);
            this.Controls.Add(this.bottomSandDock);
            this.Controls.Add(this.topSandDock);
            this.Controls.Add(this.progressBrowser);
            this.Controls.Add(this._status);
            this.helpProvider1.SetHelpNavigator(this, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("$this.HelpNavigator"))));
            this.KeyPreview = true;
            this.Name = "WinGuiMain";
            this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
            this.NavigatorFeedSubscriptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeFeeds)).EndInit();
            this.NavigatorSearch.ResumeLayout(false);
            this.panelFeedDetails.ResumeLayout(false);
            this.panelWebDetail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.htmlDetail)).EndInit();
            this.panelFeedItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.listFeedItemsO)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarBrowser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarBrowserProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarConnectionState)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarRssParser)).EndInit();
            this._docContainer.ResumeLayout(false);
            this._docFeedDetails.ResumeLayout(false);
            this.panelClientAreaContainer.ResumeLayout(false);
            this.panelFeedDetailsContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Navigator)).EndInit();
            this.Navigator.ResumeLayout(false);
            this.pNavigatorCollapsed.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._timerTreeNodeExpand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._timerRefreshFeeds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._timerRefreshCommentFeeds)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        #region Owner Interaction routines

        /// <summary>
        /// Extended Close.
        /// </summary>
        /// <param name="forceShutdown"></param>
        public void Close(bool forceShutdown)
        {
            //this.SaveUIConfiguration(forceShutdown);
            _forceShutdown = forceShutdown;
            Close();
        }

        public void SaveUIConfiguration(bool forceFlush)
        {
            try
            {
                OnSaveConfig(owner.GuiSettings);
                SaveSubscriptionTreeState();
                SaveBrowserTabState();
                listFeedItems.CheckForLayoutModifications();

                if (forceFlush)
                {
                    owner.GuiSettings.Flush();
                }
            }
            catch (Exception ex)
            {
                _log.Error("Save .settings.xml failed", ex);
            }
        }

        internal bool LoadAndRestoreBrowserTabState()
        {
            _browserTabsRestored = true;

            string fileName = RssBanditApplication.GetBrowserTabStateFileName();
            try
            {
                if (!File.Exists(fileName))
                    return false;
                using (Stream stream = FileHelper.OpenForRead(fileName))
                {
                    SerializableWebTabState state = SerializableWebTabState.Load(stream);

                    foreach (string url in state.Urls)
                    {
                        try
                        {
                            DetailTabNavigateToUrl(url, String.Empty /* tab title */, true /* createNewTab */, false
                                /* setFocus */);
                        }
                        catch (AxHost.InvalidActiveXStateException)
                        {
                            /* occurs if we are starting from sys tray because browser not visible */
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Load " + fileName + " failed", ex);
                return false;
            }
        }


        internal void SaveBrowserTabState()
        {
            string fileName = RssBanditApplication.GetBrowserTabStateFileName();
            SerializableWebTabState state = new SerializableWebTabState();

            try
            {
                foreach (DockControl doc in _docContainer.Documents)
                {
                    ITabState docState = (ITabState) doc.Tag;

                    if ((docState != null) && docState.CanClose)
                    {
                        state.Urls.Add(docState.Url);
                    }
                }

                using (Stream stream = FileHelper.OpenForWrite(fileName))
                {
                    SerializableWebTabState.Save(stream, state);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Save " + fileName + " failed", ex);
                // don't cause a load problem later on if save failed:
                try
                {
                    File.Delete(fileName);
                }
                catch (IOException)
                {
                }
            }
        }

        internal void SaveSubscriptionTreeState()
        {
            string fileName = RssBanditApplication.GetSubscriptionTreeStateFileName();
            try
            {
                using (Stream s = FileHelper.OpenForWrite(fileName))
                {
                    UltraTreeNodeExpansionMemento.Save(s, treeFeeds);
                }
            }
            catch (Exception ex)
            {
                //TR: do not bummer user with this file errors (called on AutoSave).
                //Just log - and try to recover (delete the bad file)
                //RssBanditApplication.PublishException(ex);
                _log.Error("Save " + fileName + " failed", ex);
                // don't cause a load problem later on if save failed:
                try
                {
                    File.Delete(fileName);
                }
                catch (IOException)
                {
                }
            }
        }

        internal bool LoadAndRestoreSubscriptionTreeState()
        {
            string fileName = RssBanditApplication.GetSubscriptionTreeStateFileName();
            try
            {
                if (!File.Exists(fileName))
                    return false;
                using (Stream s = FileHelper.OpenForRead(fileName))
                {
                    UltraTreeNodeExpansionMemento m = UltraTreeNodeExpansionMemento.Load(s);
                    m.Restore(treeFeeds);
                }
                return true;
            }
            catch (Exception ex)
            {
                SetDefaultExpansionTreeNodeState();
                //TR: inform user about file error
                owner.MessageWarn(SR.GUILoadFileOperationExceptionMessage(fileName, ex.Message,
                                                                          SR.GUIUserInfoAboutDefaultTreeState));
                //And log - recover may happen on save (delete the bad file)
                _log.Error("Load " + fileName + " failed", ex);
                return false;
            }
        }

        private static bool IsTreeStateAvailable()
        {
            try
            {
                return File.Exists(RssBanditApplication.GetSubscriptionTreeStateFileName());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void InitiatePopulateTreeFeeds()
        {
            if (owner == null)
            {
                //Probably should log an error here
                SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
                return;
            }
            if (owner.FeedHandler.FeedsListOK == false)
            {
                SetGuiStateFeedback(SR.GUIStatusNoFeedlistFile, ApplicationTrayState.NormalIdle);
                return;
            }

            //Ensure we update the UI in the correct thread. Since this method is likely 
            //to have been called from a thread that is not the UI thread we should ensure 
            //that calls to UI components are actually made from the UI thread or marshalled
            //accordingly. 
            InvokeOnGui(delegate
            {
                PopulateFeedSubscriptions(owner.FeedHandler.GetCategories().Values, owner.FeedHandler.GetFeeds(),
                                          RssBanditApplication.DefaultCategory);
                PopulateTreeSpecialFeeds();
            });
        }

        private void CheckForFlaggedNodeAndCreate(INewsItem ri)
        {
            ISmartFolder isf;
            TreeFeedsNodeBase tn = null;
            TreeFeedsNodeBase root = _flaggedFeedsNodeRoot; //this.GetRoot(RootFolderType.SmartFolders);

            if (ri.FlagStatus == Flagged.FollowUp && _flaggedFeedsNodeFollowUp == null)
            {
                // not yet created
                _flaggedFeedsNodeFollowUp = new FlaggedItemsNode(Flagged.FollowUp, owner.FlaggedItemsFeed,
                                                                 SR.FeedNodeFlaggedForFollowUpCaption,
                                                                 Resource.SubscriptionTreeImage.RedFlag,
                                                                 Resource.SubscriptionTreeImage.RedFlagSelected,
                                                                 _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeFollowUp);
                isf = _flaggedFeedsNodeFollowUp as ISmartFolder;
                tn = _flaggedFeedsNodeFollowUp;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Read && _flaggedFeedsNodeRead == null)
            {
                // not yet created
                _flaggedFeedsNodeRead = new FlaggedItemsNode(Flagged.Read, owner.FlaggedItemsFeed,
                                                             SR.FeedNodeFlaggedForReadCaption,
                                                             Resource.SubscriptionTreeImage.GreenFlag,
                                                             Resource.SubscriptionTreeImage.GreenFlagSelected,
                                                             _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeRead);
                isf = _flaggedFeedsNodeRead as ISmartFolder;
                tn = _flaggedFeedsNodeRead;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Review && _flaggedFeedsNodeReview == null)
            {
                // not yet created
                _flaggedFeedsNodeReview = new FlaggedItemsNode(Flagged.Review, owner.FlaggedItemsFeed,
                                                               SR.FeedNodeFlaggedForReviewCaption,
                                                               Resource.SubscriptionTreeImage.YellowFlag,
                                                               Resource.SubscriptionTreeImage.YellowFlagSelected,
                                                               _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeReview);
                isf = _flaggedFeedsNodeReview as ISmartFolder;
                tn = _flaggedFeedsNodeReview;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Forward && _flaggedFeedsNodeForward == null)
            {
                // not yet created
                _flaggedFeedsNodeForward = new FlaggedItemsNode(Flagged.Forward, owner.FlaggedItemsFeed,
                                                                SR.FeedNodeFlaggedForForwardCaption,
                                                                Resource.SubscriptionTreeImage.BlueFlag,
                                                                Resource.SubscriptionTreeImage.BlueFlagSelected,
                                                                _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeForward);
                isf = _flaggedFeedsNodeForward as ISmartFolder;
                tn = _flaggedFeedsNodeForward;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Reply && _flaggedFeedsNodeReply == null)
            {
                // not yet created
                _flaggedFeedsNodeReply = new FlaggedItemsNode(Flagged.Reply, owner.FlaggedItemsFeed,
                                                              SR.FeedNodeFlaggedForReplyCaption,
                                                              Resource.SubscriptionTreeImage.ReplyFlag,
                                                              Resource.SubscriptionTreeImage.ReplyFlagSelected,
                                                              _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeReply);
                isf = _flaggedFeedsNodeReply as ISmartFolder;
                tn = _flaggedFeedsNodeReply;
                if (isf != null) isf.UpdateReadStatus();
            }

            if (tn != null)
            {
                // overall settings
                tn.DataKey = owner.FlaggedItemsFeed.link + "?id=" + ri.FlagStatus;
            }
            //InitFeedDetailsCaption();
        }

        public void PopulateTreeSpecialFeeds()
        {
            treeFeeds.BeginUpdate();

            TreeFeedsNodeBase root = GetRoot(RootFolderType.SmartFolders);
            root.Nodes.Clear();

            _feedExceptionsFeedsNode = new ExceptionReportNode(SR.FeedNodeFeedExceptionsCaption,
                                                               Resource.SubscriptionTreeImage.Exceptions,
                                                               Resource.SubscriptionTreeImage.ExceptionsSelected,
                                                               _treeLocalFeedContextMenu);
            _feedExceptionsFeedsNode.DataKey = SpecialFeeds.ExceptionManager.GetInstance().link;
            ExceptionNode.UpdateReadStatus();

            _sentItemsFeedsNode = new SentItemsNode(owner.SentItemsFeed,
                                                    Resource.SubscriptionTreeImage.SentItems,
                                                    Resource.SubscriptionTreeImage.SentItems, _treeLocalFeedContextMenu);
            _sentItemsFeedsNode.DataKey = owner.SentItemsFeed.link;
            SentItemsNode.UpdateReadStatus();

            _watchedItemsFeedsNode = new WatchedItemsNode(owner.WatchedItemsFeed,
                                                          Resource.SubscriptionTreeImage.WatchedItems,
                                                          Resource.SubscriptionTreeImage.WatchedItemsSelected,
                                                          _treeLocalFeedContextMenu);
            _watchedItemsFeedsNode.DataKey = owner.WatchedItemsFeed.link;
            WatchedItemsNode.UpdateReadStatus();
            WatchedItemsNode.UpdateCommentStatus();

            _unreadItemsFeedsNode = new UnreadItemsNode(owner.UnreadItemsFeed,
                                                        Resource.SubscriptionTreeImage.WatchedItems,
                                                        Resource.SubscriptionTreeImage.WatchedItemsSelected,
                                                        _treeLocalFeedContextMenu);
            _unreadItemsFeedsNode.DataKey = owner.UnreadItemsFeed.link;
            UnreadItemsNode.UpdateReadStatus();

            _deletedItemsFeedsNode = new WasteBasketNode(owner.DeletedItemsFeed,
                                                         Resource.SubscriptionTreeImage.WasteBasketEmpty,
                                                         Resource.SubscriptionTreeImage.WasteBasketEmpty,
                                                         _treeLocalFeedContextMenu);
            _deletedItemsFeedsNode.DataKey = owner.DeletedItemsFeed.link;
            DeletedItemsNode.UpdateReadStatus();

            _flaggedFeedsNodeRoot = new FlaggedItemsRootNode(SR.FeedNodeFlaggedFeedsCaption,
                                                             Resource.SubscriptionTreeImage.SubscriptionsCategory,
                                                             Resource.SubscriptionTreeImage.
                                                                 SubscriptionsCategoryExpanded,
                                                             null);

            root.Nodes.AddRange(
                new UltraTreeNode[]
                    {
                        _unreadItemsFeedsNode,
                        _watchedItemsFeedsNode,
                        _flaggedFeedsNodeRoot,
                        _feedExceptionsFeedsNode,
                        _sentItemsFeedsNode,
                        _deletedItemsFeedsNode
                    });

            // method gets called more than once, reset the nodes:
            _flaggedFeedsNodeFollowUp = _flaggedFeedsNodeRead = null;
            _flaggedFeedsNodeReview = _flaggedFeedsNodeForward = null;
            _flaggedFeedsNodeReply = null;

            foreach (NewsItem ri in owner.FlaggedItemsFeed.Items)
            {
                CheckForFlaggedNodeAndCreate(ri);

                if (_flaggedFeedsNodeFollowUp != null && _flaggedFeedsNodeRead != null &&
                    _flaggedFeedsNodeReview != null && _flaggedFeedsNodeForward != null &&
                    _flaggedFeedsNodeReply != null)
                {
                    break;
                }
            }

            bool expandRoots = ! IsTreeStateAvailable();
            root.Expanded = expandRoots;

            FinderRootNode froot = (FinderRootNode) GetRoot(RootFolderType.Finder);
            SyncFinderNodes(froot);
            if (expandRoots)
                froot.ExpandAll();

            treeFeeds.EndUpdate();
        }

        public void SyncFinderNodes()
        {
            SyncFinderNodes((FinderRootNode) GetRoot(RootFolderType.Finder));
        }

        private void SyncFinderNodes(FinderRootNode finderRoot)
        {
            if (finderRoot == null)
                return;
            finderRoot.Nodes.Clear();
            finderRoot.InitFromFinders(owner.FinderList, _treeSearchFolderContextMenu);
        }

        public void PopulateFeedSubscriptions(ICollection<INewsFeedCategory> categories, IDictionary<string, INewsFeed> feedsTable,
                                              string defaultCategory)
        {
            EmptyListView();
            TreeFeedsNodeBase root = GetRoot(RootFolderType.MyFeeds);
            try
            {
                treeFeeds.BeginUpdate();

                TreeFeedsNodeBase tn;
                // reset nodes and unread counter
                root.Nodes.Clear();
                UpdateTreeNodeUnreadStatus(root, 0);

                UnreadItemsNode.Items.Clear();

                Hashtable categoryTable = new Hashtable();
                List<INewsFeedCategory> categoryList = new List<INewsFeedCategory>(categories);

                foreach (INewsFeed f in feedsTable.Values)
                {
                    if (Disposing)
                        return;

                    if (RssHelper.IsNntpUrl(f.link))
                    {
                        tn = new FeedNode(f.title, Resource.SubscriptionTreeImage.Nntp,
                                          Resource.SubscriptionTreeImage.NntpSelected,
                                          _treeFeedContextMenu);
                    }
                    else
                    {
                        tn = new FeedNode(f.title, Resource.SubscriptionTreeImage.Feed,
                                          Resource.SubscriptionTreeImage.FeedSelected,
                                          _treeFeedContextMenu,
                                          (owner.Preferences.UseFavicons ? LoadFavicon(f.favicon) : null));
                    }

                    //interconnect for speed:
                    tn.DataKey = f.link;
                    f.Tag = tn;

                    string category = (f.category ?? String.Empty);

                    TreeFeedsNodeBase catnode;
                    if (categoryTable.ContainsKey(category))
                        catnode = (TreeFeedsNodeBase)categoryTable[category];
                    else
                    {
                        catnode = TreeHelper.CreateCategoryHive(root, category, _treeCategoryContextMenu);
                        categoryTable.Add(category, catnode);
                    }

                    catnode.Nodes.Add(tn);

                    SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);

                    if (f.containsNewMessages)
                    {
                        IList<INewsItem> unread = FilterUnreadFeedItems(f);
                        if (unread.Count > 0)
                        {
                            // we build up the tree, so the call to 
                            // UpdateReadStatus(tn , 0) is not neccesary:
                            UpdateTreeNodeUnreadStatus(tn, unread.Count);
                            UnreadItemsNode.Items.AddRange(unread);
                            UnreadItemsNode.UpdateReadStatus();
                        }
                    }

                    if (f.containsNewComments)
                    {
                        UpdateCommentStatus(tn, f);
                    }

                    for (int i = 0; i < categoryList.Count; i++)
                    {
                        if (categoryList[i].Value.Equals(category))
                        {
                            categoryList.RemoveAt(i);
                            break;
                        }
                    }
                }

                //add categories, we not already have
                foreach (INewsFeedCategory c in categoryList)
                {
                    TreeHelper.CreateCategoryHive(root, c.Value, _treeCategoryContextMenu);
                }
            }
            finally
            {
                treeFeeds.EndUpdate();
            }

            if (Disposing)
                return;

            TreeSelectedFeedsNode = root;

            if (! IsTreeStateAvailable())
                root.Expanded = true;

            // also this one:
            DelayTask(DelayedTasks.SyncRssSearchTree);

            SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);

            //we'll need to fetch favicons for the newly loaded/imported feeds
            _faviconsDownloaded = false;
        }

        /// <summary>
        /// Sets the default expansion tree node states.
        /// Currently expand all root nodes.
        /// </summary>
        internal void SetDefaultExpansionTreeNodeState()
        {
            foreach (TreeFeedsNodeBase node in _roots)
                node.Expanded = true;
        }

        /// <summary>
        /// This opens the downloaded file in the users target application associated with that 
        /// file type. 
        /// </summary>
        /// <param name="enclosure">The enclosure to launch or play</param>
        private void PlayEnclosure(DownloadItem enclosure)
        {
            if (enclosure == null)
                return;

            string fileName = Path.Combine(enclosure.TargetFolder, enclosure.File.LocalName);

            if (string.IsNullOrEmpty(fileName))
                return;
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = fileName;
                    p.Start();
                }
            }
            catch (Exception ex)
            {
                //we don't want to show the user an error if they cancelled executing the file 
                //after getting a security prompt. 
                Win32Exception ex32 = ex as Win32Exception;
                if ((ex32 == null) || (ex32.NativeErrorCode != 1223))
                {
                    owner.MessageError(SR.ExceptionProcessStartToPlayEnclosure(fileName, ex.Message));
                    RssBanditApplication.PublishException(ex);
                }
            }
        }

        /// <summary>
        /// Loads a favicon from the cache 
        /// </summary>
        /// <param name="name">The name of the favicon</param>
        /// <returns>The favicon as an image or null if there was an error loading the image</returns>
        private Image LoadFavicon(string name)
        {
            Image favicon = null;

            try
            {
//if there is a favicon, load it from disk and resize to 16x16 if necessary
                if (!string.IsNullOrEmpty(name))
                {
                    if (_favicons.ContainsKey(name))
                    {
                        return _favicons[name];
                    }

                    string location = Path.Combine(RssBanditApplication.GetFeedFileCachePath(), name);
                    if (! File.Exists(location))
                        return null;

                    if (String.Compare(Path.GetExtension(location), ".ico", true) == 0)
                        try
                        {
                            // looks like an ICO:
                            //using (MultiIcon ico = new MultiIcon(location))
                            //{
                            //    Icon smallest = ico.FindIcon(MultiIcon.DisplayType.Smallest);
                            //    //HACK: this is a workaround to the AccessViolationException caused
                            //    // on call .ToBitmap(), if the ico.Width is != ico.Height (CLR 2.0)
                            //    if (smallest.Width != smallest.Height)
                            //    {
                            //        return null;
                            //    }
                            //    //resize, but do not save:
                            //    favicon = ResizeFavicon(smallest.ToBitmap(), null);
                            //}
                            using (Icon ico = new Icon(location, new Size(16, 16)))
                            {
                                if(!Win32.IsOSAtLeastWindowsVista)
                                {
                                    //HACK: this is a workaround to the AccessViolationException caused
                                    // on call .ToBitmap(), if the ico.Width is != ico.Height (CLR 2.0)
                                    // XP and below can't handle non-square icons
                                    if (ico.Width != ico.Height)
                                        return null;
                                }

                                favicon = ResizeFavicon(ico.ToBitmap(), null);
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Debug("LoadFavicon(" + name + ") failed with error:", e);
                            // may happens, if we just downloaded a new icon from Web, that is not a real ICO (e.g. .png)
                        }
                    // fallback to init an icon from other image file formats:
                    if (favicon == null)
                        favicon = ResizeFavicon(Image.FromFile(location, true), location);

                    lock (_favicons)
                    {
                        if (!_favicons.ContainsKey(name))
                            _favicons.Add(name, favicon);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Debug("LoadFavicon(" + name + ") failed with error:", e);
            }

            return favicon;
        }

        /// <summary>
        /// Resizes the image to 16x16 so it can be used as a favicon in the treeview
        /// </summary>
        /// <param name="toResize"></param>
        /// <param name="location">The name of the image on the file system so it can be saved if 
        /// if resized. </param>
        /// <returns></returns>
        private static Image ResizeFavicon(Image toResize, string location)
        {
            if ((toResize.Height == 16) && (toResize.Width == 16))
            {
                return toResize;
            }
            else
            {
                Bitmap result = new Bitmap(16, 16, toResize.PixelFormat);
                result.SetResolution(toResize.HorizontalResolution, toResize.VerticalResolution);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(toResize, 0, 0, 16, 16);
                }
                toResize.Dispose();
                if (location != null)
                    result.Save(location);

                return result;
            }
        }

        private void PopulateTreeRssSearchScope()
        {
            if (searchPanel != null)
                searchPanel.PopulateTreeRssSearchScope(GetRoot(RootFolderType.MyFeeds), _treeImages);
        }

        /// <summary>
        /// Used to jump/navigate a web-link (url). Function will create 
        /// on demand a tabpage named in parameter <c>tab</c>, move it to front
        /// and open/navigate a web browser with the provided <c>url</c>.
        /// </summary>
        /// <param name="url">Web-Link to navigate to</param>
        /// <param name="tab">tabpage title name</param>
        /// <param name="createNewTab">true to force creation of a new Tab</param>
        /// <param name="setFocus">true to force brower Tab activation (move to foreground, set focus)</param>
        public void DetailTabNavigateToUrl(string url, string tab, bool createNewTab, bool setFocus)
        {
            Debug.Assert(!InvokeRequired, "DetailTabNavigateToUrl() from Non-UI Thread called");

            if (owner.Preferences.OpenNewTabsInBackground)
            {
                setFocus = false;
            }

            if (string.IsNullOrEmpty(url))
                return;

            if (url == "about:blank" && !createNewTab)
                return;

            if (string.IsNullOrEmpty(tab))
                tab = "Web Link";

            HtmlControl hc = null;

            DockControl previousDoc, currentDoc;
            previousDoc = currentDoc = _docContainer.ActiveDocument;
            ITabState docState = (ITabState) currentDoc.Tag;

            if (!docState.CanClose)
            {
                // Feed Detail doc tab

                if (!createNewTab && owner.Preferences.ReuseFirstBrowserTab)
                {
                    foreach (DockControl c in currentDoc.LayoutSystem.Controls)
                    {
                        if (c != currentDoc)
                        {
                            // reuse first docTab not equal to news item listview container
                            hc = (HtmlControl) c.Controls[0];
                            break;
                        }
                    }
                }
            }
            else if (!createNewTab)
            {
                // web doc tab
                // reuse same tab
                hc = (HtmlControl) _docContainer.ActiveDocument.Controls[0];
            }

            if (hc == null)
            {
                // create new doc tab with a contained web browser

                hc = CreateAndInitIEControl(tab);
                DockControl doc = new DockControl(hc, tab);
                doc.Tag = new WebTabState(tab, url);
                hc.Tag = doc; // store the doc the browser belongs to
                _docContainer.AddDocument(doc);
                if (Win32.IsOSAtLeastWindowsXP)
                    ColorEx.ColorizeOneNote(doc, ++_webTabCounter);

                //old: do NOT activate, if the focus have not to be set!
                //hc.Activate();	// so users do not have to explicitly click into the browser area after navigation for keyboard scrolling, etc.
                if (setFocus)
                {
                    hc.Activate();
                        // so users do not have to explicitly click into the browser area after navigation for keyboard scrolling, etc.
                    currentDoc = (DockControl) hc.Tag;
                }
                else
                    doc.Activate();
            }
            else
            {
                currentDoc = (DockControl) hc.Tag;
            }

            // move to front, or keep the current			
            currentDoc.Activate();
            _docContainer.ActiveDocument = (setFocus ? currentDoc : previousDoc);

            hc.Navigate(url);
        }

        private HtmlControl CreateAndInitIEControl(string tabName)
        {
            HtmlControl hc = new HtmlControl();
            ComponentResourceManager resources = new ComponentResourceManager(typeof (WinGuiMain));

            hc.BeginInit();
            // we just take over some generic resource settings from htmlDetail:
            hc.AllowDrop = true;
            resources.ApplyResources(hc, "htmlDetail");
            hc.Name = tabName;
            hc.OcxState = ((AxHost.State) (resources.GetObject("htmlDetail.OcxState")));
            helpProvider1.SetShowHelp(hc, ((bool) (resources.GetObject("htmlDetail.ShowHelp"))));
            hc.ContainingControl = this;

            //hc.AccessibleDescription = resources.GetString("htmlDetail.AccessibleDescription");
            //hc.AccessibleName = tabName;
            //hc.AllowDrop = true;
            //hc.ContainingControl = this;
            //this.helpProvider1.SetHelpKeyword(hc, resources.GetString("htmlDetail.HelpKeyword"));
            //this.helpProvider1.SetHelpNavigator(hc, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("htmlDetail.HelpNavigator"))));
            //this.helpProvider1.SetHelpString(hc, resources.GetString("htmlDetail.HelpString"));
            //hc.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("htmlDetail.ImeMode")));
            //hc.Name = tabName;
            //hc.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("htmlDetail.OcxState")));
            //hc.RightToLeft = ((bool)(resources.GetObject("htmlDetail.RightToLeft")));
            //this.helpProvider1.SetShowHelp(hc, ((bool)(resources.GetObject("htmlDetail.ShowHelp"))));

            hc.EndInit();

            hc.ScriptEnabled = owner.Preferences.BrowserJavascriptAllowed;
            hc.JavaEnabled = owner.Preferences.BrowserJavaAllowed;

            hc.ActiveXEnabled = owner.Preferences.BrowserActiveXAllowed;
            HtmlControl.SetInternetFeatureEnabled(
                InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL,
                SetFeatureFlag.SET_FEATURE_ON_THREAD_INTERNET,
                hc.ActiveXEnabled);

            hc.BackroundSoundEnabled = owner.Preferences.BrowserBGSoundAllowed;
            hc.VideoEnabled = owner.Preferences.BrowserVideoAllowed;
            hc.ImagesDownloadEnabled = owner.Preferences.BrowserImagesAllowed;
            hc.SilentModeEnabled = true;
            hc.Border3d = true;

            hc.StatusTextChanged += OnWebStatusTextChanged;
            hc.BeforeNavigate += OnWebBeforeNavigate;
            hc.NavigateComplete += OnWebNavigateComplete;
            hc.DocumentComplete += OnWebDocumentComplete;
            hc.TitleChanged += OnWebTitleChanged;
            hc.CommandStateChanged += OnWebCommandStateChanged;
            hc.NewWindow += OnWebNewWindow;
            hc.ProgressChanged += OnWebProgressChanged;
            hc.TranslateAccelerator += OnWebTranslateAccelerator;
            hc.OnQuit += OnWebQuit;
            return hc;
        }

        private static ITabState GetTabStateFor(HtmlControl control)
        {
            if (control == null) return null;
            DockControl doc = (DockControl) control.Tag;
            if (doc == null) return null;
            ITabState state = (ITabState) doc.Tag;
            return state;
        }

        private bool UrlRequestHandledExternally(string url, bool forceNewTab)
        {
            if (forceNewTab || BrowserBehaviorOnNewWindow.OpenNewTab == owner.Preferences.BrowserOnNewWindow)
            {
                return false;
            }
            else if (BrowserBehaviorOnNewWindow.OpenDefaultBrowser == owner.Preferences.BrowserOnNewWindow)
            {
                owner.NavigateToUrlInExternalBrowser(url);
            }
            else if (BrowserBehaviorOnNewWindow.OpenWithCustomExecutable == owner.Preferences.BrowserOnNewWindow)
            {
                try
                {
                    Process.Start(owner.Preferences.BrowserCustomExecOnNewWindow, url);
                }
                catch (Exception  ex)
                {
                    if (
                        owner.MessageQuestion(
                            SR.ExceptionStartBrowserCustomExecMessage(owner.Preferences.BrowserCustomExecOnNewWindow,
                                                                      ex.Message, url)) == DialogResult.Yes)
                    {
                        DetailTabNavigateToUrl(url, null, true, true);
                    }
                }
            }
            else
            {
                Debug.Assert(false, "Unhandled BrowserBehaviorOnNewWindow");
            }
            return true;
        }

        /// <summary>
        /// Used to initiate a browse action.
        /// </summary>
        /// <param name="action">The specific action to perform</param>
        public void RequestBrowseAction(BrowseAction action)
        {
            if (_docContainer.ActiveDocument == _docFeedDetails)
            {
                switch (action)
                {
                    case BrowseAction.NavigateBack:
                        NavigateToHistoryEntry(_feedItemImpressionHistory.GetPrevious());
                        break;
                    case BrowseAction.NavigateForward:
                        NavigateToHistoryEntry(_feedItemImpressionHistory.GetNext());
                        break;
                    case BrowseAction.DoRefresh:
                        OnTreeFeedAfterSelectManually(TreeSelectedFeedsNode); //??
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // _docContainer.ActiveDocument != _docFeedDetails

                HtmlControl wb = (HtmlControl) _docContainer.ActiveDocument.Controls[0];
                try
                {
                    switch (action)
                    {
                        case BrowseAction.NavigateCancel:
                            wb.Stop();
                            break;
                        case BrowseAction.NavigateBack:
                            wb.GoBack();
                            break;
                        case BrowseAction.NavigateForward:
                            wb.GoForward();
                            break;
                        case BrowseAction.DoRefresh:
                            object level = 2;
                            wb.Refresh2(ref level);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    /* Can't do command */
                    ;
                }
            }
            DeactivateWebProgressInfo();
        }

        /// <summary>
        /// Renders the context menu and determines which options are enabled/visible. 
        /// </summary>
        public void RefreshListviewContextMenu()
        {
            NewsItem item = null;
            IList selectedItems = GetSelectedLVItems();

            if (selectedItems.Count > 0)
                item = ((ThreadedListViewItem) selectedItems[0]).Key as NewsItem;
            if ((selectedItems.Count == 1) && (item != null))
            {
                RefreshListviewContextMenu(item);
            }
            else
            {
                RefreshListviewContextMenu(null);
            }
        }

        /// <summary>
        /// Renders the context menu and determines which options are enabled/visible. 
        /// </summary>
        public void RefreshListviewContextMenu(NewsItem item)
        {
            if (item != null)
            {
                owner.Mediator.SetVisible("+cmdWatchItemComments", "+cmdFeedItemPostReply");
                owner.Mediator.SetEnabled("+cmdCopyNewsItem");

                if (listFeedItems.Visible)
                {
                    owner.Mediator.SetVisible("+cmdColumnChooserMain");
                }
                else
                {
                    owner.Mediator.SetVisible("-cmdColumnChooserMain");
                }

                if (item.BeenRead)
                {
                    owner.Mediator.SetVisible("+cmdMarkSelectedFeedItemsUnread", "-cmdMarkSelectedFeedItemsRead");
                }
                else
                {
                    owner.Mediator.SetVisible("-cmdMarkSelectedFeedItemsUnread", "+cmdMarkSelectedFeedItemsRead");
                }

                _listContextMenuDownloadAttachmentsSeparator.Visible = false;
                owner.Mediator.SetVisible("-cmdDownloadAttachment");

                if (item.Enclosures != null && item.Enclosures.Count > 0)
                {
                    _listContextMenuDownloadAttachmentsSeparator.Visible = true;
                    owner.Mediator.SetVisible("+cmdDownloadAttachment");
                    _listContextMenuDownloadAttachment.MenuItems.Clear();

                    foreach (Enclosure enc in item.Enclosures)
                    {
                        int index = enc.Url.LastIndexOf("/");
                        string fileName;

                        if ((index != -1) && (index + 1 < enc.Url.Length))
                        {
                            fileName = enc.Url.Substring(index + 1);
                        }
                        else
                        {
                            fileName = enc.Url;
                        }

                        AppContextMenuCommand downloadFileMenuItem =
                            new AppContextMenuCommand("cmdDownloadAttachment<" + fileName,
                                                      owner.Mediator, new ExecuteCommandHandler(CmdDownloadAttachment),
                                                      fileName, fileName, _shortcutHandler);

                        _listContextMenuDownloadAttachment.MenuItems.AddRange(new MenuItem[] {downloadFileMenuItem});
                    }
                }

                owner.Mediator.SetChecked("-cmdWatchItemComments");

                if (string.IsNullOrEmpty(item.CommentRssUrl) && (item.CommentCount == NewsItem.NoComments))
                {
                    owner.Mediator.SetEnabled("-cmdWatchItemComments");
                }
                else
                {
                    owner.Mediator.SetEnabled("+cmdWatchItemComments");

                    if (item.WatchComments)
                    {
                        owner.Mediator.SetChecked("+cmdWatchItemComments");
                    }
                }
            }
            else
            {
                _listContextMenuDownloadAttachmentsSeparator.Visible = false;

                owner.Mediator.SetVisible("+cmdMarkSelectedFeedItemsUnread", "+cmdMarkSelectedFeedItemsRead");
                owner.Mediator.SetVisible("-cmdWatchItemComments", "-cmdColumnChooserMain", "-cmdFeedItemPostReply",
                                          "-cmdDownloadAttachment");
                owner.Mediator.SetEnabled("-cmdCopyNewsItem");
            }

            if (CurrentSelectedFeedsNode is WasteBasketNode)
            {
                owner.Mediator.SetVisible("+cmdRestoreSelectedNewsItems");
            }
            else
            {
                owner.Mediator.SetVisible("-cmdRestoreSelectedNewsItems");
            }
        }

        public void RefreshTreeFeedContextMenus(TreeFeedsNodeBase feedsNode)
        {
            owner.Mediator.SetEnabled(false, "cmdColumnChooserResetToDefault");
            if (feedsNode.Type == FeedNodeType.Feed || feedsNode.Type == FeedNodeType.Category)
            {
                owner.Mediator.SetEnabled(true, "cmdColumnChooserResetToDefault");
                owner.Mediator.SetEnabled(
                    "+cmdFlagNewsItem", "+cmdNavigateToFeedHome", "+cmdNavigateToFeedCosmos",
                    "+cmdViewSourceOfFeed", "+cmdValidateFeed");

                if (RssHelper.IsNntpUrl(feedsNode.DataKey))
                {
                    _feedInfoContextMenu.Enabled = false;
                }
                else
                {
                    _feedInfoContextMenu.Enabled = true;
                }
            }
            else if (feedsNode.Type == FeedNodeType.SmartFolder)
            {
                owner.Mediator.SetEnabled(
                    "-cmdFlagNewsItem", "-cmdNavigateToFeedHome", "-cmdNavigateToFeedCosmos",
                    "-cmdViewSourceOfFeed", "-cmdValidateFeed");
                if ((feedsNode as FlaggedItemsNode) != null)
                    owner.Mediator.SetEnabled("+cmdFlagNewsItem"); // allow re-flag of items
            }
            else if (feedsNode.Type == FeedNodeType.Finder)
            {
                owner.Mediator.SetEnabled("-cmdDeleteAllFinders", "+cmdDeleteFinder", "+cmdShowFinderProperties",
                                          "+cmdFlagNewsItem", "-cmdSubscribeToFinderResult");
                FinderNode agfn = feedsNode as FinderNode;
                if (agfn != null && agfn == _searchResultNode && agfn.Finder != null)
                {
                    bool extResult = !string.IsNullOrEmpty(agfn.Finder.ExternalSearchUrl);
                    owner.Mediator.SetEnabled(extResult, "cmdSubscribeToFinderResult");
                    owner.Mediator.SetEnabled(extResult && agfn.Finder.ExternalResultMerged, "cmdShowFinderProperties");
                }
                if (agfn != null && agfn.Finder != null)
                {
                    owner.Mediator.SetChecked(!agfn.Finder.ShowFullItemContent, "cmdFinderShowFullItemText");
                }
            }
            else if (feedsNode.Type == FeedNodeType.FinderCategory)
            {
                owner.Mediator.SetEnabled("+cmdDeleteAllFinders", "+cmdDeleteFinder", "-cmdShowFinderProperties");
            }
            else if (feedsNode.Type == FeedNodeType.Root)
            {
                if ((feedsNode as FinderRootNode) != null)
                    owner.Mediator.SetEnabled("+cmdDeleteAllFinders", "-cmdDeleteFinder", "-cmdShowFinderProperties");
            }
        }

        private void MoveFeedDetailsToFront()
        {
            if (_docContainer.ActiveDocument != _docFeedDetails)
                _docContainer.ActiveDocument = _docFeedDetails;
        }

        private void RefreshDocumentState(Control doc)
        {
            if (doc == null)
                return;

            ITabState state = doc.Tag as ITabState;
            if (state == null)
                return;

            if (state.CanClose)
            {
                // not listview/detail pane doc
                doc.Text = StringHelper.ShortenByEllipsis(state.Title, 30);
            }

            if (_docContainer.ActiveDocument == doc)
            {
                SetTitleText(state.Title);
                UrlText = state.Url;

                historyMenuManager.ReBuildBrowserGoBackHistoryCommandItems(state.GoBackHistoryItems(10));
                historyMenuManager.ReBuildBrowserGoForwardHistoryCommandItems(state.GoForwardHistoryItems(10));

                owner.Mediator.SetEnabled(state.CanGoBack, "cmdBrowserGoBack");
                owner.Mediator.SetEnabled(state.CanGoForward, "cmdBrowserGoForward");
            }
        }

        public void SetGuiStateINetConnected(bool connected)
        {
            try
            {
                StatusBarPanel p = statusBarConnectionState; //_status.Panels[2];
                if (connected)
                {
                    p.Icon = Resource.LoadIcon("Resources.Connected.ico");
                }
                else
                {
                    p.Icon = Resource.LoadIcon("Resources.Disconnected.ico");
                }
            }
            catch
            {
            }
            _status.Refresh();
        }

        public void SetGuiStateFeedback(string text)
        {
            try
            {
                StatusBarPanel p = statusBarRssParser; //_status.Panels[3];
                if (!p.Text.Equals(text))
                {
                    p.Text = p.ToolTipText = text;
                    if (text.Length == 0 && p.Icon != null)
                    {
                        p.Icon = null;
                    }
                    _status.Refresh();
                }
            }
            catch
            {
            }
        }

        public void SetGuiStateFeedback(string text, ApplicationTrayState state)
        {
            try
            {
                StatusBarPanel p = statusBarRssParser; //_status.Panels[3];
                if (state == ApplicationTrayState.NormalIdle)
                {
                    _timerResetStatus.Start();
                    if (!string.IsNullOrEmpty(text))
                    {
                        SetGuiStateFeedback(text);
                    }
                }
                else
                {
                    _timerResetStatus.Stop();
                    SetGuiStateFeedback(text);
                    _trayManager.SetState(state);
                    if (state == ApplicationTrayState.BusyRefreshFeeds)
                    {
                        if (p.Icon == null)
                        {
                            p.Icon = Resource.LoadIcon("Resources.feedRefresh.ico");
                            _status.Refresh();
                        }
                    }
                    else
                    {
                        if (p.Icon != null)
                        {
                            p.Icon = null;
                            _status.Refresh();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void SetBrowserStatusBarText(string text)
        {
            try
            {
                StatusBarPanel p = statusBarBrowser; //_status.Panels[0];
                if (!p.Text.Equals(text))
                {
                    p.Text = text;
                    _status.Refresh();
                }
            }
            catch
            {
            }
        }

        public void SetSearchStatusText(string text)
        {
            SetGuiStateFeedback(text);
        }

        public void UpdateCategory(bool forceRefresh)
        {
            TreeFeedsNodeBase selectedNode = CurrentSelectedFeedsNode;
            if (selectedNode == null) return;

            owner.BeginRefreshCategoryFeeds(selectedNode.CategoryStoreName, forceRefresh);
        }


        /// <summary>
        /// Initiate a async. call to RssParser.RefreshFeeds(force_download)
        /// </summary>
        /// <param name="force_download"></param>
        public void UpdateAllCommentFeeds(bool force_download)
        {
            if (_timerRefreshCommentFeeds.Enabled)
                _timerRefreshCommentFeeds.Stop();

            owner.BeginRefreshCommentFeeds(force_download);
        }

        /// <summary>
        /// Initiate a async. call to FeedSource.RefreshFeeds(force_download)
        /// </summary>
        /// <param name="force_download"></param>
        public void UpdateAllFeeds(bool force_download)
        {
            if (_timerRefreshFeeds.Enabled)
                _timerRefreshFeeds.Stop();

            TreeFeedsNodeBase root = GetRoot(RootFolderType.MyFeeds);
            _lastUnreadFeedItemCountBeforeRefresh = root.UnreadCount;
            owner.BeginRefreshFeeds(force_download);
        }

        public void OnAllAsyncUpdateCommentFeedsFinished()
        {
#if !NOAUTO_REFRESH
            // restart the feeds auto-refresh timer:
            if (!_timerRefreshCommentFeeds.Enabled)
                _timerRefreshCommentFeeds.Start();
#endif
        }

        public void OnAllAsyncUpdateFeedsFinished()
        {
#if !NOAUTO_REFRESH
            // restart the feeds auto-refresh timer:
            if (!_timerRefreshFeeds.Enabled)
                _timerRefreshFeeds.Start();
#endif
            if (!_faviconsDownloaded && owner.Preferences.UseFavicons)
            {
                try
                {
                    owner.FeedHandler.RefreshFavicons();
                }
                finally
                {
                    _faviconsDownloaded = true;
                }
            }
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
//			if (IdleTask.IsTask(IdleTasks.InitOnFinishLoading)) 
//			{
//				
//				IdleTask.RemoveTask(IdleTasks.InitOnFinishLoading);
//				Splash.Close();	
//				owner.BeginLoadingFeedlist();
//				owner.BeginLoadingSpecialFeeds();
//			} 
//			else 
            if (IdleTask.IsTask(IdleTasks.IndexAllItems))
            {
                IdleTask.RemoveTask(IdleTasks.IndexAllItems);
                try
                {
                    FeedSource.SearchHandler.CheckIndex(true);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("LuceneIndexer failed: " + ex);
                }
            }
        }

        /// <summary>
        /// Extracts the category of the selected node within the feeds tree.
        /// </summary>
        /// <returns>Category found, or DefaultCategory</returns>
        public string CategoryOfSelectedNode()
        {
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;

            if (tn != null)
            {
                if (tn.Type == FeedNodeType.Feed)
                {
                    INewsFeed f = owner.GetFeed(tn.DataKey);
                    if (f != null)
                    {
                        return f.category;
                    }
                    else
                    {
                        return tn.CategoryStoreName;
                    }
                }
                else if (tn.Type == FeedNodeType.Category || tn.Type == FeedNodeType.Root)
                {
                    return tn.CategoryStoreName;
                }
                else
                {
                    return RssBanditApplication.DefaultCategory;
                }
            }
            else
                return RssBanditApplication.DefaultCategory;
        }

        /// <summary>
        /// Add a new feed to the GUI tree view
        /// </summary>
        /// <param name="category">Feed Category</param>
        /// <param name="f">Feed</param>
        public void AddNewFeedNode(string category, INewsFeed f)
        {
            TreeFeedsNodeBase catnode;
            TreeFeedsNodeBase tn;

            if (RssHelper.IsNntpUrl(f.link))
            {
                tn = new FeedNode(f.title, Resource.SubscriptionTreeImage.Nntp,
                                  Resource.SubscriptionTreeImage.NntpSelected,
                                  _treeFeedContextMenu);
            }
            else
            {
                tn = new FeedNode(f.title, Resource.SubscriptionTreeImage.Feed,
                                  Resource.SubscriptionTreeImage.FeedSelected,
                                  _treeFeedContextMenu,
                                  (owner.Preferences.UseFavicons ? LoadFavicon(f.favicon) : null));
            }

            //interconnect for speed:
            tn.DataKey = f.link;
            f.Tag = tn;

            SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);

            category = (f.category == RssBanditApplication.DefaultCategory ? null : f.category);
            catnode = TreeHelper.CreateCategoryHive(GetRoot(RootFolderType.MyFeeds), category, _treeCategoryContextMenu);

            if (catnode == null)
                catnode = GetRoot(RootFolderType.MyFeeds);

            catnode.Nodes.Add(tn);
//			tn.Cells[0].Value = tn.Text;
//			tn.Cells[0].Appearance.Image = tn.Override.NodeAppearance.Image;
//			tn.Cells[0].Appearance.Cursor = CursorHand;

            if (f.containsNewMessages)
            {
                IList<INewsItem> unread = FilterUnreadFeedItems(f);
                if (unread.Count > 0)
                {
                    // we build up a new tree node, so the call to 
                    // UpdateReadStatus(tn , 0) is not neccesary:
                    UpdateTreeNodeUnreadStatus(tn, unread.Count);
                    UnreadItemsNode.Items.AddRange(unread);
                    UnreadItemsNode.UpdateReadStatus();
                }
            }

            if (f.containsNewComments)
            {
                UpdateCommentStatus(tn, f);
            }

            tn.BringIntoView();

            DelayTask(DelayedTasks.SyncRssSearchTree);
        }

        public void InitiateRenameFeedOrCategory()
        {
            if (CurrentSelectedFeedsNode != null)
                DoEditTreeNodeLabel();
        }

        public bool NodeEditingActive
        {
            get
            {
                return (CurrentSelectedFeedsNode != null && CurrentSelectedFeedsNode.IsEditing);
            }
        }

//		protected void DeleteFeed(TreeFeedsNodeBase tn) 
//		{
//			if (tn == null) tn = CurrentSelectedFeedsNode;
//			if (tn == null) return;
//			if(tn.Type != FeedNodeType.Feed) return;
//			this.UpdateTreeNodeUnreadStatus(tn, 0);
//			tn.UpdateCommentStatus(tn, 0);
//
//			if (tn.Selected) 
//			{ // not just right-clicked elsewhere on a node:
//				this.EmptyListView();					
//				this.htmlDetail.Clear();
//				//TreeSelectedNode = this.GetRoot(RootFolderType.MyFeeds);
//			}
//			
//			try 
//			{
//				NewsFeed f = owner.GetFeed(tn.DataKey);
//				if (f != null) {
//					UnreadItemsNodeRemoveItems(f);
//					WatchedItemsNodeRemoveItems(f);
//					f.Tag = null;
//				}
//				if (tn.DataKey != null)
//					owner.FeedHandler.DeleteFeed(tn.DataKey);
//				// next line causes OnTreeBefore-/AfterSelected events:
//				tn.Parent.Nodes.Remove(tn); 
//				CurrentSelectedFeedsNode = null;
//				this.DelayTask(DelayedTasks.SyncRssSearchTree);
//			} 
//			catch { /* ignore delete errors (may raised by FileCacheManager) */}
//		}


        private static bool NodeIsChildOf(UltraTreeNode tn, UltraTreeNode parent)
        {
            if (parent == null)
                return false;

            UltraTreeNode p = tn.Parent;
            while (p != null)
            {
                if (p == parent) return true;
                p = p.Parent;
            }
            return false;
        }


        /// <summary>
        /// Called on each finished successful favicon request.
        /// </summary>
        /// <param name="favicon"> The name of the favicon file</param> 
        /// <param name="feedUrls">The list of URLs that will utilize this favicon</param>		
        public void UpdateFavicon(string favicon, StringCollection feedUrls)
        {
            Image icon = null;
            if (!string.IsNullOrEmpty(favicon))
            {
                string location = Path.Combine(RssBanditApplication.GetFeedFileCachePath(), favicon);

                if (String.Compare(Path.GetExtension(location), ".ico", true) == 0)
                    try
                    {
                        // looks like an ICO:
                        //using (MultiIcon ico = new MultiIcon(location))
                        //{
                        //    Icon smallest = ico.FindIcon(MultiIcon.DisplayType.Smallest);
                        //    //HACK: this is a workaround to the AccessViolationException caused
                        //    // on call .ToBitmap(), if the ico.Width is != ico.Height (CLR 2.0)
                        //    if (smallest.Width == smallest.Height) //resize, but do not save:
                        //        icon = ResizeFavicon(smallest.ToBitmap(), null);
                        //}
                        using (Icon ico = new Icon(location, new Size(16, 16)))
                        {
                            if (!Win32.IsOSAtLeastWindowsVista)
                            {
                                icon = ResizeFavicon(ico.ToBitmap(), null);
                            }
                            else
                            {
                                //HACK: this is a workaround to the AccessViolationException caused
                                // on call .ToBitmap(), if the ico.Width is != ico.Height (CLR 2.0)
                                // XP and below can't handle non-square icons
                                if (ico.Width == ico.Height) //resize, but do not save:
                                    icon = ResizeFavicon(ico.ToBitmap(), null);
                            }

                            
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Debug("UpdateFavicon(" + location + ") failed with error:", e);
                        // may happens, if we just downloaded a new icon from Web, that is not a real ICO (e.g. .png)
                    }
                else
                    // fallback to init an icon from other image file formats:
                    try
                    {
                        icon = ResizeFavicon(Image.FromFile(location, true), location);
                    }
                    catch (Exception e)
                    {
                        _log.Debug("UpdateFavicon() failed", e);
                    }
            }

            if (feedUrls != null)
            {
                foreach (string feedUrl in feedUrls)
                {
                    TreeFeedsNodeBase tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
                    if (tn == null)
                    {
                        _log.Debug("TreeHelper.FindNode() could not find matching tree node for " + feedUrl);
                    }
                    else
                    {
                        tn.SetIndividualImage(icon);
                    }
                }
            }

            if (icon != null)
            {
                lock (_favicons)
                {
                    if (!_favicons.ContainsKey(favicon))
                        _favicons.Add(favicon, icon);
                }
                //<favicon> entries added to subscriptions.xml
                owner.SubscriptionModified(NewsFeedProperty.General);
                //this.owner.FeedlistModified = true; 
            }
        }


        /// <summary>
        /// Converts the tree view to using favicons as feed icons where available
        /// </summary>
        public void ApplyFavicons()
        {
            try
            {
                string[] keys;

                // The "CopyTo()" construct prevents against InvalidOpExceptions/ArgumentOutOfRange
                // exceptions and keep the loop alive if FeedsTable gets modified from other thread(s)					
                lock (owner.FeedHandler.GetFeeds())
                {
                    keys = new string[owner.FeedHandler.GetFeeds().Count];
                    if (owner.FeedHandler.GetFeeds().Count > 0)
                        owner.FeedHandler.GetFeeds().Keys.CopyTo(keys, 0);
                }


                //foreach(string sKey in FeedsTable.Keys){
                //  NewsFeed current = FeedsTable[sKey];	

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    string feedUrl = keys[i];
                    INewsFeed f = null;

                    if (!owner.FeedHandler.GetFeeds().TryGetValue(feedUrl,out f))
                    {
                        continue;
                    }

                    if (owner.Preferences.UseFavicons)
                    {
                        if (string.IsNullOrEmpty(f.favicon))
                        {
                            continue;
                        }


                        string location = Path.Combine(RssBanditApplication.GetFeedFileCachePath(), f.favicon);
                        Image icon = null;

                        if (_favicons.ContainsKey(f.favicon))
                        {
                            icon = _favicons[f.favicon];
                        }
                        else if(File.Exists(location))
                        {
                            try
                            {
                                
                                icon = ResizeFavicon(Image.FromFile(location), location);
                                lock (_favicons)
                                {
                                    if (!_favicons.ContainsKey(f.favicon))
                                        _favicons.Add(f.favicon, icon);
                                }
                            }
                            catch (Exception e)
                            {
                                //we had an issue loading or resizing the icon
                                _log.Error("Error in ApplyFavicons(): {0}", e);
                            }
                        } //else

                        if (icon != null)
                        {
                            TreeFeedsNodeBase tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
                            if (tn != null)
                            {
                                tn.SetIndividualImage(icon);
                            }
                        }
                    }
                    else
                    {
                        //if(owner.Preferences.UseFavicons){	

                        TreeFeedsNodeBase tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);

                        if (tn != null)
                        {
                            SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);
                        }
                    } //else
                } //for(int i...)
            }
            catch (InvalidOperationException ioe)
            {
// New feeds added to FeedsTable from another thread  

                _log.Error("ApplyFavicons - InvalidOperationException: {0}", ioe);
            }
        }


        /// <summary>
        /// Called on each finished successful comment feed refresh.
        /// </summary>
        /// <param name="feedUri">The original feed Uri</param>
        /// <param name="newFeedUri">The new feed Uri (if permamently moved)</param>
        public void UpdateCommentFeed(Uri feedUri, Uri newFeedUri)
        {
            IList<INewsItem> items;

            string feedUrl = feedUri.CanonicalizedUri();
            INewsFeed feed;
            TreeFeedsNodeBase tn;
            INewsItem item = null;
            bool modified = false;

            if (newFeedUri != null)
            {
                items = owner.CommentFeedsHandler.GetCachedItemsForFeed(newFeedUri.CanonicalizedUri());
            }
            else
            {
                items = owner.CommentFeedsHandler.GetCachedItemsForFeed(feedUrl);
            }

            //get the current number of comments on the item
            int commentCount = (items.Count == 0 ? NewsItem.NoComments : items.Count);

            if (!owner.CommentFeedsHandler.IsSubscribed(feedUrl) && (feedUri.IsFile || feedUri.IsUnc))
            {
                feedUrl = feedUri.LocalPath;
            }

            owner.CommentFeedsHandler.GetFeeds().TryGetValue(feedUrl, out feed);

            if (feed != null && feed.Tag != null)
            {
                INewsFeed itemFeed = (INewsFeed) feed.Tag;
                FeedInfo itemFeedInfo = owner.FeedHandler.GetFeedDetails(itemFeed.link) as FeedInfo;

                tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), itemFeed);

                if (tn != null && itemFeedInfo != null)
                {
                    lock (itemFeedInfo.ItemsList)
                    {
                        //locate NewsItem from original feed 
                        foreach (INewsItem ni in itemFeedInfo.ItemsList)
                        {
                            if (!string.IsNullOrEmpty(ni.CommentRssUrl) &&
                                feedUrl.Equals(ni.CommentRssUrl))
                            {
                                item = ni;
                                //some comment feeds place the post as the first entry in the comments feed
                                if (items.Contains(item))
                                {
                                    commentCount--;
                                }
                                break;
                            }
                        } //foreach


                        if (item == null)
                        {
                            //item has been deleted or expired from the cache
                            owner.CommentFeedsHandler.DeleteFeed(feedUrl);
                            owner.WatchedItemsFeed.Remove(feedUrl);
                            return;
                        }
                        else if (item.WatchComments)
                        {
                            if (commentCount > item.CommentCount)
                            {
                                itemFeed.containsNewComments = item.HasNewComments = true;
                                item.CommentCount = commentCount;
                                modified = true;
                            }

                            //fix up URL if it has moved 
                            if (newFeedUri != null && newFeedUri != feedUri)
                            {
                                if (newFeedUri.IsFile || newFeedUri.IsUnc)
                                    feedUrl = newFeedUri.LocalPath;
                                else
                                    feedUrl = newFeedUri.ToString();

                                item.CommentRssUrl = feedUrl;
                                modified = true;
                            }
                        } //if(item!= null && item.WatchComments)) 
                    } //lock(itemFeedInfo.ItemsList){


                    //to prevent bandwidth abuse we fetch comment feeds twice a day once 
                    //NewsItem is over a week old since they are rarely updated if ever
                    if ((DateTime.Now.Subtract(item.Date) > SevenDays))
                    {
                        feed.refreshrateSpecified = true;
                        feed.refreshrate = 12*60*60*1000; //twelve hours
                    }

                    if (itemFeed.containsNewComments)
                    {
                        //update tree view
                        UpdateCommentStatus(tn, itemFeedInfo.ItemsList, false);

                        //update list view 
                        bool categorized = false;
                        TreeFeedsNodeBase ftnSelected = TreeSelectedFeedsNode;

                        if (ftnSelected.Type == FeedNodeType.Category && NodeIsChildOf(tn, ftnSelected))
                            categorized = true;

                        if (tn.Selected || categorized)
                        {
                            ThreadedListViewItem lvItem = GetListViewItem(item.Id);
                            if (lvItem != null)
                            {
                                ApplyStyles(lvItem);
                            }
                        } //if (tn.Selected || categorized ) {
                    } //if(itemFeed.containsNewComments){

                    /* we need to write the feed to the cache if the CommentCount or the CommentRssUrl changed 
					 * for the NewsItem changed
					 */
                    if (modified)
                    {
                        owner.FeedHandler.ApplyFeedModifications(itemFeed.link);
                    }
                } //if (tn != null && itemFeedInfo != null) {
            } //if (feed != null && feed.Tag != null) {					
        }


        /// <summary>
        /// Called on each finished successful feed refresh.
        /// </summary>
        /// <param name="feedUrl">The original feed Uri</param>
        /// <param name="newFeedUri">The new feed Uri (if permamently moved)</param>
        /// <param name="modified">Really new items received</param>
        public void UpdateFeed(string feedUrl, Uri newFeedUri, bool modified)
        {
            Uri feedUri = null;
            try
            {
                feedUri = new Uri(feedUrl);
            }
            catch (Exception)
            {
            }

            UpdateFeed(feedUri, newFeedUri, modified);
        }

        /// <summary>
        /// Called on each finished successful feed refresh.
        /// </summary>
        /// <param name="feedUri">The original feed Uri</param>
        /// <param name="newFeedUri">The new feed Uri (if permamently moved)</param>
        /// <param name="modified">Really new items received</param>
        public void UpdateFeed(Uri feedUri, Uri newFeedUri, bool modified)
        {
            if (feedUri == null)
                return;

            IList<INewsItem> items;
            IList<INewsItem> unread = null;

            string feedUrl = feedUri.CanonicalizedUri();
            INewsFeed feed;
            TreeFeedsNodeBase tn;

            if (newFeedUri != null)
            {
                items = owner.FeedHandler.GetCachedItemsForFeed(newFeedUri.CanonicalizedUri());
            }
            else
            {
                items = owner.FeedHandler.GetCachedItemsForFeed(feedUrl);
            }

            if (!owner.FeedHandler.IsSubscribed(feedUrl) && (feedUri.IsFile || feedUri.IsUnc))
            {
                feedUrl = feedUri.LocalPath;
            }


            //feed = owner.FeedHandler.GetFeeds()[feedUrl];
            feed = null;

            if (owner.FeedHandler.GetFeeds().TryGetValue(feedUrl, out feed) && feed != null)
            {
                tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feed);
            }
            else
            {
                tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
            }

            if (tn != null)
            {
                if (newFeedUri != null && newFeedUri != feedUri)
                {
                    // changed/moved
                    if (newFeedUri.IsFile || newFeedUri.IsUnc)
                        feedUrl = newFeedUri.LocalPath;
                    else
                        feedUrl = newFeedUri.CanonicalizedUri();
                    tn.DataKey = feedUrl;
                    feed = owner.GetFeed(feedUrl);
                    if (feed != null)
                        feed.Tag = tn;
                }

                if (feed != null)
                {
                    SetSubscriptionNodeState(feed, tn, FeedProcessingState.Normal);

                    if (modified)
                    // if (feed.containsNewMessages) No longer applies due to syncing state from Google Reader & NewsGator Online
                    {
                       // if (modified)
                            owner.FeedWasModified(feed, NewsFeedProperty.FeedItemReadState);
						
						unread = FilterUnreadFeedItems(items, true);
                        UnreadItemsNodeRemoveItems(unread);
                        UnreadItemsNode.Items.AddRange(unread);
                        UpdateTreeNodeUnreadStatus(tn, unread.Count);
                        UnreadItemsNode.UpdateReadStatus();

                        if ((DisplayFeedAlertWindow.All == owner.Preferences.ShowAlertWindow ||
                             (DisplayFeedAlertWindow.AsConfiguredPerFeed == owner.Preferences.ShowAlertWindow &&
                              feed.alertEnabled)) &&
                            modified)
                        {
                            // new flag on feed, states if toast is enabled (off by default)
                            toastNotifier.Alert(tn.Text, tn.UnreadCount, unread);
                        }
                        
                    }

                    if (feed.containsNewComments)
                    {
                        if (modified)
                            owner.FeedWasModified(feed, NewsFeedProperty.FeedItemCommentCount);
                        UpdateCommentStatus(tn, items, false);
                    }
                }

                bool categorized = false;
                TreeFeedsNodeBase ftnSelected = TreeSelectedFeedsNode;

                if (ftnSelected != null)
                {
                    if (ftnSelected.Type == FeedNodeType.Category && NodeIsChildOf(tn, ftnSelected))
                        categorized = true;

                    if (ftnSelected is UnreadItemsNode && unread != null && unread.Count > 0)
                    {
                        modified = categorized = true;
                        items = unread;
                    }
                }

                if (modified && (tn.Selected || categorized))
                {
                    INewsItem itemSelected = null;
                    if (listFeedItems.SelectedItems.Count > 0)
                        itemSelected = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key;

                    PopulateListView(tn, items, false, categorized, ftnSelected);

                    if (itemSelected == null || (!categorized && !itemSelected.Feed.link.Equals(tn.DataKey)))
                    {
                        //clear state
                        CurrentSelectedFeedItem = null;
                        //reload newspaper
                        RefreshFeedDisplay(tn, false);
                    }
                    else
                    {
                        ReSelectListViewItem(itemSelected);
                    }
                }

                // apply finder matches to refresh node unread state(s)
                UpdateFindersReadStatus(items);
            }
            else
            {
                _log.Info("UpdateFeed() could not find node for '" + feedUri + "'...");
            }
        }

        private void UpdateFindersReadStatus(IEnumerable<INewsItem> items)
        {
            // apply finder matches to refresh the read state only
            if (_searchResultNode != null && !_searchResultNode.AnyUnread)
            {
                SearchCriteriaCollection sc = _searchResultNode.Finder.SearchCriterias;
                foreach (INewsItem item in items)
                {
                    if (!item.BeenRead && sc.Match(item))
                    {
                        // match unread only is enough here
                        _searchResultNode.UpdateReadStatus(_searchResultNode, true);
                        break;
                    }
                } //foreach
            }

            foreach (RssFinder finder in owner.FinderList)
            {
                if (finder.Container != null && !finder.Container.AnyUnread)
                {
                    SearchCriteriaCollection sc = finder.SearchCriterias;
                    foreach (INewsItem item in items)
                    {
                        if (!item.BeenRead && sc.Match(item))
                        {
                            // match unread only is enough here
                            finder.Container.UpdateReadStatus(finder.Container, true);
                            break;
                        }
                    } //foreach
                }
            } //foreach
        }

        private void ResetFindersReadStatus()
        {
            if (_searchResultNode != null)
            {
                UpdateTreeNodeUnreadStatus(_searchResultNode, 0);
            }
            foreach (RssFinder finder in owner.FinderList)
            {
                if (finder.Container != null)
                    UpdateTreeNodeUnreadStatus(finder.Container, 0);
            }
        }

        public void NewCategory()
        {
            if (CurrentSelectedFeedsNode != null && CurrentSelectedFeedsNode.AllowedChild(FeedNodeType.Category))
            {
                TreeFeedsNodeBase curFeedsNode = CurrentSelectedFeedsNode;

                int i = 1;
                string s = SR.GeneralNewItemText;
                // check for duplicate names:
                while (TreeHelper.FindChildNode(curFeedsNode, s, FeedNodeType.Category) != null)
                {
                    s = SR.GeneralNewItemTextWithCounter(i++);
                }

                TreeFeedsNodeBase newFeedsNode = new CategoryNode(s,
                                                                  Resource.SubscriptionTreeImage.SubscriptionsCategory,
                                                                  Resource.SubscriptionTreeImage.
                                                                      SubscriptionsCategoryExpanded,
                                                                  _treeCategoryContextMenu);

                curFeedsNode.Nodes.Add(newFeedsNode);
//				newNode.Cells[0].Appearance.Image = newNode.Override.NodeAppearance.Image;
                newFeedsNode.BringIntoView();
                TreeSelectedFeedsNode = newFeedsNode;
                s = newFeedsNode.CategoryStoreName;

                if (!owner.FeedHandler.HasCategory(s))
                {
                    owner.FeedHandler.AddCategory(s);
                    owner.SubscriptionModified(NewsFeedProperty.FeedCategoryAdded);
                    //owner.FeedlistModified = true;
                }

                if (!treeFeeds.Focused) treeFeeds.Focus();
                newFeedsNode.BeginEdit();
            }
        }

//		public void DeleteCategory() 
//		{
//			this.DeleteCategory(CurrentSelectedFeedsNode);
//			this.owner.SubscriptionModified(NewsFeedProperty.FeedCategoryRemoved);
//			//owner.FeedlistModified = true;
//		}

        internal void UpdateTreeNodeUnreadStatus(TreeFeedsNodeBase node, int newCount)
        {
            if (node != null)
            {
                node.UpdateReadStatus(node, newCount);
                if (node.Selected)
                {
                    SetDetailHeaderText(node);
                }
            }
        }

        /// <summary>
        /// Can be called on every selected tree node.
        /// </summary>
        public void MarkSelectedNodeRead(TreeFeedsNodeBase startNode)
        {
            TreeFeedsNodeBase selectedNode = startNode ?? CurrentSelectedFeedsNode;

            if (selectedNode == null) return;

            INewsFeed f = null;
            if (selectedNode.Type == FeedNodeType.Feed)
                f = owner.GetFeed(selectedNode.DataKey);

            if (f != null)
            {
                UnreadItemsNodeRemoveItems(f); // BEFORE they get marked read by:
                owner.FeedHandler.MarkAllCachedItemsAsRead(f);
                owner.FeedWasModified(f, NewsFeedProperty.FeedItemReadState);
                UpdateTreeNodeUnreadStatus(selectedNode, 0);
            }

            bool selectedIsChild = NodeIsChildOf(TreeSelectedFeedsNode, selectedNode);
            bool isSmartOrAggregated = (selectedNode.Type == FeedNodeType.Finder ||
                                        selectedNode.Type == FeedNodeType.SmartFolder);

            //mark all viewed stories as read 
            // May be we are wrong here: how about a threaded item reference
            // with an ownerfeed, that is not a child of the current selectedNode?
            if (listFeedItems.Items.Count > 0)
            {
                listFeedItems.BeginUpdate();

                for (int i = 0; i < listFeedItems.Items.Count; i++)
                {
                    ThreadedListViewItem lvi = listFeedItems.Items[i];
                    INewsItem newsItem = (INewsItem) lvi.Key;

                    if (newsItem != null &&
                        (newsItem.Feed == f || selectedIsChild || selectedNode == TreeSelectedFeedsNode ||
                         isSmartOrAggregated || lvi.IndentLevel > 0 || selectedNode.Type == FeedNodeType.Root))
                    {
                        // switch image back
                        if ((lvi.ImageIndex%2) != 0)
                            lvi.ImageIndex--;

                        // switch font back
                        ApplyStyles(lvi, true);

                        if (!newsItem.BeenRead)
                        {
                            newsItem.BeenRead = true;
                            UnreadItemsNode.Remove(newsItem);

                            // now update tree state of rss items from different
                            // feeds (or also: category selected)
                            if (lvi.IndentLevel > 0 || selectedNode.Type == FeedNodeType.Finder)
                            {
                                // corresponding node can be at any hierarchy level
                                selectedNode.UpdateReadStatus(
                                    TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), newsItem), -1);
                            }
                            else if (selectedNode.Type != FeedNodeType.Feed)
                            {
                                // can only be a child node, or SmartFolder
                                if (newsItem.Feed.containsNewMessages)
                                {
                                    //if not yet handled
                                    TreeFeedsNodeBase itemFeedsNode = TreeHelper.FindNode(selectedNode, newsItem);
                                    if (itemFeedsNode != null)
                                    {
                                        UpdateTreeNodeUnreadStatus(itemFeedsNode, 0);
                                        newsItem.Feed.containsNewMessages = false;
                                    }
                                }
                            }
                        } //if (!readed
                    } //item belongs to feed 
                    //else {
                    //Trace.WriteLine("Does not belong to node: "+NewsItem.Title);
                    //}
                } //for (i=0...

                listFeedItems.EndUpdate();
            }

            if (selectedNode.Type == FeedNodeType.Root)
            {
                // all
                owner.FeedHandler.MarkAllCachedItemsAsRead();
                UpdateTreeStatus(owner.FeedHandler.GetFeeds());
                ResetFindersReadStatus();
                UnreadItemsNode.Items.Clear();
                UnreadItemsNode.UpdateReadStatus();
                SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
            }
            else if (selectedNode.Type == FeedNodeType.Category)
            {
                // category and childs
                WalkdownAndCatchupCategory(selectedNode);
            }
            if (isSmartOrAggregated)
            {
                ISmartFolder sfNode = startNode as ISmartFolder;
                if (sfNode != null) sfNode.UpdateReadStatus();
            }
        }


        /// <summary>
        /// Toggle's the flag state of the identified RSS item
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemFlagState(string id)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                INewsItem item = (INewsItem) lvItem.Key;

                Flagged oldStatus = item.FlagStatus;

                if (oldStatus != Flagged.None)
                {
                    item.FlagStatus = Flagged.None;
                }
                else
                {
                    item.FlagStatus = Flagged.FollowUp;
                }

                if (item.FlagStatus != Flagged.None)
                {
                    lvItem.Font = FontColorHelper.MergeFontStyles(lvItem.Font, FontColorHelper.HighlightStyle);
                    lvItem.ForeColor = FontColorHelper.HighlightColor;
                }
                else
                {
                    lvItem.Font = FontColorHelper.MergeFontStyles(lvItem.Font, FontColorHelper.NormalStyle);
                    lvItem.ForeColor = FontColorHelper.NormalColor;
                }

                //ApplyFlagIconTo(lvItem, item);
                ApplyFlagStateTo(lvItem, item.FlagStatus, listFeedItems.Columns.GetColumnIndexMap());

                CheckForFlaggedNodeAndCreate(item);

                if ((CurrentSelectedFeedsNode as FlaggedItemsNode) != null)
                {
                    owner.ReFlagNewsItem(item);
                }
                else
                {
                    owner.FlagNewsItem(item);
                }

                if (FlaggedFeedsNode(item.FlagStatus) != null)
                {
                    // ReFlag may remove also items
                    FlaggedFeedsNode(item.FlagStatus).UpdateReadStatus();
                }
                if (listFeedItemsO.Visible)
                    listFeedItemsO.Invalidate();
            }
        }


        /// <summary>
        /// Toggles watching a particular item or list of items for new comments by either watching the value of 
        /// slash:comments and thr:replies or subscribing to the comments feed. 
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdWatchItemComments(ICommand sender)
        {
            IList selectedItems = GetSelectedLVItems();

            for (int i = 0; i < selectedItems.Count; i++)
            {
                ThreadedListViewItem selectedItem = (ThreadedListViewItem) selectedItems[i];

                INewsItem item = (INewsItem) selectedItem.Key;
                item.WatchComments = !item.WatchComments;
                owner.WatchNewsItem(item);
            }
        }

        /// <summary>
        /// Marks the selected feed items flagged. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkFeedItemsFlagged(Flagged flag)
        {
            INewsItem item;
            ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();
            IList selectedItems = GetSelectedLVItems();
            ArrayList toBeRemoved = null;

            for (int i = 0; i < selectedItems.Count; i++)
            {
                ThreadedListViewItem selectedItem = (ThreadedListViewItem) selectedItems[i];

                item = (INewsItem) selectedItem.Key;

                if (item.FlagStatus == flag)
                    continue; // no change

                item.FlagStatus = flag;

                //font styles merged, color overrides
                if (item.FlagStatus != Flagged.None)
                {
                    selectedItem.Font =
                        FontColorHelper.MergeFontStyles(selectedItem.Font, FontColorHelper.HighlightStyle);
                    selectedItem.ForeColor = FontColorHelper.HighlightColor;
                }
                else
                {
                    selectedItem.Font = FontColorHelper.MergeFontStyles(selectedItem.Font, FontColorHelper.NormalStyle);
                    selectedItem.ForeColor = FontColorHelper.NormalColor;
                }

                //ApplyFlagIconTo(selectedItem, item);
                ApplyFlagStateTo(selectedItem, item.FlagStatus, colIndex);

                CheckForFlaggedNodeAndCreate(item);

                if ((CurrentSelectedFeedsNode as FlaggedItemsNode) != null)
                {
                    owner.ReFlagNewsItem(item);
                    if (item.FlagStatus == Flagged.None || item.FlagStatus == Flagged.Complete)
                    {
                        if (toBeRemoved == null)
                            toBeRemoved = new ArrayList();
                        toBeRemoved.Add(selectedItem);
                    }
                }
                else
                {
                    owner.FlagNewsItem(item);
                }
            } //for(i=0...

            if (toBeRemoved != null && toBeRemoved.Count > 0)
                RemoveListviewItems(toBeRemoved, false, false, false);

            if (FlaggedFeedsNode(flag) != null)
            {
                // ReFlag may remove also items
                FlaggedFeedsNode(flag).UpdateReadStatus();
            }
        }

        /// <summary>
        /// Removes the provided listview items (collection of ThreadedListViewItem objects). 
        /// Also considers childs.
        /// </summary>
        /// <param name="itemsToRemove">List of items to be removed</param>
        /// <param name="moveItemsToTrash">If true, the corresponding NewsItem(s) will be moved to the Trash SmartFolder</param>
        /// <param name="removeFromSmartFolder">If true, the  corresponding NewsItem(s) will be also removed from any SmartFolder</param>
        /// <param name="updateUnreadCounters">If true, the unread counter(s) will be updated</param>
        public void RemoveListviewItems(ICollection itemsToRemove,
                                        bool moveItemsToTrash, bool removeFromSmartFolder, bool updateUnreadCounters)
        {
            if (itemsToRemove == null || itemsToRemove.Count == 0)
                return;

            ThreadedListViewItem[] items = new ThreadedListViewItem[itemsToRemove.Count];
            itemsToRemove.CopyTo(items, 0);

            // where we are?
            TreeFeedsNodeBase thisNode = TreeSelectedFeedsNode;
            ISmartFolder isFolder = thisNode as ISmartFolder;

            int unreadItemsCount = 0;
            int itemIndex = 0;
            bool anyUnreadItem = false;

            try
            {
                listFeedItems.BeginUpdate();

                int delCounter = itemsToRemove.Count;
                while (--delCounter >= 0)
                {
                    ThreadedListViewItem currentItem = items[delCounter];

                    if (currentItem == null || currentItem.IndentLevel > 0)
                        continue; // do not delete selected childs

                    if (currentItem.HasChilds && currentItem.Expanded)
                    {
                        // also remove the childs
                        int j = currentItem.Index + 1;
                        if (j < listFeedItems.Items.Count)
                        {
                            lock (listFeedItems.Items)
                            {
                                ThreadedListViewItem child = listFeedItems.Items[j];
                                while (child != null && child.IndentLevel > currentItem.IndentLevel)
                                {
                                    listFeedItems.Items.Remove(child);
                                    if (listFeedItemsO.Visible)
                                        listFeedItemsO.Remove(child);
                                    if (j < listFeedItems.Items.Count)
                                        child = listFeedItems.Items[j];
                                    else
                                        child = null;
                                }
                            }
                        }
                    }

                    // remember for reselection of the preceeding item.
                    // we just take that of the last iterated item:
                    itemIndex = currentItem.Index;

                    INewsItem item = (INewsItem) currentItem.Key;

                    if (item == null)
                        continue;

                    if (moveItemsToTrash)
                        owner.DeleteNewsItem(item);

                    if (!item.BeenRead)
                        UnreadItemsNode.Remove(item);

                    if (item.WatchComments)
                        WatchedItemsNode.Remove(item);

                    if (item.HasNewComments)
                    {
                        TreeFeedsNodeBase n = TreeHelper.FindNode(thisNode, item);
                        n.UpdateCommentStatus(n, -1);
                    }

                    if (thisNode.Type == FeedNodeType.Category)
                    {
                        if (updateUnreadCounters && !item.BeenRead)
                        {
                            // update unread counter(s)
                            anyUnreadItem = true;
                            TreeFeedsNodeBase n = TreeHelper.FindNode(thisNode, item);
                            UpdateTreeNodeUnreadStatus(n, -1);
                        }
                    }
                    else if (isFolder == null)
                    {
                        isFolder = TreeHelper.FindNode(GetRoot(RootFolderType.SmartFolders), item) as ISmartFolder;
                    }

                    if (updateUnreadCounters && !item.BeenRead)
                    {
                        anyUnreadItem = true;
                        unreadItemsCount++;
                    }

                    if (removeFromSmartFolder && isFolder != null)
                        owner.RemoveItemFromSmartFolder(isFolder, item);

                    lock (listFeedItems.Items)
                    {
                        listFeedItems.Items.Remove(currentItem);
                        if (listFeedItemsO.Visible)
                            listFeedItemsO.Remove(currentItem);
                    }
                } //while
            }
            finally
            {
                listFeedItems.EndUpdate();
            }

            if (updateUnreadCounters && unreadItemsCount > 0)
            {
                UpdateTreeNodeUnreadStatus(thisNode, -unreadItemsCount);
            }

            if (moveItemsToTrash && anyUnreadItem)
                DeletedItemsNode.UpdateReadStatus();

            // try to select another item:
            if (listFeedItems.Items.Count > 0 && listFeedItems.SelectedItems.Count == 0)
            {
                /*	itemIndex--;

					if (itemIndex < 0) {
						itemIndex = 0;
					} else */
                if ((itemIndex != 0) && (itemIndex >= listFeedItems.Items.Count))
                {
                    itemIndex = listFeedItems.Items.Count - 1;
                }

                listFeedItems.Items[itemIndex].Selected = true;
                listFeedItems.Items[itemIndex].Focused = true;

                OnFeedListItemActivate(this, EventArgs.Empty);
            }
            else if (listFeedItems.SelectedItems.Count > 0)
            {
                // still selected not deleted items:

                OnFeedListItemActivate(this, EventArgs.Empty);
            }
            else
            {
                // no items:
                htmlDetail.Clear();
            }
        }

        /// <summary>
        /// Remove the selected feed items. 
        /// Called from the listview context menu.
        /// </summary>
        public void RemoveSelectedFeedItems()
        {
            IList selectedItems = GetSelectedLVItems();
            if (selectedItems.Count == 0)
                return;

            RemoveListviewItems(selectedItems, true, true, true);
        }

        /// <summary>
        /// Restore the selected feed items from the Wastebasket. 
        /// Called from the listview context menu.
        /// </summary>
        public void RestoreSelectedFeedItems()
        {
            IList selectedItems = GetSelectedLVItems();
            if (selectedItems.Count == 0)
                return;

            TreeFeedsNodeBase thisNode = TreeSelectedFeedsNode;
            ISmartFolder isFolder = thisNode as ISmartFolder;

            if (!(isFolder is WasteBasketNode))
                return;

            int itemIndex = 0;
            bool anyUnreadItem = false;

            try
            {
                listFeedItems.BeginUpdate();

                while (selectedItems.Count > 0)
                {
                    ThreadedListViewItem selectedItem = (ThreadedListViewItem) selectedItems[0];

                    if (selectedItem.IndentLevel > 0)
                        continue; // do not delete selected childs

                    if (selectedItem.HasChilds && selectedItem.Expanded)
                    {
                        // also remove the childs
                        int j = selectedItem.Index + 1;
                        if (j < listFeedItems.Items.Count)
                        {
                            lock (listFeedItems.Items)
                            {
                                ThreadedListViewItem child = listFeedItems.Items[j];
                                while (child != null && child.IndentLevel > selectedItem.IndentLevel)
                                {
                                    listFeedItems.Items.Remove(child);
                                    if (listFeedItemsO.Visible)
                                        listFeedItemsO.Remove(child);
                                    if (j < listFeedItems.Items.Count)
                                        child = listFeedItems.Items[j];
                                    else
                                        child = null;
                                }
                            }
                        }
                    }

                    // remember for reselection of the preceeding item.
                    // we just take that of the last iterated item:
                    itemIndex = selectedItem.Index;

                    INewsItem item = (INewsItem) selectedItem.Key;

                    if (item == null)
                    {
                        selectedItems.Remove(selectedItem);
                        continue;
                    }

                    TreeFeedsNodeBase originalContainerNode = owner.RestoreNewsItem(item);

                    if (null != originalContainerNode && ! item.BeenRead)
                    {
                        anyUnreadItem = true;
                        UpdateTreeNodeUnreadStatus(originalContainerNode, 1);
                        UnreadItemsNode.Add(item);
                    }

                    if (null == originalContainerNode)
                    {
                        // 
                        _log.Error("Item could not be restored, maybe the container feed was removed meanwhile: " +
                                   item.Title);
                    }

                    lock (listFeedItems.Items)
                    {
                        listFeedItems.Items.Remove(selectedItem);
                        if (listFeedItemsO.Visible)
                            listFeedItemsO.Remove(selectedItem);

                        selectedItems.Remove(selectedItem);
                    }
                } //while
            }
            finally
            {
                listFeedItems.EndUpdate();
            }

            if (anyUnreadItem)
                DeletedItemsNode.UpdateReadStatus();

            // try to select another item:
            if (listFeedItems.Items.Count > 0)
            {
                itemIndex--;

                if (itemIndex < 0)
                {
                    itemIndex = 0;
                }
                else if (itemIndex >= listFeedItems.Items.Count)
                {
                    itemIndex = listFeedItems.Items.Count - 1;
                }

                listFeedItems.Items[itemIndex].Selected = true;
                listFeedItems.Items[itemIndex].Focused = true;

                OnFeedListItemActivate(this, EventArgs.Empty);
            }
            else
            {
                // no items:
                htmlDetail.Clear();
            }
        }


        /// <summary>
        /// Helper function which gets the list of selected list view items from the 
        /// currently visible list view. 
        /// </summary>
        /// <returns>The list of selected ThreadedListViewItems</returns>
        private IList GetSelectedLVItems()
        {
            if (listFeedItems.Visible)
            {
                return listFeedItems.SelectedItems;
            }
            else
            {
                return listFeedItemsO.SelectedItems;
            }
        }


        /// <summary>
        /// Marks the selected listview items read. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkSelectedItemsLVRead()
        {         
            SetFeedItemsReadState(GetSelectedLVItems(), true);
        }

        /// <summary>
        /// Marks the selected listview items unread. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkSelectedItemsLVUnread()
        {
            SetFeedItemsReadState(GetSelectedLVItems(), false);
        }

        /// <summary>
        /// Marks the all listview items read. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkAllItemsLVRead()
        {
            SetFeedItemsReadState(listFeedItems.Items, true);
        }

        /// <summary>
        /// Marks the all listview items unread. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkAllItemsLVUnread()
        {
            SetFeedItemsReadState(listFeedItems.Items, false);
        }

        private void ApplyStyles(ThreadedListViewItem item)
        {
            if (item != null)
            {
                INewsItem n = (INewsItem) item.Key;
                if (n != null)
                    ApplyStyles(item, n.BeenRead, n.HasNewComments);
            }
        }

        private void ApplyStyles(ThreadedListViewItem item, bool beenRead)
        {
            if (item != null)
            {
                INewsItem n = (INewsItem) item.Key;
                if (n != null)
                    ApplyStyles(item, beenRead, n.HasNewComments);
            }
        }

        private void ApplyStyles(ThreadedListViewItem item, bool beenRead, bool newComments)
        {
            if (item != null)
            {
                if (beenRead)
                {
                    item.Font = FontColorHelper.NormalFont;
                    item.ForeColor = FontColorHelper.NormalColor;
                }
                else
                {
                    item.Font = FontColorHelper.UnreadFont;
                    item.ForeColor = FontColorHelper.UnreadColor;
                }

                if (newComments)
                {
                    item.Font = FontColorHelper.MergeFontStyles(item.Font, FontColorHelper.NewCommentsStyle);
                    item.ForeColor = FontColorHelper.NewCommentsColor;
                }

                _filterManager.Apply(item);
                if (listFeedItemsO.Visible)
                    listFeedItemsO.Invalidate();
            }
        }


        /// <summary>
        /// Marks the all feed items related to a particular URL as read. 
        /// </summary>
        /// <param name="storyId">The URL of the story</param>
        public void MarkDiscussionAsRead(string storyId)
        {
            //BUGBUG: make this the same as the value used by top stories; 
            DateTime since = DateTime.Now - new TimeSpan(7, 0, 0, 0, 0);

            IList<INewsItem> affectedItems = owner.FeedHandler.GetItemsWithIncomingLinks(storyId, since);
            List<ThreadedListViewItem> affectedItemsInListView = new List<ThreadedListViewItem>();

            for (int i = 0; i < affectedItems.Count; i++)
            {
                ThreadedListViewItem lvItem = GetListViewItem(affectedItems[i].Id);
                if (lvItem != null)
                {
                    affectedItemsInListView.Add(lvItem);
                    affectedItems.RemoveAt(i);
                }
            }

            SetFeedItemsReadState(affectedItemsInListView, true);
            SetNewsItemsReadState(affectedItems, true);
        }

        /// <summary>
        /// Moves the newspaper view to the next or previous page. 
        /// </summary>
        /// <param name="pageType">Indicates whether the page is a category or feed node</param>
        /// <param name="go2nextPage">Indicates whether we are going to the next or previous page. If true
        /// we are going to the next page, otherwise we are going to the previous page</param>
        public void SwitchPage(string pageType, bool go2nextPage)
        {
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;
            if (tn == null)
                return;

            if (go2nextPage)
            {
                _currentPageNumber++;
            }
            else
            {
                _currentPageNumber--;
            }

            if (pageType.Equals("feed"))
            {
                FeedInfo fi = GetFeedItemsAtPage(_currentPageNumber);
                if (fi != null)
                {
                    BeginTransformFeed(fi, tn, owner.FeedHandler.GetStyleSheet(tn.DataKey));
                }
            }
            else
            {
                //BUGBUG: How do we provide title of FeedInfoList? 
                FeedInfoList fil = GetCategoryItemsAtPage(_currentPageNumber);
                if (fil != null)
                {
                    BeginTransformFeedList(fil, tn, owner.FeedHandler.GetCategoryStyleSheet(tn.CategoryStoreName));
                }
            }
        }


        /// <summary>
        /// Toggles the identified item's read/unread state. 
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        /// <param name="markRead">Indicates that the item should be marked as read NOT toggled</param>
        public void ToggleItemReadState(string id, bool markRead)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                bool oldReadState = ((INewsItem) lvItem.Key).BeenRead;
                if (!markRead || (markRead != oldReadState))
                {
                    SetFeedItemsReadState(new ThreadedListViewItem[] {lvItem}, !oldReadState);
                }
            }
        }

        /// <summary>
        /// Toggles the identified item's read/unread state. 
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemReadState(string id)
        {
            ToggleItemReadState(id, false);
        }


        /// <summary>
        /// Toggles the identified item's watchd state. 
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemWatchState(string id)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                INewsItem item = (INewsItem) lvItem.Key;
                item.WatchComments = !item.WatchComments;
                owner.WatchNewsItem(item);
            }
        }

        private class RefLookupItem
        {
            public readonly TreeFeedsNodeBase Node;
            public readonly INewsFeed Feed;
            public int UnreadCount;

            public RefLookupItem(TreeFeedsNodeBase feedsNode, INewsFeed feed, int unreadCount)
            {
                Node = feedsNode;
                Feed = feed;
                UnreadCount = unreadCount;
            }
        }

        /// <summary>
        /// Marks the selected feed items read/unread. Called from the listview
        /// context menu.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="beenRead">if set to <c>true</c> [been read].</param>
        public void SetFeedItemsReadState(IList items, bool beenRead)
        {
            List<INewsItem> modifiedItems = new List<INewsItem>(listFeedItems.SelectedItems.Count);
            int amount = (beenRead ? -1 : 1);

            for (int i = 0; i < items.Count; i++)
            {
                ThreadedListViewItem selectedItem = (ThreadedListViewItem) items[i];
                INewsItem item = (INewsItem) selectedItem.Key;
                ApplyStyles(selectedItem, beenRead);

                if (item.BeenRead != beenRead)
                {
                    selectedItem.ImageIndex += amount;
                    modifiedItems.Add(item);
                }
            } //for(int i=0; i < items.Count; i++)

            SetNewsItemsReadState(modifiedItems, beenRead);
        }


        /// <summary>
        /// Marks the specified feed items read/unread. 
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="beenRead">if set to <c>true</c> [been read].</param>
        public void SetNewsItemsReadState(IList<INewsItem> items, bool beenRead)
        {
            List<INewsItem> modifiedItems = new List<INewsItem>(listFeedItems.SelectedItems.Count);
            int amount = (beenRead ? -1 : 1);

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];

                if (item.BeenRead != beenRead)
                {
                    item.BeenRead = beenRead;
                    modifiedItems.Add(item);

                    if (beenRead)
                    {
                        if (!item.Feed.storiesrecentlyviewed.Contains(item.Id))
                        {
                            item.Feed.AddViewedStory(item.Id);
                        }
                    }
                    else
                    {
                        item.Feed.RemoveViewedStory(item.Id);
                    }

                    /* locate actual item if this is a search result */
                    SearchHitNewsItem sItem = item as SearchHitNewsItem;
                    INewsItem realItem = owner.FeedHandler.FindNewsItem(sItem);

                    if (realItem != null && sItem != null)
                    {
                        realItem.BeenRead = sItem.BeenRead;
                    }
                } //if (item.BeenRead != beenRead)
            } //for(int i=0; i < items.Count; i++) 

            if (modifiedItems.Count > 0)
            {
                List<INewsItem> deepModifiedItems = new List<INewsItem>();
                int unexpectedImageState = (beenRead ? 1 : 0);
                    // unread-state images always have odd index numbers, read-state are even

                // if there is a self-reference thread, we also have to switch the Gui state for them back
                // these items can also be unselected.
                for (int i = 0; i < listFeedItems.Items.Count; i++)
                {
                    ThreadedListViewItem th = listFeedItems.Items[i];
                    INewsItem selfRef = th.Key as INewsItem;

                    foreach (INewsItem modifiedItem in modifiedItems)
                    {
                        if (modifiedItem.Equals(selfRef) && (th.ImageIndex%2) == unexpectedImageState)
                        {
                            ApplyStyles(th, beenRead);
                            th.ImageIndex += amount;

                            if (selfRef.BeenRead != beenRead)
                            {
                                // object ref is unequal, but other criteria match the item to be equal...
                                selfRef.BeenRead = beenRead;
                                deepModifiedItems.Add(selfRef);
                            }
                        }
                    }
                }

                modifiedItems.AddRange(deepModifiedItems);
                // we store stories-read in the feedlist, so enable save the new state 
                owner.SubscriptionModified(NewsFeedProperty.FeedItemReadState);
                //owner.FeedlistModified = true;	
                // and apply mods. to finders:
                UpdateFindersReadStatus(modifiedItems);

                //TODO: verify correct location  of that code here:
                if (beenRead)
                    UnreadItemsNodeRemoveItems(modifiedItems);
                else
                {
                    UnreadItemsNode.Items.AddRange(modifiedItems);
                    UnreadItemsNode.UpdateReadStatus();
                }
            }

            ISmartFolder sf = CurrentSelectedFeedsNode as ISmartFolder;
            if (sf != null)
            {
                sf.UpdateReadStatus();

                if (!(sf is FinderNode) && !(sf is UnreadItemsNode))
                    return;
            }

            // now update tree state of rss items from any
            // feed (also: category selected)

            Hashtable lookup = new Hashtable(modifiedItems.Count);

            foreach (INewsItem item in modifiedItems)
            {
                string feedurl = item.Feed.link;

                if (feedurl != null)
                {
                    RefLookupItem lookupItem = lookup[feedurl] as RefLookupItem;
                    TreeFeedsNodeBase refNode = lookupItem != null ? lookupItem.Node : null;
                    if (refNode == null)
                    {
                        // corresponding node can be at any hierarchy level, or temporary (if commentRss)
                        if (owner.FeedHandler.IsSubscribed(feedurl))
                            refNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), item);
                        else
                            refNode = TreeHelper.FindNode(GetRoot(RootFolderType.SmartFolders), item);
                    }

                    if (refNode != null)
                    {
                        if (!lookup.ContainsKey(feedurl))
                        {
                            lookup.Add(feedurl, new RefLookupItem(refNode, item.Feed, amount)); // speedup node lookup
                        }
                        else
                        {
                            lookupItem.UnreadCount += amount;
                        }
                        //refNode.UpdateReadStatus(refNode, refNode.UnreadCount + amount);
                        //item.Feed.containsNewMessages = (refNode.UnreadCount > 0);
                    }
                    else
                    {
                        // temp. (item comments)
                        string hash = RssHelper.GetHashCode(item);
                        if (tempFeedItemsRead.ContainsKey(hash))
                            tempFeedItemsRead.Remove(hash);
                    }
                }
            }

            foreach (RefLookupItem item in lookup.Values)
            {
                UpdateTreeNodeUnreadStatus(item.Node, item.Node.UnreadCount + item.UnreadCount);
                item.Feed.containsNewMessages = (item.Node.UnreadCount > 0);
            }
            if (listFeedItemsO.Visible)
                listFeedItemsO.Invalidate();
        }


        /// <summary>
        /// Moves a node to a new parent. 
        /// </summary>
        /// <param name="theNode">FeedTreeNodeBase to move.</param>
        /// <param name="target">New Parent FeedTreeNodeBase.</param>
        /// <param name="userInitiated">Indicates whether the node was moved due to a user initiated action</param>
        public void MoveNode(TreeFeedsNodeBase theNode, TreeFeedsNodeBase target, bool userInitiated)
        {
            if (theNode == null || target == null)
                return;

            if (theNode == target)
                return;

            NewsFeedProperty changes = NewsFeedProperty.None;

            if (theNode.Type == FeedNodeType.Feed)
            {
                INewsFeed f = owner.GetFeed(theNode.DataKey);
                if (f == null)
                    return;

                if (userInitiated)
                {
                    string category = target.CategoryStoreName;
                   //owner.FeedlistModified = true;
                    if (category != null && !owner.FeedHandler.HasCategory(category))
                    {
                        owner.FeedHandler.AddCategory(category);
                        changes |= NewsFeedProperty.FeedCategoryAdded;
                    }

                    owner.FeedHandler.ChangeCategory(f, category);
                    changes |= NewsFeedProperty.FeedCategory;                    
                }

                treeFeeds.BeginUpdate();

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, -theNode.UnreadCount);

                theNode.Parent.Nodes.Remove(theNode);
                target.Nodes.Add(theNode);
                theNode.Control.ActiveNode = theNode;

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, theNode.UnreadCount);

                theNode.BringIntoView();
                treeFeeds.EndUpdate();

                owner.FeedWasModified(f, changes);
            }
            else if (theNode.Type == FeedNodeType.Category)
            {
                string targetCategory = target.CategoryStoreName;
                string sourceCategory = theNode.CategoryStoreName;

                if (userInitiated)
                {
                    // refresh category store
                    if (sourceCategory != null && owner.FeedHandler.HasCategory(sourceCategory))
                    {
                        owner.FeedHandler.DeleteCategory(sourceCategory);
                        changes |= NewsFeedProperty.FeedCategoryRemoved;
                    }
                    // target is the root node:
                    if (targetCategory == null && !owner.FeedHandler.HasCategory(theNode.Text))
                    {
                        owner.FeedHandler.AddCategory(theNode.Text);
                        changes |= NewsFeedProperty.FeedCategoryAdded;
                    }
                    // target is another category node:
                    if (targetCategory != null && !owner.FeedHandler.HasCategory(targetCategory))
                    {
                        owner.FeedHandler.AddCategory(targetCategory);
                        changes |= NewsFeedProperty.FeedCategoryAdded;
                    }
                }

                treeFeeds.BeginUpdate();

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, -theNode.UnreadCount);

                theNode.Parent.Nodes.Remove(theNode);
                target.Nodes.Add(theNode);

                // reset category references on feeds - after moving node to 
                // have the correct FullPath info within this call:
                WalkdownThenRenameFeedCategory(theNode, targetCategory);
                owner.SubscriptionModified(changes);
                //owner.FeedlistModified = true;

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, theNode.UnreadCount);

                theNode.BringIntoView();
                treeFeeds.EndUpdate();
            }
            else
            {
                Debug.Assert(false, "MoveNode(): unhandled NodeType:'" + theNode.Type);
            }
        }


        /// <summary>
        /// Adds an autodiscovered URL to the auto discovered feeds drop down
        /// </summary>
        /// <param name="info"></param>
        public void AddAutoDiscoveredUrl(DiscoveredFeedsInfo info)
        {
            AppButtonToolCommand duplicateItem =
                new AppButtonToolCommand(
                    String.Concat("cmdDiscoveredFeed_", ++(AutoDiscoveredFeedsMenuHandler.cmdKeyPostfix)),
                    owner.BackgroundDiscoverFeedsHandler.mediator,
                    owner.BackgroundDiscoverFeedsHandler.OnDiscoveredItemClick,
                    owner.BackgroundDiscoverFeedsHandler.StripAndShorten(info.Title), (string) info.FeedLinks[0]);

            if (owner.BackgroundDiscoverFeedsHandler.itemDropdown.ToolbarsManager.Tools.Exists(duplicateItem.Key))
                owner.BackgroundDiscoverFeedsHandler.itemDropdown.ToolbarsManager.Tools.Remove(duplicateItem);

            owner.BackgroundDiscoverFeedsHandler.itemDropdown.ToolbarsManager.Tools.Add(duplicateItem);
            duplicateItem.SharedProps.StatusText = info.SiteBaseUrl;
            duplicateItem.SharedProps.ShowInCustomizer = false;

            Win32.PlaySound(Resource.ApplicationSound.FeedDiscovered);

            lock (owner.BackgroundDiscoverFeedsHandler.discoveredFeeds)
            {
                // add a fresh version of info
                owner.BackgroundDiscoverFeedsHandler.discoveredFeeds.Add(duplicateItem, info);
            }

            lock (owner.BackgroundDiscoverFeedsHandler.newDiscoveredFeeds)
            {
// re-order to top of list, in RefreshItemContainer()
                owner.BackgroundDiscoverFeedsHandler.newDiscoveredFeeds.Enqueue(duplicateItem);
            }
        }

        /// <summary>
        /// Calls/Open the newFeedDialog on the GUI thread, if required.
        /// </summary>
        /// <param name="newFeedUrl">Feed Url to add</param>
        public void AddFeedUrlSynchronized(string newFeedUrl)
        {
            InvokeOnGui(delegate
            {
                newFeedUrl = owner.HandleUrlFeedProtocol(newFeedUrl);
                owner.CmdNewFeed(null, newFeedUrl, null);
            });
        }

        public void OnFeedUpdateStart(Uri feedUri, ref bool cancel)
        {
            string feedUrl;
            TreeFeedsNodeBase feedsNode;

            if (feedUri.IsFile || feedUri.IsUnc)
                feedUrl = feedUri.LocalPath;
            else
                feedUrl = feedUri.CanonicalizedUri();

            INewsFeed f;
            if (owner.FeedHandler.GetFeeds().TryGetValue(feedUrl, out f))
            {
                feedsNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), f);
            }
            else
            {
                feedsNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
            }
            if (feedsNode != null)
            {
                SetSubscriptionNodeState(f, feedsNode, FeedProcessingState.Updating);
            }
        }

        public void OnFeedUpdateFinishedWithException(string feedUrl, Exception exception)
        {
            //string feedUrl = null;
            TreeFeedsNodeBase feedsNode;

            /* if (feedUri.IsFile || feedUri.IsUnc) 
				feedUrl = feedUri.LocalPath;
			else
				feedUrl = feedUri.AbsoluteUri; */

            INewsFeed f = null;
            if (owner.FeedHandler.GetFeeds().TryGetValue(feedUrl, out f))
            {
                feedsNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), f);
            }
            else
            {
                feedsNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
            }

            if (feedsNode != null)
            {
                SetSubscriptionNodeState(f, feedsNode, FeedProcessingState.Failure);
            }
        }

        public void OnRequestCertificateIssue(object sender, CertificateIssueCancelEventArgs e)
        {
            e.Cancel = true; // by default: do not continue on certificate problems

            if (!Visible) // do not bother if hidden. Just go on an report the issue as a feed error
                return;

			string feedUrl = e.WebRequest.RequestUri.CanonicalizedUri();

            INewsFeed f = null;
            string issueCaption, issueDesc;

            if (owner.FeedHandler.GetFeeds().TryGetValue(feedUrl, out f))
            {
                issueCaption = SR.CertificateIssueOnFeedCaption(f.title);
            }
            else
            {
                issueCaption = SR.CertificateIssueOnSiteCaption(feedUrl);
            }

            switch (e.CertificateIssue)
            {
                case CertificateIssue.CertCN_NO_MATCH:
                    issueDesc = SR.CertificateIssue_CertCN_NO_MATCH;
                    break;
                case CertificateIssue.CertEXPIRED:
                    issueDesc = SR.CertificateIssue_CertEXPIRED(e.Certificate.GetExpirationDateString());
                    break;
                case CertificateIssue.CertREVOKED:
                    issueDesc = SR.CertificateIssue_CertREVOKED;
                    break;
                case CertificateIssue.CertUNTRUSTEDCA:
                    issueDesc = SR.CertificateIssue_CertUNTRUSTEDCA;
                    break;
                case CertificateIssue.CertUNTRUSTEDROOT:
                    issueDesc = SR.CertificateIssue_CertUNTRUSTEDROOT;
                    break;
                case CertificateIssue.CertUNTRUSTEDTESTROOT:
                    issueDesc = SR.CertificateIssue_CertUNTRUSTEDTESTROOT;
                    break;
                case CertificateIssue.CertPURPOSE:
                    issueDesc = SR.CertificateIssue_CertPURPOSE;
                    break;
                case CertificateIssue.CertCHAINING:
                    issueDesc = SR.CertificateIssue_CertCHAINING;
                    break;
                case CertificateIssue.CertCRITICAL:
                    issueDesc = SR.CertificateIssue_CertCRITICAL;
                    break;
                case CertificateIssue.CertISSUERCHAINING:
                    issueDesc = SR.CertificateIssue_CertISSUERCHAINING;
                    break;
                case CertificateIssue.CertMALFORMED:
                    issueDesc = SR.CertificateIssue_CertMALFORMED;
                    break;
                case CertificateIssue.CertPATHLENCONST:
                    issueDesc = SR.CertificateIssue_CertPATHLENCONST;
                    break;
                case CertificateIssue.CertREVOCATION_FAILURE:
                    issueDesc = SR.CertificateIssue_CertREVOCATION_FAILURE;
                    break;
                case CertificateIssue.CertROLE:
                    issueDesc = SR.CertificateIssue_CertROLE;
                    break;
                case CertificateIssue.CertVALIDITYPERIODNESTING:
                    issueDesc = SR.CertificateIssue_CertVALIDITYPERIODNESTING;
                    break;
                case CertificateIssue.CertWRONG_USAGE:
                    issueDesc = SR.CertificateIssue_CertWRONG_USAGE;
                    break;
                default:
                    issueDesc = SR.CertificateIssue_Unknown(e.CertificateIssue.ToString());
                    break;
            }

            // show cert. issue dialog
			using (SecurityIssueDialog dialog = new SecurityIssueDialog(issueCaption, issueDesc)) 
			{
				// prepare special command (show certificate):
				dialog.CustomCommand.Tag = e.Certificate;
				dialog.CustomCommand.Click += OnSecurityIssueDialogCustomCommandClick;
				dialog.CustomCommand.Visible = (e.Certificate != null && e.Certificate.Handle != IntPtr.Zero);

				Win32.SetForegroundWindow(Handle); // ensure, it is in front
				if (dialog.ShowDialog(this) == DialogResult.OK) {
					e.Cancel = false;
					owner.AddTrustedCertificateIssue(feedUrl, e.CertificateIssue);
				}
			}
        }

        private static void OnSecurityIssueDialogCustomCommandClick(object sender, EventArgs e)
        {
            Button cmd = (Button) sender;
            cmd.Enabled = false;

            Application.DoEvents();

            X509Certificate cert = (X509Certificate) cmd.Tag;
			if (cert == null)
				return; 
			
			string certFilename = Path.Combine(Path.GetTempPath(), cert.GetHashCode() + ".temp.cer");

            try
            {
                if (File.Exists(certFilename))
                    File.Delete(certFilename);

                using (Stream stream = FileHelper.OpenForWrite(certFilename))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(cert.GetRawCertData());
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
                cmd.Enabled = false;
                return;
            }

            try
            {
                if (File.Exists(certFilename))
                {
                    Process p = Process.Start(certFilename);
                    p.WaitForExit(); // to enble delete the temp file
                }
            }
            finally
            {
                if (File.Exists(certFilename))
                    File.Delete(certFilename);
                cmd.Enabled = true;
            }
        }

        #endregion

        /// <summary>
        /// Creates and loads an instance of the SettingsHandler with 
        /// the user's keyboard shortcuts.
        /// </summary>
        protected void InitShortcutManager()
        {
            _shortcutHandler = new ShortcutHandler();
            string settingsPath = RssBanditApplication.GetShortcutSettingsFileName();
            try
            {
                     _shortcutHandler.Load(settingsPath);                
            }
            catch (InvalidShortcutSettingsFileException e)
            {
                _log.Warn("The user defined shortcut settings file is invalid. Using the default instead.", e);
                using (Stream settingsStream = Resource.GetStream("Resources.ShortcutSettings.xml"))
                {
                    _shortcutHandler.Load(settingsStream);
                }
            }
        }

        #region Resource handling

        protected void InitResources()
        {
            // For now just use XP icons until we can update the overlays on the vista one
            //// Create a strip of images by loading an embedded bitmap resource
            //if(Win32.IsOSAtLeastWindowsVista)
            //    _treeImages = Resource.LoadBitmapStrip("Resources.TreeImages.png", new Size(16, 16));
            //else
                _treeImages = Resource.LoadBitmapStrip("Resources.TreeImagesXP.png", new Size(16, 16));

            _listImages = Resource.LoadBitmapStrip("Resources.ListImages.png", new Size(16, 16));
            _allToolImages = Resource.LoadBitmapStrip("Resources.AllToolImages.png", new Size(16, 16));
        }

        #endregion

        #region Widget init routines

        private void InitWidgets()
        {
            InitFeedTreeView();
            InitFeedDetailsCaption();
            InitListView();
            InitOutlookListView();
            InitHtmlDetail();
            InitToaster();
            InitializeSearchPanel();
            InitOutlookNavigator();
            InitNavigatorHiddenCaption();
        }

        private void InitOutlookNavigator()
        {
            navigatorHeaderHelper = new NavigatorHeaderHelper(Navigator, Properties.Resources.Arrows_Left_16);
            navigatorHeaderHelper.ImageClick += OnNavigatorCollapseClick;
            Navigator.GroupClick += OnNavigatorGroupClick;
            if (SearchIndexBehavior.NoIndexing == owner.FeedHandler.Configuration.SearchIndexBehavior)
            {
                ToggleNavigationPaneView(NavigationPaneView.Subscriptions);
                owner.Mediator.SetDisabled("cmdToggleRssSearchTabState");
                Navigator.Groups[Resource.NavigatorGroup.RssSearch].Enabled = false;
            }
        }


        private void InitOutlookListView()
        {
            listFeedItemsO.ViewStyle = ViewStyle.OutlookExpress;
            UltraTreeNodeExtendedDateTimeComparer sc = new UltraTreeNodeExtendedDateTimeComparer();
            listFeedItemsO.ColumnSettings.ColumnSets[0].Columns[0].SortComparer = sc;
            listFeedItemsO.ColumnSettings.ColumnSets[0].Columns[0].SortType = SortType.Ascending;
            listFeedItemsO.ColumnSettings.AllowSorting = DefaultableBoolean.True;
            listFeedItemsO.Override.SortComparer = sc;
            listFeedItemsO.DrawFilter = new ListFeedsDrawFilter();
            listFeedItemsO.AfterSelect += OnListFeedItemsO_AfterSelect;
            listFeedItemsO.KeyDown += OnListFeedItemsO_KeyDown;
            listFeedItemsO.BeforeExpand += listFeedItemsO_BeforeExpand;
            listFeedItemsO.MouseDown += listFeedItemsO_MouseDown;
            listFeedItemsO.AfterSortChange += listFeedItemsO_AfterSortChange;
        }

        /// <summary>
        /// Init the colors, draw filter and bigger font of the Detail Header Caption.
        /// </summary>
        private void InitFeedDetailsCaption()
        {
            detailHeaderCaption.Font = new Font("Arial", 12f, FontStyle.Bold);
            detailHeaderCaption.DrawFilter = new SmoothLabelDrawFilter(detailHeaderCaption);
            detailHeaderCaption.Appearance.BackColor =
                FontColorHelper.UiColorScheme.OutlookNavPaneCurrentGroupHeaderGradientLight;
            detailHeaderCaption.Appearance.BackColor2 =
                FontColorHelper.UiColorScheme.OutlookNavPaneCurrentGroupHeaderGradientDark;
            detailHeaderCaption.Appearance.BackGradientStyle = GradientStyle.Vertical;
            detailHeaderCaption.Appearance.ForeColor =
                FontColorHelper.UiColorScheme.OutlookNavPaneCurrentGroupHeaderForecolor;
        }

        /// <summary>
        /// Init the colors, draw filter and bigger font of the Navigator Hidden Header Caption.
        /// </summary>
        private void InitNavigatorHiddenCaption()
        {
            navigatorHiddenCaption.Font = new Font("Arial", 12f, FontStyle.Bold);
            navigatorHiddenCaption.Appearance.BackColor =
                FontColorHelper.UiColorScheme.OutlookNavPaneCurrentGroupHeaderGradientLight;
            navigatorHiddenCaption.Appearance.BackColor2 =
                FontColorHelper.UiColorScheme.OutlookNavPaneCurrentGroupHeaderGradientDark;
            navigatorHiddenCaption.Appearance.BackGradientStyle = GradientStyle.Horizontal;
            navigatorHiddenCaption.ForeColor = FontColorHelper.UiColorScheme.OutlookNavPaneCurrentGroupHeaderForecolor;
            navigatorHiddenCaption.ImageClick += OnNavigatorExpandImageClick;
        }

        /// <summary>
        /// Initializes the search panel.
        /// </summary>
        private void InitializeSearchPanel()
        {
            searchPanel = new SearchPanel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.Location = new Point(0, 0);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(237, 496);
            searchPanel.TabIndex = 0;
            panelRssSearch.Controls.Add(searchPanel);

            //this.owner.FeedHandler.NewsItemSearchResult += new FeedSource.NewsItemSearchResultEventHandler(this.OnNewsItemSearchResult);
            owner.FeedHandler.SearchFinished += OnNewsItemSearchFinished;

            searchPanel.BeforeNewsItemSearch += OnSearchPanelBeforeNewsItemSearch;
            searchPanel.NewsItemSearch += OnSearchPanelStartNewsItemSearch;

            owner.FindersSearchRoot.SetScopeResolveCallback(ScopeResolve);
        }

        private void InitToaster()
        {
            toastNotifier = new ToastNotifier(
                OnExternalActivateFeedItem,
                OnExternalDisplayFeedProperties,
                OnExternalActivateFeed,
                PlayEnclosure);
        }

        private void InitListView()
        {
            colTopic.Text = SR.ListviewColumnCaptionTopic;
            colHeadline.Text = SR.ListviewColumnCaptionHeadline;
            colDate.Text = SR.ListviewColumnCaptionDate;

            listFeedItems.SmallImageList = _listImages;
            listFeedItemsO.ImageList = _listImages;
            owner.FeedlistLoaded += OnOwnerFeedlistLoaded;
            listFeedItems.ColumnClick += OnFeedListItemsColumnClick;
            listFeedItems.SelectedIndexChanged += listFeedItems_SelectedIndexChanged;

            if (owner.Preferences.BuildRelationCosmos)
            {
                listFeedItems.ShowAsThreads = true;
            }
            else
            {
                //listFeedItems.ShowAsThreads = false;
                listFeedItems.ShowInGroups = false;
                listFeedItems.AutoGroupMode = false;
            }
        }


        public void ResetHtmlDetail()
        {
            /* NOTE: ActiveX security band behavior isn't reset in this case because it seems Internet Feature
			 * Settings only applies on newly created IE Controls and cannot be changed after creation */
            ResetHtmlDetail(false);
        }

        private void InitHtmlDetail()
        {
            ResetHtmlDetail(true);
        }

        private void ResetHtmlDetail(bool clearContent)
        {
            // enable enhanced browser security available with XP SP2:
            htmlDetail.EnhanceBrowserSecurityForProcess();

            // configurable settings:
            htmlDetail.ActiveXEnabled = owner.Preferences.BrowserActiveXAllowed;
                //(bool)RssBanditApplication.ReadAppSettingsEntry("FeedDetailPane.ActiveXEnabled", typeof(bool), false);
            HtmlControl.SetInternetFeatureEnabled(
                InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL,
                SetFeatureFlag.SET_FEATURE_ON_PROCESS,
                htmlDetail.ActiveXEnabled);
            htmlDetail.ImagesDownloadEnabled = owner.Preferences.BrowserImagesAllowed;
                //(bool)RssBanditApplication.ReadAppSettingsEntry("FeedDetailPane.ImagesDownloadEnabled", typeof(bool), true);
            htmlDetail.JavaEnabled = owner.Preferences.BrowserJavaAllowed;
                //(bool)RssBanditApplication.ReadAppSettingsEntry("FeedDetailPane.JavaEnabled", typeof(bool), false);
            htmlDetail.VideoEnabled = owner.Preferences.BrowserVideoAllowed;
                //(bool)RssBanditApplication.ReadAppSettingsEntry("FeedDetailPane.VideoEnabled", typeof(bool), false);
            htmlDetail.FrameDownloadEnabled = RssBanditApplication.ReadAppSettingsEntry("FeedDetailPane.FrameDownloadEnabled", false);
            // hardcoded settings:
            htmlDetail.Border3d = true;
            htmlDetail.FlatScrollBars = true;
            htmlDetail.ScriptEnabled = owner.Preferences.BrowserJavascriptAllowed;
                //(bool)RssBanditApplication.ReadAppSettingsEntry("FeedDetailPane.ScriptEnabled", typeof(bool), true); //maybe this should be false by default?
            htmlDetail.ScriptObject = null; // set this later to enable inner-HTML function calls
            htmlDetail.ScrollBarsEnabled = true;
            htmlDetail.AllowInPlaceNavigation = false;
#if DEBUG
            // allow IEControl reporting of javascript errors while dev.:
            htmlDetail.SilentModeEnabled = true;
#else
			this.htmlDetail.SilentModeEnabled = true;
#endif

            htmlDetail.Tag = _docFeedDetails;

            htmlDetail.StatusTextChanged += OnWebStatusTextChanged;
            htmlDetail.BeforeNavigate += OnWebBeforeNavigate;
            htmlDetail.NavigateComplete += OnWebNavigateComplete;
            htmlDetail.DocumentComplete += OnWebDocumentComplete;
            htmlDetail.NewWindow += OnWebNewWindow;
            htmlDetail.ProgressChanged += OnWebProgressChanged;

            htmlDetail.TranslateAccelerator += OnWebTranslateAccelerator;
           
            if (clearContent)
            {
                htmlDetail.Clear();
            }
        }

        private void InitFeedTreeView()
        {
            // enable native info tips support:
            //Win32.ModifyWindowStyle(treeFeeds.Handle, 0, Win32.TVS_INFOTIP);
            treeFeeds.PathSeparator = FeedSource.CategorySeparator;
            treeFeeds.ImageList = _treeImages;
            treeFeeds.Nodes.Override.Sort = SortType.None; // do not sort the root entries
            treeFeeds.ScrollBounds = ScrollBounds.ScrollToFill;

            //this.treeFeeds.CreationFilter = new TreeFeedsNodeUIElementCreationFilter();
            treeFeeds.Override.SelectionType = SelectType.SingleAutoDrag;

            // create RootFolderType.MyFeeds:
            TreeFeedsNodeBase root =
                new RootNode(SR.FeedNodeMyFeedsCaption, Resource.SubscriptionTreeImage.AllSubscriptions,
                             Resource.SubscriptionTreeImage.AllSubscriptionsExpanded, _treeRootContextMenu);
            treeFeeds.Nodes.Add(root);
            root.ReadCounterZero += OnTreeNodeFeedsRootReadCounterZero;
            _roots[(int) RootFolderType.MyFeeds] = root;

            // add the root as the first history entry:
            AddHistoryEntry(root, null);

            // create RootFolderType.Finder:
            root =
                new FinderRootNode(SR.FeedNodeFinderRootCaption, Resource.SubscriptionTreeImage.AllFinderFolders,
                                   Resource.SubscriptionTreeImage.AllFinderFoldersExpanded,
                                   _treeSearchFolderRootContextMenu);
            treeFeeds.Nodes.Add(root);
            _roots[(int) RootFolderType.Finder] = root;
            if (SearchIndexBehavior.NoIndexing == owner.FeedHandler.Configuration.SearchIndexBehavior)
                root.Visible = false;

            // create RootFolderType.SmartFolder:
            root =
                new SpecialRootNode(SR.FeedNodeSpecialFeedsCaption, Resource.SubscriptionTreeImage.AllSmartFolders,
                                    Resource.SubscriptionTreeImage.AllSmartFoldersExpanded, null);
            treeFeeds.Nodes.Add(root);
            _roots[(int) RootFolderType.SmartFolders] = root;

            treeFeeds.DrawFilter = new TreeFeedsDrawFilter();

            treeFeeds.MouseDown += OnTreeFeedMouseDown;
            treeFeeds.MouseUp += OnTreeFeedMouseUp;
            treeFeeds.DragOver += OnTreeFeedDragOver;
            treeFeeds.AfterSelect += OnTreeFeedAfterSelect;
            treeFeeds.QueryContinueDrag += OnTreeFeedQueryContiueDrag;
            treeFeeds.DragEnter += OnTreeFeedDragEnter;
            treeFeeds.MouseMove += OnTreeFeedMouseMove;
            treeFeeds.BeforeSelect += OnTreeFeedBeforeSelect;
            treeFeeds.BeforeLabelEdit += OnTreeFeedBeforeLabelEdit;
            treeFeeds.ValidateLabelEdit += OnTreeFeedsValidateLabelEdit;
            treeFeeds.AfterLabelEdit += OnTreeFeedAfterLabelEdit;
            treeFeeds.DragDrop += OnTreeFeedDragDrop;
            treeFeeds.SelectionDragStart += OnTreeFeedSelectionDragStart;
            treeFeeds.GiveFeedback += OnTreeFeedGiveFeedback;
            treeFeeds.DoubleClick += OnTreeFeedDoubleClick;
        }

        #endregion

        #region Menu init routines

        protected void InitContextMenus()
        {
            #region tree view context menus

            #region root menu

            _treeRootContextMenu = new ContextMenu();

            AppContextMenuCommand sub1 = new AppContextMenuCommand("cmdNewFeed",
                                                                   owner.Mediator,
                                                                   new ExecuteCommandHandler(owner.CmdNewFeed),
                                                                   SR.MenuNewFeedCaption2, SR.MenuNewFeedDesc, 1,
                                                                   _shortcutHandler);

            //sub1.ImageList  = _toolImages;

            AppContextMenuCommand sub2 = new AppContextMenuCommand("cmdNewCategory",
                                                                   owner.Mediator,
                                                                   new ExecuteCommandHandler(owner.CmdNewCategory),
                                                                   SR.MenuNewCategoryCaption, SR.MenuNewCategoryDesc, 2,
                                                                   _shortcutHandler);

            //sub2.ImageList  = _treeImages;

            MenuItem sep = new MenuItem("-");

            AppContextMenuCommand subR1 = new AppContextMenuCommand("cmdRefreshFeeds",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdRefreshFeeds),
                                                                    SR.MenuUpdateAllFeedsCaption,
                                                                    SR.MenuUpdateAllFeedsDesc, 0, _shortcutHandler);

            //subR1.ImageList  = _toolImages;

            AppContextMenuCommand subR2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdCatchUpCurrentSelectedNode),
                                                                    SR.MenuCatchUpOnAllCaption, SR.MenuCatchUpOnAllDesc,
                                                                    0, _shortcutHandler);
            //subR2.ImageList           = _listImages;

            AppContextMenuCommand subR3 = new AppContextMenuCommand("cmdDeleteAll",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdDeleteAll),
                                                                    SR.MenuDeleteAllFeedsCaption,
                                                                    SR.MenuDeleteAllFeedsDesc, 2, _shortcutHandler);
            //subR3.ImageList           = _toolImages;

            AppContextMenuCommand subR4 = new AppContextMenuCommand("cmdShowMainAppOptions",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdShowOptions),
                                                                    SR.MenuAppOptionsCaption, SR.MenuAppOptionsDesc, 10,
                                                                    _shortcutHandler);

            // append items
            _treeRootContextMenu.MenuItems.AddRange(
                new MenuItem[] {sub1, sub2, sep, subR1, subR2, sep.CloneMenu(), subR3, sep.CloneMenu(), subR4});

            #endregion

            #region category menu

            _treeCategoryContextMenu = new ContextMenu();

            AppContextMenuCommand subC1 = new AppContextMenuCommand("cmdUpdateCategory",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdUpdateCategory),
                                                                    SR.MenuUpdateCategoryCaption,
                                                                    SR.MenuUpdateCategoryDesc, _shortcutHandler);

            AppContextMenuCommand subC2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdCatchUpCurrentSelectedNode),
                                                                    SR.MenuCatchUpCategoryCaption,
                                                                    SR.MenuCatchUpCategoryDesc, 0, _shortcutHandler);
            //subC2.ImageList            = _listImages;

            AppContextMenuCommand subC3 = new AppContextMenuCommand("cmdRenameCategory",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdRenameCategory),
                                                                    SR.MenuRenameCategoryCaption,
                                                                    SR.MenuRenameCategoryDesc, _shortcutHandler);

            AppContextMenuCommand subC4 = new AppContextMenuCommand("cmdDeleteCategory",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdDeleteCategory),
                                                                    SR.MenuDeleteCategoryCaption,
                                                                    SR.MenuDeleteCategoryDesc, 2, _shortcutHandler);

            //subC4.ImageList            = _toolImages;

            AppContextMenuCommand subC5 = new AppContextMenuCommand("cmdShowCategoryProperties",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdShowCategoryProperties),
                                                                    SR.MenuShowCategoryPropertiesCaption,
                                                                    SR.MenuShowCategoryPropertiesDesc, 10,
                                                                    _shortcutHandler);

            AppContextMenuCommand subCL_ColLayoutMain = new AppContextMenuCommand("cmdColumnChooserMain",
                                                                                  owner.Mediator,
                                                                                  new ExecuteCommandHandler(CmdNop),
                                                                                  SR.MenuColumnChooserCaption,
                                                                                  SR.MenuColumnChooserDesc,
                                                                                  _shortcutHandler);

            foreach (string colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                AppContextMenuCommand subCL4_layoutSubColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
                                                                                         owner.Mediator,
                                                                                         new ExecuteCommandHandler(
                                                                                             CmdToggleListviewColumn),
                                                                                         SR.Keys.GetString(
                                                                                             "MenuColumnChooser" + colID +
                                                                                             "Caption"),
                                                                                         SR.Keys.GetString(
                                                                                             "MenuColumnChooser" + colID +
                                                                                             "Desc"), _shortcutHandler);

                subCL_ColLayoutMain.MenuItems.AddRange(new MenuItem[] {subCL4_layoutSubColumn});
            }

            AppContextMenuCommand subCL_subUseCatLayout =
                new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
                                          owner.Mediator,
                                          new ExecuteCommandHandler(CmdColumnChooserUseCategoryLayoutGlobal),
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalDesc, _shortcutHandler);

            AppContextMenuCommand subCL_subUseFeedLayout =
                new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
                                          owner.Mediator, new ExecuteCommandHandler(CmdColumnChooserUseFeedLayoutGlobal),
                                          SR.MenuColumnChooserUseFeedLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseFeedLayoutGlobalDesc, _shortcutHandler);

            AppContextMenuCommand subCL_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
                                                                                   owner.Mediator,
                                                                                   new ExecuteCommandHandler(
                                                                                       CmdColumnChooserResetToDefault),
                                                                                   SR.
                                                                                       MenuColumnChooserResetLayoutToDefaultCaption,
                                                                                   SR.
                                                                                       MenuColumnChooserResetLayoutToDefaultDesc,
                                                                                   _shortcutHandler);

            subCL_ColLayoutMain.MenuItems.AddRange(
                new MenuItem[]
                    {
                        sep.CloneMenu(), subCL_subUseCatLayout, subCL_subUseFeedLayout, sep.CloneMenu(),
                        subCL_subResetLayout
                    });

            // append items. Reuse cmdNewCat/cmdNewFeed, because it's allowed on categories
            _treeCategoryContextMenu.MenuItems.AddRange(
                new MenuItem[]
                    {
                        sub1.CloneMenu(), sub2.CloneMenu(), sep.CloneMenu(), subC1, subC2, sep.CloneMenu(), subC3,
                        sep.CloneMenu(), subC4, sep.CloneMenu(), subCL_ColLayoutMain, sep.CloneMenu(), subC5
                    });

            #endregion

            #region feed menu

            _treeFeedContextMenu = new ContextMenu();

            AppContextMenuCommand subF1 = new AppContextMenuCommand("cmdUpdateFeed",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdUpdateFeed),
                                                                    SR.MenuUpdateThisFeedCaption,
                                                                    SR.MenuUpdateThisFeedDesc, _shortcutHandler);

            AppContextMenuCommand subF2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdCatchUpCurrentSelectedNode),
                                                                    SR.MenuCatchUpThisFeedCaption,
                                                                    SR.MenuCatchUpThisFeedDesc, 0, _shortcutHandler);

            //subF2.ImageList                     = _listImages;
            AppContextMenuCommand subF3 = new AppContextMenuCommand("cmdRenameFeed",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdRenameFeed),
                                                                    SR.MenuRenameThisFeedCaption,
                                                                    SR.MenuRenameThisFeedDesc, _shortcutHandler);

            AppContextMenuCommand subF4 = new AppContextMenuCommand("cmdDeleteFeed",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(CmdDeleteFeed),
                                                                    SR.MenuDeleteThisFeedCaption,
                                                                    SR.MenuDeleteThisFeedDesc, 2, _shortcutHandler);

            //subF4.ImageList            = _toolImages;

            AppContextMenuCommand subFeedCopy = new AppContextMenuCommand("cmdCopyFeed",
                                                                          owner.Mediator,
                                                                          new ExecuteCommandHandler(CmdCopyFeed),
                                                                          SR.MenuCopyFeedCaption, SR.MenuCopyFeedDesc, 1,
                                                                          _shortcutHandler);

            AppContextMenuCommand subFeedCopy_sub1 = new AppContextMenuCommand("cmdCopyFeedLinkToClipboard",
                                                                               owner.Mediator,
                                                                               new ExecuteCommandHandler(
                                                                                   CmdCopyFeedLinkToClipboard),
                                                                               SR.MenuCopyFeedLinkToClipboardCaption,
                                                                               SR.MenuCopyFeedLinkToClipboardDesc, 1,
                                                                               _shortcutHandler);

            AppContextMenuCommand subFeedCopy_sub2 = new AppContextMenuCommand("cmdCopyFeedHomepageLinkToClipboard",
                                                                               owner.Mediator,
                                                                               new ExecuteCommandHandler(
                                                                                   CmdCopyFeedHomeLinkToClipboard),
                                                                               SR.MenuCopyFeedHomeLinkToClipboardCaption,
                                                                               SR.MenuCopyFeedHomeLinkToClipboardDesc, 1,
                                                                               _shortcutHandler);

            AppContextMenuCommand subFeedCopy_sub3 =
                new AppContextMenuCommand("cmdCopyFeedHomepageTitleLinkToClipboard",
                                          owner.Mediator, new ExecuteCommandHandler(CmdCopyFeedHomeTitleLinkToClipboard),
                                          SR.MenuCopyFeedFeedHomeTitleLinkToClipboardCaption,
                                          SR.MenuCopyFeedFeedHomeTitleLinkToClipboardDesc, 1, _shortcutHandler);

            subFeedCopy.MenuItems.AddRange(new MenuItem[] {subFeedCopy_sub1, subFeedCopy_sub2, subFeedCopy_sub3});


            _feedInfoContextMenu = new MenuItem(SR.MenuAdvancedFeedInfoCaption);

            // the general properties item
            AppContextMenuCommand subF6 = new AppContextMenuCommand("cmdShowFeedProperties",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdShowFeedProperties),
                                                                    SR.MenuShowFeedPropertiesCaption,
                                                                    SR.MenuShowFeedPropertiesDesc, 10, _shortcutHandler);
            //subF6.ImageList				     = _browserImages;

            // layout menu(s):
            AppContextMenuCommand subFL_ColLayoutMain = new AppContextMenuCommand("cmdColumnChooserMain",
                                                                                  owner.Mediator,
                                                                                  new ExecuteCommandHandler(CmdNop),
                                                                                  SR.MenuColumnChooserCaption,
                                                                                  SR.MenuColumnChooserDesc,
                                                                                  _shortcutHandler);

            foreach (string colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                AppContextMenuCommand subFL4_layoutSubColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
                                                                                         owner.Mediator,
                                                                                         new ExecuteCommandHandler(
                                                                                             CmdToggleListviewColumn),
                                                                                         SR.Keys.GetString(
                                                                                             "MenuColumnChooser" + colID +
                                                                                             "Caption"),
                                                                                         SR.Keys.GetString(
                                                                                             "MenuColumnChooser" + colID +
                                                                                             "Desc"), _shortcutHandler);

                subFL_ColLayoutMain.MenuItems.AddRange(new MenuItem[] {subFL4_layoutSubColumn});
            }

            AppContextMenuCommand subFL_subUseCatLayout =
                new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
                                          owner.Mediator,
                                          new ExecuteCommandHandler(CmdColumnChooserUseCategoryLayoutGlobal),
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalDesc, _shortcutHandler);

            AppContextMenuCommand subFL_subUseFeedLayout =
                new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
                                          owner.Mediator, new ExecuteCommandHandler(CmdColumnChooserUseFeedLayoutGlobal),
                                          SR.MenuColumnChooserUseFeedLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseFeedLayoutGlobalDesc, _shortcutHandler);

            AppContextMenuCommand subFL_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
                                                                                   owner.Mediator,
                                                                                   new ExecuteCommandHandler(
                                                                                       CmdColumnChooserResetToDefault),
                                                                                   SR.
                                                                                       MenuColumnChooserResetLayoutToDefaultCaption,
                                                                                   SR.
                                                                                       MenuColumnChooserResetLayoutToDefaultDesc,
                                                                                   _shortcutHandler);

            subFL_ColLayoutMain.MenuItems.AddRange(
                new MenuItem[]
                    {
                        sep.CloneMenu(), subFL_subUseCatLayout, subFL_subUseFeedLayout, sep.CloneMenu(),
                        subFL_subResetLayout
                    });

            // append items. 
            _treeFeedContextMenu.MenuItems.AddRange(
                new MenuItem[]
                    {
                        subF1, subF2, subF3, sep.CloneMenu(), subF4, sep.CloneMenu(), subFeedCopy, sep.CloneMenu(),
                        _feedInfoContextMenu, sep.CloneMenu(), subFL_ColLayoutMain, sep.CloneMenu(), subF6
                    });

            #endregion

            #region feed info context submenu

            AppContextMenuCommand subInfoHome = new AppContextMenuCommand("cmdNavigateToFeedHome",
                                                                          owner.Mediator,
                                                                          new ExecuteCommandHandler(
                                                                              owner.CmdNavigateFeedHome),
                                                                          SR.MenuNavigateToFeedHomeCaption,
                                                                          SR.MenuNavigateToFeedHomeDesc,
                                                                          _shortcutHandler);

            AppContextMenuCommand subInfoCosmos = new AppContextMenuCommand("cmdNavigateToFeedCosmos",
                                                                            owner.Mediator,
                                                                            new ExecuteCommandHandler(
                                                                                owner.CmdNavigateFeedLinkCosmos),
                                                                            SR.MenuShowLinkCosmosCaption,
                                                                            SR.MenuShowLinkCosmosCaption);

            AppContextMenuCommand subInfoSource = new AppContextMenuCommand("cmdViewSourceOfFeed",
                                                                            owner.Mediator,
                                                                            new ExecuteCommandHandler(
                                                                                owner.CmdViewSourceOfFeed),
                                                                            SR.MenuViewSourceOfFeedCaption,
                                                                            SR.MenuViewSourceOfFeedDesc,
                                                                            _shortcutHandler);

            AppContextMenuCommand subInfoValidate = new AppContextMenuCommand("cmdValidateFeed",
                                                                              owner.Mediator,
                                                                              new ExecuteCommandHandler(
                                                                                  owner.CmdValidateFeed),
                                                                              SR.MenuValidateFeedCaption,
                                                                              SR.MenuValidateFeedDesc, _shortcutHandler);

            _feedInfoContextMenu.MenuItems.AddRange(
                new MenuItem[] {subInfoHome, subInfoCosmos, subInfoSource, subInfoValidate});

            #endregion

            #region root search folder context menu 

            _treeSearchFolderRootContextMenu = new ContextMenu();

            subF1 = new AppContextMenuCommand("cmdNewFinder",
                                              owner.Mediator, new ExecuteCommandHandler(CmdNewFinder),
                                              SR.MenuNewFinderCaption, SR.MenuNewFinderDesc, _shortcutHandler);
            subF2 = new AppContextMenuCommand("cmdDeleteAllFinders",
                                              owner.Mediator, new ExecuteCommandHandler(CmdDeleteAllFinder),
                                              SR.MenuFinderDeleteAllCaption, SR.MenuFinderDeleteAllDesc,
                                              _shortcutHandler);

            _treeSearchFolderRootContextMenu.MenuItems.AddRange(new MenuItem[] {subF1, sep.CloneMenu(), subF2});

            #endregion

            #region search folder context menu's

            _treeSearchFolderContextMenu = new ContextMenu();

            subF1 = new AppContextMenuCommand("cmdMarkFinderItemsRead",
                                              owner.Mediator, new ExecuteCommandHandler(CmdMarkFinderItemsRead),
                                              SR.MenuCatchUpOnAllCaption, SR.MenuCatchUpOnAllDesc, _shortcutHandler);
            subF2 = new AppContextMenuCommand("cmdRenameFinder",
                                              owner.Mediator, new ExecuteCommandHandler(CmdRenameFinder),
                                              SR.MenuFinderRenameCaption, SR.MenuFinderRenameDesc, _shortcutHandler);
            subF3 = new AppContextMenuCommand("cmdRefreshFinder",
                                              owner.Mediator, new ExecuteCommandHandler(CmdRefreshFinder),
                                              SR.MenuRefreshFinderCaption, SR.MenuRefreshFinderDesc, _shortcutHandler);
            subF4 = new AppContextMenuCommand("cmdDeleteFinder",
                                              owner.Mediator, new ExecuteCommandHandler(CmdDeleteFinder),
                                              SR.MenuFinderDeleteCaption, SR.MenuFinderDeleteDesc, _shortcutHandler);
            AppContextMenuCommand subFinderShowFullText = new AppContextMenuCommand("cmdFinderShowFullItemText",
                                                                                    owner.Mediator,
                                                                                    new ExecuteCommandHandler(
                                                                                        CmdFinderToggleExcerptsFullItemText),
                                                                                    SR.MenuFinderShowExcerptsCaption,
                                                                                    SR.MenuFinderShowExcerptsDesc,
                                                                                    _shortcutHandler);
            subF6 = new AppContextMenuCommand("cmdShowFinderProperties",
                                              owner.Mediator, new ExecuteCommandHandler(CmdShowFinderProperties),
                                              SR.MenuShowFinderPropertiesCaption, SR.MenuShowFinderPropertiesDesc,
                                              _shortcutHandler);

            _treeSearchFolderContextMenu.MenuItems.AddRange(
                new MenuItem[]
                    {
                        subF1, subF2, subF3, sep.CloneMenu(), subF4, sep.CloneMenu(), subFinderShowFullText,
                        sep.CloneMenu(), subF6
                    });


            _treeTempSearchFolderContextMenu = new ContextMenu();

            subF1 = new AppContextMenuCommand("cmdMarkFinderItemsRead",
                                              owner.Mediator, new ExecuteCommandHandler(CmdMarkFinderItemsRead),
                                              SR.MenuCatchUpOnAllCaption, SR.MenuCatchUpOnAllDesc, _shortcutHandler);
            subF2 = new AppContextMenuCommand("cmdRefreshFinder",
                                              owner.Mediator, new ExecuteCommandHandler(CmdRefreshFinder),
                                              SR.MenuRefreshFinderCaption, SR.MenuRefreshFinderDesc, _shortcutHandler);
            subF3 = new AppContextMenuCommand("cmdSubscribeToFinderResult",
                                              owner.Mediator, new ExecuteCommandHandler(CmdSubscribeToFinderResult),
                                              SR.MenuSubscribeToFinderResultCaption, SR.MenuSubscribeToFinderResultDesc,
                                              _shortcutHandler);
            subF3.Enabled = false; // dynamic
            subF4 = new AppContextMenuCommand("cmdShowFinderProperties",
                                              owner.Mediator, new ExecuteCommandHandler(CmdShowFinderProperties),
                                              SR.MenuShowFinderPropertiesCaption, SR.MenuShowFinderPropertiesDesc,
                                              _shortcutHandler);

            _treeTempSearchFolderContextMenu.MenuItems.AddRange(
                new MenuItem[]
                    {
                        subF1, subF2, sep.CloneMenu(), subF3, sep.CloneMenu(), subFinderShowFullText.CloneMenu(),
                        sep.CloneMenu(), subF4
                    });

            #endregion

            treeFeeds.ContextMenu = _treeRootContextMenu; // init to root context

            #endregion

            #region list view context menu

            _listContextMenu = new ContextMenu();

            AppContextMenuCommand subL0 = new AppContextMenuCommand("cmdMarkSelectedFeedItemsRead",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdMarkFeedItemsRead),
                                                                    SR.MenuCatchUpSelectedNodeCaption,
                                                                    SR.MenuCatchUpSelectedNodeDesc, 0, _shortcutHandler);

            AppContextMenuCommand subL1 = new AppContextMenuCommand("cmdMarkSelectedFeedItemsUnread",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdMarkFeedItemsUnread),
                                                                    SR.MenuMarkFeedItemsUnreadCaption,
                                                                    SR.MenuMarkFeedItemsUnreadDesc, 1, _shortcutHandler);

            //subL1.ImageList           = _listImages;

            AppContextMenuCommand subL2 = new AppContextMenuCommand("cmdFeedItemPostReply",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdPostReplyToItem),
                                                                    SR.MenuFeedItemPostReplyCaption,
                                                                    SR.MenuFeedItemPostReplyDesc, 5, _shortcutHandler);
            //subL2.ImageList = _toolImages;
            subL2.Enabled = false; // dynamically enabled on runtime if feed supports commentAPI

            AppContextMenuCommand subL3 = new AppContextMenuCommand("cmdFlagNewsItem",
                                                                    owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                                    SR.MenuFlagFeedItemCaption, SR.MenuFlagFeedItemDesc,
                                                                    1, _shortcutHandler);
            //subL3.ImageList                  = _listImages;
            subL3.Enabled = false; // dynamically enabled on runtime if feed supports flag

            AppContextMenuCommand subL3_sub1 = new AppContextMenuCommand("cmdFlagNewsItemForFollowUp",
                                                                         owner.Mediator,
                                                                         new ExecuteCommandHandler(
                                                                             CmdFlagNewsItemForFollowUp),
                                                                         SR.MenuFlagFeedItemFollowUpCaption,
                                                                         SR.MenuFlagFeedItemFollowUpDesc, 1,
                                                                         _shortcutHandler);
            AppContextMenuCommand subL3_sub2 = new AppContextMenuCommand("cmdFlagNewsItemForReview",
                                                                         owner.Mediator,
                                                                         new ExecuteCommandHandler(
                                                                             CmdFlagNewsItemForReview),
                                                                         SR.MenuFlagFeedItemReviewCaption,
                                                                         SR.MenuFlagFeedItemReviewDesc, 1,
                                                                         _shortcutHandler);
            AppContextMenuCommand subL3_sub3 = new AppContextMenuCommand("cmdFlagNewsItemForReply",
                                                                         owner.Mediator,
                                                                         new ExecuteCommandHandler(
                                                                             CmdFlagNewsItemForReply),
                                                                         SR.MenuFlagFeedItemReplyCaption,
                                                                         SR.MenuFlagFeedItemReplyDesc, 1,
                                                                         _shortcutHandler);
            AppContextMenuCommand subL3_sub4 = new AppContextMenuCommand("cmdFlagNewsItemRead",
                                                                         owner.Mediator,
                                                                         new ExecuteCommandHandler(CmdFlagNewsItemRead),
                                                                         SR.MenuFlagFeedItemReadCaption,
                                                                         SR.MenuFlagFeedItemReadDesc, 1,
                                                                         _shortcutHandler);
            AppContextMenuCommand subL3_sub5 = new AppContextMenuCommand("cmdFlagNewsItemForward",
                                                                         owner.Mediator,
                                                                         new ExecuteCommandHandler(
                                                                             CmdFlagNewsItemForward),
                                                                         SR.MenuFlagFeedItemForwardCaption,
                                                                         SR.MenuFlagFeedItemForwardDesc, 1,
                                                                         _shortcutHandler);
            AppContextMenuCommand subL3_sub8 = new AppContextMenuCommand("cmdFlagNewsItemComplete",
                                                                         owner.Mediator,
                                                                         new ExecuteCommandHandler(
                                                                             CmdFlagNewsItemComplete),
                                                                         SR.MenuFlagFeedItemCompleteCaption,
                                                                         SR.MenuFlagFeedItemCompleteDesc, 1,
                                                                         _shortcutHandler);
            AppContextMenuCommand subL3_sub9 = new AppContextMenuCommand("cmdFlagNewsItemNone",
                                                                         owner.Mediator,
                                                                         new ExecuteCommandHandler(CmdFlagNewsItemNone),
                                                                         SR.MenuFlagFeedItemClearCaption,
                                                                         SR.MenuFlagFeedItemClearDesc, 1,
                                                                         _shortcutHandler);

            subL3.MenuItems.AddRange(
                new MenuItem[]
                    {
                        subL3_sub1, subL3_sub2, subL3_sub3, subL3_sub4, subL3_sub5, sep.CloneMenu(), subL3_sub8,
                        sep.CloneMenu(), subL3_sub9
                    });

            AppContextMenuCommand subL10 = new AppContextMenuCommand("cmdCopyNewsItem",
                                                                     owner.Mediator,
                                                                     new ExecuteCommandHandler(CmdCopyNewsItem),
                                                                     SR.MenuCopyFeedItemCaption, SR.MenuCopyFeedItemDesc,
                                                                     1, _shortcutHandler);

            AppContextMenuCommand subL10_sub1 = new AppContextMenuCommand("cmdCopyNewsItemLinkToClipboard",
                                                                          owner.Mediator,
                                                                          new ExecuteCommandHandler(
                                                                              CmdCopyNewsItemLinkToClipboard),
                                                                          SR.MenuCopyFeedItemLinkToClipboardCaption,
                                                                          SR.MenuCopyFeedItemLinkToClipboardDesc, 1,
                                                                          _shortcutHandler);
            AppContextMenuCommand subL10_sub2 = new AppContextMenuCommand("cmdCopyNewsItemTitleLinkToClipboard",
                                                                          owner.Mediator,
                                                                          new ExecuteCommandHandler(
                                                                              CmdCopyNewsItemTitleLinkToClipboard),
                                                                          SR.MenuCopyFeedItemTitleLinkToClipboardCaption,
                                                                          SR.MenuCopyFeedItemTitleLinkToClipboardDesc, 1,
                                                                          _shortcutHandler);
            AppContextMenuCommand subL10_sub3 = new AppContextMenuCommand("cmdCopyNewsItemContentToClipboard",
                                                                          owner.Mediator,
                                                                          new ExecuteCommandHandler(
                                                                              CmdCopyNewsItemContentToClipboard),
                                                                          SR.MenuCopyFeedItemContentToClipboardCaption,
                                                                          SR.MenuCopyFeedItemContentToClipboardDesc, 1,
                                                                          _shortcutHandler);

            subL10.MenuItems.AddRange(new MenuItem[] {subL10_sub1, subL10_sub2, subL10_sub3});


            AppContextMenuCommand subL4 = new AppContextMenuCommand("cmdColumnChooserMain",
                                                                    owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                                    SR.MenuColumnChooserCaption,
                                                                    SR.MenuColumnChooserDesc, _shortcutHandler);
            //subL3.ImageList                  = _listImages;

            foreach (string colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                AppContextMenuCommand subL4_subColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
                                                                                  owner.Mediator,
                                                                                  new ExecuteCommandHandler(
                                                                                      CmdToggleListviewColumn),
                                                                                  SR.Keys.GetString(
                                                                                      "MenuColumnChooser" + colID +
                                                                                      "Caption"),
                                                                                  SR.Keys.GetString(
                                                                                      "MenuColumnChooser" + colID +
                                                                                      "Desc"), _shortcutHandler);

                subL4.MenuItems.AddRange(new MenuItem[] {subL4_subColumn});
            }

            AppContextMenuCommand subL4_subUseCatLayout =
                new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
                                          owner.Mediator,
                                          new ExecuteCommandHandler(CmdColumnChooserUseCategoryLayoutGlobal),
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalDesc, _shortcutHandler);

            AppContextMenuCommand subL4_subUseFeedLayout =
                new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
                                          owner.Mediator, new ExecuteCommandHandler(CmdColumnChooserUseFeedLayoutGlobal),
                                          SR.MenuColumnChooserUseFeedLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseFeedLayoutGlobalDesc, _shortcutHandler);

            AppContextMenuCommand subL4_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
                                                                                   owner.Mediator,
                                                                                   new ExecuteCommandHandler(
                                                                                       CmdColumnChooserResetToDefault),
                                                                                   SR.
                                                                                       MenuColumnChooserResetLayoutToDefaultCaption,
                                                                                   SR.
                                                                                       MenuColumnChooserResetLayoutToDefaultDesc,
                                                                                   _shortcutHandler);

            subL4.MenuItems.AddRange(
                new MenuItem[]
                    {
                        sep.CloneMenu(), subL4_subUseCatLayout, subL4_subUseFeedLayout, sep.CloneMenu(),
                        subL4_subResetLayout
                    });

            AppContextMenuCommand subL5 = new AppContextMenuCommand("cmdDeleteSelectedNewsItems",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdDeleteSelectedFeedItems),
                                                                    SR.MenuDeleteSelectedFeedItemsCaption,
                                                                    SR.MenuDeleteSelectedFeedItemsDesc, _shortcutHandler);

            AppContextMenuCommand subL6 = new AppContextMenuCommand("cmdRestoreSelectedNewsItems",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdRestoreSelectedFeedItems),
                                                                    SR.MenuRestoreSelectedFeedItemsCaption,
                                                                    SR.MenuRestoreSelectedFeedItemsDesc,
                                                                    _shortcutHandler);
            subL6.Visible = false; // dynamic visible only in "Deleted Items" view

            AppContextMenuCommand subL7 = new AppContextMenuCommand("cmdWatchItemComments",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(CmdWatchItemComments),
                                                                    SR.MenuFeedItemWatchCommentsCaption,
                                                                    SR.MenuFeedItemWatchCommentsDesc, 5,
                                                                    _shortcutHandler);
            //subL2.ImageList = _toolImages;
            subL7.Enabled = false;
                // dynamically enabled on runtime if feed supports thr:replied, slash:comments or wfw:commentRss			

            AppContextMenuCommand subL8 = new AppContextMenuCommand("cmdViewOutlookReadingPane",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(CmdViewOutlookReadingPane),
                                                                    SR.MenuViewOutlookReadingPane,
                                                                    SR.MenuViewOutlookReadingPane, _shortcutHandler);

            AppContextMenuCommand subL9 = new AppContextMenuCommand("cmdDownloadAttachment",
                                                                    owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                                    SR.MenuDownloadAttachmentCaption,
                                                                    SR.MenuDownloadAttachmentDesc, _shortcutHandler);
            subL9.Visible = false; // dynamic visible if the item has enclosures


            _listContextMenuDownloadAttachment = subL9;
            _listContextMenuDeleteItemsSeparator = sep.CloneMenu();
            _listContextMenuDownloadAttachmentsSeparator = sep.CloneMenu();
            _listContextMenu.MenuItems.AddRange(
                new MenuItem[]
                    {
                        subL2, subL3, subL0, subL1, subL7, sep.CloneMenu(), subL10,
                        _listContextMenuDownloadAttachmentsSeparator, subL9, _listContextMenuDeleteItemsSeparator, subL5,
                        subL6, sep.CloneMenu(), subL4, subL8
                    });
            listFeedItems.ContextMenu = _listContextMenu;
            listFeedItemsO.ContextMenu = _listContextMenu;

            #endregion

            #region Local Feeds context menu

            _treeLocalFeedContextMenu = new ContextMenu();

            AppContextMenuCommand subTL1 = new AppContextMenuCommand("cmdDeleteAllNewsItems",
                                                                     owner.Mediator,
                                                                     new ExecuteCommandHandler(
                                                                         owner.CmdDeleteAllFeedItems),
                                                                     SR.MenuDeleteAllFeedItemsCaption,
                                                                     SR.MenuDeleteAllFeedItemsDesc, 1, _shortcutHandler);
            //subTL1.ImageList           = _listImages;

            _treeLocalFeedContextMenu.MenuItems.AddRange(new MenuItem[] {subTL1});

            #endregion

            #region doc tab context menu

            _docTabContextMenu = new ContextMenu();

            AppContextMenuCommand subDT1 = new AppContextMenuCommand("cmdDocTabCloseThis",
                                                                     owner.Mediator,
                                                                     new ExecuteCommandHandler(CmdDocTabCloseSelected),
                                                                     SR.MenuDocTabsCloseCurrentCaption,
                                                                     SR.MenuDocTabsCloseCurrentDesc, 1, _shortcutHandler);
            //subDT1.ImageList           = _listImages;

            AppContextMenuCommand subDT2 = new AppContextMenuCommand("cmdDocTabCloseAllOnStrip",
                                                                     owner.Mediator,
                                                                     new ExecuteCommandHandler(CmdDocTabCloseAllOnStrip),
                                                                     SR.MenuDocTabsCloseAllOnStripCaption,
                                                                     SR.MenuDocTabsCloseAllOnStripDesc, 2,
                                                                     _shortcutHandler);
            //subDT2.ImageList           = _listImages;

            AppContextMenuCommand subDT3 = new AppContextMenuCommand("cmdDocTabCloseAll",
                                                                     owner.Mediator,
                                                                     new ExecuteCommandHandler(CmdDocTabCloseAll),
                                                                     SR.MenuDocTabsCloseAllCaption,
                                                                     SR.MenuDocTabsCloseAllDesc, 3, _shortcutHandler);
            //subDT3.ImageList           = _listImages;

            AppContextMenuCommand subDT4 = new AppContextMenuCommand("cmdDocTabLayoutHorizontal",
                                                                     owner.Mediator,
                                                                     new ExecuteCommandHandler(CmdDocTabLayoutHorizontal),
                                                                     SR.MenuDocTabsLayoutHorizontalCaption,
                                                                     SR.MenuDocTabsLayoutHorizontalDesc,
                                                                     _shortcutHandler);
            subDT4.Checked = (_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal);           
         

            AppContextMenuCommand subDT5 = new AppContextMenuCommand("cmdFeedDetailLayoutPosition",
                                                                     owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                                     SR.MenuFeedDetailLayoutCaption,
                                                                     SR.MenuFeedDetailLayoutDesc, _shortcutHandler);

            // subMenu:			
            AppContextMenuCommand subSub1 = new AppContextMenuCommand("cmdFeedDetailLayoutPosTop",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(
                                                                          CmdFeedDetailLayoutPosTop),
                                                                      SR.MenuFeedDetailLayoutTopCaption,
                                                                      SR.MenuFeedDetailLayoutTopDesc, _shortcutHandler);

            subSub1.Checked = true; // default

            AppContextMenuCommand subSub2 = new AppContextMenuCommand("cmdFeedDetailLayoutPosLeft",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(
                                                                          CmdFeedDetailLayoutPosLeft),
                                                                      SR.MenuFeedDetailLayoutLeftCaption,
                                                                      SR.MenuFeedDetailLayoutLeftDesc, _shortcutHandler);

            AppContextMenuCommand subSub3 = new AppContextMenuCommand("cmdFeedDetailLayoutPosRight",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(
                                                                          CmdFeedDetailLayoutPosRight),
                                                                      SR.MenuFeedDetailLayoutRightCaption,
                                                                      SR.MenuFeedDetailLayoutRightDesc, _shortcutHandler);

            AppContextMenuCommand subSub4 = new AppContextMenuCommand("cmdFeedDetailLayoutPosBottom",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(
                                                                          CmdFeedDetailLayoutPosBottom),
                                                                      SR.MenuFeedDetailLayoutBottomCaption,
                                                                      SR.MenuFeedDetailLayoutBottomDesc,
                                                                      _shortcutHandler);



            subDT5.MenuItems.AddRange(new MenuItem[] {subSub1, subSub2, subSub3, subSub4});


            _docTabContextMenu.MenuItems.AddRange(
                new MenuItem[] {subDT1, subDT2, subDT3, sep.CloneMenu(), subDT4, sep.CloneMenu(), subDT5});

            #endregion

            #region tray context menu

            _notifyContextMenu = new ContextMenu();

            AppContextMenuCommand subT1 = new AppContextMenuCommand("cmdShowGUI",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdShowMainGui),
                                                                    SR.MenuShowMainGuiCaption, SR.MenuShowMainGuiDesc,
                                                                    _shortcutHandler);
            subT1.DefaultItem = true;

            AppContextMenuCommand subT1_1 = new AppContextMenuCommand("cmdRefreshFeeds",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(owner.CmdRefreshFeeds),
                                                                      SR.MenuUpdateAllFeedsCaption,
                                                                      SR.MenuUpdateAllFeedsDesc, _shortcutHandler);
            AppContextMenuCommand subT2 = new AppContextMenuCommand("cmdShowMainAppOptions",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(owner.CmdShowOptions),
                                                                    SR.MenuAppOptionsCaption, SR.MenuAppOptionsDesc, 10,
                                                                    _shortcutHandler);
            //subT2.ImageList = _browserImages;

            AppContextMenuCommand subT5 = new AppContextMenuCommand("cmdShowConfiguredAlertWindows",
                                                                    owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                                    SR.MenuShowAlertWindowsCaption,
                                                                    SR.MenuShowAlertWindowsDesc, _shortcutHandler);
            //subT5.Checked = owner.Preferences.ShowConfiguredAlertWindows;

            #region ShowAlertWindows context submenu

            AppContextMenuCommand subT5_1 = new AppContextMenuCommand("cmdShowAlertWindowNone",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(
                                                                          owner.CmdShowAlertWindowNone),
                                                                      SR.MenuShowNoneAlertWindowsCaption,
                                                                      SR.MenuShowNoneAlertWindowsDesc, _shortcutHandler);
            subT5_1.Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.None);

            AppContextMenuCommand subT5_2 = new AppContextMenuCommand("cmdShowAlertWindowConfiguredFeeds",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(
                                                                          owner.CmdShowAlertWindowConfigPerFeed),
                                                                      SR.MenuShowConfiguredFeedAlertWindowsCaption,
                                                                      SR.MenuShowConfiguredFeedAlertWindowsDesc,
                                                                      _shortcutHandler);
            subT5_2.Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.AsConfiguredPerFeed);

            AppContextMenuCommand subT5_3 = new AppContextMenuCommand("cmdShowAlertWindowAll",
                                                                      owner.Mediator,
                                                                      new ExecuteCommandHandler(
                                                                          owner.CmdShowAlertWindowAll),
                                                                      SR.MenuShowAllAlertWindowsCaption,
                                                                      SR.MenuShowAllAlertWindowsDesc, _shortcutHandler);
            subT5_3.Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.All);

            subT5.MenuItems.AddRange(new MenuItem[] {subT5_1, subT5_2, subT5_3});

            #endregion

            AppContextMenuCommand subT6 = new AppContextMenuCommand("cmdShowNewItemsReceivedBalloon",
                                                                    owner.Mediator,
                                                                    new ExecuteCommandHandler(
                                                                        owner.CmdToggleShowNewItemsReceivedBalloon),
                                                                    SR.MenuShowNewItemsReceivedBalloonCaption,
                                                                    SR.MenuShowNewItemsReceivedBalloonDesc,
                                                                    _shortcutHandler);
            subT6.Checked = owner.Preferences.ShowNewItemsReceivedBalloon;

            AppContextMenuCommand subT10 = new AppContextMenuCommand("cmdCloseExit",
                                                                     owner.Mediator,
                                                                     new ExecuteCommandHandler(owner.CmdExitApp),
                                                                     SR.MenuAppCloseExitCaption, SR.MenuAppCloseExitDesc,
                                                                     _shortcutHandler);

            _notifyContextMenu.MenuItems.AddRange(
                new MenuItem[]
                    {
                        subT1, subT1_1, sep.CloneMenu(), sub1.CloneMenu(), subT2, sep.CloneMenu(), subT5, subT6,
                        sep.CloneMenu(), subT10
                    });

            #endregion
        }

        #endregion

        #region Toolbar routines

        /// <summary>
        /// Creates the IG toolbars dynamically.
        /// </summary>
        private void CreateIGToolbars()
        {
            ultraToolbarsManager = new ToolbarHelper.RssBanditToolbarManager(components);
            toolbarHelper = new ToolbarHelper(ultraToolbarsManager);

            historyMenuManager = new HistoryMenuManager();
            historyMenuManager.OnNavigateBack += OnHistoryNavigateGoBackItemClick;
            historyMenuManager.OnNavigateForward +=
                OnHistoryNavigateGoForwardItemClick;

            _Main_Toolbars_Dock_Area_Left = new UltraToolbarsDockArea();
            _Main_Toolbars_Dock_Area_Right = new UltraToolbarsDockArea();
            _Main_Toolbars_Dock_Area_Top = new UltraToolbarsDockArea();
            _Main_Toolbars_Dock_Area_Bottom = new UltraToolbarsDockArea();
            ((ISupportInitialize) (ultraToolbarsManager)).BeginInit();
            // 
            // ultraToolbarsManager
            // 
            ultraToolbarsManager.DesignerFlags = 1;
            ultraToolbarsManager.DockWithinContainer = this;
            ultraToolbarsManager.ImageListSmall = _allToolImages;
            ultraToolbarsManager.ShowFullMenusDelay = 500;
            ultraToolbarsManager.ShowToolTips = true;
            ultraToolbarsManager.Style = ToolbarStyle.Office2003;
            // 
            // _Main_Toolbars_Dock_Area_Left
            // 
            _Main_Toolbars_Dock_Area_Left.AccessibleRole = AccessibleRole.Grouping;
            _Main_Toolbars_Dock_Area_Left.BackColor = Color.FromArgb(158, 190, 245);
            _Main_Toolbars_Dock_Area_Left.DockedPosition = DockedPosition.Left;
            _Main_Toolbars_Dock_Area_Left.ForeColor = SystemColors.ControlText;
            _Main_Toolbars_Dock_Area_Left.Location = new Point(0, 99);
            _Main_Toolbars_Dock_Area_Left.Name = "_Main_Toolbars_Dock_Area_Left";
            _Main_Toolbars_Dock_Area_Left.Size = new Size(0, 212);
            _Main_Toolbars_Dock_Area_Left.ToolbarsManager = ultraToolbarsManager;
            // 
            // _Main_Toolbars_Dock_Area_Right
            // 
            _Main_Toolbars_Dock_Area_Right.AccessibleRole = AccessibleRole.Grouping;
            _Main_Toolbars_Dock_Area_Right.BackColor = Color.FromArgb(158, 190, 245);
            _Main_Toolbars_Dock_Area_Right.DockedPosition = DockedPosition.Right;
            _Main_Toolbars_Dock_Area_Right.ForeColor = SystemColors.ControlText;
            _Main_Toolbars_Dock_Area_Right.Location = new Point(507, 99);
            _Main_Toolbars_Dock_Area_Right.Name = "_Main_Toolbars_Dock_Area_Right";
            _Main_Toolbars_Dock_Area_Right.Size = new Size(0, 212);
            _Main_Toolbars_Dock_Area_Right.ToolbarsManager = ultraToolbarsManager;
            // 
            // _Main_Toolbars_Dock_Area_Top
            // 
            _Main_Toolbars_Dock_Area_Top.AccessibleRole = AccessibleRole.Grouping;
            _Main_Toolbars_Dock_Area_Top.BackColor = Color.FromArgb(158, 190, 245);
            _Main_Toolbars_Dock_Area_Top.DockedPosition = DockedPosition.Top;
            _Main_Toolbars_Dock_Area_Top.ForeColor = SystemColors.ControlText;
            _Main_Toolbars_Dock_Area_Top.Location = new Point(0, 0);
            _Main_Toolbars_Dock_Area_Top.Name = "_Main_Toolbars_Dock_Area_Top";
            _Main_Toolbars_Dock_Area_Top.Size = new Size(507, 99);
            _Main_Toolbars_Dock_Area_Top.ToolbarsManager = ultraToolbarsManager;
            // 
            // _Main_Toolbars_Dock_Area_Bottom
            // 
            _Main_Toolbars_Dock_Area_Bottom.AccessibleRole = AccessibleRole.Grouping;
            _Main_Toolbars_Dock_Area_Bottom.BackColor = Color.FromArgb(158, 190, 245);
            _Main_Toolbars_Dock_Area_Bottom.DockedPosition = DockedPosition.Bottom;
            _Main_Toolbars_Dock_Area_Bottom.ForeColor = SystemColors.ControlText;
            _Main_Toolbars_Dock_Area_Bottom.Location = new Point(0, 311);
            _Main_Toolbars_Dock_Area_Bottom.Name = "_Main_Toolbars_Dock_Area_Bottom";
            _Main_Toolbars_Dock_Area_Bottom.Size = new Size(507, 0);
            _Main_Toolbars_Dock_Area_Bottom.ToolbarsManager = ultraToolbarsManager;

            Controls.Add(_Main_Toolbars_Dock_Area_Left);
            Controls.Add(_Main_Toolbars_Dock_Area_Right);
            Controls.Add(_Main_Toolbars_Dock_Area_Top);
            Controls.Add(_Main_Toolbars_Dock_Area_Bottom);

            toolbarHelper.CreateToolbars(this, owner, _shortcutHandler);
            ((ISupportInitialize) (ultraToolbarsManager)).EndInit();

            ultraToolbarsManager.ToolClick += OnAnyToolbarToolClick;
            ultraToolbarsManager.BeforeToolDropdown += OnToolbarBeforeToolDropdown;
            owner.Mediator.BeforeCommandStateChanged += OnMediatorBeforeCommandStateChanged;
            owner.Mediator.AfterCommandStateChanged += OnMediatorAfterCommandStateChanged;
        }

        #endregion

        #region Init DocManager

        private void InitDocumentManager()
        {
            _docContainer.SuspendLayout();

            _docContainer.LayoutSystem.SplitMode = Orientation.Vertical;

            _docFeedDetails.TabImage = _listImages.Images[0];
            _docFeedDetails.Tag = this; // I'm the ITabState implementor
            if (Win32.IsOSAtLeastWindowsXP)
                ColorEx.ColorizeOneNote(_docFeedDetails, 0);

            _docContainer.ActiveDocument = _docFeedDetails;
            _docContainer.ShowControlContextMenu += OnDocContainerShowControlContextMenu;
            _docContainer.MouseDown += OnDocContainerMouseDown;
            _docContainer.DocumentClosing += OnDocContainerDocumentClosing;
            _docContainer.ActiveDocumentChanged += OnDocContainerActiveDocumentChanged;
            _docContainer.DoubleClick += OnDocContainerDoubleClick;

            panelFeedDetails.Dock = DockStyle.Fill;
            _docContainer.ResumeLayout(false);
        }

        #endregion

        #region DockHost init routines

        private void InitDockHosts()
        {
            sandDockManager.DockingFinished += OnDockManagerDockingFinished;
            sandDockManager.DockingStarted += OnDockManagerDockStarted;
        }

        #endregion

        #region TrayIcon routines

        private void InitTrayIcon()
        {
            if (components == null)
                components = new Container();
            _trayAni = new NotifyIconAnimation(components);
            _trayAni.DoubleClick += OnTrayIconDoubleClick;
            _trayAni.BalloonClick += OnTrayIconDoubleClick;
            _trayAni.BalloonTimeout += OnTrayAniBalloonTimeoutClose;
            _trayAni.ContextMenu = _notifyContextMenu;

            //_trayManager = new TrayStateManager(_trayAni, imageTrayAnimation);
            _trayManager = new TrayStateManager(_trayAni, null);
            _trayManager.SetState(ApplicationTrayState.NormalIdle);
        }

        #endregion

        #region Statusbar/Docking routines

        private void InitStatusBar()
        {
            _status.PanelClick += OnStatusPanelClick;
            _status.LocationChanged += OnStatusPanelLocationChanged;
            statusBarBrowserProgress.Width = 0;
            progressBrowser.Visible = false;
        }

        private void OnDockManagerDockStarted(object sender, EventArgs e)
        {
            SetBrowserStatusBarText(SR.DragDockablePanelInfo);
        }

        private void OnDockManagerDockingFinished(object sender, EventArgs e)
        {
            SetBrowserStatusBarText(String.Empty);
        }

        #endregion

        #region Callback and event handler routines

        private static void CmdNop(ICommand sender)
        {
            // Nop: no operation here
        }

        internal void CmdOpenLinkInExternalBrowser(ICommand sender)
        {
            owner.NavigateToUrlInExternalBrowser(UrlText);
        }

        internal void CmdToggleMainTBViewState(ICommand sender)
        {
            bool enable = owner.Mediator.IsChecked(sender);
            owner.Mediator.SetChecked(enable, "cmdToggleMainTBViewState");
            toolbarHelper.SetToolbarVisible(Resource.Toolbar.MainTools, enable);
        }

        internal void CmdToggleWebTBViewState(ICommand sender)
        {
            bool enable = owner.Mediator.IsChecked(sender);
            owner.Mediator.SetChecked(enable, "cmdToggleWebTBViewState");
            toolbarHelper.SetToolbarVisible(Resource.Toolbar.WebTools, enable);
        }

        internal void CmdToggleWebSearchTBViewState(ICommand sender)
        {
            bool enable = owner.Mediator.IsChecked(sender);
            owner.Mediator.SetChecked(enable, "cmdToggleWebSearchTBViewState");
            toolbarHelper.SetToolbarVisible(Resource.Toolbar.SearchTools, enable);
        }

        /// <summary>
        /// Called before IG view menu tool dropdown.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Infragistics.Win.UltraWinToolbars.BeforeToolDropdownEventArgs"/> instance containing the event data.</param>
        private void OnToolbarBeforeToolDropdown(object sender, BeforeToolDropdownEventArgs e)
        {
            if (e.Tool.Key == "mnuViewToolbars")
            {
                owner.Mediator.SetChecked(toolbarHelper.IsToolbarVisible(Resource.Toolbar.MainTools),
                                          "cmdToggleMainTBViewState");
                owner.Mediator.SetChecked(toolbarHelper.IsToolbarVisible(Resource.Toolbar.WebTools),
                                          "cmdToggleWebTBViewState");
                owner.Mediator.SetChecked(toolbarHelper.IsToolbarVisible(Resource.Toolbar.SearchTools),
                                          "cmdToggleWebSearchTBViewState");
            }
            else if (e.Tool.Key == "cmdColumnChooserMain")
            {
                RefreshListviewColumnContextMenu();
            }
            else if (e.Tool.Key == "cmdBrowserGoBack" || e.Tool.Key == "cmdBrowserGoForward")
            {
                // we switch the dropdown chevron dynamically.
                // as a result, we now get only the before/afterdropdown events, but
                // not anymore the toolclick. So we simulate the toolClick it here:
                AppPopupMenuCommand cmd = e.Tool as AppPopupMenuCommand;
                if (cmd != null)
                {
                    if (cmd.Tools.Count == 0)
                    {
                        e.Cancel = true;
                        OnAnyToolbarToolClick(this, new ToolClickEventArgs(e.Tool, null));
                    }
                }
            }
        }

        internal void ToggleNavigationPaneView(NavigationPaneView pane)
        {
            if (!Navigator.Visible)
                OnNavigatorExpandImageClick(this, EventArgs.Empty);

            if (pane == NavigationPaneView.RssSearch)
            {
                Navigator.SelectedGroup = Navigator.Groups[Resource.NavigatorGroup.RssSearch];
                owner.Mediator.SetChecked("+cmdToggleRssSearchTabState");
                owner.Mediator.SetChecked("-cmdToggleTreeViewState");
            }
            else
            {
                Navigator.SelectedGroup = Navigator.Groups[Resource.NavigatorGroup.Subscriptions];
                owner.Mediator.SetChecked("-cmdToggleRssSearchTabState");
                owner.Mediator.SetChecked("+cmdToggleTreeViewState");
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to view the feed subscriptions docked panel.
        internal void CmdDockShowSubscriptions(ICommand sender)
        {
            ToggleNavigationPaneView(NavigationPaneView.Subscriptions);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to view the feed descriptions docked panel.
        internal void CmdDockShowRssSearch(ICommand sender)
        {
            ToggleNavigationPaneView(NavigationPaneView.RssSearch);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to close the selected doc tab.
        private void CmdDocTabCloseSelected(ICommand sender)
        {
            Point pos = (_contextMenuCalledAt != Point.Empty ? _contextMenuCalledAt : Cursor.Position);
            ControlLayoutSystem underMouse =
                _docContainer.GetLayoutSystemAt(_docContainer.PointToClient(pos)) as ControlLayoutSystem;
            if (underMouse != null)
            {
                DockControl docUnderMouse = underMouse.GetControlAt(_docContainer.PointToClient(pos));
                if (docUnderMouse != null)
                {
                    RemoveDocTab(docUnderMouse);
                    return;
                }
            }
            // try simply to remove current active:
            RemoveDocTab(_docContainer.ActiveDocument);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to close all doc tabs on the current strip.
        private void CmdDocTabCloseAllOnStrip(ICommand sender)
        {
            Point pos = (_contextMenuCalledAt != Point.Empty ? _contextMenuCalledAt : Cursor.Position);
            ControlLayoutSystem underMouse =
                _docContainer.GetLayoutSystemAt(_docContainer.PointToClient(pos)) as ControlLayoutSystem;
            if (underMouse == null)
                underMouse = _docContainer.ActiveDocument.LayoutSystem;

            DockControl[] docs = new DockControl[underMouse.Controls.Count];
            underMouse.Controls.CopyTo(docs, 0); // prevent InvalidOpException on Collections
            foreach (DockControl doc in docs)
            {
                ITabState state = (ITabState) doc.Tag;
                if (state.CanClose)
                    _docContainer.RemoveDocument(doc);
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to close all doc tabs on all strips.
        private void CmdDocTabCloseAll(ICommand sender)
        {
            DockControl[] docs = new DockControl[_docContainer.Documents.Length];
            _docContainer.Documents.CopyTo(docs, 0); // prevent InvalidOpException on Collections
            foreach (DockControl doc in docs)
            {
                ITabState state = (ITabState) doc.Tag;
                if (state.CanClose)
                    _docContainer.RemoveDocument(doc);
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the splitted doc strip layout.
        private void CmdDocTabLayoutHorizontal(ICommand sender)
        {
            if (owner.Mediator.IsChecked("cmdDocTabLayoutHorizontal"))
            {
                _docContainer.LayoutSystem.SplitMode = Orientation.Vertical;
            }
            else
            {
                _docContainer.LayoutSystem.SplitMode = Orientation.Horizontal;
            }
            owner.Mediator.SetChecked((_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal),
                                      "cmdDocTabLayoutHorizontal");
        }

        internal void CmdViewOutlookReadingPane(ICommand sender)
        {
            if (sender != null)
            {
                // check the real command (sender) for current unified state:
                bool enable = owner.Mediator.IsChecked(sender);
                owner.Mediator.SetChecked(enable, "cmdViewOutlookReadingPane");
                ShowOutlookReadingPane(enable);
            }
        }

        private void ShowOutlookReadingPane(bool enable)
        {
            if (enable)
            {
                //Prepare ListView Contents
                ThreadedListViewItem[] items = new ThreadedListViewItem[listFeedItems.Items.Count];
                int ind = 0;
                foreach (ThreadedListViewItem lvi in listFeedItems.Items)
                {
                    items[ind++] = lvi;
                }
                listFeedItemsO.Clear();
                listFeedItemsO.AddRange(items);
                //
                listFeedItems.Visible = false;
                listFeedItemsO.Visible = true;
            }
            else
            {
                listFeedItems.Visible = true;
                listFeedItemsO.Visible = false;
                //
                listFeedItemsO.Clear();
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosTop(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Top);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosBottom(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Bottom);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosLeft(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Left);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosRight(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Right);
        }

        #region CmdFlag... routines

        public void CmdFlagNewsItemForFollowUp(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.FollowUp);
            }
        }

        public void CmdFlagNewsItemNone(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.None);
            }
        }

        public void CmdFlagNewsItemComplete(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Complete);
            }
        }

        public void CmdFlagNewsItemForward(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Forward);
            }
        }

        public void CmdFlagNewsItemRead(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Read);
            }
        }

        public void CmdFlagNewsItemForReply(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Reply);
            }
        }

        public void CmdFlagNewsItemForReview(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Review);
            }
        }

        #endregion

        #region CmdCopyNewsItemXXX and CmdCopyFeedXXX to Clipboard

        private void CmdCopyFeed(ICommand sender)
        {
            // dummy, just a submenu
            if (sender is AppContextMenuCommand)
                CurrentSelectedFeedsNode = null;
        }

        private void CmdCopyFeedLinkToClipboard(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode.Type == FeedNodeType.Feed && feedsNode.DataKey != null)
            {
                if (owner.FeedHandler.IsSubscribed(feedsNode.DataKey))
                    Clipboard.SetDataObject(feedsNode.DataKey);
            }

            if (sender is AppContextMenuCommand) // needed at the treeview
                CurrentSelectedFeedsNode = null;
        }

        private void CmdCopyFeedHomeLinkToClipboard(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode.Type == FeedNodeType.Feed)
            {
                IFeedDetails fd = owner.GetFeedDetails(feedsNode.DataKey);
                string link;

                if (fd != null)
                {
                    link = fd.Link;
                }
                else
                {
                    link = feedsNode.DataKey;
                }

                if (!string.IsNullOrEmpty(link))
                {
                    Clipboard.SetDataObject(link);
                }
            }

            if (sender is AppContextMenuCommand) // needed at the treeview
                CurrentSelectedFeedsNode = null;
        }

        private void CmdCopyFeedHomeTitleLinkToClipboard(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode.Type == FeedNodeType.Feed)
            {
                IFeedDetails fd = owner.GetFeedDetails(feedsNode.DataKey);
                string link, title;

                if (fd != null)
                {
                    link = fd.Link;
                    title = fd.Title;
                }
                else
                {
                    link = feedsNode.DataKey;
                    title = feedsNode.Text;
                }

                if (!string.IsNullOrEmpty(link))
                {
                    Clipboard.SetDataObject(
                        String.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>", link, title, feedsNode.Text));
                }
            }
            if (sender is AppContextMenuCommand) // needed at the treeview
                CurrentSelectedFeedsNode = null;
        }

        private static void CmdCopyNewsItem(ICommand sender)
        {
            // dummy, just a submenu
        }

        private void CmdCopyNewsItemLinkToClipboard(ICommand sender)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            StringBuilder data = new StringBuilder();
            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                INewsItem item = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[i]).Key;

                if (item != null)
                {
                    string link = item.Link;
                    if (!string.IsNullOrEmpty(link))
                    {
                        data.AppendFormat("{0}{1}", (i > 0 ? Environment.NewLine : String.Empty), link);
                    }
                }
            }

            if (data.Length > 0)
                Clipboard.SetDataObject(data.ToString());
        }

        private void CmdCopyNewsItemTitleLinkToClipboard(ICommand sender)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            StringBuilder data = new StringBuilder();
            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                INewsItem item = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[i]).Key;

                if (item != null)
                {
                    string link = item.Link;
                    if (!string.IsNullOrEmpty(link))
                    {
                        string title = item.Title;
                        if (!string.IsNullOrEmpty(title))
                        {
                            data.AppendFormat("{0}<a href=\"{1}\" title=\"{2}\">{3}</a>",
                                              (i > 0 ? "<br />" + Environment.NewLine : String.Empty), link, title,
                                              title);
                        }
                        else
                        {
                            data.AppendFormat("{0}<a href=\"{1}\">{2}</a>",
                                              (i > 0 ? "<br />" + Environment.NewLine : String.Empty), link, link);
                        }
                    }
                }
            }

            if (data.Length > 0)
                Clipboard.SetDataObject(data.ToString());
        }

        private void CmdCopyNewsItemContentToClipboard(ICommand sender)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            StringBuilder data = new StringBuilder();
            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                INewsItem item = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[i]).Key;

                if (item != null)
                {
                    string link = item.Link;
                    string content = item.Content;
                    if (!string.IsNullOrEmpty(content))
                    {
                        data.AppendFormat("{0}{1}", (i > 0 ? "<br />" + Environment.NewLine : String.Empty), link);
                    }
                    else if (!string.IsNullOrEmpty(link))
                    {
                        string title = item.Title;
                        if (!string.IsNullOrEmpty(title))
                        {
                            data.AppendFormat("{0}<a href=\"{1}\" title=\"{2}\">{3}</a>",
                                              (i > 0 ? "<br />" + Environment.NewLine : String.Empty), link,
                                              item.Feed.title, title);
                        }
                        else
                        {
                            data.AppendFormat("{0}<a href=\"{1}\" title=\"{2}\">{3}</a>",
                                              (i > 0 ? "<br />" + Environment.NewLine : String.Empty), link,
                                              item.Feed.title, link);
                        }
                    }
                }
            }

            if (data.Length > 0)
                Clipboard.SetDataObject(data.ToString());
        }

        #endregion

        #region CmdFinder.. routines

        /// <summary>
        /// Re-runs the search and repopulates the search folder.
        /// </summary>
        /// <remarks>Assumes that this is called when the current selected node is a search folder</remarks>
        /// <param name="sender"></param>
        private void CmdRefreshFinder(ICommand sender)
        {
            EmptyListView();
            htmlDetail.Clear();
            FinderNode afn = TreeSelectedFeedsNode as FinderNode;
            if (afn != null)
            {
                afn.Clear();
                UpdateTreeNodeUnreadStatus(afn, 0);
                if (afn.Finder != null && !string.IsNullOrEmpty(afn.Finder.ExternalSearchUrl))
                {
                    // does also initiates the local search if merge is true:
                    AsyncStartRssRemoteSearch(afn.Finder.ExternalSearchPhrase, afn.Finder.ExternalSearchUrl,
                                              afn.Finder.ExternalResultMerged, true);
                }
                else
                {
                    AsyncStartNewsSearch(afn);
                }
            }
        }


        /// <summary>
        /// Marks all the items in a search folder as read
        /// </summary>
        /// <param name="sender"></param>
        private void CmdMarkFinderItemsRead(ICommand sender)
        {
            SetFeedItemsReadState(listFeedItems.Items, true);
            UpdateTreeStatus(owner.FeedHandler.GetFeeds());
        }

        /// <summary>
        /// Renames a search folder
        /// </summary>
        /// <param name="sender"></param>
        private void CmdRenameFinder(ICommand sender)
        {
            if (CurrentSelectedFeedsNode != null)
                DoEditTreeNodeLabel();
        }

        /// <summary>
        /// Allows the user to create a new search folder
        /// </summary>
        /// <param name="sender"></param>
        private void CmdNewFinder(ICommand sender)
        {
            CmdNewRssSearch(sender);
        }

        /// <summary>
        /// Deletes a search folder
        /// </summary>
        /// <param name="sender"></param>
        private void CmdDeleteFinder(ICommand sender)
        {
            if (owner.MessageQuestion(SR.MessageBoxDeleteThisFinderQuestion) == DialogResult.Yes)
            {
                if (NodeEditingActive)
                    return;

                TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
                WalkdownThenDeleteFinders(feedsNode);
                UpdateTreeNodeUnreadStatus(feedsNode, 0);

                try
                {
                    feedsNode.Parent.Nodes.Remove(feedsNode);
                }
                catch
                {
                }

                if (sender is AppContextMenuCommand)
                    CurrentSelectedFeedsNode = null;
            }
        }

        private void CmdFinderToggleExcerptsFullItemText(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode is FinderNode)
            {
                FinderNode fn = (FinderNode) feedsNode;
                fn.Finder.ShowFullItemContent = ! fn.Finder.ShowFullItemContent;
                fn.Clear();
                UpdateTreeNodeUnreadStatus(fn, 0);
                EmptyListView();
                htmlDetail.Clear();
                AsyncStartNewsSearch(fn);
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then delete all child categories and FeedNode refs in owner.FeedHandler.
        /// </summary>
        /// <param name="startNode">new full category name (long name, with all the '\').</param>
        private void WalkdownThenDeleteFinders(TreeFeedsNodeBase startNode)
        {
            if (startNode == null) return;

            if (startNode.Type == FeedNodeType.Finder)
            {
                FinderNode agn = startNode as FinderNode;
                if (agn != null)
                {
                    owner.FinderList.Remove(agn.Finder);
                }
            }
            else
            {
                // other
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    WalkdownThenDeleteFinders(child);
                }
            }
        }

        private void CmdDeleteAllFinder(ICommand sender)
        {
            if (owner.MessageQuestion(SR.MessageBoxDeleteAllFindersQuestion) == DialogResult.Yes)
            {
                owner.FinderList.Clear();
                owner.SaveSearchFolders();

                FinderRootNode finderRoot = GetRoot(RootFolderType.Finder) as FinderRootNode;

                if (finderRoot != null)
                {
                    finderRoot.Nodes.Clear();
                    UpdateTreeNodeUnreadStatus(finderRoot, 0);
                }
            }

            if (sender is AppContextMenuCommand)
                CurrentSelectedFeedsNode = null;
        }

        private void CmdShowFinderProperties(ICommand sender)
        {
            CmdDockShowRssSearch(null);

            FinderNode node = CurrentSelectedFeedsNode as FinderNode;
            if (node != null)
            {
                searchPanel.SearchDialogSetSearchCriterias(node);
            }

            if (sender is AppContextMenuCommand)
                CurrentSelectedFeedsNode = null;
        }

        private void CmdSubscribeToFinderResult(ICommand sender)
        {
            FinderNode node = CurrentSelectedFeedsNode as FinderNode;
            if (node != null && node.Finder != null)
            {
                if (!string.IsNullOrEmpty(node.Finder.ExternalSearchUrl))
                {
                    owner.CmdNewFeed(node.Text, node.Finder.ExternalSearchUrl, node.Finder.ExternalSearchPhrase);
                }
            }
        }

        #endregion

        #region CmdListview: Column Layout, selection

        internal void CmdColumnChooserUseFeedLayoutGlobal(ICommand sender)
        {
            SetGlobalFeedColumnLayout(FeedNodeType.Feed, listFeedItems.FeedColumnLayout);
            listFeedItems.ApplyLayoutModifications();
        }

        internal void CmdColumnChooserUseCategoryLayoutGlobal(ICommand sender)
        {
            SetGlobalFeedColumnLayout(FeedNodeType.Category, listFeedItems.FeedColumnLayout);
            listFeedItems.ApplyLayoutModifications();
        }

        internal void CmdColumnChooserResetToDefault(ICommand sender)
        {
            SetFeedHandlerFeedColumnLayout(CurrentSelectedFeedsNode, null);
            listFeedItems.ApplyLayoutModifications(); // do not save temp. changes to the node
            IList<INewsItem> items = NewsItemListFrom(listFeedItems.Items);
            listFeedItems.FeedColumnLayout = GetFeedColumnLayout(CurrentSelectedFeedsNode); // also clear's the listview
            RePopulateListviewWithContent(items);
        }

        private void CmdDownloadAttachment(ICommand sender)
        {
            string fileName = sender.CommandID.Split(new char[] {'<'})[1];
            INewsItem item = CurrentSelectedFeedItem;

            try
            {
                if (item != null)
                {
                    owner.FeedHandler.DownloadEnclosure(item, fileName);
                }
            }
            catch (DownloaderException de)
            {
                MessageBox.Show(de.Message, SR.ExceptionEnclosureDownloadError, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        internal void CmdToggleListviewColumn(ICommand sender)
        {
            if (listFeedItems.Columns.Count > 1)
            {
                // show at least one column
                string[] name = sender.CommandID.Split(new char[] {'.'});

                bool enable = owner.Mediator.IsChecked(sender);
                owner.Mediator.SetChecked(enable, sender.CommandID);

                if (!enable)
                {
                    listFeedItems.Columns.Remove(name[1]);
                }
                else
                {
                    AddListviewColumn(name[1], 120);                    
                    RePopulateListviewWithCurrentContent();
                }

                listFeedItems.CheckForLayoutModifications(); 
            }
        }

        private void RefreshListviewColumnContextMenu()
        {
            ColumnKeyIndexMap map = listFeedItems.Columns.GetColumnIndexMap();

            foreach (string colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                owner.Mediator.SetChecked(map.ContainsKey(colID), "cmdListviewColumn." + colID);
            }

            bool enableIndividual = (CurrentSelectedFeedsNode != null &&
                                     (CurrentSelectedFeedsNode.Type == FeedNodeType.Feed ||
                                      CurrentSelectedFeedsNode.Type == FeedNodeType.Category));
            owner.Mediator.SetEnabled(enableIndividual, "cmdColumnChooserResetToDefault");
        }

        private void AddListviewColumn(string colID, int width)
        {
            switch (colID)
            {
                case "Title":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionHeadline, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Subject":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionTopic, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Date":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionDate, typeof (DateTime), width,
                                              HorizontalAlignment.Left);
                    break;
                case "FeedTitle":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionFeedTitle, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Author":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionCreator, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "CommentCount":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionCommentCount, typeof (int), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Enclosure":
                    //TODO: should have a paperclip picture, int type may change to a specific state (string)
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionEnclosure, typeof (int), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Flag":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionFlagStatus, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                default:
                    Trace.Assert(false, "AddListviewColumn::NewsItemSortField NOT handled: " + colID);
                    break;
            }
        }

        private void ResetFeedDetailLayoutCmds()
        {
            owner.Mediator.SetChecked(false, "cmdFeedDetailLayoutPosTop", "cmdFeedDetailLayoutPosLeft",
                                      "cmdFeedDetailLayoutPosRight", "cmdFeedDetailLayoutPosBottom");
        }


        internal void CmdFeedDetailTextSizeSmallest(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Smallest);
        }

        internal void CmdFeedDetailTextSizeSmaller(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Smaller);
        }


        internal void CmdFeedDetailTextSizeMedium(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Medium);
        }


        internal void CmdFeedDetailTextSizeLarger(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Larger);
        }


        internal void CmdFeedDetailTextSizeLargest(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Largest);
        }

        private void SetFeedDetailTextSize(TextSize size)
        {
            try
            {
                int z = (int) owner.Preferences.ReadingPaneTextSize;

                switch (size)
                {
                    case TextSize.Smallest:
                        z = 0;
                        break;
                    case TextSize.Smaller:
                        z = 1;
                        break;
                    case TextSize.Medium:
                        z = 2;
                        break;
                    case TextSize.Larger:
                        z = 3;
                        break;
                    case TextSize.Largest:
                        z = 4;
                        break;
                }

                object Z = z;
                object NULL = new Object();
                htmlDetail.ExecWB(OLECMDID.OLECMDID_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, ref Z, ref NULL);

                owner.Preferences.ReadingPaneTextSize = size; 
                owner.Mediator.SetChecked(false, "cmdFeedDetailTextSizeLargest", "cmdFeedDetailTextSizeLarger",
                                            "cmdFeedDetailTextSizeMedium", "cmdFeedDetailTextSizeSmaller",
                                            "cmdFeedDetailTextSizeSmallest"); 
                owner.Mediator.SetChecked(true, "cmdFeedDetailTextSize" + owner.Preferences.ReadingPaneTextSize.ToString());
            }
            catch (Exception e)
            {
                _log.Error("Exception while changing reading pane text size", e); 
            }
        }

        private void SetFeedDetailLayout(DockStyle style)
        {
            ResetFeedDetailLayoutCmds();
            panelFeedItems.Dock = style;
            if (style == DockStyle.Left || style == DockStyle.Right)
            {
                detailsPaneSplitter.Dock = style; // allowed styles
                detailsPaneSplitter.Cursor = Cursors.VSplit;
                panelFeedItems.Width = panelFeedDetails.Width/3;
            }
            else if (style == DockStyle.Bottom || style == DockStyle.Top)
            {
                detailsPaneSplitter.Dock = style; // allowed styles
                detailsPaneSplitter.Cursor = Cursors.HSplit;
                panelFeedItems.Height = panelFeedDetails.Height/2;
            }
            // TR - just for test with dockstyle.none:
            //panelWebDetail.Visible = detailsPaneSplitter.Visible = (style != DockStyle.None);
            owner.Mediator.SetChecked(true, "cmdFeedDetailLayoutPos" + detailsPaneSplitter.Dock.ToString());
        }

        /// <summary>
        /// Select all items of the Feeds ListView.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdSelectAllNewsItems(ICommand sender)
        {
            for (int i = 0; i < listFeedItems.Items.Count; i++)
            {
                listFeedItems.Items[i].Selected = true;
            }
            listFeedItems.Select();
        }

        #endregion

        #region CmdBrowserHistoryItem commands

        private void OnHistoryNavigateGoBackItemClick(object sender, HistoryNavigationEventArgs e)
        {
            NavigateToHistoryEntry(_feedItemImpressionHistory.GetPreviousAt(e.Index));
        }

        private void OnHistoryNavigateGoForwardItemClick(object sender, HistoryNavigationEventArgs e)
        {
            NavigateToHistoryEntry(_feedItemImpressionHistory.GetNextAt(e.Index));
        }

        #endregion

        #region CmdFeed commands

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdDeleteFeed(ICommand sender)
        {
            if (NodeEditingActive)
                return;

            // right-click selected:
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;
            if (tn == null) return;
            if (tn.Type != FeedNodeType.Feed) return;

            if (DialogResult.Yes == owner.MessageQuestion(
                                        SR.MessageBoxDeleteThisFeedQuestion,
                                        String.Format(" - {0} ({1})", SR.MenuDeleteThisFeedCaption, tn.Text)))
            {
                // raise the OnFeedDeleted event (where we really remove the node):
                owner.DeleteFeed(tn.DataKey);

                if (sender is AppContextMenuCommand)
                    CurrentSelectedFeedsNode = null;


                //select next node in tree view
//				this.treeFeeds.ActiveNode.Selected = true; 
//				this.CurrentSelectedFeedsNode = this.treeFeeds.ActiveNode as TreeFeedsNodeBase;
//				this.RefreshFeedDisplay(this.CurrentSelectedFeedsNode, true); 				
            }
        }

        #endregion

        private ITabState FeedDetailTabState
        {
            get
            {
                return (ITabState) _docFeedDetails.Tag;
            }
        }

        private bool RemoveDocTab(DockControl doc)
        {
            if (doc == null)
                doc = _docContainer.ActiveDocument;

            if (doc == null)
                return false;

            ITabState state = doc.Tag as ITabState;
            if (state != null && state.CanClose)
            {
                try
                {
                    _docContainer.RemoveDocument(doc);
                    HtmlControl browser = doc.Controls[0] as HtmlControl;
                    if (browser != null)
                    {
                        browser.Tag = null; // remove ref to containing doc
                        browser.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("_docContainer.RemoveDocument(doc) caused exception", ex);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called on generic listview commands and used for calling Addin-methods. 
        /// </summary>
        /// <param name="index">Index of the command. Points directly to the
        /// plugin within the arraylist</param>
        /// <param name="hasConfig">true, if we have to call config dialog</param>
        public void OnGenericListviewCommand(int index, bool hasConfig)
        {
            IBlogExtension ibe = blogExtensions[index];
            if (hasConfig)
            {
                try
                {
                    ibe.Configure(this);
                }
                catch (Exception e)
                {
                    _log.Error("IBlogExtension configuration exception", e);
                    owner.MessageError(SR.ExceptionIBlogExtensionFunctionCall("Configure()", e.Message));
                }
            }
            else
            {
                //if (ibe.HasEditingGUI) //TODO...? What we have to do here...?
                if (CurrentSelectedFeedItem != null)
                {
                    try
                    {
                        ibe.BlogItem(CurrentSelectedFeedItem, false);
                    }
                    catch (Exception e)
                    {
                        _log.Error("IBlogExtension command exception", e);
                        owner.MessageError(SR.ExceptionIBlogExtensionFunctionCall("BlogItem()", e.Message));
                    }
                }
            }
        }

        private void OnFormHandleCreated(object sender, EventArgs e)
        {
            // if the form is started minimized (via Shortcut Properties "Run-Minimized"
            // it seems the OnLoad event does not gets fired, so we call it here...
            if (InitialStartupState == FormWindowState.Minimized)
                OnLoad(this, new EventArgs());
            // init idle task event handler:
            Application.Idle += OnApplicationIdle;
        }

        private void OnLoad(object sender, EventArgs eva)
        {
            // do not display the ugly form init/resizing...
            Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_HIDE);

            _uiTasksTimer.Tick += new EventHandler(OnTasksTimerTick);

//			InitDrawFilters();

            LoadUIConfiguration();
            SetTitleText(String.Empty);
            SetDetailHeaderText(null);

            InitSearchEngines();
            CheckForAddIns();
            InitFilter();

            SetGuiStateFeedback(SR.GUIStatusLoadingFeedlist);

            //IdleTask.AddTask(IdleTasks.InitOnFinishLoading);
            DelayTask(DelayedTasks.InitOnFinishLoading);
        }

        /// <summary>
        /// Provide the entry point to the delayed loading of the feed list
        /// </summary>
        private void OnFinishLoading()
        {
            if (owner.CommandLineArgs.StartInTaskbarNotificationAreaOnly || SystemTrayOnlyVisible)
            {
                // forced to show in Taskbar Notification Area
                Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_HIDE);
            }
            else
            {
                Activate();
            }

            Splash.Status = SR.GUIStatusRefreshConnectionState;
            // refresh the Offline menu entry checked state
            owner.UpdateInternetConnectionState();

            // refresh the internal browser component, that does not know immediatly
            // about a still existing Offline Mode...
            Utils.SetIEOffline(owner.InternetConnectionOffline);


            owner.CmdCheckForUpdates(AutoUpdateMode.OnApplicationStart);

            RssBanditApplication.CheckAndInitSoundEvents();
            owner.BeginLoadingFeedlist();
            owner.BeginLoadingSpecialFeeds();

            Splash.Close();
            owner.AskAndCheckForDefaultAggregator();

            //Trace.WriteLine("ATTENTION!. REFRESH TIMER DISABLED FOR DEBUGGING!");
#if !NOAUTO_REFRESH
            // start the refresh timers
            _timerRefreshFeeds.Start();
            _timerRefreshCommentFeeds.Start();
#endif
        }

        private void OnFormMove(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _formRestoreBounds.Location = Location;
            }
        }

        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _formRestoreBounds.Size = Size;
            }
            if (WindowState != FormWindowState.Minimized)
            {
                // adjust the MaximumSize of the dock hosts:
                /*leftSandDock.MaximumSize = */
                rightSandDock.MaximumSize = ClientSize.Width - 20;
                topSandDock.MaximumSize = bottomSandDock.MaximumSize = ClientSize.Height - 20;
            }
            if (Visible)
            {
                SystemTrayOnlyVisible = false;
            }
        }

        /// <summary>
        /// Here is the Form minimize event handler
        /// </summary>
        /// <param name="sender">This form</param>
        /// <param name="e">Empty. See WndProc()</param>
        private void OnFormMinimize(object sender, EventArgs e)
        {
            if (owner.Preferences.HideToTrayAction == HideToTray.OnMinimize)
            {
                HideToSystemTray();
            }
        }


        /// <summary>
        /// Implements the IMessageFilter. 
        /// Helps grabbing all the important keys.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public virtual bool PreFilterMessage(ref Message m)
        {
            bool processed = false;

            try
            {
                if (m.Msg == (int) Win32.Message.WM_KEYDOWN ||
                    m.Msg == (int) Win32.Message.WM_SYSKEYDOWN)
                {
                    Keys msgKey = ((Keys) (int) m.WParam & Keys.KeyCode);
#if DEBUG
                    if (msgKey == Keys.F12)
                    {
                        IdleTask.AddTask(IdleTasks.IndexAllItems);

                        // to test tray animation:
                        //this._trayManager.SetState(ApplicationTrayState.NewUnreadFeedsReceived);
                    }
                    else 
#endif
                        if ((ModifierKeys == Keys.Alt) && msgKey == Keys.F4)
                        {
                            if (owner.Preferences.HideToTrayAction == HideToTray.OnClose &&
                                _forceShutdown == false)
                            {
                                processed = true;
                                HideToSystemTray();
                            }
                        }
                        else if (msgKey == Keys.Tab)
                        {
                            if (ModifierKeys == 0)
                            {
                                // normal Tab navigation between controls

                                Trace.WriteLine("PreFilterMessage[Tab Only], ");

                                if (treeFeeds.Visible)
                                {
                                    if (treeFeeds.Focused)
                                    {
                                        if (listFeedItems.Visible)
                                        {
                                            listFeedItems.Focus();
                                            if (listFeedItems.Items.Count > 0 && listFeedItems.SelectedItems.Count == 0)
                                            {
                                                listFeedItems.Items[0].Selected = true;
                                                listFeedItems.Items[0].Focused = true;
                                                OnFeedListItemActivate(this, new EventArgs());
                                            }
                                            processed = true;
                                        }
                                        else if (_docContainer.ActiveDocument != _docFeedDetails)
                                        {
                                            // a tabbed browser should get focus
                                            SetFocus2WebBrowser((HtmlControl) _docContainer.ActiveDocument.Controls[0]);
                                            processed = true;
                                        }
                                    }
                                    else if (listFeedItems.Focused)
                                    {
                                        SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                        processed = true;
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                    // treefeeds.invisible:
                                    if (listFeedItems.Visible)
                                    {
                                        if (listFeedItems.Focused)
                                        {
                                            SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                            processed = true;
                                        }
                                        else
                                        {
                                            // a IE browser focused
                                            Trace.WriteLine("PreFilterMessage[Tab Only] IE browser focused?" +
                                                            htmlDetail.Focused);
                                        }
                                    }
                                } // endif treefeeds.visible 
                            }
                            else if ((ModifierKeys & Keys.Shift) == Keys.Shift &&
                                     (ModifierKeys & Keys.Control) == 0)
                            {
                                // Shift-Tab only

                                Trace.WriteLine("PreFilterMessage[Shift-Tab Only]");
                                if (treeFeeds.Visible)
                                {
                                    if (treeFeeds.Focused)
                                    {
                                        if (listFeedItems.Visible)
                                        {
                                            SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                            processed = true;
                                        }
                                        else if (_docContainer.ActiveDocument != _docFeedDetails)
                                        {
                                            // a tabbed browser should get focus
                                            SetFocus2WebBrowser((HtmlControl) _docContainer.ActiveDocument.Controls[0]);
                                            processed = true;
                                        }
                                    }
                                    else if (listFeedItems.Focused)
                                    {
                                        treeFeeds.Focus();
                                        processed = true;
                                    }
                                    else
                                    {
                                        // a IE browser focused
                                        Trace.WriteLine("PreFilterMessage[Shift-Tab Only] IE browser focused?" +
                                                        htmlDetail.Focused);
                              
                                    }
                                }
                                else
                                {
                                    // treefeeds.invisible:
                                    if (listFeedItems.Visible)
                                    {
                                        if (listFeedItems.Focused)
                                        {
                                            SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                            processed = true;
                                        }
                                        else
                                        {
              
                                        }
                                    }
                                } //endif treefeeds.visible
                            }
                        }
                        else if (listFeedItems.Focused &&
                                 _shortcutHandler.IsCommandInvoked("ExpandListViewItem", m.WParam))
                        {
                            // "+" on ListView opens the thread
                            if (listFeedItems.Visible && listFeedItems.SelectedItems.Count > 0)
                            {
                                ThreadedListViewItem lvi = (ThreadedListViewItem) listFeedItems.SelectedItems[0];
                                if (lvi.HasChilds && lvi.Collapsed)
                                {
                                    lvi.Expanded = true;
                                    processed = true;
                                }
                            }
                        }
                        else if (listFeedItems.Focused &&
                                 _shortcutHandler.IsCommandInvoked("CollapseListViewItem", m.WParam))
                        {
                            // "-" on ListView close the thread
                            if (listFeedItems.Visible && listFeedItems.SelectedItems.Count > 0)
                            {
                                ThreadedListViewItem lvi = (ThreadedListViewItem) listFeedItems.SelectedItems[0];
                                if (lvi.HasChilds && lvi.Expanded)
                                {
                                    lvi.Collapsed = true;
                                    processed = true;
                                }
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("RemoveDocTab", m.WParam))
                        {
                            // Ctrl-F4: close a tab
                            if (RemoveDocTab(_docContainer.ActiveDocument))
                            {
                                processed = true;
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("CatchUpCurrentSelectedNode", m.WParam))
                        {
                            // Ctrl-Q: Catch up feed
                            owner.CmdCatchUpCurrentSelectedNode(null);
                            processed = true;
                        }
                        else if (_shortcutHandler.IsCommandInvoked("MarkFeedItemsUnread", m.WParam))
                        {
                            // Ctrl-U: close a tab
                            owner.CmdMarkFeedItemsUnread(null);
                            processed = true;

                            //We've hard-coded SPACE as a Move to Next Item
                            //But in that case, make sure there's not a modifier key pressed.
                        }
                        else if ((msgKey == Keys.Space && ModifierKeys == 0) ||
                                 _shortcutHandler.IsCommandInvoked("MoveToNextUnread", m.WParam))
                        {
                            // Space: move to next unread

                            if (listFeedItems.Focused || treeFeeds.Focused &&
                                                         !(TreeSelectedFeedsNode != null &&
                                                           TreeSelectedFeedsNode.IsEditing))
                            {
                                MoveToNextUnreadItem();
                                processed = true;
                            }
                            else if (searchPanel.ContainsFocus || treeFeeds.Focused ||
                                     UrlComboBox.Focused || SearchComboBox.Focused ||
                                     (CurrentSelectedFeedsNode != null && CurrentSelectedFeedsNode.IsEditing))
                            {
                                // ignore
                            }
                            else if (_docContainer.ActiveDocument == _docFeedDetails &&
                                     !listFeedItems.Focused)
                            {
                                // browser detail pane has focus
                                //Trace.WriteLine("htmlDetail.Focused:"+htmlDetail.Focused);
                                IHTMLDocument2 htdoc = htmlDetail.Document2;
                                if (htdoc != null)
                                {
                                    IHTMLElement2 htbody = htdoc.GetBody();
                                    if (htbody != null)
                                    {
                                        int num1 = htbody.getScrollTop();
                                        htbody.setScrollTop(num1 + 20);
                                        int num2 = htbody.getScrollTop();
                                        if (num1 == num2)
                                        {
                                            MoveToNextUnreadItem();
                                            processed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // ignore, control should handle it
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("InitiateRenameFeedOrCategory", m.WParam))
                        {
                            // rename within treeview
                            if (treeFeeds.Focused)
                            {
                                InitiateRenameFeedOrCategory();
                                processed = true;
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("UpdateFeed", m.WParam))
                        {
                            // F5: UpdateFeed()
                            CurrentSelectedFeedsNode = null;
                            owner.CmdUpdateFeed(null);
                            processed = true;
                        }
                        else if (_shortcutHandler.IsCommandInvoked("GiveFocusToUrlTextBox", m.WParam))
                        {
                            // Alt+F4 or F11: move focus to Url textbox
                            UrlComboBox.Focus();
                            processed = true;
                        }
                        else if (_shortcutHandler.IsCommandInvoked("GiveFocusToSearchTextBox", m.WParam))
                        {
                            // F12: move focus to Search textbox
                            SearchComboBox.Focus();
                            processed = true;
                        }
                        else if ((msgKey == Keys.Delete && ModifierKeys == 0) ||
                                 _shortcutHandler.IsCommandInvoked("DeleteItem", m.WParam))
                        {
                            // Delete a feed or category,...
                            // cannot be a shortcut, because then "Del" does not work when edit/rename a node caption :-(
                            // But we can add alternate shortcuts via the config file.
                            if (treeFeeds.Focused && TreeSelectedFeedsNode != null &&
                                !TreeSelectedFeedsNode.IsEditing)
                            {
                                TreeFeedsNodeBase root = GetRoot(RootFolderType.MyFeeds);
                                TreeFeedsNodeBase current = CurrentSelectedFeedsNode;
                                if (NodeIsChildOf(current, root))
                                {
                                    if (current.Type == FeedNodeType.Category)
                                    {
                                        owner.CmdDeleteCategory(null);
                                        processed = true;
                                    }
                                    if (current.Type == FeedNodeType.Feed)
                                    {
                                        CmdDeleteFeed(null);
                                        processed = true;
                                    }
                                }
                            }
                        }
                }
                else if (m.Msg == (int) Win32.Message.WM_LBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_RBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_MBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_LBUTTONUP ||
                         m.Msg == (int) Win32.Message.WM_MBUTTONUP ||
                         m.Msg == (int) Win32.Message.WM_RBUTTONUP ||
                         m.Msg == (int) Win32.Message.WM_XBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_XBUTTONUP)
                {
                    _lastMousePosition = new Point(Win32.LOWORD(m.LParam), Win32.HIWORD(m.LParam));

                    Control mouseControl = wheelSupport.GetTopmostChild(this, MousePosition);
                    _webUserNavigated = (mouseControl is HtmlControl); // set
                    _webForceNewTab = false;
                    if (_webUserNavigated)
                    {
                        // CONTROL-Click opens a new Tab
                        _webForceNewTab = (Interop.GetAsyncKeyState(Interop.VK_CONTROL) < 0);
                    }
                }
                else if (m.Msg == (int) Win32.Message.WM_MOUSEMOVE)
                {
                    Point p = new Point(Win32.LOWORD(m.LParam), Win32.HIWORD(m.LParam));
                    if (Math.Abs(p.X - _lastMousePosition.X) > 5 ||
                        Math.Abs(p.Y - _lastMousePosition.Y) > 5)
                    {
                        //Trace.WriteLine(String.Format("Reset mouse pos. Old: {0} New: {1}", _lastMousePosition, p));
                        _webForceNewTab = _webUserNavigated = false; // reset
                        _lastMousePosition = p;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("PreFilterMessage() failed", ex);
            }

#if TRACE_WIN_MESSAGES				
			if (m.Msg != (int)Win32.Message.WM_TIMER &&
				m.Msg != (int)Win32.Message.WM_MOUSEMOVE)
				Debug.WriteLine("PreFilterMessage(" + m +") handled: "+ processed);
#endif

            return processed;
        }

        /// <summary>
        /// we are interested in an OnMinimized event
        /// </summary>
        /// <param name="m">Native window message</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == (int) Win32.Message.WM_SIZE)
                {
                    if (((int) m.WParam) == 1 /*SIZE_MINIMIZED*/&& OnMinimize != null)
                    {
                        OnMinimize(this, EventArgs.Empty);
                    }
                    //			} else if (m.Msg == (int)Win32.Message.WM_MOUSEMOVE) {
                    //				Control ctrl =  this.GetChildAtPoint(this.PointToClient(MousePosition));
                    //				if (ctrl != null && !ctrl.Focused && ctrl.CanFocus) {
                    //					ctrl.Focus();
                    //				}
                }
                else if ( /* m.Msg == (int)WM_CLOSE || */ m.Msg == (int) Win32.Message.WM_QUERYENDSESSION ||
                                                          m.Msg == (int) Win32.Message.WM_ENDSESSION)
                {
                    // This is here to deal with dealing with system shutdown issues
                    // Read http://www.kuro5hin.org/story/2003/4/17/22853/6087#banditshutdown for details
                    // FYI: you could also do so:
                    // Microsoft.Win32.SystemEvents.SessionEnding += new SessionEndingEventHandler(this.OnSessionEnding);
                    // but we already have the WndProc(), so we also handle this message here

                    _forceShutdown = true; // the closing handler ask for that now
                    //this.SaveUIConfiguration(true);
                    owner.SaveApplicationState(true);
                }
                else if (m.Msg == (int) Win32.Message.WM_CLOSE &&
                         owner.Preferences.HideToTrayAction != HideToTray.OnClose)
                {
                    _forceShutdown = true; // the closing handler ask for that now
                    //this.SaveUIConfiguration(true);
                    owner.SaveApplicationState(true);
                }

                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                _log.Fatal("WndProc() failed with an exception", ex);
            }
        }

        private void OnFormMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
                RequestBrowseAction(BrowseAction.NavigateBack);
            else if (e.Button == MouseButtons.XButton2)
                RequestBrowseAction(BrowseAction.NavigateForward);
        }

        private void OnFormActivated(object sender, EventArgs e)
        {
            Application.AddMessageFilter(this);
            KeyPreview = true;
        }

        private void OnFormDeactivate(object sender, EventArgs e)
        {
            Application.RemoveMessageFilter(this);
            KeyPreview = false;
        }

        private void OnDocContainerShowControlContextMenu(object sender, ShowControlContextMenuEventArgs e)
        {
            _contextMenuCalledAt = Cursor.Position;
            _docTabContextMenu.Show(_docContainer, e.Position);
        }

        private void OnDocContainerMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (_docContainer.Visible)
                {
                    // we can only displ. the context menu on a visible control:
                    _contextMenuCalledAt = Cursor.Position;
                    _docTabContextMenu.Show(_docContainer, new Point(e.X, e.Y));
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                OnDocContainerDoubleClick(sender, e);
            }
        }

        private void OnDocContainerDocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            RemoveDocTab(e.DockControl);
        }

        private void OnDocContainerActiveDocumentChanged(object sender, ActiveDocumentEventArgs e)
        {
            RefreshDocumentState(e.NewActiveDocument);
            DeactivateWebProgressInfo();
        }

        private void OnDocContainerDoubleClick(object sender, EventArgs e)
        {
            Point p = _docContainer.PointToClient(MousePosition);
            DocumentLayoutSystem lb = (DocumentLayoutSystem) _docContainer.GetLayoutSystemAt(p);
            if (lb != null)
            {
                DockControl doc = lb.GetControlAt(p);
                if (doc != null)
                    RemoveDocTab(doc);
            }
        }

        /// <summary>
        /// GUI State persistence. Settings are: window position, splitter position, 
        /// floating window sizes, listview column order, sorting direction etc.
        /// This routine writes all of them to a centralized settings dictionary maintained
        /// by the Settings class.
        /// </summary>
        /// <param name="writer"></param>
        protected void OnSaveConfig(Settings writer)
        {
            try
            {
                //NOTE: if we are here, consider that control state is not always
                // correct (at least the .Visible state can be wrong in case Bandit
                // was closed from the system tray icon - and the main form was not 
                // displayed)

                writer.SetProperty("version", 4);

                writer.SetProperty(Name + "/Bounds", BoundsToString(_formRestoreBounds));
                writer.SetProperty(Name + "/WindowState", (int) WindowState);

                // splitter position Listview/Detail Pane: 
                writer.SetProperty(Name + "/panelFeedItems.Height", panelFeedItems.Height);
                writer.SetProperty(Name + "/panelFeedItems.Width", panelFeedItems.Width);

                // splitter position Navigator/Feed Details Pane: 
                writer.SetProperty(Name + "/Navigator.Width", Navigator.Width);

                writer.SetProperty(Name + "/docManager.WindowAlignment", (int) _docContainer.LayoutSystem.SplitMode);

                Office2003Renderer sdRenderer = sandDockManager.Renderer as Office2003Renderer;
                writer.SetProperty(Name + "/dockManager.LayoutStyle.Office2003", (sdRenderer != null));

                // workaround the issue described here: http://www.divil.co.uk/net/support/kb/article.aspx?id=14
                //using (new CultureChanger("en-US")) 
                using (CultureChanger.InvariantCulture) //TR: SF bug 1532164
                {
                    writer.SetProperty(Name + "/dockManager.LayoutInfo", sandDockManager.GetLayout());
                }

                writer.SetProperty(Name + "/ToolbarsVersion", CurrentToolbarsVersion);
                writer.SetProperty(Name + "/Toolbars",
                                   StateSerializationHelper.SaveControlStateToString(ultraToolbarsManager, true));

#if USE_USE_UltraDockManager				
				writer.SetProperty(Name+"/DockingVersion", CurrentDockingVersion);
				writer.SetProperty(Name+"/Docks", StateSerializationHelper.SaveControlStateToString(this.ultraDockManager));
#endif
                writer.SetProperty(Name + "/ExplorerBarVersion", CurrentExplorerBarVersion);
                StateSerializationHelper.SaveExplorerBar(Navigator, writer, Name + "/" + Navigator.Name);
                writer.SetProperty(Name + "/" + Navigator.Name + "/Visible",
                                   owner.Mediator.IsChecked("cmdToggleTreeViewState") ||
                                   owner.Mediator.IsChecked("cmdToggleRssSearchTabState")
                    );

                writer.SetProperty(Name + "/feedDetail.LayoutInfo.Position", (int) detailsPaneSplitter.Dock);
                writer.SetProperty(Name + "/outlookView.Visible",
                                   owner.Mediator.IsChecked("cmdViewOutlookReadingPane")
                    );
            }
            catch (Exception ex)
            {
                _log.Error("Exception while writing config entries to .settings.xml", ex);
            }
        }

        /// <summary>
        /// GUI State persistence. Restore the control settings like window position,
        /// docked window states, toolbar button layout etc.
        /// </summary>
        /// <param name="reader"></param>
        protected void OnLoadConfig(Settings reader)
        {
            // do not init from stored settings on cmd line reset:
            if (owner.CommandLineArgs.ResetUserInterface)
                return;

            try
            {
                // controls if we should load layouts from user store, or not
                // version will/have to raise, if new toolbars/buttons are available in
                // newer delivered version(s)
                //int version = (int) reader.GetProperty("version", 0, typeof (int));

                // read BEFORE set the WindowState or Bounds (that causes events, where we reset this setting to false)
                //_initialStartupTrayVisibleOnly = reader.GetBoolean(Name+"/TrayOnly.Visible", false);

                Rectangle r = StringToBounds(reader.GetString(Name + "/Bounds", BoundsToString(Bounds)));
                if (r != Rectangle.Empty)
                {
                    if (Screen.AllScreens.Length < 2)
                    {
                        // if only one sreen, correct initial location to fit the screen
                        if (r.X < 0) r.X = 0;
                        if (r.Y < 0) r.Y = 0;
                        if (r.X >= Screen.PrimaryScreen.WorkingArea.Width)
                            r.X -= Screen.PrimaryScreen.WorkingArea.Width;
                        if (r.Y >= Screen.PrimaryScreen.WorkingArea.Height)
                            r.Y -= Screen.PrimaryScreen.WorkingArea.Height;
                    }
                    _formRestoreBounds = r;
                    SetBounds(r.X, r.Y, r.Width, r.Height, BoundsSpecified.All);
                }

                FormWindowState windowState = (FormWindowState) reader.GetInt32(Name + "/WindowState",
                                                                                (int) WindowState);

                if (InitialStartupState != FormWindowState.Normal &&
                    WindowState != InitialStartupState)
                {
                    WindowState = InitialStartupState;
                }
                else
                {
                    WindowState = windowState;
                }

                DockStyle feedDetailLayout =
                    (DockStyle) reader.GetInt32(Name + "/feedDetail.LayoutInfo.Position", (int) DockStyle.Top);
                if (feedDetailLayout != DockStyle.Top && feedDetailLayout != DockStyle.Left &&
                    feedDetailLayout != DockStyle.Right && feedDetailLayout != DockStyle.Bottom)
                    feedDetailLayout = DockStyle.Top;
                SetFeedDetailLayout(feedDetailLayout); // load before restore panelFeedItems dimensions!

                // splitter position Listview/Detail Pane: 
                panelFeedItems.Height = reader.GetInt32(Name + "/panelFeedItems.Height", (panelFeedDetails.Height/2));
                panelFeedItems.Width = reader.GetInt32(Name + "/panelFeedItems.Width", (panelFeedDetails.Width/2));

                // splitter position Navigator/Feed Details Pane: 
                Navigator.Width = reader.GetInt32(Name + "/Navigator.Width", Navigator.Width);

                _docContainer.LayoutSystem.SplitMode =
                    (Orientation)
                    reader.GetInt32(Name + "/docManager.WindowAlignment", (int) _docContainer.LayoutSystem.SplitMode);
                owner.Mediator.SetChecked((_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal),
                                          "cmdDocTabLayoutHorizontal");

                // fallback layouts if something really goes wrong while laading:

                // workaround the issue described here: http://www.divil.co.uk/net/support/kb/article.aspx?id=14
                using (CultureChanger.InvariantCulture)
                {
                    //TR: SF bug 1532164

                    string fallbackSDM = sandDockManager.GetLayout();

                    try
                    {
                        sandDockManager.SetLayout(reader.GetString(Name + "/dockManager.LayoutInfo", fallbackSDM));
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Exception on restore sandDockManager layout", ex);
                        sandDockManager.SetLayout(fallbackSDM);
                    }
                }

                bool office2003 = reader.GetBoolean(Name + "/dockManager.LayoutStyle.Office2003", true);
                if (office2003)
                {
                    Office2003Renderer sdRenderer = new Office2003Renderer();
                    sdRenderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Automatic;
                    if (!RssBanditApplication.AutomaticColorSchemes)
                    {
                        sdRenderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Standard;
                    }
                    sandDockManager.Renderer = sdRenderer;
                    _docContainer.Renderer = sdRenderer;
                }
                else
                {
                    WhidbeyRenderer sdRenderer = new WhidbeyRenderer();
                    sandDockManager.Renderer = sdRenderer;
                    _docContainer.Renderer = sdRenderer;
                }

                if (reader.GetString(Name + "/ExplorerBarVersion", "0") == CurrentExplorerBarVersion)
                {
                    StateSerializationHelper.LoadExplorerBar(Navigator, reader, Name + "/" + Navigator.Name);
                }

                if (! reader.GetBoolean(Name + "/" + Navigator.Name + "/Visible", true))
                {
                    OnNavigatorCollapseClick(this, EventArgs.Empty);
                }
                // remembering/startup with search panel is not a good app start UI state: 
                if (Navigator.Visible)
                    ToggleNavigationPaneView(NavigationPaneView.Subscriptions);

                if (reader.GetString(Name + "/ToolbarsVersion", "0") == CurrentToolbarsVersion)
                {
                    // Mediator re-connects to loaded commands:
                    StateSerializationHelper.LoadControlStateFromString(
                        ultraToolbarsManager, reader.GetString(Name + "/Toolbars", null),
                        owner.Mediator);

                    // restore container control references:
                    ultraToolbarsManager.Tools["cmdUrlDropdownContainer"].Control = UrlComboBox;
                    ultraToolbarsManager.Tools["cmdSearchDropdownContainer"].Control = SearchComboBox;

                    // restore the other dynamic menu handlers:
                    historyMenuManager.SetControls(
                        (AppPopupMenuCommand) ultraToolbarsManager.Tools["cmdBrowserGoBack"],
                        (AppPopupMenuCommand) ultraToolbarsManager.Tools["cmdBrowserGoForward"]);
                    owner.BackgroundDiscoverFeedsHandler.SetControls(
                        (AppPopupMenuCommand) ultraToolbarsManager.Tools["cmdDiscoveredFeedsDropDown"],
                        (AppButtonToolCommand) ultraToolbarsManager.Tools["cmdDiscoveredFeedsListClear"]);
                    InitSearchEngines();
                }

#if USE_USE_UltraDockManager				
				if (reader.GetString(Name+"/DockingVersion", "0") == CurrentDockingVersion) {
					StateSerializationHelper.LoadControlStateFromString(this.ultraDockManager, reader.GetString(Name+"/Docks", null));
				}
#endif
                //View Outlook View Reading Pane
                bool outlookView = reader.GetBoolean(Name + "/outlookView.Visible", false);
                owner.Mediator.SetChecked(outlookView, "cmdViewOutlookReadingPane");
                ShowOutlookReadingPane(outlookView);

                // now we can change the tool states:
                owner.Mediator.SetEnabled(
                    SearchIndexBehavior.NoIndexing != owner.FeedHandler.Configuration.SearchIndexBehavior,
                    "cmdNewRssSearch",
                    "cmdToggleRssSearchTabState");
            }
            catch (Exception ex)
            {
                _log.Error("Exception while loading .settings.xml", ex);
            }
        }


        private void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (owner.Preferences.HideToTrayAction == HideToTray.OnClose &&
                _forceShutdown == false)
            {
                e.Cancel = true;
                HideToSystemTray();
            }
            else
            {
                _trayAni.Visible = false;
                toastNotifier.Dispose();
				_uiTasksTimer.Stop();
                SaveUIConfiguration(true);
            }
        }

        private bool SystemTrayOnlyVisible
        {
            get
            {
                return owner.GuiSettings.GetBoolean(Name + "/TrayOnly.Visible", false);
            }
            set
            {
                owner.GuiSettings.SetProperty(Name + "/TrayOnly.Visible", value);
            }
        }

        private void HideToSystemTray()
        {
            Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_HIDE);
            if (WindowState != FormWindowState.Minimized)
                WindowState = FormWindowState.Minimized;
            SystemTrayOnlyVisible = true;
        }

        private void RestoreFromSystemTray()
        {
            Show();
            Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_RESTORE);
            Win32.SetForegroundWindow(Handle);
            SystemTrayOnlyVisible = false;

            //if application was launced in SystemTrayOnlyVisible mode then we have to wait 
            //until now to restore browser tab state. 
            if (!_browserTabsRestored)
            {
                LoadAndRestoreBrowserTabState();
            }
        }

        internal void DoShow()
        {
            if (Visible)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_RESTORE);
                    Win32.SetForegroundWindow(Handle);
                }
                else
                {
                    Activate();
                }
            }
            else
            {
                RestoreFromSystemTray();
            }
        }

        private void LoadUIConfiguration()
        {
            try
            {
                OnLoadConfig(owner.GuiSettings);
            }
            catch (Exception ex)
            {
                _log.Error("Load .settings.xml failed", ex);
            }
        }

        /// <summary>
        /// Called to build and re-build the search engine's Gui representation(s)
        /// </summary>
        public void InitSearchEngines()
        {
            if (!owner.SearchEngineHandler.EnginesLoaded || !owner.SearchEngineHandler.EnginesOK)
                owner.LoadSearchEngines();
            toolbarHelper.BuildSearchMenuDropdown(owner.SearchEngineHandler.Engines,
                                                  owner.Mediator, CmdExecuteSearchEngine);
            owner.Mediator.SetEnabled(owner.SearchEngineHandler.Engines.Count > 0, "cmdSearchGo");
        }

        /// <summary>
        /// Iterates through the treeview and highlights all feed titles that 
        /// have unread messages. 
        /// </summary>
        private void UpdateTreeStatus(IDictionary<string, INewsFeed> feedsTable)
        {
            UpdateTreeStatus(feedsTable, RootFolderType.MyFeeds);
        }

        private void UpdateTreeStatus(IDictionary<string, INewsFeed> feedsTable, RootFolderType rootFolder)
        {
            if (feedsTable == null) return;
            if (feedsTable.Count == 0) return;

            TreeFeedsNodeBase root = GetRoot(rootFolder);

            if (root == null) // no root nodes
                return;

            // traverse driven by feedsTable. Usually the feeds count with
            // new messages should be smaller than the tree nodes count.
            foreach (NewsFeed f in feedsTable.Values)
            {
                TreeFeedsNodeBase tn = TreeHelper.FindNode(root, f);
                if (f.containsNewMessages)
                {
                    UpdateTreeNodeUnreadStatus(tn, CountUnreadFeedItems(f));
                }
                else
                {
                    UpdateTreeNodeUnreadStatus(tn, 0);
                }
                Application.DoEvents(); //??
            }
        }

        #endregion

        #region event handlers for widgets not implementing ICommand

        private void OnTreeFeedMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                UltraTree tv = (UltraTree) sender;
                TreeFeedsNodeBase selectedNode = (TreeFeedsNodeBase) tv.GetNodeFromPoint(e.X, e.Y);

                if (e.Button == MouseButtons.Right)
                {
                    //if the right click was on a treeview node then display the 
                    //appropriate context node depending on whether it was over 
                    //a feed node, category node or the top-level node. 
                    if (selectedNode != null)
                    {
                        selectedNode.UpdateContextMenu();
                        // refresh context menu items
                        RefreshTreeFeedContextMenus(selectedNode);
                    }
                    else
                    {
                        tv.ContextMenu = null; // no context menu
                    }
                    CurrentSelectedFeedsNode = selectedNode;
                }
                else
                {
                    // cleanup temp node ref., needed if a user dismiss the context menu
                    // without selecting an action
                    if ((CurrentSelectedFeedsNode != null) && (selectedNode != null))
                    {
                        //this handles left click of currently selected feed after selecting
                        //an item in the listview. For some reason no afterselect or beforeselect
                        //events are fired so we do the work here. 
                        if (ReferenceEquals(CurrentSelectedFeedsNode, selectedNode))
                        {
                            // one more test, to prevent duplicate timeconsuming population of the listview/detail pane:
                            if (selectedNode.Type == FeedNodeType.Feed)
                            {
                                // if a feed was selected in the treeview, we display the feed homepage,
                                // not the feed url in the Url dropdown box:
                                IFeedDetails fi = owner.GetFeedDetails(selectedNode.DataKey);
                                if (fi != null && fi.Link == FeedDetailTabState.Url)
                                    return; // no user navigation happened in listview/detail pane
                            }
                            else
                            {
                                // other node types does not set the FeedDetailTabState.Url
                                if (string.IsNullOrEmpty(FeedDetailTabState.Url))
                                    return;
                            }

                            OnTreeFeedAfterSelectManually(selectedNode);
//							this.listFeedItems.SelectedItems.Clear();
//							this.htmlDetail.Clear();
//							MoveFeedDetailsToFront();
//							this.AddHistoryEntry(selectedNode, null);
//							RefreshFeedDisplay(selectedNode, false);
                        }
                        else
                        {
                            CurrentSelectedFeedsNode = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected exception in OnTreeFeedMouseDown()", ex);
            }
        }

        private void OnTreeFeedMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                TreeFeedsNodeBase t = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(e.X, e.Y);
                //TR: perf. impact:
//				if (t == null) 
//					treeFeeds.Cursor = Cursors.Default;
//				else
//					treeFeeds.Cursor = Cursors.Hand;
                if (CurrentDragNode != null)
                {
                    // this code does not have any effect :-((
                    // working around the missing DragHighlight property of the treeview :-(
                    if (t == null)
                        CurrentDragHighlightNode = null;

                    if (t != null)
                    {
                        if (t.Type == FeedNodeType.Feed)
                            CurrentDragHighlightNode = t.Parent;
                        else
                            CurrentDragHighlightNode = t;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected exception in OnTreeFeedMouseMove()", ex);
            }
        }

        private void OnTreeFeedMouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                CurrentDragHighlightNode = CurrentDragNode = null;

                if (e.Button == MouseButtons.Left)
                {
                    UltraTree tv = (UltraTree) sender;
                    TreeFeedsNodeBase selectedNode = (TreeFeedsNodeBase) tv.GetNodeFromPoint(e.X, e.Y);

                    if (selectedNode != null && TreeSelectedFeedsNode == selectedNode)
                    {
                        SetTitleText(selectedNode.Text);
                        SetDetailHeaderText(selectedNode);
                        MoveFeedDetailsToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected exception in OnTreeFeedMouseUp()", ex);
            }
        }

        private void EmptyListView()
        {
            //lock(listFeedItems){
            if (listFeedItems.Items.Count > 0)
            {
                listFeedItems.BeginUpdate();
                listFeedItems.ListViewItemSorter = null;
                listFeedItems.Items.Clear();
                listFeedItems.EndUpdate();

                listFeedItemsO.Clear();
            }
            owner.Mediator.SetEnabled("-cmdFeedItemPostReply");
            //}
        }


        private void OnTreeFeedDoubleClick(object sender, EventArgs e)
        {
            try
            {
                Point point = treeFeeds.PointToClient(MousePosition);
                TreeFeedsNodeBase feedsNode = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(point);

                if (feedsNode != null)
                {
                    CurrentSelectedFeedsNode = feedsNode;
                    owner.CmdNavigateFeedHome((ICommand) null);
                    CurrentSelectedFeedsNode = null;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected Error in OnTreeFeedDoubleClick()", ex);
            }
        }


        private void OnTreeFeedBeforeSelect(object sender, BeforeSelectEventArgs e)
        {
            if (treeFeeds.SelectedNodes.Count == 0 || e.NewSelections.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            if (ReferenceEquals(treeFeeds.SelectedNodes[0], e.NewSelections[0]))
            {
                return;
            }

            if (TreeSelectedFeedsNode != null)
            {
                listFeedItems.CheckForLayoutModifications();
                TreeFeedsNodeBase tn = TreeSelectedFeedsNode;

                if (tn.Type == FeedNodeType.Category)
                {
                    string category = tn.CategoryStoreName;

                    if (owner.FeedHandler.GetCategoryMarkItemsReadOnExit(category) &&
                        !TreeHelper.IsChildNode(tn, (TreeFeedsNodeBase) e.NewSelections[0]))
                    {
                        MarkSelectedNodeRead(tn);
                        owner.SubscriptionModified(NewsFeedProperty.FeedItemReadState);
                        //owner.FeedlistModified = true;
                    }
                }
                else if (tn.Type == FeedNodeType.Feed)
                {
                    string feedUrl = tn.DataKey;
                    INewsFeed f = owner.GetFeed(feedUrl);

                    if (f != null && feedUrl != null && owner.FeedHandler.GetMarkItemsReadOnExit(feedUrl) &&
                        f.containsNewMessages)
                    {
                        MarkSelectedNodeRead(tn);
                        owner.SubscriptionModified(NewsFeedProperty.FeedItemReadState);
                        //owner.FeedlistModified = true;
                        //this.UpdateTreeStatus(owner.FeedHandler.GetFeeds());					 
                    }
                }
            } //if(this.TreeSelectedNode != null){		
        }

        private void OnTreeFeedAfterSelect(object sender, SelectEventArgs e)
        {
            if (e.NewSelections.Count == 0)
            {
                return;
            }
            TreeFeedsNodeBase tn = (TreeFeedsNodeBase) e.NewSelections[0];
            OnTreeFeedAfterSelectManually(tn);
        }

        private void OnTreeFeedAfterSelectManually(UltraTreeNode node)
        {
            try
            {
                TreeFeedsNodeBase tn = (TreeFeedsNodeBase) node;
       
                if (tn.Type != FeedNodeType.Root)
                {
                    SetTitleText(tn.Text);
                    SetDetailHeaderText(tn);
                    MoveFeedDetailsToFront();
                }

                if (tn.Selected)
                {
                    if (tn.Type != FeedNodeType.Feed)
                    {
                        owner.Mediator.SetEnabled("-cmdFeedItemNewPost");
                    }
                    else
                    {
                        string feedUrl = tn.DataKey;
                        if (feedUrl != null && owner.FeedHandler.IsSubscribed(feedUrl))
                        {
                            owner.Mediator.SetEnabled(RssHelper.IsNntpUrl(feedUrl), "cmdFeedItemNewPost");
                        }
                        else
                        {
                            owner.Mediator.SetEnabled("-cmdFeedItemNewPost");
                        }
                    }

                    listFeedItems.FeedColumnLayout = GetFeedColumnLayout(tn); // raise events, that build the columns

                    switch (tn.Type)
                    {
                        case FeedNodeType.Feed:
                            MoveFeedDetailsToFront();
                            RefreshFeedDisplay(tn, true); // does also set the FeedDetailTabState.Url
                            AddHistoryEntry(tn, null);
                            break;

                        case FeedNodeType.Category:
                            RefreshCategoryDisplay(tn); // does also set the FeedDetailTabState.Url
                            AddHistoryEntry(tn, null);
                            break;

                        case FeedNodeType.SmartFolder:
                            try
                            {
                                FeedDetailTabState.Url = String.Empty;
                                AddHistoryEntry(tn, null);

                                ISmartFolder isf = tn as ISmartFolder;
                                if (isf != null)
                                    PopulateSmartFolder(tn, true);

                                if (tn is UnreadItemsNode && UnreadItemsNode.Items.Count > 0)
                                {
                                    FeedInfoList fiList = CreateFeedInfoList(tn.Text, UnreadItemsNode.Items);
                                    BeginTransformFeedList(fiList, tn, owner.Stylesheet);
                                }
                            }
                            catch (Exception ex)
                            {
                                _log.Error("Unexpected Error on PopulateSmartFolder()", ex);
                                owner.MessageError(SR.ExceptionGeneral(ex.Message));
                            }
                            break;

                        case FeedNodeType.Finder:
                            try
                            {
                                FeedDetailTabState.Url = String.Empty;
                                AddHistoryEntry(tn, null);
                                PopulateFinderNode((FinderNode) tn, true);
                            }
                            catch (Exception ex)
                            {
                                _log.Error("Unexpected Error on PopulateAggregatedFolder()", ex);
                                owner.MessageError(SR.ExceptionGeneral(ex.Message));
                            }
                            break;

                        case FeedNodeType.FinderCategory:
                            FeedDetailTabState.Url = String.Empty;
                            AddHistoryEntry(tn, null);
                            break;

                        case FeedNodeType.Root:
                            /* 
							if (this.GetRoot(RootFolderType.MyFeeds).Equals(tn)) 
								AggregateSubFeeds(tn);	// it is slow on startup, nothing is loaded in memory...
							*/
                            FeedDetailTabState.Url = String.Empty;
                            AddHistoryEntry(tn, null);
                            SetGuiStateFeedback(SR.StatisticsAllFeedsCountMessage(owner.FeedHandler.GetFeeds().Count));
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    owner.Mediator.SetEnabled("-cmdFeedItemNewPost");
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected Error in OnTreeFeedAfterSelect()", ex);
                owner.MessageError(SR.ExceptionGeneral(ex.Message));
            }
        }

        /// <summary>
        /// Creates the feed info list. It takes the items and groups the
        /// unread items by feed for display.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        private FeedInfoList CreateFeedInfoList(string title, IList<INewsItem> items)
        {
            FeedInfoList result = new FeedInfoList(title);
            if (items == null || items.Count == 0)
                return result;

            Hashtable fiCache = new Hashtable();

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];
                if (item == null || item.Feed == null)
                    continue;

                string feedUrl = item.Feed.link;

                if (feedUrl == null || !owner.FeedHandler.IsSubscribed(feedUrl))
                    continue;

                try
                {
                    FeedInfo fi;
                    if (fiCache.ContainsKey(feedUrl))
                        fi = fiCache[feedUrl] as FeedInfo;
                    else
                    {
                        fi = (FeedInfo) owner.GetFeedDetails(feedUrl);
                        if (fi != null)
                        {
                            fi = fi.Clone(false);
                        }
                    }

                    if (fi == null) // with an error, and the like: ignore
                        continue;

                    if (!item.BeenRead)
                        fi.ItemsList.Add(item);

                    if (fi.ItemsList.Count > 0 && !fiCache.ContainsKey(feedUrl))
                    {
                        fiCache.Add(feedUrl, fi);
                        result.Add(fi);
                    }
                }
                catch (Exception e)
                {
                    owner.PublishXmlFeedError(e, feedUrl, true);
                }
            }

            return result;
        }

        internal void RenameTreeNode(TreeFeedsNodeBase tn, string newName)
        {
            tn.TextBeforeEditing = tn.Text;
            tn.Text = newName;
            OnTreeFeedAfterLabelEdit(this, new NodeEventArgs(tn));
        }

        private static void OnTreeFeedBeforeLabelEdit(object sender, CancelableNodeEventArgs e)
        {
            TreeFeedsNodeBase editedNode = (TreeFeedsNodeBase) e.TreeNode;
            e.Cancel = !editedNode.Editable;
            if (!e.Cancel)
            {
                editedNode.TextBeforeEditing = editedNode.Text;
            }
        }

        private void OnTreeFeedsValidateLabelEdit(object sender, ValidateLabelEditEventArgs e)
        {
            string newText = e.LabelEditText;
            if (string.IsNullOrEmpty(newText))
            {
                e.StayInEditMode = true;
                return;
            }

            TreeFeedsNodeBase editedNode = (TreeFeedsNodeBase) e.Node;
            string newLabel = newText.Trim();

            if (editedNode.Type != FeedNodeType.Feed &&
                editedNode.Type != FeedNodeType.Finder)
            {
                //category node 

                TreeFeedsNodeBase existingNode =
                    TreeHelper.FindChildNode(editedNode.Parent, newLabel, FeedNodeType.Category);
                if (existingNode != null && existingNode != editedNode)
                {
                    owner.MessageError(SR.ExceptionDuplicateCategoryName(newLabel));
                    e.StayInEditMode = true;
                    return;
                }
            }
        }

        private void OnTreeFeedAfterLabelEdit(object sender, NodeEventArgs e)
        {
            TreeFeedsNodeBase editedNode = (TreeFeedsNodeBase) e.TreeNode;
            string newLabel = e.TreeNode.Text.Trim();
            string oldLabel = editedNode.TextBeforeEditing;
            editedNode.TextBeforeEditing = null; // reset for safety (only used in editing mode)

            //handle the case where right-click was used to rename a tree node even though another 
            //item was currently selected. This resets the current 
            //this.CurrentSelectedFeedsNode = this.TreeSelectedFeedsNode;

            if (editedNode.Type == FeedNodeType.Feed)
            {
                //feed node 

                INewsFeed f = owner.GetFeed(editedNode.DataKey);
                if (f != null)
                {
                    f.title = newLabel;
                    owner.FeedWasModified(f, NewsFeedProperty.FeedTitle);
                    //owner.FeedlistModified = true;
                }
            }
            else if (editedNode.Type == FeedNodeType.Finder)
            {
                // all yet done
            }
            else
            {
                //category node 

                string oldFullname = oldLabel;
                string[] catArray = TreeFeedsNodeBase.BuildCategoryStoreNameArray(editedNode);
                if (catArray.Length > 0)
                {
                    // build old category store name by replace the new label returned
                    // by the oldLabel kept:
                    catArray[catArray.Length - 1] = oldLabel;
                    oldFullname = String.Join(FeedSource.CategorySeparator, catArray);
                }

                if (GetRoot(editedNode) == RootFolderType.MyFeeds)
                {
                    string newFullname = editedNode.CategoryStoreName;

                    IDictionary<string, INewsFeedCategory> categories = owner.FeedHandler.GetCategories();
                    string[] catList = new string[categories.Count];
                    categories.Keys.CopyTo(catList, 0);
                    // iterate on a copied list, so we can change the old one without
                    // side effects
                    foreach (string catKey in catList)
                    {
                        if (catKey.Equals(oldFullname) || catKey.StartsWith(oldFullname + FeedSource.CategorySeparator))
                        {
                            string newCatKey = newFullname + ( catKey.Length == oldFullname.Length ?
                                                                String.Empty : catKey.Substring(oldFullname.Length));                                            
                            owner.FeedHandler.RenameCategory(catKey, newCatKey);  
                        }
                    }

                    // funny recursive part:
                    // change category in feed manager 
                    // (also updates tree node in UI)
                    WalkdownThenRenameFeedCategory(editedNode, newFullname);
                    owner.SubscriptionModified(NewsFeedProperty.FeedCategory);
                    //owner.FeedlistModified = true;
                }
            }
        }


        private void OnTreeFeedSelectionDragStart(object sender, EventArgs e)
        {
            TreeFeedsNodeBase tn = TreeSelectedFeedsNode;
            if (tn != null && (tn.Type == FeedNodeType.Feed || tn.Type == FeedNodeType.Category))
            {
                CurrentDragNode = tn;

                if (CurrentDragNode.Expanded)
                    CurrentDragNode.Expanded = false;

                string dragObject = null;

                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    IFeedDetails fd = null;
                    if (CurrentDragNode.Type == FeedNodeType.Feed)
                        fd = owner.GetFeedDetails(CurrentDragNode.DataKey);
                    if (fd != null)
                    {
                        dragObject = fd.Link;
                    }
                }
                if (dragObject != null)
                {
                    DoDragDrop(dragObject, DragDropEffects.Copy | DragDropEffects.Link);
                }
                else
                {
                    if (CurrentDragNode.Type == FeedNodeType.Feed)
                    {
                        dragObject = CurrentDragNode.DataKey;
                    }
                    else
                    {
                        dragObject = CurrentDragNode.Text;
                    }
                    DoDragDrop(dragObject, DragDropEffects.Copy | DragDropEffects.Move);
                }
                CurrentDragHighlightNode = CurrentDragNode = null;
            }
        }

        private void OnTreeFeedDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link; // we got this on drag urls from IE !
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                    return;
                }

                Point p = new Point(e.X, e.Y);
                TreeFeedsNodeBase t = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(treeFeeds.PointToClient(p));

                if (t == null)
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                }
                if (t != null)
                {
                    if (t.Type == FeedNodeType.Feed)
                        CurrentDragHighlightNode = t.Parent;
                    else if (t.Type == FeedNodeType.Category || GetRoot(RootFolderType.MyFeeds).Equals(t))
                        CurrentDragHighlightNode = t;
                    else
                    {
                        e.Effect = DragDropEffects.None;
                        CurrentDragHighlightNode = null;
                    }
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
                CurrentDragHighlightNode = null;
            }
        }

        private static void OnTreeFeedGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            //if we are a drag source, ...
            _log.Debug("OnTreeFeedGiveFeedback() effect:" + e.Effect);
        }

        private static void OnTreeFeedQueryContiueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // keyboard or mouse button state changes
            // we listen to Unpress Ctrl:
            _log.Debug("OnTreeFeedQueryContiueDrag() action:" + e.Action + ", KeyState:" +
                       e.KeyState);
        }

        private void OnTreeFeedDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link;
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                    return;
                }

                Point p = treeFeeds.PointToClient(new Point(e.X, e.Y));
                TreeFeedsNodeBase t = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(p);

                if (t == null)
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                }

                if (t != null)
                {
                    if (t.Type == FeedNodeType.Feed)
                        CurrentDragHighlightNode = t.Parent;
                    else if (t.Type == FeedNodeType.Category || GetRoot(RootFolderType.MyFeeds).Equals(t))
                        CurrentDragHighlightNode = t;
                    else
                    {
                        e.Effect = DragDropEffects.None;
                        CurrentDragHighlightNode = null;
                    }
                }

                // UltraTree can scroll automatically, if the mouse
                // is near top/bottom, so this code is just there for
                // reference - how to apply this to a MS Treeview:
//				int tcsh = this.treeFeeds.ClientSize.Height;
//				int scrollThreshold = 25;
//				if (p.Y + scrollThreshold > tcsh)
//					Win32.SendMessage(this.treeFeeds.Handle, (int)Win32.Message.WM_VSCROLL, 1, 0);
//				else if (p.Y < scrollThreshold)
//					Win32.SendMessage(this.treeFeeds.Handle, (int)Win32.Message.WM_VSCROLL, 0, 0);
            }
            else
            {
                e.Effect = DragDropEffects.None;
                CurrentDragHighlightNode = null;
            }
        }

        private void OnTreeFeedDragDrop(object sender, DragEventArgs e)
        {
            CurrentDragHighlightNode = null;

            //get node where feed was dropped 
            Point p = new Point(e.X, e.Y);
            TreeFeedsNodeBase target = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(treeFeeds.PointToClient(p));

            //move node if dropped on a category node (or below)
            if (target != null)
            {
                TreeFeedsNodeBase node2move = CurrentDragNode;

                if (target.Type == FeedNodeType.Feed)
                {
                    // child of a category. Take the parent as target
                    target = target.Parent;
                }


                if (node2move != null)
                {
                    MoveNode(node2move, target, true);
                }
                else
                {
                    // foreign drag/drop op

                    // Bring the main window to the front so the user can
                    // enter the dropped feed details.  Otherwise the feed
                    // details window can pop up underneath the drop source,
                    // which is confusing.
                    Win32.SetForegroundWindow(Handle);

                    string sData = (string) e.Data.GetData(DataFormats.Text);
                    DelayTask(DelayedTasks.AutoSubscribeFeedUrl, new object[] {target, sData});
                }
            }

            CurrentDragNode = null;
        }

        private void OnTimerTreeNodeExpandElapsed(object sender, ElapsedEventArgs e)
        {
            //_timerTreeNodeExpand.Stop();
            //if (CurrentDragHighlightNode != null) 
            //{
            //    if (!CurrentDragHighlightNode.Expanded)
            //        CurrentDragHighlightNode.Expanded = true;
            //}
        }

        private void OnTimerFeedsRefreshElapsed(object sender, ElapsedEventArgs e)
        {
			if (owner.InternetAccessAllowed && owner.CurrentGlobalRefreshRateMinutes > 0)
            {
                UpdateAllFeeds(false);
            }
        }

        private void OnTimerCommentFeedsRefreshElapsed(object sender, ElapsedEventArgs e)
        {
			if (owner.InternetAccessAllowed && owner.CurrentGlobalRefreshRateMinutes > 0)
            {
                UpdateAllCommentFeeds(true);
            }
        }

        /// <summary>
        /// Called when startup timer fires (ca. 45 secs after UI startup).
        /// This delay is required, if Bandit gets started via Windows Auto-Start
        /// to prevent race conditions with WLAN startup/LAN init (we require the
        /// Internet connection to succeed)
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnTimerStartupTick(object sender, EventArgs e)
        {
            _startupTimer.Enabled = false;
            // start load items and refresh from web, force if we have to refresh on startup:
            if (owner.InternetAccessAllowed)
            {
                UpdateAllFeeds(owner.Preferences.FeedRefreshOnStartup);
            }
        }

        private void OnTimerResetStatusTick(object sender, EventArgs e)
        {
            _timerResetStatus.Stop();
            SetGuiStateFeedback(String.Empty);
            if (_trayManager.CurrentState == ApplicationTrayState.BusyRefreshFeeds ||
                GetRoot(RootFolderType.MyFeeds).UnreadCount == 0)
            {
                _trayManager.SetState(ApplicationTrayState.NormalIdle);
            }
        }

        internal void OnFeedListItemActivate(object sender, EventArgs e)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            if ((MouseButtons & MouseButtons.Right) == MouseButtons.Right)
                return;

            ThreadedListViewItem selectedItem = (ThreadedListViewItem) listFeedItems.SelectedItems[0];
            OnFeedListItemActivateManually(selectedItem);
        }

        internal void OnFeedListItemActivateManually(ThreadedListViewItem selectedItem)
        {
            try
            {
                // get the current item/feedNode
                INewsItem item = CurrentSelectedFeedItem = (INewsItem) selectedItem.Key;
                TreeFeedsNodeBase tn = TreeSelectedFeedsNode;
                string stylesheet;

                //load item content from disk if not in memory
                if (item != null && !item.HasContent)
                {
                    owner.FeedHandler.GetCachedContentForItem(item);
                }

                // refresh context menu items
                RefreshTreeFeedContextMenus(tn);

                if (item != null && tn != _sentItemsFeedsNode &&
                    item.CommentStyle != SupportedCommentStyle.None &&
                    owner.InternetAccessAllowed)
                    owner.Mediator.SetEnabled("+cmdFeedItemPostReply");
                else
                    owner.Mediator.SetEnabled("-cmdFeedItemPostReply");

                SearchCriteriaCollection searchCriterias = null;
                FinderNode agNode = CurrentSelectedFeedsNode as FinderNode;
                if (agNode != null && agNode.Finder.DoHighlight)
                    searchCriterias = agNode.Finder.SearchCriterias;


                //mark the item as read
                bool itemJustRead = false;

                if (item != null && !item.BeenRead)
                {
                    itemJustRead = item.BeenRead = true;

                    if (item is SearchHitNewsItem)
                    {
                        SearchHitNewsItem sItem = item as SearchHitNewsItem;
                        INewsItem realItem = owner.FeedHandler.FindNewsItem(sItem);

                        if (realItem != null)
                        {
                            realItem.BeenRead = true;
                            item = realItem;
                        }
                    }
                }

                //render 
                if (item == null)
                {
                    // can happen on dummy items ("Loading..."), if the user clicks fast enough

                    htmlDetail.Clear();
                    FeedDetailTabState.Url = String.Empty;
                    RefreshDocumentState(_docContainer.ActiveDocument);
                }
                else if (!item.HasContent && !string.IsNullOrEmpty(item.Link))
                {
                    /* if (this.UrlRequestHandledExternally(item.Link, false)) {
						htmlDetail.Clear();
					} else */
                    if (owner.Preferences.NewsItemOpenLinkInDetailWindow)
                    {
                        htmlDetail.Navigate(item.Link);
                    }
                    else
                    {
                        // not allowed: just display the Read On... 
                        stylesheet = (item.Feed != null ? owner.FeedHandler.GetStyleSheet(item.Feed.link) : String.Empty);
                        htmlDetail.Html = owner.FormatNewsItem(stylesheet, item, searchCriterias);
                        htmlDetail.Navigate(null);
                    }

                    FeedDetailTabState.Url = item.Link;
                    if (! _navigationActionInProgress)
                    {
                        AddHistoryEntry(tn, item);
                    }
                    else
                    {
                        RefreshDocumentState(_docContainer.ActiveDocument);
                    }
                }
                else
                {
                    stylesheet = (item.Feed != null ? owner.FeedHandler.GetStyleSheet(item.Feed.link) : String.Empty);
                    htmlDetail.Html = owner.FormatNewsItem(stylesheet, item, searchCriterias);
                    htmlDetail.Navigate(null);

                    FeedDetailTabState.Url = item.Link;
                    if (! _navigationActionInProgress)
                    {
                        AddHistoryEntry(tn, item);
                    }
                    else
                    {
                        RefreshDocumentState(_docContainer.ActiveDocument);
                    }
                }


                if (item != null)
                {
                    //assume that clicking on the item indicates viewing new comments 
                    //when no comment feed available
                    if (item.WatchComments && string.IsNullOrEmpty(item.CommentRssUrl))
                    {
                        MarkCommentsAsViewed(tn, item);
                        ApplyStyles(selectedItem, true);
                    } //if(item.WatchComments...)

                    //if item was read on this click then reflect the change in the GUI 

                    if (itemJustRead)
                    {
                        ApplyStyles(selectedItem, true);
                        SmartFolderNodeBase sfNode = CurrentSelectedFeedsNode as SmartFolderNodeBase;

                        if (selectedItem.ImageIndex > 0) selectedItem.ImageIndex--;

                        bool isTopLevelItem = (selectedItem.IndentLevel == 0);
                        int equalItemsRead = (isTopLevelItem ? 1 : 0);
                        lock (listFeedItems.Items)
                        {
                            for (int j = 0; j < listFeedItems.Items.Count; j++)
                            {
                                // if there is a self-reference thread, we also have to switch the Gui state for them
                                ThreadedListViewItem th = listFeedItems.Items[j];
                                NewsItem selfRef = th.Key as NewsItem;
                                if (item.Equals(selfRef) && (th.ImageIndex%2) != 0)
                                {
                                    // unread-state images always odd index numbers
                                    ApplyStyles(th, true);
                                    th.ImageIndex--;
                                    if (!selfRef.BeenRead)
                                    {
                                        // object ref is unequal, but other criteria match the item to be equal...
                                        selfRef.BeenRead = true;
                                    }
                                    if (th.IndentLevel == 0)
                                    {
                                        isTopLevelItem = true;
                                        equalItemsRead++;
                                    }
                                }
                            }
                        }

                        if (isTopLevelItem && tn.Type == FeedNodeType.Feed || tn.Type == FeedNodeType.SmartFolder ||
                            tn.Type == FeedNodeType.Finder)
                        {
                            UpdateTreeNodeUnreadStatus(tn, -equalItemsRead);
                            UnreadItemsNode.MarkItemRead(item);
                            //this.DelayTask(DelayedTasks.RefreshTreeUnreadStatus, new object[]{tn,-equalItemsRead});											
                        }

                        TreeFeedsNodeBase root = GetRoot(RootFolderType.MyFeeds);

                        if (item.Feed.link == tn.DataKey)
                        {
                            // test for catch all on selected node
                            item.Feed.containsNewMessages = (tn.UnreadCount != 0);
                        }
                        else
                        {
                            // other (categorie selected, aggregated or an threaded item from another feed)

                            if (agNode != null) agNode.UpdateReadStatus();
                            if (sfNode != null) sfNode.UpdateReadStatus();
                            if (isTopLevelItem && tn.Type == FeedNodeType.Category)
                                UnreadItemsNode.MarkItemRead(item);

                            // lookup corresponding TreeNode:
                            TreeFeedsNodeBase refNode = TreeHelper.FindNode(root, item.Feed);
                            if (refNode != null)
                            {
                                //refNode.UpdateReadStatus(refNode , -1);
                                DelayTask(DelayedTasks.RefreshTreeUnreadStatus, new object[] {refNode, -1});
                                item.Feed.containsNewMessages = (refNode.UnreadCount != 0);
                            }
                            else
                            {
                                // temp feed item, e.g. from commentRss
                                string hash = RssHelper.GetHashCode(item);
                                if (!tempFeedItemsRead.ContainsKey(hash))
                                    tempFeedItemsRead.Add(hash, null /* item ???*/);
                            }
                        }

                        owner.FeedWasModified(item.Feed, NewsFeedProperty.FeedItemReadState);
                        //owner.FeedlistModified = true;
                    } //itemJustRead
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListItemActivateManually() failed.", ex);
            }
        }


        /// <summary>
        /// Returns the URL of the original feed for this NewsItem. 
        /// </summary>
        /// <remarks>Assumes the NewsItem is in the flagged or watched items smart folder</remarks>
        /// <param name="currentNewsItem"></param>
        /// <returns>The feed URL of the source feed if  a pointer to it exists and NULL otherwise.</returns>
        private static string GetOriginalFeedUrl(INewsItem currentNewsItem)
        {
            string feedUrl = null;

            if (currentNewsItem.OptionalElements.ContainsKey(AdditionalFeedElements.OriginalFeedOfWatchedItem))
            {
                string str = (string) currentNewsItem.OptionalElements[AdditionalFeedElements.OriginalFeedOfWatchedItem];

                if (
                    str.StartsWith("<" + AdditionalFeedElements.ElementPrefix + ":" +
                                   AdditionalFeedElements.OriginalFeedOfWatchedItem.Name) ||
                    str.StartsWith("<" + AdditionalFeedElements.OldElementPrefix + ":" +
                                   AdditionalFeedElements.OriginalFeedOfWatchedItem.Name))
                {
                    int startIndex = str.IndexOf(">") + 1;
                    int endIndex = str.LastIndexOf("<");
                    feedUrl = str.Substring(startIndex, endIndex - startIndex);
                }
            }

            return feedUrl;
        }

        /// <summary>
        /// Marks the comments for a NewsItem as read in a given feed node and across any other feed nodes 
        /// in which it appears. 
        /// </summary>
        /// <param name="tn">The feed node</param>
        /// <param name="currentNewsItem">The item whose comments have been read</param>
        private void MarkCommentsAsViewed(TreeFeedsNodeBase tn, INewsItem currentNewsItem)
        {
            INewsFeed feed = currentNewsItem.Feed;
            bool commentsJustRead = currentNewsItem.HasNewComments;
            currentNewsItem.HasNewComments = false;

            if (commentsJustRead && (CurrentSelectedFeedsNode != null))
            {
                TreeFeedsNodeBase refNode = null;

                if (tn.Type == FeedNodeType.Feed)
                {
                    UpdateCommentStatus(tn, new List<INewsItem>(new INewsItem[] {currentNewsItem}), true);
                }
                else
                {
                    //if we are on a category or search folder, then locate node under MyFeeds and update its comment status												
                    if (tn.Type == FeedNodeType.Category || tn.Type == FeedNodeType.Finder)
                    {
                        refNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), currentNewsItem.Feed);
                        UpdateCommentStatus(refNode, new List<INewsItem>(new INewsItem[] {currentNewsItem}), true);

                        //we don't need to do this for a Category node because this should be done when we call this.UpdateCommentStatus()
                        if (tn.Type == FeedNodeType.Finder)
                            tn.UpdateCommentStatus(tn, -1);
                    }
                    else if (tn.Type == FeedNodeType.SmartFolder)
                    {
                        //things are more complicated if we are on a smart folder such as the 'Watched Items' folder

                        /* first get the feed URL */
                        string feedUrl = GetOriginalFeedUrl(currentNewsItem);

                        /* 
						 * now, locate NewsItem in actual feed and mark comments as viewed 
						 * then update tree node comment status. 							 
						 */
                        if (feedUrl != null && owner.FeedHandler.GetFeeds().TryGetValue(feedUrl, out feed))
                        {                            
                            IList<INewsItem> newsItems = owner.FeedHandler.GetCachedItemsForFeed(feedUrl);

                            foreach (INewsItem ni in newsItems)
                            {
                                if (currentNewsItem.Equals(ni))
                                {
                                    ni.HasNewComments = false;
                                }
                            }

                            refNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
                            UpdateCommentStatus(refNode, new List<INewsItem>(new INewsItem[] {currentNewsItem}), true);
                        } //if(feedUrl != null) 
                    }

                    /* if (refNode != null) {
								this.DelayTask(DelayedTasks.RefreshTreeCommentStatus, new object[]{refNode, new ArrayList(new NewsItem[]{currentNewsItem}), true});
							} */


                    if (refNode == null)
                    {
                        feed.containsNewComments = (tn.ItemsWithNewCommentsCount != 0);
                    }
                    else
                    {
                        feed.containsNewComments = (refNode.ItemsWithNewCommentsCount != 0);
                    }

                    owner.FeedWasModified(feed, NewsFeedProperty.FeedItemNewCommentsRead);
                } //if (tn.Type == FeedNodeType.Feed )
            } //if(commentsJustRead && (this.CurrentSelectedFeedsNode!= null))
        }

        private void OnFeedListExpandThread(object sender, ThreadEventArgs e)
        {
            try
            {
                INewsItem currentNewsItem = (INewsItem) e.Item.Key;
                INewsItem[] ikp = new INewsItem[e.Item.KeyPath.Length];
                e.Item.KeyPath.CopyTo(ikp, 0);
                IList<INewsItem> itemKeyPath = ikp;

                // column index map
                ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();

                ICollection<INewsItem> outGoingItems =
                    owner.FeedHandler.GetItemsFromOutGoingLinks(currentNewsItem, itemKeyPath);
                ICollection<INewsItem> inComingItems =
                    owner.FeedHandler.GetItemsWithIncomingLinks(currentNewsItem, itemKeyPath);

                ArrayList childs = new ArrayList(outGoingItems.Count + inComingItems.Count + 1);
                ThreadedListViewItem newListItem;

                try
                {
                    foreach (NewsItem o in outGoingItems)
                    {
                        bool hasRelations = NewsItemHasRelations(o, itemKeyPath);
                        newListItem =
                            CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.OutgoingRead, colIndex, false);

                        //does it match any filter? 
                        _filterManager.Apply(newListItem);

                        childs.Add(newListItem);
                    }
                }
                catch (Exception e1)
                {
                    _log.Error("OnFeedListExpandThread exception (iterate outgoing)", e1);
                }

                try
                {
                    foreach (NewsItem o in inComingItems)
                    {
                        bool hasRelations = NewsItemHasRelations(o, itemKeyPath);
                        newListItem =
                            CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.IncomingRead, colIndex, false);

                        //does it match any filter? 
                        _filterManager.Apply(newListItem);

                        childs.Add(newListItem);
                    } //iterator.MoveNext
                }
                catch (Exception e2)
                {
                    _log.Error("OnFeedListExpandThread exception (iterate incoming)", e2);
                }

                if (currentNewsItem.HasExternalRelations)
                {
                    // includes also commentRss support

                    if (currentNewsItem.GetExternalRelations().Count == 0 ||
                        currentNewsItem.CommentCount != currentNewsItem.GetExternalRelations().Count)
                    {
                        if (owner.InternetAccessAllowed)
                        {
                            ThreadedListViewItemPlaceHolder insertionPoint =
                                (ThreadedListViewItemPlaceHolder)
                                CreateThreadedLVItemInfo(SR.GUIStatusLoadingChildItems, false);
                            childs.Add(insertionPoint);
                            BeginLoadCommentFeed(currentNewsItem, insertionPoint.InsertionPointTicket, itemKeyPath);
                        }
                        else
                        {
                            newListItem = CreateThreadedLVItemInfo(SR.GUIStatusChildItemsNA, false);
                            childs.Add(newListItem);
                        }
                    }
                    else
                    {
                        // just take the existing collection

                        // they are sorted as we requested them, so we do not sort again here
                        //currentNewsItem.GetExternalRelations();

                        //List<NewsItem> commentItems 
                        //  = new List<RelationBase>(currentNewsItem.GetExternalRelations()).ConvertAll<NewsItem>(RssBandit.Common.Utils.TypeConverter.DownCast<RelationBase, NewsItem>());
                        //commentItems.Sort(RssHelper.GetComparer(false, NewsItemSortField.Date));

                        foreach (INewsItem o in currentNewsItem.GetExternalRelations())
                        {
                            bool hasRelations = NewsItemHasRelations(o, itemKeyPath);

                            o.BeenRead = tempFeedItemsRead.ContainsKey(RssHelper.GetHashCode(o));
                            newListItem =
                                CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.CommentRead, colIndex, true);
                            _filterManager.Apply(newListItem);
                            childs.Add(newListItem);
                        } //iterator.MoveNext
                    }
                }

                e.ChildItems = new ThreadedListViewItem[childs.Count];
                childs.CopyTo(e.ChildItems);

                //mark new comments as read once we've successfully loaded comments 				
                MarkCommentsAsViewed(CurrentSelectedFeedsNode, currentNewsItem);
                ApplyStyles(e.Item, currentNewsItem.BeenRead, currentNewsItem.HasNewComments);
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListExpandThread exception", ex);
            }
        }

        private void OnFeedListAfterExpandThread(object sender, ThreadEventArgs e)
        {
            // here we have the listview handle set and the listview items are member of the list.
            // so we refresh flag icons for new listview thread childs here:
            ApplyNewsItemPropertyImages(e.ChildItems);
        }

        private void OnFeedListLayoutChanged(object sender, ListLayoutEventArgs e)
        {
            // build columns, etc. pp
            if (e.Layout.Columns.Count > 0)
            {
                EmptyListView();
                lock (listFeedItems.Columns)
                {
                    listFeedItems.Columns.Clear();
                    int i = 0;
                    IList<int> colW = e.Layout.ColumnWidths;
                    foreach (string colID in e.Layout.Columns)
                    {
                        AddListviewColumn(colID, colW[i++]);
                    }
                }
            }
            RefreshListviewColumnContextMenu();
        }

        private void OnFeedListLayoutModified(object sender, ListLayoutEventArgs e)
        {
            if (TreeSelectedFeedsNode != null)
                SetFeedHandlerFeedColumnLayout(TreeSelectedFeedsNode, e.Layout);
        }

        private void OnFeedListItemsColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listFeedItems.Items.Count == 0)
                return;

            if (listFeedItems.SelectedItems.Count > 0)
                return;

            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode == null)
                return;

            bool unreadOnly = true;
            if (feedsNode.Type == FeedNodeType.Finder)
                unreadOnly = false;

            IList<INewsItem> items = NewsItemListFrom(listFeedItems.Items, unreadOnly);
            if (items == null || items.Count <= 1) // no need to re-sort on no or just one item
                return;


            Hashtable temp = new Hashtable();

            foreach (NewsItem item in items)
            {
                FeedInfo fi;
                if (temp.ContainsKey(item.Feed.link))
                {
                    fi = (FeedInfo) temp[item.Feed.link];
                }
                else
                {
                    fi = (FeedInfo) item.FeedDetails.Clone();
                    fi.ItemsList.Clear();
                    temp.Add(item.Feed.link, fi);
                }
                fi.ItemsList.Add(item);
            }

            string category = feedsNode.CategoryStoreName;
            FeedInfoList redispItems = new FeedInfoList(category);

            foreach (FeedInfo fi in temp.Values)
            {
                if (fi.ItemsList.Count > 0)
                    redispItems.Add(fi);
            }

            BeginTransformFeedList(redispItems, CurrentSelectedFeedsNode,
                                   owner.FeedHandler.GetCategoryStyleSheet(category));
        }

        private void OnFeedListMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                ListView lv = (ListView) sender;
                ThreadedListViewItem lvi = null;

                try
                {
                    lvi = (ThreadedListViewItem) lv.GetItemAt(e.X, e.Y);
                }
                catch
                {
                }

                if (e.Button == MouseButtons.Right)
                {
                    // behavior similar to Windows Explorer Listview:					
                    if (lv.Items.Count > 0)
                    {
                        if (lvi != null)
                        {
                            // if(Control.ModifierKeys != Keys.Control)
                            //    lv.SelectedItems.Clear();							   														 

                            lvi.Selected = true;
                            lvi.Focused = true;
                            RefreshListviewContextMenu();
                            OnFeedListItemActivateManually(lvi);
                        }
                    }

// TR: commented out - incorrect behavior (loosing selection, etc.)					
//					if (lv.Items.Count > 0) 
//					{
//					
//						if (lvi != null) 
//						{
//							lv.SelectedItems.Clear();
//							lvi.Selected = true;
//							lvi.Focused = true;							
//							RefreshListviewContextMenu();
//							this.OnFeedListItemActivate(sender, EventArgs.Empty);
//						}
//
//					}
                }
                else
                {
                    // !MouseButtons.Right

                    if (lv.Items.Count <= 0)
                        return;

                    if (lvi != null && e.Clicks > 1)
                    {
                        //DblClick

                        INewsItem item = CurrentSelectedFeedItem = (INewsItem) lvi.Key;

                        lv.SelectedItems.Clear();
                        lvi.Selected = true;
                        lvi.Focused = true;

                        if (item != null && !string.IsNullOrEmpty(item.Link))
                        {
                            if (!UrlRequestHandledExternally(item.Link, false))
                                DetailTabNavigateToUrl(item.Link, null, false, true);
                        }
                    }
                } //! MouseButtons.Right
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListMouseDown() failed", ex);
            }
        }

        private void OnStatusPanelClick(object sender, StatusBarPanelClickEventArgs e)
        {
            if (e.Clicks > 1 && e.StatusBarPanel == statusBarConnectionState)
            {
                // DblClick to the connection state panel image
                owner.UpdateInternetConnectionState(true); // force a connection check
            }
        }

        private void OnStatusPanelLocationChanged(object sender, EventArgs e)
        {
            progressBrowser.SetBounds(_status.Width -
                                      (statusBarRssParser.Width + statusBarConnectionState.Width +
                                       BrowserProgressBarWidth + 10),
                                      _status.Location.Y + 6, 0, 0, BoundsSpecified.Location);
        }

        private void RePopulateListviewWithCurrentContent()
        {
            RePopulateListviewWithContent(NewsItemListFrom(listFeedItems.Items));
        }

        private void RePopulateListviewWithContent(IList<INewsItem> newsItemList)
        {
            if (newsItemList == null)
                newsItemList = new List<INewsItem>(0);

            ThreadedListViewItem lvLastSelected = null;
            if (listFeedItems.SelectedItems.Count > 0)
                lvLastSelected = (ThreadedListViewItem) listFeedItems.SelectedItems[0];

            bool categorizedView = (CurrentSelectedFeedsNode.Type == FeedNodeType.Category) ||
                                   (CurrentSelectedFeedsNode.Type == FeedNodeType.Finder);
            PopulateListView(CurrentSelectedFeedsNode, newsItemList, true, categorizedView, CurrentSelectedFeedsNode);

            // reselect the last selected
            if (lvLastSelected != null && lvLastSelected.IndentLevel == 0)
            {
                ReSelectListViewItem((INewsItem) lvLastSelected.Key);
            }
        }

        private void ReSelectListViewItem(IRelation item)
        {
            if (item == null) return;

            string selItemId = item.Id;
            if (selItemId != null)
            {
                for (int i = 0; i < listFeedItems.Items.Count; i++)
                {
                    ThreadedListViewItem theItem = listFeedItems.Items[i];
                    string thisItemId = ((INewsItem) theItem.Key).Id;
                    if (selItemId.CompareTo(thisItemId) == 0)
                    {
                        listFeedItems.Items[i].Selected = true;
                        listFeedItems.EnsureVisible(listFeedItems.Items[i].Index);
                        break;
                    }
                }
            }
        }

        private static IList<INewsItem> NewsItemListFrom(ThreadedListViewItemCollection list)
        {
            return NewsItemListFrom(list, false);
        }

        private static IList<INewsItem> NewsItemListFrom(ThreadedListViewItemCollection list, bool unreadOnly)
        {
            List<INewsItem> items = new List<INewsItem>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                ThreadedListViewItem tlvi = list[i];
                if (tlvi.IndentLevel == 0)
                {
                    INewsItem item = (INewsItem) tlvi.Key;

                    if (unreadOnly && item != null && item.BeenRead)
                        item = null;

                    if (item != null)
                        items.Add(item);
                }
            }
            return items;
        }

        private void OnFeedListItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ThreadedListViewItem item = (ThreadedListViewItem) e.Item;
                INewsItem r = (INewsItem) item.Key;
                if (r.Link != null)
                {
                    treeFeeds.AllowDrop = false; // do not drag to tree
                    DoDragDrop(r.Link, DragDropEffects.All | DragDropEffects.Link);
                    treeFeeds.AllowDrop = true;
                }
            }
        }

        /// <summary>
        /// support the keydown/pagedown keyup/pageup listview navigation 
        /// as well as deleting items via the Delete key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFeedListItemKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!listFeedItems.Focused)
                    return;

#if TRACE_WIN_MESSAGES				
				Debug.WriteLine("OnFeedListItemKeyUp(" + e.KeyData +")");
#endif

                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.End)
                {
                    if (listFeedItems.SelectedItems.Count == 1)
                        if (listFeedItems.SelectedItems[0].Index <= listFeedItems.Items.Count)
                            OnFeedListItemActivate(sender, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Home)
                {
                    if (listFeedItems.SelectedItems.Count == 1)
                        if (listFeedItems.SelectedItems[0].Index >= 0)
                            OnFeedListItemActivate(sender, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) == Keys.Control)
                {
                    // select all
                    if (listFeedItems.Items.Count > 0 && listFeedItems.Items.Count != listFeedItems.SelectedItems.Count)
                    {
                        try
                        {
                            listFeedItems.BeginUpdate();
                            lock (listFeedItems.Items)
                            {
                                for (int i = 0; i < listFeedItems.Items.Count; i++)
                                {
                                    listFeedItems.Items[i].Selected = true;
                                }
                            }
                        }
                        finally
                        {
                            listFeedItems.EndUpdate();
                        }
                    }
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    RemoveSelectedFeedItems();
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListItemKeyUp() failed", ex);
            }
        }


        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            owner.CmdShowMainGui(null);
            //user is interested about the message this time
            _beSilentOnBalloonPopupCounter = 0; // reset balloon silent counter
        }

        //called, if the user explicitly closed the balloon
        private void OnTrayAniBalloonTimeoutClose(object sender, EventArgs e)
        {
            //user isn't interested about the message this time
            _beSilentOnBalloonPopupCounter = 12; // 12 * 5 minutes (refresh timer) == 1 hour (minimum)
        }

        #region toolbar combo's events

        internal void OnNavigateComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && e.Control == false)
            {
                // CTRL-ENTER is Url expansion
                DetailTabNavigateToUrl(UrlText, null, e.Shift, false);
            }
        }

        internal static void OnNavigateComboBoxDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link;
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        internal void OnNavigateComboBoxDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string sData = (string) e.Data.GetData(typeof (string));
                try
                {
                    // accept uri only
                    Uri uri = new Uri(sData);
                    UrlText = uri.CanonicalizedUri();
                }
                catch
                {
                    //this.UrlText = sData;
                }
            }
        }


        internal void OnSearchComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.Handled = true;
                StartSearch(null);
            }
        }

        internal void OnSearchComboBoxDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link;
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        internal void OnSearchComboBoxDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string sData = (string) e.Data.GetData(typeof (string));
                WebSearchText = sData;
            }
        }

        #endregion

        #region html control events   

        private void OnHtmlWindowError(string description, string url, int line)
        {
            /* don't show script error dialog and don't disable script due to a single script error */
            HtmlControl hc = (HtmlControl) _docContainer.ActiveDocument.Controls[0];

            if (hc != null)
            {
                IHTMLWindow2 window = (IHTMLWindow2) hc.Document2.GetParentWindow();
                IHTMLEventObj eventObj = window.eventobj;
                eventObj.ReturnValue = true;
            }
        }


        private void OnWebStatusTextChanged(object sender, BrowserStatusTextChangeEvent e)
        {
            SetBrowserStatusBarText(e.text);
        }

        private void OnWebBeforeNavigate(object sender, BrowserBeforeNavigate2Event e)
        {
            bool userNavigates = _webUserNavigated;
            bool forceNewTab = _webForceNewTab;

            string url = e.url;

            if (!url.ToLower().StartsWith("javascript:"))
            {
                _webForceNewTab = _webUserNavigated = false; // reset, but keep it for the OnWebBeforeNewWindow event
            }

            if (!url.Equals("about:blank"))
            {
                if (owner.InterceptUrlNavigation(url))
                {
                    e.Cancel = true;
                    return;
                }

                if (url.StartsWith("mailto:") || url.StartsWith("news:"))
                {
//TODO: if nntp is impl., InterceptUrlNavigation() should handle "news:"
                    return;
                }

                bool framesAllowed = false;
                bool forceSetFocus = true;
                bool tabCanClose = true;
                // if Ctrl-Click is true, Tab opens in background:
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                    forceSetFocus = false;

                HtmlControl hc = sender as HtmlControl;
                if (hc != null)
                {
                    DockControl dc = (DockControl) hc.Tag;
                    ITabState ts = (ITabState) dc.Tag;
                    tabCanClose = ts.CanClose;
                    framesAllowed = hc.FrameDownloadEnabled;
                }

                if (userNavigates && UrlRequestHandledExternally(url, forceNewTab))
                {
                    e.Cancel = true;
                    return;
                }

                if (!tabCanClose && !userNavigates && !forceNewTab)
                {
                    if (!framesAllowed)
                        e.Cancel = !e.IsRootPage; // prevent sub-sequent requests of <iframe>'s
                    // else just allow navigate in current browser
                    return;
                }

                if ((!tabCanClose && userNavigates) || forceNewTab)
                {
                    e.Cancel = true;
                    // Delay gives time to the sender control to cancel request
                    DelayTask(DelayedTasks.NavigateToWebUrl, new object[] {url, null, forceNewTab, forceSetFocus});
                }
            }
        }

        private void OnWebNavigateComplete(object sender, BrowserNavigateComplete2Event e)
        {
            // if we cancelled subsequent requests in the WebBeforeNavigate event,
            // we may not receive the OnWebDocumentComplete event for the master page
            // so in general we do the same things here as in OnWebDocumentComplete()
            try
            {
                HtmlControl hc = (HtmlControl) sender;

                //handle script errors on page
                HTMLWindowEvents2_Event window = (HTMLWindowEvents2_Event) hc.Document2.GetParentWindow();
                window.onerror += OnHtmlWindowError;

                if (!string.IsNullOrEmpty(e.url) && e.url != "about:blank" && e.IsRootPage)
                {
                    AddUrlToHistory(e.url);

                    DockControl doc = (DockControl) hc.Tag;
                    ITabState state = (ITabState) doc.Tag;
                    state.Url = e.url;
                    RefreshDocumentState(doc);
                    // we should only discover once per browse action (in OnWebDocumentComplete()):
                    //owner.BackgroundDiscoverFeedsHandler.DiscoverFeedInContent(hc.DocumentOuterHTML, state.Url, null);
                    // do some more things here, because we may also not receive the events...
                    DelayTask(DelayedTasks.ClearBrowserStatusInfo, null, 2000);
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebNavigateComplete(): " + e.url, ex);
            }
        }


        private void OnWebDocumentComplete(object sender, BrowserDocumentCompleteEvent e)
        {
            try
            {
                HtmlControl hc = (HtmlControl) sender;

                //handle script errors on page
                HTMLWindowEvents2_Event window = (HTMLWindowEvents2_Event) hc.Document2.GetParentWindow();
                window.onerror += OnHtmlWindowError;

                if (!string.IsNullOrEmpty(e.url) && e.url != "about:blank" && e.IsRootPage)
                {
                    AddUrlToHistory(e.url);

                    DockControl doc = (DockControl) hc.Tag;
                    ITabState state = (ITabState) doc.Tag;
                    state.Url = e.url;
                    RefreshDocumentState(doc);
                    owner.BackgroundDiscoverFeedsHandler.DiscoverFeedInContent(hc.DocumentInnerHTML, state.Url,
                                                                               state.Title);
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebDocumentComplete(): " + e.url, ex);
            }
        }

        private void OnWebTitleChanged(object sender, BrowserTitleChangeEvent e)
        {
            try
            {
                HtmlControl hc = (HtmlControl) sender;
                if (hc == null) return;
                DockControl doc = (DockControl) hc.Tag;
                if (doc == null) return;
                ITabState state = (ITabState) doc.Tag;
                if (state == null) return;

                state.Title = e.text;
                RefreshDocumentState(doc);
            }
            catch (Exception ex)
            {
                _log.Error("OnWebTitleChanged()", ex);
            }
        }

        private static void OnWebCommandStateChanged(object sender, BrowserCommandStateChangeEvent e)
        {
            try
            {
                ITabState state = GetTabStateFor(sender as HtmlControl);
                if (state == null) return;

                if (e.command == CommandStateChangeConstants.CSC_NAVIGATEBACK)
                    state.CanGoBack = e.enable;
                else if (e.command == CommandStateChangeConstants.CSC_NAVIGATEFORWARD)
                    state.CanGoForward = e.enable;
                else if (e.command == CommandStateChangeConstants.CSC_UPDATECOMMANDS)
                {
                    // 
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebCommandStateChanged() ", ex);
            }
        }

        private void OnWebNewWindow(object sender, BrowserNewWindowEvent e)
        {
            try
            {
                bool userNavigates = _webUserNavigated;
                bool forceNewTab = _webForceNewTab;

                _webForceNewTab = _webUserNavigated = false; // reset

                e.Cancel = true;

                string url = e.url;
                _log.Debug("OnWebNewWindow(): '" + url + "'");

                bool forceSetFocus = true;
                    // Tab in background, but IEControl does NOT display/render!!!    !(Interop.GetAsyncKeyState(Interop.VK_MENU) < 0);

                if (UrlRequestHandledExternally(url, forceNewTab))
                {
                    return;
                }

                if (userNavigates)
                {
                    // Delay gives time to the sender control to cancel request
                    DelayTask(DelayedTasks.NavigateToWebUrl, new object[] {url, null, true, forceSetFocus});
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebNewWindow(): " + e.url, ex);
            }
        }

        private void OnWebQuit(object sender, EventArgs e)
        {
            try
            {
                // javscript want to close this window: so we have to close the tab
                RemoveDocTab(_docContainer.ActiveDocument);
            }
            catch (Exception ex)
            {
                _log.Error("OnWebQuit()", ex);
            }
        }

        private void OnWebTranslateAccelerator(object sender, KeyEventArgs e)
        {
            try
            {
                // we use Control.ModifierKeys, because e.Shift etc. is not always set!
                bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                bool alt = (ModifierKeys & Keys.Alt) == Keys.Alt;
                bool noModifier = (!shift && !ctrl && ! alt);

                bool shiftOnly = (shift && !ctrl && !alt);
                bool ctrlOnly = (ctrl && !shift && !alt);
                bool ctrlShift = (ctrl && shift && !alt);

                if (_shortcutHandler.IsCommandInvoked("BrowserCreateNewTab", e.KeyData))
                {
                    // capture Ctrl-N event or whichever combination is configured (new window)
                    owner.CmdBrowserCreateNewTab(null);
                    e.Handled = true;
                }
                if (_shortcutHandler.IsCommandInvoked("Help", e.KeyData))
                {
                    // capture F1 (or whichever keys are configured) event (help)
                    Help.ShowHelp(this, helpProvider1.HelpNamespace, HelpNavigator.TableOfContents);
                    e.Handled = true;
                }

                if (!e.Handled)
                {
                    // prevent double handling of shortcuts:
                    // IE will handle this codes by itself even if a user configures other shortcuts
                    // than Ctrl-N and F1.
                    e.Handled = (e.KeyCode == Keys.N && ctrlOnly ||
                                 e.KeyCode == Keys.F1);
                }

                if (!e.Handled)
                {
                    // support: continue tab order throw the other controls than IEControl
                    if (e.KeyCode == Keys.Tab && noModifier)
                    {
                        if (htmlDetail.Document2 != null && null == htmlDetail.Document2.GetActiveElement())
                        {
                            // one turn around within ALink element classes
                            if (treeFeeds.Visible)
                            {
                                treeFeeds.Focus();
                                e.Handled = true;
                            }
                            else if (listFeedItems.Visible)
                            {
                                listFeedItems.Focus();
                                e.Handled = true;
                            }
                        }
                    }
                    else if (e.KeyCode == Keys.Tab && shiftOnly)
                    {
                        if (htmlDetail.Document2 != null && null == htmlDetail.Document2.GetActiveElement())
                        {
                            // one reverse turn around within ALink element classes
                            if (listFeedItems.Visible)
                            {
                                listFeedItems.Focus();
                                e.Handled = true;
                            }
                            else if (treeFeeds.Visible)
                            {
                                treeFeeds.Focus();
                                e.Handled = true;
                            }
                        }
                    }
                }

                if (!e.Handled)
                {
                    // support: Ctrl-Tab/Shift-Ctrl-Tab switch Browser Tabs
                    if (e.KeyCode == Keys.Tab && ctrlOnly)
                    {
                        // step forward:
                        if (_docContainer.Documents.Length > 1)
                        {
                            InvokeProcessCmdKey(_docContainer.ActiveDocument, Keys.Next | Keys.Control);
                            e.Handled = true;
                        }
                    }
                    else if (e.KeyCode == Keys.Tab && ctrlShift)
                    {
                        // step backward:
                        if (_docContainer.Documents.Length > 1)
                        {
                            InvokeProcessCmdKey(_docContainer.ActiveDocument, Keys.Prior | Keys.Control);
                            e.Handled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebTranslateAccelerator(): " + e.KeyCode, ex);
            }
        }

        private void InvokeProcessCmdKey(DockControl c, Keys keyData)
        {
            if (c != null)
            {
                Type cType = c.GetType();
                try
                {
                    // just a dummy message:
                    Message m = Message.Create(Handle, (int) Win32.Message.WM_NULL, IntPtr.Zero, IntPtr.Zero);
                    cType.InvokeMember("ProcessCmdKey",
                                           BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                                           null, c, new object[] {m, keyData});
                }
                catch (Exception ex)
                {
                    _log.Error("InvokeProcessCmdKey() failed: " + ex.Message);
                }
            }
        }

        private void OnWebProgressChanged(object sender, BrowserProgressChangeEvent e)
        {
            try
            {
                if (_lastBrowserThatProgressChanged == null)
                    _lastBrowserThatProgressChanged = sender;

                if (sender != _lastBrowserThatProgressChanged)
                {
                    DeactivateWebProgressInfo();
                    return;
                }

                if (((e.progress < 0) || (e.progressMax <= 0)) || (e.progress >= e.progressMax))
                {
                    DeactivateWebProgressInfo();
                }
                else
                {
                    if (!progressBrowser.Visible) progressBrowser.Visible = true;
                    if (statusBarBrowserProgress.Width < BrowserProgressBarWidth)
                    {
                        statusBarBrowserProgress.Width = BrowserProgressBarWidth;
                        progressBrowser.Width = BrowserProgressBarWidth - 12;
                    }
                    progressBrowser.Minimum = 0;
                    progressBrowser.Maximum = e.progressMax;
                    progressBrowser.Value = e.progress;
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebProgressChanged()", ex);
            }
        }

        private object _lastBrowserThatProgressChanged = null;

        private void DeactivateWebProgressInfo()
        {
            progressBrowser.Minimum = 0;
            progressBrowser.Maximum = 128;
            progressBrowser.Value = 128;
            progressBrowser.Visible = false;
            statusBarBrowserProgress.Width = 0;
            _lastBrowserThatProgressChanged = null;
        }

        #endregion

        #endregion

        #region Implementation of ITabState

        public bool CanClose
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public bool CanGoBack
        {
            get
            {
                return _feedItemImpressionHistory.CanGetPrevious &&
                       _feedItemImpressionHistory.Count > 1;
            }
            set
            {
            }
        }

        public bool CanGoForward
        {
            get
            {
                return _feedItemImpressionHistory.CanGetNext;
            }
            set
            {
            }
        }

        public string Title
        {
            get
            {
                if (CurrentSelectedFeedsNode != null)
                    return CurrentSelectedFeedsNode.Text;
                else
                    return String.Empty;
            }
            set
            {
                // nothing to implement here
            }
        }

        public string Url
        {
            get
            {
                return _tabStateUrl;
            }
            set
            {
                _tabStateUrl = value;
            }
        }

        public ITextImageItem[] GoBackHistoryItems(int maxItems)
        {
            return _feedItemImpressionHistory.GetHeadOfPreviousEntries(maxItems);
        }

        public ITextImageItem[] GoForwardHistoryItems(int maxItems)
        {
            return _feedItemImpressionHistory.GetHeadOfNextEntries(maxItems);
        }

        #endregion

        /// <summary>
        /// Called when any IG toolbar tool click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ToolClickEventArgs"/> instance containing the event data.</param>
        private void OnAnyToolbarToolClick(object sender, ToolClickEventArgs e)
        {
            // if we get a click on a state button, the new state (checked/unchecked) 
            // is yet applied! This is THE major diff. compared to Sandbar tools!
            owner.Mediator.Execute(e.Tool.Key);
        }

        //does nothing more than supress the beep if you press enter
        internal static void OnAnyEnterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && ModifierKeys == Keys.None)
                e.Handled = true; // supress beep
        }

        #region SearchPanel routines

        private void OnNewsItemSearchFinished(object sender, FeedSource.SearchFinishedEventArgs e)
        {
            InvokeOnGui(delegate
            {
                SearchFinishedAction(e.Tag, e.MatchingFeeds, e.MatchingItems, e.MatchingFeedsCount, e.MatchingItemsCount);
            });
        }

        private void OnSearchPanelStartNewsItemSearch(object sender, NewsItemSearchEventArgs e)
        {
            AsyncStartNewsSearch(e.FinderNode);
        }

        private void AsyncStartNewsSearch(FinderNode node)
        {
            StartNewsSearchDelegate start = StartNewsSearch;
            start.BeginInvoke(node, AsyncInvokeCleanup, start);
        }

        private void StartNewsSearch(FinderNode node)
        {
            owner.FeedHandler.SearchNewsItems(
                node.Finder.SearchCriterias,
                node.Finder.SearchScope,
                node.Finder,
                CultureInfo.CurrentUICulture.Name,
                node.Finder.ShowFullItemContent);
        }

        private void OnSearchPanelBeforeNewsItemSearch(object sender, NewsItemSearchCancelEventArgs e)
        {
            // set status text
            SetSearchStatusText(SR.RssSearchStateMessage);

            Exception criteriaValidationException;
            if (!FeedSource.SearchHandler.ValidateSearchCriteria(
                     e.SearchCriteria, CultureInfo.CurrentUICulture.Name,
                     out criteriaValidationException))
            {
                e.Cancel = true;
                throw criteriaValidationException;
            }

            // return a result container:
            FinderNode resultContainer;

            string newName = e.ResultContainerName;
            if (!string.IsNullOrEmpty(newName))
            {
                TreeFeedsNodeBase parent = GetRoot(RootFolderType.Finder);

                if (newName.IndexOf(FeedSource.CategorySeparator) > 0)
                {
                    string[] a = newName.Split(FeedSource.CategorySeparator.ToCharArray());
                    parent = TreeHelper.CreateCategoryHive(parent,
                                                           String.Join(FeedSource.CategorySeparator, a, 0, a.Length - 1),
                                                           _treeSearchFolderContextMenu,
                                                           FeedNodeType.FinderCategory);

                    newName = a[a.Length - 1].Trim();
                }

                TreeFeedsNodeBase feedsNode = TreeHelper.FindChildNode(parent, newName, FeedNodeType.Finder);
                if (feedsNode != null)
                {
                    resultContainer = (FinderNode) feedsNode;
                }
                else
                {
                    resultContainer =
                        new FinderNode(newName, Resource.SubscriptionTreeImage.SearchFolder,
                                       Resource.SubscriptionTreeImage.SearchFolderSelected, _treeSearchFolderContextMenu);
                    resultContainer.DataKey = resultContainer.InternalFeedLink;
                    parent.Nodes.Add(resultContainer);
                }
            }
            else
            {
                AddTempResultNode();
                resultContainer = _searchResultNode;
            }

            if (resultContainer.Finder == null)
            {
                //new search folder

                resultContainer.Finder = new RssFinder(resultContainer, e.SearchCriteria,
                                                       e.CategoryScope, e.FeedScope,
                                                       ScopeResolve, true);

                // not a temp result and not yet exist:
                if (resultContainer != _searchResultNode && !owner.FinderList.Contains(resultContainer.Finder))
                {
                    owner.FinderList.Add(resultContainer.Finder);
                }
            }
            else
            {
                //existing search folder
                resultContainer.Finder.SearchCriterias = e.SearchCriteria;
                resultContainer.Finder.SetSearchScope(e.CategoryScope, e.FeedScope);
                resultContainer.Finder.ExternalResultMerged = false;
                resultContainer.Finder.ExternalSearchPhrase = null;
                resultContainer.Finder.ExternalSearchUrl = null;
            }

            owner.SaveSearchFolders();

            resultContainer.Clear();
            UpdateTreeNodeUnreadStatus(resultContainer, 0);
            EmptyListView();
            htmlDetail.Clear();

            e.ResultContainer = resultContainer;
            TreeSelectedFeedsNode = resultContainer;
        }

        internal void SearchFinishedAction(object tag, FeedInfoList matchingFeeds, List<INewsItem> matchingItems,
                                           int rssFeedsCount, int newsItemsCount)
        {
            if (newsItemsCount == 0)
            {
                searchPanel.SetControlStateTo(ItemSearchState.Finished, SR.RssSearchNoResultMessage);
            }
            else
            {
                searchPanel.SetControlStateTo(ItemSearchState.Finished, SR.RssSearchSuccessResultMessage(newsItemsCount));
            }

            RssFinder finder = tag as RssFinder;
            FinderNode tn = (finder != null ? finder.Container : null);

            if (tn != null)
            {
                tn.AddRange(matchingItems);
                if (tn.Selected)
                {
                    PopulateListView(tn, matchingItems, false, true, tn);
                    if (!tn.Finder.ShowFullItemContent) // use summary stylesheet
                        BeginTransformFeedList(matchingFeeds, tn, NewsItemFormatter.SearchTemplateId);
                    else // use default stylesheet
                        BeginTransformFeedList(matchingFeeds, tn, owner.Stylesheet);
                }
            }
        }

        private INewsFeed[] ScopeResolve(ArrayList categories, ArrayList feedUrls)
        {
            if (categories == null && feedUrls == null)
                return new INewsFeed[] {};

            ArrayList result = new ArrayList();

            if (categories != null)
            {
                string sep = FeedSource.CategorySeparator;
                foreach (INewsFeed f in owner.FeedHandler.GetFeeds().Values)
                {
                    foreach (string category in categories)
                    {
                        if (f.category != null && (f.category.Equals(category) || f.category.StartsWith(category + sep)))
                        {
                            result.Add(f);
                        }
                    }
                }
            }

            if (feedUrls != null)
            {
                foreach (string url in feedUrls)
                {
                    if (url != null && owner.FeedHandler.IsSubscribed(url))
                    {
                        result.Add(owner.FeedHandler.GetFeeds()[url]);
                    }
                }
            }

            if (result.Count > 0)
            {
                INewsFeed[] fa = new INewsFeed[result.Count];
                result.CopyTo(fa);
                return fa;
            }

            return new INewsFeed[] {};
        }


        internal void CmdNewRssSearch(ICommand sender)
        {
            searchPanel.ResetControlState();
            PopulateTreeRssSearchScope();
            SetSearchStatusText(String.Empty);
            if (sender != null)
                CmdDockShowRssSearch(sender);
            searchPanel.SetFocus();
        }


        private void AddTempResultNode()
        {
            if (_searchResultNode == null)
            {
                _searchResultNode =
                    new TempFinderNode(SR.RssSearchResultNodeCaption, Resource.SubscriptionTreeImage.SearchFolder,
                                       Resource.SubscriptionTreeImage.SearchFolderSelected,
                                       _treeTempSearchFolderContextMenu);
                _searchResultNode.DataKey = _searchResultNode.InternalFeedLink;
                GetRoot(RootFolderType.SmartFolders).Nodes.Add(_searchResultNode);
            }
        }

        private void AsyncStartRssRemoteSearch(string searchPhrase, string searchUrl, bool mergeWithLocalResults,
                                               bool initialize)
        {
            AddTempResultNode();

            if (initialize)
            {
                _searchResultNode.Clear();
                EmptyListView();
                htmlDetail.Clear();
                TreeSelectedFeedsNode = _searchResultNode;
                SetSearchStatusText(SR.RssSearchStateMessage);
            }

            SearchCriteriaCollection scc = new SearchCriteriaCollection();
            if (mergeWithLocalResults)
            {
                // merge with local search result
                scc.Add(new SearchCriteriaString(searchPhrase, SearchStringElement.All, StringExpressionKind.Text));
            }
            RssFinder finder = new RssFinder(_searchResultNode, scc,
                                             null, null, ScopeResolve, true);
            finder.ExternalResultMerged = mergeWithLocalResults;
            finder.ExternalSearchPhrase = searchPhrase;
            finder.ExternalSearchUrl = searchUrl;

            _searchResultNode.Finder = finder;
            StartRssRemoteSearchDelegate start = StartRssRemoteSearch;
            start.BeginInvoke(searchUrl, _searchResultNode, AsyncInvokeCleanup, start);

            if (mergeWithLocalResults)
            {
                // start also the local search
                AsyncStartNewsSearch(_searchResultNode);
            }
        }

        private void StartRssRemoteSearch(string searchUrl, FinderNode resultContainer)
        {
            try
            {
                owner.FeedHandler.SearchRemoteFeed(searchUrl, resultContainer.Finder);
            }
            catch (Exception ex)
            {
                SetSearchStatusText("Search '" + StringHelper.ShortenByEllipsis(searchUrl, 30) + "' caused a problem: " +
                                    ex.Message);
            }
        }

        #endregion

        //used to generally get the EndInvoke() called for a gracefully cleanup and exception catching
        private static void AsyncInvokeCleanup(IAsyncResult ar)
        {
            StartNewsSearchDelegate startNewsSearchDelegate = ar.AsyncState as StartNewsSearchDelegate;
            if (startNewsSearchDelegate != null)
            {
                try
                {
                    startNewsSearchDelegate.EndInvoke(ar);
                }
                catch (Exception ex)
                {
                    _log.Error("AsyncCall 'StartNewsSearchDelegate' caused this exception", ex);
                }
                return;
            }

            StartRssRemoteSearchDelegate startRssRemoteSearchDelegate = ar.AsyncState as StartRssRemoteSearchDelegate;
            if (startRssRemoteSearchDelegate != null)
            {
                try
                {
                    startRssRemoteSearchDelegate.EndInvoke(ar);
                }
                catch (Exception ex)
                {
                    _log.Error("AsyncCall 'StartRssRemoteSearchDelegate' caused this exception", ex);
                }
                return;
            }

            GetCommentNewsItemsDelegate getCommentNewsItemsDelegate = ar.AsyncState as GetCommentNewsItemsDelegate;
            if (getCommentNewsItemsDelegate != null)
            {
                try
                {
                    getCommentNewsItemsDelegate.EndInvoke(ar);
                }
                catch (Exception ex)
                {
                    _log.Error("AsyncCall 'GetCommentNewsItemsDelegate' caused this exception", ex);
                }
                return;
            }
        }

        //toastNotify callbacks
        private void OnExternalDisplayFeedProperties(INewsFeed f)
        {
            DelayTask(DelayedTasks.ShowFeedPropertiesDialog, f);
        }

        private void OnExternalActivateFeedItem(INewsItem item)
        {
            DelayTask(DelayedTasks.NavigateToFeedNewsItem, item);
        }

        private void OnExternalActivateFeed(INewsFeed f)
        {
            DelayTask(DelayedTasks.NavigateToFeed, f);
        }

        private void DisplayFeedProperties(INewsFeed f)
        {
            TreeFeedsNodeBase tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), f);
            if (tn != null)
            {
                CurrentSelectedFeedsNode = tn;
                owner.CmdShowFeedProperties(null);
                CurrentSelectedFeedsNode = null;
            }
        }

        private void NavigateToNode(TreeFeedsNodeBase feedsNode)
        {
            NavigateToNode(feedsNode, null);
        }

        private void NavigateToNode(TreeFeedsNodeBase feedsNode, IEquatable<INewsItem> item)
        {
            // prevent adding new selected node/item to the history:
            _navigationActionInProgress = true;

            if (feedsNode != null && feedsNode.Control != null)
            {
                SelectNode(feedsNode);
                // populates listview items:
                OnTreeFeedAfterSelectManually(feedsNode); //??
                MoveFeedDetailsToFront();

                if (item != null)
                {
                    ThreadedListViewItem foundLVItem = null;

                    for (int i = 0; i < listFeedItems.Items.Count; i++)
                    {
                        ThreadedListViewItem lvi = listFeedItems.Items[i];
                        INewsItem ti = (INewsItem) lvi.Key;
                        if (item.Equals(ti))
                        {
                            foundLVItem = lvi;
                            break;
                        }
                    }

                    if (foundLVItem == null && listFeedItems.Items.Count > 0)
                    {
                        foundLVItem = listFeedItems.Items[0];
                    }

                    if (foundLVItem != null)
                    {
                        listFeedItems.BeginUpdate();
                        listFeedItems.SelectedItems.Clear();
                        foundLVItem.Selected = true;
                        foundLVItem.Focused = true;
                        OnFeedListItemActivate(null, EventArgs.Empty); //pass nulls because I don't use params
                        SetTitleText(feedsNode.Text);
                        SetDetailHeaderText(feedsNode);
                        foundLVItem.Selected = true;
                        foundLVItem.Focused = true;
                        listFeedItems.Focus();
                        listFeedItems.EnsureVisible(foundLVItem.Index);
                        listFeedItems.EndUpdate();
                    }
                }
            }

            owner.CmdShowMainGui(null);
            _navigationActionInProgress = false;
        }

        internal void NavigateToFeed(INewsFeed f)
        {
            NavigateToNode(TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), f));
        }

        private void NavigateToFeedNewsItem(INewsItem item)
        {
            NavigateToNode(TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), item), item);
        }

        private void NavigateToHistoryEntry(HistoryEntry historyEntry)
        {
            if (historyEntry != null)
            {
                if (historyEntry.Node != null)
                {
                    NavigateToNode(historyEntry.Node, historyEntry.Item);
                }
                else
                {
                    NavigateToFeedNewsItem(historyEntry.Item);
                }
            }
        }

        private void AddHistoryEntry(TreeFeedsNodeBase feedsNode, INewsItem item)
        {
            if (feedsNode == null && item == null) return;
            if (_navigationActionInProgress) return; // back/forward,... pressed
            _feedItemImpressionHistory.Add(new HistoryEntry(feedsNode, item));
        }

        private void AutoSubscribeFeed(TreeFeedsNodeBase parent, string feedUrl)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return;

            if (parent == null)
                parent = GetRoot(RootFolderType.MyFeeds);

            string feedTitle = null;
            string category = parent.CategoryStoreName;

            string[] urls = feedUrl.Split(Environment.NewLine.ToCharArray());
            bool multipleSubscriptions = (urls.Length > 1);

            for (int i = 0; i < urls.Length; i++)
            {
                feedUrl = owner.HandleUrlFeedProtocol(urls[i]);

                try
                {
                    try
                    {
                        Uri reqUri = new Uri(feedUrl);
                        feedTitle = reqUri.Host;
                    }
                    catch (UriFormatException)
                    {
                        feedTitle = feedUrl;
                        if (!feedUrl.ToLower().StartsWith("http://"))
                        {
                            feedUrl = "http://" + feedUrl;
                        }
                    }

                    if (!multipleSubscriptions && owner.FeedHandler.IsSubscribed(feedUrl))
                    {
                        INewsFeed f2 = owner.FeedHandler.GetFeeds()[feedUrl];
                        owner.MessageInfo(SR.GUIFieldLinkRedundantInfo(
                                              (f2.category == null
                                                   ? String.Empty
                                                   : f2.category + FeedSource.CategorySeparator) + f2.title, f2.link));
                        return;
                    }

                    if (owner.InternetAccessAllowed)
                    {
                        PrefetchFeedThreadHandler fetchHandler = new PrefetchFeedThreadHandler(feedUrl, owner.Proxy);

                        DialogResult result = fetchHandler.Start(this, SR.GUIStatusWaitMessagePrefetchFeed(feedUrl));

                        if (result != DialogResult.OK)
                            return;

                        if (!fetchHandler.OperationSucceeds)
                        {
                            _log.Error("AutoSubscribeFeed() caused exception", fetchHandler.OperationException);
                        }
                        else
                        {
                            if (fetchHandler.DiscoveredDetails != null)
                            {
                                if (fetchHandler.DiscoveredDetails.Title != null)
                                    feedTitle = HtmlHelper.HtmlDecode(fetchHandler.DiscoveredDetails.Title);

                                // setup the new feed magically, and add them to the parent node
                                INewsFeed f = fetchHandler.DiscoveredFeed;
                                f.link = feedUrl;
                                f.title = feedTitle;
                                f.refreshrate = 60;
                                //f.storiesrecentlyviewed = new ArrayList(); 				
                                //f.deletedstories = new ArrayList(); 				
                                if (!owner.FeedHandler.HasCategory(category))
                                {
                                    owner.FeedHandler.AddCategory(category);
                                }
                                owner.FeedHandler.ChangeCategory(f, category);                               

                                f.alertEnabled = false;
                                f = owner.FeedHandler.AddFeed(f);
                                owner.FeedWasModified(f, NewsFeedProperty.FeedAdded);
                                //owner.FeedlistModified = true;

                                AddNewFeedNode(f.category, f);

                                try
                                {
                                    owner.FeedHandler.AsyncGetItemsForFeed(f.link, true, true);
                                }
                                catch (Exception e)
                                {
                                    owner.PublishXmlFeedError(e, f.link, true);
                                }

                                if (!multipleSubscriptions)
                                    return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("AutoSubscribeFeed() caused exception", ex);
                }
            }
            // no discovered details, or
            // Exception caused, was yet report to user, or
            // No Internet access allowed
            if (!multipleSubscriptions)
                owner.CmdNewFeed(category, feedUrl, feedTitle);
        }

        private void OnOwnerFeedlistLoaded(object sender, EventArgs e)
        {
            listFeedItems.FeedColumnLayout = owner.GlobalFeedColumnLayout;
            LoadAndRestoreSubscriptionTreeState();
            if (Visible)
            {
                LoadAndRestoreBrowserTabState();
            }
            _startupTimer.Interval = 1000 * RssBanditApplication.ReadAppSettingsEntry("ForcedRefreshOfFeedsAtStartupDelay.Seconds", 30);
                // wait 30 secs
            _startupTimer.Enabled = true;
        }


        private Control OnWheelSupportGetChildControl(Control control)
        {
            if (control == _docContainer)
            {
                if (_docContainer.ActiveDocument != null && _docContainer.ActiveDocument != _docFeedDetails)
                    return _docContainer.ActiveDocument.Controls[0]; // continue within docmananger hierarchy
            }
            return null;
        }

        private void OnFeedItemImpressionHistoryStateChanged(object sender, EventArgs e)
        {
            RefreshDocumentState(_docContainer.ActiveDocument);
        }

        private void OnPreferencesChanged(object sender, EventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                if (listFeedItems.ShowAsThreads != owner.Preferences.BuildRelationCosmos)
                                {
                                    if (owner.Preferences.BuildRelationCosmos)
                                    {
                                        listFeedItems.ShowAsThreads = true;
                                    }
                                    else
                                    {
                                        //listFeedItems.ShowAsThreads = false;
                                        listFeedItems.ShowInGroups = false;
                                        listFeedItems.AutoGroupMode = false;
                                    }
                                }

                                SetFontAndColor(
                                    owner.Preferences.NormalFont, owner.Preferences.NormalFontColor,
                                    owner.Preferences.UnreadFont, owner.Preferences.UnreadFontColor,
                                    owner.Preferences.FlagFont, owner.Preferences.FlagFontColor,
                                    owner.Preferences.ErrorFont, owner.Preferences.ErrorFontColor,
                                    owner.Preferences.RefererFont, owner.Preferences.RefererFontColor,
                                    owner.Preferences.NewCommentsFont, owner.Preferences.NewCommentsFontColor
                                    );

                                owner.Mediator.SetEnabled(owner.Preferences.UseRemoteStorage, "cmdUploadFeeds",
                                                          "cmdDownloadFeeds");

                                if (Visible)
                                {
                                    // initiate a refresh of the NewsItem detail pane
                                    OnFeedListItemActivate(this, EventArgs.Empty);

                                    if (CurrentSelectedFeedsNode != null)
                                    {
                                        CurrentSelectedFeedsNode.Control.SelectedNodes.Clear();
                                        CurrentSelectedFeedsNode.Selected = true;
                                        CurrentSelectedFeedsNode.Control.ActiveNode = CurrentSelectedFeedsNode;

                                        if (NumSelectedListViewItems == 0)
                                        {
                                            //** there isn't any more the "TreeViewAction.Unknown" attribute. before: this.OnTreeFeedAfterSelectma(this, new TreeViewEventArgs(this.CurrentSelectedNode, TreeViewAction.Unknown));
                                            OnTreeFeedAfterSelectManually(CurrentSelectedFeedsNode);
                                        }
                                    }
                                }
                            });
        }

        private void OnFeedDeleted(object sender, FeedDeletedEventArgs e)
        {
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;

            ExceptionNode.UpdateReadStatus();
            PopulateSmartFolder((TreeFeedsNodeBase) ExceptionNode, false);

            if (tn == null || tn.Type != FeedNodeType.Feed || e.FeedUrl != tn.DataKey)
            {
                tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), e.FeedUrl);
            }

            if (tn == null || tn.Type != FeedNodeType.Feed || e.FeedUrl != tn.DataKey)
            {
                return;
            }

            UpdateTreeNodeUnreadStatus(tn, 0);
            UnreadItemsNodeRemoveItems(e.FeedUrl);

            if (tn.Selected)
            {
                // not just right-clicked elsewhere on a node:
                EmptyListView();
                htmlDetail.Clear();

                TreeSelectedFeedsNode = TreeHelper.GetNewNodeToActivate(tn);
                RefreshFeedDisplay(TreeSelectedFeedsNode, true);

//				this.treeFeeds.ActiveNode.Selected = true; 
//				this.CurrentSelectedFeedsNode = this.treeFeeds.ActiveNode as TreeFeedsNodeBase;
//				this.RefreshFeedDisplay(this.CurrentSelectedFeedsNode, true); 				
            }

            try
            {
                tn.DataKey = null;
                // next line causes OnTreeBefore-/AfterSelected events:
                tn.Parent.Nodes.Remove(tn);
                //CurrentSelectedFeedsNode = null;
                DelayTask(DelayedTasks.SyncRssSearchTree);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Callback for DelayedTasks timer
        /// </summary>
        private void OnTasksTimerTick(object sender, EventArgs e)
        {
            if (_uiTasksTimer[DelayedTasks.SyncRssSearchTree])
            {
                _uiTasksTimer.StopTask(DelayedTasks.SyncRssSearchTree);
                PopulateTreeRssSearchScope();
            }

            if (_uiTasksTimer[DelayedTasks.RefreshTreeUnreadStatus])
            {
                _uiTasksTimer.StopTask(DelayedTasks.RefreshTreeUnreadStatus);
                object[] param = (object[]) _uiTasksTimer.GetData(DelayedTasks.RefreshTreeUnreadStatus, true);
                TreeFeedsNodeBase tn = (TreeFeedsNodeBase) param[0];
                int counter = (int) param[1];
                if (tn != null)
                    UpdateTreeNodeUnreadStatus(tn, counter);
            }

            if (_uiTasksTimer[DelayedTasks.RefreshTreeCommentStatus])
            {
                _uiTasksTimer.StopTask(DelayedTasks.RefreshTreeCommentStatus);
                object[] param = (object[]) _uiTasksTimer.GetData(DelayedTasks.RefreshTreeCommentStatus, true);
                TreeFeedsNodeBase tn = (TreeFeedsNodeBase) param[0];
                IList<INewsItem> items = (IList<INewsItem>) param[1];
                bool commentsRead = (bool) param[2];
                if (tn != null)
                    UpdateCommentStatus(tn, items, commentsRead);
            }

            if (_uiTasksTimer[DelayedTasks.NavigateToWebUrl])
            {
                _uiTasksTimer.StopTask(DelayedTasks.NavigateToWebUrl);
                object[] param = (object[]) _uiTasksTimer.GetData(DelayedTasks.NavigateToWebUrl, true);
                DetailTabNavigateToUrl((string) param[0], (string) param[1], (bool) param[2], (bool) param[3]);
            }

            if (_uiTasksTimer[DelayedTasks.StartRefreshOneFeed])
            {
                _uiTasksTimer.StopTask(DelayedTasks.StartRefreshOneFeed);
                string feedUrl = (string) _uiTasksTimer.GetData(DelayedTasks.StartRefreshOneFeed, true);
                owner.FeedHandler.AsyncGetItemsForFeed(feedUrl, true, true);
            }

            if (_uiTasksTimer[DelayedTasks.SaveUIConfiguration])
            {
                _uiTasksTimer.StopTask(DelayedTasks.SaveUIConfiguration);
                SaveUIConfiguration(true);
            }

            if (_uiTasksTimer[DelayedTasks.ShowFeedPropertiesDialog])
            {
                _uiTasksTimer.StopTask(DelayedTasks.ShowFeedPropertiesDialog);
                INewsFeed f = (INewsFeed) _uiTasksTimer.GetData(DelayedTasks.ShowFeedPropertiesDialog, true);
                DisplayFeedProperties(f);
            }

            if (_uiTasksTimer[DelayedTasks.NavigateToFeedNewsItem])
            {
                _uiTasksTimer.StopTask(DelayedTasks.NavigateToFeedNewsItem);
                INewsItem item = (INewsItem) _uiTasksTimer.GetData(DelayedTasks.NavigateToFeedNewsItem, true);
                NavigateToFeedNewsItem(item);
            }

            if (_uiTasksTimer[DelayedTasks.NavigateToFeed])
            {
                _uiTasksTimer.StopTask(DelayedTasks.NavigateToFeed);
                INewsFeed f = (INewsFeed) _uiTasksTimer.GetData(DelayedTasks.NavigateToFeed, true);
                NavigateToFeed(f);
            }

            if (_uiTasksTimer[DelayedTasks.AutoSubscribeFeedUrl])
            {
                _uiTasksTimer.StopTask(DelayedTasks.AutoSubscribeFeedUrl);
                object[] parameter = (object[]) _uiTasksTimer.GetData(DelayedTasks.AutoSubscribeFeedUrl, true);
                AutoSubscribeFeed((TreeFeedsNodeBase) parameter[0], (string) parameter[1]);
            }

            if (_uiTasksTimer[DelayedTasks.ClearBrowserStatusInfo])
            {
                _uiTasksTimer.StopTask(DelayedTasks.ClearBrowserStatusInfo);
                SetBrowserStatusBarText(String.Empty);
                DeactivateWebProgressInfo();
                _uiTasksTimer.Interval = 100; // reset interval 
            }

            if (_uiTasksTimer[DelayedTasks.InitOnFinishLoading])
            {
                _uiTasksTimer.StopTask(DelayedTasks.InitOnFinishLoading);
                OnFinishLoading();
            }

            if (!_uiTasksTimer.AllTaskDone)
            {
                if (!_uiTasksTimer.Enabled)
                    _uiTasksTimer.Start();
            }
            else
            {
                if (_uiTasksTimer.Enabled)
                    _uiTasksTimer.Stop();
            }
        }

        private void OnTreeNodeFeedsRootReadCounterZero(object sender, EventArgs e)
        {
            ResetFindersReadStatus();
            SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
        }

        public void DelayTask(DelayedTasks task)
        {
            DelayTask(task, null, 100);
        }

        public void DelayTask(DelayedTasks task, object data)
        {
            DelayTask(task, data, 100);
        }

        public void DelayTask(DelayedTasks task, object data, int interval)
        {
            GuiInvoker.InvokeAsync(this, delegate
            {
                _uiTasksTimer.SetData(task, data);
                if (_uiTasksTimer.Interval != interval)
                    _uiTasksTimer.Interval = interval;
                _uiTasksTimer.StartTask(task);
            });
        }

        public void StopTask(DelayedTasks task)
        {
            _uiTasksTimer.StopTask(task);
        }

        #region private helper classes

        private class UITaskTimer : Timer
        {
            private readonly object SynRoot = new object();
            private DelayedTasks tasks;
            private readonly Dictionary<DelayedTasks, object> taskData = new Dictionary<DelayedTasks, object>(7);

            public UITaskTimer(IContainer component) : base(component)
            {
                base.Enabled = true;
            }

            public bool this[DelayedTasks task]
            {
                get
                {
                    lock (SynRoot)
                    {
                        if ((tasks & task) == task)
                            return true;
                        return false;
                    }
                }
                set
                {
                    lock (SynRoot)
                    {
                        if (value)
                            tasks |= task;
                        else
                            tasks ^= task;
                    }
                }
            }

            public void StartTask(DelayedTasks task)
            {
                lock (SynRoot)
                {
                    this[task] = true;
                    //if (!base.Enabled)
                    Stop();
                    Start();
                }
            }

            public void StopTask(DelayedTasks task)
            {
                lock (SynRoot)
                {
                    this[task] = false;
                    if (AllTaskDone && base.Enabled)
                        Stop();
                }
            }

            public bool AllTaskDone
            {
                get
                {
                    lock (SynRoot)
                    {
                        return (tasks == DelayedTasks.None);
                    }
                }
            }



            public object GetData(DelayedTasks task, bool clear)
            {
                object data = null;
                lock (SynRoot)
                {
                    if (taskData.ContainsKey(task))
                    {
                        data = taskData[task];
                        if (clear)
                            taskData.Remove(task);
                    }
                }
                return data;
            }

            public void SetData(DelayedTasks task, object data)
            {
                lock (SynRoot)
                {
                    if (taskData.ContainsKey(task))
                        taskData.Remove(task);
                    taskData.Add(task, data);
                }
            }
        }

        #endregion

        #region ICommandBarImplementationSupport

        public CommandBar GetToolBarInstance(string id)
        {
            throw new NotImplementedException();
        }

        public CommandBar GetMenuBarInstance(string id)
        {
            throw new NotImplementedException();
        }

        public CommandBar AddToolBar(string id)
        {
            throw new NotImplementedException();
        }

        public CommandBar AddMenuBar(string id)
        {
            throw new NotImplementedException();
        }

        public CommandBar GetContexMenuInstance(string id)
        {
            throw new NotImplementedException();
        }

        public CommandBar AddContextMenu(string id)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void OnNavigatorCollapseClick(object sender, EventArgs e)
        {
            splitterNavigator.Hide();
            Navigator.Hide();
            pNavigatorCollapsed.Show();
            owner.Mediator.SetChecked("-cmdToggleTreeViewState");
            owner.Mediator.SetChecked("-cmdToggleRssSearchTabState");
        }

        private void OnNavigatorExpandImageClick(object sender, EventArgs e)
        {
            Navigator.Show();
            pNavigatorCollapsed.Hide();
            splitterNavigator.Show();
            OnNavigatorGroupClick(null, null);
        }

        private void OnNavigatorGroupClick(object sender, GroupEventArgs e)
        {
            if (Navigator.Visible)
            {
                // also raised by OnNavigatorCollapseClick (via GroupHeaderClick)!
                if (Navigator.SelectedGroup.Key == Resource.NavigatorGroup.Subscriptions)
                {
                    owner.Mediator.SetChecked("+cmdToggleTreeViewState");
                    owner.Mediator.SetChecked("-cmdToggleRssSearchTabState");
                }
                else if (Navigator.SelectedGroup.Key == Resource.NavigatorGroup.RssSearch)
                {
                    owner.Mediator.SetChecked("-cmdToggleTreeViewState");
                    owner.Mediator.SetChecked("+cmdToggleRssSearchTabState");
                }
            }
        }

        private void OnListFeedItemsO_AfterSelect(object sender, SelectEventArgs e)
        {
            if (listFeedItemsO.IsUpdatingSelection)
                return;
            //
            listFeedItemsO.IsUpdatingSelection = true;
            ArrayList groupSelected = null;
            //Select same nodes in listFeedItems ListView
            listFeedItems.SelectedItems.Clear();
            for (int i = 0; i < listFeedItemsO.Nodes.Count; i++)
            {
                if (listFeedItemsO.Nodes[i].Selected)
                {
                    if (groupSelected == null)
                        groupSelected = new ArrayList();
                    //Select all child nodes
                    for (int j = 0; j < listFeedItemsO.Nodes[i].Nodes.Count; j++)
                    {
                        UltraTreeNodeExtended node = (UltraTreeNodeExtended) listFeedItemsO.Nodes[i].Nodes[j];
                        //node.NodeOwner.Selected = true;
                        if (node.NewsItem != null && !node.NewsItem.BeenRead)
                        {
                            groupSelected.Add(node.NewsItem);
                        }
                    }
                }
                for (int j = 0; j < listFeedItemsO.Nodes[i].Nodes.Count; j++)
                {
                    if (listFeedItemsO.Nodes[i].Nodes[j].Selected)
                    {
                        UltraTreeNodeExtended node = (UltraTreeNodeExtended) listFeedItemsO.Nodes[i].Nodes[j];
                        if (node.NodeOwner != null)
                            node.NodeOwner.Selected = true;
                    }
                    //Comments
                    for (int k = 0; k < listFeedItemsO.Nodes[i].Nodes[j].Nodes.Count; k++)
                    {
                        if (listFeedItemsO.Nodes[i].Nodes[j].Nodes[k].Selected)
                        {
                            UltraTreeNodeExtended node =
                                (UltraTreeNodeExtended) listFeedItemsO.Nodes[i].Nodes[j].Nodes[k];
                            if (node.NodeOwner != null)
                            {
                                node.NodeOwner.Selected = true;
                            }
                        }
                    }
                }
            }
            //
            listFeedItemsO.IsUpdatingSelection = false;
            if (groupSelected == null)
            {
                OnFeedListItemActivate(null, EventArgs.Empty);
            }
            else
            {
                TreeFeedsNodeBase tn = TreeSelectedFeedsNode;
                if (tn != null)
                {
                    if (tn.Type == FeedNodeType.Category)
                    {
                        string category = tn.CategoryStoreName;
                        Hashtable temp = new Hashtable();

                        foreach (INewsItem item in groupSelected)
                        {
                            IFeedDetails fi;
                            if (temp.ContainsKey(item.Feed.link))
                            {
                                fi = (IFeedDetails)temp[item.Feed.link];
                            }
                            else
                            {
                                fi = (IFeedDetails)item.FeedDetails.Clone();
                                fi.ItemsList.Clear();
                                temp.Add(item.Feed.link, fi);
                            }
                            fi.ItemsList.Add(item);
                        }

                        FeedInfoList redispItems = new FeedInfoList(category);

                        foreach (IFeedDetails fi in temp.Values)
                        {
                            if (fi.ItemsList.Count > 0)
                                redispItems.Add(fi);
                        }

                        BeginTransformFeedList(redispItems, tn, owner.FeedHandler.GetCategoryStyleSheet(category));
                    }
                    else
                    {
                        string feedUrl = tn.DataKey;
                        IFeedDetails fi = owner.FeedHandler.GetFeedDetails(feedUrl);

                        if (fi != null)
                        {
                            IFeedDetails fi2;
                            fi2 = (IFeedDetails)fi.Clone();
                            fi2.ItemsList.Clear();
                            foreach (INewsItem ni in groupSelected)
                            {
                                fi2.ItemsList.Add(ni);
                            }
                            BeginTransformFeed(fi2, tn, owner.FeedHandler.GetStyleSheet(tn.DataKey));
                        }
                    }
                }
            }
        }

        private void listFeedItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listFeedItemsO.IsUpdatingSelection)
                return;
            //
            if (listFeedItemsO.Visible)
            {
                listFeedItemsO.IsUpdatingSelection = true;
                //
                listFeedItemsO.SelectedNodes.Clear();
                for (int i = 0; i < listFeedItems.Items.Count; i++)
                {
                    if (listFeedItems.Items[i].Selected)
                    {
                        UltraTreeNodeExtended n = listFeedItemsO.GetFromLVI(listFeedItems.Items[i]);
                        if (n != null)
                        {
                            n.Selected = true;
                        }
                    }
                }
                //
                listFeedItemsO.IsUpdatingSelection = false;
            }
        }

        private void OnListFeedItemsO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (listFeedItemsO.SelectedNodes.Count == 0)
                    return;

                UltraTreeNodeExtended n = (UltraTreeNodeExtended) listFeedItemsO.SelectedNodes[0];
                if (n.Level == 1 && n.NewsItem != null)
                {
                    DetailTabNavigateToUrl(n.NewsItem.Link, null, true, true);
                }
            }
        }

        private void listFeedItemsO_BeforeExpand(object sender, CancelableNodeEventArgs e)
        {
            UltraTreeNodeExtended n = (UltraTreeNodeExtended) e.TreeNode;
            if (n.Level == 1)
            {
                if (n.NewsItem != null && n.NewsItem.CommentCount > 0)
                {
                    //Expand Comments Nodes
                    ThreadedListViewItem lvi = n.NodeOwner;
                    lvi.Expanded = true;
                    //
                    listFeedItemsO.AddCommentUpdating(lvi);
                    //listFeedItemsO.AddRangeComments(lvi, items);
                }
            }
        }


        private void listFeedItemsO_MouseDown(object sender, MouseEventArgs e)
        {
            UltraTreeNodeExtended n = (UltraTreeNodeExtended) listFeedItemsO.GetNodeFromPoint(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {
                if (n != null)
                {
                    if (!n.CollapseRectangle.IsEmpty && n.CollapseRectangle.Contains(e.X, e.Y))
                    {
                        //Click on collapse/expand icon 
                        n.Expanded = !n.Expanded;
                    }
                    if (!n.EnclosureRectangle.IsEmpty && n.EnclosureRectangle.Contains(e.X, e.Y))
                    {
                        //Click on Enclosure
                    }
                    if (!n.CommentsRectangle.IsEmpty && n.CommentsRectangle.Contains(e.X, e.Y))
                    {
                        //Click on Comment
                        if ((n.NewsItem != null) && (!string.IsNullOrEmpty(n.NewsItem.CommentRssUrl)))
                        {
                            n.Expanded = !n.Expanded;
                            listFeedItemsO_BeforeExpand(sender, new CancelableNodeEventArgs(n));
                        }
                    }
                    if (!n.FlagRectangle.IsEmpty && n.FlagRectangle.Contains(e.X, e.Y))
                    {
                        //Click on Flag
                        if (n.NewsItem != null)
                        {
                            ToggleItemFlagState(n.NewsItem.Id);
                        }
                    }
                }
                else
                {
                    /* column click */
                }

                if (listFeedItemsO.ItemsCount() <= 0)
                    return;

                if (n != null && e.Clicks > 1)
                {
                    //DblClick
                    INewsItem item = CurrentSelectedFeedItem = n.NewsItem;
                    n.Selected = true;

                    if (item != null && !string.IsNullOrEmpty(item.Link))
                    {
                        if (!UrlRequestHandledExternally(item.Link, false))
                            DetailTabNavigateToUrl(item.Link, null, false, true);
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (n != null)
                {
                    n.Selected = true;
                    listFeedItemsO.ActiveNode = n;
                }

                RefreshListviewContextMenu();
                if (n != null && n.NodeOwner != null)
                    OnFeedListItemActivateManually(n.NodeOwner);
            }
        }

        private void listFeedItemsO_AfterSortChange(object sender, AfterSortChangeEventArgs e)
        {
            UltraTreeNode n = listFeedItemsO.ActiveNode;
            if (n == null)
                n = listFeedItemsO.TopNode;
            // update the view after sort:
            if (n != null)
                n.BringIntoView(true);
        }

        private void OnMediatorBeforeCommandStateChanged(object sender, EventArgs e)
        {
            ultraToolbarsManager.EventManager.AllEventsEnabled = false;
        }

        private void OnMediatorAfterCommandStateChanged(object sender, EventArgs e)
        {
            ultraToolbarsManager.EventManager.AllEventsEnabled = true;
        }
    } // end class WinGuiMain
}