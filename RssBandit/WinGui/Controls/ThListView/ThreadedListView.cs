#region CVS Version Header
/*
 * $Id: ThreadedListView.cs,v 1.19 2005/04/12 07:12:55 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/12 07:12:55 $
 * $Revision: 1.19 $
 */
#endregion

#define NON_TRANSPARENT_SORTMARKERS

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms.ThListView.Sorting;
using THLV = System.Windows.Forms.ThListView;

using NewsComponents.Feed;
using RssBandit;
using RssBandit.Common.Logging;

namespace System.Windows.Forms.ThListView {
	
	/// <summary>
	/// ThreadedListView, an extended listview control.
	/// </summary>
	public class ThreadedListView : System.Windows.Forms.ListView {
		private static readonly log4net.ILog _log = Log.GetLogger(typeof(ThreadedListView));

		private System.Windows.Forms.ImageList imageListStates;
		
		private Bitmap upBM, downBM;    // the 2 bitmaps used for drawing the sort triangles
		private bool _isWinXP, _headerImageListLoaded;
		private System.Windows.Forms.ImageList _headerImageList;
		private ThreadedListViewSorter _sorter;
		private ThreadedListViewColumnHeader _autoGroupCol = null;
		private ArrayList _autoGroupList = new ArrayList();
		private ThreadedListViewItemCollection _items;
		private ThreadedListViewColumnHeaderCollection _columns;
		private ThreadedListViewGroupCollection _groups; 
		private FeedColumnLayout _layout;
		private bool _showInGroups = false;
		private bool _autoGroup = false;
		private bool _threadedView = true;
		private string _emptyAutoGroupText = String.Empty;
		//private IntPtr _apiRetVal;
		private ThreadedListViewItem _noChildsPlaceHolder;

		public delegate void OnBeforeListLayoutChangeCancelEventHandler(object sender, ListLayoutCancelEventArgs e);
		public event OnBeforeListLayoutChangeCancelEventHandler BeforeListLayoutChange;
		public delegate void OnListLayoutChangedEventHandler(object sender, ListLayoutEventArgs e);
		public event OnListLayoutChangedEventHandler ListLayoutChanged;
		public delegate void OnListLayoutModifiedEventHandler(object sender, ListLayoutEventArgs e);
		public event OnListLayoutModifiedEventHandler ListLayoutModified;

		public delegate void OnBeforeExpandThreadCancelEventHandler(object sender, ThreadCancelEventArgs e);
		public event OnBeforeExpandThreadCancelEventHandler BeforeExpandThread;
		public delegate void OnBeforeCollapseThreadCancelEventHandler(object sender, ThreadCancelEventArgs e);
		public event OnBeforeCollapseThreadCancelEventHandler BeforeCollapseThread;
 
		public delegate void OnExpandThreadEventHandler(object sender, ThreadEventArgs e);
		public event OnExpandThreadEventHandler ExpandThread;
		public delegate void OnCollapseThreadEventHandler(object sender, ThreadEventArgs e);
		public event OnCollapseThreadEventHandler CollapseThread;

		public delegate void InsertItemsForPlaceHolderHandler(string placeHolderTicket, ThreadedListViewItem[] newChildItems, bool sortOnInsert);

		private System.ComponentModel.IContainer components;
	
		public ThreadedListView() {
			_items = new ThreadedListViewItemCollection(this);
			_groups = new ThreadedListViewGroupCollection(this);
			_columns = new ThreadedListViewColumnHeaderCollection(this);
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			InitListView();
		}

		internal void SetColumnOrderArray(int[] orderArray) {
			Win32.API.SetColumnOrderArray(this.Handle, orderArray);
		}

