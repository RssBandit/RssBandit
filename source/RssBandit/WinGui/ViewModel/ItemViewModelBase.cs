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
using RssBandit.AppServices;
using RssBandit.AppServices.Core;

namespace RssBandit.WinGui.ViewModel
{
    public abstract class ItemViewModelBase : ModelBase, IItemViewModelBase
    {
        protected ObservableCollection<ItemViewModelBase> p_children = new ObservableCollection<ItemViewModelBase>();
        
        private bool _isExpanded;
        private bool _isSelected;

        /// <summary>
        ///   Indicates whether the tree node is expanded
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(() => IsExpanded);
                }
            }
        }



        /// <summary>
        ///   Indicates whether the tree node is selected
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(() => IsSelected);
                    RssBanditApplication.Current.ContextInternal.SelectedItem = this;
                }
            }
        }
    }
}
