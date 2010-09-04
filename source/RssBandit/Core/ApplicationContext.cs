using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RssBandit.AppServices;

namespace RssBandit.Core
{
    
    class ApplicationContext : ModelBase, IApplicationContext
    {
        private ITreeNodeViewModelBase _selectedNode;
        public ITreeNodeViewModelBase SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value;
                OnPropertyChanged(() => SelectedNode);
            }
        }
    }
}
