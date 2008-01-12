#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region CVS Version Log
/*
 * $Log: NewsGroupsConfiguration.cs,v $
 * Revision 1.34  2007/05/15 21:37:04  carnage4life
 * Fixed issue where username and password for previous newsgroup was shown when browsing from a newsgroup with auth information to one without in Server Settings
 *
 * Revision 1.33  2007/01/11 15:07:54  t_rendelmann
 * IG assemblies replaced by hotfix versions; migrated last Sandbar toolbar usage to IG ultratoolbar
 *
 * Revision 1.32  2006/08/08 10:21:40  t_rendelmann
 * fixed: on explorer bar active bar changes the active view was not always active/populated with the selected subitem
 * fixed: on remove of a identity the assigned nntp server default identities are not touched (cleared, if it was the deleted identity)
 *
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

using RssBandit;
using RssBandit.AppServices;
using RssBandit.Resources;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.News;
using NewsComponents.Utils;


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
	public class NewsgroupsConfiguration : System.Windows.Forms.Form
	{
		public event EventHandler DefinitionsModified;

		private static readonly log4net.ILog _log  = Common.Logging.Log.GetLogger(typeof(NewsgroupsConfiguration));
		private static int unnamedUserIdentityCounter = 0;
		private static int unnamedNntpServerCounter = 0;

		private string currentNGFilter = null;
		
		private WheelSupport wheelSupport;
		private ToolbarHelper toolbarHelper;
		
		private IdentityNewsServerManager application;
		private NewsgroupSettingsView currentView;
		private ListDictionary userIdentities, nntpServers;	// shallow copies of the originals

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

			this.treeServers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeServersAfterSelect);
			this.listAccounts.SelectedIndexChanged += new System.EventHandler(this.OnListAccountsSelectedIndexChanged);

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
				TD.Eyefinder.Office2003Renderer sdRenderer = new TD.Eyefinder.Office2003Renderer();
				sdRenderer.ColorScheme = TD.Eyefinder.Office2003Renderer.Office2003ColorScheme.Automatic;
				if (!RssBanditApplication.AutomaticColorSchemes) {
					sdRenderer.ColorScheme = TD.Eyefinder.Office2003Renderer.Office2003ColorScheme.Standard;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewsgroupsConfiguration));
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.navigationBar = new TD.Eyefinder.NavigationBar();
			this.newsServersPane = new TD.Eyefinder.NavigationPane();
			this.treeServers = new System.Windows.Forms.TreeView();
			this.imagesSmall = new System.Windows.Forms.ImageList(this.components);
			this.accountsPane = new TD.Eyefinder.NavigationPane();
			this.listAccounts = new System.Windows.Forms.ListView();
			this.imagesBig = new System.Windows.Forms.ImageList(this.components);
			this.accountSettingsPane = new TD.Eyefinder.HeaderControl();
			this.txtSignature = new System.Windows.Forms.TextBox();
			this.labelSignature = new System.Windows.Forms.Label();
			this.txtUserOrg = new System.Windows.Forms.TextBox();
			this.labelOrganzation = new System.Windows.Forms.Label();
			this.txtUserMailResponse = new System.Windows.Forms.TextBox();
			this.labelResponseAddress = new System.Windows.Forms.Label();
			this.txtRefererUrl = new System.Windows.Forms.TextBox();
			this.labelRefererUrl = new System.Windows.Forms.Label();
			this.txtUserMail = new System.Windows.Forms.TextBox();
			this.labelMailAddress = new System.Windows.Forms.Label();
			this.txtUserName = new System.Windows.Forms.TextBox();
			this.labelFullName = new System.Windows.Forms.Label();
			this.horizontalEdge = new System.Windows.Forms.Label();
			this.txtIdentityName = new System.Windows.Forms.TextBox();
			this.labelIdentityName = new System.Windows.Forms.Label();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panelDetailsParent = new System.Windows.Forms.Panel();
			this.btnApply = new System.Windows.Forms.Button();
			this.panelDetailsTop = new System.Windows.Forms.Panel();
			this.subscriptionsViewPane = new TD.Eyefinder.HeaderControl();
			this.txtFilterBy = new System.Windows.Forms.TextBox();
			this.labelNewsgroupsFilter = new System.Windows.Forms.Label();
			this.listOfGroups = new System.Windows.Forms.ListView();
			this.colGroupName = new System.Windows.Forms.ColumnHeader();
			this.colDescription = new System.Windows.Forms.ColumnHeader();
			this.btnRefreshGroupList = new System.Windows.Forms.Button();
			this.btnUnsubscribe = new System.Windows.Forms.Button();
			this.btnSubscribe = new System.Windows.Forms.Button();
			this.advancedViewPane = new TD.Eyefinder.HeaderControl();
			this.labelServerTimeoutSetting = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.lblCurrentTimout = new System.Windows.Forms.Label();
			this.trackBarServerTimeout = new System.Windows.Forms.TrackBar();
			this.btnUseDefaultPort = new System.Windows.Forms.Button();
			this.labelHighTimeout = new System.Windows.Forms.Label();
			this.chkUseSSL = new System.Windows.Forms.CheckBox();
			this.labelLowTimeout = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.txtServerPort = new System.Windows.Forms.TextBox();
			this.labelServerPort = new System.Windows.Forms.Label();
			this.serverViewPane = new TD.Eyefinder.HeaderControl();
			this.txtServerAuthPassword = new System.Windows.Forms.TextBox();
			this.labelNewsServerAccountPwd = new System.Windows.Forms.Label();
			this.txtServerAuthName = new System.Windows.Forms.TextBox();
			this.chkUseAuthentication = new System.Windows.Forms.CheckBox();
			this.labelNewsServerAccoutnName = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.txtNewsServerName = new System.Windows.Forms.TextBox();
			this.labelNewsServerName = new System.Windows.Forms.Label();
			this.generalViewPane = new TD.Eyefinder.HeaderControl();
			this.chkConsiderServerOnRefresh = new System.Windows.Forms.CheckBox();
			this.cboDefaultIdentities = new System.Windows.Forms.ComboBox();
			this.labelDefaultEdentity = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.txtNewsAccountName = new System.Windows.Forms.TextBox();
			this.labelNewsAccount = new System.Windows.Forms.Label();
			this.errorProvider = new System.Windows.Forms.ErrorProvider();
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this.ultraToolbarsManager = new Infragistics.Win.UltraWinToolbars.UltraToolbarsManager(this.components);
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom = new Infragistics.Win.UltraWinToolbars.UltraToolbarsDockArea();
			this.timerFilterGroups = new System.Windows.Forms.Timer(this.components);
			this.navigationBar.SuspendLayout();
			this.newsServersPane.SuspendLayout();
			this.accountsPane.SuspendLayout();
			this.accountSettingsPane.SuspendLayout();
			this.panelDetailsParent.SuspendLayout();
			this.panelDetailsTop.SuspendLayout();
			this.subscriptionsViewPane.SuspendLayout();
			this.advancedViewPane.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarServerTimeout)).BeginInit();
			this.serverViewPane.SuspendLayout();
			this.generalViewPane.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ultraToolbarsManager)).BeginInit();
			this.SuspendLayout();
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
			this.errorProvider.SetError(this.btnCancel, resources.GetString("btnCancel.Error"));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.errorProvider.SetIconAlignment(this.btnCancel, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnCancel.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.btnCancel, ((int)(resources.GetObject("btnCancel.IconPadding"))));
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
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
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
			this.errorProvider.SetError(this.btnOK, resources.GetString("btnOK.Error"));
			this.btnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOK.FlatStyle")));
			this.btnOK.Font = ((System.Drawing.Font)(resources.GetObject("btnOK.Font")));
			this.errorProvider.SetIconAlignment(this.btnOK, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnOK.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.btnOK, ((int)(resources.GetObject("btnOK.IconPadding"))));
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
			this.btnOK.Visible = ((bool)(resources.GetObject("btnOK.Visible")));
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// navigationBar
			// 
			this.navigationBar.AccessibleDescription = resources.GetString("navigationBar.AccessibleDescription");
			this.navigationBar.AccessibleName = resources.GetString("navigationBar.AccessibleName");
			this.navigationBar.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("navigationBar.Anchor")));
			this.navigationBar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("navigationBar.BackgroundImage")));
			this.navigationBar.Controls.Add(this.newsServersPane);
			this.navigationBar.Controls.Add(this.accountsPane);
			this.navigationBar.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("navigationBar.Dock")));
			this.navigationBar.Enabled = ((bool)(resources.GetObject("navigationBar.Enabled")));
			this.errorProvider.SetError(this.navigationBar, resources.GetString("navigationBar.Error"));
			this.navigationBar.Font = ((System.Drawing.Font)(resources.GetObject("navigationBar.Font")));
			this.navigationBar.HeaderFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.errorProvider.SetIconAlignment(this.navigationBar, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("navigationBar.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.navigationBar, ((int)(resources.GetObject("navigationBar.IconPadding"))));
			this.navigationBar.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("navigationBar.ImeMode")));
			this.navigationBar.Location = ((System.Drawing.Point)(resources.GetObject("navigationBar.Location")));
			this.navigationBar.Name = "navigationBar";
			this.navigationBar.PaneFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.navigationBar.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("navigationBar.RightToLeft")));
			this.navigationBar.SelectedPane = this.accountsPane;
			this.navigationBar.ShowPanes = 2;
			this.navigationBar.Size = ((System.Drawing.Size)(resources.GetObject("navigationBar.Size")));
			this.navigationBar.TabIndex = ((int)(resources.GetObject("navigationBar.TabIndex")));
			this.navigationBar.Text = resources.GetString("navigationBar.Text");
			this.navigationBar.Visible = ((bool)(resources.GetObject("navigationBar.Visible")));
			this.navigationBar.SelectedPaneChanged += new System.EventHandler(this.OnNavigationBarSelectedPaneChanged);
			// 
			// newsServersPane
			// 
			this.newsServersPane.AccessibleDescription = resources.GetString("newsServersPane.AccessibleDescription");
			this.newsServersPane.AccessibleName = resources.GetString("newsServersPane.AccessibleName");
			this.newsServersPane.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("newsServersPane.Anchor")));
			this.newsServersPane.AutoScroll = ((bool)(resources.GetObject("newsServersPane.AutoScroll")));
			this.newsServersPane.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("newsServersPane.AutoScrollMargin")));
			this.newsServersPane.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("newsServersPane.AutoScrollMinSize")));
			this.newsServersPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("newsServersPane.BackgroundImage")));
			this.newsServersPane.Controls.Add(this.treeServers);
			this.newsServersPane.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("newsServersPane.Dock")));
			this.newsServersPane.Enabled = ((bool)(resources.GetObject("newsServersPane.Enabled")));
			this.errorProvider.SetError(this.newsServersPane, resources.GetString("newsServersPane.Error"));
			this.newsServersPane.Font = ((System.Drawing.Font)(resources.GetObject("newsServersPane.Font")));
			this.errorProvider.SetIconAlignment(this.newsServersPane, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("newsServersPane.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.newsServersPane, ((int)(resources.GetObject("newsServersPane.IconPadding"))));
			this.newsServersPane.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("newsServersPane.ImeMode")));
			this.newsServersPane.LargeImage = ((System.Drawing.Image)(resources.GetObject("newsServersPane.LargeImage")));
			this.newsServersPane.Location = ((System.Drawing.Point)(resources.GetObject("newsServersPane.Location")));
			this.newsServersPane.Name = "newsServersPane";
			this.newsServersPane.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("newsServersPane.RightToLeft")));
			this.newsServersPane.Size = ((System.Drawing.Size)(resources.GetObject("newsServersPane.Size")));
			this.newsServersPane.SmallImage = ((System.Drawing.Image)(resources.GetObject("newsServersPane.SmallImage")));
			this.newsServersPane.TabIndex = ((int)(resources.GetObject("newsServersPane.TabIndex")));
			this.newsServersPane.Text = resources.GetString("newsServersPane.Text");
			this.newsServersPane.Visible = ((bool)(resources.GetObject("newsServersPane.Visible")));
			// 
			// treeServers
			// 
			this.treeServers.AccessibleDescription = resources.GetString("treeServers.AccessibleDescription");
			this.treeServers.AccessibleName = resources.GetString("treeServers.AccessibleName");
			this.treeServers.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("treeServers.Anchor")));
			this.treeServers.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("treeServers.BackgroundImage")));
			this.treeServers.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeServers.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("treeServers.Dock")));
			this.treeServers.Enabled = ((bool)(resources.GetObject("treeServers.Enabled")));
			this.errorProvider.SetError(this.treeServers, resources.GetString("treeServers.Error"));
			this.treeServers.Font = ((System.Drawing.Font)(resources.GetObject("treeServers.Font")));
			this.treeServers.HideSelection = false;
			this.errorProvider.SetIconAlignment(this.treeServers, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("treeServers.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.treeServers, ((int)(resources.GetObject("treeServers.IconPadding"))));
			this.treeServers.ImageIndex = ((int)(resources.GetObject("treeServers.ImageIndex")));
			this.treeServers.ImageList = this.imagesSmall;
			this.treeServers.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("treeServers.ImeMode")));
			this.treeServers.Indent = ((int)(resources.GetObject("treeServers.Indent")));
			this.treeServers.ItemHeight = ((int)(resources.GetObject("treeServers.ItemHeight")));
			this.treeServers.Location = ((System.Drawing.Point)(resources.GetObject("treeServers.Location")));
			this.treeServers.Name = "treeServers";
			this.treeServers.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("treeServers.RightToLeft")));
			this.treeServers.SelectedImageIndex = ((int)(resources.GetObject("treeServers.SelectedImageIndex")));
			this.treeServers.Size = ((System.Drawing.Size)(resources.GetObject("treeServers.Size")));
			this.treeServers.TabIndex = ((int)(resources.GetObject("treeServers.TabIndex")));
			this.treeServers.Text = resources.GetString("treeServers.Text");
			this.treeServers.Visible = ((bool)(resources.GetObject("treeServers.Visible")));
			// 
			// imagesSmall
			// 
			this.imagesSmall.ImageSize = ((System.Drawing.Size)(resources.GetObject("imagesSmall.ImageSize")));
			this.imagesSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagesSmall.ImageStream")));
			this.imagesSmall.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// accountsPane
			// 
			this.accountsPane.AccessibleDescription = resources.GetString("accountsPane.AccessibleDescription");
			this.accountsPane.AccessibleName = resources.GetString("accountsPane.AccessibleName");
			this.accountsPane.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accountsPane.Anchor")));
			this.accountsPane.AutoScroll = ((bool)(resources.GetObject("accountsPane.AutoScroll")));
			this.accountsPane.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("accountsPane.AutoScrollMargin")));
			this.accountsPane.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("accountsPane.AutoScrollMinSize")));
			this.accountsPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("accountsPane.BackgroundImage")));
			this.accountsPane.Controls.Add(this.listAccounts);
			this.accountsPane.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accountsPane.Dock")));
			this.accountsPane.Enabled = ((bool)(resources.GetObject("accountsPane.Enabled")));
			this.errorProvider.SetError(this.accountsPane, resources.GetString("accountsPane.Error"));
			this.accountsPane.Font = ((System.Drawing.Font)(resources.GetObject("accountsPane.Font")));
			this.errorProvider.SetIconAlignment(this.accountsPane, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("accountsPane.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.accountsPane, ((int)(resources.GetObject("accountsPane.IconPadding"))));
			this.accountsPane.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accountsPane.ImeMode")));
			this.accountsPane.LargeImage = ((System.Drawing.Image)(resources.GetObject("accountsPane.LargeImage")));
			this.accountsPane.Location = ((System.Drawing.Point)(resources.GetObject("accountsPane.Location")));
			this.accountsPane.Name = "accountsPane";
			this.accountsPane.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accountsPane.RightToLeft")));
			this.accountsPane.Size = ((System.Drawing.Size)(resources.GetObject("accountsPane.Size")));
			this.accountsPane.SmallImage = ((System.Drawing.Image)(resources.GetObject("accountsPane.SmallImage")));
			this.accountsPane.TabIndex = ((int)(resources.GetObject("accountsPane.TabIndex")));
			this.accountsPane.Text = resources.GetString("accountsPane.Text");
			this.accountsPane.Visible = ((bool)(resources.GetObject("accountsPane.Visible")));
			// 
			// listAccounts
			// 
			this.listAccounts.AccessibleDescription = resources.GetString("listAccounts.AccessibleDescription");
			this.listAccounts.AccessibleName = resources.GetString("listAccounts.AccessibleName");
			this.listAccounts.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listAccounts.Alignment")));
			this.listAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listAccounts.Anchor")));
			this.listAccounts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listAccounts.BackgroundImage")));
			this.listAccounts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listAccounts.Dock")));
			this.listAccounts.Enabled = ((bool)(resources.GetObject("listAccounts.Enabled")));
			this.errorProvider.SetError(this.listAccounts, resources.GetString("listAccounts.Error"));
			this.listAccounts.Font = ((System.Drawing.Font)(resources.GetObject("listAccounts.Font")));
			this.listAccounts.HideSelection = false;
			this.errorProvider.SetIconAlignment(this.listAccounts, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("listAccounts.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.listAccounts, ((int)(resources.GetObject("listAccounts.IconPadding"))));
			this.listAccounts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listAccounts.ImeMode")));
			this.listAccounts.LabelWrap = ((bool)(resources.GetObject("listAccounts.LabelWrap")));
			this.listAccounts.LargeImageList = this.imagesBig;
			this.listAccounts.Location = ((System.Drawing.Point)(resources.GetObject("listAccounts.Location")));
			this.listAccounts.MultiSelect = false;
			this.listAccounts.Name = "listAccounts";
			this.listAccounts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listAccounts.RightToLeft")));
			this.listAccounts.Size = ((System.Drawing.Size)(resources.GetObject("listAccounts.Size")));
			this.listAccounts.SmallImageList = this.imagesSmall;
			this.listAccounts.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listAccounts.TabIndex = ((int)(resources.GetObject("listAccounts.TabIndex")));
			this.listAccounts.Text = resources.GetString("listAccounts.Text");
			this.listAccounts.Visible = ((bool)(resources.GetObject("listAccounts.Visible")));
			// 
			// imagesBig
			// 
			this.imagesBig.ImageSize = ((System.Drawing.Size)(resources.GetObject("imagesBig.ImageSize")));
			this.imagesBig.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imagesBig.ImageStream")));
			this.imagesBig.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// accountSettingsPane
			// 
			this.accountSettingsPane.AccessibleDescription = resources.GetString("accountSettingsPane.AccessibleDescription");
			this.accountSettingsPane.AccessibleName = resources.GetString("accountSettingsPane.AccessibleName");
			this.accountSettingsPane.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accountSettingsPane.Anchor")));
			this.accountSettingsPane.AutoScroll = ((bool)(resources.GetObject("accountSettingsPane.AutoScroll")));
			this.accountSettingsPane.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("accountSettingsPane.AutoScrollMargin")));
			this.accountSettingsPane.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("accountSettingsPane.AutoScrollMinSize")));
			this.accountSettingsPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("accountSettingsPane.BackgroundImage")));
			this.accountSettingsPane.Controls.Add(this.txtSignature);
			this.accountSettingsPane.Controls.Add(this.labelSignature);
			this.accountSettingsPane.Controls.Add(this.txtUserOrg);
			this.accountSettingsPane.Controls.Add(this.labelOrganzation);
			this.accountSettingsPane.Controls.Add(this.txtUserMailResponse);
			this.accountSettingsPane.Controls.Add(this.labelResponseAddress);
			this.accountSettingsPane.Controls.Add(this.txtRefererUrl);
			this.accountSettingsPane.Controls.Add(this.labelRefererUrl);
			this.accountSettingsPane.Controls.Add(this.txtUserMail);
			this.accountSettingsPane.Controls.Add(this.labelMailAddress);
			this.accountSettingsPane.Controls.Add(this.txtUserName);
			this.accountSettingsPane.Controls.Add(this.labelFullName);
			this.accountSettingsPane.Controls.Add(this.horizontalEdge);
			this.accountSettingsPane.Controls.Add(this.txtIdentityName);
			this.accountSettingsPane.Controls.Add(this.labelIdentityName);
			this.accountSettingsPane.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accountSettingsPane.Dock")));
			this.accountSettingsPane.Enabled = ((bool)(resources.GetObject("accountSettingsPane.Enabled")));
			this.errorProvider.SetError(this.accountSettingsPane, resources.GetString("accountSettingsPane.Error"));
			this.accountSettingsPane.Font = ((System.Drawing.Font)(resources.GetObject("accountSettingsPane.Font")));
			this.accountSettingsPane.HeaderFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.errorProvider.SetIconAlignment(this.accountSettingsPane, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("accountSettingsPane.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.accountSettingsPane, ((int)(resources.GetObject("accountSettingsPane.IconPadding"))));
			this.accountSettingsPane.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accountSettingsPane.ImeMode")));
			this.accountSettingsPane.Location = ((System.Drawing.Point)(resources.GetObject("accountSettingsPane.Location")));
			this.accountSettingsPane.Name = "accountSettingsPane";
			this.accountSettingsPane.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accountSettingsPane.RightToLeft")));
			this.accountSettingsPane.Size = ((System.Drawing.Size)(resources.GetObject("accountSettingsPane.Size")));
			this.accountSettingsPane.TabIndex = ((int)(resources.GetObject("accountSettingsPane.TabIndex")));
			this.accountSettingsPane.Text = resources.GetString("accountSettingsPane.Text");
			this.accountSettingsPane.Visible = ((bool)(resources.GetObject("accountSettingsPane.Visible")));
			this.accountSettingsPane.Resize += new System.EventHandler(this.OnAccountSettingsPaneResize);
			// 
			// txtSignature
			// 
			this.txtSignature.AcceptsReturn = true;
			this.txtSignature.AccessibleDescription = resources.GetString("txtSignature.AccessibleDescription");
			this.txtSignature.AccessibleName = resources.GetString("txtSignature.AccessibleName");
			this.txtSignature.AllowDrop = true;
			this.txtSignature.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtSignature.Anchor")));
			this.txtSignature.AutoSize = ((bool)(resources.GetObject("txtSignature.AutoSize")));
			this.txtSignature.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtSignature.BackgroundImage")));
			this.txtSignature.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtSignature.Dock")));
			this.txtSignature.Enabled = ((bool)(resources.GetObject("txtSignature.Enabled")));
			this.errorProvider.SetError(this.txtSignature, resources.GetString("txtSignature.Error"));
			this.txtSignature.Font = ((System.Drawing.Font)(resources.GetObject("txtSignature.Font")));
			this.errorProvider.SetIconAlignment(this.txtSignature, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtSignature.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtSignature, ((int)(resources.GetObject("txtSignature.IconPadding"))));
			this.txtSignature.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtSignature.ImeMode")));
			this.txtSignature.Location = ((System.Drawing.Point)(resources.GetObject("txtSignature.Location")));
			this.txtSignature.MaxLength = ((int)(resources.GetObject("txtSignature.MaxLength")));
			this.txtSignature.Multiline = ((bool)(resources.GetObject("txtSignature.Multiline")));
			this.txtSignature.Name = "txtSignature";
			this.txtSignature.PasswordChar = ((char)(resources.GetObject("txtSignature.PasswordChar")));
			this.txtSignature.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtSignature.RightToLeft")));
			this.txtSignature.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtSignature.ScrollBars")));
			this.txtSignature.Size = ((System.Drawing.Size)(resources.GetObject("txtSignature.Size")));
			this.txtSignature.TabIndex = ((int)(resources.GetObject("txtSignature.TabIndex")));
			this.txtSignature.Text = resources.GetString("txtSignature.Text");
			this.txtSignature.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtSignature.TextAlign")));
			this.txtSignature.Visible = ((bool)(resources.GetObject("txtSignature.Visible")));
			this.txtSignature.WordWrap = ((bool)(resources.GetObject("txtSignature.WordWrap")));
			this.txtSignature.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtSignature.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelSignature
			// 
			this.labelSignature.AccessibleDescription = resources.GetString("labelSignature.AccessibleDescription");
			this.labelSignature.AccessibleName = resources.GetString("labelSignature.AccessibleName");
			this.labelSignature.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelSignature.Anchor")));
			this.labelSignature.AutoSize = ((bool)(resources.GetObject("labelSignature.AutoSize")));
			this.labelSignature.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelSignature.Dock")));
			this.labelSignature.Enabled = ((bool)(resources.GetObject("labelSignature.Enabled")));
			this.errorProvider.SetError(this.labelSignature, resources.GetString("labelSignature.Error"));
			this.labelSignature.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelSignature.Font = ((System.Drawing.Font)(resources.GetObject("labelSignature.Font")));
			this.errorProvider.SetIconAlignment(this.labelSignature, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelSignature.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelSignature, ((int)(resources.GetObject("labelSignature.IconPadding"))));
			this.labelSignature.Image = ((System.Drawing.Image)(resources.GetObject("labelSignature.Image")));
			this.labelSignature.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelSignature.ImageAlign")));
			this.labelSignature.ImageIndex = ((int)(resources.GetObject("labelSignature.ImageIndex")));
			this.labelSignature.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelSignature.ImeMode")));
			this.labelSignature.Location = ((System.Drawing.Point)(resources.GetObject("labelSignature.Location")));
			this.labelSignature.Name = "labelSignature";
			this.labelSignature.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelSignature.RightToLeft")));
			this.labelSignature.Size = ((System.Drawing.Size)(resources.GetObject("labelSignature.Size")));
			this.labelSignature.TabIndex = ((int)(resources.GetObject("labelSignature.TabIndex")));
			this.labelSignature.Text = resources.GetString("labelSignature.Text");
			this.labelSignature.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelSignature.TextAlign")));
			this.labelSignature.Visible = ((bool)(resources.GetObject("labelSignature.Visible")));
			// 
			// txtUserOrg
			// 
			this.txtUserOrg.AccessibleDescription = resources.GetString("txtUserOrg.AccessibleDescription");
			this.txtUserOrg.AccessibleName = resources.GetString("txtUserOrg.AccessibleName");
			this.txtUserOrg.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtUserOrg.Anchor")));
			this.txtUserOrg.AutoSize = ((bool)(resources.GetObject("txtUserOrg.AutoSize")));
			this.txtUserOrg.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtUserOrg.BackgroundImage")));
			this.txtUserOrg.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtUserOrg.Dock")));
			this.txtUserOrg.Enabled = ((bool)(resources.GetObject("txtUserOrg.Enabled")));
			this.errorProvider.SetError(this.txtUserOrg, resources.GetString("txtUserOrg.Error"));
			this.txtUserOrg.Font = ((System.Drawing.Font)(resources.GetObject("txtUserOrg.Font")));
			this.errorProvider.SetIconAlignment(this.txtUserOrg, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtUserOrg.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtUserOrg, ((int)(resources.GetObject("txtUserOrg.IconPadding"))));
			this.txtUserOrg.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtUserOrg.ImeMode")));
			this.txtUserOrg.Location = ((System.Drawing.Point)(resources.GetObject("txtUserOrg.Location")));
			this.txtUserOrg.MaxLength = ((int)(resources.GetObject("txtUserOrg.MaxLength")));
			this.txtUserOrg.Multiline = ((bool)(resources.GetObject("txtUserOrg.Multiline")));
			this.txtUserOrg.Name = "txtUserOrg";
			this.txtUserOrg.PasswordChar = ((char)(resources.GetObject("txtUserOrg.PasswordChar")));
			this.txtUserOrg.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtUserOrg.RightToLeft")));
			this.txtUserOrg.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtUserOrg.ScrollBars")));
			this.txtUserOrg.Size = ((System.Drawing.Size)(resources.GetObject("txtUserOrg.Size")));
			this.txtUserOrg.TabIndex = ((int)(resources.GetObject("txtUserOrg.TabIndex")));
			this.txtUserOrg.Text = resources.GetString("txtUserOrg.Text");
			this.txtUserOrg.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtUserOrg.TextAlign")));
			this.txtUserOrg.Visible = ((bool)(resources.GetObject("txtUserOrg.Visible")));
			this.txtUserOrg.WordWrap = ((bool)(resources.GetObject("txtUserOrg.WordWrap")));
			this.txtUserOrg.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtUserOrg.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelOrganzation
			// 
			this.labelOrganzation.AccessibleDescription = resources.GetString("labelOrganzation.AccessibleDescription");
			this.labelOrganzation.AccessibleName = resources.GetString("labelOrganzation.AccessibleName");
			this.labelOrganzation.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelOrganzation.Anchor")));
			this.labelOrganzation.AutoSize = ((bool)(resources.GetObject("labelOrganzation.AutoSize")));
			this.labelOrganzation.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelOrganzation.Dock")));
			this.labelOrganzation.Enabled = ((bool)(resources.GetObject("labelOrganzation.Enabled")));
			this.errorProvider.SetError(this.labelOrganzation, resources.GetString("labelOrganzation.Error"));
			this.labelOrganzation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelOrganzation.Font = ((System.Drawing.Font)(resources.GetObject("labelOrganzation.Font")));
			this.errorProvider.SetIconAlignment(this.labelOrganzation, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelOrganzation.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelOrganzation, ((int)(resources.GetObject("labelOrganzation.IconPadding"))));
			this.labelOrganzation.Image = ((System.Drawing.Image)(resources.GetObject("labelOrganzation.Image")));
			this.labelOrganzation.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelOrganzation.ImageAlign")));
			this.labelOrganzation.ImageIndex = ((int)(resources.GetObject("labelOrganzation.ImageIndex")));
			this.labelOrganzation.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelOrganzation.ImeMode")));
			this.labelOrganzation.Location = ((System.Drawing.Point)(resources.GetObject("labelOrganzation.Location")));
			this.labelOrganzation.Name = "labelOrganzation";
			this.labelOrganzation.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelOrganzation.RightToLeft")));
			this.labelOrganzation.Size = ((System.Drawing.Size)(resources.GetObject("labelOrganzation.Size")));
			this.labelOrganzation.TabIndex = ((int)(resources.GetObject("labelOrganzation.TabIndex")));
			this.labelOrganzation.Text = resources.GetString("labelOrganzation.Text");
			this.labelOrganzation.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelOrganzation.TextAlign")));
			this.labelOrganzation.Visible = ((bool)(resources.GetObject("labelOrganzation.Visible")));
			// 
			// txtUserMailResponse
			// 
			this.txtUserMailResponse.AccessibleDescription = resources.GetString("txtUserMailResponse.AccessibleDescription");
			this.txtUserMailResponse.AccessibleName = resources.GetString("txtUserMailResponse.AccessibleName");
			this.txtUserMailResponse.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtUserMailResponse.Anchor")));
			this.txtUserMailResponse.AutoSize = ((bool)(resources.GetObject("txtUserMailResponse.AutoSize")));
			this.txtUserMailResponse.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtUserMailResponse.BackgroundImage")));
			this.txtUserMailResponse.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtUserMailResponse.Dock")));
			this.txtUserMailResponse.Enabled = ((bool)(resources.GetObject("txtUserMailResponse.Enabled")));
			this.errorProvider.SetError(this.txtUserMailResponse, resources.GetString("txtUserMailResponse.Error"));
			this.txtUserMailResponse.Font = ((System.Drawing.Font)(resources.GetObject("txtUserMailResponse.Font")));
			this.errorProvider.SetIconAlignment(this.txtUserMailResponse, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtUserMailResponse.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtUserMailResponse, ((int)(resources.GetObject("txtUserMailResponse.IconPadding"))));
			this.txtUserMailResponse.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtUserMailResponse.ImeMode")));
			this.txtUserMailResponse.Location = ((System.Drawing.Point)(resources.GetObject("txtUserMailResponse.Location")));
			this.txtUserMailResponse.MaxLength = ((int)(resources.GetObject("txtUserMailResponse.MaxLength")));
			this.txtUserMailResponse.Multiline = ((bool)(resources.GetObject("txtUserMailResponse.Multiline")));
			this.txtUserMailResponse.Name = "txtUserMailResponse";
			this.txtUserMailResponse.PasswordChar = ((char)(resources.GetObject("txtUserMailResponse.PasswordChar")));
			this.txtUserMailResponse.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtUserMailResponse.RightToLeft")));
			this.txtUserMailResponse.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtUserMailResponse.ScrollBars")));
			this.txtUserMailResponse.Size = ((System.Drawing.Size)(resources.GetObject("txtUserMailResponse.Size")));
			this.txtUserMailResponse.TabIndex = ((int)(resources.GetObject("txtUserMailResponse.TabIndex")));
			this.txtUserMailResponse.Text = resources.GetString("txtUserMailResponse.Text");
			this.txtUserMailResponse.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtUserMailResponse.TextAlign")));
			this.txtUserMailResponse.Visible = ((bool)(resources.GetObject("txtUserMailResponse.Visible")));
			this.txtUserMailResponse.WordWrap = ((bool)(resources.GetObject("txtUserMailResponse.WordWrap")));
			this.txtUserMailResponse.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtUserMailResponse.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelResponseAddress
			// 
			this.labelResponseAddress.AccessibleDescription = resources.GetString("labelResponseAddress.AccessibleDescription");
			this.labelResponseAddress.AccessibleName = resources.GetString("labelResponseAddress.AccessibleName");
			this.labelResponseAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelResponseAddress.Anchor")));
			this.labelResponseAddress.AutoSize = ((bool)(resources.GetObject("labelResponseAddress.AutoSize")));
			this.labelResponseAddress.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelResponseAddress.Dock")));
			this.labelResponseAddress.Enabled = ((bool)(resources.GetObject("labelResponseAddress.Enabled")));
			this.errorProvider.SetError(this.labelResponseAddress, resources.GetString("labelResponseAddress.Error"));
			this.labelResponseAddress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelResponseAddress.Font = ((System.Drawing.Font)(resources.GetObject("labelResponseAddress.Font")));
			this.errorProvider.SetIconAlignment(this.labelResponseAddress, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelResponseAddress.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelResponseAddress, ((int)(resources.GetObject("labelResponseAddress.IconPadding"))));
			this.labelResponseAddress.Image = ((System.Drawing.Image)(resources.GetObject("labelResponseAddress.Image")));
			this.labelResponseAddress.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelResponseAddress.ImageAlign")));
			this.labelResponseAddress.ImageIndex = ((int)(resources.GetObject("labelResponseAddress.ImageIndex")));
			this.labelResponseAddress.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelResponseAddress.ImeMode")));
			this.labelResponseAddress.Location = ((System.Drawing.Point)(resources.GetObject("labelResponseAddress.Location")));
			this.labelResponseAddress.Name = "labelResponseAddress";
			this.labelResponseAddress.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelResponseAddress.RightToLeft")));
			this.labelResponseAddress.Size = ((System.Drawing.Size)(resources.GetObject("labelResponseAddress.Size")));
			this.labelResponseAddress.TabIndex = ((int)(resources.GetObject("labelResponseAddress.TabIndex")));
			this.labelResponseAddress.Text = resources.GetString("labelResponseAddress.Text");
			this.labelResponseAddress.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelResponseAddress.TextAlign")));
			this.labelResponseAddress.Visible = ((bool)(resources.GetObject("labelResponseAddress.Visible")));
			// 
			// txtRefererUrl
			// 
			this.txtRefererUrl.AccessibleDescription = resources.GetString("txtRefererUrl.AccessibleDescription");
			this.txtRefererUrl.AccessibleName = resources.GetString("txtRefererUrl.AccessibleName");
			this.txtRefererUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtRefererUrl.Anchor")));
			this.txtRefererUrl.AutoSize = ((bool)(resources.GetObject("txtRefererUrl.AutoSize")));
			this.txtRefererUrl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtRefererUrl.BackgroundImage")));
			this.txtRefererUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtRefererUrl.Dock")));
			this.txtRefererUrl.Enabled = ((bool)(resources.GetObject("txtRefererUrl.Enabled")));
			this.errorProvider.SetError(this.txtRefererUrl, resources.GetString("txtRefererUrl.Error"));
			this.txtRefererUrl.Font = ((System.Drawing.Font)(resources.GetObject("txtRefererUrl.Font")));
			this.errorProvider.SetIconAlignment(this.txtRefererUrl, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtRefererUrl.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtRefererUrl, ((int)(resources.GetObject("txtRefererUrl.IconPadding"))));
			this.txtRefererUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtRefererUrl.ImeMode")));
			this.txtRefererUrl.Location = ((System.Drawing.Point)(resources.GetObject("txtRefererUrl.Location")));
			this.txtRefererUrl.MaxLength = ((int)(resources.GetObject("txtRefererUrl.MaxLength")));
			this.txtRefererUrl.Multiline = ((bool)(resources.GetObject("txtRefererUrl.Multiline")));
			this.txtRefererUrl.Name = "txtRefererUrl";
			this.txtRefererUrl.PasswordChar = ((char)(resources.GetObject("txtRefererUrl.PasswordChar")));
			this.txtRefererUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtRefererUrl.RightToLeft")));
			this.txtRefererUrl.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtRefererUrl.ScrollBars")));
			this.txtRefererUrl.Size = ((System.Drawing.Size)(resources.GetObject("txtRefererUrl.Size")));
			this.txtRefererUrl.TabIndex = ((int)(resources.GetObject("txtRefererUrl.TabIndex")));
			this.txtRefererUrl.Text = resources.GetString("txtRefererUrl.Text");
			this.txtRefererUrl.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtRefererUrl.TextAlign")));
			this.txtRefererUrl.Visible = ((bool)(resources.GetObject("txtRefererUrl.Visible")));
			this.txtRefererUrl.WordWrap = ((bool)(resources.GetObject("txtRefererUrl.WordWrap")));
			this.txtRefererUrl.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtRefererUrl.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelRefererUrl
			// 
			this.labelRefererUrl.AccessibleDescription = resources.GetString("labelRefererUrl.AccessibleDescription");
			this.labelRefererUrl.AccessibleName = resources.GetString("labelRefererUrl.AccessibleName");
			this.labelRefererUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelRefererUrl.Anchor")));
			this.labelRefererUrl.AutoSize = ((bool)(resources.GetObject("labelRefererUrl.AutoSize")));
			this.labelRefererUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelRefererUrl.Dock")));
			this.labelRefererUrl.Enabled = ((bool)(resources.GetObject("labelRefererUrl.Enabled")));
			this.errorProvider.SetError(this.labelRefererUrl, resources.GetString("labelRefererUrl.Error"));
			this.labelRefererUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelRefererUrl.Font = ((System.Drawing.Font)(resources.GetObject("labelRefererUrl.Font")));
			this.errorProvider.SetIconAlignment(this.labelRefererUrl, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelRefererUrl.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelRefererUrl, ((int)(resources.GetObject("labelRefererUrl.IconPadding"))));
			this.labelRefererUrl.Image = ((System.Drawing.Image)(resources.GetObject("labelRefererUrl.Image")));
			this.labelRefererUrl.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRefererUrl.ImageAlign")));
			this.labelRefererUrl.ImageIndex = ((int)(resources.GetObject("labelRefererUrl.ImageIndex")));
			this.labelRefererUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelRefererUrl.ImeMode")));
			this.labelRefererUrl.Location = ((System.Drawing.Point)(resources.GetObject("labelRefererUrl.Location")));
			this.labelRefererUrl.Name = "labelRefererUrl";
			this.labelRefererUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelRefererUrl.RightToLeft")));
			this.labelRefererUrl.Size = ((System.Drawing.Size)(resources.GetObject("labelRefererUrl.Size")));
			this.labelRefererUrl.TabIndex = ((int)(resources.GetObject("labelRefererUrl.TabIndex")));
			this.labelRefererUrl.Text = resources.GetString("labelRefererUrl.Text");
			this.labelRefererUrl.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelRefererUrl.TextAlign")));
			this.labelRefererUrl.Visible = ((bool)(resources.GetObject("labelRefererUrl.Visible")));
			// 
			// txtUserMail
			// 
			this.txtUserMail.AccessibleDescription = resources.GetString("txtUserMail.AccessibleDescription");
			this.txtUserMail.AccessibleName = resources.GetString("txtUserMail.AccessibleName");
			this.txtUserMail.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtUserMail.Anchor")));
			this.txtUserMail.AutoSize = ((bool)(resources.GetObject("txtUserMail.AutoSize")));
			this.txtUserMail.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtUserMail.BackgroundImage")));
			this.txtUserMail.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtUserMail.Dock")));
			this.txtUserMail.Enabled = ((bool)(resources.GetObject("txtUserMail.Enabled")));
			this.errorProvider.SetError(this.txtUserMail, resources.GetString("txtUserMail.Error"));
			this.txtUserMail.Font = ((System.Drawing.Font)(resources.GetObject("txtUserMail.Font")));
			this.errorProvider.SetIconAlignment(this.txtUserMail, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtUserMail.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtUserMail, ((int)(resources.GetObject("txtUserMail.IconPadding"))));
			this.txtUserMail.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtUserMail.ImeMode")));
			this.txtUserMail.Location = ((System.Drawing.Point)(resources.GetObject("txtUserMail.Location")));
			this.txtUserMail.MaxLength = ((int)(resources.GetObject("txtUserMail.MaxLength")));
			this.txtUserMail.Multiline = ((bool)(resources.GetObject("txtUserMail.Multiline")));
			this.txtUserMail.Name = "txtUserMail";
			this.txtUserMail.PasswordChar = ((char)(resources.GetObject("txtUserMail.PasswordChar")));
			this.txtUserMail.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtUserMail.RightToLeft")));
			this.txtUserMail.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtUserMail.ScrollBars")));
			this.txtUserMail.Size = ((System.Drawing.Size)(resources.GetObject("txtUserMail.Size")));
			this.txtUserMail.TabIndex = ((int)(resources.GetObject("txtUserMail.TabIndex")));
			this.txtUserMail.Text = resources.GetString("txtUserMail.Text");
			this.txtUserMail.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtUserMail.TextAlign")));
			this.txtUserMail.Visible = ((bool)(resources.GetObject("txtUserMail.Visible")));
			this.txtUserMail.WordWrap = ((bool)(resources.GetObject("txtUserMail.WordWrap")));
			this.txtUserMail.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtUserMail.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelMailAddress
			// 
			this.labelMailAddress.AccessibleDescription = resources.GetString("labelMailAddress.AccessibleDescription");
			this.labelMailAddress.AccessibleName = resources.GetString("labelMailAddress.AccessibleName");
			this.labelMailAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelMailAddress.Anchor")));
			this.labelMailAddress.AutoSize = ((bool)(resources.GetObject("labelMailAddress.AutoSize")));
			this.labelMailAddress.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelMailAddress.Dock")));
			this.labelMailAddress.Enabled = ((bool)(resources.GetObject("labelMailAddress.Enabled")));
			this.errorProvider.SetError(this.labelMailAddress, resources.GetString("labelMailAddress.Error"));
			this.labelMailAddress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelMailAddress.Font = ((System.Drawing.Font)(resources.GetObject("labelMailAddress.Font")));
			this.errorProvider.SetIconAlignment(this.labelMailAddress, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelMailAddress.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelMailAddress, ((int)(resources.GetObject("labelMailAddress.IconPadding"))));
			this.labelMailAddress.Image = ((System.Drawing.Image)(resources.GetObject("labelMailAddress.Image")));
			this.labelMailAddress.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelMailAddress.ImageAlign")));
			this.labelMailAddress.ImageIndex = ((int)(resources.GetObject("labelMailAddress.ImageIndex")));
			this.labelMailAddress.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelMailAddress.ImeMode")));
			this.labelMailAddress.Location = ((System.Drawing.Point)(resources.GetObject("labelMailAddress.Location")));
			this.labelMailAddress.Name = "labelMailAddress";
			this.labelMailAddress.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelMailAddress.RightToLeft")));
			this.labelMailAddress.Size = ((System.Drawing.Size)(resources.GetObject("labelMailAddress.Size")));
			this.labelMailAddress.TabIndex = ((int)(resources.GetObject("labelMailAddress.TabIndex")));
			this.labelMailAddress.Text = resources.GetString("labelMailAddress.Text");
			this.labelMailAddress.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelMailAddress.TextAlign")));
			this.labelMailAddress.Visible = ((bool)(resources.GetObject("labelMailAddress.Visible")));
			// 
			// txtUserName
			// 
			this.txtUserName.AccessibleDescription = resources.GetString("txtUserName.AccessibleDescription");
			this.txtUserName.AccessibleName = resources.GetString("txtUserName.AccessibleName");
			this.txtUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtUserName.Anchor")));
			this.txtUserName.AutoSize = ((bool)(resources.GetObject("txtUserName.AutoSize")));
			this.txtUserName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtUserName.BackgroundImage")));
			this.txtUserName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtUserName.Dock")));
			this.txtUserName.Enabled = ((bool)(resources.GetObject("txtUserName.Enabled")));
			this.errorProvider.SetError(this.txtUserName, resources.GetString("txtUserName.Error"));
			this.txtUserName.Font = ((System.Drawing.Font)(resources.GetObject("txtUserName.Font")));
			this.errorProvider.SetIconAlignment(this.txtUserName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtUserName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtUserName, ((int)(resources.GetObject("txtUserName.IconPadding"))));
			this.txtUserName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtUserName.ImeMode")));
			this.txtUserName.Location = ((System.Drawing.Point)(resources.GetObject("txtUserName.Location")));
			this.txtUserName.MaxLength = ((int)(resources.GetObject("txtUserName.MaxLength")));
			this.txtUserName.Multiline = ((bool)(resources.GetObject("txtUserName.Multiline")));
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.PasswordChar = ((char)(resources.GetObject("txtUserName.PasswordChar")));
			this.txtUserName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtUserName.RightToLeft")));
			this.txtUserName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtUserName.ScrollBars")));
			this.txtUserName.Size = ((System.Drawing.Size)(resources.GetObject("txtUserName.Size")));
			this.txtUserName.TabIndex = ((int)(resources.GetObject("txtUserName.TabIndex")));
			this.txtUserName.Text = resources.GetString("txtUserName.Text");
			this.txtUserName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtUserName.TextAlign")));
			this.txtUserName.Visible = ((bool)(resources.GetObject("txtUserName.Visible")));
			this.txtUserName.WordWrap = ((bool)(resources.GetObject("txtUserName.WordWrap")));
			this.txtUserName.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtUserName.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelFullName
			// 
			this.labelFullName.AccessibleDescription = resources.GetString("labelFullName.AccessibleDescription");
			this.labelFullName.AccessibleName = resources.GetString("labelFullName.AccessibleName");
			this.labelFullName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelFullName.Anchor")));
			this.labelFullName.AutoSize = ((bool)(resources.GetObject("labelFullName.AutoSize")));
			this.labelFullName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelFullName.Dock")));
			this.labelFullName.Enabled = ((bool)(resources.GetObject("labelFullName.Enabled")));
			this.errorProvider.SetError(this.labelFullName, resources.GetString("labelFullName.Error"));
			this.labelFullName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelFullName.Font = ((System.Drawing.Font)(resources.GetObject("labelFullName.Font")));
			this.errorProvider.SetIconAlignment(this.labelFullName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelFullName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelFullName, ((int)(resources.GetObject("labelFullName.IconPadding"))));
			this.labelFullName.Image = ((System.Drawing.Image)(resources.GetObject("labelFullName.Image")));
			this.labelFullName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFullName.ImageAlign")));
			this.labelFullName.ImageIndex = ((int)(resources.GetObject("labelFullName.ImageIndex")));
			this.labelFullName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelFullName.ImeMode")));
			this.labelFullName.Location = ((System.Drawing.Point)(resources.GetObject("labelFullName.Location")));
			this.labelFullName.Name = "labelFullName";
			this.labelFullName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelFullName.RightToLeft")));
			this.labelFullName.Size = ((System.Drawing.Size)(resources.GetObject("labelFullName.Size")));
			this.labelFullName.TabIndex = ((int)(resources.GetObject("labelFullName.TabIndex")));
			this.labelFullName.Text = resources.GetString("labelFullName.Text");
			this.labelFullName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelFullName.TextAlign")));
			this.labelFullName.Visible = ((bool)(resources.GetObject("labelFullName.Visible")));
			// 
			// horizontalEdge
			// 
			this.horizontalEdge.AccessibleDescription = resources.GetString("horizontalEdge.AccessibleDescription");
			this.horizontalEdge.AccessibleName = resources.GetString("horizontalEdge.AccessibleName");
			this.horizontalEdge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("horizontalEdge.Anchor")));
			this.horizontalEdge.AutoSize = ((bool)(resources.GetObject("horizontalEdge.AutoSize")));
			this.horizontalEdge.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.horizontalEdge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("horizontalEdge.Dock")));
			this.horizontalEdge.Enabled = ((bool)(resources.GetObject("horizontalEdge.Enabled")));
			this.errorProvider.SetError(this.horizontalEdge, resources.GetString("horizontalEdge.Error"));
			this.horizontalEdge.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.horizontalEdge.Font = ((System.Drawing.Font)(resources.GetObject("horizontalEdge.Font")));
			this.errorProvider.SetIconAlignment(this.horizontalEdge, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("horizontalEdge.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.horizontalEdge, ((int)(resources.GetObject("horizontalEdge.IconPadding"))));
			this.horizontalEdge.Image = ((System.Drawing.Image)(resources.GetObject("horizontalEdge.Image")));
			this.horizontalEdge.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.ImageAlign")));
			this.horizontalEdge.ImageIndex = ((int)(resources.GetObject("horizontalEdge.ImageIndex")));
			this.horizontalEdge.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("horizontalEdge.ImeMode")));
			this.horizontalEdge.Location = ((System.Drawing.Point)(resources.GetObject("horizontalEdge.Location")));
			this.horizontalEdge.Name = "horizontalEdge";
			this.horizontalEdge.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("horizontalEdge.RightToLeft")));
			this.horizontalEdge.Size = ((System.Drawing.Size)(resources.GetObject("horizontalEdge.Size")));
			this.horizontalEdge.TabIndex = ((int)(resources.GetObject("horizontalEdge.TabIndex")));
			this.horizontalEdge.Text = resources.GetString("horizontalEdge.Text");
			this.horizontalEdge.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.TextAlign")));
			this.horizontalEdge.Visible = ((bool)(resources.GetObject("horizontalEdge.Visible")));
			// 
			// txtIdentityName
			// 
			this.txtIdentityName.AccessibleDescription = resources.GetString("txtIdentityName.AccessibleDescription");
			this.txtIdentityName.AccessibleName = resources.GetString("txtIdentityName.AccessibleName");
			this.txtIdentityName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtIdentityName.Anchor")));
			this.txtIdentityName.AutoSize = ((bool)(resources.GetObject("txtIdentityName.AutoSize")));
			this.txtIdentityName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtIdentityName.BackgroundImage")));
			this.txtIdentityName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtIdentityName.Dock")));
			this.txtIdentityName.Enabled = ((bool)(resources.GetObject("txtIdentityName.Enabled")));
			this.errorProvider.SetError(this.txtIdentityName, resources.GetString("txtIdentityName.Error"));
			this.txtIdentityName.Font = ((System.Drawing.Font)(resources.GetObject("txtIdentityName.Font")));
			this.errorProvider.SetIconAlignment(this.txtIdentityName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtIdentityName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtIdentityName, ((int)(resources.GetObject("txtIdentityName.IconPadding"))));
			this.txtIdentityName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtIdentityName.ImeMode")));
			this.txtIdentityName.Location = ((System.Drawing.Point)(resources.GetObject("txtIdentityName.Location")));
			this.txtIdentityName.MaxLength = ((int)(resources.GetObject("txtIdentityName.MaxLength")));
			this.txtIdentityName.Multiline = ((bool)(resources.GetObject("txtIdentityName.Multiline")));
			this.txtIdentityName.Name = "txtIdentityName";
			this.txtIdentityName.PasswordChar = ((char)(resources.GetObject("txtIdentityName.PasswordChar")));
			this.txtIdentityName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtIdentityName.RightToLeft")));
			this.txtIdentityName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtIdentityName.ScrollBars")));
			this.txtIdentityName.Size = ((System.Drawing.Size)(resources.GetObject("txtIdentityName.Size")));
			this.txtIdentityName.TabIndex = ((int)(resources.GetObject("txtIdentityName.TabIndex")));
			this.txtIdentityName.Text = resources.GetString("txtIdentityName.Text");
			this.txtIdentityName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtIdentityName.TextAlign")));
			this.txtIdentityName.Visible = ((bool)(resources.GetObject("txtIdentityName.Visible")));
			this.txtIdentityName.WordWrap = ((bool)(resources.GetObject("txtIdentityName.WordWrap")));
			this.txtIdentityName.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtIdentityName.Validated += new System.EventHandler(this.OnWidgetValidated);
			this.txtIdentityName.TextChanged += new System.EventHandler(this.OnIdentityNameTextChanged);
			// 
			// labelIdentityName
			// 
			this.labelIdentityName.AccessibleDescription = resources.GetString("labelIdentityName.AccessibleDescription");
			this.labelIdentityName.AccessibleName = resources.GetString("labelIdentityName.AccessibleName");
			this.labelIdentityName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelIdentityName.Anchor")));
			this.labelIdentityName.AutoSize = ((bool)(resources.GetObject("labelIdentityName.AutoSize")));
			this.labelIdentityName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelIdentityName.Dock")));
			this.labelIdentityName.Enabled = ((bool)(resources.GetObject("labelIdentityName.Enabled")));
			this.errorProvider.SetError(this.labelIdentityName, resources.GetString("labelIdentityName.Error"));
			this.labelIdentityName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelIdentityName.Font = ((System.Drawing.Font)(resources.GetObject("labelIdentityName.Font")));
			this.errorProvider.SetIconAlignment(this.labelIdentityName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelIdentityName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelIdentityName, ((int)(resources.GetObject("labelIdentityName.IconPadding"))));
			this.labelIdentityName.Image = ((System.Drawing.Image)(resources.GetObject("labelIdentityName.Image")));
			this.labelIdentityName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelIdentityName.ImageAlign")));
			this.labelIdentityName.ImageIndex = ((int)(resources.GetObject("labelIdentityName.ImageIndex")));
			this.labelIdentityName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelIdentityName.ImeMode")));
			this.labelIdentityName.Location = ((System.Drawing.Point)(resources.GetObject("labelIdentityName.Location")));
			this.labelIdentityName.Name = "labelIdentityName";
			this.labelIdentityName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelIdentityName.RightToLeft")));
			this.labelIdentityName.Size = ((System.Drawing.Size)(resources.GetObject("labelIdentityName.Size")));
			this.labelIdentityName.TabIndex = ((int)(resources.GetObject("labelIdentityName.TabIndex")));
			this.labelIdentityName.Text = resources.GetString("labelIdentityName.Text");
			this.labelIdentityName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelIdentityName.TextAlign")));
			this.labelIdentityName.Visible = ((bool)(resources.GetObject("labelIdentityName.Visible")));
			// 
			// splitter1
			// 
			this.splitter1.AccessibleDescription = resources.GetString("splitter1.AccessibleDescription");
			this.splitter1.AccessibleName = resources.GetString("splitter1.AccessibleName");
			this.splitter1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("splitter1.Anchor")));
			this.splitter1.BackColor = System.Drawing.SystemColors.Control;
			this.splitter1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("splitter1.BackgroundImage")));
			this.splitter1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("splitter1.Dock")));
			this.splitter1.Enabled = ((bool)(resources.GetObject("splitter1.Enabled")));
			this.errorProvider.SetError(this.splitter1, resources.GetString("splitter1.Error"));
			this.splitter1.Font = ((System.Drawing.Font)(resources.GetObject("splitter1.Font")));
			this.errorProvider.SetIconAlignment(this.splitter1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("splitter1.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.splitter1, ((int)(resources.GetObject("splitter1.IconPadding"))));
			this.splitter1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("splitter1.ImeMode")));
			this.splitter1.Location = ((System.Drawing.Point)(resources.GetObject("splitter1.Location")));
			this.splitter1.MinExtra = ((int)(resources.GetObject("splitter1.MinExtra")));
			this.splitter1.MinSize = ((int)(resources.GetObject("splitter1.MinSize")));
			this.splitter1.Name = "splitter1";
			this.splitter1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("splitter1.RightToLeft")));
			this.splitter1.Size = ((System.Drawing.Size)(resources.GetObject("splitter1.Size")));
			this.splitter1.TabIndex = ((int)(resources.GetObject("splitter1.TabIndex")));
			this.splitter1.TabStop = false;
			this.splitter1.Visible = ((bool)(resources.GetObject("splitter1.Visible")));
			// 
			// panelDetailsParent
			// 
			this.panelDetailsParent.AccessibleDescription = resources.GetString("panelDetailsParent.AccessibleDescription");
			this.panelDetailsParent.AccessibleName = resources.GetString("panelDetailsParent.AccessibleName");
			this.panelDetailsParent.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelDetailsParent.Anchor")));
			this.panelDetailsParent.AutoScroll = ((bool)(resources.GetObject("panelDetailsParent.AutoScroll")));
			this.panelDetailsParent.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelDetailsParent.AutoScrollMargin")));
			this.panelDetailsParent.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelDetailsParent.AutoScrollMinSize")));
			this.panelDetailsParent.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelDetailsParent.BackgroundImage")));
			this.panelDetailsParent.Controls.Add(this.btnApply);
			this.panelDetailsParent.Controls.Add(this.panelDetailsTop);
			this.panelDetailsParent.Controls.Add(this.btnCancel);
			this.panelDetailsParent.Controls.Add(this.btnOK);
			this.panelDetailsParent.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelDetailsParent.Dock")));
			this.panelDetailsParent.Enabled = ((bool)(resources.GetObject("panelDetailsParent.Enabled")));
			this.errorProvider.SetError(this.panelDetailsParent, resources.GetString("panelDetailsParent.Error"));
			this.panelDetailsParent.Font = ((System.Drawing.Font)(resources.GetObject("panelDetailsParent.Font")));
			this.errorProvider.SetIconAlignment(this.panelDetailsParent, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("panelDetailsParent.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.panelDetailsParent, ((int)(resources.GetObject("panelDetailsParent.IconPadding"))));
			this.panelDetailsParent.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelDetailsParent.ImeMode")));
			this.panelDetailsParent.Location = ((System.Drawing.Point)(resources.GetObject("panelDetailsParent.Location")));
			this.panelDetailsParent.Name = "panelDetailsParent";
			this.panelDetailsParent.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelDetailsParent.RightToLeft")));
			this.panelDetailsParent.Size = ((System.Drawing.Size)(resources.GetObject("panelDetailsParent.Size")));
			this.panelDetailsParent.TabIndex = ((int)(resources.GetObject("panelDetailsParent.TabIndex")));
			this.panelDetailsParent.Text = resources.GetString("panelDetailsParent.Text");
			this.panelDetailsParent.Visible = ((bool)(resources.GetObject("panelDetailsParent.Visible")));
			// 
			// btnApply
			// 
			this.btnApply.AccessibleDescription = resources.GetString("btnApply.AccessibleDescription");
			this.btnApply.AccessibleName = resources.GetString("btnApply.AccessibleName");
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnApply.Anchor")));
			this.btnApply.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnApply.BackgroundImage")));
			this.btnApply.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnApply.Dock")));
			this.btnApply.Enabled = ((bool)(resources.GetObject("btnApply.Enabled")));
			this.errorProvider.SetError(this.btnApply, resources.GetString("btnApply.Error"));
			this.btnApply.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnApply.FlatStyle")));
			this.btnApply.Font = ((System.Drawing.Font)(resources.GetObject("btnApply.Font")));
			this.errorProvider.SetIconAlignment(this.btnApply, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnApply.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.btnApply, ((int)(resources.GetObject("btnApply.IconPadding"))));
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
			this.btnApply.Visible = ((bool)(resources.GetObject("btnApply.Visible")));
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// panelDetailsTop
			// 
			this.panelDetailsTop.AccessibleDescription = resources.GetString("panelDetailsTop.AccessibleDescription");
			this.panelDetailsTop.AccessibleName = resources.GetString("panelDetailsTop.AccessibleName");
			this.panelDetailsTop.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelDetailsTop.Anchor")));
			this.panelDetailsTop.AutoScroll = ((bool)(resources.GetObject("panelDetailsTop.AutoScroll")));
			this.panelDetailsTop.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelDetailsTop.AutoScrollMargin")));
			this.panelDetailsTop.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelDetailsTop.AutoScrollMinSize")));
			this.panelDetailsTop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelDetailsTop.BackgroundImage")));
			this.panelDetailsTop.Controls.Add(this.subscriptionsViewPane);
			this.panelDetailsTop.Controls.Add(this.accountSettingsPane);
			this.panelDetailsTop.Controls.Add(this.advancedViewPane);
			this.panelDetailsTop.Controls.Add(this.serverViewPane);
			this.panelDetailsTop.Controls.Add(this.generalViewPane);
			this.panelDetailsTop.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelDetailsTop.Dock")));
			this.panelDetailsTop.Enabled = ((bool)(resources.GetObject("panelDetailsTop.Enabled")));
			this.errorProvider.SetError(this.panelDetailsTop, resources.GetString("panelDetailsTop.Error"));
			this.panelDetailsTop.Font = ((System.Drawing.Font)(resources.GetObject("panelDetailsTop.Font")));
			this.errorProvider.SetIconAlignment(this.panelDetailsTop, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("panelDetailsTop.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.panelDetailsTop, ((int)(resources.GetObject("panelDetailsTop.IconPadding"))));
			this.panelDetailsTop.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelDetailsTop.ImeMode")));
			this.panelDetailsTop.Location = ((System.Drawing.Point)(resources.GetObject("panelDetailsTop.Location")));
			this.panelDetailsTop.Name = "panelDetailsTop";
			this.panelDetailsTop.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelDetailsTop.RightToLeft")));
			this.panelDetailsTop.Size = ((System.Drawing.Size)(resources.GetObject("panelDetailsTop.Size")));
			this.panelDetailsTop.TabIndex = ((int)(resources.GetObject("panelDetailsTop.TabIndex")));
			this.panelDetailsTop.Text = resources.GetString("panelDetailsTop.Text");
			this.panelDetailsTop.Visible = ((bool)(resources.GetObject("panelDetailsTop.Visible")));
			// 
			// subscriptionsViewPane
			// 
			this.subscriptionsViewPane.AccessibleDescription = resources.GetString("subscriptionsViewPane.AccessibleDescription");
			this.subscriptionsViewPane.AccessibleName = resources.GetString("subscriptionsViewPane.AccessibleName");
			this.subscriptionsViewPane.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("subscriptionsViewPane.Anchor")));
			this.subscriptionsViewPane.AutoScroll = ((bool)(resources.GetObject("subscriptionsViewPane.AutoScroll")));
			this.subscriptionsViewPane.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("subscriptionsViewPane.AutoScrollMargin")));
			this.subscriptionsViewPane.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("subscriptionsViewPane.AutoScrollMinSize")));
			this.subscriptionsViewPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("subscriptionsViewPane.BackgroundImage")));
			this.subscriptionsViewPane.Controls.Add(this.txtFilterBy);
			this.subscriptionsViewPane.Controls.Add(this.labelNewsgroupsFilter);
			this.subscriptionsViewPane.Controls.Add(this.listOfGroups);
			this.subscriptionsViewPane.Controls.Add(this.btnRefreshGroupList);
			this.subscriptionsViewPane.Controls.Add(this.btnUnsubscribe);
			this.subscriptionsViewPane.Controls.Add(this.btnSubscribe);
			this.subscriptionsViewPane.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("subscriptionsViewPane.Dock")));
			this.subscriptionsViewPane.Enabled = ((bool)(resources.GetObject("subscriptionsViewPane.Enabled")));
			this.errorProvider.SetError(this.subscriptionsViewPane, resources.GetString("subscriptionsViewPane.Error"));
			this.subscriptionsViewPane.Font = ((System.Drawing.Font)(resources.GetObject("subscriptionsViewPane.Font")));
			this.subscriptionsViewPane.HeaderFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.errorProvider.SetIconAlignment(this.subscriptionsViewPane, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("subscriptionsViewPane.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.subscriptionsViewPane, ((int)(resources.GetObject("subscriptionsViewPane.IconPadding"))));
			this.subscriptionsViewPane.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("subscriptionsViewPane.ImeMode")));
			this.subscriptionsViewPane.Location = ((System.Drawing.Point)(resources.GetObject("subscriptionsViewPane.Location")));
			this.subscriptionsViewPane.Name = "subscriptionsViewPane";
			this.subscriptionsViewPane.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("subscriptionsViewPane.RightToLeft")));
			this.subscriptionsViewPane.Size = ((System.Drawing.Size)(resources.GetObject("subscriptionsViewPane.Size")));
			this.subscriptionsViewPane.TabIndex = ((int)(resources.GetObject("subscriptionsViewPane.TabIndex")));
			this.subscriptionsViewPane.Text = resources.GetString("subscriptionsViewPane.Text");
			this.subscriptionsViewPane.Visible = ((bool)(resources.GetObject("subscriptionsViewPane.Visible")));
			this.subscriptionsViewPane.Resize += new System.EventHandler(this.OnSubscriptionsPaneResize);
			// 
			// txtFilterBy
			// 
			this.txtFilterBy.AccessibleDescription = resources.GetString("txtFilterBy.AccessibleDescription");
			this.txtFilterBy.AccessibleName = resources.GetString("txtFilterBy.AccessibleName");
			this.txtFilterBy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtFilterBy.Anchor")));
			this.txtFilterBy.AutoSize = ((bool)(resources.GetObject("txtFilterBy.AutoSize")));
			this.txtFilterBy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtFilterBy.BackgroundImage")));
			this.txtFilterBy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtFilterBy.Dock")));
			this.txtFilterBy.Enabled = ((bool)(resources.GetObject("txtFilterBy.Enabled")));
			this.errorProvider.SetError(this.txtFilterBy, resources.GetString("txtFilterBy.Error"));
			this.txtFilterBy.Font = ((System.Drawing.Font)(resources.GetObject("txtFilterBy.Font")));
			this.errorProvider.SetIconAlignment(this.txtFilterBy, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtFilterBy.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtFilterBy, ((int)(resources.GetObject("txtFilterBy.IconPadding"))));
			this.txtFilterBy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtFilterBy.ImeMode")));
			this.txtFilterBy.Location = ((System.Drawing.Point)(resources.GetObject("txtFilterBy.Location")));
			this.txtFilterBy.MaxLength = ((int)(resources.GetObject("txtFilterBy.MaxLength")));
			this.txtFilterBy.Multiline = ((bool)(resources.GetObject("txtFilterBy.Multiline")));
			this.txtFilterBy.Name = "txtFilterBy";
			this.txtFilterBy.PasswordChar = ((char)(resources.GetObject("txtFilterBy.PasswordChar")));
			this.txtFilterBy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtFilterBy.RightToLeft")));
			this.txtFilterBy.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtFilterBy.ScrollBars")));
			this.txtFilterBy.Size = ((System.Drawing.Size)(resources.GetObject("txtFilterBy.Size")));
			this.txtFilterBy.TabIndex = ((int)(resources.GetObject("txtFilterBy.TabIndex")));
			this.txtFilterBy.Text = resources.GetString("txtFilterBy.Text");
			this.txtFilterBy.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtFilterBy.TextAlign")));
			this.txtFilterBy.Visible = ((bool)(resources.GetObject("txtFilterBy.Visible")));
			this.txtFilterBy.WordWrap = ((bool)(resources.GetObject("txtFilterBy.WordWrap")));
			this.txtFilterBy.TextChanged += new System.EventHandler(this.OnFilterByTextChanged);
			// 
			// labelNewsgroupsFilter
			// 
			this.labelNewsgroupsFilter.AccessibleDescription = resources.GetString("labelNewsgroupsFilter.AccessibleDescription");
			this.labelNewsgroupsFilter.AccessibleName = resources.GetString("labelNewsgroupsFilter.AccessibleName");
			this.labelNewsgroupsFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelNewsgroupsFilter.Anchor")));
			this.labelNewsgroupsFilter.AutoSize = ((bool)(resources.GetObject("labelNewsgroupsFilter.AutoSize")));
			this.labelNewsgroupsFilter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelNewsgroupsFilter.Dock")));
			this.labelNewsgroupsFilter.Enabled = ((bool)(resources.GetObject("labelNewsgroupsFilter.Enabled")));
			this.errorProvider.SetError(this.labelNewsgroupsFilter, resources.GetString("labelNewsgroupsFilter.Error"));
			this.labelNewsgroupsFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelNewsgroupsFilter.Font = ((System.Drawing.Font)(resources.GetObject("labelNewsgroupsFilter.Font")));
			this.errorProvider.SetIconAlignment(this.labelNewsgroupsFilter, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelNewsgroupsFilter.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelNewsgroupsFilter, ((int)(resources.GetObject("labelNewsgroupsFilter.IconPadding"))));
			this.labelNewsgroupsFilter.Image = ((System.Drawing.Image)(resources.GetObject("labelNewsgroupsFilter.Image")));
			this.labelNewsgroupsFilter.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsgroupsFilter.ImageAlign")));
			this.labelNewsgroupsFilter.ImageIndex = ((int)(resources.GetObject("labelNewsgroupsFilter.ImageIndex")));
			this.labelNewsgroupsFilter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelNewsgroupsFilter.ImeMode")));
			this.labelNewsgroupsFilter.Location = ((System.Drawing.Point)(resources.GetObject("labelNewsgroupsFilter.Location")));
			this.labelNewsgroupsFilter.Name = "labelNewsgroupsFilter";
			this.labelNewsgroupsFilter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelNewsgroupsFilter.RightToLeft")));
			this.labelNewsgroupsFilter.Size = ((System.Drawing.Size)(resources.GetObject("labelNewsgroupsFilter.Size")));
			this.labelNewsgroupsFilter.TabIndex = ((int)(resources.GetObject("labelNewsgroupsFilter.TabIndex")));
			this.labelNewsgroupsFilter.Text = resources.GetString("labelNewsgroupsFilter.Text");
			this.labelNewsgroupsFilter.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsgroupsFilter.TextAlign")));
			this.labelNewsgroupsFilter.Visible = ((bool)(resources.GetObject("labelNewsgroupsFilter.Visible")));
			// 
			// listOfGroups
			// 
			this.listOfGroups.AccessibleDescription = resources.GetString("listOfGroups.AccessibleDescription");
			this.listOfGroups.AccessibleName = resources.GetString("listOfGroups.AccessibleName");
			this.listOfGroups.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listOfGroups.Alignment")));
			this.listOfGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listOfGroups.Anchor")));
			this.listOfGroups.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listOfGroups.BackgroundImage")));
			this.listOfGroups.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listOfGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						   this.colGroupName,
																						   this.colDescription});
			this.listOfGroups.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listOfGroups.Dock")));
			this.listOfGroups.Enabled = ((bool)(resources.GetObject("listOfGroups.Enabled")));
			this.errorProvider.SetError(this.listOfGroups, resources.GetString("listOfGroups.Error"));
			this.listOfGroups.Font = ((System.Drawing.Font)(resources.GetObject("listOfGroups.Font")));
			this.listOfGroups.FullRowSelect = true;
			this.listOfGroups.HideSelection = false;
			this.errorProvider.SetIconAlignment(this.listOfGroups, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("listOfGroups.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.listOfGroups, ((int)(resources.GetObject("listOfGroups.IconPadding"))));
			this.listOfGroups.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listOfGroups.ImeMode")));
			this.listOfGroups.LabelWrap = ((bool)(resources.GetObject("listOfGroups.LabelWrap")));
			this.listOfGroups.Location = ((System.Drawing.Point)(resources.GetObject("listOfGroups.Location")));
			this.listOfGroups.MultiSelect = false;
			this.listOfGroups.Name = "listOfGroups";
			this.listOfGroups.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listOfGroups.RightToLeft")));
			this.listOfGroups.Size = ((System.Drawing.Size)(resources.GetObject("listOfGroups.Size")));
			this.listOfGroups.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listOfGroups.TabIndex = ((int)(resources.GetObject("listOfGroups.TabIndex")));
			this.listOfGroups.Text = resources.GetString("listOfGroups.Text");
			this.listOfGroups.View = System.Windows.Forms.View.Details;
			this.listOfGroups.Visible = ((bool)(resources.GetObject("listOfGroups.Visible")));
			this.listOfGroups.DoubleClick += new System.EventHandler(this.OnListOfGroupsDoubleClick);
			// 
			// colGroupName
			// 
			this.colGroupName.Text = resources.GetString("colGroupName.Text");
			this.colGroupName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("colGroupName.TextAlign")));
			this.colGroupName.Width = ((int)(resources.GetObject("colGroupName.Width")));
			// 
			// colDescription
			// 
			this.colDescription.Text = resources.GetString("colDescription.Text");
			this.colDescription.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("colDescription.TextAlign")));
			this.colDescription.Width = ((int)(resources.GetObject("colDescription.Width")));
			// 
			// btnRefreshGroupList
			// 
			this.btnRefreshGroupList.AccessibleDescription = resources.GetString("btnRefreshGroupList.AccessibleDescription");
			this.btnRefreshGroupList.AccessibleName = resources.GetString("btnRefreshGroupList.AccessibleName");
			this.btnRefreshGroupList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnRefreshGroupList.Anchor")));
			this.btnRefreshGroupList.BackColor = System.Drawing.SystemColors.Control;
			this.btnRefreshGroupList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRefreshGroupList.BackgroundImage")));
			this.btnRefreshGroupList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnRefreshGroupList.Dock")));
			this.btnRefreshGroupList.Enabled = ((bool)(resources.GetObject("btnRefreshGroupList.Enabled")));
			this.errorProvider.SetError(this.btnRefreshGroupList, resources.GetString("btnRefreshGroupList.Error"));
			this.btnRefreshGroupList.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnRefreshGroupList.FlatStyle")));
			this.btnRefreshGroupList.Font = ((System.Drawing.Font)(resources.GetObject("btnRefreshGroupList.Font")));
			this.errorProvider.SetIconAlignment(this.btnRefreshGroupList, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnRefreshGroupList.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.btnRefreshGroupList, ((int)(resources.GetObject("btnRefreshGroupList.IconPadding"))));
			this.btnRefreshGroupList.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshGroupList.Image")));
			this.btnRefreshGroupList.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnRefreshGroupList.ImageAlign")));
			this.btnRefreshGroupList.ImageIndex = ((int)(resources.GetObject("btnRefreshGroupList.ImageIndex")));
			this.btnRefreshGroupList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnRefreshGroupList.ImeMode")));
			this.btnRefreshGroupList.Location = ((System.Drawing.Point)(resources.GetObject("btnRefreshGroupList.Location")));
			this.btnRefreshGroupList.Name = "btnRefreshGroupList";
			this.btnRefreshGroupList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnRefreshGroupList.RightToLeft")));
			this.btnRefreshGroupList.Size = ((System.Drawing.Size)(resources.GetObject("btnRefreshGroupList.Size")));
			this.btnRefreshGroupList.TabIndex = ((int)(resources.GetObject("btnRefreshGroupList.TabIndex")));
			this.btnRefreshGroupList.Text = resources.GetString("btnRefreshGroupList.Text");
			this.btnRefreshGroupList.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnRefreshGroupList.TextAlign")));
			this.btnRefreshGroupList.Visible = ((bool)(resources.GetObject("btnRefreshGroupList.Visible")));
			this.btnRefreshGroupList.Click += new System.EventHandler(this.btnRefreshGroupList_Click);
			// 
			// btnUnsubscribe
			// 
			this.btnUnsubscribe.AccessibleDescription = resources.GetString("btnUnsubscribe.AccessibleDescription");
			this.btnUnsubscribe.AccessibleName = resources.GetString("btnUnsubscribe.AccessibleName");
			this.btnUnsubscribe.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnUnsubscribe.Anchor")));
			this.btnUnsubscribe.BackColor = System.Drawing.SystemColors.Control;
			this.btnUnsubscribe.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnUnsubscribe.BackgroundImage")));
			this.btnUnsubscribe.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnUnsubscribe.Dock")));
			this.btnUnsubscribe.Enabled = ((bool)(resources.GetObject("btnUnsubscribe.Enabled")));
			this.errorProvider.SetError(this.btnUnsubscribe, resources.GetString("btnUnsubscribe.Error"));
			this.btnUnsubscribe.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnUnsubscribe.FlatStyle")));
			this.btnUnsubscribe.Font = ((System.Drawing.Font)(resources.GetObject("btnUnsubscribe.Font")));
			this.errorProvider.SetIconAlignment(this.btnUnsubscribe, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnUnsubscribe.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.btnUnsubscribe, ((int)(resources.GetObject("btnUnsubscribe.IconPadding"))));
			this.btnUnsubscribe.Image = ((System.Drawing.Image)(resources.GetObject("btnUnsubscribe.Image")));
			this.btnUnsubscribe.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnUnsubscribe.ImageAlign")));
			this.btnUnsubscribe.ImageIndex = ((int)(resources.GetObject("btnUnsubscribe.ImageIndex")));
			this.btnUnsubscribe.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnUnsubscribe.ImeMode")));
			this.btnUnsubscribe.Location = ((System.Drawing.Point)(resources.GetObject("btnUnsubscribe.Location")));
			this.btnUnsubscribe.Name = "btnUnsubscribe";
			this.btnUnsubscribe.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnUnsubscribe.RightToLeft")));
			this.btnUnsubscribe.Size = ((System.Drawing.Size)(resources.GetObject("btnUnsubscribe.Size")));
			this.btnUnsubscribe.TabIndex = ((int)(resources.GetObject("btnUnsubscribe.TabIndex")));
			this.btnUnsubscribe.Text = resources.GetString("btnUnsubscribe.Text");
			this.btnUnsubscribe.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnUnsubscribe.TextAlign")));
			this.btnUnsubscribe.Visible = ((bool)(resources.GetObject("btnUnsubscribe.Visible")));
			this.btnUnsubscribe.Click += new System.EventHandler(this.btnUnsubscribe_Click);
			// 
			// btnSubscribe
			// 
			this.btnSubscribe.AccessibleDescription = resources.GetString("btnSubscribe.AccessibleDescription");
			this.btnSubscribe.AccessibleName = resources.GetString("btnSubscribe.AccessibleName");
			this.btnSubscribe.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSubscribe.Anchor")));
			this.btnSubscribe.BackColor = System.Drawing.SystemColors.Control;
			this.btnSubscribe.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSubscribe.BackgroundImage")));
			this.btnSubscribe.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSubscribe.Dock")));
			this.btnSubscribe.Enabled = ((bool)(resources.GetObject("btnSubscribe.Enabled")));
			this.errorProvider.SetError(this.btnSubscribe, resources.GetString("btnSubscribe.Error"));
			this.btnSubscribe.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSubscribe.FlatStyle")));
			this.btnSubscribe.Font = ((System.Drawing.Font)(resources.GetObject("btnSubscribe.Font")));
			this.errorProvider.SetIconAlignment(this.btnSubscribe, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnSubscribe.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.btnSubscribe, ((int)(resources.GetObject("btnSubscribe.IconPadding"))));
			this.btnSubscribe.Image = ((System.Drawing.Image)(resources.GetObject("btnSubscribe.Image")));
			this.btnSubscribe.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSubscribe.ImageAlign")));
			this.btnSubscribe.ImageIndex = ((int)(resources.GetObject("btnSubscribe.ImageIndex")));
			this.btnSubscribe.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSubscribe.ImeMode")));
			this.btnSubscribe.Location = ((System.Drawing.Point)(resources.GetObject("btnSubscribe.Location")));
			this.btnSubscribe.Name = "btnSubscribe";
			this.btnSubscribe.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSubscribe.RightToLeft")));
			this.btnSubscribe.Size = ((System.Drawing.Size)(resources.GetObject("btnSubscribe.Size")));
			this.btnSubscribe.TabIndex = ((int)(resources.GetObject("btnSubscribe.TabIndex")));
			this.btnSubscribe.Text = resources.GetString("btnSubscribe.Text");
			this.btnSubscribe.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSubscribe.TextAlign")));
			this.btnSubscribe.Visible = ((bool)(resources.GetObject("btnSubscribe.Visible")));
			this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
			// 
			// advancedViewPane
			// 
			this.advancedViewPane.AccessibleDescription = resources.GetString("advancedViewPane.AccessibleDescription");
			this.advancedViewPane.AccessibleName = resources.GetString("advancedViewPane.AccessibleName");
			this.advancedViewPane.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("advancedViewPane.Anchor")));
			this.advancedViewPane.AutoScroll = ((bool)(resources.GetObject("advancedViewPane.AutoScroll")));
			this.advancedViewPane.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("advancedViewPane.AutoScrollMargin")));
			this.advancedViewPane.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("advancedViewPane.AutoScrollMinSize")));
			this.advancedViewPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("advancedViewPane.BackgroundImage")));
			this.advancedViewPane.Controls.Add(this.labelServerTimeoutSetting);
			this.advancedViewPane.Controls.Add(this.label19);
			this.advancedViewPane.Controls.Add(this.lblCurrentTimout);
			this.advancedViewPane.Controls.Add(this.trackBarServerTimeout);
			this.advancedViewPane.Controls.Add(this.btnUseDefaultPort);
			this.advancedViewPane.Controls.Add(this.labelHighTimeout);
			this.advancedViewPane.Controls.Add(this.chkUseSSL);
			this.advancedViewPane.Controls.Add(this.labelLowTimeout);
			this.advancedViewPane.Controls.Add(this.label17);
			this.advancedViewPane.Controls.Add(this.txtServerPort);
			this.advancedViewPane.Controls.Add(this.labelServerPort);
			this.advancedViewPane.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("advancedViewPane.Dock")));
			this.advancedViewPane.Enabled = ((bool)(resources.GetObject("advancedViewPane.Enabled")));
			this.errorProvider.SetError(this.advancedViewPane, resources.GetString("advancedViewPane.Error"));
			this.advancedViewPane.Font = ((System.Drawing.Font)(resources.GetObject("advancedViewPane.Font")));
			this.advancedViewPane.HeaderFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.errorProvider.SetIconAlignment(this.advancedViewPane, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("advancedViewPane.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.advancedViewPane, ((int)(resources.GetObject("advancedViewPane.IconPadding"))));
			this.advancedViewPane.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("advancedViewPane.ImeMode")));
			this.advancedViewPane.Location = ((System.Drawing.Point)(resources.GetObject("advancedViewPane.Location")));
			this.advancedViewPane.Name = "advancedViewPane";
			this.advancedViewPane.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("advancedViewPane.RightToLeft")));
			this.advancedViewPane.Size = ((System.Drawing.Size)(resources.GetObject("advancedViewPane.Size")));
			this.advancedViewPane.TabIndex = ((int)(resources.GetObject("advancedViewPane.TabIndex")));
			this.advancedViewPane.Text = resources.GetString("advancedViewPane.Text");
			this.advancedViewPane.Visible = ((bool)(resources.GetObject("advancedViewPane.Visible")));
			// 
			// labelServerTimeoutSetting
			// 
			this.labelServerTimeoutSetting.AccessibleDescription = resources.GetString("labelServerTimeoutSetting.AccessibleDescription");
			this.labelServerTimeoutSetting.AccessibleName = resources.GetString("labelServerTimeoutSetting.AccessibleName");
			this.labelServerTimeoutSetting.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelServerTimeoutSetting.Anchor")));
			this.labelServerTimeoutSetting.AutoSize = ((bool)(resources.GetObject("labelServerTimeoutSetting.AutoSize")));
			this.labelServerTimeoutSetting.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelServerTimeoutSetting.Dock")));
			this.labelServerTimeoutSetting.Enabled = ((bool)(resources.GetObject("labelServerTimeoutSetting.Enabled")));
			this.errorProvider.SetError(this.labelServerTimeoutSetting, resources.GetString("labelServerTimeoutSetting.Error"));
			this.labelServerTimeoutSetting.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelServerTimeoutSetting.Font = ((System.Drawing.Font)(resources.GetObject("labelServerTimeoutSetting.Font")));
			this.errorProvider.SetIconAlignment(this.labelServerTimeoutSetting, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelServerTimeoutSetting.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelServerTimeoutSetting, ((int)(resources.GetObject("labelServerTimeoutSetting.IconPadding"))));
			this.labelServerTimeoutSetting.Image = ((System.Drawing.Image)(resources.GetObject("labelServerTimeoutSetting.Image")));
			this.labelServerTimeoutSetting.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelServerTimeoutSetting.ImageAlign")));
			this.labelServerTimeoutSetting.ImageIndex = ((int)(resources.GetObject("labelServerTimeoutSetting.ImageIndex")));
			this.labelServerTimeoutSetting.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelServerTimeoutSetting.ImeMode")));
			this.labelServerTimeoutSetting.Location = ((System.Drawing.Point)(resources.GetObject("labelServerTimeoutSetting.Location")));
			this.labelServerTimeoutSetting.Name = "labelServerTimeoutSetting";
			this.labelServerTimeoutSetting.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelServerTimeoutSetting.RightToLeft")));
			this.labelServerTimeoutSetting.Size = ((System.Drawing.Size)(resources.GetObject("labelServerTimeoutSetting.Size")));
			this.labelServerTimeoutSetting.TabIndex = ((int)(resources.GetObject("labelServerTimeoutSetting.TabIndex")));
			this.labelServerTimeoutSetting.Text = resources.GetString("labelServerTimeoutSetting.Text");
			this.labelServerTimeoutSetting.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelServerTimeoutSetting.TextAlign")));
			this.labelServerTimeoutSetting.Visible = ((bool)(resources.GetObject("labelServerTimeoutSetting.Visible")));
			// 
			// label19
			// 
			this.label19.AccessibleDescription = resources.GetString("label19.AccessibleDescription");
			this.label19.AccessibleName = resources.GetString("label19.AccessibleName");
			this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label19.Anchor")));
			this.label19.AutoSize = ((bool)(resources.GetObject("label19.AutoSize")));
			this.label19.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label19.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label19.Dock")));
			this.label19.Enabled = ((bool)(resources.GetObject("label19.Enabled")));
			this.errorProvider.SetError(this.label19, resources.GetString("label19.Error"));
			this.label19.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label19.Font = ((System.Drawing.Font)(resources.GetObject("label19.Font")));
			this.errorProvider.SetIconAlignment(this.label19, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label19.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.label19, ((int)(resources.GetObject("label19.IconPadding"))));
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
			this.label19.Visible = ((bool)(resources.GetObject("label19.Visible")));
			// 
			// lblCurrentTimout
			// 
			this.lblCurrentTimout.AccessibleDescription = resources.GetString("lblCurrentTimout.AccessibleDescription");
			this.lblCurrentTimout.AccessibleName = resources.GetString("lblCurrentTimout.AccessibleName");
			this.lblCurrentTimout.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblCurrentTimout.Anchor")));
			this.lblCurrentTimout.AutoSize = ((bool)(resources.GetObject("lblCurrentTimout.AutoSize")));
			this.lblCurrentTimout.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblCurrentTimout.Dock")));
			this.lblCurrentTimout.Enabled = ((bool)(resources.GetObject("lblCurrentTimout.Enabled")));
			this.errorProvider.SetError(this.lblCurrentTimout, resources.GetString("lblCurrentTimout.Error"));
			this.lblCurrentTimout.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCurrentTimout.Font = ((System.Drawing.Font)(resources.GetObject("lblCurrentTimout.Font")));
			this.errorProvider.SetIconAlignment(this.lblCurrentTimout, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("lblCurrentTimout.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.lblCurrentTimout, ((int)(resources.GetObject("lblCurrentTimout.IconPadding"))));
			this.lblCurrentTimout.Image = ((System.Drawing.Image)(resources.GetObject("lblCurrentTimout.Image")));
			this.lblCurrentTimout.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblCurrentTimout.ImageAlign")));
			this.lblCurrentTimout.ImageIndex = ((int)(resources.GetObject("lblCurrentTimout.ImageIndex")));
			this.lblCurrentTimout.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblCurrentTimout.ImeMode")));
			this.lblCurrentTimout.Location = ((System.Drawing.Point)(resources.GetObject("lblCurrentTimout.Location")));
			this.lblCurrentTimout.Name = "lblCurrentTimout";
			this.lblCurrentTimout.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblCurrentTimout.RightToLeft")));
			this.lblCurrentTimout.Size = ((System.Drawing.Size)(resources.GetObject("lblCurrentTimout.Size")));
			this.lblCurrentTimout.TabIndex = ((int)(resources.GetObject("lblCurrentTimout.TabIndex")));
			this.lblCurrentTimout.Text = resources.GetString("lblCurrentTimout.Text");
			this.lblCurrentTimout.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblCurrentTimout.TextAlign")));
			this.lblCurrentTimout.Visible = ((bool)(resources.GetObject("lblCurrentTimout.Visible")));
			// 
			// trackBarServerTimeout
			// 
			this.trackBarServerTimeout.AccessibleDescription = resources.GetString("trackBarServerTimeout.AccessibleDescription");
			this.trackBarServerTimeout.AccessibleName = resources.GetString("trackBarServerTimeout.AccessibleName");
			this.trackBarServerTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("trackBarServerTimeout.Anchor")));
			this.trackBarServerTimeout.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("trackBarServerTimeout.BackgroundImage")));
			this.trackBarServerTimeout.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("trackBarServerTimeout.Dock")));
			this.trackBarServerTimeout.Enabled = ((bool)(resources.GetObject("trackBarServerTimeout.Enabled")));
			this.errorProvider.SetError(this.trackBarServerTimeout, resources.GetString("trackBarServerTimeout.Error"));
			this.trackBarServerTimeout.Font = ((System.Drawing.Font)(resources.GetObject("trackBarServerTimeout.Font")));
			this.errorProvider.SetIconAlignment(this.trackBarServerTimeout, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("trackBarServerTimeout.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.trackBarServerTimeout, ((int)(resources.GetObject("trackBarServerTimeout.IconPadding"))));
			this.trackBarServerTimeout.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("trackBarServerTimeout.ImeMode")));
			this.trackBarServerTimeout.Location = ((System.Drawing.Point)(resources.GetObject("trackBarServerTimeout.Location")));
			this.trackBarServerTimeout.Name = "trackBarServerTimeout";
			this.trackBarServerTimeout.Orientation = ((System.Windows.Forms.Orientation)(resources.GetObject("trackBarServerTimeout.Orientation")));
			this.trackBarServerTimeout.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("trackBarServerTimeout.RightToLeft")));
			this.trackBarServerTimeout.Size = ((System.Drawing.Size)(resources.GetObject("trackBarServerTimeout.Size")));
			this.trackBarServerTimeout.TabIndex = ((int)(resources.GetObject("trackBarServerTimeout.TabIndex")));
			this.trackBarServerTimeout.Text = resources.GetString("trackBarServerTimeout.Text");
			this.trackBarServerTimeout.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarServerTimeout.Visible = ((bool)(resources.GetObject("trackBarServerTimeout.Visible")));
			this.trackBarServerTimeout.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.trackBarServerTimeout.Validated += new System.EventHandler(this.OnWidgetValidated);
			this.trackBarServerTimeout.Scroll += new System.EventHandler(this.OnTrackBarServerTimeoutChanged);
			// 
			// btnUseDefaultPort
			// 
			this.btnUseDefaultPort.AccessibleDescription = resources.GetString("btnUseDefaultPort.AccessibleDescription");
			this.btnUseDefaultPort.AccessibleName = resources.GetString("btnUseDefaultPort.AccessibleName");
			this.btnUseDefaultPort.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnUseDefaultPort.Anchor")));
			this.btnUseDefaultPort.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnUseDefaultPort.BackgroundImage")));
			this.btnUseDefaultPort.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnUseDefaultPort.Dock")));
			this.btnUseDefaultPort.Enabled = ((bool)(resources.GetObject("btnUseDefaultPort.Enabled")));
			this.errorProvider.SetError(this.btnUseDefaultPort, resources.GetString("btnUseDefaultPort.Error"));
			this.btnUseDefaultPort.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnUseDefaultPort.FlatStyle")));
			this.btnUseDefaultPort.Font = ((System.Drawing.Font)(resources.GetObject("btnUseDefaultPort.Font")));
			this.errorProvider.SetIconAlignment(this.btnUseDefaultPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnUseDefaultPort.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.btnUseDefaultPort, ((int)(resources.GetObject("btnUseDefaultPort.IconPadding"))));
			this.btnUseDefaultPort.Image = ((System.Drawing.Image)(resources.GetObject("btnUseDefaultPort.Image")));
			this.btnUseDefaultPort.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnUseDefaultPort.ImageAlign")));
			this.btnUseDefaultPort.ImageIndex = ((int)(resources.GetObject("btnUseDefaultPort.ImageIndex")));
			this.btnUseDefaultPort.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnUseDefaultPort.ImeMode")));
			this.btnUseDefaultPort.Location = ((System.Drawing.Point)(resources.GetObject("btnUseDefaultPort.Location")));
			this.btnUseDefaultPort.Name = "btnUseDefaultPort";
			this.btnUseDefaultPort.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnUseDefaultPort.RightToLeft")));
			this.btnUseDefaultPort.Size = ((System.Drawing.Size)(resources.GetObject("btnUseDefaultPort.Size")));
			this.btnUseDefaultPort.TabIndex = ((int)(resources.GetObject("btnUseDefaultPort.TabIndex")));
			this.btnUseDefaultPort.Text = resources.GetString("btnUseDefaultPort.Text");
			this.btnUseDefaultPort.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnUseDefaultPort.TextAlign")));
			this.btnUseDefaultPort.Visible = ((bool)(resources.GetObject("btnUseDefaultPort.Visible")));
			this.btnUseDefaultPort.Click += new System.EventHandler(this.btnUseDefaultPort_Click);
			// 
			// labelHighTimeout
			// 
			this.labelHighTimeout.AccessibleDescription = resources.GetString("labelHighTimeout.AccessibleDescription");
			this.labelHighTimeout.AccessibleName = resources.GetString("labelHighTimeout.AccessibleName");
			this.labelHighTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelHighTimeout.Anchor")));
			this.labelHighTimeout.AutoSize = ((bool)(resources.GetObject("labelHighTimeout.AutoSize")));
			this.labelHighTimeout.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelHighTimeout.Dock")));
			this.labelHighTimeout.Enabled = ((bool)(resources.GetObject("labelHighTimeout.Enabled")));
			this.errorProvider.SetError(this.labelHighTimeout, resources.GetString("labelHighTimeout.Error"));
			this.labelHighTimeout.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelHighTimeout.Font = ((System.Drawing.Font)(resources.GetObject("labelHighTimeout.Font")));
			this.errorProvider.SetIconAlignment(this.labelHighTimeout, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelHighTimeout.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelHighTimeout, ((int)(resources.GetObject("labelHighTimeout.IconPadding"))));
			this.labelHighTimeout.Image = ((System.Drawing.Image)(resources.GetObject("labelHighTimeout.Image")));
			this.labelHighTimeout.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelHighTimeout.ImageAlign")));
			this.labelHighTimeout.ImageIndex = ((int)(resources.GetObject("labelHighTimeout.ImageIndex")));
			this.labelHighTimeout.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelHighTimeout.ImeMode")));
			this.labelHighTimeout.Location = ((System.Drawing.Point)(resources.GetObject("labelHighTimeout.Location")));
			this.labelHighTimeout.Name = "labelHighTimeout";
			this.labelHighTimeout.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelHighTimeout.RightToLeft")));
			this.labelHighTimeout.Size = ((System.Drawing.Size)(resources.GetObject("labelHighTimeout.Size")));
			this.labelHighTimeout.TabIndex = ((int)(resources.GetObject("labelHighTimeout.TabIndex")));
			this.labelHighTimeout.Text = resources.GetString("labelHighTimeout.Text");
			this.labelHighTimeout.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelHighTimeout.TextAlign")));
			this.labelHighTimeout.Visible = ((bool)(resources.GetObject("labelHighTimeout.Visible")));
			// 
			// chkUseSSL
			// 
			this.chkUseSSL.AccessibleDescription = resources.GetString("chkUseSSL.AccessibleDescription");
			this.chkUseSSL.AccessibleName = resources.GetString("chkUseSSL.AccessibleName");
			this.chkUseSSL.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkUseSSL.Anchor")));
			this.chkUseSSL.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkUseSSL.Appearance")));
			this.chkUseSSL.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkUseSSL.BackgroundImage")));
			this.chkUseSSL.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkUseSSL.CheckAlign")));
			this.chkUseSSL.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkUseSSL.Dock")));
			this.chkUseSSL.Enabled = ((bool)(resources.GetObject("chkUseSSL.Enabled")));
			this.errorProvider.SetError(this.chkUseSSL, resources.GetString("chkUseSSL.Error"));
			this.chkUseSSL.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkUseSSL.FlatStyle")));
			this.chkUseSSL.Font = ((System.Drawing.Font)(resources.GetObject("chkUseSSL.Font")));
			this.errorProvider.SetIconAlignment(this.chkUseSSL, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkUseSSL.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.chkUseSSL, ((int)(resources.GetObject("chkUseSSL.IconPadding"))));
			this.chkUseSSL.Image = ((System.Drawing.Image)(resources.GetObject("chkUseSSL.Image")));
			this.chkUseSSL.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkUseSSL.ImageAlign")));
			this.chkUseSSL.ImageIndex = ((int)(resources.GetObject("chkUseSSL.ImageIndex")));
			this.chkUseSSL.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkUseSSL.ImeMode")));
			this.chkUseSSL.Location = ((System.Drawing.Point)(resources.GetObject("chkUseSSL.Location")));
			this.chkUseSSL.Name = "chkUseSSL";
			this.chkUseSSL.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkUseSSL.RightToLeft")));
			this.chkUseSSL.Size = ((System.Drawing.Size)(resources.GetObject("chkUseSSL.Size")));
			this.chkUseSSL.TabIndex = ((int)(resources.GetObject("chkUseSSL.TabIndex")));
			this.chkUseSSL.Text = resources.GetString("chkUseSSL.Text");
			this.chkUseSSL.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkUseSSL.TextAlign")));
			this.chkUseSSL.Visible = ((bool)(resources.GetObject("chkUseSSL.Visible")));
			this.chkUseSSL.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.chkUseSSL.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelLowTimeout
			// 
			this.labelLowTimeout.AccessibleDescription = resources.GetString("labelLowTimeout.AccessibleDescription");
			this.labelLowTimeout.AccessibleName = resources.GetString("labelLowTimeout.AccessibleName");
			this.labelLowTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelLowTimeout.Anchor")));
			this.labelLowTimeout.AutoSize = ((bool)(resources.GetObject("labelLowTimeout.AutoSize")));
			this.labelLowTimeout.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelLowTimeout.Dock")));
			this.labelLowTimeout.Enabled = ((bool)(resources.GetObject("labelLowTimeout.Enabled")));
			this.errorProvider.SetError(this.labelLowTimeout, resources.GetString("labelLowTimeout.Error"));
			this.labelLowTimeout.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelLowTimeout.Font = ((System.Drawing.Font)(resources.GetObject("labelLowTimeout.Font")));
			this.errorProvider.SetIconAlignment(this.labelLowTimeout, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelLowTimeout.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelLowTimeout, ((int)(resources.GetObject("labelLowTimeout.IconPadding"))));
			this.labelLowTimeout.Image = ((System.Drawing.Image)(resources.GetObject("labelLowTimeout.Image")));
			this.labelLowTimeout.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelLowTimeout.ImageAlign")));
			this.labelLowTimeout.ImageIndex = ((int)(resources.GetObject("labelLowTimeout.ImageIndex")));
			this.labelLowTimeout.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelLowTimeout.ImeMode")));
			this.labelLowTimeout.Location = ((System.Drawing.Point)(resources.GetObject("labelLowTimeout.Location")));
			this.labelLowTimeout.Name = "labelLowTimeout";
			this.labelLowTimeout.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelLowTimeout.RightToLeft")));
			this.labelLowTimeout.Size = ((System.Drawing.Size)(resources.GetObject("labelLowTimeout.Size")));
			this.labelLowTimeout.TabIndex = ((int)(resources.GetObject("labelLowTimeout.TabIndex")));
			this.labelLowTimeout.Text = resources.GetString("labelLowTimeout.Text");
			this.labelLowTimeout.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelLowTimeout.TextAlign")));
			this.labelLowTimeout.Visible = ((bool)(resources.GetObject("labelLowTimeout.Visible")));
			// 
			// label17
			// 
			this.label17.AccessibleDescription = resources.GetString("label17.AccessibleDescription");
			this.label17.AccessibleName = resources.GetString("label17.AccessibleName");
			this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label17.Anchor")));
			this.label17.AutoSize = ((bool)(resources.GetObject("label17.AutoSize")));
			this.label17.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label17.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label17.Dock")));
			this.label17.Enabled = ((bool)(resources.GetObject("label17.Enabled")));
			this.errorProvider.SetError(this.label17, resources.GetString("label17.Error"));
			this.label17.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label17.Font = ((System.Drawing.Font)(resources.GetObject("label17.Font")));
			this.errorProvider.SetIconAlignment(this.label17, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label17.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.label17, ((int)(resources.GetObject("label17.IconPadding"))));
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
			this.label17.Visible = ((bool)(resources.GetObject("label17.Visible")));
			// 
			// txtServerPort
			// 
			this.txtServerPort.AccessibleDescription = resources.GetString("txtServerPort.AccessibleDescription");
			this.txtServerPort.AccessibleName = resources.GetString("txtServerPort.AccessibleName");
			this.txtServerPort.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtServerPort.Anchor")));
			this.txtServerPort.AutoSize = ((bool)(resources.GetObject("txtServerPort.AutoSize")));
			this.txtServerPort.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtServerPort.BackgroundImage")));
			this.txtServerPort.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtServerPort.Dock")));
			this.txtServerPort.Enabled = ((bool)(resources.GetObject("txtServerPort.Enabled")));
			this.errorProvider.SetError(this.txtServerPort, resources.GetString("txtServerPort.Error"));
			this.txtServerPort.Font = ((System.Drawing.Font)(resources.GetObject("txtServerPort.Font")));
			this.errorProvider.SetIconAlignment(this.txtServerPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtServerPort.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtServerPort, ((int)(resources.GetObject("txtServerPort.IconPadding"))));
			this.txtServerPort.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtServerPort.ImeMode")));
			this.txtServerPort.Location = ((System.Drawing.Point)(resources.GetObject("txtServerPort.Location")));
			this.txtServerPort.MaxLength = ((int)(resources.GetObject("txtServerPort.MaxLength")));
			this.txtServerPort.Multiline = ((bool)(resources.GetObject("txtServerPort.Multiline")));
			this.txtServerPort.Name = "txtServerPort";
			this.txtServerPort.PasswordChar = ((char)(resources.GetObject("txtServerPort.PasswordChar")));
			this.txtServerPort.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtServerPort.RightToLeft")));
			this.txtServerPort.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtServerPort.ScrollBars")));
			this.txtServerPort.Size = ((System.Drawing.Size)(resources.GetObject("txtServerPort.Size")));
			this.txtServerPort.TabIndex = ((int)(resources.GetObject("txtServerPort.TabIndex")));
			this.txtServerPort.Text = resources.GetString("txtServerPort.Text");
			this.txtServerPort.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtServerPort.TextAlign")));
			this.txtServerPort.Visible = ((bool)(resources.GetObject("txtServerPort.Visible")));
			this.txtServerPort.WordWrap = ((bool)(resources.GetObject("txtServerPort.WordWrap")));
			this.txtServerPort.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtServerPort.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelServerPort
			// 
			this.labelServerPort.AccessibleDescription = resources.GetString("labelServerPort.AccessibleDescription");
			this.labelServerPort.AccessibleName = resources.GetString("labelServerPort.AccessibleName");
			this.labelServerPort.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelServerPort.Anchor")));
			this.labelServerPort.AutoSize = ((bool)(resources.GetObject("labelServerPort.AutoSize")));
			this.labelServerPort.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelServerPort.Dock")));
			this.labelServerPort.Enabled = ((bool)(resources.GetObject("labelServerPort.Enabled")));
			this.errorProvider.SetError(this.labelServerPort, resources.GetString("labelServerPort.Error"));
			this.labelServerPort.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelServerPort.Font = ((System.Drawing.Font)(resources.GetObject("labelServerPort.Font")));
			this.errorProvider.SetIconAlignment(this.labelServerPort, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelServerPort.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelServerPort, ((int)(resources.GetObject("labelServerPort.IconPadding"))));
			this.labelServerPort.Image = ((System.Drawing.Image)(resources.GetObject("labelServerPort.Image")));
			this.labelServerPort.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelServerPort.ImageAlign")));
			this.labelServerPort.ImageIndex = ((int)(resources.GetObject("labelServerPort.ImageIndex")));
			this.labelServerPort.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelServerPort.ImeMode")));
			this.labelServerPort.Location = ((System.Drawing.Point)(resources.GetObject("labelServerPort.Location")));
			this.labelServerPort.Name = "labelServerPort";
			this.labelServerPort.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelServerPort.RightToLeft")));
			this.labelServerPort.Size = ((System.Drawing.Size)(resources.GetObject("labelServerPort.Size")));
			this.labelServerPort.TabIndex = ((int)(resources.GetObject("labelServerPort.TabIndex")));
			this.labelServerPort.Text = resources.GetString("labelServerPort.Text");
			this.labelServerPort.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelServerPort.TextAlign")));
			this.labelServerPort.Visible = ((bool)(resources.GetObject("labelServerPort.Visible")));
			// 
			// serverViewPane
			// 
			this.serverViewPane.AccessibleDescription = resources.GetString("serverViewPane.AccessibleDescription");
			this.serverViewPane.AccessibleName = resources.GetString("serverViewPane.AccessibleName");
			this.serverViewPane.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("serverViewPane.Anchor")));
			this.serverViewPane.AutoScroll = ((bool)(resources.GetObject("serverViewPane.AutoScroll")));
			this.serverViewPane.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("serverViewPane.AutoScrollMargin")));
			this.serverViewPane.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("serverViewPane.AutoScrollMinSize")));
			this.serverViewPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("serverViewPane.BackgroundImage")));
			this.serverViewPane.Controls.Add(this.txtServerAuthPassword);
			this.serverViewPane.Controls.Add(this.labelNewsServerAccountPwd);
			this.serverViewPane.Controls.Add(this.txtServerAuthName);
			this.serverViewPane.Controls.Add(this.chkUseAuthentication);
			this.serverViewPane.Controls.Add(this.labelNewsServerAccoutnName);
			this.serverViewPane.Controls.Add(this.label12);
			this.serverViewPane.Controls.Add(this.txtNewsServerName);
			this.serverViewPane.Controls.Add(this.labelNewsServerName);
			this.serverViewPane.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("serverViewPane.Dock")));
			this.serverViewPane.Enabled = ((bool)(resources.GetObject("serverViewPane.Enabled")));
			this.errorProvider.SetError(this.serverViewPane, resources.GetString("serverViewPane.Error"));
			this.serverViewPane.Font = ((System.Drawing.Font)(resources.GetObject("serverViewPane.Font")));
			this.serverViewPane.HeaderFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.errorProvider.SetIconAlignment(this.serverViewPane, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("serverViewPane.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.serverViewPane, ((int)(resources.GetObject("serverViewPane.IconPadding"))));
			this.serverViewPane.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("serverViewPane.ImeMode")));
			this.serverViewPane.Location = ((System.Drawing.Point)(resources.GetObject("serverViewPane.Location")));
			this.serverViewPane.Name = "serverViewPane";
			this.serverViewPane.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("serverViewPane.RightToLeft")));
			this.serverViewPane.Size = ((System.Drawing.Size)(resources.GetObject("serverViewPane.Size")));
			this.serverViewPane.TabIndex = ((int)(resources.GetObject("serverViewPane.TabIndex")));
			this.serverViewPane.Text = resources.GetString("serverViewPane.Text");
			this.serverViewPane.Visible = ((bool)(resources.GetObject("serverViewPane.Visible")));
			// 
			// txtServerAuthPassword
			// 
			this.txtServerAuthPassword.AccessibleDescription = resources.GetString("txtServerAuthPassword.AccessibleDescription");
			this.txtServerAuthPassword.AccessibleName = resources.GetString("txtServerAuthPassword.AccessibleName");
			this.txtServerAuthPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtServerAuthPassword.Anchor")));
			this.txtServerAuthPassword.AutoSize = ((bool)(resources.GetObject("txtServerAuthPassword.AutoSize")));
			this.txtServerAuthPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtServerAuthPassword.BackgroundImage")));
			this.txtServerAuthPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtServerAuthPassword.Dock")));
			this.txtServerAuthPassword.Enabled = ((bool)(resources.GetObject("txtServerAuthPassword.Enabled")));
			this.errorProvider.SetError(this.txtServerAuthPassword, resources.GetString("txtServerAuthPassword.Error"));
			this.txtServerAuthPassword.Font = ((System.Drawing.Font)(resources.GetObject("txtServerAuthPassword.Font")));
			this.errorProvider.SetIconAlignment(this.txtServerAuthPassword, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtServerAuthPassword.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtServerAuthPassword, ((int)(resources.GetObject("txtServerAuthPassword.IconPadding"))));
			this.txtServerAuthPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtServerAuthPassword.ImeMode")));
			this.txtServerAuthPassword.Location = ((System.Drawing.Point)(resources.GetObject("txtServerAuthPassword.Location")));
			this.txtServerAuthPassword.MaxLength = ((int)(resources.GetObject("txtServerAuthPassword.MaxLength")));
			this.txtServerAuthPassword.Multiline = ((bool)(resources.GetObject("txtServerAuthPassword.Multiline")));
			this.txtServerAuthPassword.Name = "txtServerAuthPassword";
			this.txtServerAuthPassword.PasswordChar = ((char)(resources.GetObject("txtServerAuthPassword.PasswordChar")));
			this.txtServerAuthPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtServerAuthPassword.RightToLeft")));
			this.txtServerAuthPassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtServerAuthPassword.ScrollBars")));
			this.txtServerAuthPassword.Size = ((System.Drawing.Size)(resources.GetObject("txtServerAuthPassword.Size")));
			this.txtServerAuthPassword.TabIndex = ((int)(resources.GetObject("txtServerAuthPassword.TabIndex")));
			this.txtServerAuthPassword.Text = resources.GetString("txtServerAuthPassword.Text");
			this.txtServerAuthPassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtServerAuthPassword.TextAlign")));
			this.txtServerAuthPassword.Visible = ((bool)(resources.GetObject("txtServerAuthPassword.Visible")));
			this.txtServerAuthPassword.WordWrap = ((bool)(resources.GetObject("txtServerAuthPassword.WordWrap")));
			this.txtServerAuthPassword.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtServerAuthPassword.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelNewsServerAccountPwd
			// 
			this.labelNewsServerAccountPwd.AccessibleDescription = resources.GetString("labelNewsServerAccountPwd.AccessibleDescription");
			this.labelNewsServerAccountPwd.AccessibleName = resources.GetString("labelNewsServerAccountPwd.AccessibleName");
			this.labelNewsServerAccountPwd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelNewsServerAccountPwd.Anchor")));
			this.labelNewsServerAccountPwd.AutoSize = ((bool)(resources.GetObject("labelNewsServerAccountPwd.AutoSize")));
			this.labelNewsServerAccountPwd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelNewsServerAccountPwd.Dock")));
			this.labelNewsServerAccountPwd.Enabled = ((bool)(resources.GetObject("labelNewsServerAccountPwd.Enabled")));
			this.errorProvider.SetError(this.labelNewsServerAccountPwd, resources.GetString("labelNewsServerAccountPwd.Error"));
			this.labelNewsServerAccountPwd.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelNewsServerAccountPwd.Font = ((System.Drawing.Font)(resources.GetObject("labelNewsServerAccountPwd.Font")));
			this.errorProvider.SetIconAlignment(this.labelNewsServerAccountPwd, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelNewsServerAccountPwd.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelNewsServerAccountPwd, ((int)(resources.GetObject("labelNewsServerAccountPwd.IconPadding"))));
			this.labelNewsServerAccountPwd.Image = ((System.Drawing.Image)(resources.GetObject("labelNewsServerAccountPwd.Image")));
			this.labelNewsServerAccountPwd.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsServerAccountPwd.ImageAlign")));
			this.labelNewsServerAccountPwd.ImageIndex = ((int)(resources.GetObject("labelNewsServerAccountPwd.ImageIndex")));
			this.labelNewsServerAccountPwd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelNewsServerAccountPwd.ImeMode")));
			this.labelNewsServerAccountPwd.Location = ((System.Drawing.Point)(resources.GetObject("labelNewsServerAccountPwd.Location")));
			this.labelNewsServerAccountPwd.Name = "labelNewsServerAccountPwd";
			this.labelNewsServerAccountPwd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelNewsServerAccountPwd.RightToLeft")));
			this.labelNewsServerAccountPwd.Size = ((System.Drawing.Size)(resources.GetObject("labelNewsServerAccountPwd.Size")));
			this.labelNewsServerAccountPwd.TabIndex = ((int)(resources.GetObject("labelNewsServerAccountPwd.TabIndex")));
			this.labelNewsServerAccountPwd.Text = resources.GetString("labelNewsServerAccountPwd.Text");
			this.labelNewsServerAccountPwd.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsServerAccountPwd.TextAlign")));
			this.labelNewsServerAccountPwd.Visible = ((bool)(resources.GetObject("labelNewsServerAccountPwd.Visible")));
			// 
			// txtServerAuthName
			// 
			this.txtServerAuthName.AccessibleDescription = resources.GetString("txtServerAuthName.AccessibleDescription");
			this.txtServerAuthName.AccessibleName = resources.GetString("txtServerAuthName.AccessibleName");
			this.txtServerAuthName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtServerAuthName.Anchor")));
			this.txtServerAuthName.AutoSize = ((bool)(resources.GetObject("txtServerAuthName.AutoSize")));
			this.txtServerAuthName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtServerAuthName.BackgroundImage")));
			this.txtServerAuthName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtServerAuthName.Dock")));
			this.txtServerAuthName.Enabled = ((bool)(resources.GetObject("txtServerAuthName.Enabled")));
			this.errorProvider.SetError(this.txtServerAuthName, resources.GetString("txtServerAuthName.Error"));
			this.txtServerAuthName.Font = ((System.Drawing.Font)(resources.GetObject("txtServerAuthName.Font")));
			this.errorProvider.SetIconAlignment(this.txtServerAuthName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtServerAuthName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtServerAuthName, ((int)(resources.GetObject("txtServerAuthName.IconPadding"))));
			this.txtServerAuthName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtServerAuthName.ImeMode")));
			this.txtServerAuthName.Location = ((System.Drawing.Point)(resources.GetObject("txtServerAuthName.Location")));
			this.txtServerAuthName.MaxLength = ((int)(resources.GetObject("txtServerAuthName.MaxLength")));
			this.txtServerAuthName.Multiline = ((bool)(resources.GetObject("txtServerAuthName.Multiline")));
			this.txtServerAuthName.Name = "txtServerAuthName";
			this.txtServerAuthName.PasswordChar = ((char)(resources.GetObject("txtServerAuthName.PasswordChar")));
			this.txtServerAuthName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtServerAuthName.RightToLeft")));
			this.txtServerAuthName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtServerAuthName.ScrollBars")));
			this.txtServerAuthName.Size = ((System.Drawing.Size)(resources.GetObject("txtServerAuthName.Size")));
			this.txtServerAuthName.TabIndex = ((int)(resources.GetObject("txtServerAuthName.TabIndex")));
			this.txtServerAuthName.Text = resources.GetString("txtServerAuthName.Text");
			this.txtServerAuthName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtServerAuthName.TextAlign")));
			this.txtServerAuthName.Visible = ((bool)(resources.GetObject("txtServerAuthName.Visible")));
			this.txtServerAuthName.WordWrap = ((bool)(resources.GetObject("txtServerAuthName.WordWrap")));
			this.txtServerAuthName.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtServerAuthName.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// chkUseAuthentication
			// 
			this.chkUseAuthentication.AccessibleDescription = resources.GetString("chkUseAuthentication.AccessibleDescription");
			this.chkUseAuthentication.AccessibleName = resources.GetString("chkUseAuthentication.AccessibleName");
			this.chkUseAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkUseAuthentication.Anchor")));
			this.chkUseAuthentication.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkUseAuthentication.Appearance")));
			this.chkUseAuthentication.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkUseAuthentication.BackgroundImage")));
			this.chkUseAuthentication.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkUseAuthentication.CheckAlign")));
			this.chkUseAuthentication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkUseAuthentication.Dock")));
			this.chkUseAuthentication.Enabled = ((bool)(resources.GetObject("chkUseAuthentication.Enabled")));
			this.errorProvider.SetError(this.chkUseAuthentication, resources.GetString("chkUseAuthentication.Error"));
			this.chkUseAuthentication.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkUseAuthentication.FlatStyle")));
			this.chkUseAuthentication.Font = ((System.Drawing.Font)(resources.GetObject("chkUseAuthentication.Font")));
			this.errorProvider.SetIconAlignment(this.chkUseAuthentication, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkUseAuthentication.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.chkUseAuthentication, ((int)(resources.GetObject("chkUseAuthentication.IconPadding"))));
			this.chkUseAuthentication.Image = ((System.Drawing.Image)(resources.GetObject("chkUseAuthentication.Image")));
			this.chkUseAuthentication.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkUseAuthentication.ImageAlign")));
			this.chkUseAuthentication.ImageIndex = ((int)(resources.GetObject("chkUseAuthentication.ImageIndex")));
			this.chkUseAuthentication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkUseAuthentication.ImeMode")));
			this.chkUseAuthentication.Location = ((System.Drawing.Point)(resources.GetObject("chkUseAuthentication.Location")));
			this.chkUseAuthentication.Name = "chkUseAuthentication";
			this.chkUseAuthentication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkUseAuthentication.RightToLeft")));
			this.chkUseAuthentication.Size = ((System.Drawing.Size)(resources.GetObject("chkUseAuthentication.Size")));
			this.chkUseAuthentication.TabIndex = ((int)(resources.GetObject("chkUseAuthentication.TabIndex")));
			this.chkUseAuthentication.Text = resources.GetString("chkUseAuthentication.Text");
			this.chkUseAuthentication.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkUseAuthentication.TextAlign")));
			this.chkUseAuthentication.Visible = ((bool)(resources.GetObject("chkUseAuthentication.Visible")));
			this.chkUseAuthentication.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.chkUseAuthentication.Validated += new System.EventHandler(this.OnWidgetValidated);
			this.chkUseAuthentication.CheckedChanged += new System.EventHandler(this.OnUseAuthenticationCheckedChanged);
			// 
			// labelNewsServerAccoutnName
			// 
			this.labelNewsServerAccoutnName.AccessibleDescription = resources.GetString("labelNewsServerAccoutnName.AccessibleDescription");
			this.labelNewsServerAccoutnName.AccessibleName = resources.GetString("labelNewsServerAccoutnName.AccessibleName");
			this.labelNewsServerAccoutnName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelNewsServerAccoutnName.Anchor")));
			this.labelNewsServerAccoutnName.AutoSize = ((bool)(resources.GetObject("labelNewsServerAccoutnName.AutoSize")));
			this.labelNewsServerAccoutnName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelNewsServerAccoutnName.Dock")));
			this.labelNewsServerAccoutnName.Enabled = ((bool)(resources.GetObject("labelNewsServerAccoutnName.Enabled")));
			this.errorProvider.SetError(this.labelNewsServerAccoutnName, resources.GetString("labelNewsServerAccoutnName.Error"));
			this.labelNewsServerAccoutnName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelNewsServerAccoutnName.Font = ((System.Drawing.Font)(resources.GetObject("labelNewsServerAccoutnName.Font")));
			this.errorProvider.SetIconAlignment(this.labelNewsServerAccoutnName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelNewsServerAccoutnName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelNewsServerAccoutnName, ((int)(resources.GetObject("labelNewsServerAccoutnName.IconPadding"))));
			this.labelNewsServerAccoutnName.Image = ((System.Drawing.Image)(resources.GetObject("labelNewsServerAccoutnName.Image")));
			this.labelNewsServerAccoutnName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsServerAccoutnName.ImageAlign")));
			this.labelNewsServerAccoutnName.ImageIndex = ((int)(resources.GetObject("labelNewsServerAccoutnName.ImageIndex")));
			this.labelNewsServerAccoutnName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelNewsServerAccoutnName.ImeMode")));
			this.labelNewsServerAccoutnName.Location = ((System.Drawing.Point)(resources.GetObject("labelNewsServerAccoutnName.Location")));
			this.labelNewsServerAccoutnName.Name = "labelNewsServerAccoutnName";
			this.labelNewsServerAccoutnName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelNewsServerAccoutnName.RightToLeft")));
			this.labelNewsServerAccoutnName.Size = ((System.Drawing.Size)(resources.GetObject("labelNewsServerAccoutnName.Size")));
			this.labelNewsServerAccoutnName.TabIndex = ((int)(resources.GetObject("labelNewsServerAccoutnName.TabIndex")));
			this.labelNewsServerAccoutnName.Text = resources.GetString("labelNewsServerAccoutnName.Text");
			this.labelNewsServerAccoutnName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsServerAccoutnName.TextAlign")));
			this.labelNewsServerAccoutnName.Visible = ((bool)(resources.GetObject("labelNewsServerAccoutnName.Visible")));
			// 
			// label12
			// 
			this.label12.AccessibleDescription = resources.GetString("label12.AccessibleDescription");
			this.label12.AccessibleName = resources.GetString("label12.AccessibleName");
			this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label12.Anchor")));
			this.label12.AutoSize = ((bool)(resources.GetObject("label12.AutoSize")));
			this.label12.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label12.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label12.Dock")));
			this.label12.Enabled = ((bool)(resources.GetObject("label12.Enabled")));
			this.errorProvider.SetError(this.label12, resources.GetString("label12.Error"));
			this.label12.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label12.Font = ((System.Drawing.Font)(resources.GetObject("label12.Font")));
			this.errorProvider.SetIconAlignment(this.label12, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label12.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.label12, ((int)(resources.GetObject("label12.IconPadding"))));
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
			this.label12.Visible = ((bool)(resources.GetObject("label12.Visible")));
			// 
			// txtNewsServerName
			// 
			this.txtNewsServerName.AccessibleDescription = resources.GetString("txtNewsServerName.AccessibleDescription");
			this.txtNewsServerName.AccessibleName = resources.GetString("txtNewsServerName.AccessibleName");
			this.txtNewsServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtNewsServerName.Anchor")));
			this.txtNewsServerName.AutoSize = ((bool)(resources.GetObject("txtNewsServerName.AutoSize")));
			this.txtNewsServerName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtNewsServerName.BackgroundImage")));
			this.txtNewsServerName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtNewsServerName.Dock")));
			this.txtNewsServerName.Enabled = ((bool)(resources.GetObject("txtNewsServerName.Enabled")));
			this.errorProvider.SetError(this.txtNewsServerName, resources.GetString("txtNewsServerName.Error"));
			this.txtNewsServerName.Font = ((System.Drawing.Font)(resources.GetObject("txtNewsServerName.Font")));
			this.errorProvider.SetIconAlignment(this.txtNewsServerName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtNewsServerName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtNewsServerName, ((int)(resources.GetObject("txtNewsServerName.IconPadding"))));
			this.txtNewsServerName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtNewsServerName.ImeMode")));
			this.txtNewsServerName.Location = ((System.Drawing.Point)(resources.GetObject("txtNewsServerName.Location")));
			this.txtNewsServerName.MaxLength = ((int)(resources.GetObject("txtNewsServerName.MaxLength")));
			this.txtNewsServerName.Multiline = ((bool)(resources.GetObject("txtNewsServerName.Multiline")));
			this.txtNewsServerName.Name = "txtNewsServerName";
			this.txtNewsServerName.PasswordChar = ((char)(resources.GetObject("txtNewsServerName.PasswordChar")));
			this.txtNewsServerName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtNewsServerName.RightToLeft")));
			this.txtNewsServerName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtNewsServerName.ScrollBars")));
			this.txtNewsServerName.Size = ((System.Drawing.Size)(resources.GetObject("txtNewsServerName.Size")));
			this.txtNewsServerName.TabIndex = ((int)(resources.GetObject("txtNewsServerName.TabIndex")));
			this.txtNewsServerName.Text = resources.GetString("txtNewsServerName.Text");
			this.txtNewsServerName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtNewsServerName.TextAlign")));
			this.txtNewsServerName.Visible = ((bool)(resources.GetObject("txtNewsServerName.Visible")));
			this.txtNewsServerName.WordWrap = ((bool)(resources.GetObject("txtNewsServerName.WordWrap")));
			this.txtNewsServerName.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtNewsServerName.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelNewsServerName
			// 
			this.labelNewsServerName.AccessibleDescription = resources.GetString("labelNewsServerName.AccessibleDescription");
			this.labelNewsServerName.AccessibleName = resources.GetString("labelNewsServerName.AccessibleName");
			this.labelNewsServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelNewsServerName.Anchor")));
			this.labelNewsServerName.AutoSize = ((bool)(resources.GetObject("labelNewsServerName.AutoSize")));
			this.labelNewsServerName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelNewsServerName.Dock")));
			this.labelNewsServerName.Enabled = ((bool)(resources.GetObject("labelNewsServerName.Enabled")));
			this.errorProvider.SetError(this.labelNewsServerName, resources.GetString("labelNewsServerName.Error"));
			this.labelNewsServerName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelNewsServerName.Font = ((System.Drawing.Font)(resources.GetObject("labelNewsServerName.Font")));
			this.errorProvider.SetIconAlignment(this.labelNewsServerName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelNewsServerName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelNewsServerName, ((int)(resources.GetObject("labelNewsServerName.IconPadding"))));
			this.labelNewsServerName.Image = ((System.Drawing.Image)(resources.GetObject("labelNewsServerName.Image")));
			this.labelNewsServerName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsServerName.ImageAlign")));
			this.labelNewsServerName.ImageIndex = ((int)(resources.GetObject("labelNewsServerName.ImageIndex")));
			this.labelNewsServerName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelNewsServerName.ImeMode")));
			this.labelNewsServerName.Location = ((System.Drawing.Point)(resources.GetObject("labelNewsServerName.Location")));
			this.labelNewsServerName.Name = "labelNewsServerName";
			this.labelNewsServerName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelNewsServerName.RightToLeft")));
			this.labelNewsServerName.Size = ((System.Drawing.Size)(resources.GetObject("labelNewsServerName.Size")));
			this.labelNewsServerName.TabIndex = ((int)(resources.GetObject("labelNewsServerName.TabIndex")));
			this.labelNewsServerName.Text = resources.GetString("labelNewsServerName.Text");
			this.labelNewsServerName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsServerName.TextAlign")));
			this.labelNewsServerName.Visible = ((bool)(resources.GetObject("labelNewsServerName.Visible")));
			// 
			// generalViewPane
			// 
			this.generalViewPane.AccessibleDescription = resources.GetString("generalViewPane.AccessibleDescription");
			this.generalViewPane.AccessibleName = resources.GetString("generalViewPane.AccessibleName");
			this.generalViewPane.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("generalViewPane.Anchor")));
			this.generalViewPane.AutoScroll = ((bool)(resources.GetObject("generalViewPane.AutoScroll")));
			this.generalViewPane.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("generalViewPane.AutoScrollMargin")));
			this.generalViewPane.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("generalViewPane.AutoScrollMinSize")));
			this.generalViewPane.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("generalViewPane.BackgroundImage")));
			this.generalViewPane.Controls.Add(this.chkConsiderServerOnRefresh);
			this.generalViewPane.Controls.Add(this.cboDefaultIdentities);
			this.generalViewPane.Controls.Add(this.labelDefaultEdentity);
			this.generalViewPane.Controls.Add(this.label9);
			this.generalViewPane.Controls.Add(this.txtNewsAccountName);
			this.generalViewPane.Controls.Add(this.labelNewsAccount);
			this.generalViewPane.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("generalViewPane.Dock")));
			this.generalViewPane.Enabled = ((bool)(resources.GetObject("generalViewPane.Enabled")));
			this.errorProvider.SetError(this.generalViewPane, resources.GetString("generalViewPane.Error"));
			this.generalViewPane.Font = ((System.Drawing.Font)(resources.GetObject("generalViewPane.Font")));
			this.generalViewPane.HeaderFont = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.errorProvider.SetIconAlignment(this.generalViewPane, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("generalViewPane.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.generalViewPane, ((int)(resources.GetObject("generalViewPane.IconPadding"))));
			this.generalViewPane.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("generalViewPane.ImeMode")));
			this.generalViewPane.Location = ((System.Drawing.Point)(resources.GetObject("generalViewPane.Location")));
			this.generalViewPane.Name = "generalViewPane";
			this.generalViewPane.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("generalViewPane.RightToLeft")));
			this.generalViewPane.Size = ((System.Drawing.Size)(resources.GetObject("generalViewPane.Size")));
			this.generalViewPane.TabIndex = ((int)(resources.GetObject("generalViewPane.TabIndex")));
			this.generalViewPane.Text = resources.GetString("generalViewPane.Text");
			this.generalViewPane.Visible = ((bool)(resources.GetObject("generalViewPane.Visible")));
			// 
			// chkConsiderServerOnRefresh
			// 
			this.chkConsiderServerOnRefresh.AccessibleDescription = resources.GetString("chkConsiderServerOnRefresh.AccessibleDescription");
			this.chkConsiderServerOnRefresh.AccessibleName = resources.GetString("chkConsiderServerOnRefresh.AccessibleName");
			this.chkConsiderServerOnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("chkConsiderServerOnRefresh.Anchor")));
			this.chkConsiderServerOnRefresh.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("chkConsiderServerOnRefresh.Appearance")));
			this.chkConsiderServerOnRefresh.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("chkConsiderServerOnRefresh.BackgroundImage")));
			this.chkConsiderServerOnRefresh.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkConsiderServerOnRefresh.CheckAlign")));
			this.chkConsiderServerOnRefresh.Checked = true;
			this.chkConsiderServerOnRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkConsiderServerOnRefresh.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("chkConsiderServerOnRefresh.Dock")));
			this.chkConsiderServerOnRefresh.Enabled = ((bool)(resources.GetObject("chkConsiderServerOnRefresh.Enabled")));
			this.errorProvider.SetError(this.chkConsiderServerOnRefresh, resources.GetString("chkConsiderServerOnRefresh.Error"));
			this.chkConsiderServerOnRefresh.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("chkConsiderServerOnRefresh.FlatStyle")));
			this.chkConsiderServerOnRefresh.Font = ((System.Drawing.Font)(resources.GetObject("chkConsiderServerOnRefresh.Font")));
			this.errorProvider.SetIconAlignment(this.chkConsiderServerOnRefresh, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("chkConsiderServerOnRefresh.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.chkConsiderServerOnRefresh, ((int)(resources.GetObject("chkConsiderServerOnRefresh.IconPadding"))));
			this.chkConsiderServerOnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("chkConsiderServerOnRefresh.Image")));
			this.chkConsiderServerOnRefresh.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkConsiderServerOnRefresh.ImageAlign")));
			this.chkConsiderServerOnRefresh.ImageIndex = ((int)(resources.GetObject("chkConsiderServerOnRefresh.ImageIndex")));
			this.chkConsiderServerOnRefresh.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("chkConsiderServerOnRefresh.ImeMode")));
			this.chkConsiderServerOnRefresh.Location = ((System.Drawing.Point)(resources.GetObject("chkConsiderServerOnRefresh.Location")));
			this.chkConsiderServerOnRefresh.Name = "chkConsiderServerOnRefresh";
			this.chkConsiderServerOnRefresh.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("chkConsiderServerOnRefresh.RightToLeft")));
			this.chkConsiderServerOnRefresh.Size = ((System.Drawing.Size)(resources.GetObject("chkConsiderServerOnRefresh.Size")));
			this.chkConsiderServerOnRefresh.TabIndex = ((int)(resources.GetObject("chkConsiderServerOnRefresh.TabIndex")));
			this.chkConsiderServerOnRefresh.Text = resources.GetString("chkConsiderServerOnRefresh.Text");
			this.chkConsiderServerOnRefresh.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("chkConsiderServerOnRefresh.TextAlign")));
			this.chkConsiderServerOnRefresh.Visible = ((bool)(resources.GetObject("chkConsiderServerOnRefresh.Visible")));
			this.chkConsiderServerOnRefresh.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.chkConsiderServerOnRefresh.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// cboDefaultIdentities
			// 
			this.cboDefaultIdentities.AccessibleDescription = resources.GetString("cboDefaultIdentities.AccessibleDescription");
			this.cboDefaultIdentities.AccessibleName = resources.GetString("cboDefaultIdentities.AccessibleName");
			this.cboDefaultIdentities.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cboDefaultIdentities.Anchor")));
			this.cboDefaultIdentities.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cboDefaultIdentities.BackgroundImage")));
			this.cboDefaultIdentities.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cboDefaultIdentities.Dock")));
			this.cboDefaultIdentities.Enabled = ((bool)(resources.GetObject("cboDefaultIdentities.Enabled")));
			this.errorProvider.SetError(this.cboDefaultIdentities, resources.GetString("cboDefaultIdentities.Error"));
			this.cboDefaultIdentities.Font = ((System.Drawing.Font)(resources.GetObject("cboDefaultIdentities.Font")));
			this.errorProvider.SetIconAlignment(this.cboDefaultIdentities, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cboDefaultIdentities.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.cboDefaultIdentities, ((int)(resources.GetObject("cboDefaultIdentities.IconPadding"))));
			this.cboDefaultIdentities.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cboDefaultIdentities.ImeMode")));
			this.cboDefaultIdentities.IntegralHeight = ((bool)(resources.GetObject("cboDefaultIdentities.IntegralHeight")));
			this.cboDefaultIdentities.ItemHeight = ((int)(resources.GetObject("cboDefaultIdentities.ItemHeight")));
			this.cboDefaultIdentities.Location = ((System.Drawing.Point)(resources.GetObject("cboDefaultIdentities.Location")));
			this.cboDefaultIdentities.MaxDropDownItems = ((int)(resources.GetObject("cboDefaultIdentities.MaxDropDownItems")));
			this.cboDefaultIdentities.MaxLength = ((int)(resources.GetObject("cboDefaultIdentities.MaxLength")));
			this.cboDefaultIdentities.Name = "cboDefaultIdentities";
			this.cboDefaultIdentities.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cboDefaultIdentities.RightToLeft")));
			this.cboDefaultIdentities.Size = ((System.Drawing.Size)(resources.GetObject("cboDefaultIdentities.Size")));
			this.cboDefaultIdentities.TabIndex = ((int)(resources.GetObject("cboDefaultIdentities.TabIndex")));
			this.cboDefaultIdentities.Text = resources.GetString("cboDefaultIdentities.Text");
			this.cboDefaultIdentities.Visible = ((bool)(resources.GetObject("cboDefaultIdentities.Visible")));
			this.cboDefaultIdentities.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.cboDefaultIdentities.Validated += new System.EventHandler(this.OnWidgetValidated);
			// 
			// labelDefaultEdentity
			// 
			this.labelDefaultEdentity.AccessibleDescription = resources.GetString("labelDefaultEdentity.AccessibleDescription");
			this.labelDefaultEdentity.AccessibleName = resources.GetString("labelDefaultEdentity.AccessibleName");
			this.labelDefaultEdentity.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelDefaultEdentity.Anchor")));
			this.labelDefaultEdentity.AutoSize = ((bool)(resources.GetObject("labelDefaultEdentity.AutoSize")));
			this.labelDefaultEdentity.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelDefaultEdentity.Dock")));
			this.labelDefaultEdentity.Enabled = ((bool)(resources.GetObject("labelDefaultEdentity.Enabled")));
			this.errorProvider.SetError(this.labelDefaultEdentity, resources.GetString("labelDefaultEdentity.Error"));
			this.labelDefaultEdentity.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelDefaultEdentity.Font = ((System.Drawing.Font)(resources.GetObject("labelDefaultEdentity.Font")));
			this.errorProvider.SetIconAlignment(this.labelDefaultEdentity, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelDefaultEdentity.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelDefaultEdentity, ((int)(resources.GetObject("labelDefaultEdentity.IconPadding"))));
			this.labelDefaultEdentity.Image = ((System.Drawing.Image)(resources.GetObject("labelDefaultEdentity.Image")));
			this.labelDefaultEdentity.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelDefaultEdentity.ImageAlign")));
			this.labelDefaultEdentity.ImageIndex = ((int)(resources.GetObject("labelDefaultEdentity.ImageIndex")));
			this.labelDefaultEdentity.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelDefaultEdentity.ImeMode")));
			this.labelDefaultEdentity.Location = ((System.Drawing.Point)(resources.GetObject("labelDefaultEdentity.Location")));
			this.labelDefaultEdentity.Name = "labelDefaultEdentity";
			this.labelDefaultEdentity.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelDefaultEdentity.RightToLeft")));
			this.labelDefaultEdentity.Size = ((System.Drawing.Size)(resources.GetObject("labelDefaultEdentity.Size")));
			this.labelDefaultEdentity.TabIndex = ((int)(resources.GetObject("labelDefaultEdentity.TabIndex")));
			this.labelDefaultEdentity.Text = resources.GetString("labelDefaultEdentity.Text");
			this.labelDefaultEdentity.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelDefaultEdentity.TextAlign")));
			this.labelDefaultEdentity.Visible = ((bool)(resources.GetObject("labelDefaultEdentity.Visible")));
			// 
			// label9
			// 
			this.label9.AccessibleDescription = resources.GetString("label9.AccessibleDescription");
			this.label9.AccessibleName = resources.GetString("label9.AccessibleName");
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label9.Anchor")));
			this.label9.AutoSize = ((bool)(resources.GetObject("label9.AutoSize")));
			this.label9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label9.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label9.Dock")));
			this.label9.Enabled = ((bool)(resources.GetObject("label9.Enabled")));
			this.errorProvider.SetError(this.label9, resources.GetString("label9.Error"));
			this.label9.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label9.Font = ((System.Drawing.Font)(resources.GetObject("label9.Font")));
			this.errorProvider.SetIconAlignment(this.label9, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label9.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.label9, ((int)(resources.GetObject("label9.IconPadding"))));
			this.label9.Image = ((System.Drawing.Image)(resources.GetObject("label9.Image")));
			this.label9.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.ImageAlign")));
			this.label9.ImageIndex = ((int)(resources.GetObject("label9.ImageIndex")));
			this.label9.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label9.ImeMode")));
			this.label9.Location = ((System.Drawing.Point)(resources.GetObject("label9.Location")));
			this.label9.Name = "label9";
			this.label9.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label9.RightToLeft")));
			this.label9.Size = ((System.Drawing.Size)(resources.GetObject("label9.Size")));
			this.label9.TabIndex = ((int)(resources.GetObject("label9.TabIndex")));
			this.label9.Text = resources.GetString("label9.Text");
			this.label9.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.TextAlign")));
			this.label9.Visible = ((bool)(resources.GetObject("label9.Visible")));
			// 
			// txtNewsAccountName
			// 
			this.txtNewsAccountName.AccessibleDescription = resources.GetString("txtNewsAccountName.AccessibleDescription");
			this.txtNewsAccountName.AccessibleName = resources.GetString("txtNewsAccountName.AccessibleName");
			this.txtNewsAccountName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtNewsAccountName.Anchor")));
			this.txtNewsAccountName.AutoSize = ((bool)(resources.GetObject("txtNewsAccountName.AutoSize")));
			this.txtNewsAccountName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtNewsAccountName.BackgroundImage")));
			this.txtNewsAccountName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtNewsAccountName.Dock")));
			this.txtNewsAccountName.Enabled = ((bool)(resources.GetObject("txtNewsAccountName.Enabled")));
			this.errorProvider.SetError(this.txtNewsAccountName, resources.GetString("txtNewsAccountName.Error"));
			this.txtNewsAccountName.Font = ((System.Drawing.Font)(resources.GetObject("txtNewsAccountName.Font")));
			this.errorProvider.SetIconAlignment(this.txtNewsAccountName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtNewsAccountName.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.txtNewsAccountName, ((int)(resources.GetObject("txtNewsAccountName.IconPadding"))));
			this.txtNewsAccountName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtNewsAccountName.ImeMode")));
			this.txtNewsAccountName.Location = ((System.Drawing.Point)(resources.GetObject("txtNewsAccountName.Location")));
			this.txtNewsAccountName.MaxLength = ((int)(resources.GetObject("txtNewsAccountName.MaxLength")));
			this.txtNewsAccountName.Multiline = ((bool)(resources.GetObject("txtNewsAccountName.Multiline")));
			this.txtNewsAccountName.Name = "txtNewsAccountName";
			this.txtNewsAccountName.PasswordChar = ((char)(resources.GetObject("txtNewsAccountName.PasswordChar")));
			this.txtNewsAccountName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtNewsAccountName.RightToLeft")));
			this.txtNewsAccountName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtNewsAccountName.ScrollBars")));
			this.txtNewsAccountName.Size = ((System.Drawing.Size)(resources.GetObject("txtNewsAccountName.Size")));
			this.txtNewsAccountName.TabIndex = ((int)(resources.GetObject("txtNewsAccountName.TabIndex")));
			this.txtNewsAccountName.Text = resources.GetString("txtNewsAccountName.Text");
			this.txtNewsAccountName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtNewsAccountName.TextAlign")));
			this.txtNewsAccountName.Visible = ((bool)(resources.GetObject("txtNewsAccountName.Visible")));
			this.txtNewsAccountName.WordWrap = ((bool)(resources.GetObject("txtNewsAccountName.WordWrap")));
			this.txtNewsAccountName.Validating += new System.ComponentModel.CancelEventHandler(this.OnWidgetValidating);
			this.txtNewsAccountName.Validated += new System.EventHandler(this.OnWidgetValidated);
			this.txtNewsAccountName.TextChanged += new System.EventHandler(this.OnNewsAccountNameTextChanged);
			// 
			// labelNewsAccount
			// 
			this.labelNewsAccount.AccessibleDescription = resources.GetString("labelNewsAccount.AccessibleDescription");
			this.labelNewsAccount.AccessibleName = resources.GetString("labelNewsAccount.AccessibleName");
			this.labelNewsAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelNewsAccount.Anchor")));
			this.labelNewsAccount.AutoSize = ((bool)(resources.GetObject("labelNewsAccount.AutoSize")));
			this.labelNewsAccount.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelNewsAccount.Dock")));
			this.labelNewsAccount.Enabled = ((bool)(resources.GetObject("labelNewsAccount.Enabled")));
			this.errorProvider.SetError(this.labelNewsAccount, resources.GetString("labelNewsAccount.Error"));
			this.labelNewsAccount.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelNewsAccount.Font = ((System.Drawing.Font)(resources.GetObject("labelNewsAccount.Font")));
			this.errorProvider.SetIconAlignment(this.labelNewsAccount, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("labelNewsAccount.IconAlignment"))));
			this.errorProvider.SetIconPadding(this.labelNewsAccount, ((int)(resources.GetObject("labelNewsAccount.IconPadding"))));
			this.labelNewsAccount.Image = ((System.Drawing.Image)(resources.GetObject("labelNewsAccount.Image")));
			this.labelNewsAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsAccount.ImageAlign")));
			this.labelNewsAccount.ImageIndex = ((int)(resources.GetObject("labelNewsAccount.ImageIndex")));
			this.labelNewsAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelNewsAccount.ImeMode")));
			this.labelNewsAccount.Location = ((System.Drawing.Point)(resources.GetObject("labelNewsAccount.Location")));
			this.labelNewsAccount.Name = "labelNewsAccount";
			this.labelNewsAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelNewsAccount.RightToLeft")));
			this.labelNewsAccount.Size = ((System.Drawing.Size)(resources.GetObject("labelNewsAccount.Size")));
			this.labelNewsAccount.TabIndex = ((int)(resources.GetObject("labelNewsAccount.TabIndex")));
			this.labelNewsAccount.Text = resources.GetString("labelNewsAccount.Text");
			this.labelNewsAccount.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelNewsAccount.TextAlign")));
			this.labelNewsAccount.Visible = ((bool)(resources.GetObject("labelNewsAccount.Visible")));
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			this.errorProvider.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProvider.Icon")));
			// 
			// _NewsgroupsConfiguration_Toolbars_Dock_Area_Left
			// 
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.AccessibleDescription = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.AccessibleDescription");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.AccessibleName = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.AccessibleName");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Anchor")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.BackColor = System.Drawing.SystemColors.Window;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.BackgroundImage")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Left;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Enabled = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Enabled")));
			this.errorProvider.SetError(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left, resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Error"));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.ForeColor = System.Drawing.SystemColors.ControlText;
			this.errorProvider.SetIconAlignment(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.IconAlignment"))));
			this.errorProvider.SetIconPadding(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left, ((int)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.IconPadding"))));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.ImeMode")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Location = ((System.Drawing.Point)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Location")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Name = "_NewsgroupsConfiguration_Toolbars_Dock_Area_Left";
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.RightToLeft")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Size = ((System.Drawing.Size)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Size")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Text = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Text");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.ToolbarsManager = this.ultraToolbarsManager;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Visible = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Left.Visible")));
			// 
			// ultraToolbarsManager
			// 
			this.ultraToolbarsManager.DesignerFlags = 1;
			this.ultraToolbarsManager.DockWithinContainer = this;
			this.ultraToolbarsManager.ShowFullMenusDelay = 500;
			// 
			// _NewsgroupsConfiguration_Toolbars_Dock_Area_Right
			// 
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.AccessibleDescription = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.AccessibleDescription");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.AccessibleName = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.AccessibleName");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Anchor")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.BackColor = System.Drawing.SystemColors.Window;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.BackgroundImage")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Right;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Enabled = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Enabled")));
			this.errorProvider.SetError(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right, resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Error"));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.ForeColor = System.Drawing.SystemColors.ControlText;
			this.errorProvider.SetIconAlignment(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.IconAlignment"))));
			this.errorProvider.SetIconPadding(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right, ((int)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.IconPadding"))));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.ImeMode")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Location = ((System.Drawing.Point)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Location")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Name = "_NewsgroupsConfiguration_Toolbars_Dock_Area_Right";
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.RightToLeft")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Size = ((System.Drawing.Size)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Size")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Text = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Text");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.ToolbarsManager = this.ultraToolbarsManager;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Visible = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Right.Visible")));
			// 
			// _NewsgroupsConfiguration_Toolbars_Dock_Area_Top
			// 
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.AccessibleDescription = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.AccessibleDescription");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.AccessibleName = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.AccessibleName");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Anchor")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.BackColor = System.Drawing.SystemColors.Window;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.BackgroundImage")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Top;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Enabled = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Enabled")));
			this.errorProvider.SetError(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top, resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Error"));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.ForeColor = System.Drawing.SystemColors.ControlText;
			this.errorProvider.SetIconAlignment(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.IconAlignment"))));
			this.errorProvider.SetIconPadding(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top, ((int)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.IconPadding"))));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.ImeMode")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Location = ((System.Drawing.Point)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Location")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Name = "_NewsgroupsConfiguration_Toolbars_Dock_Area_Top";
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.RightToLeft")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Size = ((System.Drawing.Size)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Size")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Text = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Text");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.ToolbarsManager = this.ultraToolbarsManager;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Visible = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Top.Visible")));
			// 
			// _NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom
			// 
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.AccessibleDescription = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.AccessibleDescription");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.AccessibleName = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.AccessibleName");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Anchor")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.BackColor = System.Drawing.SystemColors.Window;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.BackgroundImage")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.DockedPosition = Infragistics.Win.UltraWinToolbars.DockedPosition.Bottom;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Enabled = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Enabled")));
			this.errorProvider.SetError(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom, resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Error"));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.ForeColor = System.Drawing.SystemColors.ControlText;
			this.errorProvider.SetIconAlignment(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.IconAlignment"))));
			this.errorProvider.SetIconPadding(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom, ((int)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.IconPadding"))));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.ImeMode")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Location = ((System.Drawing.Point)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Location")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Name = "_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom";
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.RightToLeft")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Size = ((System.Drawing.Size)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Size")));
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Text = resources.GetString("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Text");
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.ToolbarsManager = this.ultraToolbarsManager;
			this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Visible = ((bool)(resources.GetObject("_NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom.Visible")));
			// 
			// timerFilterGroups
			// 
			this.timerFilterGroups.Interval = 250;
			this.timerFilterGroups.Tick += new System.EventHandler(this.OnFilterNewsGroupsTick);
			// 
			// NewsgroupsConfiguration
			// 
			this.AcceptButton = this.btnOK;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.panelDetailsParent);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.navigationBar);
			this.Controls.Add(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Left);
			this.Controls.Add(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Right);
			this.Controls.Add(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Top);
			this.Controls.Add(this._NewsgroupsConfiguration_Toolbars_Dock_Area_Bottom);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "NewsgroupsConfiguration";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.navigationBar.ResumeLayout(false);
			this.newsServersPane.ResumeLayout(false);
			this.accountsPane.ResumeLayout(false);
			this.accountSettingsPane.ResumeLayout(false);
			this.panelDetailsParent.ResumeLayout(false);
			this.panelDetailsTop.ResumeLayout(false);
			this.subscriptionsViewPane.ResumeLayout(false);
			this.advancedViewPane.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackBarServerTimeout)).EndInit();
			this.serverViewPane.ResumeLayout(false);
			this.generalViewPane.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ultraToolbarsManager)).EndInit();
			this.ResumeLayout(false);

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
		
		private void InitializeWidgets(IDictionary<string, UserIdentity> userIdentities, IDictionary<string, INntpServerDefinition> nntpServers) {
			this.treeServers.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeServersAfterSelect);
			this.listAccounts.SelectedIndexChanged -= new System.EventHandler(this.OnListAccountsSelectedIndexChanged);

			if (userIdentities != null) {
				foreach (UserIdentity ui in userIdentities.Values) {
					this.AddNewIdentity((UserIdentity)ui.Clone());
				}
			}
			if (nntpServers != null) {
				foreach (NntpServerDefinition sd in nntpServers.Values) {
					this.AddNewsServer((NntpServerDefinition)sd.Clone());
				}	
			}

			PopulateDefaultIdentities();

			this.treeServers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeServersAfterSelect);
			this.listAccounts.SelectedIndexChanged += new System.EventHandler(this.OnListAccountsSelectedIndexChanged);
		}

		private void InitializeToolbars() {
			this.toolbarHelper = new ToolbarHelper(this.ultraToolbarsManager);
			this.ultraToolbarsManager.Style = Infragistics.Win.UltraWinToolbars.ToolbarStyle.Office2003;
			this.ultraToolbarsManager.ToolClick += new ToolClickEventHandler(this.OnToolbarToolClick);
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
				
				this.navigationBar.SelectedPaneChanged -= new System.EventHandler(this.OnNavigationBarSelectedPaneChanged);
				if (detail == NewsgroupSettingsView.Identity) {
					this.Text = SR.ConfigIdentitiesDialogCaption;
					if (navigationBar.SelectedPane != accountsPane)
						navigationBar.SelectedPane = accountsPane;
				} else {
					this.Text = SR.ConfigNewsServerDialogCaption;
					if (navigationBar.SelectedPane != newsServersPane)
						navigationBar.SelectedPane = newsServersPane;
				}
				this.navigationBar.SelectedPaneChanged += new System.EventHandler(this.OnNavigationBarSelectedPaneChanged);

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
				} while(this.userIdentities.Contains(ui.Name + " #" + unnamedUserIdentityCounter.ToString()));
				ui.Name += " #" + unnamedUserIdentityCounter.ToString();
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
				} while(this.nntpServers.Contains(sd.Name + " #" + unnamedNntpServerCounter.ToString()));
				sd.Name += " #" + unnamedNntpServerCounter.ToString();
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
				string u = sd.AuthUser, p = null;
				NewsHandler.GetNntpServerCredentials(sd, ref u, ref p);
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
					ICoreApplication coreApp = (ICoreApplication)this.application.GetService(typeof(ICoreApplication));
			
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

		private void OnNavigationBarSelectedPaneChanged(object sender, System.EventArgs e) {
			if (this.navigationBar.SelectedPane == this.newsServersPane) {
				this.ActivateView(NewsgroupSettingsView.NewsServerSubscriptions);
				this.OnTreeServersAfterSelect(treeServers, new TreeViewEventArgs(treeServers.SelectedNode)); 
			} else if (this.navigationBar.SelectedPane == this.accountsPane) {
				this.ActivateView(NewsgroupSettingsView.Identity);
				this.OnListAccountsSelectedIndexChanged(listAccounts, EventArgs.Empty);
			}
		}

		private void OnNewIdentityToolActivate(object sender, System.EventArgs e) {
			this.ActivateView(NewsgroupSettingsView.Identity);
			this.AddNewIdentity();
		}

		private void OnNewNewsServerToolActivate(object sender, System.EventArgs e) {
			this.ActivateView(NewsgroupSettingsView.NewsServerGeneral);
			this.AddNewsServer();
		}

		private void OnDeleteItemToolActivate(object sender, System.EventArgs e) {
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
					NewsHandler.SetNntpServerCredentials(selectedServer, this.txtServerAuthName.Text, txtServerAuthPassword.Text);
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
					if (Object.ReferenceEquals(ui, me) == false && ui.Name.Equals(txtIdentityName.Text)) {
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

		private void btnApply_Click(object sender, System.EventArgs e) {
			
			// raise event to allow take over the items from local userIdentities and nntpServers 
			// to app.FeedHandler.Identity and app.FeedHandler.NntpServers
			if (DefinitionsModified != null)
				DefinitionsModified(this, EventArgs.Empty);

			this.btnApply.Enabled = false;
		}

		private void btnOK_Click(object sender, System.EventArgs e) {
			this.Hide();
			this.btnApply_Click(sender, e);
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void OnFormLoad(object sender, System.EventArgs e) {
			this.InitializeToolbars();
			this.InitializeWidgets(application.CurrentIdentities, application.CurrentNntpServers); 
			this.ActivateView(this.currentView);
			if (currentView == NewsgroupSettingsView.Identity) {
				if (listAccounts.SelectedItems.Count > 0)
					this.OnListAccountsSelectedIndexChanged(listAccounts, EventArgs.Empty);
			} else {
				if (treeServers.SelectedNode != null)
					this.OnTreeServersAfterSelect(treeServers, new TreeViewEventArgs(treeServers.SelectedNode));
			}
		}

		private void btnSubscribe_Click(object sender, System.EventArgs e) {
			this.DoSubscribe();
		}

		private void btnUnsubscribe_Click(object sender, System.EventArgs e) {
			//TODO
		}

		private void btnRefreshGroupList_Click(object sender, System.EventArgs e) {
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

		private void OnFilterByTextChanged(object sender, System.EventArgs e) {
			this.timerFilterGroups.Enabled = true;
		}

		private void OnFilterNewsGroupsTick(object sender, System.EventArgs e) {
			this.timerFilterGroups.Enabled = false;
			this.DoFilterNewsGroups(txtFilterBy.Text);
		}

		private void OnListOfGroupsDoubleClick(object sender, System.EventArgs e)
		{
			this.DoSubscribe();
		}
		
		private void OnToolbarToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e) {
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
