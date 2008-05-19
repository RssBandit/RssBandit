using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Divelements.WizardFramework;
using NewsComponents.Net;
using RssBandit;
using RssBandit.AppServices;
using RssBandit.Common;
using RssBandit.Resources;
using RssBandit.WebSearch;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Utility;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;

namespace RssBandit.WinGui.Forms
{

	/// <summary>
	/// AddSubscriptionWizard summerize and handles 
	/// all kind of subscriptions now:
	///   By URL (direct, and autodiscovered)
	///   By Search/Topic
	///   NNTP Groups
	///   Direct NNTP Group
	/// </summary>
	internal class ImportFeedsWizard : System.Windows.Forms.Form, IServiceProvider, IWaitDialog
	{
		private enum WizardValidationTask {
			None,
			InProgress,
			Pending,
			Success,
			Failed,
		}

		/// <summary>
		/// The new subscription - feed, if not null it is ready to be subscribed.
		/// </summary>
		internal NewsFeed Feed = null;

		private AddSubscriptionWizardMode wizardMode; 
		private IServiceProvider serviceProvider;
		private WindowSerializer windowSerializer;
		private IInternetService internetService;
		private ICoreApplication coreApplication;

		private TimeSpan timeout = TimeSpan.Zero;	// no timeout
		private bool operationTimeout = false;
		private DialogResult operationResult;
		private int timeCounter;
		private AutoResetEvent waitHandle;
		private FeedInfo feedInfo;
		private bool credentialsStepReWired = false;

		#region Designer Form variables

        private System.Windows.Forms.Timer timerIncreaseProgress;
        private System.Windows.Forms.Timer timerStartValidation;
        private System.Windows.Forms.ToolTip toolTip;
        private WizardPage pageFeedCredentials;
        private Label lblFeedCredentialsIntro;
        private Label lblUsername;
        private TextBox textUser;
        private Label lblPassword;
        private TextBox textPassword;
        private WizardPage pageFeedItemControl;
        private ComboBox comboMaxItemAge;
        private ComboBox cboUpdateFrequency;
        private Label lblRemoveItemsOlderThan;
        private Label lblMinutes;
        private Label lblUpdateFrequency;
        private CheckBox checkMarkItemsReadOnExiting;
        private Label lblFeedItemControlIntro;
        private CheckBox checkEnableAlertOnNewItems;
        private WizardPage pageFeedItemDisplay;
        private ComboBox comboFormatters;
        private Label lblFormatterStylesheet;
        private CheckBox checkUseCustomFormatter;
        private Label lblFeedItemDisplayIntro;
        private FinishPage finishPage;
        private Panel pnlCompleting;
        private Label lblCompletionMessage;
        private Panel pnlCancelling;
        private WizardPage pageTitleCategory;
        private Label lblFeedCategory;
        private ComboBox cboFeedCategory;
        private Label lblPageTitleCredentialsIntro;
        private Label lblFeedTitle;
        private TextBox txtFeedTitle;
        private WizardPage pageFoundMultipleFeeds;
        private ListView listFeeds;
        private Label lblMultipleFeedsFoundHint1;
        private Label lblMultipleFeedsFoundHint2;
        private Label lblMultipleFeedsFound;
        private WizardPage pageNewBySearchTopic;
        private PictureBox pictureHelpSyndic8;
        private LinkLabel lblSyndic8Help;
        private Button btnManageSearchEngines;
        private Label lblNewBySearchEngines;
        private ComboBox cboNewBySearchEngines;
        private LinkLabel lblNewBySearchIntro;
        private TextBox txtNewBySearchWords;
        private Label lblNewBySearchWords;
        private WizardPage pageValidateUrl;
        private Label lblValidationTaskImage2;
        private Label lblValidationTask2;
        private Label lblValidationTaskImage1;
        private Label lblValidationTask1;
        private ProgressBar pbar;
        private Label lblWaitStepIntro;
        private RadioButton radioNewByNNTPGroup;
        private Label lblHowToSubscribeIntro;
        private RadioButton radioNewByTopicSearch;
        private RadioButton radioNewByURL;
        private WizardPage pageStartImport;
        private Label label1;
        private ComboBox comboFeedSource;
        private RadioButton radioImportFromFeedSource;
        private RadioButton radioImportFromOpml;
        private ComboBox comboCategory;
        private Label label3;
        private TextBox textUrlOrFile;
        internal Button btnSelectFile;
        private Label label2;
        private CheckBox chkDisplayWelcome;
        private InformationBox lblWelcomeInfoBox;
        private Label lblWelcomeHelpMessage1;
        private Label lblWelcomeHelpMessage2;
        private WizardPage pageNewByNNTPGroup;
        private ListBox lstNNTPGroups;
        private LinkLabel lblUsenetHelp;
        private PictureBox pictureBox1;
        private LinkLabel lblReloadNntpListOfGroups;
        private Label lblNNTPGroups;
        private Button btnManageNNTPServer;
        private Label lblNNTPServer;
        private ComboBox cboNNTPServer;
        private LinkLabel lblNewByNNTPGroupIntro;
        private Button _btnImmediateFinish;
        private Wizard wizard;
		private System.ComponentModel.IContainer components;

		#endregion

		#region ctor's
		private ImportFeedsWizard() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			// fix the link label(s) linkarea size to fit the whole text (in all translations):
			this.lblReloadNntpListOfGroups.LinkArea = new LinkArea(0,this.lblReloadNntpListOfGroups.Text.Length );
			this.lblSyndic8Help.LinkArea = new LinkArea(0, this.lblSyndic8Help.Text.Length);
			this.lblUsenetHelp.LinkArea = new LinkArea(0, this.lblUsenetHelp.Text.Length);
		}

        public ImportFeedsWizard(IServiceProvider provider, AddSubscriptionWizardMode mode, string urlOrFile, string selectedCategory)
            : this()
        {
            //initialize wizard state
            serviceProvider = provider;
            wizardMode = mode;

            this.textUrlOrFile.Text = (urlOrFile != null ? urlOrFile : String.Empty);

            // form location management:
            windowSerializer = new WindowSerializer(this);
            windowSerializer.SaveOnlyLocation = true;
            windowSerializer.SaveNoWindowState = true;

            windowSerializer.LoadStateEvent += OnWindowSerializerLoadStateEvent;
            windowSerializer.SaveStateEvent += OnWindowSerializerSaveStateEvent;

            // to get notified, if the inet connection state changes:
            internetService = (IInternetService)this.GetService(typeof(IInternetService));
            if (internetService != null)
            {
                internetService.InternetConnectionStateChange += OnInternetServiceInternetConnectionStateChange;
            }
            // to checkout the defaults to be used for the new feed:
            IUserPreferences preferencesService = (IUserPreferences)this.GetService(typeof(IUserPreferences));
            this.MaxItemAge = preferencesService.MaxItemAge;

            coreApplication = (ICoreApplication)this.GetService(typeof(ICoreApplication));
            this.cboUpdateFrequency.Text = String.Format("{0}", RssBanditApplication.DefaultGlobalRefreshRateMinutes);
            if (coreApplication.CurrentGlobalRefreshRate > 0)	// if not disabled refreshing
                this.cboUpdateFrequency.Text = String.Format("{0}", coreApplication.CurrentGlobalRefreshRate);

            //initialize category combo box						
            foreach (string category in coreApplication.GetCategories())
            {
                if (!string.IsNullOrEmpty(category))
                    this.comboCategory.Items.Add(category);
            }
            this.comboCategory.Text = (selectedCategory != null ? selectedCategory : String.Empty);


            this.FeedCategory = coreApplication.DefaultCategory;

            this.WireStepsForMode(this.wizardMode);
            this.wizard.SelectedPage = this.pageStartImport;
        }
        
		#endregion

		#region IWaitDialog Members

		public void Initialize(System.Threading.AutoResetEvent waitHandle, TimeSpan timeout, Icon dialogIcon) {
			this.waitHandle = waitHandle;
			this.timeout = timeout;
			this.timeCounter = 0;
			if (timeout != TimeSpan.Zero)
				this.timeCounter = (int)(timeout.TotalMilliseconds / this.timerIncreaseProgress.Interval);
			this.operationResult = DialogResult.None;
		}

		public System.Windows.Forms.DialogResult StartWaiting(IWin32Window owner, string waitMessage, bool allowCancel) {
			// start animation
			this.wizard.SelectedPage.AllowCancel = allowCancel;
			this.timerIncreaseProgress.Enabled = true;
			// wait for thread end or timeout (set by timerIncreaseProgress)
			do {
				Thread.Sleep(50);
				Application.DoEvents();
			} while (this.operationResult == DialogResult.None);

			this.timerIncreaseProgress.Enabled = false;
			if (this.operationTimeout || this.operationResult == DialogResult.Cancel)
				return DialogResult.Cancel;

			return DialogResult.OK;
		}

		#endregion
		
		#region IServiceProvider Members

		public new object GetService(Type serviceType) {
			
			object srv = null;

			if (serviceProvider != null) {
				srv = serviceProvider.GetService(serviceType);
				if (srv != null)
					return srv;
			}

			//TODO: own services...?

			return srv;
		}

		#endregion

