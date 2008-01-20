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

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.Register("Html", typeof(string), typeof(WebBrowser),
                new UIPropertyMetadata(null, OnHtmlChanged, OnCoerceHtml));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        #region /* Static Dependency Property Implementation for Html */

        private static void OnHtmlChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            // BEST PRACTICE: Leave this code as is; use the instance override to add your functionality
            WebBrowser o = source as WebBrowser;
            if (o != null)
                o.OnHtmlChanged((string)e.OldValue, (string)e.NewValue);
        }

        private static object OnCoerceHtml(DependencyObject source, object value)
        {
            // BEST PRACTICE: Leave this code as is; use the instance override to add your functionality
            WebBrowser o = source as WebBrowser;
            if (o != null)
                return o.OnCoerceHtml((string)value);

            return value;
        }

        #endregion

        protected virtual void OnHtmlChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                htmlControl.Clear();
            else
            {
                htmlControl.Html = newValue;
                htmlControl.Navigate(null);
            }
        }

        protected virtual string OnCoerceHtml(string value)
        {
            // TODO: Implementation code
            return value;
        }
    }
}
