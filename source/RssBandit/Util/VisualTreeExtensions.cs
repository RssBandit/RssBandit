using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace RssBandit.Util
{
    public static class VisualTreeExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : class
        {
            // get parent item
            var parent = VisualTreeHelper.GetParent(child);

            // we've reached the end of the tree
            if (parent == null)
                return null;

            return parent as T ?? FindParent<T>(parent);
        }
    }
}
