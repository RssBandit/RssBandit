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
using System.Windows.Interop;
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
            CurrentTheme = RssBanditApplication.Current.GuiSettings.GetString("theme", "Office2k7Black");
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