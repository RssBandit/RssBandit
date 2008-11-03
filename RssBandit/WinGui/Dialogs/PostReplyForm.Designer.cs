#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

namespace RssBandit.WinGui.Dialogs
{
	internal partial class PostReplyForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PostReplyForm));
			this.label4 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.richTextBox1 = new System.Windows.Forms.TextBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnManageIdentities = new System.Windows.Forms.Button();
			this.cboUserIdentityForComments = new System.Windows.Forms.ComboBox();
			this.txtTitle = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtSentInfos = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.grpReplyItems = new System.Windows.Forms.GroupBox();
			this.chkBeautify = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.grpReplyItems.SuspendLayout();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.label4.Image = ((System.Drawing.Image)(resources.GetObject("label4.Image")));
			this.label4.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.ImageAlign")));
			this.label4.ImageIndex = ((int)(resources.GetObject("label4.ImageIndex")));
			this.label4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label4.ImeMode")));
			this.label4.Location = ((System.Drawing.Point)(resources.GetObject("label4.Location")));
			this.label4.Name = "label4";
			this.label4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label4.RightToLeft")));
			this.label4.Size = ((System.Drawing.Size)(resources.GetObject("label4.Size")));
			this.label4.TabIndex = ((int)(resources.GetObject("label4.TabIndex")));
			this.label4.Text = resources.GetString("label4.Text");
			this.label4.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.TextAlign")));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// button1
			// 
			this.button1.AccessibleDescription = resources.GetString("button1.AccessibleDescription");
			this.button1.AccessibleName = resources.GetString("button1.AccessibleName");
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("button1.Anchor")));
			this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("button1.Dock")));
			this.button1.Enabled = ((bool)(resources.GetObject("button1.Enabled")));
			this.button1.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("button1.FlatStyle")));
			this.button1.Font = ((System.Drawing.Font)(resources.GetObject("button1.Font")));
			this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
			this.button1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("button1.ImageAlign")));
			this.button1.ImageIndex = ((int)(resources.GetObject("button1.ImageIndex")));
			this.button1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("button1.ImeMode")));
			this.button1.Location = ((System.Drawing.Point)(resources.GetObject("button1.Location")));
			this.button1.Name = "button1";
			this.button1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("button1.RightToLeft")));
			this.button1.Size = ((System.Drawing.Size)(resources.GetObject("button1.Size")));
			this.button1.TabIndex = ((int)(resources.GetObject("button1.TabIndex")));
			this.button1.Text = resources.GetString("button1.Text");
			this.button1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("button1.TextAlign")));
			this.button1.Visible = ((bool)(resources.GetObject("button1.Visible")));
			this.button1.Click += new System.EventHandler(this.DoPostReplyClick);
			// 
			// richTextBox1
			// 
			this.richTextBox1.AccessibleDescription = resources.GetString("richTextBox1.AccessibleDescription");
			this.richTextBox1.AccessibleName = resources.GetString("richTextBox1.AccessibleName");
			this.richTextBox1.AllowDrop = true;
			this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("richTextBox1.Anchor")));
			this.richTextBox1.AutoSize = ((bool)(resources.GetObject("richTextBox1.AutoSize")));
			this.richTextBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("richTextBox1.BackgroundImage")));
			this.richTextBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("richTextBox1.Dock")));
			this.richTextBox1.Enabled = ((bool)(resources.GetObject("richTextBox1.Enabled")));
			this.richTextBox1.Font = ((System.Drawing.Font)(resources.GetObject("richTextBox1.Font")));
			this.richTextBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("richTextBox1.ImeMode")));
			this.richTextBox1.Location = ((System.Drawing.Point)(resources.GetObject("richTextBox1.Location")));
			this.richTextBox1.MaxLength = ((int)(resources.GetObject("richTextBox1.MaxLength")));
			this.richTextBox1.Multiline = ((bool)(resources.GetObject("richTextBox1.Multiline")));
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.PasswordChar = ((char)(resources.GetObject("richTextBox1.PasswordChar")));
			this.richTextBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("richTextBox1.RightToLeft")));
			this.richTextBox1.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("richTextBox1.ScrollBars")));
			this.richTextBox1.Size = ((System.Drawing.Size)(resources.GetObject("richTextBox1.Size")));
			this.richTextBox1.TabIndex = ((int)(resources.GetObject("richTextBox1.TabIndex")));
			this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
			this.richTextBox1.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("richTextBox1.TextAlign")));
			this.richTextBox1.Visible = ((bool)(resources.GetObject("richTextBox1.Visible")));
			this.richTextBox1.WordWrap = ((bool)(resources.GetObject("richTextBox1.WordWrap")));
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
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnManageIdentities
			// 
			this.btnManageIdentities.AccessibleDescription = resources.GetString("btnManageIdentities.AccessibleDescription");
			this.btnManageIdentities.AccessibleName = resources.GetString("btnManageIdentities.AccessibleName");
			this.btnManageIdentities.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnManageIdentities.Anchor")));
			this.btnManageIdentities.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnManageIdentities.BackgroundImage")));
			this.btnManageIdentities.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnManageIdentities.Dock")));
			this.btnManageIdentities.Enabled = ((bool)(resources.GetObject("btnManageIdentities.Enabled")));
			this.btnManageIdentities.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnManageIdentities.FlatStyle")));
			this.btnManageIdentities.Font = ((System.Drawing.Font)(resources.GetObject("btnManageIdentities.Font")));
			this.btnManageIdentities.Image = ((System.Drawing.Image)(resources.GetObject("btnManageIdentities.Image")));
			this.btnManageIdentities.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageIdentities.ImageAlign")));
			this.btnManageIdentities.ImageIndex = ((int)(resources.GetObject("btnManageIdentities.ImageIndex")));
			this.btnManageIdentities.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnManageIdentities.ImeMode")));
			this.btnManageIdentities.Location = ((System.Drawing.Point)(resources.GetObject("btnManageIdentities.Location")));
			this.btnManageIdentities.Name = "btnManageIdentities";
			this.btnManageIdentities.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnManageIdentities.RightToLeft")));
			this.btnManageIdentities.Size = ((System.Drawing.Size)(resources.GetObject("btnManageIdentities.Size")));
			this.btnManageIdentities.TabIndex = ((int)(resources.GetObject("btnManageIdentities.TabIndex")));
			this.btnManageIdentities.Text = resources.GetString("btnManageIdentities.Text");
			this.btnManageIdentities.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageIdentities.TextAlign")));
			this.btnManageIdentities.Visible = ((bool)(resources.GetObject("btnManageIdentities.Visible")));
			this.btnManageIdentities.Click += new System.EventHandler(this.btnManageIdentities_Click);
			// 
			// cboUserIdentityForComments
			// 
			this.cboUserIdentityForComments.AccessibleDescription = resources.GetString("cboUserIdentityForComments.AccessibleDescription");
			this.cboUserIdentityForComments.AccessibleName = resources.GetString("cboUserIdentityForComments.AccessibleName");
			this.cboUserIdentityForComments.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cboUserIdentityForComments.Anchor")));
			this.cboUserIdentityForComments.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cboUserIdentityForComments.BackgroundImage")));
			this.cboUserIdentityForComments.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cboUserIdentityForComments.Dock")));
			this.cboUserIdentityForComments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboUserIdentityForComments.Enabled = ((bool)(resources.GetObject("cboUserIdentityForComments.Enabled")));
			this.cboUserIdentityForComments.Font = ((System.Drawing.Font)(resources.GetObject("cboUserIdentityForComments.Font")));
			this.cboUserIdentityForComments.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cboUserIdentityForComments.ImeMode")));
			this.cboUserIdentityForComments.IntegralHeight = ((bool)(resources.GetObject("cboUserIdentityForComments.IntegralHeight")));
			this.cboUserIdentityForComments.ItemHeight = ((int)(resources.GetObject("cboUserIdentityForComments.ItemHeight")));
			this.cboUserIdentityForComments.Location = ((System.Drawing.Point)(resources.GetObject("cboUserIdentityForComments.Location")));
			this.cboUserIdentityForComments.MaxDropDownItems = ((int)(resources.GetObject("cboUserIdentityForComments.MaxDropDownItems")));
			this.cboUserIdentityForComments.MaxLength = ((int)(resources.GetObject("cboUserIdentityForComments.MaxLength")));
			this.cboUserIdentityForComments.Name = "cboUserIdentityForComments";
			this.cboUserIdentityForComments.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cboUserIdentityForComments.RightToLeft")));
			this.cboUserIdentityForComments.Size = ((System.Drawing.Size)(resources.GetObject("cboUserIdentityForComments.Size")));
			this.cboUserIdentityForComments.Sorted = true;
			this.cboUserIdentityForComments.TabIndex = ((int)(resources.GetObject("cboUserIdentityForComments.TabIndex")));
			this.cboUserIdentityForComments.Text = resources.GetString("cboUserIdentityForComments.Text");
			this.cboUserIdentityForComments.Visible = ((bool)(resources.GetObject("cboUserIdentityForComments.Visible")));
			this.cboUserIdentityForComments.SelectionChangeCommitted += new System.EventHandler(this.OnIdentitySelectionChangeCommitted);
			// 
			// txtTitle
			// 
			this.txtTitle.AccessibleDescription = resources.GetString("txtTitle.AccessibleDescription");
			this.txtTitle.AccessibleName = resources.GetString("txtTitle.AccessibleName");
			this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtTitle.Anchor")));
			this.txtTitle.AutoSize = ((bool)(resources.GetObject("txtTitle.AutoSize")));
			this.txtTitle.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtTitle.BackgroundImage")));
			this.txtTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtTitle.Dock")));
			this.txtTitle.Enabled = ((bool)(resources.GetObject("txtTitle.Enabled")));
			this.txtTitle.Font = ((System.Drawing.Font)(resources.GetObject("txtTitle.Font")));
			this.txtTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtTitle.ImeMode")));
			this.txtTitle.Location = ((System.Drawing.Point)(resources.GetObject("txtTitle.Location")));
			this.txtTitle.MaxLength = ((int)(resources.GetObject("txtTitle.MaxLength")));
			this.txtTitle.Multiline = ((bool)(resources.GetObject("txtTitle.Multiline")));
			this.txtTitle.Name = "txtTitle";
			this.txtTitle.PasswordChar = ((char)(resources.GetObject("txtTitle.PasswordChar")));
			this.txtTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtTitle.RightToLeft")));
			this.txtTitle.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtTitle.ScrollBars")));
			this.txtTitle.Size = ((System.Drawing.Size)(resources.GetObject("txtTitle.Size")));
			this.txtTitle.TabIndex = ((int)(resources.GetObject("txtTitle.TabIndex")));
			this.txtTitle.Text = resources.GetString("txtTitle.Text");
			this.txtTitle.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtTitle.TextAlign")));
			this.txtTitle.Visible = ((bool)(resources.GetObject("txtTitle.Visible")));
			this.txtTitle.WordWrap = ((bool)(resources.GetObject("txtTitle.WordWrap")));
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
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.txtSentInfos);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.cboUserIdentityForComments);
			this.groupBox1.Controls.Add(this.btnManageIdentities);
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
			this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
			// 
			// txtSentInfos
			// 
			this.txtSentInfos.AccessibleDescription = resources.GetString("txtSentInfos.AccessibleDescription");
			this.txtSentInfos.AccessibleName = resources.GetString("txtSentInfos.AccessibleName");
			this.txtSentInfos.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtSentInfos.Anchor")));
			this.txtSentInfos.AutoSize = ((bool)(resources.GetObject("txtSentInfos.AutoSize")));
			this.txtSentInfos.BackColor = System.Drawing.SystemColors.Control;
			this.txtSentInfos.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtSentInfos.BackgroundImage")));
			this.txtSentInfos.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtSentInfos.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtSentInfos.Dock")));
			this.txtSentInfos.Enabled = ((bool)(resources.GetObject("txtSentInfos.Enabled")));
			this.txtSentInfos.Font = ((System.Drawing.Font)(resources.GetObject("txtSentInfos.Font")));
			this.txtSentInfos.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtSentInfos.ImeMode")));
			this.txtSentInfos.Location = ((System.Drawing.Point)(resources.GetObject("txtSentInfos.Location")));
			this.txtSentInfos.MaxLength = ((int)(resources.GetObject("txtSentInfos.MaxLength")));
			this.txtSentInfos.Multiline = ((bool)(resources.GetObject("txtSentInfos.Multiline")));
			this.txtSentInfos.Name = "txtSentInfos";
			this.txtSentInfos.PasswordChar = ((char)(resources.GetObject("txtSentInfos.PasswordChar")));
			this.txtSentInfos.ReadOnly = true;
			this.txtSentInfos.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtSentInfos.RightToLeft")));
			this.txtSentInfos.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtSentInfos.ScrollBars")));
			this.txtSentInfos.Size = ((System.Drawing.Size)(resources.GetObject("txtSentInfos.Size")));
			this.txtSentInfos.TabIndex = ((int)(resources.GetObject("txtSentInfos.TabIndex")));
			this.txtSentInfos.TabStop = false;
			this.txtSentInfos.Text = resources.GetString("txtSentInfos.Text");
			this.txtSentInfos.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtSentInfos.TextAlign")));
			this.txtSentInfos.Visible = ((bool)(resources.GetObject("txtSentInfos.Visible")));
			this.txtSentInfos.WordWrap = ((bool)(resources.GetObject("txtSentInfos.WordWrap")));
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
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// grpReplyItems
			// 
			this.grpReplyItems.AccessibleDescription = resources.GetString("grpReplyItems.AccessibleDescription");
			this.grpReplyItems.AccessibleName = resources.GetString("grpReplyItems.AccessibleName");
			this.grpReplyItems.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("grpReplyItems.Anchor")));
			this.grpReplyItems.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("grpReplyItems.BackgroundImage")));
			this.grpReplyItems.Controls.Add(this.chkBeautify);
			this.grpReplyItems.Controls.Add(this.richTextBox1);
			this.grpReplyItems.Controls.Add(this.txtTitle);
			this.grpReplyItems.Controls.Add(this.label2);
			this.grpReplyItems.Controls.Add(this.label4);
			this.grpReplyItems.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("grpReplyItems.Dock")));
			this.grpReplyItems.Enabled = ((bool)(resources.GetObject("grpReplyItems.Enabled")));
			this.grpReplyItems.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.grpReplyItems.Font = ((System.Drawing.Font)(resources.GetObject("grpReplyItems.Font")));
			this.grpReplyItems.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("grpReplyItems.ImeMode")));
			this.grpReplyItems.Location = ((System.Drawing.Point)(resources.GetObject("grpReplyItems.Location")));
			this.grpReplyItems.Name = "grpReplyItems";
			this.grpReplyItems.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("grpReplyItems.RightToLeft")));
			this.grpReplyItems.Size = ((System.Drawing.Size)(resources.GetObject("grpReplyItems.Size")));
			this.grpReplyItems.TabIndex = ((int)(resources.GetObject("grpReplyItems.TabIndex")));
			this.grpReplyItems.TabStop = false;
			this.grpReplyItems.Text = resources.GetString("grpReplyItems.Text");
			this.grpReplyItems.Visible = ((bool)(resources.GetObject("grpReplyItems.Visible")));
			// 
			// chkBeautify
			// 
			this.chkBeautify.AccessibleDescription = resources.GetString("chkBeautify.AccessibleDescription");
			this.chkBeautify.AccessibleName = resources.GetString("chkBeautify.AccessibleName");
			this.chkBeautify.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkBeautify.Anchor")));
			this.chkBeautify.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkBeautify.Appearance")));
			this.chkBeautify.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkBeautify.BackgroundImage")));
			this.chkBeautify.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkBeautify.CheckAlign")));
			this.chkBeautify.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkBeautify.Dock")));
			this.chkBeautify.Enabled = ((bool)(resources.GetObject("chkBeautify.Enabled")));
			this.chkBeautify.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkBeautify.FlatStyle")));
			this.chkBeautify.Font = ((System.Drawing.Font)(resources.GetObject("chkBeautify.Font")));
			this.chkBeautify.Image = ((System.Drawing.Image)(resources.GetObject("chkBeautify.Image")));
			this.chkBeautify.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkBeautify.ImageAlign")));
			this.chkBeautify.ImageIndex = ((int)(resources.GetObject("chkBeautify.ImageIndex")));
			this.chkBeautify.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkBeautify.ImeMode")));
			this.chkBeautify.Location = ((System.Drawing.Point)(resources.GetObject("chkBeautify.Location")));
			this.chkBeautify.Name = "chkBeautify";
			this.chkBeautify.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkBeautify.RightToLeft")));
			this.chkBeautify.Size = ((System.Drawing.Size)(resources.GetObject("chkBeautify.Size")));
			this.chkBeautify.TabIndex = ((int)(resources.GetObject("chkBeautify.TabIndex")));
			this.chkBeautify.Text = resources.GetString("chkBeautify.Text");
			this.chkBeautify.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkBeautify.TextAlign")));
			this.chkBeautify.Visible = ((bool)(resources.GetObject("chkBeautify.Visible")));
			// 
			// PostReplyForm
			// 
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
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.grpReplyItems);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "PostReplyForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.groupBox1.ResumeLayout(false);
			this.grpReplyItems.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

	}
}
