#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Windows.Input;
using NewsComponents;
using RssBandit.WinGui.Commands;
using System.Windows.Controls;
using RssBandit.WinGui.Dialogs;
using NewsComponents.Feed;
using System.Windows.Forms;
using log4net;
using RssBandit.Common.Logging;

namespace RssBandit.WinGui.ViewModel
{
    public class ApplicationViewModel: ViewModelBase
    {

        private static readonly ILog _log = Log.GetLogger(typeof(ApplicationViewModel));

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
        /// to import feeds froma URL (OPML, own format).
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

        void SubscribeRssFeed()
        {
            //TODO: impl.
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

        void SubscribeNntpFeed()
        {
            //TODO: impl.
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

        void SubscribeToFeed(string url, /* string category,*/ string title, string searchTerms,
                                    AddSubscriptionWizardMode mode)
        {
            using (var wiz = new AddSubscriptionWizard(mode)
            {
                FeedUrl = (url ?? String.Empty),
                FeedTitle = (title ?? String.Empty),
                SearchTerms = (searchTerms ?? String.Empty)
            })
            {

                string feedSourceName, category; 

                /* if (category != null) // does remember the last category:
                    wiz.FeedCategory = category; */ 

               GetSelectedFeedSourceAndCategoryNames(out feedSourceName , out category); 
               wiz.FeedSourceName = feedSourceName;
               wiz.FeedCategory = category; 
                
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

                            /*
                            guiMain.AddNewFeedNode(entry, f.category, f);

                            if (wiz.FeedInfo == null)
                                guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

                            anySubscription = true;
                             */
                        }

                        //return anySubscription;
                    }

                    f = bandit.CreateFeedFromWizard(wiz, entry, 0);

                    if (f == null)
                    {
                        return;
                    }

                    /*
                    // add feed visually
                    guiMain.AddNewFeedNode(entry, f.category, f);

                    if (wiz.FeedInfo == null)
                        guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);

                    */
                                     
                }
            }

        }

        #endregion


        #region Helper methods 

        /// <summary>
        /// Returns the name of the feed source of the currently selected item in the tree view. Returns null if no item in the tree view is selected
        /// </summary>
        /// <returns></returns>
        private void GetSelectedFeedSourceAndCategoryNames(out string feedSource, out string category)
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
                    feedSource = cfs.Name;
                }
            }        
        }

        #endregion 
    }
}