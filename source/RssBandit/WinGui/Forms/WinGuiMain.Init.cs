using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using IEControl;
using Infragistics.Win;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using NewsComponents.Resources;
using NewsComponents.Utils;
using RssBandit.Filter;
using RssBandit.Resources;
using RssBandit.Utility.Keyboard;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Controls.TreeView;
using RssBandit.WinGui.Forms.ControlHelpers;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Shell;
using TD.SandDock;

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
			Load += OnFormLoad;
            HandleCreated += OnFormHandleCreated;
            Move += OnFormMove;
            Activated += OnFormActivated;
            Deactivate += OnFormDeactivate;
            Shown += OnFormShown;

            owner.PreferencesChanged += OnPreferencesChanged;
			owner.FeedSourceFeedDeleted += OnFeedDeleted;
			owner.FeedSourceSubscriptionsLoaded += OnFeedSourceSubscriptionsLoaded;
			owner.AllFeedSourceSubscriptionsLoaded += OnAllFeedSourceSubscriptionsLoaded;

            wheelSupport = new WheelSupport(this);
            wheelSupport.OnGetChildControl += OnWheelSupportGetChildControl;

            SetFontAndColor(
                owner.Preferences.NormalFont, owner.Preferences.NormalFontColor,
                owner.Preferences.UnreadFont, owner.Preferences.UnreadFontColor,
                owner.Preferences.FlagFont, owner.Preferences.FlagFontColor,
                owner.Preferences.ErrorFont, owner.Preferences.ErrorFontColor,
                owner.Preferences.ReferrerFont, owner.Preferences.ReferrerFontColor,
                owner.Preferences.NewCommentsFont, owner.Preferences.NewCommentsFontColor
                );

            MaxHeadlineWidth = (int)(MaxHeadlineWidth * ScaleFactor);
            BrowserProgressBarWidth = (int)(BrowserProgressBarWidth * ScaleFactor);

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

        public float ScaleFactor => (float)DeviceDpi / 96;


        #region Windows 7 related initialization procedures 

        protected void InitWin7Components()
        {
            jumpList   = JumpList.CreateJumpList();
            jlcRecent = new JumpListCustomCategory(SR.JumpListRecentCategory);
            jumpList.AddCustomCategories(jlcRecent); 
            pictureBox = new PictureBox();                       
            
            //add tasks         
            jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddSubscriptionCaption)
            {
                IconReference = new IconReference(RssBanditApplication.GetAddFeedIconPath(), 0),
                Arguments     =  "http://www.example.com/feed.rss" 
            });
            jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddFacebookCaption)
            {
                IconReference = new IconReference(RssBanditApplication.GetAddFacebookIconPath(), 0),
                Arguments = "-f"
            });
			
            jumpList.AddUserTasks(new JumpListSeparator());
            jumpList.AddUserTasks(new JumpListLink(Resource.OutgoingLinks.ProjectNewsUrl, SR.JumpListGoToWebsiteCaption)
            {
                IconReference = new IconReference(Application.ExecutablePath, 0)
            }); 
            jumpList.Refresh();
            

            //
            //thumbnail toolbar button setup
            //
            buttonAdd = new ThumbnailToolBarButton(Properties.Resources.Add, SR.ThumbnailButtonAdd);
            buttonAdd.Enabled = true;
            buttonAdd.Click += OnTaskBarButtonAddClicked;

            buttonRefresh = new ThumbnailToolBarButton(Properties.Resources.RssFeedRefresh, SR.ThumbnailButtonRefresh);
            buttonRefresh.Enabled = true;
            buttonRefresh.Click += OnTaskBarButtonRefreshClick;

            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(this.Handle, buttonAdd, buttonRefresh);            
        }

        #endregion 

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
        	InitTooltipManager();
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

		private void InitTooltipManager()
		{
			ultraToolTipManager.ToolTipTextStyle = ToolTipTextStyle.Formatted;

			if (Win32.IsOSAtLeastWindowsVista)
			{
				ultraToolTipManager.DisplayStyle = Infragistics.Win.ToolTipDisplayStyle.WindowsVista;
			}
			else
			{
				ultraToolTipManager.DisplayStyle = Infragistics.Win.ToolTipDisplayStyle.Office2007;
				//ultraToolTipManager.TextRenderingMode = TextRenderingMode.GDI;
			}
		}

        private void InitOutlookNavigator()
        {
            navigatorHeaderHelper = new NavigatorHeaderHelper(Navigator, Properties.Resources.Arrows_Left_16.GetImageStretchedDpi(ScaleFactor));
            navigatorHeaderHelper.ImageClick += OnNavigatorCollapseClick;
            Navigator.GroupClick += OnNavigatorGroupClick;
            Navigator.SelectedGroupChanging += OnNavigatorSelectedGroupChanging;
			Navigator.SelectedGroupChanged += OnNavigatorSelectedGroupChanged;
           
            Navigator.ShowDefaultContextMenu = false;
            Navigator.ContextMenuStrip = new ContextMenuStrip();

            var subSourceDelete = new AppContextMenuCommand("cmdDeleteFeedSource",
                                                  owner.Mediator, CmdDeleteFeedSource,
                                                  SR.MenuDeleteFeedSourceCaption, SR.MenuDeleteFeedSourceDesc,
                                                  _shortcutHandler);
            var subSourceProperties = new AppContextMenuCommand("cmdFeedSourceProperties",
                                                  owner.Mediator, CmdShowFeedSourceProperties,
                                                  SR.MenuShowFeedSourceProperties, SR.MenuShowFeedSourcePropertiesDesc,
                                                  _shortcutHandler);

            Navigator.ContextMenuStrip.Items.AddRange(new[]{subSourceDelete, subSourceProperties});

			Navigator.Groups[Resource.NavigatorGroup.RssSearch].Settings.AppearancesSmall.HeaderAppearance.Image =
				Properties.Resources.feedsource_search_16.GetImageStretchedDpi(ScaleFactor);
			Navigator.Groups[Resource.NavigatorGroup.RssSearch].Settings.AppearancesLarge.HeaderAppearance.Image =
		        Properties.Resources.feedsource_search_32.GetImageStretchedDpi(ScaleFactor);
	
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
            listFeedItemsO.DrawFilter = new ListFeedsDrawFilter(listFeedItemsO);
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

            // Apply scale factor
            var sz = (int)((float)DeviceDpi / 96 * 16);
            detailHeaderCaption.ImageSize = new Size(sz, sz);
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
	        navigatorHiddenCaption.Appearance.Image = Properties.Resources.Arrows_Right_16.GetImageStretchedDpi(ScaleFactor);
			navigatorHiddenCaption.Appearance.ImageHAlign = HAlign.Center;
			navigatorHiddenCaption.Appearance.ImageVAlign = VAlign.Top;
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
            owner.FeedSources.SearchFinished += OnNewsItemSearchFinished;

            searchPanel.BeforeNewsItemSearch += OnSearchPanelBeforeNewsItemSearch;
            searchPanel.NewsItemSearch += OnSearchPanelStartNewsItemSearch;

            owner.FindersSearchRoot.SetScopeResolveCallback(ScopeResolve);
        }

        private void InitToaster()
        {
            toastNotifier = new ToastNotifier(
				owner.Preferences,
				this.ultraDesktopAlert/*,
                OnExternalActivateFeedItem,
                OnExternalDisplayFeedProperties,
                OnExternalActivateFeed,
                PlayEnclosure*/);
			toastNotifier.NotificationAction += OnToastNotificationAction;
        }

		
        private void InitListView()
        {
            colTopic.Text = SR.ListviewColumnCaptionTopic;
            colHeadline.Text = SR.ListviewColumnCaptionHeadline;
            colDate.Text = SR.ListviewColumnCaptionDate;

            listFeedItems.SmallImageList = _listImages;
            listFeedItemsO.ImageList = _listImages;
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

        private void ResetHtmlDetail(bool initializeControlUsage)
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
            htmlDetail.SilentModeEnabled = false;
#else
			this.htmlDetail.SilentModeEnabled = true;
#endif

            htmlDetail.Tag = _docFeedDetails;

			if (initializeControlUsage)
            {
				AttachEvents(htmlDetail, false);
                htmlDetail.Clear();
            }
        }

		// MUST attach the same events that are detached in DetachEvents(...) !
		private void AttachEvents(HtmlControl hc, bool isClosableWindow)
		{
			hc.StatusTextChanged += OnWebStatusTextChanged;
			hc.BeforeNavigate += OnWebBeforeNavigate;
			hc.NavigateComplete += OnWebNavigateComplete;
			hc.DocumentComplete += OnWebDocumentComplete;
			
			hc.NewWindow += OnWebNewWindow;

			if (Win32.IEVersion.Major >= 7)
			{
				hc.NewWindow3 += OnWebNewWindow3;
				HtmlControl.SetInternetFeatureEnabled(
					InternetFeatureList.FEATURE_TABBED_BROWSING,
					SetFeatureFlag.SET_FEATURE_ON_PROCESS, true);
			}

			hc.ProgressChanged += OnWebProgressChanged;
			hc.TranslateAccelerator += OnWebTranslateAccelerator;

			if (isClosableWindow)
			{
				hc.OnQuit += OnWebQuit;
				hc.TitleChanged += OnWebTitleChanged;
				hc.CommandStateChanged += OnWebCommandStateChanged;
			}
		}

		// MUST detach the same events that are attached in AttachEvents(...) !
		private void DetachEvents(HtmlControl hc, bool isClosableWindow)
		{
			hc.StatusTextChanged -= OnWebStatusTextChanged;
			hc.BeforeNavigate -= OnWebBeforeNavigate;
			hc.NavigateComplete -= OnWebNavigateComplete;
			hc.DocumentComplete -= OnWebDocumentComplete;
			
			hc.NewWindow -= OnWebNewWindow;

			if (Win32.IEVersion.Major >= 7)
			{
				hc.NewWindow3 -= OnWebNewWindow3;
			}

			hc.ProgressChanged -= OnWebProgressChanged;
			hc.TranslateAccelerator -= OnWebTranslateAccelerator;

			if (isClosableWindow)
			{
				hc.OnQuit -= OnWebQuit;
				hc.TitleChanged -= OnWebTitleChanged;
				hc.CommandStateChanged -= OnWebCommandStateChanged;
			}
		}

        Image GetTreeImage(string name)
        {
            var str = typeof(UltraTree).Assembly.GetManifestResourceStream(typeof(UltraTree), $"Images.{name}");
            var img = Image.FromStream(str).GetImageStretchedDpi(ScaleFactor);
            return img;
        }
    
        private void InitFeedTreeView()
        {
			if (Win32.IsOSAtLeastWindowsVista)
			{
				treeFeeds.DisplayStyle = UltraTreeDisplayStyle.WindowsVista;
			} 
			else
			{
				treeFeeds.TextRenderingMode = TextRenderingMode.GDI;
			}

        	// enable extended info tips support:
			treeNodesTooltipHelper = new UltraToolTipContextHelperForTreeNodes(treeFeeds, ultraToolTipManager);
        	
			treeFeeds.PathSeparator = FeedSource.CategorySeparator;
            treeFeeds.ImageList = _treeImages;
            
            treeFeeds.ExpansionIndicatorImageCollapsed = GetTreeImage("WindowsVistaExpansionIndicatorCollapsed.png");
            treeFeeds.ExpansionIndicatorImageCollapsedHotTracked = GetTreeImage("WindowsVistaExpansionIndicatorCollapsedHotTracked.png");
            treeFeeds.ExpansionIndicatorImageExpanded = GetTreeImage("WindowsVistaExpansionIndicatorExpanded.png");
            treeFeeds.ExpansionIndicatorImageExpandedHotTracked = GetTreeImage("WindowsVistaExpansionIndicatorExpandedHotTracked.png");
            treeFeeds.ExpansionIndicatorPadding = -(int)(1 * ScaleFactor);

            treeFeeds.ScrollBounds = ScrollBounds.ScrollToFill;

            //this.treeFeeds.CreationFilter = new TreeFeedsNodeUIElementCreationFilter();
            treeFeeds.Override.SelectionType = SelectType.SingleAutoDrag;
        	treeFeeds.AllowDrop = true;
        	//treeFeeds.ShowRootLines = false;
			treeFeeds.ShowLines = false;

			// we could also have impl. sorting by using a IComparable impl. at TreeFeedsNodeBase
			// and it's inherited classes. But it is easier to handle the various different
			// types at one place:
			treeFeeds.Override.SortComparer = new TreeNodesSortHelper(System.Windows.Forms.SortOrder.Ascending);
        	treeFeeds.Override.Sort = SortType.Ascending;
			
			treeFeeds.HideSelection = false;
            treeFeeds.NodeLevelOverrides[0].HotTracking = DefaultableBoolean.False;
			// grow the expansion indicator clickable image:
			//treeFeeds.ExpansionIndicatorSize = new Size(13, 13);

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
#if !PHOENIX
			historyMenuManager.OnNavigateBack += OnHistoryNavigateGoBackItemClick;
			historyMenuManager.OnNavigateForward += OnHistoryNavigateGoForwardItemClick;
#else
            ultraToolbarsManager.AfterNavigation += OnToolbarAfterHistoryNavigation;
#endif
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
            ultraToolbarsManager.ImageSizeSmall = _allToolImages.ImageSize;
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

            _subscriptionTreeRootContextMenu = new ContextMenuStrip();

            var sep = new ToolStripMenuItem("-");

            var sub1 = new AppContextMenuCommand("cmdNewFeed",
                                                 owner.Mediator,
                                                 owner.CmdNewFeed,
                                                 SR.MenuNewFeedCaption2, SR.MenuNewFeedDesc, 1,
                                                 _shortcutHandler);

            //sub1.ImageList  = _toolImages;

            var sub2 = new AppContextMenuCommand("cmdNewCategory",
                                                 owner.Mediator,
                                                 owner.CmdNewCategory,
                                                 SR.MenuNewCategoryCaption, SR.MenuNewCategoryDesc, 2,
                                                 _shortcutHandler);

            //sub2.ImageList  = _treeImages;

            var subR1 = new AppContextMenuCommand("cmdRefreshFeeds",
                                                  owner.Mediator,
                                                  owner.CmdRefreshFeeds,
                                                  SR.MenuUpdateAllFeedsCaption,
                                                  SR.MenuUpdateAllFeedsDesc, 0, _shortcutHandler);

            //subR1.ImageList  = _toolImages;

            var subR2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
                                                  owner.Mediator,
                                                  owner.CmdCatchUpCurrentSelectedNode,
                                                  SR.MenuCatchUpOnAllCaption, SR.MenuCatchUpOnAllDesc,
                                                  0, _shortcutHandler);
            //subR2.ImageList           = _listImages;

            var subR3 = new AppContextMenuCommand("cmdDeleteAll",
                                                  owner.Mediator,
                                                  owner.CmdDeleteAll,
                                                  SR.MenuDeleteAllFeedsCaption,
                                                  SR.MenuDeleteAllFeedsDesc, 2, _shortcutHandler);
            //subR3.ImageList           = _toolImages;

			//var subR4 = new AppContextMenuCommand("cmdShowMainAppOptions",
			//                                      owner.Mediator,
			//                                      new ExecuteCommandHandler(owner.CmdShowOptions),
			//                                      SR.MenuAppOptionsCaption, SR.MenuAppOptionsDesc, 10,
			//                                      _shortcutHandler);

			var subSourceRename = new AppContextMenuCommand("cmdRenameFeedSource",
												  owner.Mediator,
												  CmdRenameFeedSource,
												  SR.MenuRenameFeedSourceCaption, SR.MenuRenameFeedSourceDesc,
												  _shortcutHandler);
			var subSourceDelete = new AppContextMenuCommand("cmdDeleteFeedSource",
												  owner.Mediator,
												  CmdDeleteFeedSource,
												  SR.MenuDeleteFeedSourceCaption, SR.MenuDeleteFeedSourceDesc,
												  _shortcutHandler);

			var subSourceProperties = new AppContextMenuCommand("cmdFeedSourceProperties",
												  owner.Mediator,
												  CmdShowFeedSourceProperties,
												  SR.MenuShowFeedSourceProperties, SR.MenuShowFeedSourcePropertiesDesc,
												  _shortcutHandler);

            // append items
            _subscriptionTreeRootContextMenu.Items.AddRange(
				new[] { sub1, sub2, sep, subR1, subR2, sep.CloneMenu(), subR3, sep.CloneMenu(), subSourceRename, subSourceDelete, 
					sep.CloneMenu(), subSourceProperties });

            #endregion

            #region category menu

            _treeCategoryContextMenu = new ContextMenuStrip();

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

                subCL_ColLayoutMain.DropDownItems.AddRange(new ToolStripMenuItem[] {subCL4_layoutSubColumn});
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

            subCL_ColLayoutMain.DropDownItems.AddRange(
                new[]
                    {
                        sep.CloneMenu(), subCL_subUseCatLayout, subCL_subUseFeedLayout, sep.CloneMenu(),
                        subCL_subResetLayout
                    });

            // append items. Reuse cmdNewCat/cmdNewFeed, because it's allowed on categories
            _treeCategoryContextMenu.Items.AddRange(
                new[]
                    {
                        sub1.CloneMenu(), sub2.CloneMenu(), sep.CloneMenu(), subC1, subC2, sep.CloneMenu(), subC3,
                        sep.CloneMenu(), subC4, sep.CloneMenu(), subCL_ColLayoutMain, sep.CloneMenu(), subC5
                    });

            #endregion

            #region feed menu

            _treeFeedContextMenu = new ContextMenuStrip();

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

            subFeedCopy.DropDownItems.AddRange(new ToolStripMenuItem[] {subFeedCopy_sub1, subFeedCopy_sub2, subFeedCopy_sub3});


            _feedInfoContextMenu = new ToolStripMenuItem(SR.MenuAdvancedFeedInfoCaption);

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

                subFL_ColLayoutMain.DropDownItems.AddRange(new ToolStripMenuItem[] {subFL4_layoutSubColumn});
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

            subFL_ColLayoutMain.DropDownItems.AddRange(
                new[]
                    {
                        sep.CloneMenu(), subFL_subUseCatLayout, subFL_subUseFeedLayout, sep.CloneMenu(),
                        subFL_subResetLayout
                    });

            // append items. 
            _treeFeedContextMenu.Items.AddRange(
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

            _feedInfoContextMenu.DropDownItems.AddRange(
                new ToolStripMenuItem[] {subInfoHome, subInfoCosmos, subInfoSource, subInfoValidate});

            #endregion

            #region root search folder context menu

            _treeSearchFolderRootContextMenu = new ContextMenuStrip();

            subF1 = new AppContextMenuCommand("cmdNewFinder",
                                              owner.Mediator, new ExecuteCommandHandler(CmdNewFinder),
                                              SR.MenuNewFinderCaption, SR.MenuNewFinderDesc, _shortcutHandler);
            subF2 = new AppContextMenuCommand("cmdDeleteAllFinders",
                                              owner.Mediator, new ExecuteCommandHandler(CmdDeleteAllFinder),
                                              SR.MenuFinderDeleteAllCaption, SR.MenuFinderDeleteAllDesc,
                                              _shortcutHandler);

            _treeSearchFolderRootContextMenu.Items.AddRange(new[] {subF1, sep.CloneMenu(), subF2});

            #endregion

            #region search folder context menu's

            _treeSearchFolderContextMenu = new ContextMenuStrip();

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

            _treeSearchFolderContextMenu.Items.AddRange(
                new[]
                    {
                        subF1, subF2, subF3, sep.CloneMenu(), subF4, sep.CloneMenu(), subFinderShowFullText,
                        sep.CloneMenu(), subF6
                    });


            _treeTempSearchFolderContextMenu = new ContextMenuStrip();

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

            _treeTempSearchFolderContextMenu.Items.AddRange(
                new[]
                    {
                        subF1, subF2, sep.CloneMenu(), subF3, sep.CloneMenu(), subFinderShowFullText.CloneMenu(),
                        sep.CloneMenu(), subF4
                    });

            #endregion

            treeFeeds.ContextMenuStrip = _subscriptionTreeRootContextMenu; // init to root context

            #endregion

            #region list view context menu

            _listContextMenu = new ContextMenuStrip();

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

            subL3.DropDownItems.AddRange(
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

            subL10.DropDownItems.AddRange(new ToolStripMenuItem[] {subL10_sub1, subL10_sub2, subL10_sub3});


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

                subL4.DropDownItems.AddRange(new ToolStripMenuItem[] {subL4_subColumn});
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

            subL4.DropDownItems.AddRange(
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
            _listContextMenu.Items.AddRange(
                new[]
                    {
                        subL2, subL3, subL0, subL1, subL7, sep.CloneMenu(), subL10,
                        _listContextMenuDownloadAttachmentsSeparator, subL9, _listContextMenuDeleteItemsSeparator, subL5
                        ,
                        subL6, sep.CloneMenu(), subL4, subL8
                    });
            listFeedItems.ContextMenuStrip = _listContextMenu;
            listFeedItemsO.ContextMenuStrip = _listContextMenu;

            #endregion

            #region Local Feeds context menu

            _treeLocalFeedContextMenu = new ContextMenuStrip();

            var subTL1 = new AppContextMenuCommand("cmdDeleteAllNewsItems",
                                                   owner.Mediator,
                                                   owner.CmdDeleteAllFeedItems,
                                                   SR.MenuDeleteAllFeedItemsCaption,
                                                   SR.MenuDeleteAllFeedItemsDesc, 1, _shortcutHandler);

			var subTL2 = new AppContextMenuCommand("cmdCatchUpCurrentSelectedNode",
												  owner.Mediator,
												  owner.CmdCatchUpCurrentSelectedNode,
												  SR.MenuCatchUpThisFeedCaption,
												  SR.MenuCatchUpThisFeedDesc, 0, _shortcutHandler);
            //subTL1.ImageList           = _listImages;

            _treeLocalFeedContextMenu.Items.AddRange(new [] {subTL2, sep.CloneMenu(), subTL1});

            #endregion

            #region doc tab context menu

            _docTabContextMenu = new ContextMenuStrip();

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


            subDT5.DropDownItems.AddRange(new ToolStripMenuItem[] {subSub1, subSub2, subSub3, subSub4});


            _docTabContextMenu.Items.AddRange(
                new[] {subDT1, subDT2, subDT3, sep.CloneMenu(), subDT4, sep.CloneMenu(), subDT5});

            #endregion

            #region tray context menu

            _notifyContextMenu = new ContextMenuStrip();

            var subT1 = new AppContextMenuCommand("cmdShowGUI",
                                                  owner.Mediator,
                                                  new ExecuteCommandHandler(owner.CmdShowMainGui),
                                                  SR.MenuShowMainGuiCaption, SR.MenuShowMainGuiDesc,
                                                  _shortcutHandler)
                            {
                               
                            };
            subT1.Select();

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

            subT5.DropDownItems.AddRange(new ToolStripMenuItem[] {subT5_1, subT5_2, subT5_3});

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

            _notifyContextMenu.Items.AddRange(
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
				if (File.Exists(settingsPath))
				{
					_shortcutHandler.Load(settingsPath);
					return;
				}
			}
			catch (InvalidShortcutSettingsFileException e)
			{
				_log.Warn("The user defined shortcut settings file is invalid. Using the default instead.", e);
			}
			catch (IOException e)
			{
				_log.Error("File access error on user defined shortcut settings file. Using the default instead.", e);
			}

	        try
	        {
		        using (Stream settingsStream = Resource.GetStream("Resources.ShortcutSettings.xml"))
		        {
			        _shortcutHandler.Load(settingsStream);
					_shortcutHandler.Write(settingsPath);
		        }
	        }
	        catch (InvalidShortcutSettingsFileException e)
	        {
		        _log.Warn("Failed to write user defined shortcut settings file. Using the default instead.", e);
	        }
	        catch (IOException e)
	        {
		        _log.Error("File access error on write user defined shortcut settings file. Using the default instead.", e);
	        }
        }


        protected void InitResources()
        {
            // For now just use XP icons until we can update the overlays on the vista one
            //// Create a strip of images by loading an embedded bitmap resource
            //if(Win32.IsOSAtLeastWindowsVista)
            //    _treeImages = Resource.LoadBitmapStrip("Resources.TreeImages.png", new Size(16, 16));
            //else


            var size = (int)(16 * ScaleFactor);
            var sz = new Size(size, size);

            // hack, resize the image array by the scale factor
            var treeImgBmp = Resource.LoadBitmap("Resources.TreeImages.png").GetImageStretchedDpi(ScaleFactor);
            var listImgBmp = Resource.LoadBitmap("Resources.ListImages.png").GetImageStretchedDpi(ScaleFactor);
            var allToolImgBmp = Resource.LoadBitmap("Resources.AllToolImages.png").GetImageStretchedDpi(ScaleFactor);

            //  _treeImages = Resource.LoadBitmapStrip("Resources.TreeImages.png", sz);
            _treeImages = Resource.LoadBitmapStrip(treeImgBmp, sz);
            _listImages = Resource.LoadBitmapStrip(listImgBmp, sz);
            _allToolImages = Resource.LoadBitmapStrip(allToolImgBmp, sz);

            //_listImages = Resource.LoadBitmapStrip("Resources.ListImages.png", new Size(16, 16));
            //_allToolImages = Resource.LoadBitmapStrip("Resources.AllToolImages.png", sz);
        }

  

        #region Init DocManager

        private void InitDocumentManager()
        {
            _docContainer.SuspendLayout();

            _docContainer.LayoutSystem.SplitMode = Orientation.Vertical;

            //_docFeedDetails.TabImage = _listImages.Images[0];
            // This is here because TabImage will throw if it's not 16x16
            typeof(DockControl).GetField("e", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_docFeedDetails, _listImages.Images[0], BindingFlags.SetField, null, null);
            var b = typeof(DockControl).GetField("b", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_docFeedDetails) as ControlLayoutSystem;
            if (b != null)
            {
                typeof(ControlLayoutSystem).GetMethod("g", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(b, null);
            }
            
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
            _trayManager = new TrayStateManager(components);
			_trayManager.TrayIconClicked += OnTrayIconClick;
			_trayManager.TrayBalloonClickClicked += OnTrayIconClick;
			_trayManager.TrayBalloonTimeout += OnTrayBalloonTimeoutClose;
			_trayManager.IconContextMenu = _notifyContextMenu;
        }

        #endregion
    }
}
