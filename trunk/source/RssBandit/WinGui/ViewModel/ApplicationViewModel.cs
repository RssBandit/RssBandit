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

namespace RssBandit.WinGui.ViewModel
{
    public class ApplicationViewModel: ViewModelBase
    {
        
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
            string category = null;
            string feedSource = null;

            var bandit = RssBanditApplication.Current; 

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
            //TODO: impl.
        }


        #endregion
    }
}