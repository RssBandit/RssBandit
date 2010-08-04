#region Version Info Header
/*
 */
#endregion

using System;
using System.Windows.Input;
using NewsComponents;
using RssBandit.WinGui.Commands;
using RssBandit.WinGui.Dialogs;
using NewsComponents.Feed;
using System.Windows.Forms;
using log4net;
using RssBandit.Common.Logging;
using NewsComponents.Utils;
using RssBandit.WinGui.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;


namespace RssBandit.WinGui.ViewModel
{
    public partial class ApplicationViewModel: ViewModelBase
    {

        private static readonly ILog _log = Log.GetLogger(typeof(ApplicationViewModel)); 

        #region balloon popup related members

        ///<summary>
        /// this here because we only want to display the balloon popup, if there are really new items received:
        ///</summary>
        private int _lastUnreadFeedItemCountBeforeRefresh;

        /// <summary>
        ///  if a user explicitly close the balloon, we are silent for the next 12 retries (we refresh all 5 minutes, so this equals at least to one hour)
        /// </summary>
        private int _beSilentOnBalloonPopupCounter;

        #endregion

        #region timers

        /// <summary>
        /// Timer used for refreshing feeds 
        /// </summary>
        private System.Windows.Threading.DispatcherTimer _timerRefreshFeeds = new System.Windows.Threading.DispatcherTimer();


        /// <summary>
        /// Timer used for performing background UI tasks
        /// </summary>
        private UITaskTimer _uiTasksTimer = new UITaskTimer();

        #endregion 

        #region threading related 

        //private Dispatcher Dispatcher { get; set; }

        #endregion 

        #region Constructor

        protected ApplicationViewModel()
        {
        }

        #endregion // Constructor

        #region CloseCommand

        RelayCommand _closeCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to shutdown the application.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(param => this.OnRequestClose());

