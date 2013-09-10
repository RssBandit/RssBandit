﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IEControl;

namespace RssBandit.WinGui.Dialogs
{
    public partial class FacebookConnectDialog : Form
    {
        public static readonly string FbLoginUrlTemplate = "http://www.facebook.com/login.php?api_key={0}&&v=1.0&auth_token={1}&popup";
        public static readonly string FbPermissionsUrlTemplate = "http://www.facebook.com/authorize.php?api_key={0}&v=1.0&ext_perm={1}&popup";
        public static readonly string ApiKey = "2d8ab36a639b61dd7a1a9dab4f7a0a5a";
        public static readonly string TokenUrl = "http://www.25hoursaday.com/weblog/CreateFBtoken.aspx";

        /// <summary>
        /// Indicates that the user has authorized RSS Bandit to access their news feed. 
        /// </summary>
        private bool authorizationComplete = false; 

        /// <summary>
        /// Browser object used to navigate to Facebook Connect dialog
        /// </summary>
        private HtmlControl browserFB;
        
        /// <summary>
        /// Initializes the dialog with the location from which the user should login to Facebook from.
        /// </summary>
        /// <param name="fbconnectUrl">The URL to the Facebook login page</param>
        public FacebookConnectDialog(Uri fbconnectUrl): this() 
        {
            this.browserFB.Navigate(fbconnectUrl.OriginalString);
        }

        private FacebookConnectDialog()
        {
            InitializeComponent();
			
			this.browserFB.NavigateComplete += this.FacebookConnectDialog_Navigated;
			this.browserFB.OnQuit += this.FacebookConnectDialog_CloseWebBrowserRequest;
            
        }


        /// <summary>
        /// Confirm that we login/connection was successful.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FacebookConnectDialog_Navigated(object sender, BrowserNavigateComplete2Event e)
        {
	        var url = new Uri(e.url, UriKind.Absolute);
            if (url.PathAndQuery.Contains("desktopapp.php"))
            {                 
                base.DialogResult = DialogResult.OK;
            }
            else if (url.PathAndQuery.Contains("authorize.php"))
            {
                authorizationComplete = true;               
            }
        }

	    private void FacebookConnectDialog_CloseWebBrowserRequest(object sender, EventArgs e)
	    {
		    authorizationComplete = false;
			Close();
	    }

        /// <summary>
        /// Handle user canceling the form instead of completing sign-in/connect process. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FacebookConnectDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
             if (authorizationComplete)
            {
                base.DialogResult = DialogResult.OK;
            }
             else if (base.DialogResult != DialogResult.OK)
             {
                 base.DialogResult = DialogResult.Cancel;
             }
            
        }




    }
}