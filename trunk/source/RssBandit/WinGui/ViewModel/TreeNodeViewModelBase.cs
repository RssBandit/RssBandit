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
using System;
using NewsComponents;

namespace RssBandit.WinGui.ViewModel
{
    public abstract class TreeNodeViewModelBase : ViewModelBase
    {
        private ObservableCollection<TreeNodeViewModelBase> _children = new ObservableCollection<TreeNodeViewModelBase>();
        protected CategorizedFeedSourceViewModel _feedSource;

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
            get; set; 
        }

        public abstract string Category
        {
            get; 
         }

        public virtual CategorizedFeedSourceViewModel Source { get { return _feedSource; } }
    }
}