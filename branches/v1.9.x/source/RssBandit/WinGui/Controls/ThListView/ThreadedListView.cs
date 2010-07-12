#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#define NON_TRANSPARENT_SORTMARKERS

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using log4net;
using RssBandit.Common.Logging;
using RssBandit.WinGui.Controls.ThListView.Sorting;
using RssBandit.Resources;
using System.Collections.Generic;

namespace RssBandit.WinGui.Controls.ThListView
{
    /// <summary>
    /// ThreadedListView, an extended listview control.
    /// </summary>
    /// <remarks>
    /// Rework required: CLR 2.0 ListView already implement
    /// Groups and VirtualMode (XP and 2003 only, if visual styles enabled)
    /// </remarks>
    public class ThreadedListView : ListView
    {
        //private static readonly log4net.ILog _log = Log.GetLogger(typeof(ThreadedListView));

        private ImageList imageListStates;

        private Bitmap upBM, downBM; // the 2 bitmaps used for drawing the sort triangles
        private bool _isWinXP, _headerImageListLoaded;
        private ImageList _headerImageList;
        private ThreadedListViewSorter _sorter;
        private ThreadedListViewColumnHeader _autoGroupCol;
        private readonly List<string> _autoGroupList = new List<string>();
        private readonly ThreadedListViewItemCollection _items;
        private readonly ThreadedListViewColumnHeaderCollection _columns;
        private readonly ThreadedListViewGroupCollection _groups;
        private FeedColumnLayout _layout;
        private bool _showInGroups;
        private bool _autoGroup;
        private bool _threadedView = true;
        private string _emptyAutoGroupText = String.Empty;
        //private IntPtr _apiRetVal;
        private ThreadedListViewItem _noChildsPlaceHolder;
		private static readonly ILog _log = Log.GetLogger(typeof(ThreadedListView));


        public event EventHandler<ListLayoutCancelEventArgs> BeforeListLayoutChange;


        public event EventHandler<ListLayoutEventArgs> ListLayoutChanged;


        public event EventHandler<ListLayoutEventArgs> ListLayoutModified;

        public event EventHandler<ThreadCancelEventArgs> BeforeExpandThread;


        public event EventHandler<ThreadCancelEventArgs> BeforeCollapseThread;

        public event EventHandler<ThreadEventArgs> ExpandThread;


        public event EventHandler<ThreadEventArgs> AfterExpandThread;


        public event EventHandler<ThreadEventArgs> CollapseThread;

        private IContainer components;

        public ThreadedListView()
        {
            _items = new ThreadedListViewItemCollection(this);
            _groups = new ThreadedListViewGroupCollection(this);
            _columns = new ThreadedListViewColumnHeaderCollection(this);
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            InitListView();
        }

        internal void SetColumnOrderArray(int[] orderArray)
        {
            Win32.API.SetColumnOrderArray(Handle, orderArray);
        }

