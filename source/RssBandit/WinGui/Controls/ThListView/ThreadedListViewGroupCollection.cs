#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Runtime.InteropServices; 

namespace RssBandit.WinGui.Controls.ThListView
{
	/// <summary>
	/// Summary description for ThreadedListViewGroupCollection.
	/// </summary>
	public class ThreadedListViewGroupCollection	: System.Collections.CollectionBase 
	{ 
		public delegate void GroupAddedEventHandler(object sender, ListViewGroupEventArgs e);
		public delegate void GroupRemovedEventHandler(object sender, ListViewGroupEventArgs e);
		public event GroupAddedEventHandler GroupAdded; 
		public event GroupRemovedEventHandler GroupRemoved; 
		private ThreadedListView _owner; 

		public ThreadedListViewGroup this[int index] { 
			get { 
				return ((ThreadedListViewGroup)List[index]); 
			} 
			set { 
				List[index] = value; 
			} 
		} 

		public ThreadedListViewGroupCollection(ThreadedListView owner) { 
			_owner = owner; 
		} 

		public int Add(ThreadedListViewGroup value) { 
			NativeMethods.API.AddListViewGroup(_owner.Handle, value.GroupText, value.GroupIndex); 
			if (GroupAdded != null) { 
				GroupAdded(this, new ListViewGroupEventArgs(value)); 
			} 
			return List.Add(value); 
		} 

		public int Add(string text, int index) { 
			ThreadedListViewGroup itm = new ThreadedListViewGroup(text, index); 
			NativeMethods.API.AddListViewGroup(_owner.Handle, text, index); 
			if (GroupAdded != null) { 
				GroupAdded(this, new ListViewGroupEventArgs(itm)); 
			} 
			return List.Add(itm); 
		} 

		public int IndexOf(ThreadedListViewGroup value) { 
			return List.IndexOf(value); 
		} 

		public void Insert(int index, ThreadedListViewGroup value) { 
			List.Insert(index, value); 
		} 

		public void Remove(ThreadedListViewGroup value) { 
			NativeMethods.API.RemoveListViewGroup(_owner.Handle, value.GroupIndex); 
			if (GroupRemoved != null) { 
				GroupRemoved(this, new ListViewGroupEventArgs(value)); 
			} 
			List.Remove(value); 
		} 

		public bool Contains(ThreadedListViewGroup value) { 
			return List.Contains(value); 
		} 

		public new void Clear() { 
			NativeMethods.API.ClearListViewGroup(_owner.Handle); 
			List.Clear(); 
		} 

		public void CopyTo(ThreadedListViewGroup[] array, int index) { 
			List.CopyTo(array, index); 
		} 
	}

	public class ListViewGroupEventArgs : EventArgs { 

		public ListViewGroupEventArgs(ThreadedListViewGroup item) { 
			mItem = item; 
		} 

		public ThreadedListViewGroup Item { 
			get { 
				return mItem; 
			} 
			set { 
				mItem = value; 
			} 
		} 
		private ThreadedListViewGroup mItem; 
	} 

}
