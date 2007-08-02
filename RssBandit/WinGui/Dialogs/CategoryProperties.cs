#region CVS Version Header
/*
 * $Id: CategoryProperties.cs,v 1.3 2005/02/04 20:58:49 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/02/04 20:58:49 $
 * $Revision: 1.3 $
 */
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using NewsComponents.Utils;
using RssBandit.WinGui;
using RssBandit.WinGui.Utility;

using NewsComponents.Collections;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// Summary description for CategoryProperties.
	/// </summary>
	public class CategoryProperties : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		internal System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label2;
		internal System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ToolTip toolTip1;
		internal System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabItemControl;
		private System.Windows.Forms.Label label15;
		internal System.Windows.Forms.ComboBox comboMaxItemAge;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		internal System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label9;
		internal System.Windows.Forms.CheckBox checkCustomFormatter;
		private System.Windows.Forms.Label labelFormatters;
		internal System.Windows.Forms.ComboBox comboFormatters;
		private System.Windows.Forms.TabPage tabDisplay;
		internal System.Windows.Forms.CheckBox checkMarkItemsReadOnExit;
		private System.ComponentModel.IContainer components;


		public CategoryProperties(string title, int refreshRate, TimeSpan maxItemAge, string stylesheet): this(){		
			
			this.textBox2.Text  = title; 
			this.comboBox1.Text = refreshRate.ToString(); 
					
			this.MaxItemAge = maxItemAge;
			//this.checkEnableAlerts.Checked = false;

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


		public CategoryProperties()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CategoryProperties));
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.comboMaxItemAge = new System.Windows.Forms.ComboBox();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabItemControl = new System.Windows.Forms.TabPage();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label15 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tabDisplay = new System.Windows.Forms.TabPage();
			this.checkCustomFormatter = new System.Windows.Forms.CheckBox();
			this.labelFormatters = new System.Windows.Forms.Label();
			this.comboFormatters = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.checkMarkItemsReadOnExit = new System.Windows.Forms.CheckBox();
			this.tabControl.SuspendLayout();
			this.tabItemControl.SuspendLayout();
			this.tabDisplay.SuspendLayout();
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
			this.tabControl.Controls.Add(this.tabDisplay);
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
			this.tabItemControl.Controls.Add(this.comboBox1);
			this.tabItemControl.Controls.Add(this.label15);
			this.tabItemControl.Controls.Add(this.comboMaxItemAge);
			this.tabItemControl.Controls.Add(this.label4);
			this.tabItemControl.Controls.Add(this.label3);
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
			this.label15.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// tabDisplay
			// 
			this.tabDisplay.AccessibleDescription = resources.GetString("tabDisplay.AccessibleDescription");
			this.tabDisplay.AccessibleName = resources.GetString("tabDisplay.AccessibleName");
			this.tabDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabDisplay.Anchor")));
			this.tabDisplay.AutoScroll = ((bool)(resources.GetObject("tabDisplay.AutoScroll")));
			this.tabDisplay.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabDisplay.AutoScrollMargin")));
			this.tabDisplay.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabDisplay.AutoScrollMinSize")));
			this.tabDisplay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabDisplay.BackgroundImage")));
			this.tabDisplay.Controls.Add(this.checkCustomFormatter);
			this.tabDisplay.Controls.Add(this.labelFormatters);
			this.tabDisplay.Controls.Add(this.comboFormatters);
			this.tabDisplay.Controls.Add(this.label9);
			this.tabDisplay.Controls.Add(this.checkMarkItemsReadOnExit);
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
			this.labelFormatters.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// CategoryProperties
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
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.label2);
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
			this.Name = "CategoryProperties";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.tabControl.ResumeLayout(false);
			this.tabItemControl.ResumeLayout(false);
			this.tabDisplay.ResumeLayout(false);
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
	

	}
}
