using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NewsComponents.Net;

namespace RssBandit.Converters
{
    class FileNameToIconConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        private readonly ImageSource _errorIcon;

        public FileNameToIconConverter()
        {
            _errorIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/error.png"));
            _errorIcon.Freeze();    
        }


        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(ImageSource))
                throw new ArgumentException();

            if (value == null || value.Length != 2)
                return null;
            if (value[1] != null)
            {
                var state = (DownloadTaskState) value[1];
                if (state == DownloadTaskState.DownloadError)
                    return _errorIcon;
            }

            var fileName = value[0] as string;
            if(fileName != null)
            {
                return Win32.GetShellIconForFile(fileName);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
