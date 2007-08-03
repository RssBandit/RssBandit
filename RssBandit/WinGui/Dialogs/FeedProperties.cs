#region CVS Version Header
/*
 * $Id: FeedProperties.cs,v 1.20 2006/11/12 16:24:35 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/11/12 16:24:35 $
 * $Revision: 1.20 $
 */
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui.Utility;
using RssBandit.WinGui.Controls;

using NewsComponents.Collections;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// Feed Properties dialog.
	/// </summary>
	public class FeedProperties : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		internal System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		internal System.Windows.Forms.TextBox textBox1;
		internal System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ToolTip toolTip1;
		internal System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabItemControl;
		private System.Windows.Forms.TabPage tabAuthentication;
		internal System.Windows.Forms.TextBox textPwd;
		private System.Windows.Forms.Label label7;
		internal System.Windows.Forms.TextBox textUser;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label15;
		internal System.Windows.Forms.ComboBox comboMaxItemAge;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label8;
		internal System.Windows.Forms.CheckBox checkEnableAlerts;
		internal System.Windows.Forms.ComboBox comboBox1;
		internal System.Windows.Forms.CheckBox checkMarkItemsReadOnExit;
		private System.Windows.Forms.Label label9;
		internal System.Windows.Forms.CheckBox checkCustomFormatter;
		private System.Windows.Forms.Label labelFormatters;
		internal System.Windows.Forms.ComboBox comboFormatters;
		private System.Windows.Forms.TabPage tabDisplay;
		private System.Windows.Forms.Panel panelItemControl;
		private System.Windows.Forms.TabPage tabAttachments;
		internal System.Windows.Forms.CheckBox checkEnableEnclosureAlerts;
		internal System.Windows.Forms.CheckBox checkDownloadEnclosures;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.Label label11;
		private System.ComponentModel.IContainer components;


		public FeedProperties(string title, string link, int refreshRate, TimeSpan maxItemAge, string currentCategory, string defaultCategory, CategoriesCollection categories, string stylesheet): 
			this()
		{		

			this.textBox1.Text  = title; 
			this.textBox2.Text  = link; 
			this.comboBox1.Text = refreshRate.ToString(); 
			
			tabAuthentication.Enabled = !RssHelper.IsNntpUrl(link);

			//initialize category combo box			
			foreach(string category in categories.Keys){
				if (!StringHelper.EmptyOrNull(category))
					this.comboBox2.Items.Add(category); 
			}
			this.comboBox2.Items.Add(defaultCategory); 
			if (currentCategory != null)
				this.comboBox2.Text = currentCategory; 

			this.MaxItemAge = maxItemAge;
			this.checkEnableAlerts.Checked = false;

			// item formatters
			string tmplFolder = RssBanditApplication.GetTemplatesPath();
			this.checkCustomFormatter.Enabled = false;
			this.comboFormatters.Items.Clear();
				
			if (Directory.Exists(tmplFolder)) {
				string[] tmplFiles = Directory.GetFiles(tmplFolder, "*.fdxsl");
				
				if (tmplFiles.GetLength(0) > 0) {	
					this.checkCustomFormatter.Enabled = true;
					foreach (string filename in tmplFiles) {
						this.comboFormatters.Items.Add(Path.GetFileNameWithoutExtension(filename)); 
					}
				}			

				if (stylesheet != null && 
					stylesheet.Length > 0 &&
					File.Exists(Path.Combine(tmplFolder, stylesheet + ".fdxsl"))) {
					this.comboFormatters.Text = stylesheet; 
					this.checkCustomFormatter.Checked = true;
				}
			
			}else {
				this.comboFormatters.Text = String.Empty; 
				this.checkCustomFormatter.Checked = false;				
			}
			this.checkCustomFormatter_CheckedChanged(null, null);
			this.comboFormatters.Refresh();
		}


		public FeedProperties()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.Load += new EventHandler(this.OnFeedProperties_Load);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FeedProperties));
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.comboMaxItemAge = new System.Windows.Forms.ComboBox();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabItemControl = new System.Windows.Forms.TabPage();
			this.panelItemControl = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label15 = new System.Windows.Forms.Label();
			this.tabAuthentication = new System.Windows.Forms.TabPage();
			this.textPwd = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textUser = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tabDisplay = new System.Windows.Forms.TabPage();
			this.comboFormatters = new System.Windows.Forms.ComboBox();
			this.checkCustomFormatter = new System.Windows.Forms.CheckBox();
			this.labelFormatters = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.tabAttachments = new System.Windows.Forms.TabPage();
			this.checkDownloadEnclosures = new System.Windows.Forms.CheckBox();
			this.checkEnableEnclosureAlerts = new System.Windows.Forms.CheckBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.checkEnableAlerts = new System.Windows.Forms.CheckBox();
			this.checkMarkItemsReadOnExit = new System.Windows.Forms.CheckBox();
			this.tabControl.SuspendLayout();
			this.tabItemControl.SuspendLayout();
			this.panelItemControl.SuspendLayout();
			this.tabAuthentication.SuspendLayout();
			this.tabDisplay.SuspendLayout();
			this.tabAttachments.SuspendLayout();
			this.SuspendLayout();
			// 
			// button2
			// 
			this.button2.AccessibleDescription = resources.GetString("button2.AccessibleDescription");
			this.button2.AccessibleName = resources.GetString("button2.AccessibleName");
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("button2.Anchor")));
			this.button2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button2.BackgroundImage")));
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("button2.Dock")));
			this.button2.Enabled = ((bool)(resources.GetObject("button2.Enabled")));
			this.button2.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("button2.FlatStyle")));
			this.button2.Font = ((System.Drawing.Font)(resources.GetObject("button2.Font")));
			this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
			this.button2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("button2.ImageAlign")));
			this.button2.ImageIndex = ((int)(resources.GetObject("button2.ImageIndex")));
			this.button2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("button2.ImeMode")));
			this.button2.Location = ((System.Drawing.Point)(resources.GetObject("button2.Location")));
			this.button2.Name = "button2";
			this.button2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("button2.RightToLeft")));
			this.button2.Size = ((System.Drawing.Size)(resources.GetObject("button2.Size")));
			this.button2.TabIndex = ((int)(resources.GetObject("button2.TabIndex")));
			this.button2.Text = resources.GetString("button2.Text");
			this.button2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("button2.TextAlign")));
			this.toolTip1.SetToolTip(this.button2, resources.GetString("button2.ToolTip"));
			this.button2.Visible = ((bool)(resources.GetObject("button2.Visible")));
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
			this.toolTip1.SetToolTip(this.button1, resources.GetString("button1.ToolTip"));
			this.button1.Visible = ((bool)(resources.GetObject("button1.Visible")));
			// 
			// textBox2
			// 
			this.textBox2.AccessibleDescription = resources.GetString("textBox2.AccessibleDescription");
			this.textBox2.AccessibleName = resources.GetString("textBox2.AccessibleName");
			this.textBox2.AllowDrop = true;
			this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textBox2.Anchor")));
			this.textBox2.AutoSize = ((bool)(resources.GetObject("textBox2.AutoSize")));
			this.textBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textBox2.BackgroundImage")));
			this.textBox2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textBox2.Dock")));
			this.textBox2.Enabled = ((bool)(resources.GetObject("textBox2.Enabled")));
			this.textBox2.Font = ((System.Drawing.Font)(resources.GetObject("textBox2.Font")));
			this.textBox2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textBox2.ImeMode")));
			this.textBox2.Location = ((System.Drawing.Point)(resources.GetObject("textBox2.Location")));
			this.textBox2.MaxLength = ((int)(resources.GetObject("textBox2.MaxLength")));
			this.textBox2.Multiline = ((bool)(resources.GetObject("textBox2.Multiline")));
			this.textBox2.Name = "textBox2";
			this.textBox2.PasswordChar = ((char)(resources.GetObject("textBox2.PasswordChar")));
			this.textBox2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textBox2.RightToLeft")));
			this.textBox2.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textBox2.ScrollBars")));
			this.textBox2.Size = ((System.Drawing.Size)(resources.GetObject("textBox2.Size")));
			this.textBox2.TabIndex = ((int)(resources.GetObject("textBox2.TabIndex")));
			this.textBox2.Text = resources.GetString("textBox2.Text");
			this.textBox2.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textBox2.TextAlign")));
			this.toolTip1.SetToolTip(this.textBox2, resources.GetString("textBox2.ToolTip"));
			this.textBox2.Visible = ((bool)(resources.GetObject("textBox2.Visible")));
			this.textBox2.WordWrap = ((bool)(resources.GetObject("textBox2.WordWrap")));
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
			// textBox1
			// 
			this.textBox1.AccessibleDescription = resources.GetString("textBox1.AccessibleDescription");
			this.textBox1.AccessibleName = resources.GetString("textBox1.AccessibleName");
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textBox1.Anchor")));
			this.textBox1.AutoSize = ((bool)(resources.GetObject("textBox1.AutoSize")));
			this.textBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textBox1.BackgroundImage")));
			this.textBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textBox1.Dock")));
			this.textBox1.Enabled = ((bool)(resources.GetObject("textBox1.Enabled")));
			this.textBox1.Font = ((System.Drawing.Font)(resources.GetObject("textBox1.Font")));
			this.textBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textBox1.ImeMode")));
			this.textBox1.Location = ((System.Drawing.Point)(resources.GetObject("textBox1.Location")));
			this.textBox1.MaxLength = ((int)(resources.GetObject("textBox1.MaxLength")));
			this.textBox1.Multiline = ((bool)(resources.GetObject("textBox1.Multiline")));
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = ((char)(resources.GetObject("textBox1.PasswordChar")));
			this.textBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textBox1.RightToLeft")));
			this.textBox1.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textBox1.ScrollBars")));
			this.textBox1.Size = ((System.Drawing.Size)(resources.GetObject("textBox1.Size")));
			this.textBox1.TabIndex = ((int)(resources.GetObject("textBox1.TabIndex")));
			this.textBox1.Text = resources.GetString("textBox1.Text");
			this.textBox1.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textBox1.TextAlign")));
			this.toolTip1.SetToolTip(this.textBox1, resources.GetString("textBox1.ToolTip"));
			this.textBox1.Visible = ((bool)(resources.GetObject("textBox1.Visible")));
			this.textBox1.WordWrap = ((bool)(resources.GetObject("textBox1.WordWrap")));
			// 
			// comboBox2
			// 
			this.comboBox2.AccessibleDescription = resources.GetString("comboBox2.AccessibleDescription");
			this.comboBox2.AccessibleName = resources.GetString("comboBox2.AccessibleName");
			this.comboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboBox2.Anchor")));
			this.comboBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboBox2.BackgroundImage")));
			this.comboBox2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboBox2.Dock")));
			this.comboBox2.Enabled = ((bool)(resources.GetObject("comboBox2.Enabled")));
			this.comboBox2.Font = ((System.Drawing.Font)(resources.GetObject("comboBox2.Font")));
			this.comboBox2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboBox2.ImeMode")));
			this.comboBox2.IntegralHeight = ((bool)(resources.GetObject("comboBox2.IntegralHeight")));
			this.comboBox2.ItemHeight = ((int)(resources.GetObject("comboBox2.ItemHeight")));
			this.comboBox2.Location = ((System.Drawing.Point)(resources.GetObject("comboBox2.Location")));
			this.comboBox2.MaxDropDownItems = ((int)(resources.GetObject("comboBox2.MaxDropDownItems")));
			this.comboBox2.MaxLength = ((int)(resources.GetObject("comboBox2.MaxLength")));
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboBox2.RightToLeft")));
			this.comboBox2.Size = ((System.Drawing.Size)(resources.GetObject("comboBox2.Size")));
			this.comboBox2.Sorted = true;
			this.comboBox2.TabIndex = ((int)(resources.GetObject("comboBox2.TabIndex")));
			this.comboBox2.Text = resources.GetString("comboBox2.Text");
			this.toolTip1.SetToolTip(this.comboBox2, resources.GetString("comboBox2.ToolTip"));
			this.comboBox2.Visible = ((bool)(resources.GetObject("comboBox2.Visible")));
			// 
			// label5
			// 
			this.label5.AccessibleDescription = resources.GetString("label5.AccessibleDescription");
			this.label5.AccessibleName = resources.GetString("label5.AccessibleName");
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label5.Anchor")));
			this.label5.AutoSize = ((bool)(resources.GetObject("label5.AutoSize")));
			this.label5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label5.Dock")));
			this.label5.Enabled = ((bool)(resources.GetObject("label5.Enabled")));
			this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label5.Font = ((System.Drawing.Font)(resources.GetObject("label5.Font")));
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
			// comboMaxItemAge
			// 
			this.comboMaxItemAge.AccessibleDescription = resources.GetString("comboMaxItemAge.AccessibleDescription");
			this.comboMaxItemAge.AccessibleName = resources.GetString("comboMaxItemAge.AccessibleName");
			this.comboMaxItemAge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboMaxItemAge.Anchor")));
			this.comboMaxItemAge.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboMaxItemAge.BackgroundImage")));
			this.comboMaxItemAge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboMaxItemAge.Dock")));
			this.comboMaxItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboMaxItemAge.Enabled = ((bool)(resources.GetObject("comboMaxItemAge.Enabled")));
			this.comboMaxItemAge.Font = ((System.Drawing.Font)(resources.GetObject("comboMaxItemAge.Font")));
			this.comboMaxItemAge.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboMaxItemAge.ImeMode")));
			this.comboMaxItemAge.IntegralHeight = ((bool)(resources.GetObject("comboMaxItemAge.IntegralHeight")));
			this.comboMaxItemAge.ItemHeight = ((int)(resources.GetObject("comboMaxItemAge.ItemHeight")));
			this.comboMaxItemAge.Items.AddRange(new object[] {
																 resources.GetString("comboMaxItemAge.Items"),
																 resources.GetString("comboMaxItemAge.Items1"),
																 resources.GetString("comboMaxItemAge.Items2"),
																 resources.GetString("comboMaxItemAge.Items3"),
																 resources.GetString("comboMaxItemAge.Items4"),
																 resources.GetString("comboMaxItemAge.Items5"),
																 resources.GetString("comboMaxItemAge.Items6"),
																 resources.GetString("comboMaxItemAge.Items7"),
																 resources.GetString("comboMaxItemAge.Items8"),
																 resources.GetString("comboMaxItemAge.Items9"),
																 resources.GetString("comboMaxItemAge.Items10"),
																 resources.GetString("comboMaxItemAge.Items11"),
																 resources.GetString("comboMaxItemAge.Items12"),
																 resources.GetString("comboMaxItemAge.Items13"),
																 resources.GetString("comboMaxItemAge.Items14"),
																 resources.GetString("comboMaxItemAge.Items15")});
			this.comboMaxItemAge.Location = ((System.Drawing.Point)(resources.GetObject("comboMaxItemAge.Location")));
			this.comboMaxItemAge.MaxDropDownItems = ((int)(resources.GetObject("comboMaxItemAge.MaxDropDownItems")));
			this.comboMaxItemAge.MaxLength = ((int)(resources.GetObject("comboMaxItemAge.MaxLength")));
			this.comboMaxItemAge.Name = "comboMaxItemAge";
			this.comboMaxItemAge.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboMaxItemAge.RightToLeft")));
			this.comboMaxItemAge.Size = ((System.Drawing.Size)(resources.GetObject("comboMaxItemAge.Size")));
			this.comboMaxItemAge.TabIndex = ((int)(resources.GetObject("comboMaxItemAge.TabIndex")));
			this.comboMaxItemAge.Text = resources.GetString("comboMaxItemAge.Text");
			this.toolTip1.SetToolTip(this.comboMaxItemAge, resources.GetString("comboMaxItemAge.ToolTip"));
			this.comboMaxItemAge.Visible = ((bool)(resources.GetObject("comboMaxItemAge.Visible")));
			// 
			// tabControl
			// 
			this.tabControl.AccessibleDescription = resources.GetString("tabControl.AccessibleDescription");
			this.tabControl.AccessibleName = resources.GetString("tabControl.AccessibleName");
			this.tabControl.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tabControl.Alignment")));
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabControl.Anchor")));
			this.tabControl.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tabControl.Appearance")));
			this.tabControl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabControl.BackgroundImage")));
			this.tabControl.Controls.Add(this.tabItemControl);
			this.tabControl.Controls.Add(this.tabAuthentication);
			this.tabControl.Controls.Add(this.tabDisplay);
			this.tabControl.Controls.Add(this.tabAttachments);
			this.tabControl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabControl.Dock")));
			this.tabControl.Enabled = ((bool)(resources.GetObject("tabControl.Enabled")));
			this.tabControl.Font = ((System.Drawing.Font)(resources.GetObject("tabControl.Font")));
			this.tabControl.HotTrack = true;
			this.tabControl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabControl.ImeMode")));
			this.tabControl.ItemSize = ((System.Drawing.Size)(resources.GetObject("tabControl.ItemSize")));
			this.tabControl.Location = ((System.Drawing.Point)(resources.GetObject("tabControl.Location")));
			this.tabControl.Name = "tabControl";
			this.tabControl.Padding = ((System.Drawing.Point)(resources.GetObject("tabControl.Padding")));
			this.tabControl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabControl.RightToLeft")));
			this.tabControl.SelectedIndex = 0;
			this.tabControl.ShowToolTips = ((bool)(resources.GetObject("tabControl.ShowToolTips")));
			this.tabControl.Size = ((System.Drawing.Size)(resources.GetObject("tabControl.Size")));
			this.tabControl.TabIndex = ((int)(resources.GetObject("tabControl.TabIndex")));
			this.tabControl.Text = resources.GetString("tabControl.Text");
			this.toolTip1.SetToolTip(this.tabControl, resources.GetString("tabControl.ToolTip"));
			this.tabControl.Visible = ((bool)(resources.GetObject("tabControl.Visible")));
			this.tabControl.Resize += new System.EventHandler(this.OnTabControl_Resize);
			// 
			// tabItemControl
			// 
			this.tabItemControl.AccessibleDescription = resources.GetString("tabItemControl.AccessibleDescription");
			this.tabItemControl.AccessibleName = resources.GetString("tabItemControl.AccessibleName");
			this.tabItemControl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabItemControl.Anchor")));
			this.tabItemControl.AutoScroll = ((bool)(resources.GetObject("tabItemControl.AutoScroll")));
			this.tabItemControl.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabItemControl.AutoScrollMargin")));
			this.tabItemControl.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabItemControl.AutoScrollMinSize")));
			this.tabItemControl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabItemControl.BackgroundImage")));
			this.tabItemControl.Controls.Add(this.panelItemControl);
			this.tabItemControl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabItemControl.Dock")));
			this.tabItemControl.Enabled = ((bool)(resources.GetObject("tabItemControl.Enabled")));
			this.tabItemControl.Font = ((System.Drawing.Font)(resources.GetObject("tabItemControl.Font")));
			this.tabItemControl.ImageIndex = ((int)(resources.GetObject("tabItemControl.ImageIndex")));
			this.tabItemControl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabItemControl.ImeMode")));
			this.tabItemControl.Location = ((System.Drawing.Point)(resources.GetObject("tabItemControl.Location")));
			this.tabItemControl.Name = "tabItemControl";
			this.tabItemControl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabItemControl.RightToLeft")));
			this.tabItemControl.Size = ((System.Drawing.Size)(resources.GetObject("tabItemControl.Size")));
			this.tabItemControl.TabIndex = ((int)(resources.GetObject("tabItemControl.TabIndex")));
			this.tabItemControl.Text = resources.GetString("tabItemControl.Text");
			this.toolTip1.SetToolTip(this.tabItemControl, resources.GetString("tabItemControl.ToolTip"));
			this.tabItemControl.ToolTipText = resources.GetString("tabItemControl.ToolTipText");
			this.tabItemControl.Visible = ((bool)(resources.GetObject("tabItemControl.Visible")));
			// 
			// panelItemControl
			// 
			this.panelItemControl.AccessibleDescription = resources.GetString("panelItemControl.AccessibleDescription");
			this.panelItemControl.AccessibleName = resources.GetString("panelItemControl.AccessibleName");
			this.panelItemControl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelItemControl.Anchor")));
			this.panelItemControl.AutoScroll = ((bool)(resources.GetObject("panelItemControl.AutoScroll")));
			this.panelItemControl.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelItemControl.AutoScrollMargin")));
			this.panelItemControl.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelItemControl.AutoScrollMinSize")));
			this.panelItemControl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelItemControl.BackgroundImage")));
			this.panelItemControl.Controls.Add(this.label4);
			this.panelItemControl.Controls.Add(this.label3);
			this.panelItemControl.Controls.Add(this.comboBox1);
			this.panelItemControl.Controls.Add(this.label15);
			this.panelItemControl.Controls.Add(this.comboMaxItemAge);
			this.panelItemControl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelItemControl.Dock")));
			this.panelItemControl.Enabled = ((bool)(resources.GetObject("panelItemControl.Enabled")));
			this.panelItemControl.Font = ((System.Drawing.Font)(resources.GetObject("panelItemControl.Font")));
			this.panelItemControl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelItemControl.ImeMode")));
			this.panelItemControl.Location = ((System.Drawing.Point)(resources.GetObject("panelItemControl.Location")));
			this.panelItemControl.Name = "panelItemControl";
			this.panelItemControl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelItemControl.RightToLeft")));
			this.panelItemControl.Size = ((System.Drawing.Size)(resources.GetObject("panelItemControl.Size")));
			this.panelItemControl.TabIndex = ((int)(resources.GetObject("panelItemControl.TabIndex")));
			this.panelItemControl.Text = resources.GetString("panelItemControl.Text");
			this.toolTip1.SetToolTip(this.panelItemControl, resources.GetString("panelItemControl.ToolTip"));
			this.panelItemControl.Visible = ((bool)(resources.GetObject("panelItemControl.Visible")));
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
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
			// comboBox1
			// 
			this.comboBox1.AccessibleDescription = resources.GetString("comboBox1.AccessibleDescription");
			this.comboBox1.AccessibleName = resources.GetString("comboBox1.AccessibleName");
			this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboBox1.Anchor")));
			this.comboBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboBox1.BackgroundImage")));
			this.comboBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboBox1.Dock")));
			this.comboBox1.Enabled = ((bool)(resources.GetObject("comboBox1.Enabled")));
			this.comboBox1.Font = ((System.Drawing.Font)(resources.GetObject("comboBox1.Font")));
			this.comboBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboBox1.ImeMode")));
			this.comboBox1.IntegralHeight = ((bool)(resources.GetObject("comboBox1.IntegralHeight")));
			this.comboBox1.ItemHeight = ((int)(resources.GetObject("comboBox1.ItemHeight")));
			this.comboBox1.Items.AddRange(new object[] {
														   resources.GetString("comboBox1.Items"),
														   resources.GetString("comboBox1.Items1"),
														   resources.GetString("comboBox1.Items2"),
														   resources.GetString("comboBox1.Items3"),
														   resources.GetString("comboBox1.Items4"),
														   resources.GetString("comboBox1.Items5"),
														   resources.GetString("comboBox1.Items6"),
														   resources.GetString("comboBox1.Items7"),
														   resources.GetString("comboBox1.Items8")});
			this.comboBox1.Location = ((System.Drawing.Point)(resources.GetObject("comboBox1.Location")));
			this.comboBox1.MaxDropDownItems = ((int)(resources.GetObject("comboBox1.MaxDropDownItems")));
			this.comboBox1.MaxLength = ((int)(resources.GetObject("comboBox1.MaxLength")));
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboBox1.RightToLeft")));
			this.comboBox1.Size = ((System.Drawing.Size)(resources.GetObject("comboBox1.Size")));
			this.comboBox1.TabIndex = ((int)(resources.GetObject("comboBox1.TabIndex")));
			this.comboBox1.Text = resources.GetString("comboBox1.Text");
			this.toolTip1.SetToolTip(this.comboBox1, resources.GetString("comboBox1.ToolTip"));
			this.comboBox1.Visible = ((bool)(resources.GetObject("comboBox1.Visible")));
			// 
			// label15
			// 
			this.label15.AccessibleDescription = resources.GetString("label15.AccessibleDescription");
			this.label15.AccessibleName = resources.GetString("label15.AccessibleName");
			this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label15.Anchor")));
			this.label15.AutoSize = ((bool)(resources.GetObject("label15.AutoSize")));
			this.label15.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label15.Dock")));
			this.label15.Enabled = ((bool)(resources.GetObject("label15.Enabled")));
			this.label15.Font = ((System.Drawing.Font)(resources.GetObject("label15.Font")));
			this.label15.Image = ((System.Drawing.Image)(resources.GetObject("label15.Image")));
			this.label15.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label15.ImageAlign")));
			this.label15.ImageIndex = ((int)(resources.GetObject("label15.ImageIndex")));
			this.label15.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label15.ImeMode")));
			this.label15.Location = ((System.Drawing.Point)(resources.GetObject("label15.Location")));
			this.label15.Name = "label15";
			this.label15.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label15.RightToLeft")));
			this.label15.Size = ((System.Drawing.Size)(resources.GetObject("label15.Size")));
			this.label15.TabIndex = ((int)(resources.GetObject("label15.TabIndex")));
			this.label15.Text = resources.GetString("label15.Text");
			this.label15.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label15.TextAlign")));
			this.toolTip1.SetToolTip(this.label15, resources.GetString("label15.ToolTip"));
			this.label15.Visible = ((bool)(resources.GetObject("label15.Visible")));
			// 
			// tabAuthentication
			// 
			this.tabAuthentication.AccessibleDescription = resources.GetString("tabAuthentication.AccessibleDescription");
			this.tabAuthentication.AccessibleName = resources.GetString("tabAuthentication.AccessibleName");
			this.tabAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabAuthentication.Anchor")));
			this.tabAuthentication.AutoScroll = ((bool)(resources.GetObject("tabAuthentication.AutoScroll")));
			this.tabAuthentication.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabAuthentication.AutoScrollMargin")));
			this.tabAuthentication.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabAuthentication.AutoScrollMinSize")));
			this.tabAuthentication.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabAuthentication.BackgroundImage")));
			this.tabAuthentication.Controls.Add(this.textPwd);
			this.tabAuthentication.Controls.Add(this.label7);
			this.tabAuthentication.Controls.Add(this.textUser);
			this.tabAuthentication.Controls.Add(this.label6);
			this.tabAuthentication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabAuthentication.Dock")));
			this.tabAuthentication.Enabled = ((bool)(resources.GetObject("tabAuthentication.Enabled")));
			this.tabAuthentication.Font = ((System.Drawing.Font)(resources.GetObject("tabAuthentication.Font")));
			this.tabAuthentication.ImageIndex = ((int)(resources.GetObject("tabAuthentication.ImageIndex")));
			this.tabAuthentication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabAuthentication.ImeMode")));
			this.tabAuthentication.Location = ((System.Drawing.Point)(resources.GetObject("tabAuthentication.Location")));
			this.tabAuthentication.Name = "tabAuthentication";
			this.tabAuthentication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabAuthentication.RightToLeft")));
			this.tabAuthentication.Size = ((System.Drawing.Size)(resources.GetObject("tabAuthentication.Size")));
			this.tabAuthentication.TabIndex = ((int)(resources.GetObject("tabAuthentication.TabIndex")));
			this.tabAuthentication.Text = resources.GetString("tabAuthentication.Text");
			this.toolTip1.SetToolTip(this.tabAuthentication, resources.GetString("tabAuthentication.ToolTip"));
			this.tabAuthentication.ToolTipText = resources.GetString("tabAuthentication.ToolTipText");
			this.tabAuthentication.Visible = ((bool)(resources.GetObject("tabAuthentication.Visible")));
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
			// tabDisplay
			// 
			this.tabDisplay.AccessibleDescription = resources.GetString("tabDisplay.AccessibleDescription");
			this.tabDisplay.AccessibleName = resources.GetString("tabDisplay.AccessibleName");
			this.tabDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabDisplay.Anchor")));
			this.tabDisplay.AutoScroll = ((bool)(resources.GetObject("tabDisplay.AutoScroll")));
			this.tabDisplay.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabDisplay.AutoScrollMargin")));
			this.tabDisplay.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabDisplay.AutoScrollMinSize")));
			this.tabDisplay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabDisplay.BackgroundImage")));
			this.tabDisplay.Controls.Add(this.comboFormatters);
			this.tabDisplay.Controls.Add(this.checkCustomFormatter);
			this.tabDisplay.Controls.Add(this.labelFormatters);
			this.tabDisplay.Controls.Add(this.label9);
			this.tabDisplay.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabDisplay.Dock")));
			this.tabDisplay.Enabled = ((bool)(resources.GetObject("tabDisplay.Enabled")));
			this.tabDisplay.Font = ((System.Drawing.Font)(resources.GetObject("tabDisplay.Font")));
			this.tabDisplay.ImageIndex = ((int)(resources.GetObject("tabDisplay.ImageIndex")));
			this.tabDisplay.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabDisplay.ImeMode")));
			this.tabDisplay.Location = ((System.Drawing.Point)(resources.GetObject("tabDisplay.Location")));
			this.tabDisplay.Name = "tabDisplay";
			this.tabDisplay.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabDisplay.RightToLeft")));
			this.tabDisplay.Size = ((System.Drawing.Size)(resources.GetObject("tabDisplay.Size")));
			this.tabDisplay.TabIndex = ((int)(resources.GetObject("tabDisplay.TabIndex")));
			this.tabDisplay.Text = resources.GetString("tabDisplay.Text");
			this.toolTip1.SetToolTip(this.tabDisplay, resources.GetString("tabDisplay.ToolTip"));
			this.tabDisplay.ToolTipText = resources.GetString("tabDisplay.ToolTipText");
			this.tabDisplay.Visible = ((bool)(resources.GetObject("tabDisplay.Visible")));
			// 
			// comboFormatters
			// 
			this.comboFormatters.AccessibleDescription = resources.GetString("comboFormatters.AccessibleDescription");
			this.comboFormatters.AccessibleName = resources.GetString("comboFormatters.AccessibleName");
			this.comboFormatters.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboFormatters.Anchor")));
			this.comboFormatters.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboFormatters.BackgroundImage")));
			this.comboFormatters.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboFormatters.Dock")));
			this.comboFormatters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFormatters.Enabled = ((bool)(resources.GetObject("comboFormatters.Enabled")));
			this.comboFormatters.Font = ((System.Drawing.Font)(resources.GetObject("comboFormatters.Font")));
			this.comboFormatters.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboFormatters.ImeMode")));
			this.comboFormatters.IntegralHeight = ((bool)(resources.GetObject("comboFormatters.IntegralHeight")));
			this.comboFormatters.ItemHeight = ((int)(resources.GetObject("comboFormatters.ItemHeight")));
			this.comboFormatters.Location = ((System.Drawing.Point)(resources.GetObject("comboFormatters.Location")));
			this.comboFormatters.MaxDropDownItems = ((int)(resources.GetObject("comboFormatters.MaxDropDownItems")));
			this.comboFormatters.MaxLength = ((int)(resources.GetObject("comboFormatters.MaxLength")));
			this.comboFormatters.Name = "comboFormatters";
			this.comboFormatters.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboFormatters.RightToLeft")));
			this.comboFormatters.Size = ((System.Drawing.Size)(resources.GetObject("comboFormatters.Size")));
			this.comboFormatters.Sorted = true;
			this.comboFormatters.TabIndex = ((int)(resources.GetObject("comboFormatters.TabIndex")));
			this.comboFormatters.Text = resources.GetString("comboFormatters.Text");
			this.toolTip1.SetToolTip(this.comboFormatters, resources.GetString("comboFormatters.ToolTip"));
			this.comboFormatters.Visible = ((bool)(resources.GetObject("comboFormatters.Visible")));
			// 
			// checkCustomFormatter
			// 
			this.checkCustomFormatter.AccessibleDescription = resources.GetString("checkCustomFormatter.AccessibleDescription");
			this.checkCustomFormatter.AccessibleName = resources.GetString("checkCustomFormatter.AccessibleName");
			this.checkCustomFormatter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkCustomFormatter.Anchor")));
			this.checkCustomFormatter.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkCustomFormatter.Appearance")));
			this.checkCustomFormatter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkCustomFormatter.BackgroundImage")));
			this.checkCustomFormatter.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkCustomFormatter.CheckAlign")));
			this.checkCustomFormatter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkCustomFormatter.Dock")));
			this.checkCustomFormatter.Enabled = ((bool)(resources.GetObject("checkCustomFormatter.Enabled")));
			this.checkCustomFormatter.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkCustomFormatter.FlatStyle")));
			this.checkCustomFormatter.Font = ((System.Drawing.Font)(resources.GetObject("checkCustomFormatter.Font")));
			this.checkCustomFormatter.Image = ((System.Drawing.Image)(resources.GetObject("checkCustomFormatter.Image")));
			this.checkCustomFormatter.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkCustomFormatter.ImageAlign")));
			this.checkCustomFormatter.ImageIndex = ((int)(resources.GetObject("checkCustomFormatter.ImageIndex")));
			this.checkCustomFormatter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkCustomFormatter.ImeMode")));
			this.checkCustomFormatter.Location = ((System.Drawing.Point)(resources.GetObject("checkCustomFormatter.Location")));
			this.checkCustomFormatter.Name = "checkCustomFormatter";
			this.checkCustomFormatter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkCustomFormatter.RightToLeft")));
			this.checkCustomFormatter.Size = ((System.Drawing.Size)(resources.GetObject("checkCustomFormatter.Size")));
			this.checkCustomFormatter.TabIndex = ((int)(resources.GetObject("checkCustomFormatter.TabIndex")));
			this.checkCustomFormatter.Text = resources.GetString("checkCustomFormatter.Text");
			this.checkCustomFormatter.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkCustomFormatter.TextAlign")));
			this.toolTip1.SetToolTip(this.checkCustomFormatter, resources.GetString("checkCustomFormatter.ToolTip"));
			this.checkCustomFormatter.Visible = ((bool)(resources.GetObject("checkCustomFormatter.Visible")));
			this.checkCustomFormatter.CheckedChanged += new System.EventHandler(this.checkCustomFormatter_CheckedChanged);
			// 
			// labelFormatters
			// 
			this.labelFormatters.AccessibleDescription = resources.GetString("labelFormatters.AccessibleDescription");
			this.labelFormatters.AccessibleName = resources.GetString("labelFormatters.AccessibleName");
			this.labelFormatters.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelFormatters.Anchor")));
			this.labelFormatters.AutoSize = ((bool)(resources.GetObject("labelFormatters.AutoSize")));
			this.labelFormatters.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelFormatters.Dock")));
			this.labelFormatters.Enabled = ((bool)(resources.GetObject("labelFormatters.Enabled")));
			this.labelFormatters.Font = ((System.Drawing.Font)(resources.GetObject("labelFormatters.Font")));
			this.labelFormatters.Image = ((System.Drawing.Image)(resources.GetObject("labelFormatters.Image")));
			this.labelFormatters.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFormatters.ImageAlign")));
			this.labelFormatters.ImageIndex = ((int)(resources.GetObject("labelFormatters.ImageIndex")));
			this.labelFormatters.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelFormatters.ImeMode")));
			this.labelFormatters.Location = ((System.Drawing.Point)(resources.GetObject("labelFormatters.Location")));
			this.labelFormatters.Name = "labelFormatters";
			this.labelFormatters.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelFormatters.RightToLeft")));
			this.labelFormatters.Size = ((System.Drawing.Size)(resources.GetObject("labelFormatters.Size")));
			this.labelFormatters.TabIndex = ((int)(resources.GetObject("labelFormatters.TabIndex")));
			this.labelFormatters.Text = resources.GetString("labelFormatters.Text");
			this.labelFormatters.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFormatters.TextAlign")));
			this.toolTip1.SetToolTip(this.labelFormatters, resources.GetString("labelFormatters.ToolTip"));
			this.labelFormatters.Visible = ((bool)(resources.GetObject("labelFormatters.Visible")));
			// 
			// label9
			// 
			this.label9.AccessibleDescription = resources.GetString("label9.AccessibleDescription");
			this.label9.AccessibleName = resources.GetString("label9.AccessibleName");
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label9.Anchor")));
			this.label9.AutoSize = ((bool)(resources.GetObject("label9.AutoSize")));
			this.label9.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label9.Dock")));
			this.label9.Enabled = ((bool)(resources.GetObject("label9.Enabled")));
			this.label9.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label9.Font = ((System.Drawing.Font)(resources.GetObject("label9.Font")));
			this.label9.Image = ((System.Drawing.Image)(resources.GetObject("label9.Image")));
			this.label9.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.ImageAlign")));
			this.label9.ImageIndex = ((int)(resources.GetObject("label9.ImageIndex")));
			this.label9.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label9.ImeMode")));
			this.label9.Location = ((System.Drawing.Point)(resources.GetObject("label9.Location")));
			this.label9.Name = "label9";
			this.label9.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label9.RightToLeft")));
			this.label9.Size = ((System.Drawing.Size)(resources.GetObject("label9.Size")));
			this.label9.TabIndex = ((int)(resources.GetObject("label9.TabIndex")));
			this.label9.Text = resources.GetString("label9.Text");
			this.label9.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.TextAlign")));
			this.toolTip1.SetToolTip(this.label9, resources.GetString("label9.ToolTip"));
			this.label9.Visible = ((bool)(resources.GetObject("label9.Visible")));
			// 
			// tabAttachments
			// 
			this.tabAttachments.AccessibleDescription = resources.GetString("tabAttachments.AccessibleDescription");
			this.tabAttachments.AccessibleName = resources.GetString("tabAttachments.AccessibleName");
			this.tabAttachments.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabAttachments.Anchor")));
			this.tabAttachments.AutoScroll = ((bool)(resources.GetObject("tabAttachments.AutoScroll")));
			this.tabAttachments.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabAttachments.AutoScrollMargin")));
			this.tabAttachments.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabAttachments.AutoScrollMinSize")));
			this.tabAttachments.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabAttachments.BackgroundImage")));
			this.tabAttachments.Controls.Add(this.checkDownloadEnclosures);
			this.tabAttachments.Controls.Add(this.checkEnableEnclosureAlerts);
			this.tabAttachments.Controls.Add(this.textBox3);
			this.tabAttachments.Controls.Add(this.label11);
			this.tabAttachments.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabAttachments.Dock")));
			this.tabAttachments.Enabled = ((bool)(resources.GetObject("tabAttachments.Enabled")));
			this.tabAttachments.Font = ((System.Drawing.Font)(resources.GetObject("tabAttachments.Font")));
			this.tabAttachments.ImageIndex = ((int)(resources.GetObject("tabAttachments.ImageIndex")));
			this.tabAttachments.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabAttachments.ImeMode")));
			this.tabAttachments.Location = ((System.Drawing.Point)(resources.GetObject("tabAttachments.Location")));
			this.tabAttachments.Name = "tabAttachments";
			this.tabAttachments.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabAttachments.RightToLeft")));
			this.tabAttachments.Size = ((System.Drawing.Size)(resources.GetObject("tabAttachments.Size")));
			this.tabAttachments.TabIndex = ((int)(resources.GetObject("tabAttachments.TabIndex")));
			this.tabAttachments.Text = resources.GetString("tabAttachments.Text");
			this.toolTip1.SetToolTip(this.tabAttachments, resources.GetString("tabAttachments.ToolTip"));
			this.tabAttachments.ToolTipText = resources.GetString("tabAttachments.ToolTipText");
			this.tabAttachments.Visible = ((bool)(resources.GetObject("tabAttachments.Visible")));
			// 
			// checkDownloadEnclosures
			// 
			this.checkDownloadEnclosures.AccessibleDescription = resources.GetString("checkDownloadEnclosures.AccessibleDescription");
			this.checkDownloadEnclosures.AccessibleName = resources.GetString("checkDownloadEnclosures.AccessibleName");
			this.checkDownloadEnclosures.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkDownloadEnclosures.Anchor")));
			this.checkDownloadEnclosures.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkDownloadEnclosures.Appearance")));
			this.checkDownloadEnclosures.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkDownloadEnclosures.BackgroundImage")));
			this.checkDownloadEnclosures.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkDownloadEnclosures.CheckAlign")));
			this.checkDownloadEnclosures.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkDownloadEnclosures.Dock")));
			this.checkDownloadEnclosures.Enabled = ((bool)(resources.GetObject("checkDownloadEnclosures.Enabled")));
			this.checkDownloadEnclosures.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkDownloadEnclosures.FlatStyle")));
			this.checkDownloadEnclosures.Font = ((System.Drawing.Font)(resources.GetObject("checkDownloadEnclosures.Font")));
			this.checkDownloadEnclosures.Image = ((System.Drawing.Image)(resources.GetObject("checkDownloadEnclosures.Image")));
			this.checkDownloadEnclosures.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkDownloadEnclosures.ImageAlign")));
			this.checkDownloadEnclosures.ImageIndex = ((int)(resources.GetObject("checkDownloadEnclosures.ImageIndex")));
			this.checkDownloadEnclosures.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkDownloadEnclosures.ImeMode")));
			this.checkDownloadEnclosures.Location = ((System.Drawing.Point)(resources.GetObject("checkDownloadEnclosures.Location")));
			this.checkDownloadEnclosures.Name = "checkDownloadEnclosures";
			this.checkDownloadEnclosures.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkDownloadEnclosures.RightToLeft")));
			this.checkDownloadEnclosures.Size = ((System.Drawing.Size)(resources.GetObject("checkDownloadEnclosures.Size")));
			this.checkDownloadEnclosures.TabIndex = ((int)(resources.GetObject("checkDownloadEnclosures.TabIndex")));
			this.checkDownloadEnclosures.Text = resources.GetString("checkDownloadEnclosures.Text");
			this.checkDownloadEnclosures.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkDownloadEnclosures.TextAlign")));
			this.toolTip1.SetToolTip(this.checkDownloadEnclosures, resources.GetString("checkDownloadEnclosures.ToolTip"));
			this.checkDownloadEnclosures.Visible = ((bool)(resources.GetObject("checkDownloadEnclosures.Visible")));
			// 
			// checkEnableEnclosureAlerts
			// 
			this.checkEnableEnclosureAlerts.AccessibleDescription = resources.GetString("checkEnableEnclosureAlerts.AccessibleDescription");
			this.checkEnableEnclosureAlerts.AccessibleName = resources.GetString("checkEnableEnclosureAlerts.AccessibleName");
			this.checkEnableEnclosureAlerts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkEnableEnclosureAlerts.Anchor")));
			this.checkEnableEnclosureAlerts.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkEnableEnclosureAlerts.Appearance")));
			this.checkEnableEnclosureAlerts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkEnableEnclosureAlerts.BackgroundImage")));
			this.checkEnableEnclosureAlerts.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableEnclosureAlerts.CheckAlign")));
			this.checkEnableEnclosureAlerts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkEnableEnclosureAlerts.Dock")));
			this.checkEnableEnclosureAlerts.Enabled = ((bool)(resources.GetObject("checkEnableEnclosureAlerts.Enabled")));
			this.checkEnableEnclosureAlerts.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkEnableEnclosureAlerts.FlatStyle")));
			this.checkEnableEnclosureAlerts.Font = ((System.Drawing.Font)(resources.GetObject("checkEnableEnclosureAlerts.Font")));
			this.checkEnableEnclosureAlerts.Image = ((System.Drawing.Image)(resources.GetObject("checkEnableEnclosureAlerts.Image")));
			this.checkEnableEnclosureAlerts.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableEnclosureAlerts.ImageAlign")));
			this.checkEnableEnclosureAlerts.ImageIndex = ((int)(resources.GetObject("checkEnableEnclosureAlerts.ImageIndex")));
			this.checkEnableEnclosureAlerts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkEnableEnclosureAlerts.ImeMode")));
			this.checkEnableEnclosureAlerts.Location = ((System.Drawing.Point)(resources.GetObject("checkEnableEnclosureAlerts.Location")));
			this.checkEnableEnclosureAlerts.Name = "checkEnableEnclosureAlerts";
			this.checkEnableEnclosureAlerts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkEnableEnclosureAlerts.RightToLeft")));
			this.checkEnableEnclosureAlerts.Size = ((System.Drawing.Size)(resources.GetObject("checkEnableEnclosureAlerts.Size")));
			this.checkEnableEnclosureAlerts.TabIndex = ((int)(resources.GetObject("checkEnableEnclosureAlerts.TabIndex")));
			this.checkEnableEnclosureAlerts.Text = resources.GetString("checkEnableEnclosureAlerts.Text");
			this.checkEnableEnclosureAlerts.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableEnclosureAlerts.TextAlign")));
			this.toolTip1.SetToolTip(this.checkEnableEnclosureAlerts, resources.GetString("checkEnableEnclosureAlerts.ToolTip"));
			this.checkEnableEnclosureAlerts.Visible = ((bool)(resources.GetObject("checkEnableEnclosureAlerts.Visible")));
			// 
			// textBox3
			// 
			this.textBox3.AccessibleDescription = resources.GetString("textBox3.AccessibleDescription");
			this.textBox3.AccessibleName = resources.GetString("textBox3.AccessibleName");
			this.textBox3.AllowDrop = true;
			this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textBox3.Anchor")));
			this.textBox3.AutoSize = ((bool)(resources.GetObject("textBox3.AutoSize")));
			this.textBox3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textBox3.BackgroundImage")));
			this.textBox3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textBox3.Dock")));
			this.textBox3.Enabled = ((bool)(resources.GetObject("textBox3.Enabled")));
			this.textBox3.Font = ((System.Drawing.Font)(resources.GetObject("textBox3.Font")));
			this.textBox3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textBox3.ImeMode")));
			this.textBox3.Location = ((System.Drawing.Point)(resources.GetObject("textBox3.Location")));
			this.textBox3.MaxLength = ((int)(resources.GetObject("textBox3.MaxLength")));
			this.textBox3.Multiline = ((bool)(resources.GetObject("textBox3.Multiline")));
			this.textBox3.Name = "textBox3";
			this.textBox3.PasswordChar = ((char)(resources.GetObject("textBox3.PasswordChar")));
			this.textBox3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textBox3.RightToLeft")));
			this.textBox3.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textBox3.ScrollBars")));
			this.textBox3.Size = ((System.Drawing.Size)(resources.GetObject("textBox3.Size")));
			this.textBox3.TabIndex = ((int)(resources.GetObject("textBox3.TabIndex")));
			this.textBox3.Text = resources.GetString("textBox3.Text");
			this.textBox3.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textBox3.TextAlign")));
			this.toolTip1.SetToolTip(this.textBox3, resources.GetString("textBox3.ToolTip"));
			this.textBox3.Visible = ((bool)(resources.GetObject("textBox3.Visible")));
			this.textBox3.WordWrap = ((bool)(resources.GetObject("textBox3.WordWrap")));
			// 
			// label11
			// 
			this.label11.AccessibleDescription = resources.GetString("label11.AccessibleDescription");
			this.label11.AccessibleName = resources.GetString("label11.AccessibleName");
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label11.Anchor")));
			this.label11.AutoSize = ((bool)(resources.GetObject("label11.AutoSize")));
			this.label11.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label11.Dock")));
			this.label11.Enabled = ((bool)(resources.GetObject("label11.Enabled")));
			this.label11.Font = ((System.Drawing.Font)(resources.GetObject("label11.Font")));
			this.label11.Image = ((System.Drawing.Image)(resources.GetObject("label11.Image")));
			this.label11.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label11.ImageAlign")));
			this.label11.ImageIndex = ((int)(resources.GetObject("label11.ImageIndex")));
			this.label11.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label11.ImeMode")));
			this.label11.Location = ((System.Drawing.Point)(resources.GetObject("label11.Location")));
			this.label11.Name = "label11";
			this.label11.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label11.RightToLeft")));
			this.label11.Size = ((System.Drawing.Size)(resources.GetObject("label11.Size")));
			this.label11.TabIndex = ((int)(resources.GetObject("label11.TabIndex")));
			this.label11.Text = resources.GetString("label11.Text");
			this.label11.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label11.TextAlign")));
			this.toolTip1.SetToolTip(this.label11, resources.GetString("label11.ToolTip"));
			this.label11.Visible = ((bool)(resources.GetObject("label11.Visible")));
			// 
			// label8
			// 
			this.label8.AccessibleDescription = resources.GetString("label8.AccessibleDescription");
			this.label8.AccessibleName = resources.GetString("label8.AccessibleName");
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label8.Anchor")));
			this.label8.AutoSize = ((bool)(resources.GetObject("label8.AutoSize")));
			this.label8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label8.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label8.Dock")));
			this.label8.Enabled = ((bool)(resources.GetObject("label8.Enabled")));
			this.label8.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label8.Font = ((System.Drawing.Font)(resources.GetObject("label8.Font")));
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
			this.toolTip1.SetToolTip(this.label8, resources.GetString("label8.ToolTip"));
			this.label8.Visible = ((bool)(resources.GetObject("label8.Visible")));
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
			// checkMarkItemsReadOnExit
			// 
			this.checkMarkItemsReadOnExit.AccessibleDescription = resources.GetString("checkMarkItemsReadOnExit.AccessibleDescription");
			this.checkMarkItemsReadOnExit.AccessibleName = resources.GetString("checkMarkItemsReadOnExit.AccessibleName");
			this.checkMarkItemsReadOnExit.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkMarkItemsReadOnExit.Anchor")));
			this.checkMarkItemsReadOnExit.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkMarkItemsReadOnExit.Appearance")));
			this.checkMarkItemsReadOnExit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkMarkItemsReadOnExit.BackgroundImage")));
			this.checkMarkItemsReadOnExit.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkMarkItemsReadOnExit.CheckAlign")));
			this.checkMarkItemsReadOnExit.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkMarkItemsReadOnExit.Dock")));
			this.checkMarkItemsReadOnExit.Enabled = ((bool)(resources.GetObject("checkMarkItemsReadOnExit.Enabled")));
			this.checkMarkItemsReadOnExit.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkMarkItemsReadOnExit.FlatStyle")));
			this.checkMarkItemsReadOnExit.Font = ((System.Drawing.Font)(resources.GetObject("checkMarkItemsReadOnExit.Font")));
			this.checkMarkItemsReadOnExit.Image = ((System.Drawing.Image)(resources.GetObject("checkMarkItemsReadOnExit.Image")));
			this.checkMarkItemsReadOnExit.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkMarkItemsReadOnExit.ImageAlign")));
			this.checkMarkItemsReadOnExit.ImageIndex = ((int)(resources.GetObject("checkMarkItemsReadOnExit.ImageIndex")));
			this.checkMarkItemsReadOnExit.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkMarkItemsReadOnExit.ImeMode")));
			this.checkMarkItemsReadOnExit.Location = ((System.Drawing.Point)(resources.GetObject("checkMarkItemsReadOnExit.Location")));
			this.checkMarkItemsReadOnExit.Name = "checkMarkItemsReadOnExit";
			this.checkMarkItemsReadOnExit.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkMarkItemsReadOnExit.RightToLeft")));
			this.checkMarkItemsReadOnExit.Size = ((System.Drawing.Size)(resources.GetObject("checkMarkItemsReadOnExit.Size")));
			this.checkMarkItemsReadOnExit.TabIndex = ((int)(resources.GetObject("checkMarkItemsReadOnExit.TabIndex")));
			this.checkMarkItemsReadOnExit.Text = resources.GetString("checkMarkItemsReadOnExit.Text");
			this.checkMarkItemsReadOnExit.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkMarkItemsReadOnExit.TextAlign")));
			this.toolTip1.SetToolTip(this.checkMarkItemsReadOnExit, resources.GetString("checkMarkItemsReadOnExit.ToolTip"));
			this.checkMarkItemsReadOnExit.Visible = ((bool)(resources.GetObject("checkMarkItemsReadOnExit.Visible")));
			// 
			// FeedProperties
			// 
			this.AcceptButton = this.button1;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.button2;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.checkMarkItemsReadOnExit);
			this.Controls.Add(this.checkEnableAlerts);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "FeedProperties";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.tabControl.ResumeLayout(false);
			this.tabItemControl.ResumeLayout(false);
			this.panelItemControl.ResumeLayout(false);
			this.tabAuthentication.ResumeLayout(false);
			this.tabDisplay.ResumeLayout(false);
			this.tabAttachments.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		public TimeSpan MaxItemAge {
			get { return Utils.MaxItemAgeFromIndex(this.comboMaxItemAge.SelectedIndex); }
			set { this.comboMaxItemAge.SelectedIndex = Utils.MaxItemAgeToIndex(value);	}
		}

		private void checkCustomFormatter_CheckedChanged(object sender, System.EventArgs e) {
			if (checkCustomFormatter.Checked) {
				labelFormatters.Enabled = comboFormatters.Enabled = true; 				
			}
			else {
				labelFormatters.Enabled = comboFormatters.Enabled = false;				
				comboFormatters.Text = String.Empty;
				comboFormatters.Refresh();
			}
		}
	
		private void OnTabControl_Resize(object sender, EventArgs e) {
		 	// fix the wide screen Tab Control issue by resize ourselfs the panels at the first Tab:
			panelItemControl.SetBounds(0,0, tabControl.Width - 2*panelItemControl.Location.X, 0, BoundsSpecified.Width);
		}

		private void OnFeedProperties_Load(object sender, EventArgs e) {
		 	// fix the wide screen Tab Control issue by resize ourselfs the panels at the first Tab:
			OnTabControl_Resize(this, EventArgs.Empty);
		}

	


	}
}
