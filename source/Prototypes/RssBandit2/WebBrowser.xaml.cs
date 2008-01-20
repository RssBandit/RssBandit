using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using IEControl;

namespace RssBandit2
{
    /// <summary>
    /// Interaction logic for WebBrowser.xaml
    /// </summary>
    [ContentProperty("Html")]
    public partial class WebBrowser : UserControl
    {
        public WebBrowser()
        {
            InitializeComponent();

            htmlControl.EnhanceBrowserSecurityForProcess();
            HtmlControl.SetInternetFeatureEnabled(InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL, SetFeatureFlag.SET_FEATURE_ON_PROCESS, htmlControl.ActiveXEnabled);
        }

        private static void SetControlHtml(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var wb = (WebBrowser)obj;

            var str = (string)args.NewValue;

            if (string.IsNullOrEmpty(str))
                wb.htmlControl.Clear();
            else
            {
                wb.htmlControl.Html = str;
                wb.htmlControl.Navigate(null);
            }
        }


        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Html.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register("Html", typeof(string), typeof(WebBrowser), new UIPropertyMetadata(null, SetControlHtml));


    }
}
