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
	public partial class SearchPanel : UserControl 
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
			this.comboRssSearchItemAge.DataSource = Utils.RssSearchItemAgeStrings;
			//for (int i = 0; i <= 25; i++)
			//    this.comboRssSearchItemAge.Items.Add(Utils.MapRssSearchItemAgeString(i));

			// enable native info tips support:
			Win32.ModifyWindowStyle(treeRssSearchScope.Handle, 0, Win32.TVS_INFOTIP);
			this.treeRssSearchScope.PathSeparator = FeedSource.CategorySeparator;
			
			this.BackColor = FontColorHelper.UiColorScheme.TaskPaneNavigationArea;
		}

		private void InitializeEvents() {
			this.advancedOptionReadStatusGroup.ExpandedStateChanging += this.OnAdvancedOptionReadStatusExpandedStateChanging;
			this.advancedOptionReadStatusGroup.Click += OnAdvancedOptionReadStatusExpandableGroupClick;
			this.advancedOptionItemAgeGroup.ExpandedStateChanging += this.OnAdvancedOptionItemAgeExpandedStateChanging;
			this.advancedOptionItemAgeGroup.Click += OnAdvancedOptionItemAgeExpandableGroupClick;
			this.advancedOptionItemPostedGroup.ExpandedStateChanging += this.OnAdvancedOptionItemPostedExpandedStateChanging;
			this.advancedOptionItemPostedGroup.Click += OnAdvancedOptionItemPostedExpandableGroupClick;
			
			this.comboBoxRssSearchItemPostedOperator.SelectedIndexChanged += OnItemPostedOperatorSelectedIndexChanged;
			this.btnSearchCancel.Click += this.OnSearchCancelClick;
			this.btnNewSearch.Click += this.OnNewSearchClick;
			this.btnRssSearchSave.Click += this.OnSearchCancelClick;

			this.textSearchExpression.TextChanged += this.OnSearchExpressionChanged;
			this.textFinderCaption.TextChanged += this.OnSearchFinderCaptionChanged;
			this.textSearchExpression.KeyDown += this.OnSearchExpressionKeyDown;
			this.textSearchExpression.KeyPress += this.OnAnyEnterKeyPress;
		
			this.treeRssSearchScope.AfterCheck += this.OnRssSearchScopeTreeAfterCheck;

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
				sca.WhatRelativeToToday = Utils.RssSearchItemAgeToTimeSpan(this.comboRssSearchItemAge.SelectedIndex);
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
						this.comboRssSearchItemAge.SelectedIndex = Utils.RssSearchItemAgeToIndex(age.WhatRelativeToToday);
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
		/// <param name="nodeImages"></param>
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
		
		private void OnSearchExpressionChanged(object sender, EventArgs e) {
			RefreshButtonStates();
		}

		private void OnSearchFinderCaptionChanged(object sender, EventArgs e) {
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
		
		private void OnAdvancedOptionReadStatusExpandedStateChanging(object sender, CancelEventArgs e) {
			IsAdvancedOptionReadStatusActive = !IsAdvancedOptionReadStatusActive;
			RefreshButtonStates();
			e.Cancel = true;
		}

		private void OnAdvancedOptionReadStatusExpandableGroupClick(object sender, EventArgs e) {
			UltraExpandableGroupBox g = sender as UltraExpandableGroupBox;
			if (g != null) {
				UIElement elem = g.UIElement.ElementFromPoint(g.PointToClient(Control.MousePosition));
				if (elem is ImageAndTextUIElement.ImageAndTextDependentTextUIElement) {
					OnAdvancedOptionReadStatusExpandedStateChanging(g, new CancelEventArgs());
				}
			}
		}
		
		private void OnAdvancedOptionItemAgeExpandedStateChanging(object sender, CancelEventArgs e) {
			IsAdvancedOptionItemAgeActive = !IsAdvancedOptionItemAgeActive;
			RefreshButtonStates();
			e.Cancel = true;
		}

		private void OnAdvancedOptionItemAgeExpandableGroupClick(object sender, EventArgs e) {
			UltraExpandableGroupBox g = sender as UltraExpandableGroupBox;
			if (g != null) {
				UIElement elem = g.UIElement.ElementFromPoint(g.PointToClient(Control.MousePosition));
				if (elem is ImageAndTextUIElement.ImageAndTextDependentTextUIElement) {
					OnAdvancedOptionItemAgeExpandedStateChanging(g, new CancelEventArgs());
				}
			}
		}
		
		private void OnAdvancedOptionItemPostedExpandedStateChanging(object sender, CancelEventArgs e) {
			IsAdvancedOptionItemPostedActive = !IsAdvancedOptionItemPostedActive;
			RefreshButtonStates();
			e.Cancel = true;
		}

		private void OnAdvancedOptionItemPostedExpandableGroupClick(object sender, EventArgs e) {
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

		private void OnRssSearchScopeTreeAfterCheck(object sender, TreeViewEventArgs e) {
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
					
					FinderNode resultContainer;
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
