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
using RssBandit.Resources;
using RssBandit.WinGui.Utility;
using RssBandit.WinGui.Controls;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Summary description for CategoryProperties.
	/// </summary>
	public partial class CategoryProperties : Form
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
		private System.Windows.Forms.Panel panelFeeds;
		private System.Windows.Forms.TabPage tabAttachments;
		internal System.Windows.Forms.CheckBox checkEnableEnclosureAlerts;
		internal System.Windows.Forms.CheckBox checkDownloadEnclosures;
	
		private System.ComponentModel.IContainer components;


		public CategoryProperties(string title, int refreshRate, TimeSpan maxItemAge, string stylesheet): this(){		
			
			this.textBox2.Text  = title;
			
			this.comboBox1.Items.Clear();
			if (!Utils.RefreshRateStrings.Contains(refreshRate.ToString()))
				Utils.RefreshRateStrings.Add(refreshRate.ToString());
			this.comboBox1.DataSource = Utils.RefreshRateStrings;
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
			ApplyComponentTranslations();
			this.Load += this.OnCategoryProperties_Load;
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
			panelFeeds.SetBounds(0,0, tabControl.Width - 2*panelFeeds.Location.X, 0, BoundsSpecified.Width);
		}

		private void OnCategoryProperties_Load(object sender, EventArgs e) {
			// fix the wide screen Tab Control issue by resize ourselfs the panels at the first Tab:
			OnTabControl_Resize(this, EventArgs.Empty);
		}

		

	}
}
