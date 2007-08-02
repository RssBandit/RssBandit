#region CVS Version Header
/*
 * $Id: WinGUIMain.cs,v 1.313 2005/06/05 17:13:12 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/06/05 17:13:12 $
 * $Revision: 1.313 $
 */
#endregion

//#undef USEAUTOUPDATE
#define USEAUTOUPDATE

#region framework namespaces
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Web;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.Permissions;
#endregion

#region third party namespaces
using TD.SandBar;	// toolbars		
using TD.SandDock;	// docking panels, docked tabs
using IEControl;		// external webbrowser control
using SHDocVw;		// related interfaces
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
#endregion

#region project namespaces
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Collections;
using NewsComponents.Search;
using NewsComponents.RelationCosmos;
using NewsComponents.Utils;
using NewsComponents.Net;

using RssBandit.Filter;
using RssBandit.WebSearch;
using RssBandit.WinGui;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Menus;
using K = RssBandit.Utility.Keyboard;
using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Utility;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Interfaces;
using System.Windows.Forms.ThListView;
#endregion

//[assembly:CLSCompliant(true)]
//TODO: add permission sets

namespace RssBandit.WinGui.Forms {

	/// <summary>
	/// Enumeration that defines the possible embedded web browser actions
	/// to perform from the main application.
	/// </summary>
	public enum BrowseAction {
		NavigateCancel,
		NavigateBack,
		NavigateForward,
		DoRefresh
	}

	/// <summary>
	/// Enumeration that defines the type of the known root folders
	/// of Bandit displayed within the treeview.
	/// </summary>
	public enum RootFolderType {
		MyFeeds,
		SmartFolders,
		Finder
	}

	/// <summary>
	/// Enum defines the rss search states
	/// </summary>
	public enum RssSearchState {
		Pending,			// no search running or canceling
		Searching,		// search in progress
		Canceled		// search canceled in UI but not worker
	}


	/// <summary>
	/// Used to delay execution of some UI tasks by a timer
	/// </summary>
	[Flags]public enum DelayedTasks {
		None = 0,
		NavigateToWebUrl = 1,
		StartRefreshOneFeed = 2,
		StartRefreshAllFeeds = 4,
		ShowFeedPropertiesDialog = 8,
		NavigateToFeedNewsItem = 16, 
		AutoSubscribeFeedUrl = 32,
		ClearBrowserStatusInfo = 64,
		RefreshTreeStatus = 128,
		SyncRssSearchTree = 256,
		InitOnFinishLoading = 512,
		SaveUIConfiguration = 1024,
		NavigateToFeed = 2048,
	}

	/// <summary>
	/// Summary description for WinGuiMain.
	/// </summary>
	//[CLSCompliant(true)]
	internal class WinGuiMain : System.Windows.Forms.Form, ITabState, IMessageFilter {

		#region delegates and events declarations
		/// <summary>
		/// Delegate used for calling UpdateTreeStatus(Hashtable) in the correct thread.
		/// </summary>
		/// <remarks>Read the article at 
		/// http://msdn.microsoft.com/library/en-us/dnforms/html/winforms06112002.asp
		/// for an explanation of why this delegate is needed.</remarks>
		public delegate void UpdateTreeStatusDelegate(FeedsCollection theFeeds, RootFolderType rootFolder);
		delegate void PopulateTreeFeedsDelegate(CategoriesCollection categories, FeedsCollection feedsTable, string defaultCategory);
		/// <summary>
		/// Delegate used for calling PopulateListView() in the correct thread.
		/// </summary>
		delegate void PopulateListViewDelegate(FeedTreeNodeBase associatedNode, ArrayList list, bool forceReload, bool categorizedView, FeedTreeNodeBase initialNode);
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
		public delegate void GetCommentNewsItemsDelegate(NewsItem item, ThreadedListViewItem listViewItem );
 
		/// <summary>
		/// used to start NewsSearch asynchron
		/// </summary>
		private delegate void StartNewsSearchDelegate(FinderNode node) ;
		
		/// <summary>
		/// used to start RssRemoteSearch asynchron
		/// </summary>
		private delegate void StartRssRemoteSearchDelegate(string searchUrl, FinderNode resultContainer) ;

		/// <summary>
		/// Called from within a toast window
		/// </summary>
		public delegate void NavigateToURLDelegate(string url, string tab, bool createNewTab, bool setFocus);		

		/// <summary>
		/// Command line param
		/// </summary>
		delegate void SubscribeToFeedUrlDelegate(string newFeedUrl);
		
		// there is really no such thing on the native form interface :-(
		public event EventHandler OnMinimize;

		#endregion

		#region private variables

		private const int BrowserProgressBarWidth = 120;
		private const int MaxHeadlineWidth = 133;

		private static readonly log4net.ILog _log  = Common.Logging.Log.GetLogger(typeof(WinGuiMain));

		// Regex used to workaround the tree label edit behavior.
		// If a node has some unread items it is displayed "Node Caption (xxx)",
		// where xxx is the unread item count.
		// What we want: remove this part "(xxx)" before edit, but this does not
		// work anyway. So we remove such text after editing with the following
		// regex... :-(
		private static Regex _labelParser = new Regex(@"(?<caption>.*)(?<unreadCounter>\s*\(\d*\)\s*$)", RegexOptions.Compiled);
	
//		private static string DefaultFeedFeedColumnLayout = FeedColumnLayout.SaveAsXML(new FeedColumnLayout(new string[]{"Title", "Subject", "Date"}, new int[]{ 250, 120, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.GlobalFeedLayout));
//		private static string DefaultCategoryFeedColumnLayout = FeedColumnLayout.SaveAsXML(new FeedColumnLayout(new string[]{"Title", "Subject", "Date", "FeedTitle"}, new int[]{ 250, 120, 100, 100}, "Date", NewsComponents.SortOrder.Descending, LayoutType.GlobalCategoryLayout));
//
//		private string CurrentFeedFeedColumnLayout = DefaultFeedFeedColumnLayout;
//		private string CurrentCategoryFeedColumnLayout = DefaultCategoryFeedColumnLayout;

		private ToastNotifier _toastNotifier ;
		private WheelSupport _wheelSupport;
		private UrlCompletionExtender urlExtender;

		private FeedTreeNodeBase[] _roots = new FeedTreeNodeBase[3];		// store refs to root folders (they order within the treeview may be resorted depending on the languages)
		private FeedTreeNodeBase _feedExceptionsNode = null;
		private FeedTreeNodeBase _sentItemsNode = null;
		private FeedTreeNodeBase _deletedItemsNode = null;
		private FeedTreeNodeBase _flaggedFeedsNodeFollowUp = null;
		private FeedTreeNodeBase _flaggedFeedsNodeRead = null;
		private FeedTreeNodeBase _flaggedFeedsNodeReview = null;
		private FeedTreeNodeBase _flaggedFeedsNodeForward = null;
		private FeedTreeNodeBase _flaggedFeedsNodeReply = null;
		private FinderNode _searchResultNode = null;

		//private ListViewSortHelper _lvSortHelper = null;
		private NewsItemFilterManager _filterManager = null;
		
		// current rss search state
		private RssSearchState _rssSearchState = RssSearchState.Pending;

		private ArrayList addIns = null;

		// store the HashCodes of temp. NewsItems, that have bean read.
		// Used for commentRss implementation
		private Hashtable tempFeedItemsRead = new Hashtable();	

		// used to store temp. the currently yet populated feeds to speedup category population
		private HybridDictionary feedsCurrentlyPopulated = new HybridDictionary(true);

		private K.ShortcutHandler _shortcutHandler;

		private NewsItem _currentNewsItem = null;
		private FeedTreeNodeBase _currentSelectedFeedNode = null;		// currently selected node at the treeView (could be also temp. change on Right-Click, so it is different from treeView.SelectedNode )
		private FeedTreeNodeBase _currentDragNode = null;
		private FeedTreeNodeBase _currentDragHighlightNode = null;
		
		private Rectangle _formRestoreBounds = Rectangle.Empty;

		//these are here, because we only want to display the balloon popup,
		//if there are really new items received:
		private int _lastUnreadFeedItemCountBeforeRefresh = 0;
		private int _beSilentOnBalloonPopupCounter = 0;	// if a user explicitly close the balloon, we are silent for the next 12 retries (we refresh all 5 minutes, so this equals at least to one hour)

		private bool _forceShutdown = false;
		private bool _initialStartupTrayVisibleOnly = false;
		private FormWindowState initialStartupState = FormWindowState.Normal;

		private int _webTabCounter = 0;
		
		// variables set in PreFilterMessage() to indicate the user clicked an url; reset also there on mouse-move, or within WebBeforeNavigate event
		private bool _webUserNavigated = false;
		private bool _webForceNewTab = false;
		private Point _lastMousePosition = Point.Empty; 

		// GUI main components:
		private ImageList _toolImages = null;
		private ImageList _browserImages = null;
		private ImageList _treeImages = null;
		private ImageList _listImages = null;
		private ImageList _searchEngineImages = null;

		private System.Windows.Forms.ContextMenu _treeRootContextMenu = null;
		private System.Windows.Forms.ContextMenu _treeCategoryContextMenu = null;
		private System.Windows.Forms.ContextMenu _treeFeedContextMenu = null;
		private System.Windows.Forms.ContextMenu _notifyContextMenu = null;
		private System.Windows.Forms.ContextMenu _listContextMenu = null;
		private System.Windows.Forms.ContextMenu _treeLocalFeedContextMenu = null;
		private System.Windows.Forms.ContextMenu _treeSearchFolderRootContextMenu = null;
		private System.Windows.Forms.ContextMenu _treeSearchFolderContextMenu = null;
		private System.Windows.Forms.ContextMenu _treeTempSearchFolderContextMenu = null;
		private System.Windows.Forms.ContextMenu _docTabContextMenu = null;

		// Used to temp. store the context menu position. Processed later within
		// ICommand event receiver (in Screen-Coordinates).
		private Point _contextMenuCalledAt = Point.Empty;

		private MenuItem _listContextMenuDeleteItemsSeparator = null;

		private AppToolMenuCommand _searchesGoCommand = null;

		private TrayStateManager _trayManager = null;
		private NotifyIconAnimation _trayAni = null;

		private TD.SandBar.ComboBoxItem navigateComboBox = null;
		private TD.SandBar.ComboBoxItem searchComboBox = null;

		private string _tabStateUrl;

		// the GUI owner and Form controller
		private RssBanditApplication owner;
		private System.Windows.Forms.Panel panelFeedDetails;
		private System.Windows.Forms.Panel panelFeedItems;
		private System.Windows.Forms.Panel panelWebDetail;
		private ThreadedListView listFeedItems;

		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Timer _timerResetStatus;
		private System.Timers.Timer _timerTreeNodeExpand; 
		private System.Timers.Timer _timerRefreshFeeds; 
		private IEControl.HtmlControl htmlDetail;
		private System.Windows.Forms.TreeView treeFeeds;
		private System.Windows.Forms.StatusBar _status;
		private TD.SandBar.SandBarManager sandBarManager;
		private TD.SandBar.ToolBarContainer leftSandBarDock;
		private TD.SandBar.ToolBarContainer rightSandBarDock;
		private TD.SandBar.ToolBarContainer bottomSandBarDock;
		private TD.SandBar.ToolBarContainer topSandBarDock;
		private TD.SandBar.ToolBar toolBarMain;
		private TD.SandBar.MenuBar menuBarMain;
		private TD.SandBar.ToolBar toolBarBrowser;
		private TD.SandBar.ToolBar toolBarWebSearch;
		private System.Windows.Forms.StatusBarPanel statusBarBrowser;
		private System.Windows.Forms.StatusBarPanel statusBarBrowserProgress;
		private System.Windows.Forms.StatusBarPanel statusBarConnectionState;
		private System.Windows.Forms.StatusBarPanel statusBarRssParser;
		private System.Windows.Forms.ProgressBar progressBrowser;
		private System.Windows.Forms.Panel panelRssSearch;
		private UITaskTimer _uiTasksTimer;
		private System.Windows.Forms.TextBox textSearchExpression;
		private System.Windows.Forms.Button btnSearchCancel;
		private System.Windows.Forms.TextBox textFinderCaption;
		private System.Windows.Forms.Label labelSearchFolderNameHint;
		private System.Windows.Forms.RadioButton radioRssSearchSimpleText;
		private System.Windows.Forms.RadioButton radioRssSearchRegEx;
		private System.Windows.Forms.RadioButton radioRssSearchExprXPath;
		private System.Windows.Forms.Label labelRssSearchTypeHint;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxRssSearchUnreadItems;
		private System.Windows.Forms.CheckBox checkBoxRssSearchInTitle;
		private System.Windows.Forms.CheckBox checkBoxRssSearchInDesc;
		private System.Windows.Forms.CheckBox checkBoxRssSearchInLink;
		private System.Windows.Forms.CheckBox checkBoxRssSearchInCategory;
		private System.Windows.Forms.CheckBox checkBoxConsiderItemReadState;
		private System.Windows.Forms.RadioButton radioRssSearchItemsOlderThan;
		private System.Windows.Forms.ComboBox comboRssSearchItemAge;
		private System.Windows.Forms.CheckBox checkBoxRssSearchTimeSpan;
		private System.Windows.Forms.RadioButton radioRssSearchItemsYoungerThan;
		private System.Windows.Forms.CheckBox checkBoxRssSearchByDate;
		private System.Windows.Forms.ComboBox comboBoxRssSearchItemPostedOperator;
		private System.Windows.Forms.DateTimePicker dateTimeRssSearchItemPost;
		private System.Windows.Forms.CheckBox checkBoxRssSearchByDateRange;
		private System.Windows.Forms.DateTimePicker dateTimeRssSearchPostAfter;
		private System.Windows.Forms.DateTimePicker dateTimeRssSearchPostBefore;
		private System.Windows.Forms.Panel panelRssSearchCommands;
		private System.Windows.Forms.Button btnNewSearch;
		private System.Windows.Forms.Label labelRssSearchState;
		private System.Windows.Forms.Button btnRssSearchSave;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private TD.SandDock.SandDockManager sandDockManager;
		private TD.SandDock.DockContainer leftSandDock;
		private TD.SandDock.DockContainer rightSandDock;
		private TD.SandDock.DockContainer bottomSandDock;
		private TD.SandDock.DockContainer topSandDock;
		private TD.SandDock.DockControl dockSubscriptions;
		private TD.SandDock.DockControl dockSearch;
		private TD.SandDock.DocumentContainer _docContainer;
		private TD.SandDock.DockControl _docFeedDetails;
		private System.Windows.Forms.ThListView.ThreadedListViewColumnHeader colHeadline;
		private System.Windows.Forms.ThListView.ThreadedListViewColumnHeader colDate;
		private System.Windows.Forms.ThListView.ThreadedListViewColumnHeader colTopic;
		private XPExplorerBar.Expando collapsiblePanelSearchNameEx;
		private XPExplorerBar.Expando collapsiblePanelRssSearchExprKindEx;
		private XPExplorerBar.Expando collapsiblePanelItemPropertiesEx;
		private XPExplorerBar.Expando collapsiblePanelAdvancedOptionsEx;
		private System.Windows.Forms.Label horizontalEdge;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private XPExplorerBar.TaskPane taskPaneSearchOptions;
		private XPExplorerBar.Expando collapsiblePanelRssSearchScopeEx;
		private System.Windows.Forms.TreeView treeRssSearchScope;
		private CollapsibleSplitter detailsPaneSplitter;
		private CollapsibleSplitter searchPaneSplitter;

 
		private System.ComponentModel.IContainer components;

		#endregion

		#region Class initialize

		public WinGuiMain(RssBanditApplication theGuiOwner, FormWindowState initialFormState) {
			GuiOwner = theGuiOwner;
			this.initialStartupState = initialFormState;
			_wheelSupport = new WheelSupport(this);
			_wheelSupport.OnGetChildControl += new RssBandit.WinGui.Utility.WheelSupport.OnGetChildControlHandler(this.OnWheelSupportGetChildControl);

			urlExtender = new UrlCompletionExtender(this);
			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Init();
		}

		protected void Init() {

			this.OnMinimize += new EventHandler(this.OnFormMinimize);
			this.MouseDown += new MouseEventHandler(OnFormMouseDown);
			
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			this.SetFontAndColor(
				owner.Preferences.NormalFont, owner.Preferences.NormalFontColor,
				owner.Preferences.HighlightFont, owner.Preferences.HighlightFontColor,
				owner.Preferences.FlagFont, owner.Preferences.FlagFontColor,
				owner.Preferences.ErrorFont, owner.Preferences.ErrorFontColor,
				owner.Preferences.RefererFont, owner.Preferences.RefererFontColor
				);

			InitResources();
			InitShortcutManager();
			InitMenuBar();
			InitToolBars();		
			InitStatusBar();
			InitDockHosts();
			InitDocumentManager();
			InitContextMenus();
			InitTrayIcon();
			InitWidgets();
		}

		protected void InitFilter() {
			_filterManager = new NewsItemFilterManager(this);
			_filterManager.Add("NewsItemReferrerFilter", new NewsItemReferrerFilter(owner));
			_filterManager.Add("NewsItemFlagFilter", new NewsItemFlagFilter(owner));
		}

		#endregion

		#region public properties/accessor routines

		/// <summary>
		/// Gets and sets the GUI owner application
		/// </summary>
		public RssBanditApplication GuiOwner {
			get { return owner; }
			set { owner = value;}
		}

		/// <summary>
		/// Initialized on Class init with the initial Form state (usually defined on a Shortcut)
		/// </summary>
		public FormWindowState InitialStartupState {
			get { return initialStartupState; }
		}

		/// <summary>
		/// Provide access to the current entry text within the navigation dropdown
		/// </summary>
		public string UrlText {
			get { 
				if (navigateComboBox.ControlText.Trim().Length > 0) 
					return navigateComboBox.ControlText; 
				else
					return _urlText; 
			}
			set { 
				_urlText = (value == null ? String.Empty: value);
				if (_urlText.Equals("about:blank")) {
					_urlText = String.Empty;
				}
				navigateComboBox.ControlText = _urlText; 
			}
		}
		private string _urlText = String.Empty;	// don't know why navigateComboBox.ComboBox.Text returns an empty string after dragging a url to it, so we use this as a workaround

		/// <summary>
		/// Provide access to the current entry text within the web search dropdown
		/// </summary>
		public string WebSearchText {
			get { 
				if (searchComboBox.ComboBox.Text.Trim().Length > 0) 
					return searchComboBox.ComboBox.Text; 
				else
					return _webSearchText; 
			}
			set { 
				_webSearchText = (value == null ? String.Empty: value);
				searchComboBox.ComboBox.Text = _webSearchText; 
			}
		}
		private string _webSearchText = String.Empty;	// don't know why searchComboBox.ComboBox.Text returns an empty string after dragging a url to it, so we use this as a workaround

		/// <summary>
		/// Gets the current UI state of rss search
		/// </summary>
		public RssSearchState CurrentSearchState {
			get { return _rssSearchState; }
		}

		/// <summary>
		/// Return the TreeNode instance representing/store of 
		/// the FeedExceptions.
		/// </summary>
		public ISmartFolder ExceptionNode {
			get { return _feedExceptionsNode as ISmartFolder; }
		}

		/// <summary>
		/// Return the TreeNode instance representing/store of 
		/// the FlaggedFeeds. Because they all share the item list, it is enough
		/// to return one of them
		/// </summary>
		public ISmartFolder FlaggedFeedsNode(Flagged flag) {
			switch (flag) {
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
		public ISmartFolder SentItemsNode {
			get { return _sentItemsNode as ISmartFolder; }
		}

		/// <summary>
		/// Return the TreeNode instance representing/store of 
		/// the Deleted Items.
		/// </summary>
		public ISmartFolder DeletedItemsNode {
			get { return _deletedItemsNode as ISmartFolder; }
		}

		/// <summary>
		/// Gets the TreeNode instance representing/store of
		/// a non persistent search result (temp. result)
		/// </summary>
		public ISmartFolder SearchResultNode {
			get { return _searchResultNode as ISmartFolder; }
		}

		public void SetFontAndColor(Font normalFont, Color normalColor, 
			Font unreadFont, Color unreadColor, Font highlightFont, Color highlightColor,
			Font errorFont, Color errorColor, Font refererFont, Color refererColor) {
			// really do the work
			FontColorHelper.DefaultFont = normalFont;
			FontColorHelper.NormalStyle = normalFont.Style;		FontColorHelper.NormalColor = normalColor;
			FontColorHelper.UnreadStyle = unreadFont.Style;		FontColorHelper.UnreadColor = unreadColor;
			FontColorHelper.HighlightStyle = highlightFont.Style;	FontColorHelper.HighlightColor = highlightColor;
			FontColorHelper.ReferenceStyle = refererFont.Style;	FontColorHelper.ReferenceColor = refererColor;
			FontColorHelper.FailureStyle = errorFont.Style;			FontColorHelper.FailureColor = errorColor;

			ResetTreeViewFontAndColor();
			ResetListViewFontAndColor();
		}

		// helper
		private void ResetTreeViewFontAndColor() {
			try {
				this.treeFeeds.BeginUpdate();
				if (Utils.VisualStylesEnabled) {
					this.treeFeeds.Font = FontColorHelper.NormalFont;
					this.treeRssSearchScope.Font = FontColorHelper.NormalFont;
				} else {
					this.treeFeeds.Font = FontColorHelper.FontWithMaxWidth();
					this.treeRssSearchScope.Font = FontColorHelper.FontWithMaxWidth();
				}
				
				this.treeFeeds.ForeColor = FontColorHelper.NormalColor;
				this.treeRssSearchScope.ForeColor = FontColorHelper.NormalColor;

				// now iterate and update the single nodes
				if (this.treeFeeds.Nodes.Count > 0) {
					for (int i = 0; i < _roots.Length; i++) {
						FeedTreeNodeBase startNode = _roots[i];
						if (null != startNode)
							WalkdownThenRefreshFontColor(startNode);
					}
				}

				PopulateTreeRssSearchScope();

			} finally {
				this.treeFeeds.EndUpdate();
			}
		}

		/// <summary>
		/// Helper. Work recursive on the startNode down to the leaves.
		/// Then reset all font/colors.
		/// </summary>
		/// <param name="startNode">Node to start with.</param>
		private void WalkdownThenRefreshFontColor(FeedTreeNodeBase startNode) {
			if (startNode == null) return;

			if (startNode.UnreadCount > 0) {
				startNode.NodeFont = FontColorHelper.UnreadFont;
				startNode.ForeColor = FontColorHelper.UnreadColor;
			} else if (startNode.HighlightCount > 0) {
				startNode.NodeFont = FontColorHelper.HighlightFont;
				startNode.ForeColor = FontColorHelper.HighlightColor;
			}else {
				startNode.NodeFont = FontColorHelper.NormalFont;
				startNode.ForeColor = FontColorHelper.NormalColor;
			}
			
			for (FeedTreeNodeBase child = startNode.FirstNode; child != null; child = child.NextNode) {
				WalkdownThenRefreshFontColor(child);
			}
		}
		
		private void ResetListViewFontAndColor() {
			this.listFeedItems.BeginUpdate();

			this.listFeedItems.Font = FontColorHelper.NormalFont;
			this.listFeedItems.ForeColor = FontColorHelper.NormalColor;

			// now iterate and update the single items
			for (int i = 0; i < this.listFeedItems.Items.Count; i++) {
				ThreadedListViewItem lvi = this.listFeedItems.Items[i];
				// apply leading fonts/colors
				ApplyStyles(lvi);
			}
			this.listFeedItems.EndUpdate();
		}


		public void CmdExecuteSearchEngine(ICommand sender) {
			if (typeof(AppToolCommand).IsInstanceOfType(sender)) {
				// the search dropdown itself
				AppToolCommand cmd = (AppToolCommand)sender;
				SearchEngine se = (SearchEngine)cmd.Tag;
				this.StartSearch(se);
			}	else if (typeof(AppToolMenuCommand).IsInstanceOfType(sender)) {	
				// the menu displayed on chevron-menu
				AppToolMenuCommand cmd = (AppToolMenuCommand)sender;
				SearchEngine se = (SearchEngine)cmd.Tag;
				this.StartSearch(se);
			}	else if (typeof(AppMenuCommand).IsInstanceOfType(sender)) {
				// the displayed context menu OnDropDown 
				AppMenuCommand cmd = (AppMenuCommand)sender;
				SearchEngine se = (SearchEngine)cmd.Tag;
				this.StartSearch(se);
			}
		}

		public void CmdSearchGo(ICommand sender) {
			this.StartSearch(null);
		}

		/// <summary>
		/// Initiate the search process
		/// </summary>
		/// <param name="thisEngine">A specific engine to use. If null, all
		/// active engines are started.</param>
		public void StartSearch(SearchEngine thisEngine) {
			string phrase = WebSearchText;

			if (phrase.Length > 0) {
				if (!searchComboBox.ComboBox.Items.Contains(phrase))
					searchComboBox.ComboBox.Items.Add(phrase);
				
				if (thisEngine != null) {
					string s = thisEngine.SearchLink;
					if (s != null && s.Length > 0) {
						try {
							s = String.Format(new UrlFormatter(), s, phrase);
						} catch (Exception fmtEx){
							_log.Error("Invalid search phrase placeholder, or no placeholder '{0}'", fmtEx);
							return;
						}
						if (thisEngine.ReturnRssResult) {
							owner.BackgroundDiscoverFeedsHandler.Add(new DiscoveredFeedsInfo(s, thisEngine.Title + ": '" + phrase + "'", s ));
							this.AsyncStartRssRemoteSearch(phrase, s, thisEngine.MergeRssResult, true);
						} else {
							this.DetailTabNavigateToUrl(s, thisEngine.Title, owner.SearchEngineHandler.NewTabRequired);
						}
					}
				}
				else {	// all
					bool isFirstItem = true;
					foreach (SearchEngine engine in owner.SearchEngineHandler.Engines) {
						if (engine.IsActive) {
							string s = engine.SearchLink;
							if (s != null && s.Length > 0) {
								try {
									s = String.Format(new UrlFormatter(), s, phrase);
								} catch (Exception fmtEx){
									_log.Error("Invalid search phrase placeholder, or no placeholder '{0}'", fmtEx);
									return;
								}
								if (engine.ReturnRssResult) {
									owner.BackgroundDiscoverFeedsHandler.Add(new DiscoveredFeedsInfo(s, engine.Title + ": '" + phrase + "'", s ));
									this.AsyncStartRssRemoteSearch(phrase, s, engine.MergeRssResult, isFirstItem);
									isFirstItem = false;
								} else {
									this.DetailTabNavigateToUrl(s, engine.Title, true);
									Application.DoEvents();
								}
							}
						}
					}
				} // end all active engines
			}
		}

		public void CmdOpenConfigIdentitiesDialog(ICommand sender) {
			IdentityNewsServerManager imng = new IdentityNewsServerManager(owner);
			imng.ShowIdentityDialog(this);
		}

		public void CmdOpenConfigNntpServerDialog(ICommand sender) {
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
		public FeedTreeNodeBase CurrentSelectedNode { 
			get { 
				if (_currentSelectedFeedNode != null)
					return _currentSelectedFeedNode;
				// this may also return null
				return TreeSelectedNode; 
			}
			set { _currentSelectedFeedNode = value; }
		}

		public FeedTreeNodeBase TreeSelectedNode { 
			get { return (FeedTreeNodeBase)treeFeeds.SelectedNode; }
			set { 
				listFeedItems.CheckForLayoutModifications();
				this.treeFeeds.BeforeSelect -= new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);									
				this.treeFeeds.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
				treeFeeds.SelectedNode = value; 
				this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);									
				this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
				listFeedItems.FeedColumnLayout = this.GetFeedColumnLayout(value);
			}
		}


		/// <summary>
		/// Returns the number of selected list view items
		/// </summary>
		public int NumSelectedListViewItems{
			
			get{ return listFeedItems.SelectedItems.Count; }
		}

		/// <summary>
		/// Returns the Subscription Tree Image List.
		/// </summary>
		public ImageList TreeImageList {
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
		public NewsItem CurrentSelectedFeedItem { 
			get { 
				if (_currentNewsItem != null)
					return _currentNewsItem;
				if (listFeedItems.SelectedItems.Count > 0)
					return (NewsItem)((ThreadedListViewItem)listFeedItems.SelectedItems[0]).Key; 
				return null;
			}
			set { _currentNewsItem = value; }
		}

		#endregion

		#region private helper routines

		internal void SetTitleText(string newTitle) {
			if (newTitle != null && newTitle.Trim().Length != 0)
				this.Text = RssBanditApplication.CaptionOnly + " - " + newTitle;
			else
				this.Text = RssBanditApplication.CaptionOnly;

			if (0 != (owner.InternetConnectionState & INetState.Offline))
				this.Text += " " + Resource.Manager["RES_MenuAppInternetConnectionModeOffline"];
		}

		protected void AddUrlToHistory(string newUrl) {
			if (!newUrl.Equals("about:blank")) {
				navigateComboBox.ComboBox.Items.Remove(newUrl);
				navigateComboBox.ComboBox.Items.Insert(0, newUrl);
			}
		}

		internal FeedTreeNodeBase GetRoot(RootFolderType rootFolder) {
			if (treeFeeds.Nodes.Count > 0) {
				return _roots[(int)rootFolder];
			}
			return null;
		}

		internal RootFolderType GetRoot(FeedTreeNodeBase node) {
			if (node == null)
				throw new ArgumentNullException("node");

			if (node.Type == FeedNodeType.Root || node.Parent == null) {
				for (int i=0; i < _roots.GetLength(0); i++) {
					if (node == _roots[i])
						return (RootFolderType)i;
				}
			} else if (node.Parent != null) {
				return this.GetRoot(node.Parent);
			}
			return RootFolderType.MyFeeds;
		}

		protected FeedTreeNodeBase CurrentDragNode { 
			get { return _currentDragNode;	}
			set { _currentDragNode = value; }
		}
		protected FeedTreeNodeBase CurrentDragHighlightNode { 
			get { return _currentDragHighlightNode;	}
			set {
				if (_currentDragHighlightNode != null && _currentDragHighlightNode != value) { 
					// unhighlight old one
					_currentDragHighlightNode.BackColor = treeFeeds.BackColor;
					_currentDragHighlightNode.ForeColor = treeFeeds.ForeColor;
					if (_timerTreeNodeExpand.Enabled) 
						_timerTreeNodeExpand.Stop();
				}
				_currentDragHighlightNode = value; 
				if (_currentDragHighlightNode != null) {
					// highlight new one
					_currentDragHighlightNode.BackColor = System.Drawing.SystemColors.Highlight;
					_currentDragHighlightNode.ForeColor = System.Drawing.SystemColors.HighlightText;
					if (_currentDragHighlightNode.Nodes.Count > 0 && !_currentDragHighlightNode.IsExpanded)
						_timerTreeNodeExpand.Start();
				}
			}
		}

		private void SetFocus2WebBrowser(HtmlControl theBrowser) {
			
			if (theBrowser == null) 
				return;
			theBrowser.Focus();
			
		}

		/// <summary>
		/// Populates the list view with the items for the feed represented by 
		/// the tree node then checks to see if any are unread. If this is the 
		/// case then the unread item is given focus.  
		/// </summary>
		/// <param name="tn"></param>
		/// <returns>True if an unread item exists for this feed and false otherwise</returns>
		private  bool FindNextUnreadItem(FeedTreeNodeBase tn) {
			feedsFeed f = null;
			bool repopulated = false,isTopLevel = true;
			ListViewItem foundLVItem = null; 

			//long measure = 0;	// used only for profiling...

			if (tn.Type == FeedNodeType.Feed && owner.FeedHandler.FeedsTable.ContainsKey((string)tn.Tag))
				f = owner.FeedHandler.FeedsTable[(string)tn.Tag]; 

			bool containsUnread = ((f != null && f.containsNewMessages) || (tn == TreeSelectedNode && tn.UnreadCount > 0));

			if(containsUnread) { 
				
				if (tn != TreeSelectedNode && f != null) {	
					
					containsUnread  = false;
					ArrayList items = owner.FeedHandler.GetCachedItemsForFeed(f.link);

					for (int i = 0; i < items.Count; i++) {
						NewsItem item = (NewsItem)items[i];
						if (!item.BeenRead) {
							containsUnread = true;
							break;
						}
					}

					if (containsUnread) {
						FeedTreeNodeBase tnSelected = TreeSelectedNode;
						if (tnSelected == null)
							tnSelected = GetRoot(RootFolderType.MyFeeds);

						if  (tnSelected.Type == FeedNodeType.SmartFolder || tnSelected.Type == FeedNodeType.Finder || tnSelected.Type == FeedNodeType.Root || 
							(tnSelected != tn && tnSelected.Type == FeedNodeType.Feed  ) || 
							(tnSelected.Type == FeedNodeType.Category && !NodeIsChildOf(tn, tnSelected) ) ) {

							//ProfilerHelper.StartMeasure(ref measure);

							//re-populate list view with items for feed with unread messages, if it is not
							// the current displayed:
//							this.treeFeeds.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
//							this.treeFeeds.BeforeSelect -= new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);									
							TreeSelectedNode = tn;
							CurrentSelectedNode = null;	// reset
//							this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);
//							this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																
							this.PopulateListView(tn, items, true); 
							repopulated = true;

							//_log.Info("Profile FindNextUnreadItem() Re-Populate listview took: "+ProfilerHelper.StopMeasureString(measure));
						}
					} else {
						f.containsNewMessages = false;  // correct the property value
					}

				}
						 
			}//if(f.containsNewMessages)

			for (int i = 0; i < listFeedItems.SelectedItems.Count; i++) {
				ThreadedListViewItem tlvi =(ThreadedListViewItem)listFeedItems.SelectedItems[i]; 
				if((tlvi != null) && (tlvi.IndentLevel != 0)){
					isTopLevel = false; 
					break; 
				}			
			}

			//select a list item that hasn't been read. As an optimization, we don't 
			//walk the list view if we are on a top level listview item and there are no
			//unread posts. 
			if((!isTopLevel) || containsUnread){
				 foundLVItem = FindUnreadListViewItem();
			}

			if (foundLVItem != null) {

				MoveFeedDetailsToFront();
					
				listFeedItems.BeginUpdate();	
				listFeedItems.SelectedItems.Clear(); 
				foundLVItem.Selected = true; 
				foundLVItem.Focused  = true;
				htmlDetail.Activate();	// set focus to html after doc is loaded
				this.OnFeedListItemActivate(null, EventArgs.Empty); //pass nulls because I don't use params
				SetTitleText(tn.Key);
				foundLVItem.Selected = true; 
				foundLVItem.Focused  = true; 	
				listFeedItems.Focus(); 
				listFeedItems.EnsureVisible(foundLVItem.Index); 
				listFeedItems.EndUpdate();		

				//select new position in tree view based on feed with unread messages.
				if (TreeSelectedNode != tn && repopulated) { 
					//we unregister event here to avoid OnTreeFeedAfterSelect() being invoked
//					this.treeFeeds.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
//					this.treeFeeds.BeforeSelect -= new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																							
//					treeFeeds.BeginUpdate();
					TreeSelectedNode = tn; 
					tn.EnsureVisible();
					if (tn.Parent != null) tn.Parent.EnsureVisible();
//					treeFeeds.EndUpdate();
//					this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);
//					this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																
							
				}
					
				return true; 

			}

			return false; 
		}

		private ThreadedListViewItem FindUnreadListViewItem() {
			
			if ( listFeedItems.Items.Count == 0)
				return null;
			
			NewsItem compareItem = null; 
			ThreadedListViewItem foundLVItem = null;

			int pos = 0, incrementor = 1;

			if (listFeedItems.SortManager.SortOrder == System.Windows.Forms.SortOrder.Descending){
				pos = listFeedItems.Items.Count - 1;	// at the end
				incrementor = -1;								// decrement
			}

			while (pos >= 0 && pos < listFeedItems.Items.Count) {	// in correct range

				ThreadedListViewItem lvi = listFeedItems.Items[pos];
				NewsItem item = lvi.Key as NewsItem; 
								
				// find the oldest unread item
				if(item != null && !item.BeenRead) {		// item can be null for temp entries like "Load comments,..."
					if (compareItem == null) compareItem = item;
					if (foundLVItem == null) foundLVItem = lvi;

					if (!(listFeedItems.SortManager.GetComparer() is System.Windows.Forms.ThListView.Sorting.ThreadedListViewDateTimeItemComparer)) {
						if (DateTime.Compare(item.Date, compareItem.Date) < 0) { // worst case: compare all unread
							// instance item.Date smaller than compareItem.Date. Found one:
							compareItem = item;	// item to compare to 
							foundLVItem = lvi;	// corresponding ListViewItem
						}
					} else {	// simply the next
						compareItem = item;	// item to compare to 
						foundLVItem = lvi;	// corresponding ListViewItem
						break;
					}
				}							
			
				pos += incrementor;	// decrement or increment
			}

			return foundLVItem;
		}

		/// <summary>
		/// From the startNode, this function returns the next
		/// FeedNode with UnreadCount > 0 that is hirarchically below the startNode.
		/// </summary>
		/// <param name="startNode">the Node to start with</param>
		/// <returns>FeedTreeNodeBase found or null</returns>
		private FeedTreeNodeBase NextNearFeedNode(FeedTreeNodeBase startNode, bool ignoreStartNode) {
			FeedTreeNodeBase found = null;
			
			if (!ignoreStartNode) {
				if (startNode.Type == FeedNodeType.Feed) return startNode;
			}

			// walk childs, go down
			for (FeedTreeNodeBase sibling = startNode.FirstNode; sibling != null && found == null; sibling = sibling.NextNode) {
				if (sibling.Type == FeedNodeType.Feed) return sibling;
				if (sibling.FirstNode != null)	// childs?
					found = NextNearFeedNode(sibling.FirstNode, false);
			}
			if (found != null) return found;

			// walk next siblings. If they have childs, go down
			for (FeedTreeNodeBase sibling = (ignoreStartNode ? startNode.NextNode: startNode.FirstNode); sibling != null && found == null; sibling = sibling.NextNode) {
				if (sibling.Type == FeedNodeType.Feed) return sibling;
				if (sibling.FirstNode != null)	// childs?
					found = NextNearFeedNode(sibling.FirstNode,false);
			}
			if (found != null) return found;
			if (startNode.Parent == null) return null;	// top of tree

			// no sibling, no Feed childs.
			// go upwards, as long as the parent itself is lastNode
			for (startNode = startNode.Parent; startNode != null && startNode.NextNode == null; startNode = startNode.Parent) {
				// nix to do here
			}
			if (startNode == null) return null;
			
			// no walk next parent siblings. 
			for (FeedTreeNodeBase parentSibling = startNode.NextNode; parentSibling != null && found == null; parentSibling = parentSibling.NextNode) {
				if (parentSibling.Type == FeedNodeType.Feed) return parentSibling;
				if (parentSibling.FirstNode != null)	// childs?
					found = NextNearFeedNode(parentSibling.FirstNode,false);
			}

			return found;
		}


		/// <summary>
		/// Moves from the currently selected item to the next unread item. 
		/// If no unread item is left then this method does nothing.
		/// </summary>
		public void MoveToNextUnreadItem() {
						
			FeedTreeNodeBase startNode = null, foundNode = null, rootNode = this.GetRoot(RootFolderType.MyFeeds);
			bool unreadFound = false;
			
			if (listFeedItems.Items.Count > 0) {
				startNode = TreeSelectedNode;
				if (startNode != null && startNode.UnreadCount > 0) {
					unreadFound = this.FindNextUnreadItem(startNode);
					if(!unreadFound) {
						startNode = null;
					} else {
						return;
					}
				}
			}

			if (startNode == null)
				startNode = CurrentSelectedNode; 

			if (startNode != null && !NodeIsChildOf(startNode, rootNode))
				startNode = null;

			if (startNode == null)
				startNode = rootNode;
			

			if (startNode.Type == FeedNodeType.Feed ) {
				MoveFeedDetailsToFront();

				if(this.FindNextUnreadItem(startNode)) {
					unreadFound = true; 
				}					
			}


			if (!unreadFound) {
				// look for next near down feed node
				foundNode = NextNearFeedNode(startNode,true); 
				while (foundNode != null && !unreadFound) {
					if(this.FindNextUnreadItem(foundNode)) {
						unreadFound = true;
					}					
					foundNode = NextNearFeedNode(foundNode,true);
				}
			}

			if (!unreadFound && startNode != this.GetRoot(RootFolderType.MyFeeds)) {
				// if not already applied,
				// look for next near down feed node from top of tree
				foundNode = NextNearFeedNode((FeedTreeNodeBase)treeFeeds.Nodes[0],true); 
				while (foundNode != null && !unreadFound) {
					if(this.FindNextUnreadItem(foundNode)) {
						unreadFound = true; 
					}					
					foundNode = NextNearFeedNode(foundNode,true);
				}
			}
			
			if (!unreadFound) {
				if (owner.StateHandler.NewsHandlerState == NewsHandlerState.Idle)
					this.SetGuiStateFeedback(Resource.Manager["RES_GUIStatusNoUnreadFeedItemsLeft"], ApplicationTrayState.NormalIdle);			
			}

		}

		/// <summary>
		/// Help to simply serialize a bounds rect.
		/// </summary>
		/// <param name="b"></param>
		/// <returns>A ';' separated string: "X;Y;Width;Height".</returns>
		private string BoundsToString(Rectangle b) {
			return b.X.ToString() + ";" + b.Y.ToString() + ";" +
				b.Width.ToString() + ";" + b.Height.ToString();
		}

		private Rectangle StringToBounds(string b) {
			string[] ba = b.Split(new char[]{';'});
			Rectangle r = Rectangle.Empty;
			if (ba.GetLength(0) == 4) {
				try {
					r = new Rectangle(Int32.Parse(ba[0]),Int32.Parse(ba[1]),Int32.Parse(ba[2]),Int32.Parse(ba[3]));
				} catch {}
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
		public void PopulateSmartFolder(FeedTreeNodeBase feedNode, bool updateGui) {
			
			ArrayList items = null;
			ISmartFolder isFolder = feedNode as ISmartFolder;
			
			if (isFolder == null)
				return;

			items = isFolder.Items;						

				//Ensure we update the UI in the correct thread. Since this method is likely 
				//to have been called from a thread that is not the UI thread we should ensure 
				//that calls to UI components are actually made from the UI thread or marshalled
				//accordingly. 			

			 if(updateGui || TreeSelectedNode == feedNode) {

				NewsItem itemSelected = null;
				if (listFeedItems.SelectedItems.Count > 0) 
					itemSelected = (NewsItem)((ThreadedListViewItem)listFeedItems.SelectedItems[0]).Key as NewsItem;

				// call them sync., because we want to re-set the previous selected item
				if(listFeedItems.InvokeRequired == true) {
					PopulateListViewDelegate populateListView  = new PopulateListViewDelegate(PopulateListView);			
					this.Invoke(populateListView, new object[]{feedNode, items, true, false} );			
				}
				else {
					this.PopulateListView(feedNode, items, true, false, feedNode); 
				}

				if (updateGui) {
					
					htmlDetail.Clear();	//clear browser pane 
					if (itemSelected == null || this.listFeedItems.Items.Count == 0) {
						CurrentSelectedFeedItem = null;
					} else 
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
		public void PopulateFinderNode(FinderNode node, bool updateGui) {
			
			if (node == null)
				return;

			//Ensure we update the UI in the correct thread. Since this method is likely 
			//to have been called from a thread that is not the UI thread we should ensure 
			//that calls to UI components are actually made from the UI thread or marshalled
			//accordingly. 
			if(updateGui || TreeSelectedNode == node) {

				NewsItem itemSelected = null;
				if (listFeedItems.SelectedItems.Count > 0) 
					itemSelected = (NewsItem)((ThreadedListViewItem)listFeedItems.SelectedItems[0]).Key as NewsItem;

				// now the FinderNode handle refresh of the read state only, so we need to initiate a new search again...:
				node.AnyUnread = false;
				node.Clear(); 
				AsyncStartNewsSearch(node);	
				ArrayList items = node.Items;
			
				// call them sync., because we want to re-set the previous selected item
				if(listFeedItems.InvokeRequired == true) {
					
					PopulateListViewDelegate populateListView  = new PopulateListViewDelegate(PopulateListView);			
					this.Invoke(populateListView, new object[]{node, items, true, false} );			

				} else {
					this.PopulateListView(node, items, true, true, node); 
				}

				if (updateGui) {
					if (itemSelected != null) {
						//clear browser pane 
						CurrentSelectedFeedItem = null;
						htmlDetail.Clear();
					} else 
						ReSelectListViewItem(itemSelected);
				}
			}

		}

//		private void ApplyExclusionFromTables(NewsItem item, Hashtable a1, Hashtable a2) {
//			string foundId = FindNewsItemInTable(a1, item);
//			if (foundId != null)
//				a1.Remove(foundId);
//
//			foundId = FindNewsItemInTable(a2, item);
//			if (foundId != null)
//				a2.Remove(foundId);
//		}

//		private string FindNewsItemInTable(Hashtable list, NewsItem item) {
//			if (item.Id == null) return null;
//			if (list != null && list.ContainsKey(item.Id))	
//				return item.Id;
//			return null;
//		}

//		private int FindNewsItemInList(object[] list, NewsItem item) {
//			if (item == null || item.Id == null) return -1;
//
//			int listLen = list.GetLength(0);
//			for (int i = 0; i < listLen; i++) {
//				NewsItem ri = (NewsItem)list[i];
//				if (item.Equals(ri))
//					return i;	// found
//			}
//			return -1;	// not found
//		}

		private ThreadedListViewItem CreateThreadedLVItem(NewsItem newsItem, bool hasChilds, int imgOffset, ColumnKeyIndexMap colIndex, bool authorInTopicColumn) {	

			string[] lvItems = new string[colIndex.Count];

			foreach (string colKey in colIndex.Keys) {
				lvItems[colIndex[colKey]] = String.Empty;	// init
				switch ((NewsItemSortField)Enum.Parse(typeof(NewsItemSortField), colKey, true)) {
					case NewsItemSortField.Title:
						lvItems[colIndex[colKey]] = StringHelper.ShortenByEllipsis(newsItem.Title, MaxHeadlineWidth);
						break;
					case NewsItemSortField.Subject:
						if (authorInTopicColumn && !colIndex.ContainsKey("Author")) {
							lvItems[colIndex[colKey]] = newsItem.Author;
						} else {
							lvItems[colIndex[colKey]] = newsItem.Subject;
						}
						break;
					case NewsItemSortField.FeedTitle:
						lvItems[colIndex[colKey]] = HtmlHelper.HtmlDecode(newsItem.Feed.title);
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
					case NewsItemSortField.Enclosure:	//TODO: make it more efficient
						if (null != RssHelper.GetOptionalElement(newsItem, "enclosure", String.Empty))
							lvItems[colIndex[colKey]] = "1";	// state should be ("None", "Available", "Scheduled", "Downloaded")
						break;
					case NewsItemSortField.Flag:
						if (newsItem.FlagStatus != Flagged.None)
							lvItems[colIndex[colKey]] = newsItem.FlagStatus.ToString();	//TODO: localize
						break;
					default:
						Trace.Assert(false, "CreateThreadedLVItem::NewsItemSortField NOT handled: " + colKey);
						break;
				}
			}

			ThreadedListViewItem lvi = new ThreadedListViewItem(newsItem, lvItems);
			ApplyFlagIconTo(lvi, newsItem);

			if (lvi.ImageIndex < 10 && imgOffset > 0)	{ // not flagged, and not default image
				if(!newsItem.BeenRead) 
					imgOffset++;
				lvi.ImageIndex = imgOffset;
			}

			// apply leading fonts/colors
			ApplyStyles(lvi, newsItem.BeenRead);
			
			lvi.HasChilds = hasChilds; 

			return lvi;
		}

		private void ApplyFlagIconTo(ListViewItem lvi, NewsItem newsItem) {
			
			int imgOffset = 0;
			
			if(!newsItem.BeenRead) 
				imgOffset++;

			switch (newsItem.FlagStatus) {
				case Flagged.Complete:
					imgOffset += 10;	// Ok marker
					break;
				case Flagged.FollowUp:
					imgOffset += 12;	// red flag
					break;
				case Flagged.Forward:
					imgOffset += 14;	// blue flag
					break;
				case Flagged.Read:
					imgOffset += 16;	// green flag
					break;
				case Flagged.Review:
					imgOffset += 18;	// yellow flag
					break;
				case Flagged.Reply:
					imgOffset += 20;	// reply marker
					break;
				case Flagged.None:
					//imgOffset is already setup
					break;
			}

			lvi.ImageIndex = imgOffset;
		}

		private ThreadedListViewItem CreateThreadedLVItemInfo(string infoMessage, bool isError) {	

			ThreadedListViewItem lvi = new ThreadedListViewItemPlaceHolder(infoMessage);
			if (isError) {
				lvi.Font = FontColorHelper.FailureFont;
				lvi.ForeColor = FontColorHelper.FailureColor;
				lvi.ImageIndex = 22;
			} else {
				lvi.Font = FontColorHelper.NormalFont;
				lvi.ForeColor = FontColorHelper.NormalColor;
			}
			lvi.HasChilds = false; 

			return lvi;
		}

		/// <summary>
		/// Populates the list view with NewsItem's from the ArrayList. 
		/// </summary>
		/// <param name="associatedNode">The accociated tree Node</param>
		/// <param name="list">A list of NewsItem objects.</param>
		/// <param name="forceReload">Force reload of the listview</param>
		private void PopulateListView(FeedTreeNodeBase associatedNode, ArrayList list, bool forceReload){
			this.PopulateListView(associatedNode, list, forceReload, false, associatedNode);
		}
		
		/// <summary>
		/// Populates the list view with NewsItem's from the ArrayList. 
		/// </summary>
		/// <param name="associatedNode">The accociated tree Node to populate</param>
		/// <param name="list">A list of NewsItem objects.</param>
		/// <param name="forceReload">Force reload of the listview</param>
		/// <param name="categorizedView">True, if the feed title should be appended to
		/// each RSS Item title: "...rss item title... (feed title)"</param>
		private void PopulateListView(FeedTreeNodeBase associatedNode, ArrayList list, bool forceReload, bool categorizedView, FeedTreeNodeBase initialNode) {
			
			try {

				lock(listFeedItems.Items){

					if(TreeSelectedNode != initialNode){
						return;
					}

				}

				int unreadItems = 0;

				// detect, if we should do a smartUpdate
						
				lock(listFeedItems.Items){

					//since this is a multithreaded app there could have been a change since the last 
					//time we checked this at the beginning of the method due to context switching. 
					if(TreeSelectedNode != initialNode){	
						return;
					}

					if (initialNode.Type == FeedNodeType.Category) {				

						if(NodeIsChildOf(associatedNode, initialNode )){
							if (forceReload){
								this.EmptyListView(); 
								feedsCurrentlyPopulated.Clear();
							}
							
							bool checkForDuplicates = this.feedsCurrentlyPopulated.Contains(associatedNode.Tag);
							unreadItems = this.PopulateSmartListView(list, categorizedView, checkForDuplicates);
							if (!checkForDuplicates)
								feedsCurrentlyPopulated.Add(associatedNode.Tag, null);

							if (unreadItems != associatedNode.UnreadCount)
								associatedNode.UpdateReadStatus(associatedNode, unreadItems);

						} else if (associatedNode == initialNode){

							feedsCurrentlyPopulated.Clear();
							this.PopulateFullListView( list, categorizedView);
							if (associatedNode.Tag != null)
								feedsCurrentlyPopulated.Add(associatedNode.Tag, null);
						}
				
					} else if (TreeSelectedNode == associatedNode) {
						if (forceReload) {
							unreadItems = this.PopulateFullListView(list, categorizedView);
							if (unreadItems != associatedNode.UnreadCount)
								associatedNode.UpdateReadStatus(associatedNode, unreadItems);
						} else {
							unreadItems = this.PopulateSmartListView(list, categorizedView, true);
							if (unreadItems > 0) {
								if (categorizedView)	// e.g. AggregatedNodes
									unreadItems += associatedNode.UnreadCount;
								associatedNode.UpdateReadStatus(associatedNode, unreadItems);
							}
						}
					}

				}//lock

				this.SetGuiStateFeedback(Resource.Manager["RES_StatisticsItemsDisplayedMessage", this.listFeedItems.Items.Count]);
			
			} catch (Exception ex) {
				_log.Error("PopulateListView() failed.", ex);
			}
		}

		/// <summary>
		/// Can be called from another thread to populate the listview in the Gui thread.
		/// </summary>
		/// <param name="associatedNode"></param>
		/// <param name="list"></param>
		/// <param name="forceReload"></param>
		/// <param name="categorizedView"></param>
		/// <param name="initialNode"></param>
		public void AsyncPopulateListView(FeedTreeNodeBase associatedNode, ArrayList list, bool forceReload, bool categorizedView, FeedTreeNodeBase initialNode) {
			if (this.listFeedItems.InvokeRequired) {
				PopulateListViewDelegate populateListView  = new PopulateListViewDelegate(PopulateListView);			
				this.Invoke(populateListView, new object[]{associatedNode, list, forceReload, categorizedView});
			} else {
				this.PopulateListView(associatedNode, list, forceReload, categorizedView, initialNode);			
			}
		}

		/// <summary>
		/// Fully populates the list view with NewsItem's from the ArrayList 
		/// (forced reload).
		/// </summary>
		/// <param name="list">A list of NewsItem objects.</param>
		/// <param name="categorizedView">True, if the feed title should be appended to
		/// each RSS Item title: "...rss item title... (feed title)"</param>
		/// <returns>unread items count</returns>
		private int PopulateFullListView(ArrayList list, bool categorizedView) {

			ThreadedListViewItem[] aNew = new ThreadedListViewItem[list.Count];
			ThreadedListViewItem newItem = null;

			int unreadCounter = 0;
			ColumnKeyIndexMap colIndex = this.listFeedItems.Columns.GetColumnIndexMap();
			INewsItemFilter flagFilter = null;

			if (CurrentSelectedNode is FlaggedItemsNode) {	
				// do not apply flag filter on Flagged items node(s)
				flagFilter = _filterManager["NewsItemFlagFilter"];
				_filterManager.Remove("NewsItemFlagFilter");
			}

			this.EmptyListView();

			listFeedItems.BeginUpdate(); 

			try {
				for(int i = 0; i < list.Count; i++) {
				
					NewsItem item = (NewsItem) list[i]; 
					
					if (!item.BeenRead)
						unreadCounter++;

					bool hasRelations = false;
					hasRelations = NewsItemHasRelations(item);	// here is the bottleneck :-(

					newItem = CreateThreadedLVItem(item, hasRelations, 0, colIndex, false);
					_filterManager.Apply(newItem); 

					aNew[i] = newItem;

				}
				
				Array.Sort(aNew, listFeedItems.SortManager.GetComparer());
				listFeedItems.Items.AddRange(aNew); 
				
				listFeedItems.EndUpdate(); 
				return unreadCounter;

			} catch (Exception ex) {

				_log.Error("PopulateFullListView exception", ex);
				return unreadCounter;

			} finally {
				listFeedItems.EndUpdate(); 

				if (flagFilter != null) {	// add back
					flagFilter = _filterManager.Add("NewsItemFlagFilter", flagFilter);
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
		/// <returns>unread items counter</returns>
		public int PopulateSmartListView(ArrayList list, bool categorizedView, bool checkDuplicates) {

			ArrayList items = new ArrayList(listFeedItems.Items.Count);
			ArrayList newItems = new ArrayList(list.Count);
			
			lock(listFeedItems.Items) {
				items.AddRange(listFeedItems.Items);
			}

			ThreadedListViewItem newItem = null;

			int unreadCounter = 0;

			// column index map
			ColumnKeyIndexMap colIndexes = this.listFeedItems.Columns.GetColumnIndexMap();

			try {

				for(int i = 0; i < list.Count; i++) {
				
					NewsItem item = (NewsItem) list[i]; 
					bool hasRelations = NewsItemHasRelations(item);
					bool isDuplicate = false;
					ThreadedListViewItem tlvi = null;
						
					if (checkDuplicates) {
						//lock(listFeedItems.Items) {
							// look, if it is already there
							for (int j = 0; j < items.Count; j++) {
								tlvi = (ThreadedListViewItem)items[j];
								if (item.Equals((NewsItem)tlvi.Key) && tlvi.IndentLevel == 0) {
									tlvi.Key = item;			// update ref
									isDuplicate = true; 
									break;
								}
							}
						//}
					}

					if (isDuplicate) { 
						// do not create a new one, but check if it has new childs
						if (tlvi != null && !tlvi.HasChilds && hasRelations) 
							tlvi.HasChilds = hasRelations;
						
					} else {
						newItem = CreateThreadedLVItem(item, hasRelations, 0, colIndexes , false);

						_filterManager.Apply(newItem); 
						newItems.Add(newItem);

					}


					if (!item.BeenRead)
						unreadCounter++;

				}//for(int i)

				if (newItems.Count > 0) {
					try {
						listFeedItems.BeginUpdate(); 

						lock(listFeedItems.Items) {

							ThreadedListViewItem[] a = new ThreadedListViewItem[newItems.Count];
							newItems.CopyTo(a);
							listFeedItems.ListViewItemSorter = listFeedItems.SortManager.GetComparer();
							listFeedItems.Items.AddRange(a); 
							listFeedItems.ListViewItemSorter = null;

							if (listFeedItems.SelectedItems.Count > 0)
								listFeedItems.EnsureVisible(listFeedItems.SelectedItems[0].Index);
						}

					} finally {
						listFeedItems.EndUpdate(); 
					}
				}

			} catch (Exception ex) {
				_log.Error("PopulateSmartListView exception", ex);
			}

			return unreadCounter;

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

		private bool NewsItemHasRelations(NewsItem item) {
			return this.NewsItemHasRelations(item, new object[]{});
		}
		
		private bool NewsItemHasRelations(NewsItem item, IList itemKeyPath) {
			bool hasRelations = false;
			if (item.Feed != null & owner.FeedHandler.FeedsTable.ContainsKey(item.Feed.link)) {
				hasRelations = owner.FeedHandler.HasItemAnyRelations(item, itemKeyPath);
			}
			if (!hasRelations) hasRelations = (item.HasExternalRelations && owner.InternetAccessAllowed);
			return hasRelations;		
		}

		public void BeginLoadCommentFeed(NewsItem item, string ticket, IList itemKeyPath) {
			ShowProgressHandler handler = new ShowProgressHandler(this.OnLoadCommentFeedProgress);
			ThreadWorker.StartTask(ThreadWorker.Task.LoadCommentFeed, this, handler, owner, item, ticket, itemKeyPath);
		}
		private void OnLoadCommentFeedProgress(object sender, ShowProgressArgs args) {
			if (args.Exception != null) {
				// failure(s)
				args.Cancel = true;
				AppExceptions.ExceptionManager.Publish(args.Exception);
				object[] results = (object[])args.Result;
				string insertionPointTicket = (string)results[2];
				ThreadedListViewItem[] newChildItems = new ThreadedListViewItem[]{this.CreateThreadedLVItemInfo(args.Exception.Message, true)};
				listFeedItems.InsertItemsForPlaceHolder(insertionPointTicket, newChildItems, false);
			} else if (!args.Done) {
				// in progress
				// we already have a "loading ..." text listview item
			} else if (args.Done) {
				// done
				object[] results = (object[])args.Result;
				ArrayList commentItems = (ArrayList)results[0];
				NewsItem item = (NewsItem)results[1];
				string insertionPointTicket = (string)results[2];
				IList itemKeyPath = (IList)results[3];

				if (item.CommentCount != commentItems.Count) {
					item.CommentCount =  commentItems.Count;
					owner.FeedWasModified(item.Feed.link);
				}
					
				commentItems.Sort(RssHelper.GetComparer(false, NewsItemSortField.Date));
				item.SetExternalRelations(new NewsComponents.Collections.RelationList(commentItems));

				ThreadedListViewItem[] newChildItems = null;
				
				if (commentItems.Count > 0) {
					newChildItems = new ThreadedListViewItem[commentItems.Count];
					
					// column index map
					ColumnKeyIndexMap colIndex = this.listFeedItems.Columns.GetColumnIndexMap();

					for (int i=0; i < commentItems.Count; i++) {
									
						NewsItem o = (NewsItem)commentItems[i];
									
						bool hasRelations = this.NewsItemHasRelations(o, itemKeyPath);

						o.BeenRead = tempFeedItemsRead.ContainsKey(RssHelper.GetHashCode(o));
						ThreadedListViewItem newListItem = this.CreateThreadedLVItem(o, hasRelations, 8, colIndex, true);
						_filterManager.Apply(newListItem);
						newChildItems[i] = newListItem;

					}//iterator.MoveNext
				}
				
				listFeedItems.InsertItemsForPlaceHolder(insertionPointTicket, newChildItems, false);
			}
		}

		/// <summary>
		/// Called to refresh the GUI state after refresh of feeds/feed items
		/// </summary>
		public void TriggerGUIStateOnNewFeeds(bool handleNewReceived) {
			int unreadFeeds, unreadMessages; 
			this.CountUnread(out unreadFeeds, out unreadMessages); 

			if(unreadMessages != 0) {
				this._timerResetStatus.Stop();
				if (handleNewReceived && unreadMessages > _lastUnreadFeedItemCountBeforeRefresh) {
					string message = Resource.Manager.FormatMessage("RES_GUIStatusNewFeedItemsReceivedMessage", unreadFeeds, unreadMessages); 
					if (this.Visible) {
						this.SetGuiStateFeedback(message, ApplicationTrayState.NewUnreadFeeds);
					} else {	
						// if invisible (tray only): animate
						this.SetGuiStateFeedback(message, ApplicationTrayState.NewUnreadFeedsReceived);
					}
					if (owner.Preferences.ShowNewItemsReceivedBalloon && 
						( !this.Visible || this.WindowState == FormWindowState.Minimized) ) {
						if (_beSilentOnBalloonPopupCounter <= 0) {
							message = Resource.Manager.FormatMessage("RES_GUIStatusNewFeedItemsReceivedMessage", 
								unreadFeeds, 
								unreadMessages); 
							_trayAni.ShowBalloon(NotifyIconAnimation.EBalloonIcon.Info, message, RssBanditApplication.CaptionOnly + " - New feeds arrived");
						} else {
							_beSilentOnBalloonPopupCounter--;
						}
					}
				} 
				else {
					this.SetGuiStateFeedback(String.Empty, ApplicationTrayState.NewUnreadFeeds);
				}
			}
			else {
				this.SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
			}
		}

		private int CountUnreadFeedItems(ArrayList items) {
			int unreadMessages = 0;
			
			if (items == null) return unreadMessages;
			if (items.Count == 0) return unreadMessages;

			for (int i = 0; i < items.Count; i++) {
				NewsItem item = (NewsItem)items[i];
				if(!item.BeenRead) unreadMessages++; 
			}

			return unreadMessages;
		}

		private int CountUnreadFeedItems(feedsFeed f) {
			int unreadMessages = 0;
			
			if (f == null) return unreadMessages;

			if(f.containsNewMessages) {
				ArrayList items = null;
				try {
					items = owner.FeedHandler.GetCachedItemsForFeed(f.link);
				} catch { /* ignore cache errors here. On error, it returns always zero */}
				if (items == null) return unreadMessages;
				
				for (int i = 0; i < items.Count; i++) {
					NewsItem item = (NewsItem)items[i];
					if(!item.BeenRead) unreadMessages++; 
				}
			}
			return unreadMessages;
		}


		/// <summary>
		/// Obtains the number of unread RSS feeds and total unread RSS items
		/// </summary>
		/// <param name="unreadFeeds">Total RSS feeds with at least one unread item</param>
		/// <param name="unreadMessages">Total unread items</param>
		private void CountUnread(out int unreadFeeds, out int unreadMessages) {
			
			/* this code is inefficient because we loop through feeds and items even though 
			 * we probably just did that in a RefreshFeeds(). At least this should happen only
			 * when the app is minimized so this doesn't delay response time to user input. 
			 */

			unreadFeeds = unreadMessages = 0; 

			foreach(feedsFeed f in owner.FeedHandler.FeedsTable.Values) {
			
				if(f.containsNewMessages) {
					unreadFeeds++; 
					int urm = CountUnreadFeedItems(f);
					unreadMessages += urm;
				}
			}
		}

		private void CheckForAddIns() {
			Syndication.Extensibility.IBlogExtension ibe = null;

			try{
				addIns = AppInteropServices.ServiceManager.SearchForIBlogExtensions(RssBanditApplication.GetPlugInPath());
				if (addIns == null || addIns.Count == 0)
					return;
			
				// separator
				_listContextMenu.MenuItems.Add(new MenuItem("-"));

				for (int i = 0; i < addIns.Count; i++){
					ibe = (Syndication.Extensibility.IBlogExtension)addIns[i];
					AppContextMenuCommand m = new AppContextMenuCommand("cmdIBlogExt."+i.ToString(), 
						owner.Mediator, new ExecuteCommandHandler(owner.CmdGenericListviewCommand),
						ibe.DisplayName, "RES_MenuIBlogExtensionCommandDesc");
					_listContextMenu.MenuItems.Add(m);
					if (ibe.HasConfiguration) {
						AppContextMenuCommand mc = new AppContextMenuCommand("cmdIBlogExtConfig."+i.ToString(), 
							owner.Mediator, new ExecuteCommandHandler(owner.CmdGenericListviewCommandConfig),
							ibe.DisplayName+" - "+Resource.Manager["RES_MenuConfigCommandCaption"], "RES_MenuIBlogExtensionConfigCommandDesc");
						_listContextMenu.MenuItems.Add(mc);
					}
				}
			} catch (Exception ex) {
				_log.Fatal("Failed to load IBlogExtension plugin: " + (ibe == null ? String.Empty: ibe.GetType().FullName), ex);
				AppExceptions.ExceptionManager.Publish(ex);
			}
		}

		private void OnFeedTransformed(object sender, ShowProgressArgs args) {
			if (args.Exception != null) { // failure(s)
				args.Cancel = true;
				RssBanditApplication.PublishException(args.Exception);
			} else if (!args.Done) {
				// in progress
			} else if (args.Done) {	// done
				object[] results = (object[])args.Result;
				TreeNode node = (TreeNode)results[0];
				string html = (string)results[1];
				if((this.listFeedItems.SelectedItems.Count == 0) && Object.ReferenceEquals(this.treeFeeds.SelectedNode, node)){
					htmlDetail.Html = html;
					htmlDetail.Navigate(null);			
				}	
			}
		}

		private void BeginTransformFeed(FeedInfo feed, TreeNode feedNode, string stylesheet) {	
				
				//TODO: Ensure that there is no chance the code below can throw ArgumentOutOfRangeException 
				ThreadedListViewColumnHeader colHeader = this.listFeedItems.Columns[this.listFeedItems.SortManager.SortColumnIndex];
				IComparer newsItemSorter = RssHelper.GetComparer(this.listFeedItems.SortManager.SortOrder == System.Windows.Forms.SortOrder.Descending, 
					(NewsItemSortField)Enum.Parse(typeof(NewsItemSortField), colHeader.Key)); 				

				/* perform XSLT transformation in a background thread */
				ShowProgressHandler handler = new ShowProgressHandler(this.OnFeedTransformed);
				//TransformedFeedsHandler handler = new TransformedFeedsHandler(this.OnFeedTransformed);
				ThreadWorker.StartTask(ThreadWorker.Task.TransformFeed, this, handler, ThreadWorker.DuplicateTaskQueued.Abort, 
					this.owner, feed, feedNode, stylesheet, newsItemSorter );			
		}

		
		private void BeginTransformFeedList(FeedInfoList feeds, TreeNode feedNode, string stylesheet) {					
			
			//TODO: Ensure that there is no chance the code below can throw ArgumentOutOfRangeException 
			ThreadedListViewColumnHeader colHeader = this.listFeedItems.Columns[this.listFeedItems.SortManager.SortColumnIndex];
			IComparer newsItemSorter = RssHelper.GetComparer(this.listFeedItems.SortManager.SortOrder == System.Windows.Forms.SortOrder.Descending, 
				(NewsItemSortField)Enum.Parse(typeof(NewsItemSortField), colHeader.Key)); 				

			/* perform XSLT transformation in a background thread */
			ShowProgressHandler handler = new ShowProgressHandler(this.OnFeedTransformed);
			//TransformedFeedsHandler handler = new TransformedFeedsHandler(this.OnFeedTransformed);
			ThreadWorker.StartTask(ThreadWorker.Task.TransformCategory, this, handler, ThreadWorker.DuplicateTaskQueued.Abort, 
				this.owner, feeds, feedNode, stylesheet, newsItemSorter );			
		}	

		/// <summary>
		/// Reloads the list view if the tree node is selected and renders the newspaper view
		/// </summary>
		/// <param name="tn">the tree node</param>
		internal void RefreshFeedDisplay(FeedTreeNodeBase tn) {	
			this.RefreshFeedDisplay(tn, true); 
		}
		


		/// <summary>
		/// Reloads the list view if the tree node is selected and renders the newspaper view
		/// </summary>
		/// <param name="tn">the tree node</param>
		/// <param name="populateListview">indicates whether the list view should be repopulated or not</param>
		internal void RefreshFeedDisplay(FeedTreeNodeBase tn, bool populateListview) {	
			if (tn == null) tn = CurrentSelectedNode;
			if (tn == null) return;
			if(!tn.IsSelected || tn.Type != FeedNodeType.Feed) return;

			string feedUrl = (string)tn.Tag;

			if (feedUrl != null && owner.FeedHandler.FeedsTable.Contains(feedUrl)) {

				feedsFeed f = owner.FeedHandler.FeedsTable[feedUrl];
				owner.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOne);
				try { 				
					this.htmlDetail.Clear();
					ArrayList items = owner.FeedHandler.GetItemsForFeed(feedUrl, false);
					if ((DisplayFeedAlertWindow.All == owner.Preferences.ShowAlertWindow || 
					   (DisplayFeedAlertWindow.AsConfiguredPerFeed == owner.Preferences.ShowAlertWindow && f.alertEnabled)) &&
						tn.UnreadCount < this.CountUnreadFeedItems(items)) { //test flag on feed, if toast enabled
						_toastNotifier.Alert(tn.Text, tn.UnreadCount, items);
					}
					
					//we don't need to populate the listview if this called from 
					//RssBanditApplication.ApplyPreferences() since it is already populated
					if(populateListview){
						this.PopulateListView(tn, items, true, false, tn);					
					}
					IFeedDetails fi = owner.FeedHandler.GetFeedInfo(feedUrl);
					
					if (fi != null){
						UrlText = fi.Link;

						//we use a clone of the FeedInfo because it isn't 
						//necessarily true that everything in the main FeedInfo is being rendered
						FeedInfo fi2 = (FeedInfo) fi.Clone(); 
						fi2.ItemsList.Clear(); 
						foreach(NewsItem i in items){
							if(!i.BeenRead)
								fi2.ItemsList.Add(i);
						}

						//check to see if we still have focus 
						if(tn.IsSelected){					
							this.BeginTransformFeed(fi2, tn, this.owner.FeedHandler.GetStyleSheet((string)tn.Tag)); 
						}
					}

				} catch(Exception e) {
					this.EmptyListView(); 
					owner.PublishXmlFeedError(e, feedUrl, true);
				}
				owner.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
			}
		}

		/* LOCKS UP UI
		protected void RefreshCategoryDisplay(FeedTreeNodeBase tn) {	
			if (tn == null) tn = CurrentSelectedNode;
			if (tn == null) return;
			if(!tn.IsSelected || tn.Type == FeedNodeType.Root || tn.Type == FeedNodeType.Feed) return;

			this.EmptyListView(); 
			this.listFeedItems.Refresh();
			MoveFeedDetailsToFront();
			
			Cursor.Current = Cursors.WaitCursor;

			ArrayList allitems = new ArrayList(150);

			foreach (FeedTreeNodeBase tChild in tn.Nodes) {
			
				if (tChild.Type != FeedNodeType.Feed)
					continue;

				string feedUrl = (string)tChild.Tag;

				if (owner.FeedHandler.FeedsTable.Contains(feedUrl)) {

					feedsFeed f = (feedsFeed)owner.FeedHandler.FeedsTable[feedUrl];
					
					if(f.refreshrateSpecified && (f.refreshrate == 0)){
						continue; 		// disabled feed  
					}

					try { 				
						ArrayList items = owner.FeedHandler.GetItemsForFeed(feedUrl, false);
						if (f.containsNewMessages) {
							int cnt = this.CountUnreadFeedItems(items);
							tChild.UpdateReadStatus(tChild, cnt);
							if ((DisplayFeedAlertWindow.All == owner.Preferences.ShowAlertWindow || 
								(DisplayFeedAlertWindow.AsConfiguredPerFeed == owner.Preferences.ShowAlertWindow && f.alertEnabled))) { //test flag on feed, if toast enabled
								_toastNotifier.Alert(tn.Text, cnt, items);
							}
						}
						allitems.AddRange(items);
					} catch(Exception e) {
						owner.PublishXmlFeedError(e, feedUrl);
					}
				}
			}//foreach
			
			this.PopulateListView(tn, allitems, true, true, tn);					
			Cursor.Current = Cursors.Default;

		}
		*/

		protected void DeleteCategory(FeedTreeNodeBase categoryNode) {
			if (categoryNode == null) categoryNode = CurrentSelectedNode;
			if (categoryNode == null) return;
			if(categoryNode.Type != FeedNodeType.Category) return;
			
			FeedTreeNodeBase cnf = null;

			// if there are feed items displayed, we may have to delete the content
			// if rss items are of a feed with the category to delete
			if (listFeedItems.Items.Count > 0)
				cnf = GetTreeNodeForItem(categoryNode, (NewsItem)(listFeedItems.Items[0]).Key);
			if (cnf != null) {
				this.EmptyListView();
				this.htmlDetail.Clear();
			}

			WalkdownThenDeleteFeedsOrCategories(categoryNode);
			categoryNode.UpdateReadStatus(categoryNode, 0);
			
			TreeSelectedNode = this.GetRoot(RootFolderType.MyFeeds); 

			try {
				categoryNode.Parent.Nodes.Remove(categoryNode);
			}	finally { 
				this.DelayTask(DelayedTasks.SyncRssSearchTree);
			}

		}

		/// <summary>
		/// Helper that builds the full path trimmed category name (without root caption)
		/// </summary>
		/// <param name="theNode">the FeedTreeNodeBase</param>
		/// <returns>Category name in this form: 'Main Category\Sub Category\...\catNode Category'.</returns>
		internal string BuildCategoryStoreName(FeedTreeNodeBase theNode) {
			string s = theNode.FullPath.Trim();
			string[] a = s.Split(this.treeFeeds.PathSeparator.ToCharArray());

			if (theNode.Type == FeedNodeType.Feed || theNode.Type == FeedNodeType.Finder) {
				if (a.GetLength(0) > 2)
					return String.Join(@"\",a, 1, a.GetLength(0)-2);
			}
			else {
				if (a.GetLength(0) > 1)
					return String.Join(@"\",a, 1,a.GetLength(0)-1);
			}
			
			return null;	// default category (none)
		}

	
		/// <summary>
		/// Helper. Work recursive on the startNode down to the leaves.
		/// Then call AsyncGetItemsForFeed() for each of them.
		/// </summary>
		/// <param name="startNode">Node to start with</param>
		/// <param name="forceRefresh">true, if refresh should be forced</param>
		private void WalkdownThenRefreshFeed(FeedTreeNodeBase startNode, bool forceRefresh) {
			this.WalkdownThenRefreshFeed(startNode, forceRefresh, false, this.CurrentSelectedNode, new FeedInfoList(String.Empty)); 
		}
		


		/// <summary>
		/// Helper. Work recursive on the startNode down to the leaves.
		/// Then call AsyncGetItemsForFeed() for each of them.
		/// </summary>
		/// <param name="startNode">Node to start with</param>
		/// <param name="forceRefresh">true, if refresh should be forced</param>
		/// <param name="categorized">indicates whether this is part of the refresh or click of a category node</param>
		/// <param name="initialNode">This is the node where the refresh began from</param>
		/// <param name="unreadItems">an array list to place the unread items in the category into. This is needed to render them afterwards 
		/// in a newspaper view</param>
		private void WalkdownThenRefreshFeed(FeedTreeNodeBase startNode, bool forceRefresh, bool categorized, FeedTreeNodeBase initialNode, FeedInfoList unreadItems) {
			if (startNode == null) return;
			
			if (TreeSelectedNode != initialNode)
				return;	// do not continue, if selection was changed

			try {
				for (FeedTreeNodeBase child = startNode.FirstNode; child != null; child = child.NextNode) {

					if (this.Disposing)
						return;
				
					if (child.Type != FeedNodeType.Feed && child.FirstNode != null) {
						//if (forceRefresh) {
						WalkdownThenRefreshFeed(child, forceRefresh, categorized, initialNode, unreadItems);
						//}
					} else {
						string feedUrl =(string)child.Tag;
					
						if (feedUrl == null || !owner.FeedHandler.FeedsTable.Contains(feedUrl))
							continue;

						try {
							if(forceRefresh){
								//owner.FeedHandler.AsyncGetItemsForFeed(feedUrl, forceRefresh);
								this.DelayTask(DelayedTasks.StartRefreshOneFeed, feedUrl);
							}else if(categorized){
								ArrayList items = owner.FeedHandler.GetCachedItemsForFeed(feedUrl);									
								feedsFeed f = owner.FeedHandler.FeedsTable[feedUrl];
								FeedInfo fi = null;
					
								if (f != null){									
									fi = (FeedInfo) owner.FeedHandler.GetFeedInfo(f.link);
									
									if (fi == null)	// with with an error, and the like: ignore
										continue;
									
									fi = (FeedInfo) fi.Clone();
									fi.ItemsList.Clear();
								}else{
									fi = new FeedInfo(String.Empty, new ArrayList(), String.Empty, String.Empty, String.Empty, new Hashtable()); 
								}
									
								foreach(NewsItem i in items){
									if(!i.BeenRead)
									fi.ItemsList.Add(i);
								}

								if(fi.ItemsList.Count > 0){
									unreadItems.Add(fi);
								}else{
									fi = null; 
								}

								this.PopulateListView(child, items, false, true, initialNode); 
								Application.DoEvents();
							}
						}
						catch (Exception e) {
							owner.PublishXmlFeedError(e, feedUrl, true);
						}

					}
				}//for


			} catch (Exception ex) {
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
		private void WalkdownAndCatchupCategory(FeedTreeNodeBase startNode) {
			if (startNode == null) return;

			if (startNode.Type == FeedNodeType.Category) {
				for (FeedTreeNodeBase child = startNode.FirstNode; child != null; child = child.NextNode) {
					if (child.Type == FeedNodeType.Category) 
						WalkdownAndCatchupCategory(child);
					else {
						owner.FeedHandler.MarkAllCachedItemsAsRead((string)child.Tag);
						child.UpdateReadStatus(child, 0);
					}
				}
			} else {
				owner.FeedHandler.MarkAllCachedItemsAsRead((string)startNode.Tag);
			}
		}

		/// <summary>
		/// Helper. Work recursive on the startNode down to the leaves.
		/// Then rename categories on any FeedNode within owner.FeedsTable.
		/// </summary>
		/// <param name="startNode">Node to start with. The startNode itself is 
		/// not considered on renaming.</param>
		/// <param name="newCategory">new full category name (long name, with all the '\').</param>
		private void WalkdownThenRenameFeedCategory(FeedTreeNodeBase startNode, string newCategory) {
			if (startNode == null) return;
			feedsFeed f = null;

			if (startNode.Type == FeedNodeType.Feed) {
				f = owner.FeedHandler.FeedsTable[(string)startNode.Tag];
				f.category  = newCategory;	// may be null: then it is the default category "[Unassigned feeds]"
				owner.FeedlistModified = true;
				if (newCategory != null && !owner.FeedHandler.Categories.ContainsKey(newCategory))
					owner.FeedHandler.Categories.Add(newCategory);
			}
			else {	// other
				for (FeedTreeNodeBase child = startNode.FirstNode; child != null; child = child.NextNode) {
					if (child.Type == FeedNodeType.Feed) 
						WalkdownThenRenameFeedCategory(child, BuildCategoryStoreName(child.Parent));
					else
						WalkdownThenRenameFeedCategory(child, null /* BuildCategoryStoreName(child) */ );	// catname will be recalculated on each CategoryNode
				}
			}
		}

		/// <summary>
		/// Helper. Work recursive on the startNode down to the leaves.
		/// Then delete all child categories and FeedNode refs in owner.FeedHandler.
		/// </summary>
		/// <param name="startNode">Node to start with. The startNode itself is 
		/// considered on delete.</param>
		/// <param name="startNode">new full category name (long name, with all the '\').</param>
		private void WalkdownThenDeleteFeedsOrCategories(FeedTreeNodeBase startNode) {
			if (startNode == null) return;

			if (startNode.Type == FeedNodeType.Feed) {
				if (owner.FeedHandler.FeedsTable.ContainsKey((string)startNode.Tag) ) {
					feedsFeed f = owner.FeedHandler.FeedsTable[(string)startNode.Tag];
					try {
						f.Tag = null;
						owner.FeedHandler.DeleteFeed(f.link);
					} catch {}
					if (owner.FeedHandler.Categories.ContainsKey(f.category) )
						owner.FeedHandler.Categories.Remove(f.category);
				}
				else {
					string catName = BuildCategoryStoreName(startNode.Parent);
					if (owner.FeedHandler.Categories.ContainsKey(catName) )
						owner.FeedHandler.Categories.Remove(catName);
				}

			}
			else {	// other
				string catName = BuildCategoryStoreName(startNode);
				if (owner.FeedHandler.Categories.ContainsKey(catName) )
					owner.FeedHandler.Categories.Remove(catName);

				for (FeedTreeNodeBase child = startNode.FirstNode; child != null; child = child.NextNode) {
					WalkdownThenDeleteFeedsOrCategories(child);
				}
			}
		}

//		bool StoreFeedColumnLayout(FeedTreeNodeBase startNode, string layout) {
//			if (layout == null) throw new ArgumentNullException("layout");
//			if (startNode == null) return false;
//
//			if (startNode.Type == FeedNodeType.Feed) {
//				if (!StringHelper.EmptyOrNull(owner.FeedHandler.GetFeedColumnLayout((string)startNode.Tag)))
//					owner.FeedHandler.SetFeedColumnLayout((string)startNode.Tag, layout);
//				else
//					CurrentFeedFeedColumnLayout = layout;
//			} else if(startNode.Type == FeedNodeType.Category) {
//				if (!StringHelper.EmptyOrNull(owner.FeedHandler.GetCategoryFeedColumnLayout((string)startNode.Tag)))
//					owner.FeedHandler.SetCategoryFeedColumnLayout((string)startNode.Tag, layout);
//				else
//					CurrentCategoryFeedColumnLayout = layout;
//			} else {
//				CurrentSmartFolderFeedColumnLayout = layout;
//			}
//			
//			return true;
//		}

		//TODO: impl.
		FeedColumnLayout GetFeedColumnLayout(FeedTreeNodeBase startNode) {
			if (startNode == null) 
				startNode = TreeSelectedNode;
			if (startNode == null) 
				return listFeedItems.FeedColumnLayout;

			FeedColumnLayout layout = listFeedItems.FeedColumnLayout;
			if (startNode.Type == FeedNodeType.Feed) {
				layout = owner.GetFeedColumnLayout((string)startNode.Tag);
				if (layout == null) layout = owner.GlobalFeedColumnLayout;
			} else if(startNode.Type == FeedNodeType.Category) {
				layout = owner.GetCategoryColumnLayout(this.BuildCategoryStoreName(startNode));
				if (layout == null)	layout= owner.GlobalCategoryColumnLayout;
			} else if(startNode.Type == FeedNodeType.Finder) {
				layout= owner.GlobalSearchFolderColumnLayout;
			} else if(startNode.Type == FeedNodeType.SmartFolder) {
				layout= owner.GlobalSpecialFolderColumnLayout;
			}
			return layout;
		}

		private void SetFeedHandlerFeedColumnLayout(FeedTreeNodeBase node, FeedColumnLayout layout) {
			if (node == null) node = CurrentSelectedNode;
			if (node != null) {
				if (node.Type == FeedNodeType.Feed) {
					owner.SetFeedColumnLayout((string)node.Tag, layout);
				} else if (node.Type == FeedNodeType.Category) {
					owner.SetCategoryColumnLayout(this.BuildCategoryStoreName(node), layout);
				} else if(node.Type == FeedNodeType.Finder) {
					owner.GlobalSearchFolderColumnLayout = layout;
				} else if(node.Type == FeedNodeType.SmartFolder) {
					owner.GlobalSpecialFolderColumnLayout = layout;
				}
			}

		}

		private void SetGlobalFeedColumnLayout(FeedNodeType type, FeedColumnLayout layout) {
			if (layout == null) throw new ArgumentNullException("layout");

			if (type == FeedNodeType.Feed) {
				owner.GlobalFeedColumnLayout = layout;
			} else if(type == FeedNodeType.Category) {
				owner.GlobalCategoryColumnLayout = layout;
			} else {
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
		public ThreadedListViewItem GetListViewItem(NewsItem item) {
		
			ThreadedListViewItem theItem = null;  
			for (int i = 0; i < this.listFeedItems.Items.Count; i++) {
				ThreadedListViewItem currentItem = this.listFeedItems.Items[i];
				if(item.Equals((NewsItem)currentItem.Key)) {
					theItem = currentItem; 
					break; 
				}
			}
			return theItem; 
		}

		/// <summary>
		/// A helper method that locates the tree node containing the feed 
		/// that an NewsItem object belongs to. 
		/// </summary>
		/// <param name="item">The RSS item</param>
		/// <returns>The tree node this object belongs to or null if 
		/// it can't be found</returns>
		public FeedTreeNodeBase GetTreeNodeForItem(FeedTreeNodeBase startNode, NewsItem item) {
			return this.GetTreeNodeForItem(startNode, item.Feed);
		}

		/// <summary>
		/// Overloaded helper method that locates the tree node containing the feed. 
		/// </summary>
		/// <param name="f">The FeedsFeed</param>
		/// <returns>The tree node this object belongs to or null if 
		/// it can't be found</returns>
		public FeedTreeNodeBase GetTreeNodeForItem(FeedTreeNodeBase startNode, feedsFeed f) {
			
			FeedTreeNodeBase assocNode = f.Tag as FeedTreeNodeBase;
			if (assocNode != null)
				return assocNode;

			return this.GetTreeNodeForItem(startNode, f.link);
		}

		/// <summary>
		/// Overloaded helper method that locates the tree node containing the feed. 
		/// </summary>
		/// <param name="feedUrl">The Feed Url</param>
		/// <returns>The tree node this object belongs to or null if 
		/// it can't be found</returns>
		public FeedTreeNodeBase GetTreeNodeForItem(FeedTreeNodeBase startNode, string feedUrl) {
		
			if (feedUrl == null || feedUrl.Trim().Length == 0)
				return null;

			FeedTreeNodeBase ownernode = null;  

			if (startNode != null) {

				if( feedUrl.Equals(startNode.Tag) ) {
					return startNode;
				}
	
				foreach(FeedTreeNodeBase t in startNode.Nodes) {
					if( feedUrl.Equals(t.Tag)  && 
						(t.Type != FeedNodeType.Root && t.Type != FeedNodeType.Category) ) {
						ownernode = t; 
						break; 
					}
					
					if (t.Nodes.Count > 0) {
						ownernode = GetTreeNodeForItem(t, feedUrl);
						if (ownernode != null) 
							break;
					}
				}
			}
			return ownernode; 
		}

		/// <summary>
		/// Find a direct child node.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="text"></param>
		/// <param name="nType"></param>
		/// <returns></returns>
		private FeedTreeNodeBase FindChild(FeedTreeNodeBase n, string text, FeedNodeType nType) {
			if (n == null || text == null) return null;
			text = text.Trim();
			for (FeedTreeNodeBase t = n.FirstNode; t != null; t = t.NextNode)	{	
				if (t.Type == nType && String.Compare(t.Key, text, false, CultureInfo.CurrentCulture) == 0)	// node names are usually english or client locale
					return t;
			}
			return null;
		}

		/// <summary>
		/// Traverse down the tree on the path defined by 'category' 
		/// starting with 'startNode'.
		/// </summary>
		/// <param name="startNode">FeedTreeNodeBase to start with</param>
		/// <param name="category">A category path, e.g. 'Category1\SubCategory1'.</param>
		/// <returns>The leave category node.</returns>
		/// <remarks>If one category in the path is not found, it will be created.</remarks>
		internal FeedTreeNodeBase CreateCategoryHive(FeedTreeNodeBase startNode, string category)	{
			return this.CreateCategoryHive(startNode, category, false);
		}
		
		/// <summary>
		/// Traverse down the tree on the path defined by 'category' 
		/// starting with 'startNode'.
		/// </summary>
		/// <param name="startNode">FeedTreeNodeBase to start with</param>
		/// <param name="category">A category path, e.g. 'Category1\SubCategory1'.</param>
		/// <param name="isFinderCategory">True, if it has to create a FinderCategoryNode, else false (creates a CategoryNode)</param>
		/// <returns>The leave category node.</returns>
		/// <remarks>If one category in the path is not found, it will be created.</remarks>
		internal FeedTreeNodeBase CreateCategoryHive(FeedTreeNodeBase startNode, string category, bool isFinderCategory)	{

			if (category == null || category.Length == 0 || startNode == null) return startNode;

			string[] catHives = category.Split(new char[]{'\\'});
			FeedNodeType nType = (isFinderCategory ? FeedNodeType.FinderCategory: FeedNodeType.Category);
			FeedTreeNodeBase n = null;
			bool wasNew = false;

			foreach (string catHive in catHives){

				if (!wasNew) 
					n = FindChild(startNode, catHive, nType);
				else
					n = null;

				if (n == null) {
					
					if (isFinderCategory) {
						n = new FinderCategoryNode(catHive, 2, 3, _treeSearchFolderContextMenu);
					} else {
						n = new CategoryNode(catHive, 2, 3, _treeCategoryContextMenu);
					}
					startNode.Nodes.Add(n);
					wasNew = true;	// shorten search
				}

				startNode = n;

			}//foreach
			
			return startNode;
		}


		private void DoEditTreeNodeLabel(){
			
			if(CurrentSelectedNode!= null){
				CurrentSelectedNode.BeginEdit();
			}			
		}

		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			try {
				if( disposing ) {
					if(components != null) {
						components.Dispose();
					}
				}
				base.Dispose( disposing );
			} catch{}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WinGuiMain));
			this.panelFeedDetails = new System.Windows.Forms.Panel();
			this.panelWebDetail = new System.Windows.Forms.Panel();
			this.htmlDetail = new IEControl.HtmlControl();
			this.detailsPaneSplitter = new CollapsibleSplitter();
			this.panelFeedItems = new System.Windows.Forms.Panel();
			this.listFeedItems = new System.Windows.Forms.ThListView.ThreadedListView();
			this.colHeadline = new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader();
			this.colDate = new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader();
			this.colTopic = new System.Windows.Forms.ThListView.ThreadedListViewColumnHeader();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.comboBoxRssSearchItemPostedOperator = new System.Windows.Forms.ComboBox();
			this.dateTimeRssSearchItemPost = new System.Windows.Forms.DateTimePicker();
			this.dateTimeRssSearchPostAfter = new System.Windows.Forms.DateTimePicker();
			this.dateTimeRssSearchPostBefore = new System.Windows.Forms.DateTimePicker();
			this.treeFeeds = new System.Windows.Forms.TreeView();
			this.panelRssSearch = new System.Windows.Forms.Panel();
			this.taskPaneSearchOptions = new XPExplorerBar.TaskPane();
			this.collapsiblePanelSearchNameEx = new XPExplorerBar.Expando();
			this.btnRssSearchSave = new System.Windows.Forms.Button();
			this.labelSearchFolderNameHint = new System.Windows.Forms.Label();
			this.textFinderCaption = new System.Windows.Forms.TextBox();
			this.collapsiblePanelRssSearchExprKindEx = new XPExplorerBar.Expando();
			this.radioRssSearchSimpleText = new System.Windows.Forms.RadioButton();
			this.labelRssSearchTypeHint = new System.Windows.Forms.Label();
			this.radioRssSearchExprXPath = new System.Windows.Forms.RadioButton();
			this.radioRssSearchRegEx = new System.Windows.Forms.RadioButton();
			this.collapsiblePanelItemPropertiesEx = new XPExplorerBar.Expando();
			this.checkBoxRssSearchInDesc = new System.Windows.Forms.CheckBox();
			this.checkBoxRssSearchInCategory = new System.Windows.Forms.CheckBox();
			this.checkBoxRssSearchInTitle = new System.Windows.Forms.CheckBox();
			this.checkBoxRssSearchInLink = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.collapsiblePanelAdvancedOptionsEx = new XPExplorerBar.Expando();
			this.checkBoxConsiderItemReadState = new System.Windows.Forms.CheckBox();
			this.checkBoxRssSearchUnreadItems = new System.Windows.Forms.CheckBox();
			this.horizontalEdge = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.checkBoxRssSearchTimeSpan = new System.Windows.Forms.CheckBox();
			this.radioRssSearchItemsOlderThan = new System.Windows.Forms.RadioButton();
			this.comboRssSearchItemAge = new System.Windows.Forms.ComboBox();
			this.radioRssSearchItemsYoungerThan = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.checkBoxRssSearchByDate = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxRssSearchByDateRange = new System.Windows.Forms.CheckBox();
			this.collapsiblePanelRssSearchScopeEx = new XPExplorerBar.Expando();
			this.treeRssSearchScope = new System.Windows.Forms.TreeView();
			this.searchPaneSplitter = new CollapsibleSplitter();
			this.panelRssSearchCommands = new System.Windows.Forms.Panel();
			this.btnNewSearch = new System.Windows.Forms.Button();
			this.textSearchExpression = new System.Windows.Forms.TextBox();
			this.btnSearchCancel = new System.Windows.Forms.Button();
			this.labelRssSearchState = new System.Windows.Forms.Label();
			this._status = new System.Windows.Forms.StatusBar();
			this.statusBarBrowser = new System.Windows.Forms.StatusBarPanel();
			this.statusBarBrowserProgress = new System.Windows.Forms.StatusBarPanel();
			this.statusBarConnectionState = new System.Windows.Forms.StatusBarPanel();
			this.statusBarRssParser = new System.Windows.Forms.StatusBarPanel();
			this.bottomSandBarDock = new TD.SandBar.ToolBarContainer();
			this.sandBarManager = new TD.SandBar.SandBarManager();
			this.leftSandBarDock = new TD.SandBar.ToolBarContainer();
			this.rightSandBarDock = new TD.SandBar.ToolBarContainer();
			this.topSandBarDock = new TD.SandBar.ToolBarContainer();
			this.toolBarMain = new TD.SandBar.ToolBar();
			this.menuBarMain = new TD.SandBar.MenuBar();
			this.toolBarBrowser = new TD.SandBar.ToolBar();
			this.toolBarWebSearch = new TD.SandBar.ToolBar();
			this.progressBrowser = new System.Windows.Forms.ProgressBar();
			this.leftSandDock = new TD.SandDock.DockContainer();
			this.dockSubscriptions = new TD.SandDock.DockControl();
			this.dockSearch = new TD.SandDock.DockControl();
			this.sandDockManager = new TD.SandDock.SandDockManager();
			this.rightSandDock = new TD.SandDock.DockContainer();
			this.bottomSandDock = new TD.SandDock.DockContainer();
			this.topSandDock = new TD.SandDock.DockContainer();
			this._docContainer = new TD.SandDock.DocumentContainer();
			this._docFeedDetails = new TD.SandDock.DockControl();
			this._timerTreeNodeExpand = new System.Timers.Timer();
			this._timerRefreshFeeds = new System.Timers.Timer();
			this._timerResetStatus = new System.Windows.Forms.Timer(this.components);
			this._uiTasksTimer = new RssBandit.WinGui.Forms.WinGuiMain.UITaskTimer(this.components);
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			this.panelFeedDetails.SuspendLayout();
			this.panelWebDetail.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.htmlDetail)).BeginInit();
			this.panelFeedItems.SuspendLayout();
			this.panelRssSearch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.taskPaneSearchOptions)).BeginInit();
			this.taskPaneSearchOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelSearchNameEx)).BeginInit();
			this.collapsiblePanelSearchNameEx.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelRssSearchExprKindEx)).BeginInit();
			this.collapsiblePanelRssSearchExprKindEx.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelItemPropertiesEx)).BeginInit();
			this.collapsiblePanelItemPropertiesEx.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelAdvancedOptionsEx)).BeginInit();
			this.collapsiblePanelAdvancedOptionsEx.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelRssSearchScopeEx)).BeginInit();
			this.collapsiblePanelRssSearchScopeEx.SuspendLayout();
			this.panelRssSearchCommands.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.statusBarBrowser)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarBrowserProgress)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarConnectionState)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarRssParser)).BeginInit();
			this.topSandBarDock.SuspendLayout();
			this.leftSandDock.SuspendLayout();
			this.dockSubscriptions.SuspendLayout();
			this.dockSearch.SuspendLayout();
			this._docContainer.SuspendLayout();
			this._docFeedDetails.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._timerTreeNodeExpand)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._timerRefreshFeeds)).BeginInit();
			this.SuspendLayout();
			// 
			// panelFeedDetails
			// 
			this.panelFeedDetails.AccessibleDescription = resources.GetString("panelFeedDetails.AccessibleDescription");
			this.panelFeedDetails.AccessibleName = resources.GetString("panelFeedDetails.AccessibleName");
			this.panelFeedDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelFeedDetails.Anchor")));
			this.panelFeedDetails.AutoScroll = ((bool)(resources.GetObject("panelFeedDetails.AutoScroll")));
			this.panelFeedDetails.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelFeedDetails.AutoScrollMargin")));
			this.panelFeedDetails.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelFeedDetails.AutoScrollMinSize")));
			this.panelFeedDetails.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelFeedDetails.BackgroundImage")));
			this.panelFeedDetails.Controls.Add(this.panelWebDetail);
			this.panelFeedDetails.Controls.Add(this.detailsPaneSplitter);
			this.panelFeedDetails.Controls.Add(this.panelFeedItems);
			this.panelFeedDetails.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelFeedDetails.Dock")));
			this.panelFeedDetails.Enabled = ((bool)(resources.GetObject("panelFeedDetails.Enabled")));
			this.panelFeedDetails.Font = ((System.Drawing.Font)(resources.GetObject("panelFeedDetails.Font")));
			this.helpProvider1.SetHelpKeyword(this.panelFeedDetails, resources.GetString("panelFeedDetails.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.panelFeedDetails, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("panelFeedDetails.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.panelFeedDetails, resources.GetString("panelFeedDetails.HelpString"));
			this.panelFeedDetails.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelFeedDetails.ImeMode")));
			this.panelFeedDetails.Location = ((System.Drawing.Point)(resources.GetObject("panelFeedDetails.Location")));
			this.panelFeedDetails.Name = "panelFeedDetails";
			this.panelFeedDetails.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelFeedDetails.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.panelFeedDetails, ((bool)(resources.GetObject("panelFeedDetails.ShowHelp"))));
			this.panelFeedDetails.Size = ((System.Drawing.Size)(resources.GetObject("panelFeedDetails.Size")));
			this.panelFeedDetails.TabIndex = ((int)(resources.GetObject("panelFeedDetails.TabIndex")));
			this.panelFeedDetails.Text = resources.GetString("panelFeedDetails.Text");
			this.toolTip.SetToolTip(this.panelFeedDetails, resources.GetString("panelFeedDetails.ToolTip"));
			this.panelFeedDetails.Visible = ((bool)(resources.GetObject("panelFeedDetails.Visible")));
			// 
			// panelWebDetail
			// 
			this.panelWebDetail.AccessibleDescription = resources.GetString("panelWebDetail.AccessibleDescription");
			this.panelWebDetail.AccessibleName = resources.GetString("panelWebDetail.AccessibleName");
			this.panelWebDetail.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelWebDetail.Anchor")));
			this.panelWebDetail.AutoScroll = ((bool)(resources.GetObject("panelWebDetail.AutoScroll")));
			this.panelWebDetail.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelWebDetail.AutoScrollMargin")));
			this.panelWebDetail.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelWebDetail.AutoScrollMinSize")));
			this.panelWebDetail.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelWebDetail.BackgroundImage")));
			this.panelWebDetail.Controls.Add(this.htmlDetail);
			this.panelWebDetail.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelWebDetail.Dock")));
			this.panelWebDetail.Enabled = ((bool)(resources.GetObject("panelWebDetail.Enabled")));
			this.panelWebDetail.Font = ((System.Drawing.Font)(resources.GetObject("panelWebDetail.Font")));
			this.helpProvider1.SetHelpKeyword(this.panelWebDetail, resources.GetString("panelWebDetail.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.panelWebDetail, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("panelWebDetail.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.panelWebDetail, resources.GetString("panelWebDetail.HelpString"));
			this.panelWebDetail.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelWebDetail.ImeMode")));
			this.panelWebDetail.Location = ((System.Drawing.Point)(resources.GetObject("panelWebDetail.Location")));
			this.panelWebDetail.Name = "panelWebDetail";
			this.panelWebDetail.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelWebDetail.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.panelWebDetail, ((bool)(resources.GetObject("panelWebDetail.ShowHelp"))));
			this.panelWebDetail.Size = ((System.Drawing.Size)(resources.GetObject("panelWebDetail.Size")));
			this.panelWebDetail.TabIndex = ((int)(resources.GetObject("panelWebDetail.TabIndex")));
			this.panelWebDetail.Text = resources.GetString("panelWebDetail.Text");
			this.toolTip.SetToolTip(this.panelWebDetail, resources.GetString("panelWebDetail.ToolTip"));
			this.panelWebDetail.Visible = ((bool)(resources.GetObject("panelWebDetail.Visible")));
			// 
			// htmlDetail
			// 
			this.htmlDetail.AccessibleDescription = resources.GetString("htmlDetail.AccessibleDescription");
			this.htmlDetail.AccessibleName = resources.GetString("htmlDetail.AccessibleName");
			this.htmlDetail.AllowDrop = true;
			this.htmlDetail.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("htmlDetail.Anchor")));
			this.htmlDetail.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("htmlDetail.BackgroundImage")));
			this.htmlDetail.ContainingControl = this;
			this.htmlDetail.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("htmlDetail.Dock")));
			this.htmlDetail.Enabled = ((bool)(resources.GetObject("htmlDetail.Enabled")));
			this.htmlDetail.Font = ((System.Drawing.Font)(resources.GetObject("htmlDetail.Font")));
			this.helpProvider1.SetHelpKeyword(this.htmlDetail, resources.GetString("htmlDetail.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.htmlDetail, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("htmlDetail.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.htmlDetail, resources.GetString("htmlDetail.HelpString"));
			this.htmlDetail.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("htmlDetail.ImeMode")));
			this.htmlDetail.Location = ((System.Drawing.Point)(resources.GetObject("htmlDetail.Location")));
			this.htmlDetail.Name = "htmlDetail";
			this.htmlDetail.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("htmlDetail.OcxState")));
			this.htmlDetail.RightToLeft = ((bool)(resources.GetObject("htmlDetail.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.htmlDetail, ((bool)(resources.GetObject("htmlDetail.ShowHelp"))));
			this.htmlDetail.Size = ((System.Drawing.Size)(resources.GetObject("htmlDetail.Size")));
			this.htmlDetail.TabIndex = ((int)(resources.GetObject("htmlDetail.TabIndex")));
			this.htmlDetail.Text = resources.GetString("htmlDetail.Text");
			this.toolTip.SetToolTip(this.htmlDetail, resources.GetString("htmlDetail.ToolTip"));
			this.htmlDetail.Visible = ((bool)(resources.GetObject("htmlDetail.Visible")));
			// 
			// detailsPaneSplitter
			// 
			this.detailsPaneSplitter.AccessibleDescription = resources.GetString("detailsPaneSplitter.AccessibleDescription");
			this.detailsPaneSplitter.AccessibleName = resources.GetString("detailsPaneSplitter.AccessibleName");
			this.detailsPaneSplitter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("detailsPaneSplitter.Anchor")));
			this.detailsPaneSplitter.AnimationDelay = 20;
			this.detailsPaneSplitter.AnimationStep = 20;
			this.detailsPaneSplitter.BackColor = System.Drawing.SystemColors.Control;
			this.detailsPaneSplitter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("detailsPaneSplitter.BackgroundImage")));
			this.detailsPaneSplitter.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
			this.detailsPaneSplitter.ControlToHide = this.panelFeedItems;
			this.detailsPaneSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.detailsPaneSplitter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("detailsPaneSplitter.Dock")));
			this.detailsPaneSplitter.Enabled = ((bool)(resources.GetObject("detailsPaneSplitter.Enabled")));
			this.detailsPaneSplitter.ExpandParentForm = false;
			this.detailsPaneSplitter.Font = ((System.Drawing.Font)(resources.GetObject("detailsPaneSplitter.Font")));
			this.helpProvider1.SetHelpKeyword(this.detailsPaneSplitter, resources.GetString("detailsPaneSplitter.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.detailsPaneSplitter, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("detailsPaneSplitter.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.detailsPaneSplitter, resources.GetString("detailsPaneSplitter.HelpString"));
			this.detailsPaneSplitter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("detailsPaneSplitter.ImeMode")));
			this.detailsPaneSplitter.Location = ((System.Drawing.Point)(resources.GetObject("detailsPaneSplitter.Location")));
			this.detailsPaneSplitter.MinExtra = ((int)(resources.GetObject("detailsPaneSplitter.MinExtra")));
			this.detailsPaneSplitter.MinSize = ((int)(resources.GetObject("detailsPaneSplitter.MinSize")));
			this.detailsPaneSplitter.Name = "detailsPaneSplitter";
			this.detailsPaneSplitter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("detailsPaneSplitter.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.detailsPaneSplitter, ((bool)(resources.GetObject("detailsPaneSplitter.ShowHelp"))));
			this.detailsPaneSplitter.TabIndex = ((int)(resources.GetObject("detailsPaneSplitter.TabIndex")));
			this.detailsPaneSplitter.TabStop = false;
			this.toolTip.SetToolTip(this.detailsPaneSplitter, resources.GetString("detailsPaneSplitter.ToolTip"));
			this.detailsPaneSplitter.UseAnimations = false;
			this.detailsPaneSplitter.Visible = ((bool)(resources.GetObject("detailsPaneSplitter.Visible")));
			this.detailsPaneSplitter.VisualStyle = VisualStyles.XP;
			// 
			// panelFeedItems
			// 
			this.panelFeedItems.AccessibleDescription = resources.GetString("panelFeedItems.AccessibleDescription");
			this.panelFeedItems.AccessibleName = resources.GetString("panelFeedItems.AccessibleName");
			this.panelFeedItems.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelFeedItems.Anchor")));
			this.panelFeedItems.AutoScroll = ((bool)(resources.GetObject("panelFeedItems.AutoScroll")));
			this.panelFeedItems.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelFeedItems.AutoScrollMargin")));
			this.panelFeedItems.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelFeedItems.AutoScrollMinSize")));
			this.panelFeedItems.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelFeedItems.BackgroundImage")));
			this.panelFeedItems.Controls.Add(this.listFeedItems);
			this.panelFeedItems.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelFeedItems.Dock")));
			this.panelFeedItems.Enabled = ((bool)(resources.GetObject("panelFeedItems.Enabled")));
			this.panelFeedItems.Font = ((System.Drawing.Font)(resources.GetObject("panelFeedItems.Font")));
			this.helpProvider1.SetHelpKeyword(this.panelFeedItems, resources.GetString("panelFeedItems.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.panelFeedItems, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("panelFeedItems.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.panelFeedItems, resources.GetString("panelFeedItems.HelpString"));
			this.panelFeedItems.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelFeedItems.ImeMode")));
			this.panelFeedItems.Location = ((System.Drawing.Point)(resources.GetObject("panelFeedItems.Location")));
			this.panelFeedItems.Name = "panelFeedItems";
			this.panelFeedItems.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelFeedItems.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.panelFeedItems, ((bool)(resources.GetObject("panelFeedItems.ShowHelp"))));
			this.panelFeedItems.Size = ((System.Drawing.Size)(resources.GetObject("panelFeedItems.Size")));
			this.panelFeedItems.TabIndex = ((int)(resources.GetObject("panelFeedItems.TabIndex")));
			this.panelFeedItems.Text = resources.GetString("panelFeedItems.Text");
			this.toolTip.SetToolTip(this.panelFeedItems, resources.GetString("panelFeedItems.ToolTip"));
			this.panelFeedItems.Visible = ((bool)(resources.GetObject("panelFeedItems.Visible")));
			// 
			// listFeedItems
			// 
			this.listFeedItems.AccessibleDescription = resources.GetString("listFeedItems.AccessibleDescription");
			this.listFeedItems.AccessibleName = resources.GetString("listFeedItems.AccessibleName");
			this.listFeedItems.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.listFeedItems.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listFeedItems.Alignment")));
			this.listFeedItems.AllowColumnReorder = true;
			this.listFeedItems.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listFeedItems.Anchor")));
			this.listFeedItems.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listFeedItems.BackgroundImage")));
			this.listFeedItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.colHeadline,
																							this.colDate,
																							this.colTopic});
			this.listFeedItems.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listFeedItems.Dock")));
			this.listFeedItems.Enabled = ((bool)(resources.GetObject("listFeedItems.Enabled")));
			this.listFeedItems.Font = ((System.Drawing.Font)(resources.GetObject("listFeedItems.Font")));
			this.listFeedItems.FullRowSelect = true;
			this.helpProvider1.SetHelpKeyword(this.listFeedItems, resources.GetString("listFeedItems.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.listFeedItems, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("listFeedItems.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.listFeedItems, resources.GetString("listFeedItems.HelpString"));
			this.listFeedItems.HideSelection = false;
			this.listFeedItems.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listFeedItems.ImeMode")));
			this.listFeedItems.LabelWrap = ((bool)(resources.GetObject("listFeedItems.LabelWrap")));
			this.listFeedItems.Location = ((System.Drawing.Point)(resources.GetObject("listFeedItems.Location")));
			this.listFeedItems.Name = "listFeedItems";
			this.listFeedItems.NoThreadChildsPlaceHolder = null;
			this.listFeedItems.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listFeedItems.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.listFeedItems, ((bool)(resources.GetObject("listFeedItems.ShowHelp"))));
			this.listFeedItems.Size = ((System.Drawing.Size)(resources.GetObject("listFeedItems.Size")));
			this.listFeedItems.TabIndex = ((int)(resources.GetObject("listFeedItems.TabIndex")));
			this.listFeedItems.Text = resources.GetString("listFeedItems.Text");
			this.toolTip.SetToolTip(this.listFeedItems, resources.GetString("listFeedItems.ToolTip"));
			this.listFeedItems.View = System.Windows.Forms.View.Details;
			this.listFeedItems.Visible = ((bool)(resources.GetObject("listFeedItems.Visible")));
			this.listFeedItems.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnFeedListMouseDown);
			this.listFeedItems.ItemActivate += new System.EventHandler(this.OnFeedListItemActivate);
			this.listFeedItems.ListLayoutModified += new System.Windows.Forms.ThListView.ThreadedListView.OnListLayoutModifiedEventHandler(this.OnFeedListLayoutModified);
			this.listFeedItems.ListLayoutChanged += new System.Windows.Forms.ThListView.ThreadedListView.OnListLayoutChangedEventHandler(this.OnFeedListLayoutChanged);
			this.listFeedItems.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnFeedListItemDrag);
			this.listFeedItems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFeedListItemKeyUp);
			this.listFeedItems.ExpandThread += new System.Windows.Forms.ThListView.ThreadedListView.OnExpandThreadEventHandler(this.OnFeedListExpandThread);
			// 
			// colHeadline
			// 
			this.colHeadline.ColumnValueType = typeof(string);
			this.colHeadline.Key = "Title";
			this.colHeadline.Text = resources.GetString("colHeadline.Text");
			this.colHeadline.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("colHeadline.TextAlign")));
			this.colHeadline.Width = ((int)(resources.GetObject("colHeadline.Width")));
			// 
			// colDate
			// 
			this.colDate.ColumnValueType = typeof(System.DateTime);
			this.colDate.Key = "Date";
			this.colDate.Text = resources.GetString("colDate.Text");
			this.colDate.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("colDate.TextAlign")));
			this.colDate.Width = ((int)(resources.GetObject("colDate.Width")));
			// 
			// colTopic
			// 
			this.colTopic.ColumnValueType = typeof(string);
			this.colTopic.Key = "Subject";
			this.colTopic.Text = resources.GetString("colTopic.Text");
			this.colTopic.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("colTopic.TextAlign")));
			this.colTopic.Width = ((int)(resources.GetObject("colTopic.Width")));
			// 
			// comboBoxRssSearchItemPostedOperator
			// 
			this.comboBoxRssSearchItemPostedOperator.AccessibleDescription = resources.GetString("comboBoxRssSearchItemPostedOperator.AccessibleDescription");
			this.comboBoxRssSearchItemPostedOperator.AccessibleName = resources.GetString("comboBoxRssSearchItemPostedOperator.AccessibleName");
			this.comboBoxRssSearchItemPostedOperator.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboBoxRssSearchItemPostedOperator.Anchor")));
			this.comboBoxRssSearchItemPostedOperator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboBoxRssSearchItemPostedOperator.BackgroundImage")));
			this.comboBoxRssSearchItemPostedOperator.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboBoxRssSearchItemPostedOperator.Dock")));
			this.comboBoxRssSearchItemPostedOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxRssSearchItemPostedOperator.Enabled = ((bool)(resources.GetObject("comboBoxRssSearchItemPostedOperator.Enabled")));
			this.comboBoxRssSearchItemPostedOperator.Font = ((System.Drawing.Font)(resources.GetObject("comboBoxRssSearchItemPostedOperator.Font")));
			this.helpProvider1.SetHelpKeyword(this.comboBoxRssSearchItemPostedOperator, resources.GetString("comboBoxRssSearchItemPostedOperator.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.comboBoxRssSearchItemPostedOperator, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("comboBoxRssSearchItemPostedOperator.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.comboBoxRssSearchItemPostedOperator, resources.GetString("comboBoxRssSearchItemPostedOperator.HelpString"));
			this.comboBoxRssSearchItemPostedOperator.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboBoxRssSearchItemPostedOperator.ImeMode")));
			this.comboBoxRssSearchItemPostedOperator.IntegralHeight = ((bool)(resources.GetObject("comboBoxRssSearchItemPostedOperator.IntegralHeight")));
			this.comboBoxRssSearchItemPostedOperator.ItemHeight = ((int)(resources.GetObject("comboBoxRssSearchItemPostedOperator.ItemHeight")));
			this.comboBoxRssSearchItemPostedOperator.Items.AddRange(new object[] {
																					 resources.GetString("comboBoxRssSearchItemPostedOperator.Items"),
																					 resources.GetString("comboBoxRssSearchItemPostedOperator.Items1"),
																					 resources.GetString("comboBoxRssSearchItemPostedOperator.Items2")});
			this.comboBoxRssSearchItemPostedOperator.Location = ((System.Drawing.Point)(resources.GetObject("comboBoxRssSearchItemPostedOperator.Location")));
			this.comboBoxRssSearchItemPostedOperator.MaxDropDownItems = ((int)(resources.GetObject("comboBoxRssSearchItemPostedOperator.MaxDropDownItems")));
			this.comboBoxRssSearchItemPostedOperator.MaxLength = ((int)(resources.GetObject("comboBoxRssSearchItemPostedOperator.MaxLength")));
			this.comboBoxRssSearchItemPostedOperator.Name = "comboBoxRssSearchItemPostedOperator";
			this.comboBoxRssSearchItemPostedOperator.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboBoxRssSearchItemPostedOperator.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.comboBoxRssSearchItemPostedOperator, ((bool)(resources.GetObject("comboBoxRssSearchItemPostedOperator.ShowHelp"))));
			this.comboBoxRssSearchItemPostedOperator.Size = ((System.Drawing.Size)(resources.GetObject("comboBoxRssSearchItemPostedOperator.Size")));
			this.comboBoxRssSearchItemPostedOperator.TabIndex = ((int)(resources.GetObject("comboBoxRssSearchItemPostedOperator.TabIndex")));
			this.comboBoxRssSearchItemPostedOperator.Text = resources.GetString("comboBoxRssSearchItemPostedOperator.Text");
			this.toolTip.SetToolTip(this.comboBoxRssSearchItemPostedOperator, resources.GetString("comboBoxRssSearchItemPostedOperator.ToolTip"));
			this.comboBoxRssSearchItemPostedOperator.Visible = ((bool)(resources.GetObject("comboBoxRssSearchItemPostedOperator.Visible")));
			// 
			// dateTimeRssSearchItemPost
			// 
			this.dateTimeRssSearchItemPost.AccessibleDescription = resources.GetString("dateTimeRssSearchItemPost.AccessibleDescription");
			this.dateTimeRssSearchItemPost.AccessibleName = resources.GetString("dateTimeRssSearchItemPost.AccessibleName");
			this.dateTimeRssSearchItemPost.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dateTimeRssSearchItemPost.Anchor")));
			this.dateTimeRssSearchItemPost.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dateTimeRssSearchItemPost.BackgroundImage")));
			this.dateTimeRssSearchItemPost.CalendarFont = ((System.Drawing.Font)(resources.GetObject("dateTimeRssSearchItemPost.CalendarFont")));
			this.dateTimeRssSearchItemPost.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dateTimeRssSearchItemPost.Dock")));
			this.dateTimeRssSearchItemPost.DropDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("dateTimeRssSearchItemPost.DropDownAlign")));
			this.dateTimeRssSearchItemPost.Enabled = ((bool)(resources.GetObject("dateTimeRssSearchItemPost.Enabled")));
			this.dateTimeRssSearchItemPost.Font = ((System.Drawing.Font)(resources.GetObject("dateTimeRssSearchItemPost.Font")));
			this.dateTimeRssSearchItemPost.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.helpProvider1.SetHelpKeyword(this.dateTimeRssSearchItemPost, resources.GetString("dateTimeRssSearchItemPost.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.dateTimeRssSearchItemPost, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("dateTimeRssSearchItemPost.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.dateTimeRssSearchItemPost, resources.GetString("dateTimeRssSearchItemPost.HelpString"));
			this.dateTimeRssSearchItemPost.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dateTimeRssSearchItemPost.ImeMode")));
			this.dateTimeRssSearchItemPost.Location = ((System.Drawing.Point)(resources.GetObject("dateTimeRssSearchItemPost.Location")));
			this.dateTimeRssSearchItemPost.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
			this.dateTimeRssSearchItemPost.Name = "dateTimeRssSearchItemPost";
			this.dateTimeRssSearchItemPost.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dateTimeRssSearchItemPost.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.dateTimeRssSearchItemPost, ((bool)(resources.GetObject("dateTimeRssSearchItemPost.ShowHelp"))));
			this.dateTimeRssSearchItemPost.Size = ((System.Drawing.Size)(resources.GetObject("dateTimeRssSearchItemPost.Size")));
			this.dateTimeRssSearchItemPost.TabIndex = ((int)(resources.GetObject("dateTimeRssSearchItemPost.TabIndex")));
			this.toolTip.SetToolTip(this.dateTimeRssSearchItemPost, resources.GetString("dateTimeRssSearchItemPost.ToolTip"));
			this.dateTimeRssSearchItemPost.Visible = ((bool)(resources.GetObject("dateTimeRssSearchItemPost.Visible")));
			// 
			// dateTimeRssSearchPostAfter
			// 
			this.dateTimeRssSearchPostAfter.AccessibleDescription = resources.GetString("dateTimeRssSearchPostAfter.AccessibleDescription");
			this.dateTimeRssSearchPostAfter.AccessibleName = resources.GetString("dateTimeRssSearchPostAfter.AccessibleName");
			this.dateTimeRssSearchPostAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dateTimeRssSearchPostAfter.Anchor")));
			this.dateTimeRssSearchPostAfter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dateTimeRssSearchPostAfter.BackgroundImage")));
			this.dateTimeRssSearchPostAfter.CalendarFont = ((System.Drawing.Font)(resources.GetObject("dateTimeRssSearchPostAfter.CalendarFont")));
			this.dateTimeRssSearchPostAfter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dateTimeRssSearchPostAfter.Dock")));
			this.dateTimeRssSearchPostAfter.DropDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("dateTimeRssSearchPostAfter.DropDownAlign")));
			this.dateTimeRssSearchPostAfter.Enabled = ((bool)(resources.GetObject("dateTimeRssSearchPostAfter.Enabled")));
			this.dateTimeRssSearchPostAfter.Font = ((System.Drawing.Font)(resources.GetObject("dateTimeRssSearchPostAfter.Font")));
			this.dateTimeRssSearchPostAfter.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.helpProvider1.SetHelpKeyword(this.dateTimeRssSearchPostAfter, resources.GetString("dateTimeRssSearchPostAfter.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.dateTimeRssSearchPostAfter, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("dateTimeRssSearchPostAfter.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.dateTimeRssSearchPostAfter, resources.GetString("dateTimeRssSearchPostAfter.HelpString"));
			this.dateTimeRssSearchPostAfter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dateTimeRssSearchPostAfter.ImeMode")));
			this.dateTimeRssSearchPostAfter.Location = ((System.Drawing.Point)(resources.GetObject("dateTimeRssSearchPostAfter.Location")));
			this.dateTimeRssSearchPostAfter.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
			this.dateTimeRssSearchPostAfter.Name = "dateTimeRssSearchPostAfter";
			this.dateTimeRssSearchPostAfter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dateTimeRssSearchPostAfter.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.dateTimeRssSearchPostAfter, ((bool)(resources.GetObject("dateTimeRssSearchPostAfter.ShowHelp"))));
			this.dateTimeRssSearchPostAfter.Size = ((System.Drawing.Size)(resources.GetObject("dateTimeRssSearchPostAfter.Size")));
			this.dateTimeRssSearchPostAfter.TabIndex = ((int)(resources.GetObject("dateTimeRssSearchPostAfter.TabIndex")));
			this.toolTip.SetToolTip(this.dateTimeRssSearchPostAfter, resources.GetString("dateTimeRssSearchPostAfter.ToolTip"));
			this.dateTimeRssSearchPostAfter.Visible = ((bool)(resources.GetObject("dateTimeRssSearchPostAfter.Visible")));
			// 
			// dateTimeRssSearchPostBefore
			// 
			this.dateTimeRssSearchPostBefore.AccessibleDescription = resources.GetString("dateTimeRssSearchPostBefore.AccessibleDescription");
			this.dateTimeRssSearchPostBefore.AccessibleName = resources.GetString("dateTimeRssSearchPostBefore.AccessibleName");
			this.dateTimeRssSearchPostBefore.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dateTimeRssSearchPostBefore.Anchor")));
			this.dateTimeRssSearchPostBefore.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dateTimeRssSearchPostBefore.BackgroundImage")));
			this.dateTimeRssSearchPostBefore.CalendarFont = ((System.Drawing.Font)(resources.GetObject("dateTimeRssSearchPostBefore.CalendarFont")));
			this.dateTimeRssSearchPostBefore.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dateTimeRssSearchPostBefore.Dock")));
			this.dateTimeRssSearchPostBefore.DropDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("dateTimeRssSearchPostBefore.DropDownAlign")));
			this.dateTimeRssSearchPostBefore.Enabled = ((bool)(resources.GetObject("dateTimeRssSearchPostBefore.Enabled")));
			this.dateTimeRssSearchPostBefore.Font = ((System.Drawing.Font)(resources.GetObject("dateTimeRssSearchPostBefore.Font")));
			this.dateTimeRssSearchPostBefore.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.helpProvider1.SetHelpKeyword(this.dateTimeRssSearchPostBefore, resources.GetString("dateTimeRssSearchPostBefore.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.dateTimeRssSearchPostBefore, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("dateTimeRssSearchPostBefore.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.dateTimeRssSearchPostBefore, resources.GetString("dateTimeRssSearchPostBefore.HelpString"));
			this.dateTimeRssSearchPostBefore.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dateTimeRssSearchPostBefore.ImeMode")));
			this.dateTimeRssSearchPostBefore.Location = ((System.Drawing.Point)(resources.GetObject("dateTimeRssSearchPostBefore.Location")));
			this.dateTimeRssSearchPostBefore.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
			this.dateTimeRssSearchPostBefore.Name = "dateTimeRssSearchPostBefore";
			this.dateTimeRssSearchPostBefore.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dateTimeRssSearchPostBefore.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.dateTimeRssSearchPostBefore, ((bool)(resources.GetObject("dateTimeRssSearchPostBefore.ShowHelp"))));
			this.dateTimeRssSearchPostBefore.Size = ((System.Drawing.Size)(resources.GetObject("dateTimeRssSearchPostBefore.Size")));
			this.dateTimeRssSearchPostBefore.TabIndex = ((int)(resources.GetObject("dateTimeRssSearchPostBefore.TabIndex")));
			this.toolTip.SetToolTip(this.dateTimeRssSearchPostBefore, resources.GetString("dateTimeRssSearchPostBefore.ToolTip"));
			this.dateTimeRssSearchPostBefore.Visible = ((bool)(resources.GetObject("dateTimeRssSearchPostBefore.Visible")));
			// 
			// treeFeeds
			// 
			this.treeFeeds.AccessibleDescription = resources.GetString("treeFeeds.AccessibleDescription");
			this.treeFeeds.AccessibleName = resources.GetString("treeFeeds.AccessibleName");
			this.treeFeeds.AllowDrop = true;
			this.treeFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("treeFeeds.Anchor")));
			this.treeFeeds.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("treeFeeds.BackgroundImage")));
			this.treeFeeds.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("treeFeeds.Dock")));
			this.treeFeeds.Enabled = ((bool)(resources.GetObject("treeFeeds.Enabled")));
			this.treeFeeds.Font = ((System.Drawing.Font)(resources.GetObject("treeFeeds.Font")));
			this.helpProvider1.SetHelpKeyword(this.treeFeeds, resources.GetString("treeFeeds.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.treeFeeds, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("treeFeeds.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.treeFeeds, resources.GetString("treeFeeds.HelpString"));
			this.treeFeeds.HideSelection = false;
			this.treeFeeds.ImageIndex = ((int)(resources.GetObject("treeFeeds.ImageIndex")));
			this.treeFeeds.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("treeFeeds.ImeMode")));
			this.treeFeeds.Indent = ((int)(resources.GetObject("treeFeeds.Indent")));
			this.treeFeeds.ItemHeight = ((int)(resources.GetObject("treeFeeds.ItemHeight")));
			this.treeFeeds.LabelEdit = true;
			this.treeFeeds.Location = ((System.Drawing.Point)(resources.GetObject("treeFeeds.Location")));
			this.treeFeeds.Name = "treeFeeds";
			this.treeFeeds.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("treeFeeds.RightToLeft")));
			this.treeFeeds.SelectedImageIndex = ((int)(resources.GetObject("treeFeeds.SelectedImageIndex")));
			this.helpProvider1.SetShowHelp(this.treeFeeds, ((bool)(resources.GetObject("treeFeeds.ShowHelp"))));
			this.treeFeeds.Size = ((System.Drawing.Size)(resources.GetObject("treeFeeds.Size")));
			this.treeFeeds.Sorted = true;
			this.treeFeeds.TabIndex = ((int)(resources.GetObject("treeFeeds.TabIndex")));
			this.treeFeeds.Text = resources.GetString("treeFeeds.Text");
			this.toolTip.SetToolTip(this.treeFeeds, resources.GetString("treeFeeds.ToolTip"));
			this.treeFeeds.Visible = ((bool)(resources.GetObject("treeFeeds.Visible")));
			// 
			// panelRssSearch
			// 
			this.panelRssSearch.AccessibleDescription = resources.GetString("panelRssSearch.AccessibleDescription");
			this.panelRssSearch.AccessibleName = resources.GetString("panelRssSearch.AccessibleName");
			this.panelRssSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelRssSearch.Anchor")));
			this.panelRssSearch.AutoScroll = ((bool)(resources.GetObject("panelRssSearch.AutoScroll")));
			this.panelRssSearch.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelRssSearch.AutoScrollMargin")));
			this.panelRssSearch.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelRssSearch.AutoScrollMinSize")));
			this.panelRssSearch.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.panelRssSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelRssSearch.BackgroundImage")));
			this.panelRssSearch.Controls.Add(this.taskPaneSearchOptions);
			this.panelRssSearch.Controls.Add(this.searchPaneSplitter);
			this.panelRssSearch.Controls.Add(this.panelRssSearchCommands);
			this.panelRssSearch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelRssSearch.Dock")));
			this.panelRssSearch.Enabled = ((bool)(resources.GetObject("panelRssSearch.Enabled")));
			this.panelRssSearch.Font = ((System.Drawing.Font)(resources.GetObject("panelRssSearch.Font")));
			this.helpProvider1.SetHelpKeyword(this.panelRssSearch, resources.GetString("panelRssSearch.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.panelRssSearch, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("panelRssSearch.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.panelRssSearch, resources.GetString("panelRssSearch.HelpString"));
			this.panelRssSearch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelRssSearch.ImeMode")));
			this.panelRssSearch.Location = ((System.Drawing.Point)(resources.GetObject("panelRssSearch.Location")));
			this.panelRssSearch.Name = "panelRssSearch";
			this.panelRssSearch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelRssSearch.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.panelRssSearch, ((bool)(resources.GetObject("panelRssSearch.ShowHelp"))));
			this.panelRssSearch.Size = ((System.Drawing.Size)(resources.GetObject("panelRssSearch.Size")));
			this.panelRssSearch.TabIndex = ((int)(resources.GetObject("panelRssSearch.TabIndex")));
			this.panelRssSearch.Text = resources.GetString("panelRssSearch.Text");
			this.toolTip.SetToolTip(this.panelRssSearch, resources.GetString("panelRssSearch.ToolTip"));
			this.panelRssSearch.Visible = ((bool)(resources.GetObject("panelRssSearch.Visible")));
			// 
			// taskPaneSearchOptions
			// 
			this.taskPaneSearchOptions.AccessibleDescription = resources.GetString("taskPaneSearchOptions.AccessibleDescription");
			this.taskPaneSearchOptions.AccessibleName = resources.GetString("taskPaneSearchOptions.AccessibleName");
			this.taskPaneSearchOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("taskPaneSearchOptions.Anchor")));
			this.taskPaneSearchOptions.AutoScroll = ((bool)(resources.GetObject("taskPaneSearchOptions.AutoScroll")));
			this.taskPaneSearchOptions.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("taskPaneSearchOptions.AutoScrollMargin")));
			this.taskPaneSearchOptions.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("taskPaneSearchOptions.AutoScrollMinSize")));
			this.taskPaneSearchOptions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("taskPaneSearchOptions.Dock")));
			this.taskPaneSearchOptions.Enabled = ((bool)(resources.GetObject("taskPaneSearchOptions.Enabled")));
			this.taskPaneSearchOptions.Expandos.AddRange(new XPExplorerBar.Expando[] {
																						 this.collapsiblePanelSearchNameEx,
																						 this.collapsiblePanelRssSearchExprKindEx,
																						 this.collapsiblePanelItemPropertiesEx,
																						 this.collapsiblePanelAdvancedOptionsEx,
																						 this.collapsiblePanelRssSearchScopeEx});
			this.taskPaneSearchOptions.Font = ((System.Drawing.Font)(resources.GetObject("taskPaneSearchOptions.Font")));
			this.helpProvider1.SetHelpKeyword(this.taskPaneSearchOptions, resources.GetString("taskPaneSearchOptions.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.taskPaneSearchOptions, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("taskPaneSearchOptions.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.taskPaneSearchOptions, resources.GetString("taskPaneSearchOptions.HelpString"));
			this.taskPaneSearchOptions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("taskPaneSearchOptions.ImeMode")));
			this.taskPaneSearchOptions.Location = ((System.Drawing.Point)(resources.GetObject("taskPaneSearchOptions.Location")));
			this.taskPaneSearchOptions.Name = "taskPaneSearchOptions";
			this.taskPaneSearchOptions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("taskPaneSearchOptions.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.taskPaneSearchOptions, ((bool)(resources.GetObject("taskPaneSearchOptions.ShowHelp"))));
			this.taskPaneSearchOptions.Size = ((System.Drawing.Size)(resources.GetObject("taskPaneSearchOptions.Size")));
			this.taskPaneSearchOptions.TabIndex = ((int)(resources.GetObject("taskPaneSearchOptions.TabIndex")));
			this.taskPaneSearchOptions.Text = resources.GetString("taskPaneSearchOptions.Text");
			this.toolTip.SetToolTip(this.taskPaneSearchOptions, resources.GetString("taskPaneSearchOptions.ToolTip"));
			this.taskPaneSearchOptions.Visible = ((bool)(resources.GetObject("taskPaneSearchOptions.Visible")));
			// 
			// collapsiblePanelSearchNameEx
			// 
			this.collapsiblePanelSearchNameEx.AccessibleDescription = resources.GetString("collapsiblePanelSearchNameEx.AccessibleDescription");
			this.collapsiblePanelSearchNameEx.AccessibleName = resources.GetString("collapsiblePanelSearchNameEx.AccessibleName");
			this.collapsiblePanelSearchNameEx.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("collapsiblePanelSearchNameEx.Anchor")));
			this.collapsiblePanelSearchNameEx.Animate = true;
			this.collapsiblePanelSearchNameEx.Collapsed = true;
			this.collapsiblePanelSearchNameEx.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("collapsiblePanelSearchNameEx.Dock")));
			this.collapsiblePanelSearchNameEx.Enabled = ((bool)(resources.GetObject("collapsiblePanelSearchNameEx.Enabled")));
			this.collapsiblePanelSearchNameEx.ExpandedHeight = 170;
			this.collapsiblePanelSearchNameEx.Font = ((System.Drawing.Font)(resources.GetObject("collapsiblePanelSearchNameEx.Font")));
			this.helpProvider1.SetHelpKeyword(this.collapsiblePanelSearchNameEx, resources.GetString("collapsiblePanelSearchNameEx.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.collapsiblePanelSearchNameEx, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("collapsiblePanelSearchNameEx.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.collapsiblePanelSearchNameEx, resources.GetString("collapsiblePanelSearchNameEx.HelpString"));
			this.collapsiblePanelSearchNameEx.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("collapsiblePanelSearchNameEx.ImeMode")));
			this.collapsiblePanelSearchNameEx.Items.AddRange(new System.Windows.Forms.Control[] {
																									this.btnRssSearchSave,
																									this.labelSearchFolderNameHint,
																									this.textFinderCaption});
			this.collapsiblePanelSearchNameEx.Location = ((System.Drawing.Point)(resources.GetObject("collapsiblePanelSearchNameEx.Location")));
			this.collapsiblePanelSearchNameEx.Name = "collapsiblePanelSearchNameEx";
			this.collapsiblePanelSearchNameEx.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("collapsiblePanelSearchNameEx.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.collapsiblePanelSearchNameEx, ((bool)(resources.GetObject("collapsiblePanelSearchNameEx.ShowHelp"))));
			this.collapsiblePanelSearchNameEx.Size = ((System.Drawing.Size)(resources.GetObject("collapsiblePanelSearchNameEx.Size")));
			this.collapsiblePanelSearchNameEx.SpecialGroup = true;
			this.collapsiblePanelSearchNameEx.TabIndex = ((int)(resources.GetObject("collapsiblePanelSearchNameEx.TabIndex")));
			this.collapsiblePanelSearchNameEx.Text = resources.GetString("collapsiblePanelSearchNameEx.Text");
			this.toolTip.SetToolTip(this.collapsiblePanelSearchNameEx, resources.GetString("collapsiblePanelSearchNameEx.ToolTip"));
			this.collapsiblePanelSearchNameEx.Visible = ((bool)(resources.GetObject("collapsiblePanelSearchNameEx.Visible")));
			// 
			// btnRssSearchSave
			// 
			this.btnRssSearchSave.AccessibleDescription = resources.GetString("btnRssSearchSave.AccessibleDescription");
			this.btnRssSearchSave.AccessibleName = resources.GetString("btnRssSearchSave.AccessibleName");
			this.btnRssSearchSave.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnRssSearchSave.Anchor")));
			this.btnRssSearchSave.BackColor = System.Drawing.SystemColors.Control;
			this.btnRssSearchSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRssSearchSave.BackgroundImage")));
			this.btnRssSearchSave.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnRssSearchSave.Dock")));
			this.btnRssSearchSave.Enabled = ((bool)(resources.GetObject("btnRssSearchSave.Enabled")));
			this.btnRssSearchSave.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnRssSearchSave.FlatStyle")));
			this.btnRssSearchSave.Font = ((System.Drawing.Font)(resources.GetObject("btnRssSearchSave.Font")));
			this.helpProvider1.SetHelpKeyword(this.btnRssSearchSave, resources.GetString("btnRssSearchSave.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.btnRssSearchSave, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnRssSearchSave.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.btnRssSearchSave, resources.GetString("btnRssSearchSave.HelpString"));
			this.btnRssSearchSave.Image = ((System.Drawing.Image)(resources.GetObject("btnRssSearchSave.Image")));
			this.btnRssSearchSave.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnRssSearchSave.ImageAlign")));
			this.btnRssSearchSave.ImageIndex = ((int)(resources.GetObject("btnRssSearchSave.ImageIndex")));
			this.btnRssSearchSave.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnRssSearchSave.ImeMode")));
			this.btnRssSearchSave.Location = ((System.Drawing.Point)(resources.GetObject("btnRssSearchSave.Location")));
			this.btnRssSearchSave.Name = "btnRssSearchSave";
			this.btnRssSearchSave.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnRssSearchSave.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.btnRssSearchSave, ((bool)(resources.GetObject("btnRssSearchSave.ShowHelp"))));
			this.btnRssSearchSave.Size = ((System.Drawing.Size)(resources.GetObject("btnRssSearchSave.Size")));
			this.btnRssSearchSave.TabIndex = ((int)(resources.GetObject("btnRssSearchSave.TabIndex")));
			this.btnRssSearchSave.Text = resources.GetString("btnRssSearchSave.Text");
			this.btnRssSearchSave.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnRssSearchSave.TextAlign")));
			this.toolTip.SetToolTip(this.btnRssSearchSave, resources.GetString("btnRssSearchSave.ToolTip"));
			this.btnRssSearchSave.Visible = ((bool)(resources.GetObject("btnRssSearchSave.Visible")));
			// 
			// labelSearchFolderNameHint
			// 
			this.labelSearchFolderNameHint.AccessibleDescription = resources.GetString("labelSearchFolderNameHint.AccessibleDescription");
			this.labelSearchFolderNameHint.AccessibleName = resources.GetString("labelSearchFolderNameHint.AccessibleName");
			this.labelSearchFolderNameHint.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelSearchFolderNameHint.Anchor")));
			this.labelSearchFolderNameHint.AutoSize = ((bool)(resources.GetObject("labelSearchFolderNameHint.AutoSize")));
			this.labelSearchFolderNameHint.BackColor = System.Drawing.Color.Transparent;
			this.labelSearchFolderNameHint.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelSearchFolderNameHint.Dock")));
			this.labelSearchFolderNameHint.Enabled = ((bool)(resources.GetObject("labelSearchFolderNameHint.Enabled")));
			this.labelSearchFolderNameHint.Font = ((System.Drawing.Font)(resources.GetObject("labelSearchFolderNameHint.Font")));
			this.helpProvider1.SetHelpKeyword(this.labelSearchFolderNameHint, resources.GetString("labelSearchFolderNameHint.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.labelSearchFolderNameHint, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("labelSearchFolderNameHint.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.labelSearchFolderNameHint, resources.GetString("labelSearchFolderNameHint.HelpString"));
			this.labelSearchFolderNameHint.Image = ((System.Drawing.Image)(resources.GetObject("labelSearchFolderNameHint.Image")));
			this.labelSearchFolderNameHint.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelSearchFolderNameHint.ImageAlign")));
			this.labelSearchFolderNameHint.ImageIndex = ((int)(resources.GetObject("labelSearchFolderNameHint.ImageIndex")));
			this.labelSearchFolderNameHint.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelSearchFolderNameHint.ImeMode")));
			this.labelSearchFolderNameHint.Location = ((System.Drawing.Point)(resources.GetObject("labelSearchFolderNameHint.Location")));
			this.labelSearchFolderNameHint.Name = "labelSearchFolderNameHint";
			this.labelSearchFolderNameHint.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelSearchFolderNameHint.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.labelSearchFolderNameHint, ((bool)(resources.GetObject("labelSearchFolderNameHint.ShowHelp"))));
			this.labelSearchFolderNameHint.Size = ((System.Drawing.Size)(resources.GetObject("labelSearchFolderNameHint.Size")));
			this.labelSearchFolderNameHint.TabIndex = ((int)(resources.GetObject("labelSearchFolderNameHint.TabIndex")));
			this.labelSearchFolderNameHint.Text = resources.GetString("labelSearchFolderNameHint.Text");
			this.labelSearchFolderNameHint.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelSearchFolderNameHint.TextAlign")));
			this.toolTip.SetToolTip(this.labelSearchFolderNameHint, resources.GetString("labelSearchFolderNameHint.ToolTip"));
			this.labelSearchFolderNameHint.Visible = ((bool)(resources.GetObject("labelSearchFolderNameHint.Visible")));
			// 
			// textFinderCaption
			// 
			this.textFinderCaption.AccessibleDescription = resources.GetString("textFinderCaption.AccessibleDescription");
			this.textFinderCaption.AccessibleName = resources.GetString("textFinderCaption.AccessibleName");
			this.textFinderCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textFinderCaption.Anchor")));
			this.textFinderCaption.AutoSize = ((bool)(resources.GetObject("textFinderCaption.AutoSize")));
			this.textFinderCaption.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textFinderCaption.BackgroundImage")));
			this.textFinderCaption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textFinderCaption.Dock")));
			this.textFinderCaption.Enabled = ((bool)(resources.GetObject("textFinderCaption.Enabled")));
			this.textFinderCaption.Font = ((System.Drawing.Font)(resources.GetObject("textFinderCaption.Font")));
			this.helpProvider1.SetHelpKeyword(this.textFinderCaption, resources.GetString("textFinderCaption.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.textFinderCaption, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("textFinderCaption.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.textFinderCaption, resources.GetString("textFinderCaption.HelpString"));
			this.textFinderCaption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textFinderCaption.ImeMode")));
			this.textFinderCaption.Location = ((System.Drawing.Point)(resources.GetObject("textFinderCaption.Location")));
			this.textFinderCaption.MaxLength = ((int)(resources.GetObject("textFinderCaption.MaxLength")));
			this.textFinderCaption.Multiline = ((bool)(resources.GetObject("textFinderCaption.Multiline")));
			this.textFinderCaption.Name = "textFinderCaption";
			this.textFinderCaption.PasswordChar = ((char)(resources.GetObject("textFinderCaption.PasswordChar")));
			this.textFinderCaption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textFinderCaption.RightToLeft")));
			this.textFinderCaption.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textFinderCaption.ScrollBars")));
			this.helpProvider1.SetShowHelp(this.textFinderCaption, ((bool)(resources.GetObject("textFinderCaption.ShowHelp"))));
			this.textFinderCaption.Size = ((System.Drawing.Size)(resources.GetObject("textFinderCaption.Size")));
			this.textFinderCaption.TabIndex = ((int)(resources.GetObject("textFinderCaption.TabIndex")));
			this.textFinderCaption.Text = resources.GetString("textFinderCaption.Text");
			this.textFinderCaption.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textFinderCaption.TextAlign")));
			this.toolTip.SetToolTip(this.textFinderCaption, resources.GetString("textFinderCaption.ToolTip"));
			this.textFinderCaption.Visible = ((bool)(resources.GetObject("textFinderCaption.Visible")));
			this.textFinderCaption.WordWrap = ((bool)(resources.GetObject("textFinderCaption.WordWrap")));
			// 
			// collapsiblePanelRssSearchExprKindEx
			// 
			this.collapsiblePanelRssSearchExprKindEx.AccessibleDescription = resources.GetString("collapsiblePanelRssSearchExprKindEx.AccessibleDescription");
			this.collapsiblePanelRssSearchExprKindEx.AccessibleName = resources.GetString("collapsiblePanelRssSearchExprKindEx.AccessibleName");
			this.collapsiblePanelRssSearchExprKindEx.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.Anchor")));
			this.collapsiblePanelRssSearchExprKindEx.Animate = true;
			this.collapsiblePanelRssSearchExprKindEx.Collapsed = true;
			this.collapsiblePanelRssSearchExprKindEx.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.Dock")));
			this.collapsiblePanelRssSearchExprKindEx.Enabled = ((bool)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.Enabled")));
			this.collapsiblePanelRssSearchExprKindEx.ExpandedHeight = 140;
			this.collapsiblePanelRssSearchExprKindEx.Font = ((System.Drawing.Font)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.Font")));
			this.helpProvider1.SetHelpKeyword(this.collapsiblePanelRssSearchExprKindEx, resources.GetString("collapsiblePanelRssSearchExprKindEx.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.collapsiblePanelRssSearchExprKindEx, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.collapsiblePanelRssSearchExprKindEx, resources.GetString("collapsiblePanelRssSearchExprKindEx.HelpString"));
			this.collapsiblePanelRssSearchExprKindEx.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.ImeMode")));
			this.collapsiblePanelRssSearchExprKindEx.Items.AddRange(new System.Windows.Forms.Control[] {
																										   this.radioRssSearchSimpleText,
																										   this.labelRssSearchTypeHint,
																										   this.radioRssSearchExprXPath,
																										   this.radioRssSearchRegEx});
			this.collapsiblePanelRssSearchExprKindEx.Location = ((System.Drawing.Point)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.Location")));
			this.collapsiblePanelRssSearchExprKindEx.Name = "collapsiblePanelRssSearchExprKindEx";
			this.collapsiblePanelRssSearchExprKindEx.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.collapsiblePanelRssSearchExprKindEx, ((bool)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.ShowHelp"))));
			this.collapsiblePanelRssSearchExprKindEx.Size = ((System.Drawing.Size)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.Size")));
			this.collapsiblePanelRssSearchExprKindEx.TabIndex = ((int)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.TabIndex")));
			this.collapsiblePanelRssSearchExprKindEx.Text = resources.GetString("collapsiblePanelRssSearchExprKindEx.Text");
			this.toolTip.SetToolTip(this.collapsiblePanelRssSearchExprKindEx, resources.GetString("collapsiblePanelRssSearchExprKindEx.ToolTip"));
			this.collapsiblePanelRssSearchExprKindEx.Visible = ((bool)(resources.GetObject("collapsiblePanelRssSearchExprKindEx.Visible")));
			// 
			// radioRssSearchSimpleText
			// 
			this.radioRssSearchSimpleText.AccessibleDescription = resources.GetString("radioRssSearchSimpleText.AccessibleDescription");
			this.radioRssSearchSimpleText.AccessibleName = resources.GetString("radioRssSearchSimpleText.AccessibleName");
			this.radioRssSearchSimpleText.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioRssSearchSimpleText.Anchor")));
			this.radioRssSearchSimpleText.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioRssSearchSimpleText.Appearance")));
			this.radioRssSearchSimpleText.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioRssSearchSimpleText.BackgroundImage")));
			this.radioRssSearchSimpleText.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchSimpleText.CheckAlign")));
			this.radioRssSearchSimpleText.Checked = true;
			this.radioRssSearchSimpleText.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioRssSearchSimpleText.Dock")));
			this.radioRssSearchSimpleText.Enabled = ((bool)(resources.GetObject("radioRssSearchSimpleText.Enabled")));
			this.radioRssSearchSimpleText.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioRssSearchSimpleText.FlatStyle")));
			this.radioRssSearchSimpleText.Font = ((System.Drawing.Font)(resources.GetObject("radioRssSearchSimpleText.Font")));
			this.helpProvider1.SetHelpKeyword(this.radioRssSearchSimpleText, resources.GetString("radioRssSearchSimpleText.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.radioRssSearchSimpleText, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("radioRssSearchSimpleText.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.radioRssSearchSimpleText, resources.GetString("radioRssSearchSimpleText.HelpString"));
			this.radioRssSearchSimpleText.Image = ((System.Drawing.Image)(resources.GetObject("radioRssSearchSimpleText.Image")));
			this.radioRssSearchSimpleText.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchSimpleText.ImageAlign")));
			this.radioRssSearchSimpleText.ImageIndex = ((int)(resources.GetObject("radioRssSearchSimpleText.ImageIndex")));
			this.radioRssSearchSimpleText.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioRssSearchSimpleText.ImeMode")));
			this.radioRssSearchSimpleText.Location = ((System.Drawing.Point)(resources.GetObject("radioRssSearchSimpleText.Location")));
			this.radioRssSearchSimpleText.Name = "radioRssSearchSimpleText";
			this.radioRssSearchSimpleText.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioRssSearchSimpleText.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.radioRssSearchSimpleText, ((bool)(resources.GetObject("radioRssSearchSimpleText.ShowHelp"))));
			this.radioRssSearchSimpleText.Size = ((System.Drawing.Size)(resources.GetObject("radioRssSearchSimpleText.Size")));
			this.radioRssSearchSimpleText.TabIndex = ((int)(resources.GetObject("radioRssSearchSimpleText.TabIndex")));
			this.radioRssSearchSimpleText.TabStop = true;
			this.radioRssSearchSimpleText.Text = resources.GetString("radioRssSearchSimpleText.Text");
			this.radioRssSearchSimpleText.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchSimpleText.TextAlign")));
			this.toolTip.SetToolTip(this.radioRssSearchSimpleText, resources.GetString("radioRssSearchSimpleText.ToolTip"));
			this.radioRssSearchSimpleText.Visible = ((bool)(resources.GetObject("radioRssSearchSimpleText.Visible")));
			// 
			// labelRssSearchTypeHint
			// 
			this.labelRssSearchTypeHint.AccessibleDescription = resources.GetString("labelRssSearchTypeHint.AccessibleDescription");
			this.labelRssSearchTypeHint.AccessibleName = resources.GetString("labelRssSearchTypeHint.AccessibleName");
			this.labelRssSearchTypeHint.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelRssSearchTypeHint.Anchor")));
			this.labelRssSearchTypeHint.AutoSize = ((bool)(resources.GetObject("labelRssSearchTypeHint.AutoSize")));
			this.labelRssSearchTypeHint.BackColor = System.Drawing.Color.Transparent;
			this.labelRssSearchTypeHint.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelRssSearchTypeHint.Dock")));
			this.labelRssSearchTypeHint.Enabled = ((bool)(resources.GetObject("labelRssSearchTypeHint.Enabled")));
			this.labelRssSearchTypeHint.Font = ((System.Drawing.Font)(resources.GetObject("labelRssSearchTypeHint.Font")));
			this.helpProvider1.SetHelpKeyword(this.labelRssSearchTypeHint, resources.GetString("labelRssSearchTypeHint.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.labelRssSearchTypeHint, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("labelRssSearchTypeHint.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.labelRssSearchTypeHint, resources.GetString("labelRssSearchTypeHint.HelpString"));
			this.labelRssSearchTypeHint.Image = ((System.Drawing.Image)(resources.GetObject("labelRssSearchTypeHint.Image")));
			this.labelRssSearchTypeHint.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRssSearchTypeHint.ImageAlign")));
			this.labelRssSearchTypeHint.ImageIndex = ((int)(resources.GetObject("labelRssSearchTypeHint.ImageIndex")));
			this.labelRssSearchTypeHint.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelRssSearchTypeHint.ImeMode")));
			this.labelRssSearchTypeHint.Location = ((System.Drawing.Point)(resources.GetObject("labelRssSearchTypeHint.Location")));
			this.labelRssSearchTypeHint.Name = "labelRssSearchTypeHint";
			this.labelRssSearchTypeHint.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelRssSearchTypeHint.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.labelRssSearchTypeHint, ((bool)(resources.GetObject("labelRssSearchTypeHint.ShowHelp"))));
			this.labelRssSearchTypeHint.Size = ((System.Drawing.Size)(resources.GetObject("labelRssSearchTypeHint.Size")));
			this.labelRssSearchTypeHint.TabIndex = ((int)(resources.GetObject("labelRssSearchTypeHint.TabIndex")));
			this.labelRssSearchTypeHint.Text = resources.GetString("labelRssSearchTypeHint.Text");
			this.labelRssSearchTypeHint.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRssSearchTypeHint.TextAlign")));
			this.toolTip.SetToolTip(this.labelRssSearchTypeHint, resources.GetString("labelRssSearchTypeHint.ToolTip"));
			this.labelRssSearchTypeHint.Visible = ((bool)(resources.GetObject("labelRssSearchTypeHint.Visible")));
			// 
			// radioRssSearchExprXPath
			// 
			this.radioRssSearchExprXPath.AccessibleDescription = resources.GetString("radioRssSearchExprXPath.AccessibleDescription");
			this.radioRssSearchExprXPath.AccessibleName = resources.GetString("radioRssSearchExprXPath.AccessibleName");
			this.radioRssSearchExprXPath.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioRssSearchExprXPath.Anchor")));
			this.radioRssSearchExprXPath.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioRssSearchExprXPath.Appearance")));
			this.radioRssSearchExprXPath.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioRssSearchExprXPath.BackgroundImage")));
			this.radioRssSearchExprXPath.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchExprXPath.CheckAlign")));
			this.radioRssSearchExprXPath.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioRssSearchExprXPath.Dock")));
			this.radioRssSearchExprXPath.Enabled = ((bool)(resources.GetObject("radioRssSearchExprXPath.Enabled")));
			this.radioRssSearchExprXPath.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioRssSearchExprXPath.FlatStyle")));
			this.radioRssSearchExprXPath.Font = ((System.Drawing.Font)(resources.GetObject("radioRssSearchExprXPath.Font")));
			this.helpProvider1.SetHelpKeyword(this.radioRssSearchExprXPath, resources.GetString("radioRssSearchExprXPath.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.radioRssSearchExprXPath, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("radioRssSearchExprXPath.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.radioRssSearchExprXPath, resources.GetString("radioRssSearchExprXPath.HelpString"));
			this.radioRssSearchExprXPath.Image = ((System.Drawing.Image)(resources.GetObject("radioRssSearchExprXPath.Image")));
			this.radioRssSearchExprXPath.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchExprXPath.ImageAlign")));
			this.radioRssSearchExprXPath.ImageIndex = ((int)(resources.GetObject("radioRssSearchExprXPath.ImageIndex")));
			this.radioRssSearchExprXPath.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioRssSearchExprXPath.ImeMode")));
			this.radioRssSearchExprXPath.Location = ((System.Drawing.Point)(resources.GetObject("radioRssSearchExprXPath.Location")));
			this.radioRssSearchExprXPath.Name = "radioRssSearchExprXPath";
			this.radioRssSearchExprXPath.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioRssSearchExprXPath.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.radioRssSearchExprXPath, ((bool)(resources.GetObject("radioRssSearchExprXPath.ShowHelp"))));
			this.radioRssSearchExprXPath.Size = ((System.Drawing.Size)(resources.GetObject("radioRssSearchExprXPath.Size")));
			this.radioRssSearchExprXPath.TabIndex = ((int)(resources.GetObject("radioRssSearchExprXPath.TabIndex")));
			this.radioRssSearchExprXPath.Text = resources.GetString("radioRssSearchExprXPath.Text");
			this.radioRssSearchExprXPath.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchExprXPath.TextAlign")));
			this.toolTip.SetToolTip(this.radioRssSearchExprXPath, resources.GetString("radioRssSearchExprXPath.ToolTip"));
			this.radioRssSearchExprXPath.Visible = ((bool)(resources.GetObject("radioRssSearchExprXPath.Visible")));
			// 
			// radioRssSearchRegEx
			// 
			this.radioRssSearchRegEx.AccessibleDescription = resources.GetString("radioRssSearchRegEx.AccessibleDescription");
			this.radioRssSearchRegEx.AccessibleName = resources.GetString("radioRssSearchRegEx.AccessibleName");
			this.radioRssSearchRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioRssSearchRegEx.Anchor")));
			this.radioRssSearchRegEx.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioRssSearchRegEx.Appearance")));
			this.radioRssSearchRegEx.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioRssSearchRegEx.BackgroundImage")));
			this.radioRssSearchRegEx.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchRegEx.CheckAlign")));
			this.radioRssSearchRegEx.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioRssSearchRegEx.Dock")));
			this.radioRssSearchRegEx.Enabled = ((bool)(resources.GetObject("radioRssSearchRegEx.Enabled")));
			this.radioRssSearchRegEx.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioRssSearchRegEx.FlatStyle")));
			this.radioRssSearchRegEx.Font = ((System.Drawing.Font)(resources.GetObject("radioRssSearchRegEx.Font")));
			this.helpProvider1.SetHelpKeyword(this.radioRssSearchRegEx, resources.GetString("radioRssSearchRegEx.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.radioRssSearchRegEx, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("radioRssSearchRegEx.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.radioRssSearchRegEx, resources.GetString("radioRssSearchRegEx.HelpString"));
			this.radioRssSearchRegEx.Image = ((System.Drawing.Image)(resources.GetObject("radioRssSearchRegEx.Image")));
			this.radioRssSearchRegEx.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchRegEx.ImageAlign")));
			this.radioRssSearchRegEx.ImageIndex = ((int)(resources.GetObject("radioRssSearchRegEx.ImageIndex")));
			this.radioRssSearchRegEx.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioRssSearchRegEx.ImeMode")));
			this.radioRssSearchRegEx.Location = ((System.Drawing.Point)(resources.GetObject("radioRssSearchRegEx.Location")));
			this.radioRssSearchRegEx.Name = "radioRssSearchRegEx";
			this.radioRssSearchRegEx.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioRssSearchRegEx.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.radioRssSearchRegEx, ((bool)(resources.GetObject("radioRssSearchRegEx.ShowHelp"))));
			this.radioRssSearchRegEx.Size = ((System.Drawing.Size)(resources.GetObject("radioRssSearchRegEx.Size")));
			this.radioRssSearchRegEx.TabIndex = ((int)(resources.GetObject("radioRssSearchRegEx.TabIndex")));
			this.radioRssSearchRegEx.Text = resources.GetString("radioRssSearchRegEx.Text");
			this.radioRssSearchRegEx.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchRegEx.TextAlign")));
			this.toolTip.SetToolTip(this.radioRssSearchRegEx, resources.GetString("radioRssSearchRegEx.ToolTip"));
			this.radioRssSearchRegEx.Visible = ((bool)(resources.GetObject("radioRssSearchRegEx.Visible")));
			// 
			// collapsiblePanelItemPropertiesEx
			// 
			this.collapsiblePanelItemPropertiesEx.AccessibleDescription = resources.GetString("collapsiblePanelItemPropertiesEx.AccessibleDescription");
			this.collapsiblePanelItemPropertiesEx.AccessibleName = resources.GetString("collapsiblePanelItemPropertiesEx.AccessibleName");
			this.collapsiblePanelItemPropertiesEx.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("collapsiblePanelItemPropertiesEx.Anchor")));
			this.collapsiblePanelItemPropertiesEx.Animate = true;
			this.collapsiblePanelItemPropertiesEx.Collapsed = true;
			this.collapsiblePanelItemPropertiesEx.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("collapsiblePanelItemPropertiesEx.Dock")));
			this.collapsiblePanelItemPropertiesEx.Enabled = ((bool)(resources.GetObject("collapsiblePanelItemPropertiesEx.Enabled")));
			this.collapsiblePanelItemPropertiesEx.ExpandedHeight = 160;
			this.collapsiblePanelItemPropertiesEx.Font = ((System.Drawing.Font)(resources.GetObject("collapsiblePanelItemPropertiesEx.Font")));
			this.helpProvider1.SetHelpKeyword(this.collapsiblePanelItemPropertiesEx, resources.GetString("collapsiblePanelItemPropertiesEx.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.collapsiblePanelItemPropertiesEx, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("collapsiblePanelItemPropertiesEx.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.collapsiblePanelItemPropertiesEx, resources.GetString("collapsiblePanelItemPropertiesEx.HelpString"));
			this.collapsiblePanelItemPropertiesEx.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("collapsiblePanelItemPropertiesEx.ImeMode")));
			this.collapsiblePanelItemPropertiesEx.Items.AddRange(new System.Windows.Forms.Control[] {
																										this.checkBoxRssSearchInDesc,
																										this.checkBoxRssSearchInCategory,
																										this.checkBoxRssSearchInTitle,
																										this.checkBoxRssSearchInLink,
																										this.label1});
			this.collapsiblePanelItemPropertiesEx.Location = ((System.Drawing.Point)(resources.GetObject("collapsiblePanelItemPropertiesEx.Location")));
			this.collapsiblePanelItemPropertiesEx.Name = "collapsiblePanelItemPropertiesEx";
			this.collapsiblePanelItemPropertiesEx.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("collapsiblePanelItemPropertiesEx.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.collapsiblePanelItemPropertiesEx, ((bool)(resources.GetObject("collapsiblePanelItemPropertiesEx.ShowHelp"))));
			this.collapsiblePanelItemPropertiesEx.Size = ((System.Drawing.Size)(resources.GetObject("collapsiblePanelItemPropertiesEx.Size")));
			this.collapsiblePanelItemPropertiesEx.TabIndex = ((int)(resources.GetObject("collapsiblePanelItemPropertiesEx.TabIndex")));
			this.collapsiblePanelItemPropertiesEx.Text = resources.GetString("collapsiblePanelItemPropertiesEx.Text");
			this.toolTip.SetToolTip(this.collapsiblePanelItemPropertiesEx, resources.GetString("collapsiblePanelItemPropertiesEx.ToolTip"));
			this.collapsiblePanelItemPropertiesEx.Visible = ((bool)(resources.GetObject("collapsiblePanelItemPropertiesEx.Visible")));
			// 
			// checkBoxRssSearchInDesc
			// 
			this.checkBoxRssSearchInDesc.AccessibleDescription = resources.GetString("checkBoxRssSearchInDesc.AccessibleDescription");
			this.checkBoxRssSearchInDesc.AccessibleName = resources.GetString("checkBoxRssSearchInDesc.AccessibleName");
			this.checkBoxRssSearchInDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchInDesc.Anchor")));
			this.checkBoxRssSearchInDesc.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchInDesc.Appearance")));
			this.checkBoxRssSearchInDesc.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInDesc.BackgroundImage")));
			this.checkBoxRssSearchInDesc.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInDesc.CheckAlign")));
			this.checkBoxRssSearchInDesc.Checked = true;
			this.checkBoxRssSearchInDesc.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxRssSearchInDesc.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchInDesc.Dock")));
			this.checkBoxRssSearchInDesc.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchInDesc.Enabled")));
			this.checkBoxRssSearchInDesc.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchInDesc.FlatStyle")));
			this.checkBoxRssSearchInDesc.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchInDesc.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchInDesc, resources.GetString("checkBoxRssSearchInDesc.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchInDesc, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchInDesc.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchInDesc, resources.GetString("checkBoxRssSearchInDesc.HelpString"));
			this.checkBoxRssSearchInDesc.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInDesc.Image")));
			this.checkBoxRssSearchInDesc.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInDesc.ImageAlign")));
			this.checkBoxRssSearchInDesc.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchInDesc.ImageIndex")));
			this.checkBoxRssSearchInDesc.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchInDesc.ImeMode")));
			this.checkBoxRssSearchInDesc.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchInDesc.Location")));
			this.checkBoxRssSearchInDesc.Name = "checkBoxRssSearchInDesc";
			this.checkBoxRssSearchInDesc.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchInDesc.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchInDesc, ((bool)(resources.GetObject("checkBoxRssSearchInDesc.ShowHelp"))));
			this.checkBoxRssSearchInDesc.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchInDesc.Size")));
			this.checkBoxRssSearchInDesc.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchInDesc.TabIndex")));
			this.checkBoxRssSearchInDesc.Text = resources.GetString("checkBoxRssSearchInDesc.Text");
			this.checkBoxRssSearchInDesc.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInDesc.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchInDesc, resources.GetString("checkBoxRssSearchInDesc.ToolTip"));
			this.checkBoxRssSearchInDesc.Visible = ((bool)(resources.GetObject("checkBoxRssSearchInDesc.Visible")));
			// 
			// checkBoxRssSearchInCategory
			// 
			this.checkBoxRssSearchInCategory.AccessibleDescription = resources.GetString("checkBoxRssSearchInCategory.AccessibleDescription");
			this.checkBoxRssSearchInCategory.AccessibleName = resources.GetString("checkBoxRssSearchInCategory.AccessibleName");
			this.checkBoxRssSearchInCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchInCategory.Anchor")));
			this.checkBoxRssSearchInCategory.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchInCategory.Appearance")));
			this.checkBoxRssSearchInCategory.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInCategory.BackgroundImage")));
			this.checkBoxRssSearchInCategory.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInCategory.CheckAlign")));
			this.checkBoxRssSearchInCategory.Checked = true;
			this.checkBoxRssSearchInCategory.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxRssSearchInCategory.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchInCategory.Dock")));
			this.checkBoxRssSearchInCategory.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchInCategory.Enabled")));
			this.checkBoxRssSearchInCategory.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchInCategory.FlatStyle")));
			this.checkBoxRssSearchInCategory.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchInCategory.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchInCategory, resources.GetString("checkBoxRssSearchInCategory.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchInCategory, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchInCategory.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchInCategory, resources.GetString("checkBoxRssSearchInCategory.HelpString"));
			this.checkBoxRssSearchInCategory.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInCategory.Image")));
			this.checkBoxRssSearchInCategory.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInCategory.ImageAlign")));
			this.checkBoxRssSearchInCategory.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchInCategory.ImageIndex")));
			this.checkBoxRssSearchInCategory.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchInCategory.ImeMode")));
			this.checkBoxRssSearchInCategory.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchInCategory.Location")));
			this.checkBoxRssSearchInCategory.Name = "checkBoxRssSearchInCategory";
			this.checkBoxRssSearchInCategory.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchInCategory.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchInCategory, ((bool)(resources.GetObject("checkBoxRssSearchInCategory.ShowHelp"))));
			this.checkBoxRssSearchInCategory.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchInCategory.Size")));
			this.checkBoxRssSearchInCategory.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchInCategory.TabIndex")));
			this.checkBoxRssSearchInCategory.Text = resources.GetString("checkBoxRssSearchInCategory.Text");
			this.checkBoxRssSearchInCategory.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInCategory.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchInCategory, resources.GetString("checkBoxRssSearchInCategory.ToolTip"));
			this.checkBoxRssSearchInCategory.Visible = ((bool)(resources.GetObject("checkBoxRssSearchInCategory.Visible")));
			// 
			// checkBoxRssSearchInTitle
			// 
			this.checkBoxRssSearchInTitle.AccessibleDescription = resources.GetString("checkBoxRssSearchInTitle.AccessibleDescription");
			this.checkBoxRssSearchInTitle.AccessibleName = resources.GetString("checkBoxRssSearchInTitle.AccessibleName");
			this.checkBoxRssSearchInTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchInTitle.Anchor")));
			this.checkBoxRssSearchInTitle.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchInTitle.Appearance")));
			this.checkBoxRssSearchInTitle.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInTitle.BackgroundImage")));
			this.checkBoxRssSearchInTitle.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInTitle.CheckAlign")));
			this.checkBoxRssSearchInTitle.Checked = true;
			this.checkBoxRssSearchInTitle.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxRssSearchInTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchInTitle.Dock")));
			this.checkBoxRssSearchInTitle.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchInTitle.Enabled")));
			this.checkBoxRssSearchInTitle.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchInTitle.FlatStyle")));
			this.checkBoxRssSearchInTitle.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchInTitle.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchInTitle, resources.GetString("checkBoxRssSearchInTitle.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchInTitle, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchInTitle.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchInTitle, resources.GetString("checkBoxRssSearchInTitle.HelpString"));
			this.checkBoxRssSearchInTitle.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInTitle.Image")));
			this.checkBoxRssSearchInTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInTitle.ImageAlign")));
			this.checkBoxRssSearchInTitle.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchInTitle.ImageIndex")));
			this.checkBoxRssSearchInTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchInTitle.ImeMode")));
			this.checkBoxRssSearchInTitle.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchInTitle.Location")));
			this.checkBoxRssSearchInTitle.Name = "checkBoxRssSearchInTitle";
			this.checkBoxRssSearchInTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchInTitle.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchInTitle, ((bool)(resources.GetObject("checkBoxRssSearchInTitle.ShowHelp"))));
			this.checkBoxRssSearchInTitle.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchInTitle.Size")));
			this.checkBoxRssSearchInTitle.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchInTitle.TabIndex")));
			this.checkBoxRssSearchInTitle.Text = resources.GetString("checkBoxRssSearchInTitle.Text");
			this.checkBoxRssSearchInTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInTitle.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchInTitle, resources.GetString("checkBoxRssSearchInTitle.ToolTip"));
			this.checkBoxRssSearchInTitle.Visible = ((bool)(resources.GetObject("checkBoxRssSearchInTitle.Visible")));
			// 
			// checkBoxRssSearchInLink
			// 
			this.checkBoxRssSearchInLink.AccessibleDescription = resources.GetString("checkBoxRssSearchInLink.AccessibleDescription");
			this.checkBoxRssSearchInLink.AccessibleName = resources.GetString("checkBoxRssSearchInLink.AccessibleName");
			this.checkBoxRssSearchInLink.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchInLink.Anchor")));
			this.checkBoxRssSearchInLink.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchInLink.Appearance")));
			this.checkBoxRssSearchInLink.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInLink.BackgroundImage")));
			this.checkBoxRssSearchInLink.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInLink.CheckAlign")));
			this.checkBoxRssSearchInLink.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchInLink.Dock")));
			this.checkBoxRssSearchInLink.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchInLink.Enabled")));
			this.checkBoxRssSearchInLink.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchInLink.FlatStyle")));
			this.checkBoxRssSearchInLink.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchInLink.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchInLink, resources.GetString("checkBoxRssSearchInLink.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchInLink, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchInLink.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchInLink, resources.GetString("checkBoxRssSearchInLink.HelpString"));
			this.checkBoxRssSearchInLink.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchInLink.Image")));
			this.checkBoxRssSearchInLink.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInLink.ImageAlign")));
			this.checkBoxRssSearchInLink.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchInLink.ImageIndex")));
			this.checkBoxRssSearchInLink.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchInLink.ImeMode")));
			this.checkBoxRssSearchInLink.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchInLink.Location")));
			this.checkBoxRssSearchInLink.Name = "checkBoxRssSearchInLink";
			this.checkBoxRssSearchInLink.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchInLink.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchInLink, ((bool)(resources.GetObject("checkBoxRssSearchInLink.ShowHelp"))));
			this.checkBoxRssSearchInLink.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchInLink.Size")));
			this.checkBoxRssSearchInLink.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchInLink.TabIndex")));
			this.checkBoxRssSearchInLink.Text = resources.GetString("checkBoxRssSearchInLink.Text");
			this.checkBoxRssSearchInLink.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchInLink.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchInLink, resources.GetString("checkBoxRssSearchInLink.ToolTip"));
			this.checkBoxRssSearchInLink.Visible = ((bool)(resources.GetObject("checkBoxRssSearchInLink.Visible")));
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.helpProvider1.SetHelpKeyword(this.label1, resources.GetString("label1.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label1, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label1.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label1, resources.GetString("label1.HelpString"));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label1, ((bool)(resources.GetObject("label1.ShowHelp"))));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.toolTip.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// collapsiblePanelAdvancedOptionsEx
			// 
			this.collapsiblePanelAdvancedOptionsEx.AccessibleDescription = resources.GetString("collapsiblePanelAdvancedOptionsEx.AccessibleDescription");
			this.collapsiblePanelAdvancedOptionsEx.AccessibleName = resources.GetString("collapsiblePanelAdvancedOptionsEx.AccessibleName");
			this.collapsiblePanelAdvancedOptionsEx.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.Anchor")));
			this.collapsiblePanelAdvancedOptionsEx.Animate = true;
			this.collapsiblePanelAdvancedOptionsEx.Collapsed = true;
			this.collapsiblePanelAdvancedOptionsEx.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.Dock")));
			this.collapsiblePanelAdvancedOptionsEx.Enabled = ((bool)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.Enabled")));
			this.collapsiblePanelAdvancedOptionsEx.ExpandedHeight = 370;
			this.collapsiblePanelAdvancedOptionsEx.Font = ((System.Drawing.Font)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.Font")));
			this.helpProvider1.SetHelpKeyword(this.collapsiblePanelAdvancedOptionsEx, resources.GetString("collapsiblePanelAdvancedOptionsEx.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.collapsiblePanelAdvancedOptionsEx, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.collapsiblePanelAdvancedOptionsEx, resources.GetString("collapsiblePanelAdvancedOptionsEx.HelpString"));
			this.collapsiblePanelAdvancedOptionsEx.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.ImeMode")));
			this.collapsiblePanelAdvancedOptionsEx.Items.AddRange(new System.Windows.Forms.Control[] {
																										 this.checkBoxConsiderItemReadState,
																										 this.checkBoxRssSearchUnreadItems,
																										 this.horizontalEdge,
																										 this.label2,
																										 this.checkBoxRssSearchTimeSpan,
																										 this.radioRssSearchItemsOlderThan,
																										 this.comboRssSearchItemAge,
																										 this.radioRssSearchItemsYoungerThan,
																										 this.label3,
																										 this.checkBoxRssSearchByDate,
																										 this.comboBoxRssSearchItemPostedOperator,
																										 this.dateTimeRssSearchItemPost,
																										 this.label4,
																										 this.checkBoxRssSearchByDateRange,
																										 this.dateTimeRssSearchPostAfter,
																										 this.dateTimeRssSearchPostBefore});
			this.collapsiblePanelAdvancedOptionsEx.Location = ((System.Drawing.Point)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.Location")));
			this.collapsiblePanelAdvancedOptionsEx.Name = "collapsiblePanelAdvancedOptionsEx";
			this.collapsiblePanelAdvancedOptionsEx.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.collapsiblePanelAdvancedOptionsEx, ((bool)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.ShowHelp"))));
			this.collapsiblePanelAdvancedOptionsEx.Size = ((System.Drawing.Size)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.Size")));
			this.collapsiblePanelAdvancedOptionsEx.TabIndex = ((int)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.TabIndex")));
			this.collapsiblePanelAdvancedOptionsEx.Text = resources.GetString("collapsiblePanelAdvancedOptionsEx.Text");
			this.toolTip.SetToolTip(this.collapsiblePanelAdvancedOptionsEx, resources.GetString("collapsiblePanelAdvancedOptionsEx.ToolTip"));
			this.collapsiblePanelAdvancedOptionsEx.Visible = ((bool)(resources.GetObject("collapsiblePanelAdvancedOptionsEx.Visible")));
			// 
			// checkBoxConsiderItemReadState
			// 
			this.checkBoxConsiderItemReadState.AccessibleDescription = resources.GetString("checkBoxConsiderItemReadState.AccessibleDescription");
			this.checkBoxConsiderItemReadState.AccessibleName = resources.GetString("checkBoxConsiderItemReadState.AccessibleName");
			this.checkBoxConsiderItemReadState.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxConsiderItemReadState.Anchor")));
			this.checkBoxConsiderItemReadState.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxConsiderItemReadState.Appearance")));
			this.checkBoxConsiderItemReadState.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxConsiderItemReadState.BackgroundImage")));
			this.checkBoxConsiderItemReadState.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxConsiderItemReadState.CheckAlign")));
			this.checkBoxConsiderItemReadState.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxConsiderItemReadState.Dock")));
			this.checkBoxConsiderItemReadState.Enabled = ((bool)(resources.GetObject("checkBoxConsiderItemReadState.Enabled")));
			this.checkBoxConsiderItemReadState.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxConsiderItemReadState.FlatStyle")));
			this.checkBoxConsiderItemReadState.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxConsiderItemReadState.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxConsiderItemReadState, resources.GetString("checkBoxConsiderItemReadState.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxConsiderItemReadState, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxConsiderItemReadState.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxConsiderItemReadState, resources.GetString("checkBoxConsiderItemReadState.HelpString"));
			this.checkBoxConsiderItemReadState.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxConsiderItemReadState.Image")));
			this.checkBoxConsiderItemReadState.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxConsiderItemReadState.ImageAlign")));
			this.checkBoxConsiderItemReadState.ImageIndex = ((int)(resources.GetObject("checkBoxConsiderItemReadState.ImageIndex")));
			this.checkBoxConsiderItemReadState.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxConsiderItemReadState.ImeMode")));
			this.checkBoxConsiderItemReadState.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxConsiderItemReadState.Location")));
			this.checkBoxConsiderItemReadState.Name = "checkBoxConsiderItemReadState";
			this.checkBoxConsiderItemReadState.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxConsiderItemReadState.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxConsiderItemReadState, ((bool)(resources.GetObject("checkBoxConsiderItemReadState.ShowHelp"))));
			this.checkBoxConsiderItemReadState.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxConsiderItemReadState.Size")));
			this.checkBoxConsiderItemReadState.TabIndex = ((int)(resources.GetObject("checkBoxConsiderItemReadState.TabIndex")));
			this.checkBoxConsiderItemReadState.Text = resources.GetString("checkBoxConsiderItemReadState.Text");
			this.checkBoxConsiderItemReadState.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxConsiderItemReadState.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxConsiderItemReadState, resources.GetString("checkBoxConsiderItemReadState.ToolTip"));
			this.checkBoxConsiderItemReadState.Visible = ((bool)(resources.GetObject("checkBoxConsiderItemReadState.Visible")));
			// 
			// checkBoxRssSearchUnreadItems
			// 
			this.checkBoxRssSearchUnreadItems.AccessibleDescription = resources.GetString("checkBoxRssSearchUnreadItems.AccessibleDescription");
			this.checkBoxRssSearchUnreadItems.AccessibleName = resources.GetString("checkBoxRssSearchUnreadItems.AccessibleName");
			this.checkBoxRssSearchUnreadItems.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchUnreadItems.Anchor")));
			this.checkBoxRssSearchUnreadItems.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchUnreadItems.Appearance")));
			this.checkBoxRssSearchUnreadItems.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchUnreadItems.BackgroundImage")));
			this.checkBoxRssSearchUnreadItems.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchUnreadItems.CheckAlign")));
			this.checkBoxRssSearchUnreadItems.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchUnreadItems.Dock")));
			this.checkBoxRssSearchUnreadItems.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchUnreadItems.Enabled")));
			this.checkBoxRssSearchUnreadItems.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchUnreadItems.FlatStyle")));
			this.checkBoxRssSearchUnreadItems.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchUnreadItems.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchUnreadItems, resources.GetString("checkBoxRssSearchUnreadItems.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchUnreadItems, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchUnreadItems.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchUnreadItems, resources.GetString("checkBoxRssSearchUnreadItems.HelpString"));
			this.checkBoxRssSearchUnreadItems.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchUnreadItems.Image")));
			this.checkBoxRssSearchUnreadItems.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchUnreadItems.ImageAlign")));
			this.checkBoxRssSearchUnreadItems.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchUnreadItems.ImageIndex")));
			this.checkBoxRssSearchUnreadItems.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchUnreadItems.ImeMode")));
			this.checkBoxRssSearchUnreadItems.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchUnreadItems.Location")));
			this.checkBoxRssSearchUnreadItems.Name = "checkBoxRssSearchUnreadItems";
			this.checkBoxRssSearchUnreadItems.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchUnreadItems.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchUnreadItems, ((bool)(resources.GetObject("checkBoxRssSearchUnreadItems.ShowHelp"))));
			this.checkBoxRssSearchUnreadItems.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchUnreadItems.Size")));
			this.checkBoxRssSearchUnreadItems.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchUnreadItems.TabIndex")));
			this.checkBoxRssSearchUnreadItems.Text = resources.GetString("checkBoxRssSearchUnreadItems.Text");
			this.checkBoxRssSearchUnreadItems.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchUnreadItems.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchUnreadItems, resources.GetString("checkBoxRssSearchUnreadItems.ToolTip"));
			this.checkBoxRssSearchUnreadItems.Visible = ((bool)(resources.GetObject("checkBoxRssSearchUnreadItems.Visible")));
			// 
			// horizontalEdge
			// 
			this.horizontalEdge.AccessibleDescription = resources.GetString("horizontalEdge.AccessibleDescription");
			this.horizontalEdge.AccessibleName = resources.GetString("horizontalEdge.AccessibleName");
			this.horizontalEdge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("horizontalEdge.Anchor")));
			this.horizontalEdge.AutoSize = ((bool)(resources.GetObject("horizontalEdge.AutoSize")));
			this.horizontalEdge.BackColor = System.Drawing.SystemColors.Control;
			this.horizontalEdge.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.horizontalEdge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("horizontalEdge.Dock")));
			this.horizontalEdge.Enabled = ((bool)(resources.GetObject("horizontalEdge.Enabled")));
			this.horizontalEdge.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.horizontalEdge.Font = ((System.Drawing.Font)(resources.GetObject("horizontalEdge.Font")));
			this.helpProvider1.SetHelpKeyword(this.horizontalEdge, resources.GetString("horizontalEdge.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.horizontalEdge, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("horizontalEdge.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.horizontalEdge, resources.GetString("horizontalEdge.HelpString"));
			this.horizontalEdge.Image = ((System.Drawing.Image)(resources.GetObject("horizontalEdge.Image")));
			this.horizontalEdge.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.ImageAlign")));
			this.horizontalEdge.ImageIndex = ((int)(resources.GetObject("horizontalEdge.ImageIndex")));
			this.horizontalEdge.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("horizontalEdge.ImeMode")));
			this.horizontalEdge.Location = ((System.Drawing.Point)(resources.GetObject("horizontalEdge.Location")));
			this.horizontalEdge.Name = "horizontalEdge";
			this.horizontalEdge.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("horizontalEdge.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.horizontalEdge, ((bool)(resources.GetObject("horizontalEdge.ShowHelp"))));
			this.horizontalEdge.Size = ((System.Drawing.Size)(resources.GetObject("horizontalEdge.Size")));
			this.horizontalEdge.TabIndex = ((int)(resources.GetObject("horizontalEdge.TabIndex")));
			this.horizontalEdge.Text = resources.GetString("horizontalEdge.Text");
			this.horizontalEdge.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.TextAlign")));
			this.toolTip.SetToolTip(this.horizontalEdge, resources.GetString("horizontalEdge.ToolTip"));
			this.horizontalEdge.Visible = ((bool)(resources.GetObject("horizontalEdge.Visible")));
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.BackColor = System.Drawing.SystemColors.Control;
			this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.helpProvider1.SetHelpKeyword(this.label2, resources.GetString("label2.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label2, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label2.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label2, resources.GetString("label2.HelpString"));
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label2, ((bool)(resources.GetObject("label2.ShowHelp"))));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.toolTip.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// checkBoxRssSearchTimeSpan
			// 
			this.checkBoxRssSearchTimeSpan.AccessibleDescription = resources.GetString("checkBoxRssSearchTimeSpan.AccessibleDescription");
			this.checkBoxRssSearchTimeSpan.AccessibleName = resources.GetString("checkBoxRssSearchTimeSpan.AccessibleName");
			this.checkBoxRssSearchTimeSpan.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchTimeSpan.Anchor")));
			this.checkBoxRssSearchTimeSpan.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchTimeSpan.Appearance")));
			this.checkBoxRssSearchTimeSpan.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchTimeSpan.BackgroundImage")));
			this.checkBoxRssSearchTimeSpan.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchTimeSpan.CheckAlign")));
			this.checkBoxRssSearchTimeSpan.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchTimeSpan.Dock")));
			this.checkBoxRssSearchTimeSpan.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchTimeSpan.Enabled")));
			this.checkBoxRssSearchTimeSpan.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchTimeSpan.FlatStyle")));
			this.checkBoxRssSearchTimeSpan.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchTimeSpan.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchTimeSpan, resources.GetString("checkBoxRssSearchTimeSpan.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchTimeSpan, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchTimeSpan.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchTimeSpan, resources.GetString("checkBoxRssSearchTimeSpan.HelpString"));
			this.checkBoxRssSearchTimeSpan.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchTimeSpan.Image")));
			this.checkBoxRssSearchTimeSpan.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchTimeSpan.ImageAlign")));
			this.checkBoxRssSearchTimeSpan.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchTimeSpan.ImageIndex")));
			this.checkBoxRssSearchTimeSpan.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchTimeSpan.ImeMode")));
			this.checkBoxRssSearchTimeSpan.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchTimeSpan.Location")));
			this.checkBoxRssSearchTimeSpan.Name = "checkBoxRssSearchTimeSpan";
			this.checkBoxRssSearchTimeSpan.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchTimeSpan.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchTimeSpan, ((bool)(resources.GetObject("checkBoxRssSearchTimeSpan.ShowHelp"))));
			this.checkBoxRssSearchTimeSpan.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchTimeSpan.Size")));
			this.checkBoxRssSearchTimeSpan.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchTimeSpan.TabIndex")));
			this.checkBoxRssSearchTimeSpan.Text = resources.GetString("checkBoxRssSearchTimeSpan.Text");
			this.checkBoxRssSearchTimeSpan.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchTimeSpan.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchTimeSpan, resources.GetString("checkBoxRssSearchTimeSpan.ToolTip"));
			this.checkBoxRssSearchTimeSpan.Visible = ((bool)(resources.GetObject("checkBoxRssSearchTimeSpan.Visible")));
			// 
			// radioRssSearchItemsOlderThan
			// 
			this.radioRssSearchItemsOlderThan.AccessibleDescription = resources.GetString("radioRssSearchItemsOlderThan.AccessibleDescription");
			this.radioRssSearchItemsOlderThan.AccessibleName = resources.GetString("radioRssSearchItemsOlderThan.AccessibleName");
			this.radioRssSearchItemsOlderThan.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioRssSearchItemsOlderThan.Anchor")));
			this.radioRssSearchItemsOlderThan.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioRssSearchItemsOlderThan.Appearance")));
			this.radioRssSearchItemsOlderThan.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioRssSearchItemsOlderThan.BackgroundImage")));
			this.radioRssSearchItemsOlderThan.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchItemsOlderThan.CheckAlign")));
			this.radioRssSearchItemsOlderThan.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioRssSearchItemsOlderThan.Dock")));
			this.radioRssSearchItemsOlderThan.Enabled = ((bool)(resources.GetObject("radioRssSearchItemsOlderThan.Enabled")));
			this.radioRssSearchItemsOlderThan.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioRssSearchItemsOlderThan.FlatStyle")));
			this.radioRssSearchItemsOlderThan.Font = ((System.Drawing.Font)(resources.GetObject("radioRssSearchItemsOlderThan.Font")));
			this.helpProvider1.SetHelpKeyword(this.radioRssSearchItemsOlderThan, resources.GetString("radioRssSearchItemsOlderThan.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.radioRssSearchItemsOlderThan, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("radioRssSearchItemsOlderThan.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.radioRssSearchItemsOlderThan, resources.GetString("radioRssSearchItemsOlderThan.HelpString"));
			this.radioRssSearchItemsOlderThan.Image = ((System.Drawing.Image)(resources.GetObject("radioRssSearchItemsOlderThan.Image")));
			this.radioRssSearchItemsOlderThan.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchItemsOlderThan.ImageAlign")));
			this.radioRssSearchItemsOlderThan.ImageIndex = ((int)(resources.GetObject("radioRssSearchItemsOlderThan.ImageIndex")));
			this.radioRssSearchItemsOlderThan.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioRssSearchItemsOlderThan.ImeMode")));
			this.radioRssSearchItemsOlderThan.Location = ((System.Drawing.Point)(resources.GetObject("radioRssSearchItemsOlderThan.Location")));
			this.radioRssSearchItemsOlderThan.Name = "radioRssSearchItemsOlderThan";
			this.radioRssSearchItemsOlderThan.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioRssSearchItemsOlderThan.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.radioRssSearchItemsOlderThan, ((bool)(resources.GetObject("radioRssSearchItemsOlderThan.ShowHelp"))));
			this.radioRssSearchItemsOlderThan.Size = ((System.Drawing.Size)(resources.GetObject("radioRssSearchItemsOlderThan.Size")));
			this.radioRssSearchItemsOlderThan.TabIndex = ((int)(resources.GetObject("radioRssSearchItemsOlderThan.TabIndex")));
			this.radioRssSearchItemsOlderThan.Text = resources.GetString("radioRssSearchItemsOlderThan.Text");
			this.radioRssSearchItemsOlderThan.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchItemsOlderThan.TextAlign")));
			this.toolTip.SetToolTip(this.radioRssSearchItemsOlderThan, resources.GetString("radioRssSearchItemsOlderThan.ToolTip"));
			this.radioRssSearchItemsOlderThan.Visible = ((bool)(resources.GetObject("radioRssSearchItemsOlderThan.Visible")));
			// 
			// comboRssSearchItemAge
			// 
			this.comboRssSearchItemAge.AccessibleDescription = resources.GetString("comboRssSearchItemAge.AccessibleDescription");
			this.comboRssSearchItemAge.AccessibleName = resources.GetString("comboRssSearchItemAge.AccessibleName");
			this.comboRssSearchItemAge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboRssSearchItemAge.Anchor")));
			this.comboRssSearchItemAge.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboRssSearchItemAge.BackgroundImage")));
			this.comboRssSearchItemAge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboRssSearchItemAge.Dock")));
			this.comboRssSearchItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboRssSearchItemAge.Enabled = ((bool)(resources.GetObject("comboRssSearchItemAge.Enabled")));
			this.comboRssSearchItemAge.Font = ((System.Drawing.Font)(resources.GetObject("comboRssSearchItemAge.Font")));
			this.helpProvider1.SetHelpKeyword(this.comboRssSearchItemAge, resources.GetString("comboRssSearchItemAge.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.comboRssSearchItemAge, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("comboRssSearchItemAge.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.comboRssSearchItemAge, resources.GetString("comboRssSearchItemAge.HelpString"));
			this.comboRssSearchItemAge.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboRssSearchItemAge.ImeMode")));
			this.comboRssSearchItemAge.IntegralHeight = ((bool)(resources.GetObject("comboRssSearchItemAge.IntegralHeight")));
			this.comboRssSearchItemAge.ItemHeight = ((int)(resources.GetObject("comboRssSearchItemAge.ItemHeight")));
			this.comboRssSearchItemAge.Items.AddRange(new object[] {
																	   resources.GetString("comboRssSearchItemAge.Items"),
																	   resources.GetString("comboRssSearchItemAge.Items1"),
																	   resources.GetString("comboRssSearchItemAge.Items2"),
																	   resources.GetString("comboRssSearchItemAge.Items3"),
																	   resources.GetString("comboRssSearchItemAge.Items4"),
																	   resources.GetString("comboRssSearchItemAge.Items5"),
																	   resources.GetString("comboRssSearchItemAge.Items6"),
																	   resources.GetString("comboRssSearchItemAge.Items7"),
																	   resources.GetString("comboRssSearchItemAge.Items8"),
																	   resources.GetString("comboRssSearchItemAge.Items9"),
																	   resources.GetString("comboRssSearchItemAge.Items10"),
																	   resources.GetString("comboRssSearchItemAge.Items11"),
																	   resources.GetString("comboRssSearchItemAge.Items12"),
																	   resources.GetString("comboRssSearchItemAge.Items13"),
																	   resources.GetString("comboRssSearchItemAge.Items14"),
																	   resources.GetString("comboRssSearchItemAge.Items15"),
																	   resources.GetString("comboRssSearchItemAge.Items16"),
																	   resources.GetString("comboRssSearchItemAge.Items17"),
																	   resources.GetString("comboRssSearchItemAge.Items18"),
																	   resources.GetString("comboRssSearchItemAge.Items19"),
																	   resources.GetString("comboRssSearchItemAge.Items20"),
																	   resources.GetString("comboRssSearchItemAge.Items21"),
																	   resources.GetString("comboRssSearchItemAge.Items22"),
																	   resources.GetString("comboRssSearchItemAge.Items23"),
																	   resources.GetString("comboRssSearchItemAge.Items24"),
																	   resources.GetString("comboRssSearchItemAge.Items25")});
			this.comboRssSearchItemAge.Location = ((System.Drawing.Point)(resources.GetObject("comboRssSearchItemAge.Location")));
			this.comboRssSearchItemAge.MaxDropDownItems = ((int)(resources.GetObject("comboRssSearchItemAge.MaxDropDownItems")));
			this.comboRssSearchItemAge.MaxLength = ((int)(resources.GetObject("comboRssSearchItemAge.MaxLength")));
			this.comboRssSearchItemAge.Name = "comboRssSearchItemAge";
			this.comboRssSearchItemAge.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboRssSearchItemAge.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.comboRssSearchItemAge, ((bool)(resources.GetObject("comboRssSearchItemAge.ShowHelp"))));
			this.comboRssSearchItemAge.Size = ((System.Drawing.Size)(resources.GetObject("comboRssSearchItemAge.Size")));
			this.comboRssSearchItemAge.TabIndex = ((int)(resources.GetObject("comboRssSearchItemAge.TabIndex")));
			this.comboRssSearchItemAge.Text = resources.GetString("comboRssSearchItemAge.Text");
			this.toolTip.SetToolTip(this.comboRssSearchItemAge, resources.GetString("comboRssSearchItemAge.ToolTip"));
			this.comboRssSearchItemAge.Visible = ((bool)(resources.GetObject("comboRssSearchItemAge.Visible")));
			// 
			// radioRssSearchItemsYoungerThan
			// 
			this.radioRssSearchItemsYoungerThan.AccessibleDescription = resources.GetString("radioRssSearchItemsYoungerThan.AccessibleDescription");
			this.radioRssSearchItemsYoungerThan.AccessibleName = resources.GetString("radioRssSearchItemsYoungerThan.AccessibleName");
			this.radioRssSearchItemsYoungerThan.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioRssSearchItemsYoungerThan.Anchor")));
			this.radioRssSearchItemsYoungerThan.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioRssSearchItemsYoungerThan.Appearance")));
			this.radioRssSearchItemsYoungerThan.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioRssSearchItemsYoungerThan.BackgroundImage")));
			this.radioRssSearchItemsYoungerThan.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchItemsYoungerThan.CheckAlign")));
			this.radioRssSearchItemsYoungerThan.Checked = true;
			this.radioRssSearchItemsYoungerThan.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioRssSearchItemsYoungerThan.Dock")));
			this.radioRssSearchItemsYoungerThan.Enabled = ((bool)(resources.GetObject("radioRssSearchItemsYoungerThan.Enabled")));
			this.radioRssSearchItemsYoungerThan.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioRssSearchItemsYoungerThan.FlatStyle")));
			this.radioRssSearchItemsYoungerThan.Font = ((System.Drawing.Font)(resources.GetObject("radioRssSearchItemsYoungerThan.Font")));
			this.helpProvider1.SetHelpKeyword(this.radioRssSearchItemsYoungerThan, resources.GetString("radioRssSearchItemsYoungerThan.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.radioRssSearchItemsYoungerThan, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("radioRssSearchItemsYoungerThan.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.radioRssSearchItemsYoungerThan, resources.GetString("radioRssSearchItemsYoungerThan.HelpString"));
			this.radioRssSearchItemsYoungerThan.Image = ((System.Drawing.Image)(resources.GetObject("radioRssSearchItemsYoungerThan.Image")));
			this.radioRssSearchItemsYoungerThan.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchItemsYoungerThan.ImageAlign")));
			this.radioRssSearchItemsYoungerThan.ImageIndex = ((int)(resources.GetObject("radioRssSearchItemsYoungerThan.ImageIndex")));
			this.radioRssSearchItemsYoungerThan.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioRssSearchItemsYoungerThan.ImeMode")));
			this.radioRssSearchItemsYoungerThan.Location = ((System.Drawing.Point)(resources.GetObject("radioRssSearchItemsYoungerThan.Location")));
			this.radioRssSearchItemsYoungerThan.Name = "radioRssSearchItemsYoungerThan";
			this.radioRssSearchItemsYoungerThan.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioRssSearchItemsYoungerThan.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.radioRssSearchItemsYoungerThan, ((bool)(resources.GetObject("radioRssSearchItemsYoungerThan.ShowHelp"))));
			this.radioRssSearchItemsYoungerThan.Size = ((System.Drawing.Size)(resources.GetObject("radioRssSearchItemsYoungerThan.Size")));
			this.radioRssSearchItemsYoungerThan.TabIndex = ((int)(resources.GetObject("radioRssSearchItemsYoungerThan.TabIndex")));
			this.radioRssSearchItemsYoungerThan.TabStop = true;
			this.radioRssSearchItemsYoungerThan.Text = resources.GetString("radioRssSearchItemsYoungerThan.Text");
			this.radioRssSearchItemsYoungerThan.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioRssSearchItemsYoungerThan.TextAlign")));
			this.toolTip.SetToolTip(this.radioRssSearchItemsYoungerThan, resources.GetString("radioRssSearchItemsYoungerThan.ToolTip"));
			this.radioRssSearchItemsYoungerThan.Visible = ((bool)(resources.GetObject("radioRssSearchItemsYoungerThan.Visible")));
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.BackColor = System.Drawing.SystemColors.Control;
			this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.helpProvider1.SetHelpKeyword(this.label3, resources.GetString("label3.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label3, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label3.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label3, resources.GetString("label3.HelpString"));
			this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
			this.label3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.ImageAlign")));
			this.label3.ImageIndex = ((int)(resources.GetObject("label3.ImageIndex")));
			this.label3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label3.ImeMode")));
			this.label3.Location = ((System.Drawing.Point)(resources.GetObject("label3.Location")));
			this.label3.Name = "label3";
			this.label3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label3.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label3, ((bool)(resources.GetObject("label3.ShowHelp"))));
			this.label3.Size = ((System.Drawing.Size)(resources.GetObject("label3.Size")));
			this.label3.TabIndex = ((int)(resources.GetObject("label3.TabIndex")));
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.TextAlign")));
			this.toolTip.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// checkBoxRssSearchByDate
			// 
			this.checkBoxRssSearchByDate.AccessibleDescription = resources.GetString("checkBoxRssSearchByDate.AccessibleDescription");
			this.checkBoxRssSearchByDate.AccessibleName = resources.GetString("checkBoxRssSearchByDate.AccessibleName");
			this.checkBoxRssSearchByDate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchByDate.Anchor")));
			this.checkBoxRssSearchByDate.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchByDate.Appearance")));
			this.checkBoxRssSearchByDate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchByDate.BackgroundImage")));
			this.checkBoxRssSearchByDate.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchByDate.CheckAlign")));
			this.checkBoxRssSearchByDate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchByDate.Dock")));
			this.checkBoxRssSearchByDate.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchByDate.Enabled")));
			this.checkBoxRssSearchByDate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchByDate.FlatStyle")));
			this.checkBoxRssSearchByDate.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchByDate.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchByDate, resources.GetString("checkBoxRssSearchByDate.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchByDate, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchByDate.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchByDate, resources.GetString("checkBoxRssSearchByDate.HelpString"));
			this.checkBoxRssSearchByDate.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchByDate.Image")));
			this.checkBoxRssSearchByDate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchByDate.ImageAlign")));
			this.checkBoxRssSearchByDate.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchByDate.ImageIndex")));
			this.checkBoxRssSearchByDate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchByDate.ImeMode")));
			this.checkBoxRssSearchByDate.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchByDate.Location")));
			this.checkBoxRssSearchByDate.Name = "checkBoxRssSearchByDate";
			this.checkBoxRssSearchByDate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchByDate.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchByDate, ((bool)(resources.GetObject("checkBoxRssSearchByDate.ShowHelp"))));
			this.checkBoxRssSearchByDate.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchByDate.Size")));
			this.checkBoxRssSearchByDate.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchByDate.TabIndex")));
			this.checkBoxRssSearchByDate.Text = resources.GetString("checkBoxRssSearchByDate.Text");
			this.checkBoxRssSearchByDate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchByDate.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchByDate, resources.GetString("checkBoxRssSearchByDate.ToolTip"));
			this.checkBoxRssSearchByDate.Visible = ((bool)(resources.GetObject("checkBoxRssSearchByDate.Visible")));
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.BackColor = System.Drawing.SystemColors.Control;
			this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.helpProvider1.SetHelpKeyword(this.label4, resources.GetString("label4.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label4, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label4.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label4, resources.GetString("label4.HelpString"));
			this.label4.Image = ((System.Drawing.Image)(resources.GetObject("label4.Image")));
			this.label4.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.ImageAlign")));
			this.label4.ImageIndex = ((int)(resources.GetObject("label4.ImageIndex")));
			this.label4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label4.ImeMode")));
			this.label4.Location = ((System.Drawing.Point)(resources.GetObject("label4.Location")));
			this.label4.Name = "label4";
			this.label4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label4.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label4, ((bool)(resources.GetObject("label4.ShowHelp"))));
			this.label4.Size = ((System.Drawing.Size)(resources.GetObject("label4.Size")));
			this.label4.TabIndex = ((int)(resources.GetObject("label4.TabIndex")));
			this.label4.Text = resources.GetString("label4.Text");
			this.label4.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.TextAlign")));
			this.toolTip.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// checkBoxRssSearchByDateRange
			// 
			this.checkBoxRssSearchByDateRange.AccessibleDescription = resources.GetString("checkBoxRssSearchByDateRange.AccessibleDescription");
			this.checkBoxRssSearchByDateRange.AccessibleName = resources.GetString("checkBoxRssSearchByDateRange.AccessibleName");
			this.checkBoxRssSearchByDateRange.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxRssSearchByDateRange.Anchor")));
			this.checkBoxRssSearchByDateRange.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxRssSearchByDateRange.Appearance")));
			this.checkBoxRssSearchByDateRange.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchByDateRange.BackgroundImage")));
			this.checkBoxRssSearchByDateRange.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchByDateRange.CheckAlign")));
			this.checkBoxRssSearchByDateRange.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxRssSearchByDateRange.Dock")));
			this.checkBoxRssSearchByDateRange.Enabled = ((bool)(resources.GetObject("checkBoxRssSearchByDateRange.Enabled")));
			this.checkBoxRssSearchByDateRange.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxRssSearchByDateRange.FlatStyle")));
			this.checkBoxRssSearchByDateRange.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxRssSearchByDateRange.Font")));
			this.helpProvider1.SetHelpKeyword(this.checkBoxRssSearchByDateRange, resources.GetString("checkBoxRssSearchByDateRange.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.checkBoxRssSearchByDateRange, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("checkBoxRssSearchByDateRange.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.checkBoxRssSearchByDateRange, resources.GetString("checkBoxRssSearchByDateRange.HelpString"));
			this.checkBoxRssSearchByDateRange.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxRssSearchByDateRange.Image")));
			this.checkBoxRssSearchByDateRange.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchByDateRange.ImageAlign")));
			this.checkBoxRssSearchByDateRange.ImageIndex = ((int)(resources.GetObject("checkBoxRssSearchByDateRange.ImageIndex")));
			this.checkBoxRssSearchByDateRange.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxRssSearchByDateRange.ImeMode")));
			this.checkBoxRssSearchByDateRange.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxRssSearchByDateRange.Location")));
			this.checkBoxRssSearchByDateRange.Name = "checkBoxRssSearchByDateRange";
			this.checkBoxRssSearchByDateRange.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxRssSearchByDateRange.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.checkBoxRssSearchByDateRange, ((bool)(resources.GetObject("checkBoxRssSearchByDateRange.ShowHelp"))));
			this.checkBoxRssSearchByDateRange.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxRssSearchByDateRange.Size")));
			this.checkBoxRssSearchByDateRange.TabIndex = ((int)(resources.GetObject("checkBoxRssSearchByDateRange.TabIndex")));
			this.checkBoxRssSearchByDateRange.Text = resources.GetString("checkBoxRssSearchByDateRange.Text");
			this.checkBoxRssSearchByDateRange.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxRssSearchByDateRange.TextAlign")));
			this.toolTip.SetToolTip(this.checkBoxRssSearchByDateRange, resources.GetString("checkBoxRssSearchByDateRange.ToolTip"));
			this.checkBoxRssSearchByDateRange.Visible = ((bool)(resources.GetObject("checkBoxRssSearchByDateRange.Visible")));
			// 
			// collapsiblePanelRssSearchScopeEx
			// 
			this.collapsiblePanelRssSearchScopeEx.AccessibleDescription = resources.GetString("collapsiblePanelRssSearchScopeEx.AccessibleDescription");
			this.collapsiblePanelRssSearchScopeEx.AccessibleName = resources.GetString("collapsiblePanelRssSearchScopeEx.AccessibleName");
			this.collapsiblePanelRssSearchScopeEx.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("collapsiblePanelRssSearchScopeEx.Anchor")));
			this.collapsiblePanelRssSearchScopeEx.Animate = true;
			this.collapsiblePanelRssSearchScopeEx.Collapsed = true;
			this.collapsiblePanelRssSearchScopeEx.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("collapsiblePanelRssSearchScopeEx.Dock")));
			this.collapsiblePanelRssSearchScopeEx.Enabled = ((bool)(resources.GetObject("collapsiblePanelRssSearchScopeEx.Enabled")));
			this.collapsiblePanelRssSearchScopeEx.ExpandedHeight = 215;
			this.collapsiblePanelRssSearchScopeEx.Font = ((System.Drawing.Font)(resources.GetObject("collapsiblePanelRssSearchScopeEx.Font")));
			this.helpProvider1.SetHelpKeyword(this.collapsiblePanelRssSearchScopeEx, resources.GetString("collapsiblePanelRssSearchScopeEx.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.collapsiblePanelRssSearchScopeEx, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("collapsiblePanelRssSearchScopeEx.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.collapsiblePanelRssSearchScopeEx, resources.GetString("collapsiblePanelRssSearchScopeEx.HelpString"));
			this.collapsiblePanelRssSearchScopeEx.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("collapsiblePanelRssSearchScopeEx.ImeMode")));
			this.collapsiblePanelRssSearchScopeEx.Items.AddRange(new System.Windows.Forms.Control[] {
																										this.treeRssSearchScope});
			this.collapsiblePanelRssSearchScopeEx.Location = ((System.Drawing.Point)(resources.GetObject("collapsiblePanelRssSearchScopeEx.Location")));
			this.collapsiblePanelRssSearchScopeEx.Name = "collapsiblePanelRssSearchScopeEx";
			this.collapsiblePanelRssSearchScopeEx.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("collapsiblePanelRssSearchScopeEx.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.collapsiblePanelRssSearchScopeEx, ((bool)(resources.GetObject("collapsiblePanelRssSearchScopeEx.ShowHelp"))));
			this.collapsiblePanelRssSearchScopeEx.Size = ((System.Drawing.Size)(resources.GetObject("collapsiblePanelRssSearchScopeEx.Size")));
			this.collapsiblePanelRssSearchScopeEx.TabIndex = ((int)(resources.GetObject("collapsiblePanelRssSearchScopeEx.TabIndex")));
			this.collapsiblePanelRssSearchScopeEx.Text = resources.GetString("collapsiblePanelRssSearchScopeEx.Text");
			this.toolTip.SetToolTip(this.collapsiblePanelRssSearchScopeEx, resources.GetString("collapsiblePanelRssSearchScopeEx.ToolTip"));
			this.collapsiblePanelRssSearchScopeEx.Visible = ((bool)(resources.GetObject("collapsiblePanelRssSearchScopeEx.Visible")));
			// 
			// treeRssSearchScope
			// 
			this.treeRssSearchScope.AccessibleDescription = resources.GetString("treeRssSearchScope.AccessibleDescription");
			this.treeRssSearchScope.AccessibleName = resources.GetString("treeRssSearchScope.AccessibleName");
			this.treeRssSearchScope.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("treeRssSearchScope.Anchor")));
			this.treeRssSearchScope.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("treeRssSearchScope.BackgroundImage")));
			this.treeRssSearchScope.CheckBoxes = true;
			this.treeRssSearchScope.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("treeRssSearchScope.Dock")));
			this.treeRssSearchScope.Enabled = ((bool)(resources.GetObject("treeRssSearchScope.Enabled")));
			this.treeRssSearchScope.Font = ((System.Drawing.Font)(resources.GetObject("treeRssSearchScope.Font")));
			this.helpProvider1.SetHelpKeyword(this.treeRssSearchScope, resources.GetString("treeRssSearchScope.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.treeRssSearchScope, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("treeRssSearchScope.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.treeRssSearchScope, resources.GetString("treeRssSearchScope.HelpString"));
			this.treeRssSearchScope.HideSelection = false;
			this.treeRssSearchScope.ImageIndex = ((int)(resources.GetObject("treeRssSearchScope.ImageIndex")));
			this.treeRssSearchScope.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("treeRssSearchScope.ImeMode")));
			this.treeRssSearchScope.Indent = ((int)(resources.GetObject("treeRssSearchScope.Indent")));
			this.treeRssSearchScope.ItemHeight = ((int)(resources.GetObject("treeRssSearchScope.ItemHeight")));
			this.treeRssSearchScope.Location = ((System.Drawing.Point)(resources.GetObject("treeRssSearchScope.Location")));
			this.treeRssSearchScope.Name = "treeRssSearchScope";
			this.treeRssSearchScope.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("treeRssSearchScope.RightToLeft")));
			this.treeRssSearchScope.SelectedImageIndex = ((int)(resources.GetObject("treeRssSearchScope.SelectedImageIndex")));
			this.helpProvider1.SetShowHelp(this.treeRssSearchScope, ((bool)(resources.GetObject("treeRssSearchScope.ShowHelp"))));
			this.treeRssSearchScope.Size = ((System.Drawing.Size)(resources.GetObject("treeRssSearchScope.Size")));
			this.treeRssSearchScope.TabIndex = ((int)(resources.GetObject("treeRssSearchScope.TabIndex")));
			this.treeRssSearchScope.Text = resources.GetString("treeRssSearchScope.Text");
			this.toolTip.SetToolTip(this.treeRssSearchScope, resources.GetString("treeRssSearchScope.ToolTip"));
			this.treeRssSearchScope.Visible = ((bool)(resources.GetObject("treeRssSearchScope.Visible")));
			this.treeRssSearchScope.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.OnRssSearchScopeTreeAfterCheck);
			// 
			// searchPaneSplitter
			// 
			this.searchPaneSplitter.AccessibleDescription = resources.GetString("searchPaneSplitter.AccessibleDescription");
			this.searchPaneSplitter.AccessibleName = resources.GetString("searchPaneSplitter.AccessibleName");
			this.searchPaneSplitter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("searchPaneSplitter.Anchor")));
			this.searchPaneSplitter.AnimationDelay = 20;
			this.searchPaneSplitter.AnimationStep = 20;
			this.searchPaneSplitter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("searchPaneSplitter.BackgroundImage")));
			this.searchPaneSplitter.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
			this.searchPaneSplitter.ControlToHide = null;
			this.searchPaneSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.searchPaneSplitter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("searchPaneSplitter.Dock")));
			this.searchPaneSplitter.Enabled = ((bool)(resources.GetObject("searchPaneSplitter.Enabled")));
			this.searchPaneSplitter.ExpandParentForm = false;
			this.searchPaneSplitter.Font = ((System.Drawing.Font)(resources.GetObject("searchPaneSplitter.Font")));
			this.helpProvider1.SetHelpKeyword(this.searchPaneSplitter, resources.GetString("searchPaneSplitter.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.searchPaneSplitter, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("searchPaneSplitter.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.searchPaneSplitter, resources.GetString("searchPaneSplitter.HelpString"));
			this.searchPaneSplitter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("searchPaneSplitter.ImeMode")));
			this.searchPaneSplitter.Location = ((System.Drawing.Point)(resources.GetObject("searchPaneSplitter.Location")));
			this.searchPaneSplitter.MinExtra = ((int)(resources.GetObject("searchPaneSplitter.MinExtra")));
			this.searchPaneSplitter.MinSize = ((int)(resources.GetObject("searchPaneSplitter.MinSize")));
			this.searchPaneSplitter.Name = "searchPaneSplitter";
			this.searchPaneSplitter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("searchPaneSplitter.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.searchPaneSplitter, ((bool)(resources.GetObject("searchPaneSplitter.ShowHelp"))));
			this.searchPaneSplitter.TabIndex = ((int)(resources.GetObject("searchPaneSplitter.TabIndex")));
			this.searchPaneSplitter.TabStop = false;
			this.toolTip.SetToolTip(this.searchPaneSplitter, resources.GetString("searchPaneSplitter.ToolTip"));
			this.searchPaneSplitter.UseAnimations = true;
			this.searchPaneSplitter.Visible = ((bool)(resources.GetObject("searchPaneSplitter.Visible")));
			this.searchPaneSplitter.VisualStyle = VisualStyles.XP;
			// 
			// panelRssSearchCommands
			// 
			this.panelRssSearchCommands.AccessibleDescription = resources.GetString("panelRssSearchCommands.AccessibleDescription");
			this.panelRssSearchCommands.AccessibleName = resources.GetString("panelRssSearchCommands.AccessibleName");
			this.panelRssSearchCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelRssSearchCommands.Anchor")));
			this.panelRssSearchCommands.AutoScroll = ((bool)(resources.GetObject("panelRssSearchCommands.AutoScroll")));
			this.panelRssSearchCommands.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelRssSearchCommands.AutoScrollMargin")));
			this.panelRssSearchCommands.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelRssSearchCommands.AutoScrollMinSize")));
			this.panelRssSearchCommands.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelRssSearchCommands.BackgroundImage")));
			this.panelRssSearchCommands.Controls.Add(this.btnNewSearch);
			this.panelRssSearchCommands.Controls.Add(this.textSearchExpression);
			this.panelRssSearchCommands.Controls.Add(this.btnSearchCancel);
			this.panelRssSearchCommands.Controls.Add(this.labelRssSearchState);
			this.panelRssSearchCommands.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelRssSearchCommands.Dock")));
			this.panelRssSearchCommands.Enabled = ((bool)(resources.GetObject("panelRssSearchCommands.Enabled")));
			this.panelRssSearchCommands.Font = ((System.Drawing.Font)(resources.GetObject("panelRssSearchCommands.Font")));
			this.helpProvider1.SetHelpKeyword(this.panelRssSearchCommands, resources.GetString("panelRssSearchCommands.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.panelRssSearchCommands, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("panelRssSearchCommands.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.panelRssSearchCommands, resources.GetString("panelRssSearchCommands.HelpString"));
			this.panelRssSearchCommands.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelRssSearchCommands.ImeMode")));
			this.panelRssSearchCommands.Location = ((System.Drawing.Point)(resources.GetObject("panelRssSearchCommands.Location")));
			this.panelRssSearchCommands.Name = "panelRssSearchCommands";
			this.panelRssSearchCommands.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelRssSearchCommands.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.panelRssSearchCommands, ((bool)(resources.GetObject("panelRssSearchCommands.ShowHelp"))));
			this.panelRssSearchCommands.Size = ((System.Drawing.Size)(resources.GetObject("panelRssSearchCommands.Size")));
			this.panelRssSearchCommands.TabIndex = ((int)(resources.GetObject("panelRssSearchCommands.TabIndex")));
			this.panelRssSearchCommands.Text = resources.GetString("panelRssSearchCommands.Text");
			this.toolTip.SetToolTip(this.panelRssSearchCommands, resources.GetString("panelRssSearchCommands.ToolTip"));
			this.panelRssSearchCommands.Visible = ((bool)(resources.GetObject("panelRssSearchCommands.Visible")));
			// 
			// btnNewSearch
			// 
			this.btnNewSearch.AccessibleDescription = resources.GetString("btnNewSearch.AccessibleDescription");
			this.btnNewSearch.AccessibleName = resources.GetString("btnNewSearch.AccessibleName");
			this.btnNewSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnNewSearch.Anchor")));
			this.btnNewSearch.BackColor = System.Drawing.SystemColors.Control;
			this.btnNewSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnNewSearch.BackgroundImage")));
			this.btnNewSearch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnNewSearch.Dock")));
			this.btnNewSearch.Enabled = ((bool)(resources.GetObject("btnNewSearch.Enabled")));
			this.btnNewSearch.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnNewSearch.FlatStyle")));
			this.btnNewSearch.Font = ((System.Drawing.Font)(resources.GetObject("btnNewSearch.Font")));
			this.helpProvider1.SetHelpKeyword(this.btnNewSearch, resources.GetString("btnNewSearch.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.btnNewSearch, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnNewSearch.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.btnNewSearch, resources.GetString("btnNewSearch.HelpString"));
			this.btnNewSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnNewSearch.Image")));
			this.btnNewSearch.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnNewSearch.ImageAlign")));
			this.btnNewSearch.ImageIndex = ((int)(resources.GetObject("btnNewSearch.ImageIndex")));
			this.btnNewSearch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnNewSearch.ImeMode")));
			this.btnNewSearch.Location = ((System.Drawing.Point)(resources.GetObject("btnNewSearch.Location")));
			this.btnNewSearch.Name = "btnNewSearch";
			this.btnNewSearch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnNewSearch.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.btnNewSearch, ((bool)(resources.GetObject("btnNewSearch.ShowHelp"))));
			this.btnNewSearch.Size = ((System.Drawing.Size)(resources.GetObject("btnNewSearch.Size")));
			this.btnNewSearch.TabIndex = ((int)(resources.GetObject("btnNewSearch.TabIndex")));
			this.btnNewSearch.Text = resources.GetString("btnNewSearch.Text");
			this.btnNewSearch.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnNewSearch.TextAlign")));
			this.toolTip.SetToolTip(this.btnNewSearch, resources.GetString("btnNewSearch.ToolTip"));
			this.btnNewSearch.Visible = ((bool)(resources.GetObject("btnNewSearch.Visible")));
			// 
			// textSearchExpression
			// 
			this.textSearchExpression.AccessibleDescription = resources.GetString("textSearchExpression.AccessibleDescription");
			this.textSearchExpression.AccessibleName = resources.GetString("textSearchExpression.AccessibleName");
			this.textSearchExpression.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textSearchExpression.Anchor")));
			this.textSearchExpression.AutoSize = ((bool)(resources.GetObject("textSearchExpression.AutoSize")));
			this.textSearchExpression.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textSearchExpression.BackgroundImage")));
			this.textSearchExpression.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textSearchExpression.Dock")));
			this.textSearchExpression.Enabled = ((bool)(resources.GetObject("textSearchExpression.Enabled")));
			this.textSearchExpression.Font = ((System.Drawing.Font)(resources.GetObject("textSearchExpression.Font")));
			this.helpProvider1.SetHelpKeyword(this.textSearchExpression, resources.GetString("textSearchExpression.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.textSearchExpression, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("textSearchExpression.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.textSearchExpression, resources.GetString("textSearchExpression.HelpString"));
			this.textSearchExpression.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textSearchExpression.ImeMode")));
			this.textSearchExpression.Location = ((System.Drawing.Point)(resources.GetObject("textSearchExpression.Location")));
			this.textSearchExpression.MaxLength = ((int)(resources.GetObject("textSearchExpression.MaxLength")));
			this.textSearchExpression.Multiline = ((bool)(resources.GetObject("textSearchExpression.Multiline")));
			this.textSearchExpression.Name = "textSearchExpression";
			this.textSearchExpression.PasswordChar = ((char)(resources.GetObject("textSearchExpression.PasswordChar")));
			this.textSearchExpression.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textSearchExpression.RightToLeft")));
			this.textSearchExpression.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textSearchExpression.ScrollBars")));
			this.helpProvider1.SetShowHelp(this.textSearchExpression, ((bool)(resources.GetObject("textSearchExpression.ShowHelp"))));
			this.textSearchExpression.Size = ((System.Drawing.Size)(resources.GetObject("textSearchExpression.Size")));
			this.textSearchExpression.TabIndex = ((int)(resources.GetObject("textSearchExpression.TabIndex")));
			this.textSearchExpression.Text = resources.GetString("textSearchExpression.Text");
			this.textSearchExpression.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textSearchExpression.TextAlign")));
			this.toolTip.SetToolTip(this.textSearchExpression, resources.GetString("textSearchExpression.ToolTip"));
			this.textSearchExpression.Visible = ((bool)(resources.GetObject("textSearchExpression.Visible")));
			this.textSearchExpression.WordWrap = ((bool)(resources.GetObject("textSearchExpression.WordWrap")));
			// 
			// btnSearchCancel
			// 
			this.btnSearchCancel.AccessibleDescription = resources.GetString("btnSearchCancel.AccessibleDescription");
			this.btnSearchCancel.AccessibleName = resources.GetString("btnSearchCancel.AccessibleName");
			this.btnSearchCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSearchCancel.Anchor")));
			this.btnSearchCancel.BackColor = System.Drawing.SystemColors.Control;
			this.btnSearchCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSearchCancel.BackgroundImage")));
			this.btnSearchCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSearchCancel.Dock")));
			this.btnSearchCancel.Enabled = ((bool)(resources.GetObject("btnSearchCancel.Enabled")));
			this.btnSearchCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSearchCancel.FlatStyle")));
			this.btnSearchCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnSearchCancel.Font")));
			this.helpProvider1.SetHelpKeyword(this.btnSearchCancel, resources.GetString("btnSearchCancel.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.btnSearchCancel, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnSearchCancel.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.btnSearchCancel, resources.GetString("btnSearchCancel.HelpString"));
			this.btnSearchCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnSearchCancel.Image")));
			this.btnSearchCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSearchCancel.ImageAlign")));
			this.btnSearchCancel.ImageIndex = ((int)(resources.GetObject("btnSearchCancel.ImageIndex")));
			this.btnSearchCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSearchCancel.ImeMode")));
			this.btnSearchCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnSearchCancel.Location")));
			this.btnSearchCancel.Name = "btnSearchCancel";
			this.btnSearchCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSearchCancel.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.btnSearchCancel, ((bool)(resources.GetObject("btnSearchCancel.ShowHelp"))));
			this.btnSearchCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnSearchCancel.Size")));
			this.btnSearchCancel.TabIndex = ((int)(resources.GetObject("btnSearchCancel.TabIndex")));
			this.btnSearchCancel.Text = resources.GetString("btnSearchCancel.Text");
			this.btnSearchCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSearchCancel.TextAlign")));
			this.toolTip.SetToolTip(this.btnSearchCancel, resources.GetString("btnSearchCancel.ToolTip"));
			this.btnSearchCancel.Visible = ((bool)(resources.GetObject("btnSearchCancel.Visible")));
			// 
			// labelRssSearchState
			// 
			this.labelRssSearchState.AccessibleDescription = resources.GetString("labelRssSearchState.AccessibleDescription");
			this.labelRssSearchState.AccessibleName = resources.GetString("labelRssSearchState.AccessibleName");
			this.labelRssSearchState.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelRssSearchState.Anchor")));
			this.labelRssSearchState.AutoSize = ((bool)(resources.GetObject("labelRssSearchState.AutoSize")));
			this.labelRssSearchState.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelRssSearchState.Dock")));
			this.labelRssSearchState.Enabled = ((bool)(resources.GetObject("labelRssSearchState.Enabled")));
			this.labelRssSearchState.Font = ((System.Drawing.Font)(resources.GetObject("labelRssSearchState.Font")));
			this.helpProvider1.SetHelpKeyword(this.labelRssSearchState, resources.GetString("labelRssSearchState.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.labelRssSearchState, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("labelRssSearchState.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.labelRssSearchState, resources.GetString("labelRssSearchState.HelpString"));
			this.labelRssSearchState.Image = ((System.Drawing.Image)(resources.GetObject("labelRssSearchState.Image")));
			this.labelRssSearchState.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRssSearchState.ImageAlign")));
			this.labelRssSearchState.ImageIndex = ((int)(resources.GetObject("labelRssSearchState.ImageIndex")));
			this.labelRssSearchState.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelRssSearchState.ImeMode")));
			this.labelRssSearchState.Location = ((System.Drawing.Point)(resources.GetObject("labelRssSearchState.Location")));
			this.labelRssSearchState.Name = "labelRssSearchState";
			this.labelRssSearchState.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelRssSearchState.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.labelRssSearchState, ((bool)(resources.GetObject("labelRssSearchState.ShowHelp"))));
			this.labelRssSearchState.Size = ((System.Drawing.Size)(resources.GetObject("labelRssSearchState.Size")));
			this.labelRssSearchState.TabIndex = ((int)(resources.GetObject("labelRssSearchState.TabIndex")));
			this.labelRssSearchState.Text = resources.GetString("labelRssSearchState.Text");
			this.labelRssSearchState.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRssSearchState.TextAlign")));
			this.toolTip.SetToolTip(this.labelRssSearchState, resources.GetString("labelRssSearchState.ToolTip"));
			this.labelRssSearchState.Visible = ((bool)(resources.GetObject("labelRssSearchState.Visible")));
			// 
			// _status
			// 
			this._status.AccessibleDescription = resources.GetString("_status.AccessibleDescription");
			this._status.AccessibleName = resources.GetString("_status.AccessibleName");
			this._status.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_status.Anchor")));
			this._status.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_status.BackgroundImage")));
			this._status.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("_status.Dock")));
			this._status.Enabled = ((bool)(resources.GetObject("_status.Enabled")));
			this._status.Font = ((System.Drawing.Font)(resources.GetObject("_status.Font")));
			this.helpProvider1.SetHelpKeyword(this._status, resources.GetString("_status.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this._status, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("_status.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this._status, resources.GetString("_status.HelpString"));
			this._status.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_status.ImeMode")));
			this._status.Location = ((System.Drawing.Point)(resources.GetObject("_status.Location")));
			this._status.Name = "_status";
			this._status.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																					   this.statusBarBrowser,
																					   this.statusBarBrowserProgress,
																					   this.statusBarConnectionState,
																					   this.statusBarRssParser});
			this._status.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_status.RightToLeft")));
			this.helpProvider1.SetShowHelp(this._status, ((bool)(resources.GetObject("_status.ShowHelp"))));
			this._status.ShowPanels = true;
			this._status.Size = ((System.Drawing.Size)(resources.GetObject("_status.Size")));
			this._status.TabIndex = ((int)(resources.GetObject("_status.TabIndex")));
			this._status.Text = resources.GetString("_status.Text");
			this.toolTip.SetToolTip(this._status, resources.GetString("_status.ToolTip"));
			this._status.Visible = ((bool)(resources.GetObject("_status.Visible")));
			// 
			// statusBarBrowser
			// 
			this.statusBarBrowser.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("statusBarBrowser.Alignment")));
			this.statusBarBrowser.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusBarBrowser.Icon = ((System.Drawing.Icon)(resources.GetObject("statusBarBrowser.Icon")));
			this.statusBarBrowser.MinWidth = ((int)(resources.GetObject("statusBarBrowser.MinWidth")));
			this.statusBarBrowser.Text = resources.GetString("statusBarBrowser.Text");
			this.statusBarBrowser.ToolTipText = resources.GetString("statusBarBrowser.ToolTipText");
			this.statusBarBrowser.Width = ((int)(resources.GetObject("statusBarBrowser.Width")));
			// 
			// statusBarBrowserProgress
			// 
			this.statusBarBrowserProgress.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("statusBarBrowserProgress.Alignment")));
			this.statusBarBrowserProgress.Icon = ((System.Drawing.Icon)(resources.GetObject("statusBarBrowserProgress.Icon")));
			this.statusBarBrowserProgress.MinWidth = ((int)(resources.GetObject("statusBarBrowserProgress.MinWidth")));
			this.statusBarBrowserProgress.Text = resources.GetString("statusBarBrowserProgress.Text");
			this.statusBarBrowserProgress.ToolTipText = resources.GetString("statusBarBrowserProgress.ToolTipText");
			this.statusBarBrowserProgress.Width = ((int)(resources.GetObject("statusBarBrowserProgress.Width")));
			// 
			// statusBarConnectionState
			// 
			this.statusBarConnectionState.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("statusBarConnectionState.Alignment")));
			this.statusBarConnectionState.Icon = ((System.Drawing.Icon)(resources.GetObject("statusBarConnectionState.Icon")));
			this.statusBarConnectionState.MinWidth = ((int)(resources.GetObject("statusBarConnectionState.MinWidth")));
			this.statusBarConnectionState.Text = resources.GetString("statusBarConnectionState.Text");
			this.statusBarConnectionState.ToolTipText = resources.GetString("statusBarConnectionState.ToolTipText");
			this.statusBarConnectionState.Width = ((int)(resources.GetObject("statusBarConnectionState.Width")));
			// 
			// statusBarRssParser
			// 
			this.statusBarRssParser.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("statusBarRssParser.Alignment")));
			this.statusBarRssParser.Icon = ((System.Drawing.Icon)(resources.GetObject("statusBarRssParser.Icon")));
			this.statusBarRssParser.MinWidth = ((int)(resources.GetObject("statusBarRssParser.MinWidth")));
			this.statusBarRssParser.Text = resources.GetString("statusBarRssParser.Text");
			this.statusBarRssParser.ToolTipText = resources.GetString("statusBarRssParser.ToolTipText");
			this.statusBarRssParser.Width = ((int)(resources.GetObject("statusBarRssParser.Width")));
			// 
			// bottomSandBarDock
			// 
			this.bottomSandBarDock.AccessibleDescription = resources.GetString("bottomSandBarDock.AccessibleDescription");
			this.bottomSandBarDock.AccessibleName = resources.GetString("bottomSandBarDock.AccessibleName");
			this.bottomSandBarDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("bottomSandBarDock.Anchor")));
			this.bottomSandBarDock.AutoScroll = ((bool)(resources.GetObject("bottomSandBarDock.AutoScroll")));
			this.bottomSandBarDock.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("bottomSandBarDock.AutoScrollMargin")));
			this.bottomSandBarDock.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("bottomSandBarDock.AutoScrollMinSize")));
			this.bottomSandBarDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bottomSandBarDock.BackgroundImage")));
			this.bottomSandBarDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("bottomSandBarDock.Dock")));
			this.bottomSandBarDock.Enabled = ((bool)(resources.GetObject("bottomSandBarDock.Enabled")));
			this.bottomSandBarDock.Font = ((System.Drawing.Font)(resources.GetObject("bottomSandBarDock.Font")));
			this.bottomSandBarDock.Guid = new System.Guid("966bb07b-f317-4abc-aff5-d81d4d0a2c87");
			this.helpProvider1.SetHelpKeyword(this.bottomSandBarDock, resources.GetString("bottomSandBarDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.bottomSandBarDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("bottomSandBarDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.bottomSandBarDock, resources.GetString("bottomSandBarDock.HelpString"));
			this.bottomSandBarDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("bottomSandBarDock.ImeMode")));
			this.bottomSandBarDock.Location = ((System.Drawing.Point)(resources.GetObject("bottomSandBarDock.Location")));
			this.bottomSandBarDock.Manager = this.sandBarManager;
			this.bottomSandBarDock.Name = "bottomSandBarDock";
			this.bottomSandBarDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("bottomSandBarDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.bottomSandBarDock, ((bool)(resources.GetObject("bottomSandBarDock.ShowHelp"))));
			this.bottomSandBarDock.Size = ((System.Drawing.Size)(resources.GetObject("bottomSandBarDock.Size")));
			this.bottomSandBarDock.TabIndex = ((int)(resources.GetObject("bottomSandBarDock.TabIndex")));
			this.bottomSandBarDock.Text = resources.GetString("bottomSandBarDock.Text");
			this.toolTip.SetToolTip(this.bottomSandBarDock, resources.GetString("bottomSandBarDock.ToolTip"));
			this.bottomSandBarDock.Visible = ((bool)(resources.GetObject("bottomSandBarDock.Visible")));
			// 
			// sandBarManager
			// 
			this.sandBarManager.OwnerForm = this;
			// 
			// leftSandBarDock
			// 
			this.leftSandBarDock.AccessibleDescription = resources.GetString("leftSandBarDock.AccessibleDescription");
			this.leftSandBarDock.AccessibleName = resources.GetString("leftSandBarDock.AccessibleName");
			this.leftSandBarDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("leftSandBarDock.Anchor")));
			this.leftSandBarDock.AutoScroll = ((bool)(resources.GetObject("leftSandBarDock.AutoScroll")));
			this.leftSandBarDock.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("leftSandBarDock.AutoScrollMargin")));
			this.leftSandBarDock.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("leftSandBarDock.AutoScrollMinSize")));
			this.leftSandBarDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("leftSandBarDock.BackgroundImage")));
			this.leftSandBarDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("leftSandBarDock.Dock")));
			this.leftSandBarDock.Enabled = ((bool)(resources.GetObject("leftSandBarDock.Enabled")));
			this.leftSandBarDock.Font = ((System.Drawing.Font)(resources.GetObject("leftSandBarDock.Font")));
			this.leftSandBarDock.Guid = new System.Guid("bde346df-f16a-4686-94ff-c4abad6371de");
			this.helpProvider1.SetHelpKeyword(this.leftSandBarDock, resources.GetString("leftSandBarDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.leftSandBarDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("leftSandBarDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.leftSandBarDock, resources.GetString("leftSandBarDock.HelpString"));
			this.leftSandBarDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("leftSandBarDock.ImeMode")));
			this.leftSandBarDock.Location = ((System.Drawing.Point)(resources.GetObject("leftSandBarDock.Location")));
			this.leftSandBarDock.Manager = this.sandBarManager;
			this.leftSandBarDock.Name = "leftSandBarDock";
			this.leftSandBarDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("leftSandBarDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.leftSandBarDock, ((bool)(resources.GetObject("leftSandBarDock.ShowHelp"))));
			this.leftSandBarDock.Size = ((System.Drawing.Size)(resources.GetObject("leftSandBarDock.Size")));
			this.leftSandBarDock.TabIndex = ((int)(resources.GetObject("leftSandBarDock.TabIndex")));
			this.leftSandBarDock.Text = resources.GetString("leftSandBarDock.Text");
			this.toolTip.SetToolTip(this.leftSandBarDock, resources.GetString("leftSandBarDock.ToolTip"));
			this.leftSandBarDock.Visible = ((bool)(resources.GetObject("leftSandBarDock.Visible")));
			// 
			// rightSandBarDock
			// 
			this.rightSandBarDock.AccessibleDescription = resources.GetString("rightSandBarDock.AccessibleDescription");
			this.rightSandBarDock.AccessibleName = resources.GetString("rightSandBarDock.AccessibleName");
			this.rightSandBarDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rightSandBarDock.Anchor")));
			this.rightSandBarDock.AutoScroll = ((bool)(resources.GetObject("rightSandBarDock.AutoScroll")));
			this.rightSandBarDock.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("rightSandBarDock.AutoScrollMargin")));
			this.rightSandBarDock.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("rightSandBarDock.AutoScrollMinSize")));
			this.rightSandBarDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rightSandBarDock.BackgroundImage")));
			this.rightSandBarDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rightSandBarDock.Dock")));
			this.rightSandBarDock.Enabled = ((bool)(resources.GetObject("rightSandBarDock.Enabled")));
			this.rightSandBarDock.Font = ((System.Drawing.Font)(resources.GetObject("rightSandBarDock.Font")));
			this.rightSandBarDock.Guid = new System.Guid("762038c8-b9db-4370-b3a6-755942c4ff89");
			this.helpProvider1.SetHelpKeyword(this.rightSandBarDock, resources.GetString("rightSandBarDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.rightSandBarDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("rightSandBarDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.rightSandBarDock, resources.GetString("rightSandBarDock.HelpString"));
			this.rightSandBarDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rightSandBarDock.ImeMode")));
			this.rightSandBarDock.Location = ((System.Drawing.Point)(resources.GetObject("rightSandBarDock.Location")));
			this.rightSandBarDock.Manager = this.sandBarManager;
			this.rightSandBarDock.Name = "rightSandBarDock";
			this.rightSandBarDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rightSandBarDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.rightSandBarDock, ((bool)(resources.GetObject("rightSandBarDock.ShowHelp"))));
			this.rightSandBarDock.Size = ((System.Drawing.Size)(resources.GetObject("rightSandBarDock.Size")));
			this.rightSandBarDock.TabIndex = ((int)(resources.GetObject("rightSandBarDock.TabIndex")));
			this.rightSandBarDock.Text = resources.GetString("rightSandBarDock.Text");
			this.toolTip.SetToolTip(this.rightSandBarDock, resources.GetString("rightSandBarDock.ToolTip"));
			this.rightSandBarDock.Visible = ((bool)(resources.GetObject("rightSandBarDock.Visible")));
			// 
			// topSandBarDock
			// 
			this.topSandBarDock.AccessibleDescription = resources.GetString("topSandBarDock.AccessibleDescription");
			this.topSandBarDock.AccessibleName = resources.GetString("topSandBarDock.AccessibleName");
			this.topSandBarDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("topSandBarDock.Anchor")));
			this.topSandBarDock.AutoScroll = ((bool)(resources.GetObject("topSandBarDock.AutoScroll")));
			this.topSandBarDock.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("topSandBarDock.AutoScrollMargin")));
			this.topSandBarDock.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("topSandBarDock.AutoScrollMinSize")));
			this.topSandBarDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("topSandBarDock.BackgroundImage")));
			this.topSandBarDock.Controls.Add(this.toolBarMain);
			this.topSandBarDock.Controls.Add(this.menuBarMain);
			this.topSandBarDock.Controls.Add(this.toolBarBrowser);
			this.topSandBarDock.Controls.Add(this.toolBarWebSearch);
			this.topSandBarDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("topSandBarDock.Dock")));
			this.topSandBarDock.Enabled = ((bool)(resources.GetObject("topSandBarDock.Enabled")));
			this.topSandBarDock.Font = ((System.Drawing.Font)(resources.GetObject("topSandBarDock.Font")));
			this.topSandBarDock.Guid = new System.Guid("f7942d78-c06c-44f8-943e-b0f68611be66");
			this.helpProvider1.SetHelpKeyword(this.topSandBarDock, resources.GetString("topSandBarDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.topSandBarDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("topSandBarDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.topSandBarDock, resources.GetString("topSandBarDock.HelpString"));
			this.topSandBarDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("topSandBarDock.ImeMode")));
			this.topSandBarDock.Location = ((System.Drawing.Point)(resources.GetObject("topSandBarDock.Location")));
			this.topSandBarDock.Manager = this.sandBarManager;
			this.topSandBarDock.Name = "topSandBarDock";
			this.topSandBarDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("topSandBarDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.topSandBarDock, ((bool)(resources.GetObject("topSandBarDock.ShowHelp"))));
			this.topSandBarDock.Size = ((System.Drawing.Size)(resources.GetObject("topSandBarDock.Size")));
			this.topSandBarDock.TabIndex = ((int)(resources.GetObject("topSandBarDock.TabIndex")));
			this.topSandBarDock.Text = resources.GetString("topSandBarDock.Text");
			this.toolTip.SetToolTip(this.topSandBarDock, resources.GetString("topSandBarDock.ToolTip"));
			this.topSandBarDock.Visible = ((bool)(resources.GetObject("topSandBarDock.Visible")));
			// 
			// toolBarMain
			// 
			this.toolBarMain.AccessibleDescription = resources.GetString("toolBarMain.AccessibleDescription");
			this.toolBarMain.AccessibleName = resources.GetString("toolBarMain.AccessibleName");
			this.toolBarMain.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("toolBarMain.Anchor")));
			this.toolBarMain.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("toolBarMain.BackgroundImage")));
			this.toolBarMain.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("toolBarMain.Dock")));
			this.toolBarMain.DockLine = 1;
			this.toolBarMain.Enabled = ((bool)(resources.GetObject("toolBarMain.Enabled")));
			this.toolBarMain.Font = ((System.Drawing.Font)(resources.GetObject("toolBarMain.Font")));
			this.toolBarMain.Guid = new System.Guid("c95feca6-b21e-4660-ad20-8e8e5b9fe26c");
			this.helpProvider1.SetHelpKeyword(this.toolBarMain, resources.GetString("toolBarMain.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.toolBarMain, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("toolBarMain.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.toolBarMain, resources.GetString("toolBarMain.HelpString"));
			this.toolBarMain.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("toolBarMain.ImeMode")));
			this.toolBarMain.Location = ((System.Drawing.Point)(resources.GetObject("toolBarMain.Location")));
			this.toolBarMain.Name = "toolBarMain";
			this.toolBarMain.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("toolBarMain.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.toolBarMain, ((bool)(resources.GetObject("toolBarMain.ShowHelp"))));
			this.toolBarMain.Size = ((System.Drawing.Size)(resources.GetObject("toolBarMain.Size")));
			this.toolBarMain.TabIndex = ((int)(resources.GetObject("toolBarMain.TabIndex")));
			this.toolBarMain.Text = resources.GetString("toolBarMain.Text");
			this.toolTip.SetToolTip(this.toolBarMain, resources.GetString("toolBarMain.ToolTip"));
			this.toolBarMain.Visible = ((bool)(resources.GetObject("toolBarMain.Visible")));
			// 
			// menuBarMain
			// 
			this.menuBarMain.AccessibleDescription = resources.GetString("menuBarMain.AccessibleDescription");
			this.menuBarMain.AccessibleName = resources.GetString("menuBarMain.AccessibleName");
			this.menuBarMain.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("menuBarMain.Anchor")));
			this.menuBarMain.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("menuBarMain.BackgroundImage")));
			this.menuBarMain.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("menuBarMain.Dock")));
			this.menuBarMain.Enabled = ((bool)(resources.GetObject("menuBarMain.Enabled")));
			this.menuBarMain.Font = ((System.Drawing.Font)(resources.GetObject("menuBarMain.Font")));
			this.menuBarMain.Guid = new System.Guid("420e8a01-f6b7-4edf-a233-f94d6966eb9e");
			this.helpProvider1.SetHelpKeyword(this.menuBarMain, resources.GetString("menuBarMain.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.menuBarMain, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("menuBarMain.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.menuBarMain, resources.GetString("menuBarMain.HelpString"));
			this.menuBarMain.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("menuBarMain.ImeMode")));
			this.menuBarMain.Location = ((System.Drawing.Point)(resources.GetObject("menuBarMain.Location")));
			this.menuBarMain.Name = "menuBarMain";
			this.menuBarMain.OwnerForm = this;
			this.menuBarMain.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("menuBarMain.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.menuBarMain, ((bool)(resources.GetObject("menuBarMain.ShowHelp"))));
			this.menuBarMain.Size = ((System.Drawing.Size)(resources.GetObject("menuBarMain.Size")));
			this.menuBarMain.TabIndex = ((int)(resources.GetObject("menuBarMain.TabIndex")));
			this.menuBarMain.Text = resources.GetString("menuBarMain.Text");
			this.toolTip.SetToolTip(this.menuBarMain, resources.GetString("menuBarMain.ToolTip"));
			this.menuBarMain.Visible = ((bool)(resources.GetObject("menuBarMain.Visible")));
			// 
			// toolBarBrowser
			// 
			this.toolBarBrowser.AccessibleDescription = resources.GetString("toolBarBrowser.AccessibleDescription");
			this.toolBarBrowser.AccessibleName = resources.GetString("toolBarBrowser.AccessibleName");
			this.toolBarBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("toolBarBrowser.Anchor")));
			this.toolBarBrowser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("toolBarBrowser.BackgroundImage")));
			this.toolBarBrowser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("toolBarBrowser.Dock")));
			this.toolBarBrowser.DockLine = 2;
			this.toolBarBrowser.Enabled = ((bool)(resources.GetObject("toolBarBrowser.Enabled")));
			this.toolBarBrowser.Font = ((System.Drawing.Font)(resources.GetObject("toolBarBrowser.Font")));
			this.toolBarBrowser.Guid = new System.Guid("c632397d-48a5-4a1d-b2ad-8e6eff700483");
			this.helpProvider1.SetHelpKeyword(this.toolBarBrowser, resources.GetString("toolBarBrowser.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.toolBarBrowser, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("toolBarBrowser.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.toolBarBrowser, resources.GetString("toolBarBrowser.HelpString"));
			this.toolBarBrowser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("toolBarBrowser.ImeMode")));
			this.toolBarBrowser.Location = ((System.Drawing.Point)(resources.GetObject("toolBarBrowser.Location")));
			this.toolBarBrowser.Name = "toolBarBrowser";
			this.toolBarBrowser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("toolBarBrowser.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.toolBarBrowser, ((bool)(resources.GetObject("toolBarBrowser.ShowHelp"))));
			this.toolBarBrowser.Size = ((System.Drawing.Size)(resources.GetObject("toolBarBrowser.Size")));
			this.toolBarBrowser.TabIndex = ((int)(resources.GetObject("toolBarBrowser.TabIndex")));
			this.toolBarBrowser.Text = resources.GetString("toolBarBrowser.Text");
			this.toolTip.SetToolTip(this.toolBarBrowser, resources.GetString("toolBarBrowser.ToolTip"));
			this.toolBarBrowser.Visible = ((bool)(resources.GetObject("toolBarBrowser.Visible")));
			// 
			// toolBarWebSearch
			// 
			this.toolBarWebSearch.AccessibleDescription = resources.GetString("toolBarWebSearch.AccessibleDescription");
			this.toolBarWebSearch.AccessibleName = resources.GetString("toolBarWebSearch.AccessibleName");
			this.toolBarWebSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("toolBarWebSearch.Anchor")));
			this.toolBarWebSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("toolBarWebSearch.BackgroundImage")));
			this.toolBarWebSearch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("toolBarWebSearch.Dock")));
			this.toolBarWebSearch.DockLine = 2;
			this.toolBarWebSearch.DockOffset = 1;
			this.toolBarWebSearch.Enabled = ((bool)(resources.GetObject("toolBarWebSearch.Enabled")));
			this.toolBarWebSearch.Font = ((System.Drawing.Font)(resources.GetObject("toolBarWebSearch.Font")));
			this.toolBarWebSearch.Guid = new System.Guid("672d3df7-e580-492b-a4c4-216034d11636");
			this.helpProvider1.SetHelpKeyword(this.toolBarWebSearch, resources.GetString("toolBarWebSearch.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.toolBarWebSearch, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("toolBarWebSearch.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.toolBarWebSearch, resources.GetString("toolBarWebSearch.HelpString"));
			this.toolBarWebSearch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("toolBarWebSearch.ImeMode")));
			this.toolBarWebSearch.Location = ((System.Drawing.Point)(resources.GetObject("toolBarWebSearch.Location")));
			this.toolBarWebSearch.Name = "toolBarWebSearch";
			this.toolBarWebSearch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("toolBarWebSearch.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.toolBarWebSearch, ((bool)(resources.GetObject("toolBarWebSearch.ShowHelp"))));
			this.toolBarWebSearch.Size = ((System.Drawing.Size)(resources.GetObject("toolBarWebSearch.Size")));
			this.toolBarWebSearch.TabIndex = ((int)(resources.GetObject("toolBarWebSearch.TabIndex")));
			this.toolBarWebSearch.Text = resources.GetString("toolBarWebSearch.Text");
			this.toolTip.SetToolTip(this.toolBarWebSearch, resources.GetString("toolBarWebSearch.ToolTip"));
			this.toolBarWebSearch.Visible = ((bool)(resources.GetObject("toolBarWebSearch.Visible")));
			// 
			// progressBrowser
			// 
			this.progressBrowser.AccessibleDescription = resources.GetString("progressBrowser.AccessibleDescription");
			this.progressBrowser.AccessibleName = resources.GetString("progressBrowser.AccessibleName");
			this.progressBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("progressBrowser.Anchor")));
			this.progressBrowser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("progressBrowser.BackgroundImage")));
			this.progressBrowser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("progressBrowser.Dock")));
			this.progressBrowser.Enabled = ((bool)(resources.GetObject("progressBrowser.Enabled")));
			this.progressBrowser.Font = ((System.Drawing.Font)(resources.GetObject("progressBrowser.Font")));
			this.helpProvider1.SetHelpKeyword(this.progressBrowser, resources.GetString("progressBrowser.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.progressBrowser, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("progressBrowser.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.progressBrowser, resources.GetString("progressBrowser.HelpString"));
			this.progressBrowser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("progressBrowser.ImeMode")));
			this.progressBrowser.Location = ((System.Drawing.Point)(resources.GetObject("progressBrowser.Location")));
			this.progressBrowser.Name = "progressBrowser";
			this.progressBrowser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("progressBrowser.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.progressBrowser, ((bool)(resources.GetObject("progressBrowser.ShowHelp"))));
			this.progressBrowser.Size = ((System.Drawing.Size)(resources.GetObject("progressBrowser.Size")));
			this.progressBrowser.TabIndex = ((int)(resources.GetObject("progressBrowser.TabIndex")));
			this.progressBrowser.Text = resources.GetString("progressBrowser.Text");
			this.toolTip.SetToolTip(this.progressBrowser, resources.GetString("progressBrowser.ToolTip"));
			this.progressBrowser.Visible = ((bool)(resources.GetObject("progressBrowser.Visible")));
			// 
			// leftSandDock
			// 
			this.leftSandDock.AccessibleDescription = resources.GetString("leftSandDock.AccessibleDescription");
			this.leftSandDock.AccessibleName = resources.GetString("leftSandDock.AccessibleName");
			this.leftSandDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("leftSandDock.Anchor")));
			this.leftSandDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("leftSandDock.BackgroundImage")));
			this.leftSandDock.Controls.Add(this.dockSubscriptions);
			this.leftSandDock.Controls.Add(this.dockSearch);
			this.leftSandDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("leftSandDock.Dock")));
			this.leftSandDock.DockingManager = TD.SandDock.DockingManager.Whidbey;
			this.leftSandDock.Enabled = ((bool)(resources.GetObject("leftSandDock.Enabled")));
			this.leftSandDock.Font = ((System.Drawing.Font)(resources.GetObject("leftSandDock.Font")));
			this.leftSandDock.Guid = new System.Guid("025dc7ba-76a8-4cea-925d-8cd8237c14a6");
			this.helpProvider1.SetHelpKeyword(this.leftSandDock, resources.GetString("leftSandDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.leftSandDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("leftSandDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.leftSandDock, resources.GetString("leftSandDock.HelpString"));
			this.leftSandDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("leftSandDock.ImeMode")));
			this.leftSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400, System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
																																											 new TD.SandDock.ControlLayoutSystem(231, 397, new TD.SandDock.DockControl[] {
																																																															 this.dockSubscriptions,
																																																															 this.dockSearch}, this.dockSubscriptions)});
			this.leftSandDock.Location = ((System.Drawing.Point)(resources.GetObject("leftSandDock.Location")));
			this.leftSandDock.Manager = this.sandDockManager;
			this.leftSandDock.Name = "leftSandDock";
			this.leftSandDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("leftSandDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.leftSandDock, ((bool)(resources.GetObject("leftSandDock.ShowHelp"))));
			this.leftSandDock.Size = ((System.Drawing.Size)(resources.GetObject("leftSandDock.Size")));
			this.leftSandDock.TabIndex = ((int)(resources.GetObject("leftSandDock.TabIndex")));
			this.leftSandDock.Text = resources.GetString("leftSandDock.Text");
			this.toolTip.SetToolTip(this.leftSandDock, resources.GetString("leftSandDock.ToolTip"));
			this.leftSandDock.Visible = ((bool)(resources.GetObject("leftSandDock.Visible")));
			// 
			// dockSubscriptions
			// 
			this.dockSubscriptions.AccessibleDescription = resources.GetString("dockSubscriptions.AccessibleDescription");
			this.dockSubscriptions.AccessibleName = resources.GetString("dockSubscriptions.AccessibleName");
			this.dockSubscriptions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dockSubscriptions.Anchor")));
			this.dockSubscriptions.AutoScroll = ((bool)(resources.GetObject("dockSubscriptions.AutoScroll")));
			this.dockSubscriptions.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("dockSubscriptions.AutoScrollMargin")));
			this.dockSubscriptions.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("dockSubscriptions.AutoScrollMinSize")));
			this.dockSubscriptions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dockSubscriptions.BackgroundImage")));
			this.dockSubscriptions.Controls.Add(this.treeFeeds);
			this.dockSubscriptions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dockSubscriptions.Dock")));
			this.dockSubscriptions.Enabled = ((bool)(resources.GetObject("dockSubscriptions.Enabled")));
			this.dockSubscriptions.Font = ((System.Drawing.Font)(resources.GetObject("dockSubscriptions.Font")));
			this.dockSubscriptions.Guid = new System.Guid("a50ce358-5269-488e-8b60-fd9e858345d1");
			this.helpProvider1.SetHelpKeyword(this.dockSubscriptions, resources.GetString("dockSubscriptions.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.dockSubscriptions, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("dockSubscriptions.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.dockSubscriptions, resources.GetString("dockSubscriptions.HelpString"));
			this.dockSubscriptions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dockSubscriptions.ImeMode")));
			this.dockSubscriptions.Location = ((System.Drawing.Point)(resources.GetObject("dockSubscriptions.Location")));
			this.dockSubscriptions.Name = "dockSubscriptions";
			this.dockSubscriptions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dockSubscriptions.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.dockSubscriptions, ((bool)(resources.GetObject("dockSubscriptions.ShowHelp"))));
			this.dockSubscriptions.Size = ((System.Drawing.Size)(resources.GetObject("dockSubscriptions.Size")));
			this.dockSubscriptions.TabImage = ((System.Drawing.Image)(resources.GetObject("dockSubscriptions.TabImage")));
			this.dockSubscriptions.TabIndex = ((int)(resources.GetObject("dockSubscriptions.TabIndex")));
			this.dockSubscriptions.TabText = resources.GetString("dockSubscriptions.TabText");
			this.dockSubscriptions.Text = resources.GetString("dockSubscriptions.Text");
			this.toolTip.SetToolTip(this.dockSubscriptions, resources.GetString("dockSubscriptions.ToolTip"));
			this.dockSubscriptions.ToolTipText = resources.GetString("dockSubscriptions.ToolTipText");
			this.dockSubscriptions.Visible = ((bool)(resources.GetObject("dockSubscriptions.Visible")));
			// 
			// dockSearch
			// 
			this.dockSearch.AccessibleDescription = resources.GetString("dockSearch.AccessibleDescription");
			this.dockSearch.AccessibleName = resources.GetString("dockSearch.AccessibleName");
			this.dockSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dockSearch.Anchor")));
			this.dockSearch.AutoScroll = ((bool)(resources.GetObject("dockSearch.AutoScroll")));
			this.dockSearch.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("dockSearch.AutoScrollMargin")));
			this.dockSearch.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("dockSearch.AutoScrollMinSize")));
			this.dockSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dockSearch.BackgroundImage")));
			this.dockSearch.Controls.Add(this.panelRssSearch);
			this.dockSearch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dockSearch.Dock")));
			this.dockSearch.Enabled = ((bool)(resources.GetObject("dockSearch.Enabled")));
			this.dockSearch.Font = ((System.Drawing.Font)(resources.GetObject("dockSearch.Font")));
			this.dockSearch.Guid = new System.Guid("5b89c95c-7d15-4766-ba27-68b047c228ec");
			this.helpProvider1.SetHelpKeyword(this.dockSearch, resources.GetString("dockSearch.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.dockSearch, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("dockSearch.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.dockSearch, resources.GetString("dockSearch.HelpString"));
			this.dockSearch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dockSearch.ImeMode")));
			this.dockSearch.Location = ((System.Drawing.Point)(resources.GetObject("dockSearch.Location")));
			this.dockSearch.Name = "dockSearch";
			this.dockSearch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dockSearch.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.dockSearch, ((bool)(resources.GetObject("dockSearch.ShowHelp"))));
			this.dockSearch.Size = ((System.Drawing.Size)(resources.GetObject("dockSearch.Size")));
			this.dockSearch.TabImage = ((System.Drawing.Image)(resources.GetObject("dockSearch.TabImage")));
			this.dockSearch.TabIndex = ((int)(resources.GetObject("dockSearch.TabIndex")));
			this.dockSearch.TabText = resources.GetString("dockSearch.TabText");
			this.dockSearch.Text = resources.GetString("dockSearch.Text");
			this.toolTip.SetToolTip(this.dockSearch, resources.GetString("dockSearch.ToolTip"));
			this.dockSearch.ToolTipText = resources.GetString("dockSearch.ToolTipText");
			this.dockSearch.Visible = ((bool)(resources.GetObject("dockSearch.Visible")));
			// 
			// sandDockManager
			// 
			this.sandDockManager.DockingManager = TD.SandDock.DockingManager.Whidbey;
			this.sandDockManager.OwnerForm = this;
			this.sandDockManager.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
			// 
			// rightSandDock
			// 
			this.rightSandDock.AccessibleDescription = resources.GetString("rightSandDock.AccessibleDescription");
			this.rightSandDock.AccessibleName = resources.GetString("rightSandDock.AccessibleName");
			this.rightSandDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rightSandDock.Anchor")));
			this.rightSandDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rightSandDock.BackgroundImage")));
			this.rightSandDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rightSandDock.Dock")));
			this.rightSandDock.Enabled = ((bool)(resources.GetObject("rightSandDock.Enabled")));
			this.rightSandDock.Font = ((System.Drawing.Font)(resources.GetObject("rightSandDock.Font")));
			this.rightSandDock.Guid = new System.Guid("c6e4c477-596c-4e8c-9d35-840718d4c40d");
			this.helpProvider1.SetHelpKeyword(this.rightSandDock, resources.GetString("rightSandDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.rightSandDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("rightSandDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.rightSandDock, resources.GetString("rightSandDock.HelpString"));
			this.rightSandDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rightSandDock.ImeMode")));
			this.rightSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
			this.rightSandDock.Location = ((System.Drawing.Point)(resources.GetObject("rightSandDock.Location")));
			this.rightSandDock.Manager = this.sandDockManager;
			this.rightSandDock.Name = "rightSandDock";
			this.rightSandDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rightSandDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.rightSandDock, ((bool)(resources.GetObject("rightSandDock.ShowHelp"))));
			this.rightSandDock.Size = ((System.Drawing.Size)(resources.GetObject("rightSandDock.Size")));
			this.rightSandDock.TabIndex = ((int)(resources.GetObject("rightSandDock.TabIndex")));
			this.rightSandDock.Text = resources.GetString("rightSandDock.Text");
			this.toolTip.SetToolTip(this.rightSandDock, resources.GetString("rightSandDock.ToolTip"));
			this.rightSandDock.Visible = ((bool)(resources.GetObject("rightSandDock.Visible")));
			// 
			// bottomSandDock
			// 
			this.bottomSandDock.AccessibleDescription = resources.GetString("bottomSandDock.AccessibleDescription");
			this.bottomSandDock.AccessibleName = resources.GetString("bottomSandDock.AccessibleName");
			this.bottomSandDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("bottomSandDock.Anchor")));
			this.bottomSandDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bottomSandDock.BackgroundImage")));
			this.bottomSandDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("bottomSandDock.Dock")));
			this.bottomSandDock.Enabled = ((bool)(resources.GetObject("bottomSandDock.Enabled")));
			this.bottomSandDock.Font = ((System.Drawing.Font)(resources.GetObject("bottomSandDock.Font")));
			this.bottomSandDock.Guid = new System.Guid("9ffc7b96-a550-4e79-a533-8eee52ac0da1");
			this.helpProvider1.SetHelpKeyword(this.bottomSandDock, resources.GetString("bottomSandDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.bottomSandDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("bottomSandDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.bottomSandDock, resources.GetString("bottomSandDock.HelpString"));
			this.bottomSandDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("bottomSandDock.ImeMode")));
			this.bottomSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
			this.bottomSandDock.Location = ((System.Drawing.Point)(resources.GetObject("bottomSandDock.Location")));
			this.bottomSandDock.Manager = this.sandDockManager;
			this.bottomSandDock.Name = "bottomSandDock";
			this.bottomSandDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("bottomSandDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.bottomSandDock, ((bool)(resources.GetObject("bottomSandDock.ShowHelp"))));
			this.bottomSandDock.Size = ((System.Drawing.Size)(resources.GetObject("bottomSandDock.Size")));
			this.bottomSandDock.TabIndex = ((int)(resources.GetObject("bottomSandDock.TabIndex")));
			this.bottomSandDock.Text = resources.GetString("bottomSandDock.Text");
			this.toolTip.SetToolTip(this.bottomSandDock, resources.GetString("bottomSandDock.ToolTip"));
			this.bottomSandDock.Visible = ((bool)(resources.GetObject("bottomSandDock.Visible")));
			// 
			// topSandDock
			// 
			this.topSandDock.AccessibleDescription = resources.GetString("topSandDock.AccessibleDescription");
			this.topSandDock.AccessibleName = resources.GetString("topSandDock.AccessibleName");
			this.topSandDock.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("topSandDock.Anchor")));
			this.topSandDock.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("topSandDock.BackgroundImage")));
			this.topSandDock.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("topSandDock.Dock")));
			this.topSandDock.Enabled = ((bool)(resources.GetObject("topSandDock.Enabled")));
			this.topSandDock.Font = ((System.Drawing.Font)(resources.GetObject("topSandDock.Font")));
			this.topSandDock.Guid = new System.Guid("e1c62abd-0e7a-4bb6-aded-a74f27027165");
			this.helpProvider1.SetHelpKeyword(this.topSandDock, resources.GetString("topSandDock.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.topSandDock, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("topSandDock.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.topSandDock, resources.GetString("topSandDock.HelpString"));
			this.topSandDock.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("topSandDock.ImeMode")));
			this.topSandDock.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400);
			this.topSandDock.Location = ((System.Drawing.Point)(resources.GetObject("topSandDock.Location")));
			this.topSandDock.Manager = this.sandDockManager;
			this.topSandDock.Name = "topSandDock";
			this.topSandDock.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("topSandDock.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.topSandDock, ((bool)(resources.GetObject("topSandDock.ShowHelp"))));
			this.topSandDock.Size = ((System.Drawing.Size)(resources.GetObject("topSandDock.Size")));
			this.topSandDock.TabIndex = ((int)(resources.GetObject("topSandDock.TabIndex")));
			this.topSandDock.Text = resources.GetString("topSandDock.Text");
			this.toolTip.SetToolTip(this.topSandDock, resources.GetString("topSandDock.ToolTip"));
			this.topSandDock.Visible = ((bool)(resources.GetObject("topSandDock.Visible")));
			// 
			// _docContainer
			// 
			this._docContainer.AccessibleDescription = resources.GetString("_docContainer.AccessibleDescription");
			this._docContainer.AccessibleName = resources.GetString("_docContainer.AccessibleName");
			this._docContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_docContainer.Anchor")));
			this._docContainer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_docContainer.BackgroundImage")));
			this._docContainer.Controls.Add(this._docFeedDetails);
			this._docContainer.Cursor = System.Windows.Forms.Cursors.Default;
			this._docContainer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("_docContainer.Dock")));
			this._docContainer.DockingManager = TD.SandDock.DockingManager.Whidbey;
			this._docContainer.Enabled = ((bool)(resources.GetObject("_docContainer.Enabled")));
			this._docContainer.Font = ((System.Drawing.Font)(resources.GetObject("_docContainer.Font")));
			this._docContainer.Guid = new System.Guid("f032a648-4262-4312-ab2b-abe5094272bd");
			this.helpProvider1.SetHelpKeyword(this._docContainer, resources.GetString("_docContainer.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this._docContainer, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("_docContainer.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this._docContainer, resources.GetString("_docContainer.HelpString"));
			this._docContainer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_docContainer.ImeMode")));
			this._docContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 400, System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
																																											  new TD.SandDock.DocumentLayoutSystem(435, 395, new TD.SandDock.DockControl[] {
																																																															   this._docFeedDetails}, this._docFeedDetails)});
			this._docContainer.Location = ((System.Drawing.Point)(resources.GetObject("_docContainer.Location")));
			this._docContainer.Manager = null;
			this._docContainer.Name = "_docContainer";
			this._docContainer.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
			this._docContainer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_docContainer.RightToLeft")));
			this.helpProvider1.SetShowHelp(this._docContainer, ((bool)(resources.GetObject("_docContainer.ShowHelp"))));
			this._docContainer.Size = ((System.Drawing.Size)(resources.GetObject("_docContainer.Size")));
			this._docContainer.TabIndex = ((int)(resources.GetObject("_docContainer.TabIndex")));
			this._docContainer.Text = resources.GetString("_docContainer.Text");
			this.toolTip.SetToolTip(this._docContainer, resources.GetString("_docContainer.ToolTip"));
			this._docContainer.Visible = ((bool)(resources.GetObject("_docContainer.Visible")));
			// 
			// _docFeedDetails
			// 
			this._docFeedDetails.AccessibleDescription = resources.GetString("_docFeedDetails.AccessibleDescription");
			this._docFeedDetails.AccessibleName = resources.GetString("_docFeedDetails.AccessibleName");
			this._docFeedDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_docFeedDetails.Anchor")));
			this._docFeedDetails.AutoScroll = ((bool)(resources.GetObject("_docFeedDetails.AutoScroll")));
			this._docFeedDetails.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("_docFeedDetails.AutoScrollMargin")));
			this._docFeedDetails.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("_docFeedDetails.AutoScrollMinSize")));
			this._docFeedDetails.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_docFeedDetails.BackgroundImage")));
			this._docFeedDetails.Closable = false;
			this._docFeedDetails.Controls.Add(this.panelFeedDetails);
			this._docFeedDetails.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("_docFeedDetails.Dock")));
			this._docFeedDetails.Enabled = ((bool)(resources.GetObject("_docFeedDetails.Enabled")));
			this._docFeedDetails.Font = ((System.Drawing.Font)(resources.GetObject("_docFeedDetails.Font")));
			this._docFeedDetails.Guid = new System.Guid("9c7b7643-2ed3-402c-9e86-3c958341c81f");
			this.helpProvider1.SetHelpKeyword(this._docFeedDetails, resources.GetString("_docFeedDetails.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this._docFeedDetails, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("_docFeedDetails.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this._docFeedDetails, resources.GetString("_docFeedDetails.HelpString"));
			this._docFeedDetails.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_docFeedDetails.ImeMode")));
			this._docFeedDetails.Location = ((System.Drawing.Point)(resources.GetObject("_docFeedDetails.Location")));
			this._docFeedDetails.Name = "_docFeedDetails";
			this._docFeedDetails.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_docFeedDetails.RightToLeft")));
			this.helpProvider1.SetShowHelp(this._docFeedDetails, ((bool)(resources.GetObject("_docFeedDetails.ShowHelp"))));
			this._docFeedDetails.Size = ((System.Drawing.Size)(resources.GetObject("_docFeedDetails.Size")));
			this._docFeedDetails.TabIndex = ((int)(resources.GetObject("_docFeedDetails.TabIndex")));
			this._docFeedDetails.TabText = resources.GetString("_docFeedDetails.TabText");
			this._docFeedDetails.Text = resources.GetString("_docFeedDetails.Text");
			this.toolTip.SetToolTip(this._docFeedDetails, resources.GetString("_docFeedDetails.ToolTip"));
			this._docFeedDetails.ToolTipText = resources.GetString("_docFeedDetails.ToolTipText");
			this._docFeedDetails.Visible = ((bool)(resources.GetObject("_docFeedDetails.Visible")));
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
			// _timerResetStatus
			// 
			this._timerResetStatus.Interval = 5000;
			this._timerResetStatus.Tick += new System.EventHandler(this.OnTimerResetStatusTick);
			// 
			// helpProvider1
			// 
			this.helpProvider1.HelpNamespace = resources.GetString("helpProvider1.HelpNamespace");
			// 
			// WinGuiMain
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this._docContainer);
			this.Controls.Add(this.leftSandDock);
			this.Controls.Add(this.rightSandDock);
			this.Controls.Add(this.bottomSandDock);
			this.Controls.Add(this.topSandDock);
			this.Controls.Add(this.leftSandBarDock);
			this.Controls.Add(this.rightSandBarDock);
			this.Controls.Add(this.bottomSandBarDock);
			this.Controls.Add(this.topSandBarDock);
			this.Controls.Add(this.progressBrowser);
			this.Controls.Add(this._status);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.helpProvider1.SetHelpKeyword(this, resources.GetString("$this.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("$this.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this, resources.GetString("$this.HelpString"));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.KeyPreview = true;
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "WinGuiMain";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.Resize += new System.EventHandler(this.OnFormResize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.OnFormClosing);
			this.Load += new System.EventHandler(this.OnLoad);
			this.HandleCreated += new System.EventHandler(this.OnFormHandleCreated);
			this.Move += new System.EventHandler(this.OnFormMove);
			this.Activated += new System.EventHandler(this.OnFormActivated);
			this.Deactivate += new System.EventHandler(this.OnFormDeactivate);
			this.panelFeedDetails.ResumeLayout(false);
			this.panelWebDetail.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.htmlDetail)).EndInit();
			this.panelFeedItems.ResumeLayout(false);
			this.panelRssSearch.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.taskPaneSearchOptions)).EndInit();
			this.taskPaneSearchOptions.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelSearchNameEx)).EndInit();
			this.collapsiblePanelSearchNameEx.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelRssSearchExprKindEx)).EndInit();
			this.collapsiblePanelRssSearchExprKindEx.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelItemPropertiesEx)).EndInit();
			this.collapsiblePanelItemPropertiesEx.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelAdvancedOptionsEx)).EndInit();
			this.collapsiblePanelAdvancedOptionsEx.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.collapsiblePanelRssSearchScopeEx)).EndInit();
			this.collapsiblePanelRssSearchScopeEx.ResumeLayout(false);
			this.panelRssSearchCommands.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.statusBarBrowser)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarBrowserProgress)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarConnectionState)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarRssParser)).EndInit();
			this.topSandBarDock.ResumeLayout(false);
			this.leftSandDock.ResumeLayout(false);
			this.dockSubscriptions.ResumeLayout(false);
			this.dockSearch.ResumeLayout(false);
			this._docContainer.ResumeLayout(false);
			this._docFeedDetails.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._timerTreeNodeExpand)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._timerRefreshFeeds)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Owner Interaction routines
		
		/// <summary>
		/// Extended Close.
		/// </summary>
		/// <param name="forceShutdown"></param>
		public void Close(bool forceShutdown) {
			this.SaveUIConfiguration(forceShutdown);
			_forceShutdown = forceShutdown;
			base.Close();
		}

		public void SaveUIConfiguration(bool forceFlush) {
			try {
				this.OnSaveConfig(owner.GuiSettings);
				if (forceFlush) {
					this.listFeedItems.CheckForLayoutModifications();
					owner.GuiSettings.Flush();
				}

			} catch (Exception ex) {
				RssBanditApplication.PublishException(ex);
				_log.Error("Save .settings.xml failed", ex);
			}
		}

		public void InitiatePopulateTreeFeeds() {
		
			if(owner == null) {
				//Probably should log an error here
				this.SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
				return; 
			}
			if(owner.FeedHandler.FeedsListOK == false) { 
				this.SetGuiStateFeedback(Resource.Manager["RES_GUIStatusNoFeedlistFile"], ApplicationTrayState.NormalIdle);
				return; 
			}

			//Ensure we update the UI in the correct thread. Since this method is likely 
			//to have been called from a thread that is not the UI thread we should ensure 
			//that calls to UI components are actually made from the UI thread or marshalled
			//accordingly. 
			if(this.treeFeeds.InvokeRequired == true) {
				PopulateTreeFeedsDelegate loadTreeStatus  = new PopulateTreeFeedsDelegate(PopulateTreeFeeds);			
				this.Invoke(loadTreeStatus,new object[]{owner.FeedHandler.Categories, owner.FeedHandler.FeedsTable, RssBanditApplication.DefaultCategory});			
				this.Invoke(new MethodInvoker(this.PopulateTreeSpecialFeeds));			
			}
			else {
				this.PopulateTreeFeeds(owner.FeedHandler.Categories, owner.FeedHandler.FeedsTable, RssBanditApplication.DefaultCategory); 
				this.PopulateTreeSpecialFeeds(); 
			}
		
		}

		private void CheckForFlaggedNodeAndCreate(NewsItem ri) {

			ISmartFolder isf = null;
			FeedTreeNodeBase tn = null;
			FeedTreeNodeBase root = this.GetRoot(RootFolderType.SmartFolders);

			if (ri.FlagStatus == Flagged.FollowUp && _flaggedFeedsNodeFollowUp == null) {	// not yet created
				_flaggedFeedsNodeFollowUp = new FlaggedItemsNode(Flagged.FollowUp, owner.FlaggedItemsFeed, Resource.Manager["RES_FeedNodeFlaggedForFollowUpCaption"], 12, 12, _treeLocalFeedContextMenu);
				root.Nodes.Add(_flaggedFeedsNodeFollowUp);
				isf = _flaggedFeedsNodeFollowUp as ISmartFolder;
				tn = _flaggedFeedsNodeFollowUp;
				if (isf != null) isf.UpdateReadStatus();
			} else if (ri.FlagStatus == Flagged.Read && _flaggedFeedsNodeRead == null) {	// not yet created
				_flaggedFeedsNodeRead = new FlaggedItemsNode(Flagged.Read, owner.FlaggedItemsFeed, Resource.Manager["RES_FeedNodeFlaggedForReadCaption"], 14, 14, _treeLocalFeedContextMenu);
				root.Nodes.Add(_flaggedFeedsNodeRead);
				isf = _flaggedFeedsNodeRead as ISmartFolder;
				tn = _flaggedFeedsNodeRead;
				if (isf != null) isf.UpdateReadStatus();
			} else if (ri.FlagStatus == Flagged.Review && _flaggedFeedsNodeReview == null) {	// not yet created
				_flaggedFeedsNodeReview = new FlaggedItemsNode(Flagged.Review, owner.FlaggedItemsFeed, Resource.Manager["RES_FeedNodeFlaggedForReviewCaption"], 15, 15, _treeLocalFeedContextMenu);
				root.Nodes.Add(_flaggedFeedsNodeReview);
				isf = _flaggedFeedsNodeReview as ISmartFolder;
				tn = _flaggedFeedsNodeReview;
				if (isf != null) isf.UpdateReadStatus();
			} else if (ri.FlagStatus == Flagged.Forward && _flaggedFeedsNodeForward == null) {	// not yet created
				_flaggedFeedsNodeForward = new FlaggedItemsNode(Flagged.Forward, owner.FlaggedItemsFeed, Resource.Manager["RES_FeedNodeFlaggedForForwardCaption"], 13, 13, _treeLocalFeedContextMenu);
				root.Nodes.Add(_flaggedFeedsNodeForward);
				isf = _flaggedFeedsNodeForward as ISmartFolder;
				tn = _flaggedFeedsNodeForward;
				if (isf != null) isf.UpdateReadStatus();
			} else if (ri.FlagStatus == Flagged.Reply && _flaggedFeedsNodeReply == null) {	// not yet created
				_flaggedFeedsNodeReply = new FlaggedItemsNode(Flagged.Reply, owner.FlaggedItemsFeed, Resource.Manager["RES_FeedNodeFlaggedForReplyCaption"], 16, 16, _treeLocalFeedContextMenu);
				root.Nodes.Add(_flaggedFeedsNodeReply);
				isf = _flaggedFeedsNodeReply as ISmartFolder;
				tn = _flaggedFeedsNodeReply;
				if (isf != null) isf.UpdateReadStatus();
			}

			if (tn != null) {	// overall settings
				tn.Tag = owner.FlaggedItemsFeed.link +"?id=" + ri.FlagStatus.ToString();
			}
		
		}

		public void PopulateTreeSpecialFeeds() {
		
			treeFeeds.BeginUpdate();

			FeedTreeNodeBase root = this.GetRoot(RootFolderType.SmartFolders);
			root.Nodes.Clear();

			_feedExceptionsNode = new ExceptionReportNode(Resource.Manager["RES_FeedNodeFeedExceptionsCaption"], 6, 6, _treeLocalFeedContextMenu);
			_feedExceptionsNode.Tag = SpecialFeeds.ExceptionManager.GetInstance().link;
			this.ExceptionNode.UpdateReadStatus();

			_sentItemsNode = new SentItemsNode(owner.SentItemsFeed, 8, 8, _treeLocalFeedContextMenu);
			_sentItemsNode.Tag = owner.SentItemsFeed.link;
			this.SentItemsNode.UpdateReadStatus();

			_deletedItemsNode = new WasteBasketNode(owner.DeletedItemsFeed, 17, 17, _treeLocalFeedContextMenu );	
			_deletedItemsNode.Tag = owner.DeletedItemsFeed.link;
			this.DeletedItemsNode.UpdateReadStatus();

			root.Nodes.AddRange(new TreeNode[]{_feedExceptionsNode, _sentItemsNode, _deletedItemsNode});

			// method gets called more than once, reset the nodes:
			_flaggedFeedsNodeFollowUp = _flaggedFeedsNodeRead = null;
			_flaggedFeedsNodeReview = _flaggedFeedsNodeForward = null;
			_flaggedFeedsNodeReply = null;

			foreach (NewsItem ri in owner.FlaggedItemsFeed.Items) {
				
				CheckForFlaggedNodeAndCreate (ri);
				
				if (_flaggedFeedsNodeFollowUp != null && _flaggedFeedsNodeRead != null &&
					_flaggedFeedsNodeReview != null && _flaggedFeedsNodeForward != null && 
					_flaggedFeedsNodeReply != null) {
					break;
				}
			}

			root.Expand();

			FinderRootNode froot = (FinderRootNode)this.GetRoot(RootFolderType.Finder);
			this.SyncFinderNodes(froot);
			froot.ExpandAll();


			treeFeeds.EndUpdate();

		}

		public void SyncFinderNodes() {
			SyncFinderNodes((FinderRootNode)this.GetRoot(RootFolderType.Finder));
		}

		private void SyncFinderNodes(FinderRootNode finderRoot) {
			if (finderRoot == null)
				return;
			finderRoot.Nodes.Clear(); 
			finderRoot.InitFromFinders(owner.FinderList, _treeSearchFolderContextMenu);
		}

		public void PopulateFeedSubscriptions(CategoriesCollection categories, FeedsCollection feedsTable, string defaultCategory) {
			this.PopulateTreeFeeds(categories, feedsTable, defaultCategory);
		}
		public void PopulateTreeFeeds(CategoriesCollection categories, FeedsCollection feedsTable, string defaultCategory) {
			
			EmptyListView();

			treeFeeds.BeginUpdate();
			
			FeedTreeNodeBase root = this.GetRoot(RootFolderType.MyFeeds);
			// reset nodes and unread counter
			root.Nodes.Clear();
			root.UpdateReadStatus(root, 0);

			Hashtable categoryTable = new Hashtable(); 
			CategoriesCollection categoryList = (CategoriesCollection)categories.Clone();
			
			foreach(feedsFeed f in feedsTable.Values) {

				if (this.Disposing)
					return;

				FeedTreeNodeBase tn = new FeedNode(f.title, 4, 4, _treeFeedContextMenu);
				if (f.refreshrateSpecified && f.refreshrate <= 0) {
					tn.ImageIndex = tn.SelectedImageIndex = 5;	// disabled image
				} else if (f.authUser != null || f.link.StartsWith("https")) {
					tn.ImageIndex = tn.SelectedImageIndex = 9;	// image with lock 
				}
				
				//interconnect for speed:
				tn.Tag = f.link;
				f.Tag = tn;

				string category = (f.category == null ? String.Empty: f.category);
				
				FeedTreeNodeBase catnode;
				if (categoryTable.ContainsKey(category))
					catnode = (FeedTreeNodeBase)categoryTable[category];
				else {
					catnode = CreateCategoryHive(root, category);
					categoryTable.Add(category, catnode); 
				}
				
				catnode.Nodes.Add(tn);

				if(f.containsNewMessages)
					tn.UpdateReadStatus(tn, CountUnreadFeedItems(f));

				if (categoryList.ContainsKey(category))
					categoryList.Remove(category);
			}

			//add categories, we not already have
			foreach(string category in categoryList.Keys) {
				CreateCategoryHive(root, category);
			}

			treeFeeds.EndUpdate();
			
			if (this.Disposing)
				return;

			TreeSelectedNode = root; 
			root.Expand();

			// also this one:
			this.DelayTask(DelayedTasks.SyncRssSearchTree);

			this.SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
		}

		private void PopulateTreeRssSearchScope() {
			TreeHelper.CopyNodes(this.GetRoot(RootFolderType.MyFeeds), this.treeRssSearchScope, true);
			if (this.treeRssSearchScope.Nodes.Count > 0) {
				this.treeRssSearchScope.Nodes[0].Expand();
			}
		}

		/// <summary>
		/// Used to jump/navigate a web-link (url). Function will create 
		/// on demand a tabpage named in parameter <c>tab</c>, move it to front
		/// and open/navigate a web browser with the provided <c>url</c>.
		/// </summary>
		/// <param name="url">Web-Link to navigate to</param>
		/// <param name="tab">tabpage title name</param>
		public void DetailTabNavigateToUrl(string url, string tab) {
			this.DetailTabNavigateToUrl(url, tab, false);
		}
		public void DetailTabNavigateToUrl(string url, string tab, bool createNewTab) {
			this.DetailTabNavigateToUrl(url, tab, createNewTab, true);
		}
		public void DetailTabNavigateToUrl(string url, string tab, bool createNewTab, bool setFocus) {
			Debug.Assert(!this.InvokeRequired, "DetailTabNavigateToUrl() from Non-UI Thread called");
			
			if (StringHelper.EmptyOrNull(url)) 
				return;
			
			if (url == "about:blank" && !createNewTab)
				return;
			
			if (StringHelper.EmptyOrNull(tab)) 
				tab = "Web Link";

			HtmlControl hc = null;

			DockControl currentDoc = _docContainer.ActiveDocument;
			ITabState docState = (ITabState)currentDoc.Tag;
			
			if (!docState.CanClose) {	// Feed Detail doc tab

				if (!createNewTab && owner.Preferences.ReuseFirstBrowserTab) {
					
					foreach (DockControl c in currentDoc.LayoutSystem.Controls) {
						if (c != currentDoc) {
							// reuse first docTab not equal to news item listview container
							hc = (HtmlControl)c.Controls[0];
							break;
						}
					}
				}
			
			} else if (!createNewTab) {	// web doc tab
				// reuse same tab
				hc = (HtmlControl)_docContainer.ActiveDocument.Controls[0];
			} 

			if (hc  == null) {	// create new doc tab with a contained web browser
				hc = CreateAndInitIEControl(tab);
				DockControl doc = new DockControl(hc, tab);
				doc.Tag = new WebTabState(tab, url);
				hc.Tag = doc;		// store the doc the browser belongs to
				_docContainer.AddDocument(doc);
				if (Win32.IsOSAtLeastWindowsXP)
					ColorEx.ColorizeOneNote(doc, ++_webTabCounter);
				if (setFocus)
					hc.Activate();		// focus the new web content after navigation
			}

			currentDoc = (DockControl)hc.Tag;
			// move to front (check, if we really have to do do so
			if (_docContainer.ActiveDocument != currentDoc && setFocus)
				_docContainer.ActiveDocument = currentDoc;

			hc.Navigate(url);
		}

		private HtmlControl CreateAndInitIEControl(string tabName) {
			HtmlControl hc = new HtmlControl();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WinGuiMain));
			
			hc.BeginInit();
			// we just take over some generic resource settings from htmlDetail:
			hc.AccessibleDescription = resources.GetString("htmlDetail.AccessibleDescription");
			hc.AccessibleName = tabName;
			hc.AllowDrop = true;
			hc.ContainingControl = this;
			this.helpProvider1.SetHelpKeyword(hc, resources.GetString("htmlDetail.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(hc, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("htmlDetail.HelpNavigator"))));
			this.helpProvider1.SetHelpString(hc, resources.GetString("htmlDetail.HelpString"));
			hc.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("htmlDetail.ImeMode")));
			hc.Name = tabName;
			hc.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("htmlDetail.OcxState")));
			hc.RightToLeft = ((bool)(resources.GetObject("htmlDetail.RightToLeft")));
			this.helpProvider1.SetShowHelp(hc, ((bool)(resources.GetObject("htmlDetail.ShowHelp"))));
			hc.EndInit();

			hc.ScriptEnabled = owner.Preferences.BrowserJavascriptAllowed;
			hc.JavaEnabled = owner.Preferences.BrowserJavaAllowed;
			hc.ActiveXEnabled = owner.Preferences.BrowserActiveXAllowed;
			hc.BackroundSoundEnabled = owner.Preferences.BrowserBGSoundAllowed;
			hc.VideoEnabled = owner.Preferences.BrowserVideoAllowed;
			hc.ImagesDownloadEnabled = owner.Preferences.BrowserImagesAllowed;
			hc.SilentModeEnabled = !owner.Preferences.BrowserActiveXAllowed;

			hc.Border3d = true;

			hc.StatusTextChanged += new BrowserStatusTextChangeEventHandler(OnWebStatusTextChanged);
			hc.BeforeNavigate += new BrowserBeforeNavigate2EventHandler(OnWebBeforeNavigate);
			hc.NavigateComplete += new BrowserNavigateComplete2EventHandler(OnWebNavigateComplete);
			hc.DocumentComplete += new BrowserDocumentCompleteEventHandler(OnWebDocumentComplete);
			hc.TitleChanged += new BrowserTitleChangeEventHandler(OnWebTitleChanged);
			hc.CommandStateChanged += new BrowserCommandStateChangeEventHandler(OnWebCommandStateChanged);
			hc.NewWindow += new BrowserNewWindowEventHandler(OnWebNewWindow);
			hc.ProgressChanged += new BrowserProgressChangeEventHandler(OnWebProgressChanged);
			hc.TranslateAccelerator += new KeyEventHandler(OnWebTranslateAccelerator);
			hc.OnQuit +=new EventHandler(OnWebQuit);

			return hc;
		}

		private bool UrlRequestHandledExternally(string url, bool forceNewTab) {
			if (forceNewTab || BrowserBehaviorOnNewWindow.OpenNewTab == owner.Preferences.BrowserOnNewWindow)  {
				return false;
			} else if (BrowserBehaviorOnNewWindow.OpenDefaultBrowser == owner.Preferences.BrowserOnNewWindow) {
				owner.OpenUrlInExternalBrowser(url);
			} else if (BrowserBehaviorOnNewWindow.OpenWithCustomExecutable == owner.Preferences.BrowserOnNewWindow) {
				try {
					Process.Start(owner.Preferences.BrowserCustomExecOnNewWindow, url);
				} catch (Exception  ex) {
					if (owner.MessageQuestion("RES_ExceptionStartBrowserCustomExecMessage", owner.Preferences.BrowserCustomExecOnNewWindow, ex.Message, url) == DialogResult.Yes) {
						this.DetailTabNavigateToUrl(url, null, true);
					} 
				}
			} else {
				Debug.Assert(false, "Unhandled BrowserBehaviorOnNewWindow");
			} 
			return true;
		}

		/// <summary>
		/// Used to initiate a browse action.
		/// </summary>
		/// <param name="action">The specific action to perform</param>
		public void RequestBrowseAction(BrowseAction action) {
			
			if (_docContainer.ActiveDocument == _docFeedDetails) {
				ListViewItem lvi = null;
				if (listFeedItems.SelectedItems.Count > 0)
					lvi = listFeedItems.SelectedItems[0];
				
				switch (action) {
					case BrowseAction.NavigateBack:
						if  (lvi != null) {
							if (lvi.Index == 0)
								lvi = null;
							else
								lvi = listFeedItems.Items[lvi.Index-1];
						}
						break;
					case BrowseAction.NavigateForward:
						if  (lvi != null) {
							if ((lvi.Index + 1) >= listFeedItems.Items.Count)
								lvi = null;
							else
								lvi = listFeedItems.Items[lvi.Index+1];
						}
						break;
					case BrowseAction.DoRefresh:
						lvi = null;
						owner.CmdUpdateFeed(null);
						break;
					default:
						lvi = null;
						break;
				}

				if (lvi != null) {
					listFeedItems.SelectedItems.Clear();
					lvi.Selected = true;
					lvi.Focused = true;
					listFeedItems.EnsureVisible(lvi.Index); 
					this.OnFeedListItemActivate(null, EventArgs.Empty); //pass nulls because I don't use params
				}

			}
			else {
				HtmlControl wb = (HtmlControl)_docContainer.ActiveDocument.Controls[0];
				try {
					switch (action) {
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
							wb.Refresh();
							break;
						default:
							break;
					}
				}
				catch { /* Can't do command */ ;}
			}
			DeactivateWebProgressInfo();
		}

		public void RefreshListviewContextMenu() {
			NewsItem item = null; 
			if (listFeedItems.SelectedItems.Count > 0)
				item = ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key as NewsItem;
			if (listFeedItems.SelectedItems.Count == 1 && item != null) {
				 item = ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key as NewsItem;
				if (item.BeenRead) {
					owner.Mediator.SetVisible("+cmdMarkSelectedFeedItemsUnread" , "-cmdMarkSelectedFeedItemsRead");
				} else {
					owner.Mediator.SetVisible("-cmdMarkSelectedFeedItemsUnread" , "+cmdMarkSelectedFeedItemsRead");
				}
			} else {
				owner.Mediator.SetVisible("+cmdMarkSelectedFeedItemsUnread" , "+cmdMarkSelectedFeedItemsRead");
			}

			if (this.CurrentSelectedNode is WasteBasketNode) {
				owner.Mediator.SetVisible("+cmdRestoreSelectedNewsItems");
			} else {
				owner.Mediator.SetVisible("-cmdRestoreSelectedNewsItems");
			}
			
		}

		public void RefreshTreeFeedContextMenus(FeedTreeNodeBase node) {
			owner.Mediator.SetEnable(false, "cmdColumnChooserResetToDefault");
			if (node.Type == FeedNodeType.Feed || node.Type ==FeedNodeType.Category) {
				owner.Mediator.SetEnable(true, "cmdColumnChooserResetToDefault");
				owner.Mediator.SetEnable(
					"+cmdFlagNewsItem", "+cmdNavigateToFeedHome", "+cmdNavigateToFeedCosmos",
					"+cmdViewSourceOfFeed",	"+cmdValidateFeed");
			} else if	(node.Type == FeedNodeType.SmartFolder) {
				owner.Mediator.SetEnable(
					"-cmdFlagNewsItem", "-cmdNavigateToFeedHome",	"-cmdNavigateToFeedCosmos",
					"-cmdViewSourceOfFeed",	"-cmdValidateFeed");
				if ((node as FlaggedItemsNode) != null)
					owner.Mediator.SetEnable(	"+cmdFlagNewsItem");	// allow re-flag of items
			} else if (node.Type == FeedNodeType.Finder) {
				owner.Mediator.SetEnable("-cmdDeleteAllFinders","+cmdDeleteFinder", "+cmdShowFinderProperties", "+cmdFlagNewsItem", "-cmdSubscribeToFinderResult");
				FinderNode agfn = node as FinderNode;
				if (agfn != null && agfn == _searchResultNode && agfn.Finder != null ) {
					bool extResult = !StringHelper.EmptyOrNull(agfn.Finder.ExternalSearchUrl) ;
					owner.Mediator.SetEnable(extResult, "cmdSubscribeToFinderResult");
					owner.Mediator.SetEnable(extResult && agfn.Finder.ExternalResultMerged, "cmdShowFinderProperties");
				}
			} else if (node.Type == FeedNodeType.FinderCategory) {
				owner.Mediator.SetEnable("+cmdDeleteAllFinders","+cmdDeleteFinder", "-cmdShowFinderProperties");
			} else if (node.Type == FeedNodeType.Root) {
				if ((node as FinderRootNode) != null)
					owner.Mediator.SetEnable("+cmdDeleteAllFinders","-cmdDeleteFinder", "-cmdShowFinderProperties");
			}
		}

		private void MoveFeedDetailsToFront() {
			if (_docContainer.ActiveDocument != _docFeedDetails)
				_docContainer.ActiveDocument = _docFeedDetails;
		}

		private void RefreshDocumentState(DockControl doc) {

			if (doc == null)
				return; 

			ITabState state = doc.Tag as ITabState;
			if (state == null)
				return;

			if (_docContainer.ActiveDocument == doc) {
				SetTitleText(state.Title);
			}
			
			UrlText = state.Url;
				
			if (state.CanClose) {	// not listview/detail pane doc
				doc.Text = StringHelper.ShortenByEllipsis(state.Title, 30);
			}

			owner.Mediator.SetEnable(state.CanGoBack, "cmdBrowserGoBack");
			owner.Mediator.SetEnable(state.CanGoForward, "cmdBrowserGoForward");

		}

		public void SetGuiStateINetConnected(bool connected)	{
			try {
				StatusBarPanel p = statusBarConnectionState; //_status.Panels[2];
				if (connected) {
					p.Icon = Resource.Manager.LoadIcon("Resources.Connected.ico");
				} else {
					p.Icon = Resource.Manager.LoadIcon("Resources.Disconnected.ico");
				}
			}
			catch{}
			_status.Refresh();
		}

		public void SetGuiStateFeedback(string text)	{
			try {
				StatusBarPanel p = statusBarRssParser; //_status.Panels[3];
				if (!p.Text.Equals(text)) {
					p.Text = p.ToolTipText = text;
					if (text.Length == 0 && p.Icon != null) { 
						p.Icon = null; 
					}
					_status.Refresh();
				}
			}
			catch{}
		}
		public void SetGuiStateFeedback(string text, ApplicationTrayState state)	{
			try {
				StatusBarPanel p = statusBarRssParser; //_status.Panels[3];
				if (state == ApplicationTrayState.NormalIdle) {
					this._timerResetStatus.Start();
					if (!StringHelper.EmptyOrNull(text)) {
						this.SetGuiStateFeedback(text);
					}
				} else {
					this._timerResetStatus.Stop();
					this.SetGuiStateFeedback(text);
					_trayManager.SetState(state);
					if (state == ApplicationTrayState.BusyRefreshFeeds) {
						if (p.Icon == null) { p.Icon = Resource.Manager.LoadIcon("Resources.feedRefresh.ico"); _status.Refresh(); }
					} else {
						if (p.Icon != null) { p.Icon = null; _status.Refresh(); }
					}
				}
			}
			catch{}
		}

		public void SetBrowserStatusBarText(string text){
			try {
				StatusBarPanel p = statusBarBrowser; //_status.Panels[0];
				if (!p.Text.Equals(text)) {
					p.Text = text;
					_status.Refresh();
				}
			}
			catch{}
		}

		public void SetSearchStatusText(string text){
			SetGuiStateFeedback(text);
			this.labelRssSearchState.Text = text;
		}

		public void UpdateCategory(bool forceRefresh) {
			FeedTreeNodeBase selectedNode = CurrentSelectedNode; 
			if (selectedNode == null) return;

			owner.BeginRefreshCategoryFeeds(this.BuildCategoryStoreName(selectedNode), forceRefresh);
		}

		/// <summary>
		/// Call this.UpdateAllFeeds(true)
		/// </summary>
		public void UpdateAllFeeds() {
			this.UpdateAllFeeds(true); 					
		}

		/// <summary>
		/// Initiate a async. call to RssParser.RefreshFeeds(force_download)
		/// </summary>
		/// <param name="force_download"></param>
		public void UpdateAllFeeds(bool force_download) {
			if (_timerRefreshFeeds.Enabled)
				_timerRefreshFeeds.Stop();

			FeedTreeNodeBase root = this.GetRoot(RootFolderType.MyFeeds);
			_lastUnreadFeedItemCountBeforeRefresh = root.UnreadCount;
			owner.BeginRefreshFeeds(force_download);
		}

		public void OnAllAsyncUpdateFeedsFinished() {
#if !NOAUTO_REFRESH
			// restart the feeds auto-refresh timer:
			if (!_timerRefreshFeeds.Enabled)
				_timerRefreshFeeds.Start();
#endif
		}

		private void OnApplicationIdle(object sender, EventArgs e) {
			if (IdleTask.IsTask(IdleTasks.InitOnFinishLoading)) {
				Splash.Close();	
				IdleTask.RemoveTask(IdleTasks.InitOnFinishLoading);
				owner.BeginLoadingFeedlist();
				owner.BeginLoadingSpecialFeeds();
			}
		}

		/// <summary>
		/// Extracts the category of the selected node within the feeds tree.
		/// </summary>
		/// <returns>Category found, or DefaultCategory</returns>
		public string CategoryOfSelectedNode() {
			FeedTreeNodeBase tn = CurrentSelectedNode;

			if (tn != null) {
				if (tn.Type == FeedNodeType.Feed) {
					if (owner.FeedHandler.FeedsTable.ContainsKey((string)tn.Tag)) {
						feedsFeed f = owner.FeedHandler.FeedsTable[(string)tn.Tag];
						return f.category;
					} else {
						return BuildCategoryStoreName(CurrentSelectedNode);
					}
				} 
				else if (tn.Type == FeedNodeType.Category || tn.Type == FeedNodeType.Root) {
					return BuildCategoryStoreName(CurrentSelectedNode);
				}
				else {
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
		public void AddNewFeedNode(string category, feedsFeed f) {
			FeedTreeNodeBase catnode = null; 
			FeedTreeNodeBase tn = new FeedNode(f.title,4,4,_treeFeedContextMenu); 
			
			//interconnect for speed:
			tn.Tag = f.link;
			f.Tag = tn;
			
			category = (f.category == RssBanditApplication.DefaultCategory ? null : f.category); 
			catnode = CreateCategoryHive((FeedTreeNodeBase)treeFeeds.Nodes[0], category);

			if (catnode == null)
				catnode = this.GetRoot(RootFolderType.MyFeeds);
			
			catnode.Nodes.Add(tn);
			tn.EnsureVisible();

			this.DelayTask(DelayedTasks.SyncRssSearchTree);
		}

		public void InitiateRenameFeedOrCategory() {	
			if(this.CurrentSelectedNode!= null)
				this.DoEditTreeNodeLabel();
		}

		public bool NodeEditingActive {
			get { return (this.TreeSelectedNode != null && this.TreeSelectedNode.IsEditing); }
		}

		/// <summary>
		/// called async from InitiateRenameFeedOrCategory()
		/// </summary>
		public void DeleteFeed() {	
			this.DeleteFeed(CurrentSelectedNode);
		}
		protected void DeleteFeed(FeedTreeNodeBase tn) {
			if (tn == null) tn = CurrentSelectedNode;
			if (tn == null) return;
			if(tn.Type != FeedNodeType.Feed) return;
			tn.UpdateReadStatus(tn, 0);

			if (tn.IsSelected) {
				this.EmptyListView();					
				this.htmlDetail.Clear();
				TreeSelectedNode = this.GetRoot(RootFolderType.MyFeeds);
			}
			
			try {
				feedsFeed f = owner.FeedHandler.FeedsTable[(string)tn.Tag] as feedsFeed;
				if (f != null) {	
					f.Tag = null; 
					owner.FeedHandler.DeleteFeedFromCommonFeedList(f, f.category); 
				}
				tn.Parent.Nodes.Remove(tn); 
				owner.FeedHandler.DeleteFeed((string)tn.Tag);
				 
				this.DelayTask(DelayedTasks.SyncRssSearchTree);
			} catch { /* ignore delete errors (may raised by FileCacheManager) */}
		}


		private bool NodeIsChildOf(TreeNode tn, TreeNode parent) {
			if (parent == null)
				return false;

			TreeNode p = tn.Parent;
			while (p != null) {
				if (p == parent) return true;
				p = p.Parent;
			}
			return false;
		}

		/// <summary>
		/// Called on each finished successful feed refresh.
		/// </summary>
		/// <param name="feedUri">The original feed Uri</param>
		/// <param name="newFeedUri">The new feed Uri (if permamently moved)</param>
		/// <param name="items">The feed items retrieved</param>
		public void UpdateFeed(Uri feedUri, Uri newFeedUri, ArrayList items, bool modified) {			

			string feedUrl = feedUri.ToString();
			feedsFeed feed = null;
			FeedTreeNodeBase tn = null;

			if (!owner.FeedHandler.FeedsTable.Contains(feedUrl) && (feedUri.IsFile || feedUri.IsUnc)) {
				feedUrl = feedUri.LocalPath;
			}

			feed = owner.FeedHandler.FeedsTable[feedUrl] as feedsFeed;
			
			if (feed != null) {
				tn = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), feed);
			} else {
				tn = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), feedUrl);
			}

			if (tn != null) {

				if (newFeedUri != null && newFeedUri != feedUri) {	// changed/moved
					if (newFeedUri.IsFile || newFeedUri.IsUnc) 
						feedUrl = newFeedUri.LocalPath;
					else
						feedUrl = newFeedUri.ToString();
					tn.Tag = feedUrl;	
					feed = owner.FeedHandler.FeedsTable[feedUrl] as feedsFeed;
					if (feed != null) feed.Tag = tn;
				}

				if (feed != null) {
					if (feed.refreshrateSpecified && feed.refreshrate <= 0 && tn.ImageIndex != 5) {
						tn.ImageIndex = tn.SelectedImageIndex = 5;	// disabled image
					} else if (feed.authUser != null || feed.link.StartsWith("https") && tn.ImageIndex != 9) {
						tn.ImageIndex = tn.SelectedImageIndex = 9;	// image with lock 
					} else if (tn.ImageIndex != 4) {
						tn.ImageIndex = tn.SelectedImageIndex = 4;// normal
					}

					if (feed.containsNewMessages)	{
						if ((DisplayFeedAlertWindow.All == owner.Preferences.ShowAlertWindow || 
							(DisplayFeedAlertWindow.AsConfiguredPerFeed == owner.Preferences.ShowAlertWindow && feed.alertEnabled)) &&
							modified) {	// new flag on feed, states if toast is enabled (off by default)
							// we have to sort items first (newest on top)
							items.Sort(RssHelper.GetComparer());
							_toastNotifier.Alert(tn.Text, tn.UnreadCount, items);
						}
						tn.UpdateReadStatus(tn, CountUnreadFeedItems(items));
					}
				}

				bool categorized = false;
				FeedTreeNodeBase ftnSelected = TreeSelectedNode;

				if (ftnSelected.Type == FeedNodeType.Category && NodeIsChildOf(tn, ftnSelected))
					categorized = true;

				if (modified && (tn.IsSelected || categorized) ) {

					NewsItem itemSelected = null;
					if (listFeedItems.SelectedItems.Count > 0) 
						itemSelected = (NewsItem)((ThreadedListViewItem)listFeedItems.SelectedItems[0]).Key as NewsItem;

					this.PopulateListView(tn, items, false, categorized, ftnSelected ); 
								
					if (itemSelected == null || (itemSelected != null && !categorized && !itemSelected.Feed.link.Equals(tn.Tag)) ) {
						//clear browser pane 
						CurrentSelectedFeedItem = null;
						htmlDetail.Clear();
						RefreshFeedDisplay(tn); 					
					} else 
						ReSelectListViewItem(itemSelected);

				}
				
				// apply finder matches to refresh node unread state(s)
				UpdateFindersReadStatus(items);

			} else {
				_log.Info("UpdateFeed() could not find node for '"+feedUri.ToString()+"'...");
			}
		}

		private void UpdateFindersReadStatus(ArrayList items) {
			// apply finder matches to refresh the read state only
			if (_searchResultNode != null && !_searchResultNode.AnyUnread) {
				SearchCriteriaCollection sc = _searchResultNode.Finder.SearchCriterias;
				foreach (NewsItem item in items) {
					if (!item.BeenRead && sc.Match(item)) {	// match unread only is enough here
						_searchResultNode.UpdateReadStatus(_searchResultNode, true);
						break;
					}
				}//foreach
			}

			foreach (RssFinder finder in owner.FinderList) {
				if (finder.Container != null && !finder.Container.AnyUnread) {
					SearchCriteriaCollection sc = finder.SearchCriterias;
					foreach (NewsItem item in items) {
						if (!item.BeenRead && sc.Match(item)) {	// match unread only is enough here
							finder.Container.UpdateReadStatus(finder.Container, true);
							break;
						}
					}//foreach
				}
			}//foreach
		
		}

		private void ResetFindersReadStatus() {
			if (_searchResultNode != null) {
				_searchResultNode.UpdateReadStatus(_searchResultNode, 0);
			}
			foreach (RssFinder finder in owner.FinderList) {
				if (finder.Container != null)
					finder.Container.UpdateReadStatus(finder.Container, 0);
			}
		}

		public void NewCategory() {
			if (CurrentSelectedNode != null && CurrentSelectedNode.AllowedChild(FeedNodeType.Category)) {
				FeedTreeNodeBase curNode = CurrentSelectedNode;
				
				int i = 1;
				string s = Resource.Manager["RES_GeneralNewItemText"];
				// check for duplicate names:
				while (FindChild(curNode, s, FeedNodeType.Category) != null) {	
					s = Resource.Manager["RES_GeneralNewItemTextWithCounter", i++]; 
				}

				FeedTreeNodeBase newNode = new CategoryNode(s, 2, 3, _treeCategoryContextMenu);
				
				curNode.Nodes.Add(newNode);
				newNode.EnsureVisible();
				TreeSelectedNode = newNode;
				s = BuildCategoryStoreName(newNode);

				if(!owner.FeedHandler.Categories.ContainsKey(s)) {
					owner.FeedHandler.Categories.Add(s); 
					owner.FeedlistModified = true;
				}

				if (!treeFeeds.Focused) treeFeeds.Focus();
				newNode.BeginEdit();
			
			}
		}
		
		public void DeleteCategory() {
			this.DeleteCategory(CurrentSelectedNode);
			owner.FeedlistModified = true;
		}

		/// <summary>
		/// Can be called on every selected tree node.
		/// </summary>
		public void MarkSelectedNodeRead(FeedTreeNodeBase startNode) {
			FeedTreeNodeBase selectedNode = startNode == null ? CurrentSelectedNode: startNode;

			if (selectedNode == null) return;

			feedsFeed f = null;
			if (selectedNode.Type == FeedNodeType.Feed && owner.FeedHandler.FeedsTable.ContainsKey((string)selectedNode.Tag))
				f = owner.FeedHandler.FeedsTable[(string)selectedNode.Tag];
			
			if (f != null) {
				owner.FeedHandler.MarkAllCachedItemsAsRead(f);
				selectedNode.UpdateReadStatus(selectedNode, 0 );
			}
			
			bool selectedIsChild = this.NodeIsChildOf(TreeSelectedNode, selectedNode);
			bool isSmartOrAggregated = (selectedNode.Type == FeedNodeType.Finder ||
				selectedNode.Type == FeedNodeType.SmartFolder);

			//mark all viewed stories as read 
			// May be we are wrong here: how about a threaded item reference
			// with an ownerfeed, that is not a child of the current selectedNode?
			if (listFeedItems.Items.Count > 0) {

				listFeedItems.BeginUpdate(); 
					
				for (int i = 0; i < listFeedItems.Items.Count; i++) {
						
					ThreadedListViewItem lvi = listFeedItems.Items[i];
					NewsItem NewsItem = (NewsItem)lvi.Key;

					if (NewsItem != null && (NewsItem.Feed == f || selectedIsChild || selectedNode == TreeSelectedNode || isSmartOrAggregated || lvi.IndentLevel > 0 || selectedNode.Type == FeedNodeType.Root)) {	
						// switch image back
						if ((lvi.ImageIndex % 2) != 0) 
							lvi.ImageIndex--;

						// switch font back
						ApplyStyles(lvi, true);

						if (!NewsItem.BeenRead){
								
							NewsItem.BeenRead = true;

							// now update tree state of rss items from different
							// feeds (or also: category selected)
							if (lvi.IndentLevel > 0 || selectedNode.Type == FeedNodeType.Finder)	{
									
								// corresponding node can be at any hierarchy level
								selectedNode.UpdateReadStatus(GetTreeNodeForItem(GetRoot(RootFolderType.MyFeeds), NewsItem) , -1);

							} else if (selectedNode.Type != FeedNodeType.Feed) { 
									
								// can only be a child node, or SmartFolder
								if (NewsItem.Feed.containsNewMessages == true) { //if not yet handled
									FeedTreeNodeBase itemNode = GetTreeNodeForItem(selectedNode, NewsItem);
									if (itemNode != null) {
										itemNode.UpdateReadStatus(itemNode , 0);
										NewsItem.Feed.containsNewMessages = false;
									}
								}

							}

						}//if (!readed
					}//item belongs to feed 
					//else {
					//Trace.WriteLine("Does not belong to node: "+NewsItem.Title);
					//}
				}//for (i=0...

				listFeedItems.EndUpdate(); 

			}

			if (selectedNode.Type == FeedNodeType.Root) {	// all
				owner.FeedHandler.MarkAllCachedItemsAsRead(); 
				this.UpdateTreeStatus(owner.FeedHandler.FeedsTable);
				this.ResetFindersReadStatus();
				this.SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
			} 
			else if (selectedNode.Type == FeedNodeType.Category) { // category and childs
				WalkdownAndCatchupCategory(selectedNode);
			}
			if (isSmartOrAggregated) {
				ISmartFolder sfNode = startNode as ISmartFolder;
				if (sfNode != null) sfNode.UpdateReadStatus();
			}

			
		}

		/// <summary>
		/// Marks the selected feed items flagged. Called from the listview
		/// context menu.
		/// </summary>
		public void MarkFeedItemsFlagged(Flagged flag){
		
			NewsItem item = null;

			for (int i=0; i < listFeedItems.SelectedItems.Count; i++) {
				ThreadedListViewItem selectedItem = (ThreadedListViewItem)listFeedItems.SelectedItems[i];
				
				item = (NewsItem) selectedItem.Key; 
				
				if (item.FlagStatus == flag)
					return;	// no change

				item.FlagStatus = flag;
				
				ApplyFlagIconTo(selectedItem, item);

				//font styles merged, color overrides
				if(item.FlagStatus != Flagged.None){
					selectedItem.Font = FontColorHelper.MergeFontStyles(selectedItem.Font, FontColorHelper.HighlightStyle);
					selectedItem.ForeColor = FontColorHelper.HighlightColor;
				}

				CheckForFlaggedNodeAndCreate(item);

				if ((this.CurrentSelectedNode as FlaggedItemsNode) != null) {
					owner.ReFlagNewsItem(item);
				} else {
					owner.FlagNewsItem(item);
				}

			}//for(i=0...

			if (this.FlaggedFeedsNode(flag) != null) { // ReFlag may remove also items
				this.FlaggedFeedsNode(flag).UpdateReadStatus();
			}
		}

		
		/// <summary>
		/// Remove the selected feed items. 
		/// Called from the listview context menu.
		/// </summary>
		public void RemoveSelectedFeedItems(){
		
			if (listFeedItems.SelectedItems.Count == 0)
				return;

			// where we are?
			FeedTreeNodeBase thisNode = TreeSelectedNode;
			ISmartFolder isFolder = thisNode as ISmartFolder;

			int unreadItemsCount = 0;
			int itemIndex = 0;
			bool anyUnreadItem = false;
			Trace.WriteLine("RemoveSelectedFeedItems() gets called: " + unreadItemsCount.ToString() );

			try {
				listFeedItems.BeginUpdate();

				int delCounter = listFeedItems.SelectedItems.Count;
				while (delCounter-- > 0) {
					ThreadedListViewItem selectedItem = (ThreadedListViewItem)listFeedItems.SelectedItems[0];
					
					if (selectedItem.IndentLevel > 0)
						continue;	// do not delete selected childs

					if (selectedItem.HasChilds && selectedItem.Expanded) {
						// also remove the childs
						int j = selectedItem.Index+1;
						if (j < listFeedItems.Items.Count) {
							lock (listFeedItems.Items){
								ThreadedListViewItem child = listFeedItems.Items[j];
								while (child != null && child.IndentLevel > selectedItem.IndentLevel) {
									listFeedItems.Items.Remove(child);
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
				
					NewsItem item = (NewsItem) selectedItem.Key; 

					if (item == null)
						continue;
				
					owner.DeleteNewsItem(item);

					if (thisNode.Type == FeedNodeType.Category) {
						if (!item.BeenRead) {	// update unread counter(s)
							anyUnreadItem = true;
							FeedTreeNodeBase n = GetTreeNodeForItem(thisNode, item);
							n.UpdateReadStatus(n, -1);
						}
					} else if (isFolder == null) {
						isFolder = GetTreeNodeForItem(GetRoot(RootFolderType.SmartFolders), item) as ISmartFolder;
					}

					if (!item.BeenRead) { 
						anyUnreadItem = true;
						unreadItemsCount++;
					}

					if (isFolder != null) 
						owner.RemoveItemFromSmartFolder(isFolder, item);
					
					lock (listFeedItems.Items){
						listFeedItems.Items.Remove(selectedItem);
					}
				
				}//while
			
			} finally {
				listFeedItems.EndUpdate();
			}

			Trace.WriteLine("RemoveSelectedFeedItems() now may UpdateReadStatus: " + unreadItemsCount.ToString() );
			if (unreadItemsCount > 0) {
				thisNode.UpdateReadStatus(thisNode, -unreadItemsCount);
			}

			if (anyUnreadItem)
				this.DeletedItemsNode.UpdateReadStatus();

			// try to select another item:
			if (listFeedItems.Items.Count > 0 && listFeedItems.SelectedItems.Count == 0) {
				
			/*	itemIndex--;

				if (itemIndex < 0) {
					itemIndex = 0;
				} else */ if ((itemIndex != 0) && (itemIndex >= listFeedItems.Items.Count)) {
					itemIndex = listFeedItems.Items.Count - 1;
				}

				listFeedItems.Items[itemIndex].Selected = true;
				listFeedItems.Items[itemIndex].Focused = true;
				
				this.OnFeedListItemActivate(this, EventArgs.Empty);
			
			} else if (listFeedItems.SelectedItems.Count > 0) {	// still selected not deleted items:

				this.OnFeedListItemActivate(this, EventArgs.Empty);

			} else {	// no items:
				htmlDetail.Clear();
			}
		}
		
		/// <summary>
		/// Restore the selected feed items from the Wastebasket. 
		/// Called from the listview context menu.
		/// </summary>
		public void RestoreSelectedFeedItems(){
			
			if (listFeedItems.SelectedItems.Count == 0)
				return;

			FeedTreeNodeBase thisNode = TreeSelectedNode;
			ISmartFolder isFolder = thisNode as ISmartFolder;

			if (!(isFolder is WasteBasketNode))
				return;

			int itemIndex = 0;
			bool anyUnreadItem = false;

			try {
				listFeedItems.BeginUpdate();

				while (listFeedItems.SelectedItems.Count > 0) {
					ThreadedListViewItem selectedItem = (ThreadedListViewItem)listFeedItems.SelectedItems[0];
					
					if (selectedItem.IndentLevel > 0)
						continue;	// do not delete selected childs

					if (selectedItem.HasChilds && selectedItem.Expanded) {
						// also remove the childs
						int j = selectedItem.Index+1;
						if (j < listFeedItems.Items.Count) {
							lock (listFeedItems.Items){
								ThreadedListViewItem child = listFeedItems.Items[j];
								while (child != null && child.IndentLevel > selectedItem.IndentLevel) {
									listFeedItems.Items.Remove(child);
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
				
					NewsItem item = (NewsItem) selectedItem.Key; 
					
					if (item == null)
						continue;
				
					FeedTreeNodeBase originalContainerNode = owner.RestoreNewsItem(item);

					if (null != originalContainerNode &&  ! item.BeenRead) {
						anyUnreadItem = true;
						originalContainerNode.UpdateReadStatus(originalContainerNode, 1);
					}

					if (null == originalContainerNode) {	// 
						_log.Error("Item could not be restored, maybe the container feed was removed meanwhile: " + item.Title);
					}

					lock (listFeedItems.Items){
						listFeedItems.Items.Remove(selectedItem);
					}
				
				} //while
			
			} finally {
				listFeedItems.EndUpdate();
			}

			if (anyUnreadItem)
				this.DeletedItemsNode.UpdateReadStatus();

			// try to select another item:
			if (listFeedItems.Items.Count > 0) {
				
				itemIndex--;

				if (itemIndex < 0) {
					itemIndex = 0;
				} else if (itemIndex >= listFeedItems.Items.Count) {
					itemIndex = listFeedItems.Items.Count - 1;
				}

				listFeedItems.Items[itemIndex].Selected = true;
				listFeedItems.Items[itemIndex].Focused = true;
				
				this.OnFeedListItemActivate(this, EventArgs.Empty);
			
			} else {	// no items:
				htmlDetail.Clear();
			}
		}

		/// <summary>
		/// Marks the selected listview items read. Called from the listview
		/// context menu.
		/// </summary>
		public void MarkSelectedItemsLVRead() {
			this.SetFeedItemsReadState(listFeedItems.SelectedItems, true);
		}

		/// <summary>
		/// Marks the selected listview items unread. Called from the listview
		/// context menu.
		/// </summary>
		public void MarkSelectedItemsLVUnread() {
			this.SetFeedItemsReadState(listFeedItems.SelectedItems, false);
		}
		
		/// <summary>
		/// Marks the all listview items read. Called from the listview
		/// context menu.
		/// </summary>
		public void MarkAllItemsLVRead() {
			this.SetFeedItemsReadState(listFeedItems.Items, true);
		}

		/// <summary>
		/// Marks the all listview items unread. Called from the listview
		/// context menu.
		/// </summary>
		public void MarkAllItemsLVUnread() {
			this.SetFeedItemsReadState(listFeedItems.Items, false);
		}

		private void ApplyStyles(ThreadedListViewItem item) {
			if (item != null) {
				NewsItem n = (NewsItem) item.Key;
				if (n != null)
					ApplyStyles(item, n.BeenRead);
			}
		}

		private void ApplyStyles(ThreadedListViewItem item, bool beenRead) {
			if (item != null) {
				if (beenRead) {
					item.Font = FontColorHelper.NormalFont;
					item.ForeColor = FontColorHelper.NormalColor;
				} else {
					item.Font = FontColorHelper.UnreadFont;
					item.ForeColor = FontColorHelper.UnreadColor;
				}
				_filterManager.Apply(item);
			}
		}

		/// <summary>
		/// Marks the selected feed items read/unread. Called from the listview
		/// context menu.
		/// </summary>
		/// <param name="beenRead"></param>
		public void SetFeedItemsReadState(IList items, bool beenRead) {

			ArrayList modifiedItems = new ArrayList(listFeedItems.SelectedItems.Count);
			int amount = (beenRead ? -1: 1); 

			for  (int i=0; i < items.Count; i++) {
				
				ThreadedListViewItem selectedItem = (ThreadedListViewItem) items[i];
				NewsItem item = (NewsItem) selectedItem.Key; 
				
				ApplyStyles(selectedItem, beenRead);

				if (item.BeenRead != beenRead) {
					item.BeenRead = beenRead; 		
					selectedItem.ImageIndex += amount;	
					modifiedItems.Add(item);
				}
			}

			if (modifiedItems.Count > 0) {
				
				ArrayList deepModifiedItems = new ArrayList();
				int unexpectedImageState = (beenRead ? 1: 0); // unread-state images always have odd index numbers, read-state are even

				// if there is a self-reference thread, we also have to switch the Gui state for them back
				// these items can also be unselected.
				for  (int i=0; i < listFeedItems.Items.Count; i++) {

					ThreadedListViewItem th =   listFeedItems.Items[i];
					NewsItem selfRef = th.Key as NewsItem;

					foreach (NewsItem modifiedItem in modifiedItems) {
						
						if (modifiedItem.Equals(selfRef) && (th.ImageIndex % 2) == unexpectedImageState) {	

							ApplyStyles(th, beenRead);
							th.ImageIndex += amount;	

							if (selfRef.BeenRead != beenRead) {	// object ref is unequal, but other criteria match the item to be equal...
								selfRef.BeenRead = beenRead;	
								deepModifiedItems.Add(selfRef);
							}

						}
					}
				}

				modifiedItems.AddRange(deepModifiedItems);
				// we store stories-read in the feedlist, so enable save the new state 
				owner.FeedlistModified = true;	
				// and apply mods. to finders:
				UpdateFindersReadStatus(modifiedItems);
			
			}

			ISmartFolder sf = CurrentSelectedNode as ISmartFolder;
			if (sf != null) {

				sf.UpdateReadStatus();
			
			} else {

				// now update tree state of rss items from any
				// feed (also: category selected)

				Hashtable lookup = new Hashtable(modifiedItems.Count);

				foreach (NewsItem item in modifiedItems) {
				
					string feedurl = item.Feed.link;

					if (feedurl != null)	{

						FeedTreeNodeBase refNode = lookup[feedurl] as FeedTreeNodeBase;
						if (refNode == null) {
							// corresponding node can be at any hierarchy level, or temporary (if commentRss)
							if (owner.FeedHandler.FeedsTable.Contains(feedurl))
								refNode = GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), item);
							else 
								refNode = GetTreeNodeForItem(this.GetRoot(RootFolderType.SmartFolders), item);
						}

						if (refNode != null) {
							if (!lookup.ContainsKey(feedurl)) {
								lookup.Add(feedurl, refNode);		// speedup node lookup
							}
							refNode.UpdateReadStatus(refNode, refNode.UnreadCount + amount);
							item.Feed.containsNewMessages = (refNode.UnreadCount > 0);
						} else {
							// temp. (item comments)
							string hash = RssHelper.GetHashCode(item);
							if (tempFeedItemsRead.ContainsKey(hash))
								tempFeedItemsRead.Remove(hash);
						}

					} 
				}
			}
		}

		/// <summary>
		/// Moves a node to a new parent. 
		/// </summary>
		/// <param name="theNode">FeedTreeNodeBase to move.</param>
		/// <param name="target">New Parent FeedTreeNodeBase.</param>
		public void MoveNode(FeedTreeNodeBase theNode, FeedTreeNodeBase target) {
			
			if (theNode == null || target == null)
				return;

			if(theNode == target)
				return; 

			if (theNode.Type == FeedNodeType.Feed) {
				feedsFeed f = owner.FeedHandler.FeedsTable[(string)theNode.Tag];
				string category = BuildCategoryStoreName(target); 
				f.category  = category; 
				owner.FeedlistModified = true;
				if (category != null && !owner.FeedHandler.Categories.ContainsKey(category))
					owner.FeedHandler.Categories.Add(category);

				treeFeeds.BeginUpdate();

				if (theNode.UnreadCount > 0)
					theNode.UpdateReadStatus(theNode.Parent, -theNode.UnreadCount);
						
				theNode.Parent.Nodes.Remove(theNode);
				target.Nodes.Add(theNode); 

				if (theNode.UnreadCount > 0)
					theNode.UpdateReadStatus(theNode.Parent, theNode.UnreadCount);

				theNode.EnsureVisible();
				treeFeeds.EndUpdate();

			} else if (theNode.Type == FeedNodeType.Category) {

				string targetCategory = BuildCategoryStoreName(target); 
				string sourceCategory = BuildCategoryStoreName(theNode); 

				// refresh category store
				if (sourceCategory != null && owner.FeedHandler.Categories.ContainsKey(sourceCategory))
					owner.FeedHandler.Categories.Remove(sourceCategory);
				// target is the root node:
				if (targetCategory == null && !owner.FeedHandler.Categories.ContainsKey(theNode.Key))
					owner.FeedHandler.Categories.Add(theNode.Key);
				// target is another category node:
				if (targetCategory != null && !owner.FeedHandler.Categories.ContainsKey(targetCategory))
					owner.FeedHandler.Categories.Add(targetCategory);

				treeFeeds.BeginUpdate();

				if (theNode.UnreadCount > 0)
					theNode.UpdateReadStatus(theNode.Parent, -theNode.UnreadCount);
					
				theNode.Parent.Nodes.Remove(theNode);
				target.Nodes.Add(theNode); 

				// reset category references on feeds - after moving node to 
				// have the correct FullPath info within this call:
				WalkdownThenRenameFeedCategory(theNode, targetCategory);
				owner.FeedlistModified = true;

				if (theNode.UnreadCount > 0)
					theNode.UpdateReadStatus(theNode.Parent, theNode.UnreadCount);

				theNode.EnsureVisible();
				treeFeeds.EndUpdate();

			} else {

				Debug.Assert(false, "MoveNode(): unhandled NodeType:'"+theNode.Type.ToString());

			}

		}

		/// <summary>
		/// Calls/Open the newFeedDialog on the GUI thread, if required.
		/// </summary>
		/// <param name="newFeedUrl">Feed Url to add</param>
		public void AddFeedUrlSynchronized( string newFeedUrl) {
			if (this.treeFeeds.InvokeRequired) {
				SubscribeToFeedUrlDelegate helper = new SubscribeToFeedUrlDelegate(this.AddFeedUrlSynchronized);
				this.Invoke(helper, new object[]{newFeedUrl});
			} else {
				newFeedUrl = owner.HandleUrlFeedProtocol(newFeedUrl);
				owner.CmdNewFeed(null, newFeedUrl, null);	 
			}
		}

		public void OnFeedUpdateStart(Uri feedUri, ref bool cancel) {
			string feedUrl = null;
			FeedTreeNodeBase node = null;

			if (feedUri.IsFile || feedUri.IsUnc) 
				feedUrl = feedUri.LocalPath;
			else
				feedUrl = feedUri.ToString();
			
			feedsFeed f = owner.FeedHandler.FeedsTable[feedUrl] as feedsFeed;
			if (f != null) {
				node = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), f);
			} else {
				node = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), feedUrl);
			}
			if (node != null)  {
				if (node.ImageIndex != 11) {
					node.ImageIndex = node.SelectedImageIndex = 11;
				}
			}
		}

		public void OnFeedUpdateFinishedWithException(Uri feedUri, Exception exception) {
			
			string feedUrl = null;
			FeedTreeNodeBase node = null;

			if (feedUri.IsFile || feedUri.IsUnc) 
				feedUrl = feedUri.LocalPath;
			else
				feedUrl = feedUri.ToString();

			feedsFeed f = owner.FeedHandler.FeedsTable[feedUrl] as feedsFeed;
			if (f != null) {
				node = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), f);
			} else {
				node = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), feedUrl);
			}

			if (node != null) {
				if (node.ImageIndex != 6) {
					node.ImageIndex = node.SelectedImageIndex = 6;
				}
			}
		}

		public void OnRequestCertificateIssue(object sender, CertificateIssueCancelEventArgs e) {

			e.Cancel = true;	// by default: do not continue on certificate problems

			if (!this.Visible)	// do not bother if hidden. Just go on an report the issue as a feed error
				return;

			Uri requestUri = e.WebRequest.RequestUri;
			string feedUrl = null;

			if (requestUri.IsFile || requestUri.IsUnc) 
				feedUrl = requestUri.LocalPath;
			else
				feedUrl = requestUri.ToString();

			feedsFeed f = owner.FeedHandler.FeedsTable[feedUrl] as feedsFeed;
			string issueCaption = null, issueDesc = null;

			if (f != null) {
				issueCaption = Resource.Manager["RES_CertificateIssueOnFeedCaption", f.title];
			} else {
				issueCaption = Resource.Manager["RES_CertificateIssueOnSiteCaption", feedUrl];
			}

			switch (e.CertificateIssue) {
				case CertificateIssue.CertCN_NO_MATCH:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertCN_NO_MATCH"]; break;
				case CertificateIssue.CertEXPIRED:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertEXPIRED", e.Certificate.GetExpirationDateString()]; break;
				case CertificateIssue.CertREVOKED:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertREVOKED"]; break;
				case CertificateIssue.CertUNTRUSTEDCA:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertUNTRUSTEDCA"]; break;
				case CertificateIssue.CertUNTRUSTEDROOT:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertUNTRUSTEDROOT"]; break;
				case CertificateIssue.CertUNTRUSTEDTESTROOT:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertUNTRUSTEDTESTROOT"]; break;
				case CertificateIssue.CertPURPOSE:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertPURPOSE"]; break;
				case CertificateIssue.CertCHAINING:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertCHAINING"]; break;
				case CertificateIssue.CertCRITICAL:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertCRITICAL"]; break;
				case CertificateIssue.CertISSUERCHAINING:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertISSUERCHAINING"]; break;
				case CertificateIssue.CertMALFORMED:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertMALFORMED"]; break;
				case CertificateIssue.CertPATHLENCONST:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertPATHLENCONST"]; break;
				case CertificateIssue.CertREVOCATION_FAILURE:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertREVOCATION_FAILURE"]; break;
				case CertificateIssue.CertROLE:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertROLE"]; break;
				case CertificateIssue.CertVALIDITYPERIODNESTING:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertVALIDITYPERIODNESTING"]; break;
				case CertificateIssue.CertWRONG_USAGE:
					issueDesc = Resource.Manager["RES_CertificateIssue.CertWRONG_USAGE"]; break;
				default:
					issueDesc = Resource.Manager["RES_CertificateIssue.Unknown", e.CertificateIssue]; break;
			}

			// show cert. issue dialog
			RssBandit.WinGui.Dialogs.SecurityIssueDialog dialog = new RssBandit.WinGui.Dialogs.SecurityIssueDialog(issueCaption, issueDesc);
			
			dialog.CustomCommand.Tag = e.Certificate;
			dialog.CustomCommand.Click += new EventHandler(this.OnSecurityIssueDialogCustomCommandClick);
			dialog.CustomCommand.Visible = true;

			Win32.SetForegroundWindow(this.Handle);	// ensure, it is in front
			if (dialog.ShowDialog(this) == DialogResult.OK) {
				e.Cancel = false;
				owner.AddTrustedCertificateIssue(feedUrl, e.CertificateIssue);
			} 

		}

		private void OnSecurityIssueDialogCustomCommandClick(object sender, EventArgs e) {
			Button cmd = (Button)sender;
			cmd.Enabled = false;

			Application.DoEvents();

			System.Security.Cryptography.X509Certificates.X509Certificate cert = (System.Security.Cryptography.X509Certificates.X509Certificate)cmd.Tag;
			string certFilename = Path.Combine(Path.GetTempPath(), cert.GetHashCode().ToString() + ".temp.cer");

			try {
				if (File.Exists(certFilename))
					File.Delete(certFilename);

				using (Stream stream = FileHelper.OpenForWrite(certFilename)) {
					BinaryWriter writer = new BinaryWriter(stream);
					writer.Write(cert.GetRawCertData());
					writer.Flush();
					writer.Close();
				}
			} catch (Exception ex) {
				AppExceptions.ExceptionManager.Publish(ex);
				cmd.Enabled = false;
				return;
			}

			try {
				if (File.Exists(certFilename)) {
					Process p = Process.Start(certFilename);
					p.WaitForExit();	// to enble delete the temp file
				}
			} finally {
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
			_shortcutHandler = new K.ShortcutHandler();
			string settingsPath = RssBanditApplication.GetShortcutSettingsFileName();
			try
			{
				_shortcutHandler.Load(settingsPath);
			}
			catch(K.InvalidShortcutSettingsFileException e)
			{
				_log.Warn("The user defined shortcut settings file is invalid. Using the default instead.", e);
				using(Stream settingsStream = Resource.Manager.GetStream("Resources.ShortcutSettings.xml"))
				{
					_shortcutHandler.Load(settingsStream);
				}
			}

			/*
			_shortcuts = new K.ShortcutManager();
			string settingsPath = RssBanditApplication.GetShortcutSettingsFileName();
			if(File.Exists(settingsPath))
			{
				_shortcuts.LoadSettings(settingsPath);
			}
			else
			{
				using(Stream settingsStream = Resource.Manager.GetStream("Resources.ShortcutSettings.xml"))
				{
					_shortcuts.LoadSettings(settingsStream);
				}
			}
			*/
		}

		#region Resource handling

		protected void InitResources() {
			// Create a strip of images by loading an embedded bitmap resource
			// Ensure, the Point() parameter locates a magenta pixel to make it transparent!
			_toolImages = Resource.Manager.LoadBitmapStrip("Resources.ToolImages.bmp", new Size(16,16), new Point(0,0));
			_browserImages = Resource.Manager.LoadBitmapStrip("Resources.BrowserImages.bmp",new Size(16,16), new Point(0,0));
			//_browserImages = Resource.Manager.LoadBitmapStrip("Resources.BrowserImages24.bmp",	new Size(24,24), new Point(0,0));
			_treeImages = Resource.Manager.LoadBitmapStrip("Resources.TreeImages.bmp",	new Size(16,16), new Point(0,0));
			_listImages = Resource.Manager.LoadBitmapStrip("Resources.ListImages.bmp",	new Size(16,16), new Point(0,0));
			_searchEngineImages = new ImageList();
		}
		#endregion

		#region Widget init routines
		
		private void InitWidgets() {
			InitFeedTreeView();
			InitListView();
			InitHtmlDetail();
			InitToaster();
			InitSearchPanel();
		}

		private void InitSearchPanel() {
			// init the scope resolver callback(s)
			owner.FindersSearchRoot.SetScopeResolveCallback(
				new RssFinder.SearchScopeResolveCallback(this.ScopeResolve));

			this.textSearchExpression.TextChanged += new System.EventHandler(this.OnRssSearchExpressionChanged);
			this.textFinderCaption.TextChanged += new System.EventHandler(this.OnRssSearchFinderCaptionChanged);
			this.textSearchExpression.KeyDown += new KeyEventHandler(this.OnTextSearchExpressionKeyDown);
			this.textSearchExpression.KeyPress += new KeyPressEventHandler(this.OnAnyEnterKeyPress);
		
			this.panelRssSearch.Resize += new System.EventHandler(this.OnRssSearchPanelResize);

			this.btnSearchCancel.Click += new EventHandler(this.OnRssSearchButtonClick);
			this.btnSearchCancel.Enabled = false;
			this.btnRssSearchSave.Click += new EventHandler(this.OnRssSearchButtonClick);
			this.btnRssSearchSave.Enabled = false;
			this.btnNewSearch.Click += new EventHandler(this.CmdNewRssSearchClick);
			this.btnNewSearch.Enabled = false;
			
			this.radioRssSearchExprXPath.CheckedChanged += new System.EventHandler(this.OnRssSearchTypeCheckedChanged);
			this.radioRssSearchRegEx.CheckedChanged += new System.EventHandler(this.OnRssSearchTypeCheckedChanged);
			this.radioRssSearchSimpleText.CheckedChanged += new System.EventHandler(this.OnRssSearchTypeCheckedChanged);
			
			this.checkBoxConsiderItemReadState.CheckedChanged += new EventHandler(this.OnRssSearchConsiderItemReadStateCheckedChanged);
			this.checkBoxRssSearchTimeSpan.CheckedChanged += new EventHandler(this.OnRssSearchConsiderItemAgeCheckedChanged);
			this.checkBoxRssSearchByDate.CheckedChanged += new EventHandler(this.OnRssSearchConsiderItemPostDateCheckedChanged);
			this.checkBoxRssSearchByDateRange.CheckedChanged += new EventHandler(this.OnRssSearchConsiderItemPostDateRangeCheckedChanged);
		
			this.comboRssSearchItemAge.SelectedIndex = 0;
			this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 0;

			this.dateTimeRssSearchItemPost.Value = DateTime.Now.Subtract(new TimeSpan(1,0,0,0));
			this.dateTimeRssSearchPostAfter.Value = DateTime.Now.Subtract(new TimeSpan(2,0,0,0));
			this.dateTimeRssSearchPostBefore.Value = DateTime.Now.Subtract(new TimeSpan(1,0,0,0));
			
			this.OnRssSearchConsiderItemReadStateCheckedChanged(this, EventArgs.Empty);
			this.OnRssSearchConsiderItemAgeCheckedChanged(this, EventArgs.Empty);
			this.OnRssSearchConsiderItemPostDateCheckedChanged(this, EventArgs.Empty);
			this.OnRssSearchConsiderItemPostDateRangeCheckedChanged(this, EventArgs.Empty);

			// enable native info tips support:
			Win32.ModifyWindowStyle(treeRssSearchScope.Handle, 0, Win32.TVS_INFOTIP);
			this.treeRssSearchScope.ImageList = _treeImages;

			this.collapsiblePanelSearchNameEx.Collapsed = false;
			this.collapsiblePanelItemPropertiesEx.Collapsed = false;

			this.taskPaneSearchOptions.Refresh();
		}

		private void InitToaster() {
			_toastNotifier = new ToastNotifier(
				new ItemActivateCallback(this.OnExternalActivateFeedItem), 
				new DisplayFeedPropertiesCallback(this.OnExternalDisplayFeedProperties),
				new FeedActivateCallback(this.OnExternalActivateFeed));
		}

		private void InitListView() {

			colTopic.Text = Resource.Manager["RES_ListviewColumnCaptionTopic"];
			colHeadline.Text = Resource.Manager["RES_ListviewColumnCaptionHeadline"];
			colDate.Text = Resource.Manager["RES_ListviewColumnCaptionDate"];

			listFeedItems.SmallImageList = _listImages;
			owner.FeedlistLoaded += new EventHandler(this.OnOwnerFeedlistLoaded);
			listFeedItems.ColumnClick += new ColumnClickEventHandler(OnFeedListItemsColumnClick);
		}

		private void InitHtmlDetail() {
			
			this.htmlDetail.ActiveXEnabled = false;
			this.htmlDetail.Border3d = true;
			this.htmlDetail.FlatScrollBars = true;
			this.htmlDetail.ImagesDownloadEnabled = true;
			this.htmlDetail.JavaEnabled = false;
			this.htmlDetail.ScriptEnabled = false;
			this.htmlDetail.ScriptObject = null;
			this.htmlDetail.ScrollBarsEnabled = true;
			this.htmlDetail.VideoEnabled = false;
			this.htmlDetail.AllowInPlaceNavigation = false;
			this.htmlDetail.SilentModeEnabled = true;

			this.htmlDetail.Tag = this._docFeedDetails;

			this.htmlDetail.StatusTextChanged += new BrowserStatusTextChangeEventHandler(OnWebStatusTextChanged);
			this.htmlDetail.BeforeNavigate += new BrowserBeforeNavigate2EventHandler(OnWebBeforeNavigate);
			this.htmlDetail.NavigateComplete += new BrowserNavigateComplete2EventHandler(OnWebNavigateComplete);
			this.htmlDetail.DocumentComplete += new BrowserDocumentCompleteEventHandler(OnWebDocumentComplete);
			this.htmlDetail.NewWindow += new BrowserNewWindowEventHandler(OnWebNewWindow);
			this.htmlDetail.ProgressChanged += new BrowserProgressChangeEventHandler(OnWebProgressChanged);
			
			this.htmlDetail.TranslateAccelerator += new KeyEventHandler(OnWebTranslateAccelerator);
			this.htmlDetail.Clear();
		}

		private void InitFeedTreeView() {

			// enable native info tips support:
			Win32.ModifyWindowStyle(treeFeeds.Handle, 0, Win32.TVS_INFOTIP);
			treeFeeds.ImageList = _treeImages;

			// create RootFolderType.MyFeeds:
			FeedTreeNodeBase root = new RootNode(Resource.Manager["RES_FeedNodeMyFeedsCaption"],0,1, _treeRootContextMenu);
			treeFeeds.Nodes.Add(root);
			root.ReadCounterZero += new EventHandler(OnTreeNodeFeedsRootReadCounterZero);
			_roots[(int)RootFolderType.MyFeeds] = root;

			// create RootFolderType.SmartFolder:
			root = new SpecialRootNode(Resource.Manager["RES_FeedNodeSpecialFeedsCaption"],0,1, null);
			treeFeeds.Nodes.Add(root);
			_roots[(int)RootFolderType.SmartFolders] = root;

			// create RootFolderType.Finder:
			root = new FinderRootNode(Resource.Manager["RES_FeedNodeFinderRootCaption"],0,1,  _treeSearchFolderRootContextMenu);
			treeFeeds.Nodes.Add(root);
			_roots[(int)RootFolderType.Finder] = root;

			this.treeFeeds.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnTreeFeedMouseDown);
			this.treeFeeds.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnTreeFeedMouseUp);
			this.treeFeeds.DragOver += new System.Windows.Forms.DragEventHandler(this.OnTreeFeedDragOver);
			this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);
			this.treeFeeds.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.OnTreeFeedAfterLabelEdit);
			this.treeFeeds.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.OnTreeFeedQueryContiueDrag);
			this.treeFeeds.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnTreeFeedDragEnter);
			this.treeFeeds.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnTreeFeedMouseMove);
			this.treeFeeds.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnTreeFeedItemDrag);
			this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);			
			this.treeFeeds.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.OnTreeFeedBeforeLabelEdit);
			this.treeFeeds.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnTreeFeedDragDrop);
			this.treeFeeds.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.OnTreeFeedGiveFeedback);
			this.treeFeeds.DoubleClick += new EventHandler(this.OnTreeFeedDoubleClick);

		}

		#endregion

		#region Menu init routines
		private void InitMenuBar() {
			// Init the MainMenuControl
			menuBarMain.SuspendLayout();

			menuBarMain.ImageList = _toolImages;

			// Create the top level Menu
			MenuBarItem top1 = new MenuBarItem(Resource.Manager["RES_MainMenuFileCaption"]);
			top1.ToolTipText = Resource.Manager["RES_MainMenuFileDesc"];
			// Create the submenus
			CreateFileMenu(top1);
			
			MenuBarItem top2 = new MenuBarItem(Resource.Manager["RES_MainMenuViewCaption"]);
			top2.ToolTipText = Resource.Manager["RES_MainMenuViewDesc"];
			// Create the submenus
			CreateViewMenu(top2);

			MenuBarItem top3 = new MenuBarItem(Resource.Manager["RES_MainMenuToolsCaption"]);
			top3.ToolTipText = Resource.Manager["RES_MainMenuToolsDesc"];
			// Create the submenus
			CreateToolsMenu(top3);

			MenuBarItem top4 = new MenuBarItem(Resource.Manager["RES_MainMenuHelpCaption"]);
			top4.ToolTipText = Resource.Manager["RES_MainMenuHelpDesc"];
			// Create the submenus
			CreateHelpMenu(top4);

			MenuBarItem top5 = new MenuBarItem("&Gnomedex 2005");
			top5.ToolTipText = "Special menu item for Gnomedex 2005"; 

			top5.Items.AddRange(new MenuButtonItem[]{new AppMenuCommand("cmdReloadCFL", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdReloadCFL),
				"Reload Common Feed List", "Reloads the Common Feed List", 0,  _shortcutHandler) });

			menuBarMain.Items.AddRange(new MenuItemBase[]{top1,top2,top3,top4, top5});

			menuBarMain.ResumeLayout(false);
		}

		protected void CreateFileMenu(MenuBarItem mc) {
			
			// Create menu commands
			AppMenuCommand style5 = new AppMenuCommand("cmdNewFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdNewFeed),
				"RES_MenuNewFeedCaption", "RES_MenuNewFeedDesc", 1, _shortcutHandler);

			AppMenuCommand style6 = new AppMenuCommand("cmdImportFeeds", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdImportFeeds),
				"RES_MenuImportFeedsCaption", "RES_MenuImportFeedsDesc", _shortcutHandler);

			style6.BeginGroup = true;

			AppMenuCommand style7 = new AppMenuCommand("cmdExportFeeds", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdExportFeeds),
				"RES_MenuExportFeedsCaption", "RES_MenuExportFeedsDesc", _shortcutHandler);

			AppMenuCommand style9 = new AppMenuCommand("cmdCloseExit", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdExitApp),
				"RES_MenuAppCloseExitCaption", "RES_MenuAppCloseExitDesc", _shortcutHandler);

			AppMenuCommand style10 = new AppMenuCommand("cmdToggleOfflineMode", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdToggleInternetConnectionMode),
				"RES_MenuAppInternetConnectionModeCaption", "RES_MenuAppInternetConnectionModeDesc", _shortcutHandler);

			style10.BeginGroup = true;

			mc.Items.AddRange(new MenuButtonItem[]{style5,style6,style7,style10,style9});
		}

		protected void CreateViewMenu(MenuBarItem mc) {
			
			AppMenuCommand style1 = new AppMenuCommand("cmdToggleTreeViewState", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdDockShowFeedDescriptions),
				"RES_MenuToggleTreeViewStateCaption", "RES_MenuToggleTreeViewStateDesc", 13, _shortcutHandler);

			AppMenuCommand style2 = new AppMenuCommand("cmdToggleRssSearchTabState", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdDockShowRssSearch),
				"RES_MenuToggleRssSearchTabStateCaption", "RES_MenuToggleRssSearchTabStateDesc", 14, _shortcutHandler);

			MenuButtonItem style3 = new MenuButtonItem(Resource.Manager[ "RES_MenuViewToolbarsCaption"]);
			style3.ToolTipText = Resource.Manager["RES_MenuViewToolbarsDesc"];
			style3.BeginGroup = true; 
			
			// workaround (we did not get notified if a tearable toolbar gets closed via the "X" button).
			// So we refresh the view state (checked/unchecked) on before display the submenu
			style3.BeforePopup += new TD.SandBar.MenuItemBase.BeforePopupEventHandler(OnMenuItemViewToolbarsBeforePopup);

			// subMenus:			
			AppMenuCommand subTbMain = new AppMenuCommand("cmdToggleMainTBViewState", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdToggleMainTBViewState),
				"RES_MenuViewToolbarMainCaption", "RES_MenuViewToolbarMainDesc", _shortcutHandler);
			subTbMain.Checked = true;	// default
			AppMenuCommand subTbWeb = new AppMenuCommand("cmdToggleWebTBViewState", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdToggleWebTBViewState),
				"RES_MenuViewToolbarWebNavigationCaption", "RES_MenuViewToolbarWebNavigationDesc", _shortcutHandler);
			subTbWeb.Checked = true;	// default
			AppMenuCommand subTbWebSearch = new AppMenuCommand("cmdToggleWebSearchTBViewState", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdToggleWebSearchTBViewState),
				"RES_MenuViewToolbarWebSearchCaption", "RES_MenuViewToolbarWebSearchDesc", _shortcutHandler);
			subTbWebSearch.Checked = true;	// default

			style3.Items.AddRange(new MenuButtonItem[]{subTbMain, subTbWeb, subTbWebSearch});

		

			AppMenuCommand subL4 = new AppMenuCommand("cmdColumnChooserMain",
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuColumnChooserCaption", "RES_MenuColumnChooserDesc", _shortcutHandler);
			//subL3.ImageList                  = _listImages;

			foreach (string colID in Enum.GetNames(typeof(NewsItemSortField))) {

				AppMenuCommand subL4_subColumn = new AppMenuCommand("cmdListviewColumn." + colID,
					owner.Mediator, new ExecuteCommandHandler(this.CmdToggleListviewColumn),
					"RES_MenuColumnChooser" + colID +"Caption", "RES_MenuColumnChooser" + colID +"Desc", _shortcutHandler);
				
				subL4.Items.AddRange(new MenuButtonItem[]{subL4_subColumn});
			}

			AppMenuCommand subL4_subUseCatLayout = new AppMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseCategoryLayoutGlobal),
				"RES_MenuColumnChooserUseCategoryLayoutGlobalCaption", "RES_MenuColumnChooserUseCategoryLayoutGlobalDesc", _shortcutHandler);
			subL4_subUseCatLayout.BeginGroup = true;

			AppMenuCommand subL4_subUseFeedLayout = new AppMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseFeedLayoutGlobal),
				"RES_MenuColumnChooserUseFeedLayoutGlobalCaption", "RES_MenuColumnChooserUseFeedLayoutGlobalDesc", _shortcutHandler);

			AppMenuCommand subL4_subResetLayout = new AppMenuCommand("cmdColumnChooserResetToDefault",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserResetToDefault),
				"RES_MenuColumnChooserResetLayoutToDefaultCaption", "RES_MenuColumnChooserResetLayoutToDefaultDesc", _shortcutHandler);
			subL4_subResetLayout.BeginGroup = true;
			subL4_subResetLayout.Enabled = false;		// dynamically refreshed

			subL4.Items.AddRange(new MenuButtonItem[]{ subL4_subUseCatLayout, subL4_subUseFeedLayout, subL4_subResetLayout});

			AppMenuCommand style4 = new AppMenuCommand("cmdFeedDetailLayoutPosition", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuFeedDetailLayoutCaption", "RES_MenuFeedDetailLayoutDesc", _shortcutHandler);
			
			style4.BeginGroup = true;

			// subMenu:			
			AppMenuCommand subSub1 = new AppMenuCommand("cmdFeedDetailLayoutPosTop", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosTop),
				"RES_MenuFeedDetailLayoutTopCaption", "RES_MenuFeedDetailLayoutTopDesc",20, _shortcutHandler);
			
			subSub1.Checked = true;	// default

			AppMenuCommand subSub2 = new AppMenuCommand("cmdFeedDetailLayoutPosLeft", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosLeft),
				"RES_MenuFeedDetailLayoutLeftCaption", "RES_MenuFeedDetailLayoutLeftDesc",19, _shortcutHandler);

			AppMenuCommand subSub3 = new AppMenuCommand("cmdFeedDetailLayoutPosRight", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosRight),
				"RES_MenuFeedDetailLayoutRightCaption", "RES_MenuFeedDetailLayoutRightDesc", 17, _shortcutHandler);

			AppMenuCommand subSub4 = new AppMenuCommand("cmdFeedDetailLayoutPosBottom", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosBottom),
				"RES_MenuFeedDetailLayoutBottomCaption", "RES_MenuFeedDetailLayoutBottomDesc", 18, _shortcutHandler);

			style4.Items.AddRange(new MenuButtonItem[]{subSub1, subSub2, subSub3, subSub4});
			

			mc.Items.AddRange(new MenuButtonItem[]{style1,style2,style3,style4,subL4});
		}

		protected void CreateToolsMenu(MenuBarItem mc) {
			// Create tool menu commands

			AppMenuCommand style1 = new AppMenuCommand("cmdRefreshFeeds", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdRefreshFeeds), 
				"RES_MenuUpdateAllFeedsCaption", "RES_MenuUpdateAllFeedsDesc", 0, _shortcutHandler);
			

			AppMenuCommand style2 = new AppMenuCommand("cmdOpenConfigIdentitiesDialog", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdOpenConfigIdentitiesDialog),
				"RES_MenuOpenConfigIdentitiesDialogCaption", "RES_MenuOpenConfigIdentitiesDialogDesc", _shortcutHandler);
			
			style2.BeginGroup = true;
			
			AppMenuCommand style3 = new AppMenuCommand("cmdOpenConfigNntpServerDialog", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdOpenConfigNntpServerDialog),
				"RES_MenuOpenConfigNntpServerDialogCaption", "RES_MenuOpenConfigNntpServerDialogDesc", _shortcutHandler);
			style3.Enabled = true;		

			AppMenuCommand style4 = new AppMenuCommand("cmdAutoDiscoverFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdAutoDiscoverFeed),
				"RES_MenuAutoDiscoverFeedCaption", "RES_MenuAutoDiscoverFeedDesc",4, _shortcutHandler);

			AppMenuCommand style5 = new AppMenuCommand("cmdFeedItemPostReply", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdPostReplyToItem),
				"RES_MenuPostReplyFeedItemCaption", "RES_MenuPostReplyFeedItemDesc",5, _shortcutHandler);
			
			style5.Enabled = false;		// dynamically enabled on runtime if feed supports commentAPI
			
			AppMenuCommand style6 = new AppMenuCommand("cmdUploadFeeds", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdUploadFeeds),
				"RES_MenuUploadFeedsCaption", "RES_MenuUploadFeedsDesc", _shortcutHandler);

			AppMenuCommand style7 = new AppMenuCommand("cmdDownloadFeeds", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdDownloadFeeds),
				"RES_MenuDownloadFeedsCaption", "RES_MenuDownloadFeedsDesc", _shortcutHandler);

			AppMenuCommand style9 = new AppMenuCommand("cmdShowMainAppOptions", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowOptions),
				"RES_MenuAppOptionsCaption", "RES_MenuAppOptionsDesc", 6, _shortcutHandler);

			style9.BeginGroup = true;

			mc.Items.AddRange(new MenuButtonItem[]{style1, style4, style5, style6,style7, style2, style3, style9});
		}

		protected void CreateHelpMenu(MenuBarItem mc) {
			
			// Create help menu commands
			
			AppMenuCommand styleHelpWebDoc = new AppMenuCommand("cmdHelpWebDoc", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdWebHelp),
				"RES_MenuWebHelpCaption", "RES_MenuWebHelpDesc", _shortcutHandler); 

			AppMenuCommand style0 = new AppMenuCommand("cmdWorkspaceNews", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdWorkspaceNews),
				"RES_MenuWorkspaceNewsCaption", "RES_MenuWorkspaceNewsDesc", _shortcutHandler); 

			AppMenuCommand style1 = new AppMenuCommand("cmdReportBug", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdReportAppBug),
				"RES_MenuBugReportCaption", "RES_MenuBugReportDesc", _shortcutHandler); 


			AppMenuCommand style2 = new AppMenuCommand("cmdAbout", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdAboutApp) , 
				"RES_MenuAboutCaption", "RES_MenuAboutDesc", _shortcutHandler);

			style2.Icon = Resource.Manager.LoadIcon("Resources.App.ico");
			style2.BeginGroup = true;
			
			AppMenuCommand style3 = new AppMenuCommand("cmdCheckForUpdates", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdCheckForUpdates),
				"RES_MenuCheckForUpdatesCaption", "RES_MenuCheckForUpdatesDesc", _shortcutHandler);

			style3.BeginGroup = true;
#if USEAUTOUPDATE
			style3.Enabled = true;	
#else
			style3.Enabled = false;
#endif

			AppMenuCommand style4 = new AppMenuCommand("cmdWikiNews", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdWikiNews),
				"RES_MenuBanditWikiCaption", "RES_MenuBanditWikiDesc", _shortcutHandler);
			
			style4.BeginGroup = true;

			AppMenuCommand style5 = new AppMenuCommand("cmdVisitForum",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdVisitForum),
				"RES_MenuBanditForumCaption", "RES_MenuBanditForumDesc", _shortcutHandler);

			AppMenuCommand style6 = new AppMenuCommand("cmdDonateToProject",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdDonateToProject),
				"RES_MenuDonateToProjectCaption", "RES_MenuDonateToProjectDesc", _shortcutHandler);



			mc.Items.AddRange(new MenuButtonItem[]{styleHelpWebDoc, style4, style5, style0,style1,style3,style6,style2});
		}

		protected void InitContextMenus() {
			#region tree view context menus

			#region root menu
			_treeRootContextMenu             = new ContextMenu();
			
			AppContextMenuCommand sub1 = new AppContextMenuCommand("cmdNewFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdNewFeed),
				"RES_MenuNewFeedCaption", "RES_MenuNewFeedDesc", 1, _shortcutHandler);
			
			//sub1.ImageList  = _toolImages;

			AppContextMenuCommand sub2 = new AppContextMenuCommand("cmdNewCategory", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdNewCategory),
				"RES_MenuNewCategoryCaption", "RES_MenuNewCategoryDesc", 2, _shortcutHandler);
						
			//sub2.ImageList  = _treeImages;

			MenuItem sep = new MenuItem("-");

			AppContextMenuCommand subR1 = new AppContextMenuCommand("cmdRefreshFeeds", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdRefreshFeeds),
				"RES_MenuUpdateAllFeedsCaption", "RES_MenuUpdateAllFeedsDesc", 0, _shortcutHandler);
			
			//subR1.ImageList  = _toolImages;

			AppContextMenuCommand subR2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdCatchUpCurrentSelectedNode),
				"RES_MenuCatchUpOnAllCaption", "RES_MenuCatchUpOnAllDesc", 0, _shortcutHandler);
			//subR2.ImageList           = _listImages;

			AppContextMenuCommand subR3 = new AppContextMenuCommand("cmdDeleteAll", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdDeleteAll),
				"RES_MenuDeleteAllFeedsCaption", "RES_MenuDeleteAllFeedsDesc", 2, _shortcutHandler);
			//subR3.ImageList           = _toolImages;

			AppContextMenuCommand subR4 = new AppContextMenuCommand("cmdShowMainAppOptions", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowOptions),
				"RES_MenuAppOptionsCaption", "RES_MenuAppOptionsDesc", 10, _shortcutHandler);

			// append items
			_treeRootContextMenu.MenuItems.AddRange(new MenuItem[]{sub1,sub2,sep,subR1,subR2,sep.CloneMenu(),subR3,sep.CloneMenu(),subR4});
			#endregion

			#region category menu
			_treeCategoryContextMenu	  = new ContextMenu();

			AppContextMenuCommand subC1 = new AppContextMenuCommand("cmdUpdateCategory", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdUpdateCategory),
				"RES_MenuUpdateCategoryCaption", "RES_MenuUpdateCategoryDesc", _shortcutHandler);

			AppContextMenuCommand subC2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdCatchUpCurrentSelectedNode),
				"RES_MenuCatchUpCategoryCaption", "RES_MenuCatchUpCategoryDesc", 0, _shortcutHandler);
			//subC2.ImageList            = _listImages;

			AppContextMenuCommand subC3  = new AppContextMenuCommand("cmdRenameCategory", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdRenameCategory),
				"RES_MenuRenameCategoryCaption", "RES_MenuRenameCategoryDesc", _shortcutHandler);

			AppContextMenuCommand subC4  = new AppContextMenuCommand("cmdDeleteCategory", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdDeleteCategory),
				"RES_MenuDeleteCategoryCaption", "RES_MenuDeleteCategoryDesc", 2, _shortcutHandler);
			
			//subC4.ImageList            = _toolImages;

			AppContextMenuCommand subC5  = new AppContextMenuCommand("cmdShowCategoryProperties", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowCategoryProperties),
				"RES_MenuShowCategoryPropertiesCaption", "RES_MenuShowCategoryPropertiesDesc", 10, _shortcutHandler);

			AppContextMenuCommand subCL_ColLayoutMain = new AppContextMenuCommand("cmdColumnChooserMain",
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuColumnChooserCaption", "RES_MenuColumnChooserDesc", _shortcutHandler);

			foreach (string colID in Enum.GetNames(typeof(NewsItemSortField))) {

				AppContextMenuCommand subCL4_layoutSubColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
					owner.Mediator, new ExecuteCommandHandler(this.CmdToggleListviewColumn),
					"RES_MenuColumnChooser" + colID +"Caption", "RES_MenuColumnChooser" + colID +"Desc", _shortcutHandler);
				
				subCL_ColLayoutMain.MenuItems.AddRange(new MenuItem[]{subCL4_layoutSubColumn});
			}

			AppContextMenuCommand subCL_subUseCatLayout = new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseCategoryLayoutGlobal),
				"RES_MenuColumnChooserUseCategoryLayoutGlobalCaption", "RES_MenuColumnChooserUseCategoryLayoutGlobalDesc", _shortcutHandler);

			AppContextMenuCommand subCL_subUseFeedLayout = new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseFeedLayoutGlobal),
				"RES_MenuColumnChooserUseFeedLayoutGlobalCaption", "RES_MenuColumnChooserUseFeedLayoutGlobalDesc", _shortcutHandler);
				
			AppContextMenuCommand subCL_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserResetToDefault),
				"RES_MenuColumnChooserResetLayoutToDefaultCaption", "RES_MenuColumnChooserResetLayoutToDefaultDesc", _shortcutHandler);

			subCL_ColLayoutMain.MenuItems.AddRange(new MenuItem[]{sep.CloneMenu(), subCL_subUseCatLayout, subCL_subUseFeedLayout,sep.CloneMenu(), subCL_subResetLayout});
					
			// append items. Reuse cmdNewCat/cmdNewFeed, because it's allowed on categories
			_treeCategoryContextMenu.MenuItems.AddRange(new MenuItem[]{sub1.CloneMenu() ,sub2.CloneMenu(),sep.CloneMenu(),subC1,subC2,subC3,sep.CloneMenu(),subC4, sep.CloneMenu(), subCL_ColLayoutMain, sep.CloneMenu(), subC5});
			#endregion

			#region feed menu
			_treeFeedContextMenu				 = new ContextMenu();

			AppContextMenuCommand subF1  = new AppContextMenuCommand("cmdUpdateFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdUpdateFeed),
				"RES_MenuUpdateThisFeedCaption", "RES_MenuUpdateThisFeedDesc", _shortcutHandler);

			AppContextMenuCommand subF2  = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdCatchUpCurrentSelectedNode),
				"RES_MenuCatchUpThisFeedCaption", "RES_MenuCatchUpThisFeedDesc", 0, _shortcutHandler);
			
			//subF2.ImageList                     = _listImages;
			AppContextMenuCommand subF3  = new AppContextMenuCommand("cmdRenameFeed",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdRenameFeed),
				"RES_MenuRenameThisFeedCaption", "RES_MenuRenameThisFeedDesc", _shortcutHandler);

			AppContextMenuCommand subF4  = new AppContextMenuCommand("cmdDeleteFeed",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdDeleteFeed),
				"RES_MenuDeleteThisFeedCaption", "RES_MenuDeleteThisFeedDesc", 2, _shortcutHandler);

			//subF4.ImageList            = _toolImages;

			AppContextMenuCommand subFeedCopy = new AppContextMenuCommand("cmdCopyFeed",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyFeed),
				"RES_MenuCopyFeedCaption", "RES_MenuCopyFeedDesc", 1, _shortcutHandler);

			AppContextMenuCommand subFeedCopy_sub1 = new AppContextMenuCommand("cmdCopyFeedLinkToClipboard",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyFeedLinkToClipboard),
				"RES_MenuCopyFeedLinkToClipboardCaption", "RES_MenuCopyFeedLinkToClipboardDesc", 1, _shortcutHandler);

			AppContextMenuCommand subFeedCopy_sub2 = new AppContextMenuCommand("cmdCopyFeedHomepageLinkToClipboard",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyFeedHomeLinkToClipboard),
				"RES_MenuCopyFeedHomeLinkToClipboardCaption", "RES_MenuCopyFeedHomeLinkToClipboardDesc", 1, _shortcutHandler);

			AppContextMenuCommand subFeedCopy_sub3 = new AppContextMenuCommand("cmdCopyFeedHomepageTitleLinkToClipboard",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyFeedHomeTitleLinkToClipboard),
				"RES_MenuCopyFeedFeedHomeTitleLinkToClipboardCaption", "RES_MenuCopyFeedFeedHomeTitleLinkToClipboardDesc", 1, _shortcutHandler);

			subFeedCopy.MenuItems.AddRange(new MenuItem[] {subFeedCopy_sub1,subFeedCopy_sub2,subFeedCopy_sub3} );

			
			MenuItem subInfo = new MenuItem(Resource.Manager["RES_MenuAdvancedFeedInfoCaption"]);

			// the general properties item
			AppContextMenuCommand subF6  = new AppContextMenuCommand("cmdShowFeedProperties",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowFeedProperties),
				"RES_MenuShowFeedPropertiesCaption", "RES_MenuShowFeedPropertiesDesc", 10, _shortcutHandler);
			//subF6.ImageList				     = _browserImages;

			// layout menu(s):
			AppContextMenuCommand subFL_ColLayoutMain = new AppContextMenuCommand("cmdColumnChooserMain",
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuColumnChooserCaption", "RES_MenuColumnChooserDesc", _shortcutHandler);

			foreach (string colID in Enum.GetNames(typeof(NewsItemSortField))) {

				AppContextMenuCommand subFL4_layoutSubColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
					owner.Mediator, new ExecuteCommandHandler(this.CmdToggleListviewColumn),
					"RES_MenuColumnChooser" + colID +"Caption", "RES_MenuColumnChooser" + colID +"Desc", _shortcutHandler);
				
				subFL_ColLayoutMain.MenuItems.AddRange(new MenuItem[]{subFL4_layoutSubColumn});
			}

			AppContextMenuCommand subFL_subUseCatLayout = new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseCategoryLayoutGlobal),
				"RES_MenuColumnChooserUseCategoryLayoutGlobalCaption", "RES_MenuColumnChooserUseCategoryLayoutGlobalDesc", _shortcutHandler);

			AppContextMenuCommand subFL_subUseFeedLayout = new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseFeedLayoutGlobal),
				"RES_MenuColumnChooserUseFeedLayoutGlobalCaption", "RES_MenuColumnChooserUseFeedLayoutGlobalDesc", _shortcutHandler);
				
			AppContextMenuCommand subFL_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserResetToDefault),
				"RES_MenuColumnChooserResetLayoutToDefaultCaption", "RES_MenuColumnChooserResetLayoutToDefaultDesc", _shortcutHandler);

			subFL_ColLayoutMain.MenuItems.AddRange(new MenuItem[]{sep.CloneMenu(), subFL_subUseCatLayout, subFL_subUseFeedLayout,sep.CloneMenu(), subFL_subResetLayout});

			// append items. 
			_treeFeedContextMenu.MenuItems.AddRange(new MenuItem[]{subF1,subF2,subF3,sep.CloneMenu(),subF4,sep.CloneMenu(),subFeedCopy,sep.CloneMenu(), subInfo,sep.CloneMenu(), subFL_ColLayoutMain, sep.CloneMenu(), subF6});
			#endregion

			#region feed info context submenu
			
			AppContextMenuCommand subInfoHome = new AppContextMenuCommand("cmdNavigateToFeedHome", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdNavigateFeedHome),
				"RES_MenuNavigateToFeedHomeCaption", "RES_MenuNavigateToFeedHomeDesc", _shortcutHandler);

			AppContextMenuCommand subInfoCosmos = new AppContextMenuCommand("cmdNavigateToFeedCosmos", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdNavigateFeedLinkCosmos),
				"RES_MenuShowLinkCosmosCaption", "RES_MenuShowLinkCosmosCaption");

			AppContextMenuCommand subInfoSource = new AppContextMenuCommand("cmdViewSourceOfFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdViewSourceOfFeed),
				"RES_MenuViewSourceOfFeedCaption", "RES_MenuViewSourceOfFeedDesc", _shortcutHandler);

			AppContextMenuCommand subInfoValidate = new AppContextMenuCommand("cmdValidateFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdValidateFeed),
				"RES_MenuValidateFeedCaption", "RES_MenuValidateFeedDesc", _shortcutHandler);
			
			subInfo.MenuItems.AddRange(new MenuItem[] {subInfoHome,subInfoCosmos,subInfoSource,subInfoValidate} );
			
			#endregion

			#region root search folder context menu 

			_treeSearchFolderRootContextMenu = new ContextMenu();

			subF1  = new AppContextMenuCommand("cmdNewFinder",
				owner.Mediator, new ExecuteCommandHandler(this.CmdNewFinder),
				"RES_MenuNewFinderCaption", "RES_MenuNewFinderDesc", _shortcutHandler);
			subF2  = new AppContextMenuCommand("cmdDeleteAllFinders",
				owner.Mediator, new ExecuteCommandHandler(this.CmdDeleteAllFinder),
				"RES_MenuFinderDeleteAllCaption", "RES_MenuFinderDeleteAllDesc", _shortcutHandler);
			
			_treeSearchFolderRootContextMenu.MenuItems.AddRange(new MenuItem[]{subF1, sep.CloneMenu(), subF2});			

			#endregion 

			#region search folder context menu's
			
			_treeSearchFolderContextMenu = new ContextMenu();

			subF1  = new AppContextMenuCommand("cmdMarkFinderItemsRead",
				owner.Mediator, new ExecuteCommandHandler(this.CmdMarkFinderItemsRead),
				"RES_MenuCatchUpOnAllCaption", "RES_MenuCatchUpOnAllDesc", _shortcutHandler);
			subF2  = new AppContextMenuCommand("cmdRenameFinder",
				owner.Mediator, new ExecuteCommandHandler(this.CmdRenameFinder),
				"RES_MenuFinderRenameCaption", "RES_MenuFinderRenameDesc", _shortcutHandler);
			subF3  = new AppContextMenuCommand("cmdRefreshFinder",
				owner.Mediator, new ExecuteCommandHandler(this.CmdRefreshFinder),
				"RES_MenuRefreshFinderCaption", "RES_MenuRefreshFinderDesc", _shortcutHandler);
			subF4  = new AppContextMenuCommand("cmdDeleteFinder",
				owner.Mediator, new ExecuteCommandHandler(this.CmdDeleteFinder),
				"RES_MenuFinderDeleteCaption", "RES_MenuFinderDeleteDesc", _shortcutHandler);
			subF6  = new AppContextMenuCommand("cmdShowFinderProperties",
				owner.Mediator, new ExecuteCommandHandler(this.CmdShowFinderProperties),
				"RES_MenuShowFinderPropertiesCaption", "RES_MenuShowFinderPropertiesDesc", _shortcutHandler);

			_treeSearchFolderContextMenu.MenuItems.AddRange(new MenuItem[]{subF1, subF2, subF3, sep.CloneMenu(), subF4, sep.CloneMenu(), subF6});


			_treeTempSearchFolderContextMenu = new ContextMenu();

			subF1  = new AppContextMenuCommand("cmdMarkFinderItemsRead",
				owner.Mediator, new ExecuteCommandHandler(this.CmdMarkFinderItemsRead),
				"RES_MenuCatchUpOnAllCaption", "RES_MenuCatchUpOnAllDesc", _shortcutHandler);
			subF2  = new AppContextMenuCommand("cmdRefreshFinder",
				owner.Mediator, new ExecuteCommandHandler(this.CmdRefreshFinder),
				"RES_MenuRefreshFinderCaption", "RES_MenuRefreshFinderDesc", _shortcutHandler);
			subF3  = new AppContextMenuCommand("cmdSubscribeToFinderResult",
				owner.Mediator, new ExecuteCommandHandler(this.CmdSubscribeToFinderResult),
				"RES_MenuSubscribeToFinderResultCaption", "RES_MenuSubscribeToFinderResultDesc", _shortcutHandler);
			subF3.Enabled = false;	// dynamic
			subF4  = new AppContextMenuCommand("cmdShowFinderProperties",
				owner.Mediator, new ExecuteCommandHandler(this.CmdShowFinderProperties),
				"RES_MenuShowFinderPropertiesCaption", "RES_MenuShowFinderPropertiesDesc", _shortcutHandler);

			_treeTempSearchFolderContextMenu.MenuItems.AddRange(new MenuItem[]{subF1, subF2, sep.CloneMenu(), subF3, sep.CloneMenu(), subF4});
			#endregion

			treeFeeds.ContextMenu = _treeRootContextMenu;	// init to root context
			#endregion

			#region list view context menu
			_listContextMenu = new ContextMenu();

			AppContextMenuCommand subL0 = new AppContextMenuCommand("cmdMarkSelectedFeedItemsRead",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdMarkFeedItemsRead),
				"RES_MenuCatchUpSelectedNodeCaption", "RES_MenuCatchUpSelectedNodeDesc",0, _shortcutHandler);

			AppContextMenuCommand subL1 = new AppContextMenuCommand("cmdMarkSelectedFeedItemsUnread", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdMarkFeedItemsUnread),
				"RES_MenuMarkFeedItemsUnreadCaption", "RES_MenuMarkFeedItemsUnreadDesc", 1, _shortcutHandler);
			
			//subL1.ImageList           = _listImages;
		
			AppContextMenuCommand subL2 = new AppContextMenuCommand("cmdFeedItemPostReply", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdPostReplyToItem),
				"RES_MenuFeedItemPostReplyCaption", "RES_MenuFeedItemPostReplyDesc", 5, _shortcutHandler);
			//subL2.ImageList = _toolImages;
			subL2.Enabled = false;		// dynamically enabled on runtime if feed supports commentAPI

			AppContextMenuCommand subL3 = new AppContextMenuCommand("cmdFlagNewsItem",
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuFlagFeedItemCaption", "RES_MenuFlagFeedItemDesc", 1, _shortcutHandler);
			//subL3.ImageList                  = _listImages;
			subL3.Enabled = false;		// dynamically enabled on runtime if feed supports flag

			AppContextMenuCommand subL3_sub1 = new AppContextMenuCommand("cmdFlagNewsItemForFollowUp",
				owner.Mediator, new ExecuteCommandHandler(this.CmdFlagNewsItemForFollowUp),
				"RES_MenuFlagFeedItemFollowUpCaption", "RES_MenuFlagFeedItemFollowUpDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL3_sub2 = new AppContextMenuCommand("cmdFlagNewsItemForReview",
				owner.Mediator, new ExecuteCommandHandler(this.CmdFlagNewsItemForReview),
				"RES_MenuFlagFeedItemReviewCaption", "RES_MenuFlagFeedItemReviewDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL3_sub3 = new AppContextMenuCommand("cmdFlagNewsItemForReply",
				owner.Mediator, new ExecuteCommandHandler(this.CmdFlagNewsItemForReply),
				"RES_MenuFlagFeedItemReplyCaption", "RES_MenuFlagFeedItemReplyDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL3_sub4 = new AppContextMenuCommand("cmdFlagNewsItemRead",
				owner.Mediator, new ExecuteCommandHandler(this.CmdFlagNewsItemRead),
				"RES_MenuFlagFeedItemReadCaption", "RES_MenuFlagFeedItemReadDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL3_sub5 = new AppContextMenuCommand("cmdFlagNewsItemForward",
				owner.Mediator, new ExecuteCommandHandler(this.CmdFlagNewsItemForward),
				"RES_MenuFlagFeedItemForwardCaption", "RES_MenuFlagFeedItemForwardDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL3_sub8 = new AppContextMenuCommand("cmdFlagNewsItemComplete",
				owner.Mediator, new ExecuteCommandHandler(this.CmdFlagNewsItemComplete),
				"RES_MenuFlagFeedItemCompleteCaption", "RES_MenuFlagFeedItemCompleteDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL3_sub9 = new AppContextMenuCommand("cmdFlagNewsItemNone",
				owner.Mediator, new ExecuteCommandHandler(this.CmdFlagNewsItemNone),
				"RES_MenuFlagFeedItemClearCaption", "RES_MenuFlagFeedItemClearDesc", 1, _shortcutHandler);

			subL3.MenuItems.AddRange(new MenuItem[]{subL3_sub1,subL3_sub2,subL3_sub3,subL3_sub4,subL3_sub5,sep.CloneMenu(), subL3_sub8, sep.CloneMenu(),subL3_sub9});

			AppContextMenuCommand subL10 = new AppContextMenuCommand("cmdCopyNewsItem",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyNewsItem),
				"RES_MenuCopyFeedItemCaption", "RES_MenuCopyFeedItemDesc", 1, _shortcutHandler);

			AppContextMenuCommand subL10_sub1 = new AppContextMenuCommand("cmdCopyNewsItemLinkToClipboard",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyNewsItemLinkToClipboard),
				"RES_MenuCopyFeedItemLinkToClipboardCaption", "RES_MenuCopyFeedItemLinkToClipboardDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL10_sub2 = new AppContextMenuCommand("cmdCopyNewsItemTitleLinkToClipboard",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyNewsItemTitleLinkToClipboard),
				"RES_MenuCopyFeedItemTitleLinkToClipboardCaption", "RES_MenuCopyFeedItemTitleLinkToClipboardDesc", 1, _shortcutHandler);
			AppContextMenuCommand subL10_sub3 = new AppContextMenuCommand("cmdCopyNewsItemContentToClipboard",
				owner.Mediator, new ExecuteCommandHandler(this.CmdCopyNewsItemContentToClipboard),
				"RES_MenuCopyFeedItemContentToClipboardCaption", "RES_MenuCopyFeedItemContentToClipboardDesc", 1, _shortcutHandler);

			subL10.MenuItems.AddRange(new MenuItem[]{subL10_sub1,subL10_sub2,subL10_sub3});
			

			AppContextMenuCommand subL4 = new AppContextMenuCommand("cmdColumnChooserMain",
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuColumnChooserCaption", "RES_MenuColumnChooserDesc", _shortcutHandler);
			//subL3.ImageList                  = _listImages;

			foreach (string colID in Enum.GetNames(typeof(NewsItemSortField))) {

				AppContextMenuCommand subL4_subColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
					owner.Mediator, new ExecuteCommandHandler(this.CmdToggleListviewColumn),
					"RES_MenuColumnChooser" + colID +"Caption", "RES_MenuColumnChooser" + colID +"Desc", _shortcutHandler);
				
				subL4.MenuItems.AddRange(new MenuItem[]{subL4_subColumn});
			}

			AppContextMenuCommand subL4_subUseCatLayout = new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseCategoryLayoutGlobal),
				"RES_MenuColumnChooserUseCategoryLayoutGlobalCaption", "RES_MenuColumnChooserUseCategoryLayoutGlobalDesc", _shortcutHandler);

			AppContextMenuCommand subL4_subUseFeedLayout = new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserUseFeedLayoutGlobal),
				"RES_MenuColumnChooserUseFeedLayoutGlobalCaption", "RES_MenuColumnChooserUseFeedLayoutGlobalDesc", _shortcutHandler);
				
			AppContextMenuCommand subL4_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
				owner.Mediator, new ExecuteCommandHandler(this.CmdColumnChooserResetToDefault),
				"RES_MenuColumnChooserResetLayoutToDefaultCaption", "RES_MenuColumnChooserResetLayoutToDefaultDesc", _shortcutHandler);

			subL4.MenuItems.AddRange(new MenuItem[]{sep.CloneMenu(), subL4_subUseCatLayout, subL4_subUseFeedLayout,sep.CloneMenu(), subL4_subResetLayout});

			AppContextMenuCommand subL5 = new AppContextMenuCommand("cmdDeleteSelectedNewsItems", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdDeleteSelectedFeedItems),
				"RES_MenuDeleteSelectedFeedItemsCaption", "RES_MenuDeleteSelectedFeedItemsDesc", _shortcutHandler);

			AppContextMenuCommand subL6 = new AppContextMenuCommand("cmdRestoreSelectedNewsItems", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdRestoreSelectedFeedItems),
				"RES_MenuRestoreSelectedFeedItemsCaption", "RES_MenuRestoreSelectedFeedItemsDesc", _shortcutHandler);
			subL6.Visible = false;	// dynamic visible only in "Deleted Items" view

			_listContextMenuDeleteItemsSeparator = sep.CloneMenu();
			_listContextMenu.MenuItems.AddRange(new MenuItem[]{subL2, subL3, subL0, subL1, sep.CloneMenu(), subL10, _listContextMenuDeleteItemsSeparator, subL5, subL6, sep.CloneMenu(), subL4 });
			listFeedItems.ContextMenu = _listContextMenu;
			#endregion

			#region Local Feeds context menu
			
			_treeLocalFeedContextMenu = new ContextMenu();

			AppContextMenuCommand subTL1 = new AppContextMenuCommand("cmdDeleteAllNewsItems", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdDeleteAllFeedItems),
				"RES_MenuDeleteAllFeedItemsCaption", "RES_MenuDeleteAllFeedItemsDesc", 1, _shortcutHandler);
			//subTL1.ImageList           = _listImages;

			_treeLocalFeedContextMenu.MenuItems.AddRange(new MenuItem[]{subTL1});

			#endregion

			#region doc tab context menu
			
			_docTabContextMenu = new ContextMenu();

			AppContextMenuCommand subDT1 = new AppContextMenuCommand("cmdDocTabCloseThis", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdDocTabCloseSelected),
				"RES_MenuDocTabsCloseCurrentCaption", "RES_MenuDocTabsCloseCurrentDesc", 1, _shortcutHandler);
			//subDT1.ImageList           = _listImages;

			AppContextMenuCommand subDT2 = new AppContextMenuCommand("cmdDocTabCloseAllOnStrip", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdDocTabCloseAllOnStrip),
				"RES_MenuDocTabsCloseAllOnStripCaption", "RES_MenuDocTabsCloseAllOnStripDesc", 2, _shortcutHandler);
			//subDT2.ImageList           = _listImages;

			AppContextMenuCommand subDT3 = new AppContextMenuCommand("cmdDocTabCloseAll", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdDocTabCloseAll),
				"RES_MenuDocTabsCloseAllCaption", "RES_MenuDocTabsCloseAllDesc", 3, _shortcutHandler);
			//subDT3.ImageList           = _listImages;

			AppContextMenuCommand subDT4 = new AppContextMenuCommand("cmdDocTabLayoutHorizontal", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdDocTabLayoutHorizontal),
				"RES_MenuDocTabsLayoutHorizontalCaption", "RES_MenuDocTabsLayoutHorizontalDesc", _shortcutHandler);
			subDT4.Checked = (_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal);

			
			AppContextMenuCommand subDT5 = new AppContextMenuCommand("cmdFeedDetailLayoutPosition", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuFeedDetailLayoutCaption", "RES_MenuFeedDetailLayoutDesc", _shortcutHandler);
			
			// subMenu:			
			AppContextMenuCommand subSub1 = new AppContextMenuCommand("cmdFeedDetailLayoutPosTop", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosTop),
				"RES_MenuFeedDetailLayoutTopCaption", "RES_MenuFeedDetailLayoutTopDesc", _shortcutHandler);
			
			subSub1.Checked = true;	// default

			AppContextMenuCommand subSub2 = new AppContextMenuCommand("cmdFeedDetailLayoutPosLeft", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosLeft),
				"RES_MenuFeedDetailLayoutLeftCaption", "RES_MenuFeedDetailLayoutLeftDesc", _shortcutHandler);

			AppContextMenuCommand subSub3 = new AppContextMenuCommand("cmdFeedDetailLayoutPosRight", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosRight),
				"RES_MenuFeedDetailLayoutRightCaption", "RES_MenuFeedDetailLayoutRightDesc", _shortcutHandler);

			AppContextMenuCommand subSub4 = new AppContextMenuCommand("cmdFeedDetailLayoutPosBottom", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdFeedDetailLayoutPosBottom),
				"RES_MenuFeedDetailLayoutBottomCaption", "RES_MenuFeedDetailLayoutBottomDesc", _shortcutHandler);

			subDT5.MenuItems.AddRange(new MenuItem[]{subSub1, subSub2, subSub3, subSub4});
			

			_docTabContextMenu.MenuItems.AddRange(new MenuItem[]{subDT1, subDT2, subDT3, sep.CloneMenu(), subDT4, sep.CloneMenu(), subDT5});

			#endregion

			#region tray context menu
			_notifyContextMenu               = new ContextMenu();

			AppContextMenuCommand subT1 = new AppContextMenuCommand("cmdShowGUI", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowMainGui), 
				"RES_MenuShowMainGuiCaption", "RES_MenuShowMainGuiDesc", _shortcutHandler);
			subT1.DefaultItem = true;

			AppContextMenuCommand subT1_1 = new AppContextMenuCommand("cmdRefreshFeeds",
				owner.Mediator, new ExecuteCommandHandler (owner.CmdRefreshFeeds),
				"RES_MenuUpdateAllFeedsCaption", "RES_MenuUpdateAllFeedsDescription", _shortcutHandler);
			AppContextMenuCommand subT2   = new AppContextMenuCommand("cmdShowMainAppOptions", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowOptions),
				"RES_MenuAppOptionsCaption", "RES_MenuAppOptionsDesc", 10, _shortcutHandler);
			//subT2.ImageList = _browserImages;

			AppContextMenuCommand subT5   = new AppContextMenuCommand("cmdShowConfiguredAlertWindows",
				owner.Mediator, new ExecuteCommandHandler(this.CmdNop),
				"RES_MenuShowAlertWindowsCaption", "RES_MenuShowAlertWindowsDesc", _shortcutHandler);
			//subT5.Checked = owner.Preferences.ShowConfiguredAlertWindows;

			#region ShowAlertWindows context submenu
			AppContextMenuCommand subT5_1  = new AppContextMenuCommand("cmdShowAlertWindowNone",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowAlertWindowNone),
				"RES_MenuShowNoneAlertWindowsCaption", "RES_MenuShowNoneAlertWindowsNone", _shortcutHandler);
			subT5_1.Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.None);

			AppContextMenuCommand subT5_2  = new AppContextMenuCommand("cmdShowAlertWindowConfiguredFeeds",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowAlertWindowConfigPerFeed),
				"RES_MenuShowConfiguredFeedAlertWindowsCaption", "RES_MenuShowConfiguredFeedAlertWindowsDesc", _shortcutHandler);
			subT5_2.Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.AsConfiguredPerFeed);

			AppContextMenuCommand subT5_3  = new AppContextMenuCommand("cmdShowAlertWindowAll",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdShowAlertWindowAll),
				"RES_MenuShowAllAlertWindowsCaption", "RES_MenuShowAllAlertWindowsDesc", _shortcutHandler);
			subT5_3.Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.All);

			subT5.MenuItems.AddRange(new MenuItem[]{subT5_1, subT5_2, subT5_3});
			#endregion

			AppContextMenuCommand subT6   = new AppContextMenuCommand("cmdShowNewItemsReceivedBalloon",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdToggleShowNewItemsReceivedBalloon),
				"RES_MenuShowNewItemsReceivedBalloonCaption", "RES_MenuShowNewItemsReceivedBalloonDesc", _shortcutHandler);
			subT6.Checked = owner.Preferences.ShowNewItemsReceivedBalloon;

			AppContextMenuCommand subT10   = new AppContextMenuCommand("cmdCloseExit",
				owner.Mediator, new ExecuteCommandHandler(owner.CmdExitApp),
				"RES_MenuAppCloseExitCaption", "RES_MenuAppCloseExitDesc", _shortcutHandler);

			_notifyContextMenu.MenuItems.AddRange(new MenuItem[]{subT1, subT1_1, sep.CloneMenu(),sub1.CloneMenu(),subT2,sep.CloneMenu(), subT5, subT6, sep.CloneMenu(),subT10});
			#endregion
		}

		#endregion

		#region Toolbar routines
		private void InitToolBars() {
			
			toolBarMain.SuspendLayout();
			toolBarBrowser.SuspendLayout();
			toolBarWebSearch.SuspendLayout();

			toolBarMain.ImageList = _toolImages;
			toolBarBrowser.ImageList = _browserImages;
			toolBarWebSearch.ImageList = _searchEngineImages;

			/* with the new SandBar build 112, this is possible: */
			SandBarLanguage.AddRemoveButtonsText = Resource.Manager["RES_ToolbarAddRemoveButtonsCaption"];
			SandBarLanguage.ToolbarOptionsText = Resource.Manager["RES_ToolbarOptionsCaption"];
			//TODO: translate all the other UI texts within SandBarLanguage...

			/**/
			CreateMainToolbar(toolBarMain);
			CreateBrowserToolbar(toolBarBrowser);
			CreateSearchToolbar(toolBarWebSearch);

			// to get the click commands executed
			toolBarMain.ButtonClick += new TD.SandBar.ToolBar.ButtonClickEventHandler(OnAnyToolBarButtonClick);
			toolBarBrowser.ButtonClick += new TD.SandBar.ToolBar.ButtonClickEventHandler(OnAnyToolBarButtonClick);
			toolBarWebSearch.ButtonClick += new TD.SandBar.ToolBar.ButtonClickEventHandler(OnAnyToolBarButtonClick);
			
			toolBarMain.ResumeLayout(false);
			toolBarBrowser.ResumeLayout(false);
			toolBarWebSearch.ResumeLayout(false);

		}

		private void ConnectToolbarStateEvents(bool attach) {
			// for state manangement
			if (attach) {
				toolBarMain.VisibleChanged += new EventHandler(OnAnyToolbarStateChanged);
				toolBarMain.SizeChanged += new EventHandler(OnAnyToolbarStateChanged);
				toolBarMain.LocationChanged  += new EventHandler(OnAnyToolbarStateChanged);
				toolBarBrowser.VisibleChanged += new EventHandler(OnAnyToolbarStateChanged);
				toolBarBrowser.SizeChanged += new EventHandler(OnAnyToolbarStateChanged);
				toolBarBrowser.LocationChanged += new EventHandler(OnAnyToolbarStateChanged);
				toolBarWebSearch.VisibleChanged += new EventHandler(OnAnyToolbarStateChanged);
				toolBarWebSearch.SizeChanged += new EventHandler(OnAnyToolbarStateChanged);
				toolBarWebSearch.LocationChanged += new EventHandler(OnAnyToolbarStateChanged);
			} else {
				toolBarMain.VisibleChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarMain.SizeChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarMain.LocationChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarBrowser.VisibleChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarBrowser.SizeChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarBrowser.LocationChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarWebSearch.VisibleChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarWebSearch.SizeChanged -= new EventHandler(OnAnyToolbarStateChanged);
				toolBarWebSearch.LocationChanged -= new EventHandler(OnAnyToolbarStateChanged);
			}
		}

		private void CreateMainToolbar(TD.SandBar.ToolBar tb) {

			AppToolCommand tool0 = new AppToolCommand("cmdRefreshFeeds", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdRefreshFeeds), 
				"RES_MenuUpdateAllFeedsCaption", "RES_MenuUpdateAllFeedsDesc", 0);
			
			AppToolCommand tool1 = new AppToolCommand("cmdNextUnreadFeedItem", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdNextUnreadFeedItem),
				"RES_MenuNextUnreadItemCaption", "RES_MenuNextUnreadItemDesc",3);

			tool1.BeginGroup = true;

			AppToolCommand tool2 = new AppToolCommand("cmdCatchUpCurrentSelectedNode", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdCatchUpCurrentSelectedNode),
				"RES_MenuCatchUpSelectedNodeCaption", "RES_MenuCatchUpSelectedNodeDesc",7);

			AppToolCommand tool3 = new AppToolCommand("cmdNewFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdNewFeed),
				"RES_MenuNewFeedCaption", "RES_MenuNewFeedDesc", 1);

			tool3.BeginGroup = true;

			AppToolCommand tool4 = new AppToolCommand("cmdAutoDiscoverFeed", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdAutoDiscoverFeed),
				"RES_MenuAutoDiscoverFeedCaption", "RES_MenuAutoDiscoverFeedDesc",4);


			AppToolCommand tool5 = new AppToolCommand("cmdFeedItemPostReply", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdPostReplyToItem),
				"RES_MenuPostReplyFeedItemCaption", "RES_MenuPostReplyFeedItemDesc",5);
			
			tool5.BeginGroup = true;
			tool5.Enabled = false;

			AppToolCommand tool6 = new AppToolCommand("cmdNewRssSearch", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdNewRssSearch),
				"RES_MenuNewRssSearchCaption", "RES_MenuNewRssSearchDesc", 15);

			tool6.BeginGroup = true;

			tb.Items.AddRange(new ToolbarItemBase[]{tool0,tool1,tool2,tool3,tool4,tool5,tool6, owner.BackgroundDiscoverFeedsHandler.Control});
		}

		private void CreateBrowserToolbar(TD.SandBar.ToolBar tb) {
			
			AppToolCommand tool0 = new AppToolCommand("cmdBrowserGoBack", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdBrowserGoBack),
				"RES_MenuBrowserNavigateBackCaption", "RES_MenuBrowserNavigateBackDesc",17);
			
			//tool0.Shortcut = Keys.Alt | Keys.Left;	TODO!!!
			tool0.Enabled = false;

			AppToolCommand tool1 = new AppToolCommand("cmdBrowserGoForward", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdBrowserGoForward),
				"RES_MenuBrowserNavigateForwardCaption", "RES_MenuBrowserNavigateForwardDesc",18);

			//tool1.Shortcut = Keys.Alt | Keys.Right;	TODO !!!
			tool1.Enabled = false;

			AppToolCommand tool2 = new AppToolCommand("cmdBrowserCancelNavigation", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdBrowserCancelNavigation),
				"RES_MenuBrowserNavigateCancelCaption", "RES_MenuBrowserNavigateCancelDesc",21);

			AppToolCommand tool3 = new AppToolCommand("cmdBrowserRefresh", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdBrowserRefresh),
				"RES_MenuBrowserRefreshCaption", "RES_MenuBrowserRefreshDesc", 22);
			
			//tool3.Shortcut = Keys.F5;	TODO!!!


			navigateComboBox = new TD.SandBar.ComboBoxItem();
			navigateComboBox.Text = Resource.Manager["RES_MenuBrowserNavigateComboBoxCaption"];
			navigateComboBox.ToolTipText = Resource.Manager["RES_MenuBrowserNavigateComboBoxDesc"];
			navigateComboBox.ComboBox.KeyDown += new KeyEventHandler(OnNavigateComboBoxKeyDown);
			navigateComboBox.ComboBox.KeyPress += new KeyPressEventHandler(OnAnyEnterKeyPress);
			navigateComboBox.ComboBox.DragOver += new DragEventHandler(OnNavigateComboBoxDragOver);
			navigateComboBox.ComboBox.DragDrop += new DragEventHandler(OnNavigateComboBoxDragDrop);

			navigateComboBox.ComboBox.AllowDrop = true;
			navigateComboBox.MinimumControlWidth = 330;
			navigateComboBox.Padding.Left = 1;
			navigateComboBox.Padding.Right = 1;
			//Size previous = navigateComboBox.ComboBox.Size;
			//navigateComboBox.ComboBox.Size = new Size(330, previous.Height);	// set a initial size
			//navigateComboBox.ComboBox.DropDownWidth = 450;	// dito
			

			//tb.Resize += new EventHandler(OnToolbarBrowserResize);
			//navigateComboBox.SelectedIndexChanged += new EventHandler(NavigationSelectedIndexChanged);

			urlExtender.Add(this.navigateComboBox.ComboBox);

			AppToolCommand tool5 = new AppToolCommand("cmdBrowserNavigate", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdBrowserNavigate),
				"RES_MenuBrowserDoNavigateCaption", "RES_MenuBrowserDoNavigateDesc",38);

			AppToolCommand tool6 = new AppToolCommand("cmdBrowserNewTab", 
				owner.Mediator, new ExecuteCommandHandler(owner.CmdBrowserCreateNewTab),
				"RES_MenuBrowserNewTabCaption", "RES_MenuBrowserNewTabDesc", 0);

			tool6.BeginGroup = true;
			//tool6.Shortcut = Keys.Control | Keys.N;		TODO!!!
			
			AppToolCommand tool7 = new AppToolCommand("cmdBrowserNewExternalWindow", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdOpenLinkInExternalBrowser),
				"RES_MenuBrowserNewExternalWindowCaption", "RES_MenuBrowserNewExternalWindowDesc", 39);

			tb.Items.AddRange(new ToolbarItemBase[]{tool0,tool1,tool2,tool3,navigateComboBox,tool5,tool6,tool7});

			tb.StretchItem = navigateComboBox;
			//tb.Stretch = true;
		}


		private void CreateSearchToolbar(TD.SandBar.ToolBar tb) {

			_searchEngineImages.Images.Add(_browserImages.Images[37], Color.Magenta);

			searchComboBox = new TD.SandBar.ComboBoxItem();
			searchComboBox.Text = Resource.Manager["RES_MenuDoSearchComboBoxCaption"];
			searchComboBox.ToolTipText = Resource.Manager["RES_MenuDoSearchComboBoxDesc"];
			searchComboBox.ComboBox.KeyDown += new KeyEventHandler(OnSearchComboBoxKeyDown);
			searchComboBox.ComboBox.KeyPress += new KeyPressEventHandler(OnAnyEnterKeyPress);
			searchComboBox.ComboBox.DragOver += new DragEventHandler(OnSearchComboBoxDragOver);
			searchComboBox.ComboBox.DragDrop += new DragEventHandler(OnSearchComboBoxDragDrop);
			searchComboBox.ComboBox.Capture = false;

			searchComboBox.ComboBox.AllowDrop = true;
			searchComboBox.MinimumControlWidth = 150;
			Size previous = searchComboBox.ComboBox.Size;
			searchComboBox.ComboBox.Size = new Size(150, previous.Height);	// set a initial size
			searchComboBox.ComboBox.DropDownWidth = 350;

			//tb.Resize += new EventHandler(this.OnBarResize);
			//navigateComboBox.SelectedIndexChanged += new EventHandler(NavigationSelectedIndexChanged);

			AppToolMenuCommand tool2 = new AppToolMenuCommand("cmdSearchGo", 
				owner.Mediator, new ExecuteCommandHandler(this.CmdSearchGo),
				"RES_MenuDoSearchWebCaption", "RES_MenuDoSearchWebDesc", 0);

			_searchesGoCommand = tool2;	// need the reference later to build the search dropdown

			tb.Items.AddRange(new ToolbarItemBase[]{searchComboBox,tool2});
			tb.StretchItem = searchComboBox;
			//tb.Stretch = true;
		}

		#endregion

		#region Init DocManager
		private void InitDocumentManager() {
			
			_docContainer.SuspendLayout();
			
			_docContainer.LayoutSystem.SplitMode = Orientation.Vertical;
			
			_docFeedDetails.Text = Resource.Manager["RES_FeedDetailDocumentTabCaption"];
			_docFeedDetails.TabImage = _listImages.Images[0];
			_docFeedDetails.Tag = this; // I'm the ITabState implementor
			if (Win32.IsOSAtLeastWindowsXP)
				ColorEx.ColorizeOneNote(_docFeedDetails, 0);

			_docContainer.ShowControlContextMenu +=new ShowControlContextMenuEventHandler(OnDocContainerShowControlContextMenu);
			_docContainer.MouseDown += new MouseEventHandler(OnDocContainerMouseDown);
			_docContainer.DocumentClosing += new DocumentClosingEventHandler(OnDocContainerDocumentClosing);
			_docContainer.ActiveDocumentChanged +=new ActiveDocumentEventHandler(OnDocContainerActiveDocumentChanged);
			_docContainer.DoubleClick += new EventHandler(OnDocContainerDoubleClick);

			panelFeedDetails.Dock = DockStyle.Fill;
			_docContainer.ResumeLayout(false);
		}
		#endregion

		#region DockHost init routines

		private void InitDockHosts() {
			sandDockManager.DockingFinished += new EventHandler(OnDockManagerDockingFinished);
			sandDockManager.DockingStarted += new EventHandler(OnDockManagerDockStarted);
			dockSubscriptions.Closed += new EventHandler(OnDockControlSubscriptionsClosed);
		}


		#endregion

		#region TrayIcon routines
		private void InitTrayIcon() {
			if 	(this.components == null)
				this.components = new System.ComponentModel.Container();
			_trayAni = new NotifyIconAnimation(this.components);
			_trayAni.DoubleClick += new EventHandler(this.OnTrayIconDoubleClick);
			_trayAni.BalloonClick+= new EventHandler(this.OnTrayIconDoubleClick);
			_trayAni.BalloonTimeout += new EventHandler(this.OnTrayAniBalloonTimeoutClose);
			_trayAni.ContextMenu = _notifyContextMenu;

			//_trayManager = new TrayStateManager(_trayAni, imageTrayAnimation);
			_trayManager = new TrayStateManager(_trayAni, null);
			_trayManager.SetState(ApplicationTrayState.NormalIdle);
		}
		#endregion

		#region Statusbar routines
		
		private void InitStatusBar() {
			_status.PanelClick += new StatusBarPanelClickEventHandler(OnStatusPanelClick);
			_status.LocationChanged += new EventHandler(OnStatusPanelLocationChanged);
			statusBarBrowserProgress.Width = 0;
			progressBrowser.Visible = false;
		}

		private void OnDockManagerDockStarted(object sender, EventArgs e) {
			SetBrowserStatusBarText(Resource.Manager["RES_DragDockablePanelInfo"]);
		}

		private void OnDockManagerDockingFinished(object sender, EventArgs e) {
			SetBrowserStatusBarText(String.Empty);
		}

		private void OnDockControlSubscriptionsClosed(object sender, EventArgs e) {
			owner.Mediator.SetChecked("-cmdToggleTreeViewState");
		}

		#endregion

		#region Callback and event handler routines

		private void CmdNop(ICommand sender) {
			// Nop: no operation here
		}

		private void CmdOpenLinkInExternalBrowser(ICommand sender) {
			owner.OpenUrlInExternalBrowser(UrlText);
		}
		
		private void CmdToggleMainTBViewState(ICommand sender) {
			toolBarMain.Visible = !owner.Mediator.IsCommandComponentChecked("cmdToggleMainTBViewState");
			owner.Mediator.SetChecked(toolBarMain.IsOpen ,"cmdToggleMainTBViewState");
			owner.GuiSettings.SetProperty(Name+"/sandBar.toolBarMain.Visible", toolBarMain.Visible);
//			this.DelayTask(DelayedTasks.SaveUIConfiguration);	// save state
		}
		private void CmdToggleWebTBViewState(ICommand sender) {
			toolBarBrowser.Visible = !owner.Mediator.IsCommandComponentChecked("cmdToggleWebTBViewState");
			owner.Mediator.SetChecked(toolBarBrowser.IsOpen ,"cmdToggleWebTBViewState");
			owner.GuiSettings.SetProperty(Name+"/sandBar.toolBarBrowser.Visible", toolBarBrowser.Visible);
//			this.DelayTask(DelayedTasks.SaveUIConfiguration);	// save state
		}
		private void CmdToggleWebSearchTBViewState(ICommand sender) {
			toolBarWebSearch.Visible = !owner.Mediator.IsCommandComponentChecked("cmdToggleWebSearchTBViewState");
			owner.Mediator.SetChecked(toolBarWebSearch.IsOpen ,"cmdToggleWebSearchTBViewState");
			owner.GuiSettings.SetProperty(Name+"/sandBar.toolBarWebSearch.Visible", toolBarWebSearch.Visible);
//			this.DelayTask(DelayedTasks.SaveUIConfiguration);	// save state
		}

		private void OnMenuItemViewToolbarsBeforePopup(object sender, MenuPopupEventArgs e) {
			owner.Mediator.SetChecked(toolBarMain.IsOpen ,"cmdToggleMainTBViewState");
			owner.Mediator.SetChecked(toolBarBrowser.IsOpen ,"cmdToggleWebTBViewState");
			owner.Mediator.SetChecked(toolBarWebSearch.IsOpen ,"cmdToggleWebSearchTBViewState");
		}

		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to view the feed descriptions docked panel.
		private void CmdDockShowFeedDescriptions(ICommand sender) {
			if (owner.Mediator.IsCommandComponentChecked("cmdToggleTreeViewState")) {
				dockSubscriptions.Close();	// Close event not fired:
				owner.Mediator.SetChecked("-cmdToggleTreeViewState");
			} else {
				if (!dockSubscriptions.IsOpen)
					dockSubscriptions.Open();
				dockSubscriptions.Activate();
				owner.Mediator.SetChecked("+cmdToggleTreeViewState");
			}
		}

		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to view the feed descriptions docked panel.
		private void CmdDockShowRssSearch(ICommand sender) {
			if (!dockSearch.IsOpen)
				dockSearch.Open();
			dockSearch.Activate();
		}

		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to close the selected doc tab.
		private void CmdDocTabCloseSelected(ICommand sender) {
			Point pos = (_contextMenuCalledAt != Point.Empty ? _contextMenuCalledAt : Cursor.Position);
			ControlLayoutSystem underMouse = _docContainer.GetLayoutSystemAt(_docContainer.PointToClient(pos)) as ControlLayoutSystem;
			if (underMouse != null) {
				DockControl docUnderMouse = underMouse.GetControlAt(_docContainer.PointToClient(pos));
				if (docUnderMouse != null) {	
					this.RemoveDocTab(docUnderMouse);
					return;
				}
			}
			// try simply to remove current active:
			this.RemoveDocTab(_docContainer.ActiveDocument);
		}

		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to close all doc tabs on the current strip.
		private void CmdDocTabCloseAllOnStrip(ICommand sender) {
			Point pos = (_contextMenuCalledAt != Point.Empty ? _contextMenuCalledAt : Cursor.Position);
			ControlLayoutSystem underMouse = _docContainer.GetLayoutSystemAt(_docContainer.PointToClient(pos)) as ControlLayoutSystem;
			if (underMouse == null) 
				underMouse = _docContainer.ActiveDocument.LayoutSystem;
			
			DockControl[] docs = new DockControl[underMouse.Controls.Count];
			underMouse.Controls.CopyTo(docs, 0);	// prevent InvalidOpException on Collections
			foreach (DockControl doc in docs) {
				ITabState state = (ITabState)doc.Tag;
				if (state.CanClose)
					_docContainer.RemoveDocument(doc);
			}
		}

		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to close all doc tabs on all strips.
		private void CmdDocTabCloseAll(ICommand sender) {
			DockControl[] docs = new DockControl[_docContainer.Documents.Length];
			_docContainer.Documents.CopyTo(docs, 0);		// prevent InvalidOpException on Collections
			foreach (DockControl doc in docs) {
				ITabState state = (ITabState)doc.Tag;
				if (state.CanClose)
					_docContainer.RemoveDocument(doc);
			}
		}

		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to toggle the splitted doc strip layout.
		private void CmdDocTabLayoutHorizontal(ICommand sender) {
			if (owner.Mediator.IsCommandComponentChecked("cmdDocTabLayoutHorizontal")) {
				_docContainer.LayoutSystem.SplitMode = Orientation.Vertical;
			} else {
				_docContainer.LayoutSystem.SplitMode = Orientation.Horizontal;
			}
			owner.Mediator.SetCommandComponentChecked("cmdDocTabLayoutHorizontal", (_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal));
		}


		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to toggle the feed detail layout.
		private void CmdFeedDetailLayoutPosTop(ICommand sender) {
			this.SetFeedDetailLayout(DockStyle.Top);
		}
		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to toggle the feed detail layout.
		private void CmdFeedDetailLayoutPosBottom(ICommand sender) {
			this.SetFeedDetailLayout(DockStyle.Bottom);
		}
		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to toggle the feed detail layout.
		private void CmdFeedDetailLayoutPosLeft(ICommand sender) {
			this.SetFeedDetailLayout(DockStyle.Left);
		}
		// not needed to be handled by RssBanditApplication.
		// Gets called, if a user want to toggle the feed detail layout.
		private void CmdFeedDetailLayoutPosRight(ICommand sender) {
			this.SetFeedDetailLayout(DockStyle.Right);
		}

		#region CmdFlag... routines
		public void CmdFlagNewsItemForFollowUp(ICommand sender) {
			if (this.CurrentSelectedFeedItem != null) {			
				this.MarkFeedItemsFlagged(Flagged.FollowUp);
			}
		}

		public void CmdFlagNewsItemNone(ICommand sender) {
			if (this.CurrentSelectedFeedItem != null) {			
				this.MarkFeedItemsFlagged(Flagged.None);
			}
		}

		public void CmdFlagNewsItemComplete(ICommand sender) {
			if (this.CurrentSelectedFeedItem != null) {			
				this.MarkFeedItemsFlagged(Flagged.Complete);
			}
		}

		public void CmdFlagNewsItemForward(ICommand sender) {
			if (this.CurrentSelectedFeedItem != null) {			
				this.MarkFeedItemsFlagged(Flagged.Forward);
			}
		}

		public void CmdFlagNewsItemRead(ICommand sender) {
			if (this.CurrentSelectedFeedItem != null) {			
				this.MarkFeedItemsFlagged(Flagged.Read);
			}
		}

		public void CmdFlagNewsItemForReply(ICommand sender) {
			if (this.CurrentSelectedFeedItem != null) {			
				this.MarkFeedItemsFlagged(Flagged.Reply);
			}
		}

		public void CmdFlagNewsItemForReview(ICommand sender) {
			if (this.CurrentSelectedFeedItem != null) {			
				this.MarkFeedItemsFlagged(Flagged.Review);
			}
		}
		#endregion

		#region CmdCopyNewsItemXXX and CmdCopyFeedXXX to Clipboard

		private void CmdCopyFeed(ICommand sender) {
			// dummy, just a submenu
			if (sender is AppContextMenuCommand)
				CurrentSelectedNode = null;
		}
		private void CmdCopyFeedLinkToClipboard(ICommand sender) {

			FeedTreeNodeBase node = CurrentSelectedNode;
			if (node != null && node.Type == FeedNodeType.Feed) {
		
				if (owner.FeedHandler.FeedsTable.ContainsKey((string)node.Tag))
					Clipboard.SetDataObject((string)node.Tag);
			}

			if (sender is AppContextMenuCommand)	// needed at the treeview
				CurrentSelectedNode = null;
		}
		
		private void CmdCopyFeedHomeLinkToClipboard(ICommand sender) {

			FeedTreeNodeBase node = CurrentSelectedNode;
			if (node != null && node.Type == FeedNodeType.Feed) {

				IFeedDetails fd = null;
				string link = null;

				if (owner.FeedHandler.FeedsTable.ContainsKey((string)node.Tag))
					fd = owner.FeedHandler.GetFeedInfo((string)node.Tag);
			
				if (fd != null) {
					link = fd.Link;
				} else {
					link = (string)node.Tag;
				}	

				if (!StringHelper.EmptyOrNull(link)) {
					Clipboard.SetDataObject(link);
				}
			}

			if (sender is AppContextMenuCommand)	// needed at the treeview
				CurrentSelectedNode = null;
		}

		private void CmdCopyFeedHomeTitleLinkToClipboard(ICommand sender) {

			FeedTreeNodeBase node = CurrentSelectedNode;
			if (node != null && node.Type == FeedNodeType.Feed) {

				IFeedDetails fd = null;
				string link = null, title = null;

				if (owner.FeedHandler.FeedsTable.ContainsKey((string)node.Tag))
					fd = owner.FeedHandler.GetFeedInfo((string)node.Tag);
			
				if (fd != null) {
					link = fd.Link;				title = fd.Title; 
				} else {
					link = (string)node.Tag;	title = node.Text;
				}	

				if (!StringHelper.EmptyOrNull(link)) {
					Clipboard.SetDataObject(String.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>", link, title, node.Text));
				}
			}
			if (sender is AppContextMenuCommand)	// needed at the treeview
				CurrentSelectedNode = null;
		}

		private void CmdCopyNewsItem(ICommand sender) {
			// dummy, just a submenu
		}
		private void CmdCopyNewsItemLinkToClipboard(ICommand sender) {

			if (this.listFeedItems.SelectedItems.Count == 0)
				return;

			NewsItem item = (NewsItem)((ThreadedListViewItem)this.listFeedItems.SelectedItems[0]).Key;

			if (item != null) {
				string link = item.Link;
				if (!StringHelper.EmptyOrNull(link)) {
					Clipboard.SetDataObject(link);
				}
 
			}
		}

		private void CmdCopyNewsItemTitleLinkToClipboard(ICommand sender) {
			if (this.listFeedItems.SelectedItems.Count == 0)
				return;

			NewsItem item = (NewsItem)((ThreadedListViewItem)this.listFeedItems.SelectedItems[0]).Key;

			if (item != null) {
				string link = item.Link;
				if (!StringHelper.EmptyOrNull(link)) {
					string title = item.Title;
					if (!StringHelper.EmptyOrNull(title)) {
						Clipboard.SetDataObject(String.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>", link, item.Feed.title,item.Title));
					} else {
						Clipboard.SetDataObject(link);
					}
				}
 
			}
		}

		private void CmdCopyNewsItemContentToClipboard(ICommand sender) {
			if (this.listFeedItems.SelectedItems.Count == 0)
				return;

			NewsItem item = (NewsItem)((ThreadedListViewItem)this.listFeedItems.SelectedItems[0]).Key;

			if (item != null) {
				string content = item.Content;
				if (!StringHelper.EmptyOrNull(content)) {
					Clipboard.SetDataObject(content);
				} else {
					this.CmdCopyNewsItemTitleLinkToClipboard(sender);
				}
 
			}
		}

		#endregion
		
		#region CmdFinder.. routines

		// <summary>
		/// Re-runs the search and repopulates the search folder.
		/// </summary>
		/// <remarks>Assumes that this is called when the current selected node is a search folder</remarks>
		/// <param name="sender"></param>
		private void CmdRefreshFinder(ICommand sender){
			EmptyListView();
			htmlDetail.Clear();
			FinderNode afn = TreeSelectedNode as FinderNode;
			if(afn != null){
				afn.Clear(); 	
				afn.UpdateReadStatus(afn, 0); 
				if (afn.Finder != null && !StringHelper.EmptyOrNull(afn.Finder.ExternalSearchUrl)) {
					// does also initiates the local search if merge is true:
					AsyncStartRssRemoteSearch(afn.Finder.ExternalSearchPhrase, afn.Finder.ExternalSearchUrl, afn.Finder.ExternalResultMerged, true);
				} else {
					AsyncStartNewsSearch(afn); 		
				}
			}
		}


		/// <summary>
		/// Marks all the items in a search folder as read
		/// </summary>
		/// <param name="sender"></param>
		private void CmdMarkFinderItemsRead(ICommand sender){		
			this.SetFeedItemsReadState(this.listFeedItems.Items, true); 
			this.UpdateTreeStatus(owner.FeedHandler.FeedsTable);
		}

		/// <summary>
		/// Renames a search folder
		/// </summary>
		/// <param name="sender"></param>
		private void CmdRenameFinder(ICommand sender){
		
			if(this.CurrentSelectedNode!= null)
				this.DoEditTreeNodeLabel();
		}
		
		/// <summary>
		/// Allows the user to create a new search folder
		/// </summary>
		/// <param name="sender"></param>
		private void CmdNewFinder(ICommand sender) {
			this.CmdNewRssSearch(sender); 
		}

		/// <summary>
		/// Deletes a search folder
		/// </summary>
		/// <param name="sender"></param>
		private void CmdDeleteFinder(ICommand sender) {

			if (owner.MessageQuestion("RES_MessageBoxDeleteThisFinderQuestion") == DialogResult.Yes) {
						
				if (this.NodeEditingActive)
					return;

				FeedTreeNodeBase node = this.CurrentSelectedNode;
				WalkdownThenDeleteFinders(node);
				node.UpdateReadStatus(node, 0);

				try {
					node.Parent.Nodes.Remove(node);
				} catch {}

				if (sender is AppContextMenuCommand)
					this.CurrentSelectedNode = null;
			}
		}

		/// <summary>
		/// Helper. Work recursive on the startNode down to the leaves.
		/// Then delete all child categories and FeedNode refs in owner.FeedHandler.
		/// </summary>
		/// <param name="startNode">Node to start with. The startNode itself is 
		/// considered on delete.</param>
		/// <param name="startNode">new full category name (long name, with all the '\').</param>
		private void WalkdownThenDeleteFinders(FeedTreeNodeBase startNode) {
			if (startNode == null) return;

			if (startNode.Type == FeedNodeType.Finder) {
				FinderNode agn = startNode as FinderNode;
				if (agn != null) {
					owner.FinderList.Remove(agn.Finder);
				}
			}
			else {	// other
				for (FeedTreeNodeBase child = startNode.FirstNode; child != null; child = child.NextNode) {
					WalkdownThenDeleteFinders(child);
				}
			}
		}

		private void CmdDeleteAllFinder(ICommand sender) {

			if (owner.MessageQuestion("RES_MessageBoxDeleteAllFindersQuestion") == DialogResult.Yes) {

				owner.FinderList.Clear();
				owner.SaveSearchFolders();

				FinderRootNode finderRoot = this.GetRoot(RootFolderType.Finder) as FinderRootNode;
			
				if (finderRoot != null) {
					finderRoot.Nodes.Clear();
					finderRoot.UpdateReadStatus(finderRoot, 0);
				}
			}

			if (sender is AppContextMenuCommand)
				this.CurrentSelectedNode = null;
		}

		private void CmdShowFinderProperties(ICommand sender) {
			
			this.CmdDockShowRssSearch(null);

			FinderNode node = this.CurrentSelectedNode as FinderNode;
			if (node != null) {
				this.SearchDialogSetSearchCriterias(node); // node.Finder.SearchCriterias, node.Finder.FullPath );
			}
			
			if (sender is AppContextMenuCommand)
				this.CurrentSelectedNode = null;
		}

		private void CmdSubscribeToFinderResult(ICommand sender) {
			FinderNode node = this.CurrentSelectedNode as FinderNode;
			if (node != null && node.Finder != null) {
				if (!StringHelper.EmptyOrNull(node.Finder.ExternalSearchUrl)) {
					owner.CmdNewFeed(node.Key, node.Finder.ExternalSearchUrl, node.Finder.ExternalSearchPhrase);
				}
			}
		}

		#endregion


		#region CmdListviewColumn Layout 
		
		private void CmdColumnChooserUseFeedLayoutGlobal(ICommand sender) {
			SetGlobalFeedColumnLayout(FeedNodeType.Feed, listFeedItems.FeedColumnLayout);
			listFeedItems.ApplyLayoutModifications();
		}
		private void CmdColumnChooserUseCategoryLayoutGlobal(ICommand sender) {
			SetGlobalFeedColumnLayout(FeedNodeType.Category, listFeedItems.FeedColumnLayout);
			listFeedItems.ApplyLayoutModifications();
		}

		private void CmdColumnChooserResetToDefault(ICommand sender) {
			SetFeedHandlerFeedColumnLayout(CurrentSelectedNode, null);
			listFeedItems.ApplyLayoutModifications();	// do not save temp. changes to the node
			ArrayList items = this.NewsItemListFrom(listFeedItems.Items);
			listFeedItems.FeedColumnLayout = this.GetFeedColumnLayout(CurrentSelectedNode);	// also clear's the listview
			this.RePopulateListviewWithContent(items);
		}

		private void CmdToggleListviewColumn(ICommand sender) {
			if (listFeedItems.Columns.Count > 1) {	// show at least one column
				string[] name = sender.CommandID.Split(new char[]{'.'});

				if (sender.Mediator.IsCommandComponentChecked(sender.CommandID)) {
					listFeedItems.Columns.Remove(name[1]);
				} else {
					this.AddListviewColumn(name[1], 120);
					this.RePopulateListviewWithCurrentContent();
				}
				sender.Mediator.SetChecked(!sender.Mediator.IsCommandComponentChecked(sender.CommandID), sender.CommandID);
			}
		}

		private void RefreshListviewColumnContextMenu() {
			ColumnKeyIndexMap map = listFeedItems.Columns.GetColumnIndexMap();

			foreach (string colID in Enum.GetNames(typeof(NewsItemSortField))) {
				owner.Mediator.SetChecked(map.ContainsKey(colID), "cmdListviewColumn." + colID);
			}

			bool enableIndividual = (CurrentSelectedNode != null  && (CurrentSelectedNode.Type == FeedNodeType.Feed || CurrentSelectedNode.Type == FeedNodeType.Category));
			owner.Mediator.SetEnable(enableIndividual, "cmdColumnChooserResetToDefault");
		}

		private void AddListviewColumn(string colID, int width) {
			switch (colID) {
				case "Title": 
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionHeadline"], typeof(string), width, HorizontalAlignment.Left); break;
				case "Subject": 
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionTopic"], typeof(string), width, HorizontalAlignment.Left); break;
				case "Date": 
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionDate"], typeof(DateTime), width, HorizontalAlignment.Left); break;
				case "FeedTitle":
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionFeedTitle"], typeof(string), width, HorizontalAlignment.Left); break;
				case "Author":
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionCreator"], typeof(string), width, HorizontalAlignment.Left); break;
				case "CommentCount":
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionCommentCount"], typeof(int), width, HorizontalAlignment.Left); break;
				case "Enclosure":	//TODO: should have a paperclip picture, int type may change to a specific state (string)
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionEnclosure"], typeof(int), width, HorizontalAlignment.Left); break;
				case "Flag":
					listFeedItems.Columns.Add(colID, Resource.Manager["RES_ListviewColumnCaptionFlagStatus"], typeof(string), width, HorizontalAlignment.Left); break;
				default:
					Trace.Assert(false, "AddListviewColumn::NewsItemSortField NOT handled: " + colID);
					break;
			}
		}

		private void ResetFeedDetailLayoutCmds() {
			owner.Mediator.SetChecked(false, "cmdFeedDetailLayoutPosTop","cmdFeedDetailLayoutPosLeft","cmdFeedDetailLayoutPosRight","cmdFeedDetailLayoutPosBottom");
		}

		#endregion

		private void SetFeedDetailLayout(DockStyle style) {
			ResetFeedDetailLayoutCmds();
			panelFeedItems.Dock = style;
			detailsPaneSplitter.Dock = style;
			if (style == DockStyle.Left || style == DockStyle.Right) {
				detailsPaneSplitter.Cursor = System.Windows.Forms.Cursors.VSplit;
				panelFeedItems.Width = this.panelFeedDetails.Width / 3;
			}else{
				detailsPaneSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
				panelFeedItems.Height = this.panelFeedDetails.Height / 2;
			}
			owner.Mediator.SetCommandComponentChecked("cmdFeedDetailLayoutPos" + detailsPaneSplitter.Dock.ToString(), true );
		}

		private bool RemoveDocTab(DockControl doc){
			if (doc == null)
				doc = _docContainer.ActiveDocument;

			if (doc == null) 
				return false;

			ITabState state = doc.Tag as ITabState;
			if (state != null && state.CanClose) {
				try {
					_docContainer.RemoveDocument(doc);
					HtmlControl browser = doc.Controls[0] as HtmlControl;
					if (browser != null) {
						browser.Tag = null;	// remove ref to containing doc
						browser.Dispose();
					}
				} catch (Exception ex) {
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
		public void OnGenericListviewCommand(int index, bool hasConfig) {
			Syndication.Extensibility.IBlogExtension ibe = (Syndication.Extensibility.IBlogExtension)addIns[index];
			if(hasConfig) {
				try {
					ibe.Configure(this);
				}
				catch (Exception e) {
					_log.Error("IBlogExtension configuration exception", e);
					owner.MessageError("RES_ExceptionIBlogExtensionFunctionCall", "Configure()", e.Message);
				}
			} else {
				//if (ibe.HasEditingGUI) //TODO...? What we have to do here...?
				if (CurrentSelectedFeedItem != null) {
					try {
						ibe.BlogItem(CurrentSelectedFeedItem, false);
					} catch (Exception e) {
						_log.Error("IBlogExtension command exception", e);
						owner.MessageError("RES_ExceptionIBlogExtensionFunctionCall", "BlogItem()", e.Message);
					}
				}
			}
		}

		private void OnFormHandleCreated(object sender, EventArgs e) {
			// if the form is started minimized (via Shortcut Properties "Run-Minimized"
			// it seems the OnLoad event does not gets fired, so we call it here...
			if (initialStartupState == FormWindowState.Minimized)
				this.OnLoad(this, new EventArgs());
			// init idle task event handler:
			Application.Idle += new EventHandler(this.OnApplicationIdle);
		}
		
		private void OnLoad(object sender, System.EventArgs eva) {
		
			// do not display the ugly form init/resizing...
			Win32.ShowWindow(this.Handle, Win32.ShowWindowStyles.SW_HIDE);

			_uiTasksTimer.Tick += new EventHandler(this.OnTasksTimerTick);

			LoadUIConfiguration();
			SetTitleText(String.Empty);

			InitSearchEngines();
			CheckForAddIns();
			this.InitFilter();

			this.SetGuiStateFeedback(Resource.Manager["RES_GUIStatusLoadingFeedlist"]);

			IdleTask.AddTask(IdleTasks.InitOnFinishLoading);
			this.DelayTask(DelayedTasks.InitOnFinishLoading);
		}

		/// <summary>
		/// Provide the entry point to the delayed loading of the feed list
		/// </summary>
		/// <param name="theStateObject">The timer callback parameter</param>
		private void OnFinishLoading(object theStateObject) {
			
			if (owner.CommandLineArgs.StartInTaskbarNotificationAreaOnly || _initialStartupTrayVisibleOnly) {
				// forced to show in Taskbar Notification Area
				Win32.ShowWindow(this.Handle, Win32.ShowWindowStyles.SW_HIDE);
				owner.GuiSettings.SetProperty(Name+"/TrayOnly.Visible", true);
			} else {
				this.Activate();
			}
			
			// for UI save/restore state manangement
			this.ConnectToolbarStateEvents(true);

			Splash.Status = Resource.Manager["RES_GUIStatusRefreshConnectionState"];
			// refresh the Offline menu entry checked state
			owner.UpdateInternetConnectionState();

			// refresh the internal browser component, that does not know immediatly
			// about a still existing Offline Mode...
			Utils.SetIEOffline(owner.InternetConnectionOffline);

#if USEAUTOUPDATE
			owner.CmdCheckForUpdates(AutoUpdateMode.OnApplicationStart);
#endif

			Splash.Close();
			owner.AskAndCheckForDefaultAggregator();

			//Trace.WriteLine("ATTENTION!. REFRESH TIMER DISABLED FOR DEBUGGING!");
#if !NOAUTO_REFRESH
			// start the refresh timer, if we do not have to refresh on startup:
			_timerRefreshFeeds.Start();
#endif

			RssBanditApplication.SetWorkingSet(750000,300000);
		}

		private void OnFormMove(object sender, EventArgs e) {
			if (base.WindowState == FormWindowState.Normal) {
				_formRestoreBounds.Location = base.Location;
			}
		}

		private void OnFormResize(object sender, EventArgs e) {
			if (base.WindowState == FormWindowState.Normal) {
				_formRestoreBounds.Size = base.Size;
				// adjust the MaximumSize of the dock hosts:
				leftSandDock.MaximumSize = rightSandDock.MaximumSize = this.ClientSize.Width - 20;
				topSandDock.MaximumSize = bottomSandDock.MaximumSize = this.ClientSize.Height - 20;
			}
			if (base.WindowState != FormWindowState.Minimized) {
				if (!menuBarMain.IsOpen || !menuBarMain.Visible) {	
					//BUGBUG in SandBar. If exit started on Tray, they are saved not visible at all !?
					// detach events, that may trigger a new save .settings.xml
					this.ConnectToolbarStateEvents(false);
					// Mod. if the renderer/layout cause a visibility change:
					menuBarMain.Visible = toolBarBrowser.Visible = toolBarWebSearch.Visible = toolBarMain.Visible = true;
					this.ConnectToolbarStateEvents(true);	// attach the events again back
					this.OnAnyToolbarStateChanged(this, EventArgs.Empty);
					//this.DelayTask(DelayedTasks.SaveUIConfiguration);	// save state
				}
			}
			if (this.Visible) {
				owner.GuiSettings.SetProperty(Name+"/TrayOnly.Visible", false);
			}
		}

		/// <summary>
		/// Here is the Form minimize event handler
		/// </summary>
		/// <param name="sender">This form</param>
		/// <param name="e">Empty. See WndProc()</param>
		private void OnFormMinimize(object sender, System.EventArgs e) {
			if (owner.Preferences.HideToTrayAction == HideToTray.OnMinimize) {
				Win32.ShowWindow(this.Handle, Win32.ShowWindowStyles.SW_HIDE);
				owner.GuiSettings.SetProperty(Name+"/TrayOnly.Visible", true);
			}
		}


		/// <summary>
		/// Implements the IMessageFilter. 
		/// Helps grabbing all the important keys.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public virtual bool PreFilterMessage(ref Message m) {
			bool processed = false;
			const int WM_KEYDOWN = 0x100;
			const int WM_SYSKEYDOWN = 0x104;
		
			try {
				if (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN) {
					
					Keys msgKey = ((Keys)(int)m.WParam & Keys.KeyCode);
				
					if (msgKey == Keys.Tab) {
						//TODO

						if (Control.ModifierKeys == 0) {	// normal Tab navigation between controls
												
							Trace.WriteLine("PreFilterMessage[Tab Only], "); 

							if (this.treeFeeds.Visible) {	
								if (this.treeFeeds.Focused) {
									if (this.listFeedItems.Visible) {
										this.listFeedItems.Focus();
										if (this.listFeedItems.Items.Count > 0 && this.listFeedItems.SelectedItems.Count == 0) {
											this.listFeedItems.Items[0].Selected = true;
											this.listFeedItems.Items[0].Focused = true;
											this.OnFeedListItemActivate(this, new EventArgs());
										}
										processed = true;
									}	else if (this._docContainer.ActiveDocument != _docFeedDetails) {
										// a tabbed browser should get focus
										SetFocus2WebBrowser((HtmlControl)this._docContainer.ActiveDocument.Controls[0]); 
										processed = true;
									}
								} else if (this.listFeedItems.Focused) {
									SetFocus2WebBrowser(this.htmlDetail);	// detail browser should get focus
									processed = true;
								} else {
									// a IE browser focused
									//this.treeFeeds.Focus();
									//processed = true;
								}
							} else { // treefeeds.invisible:
								if (this.listFeedItems.Visible) {
									if (this.listFeedItems.Focused) {
										SetFocus2WebBrowser(this.htmlDetail);	// detail browser should get focus
										processed = true;
									} else {
										// a IE browser focused
										/*
															this.listFeedItems.Focus();
															if (this.listFeedItems.Items.Count > 0 && this.listFeedItems.SelectedItems.Count == 0) {
																this.listFeedItems.Items[0].Selected = true;
																this.OnFeedListItemActivate(this, new EventArgs());
															}
															processed = true;
															*/
									}
								}
							}// endif treefeeds.visible 
						
						} else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift &&
							(Control.ModifierKeys & Keys.Control) == 0) {			// Shift-Tab only
						
							Trace.WriteLine("PreFilterMessage[Shift-Tab Only]"); 
							if (this.treeFeeds.Visible) {
								if (this.treeFeeds.Focused) {
									if (this.listFeedItems.Visible) {
										SetFocus2WebBrowser(this.htmlDetail);	// detail browser should get focus
										processed = true;
									}	else if (this._docContainer.ActiveDocument != _docFeedDetails) {
										// a tabbed browser should get focus
										SetFocus2WebBrowser((HtmlControl)this._docContainer.ActiveDocument.Controls[0]); 
										processed = true;
									}
								} else if (this.listFeedItems.Focused) {
									this.treeFeeds.Focus();
									processed = true;
								} else {
									// a IE browser focused
									/*
														if (this.listFeedItems.Visible) {
															this.listFeedItems.Focus();
															if (this.listFeedItems.Items.Count > 0 && this.listFeedItems.SelectedItems.Count == 0)
																this.listFeedItems.Items[0].Selected = true;
															processed = true;
														}
														*/
								}
							} else { // treefeeds.invisible:
								if (this.listFeedItems.Visible) {
									if (this.listFeedItems.Focused) {
										SetFocus2WebBrowser(this.htmlDetail);	// detail browser should get focus
										processed = true;
									} else {
										// a IE browser focused
										/*
															this.listFeedItems.Focus();
															if (this.listFeedItems.Items.Count > 0 && this.listFeedItems.SelectedItems.Count == 0)
																this.listFeedItems.Items[0].Selected = true;
															processed = true;
															*/
									}
								}
							}		//endif treefeeds.visible
						}
					} else if (this.listFeedItems.Focused && _shortcutHandler.IsCommandInvoked("ExpandListViewItem", m.WParam)) {	// "+" on ListView opens the thread
						if (this.listFeedItems.Visible && this.listFeedItems.SelectedItems.Count > 0) {
							ThreadedListViewItem lvi = (ThreadedListViewItem)this.listFeedItems.SelectedItems[0];
							if (lvi.HasChilds && lvi.Collapsed) {
								lvi.Expanded = true;
								processed = true;
							}
						}
					} else if (this.listFeedItems.Focused && _shortcutHandler.IsCommandInvoked("CollapseListViewItem", m.WParam)) {	// "-" on ListView close the thread
						if (this.listFeedItems.Visible && this.listFeedItems.SelectedItems.Count > 0) {
							ThreadedListViewItem lvi = (ThreadedListViewItem)this.listFeedItems.SelectedItems[0];
							if (lvi.HasChilds && lvi.Expanded) {
								lvi.Collapsed = true;
								processed = true;
							}
						}
					} else if (_shortcutHandler.IsCommandInvoked("RemoveDocTab", m.WParam)) {	// Ctrl-F4: close a tab
						if (this.RemoveDocTab(_docContainer.ActiveDocument)) {
							processed = true;
						}
					} else if (_shortcutHandler.IsCommandInvoked("CatchUpCurrentSelectedNode", m.WParam)) {	// Ctrl-M: Catch up feed
						owner.CmdCatchUpCurrentSelectedNode(null);
						processed = true;
					} else if (_shortcutHandler.IsCommandInvoked("MarkFeedItemsUnread", m.WParam)) {	// Ctrl-U: close a tab
						owner.CmdMarkFeedItemsUnread(null);
						processed = true;

					//We've hard-coded SPACE as a Move to Next Item
					//But in that case, make sure there's not a modifier key pressed.
					} else if ((msgKey == Keys.Space && Control.ModifierKeys == 0) || _shortcutHandler.IsCommandInvoked("MoveToNextUnread", m.WParam)) {	// Space: move to next unread

						if (this.listFeedItems.Focused || this.treeFeeds.Focused &&
							!(this.TreeSelectedNode != null && this.TreeSelectedNode.IsEditing)) {
					
							this.MoveToNextUnreadItem();
							processed = true;
	
						} else if (this.textSearchExpression.Focused || this.treeFeeds.Focused ||
							this.navigateComboBox.ComboBox.Focused ||this.searchComboBox.ComboBox.Focused ||
							this.textFinderCaption.Focused ||
							(this.TreeSelectedNode != null && this.TreeSelectedNode.IsEditing)) {
							// ignore
						
						} else if (_docContainer.ActiveDocument == _docFeedDetails && !this.listFeedItems.Focused) {
							// browser detail pane has focus
							//Trace.WriteLine("htmlDetail.Focused:"+htmlDetail.Focused);
							IHTMLDocument2 htdoc = htmlDetail.Document2;
							if (htdoc != null) {
								IHTMLElement2 htbody = htdoc.GetBody();
								if (htbody != null) {
									int num1 = htbody.getScrollTop();
									htbody.setScrollTop(num1 + 20);
									int num2 = htbody.getScrollTop();
									if (num1 == num2) {
										this.MoveToNextUnreadItem();
										processed = true;
									}
								}
							}
						} else {
							// ignore, control should handle it
						}
					} else if (_shortcutHandler.IsCommandInvoked("InitiateRenameFeedOrCategory", m.WParam)) {		// rename within treeview
						if (this.treeFeeds.Focused) {
							this.InitiateRenameFeedOrCategory();
							processed = true;
						}
					} else if (_shortcutHandler.IsCommandInvoked("UpdateFeed", m.WParam)) {	// F5: UpdateFeed()
						this.CurrentSelectedNode = null;
						owner.CmdUpdateFeed(null);
						processed = true;
					} else if (_shortcutHandler.IsCommandInvoked("GiveFocusToUrlTextBox", m.WParam)) {	// Alt+F4 or F11: move focus to Url textbox
						this.navigateComboBox.ComboBox.Focus();
						processed = true;
					} else if (_shortcutHandler.IsCommandInvoked("GiveFocusToSearchTextBox", m.WParam)) {	// F12: move focus to Search textbox
						this.searchComboBox.ComboBox.Focus();
						processed = true;
					} else if ((msgKey == Keys.Delete && Control.ModifierKeys == 0) || _shortcutHandler.IsCommandInvoked("DeleteItem", m.WParam)) {	// Delete a feed or category,...
						// cannot be a shortcut, because then "Del" does not work when edit/rename a node caption :-(
						// But we can add alternate shortcuts via the config file.
						if (this.treeFeeds.Focused && this.TreeSelectedNode != null && !this.TreeSelectedNode.IsEditing) {
							FeedTreeNodeBase root = this.GetRoot(RootFolderType.MyFeeds);
							FeedTreeNodeBase current = this.CurrentSelectedNode;
							if (this.NodeIsChildOf(current, root)) {
								if (current.Type == FeedNodeType.Category) {
									owner.CmdDeleteCategory(null);
									processed = true;
								}
								if (current.Type == FeedNodeType.Feed) {
									owner.CmdDeleteFeed(null);
									processed = true;
								}
							}
						}
					}
				} else if (m.Msg == (int)Win32.Message.WM_LBUTTONDBLCLK ||
					m.Msg == (int)Win32.Message.WM_RBUTTONDBLCLK ||
					m.Msg == (int)Win32.Message.WM_MBUTTONDBLCLK ||
					m.Msg == (int)Win32.Message.WM_LBUTTONUP ||
					m.Msg == (int)Win32.Message.WM_MBUTTONUP ||
					m.Msg == (int)Win32.Message.WM_RBUTTONUP ||
					m.Msg == (int)Win32.Message.WM_XBUTTONDBLCLK  ||
					m.Msg == (int)Win32.Message.WM_XBUTTONUP) {

					_lastMousePosition = new Point(Win32.LOWORD(m.LParam), Win32.HIWORD(m.LParam));

					Control mouseControl = this._wheelSupport.GetTopmostChild(this, Control.MousePosition);
					_webUserNavigated = ( mouseControl is HtmlControl );	// set
					_webForceNewTab = false;
					if (_webUserNavigated) { // CONTROL-Click opens a new Tab
						_webForceNewTab = (Interop.GetAsyncKeyState(Interop.VK_CONTROL) < 0);
					}

				} else if (m.Msg == (int)Win32.Message.WM_MOUSEMOVE) {
					Point p = new Point(Win32.LOWORD(m.LParam), Win32.HIWORD(m.LParam));
					if (Math.Abs(p.X - _lastMousePosition.X) > 5  || 
						Math.Abs(p.Y - _lastMousePosition.Y) > 5 ) {
						//Trace.WriteLine(String.Format("Reset mouse pos. Old: {0} New: {1}", _lastMousePosition, p));
						_webForceNewTab = _webUserNavigated = false;	// reset
						_lastMousePosition = p;
					}
				}
			} catch (Exception ex) {
				_log.Error("PreFilterMessage() failed", ex);
			}
			return processed;
		}

		/// <summary>
		/// we are interested in an OnMinimized event
		/// </summary>
		/// <param name="m">Native window message</param>
		[SecurityPermissionAttribute(SecurityAction.LinkDemand)]
		protected override void WndProc(ref Message m) {
			try {
				if(m.Msg== (int)Win32.Message.WM_SIZE) {
					if(((int)m.WParam)==1 /*SIZE_MINIMIZED*/ && OnMinimize!=null) {
						OnMinimize(this,EventArgs.Empty);
					}
					//			} else if (m.Msg == (int)Win32.Message.WM_MOUSEMOVE) {
					//				Control ctrl =  this.GetChildAtPoint(this.PointToClient(MousePosition));
					//				if (ctrl != null && !ctrl.Focused && ctrl.CanFocus) {
					//					ctrl.Focus();
					//				}
				}else if (/* m.Msg == (int)WM_CLOSE || */ m.Msg == (int)Win32.Message.WM_QUERYENDSESSION ||
					m.Msg == (int)Win32.Message.WM_ENDSESSION){ 
				
					// This is here to deal with dealing with system shutdown issues
					// Read http://www.kuro5hin.org/story/2003/4/17/22853/6087#banditshutdown for details
					// FYI: you could also do so:
					// Microsoft.Win32.SystemEvents.SessionEnding += new SessionEndingEventHandler(this.OnSessionEnding);
					// but we already have the WndProc(), so we also handle this message here

					_forceShutdown = true;	// the closing handler ask for that now
					this.SaveUIConfiguration(true);
					owner.SaveApplicationState();
				} else if ( m.Msg == (int)Win32.Message.WM_CLOSE && owner.Preferences.HideToTrayAction != HideToTray.OnClose) {
					_forceShutdown = true;	// the closing handler ask for that now
					this.SaveUIConfiguration(true);
					owner.SaveApplicationState();
				}
			
				base.WndProc(ref m);
			} catch (Exception ex) {
				_log.Fatal("WndProc() failed with an exception", ex);
			}
		}

		private void OnFormMouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.XButton1)
				this.RequestBrowseAction(BrowseAction.NavigateBack);
			else if (e.Button == MouseButtons.XButton2)
				this.RequestBrowseAction(BrowseAction.NavigateForward);
		}

		private void OnFormActivated(object sender, System.EventArgs e) {
			Application.AddMessageFilter(this);
		}

		private void OnFormDeactivate(object sender, System.EventArgs e) {
			Application.RemoveMessageFilter(this);
		}

		private void OnDocContainerShowControlContextMenu(object sender, ShowControlContextMenuEventArgs e) {
			_contextMenuCalledAt = Cursor.Position;
			_docTabContextMenu.Show(_docContainer, e.Position);
		}

		private void OnDocContainerMouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				if (_docContainer.Visible) {		// we can only displ. the context menu on a visible control:
					_contextMenuCalledAt = Cursor.Position;
					_docTabContextMenu.Show(_docContainer, new Point(e.X, e.Y));
				}
			} else if (e.Button == MouseButtons.Middle) {
				OnDocContainerDoubleClick(sender, e);
			}
		}

		private void OnDocContainerDocumentClosing(object sender, DocumentClosingEventArgs e) {
			this.RemoveDocTab(e.DockControl);
		}
		
		private void OnDocContainerActiveDocumentChanged(object sender, ActiveDocumentEventArgs e) {
			RefreshDocumentState(e.NewActiveDocument);
			DeactivateWebProgressInfo();
		}

		private void OnDocContainerDoubleClick(object sender, EventArgs e) {
			Point p = _docContainer.PointToClient(Control.MousePosition);
			DocumentLayoutSystem lb = (DocumentLayoutSystem)_docContainer.GetLayoutSystemAt(p);
			if (lb != null) {
				DockControl doc = lb.GetControlAt(p);
				if (doc != null)
					this.RemoveDocTab(doc);
			}
		}

		/// <summary>
		/// GUI State persistence. Settings are: window position, splitter position, 
		/// floating window sizes, listview column order, sorting direction etc.
		/// This routine writes all of them to a centralized settings dictionary maintained
		/// by the Settings class.
		/// </summary>
		/// <param name="writer"></param>
		protected void OnSaveConfig(Settings writer) { 

			try {
				writer.SetProperty("version", 3);
			
				writer.SetProperty(Name+"/Bounds", BoundsToString(_formRestoreBounds));
				writer.SetProperty(Name+"/WindowState", (int) this.WindowState);

				writer.SetProperty(Name+"/panelFeedItems.Height", panelFeedItems.Height);
				writer.SetProperty(Name+"/panelFeedItems.Width", panelFeedItems.Width);

				writer.SetProperty(Name+"/docManager.WindowAlignment", (int)_docContainer.LayoutSystem.SplitMode);

				TD.SandDock.Rendering.Office2003Renderer sdRenderer = sandDockManager.Renderer as TD.SandDock.Rendering.Office2003Renderer;
				writer.SetProperty(Name+"/dockManager.LayoutStyle.Office2003", (sdRenderer != null));

				// workaround the issue described here: http://www.divil.co.uk/net/support/kb/article.aspx?id=14
				using (new CultureChanger("en-US")) {
					writer.SetProperty(Name+"/dockManager.LayoutInfo", sandDockManager.GetLayout());
					writer.SetProperty(Name+"/sandBar.LayoutInfo", sandBarManager.GetLayout(true));
				}

				writer.SetProperty(Name+"/feedDetail.LayoutInfo.Position", (int)detailsPaneSplitter.Dock );

				Office2003Renderer renderer = sandBarManager.Renderer as Office2003Renderer;
				writer.SetProperty(Name+"/sandBar.LayoutStyle.Office2003", (renderer != null));
			
			} catch (Exception ex) {
				_log.Error("Exception while writing config entries to .settings.xml", ex);
			}
		}

		/// <summary>
		/// GUI State persistence. Restore the control settings like window position,
		/// docked window states, toolbar button layout etc.
		/// </summary>
		/// <param name="reader"></param>
		protected void OnLoadConfig(Settings reader) {
			try {
				int version = (int)reader.GetProperty("version", 0, typeof(int));

				// read BEFORE set the WindowState or Bounds (that causes events, where we reset this setting to false)
				_initialStartupTrayVisibleOnly = reader.GetBoolean(Name+"/TrayOnly.Visible", false);

				Rectangle r = StringToBounds(reader.GetString(Name+"/Bounds", BoundsToString(this.Bounds)));
				if (r != Rectangle.Empty) {
					if (Screen.AllScreens.Length < 2) {	
						// if only one sreen, correct initial location to fit the screen
						if (r.X < 0) r.X = 0;
						if (r.Y < 0) r.Y = 0;
						if (r.X >= Screen.PrimaryScreen.WorkingArea.Width) r.X -= Screen.PrimaryScreen.WorkingArea.Width;
						if (r.Y >= Screen.PrimaryScreen.WorkingArea.Height) r.Y -= Screen.PrimaryScreen.WorkingArea.Height;
					}
					_formRestoreBounds = r;
					this.Bounds = r;
				}
			
				FormWindowState windowState = (FormWindowState) reader.GetInt32(Name+"/WindowState", 
					(int) this.WindowState);

				if (initialStartupState != FormWindowState.Normal && 
					this.WindowState != initialStartupState) {
					this.WindowState = initialStartupState;
				} else {
					this.WindowState = windowState;
				}


				DockStyle feedDetailLayout = (DockStyle)reader.GetInt32(Name+"/feedDetail.LayoutInfo.Position", (int)DockStyle.Top);
				if (feedDetailLayout != DockStyle.Top && feedDetailLayout != DockStyle.Left && feedDetailLayout != DockStyle.Right && feedDetailLayout != DockStyle.Bottom)
					feedDetailLayout = DockStyle.Top;
				this.SetFeedDetailLayout(feedDetailLayout);		// load before restore panelFeedItems dimensions!

				panelFeedItems.Height = reader.GetInt32(Name+"/panelFeedItems.Height", (this.panelFeedDetails.Height / 2));
				panelFeedItems.Width = reader.GetInt32(Name+"/panelFeedItems.Width", (this.panelFeedDetails.Width / 2));

				_docContainer.LayoutSystem.SplitMode = (Orientation)reader.GetInt32(Name+"/docManager.WindowAlignment", (int)_docContainer.LayoutSystem.SplitMode);
				owner.Mediator.SetCommandComponentChecked("cmdDocTabLayoutHorizontal", (_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal));

				// fallback layouts if something really goes wrong while laading:
				string fallbackSDM, fallbackSBM;

				// workaround the issue described here: http://www.divil.co.uk/net/support/kb/article.aspx?id=14
				using (new CultureChanger("en-US")) {
					
					fallbackSDM = sandDockManager.GetLayout();	// designtime layout
					fallbackSBM = sandBarManager.GetLayout(true);	// designtime layout

					try {
						sandDockManager.SetLayout(reader.GetString(Name+"/dockManager.LayoutInfo", fallbackSDM));
					} catch (Exception ex) {
						_log.Error("Exception on restore sandDockManager layout", ex);
						sandDockManager.SetLayout(fallbackSDM);
					}
				}

				bool office2003 =	reader.GetBoolean(Name+"/dockManager.LayoutStyle.Office2003", true);
				if (office2003) {
					TD.SandDock.Rendering.Office2003Renderer sdRenderer = new TD.SandDock.Rendering.Office2003Renderer();
					sdRenderer.ColorScheme = TD.SandDock.Rendering.Office2003Renderer.Office2003ColorScheme.Automatic;
					if (!RssBanditApplication.AutomaticColorSchemes) {
						sdRenderer.ColorScheme = TD.SandDock.Rendering.Office2003Renderer.Office2003ColorScheme.Standard;
					}
					sandDockManager.Renderer = sdRenderer;
					_docContainer.Renderer = sdRenderer;
				} else {
					TD.SandDock.Rendering.WhidbeyRenderer sdRenderer  = new TD.SandDock.Rendering.WhidbeyRenderer();
					sandDockManager.Renderer = sdRenderer;
					_docContainer.Renderer = sdRenderer;
				}

				// workaround the issue described here: http://www.divil.co.uk/net/support/kb/article.aspx?id=14
				using (new CultureChanger("en-US")) {
					try {
						if (version < 2) {// older version found: use historical implementation
							sandBarManager.SetLayout(reader.GetString(Name+"/sandBar.LayoutInfo", sandBarManager.GetLayout(false)));
						} else {	// new sandbar version is able to handle it
							sandBarManager.SetLayout(reader.GetString(Name+"/sandBar.LayoutInfo", fallbackSBM));
						}
					} catch (Exception ex) {
						_log.Error("Exception on restore sandBarManager layout", ex);
						sandBarManager.SetLayout(fallbackSBM);
					}
				}

				office2003 =	reader.GetBoolean(Name+"/sandBar.LayoutStyle.Office2003", true);
				if (office2003) {
					Office2003Renderer renderer = new Office2003Renderer();
					//renderer.CustomColors = true;
					renderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Automatic;
					if (!RssBanditApplication.AutomaticColorSchemes) {
						renderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Standard;
					}
					sandBarManager.Renderer = renderer;
				} else {
					Office2002Renderer renderer = new Office2002Renderer();
					sandBarManager.Renderer = renderer;
				}

				sandBarManager.MenuBar.Visible = true;	// always visible

				// Sandbar's toolbars state are not valid in the case the form is minimized.
				// So we restore toolbar visbility separately (if app was closed from tray icon, the serialized info visible state is wrong):
				toolBarMain.Visible = reader.GetBoolean(Name+"/sandBar.toolBarMain.Visible", true);
				toolBarBrowser.Visible = reader.GetBoolean(Name+"/sandBar.toolBarBrowser.Visible", true);
				toolBarWebSearch.Visible = reader.GetBoolean(Name+"/sandBar.toolBarWebSearch.Visible", true);

				owner.Mediator.SetChecked(dockSubscriptions.Visible, "cmdToggleTreeViewState");

				owner.Mediator.SetChecked(toolBarMain.IsOpen, "cmdToggleMainTBViewState");
				owner.Mediator.SetChecked(toolBarBrowser.IsOpen, "cmdToggleWebTBViewState");
				owner.Mediator.SetChecked(toolBarWebSearch.IsOpen, "cmdToggleWebSearchTBViewState");

			} catch (Exception ex) {
				_log.Error("Exception while loading .settings.xml", ex);
			}
		}


		private void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (owner.Preferences.HideToTrayAction == HideToTray.OnClose &&
				_forceShutdown == false) {
				e.Cancel = true;
				Win32.ShowWindow(this.Handle, Win32.ShowWindowStyles.SW_HIDE);
				owner.GuiSettings.SetProperty(Name+"/TrayOnly.Visible", true);
			}
			else {
				_trayAni.Visible = false;
				this._toastNotifier.Dispose();
				this.SaveUIConfiguration(true);
			}
		}

		private void LoadUIConfiguration() {
			try {
				this.OnLoadConfig(owner.GuiSettings);
			}
			catch (Exception ex) {
				_log.Error("Load .settings.xml failed", ex);
			}
		}

		/// <summary>
		/// Called to build and re-build the search engine's Gui representation(s)
		/// </summary>
		public void InitSearchEngines() {
			if (!owner.SearchEngineHandler.EnginesLoaded || !owner.SearchEngineHandler.EnginesOK)
				owner.LoadSearchEngines();
			BuildSearchDropDown();	// call it always, it also enable/disable the button
		}

		private void BuildSearchDropDown() {

			if (owner.SearchEngineHandler.Engines.Count == 0) {
				searchComboBox.Enabled = false;
				owner.Mediator.SetEnable("-cmdSearchGo");				
				return;
			}

			_searchesGoCommand.Items.Clear();

			foreach (SearchEngine engine in owner.SearchEngineHandler.Engines) {

				AppMenuCommand item = new AppMenuCommand("cmdExecuteSearchEngine"+engine.Title, 
					owner.Mediator, new ExecuteCommandHandler(this.CmdExecuteSearchEngine), 
					engine.Title, engine.Description);

				item.Tag = engine;

				if (engine.ImageName != null && engine.ImageName.Trim().Length > 0) {
					string p = Path.Combine(RssBanditApplication.GetSearchesPath(), engine.ImageName);
					if (File.Exists(p)) {
						Icon ico = null;
						Image img = null;
						try {
							if (Path.GetExtension(p).ToLower().EndsWith("ico")) {
								ico = new Icon(p);
							} else {
								img = Image.FromFile(p);
							}
						}
						catch (Exception e) {
							_log.Error("Exception reading bitmap or Icon for searchEngine '" + engine.Title + "'.", e);
						}
						if (ico != null) {
							item.Icon = ico;
						} else if (img != null) {
							item.ImageIndex = _searchEngineImages.Images.Add(img, Color.Magenta);
						}
					}
				}
				_searchesGoCommand.Items.AddRange(new MenuButtonItem[]{item});

			}//end foreach

			searchComboBox.Enabled = true;
			owner.Mediator.SetEnable("+cmdSearchGo");
		}

		/// <summary>
		/// Iterates through the treeview and highlights all feed titles that 
		/// have unread messages. 
		/// </summary>
		private void UpdateTreeStatus(FeedsCollection feedsTable){
			this.UpdateTreeStatus(feedsTable, RootFolderType.MyFeeds);
		}
		private void UpdateTreeStatus(FeedsCollection feedsTable, RootFolderType rootFolder) {
			if (feedsTable == null) return;
			if (feedsTable.Count == 0) return;

			FeedTreeNodeBase root = this.GetRoot(rootFolder);

			if (root == null)	// no root nodes
				return;

			// traverse driven by feedsTable. Usually the feeds count with
			// new messages should be smaller than the tree nodes count.
			foreach (feedsFeed f in feedsTable.Values) {
				FeedTreeNodeBase tn = GetTreeNodeForItem(root, f);
				if (f.containsNewMessages) {
					tn.UpdateReadStatus(tn, CountUnreadFeedItems(f));
				} else {
					tn.UpdateReadStatus(tn, 0 );
				}
			}
		}
		#endregion
		
		#region event handlers for widgets not implementing ICommand
		private void OnTreeFeedMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {

			try {
				TreeView tv = (TreeView)sender;
				FeedTreeNodeBase selectedNode = (FeedTreeNodeBase)tv.GetNodeAt (e.X ,e.Y ); 						

				if (e.Button == MouseButtons.Right) {

					//if the right click was on a treeview node then display the 
					//appropriate context node depending on whether it was over 
					//a feed node, category node or the top-level node. 
					if(selectedNode!= null) {
						selectedNode.UpdateContextMenu();
						// refresh context menu items
						RefreshTreeFeedContextMenus(selectedNode );
					}
					else {
						tv.ContextMenu = null; // no context menu
					}
					this.CurrentSelectedNode = selectedNode;
				
				} else {
					// cleanup temp node ref., needed if a user dismiss the context menu
					// without selecting an action
					if((this.CurrentSelectedNode != null) && (selectedNode != null)){
					   
						//this handles left click of currently selected feed after selecting
						//an item in the listview. For some reason no afterselect or beforeselect
						//events are fired so we do the work here. 
						if(Object.ReferenceEquals(this.CurrentSelectedNode, selectedNode)){
							this.listFeedItems.SelectedItems.Clear();
							MoveFeedDetailsToFront();
							RefreshFeedDisplay(selectedNode, false);
						}else{
							this.CurrentSelectedNode = null;	
						}
					}
				}
				
			} catch (Exception ex) {
				_log.Error("Unexpected exception in OnTreeFeedMouseDown()", ex);
			}
		}

		private void OnTreeFeedMouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
			try {
				if (CurrentDragNode != null) {	// this code does not have any effect :-((
					// working around the missing DragHighlight property of the treeview :-(
					Point p  = new Point(e.X, e.Y); 			
					FeedTreeNodeBase t = (FeedTreeNodeBase)this.treeFeeds.GetNodeAt(treeFeeds.PointToClient(p));
				
					if (t == null)
						CurrentDragHighlightNode = null;

					if (t != null) {
						if (t.Type == FeedNodeType.Feed)
							CurrentDragHighlightNode = t.Parent;
						else
							CurrentDragHighlightNode = t;
					}
				}
			} catch (Exception ex) {
				_log.Error("Unexpected exception in OnTreeFeedMouseMove()", ex);
			}
		}

		private void OnTreeFeedMouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			try {
				CurrentDragHighlightNode = CurrentDragNode = null;
			
				if (e.Button == MouseButtons.Left) {
				
					TreeView tv = (TreeView)sender;
					FeedTreeNodeBase selectedNode = (FeedTreeNodeBase)tv.GetNodeAt (e.X ,e.Y ); 						

					if (selectedNode != null && TreeSelectedNode == selectedNode) {
						SetTitleText(selectedNode.Key);
						MoveFeedDetailsToFront();
					}
				}
			} catch (Exception ex) {
				_log.Error("Unexpected exception in OnTreeFeedMouseUp()", ex);
			}
		}

		private void EmptyListView(){
			//lock(listFeedItems){
			if (listFeedItems.Items.Count > 0) {
				listFeedItems.BeginUpdate(); 							
				listFeedItems.ListViewItemSorter = null; 
				listFeedItems.Items.Clear(); 
				listFeedItems.EndUpdate();
			}
			owner.Mediator.SetEnable("-cmdFeedItemPostReply");
			//}
		}

		private void OnTreeFeedDoubleClick(object sender, EventArgs e) {
			try {
				Point point = this.treeFeeds.PointToClient(Control.MousePosition);
				FeedTreeNodeBase node = (FeedTreeNodeBase)this.treeFeeds.GetNodeAt(point);

				if (node != null) {
					this.CurrentSelectedNode = node;
					owner.CmdNavigateFeedHome((AppMenuCommand) null);
					this.CurrentSelectedNode = null;
				}
			} catch (Exception ex) {
				_log.Error("Unexpected Error in OnTreeFeedDoubleClick()", ex);
			}
		}


		private void OnTreeFeedBeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e) {
			
			if(Object.ReferenceEquals(this.treeFeeds.SelectedNode, e.Node)){ return; }

			if(this.TreeSelectedNode != null){
				
				listFeedItems.CheckForLayoutModifications();
				FeedTreeNodeBase tn = TreeSelectedNode;

				if(tn.Type == FeedNodeType.Category ){
					string category = this.BuildCategoryStoreName(tn);

					if(owner.FeedHandler.GetCategoryMarkItemsReadOnExit(category)==true){
						this.MarkSelectedNodeRead(tn);
						owner.FeedlistModified = true;
					}
								
				}else if (tn.Type == FeedNodeType.Feed){					
					string feedUrl = (string)tn.Tag;
					feedsFeed f    = owner.FeedHandler.FeedsTable[feedUrl] as feedsFeed; 
					
					if((owner.FeedHandler.GetMarkItemsReadOnExit((string)tn.Tag)== true)
					    && f.containsNewMessages){
						this.MarkSelectedNodeRead(tn);
						owner.FeedlistModified = true;
						//this.UpdateTreeStatus(owner.FeedHandler.FeedsTable);					 
					}												
				}
			}//if(this.TreeSelectedNode != null){		
		}

		private void OnTreeFeedAfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {

			try{			

				FeedTreeNodeBase tn = (FeedTreeNodeBase)e.Node;
				
				if (e.Action != TreeViewAction.ByMouse) {	// mousedown handled separatly
					tn.UpdateContextMenu();
					this.RefreshTreeFeedContextMenus(tn);
				}

				if (tn.Type != FeedNodeType.Root) {
					SetTitleText(tn.Key);
					MoveFeedDetailsToFront();
				}
			
				if(tn.IsSelected) {
					listFeedItems.FeedColumnLayout = this.GetFeedColumnLayout(tn);	// raise events, that build the columns

					switch (tn.Type) {
						case FeedNodeType.Feed:	
							MoveFeedDetailsToFront();
							RefreshFeedDisplay(tn);
							break;
						
						case FeedNodeType.Category:			
							/* if (!_initialFeedLoadingInProgress) { MAKES UI UNRESPONSIVE
								RefreshCategoryDisplay(tn);
							} else { */
							string category = this.BuildCategoryStoreName(tn);
							FeedInfoList unreadItems = new FeedInfoList(category); 

							PopulateListView(tn, new ArrayList(), true);
							htmlDetail.Clear();
							WalkdownThenRefreshFeed(tn, false, true, tn, unreadItems);
								
							if((tn != null) && tn.IsSelected){	
								this.BeginTransformFeedList(unreadItems, tn, this.owner.FeedHandler.GetCategoryStyleSheet(category)); 
							}
							/* } */
							break;
					
						case FeedNodeType.SmartFolder:
							try { 				
								ISmartFolder isf = tn as ISmartFolder;
								if (isf != null)
									PopulateSmartFolder(tn,true);
							}
							catch(Exception ex) {
								_log.Error("Unexpected Error on PopulateSmartFolder()", ex);
								owner.MessageError("RES_ExceptionGeneral", ex.Message);
							}
							break;

						case FeedNodeType.Finder:
							try { 				
								PopulateFinderNode((FinderNode)tn,true);
							}
							catch(Exception ex) {
								_log.Error("Unexpected Error on PopulateAggregatedFolder()", ex);
								owner.MessageError("RES_ExceptionGeneral", ex.Message);
							}
							break;

						case FeedNodeType.Root:
							/* 
							if (this.GetRoot(RootFolderType.MyFeeds).Equals(tn)) 
								AggregateSubFeeds(tn);	// it is slow on startup, nothing is loaded in memory...
							*/
							this.SetGuiStateFeedback(Resource.Manager["RES_StatisticsAllFeedsCountMessage", owner.FeedHandler.FeedsTable.Count]);
							break;

						default:
							break;
					}
				}

			}catch(Exception ex) {
				_log.Error("Unexpected Error in OnTreeFeedAfterSelect()", ex);
				owner.MessageError("RES_ExceptionGeneral", ex.Message);
			}

		}

		private void OnTreeFeedBeforeLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e){
		
			FeedTreeNodeBase editedNode = (FeedTreeNodeBase)e.Node;
			
			if (editedNode.Editable){
				if (editedNode.UnreadCount > 0){// does not have the expected effect :-((
					e.Node.Text = editedNode.Key;	// want's to remove the (xxx) unread item counter from the label edit text.
				}
			}
			else
				e.CancelEdit = true;
		}



		internal void RenameTreeNode(FeedTreeNodeBase tn, string newName){

			tn.Key = newName; 
			this.OnTreeFeedAfterLabelEdit(this, new NodeLabelEditEventArgs(tn, newName));
		}

		private void OnTreeFeedAfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e){
		
			FeedTreeNodeBase editedNode = (FeedTreeNodeBase)e.Node;

			if (e.Label != null) {
				
				if(e.Label.Trim().Length > 0) {

					string newLabel = e.Label.Trim();
					if (editedNode.UnreadCount > 0){
						Match m = _labelParser.Match(newLabel);
						if (m.Success) 
							newLabel = m.Groups["caption"].Value.Trim();
					}
					if(editedNode.Type == FeedNodeType.Feed) { //feed node 

						feedsFeed f = owner.FeedHandler.FeedsTable[(string)editedNode.Tag];
						f.title     = newLabel; 									
						owner.FeedlistModified = true;

						e.CancelEdit = true;
						treeFeeds.BeginUpdate(); 
						editedNode.Key = f.title;
						treeFeeds.EndUpdate(); 				
					
					} else if (editedNode.Type == FeedNodeType.Finder) {

						e.CancelEdit = true;
						treeFeeds.BeginUpdate(); 
						editedNode.Key = newLabel;
						treeFeeds.EndUpdate(); 				

					} else 	{ //category node 

						FeedTreeNodeBase existingNode = FindChild(editedNode.Parent, newLabel, FeedNodeType.Category);
						if(existingNode != null && existingNode != editedNode ) {
							owner.MessageError("RES_ExceptionDuplicateCategoryName", newLabel);
							e.CancelEdit = true;
							return; 
						}

						string oldFullname = BuildCategoryStoreName(editedNode);

						// rename the item
						treeFeeds.BeginUpdate();
						editedNode.Key = newLabel;
						treeFeeds.EndUpdate();
						
						if (this.GetRoot(editedNode) == RootFolderType.MyFeeds) {
							string newFullname = BuildCategoryStoreName(editedNode);

							CategoriesCollection categories = owner.FeedHandler.Categories;
							string[] catList = new string[categories.Count];
							categories.Keys.CopyTo(catList, 0);
							// iterate on a copied list, so we can change the old one without
							// side effects
							foreach (string catKey in catList) {
								if (catKey.StartsWith(oldFullname)) {
									int i = categories.IndexOfKey(catKey);
									CategoryEntry c = categories[i];
									categories.RemoveAt(i); 
									c.Key = catKey.Replace(oldFullname,newFullname);
									c.Value.Value = c.Key;
									categories.Insert(i, c);
								}
							}

							// funny recursive part:
							// change category in feed manager 
							// (also updates tree node in UI)
							WalkdownThenRenameFeedCategory(editedNode, newFullname);

						} else if (this.GetRoot(editedNode) == RootFolderType.Finder) {
							// we are done
						}

						e.CancelEdit = true;

					}

				}
				else {
					/* Cancel the label edit action, inform the user, and 
						 place the node in edit mode again. */
					e.CancelEdit = true;
					SetGuiStateFeedback(Resource.Manager["RES_GUIStatusErrorEmptyTitleNotAllowed"]);
					e.Node.BeginEdit();
				}
				
			}//if (e.Label != null) 
			else {
				editedNode.Key = editedNode.Text;		// reset formattings
				e.CancelEdit = true;
			}
		
		}

		private void OnTreeFeedItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e) {
			// this is called sometimes after display the tree context menu, so restrict to left mouse button
			if (e.Button == MouseButtons.Left && 
				(((FeedTreeNodeBase)e.Item).Type == FeedNodeType.Feed || ((FeedTreeNodeBase)e.Item).Type == FeedNodeType.Category)){

				CurrentDragNode = (FeedTreeNodeBase)e.Item;
				
				if (CurrentDragNode.IsExpanded)
					CurrentDragNode.Collapse();
				
				string dragObject = null;

				if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
					IFeedDetails fd = null;
					if (CurrentDragNode.Type == FeedNodeType.Feed && 
						owner.FeedHandler.FeedsTable.ContainsKey((string)CurrentDragNode.Tag))
						fd =owner.FeedHandler.GetFeedInfo((string)CurrentDragNode.Tag);
					if (fd != null) {
						dragObject = fd.Link;
					}	
				}
				if (dragObject != null) {
					this.DoDragDrop(dragObject, DragDropEffects.Copy | DragDropEffects.Link);
				} else {
					if (CurrentDragNode.Type == FeedNodeType.Feed) {
						dragObject = (string)CurrentDragNode.Tag;
					} else {
						dragObject = CurrentDragNode.Text;
					}
					this.DoDragDrop(dragObject, DragDropEffects.Copy | DragDropEffects.Move);
				}
				CurrentDragHighlightNode = CurrentDragNode = null;
			}

		}

		private void OnTreeFeedDragEnter(object sender, System.Windows.Forms.DragEventArgs e)	{

			if (e.Data.GetDataPresent(DataFormats.Text)) {

				if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) {
					e.Effect = DragDropEffects.Link;	// we got this on drag urls from IE !
				} else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move) {
					e.Effect = DragDropEffects.Move;
				} else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) {
					e.Effect = DragDropEffects.Copy;
				} else {
					e.Effect = DragDropEffects.None;
					CurrentDragHighlightNode = null;
					return;
				}

				Point p  = new Point(e.X, e.Y); 			
				FeedTreeNodeBase t = (FeedTreeNodeBase)this.treeFeeds.GetNodeAt(treeFeeds.PointToClient(p));
				
				if (t == null){
					e.Effect = DragDropEffects.None;
					CurrentDragHighlightNode = null;
				}
				if (t != null) {
					if (t.Type == FeedNodeType.Feed)
						CurrentDragHighlightNode = t.Parent;
					else if (t.Type == FeedNodeType.Category || GetRoot(RootFolderType.MyFeeds).Equals(t))
						CurrentDragHighlightNode = t;
					else {
						e.Effect = DragDropEffects.None;
						CurrentDragHighlightNode = null;
					}
				}
			}
			else {
				e.Effect = DragDropEffects.None;
				CurrentDragHighlightNode = null;
			}
		}

		private void OnTreeFeedGiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e) {
			//if we are a drag source, ...
			_log.Debug("OnTreeFeedGiveFeedback() effect:"+e.Effect.ToString());
		}

		private void OnTreeFeedQueryContiueDrag(object sender, System.Windows.Forms.QueryContinueDragEventArgs e) {
			// keyboard or mouse button state changes
			// we listen to Unpress Ctrl:
			_log.Debug("OnTreeFeedQueryContiueDrag() action:"+e.Action.ToString()+", KeyState:"+e.KeyState.ToString());
		}

		private void OnTreeFeedDragOver(object sender, System.Windows.Forms.DragEventArgs e) {
			
			if (e.Data.GetDataPresent(DataFormats.Text)) {

				if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) {
					e.Effect = DragDropEffects.Link;
				} else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move) {
					e.Effect = DragDropEffects.Move;
				} else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) {
					e.Effect = DragDropEffects.Copy;
				} else {
					e.Effect = DragDropEffects.None;
					CurrentDragHighlightNode = null;
					return;
				}

				Point p  = treeFeeds.PointToClient(new Point(e.X, e.Y)); 			
				FeedTreeNodeBase t = (FeedTreeNodeBase)this.treeFeeds.GetNodeAt(p);
				
				if (t == null){
					e.Effect = DragDropEffects.None;
					CurrentDragHighlightNode = null;
				}

				if (t != null) {
					if (t.Type == FeedNodeType.Feed)
						CurrentDragHighlightNode = t.Parent;
					else if (t.Type == FeedNodeType.Category || GetRoot(RootFolderType.MyFeeds).Equals(t))
						CurrentDragHighlightNode = t;
					else {
						e.Effect = DragDropEffects.None;
						CurrentDragHighlightNode = null;
					}
				}

				int tcsh = this.treeFeeds.ClientSize.Height;
				int scrollThreshold = 25;
				if (p.Y + scrollThreshold > tcsh)
					Win32.SendMessage(this.treeFeeds.Handle, (int)Win32.Message.WM_VSCROLL, 1, 0);
				else if (p.Y < scrollThreshold)
					Win32.SendMessage(this.treeFeeds.Handle, (int)Win32.Message.WM_VSCROLL, 0, 0);

			}
			else {
				e.Effect = DragDropEffects.None;
				CurrentDragHighlightNode = null;
			}
		}

		private void OnTreeFeedDragDrop(object sender, System.Windows.Forms.DragEventArgs e) {

			CurrentDragHighlightNode = null;

			//get node where feed was dropped 
			Point p  = new Point(e.X, e.Y); 			
			FeedTreeNodeBase target = (FeedTreeNodeBase)this.treeFeeds.GetNodeAt(treeFeeds.PointToClient(p));

			//move node if dropped on a category node (or below)
			if (target != null) {
				FeedTreeNodeBase node2move = CurrentDragNode;

				if (target.Type == FeedNodeType.Feed) {
					// child of a category. Take the parent as target
					target = target.Parent;
				}

				
				if(node2move != null ) {

					MoveNode (node2move, target);

				} else {	// foreign drag/drop op
					
					// Bring the main window to the front so the user can
					// enter the dropped feed details.  Otherwise the feed
					// details window can pop up underneath the drop source,
					// which is confusing.
					RssBandit.Win32.SetForegroundWindow(this.Handle);

					string sData = (string)e.Data.GetData(DataFormats.Text);
					this.DelayTask(DelayedTasks.AutoSubscribeFeedUrl, new object[]{target, sData});

//					Uri urlUri = null;
//					try {
//						urlUri = new Uri(sData);
//						string category = BuildCategoryStoreName(target); 
//						owner.CmdNewFeed(category, sData, "[New Feed]");
//					}
//					catch(UriFormatException ex) {
//						Trace.Write ("TreeView: dropped an invalid Url: " + ex.Message);
//					}

				}
			}		

			CurrentDragNode = null;
		}

		private void OnTimerTreeNodeExpandElapsed(object sender, System.Timers.ElapsedEventArgs e) {
			_timerTreeNodeExpand.Stop();
			if (CurrentDragHighlightNode != null) {
				if (!CurrentDragHighlightNode.IsExpanded)
					CurrentDragHighlightNode.Expand();
			}
		}

		private void OnTimerFeedsRefreshElapsed(object sender, System.Timers.ElapsedEventArgs e) {
			if (owner.InternetAccessAllowed) {
				this.UpdateAllFeeds(false);
			}
		}

		private void OnTimerResetStatusTick(object sender, EventArgs e) {
			this._timerResetStatus.Stop();
			this.SetGuiStateFeedback(String.Empty);		
			if (_trayManager.CurrentState == ApplicationTrayState.BusyRefreshFeeds || 
				this.GetRoot(RootFolderType.MyFeeds).UnreadCount == 0) {
				_trayManager.SetState(ApplicationTrayState.NormalIdle);
			}
		}

		internal void OnFeedListItemActivate(object sender, System.EventArgs e) {

			try {
				if(listFeedItems.SelectedItems.Count == 0)
					return; 

				ThreadedListViewItem selectedItem = (ThreadedListViewItem) listFeedItems.SelectedItems[0]; 			
			
				// get the current item/feedNode
				NewsItem item = CurrentSelectedFeedItem  = (NewsItem) selectedItem.Key;
				FeedTreeNodeBase tn = TreeSelectedNode;
				string stylesheet = null;

				// refresh context menu items
				RefreshTreeFeedContextMenus( tn );

				if (item != null && tn != this._sentItemsNode &&
					item.CommentStyle != SupportedCommentStyle.None && 
					owner.InternetAccessAllowed)
					owner.Mediator.SetEnable("+cmdFeedItemPostReply");
				else
					owner.Mediator.SetEnable("-cmdFeedItemPostReply");

				SearchCriteriaCollection searchCriterias = null;
				FinderNode agNode = CurrentSelectedNode as FinderNode;
				if (agNode != null && agNode.Finder.DoHighlight)
					searchCriterias = agNode.Finder.SearchCriterias;

				if (item == null)	{	// can happen on dummy items ("Loading..."), if the user clicks fast enough
					htmlDetail.Clear();
					_tabStateUrl = String.Empty;
				} else if ((StringHelper.EmptyOrNull(item.Content) ||	item.Content.StartsWith("http")) &&
					!StringHelper.EmptyOrNull(item.Link)) {

					/* if (this.UrlRequestHandledExternally(item.Link, false)) {
						htmlDetail.Clear();
					} else */
					if (owner.Preferences.NewsItemOpenLinkInDetailWindow) {	
						htmlDetail.Navigate(item.Link);
					} else {	// not allowed: just display the Read On... 
						stylesheet = (item.Feed != null ? this.owner.FeedHandler.GetStyleSheet(item.Feed.link) : String.Empty); 					
						htmlDetail.Html = owner.FormatNewsItem(stylesheet, item, searchCriterias);
						htmlDetail.Navigate(null);
					}

					_tabStateUrl = item.Link;

				} else {

					stylesheet = (item.Feed != null ? this.owner.FeedHandler.GetStyleSheet(item.Feed.link) : String.Empty); 
					htmlDetail.Html = owner.FormatNewsItem(stylesheet, item, searchCriterias);
					htmlDetail.Navigate(null);
				
					_tabStateUrl = item.Link;
				}

				//indicate item has been read 
				if (item != null && !item.BeenRead) {

					ApplyStyles(selectedItem, true);
					SmartFolderNodeBase sfNode = CurrentSelectedNode as SmartFolderNodeBase;

					if (selectedItem.ImageIndex > 0) selectedItem.ImageIndex--;
					item.BeenRead = true; 			
					bool isTopLevelItem = (selectedItem.IndentLevel == 0); 
					int equalItemsRead = (isTopLevelItem ? 1 : 0);
					lock(listFeedItems.Items) {
						for (int j = 0; j < listFeedItems.Items.Count; j++) {	// if there is a self-reference thread, we also have to switch the Gui state for them
							ThreadedListViewItem th = listFeedItems.Items[j];
							NewsItem selfRef = th.Key as NewsItem;
							if (item.Equals(selfRef) && (th.ImageIndex % 2) != 0) {	// unread-state images always odd index numbers
								ApplyStyles(th, true);
								th.ImageIndex--;
								if (!selfRef.BeenRead) {	// object ref is unequal, but other criteria match the item to be equal...
									selfRef.BeenRead = true;							
								}
								if (th.IndentLevel == 0){
									isTopLevelItem = true; 
									equalItemsRead++;
								}
							}
						}
					}
				
					if (isTopLevelItem && tn.Type == FeedNodeType.Feed || tn.Type == FeedNodeType.SmartFolder || tn.Type == FeedNodeType.Finder) {
						tn.UpdateReadStatus(tn , -equalItemsRead);	
						//this.DelayTask(DelayedTasks.RefreshTreeStatus, new object[]{tn,-equalItemsRead});
					}

					FeedTreeNodeBase root = GetRoot(RootFolderType.MyFeeds);

					if (item.Feed.link == (string)tn.Tag) {
						// test for catch all on selected node
						item.Feed.containsNewMessages = (tn.UnreadCount != 0);
					} else {// other (categorie selected, aggregated or an threaded item from another feed)
						
						if (agNode != null) agNode.UpdateReadStatus();
						if (sfNode != null) sfNode.UpdateReadStatus();

						// lookup corresponding TreeNode:
						FeedTreeNodeBase refNode = GetTreeNodeForItem(root, item.Feed);
						if (refNode != null) {
							//refNode.UpdateReadStatus(refNode , -1);
							this.DelayTask(DelayedTasks.RefreshTreeStatus, new object[]{refNode, -1});
							item.Feed.containsNewMessages = (refNode.UnreadCount != 0);
						} else { // temp feed item, e.g. from commentRss
							string hash = RssHelper.GetHashCode(item);
							if (!tempFeedItemsRead.ContainsKey(hash))
								tempFeedItemsRead.Add(hash, null /* item ???*/);
						}
					}

					owner.FeedlistModified = true;
				}

				// refresh Tab state
				RefreshDocumentState(_docContainer.ActiveDocument);
			
			} catch (Exception ex) {
				_log.Error("OnFeedListItemActivate() failed.", ex);
			}
		}

		
		private void OnFeedListExpandThread(object sender, ThreadEventArgs e) {
			
			try {

				NewsItem currentNewsItem = (NewsItem)e.Item.Key;
				IList itemKeyPath = e.Item.KeyPath;
				// column index map
				ColumnKeyIndexMap colIndex = this.listFeedItems.Columns.GetColumnIndexMap();

				ICollection outGoingItems = owner.FeedHandler.GetItemsFromOutGoingLinks(currentNewsItem, itemKeyPath);
				ICollection inComingItems = owner.FeedHandler.GetItemsWithIncomingLinks(currentNewsItem, itemKeyPath);

				ArrayList childs = new ArrayList(outGoingItems.Count + inComingItems.Count + 1);
				ThreadedListViewItem newListItem;

				try {
					
					foreach (NewsItem o in outGoingItems) {
			
						bool hasRelations = this.NewsItemHasRelations(o, itemKeyPath);						
						newListItem = this.CreateThreadedLVItem(o, hasRelations, 2, colIndex, false);

						//does it match any filter? 
						_filterManager.Apply(newListItem);

						childs.Add(newListItem);
					}

				} catch (Exception e1) {
					_log.Error("OnFeedListExpandThread exception (iterate outgoing)", e1);
				}

				try {
					foreach (NewsItem o in inComingItems) {
							
						bool hasRelations = this.NewsItemHasRelations(o, itemKeyPath);						
						newListItem = this.CreateThreadedLVItem(o, hasRelations, 4, colIndex , false);
				
						//does it match any filter? 
						_filterManager.Apply(newListItem);

						childs.Add(newListItem);

					}//iterator.MoveNext
				}
				catch (Exception e2) {
					_log.Error("OnFeedListExpandThread exception (iterate incoming)", e2);
				}				

				if (currentNewsItem.HasExternalRelations) {
					// includes also commentRss support

					if (currentNewsItem.GetExternalRelations() == RelationCosmos.EmptyRelationList ||
						currentNewsItem.CommentCount != currentNewsItem.GetExternalRelations().Count) {
						
						if (owner.InternetAccessAllowed) {
							ThreadedListViewItemPlaceHolder insertionPoint = (ThreadedListViewItemPlaceHolder)this.CreateThreadedLVItemInfo(Resource.Manager["RES_GUIStatusLoadingChildItems"], false);
							childs.Add(insertionPoint);
							this.BeginLoadCommentFeed(currentNewsItem, insertionPoint.InsertionPointTicket, itemKeyPath);
						} else {
							newListItem = (ThreadedListViewItemPlaceHolder)this.CreateThreadedLVItemInfo(Resource.Manager["RES_GUIStatusChildItemsNA"], false);
							childs.Add(newListItem);
						}

					} else {	// just take the existing collection

						// they are sorted as we requested them, so we do not sort again here
						ArrayList commentItems = new ArrayList(currentNewsItem.GetExternalRelations());
						//commentItems.Sort(RssHelper.GetComparer(false, NewsItemSortField.Date));

						foreach (NewsItem o in commentItems) {

							bool hasRelations = this.NewsItemHasRelations(o, itemKeyPath);

							o.BeenRead = tempFeedItemsRead.ContainsKey(RssHelper.GetHashCode(o));
							newListItem = this.CreateThreadedLVItem(o, hasRelations, 8, colIndex , true);
							_filterManager.Apply(newListItem);
							childs.Add(newListItem);

						}//iterator.MoveNext

					}
				}

				e.ChildItems = new ThreadedListViewItem[childs.Count];
				childs.CopyTo(e.ChildItems);
			
			} catch (Exception ex) {
				_log.Error("OnFeedListExpandThread exception", ex);
			}
				
		}

		private void OnFeedListLayoutChanged(object sender, ListLayoutEventArgs e) {
			// build columns, etc. pp
			if (e.Layout.Columns.Count > 0) {
				this.EmptyListView();
				lock(listFeedItems.Columns) {
					listFeedItems.Columns.Clear();
					int i = 0;
					IList colW = e.Layout.ColumnWidths;
					foreach (string colID in e.Layout.Columns) {
						AddListviewColumn(colID, (int)colW[i++]);
					}
				}
			}
			RefreshListviewColumnContextMenu();
		}

		private void OnFeedListLayoutModified(object sender, ListLayoutEventArgs e) {
			if (this.TreeSelectedNode != null)
				this.SetFeedHandlerFeedColumnLayout(TreeSelectedNode, e.Layout);
		}

		private void OnFeedListItemsColumnClick(object sender, ColumnClickEventArgs e) {
			
			if (listFeedItems.Items.Count == 0)
				return;

			if (listFeedItems.SelectedItems.Count > 0)
				return;

			FeedTreeNodeBase node = CurrentSelectedNode;
			if (node == null)
				return;

			bool unreadOnly = true;
			if (node.Type == FeedNodeType.Finder)
				unreadOnly = false;

			ArrayList items = this.NewsItemListFrom(listFeedItems.Items, unreadOnly);
			if (items == null || items.Count <= 1)	// no need to re-sort on no or just one item
				return;

			
			Hashtable temp = new Hashtable();

			foreach (NewsItem item in items) {
				FeedInfo fi = null;
				if (temp.ContainsKey(item.Feed.link)) {
					fi = (FeedInfo)temp[item.Feed.link];
				} else {
					fi = (FeedInfo)item.FeedDetails.Clone();
					fi.ItemsList.Clear();
					temp.Add(item.Feed.link, fi);
				}
				fi.ItemsList.Add(item);
			}

			string category = this.BuildCategoryStoreName(CurrentSelectedNode);
			FeedInfoList redispItems = new FeedInfoList(category); 

			foreach (FeedInfo fi in temp.Values) {
				if (fi.ItemsList.Count > 0)
					redispItems.Add(fi);
			}
			
			this.BeginTransformFeedList(redispItems, CurrentSelectedNode, this.owner.FeedHandler.GetCategoryStyleSheet(category)); 

		}

		private void OnFeedListMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			try {
				ListView lv = (ListView)sender;
				ThreadedListViewItem lvi = null; 

				try {
					lvi = (ThreadedListViewItem)lv.GetItemAt(e.X, e.Y); 
				} catch {}

				if (e.Button == MouseButtons.Right) {
				
					RefreshListviewContextMenu();
					if (lv.Items.Count > 0) {
					
						if (lvi != null && !lvi.Selected) {
							lv.SelectedItems.Clear();
							lvi.Selected = true;
							lvi.Focused = true;
							this.OnFeedListItemActivate(sender, EventArgs.Empty);
						}

					}
			
				} else {	// !MouseButtons.Right

					if (lv.Items.Count <= 0)
						return;

					if (lvi != null && e.Clicks > 1) {	//DblClick

						NewsItem item = CurrentSelectedFeedItem  = (NewsItem) lvi.Key;

						lv.SelectedItems.Clear();
						lvi.Selected = true;
						lvi.Focused = true;

						if (item != null && !StringHelper.EmptyOrNull(item.Link)) {
							if (!this.UrlRequestHandledExternally(item.Link, false))
								DetailTabNavigateToUrl(item.Link, null, false);
						}

					}
				}	//! MouseButtons.Right
			
			} catch (Exception ex) {
				_log.Error("OnFeedListMouseDown() failed", ex);
			}
		}

		private void OnStatusPanelClick(object sender, StatusBarPanelClickEventArgs e) {
			if (e.Clicks > 1 && e.StatusBarPanel == this.statusBarConnectionState) {
				// DblClick to the connection state panel image
				owner.UpdateInternetConnectionState(true);	// force a connection check
			}
		}

		private void OnStatusPanelLocationChanged(object sender, EventArgs e) {
			progressBrowser.SetBounds(_status.Width - 
				(this.statusBarRssParser.Width + this.statusBarConnectionState.Width + BrowserProgressBarWidth +10),
				_status.Location.Y+6, 0, 0, BoundsSpecified.Location);
		}

		private void RePopulateListviewWithCurrentContent() {
			this.RePopulateListviewWithContent(this.NewsItemListFrom(listFeedItems.Items));
		}
		
		private void RePopulateListviewWithContent(ArrayList newsItemList) {
			if (newsItemList == null)
				newsItemList = new ArrayList(0);

			ThreadedListViewItem lvLastSelected = null;
			if (listFeedItems.SelectedItems.Count > 0)
				lvLastSelected = (ThreadedListViewItem)listFeedItems.SelectedItems[0];

			bool categorizedView = (CurrentSelectedNode.Type == FeedNodeType.Category) || (CurrentSelectedNode.Type == FeedNodeType.Finder); 
			PopulateListView(CurrentSelectedNode, newsItemList, true, categorizedView , CurrentSelectedNode);

			// reselect the last selected
			if (lvLastSelected != null && lvLastSelected.IndentLevel == 0) {
				ReSelectListViewItem ((NewsItem)lvLastSelected.Key);
			}
		}

		private void ReSelectListViewItem(NewsItem item) {
			
			if (item == null) return;

			string selItemId = item.Id;
			if (selItemId != null) {
				for (int i = 0;  i < listFeedItems.Items.Count; i++) {
					ThreadedListViewItem theItem = listFeedItems.Items[i];
					string thisItemId = ((NewsItem)theItem.Key).Id;
					if (selItemId.CompareTo(thisItemId) == 0) {
						listFeedItems.Items[i].Selected = true;
						listFeedItems.EnsureVisible(listFeedItems.Items[i].Index);
						break;
					}
				}
			}
		}

		private ArrayList NewsItemListFrom(ThreadedListViewItemCollection list) {
			return NewsItemListFrom(list, false);
		}
		
		private ArrayList NewsItemListFrom(ThreadedListViewItemCollection list, bool unreadOnly) {
			ArrayList items = new ArrayList(list.Count);
			for (int i=0; i < list.Count; i++) {
				ThreadedListViewItem tlvi = list[i];
				if (tlvi.IndentLevel == 0) {
					NewsItem item = (NewsItem)tlvi.Key;
					
					if (unreadOnly && item != null && item.BeenRead)
						item= null;
					
					if (item != null)
						items.Add(item);
				}
			}
			return items;
		}

		private void OnFeedListItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e) {
			
			if (e.Button == MouseButtons.Left) {
				ThreadedListViewItem item = (ThreadedListViewItem)e.Item;
				NewsItem r = (NewsItem)item.Key;
				if (r.Link != null) {
					this.treeFeeds.AllowDrop = false;	// do not drag to tree
					this.DoDragDrop(r.Link, DragDropEffects.All | DragDropEffects.Link);
					this.treeFeeds.AllowDrop = true;
				}
			}
		
		}

		/// <summary>
		/// support the keydown/pagedown keyup/pageup listview navigation 
		/// as well as deleting items via the Delete key.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFeedListItemKeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
			try {
				if (e.KeyCode == Keys.Down || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.End) {
					if (listFeedItems.SelectedItems.Count == 1)
						if (listFeedItems.SelectedItems[0].Index <= listFeedItems.Items.Count)
							this.OnFeedListItemActivate(sender, EventArgs.Empty);
				} 
				else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Home) {
					if (listFeedItems.SelectedItems.Count == 1)
						if (listFeedItems.SelectedItems[0].Index >= 0)
							this.OnFeedListItemActivate(sender, EventArgs.Empty);
				} 
				else if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) == Keys.Control) { 
					// select all
					if (listFeedItems.Items.Count > 0 && listFeedItems.Items.Count != listFeedItems.SelectedItems.Count) {
						try {
							listFeedItems.BeginUpdate();
							lock(listFeedItems.Items) {
								for (int i=0; i<listFeedItems.Items.Count;i++) {
									listFeedItems.Items[i].Selected = true;
								}
							}
						} finally {
							listFeedItems.EndUpdate();
						}
					}
				} 
				else if (e.KeyCode == Keys.Delete) {
					this.RemoveSelectedFeedItems();
				}
			} catch (Exception ex) {
				_log.Error("OnFeedListItemKeyUp() failed", ex);
			}
		}


		private void OnTrayIconDoubleClick(object sender, EventArgs e) {
			owner.CmdShowMainGui(null);
			//user is interested about the message this time
			_beSilentOnBalloonPopupCounter = 0;	// reset balloon silent counter
		}

		//called, if the user explicitly closed the balloon
		private void OnTrayAniBalloonTimeoutClose(object sender, EventArgs e) {
			//user isn't interested about the message this time
			_beSilentOnBalloonPopupCounter = 12;		// 12 * 5 minutes (refresh timer) == 1 hour (minimum)
		}

		
		#region toolbar combo's events

		private void OnNavigateComboBoxKeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Return && Control.ModifierKeys == Keys.None) {
				this.DetailTabNavigateToUrl(UrlText, null, e.Control);
			}
		}

		private void OnNavigateComboBoxDragOver(object sender, System.Windows.Forms.DragEventArgs e) {
			
			if (e.Data.GetDataPresent(DataFormats.Text)) {

				if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) {
					e.Effect = DragDropEffects.Link;
				} else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move) {
					e.Effect = DragDropEffects.Move;
				} else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) {
					e.Effect = DragDropEffects.Copy;
				} else {
					e.Effect = DragDropEffects.None;
				}

			}
			else {
				e.Effect = DragDropEffects.None;
			}
		}

		private void OnNavigateComboBoxDragDrop(object sender, System.Windows.Forms.DragEventArgs e) {

			if (e.Data.GetDataPresent(DataFormats.Text)) {
				string sData = (string)e.Data.GetData(typeof(string));
				try {	// accept uri only
					Uri uri = new Uri(sData);
					this.UrlText = uri.ToString();
				} catch { /* ignore invalid Uri's */ }
			}		
		}


		private void OnSearchComboBoxKeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Return) {
				e.Handled=true;
				this.StartSearch(null);
			}
		}
		private void OnSearchComboBoxDragOver(object sender, System.Windows.Forms.DragEventArgs e) {
			
			if (e.Data.GetDataPresent(DataFormats.Text)) {

				if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) {
					e.Effect = DragDropEffects.Link;
				} else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move) {
					e.Effect = DragDropEffects.Move;
				} else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) {
					e.Effect = DragDropEffects.Copy;
				} else {
					e.Effect = DragDropEffects.None;
				}

			}
			else {
				e.Effect = DragDropEffects.None;
			}
		}

		private void OnSearchComboBoxDragDrop(object sender, System.Windows.Forms.DragEventArgs e) {

			if (e.Data.GetDataPresent(DataFormats.Text)) {
				string sData = (string)e.Data.GetData(typeof(string));
				WebSearchText = sData;
			}		
		}

		#endregion

		#region html control events
		
		private void OnWebStatusTextChanged(object sender, BrowserStatusTextChangeEvent e) {
			SetBrowserStatusBarText(e.text);
		}

		private void OnWebBeforeNavigate(object sender, BrowserBeforeNavigate2Event e) {
			
			bool userNavigates = _webUserNavigated;
			bool forceNewTab = _webForceNewTab;

			string url = e.url;

			if (!url.ToLower().StartsWith("javascript:")) {
				_webForceNewTab = _webUserNavigated = false;	// reset, but keep it for the OnWebBeforeNewWindow event
			}

			if (!url.Equals("about:blank")) {

				if ( owner.InterceptUrlNavigation(url) ) {
					e.Cancel = true;
					return;
				}
				
				if (url.StartsWith("mailto:") || url.StartsWith("news:")) {//TODO: if nntp is impl., InterceptUrlNavigation() should handle "news:"
					return;
				}

				bool forceSetFocus = true; // if false, Tab opens in background; but IEControl does NOT display/render!!!   !(Interop.GetAsyncKeyState(Interop.VK_MENU) < 0);
				bool tabCanClose = true;

				HtmlControl hc = sender as HtmlControl;
				if (hc != null) {
					DockControl dc = (DockControl)hc.Tag;
					ITabState ts = (ITabState)dc.Tag;
					tabCanClose = ts.CanClose;
				}
				
				if (userNavigates && this.UrlRequestHandledExternally(url, forceNewTab)) {
					e.Cancel = true;
					return;
				}

				if (!tabCanClose && !userNavigates && !forceNewTab) {
					e.Cancel =  !e.IsRootPage;		// prevent sub-sequent requests of <iframe>'s
															// else just allow navigate in current browser
					return;
				}

				if ( (!tabCanClose && userNavigates) || forceNewTab) {	
					e.Cancel = true;
					// Delay gives time to the sender control to cancel request
					this.DelayTask(DelayedTasks.NavigateToWebUrl, new object[]{url, null, forceNewTab, forceSetFocus});
				}
			}
		}

		private void OnWebNavigateComplete(object sender, BrowserNavigateComplete2Event e) {
			// if we cancelled subsequent requests in the WebBeforeNavigate event,
			// we may not receive the OnWebDocumentComplete event for the master page
			// so in general we do the same things here as in OnWebDocumentComplete()
			try {
				if (!StringHelper.EmptyOrNull(e.url) && e.url != "about:blank" && e.IsRootPage) {

					AddUrlToHistory (e.url);

					HtmlControl hc = (HtmlControl)sender;
					DockControl doc = (DockControl)hc.Tag;
					ITabState state = (ITabState)doc.Tag;
					state.Url = e.url;
					RefreshDocumentState(doc);
					// state.Title may contain the old caption here, so we do not provide the page title:
					owner.BackgroundDiscoverFeedsHandler.DiscoverFeedInContent(hc.DocumentInnerHTML, state.Url, null);
					// do some more things here, because we may also not receive the events...
					this.DelayTask(DelayedTasks.ClearBrowserStatusInfo, null, 2000);
				}
			} catch (Exception ex) {
				_log.Error("OnWebNavigateComplete(): "+e.url, ex);
			}
		}

		private void OnWebDocumentComplete(object sender, BrowserDocumentCompleteEvent e) {
			
			try {

				if (!StringHelper.EmptyOrNull(e.url) && e.url != "about:blank" && e.IsRootPage) {

					AddUrlToHistory (e.url);

					HtmlControl hc = (HtmlControl)sender;
					DockControl doc = (DockControl)hc.Tag;
					ITabState state = (ITabState)doc.Tag;
					state.Url = e.url;
					RefreshDocumentState(doc);
					owner.BackgroundDiscoverFeedsHandler.DiscoverFeedInContent(hc.DocumentInnerHTML, state.Url, state.Title);
				}

			} catch (Exception ex) {
				_log.Error("OnWebDocumentComplete(): "+e.url, ex);
			}
		}

		private void OnWebTitleChanged(object sender, BrowserTitleChangeEvent e) {

			try {
				HtmlControl hc = (HtmlControl)sender;

				DockControl doc = (DockControl)hc.Tag;
				ITabState state = (ITabState)doc.Tag;
				state.Title = e.text;
				RefreshDocumentState(doc);
			} catch (Exception ex) {
				_log.Error("OnWebTitleChanged()", ex);
			}

		}

		private void OnWebCommandStateChanged(object sender, BrowserCommandStateChangeEvent e) {

			try {
				HtmlControl hc = (HtmlControl)sender;

				DockControl doc = (DockControl)hc.Tag;
				ITabState state = (ITabState)doc.Tag;

				if (e.command == CommandStateChangeConstants.CSC_NAVIGATEBACK)
					state.CanGoBack = e.enable;
				else if (e.command == CommandStateChangeConstants.CSC_NAVIGATEFORWARD)
					state.CanGoForward = e.enable;
				else if (e.command == CommandStateChangeConstants.CSC_UPDATECOMMANDS) {
					// 
				}
			} catch (Exception ex) {
				_log.Error("OnWebCommandStateChanged() ", ex);
			}
	
		}

		private void OnWebNewWindow(object sender, BrowserNewWindowEvent e) {

			try {
				bool userNavigates = _webUserNavigated;
				bool forceNewTab = _webForceNewTab;

				_webForceNewTab = _webUserNavigated = false;	// reset

				e.Cancel = true;

				string url = e.url;
				_log.Debug("OnWebNewWindow(): '"+url+"'");

				bool forceSetFocus =  true; // Tab in background, but IEControl does NOT display/render!!!    !(Interop.GetAsyncKeyState(Interop.VK_MENU) < 0);
			
				if (this.UrlRequestHandledExternally(url, forceNewTab)) {
					return;
				}

				if (userNavigates) {
					// Delay gives time to the sender control to cancel request
					this.DelayTask(DelayedTasks.NavigateToWebUrl, new object[]{url, null, true, forceSetFocus});
				}
			} catch (Exception ex) {
				_log.Error("OnWebNewWindow(): "+e.url, ex);
			}

		}

		private void OnWebQuit(object sender, EventArgs e) {
			try {
				// javscript want to close this window: so we have to close the tab
				this.RemoveDocTab(_docContainer.ActiveDocument);
			} catch (Exception ex) {
				_log.Error("OnWebQuit()", ex);
			}
		}

		private void OnWebTranslateAccelerator(object sender, KeyEventArgs e) {
			try {
				
				if (_shortcutHandler.IsCommandInvoked("BrowserCreateNewTab", e.KeyData)) 
				{	// capture Ctrl-N event or whichever combination is configured (new window)
					owner.CmdBrowserCreateNewTab(null);
					e.Handled = true;
				}
				if (_shortcutHandler.IsCommandInvoked("Help", e.KeyData)) 
				{	// capture F1 (or whichever keys are configured) event (help)
					Help.ShowHelp(this, this.helpProvider1.HelpNamespace, HelpNavigator.TableOfContents);
					e.Handled = true;
				}

				if (!e.Handled) {	// prevent double handling of shortcuts:
					// IE will handle this codes by itself even if a user configures other shortcuts
					// than Ctrl-N and F1.
					e.Handled = (e.KeyCode == Keys.N && e.Control ||
									  e.KeyCode == Keys.F1);
				}
				
				bool shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
				if (!e.Handled) {	// prevent double handling of shortcuts:
					if (e.KeyCode == Keys.Tab && !shift) {
						if (this.htmlDetail.Document2 != null && null == this.htmlDetail.Document2.GetActiveElement()) {
							// one turn around within ALink element classes
							if (this.treeFeeds.Visible) {
								this.treeFeeds.Focus();
								e.Handled = true;
							} else if (this.listFeedItems.Visible) {
								this.listFeedItems.Focus();
								e.Handled = true;
							}
						}
					} else if (e.KeyCode == Keys.Tab && shift) {
						if (this.htmlDetail.Document2 != null && null == this.htmlDetail.Document2.GetActiveElement()) {
							// one reverse turn around within ALink element classes
							if (this.listFeedItems.Visible) {
								this.listFeedItems.Focus();
								e.Handled = true;
							} else if (this.treeFeeds.Visible) {
								this.treeFeeds.Focus();
								e.Handled = true;
							}
						}
					}
				}

			} catch (Exception ex) {
				_log.Error("OnWebTranslateAccelerator(): "+e.KeyCode.ToString(), ex);
			}
		}

		private void OnWebProgressChanged(object sender, BrowserProgressChangeEvent e) {
			try {
				if (_lastBrowserThatProgressChanged == null)
					_lastBrowserThatProgressChanged = sender;

				if (sender != _lastBrowserThatProgressChanged) {
					DeactivateWebProgressInfo();
					return;
				}

				if (((e.progress < 0) || (e.progressMax <= 0)) || (e.progress >= e.progressMax)) {
					DeactivateWebProgressInfo();
				}
				else {
					if (!this.progressBrowser.Visible) this.progressBrowser.Visible = true;
					if (this.statusBarBrowserProgress.Width < BrowserProgressBarWidth) this.statusBarBrowserProgress.Width = BrowserProgressBarWidth;
					this.progressBrowser.Minimum = 0;
					this.progressBrowser.Maximum = e.progressMax;
					this.progressBrowser.Value = e.progress;
				}
			} catch (Exception ex) {
				_log.Error("OnWebProgressChanged()", ex);
			}

		}

		private object _lastBrowserThatProgressChanged = null;

		private void DeactivateWebProgressInfo() {
			this.progressBrowser.Minimum = 0;
			this.progressBrowser.Maximum = 128;
			this.progressBrowser.Value = 128;
			this.progressBrowser.Visible = false;
			this.statusBarBrowserProgress.Width = 0;
			_lastBrowserThatProgressChanged = null;
		}

		#endregion
	
		#endregion


		#region Implementation of ITabState
		public bool CanClose {
			get { return false; }
			set {}
		}

		public bool CanGoBack {
			get { 
				return (listFeedItems.Items.Count > 0 && 
					listFeedItems.SelectedItems.Count > 0 && 
					listFeedItems.SelectedItems[0].Index > 0);  
			}
			set { }
		}

		public bool CanGoForward {
			get { 
				return (listFeedItems.Items.Count > 0 && 
					listFeedItems.SelectedItems.Count > 0 && 
					listFeedItems.SelectedItems[0].Index < (listFeedItems.Items.Count - 1));  
			}
			set { }
		}

		public string Title {
			get {
				if (CurrentSelectedNode != null)
					return CurrentSelectedNode.Key;
				else
					return String.Empty;
			}
			set {
				// nothing to implement here
			}
		}

		public string Url {
			get { return _tabStateUrl; }
			set { _tabStateUrl = value;}
		}

		#endregion

		private void OnAnyToolBarButtonClick(object sender, ToolBarItemEventArgs e) {
			ICommand cmd = e.Item as ICommand;
			if (cmd != null)
				cmd.Execute();
		}

		private void OnAnyToolbarStateChanged(object sender, EventArgs e) {
			owner.GuiSettings.SetProperty(Name+"/sandBar.toolBarMain.Visible", toolBarMain.Visible);
			owner.GuiSettings.SetProperty(Name+"/sandBar.toolBarBrowser.Visible", toolBarBrowser.Visible);
			owner.GuiSettings.SetProperty(Name+"/sandBar.toolBarWebSearch.Visible", toolBarWebSearch.Visible);
			//this.DelayTask(DelayedTasks.SaveUIConfiguration);
		}

		//does nothing more than supress the beep if you press enter
		private void OnAnyEnterKeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == '\r'  && Control.ModifierKeys == Keys.None)
				e.Handled = true;	// supress beep
		}

		private void OnRssSearchExpressionChanged(object sender, System.EventArgs e) {
			RefreshRssSearchButtonStates();
		}

		private void OnRssSearchFinderCaptionChanged(object sender, System.EventArgs e) {
			RefreshRssSearchButtonStates();
		}

		private void OnRssSearchTypeCheckedChanged(object sender, System.EventArgs e) {
			if (sender == this.radioRssSearchSimpleText) {
				this.labelRssSearchTypeHint.Text = Resource.Manager["RES_RssSearchTypeTextHintCaption"];
			} else if (sender == this.radioRssSearchRegEx) {
				this.labelRssSearchTypeHint.Text = Resource.Manager["RES_RssSearchTypeRegExHintCaption"];
			} else if (sender == this.radioRssSearchExprXPath) {
				this.labelRssSearchTypeHint.Text = Resource.Manager["RES_RssSearchTypeXPathHintCaption"];
			}
		}

		private void OnRssSearchConsiderItemReadStateCheckedChanged(object sender, EventArgs e) {
			this.checkBoxRssSearchUnreadItems.Enabled = this.checkBoxConsiderItemReadState.Checked;
			RefreshRssSearchButtonStates();
		}

		private void OnRssSearchConsiderItemAgeCheckedChanged(object sender, EventArgs e) {
			this.radioRssSearchItemsOlderThan.Enabled = 
				this.radioRssSearchItemsYoungerThan.Enabled = 
				this.comboRssSearchItemAge.Enabled = this.checkBoxRssSearchTimeSpan.Checked;
			if (this.radioRssSearchItemsOlderThan.Enabled) {
				this.checkBoxRssSearchByDate.Checked = false;
				this.checkBoxRssSearchByDateRange.Checked = false;
			}
			RefreshRssSearchButtonStates();
		}
		private void OnRssSearchConsiderItemPostDateCheckedChanged(object sender, EventArgs e) {
			this.comboBoxRssSearchItemPostedOperator.Enabled =
				this.dateTimeRssSearchItemPost.Enabled = this.checkBoxRssSearchByDate.Checked;
			if (this.comboBoxRssSearchItemPostedOperator.Enabled) {
				this.checkBoxRssSearchTimeSpan.Checked = false;
				this.checkBoxRssSearchByDateRange.Checked = false;
			}
			RefreshRssSearchButtonStates();
		}
		private void OnRssSearchConsiderItemPostDateRangeCheckedChanged(object sender, EventArgs e) {
			this.dateTimeRssSearchPostAfter.Enabled =
				this.dateTimeRssSearchPostBefore.Enabled =  this.checkBoxRssSearchByDateRange.Checked;
			if (this.dateTimeRssSearchPostAfter.Enabled) {
				this.checkBoxRssSearchByDate.Checked = false;
				this.checkBoxRssSearchTimeSpan.Checked = false;
			}
			RefreshRssSearchButtonStates();
		}

		private void OnRssSearchScopeTreeAfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e) {
			if (e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse) {
				TreeHelper.PerformOnCheckStateChanged(e.Node);
			}
		}

		private void OnTextSearchExpressionKeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Return && (this.textSearchExpression.Text.Length > 0)) {
				this.OnRssSearchButtonClick(sender, null);
			}
		}

		private void OnRssSearchPanelResize(object sender, EventArgs e) {
			this.textSearchExpression.SetBounds(0,0,this.btnSearchCancel.Left - this.textSearchExpression.Left - 5 , 0, BoundsSpecified.Width);
			this.textFinderCaption.SetBounds(0,0,this.btnRssSearchSave.Left - this.textFinderCaption.Left - 5 , 0, BoundsSpecified.Width);
			this.btnRssSearchSave.Invalidate();	//BUGBUG: workaround for StyleXP themes, that may render (glass) button badly
		}

		private void OnRssSearchButtonClick(object sender, System.EventArgs e) {
			
			switch (_rssSearchState) {

				case RssSearchState.Pending:
				
					_rssSearchState = RssSearchState.Searching;
					this.btnSearchCancel.Text = Resource.Manager["RES_RssSearchDialogButtonCancelCaption"];
					
					this.btnNewSearch.Enabled = this.btnRssSearchSave.Enabled = false;
					this.textSearchExpression.Enabled = this.textFinderCaption.Enabled = false;
					this.taskPaneSearchOptions.Enabled = false;
					this.panelRssSearchCommands.SetBounds(0,0,0,80, BoundsSpecified.Height);
					this.SetSearchStatusText(Resource.Manager["RES_RssSearchStateMessage"]);

					FinderNode resultContainer = null;

					string newName = textFinderCaption.Text.Trim();
					
					if (newName.Length > 0) {	
						
						FeedTreeNodeBase parent = this.GetRoot(RootFolderType.Finder);
						
						if (newName.IndexOf("\\")>0) {
							string[] a = newName.Split(new char[]{'\\'});
							parent = this.CreateCategoryHive(parent, String.Join("\\", a, 0, a.GetLength(0)-1), true);
							newName = a[a.GetLength(0)-1].Trim();
						}

						FeedTreeNodeBase node= this.FindChild(parent, newName, FeedNodeType.Finder);
						if (node != null) {
							resultContainer = (FinderNode)node;
						} else {
							resultContainer = new FinderNode(newName, 10, 10, _treeSearchFolderContextMenu);
							resultContainer.Tag = resultContainer.InternalFeedLink;
							parent.Nodes.Add(resultContainer);
						}

					} else { 
						this.AddTempResultNode();
						resultContainer = _searchResultNode;
					}

					//Test scope:
//					ArrayList cs = new ArrayList(1);
//					cs.Add("Weblogs");
//					ArrayList fs = new ArrayList(1);
//					fs.Add("http://www.rendelmann.info/blog/GetRss.asmx");
//					fs.Add("http://www.25hoursaday.com/webblog/GetRss.asmx"); // activate for scope tests:
//					resultContainer.Finder = new RssFinder(resultContainer, this.SearchDialogGetSearchCriterias(),
//						 cs, fs, new RssFinder.SearchScopeResolveCallback(this.ScopeResolve) , true);
					
					ArrayList catScope = null;
					ArrayList feedScope = null;
					
					// if we have scope nodes, and not all selected:
					if (this.treeRssSearchScope.Nodes.Count > 0 && !this.treeRssSearchScope.Nodes[0].Checked) {
						ArrayList cs = new ArrayList(), fs = new ArrayList();
						TreeHelper.GetCheckedNodes(this.treeRssSearchScope.Nodes[0], cs, fs);
						
						if (cs.Count > 0)	catScope = new ArrayList(cs.Count);
						if (fs.Count > 0)	feedScope = new ArrayList(fs.Count);

						foreach (TreeNode n in cs) {
							catScope.Add(TreeHelper.BuildCategoryStoreName(n, this.treeRssSearchScope.PathSeparator.ToCharArray()));
						}
						foreach (TreeNode n in fs) {
							feedScope.Add((string)n.Tag);
						}
					}
					
					if(resultContainer.Finder == null){ //new search folder
					
						resultContainer.Finder = new RssFinder(resultContainer, this.SearchDialogGetSearchCriterias(),
							catScope, feedScope, new RssFinder.SearchScopeResolveCallback(this.ScopeResolve) , true);					
					
						// not a temp result and not yet exist:
						if (resultContainer != _searchResultNode && !owner.FinderList.Contains(resultContainer.Finder)) {
							owner.FinderList.Add(resultContainer.Finder);
						}
					
					}else{ //existing search folder
						resultContainer.Finder.SearchCriterias = this.SearchDialogGetSearchCriterias(); 
						resultContainer.Finder.SetSearchScope(catScope, feedScope);
						resultContainer.Finder.ExternalResultMerged = false;
						resultContainer.Finder.ExternalSearchPhrase = null;
						resultContainer.Finder.ExternalSearchUrl = null;

					}

					owner.SaveSearchFolders();
					resultContainer.Clear();
					panelRssSearch.Refresh();

					EmptyListView();
					htmlDetail.Clear();
					TreeSelectedNode = resultContainer;
					AsyncStartNewsSearch(resultContainer);
					break;

				case RssSearchState.Searching:
					_rssSearchState = RssSearchState.Canceled;
					this.SetSearchStatusText(Resource.Manager["RES_RssSearchCancelledStateMessage"]);
					btnSearchCancel.Enabled = false;
					break;

				case RssSearchState.Canceled:
					Debug.Assert(false);		// should not be able to press Search button while it is canceling
					break;
			}

		}

		private void RefreshRssSearchButtonStates() {
			this.btnSearchCancel.Enabled = (this.textSearchExpression.Text.Length > 0);
			if (!this.btnSearchCancel.Enabled) {
				this.btnSearchCancel.Enabled = this.checkBoxConsiderItemReadState.Checked ||
					this.checkBoxRssSearchTimeSpan.Checked ||
					this.checkBoxRssSearchByDate.Checked ||
					this.checkBoxRssSearchByDateRange.Checked;
			}
			this.btnRssSearchSave.Enabled = (this.textFinderCaption.Text.Trim().Length > 0 && this.btnSearchCancel.Enabled);
		}

		private feedsFeed[] ScopeResolve (ArrayList categories, ArrayList feedUrls) {
			if (categories == null && feedUrls == null)
				return new feedsFeed[]{};

			ArrayList result = new ArrayList();
			
			if (categories != null) {
				string sep = this.treeRssSearchScope.PathSeparator;
				foreach(feedsFeed f in owner.FeedHandler.FeedsTable.Values){
					foreach (string category in categories) {
						if(f.category != null && (f.category.Equals(category) || f.category.StartsWith(category + sep))){
							result.Add(f);
						}
					}
				}
			}

			if (feedUrls != null) {
				foreach (string url in feedUrls) {
					if (url != null && owner.FeedHandler.FeedsTable.ContainsKey(url)) {
						result.Add(owner.FeedHandler.FeedsTable[url]);
					}
				}
			}
			
			if (result.Count > 0) {
				feedsFeed[] fa = new feedsFeed[result.Count];
				result.CopyTo(fa);
				return fa;
			}

			return new feedsFeed[]{};
		}

		private void CmdNewRssSearchClick(object sender, System.EventArgs e) {
			this.CmdNewRssSearch(null);
		}
		
		private void CmdNewRssSearch(ICommand sender) {
			this.panelRssSearchCommands.SetBounds(0,0,0,40, BoundsSpecified.Height);
			this.SetSearchStatusText(String.Empty);
			this.SearchCriteriaDialogReset();
			if (sender != null)
				this.CmdDockShowRssSearch(sender);
			PopulateTreeRssSearchScope();
			this.textSearchExpression.Focus();
		}

		private SearchCriteriaCollection SearchDialogGetSearchCriterias() {
			SearchCriteriaCollection sc = new SearchCriteriaCollection();

			SearchStringElement where =SearchStringElement.Undefined;
			if (textSearchExpression.Text.Length > 0) {
				if (checkBoxRssSearchInTitle.Checked)			where |= SearchStringElement.Title;
				if (checkBoxRssSearchInDesc.Checked)		where |= SearchStringElement.Content;
				if (checkBoxRssSearchInCategory.Checked)	where |= SearchStringElement.Subject;
				if (checkBoxRssSearchInLink.Checked)			where |= SearchStringElement.Link;
			}

			StringExpressionKind kind = StringExpressionKind.Text;
			if (radioRssSearchSimpleText.Checked)
				kind = StringExpressionKind.Text;
			else if (radioRssSearchRegEx.Checked)
				kind = StringExpressionKind.RegularExpression;
			else if (radioRssSearchExprXPath.Checked)
				kind = StringExpressionKind.XPathExpression;

			if (where != SearchStringElement.Undefined) {
				SearchCriteriaString scs = new SearchCriteriaString(textSearchExpression.Text, where, kind);
				sc.Add(scs);
			}

			if (this.checkBoxConsiderItemReadState.Checked) {
				SearchCriteriaProperty scp = new SearchCriteriaProperty();
				scp.BeenRead = !checkBoxRssSearchUnreadItems.Checked;
				scp.WhatKind = PropertyExpressionKind.Unread;
				sc.Add(scp);
			}

			if (this.checkBoxRssSearchTimeSpan.Checked) {
				SearchCriteriaAge sca = new SearchCriteriaAge();
				if (radioRssSearchItemsOlderThan.Checked)
					sca.WhatKind = DateExpressionKind.OlderThan;
				else
					sca.WhatKind = DateExpressionKind.NewerThan;
				sca.WhatRelativeToToday = Utils.MapRssSearchItemAge(this.comboRssSearchItemAge.SelectedIndex);
				sc.Add(sca);
			}

			if (this.checkBoxRssSearchByDate.Checked) {
				SearchCriteriaAge sca = new SearchCriteriaAge();
				if (comboBoxRssSearchItemPostedOperator.SelectedIndex == 0)
					sca.WhatKind = DateExpressionKind.Equal;
				else if (comboBoxRssSearchItemPostedOperator.SelectedIndex == 1)
					sca.WhatKind = DateExpressionKind.OlderThan;
				else
					sca.WhatKind = DateExpressionKind.NewerThan;

				sca.What = dateTimeRssSearchItemPost.Value;
				sc.Add(sca);
			}

			if (this.checkBoxRssSearchByDateRange.Checked) {
				
				// handle case: either one date is greater than the other or equal
				if (dateTimeRssSearchPostAfter.Value > dateTimeRssSearchPostBefore.Value) {
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchPostBefore.Value, DateExpressionKind.NewerThan));
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchPostAfter.Value, DateExpressionKind.OlderThan));
				} else if (dateTimeRssSearchPostAfter.Value < dateTimeRssSearchPostBefore.Value) {
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchPostAfter.Value, DateExpressionKind.NewerThan));
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchPostBefore.Value, DateExpressionKind.OlderThan));
				} else {
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchPostBefore.Value, DateExpressionKind.Equal));
				}
			}

			return sc;
		}

		private void SearchDialogSetSearchCriterias(FinderNode node) {
			
			if (node == null)
				return;

			SearchCriteriaCollection criterias = node.Finder.SearchCriterias;
			string searchName = node.Finder.FullPath;
			
			SearchCriteriaDialogReset();

			Queue itemAgeCriterias = new Queue(2);

			foreach (ISearchCriteria criteria in criterias) {
				SearchCriteriaString str = criteria as SearchCriteriaString;
				if (str != null) {
					SearchStringElement where = str.Where;
					if ((where & SearchStringElement.Title) == SearchStringElement.Title) 
						checkBoxRssSearchInTitle.Checked = true;
					else
						checkBoxRssSearchInTitle.Checked = false;

					if ((where & SearchStringElement.Content) == SearchStringElement.Content) 
						checkBoxRssSearchInDesc.Checked = true;
					else
						checkBoxRssSearchInDesc.Checked = false;

					if ((where & SearchStringElement.Subject) == SearchStringElement.Subject) 
						checkBoxRssSearchInCategory.Checked = true;
					else
						checkBoxRssSearchInCategory.Checked = false;

					if ((where & SearchStringElement.Link) == SearchStringElement.Link) 
						checkBoxRssSearchInLink.Checked = true;
					else
						checkBoxRssSearchInLink.Checked = false;
	
					this.collapsiblePanelItemPropertiesEx.Collapsed = false;

					if (str.WhatKind == StringExpressionKind.Text)
						radioRssSearchSimpleText.Checked = true;
					else if (str.WhatKind == StringExpressionKind.RegularExpression)
						radioRssSearchRegEx.Checked = true;
					else if (str.WhatKind == StringExpressionKind.XPathExpression)
						radioRssSearchExprXPath.Checked = true;

					if (!radioRssSearchSimpleText.Checked)
						this.collapsiblePanelRssSearchExprKindEx.Collapsed = false;

					textSearchExpression.Text = str.What;
				}

				SearchCriteriaProperty prop = criteria as SearchCriteriaProperty;
				if (prop != null) {
					if (prop.WhatKind == PropertyExpressionKind.Unread) {
						if (!prop.BeenRead)
							checkBoxRssSearchUnreadItems.Checked = true;
						else
							checkBoxRssSearchUnreadItems.Checked = false;
					}
					this.checkBoxConsiderItemReadState.Checked = true;
					this.collapsiblePanelAdvancedOptionsEx.Collapsed =false;
				}

				SearchCriteriaAge age = criteria as SearchCriteriaAge;
				if (age != null) {
					if (age.WhatRelativeToToday.CompareTo(TimeSpan.Zero) != 0) {
						// relative item age specified
						this.checkBoxRssSearchTimeSpan.Checked = true;
						if (age.WhatKind == DateExpressionKind.NewerThan)
							this.radioRssSearchItemsYoungerThan.Checked = true;
						else
							this.radioRssSearchItemsOlderThan.Checked = true;
						this.comboRssSearchItemAge.SelectedIndex = Utils.MapRssSearchItemAge(age.WhatRelativeToToday);
					} else {
						// absolute item age or range specified, queue for later handling
						itemAgeCriterias.Enqueue(age);
					}
					
					if (this.collapsiblePanelAdvancedOptionsEx.Collapsed)
						this.collapsiblePanelAdvancedOptionsEx.Collapsed = false;
				}
			}

			if (itemAgeCriterias.Count == 1) {			// absolute date specified

				this.checkBoxRssSearchByDate.Checked = true;
				SearchCriteriaAge ageAbs = (SearchCriteriaAge)itemAgeCriterias.Dequeue();
				if (ageAbs.WhatKind == DateExpressionKind.Equal)
					this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 0;
				else if (ageAbs.WhatKind == DateExpressionKind.OlderThan)
					this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 1;
				else	// Newer
					this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 2;
				this.dateTimeRssSearchItemPost.Value = ageAbs.What;

			} else if (itemAgeCriterias.Count == 2) {	// range specified
				this.checkBoxRssSearchByDateRange.Checked = true;
				SearchCriteriaAge ageFrom = (SearchCriteriaAge)itemAgeCriterias.Dequeue();
				SearchCriteriaAge ageTo = (SearchCriteriaAge)itemAgeCriterias.Dequeue();
				this.dateTimeRssSearchPostAfter.Value = ageFrom.What;
				this.dateTimeRssSearchPostBefore.Value = ageTo.What;
			}

			itemAgeCriterias.Clear();

			if (node != _searchResultNode) {
				textFinderCaption.Text = searchName;
			}

			if (textFinderCaption.Text.Length > 0)
				this.collapsiblePanelSearchNameEx.Collapsed = false;

			PopulateTreeRssSearchScope();	// init, all checked. Common case

			if (this.treeRssSearchScope.Nodes.Count == 0 ||
				(node.Finder.CategoryPathScope == null || node.Finder.CategoryPathScope.Count == 0) &&
				(node.Finder.FeedUrlScope == null || node.Finder.FeedUrlScope.Count == 0))
				return;

			this.treeRssSearchScope.Nodes[0].Checked = false;	// uncheck all. 
			TreeHelper.CheckChildNodes(this.treeRssSearchScope.Nodes[0], false);

			TreeHelper.SetCheckedNodes(this.treeRssSearchScope.Nodes[0], 
				node.Finder.CategoryPathScope, node.Finder.FeedUrlScope, 
				this.treeRssSearchScope.PathSeparator.ToCharArray());

		}

		private void AddTempResultNode() {
			if (_searchResultNode == null) {
				_searchResultNode = new FinderNode(Resource.Manager["RES_RssSearchResultNodeCaption"], 10, 10, _treeTempSearchFolderContextMenu);
				_searchResultNode.Tag = _searchResultNode.InternalFeedLink;
				this.GetRoot(RootFolderType.SmartFolders).Nodes.Add(_searchResultNode);
			}
		}

		private void SearchCriteriaDialogReset() {
			//reset to default settings
			checkBoxRssSearchInTitle.Checked = checkBoxRssSearchInDesc.Checked = checkBoxRssSearchInCategory.Checked = true;
			checkBoxRssSearchInLink.Checked = checkBoxRssSearchUnreadItems.Checked = false;
			radioRssSearchSimpleText.Checked = true;
			checkBoxConsiderItemReadState.Checked = false;
			checkBoxRssSearchByDate.Checked = false;
			checkBoxRssSearchByDateRange.Checked = false;
			textSearchExpression.Text = String.Empty;
			textFinderCaption.Text = String.Empty;
		}

		private void AsyncStartNewsSearch(FinderNode node) {
			StartNewsSearchDelegate start = new StartNewsSearchDelegate(this.StartNewsSearch);
			start.BeginInvoke(node, new AsyncCallback(this.AsynInvokeCleanup) , start);
		}

		private void StartNewsSearch(FinderNode node) {
			owner.FeedHandler.SearchNewsItems(node.Finder.SearchCriterias, node.Finder.SearchScope, node.Finder);
		}

		private void AsyncStartRssRemoteSearch(string searchPhrase, string searchUrl, bool mergeWithLocalResults, bool initialize) {
			this.AddTempResultNode();

			if (initialize) {
				_searchResultNode.Clear();
				EmptyListView();
				htmlDetail.Clear();
				TreeSelectedNode = _searchResultNode;
				this.SetSearchStatusText(Resource.Manager["RES_RssSearchStateMessage"]);
			}

			SearchCriteriaCollection scc = new SearchCriteriaCollection();
			if (mergeWithLocalResults) {	// merge with local search result
				scc.Add(new SearchCriteriaString(searchPhrase, SearchStringElement.All, StringExpressionKind.Text));
			}
			RssFinder finder = new RssFinder(_searchResultNode, scc,
				null, null, new RssFinder.SearchScopeResolveCallback(this.ScopeResolve) , true);
			finder.ExternalResultMerged = mergeWithLocalResults;
			finder.ExternalSearchPhrase = searchPhrase;
			finder.ExternalSearchUrl = searchUrl;

			_searchResultNode.Finder = finder;
			StartRssRemoteSearchDelegate start = new StartRssRemoteSearchDelegate(this.StartRssRemoteSearch);
			start.BeginInvoke(searchUrl, _searchResultNode, new AsyncCallback(this.AsynInvokeCleanup) , start);
			
			if (mergeWithLocalResults) {	// start also the local search
				this.AsyncStartNewsSearch(_searchResultNode);
			}
		}

		private void StartRssRemoteSearch(string searchUrl, FinderNode resultContainer) {
			try {
				owner.FeedHandler.SearchRemoteFeed(searchUrl, resultContainer.Finder);
			} catch (Exception ex) {
				this.SetSearchStatusText("Search '"+StringHelper.ShortenByEllipsis(searchUrl, 30)+"' caused a problem: " + ex.Message);
			}
		}

		public void SearchResultAction(object tag, ArrayList matchingItems, ref bool cancel) {
			RssFinder finder = (RssFinder)tag;
			FinderNode agn = finder.Container;
			
			if (agn == null)
				return;
			
			Stack toRemove = new Stack(matchingItems.Count); // to prevent a deep copy
			
			for (int i = 0; i < matchingItems.Count; i++) {
				NewsItem item = (NewsItem)matchingItems[i];
				if (agn.Contains(item)) {
					toRemove.Push(i);
				}else{
					agn.Add(item);
				}
			}
			while (toRemove.Count > 0) {
				int i = (int)toRemove.Pop();
				matchingItems.RemoveAt(i);
			}

			toRemove = null;

			this.PopulateListView(agn, matchingItems , false, true, agn);
			cancel = (this.CurrentSearchState == RssSearchState.Canceled);
			
			Application.DoEvents();
		}

		public void SearchFinishedAction(object tag, FeedInfoList matchingFeeds, int rssFeedsCount, int NewsItemsCount) {
			Debug.Assert(!btnSearchCancel.InvokeRequired, "Wrong thread to update GUI");
			_rssSearchState = RssSearchState.Pending;
			btnSearchCancel.Text = Resource.Manager["RES_RssSearchDialogButtonSearchCaption"];
			if (NewsItemsCount == 0) {
				this.SetSearchStatusText(Resource.Manager["RES_RssSearchNoResultMessage"]);
			} else {
				this.SetSearchStatusText(Resource.Manager["RES_RssSearchSuccessResultMessage", NewsItemsCount]);
			}
			this.btnSearchCancel.Enabled = this.btnRssSearchSave.Enabled = true;
			this.textSearchExpression.Enabled = this.textFinderCaption.Enabled = true;
			this.taskPaneSearchOptions.Enabled = true;
			this.btnNewSearch.Enabled = true;

			RssFinder finder =  tag as RssFinder;
			FinderNode tn  = (finder != null ? finder.Container : null); 			

			if((tn != null) && tn.IsSelected){				
				this.BeginTransformFeedList(matchingFeeds, tn, this.owner.FeedHandler.Stylesheet); 
			}							
			
		}

		//used to generally get the EndInvoke() called for a gracefully cleanup and exception catching
		private void AsynInvokeCleanup(IAsyncResult ar) {
			StartNewsSearchDelegate startNewsSearchDelegate = ar.AsyncState as StartNewsSearchDelegate;
			if (startNewsSearchDelegate != null) {
				try {
					startNewsSearchDelegate.EndInvoke(ar);
				} catch (Exception ex) {
					_log.Error("AsyncCall 'StartNewsSearchDelegate' caused this exception", ex);
				}
				return;
			}

			GetCommentNewsItemsDelegate getCommentNewsItemsDelegate = ar.AsyncState as GetCommentNewsItemsDelegate;
			if (getCommentNewsItemsDelegate != null) {
				try {
					getCommentNewsItemsDelegate.EndInvoke(ar);
				} catch (Exception ex) {
					_log.Error("AsyncCall 'GetCommentNewsItemsDelegate' caused this exception", ex);
				}
				return;
			}

		}

		//toastNotify callbacks
		private void OnExternalDisplayFeedProperties(feedsFeed f) {
			this.DelayTask(DelayedTasks.ShowFeedPropertiesDialog, f);
		}
		private void OnExternalActivateFeedItem(NewsItem item) {
			this.DelayTask(DelayedTasks.NavigateToFeedNewsItem, item);
		}
		private void OnExternalActivateFeed(feedsFeed f) {
			this.DelayTask(DelayedTasks.NavigateToFeed, f);
		}
		
		private void DisplayFeedProperties(feedsFeed f) {
			FeedTreeNodeBase tn = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), f);
			if (tn != null) {
				this.CurrentSelectedNode = tn;
				owner.CmdShowFeedProperties(null);
				this.CurrentSelectedNode = null;
			}
		}

		private void NavigateToFeed(feedsFeed f) {

			FeedTreeNodeBase tn = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), f);
			
			if (tn != null) {
				this.TreeSelectedNode = tn;
				tn.EnsureVisible();
				this.OnTreeFeedAfterSelect(this, new TreeViewEventArgs(tn));//??
				MoveFeedDetailsToFront();
			}

			owner.CmdShowMainGui(null);
		}

		private void NavigateToFeedNewsItem(NewsItem item) {

			FeedTreeNodeBase tn = this.GetTreeNodeForItem(this.GetRoot(RootFolderType.MyFeeds), item);
			
			if (tn != null) {
				this.TreeSelectedNode = tn;
				tn.EnsureVisible();
				this.OnTreeFeedAfterSelect(this, new TreeViewEventArgs(tn));//??
				ThreadedListViewItem foundLVItem = null;
				
				for (int i = 0; i < this.listFeedItems.Items.Count; i++) {
					ThreadedListViewItem lvi = this.listFeedItems.Items[i];
					NewsItem ti = (NewsItem)lvi.Key;
					if (item.Equals(ti)) {
						foundLVItem = lvi;
						break;
					}
				}

				if (foundLVItem == null && this.listFeedItems.Items.Count > 0) {
					foundLVItem = this.listFeedItems.Items[0];
				}

				MoveFeedDetailsToFront();
					
				if (foundLVItem != null) {

					this.listFeedItems.BeginUpdate();	
					this.listFeedItems.SelectedItems.Clear(); 
					foundLVItem.Selected = true; 
					foundLVItem.Focused  = true; 										   
					this.OnFeedListItemActivate(null, EventArgs.Empty); //pass nulls because I don't use params
					SetTitleText(tn.Key);
					foundLVItem.Selected = true; 
					foundLVItem.Focused  = true; 	
					this.listFeedItems.Focus(); 
					this.listFeedItems.EnsureVisible(foundLVItem.Index); 
					this.listFeedItems.EndUpdate();		
						
				}
			}

			owner.CmdShowMainGui(null);
		}

		private void AutoSubscribeFeed(FeedTreeNodeBase parent, string feedUrl) {

			if (StringHelper.EmptyOrNull(feedUrl))
				return;

			if (parent == null) 
				parent = this.GetRoot(RootFolderType.MyFeeds);
			
			string feedTitle = null;
			string category = this.BuildCategoryStoreName(parent);

			feedUrl = owner.HandleUrlFeedProtocol(feedUrl);

			try { 

				try{ 
					Uri reqUri = new Uri(feedUrl);
					feedTitle = reqUri.Host;
				}catch(UriFormatException){
					feedTitle = feedUrl;
					if(!feedUrl.ToLower().StartsWith("http://")){
						feedUrl = "http://" + feedUrl; 						
					}				
				}

				if(owner.FeedHandler.FeedsTable.Contains(feedUrl)) {
					feedsFeed f2 = owner.FeedHandler.FeedsTable[feedUrl]; 
					owner.MessageInfo("RES_GUIFieldLinkRedundantInfo", 
						(f2.category == null? String.Empty : category + "\\") + f2.title, f2.link );
					return; 
				}

				if (owner.InternetAccessAllowed) {
				
					PrefetchFeedThreadHandler fetchHandler = new PrefetchFeedThreadHandler(feedUrl, owner.FeedHandler);

					DialogResult result = fetchHandler.Start(this, Resource.Manager.FormatMessage("RES_GUIStatusWaitMessagePrefetchFeed", feedUrl));

					if (result != DialogResult.OK)
						return;

					if (!fetchHandler.OperationSucceeds) {
						
						_log.Error("AutoSubscribeFeed() caused exception", fetchHandler.OperationException);
					
					} else {	
					
						if(fetchHandler.DiscoveredDetails != null) {

							if (fetchHandler.DiscoveredDetails.Title != null)
								feedTitle = HtmlHelper.HtmlDecode(fetchHandler.DiscoveredDetails.Title);

							// setup the new feed magically, and add them to the parent node
							feedsFeed f = fetchHandler.DiscoveredFeed;
							f.link  = feedUrl; 
							f.title = feedTitle;
							f.refreshrate = 60; 
							//f.storiesrecentlyviewed = new ArrayList(); 				
							//f.deletedstories = new ArrayList(); 				
							f.category = category;
							if(!owner.FeedHandler.Categories.ContainsKey(f.category)) {
								owner.FeedHandler.Categories.Add(f.category); 
							}

							f.alertEnabled = false;
							owner.FeedHandler.FeedsTable.Add(f.link, f); 
							owner.FeedlistModified = true;

							this.AddNewFeedNode(f.category, f);
							
							try {
								owner.FeedHandler.AsyncGetItemsForFeed(f.link, true, true);
							} catch (Exception e) {
								owner.PublishXmlFeedError(e, f.link, true);
							}

							return;
						}
					}
				}

			}	catch(Exception ex) {
				_log.Error("AutoSubscribeFeed() caused exception", ex);
			}

			// no discovered details, or
			// Exception caused, was yet report to user, or
			// No Internet access allowed
			owner.CmdNewFeed(category, feedUrl, feedTitle);
			
		}

		private void OnOwnerFeedlistLoaded(object sender, EventArgs e) {
			this.listFeedItems.FeedColumnLayout = owner.GlobalFeedColumnLayout;
		}


		private Control OnWheelSupportGetChildControl(Control control) {
			if (control == this._docContainer ) {
				if (this._docContainer.ActiveDocument != null && this._docContainer.ActiveDocument != _docFeedDetails)
					return this._docContainer.ActiveDocument.Controls[0];	// continue within docmananger hierarchy
			}
			return null;
		}

		/// <summary>
		/// Callback for DelayedTasks timer
		/// </summary>
		private void OnTasksTimerTick(object sender, EventArgs e) {
			
			if (_uiTasksTimer[DelayedTasks.SyncRssSearchTree]) {
				_uiTasksTimer.StopTask(DelayedTasks.SyncRssSearchTree);
				this.PopulateTreeRssSearchScope();
			} 

			if (_uiTasksTimer[DelayedTasks.RefreshTreeStatus]) {
				_uiTasksTimer.StopTask(DelayedTasks.RefreshTreeStatus);
				object[] param = (object[])_uiTasksTimer.GetData(DelayedTasks.RefreshTreeStatus, true);
				FeedTreeNodeBase tn = (FeedTreeNodeBase)param[0];
				int counter = (int)param[1];
				if (tn != null)
					tn.UpdateReadStatus(tn, counter);
			} 

			if (_uiTasksTimer[DelayedTasks.NavigateToWebUrl]) {
				_uiTasksTimer.StopTask(DelayedTasks.NavigateToWebUrl);
				object[] param = (object[])_uiTasksTimer.GetData(DelayedTasks.NavigateToWebUrl, true);
				this.DetailTabNavigateToUrl((string)param[0], (string)param[1], (bool)param[2], (bool)param[3]) ;
			} 
			
			if (_uiTasksTimer[DelayedTasks.StartRefreshOneFeed]) {
				_uiTasksTimer.StopTask(DelayedTasks.StartRefreshOneFeed);
				string feedUrl = (string)_uiTasksTimer.GetData(DelayedTasks.StartRefreshOneFeed, true);
				owner.FeedHandler.AsyncGetItemsForFeed(feedUrl, true, true);
			} 

			if (_uiTasksTimer[DelayedTasks.StartRefreshAllFeeds]) {
				_uiTasksTimer.StopTask(DelayedTasks.StartRefreshAllFeeds);
				//TODO 
			} 

			if (_uiTasksTimer[DelayedTasks.ShowFeedPropertiesDialog]) {
				_uiTasksTimer.StopTask(DelayedTasks.ShowFeedPropertiesDialog);
				feedsFeed f = (feedsFeed)_uiTasksTimer.GetData(DelayedTasks.ShowFeedPropertiesDialog, true);
				this.DisplayFeedProperties(f);
			} 

			if (_uiTasksTimer[DelayedTasks.NavigateToFeedNewsItem]) {
				_uiTasksTimer.StopTask(DelayedTasks.NavigateToFeedNewsItem);
				NewsItem item = (NewsItem)_uiTasksTimer.GetData(DelayedTasks.NavigateToFeedNewsItem, true);
				this.NavigateToFeedNewsItem(item);
			} 

			if (_uiTasksTimer[DelayedTasks.NavigateToFeed]) {
				_uiTasksTimer.StopTask(DelayedTasks.NavigateToFeed);
				feedsFeed f = (feedsFeed)_uiTasksTimer.GetData(DelayedTasks.NavigateToFeed, true);
				this.NavigateToFeed(f);
			} 

			if (_uiTasksTimer[DelayedTasks.AutoSubscribeFeedUrl]) {
				_uiTasksTimer.StopTask(DelayedTasks.AutoSubscribeFeedUrl);
				object[] parameter = (object[])_uiTasksTimer.GetData(DelayedTasks.AutoSubscribeFeedUrl, true);
				this.AutoSubscribeFeed((FeedTreeNodeBase) parameter[0], (string) parameter[1]);
			} 

			if (_uiTasksTimer[DelayedTasks.ClearBrowserStatusInfo]) {
				_uiTasksTimer.StopTask(DelayedTasks.ClearBrowserStatusInfo);
				this.SetBrowserStatusBarText(String.Empty);
				this.DeactivateWebProgressInfo();
				_uiTasksTimer.Interval = 100;		// reset interval 
			} 

			if (_uiTasksTimer[DelayedTasks.InitOnFinishLoading]) {
				_uiTasksTimer.StopTask(DelayedTasks.InitOnFinishLoading);
				this.OnFinishLoading(null);
			} 

			if (_uiTasksTimer[DelayedTasks.SaveUIConfiguration]) {
				_uiTasksTimer.StopTask(DelayedTasks.SaveUIConfiguration);
				this.SaveUIConfiguration(false);
			} 
			
			if(!_uiTasksTimer.AllTaskDone){
				if (!_uiTasksTimer.Enabled)
					_uiTasksTimer.Start();
			} else {
				if (_uiTasksTimer.Enabled)
					_uiTasksTimer.Stop();
			}
		}

		private void OnTreeNodeFeedsRootReadCounterZero(object sender, EventArgs e) {
			this.ResetFindersReadStatus();
			this.SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
		}

		public void DelayTask(DelayedTasks task) {
			this.DelayTask(task, null, 100);
		}
		public void DelayTask(DelayedTasks task, object data) {
			this.DelayTask(task, data, 100);
		}
		public void DelayTask(DelayedTasks task, object data, int interval) {
			_uiTasksTimer.SetData(task, data);
			if (interval > 0)
				_uiTasksTimer.Interval = interval;
			_uiTasksTimer.StartTask(task);
		}

		public void StopTask(DelayedTasks task) {
			_uiTasksTimer.StopTask(task);
		}


		#region private helper classes

		private class UITaskTimer: System.Windows.Forms.Timer {
			
			private DelayedTasks tasks;
			private Hashtable taskData = new Hashtable(7);

			public UITaskTimer():base() {}
			public UITaskTimer(IContainer component):base(component) {}
			public bool this [DelayedTasks task] {
				get { 
					if ((tasks & task) == task) 
						return true; 
					return false;
				}
				set { 
					if (value) 
						tasks |= task;
					else
						tasks ^= task;
				}
			}

			public void StartTask(DelayedTasks task) {
				this[task] = true;
				if (!base.Enabled)
					base.Start();
			}

			public void StopTask(DelayedTasks task) {
				if (base.Enabled)
					base.Stop();
				this[task] = false;
			}

			public void ClearTasks() {
				if (base.Enabled)
					base.Stop();
				tasks = DelayedTasks.None;
				taskData.Clear();
			}

			public bool AllTaskDone {
				get {
					return (tasks == DelayedTasks.None);
				}
			}

			public DelayedTasks Tasks {
				get {
					return tasks;
				}
			}
			public object GetData(DelayedTasks task) {
				return this.GetData(task, false);
			}
			public object GetData(DelayedTasks task, bool clear) {
				object data = null;
				if (taskData.ContainsKey(task)) {
					data = taskData[task];
					if (clear)
						taskData.Remove(task);
				}
				return data;
			}
			public void SetData(DelayedTasks task, object data) {
				if (taskData.ContainsKey(task)) 
					taskData.Remove(task);
				taskData.Add(task, data);
			}

		}

		#endregion

	}// end class WinGuiMain

	
}

