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

namespace RssBandit.WinGui.ViewModel
{
    public class FolderViewModel : TreeNodeViewModelBase
    {

        private string _name;
        TreeNodeViewModelBase _parent; 
        
        public FolderViewModel(string name, TreeNodeViewModelBase parent, CategorizedFeedSourceViewModel source)
        {
            name.ExceptionIfNullOrEmpty("name");
            _name = name;
            _parent = parent;
            _feedSource = source; 
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
                TreeNodeViewModelBase parent = _parent;

                while (parent != null)
                {
                    catName = parent.Name + FeedSource.CategorySeparator + catName;
                    parent  = parent.Parent; 
                }

                return catName; 
            }
        }
       
      
    }
}