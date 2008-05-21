#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using Genghis.Windows.Forms;
using NewsComponents; 
using NewsComponents.Net;
using NewsComponents.Utils;

using RssBandit.Resources;

namespace RssBandit.WinGui.Forms
{
	public abstract class ToastNotify : Genghis.Windows.Forms.AniForm
	{
		protected ItemActivateCallback itemActivateCallback;
		protected DisplayFeedPropertiesCallback displayFeedPropertiesCallback;
		protected FeedActivateCallback feedActivateCallback;
		protected EnclosureActivateCallback enclosureActivateCallback;

		protected LinkLabel[] linkLabels = new LinkLabel[4];
		protected Label[] linkIcons = new Label[4];

		protected System.Windows.Forms.Label labelAppIcon;
		protected System.Windows.Forms.ImageList imageList1;
		protected System.Windows.Forms.Label labelCloseIcon;
		protected System.Windows.Forms.LinkLabel labelFeedInfo;
		protected System.Windows.Forms.Label label8;
		protected System.Windows.Forms.LinkLabel linkFeedProperties;
		protected System.Windows.Forms.Label labelNewItemsArrived;
		protected System.Windows.Forms.LinkLabel linkLabel1;
		protected System.Windows.Forms.LinkLabel linkLabel2;
		protected System.Windows.Forms.LinkLabel linkLabel3;
		protected System.Windows.Forms.LinkLabel linkLabel4;
		protected System.Windows.Forms.Label label1;
		protected System.Windows.Forms.Label label2;
		protected System.Windows.Forms.Label label3;
		protected System.Windows.Forms.Label label4;
		protected System.Windows.Forms.ToolTip toolTip;
		protected System.ComponentModel.IContainer components = null;

		protected void Init()
		{
			Init(null, null, null, null);
		}

		/// <summary>
		/// Init the ToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="onItemActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		protected void Init(ItemActivateCallback onItemActivateCallback, DisplayFeedPropertiesCallback onFeedPropertiesDialog){
			Init(onItemActivateCallback, onFeedPropertiesDialog, null, null); 
		}

		/// <summary>
		/// Init the ToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="onItemActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		/// <param name="onFeedActivateCallback"></param>
		protected void Init(ItemActivateCallback onItemActivateCallback, DisplayFeedPropertiesCallback onFeedPropertiesDialog, 	FeedActivateCallback onFeedActivateCallback){
			Init(onItemActivateCallback, onFeedPropertiesDialog, onFeedActivateCallback, null); 
		}

