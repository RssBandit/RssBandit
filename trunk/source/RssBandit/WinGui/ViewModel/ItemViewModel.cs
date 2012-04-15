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
using System.Diagnostics;
using System.Linq;

using NewsComponents;

namespace RssBandit.WinGui.ViewModel
{
    [DebuggerDisplay("Title = {Title}, Category = {Category}")]
    public class ItemViewModel: ItemViewModelBase
    {
        private readonly INewsItem _item;
        
        public ItemViewModel(INewsItem item)
        {
            _item = item;
            Children = new ReadOnlyObservableCollection<ItemViewModelBase>(p_children);

        }

        public ReadOnlyObservableCollection<ItemViewModelBase> Children { get; private set; }
        

        public string Title
        {
            get { return _item.Title; }
        }

        public string Category
        {
            get { return _item.Subject; }
        }

        public DateTime Published
        {
            get { return _item.Date; }
        }
    }
}
