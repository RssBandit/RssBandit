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
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using RssBandit.AppServices;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.ViewModel
{
   public abstract class TreeNodeViewModelBase : ModelBase, ITreeNodeViewModelBase
    {
        private bool _anyNewComments;
        private bool _anyUnread;
        private ObservableCollection<TreeNodeViewModelBase> _children = new ObservableCollection<TreeNodeViewModelBase>();
        private bool _editable;
        private bool _isExpanded;
        private bool _isSelected;
        private int _unreadCount;
       

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
                    RssBanditApplication.Current.ContextInternal.SelectedNode = this;
                }
            }
        }

        /// <summary>
        ///   The name of the tree node
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        ///   Indicates the type of the node
        /// </summary>
        public FeedNodeType Type { get; protected set; }


        /// <summary>
        ///   AnyUnread and UnreadCount are working interconnected:
        ///   if you set UnreadCount to non zero, AnyUnread will be set to true and
        ///   then updates the visualized info to use the Unread Font and 
        ///   read counter state info. If UnreadCount is set to zero,
        ///   also AnyUnread is reset to false and refresh the caption to default.
        /// </summary>
        public int UnreadCount
        {
            get { return _unreadCount; }
            protected set
            {
                if (value != _unreadCount)
                {
                    _unreadCount = value;
                    AnyUnread = (_unreadCount > 0);

                    OnPropertyChanged(() => UnreadCount);
                }

            }
        }

       public bool AnyUnread
       {
           get { return _anyUnread; }
           private set
           {
               if (value != _anyUnread)
               {
                   _anyUnread = value;
                   OnPropertyChanged(() => AnyUnread);
               }
           }
       }




    }
}