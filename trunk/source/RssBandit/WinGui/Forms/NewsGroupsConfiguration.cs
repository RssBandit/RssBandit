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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Infragistics.Win.UltraWinToolbars;
using RssBandit.WinGui.Forms.ControlHelpers;
using RssBandit.WinGui.Utility;
using TD.Eyefinder;

using RssBandit.AppServices;
using RssBandit.Resources;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.News;
using NewsComponents.Utils;
using UserIdentity = RssBandit.Core.Storage.Serialization.UserIdentity;

namespace RssBandit.WinGui.Forms
{
	public enum NewsgroupSettingsView {
		Identity,
		NewsServerSubscriptions,
		NewsServerGeneral,
		NewsServerSettings,
		NewsServerAdvanced,
	}

	/// <summary>
	/// Summary description for NewsgroupsConfiguration.
	/// </summary>
	internal partial class NewsgroupsConfiguration : Form
	{
		public event EventHandler DefinitionsModified;

		private static readonly log4net.ILog _log  = Common.Logging.Log.GetLogger(typeof(NewsgroupsConfiguration));
		private static int unnamedUserIdentityCounter;
		private static int unnamedNntpServerCounter;

		private string currentNGFilter;
		
		private WheelSupport wheelSupport;
		private ToolbarHelper toolbarHelper;
		
		private readonly IdentityNewsServerManager application;
		private NewsgroupSettingsView currentView;
		private readonly ListDictionary userIdentities;	// shallow copies of the originals
		private readonly ListDictionary nntpServers;	// shallow copies of the originals

		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Splitter splitter1;
		private TD.Eyefinder.NavigationBar navigationBar;
		private TD.Eyefinder.NavigationPane newsServersPane;
		private TD.Eyefinder.NavigationPane accountsPane;
		private TD.Eyefinder.HeaderControl accountSettingsPane;
		private System.Windows.Forms.Panel panelDetailsParent;
		private System.Windows.Forms.Panel panelDetailsTop;
		private System.Windows.Forms.ListView listAccounts;
		private System.Windows.Forms.TextBox txtIdentityName;
		private System.Windows.Forms.Label horizontalEdge;
		private System.Windows.Forms.TextBox txtUserName;
		private System.Windows.Forms.TextBox txtUserMail;
		private System.Windows.Forms.TextBox txtRefererUrl;
		private System.Windows.Forms.TextBox txtFilterBy;
		private System.Windows.Forms.Button btnSubscribe;
		private System.Windows.Forms.ColumnHeader colGroupName;
		private System.Windows.Forms.ColumnHeader colDescription;
		private TD.Eyefinder.HeaderControl subscriptionsViewPane;
		private TD.Eyefinder.HeaderControl generalViewPane;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckBox chkConsiderServerOnRefresh;		
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.CheckBox chkUseAuthentication;
		private System.Windows.Forms.TextBox txtServerAuthName;
		private TD.Eyefinder.HeaderControl serverViewPane;
		private TD.Eyefinder.HeaderControl advancedViewPane;
		private System.Windows.Forms.CheckBox chkUseSSL;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label lblCurrentTimout;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.ListView listOfGroups;
		private System.Windows.Forms.ImageList imagesSmall;
		private System.Windows.Forms.ImageList imagesBig;
		private System.Windows.Forms.TextBox txtNewsServerName;
		private System.Windows.Forms.TextBox txtNewsAccountName;
		private System.Windows.Forms.TreeView treeServers;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnUseDefaultPort;
		private System.Windows.Forms.TextBox txtUserOrg;
		private System.Windows.Forms.TextBox txtUserMailResponse;
		private System.Windows.Forms.TextBox txtSignature;
		private System.Windows.Forms.ComboBox cboDefaultIdentities;
		private System.Windows.Forms.TextBox txtServerAuthPassword;
		private System.Windows.Forms.TrackBar trackBarServerTimeout;
		private System.Windows.Forms.TextBox txtServerPort;
		private System.Windows.Forms.Button btnRefreshGroupList;
		private System.Windows.Forms.Button btnUnsubscribe;
		private System.Windows.Forms.Label labelSignature;
		private System.Windows.Forms.Label labelOrganzation;
		private System.Windows.Forms.Label labelResponseAddress;
		private System.Windows.Forms.Label labelRefererUrl;
		private System.Windows.Forms.Label labelMailAddress;
		private System.Windows.Forms.Label labelFullName;
		private System.Windows.Forms.Label labelIdentityName;
		private System.Windows.Forms.Label labelNewsgroupsFilter;
		private System.Windows.Forms.Label labelServerTimeoutSetting;
		private System.Windows.Forms.Label labelHighTimeout;
		private System.Windows.Forms.Label labelLowTimeout;
		private System.Windows.Forms.Label labelServerPort;
		private System.Windows.Forms.Label labelNewsServerAccountPwd;
		private System.Windows.Forms.Label labelNewsServerAccoutnName;
		private System.Windows.Forms.Label labelNewsServerName;
		private System.Windows.Forms.Label labelDefaultEdentity;
		private System.Windows.Forms.Label labelNewsAccount;
		private System.Windows.Forms.Timer timerFilterGroups;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsManager ultraToolbarsManager;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _NewsgroupsConfiguration_Toolbars_Dock_Area_Left;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _NewsgroupsConfiguration_Toolbars_Dock_Area_Right;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _NewsgroupsConfiguration_Toolbars_Dock_Area_Top;
		private Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea _NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom;
		private System.ComponentModel.IContainer components;

