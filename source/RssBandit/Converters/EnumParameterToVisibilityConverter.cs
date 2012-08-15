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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RssBandit.Converters
{
    class EnumParameterToVisibilityConverter : IValueConverter
    {
        // visible, if the parameter enum value == value to convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new ArgumentException();

            if (parameter != null && value != null)
            {
                var current = value.ToString();
                var equalsTo = parameter.ToString();

                if (String.Equals(current, equalsTo, StringComparison.Ordinal))
                    return Visibility.Visible;

            }

            return Visibility.Collapsed;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
