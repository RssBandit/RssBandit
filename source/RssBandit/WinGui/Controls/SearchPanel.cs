#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinExplorerBar;
using NewsComponents;
using NewsComponents.Search;
using RssBandit.Resources;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Controls
{
	
	/// <summary>
	/// Enum defines the news item search states
	/// </summary>
	public enum ItemSearchState {
		Pending,		// no search running or canceling
		Searching,		// search in progress
		Canceled,		// search canceled in UI but not worker
		Failure,		// invalid search expression, or other exception
		Finished,		// search finished
	}
	
	/// <summary>
	/// SearchPanel. Handles all local search options.
	/// Is also used to display the current settings of a 
	/// persisted search.
	/// </summary>
	public class SearchPanel : System.Windows.Forms.UserControl 
	{
		public event NewsItemSearchCancelEventHandler BeforeNewsItemSearch;
		public event NewsItemSearchEventHandler NewsItemSearch;
		
		private bool _isAdvancedOptionReadStatusActive = true;
		private bool _isAdvancedOptionItemAgeActive = true;
		private bool _isAdvancedOptionItemPostedActive = true;
		// current rss search state
		private ItemSearchState _rssSearchState = ItemSearchState.Pending;
		
		private TreeFeedsNodeBase externalScopeRootNode;
		
		private System.Windows.Forms.Panel panelRssSearchCommands;
		private System.Windows.Forms.Button btnNewSearch;
		private System.Windows.Forms.TextBox textSearchExpression;
		private System.Windows.Forms.Button btnSearchCancel;
		private System.Windows.Forms.Label labelRssSearchState;
		private System.Windows.Forms.Button btnRssSearchSave;
		private System.Windows.Forms.TextBox textFinderCaption;
		private Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl taskPaneSaveOptionsContainer;
		private System.Windows.Forms.Label labelSearchFieldsHint;
		private Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl taskPaneSearchFieldsContainer;
		private Infragistics.Win.UltraWinExplorerBar.UltraExplorerBar taskPaneAllSearchOptions;
		private Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl taskPaneAdvancedOptionsContainer;
		private Infragistics.Win.Misc.UltraExpandableGroupBox advancedOptionReadStatusGroup;
		private Infragistics.Win.UltraWinEditors.UltraCheckEditor checkBoxRssSearchUnreadItems;
		private Infragistics.Win.Misc.UltraExpandableGroupBoxPanel advancedOptionReadStatusGroupPanel;
		private Infragistics.Win.UltraWinEditors.UltraCheckEditor checkBoxRssSearchInTitle;
		private Infragistics.Win.UltraWinEditors.UltraCheckEditor checkBoxRssSearchInDesc;
		private Infragistics.Win.UltraWinEditors.UltraCheckEditor checkBoxRssSearchInCategory;
		private Infragistics.Win.UltraWinEditors.UltraCheckEditor checkBoxRssSearchInLink;
		private System.Windows.Forms.ComboBox comboRssSearchItemAge;
		private Infragistics.Win.UltraWinEditors.UltraOptionSet optionSetItemAge;
		private Infragistics.Win.Misc.UltraExpandableGroupBox advancedOptionItemAgeGroup;
		private System.Windows.Forms.ComboBox comboBoxRssSearchItemPostedOperator;
		private System.Windows.Forms.DateTimePicker dateTimeRssSearchItemPost;
		private Infragistics.Win.Misc.UltraExpandableGroupBox advancedOptionItemPostedGroup;
		private System.Windows.Forms.DateTimePicker dateTimeRssSearchPostBefore;
		private Infragistics.Win.Misc.UltraExpandableGroupBoxPanel advancedOptionItemPostedGroupPanel;
		private Infragistics.Win.Misc.UltraExpandableGroupBoxPanel advancedOptionItemAgeGroupPanel;
		private Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl taskPaneScopeContainer;
		private System.Windows.Forms.TreeView treeRssSearchScope;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		internal SearchPanel() {
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			InitializeComponentTranslationAndUI();
			InitializeEvents();
			ResetControlState();
		}
		
		private void InitializeComponentTranslationAndUI() {
			
			UltraExplorerBarGroupSettings settings = this.taskPaneAllSearchOptions.Groups["searchTaskPaneScope"].Settings;
			settings.ItemAreaInnerMargins.Left = 0;
			settings.ItemAreaInnerMargins.Right = 0;
			settings.ItemAreaInnerMargins.Top = 1;
			settings.ItemAreaInnerMargins.Bottom = 0;
			
			// localization:
			this.btnRssSearchSave.Text = SR.SearchPanel_btnRssSearchSave_Text;
			this.checkBoxRssSearchInLink.Text = SR.SearchPanel_checkBoxRssSearchInLink_Text;
			this.checkBoxRssSearchInCategory.Text = SR.SearchPanel_checkBoxRssSearchInCategory_Text;
			this.checkBoxRssSearchInDesc.Text = SR.SearchPanel_checkBoxRssSearchInDesc_Text;
			this.checkBoxRssSearchInTitle.Text = SR.SearchPanel_checkBoxRssSearchInTitle_Text;
			this.labelSearchFieldsHint.Text = SR.SearchPanel_labelSearchFieldsHint_Text;
			this.advancedOptionItemPostedGroup.Text = SR.SearchPanel_advancedOptionItemPostedGroup_Text;
			this.advancedOptionItemAgeGroup.Text = SR.SearchPanel_advancedOptionItemAgeGroup_Text;
			this.optionSetItemAge.Text = SR.SearchPanel_optionSetItemAge_Text;
			this.optionSetItemAge.Items[0].DisplayText = SR.SearchPanel_optionSetItemAge_Text;
			this.optionSetItemAge.Items[1].DisplayText = SR.SearchPanel_optionSetItemAgeOlderThan_Text;
			this.advancedOptionReadStatusGroup.Text = SR.SearchPanel_advancedOptionReadStatusGroup_Text;
			this.checkBoxRssSearchUnreadItems.Text = SR.SearchPanel_checkBoxRssSearchUnreadItems_Text;
			this.btnNewSearch.Text = SR.SearchPanel_btnNewSearch_Text;
			this.btnSearchCancel.Text = SR.SearchPanel_btnSearch_Text;

			this.taskPaneAllSearchOptions.Groups["searchTaskPaneSaveOptions"].Text = SR.SearchPanel_searchTaskPaneSaveOptions_Text;
			this.taskPaneAllSearchOptions.Groups["searchTaskPaneFields"].Text = SR.SearchPanel_searchTaskPaneFields_Text;
			this.taskPaneAllSearchOptions.Groups["searchTaskPaneAdvancedOptions"].Text = SR.SearchPanel_searchTaskPaneAdvancedOptions_Text;
			this.taskPaneAllSearchOptions.Groups["searchTaskPaneScope"].Text = SR.SearchPanel_searchTaskPaneScope_Text;
			
			this.comboBoxRssSearchItemPostedOperator.Items.Clear();
			this.comboBoxRssSearchItemPostedOperator.Items.AddRange(
				new string[] {
								 SR.SearchPanel_comboBoxRssSearchItemPostedOperator_at,
								 SR.SearchPanel_comboBoxRssSearchItemPostedOperator_before,
								 SR.SearchPanel_comboBoxRssSearchItemPostedOperator_after,
								 SR.SearchPanel_comboBoxRssSearchItemPostedOperator_between
							 });
			
			this.comboRssSearchItemAge.Items.Clear();
			for (int i = 0; i <= 25; i++)
				this.comboRssSearchItemAge.Items.Add(Utils.MapRssSearchItemAgeString(i));

			// enable native info tips support:
			Win32.ModifyWindowStyle(treeRssSearchScope.Handle, 0, Win32.TVS_INFOTIP);
			this.treeRssSearchScope.PathSeparator = NewsHandler.CategorySeparator;
			
			this.BackColor = FontColorHelper.UiColorScheme.TaskPaneNavigationArea;
		}

		private void InitializeEvents() {
			this.advancedOptionReadStatusGroup.ExpandedStateChanging += new CancelEventHandler(this.OnAdvancedOptionReadStatusExpandedStateChanging);
			this.advancedOptionReadStatusGroup.Click += new EventHandler(OnAdvancedOptionReadStatusExpandableGroupClick);
			this.advancedOptionItemAgeGroup.ExpandedStateChanging += new CancelEventHandler(this.OnAdvancedOptionItemAgeExpandedStateChanging);
			this.advancedOptionItemAgeGroup.Click += new EventHandler(OnAdvancedOptionItemAgeExpandableGroupClick);
			this.advancedOptionItemPostedGroup.ExpandedStateChanging += new CancelEventHandler(this.OnAdvancedOptionItemPostedExpandedStateChanging);
			this.advancedOptionItemPostedGroup.Click += new EventHandler(OnAdvancedOptionItemPostedExpandableGroupClick);
			
			this.comboBoxRssSearchItemPostedOperator.SelectedIndexChanged += new EventHandler(OnItemPostedOperatorSelectedIndexChanged);
			this.btnSearchCancel.Click += new EventHandler(this.OnSearchCancelClick);
			this.btnNewSearch.Click += new EventHandler(this.OnNewSearchClick);
			this.btnRssSearchSave.Click += new EventHandler(this.OnSearchCancelClick);

			this.textSearchExpression.TextChanged += new System.EventHandler(this.OnSearchExpressionChanged);
			this.textFinderCaption.TextChanged += new System.EventHandler(this.OnSearchFinderCaptionChanged);
			this.textSearchExpression.KeyDown += new KeyEventHandler(this.OnSearchExpressionKeyDown);
			this.textSearchExpression.KeyPress += new KeyPressEventHandler(this.OnAnyEnterKeyPress);
		
			this.treeRssSearchScope.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.OnRssSearchScopeTreeAfterCheck);

		}


		private void InitializeStates() {
			this.IsAdvancedOptionReadStatusActive = false;
			this.IsAdvancedOptionItemAgeActive = false;
			this.IsAdvancedOptionItemPostedActive = false;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SearchPanel));
			Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
			Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
			Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
			Infragistics.Win.ValueListItem valueListItem1 = new Infragistics.Win.ValueListItem();
			Infragistics.Win.ValueListItem valueListItem2 = new Infragistics.Win.ValueListItem();
			Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
			Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup ultraExplorerBarGroup1 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
			Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup ultraExplorerBarGroup2 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
			Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup ultraExplorerBarGroup3 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
			Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup ultraExplorerBarGroup4 = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup();
			this.taskPaneSaveOptionsContainer = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
			this.btnRssSearchSave = new System.Windows.Forms.Button();
			this.textFinderCaption = new System.Windows.Forms.TextBox();
			this.taskPaneSearchFieldsContainer = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
			this.checkBoxRssSearchInLink = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
			this.checkBoxRssSearchInCategory = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
			this.checkBoxRssSearchInDesc = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
			this.checkBoxRssSearchInTitle = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
			this.labelSearchFieldsHint = new System.Windows.Forms.Label();
			this.taskPaneAdvancedOptionsContainer = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
			this.advancedOptionItemPostedGroup = new Infragistics.Win.Misc.UltraExpandableGroupBox();
			this.advancedOptionItemPostedGroupPanel = new Infragistics.Win.Misc.UltraExpandableGroupBoxPanel();
			this.dateTimeRssSearchPostBefore = new System.Windows.Forms.DateTimePicker();
			this.comboBoxRssSearchItemPostedOperator = new System.Windows.Forms.ComboBox();
			this.dateTimeRssSearchItemPost = new System.Windows.Forms.DateTimePicker();
			this.advancedOptionItemAgeGroup = new Infragistics.Win.Misc.UltraExpandableGroupBox();
			this.advancedOptionItemAgeGroupPanel = new Infragistics.Win.Misc.UltraExpandableGroupBoxPanel();
			this.optionSetItemAge = new Infragistics.Win.UltraWinEditors.UltraOptionSet();
			this.comboRssSearchItemAge = new System.Windows.Forms.ComboBox();
			this.advancedOptionReadStatusGroup = new Infragistics.Win.Misc.UltraExpandableGroupBox();
			this.advancedOptionReadStatusGroupPanel = new Infragistics.Win.Misc.UltraExpandableGroupBoxPanel();
			this.checkBoxRssSearchUnreadItems = new Infragistics.Win.UltraWinEditors.UltraCheckEditor();
			this.taskPaneScopeContainer = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarContainerControl();
			this.treeRssSearchScope = new System.Windows.Forms.TreeView();
			this.panelRssSearchCommands = new System.Windows.Forms.Panel();
			this.btnNewSearch = new System.Windows.Forms.Button();
			this.textSearchExpression = new System.Windows.Forms.TextBox();
			this.btnSearchCancel = new System.Windows.Forms.Button();
			this.labelRssSearchState = new System.Windows.Forms.Label();
			this.taskPaneAllSearchOptions = new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBar();
			this.taskPaneSaveOptionsContainer.SuspendLayout();
			this.taskPaneSearchFieldsContainer.SuspendLayout();
			this.taskPaneAdvancedOptionsContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.advancedOptionItemPostedGroup)).BeginInit();
			this.advancedOptionItemPostedGroup.SuspendLayout();
			this.advancedOptionItemPostedGroupPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.advancedOptionItemAgeGroup)).BeginInit();
			this.advancedOptionItemAgeGroup.SuspendLayout();
			this.advancedOptionItemAgeGroupPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.optionSetItemAge)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.advancedOptionReadStatusGroup)).BeginInit();
			this.advancedOptionReadStatusGroup.SuspendLayout();
			this.advancedOptionReadStatusGroupPanel.SuspendLayout();
			this.taskPaneScopeContainer.SuspendLayout();
			this.panelRssSearchCommands.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.taskPaneAllSearchOptions)).BeginInit();
			this.taskPaneAllSearchOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// taskPaneSaveOptionsContainer
			// 
			this.taskPaneSaveOptionsContainer.Controls.Add(this.btnRssSearchSave);
			this.taskPaneSaveOptionsContainer.Controls.Add(this.textFinderCaption);
			this.taskPaneSaveOptionsContainer.Location = new System.Drawing.Point(23, 49);
			this.taskPaneSaveOptionsContainer.Name = "taskPaneSaveOptionsContainer";
			this.taskPaneSaveOptionsContainer.Size = new System.Drawing.Size(206, 26);
			this.taskPaneSaveOptionsContainer.TabIndex = 0;
			// 
			// btnRssSearchSave
			// 
			this.btnRssSearchSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRssSearchSave.BackColor = System.Drawing.SystemColors.Control;
			this.btnRssSearchSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRssSearchSave.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnRssSearchSave.Location = new System.Drawing.Point(127, 0);
			this.btnRssSearchSave.Name = "btnRssSearchSave";
			this.btnRssSearchSave.TabIndex = 1;
			this.btnRssSearchSave.Text = "Save";
			// 
			// textFinderCaption
			// 
			this.textFinderCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textFinderCaption.Location = new System.Drawing.Point(0, 2);
			this.textFinderCaption.Name = "textFinderCaption";
			this.textFinderCaption.Size = new System.Drawing.Size(122, 20);
			this.textFinderCaption.TabIndex = 0;
			this.textFinderCaption.Text = "";
			// 
			// taskPaneSearchFieldsContainer
			// 
			this.taskPaneSearchFieldsContainer.Controls.Add(this.checkBoxRssSearchInLink);
			this.taskPaneSearchFieldsContainer.Controls.Add(this.checkBoxRssSearchInCategory);
			this.taskPaneSearchFieldsContainer.Controls.Add(this.checkBoxRssSearchInDesc);
			this.taskPaneSearchFieldsContainer.Controls.Add(this.checkBoxRssSearchInTitle);
			this.taskPaneSearchFieldsContainer.Controls.Add(this.labelSearchFieldsHint);
			this.taskPaneSearchFieldsContainer.Location = new System.Drawing.Point(23, 134);
			this.taskPaneSearchFieldsContainer.Name = "taskPaneSearchFieldsContainer";
			this.taskPaneSearchFieldsContainer.Size = new System.Drawing.Size(206, 116);
			this.taskPaneSearchFieldsContainer.TabIndex = 1;
			// 
			// checkBoxRssSearchInLink
			// 
			this.checkBoxRssSearchInLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxRssSearchInLink.BackColor = System.Drawing.Color.Transparent;
			this.checkBoxRssSearchInLink.Location = new System.Drawing.Point(0, 60);
			this.checkBoxRssSearchInLink.Name = "checkBoxRssSearchInLink";
			this.checkBoxRssSearchInLink.Size = new System.Drawing.Size(202, 20);
			this.checkBoxRssSearchInLink.TabIndex = 3;
			this.checkBoxRssSearchInLink.Text = "Item &Link";
			this.checkBoxRssSearchInLink.UseMnemonics = true;
			// 
			// checkBoxRssSearchInCategory
			// 
			this.checkBoxRssSearchInCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxRssSearchInCategory.BackColor = System.Drawing.Color.Transparent;
			this.checkBoxRssSearchInCategory.Location = new System.Drawing.Point(0, 40);
			this.checkBoxRssSearchInCategory.Name = "checkBoxRssSearchInCategory";
			this.checkBoxRssSearchInCategory.Size = new System.Drawing.Size(202, 20);
			this.checkBoxRssSearchInCategory.TabIndex = 2;
			this.checkBoxRssSearchInCategory.Text = "Top&ic";
			this.checkBoxRssSearchInCategory.UseMnemonics = true;
			// 
			// checkBoxRssSearchInDesc
			// 
			this.checkBoxRssSearchInDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxRssSearchInDesc.BackColor = System.Drawing.Color.Transparent;
			this.checkBoxRssSearchInDesc.Location = new System.Drawing.Point(0, 20);
			this.checkBoxRssSearchInDesc.Name = "checkBoxRssSearchInDesc";
			this.checkBoxRssSearchInDesc.Size = new System.Drawing.Size(202, 20);
			this.checkBoxRssSearchInDesc.TabIndex = 1;
			this.checkBoxRssSearchInDesc.Text = "&Description";
			this.checkBoxRssSearchInDesc.UseMnemonics = true;
			// 
			// checkBoxRssSearchInTitle
			// 
			this.checkBoxRssSearchInTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxRssSearchInTitle.BackColor = System.Drawing.Color.Transparent;
			this.checkBoxRssSearchInTitle.Location = new System.Drawing.Point(0, 0);
			this.checkBoxRssSearchInTitle.Name = "checkBoxRssSearchInTitle";
			this.checkBoxRssSearchInTitle.Size = new System.Drawing.Size(202, 20);
			this.checkBoxRssSearchInTitle.TabIndex = 0;
			this.checkBoxRssSearchInTitle.Text = "&Title";
			this.checkBoxRssSearchInTitle.UseMnemonics = true;
			// 
			// labelSearchFieldsHint
			// 
			this.labelSearchFieldsHint.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.labelSearchFieldsHint.BackColor = System.Drawing.Color.Transparent;
			this.labelSearchFieldsHint.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelSearchFieldsHint.Location = new System.Drawing.Point(5, 85);
			this.labelSearchFieldsHint.Name = "labelSearchFieldsHint";
			this.labelSearchFieldsHint.Size = new System.Drawing.Size(190, 40);
			this.labelSearchFieldsHint.TabIndex = 9;
			this.labelSearchFieldsHint.Text = "Check all the fields you want searched";
			// 
			// taskPaneAdvancedOptionsContainer
			// 
			this.taskPaneAdvancedOptionsContainer.Controls.Add(this.advancedOptionItemPostedGroup);
			this.taskPaneAdvancedOptionsContainer.Controls.Add(this.advancedOptionItemAgeGroup);
			this.taskPaneAdvancedOptionsContainer.Controls.Add(this.advancedOptionReadStatusGroup);
			this.taskPaneAdvancedOptionsContainer.Location = new System.Drawing.Point(28, 148);
			this.taskPaneAdvancedOptionsContainer.Name = "taskPaneAdvancedOptionsContainer";
			this.taskPaneAdvancedOptionsContainer.Size = new System.Drawing.Size(179, 0);
			this.taskPaneAdvancedOptionsContainer.TabIndex = 2;
			this.taskPaneAdvancedOptionsContainer.Visible = false;
			// 
			// advancedOptionItemPostedGroup
			// 
			this.advancedOptionItemPostedGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			appearance1.BackColor = System.Drawing.Color.Transparent;
			this.advancedOptionItemPostedGroup.Appearance = appearance1;
			this.advancedOptionItemPostedGroup.ContentPadding.Left = 16;
			this.advancedOptionItemPostedGroup.Controls.Add(this.advancedOptionItemPostedGroupPanel);
			this.advancedOptionItemPostedGroup.ExpandedSize = new System.Drawing.Size(126, 55);
			this.advancedOptionItemPostedGroup.ExpansionIndicatorCollapsed = ((System.Drawing.Image)(resources.GetObject("advancedOptionItemPostedGroup.ExpansionIndicatorCollapsed")));
			this.advancedOptionItemPostedGroup.ExpansionIndicatorExpanded = ((System.Drawing.Image)(resources.GetObject("advancedOptionItemPostedGroup.ExpansionIndicatorExpanded")));
			this.advancedOptionItemPostedGroup.Location = new System.Drawing.Point(2, 175);
			this.advancedOptionItemPostedGroup.Name = "advancedOptionItemPostedGroup";
			this.advancedOptionItemPostedGroup.Size = new System.Drawing.Size(175, 105);
			this.advancedOptionItemPostedGroup.TabIndex = 2;
			this.advancedOptionItemPostedGroup.Text = "Item &posted";
			// 
			// advancedOptionItemPostedGroupPanel
			// 
			this.advancedOptionItemPostedGroupPanel.Controls.Add(this.dateTimeRssSearchPostBefore);
			this.advancedOptionItemPostedGroupPanel.Controls.Add(this.comboBoxRssSearchItemPostedOperator);
			this.advancedOptionItemPostedGroupPanel.Controls.Add(this.dateTimeRssSearchItemPost);
			this.advancedOptionItemPostedGroupPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.advancedOptionItemPostedGroupPanel.Location = new System.Drawing.Point(19, 19);
			this.advancedOptionItemPostedGroupPanel.Name = "advancedOptionItemPostedGroupPanel";
			this.advancedOptionItemPostedGroupPanel.Size = new System.Drawing.Size(153, 83);
			this.advancedOptionItemPostedGroupPanel.TabIndex = 0;
			// 
			// dateTimeRssSearchPostBefore
			// 
			this.dateTimeRssSearchPostBefore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dateTimeRssSearchPostBefore.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimeRssSearchPostBefore.Location = new System.Drawing.Point(0, 60);
			this.dateTimeRssSearchPostBefore.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
			this.dateTimeRssSearchPostBefore.Name = "dateTimeRssSearchPostBefore";
			this.dateTimeRssSearchPostBefore.Size = new System.Drawing.Size(145, 20);
			this.dateTimeRssSearchPostBefore.TabIndex = 12;
			// 
			// comboBoxRssSearchItemPostedOperator
			// 
			this.comboBoxRssSearchItemPostedOperator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxRssSearchItemPostedOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxRssSearchItemPostedOperator.ItemHeight = 13;
			this.comboBoxRssSearchItemPostedOperator.Items.AddRange(new object[] {
																					 "at",
																					 "before",
																					 "after",
																					 "between"});
			this.comboBoxRssSearchItemPostedOperator.Location = new System.Drawing.Point(0, 5);
			this.comboBoxRssSearchItemPostedOperator.Name = "comboBoxRssSearchItemPostedOperator";
			this.comboBoxRssSearchItemPostedOperator.Size = new System.Drawing.Size(145, 21);
			this.comboBoxRssSearchItemPostedOperator.TabIndex = 9;
			// 
			// dateTimeRssSearchItemPost
			// 
			this.dateTimeRssSearchItemPost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dateTimeRssSearchItemPost.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimeRssSearchItemPost.Location = new System.Drawing.Point(0, 35);
			this.dateTimeRssSearchItemPost.MinDate = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
			this.dateTimeRssSearchItemPost.Name = "dateTimeRssSearchItemPost";
			this.dateTimeRssSearchItemPost.Size = new System.Drawing.Size(145, 20);
			this.dateTimeRssSearchItemPost.TabIndex = 10;
			// 
			// advancedOptionItemAgeGroup
			// 
			this.advancedOptionItemAgeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			appearance2.BackColor = System.Drawing.Color.Transparent;
			this.advancedOptionItemAgeGroup.Appearance = appearance2;
			this.advancedOptionItemAgeGroup.ContentPadding.Left = 16;
			this.advancedOptionItemAgeGroup.Controls.Add(this.advancedOptionItemAgeGroupPanel);
			this.advancedOptionItemAgeGroup.ExpandedSize = new System.Drawing.Size(126, 55);
			this.advancedOptionItemAgeGroup.ExpansionIndicatorCollapsed = ((System.Drawing.Image)(resources.GetObject("advancedOptionItemAgeGroup.ExpansionIndicatorCollapsed")));
			this.advancedOptionItemAgeGroup.ExpansionIndicatorExpanded = ((System.Drawing.Image)(resources.GetObject("advancedOptionItemAgeGroup.ExpansionIndicatorExpanded")));
			this.advancedOptionItemAgeGroup.Location = new System.Drawing.Point(2, 65);
			this.advancedOptionItemAgeGroup.Name = "advancedOptionItemAgeGroup";
			this.advancedOptionItemAgeGroup.Size = new System.Drawing.Size(175, 100);
			this.advancedOptionItemAgeGroup.TabIndex = 1;
			this.advancedOptionItemAgeGroup.Text = "Item &Age";
			// 
			// advancedOptionItemAgeGroupPanel
			// 
			this.advancedOptionItemAgeGroupPanel.Controls.Add(this.optionSetItemAge);
			this.advancedOptionItemAgeGroupPanel.Controls.Add(this.comboRssSearchItemAge);
			this.advancedOptionItemAgeGroupPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.advancedOptionItemAgeGroupPanel.Location = new System.Drawing.Point(19, 19);
			this.advancedOptionItemAgeGroupPanel.Name = "advancedOptionItemAgeGroupPanel";
			this.advancedOptionItemAgeGroupPanel.Size = new System.Drawing.Size(153, 78);
			this.advancedOptionItemAgeGroupPanel.TabIndex = 0;
			// 
			// optionSetItemAge
			// 
			this.optionSetItemAge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			appearance3.BackColorDisabled = System.Drawing.Color.Transparent;
			this.optionSetItemAge.Appearance = appearance3;
			this.optionSetItemAge.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
			this.optionSetItemAge.CheckedIndex = 0;
			appearance4.BackColorDisabled = System.Drawing.Color.Transparent;
			this.optionSetItemAge.ItemAppearance = appearance4;
			valueListItem1.DataValue = "";
			valueListItem1.DisplayText = "&Newer than";
			valueListItem2.DataValue = "";
			valueListItem2.DisplayText = "&Older than";
			this.optionSetItemAge.Items.Add(valueListItem1);
			this.optionSetItemAge.Items.Add(valueListItem2);
			this.optionSetItemAge.ItemSpacingVertical = 5;
			this.optionSetItemAge.Location = new System.Drawing.Point(0, 5);
			this.optionSetItemAge.Name = "optionSetItemAge";
			this.optionSetItemAge.Size = new System.Drawing.Size(144, 40);
			this.optionSetItemAge.TabIndex = 0;
			this.optionSetItemAge.Text = "&Newer than";
			this.optionSetItemAge.UseFlatMode = Infragistics.Win.DefaultableBoolean.True;
			this.optionSetItemAge.UseMnemonics = true;
			// 
			// comboRssSearchItemAge
			// 
			this.comboRssSearchItemAge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboRssSearchItemAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboRssSearchItemAge.ItemHeight = 13;
			this.comboRssSearchItemAge.Items.AddRange(new object[] {
																	   "1 hour",
																	   "2 hours",
																	   "3 hours",
																	   "4 hours",
																	   "5 hours",
																	   "6 hours",
																	   "12 hours",
																	   "18 hours",
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
																	   "2 years",
																	   "3 years",
																	   "5 years"});
			this.comboRssSearchItemAge.Location = new System.Drawing.Point(0, 50);
			this.comboRssSearchItemAge.Name = "comboRssSearchItemAge";
			this.comboRssSearchItemAge.Size = new System.Drawing.Size(145, 21);
			this.comboRssSearchItemAge.TabIndex = 1;
			// 
			// advancedOptionReadStatusGroup
			// 
			this.advancedOptionReadStatusGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			appearance5.BackColor = System.Drawing.Color.Transparent;
			this.advancedOptionReadStatusGroup.Appearance = appearance5;
			this.advancedOptionReadStatusGroup.ContentPadding.Left = 16;
			this.advancedOptionReadStatusGroup.Controls.Add(this.advancedOptionReadStatusGroupPanel);
			this.advancedOptionReadStatusGroup.ExpandedSize = new System.Drawing.Size(126, 55);
			this.advancedOptionReadStatusGroup.ExpansionIndicatorCollapsed = ((System.Drawing.Image)(resources.GetObject("advancedOptionReadStatusGroup.ExpansionIndicatorCollapsed")));
			this.advancedOptionReadStatusGroup.ExpansionIndicatorExpanded = ((System.Drawing.Image)(resources.GetObject("advancedOptionReadStatusGroup.ExpansionIndicatorExpanded")));
			this.advancedOptionReadStatusGroup.Location = new System.Drawing.Point(0, 0);
			this.advancedOptionReadStatusGroup.Name = "advancedOptionReadStatusGroup";
			this.advancedOptionReadStatusGroup.Size = new System.Drawing.Size(175, 55);
			this.advancedOptionReadStatusGroup.TabIndex = 0;
			this.advancedOptionReadStatusGroup.Text = "&Read Status";
			// 
			// advancedOptionReadStatusGroupPanel
			// 
			this.advancedOptionReadStatusGroupPanel.Controls.Add(this.checkBoxRssSearchUnreadItems);
			this.advancedOptionReadStatusGroupPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.advancedOptionReadStatusGroupPanel.Location = new System.Drawing.Point(19, 19);
			this.advancedOptionReadStatusGroupPanel.Name = "advancedOptionReadStatusGroupPanel";
			this.advancedOptionReadStatusGroupPanel.Size = new System.Drawing.Size(153, 33);
			this.advancedOptionReadStatusGroupPanel.TabIndex = 0;
			// 
			// checkBoxRssSearchUnreadItems
			// 
			this.checkBoxRssSearchUnreadItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxRssSearchUnreadItems.Location = new System.Drawing.Point(0, 5);
			this.checkBoxRssSearchUnreadItems.Name = "checkBoxRssSearchUnreadItems";
			this.checkBoxRssSearchUnreadItems.Size = new System.Drawing.Size(144, 20);
			this.checkBoxRssSearchUnreadItems.TabIndex = 0;
			this.checkBoxRssSearchUnreadItems.Text = "&Unread Posts";
			this.checkBoxRssSearchUnreadItems.UseMnemonics = true;
			// 
			// taskPaneScopeContainer
			// 
			this.taskPaneScopeContainer.Controls.Add(this.treeRssSearchScope);
			this.taskPaneScopeContainer.Location = new System.Drawing.Point(23, 349);
			this.taskPaneScopeContainer.Name = "taskPaneScopeContainer";
			this.taskPaneScopeContainer.Size = new System.Drawing.Size(189, 0);
			this.taskPaneScopeContainer.TabIndex = 4;
			this.taskPaneScopeContainer.Visible = false;
			// 
			// treeRssSearchScope
			// 
			this.treeRssSearchScope.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeRssSearchScope.CheckBoxes = true;
			this.treeRssSearchScope.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeRssSearchScope.HideSelection = false;
			this.treeRssSearchScope.ImageIndex = -1;
			this.treeRssSearchScope.Indent = 19;
			this.treeRssSearchScope.ItemHeight = 16;
			this.treeRssSearchScope.Location = new System.Drawing.Point(0, 0);
			this.treeRssSearchScope.Name = "treeRssSearchScope";
			this.treeRssSearchScope.SelectedImageIndex = -1;
			this.treeRssSearchScope.Size = new System.Drawing.Size(189, 0);
			this.treeRssSearchScope.TabIndex = 1;
			// 
			// panelRssSearchCommands
			// 
			this.panelRssSearchCommands.Controls.Add(this.btnNewSearch);
			this.panelRssSearchCommands.Controls.Add(this.textSearchExpression);
			this.panelRssSearchCommands.Controls.Add(this.btnSearchCancel);
			this.panelRssSearchCommands.Controls.Add(this.labelRssSearchState);
			this.panelRssSearchCommands.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelRssSearchCommands.Location = new System.Drawing.Point(0, 0);
			this.panelRssSearchCommands.Name = "panelRssSearchCommands";
			this.panelRssSearchCommands.Size = new System.Drawing.Size(245, 40);
			this.panelRssSearchCommands.TabIndex = 0;
			// 
			// btnNewSearch
			// 
			this.btnNewSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnNewSearch.BackColor = System.Drawing.SystemColors.Control;
			this.btnNewSearch.Enabled = false;
			this.btnNewSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnNewSearch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnNewSearch.Location = new System.Drawing.Point(154, 43);
			this.btnNewSearch.Name = "btnNewSearch";
			this.btnNewSearch.Size = new System.Drawing.Size(80, 23);
			this.btnNewSearch.TabIndex = 3;
			this.btnNewSearch.Text = "New Search";
			// 
			// textSearchExpression
			// 
			this.textSearchExpression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textSearchExpression.Location = new System.Drawing.Point(10, 10);
			this.textSearchExpression.Name = "textSearchExpression";
			this.textSearchExpression.Size = new System.Drawing.Size(139, 20);
			this.textSearchExpression.TabIndex = 0;
			this.textSearchExpression.Text = "";
			// 
			// btnSearchCancel
			// 
			this.btnSearchCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSearchCancel.BackColor = System.Drawing.SystemColors.Control;
			this.btnSearchCancel.Enabled = false;
			this.btnSearchCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnSearchCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnSearchCancel.Location = new System.Drawing.Point(154, 9);
			this.btnSearchCancel.Name = "btnSearchCancel";
			this.btnSearchCancel.Size = new System.Drawing.Size(80, 23);
			this.btnSearchCancel.TabIndex = 1;
			this.btnSearchCancel.Text = "Search";
			// 
			// labelRssSearchState
			// 
			this.labelRssSearchState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.labelRssSearchState.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelRssSearchState.Location = new System.Drawing.Point(10, 47);
			this.labelRssSearchState.Name = "labelRssSearchState";
			this.labelRssSearchState.Size = new System.Drawing.Size(139, 15);
			this.labelRssSearchState.TabIndex = 2;
			// 
			// taskPaneAllSearchOptions
			// 
			this.taskPaneAllSearchOptions.AutoScrollStyle = Infragistics.Win.UltraWinExplorerBar.AutoScrollStyle.BringActiveControlIntoView;
			this.taskPaneAllSearchOptions.Controls.Add(this.taskPaneSaveOptionsContainer);
			this.taskPaneAllSearchOptions.Controls.Add(this.taskPaneSearchFieldsContainer);
			this.taskPaneAllSearchOptions.Controls.Add(this.taskPaneAdvancedOptionsContainer);
			this.taskPaneAllSearchOptions.Controls.Add(this.taskPaneScopeContainer);
			this.taskPaneAllSearchOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			ultraExplorerBarGroup1.Container = this.taskPaneSaveOptionsContainer;
			ultraExplorerBarGroup1.Key = "searchTaskPaneSaveOptions";
			ultraExplorerBarGroup1.Settings.ContainerHeight = 26;
			ultraExplorerBarGroup1.Settings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;
			ultraExplorerBarGroup1.Text = "&Save This Search As";
			ultraExplorerBarGroup2.Container = this.taskPaneSearchFieldsContainer;
			ultraExplorerBarGroup2.Key = "searchTaskPaneFields";
			ultraExplorerBarGroup2.Settings.ContainerHeight = 116;
			ultraExplorerBarGroup2.Settings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;
			ultraExplorerBarGroup2.Text = "Search &Fields";
			ultraExplorerBarGroup3.Container = this.taskPaneAdvancedOptionsContainer;
			ultraExplorerBarGroup3.Expanded = false;
			ultraExplorerBarGroup3.Key = "searchTaskPaneAdvancedOptions";
			ultraExplorerBarGroup3.Settings.ContainerHeight = 280;
			ultraExplorerBarGroup3.Settings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;
			ultraExplorerBarGroup3.Text = "Advanced &Options";
			ultraExplorerBarGroup4.Container = this.taskPaneScopeContainer;
			ultraExplorerBarGroup4.Expanded = false;
			ultraExplorerBarGroup4.Key = "searchTaskPaneScope";
			ultraExplorerBarGroup4.Settings.ContainerHeight = 179;
			ultraExplorerBarGroup4.Settings.Style = Infragistics.Win.UltraWinExplorerBar.GroupStyle.ControlContainer;
			ultraExplorerBarGroup4.Text = "Search Scope";
			this.taskPaneAllSearchOptions.Groups.AddRange(new Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarGroup[] {
																															   ultraExplorerBarGroup1,
																															   ultraExplorerBarGroup2,
																															   ultraExplorerBarGroup3,
																															   ultraExplorerBarGroup4});
			this.taskPaneAllSearchOptions.GroupSettings.UseMnemonics = Infragistics.Win.DefaultableBoolean.True;
			this.taskPaneAllSearchOptions.Location = new System.Drawing.Point(0, 40);
			this.taskPaneAllSearchOptions.Margins.Bottom = 10;
			this.taskPaneAllSearchOptions.Margins.Left = 10;
			this.taskPaneAllSearchOptions.Margins.Right = 10;
			this.taskPaneAllSearchOptions.Name = "taskPaneAllSearchOptions";
			this.taskPaneAllSearchOptions.Size = new System.Drawing.Size(245, 440);
			this.taskPaneAllSearchOptions.TabIndex = 1;
			this.taskPaneAllSearchOptions.ViewStyle = Infragistics.Win.UltraWinExplorerBar.UltraExplorerBarViewStyle.XPExplorerBar;
			// 
			// SearchPanel
			// 
			this.Controls.Add(this.taskPaneAllSearchOptions);
			this.Controls.Add(this.panelRssSearchCommands);
			this.Name = "SearchPanel";
			this.Size = new System.Drawing.Size(245, 480);
			this.taskPaneSaveOptionsContainer.ResumeLayout(false);
			this.taskPaneSearchFieldsContainer.ResumeLayout(false);
			this.taskPaneAdvancedOptionsContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.advancedOptionItemPostedGroup)).EndInit();
			this.advancedOptionItemPostedGroup.ResumeLayout(false);
			this.advancedOptionItemPostedGroupPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.advancedOptionItemAgeGroup)).EndInit();
			this.advancedOptionItemAgeGroup.ResumeLayout(false);
			this.advancedOptionItemAgeGroupPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.optionSetItemAge)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.advancedOptionReadStatusGroup)).EndInit();
			this.advancedOptionReadStatusGroup.ResumeLayout(false);
			this.advancedOptionReadStatusGroupPanel.ResumeLayout(false);
			this.taskPaneScopeContainer.ResumeLayout(false);
			this.panelRssSearchCommands.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.taskPaneAllSearchOptions)).EndInit();
			this.taskPaneAllSearchOptions.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region public members

		public bool IsAdvancedOptionReadStatusActive {
			get { return _isAdvancedOptionReadStatusActive; }
			set {
				if (_isAdvancedOptionReadStatusActive != value) {
					_isAdvancedOptionReadStatusActive = value;
					ToggleExpandableGroupBoxImage(advancedOptionReadStatusGroup);
					advancedOptionReadStatusGroup.Panel.Enabled = value;
				}
			}
		}
		
		public bool IsAdvancedOptionItemAgeActive {
			get { return _isAdvancedOptionItemAgeActive; }
			set {
				if (_isAdvancedOptionItemAgeActive != value) {
					_isAdvancedOptionItemAgeActive = value;
					ToggleExpandableGroupBoxImage(advancedOptionItemAgeGroup);
					advancedOptionItemAgeGroup.Panel.Enabled = value;
					
					if (_isAdvancedOptionItemAgeActive)
						IsAdvancedOptionItemPostedActive = false;
				}
			}
		}

		public bool IsAdvancedOptionItemPostedActive {
			get { return _isAdvancedOptionItemPostedActive; }
			set {
				if (_isAdvancedOptionItemPostedActive != value) {
					_isAdvancedOptionItemPostedActive = value;
					ToggleExpandableGroupBoxImage(advancedOptionItemPostedGroup);
					advancedOptionItemPostedGroup.Panel.Enabled = value;
					
					if (_isAdvancedOptionItemPostedActive)
						IsAdvancedOptionItemAgeActive = false;
				}
			}
		}

		public bool SaveSearchGroupExpanded {
			get { return this.taskPaneAllSearchOptions.Groups["searchTaskPaneSaveOptions"].Expanded; }
			set { this.taskPaneAllSearchOptions.Groups["searchTaskPaneSaveOptions"].Expanded = value; }
		}
		public bool SearchFieldsGroupExpanded {
			get { return this.taskPaneAllSearchOptions.Groups["searchTaskPaneFields"].Expanded; }
			set { this.taskPaneAllSearchOptions.Groups["searchTaskPaneFields"].Expanded = value; }
		}
		public bool AdvancedOptionsGroupExpanded {
			get { return this.taskPaneAllSearchOptions.Groups["searchTaskPaneAdvancedOptions"].Expanded; }
			set { this.taskPaneAllSearchOptions.Groups["searchTaskPaneAdvancedOptions"].Expanded = value; }
		}
		public bool SearchScopeGroupExpanded {
			get { return this.taskPaneAllSearchOptions.Groups["searchTaskPaneScope"].Expanded; }
			set { this.taskPaneAllSearchOptions.Groups["searchTaskPaneScope"].Expanded = value; }
		}
		public ItemSearchState CurrentSearchState {
			get { return this._rssSearchState; }
		}
		
		public void ResetControlState() {
			//reset to default settings
			this.panelRssSearchCommands.SetBounds(0,0,0,40, BoundsSpecified.Height);

		    checkBoxRssSearchInTitle.Checked =
				checkBoxRssSearchInDesc.Checked = true;		
			
			checkBoxRssSearchInCategory.Checked = 
				checkBoxRssSearchInLink.Checked = 
				checkBoxRssSearchUnreadItems.Checked = false;
			
			InitializeStates();
			
			textSearchExpression.Text = String.Empty;
			textFinderCaption.Text = String.Empty;
			
			this.comboRssSearchItemAge.SelectedIndex = 0;
			this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 0;
			this.optionSetItemAge.CheckedIndex = 0;	// newer than
			
			this.dateTimeRssSearchItemPost.Value = DateTime.Now;
			this.dateTimeRssSearchPostBefore.Value = DateTime.Now.Subtract(new TimeSpan(1,0,0,0));

			this.RefreshButtonStates();
		}
		
		/// <summary>
		/// Sets the focus.
		/// </summary>
		public void SetFocus() {
			this.textSearchExpression.Focus();
		}
		
		#endregion

		#region private members

		private void ToggleExpandableGroupBoxImage(UltraExpandableGroupBox g) {
			Image img = g.ExpansionIndicatorExpanded;
			g.ExpansionIndicatorExpanded = g.ExpansionIndicatorCollapsed;
			g.ExpansionIndicatorCollapsed = img;
		}

		private SearchCriteriaCollection SearchDialogGetSearchCriterias() {
			SearchCriteriaCollection sc = new SearchCriteriaCollection();

			SearchStringElement where =SearchStringElement.Undefined;
			if (textSearchExpression.Text.Length > 0) {
				if (checkBoxRssSearchInTitle.Checked)		where |= SearchStringElement.Title;
				if (checkBoxRssSearchInDesc.Checked)		where |= SearchStringElement.Content;
				if (checkBoxRssSearchInCategory.Checked)	where |= SearchStringElement.Subject;
				if (checkBoxRssSearchInLink.Checked)		where |= SearchStringElement.Link;
			}

			StringExpressionKind kind = StringExpressionKind.LuceneExpression;
//			if (radioRssSearchSimpleText.Checked)
//				kind = StringExpressionKind.Text;
//			else if (radioRssSearchRegEx.Checked)
//				kind = StringExpressionKind.RegularExpression;
//			else if (radioRssSearchExprXPath.Checked)
//				kind = StringExpressionKind.XPathExpression;

			SearchCriteriaString scs = new SearchCriteriaString(textSearchExpression.Text, where, kind);
			sc.Add(scs);

			if (this.IsAdvancedOptionReadStatusActive) {
				SearchCriteriaProperty scp = new SearchCriteriaProperty();
				scp.BeenRead = !checkBoxRssSearchUnreadItems.Checked;
				scp.WhatKind = PropertyExpressionKind.Unread;
				sc.Add(scp);
			}

			if (this.IsAdvancedOptionItemAgeActive) {
				SearchCriteriaAge sca = new SearchCriteriaAge();
				if (this.optionSetItemAge.CheckedIndex == 0)
					sca.WhatKind = DateExpressionKind.NewerThan;
				else
					sca.WhatKind = DateExpressionKind.OlderThan;
				sca.WhatRelativeToToday = Utils.MapRssSearchItemAge(this.comboRssSearchItemAge.SelectedIndex);
				sc.Add(sca);
			}

			if (this.IsAdvancedOptionItemPostedActive) 
			{
				
				if (comboBoxRssSearchItemPostedOperator.SelectedIndex == 0)
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchItemPost.Value, DateExpressionKind.Equal));
				else if (comboBoxRssSearchItemPostedOperator.SelectedIndex == 1)
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchItemPost.Value, DateExpressionKind.OlderThan));
				else if (comboBoxRssSearchItemPostedOperator.SelectedIndex == 2)
					sc.Add(new SearchCriteriaAge(dateTimeRssSearchItemPost.Value, DateExpressionKind.NewerThan));
				else 
				{
					// handle case: either one date is greater than the other or equal
					if (dateTimeRssSearchItemPost.Value > dateTimeRssSearchPostBefore.Value) {
						sc.Add(new SearchCriteriaDateRange(dateTimeRssSearchPostBefore.Value, dateTimeRssSearchItemPost.Value));
					} 
					else if (dateTimeRssSearchItemPost.Value < dateTimeRssSearchPostBefore.Value) {
						sc.Add(new SearchCriteriaDateRange(dateTimeRssSearchItemPost.Value, dateTimeRssSearchPostBefore.Value));
					} 
					else {
						sc.Add(new SearchCriteriaAge(dateTimeRssSearchPostBefore.Value, DateExpressionKind.Equal));
					}
				}
					
			}

			return sc;
		}

		public void SearchDialogSetSearchCriterias(FinderNode node) {
			
			if (node == null)
				return;

			SearchCriteriaCollection criterias = node.Finder.SearchCriterias;
			string searchName = node.Finder.FullPath;
			
			ResetControlState();

			Queue itemAgeCriterias = new Queue(2);

			foreach (ISearchCriteria criteria in criterias) {
				SearchCriteriaString str = criteria as SearchCriteriaString;
				if (str != null) {
					SearchStringElement where = str.Where;
					if ((where & SearchStringElement.Title) == SearchStringElement.Title) 
						checkBoxRssSearchInTitle.Checked = true;
					else
						checkBoxRssSearchInTitle.Checked = false;

					if ((where & SearchStringElement.Content) == SearchStringElement.Content) 
						checkBoxRssSearchInDesc.Checked = true;
					else
						checkBoxRssSearchInDesc.Checked = false;

					if ((where & SearchStringElement.Subject) == SearchStringElement.Subject) 
						checkBoxRssSearchInCategory.Checked = true;
					else
						checkBoxRssSearchInCategory.Checked = false;

					if ((where & SearchStringElement.Link) == SearchStringElement.Link) 
						checkBoxRssSearchInLink.Checked = true;
					else
						checkBoxRssSearchInLink.Checked = false;
	
					SearchFieldsGroupExpanded = true;

//					if (str.WhatKind == StringExpressionKind.Text)
//						radioRssSearchSimpleText.Checked = true;
//					else if (str.WhatKind == StringExpressionKind.RegularExpression)
//						radioRssSearchRegEx.Checked = true;
//					else if (str.WhatKind == StringExpressionKind.XPathExpression)
//						radioRssSearchExprXPath.Checked = true;
//
//					if (!radioRssSearchSimpleText.Checked)
//						this.collapsiblePanelRssSearchExprKindEx.Collapsed = false;

					textSearchExpression.Text = str.What;
				}

				SearchCriteriaProperty prop = criteria as SearchCriteriaProperty;
				if (prop != null) {
					if (prop.WhatKind == PropertyExpressionKind.Unread) {
						if (!prop.BeenRead)
							checkBoxRssSearchUnreadItems.Checked = true;
						else
							checkBoxRssSearchUnreadItems.Checked = false;
					}
					IsAdvancedOptionReadStatusActive = true;
					AdvancedOptionsGroupExpanded =true;
				}

				SearchCriteriaAge age = criteria as SearchCriteriaAge;
				if (age != null) {
					if (age.WhatRelativeToToday.CompareTo(TimeSpan.Zero) != 0) {
						// relative item age specified
						IsAdvancedOptionItemAgeActive = true;
						if (age.WhatKind == DateExpressionKind.NewerThan)
							this.optionSetItemAge.CheckedIndex = 0;
						else
							this.optionSetItemAge.CheckedIndex = 1;
						this.comboRssSearchItemAge.SelectedIndex = Utils.MapRssSearchItemAge(age.WhatRelativeToToday);
					} 
					else {
						// absolute item age or range specified, queue for later handling
						itemAgeCriterias.Enqueue(age);
					}
					
					if (! AdvancedOptionsGroupExpanded)
						AdvancedOptionsGroupExpanded = true;
				}
			}

			if (itemAgeCriterias.Count > 0) {
				// absolute date specified

				IsAdvancedOptionItemPostedActive = true;
				SearchCriteriaAge ageAbs = (SearchCriteriaAge)itemAgeCriterias.Dequeue();
				if (ageAbs.WhatKind == DateExpressionKind.Equal)
					this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 0;
				else if (ageAbs.WhatKind == DateExpressionKind.OlderThan)
					this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 1;
				else if (ageAbs.WhatKind == DateExpressionKind.NewerThan)
					this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 2;
				else	// between (range):
					this.comboBoxRssSearchItemPostedOperator.SelectedIndex = 3;
				this.dateTimeRssSearchItemPost.Value = ageAbs.What;

				if (itemAgeCriterias.Count > 1) { 
					// range specified
					SearchCriteriaAge ageFrom = (SearchCriteriaAge)itemAgeCriterias.Dequeue();
					SearchCriteriaAge ageTo = (SearchCriteriaAge)itemAgeCriterias.Dequeue();
					this.dateTimeRssSearchItemPost.Value = ageFrom.What;
					this.dateTimeRssSearchPostBefore.Value = ageTo.What;
				}
			}

			itemAgeCriterias.Clear();

			if (! node.IsTempFinderNode) {
				textFinderCaption.Text = searchName;
			}

			if (textFinderCaption.Text.Length > 0)
				SaveSearchGroupExpanded = true;

			PopulateTreeRssSearchScope();	// init, all checked. Common case

			if (this.treeRssSearchScope.Nodes.Count == 0 ||
				(node.Finder.CategoryPathScope == null || node.Finder.CategoryPathScope.Count == 0) &&
				(node.Finder.FeedUrlScope == null || node.Finder.FeedUrlScope.Count == 0))
				return;

			this.treeRssSearchScope.Nodes[0].Checked = false;	// uncheck all. 
			TreeHelper.CheckChildNodes(this.treeRssSearchScope.Nodes[0], false);

			TreeHelper.SetCheckedNodes(this.treeRssSearchScope.Nodes[0], 
				node.Finder.CategoryPathScope, node.Finder.FeedUrlScope);

		}
		
		private void PopulateTreeRssSearchScope() {
			this.PopulateTreeRssSearchScope(externalScopeRootNode, this.treeRssSearchScope.ImageList);
		}
		
		/// <summary>
		/// Populates the treeview used as RSS search scope selection.
		/// </summary>
		/// <param name="sourceRootNode">The source root node.</param>
		public void PopulateTreeRssSearchScope(TreeFeedsNodeBase sourceRootNode, ImageList nodeImages) {
			this.externalScopeRootNode = sourceRootNode;
			this.treeRssSearchScope.ImageList = nodeImages;
			
			this.treeRssSearchScope.Nodes.Clear();
			if (this.externalScopeRootNode != null)
				TreeHelper.CopyNodes(this.externalScopeRootNode, this.treeRssSearchScope, true);
			if (this.treeRssSearchScope.Nodes.Count > 0) {
				this.treeRssSearchScope.Nodes[0].Expand();
			}
		}
		
		private void RefreshButtonStates() {
			this.btnSearchCancel.Enabled = (this.textSearchExpression.Text.Length > 0);
			if (!this.btnSearchCancel.Enabled) {
				this.btnSearchCancel.Enabled = 
					this.IsAdvancedOptionItemAgeActive ||
					this.IsAdvancedOptionItemPostedActive;
			}
			this.btnRssSearchSave.Enabled = (this.textFinderCaption.Text.Trim().Length > 0 && this.btnSearchCancel.Enabled);
		}
		
		#endregion
		
		private void OnSearchExpressionChanged(object sender, System.EventArgs e) {
			RefreshButtonStates();
		}

		private void OnSearchFinderCaptionChanged(object sender, System.EventArgs e) {
			RefreshButtonStates();
		}
		
		private void OnSearchExpressionKeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Return && (this.textSearchExpression.Text.Length > 0)) {
				this.OnSearchCancelClick(sender, EventArgs.Empty);
			}
		}
		//does nothing more than supress the beep if you press enter
		private void OnAnyEnterKeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == '\r'  && Control.ModifierKeys == Keys.None)
				e.Handled = true;	// supress beep
		}
		
		private void OnAdvancedOptionReadStatusExpandedStateChanging(object sender, System.ComponentModel.CancelEventArgs e) {
			IsAdvancedOptionReadStatusActive = !IsAdvancedOptionReadStatusActive;
			RefreshButtonStates();
			e.Cancel = true;
		}

		private void OnAdvancedOptionReadStatusExpandableGroupClick(object sender, System.EventArgs e) {
			UltraExpandableGroupBox g = sender as UltraExpandableGroupBox;
			if (g != null) {
				UIElement elem = g.UIElement.ElementFromPoint(g.PointToClient(Control.MousePosition));
				if (elem is ImageAndTextUIElement.ImageAndTextDependentTextUIElement) {
					OnAdvancedOptionReadStatusExpandedStateChanging(g, new CancelEventArgs());
				}
			}
		}
		
		private void OnAdvancedOptionItemAgeExpandedStateChanging(object sender, System.ComponentModel.CancelEventArgs e) {
			IsAdvancedOptionItemAgeActive = !IsAdvancedOptionItemAgeActive;
			RefreshButtonStates();
			e.Cancel = true;
		}

		private void OnAdvancedOptionItemAgeExpandableGroupClick(object sender, System.EventArgs e) {
			UltraExpandableGroupBox g = sender as UltraExpandableGroupBox;
			if (g != null) {
				UIElement elem = g.UIElement.ElementFromPoint(g.PointToClient(Control.MousePosition));
				if (elem is ImageAndTextUIElement.ImageAndTextDependentTextUIElement) {
					OnAdvancedOptionItemAgeExpandedStateChanging(g, new CancelEventArgs());
				}
			}
		}
		
		private void OnAdvancedOptionItemPostedExpandedStateChanging(object sender, System.ComponentModel.CancelEventArgs e) {
			IsAdvancedOptionItemPostedActive = !IsAdvancedOptionItemPostedActive;
			RefreshButtonStates();
			e.Cancel = true;
		}

		private void OnAdvancedOptionItemPostedExpandableGroupClick(object sender, System.EventArgs e) {
			UltraExpandableGroupBox g = sender as UltraExpandableGroupBox;
			if (g != null) {
				UIElement elem = g.UIElement.ElementFromPoint(g.PointToClient(Control.MousePosition));
				if (elem is ImageAndTextUIElement.ImageAndTextDependentTextUIElement) {
					OnAdvancedOptionItemPostedExpandedStateChanging(g, new CancelEventArgs());
				}
			}
		}
		
		private void OnItemPostedOperatorSelectedIndexChanged(object sender, EventArgs e) {
			//if (IsAdvancedOptionItemPostedActive) {
				dateTimeRssSearchPostBefore.Enabled = 
					(comboBoxRssSearchItemPostedOperator.SelectedIndex == comboBoxRssSearchItemPostedOperator.Items.Count - 1);
			//}
		}

		private void OnRssSearchScopeTreeAfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e) {
			if (e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse) {
				TreeHelper.PerformOnCheckStateChanged(e.Node);
			}
		}

		private void OnNewSearchClick(object sender, EventArgs e) {
			SetControlStateTo(ItemSearchState.Pending, String.Empty);
			this.ResetControlState();
			PopulateTreeRssSearchScope();
			this.textSearchExpression.Focus();
		}
		
		
		private void OnSearchCancelClick(object sender, EventArgs e) {
				
			switch (_rssSearchState) {

				case ItemSearchState.Pending:
				
					SetControlStateTo(ItemSearchState.Searching, SR.RssSearchStateMessage);
					
					string searchID = Guid.NewGuid().ToString();
					string newName = textFinderCaption.Text.Trim();
					SearchCriteriaCollection criteria = this.SearchDialogGetSearchCriterias();

					//Test scope:
					//					ArrayList cs = new ArrayList(1);
					//					cs.Add("Weblogs");
					//					ArrayList fs = new ArrayList(1);
					//					fs.Add("http://www.rendelmann.info/blog/GetRss.asmx");
					//					fs.Add("http://www.25hoursaday.com/webblog/GetRss.asmx"); // activate for scope tests:
					//					resultContainer.Finder = new RssFinder(resultContainer, this.SearchDialogGetSearchCriterias(),
					//						 cs, fs, new RssFinder.SearchScopeResolveCallback(this.ScopeResolve) , true);
					
					ArrayList catScope = null;
					ArrayList feedScope = null;
					
					// if we have scope nodes, and not all selected:
					if (this.treeRssSearchScope.Nodes.Count > 0 && !this.treeRssSearchScope.Nodes[0].Checked) {
						ArrayList cs = new ArrayList(), fs = new ArrayList();
						TreeHelper.GetCheckedNodes(this.treeRssSearchScope.Nodes[0], cs, fs);
						
						if (cs.Count > 0)	catScope = new ArrayList(cs.Count);
						if (fs.Count > 0)	feedScope = new ArrayList(fs.Count);

						foreach (TreeNode n in cs) {
							catScope.Add(TreeHelper.BuildCategoryStoreName(n));
						}
						foreach (TreeNode n in fs) {
							feedScope.Add((string)n.Tag);
						}
					}
					
					FinderNode resultContainer = null;
					try 
					{
						resultContainer = this.OnBeforeSearch(searchID, newName, criteria, catScope, feedScope);
					} 
					catch (Exception validationException) 
					{
						SetControlStateTo(ItemSearchState.Failure, validationException.Message);
						break;
					}
					
					this.Refresh();
					if (resultContainer != null) {
						try {
							OnSearch(resultContainer);
						}
						catch (Exception validationException) {
							SetControlStateTo(ItemSearchState.Failure, validationException.Message);
							break;
						}
					}
					
					break;

				case ItemSearchState.Searching:
					_rssSearchState = ItemSearchState.Canceled;
					//this.SetSearchStatusText(SR.RssSearchCancelledStateMessage);
					btnSearchCancel.Enabled = false;
					break;

				case ItemSearchState.Canceled:
					Debug.Assert(false);		// should not be able to press Search button while it is canceling
					break;
			}

		}
		
		public void SetControlStateTo(ItemSearchState state, string stateMessage) {
			switch (state) {
				case ItemSearchState.Pending:
					this.btnSearchCancel.Text = SR.RssSearchDialogButtonSearchCaption;
					SetControlEnabled(true);
					break;
				case ItemSearchState.Searching:
					this.btnSearchCancel.Text = SR.RssSearchDialogButtonCancelCaption;
					this.panelRssSearchCommands.SetBounds(0,0,0,80, BoundsSpecified.Height);
					SetControlEnabled(false);
					break;
				case ItemSearchState.Finished:
					SetControlStateTo(ItemSearchState.Pending, stateMessage);
					return;
				case ItemSearchState.Canceled:
					this.btnSearchCancel.Text = SR.RssSearchDialogButtonSearchCaption;
					SetControlEnabled(true);
					break;
				case ItemSearchState.Failure:
					this.btnSearchCancel.Text = SR.RssSearchDialogButtonSearchCaption;
					SetControlEnabled(true);
					break;
				default:	
					break;
			}
			
			this.labelRssSearchState.Text = stateMessage;
			_rssSearchState = state;
		}
		
		private void SetControlEnabled(bool enabled) {
			this.btnNewSearch.Enabled = this.btnRssSearchSave.Enabled = enabled;
			this.textSearchExpression.Enabled = this.textFinderCaption.Enabled = enabled;
			this.taskPaneAllSearchOptions.Enabled = enabled;

		}
		
		private FinderNode OnBeforeSearch(string id, string resultContainerName, 
			SearchCriteriaCollection criteria, ArrayList categoryScope, ArrayList feedScope) 
		{
			if (this.BeforeNewsItemSearch != null) {
				bool cancel = false;
				NewsItemSearchCancelEventArgs args = new NewsItemSearchCancelEventArgs(id, resultContainerName, 
					criteria, categoryScope, feedScope, cancel);
				this.BeforeNewsItemSearch(this, args);
				if (args.Cancel || args.ResultContainer == null)
					return null;
				return args.ResultContainer;
			}
			return new FinderNode();
		}
	
		private void OnSearch(FinderNode finderNode) {
			if (this.NewsItemSearch != null) {
				this.NewsItemSearch(this, new NewsItemSearchEventArgs(finderNode));
			}
		}
	}
	
	public delegate void NewsItemSearchCancelEventHandler(object sender, NewsItemSearchCancelEventArgs e);
	
	public class NewsItemSearchCancelEventArgs: CancelEventArgs
	{
		public readonly string SearchID;
		public readonly string ResultContainerName;
		public readonly SearchCriteriaCollection SearchCriteria;
		public readonly ArrayList CategoryScope;
		public readonly ArrayList FeedScope;
		
		public FinderNode ResultContainer;
		public NewsItemSearchCancelEventArgs(string searchID, string resultContainerName, 
			SearchCriteriaCollection searchCriteria, 
			ArrayList categoryScope, ArrayList feedScope, bool cancel): 
			base(cancel) 
		{
			this.SearchID = searchID;
			this.ResultContainerName = resultContainerName;
			this.SearchCriteria = searchCriteria;
			this.CategoryScope = categoryScope;
			this.FeedScope = feedScope;
		}
	}
	
	public delegate void NewsItemSearchEventHandler(object sender, NewsItemSearchEventArgs e);
	
	public class NewsItemSearchEventArgs: EventArgs
	{
		public readonly FinderNode FinderNode;
		public NewsItemSearchEventArgs(FinderNode finderNode): base() {
			this.FinderNode = finderNode;
		}
	}
		
}

