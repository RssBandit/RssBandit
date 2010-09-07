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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.Util;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.ViewModel
{
    [DebuggerDisplay("Name = {Name}, Category = {Category}")]
    public class FeedViewModel : TreeNodeViewModelBase
    {
        private readonly INewsFeed _feed;
        private readonly PropertyObserver<INewsFeed> _feedObserver;

        public FeedViewModel(INewsFeed feed, CategorizedFeedSourceViewModel source)
        {
            _feed = feed;
            _feedObserver = PropertyObserver.Create(_feed);

            Source = source;
            //TODO: Image will be set to the downloaded favicon (later on, as it is downloaded)
            Type = FeedNodeType.Feed;

            _feedObserver.RegisterHandler(f => f.containsNewMessages, f => OnPropertyChanged(() => HasUnreadItems));
        }

        public string Category
        {
            get { return _feed.category; }
            //protected set
            //{
            //    if (!string.Equals(_feed.category, value, StringComparison.CurrentCulture))
            //    {
            //        _feed.category = value;
            //        OnPropertyChanged(() => Category);
            //    }
            //}
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
    }
}