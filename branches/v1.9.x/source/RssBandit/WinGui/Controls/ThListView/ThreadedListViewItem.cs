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
using System.ComponentModel;
using System.Windows.Forms;

namespace RssBandit.WinGui.Controls.ThListView
{

	/// <summary>
	/// Summary description for ThreadedListViewItem.
	/// </summary>
	public class ThreadedListViewItem:ListViewItem
	{

		private int _indentLevel;
		private int _groupIndex; 
		private bool _hasChilds;
		private bool _isComment;
		internal bool _expanded;
		private object _key;
		private static int _originalIndex;
		private ThreadedListViewItem _parent;

		public ThreadedListViewItem() {
			RaiseIndex();
		}
		public ThreadedListViewItem(string text):this(null, text) { }
		
		public ThreadedListViewItem(object key, string[] items):base(items)	{
			RaiseIndex();
			_key = key;
		}

		public ThreadedListViewItem(object key, string text):base(text)	{
			RaiseIndex();
			_key = key;
		}

		public ThreadedListViewItem(object key, string[] items, int imageIndex) : base(items, imageIndex) { 
			RaiseIndex();
			_key = key;
		} 

		public ThreadedListViewItem(ListViewSubItem[] subItems, int imageIndex) : base(subItems, imageIndex) { 
		} 

		public ThreadedListViewItem(object key, string[] items, int imageIndex, Color foreColor, Color backColor, Font font) : base(items, imageIndex, foreColor, backColor, font) { 
			RaiseIndex();
			_key = key;
		} 

		public ThreadedListViewItem(object key, string text, int imageIndex, int groupIndex) : base(text, imageIndex) { 
			RaiseIndex();
			_key = key;
			this.GroupIndex = groupIndex; 
		} 

		public ThreadedListViewItem(object key, string[] items, int imageIndex, int groupIndex) : base(items, imageIndex){ 
			RaiseIndex();
			_key = key;
			this.GroupIndex = groupIndex; 
		} 

		public ThreadedListViewItem(ListViewSubItem[] subItems, int imageIndex, int groupIndex) : base(subItems, imageIndex) { 
			this.GroupIndex = groupIndex; 
		} 

		public ThreadedListViewItem(object key, string[] items, int imageIndex, Color foreColor, Color backColor, Font font, int groupIndex) : base(items, imageIndex, foreColor, backColor, font) { 
			RaiseIndex();
			_key = key;
			this.GroupIndex = groupIndex; 
		} 

		[Browsable(true), Category("Info")] 
		public int GroupIndex { 
			get { 
				return _groupIndex; 
			} 
			set { 
				_groupIndex = value; 
				Win32.API.AddItemToGroup(base.ListView.Handle, Index, _groupIndex); 
			} 
		} 

		[Browsable(false)] 
		internal string[] SubItemsArray { 
			get { 
				if (this.SubItems.Count == 0) { 
					return null; 
				} 

				string[] a = new string[this.SubItems.Count - 1];

				for (int i = 0; i <= this.SubItems.Count - 1; i++) { 
					a[i] = this.SubItems[i].Text; 
				} 
				return a; 
			} 
		} 

		public ThreadedListViewItem Parent {
			get { return _parent;  }
			set { _parent = value; }
		}

		public new ThreadedListView ListView {
			get { return (ThreadedListView)base.ListView; }
		}

		public object Key {
			get { return _key;  }
			set { _key = value; }
		}

		/// <summary>
		/// Works like the Path property of the TreeNode class, but returns
		/// an array of Key objects instead of a string with a path delimiter.
		/// </summary>
		/// <remarks>
		/// Example: an item with IndentLevel 1 will return an array with two object keys:
		/// <c>object[] {parent.Key, this.Key}</c>.
		/// </remarks>
		public object[] KeyPath {
			get {
				Stack s = new Stack(this.IndentLevel + 1);

				s.Push(this.Key);

				if (base.ListView != null && base.ListView.Items.Count > 0){
					int currentIndent = this.IndentLevel;
					for (int i = this.Index-1; i >= 0 && currentIndent > 0; i--) {
						ThreadedListViewItem lvi = (ThreadedListViewItem)base.ListView.Items[i];
						if (lvi.IndentLevel < currentIndent) {
							s.Push(lvi.Key);
							currentIndent = lvi.IndentLevel;
						}
					}
				}

				return s.ToArray();	// LIFO order
			}
		}

		/// <summary>
		/// Get/Sets the indent level. Used to display related items as conversation threads.
		/// </summary>
		public int IndentLevel
		{
			get { return _indentLevel;  }
			set 
			{ 
				_indentLevel = value; 
				if (_indentLevel > 0 && base.ListView != null)
					this.SetListViewItemIndent(_indentLevel);
			}
		}

		/// <summary>
		/// Set an image for a sub-item column.
		/// </summary>
		/// <param name="subItemIndex">int. The sub-item index</param>
		/// <param name="imageIndex">int. The image index.</param>
		public void SetSubItemImage(int subItemIndex, int imageIndex) {
			this.SetListViewSubItemImage(subItemIndex, imageIndex);
		}
		/// <summary>
		/// Reset/Clear an previously set sub-item image.
		/// </summary>
		/// <param name="subItemIndex">int. The sub-item index</param>
		public void ClearSubItemImage(int subItemIndex) {
			this.SetListViewSubItemImage(subItemIndex, -1);
		}

