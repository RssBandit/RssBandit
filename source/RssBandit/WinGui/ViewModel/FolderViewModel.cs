﻿#region Version Info Header
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using NewsComponents;
using RssBandit.AppServices.Util;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.ViewModel
{
    [DebuggerDisplay("Name = {Name}, Category = {Category}")]
    public class FolderViewModel : TreeNodeViewModelBase, IFolderHolderNode
    {
        private readonly ObservableCollection<TreeNodeViewModelBase> _children = new ObservableCollection<TreeNodeViewModelBase>();
        private readonly ObservableCollection<FeedViewModel> _feeds = new ObservableCollection<FeedViewModel>();
        private readonly ObservableCollection<FolderViewModel> _folders = new ObservableCollection<FolderViewModel>();
// ReSharper disable UnaccessedField.Local
        private readonly CollectionChangedObserver _folderObserver;
        private readonly CollectionChangedObserver _feedObserver;
// ReSharper restore UnaccessedField.Local
        private string _name;

        private readonly ObservableCollection<INewsItem> _newsItems = new ObservableCollection<INewsItem>();

        public FolderViewModel(string name, string parentCategory, CategorizedFeedSourceViewModel source)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            _name = name;
            Source = source;

            if (parentCategory != null)
                parentCategory += FeedSource.CategorySeparator;

            Category = parentCategory + name;

            Type = FeedNodeType.Category;

            // Add them both into children
            _folders.SynchronizeCollection(_children, f => f, out _folderObserver);
            _feeds.SynchronizeCollection(_children, f => f, out _feedObserver);

            Folders = new ReadOnlyObservableCollection<FolderViewModel>(_folders);
            Feeds = new ReadOnlyObservableCollection<FeedViewModel>(_feeds);

            Children = new ReadOnlyObservableCollection<TreeNodeViewModelBase>(_children);

            NewsItems = new ReadOnlyObservableCollection<INewsItem>(_newsItems);

            // TODO: Listen for child folders/feeds to get our items
        }

        public string Category { get; private set; }
        public ReadOnlyObservableCollection<TreeNodeViewModelBase> Children { get; private set; }

        public ReadOnlyObservableCollection<FeedViewModel> Feeds { get; private set; }

        public ReadOnlyObservableCollection<FolderViewModel> Folders { get; private set; }

        public override string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(() => Name);
            }
        }

        public CategorizedFeedSourceViewModel Source { get; private set; }

        public ReadOnlyObservableCollection<INewsItem> NewsItems
        {
            get;
            private set;
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
    }
}