		/// <summary>
		/// Init the ToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="enclosureActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		protected void Init(EnclosureActivateCallback enclosureActivateCallback, DisplayFeedPropertiesCallback onFeedPropertiesDialog){
			Init(null, onFeedPropertiesDialog, null, enclosureActivateCallback); 
		}

		/// <summary>
		/// Init the ToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="enclosureActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		/// <param name="onFeedActivateCallback"></param>
		protected void Init(EnclosureActivateCallback enclosureActivateCallback, DisplayFeedPropertiesCallback onFeedPropertiesDialog, 	FeedActivateCallback onFeedActivateCallback){
			Init(null, onFeedPropertiesDialog, onFeedActivateCallback, enclosureActivateCallback); 
		}

		/// <summary>
		/// Init the ToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="onItemActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		/// <param name="onFeedActivateCallback"></param>
		/// <param name="enclosureActivateCallback"></param>
		protected void Init(ItemActivateCallback onItemActivateCallback, 
			DisplayFeedPropertiesCallback onFeedPropertiesDialog,
			FeedActivateCallback onFeedActivateCallback, 
			EnclosureActivateCallback enclosureActivateCallback){
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			// our "stacking" implemetation is reduced to provide only one stack of toasts only
			// not flooding the whole screen...
			this.StackMode = Genghis.Windows.Forms.StackMode.FirstAvailable;
			this.Placement = FormPlacement.Tray;
			this.linkLabels[0] = this.linkLabel1;
			this.linkLabels[1] = this.linkLabel2;
			this.linkLabels[2] = this.linkLabel3;
			this.linkLabels[3] = this.linkLabel4;
			this.linkIcons[0] = this.label1;
			this.linkIcons[1] = this.label2;
			this.linkIcons[2] = this.label3;
			this.linkIcons[3] = this.label4;

			//fix the link label size(s)
			this.linkFeedProperties.LinkArea = new LinkArea(0, this.linkFeedProperties.Text.Length);
	
			this.itemActivateCallback = onItemActivateCallback;
			this.displayFeedPropertiesCallback = onFeedPropertiesDialog;
			this.feedActivateCallback = onFeedActivateCallback;					
			this.enclosureActivateCallback = enclosureActivateCallback;
		}

		/// <summary>
		/// Init the toast display.
		/// </summary>
		/// <param name="feedName">Feed name</param>
		/// <param name="unreadItemsYetDisplayed"></param>
		/// <param name="items"></param>
		/// <returns>true, if items found to display, else false </returns>
		/// <exception cref="InvalidOperationException">If no new items was found</exception>
		public abstract bool ItemsToDisplay(string feedName, int unreadItemsYetDisplayed, IList items);

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ToastNotify));
			this.linkFeedProperties = new System.Windows.Forms.LinkLabel();
			this.labelAppIcon = new System.Windows.Forms.Label();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.labelCloseIcon = new System.Windows.Forms.Label();
			this.labelFeedInfo = new System.Windows.Forms.LinkLabel();
			this.label8 = new System.Windows.Forms.Label();
			this.labelNewItemsArrived = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.linkLabel2 = new System.Windows.Forms.LinkLabel();
			this.linkLabel3 = new System.Windows.Forms.LinkLabel();
			this.linkLabel4 = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// linkFeedProperties
			// 
			this.linkFeedProperties.AccessibleDescription = resources.GetString("linkFeedProperties.AccessibleDescription");
			this.linkFeedProperties.AccessibleName = resources.GetString("linkFeedProperties.AccessibleName");
			this.linkFeedProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkFeedProperties.Anchor")));
			this.linkFeedProperties.AutoSize = ((bool)(resources.GetObject("linkFeedProperties.AutoSize")));
			this.linkFeedProperties.BackColor = System.Drawing.Color.Transparent;
			this.linkFeedProperties.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkFeedProperties.Dock")));
			this.linkFeedProperties.Enabled = ((bool)(resources.GetObject("linkFeedProperties.Enabled")));
			this.linkFeedProperties.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkFeedProperties.Font = ((System.Drawing.Font)(resources.GetObject("linkFeedProperties.Font")));
			this.linkFeedProperties.Image = ((System.Drawing.Image)(resources.GetObject("linkFeedProperties.Image")));
			this.linkFeedProperties.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkFeedProperties.ImageAlign")));
			this.linkFeedProperties.ImageIndex = ((int)(resources.GetObject("linkFeedProperties.ImageIndex")));
			this.linkFeedProperties.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkFeedProperties.ImeMode")));
			this.linkFeedProperties.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkFeedProperties.LinkArea")));
			this.linkFeedProperties.Location = ((System.Drawing.Point)(resources.GetObject("linkFeedProperties.Location")));
			this.linkFeedProperties.Name = "linkFeedProperties";
			this.linkFeedProperties.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkFeedProperties.RightToLeft")));
			this.linkFeedProperties.Size = ((System.Drawing.Size)(resources.GetObject("linkFeedProperties.Size")));
			this.linkFeedProperties.TabIndex = ((int)(resources.GetObject("linkFeedProperties.TabIndex")));
			this.linkFeedProperties.TabStop = true;
			this.linkFeedProperties.Text = resources.GetString("linkFeedProperties.Text");
			this.linkFeedProperties.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkFeedProperties.TextAlign")));
			this.toolTip.SetToolTip(this.linkFeedProperties, resources.GetString("linkFeedProperties.ToolTip"));
			this.linkFeedProperties.Visible = ((bool)(resources.GetObject("linkFeedProperties.Visible")));
			this.linkFeedProperties.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkFeedProperties_LinkClicked);
			// 
			// labelAppIcon
			// 
			this.labelAppIcon.AccessibleDescription = resources.GetString("labelAppIcon.AccessibleDescription");
			this.labelAppIcon.AccessibleName = resources.GetString("labelAppIcon.AccessibleName");
			this.labelAppIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelAppIcon.Anchor")));
			this.labelAppIcon.AutoSize = ((bool)(resources.GetObject("labelAppIcon.AutoSize")));
			this.labelAppIcon.BackColor = System.Drawing.Color.Transparent;
			this.labelAppIcon.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelAppIcon.Dock")));
			this.labelAppIcon.Enabled = ((bool)(resources.GetObject("labelAppIcon.Enabled")));
			this.labelAppIcon.Font = ((System.Drawing.Font)(resources.GetObject("labelAppIcon.Font")));
			this.labelAppIcon.Image = ((System.Drawing.Image)(resources.GetObject("labelAppIcon.Image")));
			this.labelAppIcon.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelAppIcon.ImageAlign")));
			this.labelAppIcon.ImageIndex = ((int)(resources.GetObject("labelAppIcon.ImageIndex")));
			this.labelAppIcon.ImageList = this.imageList1;
			this.labelAppIcon.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelAppIcon.ImeMode")));
			this.labelAppIcon.Location = ((System.Drawing.Point)(resources.GetObject("labelAppIcon.Location")));
			this.labelAppIcon.Name = "labelAppIcon";
			this.labelAppIcon.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelAppIcon.RightToLeft")));
			this.labelAppIcon.Size = ((System.Drawing.Size)(resources.GetObject("labelAppIcon.Size")));
			this.labelAppIcon.TabIndex = ((int)(resources.GetObject("labelAppIcon.TabIndex")));
			this.labelAppIcon.Text = resources.GetString("labelAppIcon.Text");
			this.labelAppIcon.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelAppIcon.TextAlign")));
			this.toolTip.SetToolTip(this.labelAppIcon, resources.GetString("labelAppIcon.ToolTip"));
			this.labelAppIcon.Visible = ((bool)(resources.GetObject("labelAppIcon.Visible")));
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = ((System.Drawing.Size)(resources.GetObject("imageList1.ImageSize")));
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// labelCloseIcon
			// 
			this.labelCloseIcon.AccessibleDescription = resources.GetString("labelCloseIcon.AccessibleDescription");
			this.labelCloseIcon.AccessibleName = resources.GetString("labelCloseIcon.AccessibleName");
			this.labelCloseIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelCloseIcon.Anchor")));
			this.labelCloseIcon.AutoSize = ((bool)(resources.GetObject("labelCloseIcon.AutoSize")));
			this.labelCloseIcon.BackColor = System.Drawing.Color.Transparent;
			this.labelCloseIcon.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelCloseIcon.Dock")));
			this.labelCloseIcon.Enabled = ((bool)(resources.GetObject("labelCloseIcon.Enabled")));
			this.labelCloseIcon.Font = ((System.Drawing.Font)(resources.GetObject("labelCloseIcon.Font")));
			this.labelCloseIcon.Image = ((System.Drawing.Image)(resources.GetObject("labelCloseIcon.Image")));
			this.labelCloseIcon.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCloseIcon.ImageAlign")));
			this.labelCloseIcon.ImageIndex = ((int)(resources.GetObject("labelCloseIcon.ImageIndex")));
			this.labelCloseIcon.ImageList = this.imageList1;
			this.labelCloseIcon.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelCloseIcon.ImeMode")));
			this.labelCloseIcon.Location = ((System.Drawing.Point)(resources.GetObject("labelCloseIcon.Location")));
			this.labelCloseIcon.Name = "labelCloseIcon";
			this.labelCloseIcon.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelCloseIcon.RightToLeft")));
			this.labelCloseIcon.Size = ((System.Drawing.Size)(resources.GetObject("labelCloseIcon.Size")));
			this.labelCloseIcon.TabIndex = ((int)(resources.GetObject("labelCloseIcon.TabIndex")));
			this.labelCloseIcon.Text = resources.GetString("labelCloseIcon.Text");
			this.labelCloseIcon.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCloseIcon.TextAlign")));
			this.toolTip.SetToolTip(this.labelCloseIcon, resources.GetString("labelCloseIcon.ToolTip"));
			this.labelCloseIcon.Visible = ((bool)(resources.GetObject("labelCloseIcon.Visible")));
			this.labelCloseIcon.Click += new System.EventHandler(this.labelCloseIcon_Click);
			this.labelCloseIcon.MouseEnter += new System.EventHandler(this.labelCloseIcon_MouseEnter);
			this.labelCloseIcon.MouseLeave += new System.EventHandler(this.labelCloseIcon_MouseLeave);
			// 
			// labelFeedInfo
			// 
			this.labelFeedInfo.AccessibleDescription = resources.GetString("labelFeedInfo.AccessibleDescription");
			this.labelFeedInfo.AccessibleName = resources.GetString("labelFeedInfo.AccessibleName");
			this.labelFeedInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelFeedInfo.Anchor")));
			this.labelFeedInfo.AutoSize = ((bool)(resources.GetObject("labelFeedInfo.AutoSize")));
			this.labelFeedInfo.BackColor = System.Drawing.Color.Transparent;
			this.labelFeedInfo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelFeedInfo.Dock")));
			this.labelFeedInfo.Enabled = ((bool)(resources.GetObject("labelFeedInfo.Enabled")));
			this.labelFeedInfo.Font = ((System.Drawing.Font)(resources.GetObject("labelFeedInfo.Font")));
			this.labelFeedInfo.Image = ((System.Drawing.Image)(resources.GetObject("labelFeedInfo.Image")));
			this.labelFeedInfo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFeedInfo.ImageAlign")));
			this.labelFeedInfo.ImageIndex = ((int)(resources.GetObject("labelFeedInfo.ImageIndex")));
			this.labelFeedInfo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelFeedInfo.ImeMode")));
			this.labelFeedInfo.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("labelFeedInfo.LinkArea")));
			this.labelFeedInfo.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.labelFeedInfo.Location = ((System.Drawing.Point)(resources.GetObject("labelFeedInfo.Location")));
			this.labelFeedInfo.Name = "labelFeedInfo";
			this.labelFeedInfo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelFeedInfo.RightToLeft")));
			this.labelFeedInfo.Size = ((System.Drawing.Size)(resources.GetObject("labelFeedInfo.Size")));
			this.labelFeedInfo.TabIndex = ((int)(resources.GetObject("labelFeedInfo.TabIndex")));
			this.labelFeedInfo.TabStop = true;
			this.labelFeedInfo.Text = resources.GetString("labelFeedInfo.Text");
			this.labelFeedInfo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFeedInfo.TextAlign")));
			this.toolTip.SetToolTip(this.labelFeedInfo, resources.GetString("labelFeedInfo.ToolTip"));
			this.labelFeedInfo.UseMnemonic = false;
			this.labelFeedInfo.Visible = ((bool)(resources.GetObject("labelFeedInfo.Visible")));
			this.labelFeedInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnFeedLabelLinkClicked);
			// 
			// label8
			// 
			this.label8.AccessibleDescription = resources.GetString("label8.AccessibleDescription");
			this.label8.AccessibleName = resources.GetString("label8.AccessibleName");
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label8.Anchor")));
			this.label8.AutoSize = ((bool)(resources.GetObject("label8.AutoSize")));
			this.label8.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(192)), ((System.Byte)(128)));
			this.label8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label8.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label8.Dock")));
			this.label8.Enabled = ((bool)(resources.GetObject("label8.Enabled")));
			this.label8.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label8.Font = ((System.Drawing.Font)(resources.GetObject("label8.Font")));
			this.label8.ForeColor = System.Drawing.Color.White;
			this.label8.Image = ((System.Drawing.Image)(resources.GetObject("label8.Image")));
			this.label8.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label8.ImageAlign")));
			this.label8.ImageIndex = ((int)(resources.GetObject("label8.ImageIndex")));
			this.label8.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label8.ImeMode")));
			this.label8.Location = ((System.Drawing.Point)(resources.GetObject("label8.Location")));
			this.label8.Name = "label8";
			this.label8.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label8.RightToLeft")));
			this.label8.Size = ((System.Drawing.Size)(resources.GetObject("label8.Size")));
			this.label8.TabIndex = ((int)(resources.GetObject("label8.TabIndex")));
			this.label8.Text = resources.GetString("label8.Text");
			this.label8.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label8.TextAlign")));
			this.toolTip.SetToolTip(this.label8, resources.GetString("label8.ToolTip"));
			this.label8.Visible = ((bool)(resources.GetObject("label8.Visible")));
			// 
			// labelNewItemsArrived
			// 
			this.labelNewItemsArrived.AccessibleDescription = resources.GetString("labelNewItemsArrived.AccessibleDescription");
			this.labelNewItemsArrived.AccessibleName = resources.GetString("labelNewItemsArrived.AccessibleName");
			this.labelNewItemsArrived.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelNewItemsArrived.Anchor")));
			this.labelNewItemsArrived.AutoSize = ((bool)(resources.GetObject("labelNewItemsArrived.AutoSize")));
			this.labelNewItemsArrived.BackColor = System.Drawing.Color.Transparent;
			this.labelNewItemsArrived.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelNewItemsArrived.Dock")));
			this.labelNewItemsArrived.Enabled = ((bool)(resources.GetObject("labelNewItemsArrived.Enabled")));
			this.labelNewItemsArrived.Font = ((System.Drawing.Font)(resources.GetObject("labelNewItemsArrived.Font")));
			this.labelNewItemsArrived.Image = ((System.Drawing.Image)(resources.GetObject("labelNewItemsArrived.Image")));
			this.labelNewItemsArrived.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewItemsArrived.ImageAlign")));
			this.labelNewItemsArrived.ImageIndex = ((int)(resources.GetObject("labelNewItemsArrived.ImageIndex")));
			this.labelNewItemsArrived.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelNewItemsArrived.ImeMode")));
			this.labelNewItemsArrived.Location = ((System.Drawing.Point)(resources.GetObject("labelNewItemsArrived.Location")));
			this.labelNewItemsArrived.Name = "labelNewItemsArrived";
			this.labelNewItemsArrived.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelNewItemsArrived.RightToLeft")));
			this.labelNewItemsArrived.Size = ((System.Drawing.Size)(resources.GetObject("labelNewItemsArrived.Size")));
			this.labelNewItemsArrived.TabIndex = ((int)(resources.GetObject("labelNewItemsArrived.TabIndex")));
			this.labelNewItemsArrived.Text = resources.GetString("labelNewItemsArrived.Text");
			this.labelNewItemsArrived.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewItemsArrived.TextAlign")));
			this.toolTip.SetToolTip(this.labelNewItemsArrived, resources.GetString("labelNewItemsArrived.ToolTip"));
			this.labelNewItemsArrived.UseMnemonic = false;
			this.labelNewItemsArrived.Visible = ((bool)(resources.GetObject("labelNewItemsArrived.Visible")));
			// 
			// linkLabel1
			// 
			this.linkLabel1.AccessibleDescription = resources.GetString("linkLabel1.AccessibleDescription");
			this.linkLabel1.AccessibleName = resources.GetString("linkLabel1.AccessibleName");
			this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkLabel1.Anchor")));
			this.linkLabel1.AutoSize = ((bool)(resources.GetObject("linkLabel1.AutoSize")));
			this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
			this.linkLabel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkLabel1.Dock")));
			this.linkLabel1.Enabled = ((bool)(resources.GetObject("linkLabel1.Enabled")));
			this.linkLabel1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkLabel1.Font = ((System.Drawing.Font)(resources.GetObject("linkLabel1.Font")));
			this.linkLabel1.Image = ((System.Drawing.Image)(resources.GetObject("linkLabel1.Image")));
			this.linkLabel1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel1.ImageAlign")));
			this.linkLabel1.ImageIndex = ((int)(resources.GetObject("linkLabel1.ImageIndex")));
			this.linkLabel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkLabel1.ImeMode")));
			this.linkLabel1.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkLabel1.LinkArea")));
			this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkLabel1.Location = ((System.Drawing.Point)(resources.GetObject("linkLabel1.Location")));
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkLabel1.RightToLeft")));
			this.linkLabel1.Size = ((System.Drawing.Size)(resources.GetObject("linkLabel1.Size")));
			this.linkLabel1.TabIndex = ((int)(resources.GetObject("linkLabel1.TabIndex")));
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = resources.GetString("linkLabel1.Text");
			this.linkLabel1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel1.TextAlign")));
			this.toolTip.SetToolTip(this.linkLabel1, resources.GetString("linkLabel1.ToolTip"));
			this.linkLabel1.UseMnemonic = false;
			this.linkLabel1.Visible = ((bool)(resources.GetObject("linkLabel1.Visible")));
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
			// 
			// linkLabel2
			// 
			this.linkLabel2.AccessibleDescription = resources.GetString("linkLabel2.AccessibleDescription");
			this.linkLabel2.AccessibleName = resources.GetString("linkLabel2.AccessibleName");
			this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkLabel2.Anchor")));
			this.linkLabel2.AutoSize = ((bool)(resources.GetObject("linkLabel2.AutoSize")));
			this.linkLabel2.BackColor = System.Drawing.Color.Transparent;
			this.linkLabel2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkLabel2.Dock")));
			this.linkLabel2.Enabled = ((bool)(resources.GetObject("linkLabel2.Enabled")));
			this.linkLabel2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkLabel2.Font = ((System.Drawing.Font)(resources.GetObject("linkLabel2.Font")));
			this.linkLabel2.Image = ((System.Drawing.Image)(resources.GetObject("linkLabel2.Image")));
			this.linkLabel2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel2.ImageAlign")));
			this.linkLabel2.ImageIndex = ((int)(resources.GetObject("linkLabel2.ImageIndex")));
			this.linkLabel2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkLabel2.ImeMode")));
			this.linkLabel2.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkLabel2.LinkArea")));
			this.linkLabel2.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkLabel2.Location = ((System.Drawing.Point)(resources.GetObject("linkLabel2.Location")));
			this.linkLabel2.Name = "linkLabel2";
			this.linkLabel2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkLabel2.RightToLeft")));
			this.linkLabel2.Size = ((System.Drawing.Size)(resources.GetObject("linkLabel2.Size")));
			this.linkLabel2.TabIndex = ((int)(resources.GetObject("linkLabel2.TabIndex")));
			this.linkLabel2.TabStop = true;
			this.linkLabel2.Text = resources.GetString("linkLabel2.Text");
			this.linkLabel2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel2.TextAlign")));
			this.toolTip.SetToolTip(this.linkLabel2, resources.GetString("linkLabel2.ToolTip"));
			this.linkLabel2.UseMnemonic = false;
			this.linkLabel2.Visible = ((bool)(resources.GetObject("linkLabel2.Visible")));
			this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
			// 
			// linkLabel3
			// 
			this.linkLabel3.AccessibleDescription = resources.GetString("linkLabel3.AccessibleDescription");
			this.linkLabel3.AccessibleName = resources.GetString("linkLabel3.AccessibleName");
			this.linkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkLabel3.Anchor")));
			this.linkLabel3.AutoSize = ((bool)(resources.GetObject("linkLabel3.AutoSize")));
			this.linkLabel3.BackColor = System.Drawing.Color.Transparent;
			this.linkLabel3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkLabel3.Dock")));
			this.linkLabel3.Enabled = ((bool)(resources.GetObject("linkLabel3.Enabled")));
			this.linkLabel3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkLabel3.Font = ((System.Drawing.Font)(resources.GetObject("linkLabel3.Font")));
			this.linkLabel3.Image = ((System.Drawing.Image)(resources.GetObject("linkLabel3.Image")));
			this.linkLabel3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel3.ImageAlign")));
			this.linkLabel3.ImageIndex = ((int)(resources.GetObject("linkLabel3.ImageIndex")));
			this.linkLabel3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkLabel3.ImeMode")));
			this.linkLabel3.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkLabel3.LinkArea")));
			this.linkLabel3.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkLabel3.Location = ((System.Drawing.Point)(resources.GetObject("linkLabel3.Location")));
			this.linkLabel3.Name = "linkLabel3";
			this.linkLabel3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkLabel3.RightToLeft")));
			this.linkLabel3.Size = ((System.Drawing.Size)(resources.GetObject("linkLabel3.Size")));
			this.linkLabel3.TabIndex = ((int)(resources.GetObject("linkLabel3.TabIndex")));
			this.linkLabel3.TabStop = true;
			this.linkLabel3.Text = resources.GetString("linkLabel3.Text");
			this.linkLabel3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel3.TextAlign")));
			this.toolTip.SetToolTip(this.linkLabel3, resources.GetString("linkLabel3.ToolTip"));
			this.linkLabel3.UseMnemonic = false;
			this.linkLabel3.Visible = ((bool)(resources.GetObject("linkLabel3.Visible")));
			this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
			// 
			// linkLabel4
			// 
			this.linkLabel4.AccessibleDescription = resources.GetString("linkLabel4.AccessibleDescription");
			this.linkLabel4.AccessibleName = resources.GetString("linkLabel4.AccessibleName");
			this.linkLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkLabel4.Anchor")));
			this.linkLabel4.AutoSize = ((bool)(resources.GetObject("linkLabel4.AutoSize")));
			this.linkLabel4.BackColor = System.Drawing.Color.Transparent;
			this.linkLabel4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkLabel4.Dock")));
			this.linkLabel4.Enabled = ((bool)(resources.GetObject("linkLabel4.Enabled")));
			this.linkLabel4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkLabel4.Font = ((System.Drawing.Font)(resources.GetObject("linkLabel4.Font")));
			this.linkLabel4.Image = ((System.Drawing.Image)(resources.GetObject("linkLabel4.Image")));
			this.linkLabel4.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel4.ImageAlign")));
			this.linkLabel4.ImageIndex = ((int)(resources.GetObject("linkLabel4.ImageIndex")));
			this.linkLabel4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkLabel4.ImeMode")));
			this.linkLabel4.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkLabel4.LinkArea")));
			this.linkLabel4.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkLabel4.Location = ((System.Drawing.Point)(resources.GetObject("linkLabel4.Location")));
			this.linkLabel4.Name = "linkLabel4";
			this.linkLabel4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkLabel4.RightToLeft")));
			this.linkLabel4.Size = ((System.Drawing.Size)(resources.GetObject("linkLabel4.Size")));
			this.linkLabel4.TabIndex = ((int)(resources.GetObject("linkLabel4.TabIndex")));
			this.linkLabel4.TabStop = true;
			this.linkLabel4.Text = resources.GetString("linkLabel4.Text");
			this.linkLabel4.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel4.TextAlign")));
			this.toolTip.SetToolTip(this.linkLabel4, resources.GetString("linkLabel4.ToolTip"));
			this.linkLabel4.UseMnemonic = false;
			this.linkLabel4.Visible = ((bool)(resources.GetObject("linkLabel4.Visible")));
			this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImageList = this.imageList1;
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.toolTip.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImageList = this.imageList1;
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.toolTip.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
			this.label3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.ImageAlign")));
			this.label3.ImageIndex = ((int)(resources.GetObject("label3.ImageIndex")));
			this.label3.ImageList = this.imageList1;
			this.label3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label3.ImeMode")));
			this.label3.Location = ((System.Drawing.Point)(resources.GetObject("label3.Location")));
			this.label3.Name = "label3";
			this.label3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label3.RightToLeft")));
			this.label3.Size = ((System.Drawing.Size)(resources.GetObject("label3.Size")));
			this.label3.TabIndex = ((int)(resources.GetObject("label3.TabIndex")));
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.TextAlign")));
			this.toolTip.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.label4.Image = ((System.Drawing.Image)(resources.GetObject("label4.Image")));
			this.label4.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.ImageAlign")));
			this.label4.ImageIndex = ((int)(resources.GetObject("label4.ImageIndex")));
			this.label4.ImageList = this.imageList1;
			this.label4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label4.ImeMode")));
			this.label4.Location = ((System.Drawing.Point)(resources.GetObject("label4.Location")));
			this.label4.Name = "label4";
			this.label4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label4.RightToLeft")));
			this.label4.Size = ((System.Drawing.Size)(resources.GetObject("label4.Size")));
			this.label4.TabIndex = ((int)(resources.GetObject("label4.TabIndex")));
			this.label4.Text = resources.GetString("label4.Text");
			this.label4.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.TextAlign")));
			this.toolTip.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// toolTip
			// 
			this.toolTip.ShowAlways = true;
			// 
			// ToastNotify
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.SystemColors.Control;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.BorderStyle = Genghis.Windows.Forms.BorderStyle.Raised;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.linkLabel4);
			this.Controls.Add(this.linkLabel3);
			this.Controls.Add(this.linkLabel2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.labelNewItemsArrived);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.labelFeedInfo);
			this.Controls.Add(this.labelCloseIcon);
			this.Controls.Add(this.labelAppIcon);
			this.Controls.Add(this.linkFeedProperties);
			this.Delay = 15000;
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.EndColor = System.Drawing.Color.FromArgb(((System.Byte)(112)), ((System.Byte)(163)), ((System.Byte)(239)));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ToastNotify";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Speed = 50;
			this.StartColor = System.Drawing.Color.AliceBlue;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.VisibleChanged += new System.EventHandler(this.OnVisibleChanged);
			this.MouseEnter += new System.EventHandler(this.OnMouseEnter);
			this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
			this.ResumeLayout(false);

		}
		#endregion

		protected void labelCloseIcon_Click(object sender, EventArgs e) {
			this.RequestClose();
		}

		protected void labelCloseIcon_MouseEnter(object sender, EventArgs e) {
			if (labelCloseIcon.ImageIndex != 2)
				labelCloseIcon.ImageIndex = 2;
		}

		protected void labelCloseIcon_MouseLeave(object sender, EventArgs e) {
			if (labelCloseIcon.ImageIndex != 1)
				labelCloseIcon.ImageIndex = 1;
		}

		protected void OnVisibleChanged(object sender, EventArgs e) {
			if (!Visible) {
				// remove NewsItem refs:
				this.linkLabel1.Tag = this.linkLabel2.Tag = this.linkLabel3.Tag = this.linkLabel4.Tag = null;
			}
		}

		protected abstract void linkFeedProperties_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e);

		protected abstract void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e);

		protected abstract void OnFeedLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e);		

		protected void OnMouseEnter(object sender, EventArgs e) {
			this.Activate();
		}

		protected void OnMouseLeave(object sender, EventArgs e) {
			//
		}
	}

	public class NewsItemToastNotify : ToastNotify {
		/// <summary>
		/// Default constructor calls base
		/// </summary>
		public NewsItemToastNotify()
		{}

		/// <summary>
		/// Init the NewsItemToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="onItemActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		public NewsItemToastNotify(ItemActivateCallback onItemActivateCallback, DisplayFeedPropertiesCallback onFeedPropertiesDialog){
			Init(onItemActivateCallback, onFeedPropertiesDialog, null);
		}

		/// <summary>
		/// Init the NewsItemToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="onItemActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		/// <param name="onFeedActivateCallback"></param>
		public NewsItemToastNotify(ItemActivateCallback onItemActivateCallback, 
			DisplayFeedPropertiesCallback onFeedPropertiesDialog,
			FeedActivateCallback onFeedActivateCallback){
			Init(onItemActivateCallback,onFeedPropertiesDialog,onFeedActivateCallback);
		}

		/// <summary>
		/// Init the toast display.
		/// </summary>
		/// <param name="feedName">Feed name</param>
		/// <param name="unreadItemsYetDisplayed"></param>
		/// <param name="items"></param>
		/// <returns>true, if items found to display, else false </returns>
		/// <exception cref="InvalidOperationException">If no new items was found</exception>
		public override bool ItemsToDisplay(string feedName, int unreadItemsYetDisplayed, IList items) {
			
			int unreadCount = 0, currentIndex, maxLabels = linkLabels.GetLength(0);

			for (int i = 0; i < items.Count; i++) {
				INewsItem item = (INewsItem)items[i];
				if (!item.BeenRead) {
					if (unreadCount < maxLabels) {
						linkLabels[unreadCount].Text = StringHelper.ShortenByEllipsis(item.Title, 36);
						if (linkLabels[unreadCount].Text.Length < item.Title.Length)
							toolTip.SetToolTip(linkLabels[unreadCount], item.Title);
						else
							toolTip.SetToolTip(linkLabels[unreadCount], String.Empty);
						linkLabels[unreadCount].Tag = item;
						linkLabels[unreadCount].Visible = true;
						linkIcons[unreadCount].Visible = true;
					}
					unreadCount++;
				}
			}

			if (unreadCount > unreadItemsYetDisplayed) {
				currentIndex = Math.Min(maxLabels, unreadCount - unreadItemsYetDisplayed);
			} else {
				currentIndex = 0;
			}

			for (int i = currentIndex; i < maxLabels; i ++) {
				linkLabels[i].Tag = null;
				linkLabels[i].Visible = false;
				linkIcons[i].Visible = false;
				toolTip.SetToolTip(linkLabels[i], String.Empty);
			}
			
			if (currentIndex == 0)
				return false;

			this.labelFeedInfo.Text = feedName + " (" + unreadCount.ToString() + ")";
			this.labelFeedInfo.LinkArea = new  LinkArea(0, feedName.Length);
			unreadCount = unreadCount - unreadItemsYetDisplayed;	// recalc difference for display
			this.labelNewItemsArrived.Text = String.Format(SR.GUIStatusFeedJustReceivedItemsMessage,unreadCount);

			return true;
		}

		

		protected override void linkFeedProperties_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			//navigate to feed options dialog
			if (displayFeedPropertiesCallback != null) {
				try {
					INewsItem item = (INewsItem)this.linkLabel1.Tag;	// we should have always at least one
					System.Diagnostics.Debug.Assert(item != null, "linkLabel1.Tag is undefined");
					displayFeedPropertiesCallback(item.Feed);
				} catch {}
			}
			this.RequestClose();
		}

		protected override void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			//navigate to feed item 
			if (itemActivateCallback != null) {
				try {
					INewsItem item = (INewsItem)((LinkLabel)sender).Tag;
					itemActivateCallback(item);
				} catch {}
			}
			this.RequestClose();
		}

		protected override void OnFeedLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			//navigate to feed item 
			if (feedActivateCallback != null) {
				try {
					INewsItem item = (INewsItem)this.linkLabel1.Tag;	// we should have always at least one
					System.Diagnostics.Debug.Assert(item != null, "linkLabel1.Tag is undefined");
					feedActivateCallback(item.Feed);
				} catch {}
			}
			this.RequestClose();
		}	
	}

	public class EnclosureToastNotify : ToastNotify {
		/// <summary>
		/// Default constructor calls base
		/// </summary>
		public EnclosureToastNotify()
		{ }

		/// <summary>
		/// Init the NewsItemToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="onEnclosureActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		public EnclosureToastNotify(EnclosureActivateCallback onEnclosureActivateCallback, DisplayFeedPropertiesCallback onFeedPropertiesDialog){
			Init(onEnclosureActivateCallback, onFeedPropertiesDialog, null);
		}

		/// <summary>
		/// Init the NewsItemToastNotify with the needed callbacks.
		/// </summary>
		/// <param name="onEnclosureActivateCallback"></param>
		/// <param name="onFeedPropertiesDialog"></param>
		/// <param name="onFeedActivateCallback"></param>
		public EnclosureToastNotify(EnclosureActivateCallback onEnclosureActivateCallback, 
			DisplayFeedPropertiesCallback onFeedPropertiesDialog,
			FeedActivateCallback onFeedActivateCallback){
			Init(onEnclosureActivateCallback,onFeedPropertiesDialog,onFeedActivateCallback);
		}

		/// <summary>
		/// Init the toast display.
		/// </summary>
		/// <param name="feedName">Feed name</param>
		/// <param name="unreadItemsYetDisplayed"></param>
		/// <param name="items"></param>
		/// <returns>true, if items found to display, else false </returns>
		/// <exception cref="InvalidOperationException">If no new items was found</exception>
		public override bool ItemsToDisplay(string feedName, int unreadItemsYetDisplayed, IList items) {
			
			int unreadCount = 0, maxLabels = linkLabels.GetLength(0);
			
			for (int i = 0; i < items.Count; i++) {
				DownloadItem item = (DownloadItem) items[i]; 				
					
				if (unreadCount < maxLabels) {
					linkLabels[unreadCount].Text = StringHelper.ShortenByEllipsis(item.File.LocalName, 36);
					if (linkLabels[unreadCount].Text.Length < item.File.LocalName.Length)
						toolTip.SetToolTip(linkLabels[unreadCount], item.File.LocalName);
					else
						toolTip.SetToolTip(linkLabels[unreadCount], String.Empty);
					linkLabels[unreadCount].Tag = item;
					linkLabels[unreadCount].Visible = true;
					linkIcons[unreadCount].Visible = true;
				}
				unreadCount++;
				
			}//for

			//clear out remaining link labels
			for (int i = 1; i < maxLabels; i ++) {
				linkLabels[i].Tag = null;
				linkLabels[i].Visible = false;
				linkIcons[i].Visible = false;
				toolTip.SetToolTip(linkLabels[i], String.Empty);
			}
						
 
			this.labelFeedInfo.Text = feedName;
			this.labelFeedInfo.LinkArea = new  LinkArea(0, feedName.Length);
			this.labelNewItemsArrived.Text = SR.GUIStatusEnclosureJustReceivedItemsMessage;

			return true;
		}

		

		protected override void linkFeedProperties_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			//navigate to feed options dialog
			if (displayFeedPropertiesCallback != null) {
				try {
					DownloadItem item = (DownloadItem)this.linkLabel1.Tag;	// we should have always at least one
					System.Diagnostics.Debug.Assert(item != null, "linkLabel1.Tag is undefined");
					displayFeedPropertiesCallback(item.OwnerFeed);
				} catch {}
			}
			this.RequestClose();
		}

		protected override void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			//navigate to feed item 
			if (enclosureActivateCallback != null) {
				try {
					DownloadItem item = (DownloadItem)((LinkLabel)sender).Tag;
					this.enclosureActivateCallback(item);					
				} catch {}
			}
			this.RequestClose();
		}

		protected override void OnFeedLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			//navigate to feed item 
			if (feedActivateCallback != null) {
				try {
					DownloadItem item = (DownloadItem)this.linkLabel1.Tag;	// we should have always at least one
					System.Diagnostics.Debug.Assert(item != null, "linkLabel1.Tag is undefined");
					feedActivateCallback(item.OwnerFeed);
				} catch {}
			}
			this.RequestClose();
		}	
	}

}

