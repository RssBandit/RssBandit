#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using RssBandit.AppServices;
using RssBandit.WinGui.Utility;
using RssBandit.WebSearch;
using RssBandit.Resources;

using Logger = RssBandit.Common.Logging;
using UserIdentity = RssBandit.Core.Storage.Serialization.UserIdentity;

namespace RssBandit.WinGui.Dialogs {

	/// <summary>
	/// PreferencesDialog provides the editing interface
	/// for all major application options.
	/// </summary>
	internal partial class PreferencesDialog : Form {

		public event EventHandler OnApplyPreferences;
	
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(PreferencesDialog));
		
		//private const string NgosDefaultLocation = "NewsGator Web Edition"; 

		private readonly Hashtable imageIndexMap = new Hashtable();
		internal List<SearchEngine> searchEngines;
		internal bool searchEnginesModified;
		private readonly IdentityNewsServerManager identityManager;
		internal Font[] itemStateFonts;
		internal Color[] itemStateColors;

		private PreferencesDialog() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			ApplyComponentTranslations();

			itemStateFonts = new Font[lstItemStates.Items.Count];
			itemStateColors = new Color[lstItemStates.Items.Count];

			// fix: if label has FlatStyle set to "System" (to get it rendered using the OS ClearType settings), 
			// it's text is rendered at top. So we "move" it a little bit downwards:
			this.lblFontSampleABC.Text = Environment.NewLine + this.lblFontSampleABC.Text;
			this.Load += this.OnPreferencesDialog_Load;
			this.linkCommentAPI.LinkClicked += OnAnyLinkLabel_LinkClicked;
			this.linkLabel1.LinkClicked += OnAnyLinkLabel_LinkClicked;
		}

		internal PreferencesDialog( 
			int refreshRate, 
			RssBanditPreferences prefs, 
			SearchEngineHandler seHandler, 
			IdentityNewsServerManager identityManager): 
			this() 
		{
		
			this.identityManager = identityManager;
			this.PopulateComboUserIdentityForComments(identityManager.Identities, prefs.UserIdentityForComments);
			
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
			btnConfigureAppSounds.Enabled = !RssBanditApplication.PortableApplicationMode;
			checkAllowAppEventSounds.Checked = prefs.AllowAppEventSounds;
			checkRunAtStartup.Checked = prefs.RunBanditAsWindowsUserLogon;
			checkRefreshFeedsOnStartup.Checked = prefs.FeedRefreshOnStartup;

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

			this.comboRefreshRate.Items.Clear();
			if (!Utils.RefreshRateStrings.Contains(refreshRate.ToString()))
				Utils.RefreshRateStrings.Add(refreshRate.ToString());
			this.comboRefreshRate.DataSource = Utils.RefreshRateStrings;
			this.comboRefreshRate.Text = refreshRate.ToString(); 
			this.comboRefreshRate.Refresh(); 
			
			// item formatters
			string tmplFolder = RssBanditApplication.GetTemplatesPath();
			this.checkCustomFormatter.Enabled = false;
			this.checkLimitNewsItemsPerPage.Checked = prefs.LimitNewsItemsPerPage; 
			this.checkMarkItemsAsReadWhenViewed.Checked = prefs.MarkItemsAsReadWhenViewed;
			this.numNewsItemsPerPage.Value = Decimal.Floor(prefs.NumNewsItemsPerPage);
			this.comboFormatters.Items.Clear();
				
			if (Directory.Exists(tmplFolder)) {
				ICoreApplication coreApp = IoC.Resolve<ICoreApplication>();
				IList tmplFiles = coreApp.GetItemFormatterStylesheets();
				
				if (tmplFiles.Count > 0) {	
					this.checkCustomFormatter.Enabled = true;
					foreach (string formatter in tmplFiles) {
						this.comboFormatters.Items.Add(formatter); 
					}
				}
			
				if (!string.IsNullOrEmpty(prefs.NewsItemStylesheetFile) &&
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
			this.checkOpenTabsInBackground.Checked = prefs.OpenNewTabsInBackground;
			this.checkMarkItemsReadOnExit.Checked = prefs.MarkItemsReadOnExit; 
			this.checkUseFavicons.Checked         = prefs.UseFavicons; 

			this.MaxItemAge = prefs.MaxItemAge;

			// moved to this location, because of the validation init involved herein
			this.checkUseProxy.Checked = prefs.UseProxy; 
			this.checkUseIEProxySettings.Checked = prefs.UseIEProxySettings;
			this.checkProxyAuth.Checked = prefs.ProxyCustomCredentials;

			// Checked_Change event not fired if no change happened before:
			if (!this.checkUseProxy.Checked && 
				!this.checkUseIEProxySettings.Checked &&
				!this.checkProxyAuth.Checked)
				this.checkUseProxy_CheckedChanged(this, null);
			
			// font tab
			this.SetFontForState(FontStates.Read, prefs.NormalFont);	// default font
			this.SetFontForState(FontStates.Unread, prefs.UnreadFont);	
			this.SetFontForState(FontStates.Flag, prefs.FlagFont);	
			this.SetFontForState(FontStates.Referrer, prefs.RefererFont);
			this.SetFontForState(FontStates.Error, prefs.ErrorFont);	
			this.SetFontForState(FontStates.NewComments, prefs.NewCommentsFont);	
			this.SetColorForState(FontStates.Read, prefs.NormalFontColor);
			this.SetColorForState(FontStates.Unread, prefs.UnreadFontColor);
			this.SetColorForState(FontStates.Flag, prefs.FlagFontColor);
			this.SetColorForState(FontStates.Referrer, prefs.RefererFontColor);
			this.SetColorForState(FontStates.Error, prefs.ErrorFontColor);
			this.SetColorForState(FontStates.NewComments, prefs.NewCommentsFontColor);

			this.RefreshFontFamilySizeSample();
			this.lstItemStates.SelectedIndex = 1;	// raise event to refresh checkboxes and Sample

			// Remote storage tab
			this.textRemoteStorageUserName.Text = prefs.RemoteStorageUserName;
			this.textRemoteStoragePassword.Text = prefs.RemoteStoragePassword;
			this.textRemoteStorageLocation.Text = prefs.RemoteStorageLocation;
			this.checkUseRemoteStorage.Checked = prefs.UseRemoteStorage;

			int oldIndex = comboRemoteStorageProtocol.SelectedIndex;

			this.RemoteStorageProtocol = prefs.RemoteStorageProtocol;
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
				this.searchEngines = new List<SearchEngine>(seHandler.Engines);
			} else {
                this.searchEngines = new List<SearchEngine>();
			}

			InitWebSearchEnginesTab();

			btnMakeDefaultAggregator.Enabled = (!RssBanditApplication.IsDefaultAggregator());

			checkBrowserJavascriptAllowed.Checked = prefs.BrowserJavascriptAllowed;
			checkBrowserJavaAllowed.Checked = prefs.BrowserJavaAllowed;
			checkBrowserActiveXAllowed.Checked = prefs.BrowserActiveXAllowed;
			checkBrowserBGSoundAllowed.Checked = prefs.BrowserBGSoundAllowed;
			checkBrowserVideoAllowed.Checked = prefs.BrowserVideoAllowed;
			checkBrowserImagesAllowed.Checked = prefs.BrowserImagesAllowed;
			
			//set enclosure related settings 
			ICoreApplication rssBanditApp = IoC.Resolve<ICoreApplication>();
			textEnclosureDirectory.Text = rssBanditApp.EnclosureFolder; 
			checkDownloadCreateFolderPerFeed.Checked = rssBanditApp.DownloadCreateFolderPerFeed;			
			checkEnableEnclosureAlerts.Checked = rssBanditApp.EnableEnclosureAlerts;
			checkDownloadEnclosures.Checked = rssBanditApp.DownloadEnclosures;

			if(!checkDownloadEnclosures.Checked){
				this.checkOnlyDownloadLastXAttachments.Checked = false;
				this.checkOnlyDownloadLastXAttachments.Enabled = false; 
			}else if(rssBanditApp.NumEnclosuresToDownloadOnNewFeed != Int32.MaxValue){
				this.checkOnlyDownloadLastXAttachments.Checked = true; 
				this.numOnlyDownloadLastXAttachments.Value   = Convert.ToDecimal(rssBanditApp.NumEnclosuresToDownloadOnNewFeed);
			}
			
			if(rssBanditApp.EnclosureCacheSize != Int32.MaxValue){
				this.checkEnclosureSizeOnDiskLimited.Checked = true; 
				this.numEnclosureCacheSize.Value = Convert.ToDecimal(rssBanditApp.EnclosureCacheSize);
			}
				
			checkOnlyDownloadLastXAttachments_CheckedChanged(this, EventArgs.Empty);
			checkEnclosureSizeOnDiskLimited_CheckedChanged(this, EventArgs.Empty);

			this.SetElevatedOptionIndicators();
			
			this.btnApply.Enabled = false;
		}

		void ApplyComponentTranslations()
		{

			this.comboMaxItemAge.Items.Clear();
			this.comboMaxItemAge.DataSource = Utils.MaxItemAgeStrings;
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

		private void SetElevatedOptionIndicators() {
			try {
				if (UACManager.Denied(ElevationRequiredAction.RunBanditAsWindowsUserLogon)) {
					securityHintProvider.SetIconAlignment(checkRunAtStartup, ErrorIconAlignment.MiddleLeft);
					securityHintProvider.SetIconPadding(checkRunAtStartup, 5);
					securityHintProvider.SetError(checkRunAtStartup, DR.DialogBase_UACShieldedHint);
					checkRunAtStartup.Enabled = false;
				} else {
					securityHintProvider.SetError(checkRunAtStartup, null);
					checkRunAtStartup.Enabled = true;
				}
			
				if (btnMakeDefaultAggregator.Enabled && 
				    UACManager.Denied(ElevationRequiredAction.MakeDefaultAggregator)) {
					securityHintProvider.SetIconAlignment(btnMakeDefaultAggregator, ErrorIconAlignment.MiddleLeft);
					securityHintProvider.SetIconPadding(btnMakeDefaultAggregator, 5);
					securityHintProvider.SetError(btnMakeDefaultAggregator, DR.DialogBase_UACShieldedHint);
					btnMakeDefaultAggregator.Enabled = false;
				} else {
					securityHintProvider.SetError(btnMakeDefaultAggregator, null);
				}
			} catch (Exception ex) {
				_log.Error("Failure while query UACManager for denied actions.", ex);
			}
		}

		/// <summary>
		/// Gets or sets the selected section.
		/// </summary>
		/// <value>The selected section.</value>
		public OptionDialogSection SelectedSection {
			get { return (OptionDialogSection)this.tabPrefs.SelectedIndex; }
			set {
				int i = (int)value;
				if (i >= 0 && i < this.tabPrefs.TabCount)
					this.tabPrefs.SelectedIndex = i;
			}
		}

		/// <summary>
		/// Gets or sets the remote storage protocol.
		/// </summary>
		/// <value>The remote storage protocol.</value>
		public RemoteStorageProtocolType RemoteStorageProtocol {
			get {
				switch (comboRemoteStorageProtocol.SelectedIndex) {
					case 0: return RemoteStorageProtocolType.UNC;
					case 1: return RemoteStorageProtocolType.FTP;
					case 2: return RemoteStorageProtocolType.dasBlog;
					case 3: return RemoteStorageProtocolType.WebDAV;
					case -1: return RemoteStorageProtocolType.Unknown; // no item selected
					default: throw new InvalidOperationException("No RemoteStorageProtocolType for selected index: " + comboRemoteStorageProtocol.SelectedIndex);
				}
			}
			set {
				switch (value) {
					case RemoteStorageProtocolType.UNC:
						comboRemoteStorageProtocol.SelectedIndex = 0;
						break;
					case RemoteStorageProtocolType.FTP:
						comboRemoteStorageProtocol.SelectedIndex = 1;
						break;
					case RemoteStorageProtocolType.dasBlog:
						comboRemoteStorageProtocol.SelectedIndex = 2;
						break;
                    /* 
					case RemoteStorageProtocolType.NewsgatorOnline:
						comboRemoteStorageProtocol.SelectedIndex = 3; 
						textRemoteStorageLocation.Enabled = false;
						break;
							case RemoteStorageProtocolType.dasBlog_1_3:
						comboRemoteStorageProtocol.SelectedIndex = 3;
						break; */
					case RemoteStorageProtocolType.WebDAV:
						comboRemoteStorageProtocol.SelectedIndex = 3;
						break;
					default:
						comboRemoteStorageProtocol.SelectedIndex = -1;
						break;
				}			
			}
		}

		/// <summary>
		/// Gets or sets the max item age.
		/// </summary>
		/// <value>The max item age.</value>
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
			chkFontBold.CheckState =  ((s & FontStyle.Bold) == FontStyle.Bold) ? CheckState.Checked: CheckState.Unchecked;
			chkFontItalic.CheckState =  ((s & FontStyle.Italic) == FontStyle.Italic) ? CheckState.Checked: CheckState.Unchecked;
			chkFontStrikeout.CheckState =  ((s & FontStyle.Strikeout) == FontStyle.Strikeout) ? CheckState.Checked: CheckState.Unchecked;
			chkFontUnderline.CheckState =  ((s & FontStyle.Underline) == FontStyle.Underline) ? CheckState.Checked: CheckState.Unchecked;
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

		private void PopulateComboUserIdentityForComments(IDictionary<string, UserIdentity> identities, string defaultIdentity) {
			this.cboUserIdentityForComments.Items.Clear();
			foreach (UserIdentity ui in identities.Values) {
				this.cboUserIdentityForComments.Items.Add(ui.Name);
			}
			if (defaultIdentity != null && identities.ContainsKey(defaultIdentity)) {
				this.cboUserIdentityForComments.Text = defaultIdentity;
			} else {
				if (this.cboUserIdentityForComments.Items.Count > 0)
					this.cboUserIdentityForComments.SelectedIndex = 0;
			}
		}

		private void checkUseProxy_CheckedChanged(object sender, EventArgs e) {
		
			bool useProxy = checkUseProxy.Checked;
			bool useProxyAuth = checkUseProxy.Checked | checkUseIEProxySettings.Checked;

			labelProxyAddress.Enabled = textProxyAddress.Enabled = useProxy; 
			labelProxyPort.Enabled = textProxyPort.Enabled = useProxy;
			checkProxyBypassLocal.Enabled = useProxy;
			checkProxyAuth.Enabled = useProxyAuth;
			
			if(useProxy)
			{
				if (textProxyAddress.Text.Trim().Length == 0)
					errorProvider1.SetError(textProxyAddress, SR.ExceptionNoProxyUrl);
				if (textProxyPort.Text.Trim().Length == 0)
					textProxyPort.Text = "8080";
				if (checkProxyBypassLocal.Checked) {
					labelProxyBypassList.Enabled = labelProxyBypassListHint.Enabled = textProxyBypassList.Enabled = true;
				}
			}
			else 
			{
				labelProxyBypassList.Enabled = labelProxyBypassListHint.Enabled = textProxyBypassList.Enabled = false;
				errorProvider1.SetError(textProxyAddress, null);
				errorProvider1.SetError(textProxyPort, null);
			}

			if (useProxyAuth)
			{
				if (checkProxyAuth.Checked)
				{
					labelProxyCredentialUserName.Enabled = textProxyCredentialUser.Enabled = true;
					labelProxyCredentialPassword.Enabled = textProxyCredentialPassword.Enabled = true;
				}
			} 
			else
			{
				labelProxyCredentialUserName.Enabled = textProxyCredentialUser.Enabled = false;
				labelProxyCredentialPassword.Enabled = textProxyCredentialPassword.Enabled = false;
			}

			if (e != null)		// not caused by calling from another code location, enable Apply button
				this.OnControlValidated(this, null);
			
		}

		private void checkProxyBypassLocal_CheckedChanged(object sender, EventArgs e) {
			labelProxyBypassList.Enabled = labelProxyBypassListHint.Enabled = textProxyBypassList.Enabled = checkProxyBypassLocal.Checked;
		}
		
		private void checkProxyAuth_CheckedChanged(object sender, EventArgs e) {
			labelProxyCredentialUserName.Enabled = textProxyCredentialUser.Enabled = checkProxyAuth.Checked; 
			labelProxyCredentialPassword.Enabled = textProxyCredentialPassword.Enabled = checkProxyAuth.Checked;
		
			if (e != null)		// not caused by calling from another code location
				this.OnControlValidated(this, EventArgs.Empty);
		}

		private void checkCustomFormatter_CheckedChanged(object sender, EventArgs e) {
			if (checkCustomFormatter.Checked) {
				labelFormatters.Enabled = comboFormatters.Enabled = true; 
				checkMarkItemsAsReadWhenViewed.Enabled = checkLimitNewsItemsPerPage.Enabled = numNewsItemsPerPage.Enabled = false; 
				checkMarkItemsAsReadWhenViewed.Checked = checkLimitNewsItemsPerPage.Checked = false;
			}
			else {
				labelFormatters.Enabled = comboFormatters.Enabled = false;
				checkMarkItemsAsReadWhenViewed.Enabled = checkLimitNewsItemsPerPage.Enabled = numNewsItemsPerPage.Enabled = true; 				
				comboFormatters.Text = String.Empty;
				comboFormatters.Refresh();
			}
			this.OnControlValidated(this, EventArgs.Empty);
		}

		private void OnOKClick(object sender, EventArgs e) 
		{
			
			if (checkUseProxy.Checked) {
				if (textProxyAddress.Text.Trim().Length == 0 ||
					textProxyPort.Text.Trim().Length == 0) {
					checkUseProxy.Checked = false;
					checkNoProxy.Checked = true;
				}
			}


			if (checkUseRemoteStorage.Checked) {
				if (this.RemoteStorageProtocol != RemoteStorageProtocolType.UNC && 
					this.RemoteStorageProtocol != RemoteStorageProtocolType.WebDAV /* WebDAV: auth. is optional */ && 
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

		private void OnDefaultFontChangeClick(object sender, EventArgs e) {
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
		private void checkUseRemoteStorage_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = checkUseRemoteStorage.Checked;

			textRemoteStorageUserName.Enabled = enabled;
			textRemoteStoragePassword.Enabled = enabled;
			textRemoteStorageLocation.Enabled = enabled; 
			comboRemoteStorageProtocol.Enabled = enabled;

			/* location text box is disabled if NewsgatorOnline is remote storage method */
			if(enabled && this.RemoteStorageProtocol == RemoteStorageProtocolType.NewsgatorOnline){
				textRemoteStorageLocation.Enabled = false; 
			} 
	
			if (enabled) {
				if (textRemoteStorageLocation.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageLocation, SR.ExceptionNoRemoteStorageLocation);
				} else if 	(this.RemoteStorageProtocol != RemoteStorageProtocolType.UNC && 
					this.RemoteStorageProtocol != RemoteStorageProtocolType.WebDAV /* WebDAV: auth. is optional */ && 
					textRemoteStorageUserName.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageUserName, SR.ExceptionNoRemoteStorageAuthUser);
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

		private void comboRemoteStorageProtocol_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool showAuth = false;
			textRemoteStorageLocation.Enabled = true;

			switch (comboRemoteStorageProtocol.SelectedIndex)	{

				case 0: // "File Share" 	(use index, no strings to enable localization)
					labelRemoteStorageLocation.Text = SR.LabelTextRemoteStorageLocation_fileShare;
					labelExperimental.Text = SR.LabelTextRemoteStorageLocation_fileShare_hint;				
					showAuth = false;
					break;
				case 1: // "FTP"
					labelRemoteStorageLocation.Text = SR.LabelTextRemoteStorageLocation_ftp;
					labelExperimental.Text = SR.LabelTextRemoteStorageLocation_ftp_hint;				
					showAuth = true;
					break;
				case 2: //"dasBlog"
					labelRemoteStorageLocation.Text = SR.LabelTextRemoteStorageLocation_dasBlog;
					labelExperimental.Text = SR.LabelTextRemoteStorageLocation_dasBlog_hint;
					showAuth = true;
					break;
			/*	case 3: //"dasBlog 1.3"
					labelRemoteStorageLocation.Text = SR.LabelTextRemoteStorageLocation_dasBlog;
					labelExperimental.Text = SR.LabelTextRemoteStorageLocation_dasBlog_hint;
					showAuth = true;
					break; */
				case 3: //"WebDAV"
					labelRemoteStorageLocation.Text = SR.LabelTextRemoteStorageLocation_WebDAV;
					labelExperimental.Text = SR.LabelTextRemoteStorageLocation_WebDAV_hint;
					showAuth = true;
					break;
			/*	case 3: //"NewsgatorOnline"
					labelRemoteStorageLocation.Text = SR.LabelTextRemoteStorageLocation_NewsgatorOnline;
					labelExperimental.Text = SR.LabelTextRemoteStorageLocation_NewsgatorOnline_hint;
					showAuth = true;
					textRemoteStorageLocation.Text = NgosDefaultLocation;
					textRemoteStorageLocation.Enabled = false;
				    break;
           */ 
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

		private void optNewWindowCustomExec_CheckedChanged(object sender, EventArgs e) {
			labelBrowserStartExecutable.Enabled = txtBrowserStartExecutable.Enabled = btnSelectExecutable.Enabled = optNewWindowCustomExec.Checked;
			if (optNewWindowCustomExec.Checked && txtBrowserStartExecutable.Text.Trim().Length == 0) {
				//errorProvider1.SetError(txtBrowserStartExecutable, SR.ExceptionNoOnNewWindowExecutable"]);
			}
			if (optNewWindowCustomExec.Checked){
				if(checkReuseFirstBrowserTab.Enabled)checkReuseFirstBrowserTab.Enabled = false;
				if(checkOpenTabsInBackground.Enabled)checkOpenTabsInBackground.Enabled = false; 
			}
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
							try {
								Image img = Image.FromFile(p);
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

		private void OnSearchEngineItemActivate(object sender, EventArgs e) {
			bool on = (this.listSearchEngines.SelectedItems.Count > 0);
			this.btnSEMoveUp.Enabled = (on && this.listSearchEngines.SelectedItems[0].Index > 0);
			this.btnSEMoveDown.Enabled =  (on && this.listSearchEngines.SelectedItems[0].Index < (this.listSearchEngines.Items.Count - 1));
			this.btnSEProperties.Enabled = this.btnSERemove.Enabled = on;
		}

		private void OnSearchEnginesListMouseUp(object sender, MouseEventArgs e) {
			this.OnSearchEngineItemActivate(sender, null);
		}

		private void btnSEProperties_Click(object sender, EventArgs e) {
			SearchEngine engine = (SearchEngine)this.listSearchEngines.SelectedItems[0].Tag;
			ShowAndHandleEngineProperties(engine);
		}

		private void btnSEAdd_Click(object sender, EventArgs e) {
			ShowAndHandleEngineProperties(new SearchEngine());
		}

		private void btnSERemove_Click(object sender, EventArgs e) {
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

		private void btnSEMoveUp_Click(object sender, EventArgs e) {
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

		private void btnSEMoveDown_Click(object sender, EventArgs e) {
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

					string p;
					if (imn.IndexOf(Path.DirectorySeparatorChar) > 0) 
						p = imn;
					else
						p = Path.Combine(RssBanditApplication.GetSearchesPath(), imn);
					
					if (File.Exists(p)) {
						try {
							Image img = Image.FromFile(p);
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
						if (engine == lv.Tag) {
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
					errorProvider1.SetError(textProxyAddress, SR.ExceptionNoProxyUrl);
					e.Cancel = true;
				} else {
					if (textProxyAddress.Text.IndexOf("://") >= 0) {
						textProxyAddress.Text = textProxyAddress.Text.Substring(textProxyAddress.Text.IndexOf("://") + 3);
					}
				}

			} else if(sender == textEnclosureDirectory){

				textEnclosureDirectory.Text = textEnclosureDirectory.Text.Trim();
				if ((textEnclosureDirectory.Text.Length == 0) || !Directory.Exists(textEnclosureDirectory.Text)) {
					errorProvider1.SetError(textEnclosureDirectory, SR.ExceptionInvalidEnclosurePath);
					e.Cancel = true;
				}
			
			}else if (sender == textProxyPort && checkUseProxy.Checked) {

				textProxyPort.Text = textProxyPort.Text.Trim();
				if (textProxyPort.Text.Length == 0)
					textProxyPort.Text = "8080";
				else {
					try {
						if (UInt16.Parse(textProxyPort.Text) < 0){
							errorProvider1.SetError(textProxyPort, SR.ExceptionProxyPortRange);
							e.Cancel = true;
						}
					}
					catch(FormatException) {
						errorProvider1.SetError(textProxyPort, SR.FormatExceptionProxyPort);
						e.Cancel = true;
					}
					catch(OverflowException) {
						errorProvider1.SetError(textProxyPort, SR.ExceptionProxyPortRange);
						e.Cancel = true;
					}
					catch (Exception){
						errorProvider1.SetError(textProxyPort, SR.ExceptionProxyPortInvalid);
						e.Cancel = true;
					}
				}

			} else if (sender == comboRefreshRate) {
				if (comboRefreshRate.Text.Length == 0)
					comboRefreshRate.Text = "60";	
				try {
					if ( Int32.Parse(comboRefreshRate.Text) * 60 * 1000 < 0){
						errorProvider1.SetError(comboRefreshRate, SR.OverflowExceptionRefreshRate);
						e.Cancel = true;
					}
				} 
				catch(FormatException) {
					errorProvider1.SetError(comboRefreshRate, SR.FormatExceptionRefreshRate);
					e.Cancel = true;
				}
				catch(OverflowException) {
					errorProvider1.SetError(comboRefreshRate, SR.OverflowExceptionRefreshRate);
					e.Cancel = true;
				}
				catch (Exception){
					errorProvider1.SetError(comboRefreshRate, SR.ExceptionRefreshRateInvalid);
					e.Cancel = true;
				}

			} else if (sender == txtBrowserStartExecutable && optNewWindowCustomExec.Checked) {

				txtBrowserStartExecutable.Text = txtBrowserStartExecutable.Text.Trim();
				if (txtBrowserStartExecutable.Text.Length == 0) {
					//errorProvider1.SetError(txtBrowserStartExecutable, SR.ExceptionNoOnNewWindowExecutable"]);
					e.Cancel = true;
				}
			
			} else if (sender == textRemoteStorageLocation && checkUseRemoteStorage.Checked) {
				
				RemoteStorageProtocolType protocol = this.RemoteStorageProtocol;

				textRemoteStorageLocation.Text = textRemoteStorageLocation.Text.Trim();
				if (textRemoteStorageLocation.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageLocation, SR.ExceptionNoRemoteStorageLocation);
					e.Cancel = true;
				} else {
					if (protocol != RemoteStorageProtocolType.UNC &&
						protocol != RemoteStorageProtocolType.NewsgatorOnline) {	// Check url
						try {
							Uri testUri = new Uri(textRemoteStorageLocation.Text);
							if (protocol == RemoteStorageProtocolType.FTP) {	// Check url scheme
								if (testUri.Scheme != "ftp") {
									throw new UriFormatException(SR.ExceptionInvalidFtpUrlSchemeForRemoteStorageLocation);
								}
							}
						} catch (UriFormatException ufex) {
							errorProvider1.SetError(textRemoteStorageLocation, ufex.Message);	// howto: translate???
							e.Cancel = true;
						}
					} else if (protocol == RemoteStorageProtocolType.UNC) {	// check path
						if (!Directory.Exists(Environment.ExpandEnvironmentVariables(textRemoteStorageLocation.Text))) {
							errorProvider1.SetError(textRemoteStorageLocation, SR.DirectoryDoesNotExistMessage);
							e.Cancel = true;						
						}
					}
				}

			} else if (sender == textRemoteStorageUserName && checkUseRemoteStorage.Checked && 
				this.RemoteStorageProtocol != RemoteStorageProtocolType.UNC) {
				
				textRemoteStorageUserName.Text = textRemoteStorageUserName.Text.Trim();
				if (textRemoteStorageUserName.Text.Length == 0 && this.RemoteStorageProtocol != RemoteStorageProtocolType.WebDAV /* WebDAV: auth. is optional */)  {
					errorProvider1.SetError(textRemoteStorageUserName, SR.ExceptionNoRemoteStorageAuthUser);
					e.Cancel = true;
				}
			
			} else if (sender == comboRemoteStorageProtocol && checkUseRemoteStorage.Checked) {
				
				textRemoteStorageLocation.Text = textRemoteStorageLocation.Text.Trim();
				if (textRemoteStorageLocation.Text.Length == 0)  {
					errorProvider1.SetError(textRemoteStorageLocation, SR.ExceptionNoRemoteStorageLocation);
				}

				if (this.RemoteStorageProtocol != RemoteStorageProtocolType.UNC) {
					textRemoteStorageUserName.Text = textRemoteStorageUserName.Text.Trim();
					if (textRemoteStorageUserName.Text.Length == 0 && this.RemoteStorageProtocol != RemoteStorageProtocolType.WebDAV /* WebDAV: auth. is optional */)  {
						errorProvider1.SetError(textRemoteStorageUserName, SR.ExceptionNoRemoteStorageAuthUser);
					}
				}
			
			}
			
			if (!e.Cancel)
				errorProvider1.SetError((Control)sender, null);

		}

		private void OnControlValidated(object sender, EventArgs e) {
			this.btnApply.Enabled = true;
		}

		private void btnApply_Click(object sender, EventArgs e) {
			this.OnOKClick(this, e);	// may reset some invalid settings
			if (OnApplyPreferences != null)
				OnApplyPreferences(this, new EventArgs());
			this.btnApply.Enabled = false;
		}

		private void optOnOpenNewWindowChecked(object sender, EventArgs e) {
			if (e != null)	 {	// not caused by calling from another code location, enable Apply button
				errorProvider1.SetError(txtBrowserStartExecutable, null);
				this.OnControlValidated(this, null);
			}
			checkReuseFirstBrowserTab.Enabled = (sender == optNewWindowOnTab);
			checkOpenTabsInBackground.Enabled = (sender == optNewWindowOnTab);
			OnControlValidated(this, EventArgs.Empty);
		}

		private void OnSearchEngineItemChecked(object sender, ItemCheckEventArgs e) {
			ListViewItem lv = this.listSearchEngines.Items[e.Index];
			if (lv!= null) {
				SearchEngine engine = (SearchEngine)lv.Tag;
				engine.IsActive = (e.NewValue == CheckState.Checked);
				this.searchEnginesModified = true;
				OnControlValidated(this, EventArgs.Empty);
			}
		}

		private void btnMakeDefaultAggregator_Click(object sender, EventArgs e) {
			try {
				RssBanditApplication.MakeDefaultAggregator();
				btnMakeDefaultAggregator.Enabled = false;	// disable on success
				// on success, ask the next startup time, if we are not anymore the default handler:
				RssBanditApplication.ShouldAskForDefaultAggregator = true;
			} catch (System.Security.SecurityException) {
				MessageBox.Show(this, String.Format(SR.SecurityExceptionCausedByRegistryAccess,"HKEY_CLASSES_ROOT\feed"),
					SR.GUIErrorMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			} catch (Exception ex) {
				MessageBox.Show(this, String.Format(SR.ExceptionSettingDefaultAggregator,ex.Message),
					SR.GUIErrorMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			RssBanditApplication.CheckAndRegisterIEMenuExtensions();
		}
	
		private void btnSelectExecutable_Click(object sender, EventArgs e) {
			if (DialogResult.OK == openExeFileDialog.ShowDialog(this)) {
				txtBrowserStartExecutable.Text = openExeFileDialog.FileName;
			}
		}

		private void btnManageIdentities_Click(object sender, EventArgs e) {
			this.identityManager.ShowIdentityDialog(this);
			this.PopulateComboUserIdentityForComments(this.identityManager.Identities, this.cboUserIdentityForComments.Text);
		}

		private void btnChangeColor_Click(object sender, EventArgs e) {
			//TODO: open color picker
			colorDialog1.Color = this.ActiveItemStateColor;
			colorDialog1.AllowFullOpen = true;
			if (DialogResult.Cancel != colorDialog1.ShowDialog(this)) {
				this.ActiveItemStateColor = colorDialog1.Color;
				this.RefreshFontSample();
				OnControlValidated(this, EventArgs.Empty);
			}
		}

		private void OnFontStyleChanged(object sender, EventArgs e) {
			this.ActiveItemStateFont = new Font(this.DefaultStateFont, this.FontStyleFromCheckboxes());
			this.RefreshFontSample();
			OnControlValidated(this, EventArgs.Empty);
		}

		private void OnItemStatesSelectedIndexChanged(object sender, EventArgs e) {
			this.FontStyleToCheckboxes(this.ActiveItemStateFont.Style);
			this.RefreshFontSample();
		}

		private void OnAnyCheckedChanged(object sender, EventArgs e) {
			OnControlValidated(this, EventArgs.Empty); 
		}

		private void OnAnyComboSelectionChangeCommitted(object sender, EventArgs e) {
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

		private void OnAnyLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			LinkLabel o = sender as LinkLabel;
			if (o != null) {
				ICoreApplication coreApp = (ICoreApplication)this.GetService(typeof(ICoreApplication));
				if (coreApp != null) {
					coreApp.NavigateToUrlAsUserPreferred(o.Text, "", true, true);
					o.Links[o.Links.IndexOf(e.Link)].Visited = true;	
				}
			}
		}

		private void OnPodcastOptionsButtonClick(object sender, EventArgs e) {
			ICoreApplication coreApp = IoC.Resolve<ICoreApplication>();
			coreApp.ShowPodcastOptionsDialog(this, null);
		}

		private void btnSelectEnclosureFolder2_Click(object sender, EventArgs e) {

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.Description = SR.BrowseForFolderEnclosureDownloadLocation;
            
			DialogResult result = dlg.ShowDialog(); 	 
			 	  	 
			if(result == DialogResult.OK) { 	 
				this.textEnclosureDirectory.Text = dlg.SelectedPath; 	 
			}
		}

		private void checkOnlyDownloadLastXAttachments_CheckedChanged(object sender, EventArgs e) {
			lblDownloadXAttachmentsPostfix.Enabled = numOnlyDownloadLastXAttachments.Enabled = checkOnlyDownloadLastXAttachments.Checked;
			OnControlValidated(this, EventArgs.Empty);
		}

		private void checkEnclosureSizeOnDiskLimited_CheckedChanged(object sender, EventArgs e) {
			lblDownloadAttachmentsSmallerThanPostfix.Enabled = numEnclosureCacheSize.Enabled = checkEnclosureSizeOnDiskLimited.Checked;
			OnControlValidated(this, EventArgs.Empty);
		}

		private void checkDownloadEnclosures_CheckedChanged(object sender, EventArgs e) {
            lblDownloadXAttachmentsPostfix.Enabled = numOnlyDownloadLastXAttachments.Enabled = checkOnlyDownloadLastXAttachments.Checked = checkOnlyDownloadLastXAttachments.Enabled = this.checkDownloadEnclosures.Checked;
			OnControlValidated(this, EventArgs.Empty);
		}

		private void OnEnableAppSoundsCheckedChanged(object sender, EventArgs e) {
			this.OnAnyCheckedChanged(sender, e);
			this.btnConfigureAppSounds.Enabled = (this.checkAllowAppEventSounds.Checked &&
				!RssBanditApplication.PortableApplicationMode);
		}

		private void OnConfigureAppSoundsClick(object sender, EventArgs e) {
			try {
				Process.Start("rundll32.exe", "shell32.dll,Control_RunDLL mmsys.cpl,,1");
			} catch (Exception ex) {
				MessageBox.Show(this, RssBanditApplication.Caption,
				    String.Format(SR.WindowsSoundControlPanelDisplayFailed,ex.Message),
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void checkLimitNewsItemsPerPage_CheckedChanged(object sender, EventArgs e) {
			this.OnAnyCheckedChanged(sender, e);
			this.numNewsItemsPerPage.Enabled = checkLimitNewsItemsPerPage.Checked;
		}

		private void checkMarkItemsAsReadWhenViewed_CheckedChanged(object sender, EventArgs e) {
			this.OnAnyCheckedChanged(sender, e);
		}
		
	}
}
