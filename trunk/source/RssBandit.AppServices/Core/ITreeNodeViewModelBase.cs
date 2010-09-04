using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RssBandit.AppServices
{
    public interface ITreeNodeViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        ///   The category of the node.
        /// </summary>
        string Category { get; set; }

        /// <summary>
        ///   Indicates whether the tree node is expanded
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        ///   Indicates whether the tree node is selected
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        ///   The name of the tree node
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///   AnyUnread and UnreadCount are working interconnected:
        ///   if you set UnreadCount to non zero, AnyUnread will be set to true and
        ///   then updates the visualized info to use the Unread Font and 
        ///   read counter state info. If UnreadCount is set to zero,
        ///   also AnyUnread is reset to false and refresh the caption to default.
        /// </summary>
        int UnreadCount { get; set; }
    }
}
