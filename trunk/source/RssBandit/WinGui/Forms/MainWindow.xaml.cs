

using RssBandit.WinGui.Utility;
using System;
using NewsComponents;
using RssBandit.Resources;
using System.Windows.Forms;
using RssBandit.WinGui.ViewModel;
using System.Linq;
using System.Windows.Interop;
using System.Collections.Generic;

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
