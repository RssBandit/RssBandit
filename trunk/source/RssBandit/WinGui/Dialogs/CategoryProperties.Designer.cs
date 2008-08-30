
namespace RssBandit.WinGui.Dialogs
{
	partial class CategoryProperties
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CategoryProperties));
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
			this.panelFeeds = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label15 = new System.Windows.Forms.Label();
			this.tabDisplay = new System.Windows.Forms.TabPage();
			this.checkCustomFormatter = new System.Windows.Forms.CheckBox();
			this.labelFormatters = new System.Windows.Forms.Label();
			this.comboFormatters = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.checkMarkItemsReadOnExit = new System.Windows.Forms.CheckBox();
			this.tabAttachments = new System.Windows.Forms.TabPage();
			this.checkDownloadEnclosures = new System.Windows.Forms.CheckBox();
			this.checkEnableEnclosureAlerts = new System.Windows.Forms.CheckBox();
			this.tabControl.SuspendLayout();
			this.tabItemControl.SuspendLayout();
			this.panelFeeds.SuspendLayout();
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
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// comboBox2
			// 
			resources.ApplyResources(this.comboBox2, "comboBox2");
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Sorted = true;
			// 
			// label5
			// 
			resources.ApplyResources(this.label5, "label5");
			this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label5.Name = "label5";
			// 
			// comboMaxItemAge
			// 
			resources.ApplyResources(this.comboMaxItemAge, "comboMaxItemAge");
			this.comboMaxItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboMaxItemAge.Name = "comboMaxItemAge";
			this.toolTip1.SetToolTip(this.comboMaxItemAge, resources.GetString("comboMaxItemAge.ToolTip"));
			// 
			// tabControl
			// 
			resources.ApplyResources(this.tabControl, "tabControl");
			this.tabControl.Controls.Add(this.tabItemControl);
			this.tabControl.Controls.Add(this.tabDisplay);
			this.tabControl.Controls.Add(this.tabAttachments);
			this.tabControl.HotTrack = true;
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Resize += new System.EventHandler(this.OnTabControl_Resize);
			// 
			// tabItemControl
			// 
			this.tabItemControl.Controls.Add(this.panelFeeds);
			resources.ApplyResources(this.tabItemControl, "tabItemControl");
			this.tabItemControl.Name = "tabItemControl";
			// 
			// panelFeeds
			// 
			resources.ApplyResources(this.panelFeeds, "panelFeeds");
			this.panelFeeds.Controls.Add(this.label4);
			this.panelFeeds.Controls.Add(this.label3);
			this.panelFeeds.Controls.Add(this.comboBox1);
			this.panelFeeds.Controls.Add(this.comboMaxItemAge);
			this.panelFeeds.Controls.Add(this.label15);
			this.panelFeeds.Name = "panelFeeds";
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
			this.comboBox1.Name = "comboBox1";
			// 
			// label15
			// 
			resources.ApplyResources(this.label15, "label15");
			this.label15.Name = "label15";
			// 
			// tabDisplay
			// 
			this.tabDisplay.Controls.Add(this.checkCustomFormatter);
			this.tabDisplay.Controls.Add(this.labelFormatters);
			this.tabDisplay.Controls.Add(this.comboFormatters);
			this.tabDisplay.Controls.Add(this.label9);
			this.tabDisplay.Controls.Add(this.checkMarkItemsReadOnExit);
			resources.ApplyResources(this.tabDisplay, "tabDisplay");
			this.tabDisplay.Name = "tabDisplay";
			// 
			// checkCustomFormatter
			// 
			resources.ApplyResources(this.checkCustomFormatter, "checkCustomFormatter");
			this.checkCustomFormatter.Name = "checkCustomFormatter";
			this.toolTip1.SetToolTip(this.checkCustomFormatter, resources.GetString("checkCustomFormatter.ToolTip"));
			this.checkCustomFormatter.CheckedChanged += new System.EventHandler(this.checkCustomFormatter_CheckedChanged);
			// 
			// labelFormatters
			// 
			resources.ApplyResources(this.labelFormatters, "labelFormatters");
			this.labelFormatters.Name = "labelFormatters";
			// 
			// comboFormatters
			// 
			resources.ApplyResources(this.comboFormatters, "comboFormatters");
			this.comboFormatters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFormatters.Name = "comboFormatters";
			this.comboFormatters.Sorted = true;
			this.toolTip1.SetToolTip(this.comboFormatters, resources.GetString("comboFormatters.ToolTip"));
			// 
			// label9
			// 
			resources.ApplyResources(this.label9, "label9");
			this.label9.Name = "label9";
			// 
			// checkMarkItemsReadOnExit
			// 
			resources.ApplyResources(this.checkMarkItemsReadOnExit, "checkMarkItemsReadOnExit");
			this.checkMarkItemsReadOnExit.Name = "checkMarkItemsReadOnExit";
			// 
			// tabAttachments
			// 
			this.tabAttachments.Controls.Add(this.checkDownloadEnclosures);
			this.tabAttachments.Controls.Add(this.checkEnableEnclosureAlerts);
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
			// CategoryProperties
			// 
			this.AcceptButton = this.button1;
			resources.ApplyResources(this, "$this");
			this.CancelButton = this.button2;
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CategoryProperties";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.tabControl.ResumeLayout(false);
			this.tabItemControl.ResumeLayout(false);
			this.panelFeeds.ResumeLayout(false);
			this.tabDisplay.ResumeLayout(false);
			this.tabAttachments.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

	}
}
