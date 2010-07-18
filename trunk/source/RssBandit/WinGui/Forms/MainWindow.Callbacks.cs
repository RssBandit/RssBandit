using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Interop;

namespace RssBandit.WinGui.Forms
{
    partial class MainWindow 
    {

        #region Windows 7 related event handlers

        void OnTaskBarButtonAddClicked(object sender, EventArgs e)
        {
            this.AddFeedUrlSynchronized(String.Empty);
        }


        void OnTaskBarButtonRefreshClick(object sender, EventArgs e)
        {
            this.UpdateAllFeeds(true);
        }

        private void OnPictureBoxSizeChanged(object sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this); 
            TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(helper.Handle, new Rectangle(pictureBox.Location, pictureBox.Size));
        }

        #endregion
    }
}