		internal int[] GetColumnOrderArray() {
			return Win32.API.GetColumnOrderArray(this.Handle, this.Columns.Count);
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
		public new bool CheckBoxes { 
			get { return base.CheckBoxes; } 
			set { base.CheckBoxes = value; } 
		}
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
		public new ImageList StateImageList { 
			get { return base.StateImageList; } 
			set { base.StateImageList = value; } 
		}
		
		private void InitListView() {

			this._headerImageListLoaded = false;
			this._isWinXP = Win32.IsOSAtLeastWindowsXP;
			// our core state:
			this.ShowAsThreads = true;

			// layout:
			this.View = System.Windows.Forms.View.Details;
			
			// sorting:
			this._sorter = new ThreadedListViewSorter(this);
			this._sorter.BeforeSort += new EventHandler(this.OnBeforeSort);
			this._sorter.AfterSort += new EventHandler(this.OnAfterSort);
			// events:
			this.HandleCreated += new EventHandler(this.OnListviewHandleCreated);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnListviewMouseDown);
			
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ThreadedListView));
			this.imageListStates = new System.Windows.Forms.ImageList(this.components);
			// 
			// imageListStates
			// 
			this.imageListStates.ImageSize = new System.Drawing.Size(16, 16);
			this.imageListStates.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListStates.ImageStream")));
			this.imageListStates.TransparentColor = System.Drawing.Color.Transparent;

		}
		#endregion

		protected override void WndProc(ref Message m) {
			bool handled = false;
			
			// determine if we want to process this message type
			switch ( m.Msg ) {
					// notify messages can come from the header and are used 
					// for column sorting and item filtering if wanted
				case (int)Win32.W32_WM.WM_NOTIFY:
					break;

					// internal ListView notify messages are reflected to us via OCM
				case (int)Win32.W32_OCM.OCM_NOTIFY:
					//RefreshSortMarks(_sorter.SortColumnIndex, _sorter.SortOrder);
					break;

				case (int)Win32.W32_WM.WM_PAINT:
					//TODO: next code does not work...
//					int _autoSizeColumnIndex = 0;
//					if (this.View == View.Details && this.Columns.Count > _autoSizeColumnIndex) {
//						this.Columns[_autoSizeColumnIndex].Width = -2;
//					}
					break;

			}

			if (!handled)
				base.WndProc(ref m);
		}

		private void OnListviewHandleCreated(object sender, EventArgs e) {
			this.SetExtendedStyles();
		}

		private void OnListviewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (base.Items.Count <= 0)
				return;

			if (e.Button != MouseButtons.Left)	// expand only on left click
				return;

			ThreadedListViewItem lvi = this.GetItemAt(e.X, e.Y) as ThreadedListViewItem;
			if (lvi != null && lvi.StateImageHitTest(new Point(e.X, e.Y))) {

				if (lvi.HasChilds) {
					if (lvi.Expanded == false)
						ExpandListViewItem(lvi, false);
					else if (lvi.Expanded == true)
						CollapseListViewItem(lvi);
				}

			} else if (lvi != null && e.Clicks > 1) {	
				
				// double click:
				if (lvi.HasChilds) {
					if (lvi.Expanded == false) {
						ExpandListViewItem(lvi, true);
					}
					else if (lvi.Expanded == true) {
						CollapseListViewItem(lvi);
					}
				}
			}
		}

		#region Designer Properties, Grouping, etc.
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Description("the items collection of this view"),
		Editor(typeof(ThreadedListViewItemCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)),
		Category("Behavior")]
		public new ThreadedListViewItemCollection Items{
			get{ return _items; }
		}

		//TODO designer support
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Description("the header columns collection of this view"),
		//Editor(typeof(ThreadedListViewItemCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)),
		Category("Behavior")]
		public new ThreadedListViewColumnHeaderCollection Columns{
			get{ return _columns; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Description("collection of available groups (manually added)"), 
		Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(System.Drawing.Design.UITypeEditor)), 
		Category("Grouping")]
		public ThreadedListViewGroupCollection Groups{
			get{ return _groups; }
		}

		[Category("Grouping"), 
		Description("flag if the grouping view is active"), 
		DefaultValue(false)]
		public bool ShowInGroups{
			get{ return _showInGroups; }
			set{
				if(_showInGroups != value){
					_showInGroups = value;
					if (_showInGroups)
						this.ShowAsThreads = false;
					if( _autoGroup && value == false ){
						_autoGroup = false;
						_autoGroupCol = null;
						_autoGroupList.Clear();
					}
		
					this.APIEnableGrouping(value);
				}
			}
		}

		[Category("Grouping"),
		Description("flag if the autogroup mode is active"),
		DefaultValue(false)]
		public bool AutoGroupMode{
			get{ return _autoGroup; }
			set{
				_autoGroup = value;
				if (_autoGroup)
					this.ShowAsThreads = false;
				if(_autoGroupCol != null){
					AutoGroupByColumn(_autoGroupCol.Index);
				}
			}
		}
		[Category("Grouping"), 
		Description("column by with values the listiew is automatically grouped"), 
		DefaultValue(typeof(ColumnHeader), ""), 
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ThreadedListViewColumnHeader AutoGroupColumn{
			get{ return _autoGroupCol; }
			set{
				_autoGroupCol = value;

				if( _autoGroupCol != null){
					AutoGroupByColumn(_autoGroupCol.Index);
				}
			}
		}
		[Category("Grouping"), 
		Description("the text that is displayed instead of an empty auto group text"), 
		DefaultValue("")]
		public string EmptyAutoGroupText{
			get{ return _emptyAutoGroupText; }
			set{
				_emptyAutoGroupText = value;

				if ( _autoGroupCol != null){
					AutoGroupByColumn(_autoGroupCol.Index);
				}
			}
		}

		[Browsable(false), 
		Description("readonly array of all automatically created groups"), 
		Category("Grouping")]
		public Array Autogroups{
			get{ return _autoGroupList.ToArray(typeof(String)); }
		}

		public bool AutoGroupByColumn(int columnID){
			if( columnID >= this.Columns.Count || columnID < 0 ){ return false; }

			try{
				_autoGroupList.Clear();

				foreach(ThreadedListViewItem itm in this.Items){
					if ( !_autoGroupList.Contains(itm.SubItems[columnID].Text == String.Empty ? _emptyAutoGroupText : itm.SubItems[columnID].Text)) {
						_autoGroupList.Add(itm.SubItems[columnID].Text == String.Empty ? EmptyAutoGroupText : itm.SubItems[columnID].Text);
					}
				}

				_autoGroupList.Sort();

				Win32.API.ClearListViewGroup(this.Handle);
				foreach(string text in _autoGroupList){
					Win32.API.AddListViewGroup(this.Handle, text, _autoGroupList.IndexOf(text));
				}
				
				foreach(ThreadedListViewItem itm in this.Items){
					int index = _autoGroupList.IndexOf(itm.SubItems[columnID].Text == "" ? _emptyAutoGroupText : itm.SubItems[columnID].Text);
					Win32.API.AddItemToGroup(this.Handle, itm.Index, index);
				}

				this.APIEnableGrouping(true);
				_showInGroups = true;
				_autoGroup = true;
				_autoGroupCol = (ThreadedListViewColumnHeader)this.Columns[columnID];

				this.Refresh();

				return true;
			}
			catch(Exception ex){
				throw new SystemException("Error in ThreadedListView.AutoGroupByColumn: " + ex.Message);
			}
		}

		public ThreadedListViewItem NoThreadChildsPlaceHolder {
			get { return _noChildsPlaceHolder; }
			set { _noChildsPlaceHolder = value; }
		}

		public bool Regroup(){
			try{
				Win32.API.ClearListViewGroup(this.Handle);
				foreach(ThreadedListViewGroup grp in this.Groups){
					Win32.API.AddListViewGroup(this.Handle, grp.GroupText, grp.GroupIndex);
				}

				foreach(ThreadedListViewItem itm in this.Items){
					Win32.API.AddItemToGroup(this.Handle, itm.Index, itm.GroupIndex);
				}

				this.APIEnableGrouping(true);
				_showInGroups = true;
				_autoGroup = false;
				_autoGroupCol = null;
				_autoGroupList.Clear();

				return true;
			}
			catch(Exception ex){
				throw new SystemException("Error in ThreadedListView.Regroup: " + ex.Message);
			}
		}
		#endregion

		#region other public members

		public ThreadedListViewSorter SortManager {
			get { return _sorter; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public FeedColumnLayout FeedColumnLayout {
			get { 
				return this.FeedColumnLayoutFromCurrentSettings();
			}
			set { 
				if (value != null ) {

					FeedColumnLayout newLayout = value; 
					
					if (!RaiseBeforeFeedColumnLayoutChangeEventCancel(newLayout)) {

						this._layout = newLayout;
						// enable re-build of columns:
						RaiseFeedColumnLayoutChangedEvent(newLayout);
						//TODO: safety - ensure, some columns are added. 
						if (newLayout.SortOrder != NewsComponents.SortOrder.None)
							_sorter.Sort(this.Columns.GetIndexByKey(newLayout.SortByColumn), ConvertSortOrder(newLayout.SortOrder));
					}
				}
			}
		}

		[Category("Behavior"),
		Description("flag if the threaded view mode is active"),
		DefaultValue(true)]
		public bool ShowAsThreads {
			get{ return _threadedView; }
			set{
				_threadedView = value;
				if(_threadedView){
					this.ShowInGroups = false;
					this.AutoGroupMode = false;
					this.CheckBoxes = false;
					this.StateImageList = this.imageListStates;
					ReApplyItemStates();
				} else {
					this.StateImageList = null;
				}
			}
		}

		internal void RedrawItems(){
			Win32.API.RedrawItems(this, true);
			this.ArrangeIcons();
		}

		
		internal void UpdateItems(){
			Win32.API.UpdateItems(this);
		}

					
		public void SetColumnStyle(int column, Font font, Color foreColor, Color backColor){
			this.SuspendLayout();

			foreach(ThreadedListViewItem itm in this.Items){
				if( itm.SubItems.Count > column){
					itm.SubItems[column].Font = font;
					itm.SubItems[column].BackColor = backColor;
					itm.SubItems[column].ForeColor = foreColor;					
				}
			}

			this.ResumeLayout();
		}


		public void SetColumnStyle(int column, Font font,Color foreColor){
			SetColumnStyle(column, font, foreColor, this.BackColor);
		}


		public void SetColumnStyle(int column, Font font){
			SetColumnStyle(column, font, this.ForeColor, this.BackColor);
		}


		public void ResetColumnStyle(int column){
			this.SuspendLayout();

			foreach(ThreadedListViewItem itm in this.Items){
				if( itm.SubItems.Count > column){
					itm.SubItems[column].ResetStyle();
				}
			}

			this.ResumeLayout();
		}


		public void SetBackgroundImage(string imagePath, ImagePosition position){
			Win32.API.SetListViewImage(this.Handle, imagePath, position);
		}

		#endregion

		private void ReApplyItemStates() {
			if (base.Items.Count == 0)
				return;
			foreach (ThreadedListViewItem lv in this.Items) {
				if (lv.HasChilds)
					lv.Expanded = !lv.Collapsed;
			}
		}

		/// <summary>
		/// The workhorse to collapse a threaded listview item
		/// </summary>
		/// <param name="lvItem"></param>
		internal void CollapseListViewItem(ThreadedListViewItem lvItem) {
			int focusedItemIndex;
			int paramItemIndex;
			int currentIndent;
			int nextIndex;

			if (lvItem != null && lvItem.Expanded) {

				if ( RaiseBeforeCollapseEventCancel(lvItem) ) {
					lvItem.StateImageIndex = 0;
					return;
				}

				if (base.FocusedItem == null)
					focusedItemIndex = lvItem.Index;
				else
					focusedItemIndex = base.FocusedItem.Index;

				base.BeginUpdate();
				try {

					lvItem.SetThreadState(false);	
					paramItemIndex = lvItem.Index;
					currentIndent = lvItem.IndentLevel;
					nextIndex = paramItemIndex + 1;
					
					lock (this.Items) {
						while (nextIndex < base.Items.Count &&
							((ThreadedListViewItem)base.Items[nextIndex]).IndentLevel > currentIndent) {
							this.Items[nextIndex].Parent = null;
							this.Items.RemoveAt(nextIndex);
							if (nextIndex < focusedItemIndex)
								focusedItemIndex = focusedItemIndex - 1;
							else 	if (nextIndex == focusedItemIndex)
								focusedItemIndex = paramItemIndex;
						} 
					}

					RaiseCollapseEvent(lvItem);

				}
				finally {
					base.EndUpdate();
				}

				if (focusedItemIndex >= 0) {
					ListViewItem lvi = base.Items[focusedItemIndex];
					lvi.Focused = true;
					lvi.Selected = true;
				}
			}	
		}

		/// <summary>
		/// The workhorse to expand a threaded listview item
		/// </summary>
		/// <param name="lvItem"></param>
		/// <param name="activate">true, if lvItem should be activated</param>
		internal void ExpandListViewItem(ThreadedListViewItem lvItem, bool activate) {

			int paramItemIndex;
			int currentIndent;
			int selectedItemIndex = -1;

			int selIdxsCount = base.SelectedIndices.Count;
			int[] selIdxs = new int[selIdxsCount];
			base.SelectedIndices.CopyTo(selIdxs, 0);

			ThreadedListViewItem[] newItems;

			if (lvItem != null && lvItem.Collapsed) {

				if ( RaiseBeforeExpandEventCancel(lvItem) ) {
					lvItem.StateImageIndex = 0;	// switch to non-state image (no +/-)
					return;
				}

				paramItemIndex = lvItem.Index;
				currentIndent = lvItem.IndentLevel;
				
				if (base.SelectedItems.Count > 0 && activate)
					selectedItemIndex = paramItemIndex;

				newItems = RaiseExpandEvent(lvItem);

				if (newItems == null) {
					ThreadedListViewItem item = _noChildsPlaceHolder;
					if (item == null) {
						item = new ThreadedListViewItemPlaceHolder(Resource.Manager["RES_FeedListNoChildsMessage"]);
						item.Font = new Font(this.Font.FontFamily,this.Font.Size, FontStyle.Regular);
					}
					newItems = new ThreadedListViewItem[]{item};
				}

				if (newItems.Length > 1 && this.ListViewItemSorter != null) {	
					// sort new child entries according to listview sortorder
					Array.Sort(newItems, this.ListViewItemSorter);
				}

				if (_showInGroups)
					APIEnableGrouping(false);

				base.BeginUpdate();
				try {
					
					lvItem.SetThreadState(true);

					lock (this.Items) {
						foreach (ThreadedListViewItem newListItem in newItems) {

							// check, if we have  all subitems for correct grouping
							while (newListItem.SubItems.Count < this.Columns.Count) {
								newListItem.SubItems.Add(String.Empty);
							}
						
							newListItem.Parent = lvItem;
							this.Items.Insert(paramItemIndex + 1, newListItem);
							newListItem.IndentLevel = currentIndent + 1;

							paramItemIndex++;
						}
					}
				} finally {
					base.EndUpdate();
				}

				this.RedrawItems();

				if (_showInGroups)
					APIEnableGrouping(true);
  
				// Make the last inserted subfolder visible, then the parent folder visible,
				// per default treeview behavior. 
				base.EnsureVisible(paramItemIndex-1);
				base.EnsureVisible(lvItem.Index);

				if (activate) {
					this.SelectedItems.Clear();
					lvItem.Selected = true;
					lvItem.Focused = true;
				} else if (selIdxsCount > 0) {
//					foreach (int i in selIdxs) {
//						this.Items[i].Selected = true;
//					}
				}
			}
		}

		/// <summary>
		/// Tests for relevant modifications related to the listview layout
		/// and raise the ListLayoutModified event
		/// </summary>
		public void CheckForLayoutModifications() {
			if (!this.Disposing && !this.IsDisposed && this._layout != null) {
				FeedColumnLayout current = this.FeedColumnLayoutFromCurrentSettings();
				if (!this._layout.Equals(current)) {
					this.RaiseFeedColumnLayoutModifiedEvent(current);
				}
			}
		}

		/// <summary>
		/// Take over any layout modifications, so CheckForLayoutModifications() will
		/// not raise the ListLayoutModified event
		/// </summary>
		public void ApplyLayoutModifications() {
			this._layout = this.FeedColumnLayoutFromCurrentSettings();
		}

		private FeedColumnLayout FeedColumnLayoutFromCurrentSettings() { 
			
			try {
				FeedColumnLayout layout = new FeedColumnLayout();
				lock(this.Columns) {
					if (_sorter.SortColumnIndex >= 0 && _sorter.SortColumnIndex  < this.Columns.Count) {

						layout.SortByColumn = this.Columns[_sorter.SortColumnIndex].Key;
						layout.SortOrder = ConvertSortOrder(_sorter.SortOrder);
					}
					int [] colOrder = this.GetColumnOrderArray();
					ArrayList aCols = new ArrayList(this.Columns.Count);
					ArrayList aColWidths = new ArrayList(this.Columns.Count);
					for (int i = 0; i < this.Columns.Count; i++) {
						aCols.Add(this.Columns[colOrder[i]].Key);
						aColWidths.Add(this.Columns[colOrder[i]].Width);
					}
					layout.Columns = aCols;
					layout.ColumnWidths = aColWidths;
				}

				return layout; 
			
			} catch {
				return null;
			}
		}

		private bool RaiseBeforeExpandEventCancel(ThreadedListViewItem tlv) {
			bool cancel = false;
			if (BeforeExpandThread != null) {
				ThreadCancelEventArgs e = new ThreadCancelEventArgs(tlv, cancel);
				try {
					BeforeExpandThread(this, e);
					cancel = e.Cancel;
				} catch {}
			}
			return cancel;
		}

		private ThreadedListViewItem[] RaiseExpandEvent(ThreadedListViewItem tlv) {
			if (ExpandThread != null) {
				ThreadEventArgs tea = new ThreadEventArgs(tlv);
				try {
					ExpandThread(this, tea);
				} catch {}
				if (tea.ChildItems != null) {
					return tea.ChildItems;
				}
			}
			return new ThreadedListViewItem[]{};
		}

		private bool RaiseBeforeCollapseEventCancel(ThreadedListViewItem tlv) {
			bool cancel = false;
			if (BeforeCollapseThread != null) {
				ThreadCancelEventArgs e = new ThreadCancelEventArgs(tlv, cancel);
				try {
					BeforeCollapseThread(this, e);
					cancel = e.Cancel;
				} catch {}
			}
			return cancel;
		}

		private void RaiseCollapseEvent(ThreadedListViewItem tlv) {
			if (CollapseThread != null) {
				ThreadEventArgs tea = new ThreadEventArgs(tlv);
				//TODO: add the thread child elements to eventArgs
				try {
					CollapseThread(this, tea);
				} catch {}
			}
		}

		private bool RaiseBeforeFeedColumnLayoutChangeEventCancel(FeedColumnLayout newLayout) {
			bool cancel = false;
			if (BeforeListLayoutChange != null) {
				ListLayoutCancelEventArgs e = new ListLayoutCancelEventArgs(newLayout, cancel);
				try {
					BeforeListLayoutChange(this, e);
					cancel = e.Cancel;
				} catch {}
			}
			return cancel;
		}

		private void RaiseFeedColumnLayoutChangedEvent(FeedColumnLayout layout) {
			if (ListLayoutChanged != null) {
				try {
					ListLayoutChanged(this, new ListLayoutEventArgs(layout));
				} catch {}
			}
		}

		private void RaiseFeedColumnLayoutModifiedEvent(FeedColumnLayout layout) {
			if (ListLayoutModified != null) {
				try {
					ListLayoutModified(this, new ListLayoutEventArgs(layout));
				} catch {}
			}
		}

		private void SetExtendedStyles() {
			Win32.LVS_EX ex_styles = (Win32.LVS_EX)Win32.API.SendMessage(this.Handle, Win32.W32_LVM.LVM_GETEXTENDEDLISTVIEWSTYLE, 0, 0);
			ex_styles |= Win32.LVS_EX.LVS_EX_DOUBLEBUFFER | Win32.LVS_EX.LVS_EX_INFOTIP | Win32.LVS_EX.LVS_EX_SUBITEMIMAGES;
			Win32.API.SendMessage(this.Handle, Win32.W32_LVM.LVM_SETEXTENDEDLISTVIEWSTYLE, 0, (int) ex_styles);
		}
		
		/// <summary>
		/// Insert items as a replacement for a placeholder item inserted before.
		/// The placeholder item will be identified by the placeHolderTicket to be provided.
		/// </summary>
		/// <param name="placeHolderTicket">The unique placeholder item's ticket. (string)</param>
		/// <param name="newChildItems">ThreadedListViewItem array</param>
		/// <param name="sortOnInsert">True uses the current sort column and order to insert childs. 
		/// Set to false, if you want to sort by your own.</param>
		/// <exception cref="ArgumentNullException">If placeHolderTicket is null or empty</exception>
		public void InsertItemsForPlaceHolder(string placeHolderTicket, ThreadedListViewItem[] newChildItems, bool sortOnInsert) {
			
			if (placeHolderTicket == null || placeHolderTicket.Length == 0)
				throw new ArgumentNullException("placeHolderTicket");

			ThreadedListViewItemPlaceHolder placeHolder = null;

			lock(this.Items) {
					
				for (int i=0; i < this.Items.Count; i++) {
					ThreadedListViewItemPlaceHolder p = this.Items[i] as ThreadedListViewItemPlaceHolder;
					if (p != null) {		// found one; check ticket
						if (p.InsertionPointTicket.Equals(placeHolderTicket)) {
							placeHolder = p;
							break;
						}
					}
				}				

				// the ThreadedListViewItemPlaceHolder to work on is not anymore displayed;
				// because of a refresh or user has collapsed the parent meanwhile
				if (placeHolder == null)
					return;	

				int parentItemIndex = placeHolder.Index;
				int parentIndentLevel = placeHolder.IndentLevel;

				try {
					this.BeginUpdate();

					// remove the placeholder
					this.Items.RemoveAt(parentItemIndex);

					if (newChildItems == null || newChildItems.Length == 0) {
						ThreadedListViewItem item = _noChildsPlaceHolder;
						if (item == null) {
							item = new ThreadedListViewItemPlaceHolder(Resource.Manager["RES_FeedListNoChildsMessage"]);
							item.Font = new Font(this.Font.FontFamily,this.Font.Size, FontStyle.Regular);
						}
						newChildItems = new ThreadedListViewItem[]{item};
					}

					if (newChildItems.Length > 1 && _sorter.SortOrder != SortOrder.None && sortOnInsert) {	
						// sort new child entries according to listview sortorder
						Array.Sort(newChildItems, _sorter.GetComparer());
					}

					// now insert the new items
					for (int i=0; i < newChildItems.Length; i++) {
										
						ThreadedListViewItem newListItem = newChildItems[i];
						newListItem.Parent = placeHolder.Parent;

						// check, if we have  all subitems for correct grouping
						while (newListItem.SubItems.Count < this.Columns.Count) {
							newListItem.SubItems.Add(String.Empty);
						}
						
						if (parentItemIndex < this.Items.Count) {
							//_log.Info("InsertItemsForPlaceHolder() insert at " + String.Format("{0}", parentItemIndex + 1));
							this.Items.Insert(parentItemIndex, newListItem);
						} else {
							//_log.Info("InsertItemsForPlaceHolder() append.");
							this.Items.Add(newListItem);
						}

						newListItem.IndentLevel = parentIndentLevel;	// only valid after the item is part of the listview items collection!
						parentItemIndex++;

					}//iterator.MoveNext

				} finally {
					this.EndUpdate();
				}
				
			}//lock

			this.UpdateItems();
		}

		private void APIEnableGrouping(bool value) {
			int param = 0, onOff = (value ? 1 : 0);
			Win32.API.SendMessage(this.Handle, Win32.W32_LVM.LVM_ENABLEGROUPVIEW, onOff, ref param); 
		}

		#region Header Sort and Graphics

		/// <summary>
		/// RefreshSortMarks uses LVM_GETHEADER and HDM_SETITEM to manipulate the header control
		/// of the underlying listview control.
		/// </summary>
		/// <param name="sortedColumnIndex">int. Index of the column that gets sorted</param>
		/// <param name="sortOrder">The sort order.</param>
		internal void RefreshSortMarks(int sortedColumnIndex, SortOrder sortOrder) {

			if (!base.IsHandleCreated) {
				return;
			}

			// get the pointer to the header:
			IntPtr hHeader = Win32.API.SendMessage(this.Handle, 
				Win32.W32_LVM.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            
			if (!hHeader.Equals(IntPtr.Zero)) {

				if (upBM == null) {
					upBM = GetBitmap(true);
					downBM = GetBitmap(false); 
				}
				try {

					lock(this.Columns) {
						for (int i = 0; i < this.Columns.Count; i++) {

							Win32.HDITEM item = new Win32.HDITEM();
							item.cchTextMax = 255;
							item.mask = (int)(Win32.HeaderItemMask.HDI_FORMAT | Win32.HeaderItemMask.HDI_TEXT | Win32.HeaderItemMask.HDI_BITMAP);

							int result = Win32.API.SendMessage(hHeader, Win32.HeaderControlMessages.HDM_GETITEM, new IntPtr(i), item);
							if (result > 0 && item != null) {
								
								ColumnHeader colHdr = this.Columns[i];
								HorizontalAlignment align = colHdr.TextAlign;
								string txt = (colHdr.Text == null ? String.Empty: colHdr.Text);
								item.pszText = Marshal.StringToHGlobalAuto(txt);
								item.mask = (int)(Win32.HeaderItemMask.HDI_FORMAT | Win32.HeaderItemMask.HDI_TEXT );
								item.fmt = (int)(Win32.HeaderItemFlags.HDF_STRING);

#if !NON_TRANSPARENT_SORTMARKERS
								item.mask |= (int)(Win32.HeaderItemMask.HDI_IMAGE);

								if(i == sortedColumnIndex && sortOrder != SortOrder.None) {
									if (align != HorizontalAlignment.Right)
										item.fmt |= (int)(Win32.HeaderItemFlags.HDF_BITMAP_ON_RIGHT);

									item.fmt |= (int)(Win32.HeaderItemFlags.HDF_IMAGE);
									item.iImage = (int) sortOrder - 1;
								}
#else
								item.mask |= (int)(Win32.HeaderItemMask.HDI_BITMAP);
								item.hbm = IntPtr.Zero;
								if(i == sortedColumnIndex && sortOrder != SortOrder.None) {
									item.fmt |= (int)(Win32.HeaderItemFlags.HDF_BITMAP);
									if (align != HorizontalAlignment.Right)
										item.fmt |= (int)(Win32.HeaderItemFlags.HDF_BITMAP_ON_RIGHT);

									if (sortOrder == SortOrder.Ascending)
										item.hbm = upBM.GetHbitmap();
									else
										item.hbm = downBM.GetHbitmap();
								}
#endif
								Win32.API.SendMessage(hHeader, Win32.HeaderControlMessages.HDM_SETITEM, new IntPtr(i), item);
								Marshal.FreeHGlobal(item.pszText);
							} 
						}
					}
				} catch (Exception ex) {
					Trace.WriteLine("RefreshSortMarks() error: " + ex.Message);
				}
			}
		}

		private void SetHeaderImageList() {
			if (! _headerImageListLoaded) {
				_headerImageList = new ImageList(this.components);
				_headerImageList.ImageSize = (_isWinXP ? new Size(9, 9): new Size(8, 8));
				_headerImageList.TransparentColor = System.Drawing.Color.Magenta;

				_headerImageList.Images.Add(this.GetBitmap(true));		// Add ascending arrow
				_headerImageList.Images.Add(this.GetBitmap(false));		// Add descending arrow

				IntPtr hHeader = Win32.API.SendMessage(this.Handle, 
					Win32.W32_LVM.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);	
				Win32.API.SendMessage(hHeader, Win32.HeaderControlMessages.HDM_SETIMAGELIST, IntPtr.Zero, _headerImageList.Handle);
				
				_headerImageListLoaded = true;
			}
		}

		private Bitmap GetBitmap(bool ascending) {
			if (_isWinXP) {	// draw the flat triangle
				Bitmap bm = new Bitmap(9, 9);
				Graphics gfx = Graphics.FromImage(bm);

				Brush fillBrush = SystemBrushes.ControlDark;

#if !NON_TRANSPARENT_SORTMARKERS
				gfx.FillRectangle(Brushes.Magenta, 0, 0, 9, 9);
#else
				// XP ListView column header back color:
				Brush backBrush = new SolidBrush(Color.FromArgb(235, 234, 219));
				gfx.FillRectangle(backBrush /* SystemBrushes.ControlLight */ , 0, 0, 9, 9);
				backBrush.Dispose();
#endif
				GraphicsPath path = new GraphicsPath();

				if (ascending) {
					// Draw triangle pointing upwards
					path.AddLine(4, 1, -1, 7);
					path.AddLine(-1, 7, 9, 7);
					path.AddLine(9, 7, 4, 1);
					gfx.FillPath(fillBrush, path);
				} else {
					// Draw triangle pointing downwards
					path.AddLine(0, 2, 9, 2);
					path.AddLine(9, 2, 4, 7);
					path.AddLine(4, 7, 0, 2);
					gfx.FillPath(fillBrush, path);
				}

				path.Dispose();
				gfx.Dispose();
				
				return bm;
			
			} else {

				Bitmap bm = new Bitmap(8, 8);
				Graphics gfx = Graphics.FromImage(bm);

				Pen lightPen = SystemPens.ControlLightLight;
				Pen shadowPen = SystemPens.ControlDark;

#if !NON_TRANSPARENT_SORTMARKERS
				gfx.FillRectangle(Brushes.Magenta, 0, 0, 8, 8);
#else
				gfx.FillRectangle(SystemBrushes.ControlLight, 0, 0, 8, 8);
#endif

				if (ascending) {
					// Draw triangle pointing upwards
					gfx.DrawLine(lightPen, 0, 7, 7, 7);
					gfx.DrawLine(lightPen, 7, 7, 4, 0);
					gfx.DrawLine(shadowPen, 3, 0, 0, 7);
				} else {
					// Draw triangle pointing downwards
					gfx.DrawLine(lightPen, 4, 7, 7, 0);
					gfx.DrawLine(shadowPen, 3, 7, 0, 0);
					gfx.DrawLine(shadowPen, 0, 0, 7, 0);
				}

				gfx.Dispose();

				return bm;
			}
		}

		#endregion

		private void OnBeforeSort(object sender, EventArgs e) {
			this.SetHeaderImageList();	
			if (_showInGroups) { 
				APIEnableGrouping(false);
			} 
		}

		private void OnAfterSort(object sender, EventArgs e) {
			if (_showInGroups) { 
				APIEnableGrouping(true);
				if (_autoGroup == true) { 
					AutoGroupByColumn(_autoGroupCol.Index); 
				} else { 
					Regroup(); 
				} 
			} 
		}

		private static System.Windows.Forms.SortOrder ConvertSortOrder(NewsComponents.SortOrder sortOrder){
		
		
			if(sortOrder == NewsComponents.SortOrder.Ascending){
				return System.Windows.Forms.SortOrder.Ascending;
			}else if(sortOrder == NewsComponents.SortOrder.Descending){
				return System.Windows.Forms.SortOrder.Descending;
			}else{
				return System.Windows.Forms.SortOrder.None;
			}
		
		}

		private static NewsComponents.SortOrder ConvertSortOrder(System.Windows.Forms.SortOrder sortOrder){
		
		
			if(sortOrder == System.Windows.Forms.SortOrder.Ascending){
				return NewsComponents.SortOrder.Ascending;
			}else if(sortOrder == System.Windows.Forms.SortOrder.Descending){
				return NewsComponents.SortOrder.Descending;
			}else{
				return NewsComponents.SortOrder.None;
			}
		
		}
	}
}
