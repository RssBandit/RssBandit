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
		internal feedsFeed Feed = null;

		private WizardMode wizardMode; 
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
		private System.ComponentModel.IContainer components;

		#endregion

		#region ctor's
		private AddSubscriptionWizard() {
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

		public AddSubscriptionWizard(IServiceProvider provider, WizardMode mode):this()
		{
			serviceProvider = provider;
			wizardMode = mode;

			// form location management:
			windowSerializer = new WindowSerializer(this);
			windowSerializer.SaveOnlyLocation = true;
			windowSerializer.SaveNoWindowState = true;

			windowSerializer.LoadStateEvent += new WindowSerializer.WindowSerializerDelegate(OnWindowSerializerLoadStateEvent);
			windowSerializer.SaveStateEvent += new WindowSerializer.WindowSerializerDelegate(OnWindowSerializerSaveStateEvent);

			// to get notified, if the inet connection state changes:
			internetService = (IInternetService)this.GetService(typeof(IInternetService));
			if (internetService != null) {
				internetService.InternetConnectionStateChange += new InternetConnectionStateChangeHandler(OnInternetServiceInternetConnectionStateChange);
				checkNewByURLValidate.Enabled = radioNewByTopicSearch.Enabled = internetService.InternetAccessAllowed;
			}
			// to checkout the defaults to be used for the new feed:
			IUserPreferences preferencesService = (IUserPreferences)this.GetService(typeof(IUserPreferences));
			this.MaxItemAge = preferencesService.MaxItemAge;

			coreApplication = (ICoreApplication)this.GetService(typeof(ICoreApplication));
			this.cboUpdateFrequency.Text = "60";
			if (coreApplication.CurrentGlobalRefreshRate > 0)	// if not disabled refreshing
				this.cboUpdateFrequency.Text = String.Format("{0}", coreApplication.CurrentGlobalRefreshRate); 
			
			//initialize category combo box			
			foreach(string category in coreApplication.GetCategories())
			{
				if (!StringHelper.EmptyOrNull(category))
					this.cboFeedCategory.Items.Add(category); 
			}

			this.FeedCategory = coreApplication.DefaultCategory; 

			this.WireStepsForMode(this.wizardMode);
			this.wizard.SelectedPage = this.pageWelcome;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AddSubscriptionWizard));
			this.wizard = new Divelements.WizardFramework.Wizard();
			this._btnImmediateFinish = new System.Windows.Forms.Button();
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
			this.pageFoundMultipleFeeds = new Divelements.WizardFramework.WizardPage();
			this.listFeeds = new System.Windows.Forms.ListView();
			this.lblMultipleFeedsFoundHint1 = new System.Windows.Forms.Label();
			this.lblMultipleFeedsFoundHint2 = new System.Windows.Forms.Label();
			this.lblMultipleFeedsFound = new System.Windows.Forms.Label();
			this.pageTitleCategory = new Divelements.WizardFramework.WizardPage();
			this.lblFeedCategory = new System.Windows.Forms.Label();
			this.cboFeedCategory = new System.Windows.Forms.ComboBox();
			this.lblPageTitleCredentialsIntro = new System.Windows.Forms.Label();
			this.lblFeedTitle = new System.Windows.Forms.Label();
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
			this.pageHowToSelection = new Divelements.WizardFramework.WizardPage();
			this.radioNewByNNTPGroup = new System.Windows.Forms.RadioButton();
			this.lblHowToSubscribeIntro = new System.Windows.Forms.Label();
			this.radioNewByTopicSearch = new System.Windows.Forms.RadioButton();
			this.radioNewByURL = new System.Windows.Forms.RadioButton();
			this.pageNewByURL = new Divelements.WizardFramework.WizardPage();
			this.linkLabelCanonicalUrl = new System.Windows.Forms.LinkLabel();
			this.pictureHelpAutodiscover = new System.Windows.Forms.PictureBox();
			this.lblAutodiscoverHelp = new System.Windows.Forms.LinkLabel();
			this.checkNewByURLValidate = new System.Windows.Forms.CheckBox();
			this.lblNewByURLIntro = new System.Windows.Forms.LinkLabel();
			this.txtNewByURL = new System.Windows.Forms.TextBox();
			this.lblNewByURL = new System.Windows.Forms.Label();
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
			this.pageNewBySearchTopic.SuspendLayout();
			this.pageValidateUrl.SuspendLayout();
			this.pageFoundMultipleFeeds.SuspendLayout();
			this.pageTitleCategory.SuspendLayout();
			this.pageFeedCredentials.SuspendLayout();
			this.pageFeedItemControl.SuspendLayout();
			this.pageFeedItemDisplay.SuspendLayout();
			this.finishPage.SuspendLayout();
			this.pnlCompleting.SuspendLayout();
			this.pageHowToSelection.SuspendLayout();
			this.pageNewByURL.SuspendLayout();
			this.pageWelcome.SuspendLayout();
			this.pageNewByNNTPGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// wizard
			// 
			this.wizard.AccessibleDescription = resources.GetString("wizard.AccessibleDescription");
			this.wizard.AccessibleName = resources.GetString("wizard.AccessibleName");
			this.wizard.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("wizard.Anchor")));
			this.wizard.AutoScroll = ((bool)(resources.GetObject("wizard.AutoScroll")));
			this.wizard.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("wizard.AutoScrollMargin")));
			this.wizard.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("wizard.AutoScrollMinSize")));
			this.wizard.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("wizard.BackgroundImage")));
			this.wizard.BannerImage = ((System.Drawing.Image)(resources.GetObject("wizard.BannerImage")));
			this.wizard.CancelText = resources.GetString("wizard.CancelText");
			this.wizard.Controls.Add(this._btnImmediateFinish);
			this.wizard.Controls.Add(this.pageNewByURL);
			this.wizard.Controls.Add(this.pageNewBySearchTopic);
			this.wizard.Controls.Add(this.pageNewByNNTPGroup);
			this.wizard.Controls.Add(this.pageValidateUrl);
			this.wizard.Controls.Add(this.pageTitleCategory);
			this.wizard.Controls.Add(this.pageWelcome);
			this.wizard.Controls.Add(this.pageFoundMultipleFeeds);
			this.wizard.Controls.Add(this.pageFeedItemControl);
			this.wizard.Controls.Add(this.pageFeedItemDisplay);
			this.wizard.Controls.Add(this.pageHowToSelection);
			this.wizard.Controls.Add(this.finishPage);
			this.wizard.Controls.Add(this.pageFeedCredentials);
			this.wizard.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("wizard.Dock")));
			this.wizard.Enabled = ((bool)(resources.GetObject("wizard.Enabled")));
			this.wizard.FinishText = resources.GetString("wizard.FinishText");
			this.wizard.Font = ((System.Drawing.Font)(resources.GetObject("wizard.Font")));
			this.wizard.HelpText = resources.GetString("wizard.HelpText");
			this.wizard.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("wizard.ImeMode")));
			this.wizard.Location = ((System.Drawing.Point)(resources.GetObject("wizard.Location")));
			this.wizard.MarginImage = ((System.Drawing.Image)(resources.GetObject("wizard.MarginImage")));
			this.wizard.Name = "wizard";
			this.wizard.NextText = resources.GetString("wizard.NextText");
			this.wizard.PreviousText = resources.GetString("wizard.PreviousText");
			this.wizard.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("wizard.RightToLeft")));
			this.wizard.SelectedPage = this.pageNewByURL;
			this.wizard.Size = ((System.Drawing.Size)(resources.GetObject("wizard.Size")));
			this.wizard.TabIndex = ((int)(resources.GetObject("wizard.TabIndex")));
			this.wizard.Text = resources.GetString("wizard.Text");
			this.toolTip.SetToolTip(this.wizard, resources.GetString("wizard.ToolTip"));
			this.wizard.Visible = ((bool)(resources.GetObject("wizard.Visible")));
			this.wizard.Cancel += new System.EventHandler(this.OnWizardCancel);
			this.wizard.Finish += new System.EventHandler(this.OnWizardFinish);
			// 
			// _btnImmediateFinish
			// 
			this._btnImmediateFinish.AccessibleDescription = resources.GetString("_btnImmediateFinish.AccessibleDescription");
			this._btnImmediateFinish.AccessibleName = resources.GetString("_btnImmediateFinish.AccessibleName");
			this._btnImmediateFinish.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_btnImmediateFinish.Anchor")));
			this._btnImmediateFinish.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_btnImmediateFinish.BackgroundImage")));
			this._btnImmediateFinish.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("_btnImmediateFinish.Dock")));
			this._btnImmediateFinish.Enabled = ((bool)(resources.GetObject("_btnImmediateFinish.Enabled")));
			this._btnImmediateFinish.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("_btnImmediateFinish.FlatStyle")));
			this._btnImmediateFinish.Font = ((System.Drawing.Font)(resources.GetObject("_btnImmediateFinish.Font")));
			this._btnImmediateFinish.Image = ((System.Drawing.Image)(resources.GetObject("_btnImmediateFinish.Image")));
			this._btnImmediateFinish.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("_btnImmediateFinish.ImageAlign")));
			this._btnImmediateFinish.ImageIndex = ((int)(resources.GetObject("_btnImmediateFinish.ImageIndex")));
			this._btnImmediateFinish.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_btnImmediateFinish.ImeMode")));
			this._btnImmediateFinish.Location = ((System.Drawing.Point)(resources.GetObject("_btnImmediateFinish.Location")));
			this._btnImmediateFinish.Name = "_btnImmediateFinish";
			this._btnImmediateFinish.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_btnImmediateFinish.RightToLeft")));
			this._btnImmediateFinish.Size = ((System.Drawing.Size)(resources.GetObject("_btnImmediateFinish.Size")));
			this._btnImmediateFinish.TabIndex = ((int)(resources.GetObject("_btnImmediateFinish.TabIndex")));
			this._btnImmediateFinish.Text = resources.GetString("_btnImmediateFinish.Text");
			this._btnImmediateFinish.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("_btnImmediateFinish.TextAlign")));
			this.toolTip.SetToolTip(this._btnImmediateFinish, resources.GetString("_btnImmediateFinish.ToolTip"));
			this._btnImmediateFinish.Visible = ((bool)(resources.GetObject("_btnImmediateFinish.Visible")));
			this._btnImmediateFinish.Click += new System.EventHandler(this.OnImmediateFinish_Click);
			// 
			// pageNewBySearchTopic
			// 
			this.pageNewBySearchTopic.AccessibleDescription = resources.GetString("pageNewBySearchTopic.AccessibleDescription");
			this.pageNewBySearchTopic.AccessibleName = resources.GetString("pageNewBySearchTopic.AccessibleName");
			this.pageNewBySearchTopic.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageNewBySearchTopic.Anchor")));
			this.pageNewBySearchTopic.AutoScroll = ((bool)(resources.GetObject("pageNewBySearchTopic.AutoScroll")));
			this.pageNewBySearchTopic.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageNewBySearchTopic.AutoScrollMargin")));
			this.pageNewBySearchTopic.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageNewBySearchTopic.AutoScrollMinSize")));
			this.pageNewBySearchTopic.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageNewBySearchTopic.BackgroundImage")));
			this.pageNewBySearchTopic.Controls.Add(this.pictureHelpSyndic8);
			this.pageNewBySearchTopic.Controls.Add(this.lblSyndic8Help);
			this.pageNewBySearchTopic.Controls.Add(this.btnManageSearchEngines);
			this.pageNewBySearchTopic.Controls.Add(this.lblNewBySearchEngines);
			this.pageNewBySearchTopic.Controls.Add(this.cboNewBySearchEngines);
			this.pageNewBySearchTopic.Controls.Add(this.lblNewBySearchIntro);
			this.pageNewBySearchTopic.Controls.Add(this.txtNewBySearchWords);
			this.pageNewBySearchTopic.Controls.Add(this.lblNewBySearchWords);
			this.pageNewBySearchTopic.Description = resources.GetString("pageNewBySearchTopic.Description");
			this.pageNewBySearchTopic.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageNewBySearchTopic.Dock")));
			this.pageNewBySearchTopic.Enabled = ((bool)(resources.GetObject("pageNewBySearchTopic.Enabled")));
			this.pageNewBySearchTopic.Font = ((System.Drawing.Font)(resources.GetObject("pageNewBySearchTopic.Font")));
			this.pageNewBySearchTopic.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageNewBySearchTopic.ImeMode")));
			this.pageNewBySearchTopic.Location = ((System.Drawing.Point)(resources.GetObject("pageNewBySearchTopic.Location")));
			this.pageNewBySearchTopic.Name = "pageNewBySearchTopic";
			this.pageNewBySearchTopic.NextPage = this.pageValidateUrl;
			this.pageNewBySearchTopic.PreviousPage = this.pageHowToSelection;
			this.pageNewBySearchTopic.ProceedText = resources.GetString("pageNewBySearchTopic.ProceedText");
			this.pageNewBySearchTopic.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageNewBySearchTopic.RightToLeft")));
			this.pageNewBySearchTopic.Size = ((System.Drawing.Size)(resources.GetObject("pageNewBySearchTopic.Size")));
			this.pageNewBySearchTopic.TabIndex = ((int)(resources.GetObject("pageNewBySearchTopic.TabIndex")));
			this.pageNewBySearchTopic.Text = resources.GetString("pageNewBySearchTopic.Text");
			this.toolTip.SetToolTip(this.pageNewBySearchTopic, resources.GetString("pageNewBySearchTopic.ToolTip"));
			this.pageNewBySearchTopic.Visible = ((bool)(resources.GetObject("pageNewBySearchTopic.Visible")));
			this.pageNewBySearchTopic.AfterDisplay += new System.EventHandler(this.OnPageNewSearchAfterDisplay);
			// 
			// pictureHelpSyndic8
			// 
			this.pictureHelpSyndic8.AccessibleDescription = resources.GetString("pictureHelpSyndic8.AccessibleDescription");
			this.pictureHelpSyndic8.AccessibleName = resources.GetString("pictureHelpSyndic8.AccessibleName");
			this.pictureHelpSyndic8.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureHelpSyndic8.Anchor")));
			this.pictureHelpSyndic8.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureHelpSyndic8.BackgroundImage")));
			this.pictureHelpSyndic8.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureHelpSyndic8.Dock")));
			this.pictureHelpSyndic8.Enabled = ((bool)(resources.GetObject("pictureHelpSyndic8.Enabled")));
			this.pictureHelpSyndic8.Font = ((System.Drawing.Font)(resources.GetObject("pictureHelpSyndic8.Font")));
			this.pictureHelpSyndic8.Image = ((System.Drawing.Image)(resources.GetObject("pictureHelpSyndic8.Image")));
			this.pictureHelpSyndic8.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureHelpSyndic8.ImeMode")));
			this.pictureHelpSyndic8.Location = ((System.Drawing.Point)(resources.GetObject("pictureHelpSyndic8.Location")));
			this.pictureHelpSyndic8.Name = "pictureHelpSyndic8";
			this.pictureHelpSyndic8.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureHelpSyndic8.RightToLeft")));
			this.pictureHelpSyndic8.Size = ((System.Drawing.Size)(resources.GetObject("pictureHelpSyndic8.Size")));
			this.pictureHelpSyndic8.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureHelpSyndic8.SizeMode")));
			this.pictureHelpSyndic8.TabIndex = ((int)(resources.GetObject("pictureHelpSyndic8.TabIndex")));
			this.pictureHelpSyndic8.TabStop = false;
			this.pictureHelpSyndic8.Text = resources.GetString("pictureHelpSyndic8.Text");
			this.toolTip.SetToolTip(this.pictureHelpSyndic8, resources.GetString("pictureHelpSyndic8.ToolTip"));
			this.pictureHelpSyndic8.Visible = ((bool)(resources.GetObject("pictureHelpSyndic8.Visible")));
			// 
			// lblSyndic8Help
			// 
			this.lblSyndic8Help.AccessibleDescription = resources.GetString("lblSyndic8Help.AccessibleDescription");
			this.lblSyndic8Help.AccessibleName = resources.GetString("lblSyndic8Help.AccessibleName");
			this.lblSyndic8Help.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblSyndic8Help.Anchor")));
			this.lblSyndic8Help.AutoSize = ((bool)(resources.GetObject("lblSyndic8Help.AutoSize")));
			this.lblSyndic8Help.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblSyndic8Help.Dock")));
			this.lblSyndic8Help.Enabled = ((bool)(resources.GetObject("lblSyndic8Help.Enabled")));
			this.lblSyndic8Help.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblSyndic8Help.Font = ((System.Drawing.Font)(resources.GetObject("lblSyndic8Help.Font")));
			this.lblSyndic8Help.Image = ((System.Drawing.Image)(resources.GetObject("lblSyndic8Help.Image")));
			this.lblSyndic8Help.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblSyndic8Help.ImageAlign")));
			this.lblSyndic8Help.ImageIndex = ((int)(resources.GetObject("lblSyndic8Help.ImageIndex")));
			this.lblSyndic8Help.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblSyndic8Help.ImeMode")));
			this.lblSyndic8Help.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblSyndic8Help.LinkArea")));
			this.lblSyndic8Help.Location = ((System.Drawing.Point)(resources.GetObject("lblSyndic8Help.Location")));
			this.lblSyndic8Help.Name = "lblSyndic8Help";
			this.lblSyndic8Help.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblSyndic8Help.RightToLeft")));
			this.lblSyndic8Help.Size = ((System.Drawing.Size)(resources.GetObject("lblSyndic8Help.Size")));
			this.lblSyndic8Help.TabIndex = ((int)(resources.GetObject("lblSyndic8Help.TabIndex")));
			this.lblSyndic8Help.TabStop = true;
			this.lblSyndic8Help.Tag = "http://www.syndic8.com/";
			this.lblSyndic8Help.Text = resources.GetString("lblSyndic8Help.Text");
			this.lblSyndic8Help.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblSyndic8Help.TextAlign")));
			this.toolTip.SetToolTip(this.lblSyndic8Help, resources.GetString("lblSyndic8Help.ToolTip"));
			this.lblSyndic8Help.Visible = ((bool)(resources.GetObject("lblSyndic8Help.Visible")));
			this.lblSyndic8Help.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
			// 
			// btnManageSearchEngines
			// 
			this.btnManageSearchEngines.AccessibleDescription = resources.GetString("btnManageSearchEngines.AccessibleDescription");
			this.btnManageSearchEngines.AccessibleName = resources.GetString("btnManageSearchEngines.AccessibleName");
			this.btnManageSearchEngines.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnManageSearchEngines.Anchor")));
			this.btnManageSearchEngines.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnManageSearchEngines.BackgroundImage")));
			this.btnManageSearchEngines.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnManageSearchEngines.Dock")));
			this.btnManageSearchEngines.Enabled = ((bool)(resources.GetObject("btnManageSearchEngines.Enabled")));
			this.btnManageSearchEngines.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnManageSearchEngines.FlatStyle")));
			this.btnManageSearchEngines.Font = ((System.Drawing.Font)(resources.GetObject("btnManageSearchEngines.Font")));
			this.btnManageSearchEngines.Image = ((System.Drawing.Image)(resources.GetObject("btnManageSearchEngines.Image")));
			this.btnManageSearchEngines.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageSearchEngines.ImageAlign")));
			this.btnManageSearchEngines.ImageIndex = ((int)(resources.GetObject("btnManageSearchEngines.ImageIndex")));
			this.btnManageSearchEngines.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnManageSearchEngines.ImeMode")));
			this.btnManageSearchEngines.Location = ((System.Drawing.Point)(resources.GetObject("btnManageSearchEngines.Location")));
			this.btnManageSearchEngines.Name = "btnManageSearchEngines";
			this.btnManageSearchEngines.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnManageSearchEngines.RightToLeft")));
			this.btnManageSearchEngines.Size = ((System.Drawing.Size)(resources.GetObject("btnManageSearchEngines.Size")));
			this.btnManageSearchEngines.TabIndex = ((int)(resources.GetObject("btnManageSearchEngines.TabIndex")));
			this.btnManageSearchEngines.Text = resources.GetString("btnManageSearchEngines.Text");
			this.btnManageSearchEngines.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageSearchEngines.TextAlign")));
			this.toolTip.SetToolTip(this.btnManageSearchEngines, resources.GetString("btnManageSearchEngines.ToolTip"));
			this.btnManageSearchEngines.Visible = ((bool)(resources.GetObject("btnManageSearchEngines.Visible")));
			this.btnManageSearchEngines.Click += new System.EventHandler(this.btnManageSearchEngines_Click);
			// 
			// lblNewBySearchEngines
			// 
			this.lblNewBySearchEngines.AccessibleDescription = resources.GetString("lblNewBySearchEngines.AccessibleDescription");
			this.lblNewBySearchEngines.AccessibleName = resources.GetString("lblNewBySearchEngines.AccessibleName");
			this.lblNewBySearchEngines.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNewBySearchEngines.Anchor")));
			this.lblNewBySearchEngines.AutoSize = ((bool)(resources.GetObject("lblNewBySearchEngines.AutoSize")));
			this.lblNewBySearchEngines.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNewBySearchEngines.Dock")));
			this.lblNewBySearchEngines.Enabled = ((bool)(resources.GetObject("lblNewBySearchEngines.Enabled")));
			this.lblNewBySearchEngines.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewBySearchEngines.Font = ((System.Drawing.Font)(resources.GetObject("lblNewBySearchEngines.Font")));
			this.lblNewBySearchEngines.Image = ((System.Drawing.Image)(resources.GetObject("lblNewBySearchEngines.Image")));
			this.lblNewBySearchEngines.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewBySearchEngines.ImageAlign")));
			this.lblNewBySearchEngines.ImageIndex = ((int)(resources.GetObject("lblNewBySearchEngines.ImageIndex")));
			this.lblNewBySearchEngines.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNewBySearchEngines.ImeMode")));
			this.lblNewBySearchEngines.Location = ((System.Drawing.Point)(resources.GetObject("lblNewBySearchEngines.Location")));
			this.lblNewBySearchEngines.Name = "lblNewBySearchEngines";
			this.lblNewBySearchEngines.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNewBySearchEngines.RightToLeft")));
			this.lblNewBySearchEngines.Size = ((System.Drawing.Size)(resources.GetObject("lblNewBySearchEngines.Size")));
			this.lblNewBySearchEngines.TabIndex = ((int)(resources.GetObject("lblNewBySearchEngines.TabIndex")));
			this.lblNewBySearchEngines.Text = resources.GetString("lblNewBySearchEngines.Text");
			this.lblNewBySearchEngines.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewBySearchEngines.TextAlign")));
			this.toolTip.SetToolTip(this.lblNewBySearchEngines, resources.GetString("lblNewBySearchEngines.ToolTip"));
			this.lblNewBySearchEngines.Visible = ((bool)(resources.GetObject("lblNewBySearchEngines.Visible")));
			// 
			// cboNewBySearchEngines
			// 
			this.cboNewBySearchEngines.AccessibleDescription = resources.GetString("cboNewBySearchEngines.AccessibleDescription");
			this.cboNewBySearchEngines.AccessibleName = resources.GetString("cboNewBySearchEngines.AccessibleName");
			this.cboNewBySearchEngines.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cboNewBySearchEngines.Anchor")));
			this.cboNewBySearchEngines.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cboNewBySearchEngines.BackgroundImage")));
			this.cboNewBySearchEngines.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cboNewBySearchEngines.Dock")));
			this.cboNewBySearchEngines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboNewBySearchEngines.Enabled = ((bool)(resources.GetObject("cboNewBySearchEngines.Enabled")));
			this.cboNewBySearchEngines.Font = ((System.Drawing.Font)(resources.GetObject("cboNewBySearchEngines.Font")));
			this.cboNewBySearchEngines.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cboNewBySearchEngines.ImeMode")));
			this.cboNewBySearchEngines.IntegralHeight = ((bool)(resources.GetObject("cboNewBySearchEngines.IntegralHeight")));
			this.cboNewBySearchEngines.ItemHeight = ((int)(resources.GetObject("cboNewBySearchEngines.ItemHeight")));
			this.cboNewBySearchEngines.Location = ((System.Drawing.Point)(resources.GetObject("cboNewBySearchEngines.Location")));
			this.cboNewBySearchEngines.MaxDropDownItems = ((int)(resources.GetObject("cboNewBySearchEngines.MaxDropDownItems")));
			this.cboNewBySearchEngines.MaxLength = ((int)(resources.GetObject("cboNewBySearchEngines.MaxLength")));
			this.cboNewBySearchEngines.Name = "cboNewBySearchEngines";
			this.cboNewBySearchEngines.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cboNewBySearchEngines.RightToLeft")));
			this.cboNewBySearchEngines.Size = ((System.Drawing.Size)(resources.GetObject("cboNewBySearchEngines.Size")));
			this.cboNewBySearchEngines.TabIndex = ((int)(resources.GetObject("cboNewBySearchEngines.TabIndex")));
			this.cboNewBySearchEngines.Text = resources.GetString("cboNewBySearchEngines.Text");
			this.toolTip.SetToolTip(this.cboNewBySearchEngines, resources.GetString("cboNewBySearchEngines.ToolTip"));
			this.cboNewBySearchEngines.Visible = ((bool)(resources.GetObject("cboNewBySearchEngines.Visible")));
			// 
			// lblNewBySearchIntro
			// 
			this.lblNewBySearchIntro.AccessibleDescription = resources.GetString("lblNewBySearchIntro.AccessibleDescription");
			this.lblNewBySearchIntro.AccessibleName = resources.GetString("lblNewBySearchIntro.AccessibleName");
			this.lblNewBySearchIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNewBySearchIntro.Anchor")));
			this.lblNewBySearchIntro.AutoSize = ((bool)(resources.GetObject("lblNewBySearchIntro.AutoSize")));
			this.lblNewBySearchIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNewBySearchIntro.Dock")));
			this.lblNewBySearchIntro.Enabled = ((bool)(resources.GetObject("lblNewBySearchIntro.Enabled")));
			this.lblNewBySearchIntro.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewBySearchIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblNewBySearchIntro.Font")));
			this.lblNewBySearchIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblNewBySearchIntro.Image")));
			this.lblNewBySearchIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewBySearchIntro.ImageAlign")));
			this.lblNewBySearchIntro.ImageIndex = ((int)(resources.GetObject("lblNewBySearchIntro.ImageIndex")));
			this.lblNewBySearchIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNewBySearchIntro.ImeMode")));
			this.lblNewBySearchIntro.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblNewBySearchIntro.LinkArea")));
			this.lblNewBySearchIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblNewBySearchIntro.Location")));
			this.lblNewBySearchIntro.Name = "lblNewBySearchIntro";
			this.lblNewBySearchIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNewBySearchIntro.RightToLeft")));
			this.lblNewBySearchIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblNewBySearchIntro.Size")));
			this.lblNewBySearchIntro.TabIndex = ((int)(resources.GetObject("lblNewBySearchIntro.TabIndex")));
			this.lblNewBySearchIntro.Tag = "";
			this.lblNewBySearchIntro.Text = resources.GetString("lblNewBySearchIntro.Text");
			this.lblNewBySearchIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewBySearchIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblNewBySearchIntro, resources.GetString("lblNewBySearchIntro.ToolTip"));
			this.lblNewBySearchIntro.Visible = ((bool)(resources.GetObject("lblNewBySearchIntro.Visible")));
			// 
			// txtNewBySearchWords
			// 
			this.txtNewBySearchWords.AccessibleDescription = resources.GetString("txtNewBySearchWords.AccessibleDescription");
			this.txtNewBySearchWords.AccessibleName = resources.GetString("txtNewBySearchWords.AccessibleName");
			this.txtNewBySearchWords.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtNewBySearchWords.Anchor")));
			this.txtNewBySearchWords.AutoSize = ((bool)(resources.GetObject("txtNewBySearchWords.AutoSize")));
			this.txtNewBySearchWords.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtNewBySearchWords.BackgroundImage")));
			this.txtNewBySearchWords.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtNewBySearchWords.Dock")));
			this.txtNewBySearchWords.Enabled = ((bool)(resources.GetObject("txtNewBySearchWords.Enabled")));
			this.txtNewBySearchWords.Font = ((System.Drawing.Font)(resources.GetObject("txtNewBySearchWords.Font")));
			this.txtNewBySearchWords.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtNewBySearchWords.ImeMode")));
			this.txtNewBySearchWords.Location = ((System.Drawing.Point)(resources.GetObject("txtNewBySearchWords.Location")));
			this.txtNewBySearchWords.MaxLength = ((int)(resources.GetObject("txtNewBySearchWords.MaxLength")));
			this.txtNewBySearchWords.Multiline = ((bool)(resources.GetObject("txtNewBySearchWords.Multiline")));
			this.txtNewBySearchWords.Name = "txtNewBySearchWords";
			this.txtNewBySearchWords.PasswordChar = ((char)(resources.GetObject("txtNewBySearchWords.PasswordChar")));
			this.txtNewBySearchWords.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtNewBySearchWords.RightToLeft")));
			this.txtNewBySearchWords.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtNewBySearchWords.ScrollBars")));
			this.txtNewBySearchWords.Size = ((System.Drawing.Size)(resources.GetObject("txtNewBySearchWords.Size")));
			this.txtNewBySearchWords.TabIndex = ((int)(resources.GetObject("txtNewBySearchWords.TabIndex")));
			this.txtNewBySearchWords.Text = resources.GetString("txtNewBySearchWords.Text");
			this.txtNewBySearchWords.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtNewBySearchWords.TextAlign")));
			this.toolTip.SetToolTip(this.txtNewBySearchWords, resources.GetString("txtNewBySearchWords.ToolTip"));
			this.txtNewBySearchWords.Visible = ((bool)(resources.GetObject("txtNewBySearchWords.Visible")));
			this.txtNewBySearchWords.WordWrap = ((bool)(resources.GetObject("txtNewBySearchWords.WordWrap")));
			// 
			// lblNewBySearchWords
			// 
			this.lblNewBySearchWords.AccessibleDescription = resources.GetString("lblNewBySearchWords.AccessibleDescription");
			this.lblNewBySearchWords.AccessibleName = resources.GetString("lblNewBySearchWords.AccessibleName");
			this.lblNewBySearchWords.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNewBySearchWords.Anchor")));
			this.lblNewBySearchWords.AutoSize = ((bool)(resources.GetObject("lblNewBySearchWords.AutoSize")));
			this.lblNewBySearchWords.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNewBySearchWords.Dock")));
			this.lblNewBySearchWords.Enabled = ((bool)(resources.GetObject("lblNewBySearchWords.Enabled")));
			this.lblNewBySearchWords.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewBySearchWords.Font = ((System.Drawing.Font)(resources.GetObject("lblNewBySearchWords.Font")));
			this.lblNewBySearchWords.Image = ((System.Drawing.Image)(resources.GetObject("lblNewBySearchWords.Image")));
			this.lblNewBySearchWords.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewBySearchWords.ImageAlign")));
			this.lblNewBySearchWords.ImageIndex = ((int)(resources.GetObject("lblNewBySearchWords.ImageIndex")));
			this.lblNewBySearchWords.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNewBySearchWords.ImeMode")));
			this.lblNewBySearchWords.Location = ((System.Drawing.Point)(resources.GetObject("lblNewBySearchWords.Location")));
			this.lblNewBySearchWords.Name = "lblNewBySearchWords";
			this.lblNewBySearchWords.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNewBySearchWords.RightToLeft")));
			this.lblNewBySearchWords.Size = ((System.Drawing.Size)(resources.GetObject("lblNewBySearchWords.Size")));
			this.lblNewBySearchWords.TabIndex = ((int)(resources.GetObject("lblNewBySearchWords.TabIndex")));
			this.lblNewBySearchWords.Text = resources.GetString("lblNewBySearchWords.Text");
			this.lblNewBySearchWords.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewBySearchWords.TextAlign")));
			this.toolTip.SetToolTip(this.lblNewBySearchWords, resources.GetString("lblNewBySearchWords.ToolTip"));
			this.lblNewBySearchWords.Visible = ((bool)(resources.GetObject("lblNewBySearchWords.Visible")));
			// 
			// pageValidateUrl
			// 
			this.pageValidateUrl.AccessibleDescription = resources.GetString("pageValidateUrl.AccessibleDescription");
			this.pageValidateUrl.AccessibleName = resources.GetString("pageValidateUrl.AccessibleName");
			this.pageValidateUrl.AllowCancel = false;
			this.pageValidateUrl.AllowMoveNext = false;
			this.pageValidateUrl.AllowMovePrevious = false;
			this.pageValidateUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageValidateUrl.Anchor")));
			this.pageValidateUrl.AutoScroll = ((bool)(resources.GetObject("pageValidateUrl.AutoScroll")));
			this.pageValidateUrl.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageValidateUrl.AutoScrollMargin")));
			this.pageValidateUrl.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageValidateUrl.AutoScrollMinSize")));
			this.pageValidateUrl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageValidateUrl.BackgroundImage")));
			this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage2);
			this.pageValidateUrl.Controls.Add(this.lblValidationTask2);
			this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage1);
			this.pageValidateUrl.Controls.Add(this.lblValidationTask1);
			this.pageValidateUrl.Controls.Add(this.pbar);
			this.pageValidateUrl.Controls.Add(this.lblWaitStepIntro);
			this.pageValidateUrl.Description = resources.GetString("pageValidateUrl.Description");
			this.pageValidateUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageValidateUrl.Dock")));
			this.pageValidateUrl.Enabled = ((bool)(resources.GetObject("pageValidateUrl.Enabled")));
			this.pageValidateUrl.Font = ((System.Drawing.Font)(resources.GetObject("pageValidateUrl.Font")));
			this.pageValidateUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageValidateUrl.ImeMode")));
			this.pageValidateUrl.Location = ((System.Drawing.Point)(resources.GetObject("pageValidateUrl.Location")));
			this.pageValidateUrl.Name = "pageValidateUrl";
			this.pageValidateUrl.NextPage = this.pageFoundMultipleFeeds;
			this.pageValidateUrl.PreviousPage = this.pageNewBySearchTopic;
			this.pageValidateUrl.ProceedText = resources.GetString("pageValidateUrl.ProceedText");
			this.pageValidateUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageValidateUrl.RightToLeft")));
			this.pageValidateUrl.Size = ((System.Drawing.Size)(resources.GetObject("pageValidateUrl.Size")));
			this.pageValidateUrl.TabIndex = ((int)(resources.GetObject("pageValidateUrl.TabIndex")));
			this.pageValidateUrl.Text = resources.GetString("pageValidateUrl.Text");
			this.toolTip.SetToolTip(this.pageValidateUrl, resources.GetString("pageValidateUrl.ToolTip"));
			this.pageValidateUrl.Visible = ((bool)(resources.GetObject("pageValidateUrl.Visible")));
			this.pageValidateUrl.BeforeDisplay += new System.EventHandler(this.OnPageValidation_BeforeDisplay);
			this.pageValidateUrl.AfterDisplay += new System.EventHandler(this.OnPageValidation_AfterDisplay);
			// 
			// lblValidationTaskImage2
			// 
			this.lblValidationTaskImage2.AccessibleDescription = resources.GetString("lblValidationTaskImage2.AccessibleDescription");
			this.lblValidationTaskImage2.AccessibleName = resources.GetString("lblValidationTaskImage2.AccessibleName");
			this.lblValidationTaskImage2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblValidationTaskImage2.Anchor")));
			this.lblValidationTaskImage2.AutoSize = ((bool)(resources.GetObject("lblValidationTaskImage2.AutoSize")));
			this.lblValidationTaskImage2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblValidationTaskImage2.Dock")));
			this.lblValidationTaskImage2.Enabled = ((bool)(resources.GetObject("lblValidationTaskImage2.Enabled")));
			this.lblValidationTaskImage2.Font = ((System.Drawing.Font)(resources.GetObject("lblValidationTaskImage2.Font")));
			this.lblValidationTaskImage2.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTaskImage2.Image")));
			this.lblValidationTaskImage2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTaskImage2.ImageAlign")));
			this.lblValidationTaskImage2.ImageIndex = ((int)(resources.GetObject("lblValidationTaskImage2.ImageIndex")));
			this.lblValidationTaskImage2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblValidationTaskImage2.ImeMode")));
			this.lblValidationTaskImage2.Location = ((System.Drawing.Point)(resources.GetObject("lblValidationTaskImage2.Location")));
			this.lblValidationTaskImage2.Name = "lblValidationTaskImage2";
			this.lblValidationTaskImage2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblValidationTaskImage2.RightToLeft")));
			this.lblValidationTaskImage2.Size = ((System.Drawing.Size)(resources.GetObject("lblValidationTaskImage2.Size")));
			this.lblValidationTaskImage2.TabIndex = ((int)(resources.GetObject("lblValidationTaskImage2.TabIndex")));
			this.lblValidationTaskImage2.Text = resources.GetString("lblValidationTaskImage2.Text");
			this.lblValidationTaskImage2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTaskImage2.TextAlign")));
			this.toolTip.SetToolTip(this.lblValidationTaskImage2, resources.GetString("lblValidationTaskImage2.ToolTip"));
			this.lblValidationTaskImage2.Visible = ((bool)(resources.GetObject("lblValidationTaskImage2.Visible")));
			// 
			// lblValidationTask2
			// 
			this.lblValidationTask2.AccessibleDescription = resources.GetString("lblValidationTask2.AccessibleDescription");
			this.lblValidationTask2.AccessibleName = resources.GetString("lblValidationTask2.AccessibleName");
			this.lblValidationTask2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblValidationTask2.Anchor")));
			this.lblValidationTask2.AutoSize = ((bool)(resources.GetObject("lblValidationTask2.AutoSize")));
			this.lblValidationTask2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblValidationTask2.Dock")));
			this.lblValidationTask2.Enabled = ((bool)(resources.GetObject("lblValidationTask2.Enabled")));
			this.lblValidationTask2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblValidationTask2.Font = ((System.Drawing.Font)(resources.GetObject("lblValidationTask2.Font")));
			this.lblValidationTask2.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTask2.Image")));
			this.lblValidationTask2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTask2.ImageAlign")));
			this.lblValidationTask2.ImageIndex = ((int)(resources.GetObject("lblValidationTask2.ImageIndex")));
			this.lblValidationTask2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblValidationTask2.ImeMode")));
			this.lblValidationTask2.Location = ((System.Drawing.Point)(resources.GetObject("lblValidationTask2.Location")));
			this.lblValidationTask2.Name = "lblValidationTask2";
			this.lblValidationTask2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblValidationTask2.RightToLeft")));
			this.lblValidationTask2.Size = ((System.Drawing.Size)(resources.GetObject("lblValidationTask2.Size")));
			this.lblValidationTask2.TabIndex = ((int)(resources.GetObject("lblValidationTask2.TabIndex")));
			this.lblValidationTask2.Text = resources.GetString("lblValidationTask2.Text");
			this.lblValidationTask2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTask2.TextAlign")));
			this.toolTip.SetToolTip(this.lblValidationTask2, resources.GetString("lblValidationTask2.ToolTip"));
			this.lblValidationTask2.Visible = ((bool)(resources.GetObject("lblValidationTask2.Visible")));
			// 
			// lblValidationTaskImage1
			// 
			this.lblValidationTaskImage1.AccessibleDescription = resources.GetString("lblValidationTaskImage1.AccessibleDescription");
			this.lblValidationTaskImage1.AccessibleName = resources.GetString("lblValidationTaskImage1.AccessibleName");
			this.lblValidationTaskImage1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblValidationTaskImage1.Anchor")));
			this.lblValidationTaskImage1.AutoSize = ((bool)(resources.GetObject("lblValidationTaskImage1.AutoSize")));
			this.lblValidationTaskImage1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblValidationTaskImage1.Dock")));
			this.lblValidationTaskImage1.Enabled = ((bool)(resources.GetObject("lblValidationTaskImage1.Enabled")));
			this.lblValidationTaskImage1.Font = ((System.Drawing.Font)(resources.GetObject("lblValidationTaskImage1.Font")));
			this.lblValidationTaskImage1.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTaskImage1.Image")));
			this.lblValidationTaskImage1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTaskImage1.ImageAlign")));
			this.lblValidationTaskImage1.ImageIndex = ((int)(resources.GetObject("lblValidationTaskImage1.ImageIndex")));
			this.lblValidationTaskImage1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblValidationTaskImage1.ImeMode")));
			this.lblValidationTaskImage1.Location = ((System.Drawing.Point)(resources.GetObject("lblValidationTaskImage1.Location")));
			this.lblValidationTaskImage1.Name = "lblValidationTaskImage1";
			this.lblValidationTaskImage1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblValidationTaskImage1.RightToLeft")));
			this.lblValidationTaskImage1.Size = ((System.Drawing.Size)(resources.GetObject("lblValidationTaskImage1.Size")));
			this.lblValidationTaskImage1.TabIndex = ((int)(resources.GetObject("lblValidationTaskImage1.TabIndex")));
			this.lblValidationTaskImage1.Text = resources.GetString("lblValidationTaskImage1.Text");
			this.lblValidationTaskImage1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTaskImage1.TextAlign")));
			this.toolTip.SetToolTip(this.lblValidationTaskImage1, resources.GetString("lblValidationTaskImage1.ToolTip"));
			this.lblValidationTaskImage1.Visible = ((bool)(resources.GetObject("lblValidationTaskImage1.Visible")));
			// 
			// lblValidationTask1
			// 
			this.lblValidationTask1.AccessibleDescription = resources.GetString("lblValidationTask1.AccessibleDescription");
			this.lblValidationTask1.AccessibleName = resources.GetString("lblValidationTask1.AccessibleName");
			this.lblValidationTask1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblValidationTask1.Anchor")));
			this.lblValidationTask1.AutoSize = ((bool)(resources.GetObject("lblValidationTask1.AutoSize")));
			this.lblValidationTask1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblValidationTask1.Dock")));
			this.lblValidationTask1.Enabled = ((bool)(resources.GetObject("lblValidationTask1.Enabled")));
			this.lblValidationTask1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblValidationTask1.Font = ((System.Drawing.Font)(resources.GetObject("lblValidationTask1.Font")));
			this.lblValidationTask1.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTask1.Image")));
			this.lblValidationTask1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTask1.ImageAlign")));
			this.lblValidationTask1.ImageIndex = ((int)(resources.GetObject("lblValidationTask1.ImageIndex")));
			this.lblValidationTask1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblValidationTask1.ImeMode")));
			this.lblValidationTask1.Location = ((System.Drawing.Point)(resources.GetObject("lblValidationTask1.Location")));
			this.lblValidationTask1.Name = "lblValidationTask1";
			this.lblValidationTask1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblValidationTask1.RightToLeft")));
			this.lblValidationTask1.Size = ((System.Drawing.Size)(resources.GetObject("lblValidationTask1.Size")));
			this.lblValidationTask1.TabIndex = ((int)(resources.GetObject("lblValidationTask1.TabIndex")));
			this.lblValidationTask1.Text = resources.GetString("lblValidationTask1.Text");
			this.lblValidationTask1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblValidationTask1.TextAlign")));
			this.toolTip.SetToolTip(this.lblValidationTask1, resources.GetString("lblValidationTask1.ToolTip"));
			this.lblValidationTask1.Visible = ((bool)(resources.GetObject("lblValidationTask1.Visible")));
			// 
			// pbar
			// 
			this.pbar.AccessibleDescription = resources.GetString("pbar.AccessibleDescription");
			this.pbar.AccessibleName = resources.GetString("pbar.AccessibleName");
			this.pbar.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pbar.Anchor")));
			this.pbar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbar.BackgroundImage")));
			this.pbar.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pbar.Dock")));
			this.pbar.Enabled = ((bool)(resources.GetObject("pbar.Enabled")));
			this.pbar.Font = ((System.Drawing.Font)(resources.GetObject("pbar.Font")));
			this.pbar.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pbar.ImeMode")));
			this.pbar.Location = ((System.Drawing.Point)(resources.GetObject("pbar.Location")));
			this.pbar.Name = "pbar";
			this.pbar.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pbar.RightToLeft")));
			this.pbar.Size = ((System.Drawing.Size)(resources.GetObject("pbar.Size")));
			this.pbar.Step = 2;
			this.pbar.TabIndex = ((int)(resources.GetObject("pbar.TabIndex")));
			this.pbar.Text = resources.GetString("pbar.Text");
			this.toolTip.SetToolTip(this.pbar, resources.GetString("pbar.ToolTip"));
			this.pbar.Visible = ((bool)(resources.GetObject("pbar.Visible")));
			// 
			// lblWaitStepIntro
			// 
			this.lblWaitStepIntro.AccessibleDescription = resources.GetString("lblWaitStepIntro.AccessibleDescription");
			this.lblWaitStepIntro.AccessibleName = resources.GetString("lblWaitStepIntro.AccessibleName");
			this.lblWaitStepIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblWaitStepIntro.Anchor")));
			this.lblWaitStepIntro.AutoSize = ((bool)(resources.GetObject("lblWaitStepIntro.AutoSize")));
			this.lblWaitStepIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblWaitStepIntro.Dock")));
			this.lblWaitStepIntro.Enabled = ((bool)(resources.GetObject("lblWaitStepIntro.Enabled")));
			this.lblWaitStepIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblWaitStepIntro.Font")));
			this.lblWaitStepIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblWaitStepIntro.Image")));
			this.lblWaitStepIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblWaitStepIntro.ImageAlign")));
			this.lblWaitStepIntro.ImageIndex = ((int)(resources.GetObject("lblWaitStepIntro.ImageIndex")));
			this.lblWaitStepIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblWaitStepIntro.ImeMode")));
			this.lblWaitStepIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblWaitStepIntro.Location")));
			this.lblWaitStepIntro.Name = "lblWaitStepIntro";
			this.lblWaitStepIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblWaitStepIntro.RightToLeft")));
			this.lblWaitStepIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblWaitStepIntro.Size")));
			this.lblWaitStepIntro.TabIndex = ((int)(resources.GetObject("lblWaitStepIntro.TabIndex")));
			this.lblWaitStepIntro.Text = resources.GetString("lblWaitStepIntro.Text");
			this.lblWaitStepIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblWaitStepIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblWaitStepIntro, resources.GetString("lblWaitStepIntro.ToolTip"));
			this.lblWaitStepIntro.Visible = ((bool)(resources.GetObject("lblWaitStepIntro.Visible")));
			// 
			// pageFoundMultipleFeeds
			// 
			this.pageFoundMultipleFeeds.AccessibleDescription = resources.GetString("pageFoundMultipleFeeds.AccessibleDescription");
			this.pageFoundMultipleFeeds.AccessibleName = resources.GetString("pageFoundMultipleFeeds.AccessibleName");
			this.pageFoundMultipleFeeds.AllowMoveNext = false;
			this.pageFoundMultipleFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageFoundMultipleFeeds.Anchor")));
			this.pageFoundMultipleFeeds.AutoScroll = ((bool)(resources.GetObject("pageFoundMultipleFeeds.AutoScroll")));
			this.pageFoundMultipleFeeds.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageFoundMultipleFeeds.AutoScrollMargin")));
			this.pageFoundMultipleFeeds.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageFoundMultipleFeeds.AutoScrollMinSize")));
			this.pageFoundMultipleFeeds.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageFoundMultipleFeeds.BackgroundImage")));
			this.pageFoundMultipleFeeds.Controls.Add(this.listFeeds);
			this.pageFoundMultipleFeeds.Controls.Add(this.lblMultipleFeedsFoundHint1);
			this.pageFoundMultipleFeeds.Controls.Add(this.lblMultipleFeedsFoundHint2);
			this.pageFoundMultipleFeeds.Controls.Add(this.lblMultipleFeedsFound);
			this.pageFoundMultipleFeeds.Description = resources.GetString("pageFoundMultipleFeeds.Description");
			this.pageFoundMultipleFeeds.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageFoundMultipleFeeds.Dock")));
			this.pageFoundMultipleFeeds.Enabled = ((bool)(resources.GetObject("pageFoundMultipleFeeds.Enabled")));
			this.pageFoundMultipleFeeds.Font = ((System.Drawing.Font)(resources.GetObject("pageFoundMultipleFeeds.Font")));
			this.pageFoundMultipleFeeds.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageFoundMultipleFeeds.ImeMode")));
			this.pageFoundMultipleFeeds.Location = ((System.Drawing.Point)(resources.GetObject("pageFoundMultipleFeeds.Location")));
			this.pageFoundMultipleFeeds.Name = "pageFoundMultipleFeeds";
			this.pageFoundMultipleFeeds.NextPage = this.pageTitleCategory;
			this.pageFoundMultipleFeeds.PreviousPage = this.pageNewBySearchTopic;
			this.pageFoundMultipleFeeds.ProceedText = resources.GetString("pageFoundMultipleFeeds.ProceedText");
			this.pageFoundMultipleFeeds.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageFoundMultipleFeeds.RightToLeft")));
			this.pageFoundMultipleFeeds.Size = ((System.Drawing.Size)(resources.GetObject("pageFoundMultipleFeeds.Size")));
			this.pageFoundMultipleFeeds.TabIndex = ((int)(resources.GetObject("pageFoundMultipleFeeds.TabIndex")));
			this.pageFoundMultipleFeeds.Text = resources.GetString("pageFoundMultipleFeeds.Text");
			this.toolTip.SetToolTip(this.pageFoundMultipleFeeds, resources.GetString("pageFoundMultipleFeeds.ToolTip"));
			this.pageFoundMultipleFeeds.Visible = ((bool)(resources.GetObject("pageFoundMultipleFeeds.Visible")));
			this.pageFoundMultipleFeeds.BeforeMoveNext += new Divelements.WizardFramework.WizardPageEventHandler(this.OnMultipleFeedsBeforeMoveNext);
			// 
			// listFeeds
			// 
			this.listFeeds.AccessibleDescription = resources.GetString("listFeeds.AccessibleDescription");
			this.listFeeds.AccessibleName = resources.GetString("listFeeds.AccessibleName");
			this.listFeeds.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listFeeds.Alignment")));
			this.listFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listFeeds.Anchor")));
			this.listFeeds.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listFeeds.BackgroundImage")));
			this.listFeeds.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listFeeds.Dock")));
			this.listFeeds.Enabled = ((bool)(resources.GetObject("listFeeds.Enabled")));
			this.listFeeds.Font = ((System.Drawing.Font)(resources.GetObject("listFeeds.Font")));
			this.listFeeds.FullRowSelect = true;
			this.listFeeds.HideSelection = false;
			this.listFeeds.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listFeeds.ImeMode")));
			this.listFeeds.LabelWrap = ((bool)(resources.GetObject("listFeeds.LabelWrap")));
			this.listFeeds.Location = ((System.Drawing.Point)(resources.GetObject("listFeeds.Location")));
			this.listFeeds.Name = "listFeeds";
			this.listFeeds.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listFeeds.RightToLeft")));
			this.listFeeds.Size = ((System.Drawing.Size)(resources.GetObject("listFeeds.Size")));
			this.listFeeds.TabIndex = ((int)(resources.GetObject("listFeeds.TabIndex")));
			this.listFeeds.Text = resources.GetString("listFeeds.Text");
			this.toolTip.SetToolTip(this.listFeeds, resources.GetString("listFeeds.ToolTip"));
			this.listFeeds.View = System.Windows.Forms.View.Details;
			this.listFeeds.Visible = ((bool)(resources.GetObject("listFeeds.Visible")));
			this.listFeeds.DoubleClick += new System.EventHandler(this.OnListFoundFeeds_DoubleClick);
			this.listFeeds.SelectedIndexChanged += new System.EventHandler(this.OnFoundFeedsListSelectedIndexChanged);
			// 
			// lblMultipleFeedsFoundHint1
			// 
			this.lblMultipleFeedsFoundHint1.AccessibleDescription = resources.GetString("lblMultipleFeedsFoundHint1.AccessibleDescription");
			this.lblMultipleFeedsFoundHint1.AccessibleName = resources.GetString("lblMultipleFeedsFoundHint1.AccessibleName");
			this.lblMultipleFeedsFoundHint1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblMultipleFeedsFoundHint1.Anchor")));
			this.lblMultipleFeedsFoundHint1.AutoSize = ((bool)(resources.GetObject("lblMultipleFeedsFoundHint1.AutoSize")));
			this.lblMultipleFeedsFoundHint1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblMultipleFeedsFoundHint1.Dock")));
			this.lblMultipleFeedsFoundHint1.Enabled = ((bool)(resources.GetObject("lblMultipleFeedsFoundHint1.Enabled")));
			this.lblMultipleFeedsFoundHint1.Font = ((System.Drawing.Font)(resources.GetObject("lblMultipleFeedsFoundHint1.Font")));
			this.lblMultipleFeedsFoundHint1.Image = ((System.Drawing.Image)(resources.GetObject("lblMultipleFeedsFoundHint1.Image")));
			this.lblMultipleFeedsFoundHint1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMultipleFeedsFoundHint1.ImageAlign")));
			this.lblMultipleFeedsFoundHint1.ImageIndex = ((int)(resources.GetObject("lblMultipleFeedsFoundHint1.ImageIndex")));
			this.lblMultipleFeedsFoundHint1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblMultipleFeedsFoundHint1.ImeMode")));
			this.lblMultipleFeedsFoundHint1.Location = ((System.Drawing.Point)(resources.GetObject("lblMultipleFeedsFoundHint1.Location")));
			this.lblMultipleFeedsFoundHint1.Name = "lblMultipleFeedsFoundHint1";
			this.lblMultipleFeedsFoundHint1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblMultipleFeedsFoundHint1.RightToLeft")));
			this.lblMultipleFeedsFoundHint1.Size = ((System.Drawing.Size)(resources.GetObject("lblMultipleFeedsFoundHint1.Size")));
			this.lblMultipleFeedsFoundHint1.TabIndex = ((int)(resources.GetObject("lblMultipleFeedsFoundHint1.TabIndex")));
			this.lblMultipleFeedsFoundHint1.Text = resources.GetString("lblMultipleFeedsFoundHint1.Text");
			this.lblMultipleFeedsFoundHint1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMultipleFeedsFoundHint1.TextAlign")));
			this.toolTip.SetToolTip(this.lblMultipleFeedsFoundHint1, resources.GetString("lblMultipleFeedsFoundHint1.ToolTip"));
			this.lblMultipleFeedsFoundHint1.Visible = ((bool)(resources.GetObject("lblMultipleFeedsFoundHint1.Visible")));
			// 
			// lblMultipleFeedsFoundHint2
			// 
			this.lblMultipleFeedsFoundHint2.AccessibleDescription = resources.GetString("lblMultipleFeedsFoundHint2.AccessibleDescription");
			this.lblMultipleFeedsFoundHint2.AccessibleName = resources.GetString("lblMultipleFeedsFoundHint2.AccessibleName");
			this.lblMultipleFeedsFoundHint2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblMultipleFeedsFoundHint2.Anchor")));
			this.lblMultipleFeedsFoundHint2.AutoSize = ((bool)(resources.GetObject("lblMultipleFeedsFoundHint2.AutoSize")));
			this.lblMultipleFeedsFoundHint2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblMultipleFeedsFoundHint2.Dock")));
			this.lblMultipleFeedsFoundHint2.Enabled = ((bool)(resources.GetObject("lblMultipleFeedsFoundHint2.Enabled")));
			this.lblMultipleFeedsFoundHint2.Font = ((System.Drawing.Font)(resources.GetObject("lblMultipleFeedsFoundHint2.Font")));
			this.lblMultipleFeedsFoundHint2.Image = ((System.Drawing.Image)(resources.GetObject("lblMultipleFeedsFoundHint2.Image")));
			this.lblMultipleFeedsFoundHint2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMultipleFeedsFoundHint2.ImageAlign")));
			this.lblMultipleFeedsFoundHint2.ImageIndex = ((int)(resources.GetObject("lblMultipleFeedsFoundHint2.ImageIndex")));
			this.lblMultipleFeedsFoundHint2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblMultipleFeedsFoundHint2.ImeMode")));
			this.lblMultipleFeedsFoundHint2.Location = ((System.Drawing.Point)(resources.GetObject("lblMultipleFeedsFoundHint2.Location")));
			this.lblMultipleFeedsFoundHint2.Name = "lblMultipleFeedsFoundHint2";
			this.lblMultipleFeedsFoundHint2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblMultipleFeedsFoundHint2.RightToLeft")));
			this.lblMultipleFeedsFoundHint2.Size = ((System.Drawing.Size)(resources.GetObject("lblMultipleFeedsFoundHint2.Size")));
			this.lblMultipleFeedsFoundHint2.TabIndex = ((int)(resources.GetObject("lblMultipleFeedsFoundHint2.TabIndex")));
			this.lblMultipleFeedsFoundHint2.Text = resources.GetString("lblMultipleFeedsFoundHint2.Text");
			this.lblMultipleFeedsFoundHint2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMultipleFeedsFoundHint2.TextAlign")));
			this.toolTip.SetToolTip(this.lblMultipleFeedsFoundHint2, resources.GetString("lblMultipleFeedsFoundHint2.ToolTip"));
			this.lblMultipleFeedsFoundHint2.Visible = ((bool)(resources.GetObject("lblMultipleFeedsFoundHint2.Visible")));
			// 
			// lblMultipleFeedsFound
			// 
			this.lblMultipleFeedsFound.AccessibleDescription = resources.GetString("lblMultipleFeedsFound.AccessibleDescription");
			this.lblMultipleFeedsFound.AccessibleName = resources.GetString("lblMultipleFeedsFound.AccessibleName");
			this.lblMultipleFeedsFound.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblMultipleFeedsFound.Anchor")));
			this.lblMultipleFeedsFound.AutoSize = ((bool)(resources.GetObject("lblMultipleFeedsFound.AutoSize")));
			this.lblMultipleFeedsFound.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblMultipleFeedsFound.Dock")));
			this.lblMultipleFeedsFound.Enabled = ((bool)(resources.GetObject("lblMultipleFeedsFound.Enabled")));
			this.lblMultipleFeedsFound.Font = ((System.Drawing.Font)(resources.GetObject("lblMultipleFeedsFound.Font")));
			this.lblMultipleFeedsFound.Image = ((System.Drawing.Image)(resources.GetObject("lblMultipleFeedsFound.Image")));
			this.lblMultipleFeedsFound.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMultipleFeedsFound.ImageAlign")));
			this.lblMultipleFeedsFound.ImageIndex = ((int)(resources.GetObject("lblMultipleFeedsFound.ImageIndex")));
			this.lblMultipleFeedsFound.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblMultipleFeedsFound.ImeMode")));
			this.lblMultipleFeedsFound.Location = ((System.Drawing.Point)(resources.GetObject("lblMultipleFeedsFound.Location")));
			this.lblMultipleFeedsFound.Name = "lblMultipleFeedsFound";
			this.lblMultipleFeedsFound.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblMultipleFeedsFound.RightToLeft")));
			this.lblMultipleFeedsFound.Size = ((System.Drawing.Size)(resources.GetObject("lblMultipleFeedsFound.Size")));
			this.lblMultipleFeedsFound.TabIndex = ((int)(resources.GetObject("lblMultipleFeedsFound.TabIndex")));
			this.lblMultipleFeedsFound.Text = resources.GetString("lblMultipleFeedsFound.Text");
			this.lblMultipleFeedsFound.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMultipleFeedsFound.TextAlign")));
			this.toolTip.SetToolTip(this.lblMultipleFeedsFound, resources.GetString("lblMultipleFeedsFound.ToolTip"));
			this.lblMultipleFeedsFound.Visible = ((bool)(resources.GetObject("lblMultipleFeedsFound.Visible")));
			// 
			// pageTitleCategory
			// 
			this.pageTitleCategory.AccessibleDescription = resources.GetString("pageTitleCategory.AccessibleDescription");
			this.pageTitleCategory.AccessibleName = resources.GetString("pageTitleCategory.AccessibleName");
			this.pageTitleCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageTitleCategory.Anchor")));
			this.pageTitleCategory.AutoScroll = ((bool)(resources.GetObject("pageTitleCategory.AutoScroll")));
			this.pageTitleCategory.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageTitleCategory.AutoScrollMargin")));
			this.pageTitleCategory.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageTitleCategory.AutoScrollMinSize")));
			this.pageTitleCategory.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageTitleCategory.BackgroundImage")));
			this.pageTitleCategory.Controls.Add(this.lblFeedCategory);
			this.pageTitleCategory.Controls.Add(this.cboFeedCategory);
			this.pageTitleCategory.Controls.Add(this.lblPageTitleCredentialsIntro);
			this.pageTitleCategory.Controls.Add(this.lblFeedTitle);
			this.pageTitleCategory.Controls.Add(this.txtFeedTitle);
			this.pageTitleCategory.Description = resources.GetString("pageTitleCategory.Description");
			this.pageTitleCategory.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageTitleCategory.Dock")));
			this.pageTitleCategory.Enabled = ((bool)(resources.GetObject("pageTitleCategory.Enabled")));
			this.pageTitleCategory.Font = ((System.Drawing.Font)(resources.GetObject("pageTitleCategory.Font")));
			this.pageTitleCategory.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageTitleCategory.ImeMode")));
			this.pageTitleCategory.Location = ((System.Drawing.Point)(resources.GetObject("pageTitleCategory.Location")));
			this.pageTitleCategory.Name = "pageTitleCategory";
			this.pageTitleCategory.NextPage = this.pageFeedCredentials;
			this.pageTitleCategory.PreviousPage = this.pageFoundMultipleFeeds;
			this.pageTitleCategory.ProceedText = resources.GetString("pageTitleCategory.ProceedText");
			this.pageTitleCategory.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageTitleCategory.RightToLeft")));
			this.pageTitleCategory.Size = ((System.Drawing.Size)(resources.GetObject("pageTitleCategory.Size")));
			this.pageTitleCategory.TabIndex = ((int)(resources.GetObject("pageTitleCategory.TabIndex")));
			this.pageTitleCategory.Text = resources.GetString("pageTitleCategory.Text");
			this.toolTip.SetToolTip(this.pageTitleCategory, resources.GetString("pageTitleCategory.ToolTip"));
			this.pageTitleCategory.Visible = ((bool)(resources.GetObject("pageTitleCategory.Visible")));
			this.pageTitleCategory.BeforeMoveBack += new Divelements.WizardFramework.WizardPageEventHandler(this.OnPageTitleCategoryBeforeMoveBack);
			this.pageTitleCategory.BeforeDisplay += new System.EventHandler(this.OnPageTitleCategoryBeforeDisplay);
			// 
			// lblFeedCategory
			// 
			this.lblFeedCategory.AccessibleDescription = resources.GetString("lblFeedCategory.AccessibleDescription");
			this.lblFeedCategory.AccessibleName = resources.GetString("lblFeedCategory.AccessibleName");
			this.lblFeedCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFeedCategory.Anchor")));
			this.lblFeedCategory.AutoSize = ((bool)(resources.GetObject("lblFeedCategory.AutoSize")));
			this.lblFeedCategory.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFeedCategory.Dock")));
			this.lblFeedCategory.Enabled = ((bool)(resources.GetObject("lblFeedCategory.Enabled")));
			this.lblFeedCategory.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFeedCategory.Font = ((System.Drawing.Font)(resources.GetObject("lblFeedCategory.Font")));
			this.lblFeedCategory.Image = ((System.Drawing.Image)(resources.GetObject("lblFeedCategory.Image")));
			this.lblFeedCategory.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedCategory.ImageAlign")));
			this.lblFeedCategory.ImageIndex = ((int)(resources.GetObject("lblFeedCategory.ImageIndex")));
			this.lblFeedCategory.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFeedCategory.ImeMode")));
			this.lblFeedCategory.Location = ((System.Drawing.Point)(resources.GetObject("lblFeedCategory.Location")));
			this.lblFeedCategory.Name = "lblFeedCategory";
			this.lblFeedCategory.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFeedCategory.RightToLeft")));
			this.lblFeedCategory.Size = ((System.Drawing.Size)(resources.GetObject("lblFeedCategory.Size")));
			this.lblFeedCategory.TabIndex = ((int)(resources.GetObject("lblFeedCategory.TabIndex")));
			this.lblFeedCategory.Text = resources.GetString("lblFeedCategory.Text");
			this.lblFeedCategory.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedCategory.TextAlign")));
			this.toolTip.SetToolTip(this.lblFeedCategory, resources.GetString("lblFeedCategory.ToolTip"));
			this.lblFeedCategory.Visible = ((bool)(resources.GetObject("lblFeedCategory.Visible")));
			// 
			// cboFeedCategory
			// 
			this.cboFeedCategory.AccessibleDescription = resources.GetString("cboFeedCategory.AccessibleDescription");
			this.cboFeedCategory.AccessibleName = resources.GetString("cboFeedCategory.AccessibleName");
			this.cboFeedCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cboFeedCategory.Anchor")));
			this.cboFeedCategory.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cboFeedCategory.BackgroundImage")));
			this.cboFeedCategory.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cboFeedCategory.Dock")));
			this.cboFeedCategory.Enabled = ((bool)(resources.GetObject("cboFeedCategory.Enabled")));
			this.cboFeedCategory.Font = ((System.Drawing.Font)(resources.GetObject("cboFeedCategory.Font")));
			this.cboFeedCategory.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cboFeedCategory.ImeMode")));
			this.cboFeedCategory.IntegralHeight = ((bool)(resources.GetObject("cboFeedCategory.IntegralHeight")));
			this.cboFeedCategory.ItemHeight = ((int)(resources.GetObject("cboFeedCategory.ItemHeight")));
			this.cboFeedCategory.Location = ((System.Drawing.Point)(resources.GetObject("cboFeedCategory.Location")));
			this.cboFeedCategory.MaxDropDownItems = ((int)(resources.GetObject("cboFeedCategory.MaxDropDownItems")));
			this.cboFeedCategory.MaxLength = ((int)(resources.GetObject("cboFeedCategory.MaxLength")));
			this.cboFeedCategory.Name = "cboFeedCategory";
			this.cboFeedCategory.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cboFeedCategory.RightToLeft")));
			this.cboFeedCategory.Size = ((System.Drawing.Size)(resources.GetObject("cboFeedCategory.Size")));
			this.cboFeedCategory.Sorted = true;
			this.cboFeedCategory.TabIndex = ((int)(resources.GetObject("cboFeedCategory.TabIndex")));
			this.cboFeedCategory.Text = resources.GetString("cboFeedCategory.Text");
			this.toolTip.SetToolTip(this.cboFeedCategory, resources.GetString("cboFeedCategory.ToolTip"));
			this.cboFeedCategory.Visible = ((bool)(resources.GetObject("cboFeedCategory.Visible")));
			// 
			// lblPageTitleCredentialsIntro
			// 
			this.lblPageTitleCredentialsIntro.AccessibleDescription = resources.GetString("lblPageTitleCredentialsIntro.AccessibleDescription");
			this.lblPageTitleCredentialsIntro.AccessibleName = resources.GetString("lblPageTitleCredentialsIntro.AccessibleName");
			this.lblPageTitleCredentialsIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblPageTitleCredentialsIntro.Anchor")));
			this.lblPageTitleCredentialsIntro.AutoSize = ((bool)(resources.GetObject("lblPageTitleCredentialsIntro.AutoSize")));
			this.lblPageTitleCredentialsIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblPageTitleCredentialsIntro.Dock")));
			this.lblPageTitleCredentialsIntro.Enabled = ((bool)(resources.GetObject("lblPageTitleCredentialsIntro.Enabled")));
			this.lblPageTitleCredentialsIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblPageTitleCredentialsIntro.Font")));
			this.lblPageTitleCredentialsIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblPageTitleCredentialsIntro.Image")));
			this.lblPageTitleCredentialsIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPageTitleCredentialsIntro.ImageAlign")));
			this.lblPageTitleCredentialsIntro.ImageIndex = ((int)(resources.GetObject("lblPageTitleCredentialsIntro.ImageIndex")));
			this.lblPageTitleCredentialsIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblPageTitleCredentialsIntro.ImeMode")));
			this.lblPageTitleCredentialsIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblPageTitleCredentialsIntro.Location")));
			this.lblPageTitleCredentialsIntro.Name = "lblPageTitleCredentialsIntro";
			this.lblPageTitleCredentialsIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblPageTitleCredentialsIntro.RightToLeft")));
			this.lblPageTitleCredentialsIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblPageTitleCredentialsIntro.Size")));
			this.lblPageTitleCredentialsIntro.TabIndex = ((int)(resources.GetObject("lblPageTitleCredentialsIntro.TabIndex")));
			this.lblPageTitleCredentialsIntro.Text = resources.GetString("lblPageTitleCredentialsIntro.Text");
			this.lblPageTitleCredentialsIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPageTitleCredentialsIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblPageTitleCredentialsIntro, resources.GetString("lblPageTitleCredentialsIntro.ToolTip"));
			this.lblPageTitleCredentialsIntro.Visible = ((bool)(resources.GetObject("lblPageTitleCredentialsIntro.Visible")));
			// 
			// lblFeedTitle
			// 
			this.lblFeedTitle.AccessibleDescription = resources.GetString("lblFeedTitle.AccessibleDescription");
			this.lblFeedTitle.AccessibleName = resources.GetString("lblFeedTitle.AccessibleName");
			this.lblFeedTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFeedTitle.Anchor")));
			this.lblFeedTitle.AutoSize = ((bool)(resources.GetObject("lblFeedTitle.AutoSize")));
			this.lblFeedTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFeedTitle.Dock")));
			this.lblFeedTitle.Enabled = ((bool)(resources.GetObject("lblFeedTitle.Enabled")));
			this.lblFeedTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFeedTitle.Font = ((System.Drawing.Font)(resources.GetObject("lblFeedTitle.Font")));
			this.lblFeedTitle.Image = ((System.Drawing.Image)(resources.GetObject("lblFeedTitle.Image")));
			this.lblFeedTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedTitle.ImageAlign")));
			this.lblFeedTitle.ImageIndex = ((int)(resources.GetObject("lblFeedTitle.ImageIndex")));
			this.lblFeedTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFeedTitle.ImeMode")));
			this.lblFeedTitle.Location = ((System.Drawing.Point)(resources.GetObject("lblFeedTitle.Location")));
			this.lblFeedTitle.Name = "lblFeedTitle";
			this.lblFeedTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFeedTitle.RightToLeft")));
			this.lblFeedTitle.Size = ((System.Drawing.Size)(resources.GetObject("lblFeedTitle.Size")));
			this.lblFeedTitle.TabIndex = ((int)(resources.GetObject("lblFeedTitle.TabIndex")));
			this.lblFeedTitle.Text = resources.GetString("lblFeedTitle.Text");
			this.lblFeedTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedTitle.TextAlign")));
			this.toolTip.SetToolTip(this.lblFeedTitle, resources.GetString("lblFeedTitle.ToolTip"));
			this.lblFeedTitle.Visible = ((bool)(resources.GetObject("lblFeedTitle.Visible")));
			// 
			// txtFeedTitle
			// 
			this.txtFeedTitle.AccessibleDescription = resources.GetString("txtFeedTitle.AccessibleDescription");
			this.txtFeedTitle.AccessibleName = resources.GetString("txtFeedTitle.AccessibleName");
			this.txtFeedTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtFeedTitle.Anchor")));
			this.txtFeedTitle.AutoSize = ((bool)(resources.GetObject("txtFeedTitle.AutoSize")));
			this.txtFeedTitle.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtFeedTitle.BackgroundImage")));
			this.txtFeedTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtFeedTitle.Dock")));
			this.txtFeedTitle.Enabled = ((bool)(resources.GetObject("txtFeedTitle.Enabled")));
			this.txtFeedTitle.Font = ((System.Drawing.Font)(resources.GetObject("txtFeedTitle.Font")));
			this.txtFeedTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtFeedTitle.ImeMode")));
			this.txtFeedTitle.Location = ((System.Drawing.Point)(resources.GetObject("txtFeedTitle.Location")));
			this.txtFeedTitle.MaxLength = ((int)(resources.GetObject("txtFeedTitle.MaxLength")));
			this.txtFeedTitle.Multiline = ((bool)(resources.GetObject("txtFeedTitle.Multiline")));
			this.txtFeedTitle.Name = "txtFeedTitle";
			this.txtFeedTitle.PasswordChar = ((char)(resources.GetObject("txtFeedTitle.PasswordChar")));
			this.txtFeedTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtFeedTitle.RightToLeft")));
			this.txtFeedTitle.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtFeedTitle.ScrollBars")));
			this.txtFeedTitle.Size = ((System.Drawing.Size)(resources.GetObject("txtFeedTitle.Size")));
			this.txtFeedTitle.TabIndex = ((int)(resources.GetObject("txtFeedTitle.TabIndex")));
			this.txtFeedTitle.Text = resources.GetString("txtFeedTitle.Text");
			this.txtFeedTitle.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtFeedTitle.TextAlign")));
			this.toolTip.SetToolTip(this.txtFeedTitle, resources.GetString("txtFeedTitle.ToolTip"));
			this.txtFeedTitle.Visible = ((bool)(resources.GetObject("txtFeedTitle.Visible")));
			this.txtFeedTitle.WordWrap = ((bool)(resources.GetObject("txtFeedTitle.WordWrap")));
			this.txtFeedTitle.TextChanged += new System.EventHandler(this.OnNewFeedTitleTextChanged);
			// 
			// pageFeedCredentials
			// 
			this.pageFeedCredentials.AccessibleDescription = resources.GetString("pageFeedCredentials.AccessibleDescription");
			this.pageFeedCredentials.AccessibleName = resources.GetString("pageFeedCredentials.AccessibleName");
			this.pageFeedCredentials.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageFeedCredentials.Anchor")));
			this.pageFeedCredentials.AutoScroll = ((bool)(resources.GetObject("pageFeedCredentials.AutoScroll")));
			this.pageFeedCredentials.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageFeedCredentials.AutoScrollMargin")));
			this.pageFeedCredentials.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageFeedCredentials.AutoScrollMinSize")));
			this.pageFeedCredentials.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageFeedCredentials.BackgroundImage")));
			this.pageFeedCredentials.Controls.Add(this.lblFeedCredentialsIntro);
			this.pageFeedCredentials.Controls.Add(this.lblUsername);
			this.pageFeedCredentials.Controls.Add(this.textUser);
			this.pageFeedCredentials.Controls.Add(this.lblPassword);
			this.pageFeedCredentials.Controls.Add(this.textPassword);
			this.pageFeedCredentials.Description = resources.GetString("pageFeedCredentials.Description");
			this.pageFeedCredentials.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageFeedCredentials.Dock")));
			this.pageFeedCredentials.Enabled = ((bool)(resources.GetObject("pageFeedCredentials.Enabled")));
			this.pageFeedCredentials.Font = ((System.Drawing.Font)(resources.GetObject("pageFeedCredentials.Font")));
			this.pageFeedCredentials.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageFeedCredentials.ImeMode")));
			this.pageFeedCredentials.Location = ((System.Drawing.Point)(resources.GetObject("pageFeedCredentials.Location")));
			this.pageFeedCredentials.Name = "pageFeedCredentials";
			this.pageFeedCredentials.NextPage = this.pageFeedItemControl;
			this.pageFeedCredentials.PreviousPage = this.pageTitleCategory;
			this.pageFeedCredentials.ProceedText = resources.GetString("pageFeedCredentials.ProceedText");
			this.pageFeedCredentials.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageFeedCredentials.RightToLeft")));
			this.pageFeedCredentials.Size = ((System.Drawing.Size)(resources.GetObject("pageFeedCredentials.Size")));
			this.pageFeedCredentials.TabIndex = ((int)(resources.GetObject("pageFeedCredentials.TabIndex")));
			this.pageFeedCredentials.Text = resources.GetString("pageFeedCredentials.Text");
			this.toolTip.SetToolTip(this.pageFeedCredentials, resources.GetString("pageFeedCredentials.ToolTip"));
			this.pageFeedCredentials.Visible = ((bool)(resources.GetObject("pageFeedCredentials.Visible")));
			this.pageFeedCredentials.BeforeDisplay += new System.EventHandler(this.pageFeedCredentials_BeforeDisplay);
			// 
			// lblFeedCredentialsIntro
			// 
			this.lblFeedCredentialsIntro.AccessibleDescription = resources.GetString("lblFeedCredentialsIntro.AccessibleDescription");
			this.lblFeedCredentialsIntro.AccessibleName = resources.GetString("lblFeedCredentialsIntro.AccessibleName");
			this.lblFeedCredentialsIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFeedCredentialsIntro.Anchor")));
			this.lblFeedCredentialsIntro.AutoSize = ((bool)(resources.GetObject("lblFeedCredentialsIntro.AutoSize")));
			this.lblFeedCredentialsIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFeedCredentialsIntro.Dock")));
			this.lblFeedCredentialsIntro.Enabled = ((bool)(resources.GetObject("lblFeedCredentialsIntro.Enabled")));
			this.lblFeedCredentialsIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblFeedCredentialsIntro.Font")));
			this.lblFeedCredentialsIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblFeedCredentialsIntro.Image")));
			this.lblFeedCredentialsIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedCredentialsIntro.ImageAlign")));
			this.lblFeedCredentialsIntro.ImageIndex = ((int)(resources.GetObject("lblFeedCredentialsIntro.ImageIndex")));
			this.lblFeedCredentialsIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFeedCredentialsIntro.ImeMode")));
			this.lblFeedCredentialsIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblFeedCredentialsIntro.Location")));
			this.lblFeedCredentialsIntro.Name = "lblFeedCredentialsIntro";
			this.lblFeedCredentialsIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFeedCredentialsIntro.RightToLeft")));
			this.lblFeedCredentialsIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblFeedCredentialsIntro.Size")));
			this.lblFeedCredentialsIntro.TabIndex = ((int)(resources.GetObject("lblFeedCredentialsIntro.TabIndex")));
			this.lblFeedCredentialsIntro.Text = resources.GetString("lblFeedCredentialsIntro.Text");
			this.lblFeedCredentialsIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedCredentialsIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblFeedCredentialsIntro, resources.GetString("lblFeedCredentialsIntro.ToolTip"));
			this.lblFeedCredentialsIntro.Visible = ((bool)(resources.GetObject("lblFeedCredentialsIntro.Visible")));
			// 
			// lblUsername
			// 
			this.lblUsername.AccessibleDescription = resources.GetString("lblUsername.AccessibleDescription");
			this.lblUsername.AccessibleName = resources.GetString("lblUsername.AccessibleName");
			this.lblUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblUsername.Anchor")));
			this.lblUsername.AutoSize = ((bool)(resources.GetObject("lblUsername.AutoSize")));
			this.lblUsername.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblUsername.Dock")));
			this.lblUsername.Enabled = ((bool)(resources.GetObject("lblUsername.Enabled")));
			this.lblUsername.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblUsername.Font = ((System.Drawing.Font)(resources.GetObject("lblUsername.Font")));
			this.lblUsername.Image = ((System.Drawing.Image)(resources.GetObject("lblUsername.Image")));
			this.lblUsername.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUsername.ImageAlign")));
			this.lblUsername.ImageIndex = ((int)(resources.GetObject("lblUsername.ImageIndex")));
			this.lblUsername.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblUsername.ImeMode")));
			this.lblUsername.Location = ((System.Drawing.Point)(resources.GetObject("lblUsername.Location")));
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblUsername.RightToLeft")));
			this.lblUsername.Size = ((System.Drawing.Size)(resources.GetObject("lblUsername.Size")));
			this.lblUsername.TabIndex = ((int)(resources.GetObject("lblUsername.TabIndex")));
			this.lblUsername.Text = resources.GetString("lblUsername.Text");
			this.lblUsername.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUsername.TextAlign")));
			this.toolTip.SetToolTip(this.lblUsername, resources.GetString("lblUsername.ToolTip"));
			this.lblUsername.Visible = ((bool)(resources.GetObject("lblUsername.Visible")));
			// 
			// textUser
			// 
			this.textUser.AccessibleDescription = resources.GetString("textUser.AccessibleDescription");
			this.textUser.AccessibleName = resources.GetString("textUser.AccessibleName");
			this.textUser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textUser.Anchor")));
			this.textUser.AutoSize = ((bool)(resources.GetObject("textUser.AutoSize")));
			this.textUser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textUser.BackgroundImage")));
			this.textUser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textUser.Dock")));
			this.textUser.Enabled = ((bool)(resources.GetObject("textUser.Enabled")));
			this.textUser.Font = ((System.Drawing.Font)(resources.GetObject("textUser.Font")));
			this.textUser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textUser.ImeMode")));
			this.textUser.Location = ((System.Drawing.Point)(resources.GetObject("textUser.Location")));
			this.textUser.MaxLength = ((int)(resources.GetObject("textUser.MaxLength")));
			this.textUser.Multiline = ((bool)(resources.GetObject("textUser.Multiline")));
			this.textUser.Name = "textUser";
			this.textUser.PasswordChar = ((char)(resources.GetObject("textUser.PasswordChar")));
			this.textUser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textUser.RightToLeft")));
			this.textUser.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textUser.ScrollBars")));
			this.textUser.Size = ((System.Drawing.Size)(resources.GetObject("textUser.Size")));
			this.textUser.TabIndex = ((int)(resources.GetObject("textUser.TabIndex")));
			this.textUser.Text = resources.GetString("textUser.Text");
			this.textUser.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textUser.TextAlign")));
			this.toolTip.SetToolTip(this.textUser, resources.GetString("textUser.ToolTip"));
			this.textUser.Visible = ((bool)(resources.GetObject("textUser.Visible")));
			this.textUser.WordWrap = ((bool)(resources.GetObject("textUser.WordWrap")));
			// 
			// lblPassword
			// 
			this.lblPassword.AccessibleDescription = resources.GetString("lblPassword.AccessibleDescription");
			this.lblPassword.AccessibleName = resources.GetString("lblPassword.AccessibleName");
			this.lblPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblPassword.Anchor")));
			this.lblPassword.AutoSize = ((bool)(resources.GetObject("lblPassword.AutoSize")));
			this.lblPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblPassword.Dock")));
			this.lblPassword.Enabled = ((bool)(resources.GetObject("lblPassword.Enabled")));
			this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblPassword.Font = ((System.Drawing.Font)(resources.GetObject("lblPassword.Font")));
			this.lblPassword.Image = ((System.Drawing.Image)(resources.GetObject("lblPassword.Image")));
			this.lblPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPassword.ImageAlign")));
			this.lblPassword.ImageIndex = ((int)(resources.GetObject("lblPassword.ImageIndex")));
			this.lblPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblPassword.ImeMode")));
			this.lblPassword.Location = ((System.Drawing.Point)(resources.GetObject("lblPassword.Location")));
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblPassword.RightToLeft")));
			this.lblPassword.Size = ((System.Drawing.Size)(resources.GetObject("lblPassword.Size")));
			this.lblPassword.TabIndex = ((int)(resources.GetObject("lblPassword.TabIndex")));
			this.lblPassword.Text = resources.GetString("lblPassword.Text");
			this.lblPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPassword.TextAlign")));
			this.toolTip.SetToolTip(this.lblPassword, resources.GetString("lblPassword.ToolTip"));
			this.lblPassword.Visible = ((bool)(resources.GetObject("lblPassword.Visible")));
			// 
			// textPassword
			// 
			this.textPassword.AccessibleDescription = resources.GetString("textPassword.AccessibleDescription");
			this.textPassword.AccessibleName = resources.GetString("textPassword.AccessibleName");
			this.textPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textPassword.Anchor")));
			this.textPassword.AutoSize = ((bool)(resources.GetObject("textPassword.AutoSize")));
			this.textPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textPassword.BackgroundImage")));
			this.textPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textPassword.Dock")));
			this.textPassword.Enabled = ((bool)(resources.GetObject("textPassword.Enabled")));
			this.textPassword.Font = ((System.Drawing.Font)(resources.GetObject("textPassword.Font")));
			this.textPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textPassword.ImeMode")));
			this.textPassword.Location = ((System.Drawing.Point)(resources.GetObject("textPassword.Location")));
			this.textPassword.MaxLength = ((int)(resources.GetObject("textPassword.MaxLength")));
			this.textPassword.Multiline = ((bool)(resources.GetObject("textPassword.Multiline")));
			this.textPassword.Name = "textPassword";
			this.textPassword.PasswordChar = ((char)(resources.GetObject("textPassword.PasswordChar")));
			this.textPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textPassword.RightToLeft")));
			this.textPassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textPassword.ScrollBars")));
			this.textPassword.Size = ((System.Drawing.Size)(resources.GetObject("textPassword.Size")));
			this.textPassword.TabIndex = ((int)(resources.GetObject("textPassword.TabIndex")));
			this.textPassword.Text = resources.GetString("textPassword.Text");
			this.textPassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textPassword.TextAlign")));
			this.toolTip.SetToolTip(this.textPassword, resources.GetString("textPassword.ToolTip"));
			this.textPassword.Visible = ((bool)(resources.GetObject("textPassword.Visible")));
			this.textPassword.WordWrap = ((bool)(resources.GetObject("textPassword.WordWrap")));
			// 
			// pageFeedItemControl
			// 
			this.pageFeedItemControl.AccessibleDescription = resources.GetString("pageFeedItemControl.AccessibleDescription");
			this.pageFeedItemControl.AccessibleName = resources.GetString("pageFeedItemControl.AccessibleName");
			this.pageFeedItemControl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageFeedItemControl.Anchor")));
			this.pageFeedItemControl.AutoScroll = ((bool)(resources.GetObject("pageFeedItemControl.AutoScroll")));
			this.pageFeedItemControl.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageFeedItemControl.AutoScrollMargin")));
			this.pageFeedItemControl.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageFeedItemControl.AutoScrollMinSize")));
			this.pageFeedItemControl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageFeedItemControl.BackgroundImage")));
			this.pageFeedItemControl.Controls.Add(this.comboMaxItemAge);
			this.pageFeedItemControl.Controls.Add(this.cboUpdateFrequency);
			this.pageFeedItemControl.Controls.Add(this.lblRemoveItemsOlderThan);
			this.pageFeedItemControl.Controls.Add(this.lblMinutes);
			this.pageFeedItemControl.Controls.Add(this.lblUpdateFrequency);
			this.pageFeedItemControl.Controls.Add(this.checkMarkItemsReadOnExiting);
			this.pageFeedItemControl.Controls.Add(this.lblFeedItemControlIntro);
			this.pageFeedItemControl.Controls.Add(this.checkEnableAlertOnNewItems);
			this.pageFeedItemControl.Description = resources.GetString("pageFeedItemControl.Description");
			this.pageFeedItemControl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageFeedItemControl.Dock")));
			this.pageFeedItemControl.Enabled = ((bool)(resources.GetObject("pageFeedItemControl.Enabled")));
			this.pageFeedItemControl.Font = ((System.Drawing.Font)(resources.GetObject("pageFeedItemControl.Font")));
			this.pageFeedItemControl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageFeedItemControl.ImeMode")));
			this.pageFeedItemControl.Location = ((System.Drawing.Point)(resources.GetObject("pageFeedItemControl.Location")));
			this.pageFeedItemControl.Name = "pageFeedItemControl";
			this.pageFeedItemControl.NextPage = this.pageFeedItemDisplay;
			this.pageFeedItemControl.PreviousPage = this.pageFeedCredentials;
			this.pageFeedItemControl.ProceedText = resources.GetString("pageFeedItemControl.ProceedText");
			this.pageFeedItemControl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageFeedItemControl.RightToLeft")));
			this.pageFeedItemControl.Size = ((System.Drawing.Size)(resources.GetObject("pageFeedItemControl.Size")));
			this.pageFeedItemControl.TabIndex = ((int)(resources.GetObject("pageFeedItemControl.TabIndex")));
			this.pageFeedItemControl.Text = resources.GetString("pageFeedItemControl.Text");
			this.toolTip.SetToolTip(this.pageFeedItemControl, resources.GetString("pageFeedItemControl.ToolTip"));
			this.pageFeedItemControl.Visible = ((bool)(resources.GetObject("pageFeedItemControl.Visible")));
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
			this.comboMaxItemAge.Font = ((System.Drawing.Font)(resources.GetObject("comboMaxItemAge.Font")));
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
			this.toolTip.SetToolTip(this.comboMaxItemAge, resources.GetString("comboMaxItemAge.ToolTip"));
			this.comboMaxItemAge.Visible = ((bool)(resources.GetObject("comboMaxItemAge.Visible")));
			// 
			// cboUpdateFrequency
			// 
			this.cboUpdateFrequency.AccessibleDescription = resources.GetString("cboUpdateFrequency.AccessibleDescription");
			this.cboUpdateFrequency.AccessibleName = resources.GetString("cboUpdateFrequency.AccessibleName");
			this.cboUpdateFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cboUpdateFrequency.Anchor")));
			this.cboUpdateFrequency.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cboUpdateFrequency.BackgroundImage")));
			this.cboUpdateFrequency.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cboUpdateFrequency.Dock")));
			this.cboUpdateFrequency.Enabled = ((bool)(resources.GetObject("cboUpdateFrequency.Enabled")));
			this.cboUpdateFrequency.Font = ((System.Drawing.Font)(resources.GetObject("cboUpdateFrequency.Font")));
			this.cboUpdateFrequency.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cboUpdateFrequency.ImeMode")));
			this.cboUpdateFrequency.IntegralHeight = ((bool)(resources.GetObject("cboUpdateFrequency.IntegralHeight")));
			this.cboUpdateFrequency.ItemHeight = ((int)(resources.GetObject("cboUpdateFrequency.ItemHeight")));
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
			this.cboUpdateFrequency.Location = ((System.Drawing.Point)(resources.GetObject("cboUpdateFrequency.Location")));
			this.cboUpdateFrequency.MaxDropDownItems = ((int)(resources.GetObject("cboUpdateFrequency.MaxDropDownItems")));
			this.cboUpdateFrequency.MaxLength = ((int)(resources.GetObject("cboUpdateFrequency.MaxLength")));
			this.cboUpdateFrequency.Name = "cboUpdateFrequency";
			this.cboUpdateFrequency.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cboUpdateFrequency.RightToLeft")));
			this.cboUpdateFrequency.Size = ((System.Drawing.Size)(resources.GetObject("cboUpdateFrequency.Size")));
			this.cboUpdateFrequency.TabIndex = ((int)(resources.GetObject("cboUpdateFrequency.TabIndex")));
			this.cboUpdateFrequency.Text = resources.GetString("cboUpdateFrequency.Text");
			this.toolTip.SetToolTip(this.cboUpdateFrequency, resources.GetString("cboUpdateFrequency.ToolTip"));
			this.cboUpdateFrequency.Visible = ((bool)(resources.GetObject("cboUpdateFrequency.Visible")));
			// 
			// lblRemoveItemsOlderThan
			// 
			this.lblRemoveItemsOlderThan.AccessibleDescription = resources.GetString("lblRemoveItemsOlderThan.AccessibleDescription");
			this.lblRemoveItemsOlderThan.AccessibleName = resources.GetString("lblRemoveItemsOlderThan.AccessibleName");
			this.lblRemoveItemsOlderThan.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblRemoveItemsOlderThan.Anchor")));
			this.lblRemoveItemsOlderThan.AutoSize = ((bool)(resources.GetObject("lblRemoveItemsOlderThan.AutoSize")));
			this.lblRemoveItemsOlderThan.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblRemoveItemsOlderThan.Dock")));
			this.lblRemoveItemsOlderThan.Enabled = ((bool)(resources.GetObject("lblRemoveItemsOlderThan.Enabled")));
			this.lblRemoveItemsOlderThan.Font = ((System.Drawing.Font)(resources.GetObject("lblRemoveItemsOlderThan.Font")));
			this.lblRemoveItemsOlderThan.Image = ((System.Drawing.Image)(resources.GetObject("lblRemoveItemsOlderThan.Image")));
			this.lblRemoveItemsOlderThan.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRemoveItemsOlderThan.ImageAlign")));
			this.lblRemoveItemsOlderThan.ImageIndex = ((int)(resources.GetObject("lblRemoveItemsOlderThan.ImageIndex")));
			this.lblRemoveItemsOlderThan.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblRemoveItemsOlderThan.ImeMode")));
			this.lblRemoveItemsOlderThan.Location = ((System.Drawing.Point)(resources.GetObject("lblRemoveItemsOlderThan.Location")));
			this.lblRemoveItemsOlderThan.Name = "lblRemoveItemsOlderThan";
			this.lblRemoveItemsOlderThan.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblRemoveItemsOlderThan.RightToLeft")));
			this.lblRemoveItemsOlderThan.Size = ((System.Drawing.Size)(resources.GetObject("lblRemoveItemsOlderThan.Size")));
			this.lblRemoveItemsOlderThan.TabIndex = ((int)(resources.GetObject("lblRemoveItemsOlderThan.TabIndex")));
			this.lblRemoveItemsOlderThan.Text = resources.GetString("lblRemoveItemsOlderThan.Text");
			this.lblRemoveItemsOlderThan.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRemoveItemsOlderThan.TextAlign")));
			this.toolTip.SetToolTip(this.lblRemoveItemsOlderThan, resources.GetString("lblRemoveItemsOlderThan.ToolTip"));
			this.lblRemoveItemsOlderThan.Visible = ((bool)(resources.GetObject("lblRemoveItemsOlderThan.Visible")));
			// 
			// lblMinutes
			// 
			this.lblMinutes.AccessibleDescription = resources.GetString("lblMinutes.AccessibleDescription");
			this.lblMinutes.AccessibleName = resources.GetString("lblMinutes.AccessibleName");
			this.lblMinutes.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblMinutes.Anchor")));
			this.lblMinutes.AutoSize = ((bool)(resources.GetObject("lblMinutes.AutoSize")));
			this.lblMinutes.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblMinutes.Dock")));
			this.lblMinutes.Enabled = ((bool)(resources.GetObject("lblMinutes.Enabled")));
			this.lblMinutes.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblMinutes.Font = ((System.Drawing.Font)(resources.GetObject("lblMinutes.Font")));
			this.lblMinutes.Image = ((System.Drawing.Image)(resources.GetObject("lblMinutes.Image")));
			this.lblMinutes.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMinutes.ImageAlign")));
			this.lblMinutes.ImageIndex = ((int)(resources.GetObject("lblMinutes.ImageIndex")));
			this.lblMinutes.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblMinutes.ImeMode")));
			this.lblMinutes.Location = ((System.Drawing.Point)(resources.GetObject("lblMinutes.Location")));
			this.lblMinutes.Name = "lblMinutes";
			this.lblMinutes.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblMinutes.RightToLeft")));
			this.lblMinutes.Size = ((System.Drawing.Size)(resources.GetObject("lblMinutes.Size")));
			this.lblMinutes.TabIndex = ((int)(resources.GetObject("lblMinutes.TabIndex")));
			this.lblMinutes.Text = resources.GetString("lblMinutes.Text");
			this.lblMinutes.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMinutes.TextAlign")));
			this.toolTip.SetToolTip(this.lblMinutes, resources.GetString("lblMinutes.ToolTip"));
			this.lblMinutes.Visible = ((bool)(resources.GetObject("lblMinutes.Visible")));
			// 
			// lblUpdateFrequency
			// 
			this.lblUpdateFrequency.AccessibleDescription = resources.GetString("lblUpdateFrequency.AccessibleDescription");
			this.lblUpdateFrequency.AccessibleName = resources.GetString("lblUpdateFrequency.AccessibleName");
			this.lblUpdateFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblUpdateFrequency.Anchor")));
			this.lblUpdateFrequency.AutoSize = ((bool)(resources.GetObject("lblUpdateFrequency.AutoSize")));
			this.lblUpdateFrequency.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblUpdateFrequency.Dock")));
			this.lblUpdateFrequency.Enabled = ((bool)(resources.GetObject("lblUpdateFrequency.Enabled")));
			this.lblUpdateFrequency.Font = ((System.Drawing.Font)(resources.GetObject("lblUpdateFrequency.Font")));
			this.lblUpdateFrequency.Image = ((System.Drawing.Image)(resources.GetObject("lblUpdateFrequency.Image")));
			this.lblUpdateFrequency.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUpdateFrequency.ImageAlign")));
			this.lblUpdateFrequency.ImageIndex = ((int)(resources.GetObject("lblUpdateFrequency.ImageIndex")));
			this.lblUpdateFrequency.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblUpdateFrequency.ImeMode")));
			this.lblUpdateFrequency.Location = ((System.Drawing.Point)(resources.GetObject("lblUpdateFrequency.Location")));
			this.lblUpdateFrequency.Name = "lblUpdateFrequency";
			this.lblUpdateFrequency.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblUpdateFrequency.RightToLeft")));
			this.lblUpdateFrequency.Size = ((System.Drawing.Size)(resources.GetObject("lblUpdateFrequency.Size")));
			this.lblUpdateFrequency.TabIndex = ((int)(resources.GetObject("lblUpdateFrequency.TabIndex")));
			this.lblUpdateFrequency.Text = resources.GetString("lblUpdateFrequency.Text");
			this.lblUpdateFrequency.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUpdateFrequency.TextAlign")));
			this.toolTip.SetToolTip(this.lblUpdateFrequency, resources.GetString("lblUpdateFrequency.ToolTip"));
			this.lblUpdateFrequency.Visible = ((bool)(resources.GetObject("lblUpdateFrequency.Visible")));
			// 
			// checkMarkItemsReadOnExiting
			// 
			this.checkMarkItemsReadOnExiting.AccessibleDescription = resources.GetString("checkMarkItemsReadOnExiting.AccessibleDescription");
			this.checkMarkItemsReadOnExiting.AccessibleName = resources.GetString("checkMarkItemsReadOnExiting.AccessibleName");
			this.checkMarkItemsReadOnExiting.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkMarkItemsReadOnExiting.Anchor")));
			this.checkMarkItemsReadOnExiting.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkMarkItemsReadOnExiting.Appearance")));
			this.checkMarkItemsReadOnExiting.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkMarkItemsReadOnExiting.BackgroundImage")));
			this.checkMarkItemsReadOnExiting.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkMarkItemsReadOnExiting.CheckAlign")));
			this.checkMarkItemsReadOnExiting.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkMarkItemsReadOnExiting.Dock")));
			this.checkMarkItemsReadOnExiting.Enabled = ((bool)(resources.GetObject("checkMarkItemsReadOnExiting.Enabled")));
			this.checkMarkItemsReadOnExiting.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkMarkItemsReadOnExiting.FlatStyle")));
			this.checkMarkItemsReadOnExiting.Font = ((System.Drawing.Font)(resources.GetObject("checkMarkItemsReadOnExiting.Font")));
			this.checkMarkItemsReadOnExiting.Image = ((System.Drawing.Image)(resources.GetObject("checkMarkItemsReadOnExiting.Image")));
			this.checkMarkItemsReadOnExiting.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkMarkItemsReadOnExiting.ImageAlign")));
			this.checkMarkItemsReadOnExiting.ImageIndex = ((int)(resources.GetObject("checkMarkItemsReadOnExiting.ImageIndex")));
			this.checkMarkItemsReadOnExiting.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkMarkItemsReadOnExiting.ImeMode")));
			this.checkMarkItemsReadOnExiting.Location = ((System.Drawing.Point)(resources.GetObject("checkMarkItemsReadOnExiting.Location")));
			this.checkMarkItemsReadOnExiting.Name = "checkMarkItemsReadOnExiting";
			this.checkMarkItemsReadOnExiting.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkMarkItemsReadOnExiting.RightToLeft")));
			this.checkMarkItemsReadOnExiting.Size = ((System.Drawing.Size)(resources.GetObject("checkMarkItemsReadOnExiting.Size")));
			this.checkMarkItemsReadOnExiting.TabIndex = ((int)(resources.GetObject("checkMarkItemsReadOnExiting.TabIndex")));
			this.checkMarkItemsReadOnExiting.Text = resources.GetString("checkMarkItemsReadOnExiting.Text");
			this.checkMarkItemsReadOnExiting.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkMarkItemsReadOnExiting.TextAlign")));
			this.toolTip.SetToolTip(this.checkMarkItemsReadOnExiting, resources.GetString("checkMarkItemsReadOnExiting.ToolTip"));
			this.checkMarkItemsReadOnExiting.Visible = ((bool)(resources.GetObject("checkMarkItemsReadOnExiting.Visible")));
			// 
			// lblFeedItemControlIntro
			// 
			this.lblFeedItemControlIntro.AccessibleDescription = resources.GetString("lblFeedItemControlIntro.AccessibleDescription");
			this.lblFeedItemControlIntro.AccessibleName = resources.GetString("lblFeedItemControlIntro.AccessibleName");
			this.lblFeedItemControlIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFeedItemControlIntro.Anchor")));
			this.lblFeedItemControlIntro.AutoSize = ((bool)(resources.GetObject("lblFeedItemControlIntro.AutoSize")));
			this.lblFeedItemControlIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFeedItemControlIntro.Dock")));
			this.lblFeedItemControlIntro.Enabled = ((bool)(resources.GetObject("lblFeedItemControlIntro.Enabled")));
			this.lblFeedItemControlIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblFeedItemControlIntro.Font")));
			this.lblFeedItemControlIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblFeedItemControlIntro.Image")));
			this.lblFeedItemControlIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedItemControlIntro.ImageAlign")));
			this.lblFeedItemControlIntro.ImageIndex = ((int)(resources.GetObject("lblFeedItemControlIntro.ImageIndex")));
			this.lblFeedItemControlIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFeedItemControlIntro.ImeMode")));
			this.lblFeedItemControlIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblFeedItemControlIntro.Location")));
			this.lblFeedItemControlIntro.Name = "lblFeedItemControlIntro";
			this.lblFeedItemControlIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFeedItemControlIntro.RightToLeft")));
			this.lblFeedItemControlIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblFeedItemControlIntro.Size")));
			this.lblFeedItemControlIntro.TabIndex = ((int)(resources.GetObject("lblFeedItemControlIntro.TabIndex")));
			this.lblFeedItemControlIntro.Text = resources.GetString("lblFeedItemControlIntro.Text");
			this.lblFeedItemControlIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedItemControlIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblFeedItemControlIntro, resources.GetString("lblFeedItemControlIntro.ToolTip"));
			this.lblFeedItemControlIntro.Visible = ((bool)(resources.GetObject("lblFeedItemControlIntro.Visible")));
			// 
			// checkEnableAlertOnNewItems
			// 
			this.checkEnableAlertOnNewItems.AccessibleDescription = resources.GetString("checkEnableAlertOnNewItems.AccessibleDescription");
			this.checkEnableAlertOnNewItems.AccessibleName = resources.GetString("checkEnableAlertOnNewItems.AccessibleName");
			this.checkEnableAlertOnNewItems.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkEnableAlertOnNewItems.Anchor")));
			this.checkEnableAlertOnNewItems.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkEnableAlertOnNewItems.Appearance")));
			this.checkEnableAlertOnNewItems.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkEnableAlertOnNewItems.BackgroundImage")));
			this.checkEnableAlertOnNewItems.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableAlertOnNewItems.CheckAlign")));
			this.checkEnableAlertOnNewItems.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkEnableAlertOnNewItems.Dock")));
			this.checkEnableAlertOnNewItems.Enabled = ((bool)(resources.GetObject("checkEnableAlertOnNewItems.Enabled")));
			this.checkEnableAlertOnNewItems.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkEnableAlertOnNewItems.FlatStyle")));
			this.checkEnableAlertOnNewItems.Font = ((System.Drawing.Font)(resources.GetObject("checkEnableAlertOnNewItems.Font")));
			this.checkEnableAlertOnNewItems.Image = ((System.Drawing.Image)(resources.GetObject("checkEnableAlertOnNewItems.Image")));
			this.checkEnableAlertOnNewItems.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableAlertOnNewItems.ImageAlign")));
			this.checkEnableAlertOnNewItems.ImageIndex = ((int)(resources.GetObject("checkEnableAlertOnNewItems.ImageIndex")));
			this.checkEnableAlertOnNewItems.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkEnableAlertOnNewItems.ImeMode")));
			this.checkEnableAlertOnNewItems.Location = ((System.Drawing.Point)(resources.GetObject("checkEnableAlertOnNewItems.Location")));
			this.checkEnableAlertOnNewItems.Name = "checkEnableAlertOnNewItems";
			this.checkEnableAlertOnNewItems.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkEnableAlertOnNewItems.RightToLeft")));
			this.checkEnableAlertOnNewItems.Size = ((System.Drawing.Size)(resources.GetObject("checkEnableAlertOnNewItems.Size")));
			this.checkEnableAlertOnNewItems.TabIndex = ((int)(resources.GetObject("checkEnableAlertOnNewItems.TabIndex")));
			this.checkEnableAlertOnNewItems.Text = resources.GetString("checkEnableAlertOnNewItems.Text");
			this.checkEnableAlertOnNewItems.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkEnableAlertOnNewItems.TextAlign")));
			this.toolTip.SetToolTip(this.checkEnableAlertOnNewItems, resources.GetString("checkEnableAlertOnNewItems.ToolTip"));
			this.checkEnableAlertOnNewItems.Visible = ((bool)(resources.GetObject("checkEnableAlertOnNewItems.Visible")));
			// 
			// pageFeedItemDisplay
			// 
			this.pageFeedItemDisplay.AccessibleDescription = resources.GetString("pageFeedItemDisplay.AccessibleDescription");
			this.pageFeedItemDisplay.AccessibleName = resources.GetString("pageFeedItemDisplay.AccessibleName");
			this.pageFeedItemDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageFeedItemDisplay.Anchor")));
			this.pageFeedItemDisplay.AutoScroll = ((bool)(resources.GetObject("pageFeedItemDisplay.AutoScroll")));
			this.pageFeedItemDisplay.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageFeedItemDisplay.AutoScrollMargin")));
			this.pageFeedItemDisplay.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageFeedItemDisplay.AutoScrollMinSize")));
			this.pageFeedItemDisplay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageFeedItemDisplay.BackgroundImage")));
			this.pageFeedItemDisplay.Controls.Add(this.comboFormatters);
			this.pageFeedItemDisplay.Controls.Add(this.lblFormatterStylesheet);
			this.pageFeedItemDisplay.Controls.Add(this.checkUseCustomFormatter);
			this.pageFeedItemDisplay.Controls.Add(this.lblFeedItemDisplayIntro);
			this.pageFeedItemDisplay.Description = resources.GetString("pageFeedItemDisplay.Description");
			this.pageFeedItemDisplay.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageFeedItemDisplay.Dock")));
			this.pageFeedItemDisplay.Enabled = ((bool)(resources.GetObject("pageFeedItemDisplay.Enabled")));
			this.pageFeedItemDisplay.Font = ((System.Drawing.Font)(resources.GetObject("pageFeedItemDisplay.Font")));
			this.pageFeedItemDisplay.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageFeedItemDisplay.ImeMode")));
			this.pageFeedItemDisplay.Location = ((System.Drawing.Point)(resources.GetObject("pageFeedItemDisplay.Location")));
			this.pageFeedItemDisplay.Name = "pageFeedItemDisplay";
			this.pageFeedItemDisplay.NextPage = this.finishPage;
			this.pageFeedItemDisplay.PreviousPage = this.pageFeedItemControl;
			this.pageFeedItemDisplay.ProceedText = resources.GetString("pageFeedItemDisplay.ProceedText");
			this.pageFeedItemDisplay.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageFeedItemDisplay.RightToLeft")));
			this.pageFeedItemDisplay.Size = ((System.Drawing.Size)(resources.GetObject("pageFeedItemDisplay.Size")));
			this.pageFeedItemDisplay.TabIndex = ((int)(resources.GetObject("pageFeedItemDisplay.TabIndex")));
			this.pageFeedItemDisplay.Text = resources.GetString("pageFeedItemDisplay.Text");
			this.toolTip.SetToolTip(this.pageFeedItemDisplay, resources.GetString("pageFeedItemDisplay.ToolTip"));
			this.pageFeedItemDisplay.Visible = ((bool)(resources.GetObject("pageFeedItemDisplay.Visible")));
			this.pageFeedItemDisplay.AfterDisplay += new System.EventHandler(this.OnPageFeedItemDisplayAfterDisplay);
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
			this.comboFormatters.Font = ((System.Drawing.Font)(resources.GetObject("comboFormatters.Font")));
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
			this.toolTip.SetToolTip(this.comboFormatters, resources.GetString("comboFormatters.ToolTip"));
			this.comboFormatters.Visible = ((bool)(resources.GetObject("comboFormatters.Visible")));
			// 
			// lblFormatterStylesheet
			// 
			this.lblFormatterStylesheet.AccessibleDescription = resources.GetString("lblFormatterStylesheet.AccessibleDescription");
			this.lblFormatterStylesheet.AccessibleName = resources.GetString("lblFormatterStylesheet.AccessibleName");
			this.lblFormatterStylesheet.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFormatterStylesheet.Anchor")));
			this.lblFormatterStylesheet.AutoSize = ((bool)(resources.GetObject("lblFormatterStylesheet.AutoSize")));
			this.lblFormatterStylesheet.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFormatterStylesheet.Dock")));
			this.lblFormatterStylesheet.Enabled = ((bool)(resources.GetObject("lblFormatterStylesheet.Enabled")));
			this.lblFormatterStylesheet.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFormatterStylesheet.Font = ((System.Drawing.Font)(resources.GetObject("lblFormatterStylesheet.Font")));
			this.lblFormatterStylesheet.Image = ((System.Drawing.Image)(resources.GetObject("lblFormatterStylesheet.Image")));
			this.lblFormatterStylesheet.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFormatterStylesheet.ImageAlign")));
			this.lblFormatterStylesheet.ImageIndex = ((int)(resources.GetObject("lblFormatterStylesheet.ImageIndex")));
			this.lblFormatterStylesheet.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFormatterStylesheet.ImeMode")));
			this.lblFormatterStylesheet.Location = ((System.Drawing.Point)(resources.GetObject("lblFormatterStylesheet.Location")));
			this.lblFormatterStylesheet.Name = "lblFormatterStylesheet";
			this.lblFormatterStylesheet.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFormatterStylesheet.RightToLeft")));
			this.lblFormatterStylesheet.Size = ((System.Drawing.Size)(resources.GetObject("lblFormatterStylesheet.Size")));
			this.lblFormatterStylesheet.TabIndex = ((int)(resources.GetObject("lblFormatterStylesheet.TabIndex")));
			this.lblFormatterStylesheet.Text = resources.GetString("lblFormatterStylesheet.Text");
			this.lblFormatterStylesheet.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFormatterStylesheet.TextAlign")));
			this.toolTip.SetToolTip(this.lblFormatterStylesheet, resources.GetString("lblFormatterStylesheet.ToolTip"));
			this.lblFormatterStylesheet.Visible = ((bool)(resources.GetObject("lblFormatterStylesheet.Visible")));
			// 
			// checkUseCustomFormatter
			// 
			this.checkUseCustomFormatter.AccessibleDescription = resources.GetString("checkUseCustomFormatter.AccessibleDescription");
			this.checkUseCustomFormatter.AccessibleName = resources.GetString("checkUseCustomFormatter.AccessibleName");
			this.checkUseCustomFormatter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkUseCustomFormatter.Anchor")));
			this.checkUseCustomFormatter.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkUseCustomFormatter.Appearance")));
			this.checkUseCustomFormatter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkUseCustomFormatter.BackgroundImage")));
			this.checkUseCustomFormatter.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseCustomFormatter.CheckAlign")));
			this.checkUseCustomFormatter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkUseCustomFormatter.Dock")));
			this.checkUseCustomFormatter.Enabled = ((bool)(resources.GetObject("checkUseCustomFormatter.Enabled")));
			this.checkUseCustomFormatter.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkUseCustomFormatter.FlatStyle")));
			this.checkUseCustomFormatter.Font = ((System.Drawing.Font)(resources.GetObject("checkUseCustomFormatter.Font")));
			this.checkUseCustomFormatter.Image = ((System.Drawing.Image)(resources.GetObject("checkUseCustomFormatter.Image")));
			this.checkUseCustomFormatter.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseCustomFormatter.ImageAlign")));
			this.checkUseCustomFormatter.ImageIndex = ((int)(resources.GetObject("checkUseCustomFormatter.ImageIndex")));
			this.checkUseCustomFormatter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkUseCustomFormatter.ImeMode")));
			this.checkUseCustomFormatter.Location = ((System.Drawing.Point)(resources.GetObject("checkUseCustomFormatter.Location")));
			this.checkUseCustomFormatter.Name = "checkUseCustomFormatter";
			this.checkUseCustomFormatter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkUseCustomFormatter.RightToLeft")));
			this.checkUseCustomFormatter.Size = ((System.Drawing.Size)(resources.GetObject("checkUseCustomFormatter.Size")));
			this.checkUseCustomFormatter.TabIndex = ((int)(resources.GetObject("checkUseCustomFormatter.TabIndex")));
			this.checkUseCustomFormatter.Text = resources.GetString("checkUseCustomFormatter.Text");
			this.checkUseCustomFormatter.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkUseCustomFormatter.TextAlign")));
			this.toolTip.SetToolTip(this.checkUseCustomFormatter, resources.GetString("checkUseCustomFormatter.ToolTip"));
			this.checkUseCustomFormatter.Visible = ((bool)(resources.GetObject("checkUseCustomFormatter.Visible")));
			// 
			// lblFeedItemDisplayIntro
			// 
			this.lblFeedItemDisplayIntro.AccessibleDescription = resources.GetString("lblFeedItemDisplayIntro.AccessibleDescription");
			this.lblFeedItemDisplayIntro.AccessibleName = resources.GetString("lblFeedItemDisplayIntro.AccessibleName");
			this.lblFeedItemDisplayIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFeedItemDisplayIntro.Anchor")));
			this.lblFeedItemDisplayIntro.AutoSize = ((bool)(resources.GetObject("lblFeedItemDisplayIntro.AutoSize")));
			this.lblFeedItemDisplayIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFeedItemDisplayIntro.Dock")));
			this.lblFeedItemDisplayIntro.Enabled = ((bool)(resources.GetObject("lblFeedItemDisplayIntro.Enabled")));
			this.lblFeedItemDisplayIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblFeedItemDisplayIntro.Font")));
			this.lblFeedItemDisplayIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblFeedItemDisplayIntro.Image")));
			this.lblFeedItemDisplayIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedItemDisplayIntro.ImageAlign")));
			this.lblFeedItemDisplayIntro.ImageIndex = ((int)(resources.GetObject("lblFeedItemDisplayIntro.ImageIndex")));
			this.lblFeedItemDisplayIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFeedItemDisplayIntro.ImeMode")));
			this.lblFeedItemDisplayIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblFeedItemDisplayIntro.Location")));
			this.lblFeedItemDisplayIntro.Name = "lblFeedItemDisplayIntro";
			this.lblFeedItemDisplayIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFeedItemDisplayIntro.RightToLeft")));
			this.lblFeedItemDisplayIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblFeedItemDisplayIntro.Size")));
			this.lblFeedItemDisplayIntro.TabIndex = ((int)(resources.GetObject("lblFeedItemDisplayIntro.TabIndex")));
			this.lblFeedItemDisplayIntro.Text = resources.GetString("lblFeedItemDisplayIntro.Text");
			this.lblFeedItemDisplayIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFeedItemDisplayIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblFeedItemDisplayIntro, resources.GetString("lblFeedItemDisplayIntro.ToolTip"));
			this.lblFeedItemDisplayIntro.Visible = ((bool)(resources.GetObject("lblFeedItemDisplayIntro.Visible")));
			// 
			// finishPage
			// 
			this.finishPage.AccessibleDescription = resources.GetString("finishPage.AccessibleDescription");
			this.finishPage.AccessibleName = resources.GetString("finishPage.AccessibleName");
			this.finishPage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("finishPage.Anchor")));
			this.finishPage.AutoScroll = ((bool)(resources.GetObject("finishPage.AutoScroll")));
			this.finishPage.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("finishPage.AutoScrollMargin")));
			this.finishPage.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("finishPage.AutoScrollMinSize")));
			this.finishPage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("finishPage.BackgroundImage")));
			this.finishPage.Controls.Add(this.pnlCompleting);
			this.finishPage.Controls.Add(this.pnlCancelling);
			this.finishPage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("finishPage.Dock")));
			this.finishPage.Enabled = ((bool)(resources.GetObject("finishPage.Enabled")));
			this.finishPage.FinishText = resources.GetString("finishPage.FinishText");
			this.finishPage.Font = ((System.Drawing.Font)(resources.GetObject("finishPage.Font")));
			this.finishPage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("finishPage.ImeMode")));
			this.finishPage.Location = ((System.Drawing.Point)(resources.GetObject("finishPage.Location")));
			this.finishPage.Name = "finishPage";
			this.finishPage.PreviousPage = this.pageFeedItemDisplay;
			this.finishPage.ProceedText = resources.GetString("finishPage.ProceedText");
			this.finishPage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("finishPage.RightToLeft")));
			this.finishPage.SettingsHeader = "";
			this.finishPage.Size = ((System.Drawing.Size)(resources.GetObject("finishPage.Size")));
			this.finishPage.TabIndex = ((int)(resources.GetObject("finishPage.TabIndex")));
			this.finishPage.Text = resources.GetString("finishPage.Text");
			this.toolTip.SetToolTip(this.finishPage, resources.GetString("finishPage.ToolTip"));
			this.finishPage.Visible = ((bool)(resources.GetObject("finishPage.Visible")));
			this.finishPage.BeforeMoveBack += new Divelements.WizardFramework.WizardPageEventHandler(this.OnFinishPage_BeforeMoveBack);
			this.finishPage.BeforeDisplay += new System.EventHandler(this.OnFinishPage_BeforeDisplay);
			// 
			// pnlCompleting
			// 
			this.pnlCompleting.AccessibleDescription = resources.GetString("pnlCompleting.AccessibleDescription");
			this.pnlCompleting.AccessibleName = resources.GetString("pnlCompleting.AccessibleName");
			this.pnlCompleting.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pnlCompleting.Anchor")));
			this.pnlCompleting.AutoScroll = ((bool)(resources.GetObject("pnlCompleting.AutoScroll")));
			this.pnlCompleting.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pnlCompleting.AutoScrollMargin")));
			this.pnlCompleting.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pnlCompleting.AutoScrollMinSize")));
			this.pnlCompleting.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlCompleting.BackgroundImage")));
			this.pnlCompleting.Controls.Add(this.lblCompletionMessage);
			this.pnlCompleting.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pnlCompleting.Dock")));
			this.pnlCompleting.Enabled = ((bool)(resources.GetObject("pnlCompleting.Enabled")));
			this.pnlCompleting.Font = ((System.Drawing.Font)(resources.GetObject("pnlCompleting.Font")));
			this.pnlCompleting.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pnlCompleting.ImeMode")));
			this.pnlCompleting.Location = ((System.Drawing.Point)(resources.GetObject("pnlCompleting.Location")));
			this.pnlCompleting.Name = "pnlCompleting";
			this.pnlCompleting.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pnlCompleting.RightToLeft")));
			this.pnlCompleting.Size = ((System.Drawing.Size)(resources.GetObject("pnlCompleting.Size")));
			this.pnlCompleting.TabIndex = ((int)(resources.GetObject("pnlCompleting.TabIndex")));
			this.pnlCompleting.Text = resources.GetString("pnlCompleting.Text");
			this.toolTip.SetToolTip(this.pnlCompleting, resources.GetString("pnlCompleting.ToolTip"));
			this.pnlCompleting.Visible = ((bool)(resources.GetObject("pnlCompleting.Visible")));
			// 
			// lblCompletionMessage
			// 
			this.lblCompletionMessage.AccessibleDescription = resources.GetString("lblCompletionMessage.AccessibleDescription");
			this.lblCompletionMessage.AccessibleName = resources.GetString("lblCompletionMessage.AccessibleName");
			this.lblCompletionMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblCompletionMessage.Anchor")));
			this.lblCompletionMessage.AutoSize = ((bool)(resources.GetObject("lblCompletionMessage.AutoSize")));
			this.lblCompletionMessage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblCompletionMessage.Dock")));
			this.lblCompletionMessage.Enabled = ((bool)(resources.GetObject("lblCompletionMessage.Enabled")));
			this.lblCompletionMessage.Font = ((System.Drawing.Font)(resources.GetObject("lblCompletionMessage.Font")));
			this.lblCompletionMessage.Image = ((System.Drawing.Image)(resources.GetObject("lblCompletionMessage.Image")));
			this.lblCompletionMessage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblCompletionMessage.ImageAlign")));
			this.lblCompletionMessage.ImageIndex = ((int)(resources.GetObject("lblCompletionMessage.ImageIndex")));
			this.lblCompletionMessage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblCompletionMessage.ImeMode")));
			this.lblCompletionMessage.Location = ((System.Drawing.Point)(resources.GetObject("lblCompletionMessage.Location")));
			this.lblCompletionMessage.Name = "lblCompletionMessage";
			this.lblCompletionMessage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblCompletionMessage.RightToLeft")));
			this.lblCompletionMessage.Size = ((System.Drawing.Size)(resources.GetObject("lblCompletionMessage.Size")));
			this.lblCompletionMessage.TabIndex = ((int)(resources.GetObject("lblCompletionMessage.TabIndex")));
			this.lblCompletionMessage.Text = resources.GetString("lblCompletionMessage.Text");
			this.lblCompletionMessage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblCompletionMessage.TextAlign")));
			this.toolTip.SetToolTip(this.lblCompletionMessage, resources.GetString("lblCompletionMessage.ToolTip"));
			this.lblCompletionMessage.Visible = ((bool)(resources.GetObject("lblCompletionMessage.Visible")));
			// 
			// pnlCancelling
			// 
			this.pnlCancelling.AccessibleDescription = resources.GetString("pnlCancelling.AccessibleDescription");
			this.pnlCancelling.AccessibleName = resources.GetString("pnlCancelling.AccessibleName");
			this.pnlCancelling.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pnlCancelling.Anchor")));
			this.pnlCancelling.AutoScroll = ((bool)(resources.GetObject("pnlCancelling.AutoScroll")));
			this.pnlCancelling.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pnlCancelling.AutoScrollMargin")));
			this.pnlCancelling.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pnlCancelling.AutoScrollMinSize")));
			this.pnlCancelling.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlCancelling.BackgroundImage")));
			this.pnlCancelling.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pnlCancelling.Dock")));
			this.pnlCancelling.Enabled = ((bool)(resources.GetObject("pnlCancelling.Enabled")));
			this.pnlCancelling.Font = ((System.Drawing.Font)(resources.GetObject("pnlCancelling.Font")));
			this.pnlCancelling.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pnlCancelling.ImeMode")));
			this.pnlCancelling.Location = ((System.Drawing.Point)(resources.GetObject("pnlCancelling.Location")));
			this.pnlCancelling.Name = "pnlCancelling";
			this.pnlCancelling.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pnlCancelling.RightToLeft")));
			this.pnlCancelling.Size = ((System.Drawing.Size)(resources.GetObject("pnlCancelling.Size")));
			this.pnlCancelling.TabIndex = ((int)(resources.GetObject("pnlCancelling.TabIndex")));
			this.pnlCancelling.Text = resources.GetString("pnlCancelling.Text");
			this.toolTip.SetToolTip(this.pnlCancelling, resources.GetString("pnlCancelling.ToolTip"));
			this.pnlCancelling.Visible = ((bool)(resources.GetObject("pnlCancelling.Visible")));
			// 
			// pageHowToSelection
			// 
			this.pageHowToSelection.AccessibleDescription = resources.GetString("pageHowToSelection.AccessibleDescription");
			this.pageHowToSelection.AccessibleName = resources.GetString("pageHowToSelection.AccessibleName");
			this.pageHowToSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageHowToSelection.Anchor")));
			this.pageHowToSelection.AutoScroll = ((bool)(resources.GetObject("pageHowToSelection.AutoScroll")));
			this.pageHowToSelection.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageHowToSelection.AutoScrollMargin")));
			this.pageHowToSelection.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageHowToSelection.AutoScrollMinSize")));
			this.pageHowToSelection.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageHowToSelection.BackgroundImage")));
			this.pageHowToSelection.Controls.Add(this.radioNewByNNTPGroup);
			this.pageHowToSelection.Controls.Add(this.lblHowToSubscribeIntro);
			this.pageHowToSelection.Controls.Add(this.radioNewByTopicSearch);
			this.pageHowToSelection.Controls.Add(this.radioNewByURL);
			this.pageHowToSelection.Description = resources.GetString("pageHowToSelection.Description");
			this.pageHowToSelection.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageHowToSelection.Dock")));
			this.pageHowToSelection.Enabled = ((bool)(resources.GetObject("pageHowToSelection.Enabled")));
			this.pageHowToSelection.Font = ((System.Drawing.Font)(resources.GetObject("pageHowToSelection.Font")));
			this.pageHowToSelection.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageHowToSelection.ImeMode")));
			this.pageHowToSelection.Location = ((System.Drawing.Point)(resources.GetObject("pageHowToSelection.Location")));
			this.pageHowToSelection.Name = "pageHowToSelection";
			this.pageHowToSelection.NextPage = this.pageNewByURL;
			this.pageHowToSelection.PreviousPage = this.pageWelcome;
			this.pageHowToSelection.ProceedText = resources.GetString("pageHowToSelection.ProceedText");
			this.pageHowToSelection.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageHowToSelection.RightToLeft")));
			this.pageHowToSelection.Size = ((System.Drawing.Size)(resources.GetObject("pageHowToSelection.Size")));
			this.pageHowToSelection.TabIndex = ((int)(resources.GetObject("pageHowToSelection.TabIndex")));
			this.pageHowToSelection.Text = resources.GetString("pageHowToSelection.Text");
			this.toolTip.SetToolTip(this.pageHowToSelection, resources.GetString("pageHowToSelection.ToolTip"));
			this.pageHowToSelection.Visible = ((bool)(resources.GetObject("pageHowToSelection.Visible")));
			this.pageHowToSelection.AfterDisplay += new System.EventHandler(this.OnPageHowToSelectionAfterDisplay);
			// 
			// radioNewByNNTPGroup
			// 
			this.radioNewByNNTPGroup.AccessibleDescription = resources.GetString("radioNewByNNTPGroup.AccessibleDescription");
			this.radioNewByNNTPGroup.AccessibleName = resources.GetString("radioNewByNNTPGroup.AccessibleName");
			this.radioNewByNNTPGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioNewByNNTPGroup.Anchor")));
			this.radioNewByNNTPGroup.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioNewByNNTPGroup.Appearance")));
			this.radioNewByNNTPGroup.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioNewByNNTPGroup.BackgroundImage")));
			this.radioNewByNNTPGroup.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByNNTPGroup.CheckAlign")));
			this.radioNewByNNTPGroup.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioNewByNNTPGroup.Dock")));
			this.radioNewByNNTPGroup.Enabled = ((bool)(resources.GetObject("radioNewByNNTPGroup.Enabled")));
			this.radioNewByNNTPGroup.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioNewByNNTPGroup.FlatStyle")));
			this.radioNewByNNTPGroup.Font = ((System.Drawing.Font)(resources.GetObject("radioNewByNNTPGroup.Font")));
			this.radioNewByNNTPGroup.Image = ((System.Drawing.Image)(resources.GetObject("radioNewByNNTPGroup.Image")));
			this.radioNewByNNTPGroup.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByNNTPGroup.ImageAlign")));
			this.radioNewByNNTPGroup.ImageIndex = ((int)(resources.GetObject("radioNewByNNTPGroup.ImageIndex")));
			this.radioNewByNNTPGroup.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioNewByNNTPGroup.ImeMode")));
			this.radioNewByNNTPGroup.Location = ((System.Drawing.Point)(resources.GetObject("radioNewByNNTPGroup.Location")));
			this.radioNewByNNTPGroup.Name = "radioNewByNNTPGroup";
			this.radioNewByNNTPGroup.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioNewByNNTPGroup.RightToLeft")));
			this.radioNewByNNTPGroup.Size = ((System.Drawing.Size)(resources.GetObject("radioNewByNNTPGroup.Size")));
			this.radioNewByNNTPGroup.TabIndex = ((int)(resources.GetObject("radioNewByNNTPGroup.TabIndex")));
			this.radioNewByNNTPGroup.Text = resources.GetString("radioNewByNNTPGroup.Text");
			this.radioNewByNNTPGroup.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByNNTPGroup.TextAlign")));
			this.toolTip.SetToolTip(this.radioNewByNNTPGroup, resources.GetString("radioNewByNNTPGroup.ToolTip"));
			this.radioNewByNNTPGroup.Visible = ((bool)(resources.GetObject("radioNewByNNTPGroup.Visible")));
			this.radioNewByNNTPGroup.CheckedChanged += new System.EventHandler(this.OnRadioHowToSubscribeCheckedChanged);
			// 
			// lblHowToSubscribeIntro
			// 
			this.lblHowToSubscribeIntro.AccessibleDescription = resources.GetString("lblHowToSubscribeIntro.AccessibleDescription");
			this.lblHowToSubscribeIntro.AccessibleName = resources.GetString("lblHowToSubscribeIntro.AccessibleName");
			this.lblHowToSubscribeIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblHowToSubscribeIntro.Anchor")));
			this.lblHowToSubscribeIntro.AutoSize = ((bool)(resources.GetObject("lblHowToSubscribeIntro.AutoSize")));
			this.lblHowToSubscribeIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblHowToSubscribeIntro.Dock")));
			this.lblHowToSubscribeIntro.Enabled = ((bool)(resources.GetObject("lblHowToSubscribeIntro.Enabled")));
			this.lblHowToSubscribeIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblHowToSubscribeIntro.Font")));
			this.lblHowToSubscribeIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblHowToSubscribeIntro.Image")));
			this.lblHowToSubscribeIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblHowToSubscribeIntro.ImageAlign")));
			this.lblHowToSubscribeIntro.ImageIndex = ((int)(resources.GetObject("lblHowToSubscribeIntro.ImageIndex")));
			this.lblHowToSubscribeIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblHowToSubscribeIntro.ImeMode")));
			this.lblHowToSubscribeIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblHowToSubscribeIntro.Location")));
			this.lblHowToSubscribeIntro.Name = "lblHowToSubscribeIntro";
			this.lblHowToSubscribeIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblHowToSubscribeIntro.RightToLeft")));
			this.lblHowToSubscribeIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblHowToSubscribeIntro.Size")));
			this.lblHowToSubscribeIntro.TabIndex = ((int)(resources.GetObject("lblHowToSubscribeIntro.TabIndex")));
			this.lblHowToSubscribeIntro.Text = resources.GetString("lblHowToSubscribeIntro.Text");
			this.lblHowToSubscribeIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblHowToSubscribeIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblHowToSubscribeIntro, resources.GetString("lblHowToSubscribeIntro.ToolTip"));
			this.lblHowToSubscribeIntro.Visible = ((bool)(resources.GetObject("lblHowToSubscribeIntro.Visible")));
			// 
			// radioNewByTopicSearch
			// 
			this.radioNewByTopicSearch.AccessibleDescription = resources.GetString("radioNewByTopicSearch.AccessibleDescription");
			this.radioNewByTopicSearch.AccessibleName = resources.GetString("radioNewByTopicSearch.AccessibleName");
			this.radioNewByTopicSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioNewByTopicSearch.Anchor")));
			this.radioNewByTopicSearch.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioNewByTopicSearch.Appearance")));
			this.radioNewByTopicSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioNewByTopicSearch.BackgroundImage")));
			this.radioNewByTopicSearch.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByTopicSearch.CheckAlign")));
			this.radioNewByTopicSearch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioNewByTopicSearch.Dock")));
			this.radioNewByTopicSearch.Enabled = ((bool)(resources.GetObject("radioNewByTopicSearch.Enabled")));
			this.radioNewByTopicSearch.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioNewByTopicSearch.FlatStyle")));
			this.radioNewByTopicSearch.Font = ((System.Drawing.Font)(resources.GetObject("radioNewByTopicSearch.Font")));
			this.radioNewByTopicSearch.Image = ((System.Drawing.Image)(resources.GetObject("radioNewByTopicSearch.Image")));
			this.radioNewByTopicSearch.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByTopicSearch.ImageAlign")));
			this.radioNewByTopicSearch.ImageIndex = ((int)(resources.GetObject("radioNewByTopicSearch.ImageIndex")));
			this.radioNewByTopicSearch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioNewByTopicSearch.ImeMode")));
			this.radioNewByTopicSearch.Location = ((System.Drawing.Point)(resources.GetObject("radioNewByTopicSearch.Location")));
			this.radioNewByTopicSearch.Name = "radioNewByTopicSearch";
			this.radioNewByTopicSearch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioNewByTopicSearch.RightToLeft")));
			this.radioNewByTopicSearch.Size = ((System.Drawing.Size)(resources.GetObject("radioNewByTopicSearch.Size")));
			this.radioNewByTopicSearch.TabIndex = ((int)(resources.GetObject("radioNewByTopicSearch.TabIndex")));
			this.radioNewByTopicSearch.Text = resources.GetString("radioNewByTopicSearch.Text");
			this.radioNewByTopicSearch.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByTopicSearch.TextAlign")));
			this.toolTip.SetToolTip(this.radioNewByTopicSearch, resources.GetString("radioNewByTopicSearch.ToolTip"));
			this.radioNewByTopicSearch.Visible = ((bool)(resources.GetObject("radioNewByTopicSearch.Visible")));
			this.radioNewByTopicSearch.CheckedChanged += new System.EventHandler(this.OnRadioHowToSubscribeCheckedChanged);
			// 
			// radioNewByURL
			// 
			this.radioNewByURL.AccessibleDescription = resources.GetString("radioNewByURL.AccessibleDescription");
			this.radioNewByURL.AccessibleName = resources.GetString("radioNewByURL.AccessibleName");
			this.radioNewByURL.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioNewByURL.Anchor")));
			this.radioNewByURL.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioNewByURL.Appearance")));
			this.radioNewByURL.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioNewByURL.BackgroundImage")));
			this.radioNewByURL.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByURL.CheckAlign")));
			this.radioNewByURL.Checked = true;
			this.radioNewByURL.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioNewByURL.Dock")));
			this.radioNewByURL.Enabled = ((bool)(resources.GetObject("radioNewByURL.Enabled")));
			this.radioNewByURL.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioNewByURL.FlatStyle")));
			this.radioNewByURL.Font = ((System.Drawing.Font)(resources.GetObject("radioNewByURL.Font")));
			this.radioNewByURL.Image = ((System.Drawing.Image)(resources.GetObject("radioNewByURL.Image")));
			this.radioNewByURL.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByURL.ImageAlign")));
			this.radioNewByURL.ImageIndex = ((int)(resources.GetObject("radioNewByURL.ImageIndex")));
			this.radioNewByURL.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioNewByURL.ImeMode")));
			this.radioNewByURL.Location = ((System.Drawing.Point)(resources.GetObject("radioNewByURL.Location")));
			this.radioNewByURL.Name = "radioNewByURL";
			this.radioNewByURL.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioNewByURL.RightToLeft")));
			this.radioNewByURL.Size = ((System.Drawing.Size)(resources.GetObject("radioNewByURL.Size")));
			this.radioNewByURL.TabIndex = ((int)(resources.GetObject("radioNewByURL.TabIndex")));
			this.radioNewByURL.TabStop = true;
			this.radioNewByURL.Text = resources.GetString("radioNewByURL.Text");
			this.radioNewByURL.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewByURL.TextAlign")));
			this.toolTip.SetToolTip(this.radioNewByURL, resources.GetString("radioNewByURL.ToolTip"));
			this.radioNewByURL.Visible = ((bool)(resources.GetObject("radioNewByURL.Visible")));
			this.radioNewByURL.CheckedChanged += new System.EventHandler(this.OnRadioHowToSubscribeCheckedChanged);
			// 
			// pageNewByURL
			// 
			this.pageNewByURL.AccessibleDescription = resources.GetString("pageNewByURL.AccessibleDescription");
			this.pageNewByURL.AccessibleName = resources.GetString("pageNewByURL.AccessibleName");
			this.pageNewByURL.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageNewByURL.Anchor")));
			this.pageNewByURL.AutoScroll = ((bool)(resources.GetObject("pageNewByURL.AutoScroll")));
			this.pageNewByURL.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageNewByURL.AutoScrollMargin")));
			this.pageNewByURL.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageNewByURL.AutoScrollMinSize")));
			this.pageNewByURL.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageNewByURL.BackgroundImage")));
			this.pageNewByURL.Controls.Add(this.linkLabelCanonicalUrl);
			this.pageNewByURL.Controls.Add(this.pictureHelpAutodiscover);
			this.pageNewByURL.Controls.Add(this.lblAutodiscoverHelp);
			this.pageNewByURL.Controls.Add(this.checkNewByURLValidate);
			this.pageNewByURL.Controls.Add(this.lblNewByURLIntro);
			this.pageNewByURL.Controls.Add(this.txtNewByURL);
			this.pageNewByURL.Controls.Add(this.lblNewByURL);
			this.pageNewByURL.Description = resources.GetString("pageNewByURL.Description");
			this.pageNewByURL.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageNewByURL.Dock")));
			this.pageNewByURL.Enabled = ((bool)(resources.GetObject("pageNewByURL.Enabled")));
			this.pageNewByURL.Font = ((System.Drawing.Font)(resources.GetObject("pageNewByURL.Font")));
			this.pageNewByURL.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageNewByURL.ImeMode")));
			this.pageNewByURL.Location = ((System.Drawing.Point)(resources.GetObject("pageNewByURL.Location")));
			this.pageNewByURL.Name = "pageNewByURL";
			this.pageNewByURL.NextPage = this.pageValidateUrl;
			this.pageNewByURL.PreviousPage = this.pageHowToSelection;
			this.pageNewByURL.ProceedText = resources.GetString("pageNewByURL.ProceedText");
			this.pageNewByURL.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageNewByURL.RightToLeft")));
			this.pageNewByURL.Size = ((System.Drawing.Size)(resources.GetObject("pageNewByURL.Size")));
			this.pageNewByURL.TabIndex = ((int)(resources.GetObject("pageNewByURL.TabIndex")));
			this.pageNewByURL.Text = resources.GetString("pageNewByURL.Text");
			this.toolTip.SetToolTip(this.pageNewByURL, resources.GetString("pageNewByURL.ToolTip"));
			this.pageNewByURL.Visible = ((bool)(resources.GetObject("pageNewByURL.Visible")));
			this.pageNewByURL.AfterDisplay += new System.EventHandler(this.OnPageNewURLAfterDisplay);
			// 
			// linkLabelCanonicalUrl
			// 
			this.linkLabelCanonicalUrl.AccessibleDescription = resources.GetString("linkLabelCanonicalUrl.AccessibleDescription");
			this.linkLabelCanonicalUrl.AccessibleName = resources.GetString("linkLabelCanonicalUrl.AccessibleName");
			this.linkLabelCanonicalUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("linkLabelCanonicalUrl.Anchor")));
			this.linkLabelCanonicalUrl.AutoSize = ((bool)(resources.GetObject("linkLabelCanonicalUrl.AutoSize")));
			this.linkLabelCanonicalUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("linkLabelCanonicalUrl.Dock")));
			this.linkLabelCanonicalUrl.Enabled = ((bool)(resources.GetObject("linkLabelCanonicalUrl.Enabled")));
			this.linkLabelCanonicalUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.linkLabelCanonicalUrl.Font = ((System.Drawing.Font)(resources.GetObject("linkLabelCanonicalUrl.Font")));
			this.linkLabelCanonicalUrl.Image = ((System.Drawing.Image)(resources.GetObject("linkLabelCanonicalUrl.Image")));
			this.linkLabelCanonicalUrl.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabelCanonicalUrl.ImageAlign")));
			this.linkLabelCanonicalUrl.ImageIndex = ((int)(resources.GetObject("linkLabelCanonicalUrl.ImageIndex")));
			this.linkLabelCanonicalUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("linkLabelCanonicalUrl.ImeMode")));
			this.linkLabelCanonicalUrl.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("linkLabelCanonicalUrl.LinkArea")));
			this.linkLabelCanonicalUrl.Location = ((System.Drawing.Point)(resources.GetObject("linkLabelCanonicalUrl.Location")));
			this.linkLabelCanonicalUrl.Name = "linkLabelCanonicalUrl";
			this.linkLabelCanonicalUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("linkLabelCanonicalUrl.RightToLeft")));
			this.linkLabelCanonicalUrl.Size = ((System.Drawing.Size)(resources.GetObject("linkLabelCanonicalUrl.Size")));
			this.linkLabelCanonicalUrl.TabIndex = ((int)(resources.GetObject("linkLabelCanonicalUrl.TabIndex")));
			this.linkLabelCanonicalUrl.Text = resources.GetString("linkLabelCanonicalUrl.Text");
			this.linkLabelCanonicalUrl.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("linkLabelCanonicalUrl.TextAlign")));
			this.toolTip.SetToolTip(this.linkLabelCanonicalUrl, resources.GetString("linkLabelCanonicalUrl.ToolTip"));
			this.linkLabelCanonicalUrl.Visible = ((bool)(resources.GetObject("linkLabelCanonicalUrl.Visible")));
			this.linkLabelCanonicalUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
			// 
			// pictureHelpAutodiscover
			// 
			this.pictureHelpAutodiscover.AccessibleDescription = resources.GetString("pictureHelpAutodiscover.AccessibleDescription");
			this.pictureHelpAutodiscover.AccessibleName = resources.GetString("pictureHelpAutodiscover.AccessibleName");
			this.pictureHelpAutodiscover.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureHelpAutodiscover.Anchor")));
			this.pictureHelpAutodiscover.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureHelpAutodiscover.BackgroundImage")));
			this.pictureHelpAutodiscover.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureHelpAutodiscover.Dock")));
			this.pictureHelpAutodiscover.Enabled = ((bool)(resources.GetObject("pictureHelpAutodiscover.Enabled")));
			this.pictureHelpAutodiscover.Font = ((System.Drawing.Font)(resources.GetObject("pictureHelpAutodiscover.Font")));
			this.pictureHelpAutodiscover.Image = ((System.Drawing.Image)(resources.GetObject("pictureHelpAutodiscover.Image")));
			this.pictureHelpAutodiscover.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureHelpAutodiscover.ImeMode")));
			this.pictureHelpAutodiscover.Location = ((System.Drawing.Point)(resources.GetObject("pictureHelpAutodiscover.Location")));
			this.pictureHelpAutodiscover.Name = "pictureHelpAutodiscover";
			this.pictureHelpAutodiscover.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureHelpAutodiscover.RightToLeft")));
			this.pictureHelpAutodiscover.Size = ((System.Drawing.Size)(resources.GetObject("pictureHelpAutodiscover.Size")));
			this.pictureHelpAutodiscover.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureHelpAutodiscover.SizeMode")));
			this.pictureHelpAutodiscover.TabIndex = ((int)(resources.GetObject("pictureHelpAutodiscover.TabIndex")));
			this.pictureHelpAutodiscover.TabStop = false;
			this.pictureHelpAutodiscover.Text = resources.GetString("pictureHelpAutodiscover.Text");
			this.toolTip.SetToolTip(this.pictureHelpAutodiscover, resources.GetString("pictureHelpAutodiscover.ToolTip"));
			this.pictureHelpAutodiscover.Visible = ((bool)(resources.GetObject("pictureHelpAutodiscover.Visible")));
			// 
			// lblAutodiscoverHelp
			// 
			this.lblAutodiscoverHelp.AccessibleDescription = resources.GetString("lblAutodiscoverHelp.AccessibleDescription");
			this.lblAutodiscoverHelp.AccessibleName = resources.GetString("lblAutodiscoverHelp.AccessibleName");
			this.lblAutodiscoverHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblAutodiscoverHelp.Anchor")));
			this.lblAutodiscoverHelp.AutoSize = ((bool)(resources.GetObject("lblAutodiscoverHelp.AutoSize")));
			this.lblAutodiscoverHelp.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblAutodiscoverHelp.Dock")));
			this.lblAutodiscoverHelp.Enabled = ((bool)(resources.GetObject("lblAutodiscoverHelp.Enabled")));
			this.lblAutodiscoverHelp.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblAutodiscoverHelp.Font = ((System.Drawing.Font)(resources.GetObject("lblAutodiscoverHelp.Font")));
			this.lblAutodiscoverHelp.Image = ((System.Drawing.Image)(resources.GetObject("lblAutodiscoverHelp.Image")));
			this.lblAutodiscoverHelp.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblAutodiscoverHelp.ImageAlign")));
			this.lblAutodiscoverHelp.ImageIndex = ((int)(resources.GetObject("lblAutodiscoverHelp.ImageIndex")));
			this.lblAutodiscoverHelp.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblAutodiscoverHelp.ImeMode")));
			this.lblAutodiscoverHelp.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblAutodiscoverHelp.LinkArea")));
			this.lblAutodiscoverHelp.Location = ((System.Drawing.Point)(resources.GetObject("lblAutodiscoverHelp.Location")));
			this.lblAutodiscoverHelp.Name = "lblAutodiscoverHelp";
			this.lblAutodiscoverHelp.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblAutodiscoverHelp.RightToLeft")));
			this.lblAutodiscoverHelp.Size = ((System.Drawing.Size)(resources.GetObject("lblAutodiscoverHelp.Size")));
			this.lblAutodiscoverHelp.TabIndex = ((int)(resources.GetObject("lblAutodiscoverHelp.TabIndex")));
			this.lblAutodiscoverHelp.TabStop = true;
			this.lblAutodiscoverHelp.Tag = "http://diveintomark.org/archives/2002/08/15/ultraliberal_rss_locator";
			this.lblAutodiscoverHelp.Text = resources.GetString("lblAutodiscoverHelp.Text");
			this.lblAutodiscoverHelp.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblAutodiscoverHelp.TextAlign")));
			this.toolTip.SetToolTip(this.lblAutodiscoverHelp, resources.GetString("lblAutodiscoverHelp.ToolTip"));
			this.lblAutodiscoverHelp.Visible = ((bool)(resources.GetObject("lblAutodiscoverHelp.Visible")));
			this.lblAutodiscoverHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
			// 
			// checkNewByURLValidate
			// 
			this.checkNewByURLValidate.AccessibleDescription = resources.GetString("checkNewByURLValidate.AccessibleDescription");
			this.checkNewByURLValidate.AccessibleName = resources.GetString("checkNewByURLValidate.AccessibleName");
			this.checkNewByURLValidate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkNewByURLValidate.Anchor")));
			this.checkNewByURLValidate.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkNewByURLValidate.Appearance")));
			this.checkNewByURLValidate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkNewByURLValidate.BackgroundImage")));
			this.checkNewByURLValidate.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkNewByURLValidate.CheckAlign")));
			this.checkNewByURLValidate.Checked = true;
			this.checkNewByURLValidate.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkNewByURLValidate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkNewByURLValidate.Dock")));
			this.checkNewByURLValidate.Enabled = ((bool)(resources.GetObject("checkNewByURLValidate.Enabled")));
			this.checkNewByURLValidate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkNewByURLValidate.FlatStyle")));
			this.checkNewByURLValidate.Font = ((System.Drawing.Font)(resources.GetObject("checkNewByURLValidate.Font")));
			this.checkNewByURLValidate.Image = ((System.Drawing.Image)(resources.GetObject("checkNewByURLValidate.Image")));
			this.checkNewByURLValidate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkNewByURLValidate.ImageAlign")));
			this.checkNewByURLValidate.ImageIndex = ((int)(resources.GetObject("checkNewByURLValidate.ImageIndex")));
			this.checkNewByURLValidate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkNewByURLValidate.ImeMode")));
			this.checkNewByURLValidate.Location = ((System.Drawing.Point)(resources.GetObject("checkNewByURLValidate.Location")));
			this.checkNewByURLValidate.Name = "checkNewByURLValidate";
			this.checkNewByURLValidate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkNewByURLValidate.RightToLeft")));
			this.checkNewByURLValidate.Size = ((System.Drawing.Size)(resources.GetObject("checkNewByURLValidate.Size")));
			this.checkNewByURLValidate.TabIndex = ((int)(resources.GetObject("checkNewByURLValidate.TabIndex")));
			this.checkNewByURLValidate.Text = resources.GetString("checkNewByURLValidate.Text");
			this.checkNewByURLValidate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkNewByURLValidate.TextAlign")));
			this.toolTip.SetToolTip(this.checkNewByURLValidate, resources.GetString("checkNewByURLValidate.ToolTip"));
			this.checkNewByURLValidate.Visible = ((bool)(resources.GetObject("checkNewByURLValidate.Visible")));
			this.checkNewByURLValidate.CheckedChanged += new System.EventHandler(this.OnAutodiscoverVerifyCheckedChanged);
			// 
			// lblNewByURLIntro
			// 
			this.lblNewByURLIntro.AccessibleDescription = resources.GetString("lblNewByURLIntro.AccessibleDescription");
			this.lblNewByURLIntro.AccessibleName = resources.GetString("lblNewByURLIntro.AccessibleName");
			this.lblNewByURLIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNewByURLIntro.Anchor")));
			this.lblNewByURLIntro.AutoSize = ((bool)(resources.GetObject("lblNewByURLIntro.AutoSize")));
			this.lblNewByURLIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNewByURLIntro.Dock")));
			this.lblNewByURLIntro.Enabled = ((bool)(resources.GetObject("lblNewByURLIntro.Enabled")));
			this.lblNewByURLIntro.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewByURLIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblNewByURLIntro.Font")));
			this.lblNewByURLIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblNewByURLIntro.Image")));
			this.lblNewByURLIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewByURLIntro.ImageAlign")));
			this.lblNewByURLIntro.ImageIndex = ((int)(resources.GetObject("lblNewByURLIntro.ImageIndex")));
			this.lblNewByURLIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNewByURLIntro.ImeMode")));
			this.lblNewByURLIntro.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblNewByURLIntro.LinkArea")));
			this.lblNewByURLIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblNewByURLIntro.Location")));
			this.lblNewByURLIntro.Name = "lblNewByURLIntro";
			this.lblNewByURLIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNewByURLIntro.RightToLeft")));
			this.lblNewByURLIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblNewByURLIntro.Size")));
			this.lblNewByURLIntro.TabIndex = ((int)(resources.GetObject("lblNewByURLIntro.TabIndex")));
			this.lblNewByURLIntro.Tag = "";
			this.lblNewByURLIntro.Text = resources.GetString("lblNewByURLIntro.Text");
			this.lblNewByURLIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewByURLIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblNewByURLIntro, resources.GetString("lblNewByURLIntro.ToolTip"));
			this.lblNewByURLIntro.Visible = ((bool)(resources.GetObject("lblNewByURLIntro.Visible")));
			// 
			// txtNewByURL
			// 
			this.txtNewByURL.AccessibleDescription = resources.GetString("txtNewByURL.AccessibleDescription");
			this.txtNewByURL.AccessibleName = resources.GetString("txtNewByURL.AccessibleName");
			this.txtNewByURL.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtNewByURL.Anchor")));
			this.txtNewByURL.AutoSize = ((bool)(resources.GetObject("txtNewByURL.AutoSize")));
			this.txtNewByURL.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtNewByURL.BackgroundImage")));
			this.txtNewByURL.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtNewByURL.Dock")));
			this.txtNewByURL.Enabled = ((bool)(resources.GetObject("txtNewByURL.Enabled")));
			this.txtNewByURL.Font = ((System.Drawing.Font)(resources.GetObject("txtNewByURL.Font")));
			this.txtNewByURL.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtNewByURL.ImeMode")));
			this.txtNewByURL.Location = ((System.Drawing.Point)(resources.GetObject("txtNewByURL.Location")));
			this.txtNewByURL.MaxLength = ((int)(resources.GetObject("txtNewByURL.MaxLength")));
			this.txtNewByURL.Multiline = ((bool)(resources.GetObject("txtNewByURL.Multiline")));
			this.txtNewByURL.Name = "txtNewByURL";
			this.txtNewByURL.PasswordChar = ((char)(resources.GetObject("txtNewByURL.PasswordChar")));
			this.txtNewByURL.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtNewByURL.RightToLeft")));
			this.txtNewByURL.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtNewByURL.ScrollBars")));
			this.txtNewByURL.Size = ((System.Drawing.Size)(resources.GetObject("txtNewByURL.Size")));
			this.txtNewByURL.TabIndex = ((int)(resources.GetObject("txtNewByURL.TabIndex")));
			this.txtNewByURL.Text = resources.GetString("txtNewByURL.Text");
			this.txtNewByURL.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtNewByURL.TextAlign")));
			this.toolTip.SetToolTip(this.txtNewByURL, resources.GetString("txtNewByURL.ToolTip"));
			this.txtNewByURL.Visible = ((bool)(resources.GetObject("txtNewByURL.Visible")));
			this.txtNewByURL.WordWrap = ((bool)(resources.GetObject("txtNewByURL.WordWrap")));
			this.txtNewByURL.Validating += new System.ComponentModel.CancelEventHandler(this.OnTextNewByUrlValidating);
			this.txtNewByURL.TextChanged += new System.EventHandler(this.OnNewFeedUrlTextChanged);
			// 
			// lblNewByURL
			// 
			this.lblNewByURL.AccessibleDescription = resources.GetString("lblNewByURL.AccessibleDescription");
			this.lblNewByURL.AccessibleName = resources.GetString("lblNewByURL.AccessibleName");
			this.lblNewByURL.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNewByURL.Anchor")));
			this.lblNewByURL.AutoSize = ((bool)(resources.GetObject("lblNewByURL.AutoSize")));
			this.lblNewByURL.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNewByURL.Dock")));
			this.lblNewByURL.Enabled = ((bool)(resources.GetObject("lblNewByURL.Enabled")));
			this.lblNewByURL.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewByURL.Font = ((System.Drawing.Font)(resources.GetObject("lblNewByURL.Font")));
			this.lblNewByURL.Image = ((System.Drawing.Image)(resources.GetObject("lblNewByURL.Image")));
			this.lblNewByURL.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewByURL.ImageAlign")));
			this.lblNewByURL.ImageIndex = ((int)(resources.GetObject("lblNewByURL.ImageIndex")));
			this.lblNewByURL.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNewByURL.ImeMode")));
			this.lblNewByURL.Location = ((System.Drawing.Point)(resources.GetObject("lblNewByURL.Location")));
			this.lblNewByURL.Name = "lblNewByURL";
			this.lblNewByURL.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNewByURL.RightToLeft")));
			this.lblNewByURL.Size = ((System.Drawing.Size)(resources.GetObject("lblNewByURL.Size")));
			this.lblNewByURL.TabIndex = ((int)(resources.GetObject("lblNewByURL.TabIndex")));
			this.lblNewByURL.Text = resources.GetString("lblNewByURL.Text");
			this.lblNewByURL.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewByURL.TextAlign")));
			this.toolTip.SetToolTip(this.lblNewByURL, resources.GetString("lblNewByURL.ToolTip"));
			this.lblNewByURL.Visible = ((bool)(resources.GetObject("lblNewByURL.Visible")));
			// 
			// pageWelcome
			// 
			this.pageWelcome.AccessibleDescription = resources.GetString("pageWelcome.AccessibleDescription");
			this.pageWelcome.AccessibleName = resources.GetString("pageWelcome.AccessibleName");
			this.pageWelcome.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageWelcome.Anchor")));
			this.pageWelcome.AutoScroll = ((bool)(resources.GetObject("pageWelcome.AutoScroll")));
			this.pageWelcome.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageWelcome.AutoScrollMargin")));
			this.pageWelcome.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageWelcome.AutoScrollMinSize")));
			this.pageWelcome.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageWelcome.BackgroundImage")));
			this.pageWelcome.Controls.Add(this.chkDisplayWelcome);
			this.pageWelcome.Controls.Add(this.lblWelcomeInfoBox);
			this.pageWelcome.Controls.Add(this.lblWelcomeHelpMessage1);
			this.pageWelcome.Controls.Add(this.lblWelcomeHelpMessage2);
			this.pageWelcome.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageWelcome.Dock")));
			this.pageWelcome.Enabled = ((bool)(resources.GetObject("pageWelcome.Enabled")));
			this.pageWelcome.Font = ((System.Drawing.Font)(resources.GetObject("pageWelcome.Font")));
			this.pageWelcome.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageWelcome.ImeMode")));
			this.pageWelcome.IntroductionText = resources.GetString("pageWelcome.IntroductionText");
			this.pageWelcome.Location = ((System.Drawing.Point)(resources.GetObject("pageWelcome.Location")));
			this.pageWelcome.Name = "pageWelcome";
			this.pageWelcome.NextPage = this.pageHowToSelection;
			this.pageWelcome.ProceedText = resources.GetString("pageWelcome.ProceedText");
			this.pageWelcome.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageWelcome.RightToLeft")));
			this.pageWelcome.Size = ((System.Drawing.Size)(resources.GetObject("pageWelcome.Size")));
			this.pageWelcome.TabIndex = ((int)(resources.GetObject("pageWelcome.TabIndex")));
			this.pageWelcome.Text = resources.GetString("pageWelcome.Text");
			this.toolTip.SetToolTip(this.pageWelcome, resources.GetString("pageWelcome.ToolTip"));
			this.pageWelcome.Visible = ((bool)(resources.GetObject("pageWelcome.Visible")));
			this.pageWelcome.BeforeDisplay += new System.EventHandler(this.OnPageWelcome_BeforeDisplay);
			// 
			// chkDisplayWelcome
			// 
			this.chkDisplayWelcome.AccessibleDescription = resources.GetString("chkDisplayWelcome.AccessibleDescription");
			this.chkDisplayWelcome.AccessibleName = resources.GetString("chkDisplayWelcome.AccessibleName");
			this.chkDisplayWelcome.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkDisplayWelcome.Anchor")));
			this.chkDisplayWelcome.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkDisplayWelcome.Appearance")));
			this.chkDisplayWelcome.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkDisplayWelcome.BackgroundImage")));
			this.chkDisplayWelcome.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkDisplayWelcome.CheckAlign")));
			this.chkDisplayWelcome.Checked = true;
			this.chkDisplayWelcome.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkDisplayWelcome.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkDisplayWelcome.Dock")));
			this.chkDisplayWelcome.Enabled = ((bool)(resources.GetObject("chkDisplayWelcome.Enabled")));
			this.chkDisplayWelcome.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkDisplayWelcome.FlatStyle")));
			this.chkDisplayWelcome.Font = ((System.Drawing.Font)(resources.GetObject("chkDisplayWelcome.Font")));
			this.chkDisplayWelcome.Image = ((System.Drawing.Image)(resources.GetObject("chkDisplayWelcome.Image")));
			this.chkDisplayWelcome.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkDisplayWelcome.ImageAlign")));
			this.chkDisplayWelcome.ImageIndex = ((int)(resources.GetObject("chkDisplayWelcome.ImageIndex")));
			this.chkDisplayWelcome.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkDisplayWelcome.ImeMode")));
			this.chkDisplayWelcome.Location = ((System.Drawing.Point)(resources.GetObject("chkDisplayWelcome.Location")));
			this.chkDisplayWelcome.Name = "chkDisplayWelcome";
			this.chkDisplayWelcome.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkDisplayWelcome.RightToLeft")));
			this.chkDisplayWelcome.Size = ((System.Drawing.Size)(resources.GetObject("chkDisplayWelcome.Size")));
			this.chkDisplayWelcome.TabIndex = ((int)(resources.GetObject("chkDisplayWelcome.TabIndex")));
			this.chkDisplayWelcome.Text = resources.GetString("chkDisplayWelcome.Text");
			this.chkDisplayWelcome.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkDisplayWelcome.TextAlign")));
			this.toolTip.SetToolTip(this.chkDisplayWelcome, resources.GetString("chkDisplayWelcome.ToolTip"));
			this.chkDisplayWelcome.Visible = ((bool)(resources.GetObject("chkDisplayWelcome.Visible")));
			// 
			// lblWelcomeInfoBox
			// 
			this.lblWelcomeInfoBox.AccessibleDescription = resources.GetString("lblWelcomeInfoBox.AccessibleDescription");
			this.lblWelcomeInfoBox.AccessibleName = resources.GetString("lblWelcomeInfoBox.AccessibleName");
			this.lblWelcomeInfoBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblWelcomeInfoBox.Anchor")));
			this.lblWelcomeInfoBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lblWelcomeInfoBox.BackgroundImage")));
			this.lblWelcomeInfoBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblWelcomeInfoBox.Dock")));
			this.lblWelcomeInfoBox.Enabled = ((bool)(resources.GetObject("lblWelcomeInfoBox.Enabled")));
			this.lblWelcomeInfoBox.Font = ((System.Drawing.Font)(resources.GetObject("lblWelcomeInfoBox.Font")));
			this.lblWelcomeInfoBox.Icon = Divelements.WizardFramework.SystemIconType.Warning;
			this.lblWelcomeInfoBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblWelcomeInfoBox.ImeMode")));
			this.lblWelcomeInfoBox.Location = ((System.Drawing.Point)(resources.GetObject("lblWelcomeInfoBox.Location")));
			this.lblWelcomeInfoBox.Name = "lblWelcomeInfoBox";
			this.lblWelcomeInfoBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblWelcomeInfoBox.RightToLeft")));
			this.lblWelcomeInfoBox.Size = ((System.Drawing.Size)(resources.GetObject("lblWelcomeInfoBox.Size")));
			this.lblWelcomeInfoBox.TabIndex = ((int)(resources.GetObject("lblWelcomeInfoBox.TabIndex")));
			this.lblWelcomeInfoBox.Text = resources.GetString("lblWelcomeInfoBox.Text");
			this.lblWelcomeInfoBox.Text2 = resources.GetString("lblWelcomeInfoBox.Text2");
			this.toolTip.SetToolTip(this.lblWelcomeInfoBox, resources.GetString("lblWelcomeInfoBox.ToolTip"));
			this.lblWelcomeInfoBox.Visible = ((bool)(resources.GetObject("lblWelcomeInfoBox.Visible")));
			// 
			// lblWelcomeHelpMessage1
			// 
			this.lblWelcomeHelpMessage1.AccessibleDescription = resources.GetString("lblWelcomeHelpMessage1.AccessibleDescription");
			this.lblWelcomeHelpMessage1.AccessibleName = resources.GetString("lblWelcomeHelpMessage1.AccessibleName");
			this.lblWelcomeHelpMessage1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblWelcomeHelpMessage1.Anchor")));
			this.lblWelcomeHelpMessage1.AutoSize = ((bool)(resources.GetObject("lblWelcomeHelpMessage1.AutoSize")));
			this.lblWelcomeHelpMessage1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblWelcomeHelpMessage1.Dock")));
			this.lblWelcomeHelpMessage1.Enabled = ((bool)(resources.GetObject("lblWelcomeHelpMessage1.Enabled")));
			this.lblWelcomeHelpMessage1.Font = ((System.Drawing.Font)(resources.GetObject("lblWelcomeHelpMessage1.Font")));
			this.lblWelcomeHelpMessage1.Image = ((System.Drawing.Image)(resources.GetObject("lblWelcomeHelpMessage1.Image")));
			this.lblWelcomeHelpMessage1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblWelcomeHelpMessage1.ImageAlign")));
			this.lblWelcomeHelpMessage1.ImageIndex = ((int)(resources.GetObject("lblWelcomeHelpMessage1.ImageIndex")));
			this.lblWelcomeHelpMessage1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblWelcomeHelpMessage1.ImeMode")));
			this.lblWelcomeHelpMessage1.Location = ((System.Drawing.Point)(resources.GetObject("lblWelcomeHelpMessage1.Location")));
			this.lblWelcomeHelpMessage1.Name = "lblWelcomeHelpMessage1";
			this.lblWelcomeHelpMessage1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblWelcomeHelpMessage1.RightToLeft")));
			this.lblWelcomeHelpMessage1.Size = ((System.Drawing.Size)(resources.GetObject("lblWelcomeHelpMessage1.Size")));
			this.lblWelcomeHelpMessage1.TabIndex = ((int)(resources.GetObject("lblWelcomeHelpMessage1.TabIndex")));
			this.lblWelcomeHelpMessage1.Text = resources.GetString("lblWelcomeHelpMessage1.Text");
			this.lblWelcomeHelpMessage1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblWelcomeHelpMessage1.TextAlign")));
			this.toolTip.SetToolTip(this.lblWelcomeHelpMessage1, resources.GetString("lblWelcomeHelpMessage1.ToolTip"));
			this.lblWelcomeHelpMessage1.Visible = ((bool)(resources.GetObject("lblWelcomeHelpMessage1.Visible")));
			// 
			// lblWelcomeHelpMessage2
			// 
			this.lblWelcomeHelpMessage2.AccessibleDescription = resources.GetString("lblWelcomeHelpMessage2.AccessibleDescription");
			this.lblWelcomeHelpMessage2.AccessibleName = resources.GetString("lblWelcomeHelpMessage2.AccessibleName");
			this.lblWelcomeHelpMessage2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblWelcomeHelpMessage2.Anchor")));
			this.lblWelcomeHelpMessage2.AutoSize = ((bool)(resources.GetObject("lblWelcomeHelpMessage2.AutoSize")));
			this.lblWelcomeHelpMessage2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblWelcomeHelpMessage2.Dock")));
			this.lblWelcomeHelpMessage2.Enabled = ((bool)(resources.GetObject("lblWelcomeHelpMessage2.Enabled")));
			this.lblWelcomeHelpMessage2.Font = ((System.Drawing.Font)(resources.GetObject("lblWelcomeHelpMessage2.Font")));
			this.lblWelcomeHelpMessage2.Image = ((System.Drawing.Image)(resources.GetObject("lblWelcomeHelpMessage2.Image")));
			this.lblWelcomeHelpMessage2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblWelcomeHelpMessage2.ImageAlign")));
			this.lblWelcomeHelpMessage2.ImageIndex = ((int)(resources.GetObject("lblWelcomeHelpMessage2.ImageIndex")));
			this.lblWelcomeHelpMessage2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblWelcomeHelpMessage2.ImeMode")));
			this.lblWelcomeHelpMessage2.Location = ((System.Drawing.Point)(resources.GetObject("lblWelcomeHelpMessage2.Location")));
			this.lblWelcomeHelpMessage2.Name = "lblWelcomeHelpMessage2";
			this.lblWelcomeHelpMessage2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblWelcomeHelpMessage2.RightToLeft")));
			this.lblWelcomeHelpMessage2.Size = ((System.Drawing.Size)(resources.GetObject("lblWelcomeHelpMessage2.Size")));
			this.lblWelcomeHelpMessage2.TabIndex = ((int)(resources.GetObject("lblWelcomeHelpMessage2.TabIndex")));
			this.lblWelcomeHelpMessage2.Text = resources.GetString("lblWelcomeHelpMessage2.Text");
			this.lblWelcomeHelpMessage2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblWelcomeHelpMessage2.TextAlign")));
			this.toolTip.SetToolTip(this.lblWelcomeHelpMessage2, resources.GetString("lblWelcomeHelpMessage2.ToolTip"));
			this.lblWelcomeHelpMessage2.Visible = ((bool)(resources.GetObject("lblWelcomeHelpMessage2.Visible")));
			// 
			// pageNewByNNTPGroup
			// 
			this.pageNewByNNTPGroup.AccessibleDescription = resources.GetString("pageNewByNNTPGroup.AccessibleDescription");
			this.pageNewByNNTPGroup.AccessibleName = resources.GetString("pageNewByNNTPGroup.AccessibleName");
			this.pageNewByNNTPGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pageNewByNNTPGroup.Anchor")));
			this.pageNewByNNTPGroup.AutoScroll = ((bool)(resources.GetObject("pageNewByNNTPGroup.AutoScroll")));
			this.pageNewByNNTPGroup.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("pageNewByNNTPGroup.AutoScrollMargin")));
			this.pageNewByNNTPGroup.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("pageNewByNNTPGroup.AutoScrollMinSize")));
			this.pageNewByNNTPGroup.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pageNewByNNTPGroup.BackgroundImage")));
			this.pageNewByNNTPGroup.Controls.Add(this.lstNNTPGroups);
			this.pageNewByNNTPGroup.Controls.Add(this.lblUsenetHelp);
			this.pageNewByNNTPGroup.Controls.Add(this.pictureBox1);
			this.pageNewByNNTPGroup.Controls.Add(this.lblReloadNntpListOfGroups);
			this.pageNewByNNTPGroup.Controls.Add(this.lblNNTPGroups);
			this.pageNewByNNTPGroup.Controls.Add(this.btnManageNNTPServer);
			this.pageNewByNNTPGroup.Controls.Add(this.lblNNTPServer);
			this.pageNewByNNTPGroup.Controls.Add(this.cboNNTPServer);
			this.pageNewByNNTPGroup.Controls.Add(this.lblNewByNNTPGroupIntro);
			this.pageNewByNNTPGroup.Description = resources.GetString("pageNewByNNTPGroup.Description");
			this.pageNewByNNTPGroup.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pageNewByNNTPGroup.Dock")));
			this.pageNewByNNTPGroup.Enabled = ((bool)(resources.GetObject("pageNewByNNTPGroup.Enabled")));
			this.pageNewByNNTPGroup.Font = ((System.Drawing.Font)(resources.GetObject("pageNewByNNTPGroup.Font")));
			this.pageNewByNNTPGroup.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pageNewByNNTPGroup.ImeMode")));
			this.pageNewByNNTPGroup.Location = ((System.Drawing.Point)(resources.GetObject("pageNewByNNTPGroup.Location")));
			this.pageNewByNNTPGroup.Name = "pageNewByNNTPGroup";
			this.pageNewByNNTPGroup.NextPage = this.pageTitleCategory;
			this.pageNewByNNTPGroup.PreviousPage = this.pageHowToSelection;
			this.pageNewByNNTPGroup.ProceedText = resources.GetString("pageNewByNNTPGroup.ProceedText");
			this.pageNewByNNTPGroup.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pageNewByNNTPGroup.RightToLeft")));
			this.pageNewByNNTPGroup.Size = ((System.Drawing.Size)(resources.GetObject("pageNewByNNTPGroup.Size")));
			this.pageNewByNNTPGroup.TabIndex = ((int)(resources.GetObject("pageNewByNNTPGroup.TabIndex")));
			this.pageNewByNNTPGroup.Text = resources.GetString("pageNewByNNTPGroup.Text");
			this.toolTip.SetToolTip(this.pageNewByNNTPGroup, resources.GetString("pageNewByNNTPGroup.ToolTip"));
			this.pageNewByNNTPGroup.Visible = ((bool)(resources.GetObject("pageNewByNNTPGroup.Visible")));
			this.pageNewByNNTPGroup.AfterDisplay += new System.EventHandler(this.OnPageNewNNTPGroupAfterDisplay);
			// 
			// lstNNTPGroups
			// 
			this.lstNNTPGroups.AccessibleDescription = resources.GetString("lstNNTPGroups.AccessibleDescription");
			this.lstNNTPGroups.AccessibleName = resources.GetString("lstNNTPGroups.AccessibleName");
			this.lstNNTPGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lstNNTPGroups.Anchor")));
			this.lstNNTPGroups.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lstNNTPGroups.BackgroundImage")));
			this.lstNNTPGroups.ColumnWidth = ((int)(resources.GetObject("lstNNTPGroups.ColumnWidth")));
			this.lstNNTPGroups.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lstNNTPGroups.Dock")));
			this.lstNNTPGroups.Enabled = ((bool)(resources.GetObject("lstNNTPGroups.Enabled")));
			this.lstNNTPGroups.Font = ((System.Drawing.Font)(resources.GetObject("lstNNTPGroups.Font")));
			this.lstNNTPGroups.HorizontalExtent = ((int)(resources.GetObject("lstNNTPGroups.HorizontalExtent")));
			this.lstNNTPGroups.HorizontalScrollbar = ((bool)(resources.GetObject("lstNNTPGroups.HorizontalScrollbar")));
			this.lstNNTPGroups.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lstNNTPGroups.ImeMode")));
			this.lstNNTPGroups.IntegralHeight = ((bool)(resources.GetObject("lstNNTPGroups.IntegralHeight")));
			this.lstNNTPGroups.ItemHeight = ((int)(resources.GetObject("lstNNTPGroups.ItemHeight")));
			this.lstNNTPGroups.Location = ((System.Drawing.Point)(resources.GetObject("lstNNTPGroups.Location")));
			this.lstNNTPGroups.Name = "lstNNTPGroups";
			this.lstNNTPGroups.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lstNNTPGroups.RightToLeft")));
			this.lstNNTPGroups.ScrollAlwaysVisible = ((bool)(resources.GetObject("lstNNTPGroups.ScrollAlwaysVisible")));
			this.lstNNTPGroups.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.lstNNTPGroups.Size = ((System.Drawing.Size)(resources.GetObject("lstNNTPGroups.Size")));
			this.lstNNTPGroups.Sorted = true;
			this.lstNNTPGroups.TabIndex = ((int)(resources.GetObject("lstNNTPGroups.TabIndex")));
			this.toolTip.SetToolTip(this.lstNNTPGroups, resources.GetString("lstNNTPGroups.ToolTip"));
			this.lstNNTPGroups.Visible = ((bool)(resources.GetObject("lstNNTPGroups.Visible")));
			this.lstNNTPGroups.DoubleClick += new System.EventHandler(this.OnNNTPGroupsDoubleClick);
			this.lstNNTPGroups.SelectedValueChanged += new System.EventHandler(this.OnNNTPGroupsListSelectedValueChanged);
			// 
			// lblUsenetHelp
			// 
			this.lblUsenetHelp.AccessibleDescription = resources.GetString("lblUsenetHelp.AccessibleDescription");
			this.lblUsenetHelp.AccessibleName = resources.GetString("lblUsenetHelp.AccessibleName");
			this.lblUsenetHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblUsenetHelp.Anchor")));
			this.lblUsenetHelp.AutoSize = ((bool)(resources.GetObject("lblUsenetHelp.AutoSize")));
			this.lblUsenetHelp.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblUsenetHelp.Dock")));
			this.lblUsenetHelp.Enabled = ((bool)(resources.GetObject("lblUsenetHelp.Enabled")));
			this.lblUsenetHelp.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblUsenetHelp.Font = ((System.Drawing.Font)(resources.GetObject("lblUsenetHelp.Font")));
			this.lblUsenetHelp.Image = ((System.Drawing.Image)(resources.GetObject("lblUsenetHelp.Image")));
			this.lblUsenetHelp.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUsenetHelp.ImageAlign")));
			this.lblUsenetHelp.ImageIndex = ((int)(resources.GetObject("lblUsenetHelp.ImageIndex")));
			this.lblUsenetHelp.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblUsenetHelp.ImeMode")));
			this.lblUsenetHelp.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblUsenetHelp.LinkArea")));
			this.lblUsenetHelp.Location = ((System.Drawing.Point)(resources.GetObject("lblUsenetHelp.Location")));
			this.lblUsenetHelp.Name = "lblUsenetHelp";
			this.lblUsenetHelp.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblUsenetHelp.RightToLeft")));
			this.lblUsenetHelp.Size = ((System.Drawing.Size)(resources.GetObject("lblUsenetHelp.Size")));
			this.lblUsenetHelp.TabIndex = ((int)(resources.GetObject("lblUsenetHelp.TabIndex")));
			this.lblUsenetHelp.TabStop = true;
			this.lblUsenetHelp.Tag = "http://wikipedia.org/wiki/Usenet";
			this.lblUsenetHelp.Text = resources.GetString("lblUsenetHelp.Text");
			this.lblUsenetHelp.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblUsenetHelp.TextAlign")));
			this.toolTip.SetToolTip(this.lblUsenetHelp, resources.GetString("lblUsenetHelp.ToolTip"));
			this.lblUsenetHelp.Visible = ((bool)(resources.GetObject("lblUsenetHelp.Visible")));
			this.lblUsenetHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAnyLinkLabel_LinkClicked);
			// 
			// pictureBox1
			// 
			this.pictureBox1.AccessibleDescription = resources.GetString("pictureBox1.AccessibleDescription");
			this.pictureBox1.AccessibleName = resources.GetString("pictureBox1.AccessibleName");
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox1.Anchor")));
			this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
			this.pictureBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox1.Dock")));
			this.pictureBox1.Enabled = ((bool)(resources.GetObject("pictureBox1.Enabled")));
			this.pictureBox1.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox1.Font")));
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox1.ImeMode")));
			this.pictureBox1.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox1.Location")));
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox1.RightToLeft")));
			this.pictureBox1.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox1.Size")));
			this.pictureBox1.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox1.SizeMode")));
			this.pictureBox1.TabIndex = ((int)(resources.GetObject("pictureBox1.TabIndex")));
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Text = resources.GetString("pictureBox1.Text");
			this.toolTip.SetToolTip(this.pictureBox1, resources.GetString("pictureBox1.ToolTip"));
			this.pictureBox1.Visible = ((bool)(resources.GetObject("pictureBox1.Visible")));
			// 
			// lblReloadNntpListOfGroups
			// 
			this.lblReloadNntpListOfGroups.AccessibleDescription = resources.GetString("lblReloadNntpListOfGroups.AccessibleDescription");
			this.lblReloadNntpListOfGroups.AccessibleName = resources.GetString("lblReloadNntpListOfGroups.AccessibleName");
			this.lblReloadNntpListOfGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblReloadNntpListOfGroups.Anchor")));
			this.lblReloadNntpListOfGroups.AutoSize = ((bool)(resources.GetObject("lblReloadNntpListOfGroups.AutoSize")));
			this.lblReloadNntpListOfGroups.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblReloadNntpListOfGroups.Dock")));
			this.lblReloadNntpListOfGroups.Enabled = ((bool)(resources.GetObject("lblReloadNntpListOfGroups.Enabled")));
			this.lblReloadNntpListOfGroups.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblReloadNntpListOfGroups.Font = ((System.Drawing.Font)(resources.GetObject("lblReloadNntpListOfGroups.Font")));
			this.lblReloadNntpListOfGroups.Image = ((System.Drawing.Image)(resources.GetObject("lblReloadNntpListOfGroups.Image")));
			this.lblReloadNntpListOfGroups.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblReloadNntpListOfGroups.ImageAlign")));
			this.lblReloadNntpListOfGroups.ImageIndex = ((int)(resources.GetObject("lblReloadNntpListOfGroups.ImageIndex")));
			this.lblReloadNntpListOfGroups.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblReloadNntpListOfGroups.ImeMode")));
			this.lblReloadNntpListOfGroups.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblReloadNntpListOfGroups.LinkArea")));
			this.lblReloadNntpListOfGroups.Location = ((System.Drawing.Point)(resources.GetObject("lblReloadNntpListOfGroups.Location")));
			this.lblReloadNntpListOfGroups.Name = "lblReloadNntpListOfGroups";
			this.lblReloadNntpListOfGroups.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblReloadNntpListOfGroups.RightToLeft")));
			this.lblReloadNntpListOfGroups.Size = ((System.Drawing.Size)(resources.GetObject("lblReloadNntpListOfGroups.Size")));
			this.lblReloadNntpListOfGroups.TabIndex = ((int)(resources.GetObject("lblReloadNntpListOfGroups.TabIndex")));
			this.lblReloadNntpListOfGroups.TabStop = true;
			this.lblReloadNntpListOfGroups.Text = resources.GetString("lblReloadNntpListOfGroups.Text");
			this.lblReloadNntpListOfGroups.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblReloadNntpListOfGroups.TextAlign")));
			this.toolTip.SetToolTip(this.lblReloadNntpListOfGroups, resources.GetString("lblReloadNntpListOfGroups.ToolTip"));
			this.lblReloadNntpListOfGroups.Visible = ((bool)(resources.GetObject("lblReloadNntpListOfGroups.Visible")));
			this.lblReloadNntpListOfGroups.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnReloadNntpGroupList);
			// 
			// lblNNTPGroups
			// 
			this.lblNNTPGroups.AccessibleDescription = resources.GetString("lblNNTPGroups.AccessibleDescription");
			this.lblNNTPGroups.AccessibleName = resources.GetString("lblNNTPGroups.AccessibleName");
			this.lblNNTPGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNNTPGroups.Anchor")));
			this.lblNNTPGroups.AutoSize = ((bool)(resources.GetObject("lblNNTPGroups.AutoSize")));
			this.lblNNTPGroups.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNNTPGroups.Dock")));
			this.lblNNTPGroups.Enabled = ((bool)(resources.GetObject("lblNNTPGroups.Enabled")));
			this.lblNNTPGroups.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNNTPGroups.Font = ((System.Drawing.Font)(resources.GetObject("lblNNTPGroups.Font")));
			this.lblNNTPGroups.Image = ((System.Drawing.Image)(resources.GetObject("lblNNTPGroups.Image")));
			this.lblNNTPGroups.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNNTPGroups.ImageAlign")));
			this.lblNNTPGroups.ImageIndex = ((int)(resources.GetObject("lblNNTPGroups.ImageIndex")));
			this.lblNNTPGroups.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNNTPGroups.ImeMode")));
			this.lblNNTPGroups.Location = ((System.Drawing.Point)(resources.GetObject("lblNNTPGroups.Location")));
			this.lblNNTPGroups.Name = "lblNNTPGroups";
			this.lblNNTPGroups.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNNTPGroups.RightToLeft")));
			this.lblNNTPGroups.Size = ((System.Drawing.Size)(resources.GetObject("lblNNTPGroups.Size")));
			this.lblNNTPGroups.TabIndex = ((int)(resources.GetObject("lblNNTPGroups.TabIndex")));
			this.lblNNTPGroups.Text = resources.GetString("lblNNTPGroups.Text");
			this.lblNNTPGroups.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNNTPGroups.TextAlign")));
			this.toolTip.SetToolTip(this.lblNNTPGroups, resources.GetString("lblNNTPGroups.ToolTip"));
			this.lblNNTPGroups.Visible = ((bool)(resources.GetObject("lblNNTPGroups.Visible")));
			// 
			// btnManageNNTPServer
			// 
			this.btnManageNNTPServer.AccessibleDescription = resources.GetString("btnManageNNTPServer.AccessibleDescription");
			this.btnManageNNTPServer.AccessibleName = resources.GetString("btnManageNNTPServer.AccessibleName");
			this.btnManageNNTPServer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnManageNNTPServer.Anchor")));
			this.btnManageNNTPServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnManageNNTPServer.BackgroundImage")));
			this.btnManageNNTPServer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnManageNNTPServer.Dock")));
			this.btnManageNNTPServer.Enabled = ((bool)(resources.GetObject("btnManageNNTPServer.Enabled")));
			this.btnManageNNTPServer.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnManageNNTPServer.FlatStyle")));
			this.btnManageNNTPServer.Font = ((System.Drawing.Font)(resources.GetObject("btnManageNNTPServer.Font")));
			this.btnManageNNTPServer.Image = ((System.Drawing.Image)(resources.GetObject("btnManageNNTPServer.Image")));
			this.btnManageNNTPServer.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageNNTPServer.ImageAlign")));
			this.btnManageNNTPServer.ImageIndex = ((int)(resources.GetObject("btnManageNNTPServer.ImageIndex")));
			this.btnManageNNTPServer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnManageNNTPServer.ImeMode")));
			this.btnManageNNTPServer.Location = ((System.Drawing.Point)(resources.GetObject("btnManageNNTPServer.Location")));
			this.btnManageNNTPServer.Name = "btnManageNNTPServer";
			this.btnManageNNTPServer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnManageNNTPServer.RightToLeft")));
			this.btnManageNNTPServer.Size = ((System.Drawing.Size)(resources.GetObject("btnManageNNTPServer.Size")));
			this.btnManageNNTPServer.TabIndex = ((int)(resources.GetObject("btnManageNNTPServer.TabIndex")));
			this.btnManageNNTPServer.Text = resources.GetString("btnManageNNTPServer.Text");
			this.btnManageNNTPServer.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnManageNNTPServer.TextAlign")));
			this.toolTip.SetToolTip(this.btnManageNNTPServer, resources.GetString("btnManageNNTPServer.ToolTip"));
			this.btnManageNNTPServer.Visible = ((bool)(resources.GetObject("btnManageNNTPServer.Visible")));
			this.btnManageNNTPServer.Click += new System.EventHandler(this.btnManageNNTPServer_Click);
			// 
			// lblNNTPServer
			// 
			this.lblNNTPServer.AccessibleDescription = resources.GetString("lblNNTPServer.AccessibleDescription");
			this.lblNNTPServer.AccessibleName = resources.GetString("lblNNTPServer.AccessibleName");
			this.lblNNTPServer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNNTPServer.Anchor")));
			this.lblNNTPServer.AutoSize = ((bool)(resources.GetObject("lblNNTPServer.AutoSize")));
			this.lblNNTPServer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNNTPServer.Dock")));
			this.lblNNTPServer.Enabled = ((bool)(resources.GetObject("lblNNTPServer.Enabled")));
			this.lblNNTPServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNNTPServer.Font = ((System.Drawing.Font)(resources.GetObject("lblNNTPServer.Font")));
			this.lblNNTPServer.Image = ((System.Drawing.Image)(resources.GetObject("lblNNTPServer.Image")));
			this.lblNNTPServer.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNNTPServer.ImageAlign")));
			this.lblNNTPServer.ImageIndex = ((int)(resources.GetObject("lblNNTPServer.ImageIndex")));
			this.lblNNTPServer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNNTPServer.ImeMode")));
			this.lblNNTPServer.Location = ((System.Drawing.Point)(resources.GetObject("lblNNTPServer.Location")));
			this.lblNNTPServer.Name = "lblNNTPServer";
			this.lblNNTPServer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNNTPServer.RightToLeft")));
			this.lblNNTPServer.Size = ((System.Drawing.Size)(resources.GetObject("lblNNTPServer.Size")));
			this.lblNNTPServer.TabIndex = ((int)(resources.GetObject("lblNNTPServer.TabIndex")));
			this.lblNNTPServer.Text = resources.GetString("lblNNTPServer.Text");
			this.lblNNTPServer.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNNTPServer.TextAlign")));
			this.toolTip.SetToolTip(this.lblNNTPServer, resources.GetString("lblNNTPServer.ToolTip"));
			this.lblNNTPServer.Visible = ((bool)(resources.GetObject("lblNNTPServer.Visible")));
			// 
			// cboNNTPServer
			// 
			this.cboNNTPServer.AccessibleDescription = resources.GetString("cboNNTPServer.AccessibleDescription");
			this.cboNNTPServer.AccessibleName = resources.GetString("cboNNTPServer.AccessibleName");
			this.cboNNTPServer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cboNNTPServer.Anchor")));
			this.cboNNTPServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cboNNTPServer.BackgroundImage")));
			this.cboNNTPServer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cboNNTPServer.Dock")));
			this.cboNNTPServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboNNTPServer.Enabled = ((bool)(resources.GetObject("cboNNTPServer.Enabled")));
			this.cboNNTPServer.Font = ((System.Drawing.Font)(resources.GetObject("cboNNTPServer.Font")));
			this.cboNNTPServer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cboNNTPServer.ImeMode")));
			this.cboNNTPServer.IntegralHeight = ((bool)(resources.GetObject("cboNNTPServer.IntegralHeight")));
			this.cboNNTPServer.ItemHeight = ((int)(resources.GetObject("cboNNTPServer.ItemHeight")));
			this.cboNNTPServer.Location = ((System.Drawing.Point)(resources.GetObject("cboNNTPServer.Location")));
			this.cboNNTPServer.MaxDropDownItems = ((int)(resources.GetObject("cboNNTPServer.MaxDropDownItems")));
			this.cboNNTPServer.MaxLength = ((int)(resources.GetObject("cboNNTPServer.MaxLength")));
			this.cboNNTPServer.Name = "cboNNTPServer";
			this.cboNNTPServer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cboNNTPServer.RightToLeft")));
			this.cboNNTPServer.Size = ((System.Drawing.Size)(resources.GetObject("cboNNTPServer.Size")));
			this.cboNNTPServer.TabIndex = ((int)(resources.GetObject("cboNNTPServer.TabIndex")));
			this.cboNNTPServer.Text = resources.GetString("cboNNTPServer.Text");
			this.toolTip.SetToolTip(this.cboNNTPServer, resources.GetString("cboNNTPServer.ToolTip"));
			this.cboNNTPServer.Visible = ((bool)(resources.GetObject("cboNNTPServer.Visible")));
			this.cboNNTPServer.SelectedValueChanged += new System.EventHandler(this.OnNNTPServerSelectedValueChanged);
			// 
			// lblNewByNNTPGroupIntro
			// 
			this.lblNewByNNTPGroupIntro.AccessibleDescription = resources.GetString("lblNewByNNTPGroupIntro.AccessibleDescription");
			this.lblNewByNNTPGroupIntro.AccessibleName = resources.GetString("lblNewByNNTPGroupIntro.AccessibleName");
			this.lblNewByNNTPGroupIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNewByNNTPGroupIntro.Anchor")));
			this.lblNewByNNTPGroupIntro.AutoSize = ((bool)(resources.GetObject("lblNewByNNTPGroupIntro.AutoSize")));
			this.lblNewByNNTPGroupIntro.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNewByNNTPGroupIntro.Dock")));
			this.lblNewByNNTPGroupIntro.Enabled = ((bool)(resources.GetObject("lblNewByNNTPGroupIntro.Enabled")));
			this.lblNewByNNTPGroupIntro.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblNewByNNTPGroupIntro.Font = ((System.Drawing.Font)(resources.GetObject("lblNewByNNTPGroupIntro.Font")));
			this.lblNewByNNTPGroupIntro.Image = ((System.Drawing.Image)(resources.GetObject("lblNewByNNTPGroupIntro.Image")));
			this.lblNewByNNTPGroupIntro.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewByNNTPGroupIntro.ImageAlign")));
			this.lblNewByNNTPGroupIntro.ImageIndex = ((int)(resources.GetObject("lblNewByNNTPGroupIntro.ImageIndex")));
			this.lblNewByNNTPGroupIntro.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNewByNNTPGroupIntro.ImeMode")));
			this.lblNewByNNTPGroupIntro.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblNewByNNTPGroupIntro.LinkArea")));
			this.lblNewByNNTPGroupIntro.Location = ((System.Drawing.Point)(resources.GetObject("lblNewByNNTPGroupIntro.Location")));
			this.lblNewByNNTPGroupIntro.Name = "lblNewByNNTPGroupIntro";
			this.lblNewByNNTPGroupIntro.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNewByNNTPGroupIntro.RightToLeft")));
			this.lblNewByNNTPGroupIntro.Size = ((System.Drawing.Size)(resources.GetObject("lblNewByNNTPGroupIntro.Size")));
			this.lblNewByNNTPGroupIntro.TabIndex = ((int)(resources.GetObject("lblNewByNNTPGroupIntro.TabIndex")));
			this.lblNewByNNTPGroupIntro.Tag = "";
			this.lblNewByNNTPGroupIntro.Text = resources.GetString("lblNewByNNTPGroupIntro.Text");
			this.lblNewByNNTPGroupIntro.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewByNNTPGroupIntro.TextAlign")));
			this.toolTip.SetToolTip(this.lblNewByNNTPGroupIntro, resources.GetString("lblNewByNNTPGroupIntro.ToolTip"));
			this.lblNewByNNTPGroupIntro.Visible = ((bool)(resources.GetObject("lblNewByNNTPGroupIntro.Visible")));
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
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.wizard);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "AddSubscriptionWizard";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.wizard.ResumeLayout(false);
			this.pageNewBySearchTopic.ResumeLayout(false);
			this.pageValidateUrl.ResumeLayout(false);
			this.pageFoundMultipleFeeds.ResumeLayout(false);
			this.pageTitleCategory.ResumeLayout(false);
			this.pageFeedCredentials.ResumeLayout(false);
			this.pageFeedItemControl.ResumeLayout(false);
			this.pageFeedItemDisplay.ResumeLayout(false);
			this.finishPage.ResumeLayout(false);
			this.pnlCompleting.ResumeLayout(false);
			this.pageHowToSelection.ResumeLayout(false);
			this.pageNewByURL.ResumeLayout(false);
			this.pageWelcome.ResumeLayout(false);
			this.pageNewByNNTPGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void WireStepsForMode(WizardMode m) 
		{
			// reset rewire credential steps:
			this.ReWireCredentialsStep(false);
			this.wizardMode = m;
			switch (m) {
				case WizardMode.Default:
					// nothing yet. Depends on user selection (pageHowToSelection)
					break;
				case WizardMode.SubscribeURL:
					pageHowToSelection.NextPage = pageNewByURL;
					break;
				case WizardMode.SubscribeURLDirect:
					pageWelcome.NextPage = pageNewByURL;
					pageNewByURL.PreviousPage = pageWelcome;
					pageTitleCategory.PreviousPage = pageNewByURL;
					break;
				case WizardMode.SubscribeNNTPGroup:
					pageHowToSelection.NextPage = pageNewByNNTPGroup;
					pageTitleCategory.PreviousPage = pageNewByNNTPGroup;
					break;
				case WizardMode.SubscribeNNTPDirect:
					pageWelcome.NextPage = pageNewByNNTPGroup;
					pageNewByNNTPGroup.PreviousPage = pageWelcome;
					pageTitleCategory.PreviousPage = pageNewByNNTPGroup;
					break;
				case WizardMode.SubscribeSearch:
					pageHowToSelection.NextPage = pageNewBySearchTopic;
					pageFoundMultipleFeeds.PreviousPage = pageNewBySearchTopic;
					pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
					break;
				case WizardMode.SubscribeSearchDirect:
					pageWelcome.NextPage = pageNewBySearchTopic;
					pageNewBySearchTopic.PreviousPage = pageWelcome;
					pageFoundMultipleFeeds.PreviousPage = pageNewBySearchTopic;
					pageTitleCategory.PreviousPage = pageFoundMultipleFeeds;
					break;
				case WizardMode.SubscribeNNTPGroupDirect:
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
					autoDiscover.WebPageUrl = this.txtNewByURL.Text;
					autoDiscover.OperationMessage = SR.GUIStatusWaitMessageDetectingFeeds(this.txtNewBySearchWords.Text);
				}
				
				if (this.textUser.Text.Trim().Length > 0) {
					autoDiscover.Credentials = NewsHandler.CreateCredentialsFrom(this.textUser.Text.Trim(), this.textPassword.Text.Trim());
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
					fetchHandler.Credentials = NewsHandler.CreateCredentialsFrom(this.textUser.Text.Trim(), this.textPassword.Text.Trim());
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
			if (StringHelper.EmptyOrNull(nntpServer)) {
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
			bool valid = ( ! StringHelper.EmptyOrNull(validUrl) && ex == null);
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
		/// <returns>valid Url or null</returns>
		private string ValidateFeedUri(string url, out Exception invalidUriException) {
			invalidUriException = null;

			if (StringHelper.EmptyTrimOrNull(url))
				return String.Empty;

			// canonicalize the provided url:
			string newUrl = HtmlHelper.HtmlDecode(url);
			
			//some weird URLs have newlines. 
			// We remove them before create the Uri, so we do not fail on THAT error there	
			newUrl = newUrl.Replace(Environment.NewLine, String.Empty).Replace(@"\n", String.Empty); 
			
			//handle the common case of feed URI not beginning with HTTP 
			try{ 
				Uri reqUri = new Uri(newUrl);
				newUrl     = reqUri.AbsoluteUri;
			}catch(UriFormatException){

				if(!url.ToLower().StartsWith("http://")){
					try {		
						Uri reqUri = new Uri("http://" + newUrl); 
						newUrl     = reqUri.AbsoluteUri;
					} catch (UriFormatException ex) {
						invalidUriException = ex;
						return null;
					}
				}
				
			}
	
			IDictionary subscribed = coreApplication.Subscriptions;
			if(subscribed.Contains(newUrl)) {
				feedsFeed f2 = (feedsFeed)subscribed[newUrl]; 
				invalidUriException = new InvalidOperationException( 
					SR.GUIFieldLinkRedundantInfo( 
					(f2.category == null? String.Empty : f2.category + NewsHandler.CategorySeparator) + f2.title, f2.link ));
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
				if ((this.wizardMode == WizardMode.SubscribeNNTPDirect ||
					this.wizardMode == WizardMode.SubscribeNNTPGroup ||
					this.wizardMode == WizardMode.SubscribeNNTPGroupDirect) &&
					this.lstNNTPGroups.SelectedItems.Count > 1)
					return true;
				if ((this.wizardMode == WizardMode.SubscribeURL ||
					this.wizardMode == WizardMode.SubscribeURLDirect ) &&
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
				if ((this.wizardMode == WizardMode.SubscribeNNTPDirect ||
					this.wizardMode == WizardMode.SubscribeNNTPGroup ||
					this.wizardMode == WizardMode.SubscribeNNTPGroupDirect))
					return this.lstNNTPGroups.SelectedItems.Count;
				if ((this.wizardMode == WizardMode.SubscribeURL ||
					this.wizardMode == WizardMode.SubscribeURLDirect ))
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
			
			if (this.wizardMode == WizardMode.SubscribeNNTPDirect ||
				this.wizardMode == WizardMode.SubscribeNNTPGroup ||
				this.wizardMode == WizardMode.SubscribeNNTPGroupDirect) {
				
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

			} else if (this.wizardMode == WizardMode.SubscribeURL ||
					this.wizardMode == WizardMode.SubscribeURLDirect ) {
				
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
			
			if (this.wizardMode == WizardMode.SubscribeNNTPDirect ||
				this.wizardMode == WizardMode.SubscribeNNTPGroup ||
				this.wizardMode == WizardMode.SubscribeNNTPGroupDirect) {
				
				if (this.lstNNTPGroups.SelectedItems.Count > 1)
					return this.lstNNTPGroups.SelectedItems[index].ToString(); 
				else
					return this.FeedTitle;

			} else if (this.wizardMode == WizardMode.SubscribeURL ||
				this.wizardMode == WizardMode.SubscribeURLDirect ) {
				
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
				if (StringHelper.EmptyOrNull(c))
					return null;
				if (coreApplication.DefaultCategory.Equals(c))
					return null;
				return c;
			} 
			set
			{
				if (!StringHelper.EmptyOrNull(value))
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
					if (StringHelper.EmptyOrNull(this.comboFormatters.Text))
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
		/// Gets the refresh rate.
		/// </summary>
		/// <value>The refresh rate.</value>
		public int RefreshRate {
			get {
				// should be yet validated. But for safety:
				try { 

					if((!StringHelper.EmptyOrNull(this.cboUpdateFrequency.Text.Trim()))) {
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
						return 60;
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
			if (StringHelper.EmptyOrNull(this.FeedCategory) ||
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
			if (!StringHelper.EmptyOrNull(this.FeedCategory) && 
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

		private void OnImmediateFinish_Click(object sender, System.EventArgs e)
		{
			this.ProcessFeedUrl();
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
				WireStepsForMode(WizardMode.SubscribeURL);
			else if (sender == radioNewByTopicSearch)
				WireStepsForMode(WizardMode.SubscribeSearch);
			else if (sender == radioNewByNNTPGroup)
				WireStepsForMode(WizardMode.SubscribeNNTPGroup);
		}

		private void OnAutodiscoverVerifyCheckedChanged(object sender, System.EventArgs e) {
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

		private void OnNewFeedUrlTextChanged(object sender, System.EventArgs e) {
			Exception ex = null;
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

		private void OnPageTitleCategoryBeforeDisplay(object sender, System.EventArgs e) {
			this._btnImmediateFinish.Visible = true;
			this.txtFeedTitle.Enabled = !this.MultipleFeedsToSubscribe;

			if (this.wizardMode == WizardMode.SubscribeNNTPDirect ||
				this.wizardMode == WizardMode.SubscribeNNTPGroup ||
				this.wizardMode == WizardMode.SubscribeNNTPGroupDirect ||
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
			this.WireStepsForMode(WizardMode.SubscribeURL);
		}

		private void OnPageNewNNTPGroupAfterDisplay(object sender, System.EventArgs e) {
			this.feedInfo = null;
			pageNewByNNTPGroup.AllowMoveNext = false;
			PopulateNntpServerDefinitions((IDictionary<string, INntpServerDefinition>) coreApplication.NntpServerDefinitions, null);
		}

		private void OnPageNewURLAfterDisplay(object sender, System.EventArgs e) {
			this.feedInfo = null;
			if (StringHelper.EmptyOrNull(this.txtNewByURL.Text))
				pageNewByURL.AllowMoveNext = false;
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
				(this.wizardMode == WizardMode.SubscribeNNTPDirect) ||
				(this.wizardMode == WizardMode.SubscribeNNTPGroup) ){
				
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

		private void OnTimerStartValidation(object sender, System.EventArgs e) {
			
			this.timerStartValidation.Enabled = false;
			
			if (this.wizardMode == WizardMode.SubscribeURL || this.wizardMode == WizardMode.SubscribeURLDirect) {
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
					
			} else if (this.wizardMode == WizardMode.SubscribeSearch || this.wizardMode == WizardMode.SubscribeSearchDirect) {

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
	internal enum WizardMode {
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
