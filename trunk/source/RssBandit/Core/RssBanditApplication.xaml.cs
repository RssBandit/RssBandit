using System;
using System.Windows;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.ViewModel;

namespace RssBandit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class RssBanditApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // we cannot use the App.xaml StartupUri xaml code to instanciate the MainWindow,
            // because we have to init the ViewModel DataContext binding:

            MainWindow window = new MainWindow();

            // Create the ViewModel to which the main window binds.
            var viewModel = new MainWindowViewModel();

            
            // When the ViewModel asks to be closed, close the window.
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                //TODO: how about close to systray?
                window.Close();
                this.CmdExitApp(null);
                
            };
            viewModel.RequestClose += handler;
            
            
            // Allow all controls in the window to bind to the ViewModel by setting the 
            // DataContext, which propagates down the element tree.
            window.DataContext = viewModel;

            base.MainWindow = window;

            window.Show();
        }

      
    }
}
