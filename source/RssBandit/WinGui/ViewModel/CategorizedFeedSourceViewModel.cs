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
using System.Globalization;
using System.Linq;
using System.Text;
using NewsComponents;
using NewsComponents.Feed;
using RssBandit.Util;

namespace RssBandit.WinGui.ViewModel
{
    [DebuggerDisplay("Name = {Name}")]
    public class CategorizedFeedSourceViewModel : TreeNodeViewModelBase, IFolderHolderNode
    {
        private readonly ObservableCollection<TreeNodeViewModelBase> _children = new ObservableCollection<TreeNodeViewModelBase>();

        /// <summary>
        ///   The underlying feed source entry
        /// </summary>
        private readonly FeedSourceEntry _entry;


        private readonly ObservableCollection<FeedViewModel> _feeds = new ObservableCollection<FeedViewModel>();
        private readonly ObservableCollection<FolderViewModel> _folders = new ObservableCollection<FolderViewModel>();

        /// <summary>
        ///   Constructor intializes underlying feedsource
        /// </summary>
        /// <param name = "feedSource"></param>
        public CategorizedFeedSourceViewModel(FeedSourceEntry feedSource)
        {
            _entry = feedSource;
            _entry.Source.LoadFeedlist();

            // Add them both into children
            _folders.SynchronizeCollection(_children, f => f);
            _feeds.SynchronizeCollection(_children, f => f);

            Folders = new ReadOnlyObservableCollection<FolderViewModel>(_folders);
            Feeds = new ReadOnlyObservableCollection<FeedViewModel>(_feeds);

            Children = new ReadOnlyObservableCollection<TreeNodeViewModelBase>(_children);
            
            _entry.Source.Feeds.ListenToCollectionChanged(OnFeedsChanged);
            _entry.Source.Feeds.Run(n => AddNewsFeed(n));
        }

      

        public ReadOnlyObservableCollection<TreeNodeViewModelBase> Children { get; private set; }
        public ReadOnlyObservableCollection<FeedViewModel> Feeds { get; private set; }
        public ReadOnlyObservableCollection<FolderViewModel> Folders { get; private set; }

        /// <summary>
        ///   The user provided name of the feed source
        /// </summary>
        public override string Name
        {
            get { return _entry.Name; }
            set
            {
                _entry.Name = value;
                OnPropertyChanged(() => Name);
            }
        }

        public FeedSourceType SourceType
        {
            get { return _entry.SourceType; }
        }


     

        private void AddNewsFeed(INewsFeed feed)
        {
            var vm = new FeedViewModel(feed, this);

            IFolderHolderNode folder = GetOrCreateFolderForCategory(feed.category);

            folder.AddFeed(vm);
        }

        private IFolderHolderNode GetOrCreateFolderForCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return this;

            var catHives = new Queue<string>(category.Split(FeedSource.CategorySeparator.ToCharArray()));
            IFolderHolderNode parent = this;
            string parentCategory = null;
            while (catHives.Count > 0)
            {
                var nodeName = catHives.Dequeue();
                

                var node = parent.Folders.FirstOrDefault(f => f.Name == nodeName);
                if (node == null)
                {
                    node = new FolderViewModel(nodeName, parent.Category, this);
                    parent.AddFolder(node);
                }
                
                parentCategory += (FeedSource.CategorySeparator + nodeName);
                parent = node;
            }

            return parent;

        }

        public void AddFeed(FeedViewModel feed)
        {
            _feeds.Add(feed);
        }

        public void AddFolder(FolderViewModel folder)
        {
            _folders.Add(folder);
        }

        public void RemoveFeed(FeedViewModel feed)
        {
            _feeds.Remove(feed);
        }

        public void RemoveFolder(FolderViewModel folder)
        {
            _folders.Remove(folder);
        }


        public string Category
        {
            get { return null; }
        }


        private void OnFeedsChanged(INotifyCollectionChanged collection, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNewsFeed((INewsFeed) args.NewItems[0]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveNewsFeed((INewsFeed) args.OldItems[0]);
                    break;
            }
        }

        private void RemoveNewsFeed(INewsFeed feed)
        {
            IFolderHolderNode folder = GetOrCreateFolderForCategory(feed.category);

            var vm = folder.Feeds.FirstOrDefault(f => f.Name == feed.title);

            if (vm != null)
                folder.RemoveFeed(vm);
        }
    }


    public interface IFolderHolderNode
    {
        ReadOnlyObservableCollection<FolderViewModel> Folders { get; }
        ReadOnlyObservableCollection<FeedViewModel> Feeds { get; }
        void AddFeed(FeedViewModel feed);
        void AddFolder(FolderViewModel folder);
        void RemoveFeed(FeedViewModel feed);
        void RemoveFolder(FolderViewModel folder);
        string Category { get; }
    }
}