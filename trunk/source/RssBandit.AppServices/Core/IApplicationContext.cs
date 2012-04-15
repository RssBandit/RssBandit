using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RssBandit.AppServices
{
    public interface IApplicationContext : INotifyPropertyChanged
    {
        ITreeNodeViewModelBase SelectedNode { get; }
        IItemViewModelBase SelectedItem { get; }
    }
}
