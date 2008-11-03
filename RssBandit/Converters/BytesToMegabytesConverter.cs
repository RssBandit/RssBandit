using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace RssBandit.Converters
{
    internal sealed class BytesToMegabytesConverter : IValueConverter
    {
        private const double bytesPerMb = 1024 * 1024;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value != null && value is long)
            {
                var size = (long)value;

                double mb = size / bytesPerMb;

                return string.Format(culture, "{0:n2} MB", mb);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
