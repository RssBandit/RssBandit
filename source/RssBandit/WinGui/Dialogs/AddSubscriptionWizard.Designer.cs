
namespace RssBandit.WinGui.Dialogs
{
	partial class AddSubscriptionWizard
	{
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
			this.pageTitleCategory.SuspendLayout();
			this.pageFeedCredentials.SuspendLayout();
			this.pageFeedItemControl.SuspendLayout();
			this.pageFeedItemDisplay.SuspendLayout();
			this.finishPage.SuspendLayout();
			this.pnlCompleting.SuspendLayout();
			this.pageFoundMultipleFeeds.SuspendLayout();
			this.pageNewBySearchTopic.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpSyndic8)).BeginInit();
			this.pageValidateUrl.SuspendLayout();
			this.pageHowToSelection.SuspendLayout();
			this.pageNewByURL.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpAutodiscover)).BeginInit();
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
			this.wizard.SelectedPage = this.pageFeedItemControl;
			this.wizard.Finish += new System.EventHandler(this.OnWizardFinish);
			this.wizard.Cancel += new System.EventHandler(this.OnWizardCancel);
			// 
			// _btnImmediateFinish
			// 
			resources.ApplyResources(this._btnImmediateFinish, "_btnImmediateFinish");
			this._btnImmediateFinish.Name = "_btnImmediateFinish";
			this._btnImmediateFinish.Click += new System.EventHandler(this.OnImmediateFinish_Click);
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
			this.cboFeedSources.SelectedIndexChanged += new System.EventHandler(this.cboFeedSources_SelectedIndexChanged);
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
			this.pageTitleCategory.ResumeLayout(false);
			this.pageTitleCategory.PerformLayout();
			this.pageFeedCredentials.ResumeLayout(false);
			this.pageFeedCredentials.PerformLayout();
			this.pageFeedItemControl.ResumeLayout(false);
			this.pageFeedItemDisplay.ResumeLayout(false);
			this.finishPage.ResumeLayout(false);
			this.pnlCompleting.ResumeLayout(false);
			this.pageFoundMultipleFeeds.ResumeLayout(false);
			this.pageNewBySearchTopic.ResumeLayout(false);
			this.pageNewBySearchTopic.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpSyndic8)).EndInit();
			this.pageValidateUrl.ResumeLayout(false);
			this.pageHowToSelection.ResumeLayout(false);
			this.pageNewByURL.ResumeLayout(false);
			this.pageNewByURL.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureHelpAutodiscover)).EndInit();
			this.pageWelcome.ResumeLayout(false);
			this.pageNewByNNTPGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

	}
}