		#region ctor's/disposing
		public NewsgroupsConfiguration(IdentityNewsServerManager app, NewsgroupSettingsView initialSettingsDisplay)
		{
			if (null == app)
				throw new ArgumentNullException("app");

			this.application = app;
			this.userIdentities = new ListDictionary();
			this.nntpServers = new ListDictionary();

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.wheelSupport = new WheelSupport(this);

			this.treeServers.AfterSelect += this.OnTreeServersAfterSelect;
			this.listAccounts.SelectedIndexChanged += this.OnListAccountsSelectedIndexChanged;

			this.BackColor = SystemColors.Window;
			this.splitter1.BackColor = FontColorHelper.UiColorScheme.ToolbarGradientLight;
			this.accountSettingsPane.Location = this.subscriptionsViewPane.Location = new Point(0,0);
			this.generalViewPane.Location = this.serverViewPane.Location = this.advancedViewPane.Location = new Point(0,0);
			this.accountSettingsPane.Dock = this.subscriptionsViewPane.Dock = DockStyle.Fill;
			this.generalViewPane.Dock = this.serverViewPane.Dock = this.advancedViewPane.Dock = DockStyle.Fill;

			EnableView(subscriptionsViewPane, false);
			EnableView(generalViewPane, false);
			EnableView(advancedViewPane, false);
			EnableView(serverViewPane, false);
			EnableView(accountSettingsPane, false);

			if (!RssBanditApplication.AutomaticColorSchemes ) {
				Office2003Renderer sdRenderer = new Office2003Renderer();
				sdRenderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Automatic;
				if (!RssBanditApplication.AutomaticColorSchemes) {
					sdRenderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Standard;
				}
				navigationBar.SetActiveRenderer(sdRenderer);
			}

			this.currentView = initialSettingsDisplay;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion


		#region public methods
		public IDictionary ConfiguredIdentities {
			get { return this.userIdentities; }
		}
		public IDictionary ConfiguredNntpServers {
			get { return this.nntpServers; }
		}
		#endregion

		#region private methods
		
		private void InitializeWidgets(IDictionary<string, UserIdentity> identities, IDictionary<string, INntpServerDefinition> nntpServerDefs) {
			this.treeServers.AfterSelect -= this.OnTreeServersAfterSelect;
			this.listAccounts.SelectedIndexChanged -= this.OnListAccountsSelectedIndexChanged;

			if (identities != null) {
				foreach (UserIdentity ui in identities.Values) {
					this.AddNewIdentity((UserIdentity)ui.Clone());
				}
			}
			if (nntpServerDefs != null) {
				foreach (NntpServerDefinition sd in nntpServerDefs.Values) {
					this.AddNewsServer((NntpServerDefinition)sd.Clone());
				}	
			}

			PopulateDefaultIdentities();

			this.treeServers.AfterSelect += this.OnTreeServersAfterSelect;
			this.listAccounts.SelectedIndexChanged += this.OnListAccountsSelectedIndexChanged;
		}

		private void InitializeToolbars() {
			this.toolbarHelper = new ToolbarHelper(this.ultraToolbarsManager);
			this.ultraToolbarsManager.Style = ToolbarStyle.Office2003;
			this.ultraToolbarsManager.ToolClick += this.OnToolbarToolClick;
			this.toolbarHelper.CreateToolbars(this);
		}
		
		private void ActivateView(NewsgroupSettingsView detail) {
			try {
				foreach (Control c in this.panelDetailsTop.Controls) {
					if (c is HeaderControl) {
						c.Visible = false;
					} else {
						System.Diagnostics.Debug.Assert(false, "Unexpected child control type found: " + c.Name);
					}
				}
				
				this.navigationBar.SelectedPaneChanged -= this.OnNavigationBarSelectedPaneChanged;
				if (detail == NewsgroupSettingsView.Identity) {
					this.Text = SR.ConfigIdentitiesDialogCaption;
					if (navigationBar.SelectedPane != accountsPane)
						navigationBar.SelectedPane = accountsPane;
				} else {
					this.Text = SR.ConfigNewsServerDialogCaption;
					if (navigationBar.SelectedPane != newsServersPane)
						navigationBar.SelectedPane = newsServersPane;
				}
				this.navigationBar.SelectedPaneChanged += this.OnNavigationBarSelectedPaneChanged;

				switch (detail) {
					case NewsgroupSettingsView.Identity:
						this.OnAccountSettingsPaneResize(this, EventArgs.Empty);
						accountSettingsPane.Visible = true;
						break;
					case NewsgroupSettingsView.NewsServerSubscriptions:
						this.OnSubscriptionsPaneResize(this, EventArgs.Empty);
						subscriptionsViewPane.Visible = true;
						break;
					case NewsgroupSettingsView.NewsServerGeneral:
						generalViewPane.Visible = true;
						break;
					case NewsgroupSettingsView.NewsServerSettings:
						serverViewPane.Visible = true;
						break;
					case NewsgroupSettingsView.NewsServerAdvanced:
						advancedViewPane.Visible = true;
						break;
				}
				this.currentView = detail;
			} catch (Exception ex) {
				_log.Error("ActivateView() failed", ex);
			}
		}

		private void EnableView(HeaderControl control, bool enable) {
			if (control.Enabled != enable) {
				control.Enabled = enable;
				foreach (Control c in control.Controls) {
					c.Enabled = enable;
				}
			}
		}
		
		
		private void AddNewIdentity() {
			this.AddNewIdentity(new UserIdentity());
			PopulateDefaultIdentities();
			txtIdentityName.SelectAll();
			txtIdentityName.Focus();
		}
		
		private void AddNewIdentity(UserIdentity ui) {
			if (ui == null)
				throw new ArgumentNullException("ui");

			if (string.IsNullOrEmpty(ui.Name)) {
				ui.Name = "New"; // SR.NewUserIdentityNameTemplate"];
			}
			if (this.userIdentities.Contains(ui.Name)) {
				do {
					unnamedUserIdentityCounter++;
				} while(this.userIdentities.Contains(ui.Name + " #" + unnamedUserIdentityCounter));
				ui.Name += " #" + unnamedUserIdentityCounter;
			}
			
			this.userIdentities.Add(ui.Name, ui);
			try {
				listAccounts.BeginUpdate();
				ListViewItem newItem = listAccounts.Items.Add(new ListViewItem(ui.Name, 0));
				newItem.Tag = ui.Name;
				newItem.Selected = true;	// cause event fired
			} finally {
				listAccounts.EndUpdate();
			}
			
		}
		
		private void PopulateIdentity(string identityID) {
			if (identityID != null && this.userIdentities.Contains(identityID)) {
				UserIdentity ui = (UserIdentity)this.userIdentities[identityID];
				txtIdentityName.Text = ui.Name;
				txtUserName.Text = (ui.RealName == null ? String.Empty: ui.RealName);
				txtUserOrg.Text = (ui.Organization == null ? String.Empty: ui.Organization);
				txtUserMail.Text = (ui.MailAddress == null ? String.Empty: ui.MailAddress);
				txtUserMailResponse.Text = (ui.ResponseAddress == null ? String.Empty: ui.ResponseAddress);
				txtRefererUrl.Text = (ui.ReferrerUrl == null ? String.Empty: ui.ReferrerUrl);
				txtSignature.Text = (ui.Signature == null ? String.Empty: ui.Signature);
			}
		}

		private void AddNewsServer() {
			this.AddNewsServer(new NntpServerDefinition());
			txtNewsAccountName.SelectAll();
			txtNewsAccountName.Focus();
		}
		
		private void AddNewsServer(NntpServerDefinition sd) {
			if (sd == null)
				throw new ArgumentNullException("sd");

			if (string.IsNullOrEmpty(sd.Name)) {
				sd.Name = "New"; // SR.NewNntpServerAccountNameTemplate"];
			}
			if (this.nntpServers.Contains(sd.Name)) {
				do {
					unnamedNntpServerCounter++;
				} while(this.nntpServers.Contains(sd.Name + " #" + unnamedNntpServerCounter));
				sd.Name += " #" + unnamedNntpServerCounter;
			}
			
			this.nntpServers.Add(sd.Name, sd);

			TreeNode[] childs = new TreeNode[3];
			childs[0] = new TreeNode(SR.NntpServerConfig_GeneralSettingsNodeCaption, 2, 2);
			childs[1] = new TreeNode(SR.NntpServerConfig_ServerSettingsNodeCaption, 3, 3);
			childs[2] = new TreeNode(SR.NntpServerConfig_AdvancedSettingsNodeCaption, 4, 4);
			childs[0].Tag = NewsgroupSettingsView.NewsServerGeneral;
			childs[1].Tag = NewsgroupSettingsView.NewsServerSettings;
			childs[2].Tag = NewsgroupSettingsView.NewsServerAdvanced;

			TreeNode newItem = new TreeNode(sd.Name, 1, 1, childs);
			newItem.Tag = sd.Name;
			treeServers.Nodes.Add(newItem);
			treeServers.SelectedNode = childs[0];	// cause event fired
		}

		private void PopulateNntpServerDefinition(string nntpServerID) {
			if (nntpServerID != null && this.nntpServers.Contains(nntpServerID)) {
				NntpServerDefinition sd = (NntpServerDefinition)this.nntpServers[nntpServerID];
				
				txtNewsAccountName.Text = sd.Name;
				cboDefaultIdentities.Text = sd.DefaultIdentity;
				chkConsiderServerOnRefresh.Checked = false;
				if (sd.PreventDownloadOnRefreshSpecified)
					chkConsiderServerOnRefresh.Checked = !sd.PreventDownloadOnRefresh;

				txtNewsServerName.Text = sd.Server;
				string u, p;
				FeedSource.GetNntpServerCredentials(sd, out u, out p);
				chkUseAuthentication.Checked = false;
				txtServerAuthName.Enabled = txtServerAuthPassword.Enabled = false;
				if (!string.IsNullOrEmpty(u)) {
					chkUseAuthentication.Checked = true;
					txtServerAuthName.Enabled = txtServerAuthPassword.Enabled = true;
					txtServerAuthName.Text = u;	
					txtServerAuthPassword.Text = p;
				}else{
					txtServerAuthName.Text = txtServerAuthPassword.Text = String.Empty;				
				}
								

				txtServerPort.Text = String.Empty;
				if (sd.PortSpecified)
					txtServerPort.Text = sd.Port.ToString();

				chkUseSSL.Checked = false;
				if (sd.UseSSLSpecified)
					chkUseSSL.Checked = sd.UseSSL;

				trackBarServerTimeout.Value = 0;
				lblCurrentTimout.Text = "0";
				if (sd.TimeoutSpecified) {
					trackBarServerTimeout.Value = sd.Timeout;
					lblCurrentTimout.Text = sd.Timeout.ToString();
				}

				IList<string> groups = application.LoadNntpNewsGroups(this, sd, false);
				listOfGroups.Tag = null;
				this.PopulateNewsGroups(sd, groups, application.CurrentSubscriptions(sd));

			}
		}

		private UserIdentity GetSelectedUserIdentity() {
			if (listAccounts.SelectedItems.Count > 0) {
				string key = (string)listAccounts.SelectedItems[0].Tag;
				if (this.userIdentities.Contains(key))
					return (UserIdentity)this.userIdentities[key];
			} 
			return null;
		}
		
		private NntpServerDefinition GetSelectedNntpServerDefinition() {
			if (treeServers.SelectedNode != null) {
				string key = null;
				if (treeServers.SelectedNode.Parent != null)
					key = (string)treeServers.SelectedNode.Parent.Tag;
				else
					key = (string)treeServers.SelectedNode.Tag;
				if (this.nntpServers.Contains(key))
					return (NntpServerDefinition)this.nntpServers[key];
			} 
			return null;
		}

		private void PopulateDefaultIdentities() {
			cboDefaultIdentities.Items.Clear();

			if (this.userIdentities != null) {
				foreach (UserIdentity ui in userIdentities.Values) {
					cboDefaultIdentities.Items.Add(ui.Name);
				}
			}
		}
        private void PopulateNewsGroups(NntpServerDefinition sd, IList<string> groups, IList<NewsFeed> currentSubscriptions)
        {
			this.PopulateNewsGroups(sd, groups, currentSubscriptions, null);
		}

		private void PopulateNewsGroups(NntpServerDefinition sd, IList<string> groups, IList<NewsFeed> currentSubscriptions, Regex filterExpression) {
			
           if (groups != null) {
				ArrayList alvs = new ArrayList(groups.Count);
				//int imageIndex = 0;
				foreach (string group in groups) {
					// String.Empty is the group description
					//TODO: how we get this nntp group description?

					//DISCUSS: how is the NewsFeed.link build up?
					// all NewsFeed objects with f.newsaccount == sd.Name
					foreach (NewsFeed f in currentSubscriptions) {
						/* if (f.link.IndexOf(group) >= 0)
							imageIndex = 1;	 subscribed */ 
					}

					//TODO: add the "Subscribed" icon, if server/group match an item in currentSubscriptions
					if (filterExpression == null || filterExpression.IsMatch(group)) {
						alvs.Add(new ListViewItem(new string[]{group, String.Empty}));
					}
				}
					
				ListViewItem[] lvs = null;
				
				if (alvs.Count > 0) { 
					lvs = new ListViewItem[alvs.Count];
					alvs.CopyTo(lvs);
				}
					
				lock(this.listOfGroups.Items) {
					this.listOfGroups.Items.Clear();
					if (lvs != null)
						this.listOfGroups.Items.AddRange(lvs);
				}

			} else {
				lock(this.listOfGroups.Items) {
					this.listOfGroups.Items.Clear();
				}
			}
			
		}

		private void DoSubscribe()
		{
			if (listOfGroups.SelectedItems.Count > 0) 
			{
				ListViewItem lv = listOfGroups.SelectedItems[0];
				NntpServerDefinition sd = GetSelectedNntpServerDefinition();
				if (sd != null) 
				{
					ICoreApplication coreApp = IoC.Resolve<ICoreApplication>();
			
					if (coreApp != null) 
					{
						if (coreApp.SubscribeToFeed(
							IdentityNewsServerManager.BuildNntpRequestUri(sd, lv.Text).ToString(), 
							null, lv.Text))
						{
							//TODO set icon
						}
					}
				}
			}

		}
		private void DoFilterNewsGroups(string filterText) {
			
			if (String.Compare(currentNGFilter, filterText, true) != 0) {
				
				NntpServerDefinition sd = this.GetSelectedNntpServerDefinition();
				IList<string> groups = null;
				if (listOfGroups.Tag == null) {
					if (sd != null) {
						listOfGroups.Tag = groups = application.LoadNntpNewsGroups(this, sd, false);
						this.PopulateNewsGroups(sd, groups, application.CurrentSubscriptions(sd));
					}
				} else {
					groups = (IList<string>)listOfGroups.Tag;
				}

				if (StringHelper.EmptyTrimOrNull(filterText)) {	// reset to view all
					this.PopulateNewsGroups(sd, groups, application.CurrentSubscriptions(sd));
				} else {	// do filter
					Regex regFilter = new Regex(filterText.Trim(), RegexOptions.IgnoreCase);
					this.PopulateNewsGroups(sd, groups, application.CurrentSubscriptions(sd), regFilter);
				}
			}
			
			currentNGFilter = filterText;
		}

		#endregion

		#region event handling

		private void OnNavigationBarSelectedPaneChanged(object sender, EventArgs e) {
			if (this.navigationBar.SelectedPane == this.newsServersPane) {
				this.ActivateView(NewsgroupSettingsView.NewsServerSubscriptions);
				this.OnTreeServersAfterSelect(treeServers, new TreeViewEventArgs(treeServers.SelectedNode)); 
			} else if (this.navigationBar.SelectedPane == this.accountsPane) {
				this.ActivateView(NewsgroupSettingsView.Identity);
				this.OnListAccountsSelectedIndexChanged(listAccounts, EventArgs.Empty);
			}
		}

		private void OnNewIdentityToolActivate(object sender, EventArgs e) {
			this.ActivateView(NewsgroupSettingsView.Identity);
			this.AddNewIdentity();
		}

		private void OnNewNewsServerToolActivate(object sender, EventArgs e) {
			this.ActivateView(NewsgroupSettingsView.NewsServerGeneral);
			this.AddNewsServer();
		}

		private void OnDeleteItemToolActivate(object sender, EventArgs e) {
			if (currentView == NewsgroupSettingsView.Identity) {
				if (listAccounts.SelectedItems.Count > 0) {
					string key = (string)listAccounts.SelectedItems[0].Tag;
					if (key != null && this.userIdentities.Contains(key)) {
						UserIdentity ui = (UserIdentity)this.userIdentities[key];
						this.userIdentities.Remove(key);
						// remove from default identities list AND assigned nntp servers:
						this.cboDefaultIdentities.Items.Remove(ui.Name);
						foreach (string serverKey in this.nntpServers.Keys) {
							NntpServerDefinition sd = (NntpServerDefinition)this.nntpServers[serverKey];
							if (sd.DefaultIdentity == ui.Name)
								sd.DefaultIdentity = String.Empty;
						}
						this.listAccounts.Items.Remove(listAccounts.SelectedItems[0]);
					}
				}
			} else {
				if (treeServers.SelectedNode != null) {
					TreeNode n = null;
					if (treeServers.SelectedNode.Parent != null) {
						n = treeServers.SelectedNode.Parent;
					} else {
						n = treeServers.SelectedNode;
					}
					string key = (string)n.Tag;

					if (key != null && this.nntpServers.Contains(key)) {
						//NntpServerDefinition sd = (NntpServerDefinition)this.nntpServers[key];
						this.nntpServers.Remove(key);
						this.treeServers.Nodes.Remove(n);
					}
				}
			}
		}

		private void OnSubscriptionsPaneResize(object sender, System.EventArgs e) {
			// only there because the listview does not resize correctly of the parent gets a dock setting of fill :-(
			this.listOfGroups.Height = subscriptionsViewPane.Height - this.listOfGroups.Top - 10;
			this.listOfGroups.Width = this.txtFilterBy.Width;
		}

		private void OnAccountSettingsPaneResize(object sender, System.EventArgs e) {
			this.txtSignature.Height = accountSettingsPane.Height - this.txtSignature.Top - 10;
			this.txtSignature.Width = this.txtUserName.Width;
		}

		private void OnListAccountsSelectedIndexChanged(object sender, System.EventArgs e) {
			bool enable = listAccounts.SelectedItems.Count > 0;
			EnableView(accountSettingsPane, enable);
			if (enable) {
				this.PopulateIdentity((string)listAccounts.SelectedItems[0].Tag);
			}
		}

		private void OnTreeServersAfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {
			bool enable = e.Node != null;
			EnableView(subscriptionsViewPane, enable);
			EnableView(generalViewPane, enable);
			EnableView(advancedViewPane, enable);
			EnableView(serverViewPane, enable);
			
			if (enable) {
				string key = null;
				NewsgroupSettingsView what = NewsgroupSettingsView.NewsServerSubscriptions;
				if (e.Node.Parent != null) {
					what = (NewsgroupSettingsView)e.Node.Tag;
					key = (string)e.Node.Parent.Tag;
				} else {
					key = (string)e.Node.Tag;
				}
				if (what == NewsgroupSettingsView.NewsServerGeneral) {
					this.ActivateView(NewsgroupSettingsView.NewsServerGeneral);
				} else if (what == NewsgroupSettingsView.NewsServerSettings) {
					this.ActivateView(NewsgroupSettingsView.NewsServerSettings);
				} else if (what == NewsgroupSettingsView.NewsServerAdvanced) {
					this.ActivateView(NewsgroupSettingsView.NewsServerAdvanced);
				} else {
					// !! the node.Tag here contains the string ID of the nntp server def. !!
					this.ActivateView(NewsgroupSettingsView.NewsServerSubscriptions);
				}
				
				this.PopulateNntpServerDefinition(key);
			}

		}

		private void OnNewsAccountNameTextChanged(object sender, System.EventArgs e) {
			if (treeServers.SelectedNode.Parent != null)
				treeServers.SelectedNode.Parent.Text = txtNewsAccountName.Text;
			else
				treeServers.SelectedNode.Text = txtNewsAccountName.Text;
		}
		private void OnIdentityNameTextChanged(object sender, System.EventArgs e) {
			listAccounts.SelectedItems[0].Text = txtIdentityName.Text;
		}
		private void OnTrackBarServerTimeoutChanged(object sender, System.EventArgs e) {
			lblCurrentTimout.Text = trackBarServerTimeout.Value.ToString();
		}
		private void btnUseDefaultPort_Click(object sender, System.EventArgs e) {
			txtServerPort.Text = NntpWebRequest.NntpDefaultServerPort.ToString();
		}
		private void OnUseAuthenticationCheckedChanged(object sender, System.EventArgs e) {
			this.txtServerAuthName.Enabled = this.txtServerAuthPassword.Enabled = this.chkUseAuthentication.Checked;
		}

		private void OnWidgetValidated(object sender, System.EventArgs e) {
			// general widget validated.
			UserIdentity selectedUser = this.GetSelectedUserIdentity();
			NntpServerDefinition selectedServer = this.GetSelectedNntpServerDefinition();

			if (sender == this.txtIdentityName) {
				if (selectedUser != null)
					selectedUser.Name = this.txtIdentityName.Text;
			}
			if (sender == this.txtUserName) {
				if (selectedUser != null)
					selectedUser.RealName = this.txtUserName.Text;
			}
			if (sender == this.txtUserOrg) {
				if (selectedUser != null)
					selectedUser.Organization = this.txtUserOrg.Text;
			}
			if (sender == this.txtUserMail) {
				if (selectedUser != null)
					selectedUser.MailAddress = this.txtUserMail.Text;
			}
			if (sender == this.txtUserMailResponse) {
				if (selectedUser != null)
					selectedUser.ResponseAddress = this.txtUserMailResponse.Text;
			}
			if (sender == this.txtRefererUrl) {
				if (selectedUser != null)
					selectedUser.ReferrerUrl = this.txtRefererUrl.Text;
			}
			if (sender == this.txtSignature) {
				if (selectedUser != null)
					selectedUser.Signature = this.txtSignature.Text;
			}

			// Nntp server def. takeover

			if (sender == this.txtNewsAccountName) {
				if (selectedServer != null)
					selectedServer.Name = this.txtNewsAccountName.Text;
			}
			if (sender == this.cboDefaultIdentities) {
				if (selectedServer != null)
					selectedServer.DefaultIdentity = this.cboDefaultIdentities.Text;
			}
			if (sender == this.chkConsiderServerOnRefresh) {
				if (selectedServer != null) {
					selectedServer.PreventDownloadOnRefreshSpecified = (!this.chkConsiderServerOnRefresh.Checked);
					selectedServer.PreventDownloadOnRefresh = !this.chkConsiderServerOnRefresh.Checked;
				}
			}
			if (sender == this.txtNewsServerName) {
				if (selectedServer != null)
					selectedServer.Server = this.txtNewsServerName.Text;
			}
			if (sender == this.txtServerAuthName) {
				if (selectedServer != null)
					selectedServer.AuthUser = this.txtServerAuthName.Text;
			}
			if (sender == this.txtServerAuthPassword) {
				if (selectedServer != null)
				{
					FeedSource.SetNntpServerCredentials(
						selectedServer, this.txtServerAuthName.Text, txtServerAuthPassword.Text);
				}
			}			
			if (sender == this.txtServerPort) {
				if (selectedServer != null) {
					int port = Int32.Parse(this.txtServerPort.Text);
					if (port > 0 && port != NntpWebRequest.NntpDefaultServerPort) {
						selectedServer.Port = port;
						selectedServer.PortSpecified = true;
					} else {
						selectedServer.PortSpecified = false;
					}
				}
			}
			if (sender == this.chkUseSSL) {
				if (selectedServer != null) {
					selectedServer.UseSSLSpecified = (this.chkUseSSL.Checked);
					selectedServer.UseSSL = this.chkUseSSL.Checked;
				}
			}
			if (sender == this.trackBarServerTimeout) {
				if (selectedServer != null) {
					selectedServer.TimeoutSpecified = false;
					if (trackBarServerTimeout.Value > 0) {
						selectedServer.TimeoutSpecified = true;
						selectedServer.Timeout  = trackBarServerTimeout.Value;
					}
				}
			}

			this.btnApply.Enabled = true;
		}

		private void OnWidgetValidating(object sender, System.ComponentModel.CancelEventArgs e) {
			// general widget validating

			this.btnApply.Enabled = false;

			if (sender == txtIdentityName) {

				txtIdentityName.Text = txtIdentityName.Text.Trim();
				if (txtIdentityName.Text.Length == 0) {
					errorProvider.SetError(txtIdentityName, "Empty value is not allowed here");
					e.Cancel = true;
				}

				string myKey = (string)listAccounts.SelectedItems[0].Tag;
				UserIdentity me = (UserIdentity)this.userIdentities[myKey];
				foreach (UserIdentity ui in this.userIdentities.Values) {
					if (ReferenceEquals(ui, me) == false && ui.Name.Equals(txtIdentityName.Text)) {
						errorProvider.SetError(txtIdentityName, "There is already a user identity defined with this name. Please use a different one.");
						e.Cancel = true;
					}
				}

			} else if (sender == txtNewsAccountName) {

				txtNewsAccountName.Text = txtNewsAccountName.Text.Trim();
				if (txtNewsAccountName.Text.Length == 0) {
					errorProvider.SetError(txtNewsAccountName, "Empty value is not allowed here");
					e.Cancel = true;
				}
			}

			if (!e.Cancel)
				errorProvider.SetError((Control)sender, null);

		}

		private void btnApply_Click(object sender, EventArgs e) {
			
			// raise event to allow take over the items from local userIdentities and nntpServers 
			// to app.FeedHandler.Identity and app.FeedHandler.NntpServers
			if (DefinitionsModified != null)
				DefinitionsModified(this, EventArgs.Empty);

			this.btnApply.Enabled = false;
		}

		private void btnOK_Click(object sender, EventArgs e) {
			this.Hide();
			this.btnApply_Click(sender, e);
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void OnFormLoad(object sender, EventArgs e) {
			this.InitializeToolbars();
			this.InitializeWidgets(application.Identities, application.CurrentNntpServers); 
			this.ActivateView(this.currentView);
			if (currentView == NewsgroupSettingsView.Identity) {
				if (listAccounts.SelectedItems.Count > 0)
					this.OnListAccountsSelectedIndexChanged(listAccounts, EventArgs.Empty);
			} else {
				if (treeServers.SelectedNode != null)
					this.OnTreeServersAfterSelect(treeServers, new TreeViewEventArgs(treeServers.SelectedNode));
			}
		}

		private void btnSubscribe_Click(object sender, EventArgs e) {
			this.DoSubscribe();
		}

		private void btnUnsubscribe_Click(object sender, EventArgs e) {
			//TODO
		}

		private void btnRefreshGroupList_Click(object sender, EventArgs e) {
			try {
				using (new Genghis.Windows.Forms.CursorChanger(Cursors.WaitCursor)) {
					NntpServerDefinition sd = this.GetSelectedNntpServerDefinition();
					if (sd != null) {
						IList<string> groups = application.LoadNntpNewsGroups(this, sd, true);
						listOfGroups.Tag = null;
						this.PopulateNewsGroups(sd, groups, application.CurrentSubscriptions(sd));
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OnFilterByTextChanged(object sender, EventArgs e) {
			this.timerFilterGroups.Enabled = true;
		}

		private void OnFilterNewsGroupsTick(object sender, EventArgs e) {
			this.timerFilterGroups.Enabled = false;
			this.DoFilterNewsGroups(txtFilterBy.Text);
		}

		private void OnListOfGroupsDoubleClick(object sender, EventArgs e)
		{
			this.DoSubscribe();
		}
		
		private void OnToolbarToolClick(object sender, ToolClickEventArgs e) {
			switch (e.Tool.Key) {
				case "toolNewIndentity":    // ButtonTool
					OnNewIdentityToolActivate(sender, EventArgs.Empty);
					break;

				case "toolNewNntpServer":    // ButtonTool
					OnNewNewsServerToolActivate(sender, EventArgs.Empty);
					break;

				case "toolDelete":    // ButtonTool
					OnDeleteItemToolActivate(sender, EventArgs.Empty);
					break;

			}
		}
		
		#endregion

	}
}
