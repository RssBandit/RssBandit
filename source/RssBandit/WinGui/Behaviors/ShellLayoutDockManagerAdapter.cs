using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;
using NewsComponents.Utils;
using RssBandit.WinGui.Commands;

namespace RssBandit.WinGui.Behaviors
{
    public class ShellLayoutDockManagerAdapter : IShellLayoutManager
    {

        #region LayoutManager dep. property

        /// <summary>
        /// LayoutManager Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty LayoutManagerProperty =
            DependencyProperty.RegisterAttached("LayoutManager", typeof(ShellLayoutDockManagerAdapter), typeof(ShellLayoutDockManagerAdapter),
                new FrameworkPropertyMetadata((ShellLayoutDockManagerAdapter)null,
                    new PropertyChangedCallback(OnLayoutManagerChanged)));

        /// <summary>
        /// Gets the LayoutManager property. This dependency property 
        /// indicates ....
        /// </summary>
        public static object GetLayoutManager(DependencyObject d)
        {
            return d.GetValue(LayoutManagerProperty);
        }

        /// <summary>
        /// Sets the LayoutManager property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetLayoutManager(DependencyObject d, object value)
        {
            d.SetValue(LayoutManagerProperty, value);
        }

        /// <summary>
        /// Handles changes to the LayoutManager property.
        /// </summary>
        private static void OnLayoutManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShellLayoutDockManagerAdapter instance = (ShellLayoutDockManagerAdapter)e.NewValue;
            // inject XamDockManager to adapter instance:
            if (instance != null)
                instance.DockManager = (XamDockManager)d;
        }


        #endregion

        private static string _defaultLayout;
        private XamDockManager _dockManager;

        public XamDockManager DockManager
        {
            get { return _dockManager; }
            private set
            {
                if (_dockManager != null)
                {
                    _dockManager.Loaded -= OnDockManagerLoaded; 
                    _dockManager.InitializePaneContent -= OnDockManagerInitializePaneContent;
                }
                _dockManager = value;
                if (_dockManager != null)
                {
                    _dockManager.Loaded += OnDockManagerLoaded;
                    _dockManager.InitializePaneContent += OnDockManagerInitializePaneContent;
                }
            }
        }


        static void OnDockManagerInitializePaneContent(object sender, InitializePaneContentEventArgs e)
        {
            //TODO: 
            // 1. create a Win32 content container hosting a IEcontrol instance
            // 2. extract the recent URL from the e.NewPane.SerializationId and 
            // 3. initiate IEControl.Navigate(URL)...
        }

        void OnDockManagerLoaded(object sender, RoutedEventArgs e)
        {
            // get the standard layout (design time, but not the new loaded layout as the default):
            if (!_loadLayoutInProgress)
                _defaultLayout = _dockManager.SaveLayout();
        }

        #region Implementation of IShellLayoutManager

        private bool _loadLayoutInProgress;

        public void LoadLayout()
        {
            if (DockManager != null)
            {
                string layoutFile = RssBanditApplication.GetShellLayoutFileName();
                try
                {
                    if (!File.Exists(layoutFile))
                        return;

                    _loadLayoutInProgress = true;
                
                    using (Stream stream = FileHelper.OpenForRead(layoutFile))
                    {
                        DockManager.LoadLayout(stream);
                    }
                }
                catch (Exception ex)
                {
                    //TODO: localize:
                    RssBanditApplication.Current.MessageError(String.Format("Load shell layout file '{0}' failed: {1}.{2}The default view layout will be used." , layoutFile, ex.Message, Environment.NewLine));
                } 
                finally
                {
                    _loadLayoutInProgress = false;
                }
            }
        }

        public void SaveLayout()
        {
            if (DockManager != null)
            {
                string layoutFile = RssBanditApplication.GetShellLayoutFileName();

                try
                {
                    using (Stream stream = FileHelper.OpenForWrite(layoutFile))
                    {
                        DockManager.SaveLayout(stream);
                    }
                }
                catch (Exception ex)
                {
                    //TODO: localize:
                    RssBanditApplication.Current.MessageError(String.Format("Save shell layout '{0}' failed: {1}.", layoutFile, ex.Message));
                }
            }
        }

        public void ResetLayout()
        {
            if (_defaultLayout != null && DockManager != null)
            {
                DockManager.LoadLayout(_defaultLayout);
            }
        }
        #endregion

        #region Commands
        private RelayCommand _resetLayoutCommand;

        /// <summary>
        ///   Returns the command that, when invoked, attempts
        ///   to reset the window frames docking layout to the default.
        /// </summary>
        public ICommand ResetShellLayoutCommand
        {
            get { return _resetLayoutCommand ?? (_resetLayoutCommand = new RelayCommand(param => ResetLayout(), param => true)); }
        }

        #endregion
    }
}
