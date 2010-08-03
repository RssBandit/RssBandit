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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Infragistics.Windows.Ribbon;
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
            CurrentTheme = RssBanditApplication.Current.GuiSettings.GetString("theme", Win32.IsOSAtLeastWindows7 ? "Scenic" : "Office2k7Black");
            // get recent QAT location:
            QuickAccessToolbarLocation = (QuickAccessToolbarLocation)Enum.Parse(typeof(QuickAccessToolbarLocation),
                RssBanditApplication.Current.GuiSettings.GetString("quickAccessToolbarLocation", "AboveRibbon"));
            // wqs toolbar minimized?
            ToolbarIsMinimized = Boolean.Parse(RssBanditApplication.Current.GuiSettings.GetString("toolbarIsMinimized", "false"));
        }


        /// <summary>
        /// Gets or sets the feed sources (XAML bound).
        /// </summary>
        /// <value>The feed sources.</value>
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

        private string _currentTheme;
        /// <summary>
        /// Gets the current theme.
        /// </summary>
        /// <value>The current theme.</value>
        public string CurrentTheme
        {
            get
            {
                return _currentTheme;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    return;
                
                if (_currentTheme == value)
                    return;

                _currentTheme = value;
                
                // apply theme to any windows forms IG control(s), we might still use:
                ThemeManager.CurrentTheme = value;
                //// apply theme to WPF IG controls:
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

        /// <summary>
        /// Gets or sets the selected menu band (XAML bound).
        /// </summary>
        /// <value>The selected menu band.</value>
        public RibbonTabItem SelectedMenuBand
        {
            get
            {
                return RssBanditApplication.MainWindow.xamRibbon.SelectedTab;
            }
            set
            {
                if (RssBanditApplication.MainWindow.xamRibbon.SelectedTab != value)
                {
                    RssBanditApplication.MainWindow.xamRibbon.SelectedTab = value;
                    OnPropertyChanged("SelectedMenuBand");
                }
            }
        }

        private QuickAccessToolbarLocation _quickAccessToolbarLocation = QuickAccessToolbarLocation.AboveRibbon;
        /// <summary>
        /// Gets or sets the quick access toolbar location (XAML bound).
        /// </summary>
        /// <value>The quick access toolbar location.</value>
        public QuickAccessToolbarLocation QuickAccessToolbarLocation
        {
            get
            {
                return _quickAccessToolbarLocation;
            }
            set
            {
                if (_quickAccessToolbarLocation != value)
                {
                    _quickAccessToolbarLocation = value;
                    RssBanditApplication.Current.GuiSettings.SetProperty("quickAccessToolbarLocation", value);
                    OnPropertyChanged("QuickAccessToolbarLocation");
                }
            }
        }

        private bool _toolbarIsMinimized;
        
        /// <summary>
        /// Gets or sets a value indicating whether [toolbar is minimized] (XAML bound).
        /// </summary>
        /// <value><c>true</c> if [toolbar is minimized]; otherwise, <c>false</c>.</value>
        public bool ToolbarIsMinimized
        {
            get
            {
                return _toolbarIsMinimized;
            }
            set
            {
                if (_toolbarIsMinimized != value)
                {
                    _toolbarIsMinimized = value;
                    RssBanditApplication.Current.GuiSettings.SetProperty("toolbarIsMinimized", value);
                    OnPropertyChanged("ToolbarIsMinimized");
                }
            }
        }

        internal void OnTreeModelSelectionChanged(TreeNodeViewModelBase sender, bool selected)
        {
            if (selected)
            {
                if (sender is FeedViewModel)
                    ActivateContextMenuBand("mnuBandFeed");
                if (sender is FolderViewModel)
                    ActivateContextMenuBand("mnuBandFolder");
                if (sender is CategorizedFeedSourceViewModel)
                    ActivateContextMenuBand("mnuBandHome");
            }
        }

        internal void ActivateContextMenuBand(string name)
        {

            RibbonTabItem byName = RssBanditApplication.MainWindow.xamRibbon.FindName(name) as RibbonTabItem;
            if (byName == null)
                throw new ArgumentException("No such menu band with name: " + name, "name");

            if (RssBanditApplication.MainWindow.xamRibbon.SelectedTab == byName)
                return;

            
            foreach (RibbonTabItem ctg in RssBanditApplication.MainWindow.xamRibbon.Tabs)
                if (ctg.ContextualTabGroup != null)
                    ctg.Visibility = Visibility.Collapsed;

            byName.BringIntoView();
            SelectedMenuBand = byName;
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