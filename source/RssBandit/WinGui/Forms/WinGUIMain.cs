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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using IEControl;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinToolTip;
using Infragistics.Win.UltraWinTree;
using log4net;
using NewsComponents;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.Search;
using NewsComponents.Utils;
using RssBandit.Common.Logging;
using RssBandit.Filter;
using RssBandit.Resources;
using RssBandit.SpecialFeeds;
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
using RssBandit.WinGui.Controls.ThListView;
using ToolTip=System.Windows.Forms.ToolTip;
using Microsoft.WindowsAPICodePack.Taskbar;

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

        private static int BrowserProgressBarWidth = 120;
        private static int MaxHeadlineWidth = 133;

        /// <summary>
        /// To be raised by one on every Toolbars modification like new tools or menus!
        /// </summary>
        /// <remarks>
        /// If you forget this, you will always get your old toolbars layout
        /// restored from the users local machine.
        /// </remarks>
        private static readonly int _currentToolbarsVersion = ToolbarHelper.CurrentToolbarsVersion;

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

        private bool _browserTabsRestored;

        private ToastNotifier toastNotifier;
        private WheelSupport wheelSupport;
        internal UrlCompletionExtender urlExtender;
        private NavigatorHeaderHelper navigatorHeaderHelper;
        private ToolbarHelper toolbarHelper;
        internal HistoryMenuManager historyMenuManager;
		private UltraToolTipContextHelperForTreeNodes treeNodesTooltipHelper;

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

        private NewsItemFilterManager _filterManager;

        private SearchPanel searchPanel;

        private IList<IBlogExtension> blogExtensions;

        // store the HashCodes of temp. NewsItems, that have bean read.
        // Used for commentRss implementation
        private readonly Dictionary<string, object> tempFeedItemsRead = new Dictionary<string, object>();

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

    	private string _lastVisualFeedSource;

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

        private ToolStripMenuItem _feedInfoContextMenu;
        private ContextMenuStrip _subscriptionTreeRootContextMenu;
        private ContextMenuStrip _treeCategoryContextMenu;
        private ContextMenuStrip _treeFeedContextMenu;
        private ContextMenuStrip _notifyContextMenu;
        private ContextMenuStrip _listContextMenu;
        private ContextMenuStrip _treeLocalFeedContextMenu;
        private ContextMenuStrip _treeSearchFolderRootContextMenu;
        private ContextMenuStrip _treeSearchFolderContextMenu;
        private ContextMenuStrip _treeTempSearchFolderContextMenu;
        private ContextMenuStrip _docTabContextMenu;

        // Used to temp. store the context menu position. Processed later within
        // ICommand event receiver (in Screen-Coordinates).
        private Point _contextMenuCalledAt = Point.Empty;

        private AppContextMenuCommand _listContextMenuDownloadAttachment;
        private ToolStripItem _listContextMenuDeleteItemsSeparator;
        private ToolStripItem _listContextMenuDownloadAttachmentsSeparator;

        private TrayStateManager _trayManager;

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
        private StatusStrip _status;
        private ToolStripStatusLabel statusBarBrowser;
        private ToolStripStatusLabel statusBarBrowserProgress;
        private ToolStripStatusLabel statusBarConnectionState;
        private ToolStripStatusLabel statusBarRssParser;
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
        private UltraToolTipManager ultraToolTipManager;
        private UltraTreeExtended listFeedItemsO;
		private UltraExplorerBar Navigator;
        private UltraExplorerBarContainerControl NavigatorSearch;
        private Panel panelFeedItems;
        private Splitter splitterNavigator;
        private Panel pNavigatorCollapsed;
        private Panel panelClientAreaContainer;
        private Panel panelFeedDetailsContainer;
        private UltraLabel detailHeaderCaption;
		private VerticalHeaderLabel navigatorHiddenCaption;
		private UltraTree treeFeeds;
        private UltraDesktopAlert ultraDesktopAlert;
        private PictureBox pictureBox;
        private ThumbnailToolBarButton buttonAdd;
        private ThumbnailToolBarButton buttonRefresh;
        private JumpList jumpList;
        private JumpListCustomCategory jlcRecent;
		private KeyItemCollection<string, string> jlcRecentContents = new KeyItemCollection<string, string>(); 

        private IContainer components;

        #endregion

        #region Class initialize

        public WinGuiMain(RssBanditApplication theGuiOwner, FormWindowState initialFormState)
        {
            InvokeOnGuiSync = a => GuiInvoker.Invoke(this, a);
			InvokeOnGui = a => GuiInvoker.InvokeAsync(this, a);

            GuiOwner = theGuiOwner;
            initialStartupState = initialFormState;

            // set IG scale factor
            typeof(DrawUtility).GetField("scalingFactor", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, ScaleFactor);
            var sf = new SizeF(DeviceDpi, DeviceDpi);
            typeof(DrawUtility).GetField("screenDpi", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, sf);

            UltraTreeExtended.COMMENT_HEIGHT = (int)(UltraTreeExtended.COMMENT_HEIGHT * ScaleFactor);
            UltraTreeExtended.DATETIME_GROUP_HEIGHT = (int)(UltraTreeExtended.DATETIME_GROUP_HEIGHT * ScaleFactor);

            urlExtender = new UrlCompletionExtender(this);
            _feedItemImpressionHistory = new History( /* TODO: get maxEntries from .config */);
            _feedItemImpressionHistory.StateChanged += OnFeedItemImpressionHistoryStateChanged;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
	        
			this.Icon = Properties.Resources.App;
            
			Init();
            ApplyComponentTranslation();
        }


        private void ApplyComponentTranslation()
        {
            Text = RssBanditApplication.CaptionOnly; // dynamically changed!
            detailHeaderCaption.Text = SR.MainForm_DetailHeaderCaption_AtStartup; // dynamically changed!
            navigatorHiddenCaption.Text = SR.MainForm_SubscriptionNavigatorCaption;
            //Navigator.Groups[Resource.NavigatorGroup.Subscriptions].Text = SR.MainForm_SubscriptionNavigatorCaption;
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
			[DebuggerStepThrough]
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
        	ResetUltraTooltipDisplay();
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
                    var gz = new Size((int) sz.Width, (int) sz.Height);

                    // adjust global sizes
                    if (! treeFeeds.RightImagesSize.Equals(gz))
                        treeFeeds.RightImagesSize = gz;
                }

                treeFeeds.Override.NodeAppearance.ForeColor = FontColorHelper.NormalColor;

                // now iterate and update the single nodes
                if (treeFeeds.Nodes.Count > 0)
                {
					foreach (TreeFeedsNodeBase startNode in treeFeeds.Nodes)
						WalkdownThenRefreshFontColor(startNode);
					//for (int i = 0; i < _roots.Count; i++)
					//{
					//    TreeFeedsNodeBase startNode = _roots[i];
					//    if (null != startNode)
					//        WalkdownThenRefreshFontColor(startNode);
					//}
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

            var scale = (float)DeviceDpi / 96;
            var sz1 = (int)(1 * scale);
            var sz2 = (int)(2 * scale);
            var sz3 = (int)(3 * scale);
            var sz4 = (int)(4 * scale);
            var sz5 = (int)(5 * scale);
            var sz6 = (int)(6 * scale);
            var sz7 = (int)(7 * scale);
            var sz8 = (int)(8 * scale);
            var sz9 = (int)(9 * scale);

            listFeedItemsO.BeginUpdate();

            listFeedItemsO.Font = FontColorHelper.NormalFont;
            var hh = (int) listFeedItemsO.CreateGraphics().MeasureString("W", listFeedItemsO.Font).Height;
            int hh2 = sz2 + hh + sz3 + hh + sz2;
            listFeedItemsO.Override.ItemHeight = hh2%2 == 0 ? hh2 + sz1 : hh2;
            //BUGBUG: if the height is an even number, it uses a bad rectangle
            listFeedItemsO.ColumnSettings.ColumnSets[0].Columns[0].LayoutInfo.PreferredCellSize =
                new Size(listFeedItemsO.Width, listFeedItemsO.Override.ItemHeight);
            hh2 = sz5 + hh;
            UltraTreeExtended.COMMENT_HEIGHT = hh2%2 == 0 ? hh2 + sz1 : hh2;
            hh2 = hh + sz9;
            UltraTreeExtended.DATETIME_GROUP_HEIGHT = hh2%2 == 0 ? hh2 + sz1 : hh2;

            // now iterate and update the single items
            //Root DateTime Nodes
            for (int i = 0; i < listFeedItemsO.Nodes.Count; i++)
            {
                listFeedItemsO.Nodes[i].Override.ItemHeight = UltraTreeExtended.DATETIME_GROUP_HEIGHT;
                //INewsItem Nodes
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

		void ResetUltraTooltipDisplay()
		{
			FontColorHelper.CopyFromFont(
				ultraToolTipManager.ToolTipTitleAppearance.FontData, 
				FontColorHelper.NormalFont);
			ultraToolTipManager.ToolTipTitleAppearance.FontData.Bold = DefaultableBoolean.True;
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
            string phrase = WebSearchText;

            if (phrase.Length > 0)
            {
                if (!SearchComboBox.Items.Contains(phrase))
                    SearchComboBox.Items.Add(phrase);

                if (thisEngine != null)
                {
                    string s = thisEngine.SearchLink;
                    if (!string.IsNullOrEmpty(s))
                    {
                        try
                        {
                            //s = String.Format(new UrlFormatter(), s, phrase);
                            s = string.Format(s, Uri.EscapeUriString(phrase));
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
                            DetailTabNavigateToUrl(s
								, thisEngine.Title
								, RssBanditApplication.PersistedSettings.GetProperty(Ps.WebSearchEnginesRequiresNewTab, true)
								, true);
                        }
                    }
                }
                else
                {
                    // all
                    bool isFirstItem = true;
                    int engineCount = 0;
                    foreach (var engine in owner.SearchEngineHandler.Engines)
                    {
                        if (engine.IsActive)
                        {
                            engineCount++;
                            string s = engine.SearchLink;
                            if (!string.IsNullOrEmpty(s))
                            {
                                try
                                {
                                    //s = String.Format(new UrlFormatter(), s, phrase);
                                    s = string.Format(s, Uri.EscapeUriString(phrase));
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


        public FeedSourceEntry CurrentSelectedFeedSource
        {
            get
            {
                return owner.FeedSources.Sources.FirstOrDefault(entry => entry.ID.ToString() == Navigator.SelectedGroup.Key);               
            }
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
            	FeedSourceEntry currentSource = FeedSourceEntryOf(GetRoot(RootFolderType.MyFeeds));
                foreach (TreeFeedsNodeBase selected in treeFeeds.SelectedNodes)
					if (selected.Visible && FeedSourceEntryOf(selected) == currentSource) 
						return selected;

				return treeFeeds.ActiveNode as TreeFeedsNodeBase;
            }
            set
            {
                if (value != null && value.Control != null)
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
        /// Gets or sets the current internal selected feed story item (INewsItem)
        /// </summary>
        /// <value>an INewsItem instance</value>
        /// <remarks>If the internal current INewsItem is null, it
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
		            
					if (_trayManager != null)
						_trayManager.Dispose();
		            _trayManager = null;

		            if (buttonAdd != null)
						buttonAdd.Dispose();
		            buttonAdd = null;

					if (buttonRefresh != null)
						buttonRefresh.Dispose();
		            buttonRefresh = null;

					if (treeNodesTooltipHelper != null)
						treeNodesTooltipHelper.Dispose();
		            treeNodesTooltipHelper = null;

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
			Infragistics.Win.UltraWinTree.UltraTreeColumnSet ultraTreeColumnSet1 = new Infragistics.Win.UltraWinTree.UltraTreeColumnSet();
			Infragistics.Win.UltraWinTree.UltraTreeNodeColumn ultraTreeNodeColumn1 = new Infragistics.Win.UltraWinTree.UltraTreeNodeColumn();
			Infragistics.Win.UltraWinTree.Override _override1 = new Infragistics.Win.UltraWinTree.Override();
			Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
			Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup ultraExplorerBarGroup1 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
			Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
			this.NavigatorSearch = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
			this.panelRssSearch = new System.Windows.Forms.Panel();
			this.treeFeeds = new Infragistics.Win.UltraWinTree.UltraTree();
			this.ultraToolTipManager = new Infragistics.Win.UltraWinToolTip.UltraToolTipManager(this.components);
			this.panelFeedDetails = new System.Windows.Forms.Panel();
			this.panelWebDetail = new System.Windows.Forms.Panel();
			this.htmlDetail = new IEControl.HtmlControl();
			this.detailsPaneSplitter = new RssBandit.WinGui.Controls.CollapsibleSplitter();
			this.panelFeedItems = new System.Windows.Forms.Panel();
			this.listFeedItemsO = new RssBandit.WinGui.Controls.UltraTreeExtended();
			this.listFeedItems = new RssBandit.WinGui.Controls.ThListView.ThreadedListView();
			this.colHeadline = ((RssBandit.WinGui.Controls.ThListView.ThreadedListViewColumnHeader)(new RssBandit.WinGui.Controls.ThListView.ThreadedListViewColumnHeader()));
			this.colDate = ((RssBandit.WinGui.Controls.ThListView.ThreadedListViewColumnHeader)(new RssBandit.WinGui.Controls.ThListView.ThreadedListViewColumnHeader()));
			this.colTopic = ((RssBandit.WinGui.Controls.ThListView.ThreadedListViewColumnHeader)(new RssBandit.WinGui.Controls.ThListView.ThreadedListViewColumnHeader()));
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._status = new System.Windows.Forms.StatusStrip();
			this.statusBarBrowser = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusBarBrowserProgress = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusBarConnectionState = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusBarRssParser = new System.Windows.Forms.ToolStripStatusLabel();
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
			this.ultraDesktopAlert = new Infragistics.Win.Misc.UltraDesktopAlert(this.components);
			this.panelRssSearch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeFeeds)).BeginInit();
			this.panelFeedDetails.SuspendLayout();
			this.panelWebDetail.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.htmlDetail)).BeginInit();
			this.panelFeedItems.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listFeedItemsO)).BeginInit();
			this._docContainer.SuspendLayout();
			this._docFeedDetails.SuspendLayout();
			this.panelClientAreaContainer.SuspendLayout();
			this.panelFeedDetailsContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Navigator)).BeginInit();
			this.pNavigatorCollapsed.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._timerTreeNodeExpand)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._timerRefreshFeeds)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._timerRefreshCommentFeeds)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ultraDesktopAlert)).BeginInit();
			this.SuspendLayout();
			// 
			// NavigatorSearch
			// 
			this.NavigatorSearch.Controls.Add(this.panelRssSearch);
			this.NavigatorSearch.Location = new System.Drawing.Point(0, 30);
			this.NavigatorSearch.Name = "NavigatorSearch";
			this.helpProvider1.SetShowHelp(this.NavigatorSearch, false);
			this.NavigatorSearch.Size = new System.Drawing.Size(322, 348);
			this.NavigatorSearch.TabIndex = 1;
			// 
			// panelRssSearch
			// 
			this.panelRssSearch.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.panelRssSearch.Controls.Add(this.treeFeeds);
			this.panelRssSearch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelRssSearch.Location = new System.Drawing.Point(0, 0);
			this.panelRssSearch.Name = "panelRssSearch";
			this.helpProvider1.SetShowHelp(this.panelRssSearch, false);
			this.panelRssSearch.Size = new System.Drawing.Size(322, 348);
			this.panelRssSearch.TabIndex = 0;
			// 
			// treeFeeds
			// 
			this.treeFeeds.Location = new System.Drawing.Point(24, 39);
			this.treeFeeds.Name = "treeFeeds";
			this.treeFeeds.Size = new System.Drawing.Size(169, 118);
			this.treeFeeds.TabIndex = 0;
			this.treeFeeds.Visible = false;
			// 
			// ultraToolTipManager
			// 
			this.ultraToolTipManager.ContainingControl = this;
			this.ultraToolTipManager.DisplayStyle = Infragistics.Win.ToolTipDisplayStyle.Office2007;
			this.ultraToolTipManager.ToolTipTextStyle = Infragistics.Win.ToolTipTextStyle.Formatted;
			// 
			// panelFeedDetails
			// 
			this.panelFeedDetails.Controls.Add(this.panelWebDetail);
			this.panelFeedDetails.Controls.Add(this.detailsPaneSplitter);
			this.panelFeedDetails.Controls.Add(this.panelFeedItems);
			this.panelFeedDetails.Location = new System.Drawing.Point(49, 24);
			this.panelFeedDetails.Name = "panelFeedDetails";
			this.helpProvider1.SetShowHelp(this.panelFeedDetails, false);
			this.panelFeedDetails.Size = new System.Drawing.Size(402, 328);
			this.panelFeedDetails.TabIndex = 998;
			// 
			// panelWebDetail
			// 
			this.panelWebDetail.Controls.Add(this.htmlDetail);
			this.panelWebDetail.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelWebDetail.Location = new System.Drawing.Point(0, 129);
			this.panelWebDetail.Name = "panelWebDetail";
			this.helpProvider1.SetShowHelp(this.panelWebDetail, false);
			this.panelWebDetail.Size = new System.Drawing.Size(402, 199);
			this.panelWebDetail.TabIndex = 997;
			// 
			// htmlDetail
			// 
			this.htmlDetail.AllowDrop = true;
			this.htmlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
			this.htmlDetail.Enabled = true;
			this.htmlDetail.Location = new System.Drawing.Point(0, 0);
			this.htmlDetail.Name = "htmlDetail";
			this.htmlDetail.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("htmlDetail.OcxState")));
			this.htmlDetail.RightToLeft = false;
			this.helpProvider1.SetShowHelp(this.htmlDetail, false);
			this.htmlDetail.Size = new System.Drawing.Size(402, 199);
			this.htmlDetail.TabIndex = 170;
			// 
			// detailsPaneSplitter
			// 
			this.detailsPaneSplitter.AnimationDelay = 20;
			this.detailsPaneSplitter.AnimationStep = 20;
			this.detailsPaneSplitter.BackColor = System.Drawing.SystemColors.Control;
			this.detailsPaneSplitter.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
			this.detailsPaneSplitter.ControlToHide = this.panelFeedItems;
			this.detailsPaneSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.detailsPaneSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailsPaneSplitter.ExpandParentForm = false;
			this.detailsPaneSplitter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.detailsPaneSplitter.Location = new System.Drawing.Point(0, 121);
			this.detailsPaneSplitter.Name = "detailsPaneSplitter";
			this.helpProvider1.SetShowHelp(this.detailsPaneSplitter, false);
			this.detailsPaneSplitter.TabIndex = 2;
			this.detailsPaneSplitter.TabStop = false;
			this.detailsPaneSplitter.UseAnimations = false;
			this.detailsPaneSplitter.VisualStyle = RssBandit.WinGui.Controls.VisualStyles.XP;
			// 
			// panelFeedItems
			// 
			this.panelFeedItems.Controls.Add(this.listFeedItemsO);
			this.panelFeedItems.Controls.Add(this.listFeedItems);
			this.panelFeedItems.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelFeedItems.Location = new System.Drawing.Point(0, 0);
			this.panelFeedItems.Name = "panelFeedItems";
			this.helpProvider1.SetShowHelp(this.panelFeedItems, false);
			this.panelFeedItems.Size = new System.Drawing.Size(402, 121);
			this.panelFeedItems.TabIndex = 1000;
			// 
			// listFeedItemsO
			// 
			this.listFeedItemsO.ColumnSettings.AllowCellEdit = Infragistics.Win.UltraWinTree.AllowCellEdit.Disabled;
			this.listFeedItemsO.ColumnSettings.AutoFitColumns = Infragistics.Win.UltraWinTree.AutoFitColumns.ResizeAllColumns;
			ultraTreeColumnSet1.AllowCellEdit = Infragistics.Win.UltraWinTree.AllowCellEdit.Disabled;
			ultraTreeNodeColumn1.AllowCellEdit = Infragistics.Win.UltraWinTree.AllowCellEdit.Disabled;
			ultraTreeNodeColumn1.ButtonDisplayStyle = Infragistics.Win.UltraWinTree.ButtonDisplayStyle.Always;
			ultraTreeNodeColumn1.Key = "Arranged by: Date";
			ultraTreeColumnSet1.Columns.Add(ultraTreeNodeColumn1);
			ultraTreeColumnSet1.Key = "csOutlook";
			this.listFeedItemsO.ColumnSettings.ColumnSets.Add(ultraTreeColumnSet1);
			this.listFeedItemsO.ColumnSettings.HeaderStyle = Infragistics.Win.HeaderStyle.XPThemed;
			this.listFeedItemsO.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFeedItemsO.FullRowSelect = true;
			this.listFeedItemsO.HideSelection = false;
			this.listFeedItemsO.ImageTransparentColor = System.Drawing.Color.Transparent;
			this.listFeedItemsO.IsUpdatingSelection = false;
			this.listFeedItemsO.Location = new System.Drawing.Point(0, 0);
			this.listFeedItemsO.Name = "listFeedItemsO";
			this.listFeedItemsO.NodeConnectorColor = System.Drawing.SystemColors.ControlDark;
			_override1.ColumnSetIndex = 0;
			_override1.ItemHeight = 35;
			_override1.SelectionType = Infragistics.Win.UltraWinTree.SelectType.Extended;
			this.listFeedItemsO.Override = _override1;
			this.listFeedItemsO.SettingsKey = "WinGuiMain.listFeedItemsO";
			this.helpProvider1.SetShowHelp(this.listFeedItemsO, false);
			this.listFeedItemsO.Size = new System.Drawing.Size(402, 121);
			this.listFeedItemsO.TabIndex = 1;
			this.listFeedItemsO.Visible = false;
			this.listFeedItemsO.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFeedListItemKeyUp);
			// 
			// listFeedItems
			// 
			this.listFeedItems.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.listFeedItems.AllowColumnReorder = true;
			this.listFeedItems.Columns.AddRange(new RssBandit.WinGui.Controls.ThListView.ThreadedListViewColumnHeader[] {
            this.colHeadline,
            this.colDate,
            this.colTopic});
			this.listFeedItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFeedItems.FullRowSelect = true;
			this.listFeedItems.HideSelection = false;
			this.listFeedItems.Location = new System.Drawing.Point(0, 0);
			this.listFeedItems.Name = "listFeedItems";
			this.listFeedItems.NoThreadChildsPlaceHolder = null;
			this.helpProvider1.SetShowHelp(this.listFeedItems, false);
			this.listFeedItems.ShowItemToolTips = true;
			this.listFeedItems.Size = new System.Drawing.Size(402, 121);
			this.listFeedItems.TabIndex = 0;
			this.listFeedItems.UseCompatibleStateImageBehavior = false;
			this.listFeedItems.View = System.Windows.Forms.View.Details;
			this.listFeedItems.ListLayoutChanged += new System.EventHandler<RssBandit.WinGui.Controls.ThListView.ListLayoutEventArgs>(this.OnFeedListLayoutChanged);
			this.listFeedItems.ListLayoutModified += new System.EventHandler<RssBandit.WinGui.Controls.ThListView.ListLayoutEventArgs>(this.OnFeedListLayoutModified);
			this.listFeedItems.ExpandThread += new System.EventHandler<RssBandit.WinGui.Controls.ThListView.ThreadEventArgs>(this.OnFeedListExpandThread);
			this.listFeedItems.AfterExpandThread += new System.EventHandler<RssBandit.WinGui.Controls.ThListView.ThreadEventArgs>(this.OnFeedListAfterExpandThread);
			this.listFeedItems.ItemActivate += new System.EventHandler(this.OnFeedListItemActivate);
			this.listFeedItems.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnFeedListItemDrag);
			this.listFeedItems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFeedListItemKeyUp);
			this.listFeedItems.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnFeedListMouseDown);
			// 
			// colHeadline
			// 
			this.colHeadline.ColumnValueType = typeof(string);
			this.colHeadline.Key = "Title";
			this.colHeadline.Text = "Headline";
			this.colHeadline.Width = 150;
			// 
			// colDate
			// 
			this.colDate.ColumnValueType = typeof(System.DateTime);
			this.colDate.Key = "Date";
			this.colDate.Text = "Date";
			this.colDate.Width = 80;
			// 
			// colTopic
			// 
			this.colTopic.ColumnValueType = typeof(string);
			this.colTopic.Key = "Subject";
			this.colTopic.Text = "Topic";
			this.colTopic.Width = 80;
			// 
			// _status
			// 
			this._status.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._status.Location = new System.Drawing.Point(0, 446);
			this._status.Name = "_status";
			this._status.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            this.statusBarBrowser,
            this.statusBarBrowserProgress,
            this.statusBarConnectionState,
            this.statusBarRssParser});
			this.helpProvider1.SetShowHelp(this._status, false);
			this._status.Size = new System.Drawing.Size(671, 30);
			this._status.TabIndex = 1003;
			// 
			// statusBarBrowser
			// 
			this.statusBarBrowser.Name = "statusBarBrowser";
			this.statusBarBrowser.Text = "Browser";
			this.statusBarBrowser.ToolTipText = "Web Browser status...";
			this.statusBarBrowser.Width = 256;
			// 
			// statusBarBrowserProgress
			// 
			this.statusBarBrowserProgress.Name = "statusBarBrowserProgress";
			this.statusBarBrowserProgress.ToolTipText = "Request page progress...";
			this.statusBarBrowserProgress.Width = 120;
			// 
			// statusBarConnectionState
			// 
			this.statusBarConnectionState.Name = "statusBarConnectionState";
			this.statusBarConnectionState.ToolTipText = "Network connection state...";
			this.statusBarConnectionState.Width = 24;
			// 
			// statusBarRssParser
			// 
			this.statusBarRssParser.Name = "statusBarRssParser";
			this.statusBarRssParser.ToolTipText = "RSS Engine state...";
			this.statusBarRssParser.Width = 250;
			// 
			// progressBrowser
			// 
			this.progressBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBrowser.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.progressBrowser.Location = new System.Drawing.Point(110, 453);
			this.progressBrowser.Name = "progressBrowser";
			this.helpProvider1.SetShowHelp(this.progressBrowser, false);
			this.progressBrowser.Size = new System.Drawing.Size(147, 18);
			this.progressBrowser.TabIndex = 1010;
			this.progressBrowser.Visible = false;
			// 
			// rightSandDock
			// 
			this.rightSandDock.Dock = System.Windows.Forms.DockStyle.Right;
			this.rightSandDock.Guid = new System.Guid("c6e4c477-596c-4e8c-9d35-840718d4c40d");
			this.rightSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
			this.rightSandDock.Location = new System.Drawing.Point(671, 0);
			this.rightSandDock.Manager = this.sandDockManager;
			this.rightSandDock.Name = "rightSandDock";
			this.helpProvider1.SetShowHelp(this.rightSandDock, false);
			this.rightSandDock.Size = new System.Drawing.Size(0, 446);
			this.rightSandDock.TabIndex = 1012;
			// 
			// sandDockManager
			// 
			this.sandDockManager.DockingManager = TD.SandDock.DockingManager.Whidbey;
			this.sandDockManager.OwnerForm = this;
			this.sandDockManager.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
			// 
			// bottomSandDock
			// 
			this.bottomSandDock.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomSandDock.Guid = new System.Guid("9ffc7b96-a550-4e79-a533-8eee52ac0da1");
			this.bottomSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
			this.bottomSandDock.Location = new System.Drawing.Point(0, 446);
			this.bottomSandDock.Manager = this.sandDockManager;
			this.bottomSandDock.Name = "bottomSandDock";
			this.helpProvider1.SetShowHelp(this.bottomSandDock, false);
			this.bottomSandDock.Size = new System.Drawing.Size(671, 0);
			this.bottomSandDock.TabIndex = 1013;
			// 
			// topSandDock
			// 
			this.topSandDock.Dock = System.Windows.Forms.DockStyle.Top;
			this.topSandDock.Guid = new System.Guid("e1c62abd-0e7a-4bb6-aded-a74f27027165");
			this.topSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
			this.topSandDock.Location = new System.Drawing.Point(0, 0);
			this.topSandDock.Manager = this.sandDockManager;
			this.topSandDock.Name = "topSandDock";
			this.helpProvider1.SetShowHelp(this.topSandDock, false);
			this.topSandDock.Size = new System.Drawing.Size(671, 0);
			this.topSandDock.TabIndex = 1014;
			// 
			// _docContainer
			// 
			this._docContainer.Controls.Add(this._docFeedDetails);
			this._docContainer.Cursor = System.Windows.Forms.Cursors.Default;
			this._docContainer.DockingManager = TD.SandDock.DockingManager.Whidbey;
			this._docContainer.Guid = new System.Guid("f032a648-4262-4312-ab2b-abe5094272bd");
			this._docContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400, System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.DocumentLayoutSystem(295, 414, new TD.SandDock.DockControl[] {
                        this._docFeedDetails}, this._docFeedDetails)))});
			this._docContainer.Location = new System.Drawing.Point(0, 30);
			this._docContainer.Manager = null;
			this._docContainer.Margin = new System.Windows.Forms.Padding(0);
			this._docContainer.Name = "_docContainer";
			this._docContainer.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
			this.helpProvider1.SetShowHelp(this._docContainer, false);
			this._docContainer.Size = new System.Drawing.Size(297, 416);
			this._docContainer.TabIndex = 100;
			// 
			// _docFeedDetails
			// 
			this._docFeedDetails.Closable = false;
			this._docFeedDetails.Controls.Add(this.panelFeedDetails);
			this._docFeedDetails.Guid = new System.Guid("9c7b7643-2ed3-402c-9e86-3c958341c81f");
			this._docFeedDetails.Location = new System.Drawing.Point(5, 35);
			this._docFeedDetails.Name = "_docFeedDetails";
			this.helpProvider1.SetShowHelp(this._docFeedDetails, false);
			this._docFeedDetails.Size = new System.Drawing.Size(287, 376);
			this._docFeedDetails.TabIndex = 150;
			this._docFeedDetails.Text = "Feed Details";
			// 
			// panelClientAreaContainer
			// 
			this.panelClientAreaContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(247)))));
			this.panelClientAreaContainer.Controls.Add(this.panelFeedDetailsContainer);
			this.panelClientAreaContainer.Controls.Add(this.splitterNavigator);
			this.panelClientAreaContainer.Controls.Add(this.Navigator);
			this.panelClientAreaContainer.Controls.Add(this.pNavigatorCollapsed);
			this.panelClientAreaContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelClientAreaContainer.Location = new System.Drawing.Point(0, 0);
			this.panelClientAreaContainer.Name = "panelClientAreaContainer";
			this.helpProvider1.SetShowHelp(this.panelClientAreaContainer, false);
			this.panelClientAreaContainer.Size = new System.Drawing.Size(671, 446);
			this.panelClientAreaContainer.TabIndex = 1015;
			// 
			// panelFeedDetailsContainer
			// 
			this.panelFeedDetailsContainer.Controls.Add(this._docContainer);
			this.panelFeedDetailsContainer.Controls.Add(this.detailHeaderCaption);
			this.panelFeedDetailsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelFeedDetailsContainer.Location = new System.Drawing.Point(374, 0);
			this.panelFeedDetailsContainer.Name = "panelFeedDetailsContainer";
			this.helpProvider1.SetShowHelp(this.panelFeedDetailsContainer, false);
			this.panelFeedDetailsContainer.Size = new System.Drawing.Size(297, 446);
			this.panelFeedDetailsContainer.TabIndex = 106;
			// 
			// detailHeaderCaption
			// 
			appearance1.BackColor = System.Drawing.Color.CornflowerBlue;
			appearance1.BackColor2 = System.Drawing.Color.MidnightBlue;
			appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
			appearance1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			appearance1.ImageHAlign = Infragistics.Win.HAlign.Right;
			appearance1.ImageVAlign = Infragistics.Win.VAlign.Middle;
			appearance1.TextHAlignAsString = "Left";
			appearance1.TextTrimming = Infragistics.Win.TextTrimming.EllipsisWord;
			appearance1.TextVAlignAsString = "Middle";
			this.detailHeaderCaption.Appearance = appearance1;
			this.detailHeaderCaption.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailHeaderCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.detailHeaderCaption.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.detailHeaderCaption.Location = new System.Drawing.Point(0, 0);
			this.detailHeaderCaption.Name = "detailHeaderCaption";
			this.detailHeaderCaption.Padding = new System.Drawing.Size(5, 0);
			this.helpProvider1.SetShowHelp(this.detailHeaderCaption, false);
			this.detailHeaderCaption.Size = new System.Drawing.Size(297, 30);
			this.detailHeaderCaption.TabIndex = 0;
			this.detailHeaderCaption.Text = "Welcome!";
			this.detailHeaderCaption.WrapText = false;
			// 
			// splitterNavigator
			// 
			this.splitterNavigator.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.splitterNavigator.Location = new System.Drawing.Point(367, 0);
			this.splitterNavigator.Name = "splitterNavigator";
			this.helpProvider1.SetShowHelp(this.splitterNavigator, false);
			this.splitterNavigator.Size = new System.Drawing.Size(7, 446);
			this.splitterNavigator.TabIndex = 1;
			this.splitterNavigator.TabStop = false;
			// 
			// Navigator
			// 
			this.Navigator.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
			this.Navigator.Controls.Add(this.NavigatorSearch);
			this.Navigator.Dock = System.Windows.Forms.DockStyle.Left;
			ultraExplorerBarGroup1.Container = this.NavigatorSearch;
			ultraExplorerBarGroup1.Key = "groupFeedsSearch";
			ultraExplorerBarGroup1.Text = "Search";
			this.Navigator.Groups.AddRange(new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup[] {
            ultraExplorerBarGroup1});
			this.Navigator.GroupSettings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;
			this.Navigator.Location = new System.Drawing.Point(45, 0);
			this.Navigator.Name = "Navigator";
			this.Navigator.NavigationMaxGroupHeaders = 5;
			this.Navigator.SettingsKey = "";
			this.helpProvider1.SetShowHelp(this.Navigator, false);
			this.Navigator.Size = new System.Drawing.Size(322, 446);
			this.Navigator.Style = Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarStyle.OutlookNavigationPane;
			this.Navigator.TabIndex = 0;
			// 
			// pNavigatorCollapsed
			// 
			this.pNavigatorCollapsed.BackColor = System.Drawing.Color.Transparent;
			this.pNavigatorCollapsed.Controls.Add(this.navigatorHiddenCaption);
			this.pNavigatorCollapsed.Dock = System.Windows.Forms.DockStyle.Left;
			this.pNavigatorCollapsed.Location = new System.Drawing.Point(0, 0);
			this.pNavigatorCollapsed.Name = "pNavigatorCollapsed";
			this.helpProvider1.SetShowHelp(this.pNavigatorCollapsed, false);
			this.pNavigatorCollapsed.Size = new System.Drawing.Size(45, 446);
			this.pNavigatorCollapsed.TabIndex = 104;
			this.pNavigatorCollapsed.Visible = false;
			// 
			// navigatorHiddenCaption
			// 
			appearance2.BackColor = System.Drawing.Color.CornflowerBlue;
			appearance2.BackColor2 = System.Drawing.Color.MidnightBlue;
			appearance2.BackGradientStyle = Infragistics.Win.GradientStyle.Horizontal;
			appearance2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			appearance2.ImageHAlign = Infragistics.Win.HAlign.Center;
			appearance2.ImageVAlign = Infragistics.Win.VAlign.Top;
			appearance2.TextHAlignAsString = "Left";
			appearance2.TextTrimming = Infragistics.Win.TextTrimming.EllipsisWord;
			appearance2.TextVAlignAsString = "Top";
			this.navigatorHiddenCaption.Appearance = appearance2;
			this.navigatorHiddenCaption.Dock = System.Windows.Forms.DockStyle.Left;
			this.navigatorHiddenCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.navigatorHiddenCaption.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.navigatorHiddenCaption.Location = new System.Drawing.Point(0, 0);
			this.navigatorHiddenCaption.Name = "navigatorHiddenCaption";
			this.navigatorHiddenCaption.Padding = new System.Drawing.Size(0, 5);
			this.helpProvider1.SetShowHelp(this.navigatorHiddenCaption, false);
			this.navigatorHiddenCaption.Size = new System.Drawing.Size(35, 446);
			this.navigatorHiddenCaption.TabIndex = 105;
			this.navigatorHiddenCaption.Text = "Feed Subscriptions";
			this.navigatorHiddenCaption.WrapText = false;
			// 
			// _startupTimer
			// 
			this._startupTimer.Interval = 45000;
			this._startupTimer.Tick += new System.EventHandler(this.OnTimerStartupTick);
			// 
			// _timerTreeNodeExpand
			// 
			this._timerTreeNodeExpand.SynchronizingObject = this;
			// 
			// _timerRefreshFeeds
			// 
			this._timerRefreshFeeds.Interval = 600000D;
			this._timerRefreshFeeds.SynchronizingObject = this;
			this._timerRefreshFeeds.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerFeedsRefreshElapsed);
			// 
			// _timerRefreshCommentFeeds
			// 
			this._timerRefreshCommentFeeds.Interval = 600000D;
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
			this.helpProvider1.HelpNamespace = "BanditHelp.chm";
			// 
			// _timerDispatchResultsToUI
			// 
			this._timerDispatchResultsToUI.Interval = 250;
			// 
			// ultraDesktopAlert
			// 
			this.ultraDesktopAlert.Style = Infragistics.Win.Misc.DesktopAlertStyle.Office2007;
			// 
			// WinGuiMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 17);
			this.ClientSize = new System.Drawing.Size(671, 476);
			this.Controls.Add(this.panelClientAreaContainer);
			this.Controls.Add(this.rightSandDock);
			this.Controls.Add(this.bottomSandDock);
			this.Controls.Add(this.topSandDock);
			this.Controls.Add(this.progressBrowser);
			this.Controls.Add(this._status);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.helpProvider1.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.TableOfContents);
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(490, 304);
			this.Name = "WinGuiMain";
			this.helpProvider1.SetShowHelp(this, true);
			this.Text = "RSS Bandit";
			this.panelRssSearch.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeFeeds)).EndInit();
			this.panelFeedDetails.ResumeLayout(false);
			this.panelWebDetail.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.htmlDetail)).EndInit();
			this.panelFeedItems.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listFeedItemsO)).EndInit();
			this._docContainer.ResumeLayout(false);
			this._docFeedDetails.ResumeLayout(false);
			this.panelClientAreaContainer.ResumeLayout(false);
			this.panelFeedDetailsContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Navigator)).EndInit();
			this.pNavigatorCollapsed.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._timerTreeNodeExpand)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._timerRefreshFeeds)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._timerRefreshCommentFeeds)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ultraDesktopAlert)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        #region Statusbar/Docking routines

        private void InitStatusBar()
        {
            //_status.ItemClicked += OnStatusPanelClick;
            statusBarConnectionState.DoubleClick += OnConnectionStateDoubleClick;
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
#if PHOENIX
        public ITextImageItem[] GoBackHistoryItems
        {
			get; set;
#else
        public ITextImageItem[] GoBackHistoryItems(int maxItems)
        {
			return _feedItemImpressionHistory.GetHeadOfPreviousEntries(maxItems); 
#endif
        }
#if PHOENIX
        public ITextImageItem[] GoForwardHistoryItems
        {
			get; set; 
#else
        public ITextImageItem[] GoForwardHistoryItems(int maxItems)
        {
            return _feedItemImpressionHistory.GetHeadOfNextEntries(maxItems);
#endif
        }
		public ITextImageItem CurrentHistoryItem { get; set; }
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

        private void OnNewsItemSearchFinished(object sender, FeedSourceManager.SearchFinishedEventArgs e)
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
                                                    _log.Error(
                                                        "AsyncCall 'StartNewsSearchDelegate' caused this exception", ex);
                                                }
                                            }
                                        }, null);
        }

        private void StartNewsSearch(FinderNode node)
        {
			owner.FeedSources.SearchNewsItems(
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
                searchPanel.SetControlStateTo(ItemSearchState.Finished,
                                              String.Format(SR.RssSearchSuccessResultMessage, newsItemsCount));
            }

            var finder = tag as RssFinder;
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

            var result = new ArrayList();

            if (categories != null)
            {
				string sep = FeedSource.CategorySeparator;
				foreach (FeedSourceEntry e in owner.FeedSources.Sources)
					foreach (var f in e.Source.GetFeeds().Values)
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
					if (url != null && CurrentSelectedFeedSource.Source.IsSubscribed(url))
                    {
						result.Add(CurrentSelectedFeedSource.Source.GetFeeds()[url]);
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
			start.BeginInvoke(searchUrl, _searchResultNode,
				cb =>
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
				owner.FeedSources.SearchRemoteFeed(searchUrl, resultContainer.Finder);
            }
            catch (Exception ex)
            {
                _log.Error(String.Format("RSS Remote Search '{0}' caused exception", searchUrl), ex);
                InvokeOnGuiSync(() =>
                                SetSearchStatusText("Search '" + StringHelper.ShortenByEllipsis(searchUrl, 30) +
                                                    "' caused a problem: " +
                                                    ex.Message));
            }
        }

        #endregion

        //toastNotify callback (not called on main window thread!)
		void OnToastNotificationAction(object sender, NotifierActionEventArgs e)
		{
			switch (e.Action)
			{
				case NotifierAction.ActivateItem:
					if (e.DownloadItem != null)
						PlayEnclosure(e.DownloadItem);
					else
						DelayTask(DelayedTasks.NavigateToFeedNewsItem, e.NewsItem);
					break;
				case NotifierAction.ActivateFeed:
					DelayTask(DelayedTasks.NavigateToFeed, e.NewsFeed);
					break;
				case NotifierAction.ShowFeedProperties:
					DelayTask(DelayedTasks.ShowFeedPropertiesDialog, e.NewsFeed);
					break;
			}
		}

		//private void OnExternalDisplayFeedProperties(INewsFeed f)
		//{
		//	DelayTask(DelayedTasks.ShowFeedPropertiesDialog, f);
		//}

		//private void OnExternalActivateFeedItem(INewsItem item)
		//{
		//	DelayTask(DelayedTasks.NavigateToFeedNewsItem, item);
		//}

		//private void OnExternalActivateFeed(INewsFeed f)
		//{
		//	DelayTask(DelayedTasks.NavigateToFeed, f);
		//}

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
            	FeedSourceEntry entry = FeedSourceEntryOf(feedsNode);
            	if (entry != null) SelectFeedSource(entry);
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
			NavigateToNode(TreeHelper.FindNode(
				GetAllSubscriptionRootNodes().ConvertAll(
				n => {
					return (TreeFeedsNodeBase)n;
				}), f));
		}
		
	    private void NavigateToFeedNewsItem(INewsItem item)
	    {
		    if (item != null)
		    {
				NavigateToFeed(item.Feed);
			    var listItemToSelect = listFeedItems.Items.FirstOrDefault(lvi => item.Equals(lvi.Key));
			    if (listItemToSelect != null)
			    {
				    listItemToSelect.Selected = true;
					OnFeedListItemActivateManually(listItemToSelect);
			    }
		    }
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
        	HistoryEntry entry = new HistoryEntry(feedsNode, item);
#if PHOENIX
            // we have to switch off events:
        	ultraToolbarsManager.EventManager.AllEventsEnabled = false;
			ultraToolbarsManager.NavigationToolbar.NavigateTo(new NavigationHistoryItem(entry.ToString(), entry));
			ultraToolbarsManager.EventManager.AllEventsEnabled = true;
#else
			_feedItemImpressionHistory.Add(new HistoryEntry(feedsNode, item));
#endif

			if (item != null && item.Link != null && TaskbarManager.IsPlatformSupported)
            {
                string urlToFeedAndItem = item.Link.Replace("http://", "feed://") + "*" + item.FeedLink; 
                AddUrlToJumpList(urlToFeedAndItem, item.Title); 
            }
        }    

        private void AutoSubscribeFeed(TreeFeedsNodeBase parent, string feedUrl)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return;

            if (parent == null)
                parent = GetRoot(RootFolderType.MyFeeds);

        	FeedSourceEntry entry = FeedSourceEntryOf(parent);
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

					if (!multipleSubscriptions && entry.Source.IsSubscribed(feedUrl))
                    {
						INewsFeed f2 = entry.Source.GetFeeds()[feedUrl];
                        owner.MessageInfo(String.Format(SR.GUIFieldLinkRedundantInfo,
                                                        (f2.category == null
                                                             ? String.Empty
                                                             : f2.category + FeedSource.CategorySeparator) + f2.title,
                                                        f2.link));
                        return;
                    }

                    if (owner.InternetAccessAllowed)
                    {
                        var fetchHandler = new PrefetchFeedThreadHandler(feedUrl, owner.Proxy);

                        DialogResult result = fetchHandler.Start(this,
                                                                 String.Format(SR.GUIStatusWaitMessagePrefetchFeed,
                                                                               feedUrl));

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
								if (!entry.Source.HasCategory(category))
                                {
									entry.Source.AddCategory(category);
                                }
								entry.Source.ChangeCategory(f, category);

                                f.alertEnabled = false;
								f = entry.Source.AddFeed(f);
                                owner.FeedWasModified(f, NewsFeedProperty.FeedAdded);
                                //owner.FeedlistModified = true;

                                AddNewFeedNode(entry, f.category, f);

                                try
                                {
									entry.Source.AsyncGetItemsForFeed(f.link, true, true);
                                }
                                catch (Exception e)
                                {
                                    owner.PublishXmlFeedError(e, f, true, entry);
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

        private void ApplyAfterAllSubscriptionListsLoaded()
        {
            listFeedItems.FeedColumnLayout = owner.GlobalFeedColumnLayout;
            
			LoadAndRestoreSubscriptionTreeState();
            if (Visible)
            {
                LoadAndRestoreBrowserTabState();
            }
			
			//owner.AskAndCheckForDefaultAggregator();

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
                                    owner.Preferences.ReferrerFont, owner.Preferences.ReferrerFontColor,
                                    owner.Preferences.NewCommentsFont, owner.Preferences.NewCommentsFontColor
                                    );

                                owner.Mediator.SetEnabled(owner.Preferences.UseRemoteStorage, "cmdUploadFeeds",
                                                          "cmdDownloadFeeds");

                                if (Visible)
                                {
                                    // initiate a refresh of the INewsItem detail pane
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

        private void OnFeedDeleted(object sender, FeedSourceFeedUrlTitleEventArgs e)
        {
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;
			
			// user clicks to "remove feed" within feed detail view, so clean this if it was the recent view:
	        bool isExceptionNodeItemDisplayed = false;
	        if (this.CurrentSelectedFeedItem != null && this.CurrentSelectedFeedItem.Feed != null)
		        isExceptionNodeItemDisplayed = this.CurrentSelectedFeedItem.Feed is ExceptionManager;
	        
            ExceptionNode.UpdateReadStatus();
			PopulateSmartFolder((TreeFeedsNodeBase)ExceptionNode, isExceptionNodeItemDisplayed);

            if (tn == null || tn.Type != FeedNodeType.Feed || e.FeedUrl != tn.DataKey)
            {
                tn = TreeHelper.FindNode(GetSubscriptionRootNode(e.Entry), e.FeedUrl);
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

		private void OnFeedSourceSubscriptionsLoaded(object sender, FeedSourceEventArgs e)
		{
			PopulateFeedSubscriptions(e.Entry, RssBanditApplication.DefaultCategory);
		}

		void OnAllFeedSourceSubscriptionsLoaded(object sender, EventArgs e)
		{
			//remember subscription tree state:
			ApplyAfterAllSubscriptionListsLoaded();

			SetGuiStateFeedback(SR.GUIStatusDone);
			
#if !NOAUTO_REFRESH
			// start the refresh timers
			_timerRefreshFeeds.Start();
			_timerRefreshCommentFeeds.Start();
#else
			Trace.WriteLine("ATTENTION!. REFRESH TIMER DISABLED FOR DEBUGGING!");
#endif
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
                FeedSource source = FeedSourceOf(feedUrl); 
                source.AsyncGetItemsForFeed(feedUrl, true, true);
            }

            if (_uiTasksTimer[DelayedTasks.SaveConfiguration])
            {
                _uiTasksTimer.StopTask(DelayedTasks.SaveConfiguration);
                SaveConfiguration(false);
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

		private void OnNavigatorSelectedGroupChanging(object sender, CancelableGroupEventArgs e)
		{
			if (e.Group.Key == Resource.NavigatorGroup.RssSearch)
				return;

			TreeFeedsNodeBase myRoot = GetSubscriptionRootNode(e.Group.Text);
			if (myRoot != null)
			{
				// remember subscription node selection:
				if (Navigator.SelectedGroup.Key != Resource.NavigatorGroup.RssSearch) 
					Navigator.SelectedGroup.Tag = TreeSelectedFeedsNode;
				// hide all other root nodes:
				ShowSubscriptionRootNodes(false);
				// make my root node visible:
				myRoot.Visible = true;
				_lastVisualFeedSource = myRoot.Text;
				treeFeeds.Parent.Controls.Remove(treeFeeds);
				e.Group.Container.Controls.Add(treeFeeds);
				if (!treeFeeds.Visible)
				{
					treeFeeds.Dock = DockStyle.Fill;
					treeFeeds.Visible = true;
				}
			}
		}

		private void OnNavigatorSelectedGroupChanged(object sender, GroupEventArgs e)
		{
			if (e.Group.Key == Resource.NavigatorGroup.RssSearch)
			{
				owner.Mediator.SetEnabled("-cmdFeedSourceProperties");
				owner.Mediator.SetEnabled("-cmdDeleteFeedSource");
			}
			else
			{
				SubscriptionRootNode myRoot = GetSubscriptionRootNode(e.Group.Text);
				if (myRoot != null)
				{
					// restore node selection:
					TreeFeedsNodeBase groupSelectedNode = e.Group.Tag as TreeFeedsNodeBase;
					if (groupSelectedNode != null)
					{
						TreeSelectedFeedsNode = groupSelectedNode;
						OnTreeFeedAfterSelectManually(groupSelectedNode);
					}
					owner.Mediator.SetEnabled("+cmdFeedSourceProperties");
					owner.Mediator.SetEnabled(FeedSourceType.DirectAccess != owner.FeedSources[myRoot.SourceID].SourceType,
						"cmdDeleteFeedSource");
				}
			}
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
            List<INewsItem> groupSelected = null;
            //Select same nodes in listFeedItems ListView
            listFeedItems.SelectedItems.Clear();
            for (int i = 0; i < listFeedItemsO.Nodes.Count; i++)
            {
                if (listFeedItemsO.Nodes[i].Selected)
                {
                    if (groupSelected == null)
                        groupSelected = new List<INewsItem>();
                    //Select all child nodes
                    for (int j = 0; j < listFeedItemsO.Nodes[i].Nodes.Count; j++)
                    {
                        var node = (UltraTreeNodeExtended) listFeedItemsO.Nodes[i].Nodes[j];
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
                        var node = (UltraTreeNodeExtended) listFeedItemsO.Nodes[i].Nodes[j];
                        if (node.NodeOwner != null)
                            node.NodeOwner.Selected = true;
                    }
                    //Comments
                    for (int k = 0; k < listFeedItemsO.Nodes[i].Nodes[j].Nodes.Count; k++)
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
                TreeFeedsNodeBase tn = TreeSelectedFeedsNode;
                FeedSource source = FeedSourceOf(tn); 
                if (tn != null)
                {
                    if (tn.Type == FeedNodeType.Category)
                    {
                        string category = tn.CategoryStoreName;
                        var redispItems = BuildGroupedFeedInfoList(category, groupSelected);

						SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
						BeginTransformFeedList(redispItems, tn, source.GetCategoryStyleSheet(category));
                    }
                    else
                    {
                        string feedUrl = tn.DataKey;
                        IFeedDetails fi = source.GetFeedDetails(feedUrl);

                        if (fi != null)
                        {
                            var fi2 = new FeedInfo(fi, null); //(IFeedDetails) fi.Clone();
                            //fi2.ItemsList.Clear();
                            foreach (INewsItem ni in groupSelected)
                            {
                                fi2.ItemsList.Add(ni);
                            }
							SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
							BeginTransformFeed(fi2, tn, source.GetStyleSheet(tn.DataKey));
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
            UltraTreeNode n = listFeedItemsO.ActiveNode ?? listFeedItemsO.TopNode;
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

        #region Windows 7 related event handlers

        void OnTaskBarButtonAddClicked(object sender, EventArgs e)
        {
            this.AddFeedUrlSynchronized(String.Empty); 
        }


        void OnTaskBarButtonRefreshClick(object sender, EventArgs e)
        {
            this.UpdateAllFeeds(true); 
        }

        private void OnPictureBoxSizeChanged(object sender, EventArgs e)
        {
            TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(this.Handle, new Rectangle(pictureBox.Location, pictureBox.Size));
        }

        #endregion

    } // end class WinGuiMain
}
