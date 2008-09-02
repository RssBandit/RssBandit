using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RssBandit.WinGui.Dialogs
{
	partial class FeedProperties
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeedProperties));
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
			this.comboFormatters = new System.Windows.Forms.ComboBox();
			this.checkCustomFormatter = new System.Windows.Forms.CheckBox();
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
			resources.ApplyResources(this.button2, "button2");
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Name = "button2";
			// 
			// button1
			// 
			resources.ApplyResources(this.button1, "button1");
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Name = "button1";
			// 
			// textBox2
			// 
			this.textBox2.AllowDrop = true;
			resources.ApplyResources(this.textBox2, "textBox2");
			this.textBox2.Name = "textBox2";
			// 
			// label2
			// 
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// textBox1
			// 
			resources.ApplyResources(this.textBox1, "textBox1");
			this.textBox1.Name = "textBox1";
			// 
			// comboBox2
			// 
			resources.ApplyResources(this.comboBox2, "comboBox2");
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Sorted = true;
			// 
			// label5
			// 
			this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			// 
			// comboMaxItemAge
			// 
			resources.ApplyResources(this.comboMaxItemAge, "comboMaxItemAge");
			this.comboMaxItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboMaxItemAge.Name = "comboMaxItemAge";
			this.toolTip1.SetToolTip(this.comboMaxItemAge, resources.GetString("comboMaxItemAge.ToolTip"));
			// 
			// comboFormatters
			// 
			resources.ApplyResources(this.comboFormatters, "comboFormatters");
			this.comboFormatters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFormatters.Name = "comboFormatters";
			this.comboFormatters.Sorted = true;
			this.toolTip1.SetToolTip(this.comboFormatters, resources.GetString("comboFormatters.ToolTip"));
			// 
			// checkCustomFormatter
			// 
			resources.ApplyResources(this.checkCustomFormatter, "checkCustomFormatter");
			this.checkCustomFormatter.Name = "checkCustomFormatter";
			this.toolTip1.SetToolTip(this.checkCustomFormatter, resources.GetString("checkCustomFormatter.ToolTip"));
			this.checkCustomFormatter.CheckedChanged += new System.EventHandler(this.checkCustomFormatter_CheckedChanged);
			// 
			// tabControl
			// 
			resources.ApplyResources(this.tabControl, "tabControl");
			this.tabControl.Controls.Add(this.tabItemControl);
			this.tabControl.Controls.Add(this.tabAuthentication);
			this.tabControl.Controls.Add(this.tabDisplay);
			this.tabControl.Controls.Add(this.tabAttachments);
			this.tabControl.HotTrack = true;
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Resize += new System.EventHandler(this.OnTabControl_Resize);
			// 
			// tabItemControl
			// 
			this.tabItemControl.Controls.Add(this.panelItemControl);
			resources.ApplyResources(this.tabItemControl, "tabItemControl");
			this.tabItemControl.Name = "tabItemControl";
			// 
			// panelItemControl
			// 
			resources.ApplyResources(this.panelItemControl, "panelItemControl");
			this.panelItemControl.Controls.Add(this.label4);
			this.panelItemControl.Controls.Add(this.label3);
			this.panelItemControl.Controls.Add(this.comboBox1);
			this.panelItemControl.Controls.Add(this.label15);
			this.panelItemControl.Controls.Add(this.comboMaxItemAge);
			this.panelItemControl.Name = "panelItemControl";
			// 
			// label4
			// 
			resources.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// comboBox1
			// 
			resources.ApplyResources(this.comboBox1, "comboBox1");
			this.comboBox1.Name = "comboBox1";
			// 
			// label15
			// 
			resources.ApplyResources(this.label15, "label15");
			this.label15.Name = "label15";
			// 
			// tabAuthentication
			// 
			this.tabAuthentication.Controls.Add(this.textPwd);
			this.tabAuthentication.Controls.Add(this.label7);
			this.tabAuthentication.Controls.Add(this.textUser);
			this.tabAuthentication.Controls.Add(this.label6);
			resources.ApplyResources(this.tabAuthentication, "tabAuthentication");
			this.tabAuthentication.Name = "tabAuthentication";
			// 
			// textPwd
			// 
			this.textPwd.AllowDrop = true;
			resources.ApplyResources(this.textPwd, "textPwd");
			this.textPwd.Name = "textPwd";
			// 
			// label7
			// 
			resources.ApplyResources(this.label7, "label7");
			this.label7.Name = "label7";
			// 
			// textUser
			// 
			this.textUser.AllowDrop = true;
			resources.ApplyResources(this.textUser, "textUser");
			this.textUser.Name = "textUser";
			// 
			// label6
			// 
			resources.ApplyResources(this.label6, "label6");
			this.label6.Name = "label6";
			// 
			// tabDisplay
			// 
			this.tabDisplay.Controls.Add(this.comboFormatters);
			this.tabDisplay.Controls.Add(this.checkCustomFormatter);
			this.tabDisplay.Controls.Add(this.labelFormatters);
			this.tabDisplay.Controls.Add(this.label9);
			resources.ApplyResources(this.tabDisplay, "tabDisplay");
			this.tabDisplay.Name = "tabDisplay";
			// 
			// labelFormatters
			// 
			resources.ApplyResources(this.labelFormatters, "labelFormatters");
			this.labelFormatters.Name = "labelFormatters";
			// 
			// label9
			// 
			resources.ApplyResources(this.label9, "label9");
			this.label9.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label9.Name = "label9";
			// 
			// tabAttachments
			// 
			this.tabAttachments.Controls.Add(this.checkDownloadEnclosures);
			this.tabAttachments.Controls.Add(this.checkEnableEnclosureAlerts);
			this.tabAttachments.Controls.Add(this.textBox3);
			this.tabAttachments.Controls.Add(this.label11);
			resources.ApplyResources(this.tabAttachments, "tabAttachments");
			this.tabAttachments.Name = "tabAttachments";
			// 
			// checkDownloadEnclosures
			// 
			resources.ApplyResources(this.checkDownloadEnclosures, "checkDownloadEnclosures");
			this.checkDownloadEnclosures.Name = "checkDownloadEnclosures";
			// 
			// checkEnableEnclosureAlerts
			// 
			resources.ApplyResources(this.checkEnableEnclosureAlerts, "checkEnableEnclosureAlerts");
			this.checkEnableEnclosureAlerts.Name = "checkEnableEnclosureAlerts";
			// 
			// textBox3
			// 
			this.textBox3.AllowDrop = true;
			resources.ApplyResources(this.textBox3, "textBox3");
			this.textBox3.Name = "textBox3";
			// 
			// label11
			// 
			resources.ApplyResources(this.label11, "label11");
			this.label11.Name = "label11";
			// 
			// label8
			// 
			resources.ApplyResources(this.label8, "label8");
			this.label8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label8.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label8.Name = "label8";
			// 
			// checkEnableAlerts
			// 
			resources.ApplyResources(this.checkEnableAlerts, "checkEnableAlerts");
			this.checkEnableAlerts.Name = "checkEnableAlerts";
			// 
			// checkMarkItemsReadOnExit
			// 
			resources.ApplyResources(this.checkMarkItemsReadOnExit, "checkMarkItemsReadOnExit");
			this.checkMarkItemsReadOnExit.Name = "checkMarkItemsReadOnExit";
			// 
			// FeedProperties
			// 
			this.AcceptButton = this.button1;
			resources.ApplyResources(this, "$this");
			this.CancelButton = this.button2;
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
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FeedProperties";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.tabControl.ResumeLayout(false);
			this.tabItemControl.ResumeLayout(false);
			this.panelItemControl.ResumeLayout(false);
			this.tabAuthentication.ResumeLayout(false);
			this.tabAuthentication.PerformLayout();
			this.tabDisplay.ResumeLayout(false);
			this.tabAttachments.ResumeLayout(false);
			this.tabAttachments.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion


	}
}