		#region Dispose
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing ) {

				if (components != null) {
					components.Dispose();
				}
				if (internetService != null) {
					internetService.InternetConnectionStateChange -= new InternetConnectionStateChangeHandler(OnInternetServiceInternetConnectionStateChange);
					internetService = null;
				}

			}

			base.Dispose( disposing );
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportFeedsWizard));
            this.timerIncreaseProgress = new System.Windows.Forms.Timer(this.components);
            this.timerStartValidation = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.textUrlOrFile = new System.Windows.Forms.TextBox();
            this.comboCategory = new System.Windows.Forms.ComboBox();
            this.pageFeedCredentials = new Divelements.WizardFramework.WizardPage();
            this.lblFeedCredentialsIntro = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.textUser = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.textPassword = new System.Windows.Forms.TextBox();
            this.pageFeedItemControl = new Divelements.WizardFramework.WizardPage();
            this.comboMaxItemAge = new System.Windows.Forms.ComboBox();
            this.cboUpdateFrequency = new System.Windows.Forms.ComboBox();
            this.lblRemoveItemsOlderThan = new System.Windows.Forms.Label();
            this.lblMinutes = new System.Windows.Forms.Label();
            this.lblUpdateFrequency = new System.Windows.Forms.Label();
            this.checkMarkItemsReadOnExiting = new System.Windows.Forms.CheckBox();
            this.lblFeedItemControlIntro = new System.Windows.Forms.Label();
            this.checkEnableAlertOnNewItems = new System.Windows.Forms.CheckBox();
            this.pageFeedItemDisplay = new Divelements.WizardFramework.WizardPage();
            this.comboFormatters = new System.Windows.Forms.ComboBox();
            this.lblFormatterStylesheet = new System.Windows.Forms.Label();
            this.checkUseCustomFormatter = new System.Windows.Forms.CheckBox();
            this.lblFeedItemDisplayIntro = new System.Windows.Forms.Label();
            this.finishPage = new Divelements.WizardFramework.FinishPage();
            this.pnlCompleting = new System.Windows.Forms.Panel();
            this.lblCompletionMessage = new System.Windows.Forms.Label();
            this.pnlCancelling = new System.Windows.Forms.Panel();
            this.pageTitleCategory = new Divelements.WizardFramework.WizardPage();
            this.lblFeedCategory = new System.Windows.Forms.Label();
            this.cboFeedCategory = new System.Windows.Forms.ComboBox();
            this.lblPageTitleCredentialsIntro = new System.Windows.Forms.Label();
            this.lblFeedTitle = new System.Windows.Forms.Label();
            this.txtFeedTitle = new System.Windows.Forms.TextBox();
            this.pageFoundMultipleFeeds = new Divelements.WizardFramework.WizardPage();
            this.listFeeds = new System.Windows.Forms.ListView();
            this.lblMultipleFeedsFoundHint1 = new System.Windows.Forms.Label();
            this.lblMultipleFeedsFoundHint2 = new System.Windows.Forms.Label();
            this.lblMultipleFeedsFound = new System.Windows.Forms.Label();
            this.pageNewBySearchTopic = new Divelements.WizardFramework.WizardPage();
            this.pictureHelpSyndic8 = new System.Windows.Forms.PictureBox();
            this.lblSyndic8Help = new System.Windows.Forms.LinkLabel();
            this.btnManageSearchEngines = new System.Windows.Forms.Button();
            this.lblNewBySearchEngines = new System.Windows.Forms.Label();
            this.cboNewBySearchEngines = new System.Windows.Forms.ComboBox();
            this.lblNewBySearchIntro = new System.Windows.Forms.LinkLabel();
            this.txtNewBySearchWords = new System.Windows.Forms.TextBox();
            this.lblNewBySearchWords = new System.Windows.Forms.Label();
            this.pageValidateUrl = new Divelements.WizardFramework.WizardPage();
            this.lblValidationTaskImage2 = new System.Windows.Forms.Label();
            this.lblValidationTask2 = new System.Windows.Forms.Label();
            this.lblValidationTaskImage1 = new System.Windows.Forms.Label();
            this.lblValidationTask1 = new System.Windows.Forms.Label();
            this.pbar = new System.Windows.Forms.ProgressBar();
            this.lblWaitStepIntro = new System.Windows.Forms.Label();
            this.radioNewByNNTPGroup = new System.Windows.Forms.RadioButton();
            this.lblHowToSubscribeIntro = new System.Windows.Forms.Label();
            this.radioNewByTopicSearch = new System.Windows.Forms.RadioButton();
            this.radioNewByURL = new System.Windows.Forms.RadioButton();
            this.pageStartImport = new Divelements.WizardFramework.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.comboFeedSource = new System.Windows.Forms.ComboBox();
            this.radioImportFromFeedSource = new System.Windows.Forms.RadioButton();
            this.radioImportFromOpml = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblWelcomeHelpMessage2 = new System.Windows.Forms.Label();
            this.lblWelcomeHelpMessage1 = new System.Windows.Forms.Label();
            this.lblWelcomeInfoBox = new Divelements.WizardFramework.InformationBox();
            this.chkDisplayWelcome = new System.Windows.Forms.CheckBox();
            this.pageNewByNNTPGroup = new Divelements.WizardFramework.WizardPage();
            this.lstNNTPGroups = new System.Windows.Forms.ListBox();
            this.lblUsenetHelp = new System.Windows.Forms.LinkLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblReloadNntpListOfGroups = new System.Windows.Forms.LinkLabel();
            this.lblNNTPGroups = new System.Windows.Forms.Label();
            this.btnManageNNTPServer = new System.Windows.Forms.Button();
            this.lblNNTPServer = new System.Windows.Forms.Label();
            this.cboNNTPServer = new System.Windows.Forms.ComboBox();
            this.lblNewByNNTPGroupIntro = new System.Windows.Forms.LinkLabel();
            this._btnImmediateFinish = new System.Windows.Forms.Button();
            this.wizard = new Divelements.WizardFramework.Wizard();
            this.pageFeedCredentials.SuspendLayout();
            this.pageFeedItemControl.SuspendLayout();
            this.pageFeedItemDisplay.SuspendLayout();
            this.finishPage.SuspendLayout();
            this.pnlCompleting.SuspendLayout();
            this.pageTitleCategory.SuspendLayout();
            this.pageFoundMultipleFeeds.SuspendLayout();
            this.pageNewBySearchTopic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureHelpSyndic8)).BeginInit();
            this.pageValidateUrl.SuspendLayout();
            this.pageStartImport.SuspendLayout();
            this.pageNewByNNTPGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.wizard.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerIncreaseProgress
            // 
            this.timerIncreaseProgress.Tick += new System.EventHandler(this.OnTimerIncreaseProgress_Tick);
            // 
            // timerStartValidation
            // 
            this.timerStartValidation.Tick += new System.EventHandler(this.OnTimerStartValidation);
            // 
            // btnSelectFile
            // 
            resources.ApplyResources(this.btnSelectFile, "btnSelectFile");
            this.btnSelectFile.Name = "btnSelectFile";
            this.toolTip.SetToolTip(this.btnSelectFile, resources.GetString("btnSelectFile.ToolTip"));
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // textUrlOrFile
            // 
            this.textUrlOrFile.AllowDrop = true;
            resources.ApplyResources(this.textUrlOrFile, "textUrlOrFile");
            this.textUrlOrFile.Name = "textUrlOrFile";
            this.toolTip.SetToolTip(this.textUrlOrFile, resources.GetString("textUrlOrFile.ToolTip"));
            // 
            // comboCategory
            // 
            resources.ApplyResources(this.comboCategory, "comboCategory");
            this.comboCategory.Name = "comboCategory";
            this.comboCategory.Sorted = true;
            this.toolTip.SetToolTip(this.comboCategory, resources.GetString("comboCategory.ToolTip"));
            // 
            // pageFeedCredentials
            // 
            this.pageFeedCredentials.Controls.Add(this.lblFeedCredentialsIntro);
            this.pageFeedCredentials.Controls.Add(this.lblUsername);
            this.pageFeedCredentials.Controls.Add(this.textUser);
            this.pageFeedCredentials.Controls.Add(this.lblPassword);
            this.pageFeedCredentials.Controls.Add(this.textPassword);
            resources.ApplyResources(this.pageFeedCredentials, "pageFeedCredentials");
            this.pageFeedCredentials.Name = "pageFeedCredentials";
            this.pageFeedCredentials.NextPage = this.pageFeedItemControl;
            this.pageFeedCredentials.PreviousPage = this.pageTitleCategory;
            this.pageFeedCredentials.BeforeDisplay += new System.EventHandler(this.pageFeedCredentials_BeforeDisplay);
            // 
            // lblFeedCredentialsIntro
            // 
            resources.ApplyResources(this.lblFeedCredentialsIntro, "lblFeedCredentialsIntro");
            this.lblFeedCredentialsIntro.Name = "lblFeedCredentialsIntro";
            // 
            // lblUsername
            // 
            resources.ApplyResources(this.lblUsername, "lblUsername");
            this.lblUsername.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblUsername.Name = "lblUsername";
            // 
            // textUser
            // 
            resources.ApplyResources(this.textUser, "textUser");
            this.textUser.Name = "textUser";
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblPassword.Name = "lblPassword";
            // 
            // textPassword
            // 
            resources.ApplyResources(this.textPassword, "textPassword");
            this.textPassword.Name = "textPassword";
            // 
            // pageFeedItemControl
            // 
            this.pageFeedItemControl.Controls.Add(this.comboMaxItemAge);
            this.pageFeedItemControl.Controls.Add(this.cboUpdateFrequency);
            this.pageFeedItemControl.Controls.Add(this.lblRemoveItemsOlderThan);
            this.pageFeedItemControl.Controls.Add(this.lblMinutes);
            this.pageFeedItemControl.Controls.Add(this.lblUpdateFrequency);
            this.pageFeedItemControl.Controls.Add(this.checkMarkItemsReadOnExiting);
            this.pageFeedItemControl.Controls.Add(this.lblFeedItemControlIntro);
            this.pageFeedItemControl.Controls.Add(this.checkEnableAlertOnNewItems);
            resources.ApplyResources(this.pageFeedItemControl, "pageFeedItemControl");
            this.pageFeedItemControl.Name = "pageFeedItemControl";
            this.pageFeedItemControl.NextPage = this.pageFeedItemDisplay;
            this.pageFeedItemControl.PreviousPage = this.pageFeedCredentials;
            // 
            // comboMaxItemAge
            // 
            this.comboMaxItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.comboMaxItemAge, "comboMaxItemAge");
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
            this.comboMaxItemAge.Name = "comboMaxItemAge";
            // 
            // cboUpdateFrequency
            // 
            resources.ApplyResources(this.cboUpdateFrequency, "cboUpdateFrequency");
            this.cboUpdateFrequency.Items.AddRange(new object[] {
            resources.GetString("cboUpdateFrequency.Items"),
            resources.GetString("cboUpdateFrequency.Items1"),
            resources.GetString("cboUpdateFrequency.Items2"),
            resources.GetString("cboUpdateFrequency.Items3"),
            resources.GetString("cboUpdateFrequency.Items4"),
            resources.GetString("cboUpdateFrequency.Items5"),
            resources.GetString("cboUpdateFrequency.Items6"),
            resources.GetString("cboUpdateFrequency.Items7"),
            resources.GetString("cboUpdateFrequency.Items8")});
            this.cboUpdateFrequency.Name = "cboUpdateFrequency";
            // 
            // lblRemoveItemsOlderThan
            // 
            resources.ApplyResources(this.lblRemoveItemsOlderThan, "lblRemoveItemsOlderThan");
            this.lblRemoveItemsOlderThan.Name = "lblRemoveItemsOlderThan";
            // 
            // lblMinutes
            // 
            resources.ApplyResources(this.lblMinutes, "lblMinutes");
            this.lblMinutes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblMinutes.Name = "lblMinutes";
            // 
            // lblUpdateFrequency
            // 
            resources.ApplyResources(this.lblUpdateFrequency, "lblUpdateFrequency");
            this.lblUpdateFrequency.Name = "lblUpdateFrequency";
            // 
            // checkMarkItemsReadOnExiting
            // 
            resources.ApplyResources(this.checkMarkItemsReadOnExiting, "checkMarkItemsReadOnExiting");
            this.checkMarkItemsReadOnExiting.Name = "checkMarkItemsReadOnExiting";
            // 
            // lblFeedItemControlIntro
            // 
            resources.ApplyResources(this.lblFeedItemControlIntro, "lblFeedItemControlIntro");
            this.lblFeedItemControlIntro.Name = "lblFeedItemControlIntro";
            // 
            // checkEnableAlertOnNewItems
            // 
            resources.ApplyResources(this.checkEnableAlertOnNewItems, "checkEnableAlertOnNewItems");
            this.checkEnableAlertOnNewItems.Name = "checkEnableAlertOnNewItems";
            // 
            // pageFeedItemDisplay
            // 
            this.pageFeedItemDisplay.Controls.Add(this.comboFormatters);
            this.pageFeedItemDisplay.Controls.Add(this.lblFormatterStylesheet);
            this.pageFeedItemDisplay.Controls.Add(this.checkUseCustomFormatter);
            this.pageFeedItemDisplay.Controls.Add(this.lblFeedItemDisplayIntro);
            resources.ApplyResources(this.pageFeedItemDisplay, "pageFeedItemDisplay");
            this.pageFeedItemDisplay.Name = "pageFeedItemDisplay";
            this.pageFeedItemDisplay.NextPage = this.finishPage;
            this.pageFeedItemDisplay.PreviousPage = this.pageFeedItemControl;
            this.pageFeedItemDisplay.AfterDisplay += new System.EventHandler(this.OnPageFeedItemDisplayAfterDisplay);
            // 
            // comboFormatters
            // 
            resources.ApplyResources(this.comboFormatters, "comboFormatters");
            this.comboFormatters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFormatters.Name = "comboFormatters";
            this.comboFormatters.Sorted = true;
            // 
            // lblFormatterStylesheet
            // 
            resources.ApplyResources(this.lblFormatterStylesheet, "lblFormatterStylesheet");
            this.lblFormatterStylesheet.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblFormatterStylesheet.Name = "lblFormatterStylesheet";
            // 
            // checkUseCustomFormatter
            // 
            resources.ApplyResources(this.checkUseCustomFormatter, "checkUseCustomFormatter");
            this.checkUseCustomFormatter.Name = "checkUseCustomFormatter";
            // 
            // lblFeedItemDisplayIntro
            // 
            resources.ApplyResources(this.lblFeedItemDisplayIntro, "lblFeedItemDisplayIntro");
            this.lblFeedItemDisplayIntro.Name = "lblFeedItemDisplayIntro";
            // 
            // finishPage
            // 
            this.finishPage.Controls.Add(this.pnlCompleting);
            this.finishPage.Controls.Add(this.pnlCancelling);
            resources.ApplyResources(this.finishPage, "finishPage");
            this.finishPage.Name = "finishPage";
            this.finishPage.PreviousPage = this.pageFeedItemDisplay;
            this.finishPage.SettingsHeader = "";
            this.finishPage.BeforeMoveBack += new Divelements.WizardFramework.WizardPageEventHandler(this.OnFinishPage_BeforeMoveBack);
            this.finishPage.BeforeDisplay += new System.EventHandler(this.OnFinishPage_BeforeDisplay);
            // 
            // pnlCompleting
            // 
            resources.ApplyResources(this.pnlCompleting, "pnlCompleting");
            this.pnlCompleting.Controls.Add(this.lblCompletionMessage);
            this.pnlCompleting.Name = "pnlCompleting";
            // 
            // lblCompletionMessage
            // 
            resources.ApplyResources(this.lblCompletionMessage, "lblCompletionMessage");
            this.lblCompletionMessage.Name = "lblCompletionMessage";
            // 
            // pnlCancelling
            // 
            resources.ApplyResources(this.pnlCancelling, "pnlCancelling");
            this.pnlCancelling.Name = "pnlCancelling";
            // 
            // pageTitleCategory
            // 
            this.pageTitleCategory.Controls.Add(this.lblFeedCategory);
            this.pageTitleCategory.Controls.Add(this.cboFeedCategory);
            this.pageTitleCategory.Controls.Add(this.lblPageTitleCredentialsIntro);
            this.pageTitleCategory.Controls.Add(this.lblFeedTitle);
            this.pageTitleCategory.Controls.Add(this.txtFeedTitle);
            resources.ApplyResources(this.pageTitleCategory, "pageTitleCategory");
            this.pageTitleCategory.Name = "pageTitleCategory";
            this.pageTitleCategory.NextPage = this.pageFeedCredentials;
            this.pageTitleCategory.PreviousPage = this.pageFoundMultipleFeeds;
            this.pageTitleCategory.BeforeMoveBack += new Divelements.WizardFramework.WizardPageEventHandler(this.OnPageTitleCategoryBeforeMoveBack);
            this.pageTitleCategory.BeforeDisplay += new System.EventHandler(this.OnPageTitleCategoryBeforeDisplay);
            // 
            // lblFeedCategory
            // 
            resources.ApplyResources(this.lblFeedCategory, "lblFeedCategory");
            this.lblFeedCategory.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblFeedCategory.Name = "lblFeedCategory";
            // 
            // cboFeedCategory
            // 
            resources.ApplyResources(this.cboFeedCategory, "cboFeedCategory");
            this.cboFeedCategory.Name = "cboFeedCategory";
            this.cboFeedCategory.Sorted = true;
            // 
            // lblPageTitleCredentialsIntro
            // 
            resources.ApplyResources(this.lblPageTitleCredentialsIntro, "lblPageTitleCredentialsIntro");
            this.lblPageTitleCredentialsIntro.Name = "lblPageTitleCredentialsIntro";
            // 
            // lblFeedTitle
            // 
            resources.ApplyResources(this.lblFeedTitle, "lblFeedTitle");
            this.lblFeedTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblFeedTitle.Name = "lblFeedTitle";
            // 
            // txtFeedTitle
            // 
            resources.ApplyResources(this.txtFeedTitle, "txtFeedTitle");
            this.txtFeedTitle.Name = "txtFeedTitle";
            this.txtFeedTitle.TextChanged += new System.EventHandler(this.OnNewFeedTitleTextChanged);
            // 
            // pageFoundMultipleFeeds
            // 
            this.pageFoundMultipleFeeds.AllowMoveNext = false;
            this.pageFoundMultipleFeeds.Controls.Add(this.listFeeds);
            this.pageFoundMultipleFeeds.Controls.Add(this.lblMultipleFeedsFoundHint1);
            this.pageFoundMultipleFeeds.Controls.Add(this.lblMultipleFeedsFoundHint2);
            this.pageFoundMultipleFeeds.Controls.Add(this.lblMultipleFeedsFound);
            resources.ApplyResources(this.pageFoundMultipleFeeds, "pageFoundMultipleFeeds");
            this.pageFoundMultipleFeeds.Name = "pageFoundMultipleFeeds";
            this.pageFoundMultipleFeeds.NextPage = this.pageTitleCategory;
            this.pageFoundMultipleFeeds.PreviousPage = this.pageNewBySearchTopic;
            this.pageFoundMultipleFeeds.BeforeMoveNext += new Divelements.WizardFramework.WizardPageEventHandler(this.OnMultipleFeedsBeforeMoveNext);
            // 
            // listFeeds
            // 
            resources.ApplyResources(this.listFeeds, "listFeeds");
            this.listFeeds.FullRowSelect = true;
            this.listFeeds.HideSelection = false;
            this.listFeeds.Name = "listFeeds";
            this.listFeeds.UseCompatibleStateImageBehavior = false;
            this.listFeeds.View = System.Windows.Forms.View.Details;
            this.listFeeds.SelectedIndexChanged += new System.EventHandler(this.OnFoundFeedsListSelectedIndexChanged);
            this.listFeeds.DoubleClick += new System.EventHandler(this.OnListFoundFeeds_DoubleClick);
            // 
            // lblMultipleFeedsFoundHint1
            // 
            resources.ApplyResources(this.lblMultipleFeedsFoundHint1, "lblMultipleFeedsFoundHint1");
            this.lblMultipleFeedsFoundHint1.Name = "lblMultipleFeedsFoundHint1";
            // 
            // lblMultipleFeedsFoundHint2
            // 
            resources.ApplyResources(this.lblMultipleFeedsFoundHint2, "lblMultipleFeedsFoundHint2");
            this.lblMultipleFeedsFoundHint2.Name = "lblMultipleFeedsFoundHint2";
            // 
            // lblMultipleFeedsFound
            // 
            resources.ApplyResources(this.lblMultipleFeedsFound, "lblMultipleFeedsFound");
            this.lblMultipleFeedsFound.Name = "lblMultipleFeedsFound";
            // 
            // pageNewBySearchTopic
            // 
            this.pageNewBySearchTopic.Controls.Add(this.pictureHelpSyndic8);
            this.pageNewBySearchTopic.Controls.Add(this.lblSyndic8Help);
            this.pageNewBySearchTopic.Controls.Add(this.btnManageSearchEngines);
            this.pageNewBySearchTopic.Controls.Add(this.lblNewBySearchEngines);
            this.pageNewBySearchTopic.Controls.Add(this.cboNewBySearchEngines);
            this.pageNewBySearchTopic.Controls.Add(this.lblNewBySearchIntro);
            this.pageNewBySearchTopic.Controls.Add(this.txtNewBySearchWords);
            this.pageNewBySearchTopic.Controls.Add(this.lblNewBySearchWords);
            resources.ApplyResources(this.pageNewBySearchTopic, "pageNewBySearchTopic");
            this.pageNewBySearchTopic.Name = "pageNewBySearchTopic";
            this.pageNewBySearchTopic.NextPage = this.pageValidateUrl;
            this.pageNewBySearchTopic.AfterDisplay += new System.EventHandler(this.OnPageNewSearchAfterDisplay);
            // 
            // pictureHelpSyndic8
            // 
            resources.ApplyResources(this.pictureHelpSyndic8, "pictureHelpSyndic8");
            this.pictureHelpSyndic8.Name = "pictureHelpSyndic8";
            this.pictureHelpSyndic8.TabStop = false;
            // 
            // lblSyndic8Help
            // 
            this.lblSyndic8Help.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.lblSyndic8Help, "lblSyndic8Help");
            this.lblSyndic8Help.Name = "lblSyndic8Help";
            this.lblSyndic8Help.TabStop = true;
            this.lblSyndic8Help.Tag = "http://www.syndic8.com/";
            this.lblSyndic8Help.UseCompatibleTextRendering = true;
            this.lblSyndic8Help.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
            // 
            // btnManageSearchEngines
            // 
            resources.ApplyResources(this.btnManageSearchEngines, "btnManageSearchEngines");
            this.btnManageSearchEngines.Name = "btnManageSearchEngines";
            this.btnManageSearchEngines.Click += new System.EventHandler(this.btnManageSearchEngines_Click);
            // 
            // lblNewBySearchEngines
            // 
            resources.ApplyResources(this.lblNewBySearchEngines, "lblNewBySearchEngines");
            this.lblNewBySearchEngines.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblNewBySearchEngines.Name = "lblNewBySearchEngines";
            // 
            // cboNewBySearchEngines
            // 
            resources.ApplyResources(this.cboNewBySearchEngines, "cboNewBySearchEngines");
            this.cboNewBySearchEngines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNewBySearchEngines.Name = "cboNewBySearchEngines";
            // 
            // lblNewBySearchIntro
            // 
            resources.ApplyResources(this.lblNewBySearchIntro, "lblNewBySearchIntro");
            this.lblNewBySearchIntro.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblNewBySearchIntro.Name = "lblNewBySearchIntro";
            this.lblNewBySearchIntro.Tag = "";
            // 
            // txtNewBySearchWords
            // 
            resources.ApplyResources(this.txtNewBySearchWords, "txtNewBySearchWords");
            this.txtNewBySearchWords.Name = "txtNewBySearchWords";
            // 
            // lblNewBySearchWords
            // 
            resources.ApplyResources(this.lblNewBySearchWords, "lblNewBySearchWords");
            this.lblNewBySearchWords.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblNewBySearchWords.Name = "lblNewBySearchWords";
            // 
            // pageValidateUrl
            // 
            this.pageValidateUrl.AllowCancel = false;
            this.pageValidateUrl.AllowMoveNext = false;
            this.pageValidateUrl.AllowMovePrevious = false;
            this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage2);
            this.pageValidateUrl.Controls.Add(this.lblValidationTask2);
            this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage1);
            this.pageValidateUrl.Controls.Add(this.lblValidationTask1);
            this.pageValidateUrl.Controls.Add(this.pbar);
            this.pageValidateUrl.Controls.Add(this.lblWaitStepIntro);
            resources.ApplyResources(this.pageValidateUrl, "pageValidateUrl");
            this.pageValidateUrl.Name = "pageValidateUrl";
            this.pageValidateUrl.NextPage = this.pageFoundMultipleFeeds;
            this.pageValidateUrl.PreviousPage = this.pageNewBySearchTopic;
            this.pageValidateUrl.AfterDisplay += new System.EventHandler(this.OnPageValidation_AfterDisplay);
            this.pageValidateUrl.BeforeDisplay += new System.EventHandler(this.OnPageValidation_BeforeDisplay);
            // 
            // lblValidationTaskImage2
            // 
            resources.ApplyResources(this.lblValidationTaskImage2, "lblValidationTaskImage2");
            this.lblValidationTaskImage2.Name = "lblValidationTaskImage2";
            // 
            // lblValidationTask2
            // 
            resources.ApplyResources(this.lblValidationTask2, "lblValidationTask2");
            this.lblValidationTask2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblValidationTask2.Name = "lblValidationTask2";
            // 
            // lblValidationTaskImage1
            // 
            resources.ApplyResources(this.lblValidationTaskImage1, "lblValidationTaskImage1");
            this.lblValidationTaskImage1.Name = "lblValidationTaskImage1";
            // 
            // lblValidationTask1
            // 
            resources.ApplyResources(this.lblValidationTask1, "lblValidationTask1");
            this.lblValidationTask1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblValidationTask1.Name = "lblValidationTask1";
            // 
            // pbar
            // 
            resources.ApplyResources(this.pbar, "pbar");
            this.pbar.Name = "pbar";
            this.pbar.Step = 2;
            // 
            // lblWaitStepIntro
            // 
            resources.ApplyResources(this.lblWaitStepIntro, "lblWaitStepIntro");
            this.lblWaitStepIntro.Name = "lblWaitStepIntro";
            // 
            // radioNewByNNTPGroup
            // 
            resources.ApplyResources(this.radioNewByNNTPGroup, "radioNewByNNTPGroup");
            this.radioNewByNNTPGroup.Name = "radioNewByNNTPGroup";
            this.radioNewByNNTPGroup.CheckedChanged += new System.EventHandler(this.OnRadioHowToSubscribeCheckedChanged);
            // 
            // lblHowToSubscribeIntro
            // 
            resources.ApplyResources(this.lblHowToSubscribeIntro, "lblHowToSubscribeIntro");
            this.lblHowToSubscribeIntro.Name = "lblHowToSubscribeIntro";
            // 
            // radioNewByTopicSearch
            // 
            resources.ApplyResources(this.radioNewByTopicSearch, "radioNewByTopicSearch");
            this.radioNewByTopicSearch.Name = "radioNewByTopicSearch";
            this.radioNewByTopicSearch.CheckedChanged += new System.EventHandler(this.OnRadioHowToSubscribeCheckedChanged);
            // 
            // radioNewByURL
            // 
            resources.ApplyResources(this.radioNewByURL, "radioNewByURL");
            this.radioNewByURL.Checked = true;
            this.radioNewByURL.Name = "radioNewByURL";
            this.radioNewByURL.TabStop = true;
            this.radioNewByURL.CheckedChanged += new System.EventHandler(this.OnRadioHowToSubscribeCheckedChanged);
            // 
            // pageStartImport
            // 
            this.pageStartImport.Controls.Add(this.label1);
            this.pageStartImport.Controls.Add(this.comboFeedSource);
            this.pageStartImport.Controls.Add(this.radioImportFromFeedSource);
            this.pageStartImport.Controls.Add(this.radioImportFromOpml);
            this.pageStartImport.Controls.Add(this.comboCategory);
            this.pageStartImport.Controls.Add(this.label3);
            this.pageStartImport.Controls.Add(this.textUrlOrFile);
            this.pageStartImport.Controls.Add(this.btnSelectFile);
            this.pageStartImport.Controls.Add(this.label2);
            resources.ApplyResources(this.pageStartImport, "pageStartImport");
            this.pageStartImport.Name = "pageStartImport";
            this.pageStartImport.NextPage = this.pageValidateUrl;
            this.pageStartImport.AfterDisplay += new System.EventHandler(this.OnPageNewURLAfterDisplay);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // comboFeedSource
            // 
            this.comboFeedSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFeedSource.FormattingEnabled = true;
            this.comboFeedSource.Items.AddRange(new object[] {
            resources.GetString("comboFeedSource.Items"),
            resources.GetString("comboFeedSource.Items1"),
            resources.GetString("comboFeedSource.Items2")});
            resources.ApplyResources(this.comboFeedSource, "comboFeedSource");
            this.comboFeedSource.Name = "comboFeedSource";
            // 
            // radioImportFromFeedSource
            // 
            resources.ApplyResources(this.radioImportFromFeedSource, "radioImportFromFeedSource");
            this.radioImportFromFeedSource.Name = "radioImportFromFeedSource";
            this.radioImportFromFeedSource.TabStop = true;
            this.radioImportFromFeedSource.UseVisualStyleBackColor = true;
            this.radioImportFromFeedSource.CheckedChanged += new System.EventHandler(this.radioImportFromFeedSource_CheckedChanged);
            // 
            // radioImportFromOpml
            // 
            resources.ApplyResources(this.radioImportFromOpml, "radioImportFromOpml");
            this.radioImportFromOpml.Name = "radioImportFromOpml";
            this.radioImportFromOpml.TabStop = true;
            this.radioImportFromOpml.UseVisualStyleBackColor = true;
            this.radioImportFromOpml.CheckedChanged += new System.EventHandler(this.radioImportFromOpml_CheckedChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // lblWelcomeHelpMessage2
            // 
            resources.ApplyResources(this.lblWelcomeHelpMessage2, "lblWelcomeHelpMessage2");
            this.lblWelcomeHelpMessage2.Name = "lblWelcomeHelpMessage2";
            // 
            // lblWelcomeHelpMessage1
            // 
            resources.ApplyResources(this.lblWelcomeHelpMessage1, "lblWelcomeHelpMessage1");
            this.lblWelcomeHelpMessage1.Name = "lblWelcomeHelpMessage1";
            // 
            // lblWelcomeInfoBox
            // 
            resources.ApplyResources(this.lblWelcomeInfoBox, "lblWelcomeInfoBox");
            this.lblWelcomeInfoBox.Icon = Divelements.WizardFramework.SystemIconType.Warning;
            this.lblWelcomeInfoBox.Name = "lblWelcomeInfoBox";
            // 
            // chkDisplayWelcome
            // 
            this.chkDisplayWelcome.Checked = true;
            this.chkDisplayWelcome.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.chkDisplayWelcome, "chkDisplayWelcome");
            this.chkDisplayWelcome.Name = "chkDisplayWelcome";
            // 
            // pageNewByNNTPGroup
            // 
            this.pageNewByNNTPGroup.Controls.Add(this.lstNNTPGroups);
            this.pageNewByNNTPGroup.Controls.Add(this.lblUsenetHelp);
            this.pageNewByNNTPGroup.Controls.Add(this.pictureBox1);
            this.pageNewByNNTPGroup.Controls.Add(this.lblReloadNntpListOfGroups);
            this.pageNewByNNTPGroup.Controls.Add(this.lblNNTPGroups);
            this.pageNewByNNTPGroup.Controls.Add(this.btnManageNNTPServer);
            this.pageNewByNNTPGroup.Controls.Add(this.lblNNTPServer);
            this.pageNewByNNTPGroup.Controls.Add(this.cboNNTPServer);
            this.pageNewByNNTPGroup.Controls.Add(this.lblNewByNNTPGroupIntro);
            resources.ApplyResources(this.pageNewByNNTPGroup, "pageNewByNNTPGroup");
            this.pageNewByNNTPGroup.Name = "pageNewByNNTPGroup";
            this.pageNewByNNTPGroup.NextPage = this.pageTitleCategory;
            this.pageNewByNNTPGroup.AfterDisplay += new System.EventHandler(this.OnPageNewNNTPGroupAfterDisplay);
            // 
            // lstNNTPGroups
            // 
            resources.ApplyResources(this.lstNNTPGroups, "lstNNTPGroups");
            this.lstNNTPGroups.Name = "lstNNTPGroups";
            this.lstNNTPGroups.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstNNTPGroups.Sorted = true;
            this.lstNNTPGroups.DoubleClick += new System.EventHandler(this.OnNNTPGroupsDoubleClick);
            this.lstNNTPGroups.SelectedValueChanged += new System.EventHandler(this.OnNNTPGroupsListSelectedValueChanged);
            // 
            // lblUsenetHelp
            // 
            this.lblUsenetHelp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.lblUsenetHelp, "lblUsenetHelp");
            this.lblUsenetHelp.Name = "lblUsenetHelp";
            this.lblUsenetHelp.TabStop = true;
            this.lblUsenetHelp.Tag = "http://wikipedia.org/wiki/Usenet";
            this.lblUsenetHelp.UseCompatibleTextRendering = true;
            this.lblUsenetHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // lblReloadNntpListOfGroups
            // 
            resources.ApplyResources(this.lblReloadNntpListOfGroups, "lblReloadNntpListOfGroups");
            this.lblReloadNntpListOfGroups.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblReloadNntpListOfGroups.Name = "lblReloadNntpListOfGroups";
            this.lblReloadNntpListOfGroups.TabStop = true;
            this.lblReloadNntpListOfGroups.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnReloadNntpGroupList);
            // 
            // lblNNTPGroups
            // 
            resources.ApplyResources(this.lblNNTPGroups, "lblNNTPGroups");
            this.lblNNTPGroups.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblNNTPGroups.Name = "lblNNTPGroups";
            // 
            // btnManageNNTPServer
            // 
            resources.ApplyResources(this.btnManageNNTPServer, "btnManageNNTPServer");
            this.btnManageNNTPServer.Name = "btnManageNNTPServer";
            this.btnManageNNTPServer.Click += new System.EventHandler(this.btnManageNNTPServer_Click);
            // 
            // lblNNTPServer
            // 
            resources.ApplyResources(this.lblNNTPServer, "lblNNTPServer");
            this.lblNNTPServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblNNTPServer.Name = "lblNNTPServer";
            // 
            // cboNNTPServer
            // 
            resources.ApplyResources(this.cboNNTPServer, "cboNNTPServer");
            this.cboNNTPServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNNTPServer.Name = "cboNNTPServer";
            this.cboNNTPServer.SelectedValueChanged += new System.EventHandler(this.OnNNTPServerSelectedValueChanged);
            // 
            // lblNewByNNTPGroupIntro
            // 
            resources.ApplyResources(this.lblNewByNNTPGroupIntro, "lblNewByNNTPGroupIntro");
            this.lblNewByNNTPGroupIntro.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblNewByNNTPGroupIntro.Name = "lblNewByNNTPGroupIntro";
            this.lblNewByNNTPGroupIntro.Tag = "";
            // 
            // _btnImmediateFinish
            // 
            resources.ApplyResources(this._btnImmediateFinish, "_btnImmediateFinish");
            this._btnImmediateFinish.Name = "_btnImmediateFinish";
            this._btnImmediateFinish.Click += new System.EventHandler(this.OnImmediateFinish_Click);
            // 
            // wizard
            // 
            this.wizard.BannerImage = ((System.Drawing.Image)(resources.GetObject("wizard.BannerImage")));
            this.wizard.Controls.Add(this._btnImmediateFinish);
            this.wizard.Controls.Add(this.pageStartImport);
            this.wizard.Controls.Add(this.pageNewBySearchTopic);
            this.wizard.Controls.Add(this.pageNewByNNTPGroup);
            this.wizard.Controls.Add(this.pageValidateUrl);
            this.wizard.Controls.Add(this.pageTitleCategory);
            this.wizard.Controls.Add(this.pageFoundMultipleFeeds);
            this.wizard.Controls.Add(this.pageFeedItemControl);
            this.wizard.Controls.Add(this.pageFeedItemDisplay);
            this.wizard.Controls.Add(this.finishPage);
            this.wizard.Controls.Add(this.pageFeedCredentials);
            resources.ApplyResources(this.wizard, "wizard");
            this.wizard.MarginImage = ((System.Drawing.Image)(resources.GetObject("wizard.MarginImage")));
            this.wizard.Name = "wizard";
            this.wizard.SelectedPage = this.pageStartImport;
            this.wizard.Finish += new System.EventHandler(this.OnWizardFinish);
            this.wizard.Cancel += new System.EventHandler(this.OnWizardCancel);
            // 
            // ImportFeedsWizard
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.wizard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportFeedsWizard";
            this.pageFeedCredentials.ResumeLayout(false);
            this.pageFeedCredentials.PerformLayout();
            this.pageFeedItemControl.ResumeLayout(false);
            this.pageFeedItemDisplay.ResumeLayout(false);
            this.finishPage.ResumeLayout(false);
            this.pnlCompleting.ResumeLayout(false);
            this.pageTitleCategory.ResumeLayout(false);
            this.pageTitleCategory.PerformLayout();
            this.pageFoundMultipleFeeds.ResumeLayout(false);
            this.pageNewBySearchTopic.ResumeLayout(false);
            this.pageNewBySearchTopic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureHelpSyndic8)).EndInit();
            this.pageValidateUrl.ResumeLayout(false);
            this.pageStartImport.ResumeLayout(false);
            this.pageStartImport.PerformLayout();
            this.pageNewByNNTPGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.wizard.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void WireStepsForMode(AddSubscriptionWizardMode m) 
		{
			// reset rewire credential steps:
			this.ReWireCredentialsStep(false);
			this.wizardMode = m;
			switch (m) {
				case AddSubscriptionWizardMode.Default:
					// nothing yet. Depends on user selection (pageHowToSelection)
					break;
				case AddSubscriptionWizardMode.SubscribeURL:
					
					break;
				case AddSubscriptionWizardMode.SubscribeURLDirect:				
					pageTitleCategory.PreviousPage = pageStartImport;
					break;
				case AddSubscriptionWizardMode.SubscribeNNTPGroup:
				
					pageTitleCategory.PreviousPage = pageNewByNNTPGroup;
					break;
				case AddSubscriptionWizardMode.SubscribeNNTPDirect:				
					pageTitleCategory.PreviousPage = pageNewByNNTPGroup;
					break;
				case AddSubscriptionWizardMode.SubscribeSearch:
				
					pageFoundMultipleFeeds.PreviousPage = pageNewBySearchTopic;
					pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
					break;
				case AddSubscriptionWizardMode.SubscribeSearchDirect:

					pageFoundMultipleFeeds.PreviousPage = pageNewBySearchTopic;
					pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
					break;
				case AddSubscriptionWizardMode.SubscribeNNTPGroupDirect:
				
					break;
				default:
					throw new InvalidOperationException("WizardMode '" + m.ToString() + "' not supported");
			}
		}
		
		internal void ReWireCredentialsStep(bool beforeDiscoverValidateUrl) {
			if (this.credentialsStepReWired == beforeDiscoverValidateUrl)
				return;
			if (beforeDiscoverValidateUrl) {
				pageStartImport.NextPage = pageFeedCredentials;
				pageFeedCredentials.PreviousPage = pageStartImport;
				pageFeedCredentials.NextPage = pageValidateUrl;
				pageValidateUrl.PreviousPage = pageFeedCredentials;
			} else {
				pageStartImport.NextPage = pageValidateUrl;
				pageFeedCredentials.PreviousPage = pageTitleCategory;
				pageFeedCredentials.NextPage = pageFeedItemControl;
				pageValidateUrl.PreviousPage = pageStartImport;
			}
			this.credentialsStepReWired = beforeDiscoverValidateUrl;
		}

		private void ResetValidationPage() {
			this.feedInfo = null;
			pbar.Value = 0;
			for (int i = 0; i < 2; i++)
				SetWizardTaskInfo(i, WizardValidationTask.None, String.Empty);
		}

		private Image GetWizardTaskImage(WizardValidationTask task) {
			if (task == WizardValidationTask.None)
				return null;
			return Resource.LoadBitmap("Resources.WizardTask." + task.ToString() + ".png");
		}

		/// <summary>
		/// Set the validation task info for one task
		/// </summary>
		/// <param name="index">Zero based.</param>
		/// <param name="task"></param>
		/// <param name="message"></param>
		private void SetWizardTaskInfo(int index, WizardValidationTask task, string message) {
			switch (index) {
				case 0:
					lblValidationTaskImage1.Image = GetWizardTaskImage(task);
					lblValidationTask1.Text = message;
					lblValidationTaskImage1.Visible = lblValidationTask1.Visible = (task != WizardValidationTask.None);
					break;
				case 1:
					lblValidationTaskImage2.Image = GetWizardTaskImage(task);
					lblValidationTask2.Text = message;
					lblValidationTaskImage2.Visible = lblValidationTask2.Visible = (task != WizardValidationTask.None);
					break;
				default:
					throw new InvalidOperationException("No wizard info can be set for index '" + index.ToString() + "'.");
			}
		}
		
