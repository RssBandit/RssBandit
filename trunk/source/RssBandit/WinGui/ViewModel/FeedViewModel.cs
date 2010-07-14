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
using RssBandit.WinGui.Interfaces; 

namespace RssBandit.WinGui.ViewModel
{
    public class FeedViewModel : TreeNodeViewModelBase
    {
        private readonly INewsFeed _feed;
       

        public FeedViewModel(INewsFeed feed, TreeNodeViewModelBase parent, CategorizedFeedSourceViewModel source)
        {
            _feed = feed;
            BaseParent = parent;
            BaseFeedSource = source;
            BaseImage = IsNntp ? "/Resources/Images/TreeView/doc.nntp.16.png" : "/Resources/Images/TreeView/doc.feed.16.png";   
            //TODO: Image will be set to the downloaded favicon (later on, as it is downloaded)
            Type = FeedNodeType.Feed; 
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
