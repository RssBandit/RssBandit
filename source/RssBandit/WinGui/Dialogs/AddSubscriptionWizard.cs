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
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Divelements.WizardFramework;
using NewsComponents.Net;
using Ninject;
using RssBandit;
using RssBandit.AppServices;
using RssBandit.Common;
using RssBandit.Resources;
using RssBandit.WebSearch;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Utility;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;

namespace RssBandit.WinGui.Dialogs
{

	/// <summary>
	/// SynchronizeFeedsWizard summerize and handles 
	/// all kind of subscriptions now:
	///   By URL (direct, and autodiscovered)
	///   By Search/Topic
	///   NNTP Groups
	///   Direct NNTP Group
	/// </summary>
	internal partial class AddSubscriptionWizard : Form, IWaitDialog
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
		//internal NewsFeed Feed;

		private AddSubscriptionWizardMode wizardMode; 
		private WindowSerializer windowSerializer;
		private IInternetService internetService;
		private ICoreApplication coreApplication;
        private FeedSourceManager Sources; 

		private TimeSpan timeout = TimeSpan.Zero;	// no timeout
		private bool operationTimeout = false;
		private DialogResult operationResult;
		private int timeCounter;
		private AutoResetEvent waitHandle;
		private FeedInfo feedInfo;
		private bool credentialsStepReWired;

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
        	ApplyComponentTranslations();
			// fix the link label(s) linkarea size to fit the whole text (in all translations):
			this.lblReloadNntpListOfGroups.LinkArea = new LinkArea(0,this.lblReloadNntpListOfGroups.Text.Length );
			this.lblAutodiscoverHelp.LinkArea = new LinkArea(0, this.lblAutodiscoverHelp.Text.Length);
			this.lblSyndic8Help.LinkArea = new LinkArea(0, this.lblSyndic8Help.Text.Length);
			this.lblUsenetHelp.LinkArea = new LinkArea(0, this.lblUsenetHelp.Text.Length);
		}

        public AddSubscriptionWizard(AddSubscriptionWizardMode mode)
            : this()
		{
			wizardMode = mode;

			// form location management:
			windowSerializer = new WindowSerializer(this);
			windowSerializer.SaveOnlyLocation = true;
			windowSerializer.SaveNoWindowState = true;

			windowSerializer.LoadStateEvent += OnWindowSerializerLoadStateEvent;
			windowSerializer.SaveStateEvent += OnWindowSerializerSaveStateEvent;

			// to get notified, if the inet connection state changes:
            internetService = RssBanditApplication.Current.Kernel.Get<IInternetService>();
			if (internetService != null) {
				internetService.InternetConnectionStateChange += OnInternetServiceInternetConnectionStateChange;
				checkNewByURLValidate.Enabled = radioNewByTopicSearch.Enabled = internetService.InternetAccessAllowed;
			}
			// to checkout the defaults to be used for the new feed:
            IUserPreferences preferencesService = RssBanditApplication.Current.Kernel.Get<IUserPreferences>();
			this.MaxItemAge = preferencesService.MaxItemAge;

            coreApplication = RssBanditApplication.Current.Kernel.Get<ICoreApplication>();

			this.cboUpdateFrequency.Items.Clear();
			if (!Utils.RefreshRateStrings.Contains(RssBanditApplication.DefaultGlobalRefreshRateMinutes.ToString()))
				Utils.RefreshRateStrings.Add(RssBanditApplication.DefaultGlobalRefreshRateMinutes.ToString());
			if (!Utils.RefreshRateStrings.Contains(coreApplication.CurrentGlobalRefreshRate.ToString()))
				Utils.RefreshRateStrings.Add(coreApplication.CurrentGlobalRefreshRate.ToString());
			
			this.cboUpdateFrequency.DataSource = Utils.RefreshRateStrings;
			this.cboUpdateFrequency.Text = RssBanditApplication.DefaultGlobalRefreshRateMinutes.ToString();
			if (coreApplication.CurrentGlobalRefreshRate > 0)	// if not disabled refreshing
				this.cboUpdateFrequency.Text = coreApplication.CurrentGlobalRefreshRate.ToString(); 					

			// init feedsource combo:
			//TODO: that cast should not be there (extend interface!)
        	RssBanditApplication core = (RssBanditApplication)coreApplication;            
            this.Sources = core.FeedSources;

            foreach (FeedSourceEntry entry in this.Sources.GetOrderedFeedSources())
            {
                this.cboFeedSources.Items.Add(entry.Name);
            }
        	this.FeedSourceName = this.cboFeedSources.Items[0] as string;

            //this may have already been populated by cboFeedSources_SelectedIndexChanged in some cases
            if (this.cboFeedCategory.Items.Count == 0)
            {
                //initialize category combo box			
                foreach (string category in this.Sources[this.FeedSourceName].Source.GetCategories().Keys)
                {
                    if (!string.IsNullOrEmpty(category))
                        this.cboFeedCategory.Items.Add(category);
                }
                this.FeedCategory = coreApplication.DefaultCategory;
            }

			this.WireStepsForMode(this.wizardMode);
			this.wizard.SelectedPage = this.pageWelcome;
		}

		void ApplyComponentTranslations()
		{

			this.comboMaxItemAge.Items.Clear();
			this.comboMaxItemAge.DataSource = Utils.MaxItemAgeStrings;
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

				Dictionary<string,string[]> feedUrls = autoDiscover.DiscoveredFeeds;
                    
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

			foreach(string feedUrl in rssFeeds.Keys) {
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
					    {
                            return string.Format(engine.SearchLink, Uri.EscapeUriString(searchTerm));
                            //return String.Format(new UrlFormatter(), engine.SearchLink, searchTerm);   
					    }
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

            //handle feed URI scheme
            newUrl = newUrl.ToLower().StartsWith("feed://") ? newUrl.Substring(7): newUrl;
            newUrl = newUrl.ToLower().StartsWith("feed:") ? newUrl.Substring(5) : newUrl; 
			
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

                        if (this.listFeeds.Items.Count == 1)
                            return (string)this.listFeeds.Items[0].Tag; 
                        else if (this.listFeeds.SelectedItems.Count > 0)
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

        private void OnAnyLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel o = sender as LinkLabel;
            if (o != null)
            {
                string url = (string)o.Tag;
                if (url == null)
                    url = o.Text;
                coreApplication.NavigateToUrlInExternalBrowser(url);
                o.Links[o.Links.IndexOf(e.Link)].Visited = true;
            }
        }
		
		private void OnReloadNntpGroupList(object sender, LinkLabelLinkClickedEventArgs e) {
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

        private void cboFeedSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cboFeedSources.SelectedItem.ToString()))
            {

                //update category combo box
                cboFeedCategory.Items.Clear();

                foreach (string category in this.Sources[cboFeedSources.SelectedItem.ToString()].Source.GetCategories().Keys)
                {
                    if (!string.IsNullOrEmpty(category))
                        this.cboFeedCategory.Items.Add(category);
                }

                int index = this.cboFeedCategory.Items.Add(coreApplication.DefaultCategory);
                this.cboFeedCategory.SelectedIndex = index; 
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
