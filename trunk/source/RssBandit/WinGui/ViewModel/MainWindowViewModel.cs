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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Infragistics.Windows.Themes;
using RssBandit.WinGui.Commands;

namespace RssBandit.WinGui.ViewModel
{
    public class MainWindowViewModel : ApplicationViewModel
    {
        private FeedSourcesViewModel _feedSources;

        public MainWindowViewModel()
        {
            base.DisplayName = RssBanditApplication.CaptionOnly;
            // assign the rencent selected theme, or the default:
            CurrentTheme = RssBanditApplication.Current.GuiSettings.GetString("theme", Win32.IsOSAtLeastWindows7 ? "Scenic": "Office2k7Black");
        }


        public FeedSourcesViewModel FeedSources
        {
            get
            {
                if (_feedSources == null)
                    _feedSources = new FeedSourcesViewModel();
                return _feedSources;
            }
            set { _feedSources = value; }
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

                if ("Scenic".Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    // display no icon/icon, but the caption:
                    RssBanditApplication.MainWindow.xamRibbon.ApplicationMenu.Image = null;
                    // HACK: tweaks for the windows 7 scenic theme vs. Office themes (Infragistics or MS? has issues
                    // if we keep the App.ico with all the embedded resolutions and "oversize" the app icon displayed in Scenic/Windows 7 theme):
                    RssBanditApplication.MainWindow.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Small.App.ico"));
                }
                else
                {
                    // redisplay the full app menu icon:
                    RssBanditApplication.MainWindow.xamRibbon.ApplicationMenu.Image = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Toolbar/rssbandit.32.png"));
                    // HACK: restore the App.ico with all the embedded resolutions:
                    RssBanditApplication.MainWindow.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/App.ico"));
                }

                OnPropertyChanged("CurrentTheme");
            }
        }

        #region App Options Command

        RelayCommand _appOptionsCommand;

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to show the app options dialog.
        /// </summary>
        public ICommand AppOptionsCommand
        {
            get
            {
                if (_appOptionsCommand == null)
                    _appOptionsCommand = new RelayCommand(param => ShowAppOptions(), param => CanShowAppOptions);

                return _appOptionsCommand;
            }
        }
        public bool CanShowAppOptions
        {
            get { return true; }
        }

        static void ShowAppOptions()
        {
            RssBanditApplication.Current.ShowOptions(OptionDialogSection.Default, RssBanditApplication.MainWindow32, null);
        }


        #endregion
    }
}