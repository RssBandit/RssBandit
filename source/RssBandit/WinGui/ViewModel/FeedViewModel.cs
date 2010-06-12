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
using NewsComponents.Feed;
using NewsComponents.Utils;

namespace RssBandit.WinGui.ViewModel
{
    public class FeedViewModel : TreeNodeViewModelBase
    {
        private readonly INewsFeed _feed;
       

        public FeedViewModel(INewsFeed feed, TreeNodeViewModelBase parent, CategorizedFeedSourceViewModel source)
        {
            _feed = feed;
            baseParent = parent;
            _feedSource = source;             
        }

        public override string Name
        {
            get { return _feed.title; }
            set { _feed.title = value; }
        }

        public bool IsNntp { get { return RssHelper.IsNntpUrl(_feed.link); } }

        public bool HasUnreadItems { get { return _feed.containsNewMessages; } }

        public override string Category
        {
            get { return _feed.category; }
            set
            {
                if (!string.Equals(_feed.category, value, StringComparison.CurrentCulture))
                {
                    _feed.category = value;
                    OnPropertyChanged("Category");
                }
            }
        }
    }
}
