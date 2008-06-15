using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IEControl;
using Infragistics.Win;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using NewsComponents.Utils;
using RssBandit.Filter;
using RssBandit.Resources;
using RssBandit.Utility.Keyboard;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Forms.ControlHelpers;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;


namespace RssBandit.WinGui.Forms
{
    internal partial class WinGuiMain
    {
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
            Navigator.SelectedGroupChanging += OnNavigatorSelectedGroupChanging;
			Navigator.SelectedGroupChanged += OnNavigatorSelectedGroupChanged;          
			if (SearchIndexBehavior.NoIndexing == RssBanditApplication.SearchIndexBehavior)
            {
                //ToggleNavigationPaneView(NavigationPaneView.Subscriptions);
                owner.Mediator.SetDisabled("cmdToggleRssSearchTabState");
                Navigator.Groups[Resource.NavigatorGroup.RssSearch].Enabled = false;
            }
        }

    	private void InitOutlookListView()
        {
            listFeedItemsO.ViewStyle = ViewStyle.OutlookExpress;
            var sc = new UltraTreeNodeExtendedDateTimeComparer();
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
            searchPanel = new SearchPanel
                              {
                                  Dock = DockStyle.Fill,
                                  Location = new Point(0, 0),
                                  Name = "searchPanel",
                                  Size = new Size(237, 496),
                                  TabIndex = 0
                              };
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
            // owner.FeedlistLoaded += OnFeedlistsLoaded; NO LONGER AN EVENT 
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
            htmlDetail.FrameDownloadEnabled =
                RssBanditApplication.ReadAppSettingsEntry("FeedDetailPane.FrameDownloadEnabled", false);
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
        	treeFeeds.HideSelection = false;

			//// create RootFolderType.MyFeeds:
			//TreeFeedsNodeBase root =
			//    new SubscriptionRootNode(SR.FeedNodeMyFeedsCaption, Resource.SubscriptionTreeImage.AllSubscriptions,
			//                 Resource.SubscriptionTreeImage.AllSubscriptionsExpanded, _treeRootContextMenu);
			//treeFeeds.Nodes.Add(root);
			//root.ReadCounterZero += OnTreeNodeFeedsRootReadCounterZero;
            //_roots[(int) RootFolderType.MyFeeds] = root;

            // add the root as the first history entry:
            //AddHistoryEntry(root, null);

            // create RootFolderType.Finder:
			TreeFeedsNodeBase root =
                new FinderRootNode(SR.FeedNodeFinderRootCaption, Resource.SubscriptionTreeImage.AllFinderFolders,
                                   Resource.SubscriptionTreeImage.AllFinderFoldersExpanded,
                                   _treeSearchFolderRootContextMenu);
            treeFeeds.Nodes.Add(root);
            //_roots[(int) RootFolderType.Finder] = root;
            if (SearchIndexBehavior.NoIndexing == RssBanditApplication.SearchIndexBehavior)
                root.Visible = false;

            // create RootFolderType.SmartFolder:
            root =
                new SpecialRootNode(SR.FeedNodeSpecialFeedsCaption, Resource.SubscriptionTreeImage.AllSmartFolders,
                                    Resource.SubscriptionTreeImage.AllSmartFoldersExpanded, null);
            treeFeeds.Nodes.Add(root);
            //_roots[(int) RootFolderType.SmartFolders] = root;

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
            historyMenuManager.OnNavigateForward += OnHistoryNavigateGoForwardItemClick;

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

        #region Menu init routines

        protected void InitContextMenus()
        {
            #region tree view context menus

            #region root menu

            _subscriptionTreeRootContextMenu = new ContextMenu();

            var sub1 = new AppContextMenuCommand("cmdNewFeed",
                                                 owner.Mediator,
                                                 new ExecuteCommandHandler(owner.CmdNewFeed),
                                                 SR.MenuNewFeedCaption2, SR.MenuNewFeedDesc, 1,
                                                 _shortcutHandler);

            //sub1.ImageList  = _toolImages;

            var sub2 = new AppContextMenuCommand("cmdNewCategory",
                                                 owner.Mediator,
                                                 new ExecuteCommandHandler(owner.CmdNewCategory),
                                                 SR.MenuNewCategoryCaption, SR.MenuNewCategoryDesc, 2,
                                                 _shortcutHandler);

            //sub2.ImageList  = _treeImages;

            var sep = new MenuItem("-");

            var subR1 = new AppContextMenuCommand("cmdRefreshFeeds",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdRefreshFeeds),
                                                  SR.MenuUpdateAllFeedsCaption,
                                                  SR.MenuUpdateAllFeedsDesc, 0, _shortcutHandler);

            //subR1.ImageList  = _toolImages;

            var subR2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdCatchUpCurrentSelectedNode),
                                                  SR.MenuCatchUpOnAllCaption, SR.MenuCatchUpOnAllDesc,
                                                  0, _shortcutHandler);
            //subR2.ImageList           = _listImages;

            var subR3 = new AppContextMenuCommand("cmdDeleteAll",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdDeleteAll),
                                                  SR.MenuDeleteAllFeedsCaption,
                                                  SR.MenuDeleteAllFeedsDesc, 2, _shortcutHandler);
            //subR3.ImageList           = _toolImages;

            var subR4 = new AppContextMenuCommand("cmdShowMainAppOptions",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdShowOptions),
                                                  SR.MenuAppOptionsCaption, SR.MenuAppOptionsDesc, 10,
                                                  _shortcutHandler);

			var subSourceRename = new AppContextMenuCommand("cmdRenameFeedSource",
												  owner.Mediator,
												  new ExecuteCommandHandler(CmdRenameFeedSource),
												  SR.MenuRenameFeedSourceCaption, SR.MenuRenameFeedSourceDesc,
												  _shortcutHandler);
			var subSourceDelete = new AppContextMenuCommand("cmdDeleteFeedSource",
												  owner.Mediator,
												  new ExecuteCommandHandler(CmdDeleteFeedSource),
												  SR.MenuDeleteFeedSourceCaption, SR.MenuDeleteFeedSourceDesc,
												  _shortcutHandler);

            // append items
            _subscriptionTreeRootContextMenu.MenuItems.AddRange(
				new[] { sub1, sub2, sep, subR1, subR2, sep.CloneMenu(), subR3, sep.CloneMenu(), subSourceRename, subSourceDelete, sep.CloneMenu(), subR4 });

            #endregion

            #region category menu

            _treeCategoryContextMenu = new ContextMenu();

            var subC1 = new AppContextMenuCommand("cmdUpdateCategory",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdUpdateCategory),
                                                  SR.MenuUpdateCategoryCaption,
                                                  SR.MenuUpdateCategoryDesc, _shortcutHandler);

            var subC2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdCatchUpCurrentSelectedNode),
                                                  SR.MenuCatchUpCategoryCaption,
                                                  SR.MenuCatchUpCategoryDesc, 0, _shortcutHandler);
            //subC2.ImageList            = _listImages;

            var subC3 = new AppContextMenuCommand("cmdRenameCategory",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdRenameCategory),
                                                  SR.MenuRenameCategoryCaption,
                                                  SR.MenuRenameCategoryDesc, _shortcutHandler);

            var subC4 = new AppContextMenuCommand("cmdDeleteCategory",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdDeleteCategory),
                                                  SR.MenuDeleteCategoryCaption,
                                                  SR.MenuDeleteCategoryDesc, 2, _shortcutHandler);

            //subC4.ImageList            = _toolImages;

            var subC5 = new AppContextMenuCommand("cmdShowCategoryProperties",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdShowCategoryProperties),
                                                  SR.MenuShowCategoryPropertiesCaption,
                                                  SR.MenuShowCategoryPropertiesDesc, 10,
                                                  _shortcutHandler);

            var subCL_ColLayoutMain = new AppContextMenuCommand("cmdColumnChooserMain",
                                                                owner.Mediator,
                                                                new ExecuteCommandHandler(CmdNop),
                                                                SR.MenuColumnChooserCaption,
                                                                SR.MenuColumnChooserDesc,
                                                                _shortcutHandler);

            foreach (var colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                var subCL4_layoutSubColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
                                                                       owner.Mediator,
                                                                       new ExecuteCommandHandler(
                                                                           CmdToggleListviewColumn),
                                                                       SR.ResourceManager.GetString(
                                                                           "MenuColumnChooser" + colID +
                                                                           "Caption"),
                                                                       SR.ResourceManager.GetString(
                                                                           "MenuColumnChooser" + colID +
                                                                           "Desc"), _shortcutHandler);

                subCL_ColLayoutMain.MenuItems.AddRange(new MenuItem[] {subCL4_layoutSubColumn});
            }

            var subCL_subUseCatLayout =
                new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
                                          owner.Mediator,
                                          new ExecuteCommandHandler(CmdColumnChooserUseCategoryLayoutGlobal),
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalDesc, _shortcutHandler);

            var subCL_subUseFeedLayout =
                new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
                                          owner.Mediator, new ExecuteCommandHandler(CmdColumnChooserUseFeedLayoutGlobal),
                                          SR.MenuColumnChooserUseFeedLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseFeedLayoutGlobalDesc, _shortcutHandler);

            var subCL_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
                                                                 owner.Mediator,
                                                                 new ExecuteCommandHandler(
                                                                     CmdColumnChooserResetToDefault),
                                                                 SR.
                                                                     MenuColumnChooserResetLayoutToDefaultCaption,
                                                                 SR.
                                                                     MenuColumnChooserResetLayoutToDefaultDesc,
                                                                 _shortcutHandler);

            subCL_ColLayoutMain.MenuItems.AddRange(
                new[]
                    {
                        sep.CloneMenu(), subCL_subUseCatLayout, subCL_subUseFeedLayout, sep.CloneMenu(),
                        subCL_subResetLayout
                    });

            // append items. Reuse cmdNewCat/cmdNewFeed, because it's allowed on categories
            _treeCategoryContextMenu.MenuItems.AddRange(
                new[]
                    {
                        sub1.CloneMenu(), sub2.CloneMenu(), sep.CloneMenu(), subC1, subC2, sep.CloneMenu(), subC3,
                        sep.CloneMenu(), subC4, sep.CloneMenu(), subCL_ColLayoutMain, sep.CloneMenu(), subC5
                    });

            #endregion

            #region feed menu

            _treeFeedContextMenu = new ContextMenu();

            var subF1 = new AppContextMenuCommand("cmdUpdateFeed",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdUpdateFeed),
                                                  SR.MenuUpdateThisFeedCaption,
                                                  SR.MenuUpdateThisFeedDesc, _shortcutHandler);

            var subF2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdCatchUpCurrentSelectedNode),
                                                  SR.MenuCatchUpThisFeedCaption,
                                                  SR.MenuCatchUpThisFeedDesc, 0, _shortcutHandler);

            //subF2.ImageList                     = _listImages;
            var subF3 = new AppContextMenuCommand("cmdRenameFeed",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdRenameFeed),
                                                  SR.MenuRenameThisFeedCaption,
                                                  SR.MenuRenameThisFeedDesc, _shortcutHandler);

            var subF4 = new AppContextMenuCommand("cmdDeleteFeed",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(CmdDeleteFeed),
                                                  SR.MenuDeleteThisFeedCaption,
                                                  SR.MenuDeleteThisFeedDesc, 2, _shortcutHandler);

            //subF4.ImageList            = _toolImages;

            var subFeedCopy = new AppContextMenuCommand("cmdCopyFeed",
                                                        owner.Mediator,
                                                        new ExecuteCommandHandler(CmdCopyFeed),
                                                        SR.MenuCopyFeedCaption, SR.MenuCopyFeedDesc, 1,
                                                        _shortcutHandler);

            var subFeedCopy_sub1 = new AppContextMenuCommand("cmdCopyFeedLinkToClipboard",
                                                             owner.Mediator,
                                                             new ExecuteCommandHandler(
                                                                 CmdCopyFeedLinkToClipboard),
                                                             SR.MenuCopyFeedLinkToClipboardCaption,
                                                             SR.MenuCopyFeedLinkToClipboardDesc, 1,
                                                             _shortcutHandler);

            var subFeedCopy_sub2 = new AppContextMenuCommand("cmdCopyFeedHomepageLinkToClipboard",
                                                             owner.Mediator,
                                                             new ExecuteCommandHandler(
                                                                 CmdCopyFeedHomeLinkToClipboard),
                                                             SR.MenuCopyFeedHomeLinkToClipboardCaption,
                                                             SR.MenuCopyFeedHomeLinkToClipboardDesc, 1,
                                                             _shortcutHandler);

            var subFeedCopy_sub3 =
                new AppContextMenuCommand("cmdCopyFeedHomepageTitleLinkToClipboard",
                                          owner.Mediator, new ExecuteCommandHandler(CmdCopyFeedHomeTitleLinkToClipboard),
                                          SR.MenuCopyFeedFeedHomeTitleLinkToClipboardCaption,
                                          SR.MenuCopyFeedFeedHomeTitleLinkToClipboardDesc, 1, _shortcutHandler);

            subFeedCopy.MenuItems.AddRange(new MenuItem[] {subFeedCopy_sub1, subFeedCopy_sub2, subFeedCopy_sub3});


            _feedInfoContextMenu = new MenuItem(SR.MenuAdvancedFeedInfoCaption);

            // the general properties item
            var subF6 = new AppContextMenuCommand("cmdShowFeedProperties",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdShowFeedProperties),
                                                  SR.MenuShowFeedPropertiesCaption,
                                                  SR.MenuShowFeedPropertiesDesc, 10, _shortcutHandler);
            //subF6.ImageList				     = _browserImages;

            // layout menu(s):
            var subFL_ColLayoutMain = new AppContextMenuCommand("cmdColumnChooserMain",
                                                                owner.Mediator,
                                                                new ExecuteCommandHandler(CmdNop),
                                                                SR.MenuColumnChooserCaption,
                                                                SR.MenuColumnChooserDesc,
                                                                _shortcutHandler);

            foreach (var colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                var subFL4_layoutSubColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
                                                                       owner.Mediator,
                                                                       new ExecuteCommandHandler(
                                                                           CmdToggleListviewColumn),
                                                                       SR.ResourceManager.GetString(
                                                                           "MenuColumnChooser" + colID +
                                                                           "Caption"),
                                                                       SR.ResourceManager.GetString(
                                                                           "MenuColumnChooser" + colID +
                                                                           "Desc"), _shortcutHandler);

                subFL_ColLayoutMain.MenuItems.AddRange(new MenuItem[] {subFL4_layoutSubColumn});
            }

            var subFL_subUseCatLayout =
                new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
                                          owner.Mediator,
                                          new ExecuteCommandHandler(CmdColumnChooserUseCategoryLayoutGlobal),
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalDesc, _shortcutHandler);

            var subFL_subUseFeedLayout =
                new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
                                          owner.Mediator, new ExecuteCommandHandler(CmdColumnChooserUseFeedLayoutGlobal),
                                          SR.MenuColumnChooserUseFeedLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseFeedLayoutGlobalDesc, _shortcutHandler);

            var subFL_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
                                                                 owner.Mediator,
                                                                 new ExecuteCommandHandler(
                                                                     CmdColumnChooserResetToDefault),
                                                                 SR.
                                                                     MenuColumnChooserResetLayoutToDefaultCaption,
                                                                 SR.
                                                                     MenuColumnChooserResetLayoutToDefaultDesc,
                                                                 _shortcutHandler);

            subFL_ColLayoutMain.MenuItems.AddRange(
                new[]
                    {
                        sep.CloneMenu(), subFL_subUseCatLayout, subFL_subUseFeedLayout, sep.CloneMenu(),
                        subFL_subResetLayout
                    });

            // append items. 
            _treeFeedContextMenu.MenuItems.AddRange(
                new[]
                    {
                        subF1, subF2, subF3, sep.CloneMenu(), subF4, sep.CloneMenu(), subFeedCopy, sep.CloneMenu(),
                        _feedInfoContextMenu, sep.CloneMenu(), subFL_ColLayoutMain, sep.CloneMenu(), subF6
                    });

            #endregion

            #region feed info context submenu

            var subInfoHome = new AppContextMenuCommand("cmdNavigateToFeedHome",
                                                        owner.Mediator,
                                                        new ExecuteCommandHandler(
                                                            owner.CmdNavigateFeedHome),
                                                        SR.MenuNavigateToFeedHomeCaption,
                                                        SR.MenuNavigateToFeedHomeDesc,
                                                        _shortcutHandler);

            var subInfoCosmos = new AppContextMenuCommand("cmdNavigateToFeedCosmos",
                                                          owner.Mediator,
                                                          new ExecuteCommandHandler(
                                                              owner.CmdNavigateFeedLinkCosmos),
                                                          SR.MenuShowLinkCosmosCaption,
                                                          SR.MenuShowLinkCosmosCaption);

            var subInfoSource = new AppContextMenuCommand("cmdViewSourceOfFeed",
                                                          owner.Mediator,
                                                          new ExecuteCommandHandler(
                                                              owner.CmdViewSourceOfFeed),
                                                          SR.MenuViewSourceOfFeedCaption,
                                                          SR.MenuViewSourceOfFeedDesc,
                                                          _shortcutHandler);

            var subInfoValidate = new AppContextMenuCommand("cmdValidateFeed",
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

            _treeSearchFolderRootContextMenu.MenuItems.AddRange(new[] {subF1, sep.CloneMenu(), subF2});

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
            var subFinderShowFullText = new AppContextMenuCommand("cmdFinderShowFullItemText",
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
                new[]
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
                                              _shortcutHandler)
                        {
                            Enabled = false
                        };
            subF4 = new AppContextMenuCommand("cmdShowFinderProperties",
                                              owner.Mediator, new ExecuteCommandHandler(CmdShowFinderProperties),
                                              SR.MenuShowFinderPropertiesCaption, SR.MenuShowFinderPropertiesDesc,
                                              _shortcutHandler);

            _treeTempSearchFolderContextMenu.MenuItems.AddRange(
                new[]
                    {
                        subF1, subF2, sep.CloneMenu(), subF3, sep.CloneMenu(), subFinderShowFullText.CloneMenu(),
                        sep.CloneMenu(), subF4
                    });

            #endregion

            treeFeeds.ContextMenu = _subscriptionTreeRootContextMenu; // init to root context

            #endregion

            #region list view context menu

            _listContextMenu = new ContextMenu();

            var subL0 = new AppContextMenuCommand("cmdMarkSelectedFeedItemsRead",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdMarkFeedItemsRead),
                                                  SR.MenuCatchUpSelectedNodeCaption,
                                                  SR.MenuCatchUpSelectedNodeDesc, 0, _shortcutHandler);

            var subL1 = new AppContextMenuCommand("cmdMarkSelectedFeedItemsUnread",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdMarkFeedItemsUnread),
                                                  SR.MenuMarkFeedItemsUnreadCaption,
                                                  SR.MenuMarkFeedItemsUnreadDesc, 1, _shortcutHandler);

            //subL1.ImageList           = _listImages;

            var subL2 = new AppContextMenuCommand("cmdFeedItemPostReply",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdPostReplyToItem),
                                                  SR.MenuFeedItemPostReplyCaption,
                                                  SR.MenuFeedItemPostReplyDesc, 5, _shortcutHandler)
                            {
                                Enabled = false
                            };
            //subL2.ImageList = _toolImages;

            var subL3 = new AppContextMenuCommand("cmdFlagNewsItem",
                                                  owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                  SR.MenuFlagFeedItemCaption, SR.MenuFlagFeedItemDesc,
                                                  1, _shortcutHandler)
                            {
                                Enabled = false
                            };
            //subL3.ImageList                  = _listImages;

            var subL3_sub1 = new AppContextMenuCommand("cmdFlagNewsItemForFollowUp",
                                                       owner.Mediator,
                                                       new ExecuteCommandHandler(
                                                           CmdFlagNewsItemForFollowUp),
                                                       SR.MenuFlagFeedItemFollowUpCaption,
                                                       SR.MenuFlagFeedItemFollowUpDesc, 1,
                                                       _shortcutHandler);
            var subL3_sub2 = new AppContextMenuCommand("cmdFlagNewsItemForReview",
                                                       owner.Mediator,
                                                       new ExecuteCommandHandler(
                                                           CmdFlagNewsItemForReview),
                                                       SR.MenuFlagFeedItemReviewCaption,
                                                       SR.MenuFlagFeedItemReviewDesc, 1,
                                                       _shortcutHandler);
            var subL3_sub3 = new AppContextMenuCommand("cmdFlagNewsItemForReply",
                                                       owner.Mediator,
                                                       new ExecuteCommandHandler(
                                                           CmdFlagNewsItemForReply),
                                                       SR.MenuFlagFeedItemReplyCaption,
                                                       SR.MenuFlagFeedItemReplyDesc, 1,
                                                       _shortcutHandler);
            var subL3_sub4 = new AppContextMenuCommand("cmdFlagNewsItemRead",
                                                       owner.Mediator,
                                                       new ExecuteCommandHandler(CmdFlagNewsItemRead),
                                                       SR.MenuFlagFeedItemReadCaption,
                                                       SR.MenuFlagFeedItemReadDesc, 1,
                                                       _shortcutHandler);
            var subL3_sub5 = new AppContextMenuCommand("cmdFlagNewsItemForward",
                                                       owner.Mediator,
                                                       new ExecuteCommandHandler(
                                                           CmdFlagNewsItemForward),
                                                       SR.MenuFlagFeedItemForwardCaption,
                                                       SR.MenuFlagFeedItemForwardDesc, 1,
                                                       _shortcutHandler);
            var subL3_sub8 = new AppContextMenuCommand("cmdFlagNewsItemComplete",
                                                       owner.Mediator,
                                                       new ExecuteCommandHandler(
                                                           CmdFlagNewsItemComplete),
                                                       SR.MenuFlagFeedItemCompleteCaption,
                                                       SR.MenuFlagFeedItemCompleteDesc, 1,
                                                       _shortcutHandler);
            var subL3_sub9 = new AppContextMenuCommand("cmdFlagNewsItemNone",
                                                       owner.Mediator,
                                                       new ExecuteCommandHandler(CmdFlagNewsItemNone),
                                                       SR.MenuFlagFeedItemClearCaption,
                                                       SR.MenuFlagFeedItemClearDesc, 1,
                                                       _shortcutHandler);

            subL3.MenuItems.AddRange(
                new[]
                    {
                        subL3_sub1, subL3_sub2, subL3_sub3, subL3_sub4, subL3_sub5, sep.CloneMenu(), subL3_sub8,
                        sep.CloneMenu(), subL3_sub9
                    });

            var subL10 = new AppContextMenuCommand("cmdCopyNewsItem",
                                                   owner.Mediator,
                                                   new ExecuteCommandHandler(CmdCopyNewsItem),
                                                   SR.MenuCopyFeedItemCaption, SR.MenuCopyFeedItemDesc,
                                                   1, _shortcutHandler);

            var subL10_sub1 = new AppContextMenuCommand("cmdCopyNewsItemLinkToClipboard",
                                                        owner.Mediator,
                                                        new ExecuteCommandHandler(
                                                            CmdCopyNewsItemLinkToClipboard),
                                                        SR.MenuCopyFeedItemLinkToClipboardCaption,
                                                        SR.MenuCopyFeedItemLinkToClipboardDesc, 1,
                                                        _shortcutHandler);
            var subL10_sub2 = new AppContextMenuCommand("cmdCopyNewsItemTitleLinkToClipboard",
                                                        owner.Mediator,
                                                        new ExecuteCommandHandler(
                                                            CmdCopyNewsItemTitleLinkToClipboard),
                                                        SR.MenuCopyFeedItemTitleLinkToClipboardCaption,
                                                        SR.MenuCopyFeedItemTitleLinkToClipboardDesc, 1,
                                                        _shortcutHandler);
            var subL10_sub3 = new AppContextMenuCommand("cmdCopyNewsItemContentToClipboard",
                                                        owner.Mediator,
                                                        new ExecuteCommandHandler(
                                                            CmdCopyNewsItemContentToClipboard),
                                                        SR.MenuCopyFeedItemContentToClipboardCaption,
                                                        SR.MenuCopyFeedItemContentToClipboardDesc, 1,
                                                        _shortcutHandler);

            subL10.MenuItems.AddRange(new MenuItem[] {subL10_sub1, subL10_sub2, subL10_sub3});


            var subL4 = new AppContextMenuCommand("cmdColumnChooserMain",
                                                  owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                  SR.MenuColumnChooserCaption,
                                                  SR.MenuColumnChooserDesc, _shortcutHandler);
            //subL3.ImageList                  = _listImages;

            foreach (var colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                var subL4_subColumn = new AppContextMenuCommand("cmdListviewColumn." + colID,
                                                                owner.Mediator,
                                                                new ExecuteCommandHandler(
                                                                    CmdToggleListviewColumn),
                                                                SR.ResourceManager.GetString(
                                                                    "MenuColumnChooser" + colID +
                                                                    "Caption"),
                                                                SR.ResourceManager.GetString(
                                                                    "MenuColumnChooser" + colID +
                                                                    "Desc"), _shortcutHandler);

                subL4.MenuItems.AddRange(new MenuItem[] {subL4_subColumn});
            }

            var subL4_subUseCatLayout =
                new AppContextMenuCommand("cmdColumnChooserUseCategoryLayoutGlobal",
                                          owner.Mediator,
                                          new ExecuteCommandHandler(CmdColumnChooserUseCategoryLayoutGlobal),
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseCategoryLayoutGlobalDesc, _shortcutHandler);

            var subL4_subUseFeedLayout =
                new AppContextMenuCommand("cmdColumnChooserUseFeedLayoutGlobal",
                                          owner.Mediator, new ExecuteCommandHandler(CmdColumnChooserUseFeedLayoutGlobal),
                                          SR.MenuColumnChooserUseFeedLayoutGlobalCaption,
                                          SR.MenuColumnChooserUseFeedLayoutGlobalDesc, _shortcutHandler);

            var subL4_subResetLayout = new AppContextMenuCommand("cmdColumnChooserResetToDefault",
                                                                 owner.Mediator,
                                                                 new ExecuteCommandHandler(
                                                                     CmdColumnChooserResetToDefault),
                                                                 SR.
                                                                     MenuColumnChooserResetLayoutToDefaultCaption,
                                                                 SR.
                                                                     MenuColumnChooserResetLayoutToDefaultDesc,
                                                                 _shortcutHandler);

            subL4.MenuItems.AddRange(
                new[]
                    {
                        sep.CloneMenu(), subL4_subUseCatLayout, subL4_subUseFeedLayout, sep.CloneMenu(),
                        subL4_subResetLayout
                    });

            var subL5 = new AppContextMenuCommand("cmdDeleteSelectedNewsItems",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdDeleteSelectedFeedItems),
                                                  SR.MenuDeleteSelectedFeedItemsCaption,
                                                  SR.MenuDeleteSelectedFeedItemsDesc, _shortcutHandler);

            var subL6 = new AppContextMenuCommand("cmdRestoreSelectedNewsItems",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdRestoreSelectedFeedItems),
                                                  SR.MenuRestoreSelectedFeedItemsCaption,
                                                  SR.MenuRestoreSelectedFeedItemsDesc,
                                                  _shortcutHandler)
                            {
                                Visible = false
                            };

            var subL7 = new AppContextMenuCommand("cmdWatchItemComments",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(CmdWatchItemComments),
                                                  SR.MenuFeedItemWatchCommentsCaption,
                                                  SR.MenuFeedItemWatchCommentsDesc, 5,
                                                  _shortcutHandler)
                            {
                                Enabled = false
                            };
            //subL2.ImageList = _toolImages;
            // dynamically enabled on runtime if feed supports thr:replied, slash:comments or wfw:commentRss			

            var subL8 = new AppContextMenuCommand("cmdViewOutlookReadingPane",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(CmdViewOutlookReadingPane),
                                                  SR.MenuViewOutlookReadingPane,
                                                  SR.MenuViewOutlookReadingPane, _shortcutHandler);

            var subL9 = new AppContextMenuCommand("cmdDownloadAttachment",
                                                  owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                  SR.MenuDownloadAttachmentCaption,
                                                  SR.MenuDownloadAttachmentDesc, _shortcutHandler)
                            {
                                Visible = false
                            };


            _listContextMenuDownloadAttachment = subL9;
            _listContextMenuDeleteItemsSeparator = sep.CloneMenu();
            _listContextMenuDownloadAttachmentsSeparator = sep.CloneMenu();
            _listContextMenu.MenuItems.AddRange(
                new[]
                    {
                        subL2, subL3, subL0, subL1, subL7, sep.CloneMenu(), subL10,
                        _listContextMenuDownloadAttachmentsSeparator, subL9, _listContextMenuDeleteItemsSeparator, subL5
                        ,
                        subL6, sep.CloneMenu(), subL4, subL8
                    });
            listFeedItems.ContextMenu = _listContextMenu;
            listFeedItemsO.ContextMenu = _listContextMenu;

            #endregion

            #region Local Feeds context menu

            _treeLocalFeedContextMenu = new ContextMenu();

            var subTL1 = new AppContextMenuCommand("cmdDeleteAllNewsItems",
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

            var subDT1 = new AppContextMenuCommand("cmdDocTabCloseThis",
                                                   owner.Mediator,
                                                   new ExecuteCommandHandler(CmdDocTabCloseSelected),
                                                   SR.MenuDocTabsCloseCurrentCaption,
                                                   SR.MenuDocTabsCloseCurrentDesc, 1, _shortcutHandler);
            //subDT1.ImageList           = _listImages;

            var subDT2 = new AppContextMenuCommand("cmdDocTabCloseAllOnStrip",
                                                   owner.Mediator,
                                                   new ExecuteCommandHandler(CmdDocTabCloseAllOnStrip),
                                                   SR.MenuDocTabsCloseAllOnStripCaption,
                                                   SR.MenuDocTabsCloseAllOnStripDesc, 2,
                                                   _shortcutHandler);
            //subDT2.ImageList           = _listImages;

            var subDT3 = new AppContextMenuCommand("cmdDocTabCloseAll",
                                                   owner.Mediator,
                                                   new ExecuteCommandHandler(CmdDocTabCloseAll),
                                                   SR.MenuDocTabsCloseAllCaption,
                                                   SR.MenuDocTabsCloseAllDesc, 3, _shortcutHandler);
            //subDT3.ImageList           = _listImages;

            var subDT4 = new AppContextMenuCommand("cmdDocTabLayoutHorizontal",
                                                   owner.Mediator,
                                                   new ExecuteCommandHandler(CmdDocTabLayoutHorizontal),
                                                   SR.MenuDocTabsLayoutHorizontalCaption,
                                                   SR.MenuDocTabsLayoutHorizontalDesc,
                                                   _shortcutHandler)
                             {
                                 Checked = (_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal)
                             };


            var subDT5 = new AppContextMenuCommand("cmdFeedDetailLayoutPosition",
                                                   owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                   SR.MenuFeedDetailLayoutCaption,
                                                   SR.MenuFeedDetailLayoutDesc, _shortcutHandler);

            // subMenu:			
            var subSub1 = new AppContextMenuCommand("cmdFeedDetailLayoutPosTop",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(
                                                        CmdFeedDetailLayoutPosTop),
                                                    SR.MenuFeedDetailLayoutTopCaption,
                                                    SR.MenuFeedDetailLayoutTopDesc, _shortcutHandler)
                              {
                                  Checked = true
                              };

            var subSub2 = new AppContextMenuCommand("cmdFeedDetailLayoutPosLeft",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(
                                                        CmdFeedDetailLayoutPosLeft),
                                                    SR.MenuFeedDetailLayoutLeftCaption,
                                                    SR.MenuFeedDetailLayoutLeftDesc, _shortcutHandler);

            var subSub3 = new AppContextMenuCommand("cmdFeedDetailLayoutPosRight",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(
                                                        CmdFeedDetailLayoutPosRight),
                                                    SR.MenuFeedDetailLayoutRightCaption,
                                                    SR.MenuFeedDetailLayoutRightDesc, _shortcutHandler);

            var subSub4 = new AppContextMenuCommand("cmdFeedDetailLayoutPosBottom",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(
                                                        CmdFeedDetailLayoutPosBottom),
                                                    SR.MenuFeedDetailLayoutBottomCaption,
                                                    SR.MenuFeedDetailLayoutBottomDesc,
                                                    _shortcutHandler);


            subDT5.MenuItems.AddRange(new MenuItem[] {subSub1, subSub2, subSub3, subSub4});


            _docTabContextMenu.MenuItems.AddRange(
                new[] {subDT1, subDT2, subDT3, sep.CloneMenu(), subDT4, sep.CloneMenu(), subDT5});

            #endregion

            #region tray context menu

            _notifyContextMenu = new ContextMenu();

            var subT1 = new AppContextMenuCommand("cmdShowGUI",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdShowMainGui),
                                                  SR.MenuShowMainGuiCaption, SR.MenuShowMainGuiDesc,
                                                  _shortcutHandler)
                            {
                                DefaultItem = true
                            };

            var subT1_1 = new AppContextMenuCommand("cmdRefreshFeeds",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(owner.CmdRefreshFeeds),
                                                    SR.MenuUpdateAllFeedsCaption,
                                                    SR.MenuUpdateAllFeedsDesc, _shortcutHandler);
            var subT2 = new AppContextMenuCommand("cmdShowMainAppOptions",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdShowOptions),
                                                  SR.MenuAppOptionsCaption, SR.MenuAppOptionsDesc, 10,
                                                  _shortcutHandler);
            //subT2.ImageList = _browserImages;

            var subT5 = new AppContextMenuCommand("cmdShowConfiguredAlertWindows",
                                                  owner.Mediator, new ExecuteCommandHandler(CmdNop),
                                                  SR.MenuShowAlertWindowsCaption,
                                                  SR.MenuShowAlertWindowsDesc, _shortcutHandler);
            //subT5.Checked = owner.Preferences.ShowConfiguredAlertWindows;

            #region ShowAlertWindows context submenu

            var subT5_1 = new AppContextMenuCommand("cmdShowAlertWindowNone",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(
                                                        owner.CmdShowAlertWindowNone),
                                                    SR.MenuShowNoneAlertWindowsCaption,
                                                    SR.MenuShowNoneAlertWindowsDesc, _shortcutHandler)
                              {
                                  Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.None)
                              };

            var subT5_2 = new AppContextMenuCommand("cmdShowAlertWindowConfiguredFeeds",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(
                                                        owner.CmdShowAlertWindowConfigPerFeed),
                                                    SR.MenuShowConfiguredFeedAlertWindowsCaption,
                                                    SR.MenuShowConfiguredFeedAlertWindowsDesc,
                                                    _shortcutHandler)
                              {
                                  Checked =
                                      (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.AsConfiguredPerFeed)
                              };

            var subT5_3 = new AppContextMenuCommand("cmdShowAlertWindowAll",
                                                    owner.Mediator,
                                                    new ExecuteCommandHandler(
                                                        owner.CmdShowAlertWindowAll),
                                                    SR.MenuShowAllAlertWindowsCaption,
                                                    SR.MenuShowAllAlertWindowsDesc, _shortcutHandler)
                              {
                                  Checked = (owner.Preferences.ShowAlertWindow == DisplayFeedAlertWindow.All)
                              };

            subT5.MenuItems.AddRange(new MenuItem[] {subT5_1, subT5_2, subT5_3});

            #endregion

            var subT6 = new AppContextMenuCommand("cmdShowNewItemsReceivedBalloon",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(
                                                      owner.CmdToggleShowNewItemsReceivedBalloon),
                                                  SR.MenuShowNewItemsReceivedBalloonCaption,
                                                  SR.MenuShowNewItemsReceivedBalloonDesc,
                                                  _shortcutHandler)
                            {
                                Checked = owner.Preferences.ShowNewItemsReceivedBalloon
                            };

            var subT10 = new AppContextMenuCommand("cmdCloseExit",
                                                   owner.Mediator,
                                                   new ExecuteCommandHandler(owner.CmdExitApp),
                                                   SR.MenuAppCloseExitCaption, SR.MenuAppCloseExitDesc,
                                                   _shortcutHandler);

            _notifyContextMenu.MenuItems.AddRange(
                new[]
                    {
                        subT1, subT1_1, sep.CloneMenu(), sub1.CloneMenu(), subT2, sep.CloneMenu(), subT5, subT6,
                        sep.CloneMenu(), subT10
                    });

            #endregion
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
    }
}