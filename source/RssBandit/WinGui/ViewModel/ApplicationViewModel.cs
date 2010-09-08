#region Version Info Header
/*
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using log4net;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.AppServices.Core;
using RssBandit.Common.Logging;
using RssBandit.WinGui.Commands;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Forms;


namespace RssBandit.WinGui.ViewModel
{
    public partial class ApplicationViewModel : ModelBase
    {
        private readonly RssBanditApplication _current;
        private static readonly ILog Log = DefaultLog.GetLogger(typeof (ApplicationViewModel));
        private RelayCommand _addFacebookFeedSourceCommand;
        private RelayCommand _addFeedSourcesCommand;
        private RelayCommand _addGoogleReaderFeedSourceCommand;
        private RelayCommand _addWindowsCommonFeedSourceCommand;

        /// <summary>
        ///   if a user explicitly close the balloon, we are silent for the next 12 retries (we refresh all 5 minutes, so this equals at least to one hour)
        /// </summary>
        private int _beSilentOnBalloonPopupCounter;

        private RelayCommand _catchUpAllFeedsCommand;
        private RelayCommand _catchUpFeedsInFolderCommand;

        private RelayCommand _closeCommand;

        private RelayCommand _exportFeedsCommand;
        private RelayCommand _importFeedsCommand;

        ///<summary>
        ///  this here because we only want to display the balloon popup, if there are really new items received:
        ///</summary>
        private int _lastUnreadFeedItemCountBeforeRefresh;

        private RelayCommand _subscribeNntpFeedCommand;
        private RelayCommand _subscribeRssFeedCommand;

        private RelayCommand _subscribeSearchResultFeedCommand;

        /// <summary>
        ///   Timer used for refreshing feeds
        /// </summary>
        private readonly DispatcherTimer _timerRefreshFeeds = new DispatcherTimer();


        /// <summary>
        ///   Timer used for performing background UI tasks
        /// </summary>
        private readonly UITaskTimer _uiTasksTimer = new UITaskTimer();

        private RelayCommand _updateAllFeedsCommand;

        private RelayCommand _updateFeedsInFolderCommand;

        protected ApplicationViewModel(RssBanditApplication current)
        {
            _current = current;
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to add the facebook feedsource.
        /// </summary>
        public ICommand AddFacebookFeedSourceCommand
        {
            get
            {
                if (_addFacebookFeedSourceCommand == null)
                    _addFacebookFeedSourceCommand = new RelayCommand(param => AddFacebookFeedSource(), param => CanAddFacebookFeedSource);

                return _addFacebookFeedSourceCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to add one of the supported feedsource(s).
        /// </summary>
        public ICommand AddFeedSourcesCommand
        {
            get
            {
                if (_addFeedSourcesCommand == null)
                    _addFeedSourcesCommand = new RelayCommand(param => AddFeedSources(), param => CanAddFeedSources);

                return _addFeedSourcesCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to add the google reader feedsource.
        /// </summary>
        public ICommand AddGoogleReaderFeedSourceCommand
        {
            get
            {
                if (_addGoogleReaderFeedSourceCommand == null)
                    _addGoogleReaderFeedSourceCommand = new RelayCommand(param => AddGoogleReaderFeedSource(), param => CanAddGoogleReaderFeedSource);

                return _addGoogleReaderFeedSourceCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to add the windows common feedlist feedsource.
        /// </summary>
        public ICommand AddWindowsCommonFeedSourceCommand
        {
            get
            {
                if (_addWindowsCommonFeedSourceCommand == null)
                    _addWindowsCommonFeedSourceCommand = new RelayCommand(param => AddWindowsCommonFeedSource(), param => CanAddWindowsCommonFeedSource);

                return _addWindowsCommonFeedSourceCommand;
            }
        }

        public bool CanAddFacebookFeedSource
        {
            get { return true; }
        }

        public bool CanAddFeedSources
        {
            get { return true; }
        }

        public bool CanAddGoogleReaderFeedSource
        {
            get { return true; }
        }

        public bool CanAddWindowsCommonFeedSource
        {
            get { return true; }
        }

        public bool CanCatchUpAllFeeds
        {
            get { return true; }
        }

        public bool CanCatchUpFeedsInFolder
        {
            get { return true; }
        }

        public bool CanExportFeeds
        {
            get { return true; }
        }

        public bool CanImportFeeds
        {
            get { return true; }
        }

        public bool CanSubscribeNntpFeed
        {
            get { return true; }
        }

        public bool CanSubscribeRssFeed
        {
            get { return true; }
        }

        public bool CanSubscribeSearchResultFeed
        {
            get { return true; }
        }

        public bool CanUpdateAllFeeds
        {
            get { return true; }
        }

        public bool CanUpdateFeedsInFolder
        {
            get { return true; }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to catch up all feeds (mark read).
        /// </summary>
        public ICommand CatchUpAllFeedsCommand
        {
            get
            {
                if (_catchUpAllFeedsCommand == null)
                    _catchUpAllFeedsCommand = new RelayCommand(param => CatchUpAllFeeds(), param => CanCatchUpAllFeeds);

                return _catchUpAllFeedsCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to catch up all feeds (mark read).
        /// </summary>
        public ICommand CatchUpFeedsInFolderCommand
        {
            get
            {
                if (_catchUpFeedsInFolderCommand == null)
                    _catchUpFeedsInFolderCommand = new RelayCommand(param => CatchUpFeedsInFolder(), param => CanCatchUpFeedsInFolder);

                return _catchUpFeedsInFolderCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to shutdown the application.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(param => OnRequestClose());

                return _closeCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to export feeds to a file .
        /// </summary>
        public ICommand ExportFeedsCommand
        {
            get
            {
                if (_exportFeedsCommand == null)
                    _exportFeedsCommand = new RelayCommand(param => ExportFeeds(), param => CanExportFeeds);

                return _exportFeedsCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to import feeds from a URL (OPML, own format).
        /// </summary>
        public ICommand ImportFeedsCommand
        {
            get
            {
                if (_importFeedsCommand == null)
                    _importFeedsCommand = new RelayCommand(param => ImportFeeds(), param => CanImportFeeds);

                return _importFeedsCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to create a new NNTP Feed subscription.
        /// </summary>
        public ICommand SubscribeNntpFeedCommand
        {
            get
            {
                if (_subscribeNntpFeedCommand == null)
                    _subscribeNntpFeedCommand = new RelayCommand(param => SubscribeNntpFeed(), param => CanSubscribeNntpFeed);

                return _subscribeNntpFeedCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to create a new RSS Feed subscription.
        /// </summary>
        public ICommand SubscribeRssFeedCommand
        {
            get
            {
                if (_subscribeRssFeedCommand == null)
                    _subscribeRssFeedCommand = new RelayCommand(param => SubscribeRssFeed(), param => CanSubscribeRssFeed);

                return _subscribeRssFeedCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to create a new search result Feed subscription.
        /// </summary>
        public ICommand SubscribeSearchResultFeedCommand
        {
            get
            {
                if (_subscribeSearchResultFeedCommand == null)
                    _subscribeSearchResultFeedCommand = new RelayCommand(param => SubscribeSearchResultFeed(), param => CanSubscribeSearchResultFeed);

                return _subscribeSearchResultFeedCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to update all feeds.
        /// </summary>
        public ICommand UpdateAllFeedsCommand
        {
            get
            {
                if (_updateAllFeedsCommand == null)
                    _updateAllFeedsCommand = new RelayCommand(param => UpdateAllFeeds(), param => CanUpdateAllFeeds);

                return _updateAllFeedsCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to update all feeds in the selected folder.
        /// </summary>
        public ICommand UpdateFeedsInFolderCommand
        {
            get
            {
                if (_updateFeedsInFolderCommand == null)
                    _updateFeedsInFolderCommand = new RelayCommand(param => UpdateFeedsInFolder(), param => CanUpdateFeedsInFolder);

                return _updateFeedsInFolderCommand;
            }
        }
        private FeedSourcesViewModel _feedSources;
        /// <summary>
        ///   Gets or sets the feed sources (XAML bound).
        /// </summary>
        /// <value>The feed sources.</value>
        public FeedSourcesViewModel FeedSources
        {
            get
            {
                if (_feedSources == null)
                    _feedSources = new FeedSourcesViewModel(_current.FeedSources);
                return _feedSources;
            }
        }

        public bool WorkOffline
        {
            get { return FeedSource.Offline; }
            set { FeedSource.Offline = value; }
        }

        public CategorizedFeedSourceViewModel ViewModelOf(FeedSourceEntry entry)
        {
            return FeedSources.Sources.FirstOrDefault(treeItem => (treeItem != null) && treeItem.Name.Equals(entry.Name));
        }

        /// <summary>
        ///   Add a new feed to the GUI tree view
        /// </summary>
        /// <param name = "entry">The entry.</param>
        /// <param name = "category">Feed Category</param>
        /// <param name = "f">Feed</param>
        public void AddNewFeedNode(FeedSourceEntry entry, string category, INewsFeed f)
        {
            //find category node or create if it doesn't exist
            //CategorizedFeedSourceViewModel entryModel = ViewModelOf(entry);
            //FolderViewModel folder = entryModel.CreateHive(category);

            //var feed = new FeedViewModel(f, folder, entryModel);
            //var feed = new FeedViewModel(f, entryModel);
            //folder.Children.Add(feed);

            /*  
            if (RssHelper.IsNntpUrl(f.link))
            {
                tn = new RssBandit.WinGui.Controls.FeedNode(f.title, Resource.SubscriptionTreeImage.Nntp,
                                  Resource.SubscriptionTreeImage.NntpSelected,
                                  _treeFeedContextMenu);
            }
            else
            {
                tn = new FeedNode(f.title, Resource.SubscriptionTreeImage.Feed,
                                  Resource.SubscriptionTreeImage.FeedSelected,
                                  _treeFeedContextMenu,
                                  (owner.Preferences.UseFavicons ? LoadCachedFavicon(entry.Source, f) : null));
            }

            //interconnect for speed:
            tn.DataKey = f.link;
            f.Tag = tn;

            SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);

            SubscriptionRootNode root = GetSubscriptionRootNode(entry);
            category = (f.category == RssBanditApplication.DefaultCategory ? null : f.category);
            TreeFeedsNodeBase catnode = TreeHelper.CreateCategoryHive(root, category,
                                                                      _treeCategoryContextMenu);

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

            if (root.Visible)
                tn.BringIntoView();

            DelayTask(DelayedTasks.SyncRssSearchTree);
           */
        }

        /// <summary>
        ///   Returns the FeedSourceEntry where this feed is subscribed in.
        /// </summary>
        /// <param name = "feedUrl">The specified feed</param>
        /// <returns>The FeedSourceEntry where the feed is subscribed</returns>
        public FeedSourceEntry FeedSourceEntryOf(string feedUrl)
        {
            if (StringHelper.EmptyTrimOrNull(feedUrl))
                return null;

            return RssBanditApplication.Current.FeedSources.Sources.FirstOrDefault(fse => fse.Source.IsSubscribed(feedUrl));
        }

        /// <summary>
        ///   Returns the FeedSource where this feed is subscribed in.
        /// </summary>
        /// <param name = "feedUrl">The specified feed</param>
        /// <returns>The FeedSource where the feed is subscribed</returns>
        public FeedSource FeedSourceOf(string feedUrl)
        {
            FeedSourceEntry entry = FeedSourceEntryOf(feedUrl);
            if (entry != null)
                return entry.Source;
            return null;
        }

        /// <summary>
        ///   Starts the process of subscribing to the specified RSS feed via the new subscription wizard
        /// </summary>
        /// <param name = "feedUrl">The URL of the feed to subscribe to</param>
        public void SubscribeRssFeed(string feedUrl)
        {
            //handle occasions where URL begins with feed: protocol 
            feedUrl = RssLocater.UrlFromFeedProtocolUrl(feedUrl);
            SubscribeToFeed(feedUrl, null, String.Empty, AddSubscriptionWizardMode.SubscribeURLDirect);
        }

        /// <summary>
        ///   Initiate a async. call to FeedSource.RefreshFeeds(force_download)
        /// </summary>
        /// <param name = "force_download"></param>
        public void UpdateAllFeeds(bool force_download)
        {
            //var rootNodes = new List<CategorizedFeedSourceViewModel>();

            //foreach (var treeItem in RssBanditApplication.MainWindow.tree.Items)
            //{
            //    var cfsvm = treeItem as CategorizedFeedSourceViewModel;
            //    if (cfsvm != null)
            //    {
            //        rootNodes.Add(cfsvm);
            //    }
            //}

            //if (rootNodes.Count != 0)
            //{
            //    if (_timerRefreshFeeds.IsEnabled)
            //        _timerRefreshFeeds.Stop();
            //    _lastUnreadFeedItemCountBeforeRefresh = rootNodes.Sum(n => n.UnreadCount);
            //    RssBanditApplication.Current.BeginRefreshFeeds(force_download);
            //}

            RssBanditApplication.Current.BeginRefreshFeeds(force_download);
        }

        internal void DelayTask(DelayedTasks task)
        {
            DelayTask(task, null, new TimeSpan(0, 0, 0, 0, 100));
        }

        internal void DelayTask(DelayedTasks task, object data)
        {
            DelayTask(task, data, new TimeSpan(0, 0, 0, 0, 100));
        }

        internal void DelayTask(DelayedTasks task, object data, TimeSpan interval)
        {
            _uiTasksTimer.SetData(task, data);
            if (_uiTasksTimer.Interval != interval)
                _uiTasksTimer.Interval = interval;
            _uiTasksTimer.StartTask(task);
        }

        /// <summary>
        ///   Initializes main UI components and data structures
        /// </summary>
        internal void Init()
        {
            //Dispatcher = RssBanditApplication.MainWindow.Dispatcher; 

            //support for Windows 7 taskbar features
            //if (TaskbarManager.IsPlatformSupported)
            //{
            //    InitWin7Components();
            //}

            //start UI tasks timer
            _uiTasksTimer.Tick += OnTasksTimerTick;

            //initialize timers
            _timerRefreshFeeds.Interval = TimeSpan.FromMilliseconds(600000);
            _timerRefreshFeeds.Tick += OnTimerFeedsRefreshElapsed;
        }

        private object _selectedTreeNodeItem;
        public object SelectedTreeNodeItem
        {
            get { return _selectedTreeNodeItem; }
            set
            {
                _selectedTreeNodeItem = value;

                OnPropertyChanged(() => SelectedTreeNodeItem);
            }
        }

        /// <summary>
        ///   Returns the name of the feed source of the currently selected item in the tree view. Returns null if no item in the tree view is selected
        /// </summary>
        /// <returns></returns>
        private void GetSelectedFeedSourceAndCategoryNames(out string feedSource, out string category)
        {
            feedSource = category = null;

            if (SelectedTreeNodeItem != null)
            {
                if (SelectedTreeNodeItem is FeedViewModel)
                {
                    category = ((FeedViewModel)SelectedTreeNodeItem).Category;
                    feedSource = ((FeedViewModel)SelectedTreeNodeItem).Source.Name;
                }
                else if (SelectedTreeNodeItem is FolderViewModel)
                {
                    category = ((FolderViewModel)SelectedTreeNodeItem).Category;
                    feedSource = ((FolderViewModel)SelectedTreeNodeItem).Source.Name;
                }
                else if (SelectedTreeNodeItem is CategorizedFeedSourceViewModel)
                {
                    feedSource = ((CategorizedFeedSourceViewModel)SelectedTreeNodeItem).Name;
                }
            }
        }

        private void AddFacebookFeedSource()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        private void AddFeedSources()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        private void AddGoogleReaderFeedSource()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        private void AddWindowsCommonFeedSource()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        private void CatchUpAllFeeds()
        {
            //TODO:
            MessageBox.Show("Not yet connected...");
        }

        private void CatchUpFeedsInFolder()
        {
            //TODO:
            MessageBox.Show("Not yet connected...");
        }

        private void ExportFeeds()
        {
            //TODO:
            //RssBanditApplication.Current.CmdExportFeeds(...);
            MessageBox.Show("Not yet connected...");
        }

        private void ImportFeeds()
        {
            string category = null, feedSource = null;
            GetSelectedFeedSourceAndCategoryNames(out feedSource, out category);

            RssBanditApplication.Current.ImportFeeds(String.Empty, category, feedSource);
        }

        private void OnRequestClose()
        {
            EventHandler handler = RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///   Callback for UITasksTimer
        /// </summary>
        private void OnTasksTimerTick(object sender, EventArgs e)
        {
            if (_uiTasksTimer[DelayedTasks.StartRefreshOneFeed])
            {
                _uiTasksTimer.StopTask(DelayedTasks.StartRefreshOneFeed);
                var feedUrl = (string) _uiTasksTimer.GetData(DelayedTasks.StartRefreshOneFeed, true);
                FeedSource source = FeedSourceOf(feedUrl);
                source.AsyncGetItemsForFeed(feedUrl, true, true);
            }

            /* 
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

             */

            if (!_uiTasksTimer.AllTaskDone)
            {
                if (!_uiTasksTimer.IsEnabled)
                    _uiTasksTimer.Start();
            }
            else
            {
                if (_uiTasksTimer.IsEnabled)
                    _uiTasksTimer.Stop();
            }
        }

        /// <summary>
        ///   Callback called refresh feeds timer  c
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        private void OnTimerFeedsRefreshElapsed(object sender, EventArgs e)
        {
            if (RssBanditApplication.Current.InternetAccessAllowed && RssBanditApplication.Current.CurrentGlobalRefreshRateMinutes > 0)
            {
                UpdateAllFeeds(false);
            }
        }

        /// <summary>
        ///   Starts the process of subscribing to an NNTP feed via the new subscription wizard
        /// </summary>
        private void SubscribeNntpFeed()
        {
            SubscribeToFeed(null, null, String.Empty, AddSubscriptionWizardMode.SubscribeNNTPDirect);
        }

        /// <summary>
        ///   Starts the process of subscribing to an RSS feed via the new subscription wizard
        /// </summary>
        private void SubscribeRssFeed()
        {
            SubscribeToFeed(null, null, String.Empty, AddSubscriptionWizardMode.SubscribeURLDirect);
        }

        private void SubscribeSearchResultFeed()
        {
            SubscribeToFeed(null, null, String.Empty, AddSubscriptionWizardMode.SubscribeSearchDirect);
        }


        /// <summary>
        ///   Launches the subscription wizard with the specified wizard mode
        /// </summary>
        /// <param name = "url">The feed URL to subscribe to</param>
        /// <param name = "title">The title of the feed</param>
        /// <param name = "searchTerms">The search terms. Used for search results feeds.</param>
        /// <param name = "mode">The mode the subscription wizard should start in</param>
        /// <returns>true if any feeds were successfulyy subscribed to</returns>
        private bool SubscribeToFeed(string url, string title, string searchTerms,
                                     AddSubscriptionWizardMode mode)
        {
            return SubscribeToFeed(url, null, title, searchTerms, mode);
        }


        /// <summary>
        ///   Launches the subscription wizard with the specified wizard mode
        /// </summary>
        /// <param name = "url">The feed URL to subscribe to</param>
        /// <param name = "category">The category of the feed node.</param>
        /// <param name = "title">The title of the feed</param>
        /// <param name = "searchTerms">The search terms. Used for search results feeds.</param>
        /// <param name = "mode">The mode the subscription wizard should start in</param>
        /// <returns>true if any feeds were successfulyy subscribed to</returns>
        private bool SubscribeToFeed(string url, string category, string title, string searchTerms,
                                     AddSubscriptionWizardMode mode)
        {
            using (var wiz = new AddSubscriptionWizard(mode)
                                 {
                                     FeedUrl = (url ?? String.Empty),
                                     FeedTitle = (title ?? String.Empty),
                                     SearchTerms = (searchTerms ?? String.Empty)
                                 })
            {
                string feedSourceName, currentCategory;

                /* if (category != null) // does remember the last category:
                    wiz.FeedCategory = category; */

                GetSelectedFeedSourceAndCategoryNames(out feedSourceName, out currentCategory);
                wiz.FeedSourceName = feedSourceName;
                wiz.FeedCategory = (category ?? currentCategory);

                try
                {
                    wiz.ShowDialog();
                }
                catch (Exception ex)
                {
                    Log.Error("SubscribeToFeed caused exception.", ex);
                    wiz.DialogResult = DialogResult.Cancel;
                }

                if (wiz.DialogResult == DialogResult.OK)
                {
                    RssBanditApplication bandit = RssBanditApplication.Current;
                    INewsFeed f;
                    FeedSourceEntry entry = bandit.FeedSources[wiz.FeedSourceName];

                    if (wiz.MultipleFeedsToSubscribe)
                    {
                        bool anySubscription = false;

                        for (int i = 0; i < wiz.MultipleFeedsToSubscribeCount; i++)
                        {
                            f = bandit.CreateFeedFromWizard(wiz, entry, i);
                            if (f == null)
                            {
                                continue;
                            }

                            AddNewFeedNode(entry, f.category, f);

                            /* 
                            if (wiz.FeedInfo == null)
                                guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

                             */
                            anySubscription = true;
                        }

                        return anySubscription;
                    }

                    f = bandit.CreateFeedFromWizard(wiz, entry, 0);

                    if (f == null)
                    {
                        return false;
                    }


                    // add feed visually
                    AddNewFeedNode(entry, f.category, f);

                    if (wiz.FeedInfo == null)
                        DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

                    return true;
                } // if (wiz.DialogResult == DialogResult.OK)
            } // using (var wiz = new AddSubscriptionWizard(mode)

            return false;
        }

        /// <summary>
        ///   Initiate a async. call to FeedSource.RefreshFeeds(force_download)
        /// </summary>
        private void UpdateAllFeeds()
        {
            UpdateAllFeeds(false);
        }

        private void UpdateFeedsInFolder()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        /// <summary>
        ///   Raised when this app should be closed at all.
        /// </summary>
        public event EventHandler RequestClose;
    }
}