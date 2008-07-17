using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using NewsComponents.Net;
using RssBandit.Resources;

namespace RssBandit.WinGui.Dialogs
{
    /// <summary>
    /// Interaction logic for DownloadManagerWindow.xaml
    /// </summary>
    public partial class DownloadManagerWindow : Window
    {
    	private RssBanditApplication owner;
        
		public DownloadManagerWindow()
        {
            InitializeComponent();
        }
		internal DownloadManagerWindow(RssBanditApplication app) :this()
		{
			owner = app;
		}

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            foreach(var task in DownloadRegistryManager.Current.GetTasks())
            {
                // skip in-progress downloads
                if (task.State == DownloadTaskState.Downloading)
                    continue;
                
                DownloadRegistryManager.Current.UnRegisterTask(task);
            }
        }

        private void Grid_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
			try
			{
				Process.Start(new ProcessStartInfo(e.Uri.ToString()) {UseShellExecute = true});
				e.Handled = true;
			} 
			catch (Exception startupEx)
			{
				owner.MessageWarn(String.Format(SR.ExecutableForFileStartupFailure, e.Uri, startupEx.Message));
			}
        }
    }
}
