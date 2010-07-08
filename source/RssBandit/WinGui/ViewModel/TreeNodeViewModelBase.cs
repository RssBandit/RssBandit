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

namespace RssBandit.WinGui.ViewModel
{
    public abstract class TreeNodeViewModelBase : ViewModelBase
    {
        private ObservableCollection<TreeNodeViewModelBase> _children = new ObservableCollection<TreeNodeViewModelBase>();
        private bool _isExpanded;
        
        protected CategorizedFeedSourceViewModel BaseFeedSource;
        protected TreeNodeViewModelBase BaseParent;
        protected string BaseImage;
        
        public abstract string Name
        {
            get;
            set;
        }

        public virtual string Image
        {
            get { return BaseImage; }
            set
            {
                BaseImage = value;
                OnPropertyChanged("Image");
            }
        }

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


        public virtual ObservableCollection<TreeNodeViewModelBase> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        public virtual TreeNodeViewModelBase Parent
        {
            get { return BaseParent; }
            set { BaseParent = value; }
        }

        public abstract string Category
        {
            get; set;
        }

        public virtual CategorizedFeedSourceViewModel Source
        {
            get { return BaseFeedSource; }
        }

        public FeedNodeType Type
        {
            get;
            protected set; 
        }
    }
}