//		public int MeasureStringWidth(Graphics g, Control c) {
//			string measureString = c.Text;
//			Font stringFont = c.Font;
//			// Set string format.
//			StringFormat newStringFormat = StringFormat.GenericTypographic;
//			// Measure string.
//			SizeF stringSize = new SizeF();
//			stringSize = g.MeasureString(
//				measureString,
//				stringFont,
//				c.Width,
//				newStringFormat);
//			return (int)Math.Ceiling(stringSize.Height) + 3;
//		}

		private bool TaskBeginSearchFeedLocations(int taskIndex, FeedLocationMethod method) {
			try { 

				AutoDiscoverFeedsThreadHandler autoDiscover = new AutoDiscoverFeedsThreadHandler();
				autoDiscover.Proxy = coreApplication.Proxy;
				autoDiscover.LocationMethod = method;
				if (method == FeedLocationMethod.Syndic8Search) {
					autoDiscover.SearchTerms = this.txtNewBySearchWords.Text;
					autoDiscover.OperationMessage = SR.GUIStatusWaitMessageDetectingFeedsWithKeywords(this.txtNewBySearchWords.Text);
				} else {
					autoDiscover.OperationMessage = SR.GUIStatusWaitMessageDetectingFeeds(this.txtNewBySearchWords.Text);
				}
				
				if (this.textUser.Text.Trim().Length > 0) {
					autoDiscover.Credentials = FeedSource.CreateCredentialsFrom(this.textUser.Text.Trim(), this.textPassword.Text.Trim());
				}
				
				SetWizardTaskInfo(taskIndex, WizardValidationTask.InProgress, autoDiscover.OperationMessage);

				if (DialogResult.OK != autoDiscover.Start( this )) {
					SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, SR.WizardValidationTask_SearchCanceledMessage);
					this.pageValidateUrl.AllowMovePrevious = true;
					return false;	// cancelled
				}
                    
				if (!autoDiscover.OperationSucceeds) {
					SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, autoDiscover.OperationException != null ? 
						autoDiscover.OperationException.Message:  SR.WizardValidationTask_SearchFailedMessage);
					this.ReWireCredentialsStep(autoDiscover.OperationException is ResourceAuthorizationException);
					this.pageValidateUrl.AllowMovePrevious = true;
					return false;
				}

				Hashtable feedUrls = autoDiscover.DiscoveredFeeds;
                    
				if(feedUrls.Count == 0) {
					SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, SR.GUIStatusInfoMessageNoFeedsFound);
					this.pageValidateUrl.AllowMovePrevious = true;
					if (method == FeedLocationMethod.Syndic8Search) {
						this.pageValidateUrl.ProceedText = SR.WizardLocateFeedBySearchTask_NoFeedFoundMessage;
					} else {
						this.pageValidateUrl.ProceedText = SR.WizardLocateFeedByUrlTask_NoFeedFoundMessage;
					}
					return false; 
				}

				SetWizardTaskInfo(taskIndex, WizardValidationTask.Success, SR.WizardValidationTask_Success_xFeedsFound_Message(feedUrls.Count));
				PopulateFoundFeeds(feedUrls);
				
				return true;
			
			} catch(Exception e) {
				SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, e.Message);
				this.pageValidateUrl.AllowMovePrevious = true;
				return false;
			}
		}

		private bool TaskBeginGetFeedTitle(int taskIndex, string url) {
			try { 
				
				this.feedInfo = null;
				url = this.ValidateFeedUri(pageValidateUrl, url);
				pageValidateUrl.AllowMoveNext = false;
				if (url == null) {
					SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, SR.WizardValidationTask_InvalidFeedMessage);	
					this.pageValidateUrl.AllowMovePrevious = true;
					return false;	// cancelled
				}

				PrefetchFeedThreadHandler fetchHandler = new PrefetchFeedThreadHandler(url, coreApplication.Proxy);
				if (this.textUser.Text.Trim().Length > 0) {
					fetchHandler.Credentials = FeedSource.CreateCredentialsFrom(this.textUser.Text.Trim(), this.textPassword.Text.Trim());
				}
				string message = SR.GUIStatusWaitMessagePrefetchFeed(url);
				SetWizardTaskInfo(taskIndex, WizardValidationTask.InProgress, message);
				DialogResult result = fetchHandler.Start(this, message);

				if (result != DialogResult.OK) {
					SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, SR.WizardValidationTask_PrefetchCanceledMessage);	
					this.pageValidateUrl.AllowMovePrevious = true;
					return false;	// cancelled
				}

				if (!fetchHandler.OperationSucceeds) {
					SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, 
						fetchHandler.OperationException != null ? fetchHandler.OperationException.Message :  SR.WizardValidationTask_PrefetchFailedMessage);
					this.pageValidateUrl.AllowMovePrevious = true;
					return false;
				}

				if(fetchHandler.DiscoveredDetails != null) {
					this.feedInfo = fetchHandler.FeedInfo;					
					this.FeedTitle = fetchHandler.DiscoveredDetails.Title;
				}


				SetWizardTaskInfo(taskIndex, WizardValidationTask.Success, SR.WizardValidationTask_SuccessMessage);
				
				return true;
			
			} catch(Exception e) {
				SetWizardTaskInfo(taskIndex, WizardValidationTask.Failed, e.Message);
				this.pageValidateUrl.AllowMovePrevious = true;
				return false;
			}
		}

		private string StripAndDecode(string s) {
			if (s == null)
				return s;
			string t = HtmlHelper.StripAnyTags(s);
			if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
				t = HtmlHelper.HtmlDecode(t);
			}
			return t.Trim();
		}
		
		private void PopulateFoundFeeds(Hashtable rssFeeds) {
			ListViewItem lv = null;
			this.listFeeds.Items.Clear();

			foreach(object feedUrl in rssFeeds.Keys) {
				object feedinfo = rssFeeds[feedUrl];
				Type t = feedinfo.GetType();
				if (t.IsArray) {
					string[] subItems = (string[])feedinfo;
					if (subItems.Length != this.listFeeds.Columns.Count) {	// additional fields
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedTitle, listFeeds.Width / 2, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedDesc, listFeeds.Width / 2, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionSiteUrl, 80, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedUrl, 80, HorizontalAlignment.Left);
					}
					lv = new ListViewItem(subItems);
				} else { // obsolete, not used anymore
					if (2 != listFeeds.Columns.Count) {	// additional fields
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedTitle, 80, HorizontalAlignment.Left);
						listFeeds.Columns.Add(SR.ListviewColumnCaptionFeedUrl, 80, HorizontalAlignment.Left);
					}
					lv = new ListViewItem(new string[]{SR.AutoDiscoveredDefaultTitle, (string)feedUrl});
				}
				lv.Tag = feedUrl;
				listFeeds.Items.Add(lv);
			}

			if (listFeeds.Items.Count > 0) {
				listFeeds.Items[0].Selected = true;
			}

		}

		private bool SearchEngineReturnSingleFeed {
			get {
				if (cboNewBySearchEngines.Items.Count > 0)
					return cboNewBySearchEngines.SelectedIndex != 0;
				return false;
			}	
		}

		private string GetSearchEngineSearchUrl(string searchTerm) {
			if (SearchEngineReturnSingleFeed) {
				string engineTitle = cboNewBySearchEngines.Text;
				foreach (ISearchEngine engine in coreApplication.WebSearchEngines) {
					if (engineTitle.Equals(engine.Title)) {
						return String.Format(new UrlFormatter(), engine.SearchLink, searchTerm);
					}
				}
			}
			return searchTerm;
		}

		private void PopulateSearchEngines(IList engines, string selectedEngine) {
			cboNewBySearchEngines.Items.Clear();
			int selIndex = -1;
			
			string syndic8Title = SR.Syndic8FeedSearchTitle;
			cboNewBySearchEngines.Items.Add(syndic8Title);
			if (syndic8Title.Equals(selectedEngine))
				selIndex = 0;

			foreach (ISearchEngine engine in engines) {
				if (engine.ReturnRssResult) {
					int index = cboNewBySearchEngines.Items.Add(engine.Title);
					if (engine.Title.Equals(selectedEngine))
						selIndex = index;
				}
			}

			if (selIndex >= 0)
				cboNewBySearchEngines.SelectedIndex = selIndex;
			else if (cboNewBySearchEngines.Items.Count > 0)
				cboNewBySearchEngines.SelectedIndex = 0;
		}
		
		private void PopulateStylesheets(IList stylesheets, string selectedStylesheet) {
			comboFormatters.Items.Clear();
			int selIndex = -1;
			foreach (string formatter in stylesheets) {
				int index = comboFormatters.Items.Add(formatter);
				if (formatter.Equals(selectedStylesheet))
					selIndex = index;
			}
			if (selIndex >= 0)
				comboFormatters.SelectedIndex = selIndex;
			else if (comboFormatters.Items.Count > 0)
				comboFormatters.SelectedIndex = 0;
		}
		
		private void PopulateNntpServerDefinitions(IDictionary<string, INntpServerDefinition> definitions, string selectedServer) {
			cboNNTPServer.Items.Clear();
			int selIndex = -1;
			foreach (INntpServerDefinition sd in definitions.Values) {
				int index = cboNNTPServer.Items.Add(sd.Name);
				if (sd.Name.Equals(selectedServer))
					selIndex = index;
			}
			if (selIndex >= 0)
				cboNNTPServer.SelectedIndex = selIndex;
			else if (cboNNTPServer.Items.Count > 0)
				cboNNTPServer.SelectedIndex = 0;
		}

		private void PopulateNntpServerGroups(bool forceReloadFromServer) {
			pageNewByNNTPGroup.AllowMoveNext = false;
			lstNNTPGroups.Items.Clear();
			string nntpServer = cboNNTPServer.Text;
			if (string.IsNullOrEmpty(nntpServer)) {
				return;
			}
			IList groups = coreApplication.GetNntpNewsGroups(nntpServer, forceReloadFromServer);
			if (groups != null && groups.Count > 0) {
				object[] g = new object[groups.Count];
				groups.CopyTo(g, 0);
				lstNNTPGroups.Items.AddRange(g);
			}
		}

		private string ValidateFeedUri(WizardPage page, string url) {
			Exception ex = null;
			string validUrl = this.ValidateFeedUri(url, out ex);
			bool valid = ( ! string.IsNullOrEmpty(validUrl) && ex == null);
			page.AllowMoveNext = valid;
			if (valid) {
				page.ProceedText = String.Empty;
			} else if (ex != null) {
				page.ProceedText = ex.Message;
				return null;
			}
			return validUrl;
		}

		/// <summary>
		/// Returns null, if invalid
		/// </summary>
		/// <param name="url"></param>
		/// <param name="invalidUriException"></param>
		/// <returns>valid Url or null</returns>
		private string ValidateFeedUri(string url, out Exception invalidUriException) {
			invalidUriException = null;

			if (StringHelper.EmptyTrimOrNull(url))
				return String.Empty;

			// canonicalize the provided url:
			string newUrl = HtmlHelper.HtmlDecode(url);
			
			//some weird URLs have newlines. 
			// We remove them before create the Uri, so we do not fail on THAT error there	
			newUrl = newUrl.Replace(Environment.NewLine, String.Empty); 
			
			//handle the common case of feed URI not beginning with HTTP 
			try{ 
				Uri reqUri = new Uri(newUrl);
				newUrl     = reqUri.CanonicalizedUri();
			}catch(UriFormatException){

				if(!url.ToLower().StartsWith("http://")){
					try {		
						Uri reqUri = new Uri("http://" + newUrl); 
						newUrl     = reqUri.CanonicalizedUri();
					} catch (UriFormatException ex) {
						invalidUriException = ex;
						return null;
					}
				}
				
			}

    
            if (coreApplication.ContainsFeed(newUrl))
            {
				string category, title, link;
                coreApplication.TryGetFeedDetails(newUrl, out category, out title, out link);
				invalidUriException = new InvalidOperationException( 
					SR.GUIFieldLinkRedundantInfo( 
					((category ?? String.Empty) + FeedSource.CategorySeparator) + title, link ));
				return null; 
			}

			return newUrl;
		}

		/// <summary>
		/// Gets a value indicating whether multiple feeds available to subscribe or not.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if multiple feeds to subscribe; otherwise, <c>false</c>.
		/// </value>
		public bool MultipleFeedsToSubscribe {
			get { 
				if ((this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPDirect ||
					this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroup ||
					this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroupDirect) &&
					this.lstNNTPGroups.SelectedItems.Count > 1)
					return true;
				if ((this.wizardMode == AddSubscriptionWizardMode.SubscribeURL ||
					this.wizardMode == AddSubscriptionWizardMode.SubscribeURLDirect ) &&
					this.listFeeds.SelectedItems.Count > 1)
					return true;
				return false;
			}
		}
		
		/// <summary>
		/// Gets the count of multiple feeds to subscribe.
		/// </summary>
		/// <value>int</value>
		public int MultipleFeedsToSubscribeCount {
			get {
				if ((this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPDirect ||
					this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroup ||
					this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroupDirect))
					return this.lstNNTPGroups.SelectedItems.Count;
				if ((this.wizardMode == AddSubscriptionWizardMode.SubscribeURL ||
					this.wizardMode == AddSubscriptionWizardMode.SubscribeURLDirect ))
					return this.listFeeds.SelectedItems.Count;
				return 0;
			}	
		}

		/// <summary>
		/// Gets the indexed feed URL.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public string FeedUrls(int index) {
			
			if (this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPDirect ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroup ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroupDirect) {
				
				if (this.lstNNTPGroups.SelectedItems.Count > 0) {
					string newsServer = String.Empty;
					INntpServerDefinition sd = (INntpServerDefinition) this.coreApplication.NntpServerDefinitions[cboNNTPServer.Text];

					if(sd != null){
						newsServer = sd.Server; 						
					}	
					return "nntp://" + newsServer + "/" + this.lstNNTPGroups.SelectedItems[index].ToString(); 
				
				} else {
					return this.FeedUrl;
				}

			} else if (this.wizardMode == AddSubscriptionWizardMode.SubscribeURL ||
					this.wizardMode == AddSubscriptionWizardMode.SubscribeURLDirect ) {
				
				if (this.listFeeds.SelectedItems.Count > 0)
					return (string)this.listFeeds.SelectedItems[index].Tag;
				else
					return this.FeedUrl;

			} else {

				return this.FeedUrl;

			}
		
		} 
		
		/// <summary>
		/// Gets the indexed feed title.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public string FeedTitles(int index) {
			
			if (this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPDirect ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroup ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroupDirect) {
				
				if (this.lstNNTPGroups.SelectedItems.Count > 1)
					return this.lstNNTPGroups.SelectedItems[index].ToString(); 
				else
					return this.FeedTitle;

			} else if (this.wizardMode == AddSubscriptionWizardMode.SubscribeURL ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeURLDirect ) {
				
				if (this.listFeeds.SelectedItems.Count > 1)
					return this.listFeeds.SelectedItems[index].SubItems[0].Text;
				else
					return this.FeedTitle;

			} else {
				return this.FeedTitle;
			}
		}

		/// <summary>
		/// Gets or sets the feed URL.
		/// </summary>
		/// <value>The feed URL.</value>
		public string FeedUrl
		{
			get { return String.Empty; } 
			set { ; }
		} 

		/// <summary>
		/// Gets or sets the feed category.
		/// </summary>
		/// <value>The feed category.</value>
		public string FeedCategory
		{
			get {
				string c = this.cboFeedCategory.Text.Trim();
				if (string.IsNullOrEmpty(c))
					return null;
				if (coreApplication.DefaultCategory.Equals(c))
					return null;
				return c;
			} 
			set
			{
				if (!string.IsNullOrEmpty(value))
					this.cboFeedCategory.Text = value;
			}
		} 

		/// <summary>
		/// Gets or sets the feed title.
		/// </summary>
		/// <value>The feed title.</value>
		public string FeedTitle {
			get { return this.txtFeedTitle.Text.Trim(); } 
			set { this.txtFeedTitle.Text = StripAndDecode(value); }
		} 
		/// <summary>
		/// Gets or sets the search terms.
		/// </summary>
		/// <value>The search terms.</value>
		public string SearchTerms {
			get { return this.txtNewBySearchWords.Text.Trim(); } 
			set { this.txtNewBySearchWords.Text = value; }
		} 
		/// <summary>
		/// Gets the feed credential user.
		/// </summary>
		/// <value>The feed credential user.</value>
		public string FeedCredentialUser {
			get { return this.textUser.Text.Trim(); } 
		} 
		/// <summary>
		/// Gets the feed credential PWD.
		/// </summary>
		/// <value>The feed credential PWD.</value>
		public string FeedCredentialPwd {
			get { return this.textPassword.Text.Trim(); } 
		} 
		/// <summary>
		/// Gets the feed stylesheet.
		/// </summary>
		/// <value>The feed stylesheet.</value>
		public string FeedStylesheet {
			get {
				if (this.checkUseCustomFormatter.Checked) {
					if (string.IsNullOrEmpty(this.comboFormatters.Text))
						return null;
					return this.comboFormatters.Text.Trim();
				}
				return null;
			} 
		} 

		internal FeedInfo FeedInfo {
			get {	return this.feedInfo;	}	
		}

		/// <summary>
		/// Gets or sets the max item age.
		/// </summary>
		/// <value>The max item age.</value>
		public TimeSpan MaxItemAge 
		{
			get { return Utils.MaxItemAgeFromIndex(this.comboMaxItemAge.SelectedIndex); }
			set { this.comboMaxItemAge.SelectedIndex = Utils.MaxItemAgeToIndex(value);	}
		}

        /// <summary>
        /// Gets the path to the file or URL from which to import feeds when importing from OPML.
        /// </summary>
        public string FeedsUrlOrFile { get { return textUrlOrFile.Text; } }

		/// <summary>
		/// Gets a value indicating whether [alert enabled].
		/// </summary>
		/// <value><c>true</c> if [alert enabled]; otherwise, <c>false</c>.</value>
		public bool AlertEnabled {
			get { return this.checkEnableAlertOnNewItems.Checked; }	
		}
		/// <summary>
		/// Gets a value indicating whether to mark items read on exit.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [mark items read on exit]; otherwise, <c>false</c>.
		/// </value>
		public bool MarkItemsReadOnExit {
			get { return this.checkMarkItemsReadOnExiting.Checked; }	
		}

		/// <summary>
		/// Gets the refresh rate in minutes.
		/// </summary>
		/// <value>The refresh rate.</value>
		public int RefreshRate {
			get {
				// should be yet validated. But for safety:
				try { 

					if((!string.IsNullOrEmpty(this.cboUpdateFrequency.Text.Trim()))) {
						Int32 intIn = System.Int32.Parse(this.cboUpdateFrequency.Text.Trim());
						if (intIn <= 0) {
							return 0;
						} else {
							return intIn;
						}
					} else {
						return 0;
					}

				}
				catch(FormatException) {
					if (coreApplication.CurrentGlobalRefreshRate > 0)
						return coreApplication.CurrentGlobalRefreshRate;
					else
						return RssBanditApplication.DefaultGlobalRefreshRateMinutes;
				}
				catch(OverflowException) {
					return Int16.MaxValue;
				}
			}
		}

		// -------------------------------------------------------------------
		// Events 
		// -------------------------------------------------------------------
		
		private void OnTimerIncreaseProgress_Tick(object sender, System.EventArgs e)
		{
			
			if ( waitHandle.WaitOne(0,false) ) {	// gets signal
				timerIncreaseProgress.Stop();
				pbar.Value = pbar.Maximum;
				this.operationResult = DialogResult.OK;
				return;
			}

			if (this.timeout != TimeSpan.Zero) {
				this.timeCounter--;
				if (this.timeCounter <= 0) {
					timerIncreaseProgress.Stop();
					pbar.Value = pbar.Maximum;
					waitHandle.Set();		// signal done (timeout)
					this.operationTimeout = true;
					this.operationResult = DialogResult.Abort;
					return;
				}
			}
            
			// update progress bar info
			if ( pbar.Value + pbar.Step >= pbar.Maximum ) {
				pbar.Value = 0;
			} else {
				pbar.Increment(pbar.Step);
			}

		}

		private void OnPageValidation_BeforeDisplay(object sender, System.EventArgs e) {
			ResetValidationPage();
		}

		private void OnPageValidation_AfterDisplay(object sender, System.EventArgs e)
		{
			pageValidateUrl.AllowCancel = true;
			//pageValidateUrl.AllowMoveNext = false;
			this.timerStartValidation.Enabled = true;

		}

		private void OnWizardCancel(object sender, System.EventArgs e) {
			if (wizard.SelectedPage != pageValidateUrl) {
				this.DialogResult = DialogResult.Cancel;
				Close();
			} else if (wizard.SelectedPage.AllowMovePrevious) {
				this.DialogResult = DialogResult.Cancel;
				Close();
			} else {
				this.operationResult = DialogResult.Cancel;
				this.DialogResult = System.Windows.Forms.DialogResult.None;
			}
		}

		private void OnWizardFinish(object sender, System.EventArgs e) {
			
			this.ProcessFeedUrl();
			this.DialogResult = DialogResult.OK;
			Close();
		}

		

		private void OnFoundFeedsListSelectedIndexChanged(object sender, System.EventArgs e)
		{
			pageFoundMultipleFeeds.AllowMoveNext = listFeeds.SelectedItems.Count > 0;
		}

		private void OnListFoundFeeds_DoubleClick(object sender, System.EventArgs e)
		{
			if (pageFoundMultipleFeeds.AllowMoveNext)
				wizard.GoNext();
		}

		private void OnPageWelcome_BeforeDisplay(object sender, System.EventArgs e) {
			
			this.radioNewByURL.Checked = true;

			// get the check value: display this page?
			if (false == this.chkDisplayWelcome.Checked) {
				// move to next wizard page 
				wizard.GoNext();
			}
		}

		private void OnWindowSerializerLoadStateEvent(object sender, Genghis.Preferences preferences) {
			this.chkDisplayWelcome.Checked = preferences.GetBoolean(this.Name + ".DisplayWelcome", true);
			if (string.IsNullOrEmpty(this.FeedCategory) ||
				coreApplication.DefaultCategory.Equals(this.FeedCategory)) 
			{
				string recentCategory = preferences.GetString(this.Name + ".LastFeedCategory", this.FeedCategory);
				foreach(string category in coreApplication.GetCategories()) {
					if (String.Compare(category, recentCategory, false) == 0) {
						this.FeedCategory = recentCategory;
						break;
					}
				}
			}
			// event does not get fired:
			OnPageWelcome_BeforeDisplay(this, EventArgs.Empty);
		}

		private void OnWindowSerializerSaveStateEvent(object sender, Genghis.Preferences preferences) {
			preferences.SetProperty(this.Name + ".DisplayWelcome", this.chkDisplayWelcome.Checked);
			if (!string.IsNullOrEmpty(this.FeedCategory) && 
				!this.FeedCategory.Equals(coreApplication.DefaultCategory))
			{
				preferences.SetProperty(this.Name + ".LastFeedCategory", this.FeedCategory);
			} else {
				preferences.SetProperty(this.Name + ".LastFeedCategory", null);
			}
		}

		private void OnInternetServiceInternetConnectionStateChange(object sender, InternetConnectionStateChangeEventArgs e) {
			bool internetConnected = (e.NewState & INetState.Connected) > 0 && (e.NewState & INetState.Online) > 0;
		}

		private void OnImmediateFinish_Click(object sender, System.EventArgs e)
		{			
			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void OnFinishPage_BeforeDisplay(object sender, System.EventArgs e)
		{
			this._btnImmediateFinish.Visible = false;
		}

		private void OnFinishPage_BeforeMoveBack(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this._btnImmediateFinish.Visible = true;
		}

		private void pageFeedCredentials_BeforeDisplay(object sender, System.EventArgs e) {
			//
		}

		private void btnManageSearchEngines_Click(object sender, System.EventArgs e) {
			coreApplication.ShowOptions(OptionDialogSection.WebSearch, this, new EventHandler(this.OnPreferenceDialogOptionChanged));
		}

		private void OnPreferenceDialogOptionChanged(object sender, EventArgs e) {
			// refresh widgets that depends on Option settings
			// search engine list:
			this.PopulateSearchEngines(coreApplication.WebSearchEngines, cboNewBySearchEngines.Text);
		}

		private void OnRadioHowToSubscribeCheckedChanged(object sender, System.EventArgs e) {
			if (sender == radioNewByURL)
				WireStepsForMode(AddSubscriptionWizardMode.SubscribeURL);
			else if (sender == radioNewByTopicSearch)
				WireStepsForMode(AddSubscriptionWizardMode.SubscribeSearch);
			else if (sender == radioNewByNNTPGroup)
				WireStepsForMode(AddSubscriptionWizardMode.SubscribeNNTPGroup);
		}

		private void OnAutodiscoverVerifyCheckedChanged(object sender, System.EventArgs e) {
			
		}

		private void OnMultipleFeedsBeforeMoveNext(object sender, System.ComponentModel.CancelEventArgs e) {
//			this.FeedUrl = (string)listFeeds.SelectedItems[0].Tag;
			if (this.MultipleFeedsToSubscribe)
				this.FeedTitle = String.Empty;
			else
				this.FeedTitle = listFeeds.SelectedItems[0].SubItems[0].Text;
		}

	

		private void OnPageTitleCategoryBeforeMoveBack(object sender, System.ComponentModel.CancelEventArgs e) {
			this._btnImmediateFinish.Visible = false;
		}

		private void OnPageTitleCategoryBeforeDisplay(object sender, System.EventArgs e) {
			this._btnImmediateFinish.Visible = true;
			this.txtFeedTitle.Enabled = !this.MultipleFeedsToSubscribe;

			if (this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPDirect ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroup ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroupDirect ||
				this.credentialsStepReWired) 
			{
				if (this.credentialsStepReWired)
					this.pageTitleCategory.PreviousPage = this.pageFeedCredentials;

				this.pageTitleCategory.NextPage = this.pageFeedItemControl;
				this.pageFeedItemControl.PreviousPage = this.pageTitleCategory;
			} 
			else 
			{
				this.pageTitleCategory.NextPage = this.pageFeedCredentials;
				this.pageFeedItemControl.PreviousPage = this.pageFeedCredentials;
			}

			this.pageTitleCategory.AllowMoveNext = this._btnImmediateFinish.Enabled = 
				(txtFeedTitle.Text.Length > 0 || !txtFeedTitle.Enabled);

		}

		private void OnNewFeedTitleTextChanged(object sender, System.EventArgs e) {
			this.pageTitleCategory.AllowMoveNext = this._btnImmediateFinish.Enabled = 
				(txtFeedTitle.Text.Length > 0 || !txtFeedTitle.Enabled);
		}

		private void OnPageHowToSelectionAfterDisplay(object sender, System.EventArgs e) {
			this.WireStepsForMode(AddSubscriptionWizardMode.SubscribeURL);
		}

		private void OnPageNewNNTPGroupAfterDisplay(object sender, System.EventArgs e) {
			this.feedInfo = null;
			pageNewByNNTPGroup.AllowMoveNext = false;
			PopulateNntpServerDefinitions((IDictionary<string, INntpServerDefinition>) coreApplication.NntpServerDefinitions, null);
		}

        private void OnPageNewURLAfterDisplay(object sender, System.EventArgs e)
        {
            this.feedInfo = null;
        }

		private void OnPageNewSearchAfterDisplay(object sender, System.EventArgs e) {
			this.feedInfo = null;
			this.PopulateSearchEngines(coreApplication.WebSearchEngines, cboNewBySearchEngines.Text);
		}

		private void btnManageNNTPServer_Click(object sender, System.EventArgs e) {
			coreApplication.ShowNntpServerManagementDialog(this, new EventHandler(this.OnManagedNNTPServersChange));
		}
		
		private void OnManagedNNTPServersChange(object sender, System.EventArgs e) {
			this.PopulateNntpServerDefinitions(coreApplication.NntpServerDefinitions, cboNNTPServer.Text);
		}

		private void OnNNTPServerSelectedValueChanged(object sender, System.EventArgs e) {
			this.PopulateNntpServerGroups(false);
		}

		private void OnNNTPGroupsListSelectedValueChanged(object sender, System.EventArgs e) {
			pageNewByNNTPGroup.AllowMoveNext = lstNNTPGroups.SelectedItems.Count > 0;
			if (lstNNTPGroups.SelectedItems.Count == 1) {
				txtFeedTitle.Text = lstNNTPGroups.SelectedItems[0].ToString();
			} else if(lstNNTPGroups.SelectedItems.Count > 1) {
				txtFeedTitle.Text = string.Empty;
			}
		}

		private void OnNNTPGroupsDoubleClick(object sender, System.EventArgs e) {
			if (lstNNTPGroups.SelectedItems.Count > 0) {
				pageNewByNNTPGroup.AllowMoveNext = true;
				if (lstNNTPGroups.SelectedItems.Count == 1) {
					txtFeedTitle.Text = lstNNTPGroups.SelectedItems[0].ToString();
				} else {
					txtFeedTitle.Text = string.Empty;
				}
				wizard.GoNext();
			}
		}

		private void OnPageFeedItemDisplayAfterDisplay(object sender, System.EventArgs e) {
			PopulateStylesheets(coreApplication.GetItemFormatterStylesheets(), comboFormatters.Text );
		}

		private void OnAnyLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			LinkLabel o = sender as LinkLabel;
			if (o != null) {
				ICoreApplication coreApp = (ICoreApplication)this.serviceProvider.GetService(typeof(ICoreApplication));
				if (coreApp != null) {
					string url = (string)o.Tag;
					if (url == null)
						url = o.Text;
					coreApp.NavigateToUrlInExternalBrowser(url);
					o.Links[o.Links.IndexOf(e.Link)].Visited = true;	
				}
			}
		}
		
		private void OnReloadNntpGroupList(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e) {
			try {
				this.PopulateNntpServerGroups(true);
			} catch (Exception ex) {
				RssBanditApplication.PublishException(ex, false);
			}
		}

		/// <summary>
		/// Ensures the Feed URL is valid and is in the proper form if an NNTP URL
		/// </summary>		
		private void ProcessFeedUrl(){
		
			Exception invalidUriException; 

			if(radioNewByNNTPGroup.Checked || this.MultipleFeedsToSubscribe ||
				(this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPDirect) ||
				(this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroup) ){
				
				// take/set the first:
				this.FeedUrl = this.FeedUrls(0);
			} 

			this.FeedUrl = ValidateFeedUri(this.FeedUrl, out invalidUriException);

			if(!this.MultipleFeedsToSubscribe && invalidUriException != null) //this shouldn't happen
				throw invalidUriException;		
		}

		private void OnTextNewByUrlValidating(object sender, System.ComponentModel.CancelEventArgs e) {
			
		}

		private void OnTimerStartValidation(object sender, System.EventArgs e) {
			
			this.timerStartValidation.Enabled = false;
			
			if (this.wizardMode == AddSubscriptionWizardMode.SubscribeURL || this.wizardMode == AddSubscriptionWizardMode.SubscribeURLDirect) {
				pageValidateUrl.NextPage = pageTitleCategory;
				pageValidateUrl.PreviousPage = pageStartImport;
				SetWizardTaskInfo(0, WizardValidationTask.InProgress, SR.WizardValidationTask_VerifyingUrlMessage);
				SetWizardTaskInfo(1, WizardValidationTask.Pending, SR.WizardValidationTask_GetWebFeedTitleMessage);
				
				// MUST BE YET VALIDATED:
				pageValidateUrl.AllowMoveNext = false;

				bool success = TaskBeginSearchFeedLocations(0, FeedLocationMethod.AutoDiscoverUrl);
				Application.DoEvents();
				if (success) {
					
					if (this.listFeeds.Items.Count > 1) {
						pageValidateUrl.NextPage = pageFoundMultipleFeeds;
						pageFoundMultipleFeeds.PreviousPage = pageStartImport;
						pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
						this.FeedTitle = String.Empty;
					} else {
						pageValidateUrl.NextPage = pageTitleCategory;
						if (!this.credentialsStepReWired)
							pageTitleCategory.PreviousPage = pageStartImport;
						this.FeedTitle = this.listFeeds.Items[0].SubItems[0].Text;
					}

					Application.DoEvents();
					Thread.Sleep(1500);
					this.pageValidateUrl.AllowMoveNext = true;
					if (this.operationResult != DialogResult.Cancel)
						wizard.GoNext();
				}
					
			} else if (this.wizardMode == AddSubscriptionWizardMode.SubscribeSearch || this.wizardMode == AddSubscriptionWizardMode.SubscribeSearchDirect) {

				if (this.SearchEngineReturnSingleFeed) {

					pageValidateUrl.NextPage = pageTitleCategory;
					pageTitleCategory.PreviousPage = pageNewBySearchTopic;
					pageValidateUrl.PreviousPage = pageNewBySearchTopic;
					SetWizardTaskInfo(0, WizardValidationTask.InProgress, SR.WizardValidationTask_VerifyingUrlMessage);
					SetWizardTaskInfo(1, WizardValidationTask.Pending, SR.WizardValidationTask_GetWebFeedTitleMessage);
				
					// MUST BE YET VALIDATED:
					pageValidateUrl.AllowMoveNext = false;

					bool success = TaskBeginSearchFeedLocations(0, FeedLocationMethod.AutoDiscoverUrl);
					Application.DoEvents();
					if (success) {
						success = TaskBeginGetFeedTitle(1, (string)this.listFeeds.Items[0].Tag);
						Application.DoEvents();
						if (success) {
							Application.DoEvents();
							Thread.Sleep(1500);
							this.pageValidateUrl.AllowMoveNext = true;
							if (this.operationResult != DialogResult.Cancel)
								wizard.GoNext();
						}
					}

				} else {
					// syndic8 service

					pageValidateUrl.NextPage = pageFoundMultipleFeeds;
					pageValidateUrl.PreviousPage = pageNewBySearchTopic;
					pageFoundMultipleFeeds.PreviousPage = pageNewBySearchTopic;
					pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;

					SetWizardTaskInfo(0, WizardValidationTask.InProgress, SR.WizardValidationTask_SearchingMessage);
					SetWizardTaskInfo(1, WizardValidationTask.None, String.Empty);

					bool success = TaskBeginSearchFeedLocations(0, FeedLocationMethod.Syndic8Search);
					Application.DoEvents();
					if (success) {
						Thread.Sleep(1500);
						if (this.operationResult != DialogResult.Cancel)
							wizard.GoNext();
					}
				}
			}

		}

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "OPML files (*.opml)|*.opml|OCS files (*.ocs)|*.ocs|XML files (*.xml)|*.xml|All files (*.*)|*.*";
            ofd.FilterIndex = 4;
            ofd.InitialDirectory = Environment.CurrentDirectory;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textUrlOrFile.Text = ofd.FileName;
            }
        }

        private void radioImportFromOpml_CheckedChanged(object sender, EventArgs e)
        {
            this.comboFeedSource.Enabled = false;
            this.textUrlOrFile.Enabled = this.comboCategory.Enabled = true;
        }

        private void radioImportFromFeedSource_CheckedChanged(object sender, EventArgs e)
        {
            this.textUrlOrFile.Enabled = this.comboCategory.Enabled = false;
            this.comboFeedSource.Enabled = true; 
        }

	}

}

