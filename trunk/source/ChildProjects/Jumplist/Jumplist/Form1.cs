using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Shell;

namespace Jumplist
{
    public partial class Form1 : Form
    {
        private ThumbnailToolbarButton buttonAdd;
        private ThumbnailToolbarButton buttonRefresh;
        private PictureBox pictureBox1;
        private JumpListCustomCategory category1 = new JumpListCustomCategory("Recent");
        private JumpListCustomCategory category2 = new JumpListCustomCategory("Tasks");
        private JumpList jumpList;        
        private TaskbarManager windowsTaskbar = TaskbarManager.Instance;

        public Form1()
        {
            InitializeComponent();

            if (!TaskbarManager.IsPlatformSupported)
            {
                MessageBox.Show("This demo requires Windows 7 to run", "Demo needs Windows 7", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);
                return;
                
            }

            this.Shown += new System.EventHandler(Form1_Shown);
            windowsTaskbar.ApplicationId = "RssBandit"; 
        }

        void Form1_Shown(object sender, System.EventArgs e)
        {                

            //thumbnail toolbar button setup
            buttonAdd = new ThumbnailToolbarButton(Properties.Resources.feed, "Add New Subscription");
            buttonAdd.Enabled = true;
            buttonAdd.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(buttonAdd_Click);

            buttonRefresh = new ThumbnailToolbarButton(Properties.Resources.feedRefresh, "Refresh Feeds");
            buttonRefresh.Enabled = true;
            buttonRefresh.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(buttonRefresh_Click);

            TaskbarManager.Instance.ThumbnailToolbars.AddButtons(this.Handle, buttonAdd, buttonRefresh);                     
            TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(this.Handle, new Rectangle(pictureBox1.Location, pictureBox1.Size));

            string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);

            //setup jumplist
            jumpList = JumpList.CreateJumpList();
            category1.AddJumpListItems(new JumpListLink(Path.Combine(systemFolder, "notepad.exe"), "SteveSi's Office Hours")); 
            category2.AddJumpListItems(new JumpListLink(Path.Combine(systemFolder, "mspaint2.exe"), "Add Google Reader Feeds"));
            category2.AddJumpListItems(new JumpListLink(Path.Combine(systemFolder, "mspaint3.exe"), "Add Facebook News Feed"));
            category2.AddJumpListItems(new JumpListLink(Path.Combine(systemFolder, "mspaint4.exe"), "Refresh Feeds"));
            jumpList.AddCustomCategories(category1, category2);           
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            jumpList.Refresh(); 
        }

        void buttonAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Adding a new subscription"); 
        }

        void buttonRefresh_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Refreshing feeds");
        }
    }
}
