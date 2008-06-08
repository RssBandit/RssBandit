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
	/// SynchronizeFeedsWizard summerize and handles 
	/// all kind of subscriptions now:
	///   By URL (direct, and autodiscovered)
	///   By Search/Topic
	///   NNTP Groups
	///   Direct NNTP Group
	/// </summary>
	internal class AddSubscriptionWizard : System.Windows.Forms.Form, IServiceProvider, IWaitDialog
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

		private System.Windows.Forms.ProgressBar pbar;
		private System.Windows.Forms.Timer timerIncreaseProgress;
		private System.Windows.Forms.Panel pnlCompleting;
		private System.Windows.Forms.Panel pnlCancelling;
		private Divelements.WizardFramework.Wizard wizard;
		private Divelements.WizardFramework.IntroductionPage pageWelcome;
		private Divelements.WizardFramework.WizardPage pageValidateUrl;
		private System.Windows.Forms.Label lblValidationTask1;
		private System.Windows.Forms.Label lblValidationTaskImage1;
		private System.Windows.Forms.Label lblValidationTaskImage2;
		private System.Windows.Forms.Label lblValidationTask2;
		private System.Windows.Forms.CheckBox chkDisplayWelcome;
		private Divelements.WizardFramework.WizardPage pageFoundMultipleFeeds;
		private Divelements.WizardFramework.FinishPage finishPage;
		private System.Windows.Forms.Label lblFeedTitle;
		private System.Windows.Forms.TextBox txtFeedTitle;
		private System.Windows.Forms.Label lblPageTitleCredentialsIntro;
		private System.Windows.Forms.Button _btnImmediateFinish;
		private Divelements.WizardFramework.WizardPage pageFeedItemControl;
		private System.Windows.Forms.Label lblFeedItemControlIntro;
		private Divelements.WizardFramework.WizardPage pageFeedItemDisplay;
		private System.Windows.Forms.Label lblFeedItemDisplayIntro;
		private System.Windows.Forms.ComboBox comboMaxItemAge;
		private System.Windows.Forms.Label lblFormatterStylesheet;
		private System.Windows.Forms.ComboBox comboFormatters;
		private System.Windows.Forms.ComboBox cboUpdateFrequency;
		private System.Windows.Forms.Label lblCompletionMessage;
		private System.Windows.Forms.Label lblRemoveItemsOlderThan;
		private System.Windows.Forms.Label lblMinutes;
		private System.Windows.Forms.Label lblUpdateFrequency;
		private System.Windows.Forms.Label lblMultipleFeedsFoundHint1;
		private System.Windows.Forms.Label lblMultipleFeedsFoundHint2;
		private System.Windows.Forms.Label lblMultipleFeedsFound;
		private Divelements.WizardFramework.InformationBox lblWelcomeInfoBox;
		private System.Windows.Forms.Label lblWelcomeHelpMessage1;
		private System.Windows.Forms.Label lblWelcomeHelpMessage2;
		private Divelements.WizardFramework.WizardPage pageHowToSelection;
		private System.Windows.Forms.RadioButton radioNewByTopicSearch;
		private System.Windows.Forms.RadioButton radioNewByURL;
		private Divelements.WizardFramework.WizardPage pageNewByURL;
		private System.Windows.Forms.RadioButton radioNewByNNTPGroup;
		private System.Windows.Forms.LinkLabel lblNewByURLIntro;
		private System.Windows.Forms.CheckBox checkNewByURLValidate;
		private System.Windows.Forms.Label lblNewByURL;
		private System.Windows.Forms.TextBox txtNewByURL;
		private Divelements.WizardFramework.WizardPage pageNewBySearchTopic;
		private System.Windows.Forms.LinkLabel lblNewBySearchIntro;
		private System.Windows.Forms.TextBox txtNewBySearchWords;
		private System.Windows.Forms.Label lblNewBySearchWords;
		private System.Windows.Forms.Label lblNewBySearchEngines;
		private System.Windows.Forms.ComboBox cboNewBySearchEngines;
		private Divelements.WizardFramework.WizardPage pageTitleCategory;
		private System.Windows.Forms.Label lblFeedCategory;
		private System.Windows.Forms.ComboBox cboFeedCategory;
		private Divelements.WizardFramework.WizardPage pageFeedCredentials;
		private System.Windows.Forms.Label lblFeedCredentialsIntro;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.Label lblUsername;
		private Divelements.WizardFramework.WizardPage pageNewByNNTPGroup;
		private System.Windows.Forms.Button btnManageSearchEngines;
		private System.Windows.Forms.Button btnManageNNTPServer;
		private System.Windows.Forms.ComboBox cboNNTPServer;
		private System.Windows.Forms.ListBox lstNNTPGroups;
		private System.Windows.Forms.Label lblNNTPGroups;
		private System.Windows.Forms.Label lblNNTPServer;
		private System.Windows.Forms.CheckBox checkMarkItemsReadOnExiting;
		private System.Windows.Forms.CheckBox checkEnableAlertOnNewItems;
		private System.Windows.Forms.CheckBox checkUseCustomFormatter;
		private System.Windows.Forms.Label lblHowToSubscribeIntro;
		private System.Windows.Forms.ListView listFeeds;
		private System.Windows.Forms.TextBox textUser;
		private System.Windows.Forms.TextBox textPassword;
		private System.Windows.Forms.Label lblWaitStepIntro;
		private System.Windows.Forms.LinkLabel lblNewByNNTPGroupIntro;
		private System.Windows.Forms.LinkLabel lblReloadNntpListOfGroups;
		private System.Windows.Forms.LinkLabel lblAutodiscoverHelp;
		private System.Windows.Forms.PictureBox pictureHelpAutodiscover;
		private System.Windows.Forms.PictureBox pictureHelpSyndic8;
		private System.Windows.Forms.LinkLabel lblSyndic8Help;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.LinkLabel lblUsenetHelp;
		private System.Windows.Forms.Timer timerStartValidation;
		private System.Windows.Forms.LinkLabel linkLabelCanonicalUrl;
		private System.Windows.Forms.ToolTip toolTip;
		private Label lblFeedSources;
		private ComboBox cboFeedSources;
		private System.ComponentModel.IContainer components;

		#endregion

		#region ctor's
        private AddSubscriptionWizard()
        {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			// fix the link label(s) linkarea size to fit the whole text (in all translations):
			this.lblReloadNntpListOfGroups.LinkArea = new LinkArea(0,this.lblReloadNntpListOfGroups.Text.Length );
			this.lblAutodiscoverHelp.LinkArea = new LinkArea(0, this.lblAutodiscoverHelp.Text.Length);
			this.lblSyndic8Help.LinkArea = new LinkArea(0, this.lblSyndic8Help.Text.Length);
			this.lblUsenetHelp.LinkArea = new LinkArea(0, this.lblUsenetHelp.Text.Length);
		}

        public AddSubscriptionWizard(IServiceProvider provider, AddSubscriptionWizardMode mode)
            : this()
		{
			serviceProvider = provider;
			wizardMode = mode;

			// form location management:
			windowSerializer = new WindowSerializer(this);
			windowSerializer.SaveOnlyLocation = true;
			windowSerializer.SaveNoWindowState = true;

			windowSerializer.LoadStateEvent += OnWindowSerializerLoadStateEvent;
			windowSerializer.SaveStateEvent += OnWindowSerializerSaveStateEvent;

			// to get notified, if the inet connection state changes:
			internetService = (IInternetService)this.GetService(typeof(IInternetService));
			if (internetService != null) {
				internetService.InternetConnectionStateChange += OnInternetServiceInternetConnectionStateChange;
				checkNewByURLValidate.Enabled = radioNewByTopicSearch.Enabled = internetService.InternetAccessAllowed;
			}
			// to checkout the defaults to be used for the new feed:
			IUserPreferences preferencesService = (IUserPreferences)this.GetService(typeof(IUserPreferences));
			this.MaxItemAge = preferencesService.MaxItemAge;

			coreApplication = (ICoreApplication)this.GetService(typeof(ICoreApplication));
			this.cboUpdateFrequency.Text = String.Format("{0}", RssBanditApplication.DefaultGlobalRefreshRateMinutes);
			if (coreApplication.CurrentGlobalRefreshRate > 0)	// if not disabled refreshing
				this.cboUpdateFrequency.Text = String.Format("{0}", coreApplication.CurrentGlobalRefreshRate); 
			
			//initialize category combo box			
			foreach(string category in coreApplication.GetCategories())
			{
				if (!string.IsNullOrEmpty(category))
					this.cboFeedCategory.Items.Add(category); 
			}
			this.FeedCategory = coreApplication.DefaultCategory;

			// init feedsource combo:
			//TODO: that cast should not be there (extend interface!)
        	RssBanditApplication core = (RssBanditApplication)coreApplication;
			foreach (FeedSourceEntry entry in core.SourceManager.Sources)
				this.cboFeedSources.Items.Add(entry.Name);
        	this.FeedSourceName = this.cboFeedSources.Items[0] as string;	

			this.WireStepsForMode(this.wizardMode);
			this.wizard.SelectedPage = this.pageWelcome;
		}
		#endregion

		#region IWaitDialog Members

		public void Initialize(AutoResetEvent waitHandle, TimeSpan timeout, Icon dialogIcon) {
			this.waitHandle = waitHandle;
			this.timeout = timeout;
			this.timeCounter = 0;
			if (timeout != TimeSpan.Zero)
				this.timeCounter = (int)(timeout.TotalMilliseconds / this.timerIncreaseProgress.Interval);
			this.operationResult = DialogResult.None;
		}

		public DialogResult StartWaiting(IWin32Window owner, string waitMessage, bool allowCancel) {
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
					internetService.InternetConnectionStateChange -= OnInternetServiceInternetConnectionStateChange;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddSubscriptionWizard));
			this.wizard = new Divelements.WizardFramework.Wizard();
			this._btnImmediateFinish = new System.Windows.Forms.Button();
			this.pageNewByURL = new Divelements.WizardFramework.WizardPage();
			this.linkLabelCanonicalUrl = new System.Windows.Forms.LinkLabel();
			this.pictureHelpAutodiscover = new System.Windows.Forms.PictureBox();
			this.lblAutodiscoverHelp = new System.Windows.Forms.LinkLabel();
			this.checkNewByURLValidate = new System.Windows.Forms.CheckBox();
			this.lblNewByURLIntro = new System.Windows.Forms.LinkLabel();
			this.txtNewByURL = new System.Windows.Forms.TextBox();
			this.lblNewByURL = new System.Windows.Forms.Label();
			this.pageValidateUrl = new Divelements.WizardFramework.WizardPage();
			this.lblValidationTaskImage2 = new System.Windows.Forms.Label();
			this.lblValidationTask2 = new System.Windows.Forms.Label();
			this.lblValidationTaskImage1 = new System.Windows.Forms.Label();
			this.lblValidationTask1 = new System.Windows.Forms.Label();
			this.pbar = new System.Windows.Forms.ProgressBar();
			this.lblWaitStepIntro = new System.Windows.Forms.Label();
			this.pageFoundMultipleFeeds = new Divelements.WizardFramework.WizardPage();
			this.listFeeds = new System.Windows.Forms.ListView();
			this.lblMultipleFeedsFoundHint1 = new System.Windows.Forms.Label();
			this.lblMultipleFeedsFoundHint2 = new System.Windows.Forms.Label();
			this.lblMultipleFeedsFound = new System.Windows.Forms.Label();
			this.pageTitleCategory = new Divelements.WizardFramework.WizardPage();
			this.lblFeedSources = new System.Windows.Forms.Label();
			this.cboFeedSources = new System.Windows.Forms.ComboBox();
			this.lblFeedTitle = new System.Windows.Forms.Label();
			this.lblFeedCategory = new System.Windows.Forms.Label();
			this.cboFeedCategory = new System.Windows.Forms.ComboBox();
			this.lblPageTitleCredentialsIntro = new System.Windows.Forms.Label();
			this.txtFeedTitle = new System.Windows.Forms.TextBox();
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
			this.pageNewBySearchTopic = new Divelements.WizardFramework.WizardPage();
			this.pictureHelpSyndic8 = new System.Windows.Forms.PictureBox();
			this.lblSyndic8Help = new System.Windows.Forms.LinkLabel();
			this.btnManageSearchEngines = new System.Windows.Forms.Button();
			this.lblNewBySearchEngines = new System.Windows.Forms.Label();
			this.cboNewBySearchEngines = new System.Windows.Forms.ComboBox();
			this.lblNewBySearchIntro = new System.Windows.Forms.LinkLabel();
			this.txtNewBySearchWords = new System.Windows.Forms.TextBox();
			this.lblNewBySearchWords = new System.Windows.Forms.Label();
			this.pageHowToSelection = new Divelements.WizardFramework.WizardPage();
			this.radioNewByNNTPGroup = new System.Windows.Forms.RadioButton();
			this.lblHowToSubscribeIntro = new System.Windows.Forms.Label();
			this.radioNewByTopicSearch = new System.Windows.Forms.RadioButton();
			this.radioNewByURL = new System.Windows.Forms.RadioButton();
			this.pageWelcome = new Divelements.WizardFramework.IntroductionPage();
			this.chkDisplayWelcome = new System.Windows.Forms.CheckBox();
			this.lblWelcomeInfoBox = new Divelements.WizardFramework.InformationBox();
			this.lblWelcomeHelpMessage1 = new System.Windows.Forms.Label();
			this.lblWelcomeHelpMessage2 = new System.Windows.Forms.Label();
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
			this.timerIncreaseProgress = new System.Windows.Forms.Timer(this.components);
			this.timerStartValidation = new System.Windows.Forms.Timer(this.components);
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.wizard.SuspendLayout();
			this.pageNewByURL.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpAutodiscover)).BeginInit();
			this.pageValidateUrl.SuspendLayout();
			this.pageFoundMultipleFeeds.SuspendLayout();
			this.pageTitleCategory.SuspendLayout();
			this.pageFeedCredentials.SuspendLayout();
			this.pageFeedItemControl.SuspendLayout();
			this.pageFeedItemDisplay.SuspendLayout();
			this.finishPage.SuspendLayout();
			this.pnlCompleting.SuspendLayout();
			this.pageNewBySearchTopic.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpSyndic8)).BeginInit();
			this.pageHowToSelection.SuspendLayout();
			this.pageWelcome.SuspendLayout();
			this.pageNewByNNTPGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// wizard
			// 
			this.wizard.BannerImage = ((System.Drawing.Image)(resources.GetObject("wizard.BannerImage")));
			this.wizard.Controls.Add(this._btnImmediateFinish);
			this.wizard.Controls.Add(this.pageTitleCategory);
			this.wizard.Controls.Add(this.pageNewByURL);
			this.wizard.Controls.Add(this.pageNewBySearchTopic);
			this.wizard.Controls.Add(this.pageNewByNNTPGroup);
			this.wizard.Controls.Add(this.pageValidateUrl);
			this.wizard.Controls.Add(this.pageWelcome);
			this.wizard.Controls.Add(this.pageFoundMultipleFeeds);
			this.wizard.Controls.Add(this.pageFeedItemControl);
			this.wizard.Controls.Add(this.pageFeedItemDisplay);
			this.wizard.Controls.Add(this.pageHowToSelection);
			this.wizard.Controls.Add(this.finishPage);
			this.wizard.Controls.Add(this.pageFeedCredentials);
			resources.ApplyResources(this.wizard, "wizard");
			this.wizard.MarginImage = ((System.Drawing.Image)(resources.GetObject("wizard.MarginImage")));
			this.wizard.Name = "wizard";
			this.wizard.SelectedPage = this.pageTitleCategory;
			this.wizard.Finish += new System.EventHandler(this.OnWizardFinish);
			this.wizard.Cancel += new System.EventHandler(this.OnWizardCancel);
			// 
			// _btnImmediateFinish
			// 
			resources.ApplyResources(this._btnImmediateFinish, "_btnImmediateFinish");
			this._btnImmediateFinish.Name = "_btnImmediateFinish";
			this._btnImmediateFinish.Click += new System.EventHandler(this.OnImmediateFinish_Click);
			// 
			// pageNewByURL
			// 
			this.pageNewByURL.Controls.Add(this.linkLabelCanonicalUrl);
			this.pageNewByURL.Controls.Add(this.pictureHelpAutodiscover);
			this.pageNewByURL.Controls.Add(this.lblAutodiscoverHelp);
			this.pageNewByURL.Controls.Add(this.checkNewByURLValidate);
			this.pageNewByURL.Controls.Add(this.lblNewByURLIntro);
			this.pageNewByURL.Controls.Add(this.txtNewByURL);
			this.pageNewByURL.Controls.Add(this.lblNewByURL);
			resources.ApplyResources(this.pageNewByURL, "pageNewByURL");
			this.pageNewByURL.Name = "pageNewByURL";
			this.pageNewByURL.NextPage = this.pageValidateUrl;
			this.pageNewByURL.PreviousPage = this.pageHowToSelection;
			this.pageNewByURL.AfterDisplay += new System.EventHandler(this.OnPageNewURLAfterDisplay);
			// 
			// linkLabelCanonicalUrl
			// 
			resources.ApplyResources(this.linkLabelCanonicalUrl, "linkLabelCanonicalUrl");
			this.linkLabelCanonicalUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkLabelCanonicalUrl.Name = "linkLabelCanonicalUrl";
			this.toolTip.SetToolTip(this.linkLabelCanonicalUrl, resources.GetString("linkLabelCanonicalUrl.ToolTip"));
			this.linkLabelCanonicalUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
			// 
			// pictureHelpAutodiscover
			// 
			resources.ApplyResources(this.pictureHelpAutodiscover, "pictureHelpAutodiscover");
			this.pictureHelpAutodiscover.Name = "pictureHelpAutodiscover";
			this.pictureHelpAutodiscover.TabStop = false;
			// 
			// lblAutodiscoverHelp
			// 
			this.lblAutodiscoverHelp.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.lblAutodiscoverHelp, "lblAutodiscoverHelp");
			this.lblAutodiscoverHelp.Name = "lblAutodiscoverHelp";
			this.lblAutodiscoverHelp.TabStop = true;
			this.lblAutodiscoverHelp.Tag = "http://diveintomark.org/archives/2002/08/15/ultraliberal_rss_locator";
			this.lblAutodiscoverHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
			// 
			// checkNewByURLValidate
			// 
			resources.ApplyResources(this.checkNewByURLValidate, "checkNewByURLValidate");
			this.checkNewByURLValidate.Checked = true;
			this.checkNewByURLValidate.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkNewByURLValidate.Name = "checkNewByURLValidate";
			this.checkNewByURLValidate.CheckedChanged += new System.EventHandler(this.OnAutodiscoverVerifyCheckedChanged);
			// 
			// lblNewByURLIntro
			// 
			resources.ApplyResources(this.lblNewByURLIntro, "lblNewByURLIntro");
			this.lblNewByURLIntro.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewByURLIntro.Name = "lblNewByURLIntro";
			this.lblNewByURLIntro.Tag = "";
			// 
			// txtNewByURL
			// 
			resources.ApplyResources(this.txtNewByURL, "txtNewByURL");
			this.txtNewByURL.Name = "txtNewByURL";
			this.txtNewByURL.TextChanged += new System.EventHandler(this.OnNewFeedUrlTextChanged);
			this.txtNewByURL.Validating += new System.ComponentModel.CancelEventHandler(this.OnTextNewByUrlValidating);
			// 
			// lblNewByURL
			// 
			resources.ApplyResources(this.lblNewByURL, "lblNewByURL");
			this.lblNewByURL.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewByURL.Name = "lblNewByURL";
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
			// pageTitleCategory
			// 
			this.pageTitleCategory.Controls.Add(this.lblFeedSources);
			this.pageTitleCategory.Controls.Add(this.cboFeedSources);
			this.pageTitleCategory.Controls.Add(this.lblFeedTitle);
			this.pageTitleCategory.Controls.Add(this.lblFeedCategory);
			this.pageTitleCategory.Controls.Add(this.cboFeedCategory);
			this.pageTitleCategory.Controls.Add(this.lblPageTitleCredentialsIntro);
			this.pageTitleCategory.Controls.Add(this.txtFeedTitle);
			resources.ApplyResources(this.pageTitleCategory, "pageTitleCategory");
			this.pageTitleCategory.Name = "pageTitleCategory";
			this.pageTitleCategory.NextPage = this.pageFeedCredentials;
			this.pageTitleCategory.PreviousPage = this.pageFoundMultipleFeeds;
			this.pageTitleCategory.BeforeMoveBack += new Divelements.WizardFramework.WizardPageEventHandler(this.OnPageTitleCategoryBeforeMoveBack);
			this.pageTitleCategory.BeforeDisplay += new System.EventHandler(this.OnPageTitleCategoryBeforeDisplay);
			// 
			// lblFeedSources
			// 
			resources.ApplyResources(this.lblFeedSources, "lblFeedSources");
			this.lblFeedSources.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFeedSources.Name = "lblFeedSources";
			// 
			// cboFeedSources
			// 
			resources.ApplyResources(this.cboFeedSources, "cboFeedSources");
			this.cboFeedSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboFeedSources.Name = "cboFeedSources";
			this.cboFeedSources.Sorted = true;
			// 
			// lblFeedTitle
			// 
			resources.ApplyResources(this.lblFeedTitle, "lblFeedTitle");
			this.lblFeedTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFeedTitle.Name = "lblFeedTitle";
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
			// txtFeedTitle
			// 
			resources.ApplyResources(this.txtFeedTitle, "txtFeedTitle");
			this.txtFeedTitle.Name = "txtFeedTitle";
			this.txtFeedTitle.TextChanged += new System.EventHandler(this.OnNewFeedTitleTextChanged);
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
			this.pageNewBySearchTopic.PreviousPage = this.pageHowToSelection;
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
			// pageHowToSelection
			// 
			this.pageHowToSelection.Controls.Add(this.radioNewByNNTPGroup);
			this.pageHowToSelection.Controls.Add(this.lblHowToSubscribeIntro);
			this.pageHowToSelection.Controls.Add(this.radioNewByTopicSearch);
			this.pageHowToSelection.Controls.Add(this.radioNewByURL);
			resources.ApplyResources(this.pageHowToSelection, "pageHowToSelection");
			this.pageHowToSelection.Name = "pageHowToSelection";
			this.pageHowToSelection.NextPage = this.pageNewByURL;
			this.pageHowToSelection.PreviousPage = this.pageWelcome;
			this.pageHowToSelection.AfterDisplay += new System.EventHandler(this.OnPageHowToSelectionAfterDisplay);
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
			// pageWelcome
			// 
			this.pageWelcome.Controls.Add(this.chkDisplayWelcome);
			this.pageWelcome.Controls.Add(this.lblWelcomeInfoBox);
			this.pageWelcome.Controls.Add(this.lblWelcomeHelpMessage1);
			this.pageWelcome.Controls.Add(this.lblWelcomeHelpMessage2);
			resources.ApplyResources(this.pageWelcome, "pageWelcome");
			this.pageWelcome.Name = "pageWelcome";
			this.pageWelcome.NextPage = this.pageHowToSelection;
			this.pageWelcome.BeforeDisplay += new System.EventHandler(this.OnPageWelcome_BeforeDisplay);
			// 
			// chkDisplayWelcome
			// 
			this.chkDisplayWelcome.Checked = true;
			this.chkDisplayWelcome.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.chkDisplayWelcome, "chkDisplayWelcome");
			this.chkDisplayWelcome.Name = "chkDisplayWelcome";
			// 
			// lblWelcomeInfoBox
			// 
			resources.ApplyResources(this.lblWelcomeInfoBox, "lblWelcomeInfoBox");
			this.lblWelcomeInfoBox.Icon = Divelements.WizardFramework.SystemIconType.Warning;
			this.lblWelcomeInfoBox.Name = "lblWelcomeInfoBox";
			// 
			// lblWelcomeHelpMessage1
			// 
			resources.ApplyResources(this.lblWelcomeHelpMessage1, "lblWelcomeHelpMessage1");
			this.lblWelcomeHelpMessage1.Name = "lblWelcomeHelpMessage1";
			// 
			// lblWelcomeHelpMessage2
			// 
			resources.ApplyResources(this.lblWelcomeHelpMessage2, "lblWelcomeHelpMessage2");
			this.lblWelcomeHelpMessage2.Name = "lblWelcomeHelpMessage2";
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
			this.pageNewByNNTPGroup.PreviousPage = this.pageHowToSelection;
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
			// timerIncreaseProgress
			// 
			this.timerIncreaseProgress.Tick += new System.EventHandler(this.OnTimerIncreaseProgress_Tick);
			// 
			// timerStartValidation
			// 
			this.timerStartValidation.Tick += new System.EventHandler(this.OnTimerStartValidation);
			// 
			// AddSubscriptionWizard
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.wizard);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddSubscriptionWizard";
			this.wizard.ResumeLayout(false);
			this.pageNewByURL.ResumeLayout(false);
			this.pageNewByURL.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpAutodiscover)).EndInit();
			this.pageValidateUrl.ResumeLayout(false);
			this.pageFoundMultipleFeeds.ResumeLayout(false);
			this.pageTitleCategory.ResumeLayout(false);
			this.pageTitleCategory.PerformLayout();
			this.pageFeedCredentials.ResumeLayout(false);
			this.pageFeedCredentials.PerformLayout();
			this.pageFeedItemControl.ResumeLayout(false);
			this.pageFeedItemDisplay.ResumeLayout(false);
			this.finishPage.ResumeLayout(false);
			this.pnlCompleting.ResumeLayout(false);
			this.pageNewBySearchTopic.ResumeLayout(false);
			this.pageNewBySearchTopic.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpSyndic8)).EndInit();
			this.pageHowToSelection.ResumeLayout(false);
			this.pageWelcome.ResumeLayout(false);
			this.pageNewByNNTPGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
					pageHowToSelection.NextPage = pageNewByURL;
					break;
				case AddSubscriptionWizardMode.SubscribeURLDirect:
					pageWelcome.NextPage = pageNewByURL;
					pageNewByURL.PreviousPage = pageWelcome;
					pageTitleCategory.PreviousPage = pageNewByURL;
					break;
				case AddSubscriptionWizardMode.SubscribeNNTPGroup:
					pageHowToSelection.NextPage = pageNewByNNTPGroup;
					pageTitleCategory.PreviousPage = pageNewByNNTPGroup;
					break;
				case AddSubscriptionWizardMode.SubscribeNNTPDirect:
					pageWelcome.NextPage = pageNewByNNTPGroup;
					pageNewByNNTPGroup.PreviousPage = pageWelcome;
					pageTitleCategory.PreviousPage = pageNewByNNTPGroup;
					break;
				case AddSubscriptionWizardMode.SubscribeSearch:
					pageHowToSelection.NextPage = pageNewBySearchTopic;
					pageFoundMultipleFeeds.PreviousPage = pageNewBySearchTopic;
					pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
					break;
				case AddSubscriptionWizardMode.SubscribeSearchDirect:
					pageWelcome.NextPage = pageNewBySearchTopic;
					pageNewBySearchTopic.PreviousPage = pageWelcome;
					pageFoundMultipleFeeds.PreviousPage = pageNewBySearchTopic;
					pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
					break;
				case AddSubscriptionWizardMode.SubscribeNNTPGroupDirect:
					pageWelcome.NextPage = pageTitleCategory;
					pageTitleCategory.PreviousPage = pageWelcome;
					break;
				default:
					throw new InvalidOperationException("WizardMode '" + m.ToString() + "' not supported");
			}
		}
		
		internal void ReWireCredentialsStep(bool beforeDiscoverValidateUrl) {
			if (this.credentialsStepReWired == beforeDiscoverValidateUrl)
				return;
			if (beforeDiscoverValidateUrl) {
				pageNewByURL.NextPage = pageFeedCredentials;
				pageFeedCredentials.PreviousPage = pageNewByURL;
				pageFeedCredentials.NextPage = pageValidateUrl;
				pageValidateUrl.PreviousPage = pageFeedCredentials;
			} else {
				pageNewByURL.NextPage = pageValidateUrl;
				pageFeedCredentials.PreviousPage = pageTitleCategory;
				pageFeedCredentials.NextPage = pageFeedItemControl;
				pageValidateUrl.PreviousPage = pageNewByURL;
			}
			this.credentialsStepReWired = beforeDiscoverValidateUrl;
		}

		private void ResetValidationPage() {
			this.feedInfo = null;
			pbar.Value = 0;
			for (int i = 0; i < 2; i++)
				SetWizardTaskInfo(i, WizardValidationTask.None, String.Empty);
		}

		private static Image GetWizardTaskImage(WizardValidationTask task) {
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
					autoDiscover.OperationMessage = String.Format(SR.GUIStatusWaitMessageDetectingFeedsWithKeywords,this.txtNewBySearchWords.Text);
				} else {
					autoDiscover.WebPageUrl = this.txtNewByURL.Text;
					autoDiscover.OperationMessage = String.Format(SR.GUIStatusWaitMessageDetectingFeeds,this.txtNewBySearchWords.Text);
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

				SetWizardTaskInfo(taskIndex, WizardValidationTask.Success, String.Format(SR.WizardValidationTask_Success_xFeedsFound_Message,feedUrls.Count));
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
				string message = String.Format(SR.GUIStatusWaitMessagePrefetchFeed,url);
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
					this.txtNewByURL.Text = url;
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

		private static string StripAndDecode(string s) {
			if (s == null)
				return s;
			string t = HtmlHelper.StripAnyTags(s);
			if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
				t = HtmlHelper.HtmlDecode(t);
			}
			return t.Trim();
		}
		
		private void PopulateFoundFeeds(IDictionary rssFeeds) {
			ListViewItem lv;
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
			Exception ex;
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
			Uri reqUri;
			if (Uri.TryCreate(newUrl, UriKind.Absolute, out reqUri))
				newUrl = reqUri.CanonicalizedUri();
			else
			{
				if(!newUrl.ToLower().StartsWith("http://"))
				{
					try
					{
						reqUri = new Uri("http://" + newUrl);
						newUrl = reqUri.CanonicalizedUri();
					}
					catch (UriFormatException ex)
					{
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
					String.Format(SR.GUIFieldLinkRedundantInfo, 
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
					INntpServerDefinition sd = this.coreApplication.NntpServerDefinitions[cboNNTPServer.Text];

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
				this.wizardMode == AddSubscriptionWizardMode.SubscribeNNTPGroupDirect)
			{
				if (this.lstNNTPGroups.SelectedItems.Count > 1)
					return this.lstNNTPGroups.SelectedItems[index].ToString();
				return this.FeedTitle;
			}
			else if (this.wizardMode == AddSubscriptionWizardMode.SubscribeURL ||
				this.wizardMode == AddSubscriptionWizardMode.SubscribeURLDirect )
			{
				if (this.listFeeds.SelectedItems.Count > 1)
					return this.listFeeds.SelectedItems[index].SubItems[0].Text;
				return this.FeedTitle;
			}
			else {
				return this.FeedTitle;
			}
		}

		/// <summary>
		/// Gets or sets the feed URL.
		/// </summary>
		/// <value>The feed URL.</value>
		public string FeedUrl
		{
			get { return this.txtNewByURL.Text.Trim(); } 
			set { this.txtNewByURL.Text = value; }
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
		/// Gets or sets the current selected feed source name.
		/// </summary>
		/// <value>The feed category.</value>
		public string FeedSourceName
		{
			get
			{
				string c = this.cboFeedSources.Text.Trim();
				return c;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
					this.cboFeedSources.Text = value;
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
				try
				{
					if((!string.IsNullOrEmpty(this.cboUpdateFrequency.Text.Trim()))) {
						Int32 intIn = Int32.Parse(this.cboUpdateFrequency.Text.Trim());
						if (intIn <= 0) {
							return 0;
						}
						return intIn;
					}
					return 0;
				}
				catch(FormatException)
				{
					if (coreApplication.CurrentGlobalRefreshRate > 0)
						return coreApplication.CurrentGlobalRefreshRate;
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
		
		private void OnTimerIncreaseProgress_Tick(object sender, EventArgs e)
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

		private void OnPageValidation_BeforeDisplay(object sender, EventArgs e) {
			ResetValidationPage();
		}

		private void OnPageValidation_AfterDisplay(object sender, EventArgs e)
		{
			pageValidateUrl.AllowCancel = true;
			//pageValidateUrl.AllowMoveNext = false;
			this.timerStartValidation.Enabled = true;

		}

		private void OnWizardCancel(object sender, EventArgs e) {
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

		private void OnWizardFinish(object sender, EventArgs e) {
			
			this.ProcessFeedUrl();
			this.DialogResult = DialogResult.OK;
			Close();
		}

		

		private void OnFoundFeedsListSelectedIndexChanged(object sender, EventArgs e)
		{
			pageFoundMultipleFeeds.AllowMoveNext = listFeeds.SelectedItems.Count > 0;
		}

		private void OnListFoundFeeds_DoubleClick(object sender, EventArgs e)
		{
			if (pageFoundMultipleFeeds.AllowMoveNext)
				wizard.GoNext();
		}

		private void OnPageWelcome_BeforeDisplay(object sender, EventArgs e) {
			
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
			checkNewByURLValidate.Enabled = radioNewByTopicSearch.Enabled = internetConnected;
		}

		private void OnImmediateFinish_Click(object sender, EventArgs e)
		{
			this.ProcessFeedUrl();
			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void OnFinishPage_BeforeDisplay(object sender, EventArgs e)
		{
			this._btnImmediateFinish.Visible = false;
		}

		private void OnFinishPage_BeforeMoveBack(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this._btnImmediateFinish.Visible = true;
		}

		private void pageFeedCredentials_BeforeDisplay(object sender, EventArgs e) {
			//
		}

		private void btnManageSearchEngines_Click(object sender, EventArgs e) {
			coreApplication.ShowOptions(OptionDialogSection.WebSearch, this, this.OnPreferenceDialogOptionChanged);
		}

		private void OnPreferenceDialogOptionChanged(object sender, EventArgs e) {
			// refresh widgets that depends on Option settings
			// search engine list:
			this.PopulateSearchEngines(coreApplication.WebSearchEngines, cboNewBySearchEngines.Text);
		}

		private void OnRadioHowToSubscribeCheckedChanged(object sender, EventArgs e) {
			if (sender == radioNewByURL)
				WireStepsForMode(AddSubscriptionWizardMode.SubscribeURL);
			else if (sender == radioNewByTopicSearch)
				WireStepsForMode(AddSubscriptionWizardMode.SubscribeSearch);
			else if (sender == radioNewByNNTPGroup)
				WireStepsForMode(AddSubscriptionWizardMode.SubscribeNNTPGroup);
		}

		private void OnAutodiscoverVerifyCheckedChanged(object sender, EventArgs e) {
			if (this.checkNewByURLValidate.Checked) {
				pageNewByURL.NextPage = pageValidateUrl;
				pageTitleCategory.PreviousPage = pageNewByURL;
				pageValidateUrl.NextPage = pageTitleCategory;
			} else {
				pageNewByURL.NextPage = pageTitleCategory;
				pageTitleCategory.PreviousPage = pageNewByURL;
			}
		}

		private void OnMultipleFeedsBeforeMoveNext(object sender, System.ComponentModel.CancelEventArgs e) {
//			this.FeedUrl = (string)listFeeds.SelectedItems[0].Tag;
			if (this.MultipleFeedsToSubscribe)
				this.FeedTitle = String.Empty;
			else
				this.FeedTitle = listFeeds.SelectedItems[0].SubItems[0].Text;
		}

		private void OnNewFeedUrlTextChanged(object sender, EventArgs e) {
			Exception ex;
			string validUrl = this.ValidateFeedUri(this.txtNewByURL.Text, out ex);
			bool valid = ( validUrl != null && ex == null);
			pageNewByURL.AllowMoveNext = valid;
			if (valid) {
				pageNewByURL.ProceedText = String.Empty;
				lblAutodiscoverHelp.Visible = true;
				pictureHelpAutodiscover.Visible = true;
				if (this.txtNewByURL.Text != validUrl) {
					linkLabelCanonicalUrl.Text = validUrl;
					linkLabelCanonicalUrl.LinkArea = new LinkArea(0, validUrl.Length);
				} else {
					linkLabelCanonicalUrl.Text = null;
				}
			} else if (ex != null) {
				pageNewByURL.ProceedText = ex.Message;
				lblAutodiscoverHelp.Visible = false;
				pictureHelpAutodiscover.Visible = false;
				linkLabelCanonicalUrl.Text = null;
			}
		}

		private void OnPageTitleCategoryBeforeMoveBack(object sender, System.ComponentModel.CancelEventArgs e) {
			this._btnImmediateFinish.Visible = false;
		}

		private void OnPageTitleCategoryBeforeDisplay(object sender, EventArgs e) {
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

		private void OnNewFeedTitleTextChanged(object sender, EventArgs e) {
			this.pageTitleCategory.AllowMoveNext = this._btnImmediateFinish.Enabled = 
				(txtFeedTitle.Text.Length > 0 || !txtFeedTitle.Enabled);
		}

		private void OnPageHowToSelectionAfterDisplay(object sender, EventArgs e) {
			this.WireStepsForMode(AddSubscriptionWizardMode.SubscribeURL);
		}

		private void OnPageNewNNTPGroupAfterDisplay(object sender, EventArgs e) {
			this.feedInfo = null;
			pageNewByNNTPGroup.AllowMoveNext = false;
			PopulateNntpServerDefinitions(coreApplication.NntpServerDefinitions, null);
		}

		private void OnPageNewURLAfterDisplay(object sender, EventArgs e) {
			this.feedInfo = null;
			if (string.IsNullOrEmpty(this.txtNewByURL.Text))
				pageNewByURL.AllowMoveNext = false;
		}

		private void OnPageNewSearchAfterDisplay(object sender, EventArgs e) {
			this.feedInfo = null;
			this.PopulateSearchEngines(coreApplication.WebSearchEngines, cboNewBySearchEngines.Text);
		}

		private void btnManageNNTPServer_Click(object sender, EventArgs e) {
			coreApplication.ShowNntpServerManagementDialog(this, this.OnManagedNNTPServersChange);
		}
		
		private void OnManagedNNTPServersChange(object sender, EventArgs e) {
			this.PopulateNntpServerDefinitions(coreApplication.NntpServerDefinitions, cboNNTPServer.Text);
		}

		private void OnNNTPServerSelectedValueChanged(object sender, EventArgs e) {
			this.PopulateNntpServerGroups(false);
		}

		private void OnNNTPGroupsListSelectedValueChanged(object sender, EventArgs e) {
			pageNewByNNTPGroup.AllowMoveNext = lstNNTPGroups.SelectedItems.Count > 0;
			if (lstNNTPGroups.SelectedItems.Count == 1) {
				txtFeedTitle.Text = lstNNTPGroups.SelectedItems[0].ToString();
			} else if(lstNNTPGroups.SelectedItems.Count > 1) {
				txtFeedTitle.Text = string.Empty;
			}
		}

		private void OnNNTPGroupsDoubleClick(object sender, EventArgs e) {
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

		private void OnPageFeedItemDisplayAfterDisplay(object sender, EventArgs e) {
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
			ValidateFeedUri(pageNewByURL, this.txtNewByURL.Text);
		}

		private void OnTimerStartValidation(object sender, EventArgs e) {
			
			this.timerStartValidation.Enabled = false;
			
			if (this.wizardMode == AddSubscriptionWizardMode.SubscribeURL || this.wizardMode == AddSubscriptionWizardMode.SubscribeURLDirect) {
				pageValidateUrl.NextPage = pageTitleCategory;
				pageValidateUrl.PreviousPage = pageNewByURL;
				SetWizardTaskInfo(0, WizardValidationTask.InProgress, SR.WizardValidationTask_VerifyingUrlMessage);
				SetWizardTaskInfo(1, WizardValidationTask.Pending, SR.WizardValidationTask_GetWebFeedTitleMessage);
				
				// MUST BE YET VALIDATED:
				this.txtNewByURL.Text = this.ValidateFeedUri(pageValidateUrl, this.txtNewByURL.Text);
				pageValidateUrl.AllowMoveNext = false;

				bool success = TaskBeginSearchFeedLocations(0, FeedLocationMethod.AutoDiscoverUrl);
				Application.DoEvents();
				if (success) {
					
					if (this.listFeeds.Items.Count > 1) {
						pageValidateUrl.NextPage = pageFoundMultipleFeeds;
						pageFoundMultipleFeeds.PreviousPage = pageNewByURL;
						pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
						this.FeedTitle = String.Empty;
					} else {
						pageValidateUrl.NextPage = pageTitleCategory;
						if (!this.credentialsStepReWired)
							pageTitleCategory.PreviousPage = pageNewByURL;
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
					this.txtNewByURL.Text = this.ValidateFeedUri(pageValidateUrl, this.GetSearchEngineSearchUrl(this.SearchTerms));
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

	}

	#region WizardMode enum
	internal enum AddSubscriptionWizardMode {
		/// <summary>
		/// Show all steps
		/// </summary>
		Default,
		/// <summary>
		/// Feed URL steps
		/// </summary>
		SubscribeURL,
		/// <summary>
		/// Search steps
		/// </summary>
		SubscribeSearch,
		/// <summary>
		/// NNTP Group steps
		/// </summary>
		SubscribeNNTPGroup,
		/// <summary>
		/// Like SubscribeNNTPGroup, but but ignores the pageHowToSelection
		/// </summary>
		SubscribeNNTPDirect,
		/// <summary>
		/// Like SubscribeNNTPGroup, but did no go back from title/category step
		/// </summary>
		SubscribeNNTPGroupDirect,
		/// <summary>
		/// Like SubscribeURL, but but ignores the pageHowToSelection
		/// </summary>
		SubscribeURLDirect,
		/// <summary>
		/// Like SubscribeSearch, but but ignores the pageHowToSelection
		/// </summary>
		SubscribeSearchDirect,
	}
	#endregion

}

#region CVS Version Log
/*
 * $Log: SynchronizeFeedsWizard.cs,v $
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
