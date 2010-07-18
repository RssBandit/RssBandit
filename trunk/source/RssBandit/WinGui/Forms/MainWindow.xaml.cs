

using RssBandit.WinGui.Utility;
using System;
using NewsComponents;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Shell;
using RssBandit.Resources;
using System.Windows.Forms;
using RssBandit.WinGui.ViewModel;
using System.Linq;
using System.Windows.Interop; 

namespace RssBandit.WinGui.Forms
{ 

     
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        #region Windows 7 jumplist and taskbar related members

        /// <summary>
        /// The add new feed button in the thumbnail strip that shows up on hover in the Windows 7 task bar
        /// </summary>
        private ThumbnailToolbarButton buttonAdd;

        /// <summary>
        /// The refresh feeds button in the thumbnail strip that shows up on hover in the Windows 7 task bar
        /// </summary>     
        private ThumbnailToolbarButton buttonRefresh;

        /// <summary>
        /// Represents the Windows 7 jumplist for this application. 
        /// </summary>
        private JumpList jumpList;

        /// <summary>
        /// Represents the recently browsed web pages that show up in the Windows 7 jump list
        /// </summary>
        private JumpListCustomCategory jlcRecent;

        /// <summary>
        /// Contents of the Recent jump list category
        /// </summary>
        private List<string> jlcRecentContents = new List<string>();

        /// <summary>
        /// Picture box used for rendering thumbnail buttons on hover in the task bar
        /// </summary>
        private PictureBox pictureBox;

        #endregion 
              
        ///<summary>
        /// this here because we only want to display the balloon popup, if there are really new items received:
        ///</summary>
        private int _lastUnreadFeedItemCountBeforeRefresh;

        /// <summary>
        ///  if a user explicitly close the balloon, we are silent for the next 12 retries (we refresh all 5 minutes, so this equals at least to one hour)
        /// </summary>
        private int _beSilentOnBalloonPopupCounter;


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

        /// <summary>
        /// Constructor initializes class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();            
            WpfWindowSerializer.Register(this, WindowStates.All);

            Loaded += delegate
            {
                Splash.Close();

                //support for Windows 7 taskbar features
                if (TaskbarManager.IsPlatformSupported)
                {
                    InitWin7Components();
                }  

                //start UI tasks timer
                _uiTasksTimer.Tick += new EventHandler(OnTasksTimerTick);
                this.Loaded -= delegate { };
            };

        }

        /// <summary>
        /// Initializes main UI components and data structures
        /// </summary>
        private void Init()
        {

            //initialize timers
            this._timerRefreshFeeds.Interval = TimeSpan.FromMilliseconds(600000); 
            this._timerRefreshFeeds.Tick += new EventHandler(this.OnTimerFeedsRefreshElapsed);
        }

        /// <summary>
        /// initializes jump list icons and thumbnail strip in Windows 7.  
        /// </summary>
        protected void InitWin7Components()
        {
            jumpList = JumpList.CreateJumpList();
            jlcRecent = new JumpListCustomCategory(SR.JumpListRecentCategory);
            jumpList.AddCustomCategories(jlcRecent);
            pictureBox = new PictureBox();                                  

            //add tasks         
            jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddSubscriptionCaption)
            {
                IconReference = new IconReference(RssBanditApplication.GetFeedIconPath(), 0),
                Arguments = "http://www.example.com/feed.rss"
            });
            jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddFacebookCaption)
            {
                IconReference = new IconReference(RssBanditApplication.GetFacebookIconPath(), 0),
                Arguments = "-f"
            });
            jumpList.AddUserTasks(new JumpListLink(Application.ExecutablePath, SR.JumpListAddGoogleCaption)
            {
                IconReference = new IconReference(RssBanditApplication.GetGoogleIconPath(), 0),
                Arguments = "-g"
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
            buttonAdd = new ThumbnailToolbarButton(Properties.Resources.RssDiscovered1, SR.ThumbnailButtonAdd);
            buttonAdd.Enabled = true;
            buttonAdd.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(OnTaskBarButtonAddClicked);

            buttonRefresh = new ThumbnailToolbarButton(Properties.Resources.feedRefresh, SR.ThumbnailButtonRefresh);
            buttonRefresh.Enabled = true;
            buttonRefresh.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(OnTaskBarButtonRefreshClick);

            TaskbarManager.Instance.ThumbnailToolbars.AddButtons(new WindowInteropHelper(this).Handle, buttonAdd, buttonRefresh);
        }


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
            DelayTask(task, null, new TimeSpan(0,0,0,0,100));
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


        /// <summary>
        /// Calls/Open the newFeedDialog on the GUI thread, if required.
        /// </summary>
        /// <param name="newFeedUrl">Feed Url to add</param>
        public void AddFeedUrlSynchronized(string newFeedUrl)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                this.Model.SubscribeRssFeed(newFeedUrl); 
            })
            );
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
    }
}
