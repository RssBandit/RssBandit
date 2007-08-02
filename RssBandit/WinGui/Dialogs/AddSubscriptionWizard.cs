using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using NewsComponents.Utils;
using RssBandit;
using RssBandit.AppServices;
using RssBandit.WinGui.Controls;

using NewsComponents;
using NewsComponents.Feed;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class AddSubscriptionWizard : System.Windows.Forms.Form, IServiceProvider
	{
		/// <summary>
		/// The new subscription - feed, if not null it is ready to be subscribed.
		/// </summary>
		internal feedsFeed Feed= null;

		private IServiceProvider serviceProvider;
		private WindowSerializer windowSerializer;
		private IInternetService internetService;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Divelements.WizardFramework.InformationBox informationBox1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ProgressBar pbar;
		private System.Windows.Forms.Timer timerIncreaseProgress;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Panel pnlCompleting;
		private System.Windows.Forms.Panel pnlCancelling;
		private Divelements.WizardFramework.Wizard wizard;
		private Divelements.WizardFramework.IntroductionPage pageWelcome;
		private Divelements.WizardFramework.WizardPage pageValidateUrl;
		private System.Windows.Forms.Label lblValidationTask1;
		private System.Windows.Forms.Label lblValidationTaskImage1;
		private System.Windows.Forms.Label lblValidationTaskImage2;
		private System.Windows.Forms.Label lblValidationTask2;
		private System.Windows.Forms.Label lblValidationTaskImage3;
		private System.Windows.Forms.Label lblValidationTask3;
		private System.Windows.Forms.Label lblValidationTaskImage4;
		private System.Windows.Forms.Label lblValidationTask4;
		private System.Windows.Forms.CheckBox chkDisplayWelcome;
		private Divelements.WizardFramework.WizardPage pageFoundMultipleFeeds;
		private System.Windows.Forms.RadioButton radioReqInfoWebAddress;
		private System.Windows.Forms.Label lblReqInfoIntro;
		private System.Windows.Forms.RadioButton radioReqInfoKeyWords;
		private System.Windows.Forms.TextBox txtReqInfoAddressKeyWords;
		private System.Windows.Forms.Label lblReqInfoAddressKeyWords;
		private System.Windows.Forms.ComboBox cboReqInfoCategory;
		private System.Windows.Forms.Label label10;
		private Divelements.WizardFramework.WizardPage pageRequiredInfos;
		private Divelements.WizardFramework.FinishPage finishPage;
		private System.Windows.Forms.ListBox lstFoundFeeds;
		private System.Windows.Forms.Label lblFeedTitle;
		private System.Windows.Forms.TextBox txtFeedTitle;
		private System.Windows.Forms.Label lblPageTitleCredentialsIntro;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textBox3;
		private Divelements.WizardFramework.WizardPage pageTitleCredentials;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Button _btnImmediateFinish;
		private Divelements.WizardFramework.WizardPage pageFeedItemControl;
		private System.Windows.Forms.CheckBox chkEnableLaertOnNewItems;
		private System.Windows.Forms.Label lblFeedItemControlIntro;
		private System.Windows.Forms.CheckBox chkMarkItemsReadOnExiting;
		private Divelements.WizardFramework.WizardPage pageFeedItemDisplay;
		private System.Windows.Forms.Label lblFeedItemDisplayIntro;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.CheckBox checkBox1;
		private RssBandit.WinGui.Controls.OptionSectionPanel sectionPanelFeedCredentials;
		private System.Windows.Forms.ComboBox comboMaxItemAge;
		private System.Windows.Forms.Label lblFormatterStylesheet;
		private System.Windows.Forms.ComboBox comboFormatters;
		private System.Windows.Forms.ComboBox cboUpdateFrequency;
		private System.ComponentModel.IContainer components;

		public AddSubscriptionWizard(IServiceProvider provider)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			serviceProvider = provider;

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
			}
			// to checkout the defaults to be used for the new feed:
			IUserPreferences preferencesService = (IUserPreferences)this.GetService(typeof(IUserPreferences));
			this.MaxItemAge = preferencesService.MaxItemAge;

			ICoreApplication coreApp = (ICoreApplication)this.GetService(typeof(ICoreApplication));
			this.cboUpdateFrequency.Text = String.Format("{0}", coreApp.CurrentGlobalRefreshRate); 
			
			//initialize category combo box			
			foreach(string category in coreApp.GetCategories())
			{
				if (!StringHelper.EmptyOrNull(category))
					this.cboReqInfoCategory.Items.Add(category); 
			}

			this.cboReqInfoCategory.Text = coreApp.DefaultCategory; 

			this.wizard.SelectedPage = this.pageWelcome;
		}

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
			this.pageWelcome = new Divelements.WizardFramework.IntroductionPage();
			this.chkDisplayWelcome = new System.Windows.Forms.CheckBox();
			this.informationBox1 = new Divelements.WizardFramework.InformationBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.pageRequiredInfos = new Divelements.WizardFramework.WizardPage();
			this.label10 = new System.Windows.Forms.Label();
			this.cboReqInfoCategory = new System.Windows.Forms.ComboBox();
			this.lblReqInfoAddressKeyWords = new System.Windows.Forms.Label();
			this.txtReqInfoAddressKeyWords = new System.Windows.Forms.TextBox();
			this.radioReqInfoKeyWords = new System.Windows.Forms.RadioButton();
			this.lblReqInfoIntro = new System.Windows.Forms.Label();
			this.radioReqInfoWebAddress = new System.Windows.Forms.RadioButton();
			this.pageValidateUrl = new Divelements.WizardFramework.WizardPage();
			this.lblValidationTaskImage4 = new System.Windows.Forms.Label();
			this.lblValidationTask4 = new System.Windows.Forms.Label();
			this.lblValidationTaskImage3 = new System.Windows.Forms.Label();
			this.lblValidationTask3 = new System.Windows.Forms.Label();
			this.lblValidationTaskImage2 = new System.Windows.Forms.Label();
			this.lblValidationTask2 = new System.Windows.Forms.Label();
			this.lblValidationTaskImage1 = new System.Windows.Forms.Label();
			this.lblValidationTask1 = new System.Windows.Forms.Label();
			this.pbar = new System.Windows.Forms.ProgressBar();
			this.label3 = new System.Windows.Forms.Label();
			this.pageFoundMultipleFeeds = new Divelements.WizardFramework.WizardPage();
			this.lstFoundFeeds = new System.Windows.Forms.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.pageTitleCredentials = new Divelements.WizardFramework.WizardPage();
			this.sectionPanelFeedCredentials = new RssBandit.WinGui.Controls.OptionSectionPanel();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.lblPageTitleCredentialsIntro = new System.Windows.Forms.Label();
			this.lblFeedTitle = new System.Windows.Forms.Label();
			this.txtFeedTitle = new System.Windows.Forms.TextBox();
			this.pageFeedItemControl = new Divelements.WizardFramework.WizardPage();
			this.comboMaxItemAge = new System.Windows.Forms.ComboBox();
			this.cboUpdateFrequency = new System.Windows.Forms.ComboBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.chkMarkItemsReadOnExiting = new System.Windows.Forms.CheckBox();
			this.lblFeedItemControlIntro = new System.Windows.Forms.Label();
			this.chkEnableLaertOnNewItems = new System.Windows.Forms.CheckBox();
			this.pageFeedItemDisplay = new Divelements.WizardFramework.WizardPage();
			this.comboFormatters = new System.Windows.Forms.ComboBox();
			this.lblFormatterStylesheet = new System.Windows.Forms.Label();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.lblFeedItemDisplayIntro = new System.Windows.Forms.Label();
			this.finishPage = new Divelements.WizardFramework.FinishPage();
			this.pnlCompleting = new System.Windows.Forms.Panel();
			this.label9 = new System.Windows.Forms.Label();
			this.pnlCancelling = new System.Windows.Forms.Panel();
			this.timerIncreaseProgress = new System.Windows.Forms.Timer(this.components);
			this.wizard.SuspendLayout();
			this.pageWelcome.SuspendLayout();
			this.pageRequiredInfos.SuspendLayout();
			this.pageValidateUrl.SuspendLayout();
			this.pageFoundMultipleFeeds.SuspendLayout();
			this.pageTitleCredentials.SuspendLayout();
			this.sectionPanelFeedCredentials.SuspendLayout();
			this.pageFeedItemControl.SuspendLayout();
			this.pageFeedItemDisplay.SuspendLayout();
			this.finishPage.SuspendLayout();
			this.pnlCompleting.SuspendLayout();
			this.SuspendLayout();
			// 
			// wizard
			// 
			this.wizard.BannerImage = ((System.Drawing.Image)(resources.GetObject("wizard.BannerImage")));
			this.wizard.Controls.Add(this._btnImmediateFinish);
			this.wizard.Controls.Add(this.finishPage);
			this.wizard.Controls.Add(this.pageTitleCredentials);
			this.wizard.Controls.Add(this.pageFoundMultipleFeeds);
			this.wizard.Controls.Add(this.pageRequiredInfos);
			this.wizard.Controls.Add(this.pageWelcome);
			this.wizard.Controls.Add(this.pageFeedItemDisplay);
			this.wizard.Controls.Add(this.pageValidateUrl);
			this.wizard.Controls.Add(this.pageFeedItemControl);
			this.wizard.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wizard.Location = new System.Drawing.Point(0, 0);
			this.wizard.MarginImage = ((System.Drawing.Image)(resources.GetObject("wizard.MarginImage")));
			this.wizard.Name = "wizard";
			this.wizard.SelectedPage = this.pageRequiredInfos;
			this.wizard.Size = new System.Drawing.Size(695, 437);
			this.wizard.TabIndex = 0;
			this.wizard.Text = "wizard";
			this.wizard.Cancel += new System.EventHandler(this.OnWizardCancel);
			this.wizard.Finish += new System.EventHandler(this.OnWizardFinish);
			// 
			// _btnImmediateFinish
			// 
			this._btnImmediateFinish.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this._btnImmediateFinish.Location = new System.Drawing.Point(171, 398);
			this._btnImmediateFinish.Name = "_btnImmediateFinish";
			this._btnImmediateFinish.Size = new System.Drawing.Size(112, 27);
			this._btnImmediateFinish.TabIndex = 1008;
			this._btnImmediateFinish.Text = "Finish";
			this._btnImmediateFinish.Visible = false;
			this._btnImmediateFinish.Click += new System.EventHandler(this.OnImmediateFinish_Click);
			// 
			// pageWelcome
			// 
			this.pageWelcome.Controls.Add(this.chkDisplayWelcome);
			this.pageWelcome.Controls.Add(this.informationBox1);
			this.pageWelcome.Controls.Add(this.label1);
			this.pageWelcome.Controls.Add(this.label2);
			this.pageWelcome.IntroductionText = "This wizard helps you:";
			this.pageWelcome.Location = new System.Drawing.Point(179, 80);
			this.pageWelcome.Name = "pageWelcome";
			this.pageWelcome.NextPage = this.pageRequiredInfos;
			this.pageWelcome.Size = new System.Drawing.Size(501, 288);
			this.pageWelcome.TabIndex = 1003;
			this.pageWelcome.Text = "Welcome to the Add Subscription Wizard!";
			this.pageWelcome.BeforeDisplay += new System.EventHandler(this.OnPageWelcome_BeforeDisplay);
			// 
			// chkDisplayWelcome
			// 
			this.chkDisplayWelcome.Checked = true;
			this.chkDisplayWelcome.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkDisplayWelcome.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkDisplayWelcome.Location = new System.Drawing.Point(45, 223);
			this.chkDisplayWelcome.Name = "chkDisplayWelcome";
			this.chkDisplayWelcome.Size = new System.Drawing.Size(383, 30);
			this.chkDisplayWelcome.TabIndex = 2;
			this.chkDisplayWelcome.Text = "Always display this Wizard Welcome Page";
			// 
			// informationBox1
			// 
			this.informationBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.informationBox1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.informationBox1.Icon = Divelements.WizardFramework.SystemIconType.Warning;
			this.informationBox1.Location = new System.Drawing.Point(45, 155);
			this.informationBox1.Name = "informationBox1";
			this.informationBox1.Size = new System.Drawing.Size(455, 68);
			this.informationBox1.TabIndex = 1;
			this.informationBox1.Text = "The wizard requires at some steps you are connected to the Internet! If not, ensu" +
				"re to connect your computer now.";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(45, 44);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(455, 39);
			this.label1.TabIndex = 0;
			this.label1.Text = "• Examining an HTML page for available feeds or find feeds by specific key words." +
				"";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(45, 92);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(455, 54);
			this.label2.TabIndex = 0;
			this.label2.Text = "• Validating the subscription source and manage additional properties maybe requi" +
				"red for a successful request (e.g. credentials).";
			// 
			// pageRequiredInfos
			// 
			this.pageRequiredInfos.Controls.Add(this.label10);
			this.pageRequiredInfos.Controls.Add(this.cboReqInfoCategory);
			this.pageRequiredInfos.Controls.Add(this.lblReqInfoAddressKeyWords);
			this.pageRequiredInfos.Controls.Add(this.txtReqInfoAddressKeyWords);
			this.pageRequiredInfos.Controls.Add(this.radioReqInfoKeyWords);
			this.pageRequiredInfos.Controls.Add(this.lblReqInfoIntro);
			this.pageRequiredInfos.Controls.Add(this.radioReqInfoWebAddress);
			this.pageRequiredInfos.Description = "Please provide the information(s) to add a new subscription";
			this.pageRequiredInfos.Location = new System.Drawing.Point(22, 84);
			this.pageRequiredInfos.Name = "pageRequiredInfos";
			this.pageRequiredInfos.NextPage = this.pageValidateUrl;
			this.pageRequiredInfos.PreviousPage = this.pageWelcome;
			this.pageRequiredInfos.Size = new System.Drawing.Size(651, 284);
			this.pageRequiredInfos.TabIndex = 1009;
			this.pageRequiredInfos.Text = "Required Informations";
			this.pageRequiredInfos.BeforeMoveBack += new Divelements.WizardFramework.WizardPageEventHandler(this.OnPageRequiredInfos_MoveBack);
			this.pageRequiredInfos.BeforeDisplay += new System.EventHandler(this.OnPageRequiredInfos_BeforeDisplay);
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label10.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label10.Location = new System.Drawing.Point(146, 204);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(467, 19);
			this.label10.TabIndex = 6;
			this.label10.Text = "Category:";
			// 
			// cboReqInfoCategory
			// 
			this.cboReqInfoCategory.Location = new System.Drawing.Point(146, 223);
			this.cboReqInfoCategory.Name = "cboReqInfoCategory";
			this.cboReqInfoCategory.Size = new System.Drawing.Size(324, 25);
			this.cboReqInfoCategory.TabIndex = 5;
			// 
			// lblReqInfoAddressKeyWords
			// 
			this.lblReqInfoAddressKeyWords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblReqInfoAddressKeyWords.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblReqInfoAddressKeyWords.Location = new System.Drawing.Point(146, 146);
			this.lblReqInfoAddressKeyWords.Name = "lblReqInfoAddressKeyWords";
			this.lblReqInfoAddressKeyWords.Size = new System.Drawing.Size(467, 19);
			this.lblReqInfoAddressKeyWords.TabIndex = 4;
			this.lblReqInfoAddressKeyWords.Text = "[DYNAMIC CAPTION FOR SELECTED OPTION]:";
			// 
			// txtReqInfoAddressKeyWords
			// 
			this.txtReqInfoAddressKeyWords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtReqInfoAddressKeyWords.Location = new System.Drawing.Point(146, 165);
			this.txtReqInfoAddressKeyWords.Name = "txtReqInfoAddressKeyWords";
			this.txtReqInfoAddressKeyWords.Size = new System.Drawing.Size(467, 24);
			this.txtReqInfoAddressKeyWords.TabIndex = 3;
			this.txtReqInfoAddressKeyWords.Text = "";
			// 
			// radioReqInfoKeyWords
			// 
			this.radioReqInfoKeyWords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.radioReqInfoKeyWords.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioReqInfoKeyWords.Location = new System.Drawing.Point(146, 107);
			this.radioReqInfoKeyWords.Name = "radioReqInfoKeyWords";
			this.radioReqInfoKeyWords.Size = new System.Drawing.Size(445, 29);
			this.radioReqInfoKeyWords.TabIndex = 2;
			this.radioReqInfoKeyWords.Text = "Key word(s)";
			this.radioReqInfoKeyWords.CheckedChanged += new System.EventHandler(this.OnOptionKeyWordsCheckedChanged);
			// 
			// lblReqInfoIntro
			// 
			this.lblReqInfoIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblReqInfoIntro.Location = new System.Drawing.Point(38, 5);
			this.lblReqInfoIntro.Name = "lblReqInfoIntro";
			this.lblReqInfoIntro.Size = new System.Drawing.Size(574, 73);
			this.lblReqInfoIntro.TabIndex = 1;
			this.lblReqInfoIntro.Text = "If you want to specify a web address (Url), please check the option \'Web Address\'" +
				". Otherwise check \'Key word(s)\' to provide some key words to lookup matching fee" +
				"ds:";
			// 
			// radioReqInfoWebAddress
			// 
			this.radioReqInfoWebAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.radioReqInfoWebAddress.Checked = true;
			this.radioReqInfoWebAddress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioReqInfoWebAddress.Location = new System.Drawing.Point(146, 78);
			this.radioReqInfoWebAddress.Name = "radioReqInfoWebAddress";
			this.radioReqInfoWebAddress.Size = new System.Drawing.Size(445, 29);
			this.radioReqInfoWebAddress.TabIndex = 0;
			this.radioReqInfoWebAddress.TabStop = true;
			this.radioReqInfoWebAddress.Text = "Web Address (Url)";
			this.radioReqInfoWebAddress.CheckedChanged += new System.EventHandler(this.OnOptionWebAddressCheckedChanged);
			// 
			// pageValidateUrl
			// 
			this.pageValidateUrl.AllowCancel = false;
			this.pageValidateUrl.AllowMoveNext = false;
			this.pageValidateUrl.AllowMovePrevious = false;
			this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage4);
			this.pageValidateUrl.Controls.Add(this.lblValidationTask4);
			this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage3);
			this.pageValidateUrl.Controls.Add(this.lblValidationTask3);
			this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage2);
			this.pageValidateUrl.Controls.Add(this.lblValidationTask2);
			this.pageValidateUrl.Controls.Add(this.lblValidationTaskImage1);
			this.pageValidateUrl.Controls.Add(this.lblValidationTask1);
			this.pageValidateUrl.Controls.Add(this.pbar);
			this.pageValidateUrl.Controls.Add(this.label3);
			this.pageValidateUrl.Description = "";
			this.pageValidateUrl.Location = new System.Drawing.Point(22, 84);
			this.pageValidateUrl.Name = "pageValidateUrl";
			this.pageValidateUrl.NextPage = this.pageFoundMultipleFeeds;
			this.pageValidateUrl.PreviousPage = this.pageRequiredInfos;
			this.pageValidateUrl.Size = new System.Drawing.Size(651, 284);
			this.pageValidateUrl.TabIndex = 1004;
			this.pageValidateUrl.Text = "Please wait...";
			this.pageValidateUrl.BeforeDisplay += new System.EventHandler(this.OnPageSearching_BeforeDisplay);
			this.pageValidateUrl.AfterDisplay += new System.EventHandler(this.OnPageSearching_AfterDisplay);
			// 
			// lblValidationTaskImage4
			// 
			this.lblValidationTaskImage4.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTaskImage4.Image")));
			this.lblValidationTaskImage4.Location = new System.Drawing.Point(134, 233);
			this.lblValidationTaskImage4.Name = "lblValidationTaskImage4";
			this.lblValidationTaskImage4.Size = new System.Drawing.Size(45, 20);
			this.lblValidationTaskImage4.TabIndex = 9;
			// 
			// lblValidationTask4
			// 
			this.lblValidationTask4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblValidationTask4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblValidationTask4.Location = new System.Drawing.Point(179, 233);
			this.lblValidationTask4.Name = "lblValidationTask4";
			this.lblValidationTask4.Size = new System.Drawing.Size(434, 20);
			this.lblValidationTask4.TabIndex = 8;
			this.lblValidationTask4.Text = "...";
			// 
			// lblValidationTaskImage3
			// 
			this.lblValidationTaskImage3.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTaskImage3.Image")));
			this.lblValidationTaskImage3.Location = new System.Drawing.Point(134, 204);
			this.lblValidationTaskImage3.Name = "lblValidationTaskImage3";
			this.lblValidationTaskImage3.Size = new System.Drawing.Size(45, 19);
			this.lblValidationTaskImage3.TabIndex = 7;
			// 
			// lblValidationTask3
			// 
			this.lblValidationTask3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblValidationTask3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblValidationTask3.Location = new System.Drawing.Point(179, 204);
			this.lblValidationTask3.Name = "lblValidationTask3";
			this.lblValidationTask3.Size = new System.Drawing.Size(434, 19);
			this.lblValidationTask3.TabIndex = 6;
			this.lblValidationTask3.Text = "...";
			// 
			// lblValidationTaskImage2
			// 
			this.lblValidationTaskImage2.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTaskImage2.Image")));
			this.lblValidationTaskImage2.Location = new System.Drawing.Point(134, 175);
			this.lblValidationTaskImage2.Name = "lblValidationTaskImage2";
			this.lblValidationTaskImage2.Size = new System.Drawing.Size(45, 19);
			this.lblValidationTaskImage2.TabIndex = 5;
			// 
			// lblValidationTask2
			// 
			this.lblValidationTask2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblValidationTask2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblValidationTask2.Location = new System.Drawing.Point(179, 175);
			this.lblValidationTask2.Name = "lblValidationTask2";
			this.lblValidationTask2.Size = new System.Drawing.Size(434, 19);
			this.lblValidationTask2.TabIndex = 4;
			this.lblValidationTask2.Text = "Examine Feed Title";
			// 
			// lblValidationTaskImage1
			// 
			this.lblValidationTaskImage1.Image = ((System.Drawing.Image)(resources.GetObject("lblValidationTaskImage1.Image")));
			this.lblValidationTaskImage1.Location = new System.Drawing.Point(134, 146);
			this.lblValidationTaskImage1.Name = "lblValidationTaskImage1";
			this.lblValidationTaskImage1.Size = new System.Drawing.Size(45, 19);
			this.lblValidationTaskImage1.TabIndex = 3;
			// 
			// lblValidationTask1
			// 
			this.lblValidationTask1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblValidationTask1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblValidationTask1.Location = new System.Drawing.Point(179, 146);
			this.lblValidationTask1.Name = "lblValidationTask1";
			this.lblValidationTask1.Size = new System.Drawing.Size(434, 19);
			this.lblValidationTask1.TabIndex = 2;
			this.lblValidationTask1.Text = "Validate web address";
			// 
			// pbar
			// 
			this.pbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pbar.Location = new System.Drawing.Point(106, 102);
			this.pbar.Name = "pbar";
			this.pbar.Size = new System.Drawing.Size(440, 19);
			this.pbar.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label3.Location = new System.Drawing.Point(39, 5);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(574, 39);
			this.label3.TabIndex = 0;
			this.label3.Text = "This wizard validates the web resource you provided, or search for feeds by key w" +
				"ords:";
			// 
			// pageFoundMultipleFeeds
			// 
			this.pageFoundMultipleFeeds.AllowMoveNext = false;
			this.pageFoundMultipleFeeds.Controls.Add(this.lstFoundFeeds);
			this.pageFoundMultipleFeeds.Controls.Add(this.label5);
			this.pageFoundMultipleFeeds.Controls.Add(this.label6);
			this.pageFoundMultipleFeeds.Controls.Add(this.label7);
			this.pageFoundMultipleFeeds.Description = "Please select the desired feed for subscription";
			this.pageFoundMultipleFeeds.Location = new System.Drawing.Point(22, 84);
			this.pageFoundMultipleFeeds.Name = "pageFoundMultipleFeeds";
			this.pageFoundMultipleFeeds.NextPage = this.pageTitleCredentials;
			this.pageFoundMultipleFeeds.PreviousPage = this.pageRequiredInfos;
			this.pageFoundMultipleFeeds.Size = new System.Drawing.Size(651, 284);
			this.pageFoundMultipleFeeds.TabIndex = 1007;
			this.pageFoundMultipleFeeds.Text = "Multiple feeds found";
			this.pageFoundMultipleFeeds.BeforeDisplay += new System.EventHandler(this.OnPageFoundMultipleFeeds_BeforeDisplay);
			// 
			// lstFoundFeeds
			// 
			this.lstFoundFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstFoundFeeds.IntegralHeight = false;
			this.lstFoundFeeds.ItemHeight = 17;
			this.lstFoundFeeds.Location = new System.Drawing.Point(39, 141);
			this.lstFoundFeeds.Name = "lstFoundFeeds";
			this.lstFoundFeeds.Size = new System.Drawing.Size(574, 140);
			this.lstFoundFeeds.Sorted = true;
			this.lstFoundFeeds.TabIndex = 1;
			this.lstFoundFeeds.DoubleClick += new System.EventHandler(this.OnListFoundFeeds_DoubleClick);
			this.lstFoundFeeds.SelectedIndexChanged += new System.EventHandler(this.OnFoundFeedsListSelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label5.Location = new System.Drawing.Point(39, 5);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(569, 39);
			this.label5.TabIndex = 0;
			this.label5.Text = "From the list below, select a feed, then click Next to add credentials or check a" +
				"dditional properties.";
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label6.Location = new System.Drawing.Point(39, 53);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(569, 44);
			this.label6.TabIndex = 0;
			this.label6.Text = "If required feed cannot be found at the list, please click Back to initiate a new" +
				" search.";
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label7.Location = new System.Drawing.Point(39, 121);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(569, 25);
			this.label7.TabIndex = 0;
			this.label7.Text = "Feeds found:";
			// 
			// pageTitleCredentials
			// 
			this.pageTitleCredentials.Controls.Add(this.sectionPanelFeedCredentials);
			this.pageTitleCredentials.Controls.Add(this.lblPageTitleCredentialsIntro);
			this.pageTitleCredentials.Controls.Add(this.lblFeedTitle);
			this.pageTitleCredentials.Controls.Add(this.txtFeedTitle);
			this.pageTitleCredentials.Description = "Please verify the title and add credentials, if required";
			this.pageTitleCredentials.Location = new System.Drawing.Point(22, 84);
			this.pageTitleCredentials.Name = "pageTitleCredentials";
			this.pageTitleCredentials.NextPage = this.pageFeedItemControl;
			this.pageTitleCredentials.PreviousPage = this.pageFoundMultipleFeeds;
			this.pageTitleCredentials.Size = new System.Drawing.Size(651, 284);
			this.pageTitleCredentials.TabIndex = 1010;
			this.pageTitleCredentials.Text = "Title and Credentials";
			// 
			// sectionPanelFeedCredentials
			// 
			this.sectionPanelFeedCredentials.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.sectionPanelFeedCredentials.Controls.Add(this.textBox2);
			this.sectionPanelFeedCredentials.Controls.Add(this.label8);
			this.sectionPanelFeedCredentials.Controls.Add(this.textBox3);
			this.sectionPanelFeedCredentials.Controls.Add(this.label4);
			this.sectionPanelFeedCredentials.Image = ((System.Drawing.Image)(resources.GetObject("sectionPanelFeedCredentials.Image")));
			this.sectionPanelFeedCredentials.ImageLocation = new System.Drawing.Point(0, 20);
			this.sectionPanelFeedCredentials.Location = new System.Drawing.Point(91, 138);
			this.sectionPanelFeedCredentials.Name = "sectionPanelFeedCredentials";
			this.sectionPanelFeedCredentials.Size = new System.Drawing.Size(499, 135);
			this.sectionPanelFeedCredentials.TabIndex = 12;
			this.sectionPanelFeedCredentials.Text = "Credentials";
			// 
			// textBox2
			// 
			this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox2.Location = new System.Drawing.Point(56, 44);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(356, 24);
			this.textBox2.TabIndex = 8;
			this.textBox2.Text = "";
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label8.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label8.Location = new System.Drawing.Point(56, 80);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(356, 20);
			this.label8.TabIndex = 11;
			this.label8.Text = "&Password:";
			// 
			// textBox3
			// 
			this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox3.Location = new System.Drawing.Point(56, 102);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(356, 24);
			this.textBox3.TabIndex = 10;
			this.textBox3.Text = "";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Location = new System.Drawing.Point(56, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(356, 20);
			this.label4.TabIndex = 9;
			this.label4.Text = "&Username:";
			// 
			// lblPageTitleCredentialsIntro
			// 
			this.lblPageTitleCredentialsIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblPageTitleCredentialsIntro.Location = new System.Drawing.Point(38, 5);
			this.lblPageTitleCredentialsIntro.Name = "lblPageTitleCredentialsIntro";
			this.lblPageTitleCredentialsIntro.Size = new System.Drawing.Size(574, 63);
			this.lblPageTitleCredentialsIntro.TabIndex = 7;
			this.lblPageTitleCredentialsIntro.Text = "Here you can change the title of the feed and add (optional) credentials, if the " +
				"feed is a protected web resource.";
			// 
			// lblFeedTitle
			// 
			this.lblFeedTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblFeedTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFeedTitle.Location = new System.Drawing.Point(91, 78);
			this.lblFeedTitle.Name = "lblFeedTitle";
			this.lblFeedTitle.Size = new System.Drawing.Size(468, 19);
			this.lblFeedTitle.TabIndex = 6;
			this.lblFeedTitle.Text = "&Title:";
			// 
			// txtFeedTitle
			// 
			this.txtFeedTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFeedTitle.Location = new System.Drawing.Point(91, 97);
			this.txtFeedTitle.Name = "txtFeedTitle";
			this.txtFeedTitle.Size = new System.Drawing.Size(468, 24);
			this.txtFeedTitle.TabIndex = 5;
			this.txtFeedTitle.Text = "";
			// 
			// pageFeedItemControl
			// 
			this.pageFeedItemControl.Controls.Add(this.comboMaxItemAge);
			this.pageFeedItemControl.Controls.Add(this.cboUpdateFrequency);
			this.pageFeedItemControl.Controls.Add(this.label13);
			this.pageFeedItemControl.Controls.Add(this.label12);
			this.pageFeedItemControl.Controls.Add(this.label11);
			this.pageFeedItemControl.Controls.Add(this.chkMarkItemsReadOnExiting);
			this.pageFeedItemControl.Controls.Add(this.lblFeedItemControlIntro);
			this.pageFeedItemControl.Controls.Add(this.chkEnableLaertOnNewItems);
			this.pageFeedItemControl.Description = "Optional Subscription Properties related to the feed and it\'s items";
			this.pageFeedItemControl.Location = new System.Drawing.Point(22, 84);
			this.pageFeedItemControl.Name = "pageFeedItemControl";
			this.pageFeedItemControl.NextPage = this.pageFeedItemDisplay;
			this.pageFeedItemControl.PreviousPage = this.pageTitleCredentials;
			this.pageFeedItemControl.Size = new System.Drawing.Size(651, 284);
			this.pageFeedItemControl.TabIndex = 1011;
			this.pageFeedItemControl.Text = "Feed/Item Control";
			// 
			// comboMaxItemAge
			// 
			this.comboMaxItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboMaxItemAge.ItemHeight = 17;
			this.comboMaxItemAge.Items.AddRange(new object[] {
																 "1 day",
																 "2 days",
																 "3 days",
																 "4 days",
																 "5 days",
																 "6 days",
																 "7 days",
																 "14 days",
																 "21 days",
																 "1 month",
																 "2 months",
																 "1 quarter",
																 "2 quarters",
																 "3 quarters",
																 "1 year",
																 "Unlimited"});
			this.comboMaxItemAge.Location = new System.Drawing.Point(294, 215);
			this.comboMaxItemAge.Name = "comboMaxItemAge";
			this.comboMaxItemAge.Size = new System.Drawing.Size(182, 25);
			this.comboMaxItemAge.TabIndex = 16;
			// 
			// cboUpdateFrequency
			// 
			this.cboUpdateFrequency.ItemHeight = 17;
			this.cboUpdateFrequency.Items.AddRange(new object[] {
																	"0",
																	"15",
																	"30",
																	"45",
																	"60",
																	"75",
																	"90",
																	"105",
																	"120"});
			this.cboUpdateFrequency.Location = new System.Drawing.Point(294, 182);
			this.cboUpdateFrequency.Name = "cboUpdateFrequency";
			this.cboUpdateFrequency.Size = new System.Drawing.Size(182, 25);
			this.cboUpdateFrequency.TabIndex = 14;
			// 
			// label13
			// 
			this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label13.Location = new System.Drawing.Point(56, 219);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(244, 19);
			this.label13.TabIndex = 12;
			this.label13.Text = "Remove items older than:";
			this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label12
			// 
			this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label12.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label12.Location = new System.Drawing.Point(483, 185);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(130, 19);
			this.label12.TabIndex = 11;
			this.label12.Text = "minutes";
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label11.Location = new System.Drawing.Point(56, 185);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(244, 19);
			this.label11.TabIndex = 10;
			this.label11.Text = "Update frequency:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// chkMarkItemsReadOnExiting
			// 
			this.chkMarkItemsReadOnExiting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.chkMarkItemsReadOnExiting.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkMarkItemsReadOnExiting.Location = new System.Drawing.Point(146, 126);
			this.chkMarkItemsReadOnExiting.Name = "chkMarkItemsReadOnExiting";
			this.chkMarkItemsReadOnExiting.Size = new System.Drawing.Size(460, 29);
			this.chkMarkItemsReadOnExiting.TabIndex = 9;
			this.chkMarkItemsReadOnExiting.Text = "&Mark items as read on exiting feed";
			// 
			// lblFeedItemControlIntro
			// 
			this.lblFeedItemControlIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblFeedItemControlIntro.Location = new System.Drawing.Point(38, 5);
			this.lblFeedItemControlIntro.Name = "lblFeedItemControlIntro";
			this.lblFeedItemControlIntro.Size = new System.Drawing.Size(574, 63);
			this.lblFeedItemControlIntro.TabIndex = 8;
			this.lblFeedItemControlIntro.Text = "Here you can change the application behavior for the \"New Items received\" event, " +
				"the maximum item age to control the time items get automatically purged and the " +
				"update request frequency.";
			// 
			// chkEnableLaertOnNewItems
			// 
			this.chkEnableLaertOnNewItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.chkEnableLaertOnNewItems.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkEnableLaertOnNewItems.Location = new System.Drawing.Point(146, 97);
			this.chkEnableLaertOnNewItems.Name = "chkEnableLaertOnNewItems";
			this.chkEnableLaertOnNewItems.Size = new System.Drawing.Size(460, 29);
			this.chkEnableLaertOnNewItems.TabIndex = 1;
			this.chkEnableLaertOnNewItems.Text = "&Enable alert windows on received items";
			// 
			// pageFeedItemDisplay
			// 
			this.pageFeedItemDisplay.Controls.Add(this.comboFormatters);
			this.pageFeedItemDisplay.Controls.Add(this.lblFormatterStylesheet);
			this.pageFeedItemDisplay.Controls.Add(this.checkBox1);
			this.pageFeedItemDisplay.Controls.Add(this.lblFeedItemDisplayIntro);
			this.pageFeedItemDisplay.Description = "Optional Properties to control the display of the feed and it\'s items";
			this.pageFeedItemDisplay.Location = new System.Drawing.Point(22, 84);
			this.pageFeedItemDisplay.Name = "pageFeedItemDisplay";
			this.pageFeedItemDisplay.NextPage = this.finishPage;
			this.pageFeedItemDisplay.PreviousPage = this.pageFeedItemControl;
			this.pageFeedItemDisplay.Size = new System.Drawing.Size(651, 284);
			this.pageFeedItemDisplay.TabIndex = 1012;
			this.pageFeedItemDisplay.Text = "Feed/Item Display";
			// 
			// comboFormatters
			// 
			this.comboFormatters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboFormatters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFormatters.ItemHeight = 17;
			this.comboFormatters.Location = new System.Drawing.Point(146, 160);
			this.comboFormatters.Name = "comboFormatters";
			this.comboFormatters.Size = new System.Drawing.Size(362, 25);
			this.comboFormatters.Sorted = true;
			this.comboFormatters.TabIndex = 15;
			// 
			// lblFormatterStylesheet
			// 
			this.lblFormatterStylesheet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblFormatterStylesheet.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFormatterStylesheet.Location = new System.Drawing.Point(146, 138);
			this.lblFormatterStylesheet.Name = "lblFormatterStylesheet";
			this.lblFormatterStylesheet.Size = new System.Drawing.Size(467, 20);
			this.lblFormatterStylesheet.TabIndex = 12;
			this.lblFormatterStylesheet.Text = "Formatter &Stylesheet:";
			// 
			// checkBox1
			// 
			this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBox1.Location = new System.Drawing.Point(146, 97);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(460, 29);
			this.checkBox1.TabIndex = 10;
			this.checkBox1.Text = "Use a &custom formatter";
			// 
			// lblFeedItemDisplayIntro
			// 
			this.lblFeedItemDisplayIntro.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblFeedItemDisplayIntro.Location = new System.Drawing.Point(38, 5);
			this.lblFeedItemDisplayIntro.Name = "lblFeedItemDisplayIntro";
			this.lblFeedItemDisplayIntro.Size = new System.Drawing.Size(574, 63);
			this.lblFeedItemDisplayIntro.TabIndex = 9;
			this.lblFeedItemDisplayIntro.Text = "Use the default feed/item formatting or select a custom XSLT formatter.";
			// 
			// finishPage
			// 
			this.finishPage.Controls.Add(this.pnlCompleting);
			this.finishPage.Controls.Add(this.pnlCancelling);
			this.finishPage.FinishText = "You have successfully completed the Sample Wizard.";
			this.finishPage.Location = new System.Drawing.Point(179, 80);
			this.finishPage.Name = "finishPage";
			this.finishPage.PreviousPage = this.pageFeedItemDisplay;
			this.finishPage.SettingsHeader = "";
			this.finishPage.Size = new System.Drawing.Size(501, 288);
			this.finishPage.TabIndex = 1005;
			this.finishPage.Text = "Complete!";
			this.finishPage.BeforeMoveBack += new Divelements.WizardFramework.WizardPageEventHandler(this.OnFinishPage_BeforeMoveBack);
			this.finishPage.BeforeDisplay += new System.EventHandler(this.OnFinishPage_BeforeDisplay);
			// 
			// pnlCompleting
			// 
			this.pnlCompleting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlCompleting.Controls.Add(this.label9);
			this.pnlCompleting.Location = new System.Drawing.Point(0, 39);
			this.pnlCompleting.Name = "pnlCompleting";
			this.pnlCompleting.Size = new System.Drawing.Size(497, 189);
			this.pnlCompleting.TabIndex = 0;
			this.pnlCompleting.Visible = false;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(0, 10);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(437, 58);
			this.label9.TabIndex = 1;
			this.label9.Text = "The subscription is ready to be added to your subscriptions list.";
			// 
			// pnlCancelling
			// 
			this.pnlCancelling.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlCancelling.Location = new System.Drawing.Point(1, 39);
			this.pnlCancelling.Name = "pnlCancelling";
			this.pnlCancelling.Size = new System.Drawing.Size(497, 150);
			this.pnlCancelling.TabIndex = 1;
			// 
			// timerIncreaseProgress
			// 
			this.timerIncreaseProgress.Interval = 30;
			this.timerIncreaseProgress.Tick += new System.EventHandler(this.timerIncreaseProgress_Tick);
			// 
			// AddSubscriptionWizard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 17);
			this.ClientSize = new System.Drawing.Size(695, 437);
			this.Controls.Add(this.wizard);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddSubscriptionWizard";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Add Subscription Wizard";
			this.wizard.ResumeLayout(false);
			this.pageWelcome.ResumeLayout(false);
			this.pageRequiredInfos.ResumeLayout(false);
			this.pageValidateUrl.ResumeLayout(false);
			this.pageFoundMultipleFeeds.ResumeLayout(false);
			this.pageTitleCredentials.ResumeLayout(false);
			this.sectionPanelFeedCredentials.ResumeLayout(false);
			this.pageFeedItemControl.ResumeLayout(false);
			this.pageFeedItemDisplay.ResumeLayout(false);
			this.finishPage.ResumeLayout(false);
			this.pnlCompleting.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new AddSubscriptionWizard(null));
		}

		public string FeedUrl
		{
			get { return this.txtReqInfoAddressKeyWords.Text; } 
			set { this.txtReqInfoAddressKeyWords.Text = value; }
		} 
		public string FeedCategory
		{
			get { return this.cboReqInfoCategory.Text; } 
			set
			{
				if (!StringHelper.EmptyOrNull(value))
					this.cboReqInfoCategory.Text = value;
			}
		} 
		public string FeedTitle { get { return this.txtFeedTitle.Text; } set { this.txtFeedTitle.Text = value; }} 
		public TimeSpan MaxItemAge 
		{
			get { return Utils.MaxItemAgeFromIndex(this.comboMaxItemAge.SelectedIndex); }
			set { this.comboMaxItemAge.SelectedIndex = Utils.MaxItemAgeToIndex(value);	}
		}

		private void OnPageSearching_BeforeDisplay(object sender, System.EventArgs e)
		{
			pbar.Value = 0;
		}

		private void timerIncreaseProgress_Tick(object sender, System.EventArgs e)
		{
			pbar.Value += 1;
			if (pbar.Value == pbar.Maximum)
			{
				timerIncreaseProgress.Stop();
				wizard.GoNext();
			}
		}

		private void OnPageSearching_AfterDisplay(object sender, System.EventArgs e)
		{
			timerIncreaseProgress.Enabled = true;
		}

		private void OnWizardCancel(object sender, System.EventArgs e)
		{
			Close();
		}

		private void OnWizardFinish(object sender, System.EventArgs e)
		{
			Close();
		}

		private void rdoAlreadyAdded_CheckedChanged(object sender, System.EventArgs e)
		{
			//pageIsHardwareConnected.AllowMoveNext = rdoNotYetAdded.Checked || rdoAlreadyAdded.Checked;
		}

		// not used
		private void pageIsHardwareConnected_BeforeMoveNext(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (true)
			{
				// The hardware is already added, so move on to the installed hardware list
				//pageIsHardwareConnected.NextPage = pageFoundMultipleFeeds;
				//finishPage.PreviousPage = pageFoundMultipleFeeds;
				finishPage.Text = "Completing the Add Hardware Wizard";
				finishPage.FinishText = "Here is the current status of the hardware you selected:";
				finishPage.ProceedText = "To exit this wizard, click Cancel.";
			}
			else
			{
				// Finish the wizard indicating it cannot continue
				//pageIsHardwareConnected.NextPage = finishPage1;
				//finishPage.PreviousPage = pageIsHardwareConnected;
				finishPage.Text = "Cannot Continue the Add Hardware Wizard";
				finishPage.FinishText = "To continue, connect this hardware to your computer.";
				finishPage.ProceedText = "To close this wizard, click Finish.";
			}

			pnlCompleting.Visible = true;//rdoAlreadyAdded.Checked;
			pnlCancelling.Visible = false; //rdoNotYetAdded.Checked;
		}

		private void OnFoundFeedsListSelectedIndexChanged(object sender, System.EventArgs e)
		{
			pageFoundMultipleFeeds.AllowMoveNext = lstFoundFeeds.SelectedIndex >= 0;
		}

		private void OnPageFoundMultipleFeeds_BeforeDisplay(object sender, System.EventArgs e)
		{
			lstFoundFeeds.Items.Clear();
			lstFoundFeeds.Items.AddRange(new string[] { "geeks blog (http://www.geeks.com/blog/rss.asp)", "news blog (http://sample.feed.com/feed.xml)" });
		}

		private void OnListFoundFeeds_DoubleClick(object sender, System.EventArgs e)
		{
			if (pageFoundMultipleFeeds.AllowMoveNext)
				wizard.GoNext();
		}

		private void OnPageWelcome_BeforeDisplay(object sender, System.EventArgs e) {
			
			this.radioReqInfoWebAddress.Checked = true;
			OnOptionWebAddressCheckedChanged(this, EventArgs.Empty);

			// get the check value: display this page?
			if (false == this.chkDisplayWelcome.Checked) {
				// move to next wizard page 
				wizard.GoNext();
			}
		}

		private void OnOptionWebAddressCheckedChanged(object sender, System.EventArgs e) {
			this.lblReqInfoAddressKeyWords.Text = this.radioReqInfoWebAddress.Text + ":";
		}

		private void OnOptionKeyWordsCheckedChanged(object sender, System.EventArgs e) {
			this.lblReqInfoAddressKeyWords.Text = this.radioReqInfoKeyWords.Text + ":";
		}

		private void OnWindowSerializerLoadStateEvent(object sender, Genghis.Preferences preferences) {
			this.chkDisplayWelcome.Checked = preferences.GetBoolean(this.Name + ".DisplayWelcome", true);
			this.FeedCategory = preferences.GetString(this.Name + ".LastFeedCategory", this.FeedCategory);

			// BUGBUG: event does not get fired:
			OnPageWelcome_BeforeDisplay(this, EventArgs.Empty);
		}

		private void OnWindowSerializerSaveStateEvent(object sender, Genghis.Preferences preferences) {
			preferences.SetProperty(this.Name + ".DisplayWelcome", this.chkDisplayWelcome.Checked);
			if (!StringHelper.EmptyOrNull(this.FeedCategory) && 
				!this.FeedCategory.StartsWith("[") && 
				!this.FeedCategory.EndsWith("]"))
			{
				preferences.SetProperty(this.Name + ".LastFeedCategory", this.FeedCategory);
			}
		}

		private void OnInternetServiceInternetConnectionStateChange(object sender, InternetConnectionStateChangeEventArgs e) {
			//TODO: if no connection allowed, ignore the url validation and title request...
		}

		private void OnPageRequiredInfos_BeforeDisplay(object sender, System.EventArgs e)
		{
			this._btnImmediateFinish.Visible = true;
			this._btnImmediateFinish.Enabled = pageRequiredInfos.AllowMoveNext;
		}

		private void OnPageRequiredInfos_MoveBack(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this._btnImmediateFinish.Visible = false;
		}

		private void OnImmediateFinish_Click(object sender, System.EventArgs e)
		{
			//TODO: do all the stuff in background, no user questions anymore
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

	}
}
