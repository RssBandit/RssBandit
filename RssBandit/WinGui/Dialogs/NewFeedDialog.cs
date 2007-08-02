#region CVS Version Header
/*
 * $Id: NewFeedDialog.cs,v 1.25 2005/04/08 15:00:20 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/08 15:00:20 $
 * $Revision: 1.25 $
 */
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Threading;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Collections;
using NewsComponents.Utils;
using RssBandit.WinGui;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// Summary description for NewFeedDialog.
	/// </summary>
	public class NewFeedDialog : System.Windows.Forms.Form
	{
		public string FeedTitle { get { return textTitle.Text; } }
		public string FeedUrl {get { return textUri.Text; } }
		public string FeedCategory { get { return comboCategory.Text; } } 
		public NewsHandler FeedHandler; 

		private UrlCompletionExtender urlExtender;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		internal System.Windows.Forms.Button btnLookupTitle;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox textTitle;
        private System.Windows.Forms.TextBox textUri;
        private System.Windows.Forms.ComboBox comboCategory;
		private System.Windows.Forms.ToolTip toolTip1;
		internal System.Windows.Forms.CheckBox checkEnableAlerts;
		private System.Windows.Forms.GroupBox groupBox1;
		internal System.Windows.Forms.TextBox textPwd;
		private System.Windows.Forms.Label label7;
		internal System.Windows.Forms.TextBox textUser;
		private System.Windows.Forms.Label label6;
		private System.ComponentModel.IContainer components;

		
		/// <summary>
		/// Constructor is private because we always want the categories
		/// combo box to be filled
		/// </summary>
		private NewFeedDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			urlExtender = new UrlCompletionExtender(this);
			urlExtender.Add(this.textUri);
		}

		public NewFeedDialog(string currentCategory, string defaultCategory, CategoriesCollection categories):this()
		{
			//initialize combo box			
			if (categories != null) {
				foreach(string category in categories.Keys){
					if (!StringHelper.EmptyOrNull(category))
						this.comboCategory.Items.Add(category); 
				}
			}
			this.comboCategory.Items.Add(defaultCategory); 
			this.comboCategory.Text = (currentCategory != null ? currentCategory : defaultCategory); 
		}

		public NewFeedDialog(string currentCategory, string defaultCategory, CategoriesCollection categories, string feedUrl, string feedTitle):this(currentCategory, defaultCategory, categories)
		{
			this.textUri.Text   = (feedUrl   != null ? feedUrl  : String.Empty);
			this.textTitle.Text = (feedTitle != null ? feedTitle: String.Empty);
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewFeedDialog));
			this.label1 = new System.Windows.Forms.Label();
			this.textTitle = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textUri = new System.Windows.Forms.TextBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.comboCategory = new System.Windows.Forms.ComboBox();
			this.btnLookupTitle = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.checkEnableAlerts = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textPwd = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textUser = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.toolTip1.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// textTitle
			// 
			this.textTitle.AccessibleDescription = resources.GetString("textTitle.AccessibleDescription");
			this.textTitle.AccessibleName = resources.GetString("textTitle.AccessibleName");
			this.textTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textTitle.Anchor")));
			this.textTitle.AutoSize = ((bool)(resources.GetObject("textTitle.AutoSize")));
			this.textTitle.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textTitle.BackgroundImage")));
			this.textTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textTitle.Dock")));
			this.textTitle.Enabled = ((bool)(resources.GetObject("textTitle.Enabled")));
			this.textTitle.Font = ((System.Drawing.Font)(resources.GetObject("textTitle.Font")));
			this.textTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textTitle.ImeMode")));
			this.textTitle.Location = ((System.Drawing.Point)(resources.GetObject("textTitle.Location")));
			this.textTitle.MaxLength = ((int)(resources.GetObject("textTitle.MaxLength")));
			this.textTitle.Multiline = ((bool)(resources.GetObject("textTitle.Multiline")));
			this.textTitle.Name = "textTitle";
			this.textTitle.PasswordChar = ((char)(resources.GetObject("textTitle.PasswordChar")));
			this.textTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textTitle.RightToLeft")));
			this.textTitle.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textTitle.ScrollBars")));
			this.textTitle.Size = ((System.Drawing.Size)(resources.GetObject("textTitle.Size")));
			this.textTitle.TabIndex = ((int)(resources.GetObject("textTitle.TabIndex")));
			this.textTitle.Text = resources.GetString("textTitle.Text");
			this.textTitle.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textTitle.TextAlign")));
			this.toolTip1.SetToolTip(this.textTitle, resources.GetString("textTitle.ToolTip"));
			this.textTitle.Visible = ((bool)(resources.GetObject("textTitle.Visible")));
			this.textTitle.WordWrap = ((bool)(resources.GetObject("textTitle.WordWrap")));
			this.textTitle.TextChanged += new System.EventHandler(this.textTitle_TextChanged);
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.toolTip1.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// textUri
			// 
			this.textUri.AccessibleDescription = resources.GetString("textUri.AccessibleDescription");
			this.textUri.AccessibleName = resources.GetString("textUri.AccessibleName");
			this.textUri.AllowDrop = true;
			this.textUri.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textUri.Anchor")));
			this.textUri.AutoSize = ((bool)(resources.GetObject("textUri.AutoSize")));
			this.textUri.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textUri.BackgroundImage")));
			this.textUri.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textUri.Dock")));
			this.textUri.Enabled = ((bool)(resources.GetObject("textUri.Enabled")));
			this.textUri.Font = ((System.Drawing.Font)(resources.GetObject("textUri.Font")));
			this.textUri.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textUri.ImeMode")));
			this.textUri.Location = ((System.Drawing.Point)(resources.GetObject("textUri.Location")));
			this.textUri.MaxLength = ((int)(resources.GetObject("textUri.MaxLength")));
			this.textUri.Multiline = ((bool)(resources.GetObject("textUri.Multiline")));
			this.textUri.Name = "textUri";
			this.textUri.PasswordChar = ((char)(resources.GetObject("textUri.PasswordChar")));
			this.textUri.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textUri.RightToLeft")));
			this.textUri.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textUri.ScrollBars")));
			this.textUri.Size = ((System.Drawing.Size)(resources.GetObject("textUri.Size")));
			this.textUri.TabIndex = ((int)(resources.GetObject("textUri.TabIndex")));
			this.textUri.Text = resources.GetString("textUri.Text");
			this.textUri.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textUri.TextAlign")));
			this.toolTip1.SetToolTip(this.textUri, resources.GetString("textUri.ToolTip"));
			this.textUri.Visible = ((bool)(resources.GetObject("textUri.Visible")));
			this.textUri.WordWrap = ((bool)(resources.GetObject("textUri.WordWrap")));
			this.textUri.TextChanged += new System.EventHandler(this.textUri_TextChanged);
			// 
			// btnOk
			// 
			this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
			this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
			this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
			this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
			this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
			this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
			this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
			this.btnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.ImageAlign")));
			this.btnOk.ImageIndex = ((int)(resources.GetObject("btnOk.ImageIndex")));
			this.btnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOk.ImeMode")));
			this.btnOk.Location = ((System.Drawing.Point)(resources.GetObject("btnOk.Location")));
			this.btnOk.Name = "btnOk";
			this.btnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOk.RightToLeft")));
			this.btnOk.Size = ((System.Drawing.Size)(resources.GetObject("btnOk.Size")));
			this.btnOk.TabIndex = ((int)(resources.GetObject("btnOk.TabIndex")));
			this.btnOk.Text = resources.GetString("btnOk.Text");
			this.btnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.TextAlign")));
			this.toolTip1.SetToolTip(this.btnOk, resources.GetString("btnOk.ToolTip"));
			this.btnOk.Visible = ((bool)(resources.GetObject("btnOk.Visible")));
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
			this.toolTip1.SetToolTip(this.btnCancel, resources.GetString("btnCancel.ToolTip"));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
			this.label3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.ImageAlign")));
			this.label3.ImageIndex = ((int)(resources.GetObject("label3.ImageIndex")));
			this.label3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label3.ImeMode")));
			this.label3.Location = ((System.Drawing.Point)(resources.GetObject("label3.Location")));
			this.label3.Name = "label3";
			this.label3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label3.RightToLeft")));
			this.label3.Size = ((System.Drawing.Size)(resources.GetObject("label3.Size")));
			this.label3.TabIndex = ((int)(resources.GetObject("label3.TabIndex")));
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.TextAlign")));
			this.toolTip1.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// comboCategory
			// 
			this.comboCategory.AccessibleDescription = resources.GetString("comboCategory.AccessibleDescription");
			this.comboCategory.AccessibleName = resources.GetString("comboCategory.AccessibleName");
			this.comboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboCategory.Anchor")));
			this.comboCategory.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboCategory.BackgroundImage")));
			this.comboCategory.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboCategory.Dock")));
			this.comboCategory.Enabled = ((bool)(resources.GetObject("comboCategory.Enabled")));
			this.comboCategory.Font = ((System.Drawing.Font)(resources.GetObject("comboCategory.Font")));
			this.comboCategory.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboCategory.ImeMode")));
			this.comboCategory.IntegralHeight = ((bool)(resources.GetObject("comboCategory.IntegralHeight")));
			this.comboCategory.ItemHeight = ((int)(resources.GetObject("comboCategory.ItemHeight")));
			this.comboCategory.Location = ((System.Drawing.Point)(resources.GetObject("comboCategory.Location")));
			this.comboCategory.MaxDropDownItems = ((int)(resources.GetObject("comboCategory.MaxDropDownItems")));
			this.comboCategory.MaxLength = ((int)(resources.GetObject("comboCategory.MaxLength")));
			this.comboCategory.Name = "comboCategory";
			this.comboCategory.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboCategory.RightToLeft")));
			this.comboCategory.Size = ((System.Drawing.Size)(resources.GetObject("comboCategory.Size")));
			this.comboCategory.Sorted = true;
			this.comboCategory.TabIndex = ((int)(resources.GetObject("comboCategory.TabIndex")));
			this.comboCategory.Text = resources.GetString("comboCategory.Text");
			this.toolTip1.SetToolTip(this.comboCategory, resources.GetString("comboCategory.ToolTip"));
			this.comboCategory.Visible = ((bool)(resources.GetObject("comboCategory.Visible")));
			// 
			// btnLookupTitle
			// 
			this.btnLookupTitle.AccessibleDescription = resources.GetString("btnLookupTitle.AccessibleDescription");
			this.btnLookupTitle.AccessibleName = resources.GetString("btnLookupTitle.AccessibleName");
			this.btnLookupTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnLookupTitle.Anchor")));
			this.btnLookupTitle.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLookupTitle.BackgroundImage")));
			this.btnLookupTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnLookupTitle.Dock")));
			this.btnLookupTitle.Enabled = ((bool)(resources.GetObject("btnLookupTitle.Enabled")));
			this.btnLookupTitle.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnLookupTitle.FlatStyle")));
			this.btnLookupTitle.Font = ((System.Drawing.Font)(resources.GetObject("btnLookupTitle.Font")));
			this.btnLookupTitle.Image = ((System.Drawing.Image)(resources.GetObject("btnLookupTitle.Image")));
			this.btnLookupTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnLookupTitle.ImageAlign")));
			this.btnLookupTitle.ImageIndex = ((int)(resources.GetObject("btnLookupTitle.ImageIndex")));
			this.btnLookupTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnLookupTitle.ImeMode")));
			this.btnLookupTitle.Location = ((System.Drawing.Point)(resources.GetObject("btnLookupTitle.Location")));
			this.btnLookupTitle.Name = "btnLookupTitle";
			this.btnLookupTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnLookupTitle.RightToLeft")));
			this.btnLookupTitle.Size = ((System.Drawing.Size)(resources.GetObject("btnLookupTitle.Size")));
			this.btnLookupTitle.TabIndex = ((int)(resources.GetObject("btnLookupTitle.TabIndex")));
			this.btnLookupTitle.Text = resources.GetString("btnLookupTitle.Text");
			this.btnLookupTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnLookupTitle.TextAlign")));
			this.toolTip1.SetToolTip(this.btnLookupTitle, resources.GetString("btnLookupTitle.ToolTip"));
			this.btnLookupTitle.Visible = ((bool)(resources.GetObject("btnLookupTitle.Visible")));
			this.btnLookupTitle.Click += new System.EventHandler(this.btnLookupTitle_Click);
			// 
			// checkEnableAlerts
			// 
			this.checkEnableAlerts.AccessibleDescription = resources.GetString("checkEnableAlerts.AccessibleDescription");
			this.checkEnableAlerts.AccessibleName = resources.GetString("checkEnableAlerts.AccessibleName");
			this.checkEnableAlerts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkEnableAlerts.Anchor")));
			this.checkEnableAlerts.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkEnableAlerts.Appearance")));
			this.checkEnableAlerts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkEnableAlerts.BackgroundImage")));
			this.checkEnableAlerts.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableAlerts.CheckAlign")));
			this.checkEnableAlerts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkEnableAlerts.Dock")));
			this.checkEnableAlerts.Enabled = ((bool)(resources.GetObject("checkEnableAlerts.Enabled")));
			this.checkEnableAlerts.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkEnableAlerts.FlatStyle")));
			this.checkEnableAlerts.Font = ((System.Drawing.Font)(resources.GetObject("checkEnableAlerts.Font")));
			this.checkEnableAlerts.Image = ((System.Drawing.Image)(resources.GetObject("checkEnableAlerts.Image")));
			this.checkEnableAlerts.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableAlerts.ImageAlign")));
			this.checkEnableAlerts.ImageIndex = ((int)(resources.GetObject("checkEnableAlerts.ImageIndex")));
			this.checkEnableAlerts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkEnableAlerts.ImeMode")));
			this.checkEnableAlerts.Location = ((System.Drawing.Point)(resources.GetObject("checkEnableAlerts.Location")));
			this.checkEnableAlerts.Name = "checkEnableAlerts";
			this.checkEnableAlerts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkEnableAlerts.RightToLeft")));
			this.checkEnableAlerts.Size = ((System.Drawing.Size)(resources.GetObject("checkEnableAlerts.Size")));
			this.checkEnableAlerts.TabIndex = ((int)(resources.GetObject("checkEnableAlerts.TabIndex")));
			this.checkEnableAlerts.Text = resources.GetString("checkEnableAlerts.Text");
			this.checkEnableAlerts.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableAlerts.TextAlign")));
			this.toolTip1.SetToolTip(this.checkEnableAlerts, resources.GetString("checkEnableAlerts.ToolTip"));
			this.checkEnableAlerts.Visible = ((bool)(resources.GetObject("checkEnableAlerts.Visible")));
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.textPwd);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.textUser);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Font = ((System.Drawing.Font)(resources.GetObject("groupBox1.Font")));
			this.groupBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox1.ImeMode")));
			this.groupBox1.Location = ((System.Drawing.Point)(resources.GetObject("groupBox1.Location")));
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox1.RightToLeft")));
			this.groupBox1.Size = ((System.Drawing.Size)(resources.GetObject("groupBox1.Size")));
			this.groupBox1.TabIndex = ((int)(resources.GetObject("groupBox1.TabIndex")));
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = resources.GetString("groupBox1.Text");
			this.toolTip1.SetToolTip(this.groupBox1, resources.GetString("groupBox1.ToolTip"));
			this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
			// 
			// textPwd
			// 
			this.textPwd.AccessibleDescription = resources.GetString("textPwd.AccessibleDescription");
			this.textPwd.AccessibleName = resources.GetString("textPwd.AccessibleName");
			this.textPwd.AllowDrop = true;
			this.textPwd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textPwd.Anchor")));
			this.textPwd.AutoSize = ((bool)(resources.GetObject("textPwd.AutoSize")));
			this.textPwd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textPwd.BackgroundImage")));
			this.textPwd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textPwd.Dock")));
			this.textPwd.Enabled = ((bool)(resources.GetObject("textPwd.Enabled")));
			this.textPwd.Font = ((System.Drawing.Font)(resources.GetObject("textPwd.Font")));
			this.textPwd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textPwd.ImeMode")));
			this.textPwd.Location = ((System.Drawing.Point)(resources.GetObject("textPwd.Location")));
			this.textPwd.MaxLength = ((int)(resources.GetObject("textPwd.MaxLength")));
			this.textPwd.Multiline = ((bool)(resources.GetObject("textPwd.Multiline")));
			this.textPwd.Name = "textPwd";
			this.textPwd.PasswordChar = ((char)(resources.GetObject("textPwd.PasswordChar")));
			this.textPwd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textPwd.RightToLeft")));
			this.textPwd.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textPwd.ScrollBars")));
			this.textPwd.Size = ((System.Drawing.Size)(resources.GetObject("textPwd.Size")));
			this.textPwd.TabIndex = ((int)(resources.GetObject("textPwd.TabIndex")));
			this.textPwd.Text = resources.GetString("textPwd.Text");
			this.textPwd.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textPwd.TextAlign")));
			this.toolTip1.SetToolTip(this.textPwd, resources.GetString("textPwd.ToolTip"));
			this.textPwd.Visible = ((bool)(resources.GetObject("textPwd.Visible")));
			this.textPwd.WordWrap = ((bool)(resources.GetObject("textPwd.WordWrap")));
			// 
			// label7
			// 
			this.label7.AccessibleDescription = resources.GetString("label7.AccessibleDescription");
			this.label7.AccessibleName = resources.GetString("label7.AccessibleName");
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label7.Anchor")));
			this.label7.AutoSize = ((bool)(resources.GetObject("label7.AutoSize")));
			this.label7.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label7.Dock")));
			this.label7.Enabled = ((bool)(resources.GetObject("label7.Enabled")));
			this.label7.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label7.Font = ((System.Drawing.Font)(resources.GetObject("label7.Font")));
			this.label7.Image = ((System.Drawing.Image)(resources.GetObject("label7.Image")));
			this.label7.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label7.ImageAlign")));
			this.label7.ImageIndex = ((int)(resources.GetObject("label7.ImageIndex")));
			this.label7.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label7.ImeMode")));
			this.label7.Location = ((System.Drawing.Point)(resources.GetObject("label7.Location")));
			this.label7.Name = "label7";
			this.label7.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label7.RightToLeft")));
			this.label7.Size = ((System.Drawing.Size)(resources.GetObject("label7.Size")));
			this.label7.TabIndex = ((int)(resources.GetObject("label7.TabIndex")));
			this.label7.Text = resources.GetString("label7.Text");
			this.label7.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label7.TextAlign")));
			this.toolTip1.SetToolTip(this.label7, resources.GetString("label7.ToolTip"));
			this.label7.Visible = ((bool)(resources.GetObject("label7.Visible")));
			// 
			// textUser
			// 
			this.textUser.AccessibleDescription = resources.GetString("textUser.AccessibleDescription");
			this.textUser.AccessibleName = resources.GetString("textUser.AccessibleName");
			this.textUser.AllowDrop = true;
			this.textUser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textUser.Anchor")));
			this.textUser.AutoSize = ((bool)(resources.GetObject("textUser.AutoSize")));
			this.textUser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textUser.BackgroundImage")));
			this.textUser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textUser.Dock")));
			this.textUser.Enabled = ((bool)(resources.GetObject("textUser.Enabled")));
			this.textUser.Font = ((System.Drawing.Font)(resources.GetObject("textUser.Font")));
			this.textUser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textUser.ImeMode")));
			this.textUser.Location = ((System.Drawing.Point)(resources.GetObject("textUser.Location")));
			this.textUser.MaxLength = ((int)(resources.GetObject("textUser.MaxLength")));
			this.textUser.Multiline = ((bool)(resources.GetObject("textUser.Multiline")));
			this.textUser.Name = "textUser";
			this.textUser.PasswordChar = ((char)(resources.GetObject("textUser.PasswordChar")));
			this.textUser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textUser.RightToLeft")));
			this.textUser.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textUser.ScrollBars")));
			this.textUser.Size = ((System.Drawing.Size)(resources.GetObject("textUser.Size")));
			this.textUser.TabIndex = ((int)(resources.GetObject("textUser.TabIndex")));
			this.textUser.Text = resources.GetString("textUser.Text");
			this.textUser.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textUser.TextAlign")));
			this.toolTip1.SetToolTip(this.textUser, resources.GetString("textUser.ToolTip"));
			this.textUser.Visible = ((bool)(resources.GetObject("textUser.Visible")));
			this.textUser.WordWrap = ((bool)(resources.GetObject("textUser.WordWrap")));
			// 
			// label6
			// 
			this.label6.AccessibleDescription = resources.GetString("label6.AccessibleDescription");
			this.label6.AccessibleName = resources.GetString("label6.AccessibleName");
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label6.Anchor")));
			this.label6.AutoSize = ((bool)(resources.GetObject("label6.AutoSize")));
			this.label6.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label6.Dock")));
			this.label6.Enabled = ((bool)(resources.GetObject("label6.Enabled")));
			this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label6.Font = ((System.Drawing.Font)(resources.GetObject("label6.Font")));
			this.label6.Image = ((System.Drawing.Image)(resources.GetObject("label6.Image")));
			this.label6.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label6.ImageAlign")));
			this.label6.ImageIndex = ((int)(resources.GetObject("label6.ImageIndex")));
			this.label6.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label6.ImeMode")));
			this.label6.Location = ((System.Drawing.Point)(resources.GetObject("label6.Location")));
			this.label6.Name = "label6";
			this.label6.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label6.RightToLeft")));
			this.label6.Size = ((System.Drawing.Size)(resources.GetObject("label6.Size")));
			this.label6.TabIndex = ((int)(resources.GetObject("label6.TabIndex")));
			this.label6.Text = resources.GetString("label6.Text");
			this.label6.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label6.TextAlign")));
			this.toolTip1.SetToolTip(this.label6, resources.GetString("label6.ToolTip"));
			this.label6.Visible = ((bool)(resources.GetObject("label6.Visible")));
			// 
			// NewFeedDialog
			// 
			this.AcceptButton = this.btnOk;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.checkEnableAlerts);
			this.Controls.Add(this.btnLookupTitle);
			this.Controls.Add(this.comboCategory);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.textUri);
			this.Controls.Add(this.textTitle);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "NewFeedDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion	

		private void btnLookupTitle_Click(object sender, System.EventArgs e){

			if (this.textUri.Text == null || this.textUri.Text.Trim().Length == 0)
				return;

			try{ 
				Uri reqUri = new Uri(this.textUri.Text);
			}catch(UriFormatException){

				if(!this.textUri.Text.ToLower().StartsWith("http://")){
					this.textUri.Text = "http://" + this.textUri.Text; 						
				}				
			}

			PrefetchFeedThreadHandler fetchHandler = new PrefetchFeedThreadHandler(this.textUri.Text, FeedHandler);
			if (this.textUser.Text.Trim().Length > 0) {
				fetchHandler.Credentials = NewsHandler.CreateCredentialsFrom(this.textUser.Text.Trim(), this.textPwd.Text.Trim());
			}

			DialogResult result = fetchHandler.Start(this, Resource.Manager.FormatMessage("RES_GUIStatusWaitMessagePrefetchFeed", this.textUri.Text));

			if (result != DialogResult.OK)
				return;

			if (!fetchHandler.OperationSucceeds) {
				MessageBox.Show(
					Resource.Manager.FormatMessage("RES_WebExceptionOnUrlAccess", this.textUri.Text, fetchHandler.OperationException.Message), 
					Resource.Manager["RES_GUIErrorMessageBoxCaption"], MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}

			if(fetchHandler.DiscoveredDetails != null) {
				if (fetchHandler.DiscoveredDetails.Title != null)
					this.textTitle.Text = System.Web.HttpUtility.HtmlDecode(fetchHandler.DiscoveredDetails.Title);
			}

		}

		private void textUri_TextChanged(object sender, System.EventArgs e) {
			btnOk.Enabled = (textUri.Text.Trim().Length > 0) && (textTitle.Text.Trim().Length > 0);
		}

		private void textTitle_TextChanged(object sender, System.EventArgs e) {
			btnOk.Enabled = (textUri.Text.Trim().Length > 0) && (textTitle.Text.Trim().Length > 0);
		}


	}

}
