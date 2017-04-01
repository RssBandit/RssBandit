
using System.Windows.Forms;
using RssBandit.WinGui.Controls;


namespace RssBandit.WinGui.Dialogs
{
	internal partial class PreferencesDialog
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferencesDialog));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabPrefs = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.optionSectionPanel1 = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.checkAllowAppEventSounds = new System.Windows.Forms.CheckBox();
            this.btnConfigureAppSounds = new System.Windows.Forms.Button();
            this.sectionPanelGeneralStartup = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.checkRunAtStartup = new System.Windows.Forms.CheckBox();
            this.checkRefreshFeedsOnStartup = new System.Windows.Forms.CheckBox();
            this.sectionPanelGeneralBehavior = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.label12 = new System.Windows.Forms.Label();
            this.radioTrayActionNone = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.radioTrayActionClose = new System.Windows.Forms.RadioButton();
            this.radioTrayActionMinimize = new System.Windows.Forms.RadioButton();
            this.tabRemoteStorage = new System.Windows.Forms.TabPage();
            this.sectionPanelRemoteStorageFeedlist = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.textRemoteStorageLocation = new System.Windows.Forms.TextBox();
            this.textRemoteStoragePassword = new System.Windows.Forms.TextBox();
            this.textRemoteStorageUserName = new System.Windows.Forms.TextBox();
            this.comboRemoteStorageProtocol = new System.Windows.Forms.ComboBox();
            this.checkUseRemoteStorage = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.labelRemoteStorageLocation = new System.Windows.Forms.Label();
            this.labelRemoteStoragePassword = new System.Windows.Forms.Label();
            this.labelRemoteStorageUserName = new System.Windows.Forms.Label();
            this.labelRemoteStorageProtocol = new System.Windows.Forms.Label();
            this.labelExperimental = new System.Windows.Forms.Label();
            this.tabNewsItems = new System.Windows.Forms.TabPage();
            this.sectionPanelDisplayGeneral = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.checkUseFavicons = new System.Windows.Forms.CheckBox();
            this.checkMarkItemsReadOnExit = new System.Windows.Forms.CheckBox();
            this.chkNewsItemOpenLinkInDetailWindow = new System.Windows.Forms.CheckBox();
            this.sectionPanelDisplayItemFormatting = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.checkMarkItemsAsReadWhenViewed = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.numNewsItemsPerPage = new System.Windows.Forms.NumericUpDown();
            this.checkLimitNewsItemsPerPage = new System.Windows.Forms.CheckBox();
            this.comboFormatters = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkCustomFormatter = new System.Windows.Forms.CheckBox();
            this.labelFormatters = new System.Windows.Forms.Label();
            this.tabFeeds = new System.Windows.Forms.TabPage();
            this.sectionPanelFeedsCommentDefs = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.cboUserIdentityForComments = new System.Windows.Forms.ComboBox();
            this.btnManageIdentities = new System.Windows.Forms.Button();
            this.linkCommentAPI = new System.Windows.Forms.LinkLabel();
            this.lblIdentityDropdownCaption = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.sectionPanelFeedsTimings = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.checkClearFeedCategoryItemAgeSettings = new System.Windows.Forms.CheckBox();
            this.checkResetIndividualRefreshRates = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.comboRefreshRate = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboMaxItemAge = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabWebBrowser = new System.Windows.Forms.TabPage();
            this.sectionPanelWebBrowserSecurity = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.checkBrowserActiveXAllowed = new System.Windows.Forms.CheckBox();
            this.checkBrowserBGSoundAllowed = new System.Windows.Forms.CheckBox();
            this.lblCheckImage = new System.Windows.Forms.Label();
            this.labelCheckToAllow = new System.Windows.Forms.Label();
            this.checkBrowserJavascriptAllowed = new System.Windows.Forms.CheckBox();
            this.checkBrowserJavaAllowed = new System.Windows.Forms.CheckBox();
            this.checkBrowserVideoAllowed = new System.Windows.Forms.CheckBox();
            this.checkBrowserImagesAllowed = new System.Windows.Forms.CheckBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.sectionPanelWebBrowserOnNewWindow = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.checkOpenTabsInBackground = new System.Windows.Forms.CheckBox();
            this.optNewWindowCustomExec = new System.Windows.Forms.RadioButton();
            this.optNewWindowDefaultWebBrowser = new System.Windows.Forms.RadioButton();
            this.optNewWindowOnTab = new System.Windows.Forms.RadioButton();
            this.checkReuseFirstBrowserTab = new System.Windows.Forms.CheckBox();
            this.btnSelectExecutable = new System.Windows.Forms.Button();
            this.txtBrowserStartExecutable = new System.Windows.Forms.TextBox();
            this.labelBrowserStartExecutable = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.tabWebSearch = new System.Windows.Forms.TabPage();
            this.sectionPanelWebSearchEngines = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.btnSERemove = new System.Windows.Forms.Button();
            this.btnSEAdd = new System.Windows.Forms.Button();
            this.btnSEProperties = new System.Windows.Forms.Button();
            this.btnSEMoveDown = new System.Windows.Forms.Button();
            this.btnSEMoveUp = new System.Windows.Forms.Button();
            this.listSearchEngines = new System.Windows.Forms.ListView();
            this.columnHeader0 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.imagesSearchEngines = new System.Windows.Forms.ImageList(this.components);
            this.label17 = new System.Windows.Forms.Label();
            this.tabNetConnection = new System.Windows.Forms.TabPage();
            this.sectionPanelNetworkConnectionProxy = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.textProxyBypassList = new System.Windows.Forms.TextBox();
            this.checkNoProxy = new System.Windows.Forms.RadioButton();
            this.textProxyCredentialPassword = new System.Windows.Forms.TextBox();
            this.labelProxyCredentialPassword = new System.Windows.Forms.Label();
            this.labelProxyCredentialUserName = new System.Windows.Forms.Label();
            this.textProxyCredentialUser = new System.Windows.Forms.TextBox();
            this.checkProxyAuth = new System.Windows.Forms.CheckBox();
            this.labelProxyBypassListHint = new System.Windows.Forms.Label();
            this.labelProxyBypassList = new System.Windows.Forms.Label();
            this.checkUseProxy = new System.Windows.Forms.RadioButton();
            this.checkUseIEProxySettings = new System.Windows.Forms.RadioButton();
            this.labelProxyServerSummery = new System.Windows.Forms.Label();
            this.textProxyPort = new System.Windows.Forms.TextBox();
            this.labelProxyPort = new System.Windows.Forms.Label();
            this.labelProxyAddress = new System.Windows.Forms.Label();
            this.textProxyAddress = new System.Windows.Forms.TextBox();
            this.checkProxyBypassLocal = new System.Windows.Forms.CheckBox();
            this.tabFonts = new System.Windows.Forms.TabPage();
            this.sectionPanelFontsSubscriptions = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.labelFontsSubscriptionsSummery = new System.Windows.Forms.Label();
            this.btnChangeFont = new System.Windows.Forms.Button();
            this.lblUsedFontNameSize = new System.Windows.Forms.Label();
            this.chkFontItalic = new System.Windows.Forms.CheckBox();
            this.chkFontBold = new System.Windows.Forms.CheckBox();
            this.btnChangeColor = new System.Windows.Forms.Button();
            this.lblItemStates = new System.Windows.Forms.Label();
            this.lstItemStates = new System.Windows.Forms.ListBox();
            this.chkFontUnderline = new System.Windows.Forms.CheckBox();
            this.chkFontStrikeout = new System.Windows.Forms.CheckBox();
            this.lblFontSampleCaption = new System.Windows.Forms.Label();
            this.lblFontSampleABC = new System.Windows.Forms.Label();
            this.tabEnclosures = new System.Windows.Forms.TabPage();
            this.sectionPanelEnclosurePodcasts = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.btnPodcastOptions = new System.Windows.Forms.Button();
            this.lblSectionPanelPodcastOptions = new System.Windows.Forms.Label();
            this.sectionPanelEnclosureGeneral = new RssBandit.WinGui.Controls.OptionSectionPanel();
            this.btnSelectEnclosureFolder2 = new System.Windows.Forms.Button();
            this.lblDownloadAttachmentsSmallerThanPostfix = new System.Windows.Forms.Label();
            this.numEnclosureCacheSize = new System.Windows.Forms.NumericUpDown();
            this.checkEnclosureSizeOnDiskLimited = new System.Windows.Forms.CheckBox();
            this.lblDownloadXAttachmentsPostfix = new System.Windows.Forms.Label();
            this.numOnlyDownloadLastXAttachments = new System.Windows.Forms.NumericUpDown();
            this.checkOnlyDownloadLastXAttachments = new System.Windows.Forms.CheckBox();
            this.checkDownloadCreateFolderPerFeed = new System.Windows.Forms.CheckBox();
            this.textEnclosureDirectory = new System.Windows.Forms.TextBox();
            this.labelEnclosureDirectory = new System.Windows.Forms.Label();
            this.checkDownloadEnclosures = new System.Windows.Forms.CheckBox();
            this.checkEnableEnclosureAlerts = new System.Windows.Forms.CheckBox();
            this.lblSectionPanelEnclosureGeneral = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnApply = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.openExeFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.securityHintProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabPrefs.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.optionSectionPanel1.SuspendLayout();
            this.sectionPanelGeneralStartup.SuspendLayout();
            this.sectionPanelGeneralBehavior.SuspendLayout();
            this.tabRemoteStorage.SuspendLayout();
            this.sectionPanelRemoteStorageFeedlist.SuspendLayout();
            this.tabNewsItems.SuspendLayout();
            this.sectionPanelDisplayGeneral.SuspendLayout();
            this.sectionPanelDisplayItemFormatting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numNewsItemsPerPage)).BeginInit();
            this.tabFeeds.SuspendLayout();
            this.sectionPanelFeedsCommentDefs.SuspendLayout();
            this.sectionPanelFeedsTimings.SuspendLayout();
            this.tabWebBrowser.SuspendLayout();
            this.sectionPanelWebBrowserSecurity.SuspendLayout();
            this.sectionPanelWebBrowserOnNewWindow.SuspendLayout();
            this.tabWebSearch.SuspendLayout();
            this.sectionPanelWebSearchEngines.SuspendLayout();
            this.tabNetConnection.SuspendLayout();
            this.sectionPanelNetworkConnectionProxy.SuspendLayout();
            this.tabFonts.SuspendLayout();
            this.sectionPanelFontsSubscriptions.SuspendLayout();
            this.tabEnclosures.SuspendLayout();
            this.sectionPanelEnclosurePodcasts.SuspendLayout();
            this.sectionPanelEnclosureGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEnclosureCacheSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOnlyDownloadLastXAttachments)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.securityHintProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.Click += new System.EventHandler(this.OnOKClick);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.CausesValidation = false;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            // 
            // tabPrefs
            // 
            resources.ApplyResources(this.tabPrefs, "tabPrefs");
            this.tabPrefs.Controls.Add(this.tabGeneral);
            this.tabPrefs.Controls.Add(this.tabRemoteStorage);
            this.tabPrefs.Controls.Add(this.tabNewsItems);
            this.tabPrefs.Controls.Add(this.tabFeeds);
            this.tabPrefs.Controls.Add(this.tabWebBrowser);
            this.tabPrefs.Controls.Add(this.tabWebSearch);
            this.tabPrefs.Controls.Add(this.tabNetConnection);
            this.tabPrefs.Controls.Add(this.tabFonts);
            this.tabPrefs.Controls.Add(this.tabEnclosures);
            this.tabPrefs.Multiline = true;
            this.tabPrefs.Name = "tabPrefs";
            this.tabPrefs.SelectedIndex = 0;
            this.tabPrefs.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.tabPrefs.Resize += new System.EventHandler(this.OnTabPrefs_Resize);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.optionSectionPanel1);
            this.tabGeneral.Controls.Add(this.sectionPanelGeneralStartup);
            this.tabGeneral.Controls.Add(this.sectionPanelGeneralBehavior);
            resources.ApplyResources(this.tabGeneral, "tabGeneral");
            this.tabGeneral.Name = "tabGeneral";
            // 
            // optionSectionPanel1
            // 
            this.optionSectionPanel1.Controls.Add(this.checkAllowAppEventSounds);
            this.optionSectionPanel1.Controls.Add(this.btnConfigureAppSounds);
            this.optionSectionPanel1.Image = ((System.Drawing.Image)(resources.GetObject("optionSectionPanel1.Image")));
            this.optionSectionPanel1.ImageLocation = new System.Drawing.Point(0, 20);
            resources.ApplyResources(this.optionSectionPanel1, "optionSectionPanel1");
            this.optionSectionPanel1.Name = "optionSectionPanel1";
            // 
            // checkAllowAppEventSounds
            // 
            resources.ApplyResources(this.checkAllowAppEventSounds, "checkAllowAppEventSounds");
            this.checkAllowAppEventSounds.Name = "checkAllowAppEventSounds";
            this.checkAllowAppEventSounds.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkAllowAppEventSounds.CheckedChanged += new System.EventHandler(this.OnEnableAppSoundsCheckedChanged);
            // 
            // btnConfigureAppSounds
            // 
            resources.ApplyResources(this.btnConfigureAppSounds, "btnConfigureAppSounds");
            this.btnConfigureAppSounds.Name = "btnConfigureAppSounds";
            this.toolTip1.SetToolTip(this.btnConfigureAppSounds, resources.GetString("btnConfigureAppSounds.ToolTip"));
            this.btnConfigureAppSounds.Click += new System.EventHandler(this.OnConfigureAppSoundsClick);
            // 
            // sectionPanelGeneralStartup
            // 
            this.sectionPanelGeneralStartup.Controls.Add(this.checkRunAtStartup);
            this.sectionPanelGeneralStartup.Controls.Add(this.checkRefreshFeedsOnStartup);
            this.sectionPanelGeneralStartup.ImageLocation = new System.Drawing.Point(0, 20);
            resources.ApplyResources(this.sectionPanelGeneralStartup, "sectionPanelGeneralStartup");
            this.sectionPanelGeneralStartup.Name = "sectionPanelGeneralStartup";
            // 
            // checkRunAtStartup
            // 
            resources.ApplyResources(this.checkRunAtStartup, "checkRunAtStartup");
            this.securityHintProvider.SetIconAlignment(this.checkRunAtStartup, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkRunAtStartup.IconAlignment"))));
            this.securityHintProvider.SetIconPadding(this.checkRunAtStartup, ((int)(resources.GetObject("checkRunAtStartup.IconPadding"))));
            this.checkRunAtStartup.Name = "checkRunAtStartup";
            this.checkRunAtStartup.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkRunAtStartup.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // checkRefreshFeedsOnStartup
            // 
            resources.ApplyResources(this.checkRefreshFeedsOnStartup, "checkRefreshFeedsOnStartup");
            this.checkRefreshFeedsOnStartup.Name = "checkRefreshFeedsOnStartup";
            this.checkRefreshFeedsOnStartup.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkRefreshFeedsOnStartup.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // sectionPanelGeneralBehavior
            // 
            this.sectionPanelGeneralBehavior.Controls.Add(this.label12);
            this.sectionPanelGeneralBehavior.Controls.Add(this.radioTrayActionNone);
            this.sectionPanelGeneralBehavior.Controls.Add(this.label1);
            this.sectionPanelGeneralBehavior.Controls.Add(this.radioTrayActionClose);
            this.sectionPanelGeneralBehavior.Controls.Add(this.radioTrayActionMinimize);
            this.sectionPanelGeneralBehavior.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelGeneralBehavior.Image")));
            this.sectionPanelGeneralBehavior.ImageLocation = new System.Drawing.Point(0, 20);
            resources.ApplyResources(this.sectionPanelGeneralBehavior, "sectionPanelGeneralBehavior");
            this.sectionPanelGeneralBehavior.Name = "sectionPanelGeneralBehavior";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label12.Name = "label12";
            // 
            // radioTrayActionNone
            // 
            resources.ApplyResources(this.radioTrayActionNone, "radioTrayActionNone");
            this.radioTrayActionNone.Name = "radioTrayActionNone";
            this.toolTip1.SetToolTip(this.radioTrayActionNone, resources.GetString("radioTrayActionNone.ToolTip"));
            this.radioTrayActionNone.Validated += new System.EventHandler(this.OnControlValidated);
            this.radioTrayActionNone.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.radioTrayActionNone.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Name = "label1";
            // 
            // radioTrayActionClose
            // 
            resources.ApplyResources(this.radioTrayActionClose, "radioTrayActionClose");
            this.radioTrayActionClose.Name = "radioTrayActionClose";
            this.toolTip1.SetToolTip(this.radioTrayActionClose, resources.GetString("radioTrayActionClose.ToolTip"));
            this.radioTrayActionClose.Validated += new System.EventHandler(this.OnControlValidated);
            this.radioTrayActionClose.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.radioTrayActionClose.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // radioTrayActionMinimize
            // 
            resources.ApplyResources(this.radioTrayActionMinimize, "radioTrayActionMinimize");
            this.radioTrayActionMinimize.Checked = true;
            this.radioTrayActionMinimize.Name = "radioTrayActionMinimize";
            this.radioTrayActionMinimize.TabStop = true;
            this.toolTip1.SetToolTip(this.radioTrayActionMinimize, resources.GetString("radioTrayActionMinimize.ToolTip"));
            this.radioTrayActionMinimize.Validated += new System.EventHandler(this.OnControlValidated);
            this.radioTrayActionMinimize.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.radioTrayActionMinimize.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // tabRemoteStorage
            // 
            this.tabRemoteStorage.Controls.Add(this.sectionPanelRemoteStorageFeedlist);
            resources.ApplyResources(this.tabRemoteStorage, "tabRemoteStorage");
            this.tabRemoteStorage.Name = "tabRemoteStorage";
            // 
            // sectionPanelRemoteStorageFeedlist
            // 
            resources.ApplyResources(this.sectionPanelRemoteStorageFeedlist, "sectionPanelRemoteStorageFeedlist");
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.textRemoteStorageLocation);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.textRemoteStoragePassword);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.textRemoteStorageUserName);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.comboRemoteStorageProtocol);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.checkUseRemoteStorage);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.label10);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.labelRemoteStorageLocation);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.labelRemoteStoragePassword);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.labelRemoteStorageUserName);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.labelRemoteStorageProtocol);
            this.sectionPanelRemoteStorageFeedlist.Controls.Add(this.labelExperimental);
            this.sectionPanelRemoteStorageFeedlist.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Image")));
            this.sectionPanelRemoteStorageFeedlist.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelRemoteStorageFeedlist.Name = "sectionPanelRemoteStorageFeedlist";
            // 
            // textRemoteStorageLocation
            // 
            resources.ApplyResources(this.textRemoteStorageLocation, "textRemoteStorageLocation");
            this.errorProvider1.SetIconAlignment(this.textRemoteStorageLocation, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textRemoteStorageLocation.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.textRemoteStorageLocation, ((int)(resources.GetObject("textRemoteStorageLocation.IconPadding"))));
            this.textRemoteStorageLocation.Name = "textRemoteStorageLocation";
            this.textRemoteStorageLocation.Validated += new System.EventHandler(this.OnControlValidated);
            this.textRemoteStorageLocation.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // textRemoteStoragePassword
            // 
            resources.ApplyResources(this.textRemoteStoragePassword, "textRemoteStoragePassword");
            this.errorProvider1.SetIconAlignment(this.textRemoteStoragePassword, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textRemoteStoragePassword.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.textRemoteStoragePassword, ((int)(resources.GetObject("textRemoteStoragePassword.IconPadding"))));
            this.textRemoteStoragePassword.Name = "textRemoteStoragePassword";
            this.textRemoteStoragePassword.Validated += new System.EventHandler(this.OnControlValidated);
            this.textRemoteStoragePassword.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // textRemoteStorageUserName
            // 
            resources.ApplyResources(this.textRemoteStorageUserName, "textRemoteStorageUserName");
            this.errorProvider1.SetIconAlignment(this.textRemoteStorageUserName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textRemoteStorageUserName.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.textRemoteStorageUserName, ((int)(resources.GetObject("textRemoteStorageUserName.IconPadding"))));
            this.textRemoteStorageUserName.Name = "textRemoteStorageUserName";
            this.toolTip1.SetToolTip(this.textRemoteStorageUserName, resources.GetString("textRemoteStorageUserName.ToolTip"));
            this.textRemoteStorageUserName.Validated += new System.EventHandler(this.OnControlValidated);
            this.textRemoteStorageUserName.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // comboRemoteStorageProtocol
            // 
            resources.ApplyResources(this.comboRemoteStorageProtocol, "comboRemoteStorageProtocol");
            this.comboRemoteStorageProtocol.CausesValidation = false;
            this.comboRemoteStorageProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.errorProvider1.SetIconAlignment(this.comboRemoteStorageProtocol, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboRemoteStorageProtocol.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.comboRemoteStorageProtocol, ((int)(resources.GetObject("comboRemoteStorageProtocol.IconPadding"))));
            this.comboRemoteStorageProtocol.Items.AddRange(new object[] {
            resources.GetString("comboRemoteStorageProtocol.Items"),
            resources.GetString("comboRemoteStorageProtocol.Items1"),
            resources.GetString("comboRemoteStorageProtocol.Items2"),
            resources.GetString("comboRemoteStorageProtocol.Items3")});
            this.comboRemoteStorageProtocol.Name = "comboRemoteStorageProtocol";
            this.comboRemoteStorageProtocol.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.comboRemoteStorageProtocol.SelectedIndexChanged += new System.EventHandler(this.comboRemoteStorageProtocol_SelectedIndexChanged);
            this.comboRemoteStorageProtocol.Validated += new System.EventHandler(this.OnControlValidated);
            // 
            // checkUseRemoteStorage
            // 
            resources.ApplyResources(this.checkUseRemoteStorage, "checkUseRemoteStorage");
            this.checkUseRemoteStorage.CausesValidation = false;
            this.checkUseRemoteStorage.Name = "checkUseRemoteStorage";
            this.checkUseRemoteStorage.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkUseRemoteStorage.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkUseRemoteStorage.CheckedChanged += new System.EventHandler(this.checkUseRemoteStorage_CheckedChanged);
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label10.Name = "label10";
            // 
            // labelRemoteStorageLocation
            // 
            this.labelRemoteStorageLocation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelRemoteStorageLocation, "labelRemoteStorageLocation");
            this.labelRemoteStorageLocation.Name = "labelRemoteStorageLocation";
            // 
            // labelRemoteStoragePassword
            // 
            this.labelRemoteStoragePassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelRemoteStoragePassword, "labelRemoteStoragePassword");
            this.labelRemoteStoragePassword.Name = "labelRemoteStoragePassword";
            this.toolTip1.SetToolTip(this.labelRemoteStoragePassword, resources.GetString("labelRemoteStoragePassword.ToolTip"));
            // 
            // labelRemoteStorageUserName
            // 
            this.labelRemoteStorageUserName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelRemoteStorageUserName, "labelRemoteStorageUserName");
            this.labelRemoteStorageUserName.Name = "labelRemoteStorageUserName";
            this.toolTip1.SetToolTip(this.labelRemoteStorageUserName, resources.GetString("labelRemoteStorageUserName.ToolTip"));
            // 
            // labelRemoteStorageProtocol
            // 
            this.labelRemoteStorageProtocol.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelRemoteStorageProtocol, "labelRemoteStorageProtocol");
            this.labelRemoteStorageProtocol.Name = "labelRemoteStorageProtocol";
            // 
            // labelExperimental
            // 
            resources.ApplyResources(this.labelExperimental, "labelExperimental");
            this.labelExperimental.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelExperimental.Name = "labelExperimental";
            // 
            // tabNewsItems
            // 
            this.tabNewsItems.Controls.Add(this.sectionPanelDisplayGeneral);
            this.tabNewsItems.Controls.Add(this.sectionPanelDisplayItemFormatting);
            resources.ApplyResources(this.tabNewsItems, "tabNewsItems");
            this.tabNewsItems.Name = "tabNewsItems";
            // 
            // sectionPanelDisplayGeneral
            // 
            resources.ApplyResources(this.sectionPanelDisplayGeneral, "sectionPanelDisplayGeneral");
            this.sectionPanelDisplayGeneral.Controls.Add(this.checkUseFavicons);
            this.sectionPanelDisplayGeneral.Controls.Add(this.checkMarkItemsReadOnExit);
            this.sectionPanelDisplayGeneral.Controls.Add(this.chkNewsItemOpenLinkInDetailWindow);
            this.sectionPanelDisplayGeneral.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelDisplayGeneral.Name = "sectionPanelDisplayGeneral";
            // 
            // checkUseFavicons
            // 
            resources.ApplyResources(this.checkUseFavicons, "checkUseFavicons");
            this.checkUseFavicons.Checked = true;
            this.checkUseFavicons.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkUseFavicons.Name = "checkUseFavicons";
            this.toolTip1.SetToolTip(this.checkUseFavicons, resources.GetString("checkUseFavicons.ToolTip"));
            this.checkUseFavicons.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkUseFavicons.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkUseFavicons.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // checkMarkItemsReadOnExit
            // 
            resources.ApplyResources(this.checkMarkItemsReadOnExit, "checkMarkItemsReadOnExit");
            this.checkMarkItemsReadOnExit.Checked = true;
            this.checkMarkItemsReadOnExit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkMarkItemsReadOnExit.Name = "checkMarkItemsReadOnExit";
            this.checkMarkItemsReadOnExit.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // chkNewsItemOpenLinkInDetailWindow
            // 
            resources.ApplyResources(this.chkNewsItemOpenLinkInDetailWindow, "chkNewsItemOpenLinkInDetailWindow");
            this.chkNewsItemOpenLinkInDetailWindow.Checked = true;
            this.chkNewsItemOpenLinkInDetailWindow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNewsItemOpenLinkInDetailWindow.Name = "chkNewsItemOpenLinkInDetailWindow";
            this.chkNewsItemOpenLinkInDetailWindow.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // sectionPanelDisplayItemFormatting
            // 
            resources.ApplyResources(this.sectionPanelDisplayItemFormatting, "sectionPanelDisplayItemFormatting");
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.checkMarkItemsAsReadWhenViewed);
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.label8);
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.numNewsItemsPerPage);
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.checkLimitNewsItemsPerPage);
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.comboFormatters);
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.label2);
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.checkCustomFormatter);
            this.sectionPanelDisplayItemFormatting.Controls.Add(this.labelFormatters);
            this.sectionPanelDisplayItemFormatting.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelDisplayItemFormatting.Image")));
            this.sectionPanelDisplayItemFormatting.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelDisplayItemFormatting.Name = "sectionPanelDisplayItemFormatting";
            // 
            // checkMarkItemsAsReadWhenViewed
            // 
            resources.ApplyResources(this.checkMarkItemsAsReadWhenViewed, "checkMarkItemsAsReadWhenViewed");
            this.checkMarkItemsAsReadWhenViewed.Name = "checkMarkItemsAsReadWhenViewed";
            this.toolTip1.SetToolTip(this.checkMarkItemsAsReadWhenViewed, resources.GetString("checkMarkItemsAsReadWhenViewed.ToolTip"));
            this.checkMarkItemsAsReadWhenViewed.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkMarkItemsAsReadWhenViewed.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkMarkItemsAsReadWhenViewed.CheckedChanged += new System.EventHandler(this.checkMarkItemsAsReadWhenViewed_CheckedChanged);
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label8.Name = "label8";
            // 
            // numNewsItemsPerPage
            // 
            resources.ApplyResources(this.numNewsItemsPerPage, "numNewsItemsPerPage");
            this.numNewsItemsPerPage.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numNewsItemsPerPage.Name = "numNewsItemsPerPage";
            this.toolTip1.SetToolTip(this.numNewsItemsPerPage, resources.GetString("numNewsItemsPerPage.ToolTip"));
            this.numNewsItemsPerPage.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numNewsItemsPerPage.Validated += new System.EventHandler(this.OnControlValidated);
            this.numNewsItemsPerPage.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // checkLimitNewsItemsPerPage
            // 
            resources.ApplyResources(this.checkLimitNewsItemsPerPage, "checkLimitNewsItemsPerPage");
            this.checkLimitNewsItemsPerPage.BackColor = System.Drawing.SystemColors.Control;
            this.checkLimitNewsItemsPerPage.Name = "checkLimitNewsItemsPerPage";
            this.toolTip1.SetToolTip(this.checkLimitNewsItemsPerPage, resources.GetString("checkLimitNewsItemsPerPage.ToolTip"));
            this.checkLimitNewsItemsPerPage.UseVisualStyleBackColor = false;
            this.checkLimitNewsItemsPerPage.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkLimitNewsItemsPerPage.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkLimitNewsItemsPerPage.CheckedChanged += new System.EventHandler(this.checkLimitNewsItemsPerPage_CheckedChanged);
            // 
            // comboFormatters
            // 
            resources.ApplyResources(this.comboFormatters, "comboFormatters");
            this.comboFormatters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFormatters.Name = "comboFormatters";
            this.comboFormatters.Sorted = true;
            this.toolTip1.SetToolTip(this.comboFormatters, resources.GetString("comboFormatters.ToolTip"));
            this.comboFormatters.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.comboFormatters.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
            this.comboFormatters.Validated += new System.EventHandler(this.OnControlValidated);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Name = "label2";
            // 
            // checkCustomFormatter
            // 
            resources.ApplyResources(this.checkCustomFormatter, "checkCustomFormatter");
            this.checkCustomFormatter.Name = "checkCustomFormatter";
            this.toolTip1.SetToolTip(this.checkCustomFormatter, resources.GetString("checkCustomFormatter.ToolTip"));
            this.checkCustomFormatter.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkCustomFormatter.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkCustomFormatter.CheckedChanged += new System.EventHandler(this.checkCustomFormatter_CheckedChanged);
            // 
            // labelFormatters
            // 
            this.labelFormatters.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelFormatters, "labelFormatters");
            this.labelFormatters.Name = "labelFormatters";
            // 
            // tabFeeds
            // 
            this.tabFeeds.Controls.Add(this.sectionPanelFeedsCommentDefs);
            this.tabFeeds.Controls.Add(this.sectionPanelFeedsTimings);
            resources.ApplyResources(this.tabFeeds, "tabFeeds");
            this.tabFeeds.Name = "tabFeeds";
            // 
            // sectionPanelFeedsCommentDefs
            // 
            resources.ApplyResources(this.sectionPanelFeedsCommentDefs, "sectionPanelFeedsCommentDefs");
            this.sectionPanelFeedsCommentDefs.Controls.Add(this.cboUserIdentityForComments);
            this.sectionPanelFeedsCommentDefs.Controls.Add(this.btnManageIdentities);
            this.sectionPanelFeedsCommentDefs.Controls.Add(this.linkCommentAPI);
            this.sectionPanelFeedsCommentDefs.Controls.Add(this.lblIdentityDropdownCaption);
            this.sectionPanelFeedsCommentDefs.Controls.Add(this.label6);
            this.sectionPanelFeedsCommentDefs.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelFeedsCommentDefs.Image")));
            this.sectionPanelFeedsCommentDefs.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelFeedsCommentDefs.Name = "sectionPanelFeedsCommentDefs";
            // 
            // cboUserIdentityForComments
            // 
            resources.ApplyResources(this.cboUserIdentityForComments, "cboUserIdentityForComments");
            this.cboUserIdentityForComments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.errorProvider1.SetIconAlignment(this.cboUserIdentityForComments, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cboUserIdentityForComments.IconAlignment"))));
            this.cboUserIdentityForComments.Name = "cboUserIdentityForComments";
            this.cboUserIdentityForComments.Sorted = true;
            this.cboUserIdentityForComments.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.cboUserIdentityForComments.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
            this.cboUserIdentityForComments.Validated += new System.EventHandler(this.OnControlValidated);
            // 
            // btnManageIdentities
            // 
            resources.ApplyResources(this.btnManageIdentities, "btnManageIdentities");
            this.btnManageIdentities.Name = "btnManageIdentities";
            this.btnManageIdentities.Click += new System.EventHandler(this.btnManageIdentities_Click);
            // 
            // linkCommentAPI
            // 
            resources.ApplyResources(this.linkCommentAPI, "linkCommentAPI");
            this.linkCommentAPI.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkCommentAPI.Name = "linkCommentAPI";
            this.linkCommentAPI.TabStop = true;
            this.toolTip1.SetToolTip(this.linkCommentAPI, resources.GetString("linkCommentAPI.ToolTip"));
            // 
            // lblIdentityDropdownCaption
            // 
            resources.ApplyResources(this.lblIdentityDropdownCaption, "lblIdentityDropdownCaption");
            this.lblIdentityDropdownCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblIdentityDropdownCaption.Name = "lblIdentityDropdownCaption";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label6.Name = "label6";
            // 
            // sectionPanelFeedsTimings
            // 
            resources.ApplyResources(this.sectionPanelFeedsTimings, "sectionPanelFeedsTimings");
            this.sectionPanelFeedsTimings.Controls.Add(this.checkClearFeedCategoryItemAgeSettings);
            this.sectionPanelFeedsTimings.Controls.Add(this.checkResetIndividualRefreshRates);
            this.sectionPanelFeedsTimings.Controls.Add(this.label15);
            this.sectionPanelFeedsTimings.Controls.Add(this.linkLabel1);
            this.sectionPanelFeedsTimings.Controls.Add(this.comboRefreshRate);
            this.sectionPanelFeedsTimings.Controls.Add(this.label5);
            this.sectionPanelFeedsTimings.Controls.Add(this.label4);
            this.sectionPanelFeedsTimings.Controls.Add(this.comboMaxItemAge);
            this.sectionPanelFeedsTimings.Controls.Add(this.label3);
            this.sectionPanelFeedsTimings.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelFeedsTimings.Image")));
            this.sectionPanelFeedsTimings.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelFeedsTimings.Name = "sectionPanelFeedsTimings";
            // 
            // checkClearFeedCategoryItemAgeSettings
            // 
            resources.ApplyResources(this.checkClearFeedCategoryItemAgeSettings, "checkClearFeedCategoryItemAgeSettings");
            this.checkClearFeedCategoryItemAgeSettings.Name = "checkClearFeedCategoryItemAgeSettings";
            this.checkClearFeedCategoryItemAgeSettings.UseVisualStyleBackColor = true;
            this.checkClearFeedCategoryItemAgeSettings.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // checkResetIndividualRefreshRates
            // 
            resources.ApplyResources(this.checkResetIndividualRefreshRates, "checkResetIndividualRefreshRates");
            this.checkResetIndividualRefreshRates.Name = "checkResetIndividualRefreshRates";
            this.checkResetIndividualRefreshRates.UseVisualStyleBackColor = true;
            this.checkResetIndividualRefreshRates.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // label15
            // 
            this.label15.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.toolTip1.SetToolTip(this.linkLabel1, resources.GetString("linkLabel1.ToolTip"));
            // 
            // comboRefreshRate
            // 
            resources.ApplyResources(this.comboRefreshRate, "comboRefreshRate");
            this.errorProvider1.SetIconAlignment(this.comboRefreshRate, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboRefreshRate.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.comboRefreshRate, ((int)(resources.GetObject("comboRefreshRate.IconPadding"))));
            this.comboRefreshRate.Name = "comboRefreshRate";
            this.toolTip1.SetToolTip(this.comboRefreshRate, resources.GetString("comboRefreshRate.ToolTip"));
            this.comboRefreshRate.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.comboRefreshRate.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
            this.comboRefreshRate.Validated += new System.EventHandler(this.OnControlValidated);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label5.Name = "label5";
            // 
            // label4
            // 
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // comboMaxItemAge
            // 
            resources.ApplyResources(this.comboMaxItemAge, "comboMaxItemAge");
            this.comboMaxItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.errorProvider1.SetIconAlignment(this.comboMaxItemAge, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboMaxItemAge.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.comboMaxItemAge, ((int)(resources.GetObject("comboMaxItemAge.IconPadding"))));
            this.comboMaxItemAge.Name = "comboMaxItemAge";
            this.toolTip1.SetToolTip(this.comboMaxItemAge, resources.GetString("comboMaxItemAge.ToolTip"));
            this.comboMaxItemAge.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Name = "label3";
            // 
            // tabWebBrowser
            // 
            this.tabWebBrowser.Controls.Add(this.sectionPanelWebBrowserSecurity);
            this.tabWebBrowser.Controls.Add(this.sectionPanelWebBrowserOnNewWindow);
            resources.ApplyResources(this.tabWebBrowser, "tabWebBrowser");
            this.tabWebBrowser.Name = "tabWebBrowser";
            // 
            // sectionPanelWebBrowserSecurity
            // 
            resources.ApplyResources(this.sectionPanelWebBrowserSecurity, "sectionPanelWebBrowserSecurity");
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserImagesAllowed);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserBGSoundAllowed);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserVideoAllowed);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserActiveXAllowed);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.lblCheckImage);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.labelCheckToAllow);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserJavascriptAllowed);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserJavaAllowed);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.label19);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.label7);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBox1);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBox2);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBox3);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBox4);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBox5);
		    this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBox6);
            this.sectionPanelWebBrowserSecurity.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebBrowserSecurity.Image")));
            this.sectionPanelWebBrowserSecurity.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelWebBrowserSecurity.Name = "sectionPanelWebBrowserSecurity";
            // 
            // checkBrowserActiveXAllowed
            // 
            resources.ApplyResources(this.checkBrowserActiveXAllowed, "checkBrowserActiveXAllowed");
            this.checkBrowserActiveXAllowed.Name = "checkBrowserActiveXAllowed";
            this.checkBrowserActiveXAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // checkBrowserBGSoundAllowed
            // 
            resources.ApplyResources(this.checkBrowserBGSoundAllowed, "checkBrowserBGSoundAllowed");
            this.checkBrowserBGSoundAllowed.Name = "checkBrowserBGSoundAllowed";
            this.checkBrowserBGSoundAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // lblCheckImage
            // 
            resources.ApplyResources(this.lblCheckImage, "lblCheckImage");
            this.lblCheckImage.BackColor = System.Drawing.Color.Transparent;
            this.lblCheckImage.Name = "lblCheckImage";
            // 
            // labelCheckToAllow
            // 
            resources.ApplyResources(this.labelCheckToAllow, "labelCheckToAllow");
            this.labelCheckToAllow.BackColor = System.Drawing.Color.Transparent;
            this.labelCheckToAllow.Name = "labelCheckToAllow";
            // 
            // checkBrowserJavascriptAllowed
            // 
            resources.ApplyResources(this.checkBrowserJavascriptAllowed, "checkBrowserJavascriptAllowed");
            this.checkBrowserJavascriptAllowed.Name = "checkBrowserJavascriptAllowed";
            this.checkBrowserJavascriptAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // checkBrowserJavaAllowed
            // 
            resources.ApplyResources(this.checkBrowserJavaAllowed, "checkBrowserJavaAllowed");
            this.checkBrowserJavaAllowed.Name = "checkBrowserJavaAllowed";
            this.checkBrowserJavaAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // checkBrowserVideoAllowed
            // 
            resources.ApplyResources(this.checkBrowserVideoAllowed, "checkBrowserVideoAllowed");
            this.checkBrowserVideoAllowed.Name = "checkBrowserVideoAllowed";
            this.checkBrowserVideoAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // checkBrowserImagesAllowed
            // 
            resources.ApplyResources(this.checkBrowserImagesAllowed, "checkBrowserImagesAllowed");
            this.checkBrowserImagesAllowed.Checked = true;
            this.checkBrowserImagesAllowed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBrowserImagesAllowed.Name = "checkBrowserImagesAllowed";
            this.checkBrowserImagesAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // label19
            // 
            resources.ApplyResources(this.label19, "label19");
            this.label19.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label19.Name = "label19";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.errorProvider1.SetIconAlignment(this.label7, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label7.IconAlignment"))));
            this.label7.Name = "label7";
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.errorProvider1.SetIconAlignment(this.checkBox1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBox1.IconAlignment"))));
            this.checkBox1.Name = "checkBox1";
            // 
            // checkBox2
            // 
            resources.ApplyResources(this.checkBox2, "checkBox2");
            this.errorProvider1.SetIconAlignment(this.checkBox2, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBox2.IconAlignment"))));
            this.checkBox2.Name = "checkBox2";
            // 
            // checkBox3
            // 
            resources.ApplyResources(this.checkBox3, "checkBox3");
            this.errorProvider1.SetIconAlignment(this.checkBox3, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBox3.IconAlignment"))));
            this.checkBox3.Name = "checkBox3";
            // 
            // checkBox4
            // 
            resources.ApplyResources(this.checkBox4, "checkBox4");
            this.errorProvider1.SetIconAlignment(this.checkBox4, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBox4.IconAlignment"))));
            this.checkBox4.Name = "checkBox4";
            // 
            // checkBox5
            // 
            resources.ApplyResources(this.checkBox5, "checkBox5");
            this.errorProvider1.SetIconAlignment(this.checkBox5, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBox5.IconAlignment"))));
            this.checkBox5.Name = "checkBox5";
            // 
            // checkBox6
            // 
            resources.ApplyResources(this.checkBox6, "checkBox6");
            this.checkBox6.Checked = true;
            this.checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.errorProvider1.SetIconAlignment(this.checkBox6, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBox6.IconAlignment"))));
            this.checkBox6.Name = "checkBox6";
            // 
            // sectionPanelWebBrowserOnNewWindow
            // 
            resources.ApplyResources(this.sectionPanelWebBrowserOnNewWindow, "sectionPanelWebBrowserOnNewWindow");
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.checkOpenTabsInBackground);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.optNewWindowCustomExec);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.optNewWindowDefaultWebBrowser);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.optNewWindowOnTab);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.checkReuseFirstBrowserTab);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.btnSelectExecutable);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.txtBrowserStartExecutable);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.labelBrowserStartExecutable);
            this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.label16);
            this.sectionPanelWebBrowserOnNewWindow.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Image")));
            this.sectionPanelWebBrowserOnNewWindow.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelWebBrowserOnNewWindow.Name = "sectionPanelWebBrowserOnNewWindow";
            // 
            // checkOpenTabsInBackground
            // 
            resources.ApplyResources(this.checkOpenTabsInBackground, "checkOpenTabsInBackground");
            this.checkOpenTabsInBackground.Name = "checkOpenTabsInBackground";
            this.checkOpenTabsInBackground.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // optNewWindowCustomExec
            // 
            resources.ApplyResources(this.optNewWindowCustomExec, "optNewWindowCustomExec");
            this.optNewWindowCustomExec.Name = "optNewWindowCustomExec";
            this.optNewWindowCustomExec.Validated += new System.EventHandler(this.OnControlValidated);
            this.optNewWindowCustomExec.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.optNewWindowCustomExec.CheckedChanged += new System.EventHandler(this.optNewWindowCustomExec_CheckedChanged);
            // 
            // optNewWindowDefaultWebBrowser
            // 
            resources.ApplyResources(this.optNewWindowDefaultWebBrowser, "optNewWindowDefaultWebBrowser");
            this.optNewWindowDefaultWebBrowser.CausesValidation = false;
            this.optNewWindowDefaultWebBrowser.Name = "optNewWindowDefaultWebBrowser";
            this.optNewWindowDefaultWebBrowser.Validated += new System.EventHandler(this.OnControlValidated);
            this.optNewWindowDefaultWebBrowser.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.optNewWindowDefaultWebBrowser.CheckedChanged += new System.EventHandler(this.optOnOpenNewWindowChecked);
            // 
            // optNewWindowOnTab
            // 
            resources.ApplyResources(this.optNewWindowOnTab, "optNewWindowOnTab");
            this.optNewWindowOnTab.CausesValidation = false;
            this.optNewWindowOnTab.Checked = true;
            this.optNewWindowOnTab.Name = "optNewWindowOnTab";
            this.optNewWindowOnTab.TabStop = true;
            this.optNewWindowOnTab.Validated += new System.EventHandler(this.OnControlValidated);
            this.optNewWindowOnTab.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.optNewWindowOnTab.CheckedChanged += new System.EventHandler(this.optOnOpenNewWindowChecked);
            // 
            // checkReuseFirstBrowserTab
            // 
            resources.ApplyResources(this.checkReuseFirstBrowserTab, "checkReuseFirstBrowserTab");
            this.checkReuseFirstBrowserTab.Name = "checkReuseFirstBrowserTab";
            this.checkReuseFirstBrowserTab.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkReuseFirstBrowserTab.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkReuseFirstBrowserTab.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // btnSelectExecutable
            // 
            resources.ApplyResources(this.btnSelectExecutable, "btnSelectExecutable");
            this.btnSelectExecutable.CausesValidation = false;
            this.btnSelectExecutable.Name = "btnSelectExecutable";
            this.btnSelectExecutable.Click += new System.EventHandler(this.btnSelectExecutable_Click);
            // 
            // txtBrowserStartExecutable
            // 
            resources.ApplyResources(this.txtBrowserStartExecutable, "txtBrowserStartExecutable");
            this.errorProvider1.SetIconAlignment(this.txtBrowserStartExecutable, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtBrowserStartExecutable.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.txtBrowserStartExecutable, ((int)(resources.GetObject("txtBrowserStartExecutable.IconPadding"))));
            this.txtBrowserStartExecutable.Name = "txtBrowserStartExecutable";
            this.txtBrowserStartExecutable.Validated += new System.EventHandler(this.OnControlValidated);
            this.txtBrowserStartExecutable.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // labelBrowserStartExecutable
            // 
            this.labelBrowserStartExecutable.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelBrowserStartExecutable, "labelBrowserStartExecutable");
            this.labelBrowserStartExecutable.Name = "labelBrowserStartExecutable";
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label16.Name = "label16";
            // 
            // tabWebSearch
            // 
            this.tabWebSearch.Controls.Add(this.sectionPanelWebSearchEngines);
            resources.ApplyResources(this.tabWebSearch, "tabWebSearch");
            this.tabWebSearch.Name = "tabWebSearch";
            // 
            // sectionPanelWebSearchEngines
            // 
            resources.ApplyResources(this.sectionPanelWebSearchEngines, "sectionPanelWebSearchEngines");
            this.sectionPanelWebSearchEngines.Controls.Add(this.btnSERemove);
            this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEAdd);
            this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEProperties);
            this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEMoveDown);
            this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEMoveUp);
            this.sectionPanelWebSearchEngines.Controls.Add(this.listSearchEngines);
            this.sectionPanelWebSearchEngines.Controls.Add(this.label17);
            this.sectionPanelWebSearchEngines.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebSearchEngines.Image")));
            this.sectionPanelWebSearchEngines.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelWebSearchEngines.Name = "sectionPanelWebSearchEngines";
            // 
            // btnSERemove
            // 
            resources.ApplyResources(this.btnSERemove, "btnSERemove");
            this.btnSERemove.Name = "btnSERemove";
            this.toolTip1.SetToolTip(this.btnSERemove, resources.GetString("btnSERemove.ToolTip"));
            this.btnSERemove.Click += new System.EventHandler(this.btnSERemove_Click);
            // 
            // btnSEAdd
            // 
            resources.ApplyResources(this.btnSEAdd, "btnSEAdd");
            this.btnSEAdd.Name = "btnSEAdd";
            this.toolTip1.SetToolTip(this.btnSEAdd, resources.GetString("btnSEAdd.ToolTip"));
            this.btnSEAdd.Click += new System.EventHandler(this.btnSEAdd_Click);
            // 
            // btnSEProperties
            // 
            resources.ApplyResources(this.btnSEProperties, "btnSEProperties");
            this.btnSEProperties.Name = "btnSEProperties";
            this.toolTip1.SetToolTip(this.btnSEProperties, resources.GetString("btnSEProperties.ToolTip"));
            this.btnSEProperties.Click += new System.EventHandler(this.btnSEProperties_Click);
            // 
            // btnSEMoveDown
            // 
            resources.ApplyResources(this.btnSEMoveDown, "btnSEMoveDown");
            this.btnSEMoveDown.Name = "btnSEMoveDown";
            this.toolTip1.SetToolTip(this.btnSEMoveDown, resources.GetString("btnSEMoveDown.ToolTip"));
            this.btnSEMoveDown.Click += new System.EventHandler(this.btnSEMoveDown_Click);
            // 
            // btnSEMoveUp
            // 
            resources.ApplyResources(this.btnSEMoveUp, "btnSEMoveUp");
            this.btnSEMoveUp.Name = "btnSEMoveUp";
            this.toolTip1.SetToolTip(this.btnSEMoveUp, resources.GetString("btnSEMoveUp.ToolTip"));
            this.btnSEMoveUp.Click += new System.EventHandler(this.btnSEMoveUp_Click);
            // 
            // listSearchEngines
            // 
            this.listSearchEngines.Activation = System.Windows.Forms.ItemActivation.OneClick;
            resources.ApplyResources(this.listSearchEngines, "listSearchEngines");
            this.listSearchEngines.AutoArrange = false;
            this.listSearchEngines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listSearchEngines.CheckBoxes = true;
            this.listSearchEngines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader0,
            this.columnHeader1,
            this.columnHeader2});
            this.listSearchEngines.FullRowSelect = true;
            this.listSearchEngines.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listSearchEngines.HideSelection = false;
            this.listSearchEngines.MultiSelect = false;
            this.listSearchEngines.Name = "listSearchEngines";
            this.listSearchEngines.SmallImageList = this.imagesSearchEngines;
            this.listSearchEngines.UseCompatibleStateImageBehavior = false;
            this.listSearchEngines.View = System.Windows.Forms.View.Details;
            this.listSearchEngines.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.listSearchEngines.ItemActivate += new System.EventHandler(this.OnSearchEngineItemActivate);
            this.listSearchEngines.Validated += new System.EventHandler(this.OnControlValidated);
            this.listSearchEngines.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnSearchEngineItemChecked);
            this.listSearchEngines.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnSearchEnginesListMouseUp);
            // 
            // columnHeader0
            // 
            resources.ApplyResources(this.columnHeader0, "columnHeader0");
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // imagesSearchEngines
            // 
            this.imagesSearchEngines.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            resources.ApplyResources(this.imagesSearchEngines, "imagesSearchEngines");
            this.imagesSearchEngines.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label17.Name = "label17";
            // 
            // tabNetConnection
            // 
            this.tabNetConnection.Controls.Add(this.sectionPanelNetworkConnectionProxy);
            resources.ApplyResources(this.tabNetConnection, "tabNetConnection");
            this.tabNetConnection.Name = "tabNetConnection";
            // 
            // sectionPanelNetworkConnectionProxy
            // 
            resources.ApplyResources(this.sectionPanelNetworkConnectionProxy, "sectionPanelNetworkConnectionProxy");
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.textProxyBypassList);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.checkNoProxy);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.textProxyCredentialPassword);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.labelProxyCredentialPassword);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.labelProxyCredentialUserName);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.textProxyCredentialUser);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.checkProxyAuth);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.labelProxyBypassListHint);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.labelProxyBypassList);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.checkUseProxy);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.checkUseIEProxySettings);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.labelProxyServerSummery);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.textProxyPort);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.labelProxyPort);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.labelProxyAddress);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.textProxyAddress);
            this.sectionPanelNetworkConnectionProxy.Controls.Add(this.checkProxyBypassLocal);
            this.sectionPanelNetworkConnectionProxy.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelNetworkConnectionProxy.Image")));
            this.sectionPanelNetworkConnectionProxy.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelNetworkConnectionProxy.Name = "sectionPanelNetworkConnectionProxy";
            // 
            // textProxyBypassList
            // 
            resources.ApplyResources(this.textProxyBypassList, "textProxyBypassList");
            this.textProxyBypassList.Name = "textProxyBypassList";
            this.toolTip1.SetToolTip(this.textProxyBypassList, resources.GetString("textProxyBypassList.ToolTip"));
            // 
            // checkNoProxy
            // 
            resources.ApplyResources(this.checkNoProxy, "checkNoProxy");
            this.checkNoProxy.CausesValidation = false;
            this.checkNoProxy.Checked = true;
            this.checkNoProxy.Name = "checkNoProxy";
            this.checkNoProxy.TabStop = true;
            this.checkNoProxy.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkNoProxy.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkNoProxy.CheckedChanged += new System.EventHandler(this.checkUseProxy_CheckedChanged);
            // 
            // textProxyCredentialPassword
            // 
            resources.ApplyResources(this.textProxyCredentialPassword, "textProxyCredentialPassword");
            this.errorProvider1.SetIconAlignment(this.textProxyCredentialPassword, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyCredentialPassword.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.textProxyCredentialPassword, ((int)(resources.GetObject("textProxyCredentialPassword.IconPadding"))));
            this.textProxyCredentialPassword.Name = "textProxyCredentialPassword";
            this.textProxyCredentialPassword.Validated += new System.EventHandler(this.OnControlValidated);
            this.textProxyCredentialPassword.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // labelProxyCredentialPassword
            // 
            this.labelProxyCredentialPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelProxyCredentialPassword, "labelProxyCredentialPassword");
            this.labelProxyCredentialPassword.Name = "labelProxyCredentialPassword";
            this.toolTip1.SetToolTip(this.labelProxyCredentialPassword, resources.GetString("labelProxyCredentialPassword.ToolTip"));
            // 
            // labelProxyCredentialUserName
            // 
            this.labelProxyCredentialUserName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelProxyCredentialUserName, "labelProxyCredentialUserName");
            this.labelProxyCredentialUserName.Name = "labelProxyCredentialUserName";
            this.toolTip1.SetToolTip(this.labelProxyCredentialUserName, resources.GetString("labelProxyCredentialUserName.ToolTip"));
            // 
            // textProxyCredentialUser
            // 
            resources.ApplyResources(this.textProxyCredentialUser, "textProxyCredentialUser");
            this.errorProvider1.SetIconAlignment(this.textProxyCredentialUser, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyCredentialUser.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.textProxyCredentialUser, ((int)(resources.GetObject("textProxyCredentialUser.IconPadding"))));
            this.textProxyCredentialUser.Name = "textProxyCredentialUser";
            this.toolTip1.SetToolTip(this.textProxyCredentialUser, resources.GetString("textProxyCredentialUser.ToolTip"));
            this.textProxyCredentialUser.Validated += new System.EventHandler(this.OnControlValidated);
            this.textProxyCredentialUser.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // checkProxyAuth
            // 
            resources.ApplyResources(this.checkProxyAuth, "checkProxyAuth");
            this.checkProxyAuth.CausesValidation = false;
            this.checkProxyAuth.Name = "checkProxyAuth";
            this.checkProxyAuth.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkProxyAuth.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkProxyAuth.CheckedChanged += new System.EventHandler(this.checkProxyAuth_CheckedChanged);
            // 
            // labelProxyBypassListHint
            // 
            this.labelProxyBypassListHint.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelProxyBypassListHint, "labelProxyBypassListHint");
            this.labelProxyBypassListHint.Name = "labelProxyBypassListHint";
            // 
            // labelProxyBypassList
            // 
            resources.ApplyResources(this.labelProxyBypassList, "labelProxyBypassList");
            this.labelProxyBypassList.Name = "labelProxyBypassList";
            // 
            // checkUseProxy
            // 
            resources.ApplyResources(this.checkUseProxy, "checkUseProxy");
            this.checkUseProxy.Name = "checkUseProxy";
            this.checkUseProxy.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkUseProxy.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkUseProxy.CheckedChanged += new System.EventHandler(this.checkUseProxy_CheckedChanged);
            // 
            // checkUseIEProxySettings
            // 
            resources.ApplyResources(this.checkUseIEProxySettings, "checkUseIEProxySettings");
            this.checkUseIEProxySettings.CausesValidation = false;
            this.checkUseIEProxySettings.Name = "checkUseIEProxySettings";
            this.checkUseIEProxySettings.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkUseIEProxySettings.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkUseIEProxySettings.CheckedChanged += new System.EventHandler(this.checkUseProxy_CheckedChanged);
            // 
            // labelProxyServerSummery
            // 
            resources.ApplyResources(this.labelProxyServerSummery, "labelProxyServerSummery");
            this.labelProxyServerSummery.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelProxyServerSummery.Name = "labelProxyServerSummery";
            // 
            // textProxyPort
            // 
            resources.ApplyResources(this.textProxyPort, "textProxyPort");
            this.errorProvider1.SetIconAlignment(this.textProxyPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyPort.IconAlignment"))));
            this.textProxyPort.Name = "textProxyPort";
            this.toolTip1.SetToolTip(this.textProxyPort, resources.GetString("textProxyPort.ToolTip"));
            this.textProxyPort.Validated += new System.EventHandler(this.OnControlValidated);
            this.textProxyPort.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // labelProxyPort
            // 
            resources.ApplyResources(this.labelProxyPort, "labelProxyPort");
            this.labelProxyPort.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelProxyPort.Name = "labelProxyPort";
            // 
            // labelProxyAddress
            // 
            this.labelProxyAddress.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.labelProxyAddress, "labelProxyAddress");
            this.labelProxyAddress.Name = "labelProxyAddress";
            // 
            // textProxyAddress
            // 
            resources.ApplyResources(this.textProxyAddress, "textProxyAddress");
            this.errorProvider1.SetIconAlignment(this.textProxyAddress, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyAddress.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.textProxyAddress, ((int)(resources.GetObject("textProxyAddress.IconPadding"))));
            this.textProxyAddress.Name = "textProxyAddress";
            this.toolTip1.SetToolTip(this.textProxyAddress, resources.GetString("textProxyAddress.ToolTip"));
            this.textProxyAddress.Validated += new System.EventHandler(this.OnControlValidated);
            this.textProxyAddress.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // checkProxyBypassLocal
            // 
            resources.ApplyResources(this.checkProxyBypassLocal, "checkProxyBypassLocal");
            this.checkProxyBypassLocal.Name = "checkProxyBypassLocal";
            this.checkProxyBypassLocal.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkProxyBypassLocal.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkProxyBypassLocal.CheckedChanged += new System.EventHandler(this.checkProxyBypassLocal_CheckedChanged);
            // 
            // tabFonts
            // 
            this.tabFonts.Controls.Add(this.sectionPanelFontsSubscriptions);
            resources.ApplyResources(this.tabFonts, "tabFonts");
            this.tabFonts.Name = "tabFonts";
            // 
            // sectionPanelFontsSubscriptions
            // 
            resources.ApplyResources(this.sectionPanelFontsSubscriptions, "sectionPanelFontsSubscriptions");
            this.sectionPanelFontsSubscriptions.Controls.Add(this.labelFontsSubscriptionsSummery);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.btnChangeFont);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.lblUsedFontNameSize);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.chkFontItalic);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.chkFontBold);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.btnChangeColor);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.lblItemStates);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.lstItemStates);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.chkFontUnderline);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.chkFontStrikeout);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.lblFontSampleCaption);
            this.sectionPanelFontsSubscriptions.Controls.Add(this.lblFontSampleABC);
            this.sectionPanelFontsSubscriptions.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelFontsSubscriptions.Image")));
            this.sectionPanelFontsSubscriptions.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelFontsSubscriptions.Name = "sectionPanelFontsSubscriptions";
            // 
            // labelFontsSubscriptionsSummery
            // 
            resources.ApplyResources(this.labelFontsSubscriptionsSummery, "labelFontsSubscriptionsSummery");
            this.labelFontsSubscriptionsSummery.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelFontsSubscriptionsSummery.Name = "labelFontsSubscriptionsSummery";
            // 
            // btnChangeFont
            // 
            resources.ApplyResources(this.btnChangeFont, "btnChangeFont");
            this.btnChangeFont.Name = "btnChangeFont";
            this.btnChangeFont.Click += new System.EventHandler(this.OnDefaultFontChangeClick);
            // 
            // lblUsedFontNameSize
            // 
            resources.ApplyResources(this.lblUsedFontNameSize, "lblUsedFontNameSize");
            this.lblUsedFontNameSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblUsedFontNameSize.Name = "lblUsedFontNameSize";
            // 
            // chkFontItalic
            // 
            resources.ApplyResources(this.chkFontItalic, "chkFontItalic");
            this.chkFontItalic.Name = "chkFontItalic";
            this.chkFontItalic.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
            // 
            // chkFontBold
            // 
            resources.ApplyResources(this.chkFontBold, "chkFontBold");
            this.chkFontBold.Name = "chkFontBold";
            this.chkFontBold.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
            // 
            // btnChangeColor
            // 
            resources.ApplyResources(this.btnChangeColor, "btnChangeColor");
            this.btnChangeColor.Name = "btnChangeColor";
            this.btnChangeColor.Click += new System.EventHandler(this.btnChangeColor_Click);
            // 
            // lblItemStates
            // 
            resources.ApplyResources(this.lblItemStates, "lblItemStates");
            this.lblItemStates.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblItemStates.Name = "lblItemStates";
            // 
            // lstItemStates
            // 
            resources.ApplyResources(this.lstItemStates, "lstItemStates");
            this.lstItemStates.Items.AddRange(new object[] {
            resources.GetString("lstItemStates.Items"),
            resources.GetString("lstItemStates.Items1"),
            resources.GetString("lstItemStates.Items2"),
            resources.GetString("lstItemStates.Items3"),
            resources.GetString("lstItemStates.Items4"),
            resources.GetString("lstItemStates.Items5")});
            this.lstItemStates.Name = "lstItemStates";
            this.toolTip1.SetToolTip(this.lstItemStates, resources.GetString("lstItemStates.ToolTip"));
            this.lstItemStates.SelectedIndexChanged += new System.EventHandler(this.OnItemStatesSelectedIndexChanged);
            // 
            // chkFontUnderline
            // 
            resources.ApplyResources(this.chkFontUnderline, "chkFontUnderline");
            this.chkFontUnderline.Name = "chkFontUnderline";
            this.chkFontUnderline.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
            // 
            // chkFontStrikeout
            // 
            resources.ApplyResources(this.chkFontStrikeout, "chkFontStrikeout");
            this.chkFontStrikeout.Name = "chkFontStrikeout";
            this.chkFontStrikeout.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
            // 
            // lblFontSampleCaption
            // 
            resources.ApplyResources(this.lblFontSampleCaption, "lblFontSampleCaption");
            this.lblFontSampleCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblFontSampleCaption.Name = "lblFontSampleCaption";
            // 
            // lblFontSampleABC
            // 
            resources.ApplyResources(this.lblFontSampleABC, "lblFontSampleABC");
            this.lblFontSampleABC.BackColor = System.Drawing.SystemColors.Window;
            this.lblFontSampleABC.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblFontSampleABC.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblFontSampleABC.Name = "lblFontSampleABC";
            this.lblFontSampleABC.UseMnemonic = false;
            // 
            // tabEnclosures
            // 
            this.tabEnclosures.Controls.Add(this.sectionPanelEnclosurePodcasts);
            this.tabEnclosures.Controls.Add(this.sectionPanelEnclosureGeneral);
            resources.ApplyResources(this.tabEnclosures, "tabEnclosures");
            this.tabEnclosures.Name = "tabEnclosures";
            // 
            // sectionPanelEnclosurePodcasts
            // 
            resources.ApplyResources(this.sectionPanelEnclosurePodcasts, "sectionPanelEnclosurePodcasts");
            this.sectionPanelEnclosurePodcasts.Controls.Add(this.btnPodcastOptions);
            this.sectionPanelEnclosurePodcasts.Controls.Add(this.lblSectionPanelPodcastOptions);
            this.sectionPanelEnclosurePodcasts.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelEnclosurePodcasts.Image")));
            this.sectionPanelEnclosurePodcasts.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelEnclosurePodcasts.Name = "sectionPanelEnclosurePodcasts";
            // 
            // btnPodcastOptions
            // 
            resources.ApplyResources(this.btnPodcastOptions, "btnPodcastOptions");
            this.btnPodcastOptions.Name = "btnPodcastOptions";
            this.btnPodcastOptions.Click += new System.EventHandler(this.OnPodcastOptionsButtonClick);
            // 
            // lblSectionPanelPodcastOptions
            // 
            resources.ApplyResources(this.lblSectionPanelPodcastOptions, "lblSectionPanelPodcastOptions");
            this.lblSectionPanelPodcastOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSectionPanelPodcastOptions.Name = "lblSectionPanelPodcastOptions";
            // 
            // sectionPanelEnclosureGeneral
            // 
            resources.ApplyResources(this.sectionPanelEnclosureGeneral, "sectionPanelEnclosureGeneral");
            this.sectionPanelEnclosureGeneral.Controls.Add(this.btnSelectEnclosureFolder2);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.lblDownloadAttachmentsSmallerThanPostfix);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.numEnclosureCacheSize);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.checkEnclosureSizeOnDiskLimited);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.lblDownloadXAttachmentsPostfix);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.numOnlyDownloadLastXAttachments);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.checkOnlyDownloadLastXAttachments);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.checkDownloadCreateFolderPerFeed);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.textEnclosureDirectory);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.labelEnclosureDirectory);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.checkDownloadEnclosures);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.checkEnableEnclosureAlerts);
            this.sectionPanelEnclosureGeneral.Controls.Add(this.lblSectionPanelEnclosureGeneral);
            this.sectionPanelEnclosureGeneral.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelEnclosureGeneral.Image")));
            this.sectionPanelEnclosureGeneral.ImageLocation = new System.Drawing.Point(0, 20);
            this.sectionPanelEnclosureGeneral.Name = "sectionPanelEnclosureGeneral";
            // 
            // btnSelectEnclosureFolder2
            // 
            resources.ApplyResources(this.btnSelectEnclosureFolder2, "btnSelectEnclosureFolder2");
            this.btnSelectEnclosureFolder2.CausesValidation = false;
            this.btnSelectEnclosureFolder2.Name = "btnSelectEnclosureFolder2";
            this.toolTip1.SetToolTip(this.btnSelectEnclosureFolder2, resources.GetString("btnSelectEnclosureFolder2.ToolTip"));
            this.btnSelectEnclosureFolder2.Click += new System.EventHandler(this.btnSelectEnclosureFolder2_Click);
            // 
            // lblDownloadAttachmentsSmallerThanPostfix
            // 
            resources.ApplyResources(this.lblDownloadAttachmentsSmallerThanPostfix, "lblDownloadAttachmentsSmallerThanPostfix");
            this.lblDownloadAttachmentsSmallerThanPostfix.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblDownloadAttachmentsSmallerThanPostfix.Name = "lblDownloadAttachmentsSmallerThanPostfix";
            // 
            // numEnclosureCacheSize
            // 
            resources.ApplyResources(this.numEnclosureCacheSize, "numEnclosureCacheSize");
            this.numEnclosureCacheSize.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numEnclosureCacheSize.Name = "numEnclosureCacheSize";
            this.numEnclosureCacheSize.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numEnclosureCacheSize.Validated += new System.EventHandler(this.OnControlValidated);
            this.numEnclosureCacheSize.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // checkEnclosureSizeOnDiskLimited
            // 
            resources.ApplyResources(this.checkEnclosureSizeOnDiskLimited, "checkEnclosureSizeOnDiskLimited");
            this.checkEnclosureSizeOnDiskLimited.BackColor = System.Drawing.SystemColors.Control;
            this.checkEnclosureSizeOnDiskLimited.Name = "checkEnclosureSizeOnDiskLimited";
            this.checkEnclosureSizeOnDiskLimited.UseVisualStyleBackColor = false;
            this.checkEnclosureSizeOnDiskLimited.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkEnclosureSizeOnDiskLimited.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkEnclosureSizeOnDiskLimited.CheckedChanged += new System.EventHandler(this.checkEnclosureSizeOnDiskLimited_CheckedChanged);
            // 
            // lblDownloadXAttachmentsPostfix
            // 
            resources.ApplyResources(this.lblDownloadXAttachmentsPostfix, "lblDownloadXAttachmentsPostfix");
            this.lblDownloadXAttachmentsPostfix.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDownloadXAttachmentsPostfix.Name = "lblDownloadXAttachmentsPostfix";
            // 
            // numOnlyDownloadLastXAttachments
            // 
            resources.ApplyResources(this.numOnlyDownloadLastXAttachments, "numOnlyDownloadLastXAttachments");
            this.numOnlyDownloadLastXAttachments.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numOnlyDownloadLastXAttachments.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numOnlyDownloadLastXAttachments.Name = "numOnlyDownloadLastXAttachments";
            this.numOnlyDownloadLastXAttachments.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numOnlyDownloadLastXAttachments.Validated += new System.EventHandler(this.OnControlValidated);
            this.numOnlyDownloadLastXAttachments.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // checkOnlyDownloadLastXAttachments
            // 
            resources.ApplyResources(this.checkOnlyDownloadLastXAttachments, "checkOnlyDownloadLastXAttachments");
            this.checkOnlyDownloadLastXAttachments.BackColor = System.Drawing.SystemColors.Control;
            this.checkOnlyDownloadLastXAttachments.Name = "checkOnlyDownloadLastXAttachments";
            this.checkOnlyDownloadLastXAttachments.UseVisualStyleBackColor = false;
            this.checkOnlyDownloadLastXAttachments.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkOnlyDownloadLastXAttachments.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkOnlyDownloadLastXAttachments.CheckedChanged += new System.EventHandler(this.checkOnlyDownloadLastXAttachments_CheckedChanged);
            // 
            // checkDownloadCreateFolderPerFeed
            // 
            resources.ApplyResources(this.checkDownloadCreateFolderPerFeed, "checkDownloadCreateFolderPerFeed");
            this.checkDownloadCreateFolderPerFeed.Name = "checkDownloadCreateFolderPerFeed";
            this.checkDownloadCreateFolderPerFeed.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkDownloadCreateFolderPerFeed.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // textEnclosureDirectory
            // 
            this.textEnclosureDirectory.AllowDrop = true;
            resources.ApplyResources(this.textEnclosureDirectory, "textEnclosureDirectory");
            this.textEnclosureDirectory.Name = "textEnclosureDirectory";
            this.toolTip1.SetToolTip(this.textEnclosureDirectory, resources.GetString("textEnclosureDirectory.ToolTip"));
            this.textEnclosureDirectory.Validated += new System.EventHandler(this.OnControlValidated);
            this.textEnclosureDirectory.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            // 
            // labelEnclosureDirectory
            // 
            resources.ApplyResources(this.labelEnclosureDirectory, "labelEnclosureDirectory");
            this.labelEnclosureDirectory.Name = "labelEnclosureDirectory";
            // 
            // checkDownloadEnclosures
            // 
            resources.ApplyResources(this.checkDownloadEnclosures, "checkDownloadEnclosures");
            this.checkDownloadEnclosures.BackColor = System.Drawing.SystemColors.Control;
            this.checkDownloadEnclosures.Name = "checkDownloadEnclosures";
            this.checkDownloadEnclosures.UseVisualStyleBackColor = false;
            this.checkDownloadEnclosures.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkDownloadEnclosures.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkDownloadEnclosures.CheckedChanged += new System.EventHandler(this.checkDownloadEnclosures_CheckedChanged);
            // 
            // checkEnableEnclosureAlerts
            // 
            resources.ApplyResources(this.checkEnableEnclosureAlerts, "checkEnableEnclosureAlerts");
            this.checkEnableEnclosureAlerts.Name = "checkEnableEnclosureAlerts";
            this.checkEnableEnclosureAlerts.Validated += new System.EventHandler(this.OnControlValidated);
            this.checkEnableEnclosureAlerts.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
            this.checkEnableEnclosureAlerts.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
            // 
            // lblSectionPanelEnclosureGeneral
            // 
            resources.ApplyResources(this.lblSectionPanelEnclosureGeneral, "lblSectionPanelEnclosureGeneral");
            this.lblSectionPanelEnclosureGeneral.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSectionPanelEnclosureGeneral.Name = "lblSectionPanelEnclosureGeneral";
            // 
            // btnApply
            // 
            resources.ApplyResources(this.btnApply, "btnApply");
            this.btnApply.CausesValidation = false;
            this.btnApply.Name = "btnApply";
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            resources.ApplyResources(this.errorProvider1, "errorProvider1");
            // 
            // fontDialog1
            // 
            this.fontDialog1.ShowColor = true;
            // 
            // openExeFileDialog
            // 
            this.openExeFileDialog.DefaultExt = "exe";
            resources.ApplyResources(this.openExeFileDialog, "openExeFileDialog");
            // 
            // securityHintProvider
            // 
            this.securityHintProvider.ContainerControl = this;
            resources.ApplyResources(this.securityHintProvider, "securityHintProvider");
            // 
            // PreferencesDialog
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.tabPrefs);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreferencesDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.tabPrefs.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.optionSectionPanel1.ResumeLayout(false);
            this.sectionPanelGeneralStartup.ResumeLayout(false);
            this.sectionPanelGeneralBehavior.ResumeLayout(false);
            this.tabRemoteStorage.ResumeLayout(false);
            this.sectionPanelRemoteStorageFeedlist.ResumeLayout(false);
            this.sectionPanelRemoteStorageFeedlist.PerformLayout();
            this.tabNewsItems.ResumeLayout(false);
            this.sectionPanelDisplayGeneral.ResumeLayout(false);
            this.sectionPanelDisplayItemFormatting.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numNewsItemsPerPage)).EndInit();
            this.tabFeeds.ResumeLayout(false);
            this.sectionPanelFeedsCommentDefs.ResumeLayout(false);
            this.sectionPanelFeedsTimings.ResumeLayout(false);
            this.tabWebBrowser.ResumeLayout(false);
            this.sectionPanelWebBrowserSecurity.ResumeLayout(false);
            this.sectionPanelWebBrowserOnNewWindow.ResumeLayout(false);
            this.sectionPanelWebBrowserOnNewWindow.PerformLayout();
            this.tabWebSearch.ResumeLayout(false);
            this.sectionPanelWebSearchEngines.ResumeLayout(false);
            this.tabNetConnection.ResumeLayout(false);
            this.sectionPanelNetworkConnectionProxy.ResumeLayout(false);
            this.sectionPanelNetworkConnectionProxy.PerformLayout();
            this.tabFonts.ResumeLayout(false);
            this.sectionPanelFontsSubscriptions.ResumeLayout(false);
            this.tabEnclosures.ResumeLayout(false);
            this.sectionPanelEnclosurePodcasts.ResumeLayout(false);
            this.sectionPanelEnclosureGeneral.ResumeLayout(false);
            this.sectionPanelEnclosureGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEnclosureCacheSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOnlyDownloadLastXAttachments)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.securityHintProvider)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabPrefs;
		private System.Windows.Forms.ToolTip toolTip1;
		internal System.Windows.Forms.CheckBox checkProxyBypassLocal;
		internal System.Windows.Forms.CheckBox checkProxyAuth;
		internal System.Windows.Forms.TextBox textProxyCredentialPassword;
		internal System.Windows.Forms.TextBox textProxyCredentialUser;
		internal System.Windows.Forms.TextBox textProxyPort;
		internal System.Windows.Forms.TextBox textProxyAddress;
		private System.Windows.Forms.Label labelProxyPort;
		private System.Windows.Forms.Label labelProxyAddress;
		private System.Windows.Forms.Label labelProxyCredentialPassword;
		private System.Windows.Forms.Label labelProxyCredentialUserName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelFormatters;
		internal System.Windows.Forms.ComboBox comboFormatters;
		internal System.Windows.Forms.CheckBox checkCustomFormatter;
		internal System.Windows.Forms.RadioButton radioTrayActionNone;
		private System.Windows.Forms.Label label1;
		internal System.Windows.Forms.RadioButton radioTrayActionMinimize;
		internal System.Windows.Forms.RadioButton radioTrayActionClose;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.FontDialog fontDialog1;
		private System.Windows.Forms.Label label10;
		internal System.Windows.Forms.CheckBox checkUseRemoteStorage;
		private System.Windows.Forms.Label labelRemoteStoragePassword;
		internal System.Windows.Forms.TextBox textRemoteStoragePassword;
		private System.Windows.Forms.Label labelRemoteStorageUserName;
		internal System.Windows.Forms.TextBox textRemoteStorageUserName;
		private System.Windows.Forms.Label labelExperimental;
		private System.Windows.Forms.Label labelRemoteStorageLocation;
		internal System.Windows.Forms.TextBox textRemoteStorageLocation;
		private System.Windows.Forms.Label labelRemoteStorageProtocol;
		internal System.Windows.Forms.ComboBox comboRemoteStorageProtocol;
		private System.Windows.Forms.TabPage tabWebBrowser;
		internal System.Windows.Forms.RadioButton optNewWindowCustomExec;
		internal System.Windows.Forms.RadioButton optNewWindowDefaultWebBrowser;
		internal System.Windows.Forms.RadioButton optNewWindowOnTab;
		private System.Windows.Forms.Label label16;
		internal System.Windows.Forms.TextBox txtBrowserStartExecutable;
		private System.Windows.Forms.Label labelBrowserStartExecutable;
		private System.Windows.Forms.TabPage tabWebSearch;
		private System.Windows.Forms.Label label17;
		internal System.Windows.Forms.ListView listSearchEngines;
		private System.Windows.Forms.ImageList imagesSearchEngines;
		private System.Windows.Forms.Button btnSEMoveUp;
		private System.Windows.Forms.Button btnSEMoveDown;
		private System.Windows.Forms.Button btnSEProperties;
		private System.Windows.Forms.Button btnSEAdd;
		private System.Windows.Forms.Button btnSERemove;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ColumnHeader columnHeader0;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.LinkLabel linkCommentAPI;
		private System.Windows.Forms.Label label6;
		internal System.Windows.Forms.CheckBox checkRefreshFeedsOnStartup;
		internal System.Windows.Forms.RadioButton checkNoProxy;
		internal System.Windows.Forms.RadioButton checkUseIEProxySettings;
		internal System.Windows.Forms.RadioButton checkUseProxy;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabNetConnection;
		private System.Windows.Forms.TabPage tabNewsItems;
		private System.Windows.Forms.TabPage tabFeeds;
		private System.Windows.Forms.TabPage tabFonts;
		private System.Windows.Forms.TabPage tabRemoteStorage;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		internal System.Windows.Forms.ComboBox comboRefreshRate;
		internal System.Windows.Forms.CheckBox chkNewsItemOpenLinkInDetailWindow;
		private System.Windows.Forms.Label label15;
		internal System.Windows.Forms.ComboBox comboMaxItemAge;
		private System.Windows.Forms.Label label12;
		internal System.Windows.Forms.CheckBox checkBrowserJavascriptAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserActiveXAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserJavaAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserVideoAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserBGSoundAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserImagesAllowed;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Button btnSelectExecutable;
		private System.Windows.Forms.OpenFileDialog openExeFileDialog;
		private System.Windows.Forms.Label labelProxyBypassList;
		internal System.Windows.Forms.TextBox textProxyBypassList;
		private System.Windows.Forms.Label labelProxyBypassListHint;
		internal System.Windows.Forms.ComboBox cboUserIdentityForComments;
		private System.Windows.Forms.Button btnManageIdentities;
		private System.Windows.Forms.Label lblUsedFontNameSize;
		private System.Windows.Forms.Button btnChangeFont;
		private System.Windows.Forms.ListBox lstItemStates;
		private System.Windows.Forms.Button btnChangeColor;
		private System.Windows.Forms.CheckBox chkFontBold;
		private System.Windows.Forms.CheckBox chkFontItalic;
		private System.Windows.Forms.CheckBox chkFontStrikeout;
		private System.Windows.Forms.CheckBox chkFontUnderline;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.Label lblItemStates;
		private System.Windows.Forms.Label lblFontSampleCaption;
		private System.Windows.Forms.Label lblIdentityDropdownCaption;
		private System.Windows.Forms.Label lblFontSampleABC;
		internal System.Windows.Forms.CheckBox checkReuseFirstBrowserTab;
		private OptionSectionPanel sectionPanelGeneralBehavior;
		private OptionSectionPanel sectionPanelGeneralStartup;
		private OptionSectionPanel sectionPanelWebBrowserOnNewWindow;
		private System.Windows.Forms.Label labelCheckToAllow;
		private OptionSectionPanel sectionPanelFontsSubscriptions;
		private System.Windows.Forms.Label labelFontsSubscriptionsSummery;
		private OptionSectionPanel sectionPanelNetworkConnectionProxy;
		private System.Windows.Forms.Label labelProxyServerSummery;
		private OptionSectionPanel sectionPanelDisplayItemFormatting;
		private OptionSectionPanel sectionPanelFeedsTimings;
		private OptionSectionPanel sectionPanelFeedsCommentDefs;
		private OptionSectionPanel sectionPanelRemoteStorageFeedlist;
		private OptionSectionPanel sectionPanelWebSearchEngines;
		private OptionSectionPanel sectionPanelDisplayGeneral;
        private OptionSectionPanel sectionPanelWebBrowserSecurity;
		private System.Windows.Forms.Label lblCheckImage;
		internal System.Windows.Forms.CheckBox checkOpenTabsInBackground;
		private System.Windows.Forms.Label label7;
		internal System.Windows.Forms.CheckBox checkBox1;
		internal System.Windows.Forms.CheckBox checkBox2;
		internal System.Windows.Forms.CheckBox checkBox3;
		internal System.Windows.Forms.CheckBox checkBox4;
		internal System.Windows.Forms.CheckBox checkBox5;
		internal System.Windows.Forms.CheckBox checkBox6;
		internal System.Windows.Forms.CheckBox checkUseFavicons;
		private System.Windows.Forms.TabPage tabEnclosures;
		private RssBandit.WinGui.Controls.OptionSectionPanel sectionPanelEnclosureGeneral;
		private System.Windows.Forms.Label lblSectionPanelEnclosureGeneral;
		internal System.Windows.Forms.TextBox textEnclosureDirectory;
		private System.Windows.Forms.Label labelEnclosureDirectory;
		internal System.Windows.Forms.CheckBox checkDownloadEnclosures;
		internal System.Windows.Forms.CheckBox checkEnableEnclosureAlerts;
		private RssBandit.WinGui.Controls.OptionSectionPanel sectionPanelEnclosurePodcasts;
		private System.Windows.Forms.Label lblSectionPanelPodcastOptions;
		internal System.Windows.Forms.CheckBox checkDownloadCreateFolderPerFeed;
		private System.Windows.Forms.Button btnPodcastOptions;
		internal System.Windows.Forms.CheckBox checkOnlyDownloadLastXAttachments;
		internal System.Windows.Forms.NumericUpDown numOnlyDownloadLastXAttachments;
		private System.Windows.Forms.Label lblDownloadXAttachmentsPostfix;
		private System.Windows.Forms.Label lblDownloadAttachmentsSmallerThanPostfix;
		internal System.Windows.Forms.NumericUpDown numEnclosureCacheSize;
		internal System.Windows.Forms.CheckBox checkEnclosureSizeOnDiskLimited;
		private System.Windows.Forms.Button btnSelectEnclosureFolder2;
		internal System.Windows.Forms.CheckBox checkRunAtStartup;
		internal System.Windows.Forms.CheckBox checkAllowAppEventSounds;
		internal System.Windows.Forms.Button btnConfigureAppSounds;
		private RssBandit.WinGui.Controls.OptionSectionPanel optionSectionPanel1;
		private System.Windows.Forms.ErrorProvider securityHintProvider;
		private System.Windows.Forms.Label label8;
		internal System.Windows.Forms.NumericUpDown numNewsItemsPerPage;
		internal System.Windows.Forms.CheckBox checkLimitNewsItemsPerPage;
		internal System.Windows.Forms.CheckBox checkMarkItemsAsReadWhenViewed;
		internal System.Windows.Forms.CheckBox checkMarkItemsReadOnExit;
		internal CheckBox checkClearFeedCategoryItemAgeSettings;
		internal CheckBox checkResetIndividualRefreshRates;

	}
}
