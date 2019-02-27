#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.Windows.Forms;

using RssBandit.Resources;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Summary description for AutoDiscoveredFeedsDialog.
	/// </summary>
	public class DiscoveredFeedsDialog : System.Windows.Forms.Form
	{
        private Hashtable rssFeeds;
        private System.Windows.Forms.Button btnAddFeeds;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label horizontalEdge;
        internal System.Windows.Forms.ListView listFeeds;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private DiscoveredFeedsDialog(){;}

		public DiscoveredFeedsDialog(Hashtable feeds)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.rssFeeds = feeds;		
            
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DiscoveredFeedsDialog));
			this.btnAddFeeds = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.horizontalEdge = new System.Windows.Forms.Label();
			this.listFeeds = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// btnAddFeeds
			// 
			this.btnAddFeeds.AccessibleDescription = resources.GetString("btnAddFeeds.AccessibleDescription");
			this.btnAddFeeds.AccessibleName = resources.GetString("btnAddFeeds.AccessibleName");
			this.btnAddFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnAddFeeds.Anchor")));
			this.btnAddFeeds.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAddFeeds.BackgroundImage")));
			this.btnAddFeeds.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnAddFeeds.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnAddFeeds.Dock")));
			this.btnAddFeeds.Enabled = ((bool)(resources.GetObject("btnAddFeeds.Enabled")));
			this.btnAddFeeds.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnAddFeeds.FlatStyle")));
			this.btnAddFeeds.Font = ((System.Drawing.Font)(resources.GetObject("btnAddFeeds.Font")));
			this.btnAddFeeds.Image = ((System.Drawing.Image)(resources.GetObject("btnAddFeeds.Image")));
			this.btnAddFeeds.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAddFeeds.ImageAlign")));
			this.btnAddFeeds.ImageIndex = ((int)(resources.GetObject("btnAddFeeds.ImageIndex")));
			this.btnAddFeeds.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnAddFeeds.ImeMode")));
			this.btnAddFeeds.Location = ((System.Drawing.Point)(resources.GetObject("btnAddFeeds.Location")));
			this.btnAddFeeds.Name = "btnAddFeeds";
			this.btnAddFeeds.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnAddFeeds.RightToLeft")));
			this.btnAddFeeds.Size = ((System.Drawing.Size)(resources.GetObject("btnAddFeeds.Size")));
			this.btnAddFeeds.TabIndex = ((int)(resources.GetObject("btnAddFeeds.TabIndex")));
			this.btnAddFeeds.Text = resources.GetString("btnAddFeeds.Text");
			this.btnAddFeeds.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAddFeeds.TextAlign")));
			this.btnAddFeeds.Visible = ((bool)(resources.GetObject("btnAddFeeds.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			// 
			// horizontalEdge
			// 
			this.horizontalEdge.AccessibleDescription = resources.GetString("horizontalEdge.AccessibleDescription");
			this.horizontalEdge.AccessibleName = resources.GetString("horizontalEdge.AccessibleName");
			this.horizontalEdge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("horizontalEdge.Anchor")));
			this.horizontalEdge.AutoSize = ((bool)(resources.GetObject("horizontalEdge.AutoSize")));
			this.horizontalEdge.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.horizontalEdge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("horizontalEdge.Dock")));
			this.horizontalEdge.Enabled = ((bool)(resources.GetObject("horizontalEdge.Enabled")));
			this.horizontalEdge.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.horizontalEdge.Font = ((System.Drawing.Font)(resources.GetObject("horizontalEdge.Font")));
			this.horizontalEdge.Image = ((System.Drawing.Image)(resources.GetObject("horizontalEdge.Image")));
			this.horizontalEdge.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.ImageAlign")));
			this.horizontalEdge.ImageIndex = ((int)(resources.GetObject("horizontalEdge.ImageIndex")));
			this.horizontalEdge.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("horizontalEdge.ImeMode")));
			this.horizontalEdge.Location = ((System.Drawing.Point)(resources.GetObject("horizontalEdge.Location")));
			this.horizontalEdge.Name = "horizontalEdge";
			this.horizontalEdge.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("horizontalEdge.RightToLeft")));
			this.horizontalEdge.Size = ((System.Drawing.Size)(resources.GetObject("horizontalEdge.Size")));
			this.horizontalEdge.TabIndex = ((int)(resources.GetObject("horizontalEdge.TabIndex")));
			this.horizontalEdge.Text = resources.GetString("horizontalEdge.Text");
			this.horizontalEdge.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.TextAlign")));
			this.horizontalEdge.Visible = ((bool)(resources.GetObject("horizontalEdge.Visible")));
			// 
			// listFeeds
			// 
			this.listFeeds.AccessibleDescription = resources.GetString("listFeeds.AccessibleDescription");
			this.listFeeds.AccessibleName = resources.GetString("listFeeds.AccessibleName");
			this.listFeeds.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listFeeds.Alignment")));
			this.listFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listFeeds.Anchor")));
			this.listFeeds.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listFeeds.BackgroundImage")));
			this.listFeeds.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listFeeds.Dock")));
			this.listFeeds.Enabled = ((bool)(resources.GetObject("listFeeds.Enabled")));
			this.listFeeds.Font = ((System.Drawing.Font)(resources.GetObject("listFeeds.Font")));
			this.listFeeds.FullRowSelect = true;
			this.listFeeds.HideSelection = false;
			this.listFeeds.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listFeeds.ImeMode")));
			this.listFeeds.LabelWrap = ((bool)(resources.GetObject("listFeeds.LabelWrap")));
			this.listFeeds.Location = ((System.Drawing.Point)(resources.GetObject("listFeeds.Location")));
			this.listFeeds.Name = "listFeeds";
			this.listFeeds.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listFeeds.RightToLeft")));
			this.listFeeds.Size = ((System.Drawing.Size)(resources.GetObject("listFeeds.Size")));
			this.listFeeds.TabIndex = ((int)(resources.GetObject("listFeeds.TabIndex")));
			this.listFeeds.Text = resources.GetString("listFeeds.Text");
			this.listFeeds.View = System.Windows.Forms.View.Details;
			this.listFeeds.Visible = ((bool)(resources.GetObject("listFeeds.Visible")));
			// 
			// DiscoveredFeedsDialog
			// 
			this.AcceptButton = this.btnAddFeeds;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.listFeeds);
			this.Controls.Add(this.horizontalEdge);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnAddFeeds);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "DiscoveredFeedsDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.DiscoveredFeedsDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

        private void DiscoveredFeedsDialog_Load(object sender, System.EventArgs e)
        {
            ListViewItem lv = null;
			foreach(object feedUrl in rssFeeds.Keys)
            {
				object feedinfo = rssFeeds[feedUrl];
				Type t = feedinfo.GetType();
				if (t.IsArray) {
					string[] subItems = (string[])feedinfo;
					if (subItems.GetLength(0) != listFeeds.Columns.Count) {	// additional fields
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedTitle, 80, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedDesc, 80, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionSiteUrl, 80, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedUrl, 80, HorizontalAlignment.Left);
					}
					lv = new ListViewItem((string[])feedinfo);
				} else { // obsolete, not used anymore
					if (2 != listFeeds.Columns.Count) {	// additional fields
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedTitle, 80, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedUrl, 80, HorizontalAlignment.Left);
					}
					lv = new ListViewItem(new string[]{SR.AutoDiscoveredDefaultTitle, (string)feedUrl});
				}
				lv.Tag = feedUrl;
                listFeeds.Items.Add(lv);
            }

            foreach(ListViewItem item in listFeeds.Items) {
                item.Selected = true;
            }
			foreach(ColumnHeader column in listFeeds.Columns) {
				column.Width = -2;
			}
		}

	}
}
