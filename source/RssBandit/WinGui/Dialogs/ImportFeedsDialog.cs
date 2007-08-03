#region CVS Version Header
/*
 * $Id: ImportFeedsDialog.cs,v 1.11 2005/12/02 14:32:00 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/12/02 14:32:00 $
 * $Revision: 1.11 $
 */
#endregion
using System;using System.Drawing;using System.Collections;using System.ComponentModel;using System.Windows.Forms;using System.Diagnostics;using System.Threading;using RssBandit.WinGui;using RssBandit.WinGui.Utility;using NewsComponents.Utils;using NewsComponents.Collections;namespace RssBandit.WinGui.Forms{	/// <summary>	/// Summary description for ImportFeedsDialog.	/// </summary>	public class ImportFeedsDialog : System.Windows.Forms.Form	{		private UrlCompletionExtender urlExtender;
		private System.Windows.Forms.Label label2;        private System.Windows.Forms.Button btnOk;        private System.Windows.Forms.Button btnCancel;        private System.Windows.Forms.TextBox textUrlOrFile;		private System.Windows.Forms.ToolTip toolTip1;		private System.Windows.Forms.Label label4;
		internal System.Windows.Forms.Button btnSelectFile;
		private System.Windows.Forms.ComboBox comboCategory;
		private System.Windows.Forms.Label label3;		private System.ComponentModel.IContainer components;				/// <summary>		/// Constructor is private because we always want the categories		/// combo box to be filled		/// </summary>		private ImportFeedsDialog()		{			//			// Required for Windows Form Designer support			//			InitializeComponent();			urlExtender = new UrlCompletionExtender(this);
			urlExtender.Add(this.textUrlOrFile, true);
		}				public ImportFeedsDialog(string urlOrFile, string selectedCategory, string defaultCategory, CategoriesCollection categories):this()	{			this.textUrlOrFile.Text = (urlOrFile != null ? urlOrFile  : String.Empty);			//initialize combo box			
			if (categories != null) {
				foreach(string category in categories.Keys){
					if (!StringHelper.EmptyOrNull(category))
						this.comboCategory.Items.Add(category); 
				}
			}
			this.comboCategory.Items.Add(defaultCategory);
			this.comboCategory.Text = (selectedCategory != null ? selectedCategory : String.Empty); 
		}				public string FeedsUrlOrFile {get { return textUrlOrFile.Text; } }		public string FeedCategory { get { return comboCategory.Text; } } 
		/// <summary>		/// Clean up any resources being used.		/// </summary>		protected override void Dispose( bool disposing )		{			if( disposing )			{				if(components != null)				{					components.Dispose();				}			}			base.Dispose( disposing );		}		#region Windows Form Designer generated code		/// <summary>		/// Required method for Designer support - do not modify		/// the contents of this method with the code editor.		/// </summary>		private void InitializeComponent()		{			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ImportFeedsDialog));
			this.label2 = new System.Windows.Forms.Label();
			this.textUrlOrFile = new System.Windows.Forms.TextBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnSelectFile = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label4 = new System.Windows.Forms.Label();
			this.comboCategory = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
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
			// textUrlOrFile
			// 
			this.textUrlOrFile.AccessibleDescription = resources.GetString("textUrlOrFile.AccessibleDescription");
			this.textUrlOrFile.AccessibleName = resources.GetString("textUrlOrFile.AccessibleName");
			this.textUrlOrFile.AllowDrop = true;
			this.textUrlOrFile.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textUrlOrFile.Anchor")));
			this.textUrlOrFile.AutoSize = ((bool)(resources.GetObject("textUrlOrFile.AutoSize")));
			this.textUrlOrFile.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textUrlOrFile.BackgroundImage")));
			this.textUrlOrFile.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textUrlOrFile.Dock")));
			this.textUrlOrFile.Enabled = ((bool)(resources.GetObject("textUrlOrFile.Enabled")));
			this.textUrlOrFile.Font = ((System.Drawing.Font)(resources.GetObject("textUrlOrFile.Font")));
			this.textUrlOrFile.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textUrlOrFile.ImeMode")));
			this.textUrlOrFile.Location = ((System.Drawing.Point)(resources.GetObject("textUrlOrFile.Location")));
			this.textUrlOrFile.MaxLength = ((int)(resources.GetObject("textUrlOrFile.MaxLength")));
			this.textUrlOrFile.Multiline = ((bool)(resources.GetObject("textUrlOrFile.Multiline")));
			this.textUrlOrFile.Name = "textUrlOrFile";
			this.textUrlOrFile.PasswordChar = ((char)(resources.GetObject("textUrlOrFile.PasswordChar")));
			this.textUrlOrFile.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textUrlOrFile.RightToLeft")));
			this.textUrlOrFile.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textUrlOrFile.ScrollBars")));
			this.textUrlOrFile.Size = ((System.Drawing.Size)(resources.GetObject("textUrlOrFile.Size")));
			this.textUrlOrFile.TabIndex = ((int)(resources.GetObject("textUrlOrFile.TabIndex")));
			this.textUrlOrFile.Text = resources.GetString("textUrlOrFile.Text");
			this.textUrlOrFile.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textUrlOrFile.TextAlign")));
			this.toolTip1.SetToolTip(this.textUrlOrFile, resources.GetString("textUrlOrFile.ToolTip"));
			this.textUrlOrFile.Visible = ((bool)(resources.GetObject("textUrlOrFile.Visible")));
			this.textUrlOrFile.WordWrap = ((bool)(resources.GetObject("textUrlOrFile.WordWrap")));
			this.textUrlOrFile.TextChanged += new System.EventHandler(this.textUri_TextChanged);
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
			// btnSelectFile
			// 
			this.btnSelectFile.AccessibleDescription = resources.GetString("btnSelectFile.AccessibleDescription");
			this.btnSelectFile.AccessibleName = resources.GetString("btnSelectFile.AccessibleName");
			this.btnSelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSelectFile.Anchor")));
			this.btnSelectFile.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSelectFile.BackgroundImage")));
			this.btnSelectFile.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSelectFile.Dock")));
			this.btnSelectFile.Enabled = ((bool)(resources.GetObject("btnSelectFile.Enabled")));
			this.btnSelectFile.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSelectFile.FlatStyle")));
			this.btnSelectFile.Font = ((System.Drawing.Font)(resources.GetObject("btnSelectFile.Font")));
			this.btnSelectFile.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectFile.Image")));
			this.btnSelectFile.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSelectFile.ImageAlign")));
			this.btnSelectFile.ImageIndex = ((int)(resources.GetObject("btnSelectFile.ImageIndex")));
			this.btnSelectFile.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSelectFile.ImeMode")));
			this.btnSelectFile.Location = ((System.Drawing.Point)(resources.GetObject("btnSelectFile.Location")));
			this.btnSelectFile.Name = "btnSelectFile";
			this.btnSelectFile.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSelectFile.RightToLeft")));
			this.btnSelectFile.Size = ((System.Drawing.Size)(resources.GetObject("btnSelectFile.Size")));
			this.btnSelectFile.TabIndex = ((int)(resources.GetObject("btnSelectFile.TabIndex")));
			this.btnSelectFile.Text = resources.GetString("btnSelectFile.Text");
			this.btnSelectFile.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSelectFile.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSelectFile, resources.GetString("btnSelectFile.ToolTip"));
			this.btnSelectFile.Visible = ((bool)(resources.GetObject("btnSelectFile.Visible")));
			this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
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
			this.toolTip1.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
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
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
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
			// ImportFeedsDialog
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
			this.Controls.Add(this.comboCategory);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textUrlOrFile);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.btnSelectFile);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.label2);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ImportFeedsDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.ResumeLayout(false);

		}		#endregion			private void textUri_TextChanged(object sender, System.EventArgs e)		{			btnOk.Enabled = (textUrlOrFile.Text.Length > 0);		}		private void btnSelectFile_Click(object sender, System.EventArgs e) {
			OpenFileDialog ofd = new OpenFileDialog();

			ofd.Filter = "OPML files (*.opml)|*.opml|OCS files (*.ocs)|*.ocs|XML files (*.xml)|*.xml|All files (*.*)|*.*" ;
			ofd.FilterIndex = 4 ;
			ofd.InitialDirectory = Environment.CurrentDirectory;
			ofd.RestoreDirectory = true ;

			if(ofd.ShowDialog() == DialogResult.OK) {
				textUrlOrFile.Text = ofd.FileName;
			}
		}	}}