		/// <summary>
		/// Gets the original index (item creation order). 
		/// This usually does NOT start with an index of 0 (zero).
		/// Used to enable a sort by original creation order.
		/// </summary>
		internal int OriginalIndex {
			get { return _originalIndex;  }
		}
		internal static void ResetOriginalIndex() {
			System.Threading.Interlocked.Exchange(ref _originalIndex, 0);
		}
		// used to enable a sort by original creation order
		internal protected void RaiseIndex() {
			try {
				checked {
					System.Threading.Interlocked.Increment(ref _originalIndex);
				}
			} catch (OverflowException) {
				ResetOriginalIndex();
			}
		}

		/// <summary>
		/// Sets or gets the info, if this item has or will be have childs.
		/// This decides about if any state image will be displayed.
		/// </summary>
		public virtual bool HasChilds {
			get { return _hasChilds;  }
			set { 
				if (value)
					this.StateImageIndex = 2;
				else
					this.StateImageIndex = 0;	// makes +/- invisible
				_hasChilds = value; 
			}
		}

		/// <summary>
		/// Indicates that the threaded listview item is a threaded comment. 
		/// </summary>
		public virtual bool IsComment{
			get { return _isComment; }	
			set { _isComment = value; }
		}


		/// <summary>
		/// Sets or gets the item's Expanded state
		/// </summary>
		public virtual bool Expanded {
			get { return _expanded;  }
			set { 
				if (value) {
					if (this.ListView != null)
						this.ListView.ExpandListViewItem(this, false);
				} else {
					if (this.ListView != null)
						this.ListView.CollapseListViewItem(this);
				}
				this.SetThreadState(value);
			}
		}

		internal void SetThreadState(bool expanded) {
			if (expanded) {
				this.StateImageIndex = 3;
			} else {
				this.StateImageIndex = 2;
			}
			_expanded = expanded; 
		}

		/// <summary>
		/// Sets or gets the item's Collapsed state.
		/// </summary>
		public virtual bool Collapsed {
			get { return !this.Expanded;  }
			set { this.Expanded = !value; }
		}

		internal void ApplyIndentLevel()
		{
			IndentLevel = _indentLevel;
		}

		public bool StateImageHitTest(Point p)
		{
			Win32.LVHITTESTINFO htInfo;
            IntPtr ret;

			htInfo.pt.x = p.X;
			htInfo.pt.y = p.Y;
			htInfo.flags = 0;
			htInfo.iItem = 0;
			htInfo.iSubItem = 0;
			ret = Win32.API.SendMessage((IntPtr)base.ListView.Handle,Win32.W32_LVM.LVM_SUBITEMHITTEST /* 4153 */, 0, ref htInfo);
			//if (((ListViewHitTestFlags)htInfo.flags & ListViewHitTestFlags.LVHT_ONITEMSTATEICON) == ListViewHitTestFlags.LVHT_ONITEMSTATEICON)
			if ((Win32.ListViewHitTestFlags)htInfo.flags == Win32.ListViewHitTestFlags.LVHT_ONITEMSTATEICON)
				return true;
			return false;
		}

		private void SetListViewSubItemImage(int subItem, int imageIndex) {
			Win32.LVITEM lvi = new Win32.LVITEM();

			lvi.iItem = Index;
			lvi.iSubItem = subItem;
			lvi.iImage = imageIndex;
			lvi.mask = Win32.ListViewItemFlags.LVIF_IMAGE;
			Win32.API.SendMessage(base.ListView.Handle, Win32.W32_LVM.LVM_SETITEMA /* 4102 */, 0, ref lvi);
		}

		private void SetListViewItemIndent(int level) 
		{
			Win32.LVITEM lvi = new Win32.LVITEM();

			lvi.iItem = Index;
			lvi.iIndent = level;
			lvi.mask = Win32.ListViewItemFlags.LVIF_INDENT;
			Win32.API.SendMessage(base.ListView.Handle, Win32.W32_LVM.LVM_SETITEMA /* 4102 */, 0, ref lvi);
		}

		private int GetListViewItemIndent() 
		{
			Win32.LVITEM lvi = new Win32.LVITEM();
			int ret;

			lvi.iItem = Index;
			lvi.mask = Win32.ListViewItemFlags.LVIF_INDENT;
			Win32.API.SendMessage(base.ListView.Handle, Win32.W32_LVM.LVM_GETITEMA /* 4101 */, 0, ref lvi);
			ret = lvi.iIndent;
			return ret;

		}	

//		public void SetInfoText(string text) {
//			LVSETINFOTIP lit = new LVSETINFOTIP();
//			lit.cbSize = Marshal.SizeOf(lit);
//			lit.dwFlags = 0;
//			lit.iItem = base.Index;
//			lit.iSubItem = 1;
//			lit.pszText = text;
//			SendMessage((IntPtr)base.ListView.Handle,W32_LVM.LVM_SETINFOTIP , 0, lit);
//		}
	}

	/// <summary>
	/// To be used as a placeholder for async. insertion of items.
	/// <see cref="InsertionPointTicket">InsertionPointTicket</see> returns
	/// a unique indentifier for this item.
	/// </summary>
	public class ThreadedListViewItemPlaceHolder: ThreadedListViewItem {
		
		private string _insertionPointTicket;

		public ThreadedListViewItemPlaceHolder() {
			CreateInsertionPointTicket();
		}
		public ThreadedListViewItemPlaceHolder(string text):base(null, text) { 
			CreateInsertionPointTicket();
		}
		
		public ThreadedListViewItemPlaceHolder(string[] items):base(null, items)	{
			CreateInsertionPointTicket();
		}

		/// <summary>
		/// Gets the unique insertion point ticket.
		/// </summary>
		public string InsertionPointTicket { get { return this._insertionPointTicket; } }

		private void CreateInsertionPointTicket() {
			this._insertionPointTicket = Guid.NewGuid().ToString();
		}
	}
}
