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
using System.Linq;
using System.Text;
using RssBandit.AppServices;
using RssBandit.AppServices.Core;

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