                return _closeCommand;
            }
        }

        #endregion // CloseCommand

        #region RequestClose [event]

        /// <summary>
        /// Raised when this app should be closed at all.
        /// </summary>
        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // RequestClose [event]    

        #region WorkOffline property
       
        //TODO: review (we might not be notified about a change...)
        public bool WorkOffline
        {
            get { return FeedSource.Offline; }
            set { FeedSource.Offline = value; }
        }

        #endregion

        #region Import Feeds Command

        RelayCommand _importFeedsCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to import feeds from a URL (OPML, own format).
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
        public bool CanImportFeeds
        {
            get { return true; }
        }

        void ImportFeeds()
        {
            string category = null, feedSource = null;
            GetSelectedFeedSourceAndCategoryNames(out feedSource, out category);                     

            RssBanditApplication.Current.ImportFeeds(String.Empty, category, feedSource);
        }


        #endregion

        #region Export Feeds Command

        RelayCommand _exportFeedsCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to export feeds to a file .
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
        public bool CanExportFeeds
        {
            get { return true; }
        }

        void ExportFeeds()
        {
            //TODO:
            //RssBanditApplication.Current.CmdExportFeeds(...);
            MessageBox.Show("Not yet connected...");
        }


        #endregion

        #region Subscribe RssFeed Command

        RelayCommand _subscribeRssFeedCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to create a new RSS Feed subscription.
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
        public bool CanSubscribeRssFeed
        {
            get { return true; }
        }

        /// <summary>
        /// Starts the process of subscribing to an RSS feed via the new subscription wizard
        /// </summary>     
        void SubscribeRssFeed()
        {
            SubscribeToFeed(null, null, String.Empty, AddSubscriptionWizardMode.SubscribeURLDirect);
        }


        /// <summary>
        /// Starts the process of subscribing to the specified RSS feed via the new subscription wizard
        /// </summary>     
        /// <param name="feedUrl">The URL of the feed to subscribe to</param>
        public void SubscribeRssFeed(string feedUrl)
        {
            //handle occassions where URL begins with feed: protocol 
            feedUrl = RssLocater.UrlFromFeedProtocolUrl(feedUrl);
            SubscribeToFeed(feedUrl, null, String.Empty, AddSubscriptionWizardMode.SubscribeURLDirect);
        }



        #endregion

        #region Subscribe NntpFeed Command

        RelayCommand _subscribeNntpFeedCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to create a new NNTP Feed subscription.
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
        public bool CanSubscribeNntpFeed
        {
            get { return true; }
        }

        /// <summary>
        /// Starts the process of subscribing to an NNTP feed via the new subscription wizard
        /// </summary>
        void SubscribeNntpFeed()
        {
            SubscribeToFeed(null, null, String.Empty, AddSubscriptionWizardMode.SubscribeNNTPDirect);
        }


        #endregion

        #region Subscribe Search Result Feed Command

        RelayCommand _subscribeSearchResultFeedCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to create a new search result Feed subscription.
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
        public bool CanSubscribeSearchResultFeed
        {
            get { return true; }
        }

        void SubscribeSearchResultFeed()
        {
             SubscribeToFeed(null, null, String.Empty, AddSubscriptionWizardMode.SubscribeSearchDirect);
        }



        /// <summary>
        /// Launches the subscription wizard with the specified wizard mode
        /// </summary>
        /// <param name="url">The feed URL to subscribe to</param>
        /// <param name="title">The title of the feed</param>
        /// <param name="searchTerms">The search terms. Used for search results feeds.</param>
        /// <param name="mode">The mode the subscription wizard should start in</param>
        /// <returns>true if any feeds were successfulyy subscribed to</returns>
        bool SubscribeToFeed(string url, string title, string searchTerms,
                                   AddSubscriptionWizardMode mode)
        {
            return this.SubscribeToFeed(url, null, title, searchTerms, mode); 
        }


        /// <summary>
        /// Launches the subscription wizard with the specified wizard mode
        /// </summary>
        /// <param name="url">The feed URL to subscribe to</param>
        /// <param name="category">The category of the feed node.</param>
        /// <param name="title">The title of the feed</param>
        /// <param name="searchTerms">The search terms. Used for search results feeds.</param>
        /// <param name="mode">The mode the subscription wizard should start in</param>
        /// <returns>true if any feeds were successfulyy subscribed to</returns>     
        bool SubscribeToFeed(string url, string category, string title, string searchTerms,
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
                    _log.Error("SubscribeToFeed caused exception.", ex);
                    wiz.DialogResult = DialogResult.Cancel;
                }

                if (wiz.DialogResult == DialogResult.OK)
                {
                    var bandit = RssBanditApplication.Current; 
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
                        this.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);
                   
                    return true;

                }// if (wiz.DialogResult == DialogResult.OK)
            }// using (var wiz = new AddSubscriptionWizard(mode)

            return false; 
        }

        #endregion

        #region Update Feeds Commands

        RelayCommand _updateAllFeedsCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to update all feeds.
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
        public bool CanUpdateAllFeeds
        {
            get { return true; }
        }

        /// <summary>
        /// Initiate a async. call to FeedSource.RefreshFeeds(force_download)
        /// </summary>               
        void UpdateAllFeeds()
        {
            this.UpdateAllFeeds(false); 
        }


        /// <summary>
        /// Initiate a async. call to FeedSource.RefreshFeeds(force_download)
        /// </summary>
        /// <param name="force_download"></param>
        public void UpdateAllFeeds(bool force_download)
        {
            var rootNodes = new List<CategorizedFeedSourceViewModel>();

            foreach (var treeItem in RssBanditApplication.MainWindow.tree.Items)
            {
                CategorizedFeedSourceViewModel cfsvm = treeItem as CategorizedFeedSourceViewModel;
                if (cfsvm != null)
                {
                    rootNodes.Add(cfsvm);
                }
            }

            if (rootNodes.Count != 0)
            {
                if (_timerRefreshFeeds.IsEnabled)
                    _timerRefreshFeeds.Stop();
                _lastUnreadFeedItemCountBeforeRefresh = rootNodes.Sum(n => n.UnreadCount);
                RssBanditApplication.Current.BeginRefreshFeeds(force_download);
            }
        }

        RelayCommand _updateFeedsInFolderCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to update all feeds in the selected folder.
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
        public bool CanUpdateFeedsInFolder
        {
            get { return true; }
        }

        void UpdateFeedsInFolder()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        #endregion

        #region CatchUp Feeds Commands

        RelayCommand _catchUpAllFeedsCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to catch up all feeds (mark read).
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
        public bool CanCatchUpAllFeeds
        {
            get { return true; }
        }

        void CatchUpAllFeeds()
        {
            //TODO:
            MessageBox.Show("Not yet connected...");
        }

        RelayCommand _catchUpFeedsInFolderCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to catch up all feeds (mark read).
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
        public bool CanCatchUpFeedsInFolder
        {
            get { return true; }
        }

        void CatchUpFeedsInFolder()
        {
            //TODO:
            MessageBox.Show("Not yet connected...");
        }

        #endregion

        #region Add Feed Sources Command(s)

        RelayCommand _addFeedSourcesCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to add one of the supported feedsource(s).
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
        public bool CanAddFeedSources
        {
            get { return true; }
        }

        void AddFeedSources()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        RelayCommand _addFacebookFeedSourceCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to add the facebook feedsource.
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

        public bool CanAddFacebookFeedSource
        {
            get { return true; }
        }

        void AddFacebookFeedSource()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        RelayCommand _addGoogleReaderFeedSourceCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to add the google reader feedsource.
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

        public bool CanAddGoogleReaderFeedSource
        {
            get { return true; }
        }

        void AddGoogleReaderFeedSource()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        RelayCommand _addWindowsCommonFeedSourceCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to add the windows common feedlist feedsource.
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

        public bool CanAddWindowsCommonFeedSource
        {
            get { return true; }
        }

        void AddWindowsCommonFeedSource()
        {
            //TODO
            MessageBox.Show("Not yet connected...");
        }

        #endregion

        #region initialization methods 

        /// <summary>
        /// Initializes main UI components and data structures
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
            this._timerRefreshFeeds.Interval = TimeSpan.FromMilliseconds(600000);
            this._timerRefreshFeeds.Tick += this.OnTimerFeedsRefreshElapsed;
        }

        ///// <summary>
        ///// initializes jump list icons and thumbnail strip in Windows 7.  
        ///// </summary>
        //private void InitWin7Components()
        //{
        //    jumpList = JumpList.CreateJumpList();
        //    jlcRecent = new JumpListCustomCategory(SR.JumpListRecentCategory);
        //    jumpList.AddCustomCategories(jlcRecent);
        //    pictureBox = new PictureBox();

        //    //add tasks         
        //    jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddSubscriptionCaption)
        //    {
        //        IconReference = new IconReference(RssBanditApplication.GetFeedIconPath(), 0),
        //        Arguments = "http://www.example.com/feed.rss"
        //    });
        //    jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddFacebookCaption)
        //    {
        //        IconReference = new IconReference(RssBanditApplication.GetFacebookIconPath(), 0),
        //        Arguments = "-f"
        //    });
        //    jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddGoogleCaption)
        //    {
        //        IconReference = new IconReference(RssBanditApplication.GetGoogleIconPath(), 0),
        //        Arguments = "-g"
        //    });
        //    jumpList.AddUserTasks(new JumpListSeparator());
        //    jumpList.AddUserTasks(new JumpListLink(Resource.OutgoingLinks.ProjectNewsUrl, SR.JumpListGoToWebsiteCaption)
        //    {
        //        IconReference = new IconReference(Application.ExecutablePath, 0)
        //    });
        //    jumpList.Refresh();


        //    //
        //    //thumbnail toolbar button setup
        //    //
        //    buttonAdd = new ThumbnailToolbarButton(Properties.Resources.RssDiscovered1, SR.ThumbnailButtonAdd);
        //    buttonAdd.Enabled = true;
        //    buttonAdd.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(OnTaskBarButtonAddClicked);

        //    buttonRefresh = new ThumbnailToolbarButton(Properties.Resources.feedRefresh, SR.ThumbnailButtonRefresh);
        //    buttonRefresh.Enabled = true;
        //    buttonRefresh.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(OnTaskBarButtonRefreshClick);

        //    TaskbarManager.Instance.ThumbnailToolbars.AddButtons(new WindowInteropHelper(RssBanditApplication.MainWindow).Handle, buttonAdd, buttonRefresh);
        //}


        #endregion 

        #region Timer related methods

        /// <summary>
        /// Callback called refresh feeds timer  c
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerFeedsRefreshElapsed(object sender, EventArgs e)
        {
            if (RssBanditApplication.Current.InternetAccessAllowed && RssBanditApplication.Current.CurrentGlobalRefreshRateMinutes > 0)
            {
                UpdateAllFeeds(false);
            }
        }

        /// <summary>
        /// Callback for UITasksTimer
        /// </summary>
        private void OnTasksTimerTick(object sender, EventArgs e)
        {

            if (_uiTasksTimer[DelayedTasks.StartRefreshOneFeed])
            {
                _uiTasksTimer.StopTask(DelayedTasks.StartRefreshOneFeed);
                var feedUrl = (string)_uiTasksTimer.GetData(DelayedTasks.StartRefreshOneFeed, true);
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

        #endregion 

        #region Helper methods 

        #region Conversion methods from NewsComponents objects to ViewModel objects 

        public static CategorizedFeedSourceViewModel ViewModelOf(FeedSourceEntry entry)
        {
            
            foreach (var treeItem in RssBanditApplication.MainWindow.tree.Items)
            {
                CategorizedFeedSourceViewModel cfsvm = treeItem as CategorizedFeedSourceViewModel; 
                if ((cfsvm != null) && cfsvm.Name.Equals(entry.Name))
                {
                    return cfsvm; 
                }
            }
            
            return null; 
        }

        #endregion


        /// <summary>
        /// Returns the FeedSource where this feed is subscribed in. 
        /// </summary>
        /// <param name="feedUrl">The specified feed</param>
        /// <returns>The FeedSource where the feed is subscribed</returns>
        public FeedSource FeedSourceOf(string feedUrl)
        {
            FeedSourceEntry entry = FeedSourceEntryOf(feedUrl);
            if (entry != null)
                return entry.Source;
            return null;
        }

        /// <summary>
        /// Returns the FeedSourceEntry where this feed is subscribed in. 
        /// </summary>
        /// <param name="feedUrl">The specified feed</param>
        /// <returns>The FeedSourceEntry where the feed is subscribed</returns>		
        public FeedSourceEntry FeedSourceEntryOf(string feedUrl)
        {
            if (StringHelper.EmptyTrimOrNull(feedUrl))
                return null;

            return RssBanditApplication.Current.FeedSources.Sources.FirstOrDefault(fse => fse.Source.IsSubscribed(feedUrl));
        }

        /// <summary>
        /// Returns the name of the feed source of the currently selected item in the tree view. Returns null if no item in the tree view is selected
        /// </summary>
        /// <returns></returns>
        private static void GetSelectedFeedSourceAndCategoryNames(out string feedSource, out string category)
        {
            feedSource = category = null; 

            if (RssBanditApplication.MainWindow.tree.SelectedItem != null)
            {
                var si = RssBanditApplication.MainWindow.tree.SelectedItem as TreeNodeViewModelBase;
                if (si != null)
                {
                    category = si.Category;
                    feedSource = si.Source.Name;
                }
                else
                {
                    var cfs = RssBanditApplication.MainWindow.tree.SelectedItem as CategorizedFeedSourceViewModel;
                    if (cfs != null)
                        feedSource = cfs.Name;
                }
            }        
        }


        /// <summary>
        /// Add a new feed to the GUI tree view
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="category">Feed Category</param>
        /// <param name="f">Feed</param>
        public void AddNewFeedNode(FeedSourceEntry entry, string category, INewsFeed f)
        {
            //find category node or create if it doesn't exist
            CategorizedFeedSourceViewModel entryModel = ViewModelOf(entry);
            FolderViewModel folder = entryModel.CreateHive(category); 

            var feed = new FeedViewModel(f, folder, entryModel);
            folder.Children.Add(feed); 

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

        #endregion 
    }
}