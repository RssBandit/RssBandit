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
using System.Globalization;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.ViewModel
{
    public class CategorizedFeedSourceViewModel : TreeNodeViewModelBase//ViewModelBase
    {
        /// <summary>
        /// The underlying feed source entry 
        /// </summary>
        private readonly FeedSourceEntry _entry;

        /// <summary>
        /// The children of the tree node
        /// </summary>
        private ObservableCollection<TreeNodeViewModelBase> _children;


        /// <summary>
        /// Constructor intializes underlying feedsource
        /// </summary>
        /// <param name="feedSource"></param>
        public CategorizedFeedSourceViewModel(FeedSourceEntry feedSource)
        {
            _entry = feedSource;
        }


        /// <summary>
        /// The user provided name of the feed source
        /// </summary>
        public override string Name {
            get { return _entry.Name; }
            set { _entry.Name = value; }
        }


        /// <summary>
        /// The image that represents the feed source
        /// </summary>
        public override string Image
        {
            get
            {
                switch (_entry.SourceType)
                {
                    case FeedSourceType.DirectAccess:
                        return "/Resources/Images/TreeView/bandit.feedsource.16.png";
                    case FeedSourceType.Google:
                        return "/Resources/Images/TreeView/google.feedsource.16.png";
                    case FeedSourceType.WindowsRSS:
                        return "/Resources/Images/TreeView/windows.feedsource.16.png";
                    case FeedSourceType.Facebook:
                        return "/Resources/Images/TreeView/facebook.feedsource.16.png";
                    case FeedSourceType.NewsGator:
                        return "/Resources/Images/TreeView/newsgator.feedsource.16.png";
                    default:
                        break;
                }
                return base.Image;
            }
            set
            {
                base.Image = value;
            }
        }

        /// <summary>
        /// The children of the feed source in the tree view
        /// </summary>
        public override ObservableCollection<TreeNodeViewModelBase> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<TreeNodeViewModelBase>();

                    _entry.Source.LoadFeedlist();

                    if (_entry.Source.FeedsListOK)
                    {
                        ICollection<INewsFeedCategory> categories = _entry.Source.GetCategories().Values;
                        
                        var categoryTable = new Dictionary<string, FolderViewModel>();
                        var categoryList = new List<INewsFeedCategory>(categories);

                        foreach (var f in _entry.Source.GetFeeds().Values)
                        {                          

                            string category = (f.category ?? String.Empty);
                            if (String.IsNullOrEmpty(category))
                            {
                                _children.Add(new FeedViewModel(f, null, this));
                            }
                            else
                            {
                                FolderViewModel catnode;
                                if (!categoryTable.TryGetValue(category, out catnode))
                                {
                                    catnode = CreateHive(category,_children, categoryTable, this);
                                } 
                                
                                catnode.Children.Add(new FeedViewModel(f, catnode, this));
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
                            if (!categoryTable.ContainsKey(c.Value))
                                CreateHive(c.Value, _children, categoryTable, this);
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

        /// <summary>
        /// The category of the feed source. 
        /// </summary>
        /// <remarks>Always returns null</remarks>
        public override string Category
        {
            get { return null; }
            set {  }
        }

        /// <summary>
        /// Returns the current object. 
        /// </summary>
        public override CategorizedFeedSourceViewModel Source
        {
            get { return this; }
        }

        /// <summary>
        /// Creates a FolderViewModel that represents a feed category in the tree view
        /// </summary>
        /// <param name="pathName">Full path or category name</param>      
        public FolderViewModel CreateHive(string pathName)
        {
            TreeNodeViewModelBase target = null;

            if (string.IsNullOrEmpty(pathName)) return null;

            string[] catHives = pathName.Split(FeedSource.CategorySeparator.ToCharArray());
            bool wasNew = false;
          
            foreach (var catHive in catHives)
            {
                TreeNodeViewModelBase n = !wasNew ? FindChildNode(catHive, FeedNodeType.Category) : null;

                if (n == null)
                {
                    n = new FolderViewModel(catHive, target, this);

                    if (target == null)
                        this.Children.Add(n);
                    else
                        target.Children.Add(n);

                    wasNew = true; // shorten search
                }

               target = n;
            } //foreach

            
            return target as FolderViewModel; 
        }


        /// <summary>
        /// Find a direct child node.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="text"></param>
        /// <param name="nType"></param>
        /// <returns></returns>
        public TreeNodeViewModelBase FindChildNode(string text, FeedNodeType nType)
        {
            if ( text == null) return null;
            text = text.Trim();

            foreach(TreeNodeViewModelBase t in this.Children)
            {
                if (t.Type == nType && String.Compare(t.Name, text, false, CultureInfo.CurrentUICulture) == 0)
                    // node names are usually english or client locale
                    return t;
            }
            return null;
        }

        #region static methods

        /// <summary>
        /// Creates a FolderViewModel that represents a feed category in the tree view
        /// </summary>
        /// <param name="pathName">Full path or category name</param>
        /// <param name="childNodes">Child nodes of the root node in the tree view</param>
        /// <param name="knownFolders">List of known FolderViewModel objects encountered thus far</param>
        /// <param name="source">The feed source that owns the folder</param>
        /// <returns></returns>
        static FolderViewModel CreateHive(string pathName, ICollection<TreeNodeViewModelBase> childNodes, Dictionary<string, FolderViewModel> knownFolders, CategorizedFeedSourceViewModel source)
        {
            pathName.ExceptionIfNullOrEmpty("pathName");

            FolderViewModel startNode = null, previous = null;

            List<string> catHives = new List<string>(pathName.Split(FeedSource.CategorySeparator.ToCharArray()));
            bool wasNew = false;
            StringBuilder path = new StringBuilder(pathName.Length);

            for (int i = 0; i < catHives.Count; i++)
            {
                path.AppendFormat("{1}{0}", catHives[i], path.Length > 0 ? FeedSource.CategorySeparator : String.Empty);
                pathName = path.ToString();

                if (!knownFolders.TryGetValue(pathName, out startNode))
                {

                    if (!wasNew)
                        startNode = (FolderViewModel)childNodes.FirstOrDefault(
                            n => (n is FolderViewModel && n.Name.Equals(catHives[i], StringComparison.CurrentCulture)));


                    if (startNode == null)
                    {
                        startNode = new FolderViewModel(catHives[i], previous, source);
                        childNodes.Add(startNode);

                        if (!knownFolders.ContainsKey(pathName))
                            knownFolders.Add(pathName, startNode);

                        wasNew = true;
                    }
                }

                previous = startNode;
                childNodes = startNode.Children;
            }


            return startNode;
        }

        #endregion 

    }

    
}
