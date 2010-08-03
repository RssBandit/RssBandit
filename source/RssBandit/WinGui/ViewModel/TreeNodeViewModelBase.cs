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
using RssBandit.WinGui.Interfaces;
using System;

namespace RssBandit.WinGui.ViewModel
{
    public abstract class TreeNodeViewModelBase : ViewModelBase
    {
        private ObservableCollection<TreeNodeViewModelBase> _children = new ObservableCollection<TreeNodeViewModelBase>();
        private bool _isExpanded;
        private bool _isSelected;
        private bool _editable, _anyUnread, _anyNewComments;
        protected bool m_hasCustomIcon;
        private int _unreadCount;        
        
        protected CategorizedFeedSourceViewModel BaseFeedSource;
        protected TreeNodeViewModelBase BaseParent;
        protected string BaseImage;


        /// <summary>
        /// Gets raised, if the node's read counter reach zero
        /// </summary>
        public event EventHandler ReadCounterZero;
	

        /// <summary>
        /// The name of the tree node
        /// </summary>
        public abstract string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The image that represents the current state of the tree node
        /// </summary>
        public virtual string Image
        {
            get { return BaseImage; }
            set
            {
                BaseImage = value;
                OnPropertyChanged("Image");
            }
        }

        /// <summary>
        /// Indicates whether the tree node is expanded
        /// </summary>
        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        /// <summary>
        /// Indicates whether the tree node is selected
        /// </summary>
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                    RssBanditApplication.MainWindow.Model.OnTreeModelSelectionChanged(this, value);
                }
            }
        }

        /// <summary>
        /// The child nodes
        /// </summary>
        public virtual ObservableCollection<TreeNodeViewModelBase> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        /// <summary>
        /// The parent node
        /// </summary>
        public virtual TreeNodeViewModelBase Parent
        {
            get { return BaseParent; }
            set { BaseParent = value; }
        }

        /// <summary>
        /// The category of the node. 
        /// </summary>
        public abstract string Category
        {
            get; set;
        }

        /// <summary>
        /// The owning feed source view model
        /// </summary>
        public virtual CategorizedFeedSourceViewModel Source
        {
            get { return BaseFeedSource; }
        }


        /// <summary>
        /// Indicates the type of the node
        /// </summary>
        public FeedNodeType Type
        {
            get;
            protected set; 
        }


        /// <summary>
        /// Callback invoked to indicate notify the UI that this item has no more unread items
        /// </summary>
        private void RaiseReadCounterZero()
        {
            if (this.ReadCounterZero != null)
                this.ReadCounterZero(this, EventArgs.Empty);
        }


        /// <summary>
        /// AnyUnread and UnreadCount are working interconnected:
        /// if you set UnreadCount to non zero, AnyUnread will be set to true and
        /// then updates the visualized info to use the Unread Font and 
        /// read counter state info. If UnreadCount is set to zero,
        /// also AnyUnread is reset to false and refresh the caption to default.
        /// </summary>
        public virtual int UnreadCount
        {
            get { return _unreadCount; }
            set
            {
                if (value != _unreadCount)
                {
                    _unreadCount = value;
                    _anyUnread = (_unreadCount > 0);

                  //  InvalidateNode();

                    if (_unreadCount == 0)
                        RaiseReadCounterZero();
                }
            }
        }
    }
}