#region CVS Version Log
/*
 * $Log: SearchPanel.cs,v $
 * Revision 1.15  2007/02/27 17:12:49  t_rendelmann
 * NEW: applied dutch localization
 *
 * Revision 1.14  2007/02/09 07:51:30  t_rendelmann
 * fixed: save search button did not work
 *
 * Revision 1.13  2007/02/08 16:22:17  carnage4life
 * Fixed regression where checked fields in local search are treated as an AND instead of an OR
 *
 * Revision 1.12  2007/02/07 17:51:58  t_rendelmann
 * fixed: not all UI widgets were localized
 *
 * Revision 1.11  2007/01/20 15:54:08  carnage4life
 * Fixed problems that occur when users import OPMLs with bad feed URLs
 *
 * Revision 1.10  2007/01/18 18:21:31  t_rendelmann
 * code cleanup (old search controls and code removed)
 *
 * Revision 1.9  2007/01/18 15:07:29  t_rendelmann
 * finished: lucene integration (scoped searches are now working)
 *
 * Revision 1.8  2007/01/18 04:03:09  carnage4life
 * Completed support for custom newspaper view for search results
 *
 * Revision 1.7  2007/01/16 19:43:40  t_rendelmann
 * cont.: now we populate the rss search tree;
 * fixed: treeview images are now correct (not using the favicons)
 *
 * Revision 1.6  2007/01/12 14:55:26  t_rendelmann
 * cont. SearchPanel: added localization support
 *
 * Revision 1.5  2006/10/28 16:38:25  t_rendelmann
 * added: new "Unread Items" folder, not anymore based on search, but populated directly with the unread items
 *
 * Revision 1.4  2006/10/19 15:36:47  t_rendelmann
 * *** empty log message ***
 *
 * Revision 1.3  2006/10/04 21:27:27  carnage4life
 * Fixed issue where relative links in Atom feeds did not work
 *
 * Revision 1.2  2006/10/03 16:53:17  t_rendelmann
 * cont. integrate lucene - the search and UI part
 *
 * Revision 1.1  2006/09/29 18:14:38  t_rendelmann
 * a) integrated lucene index refreshs;
 * b) now using a centralized defined category separator;
 * c) unified decision about storage relevant changes to feed, feed and feeditem properties;
 * d) fixed: issue [ 1546921 ] Extra Category Folders Created
 * e) fixed: issue [ 1550083 ] Problem when renaming categories
 *
 */
#endregion
