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
    /// SynchronizeFeedsWizard handles adding new FeedSources including Google Reader, Windows RSS platform and NewsGator Online.
    /// </summary>
    internal class SynchronizeFeedsWizard : Form, IServiceProvider, IWaitDialog
    {
        private enum WizardValidationTask
        {
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
        private readonly IServiceProvider serviceProvider;
        private readonly WindowSerializer windowSerializer;
        private IInternetService internetService;
        private readonly ICoreApplication coreApplication;

        private TimeSpan timeout = TimeSpan.Zero;	// no timeout
        private bool operationTimeout;
        private DialogResult operationResult;
        private int timeCounter;
        private AutoResetEvent waitHandle;
        private FeedInfo feedInfo;
        private bool credentialsStepReWired;

        #region Designer Form variables

        private System.Windows.Forms.ToolTip toolTip;
        private CheckBox chkDisplayWelcome;
        private WizardPage pageFeedCredentials;
        private Label lblFeedCredentialsIntro;
        private Label lblUsername;
        private TextBox textUser;
        private Label lblPassword;
        private TextBox textPassword;
        private WizardPage pageStartImport;
        private RadioButton radioGoogleReader;
        private RadioButton radioCommonFeedlist;
        private Button _btnImmediateFinish;
        private Wizard wizard;
        private Label label1;
        private RadioButton radioNewsGator;
        private System.ComponentModel.IContainer components;

        public FeedSourceType SelectedFeedSource
        {
            get
            {
                if (radioCommonFeedlist.Checked)
                {
                    return FeedSourceType.WindowsRSS;
                }
                else if (radioGoogleReader.Checked)
                {
                    return FeedSourceType.Google;
                }
                else if (radioNewsGator.Checked)
                {
                    return FeedSourceType.NewsGator;
                }
                else
                {
                    return FeedSourceType.Unknown;
                }

            }
        }

        public string UserName
        {
            get { return textUser.Text; }
        }

        public string Password
        {
            get { return textPassword.Text; }
        }

        #endregion

        #region ctor's
        private SynchronizeFeedsWizard()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();           
        }

        public SynchronizeFeedsWizard(IServiceProvider provider): this()
        {
            //initialize wizard state
            serviceProvider = provider;
           
            // form location management:
            windowSerializer = new WindowSerializer(this);
            windowSerializer.SaveOnlyLocation = true;
            windowSerializer.SaveNoWindowState = true;
          
            // to get notified, if the inet connection state changes:
            internetService = (IInternetService)this.GetService(typeof(IInternetService));
            if (internetService != null)
            {
                internetService.InternetConnectionStateChange += OnInternetServiceInternetConnectionStateChange;
            }
            // to checkout the defaults to be used for the new feed:
            IUserPreferences preferencesService = (IUserPreferences)this.GetService(typeof(IUserPreferences));
            coreApplication = (ICoreApplication)this.GetService(typeof(ICoreApplication));         

            this.wizard.SelectedPage = this.pageStartImport;
        }

        #endregion

        #region IWaitDialog Members

        public void Initialize(AutoResetEvent waitHandle, TimeSpan timeout, Icon dialogIcon)
        {
            this.waitHandle = waitHandle;
            this.timeout = timeout;
            this.timeCounter = 0;
            this.operationResult = DialogResult.None;
        }

        public DialogResult StartWaiting(IWin32Window owner, string waitMessage, bool allowCancel)
        {
            // start animation
            this.wizard.SelectedPage.AllowCancel = allowCancel;
            // wait for thread end or timeout (set by timerIncreaseProgress)
            do
            {
                Thread.Sleep(50);
                Application.DoEvents();
            } while (this.operationResult == DialogResult.None);

            if (this.operationTimeout || this.operationResult == DialogResult.Cancel)
                return DialogResult.Cancel;

            return DialogResult.OK;
        }

        #endregion

        #region IServiceProvider Members

        public new object GetService(Type serviceType)
        {

            object srv = null;

            if (serviceProvider != null)
            {
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (components != null)
                {
                    components.Dispose();
                }
                if (internetService != null)
                {
                    internetService.InternetConnectionStateChange -= OnInternetServiceInternetConnectionStateChange;
                    internetService = null;
                }

            }

            base.Dispose(disposing);
        }
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SynchronizeFeedsWizard));
            this.chkDisplayWelcome = new System.Windows.Forms.CheckBox();
            this.pageFeedCredentials = new Divelements.WizardFramework.WizardPage();
            this.lblFeedCredentialsIntro = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.textUser = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.textPassword = new System.Windows.Forms.TextBox();
            this.pageStartImport = new Divelements.WizardFramework.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.radioNewsGator = new System.Windows.Forms.RadioButton();
            this.radioGoogleReader = new System.Windows.Forms.RadioButton();
            this.radioCommonFeedlist = new System.Windows.Forms.RadioButton();
            this._btnImmediateFinish = new System.Windows.Forms.Button();
            this.wizard = new Divelements.WizardFramework.Wizard();
            this.pageFeedCredentials.SuspendLayout();
            this.pageStartImport.SuspendLayout();
            this.wizard.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkDisplayWelcome
            // 
            this.chkDisplayWelcome.Checked = true;
            this.chkDisplayWelcome.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.chkDisplayWelcome, "chkDisplayWelcome");
            this.chkDisplayWelcome.Name = "chkDisplayWelcome";
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
            this.pageFeedCredentials.PreviousPage = this.pageStartImport;
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
            this.textUser.TextChanged += new System.EventHandler(this.textUser_TextChanged);
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
            this.textPassword.TextChanged += new System.EventHandler(this.textPassword_TextChanged);
            // 
            // pageStartImport
            // 
            this.pageStartImport.Controls.Add(this.label1);
            this.pageStartImport.Controls.Add(this.radioNewsGator);
            this.pageStartImport.Controls.Add(this.radioGoogleReader);
            this.pageStartImport.Controls.Add(this.radioCommonFeedlist);
            resources.ApplyResources(this.pageStartImport, "pageStartImport");
            this.pageStartImport.Name = "pageStartImport";
            this.pageStartImport.NextPage = this.pageFeedCredentials;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // radioNewsGator
            // 
            resources.ApplyResources(this.radioNewsGator, "radioNewsGator");
            this.radioNewsGator.Name = "radioNewsGator";
            this.radioNewsGator.UseVisualStyleBackColor = true;
            this.radioNewsGator.CheckedChanged += new System.EventHandler(this.radioNewsGator_CheckedChanged);
            // 
            // radioGoogleReader
            // 
            resources.ApplyResources(this.radioGoogleReader, "radioGoogleReader");
            this.radioGoogleReader.Checked = true;
            this.radioGoogleReader.Name = "radioGoogleReader";
            this.radioGoogleReader.TabStop = true;
            this.radioGoogleReader.UseVisualStyleBackColor = true;
            this.radioGoogleReader.CheckedChanged += new System.EventHandler(this.radioGoogleReader_CheckedChanged);
            // 
            // radioCommonFeedlist
            // 
            resources.ApplyResources(this.radioCommonFeedlist, "radioCommonFeedlist");
            this.radioCommonFeedlist.Name = "radioCommonFeedlist";
            this.radioCommonFeedlist.UseVisualStyleBackColor = true;
            this.radioCommonFeedlist.CheckedChanged += new System.EventHandler(this.radioCommonFeedlist_CheckedChanged);
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
            this.wizard.Controls.Add(this.pageFeedCredentials);
            this.wizard.Controls.Add(this.pageStartImport);
            resources.ApplyResources(this.wizard, "wizard");
            this.wizard.MarginImage = ((System.Drawing.Image)(resources.GetObject("wizard.MarginImage")));
            this.wizard.Name = "wizard";
            this.wizard.SelectedPage = this.pageStartImport;
            this.wizard.Finish += new System.EventHandler(this.OnWizardFinish);
            this.wizard.Cancel += new System.EventHandler(this.OnWizardCancel);
            // 
            // SynchronizeFeedsWizard
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.wizard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SynchronizeFeedsWizard";
            this.pageFeedCredentials.ResumeLayout(false);
            this.pageFeedCredentials.PerformLayout();
            this.pageStartImport.ResumeLayout(false);
            this.pageStartImport.PerformLayout();
            this.wizard.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

   

        internal void ReWireCredentialsStep(bool beforeDiscoverValidateUrl)
        {
           
        }   

        private Image GetWizardTaskImage(WizardValidationTask task)
        {
            if (task == WizardValidationTask.None)
                return null;
            return Resource.LoadBitmap("Resources.WizardTask." + task.ToString() + ".png");
        }
          
      

        /// <summary>
        /// Gets the feed credential user.
        /// </summary>
        /// <value>The feed credential user.</value>
        public string FeedCredentialUser
        {
            get { return this.textUser.Text.Trim(); }
        }
        /// <summary>
        /// Gets the feed credential PWD.
        /// </summary>
        /// <value>The feed credential PWD.</value>
        public string FeedCredentialPwd
        {
            get { return this.textPassword.Text.Trim(); }
        }
             
        // -------------------------------------------------------------------
        // Events 
        // -------------------------------------------------------------------



        private void OnWizardCancel(object sender, EventArgs e)
        {
            if (wizard.SelectedPage.AllowMovePrevious)
            {
                this.DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                this.operationResult = DialogResult.Cancel;
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        private void OnWizardFinish(object sender, EventArgs e)
        {          
            this.DialogResult = DialogResult.OK;
            Close();
        }
       

        private void OnPageWelcome_BeforeDisplay(object sender, EventArgs e)
        {
          
            // get the check value: display this page?
            if (false == this.chkDisplayWelcome.Checked)
            {
                // move to next wizard page 
                wizard.GoNext();
            }
        }

     

        private void OnInternetServiceInternetConnectionStateChange(object sender, InternetConnectionStateChangeEventArgs e)
        {
            bool internetConnected = (e.NewState & INetState.Connected) > 0 && (e.NewState & INetState.Online) > 0;
        }

        private void OnImmediateFinish_Click(object sender, EventArgs e)
        {
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

        private void radioNewsGator_CheckedChanged(object sender, EventArgs e)
        {
            this.pageStartImport.NextPage = pageFeedCredentials; 
        }

        private void radioCommonFeedlist_CheckedChanged(object sender, EventArgs e)
        {
            this.pageStartImport.NextPage = null; 
            
        }

        private void textUser_TextChanged(object sender, EventArgs e)
        {

        }

        private void textPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioGoogleReader_CheckedChanged(object sender, EventArgs e)
        {
            this.pageStartImport.NextPage = pageFeedCredentials; 
        }

     

    }

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
