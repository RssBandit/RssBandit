#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

using RssBandit.WebSearch;
using RssBandit.Resources;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Summary description for SearchEngineProperties.
	/// </summary>
	public class SearchEngineProperties : Form
	{
		private Label label4;
		private Button btnCancel;
		private Button btnOk;
		private Label label1;
		internal TextBox textCaption;
		private ToolTip toolTip1;
		private Label label2;
		private Label label3;
		private Button btnSelectImage;
		internal TextBox textUrl;
		private Label label5;
		internal TextBox textDesc;
		internal TextBox textPicture;
		private OpenFileDialog openFileDialog1;
		private GroupBox groupBox1;
		internal PictureBox pictureEngine;
		private Label label6;
		internal CheckBox checkBoxResultsetIsRssFeed;
		internal CheckBox checkBoxMergeRssResultset;
		private ErrorProvider errorProvider1;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Initializes the dialog to display WebSearchEngine properties.
		/// </summary>
		/// <param name="engine">SearchEngine to display</param>
		public SearchEngineProperties(SearchEngine engine):this() 
		{
		
			this._engine = engine;	// keep ref, if it yet exists 
	
			if (engine != null) {
				this.textUrl.Text = engine.SearchLink;
				this.textCaption.Text = engine.Title;
				this.textDesc.Text = engine.Description;
				this.textPicture.Text = engine.ImageName;
				this.checkBoxResultsetIsRssFeed.Checked = engine.ReturnRssResult;
				this.checkBoxMergeRssResultset.Checked = engine.MergeRssResult;
			}
		
		}

		public SearchEngineProperties()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SearchEngineProperties));
			this.label4 = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textCaption = new System.Windows.Forms.TextBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.textDesc = new System.Windows.Forms.TextBox();
			this.textPicture = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.btnSelectImage = new System.Windows.Forms.Button();
			this.textUrl = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.pictureEngine = new System.Windows.Forms.PictureBox();
			this.checkBoxResultsetIsRssFeed = new System.Windows.Forms.CheckBox();
			this.checkBoxMergeRssResultset = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.errorProvider1.SetError(this.label4, resources.GetString("label4.Error"));
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.errorProvider1.SetIconAlignment(this.label4, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label4.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label4, ((int)(resources.GetObject("label4.IconPadding"))));
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
			this.toolTip1.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.CausesValidation = false;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.errorProvider1.SetError(this.btnCancel, resources.GetString("btnCancel.Error"));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.errorProvider1.SetIconAlignment(this.btnCancel, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnCancel.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnCancel, ((int)(resources.GetObject("btnCancel.IconPadding"))));
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
			// btnOk
			// 
			this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
			this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
			this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
			this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
			this.errorProvider1.SetError(this.btnOk, resources.GetString("btnOk.Error"));
			this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
			this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
			this.errorProvider1.SetIconAlignment(this.btnOk, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnOk.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnOk, ((int)(resources.GetObject("btnOk.IconPadding"))));
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
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.errorProvider1.SetError(this.label1, resources.GetString("label1.Error"));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.errorProvider1.SetIconAlignment(this.label1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label1.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label1, ((int)(resources.GetObject("label1.IconPadding"))));
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
			// textCaption
			// 
			this.textCaption.AccessibleDescription = resources.GetString("textCaption.AccessibleDescription");
			this.textCaption.AccessibleName = resources.GetString("textCaption.AccessibleName");
			this.textCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textCaption.Anchor")));
			this.textCaption.AutoSize = ((bool)(resources.GetObject("textCaption.AutoSize")));
			this.textCaption.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textCaption.BackgroundImage")));
			this.textCaption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textCaption.Dock")));
			this.textCaption.Enabled = ((bool)(resources.GetObject("textCaption.Enabled")));
			this.errorProvider1.SetError(this.textCaption, resources.GetString("textCaption.Error"));
			this.textCaption.Font = ((System.Drawing.Font)(resources.GetObject("textCaption.Font")));
			this.errorProvider1.SetIconAlignment(this.textCaption, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textCaption.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textCaption, ((int)(resources.GetObject("textCaption.IconPadding"))));
			this.textCaption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textCaption.ImeMode")));
			this.textCaption.Location = ((System.Drawing.Point)(resources.GetObject("textCaption.Location")));
			this.textCaption.MaxLength = ((int)(resources.GetObject("textCaption.MaxLength")));
			this.textCaption.Multiline = ((bool)(resources.GetObject("textCaption.Multiline")));
			this.textCaption.Name = "textCaption";
			this.textCaption.PasswordChar = ((char)(resources.GetObject("textCaption.PasswordChar")));
			this.textCaption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textCaption.RightToLeft")));
			this.textCaption.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textCaption.ScrollBars")));
			this.textCaption.Size = ((System.Drawing.Size)(resources.GetObject("textCaption.Size")));
			this.textCaption.TabIndex = ((int)(resources.GetObject("textCaption.TabIndex")));
			this.textCaption.Text = resources.GetString("textCaption.Text");
			this.textCaption.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textCaption.TextAlign")));
			this.toolTip1.SetToolTip(this.textCaption, resources.GetString("textCaption.ToolTip"));
			this.textCaption.Visible = ((bool)(resources.GetObject("textCaption.Visible")));
			this.textCaption.WordWrap = ((bool)(resources.GetObject("textCaption.WordWrap")));
			this.textCaption.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.textCaption.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// textDesc
			// 
			this.textDesc.AccessibleDescription = resources.GetString("textDesc.AccessibleDescription");
			this.textDesc.AccessibleName = resources.GetString("textDesc.AccessibleName");
			this.textDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textDesc.Anchor")));
			this.textDesc.AutoSize = ((bool)(resources.GetObject("textDesc.AutoSize")));
			this.textDesc.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textDesc.BackgroundImage")));
			this.textDesc.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textDesc.Dock")));
			this.textDesc.Enabled = ((bool)(resources.GetObject("textDesc.Enabled")));
			this.errorProvider1.SetError(this.textDesc, resources.GetString("textDesc.Error"));
			this.textDesc.Font = ((System.Drawing.Font)(resources.GetObject("textDesc.Font")));
			this.errorProvider1.SetIconAlignment(this.textDesc, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textDesc.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textDesc, ((int)(resources.GetObject("textDesc.IconPadding"))));
			this.textDesc.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textDesc.ImeMode")));
			this.textDesc.Location = ((System.Drawing.Point)(resources.GetObject("textDesc.Location")));
			this.textDesc.MaxLength = ((int)(resources.GetObject("textDesc.MaxLength")));
			this.textDesc.Multiline = ((bool)(resources.GetObject("textDesc.Multiline")));
			this.textDesc.Name = "textDesc";
			this.textDesc.PasswordChar = ((char)(resources.GetObject("textDesc.PasswordChar")));
			this.textDesc.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textDesc.RightToLeft")));
			this.textDesc.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textDesc.ScrollBars")));
			this.textDesc.Size = ((System.Drawing.Size)(resources.GetObject("textDesc.Size")));
			this.textDesc.TabIndex = ((int)(resources.GetObject("textDesc.TabIndex")));
			this.textDesc.Text = resources.GetString("textDesc.Text");
			this.textDesc.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textDesc.TextAlign")));
			this.toolTip1.SetToolTip(this.textDesc, resources.GetString("textDesc.ToolTip"));
			this.textDesc.Visible = ((bool)(resources.GetObject("textDesc.Visible")));
			this.textDesc.WordWrap = ((bool)(resources.GetObject("textDesc.WordWrap")));
			this.textDesc.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// textPicture
			// 
			this.textPicture.AccessibleDescription = resources.GetString("textPicture.AccessibleDescription");
			this.textPicture.AccessibleName = resources.GetString("textPicture.AccessibleName");
			this.textPicture.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textPicture.Anchor")));
			this.textPicture.AutoSize = ((bool)(resources.GetObject("textPicture.AutoSize")));
			this.textPicture.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textPicture.BackgroundImage")));
			this.textPicture.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textPicture.Dock")));
			this.textPicture.Enabled = ((bool)(resources.GetObject("textPicture.Enabled")));
			this.errorProvider1.SetError(this.textPicture, resources.GetString("textPicture.Error"));
			this.textPicture.Font = ((System.Drawing.Font)(resources.GetObject("textPicture.Font")));
			this.errorProvider1.SetIconAlignment(this.textPicture, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textPicture.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textPicture, ((int)(resources.GetObject("textPicture.IconPadding"))));
			this.textPicture.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textPicture.ImeMode")));
			this.textPicture.Location = ((System.Drawing.Point)(resources.GetObject("textPicture.Location")));
			this.textPicture.MaxLength = ((int)(resources.GetObject("textPicture.MaxLength")));
			this.textPicture.Multiline = ((bool)(resources.GetObject("textPicture.Multiline")));
			this.textPicture.Name = "textPicture";
			this.textPicture.PasswordChar = ((char)(resources.GetObject("textPicture.PasswordChar")));
			this.textPicture.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textPicture.RightToLeft")));
			this.textPicture.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textPicture.ScrollBars")));
			this.textPicture.Size = ((System.Drawing.Size)(resources.GetObject("textPicture.Size")));
			this.textPicture.TabIndex = ((int)(resources.GetObject("textPicture.TabIndex")));
			this.textPicture.Text = resources.GetString("textPicture.Text");
			this.textPicture.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textPicture.TextAlign")));
			this.toolTip1.SetToolTip(this.textPicture, resources.GetString("textPicture.ToolTip"));
			this.textPicture.Visible = ((bool)(resources.GetObject("textPicture.Visible")));
			this.textPicture.WordWrap = ((bool)(resources.GetObject("textPicture.WordWrap")));
			this.textPicture.Validated += new System.EventHandler(this.OnWidgetValidated);
			this.textPicture.TextChanged += new System.EventHandler(this.OnPictureTextChanged);
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.errorProvider1.SetError(this.label2, resources.GetString("label2.Error"));
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.errorProvider1.SetIconAlignment(this.label2, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label2.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label2, ((int)(resources.GetObject("label2.IconPadding"))));
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
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.errorProvider1.SetError(this.label3, resources.GetString("label3.Error"));
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.errorProvider1.SetIconAlignment(this.label3, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label3.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label3, ((int)(resources.GetObject("label3.IconPadding"))));
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
			// btnSelectImage
			// 
			this.btnSelectImage.AccessibleDescription = resources.GetString("btnSelectImage.AccessibleDescription");
			this.btnSelectImage.AccessibleName = resources.GetString("btnSelectImage.AccessibleName");
			this.btnSelectImage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSelectImage.Anchor")));
			this.btnSelectImage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSelectImage.BackgroundImage")));
			this.btnSelectImage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSelectImage.Dock")));
			this.btnSelectImage.Enabled = ((bool)(resources.GetObject("btnSelectImage.Enabled")));
			this.errorProvider1.SetError(this.btnSelectImage, resources.GetString("btnSelectImage.Error"));
			this.btnSelectImage.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSelectImage.FlatStyle")));
			this.btnSelectImage.Font = ((System.Drawing.Font)(resources.GetObject("btnSelectImage.Font")));
			this.errorProvider1.SetIconAlignment(this.btnSelectImage, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSelectImage.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnSelectImage, ((int)(resources.GetObject("btnSelectImage.IconPadding"))));
			this.btnSelectImage.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectImage.Image")));
			this.btnSelectImage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSelectImage.ImageAlign")));
			this.btnSelectImage.ImageIndex = ((int)(resources.GetObject("btnSelectImage.ImageIndex")));
			this.btnSelectImage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSelectImage.ImeMode")));
			this.btnSelectImage.Location = ((System.Drawing.Point)(resources.GetObject("btnSelectImage.Location")));
			this.btnSelectImage.Name = "btnSelectImage";
			this.btnSelectImage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSelectImage.RightToLeft")));
			this.btnSelectImage.Size = ((System.Drawing.Size)(resources.GetObject("btnSelectImage.Size")));
			this.btnSelectImage.TabIndex = ((int)(resources.GetObject("btnSelectImage.TabIndex")));
			this.btnSelectImage.Text = resources.GetString("btnSelectImage.Text");
			this.btnSelectImage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSelectImage.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSelectImage, resources.GetString("btnSelectImage.ToolTip"));
			this.btnSelectImage.Visible = ((bool)(resources.GetObject("btnSelectImage.Visible")));
			this.btnSelectImage.Click += new System.EventHandler(this.btnSelectImage_Click);
			// 
			// textUrl
			// 
			this.textUrl.AccessibleDescription = resources.GetString("textUrl.AccessibleDescription");
			this.textUrl.AccessibleName = resources.GetString("textUrl.AccessibleName");
			this.textUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textUrl.Anchor")));
			this.textUrl.AutoSize = ((bool)(resources.GetObject("textUrl.AutoSize")));
			this.textUrl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textUrl.BackgroundImage")));
			this.textUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textUrl.Dock")));
			this.textUrl.Enabled = ((bool)(resources.GetObject("textUrl.Enabled")));
			this.errorProvider1.SetError(this.textUrl, resources.GetString("textUrl.Error"));
			this.textUrl.Font = ((System.Drawing.Font)(resources.GetObject("textUrl.Font")));
			this.errorProvider1.SetIconAlignment(this.textUrl, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textUrl.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textUrl, ((int)(resources.GetObject("textUrl.IconPadding"))));
			this.textUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textUrl.ImeMode")));
			this.textUrl.Location = ((System.Drawing.Point)(resources.GetObject("textUrl.Location")));
			this.textUrl.MaxLength = ((int)(resources.GetObject("textUrl.MaxLength")));
			this.textUrl.Multiline = ((bool)(resources.GetObject("textUrl.Multiline")));
			this.textUrl.Name = "textUrl";
			this.textUrl.PasswordChar = ((char)(resources.GetObject("textUrl.PasswordChar")));
			this.textUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textUrl.RightToLeft")));
			this.textUrl.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textUrl.ScrollBars")));
			this.textUrl.Size = ((System.Drawing.Size)(resources.GetObject("textUrl.Size")));
			this.textUrl.TabIndex = ((int)(resources.GetObject("textUrl.TabIndex")));
			this.textUrl.Text = resources.GetString("textUrl.Text");
			this.textUrl.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textUrl.TextAlign")));
			this.toolTip1.SetToolTip(this.textUrl, resources.GetString("textUrl.ToolTip"));
			this.textUrl.Visible = ((bool)(resources.GetObject("textUrl.Visible")));
			this.textUrl.WordWrap = ((bool)(resources.GetObject("textUrl.WordWrap")));
			this.textUrl.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.textUrl.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// label5
			// 
			this.label5.AccessibleDescription = resources.GetString("label5.AccessibleDescription");
			this.label5.AccessibleName = resources.GetString("label5.AccessibleName");
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label5.Anchor")));
			this.label5.AutoSize = ((bool)(resources.GetObject("label5.AutoSize")));
			this.label5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label5.Dock")));
			this.label5.Enabled = ((bool)(resources.GetObject("label5.Enabled")));
			this.errorProvider1.SetError(this.label5, resources.GetString("label5.Error"));
			this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label5.Font = ((System.Drawing.Font)(resources.GetObject("label5.Font")));
			this.errorProvider1.SetIconAlignment(this.label5, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label5.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label5, ((int)(resources.GetObject("label5.IconPadding"))));
			this.label5.Image = ((System.Drawing.Image)(resources.GetObject("label5.Image")));
			this.label5.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.ImageAlign")));
			this.label5.ImageIndex = ((int)(resources.GetObject("label5.ImageIndex")));
			this.label5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label5.ImeMode")));
			this.label5.Location = ((System.Drawing.Point)(resources.GetObject("label5.Location")));
			this.label5.Name = "label5";
			this.label5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label5.RightToLeft")));
			this.label5.Size = ((System.Drawing.Size)(resources.GetObject("label5.Size")));
			this.label5.TabIndex = ((int)(resources.GetObject("label5.TabIndex")));
			this.label5.Text = resources.GetString("label5.Text");
			this.label5.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.TextAlign")));
			this.toolTip1.SetToolTip(this.label5, resources.GetString("label5.ToolTip"));
			this.label5.Visible = ((bool)(resources.GetObject("label5.Visible")));
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.pictureEngine);
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.errorProvider1.SetError(this.groupBox1, resources.GetString("groupBox1.Error"));
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Font = ((System.Drawing.Font)(resources.GetObject("groupBox1.Font")));
			this.errorProvider1.SetIconAlignment(this.groupBox1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("groupBox1.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.groupBox1, ((int)(resources.GetObject("groupBox1.IconPadding"))));
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
			// pictureEngine
			// 
			this.pictureEngine.AccessibleDescription = resources.GetString("pictureEngine.AccessibleDescription");
			this.pictureEngine.AccessibleName = resources.GetString("pictureEngine.AccessibleName");
			this.pictureEngine.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureEngine.Anchor")));
			this.pictureEngine.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureEngine.BackgroundImage")));
			this.pictureEngine.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureEngine.Dock")));
			this.pictureEngine.Enabled = ((bool)(resources.GetObject("pictureEngine.Enabled")));
			this.errorProvider1.SetError(this.pictureEngine, resources.GetString("pictureEngine.Error"));
			this.pictureEngine.Font = ((System.Drawing.Font)(resources.GetObject("pictureEngine.Font")));
			this.errorProvider1.SetIconAlignment(this.pictureEngine, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("pictureEngine.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.pictureEngine, ((int)(resources.GetObject("pictureEngine.IconPadding"))));
			this.pictureEngine.Image = ((System.Drawing.Image)(resources.GetObject("pictureEngine.Image")));
			this.pictureEngine.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureEngine.ImeMode")));
			this.pictureEngine.Location = ((System.Drawing.Point)(resources.GetObject("pictureEngine.Location")));
			this.pictureEngine.Name = "pictureEngine";
			this.pictureEngine.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureEngine.RightToLeft")));
			this.pictureEngine.Size = ((System.Drawing.Size)(resources.GetObject("pictureEngine.Size")));
			this.pictureEngine.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureEngine.SizeMode")));
			this.pictureEngine.TabIndex = ((int)(resources.GetObject("pictureEngine.TabIndex")));
			this.pictureEngine.TabStop = false;
			this.pictureEngine.Text = resources.GetString("pictureEngine.Text");
			this.toolTip1.SetToolTip(this.pictureEngine, resources.GetString("pictureEngine.ToolTip"));
			this.pictureEngine.Visible = ((bool)(resources.GetObject("pictureEngine.Visible")));
			// 
			// checkBoxResultsetIsRssFeed
			// 
			this.checkBoxResultsetIsRssFeed.AccessibleDescription = resources.GetString("checkBoxResultsetIsRssFeed.AccessibleDescription");
			this.checkBoxResultsetIsRssFeed.AccessibleName = resources.GetString("checkBoxResultsetIsRssFeed.AccessibleName");
			this.checkBoxResultsetIsRssFeed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxResultsetIsRssFeed.Anchor")));
			this.checkBoxResultsetIsRssFeed.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxResultsetIsRssFeed.Appearance")));
			this.checkBoxResultsetIsRssFeed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxResultsetIsRssFeed.BackgroundImage")));
			this.checkBoxResultsetIsRssFeed.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxResultsetIsRssFeed.CheckAlign")));
			this.checkBoxResultsetIsRssFeed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxResultsetIsRssFeed.Dock")));
			this.checkBoxResultsetIsRssFeed.Enabled = ((bool)(resources.GetObject("checkBoxResultsetIsRssFeed.Enabled")));
			this.errorProvider1.SetError(this.checkBoxResultsetIsRssFeed, resources.GetString("checkBoxResultsetIsRssFeed.Error"));
			this.checkBoxResultsetIsRssFeed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxResultsetIsRssFeed.FlatStyle")));
			this.checkBoxResultsetIsRssFeed.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxResultsetIsRssFeed.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBoxResultsetIsRssFeed, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBoxResultsetIsRssFeed.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBoxResultsetIsRssFeed, ((int)(resources.GetObject("checkBoxResultsetIsRssFeed.IconPadding"))));
			this.checkBoxResultsetIsRssFeed.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxResultsetIsRssFeed.Image")));
			this.checkBoxResultsetIsRssFeed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxResultsetIsRssFeed.ImageAlign")));
			this.checkBoxResultsetIsRssFeed.ImageIndex = ((int)(resources.GetObject("checkBoxResultsetIsRssFeed.ImageIndex")));
			this.checkBoxResultsetIsRssFeed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxResultsetIsRssFeed.ImeMode")));
			this.checkBoxResultsetIsRssFeed.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxResultsetIsRssFeed.Location")));
			this.checkBoxResultsetIsRssFeed.Name = "checkBoxResultsetIsRssFeed";
			this.checkBoxResultsetIsRssFeed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxResultsetIsRssFeed.RightToLeft")));
			this.checkBoxResultsetIsRssFeed.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxResultsetIsRssFeed.Size")));
			this.checkBoxResultsetIsRssFeed.TabIndex = ((int)(resources.GetObject("checkBoxResultsetIsRssFeed.TabIndex")));
			this.checkBoxResultsetIsRssFeed.Text = resources.GetString("checkBoxResultsetIsRssFeed.Text");
			this.checkBoxResultsetIsRssFeed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxResultsetIsRssFeed.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBoxResultsetIsRssFeed, resources.GetString("checkBoxResultsetIsRssFeed.ToolTip"));
			this.checkBoxResultsetIsRssFeed.Visible = ((bool)(resources.GetObject("checkBoxResultsetIsRssFeed.Visible")));
			this.checkBoxResultsetIsRssFeed.Validated += new System.EventHandler(this.OnWidgetValidated);
			this.checkBoxResultsetIsRssFeed.CheckedChanged += new System.EventHandler(this.OnResultsetIsRssCheckedChanged);
			// 
			// checkBoxMergeRssResultset
			// 
			this.checkBoxMergeRssResultset.AccessibleDescription = resources.GetString("checkBoxMergeRssResultset.AccessibleDescription");
			this.checkBoxMergeRssResultset.AccessibleName = resources.GetString("checkBoxMergeRssResultset.AccessibleName");
			this.checkBoxMergeRssResultset.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxMergeRssResultset.Anchor")));
			this.checkBoxMergeRssResultset.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxMergeRssResultset.Appearance")));
			this.checkBoxMergeRssResultset.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxMergeRssResultset.BackgroundImage")));
			this.checkBoxMergeRssResultset.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxMergeRssResultset.CheckAlign")));
			this.checkBoxMergeRssResultset.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxMergeRssResultset.Dock")));
			this.checkBoxMergeRssResultset.Enabled = ((bool)(resources.GetObject("checkBoxMergeRssResultset.Enabled")));
			this.errorProvider1.SetError(this.checkBoxMergeRssResultset, resources.GetString("checkBoxMergeRssResultset.Error"));
			this.checkBoxMergeRssResultset.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxMergeRssResultset.FlatStyle")));
			this.checkBoxMergeRssResultset.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxMergeRssResultset.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBoxMergeRssResultset, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBoxMergeRssResultset.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBoxMergeRssResultset, ((int)(resources.GetObject("checkBoxMergeRssResultset.IconPadding"))));
			this.checkBoxMergeRssResultset.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxMergeRssResultset.Image")));
			this.checkBoxMergeRssResultset.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxMergeRssResultset.ImageAlign")));
			this.checkBoxMergeRssResultset.ImageIndex = ((int)(resources.GetObject("checkBoxMergeRssResultset.ImageIndex")));
			this.checkBoxMergeRssResultset.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxMergeRssResultset.ImeMode")));
			this.checkBoxMergeRssResultset.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxMergeRssResultset.Location")));
			this.checkBoxMergeRssResultset.Name = "checkBoxMergeRssResultset";
			this.checkBoxMergeRssResultset.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxMergeRssResultset.RightToLeft")));
			this.checkBoxMergeRssResultset.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxMergeRssResultset.Size")));
			this.checkBoxMergeRssResultset.TabIndex = ((int)(resources.GetObject("checkBoxMergeRssResultset.TabIndex")));
			this.checkBoxMergeRssResultset.Text = resources.GetString("checkBoxMergeRssResultset.Text");
			this.checkBoxMergeRssResultset.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxMergeRssResultset.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBoxMergeRssResultset, resources.GetString("checkBoxMergeRssResultset.ToolTip"));
			this.checkBoxMergeRssResultset.Visible = ((bool)(resources.GetObject("checkBoxMergeRssResultset.Visible")));
			this.checkBoxMergeRssResultset.CheckedChanged += new System.EventHandler(this.OnMergeRssResultsetCheckedChanged);
			this.checkBoxMergeRssResultset.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// label6
			// 
			this.label6.AccessibleDescription = resources.GetString("label6.AccessibleDescription");
			this.label6.AccessibleName = resources.GetString("label6.AccessibleName");
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label6.Anchor")));
			this.label6.AutoSize = ((bool)(resources.GetObject("label6.AutoSize")));
			this.label6.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label6.Dock")));
			this.label6.Enabled = ((bool)(resources.GetObject("label6.Enabled")));
			this.errorProvider1.SetError(this.label6, resources.GetString("label6.Error"));
			this.label6.Font = ((System.Drawing.Font)(resources.GetObject("label6.Font")));
			this.errorProvider1.SetIconAlignment(this.label6, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label6.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label6, ((int)(resources.GetObject("label6.IconPadding"))));
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
			// openFileDialog1
			// 
			this.openFileDialog1.DefaultExt = "gif";
			this.openFileDialog1.Filter = resources.GetString("openFileDialog1.Filter");
			this.openFileDialog1.Title = resources.GetString("openFileDialog1.Title");
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			this.errorProvider1.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProvider1.Icon")));
			// 
			// SearchEngineProperties
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
			this.Controls.Add(this.checkBoxMergeRssResultset);
			this.Controls.Add(this.checkBoxResultsetIsRssFeed);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.textUrl);
			this.Controls.Add(this.textPicture);
			this.Controls.Add(this.textDesc);
			this.Controls.Add(this.textCaption);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.btnSelectImage);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SearchEngineProperties";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		private SearchEngine _engine;
		public SearchEngine Engine {
			get { return _engine; }
			set { _engine = value;}
		}

		private void btnSelectImage_Click(object sender, EventArgs e) {
			if (this.openFileDialog1.ShowDialog(this) == DialogResult.OK) {
				Bitmap bitmap;
				try {
					bitmap = new Bitmap(this.openFileDialog1.OpenFile());
				} catch (Exception ex) {
					MessageBox.Show(this, String.Format(SR.ExceptionOpenFileMessage,this.openFileDialog1.FileName, ex.Message), 
						SR.PreferencesExceptionMessageTitle);
					return;
				}
				if (bitmap != null) {
					if (bitmap.Height != 16 && bitmap.Width != 16) {
						MessageBox.Show(this, SR.WrongImageSizeMessage,
							SR.PreferencesExceptionMessageTitle);
						return;
					}
					this.pictureEngine.Image = bitmap;
					this.textPicture.Text = this.openFileDialog1.FileName;
					OnWidgetValidated(this, EventArgs.Empty);
				}
			}
		}

		private void OnPictureTextChanged(object sender, EventArgs e) {
			if (this.textPicture.Text.Length == 0)
				this.pictureEngine.Image = null;
		}

		private void OnResultsetIsRssCheckedChanged(object sender, EventArgs e) {
			this.checkBoxMergeRssResultset.Enabled = this.checkBoxResultsetIsRssFeed.Checked;
			OnWidgetValidated(this, EventArgs.Empty);
		}

		private void OnMergeRssResultsetCheckedChanged(object sender, EventArgs e) {
			OnWidgetValidated(this, EventArgs.Empty);
		}
		
		private void OnWidgetValidated(object sender, EventArgs e) {
			if (_engine != null) {
				bool anyChange = (
					this.textUrl.Text != _engine.SearchLink ||
					this.textCaption.Text != _engine.Title ||
					this.textDesc.Text != _engine.Description ||
					this.textPicture.Text != _engine.ImageName ||
					this.checkBoxResultsetIsRssFeed.Checked != _engine.ReturnRssResult ||
					this.checkBoxMergeRssResultset.Checked != _engine.MergeRssResult);
				if (anyChange)
					anyChange = (this.textUrl.Text.Length > 0 && this.textCaption.Text.Length > 0);
				if (anyChange && !this.btnOk.Enabled)
					this.btnOk.Enabled = true;
			} else
				if ((this.textUrl.Text.Length > 0 && this.textCaption.Text.Length > 0) && !this.btnOk.Enabled)
				this.btnOk.Enabled = true;
		}

		private void OnWidgetValidating(object sender, System.ComponentModel.CancelEventArgs e) {
			this.btnOk.Enabled = false;

			if (sender == textUrl) {

				textUrl.Text = textUrl.Text.Trim();
				if (textUrl.Text.Length == 0) {
					errorProvider1.SetError(textUrl, SR.SearchEnginePropertiesSearchUrlEmpty);
					e.Cancel = true;
				} else {
					try {
						if (new Uri(this.textUrl.Text) != null && 
							this.textUrl.Text.IndexOf("{0}") < 0 && 
							this.textUrl.Text.IndexOf("{0:") < 0) {
							errorProvider1.SetError(textUrl, String.Format(SR.SearchEnginePropertiesSearchUrlMissingParam, "{0}"));
							e.Cancel = true;
						}
					} catch (UriFormatException) {
						e.Cancel = true;
					}
				}
			} else if(sender == textCaption) {
				textCaption.Text = textCaption.Text.Trim();
				if (textCaption.Text.Length == 0) {
					errorProvider1.SetError(textCaption, SR.SearchEnginePropertiesSearchCaptionEmpty);
					e.Cancel = true;
				}
				
			} // if (sender)

			if (!e.Cancel)
				errorProvider1.SetError((Control)sender, null);

		}
	}
}
