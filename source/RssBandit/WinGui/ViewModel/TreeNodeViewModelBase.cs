#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Collections.ObjectModel;

namespace RssBandit.WinGui.ViewModel
{
    public abstract class TreeNodeViewModelBase : ViewModelBase
    {
        private ObservableCollection<TreeNodeViewModelBase> _children = new ObservableCollection<TreeNodeViewModelBase>();
        protected CategorizedFeedSourceViewModel _feedSource;

        protected TreeNodeViewModelBase baseParent;

        public abstract string Name
        {
            get; set;
        }

        public virtual ObservableCollection<TreeNodeViewModelBase> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        public virtual TreeNodeViewModelBase Parent
        {
            get { return baseParent; }
            set { baseParent = value; }
        }

        public abstract string Category
        {
            get; set;
        }

        public virtual CategorizedFeedSourceViewModel Source { get { return _feedSource; } }
    }
}