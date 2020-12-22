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
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Collections.Generic;
using NewsComponents.Utils;
using RssBandit.WinGui.Utility;
using RssBandit.Resources;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Feed Properties dialog.
	/// </summary>
	internal partial class FeedProperties : DialogBase
	{
		private X509Certificate2 _clientCertificate;
		internal System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		internal System.Windows.Forms.TextBox textBox1;
		internal System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label5;
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
		private System.ComponentModel.IContainer components=null;


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
			ClientCertificate = null;

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
			InitializeComponentTranslation();
			
			this.btnRemoveCertificate.Click += this.OnButtonRemoveCertificateClick;
			this.btnSelectCertificate.Click += this.OnButtonSelectCertificateClick;
			this.btnViewCertificate.Click += this.OnButtonViewCertificateClick;
			
		}

		#region Localization

		protected override void InitializeComponentTranslation()
		{
			base.InitializeComponentTranslation();
			
			Text = DR.FeedProperties_Text;
			checkCustomFormatter.Text = DR.FeedProperties_checkCustomFormatter_Text;
			toolTip.SetToolTip(checkCustomFormatter, DR.FeedProperties_checkCustomFormatter_ToolTip);
			checkDownloadEnclosures.Text = DR.FeedProperties_checkDownloadEnclosures_Text;
			checkEnableAlerts.Text = DR.FeedProperties_checkEnableAlerts_Text;
			checkEnableEnclosureAlerts.Text = DR.FeedProperties_checkEnableEnclosureAlerts_Text;
			checkMarkItemsReadOnExit.Text = DR.FeedProperties_checkMarkItemsReadOnExit_Text;
			toolTip.SetToolTip(comboBox1, DR.FeedProperties_comboBox1_ToolTip);
			toolTip.SetToolTip(comboFormatters, DR.FeedProperties_comboFormatters_ToolTip);
			toolTip.SetToolTip(comboMaxItemAge, DR.FeedProperties_comboMaxItemAge_ToolTip);
			label1.Text = DR.FeedProperties_label1_Text;
			label15.Text = DR.FeedProperties_label15_Text;
			label2.Text = DR.FeedProperties_label2_Text;
			label3.Text = DR.FeedProperties_label3_Text;
			label4.Text = DR.FeedProperties_label4_Text;
			label5.Text = DR.FeedProperties_label5_Text;
			label6.Text = DR.FeedProperties_label6_Text;
			label7.Text = DR.FeedProperties_label7_Text;
			label9.Text = DR.FeedProperties_label9_Text;
			labelFormatters.Text = DR.FeedProperties_labelFormatters_Text;
			tabAttachments.Text = DR.FeedProperties_tabAttachments_Text;
			tabAuthentication.Text = DR.FeedProperties_tabAuthentication_Text;
			tabDisplay.Text = DR.FeedProperties_tabDisplay_Text;
			tabItemControl.Text = DR.FeedProperties_tabItemControl_Text;

			labelClientCertificate.Text = DR.FeedProperties_labelClientCertificate_Text;
			toolTip.SetToolTip(btnSelectCertificate, DR.FeedProperties_btnSelectClientCertificate_Tooltip);
			toolTip.SetToolTip(btnViewCertificate, DR.FeedProperties_btnViewClientCertificate_Tooltip);
			toolTip.SetToolTip(btnRemoveCertificate, DR.FeedProperties_btnRemoveClientCertificate_Tooltip);
			
			comboMaxItemAge.Items.Clear();
			comboMaxItemAge.DataSource = Utils.MaxItemAgeStrings;
		}
		#endregion
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

		public X509Certificate2 ClientCertificate
		{
			get { return _clientCertificate; }
			set
			{
				_clientCertificate = value;
				if (_clientCertificate != null)
				{
					btnRemoveCertificate.Enabled = true;
					btnViewCertificate.Enabled = true;

					if (!String.IsNullOrEmpty(_clientCertificate.FriendlyName))
						textCertificate.Text = String.Format("{0} / {1}", _clientCertificate.FriendlyName, _clientCertificate.GetExpirationDateString());
					else
						textCertificate.Text = String.Format("{0} / {1}", _clientCertificate.ToString(false), _clientCertificate.GetExpirationDateString());
				} 
				else
				{
					btnRemoveCertificate.Enabled = false;
					btnViewCertificate.Enabled = false;
					textCertificate.Text = String.Empty;
				}
			}
		}

		private void checkCustomFormatter_CheckedChanged(object sender, EventArgs e) {
			if (checkCustomFormatter.Checked) {
				labelFormatters.Enabled = comboFormatters.Enabled = true; 				
			}
			else {
				labelFormatters.Enabled = comboFormatters.Enabled = false;				
				comboFormatters.Text = String.Empty;
				comboFormatters.Refresh();
			}
		}
	
		private void OnButtonSelectCertificateClick(object sender, EventArgs e)
		{
			X509Certificate2 cert = CertificateHelper.SelectCertificate(
				SR.Certificate_SelectionDialog_Message);

			if (cert != null)
			{
				ClientCertificate = cert;
			}
		}

		private void OnButtonViewCertificateClick(object sender, EventArgs e)
		{
			if (ClientCertificate != null)
			{
				CertificateHelper.ShowCertificate(ClientCertificate, this.Handle);
			}
		}
		private void OnButtonRemoveCertificateClick(object sender, EventArgs e)
		{
			ClientCertificate = null;
		}


	}
}