#region CVS Version Log
/*
 * $Log: AddSubscriptionWizard.cs,v $
 * Revision 1.42  2007/08/02 13:37:40  t_rendelmann
 * added support to NewsSubscriptionWizard for rewire the credential step on authentication exceptions
 *
 * Revision 1.41  2007/01/21 12:54:09  t_rendelmann
 * fixed: HtmlDecoding/Mnemonics issue caused by feed titles using "&" and the like in discovered feeds dropdown and subscription wizard; as a test Url you can use e.g.  http://www.codeplex.com/entlib/Release/ProjectReleases.aspx?ReleaseId=1649
 *
 * Revision 1.40  2006/11/24 12:18:45  t_rendelmann
 * small fix: recent used category in the subscription wizard was not validated against the categories
 *
 * Revision 1.39  2006/11/11 14:42:42  t_rendelmann
 * added: DialogBase base Form to be able to inherit simple OK/Cancel dialogs;
 * added new PodcastOptionsDialog (inherits DialogBase)
 *
 * Revision 1.38  2006/10/17 15:43:36  t_rendelmann
 * fixed: HTML entities in feed (-item) urls not decoded (https://sourceforge.net/tracker/index.php?func=detail&aid=1564959&group_id=96589&atid=615248)
 *
 */
#endregion
