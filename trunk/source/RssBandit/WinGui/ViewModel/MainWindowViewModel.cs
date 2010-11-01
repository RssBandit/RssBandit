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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Infragistics.Windows.Ribbon;
using RssBandit.AppServices;
using RssBandit.AppServices.Util;
using RssBandit.Util;
using RssBandit.WinGui.Behaviors;
using RssBandit.WinGui.Commands;

namespace RssBandit.WinGui.ViewModel
{
    public class MainWindowViewModel : ApplicationViewModel
    {
        private RelayCommand _appOptionsCommand;
        private string _currentTheme;
        
        private QuickAccessToolbarLocation _quickAccessToolbarLocation = QuickAccessToolbarLocation.AboveRibbon;
        private bool _toolbarIsMinimized;

        private PropertyObserver<IApplicationContext> _contextObserver;

        public string DisplayName { get; private set; }
        public MainWindowViewModel(RssBanditApplication current, IShellLayoutManager layoutManager)
            : base(current)
        {
            // init layout manager (adapter); that raise property changed and inject the XamDockManager instance into the adapter:
            LayoutManager = layoutManager;

            DisplayName = RssBanditApplication.CaptionOnly;
            // assign the recent selected theme, or the default:
            CurrentTheme = RssBanditApplication.Current.GuiSettings.GetString("theme", Win32.IsOSAtLeastWindows7 ? "Scenic" : "Office2k7Black");
            // get recent QAT location:
            QuickAccessToolbarLocation = (QuickAccessToolbarLocation) Enum.Parse(typeof (QuickAccessToolbarLocation),
                                                                                 RssBanditApplication.Current.GuiSettings.GetString("quickAccessToolbarLocation", "AboveRibbon"));
            // was toolbar minimized?
            ToolbarIsMinimized = Boolean.Parse(RssBanditApplication.Current.GuiSettings.GetString("toolbarIsMinimized", "false"));

            _contextObserver = PropertyObserver.Create(RssBanditApplication.Current.Context);

            _contextObserver.RegisterHandler(c => c.SelectedNode, c => OnTreeModelSelectionChanged(c.SelectedNode));
        }

        private IShellLayoutManager _shellLayoutManager;

        public IShellLayoutManager LayoutManager
        {
            get { return _shellLayoutManager; }
            set 
            {
                _shellLayoutManager = value;
                OnPropertyChanged(() => LayoutManager);
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to show the app options dialog.
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


        /// <summary>
        ///   Gets the current theme.
        /// </summary>
        /// <value>The current theme.</value>
        public string CurrentTheme
        {
            get { return _currentTheme; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    return;

                if (_currentTheme == value)
                    return;

                _currentTheme = value;

                // apply theme to any windows forms IG control(s), we might still use:
                //ThemeManager.CurrentTheme = value;

                // save to settings:
                RssBanditApplication.Current.GuiSettings.SetProperty("theme", value);

                if ("Scenic".Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    // display no icon/icon, but the caption:
                    //RssBanditApplication.MainWindow.xamRibbon.ApplicationMenu.Image = null;
                    ApplicationMenuImage = null;
                    // HACK: tweaks for the windows 7 scenic theme vs. Office themes (Infragistics or MS? has issues
                    // if we keep the App.ico with all the embedded resolutions and "oversize" the app icon displayed in Scenic/Windows 7 theme):
                    ApplicationIcon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Small.App.ico"));
                }
                else
                {
                    // redisplay the full app menu icon:
                    //RssBanditApplication.MainWindow.xamRibbon.ApplicationMenu.Image = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Toolbar/rssbandit.32.png"));
                    ApplicationMenuImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Toolbar/rssbandit.32.png")); 
                    // HACK: restore the App.ico with all the embedded resolutions:
                    ApplicationIcon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/App.ico"));
                }

                OnPropertyChanged(() => CurrentTheme);
            }
        }

        private ImageSource _applicationMenuImage;
        public ImageSource ApplicationMenuImage
        {
            get { return _applicationMenuImage; }
            private set
            {
                _applicationMenuImage = value;

                OnPropertyChanged(() => ApplicationMenuImage);
            }
        }

        private ImageSource _applicationIcon;
        public ImageSource ApplicationIcon
        {
            get { return _applicationIcon; }
            private set
            {
                _applicationIcon = value;
                OnPropertyChanged(() => ApplicationIcon);
            }
        }

       

        /// <summary>
        ///   Gets or sets the quick access toolbar location (XAML bound).
        /// </summary>
        /// <value>The quick access toolbar location.</value>
        public QuickAccessToolbarLocation QuickAccessToolbarLocation
        {
            get { return _quickAccessToolbarLocation; }
            set
            {
                if (_quickAccessToolbarLocation != value)
                {
                    _quickAccessToolbarLocation = value;
                    RssBanditApplication.Current.GuiSettings.SetProperty("quickAccessToolbarLocation", value);
                    OnPropertyChanged(() => QuickAccessToolbarLocation);
                }
            }
        }

        private RibbonContext _currentRibbonContext = RibbonContext.Home;
        /// <summary>
        ///   Gets or sets the selected menu band (XAML bound).
        /// </summary>
        /// <value>The selected menu band.</value>
        public RibbonContext CurrentRibbonContext
        {
            get { return _currentRibbonContext; }
            set
            {
                if (_currentRibbonContext != value)
                {
                    _currentRibbonContext = value;
                    OnPropertyChanged(() => CurrentRibbonContext);
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether [toolbar is minimized] (XAML bound).
        /// </summary>
        /// <value><c>true</c> if [toolbar is minimized]; otherwise, <c>false</c>.</value>
        public bool ToolbarIsMinimized
        {
            get { return _toolbarIsMinimized; }
            set
            {
                if (_toolbarIsMinimized != value)
                {
                    _toolbarIsMinimized = value;
                    RssBanditApplication.Current.GuiSettings.SetProperty("toolbarIsMinimized", value);
                    OnPropertyChanged(() => ToolbarIsMinimized);
                }
            }
        }
        
        private void OnTreeModelSelectionChanged(ITreeNodeViewModelBase sender)
        {
            if (sender.IsSelected)
            {
                if (sender is FeedViewModel)
                {
                    CurrentRibbonContext = RibbonContext.Feed;
                }
                if (sender is FolderViewModel)
                {
                    CurrentRibbonContext = RibbonContext.Folder;
                }
                if (sender is CategorizedFeedSourceViewModel)
                {
                    CurrentRibbonContext = RibbonContext.Home;
                }
            }
        }

        private static void ShowAppOptions()
        {
            RssBanditApplication.Current.ShowOptions(OptionDialogSection.Default, RssBanditApplication.MainWindow32, null);
        }
    }

    public enum RibbonContext
    {
        Feed,
        Folder,
        Home
    }
}