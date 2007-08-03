#region CVS Version Header
/*
 * $Id: ThreadedListViewItemCollection.cs,v 1.3 2005/01/25 14:08:37 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/01/25 14:08:37 $
 * $Revision: 1.3 $
 */
#endregion

using System;
using System.Collections;
using System.Windows.Forms; 

namespace System.Windows.Forms.ThListView
{
	/// <summary>
	/// Summary description for ThreadedListViewItemCollection.
	/// </summary>
	public class ThreadedListViewItemCollection: System.Windows.Forms.ListView.ListViewItemCollection {
		
		public delegate void ItemAddedEventHandler(object sender, ListViewItemEventArgs e);
		public delegate void ItemRemovedEventHandler(object sender, ListViewItemEventArgs e);
		public event ItemAddedEventHandler ItemAdded; 
		public event ItemRemovedEventHandler ItemRemoved; 

		private ThreadedListView owner;
		public ThreadedListViewItemCollection(ThreadedListView owner) : base(((ListView)owner)) { 
			this.owner = owner;
		} 

		protected ThreadedListView ListView { get { return this.owner; } }

		#region Add()
		public ThreadedListViewItem Add(ThreadedListViewItem item) { 
			ThreadedListViewItem itm = ((ThreadedListViewItem)base.Add(item)); 
			if (ListView.ShowInGroups) {
				Win32.API.AddItemToGroup(itm.ListView.Handle, itm.Index, itm.GroupIndex); 
				if (ItemAdded != null) { 
					ItemAdded(this, new ListViewItemEventArgs(itm)); 
				} 
			}
			return itm; 
		} 

		public new ThreadedListViewItem Add(string text) { 
			return this.Add(null, text); 
		} 
		public ThreadedListViewItem Add(object key, string text) { 
			ThreadedListViewItem itm = new ThreadedListViewItem(key, text); 
			return Add(itm); 
		} 

		public ThreadedListViewItem Add(string text, int imageIndex, int groupindex) { 
			ThreadedListViewItem itm = new ThreadedListViewItem(null, text, imageIndex, groupindex); 
			return Add(itm); 
		} 

		#endregion

		public void AddRange(ThreadedListViewItem[] values) { 
			base.AddRange(values); 
			if (ListView.ShowInGroups) {
				foreach (ThreadedListViewItem itm in values) { 
					Win32.API.AddItemToGroup(itm.ListView.Handle, itm.Index, itm.GroupIndex); 
					if (ItemAdded != null) { 
						ItemAdded(this, new ListViewItemEventArgs(itm)); 
					} 
				} 
			}
		} 

		public bool Contains(ThreadedListViewItem item) { 
			return base.Contains(item); 
		} 

		public int IndexOf(ThreadedListViewItem item) { 
			return base.IndexOf(item); 
		} 

		public ThreadedListViewItem Insert(int index, ThreadedListViewItem item) { 
			return ((ThreadedListViewItem)base.Insert(index, item)); 
		} 

		public new ThreadedListViewItem this[int displayIndex] { 
			get { 
				return ((ThreadedListViewItem)base[displayIndex]);  //((ThreadedListViewItem)this[displayIndex])
			} 
			set {
				base[displayIndex] = value; 
			} 
		} 

		public void Remove(ThreadedListViewItem item) { 
			if (ItemRemoved != null) { 
				ItemRemoved(this, new ListViewItemEventArgs(item)); 
			} 
			base.Remove(item); 
		} 

		public new void RemoveAt(int index) { 
			if (ItemRemoved != null) { 
				ItemRemoved(this, new ListViewItemEventArgs(this[index])); 
			} 
			base.RemoveAt(index); 
		} 

		public void CopyTo(ThreadedListViewItem[] array, int index) { 
			base.CopyTo(array, index); 
		} 
	}


	public class ListViewItemEventArgs : EventArgs { 
		private ThreadedListViewItem mItem = new ThreadedListViewItem(); 

		public ListViewItemEventArgs(ThreadedListViewItem item) { 
			mItem = item; 
		} 

		public ThreadedListViewItem Item { 
			get { 
				return mItem; 
			} 
			set { 
				mItem = value; 
			} 
		} 
	} 

}
