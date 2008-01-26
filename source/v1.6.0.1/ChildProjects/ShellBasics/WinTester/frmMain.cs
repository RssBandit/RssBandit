using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace WinTester4
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox edtFile;
		private System.Windows.Forms.Label lblFile;
		private System.Windows.Forms.Button btnConnectSimple;
		private System.Windows.Forms.GroupBox grpAutoAppend;
		private System.Windows.Forms.RadioButton rdoAutoAppendForceOn;
		private System.Windows.Forms.RadioButton rdoAutoAppendForceOff;
		private System.Windows.Forms.GroupBox grpAutoSuggest;
		private System.Windows.Forms.RadioButton rdoAutoSuggestForceOff;
		private System.Windows.Forms.RadioButton rdoAutoSuggestForceOn;
		private System.Windows.Forms.GroupBox grpFlags;
		private System.Windows.Forms.CheckBox chkFileSystem;
		private System.Windows.Forms.CheckBox chkUrlHistory;
		private System.Windows.Forms.CheckBox chkUrlMRU;
		private System.Windows.Forms.CheckBox chkUseTab;
		private System.Windows.Forms.CheckBox chkFileSysOnly;
		private System.Windows.Forms.CheckBox chkFileSysDirs;
		private System.Windows.Forms.GroupBox grpACOptions;
		private System.Windows.Forms.CheckBox chkACOAutoSuggest;
		private System.Windows.Forms.CheckBox chkACOAutoAppend;
		private System.Windows.Forms.CheckBox chkACOFilterPrefixs;
		private System.Windows.Forms.CheckBox chkACOSearch;
		private System.Windows.Forms.CheckBox chkACOUpDownKeyDropsList;
		private System.Windows.Forms.CheckBox chkACOUseTab;
		private System.Windows.Forms.CheckBox chkACORtlReading;
		private System.Windows.Forms.Button btnConnectObject;
		private System.Windows.Forms.GroupBox grpListSource;
		private System.Windows.Forms.RadioButton rdoMultiSource;
		private System.Windows.Forms.RadioButton rdoHistory;
		private System.Windows.Forms.RadioButton rdoMRU;
		private System.Windows.Forms.RadioButton rdoShellNamespace;
		private System.Windows.Forms.CheckBox chkHistory;
		private System.Windows.Forms.CheckBox chkMRU;
		private System.Windows.Forms.CheckBox chkShellNamespace;
		private System.Windows.Forms.RadioButton rdoCustomList;
		private System.Windows.Forms.CheckBox chkCustomList;
		private System.Windows.Forms.Button btnAddString;
		private System.Windows.Forms.TextBox txtString;
		private System.Windows.Forms.ListBox lstCustomList;
		private System.Windows.Forms.GroupBox grpCustomList;
		private System.Windows.Forms.ComboBox cmbFile;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmMain()
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
				if (components != null) 
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
			this.edtFile = new System.Windows.Forms.TextBox();
			this.lblFile = new System.Windows.Forms.Label();
			this.btnConnectSimple = new System.Windows.Forms.Button();
			this.grpAutoAppend = new System.Windows.Forms.GroupBox();
			this.rdoAutoAppendForceOff = new System.Windows.Forms.RadioButton();
			this.rdoAutoAppendForceOn = new System.Windows.Forms.RadioButton();
			this.grpAutoSuggest = new System.Windows.Forms.GroupBox();
			this.rdoAutoSuggestForceOff = new System.Windows.Forms.RadioButton();
			this.rdoAutoSuggestForceOn = new System.Windows.Forms.RadioButton();
			this.grpFlags = new System.Windows.Forms.GroupBox();
			this.chkFileSysDirs = new System.Windows.Forms.CheckBox();
			this.chkFileSysOnly = new System.Windows.Forms.CheckBox();
			this.chkUseTab = new System.Windows.Forms.CheckBox();
			this.chkUrlMRU = new System.Windows.Forms.CheckBox();
			this.chkUrlHistory = new System.Windows.Forms.CheckBox();
			this.chkFileSystem = new System.Windows.Forms.CheckBox();
			this.grpACOptions = new System.Windows.Forms.GroupBox();
			this.chkACORtlReading = new System.Windows.Forms.CheckBox();
			this.chkACOUseTab = new System.Windows.Forms.CheckBox();
			this.chkACOUpDownKeyDropsList = new System.Windows.Forms.CheckBox();
			this.chkACOSearch = new System.Windows.Forms.CheckBox();
			this.chkACOFilterPrefixs = new System.Windows.Forms.CheckBox();
			this.chkACOAutoAppend = new System.Windows.Forms.CheckBox();
			this.chkACOAutoSuggest = new System.Windows.Forms.CheckBox();
			this.btnConnectObject = new System.Windows.Forms.Button();
			this.grpListSource = new System.Windows.Forms.GroupBox();
			this.grpCustomList = new System.Windows.Forms.GroupBox();
			this.btnAddString = new System.Windows.Forms.Button();
			this.txtString = new System.Windows.Forms.TextBox();
			this.lstCustomList = new System.Windows.Forms.ListBox();
			this.chkCustomList = new System.Windows.Forms.CheckBox();
			this.rdoCustomList = new System.Windows.Forms.RadioButton();
			this.chkShellNamespace = new System.Windows.Forms.CheckBox();
			this.chkMRU = new System.Windows.Forms.CheckBox();
			this.chkHistory = new System.Windows.Forms.CheckBox();
			this.rdoShellNamespace = new System.Windows.Forms.RadioButton();
			this.rdoMRU = new System.Windows.Forms.RadioButton();
			this.rdoHistory = new System.Windows.Forms.RadioButton();
			this.rdoMultiSource = new System.Windows.Forms.RadioButton();
			this.cmbFile = new System.Windows.Forms.ComboBox();
			this.grpAutoAppend.SuspendLayout();
			this.grpAutoSuggest.SuspendLayout();
			this.grpFlags.SuspendLayout();
			this.grpACOptions.SuspendLayout();
			this.grpListSource.SuspendLayout();
			this.grpCustomList.SuspendLayout();
			this.SuspendLayout();
			// 
			// edtFile
			// 
			this.edtFile.Location = new System.Drawing.Point(80, 8);
			this.edtFile.Name = "edtFile";
			this.edtFile.Size = new System.Drawing.Size(400, 20);
			this.edtFile.TabIndex = 0;
			this.edtFile.Text = "";
			// 
			// lblFile
			// 
			this.lblFile.Location = new System.Drawing.Point(8, 10);
			this.lblFile.Name = "lblFile";
			this.lblFile.Size = new System.Drawing.Size(64, 16);
			this.lblFile.TabIndex = 1;
			this.lblFile.Text = "Select File:";
			// 
			// btnConnectSimple
			// 
			this.btnConnectSimple.Location = new System.Drawing.Point(8, 144);
			this.btnConnectSimple.Name = "btnConnectSimple";
			this.btnConnectSimple.Size = new System.Drawing.Size(472, 24);
			this.btnConnectSimple.TabIndex = 2;
			this.btnConnectSimple.Text = "Use SHAutoComplete Function - On EditBox";
			this.btnConnectSimple.Click += new System.EventHandler(this.btnConnectSimple_Click);
			// 
			// grpAutoAppend
			// 
			this.grpAutoAppend.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.rdoAutoAppendForceOff,
																						this.rdoAutoAppendForceOn});
			this.grpAutoAppend.Location = new System.Drawing.Point(376, 40);
			this.grpAutoAppend.Name = "grpAutoAppend";
			this.grpAutoAppend.Size = new System.Drawing.Size(104, 96);
			this.grpAutoAppend.TabIndex = 3;
			this.grpAutoAppend.TabStop = false;
			this.grpAutoAppend.Text = "Auto Append";
			// 
			// rdoAutoAppendForceOff
			// 
			this.rdoAutoAppendForceOff.Location = new System.Drawing.Point(16, 56);
			this.rdoAutoAppendForceOff.Name = "rdoAutoAppendForceOff";
			this.rdoAutoAppendForceOff.Size = new System.Drawing.Size(72, 24);
			this.rdoAutoAppendForceOff.TabIndex = 1;
			this.rdoAutoAppendForceOff.Text = "Force Off";
			// 
			// rdoAutoAppendForceOn
			// 
			this.rdoAutoAppendForceOn.Checked = true;
			this.rdoAutoAppendForceOn.Location = new System.Drawing.Point(16, 24);
			this.rdoAutoAppendForceOn.Name = "rdoAutoAppendForceOn";
			this.rdoAutoAppendForceOn.Size = new System.Drawing.Size(72, 24);
			this.rdoAutoAppendForceOn.TabIndex = 0;
			this.rdoAutoAppendForceOn.TabStop = true;
			this.rdoAutoAppendForceOn.Text = "Force On";
			// 
			// grpAutoSuggest
			// 
			this.grpAutoSuggest.Controls.AddRange(new System.Windows.Forms.Control[] {
																						 this.rdoAutoSuggestForceOff,
																						 this.rdoAutoSuggestForceOn});
			this.grpAutoSuggest.Location = new System.Drawing.Point(264, 40);
			this.grpAutoSuggest.Name = "grpAutoSuggest";
			this.grpAutoSuggest.Size = new System.Drawing.Size(104, 96);
			this.grpAutoSuggest.TabIndex = 4;
			this.grpAutoSuggest.TabStop = false;
			this.grpAutoSuggest.Text = "Auto Suggest";
			// 
			// rdoAutoSuggestForceOff
			// 
			this.rdoAutoSuggestForceOff.Location = new System.Drawing.Point(16, 56);
			this.rdoAutoSuggestForceOff.Name = "rdoAutoSuggestForceOff";
			this.rdoAutoSuggestForceOff.Size = new System.Drawing.Size(72, 24);
			this.rdoAutoSuggestForceOff.TabIndex = 3;
			this.rdoAutoSuggestForceOff.Text = "Force Off";
			// 
			// rdoAutoSuggestForceOn
			// 
			this.rdoAutoSuggestForceOn.Checked = true;
			this.rdoAutoSuggestForceOn.Location = new System.Drawing.Point(16, 24);
			this.rdoAutoSuggestForceOn.Name = "rdoAutoSuggestForceOn";
			this.rdoAutoSuggestForceOn.Size = new System.Drawing.Size(72, 24);
			this.rdoAutoSuggestForceOn.TabIndex = 2;
			this.rdoAutoSuggestForceOn.TabStop = true;
			this.rdoAutoSuggestForceOn.Text = "Force On";
			// 
			// grpFlags
			// 
			this.grpFlags.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.chkFileSysDirs,
																				   this.chkFileSysOnly,
																				   this.chkUseTab,
																				   this.chkUrlMRU,
																				   this.chkUrlHistory,
																				   this.chkFileSystem});
			this.grpFlags.Location = new System.Drawing.Point(8, 40);
			this.grpFlags.Name = "grpFlags";
			this.grpFlags.Size = new System.Drawing.Size(248, 96);
			this.grpFlags.TabIndex = 6;
			this.grpFlags.TabStop = false;
			this.grpFlags.Text = "Function Flags";
			// 
			// chkFileSysDirs
			// 
			this.chkFileSysDirs.Location = new System.Drawing.Point(128, 64);
			this.chkFileSysDirs.Name = "chkFileSysDirs";
			this.chkFileSysDirs.Size = new System.Drawing.Size(88, 24);
			this.chkFileSysDirs.TabIndex = 11;
			this.chkFileSysDirs.Text = "File Sys Dirs";
			// 
			// chkFileSysOnly
			// 
			this.chkFileSysOnly.Location = new System.Drawing.Point(128, 40);
			this.chkFileSysOnly.Name = "chkFileSysOnly";
			this.chkFileSysOnly.Size = new System.Drawing.Size(96, 24);
			this.chkFileSysOnly.TabIndex = 10;
			this.chkFileSysOnly.Text = "File Sys Only";
			// 
			// chkUseTab
			// 
			this.chkUseTab.Location = new System.Drawing.Point(128, 16);
			this.chkUseTab.Name = "chkUseTab";
			this.chkUseTab.Size = new System.Drawing.Size(96, 24);
			this.chkUseTab.TabIndex = 9;
			this.chkUseTab.Text = "Use Tab";
			// 
			// chkUrlMRU
			// 
			this.chkUrlMRU.Location = new System.Drawing.Point(24, 64);
			this.chkUrlMRU.Name = "chkUrlMRU";
			this.chkUrlMRU.Size = new System.Drawing.Size(88, 24);
			this.chkUrlMRU.TabIndex = 8;
			this.chkUrlMRU.Text = "Url MRU";
			// 
			// chkUrlHistory
			// 
			this.chkUrlHistory.Location = new System.Drawing.Point(24, 40);
			this.chkUrlHistory.Name = "chkUrlHistory";
			this.chkUrlHistory.Size = new System.Drawing.Size(88, 24);
			this.chkUrlHistory.TabIndex = 7;
			this.chkUrlHistory.Text = "Url History";
			// 
			// chkFileSystem
			// 
			this.chkFileSystem.Location = new System.Drawing.Point(24, 16);
			this.chkFileSystem.Name = "chkFileSystem";
			this.chkFileSystem.Size = new System.Drawing.Size(88, 24);
			this.chkFileSystem.TabIndex = 6;
			this.chkFileSystem.Text = "File System";
			// 
			// grpACOptions
			// 
			this.grpACOptions.Controls.AddRange(new System.Windows.Forms.Control[] {
																					   this.chkACORtlReading,
																					   this.chkACOUseTab,
																					   this.chkACOUpDownKeyDropsList,
																					   this.chkACOSearch,
																					   this.chkACOFilterPrefixs,
																					   this.chkACOAutoAppend,
																					   this.chkACOAutoSuggest});
			this.grpACOptions.Location = new System.Drawing.Point(8, 176);
			this.grpACOptions.Name = "grpACOptions";
			this.grpACOptions.Size = new System.Drawing.Size(208, 136);
			this.grpACOptions.TabIndex = 8;
			this.grpACOptions.TabStop = false;
			this.grpACOptions.Text = "AutoComplete Options";
			// 
			// chkACORtlReading
			// 
			this.chkACORtlReading.Location = new System.Drawing.Point(104, 72);
			this.chkACORtlReading.Name = "chkACORtlReading";
			this.chkACORtlReading.Size = new System.Drawing.Size(88, 16);
			this.chkACORtlReading.TabIndex = 6;
			this.chkACORtlReading.Text = "Rtl Reading";
			// 
			// chkACOUseTab
			// 
			this.chkACOUseTab.Location = new System.Drawing.Point(104, 48);
			this.chkACOUseTab.Name = "chkACOUseTab";
			this.chkACOUseTab.Size = new System.Drawing.Size(72, 16);
			this.chkACOUseTab.TabIndex = 5;
			this.chkACOUseTab.Text = "Use Tab";
			// 
			// chkACOUpDownKeyDropsList
			// 
			this.chkACOUpDownKeyDropsList.Checked = true;
			this.chkACOUpDownKeyDropsList.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkACOUpDownKeyDropsList.Location = new System.Drawing.Point(8, 96);
			this.chkACOUpDownKeyDropsList.Name = "chkACOUpDownKeyDropsList";
			this.chkACOUpDownKeyDropsList.Size = new System.Drawing.Size(152, 16);
			this.chkACOUpDownKeyDropsList.TabIndex = 4;
			this.chkACOUpDownKeyDropsList.Text = "Up Down Key Drops List";
			// 
			// chkACOSearch
			// 
			this.chkACOSearch.Location = new System.Drawing.Point(8, 72);
			this.chkACOSearch.Name = "chkACOSearch";
			this.chkACOSearch.Size = new System.Drawing.Size(72, 16);
			this.chkACOSearch.TabIndex = 3;
			this.chkACOSearch.Text = "Search";
			// 
			// chkACOFilterPrefixs
			// 
			this.chkACOFilterPrefixs.Location = new System.Drawing.Point(104, 24);
			this.chkACOFilterPrefixs.Name = "chkACOFilterPrefixs";
			this.chkACOFilterPrefixs.Size = new System.Drawing.Size(96, 16);
			this.chkACOFilterPrefixs.TabIndex = 2;
			this.chkACOFilterPrefixs.Text = "Filter Prefixes";
			// 
			// chkACOAutoAppend
			// 
			this.chkACOAutoAppend.Checked = true;
			this.chkACOAutoAppend.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkACOAutoAppend.Location = new System.Drawing.Point(8, 48);
			this.chkACOAutoAppend.Name = "chkACOAutoAppend";
			this.chkACOAutoAppend.Size = new System.Drawing.Size(96, 16);
			this.chkACOAutoAppend.TabIndex = 1;
			this.chkACOAutoAppend.Text = "Auto Append";
			// 
			// chkACOAutoSuggest
			// 
			this.chkACOAutoSuggest.Checked = true;
			this.chkACOAutoSuggest.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkACOAutoSuggest.Location = new System.Drawing.Point(8, 24);
			this.chkACOAutoSuggest.Name = "chkACOAutoSuggest";
			this.chkACOAutoSuggest.Size = new System.Drawing.Size(96, 16);
			this.chkACOAutoSuggest.TabIndex = 0;
			this.chkACOAutoSuggest.Text = "Auto Suggest";
			// 
			// btnConnectObject
			// 
			this.btnConnectObject.Location = new System.Drawing.Point(8, 320);
			this.btnConnectObject.Name = "btnConnectObject";
			this.btnConnectObject.Size = new System.Drawing.Size(472, 24);
			this.btnConnectObject.TabIndex = 9;
			this.btnConnectObject.Text = "Use AutoComplete Complex Object - On ComboBox";
			this.btnConnectObject.Click += new System.EventHandler(this.btnConnectObject_Click);
			// 
			// grpListSource
			// 
			this.grpListSource.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.grpCustomList,
																						this.chkCustomList,
																						this.rdoCustomList,
																						this.chkShellNamespace,
																						this.chkMRU,
																						this.chkHistory,
																						this.rdoShellNamespace,
																						this.rdoMRU,
																						this.rdoHistory,
																						this.rdoMultiSource});
			this.grpListSource.Location = new System.Drawing.Point(224, 176);
			this.grpListSource.Name = "grpListSource";
			this.grpListSource.Size = new System.Drawing.Size(256, 136);
			this.grpListSource.TabIndex = 10;
			this.grpListSource.TabStop = false;
			this.grpListSource.Text = "List Source";
			// 
			// grpCustomList
			// 
			this.grpCustomList.Controls.AddRange(new System.Windows.Forms.Control[] {
																						this.btnAddString,
																						this.txtString,
																						this.lstCustomList});
			this.grpCustomList.Enabled = false;
			this.grpCustomList.Location = new System.Drawing.Point(144, 16);
			this.grpCustomList.Name = "grpCustomList";
			this.grpCustomList.Size = new System.Drawing.Size(104, 112);
			this.grpCustomList.TabIndex = 12;
			this.grpCustomList.TabStop = false;
			this.grpCustomList.Text = "Custom List";
			// 
			// btnAddString
			// 
			this.btnAddString.Location = new System.Drawing.Point(8, 40);
			this.btnAddString.Name = "btnAddString";
			this.btnAddString.Size = new System.Drawing.Size(88, 24);
			this.btnAddString.TabIndex = 14;
			this.btnAddString.Text = "Add String";
			this.btnAddString.Click += new System.EventHandler(this.btnAddString_Click);
			// 
			// txtString
			// 
			this.txtString.Location = new System.Drawing.Point(8, 16);
			this.txtString.Name = "txtString";
			this.txtString.Size = new System.Drawing.Size(88, 20);
			this.txtString.TabIndex = 13;
			this.txtString.Text = "";
			// 
			// lstCustomList
			// 
			this.lstCustomList.Items.AddRange(new object[] {
															   "This",
															   "Is",
															   "A",
															   "Custom",
															   "List"});
			this.lstCustomList.Location = new System.Drawing.Point(8, 72);
			this.lstCustomList.Name = "lstCustomList";
			this.lstCustomList.Size = new System.Drawing.Size(88, 30);
			this.lstCustomList.TabIndex = 12;
			// 
			// chkCustomList
			// 
			this.chkCustomList.Location = new System.Drawing.Point(8, 112);
			this.chkCustomList.Name = "chkCustomList";
			this.chkCustomList.Size = new System.Drawing.Size(16, 16);
			this.chkCustomList.TabIndex = 8;
			this.chkCustomList.CheckedChanged += new System.EventHandler(this.chkCustomList_CheckedChanged);
			// 
			// rdoCustomList
			// 
			this.rdoCustomList.Location = new System.Drawing.Point(24, 112);
			this.rdoCustomList.Name = "rdoCustomList";
			this.rdoCustomList.Size = new System.Drawing.Size(104, 16);
			this.rdoCustomList.TabIndex = 7;
			this.rdoCustomList.Text = "My Custom List";
			this.rdoCustomList.CheckedChanged += new System.EventHandler(this.rdoCustomList_CheckedChanged);
			// 
			// chkShellNamespace
			// 
			this.chkShellNamespace.Location = new System.Drawing.Point(8, 88);
			this.chkShellNamespace.Name = "chkShellNamespace";
			this.chkShellNamespace.Size = new System.Drawing.Size(16, 16);
			this.chkShellNamespace.TabIndex = 6;
			// 
			// chkMRU
			// 
			this.chkMRU.Location = new System.Drawing.Point(8, 64);
			this.chkMRU.Name = "chkMRU";
			this.chkMRU.Size = new System.Drawing.Size(16, 16);
			this.chkMRU.TabIndex = 5;
			// 
			// chkHistory
			// 
			this.chkHistory.Location = new System.Drawing.Point(8, 40);
			this.chkHistory.Name = "chkHistory";
			this.chkHistory.Size = new System.Drawing.Size(16, 16);
			this.chkHistory.TabIndex = 4;
			// 
			// rdoShellNamespace
			// 
			this.rdoShellNamespace.Location = new System.Drawing.Point(24, 88);
			this.rdoShellNamespace.Name = "rdoShellNamespace";
			this.rdoShellNamespace.Size = new System.Drawing.Size(112, 16);
			this.rdoShellNamespace.TabIndex = 3;
			this.rdoShellNamespace.Text = "Shell Namespace";
			this.rdoShellNamespace.CheckedChanged += new System.EventHandler(this.rdoCustomList_CheckedChanged);
			// 
			// rdoMRU
			// 
			this.rdoMRU.Location = new System.Drawing.Point(24, 64);
			this.rdoMRU.Name = "rdoMRU";
			this.rdoMRU.Size = new System.Drawing.Size(128, 16);
			this.rdoMRU.TabIndex = 2;
			this.rdoMRU.Text = "Most Recently Used";
			this.rdoMRU.CheckedChanged += new System.EventHandler(this.rdoCustomList_CheckedChanged);
			// 
			// rdoHistory
			// 
			this.rdoHistory.Location = new System.Drawing.Point(24, 40);
			this.rdoHistory.Name = "rdoHistory";
			this.rdoHistory.Size = new System.Drawing.Size(88, 16);
			this.rdoHistory.TabIndex = 1;
			this.rdoHistory.Text = "History";
			this.rdoHistory.CheckedChanged += new System.EventHandler(this.rdoCustomList_CheckedChanged);
			// 
			// rdoMultiSource
			// 
			this.rdoMultiSource.Checked = true;
			this.rdoMultiSource.Location = new System.Drawing.Point(24, 16);
			this.rdoMultiSource.Name = "rdoMultiSource";
			this.rdoMultiSource.Size = new System.Drawing.Size(96, 16);
			this.rdoMultiSource.TabIndex = 0;
			this.rdoMultiSource.TabStop = true;
			this.rdoMultiSource.Text = "Multi Sources";
			this.rdoMultiSource.CheckedChanged += new System.EventHandler(this.rdoMultiSource_CheckedChanged);
			// 
			// cmbFile
			// 
			this.cmbFile.Location = new System.Drawing.Point(8, 352);
			this.cmbFile.Name = "cmbFile";
			this.cmbFile.Size = new System.Drawing.Size(472, 21);
			this.cmbFile.TabIndex = 11;
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(486, 379);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.cmbFile,
																		  this.grpListSource,
																		  this.btnConnectObject,
																		  this.grpACOptions,
																		  this.grpFlags,
																		  this.grpAutoSuggest,
																		  this.grpAutoAppend,
																		  this.btnConnectSimple,
																		  this.lblFile,
																		  this.edtFile});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "frmMain";
			this.Text = "WinTester for Part 4";
			this.grpAutoAppend.ResumeLayout(false);
			this.grpAutoSuggest.ResumeLayout(false);
			this.grpFlags.ResumeLayout(false);
			this.grpACOptions.ResumeLayout(false);
			this.grpListSource.ResumeLayout(false);
			this.grpCustomList.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmMain());
		}

		private void btnConnectSimple_Click(object sender, System.EventArgs e)
		{
			ShellLib.ShellAutoComplete.AutoCompleteFlags flags = 0;

			flags |= (chkFileSystem.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.FileSystem : 0;
			flags |= (chkUrlHistory.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.UrlHistory : 0;
			flags |= (chkUrlMRU.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.UrlMRU : 0;
			flags |= (chkUseTab.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.UseTab : 0;
			flags |= (chkFileSysOnly.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.FileSys_Only : 0;
			flags |= (chkFileSysDirs.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.FileSys_Dirs : 0;
			flags |= (rdoAutoAppendForceOff.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.AutoAppend_Force_Off : 0;
			flags |= (rdoAutoAppendForceOn.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.AutoAppend_Force_On : 0;
			flags |= (rdoAutoSuggestForceOff.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.AutoSuggest_Force_Off : 0;
			flags |= (rdoAutoSuggestForceOn.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteFlags.AutoSuggest_Force_On : 0;

			ShellLib.ShellAutoComplete.DoAutoComplete(edtFile.Handle,flags);
		}

		private void btnConnectObject_Click(object sender, System.EventArgs e)
		{
			
			// create an AutoComplete object
			ShellLib.ShellAutoComplete ac = new ShellLib.ShellAutoComplete();
			
			// set combo handle
			ShellLib.ShellApi.ComboBoxInfo info = new ShellLib.ShellApi.ComboBoxInfo();
			info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
			if (ShellLib.ShellApi.GetComboBoxInfo(cmbFile.Handle, ref info))
			{
				if (info.hwndEdit != IntPtr.Zero)
					ac.EditHandle = info.hwndEdit;
				else
				{
					throw new Exception("ComboBox must have the DropDown style!");
				}
			} 
			
			// set options
			ac.ACOptions = ShellLib.ShellAutoComplete.AutoCompleteOptions.None;
			ac.ACOptions |= (chkACOAutoSuggest.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.AutoSuggest : 0;
			ac.ACOptions |= (chkACOAutoAppend.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.AutoAppend : 0;
			ac.ACOptions |= (chkACOSearch.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.Search : 0;
			ac.ACOptions |= (chkACOUpDownKeyDropsList.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.UpDownKeyDropsList : 0;
			ac.ACOptions |= (chkACOFilterPrefixs.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.FilterPreFixes : 0;
			ac.ACOptions |= (chkACOUseTab.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.UseTab : 0;
			ac.ACOptions |= (chkACORtlReading.Checked) ? ShellLib.ShellAutoComplete.AutoCompleteOptions.RtlReading : 0;
			
			// set source
			if (rdoMultiSource.Checked)
			{
				if ((!chkHistory.Checked) 
					&& (!chkMRU.Checked) 
					&& (!chkShellNamespace.Checked)
					&& (!chkCustomList.Checked))
				{
					MessageBox.Show("At least one source should be checked!");
					return;
				}

				ShellLib.IObjMgr multi = (ShellLib.IObjMgr)ShellLib.ShellAutoComplete.GetACLMulti();
				if (chkHistory.Checked)
					multi.Append(ShellLib.ShellAutoComplete.GetACLHistory());
				if (chkMRU.Checked)
					multi.Append(ShellLib.ShellAutoComplete.GetACLMRU());
				if (chkShellNamespace.Checked)
					multi.Append(ShellLib.ShellAutoComplete.GetACListISF());
				if (chkCustomList.Checked)
				{
					ShellLib.SourceCustomList custom = new ShellLib.SourceCustomList();
					custom.StringList = GetCustomList();
					multi.Append(custom);
				}
				ac.ListSource = multi;
			}
			else if (rdoHistory.Checked)
				ac.ListSource = ShellLib.ShellAutoComplete.GetACLHistory();
			else if (rdoMRU.Checked)
				ac.ListSource = ShellLib.ShellAutoComplete.GetACLMRU();
			else if (rdoShellNamespace.Checked)
				ac.ListSource = ShellLib.ShellAutoComplete.GetACListISF();
			else if (rdoCustomList.Checked)
			{
				ShellLib.SourceCustomList custom = new ShellLib.SourceCustomList();
				custom.StringList = GetCustomList();
				ac.ListSource = custom;
			}
			
			// activate AutoComplete
			ac.SetAutoComplete(true);
			// does not work...:
			//ac.SetAutoComplete(true, "http://www.%s.com/");

		}

		private void rdoMultiSource_CheckedChanged(object sender, System.EventArgs e)
		{
			chkHistory.Enabled = rdoMultiSource.Checked;
			chkMRU.Enabled = rdoMultiSource.Checked;
			chkShellNamespace.Enabled = rdoMultiSource.Checked;
			chkCustomList.Enabled = rdoMultiSource.Checked;
			grpCustomList.Enabled = chkCustomList.Checked;
		}

		private void rdoCustomList_CheckedChanged(object sender, System.EventArgs e)
		{
			grpCustomList.Enabled = rdoCustomList.Checked;
		}

		private void chkCustomList_CheckedChanged(object sender, System.EventArgs e)
		{
			grpCustomList.Enabled = chkCustomList.Checked;
		}

		private void btnAddString_Click(object sender, System.EventArgs e)
		{
			lstCustomList.Items.Insert(lstCustomList.Items.Count,txtString.Text);
		}

		private String[] GetCustomList()
		{
			String[] retArray = new String[lstCustomList.Items.Count];
			int current;

			for (current=0 ; current<lstCustomList.Items.Count ; current++)
			{
				retArray[current] = (String)lstCustomList.Items[current];
			}

			return retArray;
		}
		

	}
}
