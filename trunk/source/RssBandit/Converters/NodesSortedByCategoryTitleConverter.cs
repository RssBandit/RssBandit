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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;
using RssBandit.WinGui.ViewModel;

namespace RssBandit.Converters
{
    /// <summary>
    /// See this post: http://bea.stollnitz.com/blog/?p=434 why we need this converter.
    /// </summary>
    public class NodesSortedByCategoryTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<NodeViewModel> nodes = value as IEnumerable<NodeViewModel>;
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(nodes);
            lcv.CustomSort = new NodesSortedByCategoryTitleComparer();
            return lcv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        class NodesSortedByCategoryTitleComparer : IComparer<NodeViewModel>, IComparer
        {
            private readonly bool _sortAscending;

            public NodesSortedByCategoryTitleComparer()
                : this(true)
            {
            }

            public NodesSortedByCategoryTitleComparer(bool sortAscending)
            {
                this._sortAscending = sortAscending;
            }

            public int Compare(NodeViewModel x, NodeViewModel y)
            {
                if (x == null || y == null)
                    return 0;

                int reverse = (this._sortAscending ? 1 : -1);

                if (x is FolderViewModel && y is FolderViewModel)
                    return reverse * String.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
                if (x is FolderViewModel)
                    return -1;
                if (y is FolderViewModel)
                    return 1;

                return reverse * String.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
            }

            public int Compare(object x, object y)
            {
                return Compare(x as NodeViewModel, y as NodeViewModel);
            }
        }

    }
}