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
using System.Linq;
using System.Text;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;

namespace RssBandit.WinGui.ViewModel
{
    public class CategorizedFeedSourceViewModel: ViewModelBase
    {
        private readonly FeedSourceEntry _entry;
        private ObservableCollection<NodeViewModel> _children;

        public CategorizedFeedSourceViewModel(FeedSourceEntry feedSource)
        {
            _entry = feedSource;
        }

        public string Name {
            get { return _entry.Name; }
            set { _entry.Name = value; }
        }

        public ObservableCollection<NodeViewModel> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<NodeViewModel>();

                    _entry.Source.LoadFeedlist();

                    if (_entry.Source.FeedsListOK)
                    {
                        ICollection<INewsFeedCategory> categories = _entry.Source.GetCategories().Values;
                        
                        var categoryTable = new Dictionary<string, FolderViewModel>();
                        var categoryList = new List<INewsFeedCategory>(categories);

                        foreach (var f in _entry.Source.GetFeeds().Values)
                        {
                            FeedViewModel tn = new FeedViewModel(f);

                            string category = (f.category ?? String.Empty);
                            if (String.IsNullOrEmpty(category))
                            {
                                _children.Add(tn);
                            }
                            else
                            {
                                FolderViewModel catnode;
                                if (!categoryTable.TryGetValue(category, out catnode))
                                {
                                    catnode = CreateHive(category,_children, categoryTable);
                                } 
                                
                                catnode.Children.Add(tn);
                            }

                            for (int i = 0; i < categoryList.Count; i++)
                            {
                                if (categoryList[i].Value.Equals(category))
                                {
                                    categoryList.RemoveAt(i);
                                    break;
                                }
                            }
                        }

                        //add categories, we not already have
                        foreach (var c in categoryList)
                        {
                            CreateHive(c.Value, _children, categoryTable);
                        }

                    }
                    else
                    {
                        //TODO: indicate the error in the UI
                    }
                }

                return _children;
            }
            set { _children = value; }
        }

        static FolderViewModel CreateHive(string pathName, ICollection<NodeViewModel> rootNodes, Dictionary<string, FolderViewModel> knownFolders)
        {
            pathName.ExceptionIfNullOrEmpty("pathName");
            
            FolderViewModel folderViewModel;
            if (!knownFolders.TryGetValue(pathName, out folderViewModel))
            {
                List<string> catHives = new List<string>(pathName.Split(FeedSource.CategorySeparator.ToCharArray()));
                bool wasNew = false;
                StringBuilder path = new StringBuilder(pathName.Length);

                for (int i = 0; i < catHives.Count; i++)
                {
                    FolderViewModel rootNode = null;
                    if (!wasNew)
                        rootNode = (FolderViewModel) rootNodes.FirstOrDefault(
                            n => (n is FolderViewModel && n.Name.Equals(catHives[i], StringComparison.CurrentCulture)));

                    if (rootNode == null)
                    {
                        rootNode = new FolderViewModel(catHives[i]);
                        rootNodes.Add(rootNode);
                        
                        path.AppendFormat("{1}{0}", catHives[i], path.Length > 0 ? FeedSource.CategorySeparator: String.Empty);
                        if (!knownFolders.ContainsKey(path.ToString()))
                            knownFolders.Add(path.ToString(), rootNode);

                        wasNew = true;
                    }

                    folderViewModel = rootNode;
                    rootNodes = rootNode.Children;

                }
            }

            return folderViewModel;
        }


    }

    
}
