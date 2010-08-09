#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using RssBandit.WinGui.Utility;
using System;
using RssBandit.Resources;
using RssBandit.WinGui.ViewModel;

namespace RssBandit.WinGui.Forms
{ 

     
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {       

        /// <summary>
        /// Constructor initializes class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();            
            WpfWindowSerializer.Register(this, WindowStates.All);

            Loaded += delegate
            {
                Splash.Status = SR.GUIStatusRefreshConnectionState;
                // refresh the Offline menu entry checked state
                RssBanditApplication.Current.UpdateInternetConnectionState();

                // refresh the internal browser component, that does not know immediatly
                // about a still existing Offline Mode...
                Utils.SetIEOffline(RssBanditApplication.Current.InternetConnectionOffline);

                RssBanditApplication.CheckAndInitSoundEvents();

                RssBanditApplication.Current.CmdCheckForUpdates(AutoUpdateMode.OnApplicationStart);

                Splash.Close();

                this.Model.Init(); 
                this.Loaded -= delegate { };
            };

        }


        /// <summary>
        /// Returns underlying view model
        /// </summary>
        public MainWindowViewModel Model
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        /// <summary>
        /// Calls/Open the newFeedDialog on the GUI thread, if required.
        /// </summary>
        /// <param name="newFeedUrl">Feed Url to add</param>
        public void AddFeedUrlSynchronized(string newFeedUrl)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                this.Model.SubscribeRssFeed(newFeedUrl); 
            })
            );
        }
        
    }
}
