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
using Infragistics.Windows.Themes;

namespace RssBandit.WinGui.ViewModel
{
    public class MainWindowViewModel : ApplicationViewModel
    {
        public MainWindowViewModel()
        {
            base.DisplayName = RssBanditApplication.CaptionOnly;
            // assign the rencent selected theme, or the default:
            CurrentTheme = RssBanditApplication.Current.GuiSettings.GetString("theme", "Office2k7Black");
        }

        /// <summary>
        /// Gets the current theme (for binding).
        /// </summary>
        /// <value>The current theme.</value>
        public string CurrentTheme
        {
            get { return ThemeManager.CurrentTheme; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    return;

                // apply theme to any windows forms IG control(s), we might still use:
                ThemeManager.CurrentTheme = value;
                // apply theme to WPF IG controls:
                RssBanditApplication.MainWindow.xamRibbon.Theme = value;
                // save to settings:
                RssBanditApplication.Current.GuiSettings.SetProperty("theme", value);
            }
        }
      
    }
}