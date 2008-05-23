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


// docking panels, docked tabs
// external webbrowser control
// related interfaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.ThListView;
using IEControl;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinToolTip;
using Infragistics.Win.UltraWinTree;
using log4net;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Search;
using NewsComponents.Utils;
using RssBandit.AppServices;
using RssBandit.Common.Logging;
using RssBandit.Filter;
using RssBandit.Resources;
using RssBandit.Utility.Keyboard;
using RssBandit.WebSearch;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Forms.ControlHelpers;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Utility;
using Syndication.Extensibility;
using TD.SandDock;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;

using K = RssBandit.Utility.Keyboard;
using Timer=System.Windows.Forms.Timer;
using ToolTip=System.Windows.Forms.ToolTip;


namespace RssBandit.WinGui.Forms
{
    /// <summary>
    /// Summary description for WinGuiMain.
    /// </summary>
    //[CLSCompliant(true)]
    internal partial class WinGuiMain : Form,
                                ITabState, IMessageFilter, ICommandBarImplementationSupport
    {
  
        // there is really no such thing on the native form interface :-(
        public event EventHandler OnMinimize;


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
        private const int _currentToolbarsVersion = 11;

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
        private bool _navigationActionInProgress;

        private bool _faviconsDownloaded;
        private bool _browserTabsRestored;

        private ToastNotifier toastNotifier;
        private WheelSupport wheelSupport;
        internal UrlCompletionExtender urlExtender;
        private NavigatorHeaderHelper navigatorHeaderHelper;
        private ToolbarHelper toolbarHelper;
        internal HistoryMenuManager historyMenuManager;

        private readonly TreeFeedsNodeBase[] _roots = new TreeFeedsNodeBase[3];
        // store refs to root folders (they order within the treeview may be resorted depending on the languages)

        private TreeFeedsNodeBase _unreadItemsFeedsNode;
        private TreeFeedsNodeBase _feedExceptionsFeedsNode;
        private TreeFeedsNodeBase _sentItemsFeedsNode;
        private TreeFeedsNodeBase _watchedItemsFeedsNode;
        private TreeFeedsNodeBase _deletedItemsFeedsNode;
        private TreeFeedsNodeBase _flaggedFeedsNodeRoot;
        private TreeFeedsNodeBase _flaggedFeedsNodeFollowUp;
        private TreeFeedsNodeBase _flaggedFeedsNodeRead;
        private TreeFeedsNodeBase _flaggedFeedsNodeReview;
        private TreeFeedsNodeBase _flaggedFeedsNodeForward;
        private TreeFeedsNodeBase _flaggedFeedsNodeReply;
        private FinderNode _searchResultNode;

        //private TreeFeedsPainter _treeFeedsPainter = null;

        //private ListViewSortHelper _lvSortHelper = null;
        private NewsItemFilterManager _filterManager;

        private SearchPanel searchPanel;

        private IList<IBlogExtension> blogExtensions;

        // store the HashCodes of temp. NewsItems, that have bean read.
        // Used for commentRss implementation
        private readonly Dictionary<string, object> tempFeedItemsRead = new Dictionary<string, object>();

        //Stores Image object for favicons so we can reuse the same object if used by
        //multiple feeds. 
        private readonly Dictionary<string, Image> _favicons = new Dictionary<string, Image>();

        // used to store temp. the currently yet populated feeds to speedup category population
        private readonly Dictionary<string, object> feedsCurrentlyPopulated = new Dictionary<string, object>();

        private ShortcutHandler _shortcutHandler;

        private INewsItem _currentNewsItem;

        private TreeFeedsNodeBase _currentSelectedFeedsNode;
        // currently selected node at the treeView (could be also temp. change on Right-Click, so it is different from treeView.SelectedNode )

        private TreeFeedsNodeBase _currentDragHighlightFeedsNode;

        //used for storing current news items to display in reading pane. We store them 
        //in member variables for quick access as part of newspaper paging implementation.
        private FeedInfo _currentFeedNewsItems;
        private FeedInfoList _currentCategoryNewsItems;
        private int _currentPageNumber = 1;
        private int _lastPageNumber = 1;

        // used to save last used window size to be restored after System Tray Mode:
        private Rectangle _formRestoreBounds = Rectangle.Empty;

        //these are here, because we only want to display the balloon popup,
        //if there are really new items received:
        private int _lastUnreadFeedItemCountBeforeRefresh;

        private int _beSilentOnBalloonPopupCounter;
        // if a user explicitly close the balloon, we are silent for the next 12 retries (we refresh all 5 minutes, so this equals at least to one hour)

        private bool _forceShutdown;
        private readonly FormWindowState initialStartupState = FormWindowState.Normal;

        private int _webTabCounter;

        // variables set in PreFilterMessage() to indicate the user clicked an url; reset also there on mouse-move, or within WebBeforeNavigate event
        private bool _webUserNavigated;
        private bool _webForceNewTab;
        private Point _lastMousePosition = Point.Empty;

        // GUI main components:
        private ImageList _allToolImages;
        private ImageList _treeImages;
        private ImageList _listImages;

        private MenuItem _feedInfoContextMenu;
        private ContextMenu _treeRootContextMenu;
        private ContextMenu _treeCategoryContextMenu;
        private ContextMenu _treeFeedContextMenu;
        private ContextMenu _notifyContextMenu;
        private ContextMenu _listContextMenu;
        private ContextMenu _treeLocalFeedContextMenu;
        private ContextMenu _treeSearchFolderRootContextMenu;
        private ContextMenu _treeSearchFolderContextMenu;
        private ContextMenu _treeTempSearchFolderContextMenu;
        private ContextMenu _docTabContextMenu;

        // Used to temp. store the context menu position. Processed later within
        // ICommand event receiver (in Screen-Coordinates).
        private Point _contextMenuCalledAt = Point.Empty;

        private AppContextMenuCommand _listContextMenuDownloadAttachment;
        private MenuItem _listContextMenuDeleteItemsSeparator;
        private MenuItem _listContextMenuDownloadAttachmentsSeparator;

        private TrayStateManager _trayManager;
        private NotifyIconAnimation _trayAni;

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
            InvokeOnGuiSync = a => GuiInvoker.Invoke(this, a);

            InvokeOnGui = a => GuiInvoker.InvokeAsync(this, a);

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


        #endregion

        #region public properties/accessor routines

        /// <summary>
        /// Returns the current page number in the reading pane. 
        /// </summary>
        public int CurrentPageNumber
        {
            get { return _currentPageNumber; }
        }

        /// <summary>
        /// Returns the page number for the last page in the reading pane. 
        /// </summary>
        public int LastPageNumber
        {
            get { return _lastPageNumber; }
        }

        /// <summary>
        /// Gets and sets the GUI owner application
        /// </summary>
        public RssBanditApplication GuiOwner
        {
            get { return owner; }
            set { owner = value; }
        }

        /// <summary>
        /// Initialized on Class init with the initial Form state (usually defined on a Shortcut)
        /// </summary>
        public FormWindowState InitialStartupState
        {
            get { return initialStartupState; }
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
            get { return _feedExceptionsFeedsNode as ISmartFolder; }
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
            get { return _sentItemsFeedsNode as ISmartFolder; }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the Watched Items.
        /// </summary>
        public ISmartFolder WatchedItemsNode
        {
            get { return _watchedItemsFeedsNode as ISmartFolder; }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the Unread Items.
        /// </summary>
        /// <value>The unread items node.</value>
        public ISmartFolder UnreadItemsNode
        {
            get { return _unreadItemsFeedsNode as ISmartFolder; }
        }

        /// <summary>
        /// Return the TreeNode instance representing/store of 
        /// the Deleted Items.
        /// </summary>
        public ISmartFolder DeletedItemsNode
        {
            get { return _deletedItemsFeedsNode as ISmartFolder; }
        }

        /// <summary>
        /// Gets the TreeNode instance representing/store of
        /// a non persistent search result (temp. result)
        /// </summary>
        public ISmartFolder SearchResultNode
        {
            get { return _searchResultNode; }
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
                using (var g = CreateGraphics())
                {
                    var sz = g.MeasureString("(99999)", FontColorHelper.UnreadFont);
                    var gz = new Size((int) sz.Width, (int) sz.Height);

                    // adjust global sizes
                    if (! treeFeeds.RightImagesSize.Equals(gz))
                        treeFeeds.RightImagesSize = gz;
                }

                treeFeeds.Override.NodeAppearance.ForeColor = FontColorHelper.NormalColor;

                // now iterate and update the single nodes
                if (treeFeeds.Nodes.Count > 0)
                {
                    for (var i = 0; i < _roots.Length; i++)
                    {
                        var startNode = _roots[i];
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

            for (var child = startNode.FirstNode; child != null; child = child.NextNode)
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
            for (var i = 0; i < listFeedItems.Items.Count; i++)
            {
                var lvi = listFeedItems.Items[i];
                // apply leading fonts/colors
                ApplyStyles(lvi);
            }
            listFeedItems.EndUpdate();
        }

        private void ResetListViewOutlookFontAndColor()
        {
            listFeedItemsO.BeginUpdate();

            listFeedItemsO.Font = FontColorHelper.NormalFont;
            var hh = (int) listFeedItemsO.CreateGraphics().MeasureString("W", listFeedItemsO.Font).Height;
            var hh2 = 2 + hh + 3 + hh + 2;
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
            for (var i = 0; i < listFeedItemsO.Nodes.Count; i++)
            {
                listFeedItemsO.Nodes[i].Override.ItemHeight = UltraTreeExtended.DATETIME_GROUP_HEIGHT;
                //NewsItem Nodes
                for (var j = 0; j < listFeedItemsO.Nodes[i].Nodes.Count; j++)
                {
                    //Already done

                    //Comments
                    for (var k = 0; k < listFeedItemsO.Nodes[i].Nodes[j].Nodes.Count; k++)
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
                var cmd = (AppButtonToolCommand) sender;
                var se = (SearchEngine) cmd.Tag;
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
            var phrase = WebSearchText;

            if (phrase.Length > 0)
            {
                if (!SearchComboBox.Items.Contains(phrase))
                    SearchComboBox.Items.Add(phrase);

                if (thisEngine != null)
                {
                    var s = thisEngine.SearchLink;
                    if (!string.IsNullOrEmpty(s))
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
                    var isFirstItem = true;
                    var engineCount = 0;
                    foreach (var engine in owner.SearchEngineHandler.Engines)
                    {
                        if (engine.IsActive)
                        {
                            engineCount++;
                            var s = engine.SearchLink;
                            if (!string.IsNullOrEmpty(s))
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
            var imng = new IdentityNewsServerManager(owner);
            imng.ShowIdentityDialog(this);
        }

        public void CmdOpenConfigNntpServerDialog(ICommand sender)
        {
            var imng = new IdentityNewsServerManager(owner);
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
            set { _currentSelectedFeedsNode = value; }
        }

        public TreeFeedsNodeBase TreeSelectedFeedsNode
        {
            get
            {
                if (treeFeeds.SelectedNodes.Count == 0)
                    return null;
                
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
            get { return listFeedItems.SelectedItems.Count; }
        }

        /// <summary>
        /// Returns the Subscription Tree Image List.
        /// </summary>
        public ImageList TreeImageList
        {
            get { return _treeImages; }
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
            set { _currentNewsItem = value; }
        }

        /// <summary>
        /// Gets the UI Result Dispatcher timer.
        /// </summary>
        internal Timer ResultDispatcher
        {
            get { return _timerDispatchResultsToUI; }
        }

        /// <summary>
        /// Gets a value indicating whether shutdown is in progress.
        /// </summary>
        /// <value><c>true</c> if [shutdown in progress]; otherwise, <c>false</c>.</value>
        internal bool ShutdownInProgress
        {
            get { return _forceShutdown; }
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof (WinGuiMain));
            var _override1 = new Infragistics.Win.UltraWinTree.Override();
            var ultraTreeColumnSet1 = new Infragistics.Win.UltraWinTree.UltraTreeColumnSet();
            var ultraTreeNodeColumn1 = new Infragistics.Win.UltraWinTree.UltraTreeNodeColumn();
            var _override2 = new Infragistics.Win.UltraWinTree.Override();
            var appearance7 = new Infragistics.Win.Appearance();
            var ultraExplorerBarGroup1 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
            var appearance2 = new Infragistics.Win.Appearance();
            var appearance3 = new Infragistics.Win.Appearance();
            var ultraExplorerBarGroup2 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
            var appearance4 = new Infragistics.Win.Appearance();
            var appearance5 = new Infragistics.Win.Appearance();
            var appearance8 = new Infragistics.Win.Appearance();
            this.NavigatorFeedSubscriptions =
                new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
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
            ((System.ComponentModel.ISupportInitialize) (this.treeFeeds)).BeginInit();
            this.NavigatorSearch.SuspendLayout();
            this.panelFeedDetails.SuspendLayout();
            this.panelWebDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.htmlDetail)).BeginInit();
            this.panelFeedItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.listFeedItemsO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarBrowser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarBrowserProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarConnectionState)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarRssParser)).BeginInit();
            this._docContainer.SuspendLayout();
            this._docFeedDetails.SuspendLayout();
            this.panelClientAreaContainer.SuspendLayout();
            this.panelFeedDetailsContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.Navigator)).BeginInit();
            this.Navigator.SuspendLayout();
            this.pNavigatorCollapsed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this._timerTreeNodeExpand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this._timerRefreshFeeds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this._timerRefreshCommentFeeds)).BeginInit();
            this.SuspendLayout();
            // 
            // NavigatorFeedSubscriptions
            // 
            this.NavigatorFeedSubscriptions.Controls.Add(this.treeFeeds);
            resources.ApplyResources(this.NavigatorFeedSubscriptions, "NavigatorFeedSubscriptions");
            this.NavigatorFeedSubscriptions.Name = "NavigatorFeedSubscriptions";
            this.helpProvider1.SetShowHelp(this.NavigatorFeedSubscriptions,
                                           ((bool) (resources.GetObject("NavigatorFeedSubscriptions.ShowHelp"))));
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
            this.helpProvider1.SetShowHelp(this.treeFeeds, ((bool) (resources.GetObject("treeFeeds.ShowHelp"))));
            // 
            // NavigatorSearch
            // 
            this.NavigatorSearch.Controls.Add(this.panelRssSearch);
            resources.ApplyResources(this.NavigatorSearch, "NavigatorSearch");
            this.NavigatorSearch.Name = "NavigatorSearch";
            this.helpProvider1.SetShowHelp(this.NavigatorSearch,
                                           ((bool) (resources.GetObject("NavigatorSearch.ShowHelp"))));
            // 
            // panelRssSearch
            // 
            this.panelRssSearch.BackColor = System.Drawing.SystemColors.InactiveCaption;
            resources.ApplyResources(this.panelRssSearch, "panelRssSearch");
            this.panelRssSearch.Name = "panelRssSearch";
            this.helpProvider1.SetShowHelp(this.panelRssSearch,
                                           ((bool) (resources.GetObject("panelRssSearch.ShowHelp"))));
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
            this.helpProvider1.SetShowHelp(this.panelFeedDetails,
                                           ((bool) (resources.GetObject("panelFeedDetails.ShowHelp"))));
            // 
            // panelWebDetail
            // 
            this.panelWebDetail.Controls.Add(this.htmlDetail);
            resources.ApplyResources(this.panelWebDetail, "panelWebDetail");
            this.panelWebDetail.Name = "panelWebDetail";
            this.helpProvider1.SetShowHelp(this.panelWebDetail,
                                           ((bool) (resources.GetObject("panelWebDetail.ShowHelp"))));
            // 
            // htmlDetail
            // 
            this.htmlDetail.AllowDrop = true;
            resources.ApplyResources(this.htmlDetail, "htmlDetail");
            this.htmlDetail.Name = "htmlDetail";
            this.htmlDetail.OcxState =
                ((System.Windows.Forms.AxHost.State) (resources.GetObject("htmlDetail.OcxState")));
            this.helpProvider1.SetShowHelp(this.htmlDetail, ((bool) (resources.GetObject("htmlDetail.ShowHelp"))));
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
            this.helpProvider1.SetShowHelp(this.detailsPaneSplitter,
                                           ((bool) (resources.GetObject("detailsPaneSplitter.ShowHelp"))));
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
            this.helpProvider1.SetShowHelp(this.panelFeedItems,
                                           ((bool) (resources.GetObject("panelFeedItems.ShowHelp"))));
            // 
            // listFeedItemsO
            // 
            this.listFeedItemsO.ColumnSettings.AllowCellEdit = Infragistics.Win.UltraWinTree.AllowCellEdit.Disabled;
            this.listFeedItemsO.ColumnSettings.AutoFitColumns =
                Infragistics.Win.UltraWinTree.AutoFitColumns.ResizeAllColumns;
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
            this.helpProvider1.SetShowHelp(this.listFeedItemsO,
                                           ((bool) (resources.GetObject("listFeedItemsO.ShowHelp"))));
            // 
            // listFeedItems
            // 
            this.listFeedItems.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listFeedItems.AllowColumnReorder = true;
            this.listFeedItems.Columns.AddRange(new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader[]
                                                    {
                                                        this.colHeadline,
                                                        this.colDate,
                                                        this.colTopic
                                                    });
            resources.ApplyResources(this.listFeedItems, "listFeedItems");
            this.listFeedItems.FullRowSelect = true;
            this.listFeedItems.HideSelection = false;
            this.listFeedItems.Name = "listFeedItems";
            this.listFeedItems.NoThreadChildsPlaceHolder = null;
            this.helpProvider1.SetShowHelp(this.listFeedItems, ((bool) (resources.GetObject("listFeedItems.ShowHelp"))));
            this.listFeedItems.UseCompatibleStateImageBehavior = false;
            this.listFeedItems.View = System.Windows.Forms.View.Details;
            this.listFeedItems.ListLayoutModified +=this.OnFeedListLayoutModified;
            this.listFeedItems.ItemActivate += new System.EventHandler(this.OnFeedListItemActivate);
            this.listFeedItems.ExpandThread +=this.OnFeedListExpandThread;
            this.listFeedItems.ListLayoutChanged +=this.OnFeedListLayoutChanged;
            this.listFeedItems.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnFeedListMouseDown);
            this.listFeedItems.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnFeedListItemDrag);
            this.listFeedItems.AfterExpandThread +=this.OnFeedListAfterExpandThread;
            this.listFeedItems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFeedListItemKeyUp);
            // 
            // colHeadline
            // 
            this.colHeadline.ColumnValueType = typeof (string);
            this.colHeadline.Key = "Title";
            resources.ApplyResources(this.colHeadline, "colHeadline");
            // 
            // colDate
            // 
            this.colDate.ColumnValueType = typeof (System.DateTime);
            this.colDate.Key = "Date";
            resources.ApplyResources(this.colDate, "colDate");
            // 
            // colTopic
            // 
            this.colTopic.ColumnValueType = typeof (string);
            this.colTopic.Key = "Subject";
            resources.ApplyResources(this.colTopic, "colTopic");
            // 
            // _status
            // 
            resources.ApplyResources(this._status, "_status");
            this._status.Name = "_status";
            this._status.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[]
                                             {
                                                 this.statusBarBrowser,
                                                 this.statusBarBrowserProgress,
                                                 this.statusBarConnectionState,
                                                 this.statusBarRssParser
                                             });
            this.helpProvider1.SetShowHelp(this._status, ((bool) (resources.GetObject("_status.ShowHelp"))));
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
            this.helpProvider1.SetShowHelp(this.progressBrowser,
                                           ((bool) (resources.GetObject("progressBrowser.ShowHelp"))));
            // 
            // rightSandDock
            // 
            resources.ApplyResources(this.rightSandDock, "rightSandDock");
            this.rightSandDock.Guid = new System.Guid("c6e4c477-596c-4e8c-9d35-840718d4c40d");
            this.rightSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
            this.rightSandDock.Manager = this.sandDockManager;
            this.rightSandDock.Name = "rightSandDock";
            this.helpProvider1.SetShowHelp(this.rightSandDock, ((bool) (resources.GetObject("rightSandDock.ShowHelp"))));
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
            this.helpProvider1.SetShowHelp(this.bottomSandDock,
                                           ((bool) (resources.GetObject("bottomSandDock.ShowHelp"))));
            // 
            // topSandDock
            // 
            resources.ApplyResources(this.topSandDock, "topSandDock");
            this.topSandDock.Guid = new System.Guid("e1c62abd-0e7a-4bb6-aded-a74f27027165");
            this.topSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
            this.topSandDock.Manager = this.sandDockManager;
            this.topSandDock.Name = "topSandDock";
            this.helpProvider1.SetShowHelp(this.topSandDock, ((bool) (resources.GetObject("topSandDock.ShowHelp"))));
            // 
            // _docContainer
            // 
            this._docContainer.Controls.Add(this._docFeedDetails);
            this._docContainer.Cursor = System.Windows.Forms.Cursors.Default;
            this._docContainer.DockingManager = TD.SandDock.DockingManager.Whidbey;
            this._docContainer.Guid = new System.Guid("f032a648-4262-4312-ab2b-abe5094272bd");
            this._docContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400,
                                                                                System.Windows.Forms.Orientation.
                                                                                    Horizontal,
                                                                                new TD.SandDock.LayoutSystemBase[]
                                                                                    {
                                                                                        ((TD.SandDock.LayoutSystemBase)
                                                                                         (new TD.SandDock.
                                                                                             DocumentLayoutSystem(392,
                                                                                                                  414,
                                                                                                                  new TD
                                                                                                                      .
                                                                                                                      SandDock
                                                                                                                      .
                                                                                                                      DockControl
                                                                                                                      []
                                                                                                                      {
                                                                                                                          this
                                                                                                                              .
                                                                                                                              _docFeedDetails
                                                                                                                      },
                                                                                                                  this.
                                                                                                                      _docFeedDetails)))
                                                                                    });
            resources.ApplyResources(this._docContainer, "_docContainer");
            this._docContainer.Manager = null;
            this._docContainer.Name = "_docContainer";
            this._docContainer.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
            this.helpProvider1.SetShowHelp(this._docContainer, ((bool) (resources.GetObject("_docContainer.ShowHelp"))));
            // 
            // _docFeedDetails
            // 
            this._docFeedDetails.Closable = false;
            this._docFeedDetails.Controls.Add(this.panelFeedDetails);
            this._docFeedDetails.Guid = new System.Guid("9c7b7643-2ed3-402c-9e86-3c958341c81f");
            resources.ApplyResources(this._docFeedDetails, "_docFeedDetails");
            this._docFeedDetails.Name = "_docFeedDetails";
            this.helpProvider1.SetShowHelp(this._docFeedDetails,
                                           ((bool) (resources.GetObject("_docFeedDetails.ShowHelp"))));
            // 
            // panelClientAreaContainer
            // 
            this.panelClientAreaContainer.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (243)))),
                                                                                    ((int) (((byte) (243)))),
                                                                                    ((int) (((byte) (247)))));
            this.panelClientAreaContainer.Controls.Add(this.panelFeedDetailsContainer);
            this.panelClientAreaContainer.Controls.Add(this.splitterNavigator);
            this.panelClientAreaContainer.Controls.Add(this.Navigator);
            this.panelClientAreaContainer.Controls.Add(this.pNavigatorCollapsed);
            resources.ApplyResources(this.panelClientAreaContainer, "panelClientAreaContainer");
            this.panelClientAreaContainer.Name = "panelClientAreaContainer";
            this.helpProvider1.SetShowHelp(this.panelClientAreaContainer,
                                           ((bool) (resources.GetObject("panelClientAreaContainer.ShowHelp"))));
            // 
            // panelFeedDetailsContainer
            // 
            this.panelFeedDetailsContainer.Controls.Add(this._docContainer);
            this.panelFeedDetailsContainer.Controls.Add(this.detailHeaderCaption);
            resources.ApplyResources(this.panelFeedDetailsContainer, "panelFeedDetailsContainer");
            this.panelFeedDetailsContainer.Name = "panelFeedDetailsContainer";
            this.helpProvider1.SetShowHelp(this.panelFeedDetailsContainer,
                                           ((bool) (resources.GetObject("panelFeedDetailsContainer.ShowHelp"))));
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
            this.helpProvider1.SetShowHelp(this.detailHeaderCaption,
                                           ((bool) (resources.GetObject("detailHeaderCaption.ShowHelp"))));
            this.detailHeaderCaption.WrapText = false;
            // 
            // splitterNavigator
            // 
            resources.ApplyResources(this.splitterNavigator, "splitterNavigator");
            this.splitterNavigator.Name = "splitterNavigator";
            this.helpProvider1.SetShowHelp(this.splitterNavigator,
                                           ((bool) (resources.GetObject("splitterNavigator.ShowHelp"))));
            this.splitterNavigator.TabStop = false;
            // 
            // Navigator
            // 
            this.Navigator.Controls.Add(this.NavigatorFeedSubscriptions);
            this.Navigator.Controls.Add(this.NavigatorSearch);
            resources.ApplyResources(this.Navigator, "Navigator");
            ultraExplorerBarGroup1.Container = this.NavigatorFeedSubscriptions;
            ultraExplorerBarGroup1.Key = "groupFeedsTree";
            appearance2.Image = ((object) (resources.GetObject("appearance2.Image")));
            ultraExplorerBarGroup1.Settings.AppearancesLarge.HeaderAppearance = appearance2;
            appearance3.Image = ((object) (resources.GetObject("appearance3.Image")));
            ultraExplorerBarGroup1.Settings.AppearancesSmall.HeaderAppearance = appearance3;
            resources.ApplyResources(ultraExplorerBarGroup1, "ultraExplorerBarGroup1");
            ultraExplorerBarGroup2.Container = this.NavigatorSearch;
            ultraExplorerBarGroup2.Key = "groupFeedsSearch";
            appearance4.Image = ((object) (resources.GetObject("appearance4.Image")));
            ultraExplorerBarGroup2.Settings.AppearancesLarge.HeaderAppearance = appearance4;
            appearance5.Image = ((object) (resources.GetObject("appearance5.Image")));
            ultraExplorerBarGroup2.Settings.AppearancesSmall.HeaderAppearance = appearance5;
            resources.ApplyResources(ultraExplorerBarGroup2, "ultraExplorerBarGroup2");
            this.Navigator.Groups.AddRange(new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup[]
                                               {
                                                   ultraExplorerBarGroup1,
                                                   ultraExplorerBarGroup2
                                               });
            this.Navigator.GroupSettings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;
            this.Navigator.Name = "Navigator";
            this.Navigator.NavigationMaxGroupHeaders = 0;
            this.helpProvider1.SetShowHelp(this.Navigator, ((bool) (resources.GetObject("Navigator.ShowHelp"))));
            this.Navigator.Style = Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarStyle.OutlookNavigationPane;
            // 
            // pNavigatorCollapsed
            // 
            this.pNavigatorCollapsed.BackColor = System.Drawing.Color.Transparent;
            this.pNavigatorCollapsed.Controls.Add(this.navigatorHiddenCaption);
            resources.ApplyResources(this.pNavigatorCollapsed, "pNavigatorCollapsed");
            this.pNavigatorCollapsed.Name = "pNavigatorCollapsed";
            this.helpProvider1.SetShowHelp(this.pNavigatorCollapsed,
                                           ((bool) (resources.GetObject("pNavigatorCollapsed.ShowHelp"))));
            // 
            // navigatorHiddenCaption
            // 
            appearance8.BackColor = System.Drawing.Color.CornflowerBlue;
            appearance8.BackColor2 = System.Drawing.Color.MidnightBlue;
            appearance8.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
            appearance8.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            appearance8.Image = ((object) (resources.GetObject("appearance8.Image")));
            appearance8.ImageHAlign = Infragistics.Win.HAlign.Center;
            appearance8.ImageVAlign = Infragistics.Win.VAlign.Top;
            resources.ApplyResources(appearance8, "appearance8");
            appearance8.TextTrimming = Infragistics.Win.TextTrimming.EllipsisWord;
            this.navigatorHiddenCaption.Appearance = appearance8;
            resources.ApplyResources(this.navigatorHiddenCaption, "navigatorHiddenCaption");
            this.navigatorHiddenCaption.Name = "navigatorHiddenCaption";
            this.navigatorHiddenCaption.Padding = new System.Drawing.Size(0, 5);
            this.helpProvider1.SetShowHelp(this.navigatorHiddenCaption,
                                           ((bool) (resources.GetObject("navigatorHiddenCaption.ShowHelp"))));
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
            this._timerRefreshCommentFeeds.Elapsed +=
                new System.Timers.ElapsedEventHandler(this.OnTimerCommentFeedsRefreshElapsed);
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
            this.helpProvider1.SetHelpNavigator(this,
                                                ((System.Windows.Forms.HelpNavigator)
                                                 (resources.GetObject("$this.HelpNavigator"))));
            this.KeyPreview = true;
            this.Name = "WinGuiMain";
            this.helpProvider1.SetShowHelp(this, ((bool) (resources.GetObject("$this.ShowHelp"))));
            this.NavigatorFeedSubscriptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.treeFeeds)).EndInit();
            this.NavigatorSearch.ResumeLayout(false);
            this.panelFeedDetails.ResumeLayout(false);
            this.panelWebDetail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.htmlDetail)).EndInit();
            this.panelFeedItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.listFeedItemsO)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarBrowser)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarBrowserProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarConnectionState)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarRssParser)).EndInit();
            this._docContainer.ResumeLayout(false);
            this._docFeedDetails.ResumeLayout(false);
            this.panelClientAreaContainer.ResumeLayout(false);
            this.panelFeedDetailsContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.Navigator)).EndInit();
            this.Navigator.ResumeLayout(false);
            this.pNavigatorCollapsed.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this._timerTreeNodeExpand)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this._timerRefreshFeeds)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this._timerRefreshCommentFeeds)).EndInit();
            this.ResumeLayout(false);
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

        #region Implementation of ITabState

        public bool CanClose
        {
            get { return false; }
            set { }
        }

        public bool CanGoBack
        {
            get
            {
                return _feedItemImpressionHistory.CanGetPrevious &&
                       _feedItemImpressionHistory.Count > 1;
            }
            set { }
        }

        public bool CanGoForward
        {
            get { return _feedItemImpressionHistory.CanGetNext; }
            set { }
        }

        public string Title
        {
            get
            {
                if (CurrentSelectedFeedsNode != null)
                    return CurrentSelectedFeedsNode.Text;
                
                return String.Empty;
            }
            set
            {
                // nothing to implement here
            }
        }

        public string Url { get; set; }

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
            InvokeOnGui(
                () => SearchFinishedAction(e.Tag, e.MatchingFeeds, e.MatchingItems, e.MatchingFeedsCount,
                                           e.MatchingItemsCount));
        }

        private void OnSearchPanelStartNewsItemSearch(object sender, NewsItemSearchEventArgs e)
        {
            AsyncStartNewsSearch(e.FinderNode);
        }

        private void AsyncStartNewsSearch(FinderNode node)
        {
            Action<FinderNode> start = StartNewsSearch;
            start.BeginInvoke(node, cb =>
                                        {
                                            if (start != null)
                                            {
                                                try
                                                {
                                                    start.EndInvoke(cb);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _log.Error("AsyncCall 'StartNewsSearchDelegate' caused this exception", ex);
                                                }
                                            }
                                        }, null);
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

            var newName = e.ResultContainerName;
            if (!string.IsNullOrEmpty(newName))
            {
                var parent = GetRoot(RootFolderType.Finder);

                if (newName.IndexOf(FeedSource.CategorySeparator) > 0)
                {
                    var a = newName.Split(FeedSource.CategorySeparator.ToCharArray());
                    parent = TreeHelper.CreateCategoryHive(parent,
                                                           String.Join(FeedSource.CategorySeparator, a, 0, a.Length - 1),
                                                           _treeSearchFolderContextMenu,
                                                           FeedNodeType.FinderCategory);

                    newName = a[a.Length - 1].Trim();
                }

                var feedsNode = TreeHelper.FindChildNode(parent, newName, FeedNodeType.Finder);
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
                searchPanel.SetControlStateTo(ItemSearchState.Finished, String.Format(SR.RssSearchSuccessResultMessage,newsItemsCount));
            }

            var finder = tag as RssFinder;
            var tn = (finder != null ? finder.Container : null);

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

            var result = new ArrayList();

            if (categories != null)
            {
                var sep = FeedSource.CategorySeparator;
                foreach (var f in owner.FeedHandler.GetFeeds().Values)
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
                var fa = new INewsFeed[result.Count];
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

            var scc = new SearchCriteriaCollection();
            if (mergeWithLocalResults)
            {
                // merge with local search result
                scc.Add(new SearchCriteriaString(searchPhrase, SearchStringElement.All, StringExpressionKind.Text));
            }
            var finder = new RssFinder(_searchResultNode, scc,
                                       null, null, ScopeResolve, true)
                             {
                                 ExternalResultMerged = mergeWithLocalResults,
                                 ExternalSearchPhrase = searchPhrase,
                                 ExternalSearchUrl = searchUrl
                             };

            _searchResultNode.Finder = finder;
            Action<string, FinderNode> start = StartRssRemoteSearch;
            start.BeginInvoke(searchUrl, _searchResultNode, cb =>
                                                                {
                                                                    try
                                                                    {
                                                                        start.EndInvoke(cb);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        _log.Error("AsyncCall 'StartRssRemoteSearch' caused this exception", ex);
                                                                    }
                                                                }, null);

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
				_log.Error(String.Format("RSS Remote Search '{0}' caused exception", searchUrl), ex);
				InvokeOnGuiSync(() =>
                SetSearchStatusText("Search '" + StringHelper.ShortenByEllipsis(searchUrl, 30) + "' caused a problem: " +
                                    ex.Message));
            }
        }

        #endregion

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
            var tn = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), f);
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

                    for (var i = 0; i < listFeedItems.Items.Count; i++)
                    {
                        var lvi = listFeedItems.Items[i];
                        var ti = (INewsItem) lvi.Key;
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
            var category = parent.CategoryStoreName;

            var urls = feedUrl.Split(Environment.NewLine.ToCharArray());
            var multipleSubscriptions = (urls.Length > 1);

            for (var i = 0; i < urls.Length; i++)
            {
                feedUrl = owner.HandleUrlFeedProtocol(urls[i]);

                try
                {
                    try
                    {
                        var reqUri = new Uri(feedUrl);
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
                        var f2 = owner.FeedHandler.GetFeeds()[feedUrl];
                        owner.MessageInfo(String.Format(SR.GUIFieldLinkRedundantInfo,
                                              (f2.category == null
                                                   ? String.Empty
                                                   : f2.category + FeedSource.CategorySeparator) + f2.title, f2.link));
                        return;
                    }

                    if (owner.InternetAccessAllowed)
                    {
                        var fetchHandler = new PrefetchFeedThreadHandler(feedUrl, owner.Proxy);

                        var result = fetchHandler.Start(this, String.Format(SR.GUIStatusWaitMessagePrefetchFeed,feedUrl));

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
            _startupTimer.Interval = 1000*
                                     RssBanditApplication.ReadAppSettingsEntry(
                                         "ForcedRefreshOfFeedsAtStartupDelay.Seconds", 30);
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
            var tn = CurrentSelectedFeedsNode;

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
                var param = (object[]) _uiTasksTimer.GetData(DelayedTasks.RefreshTreeUnreadStatus, true);
                var tn = (TreeFeedsNodeBase) param[0];
                var counter = (int) param[1];
                if (tn != null)
                    UpdateTreeNodeUnreadStatus(tn, counter);
            }

            if (_uiTasksTimer[DelayedTasks.RefreshTreeCommentStatus])
            {
                _uiTasksTimer.StopTask(DelayedTasks.RefreshTreeCommentStatus);
                var param = (object[]) _uiTasksTimer.GetData(DelayedTasks.RefreshTreeCommentStatus, true);
                var tn = (TreeFeedsNodeBase) param[0];
                var items = (IList<INewsItem>) param[1];
                var commentsRead = (bool) param[2];
                if (tn != null)
                    UpdateCommentStatus(tn, items, commentsRead);
            }

            if (_uiTasksTimer[DelayedTasks.NavigateToWebUrl])
            {
                _uiTasksTimer.StopTask(DelayedTasks.NavigateToWebUrl);
                var param = (object[]) _uiTasksTimer.GetData(DelayedTasks.NavigateToWebUrl, true);
                DetailTabNavigateToUrl((string) param[0], (string) param[1], (bool) param[2], (bool) param[3]);
            }

            if (_uiTasksTimer[DelayedTasks.StartRefreshOneFeed])
            {
                _uiTasksTimer.StopTask(DelayedTasks.StartRefreshOneFeed);
                var feedUrl = (string) _uiTasksTimer.GetData(DelayedTasks.StartRefreshOneFeed, true);
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
                var f = (INewsFeed) _uiTasksTimer.GetData(DelayedTasks.ShowFeedPropertiesDialog, true);
                DisplayFeedProperties(f);
            }

            if (_uiTasksTimer[DelayedTasks.NavigateToFeedNewsItem])
            {
                _uiTasksTimer.StopTask(DelayedTasks.NavigateToFeedNewsItem);
                var item = (INewsItem) _uiTasksTimer.GetData(DelayedTasks.NavigateToFeedNewsItem, true);
                NavigateToFeedNewsItem(item);
            }

            if (_uiTasksTimer[DelayedTasks.NavigateToFeed])
            {
                _uiTasksTimer.StopTask(DelayedTasks.NavigateToFeed);
                var f = (INewsFeed) _uiTasksTimer.GetData(DelayedTasks.NavigateToFeed, true);
                NavigateToFeed(f);
            }

            if (_uiTasksTimer[DelayedTasks.AutoSubscribeFeedUrl])
            {
                _uiTasksTimer.StopTask(DelayedTasks.AutoSubscribeFeedUrl);
                var parameter = (object[]) _uiTasksTimer.GetData(DelayedTasks.AutoSubscribeFeedUrl, true);
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
            for (var i = 0; i < listFeedItemsO.Nodes.Count; i++)
            {
                if (listFeedItemsO.Nodes[i].Selected)
                {
                    if (groupSelected == null)
                        groupSelected = new ArrayList();
                    //Select all child nodes
                    for (var j = 0; j < listFeedItemsO.Nodes[i].Nodes.Count; j++)
                    {
                        var node = (UltraTreeNodeExtended) listFeedItemsO.Nodes[i].Nodes[j];
                        //node.NodeOwner.Selected = true;
                        if (node.NewsItem != null && !node.NewsItem.BeenRead)
                        {
                            groupSelected.Add(node.NewsItem);
                        }
                    }
                }
                for (var j = 0; j < listFeedItemsO.Nodes[i].Nodes.Count; j++)
                {
                    if (listFeedItemsO.Nodes[i].Nodes[j].Selected)
                    {
                        var node = (UltraTreeNodeExtended) listFeedItemsO.Nodes[i].Nodes[j];
                        if (node.NodeOwner != null)
                            node.NodeOwner.Selected = true;
                    }
                    //Comments
                    for (var k = 0; k < listFeedItemsO.Nodes[i].Nodes[j].Nodes.Count; k++)
                    {
                        if (listFeedItemsO.Nodes[i].Nodes[j].Nodes[k].Selected)
                        {
                            var node =
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
                var tn = TreeSelectedFeedsNode;
                if (tn != null)
                {
                    if (tn.Type == FeedNodeType.Category)
                    {
                        var category = tn.CategoryStoreName;
                        var temp = new Hashtable();

                        foreach (INewsItem item in groupSelected)
                        {
                            IFeedDetails fi;
                            if (temp.ContainsKey(item.Feed.link))
                            {
                                fi = (IFeedDetails) temp[item.Feed.link];
                            }
                            else
                            {
                                fi = (IFeedDetails) item.FeedDetails.Clone();
                                fi.ItemsList.Clear();
                                temp.Add(item.Feed.link, fi);
                            }
                            fi.ItemsList.Add(item);
                        }

                        var redispItems = new FeedInfoList(category);

                        foreach (IFeedDetails fi in temp.Values)
                        {
                            if (fi.ItemsList.Count > 0)
                                redispItems.Add(fi);
                        }

                        BeginTransformFeedList(redispItems, tn, owner.FeedHandler.GetCategoryStyleSheet(category));
                    }
                    else
                    {
                        var feedUrl = tn.DataKey;
                        var fi = owner.FeedHandler.GetFeedDetails(feedUrl);

                        if (fi != null)
                        {
                        	IFeedDetails fi2 = (IFeedDetails) fi.Clone();
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
                for (var i = 0; i < listFeedItems.Items.Count; i++)
                {
                    if (listFeedItems.Items[i].Selected)
                    {
                        var n = listFeedItemsO.GetFromLVI(listFeedItems.Items[i]);
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

                var n = (UltraTreeNodeExtended) listFeedItemsO.SelectedNodes[0];
                if (n.Level == 1 && n.NewsItem != null)
                {
                    DetailTabNavigateToUrl(n.NewsItem.Link, null, true, true);
                }
            }
        }

        private void listFeedItemsO_BeforeExpand(object sender, CancelableNodeEventArgs e)
        {
            var n = (UltraTreeNodeExtended) e.TreeNode;
            if (n.Level == 1)
            {
                if (n.NewsItem != null && n.NewsItem.CommentCount > 0)
                {
                    //Expand Comments Nodes
                    var lvi = n.NodeOwner;
                    lvi.Expanded = true;
                    //
                    listFeedItemsO.AddCommentUpdating(lvi);
                    //listFeedItemsO.AddRangeComments(lvi, items);
                }
            }
        }


        private void listFeedItemsO_MouseDown(object sender, MouseEventArgs e)
        {
            var n = (UltraTreeNodeExtended) listFeedItemsO.GetNodeFromPoint(e.X, e.Y);

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
                    var item = CurrentSelectedFeedItem = n.NewsItem;
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
            var n = listFeedItemsO.ActiveNode ?? listFeedItemsO.TopNode;
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