#region CVS Version Header
/*
 * $Id: PreferencesDialog.cs,v 1.50 2005/06/10 18:24:04 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/06/10 18:24:04 $
 * $Revision: 1.50 $
 */
#endregion

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Utility;
using RssBandit.WebSearch;
using RssBandit.Dialogs;
using NewsComponents.Feed;

using Logger = RssBandit.Common.Logging;

namespace RssBandit.WinGui.Forms {

	/// <summary>
	/// Summary description for RssBanditProperties.
	/// </summary>
	public class PreferencesDialog : System.Windows.Forms.Form {

		public event EventHandler OnApplyPreferences;
	
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(PreferencesDialog));

		private Hashtable imageIndexMap = new Hashtable();
		internal ArrayList searchEngines = null;
		internal bool searchEnginesModified = false;
		private IdentityNewsServerManager identityManager = null;
		internal Font[] itemStateFonts;
		internal Color[] itemStateColors;

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
		private System.Windows.Forms.Label labelCheckForUpdates;
		internal System.Windows.Forms.ComboBox comboAppUpdateFrequency;
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
		internal System.Windows.Forms.Button btnMakeDefaultAggregator;
		internal System.Windows.Forms.CheckBox checkBrowserJavascriptAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserActiveXAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserJavaAllowed;
		internal System.Windows.Forms.CheckBox checkBrowserVdieoAllowed;
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
		internal System.Windows.Forms.CheckBox checkMarkItemsReadOnExit;


		internal PreferencesDialog(int refreshRate, RssBanditPreferences prefs, SearchEngineHandler seHandler, IdentityNewsServerManager identityManager): this() {
		
			this.identityManager = identityManager;
			this.PopulateComboUserIdentityForComments(identityManager.CurrentIdentities, prefs.UserIdentityForComments);
			
			//general
			switch (prefs.HideToTrayAction) {
				case HideToTray.OnMinimize:
					this.radioTrayActionMinimize.Checked = true;
					break;
				case HideToTray.OnClose:
					this.radioTrayActionClose.Checked = true;
					break;
				default:
					this.radioTrayActionNone.Checked = true;
					break;
			}

			comboAppUpdateFrequency.SelectedIndex = (int)prefs.AutoUpdateFrequency;

			// proxy stuff
			this.textProxyAddress.Text = prefs.ProxyAddress; 
			this.textProxyPort.Text = (prefs.ProxyPort != 0 ? prefs.ProxyPort + "": "");

			this.checkProxyBypassLocal.Checked = prefs.BypassProxyOnLocal;
			if (prefs.ProxyBypassList != null) {
				foreach (string bypass in prefs.ProxyBypassList) {
					if (this.textProxyBypassList.Text.Length > 0)
						this.textProxyBypassList.Text += "; ";

					this.textProxyBypassList.Text += bypass.Trim();
				}
			}

			this.textProxyCredentialUser.Text = prefs.ProxyUser;
			this.textProxyCredentialPassword.Text = prefs.ProxyPassword; 

			this.comboRefreshRate.Text = refreshRate.ToString() + ""; 
			this.comboRefreshRate.Refresh(); 
			
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
			

				if (prefs.NewsItemStylesheetFile != null && 
					prefs.NewsItemStylesheetFile.Length > 0 &&
					File.Exists(Path.Combine(tmplFolder, prefs.NewsItemStylesheetFile + ".fdxsl"))) {
					this.comboFormatters.Text = prefs.NewsItemStylesheetFile; 
					this.checkCustomFormatter.Checked = true;
				}
			
			}else {
				this.comboFormatters.Text = String.Empty; 
				this.checkCustomFormatter.Checked = false;
				
			}
			this.checkCustomFormatter_CheckedChanged(null, null);
			this.comboFormatters.Refresh();

			this.checkReuseFirstBrowserTab.Checked = prefs.ReuseFirstBrowserTab;
			this.checkMarkItemsReadOnExit.Checked = prefs.MarkItemsReadOnExit; 

			this.MaxItemAge = prefs.MaxItemAge;

			// moved to this location, because of the validation init involved herein
			this.checkUseProxy.Checked = prefs.UseProxy; 
			this.checkUseIEProxySettings.Checked = prefs.UseIEProxySettings;
			this.checkProxyAuth.Checked = prefs.ProxyCustomCredentials;

			if (!this.checkUseProxy.Checked && !this.checkUseIEProxySettings.Checked)
				this.checkUseProxy_CheckedChanged(this, null);
			if (!this.checkProxyAuth.Checked)
				this.checkProxyAuth_CheckedChanged(this, null);
			
			// font tab
			this.SetFontForState(FontStates.Read, prefs.NormalFont);	// default font
			this.SetFontForState(FontStates.Unread, prefs.HighlightFont);	
			this.SetFontForState(FontStates.Flag, prefs.FlagFont);	
			this.SetFontForState(FontStates.Referrer, prefs.RefererFont);
			this.SetFontForState(FontStates.Error, prefs.ErrorFont);	
			this.SetColorForState(FontStates.Read, prefs.NormalFontColor);
			this.SetColorForState(FontStates.Unread, prefs.HighlightFontColor);
			this.SetColorForState(FontStates.Flag, prefs.FlagFontColor);
			this.SetColorForState(FontStates.Referrer, prefs.RefererFontColor);
			this.SetColorForState(FontStates.Error, prefs.ErrorFontColor);

			this.RefreshFontFamilySizeSample();
			this.lstItemStates.SelectedIndex = 1;	// raise event to refresh checkboxes and Sample

			// Remote storage tab
			this.textRemoteStorageUserName.Text = prefs.RemoteStorageUserName;
			this.textRemoteStoragePassword.Text = prefs.RemoteStoragePassword;
			this.textRemoteStorageLocation.Text = prefs.RemoteStorageLocation;
			this.checkUseRemoteStorage.Checked = prefs.UseRemoteStorage;

			int oldIndex = comboRemoteStorageProtocol.SelectedIndex;

			switch (prefs.RemoteStorageProtocol)
			{
				case RemoteStorageProtocolType.UNC:
					comboRemoteStorageProtocol.SelectedIndex = 0;
					break;
				case RemoteStorageProtocolType.FTP:
					comboRemoteStorageProtocol.SelectedIndex = 1;
					break;
				case RemoteStorageProtocolType.dasBlog:
					comboRemoteStorageProtocol.SelectedIndex = 2;
					break;
				case RemoteStorageProtocolType.dasBlog_1_3:
					comboRemoteStorageProtocol.SelectedIndex = 3;
					break;
				case RemoteStorageProtocolType.WebDAV:
					comboRemoteStorageProtocol.SelectedIndex = 4;
					break;
			}
			
			checkUseRemoteStorage_CheckedChanged(this, null);
			if (oldIndex == comboRemoteStorageProtocol.SelectedIndex) {	// the change event was not yet fired
				comboRemoteStorageProtocol_SelectedIndexChanged(this, null);
			}

			this.txtBrowserStartExecutable.Text = prefs.BrowserCustomExecOnNewWindow;
		
			switch (prefs.BrowserOnNewWindow) {
				case BrowserBehaviorOnNewWindow.OpenNewTab:
					this.optNewWindowOnTab.Checked = true;
					break;
				case BrowserBehaviorOnNewWindow.OpenDefaultBrowser:
					this.optNewWindowDefaultWebBrowser.Checked = true;
					break;
				default:
					this.optNewWindowCustomExec.Checked = true;
					break;
			}

			optNewWindowCustomExec_CheckedChanged(this, new EventArgs());

			this.chkNewsItemOpenLinkInDetailWindow.Checked = prefs.NewsItemOpenLinkInDetailWindow;

			if (seHandler != null && seHandler.EnginesOK) {
				this.searchEngines = (ArrayList)seHandler.Engines.Clone();
			} else {
				this.searchEngines = new ArrayList();
			}

			InitWebSearchEnginesTab();

			btnMakeDefaultAggregator.Enabled = (!RssBanditApplication.IsDefaultAggregator());

			checkBrowserJavascriptAllowed.Checked = prefs.BrowserJavascriptAllowed;
			checkBrowserJavaAllowed.Checked = prefs.BrowserJavaAllowed;
			checkBrowserActiveXAllowed.Checked = prefs.BrowserActiveXAllowed;
			checkBrowserBGSoundAllowed.Checked = prefs.BrowserBGSoundAllowed;
			checkBrowserVdieoAllowed.Checked = prefs.BrowserVideoAllowed;
			checkBrowserImagesAllowed.Checked = prefs.BrowserImagesAllowed;

			this.checkRefreshFeedsOnStartup.Checked = prefs.FeedRefreshOnStartup;

			this.btnApply.Enabled = false;
		}

		public PreferencesDialog() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			itemStateFonts = new Font[lstItemStates.Items.Count];
			itemStateColors = new Color[lstItemStates.Items.Count];

			this.Load += new EventHandler(this.OnPreferencesDialog_Load);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
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
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PreferencesDialog));
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tabPrefs = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.sectionPanelGeneralStartup = new RssBandit.WinGui.Controls.OptionSectionPanel();
			this.checkRefreshFeedsOnStartup = new System.Windows.Forms.CheckBox();
			this.sectionPanelGeneralBehavior = new RssBandit.WinGui.Controls.OptionSectionPanel();
			this.btnMakeDefaultAggregator = new System.Windows.Forms.Button();
			this.label12 = new System.Windows.Forms.Label();
			this.radioTrayActionNone = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.radioTrayActionClose = new System.Windows.Forms.RadioButton();
			this.radioTrayActionMinimize = new System.Windows.Forms.RadioButton();
			this.labelCheckForUpdates = new System.Windows.Forms.Label();
			this.comboAppUpdateFrequency = new System.Windows.Forms.ComboBox();
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
			this.checkMarkItemsReadOnExit = new System.Windows.Forms.CheckBox();
			this.chkNewsItemOpenLinkInDetailWindow = new System.Windows.Forms.CheckBox();
			this.sectionPanelDisplayItemFormatting = new RssBandit.WinGui.Controls.OptionSectionPanel();
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
			this.label15 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.comboRefreshRate = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.comboMaxItemAge = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
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
			this.tabWebBrowser = new System.Windows.Forms.TabPage();
			this.sectionPanelWebBrowserSecurity = new RssBandit.WinGui.Controls.OptionSectionPanel();
			this.labelCheckToAllow = new System.Windows.Forms.Label();
			this.checkBrowserActiveXAllowed = new System.Windows.Forms.CheckBox();
			this.checkBrowserJavascriptAllowed = new System.Windows.Forms.CheckBox();
			this.checkBrowserJavaAllowed = new System.Windows.Forms.CheckBox();
			this.checkBrowserBGSoundAllowed = new System.Windows.Forms.CheckBox();
			this.checkBrowserVdieoAllowed = new System.Windows.Forms.CheckBox();
			this.checkBrowserImagesAllowed = new System.Windows.Forms.CheckBox();
			this.label19 = new System.Windows.Forms.Label();
			this.sectionPanelWebBrowserOnNewWindow = new RssBandit.WinGui.Controls.OptionSectionPanel();
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
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.btnApply = new System.Windows.Forms.Button();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
			this.fontDialog1 = new System.Windows.Forms.FontDialog();
			this.openExeFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.tabPrefs.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.sectionPanelGeneralStartup.SuspendLayout();
			this.sectionPanelGeneralBehavior.SuspendLayout();
			this.tabRemoteStorage.SuspendLayout();
			this.sectionPanelRemoteStorageFeedlist.SuspendLayout();
			this.tabNewsItems.SuspendLayout();
			this.sectionPanelDisplayGeneral.SuspendLayout();
			this.sectionPanelDisplayItemFormatting.SuspendLayout();
			this.tabFeeds.SuspendLayout();
			this.sectionPanelFeedsCommentDefs.SuspendLayout();
			this.sectionPanelFeedsTimings.SuspendLayout();
			this.tabNetConnection.SuspendLayout();
			this.sectionPanelNetworkConnectionProxy.SuspendLayout();
			this.tabFonts.SuspendLayout();
			this.sectionPanelFontsSubscriptions.SuspendLayout();
			this.tabWebBrowser.SuspendLayout();
			this.sectionPanelWebBrowserSecurity.SuspendLayout();
			this.sectionPanelWebBrowserOnNewWindow.SuspendLayout();
			this.tabWebSearch.SuspendLayout();
			this.sectionPanelWebSearchEngines.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.AccessibleDescription = resources.GetString("btnOK.AccessibleDescription");
			this.btnOK.AccessibleName = resources.GetString("btnOK.AccessibleName");
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOK.Anchor")));
			this.btnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOK.BackgroundImage")));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOK.Dock")));
			this.btnOK.Enabled = ((bool)(resources.GetObject("btnOK.Enabled")));
			this.errorProvider1.SetError(this.btnOK, resources.GetString("btnOK.Error"));
			this.btnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOK.FlatStyle")));
			this.btnOK.Font = ((System.Drawing.Font)(resources.GetObject("btnOK.Font")));
			this.errorProvider1.SetIconAlignment(this.btnOK, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnOK.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnOK, ((int)(resources.GetObject("btnOK.IconPadding"))));
			this.btnOK.Image = ((System.Drawing.Image)(resources.GetObject("btnOK.Image")));
			this.btnOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOK.ImageAlign")));
			this.btnOK.ImageIndex = ((int)(resources.GetObject("btnOK.ImageIndex")));
			this.btnOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOK.ImeMode")));
			this.btnOK.Location = ((System.Drawing.Point)(resources.GetObject("btnOK.Location")));
			this.btnOK.Name = "btnOK";
			this.btnOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOK.RightToLeft")));
			this.btnOK.Size = ((System.Drawing.Size)(resources.GetObject("btnOK.Size")));
			this.btnOK.TabIndex = ((int)(resources.GetObject("btnOK.TabIndex")));
			this.btnOK.Text = resources.GetString("btnOK.Text");
			this.btnOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOK.TextAlign")));
			this.toolTip1.SetToolTip(this.btnOK, resources.GetString("btnOK.ToolTip"));
			this.btnOK.Visible = ((bool)(resources.GetObject("btnOK.Visible")));
			this.btnOK.Click += new System.EventHandler(this.OnOKClick);
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.CausesValidation = false;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.errorProvider1.SetError(this.btnCancel, resources.GetString("btnCancel.Error"));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.errorProvider1.SetIconAlignment(this.btnCancel, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnCancel.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnCancel, ((int)(resources.GetObject("btnCancel.IconPadding"))));
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
			// tabPrefs
			// 
			this.tabPrefs.AccessibleDescription = resources.GetString("tabPrefs.AccessibleDescription");
			this.tabPrefs.AccessibleName = resources.GetString("tabPrefs.AccessibleName");
			this.tabPrefs.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tabPrefs.Alignment")));
			this.tabPrefs.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPrefs.Anchor")));
			this.tabPrefs.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tabPrefs.Appearance")));
			this.tabPrefs.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPrefs.BackgroundImage")));
			this.tabPrefs.Controls.Add(this.tabGeneral);
			this.tabPrefs.Controls.Add(this.tabRemoteStorage);
			this.tabPrefs.Controls.Add(this.tabNewsItems);
			this.tabPrefs.Controls.Add(this.tabFeeds);
			this.tabPrefs.Controls.Add(this.tabNetConnection);
			this.tabPrefs.Controls.Add(this.tabFonts);
			this.tabPrefs.Controls.Add(this.tabWebBrowser);
			this.tabPrefs.Controls.Add(this.tabWebSearch);
			this.tabPrefs.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPrefs.Dock")));
			this.tabPrefs.Enabled = ((bool)(resources.GetObject("tabPrefs.Enabled")));
			this.errorProvider1.SetError(this.tabPrefs, resources.GetString("tabPrefs.Error"));
			this.tabPrefs.Font = ((System.Drawing.Font)(resources.GetObject("tabPrefs.Font")));
			this.errorProvider1.SetIconAlignment(this.tabPrefs, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabPrefs.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabPrefs, ((int)(resources.GetObject("tabPrefs.IconPadding"))));
			this.tabPrefs.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPrefs.ImeMode")));
			this.tabPrefs.ItemSize = ((System.Drawing.Size)(resources.GetObject("tabPrefs.ItemSize")));
			this.tabPrefs.Location = ((System.Drawing.Point)(resources.GetObject("tabPrefs.Location")));
			this.tabPrefs.Multiline = true;
			this.tabPrefs.Name = "tabPrefs";
			this.tabPrefs.Padding = ((System.Drawing.Point)(resources.GetObject("tabPrefs.Padding")));
			this.tabPrefs.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPrefs.RightToLeft")));
			this.tabPrefs.SelectedIndex = 0;
			this.tabPrefs.ShowToolTips = ((bool)(resources.GetObject("tabPrefs.ShowToolTips")));
			this.tabPrefs.Size = ((System.Drawing.Size)(resources.GetObject("tabPrefs.Size")));
			this.tabPrefs.TabIndex = ((int)(resources.GetObject("tabPrefs.TabIndex")));
			this.tabPrefs.Text = resources.GetString("tabPrefs.Text");
			this.toolTip1.SetToolTip(this.tabPrefs, resources.GetString("tabPrefs.ToolTip"));
			this.tabPrefs.Visible = ((bool)(resources.GetObject("tabPrefs.Visible")));
			this.tabPrefs.Resize += new EventHandler(this.OnTabPrefs_Resize);
			// 
			// tabGeneral
			// 
			this.tabGeneral.AccessibleDescription = resources.GetString("tabGeneral.AccessibleDescription");
			this.tabGeneral.AccessibleName = resources.GetString("tabGeneral.AccessibleName");
			this.tabGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabGeneral.Anchor")));
			this.tabGeneral.AutoScroll = ((bool)(resources.GetObject("tabGeneral.AutoScroll")));
			this.tabGeneral.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabGeneral.AutoScrollMargin")));
			this.tabGeneral.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabGeneral.AutoScrollMinSize")));
			this.tabGeneral.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabGeneral.BackgroundImage")));
			this.tabGeneral.Controls.Add(this.sectionPanelGeneralStartup);
			this.tabGeneral.Controls.Add(this.sectionPanelGeneralBehavior);
			this.tabGeneral.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabGeneral.Dock")));
			this.tabGeneral.Enabled = ((bool)(resources.GetObject("tabGeneral.Enabled")));
			this.errorProvider1.SetError(this.tabGeneral, resources.GetString("tabGeneral.Error"));
			this.tabGeneral.Font = ((System.Drawing.Font)(resources.GetObject("tabGeneral.Font")));
			this.errorProvider1.SetIconAlignment(this.tabGeneral, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabGeneral.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabGeneral, ((int)(resources.GetObject("tabGeneral.IconPadding"))));
			this.tabGeneral.ImageIndex = ((int)(resources.GetObject("tabGeneral.ImageIndex")));
			this.tabGeneral.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabGeneral.ImeMode")));
			this.tabGeneral.Location = ((System.Drawing.Point)(resources.GetObject("tabGeneral.Location")));
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabGeneral.RightToLeft")));
			this.tabGeneral.Size = ((System.Drawing.Size)(resources.GetObject("tabGeneral.Size")));
			this.tabGeneral.TabIndex = ((int)(resources.GetObject("tabGeneral.TabIndex")));
			this.tabGeneral.Text = resources.GetString("tabGeneral.Text");
			this.toolTip1.SetToolTip(this.tabGeneral, resources.GetString("tabGeneral.ToolTip"));
			this.tabGeneral.ToolTipText = resources.GetString("tabGeneral.ToolTipText");
			this.tabGeneral.Visible = ((bool)(resources.GetObject("tabGeneral.Visible")));
			// 
			// sectionPanelGeneralStartup
			// 
			this.sectionPanelGeneralStartup.AccessibleDescription = resources.GetString("sectionPanelGeneralStartup.AccessibleDescription");
			this.sectionPanelGeneralStartup.AccessibleName = resources.GetString("sectionPanelGeneralStartup.AccessibleName");
			this.sectionPanelGeneralStartup.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelGeneralStartup.Anchor")));
			this.sectionPanelGeneralStartup.AutoScroll = ((bool)(resources.GetObject("sectionPanelGeneralStartup.AutoScroll")));
			this.sectionPanelGeneralStartup.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelGeneralStartup.AutoScrollMargin")));
			this.sectionPanelGeneralStartup.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelGeneralStartup.AutoScrollMinSize")));
			this.sectionPanelGeneralStartup.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelGeneralStartup.BackgroundImage")));
			this.sectionPanelGeneralStartup.Controls.Add(this.checkRefreshFeedsOnStartup);
			this.sectionPanelGeneralStartup.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelGeneralStartup.Dock")));
			this.sectionPanelGeneralStartup.Enabled = ((bool)(resources.GetObject("sectionPanelGeneralStartup.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelGeneralStartup, resources.GetString("sectionPanelGeneralStartup.Error"));
			this.sectionPanelGeneralStartup.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelGeneralStartup.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelGeneralStartup, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelGeneralStartup.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelGeneralStartup, ((int)(resources.GetObject("sectionPanelGeneralStartup.IconPadding"))));
			this.sectionPanelGeneralStartup.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelGeneralStartup.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelGeneralStartup.ImeMode")));
			this.sectionPanelGeneralStartup.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelGeneralStartup.Location")));
			this.sectionPanelGeneralStartup.Name = "sectionPanelGeneralStartup";
			this.sectionPanelGeneralStartup.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelGeneralStartup.RightToLeft")));
			this.sectionPanelGeneralStartup.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelGeneralStartup.Size")));
			this.sectionPanelGeneralStartup.TabIndex = ((int)(resources.GetObject("sectionPanelGeneralStartup.TabIndex")));
			this.sectionPanelGeneralStartup.Text = resources.GetString("sectionPanelGeneralStartup.Text");
			this.toolTip1.SetToolTip(this.sectionPanelGeneralStartup, resources.GetString("sectionPanelGeneralStartup.ToolTip"));
			this.sectionPanelGeneralStartup.Visible = ((bool)(resources.GetObject("sectionPanelGeneralStartup.Visible")));
			// 
			// checkRefreshFeedsOnStartup
			// 
			this.checkRefreshFeedsOnStartup.AccessibleDescription = resources.GetString("checkRefreshFeedsOnStartup.AccessibleDescription");
			this.checkRefreshFeedsOnStartup.AccessibleName = resources.GetString("checkRefreshFeedsOnStartup.AccessibleName");
			this.checkRefreshFeedsOnStartup.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkRefreshFeedsOnStartup.Anchor")));
			this.checkRefreshFeedsOnStartup.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkRefreshFeedsOnStartup.Appearance")));
			this.checkRefreshFeedsOnStartup.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkRefreshFeedsOnStartup.BackgroundImage")));
			this.checkRefreshFeedsOnStartup.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkRefreshFeedsOnStartup.CheckAlign")));
			this.checkRefreshFeedsOnStartup.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkRefreshFeedsOnStartup.Dock")));
			this.checkRefreshFeedsOnStartup.Enabled = ((bool)(resources.GetObject("checkRefreshFeedsOnStartup.Enabled")));
			this.errorProvider1.SetError(this.checkRefreshFeedsOnStartup, resources.GetString("checkRefreshFeedsOnStartup.Error"));
			this.checkRefreshFeedsOnStartup.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkRefreshFeedsOnStartup.FlatStyle")));
			this.checkRefreshFeedsOnStartup.Font = ((System.Drawing.Font)(resources.GetObject("checkRefreshFeedsOnStartup.Font")));
			this.errorProvider1.SetIconAlignment(this.checkRefreshFeedsOnStartup, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkRefreshFeedsOnStartup.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkRefreshFeedsOnStartup, ((int)(resources.GetObject("checkRefreshFeedsOnStartup.IconPadding"))));
			this.checkRefreshFeedsOnStartup.Image = ((System.Drawing.Image)(resources.GetObject("checkRefreshFeedsOnStartup.Image")));
			this.checkRefreshFeedsOnStartup.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkRefreshFeedsOnStartup.ImageAlign")));
			this.checkRefreshFeedsOnStartup.ImageIndex = ((int)(resources.GetObject("checkRefreshFeedsOnStartup.ImageIndex")));
			this.checkRefreshFeedsOnStartup.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkRefreshFeedsOnStartup.ImeMode")));
			this.checkRefreshFeedsOnStartup.Location = ((System.Drawing.Point)(resources.GetObject("checkRefreshFeedsOnStartup.Location")));
			this.checkRefreshFeedsOnStartup.Name = "checkRefreshFeedsOnStartup";
			this.checkRefreshFeedsOnStartup.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkRefreshFeedsOnStartup.RightToLeft")));
			this.checkRefreshFeedsOnStartup.Size = ((System.Drawing.Size)(resources.GetObject("checkRefreshFeedsOnStartup.Size")));
			this.checkRefreshFeedsOnStartup.TabIndex = ((int)(resources.GetObject("checkRefreshFeedsOnStartup.TabIndex")));
			this.checkRefreshFeedsOnStartup.Text = resources.GetString("checkRefreshFeedsOnStartup.Text");
			this.checkRefreshFeedsOnStartup.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkRefreshFeedsOnStartup.TextAlign")));
			this.toolTip1.SetToolTip(this.checkRefreshFeedsOnStartup, resources.GetString("checkRefreshFeedsOnStartup.ToolTip"));
			this.checkRefreshFeedsOnStartup.Visible = ((bool)(resources.GetObject("checkRefreshFeedsOnStartup.Visible")));
			this.checkRefreshFeedsOnStartup.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkRefreshFeedsOnStartup.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// sectionPanelGeneralBehavior
			// 
			this.sectionPanelGeneralBehavior.AccessibleDescription = resources.GetString("sectionPanelGeneralBehavior.AccessibleDescription");
			this.sectionPanelGeneralBehavior.AccessibleName = resources.GetString("sectionPanelGeneralBehavior.AccessibleName");
			this.sectionPanelGeneralBehavior.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelGeneralBehavior.Anchor")));
			this.sectionPanelGeneralBehavior.AutoScroll = ((bool)(resources.GetObject("sectionPanelGeneralBehavior.AutoScroll")));
			this.sectionPanelGeneralBehavior.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelGeneralBehavior.AutoScrollMargin")));
			this.sectionPanelGeneralBehavior.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelGeneralBehavior.AutoScrollMinSize")));
			this.sectionPanelGeneralBehavior.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelGeneralBehavior.BackgroundImage")));
			this.sectionPanelGeneralBehavior.Controls.Add(this.btnMakeDefaultAggregator);
			this.sectionPanelGeneralBehavior.Controls.Add(this.label12);
			this.sectionPanelGeneralBehavior.Controls.Add(this.radioTrayActionNone);
			this.sectionPanelGeneralBehavior.Controls.Add(this.label1);
			this.sectionPanelGeneralBehavior.Controls.Add(this.radioTrayActionClose);
			this.sectionPanelGeneralBehavior.Controls.Add(this.radioTrayActionMinimize);
			this.sectionPanelGeneralBehavior.Controls.Add(this.labelCheckForUpdates);
			this.sectionPanelGeneralBehavior.Controls.Add(this.comboAppUpdateFrequency);
			this.sectionPanelGeneralBehavior.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelGeneralBehavior.Dock")));
			this.sectionPanelGeneralBehavior.Enabled = ((bool)(resources.GetObject("sectionPanelGeneralBehavior.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelGeneralBehavior, resources.GetString("sectionPanelGeneralBehavior.Error"));
			this.sectionPanelGeneralBehavior.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelGeneralBehavior.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelGeneralBehavior, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelGeneralBehavior.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelGeneralBehavior, ((int)(resources.GetObject("sectionPanelGeneralBehavior.IconPadding"))));
			this.sectionPanelGeneralBehavior.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelGeneralBehavior.Image")));
			this.sectionPanelGeneralBehavior.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelGeneralBehavior.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelGeneralBehavior.ImeMode")));
			this.sectionPanelGeneralBehavior.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelGeneralBehavior.Location")));
			this.sectionPanelGeneralBehavior.Name = "sectionPanelGeneralBehavior";
			this.sectionPanelGeneralBehavior.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelGeneralBehavior.RightToLeft")));
			this.sectionPanelGeneralBehavior.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelGeneralBehavior.Size")));
			this.sectionPanelGeneralBehavior.TabIndex = ((int)(resources.GetObject("sectionPanelGeneralBehavior.TabIndex")));
			this.sectionPanelGeneralBehavior.Text = resources.GetString("sectionPanelGeneralBehavior.Text");
			this.toolTip1.SetToolTip(this.sectionPanelGeneralBehavior, resources.GetString("sectionPanelGeneralBehavior.ToolTip"));
			this.sectionPanelGeneralBehavior.Visible = ((bool)(resources.GetObject("sectionPanelGeneralBehavior.Visible")));
			// 
			// btnMakeDefaultAggregator
			// 
			this.btnMakeDefaultAggregator.AccessibleDescription = resources.GetString("btnMakeDefaultAggregator.AccessibleDescription");
			this.btnMakeDefaultAggregator.AccessibleName = resources.GetString("btnMakeDefaultAggregator.AccessibleName");
			this.btnMakeDefaultAggregator.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnMakeDefaultAggregator.Anchor")));
			this.btnMakeDefaultAggregator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnMakeDefaultAggregator.BackgroundImage")));
			this.btnMakeDefaultAggregator.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnMakeDefaultAggregator.Dock")));
			this.btnMakeDefaultAggregator.Enabled = ((bool)(resources.GetObject("btnMakeDefaultAggregator.Enabled")));
			this.errorProvider1.SetError(this.btnMakeDefaultAggregator, resources.GetString("btnMakeDefaultAggregator.Error"));
			this.btnMakeDefaultAggregator.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnMakeDefaultAggregator.FlatStyle")));
			this.btnMakeDefaultAggregator.Font = ((System.Drawing.Font)(resources.GetObject("btnMakeDefaultAggregator.Font")));
			this.errorProvider1.SetIconAlignment(this.btnMakeDefaultAggregator, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnMakeDefaultAggregator.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnMakeDefaultAggregator, ((int)(resources.GetObject("btnMakeDefaultAggregator.IconPadding"))));
			this.btnMakeDefaultAggregator.Image = ((System.Drawing.Image)(resources.GetObject("btnMakeDefaultAggregator.Image")));
			this.btnMakeDefaultAggregator.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnMakeDefaultAggregator.ImageAlign")));
			this.btnMakeDefaultAggregator.ImageIndex = ((int)(resources.GetObject("btnMakeDefaultAggregator.ImageIndex")));
			this.btnMakeDefaultAggregator.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnMakeDefaultAggregator.ImeMode")));
			this.btnMakeDefaultAggregator.Location = ((System.Drawing.Point)(resources.GetObject("btnMakeDefaultAggregator.Location")));
			this.btnMakeDefaultAggregator.Name = "btnMakeDefaultAggregator";
			this.btnMakeDefaultAggregator.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnMakeDefaultAggregator.RightToLeft")));
			this.btnMakeDefaultAggregator.Size = ((System.Drawing.Size)(resources.GetObject("btnMakeDefaultAggregator.Size")));
			this.btnMakeDefaultAggregator.TabIndex = ((int)(resources.GetObject("btnMakeDefaultAggregator.TabIndex")));
			this.btnMakeDefaultAggregator.Text = resources.GetString("btnMakeDefaultAggregator.Text");
			this.btnMakeDefaultAggregator.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnMakeDefaultAggregator.TextAlign")));
			this.toolTip1.SetToolTip(this.btnMakeDefaultAggregator, resources.GetString("btnMakeDefaultAggregator.ToolTip"));
			this.btnMakeDefaultAggregator.Visible = ((bool)(resources.GetObject("btnMakeDefaultAggregator.Visible")));
			this.btnMakeDefaultAggregator.Click += new System.EventHandler(this.btnMakeDefaultAggregator_Click);
			// 
			// label12
			// 
			this.label12.AccessibleDescription = resources.GetString("label12.AccessibleDescription");
			this.label12.AccessibleName = resources.GetString("label12.AccessibleName");
			this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label12.Anchor")));
			this.label12.AutoSize = ((bool)(resources.GetObject("label12.AutoSize")));
			this.label12.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label12.Dock")));
			this.label12.Enabled = ((bool)(resources.GetObject("label12.Enabled")));
			this.errorProvider1.SetError(this.label12, resources.GetString("label12.Error"));
			this.label12.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label12.Font = ((System.Drawing.Font)(resources.GetObject("label12.Font")));
			this.errorProvider1.SetIconAlignment(this.label12, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label12.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label12, ((int)(resources.GetObject("label12.IconPadding"))));
			this.label12.Image = ((System.Drawing.Image)(resources.GetObject("label12.Image")));
			this.label12.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label12.ImageAlign")));
			this.label12.ImageIndex = ((int)(resources.GetObject("label12.ImageIndex")));
			this.label12.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label12.ImeMode")));
			this.label12.Location = ((System.Drawing.Point)(resources.GetObject("label12.Location")));
			this.label12.Name = "label12";
			this.label12.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label12.RightToLeft")));
			this.label12.Size = ((System.Drawing.Size)(resources.GetObject("label12.Size")));
			this.label12.TabIndex = ((int)(resources.GetObject("label12.TabIndex")));
			this.label12.Text = resources.GetString("label12.Text");
			this.label12.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label12.TextAlign")));
			this.toolTip1.SetToolTip(this.label12, resources.GetString("label12.ToolTip"));
			this.label12.Visible = ((bool)(resources.GetObject("label12.Visible")));
			// 
			// radioTrayActionNone
			// 
			this.radioTrayActionNone.AccessibleDescription = resources.GetString("radioTrayActionNone.AccessibleDescription");
			this.radioTrayActionNone.AccessibleName = resources.GetString("radioTrayActionNone.AccessibleName");
			this.radioTrayActionNone.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioTrayActionNone.Anchor")));
			this.radioTrayActionNone.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioTrayActionNone.Appearance")));
			this.radioTrayActionNone.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioTrayActionNone.BackgroundImage")));
			this.radioTrayActionNone.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionNone.CheckAlign")));
			this.radioTrayActionNone.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioTrayActionNone.Dock")));
			this.radioTrayActionNone.Enabled = ((bool)(resources.GetObject("radioTrayActionNone.Enabled")));
			this.errorProvider1.SetError(this.radioTrayActionNone, resources.GetString("radioTrayActionNone.Error"));
			this.radioTrayActionNone.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioTrayActionNone.FlatStyle")));
			this.radioTrayActionNone.Font = ((System.Drawing.Font)(resources.GetObject("radioTrayActionNone.Font")));
			this.errorProvider1.SetIconAlignment(this.radioTrayActionNone, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("radioTrayActionNone.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.radioTrayActionNone, ((int)(resources.GetObject("radioTrayActionNone.IconPadding"))));
			this.radioTrayActionNone.Image = ((System.Drawing.Image)(resources.GetObject("radioTrayActionNone.Image")));
			this.radioTrayActionNone.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionNone.ImageAlign")));
			this.radioTrayActionNone.ImageIndex = ((int)(resources.GetObject("radioTrayActionNone.ImageIndex")));
			this.radioTrayActionNone.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioTrayActionNone.ImeMode")));
			this.radioTrayActionNone.Location = ((System.Drawing.Point)(resources.GetObject("radioTrayActionNone.Location")));
			this.radioTrayActionNone.Name = "radioTrayActionNone";
			this.radioTrayActionNone.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioTrayActionNone.RightToLeft")));
			this.radioTrayActionNone.Size = ((System.Drawing.Size)(resources.GetObject("radioTrayActionNone.Size")));
			this.radioTrayActionNone.TabIndex = ((int)(resources.GetObject("radioTrayActionNone.TabIndex")));
			this.radioTrayActionNone.Text = resources.GetString("radioTrayActionNone.Text");
			this.radioTrayActionNone.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionNone.TextAlign")));
			this.toolTip1.SetToolTip(this.radioTrayActionNone, resources.GetString("radioTrayActionNone.ToolTip"));
			this.radioTrayActionNone.Visible = ((bool)(resources.GetObject("radioTrayActionNone.Visible")));
			this.radioTrayActionNone.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.radioTrayActionNone.Validated += new System.EventHandler(this.OnControlValidated);
			this.radioTrayActionNone.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.errorProvider1.SetError(this.label1, resources.GetString("label1.Error"));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.errorProvider1.SetIconAlignment(this.label1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label1.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label1, ((int)(resources.GetObject("label1.IconPadding"))));
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
			// radioTrayActionClose
			// 
			this.radioTrayActionClose.AccessibleDescription = resources.GetString("radioTrayActionClose.AccessibleDescription");
			this.radioTrayActionClose.AccessibleName = resources.GetString("radioTrayActionClose.AccessibleName");
			this.radioTrayActionClose.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioTrayActionClose.Anchor")));
			this.radioTrayActionClose.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioTrayActionClose.Appearance")));
			this.radioTrayActionClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioTrayActionClose.BackgroundImage")));
			this.radioTrayActionClose.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionClose.CheckAlign")));
			this.radioTrayActionClose.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioTrayActionClose.Dock")));
			this.radioTrayActionClose.Enabled = ((bool)(resources.GetObject("radioTrayActionClose.Enabled")));
			this.errorProvider1.SetError(this.radioTrayActionClose, resources.GetString("radioTrayActionClose.Error"));
			this.radioTrayActionClose.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioTrayActionClose.FlatStyle")));
			this.radioTrayActionClose.Font = ((System.Drawing.Font)(resources.GetObject("radioTrayActionClose.Font")));
			this.errorProvider1.SetIconAlignment(this.radioTrayActionClose, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("radioTrayActionClose.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.radioTrayActionClose, ((int)(resources.GetObject("radioTrayActionClose.IconPadding"))));
			this.radioTrayActionClose.Image = ((System.Drawing.Image)(resources.GetObject("radioTrayActionClose.Image")));
			this.radioTrayActionClose.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionClose.ImageAlign")));
			this.radioTrayActionClose.ImageIndex = ((int)(resources.GetObject("radioTrayActionClose.ImageIndex")));
			this.radioTrayActionClose.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioTrayActionClose.ImeMode")));
			this.radioTrayActionClose.Location = ((System.Drawing.Point)(resources.GetObject("radioTrayActionClose.Location")));
			this.radioTrayActionClose.Name = "radioTrayActionClose";
			this.radioTrayActionClose.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioTrayActionClose.RightToLeft")));
			this.radioTrayActionClose.Size = ((System.Drawing.Size)(resources.GetObject("radioTrayActionClose.Size")));
			this.radioTrayActionClose.TabIndex = ((int)(resources.GetObject("radioTrayActionClose.TabIndex")));
			this.radioTrayActionClose.Text = resources.GetString("radioTrayActionClose.Text");
			this.radioTrayActionClose.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionClose.TextAlign")));
			this.toolTip1.SetToolTip(this.radioTrayActionClose, resources.GetString("radioTrayActionClose.ToolTip"));
			this.radioTrayActionClose.Visible = ((bool)(resources.GetObject("radioTrayActionClose.Visible")));
			this.radioTrayActionClose.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.radioTrayActionClose.Validated += new System.EventHandler(this.OnControlValidated);
			this.radioTrayActionClose.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// radioTrayActionMinimize
			// 
			this.radioTrayActionMinimize.AccessibleDescription = resources.GetString("radioTrayActionMinimize.AccessibleDescription");
			this.radioTrayActionMinimize.AccessibleName = resources.GetString("radioTrayActionMinimize.AccessibleName");
			this.radioTrayActionMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioTrayActionMinimize.Anchor")));
			this.radioTrayActionMinimize.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioTrayActionMinimize.Appearance")));
			this.radioTrayActionMinimize.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioTrayActionMinimize.BackgroundImage")));
			this.radioTrayActionMinimize.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionMinimize.CheckAlign")));
			this.radioTrayActionMinimize.Checked = true;
			this.radioTrayActionMinimize.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioTrayActionMinimize.Dock")));
			this.radioTrayActionMinimize.Enabled = ((bool)(resources.GetObject("radioTrayActionMinimize.Enabled")));
			this.errorProvider1.SetError(this.radioTrayActionMinimize, resources.GetString("radioTrayActionMinimize.Error"));
			this.radioTrayActionMinimize.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioTrayActionMinimize.FlatStyle")));
			this.radioTrayActionMinimize.Font = ((System.Drawing.Font)(resources.GetObject("radioTrayActionMinimize.Font")));
			this.errorProvider1.SetIconAlignment(this.radioTrayActionMinimize, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("radioTrayActionMinimize.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.radioTrayActionMinimize, ((int)(resources.GetObject("radioTrayActionMinimize.IconPadding"))));
			this.radioTrayActionMinimize.Image = ((System.Drawing.Image)(resources.GetObject("radioTrayActionMinimize.Image")));
			this.radioTrayActionMinimize.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionMinimize.ImageAlign")));
			this.radioTrayActionMinimize.ImageIndex = ((int)(resources.GetObject("radioTrayActionMinimize.ImageIndex")));
			this.radioTrayActionMinimize.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioTrayActionMinimize.ImeMode")));
			this.radioTrayActionMinimize.Location = ((System.Drawing.Point)(resources.GetObject("radioTrayActionMinimize.Location")));
			this.radioTrayActionMinimize.Name = "radioTrayActionMinimize";
			this.radioTrayActionMinimize.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioTrayActionMinimize.RightToLeft")));
			this.radioTrayActionMinimize.Size = ((System.Drawing.Size)(resources.GetObject("radioTrayActionMinimize.Size")));
			this.radioTrayActionMinimize.TabIndex = ((int)(resources.GetObject("radioTrayActionMinimize.TabIndex")));
			this.radioTrayActionMinimize.TabStop = true;
			this.radioTrayActionMinimize.Text = resources.GetString("radioTrayActionMinimize.Text");
			this.radioTrayActionMinimize.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioTrayActionMinimize.TextAlign")));
			this.toolTip1.SetToolTip(this.radioTrayActionMinimize, resources.GetString("radioTrayActionMinimize.ToolTip"));
			this.radioTrayActionMinimize.Visible = ((bool)(resources.GetObject("radioTrayActionMinimize.Visible")));
			this.radioTrayActionMinimize.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.radioTrayActionMinimize.Validated += new System.EventHandler(this.OnControlValidated);
			this.radioTrayActionMinimize.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// labelCheckForUpdates
			// 
			this.labelCheckForUpdates.AccessibleDescription = resources.GetString("labelCheckForUpdates.AccessibleDescription");
			this.labelCheckForUpdates.AccessibleName = resources.GetString("labelCheckForUpdates.AccessibleName");
			this.labelCheckForUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelCheckForUpdates.Anchor")));
			this.labelCheckForUpdates.AutoSize = ((bool)(resources.GetObject("labelCheckForUpdates.AutoSize")));
			this.labelCheckForUpdates.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelCheckForUpdates.Dock")));
			this.labelCheckForUpdates.Enabled = ((bool)(resources.GetObject("labelCheckForUpdates.Enabled")));
			this.errorProvider1.SetError(this.labelCheckForUpdates, resources.GetString("labelCheckForUpdates.Error"));
			this.labelCheckForUpdates.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelCheckForUpdates.Font = ((System.Drawing.Font)(resources.GetObject("labelCheckForUpdates.Font")));
			this.errorProvider1.SetIconAlignment(this.labelCheckForUpdates, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelCheckForUpdates.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelCheckForUpdates, ((int)(resources.GetObject("labelCheckForUpdates.IconPadding"))));
			this.labelCheckForUpdates.Image = ((System.Drawing.Image)(resources.GetObject("labelCheckForUpdates.Image")));
			this.labelCheckForUpdates.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCheckForUpdates.ImageAlign")));
			this.labelCheckForUpdates.ImageIndex = ((int)(resources.GetObject("labelCheckForUpdates.ImageIndex")));
			this.labelCheckForUpdates.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelCheckForUpdates.ImeMode")));
			this.labelCheckForUpdates.Location = ((System.Drawing.Point)(resources.GetObject("labelCheckForUpdates.Location")));
			this.labelCheckForUpdates.Name = "labelCheckForUpdates";
			this.labelCheckForUpdates.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelCheckForUpdates.RightToLeft")));
			this.labelCheckForUpdates.Size = ((System.Drawing.Size)(resources.GetObject("labelCheckForUpdates.Size")));
			this.labelCheckForUpdates.TabIndex = ((int)(resources.GetObject("labelCheckForUpdates.TabIndex")));
			this.labelCheckForUpdates.Text = resources.GetString("labelCheckForUpdates.Text");
			this.labelCheckForUpdates.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCheckForUpdates.TextAlign")));
			this.toolTip1.SetToolTip(this.labelCheckForUpdates, resources.GetString("labelCheckForUpdates.ToolTip"));
			this.labelCheckForUpdates.Visible = ((bool)(resources.GetObject("labelCheckForUpdates.Visible")));
			// 
			// comboAppUpdateFrequency
			// 
			this.comboAppUpdateFrequency.AccessibleDescription = resources.GetString("comboAppUpdateFrequency.AccessibleDescription");
			this.comboAppUpdateFrequency.AccessibleName = resources.GetString("comboAppUpdateFrequency.AccessibleName");
			this.comboAppUpdateFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboAppUpdateFrequency.Anchor")));
			this.comboAppUpdateFrequency.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboAppUpdateFrequency.BackgroundImage")));
			this.comboAppUpdateFrequency.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboAppUpdateFrequency.Dock")));
			this.comboAppUpdateFrequency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboAppUpdateFrequency.Enabled = ((bool)(resources.GetObject("comboAppUpdateFrequency.Enabled")));
			this.errorProvider1.SetError(this.comboAppUpdateFrequency, resources.GetString("comboAppUpdateFrequency.Error"));
			this.comboAppUpdateFrequency.Font = ((System.Drawing.Font)(resources.GetObject("comboAppUpdateFrequency.Font")));
			this.errorProvider1.SetIconAlignment(this.comboAppUpdateFrequency, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboAppUpdateFrequency.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.comboAppUpdateFrequency, ((int)(resources.GetObject("comboAppUpdateFrequency.IconPadding"))));
			this.comboAppUpdateFrequency.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboAppUpdateFrequency.ImeMode")));
			this.comboAppUpdateFrequency.IntegralHeight = ((bool)(resources.GetObject("comboAppUpdateFrequency.IntegralHeight")));
			this.comboAppUpdateFrequency.ItemHeight = ((int)(resources.GetObject("comboAppUpdateFrequency.ItemHeight")));
			this.comboAppUpdateFrequency.Items.AddRange(new object[] {
																		 resources.GetString("comboAppUpdateFrequency.Items"),
																		 resources.GetString("comboAppUpdateFrequency.Items1"),
																		 resources.GetString("comboAppUpdateFrequency.Items2"),
																		 resources.GetString("comboAppUpdateFrequency.Items3")});
			this.comboAppUpdateFrequency.Location = ((System.Drawing.Point)(resources.GetObject("comboAppUpdateFrequency.Location")));
			this.comboAppUpdateFrequency.MaxDropDownItems = ((int)(resources.GetObject("comboAppUpdateFrequency.MaxDropDownItems")));
			this.comboAppUpdateFrequency.MaxLength = ((int)(resources.GetObject("comboAppUpdateFrequency.MaxLength")));
			this.comboAppUpdateFrequency.Name = "comboAppUpdateFrequency";
			this.comboAppUpdateFrequency.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboAppUpdateFrequency.RightToLeft")));
			this.comboAppUpdateFrequency.Size = ((System.Drawing.Size)(resources.GetObject("comboAppUpdateFrequency.Size")));
			this.comboAppUpdateFrequency.TabIndex = ((int)(resources.GetObject("comboAppUpdateFrequency.TabIndex")));
			this.comboAppUpdateFrequency.Text = resources.GetString("comboAppUpdateFrequency.Text");
			this.toolTip1.SetToolTip(this.comboAppUpdateFrequency, resources.GetString("comboAppUpdateFrequency.ToolTip"));
			this.comboAppUpdateFrequency.Visible = ((bool)(resources.GetObject("comboAppUpdateFrequency.Visible")));
			this.comboAppUpdateFrequency.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.comboAppUpdateFrequency.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// tabRemoteStorage
			// 
			this.tabRemoteStorage.AccessibleDescription = resources.GetString("tabRemoteStorage.AccessibleDescription");
			this.tabRemoteStorage.AccessibleName = resources.GetString("tabRemoteStorage.AccessibleName");
			this.tabRemoteStorage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabRemoteStorage.Anchor")));
			this.tabRemoteStorage.AutoScroll = ((bool)(resources.GetObject("tabRemoteStorage.AutoScroll")));
			this.tabRemoteStorage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabRemoteStorage.AutoScrollMargin")));
			this.tabRemoteStorage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabRemoteStorage.AutoScrollMinSize")));
			this.tabRemoteStorage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabRemoteStorage.BackgroundImage")));
			this.tabRemoteStorage.Controls.Add(this.sectionPanelRemoteStorageFeedlist);
			this.tabRemoteStorage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabRemoteStorage.Dock")));
			this.tabRemoteStorage.Enabled = ((bool)(resources.GetObject("tabRemoteStorage.Enabled")));
			this.errorProvider1.SetError(this.tabRemoteStorage, resources.GetString("tabRemoteStorage.Error"));
			this.tabRemoteStorage.Font = ((System.Drawing.Font)(resources.GetObject("tabRemoteStorage.Font")));
			this.errorProvider1.SetIconAlignment(this.tabRemoteStorage, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabRemoteStorage.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabRemoteStorage, ((int)(resources.GetObject("tabRemoteStorage.IconPadding"))));
			this.tabRemoteStorage.ImageIndex = ((int)(resources.GetObject("tabRemoteStorage.ImageIndex")));
			this.tabRemoteStorage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabRemoteStorage.ImeMode")));
			this.tabRemoteStorage.Location = ((System.Drawing.Point)(resources.GetObject("tabRemoteStorage.Location")));
			this.tabRemoteStorage.Name = "tabRemoteStorage";
			this.tabRemoteStorage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabRemoteStorage.RightToLeft")));
			this.tabRemoteStorage.Size = ((System.Drawing.Size)(resources.GetObject("tabRemoteStorage.Size")));
			this.tabRemoteStorage.TabIndex = ((int)(resources.GetObject("tabRemoteStorage.TabIndex")));
			this.tabRemoteStorage.Text = resources.GetString("tabRemoteStorage.Text");
			this.toolTip1.SetToolTip(this.tabRemoteStorage, resources.GetString("tabRemoteStorage.ToolTip"));
			this.tabRemoteStorage.ToolTipText = resources.GetString("tabRemoteStorage.ToolTipText");
			this.tabRemoteStorage.Visible = ((bool)(resources.GetObject("tabRemoteStorage.Visible")));
			// 
			// sectionPanelRemoteStorageFeedlist
			// 
			this.sectionPanelRemoteStorageFeedlist.AccessibleDescription = resources.GetString("sectionPanelRemoteStorageFeedlist.AccessibleDescription");
			this.sectionPanelRemoteStorageFeedlist.AccessibleName = resources.GetString("sectionPanelRemoteStorageFeedlist.AccessibleName");
			this.sectionPanelRemoteStorageFeedlist.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Anchor")));
			this.sectionPanelRemoteStorageFeedlist.AutoScroll = ((bool)(resources.GetObject("sectionPanelRemoteStorageFeedlist.AutoScroll")));
			this.sectionPanelRemoteStorageFeedlist.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelRemoteStorageFeedlist.AutoScrollMargin")));
			this.sectionPanelRemoteStorageFeedlist.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelRemoteStorageFeedlist.AutoScrollMinSize")));
			this.sectionPanelRemoteStorageFeedlist.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelRemoteStorageFeedlist.BackgroundImage")));
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
			this.sectionPanelRemoteStorageFeedlist.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Dock")));
			this.sectionPanelRemoteStorageFeedlist.Enabled = ((bool)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelRemoteStorageFeedlist, resources.GetString("sectionPanelRemoteStorageFeedlist.Error"));
			this.sectionPanelRemoteStorageFeedlist.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelRemoteStorageFeedlist, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelRemoteStorageFeedlist.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelRemoteStorageFeedlist, ((int)(resources.GetObject("sectionPanelRemoteStorageFeedlist.IconPadding"))));
			this.sectionPanelRemoteStorageFeedlist.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Image")));
			this.sectionPanelRemoteStorageFeedlist.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelRemoteStorageFeedlist.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelRemoteStorageFeedlist.ImeMode")));
			this.sectionPanelRemoteStorageFeedlist.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Location")));
			this.sectionPanelRemoteStorageFeedlist.Name = "sectionPanelRemoteStorageFeedlist";
			this.sectionPanelRemoteStorageFeedlist.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelRemoteStorageFeedlist.RightToLeft")));
			this.sectionPanelRemoteStorageFeedlist.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Size")));
			this.sectionPanelRemoteStorageFeedlist.TabIndex = ((int)(resources.GetObject("sectionPanelRemoteStorageFeedlist.TabIndex")));
			this.sectionPanelRemoteStorageFeedlist.Text = resources.GetString("sectionPanelRemoteStorageFeedlist.Text");
			this.toolTip1.SetToolTip(this.sectionPanelRemoteStorageFeedlist, resources.GetString("sectionPanelRemoteStorageFeedlist.ToolTip"));
			this.sectionPanelRemoteStorageFeedlist.Visible = ((bool)(resources.GetObject("sectionPanelRemoteStorageFeedlist.Visible")));
			// 
			// textRemoteStorageLocation
			// 
			this.textRemoteStorageLocation.AccessibleDescription = resources.GetString("textRemoteStorageLocation.AccessibleDescription");
			this.textRemoteStorageLocation.AccessibleName = resources.GetString("textRemoteStorageLocation.AccessibleName");
			this.textRemoteStorageLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textRemoteStorageLocation.Anchor")));
			this.textRemoteStorageLocation.AutoSize = ((bool)(resources.GetObject("textRemoteStorageLocation.AutoSize")));
			this.textRemoteStorageLocation.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textRemoteStorageLocation.BackgroundImage")));
			this.textRemoteStorageLocation.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textRemoteStorageLocation.Dock")));
			this.textRemoteStorageLocation.Enabled = ((bool)(resources.GetObject("textRemoteStorageLocation.Enabled")));
			this.errorProvider1.SetError(this.textRemoteStorageLocation, resources.GetString("textRemoteStorageLocation.Error"));
			this.textRemoteStorageLocation.Font = ((System.Drawing.Font)(resources.GetObject("textRemoteStorageLocation.Font")));
			this.errorProvider1.SetIconAlignment(this.textRemoteStorageLocation, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textRemoteStorageLocation.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textRemoteStorageLocation, ((int)(resources.GetObject("textRemoteStorageLocation.IconPadding"))));
			this.textRemoteStorageLocation.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textRemoteStorageLocation.ImeMode")));
			this.textRemoteStorageLocation.Location = ((System.Drawing.Point)(resources.GetObject("textRemoteStorageLocation.Location")));
			this.textRemoteStorageLocation.MaxLength = ((int)(resources.GetObject("textRemoteStorageLocation.MaxLength")));
			this.textRemoteStorageLocation.Multiline = ((bool)(resources.GetObject("textRemoteStorageLocation.Multiline")));
			this.textRemoteStorageLocation.Name = "textRemoteStorageLocation";
			this.textRemoteStorageLocation.PasswordChar = ((char)(resources.GetObject("textRemoteStorageLocation.PasswordChar")));
			this.textRemoteStorageLocation.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textRemoteStorageLocation.RightToLeft")));
			this.textRemoteStorageLocation.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textRemoteStorageLocation.ScrollBars")));
			this.textRemoteStorageLocation.Size = ((System.Drawing.Size)(resources.GetObject("textRemoteStorageLocation.Size")));
			this.textRemoteStorageLocation.TabIndex = ((int)(resources.GetObject("textRemoteStorageLocation.TabIndex")));
			this.textRemoteStorageLocation.Text = resources.GetString("textRemoteStorageLocation.Text");
			this.textRemoteStorageLocation.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textRemoteStorageLocation.TextAlign")));
			this.toolTip1.SetToolTip(this.textRemoteStorageLocation, resources.GetString("textRemoteStorageLocation.ToolTip"));
			this.textRemoteStorageLocation.Visible = ((bool)(resources.GetObject("textRemoteStorageLocation.Visible")));
			this.textRemoteStorageLocation.WordWrap = ((bool)(resources.GetObject("textRemoteStorageLocation.WordWrap")));
			this.textRemoteStorageLocation.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.textRemoteStorageLocation.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// textRemoteStoragePassword
			// 
			this.textRemoteStoragePassword.AccessibleDescription = resources.GetString("textRemoteStoragePassword.AccessibleDescription");
			this.textRemoteStoragePassword.AccessibleName = resources.GetString("textRemoteStoragePassword.AccessibleName");
			this.textRemoteStoragePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textRemoteStoragePassword.Anchor")));
			this.textRemoteStoragePassword.AutoSize = ((bool)(resources.GetObject("textRemoteStoragePassword.AutoSize")));
			this.textRemoteStoragePassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textRemoteStoragePassword.BackgroundImage")));
			this.textRemoteStoragePassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textRemoteStoragePassword.Dock")));
			this.textRemoteStoragePassword.Enabled = ((bool)(resources.GetObject("textRemoteStoragePassword.Enabled")));
			this.errorProvider1.SetError(this.textRemoteStoragePassword, resources.GetString("textRemoteStoragePassword.Error"));
			this.textRemoteStoragePassword.Font = ((System.Drawing.Font)(resources.GetObject("textRemoteStoragePassword.Font")));
			this.errorProvider1.SetIconAlignment(this.textRemoteStoragePassword, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textRemoteStoragePassword.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textRemoteStoragePassword, ((int)(resources.GetObject("textRemoteStoragePassword.IconPadding"))));
			this.textRemoteStoragePassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textRemoteStoragePassword.ImeMode")));
			this.textRemoteStoragePassword.Location = ((System.Drawing.Point)(resources.GetObject("textRemoteStoragePassword.Location")));
			this.textRemoteStoragePassword.MaxLength = ((int)(resources.GetObject("textRemoteStoragePassword.MaxLength")));
			this.textRemoteStoragePassword.Multiline = ((bool)(resources.GetObject("textRemoteStoragePassword.Multiline")));
			this.textRemoteStoragePassword.Name = "textRemoteStoragePassword";
			this.textRemoteStoragePassword.PasswordChar = ((char)(resources.GetObject("textRemoteStoragePassword.PasswordChar")));
			this.textRemoteStoragePassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textRemoteStoragePassword.RightToLeft")));
			this.textRemoteStoragePassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textRemoteStoragePassword.ScrollBars")));
			this.textRemoteStoragePassword.Size = ((System.Drawing.Size)(resources.GetObject("textRemoteStoragePassword.Size")));
			this.textRemoteStoragePassword.TabIndex = ((int)(resources.GetObject("textRemoteStoragePassword.TabIndex")));
			this.textRemoteStoragePassword.Text = resources.GetString("textRemoteStoragePassword.Text");
			this.textRemoteStoragePassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textRemoteStoragePassword.TextAlign")));
			this.toolTip1.SetToolTip(this.textRemoteStoragePassword, resources.GetString("textRemoteStoragePassword.ToolTip"));
			this.textRemoteStoragePassword.Visible = ((bool)(resources.GetObject("textRemoteStoragePassword.Visible")));
			this.textRemoteStoragePassword.WordWrap = ((bool)(resources.GetObject("textRemoteStoragePassword.WordWrap")));
			this.textRemoteStoragePassword.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.textRemoteStoragePassword.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// textRemoteStorageUserName
			// 
			this.textRemoteStorageUserName.AccessibleDescription = resources.GetString("textRemoteStorageUserName.AccessibleDescription");
			this.textRemoteStorageUserName.AccessibleName = resources.GetString("textRemoteStorageUserName.AccessibleName");
			this.textRemoteStorageUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textRemoteStorageUserName.Anchor")));
			this.textRemoteStorageUserName.AutoSize = ((bool)(resources.GetObject("textRemoteStorageUserName.AutoSize")));
			this.textRemoteStorageUserName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textRemoteStorageUserName.BackgroundImage")));
			this.textRemoteStorageUserName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textRemoteStorageUserName.Dock")));
			this.textRemoteStorageUserName.Enabled = ((bool)(resources.GetObject("textRemoteStorageUserName.Enabled")));
			this.errorProvider1.SetError(this.textRemoteStorageUserName, resources.GetString("textRemoteStorageUserName.Error"));
			this.textRemoteStorageUserName.Font = ((System.Drawing.Font)(resources.GetObject("textRemoteStorageUserName.Font")));
			this.errorProvider1.SetIconAlignment(this.textRemoteStorageUserName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textRemoteStorageUserName.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textRemoteStorageUserName, ((int)(resources.GetObject("textRemoteStorageUserName.IconPadding"))));
			this.textRemoteStorageUserName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textRemoteStorageUserName.ImeMode")));
			this.textRemoteStorageUserName.Location = ((System.Drawing.Point)(resources.GetObject("textRemoteStorageUserName.Location")));
			this.textRemoteStorageUserName.MaxLength = ((int)(resources.GetObject("textRemoteStorageUserName.MaxLength")));
			this.textRemoteStorageUserName.Multiline = ((bool)(resources.GetObject("textRemoteStorageUserName.Multiline")));
			this.textRemoteStorageUserName.Name = "textRemoteStorageUserName";
			this.textRemoteStorageUserName.PasswordChar = ((char)(resources.GetObject("textRemoteStorageUserName.PasswordChar")));
			this.textRemoteStorageUserName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textRemoteStorageUserName.RightToLeft")));
			this.textRemoteStorageUserName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textRemoteStorageUserName.ScrollBars")));
			this.textRemoteStorageUserName.Size = ((System.Drawing.Size)(resources.GetObject("textRemoteStorageUserName.Size")));
			this.textRemoteStorageUserName.TabIndex = ((int)(resources.GetObject("textRemoteStorageUserName.TabIndex")));
			this.textRemoteStorageUserName.Text = resources.GetString("textRemoteStorageUserName.Text");
			this.textRemoteStorageUserName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textRemoteStorageUserName.TextAlign")));
			this.toolTip1.SetToolTip(this.textRemoteStorageUserName, resources.GetString("textRemoteStorageUserName.ToolTip"));
			this.textRemoteStorageUserName.Visible = ((bool)(resources.GetObject("textRemoteStorageUserName.Visible")));
			this.textRemoteStorageUserName.WordWrap = ((bool)(resources.GetObject("textRemoteStorageUserName.WordWrap")));
			this.textRemoteStorageUserName.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.textRemoteStorageUserName.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// comboRemoteStorageProtocol
			// 
			this.comboRemoteStorageProtocol.AccessibleDescription = resources.GetString("comboRemoteStorageProtocol.AccessibleDescription");
			this.comboRemoteStorageProtocol.AccessibleName = resources.GetString("comboRemoteStorageProtocol.AccessibleName");
			this.comboRemoteStorageProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboRemoteStorageProtocol.Anchor")));
			this.comboRemoteStorageProtocol.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboRemoteStorageProtocol.BackgroundImage")));
			this.comboRemoteStorageProtocol.CausesValidation = false;
			this.comboRemoteStorageProtocol.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboRemoteStorageProtocol.Dock")));
			this.comboRemoteStorageProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboRemoteStorageProtocol.Enabled = ((bool)(resources.GetObject("comboRemoteStorageProtocol.Enabled")));
			this.errorProvider1.SetError(this.comboRemoteStorageProtocol, resources.GetString("comboRemoteStorageProtocol.Error"));
			this.comboRemoteStorageProtocol.Font = ((System.Drawing.Font)(resources.GetObject("comboRemoteStorageProtocol.Font")));
			this.errorProvider1.SetIconAlignment(this.comboRemoteStorageProtocol, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboRemoteStorageProtocol.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.comboRemoteStorageProtocol, ((int)(resources.GetObject("comboRemoteStorageProtocol.IconPadding"))));
			this.comboRemoteStorageProtocol.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboRemoteStorageProtocol.ImeMode")));
			this.comboRemoteStorageProtocol.IntegralHeight = ((bool)(resources.GetObject("comboRemoteStorageProtocol.IntegralHeight")));
			this.comboRemoteStorageProtocol.ItemHeight = ((int)(resources.GetObject("comboRemoteStorageProtocol.ItemHeight")));
			this.comboRemoteStorageProtocol.Items.AddRange(new object[] {
																			resources.GetString("comboRemoteStorageProtocol.Items"),
																			resources.GetString("comboRemoteStorageProtocol.Items1"),
																			resources.GetString("comboRemoteStorageProtocol.Items2"),
																			resources.GetString("comboRemoteStorageProtocol.Items3"),
																			resources.GetString("comboRemoteStorageProtocol.Items4")});
			this.comboRemoteStorageProtocol.Location = ((System.Drawing.Point)(resources.GetObject("comboRemoteStorageProtocol.Location")));
			this.comboRemoteStorageProtocol.MaxDropDownItems = ((int)(resources.GetObject("comboRemoteStorageProtocol.MaxDropDownItems")));
			this.comboRemoteStorageProtocol.MaxLength = ((int)(resources.GetObject("comboRemoteStorageProtocol.MaxLength")));
			this.comboRemoteStorageProtocol.Name = "comboRemoteStorageProtocol";
			this.comboRemoteStorageProtocol.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboRemoteStorageProtocol.RightToLeft")));
			this.comboRemoteStorageProtocol.Size = ((System.Drawing.Size)(resources.GetObject("comboRemoteStorageProtocol.Size")));
			this.comboRemoteStorageProtocol.TabIndex = ((int)(resources.GetObject("comboRemoteStorageProtocol.TabIndex")));
			this.comboRemoteStorageProtocol.Text = resources.GetString("comboRemoteStorageProtocol.Text");
			this.toolTip1.SetToolTip(this.comboRemoteStorageProtocol, resources.GetString("comboRemoteStorageProtocol.ToolTip"));
			this.comboRemoteStorageProtocol.Visible = ((bool)(resources.GetObject("comboRemoteStorageProtocol.Visible")));
			this.comboRemoteStorageProtocol.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.comboRemoteStorageProtocol.Validated += new System.EventHandler(this.OnControlValidated);
			this.comboRemoteStorageProtocol.SelectedIndexChanged += new System.EventHandler(this.comboRemoteStorageProtocol_SelectedIndexChanged);
			// 
			// checkUseRemoteStorage
			// 
			this.checkUseRemoteStorage.AccessibleDescription = resources.GetString("checkUseRemoteStorage.AccessibleDescription");
			this.checkUseRemoteStorage.AccessibleName = resources.GetString("checkUseRemoteStorage.AccessibleName");
			this.checkUseRemoteStorage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkUseRemoteStorage.Anchor")));
			this.checkUseRemoteStorage.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkUseRemoteStorage.Appearance")));
			this.checkUseRemoteStorage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkUseRemoteStorage.BackgroundImage")));
			this.checkUseRemoteStorage.CausesValidation = false;
			this.checkUseRemoteStorage.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseRemoteStorage.CheckAlign")));
			this.checkUseRemoteStorage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkUseRemoteStorage.Dock")));
			this.checkUseRemoteStorage.Enabled = ((bool)(resources.GetObject("checkUseRemoteStorage.Enabled")));
			this.errorProvider1.SetError(this.checkUseRemoteStorage, resources.GetString("checkUseRemoteStorage.Error"));
			this.checkUseRemoteStorage.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkUseRemoteStorage.FlatStyle")));
			this.checkUseRemoteStorage.Font = ((System.Drawing.Font)(resources.GetObject("checkUseRemoteStorage.Font")));
			this.errorProvider1.SetIconAlignment(this.checkUseRemoteStorage, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkUseRemoteStorage.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkUseRemoteStorage, ((int)(resources.GetObject("checkUseRemoteStorage.IconPadding"))));
			this.checkUseRemoteStorage.Image = ((System.Drawing.Image)(resources.GetObject("checkUseRemoteStorage.Image")));
			this.checkUseRemoteStorage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseRemoteStorage.ImageAlign")));
			this.checkUseRemoteStorage.ImageIndex = ((int)(resources.GetObject("checkUseRemoteStorage.ImageIndex")));
			this.checkUseRemoteStorage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkUseRemoteStorage.ImeMode")));
			this.checkUseRemoteStorage.Location = ((System.Drawing.Point)(resources.GetObject("checkUseRemoteStorage.Location")));
			this.checkUseRemoteStorage.Name = "checkUseRemoteStorage";
			this.checkUseRemoteStorage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkUseRemoteStorage.RightToLeft")));
			this.checkUseRemoteStorage.Size = ((System.Drawing.Size)(resources.GetObject("checkUseRemoteStorage.Size")));
			this.checkUseRemoteStorage.TabIndex = ((int)(resources.GetObject("checkUseRemoteStorage.TabIndex")));
			this.checkUseRemoteStorage.Text = resources.GetString("checkUseRemoteStorage.Text");
			this.checkUseRemoteStorage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseRemoteStorage.TextAlign")));
			this.toolTip1.SetToolTip(this.checkUseRemoteStorage, resources.GetString("checkUseRemoteStorage.ToolTip"));
			this.checkUseRemoteStorage.Visible = ((bool)(resources.GetObject("checkUseRemoteStorage.Visible")));
			this.checkUseRemoteStorage.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkUseRemoteStorage.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkUseRemoteStorage.CheckedChanged += new System.EventHandler(this.checkUseRemoteStorage_CheckedChanged);
			// 
			// label10
			// 
			this.label10.AccessibleDescription = resources.GetString("label10.AccessibleDescription");
			this.label10.AccessibleName = resources.GetString("label10.AccessibleName");
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label10.Anchor")));
			this.label10.AutoSize = ((bool)(resources.GetObject("label10.AutoSize")));
			this.label10.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label10.Dock")));
			this.label10.Enabled = ((bool)(resources.GetObject("label10.Enabled")));
			this.errorProvider1.SetError(this.label10, resources.GetString("label10.Error"));
			this.label10.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label10.Font = ((System.Drawing.Font)(resources.GetObject("label10.Font")));
			this.errorProvider1.SetIconAlignment(this.label10, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label10.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label10, ((int)(resources.GetObject("label10.IconPadding"))));
			this.label10.Image = ((System.Drawing.Image)(resources.GetObject("label10.Image")));
			this.label10.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label10.ImageAlign")));
			this.label10.ImageIndex = ((int)(resources.GetObject("label10.ImageIndex")));
			this.label10.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label10.ImeMode")));
			this.label10.Location = ((System.Drawing.Point)(resources.GetObject("label10.Location")));
			this.label10.Name = "label10";
			this.label10.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label10.RightToLeft")));
			this.label10.Size = ((System.Drawing.Size)(resources.GetObject("label10.Size")));
			this.label10.TabIndex = ((int)(resources.GetObject("label10.TabIndex")));
			this.label10.Text = resources.GetString("label10.Text");
			this.label10.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label10.TextAlign")));
			this.toolTip1.SetToolTip(this.label10, resources.GetString("label10.ToolTip"));
			this.label10.Visible = ((bool)(resources.GetObject("label10.Visible")));
			// 
			// labelRemoteStorageLocation
			// 
			this.labelRemoteStorageLocation.AccessibleDescription = resources.GetString("labelRemoteStorageLocation.AccessibleDescription");
			this.labelRemoteStorageLocation.AccessibleName = resources.GetString("labelRemoteStorageLocation.AccessibleName");
			this.labelRemoteStorageLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelRemoteStorageLocation.Anchor")));
			this.labelRemoteStorageLocation.AutoSize = ((bool)(resources.GetObject("labelRemoteStorageLocation.AutoSize")));
			this.labelRemoteStorageLocation.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelRemoteStorageLocation.Dock")));
			this.labelRemoteStorageLocation.Enabled = ((bool)(resources.GetObject("labelRemoteStorageLocation.Enabled")));
			this.errorProvider1.SetError(this.labelRemoteStorageLocation, resources.GetString("labelRemoteStorageLocation.Error"));
			this.labelRemoteStorageLocation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelRemoteStorageLocation.Font = ((System.Drawing.Font)(resources.GetObject("labelRemoteStorageLocation.Font")));
			this.errorProvider1.SetIconAlignment(this.labelRemoteStorageLocation, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelRemoteStorageLocation.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelRemoteStorageLocation, ((int)(resources.GetObject("labelRemoteStorageLocation.IconPadding"))));
			this.labelRemoteStorageLocation.Image = ((System.Drawing.Image)(resources.GetObject("labelRemoteStorageLocation.Image")));
			this.labelRemoteStorageLocation.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStorageLocation.ImageAlign")));
			this.labelRemoteStorageLocation.ImageIndex = ((int)(resources.GetObject("labelRemoteStorageLocation.ImageIndex")));
			this.labelRemoteStorageLocation.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelRemoteStorageLocation.ImeMode")));
			this.labelRemoteStorageLocation.Location = ((System.Drawing.Point)(resources.GetObject("labelRemoteStorageLocation.Location")));
			this.labelRemoteStorageLocation.Name = "labelRemoteStorageLocation";
			this.labelRemoteStorageLocation.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelRemoteStorageLocation.RightToLeft")));
			this.labelRemoteStorageLocation.Size = ((System.Drawing.Size)(resources.GetObject("labelRemoteStorageLocation.Size")));
			this.labelRemoteStorageLocation.TabIndex = ((int)(resources.GetObject("labelRemoteStorageLocation.TabIndex")));
			this.labelRemoteStorageLocation.Text = resources.GetString("labelRemoteStorageLocation.Text");
			this.labelRemoteStorageLocation.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStorageLocation.TextAlign")));
			this.toolTip1.SetToolTip(this.labelRemoteStorageLocation, resources.GetString("labelRemoteStorageLocation.ToolTip"));
			this.labelRemoteStorageLocation.Visible = ((bool)(resources.GetObject("labelRemoteStorageLocation.Visible")));
			// 
			// labelRemoteStoragePassword
			// 
			this.labelRemoteStoragePassword.AccessibleDescription = resources.GetString("labelRemoteStoragePassword.AccessibleDescription");
			this.labelRemoteStoragePassword.AccessibleName = resources.GetString("labelRemoteStoragePassword.AccessibleName");
			this.labelRemoteStoragePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelRemoteStoragePassword.Anchor")));
			this.labelRemoteStoragePassword.AutoSize = ((bool)(resources.GetObject("labelRemoteStoragePassword.AutoSize")));
			this.labelRemoteStoragePassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelRemoteStoragePassword.Dock")));
			this.labelRemoteStoragePassword.Enabled = ((bool)(resources.GetObject("labelRemoteStoragePassword.Enabled")));
			this.errorProvider1.SetError(this.labelRemoteStoragePassword, resources.GetString("labelRemoteStoragePassword.Error"));
			this.labelRemoteStoragePassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelRemoteStoragePassword.Font = ((System.Drawing.Font)(resources.GetObject("labelRemoteStoragePassword.Font")));
			this.errorProvider1.SetIconAlignment(this.labelRemoteStoragePassword, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelRemoteStoragePassword.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelRemoteStoragePassword, ((int)(resources.GetObject("labelRemoteStoragePassword.IconPadding"))));
			this.labelRemoteStoragePassword.Image = ((System.Drawing.Image)(resources.GetObject("labelRemoteStoragePassword.Image")));
			this.labelRemoteStoragePassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStoragePassword.ImageAlign")));
			this.labelRemoteStoragePassword.ImageIndex = ((int)(resources.GetObject("labelRemoteStoragePassword.ImageIndex")));
			this.labelRemoteStoragePassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelRemoteStoragePassword.ImeMode")));
			this.labelRemoteStoragePassword.Location = ((System.Drawing.Point)(resources.GetObject("labelRemoteStoragePassword.Location")));
			this.labelRemoteStoragePassword.Name = "labelRemoteStoragePassword";
			this.labelRemoteStoragePassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelRemoteStoragePassword.RightToLeft")));
			this.labelRemoteStoragePassword.Size = ((System.Drawing.Size)(resources.GetObject("labelRemoteStoragePassword.Size")));
			this.labelRemoteStoragePassword.TabIndex = ((int)(resources.GetObject("labelRemoteStoragePassword.TabIndex")));
			this.labelRemoteStoragePassword.Text = resources.GetString("labelRemoteStoragePassword.Text");
			this.labelRemoteStoragePassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStoragePassword.TextAlign")));
			this.toolTip1.SetToolTip(this.labelRemoteStoragePassword, resources.GetString("labelRemoteStoragePassword.ToolTip"));
			this.labelRemoteStoragePassword.Visible = ((bool)(resources.GetObject("labelRemoteStoragePassword.Visible")));
			// 
			// labelRemoteStorageUserName
			// 
			this.labelRemoteStorageUserName.AccessibleDescription = resources.GetString("labelRemoteStorageUserName.AccessibleDescription");
			this.labelRemoteStorageUserName.AccessibleName = resources.GetString("labelRemoteStorageUserName.AccessibleName");
			this.labelRemoteStorageUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelRemoteStorageUserName.Anchor")));
			this.labelRemoteStorageUserName.AutoSize = ((bool)(resources.GetObject("labelRemoteStorageUserName.AutoSize")));
			this.labelRemoteStorageUserName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelRemoteStorageUserName.Dock")));
			this.labelRemoteStorageUserName.Enabled = ((bool)(resources.GetObject("labelRemoteStorageUserName.Enabled")));
			this.errorProvider1.SetError(this.labelRemoteStorageUserName, resources.GetString("labelRemoteStorageUserName.Error"));
			this.labelRemoteStorageUserName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelRemoteStorageUserName.Font = ((System.Drawing.Font)(resources.GetObject("labelRemoteStorageUserName.Font")));
			this.errorProvider1.SetIconAlignment(this.labelRemoteStorageUserName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelRemoteStorageUserName.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelRemoteStorageUserName, ((int)(resources.GetObject("labelRemoteStorageUserName.IconPadding"))));
			this.labelRemoteStorageUserName.Image = ((System.Drawing.Image)(resources.GetObject("labelRemoteStorageUserName.Image")));
			this.labelRemoteStorageUserName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStorageUserName.ImageAlign")));
			this.labelRemoteStorageUserName.ImageIndex = ((int)(resources.GetObject("labelRemoteStorageUserName.ImageIndex")));
			this.labelRemoteStorageUserName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelRemoteStorageUserName.ImeMode")));
			this.labelRemoteStorageUserName.Location = ((System.Drawing.Point)(resources.GetObject("labelRemoteStorageUserName.Location")));
			this.labelRemoteStorageUserName.Name = "labelRemoteStorageUserName";
			this.labelRemoteStorageUserName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelRemoteStorageUserName.RightToLeft")));
			this.labelRemoteStorageUserName.Size = ((System.Drawing.Size)(resources.GetObject("labelRemoteStorageUserName.Size")));
			this.labelRemoteStorageUserName.TabIndex = ((int)(resources.GetObject("labelRemoteStorageUserName.TabIndex")));
			this.labelRemoteStorageUserName.Text = resources.GetString("labelRemoteStorageUserName.Text");
			this.labelRemoteStorageUserName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStorageUserName.TextAlign")));
			this.toolTip1.SetToolTip(this.labelRemoteStorageUserName, resources.GetString("labelRemoteStorageUserName.ToolTip"));
			this.labelRemoteStorageUserName.Visible = ((bool)(resources.GetObject("labelRemoteStorageUserName.Visible")));
			// 
			// labelRemoteStorageProtocol
			// 
			this.labelRemoteStorageProtocol.AccessibleDescription = resources.GetString("labelRemoteStorageProtocol.AccessibleDescription");
			this.labelRemoteStorageProtocol.AccessibleName = resources.GetString("labelRemoteStorageProtocol.AccessibleName");
			this.labelRemoteStorageProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelRemoteStorageProtocol.Anchor")));
			this.labelRemoteStorageProtocol.AutoSize = ((bool)(resources.GetObject("labelRemoteStorageProtocol.AutoSize")));
			this.labelRemoteStorageProtocol.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelRemoteStorageProtocol.Dock")));
			this.labelRemoteStorageProtocol.Enabled = ((bool)(resources.GetObject("labelRemoteStorageProtocol.Enabled")));
			this.errorProvider1.SetError(this.labelRemoteStorageProtocol, resources.GetString("labelRemoteStorageProtocol.Error"));
			this.labelRemoteStorageProtocol.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelRemoteStorageProtocol.Font = ((System.Drawing.Font)(resources.GetObject("labelRemoteStorageProtocol.Font")));
			this.errorProvider1.SetIconAlignment(this.labelRemoteStorageProtocol, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelRemoteStorageProtocol.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelRemoteStorageProtocol, ((int)(resources.GetObject("labelRemoteStorageProtocol.IconPadding"))));
			this.labelRemoteStorageProtocol.Image = ((System.Drawing.Image)(resources.GetObject("labelRemoteStorageProtocol.Image")));
			this.labelRemoteStorageProtocol.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStorageProtocol.ImageAlign")));
			this.labelRemoteStorageProtocol.ImageIndex = ((int)(resources.GetObject("labelRemoteStorageProtocol.ImageIndex")));
			this.labelRemoteStorageProtocol.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelRemoteStorageProtocol.ImeMode")));
			this.labelRemoteStorageProtocol.Location = ((System.Drawing.Point)(resources.GetObject("labelRemoteStorageProtocol.Location")));
			this.labelRemoteStorageProtocol.Name = "labelRemoteStorageProtocol";
			this.labelRemoteStorageProtocol.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelRemoteStorageProtocol.RightToLeft")));
			this.labelRemoteStorageProtocol.Size = ((System.Drawing.Size)(resources.GetObject("labelRemoteStorageProtocol.Size")));
			this.labelRemoteStorageProtocol.TabIndex = ((int)(resources.GetObject("labelRemoteStorageProtocol.TabIndex")));
			this.labelRemoteStorageProtocol.Text = resources.GetString("labelRemoteStorageProtocol.Text");
			this.labelRemoteStorageProtocol.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRemoteStorageProtocol.TextAlign")));
			this.toolTip1.SetToolTip(this.labelRemoteStorageProtocol, resources.GetString("labelRemoteStorageProtocol.ToolTip"));
			this.labelRemoteStorageProtocol.Visible = ((bool)(resources.GetObject("labelRemoteStorageProtocol.Visible")));
			// 
			// labelExperimental
			// 
			this.labelExperimental.AccessibleDescription = resources.GetString("labelExperimental.AccessibleDescription");
			this.labelExperimental.AccessibleName = resources.GetString("labelExperimental.AccessibleName");
			this.labelExperimental.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelExperimental.Anchor")));
			this.labelExperimental.AutoSize = ((bool)(resources.GetObject("labelExperimental.AutoSize")));
			this.labelExperimental.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelExperimental.Dock")));
			this.labelExperimental.Enabled = ((bool)(resources.GetObject("labelExperimental.Enabled")));
			this.errorProvider1.SetError(this.labelExperimental, resources.GetString("labelExperimental.Error"));
			this.labelExperimental.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelExperimental.Font = ((System.Drawing.Font)(resources.GetObject("labelExperimental.Font")));
			this.errorProvider1.SetIconAlignment(this.labelExperimental, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelExperimental.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelExperimental, ((int)(resources.GetObject("labelExperimental.IconPadding"))));
			this.labelExperimental.Image = ((System.Drawing.Image)(resources.GetObject("labelExperimental.Image")));
			this.labelExperimental.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelExperimental.ImageAlign")));
			this.labelExperimental.ImageIndex = ((int)(resources.GetObject("labelExperimental.ImageIndex")));
			this.labelExperimental.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelExperimental.ImeMode")));
			this.labelExperimental.Location = ((System.Drawing.Point)(resources.GetObject("labelExperimental.Location")));
			this.labelExperimental.Name = "labelExperimental";
			this.labelExperimental.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelExperimental.RightToLeft")));
			this.labelExperimental.Size = ((System.Drawing.Size)(resources.GetObject("labelExperimental.Size")));
			this.labelExperimental.TabIndex = ((int)(resources.GetObject("labelExperimental.TabIndex")));
			this.labelExperimental.Text = resources.GetString("labelExperimental.Text");
			this.labelExperimental.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelExperimental.TextAlign")));
			this.toolTip1.SetToolTip(this.labelExperimental, resources.GetString("labelExperimental.ToolTip"));
			this.labelExperimental.Visible = ((bool)(resources.GetObject("labelExperimental.Visible")));
			// 
			// tabNewsItems
			// 
			this.tabNewsItems.AccessibleDescription = resources.GetString("tabNewsItems.AccessibleDescription");
			this.tabNewsItems.AccessibleName = resources.GetString("tabNewsItems.AccessibleName");
			this.tabNewsItems.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabNewsItems.Anchor")));
			this.tabNewsItems.AutoScroll = ((bool)(resources.GetObject("tabNewsItems.AutoScroll")));
			this.tabNewsItems.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabNewsItems.AutoScrollMargin")));
			this.tabNewsItems.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabNewsItems.AutoScrollMinSize")));
			this.tabNewsItems.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabNewsItems.BackgroundImage")));
			this.tabNewsItems.Controls.Add(this.sectionPanelDisplayGeneral);
			this.tabNewsItems.Controls.Add(this.sectionPanelDisplayItemFormatting);
			this.tabNewsItems.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabNewsItems.Dock")));
			this.tabNewsItems.Enabled = ((bool)(resources.GetObject("tabNewsItems.Enabled")));
			this.errorProvider1.SetError(this.tabNewsItems, resources.GetString("tabNewsItems.Error"));
			this.tabNewsItems.Font = ((System.Drawing.Font)(resources.GetObject("tabNewsItems.Font")));
			this.errorProvider1.SetIconAlignment(this.tabNewsItems, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabNewsItems.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabNewsItems, ((int)(resources.GetObject("tabNewsItems.IconPadding"))));
			this.tabNewsItems.ImageIndex = ((int)(resources.GetObject("tabNewsItems.ImageIndex")));
			this.tabNewsItems.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabNewsItems.ImeMode")));
			this.tabNewsItems.Location = ((System.Drawing.Point)(resources.GetObject("tabNewsItems.Location")));
			this.tabNewsItems.Name = "tabNewsItems";
			this.tabNewsItems.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabNewsItems.RightToLeft")));
			this.tabNewsItems.Size = ((System.Drawing.Size)(resources.GetObject("tabNewsItems.Size")));
			this.tabNewsItems.TabIndex = ((int)(resources.GetObject("tabNewsItems.TabIndex")));
			this.tabNewsItems.Text = resources.GetString("tabNewsItems.Text");
			this.toolTip1.SetToolTip(this.tabNewsItems, resources.GetString("tabNewsItems.ToolTip"));
			this.tabNewsItems.ToolTipText = resources.GetString("tabNewsItems.ToolTipText");
			this.tabNewsItems.Visible = ((bool)(resources.GetObject("tabNewsItems.Visible")));
			// 
			// sectionPanelDisplayGeneral
			// 
			this.sectionPanelDisplayGeneral.AccessibleDescription = resources.GetString("sectionPanelDisplayGeneral.AccessibleDescription");
			this.sectionPanelDisplayGeneral.AccessibleName = resources.GetString("sectionPanelDisplayGeneral.AccessibleName");
			this.sectionPanelDisplayGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelDisplayGeneral.Anchor")));
			this.sectionPanelDisplayGeneral.AutoScroll = ((bool)(resources.GetObject("sectionPanelDisplayGeneral.AutoScroll")));
			this.sectionPanelDisplayGeneral.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelDisplayGeneral.AutoScrollMargin")));
			this.sectionPanelDisplayGeneral.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelDisplayGeneral.AutoScrollMinSize")));
			this.sectionPanelDisplayGeneral.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelDisplayGeneral.BackgroundImage")));
			this.sectionPanelDisplayGeneral.Controls.Add(this.checkMarkItemsReadOnExit);
			this.sectionPanelDisplayGeneral.Controls.Add(this.chkNewsItemOpenLinkInDetailWindow);
			this.sectionPanelDisplayGeneral.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelDisplayGeneral.Dock")));
			this.sectionPanelDisplayGeneral.Enabled = ((bool)(resources.GetObject("sectionPanelDisplayGeneral.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelDisplayGeneral, resources.GetString("sectionPanelDisplayGeneral.Error"));
			this.sectionPanelDisplayGeneral.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelDisplayGeneral.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelDisplayGeneral, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelDisplayGeneral.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelDisplayGeneral, ((int)(resources.GetObject("sectionPanelDisplayGeneral.IconPadding"))));
			this.sectionPanelDisplayGeneral.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelDisplayGeneral.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelDisplayGeneral.ImeMode")));
			this.sectionPanelDisplayGeneral.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelDisplayGeneral.Location")));
			this.sectionPanelDisplayGeneral.Name = "sectionPanelDisplayGeneral";
			this.sectionPanelDisplayGeneral.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelDisplayGeneral.RightToLeft")));
			this.sectionPanelDisplayGeneral.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelDisplayGeneral.Size")));
			this.sectionPanelDisplayGeneral.TabIndex = ((int)(resources.GetObject("sectionPanelDisplayGeneral.TabIndex")));
			this.sectionPanelDisplayGeneral.Text = resources.GetString("sectionPanelDisplayGeneral.Text");
			this.toolTip1.SetToolTip(this.sectionPanelDisplayGeneral, resources.GetString("sectionPanelDisplayGeneral.ToolTip"));
			this.sectionPanelDisplayGeneral.Visible = ((bool)(resources.GetObject("sectionPanelDisplayGeneral.Visible")));
			// 
			// checkMarkItemsReadOnExit
			// 
			this.checkMarkItemsReadOnExit.AccessibleDescription = resources.GetString("checkMarkItemsReadOnExit.AccessibleDescription");
			this.checkMarkItemsReadOnExit.AccessibleName = resources.GetString("checkMarkItemsReadOnExit.AccessibleName");
			this.checkMarkItemsReadOnExit.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkMarkItemsReadOnExit.Anchor")));
			this.checkMarkItemsReadOnExit.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkMarkItemsReadOnExit.Appearance")));
			this.checkMarkItemsReadOnExit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkMarkItemsReadOnExit.BackgroundImage")));
			this.checkMarkItemsReadOnExit.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkMarkItemsReadOnExit.CheckAlign")));
			this.checkMarkItemsReadOnExit.Checked = true;
			this.checkMarkItemsReadOnExit.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkMarkItemsReadOnExit.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkMarkItemsReadOnExit.Dock")));
			this.checkMarkItemsReadOnExit.Enabled = ((bool)(resources.GetObject("checkMarkItemsReadOnExit.Enabled")));
			this.errorProvider1.SetError(this.checkMarkItemsReadOnExit, resources.GetString("checkMarkItemsReadOnExit.Error"));
			this.checkMarkItemsReadOnExit.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkMarkItemsReadOnExit.FlatStyle")));
			this.checkMarkItemsReadOnExit.Font = ((System.Drawing.Font)(resources.GetObject("checkMarkItemsReadOnExit.Font")));
			this.errorProvider1.SetIconAlignment(this.checkMarkItemsReadOnExit, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkMarkItemsReadOnExit.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkMarkItemsReadOnExit, ((int)(resources.GetObject("checkMarkItemsReadOnExit.IconPadding"))));
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
			this.checkMarkItemsReadOnExit.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// chkNewsItemOpenLinkInDetailWindow
			// 
			this.chkNewsItemOpenLinkInDetailWindow.AccessibleDescription = resources.GetString("chkNewsItemOpenLinkInDetailWindow.AccessibleDescription");
			this.chkNewsItemOpenLinkInDetailWindow.AccessibleName = resources.GetString("chkNewsItemOpenLinkInDetailWindow.AccessibleName");
			this.chkNewsItemOpenLinkInDetailWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Anchor")));
			this.chkNewsItemOpenLinkInDetailWindow.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Appearance")));
			this.chkNewsItemOpenLinkInDetailWindow.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.BackgroundImage")));
			this.chkNewsItemOpenLinkInDetailWindow.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.CheckAlign")));
			this.chkNewsItemOpenLinkInDetailWindow.Checked = true;
			this.chkNewsItemOpenLinkInDetailWindow.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkNewsItemOpenLinkInDetailWindow.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Dock")));
			this.chkNewsItemOpenLinkInDetailWindow.Enabled = ((bool)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Enabled")));
			this.errorProvider1.SetError(this.chkNewsItemOpenLinkInDetailWindow, resources.GetString("chkNewsItemOpenLinkInDetailWindow.Error"));
			this.chkNewsItemOpenLinkInDetailWindow.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.FlatStyle")));
			this.chkNewsItemOpenLinkInDetailWindow.Font = ((System.Drawing.Font)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Font")));
			this.errorProvider1.SetIconAlignment(this.chkNewsItemOpenLinkInDetailWindow, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.chkNewsItemOpenLinkInDetailWindow, ((int)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.IconPadding"))));
			this.chkNewsItemOpenLinkInDetailWindow.Image = ((System.Drawing.Image)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Image")));
			this.chkNewsItemOpenLinkInDetailWindow.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.ImageAlign")));
			this.chkNewsItemOpenLinkInDetailWindow.ImageIndex = ((int)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.ImageIndex")));
			this.chkNewsItemOpenLinkInDetailWindow.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.ImeMode")));
			this.chkNewsItemOpenLinkInDetailWindow.Location = ((System.Drawing.Point)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Location")));
			this.chkNewsItemOpenLinkInDetailWindow.Name = "chkNewsItemOpenLinkInDetailWindow";
			this.chkNewsItemOpenLinkInDetailWindow.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.RightToLeft")));
			this.chkNewsItemOpenLinkInDetailWindow.Size = ((System.Drawing.Size)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Size")));
			this.chkNewsItemOpenLinkInDetailWindow.TabIndex = ((int)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.TabIndex")));
			this.chkNewsItemOpenLinkInDetailWindow.Text = resources.GetString("chkNewsItemOpenLinkInDetailWindow.Text");
			this.chkNewsItemOpenLinkInDetailWindow.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.TextAlign")));
			this.toolTip1.SetToolTip(this.chkNewsItemOpenLinkInDetailWindow, resources.GetString("chkNewsItemOpenLinkInDetailWindow.ToolTip"));
			this.chkNewsItemOpenLinkInDetailWindow.Visible = ((bool)(resources.GetObject("chkNewsItemOpenLinkInDetailWindow.Visible")));
			this.chkNewsItemOpenLinkInDetailWindow.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// sectionPanelDisplayItemFormatting
			// 
			this.sectionPanelDisplayItemFormatting.AccessibleDescription = resources.GetString("sectionPanelDisplayItemFormatting.AccessibleDescription");
			this.sectionPanelDisplayItemFormatting.AccessibleName = resources.GetString("sectionPanelDisplayItemFormatting.AccessibleName");
			this.sectionPanelDisplayItemFormatting.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelDisplayItemFormatting.Anchor")));
			this.sectionPanelDisplayItemFormatting.AutoScroll = ((bool)(resources.GetObject("sectionPanelDisplayItemFormatting.AutoScroll")));
			this.sectionPanelDisplayItemFormatting.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelDisplayItemFormatting.AutoScrollMargin")));
			this.sectionPanelDisplayItemFormatting.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelDisplayItemFormatting.AutoScrollMinSize")));
			this.sectionPanelDisplayItemFormatting.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelDisplayItemFormatting.BackgroundImage")));
			this.sectionPanelDisplayItemFormatting.Controls.Add(this.comboFormatters);
			this.sectionPanelDisplayItemFormatting.Controls.Add(this.label2);
			this.sectionPanelDisplayItemFormatting.Controls.Add(this.checkCustomFormatter);
			this.sectionPanelDisplayItemFormatting.Controls.Add(this.labelFormatters);
			this.sectionPanelDisplayItemFormatting.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelDisplayItemFormatting.Dock")));
			this.sectionPanelDisplayItemFormatting.Enabled = ((bool)(resources.GetObject("sectionPanelDisplayItemFormatting.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelDisplayItemFormatting, resources.GetString("sectionPanelDisplayItemFormatting.Error"));
			this.sectionPanelDisplayItemFormatting.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelDisplayItemFormatting.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelDisplayItemFormatting, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelDisplayItemFormatting.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelDisplayItemFormatting, ((int)(resources.GetObject("sectionPanelDisplayItemFormatting.IconPadding"))));
			this.sectionPanelDisplayItemFormatting.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelDisplayItemFormatting.Image")));
			this.sectionPanelDisplayItemFormatting.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelDisplayItemFormatting.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelDisplayItemFormatting.ImeMode")));
			this.sectionPanelDisplayItemFormatting.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelDisplayItemFormatting.Location")));
			this.sectionPanelDisplayItemFormatting.Name = "sectionPanelDisplayItemFormatting";
			this.sectionPanelDisplayItemFormatting.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelDisplayItemFormatting.RightToLeft")));
			this.sectionPanelDisplayItemFormatting.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelDisplayItemFormatting.Size")));
			this.sectionPanelDisplayItemFormatting.TabIndex = ((int)(resources.GetObject("sectionPanelDisplayItemFormatting.TabIndex")));
			this.sectionPanelDisplayItemFormatting.Text = resources.GetString("sectionPanelDisplayItemFormatting.Text");
			this.toolTip1.SetToolTip(this.sectionPanelDisplayItemFormatting, resources.GetString("sectionPanelDisplayItemFormatting.ToolTip"));
			this.sectionPanelDisplayItemFormatting.Visible = ((bool)(resources.GetObject("sectionPanelDisplayItemFormatting.Visible")));
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
			this.errorProvider1.SetError(this.comboFormatters, resources.GetString("comboFormatters.Error"));
			this.comboFormatters.Font = ((System.Drawing.Font)(resources.GetObject("comboFormatters.Font")));
			this.errorProvider1.SetIconAlignment(this.comboFormatters, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboFormatters.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.comboFormatters, ((int)(resources.GetObject("comboFormatters.IconPadding"))));
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
			this.comboFormatters.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.comboFormatters.Validated += new System.EventHandler(this.OnControlValidated);
			this.comboFormatters.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.errorProvider1.SetError(this.label2, resources.GetString("label2.Error"));
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.errorProvider1.SetIconAlignment(this.label2, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label2.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label2, ((int)(resources.GetObject("label2.IconPadding"))));
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
			this.errorProvider1.SetError(this.checkCustomFormatter, resources.GetString("checkCustomFormatter.Error"));
			this.checkCustomFormatter.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkCustomFormatter.FlatStyle")));
			this.checkCustomFormatter.Font = ((System.Drawing.Font)(resources.GetObject("checkCustomFormatter.Font")));
			this.errorProvider1.SetIconAlignment(this.checkCustomFormatter, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkCustomFormatter.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkCustomFormatter, ((int)(resources.GetObject("checkCustomFormatter.IconPadding"))));
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
			this.checkCustomFormatter.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkCustomFormatter.Validated += new System.EventHandler(this.OnControlValidated);
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
			this.errorProvider1.SetError(this.labelFormatters, resources.GetString("labelFormatters.Error"));
			this.labelFormatters.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelFormatters.Font = ((System.Drawing.Font)(resources.GetObject("labelFormatters.Font")));
			this.errorProvider1.SetIconAlignment(this.labelFormatters, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelFormatters.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelFormatters, ((int)(resources.GetObject("labelFormatters.IconPadding"))));
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
			// tabFeeds
			// 
			this.tabFeeds.AccessibleDescription = resources.GetString("tabFeeds.AccessibleDescription");
			this.tabFeeds.AccessibleName = resources.GetString("tabFeeds.AccessibleName");
			this.tabFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabFeeds.Anchor")));
			this.tabFeeds.AutoScroll = ((bool)(resources.GetObject("tabFeeds.AutoScroll")));
			this.tabFeeds.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabFeeds.AutoScrollMargin")));
			this.tabFeeds.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabFeeds.AutoScrollMinSize")));
			this.tabFeeds.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabFeeds.BackgroundImage")));
			this.tabFeeds.Controls.Add(this.sectionPanelFeedsCommentDefs);
			this.tabFeeds.Controls.Add(this.sectionPanelFeedsTimings);
			this.tabFeeds.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabFeeds.Dock")));
			this.tabFeeds.Enabled = ((bool)(resources.GetObject("tabFeeds.Enabled")));
			this.errorProvider1.SetError(this.tabFeeds, resources.GetString("tabFeeds.Error"));
			this.tabFeeds.Font = ((System.Drawing.Font)(resources.GetObject("tabFeeds.Font")));
			this.errorProvider1.SetIconAlignment(this.tabFeeds, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabFeeds.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabFeeds, ((int)(resources.GetObject("tabFeeds.IconPadding"))));
			this.tabFeeds.ImageIndex = ((int)(resources.GetObject("tabFeeds.ImageIndex")));
			this.tabFeeds.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabFeeds.ImeMode")));
			this.tabFeeds.Location = ((System.Drawing.Point)(resources.GetObject("tabFeeds.Location")));
			this.tabFeeds.Name = "tabFeeds";
			this.tabFeeds.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabFeeds.RightToLeft")));
			this.tabFeeds.Size = ((System.Drawing.Size)(resources.GetObject("tabFeeds.Size")));
			this.tabFeeds.TabIndex = ((int)(resources.GetObject("tabFeeds.TabIndex")));
			this.tabFeeds.Text = resources.GetString("tabFeeds.Text");
			this.toolTip1.SetToolTip(this.tabFeeds, resources.GetString("tabFeeds.ToolTip"));
			this.tabFeeds.ToolTipText = resources.GetString("tabFeeds.ToolTipText");
			this.tabFeeds.Visible = ((bool)(resources.GetObject("tabFeeds.Visible")));
			// 
			// sectionPanelFeedsCommentDefs
			// 
			this.sectionPanelFeedsCommentDefs.AccessibleDescription = resources.GetString("sectionPanelFeedsCommentDefs.AccessibleDescription");
			this.sectionPanelFeedsCommentDefs.AccessibleName = resources.GetString("sectionPanelFeedsCommentDefs.AccessibleName");
			this.sectionPanelFeedsCommentDefs.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelFeedsCommentDefs.Anchor")));
			this.sectionPanelFeedsCommentDefs.AutoScroll = ((bool)(resources.GetObject("sectionPanelFeedsCommentDefs.AutoScroll")));
			this.sectionPanelFeedsCommentDefs.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelFeedsCommentDefs.AutoScrollMargin")));
			this.sectionPanelFeedsCommentDefs.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelFeedsCommentDefs.AutoScrollMinSize")));
			this.sectionPanelFeedsCommentDefs.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelFeedsCommentDefs.BackgroundImage")));
			this.sectionPanelFeedsCommentDefs.Controls.Add(this.cboUserIdentityForComments);
			this.sectionPanelFeedsCommentDefs.Controls.Add(this.btnManageIdentities);
			this.sectionPanelFeedsCommentDefs.Controls.Add(this.linkCommentAPI);
			this.sectionPanelFeedsCommentDefs.Controls.Add(this.lblIdentityDropdownCaption);
			this.sectionPanelFeedsCommentDefs.Controls.Add(this.label6);
			this.sectionPanelFeedsCommentDefs.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelFeedsCommentDefs.Dock")));
			this.sectionPanelFeedsCommentDefs.Enabled = ((bool)(resources.GetObject("sectionPanelFeedsCommentDefs.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelFeedsCommentDefs, resources.GetString("sectionPanelFeedsCommentDefs.Error"));
			this.sectionPanelFeedsCommentDefs.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelFeedsCommentDefs.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelFeedsCommentDefs, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelFeedsCommentDefs.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelFeedsCommentDefs, ((int)(resources.GetObject("sectionPanelFeedsCommentDefs.IconPadding"))));
			this.sectionPanelFeedsCommentDefs.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelFeedsCommentDefs.Image")));
			this.sectionPanelFeedsCommentDefs.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelFeedsCommentDefs.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelFeedsCommentDefs.ImeMode")));
			this.sectionPanelFeedsCommentDefs.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelFeedsCommentDefs.Location")));
			this.sectionPanelFeedsCommentDefs.Name = "sectionPanelFeedsCommentDefs";
			this.sectionPanelFeedsCommentDefs.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelFeedsCommentDefs.RightToLeft")));
			this.sectionPanelFeedsCommentDefs.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelFeedsCommentDefs.Size")));
			this.sectionPanelFeedsCommentDefs.TabIndex = ((int)(resources.GetObject("sectionPanelFeedsCommentDefs.TabIndex")));
			this.sectionPanelFeedsCommentDefs.Text = resources.GetString("sectionPanelFeedsCommentDefs.Text");
			this.toolTip1.SetToolTip(this.sectionPanelFeedsCommentDefs, resources.GetString("sectionPanelFeedsCommentDefs.ToolTip"));
			this.sectionPanelFeedsCommentDefs.Visible = ((bool)(resources.GetObject("sectionPanelFeedsCommentDefs.Visible")));
			// 
			// cboUserIdentityForComments
			// 
			this.cboUserIdentityForComments.AccessibleDescription = resources.GetString("cboUserIdentityForComments.AccessibleDescription");
			this.cboUserIdentityForComments.AccessibleName = resources.GetString("cboUserIdentityForComments.AccessibleName");
			this.cboUserIdentityForComments.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cboUserIdentityForComments.Anchor")));
			this.cboUserIdentityForComments.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cboUserIdentityForComments.BackgroundImage")));
			this.cboUserIdentityForComments.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cboUserIdentityForComments.Dock")));
			this.cboUserIdentityForComments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboUserIdentityForComments.Enabled = ((bool)(resources.GetObject("cboUserIdentityForComments.Enabled")));
			this.errorProvider1.SetError(this.cboUserIdentityForComments, resources.GetString("cboUserIdentityForComments.Error"));
			this.cboUserIdentityForComments.Font = ((System.Drawing.Font)(resources.GetObject("cboUserIdentityForComments.Font")));
			this.errorProvider1.SetIconAlignment(this.cboUserIdentityForComments, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cboUserIdentityForComments.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.cboUserIdentityForComments, ((int)(resources.GetObject("cboUserIdentityForComments.IconPadding"))));
			this.cboUserIdentityForComments.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cboUserIdentityForComments.ImeMode")));
			this.cboUserIdentityForComments.IntegralHeight = ((bool)(resources.GetObject("cboUserIdentityForComments.IntegralHeight")));
			this.cboUserIdentityForComments.ItemHeight = ((int)(resources.GetObject("cboUserIdentityForComments.ItemHeight")));
			this.cboUserIdentityForComments.Location = ((System.Drawing.Point)(resources.GetObject("cboUserIdentityForComments.Location")));
			this.cboUserIdentityForComments.MaxDropDownItems = ((int)(resources.GetObject("cboUserIdentityForComments.MaxDropDownItems")));
			this.cboUserIdentityForComments.MaxLength = ((int)(resources.GetObject("cboUserIdentityForComments.MaxLength")));
			this.cboUserIdentityForComments.Name = "cboUserIdentityForComments";
			this.cboUserIdentityForComments.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cboUserIdentityForComments.RightToLeft")));
			this.cboUserIdentityForComments.Size = ((System.Drawing.Size)(resources.GetObject("cboUserIdentityForComments.Size")));
			this.cboUserIdentityForComments.Sorted = true;
			this.cboUserIdentityForComments.TabIndex = ((int)(resources.GetObject("cboUserIdentityForComments.TabIndex")));
			this.cboUserIdentityForComments.Text = resources.GetString("cboUserIdentityForComments.Text");
			this.toolTip1.SetToolTip(this.cboUserIdentityForComments, resources.GetString("cboUserIdentityForComments.ToolTip"));
			this.cboUserIdentityForComments.Visible = ((bool)(resources.GetObject("cboUserIdentityForComments.Visible")));
			this.cboUserIdentityForComments.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.cboUserIdentityForComments.Validated += new System.EventHandler(this.OnControlValidated);
			this.cboUserIdentityForComments.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
			// 
			// btnManageIdentities
			// 
			this.btnManageIdentities.AccessibleDescription = resources.GetString("btnManageIdentities.AccessibleDescription");
			this.btnManageIdentities.AccessibleName = resources.GetString("btnManageIdentities.AccessibleName");
			this.btnManageIdentities.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnManageIdentities.Anchor")));
			this.btnManageIdentities.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnManageIdentities.BackgroundImage")));
			this.btnManageIdentities.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnManageIdentities.Dock")));
			this.btnManageIdentities.Enabled = ((bool)(resources.GetObject("btnManageIdentities.Enabled")));
			this.errorProvider1.SetError(this.btnManageIdentities, resources.GetString("btnManageIdentities.Error"));
			this.btnManageIdentities.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnManageIdentities.FlatStyle")));
			this.btnManageIdentities.Font = ((System.Drawing.Font)(resources.GetObject("btnManageIdentities.Font")));
			this.errorProvider1.SetIconAlignment(this.btnManageIdentities, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnManageIdentities.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnManageIdentities, ((int)(resources.GetObject("btnManageIdentities.IconPadding"))));
			this.btnManageIdentities.Image = ((System.Drawing.Image)(resources.GetObject("btnManageIdentities.Image")));
			this.btnManageIdentities.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageIdentities.ImageAlign")));
			this.btnManageIdentities.ImageIndex = ((int)(resources.GetObject("btnManageIdentities.ImageIndex")));
			this.btnManageIdentities.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnManageIdentities.ImeMode")));
			this.btnManageIdentities.Location = ((System.Drawing.Point)(resources.GetObject("btnManageIdentities.Location")));
			this.btnManageIdentities.Name = "btnManageIdentities";
			this.btnManageIdentities.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnManageIdentities.RightToLeft")));
			this.btnManageIdentities.Size = ((System.Drawing.Size)(resources.GetObject("btnManageIdentities.Size")));
			this.btnManageIdentities.TabIndex = ((int)(resources.GetObject("btnManageIdentities.TabIndex")));
			this.btnManageIdentities.Text = resources.GetString("btnManageIdentities.Text");
			this.btnManageIdentities.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageIdentities.TextAlign")));
			this.toolTip1.SetToolTip(this.btnManageIdentities, resources.GetString("btnManageIdentities.ToolTip"));
			this.btnManageIdentities.Visible = ((bool)(resources.GetObject("btnManageIdentities.Visible")));
			this.btnManageIdentities.Click += new System.EventHandler(this.btnManageIdentities_Click);
			// 
			// linkCommentAPI
			// 
			this.linkCommentAPI.AccessibleDescription = resources.GetString("linkCommentAPI.AccessibleDescription");
			this.linkCommentAPI.AccessibleName = resources.GetString("linkCommentAPI.AccessibleName");
			this.linkCommentAPI.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkCommentAPI.Anchor")));
			this.linkCommentAPI.AutoSize = ((bool)(resources.GetObject("linkCommentAPI.AutoSize")));
			this.linkCommentAPI.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkCommentAPI.Dock")));
			this.linkCommentAPI.Enabled = ((bool)(resources.GetObject("linkCommentAPI.Enabled")));
			this.errorProvider1.SetError(this.linkCommentAPI, resources.GetString("linkCommentAPI.Error"));
			this.linkCommentAPI.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkCommentAPI.Font = ((System.Drawing.Font)(resources.GetObject("linkCommentAPI.Font")));
			this.errorProvider1.SetIconAlignment(this.linkCommentAPI, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("linkCommentAPI.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.linkCommentAPI, ((int)(resources.GetObject("linkCommentAPI.IconPadding"))));
			this.linkCommentAPI.Image = ((System.Drawing.Image)(resources.GetObject("linkCommentAPI.Image")));
			this.linkCommentAPI.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkCommentAPI.ImageAlign")));
			this.linkCommentAPI.ImageIndex = ((int)(resources.GetObject("linkCommentAPI.ImageIndex")));
			this.linkCommentAPI.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkCommentAPI.ImeMode")));
			this.linkCommentAPI.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkCommentAPI.LinkArea")));
			this.linkCommentAPI.Location = ((System.Drawing.Point)(resources.GetObject("linkCommentAPI.Location")));
			this.linkCommentAPI.Name = "linkCommentAPI";
			this.linkCommentAPI.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkCommentAPI.RightToLeft")));
			this.linkCommentAPI.Size = ((System.Drawing.Size)(resources.GetObject("linkCommentAPI.Size")));
			this.linkCommentAPI.TabIndex = ((int)(resources.GetObject("linkCommentAPI.TabIndex")));
			this.linkCommentAPI.TabStop = true;
			this.linkCommentAPI.Text = resources.GetString("linkCommentAPI.Text");
			this.linkCommentAPI.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkCommentAPI.TextAlign")));
			this.toolTip1.SetToolTip(this.linkCommentAPI, resources.GetString("linkCommentAPI.ToolTip"));
			this.linkCommentAPI.Visible = ((bool)(resources.GetObject("linkCommentAPI.Visible")));
			// 
			// lblIdentityDropdownCaption
			// 
			this.lblIdentityDropdownCaption.AccessibleDescription = resources.GetString("lblIdentityDropdownCaption.AccessibleDescription");
			this.lblIdentityDropdownCaption.AccessibleName = resources.GetString("lblIdentityDropdownCaption.AccessibleName");
			this.lblIdentityDropdownCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblIdentityDropdownCaption.Anchor")));
			this.lblIdentityDropdownCaption.AutoSize = ((bool)(resources.GetObject("lblIdentityDropdownCaption.AutoSize")));
			this.lblIdentityDropdownCaption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblIdentityDropdownCaption.Dock")));
			this.lblIdentityDropdownCaption.Enabled = ((bool)(resources.GetObject("lblIdentityDropdownCaption.Enabled")));
			this.errorProvider1.SetError(this.lblIdentityDropdownCaption, resources.GetString("lblIdentityDropdownCaption.Error"));
			this.lblIdentityDropdownCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblIdentityDropdownCaption.Font = ((System.Drawing.Font)(resources.GetObject("lblIdentityDropdownCaption.Font")));
			this.errorProvider1.SetIconAlignment(this.lblIdentityDropdownCaption, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lblIdentityDropdownCaption.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.lblIdentityDropdownCaption, ((int)(resources.GetObject("lblIdentityDropdownCaption.IconPadding"))));
			this.lblIdentityDropdownCaption.Image = ((System.Drawing.Image)(resources.GetObject("lblIdentityDropdownCaption.Image")));
			this.lblIdentityDropdownCaption.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblIdentityDropdownCaption.ImageAlign")));
			this.lblIdentityDropdownCaption.ImageIndex = ((int)(resources.GetObject("lblIdentityDropdownCaption.ImageIndex")));
			this.lblIdentityDropdownCaption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblIdentityDropdownCaption.ImeMode")));
			this.lblIdentityDropdownCaption.Location = ((System.Drawing.Point)(resources.GetObject("lblIdentityDropdownCaption.Location")));
			this.lblIdentityDropdownCaption.Name = "lblIdentityDropdownCaption";
			this.lblIdentityDropdownCaption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblIdentityDropdownCaption.RightToLeft")));
			this.lblIdentityDropdownCaption.Size = ((System.Drawing.Size)(resources.GetObject("lblIdentityDropdownCaption.Size")));
			this.lblIdentityDropdownCaption.TabIndex = ((int)(resources.GetObject("lblIdentityDropdownCaption.TabIndex")));
			this.lblIdentityDropdownCaption.Text = resources.GetString("lblIdentityDropdownCaption.Text");
			this.lblIdentityDropdownCaption.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblIdentityDropdownCaption.TextAlign")));
			this.toolTip1.SetToolTip(this.lblIdentityDropdownCaption, resources.GetString("lblIdentityDropdownCaption.ToolTip"));
			this.lblIdentityDropdownCaption.Visible = ((bool)(resources.GetObject("lblIdentityDropdownCaption.Visible")));
			// 
			// label6
			// 
			this.label6.AccessibleDescription = resources.GetString("label6.AccessibleDescription");
			this.label6.AccessibleName = resources.GetString("label6.AccessibleName");
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label6.Anchor")));
			this.label6.AutoSize = ((bool)(resources.GetObject("label6.AutoSize")));
			this.label6.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label6.Dock")));
			this.label6.Enabled = ((bool)(resources.GetObject("label6.Enabled")));
			this.errorProvider1.SetError(this.label6, resources.GetString("label6.Error"));
			this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label6.Font = ((System.Drawing.Font)(resources.GetObject("label6.Font")));
			this.errorProvider1.SetIconAlignment(this.label6, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label6.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label6, ((int)(resources.GetObject("label6.IconPadding"))));
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
			// sectionPanelFeedsTimings
			// 
			this.sectionPanelFeedsTimings.AccessibleDescription = resources.GetString("sectionPanelFeedsTimings.AccessibleDescription");
			this.sectionPanelFeedsTimings.AccessibleName = resources.GetString("sectionPanelFeedsTimings.AccessibleName");
			this.sectionPanelFeedsTimings.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelFeedsTimings.Anchor")));
			this.sectionPanelFeedsTimings.AutoScroll = ((bool)(resources.GetObject("sectionPanelFeedsTimings.AutoScroll")));
			this.sectionPanelFeedsTimings.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelFeedsTimings.AutoScrollMargin")));
			this.sectionPanelFeedsTimings.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelFeedsTimings.AutoScrollMinSize")));
			this.sectionPanelFeedsTimings.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelFeedsTimings.BackgroundImage")));
			this.sectionPanelFeedsTimings.Controls.Add(this.label15);
			this.sectionPanelFeedsTimings.Controls.Add(this.linkLabel1);
			this.sectionPanelFeedsTimings.Controls.Add(this.comboRefreshRate);
			this.sectionPanelFeedsTimings.Controls.Add(this.label5);
			this.sectionPanelFeedsTimings.Controls.Add(this.label4);
			this.sectionPanelFeedsTimings.Controls.Add(this.comboMaxItemAge);
			this.sectionPanelFeedsTimings.Controls.Add(this.label3);
			this.sectionPanelFeedsTimings.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelFeedsTimings.Dock")));
			this.sectionPanelFeedsTimings.Enabled = ((bool)(resources.GetObject("sectionPanelFeedsTimings.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelFeedsTimings, resources.GetString("sectionPanelFeedsTimings.Error"));
			this.sectionPanelFeedsTimings.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelFeedsTimings.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelFeedsTimings, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelFeedsTimings.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelFeedsTimings, ((int)(resources.GetObject("sectionPanelFeedsTimings.IconPadding"))));
			this.sectionPanelFeedsTimings.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelFeedsTimings.Image")));
			this.sectionPanelFeedsTimings.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelFeedsTimings.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelFeedsTimings.ImeMode")));
			this.sectionPanelFeedsTimings.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelFeedsTimings.Location")));
			this.sectionPanelFeedsTimings.Name = "sectionPanelFeedsTimings";
			this.sectionPanelFeedsTimings.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelFeedsTimings.RightToLeft")));
			this.sectionPanelFeedsTimings.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelFeedsTimings.Size")));
			this.sectionPanelFeedsTimings.TabIndex = ((int)(resources.GetObject("sectionPanelFeedsTimings.TabIndex")));
			this.sectionPanelFeedsTimings.Text = resources.GetString("sectionPanelFeedsTimings.Text");
			this.toolTip1.SetToolTip(this.sectionPanelFeedsTimings, resources.GetString("sectionPanelFeedsTimings.ToolTip"));
			this.sectionPanelFeedsTimings.Visible = ((bool)(resources.GetObject("sectionPanelFeedsTimings.Visible")));
			// 
			// label15
			// 
			this.label15.AccessibleDescription = resources.GetString("label15.AccessibleDescription");
			this.label15.AccessibleName = resources.GetString("label15.AccessibleName");
			this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label15.Anchor")));
			this.label15.AutoSize = ((bool)(resources.GetObject("label15.AutoSize")));
			this.label15.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label15.Dock")));
			this.label15.Enabled = ((bool)(resources.GetObject("label15.Enabled")));
			this.errorProvider1.SetError(this.label15, resources.GetString("label15.Error"));
			this.label15.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label15.Font = ((System.Drawing.Font)(resources.GetObject("label15.Font")));
			this.errorProvider1.SetIconAlignment(this.label15, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label15.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label15, ((int)(resources.GetObject("label15.IconPadding"))));
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
			// linkLabel1
			// 
			this.linkLabel1.AccessibleDescription = resources.GetString("linkLabel1.AccessibleDescription");
			this.linkLabel1.AccessibleName = resources.GetString("linkLabel1.AccessibleName");
			this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkLabel1.Anchor")));
			this.linkLabel1.AutoSize = ((bool)(resources.GetObject("linkLabel1.AutoSize")));
			this.linkLabel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkLabel1.Dock")));
			this.linkLabel1.Enabled = ((bool)(resources.GetObject("linkLabel1.Enabled")));
			this.errorProvider1.SetError(this.linkLabel1, resources.GetString("linkLabel1.Error"));
			this.linkLabel1.Font = ((System.Drawing.Font)(resources.GetObject("linkLabel1.Font")));
			this.errorProvider1.SetIconAlignment(this.linkLabel1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("linkLabel1.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.linkLabel1, ((int)(resources.GetObject("linkLabel1.IconPadding"))));
			this.linkLabel1.Image = ((System.Drawing.Image)(resources.GetObject("linkLabel1.Image")));
			this.linkLabel1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel1.ImageAlign")));
			this.linkLabel1.ImageIndex = ((int)(resources.GetObject("linkLabel1.ImageIndex")));
			this.linkLabel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkLabel1.ImeMode")));
			this.linkLabel1.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkLabel1.LinkArea")));
			this.linkLabel1.Location = ((System.Drawing.Point)(resources.GetObject("linkLabel1.Location")));
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkLabel1.RightToLeft")));
			this.linkLabel1.Size = ((System.Drawing.Size)(resources.GetObject("linkLabel1.Size")));
			this.linkLabel1.TabIndex = ((int)(resources.GetObject("linkLabel1.TabIndex")));
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = resources.GetString("linkLabel1.Text");
			this.linkLabel1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabel1.TextAlign")));
			this.toolTip1.SetToolTip(this.linkLabel1, resources.GetString("linkLabel1.ToolTip"));
			this.linkLabel1.Visible = ((bool)(resources.GetObject("linkLabel1.Visible")));
			// 
			// comboRefreshRate
			// 
			this.comboRefreshRate.AccessibleDescription = resources.GetString("comboRefreshRate.AccessibleDescription");
			this.comboRefreshRate.AccessibleName = resources.GetString("comboRefreshRate.AccessibleName");
			this.comboRefreshRate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboRefreshRate.Anchor")));
			this.comboRefreshRate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboRefreshRate.BackgroundImage")));
			this.comboRefreshRate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboRefreshRate.Dock")));
			this.comboRefreshRate.Enabled = ((bool)(resources.GetObject("comboRefreshRate.Enabled")));
			this.errorProvider1.SetError(this.comboRefreshRate, resources.GetString("comboRefreshRate.Error"));
			this.comboRefreshRate.Font = ((System.Drawing.Font)(resources.GetObject("comboRefreshRate.Font")));
			this.errorProvider1.SetIconAlignment(this.comboRefreshRate, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboRefreshRate.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.comboRefreshRate, ((int)(resources.GetObject("comboRefreshRate.IconPadding"))));
			this.comboRefreshRate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("comboRefreshRate.ImeMode")));
			this.comboRefreshRate.IntegralHeight = ((bool)(resources.GetObject("comboRefreshRate.IntegralHeight")));
			this.comboRefreshRate.ItemHeight = ((int)(resources.GetObject("comboRefreshRate.ItemHeight")));
			this.comboRefreshRate.Items.AddRange(new object[] {
																  resources.GetString("comboRefreshRate.Items"),
																  resources.GetString("comboRefreshRate.Items1"),
																  resources.GetString("comboRefreshRate.Items2"),
																  resources.GetString("comboRefreshRate.Items3"),
																  resources.GetString("comboRefreshRate.Items4"),
																  resources.GetString("comboRefreshRate.Items5"),
																  resources.GetString("comboRefreshRate.Items6"),
																  resources.GetString("comboRefreshRate.Items7"),
																  resources.GetString("comboRefreshRate.Items8"),
																  resources.GetString("comboRefreshRate.Items9"),
																  resources.GetString("comboRefreshRate.Items10"),
																  resources.GetString("comboRefreshRate.Items11")});
			this.comboRefreshRate.Location = ((System.Drawing.Point)(resources.GetObject("comboRefreshRate.Location")));
			this.comboRefreshRate.MaxDropDownItems = ((int)(resources.GetObject("comboRefreshRate.MaxDropDownItems")));
			this.comboRefreshRate.MaxLength = ((int)(resources.GetObject("comboRefreshRate.MaxLength")));
			this.comboRefreshRate.Name = "comboRefreshRate";
			this.comboRefreshRate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("comboRefreshRate.RightToLeft")));
			this.comboRefreshRate.Size = ((System.Drawing.Size)(resources.GetObject("comboRefreshRate.Size")));
			this.comboRefreshRate.TabIndex = ((int)(resources.GetObject("comboRefreshRate.TabIndex")));
			this.comboRefreshRate.Text = resources.GetString("comboRefreshRate.Text");
			this.toolTip1.SetToolTip(this.comboRefreshRate, resources.GetString("comboRefreshRate.ToolTip"));
			this.comboRefreshRate.Visible = ((bool)(resources.GetObject("comboRefreshRate.Visible")));
			this.comboRefreshRate.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.comboRefreshRate.Validated += new System.EventHandler(this.OnControlValidated);
			this.comboRefreshRate.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
			// 
			// label5
			// 
			this.label5.AccessibleDescription = resources.GetString("label5.AccessibleDescription");
			this.label5.AccessibleName = resources.GetString("label5.AccessibleName");
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label5.Anchor")));
			this.label5.AutoSize = ((bool)(resources.GetObject("label5.AutoSize")));
			this.label5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label5.Dock")));
			this.label5.Enabled = ((bool)(resources.GetObject("label5.Enabled")));
			this.errorProvider1.SetError(this.label5, resources.GetString("label5.Error"));
			this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label5.Font = ((System.Drawing.Font)(resources.GetObject("label5.Font")));
			this.errorProvider1.SetIconAlignment(this.label5, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label5.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label5, ((int)(resources.GetObject("label5.IconPadding"))));
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
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.errorProvider1.SetError(this.label4, resources.GetString("label4.Error"));
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.errorProvider1.SetIconAlignment(this.label4, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label4.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label4, ((int)(resources.GetObject("label4.IconPadding"))));
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
			// comboMaxItemAge
			// 
			this.comboMaxItemAge.AccessibleDescription = resources.GetString("comboMaxItemAge.AccessibleDescription");
			this.comboMaxItemAge.AccessibleName = resources.GetString("comboMaxItemAge.AccessibleName");
			this.comboMaxItemAge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("comboMaxItemAge.Anchor")));
			this.comboMaxItemAge.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("comboMaxItemAge.BackgroundImage")));
			this.comboMaxItemAge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("comboMaxItemAge.Dock")));
			this.comboMaxItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboMaxItemAge.Enabled = ((bool)(resources.GetObject("comboMaxItemAge.Enabled")));
			this.errorProvider1.SetError(this.comboMaxItemAge, resources.GetString("comboMaxItemAge.Error"));
			this.comboMaxItemAge.Font = ((System.Drawing.Font)(resources.GetObject("comboMaxItemAge.Font")));
			this.errorProvider1.SetIconAlignment(this.comboMaxItemAge, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("comboMaxItemAge.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.comboMaxItemAge, ((int)(resources.GetObject("comboMaxItemAge.IconPadding"))));
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
			this.comboMaxItemAge.SelectionChangeCommitted += new System.EventHandler(this.OnAnyComboSelectionChangeCommitted);
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.errorProvider1.SetError(this.label3, resources.GetString("label3.Error"));
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.errorProvider1.SetIconAlignment(this.label3, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label3.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label3, ((int)(resources.GetObject("label3.IconPadding"))));
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
			// tabNetConnection
			// 
			this.tabNetConnection.AccessibleDescription = resources.GetString("tabNetConnection.AccessibleDescription");
			this.tabNetConnection.AccessibleName = resources.GetString("tabNetConnection.AccessibleName");
			this.tabNetConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabNetConnection.Anchor")));
			this.tabNetConnection.AutoScroll = ((bool)(resources.GetObject("tabNetConnection.AutoScroll")));
			this.tabNetConnection.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabNetConnection.AutoScrollMargin")));
			this.tabNetConnection.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabNetConnection.AutoScrollMinSize")));
			this.tabNetConnection.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabNetConnection.BackgroundImage")));
			this.tabNetConnection.Controls.Add(this.sectionPanelNetworkConnectionProxy);
			this.tabNetConnection.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabNetConnection.Dock")));
			this.tabNetConnection.Enabled = ((bool)(resources.GetObject("tabNetConnection.Enabled")));
			this.errorProvider1.SetError(this.tabNetConnection, resources.GetString("tabNetConnection.Error"));
			this.tabNetConnection.Font = ((System.Drawing.Font)(resources.GetObject("tabNetConnection.Font")));
			this.errorProvider1.SetIconAlignment(this.tabNetConnection, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabNetConnection.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabNetConnection, ((int)(resources.GetObject("tabNetConnection.IconPadding"))));
			this.tabNetConnection.ImageIndex = ((int)(resources.GetObject("tabNetConnection.ImageIndex")));
			this.tabNetConnection.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabNetConnection.ImeMode")));
			this.tabNetConnection.Location = ((System.Drawing.Point)(resources.GetObject("tabNetConnection.Location")));
			this.tabNetConnection.Name = "tabNetConnection";
			this.tabNetConnection.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabNetConnection.RightToLeft")));
			this.tabNetConnection.Size = ((System.Drawing.Size)(resources.GetObject("tabNetConnection.Size")));
			this.tabNetConnection.TabIndex = ((int)(resources.GetObject("tabNetConnection.TabIndex")));
			this.tabNetConnection.Text = resources.GetString("tabNetConnection.Text");
			this.toolTip1.SetToolTip(this.tabNetConnection, resources.GetString("tabNetConnection.ToolTip"));
			this.tabNetConnection.ToolTipText = resources.GetString("tabNetConnection.ToolTipText");
			this.tabNetConnection.Visible = ((bool)(resources.GetObject("tabNetConnection.Visible")));
			// 
			// sectionPanelNetworkConnectionProxy
			// 
			this.sectionPanelNetworkConnectionProxy.AccessibleDescription = resources.GetString("sectionPanelNetworkConnectionProxy.AccessibleDescription");
			this.sectionPanelNetworkConnectionProxy.AccessibleName = resources.GetString("sectionPanelNetworkConnectionProxy.AccessibleName");
			this.sectionPanelNetworkConnectionProxy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelNetworkConnectionProxy.Anchor")));
			this.sectionPanelNetworkConnectionProxy.AutoScroll = ((bool)(resources.GetObject("sectionPanelNetworkConnectionProxy.AutoScroll")));
			this.sectionPanelNetworkConnectionProxy.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelNetworkConnectionProxy.AutoScrollMargin")));
			this.sectionPanelNetworkConnectionProxy.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelNetworkConnectionProxy.AutoScrollMinSize")));
			this.sectionPanelNetworkConnectionProxy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelNetworkConnectionProxy.BackgroundImage")));
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
			this.sectionPanelNetworkConnectionProxy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelNetworkConnectionProxy.Dock")));
			this.sectionPanelNetworkConnectionProxy.Enabled = ((bool)(resources.GetObject("sectionPanelNetworkConnectionProxy.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelNetworkConnectionProxy, resources.GetString("sectionPanelNetworkConnectionProxy.Error"));
			this.sectionPanelNetworkConnectionProxy.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelNetworkConnectionProxy.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelNetworkConnectionProxy, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelNetworkConnectionProxy.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelNetworkConnectionProxy, ((int)(resources.GetObject("sectionPanelNetworkConnectionProxy.IconPadding"))));
			this.sectionPanelNetworkConnectionProxy.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelNetworkConnectionProxy.Image")));
			this.sectionPanelNetworkConnectionProxy.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelNetworkConnectionProxy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelNetworkConnectionProxy.ImeMode")));
			this.sectionPanelNetworkConnectionProxy.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelNetworkConnectionProxy.Location")));
			this.sectionPanelNetworkConnectionProxy.Name = "sectionPanelNetworkConnectionProxy";
			this.sectionPanelNetworkConnectionProxy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelNetworkConnectionProxy.RightToLeft")));
			this.sectionPanelNetworkConnectionProxy.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelNetworkConnectionProxy.Size")));
			this.sectionPanelNetworkConnectionProxy.TabIndex = ((int)(resources.GetObject("sectionPanelNetworkConnectionProxy.TabIndex")));
			this.sectionPanelNetworkConnectionProxy.Text = resources.GetString("sectionPanelNetworkConnectionProxy.Text");
			this.toolTip1.SetToolTip(this.sectionPanelNetworkConnectionProxy, resources.GetString("sectionPanelNetworkConnectionProxy.ToolTip"));
			this.sectionPanelNetworkConnectionProxy.Visible = ((bool)(resources.GetObject("sectionPanelNetworkConnectionProxy.Visible")));
			// 
			// textProxyBypassList
			// 
			this.textProxyBypassList.AccessibleDescription = resources.GetString("textProxyBypassList.AccessibleDescription");
			this.textProxyBypassList.AccessibleName = resources.GetString("textProxyBypassList.AccessibleName");
			this.textProxyBypassList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textProxyBypassList.Anchor")));
			this.textProxyBypassList.AutoSize = ((bool)(resources.GetObject("textProxyBypassList.AutoSize")));
			this.textProxyBypassList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textProxyBypassList.BackgroundImage")));
			this.textProxyBypassList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textProxyBypassList.Dock")));
			this.textProxyBypassList.Enabled = ((bool)(resources.GetObject("textProxyBypassList.Enabled")));
			this.errorProvider1.SetError(this.textProxyBypassList, resources.GetString("textProxyBypassList.Error"));
			this.textProxyBypassList.Font = ((System.Drawing.Font)(resources.GetObject("textProxyBypassList.Font")));
			this.errorProvider1.SetIconAlignment(this.textProxyBypassList, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyBypassList.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textProxyBypassList, ((int)(resources.GetObject("textProxyBypassList.IconPadding"))));
			this.textProxyBypassList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textProxyBypassList.ImeMode")));
			this.textProxyBypassList.Location = ((System.Drawing.Point)(resources.GetObject("textProxyBypassList.Location")));
			this.textProxyBypassList.MaxLength = ((int)(resources.GetObject("textProxyBypassList.MaxLength")));
			this.textProxyBypassList.Multiline = ((bool)(resources.GetObject("textProxyBypassList.Multiline")));
			this.textProxyBypassList.Name = "textProxyBypassList";
			this.textProxyBypassList.PasswordChar = ((char)(resources.GetObject("textProxyBypassList.PasswordChar")));
			this.textProxyBypassList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textProxyBypassList.RightToLeft")));
			this.textProxyBypassList.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textProxyBypassList.ScrollBars")));
			this.textProxyBypassList.Size = ((System.Drawing.Size)(resources.GetObject("textProxyBypassList.Size")));
			this.textProxyBypassList.TabIndex = ((int)(resources.GetObject("textProxyBypassList.TabIndex")));
			this.textProxyBypassList.Text = resources.GetString("textProxyBypassList.Text");
			this.textProxyBypassList.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textProxyBypassList.TextAlign")));
			this.toolTip1.SetToolTip(this.textProxyBypassList, resources.GetString("textProxyBypassList.ToolTip"));
			this.textProxyBypassList.Visible = ((bool)(resources.GetObject("textProxyBypassList.Visible")));
			this.textProxyBypassList.WordWrap = ((bool)(resources.GetObject("textProxyBypassList.WordWrap")));
			// 
			// checkNoProxy
			// 
			this.checkNoProxy.AccessibleDescription = resources.GetString("checkNoProxy.AccessibleDescription");
			this.checkNoProxy.AccessibleName = resources.GetString("checkNoProxy.AccessibleName");
			this.checkNoProxy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkNoProxy.Anchor")));
			this.checkNoProxy.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkNoProxy.Appearance")));
			this.checkNoProxy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkNoProxy.BackgroundImage")));
			this.checkNoProxy.CausesValidation = false;
			this.checkNoProxy.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkNoProxy.CheckAlign")));
			this.checkNoProxy.Checked = true;
			this.checkNoProxy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkNoProxy.Dock")));
			this.checkNoProxy.Enabled = ((bool)(resources.GetObject("checkNoProxy.Enabled")));
			this.errorProvider1.SetError(this.checkNoProxy, resources.GetString("checkNoProxy.Error"));
			this.checkNoProxy.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkNoProxy.FlatStyle")));
			this.checkNoProxy.Font = ((System.Drawing.Font)(resources.GetObject("checkNoProxy.Font")));
			this.errorProvider1.SetIconAlignment(this.checkNoProxy, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkNoProxy.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkNoProxy, ((int)(resources.GetObject("checkNoProxy.IconPadding"))));
			this.checkNoProxy.Image = ((System.Drawing.Image)(resources.GetObject("checkNoProxy.Image")));
			this.checkNoProxy.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkNoProxy.ImageAlign")));
			this.checkNoProxy.ImageIndex = ((int)(resources.GetObject("checkNoProxy.ImageIndex")));
			this.checkNoProxy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkNoProxy.ImeMode")));
			this.checkNoProxy.Location = ((System.Drawing.Point)(resources.GetObject("checkNoProxy.Location")));
			this.checkNoProxy.Name = "checkNoProxy";
			this.checkNoProxy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkNoProxy.RightToLeft")));
			this.checkNoProxy.Size = ((System.Drawing.Size)(resources.GetObject("checkNoProxy.Size")));
			this.checkNoProxy.TabIndex = ((int)(resources.GetObject("checkNoProxy.TabIndex")));
			this.checkNoProxy.TabStop = true;
			this.checkNoProxy.Text = resources.GetString("checkNoProxy.Text");
			this.checkNoProxy.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkNoProxy.TextAlign")));
			this.toolTip1.SetToolTip(this.checkNoProxy, resources.GetString("checkNoProxy.ToolTip"));
			this.checkNoProxy.Visible = ((bool)(resources.GetObject("checkNoProxy.Visible")));
			this.checkNoProxy.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkNoProxy.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkNoProxy.CheckedChanged += new System.EventHandler(this.checkUseProxy_CheckedChanged);
			// 
			// textProxyCredentialPassword
			// 
			this.textProxyCredentialPassword.AccessibleDescription = resources.GetString("textProxyCredentialPassword.AccessibleDescription");
			this.textProxyCredentialPassword.AccessibleName = resources.GetString("textProxyCredentialPassword.AccessibleName");
			this.textProxyCredentialPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textProxyCredentialPassword.Anchor")));
			this.textProxyCredentialPassword.AutoSize = ((bool)(resources.GetObject("textProxyCredentialPassword.AutoSize")));
			this.textProxyCredentialPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textProxyCredentialPassword.BackgroundImage")));
			this.textProxyCredentialPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textProxyCredentialPassword.Dock")));
			this.textProxyCredentialPassword.Enabled = ((bool)(resources.GetObject("textProxyCredentialPassword.Enabled")));
			this.errorProvider1.SetError(this.textProxyCredentialPassword, resources.GetString("textProxyCredentialPassword.Error"));
			this.textProxyCredentialPassword.Font = ((System.Drawing.Font)(resources.GetObject("textProxyCredentialPassword.Font")));
			this.errorProvider1.SetIconAlignment(this.textProxyCredentialPassword, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyCredentialPassword.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textProxyCredentialPassword, ((int)(resources.GetObject("textProxyCredentialPassword.IconPadding"))));
			this.textProxyCredentialPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textProxyCredentialPassword.ImeMode")));
			this.textProxyCredentialPassword.Location = ((System.Drawing.Point)(resources.GetObject("textProxyCredentialPassword.Location")));
			this.textProxyCredentialPassword.MaxLength = ((int)(resources.GetObject("textProxyCredentialPassword.MaxLength")));
			this.textProxyCredentialPassword.Multiline = ((bool)(resources.GetObject("textProxyCredentialPassword.Multiline")));
			this.textProxyCredentialPassword.Name = "textProxyCredentialPassword";
			this.textProxyCredentialPassword.PasswordChar = ((char)(resources.GetObject("textProxyCredentialPassword.PasswordChar")));
			this.textProxyCredentialPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textProxyCredentialPassword.RightToLeft")));
			this.textProxyCredentialPassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textProxyCredentialPassword.ScrollBars")));
			this.textProxyCredentialPassword.Size = ((System.Drawing.Size)(resources.GetObject("textProxyCredentialPassword.Size")));
			this.textProxyCredentialPassword.TabIndex = ((int)(resources.GetObject("textProxyCredentialPassword.TabIndex")));
			this.textProxyCredentialPassword.Text = resources.GetString("textProxyCredentialPassword.Text");
			this.textProxyCredentialPassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textProxyCredentialPassword.TextAlign")));
			this.toolTip1.SetToolTip(this.textProxyCredentialPassword, resources.GetString("textProxyCredentialPassword.ToolTip"));
			this.textProxyCredentialPassword.Visible = ((bool)(resources.GetObject("textProxyCredentialPassword.Visible")));
			this.textProxyCredentialPassword.WordWrap = ((bool)(resources.GetObject("textProxyCredentialPassword.WordWrap")));
			this.textProxyCredentialPassword.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.textProxyCredentialPassword.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// labelProxyCredentialPassword
			// 
			this.labelProxyCredentialPassword.AccessibleDescription = resources.GetString("labelProxyCredentialPassword.AccessibleDescription");
			this.labelProxyCredentialPassword.AccessibleName = resources.GetString("labelProxyCredentialPassword.AccessibleName");
			this.labelProxyCredentialPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProxyCredentialPassword.Anchor")));
			this.labelProxyCredentialPassword.AutoSize = ((bool)(resources.GetObject("labelProxyCredentialPassword.AutoSize")));
			this.labelProxyCredentialPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProxyCredentialPassword.Dock")));
			this.labelProxyCredentialPassword.Enabled = ((bool)(resources.GetObject("labelProxyCredentialPassword.Enabled")));
			this.errorProvider1.SetError(this.labelProxyCredentialPassword, resources.GetString("labelProxyCredentialPassword.Error"));
			this.labelProxyCredentialPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelProxyCredentialPassword.Font = ((System.Drawing.Font)(resources.GetObject("labelProxyCredentialPassword.Font")));
			this.errorProvider1.SetIconAlignment(this.labelProxyCredentialPassword, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelProxyCredentialPassword.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelProxyCredentialPassword, ((int)(resources.GetObject("labelProxyCredentialPassword.IconPadding"))));
			this.labelProxyCredentialPassword.Image = ((System.Drawing.Image)(resources.GetObject("labelProxyCredentialPassword.Image")));
			this.labelProxyCredentialPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyCredentialPassword.ImageAlign")));
			this.labelProxyCredentialPassword.ImageIndex = ((int)(resources.GetObject("labelProxyCredentialPassword.ImageIndex")));
			this.labelProxyCredentialPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProxyCredentialPassword.ImeMode")));
			this.labelProxyCredentialPassword.Location = ((System.Drawing.Point)(resources.GetObject("labelProxyCredentialPassword.Location")));
			this.labelProxyCredentialPassword.Name = "labelProxyCredentialPassword";
			this.labelProxyCredentialPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProxyCredentialPassword.RightToLeft")));
			this.labelProxyCredentialPassword.Size = ((System.Drawing.Size)(resources.GetObject("labelProxyCredentialPassword.Size")));
			this.labelProxyCredentialPassword.TabIndex = ((int)(resources.GetObject("labelProxyCredentialPassword.TabIndex")));
			this.labelProxyCredentialPassword.Text = resources.GetString("labelProxyCredentialPassword.Text");
			this.labelProxyCredentialPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyCredentialPassword.TextAlign")));
			this.toolTip1.SetToolTip(this.labelProxyCredentialPassword, resources.GetString("labelProxyCredentialPassword.ToolTip"));
			this.labelProxyCredentialPassword.Visible = ((bool)(resources.GetObject("labelProxyCredentialPassword.Visible")));
			// 
			// labelProxyCredentialUserName
			// 
			this.labelProxyCredentialUserName.AccessibleDescription = resources.GetString("labelProxyCredentialUserName.AccessibleDescription");
			this.labelProxyCredentialUserName.AccessibleName = resources.GetString("labelProxyCredentialUserName.AccessibleName");
			this.labelProxyCredentialUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProxyCredentialUserName.Anchor")));
			this.labelProxyCredentialUserName.AutoSize = ((bool)(resources.GetObject("labelProxyCredentialUserName.AutoSize")));
			this.labelProxyCredentialUserName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProxyCredentialUserName.Dock")));
			this.labelProxyCredentialUserName.Enabled = ((bool)(resources.GetObject("labelProxyCredentialUserName.Enabled")));
			this.errorProvider1.SetError(this.labelProxyCredentialUserName, resources.GetString("labelProxyCredentialUserName.Error"));
			this.labelProxyCredentialUserName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelProxyCredentialUserName.Font = ((System.Drawing.Font)(resources.GetObject("labelProxyCredentialUserName.Font")));
			this.errorProvider1.SetIconAlignment(this.labelProxyCredentialUserName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelProxyCredentialUserName.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelProxyCredentialUserName, ((int)(resources.GetObject("labelProxyCredentialUserName.IconPadding"))));
			this.labelProxyCredentialUserName.Image = ((System.Drawing.Image)(resources.GetObject("labelProxyCredentialUserName.Image")));
			this.labelProxyCredentialUserName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyCredentialUserName.ImageAlign")));
			this.labelProxyCredentialUserName.ImageIndex = ((int)(resources.GetObject("labelProxyCredentialUserName.ImageIndex")));
			this.labelProxyCredentialUserName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProxyCredentialUserName.ImeMode")));
			this.labelProxyCredentialUserName.Location = ((System.Drawing.Point)(resources.GetObject("labelProxyCredentialUserName.Location")));
			this.labelProxyCredentialUserName.Name = "labelProxyCredentialUserName";
			this.labelProxyCredentialUserName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProxyCredentialUserName.RightToLeft")));
			this.labelProxyCredentialUserName.Size = ((System.Drawing.Size)(resources.GetObject("labelProxyCredentialUserName.Size")));
			this.labelProxyCredentialUserName.TabIndex = ((int)(resources.GetObject("labelProxyCredentialUserName.TabIndex")));
			this.labelProxyCredentialUserName.Text = resources.GetString("labelProxyCredentialUserName.Text");
			this.labelProxyCredentialUserName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyCredentialUserName.TextAlign")));
			this.toolTip1.SetToolTip(this.labelProxyCredentialUserName, resources.GetString("labelProxyCredentialUserName.ToolTip"));
			this.labelProxyCredentialUserName.Visible = ((bool)(resources.GetObject("labelProxyCredentialUserName.Visible")));
			// 
			// textProxyCredentialUser
			// 
			this.textProxyCredentialUser.AccessibleDescription = resources.GetString("textProxyCredentialUser.AccessibleDescription");
			this.textProxyCredentialUser.AccessibleName = resources.GetString("textProxyCredentialUser.AccessibleName");
			this.textProxyCredentialUser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textProxyCredentialUser.Anchor")));
			this.textProxyCredentialUser.AutoSize = ((bool)(resources.GetObject("textProxyCredentialUser.AutoSize")));
			this.textProxyCredentialUser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textProxyCredentialUser.BackgroundImage")));
			this.textProxyCredentialUser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textProxyCredentialUser.Dock")));
			this.textProxyCredentialUser.Enabled = ((bool)(resources.GetObject("textProxyCredentialUser.Enabled")));
			this.errorProvider1.SetError(this.textProxyCredentialUser, resources.GetString("textProxyCredentialUser.Error"));
			this.textProxyCredentialUser.Font = ((System.Drawing.Font)(resources.GetObject("textProxyCredentialUser.Font")));
			this.errorProvider1.SetIconAlignment(this.textProxyCredentialUser, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyCredentialUser.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textProxyCredentialUser, ((int)(resources.GetObject("textProxyCredentialUser.IconPadding"))));
			this.textProxyCredentialUser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textProxyCredentialUser.ImeMode")));
			this.textProxyCredentialUser.Location = ((System.Drawing.Point)(resources.GetObject("textProxyCredentialUser.Location")));
			this.textProxyCredentialUser.MaxLength = ((int)(resources.GetObject("textProxyCredentialUser.MaxLength")));
			this.textProxyCredentialUser.Multiline = ((bool)(resources.GetObject("textProxyCredentialUser.Multiline")));
			this.textProxyCredentialUser.Name = "textProxyCredentialUser";
			this.textProxyCredentialUser.PasswordChar = ((char)(resources.GetObject("textProxyCredentialUser.PasswordChar")));
			this.textProxyCredentialUser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textProxyCredentialUser.RightToLeft")));
			this.textProxyCredentialUser.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textProxyCredentialUser.ScrollBars")));
			this.textProxyCredentialUser.Size = ((System.Drawing.Size)(resources.GetObject("textProxyCredentialUser.Size")));
			this.textProxyCredentialUser.TabIndex = ((int)(resources.GetObject("textProxyCredentialUser.TabIndex")));
			this.textProxyCredentialUser.Text = resources.GetString("textProxyCredentialUser.Text");
			this.textProxyCredentialUser.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textProxyCredentialUser.TextAlign")));
			this.toolTip1.SetToolTip(this.textProxyCredentialUser, resources.GetString("textProxyCredentialUser.ToolTip"));
			this.textProxyCredentialUser.Visible = ((bool)(resources.GetObject("textProxyCredentialUser.Visible")));
			this.textProxyCredentialUser.WordWrap = ((bool)(resources.GetObject("textProxyCredentialUser.WordWrap")));
			this.textProxyCredentialUser.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.textProxyCredentialUser.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// checkProxyAuth
			// 
			this.checkProxyAuth.AccessibleDescription = resources.GetString("checkProxyAuth.AccessibleDescription");
			this.checkProxyAuth.AccessibleName = resources.GetString("checkProxyAuth.AccessibleName");
			this.checkProxyAuth.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkProxyAuth.Anchor")));
			this.checkProxyAuth.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkProxyAuth.Appearance")));
			this.checkProxyAuth.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkProxyAuth.BackgroundImage")));
			this.checkProxyAuth.CausesValidation = false;
			this.checkProxyAuth.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkProxyAuth.CheckAlign")));
			this.checkProxyAuth.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkProxyAuth.Dock")));
			this.checkProxyAuth.Enabled = ((bool)(resources.GetObject("checkProxyAuth.Enabled")));
			this.errorProvider1.SetError(this.checkProxyAuth, resources.GetString("checkProxyAuth.Error"));
			this.checkProxyAuth.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkProxyAuth.FlatStyle")));
			this.checkProxyAuth.Font = ((System.Drawing.Font)(resources.GetObject("checkProxyAuth.Font")));
			this.errorProvider1.SetIconAlignment(this.checkProxyAuth, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkProxyAuth.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkProxyAuth, ((int)(resources.GetObject("checkProxyAuth.IconPadding"))));
			this.checkProxyAuth.Image = ((System.Drawing.Image)(resources.GetObject("checkProxyAuth.Image")));
			this.checkProxyAuth.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkProxyAuth.ImageAlign")));
			this.checkProxyAuth.ImageIndex = ((int)(resources.GetObject("checkProxyAuth.ImageIndex")));
			this.checkProxyAuth.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkProxyAuth.ImeMode")));
			this.checkProxyAuth.Location = ((System.Drawing.Point)(resources.GetObject("checkProxyAuth.Location")));
			this.checkProxyAuth.Name = "checkProxyAuth";
			this.checkProxyAuth.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkProxyAuth.RightToLeft")));
			this.checkProxyAuth.Size = ((System.Drawing.Size)(resources.GetObject("checkProxyAuth.Size")));
			this.checkProxyAuth.TabIndex = ((int)(resources.GetObject("checkProxyAuth.TabIndex")));
			this.checkProxyAuth.Text = resources.GetString("checkProxyAuth.Text");
			this.checkProxyAuth.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkProxyAuth.TextAlign")));
			this.toolTip1.SetToolTip(this.checkProxyAuth, resources.GetString("checkProxyAuth.ToolTip"));
			this.checkProxyAuth.Visible = ((bool)(resources.GetObject("checkProxyAuth.Visible")));
			this.checkProxyAuth.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkProxyAuth.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkProxyAuth.CheckedChanged += new System.EventHandler(this.checkProxyAuth_CheckedChanged);
			// 
			// labelProxyBypassListHint
			// 
			this.labelProxyBypassListHint.AccessibleDescription = resources.GetString("labelProxyBypassListHint.AccessibleDescription");
			this.labelProxyBypassListHint.AccessibleName = resources.GetString("labelProxyBypassListHint.AccessibleName");
			this.labelProxyBypassListHint.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProxyBypassListHint.Anchor")));
			this.labelProxyBypassListHint.AutoSize = ((bool)(resources.GetObject("labelProxyBypassListHint.AutoSize")));
			this.labelProxyBypassListHint.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProxyBypassListHint.Dock")));
			this.labelProxyBypassListHint.Enabled = ((bool)(resources.GetObject("labelProxyBypassListHint.Enabled")));
			this.errorProvider1.SetError(this.labelProxyBypassListHint, resources.GetString("labelProxyBypassListHint.Error"));
			this.labelProxyBypassListHint.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelProxyBypassListHint.Font = ((System.Drawing.Font)(resources.GetObject("labelProxyBypassListHint.Font")));
			this.errorProvider1.SetIconAlignment(this.labelProxyBypassListHint, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelProxyBypassListHint.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelProxyBypassListHint, ((int)(resources.GetObject("labelProxyBypassListHint.IconPadding"))));
			this.labelProxyBypassListHint.Image = ((System.Drawing.Image)(resources.GetObject("labelProxyBypassListHint.Image")));
			this.labelProxyBypassListHint.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyBypassListHint.ImageAlign")));
			this.labelProxyBypassListHint.ImageIndex = ((int)(resources.GetObject("labelProxyBypassListHint.ImageIndex")));
			this.labelProxyBypassListHint.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProxyBypassListHint.ImeMode")));
			this.labelProxyBypassListHint.Location = ((System.Drawing.Point)(resources.GetObject("labelProxyBypassListHint.Location")));
			this.labelProxyBypassListHint.Name = "labelProxyBypassListHint";
			this.labelProxyBypassListHint.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProxyBypassListHint.RightToLeft")));
			this.labelProxyBypassListHint.Size = ((System.Drawing.Size)(resources.GetObject("labelProxyBypassListHint.Size")));
			this.labelProxyBypassListHint.TabIndex = ((int)(resources.GetObject("labelProxyBypassListHint.TabIndex")));
			this.labelProxyBypassListHint.Text = resources.GetString("labelProxyBypassListHint.Text");
			this.labelProxyBypassListHint.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyBypassListHint.TextAlign")));
			this.toolTip1.SetToolTip(this.labelProxyBypassListHint, resources.GetString("labelProxyBypassListHint.ToolTip"));
			this.labelProxyBypassListHint.Visible = ((bool)(resources.GetObject("labelProxyBypassListHint.Visible")));
			// 
			// labelProxyBypassList
			// 
			this.labelProxyBypassList.AccessibleDescription = resources.GetString("labelProxyBypassList.AccessibleDescription");
			this.labelProxyBypassList.AccessibleName = resources.GetString("labelProxyBypassList.AccessibleName");
			this.labelProxyBypassList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProxyBypassList.Anchor")));
			this.labelProxyBypassList.AutoSize = ((bool)(resources.GetObject("labelProxyBypassList.AutoSize")));
			this.labelProxyBypassList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProxyBypassList.Dock")));
			this.labelProxyBypassList.Enabled = ((bool)(resources.GetObject("labelProxyBypassList.Enabled")));
			this.errorProvider1.SetError(this.labelProxyBypassList, resources.GetString("labelProxyBypassList.Error"));
			this.labelProxyBypassList.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelProxyBypassList.Font = ((System.Drawing.Font)(resources.GetObject("labelProxyBypassList.Font")));
			this.errorProvider1.SetIconAlignment(this.labelProxyBypassList, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelProxyBypassList.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelProxyBypassList, ((int)(resources.GetObject("labelProxyBypassList.IconPadding"))));
			this.labelProxyBypassList.Image = ((System.Drawing.Image)(resources.GetObject("labelProxyBypassList.Image")));
			this.labelProxyBypassList.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyBypassList.ImageAlign")));
			this.labelProxyBypassList.ImageIndex = ((int)(resources.GetObject("labelProxyBypassList.ImageIndex")));
			this.labelProxyBypassList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProxyBypassList.ImeMode")));
			this.labelProxyBypassList.Location = ((System.Drawing.Point)(resources.GetObject("labelProxyBypassList.Location")));
			this.labelProxyBypassList.Name = "labelProxyBypassList";
			this.labelProxyBypassList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProxyBypassList.RightToLeft")));
			this.labelProxyBypassList.Size = ((System.Drawing.Size)(resources.GetObject("labelProxyBypassList.Size")));
			this.labelProxyBypassList.TabIndex = ((int)(resources.GetObject("labelProxyBypassList.TabIndex")));
			this.labelProxyBypassList.Text = resources.GetString("labelProxyBypassList.Text");
			this.labelProxyBypassList.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyBypassList.TextAlign")));
			this.toolTip1.SetToolTip(this.labelProxyBypassList, resources.GetString("labelProxyBypassList.ToolTip"));
			this.labelProxyBypassList.Visible = ((bool)(resources.GetObject("labelProxyBypassList.Visible")));
			// 
			// checkUseProxy
			// 
			this.checkUseProxy.AccessibleDescription = resources.GetString("checkUseProxy.AccessibleDescription");
			this.checkUseProxy.AccessibleName = resources.GetString("checkUseProxy.AccessibleName");
			this.checkUseProxy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkUseProxy.Anchor")));
			this.checkUseProxy.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkUseProxy.Appearance")));
			this.checkUseProxy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkUseProxy.BackgroundImage")));
			this.checkUseProxy.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseProxy.CheckAlign")));
			this.checkUseProxy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkUseProxy.Dock")));
			this.checkUseProxy.Enabled = ((bool)(resources.GetObject("checkUseProxy.Enabled")));
			this.errorProvider1.SetError(this.checkUseProxy, resources.GetString("checkUseProxy.Error"));
			this.checkUseProxy.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkUseProxy.FlatStyle")));
			this.checkUseProxy.Font = ((System.Drawing.Font)(resources.GetObject("checkUseProxy.Font")));
			this.errorProvider1.SetIconAlignment(this.checkUseProxy, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkUseProxy.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkUseProxy, ((int)(resources.GetObject("checkUseProxy.IconPadding"))));
			this.checkUseProxy.Image = ((System.Drawing.Image)(resources.GetObject("checkUseProxy.Image")));
			this.checkUseProxy.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseProxy.ImageAlign")));
			this.checkUseProxy.ImageIndex = ((int)(resources.GetObject("checkUseProxy.ImageIndex")));
			this.checkUseProxy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkUseProxy.ImeMode")));
			this.checkUseProxy.Location = ((System.Drawing.Point)(resources.GetObject("checkUseProxy.Location")));
			this.checkUseProxy.Name = "checkUseProxy";
			this.checkUseProxy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkUseProxy.RightToLeft")));
			this.checkUseProxy.Size = ((System.Drawing.Size)(resources.GetObject("checkUseProxy.Size")));
			this.checkUseProxy.TabIndex = ((int)(resources.GetObject("checkUseProxy.TabIndex")));
			this.checkUseProxy.Text = resources.GetString("checkUseProxy.Text");
			this.checkUseProxy.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseProxy.TextAlign")));
			this.toolTip1.SetToolTip(this.checkUseProxy, resources.GetString("checkUseProxy.ToolTip"));
			this.checkUseProxy.Visible = ((bool)(resources.GetObject("checkUseProxy.Visible")));
			this.checkUseProxy.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkUseProxy.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkUseProxy.CheckedChanged += new System.EventHandler(this.checkUseProxy_CheckedChanged);
			// 
			// checkUseIEProxySettings
			// 
			this.checkUseIEProxySettings.AccessibleDescription = resources.GetString("checkUseIEProxySettings.AccessibleDescription");
			this.checkUseIEProxySettings.AccessibleName = resources.GetString("checkUseIEProxySettings.AccessibleName");
			this.checkUseIEProxySettings.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkUseIEProxySettings.Anchor")));
			this.checkUseIEProxySettings.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkUseIEProxySettings.Appearance")));
			this.checkUseIEProxySettings.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkUseIEProxySettings.BackgroundImage")));
			this.checkUseIEProxySettings.CausesValidation = false;
			this.checkUseIEProxySettings.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseIEProxySettings.CheckAlign")));
			this.checkUseIEProxySettings.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkUseIEProxySettings.Dock")));
			this.checkUseIEProxySettings.Enabled = ((bool)(resources.GetObject("checkUseIEProxySettings.Enabled")));
			this.errorProvider1.SetError(this.checkUseIEProxySettings, resources.GetString("checkUseIEProxySettings.Error"));
			this.checkUseIEProxySettings.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkUseIEProxySettings.FlatStyle")));
			this.checkUseIEProxySettings.Font = ((System.Drawing.Font)(resources.GetObject("checkUseIEProxySettings.Font")));
			this.errorProvider1.SetIconAlignment(this.checkUseIEProxySettings, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkUseIEProxySettings.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkUseIEProxySettings, ((int)(resources.GetObject("checkUseIEProxySettings.IconPadding"))));
			this.checkUseIEProxySettings.Image = ((System.Drawing.Image)(resources.GetObject("checkUseIEProxySettings.Image")));
			this.checkUseIEProxySettings.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseIEProxySettings.ImageAlign")));
			this.checkUseIEProxySettings.ImageIndex = ((int)(resources.GetObject("checkUseIEProxySettings.ImageIndex")));
			this.checkUseIEProxySettings.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkUseIEProxySettings.ImeMode")));
			this.checkUseIEProxySettings.Location = ((System.Drawing.Point)(resources.GetObject("checkUseIEProxySettings.Location")));
			this.checkUseIEProxySettings.Name = "checkUseIEProxySettings";
			this.checkUseIEProxySettings.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkUseIEProxySettings.RightToLeft")));
			this.checkUseIEProxySettings.Size = ((System.Drawing.Size)(resources.GetObject("checkUseIEProxySettings.Size")));
			this.checkUseIEProxySettings.TabIndex = ((int)(resources.GetObject("checkUseIEProxySettings.TabIndex")));
			this.checkUseIEProxySettings.Text = resources.GetString("checkUseIEProxySettings.Text");
			this.checkUseIEProxySettings.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseIEProxySettings.TextAlign")));
			this.toolTip1.SetToolTip(this.checkUseIEProxySettings, resources.GetString("checkUseIEProxySettings.ToolTip"));
			this.checkUseIEProxySettings.Visible = ((bool)(resources.GetObject("checkUseIEProxySettings.Visible")));
			this.checkUseIEProxySettings.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkUseIEProxySettings.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkUseIEProxySettings.CheckedChanged += new System.EventHandler(this.checkUseProxy_CheckedChanged);
			// 
			// labelProxyServerSummery
			// 
			this.labelProxyServerSummery.AccessibleDescription = resources.GetString("labelProxyServerSummery.AccessibleDescription");
			this.labelProxyServerSummery.AccessibleName = resources.GetString("labelProxyServerSummery.AccessibleName");
			this.labelProxyServerSummery.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProxyServerSummery.Anchor")));
			this.labelProxyServerSummery.AutoSize = ((bool)(resources.GetObject("labelProxyServerSummery.AutoSize")));
			this.labelProxyServerSummery.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProxyServerSummery.Dock")));
			this.labelProxyServerSummery.Enabled = ((bool)(resources.GetObject("labelProxyServerSummery.Enabled")));
			this.errorProvider1.SetError(this.labelProxyServerSummery, resources.GetString("labelProxyServerSummery.Error"));
			this.labelProxyServerSummery.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelProxyServerSummery.Font = ((System.Drawing.Font)(resources.GetObject("labelProxyServerSummery.Font")));
			this.errorProvider1.SetIconAlignment(this.labelProxyServerSummery, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelProxyServerSummery.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelProxyServerSummery, ((int)(resources.GetObject("labelProxyServerSummery.IconPadding"))));
			this.labelProxyServerSummery.Image = ((System.Drawing.Image)(resources.GetObject("labelProxyServerSummery.Image")));
			this.labelProxyServerSummery.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyServerSummery.ImageAlign")));
			this.labelProxyServerSummery.ImageIndex = ((int)(resources.GetObject("labelProxyServerSummery.ImageIndex")));
			this.labelProxyServerSummery.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProxyServerSummery.ImeMode")));
			this.labelProxyServerSummery.Location = ((System.Drawing.Point)(resources.GetObject("labelProxyServerSummery.Location")));
			this.labelProxyServerSummery.Name = "labelProxyServerSummery";
			this.labelProxyServerSummery.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProxyServerSummery.RightToLeft")));
			this.labelProxyServerSummery.Size = ((System.Drawing.Size)(resources.GetObject("labelProxyServerSummery.Size")));
			this.labelProxyServerSummery.TabIndex = ((int)(resources.GetObject("labelProxyServerSummery.TabIndex")));
			this.labelProxyServerSummery.Text = resources.GetString("labelProxyServerSummery.Text");
			this.labelProxyServerSummery.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyServerSummery.TextAlign")));
			this.toolTip1.SetToolTip(this.labelProxyServerSummery, resources.GetString("labelProxyServerSummery.ToolTip"));
			this.labelProxyServerSummery.Visible = ((bool)(resources.GetObject("labelProxyServerSummery.Visible")));
			// 
			// textProxyPort
			// 
			this.textProxyPort.AccessibleDescription = resources.GetString("textProxyPort.AccessibleDescription");
			this.textProxyPort.AccessibleName = resources.GetString("textProxyPort.AccessibleName");
			this.textProxyPort.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textProxyPort.Anchor")));
			this.textProxyPort.AutoSize = ((bool)(resources.GetObject("textProxyPort.AutoSize")));
			this.textProxyPort.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textProxyPort.BackgroundImage")));
			this.textProxyPort.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textProxyPort.Dock")));
			this.textProxyPort.Enabled = ((bool)(resources.GetObject("textProxyPort.Enabled")));
			this.errorProvider1.SetError(this.textProxyPort, resources.GetString("textProxyPort.Error"));
			this.textProxyPort.Font = ((System.Drawing.Font)(resources.GetObject("textProxyPort.Font")));
			this.errorProvider1.SetIconAlignment(this.textProxyPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyPort.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textProxyPort, ((int)(resources.GetObject("textProxyPort.IconPadding"))));
			this.textProxyPort.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textProxyPort.ImeMode")));
			this.textProxyPort.Location = ((System.Drawing.Point)(resources.GetObject("textProxyPort.Location")));
			this.textProxyPort.MaxLength = ((int)(resources.GetObject("textProxyPort.MaxLength")));
			this.textProxyPort.Multiline = ((bool)(resources.GetObject("textProxyPort.Multiline")));
			this.textProxyPort.Name = "textProxyPort";
			this.textProxyPort.PasswordChar = ((char)(resources.GetObject("textProxyPort.PasswordChar")));
			this.textProxyPort.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textProxyPort.RightToLeft")));
			this.textProxyPort.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textProxyPort.ScrollBars")));
			this.textProxyPort.Size = ((System.Drawing.Size)(resources.GetObject("textProxyPort.Size")));
			this.textProxyPort.TabIndex = ((int)(resources.GetObject("textProxyPort.TabIndex")));
			this.textProxyPort.Text = resources.GetString("textProxyPort.Text");
			this.textProxyPort.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textProxyPort.TextAlign")));
			this.toolTip1.SetToolTip(this.textProxyPort, resources.GetString("textProxyPort.ToolTip"));
			this.textProxyPort.Visible = ((bool)(resources.GetObject("textProxyPort.Visible")));
			this.textProxyPort.WordWrap = ((bool)(resources.GetObject("textProxyPort.WordWrap")));
			this.textProxyPort.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.textProxyPort.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// labelProxyPort
			// 
			this.labelProxyPort.AccessibleDescription = resources.GetString("labelProxyPort.AccessibleDescription");
			this.labelProxyPort.AccessibleName = resources.GetString("labelProxyPort.AccessibleName");
			this.labelProxyPort.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProxyPort.Anchor")));
			this.labelProxyPort.AutoSize = ((bool)(resources.GetObject("labelProxyPort.AutoSize")));
			this.labelProxyPort.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProxyPort.Dock")));
			this.labelProxyPort.Enabled = ((bool)(resources.GetObject("labelProxyPort.Enabled")));
			this.errorProvider1.SetError(this.labelProxyPort, resources.GetString("labelProxyPort.Error"));
			this.labelProxyPort.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelProxyPort.Font = ((System.Drawing.Font)(resources.GetObject("labelProxyPort.Font")));
			this.errorProvider1.SetIconAlignment(this.labelProxyPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelProxyPort.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelProxyPort, ((int)(resources.GetObject("labelProxyPort.IconPadding"))));
			this.labelProxyPort.Image = ((System.Drawing.Image)(resources.GetObject("labelProxyPort.Image")));
			this.labelProxyPort.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyPort.ImageAlign")));
			this.labelProxyPort.ImageIndex = ((int)(resources.GetObject("labelProxyPort.ImageIndex")));
			this.labelProxyPort.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProxyPort.ImeMode")));
			this.labelProxyPort.Location = ((System.Drawing.Point)(resources.GetObject("labelProxyPort.Location")));
			this.labelProxyPort.Name = "labelProxyPort";
			this.labelProxyPort.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProxyPort.RightToLeft")));
			this.labelProxyPort.Size = ((System.Drawing.Size)(resources.GetObject("labelProxyPort.Size")));
			this.labelProxyPort.TabIndex = ((int)(resources.GetObject("labelProxyPort.TabIndex")));
			this.labelProxyPort.Text = resources.GetString("labelProxyPort.Text");
			this.labelProxyPort.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyPort.TextAlign")));
			this.toolTip1.SetToolTip(this.labelProxyPort, resources.GetString("labelProxyPort.ToolTip"));
			this.labelProxyPort.Visible = ((bool)(resources.GetObject("labelProxyPort.Visible")));
			// 
			// labelProxyAddress
			// 
			this.labelProxyAddress.AccessibleDescription = resources.GetString("labelProxyAddress.AccessibleDescription");
			this.labelProxyAddress.AccessibleName = resources.GetString("labelProxyAddress.AccessibleName");
			this.labelProxyAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProxyAddress.Anchor")));
			this.labelProxyAddress.AutoSize = ((bool)(resources.GetObject("labelProxyAddress.AutoSize")));
			this.labelProxyAddress.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProxyAddress.Dock")));
			this.labelProxyAddress.Enabled = ((bool)(resources.GetObject("labelProxyAddress.Enabled")));
			this.errorProvider1.SetError(this.labelProxyAddress, resources.GetString("labelProxyAddress.Error"));
			this.labelProxyAddress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelProxyAddress.Font = ((System.Drawing.Font)(resources.GetObject("labelProxyAddress.Font")));
			this.errorProvider1.SetIconAlignment(this.labelProxyAddress, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelProxyAddress.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelProxyAddress, ((int)(resources.GetObject("labelProxyAddress.IconPadding"))));
			this.labelProxyAddress.Image = ((System.Drawing.Image)(resources.GetObject("labelProxyAddress.Image")));
			this.labelProxyAddress.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyAddress.ImageAlign")));
			this.labelProxyAddress.ImageIndex = ((int)(resources.GetObject("labelProxyAddress.ImageIndex")));
			this.labelProxyAddress.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProxyAddress.ImeMode")));
			this.labelProxyAddress.Location = ((System.Drawing.Point)(resources.GetObject("labelProxyAddress.Location")));
			this.labelProxyAddress.Name = "labelProxyAddress";
			this.labelProxyAddress.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProxyAddress.RightToLeft")));
			this.labelProxyAddress.Size = ((System.Drawing.Size)(resources.GetObject("labelProxyAddress.Size")));
			this.labelProxyAddress.TabIndex = ((int)(resources.GetObject("labelProxyAddress.TabIndex")));
			this.labelProxyAddress.Text = resources.GetString("labelProxyAddress.Text");
			this.labelProxyAddress.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProxyAddress.TextAlign")));
			this.toolTip1.SetToolTip(this.labelProxyAddress, resources.GetString("labelProxyAddress.ToolTip"));
			this.labelProxyAddress.Visible = ((bool)(resources.GetObject("labelProxyAddress.Visible")));
			// 
			// textProxyAddress
			// 
			this.textProxyAddress.AccessibleDescription = resources.GetString("textProxyAddress.AccessibleDescription");
			this.textProxyAddress.AccessibleName = resources.GetString("textProxyAddress.AccessibleName");
			this.textProxyAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textProxyAddress.Anchor")));
			this.textProxyAddress.AutoSize = ((bool)(resources.GetObject("textProxyAddress.AutoSize")));
			this.textProxyAddress.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textProxyAddress.BackgroundImage")));
			this.textProxyAddress.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textProxyAddress.Dock")));
			this.textProxyAddress.Enabled = ((bool)(resources.GetObject("textProxyAddress.Enabled")));
			this.errorProvider1.SetError(this.textProxyAddress, resources.GetString("textProxyAddress.Error"));
			this.textProxyAddress.Font = ((System.Drawing.Font)(resources.GetObject("textProxyAddress.Font")));
			this.errorProvider1.SetIconAlignment(this.textProxyAddress, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("textProxyAddress.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.textProxyAddress, ((int)(resources.GetObject("textProxyAddress.IconPadding"))));
			this.textProxyAddress.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textProxyAddress.ImeMode")));
			this.textProxyAddress.Location = ((System.Drawing.Point)(resources.GetObject("textProxyAddress.Location")));
			this.textProxyAddress.MaxLength = ((int)(resources.GetObject("textProxyAddress.MaxLength")));
			this.textProxyAddress.Multiline = ((bool)(resources.GetObject("textProxyAddress.Multiline")));
			this.textProxyAddress.Name = "textProxyAddress";
			this.textProxyAddress.PasswordChar = ((char)(resources.GetObject("textProxyAddress.PasswordChar")));
			this.textProxyAddress.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textProxyAddress.RightToLeft")));
			this.textProxyAddress.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textProxyAddress.ScrollBars")));
			this.textProxyAddress.Size = ((System.Drawing.Size)(resources.GetObject("textProxyAddress.Size")));
			this.textProxyAddress.TabIndex = ((int)(resources.GetObject("textProxyAddress.TabIndex")));
			this.textProxyAddress.Text = resources.GetString("textProxyAddress.Text");
			this.textProxyAddress.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textProxyAddress.TextAlign")));
			this.toolTip1.SetToolTip(this.textProxyAddress, resources.GetString("textProxyAddress.ToolTip"));
			this.textProxyAddress.Visible = ((bool)(resources.GetObject("textProxyAddress.Visible")));
			this.textProxyAddress.WordWrap = ((bool)(resources.GetObject("textProxyAddress.WordWrap")));
			this.textProxyAddress.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.textProxyAddress.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// checkProxyBypassLocal
			// 
			this.checkProxyBypassLocal.AccessibleDescription = resources.GetString("checkProxyBypassLocal.AccessibleDescription");
			this.checkProxyBypassLocal.AccessibleName = resources.GetString("checkProxyBypassLocal.AccessibleName");
			this.checkProxyBypassLocal.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkProxyBypassLocal.Anchor")));
			this.checkProxyBypassLocal.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkProxyBypassLocal.Appearance")));
			this.checkProxyBypassLocal.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkProxyBypassLocal.BackgroundImage")));
			this.checkProxyBypassLocal.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkProxyBypassLocal.CheckAlign")));
			this.checkProxyBypassLocal.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkProxyBypassLocal.Dock")));
			this.checkProxyBypassLocal.Enabled = ((bool)(resources.GetObject("checkProxyBypassLocal.Enabled")));
			this.errorProvider1.SetError(this.checkProxyBypassLocal, resources.GetString("checkProxyBypassLocal.Error"));
			this.checkProxyBypassLocal.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkProxyBypassLocal.FlatStyle")));
			this.checkProxyBypassLocal.Font = ((System.Drawing.Font)(resources.GetObject("checkProxyBypassLocal.Font")));
			this.errorProvider1.SetIconAlignment(this.checkProxyBypassLocal, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkProxyBypassLocal.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkProxyBypassLocal, ((int)(resources.GetObject("checkProxyBypassLocal.IconPadding"))));
			this.checkProxyBypassLocal.Image = ((System.Drawing.Image)(resources.GetObject("checkProxyBypassLocal.Image")));
			this.checkProxyBypassLocal.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkProxyBypassLocal.ImageAlign")));
			this.checkProxyBypassLocal.ImageIndex = ((int)(resources.GetObject("checkProxyBypassLocal.ImageIndex")));
			this.checkProxyBypassLocal.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkProxyBypassLocal.ImeMode")));
			this.checkProxyBypassLocal.Location = ((System.Drawing.Point)(resources.GetObject("checkProxyBypassLocal.Location")));
			this.checkProxyBypassLocal.Name = "checkProxyBypassLocal";
			this.checkProxyBypassLocal.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkProxyBypassLocal.RightToLeft")));
			this.checkProxyBypassLocal.Size = ((System.Drawing.Size)(resources.GetObject("checkProxyBypassLocal.Size")));
			this.checkProxyBypassLocal.TabIndex = ((int)(resources.GetObject("checkProxyBypassLocal.TabIndex")));
			this.checkProxyBypassLocal.Text = resources.GetString("checkProxyBypassLocal.Text");
			this.checkProxyBypassLocal.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkProxyBypassLocal.TextAlign")));
			this.toolTip1.SetToolTip(this.checkProxyBypassLocal, resources.GetString("checkProxyBypassLocal.ToolTip"));
			this.checkProxyBypassLocal.Visible = ((bool)(resources.GetObject("checkProxyBypassLocal.Visible")));
			this.checkProxyBypassLocal.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkProxyBypassLocal.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkProxyBypassLocal.CheckedChanged += new System.EventHandler(this.checkProxyBypassLocal_CheckedChanged);
			// 
			// tabFonts
			// 
			this.tabFonts.AccessibleDescription = resources.GetString("tabFonts.AccessibleDescription");
			this.tabFonts.AccessibleName = resources.GetString("tabFonts.AccessibleName");
			this.tabFonts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabFonts.Anchor")));
			this.tabFonts.AutoScroll = ((bool)(resources.GetObject("tabFonts.AutoScroll")));
			this.tabFonts.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabFonts.AutoScrollMargin")));
			this.tabFonts.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabFonts.AutoScrollMinSize")));
			this.tabFonts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabFonts.BackgroundImage")));
			this.tabFonts.Controls.Add(this.sectionPanelFontsSubscriptions);
			this.tabFonts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabFonts.Dock")));
			this.tabFonts.Enabled = ((bool)(resources.GetObject("tabFonts.Enabled")));
			this.errorProvider1.SetError(this.tabFonts, resources.GetString("tabFonts.Error"));
			this.tabFonts.Font = ((System.Drawing.Font)(resources.GetObject("tabFonts.Font")));
			this.errorProvider1.SetIconAlignment(this.tabFonts, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabFonts.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabFonts, ((int)(resources.GetObject("tabFonts.IconPadding"))));
			this.tabFonts.ImageIndex = ((int)(resources.GetObject("tabFonts.ImageIndex")));
			this.tabFonts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabFonts.ImeMode")));
			this.tabFonts.Location = ((System.Drawing.Point)(resources.GetObject("tabFonts.Location")));
			this.tabFonts.Name = "tabFonts";
			this.tabFonts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabFonts.RightToLeft")));
			this.tabFonts.Size = ((System.Drawing.Size)(resources.GetObject("tabFonts.Size")));
			this.tabFonts.TabIndex = ((int)(resources.GetObject("tabFonts.TabIndex")));
			this.tabFonts.Text = resources.GetString("tabFonts.Text");
			this.toolTip1.SetToolTip(this.tabFonts, resources.GetString("tabFonts.ToolTip"));
			this.tabFonts.ToolTipText = resources.GetString("tabFonts.ToolTipText");
			this.tabFonts.Visible = ((bool)(resources.GetObject("tabFonts.Visible")));
			// 
			// sectionPanelFontsSubscriptions
			// 
			this.sectionPanelFontsSubscriptions.AccessibleDescription = resources.GetString("sectionPanelFontsSubscriptions.AccessibleDescription");
			this.sectionPanelFontsSubscriptions.AccessibleName = resources.GetString("sectionPanelFontsSubscriptions.AccessibleName");
			this.sectionPanelFontsSubscriptions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelFontsSubscriptions.Anchor")));
			this.sectionPanelFontsSubscriptions.AutoScroll = ((bool)(resources.GetObject("sectionPanelFontsSubscriptions.AutoScroll")));
			this.sectionPanelFontsSubscriptions.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelFontsSubscriptions.AutoScrollMargin")));
			this.sectionPanelFontsSubscriptions.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelFontsSubscriptions.AutoScrollMinSize")));
			this.sectionPanelFontsSubscriptions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelFontsSubscriptions.BackgroundImage")));
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
			this.sectionPanelFontsSubscriptions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelFontsSubscriptions.Dock")));
			this.sectionPanelFontsSubscriptions.Enabled = ((bool)(resources.GetObject("sectionPanelFontsSubscriptions.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelFontsSubscriptions, resources.GetString("sectionPanelFontsSubscriptions.Error"));
			this.sectionPanelFontsSubscriptions.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelFontsSubscriptions.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelFontsSubscriptions, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelFontsSubscriptions.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelFontsSubscriptions, ((int)(resources.GetObject("sectionPanelFontsSubscriptions.IconPadding"))));
			this.sectionPanelFontsSubscriptions.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelFontsSubscriptions.Image")));
			this.sectionPanelFontsSubscriptions.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelFontsSubscriptions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelFontsSubscriptions.ImeMode")));
			this.sectionPanelFontsSubscriptions.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelFontsSubscriptions.Location")));
			this.sectionPanelFontsSubscriptions.Name = "sectionPanelFontsSubscriptions";
			this.sectionPanelFontsSubscriptions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelFontsSubscriptions.RightToLeft")));
			this.sectionPanelFontsSubscriptions.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelFontsSubscriptions.Size")));
			this.sectionPanelFontsSubscriptions.TabIndex = ((int)(resources.GetObject("sectionPanelFontsSubscriptions.TabIndex")));
			this.sectionPanelFontsSubscriptions.Text = resources.GetString("sectionPanelFontsSubscriptions.Text");
			this.toolTip1.SetToolTip(this.sectionPanelFontsSubscriptions, resources.GetString("sectionPanelFontsSubscriptions.ToolTip"));
			this.sectionPanelFontsSubscriptions.Visible = ((bool)(resources.GetObject("sectionPanelFontsSubscriptions.Visible")));
			// 
			// labelFontsSubscriptionsSummery
			// 
			this.labelFontsSubscriptionsSummery.AccessibleDescription = resources.GetString("labelFontsSubscriptionsSummery.AccessibleDescription");
			this.labelFontsSubscriptionsSummery.AccessibleName = resources.GetString("labelFontsSubscriptionsSummery.AccessibleName");
			this.labelFontsSubscriptionsSummery.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelFontsSubscriptionsSummery.Anchor")));
			this.labelFontsSubscriptionsSummery.AutoSize = ((bool)(resources.GetObject("labelFontsSubscriptionsSummery.AutoSize")));
			this.labelFontsSubscriptionsSummery.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelFontsSubscriptionsSummery.Dock")));
			this.labelFontsSubscriptionsSummery.Enabled = ((bool)(resources.GetObject("labelFontsSubscriptionsSummery.Enabled")));
			this.errorProvider1.SetError(this.labelFontsSubscriptionsSummery, resources.GetString("labelFontsSubscriptionsSummery.Error"));
			this.labelFontsSubscriptionsSummery.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelFontsSubscriptionsSummery.Font = ((System.Drawing.Font)(resources.GetObject("labelFontsSubscriptionsSummery.Font")));
			this.errorProvider1.SetIconAlignment(this.labelFontsSubscriptionsSummery, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelFontsSubscriptionsSummery.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelFontsSubscriptionsSummery, ((int)(resources.GetObject("labelFontsSubscriptionsSummery.IconPadding"))));
			this.labelFontsSubscriptionsSummery.Image = ((System.Drawing.Image)(resources.GetObject("labelFontsSubscriptionsSummery.Image")));
			this.labelFontsSubscriptionsSummery.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFontsSubscriptionsSummery.ImageAlign")));
			this.labelFontsSubscriptionsSummery.ImageIndex = ((int)(resources.GetObject("labelFontsSubscriptionsSummery.ImageIndex")));
			this.labelFontsSubscriptionsSummery.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelFontsSubscriptionsSummery.ImeMode")));
			this.labelFontsSubscriptionsSummery.Location = ((System.Drawing.Point)(resources.GetObject("labelFontsSubscriptionsSummery.Location")));
			this.labelFontsSubscriptionsSummery.Name = "labelFontsSubscriptionsSummery";
			this.labelFontsSubscriptionsSummery.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelFontsSubscriptionsSummery.RightToLeft")));
			this.labelFontsSubscriptionsSummery.Size = ((System.Drawing.Size)(resources.GetObject("labelFontsSubscriptionsSummery.Size")));
			this.labelFontsSubscriptionsSummery.TabIndex = ((int)(resources.GetObject("labelFontsSubscriptionsSummery.TabIndex")));
			this.labelFontsSubscriptionsSummery.Text = resources.GetString("labelFontsSubscriptionsSummery.Text");
			this.labelFontsSubscriptionsSummery.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFontsSubscriptionsSummery.TextAlign")));
			this.toolTip1.SetToolTip(this.labelFontsSubscriptionsSummery, resources.GetString("labelFontsSubscriptionsSummery.ToolTip"));
			this.labelFontsSubscriptionsSummery.Visible = ((bool)(resources.GetObject("labelFontsSubscriptionsSummery.Visible")));
			// 
			// btnChangeFont
			// 
			this.btnChangeFont.AccessibleDescription = resources.GetString("btnChangeFont.AccessibleDescription");
			this.btnChangeFont.AccessibleName = resources.GetString("btnChangeFont.AccessibleName");
			this.btnChangeFont.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnChangeFont.Anchor")));
			this.btnChangeFont.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnChangeFont.BackgroundImage")));
			this.btnChangeFont.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnChangeFont.Dock")));
			this.btnChangeFont.Enabled = ((bool)(resources.GetObject("btnChangeFont.Enabled")));
			this.errorProvider1.SetError(this.btnChangeFont, resources.GetString("btnChangeFont.Error"));
			this.btnChangeFont.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnChangeFont.FlatStyle")));
			this.btnChangeFont.Font = ((System.Drawing.Font)(resources.GetObject("btnChangeFont.Font")));
			this.errorProvider1.SetIconAlignment(this.btnChangeFont, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnChangeFont.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnChangeFont, ((int)(resources.GetObject("btnChangeFont.IconPadding"))));
			this.btnChangeFont.Image = ((System.Drawing.Image)(resources.GetObject("btnChangeFont.Image")));
			this.btnChangeFont.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnChangeFont.ImageAlign")));
			this.btnChangeFont.ImageIndex = ((int)(resources.GetObject("btnChangeFont.ImageIndex")));
			this.btnChangeFont.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnChangeFont.ImeMode")));
			this.btnChangeFont.Location = ((System.Drawing.Point)(resources.GetObject("btnChangeFont.Location")));
			this.btnChangeFont.Name = "btnChangeFont";
			this.btnChangeFont.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnChangeFont.RightToLeft")));
			this.btnChangeFont.Size = ((System.Drawing.Size)(resources.GetObject("btnChangeFont.Size")));
			this.btnChangeFont.TabIndex = ((int)(resources.GetObject("btnChangeFont.TabIndex")));
			this.btnChangeFont.Text = resources.GetString("btnChangeFont.Text");
			this.btnChangeFont.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnChangeFont.TextAlign")));
			this.toolTip1.SetToolTip(this.btnChangeFont, resources.GetString("btnChangeFont.ToolTip"));
			this.btnChangeFont.Visible = ((bool)(resources.GetObject("btnChangeFont.Visible")));
			this.btnChangeFont.Click += new System.EventHandler(this.OnDefaultFontChangeClick);
			// 
			// lblUsedFontNameSize
			// 
			this.lblUsedFontNameSize.AccessibleDescription = resources.GetString("lblUsedFontNameSize.AccessibleDescription");
			this.lblUsedFontNameSize.AccessibleName = resources.GetString("lblUsedFontNameSize.AccessibleName");
			this.lblUsedFontNameSize.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblUsedFontNameSize.Anchor")));
			this.lblUsedFontNameSize.AutoSize = ((bool)(resources.GetObject("lblUsedFontNameSize.AutoSize")));
			this.lblUsedFontNameSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblUsedFontNameSize.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblUsedFontNameSize.Dock")));
			this.lblUsedFontNameSize.Enabled = ((bool)(resources.GetObject("lblUsedFontNameSize.Enabled")));
			this.errorProvider1.SetError(this.lblUsedFontNameSize, resources.GetString("lblUsedFontNameSize.Error"));
			this.lblUsedFontNameSize.Font = ((System.Drawing.Font)(resources.GetObject("lblUsedFontNameSize.Font")));
			this.errorProvider1.SetIconAlignment(this.lblUsedFontNameSize, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lblUsedFontNameSize.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.lblUsedFontNameSize, ((int)(resources.GetObject("lblUsedFontNameSize.IconPadding"))));
			this.lblUsedFontNameSize.Image = ((System.Drawing.Image)(resources.GetObject("lblUsedFontNameSize.Image")));
			this.lblUsedFontNameSize.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUsedFontNameSize.ImageAlign")));
			this.lblUsedFontNameSize.ImageIndex = ((int)(resources.GetObject("lblUsedFontNameSize.ImageIndex")));
			this.lblUsedFontNameSize.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblUsedFontNameSize.ImeMode")));
			this.lblUsedFontNameSize.Location = ((System.Drawing.Point)(resources.GetObject("lblUsedFontNameSize.Location")));
			this.lblUsedFontNameSize.Name = "lblUsedFontNameSize";
			this.lblUsedFontNameSize.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblUsedFontNameSize.RightToLeft")));
			this.lblUsedFontNameSize.Size = ((System.Drawing.Size)(resources.GetObject("lblUsedFontNameSize.Size")));
			this.lblUsedFontNameSize.TabIndex = ((int)(resources.GetObject("lblUsedFontNameSize.TabIndex")));
			this.lblUsedFontNameSize.Text = resources.GetString("lblUsedFontNameSize.Text");
			this.lblUsedFontNameSize.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUsedFontNameSize.TextAlign")));
			this.toolTip1.SetToolTip(this.lblUsedFontNameSize, resources.GetString("lblUsedFontNameSize.ToolTip"));
			this.lblUsedFontNameSize.Visible = ((bool)(resources.GetObject("lblUsedFontNameSize.Visible")));
			// 
			// chkFontItalic
			// 
			this.chkFontItalic.AccessibleDescription = resources.GetString("chkFontItalic.AccessibleDescription");
			this.chkFontItalic.AccessibleName = resources.GetString("chkFontItalic.AccessibleName");
			this.chkFontItalic.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkFontItalic.Anchor")));
			this.chkFontItalic.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkFontItalic.Appearance")));
			this.chkFontItalic.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkFontItalic.BackgroundImage")));
			this.chkFontItalic.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontItalic.CheckAlign")));
			this.chkFontItalic.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkFontItalic.Dock")));
			this.chkFontItalic.Enabled = ((bool)(resources.GetObject("chkFontItalic.Enabled")));
			this.errorProvider1.SetError(this.chkFontItalic, resources.GetString("chkFontItalic.Error"));
			this.chkFontItalic.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkFontItalic.FlatStyle")));
			this.chkFontItalic.Font = ((System.Drawing.Font)(resources.GetObject("chkFontItalic.Font")));
			this.errorProvider1.SetIconAlignment(this.chkFontItalic, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkFontItalic.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.chkFontItalic, ((int)(resources.GetObject("chkFontItalic.IconPadding"))));
			this.chkFontItalic.Image = ((System.Drawing.Image)(resources.GetObject("chkFontItalic.Image")));
			this.chkFontItalic.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontItalic.ImageAlign")));
			this.chkFontItalic.ImageIndex = ((int)(resources.GetObject("chkFontItalic.ImageIndex")));
			this.chkFontItalic.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkFontItalic.ImeMode")));
			this.chkFontItalic.Location = ((System.Drawing.Point)(resources.GetObject("chkFontItalic.Location")));
			this.chkFontItalic.Name = "chkFontItalic";
			this.chkFontItalic.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkFontItalic.RightToLeft")));
			this.chkFontItalic.Size = ((System.Drawing.Size)(resources.GetObject("chkFontItalic.Size")));
			this.chkFontItalic.TabIndex = ((int)(resources.GetObject("chkFontItalic.TabIndex")));
			this.chkFontItalic.Text = resources.GetString("chkFontItalic.Text");
			this.chkFontItalic.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontItalic.TextAlign")));
			this.toolTip1.SetToolTip(this.chkFontItalic, resources.GetString("chkFontItalic.ToolTip"));
			this.chkFontItalic.Visible = ((bool)(resources.GetObject("chkFontItalic.Visible")));
			this.chkFontItalic.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
			// 
			// chkFontBold
			// 
			this.chkFontBold.AccessibleDescription = resources.GetString("chkFontBold.AccessibleDescription");
			this.chkFontBold.AccessibleName = resources.GetString("chkFontBold.AccessibleName");
			this.chkFontBold.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkFontBold.Anchor")));
			this.chkFontBold.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkFontBold.Appearance")));
			this.chkFontBold.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkFontBold.BackgroundImage")));
			this.chkFontBold.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontBold.CheckAlign")));
			this.chkFontBold.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkFontBold.Dock")));
			this.chkFontBold.Enabled = ((bool)(resources.GetObject("chkFontBold.Enabled")));
			this.errorProvider1.SetError(this.chkFontBold, resources.GetString("chkFontBold.Error"));
			this.chkFontBold.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkFontBold.FlatStyle")));
			this.chkFontBold.Font = ((System.Drawing.Font)(resources.GetObject("chkFontBold.Font")));
			this.errorProvider1.SetIconAlignment(this.chkFontBold, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkFontBold.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.chkFontBold, ((int)(resources.GetObject("chkFontBold.IconPadding"))));
			this.chkFontBold.Image = ((System.Drawing.Image)(resources.GetObject("chkFontBold.Image")));
			this.chkFontBold.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontBold.ImageAlign")));
			this.chkFontBold.ImageIndex = ((int)(resources.GetObject("chkFontBold.ImageIndex")));
			this.chkFontBold.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkFontBold.ImeMode")));
			this.chkFontBold.Location = ((System.Drawing.Point)(resources.GetObject("chkFontBold.Location")));
			this.chkFontBold.Name = "chkFontBold";
			this.chkFontBold.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkFontBold.RightToLeft")));
			this.chkFontBold.Size = ((System.Drawing.Size)(resources.GetObject("chkFontBold.Size")));
			this.chkFontBold.TabIndex = ((int)(resources.GetObject("chkFontBold.TabIndex")));
			this.chkFontBold.Text = resources.GetString("chkFontBold.Text");
			this.chkFontBold.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontBold.TextAlign")));
			this.toolTip1.SetToolTip(this.chkFontBold, resources.GetString("chkFontBold.ToolTip"));
			this.chkFontBold.Visible = ((bool)(resources.GetObject("chkFontBold.Visible")));
			this.chkFontBold.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
			// 
			// btnChangeColor
			// 
			this.btnChangeColor.AccessibleDescription = resources.GetString("btnChangeColor.AccessibleDescription");
			this.btnChangeColor.AccessibleName = resources.GetString("btnChangeColor.AccessibleName");
			this.btnChangeColor.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnChangeColor.Anchor")));
			this.btnChangeColor.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnChangeColor.BackgroundImage")));
			this.btnChangeColor.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnChangeColor.Dock")));
			this.btnChangeColor.Enabled = ((bool)(resources.GetObject("btnChangeColor.Enabled")));
			this.errorProvider1.SetError(this.btnChangeColor, resources.GetString("btnChangeColor.Error"));
			this.btnChangeColor.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnChangeColor.FlatStyle")));
			this.btnChangeColor.Font = ((System.Drawing.Font)(resources.GetObject("btnChangeColor.Font")));
			this.errorProvider1.SetIconAlignment(this.btnChangeColor, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnChangeColor.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnChangeColor, ((int)(resources.GetObject("btnChangeColor.IconPadding"))));
			this.btnChangeColor.Image = ((System.Drawing.Image)(resources.GetObject("btnChangeColor.Image")));
			this.btnChangeColor.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnChangeColor.ImageAlign")));
			this.btnChangeColor.ImageIndex = ((int)(resources.GetObject("btnChangeColor.ImageIndex")));
			this.btnChangeColor.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnChangeColor.ImeMode")));
			this.btnChangeColor.Location = ((System.Drawing.Point)(resources.GetObject("btnChangeColor.Location")));
			this.btnChangeColor.Name = "btnChangeColor";
			this.btnChangeColor.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnChangeColor.RightToLeft")));
			this.btnChangeColor.Size = ((System.Drawing.Size)(resources.GetObject("btnChangeColor.Size")));
			this.btnChangeColor.TabIndex = ((int)(resources.GetObject("btnChangeColor.TabIndex")));
			this.btnChangeColor.Text = resources.GetString("btnChangeColor.Text");
			this.btnChangeColor.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnChangeColor.TextAlign")));
			this.toolTip1.SetToolTip(this.btnChangeColor, resources.GetString("btnChangeColor.ToolTip"));
			this.btnChangeColor.Visible = ((bool)(resources.GetObject("btnChangeColor.Visible")));
			this.btnChangeColor.Click += new System.EventHandler(this.btnChangeColor_Click);
			// 
			// lblItemStates
			// 
			this.lblItemStates.AccessibleDescription = resources.GetString("lblItemStates.AccessibleDescription");
			this.lblItemStates.AccessibleName = resources.GetString("lblItemStates.AccessibleName");
			this.lblItemStates.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblItemStates.Anchor")));
			this.lblItemStates.AutoSize = ((bool)(resources.GetObject("lblItemStates.AutoSize")));
			this.lblItemStates.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblItemStates.Dock")));
			this.lblItemStates.Enabled = ((bool)(resources.GetObject("lblItemStates.Enabled")));
			this.errorProvider1.SetError(this.lblItemStates, resources.GetString("lblItemStates.Error"));
			this.lblItemStates.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblItemStates.Font = ((System.Drawing.Font)(resources.GetObject("lblItemStates.Font")));
			this.errorProvider1.SetIconAlignment(this.lblItemStates, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lblItemStates.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.lblItemStates, ((int)(resources.GetObject("lblItemStates.IconPadding"))));
			this.lblItemStates.Image = ((System.Drawing.Image)(resources.GetObject("lblItemStates.Image")));
			this.lblItemStates.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblItemStates.ImageAlign")));
			this.lblItemStates.ImageIndex = ((int)(resources.GetObject("lblItemStates.ImageIndex")));
			this.lblItemStates.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblItemStates.ImeMode")));
			this.lblItemStates.Location = ((System.Drawing.Point)(resources.GetObject("lblItemStates.Location")));
			this.lblItemStates.Name = "lblItemStates";
			this.lblItemStates.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblItemStates.RightToLeft")));
			this.lblItemStates.Size = ((System.Drawing.Size)(resources.GetObject("lblItemStates.Size")));
			this.lblItemStates.TabIndex = ((int)(resources.GetObject("lblItemStates.TabIndex")));
			this.lblItemStates.Text = resources.GetString("lblItemStates.Text");
			this.lblItemStates.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblItemStates.TextAlign")));
			this.toolTip1.SetToolTip(this.lblItemStates, resources.GetString("lblItemStates.ToolTip"));
			this.lblItemStates.Visible = ((bool)(resources.GetObject("lblItemStates.Visible")));
			// 
			// lstItemStates
			// 
			this.lstItemStates.AccessibleDescription = resources.GetString("lstItemStates.AccessibleDescription");
			this.lstItemStates.AccessibleName = resources.GetString("lstItemStates.AccessibleName");
			this.lstItemStates.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lstItemStates.Anchor")));
			this.lstItemStates.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lstItemStates.BackgroundImage")));
			this.lstItemStates.ColumnWidth = ((int)(resources.GetObject("lstItemStates.ColumnWidth")));
			this.lstItemStates.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lstItemStates.Dock")));
			this.lstItemStates.Enabled = ((bool)(resources.GetObject("lstItemStates.Enabled")));
			this.errorProvider1.SetError(this.lstItemStates, resources.GetString("lstItemStates.Error"));
			this.lstItemStates.Font = ((System.Drawing.Font)(resources.GetObject("lstItemStates.Font")));
			this.lstItemStates.HorizontalExtent = ((int)(resources.GetObject("lstItemStates.HorizontalExtent")));
			this.lstItemStates.HorizontalScrollbar = ((bool)(resources.GetObject("lstItemStates.HorizontalScrollbar")));
			this.errorProvider1.SetIconAlignment(this.lstItemStates, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lstItemStates.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.lstItemStates, ((int)(resources.GetObject("lstItemStates.IconPadding"))));
			this.lstItemStates.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lstItemStates.ImeMode")));
			this.lstItemStates.IntegralHeight = ((bool)(resources.GetObject("lstItemStates.IntegralHeight")));
			this.lstItemStates.ItemHeight = ((int)(resources.GetObject("lstItemStates.ItemHeight")));
			this.lstItemStates.Items.AddRange(new object[] {
															   resources.GetString("lstItemStates.Items"),
															   resources.GetString("lstItemStates.Items1"),
															   resources.GetString("lstItemStates.Items2"),
															   resources.GetString("lstItemStates.Items3"),
															   resources.GetString("lstItemStates.Items4")});
			this.lstItemStates.Location = ((System.Drawing.Point)(resources.GetObject("lstItemStates.Location")));
			this.lstItemStates.Name = "lstItemStates";
			this.lstItemStates.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lstItemStates.RightToLeft")));
			this.lstItemStates.ScrollAlwaysVisible = ((bool)(resources.GetObject("lstItemStates.ScrollAlwaysVisible")));
			this.lstItemStates.Size = ((System.Drawing.Size)(resources.GetObject("lstItemStates.Size")));
			this.lstItemStates.TabIndex = ((int)(resources.GetObject("lstItemStates.TabIndex")));
			this.toolTip1.SetToolTip(this.lstItemStates, resources.GetString("lstItemStates.ToolTip"));
			this.lstItemStates.Visible = ((bool)(resources.GetObject("lstItemStates.Visible")));
			this.lstItemStates.SelectedIndexChanged += new System.EventHandler(this.OnItemStatesSelectedIndexChanged);
			// 
			// chkFontUnderline
			// 
			this.chkFontUnderline.AccessibleDescription = resources.GetString("chkFontUnderline.AccessibleDescription");
			this.chkFontUnderline.AccessibleName = resources.GetString("chkFontUnderline.AccessibleName");
			this.chkFontUnderline.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkFontUnderline.Anchor")));
			this.chkFontUnderline.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkFontUnderline.Appearance")));
			this.chkFontUnderline.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkFontUnderline.BackgroundImage")));
			this.chkFontUnderline.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontUnderline.CheckAlign")));
			this.chkFontUnderline.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkFontUnderline.Dock")));
			this.chkFontUnderline.Enabled = ((bool)(resources.GetObject("chkFontUnderline.Enabled")));
			this.errorProvider1.SetError(this.chkFontUnderline, resources.GetString("chkFontUnderline.Error"));
			this.chkFontUnderline.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkFontUnderline.FlatStyle")));
			this.chkFontUnderline.Font = ((System.Drawing.Font)(resources.GetObject("chkFontUnderline.Font")));
			this.errorProvider1.SetIconAlignment(this.chkFontUnderline, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkFontUnderline.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.chkFontUnderline, ((int)(resources.GetObject("chkFontUnderline.IconPadding"))));
			this.chkFontUnderline.Image = ((System.Drawing.Image)(resources.GetObject("chkFontUnderline.Image")));
			this.chkFontUnderline.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontUnderline.ImageAlign")));
			this.chkFontUnderline.ImageIndex = ((int)(resources.GetObject("chkFontUnderline.ImageIndex")));
			this.chkFontUnderline.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkFontUnderline.ImeMode")));
			this.chkFontUnderline.Location = ((System.Drawing.Point)(resources.GetObject("chkFontUnderline.Location")));
			this.chkFontUnderline.Name = "chkFontUnderline";
			this.chkFontUnderline.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkFontUnderline.RightToLeft")));
			this.chkFontUnderline.Size = ((System.Drawing.Size)(resources.GetObject("chkFontUnderline.Size")));
			this.chkFontUnderline.TabIndex = ((int)(resources.GetObject("chkFontUnderline.TabIndex")));
			this.chkFontUnderline.Text = resources.GetString("chkFontUnderline.Text");
			this.chkFontUnderline.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontUnderline.TextAlign")));
			this.toolTip1.SetToolTip(this.chkFontUnderline, resources.GetString("chkFontUnderline.ToolTip"));
			this.chkFontUnderline.Visible = ((bool)(resources.GetObject("chkFontUnderline.Visible")));
			this.chkFontUnderline.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
			// 
			// chkFontStrikeout
			// 
			this.chkFontStrikeout.AccessibleDescription = resources.GetString("chkFontStrikeout.AccessibleDescription");
			this.chkFontStrikeout.AccessibleName = resources.GetString("chkFontStrikeout.AccessibleName");
			this.chkFontStrikeout.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkFontStrikeout.Anchor")));
			this.chkFontStrikeout.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkFontStrikeout.Appearance")));
			this.chkFontStrikeout.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkFontStrikeout.BackgroundImage")));
			this.chkFontStrikeout.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontStrikeout.CheckAlign")));
			this.chkFontStrikeout.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkFontStrikeout.Dock")));
			this.chkFontStrikeout.Enabled = ((bool)(resources.GetObject("chkFontStrikeout.Enabled")));
			this.errorProvider1.SetError(this.chkFontStrikeout, resources.GetString("chkFontStrikeout.Error"));
			this.chkFontStrikeout.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkFontStrikeout.FlatStyle")));
			this.chkFontStrikeout.Font = ((System.Drawing.Font)(resources.GetObject("chkFontStrikeout.Font")));
			this.errorProvider1.SetIconAlignment(this.chkFontStrikeout, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkFontStrikeout.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.chkFontStrikeout, ((int)(resources.GetObject("chkFontStrikeout.IconPadding"))));
			this.chkFontStrikeout.Image = ((System.Drawing.Image)(resources.GetObject("chkFontStrikeout.Image")));
			this.chkFontStrikeout.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontStrikeout.ImageAlign")));
			this.chkFontStrikeout.ImageIndex = ((int)(resources.GetObject("chkFontStrikeout.ImageIndex")));
			this.chkFontStrikeout.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkFontStrikeout.ImeMode")));
			this.chkFontStrikeout.Location = ((System.Drawing.Point)(resources.GetObject("chkFontStrikeout.Location")));
			this.chkFontStrikeout.Name = "chkFontStrikeout";
			this.chkFontStrikeout.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkFontStrikeout.RightToLeft")));
			this.chkFontStrikeout.Size = ((System.Drawing.Size)(resources.GetObject("chkFontStrikeout.Size")));
			this.chkFontStrikeout.TabIndex = ((int)(resources.GetObject("chkFontStrikeout.TabIndex")));
			this.chkFontStrikeout.Text = resources.GetString("chkFontStrikeout.Text");
			this.chkFontStrikeout.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkFontStrikeout.TextAlign")));
			this.toolTip1.SetToolTip(this.chkFontStrikeout, resources.GetString("chkFontStrikeout.ToolTip"));
			this.chkFontStrikeout.Visible = ((bool)(resources.GetObject("chkFontStrikeout.Visible")));
			this.chkFontStrikeout.CheckedChanged += new System.EventHandler(this.OnFontStyleChanged);
			// 
			// lblFontSampleCaption
			// 
			this.lblFontSampleCaption.AccessibleDescription = resources.GetString("lblFontSampleCaption.AccessibleDescription");
			this.lblFontSampleCaption.AccessibleName = resources.GetString("lblFontSampleCaption.AccessibleName");
			this.lblFontSampleCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFontSampleCaption.Anchor")));
			this.lblFontSampleCaption.AutoSize = ((bool)(resources.GetObject("lblFontSampleCaption.AutoSize")));
			this.lblFontSampleCaption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFontSampleCaption.Dock")));
			this.lblFontSampleCaption.Enabled = ((bool)(resources.GetObject("lblFontSampleCaption.Enabled")));
			this.errorProvider1.SetError(this.lblFontSampleCaption, resources.GetString("lblFontSampleCaption.Error"));
			this.lblFontSampleCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFontSampleCaption.Font = ((System.Drawing.Font)(resources.GetObject("lblFontSampleCaption.Font")));
			this.errorProvider1.SetIconAlignment(this.lblFontSampleCaption, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lblFontSampleCaption.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.lblFontSampleCaption, ((int)(resources.GetObject("lblFontSampleCaption.IconPadding"))));
			this.lblFontSampleCaption.Image = ((System.Drawing.Image)(resources.GetObject("lblFontSampleCaption.Image")));
			this.lblFontSampleCaption.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFontSampleCaption.ImageAlign")));
			this.lblFontSampleCaption.ImageIndex = ((int)(resources.GetObject("lblFontSampleCaption.ImageIndex")));
			this.lblFontSampleCaption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFontSampleCaption.ImeMode")));
			this.lblFontSampleCaption.Location = ((System.Drawing.Point)(resources.GetObject("lblFontSampleCaption.Location")));
			this.lblFontSampleCaption.Name = "lblFontSampleCaption";
			this.lblFontSampleCaption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFontSampleCaption.RightToLeft")));
			this.lblFontSampleCaption.Size = ((System.Drawing.Size)(resources.GetObject("lblFontSampleCaption.Size")));
			this.lblFontSampleCaption.TabIndex = ((int)(resources.GetObject("lblFontSampleCaption.TabIndex")));
			this.lblFontSampleCaption.Text = resources.GetString("lblFontSampleCaption.Text");
			this.lblFontSampleCaption.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFontSampleCaption.TextAlign")));
			this.toolTip1.SetToolTip(this.lblFontSampleCaption, resources.GetString("lblFontSampleCaption.ToolTip"));
			this.lblFontSampleCaption.Visible = ((bool)(resources.GetObject("lblFontSampleCaption.Visible")));
			// 
			// lblFontSampleABC
			// 
			this.lblFontSampleABC.AccessibleDescription = resources.GetString("lblFontSampleABC.AccessibleDescription");
			this.lblFontSampleABC.AccessibleName = resources.GetString("lblFontSampleABC.AccessibleName");
			this.lblFontSampleABC.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFontSampleABC.Anchor")));
			this.lblFontSampleABC.AutoSize = ((bool)(resources.GetObject("lblFontSampleABC.AutoSize")));
			this.lblFontSampleABC.BackColor = System.Drawing.SystemColors.Window;
			this.lblFontSampleABC.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblFontSampleABC.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFontSampleABC.Dock")));
			this.lblFontSampleABC.Enabled = ((bool)(resources.GetObject("lblFontSampleABC.Enabled")));
			this.errorProvider1.SetError(this.lblFontSampleABC, resources.GetString("lblFontSampleABC.Error"));
			this.lblFontSampleABC.Font = ((System.Drawing.Font)(resources.GetObject("lblFontSampleABC.Font")));
			this.errorProvider1.SetIconAlignment(this.lblFontSampleABC, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lblFontSampleABC.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.lblFontSampleABC, ((int)(resources.GetObject("lblFontSampleABC.IconPadding"))));
			this.lblFontSampleABC.Image = ((System.Drawing.Image)(resources.GetObject("lblFontSampleABC.Image")));
			this.lblFontSampleABC.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFontSampleABC.ImageAlign")));
			this.lblFontSampleABC.ImageIndex = ((int)(resources.GetObject("lblFontSampleABC.ImageIndex")));
			this.lblFontSampleABC.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFontSampleABC.ImeMode")));
			this.lblFontSampleABC.Location = ((System.Drawing.Point)(resources.GetObject("lblFontSampleABC.Location")));
			this.lblFontSampleABC.Name = "lblFontSampleABC";
			this.lblFontSampleABC.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFontSampleABC.RightToLeft")));
			this.lblFontSampleABC.Size = ((System.Drawing.Size)(resources.GetObject("lblFontSampleABC.Size")));
			this.lblFontSampleABC.TabIndex = ((int)(resources.GetObject("lblFontSampleABC.TabIndex")));
			this.lblFontSampleABC.Text = resources.GetString("lblFontSampleABC.Text");
			this.lblFontSampleABC.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFontSampleABC.TextAlign")));
			this.toolTip1.SetToolTip(this.lblFontSampleABC, resources.GetString("lblFontSampleABC.ToolTip"));
			this.lblFontSampleABC.Visible = ((bool)(resources.GetObject("lblFontSampleABC.Visible")));
			// 
			// tabWebBrowser
			// 
			this.tabWebBrowser.AccessibleDescription = resources.GetString("tabWebBrowser.AccessibleDescription");
			this.tabWebBrowser.AccessibleName = resources.GetString("tabWebBrowser.AccessibleName");
			this.tabWebBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabWebBrowser.Anchor")));
			this.tabWebBrowser.AutoScroll = ((bool)(resources.GetObject("tabWebBrowser.AutoScroll")));
			this.tabWebBrowser.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabWebBrowser.AutoScrollMargin")));
			this.tabWebBrowser.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabWebBrowser.AutoScrollMinSize")));
			this.tabWebBrowser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabWebBrowser.BackgroundImage")));
			this.tabWebBrowser.Controls.Add(this.sectionPanelWebBrowserSecurity);
			this.tabWebBrowser.Controls.Add(this.sectionPanelWebBrowserOnNewWindow);
			this.tabWebBrowser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabWebBrowser.Dock")));
			this.tabWebBrowser.Enabled = ((bool)(resources.GetObject("tabWebBrowser.Enabled")));
			this.errorProvider1.SetError(this.tabWebBrowser, resources.GetString("tabWebBrowser.Error"));
			this.tabWebBrowser.Font = ((System.Drawing.Font)(resources.GetObject("tabWebBrowser.Font")));
			this.errorProvider1.SetIconAlignment(this.tabWebBrowser, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabWebBrowser.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabWebBrowser, ((int)(resources.GetObject("tabWebBrowser.IconPadding"))));
			this.tabWebBrowser.ImageIndex = ((int)(resources.GetObject("tabWebBrowser.ImageIndex")));
			this.tabWebBrowser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabWebBrowser.ImeMode")));
			this.tabWebBrowser.Location = ((System.Drawing.Point)(resources.GetObject("tabWebBrowser.Location")));
			this.tabWebBrowser.Name = "tabWebBrowser";
			this.tabWebBrowser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabWebBrowser.RightToLeft")));
			this.tabWebBrowser.Size = ((System.Drawing.Size)(resources.GetObject("tabWebBrowser.Size")));
			this.tabWebBrowser.TabIndex = ((int)(resources.GetObject("tabWebBrowser.TabIndex")));
			this.tabWebBrowser.Text = resources.GetString("tabWebBrowser.Text");
			this.toolTip1.SetToolTip(this.tabWebBrowser, resources.GetString("tabWebBrowser.ToolTip"));
			this.tabWebBrowser.ToolTipText = resources.GetString("tabWebBrowser.ToolTipText");
			this.tabWebBrowser.Visible = ((bool)(resources.GetObject("tabWebBrowser.Visible")));
			// 
			// sectionPanelWebBrowserSecurity
			// 
			this.sectionPanelWebBrowserSecurity.AccessibleDescription = resources.GetString("sectionPanelWebBrowserSecurity.AccessibleDescription");
			this.sectionPanelWebBrowserSecurity.AccessibleName = resources.GetString("sectionPanelWebBrowserSecurity.AccessibleName");
			this.sectionPanelWebBrowserSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelWebBrowserSecurity.Anchor")));
			this.sectionPanelWebBrowserSecurity.AutoScroll = ((bool)(resources.GetObject("sectionPanelWebBrowserSecurity.AutoScroll")));
			this.sectionPanelWebBrowserSecurity.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebBrowserSecurity.AutoScrollMargin")));
			this.sectionPanelWebBrowserSecurity.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebBrowserSecurity.AutoScrollMinSize")));
			this.sectionPanelWebBrowserSecurity.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebBrowserSecurity.BackgroundImage")));
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.labelCheckToAllow);
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserActiveXAllowed);
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserJavascriptAllowed);
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserJavaAllowed);
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserBGSoundAllowed);
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserVdieoAllowed);
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.checkBrowserImagesAllowed);
			this.sectionPanelWebBrowserSecurity.Controls.Add(this.label19);
			this.sectionPanelWebBrowserSecurity.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelWebBrowserSecurity.Dock")));
			this.sectionPanelWebBrowserSecurity.Enabled = ((bool)(resources.GetObject("sectionPanelWebBrowserSecurity.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelWebBrowserSecurity, resources.GetString("sectionPanelWebBrowserSecurity.Error"));
			this.sectionPanelWebBrowserSecurity.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelWebBrowserSecurity.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelWebBrowserSecurity, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelWebBrowserSecurity.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelWebBrowserSecurity, ((int)(resources.GetObject("sectionPanelWebBrowserSecurity.IconPadding"))));
			this.sectionPanelWebBrowserSecurity.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebBrowserSecurity.Image")));
			this.sectionPanelWebBrowserSecurity.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelWebBrowserSecurity.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelWebBrowserSecurity.ImeMode")));
			this.sectionPanelWebBrowserSecurity.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelWebBrowserSecurity.Location")));
			this.sectionPanelWebBrowserSecurity.Name = "sectionPanelWebBrowserSecurity";
			this.sectionPanelWebBrowserSecurity.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelWebBrowserSecurity.RightToLeft")));
			this.sectionPanelWebBrowserSecurity.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebBrowserSecurity.Size")));
			this.sectionPanelWebBrowserSecurity.TabIndex = ((int)(resources.GetObject("sectionPanelWebBrowserSecurity.TabIndex")));
			this.sectionPanelWebBrowserSecurity.Text = resources.GetString("sectionPanelWebBrowserSecurity.Text");
			this.toolTip1.SetToolTip(this.sectionPanelWebBrowserSecurity, resources.GetString("sectionPanelWebBrowserSecurity.ToolTip"));
			this.sectionPanelWebBrowserSecurity.Visible = ((bool)(resources.GetObject("sectionPanelWebBrowserSecurity.Visible")));
			// 
			// labelCheckToAllow
			// 
			this.labelCheckToAllow.AccessibleDescription = resources.GetString("labelCheckToAllow.AccessibleDescription");
			this.labelCheckToAllow.AccessibleName = resources.GetString("labelCheckToAllow.AccessibleName");
			this.labelCheckToAllow.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelCheckToAllow.Anchor")));
			this.labelCheckToAllow.AutoSize = ((bool)(resources.GetObject("labelCheckToAllow.AutoSize")));
			this.labelCheckToAllow.BackColor = System.Drawing.Color.Transparent;
			this.labelCheckToAllow.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelCheckToAllow.Dock")));
			this.labelCheckToAllow.Enabled = ((bool)(resources.GetObject("labelCheckToAllow.Enabled")));
			this.errorProvider1.SetError(this.labelCheckToAllow, resources.GetString("labelCheckToAllow.Error"));
			this.labelCheckToAllow.Font = ((System.Drawing.Font)(resources.GetObject("labelCheckToAllow.Font")));
			this.errorProvider1.SetIconAlignment(this.labelCheckToAllow, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelCheckToAllow.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelCheckToAllow, ((int)(resources.GetObject("labelCheckToAllow.IconPadding"))));
			this.labelCheckToAllow.Image = ((System.Drawing.Image)(resources.GetObject("labelCheckToAllow.Image")));
			this.labelCheckToAllow.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCheckToAllow.ImageAlign")));
			this.labelCheckToAllow.ImageIndex = ((int)(resources.GetObject("labelCheckToAllow.ImageIndex")));
			this.labelCheckToAllow.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelCheckToAllow.ImeMode")));
			this.labelCheckToAllow.Location = ((System.Drawing.Point)(resources.GetObject("labelCheckToAllow.Location")));
			this.labelCheckToAllow.Name = "labelCheckToAllow";
			this.labelCheckToAllow.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelCheckToAllow.RightToLeft")));
			this.labelCheckToAllow.Size = ((System.Drawing.Size)(resources.GetObject("labelCheckToAllow.Size")));
			this.labelCheckToAllow.TabIndex = ((int)(resources.GetObject("labelCheckToAllow.TabIndex")));
			this.labelCheckToAllow.Text = resources.GetString("labelCheckToAllow.Text");
			this.labelCheckToAllow.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCheckToAllow.TextAlign")));
			this.toolTip1.SetToolTip(this.labelCheckToAllow, resources.GetString("labelCheckToAllow.ToolTip"));
			this.labelCheckToAllow.Visible = ((bool)(resources.GetObject("labelCheckToAllow.Visible")));
			// 
			// checkBrowserActiveXAllowed
			// 
			this.checkBrowserActiveXAllowed.AccessibleDescription = resources.GetString("checkBrowserActiveXAllowed.AccessibleDescription");
			this.checkBrowserActiveXAllowed.AccessibleName = resources.GetString("checkBrowserActiveXAllowed.AccessibleName");
			this.checkBrowserActiveXAllowed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBrowserActiveXAllowed.Anchor")));
			this.checkBrowserActiveXAllowed.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBrowserActiveXAllowed.Appearance")));
			this.checkBrowserActiveXAllowed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBrowserActiveXAllowed.BackgroundImage")));
			this.checkBrowserActiveXAllowed.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserActiveXAllowed.CheckAlign")));
			this.checkBrowserActiveXAllowed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBrowserActiveXAllowed.Dock")));
			this.checkBrowserActiveXAllowed.Enabled = ((bool)(resources.GetObject("checkBrowserActiveXAllowed.Enabled")));
			this.errorProvider1.SetError(this.checkBrowserActiveXAllowed, resources.GetString("checkBrowserActiveXAllowed.Error"));
			this.checkBrowserActiveXAllowed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBrowserActiveXAllowed.FlatStyle")));
			this.checkBrowserActiveXAllowed.Font = ((System.Drawing.Font)(resources.GetObject("checkBrowserActiveXAllowed.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBrowserActiveXAllowed, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBrowserActiveXAllowed.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBrowserActiveXAllowed, ((int)(resources.GetObject("checkBrowserActiveXAllowed.IconPadding"))));
			this.checkBrowserActiveXAllowed.Image = ((System.Drawing.Image)(resources.GetObject("checkBrowserActiveXAllowed.Image")));
			this.checkBrowserActiveXAllowed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserActiveXAllowed.ImageAlign")));
			this.checkBrowserActiveXAllowed.ImageIndex = ((int)(resources.GetObject("checkBrowserActiveXAllowed.ImageIndex")));
			this.checkBrowserActiveXAllowed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBrowserActiveXAllowed.ImeMode")));
			this.checkBrowserActiveXAllowed.Location = ((System.Drawing.Point)(resources.GetObject("checkBrowserActiveXAllowed.Location")));
			this.checkBrowserActiveXAllowed.Name = "checkBrowserActiveXAllowed";
			this.checkBrowserActiveXAllowed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBrowserActiveXAllowed.RightToLeft")));
			this.checkBrowserActiveXAllowed.Size = ((System.Drawing.Size)(resources.GetObject("checkBrowserActiveXAllowed.Size")));
			this.checkBrowserActiveXAllowed.TabIndex = ((int)(resources.GetObject("checkBrowserActiveXAllowed.TabIndex")));
			this.checkBrowserActiveXAllowed.Text = resources.GetString("checkBrowserActiveXAllowed.Text");
			this.checkBrowserActiveXAllowed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserActiveXAllowed.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBrowserActiveXAllowed, resources.GetString("checkBrowserActiveXAllowed.ToolTip"));
			this.checkBrowserActiveXAllowed.Visible = ((bool)(resources.GetObject("checkBrowserActiveXAllowed.Visible")));
			this.checkBrowserActiveXAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// checkBrowserJavascriptAllowed
			// 
			this.checkBrowserJavascriptAllowed.AccessibleDescription = resources.GetString("checkBrowserJavascriptAllowed.AccessibleDescription");
			this.checkBrowserJavascriptAllowed.AccessibleName = resources.GetString("checkBrowserJavascriptAllowed.AccessibleName");
			this.checkBrowserJavascriptAllowed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBrowserJavascriptAllowed.Anchor")));
			this.checkBrowserJavascriptAllowed.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBrowserJavascriptAllowed.Appearance")));
			this.checkBrowserJavascriptAllowed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBrowserJavascriptAllowed.BackgroundImage")));
			this.checkBrowserJavascriptAllowed.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserJavascriptAllowed.CheckAlign")));
			this.checkBrowserJavascriptAllowed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBrowserJavascriptAllowed.Dock")));
			this.checkBrowserJavascriptAllowed.Enabled = ((bool)(resources.GetObject("checkBrowserJavascriptAllowed.Enabled")));
			this.errorProvider1.SetError(this.checkBrowserJavascriptAllowed, resources.GetString("checkBrowserJavascriptAllowed.Error"));
			this.checkBrowserJavascriptAllowed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBrowserJavascriptAllowed.FlatStyle")));
			this.checkBrowserJavascriptAllowed.Font = ((System.Drawing.Font)(resources.GetObject("checkBrowserJavascriptAllowed.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBrowserJavascriptAllowed, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBrowserJavascriptAllowed.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBrowserJavascriptAllowed, ((int)(resources.GetObject("checkBrowserJavascriptAllowed.IconPadding"))));
			this.checkBrowserJavascriptAllowed.Image = ((System.Drawing.Image)(resources.GetObject("checkBrowserJavascriptAllowed.Image")));
			this.checkBrowserJavascriptAllowed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserJavascriptAllowed.ImageAlign")));
			this.checkBrowserJavascriptAllowed.ImageIndex = ((int)(resources.GetObject("checkBrowserJavascriptAllowed.ImageIndex")));
			this.checkBrowserJavascriptAllowed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBrowserJavascriptAllowed.ImeMode")));
			this.checkBrowserJavascriptAllowed.Location = ((System.Drawing.Point)(resources.GetObject("checkBrowserJavascriptAllowed.Location")));
			this.checkBrowserJavascriptAllowed.Name = "checkBrowserJavascriptAllowed";
			this.checkBrowserJavascriptAllowed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBrowserJavascriptAllowed.RightToLeft")));
			this.checkBrowserJavascriptAllowed.Size = ((System.Drawing.Size)(resources.GetObject("checkBrowserJavascriptAllowed.Size")));
			this.checkBrowserJavascriptAllowed.TabIndex = ((int)(resources.GetObject("checkBrowserJavascriptAllowed.TabIndex")));
			this.checkBrowserJavascriptAllowed.Text = resources.GetString("checkBrowserJavascriptAllowed.Text");
			this.checkBrowserJavascriptAllowed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserJavascriptAllowed.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBrowserJavascriptAllowed, resources.GetString("checkBrowserJavascriptAllowed.ToolTip"));
			this.checkBrowserJavascriptAllowed.Visible = ((bool)(resources.GetObject("checkBrowserJavascriptAllowed.Visible")));
			this.checkBrowserJavascriptAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// checkBrowserJavaAllowed
			// 
			this.checkBrowserJavaAllowed.AccessibleDescription = resources.GetString("checkBrowserJavaAllowed.AccessibleDescription");
			this.checkBrowserJavaAllowed.AccessibleName = resources.GetString("checkBrowserJavaAllowed.AccessibleName");
			this.checkBrowserJavaAllowed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBrowserJavaAllowed.Anchor")));
			this.checkBrowserJavaAllowed.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBrowserJavaAllowed.Appearance")));
			this.checkBrowserJavaAllowed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBrowserJavaAllowed.BackgroundImage")));
			this.checkBrowserJavaAllowed.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserJavaAllowed.CheckAlign")));
			this.checkBrowserJavaAllowed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBrowserJavaAllowed.Dock")));
			this.checkBrowserJavaAllowed.Enabled = ((bool)(resources.GetObject("checkBrowserJavaAllowed.Enabled")));
			this.errorProvider1.SetError(this.checkBrowserJavaAllowed, resources.GetString("checkBrowserJavaAllowed.Error"));
			this.checkBrowserJavaAllowed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBrowserJavaAllowed.FlatStyle")));
			this.checkBrowserJavaAllowed.Font = ((System.Drawing.Font)(resources.GetObject("checkBrowserJavaAllowed.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBrowserJavaAllowed, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBrowserJavaAllowed.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBrowserJavaAllowed, ((int)(resources.GetObject("checkBrowserJavaAllowed.IconPadding"))));
			this.checkBrowserJavaAllowed.Image = ((System.Drawing.Image)(resources.GetObject("checkBrowserJavaAllowed.Image")));
			this.checkBrowserJavaAllowed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserJavaAllowed.ImageAlign")));
			this.checkBrowserJavaAllowed.ImageIndex = ((int)(resources.GetObject("checkBrowserJavaAllowed.ImageIndex")));
			this.checkBrowserJavaAllowed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBrowserJavaAllowed.ImeMode")));
			this.checkBrowserJavaAllowed.Location = ((System.Drawing.Point)(resources.GetObject("checkBrowserJavaAllowed.Location")));
			this.checkBrowserJavaAllowed.Name = "checkBrowserJavaAllowed";
			this.checkBrowserJavaAllowed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBrowserJavaAllowed.RightToLeft")));
			this.checkBrowserJavaAllowed.Size = ((System.Drawing.Size)(resources.GetObject("checkBrowserJavaAllowed.Size")));
			this.checkBrowserJavaAllowed.TabIndex = ((int)(resources.GetObject("checkBrowserJavaAllowed.TabIndex")));
			this.checkBrowserJavaAllowed.Text = resources.GetString("checkBrowserJavaAllowed.Text");
			this.checkBrowserJavaAllowed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserJavaAllowed.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBrowserJavaAllowed, resources.GetString("checkBrowserJavaAllowed.ToolTip"));
			this.checkBrowserJavaAllowed.Visible = ((bool)(resources.GetObject("checkBrowserJavaAllowed.Visible")));
			this.checkBrowserJavaAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// checkBrowserBGSoundAllowed
			// 
			this.checkBrowserBGSoundAllowed.AccessibleDescription = resources.GetString("checkBrowserBGSoundAllowed.AccessibleDescription");
			this.checkBrowserBGSoundAllowed.AccessibleName = resources.GetString("checkBrowserBGSoundAllowed.AccessibleName");
			this.checkBrowserBGSoundAllowed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBrowserBGSoundAllowed.Anchor")));
			this.checkBrowserBGSoundAllowed.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBrowserBGSoundAllowed.Appearance")));
			this.checkBrowserBGSoundAllowed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBrowserBGSoundAllowed.BackgroundImage")));
			this.checkBrowserBGSoundAllowed.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserBGSoundAllowed.CheckAlign")));
			this.checkBrowserBGSoundAllowed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBrowserBGSoundAllowed.Dock")));
			this.checkBrowserBGSoundAllowed.Enabled = ((bool)(resources.GetObject("checkBrowserBGSoundAllowed.Enabled")));
			this.errorProvider1.SetError(this.checkBrowserBGSoundAllowed, resources.GetString("checkBrowserBGSoundAllowed.Error"));
			this.checkBrowserBGSoundAllowed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBrowserBGSoundAllowed.FlatStyle")));
			this.checkBrowserBGSoundAllowed.Font = ((System.Drawing.Font)(resources.GetObject("checkBrowserBGSoundAllowed.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBrowserBGSoundAllowed, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBrowserBGSoundAllowed.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBrowserBGSoundAllowed, ((int)(resources.GetObject("checkBrowserBGSoundAllowed.IconPadding"))));
			this.checkBrowserBGSoundAllowed.Image = ((System.Drawing.Image)(resources.GetObject("checkBrowserBGSoundAllowed.Image")));
			this.checkBrowserBGSoundAllowed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserBGSoundAllowed.ImageAlign")));
			this.checkBrowserBGSoundAllowed.ImageIndex = ((int)(resources.GetObject("checkBrowserBGSoundAllowed.ImageIndex")));
			this.checkBrowserBGSoundAllowed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBrowserBGSoundAllowed.ImeMode")));
			this.checkBrowserBGSoundAllowed.Location = ((System.Drawing.Point)(resources.GetObject("checkBrowserBGSoundAllowed.Location")));
			this.checkBrowserBGSoundAllowed.Name = "checkBrowserBGSoundAllowed";
			this.checkBrowserBGSoundAllowed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBrowserBGSoundAllowed.RightToLeft")));
			this.checkBrowserBGSoundAllowed.Size = ((System.Drawing.Size)(resources.GetObject("checkBrowserBGSoundAllowed.Size")));
			this.checkBrowserBGSoundAllowed.TabIndex = ((int)(resources.GetObject("checkBrowserBGSoundAllowed.TabIndex")));
			this.checkBrowserBGSoundAllowed.Text = resources.GetString("checkBrowserBGSoundAllowed.Text");
			this.checkBrowserBGSoundAllowed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserBGSoundAllowed.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBrowserBGSoundAllowed, resources.GetString("checkBrowserBGSoundAllowed.ToolTip"));
			this.checkBrowserBGSoundAllowed.Visible = ((bool)(resources.GetObject("checkBrowserBGSoundAllowed.Visible")));
			this.checkBrowserBGSoundAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// checkBrowserVdieoAllowed
			// 
			this.checkBrowserVdieoAllowed.AccessibleDescription = resources.GetString("checkBrowserVdieoAllowed.AccessibleDescription");
			this.checkBrowserVdieoAllowed.AccessibleName = resources.GetString("checkBrowserVdieoAllowed.AccessibleName");
			this.checkBrowserVdieoAllowed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBrowserVdieoAllowed.Anchor")));
			this.checkBrowserVdieoAllowed.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBrowserVdieoAllowed.Appearance")));
			this.checkBrowserVdieoAllowed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBrowserVdieoAllowed.BackgroundImage")));
			this.checkBrowserVdieoAllowed.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserVdieoAllowed.CheckAlign")));
			this.checkBrowserVdieoAllowed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBrowserVdieoAllowed.Dock")));
			this.checkBrowserVdieoAllowed.Enabled = ((bool)(resources.GetObject("checkBrowserVdieoAllowed.Enabled")));
			this.errorProvider1.SetError(this.checkBrowserVdieoAllowed, resources.GetString("checkBrowserVdieoAllowed.Error"));
			this.checkBrowserVdieoAllowed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBrowserVdieoAllowed.FlatStyle")));
			this.checkBrowserVdieoAllowed.Font = ((System.Drawing.Font)(resources.GetObject("checkBrowserVdieoAllowed.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBrowserVdieoAllowed, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBrowserVdieoAllowed.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBrowserVdieoAllowed, ((int)(resources.GetObject("checkBrowserVdieoAllowed.IconPadding"))));
			this.checkBrowserVdieoAllowed.Image = ((System.Drawing.Image)(resources.GetObject("checkBrowserVdieoAllowed.Image")));
			this.checkBrowserVdieoAllowed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserVdieoAllowed.ImageAlign")));
			this.checkBrowserVdieoAllowed.ImageIndex = ((int)(resources.GetObject("checkBrowserVdieoAllowed.ImageIndex")));
			this.checkBrowserVdieoAllowed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBrowserVdieoAllowed.ImeMode")));
			this.checkBrowserVdieoAllowed.Location = ((System.Drawing.Point)(resources.GetObject("checkBrowserVdieoAllowed.Location")));
			this.checkBrowserVdieoAllowed.Name = "checkBrowserVdieoAllowed";
			this.checkBrowserVdieoAllowed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBrowserVdieoAllowed.RightToLeft")));
			this.checkBrowserVdieoAllowed.Size = ((System.Drawing.Size)(resources.GetObject("checkBrowserVdieoAllowed.Size")));
			this.checkBrowserVdieoAllowed.TabIndex = ((int)(resources.GetObject("checkBrowserVdieoAllowed.TabIndex")));
			this.checkBrowserVdieoAllowed.Text = resources.GetString("checkBrowserVdieoAllowed.Text");
			this.checkBrowserVdieoAllowed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserVdieoAllowed.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBrowserVdieoAllowed, resources.GetString("checkBrowserVdieoAllowed.ToolTip"));
			this.checkBrowserVdieoAllowed.Visible = ((bool)(resources.GetObject("checkBrowserVdieoAllowed.Visible")));
			this.checkBrowserVdieoAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// checkBrowserImagesAllowed
			// 
			this.checkBrowserImagesAllowed.AccessibleDescription = resources.GetString("checkBrowserImagesAllowed.AccessibleDescription");
			this.checkBrowserImagesAllowed.AccessibleName = resources.GetString("checkBrowserImagesAllowed.AccessibleName");
			this.checkBrowserImagesAllowed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBrowserImagesAllowed.Anchor")));
			this.checkBrowserImagesAllowed.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBrowserImagesAllowed.Appearance")));
			this.checkBrowserImagesAllowed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBrowserImagesAllowed.BackgroundImage")));
			this.checkBrowserImagesAllowed.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserImagesAllowed.CheckAlign")));
			this.checkBrowserImagesAllowed.Checked = true;
			this.checkBrowserImagesAllowed.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBrowserImagesAllowed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBrowserImagesAllowed.Dock")));
			this.checkBrowserImagesAllowed.Enabled = ((bool)(resources.GetObject("checkBrowserImagesAllowed.Enabled")));
			this.errorProvider1.SetError(this.checkBrowserImagesAllowed, resources.GetString("checkBrowserImagesAllowed.Error"));
			this.checkBrowserImagesAllowed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBrowserImagesAllowed.FlatStyle")));
			this.checkBrowserImagesAllowed.Font = ((System.Drawing.Font)(resources.GetObject("checkBrowserImagesAllowed.Font")));
			this.errorProvider1.SetIconAlignment(this.checkBrowserImagesAllowed, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkBrowserImagesAllowed.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkBrowserImagesAllowed, ((int)(resources.GetObject("checkBrowserImagesAllowed.IconPadding"))));
			this.checkBrowserImagesAllowed.Image = ((System.Drawing.Image)(resources.GetObject("checkBrowserImagesAllowed.Image")));
			this.checkBrowserImagesAllowed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserImagesAllowed.ImageAlign")));
			this.checkBrowserImagesAllowed.ImageIndex = ((int)(resources.GetObject("checkBrowserImagesAllowed.ImageIndex")));
			this.checkBrowserImagesAllowed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBrowserImagesAllowed.ImeMode")));
			this.checkBrowserImagesAllowed.Location = ((System.Drawing.Point)(resources.GetObject("checkBrowserImagesAllowed.Location")));
			this.checkBrowserImagesAllowed.Name = "checkBrowserImagesAllowed";
			this.checkBrowserImagesAllowed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBrowserImagesAllowed.RightToLeft")));
			this.checkBrowserImagesAllowed.Size = ((System.Drawing.Size)(resources.GetObject("checkBrowserImagesAllowed.Size")));
			this.checkBrowserImagesAllowed.TabIndex = ((int)(resources.GetObject("checkBrowserImagesAllowed.TabIndex")));
			this.checkBrowserImagesAllowed.Text = resources.GetString("checkBrowserImagesAllowed.Text");
			this.checkBrowserImagesAllowed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBrowserImagesAllowed.TextAlign")));
			this.toolTip1.SetToolTip(this.checkBrowserImagesAllowed, resources.GetString("checkBrowserImagesAllowed.ToolTip"));
			this.checkBrowserImagesAllowed.Visible = ((bool)(resources.GetObject("checkBrowserImagesAllowed.Visible")));
			this.checkBrowserImagesAllowed.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// label19
			// 
			this.label19.AccessibleDescription = resources.GetString("label19.AccessibleDescription");
			this.label19.AccessibleName = resources.GetString("label19.AccessibleName");
			this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label19.Anchor")));
			this.label19.AutoSize = ((bool)(resources.GetObject("label19.AutoSize")));
			this.label19.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label19.Dock")));
			this.label19.Enabled = ((bool)(resources.GetObject("label19.Enabled")));
			this.errorProvider1.SetError(this.label19, resources.GetString("label19.Error"));
			this.label19.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label19.Font = ((System.Drawing.Font)(resources.GetObject("label19.Font")));
			this.errorProvider1.SetIconAlignment(this.label19, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label19.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label19, ((int)(resources.GetObject("label19.IconPadding"))));
			this.label19.Image = ((System.Drawing.Image)(resources.GetObject("label19.Image")));
			this.label19.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label19.ImageAlign")));
			this.label19.ImageIndex = ((int)(resources.GetObject("label19.ImageIndex")));
			this.label19.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label19.ImeMode")));
			this.label19.Location = ((System.Drawing.Point)(resources.GetObject("label19.Location")));
			this.label19.Name = "label19";
			this.label19.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label19.RightToLeft")));
			this.label19.Size = ((System.Drawing.Size)(resources.GetObject("label19.Size")));
			this.label19.TabIndex = ((int)(resources.GetObject("label19.TabIndex")));
			this.label19.Text = resources.GetString("label19.Text");
			this.label19.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label19.TextAlign")));
			this.toolTip1.SetToolTip(this.label19, resources.GetString("label19.ToolTip"));
			this.label19.Visible = ((bool)(resources.GetObject("label19.Visible")));
			// 
			// sectionPanelWebBrowserOnNewWindow
			// 
			this.sectionPanelWebBrowserOnNewWindow.AccessibleDescription = resources.GetString("sectionPanelWebBrowserOnNewWindow.AccessibleDescription");
			this.sectionPanelWebBrowserOnNewWindow.AccessibleName = resources.GetString("sectionPanelWebBrowserOnNewWindow.AccessibleName");
			this.sectionPanelWebBrowserOnNewWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Anchor")));
			this.sectionPanelWebBrowserOnNewWindow.AutoScroll = ((bool)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.AutoScroll")));
			this.sectionPanelWebBrowserOnNewWindow.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.AutoScrollMargin")));
			this.sectionPanelWebBrowserOnNewWindow.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.AutoScrollMinSize")));
			this.sectionPanelWebBrowserOnNewWindow.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.BackgroundImage")));
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.optNewWindowCustomExec);
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.optNewWindowDefaultWebBrowser);
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.optNewWindowOnTab);
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.checkReuseFirstBrowserTab);
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.btnSelectExecutable);
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.txtBrowserStartExecutable);
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.labelBrowserStartExecutable);
			this.sectionPanelWebBrowserOnNewWindow.Controls.Add(this.label16);
			this.sectionPanelWebBrowserOnNewWindow.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Dock")));
			this.sectionPanelWebBrowserOnNewWindow.Enabled = ((bool)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelWebBrowserOnNewWindow, resources.GetString("sectionPanelWebBrowserOnNewWindow.Error"));
			this.sectionPanelWebBrowserOnNewWindow.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelWebBrowserOnNewWindow, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelWebBrowserOnNewWindow, ((int)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.IconPadding"))));
			this.sectionPanelWebBrowserOnNewWindow.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Image")));
			this.sectionPanelWebBrowserOnNewWindow.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelWebBrowserOnNewWindow.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.ImeMode")));
			this.sectionPanelWebBrowserOnNewWindow.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Location")));
			this.sectionPanelWebBrowserOnNewWindow.Name = "sectionPanelWebBrowserOnNewWindow";
			this.sectionPanelWebBrowserOnNewWindow.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.RightToLeft")));
			this.sectionPanelWebBrowserOnNewWindow.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Size")));
			this.sectionPanelWebBrowserOnNewWindow.TabIndex = ((int)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.TabIndex")));
			this.sectionPanelWebBrowserOnNewWindow.Text = resources.GetString("sectionPanelWebBrowserOnNewWindow.Text");
			this.toolTip1.SetToolTip(this.sectionPanelWebBrowserOnNewWindow, resources.GetString("sectionPanelWebBrowserOnNewWindow.ToolTip"));
			this.sectionPanelWebBrowserOnNewWindow.Visible = ((bool)(resources.GetObject("sectionPanelWebBrowserOnNewWindow.Visible")));
			// 
			// optNewWindowCustomExec
			// 
			this.optNewWindowCustomExec.AccessibleDescription = resources.GetString("optNewWindowCustomExec.AccessibleDescription");
			this.optNewWindowCustomExec.AccessibleName = resources.GetString("optNewWindowCustomExec.AccessibleName");
			this.optNewWindowCustomExec.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("optNewWindowCustomExec.Anchor")));
			this.optNewWindowCustomExec.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("optNewWindowCustomExec.Appearance")));
			this.optNewWindowCustomExec.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("optNewWindowCustomExec.BackgroundImage")));
			this.optNewWindowCustomExec.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowCustomExec.CheckAlign")));
			this.optNewWindowCustomExec.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("optNewWindowCustomExec.Dock")));
			this.optNewWindowCustomExec.Enabled = ((bool)(resources.GetObject("optNewWindowCustomExec.Enabled")));
			this.errorProvider1.SetError(this.optNewWindowCustomExec, resources.GetString("optNewWindowCustomExec.Error"));
			this.optNewWindowCustomExec.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("optNewWindowCustomExec.FlatStyle")));
			this.optNewWindowCustomExec.Font = ((System.Drawing.Font)(resources.GetObject("optNewWindowCustomExec.Font")));
			this.errorProvider1.SetIconAlignment(this.optNewWindowCustomExec, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("optNewWindowCustomExec.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.optNewWindowCustomExec, ((int)(resources.GetObject("optNewWindowCustomExec.IconPadding"))));
			this.optNewWindowCustomExec.Image = ((System.Drawing.Image)(resources.GetObject("optNewWindowCustomExec.Image")));
			this.optNewWindowCustomExec.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowCustomExec.ImageAlign")));
			this.optNewWindowCustomExec.ImageIndex = ((int)(resources.GetObject("optNewWindowCustomExec.ImageIndex")));
			this.optNewWindowCustomExec.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("optNewWindowCustomExec.ImeMode")));
			this.optNewWindowCustomExec.Location = ((System.Drawing.Point)(resources.GetObject("optNewWindowCustomExec.Location")));
			this.optNewWindowCustomExec.Name = "optNewWindowCustomExec";
			this.optNewWindowCustomExec.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("optNewWindowCustomExec.RightToLeft")));
			this.optNewWindowCustomExec.Size = ((System.Drawing.Size)(resources.GetObject("optNewWindowCustomExec.Size")));
			this.optNewWindowCustomExec.TabIndex = ((int)(resources.GetObject("optNewWindowCustomExec.TabIndex")));
			this.optNewWindowCustomExec.Text = resources.GetString("optNewWindowCustomExec.Text");
			this.optNewWindowCustomExec.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowCustomExec.TextAlign")));
			this.toolTip1.SetToolTip(this.optNewWindowCustomExec, resources.GetString("optNewWindowCustomExec.ToolTip"));
			this.optNewWindowCustomExec.Visible = ((bool)(resources.GetObject("optNewWindowCustomExec.Visible")));
			this.optNewWindowCustomExec.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.optNewWindowCustomExec.Validated += new System.EventHandler(this.OnControlValidated);
			this.optNewWindowCustomExec.CheckedChanged += new System.EventHandler(this.optNewWindowCustomExec_CheckedChanged);
			// 
			// optNewWindowDefaultWebBrowser
			// 
			this.optNewWindowDefaultWebBrowser.AccessibleDescription = resources.GetString("optNewWindowDefaultWebBrowser.AccessibleDescription");
			this.optNewWindowDefaultWebBrowser.AccessibleName = resources.GetString("optNewWindowDefaultWebBrowser.AccessibleName");
			this.optNewWindowDefaultWebBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("optNewWindowDefaultWebBrowser.Anchor")));
			this.optNewWindowDefaultWebBrowser.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("optNewWindowDefaultWebBrowser.Appearance")));
			this.optNewWindowDefaultWebBrowser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("optNewWindowDefaultWebBrowser.BackgroundImage")));
			this.optNewWindowDefaultWebBrowser.CausesValidation = false;
			this.optNewWindowDefaultWebBrowser.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowDefaultWebBrowser.CheckAlign")));
			this.optNewWindowDefaultWebBrowser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("optNewWindowDefaultWebBrowser.Dock")));
			this.optNewWindowDefaultWebBrowser.Enabled = ((bool)(resources.GetObject("optNewWindowDefaultWebBrowser.Enabled")));
			this.errorProvider1.SetError(this.optNewWindowDefaultWebBrowser, resources.GetString("optNewWindowDefaultWebBrowser.Error"));
			this.optNewWindowDefaultWebBrowser.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("optNewWindowDefaultWebBrowser.FlatStyle")));
			this.optNewWindowDefaultWebBrowser.Font = ((System.Drawing.Font)(resources.GetObject("optNewWindowDefaultWebBrowser.Font")));
			this.errorProvider1.SetIconAlignment(this.optNewWindowDefaultWebBrowser, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("optNewWindowDefaultWebBrowser.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.optNewWindowDefaultWebBrowser, ((int)(resources.GetObject("optNewWindowDefaultWebBrowser.IconPadding"))));
			this.optNewWindowDefaultWebBrowser.Image = ((System.Drawing.Image)(resources.GetObject("optNewWindowDefaultWebBrowser.Image")));
			this.optNewWindowDefaultWebBrowser.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowDefaultWebBrowser.ImageAlign")));
			this.optNewWindowDefaultWebBrowser.ImageIndex = ((int)(resources.GetObject("optNewWindowDefaultWebBrowser.ImageIndex")));
			this.optNewWindowDefaultWebBrowser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("optNewWindowDefaultWebBrowser.ImeMode")));
			this.optNewWindowDefaultWebBrowser.Location = ((System.Drawing.Point)(resources.GetObject("optNewWindowDefaultWebBrowser.Location")));
			this.optNewWindowDefaultWebBrowser.Name = "optNewWindowDefaultWebBrowser";
			this.optNewWindowDefaultWebBrowser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("optNewWindowDefaultWebBrowser.RightToLeft")));
			this.optNewWindowDefaultWebBrowser.Size = ((System.Drawing.Size)(resources.GetObject("optNewWindowDefaultWebBrowser.Size")));
			this.optNewWindowDefaultWebBrowser.TabIndex = ((int)(resources.GetObject("optNewWindowDefaultWebBrowser.TabIndex")));
			this.optNewWindowDefaultWebBrowser.Text = resources.GetString("optNewWindowDefaultWebBrowser.Text");
			this.optNewWindowDefaultWebBrowser.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowDefaultWebBrowser.TextAlign")));
			this.toolTip1.SetToolTip(this.optNewWindowDefaultWebBrowser, resources.GetString("optNewWindowDefaultWebBrowser.ToolTip"));
			this.optNewWindowDefaultWebBrowser.Visible = ((bool)(resources.GetObject("optNewWindowDefaultWebBrowser.Visible")));
			this.optNewWindowDefaultWebBrowser.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.optNewWindowDefaultWebBrowser.Validated += new System.EventHandler(this.OnControlValidated);
			this.optNewWindowDefaultWebBrowser.CheckedChanged += new System.EventHandler(this.optOnOpenNewWindowChecked);
			// 
			// optNewWindowOnTab
			// 
			this.optNewWindowOnTab.AccessibleDescription = resources.GetString("optNewWindowOnTab.AccessibleDescription");
			this.optNewWindowOnTab.AccessibleName = resources.GetString("optNewWindowOnTab.AccessibleName");
			this.optNewWindowOnTab.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("optNewWindowOnTab.Anchor")));
			this.optNewWindowOnTab.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("optNewWindowOnTab.Appearance")));
			this.optNewWindowOnTab.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("optNewWindowOnTab.BackgroundImage")));
			this.optNewWindowOnTab.CausesValidation = false;
			this.optNewWindowOnTab.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowOnTab.CheckAlign")));
			this.optNewWindowOnTab.Checked = true;
			this.optNewWindowOnTab.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("optNewWindowOnTab.Dock")));
			this.optNewWindowOnTab.Enabled = ((bool)(resources.GetObject("optNewWindowOnTab.Enabled")));
			this.errorProvider1.SetError(this.optNewWindowOnTab, resources.GetString("optNewWindowOnTab.Error"));
			this.optNewWindowOnTab.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("optNewWindowOnTab.FlatStyle")));
			this.optNewWindowOnTab.Font = ((System.Drawing.Font)(resources.GetObject("optNewWindowOnTab.Font")));
			this.errorProvider1.SetIconAlignment(this.optNewWindowOnTab, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("optNewWindowOnTab.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.optNewWindowOnTab, ((int)(resources.GetObject("optNewWindowOnTab.IconPadding"))));
			this.optNewWindowOnTab.Image = ((System.Drawing.Image)(resources.GetObject("optNewWindowOnTab.Image")));
			this.optNewWindowOnTab.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowOnTab.ImageAlign")));
			this.optNewWindowOnTab.ImageIndex = ((int)(resources.GetObject("optNewWindowOnTab.ImageIndex")));
			this.optNewWindowOnTab.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("optNewWindowOnTab.ImeMode")));
			this.optNewWindowOnTab.Location = ((System.Drawing.Point)(resources.GetObject("optNewWindowOnTab.Location")));
			this.optNewWindowOnTab.Name = "optNewWindowOnTab";
			this.optNewWindowOnTab.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("optNewWindowOnTab.RightToLeft")));
			this.optNewWindowOnTab.Size = ((System.Drawing.Size)(resources.GetObject("optNewWindowOnTab.Size")));
			this.optNewWindowOnTab.TabIndex = ((int)(resources.GetObject("optNewWindowOnTab.TabIndex")));
			this.optNewWindowOnTab.TabStop = true;
			this.optNewWindowOnTab.Text = resources.GetString("optNewWindowOnTab.Text");
			this.optNewWindowOnTab.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("optNewWindowOnTab.TextAlign")));
			this.toolTip1.SetToolTip(this.optNewWindowOnTab, resources.GetString("optNewWindowOnTab.ToolTip"));
			this.optNewWindowOnTab.Visible = ((bool)(resources.GetObject("optNewWindowOnTab.Visible")));
			this.optNewWindowOnTab.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.optNewWindowOnTab.Validated += new System.EventHandler(this.OnControlValidated);
			this.optNewWindowOnTab.CheckedChanged += new System.EventHandler(this.optOnOpenNewWindowChecked);
			// 
			// checkReuseFirstBrowserTab
			// 
			this.checkReuseFirstBrowserTab.AccessibleDescription = resources.GetString("checkReuseFirstBrowserTab.AccessibleDescription");
			this.checkReuseFirstBrowserTab.AccessibleName = resources.GetString("checkReuseFirstBrowserTab.AccessibleName");
			this.checkReuseFirstBrowserTab.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkReuseFirstBrowserTab.Anchor")));
			this.checkReuseFirstBrowserTab.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkReuseFirstBrowserTab.Appearance")));
			this.checkReuseFirstBrowserTab.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkReuseFirstBrowserTab.BackgroundImage")));
			this.checkReuseFirstBrowserTab.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkReuseFirstBrowserTab.CheckAlign")));
			this.checkReuseFirstBrowserTab.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkReuseFirstBrowserTab.Dock")));
			this.checkReuseFirstBrowserTab.Enabled = ((bool)(resources.GetObject("checkReuseFirstBrowserTab.Enabled")));
			this.errorProvider1.SetError(this.checkReuseFirstBrowserTab, resources.GetString("checkReuseFirstBrowserTab.Error"));
			this.checkReuseFirstBrowserTab.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkReuseFirstBrowserTab.FlatStyle")));
			this.checkReuseFirstBrowserTab.Font = ((System.Drawing.Font)(resources.GetObject("checkReuseFirstBrowserTab.Font")));
			this.errorProvider1.SetIconAlignment(this.checkReuseFirstBrowserTab, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("checkReuseFirstBrowserTab.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.checkReuseFirstBrowserTab, ((int)(resources.GetObject("checkReuseFirstBrowserTab.IconPadding"))));
			this.checkReuseFirstBrowserTab.Image = ((System.Drawing.Image)(resources.GetObject("checkReuseFirstBrowserTab.Image")));
			this.checkReuseFirstBrowserTab.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkReuseFirstBrowserTab.ImageAlign")));
			this.checkReuseFirstBrowserTab.ImageIndex = ((int)(resources.GetObject("checkReuseFirstBrowserTab.ImageIndex")));
			this.checkReuseFirstBrowserTab.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkReuseFirstBrowserTab.ImeMode")));
			this.checkReuseFirstBrowserTab.Location = ((System.Drawing.Point)(resources.GetObject("checkReuseFirstBrowserTab.Location")));
			this.checkReuseFirstBrowserTab.Name = "checkReuseFirstBrowserTab";
			this.checkReuseFirstBrowserTab.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkReuseFirstBrowserTab.RightToLeft")));
			this.checkReuseFirstBrowserTab.Size = ((System.Drawing.Size)(resources.GetObject("checkReuseFirstBrowserTab.Size")));
			this.checkReuseFirstBrowserTab.TabIndex = ((int)(resources.GetObject("checkReuseFirstBrowserTab.TabIndex")));
			this.checkReuseFirstBrowserTab.Text = resources.GetString("checkReuseFirstBrowserTab.Text");
			this.checkReuseFirstBrowserTab.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkReuseFirstBrowserTab.TextAlign")));
			this.toolTip1.SetToolTip(this.checkReuseFirstBrowserTab, resources.GetString("checkReuseFirstBrowserTab.ToolTip"));
			this.checkReuseFirstBrowserTab.Visible = ((bool)(resources.GetObject("checkReuseFirstBrowserTab.Visible")));
			this.checkReuseFirstBrowserTab.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.checkReuseFirstBrowserTab.Validated += new System.EventHandler(this.OnControlValidated);
			this.checkReuseFirstBrowserTab.CheckedChanged += new System.EventHandler(this.OnAnyCheckedChanged);
			// 
			// btnSelectExecutable
			// 
			this.btnSelectExecutable.AccessibleDescription = resources.GetString("btnSelectExecutable.AccessibleDescription");
			this.btnSelectExecutable.AccessibleName = resources.GetString("btnSelectExecutable.AccessibleName");
			this.btnSelectExecutable.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSelectExecutable.Anchor")));
			this.btnSelectExecutable.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSelectExecutable.BackgroundImage")));
			this.btnSelectExecutable.CausesValidation = false;
			this.btnSelectExecutable.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSelectExecutable.Dock")));
			this.btnSelectExecutable.Enabled = ((bool)(resources.GetObject("btnSelectExecutable.Enabled")));
			this.errorProvider1.SetError(this.btnSelectExecutable, resources.GetString("btnSelectExecutable.Error"));
			this.btnSelectExecutable.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSelectExecutable.FlatStyle")));
			this.btnSelectExecutable.Font = ((System.Drawing.Font)(resources.GetObject("btnSelectExecutable.Font")));
			this.errorProvider1.SetIconAlignment(this.btnSelectExecutable, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSelectExecutable.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnSelectExecutable, ((int)(resources.GetObject("btnSelectExecutable.IconPadding"))));
			this.btnSelectExecutable.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectExecutable.Image")));
			this.btnSelectExecutable.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSelectExecutable.ImageAlign")));
			this.btnSelectExecutable.ImageIndex = ((int)(resources.GetObject("btnSelectExecutable.ImageIndex")));
			this.btnSelectExecutable.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSelectExecutable.ImeMode")));
			this.btnSelectExecutable.Location = ((System.Drawing.Point)(resources.GetObject("btnSelectExecutable.Location")));
			this.btnSelectExecutable.Name = "btnSelectExecutable";
			this.btnSelectExecutable.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSelectExecutable.RightToLeft")));
			this.btnSelectExecutable.Size = ((System.Drawing.Size)(resources.GetObject("btnSelectExecutable.Size")));
			this.btnSelectExecutable.TabIndex = ((int)(resources.GetObject("btnSelectExecutable.TabIndex")));
			this.btnSelectExecutable.Text = resources.GetString("btnSelectExecutable.Text");
			this.btnSelectExecutable.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSelectExecutable.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSelectExecutable, resources.GetString("btnSelectExecutable.ToolTip"));
			this.btnSelectExecutable.Visible = ((bool)(resources.GetObject("btnSelectExecutable.Visible")));
			this.btnSelectExecutable.Click += new System.EventHandler(this.btnSelectExecutable_Click);
			// 
			// txtBrowserStartExecutable
			// 
			this.txtBrowserStartExecutable.AccessibleDescription = resources.GetString("txtBrowserStartExecutable.AccessibleDescription");
			this.txtBrowserStartExecutable.AccessibleName = resources.GetString("txtBrowserStartExecutable.AccessibleName");
			this.txtBrowserStartExecutable.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtBrowserStartExecutable.Anchor")));
			this.txtBrowserStartExecutable.AutoSize = ((bool)(resources.GetObject("txtBrowserStartExecutable.AutoSize")));
			this.txtBrowserStartExecutable.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtBrowserStartExecutable.BackgroundImage")));
			this.txtBrowserStartExecutable.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtBrowserStartExecutable.Dock")));
			this.txtBrowserStartExecutable.Enabled = ((bool)(resources.GetObject("txtBrowserStartExecutable.Enabled")));
			this.errorProvider1.SetError(this.txtBrowserStartExecutable, resources.GetString("txtBrowserStartExecutable.Error"));
			this.txtBrowserStartExecutable.Font = ((System.Drawing.Font)(resources.GetObject("txtBrowserStartExecutable.Font")));
			this.errorProvider1.SetIconAlignment(this.txtBrowserStartExecutable, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtBrowserStartExecutable.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.txtBrowserStartExecutable, ((int)(resources.GetObject("txtBrowserStartExecutable.IconPadding"))));
			this.txtBrowserStartExecutable.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtBrowserStartExecutable.ImeMode")));
			this.txtBrowserStartExecutable.Location = ((System.Drawing.Point)(resources.GetObject("txtBrowserStartExecutable.Location")));
			this.txtBrowserStartExecutable.MaxLength = ((int)(resources.GetObject("txtBrowserStartExecutable.MaxLength")));
			this.txtBrowserStartExecutable.Multiline = ((bool)(resources.GetObject("txtBrowserStartExecutable.Multiline")));
			this.txtBrowserStartExecutable.Name = "txtBrowserStartExecutable";
			this.txtBrowserStartExecutable.PasswordChar = ((char)(resources.GetObject("txtBrowserStartExecutable.PasswordChar")));
			this.txtBrowserStartExecutable.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtBrowserStartExecutable.RightToLeft")));
			this.txtBrowserStartExecutable.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtBrowserStartExecutable.ScrollBars")));
			this.txtBrowserStartExecutable.Size = ((System.Drawing.Size)(resources.GetObject("txtBrowserStartExecutable.Size")));
			this.txtBrowserStartExecutable.TabIndex = ((int)(resources.GetObject("txtBrowserStartExecutable.TabIndex")));
			this.txtBrowserStartExecutable.Text = resources.GetString("txtBrowserStartExecutable.Text");
			this.txtBrowserStartExecutable.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtBrowserStartExecutable.TextAlign")));
			this.toolTip1.SetToolTip(this.txtBrowserStartExecutable, resources.GetString("txtBrowserStartExecutable.ToolTip"));
			this.txtBrowserStartExecutable.Visible = ((bool)(resources.GetObject("txtBrowserStartExecutable.Visible")));
			this.txtBrowserStartExecutable.WordWrap = ((bool)(resources.GetObject("txtBrowserStartExecutable.WordWrap")));
			this.txtBrowserStartExecutable.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.txtBrowserStartExecutable.Validated += new System.EventHandler(this.OnControlValidated);
			// 
			// labelBrowserStartExecutable
			// 
			this.labelBrowserStartExecutable.AccessibleDescription = resources.GetString("labelBrowserStartExecutable.AccessibleDescription");
			this.labelBrowserStartExecutable.AccessibleName = resources.GetString("labelBrowserStartExecutable.AccessibleName");
			this.labelBrowserStartExecutable.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelBrowserStartExecutable.Anchor")));
			this.labelBrowserStartExecutable.AutoSize = ((bool)(resources.GetObject("labelBrowserStartExecutable.AutoSize")));
			this.labelBrowserStartExecutable.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelBrowserStartExecutable.Dock")));
			this.labelBrowserStartExecutable.Enabled = ((bool)(resources.GetObject("labelBrowserStartExecutable.Enabled")));
			this.errorProvider1.SetError(this.labelBrowserStartExecutable, resources.GetString("labelBrowserStartExecutable.Error"));
			this.labelBrowserStartExecutable.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelBrowserStartExecutable.Font = ((System.Drawing.Font)(resources.GetObject("labelBrowserStartExecutable.Font")));
			this.errorProvider1.SetIconAlignment(this.labelBrowserStartExecutable, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelBrowserStartExecutable.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.labelBrowserStartExecutable, ((int)(resources.GetObject("labelBrowserStartExecutable.IconPadding"))));
			this.labelBrowserStartExecutable.Image = ((System.Drawing.Image)(resources.GetObject("labelBrowserStartExecutable.Image")));
			this.labelBrowserStartExecutable.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelBrowserStartExecutable.ImageAlign")));
			this.labelBrowserStartExecutable.ImageIndex = ((int)(resources.GetObject("labelBrowserStartExecutable.ImageIndex")));
			this.labelBrowserStartExecutable.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelBrowserStartExecutable.ImeMode")));
			this.labelBrowserStartExecutable.Location = ((System.Drawing.Point)(resources.GetObject("labelBrowserStartExecutable.Location")));
			this.labelBrowserStartExecutable.Name = "labelBrowserStartExecutable";
			this.labelBrowserStartExecutable.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelBrowserStartExecutable.RightToLeft")));
			this.labelBrowserStartExecutable.Size = ((System.Drawing.Size)(resources.GetObject("labelBrowserStartExecutable.Size")));
			this.labelBrowserStartExecutable.TabIndex = ((int)(resources.GetObject("labelBrowserStartExecutable.TabIndex")));
			this.labelBrowserStartExecutable.Text = resources.GetString("labelBrowserStartExecutable.Text");
			this.labelBrowserStartExecutable.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelBrowserStartExecutable.TextAlign")));
			this.toolTip1.SetToolTip(this.labelBrowserStartExecutable, resources.GetString("labelBrowserStartExecutable.ToolTip"));
			this.labelBrowserStartExecutable.Visible = ((bool)(resources.GetObject("labelBrowserStartExecutable.Visible")));
			// 
			// label16
			// 
			this.label16.AccessibleDescription = resources.GetString("label16.AccessibleDescription");
			this.label16.AccessibleName = resources.GetString("label16.AccessibleName");
			this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label16.Anchor")));
			this.label16.AutoSize = ((bool)(resources.GetObject("label16.AutoSize")));
			this.label16.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label16.Dock")));
			this.label16.Enabled = ((bool)(resources.GetObject("label16.Enabled")));
			this.errorProvider1.SetError(this.label16, resources.GetString("label16.Error"));
			this.label16.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label16.Font = ((System.Drawing.Font)(resources.GetObject("label16.Font")));
			this.errorProvider1.SetIconAlignment(this.label16, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label16.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label16, ((int)(resources.GetObject("label16.IconPadding"))));
			this.label16.Image = ((System.Drawing.Image)(resources.GetObject("label16.Image")));
			this.label16.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label16.ImageAlign")));
			this.label16.ImageIndex = ((int)(resources.GetObject("label16.ImageIndex")));
			this.label16.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label16.ImeMode")));
			this.label16.Location = ((System.Drawing.Point)(resources.GetObject("label16.Location")));
			this.label16.Name = "label16";
			this.label16.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label16.RightToLeft")));
			this.label16.Size = ((System.Drawing.Size)(resources.GetObject("label16.Size")));
			this.label16.TabIndex = ((int)(resources.GetObject("label16.TabIndex")));
			this.label16.Text = resources.GetString("label16.Text");
			this.label16.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label16.TextAlign")));
			this.toolTip1.SetToolTip(this.label16, resources.GetString("label16.ToolTip"));
			this.label16.Visible = ((bool)(resources.GetObject("label16.Visible")));
			// 
			// tabWebSearch
			// 
			this.tabWebSearch.AccessibleDescription = resources.GetString("tabWebSearch.AccessibleDescription");
			this.tabWebSearch.AccessibleName = resources.GetString("tabWebSearch.AccessibleName");
			this.tabWebSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabWebSearch.Anchor")));
			this.tabWebSearch.AutoScroll = ((bool)(resources.GetObject("tabWebSearch.AutoScroll")));
			this.tabWebSearch.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabWebSearch.AutoScrollMargin")));
			this.tabWebSearch.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabWebSearch.AutoScrollMinSize")));
			this.tabWebSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabWebSearch.BackgroundImage")));
			this.tabWebSearch.Controls.Add(this.sectionPanelWebSearchEngines);
			this.tabWebSearch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabWebSearch.Dock")));
			this.tabWebSearch.Enabled = ((bool)(resources.GetObject("tabWebSearch.Enabled")));
			this.errorProvider1.SetError(this.tabWebSearch, resources.GetString("tabWebSearch.Error"));
			this.tabWebSearch.Font = ((System.Drawing.Font)(resources.GetObject("tabWebSearch.Font")));
			this.errorProvider1.SetIconAlignment(this.tabWebSearch, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("tabWebSearch.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.tabWebSearch, ((int)(resources.GetObject("tabWebSearch.IconPadding"))));
			this.tabWebSearch.ImageIndex = ((int)(resources.GetObject("tabWebSearch.ImageIndex")));
			this.tabWebSearch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabWebSearch.ImeMode")));
			this.tabWebSearch.Location = ((System.Drawing.Point)(resources.GetObject("tabWebSearch.Location")));
			this.tabWebSearch.Name = "tabWebSearch";
			this.tabWebSearch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabWebSearch.RightToLeft")));
			this.tabWebSearch.Size = ((System.Drawing.Size)(resources.GetObject("tabWebSearch.Size")));
			this.tabWebSearch.TabIndex = ((int)(resources.GetObject("tabWebSearch.TabIndex")));
			this.tabWebSearch.Text = resources.GetString("tabWebSearch.Text");
			this.toolTip1.SetToolTip(this.tabWebSearch, resources.GetString("tabWebSearch.ToolTip"));
			this.tabWebSearch.ToolTipText = resources.GetString("tabWebSearch.ToolTipText");
			this.tabWebSearch.Visible = ((bool)(resources.GetObject("tabWebSearch.Visible")));
			// 
			// sectionPanelWebSearchEngines
			// 
			this.sectionPanelWebSearchEngines.AccessibleDescription = resources.GetString("sectionPanelWebSearchEngines.AccessibleDescription");
			this.sectionPanelWebSearchEngines.AccessibleName = resources.GetString("sectionPanelWebSearchEngines.AccessibleName");
			this.sectionPanelWebSearchEngines.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("sectionPanelWebSearchEngines.Anchor")));
			this.sectionPanelWebSearchEngines.AutoScroll = ((bool)(resources.GetObject("sectionPanelWebSearchEngines.AutoScroll")));
			this.sectionPanelWebSearchEngines.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebSearchEngines.AutoScrollMargin")));
			this.sectionPanelWebSearchEngines.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebSearchEngines.AutoScrollMinSize")));
			this.sectionPanelWebSearchEngines.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebSearchEngines.BackgroundImage")));
			this.sectionPanelWebSearchEngines.Controls.Add(this.btnSERemove);
			this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEAdd);
			this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEProperties);
			this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEMoveDown);
			this.sectionPanelWebSearchEngines.Controls.Add(this.btnSEMoveUp);
			this.sectionPanelWebSearchEngines.Controls.Add(this.listSearchEngines);
			this.sectionPanelWebSearchEngines.Controls.Add(this.label17);
			this.sectionPanelWebSearchEngines.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("sectionPanelWebSearchEngines.Dock")));
			this.sectionPanelWebSearchEngines.Enabled = ((bool)(resources.GetObject("sectionPanelWebSearchEngines.Enabled")));
			this.errorProvider1.SetError(this.sectionPanelWebSearchEngines, resources.GetString("sectionPanelWebSearchEngines.Error"));
			this.sectionPanelWebSearchEngines.Font = ((System.Drawing.Font)(resources.GetObject("sectionPanelWebSearchEngines.Font")));
			this.errorProvider1.SetIconAlignment(this.sectionPanelWebSearchEngines, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("sectionPanelWebSearchEngines.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.sectionPanelWebSearchEngines, ((int)(resources.GetObject("sectionPanelWebSearchEngines.IconPadding"))));
			this.sectionPanelWebSearchEngines.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelWebSearchEngines.Image")));
			this.sectionPanelWebSearchEngines.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelWebSearchEngines.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("sectionPanelWebSearchEngines.ImeMode")));
			this.sectionPanelWebSearchEngines.Location = ((System.Drawing.Point)(resources.GetObject("sectionPanelWebSearchEngines.Location")));
			this.sectionPanelWebSearchEngines.Name = "sectionPanelWebSearchEngines";
			this.sectionPanelWebSearchEngines.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("sectionPanelWebSearchEngines.RightToLeft")));
			this.sectionPanelWebSearchEngines.Size = ((System.Drawing.Size)(resources.GetObject("sectionPanelWebSearchEngines.Size")));
			this.sectionPanelWebSearchEngines.TabIndex = ((int)(resources.GetObject("sectionPanelWebSearchEngines.TabIndex")));
			this.sectionPanelWebSearchEngines.Text = resources.GetString("sectionPanelWebSearchEngines.Text");
			this.toolTip1.SetToolTip(this.sectionPanelWebSearchEngines, resources.GetString("sectionPanelWebSearchEngines.ToolTip"));
			this.sectionPanelWebSearchEngines.Visible = ((bool)(resources.GetObject("sectionPanelWebSearchEngines.Visible")));
			// 
			// btnSERemove
			// 
			this.btnSERemove.AccessibleDescription = resources.GetString("btnSERemove.AccessibleDescription");
			this.btnSERemove.AccessibleName = resources.GetString("btnSERemove.AccessibleName");
			this.btnSERemove.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSERemove.Anchor")));
			this.btnSERemove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSERemove.BackgroundImage")));
			this.btnSERemove.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSERemove.Dock")));
			this.btnSERemove.Enabled = ((bool)(resources.GetObject("btnSERemove.Enabled")));
			this.errorProvider1.SetError(this.btnSERemove, resources.GetString("btnSERemove.Error"));
			this.btnSERemove.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSERemove.FlatStyle")));
			this.btnSERemove.Font = ((System.Drawing.Font)(resources.GetObject("btnSERemove.Font")));
			this.errorProvider1.SetIconAlignment(this.btnSERemove, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSERemove.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnSERemove, ((int)(resources.GetObject("btnSERemove.IconPadding"))));
			this.btnSERemove.Image = ((System.Drawing.Image)(resources.GetObject("btnSERemove.Image")));
			this.btnSERemove.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSERemove.ImageAlign")));
			this.btnSERemove.ImageIndex = ((int)(resources.GetObject("btnSERemove.ImageIndex")));
			this.btnSERemove.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSERemove.ImeMode")));
			this.btnSERemove.Location = ((System.Drawing.Point)(resources.GetObject("btnSERemove.Location")));
			this.btnSERemove.Name = "btnSERemove";
			this.btnSERemove.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSERemove.RightToLeft")));
			this.btnSERemove.Size = ((System.Drawing.Size)(resources.GetObject("btnSERemove.Size")));
			this.btnSERemove.TabIndex = ((int)(resources.GetObject("btnSERemove.TabIndex")));
			this.btnSERemove.Text = resources.GetString("btnSERemove.Text");
			this.btnSERemove.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSERemove.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSERemove, resources.GetString("btnSERemove.ToolTip"));
			this.btnSERemove.Visible = ((bool)(resources.GetObject("btnSERemove.Visible")));
			this.btnSERemove.Click += new System.EventHandler(this.btnSERemove_Click);
			// 
			// btnSEAdd
			// 
			this.btnSEAdd.AccessibleDescription = resources.GetString("btnSEAdd.AccessibleDescription");
			this.btnSEAdd.AccessibleName = resources.GetString("btnSEAdd.AccessibleName");
			this.btnSEAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSEAdd.Anchor")));
			this.btnSEAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSEAdd.BackgroundImage")));
			this.btnSEAdd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSEAdd.Dock")));
			this.btnSEAdd.Enabled = ((bool)(resources.GetObject("btnSEAdd.Enabled")));
			this.errorProvider1.SetError(this.btnSEAdd, resources.GetString("btnSEAdd.Error"));
			this.btnSEAdd.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSEAdd.FlatStyle")));
			this.btnSEAdd.Font = ((System.Drawing.Font)(resources.GetObject("btnSEAdd.Font")));
			this.errorProvider1.SetIconAlignment(this.btnSEAdd, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSEAdd.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnSEAdd, ((int)(resources.GetObject("btnSEAdd.IconPadding"))));
			this.btnSEAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnSEAdd.Image")));
			this.btnSEAdd.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEAdd.ImageAlign")));
			this.btnSEAdd.ImageIndex = ((int)(resources.GetObject("btnSEAdd.ImageIndex")));
			this.btnSEAdd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSEAdd.ImeMode")));
			this.btnSEAdd.Location = ((System.Drawing.Point)(resources.GetObject("btnSEAdd.Location")));
			this.btnSEAdd.Name = "btnSEAdd";
			this.btnSEAdd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSEAdd.RightToLeft")));
			this.btnSEAdd.Size = ((System.Drawing.Size)(resources.GetObject("btnSEAdd.Size")));
			this.btnSEAdd.TabIndex = ((int)(resources.GetObject("btnSEAdd.TabIndex")));
			this.btnSEAdd.Text = resources.GetString("btnSEAdd.Text");
			this.btnSEAdd.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEAdd.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSEAdd, resources.GetString("btnSEAdd.ToolTip"));
			this.btnSEAdd.Visible = ((bool)(resources.GetObject("btnSEAdd.Visible")));
			this.btnSEAdd.Click += new System.EventHandler(this.btnSEAdd_Click);
			// 
			// btnSEProperties
			// 
			this.btnSEProperties.AccessibleDescription = resources.GetString("btnSEProperties.AccessibleDescription");
			this.btnSEProperties.AccessibleName = resources.GetString("btnSEProperties.AccessibleName");
			this.btnSEProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSEProperties.Anchor")));
			this.btnSEProperties.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSEProperties.BackgroundImage")));
			this.btnSEProperties.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSEProperties.Dock")));
			this.btnSEProperties.Enabled = ((bool)(resources.GetObject("btnSEProperties.Enabled")));
			this.errorProvider1.SetError(this.btnSEProperties, resources.GetString("btnSEProperties.Error"));
			this.btnSEProperties.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSEProperties.FlatStyle")));
			this.btnSEProperties.Font = ((System.Drawing.Font)(resources.GetObject("btnSEProperties.Font")));
			this.errorProvider1.SetIconAlignment(this.btnSEProperties, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSEProperties.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnSEProperties, ((int)(resources.GetObject("btnSEProperties.IconPadding"))));
			this.btnSEProperties.Image = ((System.Drawing.Image)(resources.GetObject("btnSEProperties.Image")));
			this.btnSEProperties.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEProperties.ImageAlign")));
			this.btnSEProperties.ImageIndex = ((int)(resources.GetObject("btnSEProperties.ImageIndex")));
			this.btnSEProperties.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSEProperties.ImeMode")));
			this.btnSEProperties.Location = ((System.Drawing.Point)(resources.GetObject("btnSEProperties.Location")));
			this.btnSEProperties.Name = "btnSEProperties";
			this.btnSEProperties.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSEProperties.RightToLeft")));
			this.btnSEProperties.Size = ((System.Drawing.Size)(resources.GetObject("btnSEProperties.Size")));
			this.btnSEProperties.TabIndex = ((int)(resources.GetObject("btnSEProperties.TabIndex")));
			this.btnSEProperties.Text = resources.GetString("btnSEProperties.Text");
			this.btnSEProperties.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEProperties.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSEProperties, resources.GetString("btnSEProperties.ToolTip"));
			this.btnSEProperties.Visible = ((bool)(resources.GetObject("btnSEProperties.Visible")));
			this.btnSEProperties.Click += new System.EventHandler(this.btnSEProperties_Click);
			// 
			// btnSEMoveDown
			// 
			this.btnSEMoveDown.AccessibleDescription = resources.GetString("btnSEMoveDown.AccessibleDescription");
			this.btnSEMoveDown.AccessibleName = resources.GetString("btnSEMoveDown.AccessibleName");
			this.btnSEMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSEMoveDown.Anchor")));
			this.btnSEMoveDown.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSEMoveDown.BackgroundImage")));
			this.btnSEMoveDown.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSEMoveDown.Dock")));
			this.btnSEMoveDown.Enabled = ((bool)(resources.GetObject("btnSEMoveDown.Enabled")));
			this.errorProvider1.SetError(this.btnSEMoveDown, resources.GetString("btnSEMoveDown.Error"));
			this.btnSEMoveDown.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSEMoveDown.FlatStyle")));
			this.btnSEMoveDown.Font = ((System.Drawing.Font)(resources.GetObject("btnSEMoveDown.Font")));
			this.errorProvider1.SetIconAlignment(this.btnSEMoveDown, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSEMoveDown.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnSEMoveDown, ((int)(resources.GetObject("btnSEMoveDown.IconPadding"))));
			this.btnSEMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("btnSEMoveDown.Image")));
			this.btnSEMoveDown.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEMoveDown.ImageAlign")));
			this.btnSEMoveDown.ImageIndex = ((int)(resources.GetObject("btnSEMoveDown.ImageIndex")));
			this.btnSEMoveDown.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSEMoveDown.ImeMode")));
			this.btnSEMoveDown.Location = ((System.Drawing.Point)(resources.GetObject("btnSEMoveDown.Location")));
			this.btnSEMoveDown.Name = "btnSEMoveDown";
			this.btnSEMoveDown.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSEMoveDown.RightToLeft")));
			this.btnSEMoveDown.Size = ((System.Drawing.Size)(resources.GetObject("btnSEMoveDown.Size")));
			this.btnSEMoveDown.TabIndex = ((int)(resources.GetObject("btnSEMoveDown.TabIndex")));
			this.btnSEMoveDown.Text = resources.GetString("btnSEMoveDown.Text");
			this.btnSEMoveDown.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEMoveDown.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSEMoveDown, resources.GetString("btnSEMoveDown.ToolTip"));
			this.btnSEMoveDown.Visible = ((bool)(resources.GetObject("btnSEMoveDown.Visible")));
			this.btnSEMoveDown.Click += new System.EventHandler(this.btnSEMoveDown_Click);
			// 
			// btnSEMoveUp
			// 
			this.btnSEMoveUp.AccessibleDescription = resources.GetString("btnSEMoveUp.AccessibleDescription");
			this.btnSEMoveUp.AccessibleName = resources.GetString("btnSEMoveUp.AccessibleName");
			this.btnSEMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSEMoveUp.Anchor")));
			this.btnSEMoveUp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSEMoveUp.BackgroundImage")));
			this.btnSEMoveUp.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSEMoveUp.Dock")));
			this.btnSEMoveUp.Enabled = ((bool)(resources.GetObject("btnSEMoveUp.Enabled")));
			this.errorProvider1.SetError(this.btnSEMoveUp, resources.GetString("btnSEMoveUp.Error"));
			this.btnSEMoveUp.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSEMoveUp.FlatStyle")));
			this.btnSEMoveUp.Font = ((System.Drawing.Font)(resources.GetObject("btnSEMoveUp.Font")));
			this.errorProvider1.SetIconAlignment(this.btnSEMoveUp, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSEMoveUp.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnSEMoveUp, ((int)(resources.GetObject("btnSEMoveUp.IconPadding"))));
			this.btnSEMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("btnSEMoveUp.Image")));
			this.btnSEMoveUp.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEMoveUp.ImageAlign")));
			this.btnSEMoveUp.ImageIndex = ((int)(resources.GetObject("btnSEMoveUp.ImageIndex")));
			this.btnSEMoveUp.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSEMoveUp.ImeMode")));
			this.btnSEMoveUp.Location = ((System.Drawing.Point)(resources.GetObject("btnSEMoveUp.Location")));
			this.btnSEMoveUp.Name = "btnSEMoveUp";
			this.btnSEMoveUp.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSEMoveUp.RightToLeft")));
			this.btnSEMoveUp.Size = ((System.Drawing.Size)(resources.GetObject("btnSEMoveUp.Size")));
			this.btnSEMoveUp.TabIndex = ((int)(resources.GetObject("btnSEMoveUp.TabIndex")));
			this.btnSEMoveUp.Text = resources.GetString("btnSEMoveUp.Text");
			this.btnSEMoveUp.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSEMoveUp.TextAlign")));
			this.toolTip1.SetToolTip(this.btnSEMoveUp, resources.GetString("btnSEMoveUp.ToolTip"));
			this.btnSEMoveUp.Visible = ((bool)(resources.GetObject("btnSEMoveUp.Visible")));
			this.btnSEMoveUp.Click += new System.EventHandler(this.btnSEMoveUp_Click);
			// 
			// listSearchEngines
			// 
			this.listSearchEngines.AccessibleDescription = resources.GetString("listSearchEngines.AccessibleDescription");
			this.listSearchEngines.AccessibleName = resources.GetString("listSearchEngines.AccessibleName");
			this.listSearchEngines.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.listSearchEngines.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listSearchEngines.Alignment")));
			this.listSearchEngines.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listSearchEngines.Anchor")));
			this.listSearchEngines.AutoArrange = false;
			this.listSearchEngines.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listSearchEngines.BackgroundImage")));
			this.listSearchEngines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listSearchEngines.CheckBoxes = true;
			this.listSearchEngines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								this.columnHeader0,
																								this.columnHeader1,
																								this.columnHeader2});
			this.listSearchEngines.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listSearchEngines.Dock")));
			this.listSearchEngines.Enabled = ((bool)(resources.GetObject("listSearchEngines.Enabled")));
			this.errorProvider1.SetError(this.listSearchEngines, resources.GetString("listSearchEngines.Error"));
			this.listSearchEngines.Font = ((System.Drawing.Font)(resources.GetObject("listSearchEngines.Font")));
			this.listSearchEngines.FullRowSelect = true;
			this.listSearchEngines.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listSearchEngines.HideSelection = false;
			this.errorProvider1.SetIconAlignment(this.listSearchEngines, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("listSearchEngines.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.listSearchEngines, ((int)(resources.GetObject("listSearchEngines.IconPadding"))));
			this.listSearchEngines.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listSearchEngines.ImeMode")));
			this.listSearchEngines.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																							  ((System.Windows.Forms.ListViewItem)(resources.GetObject("listSearchEngines.Items")))});
			this.listSearchEngines.LabelWrap = ((bool)(resources.GetObject("listSearchEngines.LabelWrap")));
			this.listSearchEngines.Location = ((System.Drawing.Point)(resources.GetObject("listSearchEngines.Location")));
			this.listSearchEngines.MultiSelect = false;
			this.listSearchEngines.Name = "listSearchEngines";
			this.listSearchEngines.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listSearchEngines.RightToLeft")));
			this.listSearchEngines.Size = ((System.Drawing.Size)(resources.GetObject("listSearchEngines.Size")));
			this.listSearchEngines.SmallImageList = this.imagesSearchEngines;
			this.listSearchEngines.TabIndex = ((int)(resources.GetObject("listSearchEngines.TabIndex")));
			this.listSearchEngines.Text = resources.GetString("listSearchEngines.Text");
			this.toolTip1.SetToolTip(this.listSearchEngines, resources.GetString("listSearchEngines.ToolTip"));
			this.listSearchEngines.View = System.Windows.Forms.View.Details;
			this.listSearchEngines.Visible = ((bool)(resources.GetObject("listSearchEngines.Visible")));
			this.listSearchEngines.ItemActivate += new System.EventHandler(this.OnSearchEngineItemActivate);
			this.listSearchEngines.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnSearchEnginesListMouseUp);
			this.listSearchEngines.Validating += new System.ComponentModel.CancelEventHandler(this.OnControlValidating);
			this.listSearchEngines.Validated += new System.EventHandler(this.OnControlValidated);
			this.listSearchEngines.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnSearchEngineItemChecked);
			// 
			// columnHeader0
			// 
			this.columnHeader0.Text = resources.GetString("columnHeader0.Text");
			this.columnHeader0.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader0.TextAlign")));
			this.columnHeader0.Width = ((int)(resources.GetObject("columnHeader0.Width")));
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = resources.GetString("columnHeader1.Text");
			this.columnHeader1.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader1.TextAlign")));
			this.columnHeader1.Width = ((int)(resources.GetObject("columnHeader1.Width")));
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = resources.GetString("columnHeader2.Text");
			this.columnHeader2.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader2.TextAlign")));
			this.columnHeader2.Width = ((int)(resources.GetObject("columnHeader2.Width")));
			// 
			// imagesSearchEngines
			// 
			this.imagesSearchEngines.ImageSize = ((System.Drawing.Size)(resources.GetObject("imagesSearchEngines.ImageSize")));
			this.imagesSearchEngines.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// label17
			// 
			this.label17.AccessibleDescription = resources.GetString("label17.AccessibleDescription");
			this.label17.AccessibleName = resources.GetString("label17.AccessibleName");
			this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label17.Anchor")));
			this.label17.AutoSize = ((bool)(resources.GetObject("label17.AutoSize")));
			this.label17.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label17.Dock")));
			this.label17.Enabled = ((bool)(resources.GetObject("label17.Enabled")));
			this.errorProvider1.SetError(this.label17, resources.GetString("label17.Error"));
			this.label17.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label17.Font = ((System.Drawing.Font)(resources.GetObject("label17.Font")));
			this.errorProvider1.SetIconAlignment(this.label17, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label17.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.label17, ((int)(resources.GetObject("label17.IconPadding"))));
			this.label17.Image = ((System.Drawing.Image)(resources.GetObject("label17.Image")));
			this.label17.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label17.ImageAlign")));
			this.label17.ImageIndex = ((int)(resources.GetObject("label17.ImageIndex")));
			this.label17.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label17.ImeMode")));
			this.label17.Location = ((System.Drawing.Point)(resources.GetObject("label17.Location")));
			this.label17.Name = "label17";
			this.label17.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label17.RightToLeft")));
			this.label17.Size = ((System.Drawing.Size)(resources.GetObject("label17.Size")));
			this.label17.TabIndex = ((int)(resources.GetObject("label17.TabIndex")));
			this.label17.Text = resources.GetString("label17.Text");
			this.label17.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label17.TextAlign")));
			this.toolTip1.SetToolTip(this.label17, resources.GetString("label17.ToolTip"));
			this.label17.Visible = ((bool)(resources.GetObject("label17.Visible")));
			// 
			// btnApply
			// 
			this.btnApply.AccessibleDescription = resources.GetString("btnApply.AccessibleDescription");
			this.btnApply.AccessibleName = resources.GetString("btnApply.AccessibleName");
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnApply.Anchor")));
			this.btnApply.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnApply.BackgroundImage")));
			this.btnApply.CausesValidation = false;
			this.btnApply.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnApply.Dock")));
			this.btnApply.Enabled = ((bool)(resources.GetObject("btnApply.Enabled")));
			this.errorProvider1.SetError(this.btnApply, resources.GetString("btnApply.Error"));
			this.btnApply.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnApply.FlatStyle")));
			this.btnApply.Font = ((System.Drawing.Font)(resources.GetObject("btnApply.Font")));
			this.errorProvider1.SetIconAlignment(this.btnApply, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnApply.IconAlignment"))));
			this.errorProvider1.SetIconPadding(this.btnApply, ((int)(resources.GetObject("btnApply.IconPadding"))));
			this.btnApply.Image = ((System.Drawing.Image)(resources.GetObject("btnApply.Image")));
			this.btnApply.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnApply.ImageAlign")));
			this.btnApply.ImageIndex = ((int)(resources.GetObject("btnApply.ImageIndex")));
			this.btnApply.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnApply.ImeMode")));
			this.btnApply.Location = ((System.Drawing.Point)(resources.GetObject("btnApply.Location")));
			this.btnApply.Name = "btnApply";
			this.btnApply.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnApply.RightToLeft")));
			this.btnApply.Size = ((System.Drawing.Size)(resources.GetObject("btnApply.Size")));
			this.btnApply.TabIndex = ((int)(resources.GetObject("btnApply.TabIndex")));
			this.btnApply.Text = resources.GetString("btnApply.Text");
			this.btnApply.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnApply.TextAlign")));
			this.toolTip1.SetToolTip(this.btnApply, resources.GetString("btnApply.ToolTip"));
			this.btnApply.Visible = ((bool)(resources.GetObject("btnApply.Visible")));
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			this.errorProvider1.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProvider1.Icon")));
			// 
			// fontDialog1
			// 
			this.fontDialog1.ShowColor = true;
			// 
			// openExeFileDialog
			// 
			this.openExeFileDialog.DefaultExt = "exe";
			this.openExeFileDialog.Filter = resources.GetString("openExeFileDialog.Filter");
			this.openExeFileDialog.Title = resources.GetString("openExeFileDialog.Title");
			// 
			// PreferencesDialog
			// 
			this.AcceptButton = this.btnOK;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.tabPrefs);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "PreferencesDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.tabPrefs.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.sectionPanelGeneralStartup.ResumeLayout(false);
			this.sectionPanelGeneralBehavior.ResumeLayout(false);
			this.tabRemoteStorage.ResumeLayout(false);
			this.sectionPanelRemoteStorageFeedlist.ResumeLayout(false);
			this.tabNewsItems.ResumeLayout(false);
			this.sectionPanelDisplayGeneral.ResumeLayout(false);
			this.sectionPanelDisplayItemFormatting.ResumeLayout(false);
			this.tabFeeds.ResumeLayout(false);
			this.sectionPanelFeedsCommentDefs.ResumeLayout(false);
			this.sectionPanelFeedsTimings.ResumeLayout(false);
			this.tabNetConnection.ResumeLayout(false);
			this.sectionPanelNetworkConnectionProxy.ResumeLayout(false);
			this.tabFonts.ResumeLayout(false);
			this.sectionPanelFontsSubscriptions.ResumeLayout(false);
			this.tabWebBrowser.ResumeLayout(false);
			this.sectionPanelWebBrowserSecurity.ResumeLayout(false);
			this.sectionPanelWebBrowserOnNewWindow.ResumeLayout(false);
			this.tabWebSearch.ResumeLayout(false);
			this.sectionPanelWebSearchEngines.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public TimeSpan MaxItemAge {
			get { return Utils.MaxItemAgeFromIndex(this.comboMaxItemAge.SelectedIndex); }
			set { this.comboMaxItemAge.SelectedIndex = Utils.MaxItemAgeToIndex(value);	}
		}

		private Font ActiveItemStateFont {
			get {	return itemStateFonts[lstItemStates.SelectedIndex];	}
			set { itemStateFonts[lstItemStates.SelectedIndex] = value; }
		}
		private Color ActiveItemStateColor {
			get {	return itemStateColors[lstItemStates.SelectedIndex];	}
			set { itemStateColors[lstItemStates.SelectedIndex] = value; }
		}

		private FontStyle FontStyleFromCheckboxes() {
			FontStyle s = FontStyle.Regular;
			if (chkFontBold.Checked) s |= FontStyle.Bold;
			if (chkFontItalic.Checked) s |= FontStyle.Italic;
			if (chkFontStrikeout.Checked) s |= FontStyle.Strikeout;
			if (chkFontUnderline.Checked) s |= FontStyle.Underline;
			return s;
		}

		private void FontStyleToCheckboxes(FontStyle s) {
			// we use the checkState here to prevent the Checked event
			chkFontBold.CheckState =  ((s & FontStyle.Bold) == FontStyle.Bold) ? System.Windows.Forms.CheckState.Checked: System.Windows.Forms.CheckState.Unchecked;
			chkFontItalic.CheckState =  ((s & FontStyle.Italic) == FontStyle.Bold) ? System.Windows.Forms.CheckState.Checked: System.Windows.Forms.CheckState.Unchecked;
			chkFontStrikeout.CheckState =  ((s & FontStyle.Strikeout) == FontStyle.Bold) ? System.Windows.Forms.CheckState.Checked: System.Windows.Forms.CheckState.Unchecked;
			chkFontUnderline.CheckState =  ((s & FontStyle.Underline) == FontStyle.Bold) ? System.Windows.Forms.CheckState.Checked: System.Windows.Forms.CheckState.Unchecked;
		}

		private Font DefaultStateFont {
			get {	return FontForState(FontStates.Read);	}
			set { SetFontForState(FontStates.Read, value); }
		}
		private Color DefaultColor {
			get {	return ColorForState(FontStates.Read);	}
			set { SetColorForState(FontStates.Read, value); }
		}

		public Font FontForState(FontStates state) {
			return itemStateFonts[(int)state];	
		}
		public Color ColorForState(FontStates state) {
			return itemStateColors[(int)state];	
		}
		private void SetFontForState(FontStates state, Font f) {
			if (state == FontStates.Read)
				itemStateFonts[(int)state] = f;	
			else
				itemStateFonts[(int)state] = new Font(DefaultStateFont, f.Style);	
		}
		private void SetColorForState(FontStates state, Color c) {
			itemStateColors[(int)state] = c;	
		}

		private void RefreshFontsFromDefault() {
			Font def = this.DefaultStateFont;
			for (int i = 1; i < lstItemStates.Items.Count; i++) {
				// reset font family/size, but keep styles
				itemStateFonts[i] = new Font(def, itemStateFonts[i].Style);
			}
		}

		private void RefreshFontSample() {
			lblFontSampleABC.Font = this.ActiveItemStateFont;
			lblFontSampleABC.ForeColor = this.ActiveItemStateColor;
		}

		private void RefreshFontFamilySizeSample() {
			this.lblUsedFontNameSize.Text = String.Format("{0}, {1} pt",  this.DefaultStateFont.Name, this.DefaultStateFont.Size);
		}

		private void PopulateComboUserIdentityForComments(IDictionary identities, string defaultIdentity) {
			this.cboUserIdentityForComments.Items.Clear();
			foreach (UserIdentity ui in identities.Values) {
				this.cboUserIdentityForComments.Items.Add(ui.Name);
			}
			if (defaultIdentity != null && identities.Contains(defaultIdentity)) {
				this.cboUserIdentityForComments.Text = defaultIdentity;
			} else {
				if (this.cboUserIdentityForComments.Items.Count > 0)
					this.cboUserIdentityForComments.SelectedIndex = 0;
			}
		}

		private void checkUseProxy_CheckedChanged(object sender, System.EventArgs e) {
		
			bool useProxy = checkUseProxy.Checked;

			labelProxyAddress.Enabled = textProxyAddress.Enabled = useProxy; 
			labelProxyPort.Enabled = textProxyPort.Enabled = useProxy;
			checkProxyBypassLocal.Enabled = checkProxyAuth.Enabled = useProxy;
			
			if(useProxy) {
				if (textProxyAddress.Text.Trim().Length == 0)
					errorProvider1.SetError(textProxyAddress, Resource.Manager["RES_ExceptionNoProxyUrl"]);
				if (textProxyPort.Text.Trim().Length == 0)
					textProxyPort.Text = "8080";
				if (checkProxyBypassLocal.Checked) {
					labelProxyBypassList.Enabled = labelProxyBypassListHint.Enabled = textProxyBypassList.Enabled = true;
				}
				if (checkProxyAuth.Checked) {
					labelProxyCredentialUserName.Enabled = textProxyCredentialUser.Enabled = true; 
					labelProxyCredentialPassword.Enabled = textProxyCredentialPassword.Enabled = true;
					if (textProxyCredentialUser.Text.Trim().Length == 0)
						errorProvider1.SetError(textProxyCredentialUser, Resource.Manager["RES_ExceptionNoProxyAuthUser"]);
				}
			}
			else {
				labelProxyBypassList.Enabled = labelProxyBypassListHint.Enabled = textProxyBypassList.Enabled = false;
				labelProxyCredentialUserName.Enabled = textProxyCredentialUser.Enabled = false; 
				labelProxyCredentialPassword.Enabled = textProxyCredentialPassword.Enabled = false;
				errorProvider1.SetError(textProxyAddress, null);
				errorProvider1.SetError(textProxyPort, null);
				errorProvider1.SetError(textProxyCredentialUser, null);
			}

			if (e != null)		// not caused by calling from another code location, enable Apply button
				this.OnControlValidated(this, null);
			
		}

		private void checkProxyBypassLocal_CheckedChanged(object sender, System.EventArgs e) {
			labelProxyBypassList.Enabled = labelProxyBypassListHint.Enabled = textProxyBypassList.Enabled = checkProxyBypassLocal.Checked;
		}
		
		private void checkProxyAuth_CheckedChanged(object sender, System.EventArgs e) {
			labelProxyCredentialUserName.Enabled = textProxyCredentialUser.Enabled = checkProxyAuth.Checked; 
			labelProxyCredentialPassword.Enabled = textProxyCredentialPassword.Enabled = checkProxyAuth.Checked;
			if (checkProxyAuth.Checked) {
				if (textProxyCredentialUser.Text.Trim().Length == 0)
					errorProvider1.SetError(textProxyCredentialUser, Resource.Manager["RES_ExceptionNoProxyAuthUser"]);
			}
			else {
				errorProvider1.SetError(textProxyCredentialUser, null);
			}
			if (e != null)		// not caused by calling from another code location
				this.OnControlValidated(this, EventArgs.Empty);
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
			this.OnControlValidated(this, EventArgs.Empty);
		}

		private void OnOKClick(object sender, System.EventArgs e) {
			if (checkProxyAuth.Checked) {
				if (textProxyCredentialUser.Text.Trim().Length == 0 ||
					textProxyCredentialPassword.Text.Trim().Length == 0)
					checkProxyAuth.Checked = false;
			}

			if (checkUseProxy.Checked) {
				if (textProxyAddress.Text.Trim().Length == 0 ||
					textProxyPort.Text.Trim().Length == 0) {
					checkUseProxy.Checked = false;
					checkNoProxy.Checked = true;
				}
			}


			if (checkUseRemoteStorage.Checked) {
				if (comboRemoteStorageProtocol.SelectedIndex != 0 && 
					comboRemoteStorageProtocol.SelectedIndex != 4 /* WebDAV: auth. is optional */ && 
					textRemoteStorageUserName.Text.Trim().Length == 0 || 
					textRemoteStorageLocation.Text.Trim().Length == 0) {
					checkUseRemoteStorage.Checked = false;
				}
			}

			if (optNewWindowCustomExec.Checked) {
				if (txtBrowserStartExecutable.Text.Trim().Length == 0)
					optNewWindowOnTab.Checked = true;
			}
		}

		private void OnDefaultFontChangeClick(object sender, System.EventArgs e) {
			fontDialog1.Font = this.DefaultStateFont;
			fontDialog1.Color = this.DefaultColor;

			if (fontDialog1.ShowDialog(this) != DialogResult.Cancel) {
				this.DefaultStateFont = fontDialog1.Font;
				this.DefaultColor = fontDialog1.Color;

				this.RefreshFontFamilySizeSample();
				this.RefreshFontsFromDefault();

				if (lstItemStates.SelectedIndex == 0) {
					// refresh style checkboxes
					this.FontStyleToCheckboxes(this.DefaultStateFont.Style);
				}
				this.RefreshFontSample();
				OnControlValidated(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Enable or disable remote storage
		/// </summary>
		private void checkUseRemoteStorage_CheckedChanged(object sender, System.EventArgs e)
		{
			bool enabled = checkUseRemoteStorage.Checked;

			textRemoteStorageUserName.Enabled = enabled;
			textRemoteStoragePassword.Enabled = enabled;
			comboRemoteStorageProtocol.Enabled = enabled;
			textRemoteStorageLocation.Enabled = enabled;
			if (enabled) {
				if (textRemoteStorageLocation.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageLocation, Resource.Manager["RES_ExceptionNoRemoteStorageLocation"]);
				} else if 	(comboRemoteStorageProtocol.SelectedIndex != 0 && 
					comboRemoteStorageProtocol.SelectedIndex != 4 /* WebDAV: auth. is optional */ && 
					textRemoteStorageUserName.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageUserName, Resource.Manager["RES_ExceptionNoRemoteStorageAuthUser"]);
				} else {
					errorProvider1.SetError(textRemoteStorageLocation, null);
					errorProvider1.SetError(textRemoteStorageUserName, null);
				}

			} else {
				errorProvider1.SetError(textRemoteStorageLocation, null);
				errorProvider1.SetError(textRemoteStorageUserName, null);
			}

			if (e != null)		// not caused by calling from another code location, enable Apply button
				this.OnControlValidated(this, null);
		}

		private void comboRemoteStorageProtocol_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			bool showAuth = false;

			switch (comboRemoteStorageProtocol.SelectedIndex)	{

				case 0: // "File Share" 	(use index, no strings to enable localization)
					labelRemoteStorageLocation.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.fileShare"];
					labelExperimental.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.fileShare.hint"];				
					showAuth = false;
					break;
				case 1: // "FTP"
					labelRemoteStorageLocation.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.ftp"];
					labelExperimental.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.ftp.hint"];				
					showAuth = true;
					break;
				case 2: //"dasBlog"
					labelRemoteStorageLocation.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.dasBlog"];
					labelExperimental.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.dasBlog.hint"];
					showAuth = true;
					break;
				case 3: //"dasBlog 1.3"
					labelRemoteStorageLocation.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.dasBlog"];
					labelExperimental.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.dasBlog.hint"];
					showAuth = true;
					break;
				case 4: //"WebDAV"
					labelRemoteStorageLocation.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.WebDAV"];
					labelExperimental.Text = Resource.Manager["RES_LabelTextRemoteStorageLocation.WebDAV.hint"];
					showAuth = true;
					break;
				default:
					// There is no selected protocol or the selected one is invalid;
					// use some defaults
					Debug.Assert(false);
					break;
			}

			// Hide the auth boxes if they're not relevant
			labelRemoteStorageUserName.Visible = textRemoteStorageUserName.Visible = showAuth;
			labelRemoteStoragePassword.Visible = textRemoteStoragePassword.Visible = showAuth;
		}

		private void optNewWindowCustomExec_CheckedChanged(object sender, System.EventArgs e) {
			labelBrowserStartExecutable.Enabled = txtBrowserStartExecutable.Enabled = btnSelectExecutable.Enabled = optNewWindowCustomExec.Checked;
			if (optNewWindowCustomExec.Checked && txtBrowserStartExecutable.Text.Trim().Length == 0) {
				//errorProvider1.SetError(txtBrowserStartExecutable, Resource.Manager["RES_ExceptionNoOnNewWindowExecutable"]);
			}
			if (optNewWindowCustomExec.Checked && checkReuseFirstBrowserTab.Enabled)
				checkReuseFirstBrowserTab.Enabled = false;
			OnControlValidated(this, EventArgs.Empty);
		}

		/// <summary>
		/// Initialze the WebSearchEngines Tab Page
		/// </summary>
		private void InitWebSearchEnginesTab() {
			
			this.imageIndexMap.Clear();
			this.imagesSearchEngines.Images.Clear();
			this.listSearchEngines.Items.Clear();

			int i = 0;
			foreach (SearchEngine engine in this.searchEngines) {

				string t = String.Empty, d = String.Empty;
				if (engine.Title != null) t = engine.Title;
				if (engine.Description != null) d = engine.Description;

				ListViewItem lv = new ListViewItem(new string[]{String.Empty, t, d});

				if (engine.ImageName != null && engine.ImageName.Trim().Length > 0) {

					if (this.imageIndexMap.ContainsKey(engine.ImageName)) {
						lv.ImageIndex = (int)this.imageIndexMap[engine.ImageName];
					} else {
						string p = Path.Combine(RssBanditApplication.GetSearchesPath(), engine.ImageName);
						if (File.Exists(p)) {
							Image img = null;
							try {
								img = Image.FromFile(p);
								this.imagesSearchEngines.Images.Add(img);
								this.imageIndexMap.Add(engine.ImageName, i);
								lv.ImageIndex = i;
								i++;
							}
							catch (Exception e) { 
								_log.Debug("InitWebSearchEnginesTab() Exception",e);
							}
						}
					}
				}
				lv.Checked = engine.IsActive;
				lv.Tag = engine;
				this.listSearchEngines.Items.Add(lv);
			}

			this.listSearchEngines.Columns[0].Width = -1;
			this.listSearchEngines.Columns[1].Width = -1;
			this.listSearchEngines.Columns[2].Width = -2;
		}

		private void OnSearchEngineItemActivate(object sender, System.EventArgs e) {
			bool on = (this.listSearchEngines.SelectedItems.Count > 0);
			this.btnSEMoveUp.Enabled = (on && this.listSearchEngines.SelectedItems[0].Index > 0);
			this.btnSEMoveDown.Enabled =  (on && this.listSearchEngines.SelectedItems[0].Index < (this.listSearchEngines.Items.Count - 1));
			this.btnSEProperties.Enabled = this.btnSERemove.Enabled = on;
		}

		private void OnSearchEnginesListMouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			this.OnSearchEngineItemActivate(sender, null);
		}

		private void btnSEProperties_Click(object sender, System.EventArgs e) {
			SearchEngine engine = (SearchEngine)this.listSearchEngines.SelectedItems[0].Tag;
			ShowAndHandleEngineProperties(engine);
		}

		private void btnSEAdd_Click(object sender, System.EventArgs e) {
			ShowAndHandleEngineProperties(new SearchEngine());
		}

		private void btnSERemove_Click(object sender, System.EventArgs e) {
			ListViewItem lvi = this.listSearchEngines.SelectedItems[0];
			if (lvi != null) {
				int index = lvi.Index;
				this.listSearchEngines.Items.Remove(lvi);
				if (this.listSearchEngines.Items.Count > 0) {
					if (index < this.listSearchEngines.Items.Count)	// reselect the next item
						this.listSearchEngines.Items[index].Selected = true;
					else
						this.listSearchEngines.Items[this.listSearchEngines.Items.Count-1].Selected = true;
				} 

				SearchEngine engine = (SearchEngine)lvi.Tag;
				this.searchEngines.Remove (engine);
				this.searchEnginesModified = true;
				this.OnSearchEngineItemActivate(this, null);
			} 
		}

		private void btnSEMoveUp_Click(object sender, System.EventArgs e) {
			ListViewItem lvi = this.listSearchEngines.SelectedItems[0];
			if (lvi != null && lvi.Index > 0) {
				int index = lvi.Index;
				this.listSearchEngines.Items.Remove(lvi);
				this.listSearchEngines.Items.Insert(index-1, lvi);

				SearchEngine engine = (SearchEngine)lvi.Tag;
				this.searchEngines.Remove (engine);
				this.searchEngines.Insert(index-1, engine);
				
				this.searchEnginesModified = true;
			}
			this.OnSearchEngineItemActivate(this, null);
		}

		private void btnSEMoveDown_Click(object sender, System.EventArgs e) {
			ListViewItem lvi = this.listSearchEngines.SelectedItems[0];
			if (lvi != null && lvi.Index < this.listSearchEngines.Items.Count - 1) {
				int index = lvi.Index;
				this.listSearchEngines.Items.Remove(lvi);
				this.listSearchEngines.Items.Insert(index+1, lvi);
				
				SearchEngine engine = (SearchEngine)lvi.Tag;
				this.searchEngines.Remove (engine);
				this.searchEngines.Insert(index+1, engine);
				
				this.searchEnginesModified = true;
			} 
			this.OnSearchEngineItemActivate(this, null);
		}

		private void ShowAndHandleEngineProperties(SearchEngine engine) {
			
			SearchEngineProperties propertyDialog = new SearchEngineProperties(engine);
			
			if (engine.ImageName != null && this.imageIndexMap.ContainsKey(engine.ImageName)) {
				propertyDialog.pictureEngine.Image = this.imagesSearchEngines.Images[(int)this.imageIndexMap[engine.ImageName]];
			}
			
			if (propertyDialog.ShowDialog(this) == DialogResult.OK) {
				
				bool modified = false;
				engine = propertyDialog.Engine;
				string t = String.Empty, d = String.Empty, u = String.Empty, imn = String.Empty;
				int imageIndex = -1;

				if (engine.Title != null) t = engine.Title;
				if (propertyDialog.textCaption.Text.CompareTo(t) != 0) {
					t = propertyDialog.textCaption.Text.Trim();
					engine.Title = t;
					modified = true;
				}

				if (engine.Description != null) d = engine.Description;
				if (propertyDialog.textDesc.Text.CompareTo(d) != 0) {
					d = propertyDialog.textDesc.Text.Trim();
					engine.Description = d;
					modified = true;
				}
				
				if (engine.SearchLink != null) u = engine.SearchLink;
				if (propertyDialog.textUrl.Text.CompareTo(u) != 0) {
					u = propertyDialog.textUrl.Text.Trim();
					engine.SearchLink = u;
					modified = true;
				}

				if (engine.ReturnRssResult != propertyDialog.checkBoxResultsetIsRssFeed.Checked) {
					engine.ReturnRssResult = propertyDialog.checkBoxResultsetIsRssFeed.Checked;
					modified = true;
				}

				if (engine.MergeRssResult != propertyDialog.checkBoxMergeRssResultset.Checked) {
					engine.MergeRssResult = propertyDialog.checkBoxMergeRssResultset.Checked;
					modified = true;
				}

				if (engine.ImageName != null) imn = engine.ImageName;
				if (propertyDialog.textPicture.Text.CompareTo(imn) != 0) {
					imn = propertyDialog.textPicture.Text.Trim();
					if (imn.Length > 0) 
						engine.ImageName = imn;
					else
						engine.ImageName = String.Empty;
					modified = true;
				}

				if (this.imageIndexMap.ContainsKey(imn)) {

					imageIndex = (int)this.imageIndexMap[imn];

				} else {

					string p = null;
					if (imn.IndexOf(Path.DirectorySeparatorChar) > 0) 
						p = imn;
					else
						p = Path.Combine(RssBanditApplication.GetSearchesPath(), imn);
					
					if (File.Exists(p)) {
						Image img = null;
						try {
							img = Image.FromFile(p);
							this.imagesSearchEngines.Images.Add(img);
							imageIndex = this.imagesSearchEngines.Images.Count - 1;
							this.imageIndexMap.Add(imn, imageIndex);
						}
						catch (Exception ex) { 
							_log.Debug("AddWebSearchEngine() Exception", ex);
						}
					}
				}
				

				ListViewItem lv = null;

				if (!this.searchEngines.Contains(engine)) {
					engine.IsActive = true;	//activate new engine
					this.searchEngines.Add(engine);
					lv = new ListViewItem(new string[]{String.Empty, t, d});
					lv.Checked = engine.IsActive;
					lv.Tag = engine;
					lv.ImageIndex = imageIndex;
					this.listSearchEngines.Items.Add(lv);
					this.listSearchEngines.Items[this.listSearchEngines.Items.Count - 1].Selected = true;
				} else {
					for (int i = 0; i < this.listSearchEngines.Items.Count; i++) {
						lv = this.listSearchEngines.Items[i];
						if (engine == (SearchEngine)lv.Tag) {
							break;
						}
					}
					if (lv != null) {
						lv.SubItems[1].Text = t; lv.SubItems[2].Text = d;
						lv.ImageIndex = imageIndex;
					}
				}

				if (modified)
					this.searchEnginesModified = true;
			}
		}

		/// <summary>
		/// called on every control
		/// </summary>
		/// <param name="sender">Which control is validated?</param>
		/// <param name="e">EventArgs with cancel parameter</param>
		private void OnControlValidating(object sender, System.ComponentModel.CancelEventArgs e) {

			this.btnApply.Enabled = false;

			if (sender == textProxyAddress && checkUseProxy.Checked) {

				textProxyAddress.Text = textProxyAddress.Text.Trim();
				if (textProxyAddress.Text.Length == 0) {
					errorProvider1.SetError(textProxyAddress, Resource.Manager["RES_ExceptionNoProxyUrl"]);
					e.Cancel = true;
				} else {
					if (textProxyAddress.Text.IndexOf("://") >= 0) {
						textProxyAddress.Text = textProxyAddress.Text.Substring(textProxyAddress.Text.IndexOf("://") + 3);
					}
				}

			} else if (sender == textProxyPort && checkUseProxy.Checked) {

				textProxyPort.Text = textProxyPort.Text.Trim();
				if (textProxyPort.Text.Length == 0)
					textProxyPort.Text = "8080";
				else {
					try {
						if (UInt16.Parse(textProxyPort.Text) < 0){
							errorProvider1.SetError(textProxyPort, Resource.Manager["RES_ExceptionProxyPortRange"]);
							e.Cancel = true;
						}
					}
					catch(FormatException) {
						errorProvider1.SetError(textProxyPort, Resource.Manager["RES_FormatExceptionProxyPort"]);
						e.Cancel = true;
					}
					catch(OverflowException) {
						errorProvider1.SetError(textProxyPort, Resource.Manager["RES_ExceptionProxyPortRange"]);
						e.Cancel = true;
					}
					catch (Exception){
						errorProvider1.SetError(textProxyPort, Resource.Manager["RES_ExceptionProxyPortInvalid"]);
						e.Cancel = true;
					}
				}

			} else if (sender == textProxyCredentialUser && checkProxyAuth.Checked) {
				
				textProxyCredentialUser.Text = textProxyCredentialUser.Text.Trim();
				if (textProxyCredentialUser.Text.Length == 0) {
					errorProvider1.SetError(textProxyCredentialUser, Resource.Manager["RES_ExceptionNoProxyAuthUser"]);
					e.Cancel = true;
				}
				
			} else if (sender == comboRefreshRate) {
				if (comboRefreshRate.Text.Length == 0)
					comboRefreshRate.Text = "60";	
				try {
					if ( System.Int32.Parse(comboRefreshRate.Text) * 60 * 1000 < 0){
						errorProvider1.SetError(comboRefreshRate, Resource.Manager["RES_OverflowExceptionRefreshRate"]);
						e.Cancel = true;
					}
				} 
				catch(FormatException) {
					errorProvider1.SetError(comboRefreshRate, Resource.Manager["RES_FormatExceptionRefreshRate"]);
					e.Cancel = true;
				}
				catch(OverflowException) {
					errorProvider1.SetError(comboRefreshRate, Resource.Manager["RES_OverflowExceptionRefreshRate"]);
					e.Cancel = true;
				}
				catch (Exception){
					errorProvider1.SetError(comboRefreshRate, Resource.Manager["RES_ExceptionRefreshRateInvalid"]);
					e.Cancel = true;
				}

			} else if (sender == txtBrowserStartExecutable && optNewWindowCustomExec.Checked) {

				txtBrowserStartExecutable.Text = txtBrowserStartExecutable.Text.Trim();
				if (txtBrowserStartExecutable.Text.Length == 0) {
					//errorProvider1.SetError(txtBrowserStartExecutable, Resource.Manager["RES_ExceptionNoOnNewWindowExecutable"]);
					e.Cancel = true;
				}
			
			} else if (sender == textRemoteStorageLocation && checkUseRemoteStorage.Checked) {
				
				textRemoteStorageLocation.Text = textRemoteStorageLocation.Text.Trim();
				if (textRemoteStorageLocation.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageLocation, Resource.Manager["RES_ExceptionNoRemoteStorageLocation"]);
					e.Cancel = true;
				} else {
					if (comboRemoteStorageProtocol.SelectedIndex != 0) {	// Check url
						try {
							Uri testUri = new Uri(textRemoteStorageLocation.Text);
							if (comboRemoteStorageProtocol.SelectedIndex == 1) {	// ftp. Check url scheme
								if (testUri.Scheme != "ftp") {
									throw new UriFormatException(Resource.Manager["RES_ExceptionInvalidFtpUrlSchemeForRemoteStorageLocation"]);
								}
							}
						} catch (UriFormatException ufex) {
							errorProvider1.SetError(textRemoteStorageLocation, ufex.Message);	// howto: translate???
							e.Cancel = true;
						}
					} else {	// check path
						if (!Directory.Exists(Environment.ExpandEnvironmentVariables(textRemoteStorageLocation.Text))) {
							errorProvider1.SetError(textRemoteStorageLocation, Resource.Manager["RES_DirectoryDoesNotExistMessage"]);
							e.Cancel = true;						
						}
					}
				}

			} else if (sender == textRemoteStorageUserName && checkUseRemoteStorage.Checked && 
						comboRemoteStorageProtocol.SelectedIndex != 0) {
				
				textRemoteStorageUserName.Text = textRemoteStorageUserName.Text.Trim();
				if (textRemoteStorageUserName.Text.Length == 0 && comboRemoteStorageProtocol.SelectedIndex != 4 /* WebDAV: auth. is optional */)  {
					errorProvider1.SetError(textRemoteStorageUserName, Resource.Manager["RES_ExceptionNoRemoteStorageAuthUser"]);
					e.Cancel = true;
				}
			
			} else if (sender == comboRemoteStorageProtocol && checkUseRemoteStorage.Checked) {
				
				textRemoteStorageLocation.Text = textRemoteStorageLocation.Text.Trim();
				if (textRemoteStorageLocation.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageLocation, Resource.Manager["RES_ExceptionNoRemoteStorageLocation"]);
				}

				if (comboRemoteStorageProtocol.SelectedIndex != 0) {
					textRemoteStorageUserName.Text = textRemoteStorageUserName.Text.Trim();
					if (textRemoteStorageUserName.Text.Length == 0 && comboRemoteStorageProtocol.SelectedIndex != 4 /* WebDAV: auth. is optional */)  {
						errorProvider1.SetError(textRemoteStorageUserName, Resource.Manager["RES_ExceptionNoRemoteStorageAuthUser"]);
					}
				}
			
			}
			
			if (!e.Cancel)
				errorProvider1.SetError((Control)sender, null);

		}

		private void OnControlValidated(object sender, System.EventArgs e) {
			this.btnApply.Enabled = true;
		}

		private void btnApply_Click(object sender, System.EventArgs e) {
			this.OnOKClick(this, e);	// may reset some invalid settings
			if (OnApplyPreferences != null)
				OnApplyPreferences(this, new EventArgs());
			this.btnApply.Enabled = false;
		}

		private void optOnOpenNewWindowChecked(object sender, System.EventArgs e) {
			if (e != null)	 {	// not caused by calling from another code location, enable Apply button
				errorProvider1.SetError(txtBrowserStartExecutable, null);
				this.OnControlValidated(this, null);
			}
			checkReuseFirstBrowserTab.Enabled = (sender == optNewWindowOnTab);
			OnControlValidated(this, EventArgs.Empty);
		}

		private void OnSearchEngineItemChecked(object sender, System.Windows.Forms.ItemCheckEventArgs e) {
			ListViewItem lv = this.listSearchEngines.Items[e.Index];
			if (lv!= null) {
				SearchEngine engine = (SearchEngine)lv.Tag;
				engine.IsActive = (e.NewValue == CheckState.Checked);
				this.searchEnginesModified = true;
			}
		}

		private void btnMakeDefaultAggregator_Click(object sender, System.EventArgs e) {
			try {
				RssBanditApplication.MakeDefaultAggregator();
				btnMakeDefaultAggregator.Enabled = false;	// disable on success
				// on success, ask the next startup time, if we are not anymore the default handler:
				RssBanditApplication.ShouldAskForDefaultAggregator = true;
			} catch (System.Security.SecurityException) {
				MessageBox.Show(this, Resource.Manager.FormatMessage("RES_SecurityExceptionCausedByRegistryAccess", "HKEY_CLASSES_ROOT\feed"),
					Resource.Manager["RES_GUIErrorMessageBoxCaption"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
			} catch (Exception ex) {
				MessageBox.Show(this, Resource.Manager.FormatMessage("RES_ExceptionSettingDefaultAggregator", ex.Message),
					Resource.Manager["RES_GUIErrorMessageBoxCaption"], MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			RssBanditApplication.CheckAndRegisterIEMenuExtensions();
		}
	
		private void btnSelectExecutable_Click(object sender, System.EventArgs e) {
			if (DialogResult.OK == openExeFileDialog.ShowDialog(this)) {
				txtBrowserStartExecutable.Text = openExeFileDialog.FileName;
			}
		}

		private void btnManageIdentities_Click(object sender, System.EventArgs e) {
			this.identityManager.ShowIdentityDialog(this);
			this.PopulateComboUserIdentityForComments(this.identityManager.CurrentIdentities, this.cboUserIdentityForComments.Text);
		}

		private void btnChangeColor_Click(object sender, System.EventArgs e) {
			//TODO: open color picker
			colorDialog1.Color = this.ActiveItemStateColor;
			colorDialog1.AllowFullOpen = true;
			if (DialogResult.Cancel != colorDialog1.ShowDialog(this)) {
				this.ActiveItemStateColor = colorDialog1.Color;
				this.RefreshFontSample();
				OnControlValidated(this, EventArgs.Empty);
			}
		}

		private void OnFontStyleChanged(object sender, System.EventArgs e) {
			this.ActiveItemStateFont = new Font(this.DefaultStateFont, this.FontStyleFromCheckboxes());
			this.RefreshFontSample();
			OnControlValidated(this, EventArgs.Empty);
		}

		private void OnItemStatesSelectedIndexChanged(object sender, System.EventArgs e) {
			this.FontStyleToCheckboxes(this.ActiveItemStateFont.Style);
			this.RefreshFontSample();
		}

		private void OnAnyCheckedChanged(object sender, System.EventArgs e) {
			OnControlValidated(this, EventArgs.Empty);
		}

		private void OnAnyComboSelectionChangeCommitted(object sender, System.EventArgs e) {
			OnControlValidated(this, EventArgs.Empty);
		}

		private void OnTabPrefs_Resize(object sender, EventArgs e)
		{	// fix the wide screen Tab Control issue by resize ourselfs the panels at the first Tab:
			sectionPanelGeneralBehavior.SetBounds(0,0, tabPrefs.Width - 2*sectionPanelGeneralBehavior.Location.X, 0, BoundsSpecified.Width);
			sectionPanelGeneralStartup.SetBounds(0,0, tabPrefs.Width - 2*sectionPanelGeneralStartup.Location.X, 0, BoundsSpecified.Width);
		}

		private void OnPreferencesDialog_Load(object sender, EventArgs e)
		{	// fix the wide screen Tab Control issue by resize ourselfs the panels at the first Tab:
			OnTabPrefs_Resize(this, EventArgs.Empty);
		}
	}
}
