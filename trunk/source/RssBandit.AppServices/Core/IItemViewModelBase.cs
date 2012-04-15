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
using System.ComponentModel;
using System.Linq;

namespace RssBandit.AppServices
{
    public interface IItemViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        ///   Indicates whether the item node is expanded
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        ///   Indicates whether the item node is selected
        /// </summary>
        bool IsSelected { get; set; }

    }
}
