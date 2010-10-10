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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.AppServices.Util;
using RssBandit.Util;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.ViewModel
{
    [DebuggerDisplay("Name = {Name}, Category = {Category}")]
    public class FeedViewModel : TreeNodeViewModelBase
    {
        private readonly INewsFeed _feed;
        private readonly IObservable<IFeedDetails> _detailsObs;
        private readonly PropertyObserver<INewsFeed> _feedObserver;
        private CollectionChangedObserver _itemsObserver;
        private readonly IDisposable _feedDetailsSubscription;
        private readonly ObservableCollection<INewsItem> _newsItems = new ObservableCollection<INewsItem>();
        private IFeedDetails _currentDetails;

        public FeedViewModel(INewsFeed feed, IObservable<IFeedDetails> details, CategorizedFeedSourceViewModel source)
        {
            _feed = feed;
            _detailsObs = details;
            _feedObserver = PropertyObserver.Create(_feed);
            NewsItems = new ReadOnlyObservableCollection<INewsItem>(_newsItems);

            Source = source;
            //TODO: Image will be set to the downloaded favicon (later on, as it is downloaded)
            Type = FeedNodeType.Feed;

            _feedObserver.RegisterHandler(f => f.containsNewMessages, f => OnPropertyChanged(() => HasUnreadItems));
            _feedObserver.RegisterHandler(f => f.title, f => OnPropertyChanged(() => Name));

            _feedDetailsSubscription = _detailsObs.Subscribe(OnFeedDetailsChanged);

 
        }

        private void OnFeedDetailsChanged(IFeedDetails details)
        {
            if (_itemsObserver != null)
            {
                _itemsObserver.Dispose();
            }

            Trace.WriteLine(string.Format("Feed Details Changed {0}", details != null ? details.Link : _feed.id));
            Trace.WriteLine(string.Format("Null?: {0}", details == null));
            _currentDetails = details;
            _newsItems.Clear();
            if (_currentDetails == null)
                return;



            _currentDetails.ItemsList.SynchronizeCollection(_newsItems, f => f, out _itemsObserver, context: SynchronizationContext.Current);
        }


        public string Category
        {
            get { return _feed.category; }
        }

        public bool HasUnreadItems
        {
            get { return _feed.containsNewMessages; }
        }

        public bool IsNntp
        {
            get { return RssHelper.IsNntpUrl(_feed.link); }
        }

        public override string Name
        {
            get { return _feed.title; }
            set
            {
                _feed.title = value;
                OnPropertyChanged(() => Name);
            }
        }

        public CategorizedFeedSourceViewModel Source { get; private set; }

        public ReadOnlyObservableCollection<INewsItem> NewsItems
        {
            get;
            private set;
        }
    }
}