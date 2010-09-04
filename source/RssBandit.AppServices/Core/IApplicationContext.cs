using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RssBandit.AppServices
{
    public interface IApplicationContext : INotifyPropertyChanged
    {
        ITreeNodeViewModelBase SelectedNode { get; }
    }
}
