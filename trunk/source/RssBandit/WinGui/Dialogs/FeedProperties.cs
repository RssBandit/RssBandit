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
using System.Windows.Forms;
using System.Collections.Generic;
using NewsComponents.Utils;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Feed Properties dialog.
	/// </summary>
	internal partial class FeedProperties : Form
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


		public FeedProperties(string title, string link, int refreshRate, TimeSpan maxItemAge, string currentCategory, string defaultCategory, ICollection<string> categories, string stylesheet): 
			this()
		{		

			this.textBox1.Text  = title; 
			this.textBox2.Text  = link;

			this.comboBox1.Items.Clear();
			if (!Utils.RefreshRateStrings.Contains(refreshRate.ToString()))
				Utils.RefreshRateStrings.Add(refreshRate.ToString());
			this.comboBox1.DataSource = Utils.RefreshRateStrings; 
			this.comboBox1.Text = refreshRate.ToString(); 
			
			tabAuthentication.Enabled = !RssHelper.IsNntpUrl(link);

			//initialize category combo box			
			foreach(string category in categories){
				if (!string.IsNullOrEmpty(category))
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

				if (!string.IsNullOrEmpty(stylesheet) &&
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
			ApplyComponentTranslations();
			this.Load += this.OnForm_Load;
		}

		void ApplyComponentTranslations()
		{
			this.comboMaxItemAge.Items.Clear();
			this.comboMaxItemAge.DataSource = Utils.MaxItemAgeStrings;
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

		private void OnForm_Load(object sender, EventArgs e) {
		 	// fix the wide screen Tab Control issue by resize ourselfs the panels at the first Tab:
			OnTabControl_Resize(this, EventArgs.Empty);
		}

	


	}
}
