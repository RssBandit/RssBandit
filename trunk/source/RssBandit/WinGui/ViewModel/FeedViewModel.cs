#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using NewsComponents.Feed;
using NewsComponents.Utils;

namespace RssBandit.WinGui.ViewModel
{
    public class FeedViewModel : TreeNodeViewModelBase
    {
        private readonly INewsFeed _feed;

        public FeedViewModel(INewsFeed feed)
        {
            _feed = feed;
        }

        public override string Name
        {
            get { return _feed.title; }
            set { _feed.title = value; }
        }

        public bool IsNntp { get { return RssHelper.IsNntpUrl(_feed.link); } }

        public bool HasUnreadItems { get { return _feed.containsNewMessages; } }
    }
}