        internal int[] GetColumnOrderArray()
        {
            return Win32.API.GetColumnOrderArray(Handle, Columns.Count);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public new bool CheckBoxes
        {
            get { return base.CheckBoxes; }
            set { base.CheckBoxes = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public new ImageList StateImageList
        {
            get { return base.StateImageList; }
            set { base.StateImageList = value; }
        }

        private void InitListView()
        {
            _headerImageListLoaded = false;
            _isWinXP = RssBandit.Win32.IsOSAtLeastWindowsXP;
        	
			DoubleBuffered = true;
        	ShowItemToolTips = true;
			
			// our core state:
            ShowAsThreads = true;

            // layout:
            View = View.Details;

            // sorting:
            _sorter = new ThreadedListViewSorter(this);
            _sorter.BeforeSort += OnBeforeSort;
            _sorter.AfterSort += OnAfterSort;
            // events:
            HandleCreated += OnListviewHandleCreated;
            MouseDown += OnListviewMouseDown;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            var resources = new System.Resources.ResourceManager(typeof (ThreadedListView));
            this.imageListStates = new System.Windows.Forms.ImageList(this.components);
            // 
            // imageListStates
            // 
            this.imageListStates.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListStates.ImageStream =
                ((System.Windows.Forms.ImageListStreamer) (resources.GetObject("imageListStates.ImageStream")));
            this.imageListStates.TransparentColor = System.Drawing.Color.Transparent;
        }

        #endregion

        // no real implementation, so it is commented out for now.
        // kept for ref. or future code...

//		protected override void WndProc(ref Message m) {
//			bool handled = false;
//			
//			// determine if we want to process this message type
//			switch ( m.Msg ) {
//					// notify messages can come from the header and are used 
//					// for column sorting and item filtering if wanted
//				case (int)Win32.W32_WM.WM_NOTIFY:
//					break;
//
//					// internal ListView notify messages are reflected to us via OCM
//				case (int)Win32.W32_OCM.OCM_NOTIFY:
//					//RefreshSortMarks(_sorter.SortColumnIndex, _sorter.SortOrder);
//					break;
//
//				case (int)Win32.W32_WM.WM_PAINT:
//					//TODO: next code does not work...
////					int _autoSizeColumnIndex = 0;
////					if (this.View == View.Details && this.Columns.Count > _autoSizeColumnIndex) {
////						this.Columns[_autoSizeColumnIndex].Width = -2;
////					}
//					break;
//
//			}
//
//			if (!handled)
//				base.WndProc(ref m);
//		}

        private void OnListviewHandleCreated(object sender, EventArgs e)
        {
            SetExtendedStyles();
        }

        private void OnListviewMouseDown(object sender, MouseEventArgs e)
        {
            if (base.Items.Count <= 0)
                return;

            if (e.Button != MouseButtons.Left) // expand only on left click
                return;

            var lvi = GetItemAt(e.X, e.Y) as ThreadedListViewItem;
            if (lvi != null && lvi.StateImageHitTest(new Point(e.X, e.Y)))
            {
                if (lvi.HasChilds)
                {
                    if (lvi.Expanded == false)
                        ExpandListViewItem(lvi, false);
                    else if (lvi.Expanded)
                        CollapseListViewItem(lvi);
                }
            }
            else if (lvi != null && e.Clicks > 1)
            {
                // double click:
                if (lvi.HasChilds)
                {
                    if (lvi.Expanded == false)
                    {
                        ExpandListViewItem(lvi, true);
                    }
                    else if (lvi.Expanded)
                    {
                        CollapseListViewItem(lvi);
                    }
                }
            }
        }

        #region Designer Properties, Grouping, etc.

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         Description("the items collection of this view"),
         /* Editor(typeof (ThreadedListViewItemCollectionEditor), typeof (UITypeEditor)), */
         Category("Behavior")]
        public new ThreadedListViewItemCollection Items
        {
            get { return _items; }
        }

        //TODO designer support
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         Description("the header columns collection of this view"),
        //Editor(typeof(ThreadedListViewItemCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)),
         Category("Behavior")]
        public new ThreadedListViewColumnHeaderCollection Columns
        {
            get { return _columns; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         Description("collection of available groups (manually added)"),
         //Editor(typeof (CollectionEditor), typeof (UITypeEditor)), // We don't design this
         Category("Grouping")]
        public new ThreadedListViewGroupCollection Groups
        {
            get { return _groups; }
        }

        [Category("Grouping"),
         Description("flag if the grouping view is active"),
         DefaultValue(false)]
        public bool ShowInGroups
        {
            get { return _showInGroups; }
            set
            {
                if (_showInGroups != value)
                {
                    _showInGroups = value;
                    if (_showInGroups)
                        ShowAsThreads = false;
                    if (_autoGroup && value == false)
                    {
                        _autoGroup = false;
                        _autoGroupCol = null;
                        _autoGroupList.Clear();
                    }

                    APIEnableGrouping(value);
                }
            }
        }

        [Category("Grouping"),
         Description("flag if the autogroup mode is active"),
         DefaultValue(false)]
        public bool AutoGroupMode
        {
            get { return _autoGroup; }
            set
            {
                _autoGroup = value;
                if (_autoGroup)
                    ShowAsThreads = false;
                if (_autoGroupCol != null)
                {
                    AutoGroupByColumn(_autoGroupCol.Index);
                }
            }
        }

        [Category("Grouping"),
         Description("column by with values the listiew is automatically grouped"),
         DefaultValue(typeof (ColumnHeader), ""),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ThreadedListViewColumnHeader AutoGroupColumn
        {
            get { return _autoGroupCol; }
            set
            {
                _autoGroupCol = value;

                if (_autoGroupCol != null)
                {
                    AutoGroupByColumn(_autoGroupCol.Index);
                }
            }
        }

        [Category("Grouping"),
         Description("the text that is displayed instead of an empty auto group text"),
         DefaultValue("")]
        public string EmptyAutoGroupText
        {
            get { return _emptyAutoGroupText; }
            set
            {
                _emptyAutoGroupText = value;

                if (_autoGroupCol != null)
                {
                    AutoGroupByColumn(_autoGroupCol.Index);
                }
            }
        }

        [Browsable(false),
         Description("readonly array of all automatically created groups"),
         Category("Grouping")]
        public string[] Autogroups
        {
            get { return _autoGroupList.ToArray(); }
        }

        public bool AutoGroupByColumn(int columnID)
        {
            if (columnID >= Columns.Count || columnID < 0)
            {
                return false;
            }

            try
            {
                _autoGroupList.Clear();

                foreach (var itm in Items)
                {
                    if (
                        !_autoGroupList.Contains(itm.SubItems[columnID].Text == String.Empty
                                                     ? _emptyAutoGroupText
                                                     : itm.SubItems[columnID].Text))
                    {
                        _autoGroupList.Add(itm.SubItems[columnID].Text == String.Empty
                                               ? EmptyAutoGroupText
                                               : itm.SubItems[columnID].Text);
                    }
                }

                _autoGroupList.Sort();

                Win32.API.ClearListViewGroup(Handle);
                foreach (var text in _autoGroupList)
                {
                    Win32.API.AddListViewGroup(Handle, text, _autoGroupList.IndexOf(text));
                }

                foreach (var itm in Items)
                {
                    int index =
                        _autoGroupList.IndexOf(itm.SubItems[columnID].Text == ""
                                                   ? _emptyAutoGroupText
                                                   : itm.SubItems[columnID].Text);
                    Win32.API.AddItemToGroup(Handle, itm.Index, index);
                }

                APIEnableGrouping(true);
                _showInGroups = true;
                _autoGroup = true;
                _autoGroupCol = Columns[columnID];

                Refresh();

                return true;
            }
            catch (Exception ex)
            {
                throw new SystemException("Error in ThreadedListView.AutoGroupByColumn: " + ex.Message);
            }
        }

        public ThreadedListViewItem NoThreadChildsPlaceHolder
        {
            get { return _noChildsPlaceHolder; }
            set { _noChildsPlaceHolder = value; }
        }

        public bool Regroup()
        {
            try
            {
                Win32.API.ClearListViewGroup(Handle);
                foreach (ThreadedListViewGroup grp in Groups)
                {
                    Win32.API.AddListViewGroup(Handle, grp.GroupText, grp.GroupIndex);
                }

                foreach (var itm in Items)
                {
                    Win32.API.AddItemToGroup(Handle, itm.Index, itm.GroupIndex);
                }

                APIEnableGrouping(true);
                _showInGroups = true;
                _autoGroup = false;
                _autoGroupCol = null;
                _autoGroupList.Clear();

                return true;
            }
            catch (Exception ex)
            {
                throw new SystemException("Error in ThreadedListView.Regroup: " + ex.Message);
            }
        }

        #endregion

        #region other public members

        public ThreadedListViewSorter SortManager
        {
            get { return _sorter; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FeedColumnLayout FeedColumnLayout
        {
            get { return FeedColumnLayoutFromCurrentSettings(); }
            set
            {
                if (value != null)
                {
                    FeedColumnLayout newLayout = value;

                    if (!RaiseBeforeFeedColumnLayoutChangeEventCancel(newLayout))
                    {
                        _layout = newLayout;
                        // enable re-build of columns:
                        RaiseFeedColumnLayoutChangedEvent(newLayout);
                        //TODO: safety - ensure, some columns are added. 
                        if (newLayout.SortOrder != SortOrder.None)
                            _sorter.Sort(Columns.GetIndexByKey(newLayout.SortByColumn),
                                         newLayout.SortOrder);
                    }
                }
            }
        }

        [Category("Behavior"),
         Description("flag if the threaded view mode is active"),
         DefaultValue(true)]
        public bool ShowAsThreads
        {
            get { return _threadedView; }
            set
            {
                _threadedView = value;
                if (_threadedView)
                {
                    ShowInGroups = false;
                    AutoGroupMode = false;
                    CheckBoxes = false;
                    StateImageList = imageListStates;
                    ReApplyItemStates();
                }
                else
                {
                    StateImageList = null;
                }
            }
        }

        internal void RedrawItems()
        {
            Win32.API.RedrawItems(this, true);
            ArrangeIcons();
        }


        internal void UpdateItems()
        {
            Win32.API.UpdateItems(this);
        }


        public void SetColumnStyle(int column, Font font, Color foreColor, Color backColor)
        {
            SuspendLayout();

            foreach (var itm in Items)
            {
                if (itm.SubItems.Count > column)
                {
                    itm.SubItems[column].Font = font;
                    itm.SubItems[column].BackColor = backColor;
                    itm.SubItems[column].ForeColor = foreColor;
                }
            }

            ResumeLayout();
        }


        public void SetColumnStyle(int column, Font font, Color foreColor)
        {
            SetColumnStyle(column, font, foreColor, BackColor);
        }


        public void SetColumnStyle(int column, Font font)
        {
            SetColumnStyle(column, font, ForeColor, BackColor);
        }


        public void ResetColumnStyle(int column)
        {
            SuspendLayout();

            foreach (var itm in Items)
            {
                if (itm.SubItems.Count > column)
                {
                    itm.SubItems[column].ResetStyle();
                }
            }

            ResumeLayout();
        }


        public void SetBackgroundImage(string imagePath, ImagePosition position)
        {
            Win32.API.SetListViewImage(Handle, imagePath, position);
        }

        #endregion

        private void ReApplyItemStates()
        {
            if (base.Items.Count == 0)
                return;
            foreach (var lv in Items)
            {
                if (lv.HasChilds)
                    lv.Expanded = !lv.Collapsed;
            }
        }

        /// <summary>
        /// The workhorse to collapse a threaded listview item
        /// </summary>
        /// <param name="lvItem"></param>
        internal void CollapseListViewItem(ThreadedListViewItem lvItem)
        {
            if (lvItem != null && lvItem.Expanded)
            {
                if (RaiseBeforeCollapseEventCancel(lvItem))
                {
                    lvItem.StateImageIndex = 0;
                    return;
                }

                int focusedItemIndex;
                if (FocusedItem == null)
                    focusedItemIndex = lvItem.Index;
                else
                    focusedItemIndex = FocusedItem.Index;

                BeginUpdate();
                try
                {
                    lvItem.SetThreadState(false);
                    int paramItemIndex = lvItem.Index;
                    int currentIndent = lvItem.IndentLevel;
                    int nextIndex = paramItemIndex + 1;

                    lock (Items)
                    {
                        while (nextIndex < base.Items.Count &&
                               ((ThreadedListViewItem) base.Items[nextIndex]).IndentLevel > currentIndent)
                        {
                            Items[nextIndex].Parent = null;
                            Items.RemoveAt(nextIndex);
                            if (nextIndex < focusedItemIndex)
                                focusedItemIndex = focusedItemIndex - 1;
                            else if (nextIndex == focusedItemIndex)
                                focusedItemIndex = paramItemIndex;
                        }
                    }

                    RaiseCollapseEvent(lvItem);
                }
                finally
                {
                    EndUpdate();
                }

                if (focusedItemIndex >= 0)
                {
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
        internal void ExpandListViewItem(ThreadedListViewItem lvItem, bool activate)
        {
            int selIdxsCount = SelectedIndices.Count;
            var selIdxs = new int[selIdxsCount];
            SelectedIndices.CopyTo(selIdxs, 0);

            ThreadedListViewItem[] newItems;

            if (lvItem != null && lvItem.Collapsed)
            {
                if (RaiseBeforeExpandEventCancel(lvItem))
                {
                    lvItem.StateImageIndex = 0; // switch to non-state image (no +/-)
                    return;
                }

                int paramItemIndex = lvItem.Index;
                int currentIndent = lvItem.IndentLevel;

//				if (base.SelectedItems.Count > 0 && activate)
//					selectedItemIndex = paramItemIndex;

                newItems = RaiseExpandEvent(lvItem);

                if (newItems == null)
                {
                    ThreadedListViewItem item = _noChildsPlaceHolder;
                    if (item == null)
                    {
                        item = new ThreadedListViewItemPlaceHolder(SR.FeedListNoChildsMessage)
                                   {
                                       Font = new Font(Font.FontFamily, Font.Size, FontStyle.Regular)
                                   };
                    }
                    newItems = new[] {item};
                }

                if (newItems.Length > 1 && ListViewItemSorter != null)
                {
                    // sort new child entries according to listview sortorder
                    Array.Sort(newItems, ListViewItemSorter);
                }

                if (_showInGroups)
                    APIEnableGrouping(false);

                BeginUpdate();
                try
                {
                    lvItem.SetThreadState(true);

                    lock (Items)
                    {
                        foreach (var newListItem in newItems)
                        {
                            // check, if we have  all subitems for correct grouping
                            while (newListItem.SubItems.Count < Columns.Count)
                            {
                                newListItem.SubItems.Add(String.Empty);
                            }

                            newListItem.Parent = lvItem;
                            Items.Insert(paramItemIndex + 1, newListItem);
                            newListItem.IndentLevel = currentIndent + 1;

                            paramItemIndex++;
                        }
                    }
                }
                finally
                {
                    EndUpdate();
                }

                RedrawItems();

                if (_showInGroups)
                    APIEnableGrouping(true);

                // Make the last inserted subfolder visible, then the parent folder visible,
                // per default treeview behavior. 
                try
                {
                    EnsureVisible(paramItemIndex - 1);
                    EnsureVisible(lvItem.Index);
                }
                catch (Exception ex)
                {
					_log.Error("EnsureVisible() failed", ex);
                }

                if (activate)
                {
                    SelectedItems.Clear();
                    lvItem.Selected = true;
                    lvItem.Focused = true;
                }
                else if (selIdxsCount > 0)
                {
//					foreach (int i in selIdxs) {
//						this.Items[i].Selected = true;
//					}
                }

                RaiseAfterExpandEvent(lvItem, newItems);
            }
        }

        /// <summary>
        /// Tests for relevant modifications related to the listview layout
        /// and raise the ListLayoutModified event
        /// </summary>
        public void CheckForLayoutModifications()
        {
            FeedColumnLayout layout = _layout;
            if (layout != null)
            {
                GuiInvoker.Invoke(this, delegate
                                                 {
                                                     FeedColumnLayout current = FeedColumnLayoutFromCurrentSettings();

                                                     if (!layout.Equals(current))
                                                     {
                                                         RaiseFeedColumnLayoutModifiedEvent(current);
                                                     }
                                                 });
            }
        }

        /// <summary>
        /// Take over any layout modifications, so CheckForLayoutModifications() will
        /// not raise the ListLayoutModified event
        /// </summary>
        public void ApplyLayoutModifications()
        {
            _layout = FeedColumnLayoutFromCurrentSettings();
        }

        private FeedColumnLayout FeedColumnLayoutFromCurrentSettings()
        {
            try
            {
                var layout = new FeedColumnLayout();
                lock (Columns)
                {
                    if (_sorter.SortColumnIndex >= 0 && _sorter.SortColumnIndex < Columns.Count)
                    {
                        layout.SortByColumn = Columns[_sorter.SortColumnIndex].Key;
                        layout.SortOrder = _sorter.SortOrder;
                    }
                    int[] colOrder = GetColumnOrderArray();
                    var aCols = new List<string>(Columns.Count);
                    var aColWidths = new List<int>(Columns.Count);
                    for (int i = 0; i < Columns.Count; i++)
                    {
                        aCols.Add(Columns[colOrder[i]].Key);
                        aColWidths.Add(Columns[colOrder[i]].Width);
                    }
                    layout.Columns = aCols;
                    layout.ColumnWidths = aColWidths;
                }

                return layout;
            }
            catch
            {
                return null;
            }
        }

        private bool RaiseBeforeExpandEventCancel(ThreadedListViewItem tlv)
        {
            bool cancel = false;
            if (BeforeExpandThread != null)
            {
                var e = new ThreadCancelEventArgs(tlv, false);
                try
                {
                    BeforeExpandThread(this, e);
                    cancel = e.Cancel;
                }
                catch (Exception ex)
                {
					_log.Error("Event BeforeExpandThread() failed", ex);
                }
            }
            return cancel;
        }

        private ThreadedListViewItem[] RaiseExpandEvent(ThreadedListViewItem tlv)
        {
            if (ExpandThread != null)
            {
                var tea = new ThreadEventArgs(tlv);
                try
                {
                    ExpandThread(this, tea);
                }
				catch (Exception ex)
				{
					_log.Error("Event ExpandThread() failed", ex);
				}
                if (tea.ChildItems != null)
                {
                    return tea.ChildItems;
                }
            }
            return new ThreadedListViewItem[] {};
        }

        private void RaiseAfterExpandEvent(ThreadedListViewItem parent, ThreadedListViewItem[] newItems)
        {
            if (AfterExpandThread != null)
            {
                var tea = new ThreadEventArgs(parent)
                              {
                                  ChildItems = newItems
                              };
                try
                {
                    AfterExpandThread(this, tea);
                }
				catch (Exception ex)
				{
					_log.Error("Event AfterExpandThread() failed", ex);
				}
            }
        }

        private bool RaiseBeforeCollapseEventCancel(ThreadedListViewItem tlv)
        {
            bool cancel = false;
            if (BeforeCollapseThread != null)
            {
                var e = new ThreadCancelEventArgs(tlv, false);
                try
                {
                    BeforeCollapseThread(this, e);
                    cancel = e.Cancel;
                }
				catch (Exception ex)
				{
					_log.Error("Event BeforeCollapseThread() failed", ex);
				}
            }
            return cancel;
        }

        private void RaiseCollapseEvent(ThreadedListViewItem tlv)
        {
            if (CollapseThread != null)
            {
                var tea = new ThreadEventArgs(tlv);
                //TODO: add the thread child elements to eventArgs
                try
                {
                    CollapseThread(this, tea);
                }
				catch (Exception ex)
				{
					_log.Error("Event CollapseThread() failed", ex);
				}
            }
        }

        private bool RaiseBeforeFeedColumnLayoutChangeEventCancel(FeedColumnLayout newLayout)
        {
            bool cancel = false;
            if (BeforeListLayoutChange != null)
            {
                var e = new ListLayoutCancelEventArgs(newLayout, false);
                try
                {
                    BeforeListLayoutChange(this, e);
                    cancel = e.Cancel;
                }
				catch (Exception ex)
				{
					_log.Error("Event BeforeListLayoutChange() failed", ex);
				}
            }
            return cancel;
        }

        private void RaiseFeedColumnLayoutChangedEvent(FeedColumnLayout layout)
        {
            if (ListLayoutChanged != null)
            {
                try
                {
                    ListLayoutChanged(this, new ListLayoutEventArgs(layout));
                }
				catch (Exception ex)
				{
					_log.Error("Event ListLayoutChanged() failed", ex);
				}
            }
        }

        private void RaiseFeedColumnLayoutModifiedEvent(FeedColumnLayout layout)
        {
            if (ListLayoutModified != null)
            {
                try
                {
                    ListLayoutModified(this, new ListLayoutEventArgs(layout));
                }
				catch (Exception ex)
				{
					_log.Error("Event ListLayoutModified() failed", ex);
				}
            }
        }

        private void SetExtendedStyles()
        {
            var ex_styles = (Win32.LVS_EX) Win32.API.SendMessage(Handle, Win32.W32_LVM.LVM_GETEXTENDEDLISTVIEWSTYLE, 0, IntPtr.Zero);
            ex_styles |= Win32.LVS_EX.LVS_EX_SUBITEMIMAGES;
            Win32.API.SendMessage(Handle, Win32.W32_LVM.LVM_SETEXTENDEDLISTVIEWSTYLE, 0, new IntPtr((int) ex_styles));
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
        public void InsertItemsForPlaceHolder(string placeHolderTicket, ThreadedListViewItem[] newChildItems,
                                              bool sortOnInsert)
        {
            if (string.IsNullOrEmpty(placeHolderTicket))
                throw new ArgumentNullException("placeHolderTicket");

            ThreadedListViewItemPlaceHolder placeHolder = null;

            lock (Items)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var p = Items[i] as ThreadedListViewItemPlaceHolder;
                    if (p != null)
                    {
                        // found one; check ticket
                        if (p.InsertionPointTicket.Equals(placeHolderTicket))
                        {
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

                try
                {
                    BeginUpdate();

                    // remove the placeholder
                    Items.RemoveAt(parentItemIndex);

                    if (newChildItems == null || newChildItems.Length == 0)
                    {
                        ThreadedListViewItem item = _noChildsPlaceHolder;
                        if (item == null)
                        {
                            item = new ThreadedListViewItemPlaceHolder(SR.FeedListNoChildsMessage)
                                       {
                                           Font = new Font(Font.FontFamily, Font.Size, FontStyle.Regular)
                                       };
                        }
                        newChildItems = new[] {item};
                    }

                    if (newChildItems.Length > 1 && _sorter.SortOrder != SortOrder.None && sortOnInsert)
                    {
                        // sort new child entries according to listview sortorder
                        Array.Sort(newChildItems, _sorter.GetComparer());
                    }

                    // now insert the new items
                    for (int i = 0; i < newChildItems.Length; i++)
                    {
                        ThreadedListViewItem newListItem = newChildItems[i];
                        newListItem.Parent = placeHolder.Parent;

                        // check, if we have  all subitems for correct grouping
                        while (newListItem.SubItems.Count < Columns.Count)
                        {
                            newListItem.SubItems.Add(String.Empty);
                        }

                        if (parentItemIndex < Items.Count)
                        {
                            //_log.Info("InsertItemsForPlaceHolder() insert at " + String.Format("{0}", parentItemIndex + 1));
                            Items.Insert(parentItemIndex, newListItem);
                        }
                        else
                        {
                            //_log.Info("InsertItemsForPlaceHolder() append.");
                            Items.Add(newListItem);
                        }

                        newListItem.IndentLevel = parentIndentLevel;
                        // only valid after the item is part of the listview items collection!
                        parentItemIndex++;
                    } //iterator.MoveNext
                }
                finally
                {
                    EndUpdate();
                }
            } //lock

            UpdateItems();
        }

        private void APIEnableGrouping(bool value)
        {
            IntPtr param = IntPtr.Zero;
            int onOff = (value ? 1 : 0);
            Win32.API.SendMessage(Handle, Win32.W32_LVM.LVM_ENABLEGROUPVIEW, onOff, ref param);
        }

        #region Header Sort and Graphics

        /// <summary>
        /// RefreshSortMarks uses LVM_GETHEADER and HDM_SETITEM to manipulate the header control
        /// of the underlying listview control.
        /// </summary>
        /// <param name="sortedColumnIndex">int. Index of the column that gets sorted</param>
        /// <param name="sortOrder">The sort order.</param>
        internal void RefreshSortMarks(int sortedColumnIndex, SortOrder sortOrder)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            // get the pointer to the header:
            IntPtr hHeader = Win32.API.SendMessage(Handle,
                                                   Win32.W32_LVM.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            if (!hHeader.Equals(IntPtr.Zero))
            {
                if (upBM == null)
                {
                    upBM = GetBitmap(true);
                    downBM = GetBitmap(false);
                }
                try
                {
                    lock (Columns)
                    {
                        for (int i = 0; i < Columns.Count; i++)
                        {
                            var item = new Win32.HDITEM
                                           {
                                               cchTextMax = 255,
                                               mask = ((int)
                                                       (Win32.HeaderItemMask.HDI_FORMAT | Win32.HeaderItemMask.HDI_TEXT |
                                                        Win32.HeaderItemMask.HDI_BITMAP))
                                           };

                            IntPtr result =
                                Win32.API.SendMessage(hHeader, Win32.HeaderControlMessages.HDM_GETITEM, new IntPtr(i),
                                                      item);
                            if (result.ToInt32() > 0)
                            {
                                ColumnHeader colHdr = Columns[i];
                                HorizontalAlignment align = colHdr.TextAlign;
                                string txt = (colHdr.Text ?? String.Empty);
                                item.pszText = Marshal.StringToHGlobalAuto(txt);
                                item.mask = (int) (Win32.HeaderItemMask.HDI_FORMAT | Win32.HeaderItemMask.HDI_TEXT);
                                item.fmt = (int) (Win32.HeaderItemFlags.HDF_STRING);

#if !NON_TRANSPARENT_SORTMARKERS
								item.mask |= (int)(Win32.HeaderItemMask.HDI_IMAGE);

								if(i == sortedColumnIndex && sortOrder != SortOrder.None) {
									if (align != HorizontalAlignment.Right)
										item.fmt |= (int)(Win32.HeaderItemFlags.HDF_BITMAP_ON_RIGHT);

									item.fmt |= (int)(Win32.HeaderItemFlags.HDF_IMAGE);
									item.iImage = (int) sortOrder - 1;
								}
#else
                                item.mask |= (int) (Win32.HeaderItemMask.HDI_BITMAP);
                                item.hbm = IntPtr.Zero;
                                if (i == sortedColumnIndex && sortOrder != SortOrder.None)
                                {
                                    item.fmt |= (int) (Win32.HeaderItemFlags.HDF_BITMAP);
                                    if (align != HorizontalAlignment.Right)
                                        item.fmt |= (int) (Win32.HeaderItemFlags.HDF_BITMAP_ON_RIGHT);

                                    item.hbm = sortOrder == SortOrder.Ascending
                                                   ? upBM.GetHbitmap()
                                                   : downBM.GetHbitmap();
                                }
#endif
                                Win32.API.SendMessage(hHeader, Win32.HeaderControlMessages.HDM_SETITEM, new IntPtr(i),
                                                      item);
                                Marshal.FreeHGlobal(item.pszText);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("RefreshSortMarks() error: " + ex.Message);
                }
            }
        }

        private void SetHeaderImageList()
        {
            if (! _headerImageListLoaded)
            {
                _headerImageList = new ImageList(components);
                _headerImageList.ImageSize = (_isWinXP ? new Size(9, 9) : new Size(8, 8));
                _headerImageList.TransparentColor = Color.Magenta;

                _headerImageList.Images.Add(GetBitmap(true)); // Add ascending arrow
                _headerImageList.Images.Add(GetBitmap(false)); // Add descending arrow

                IntPtr hHeader = Win32.API.SendMessage(Handle,
                                                       Win32.W32_LVM.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
                Win32.API.SendMessage(hHeader, Win32.HeaderControlMessages.HDM_SETIMAGELIST, IntPtr.Zero,
                                      _headerImageList.Handle);

                _headerImageListLoaded = true;
            }
        }

        private Bitmap GetBitmap(bool ascending)
        {
            if (_isWinXP)
            {
                // draw the flat triangle
                var bm = new Bitmap(9, 9);
                Graphics gfx = Graphics.FromImage(bm);

                Brush fillBrush = SystemBrushes.ControlDark;

#if !NON_TRANSPARENT_SORTMARKERS
				gfx.FillRectangle(Brushes.Magenta, 0, 0, 9, 9);
#else
                // XP ListView column header back color:
                Brush backBrush = new SolidBrush(Color.FromArgb(235, 234, 219));
                gfx.FillRectangle(backBrush /* SystemBrushes.ControlLight */, 0, 0, 9, 9);
                backBrush.Dispose();
#endif
                var path = new GraphicsPath();

                if (ascending)
                {
                    // Draw triangle pointing upwards
                    path.AddLine(4, 1, -1, 7);
                    path.AddLine(-1, 7, 9, 7);
                    path.AddLine(9, 7, 4, 1);
                    gfx.FillPath(fillBrush, path);
                }
                else
                {
                    // Draw triangle pointing downwards
                    path.AddLine(0, 2, 9, 2);
                    path.AddLine(9, 2, 4, 7);
                    path.AddLine(4, 7, 0, 2);
                    gfx.FillPath(fillBrush, path);
                }

                path.Dispose();
                gfx.Dispose();

                return bm;
            }
            else
            {
                var bm = new Bitmap(8, 8);
                Graphics gfx = Graphics.FromImage(bm);

                Pen lightPen = SystemPens.ControlLightLight;
                Pen shadowPen = SystemPens.ControlDark;

#if !NON_TRANSPARENT_SORTMARKERS
				gfx.FillRectangle(Brushes.Magenta, 0, 0, 8, 8);
#else
                gfx.FillRectangle(SystemBrushes.ControlLight, 0, 0, 8, 8);
#endif

                if (ascending)
                {
                    // Draw triangle pointing upwards
                    gfx.DrawLine(lightPen, 0, 7, 7, 7);
                    gfx.DrawLine(lightPen, 7, 7, 4, 0);
                    gfx.DrawLine(shadowPen, 3, 0, 0, 7);
                }
                else
                {
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

        private void OnBeforeSort(object sender, EventArgs e)
        {
            SetHeaderImageList();
            if (_showInGroups)
            {
                APIEnableGrouping(false);
            }
        }

        private void OnAfterSort(object sender, EventArgs e)
        {
            if (_showInGroups)
            {
                APIEnableGrouping(true);
                if (_autoGroup)
                {
                    AutoGroupByColumn(_autoGroupCol.Index);
                }
                else
                {
                    Regroup();
                }
            }
        }

		//private static SortOrder ConvertSortOrder(NewsComponents.SortOrder sortOrder)
		//{
		//    switch (sortOrder)
		//    {
		//        case NewsComponents.SortOrder.Ascending:
		//            return SortOrder.Ascending;
		//        case NewsComponents.SortOrder.Descending:
		//            return SortOrder.Descending;
		//        default:
		//            return SortOrder.None;
		//    }
		//}

		//private static NewsComponents.SortOrder ConvertSortOrder(SortOrder sortOrder)
		//{
		//    switch (sortOrder)
		//    {
		//        case SortOrder.Ascending:
		//            return NewsComponents.SortOrder.Ascending;
		//        case SortOrder.Descending:
		//            return NewsComponents.SortOrder.Descending;
		//        default:
		//            return NewsComponents.SortOrder.None;
		//    }
		//}
    }
}

#region CVS Version Log

/*
 * $Log: ThreadedListView.cs,v $
 * Revision 1.28  2007/03/13 16:50:49  t_rendelmann
 * fixed: new feed source dialog is now modal (key events are badly processed by parent window)
 *
 * Revision 1.27  2006/11/21 17:25:53  carnage4life
 * Made changes to support options for Podcasts
 *
 * Revision 1.26  2006/11/21 16:08:05  carnage4life
 * Made CheckForLayoutModifications thread safe.
 *
 * Revision 1.25  2006/11/05 10:54:40  t_rendelmann
 * fixed: surrounded the small diffs between CLR 2.0 and CLR 1.1 with conditional compile defs.
 *
 * Revision 1.24  2006/11/05 01:23:55  carnage4life
 * Reduced time consuming locks in indexing code
 *
 * Revision 1.23  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 */

#endregion