using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace RssBandit.Converters
{
    public static class CommonConverters
    {
        static CommonConverters()
        {
            NodesSortedByCategoryTitle = new NodesSortedByCategoryTitleConverter();
        }

        public static IValueConverter NodesSortedByCategoryTitle { get; private set; }
    }
}
