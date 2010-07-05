#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using NewsComponents.Utils;
using NewsComponents;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.ViewModel
{
    public class FolderViewModel : TreeNodeViewModelBase
    {

        private string _name;
        
        public FolderViewModel(string name, TreeNodeViewModelBase parent, CategorizedFeedSourceViewModel source)
        {
            name.ExceptionIfNullOrEmpty("name");
            _name = name;
            baseParent = parent;
            _feedSource = source;
            Type = FeedNodeType.Category; 
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public override string Category
        {
            get { 
                string catName = _name;
                TreeNodeViewModelBase parent = baseParent;

                while (parent != null)
                {
                    catName = parent.Name + FeedSource.CategorySeparator + catName;
                    parent  = parent.Parent; 
                }

                return catName; 
            }
            set { /* not required: we have the Name and the Parents to calculate it */ }
        }
       
      
    }
}