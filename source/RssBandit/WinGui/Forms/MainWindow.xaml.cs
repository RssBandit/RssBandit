using System;
using System.Collections.Generic;
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
using Infragistics.Windows.Ribbon;
using RssBandit.WinGui.Commands;

namespace RssBandit.WinGui.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += delegate
            {
                Splash.Close();
                this.Loaded -= delegate { };
            };
            
        }

        /// <summary>
        /// OnThemeSelected switches the theme depending upon the Tag name 
        /// specified in the ButtonTool that calls this method (parameter contains that value).
        /// There is a direct correlation between the ThemeName and the name 
        /// specified in the "Tag" property.  The theme name is case sensitive.
        /// </summary>
        private void OnThemeSelected(object sender, ExecutedRoutedEventArgs e)
        {
            this.xamRibbon.Theme = e.Parameter as string;
        }
        
    }
}
