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

        public abstract string Name
        {
            get; set;
        }

        public virtual ObservableCollection<TreeNodeViewModelBase> Children
        {
            get { return _children; }
            set { _children = value; }
        }
    }
}