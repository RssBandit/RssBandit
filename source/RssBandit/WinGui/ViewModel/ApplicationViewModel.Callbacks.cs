using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Interop;

namespace RssBandit.WinGui.ViewModel
{
    partial class ApplicationViewModel 
    {

        #region Windows 7 related event handlers

        void OnTaskBarButtonAddClicked(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                this.SubscribeRssFeed();
            })
           );
        }


        void OnTaskBarButtonRefreshClick(object sender, EventArgs e)
        {
            this.UpdateAllFeeds(true);
        }      

        #endregion
    }
}
