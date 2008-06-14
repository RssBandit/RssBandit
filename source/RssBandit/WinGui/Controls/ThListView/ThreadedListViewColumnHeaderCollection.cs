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
using System.Windows.Forms;

namespace RssBandit.WinGui.Controls.ThListView
{
	public enum ThreadedListViewColumnHeaderChangedAction {
		Add,
		Remove
	}

	public delegate void ColumnHeaderCollectionChangedHandler(object sender, ThreadedListViewColumnHeaderChangedEventArgs e);

	public class ThreadedListViewColumnHeaderChangedEventArgs: EventArgs {
		public ThreadedListViewColumnHeader[] Columns;
		public ThreadedListViewColumnHeaderChangedAction Action;
		public ThreadedListViewColumnHeaderChangedEventArgs(ThreadedListViewColumnHeader[] c, ThreadedListViewColumnHeaderChangedAction a) {
			this.Columns = c;
			this.Action = a;
		}
	}

	/// <summary>
	/// Summary description for ThreadedListViewColumnHeaderCollection.
	/// </summary>
	public class ThreadedListViewColumnHeaderCollection: System.Windows.Forms.ListView.ColumnHeaderCollection
	{
		public event ColumnHeaderCollectionChangedHandler OnColumnHeaderCollectionChanged;
 
		public ThreadedListViewColumnHeaderCollection(ThreadedListView owner): base(((ListView)owner)) { 	} 

		#region Add
		public int Add(ThreadedListViewColumnHeader colHeader) {
			int idx = base.Add(colHeader);

			RaiseThreadedListViewColumnHeaderChangedEvent(
				new ThreadedListViewColumnHeader[]{colHeader}, 
				ThreadedListViewColumnHeaderChangedAction.Add);
			
			return idx;
		}

		public new ThreadedListViewColumnHeader Add(string caption, int width, HorizontalAlignment textAlign) {
			return Add(null, caption, typeof(string), width, textAlign);
		}

		public ThreadedListViewColumnHeader Add(string id, string caption, Type valueType, int width, HorizontalAlignment textAlign) {
			ThreadedListViewColumnHeader c = new ThreadedListViewColumnHeader(id, valueType);
			c.Text = caption;
			c.Width = width;
			c.TextAlign = textAlign;
			int index = base.Add(c);

			RaiseThreadedListViewColumnHeaderChangedEvent(
				new ThreadedListViewColumnHeader[]{c}, 
				ThreadedListViewColumnHeaderChangedAction.Add);

			return c;
		}
		#endregion

		public void AddRange(ThreadedListViewColumnHeader[] columns) {
			base.AddRange(columns);
			RaiseThreadedListViewColumnHeaderChangedEvent(columns, 
				ThreadedListViewColumnHeaderChangedAction.Add);
		}

		public void Remove(string columnID) {
			ThreadedListViewColumnHeader c = this[columnID];
			base.Remove(c);
			RaiseThreadedListViewColumnHeaderChangedEvent(
				new ThreadedListViewColumnHeader[]{c}, 
				ThreadedListViewColumnHeaderChangedAction.Remove);
		}

		public new ThreadedListViewColumnHeader this[int displayIndex] { 
			get { 
				return ((ThreadedListViewColumnHeader)base[displayIndex]); 
			} 
		} 

		public new ThreadedListViewColumnHeader this[string columnID] {

			get { 
				if (columnID == null)
					throw new ArgumentNullException("columnID");
				int idx = this.GetIndexByKey(columnID);
				if (idx < 0)
					throw new InvalidOperationException("No ThreadedListViewColumnHeader found with ID '" + columnID + "'");
				return this[idx];
			} 
		}
 
		/// <summary>
		/// Returns the index of the Column identified by column ID.
		/// </summary>
		/// <param name="columnID">string</param>
		/// <returns>int. -1, if not found</returns>
		public int GetIndexByKey(string columnID) {
			for (int i=0; i < base.Count; i++) {
				ThreadedListViewColumnHeader c = this[i]; 
				if (c.Key == columnID)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Returns a collection for faster column key to index lookup.
		/// Can be used to create new items/subitems according to the current column keys.
		/// </summary>
		public ColumnKeyIndexMap GetColumnIndexMap() {
			ColumnKeyIndexMap map = new ColumnKeyIndexMap(base.Count);
			lock (this) {
				for (int i=0; i < base.Count; i++) {
					ThreadedListViewColumnHeader c = this[i]; 
					map.Add(c.Key, i);
				}
			}
			return map;
		}


		private void RaiseThreadedListViewColumnHeaderChangedEvent(ThreadedListViewColumnHeader[] c, ThreadedListViewColumnHeaderChangedAction a) {
			if (OnColumnHeaderCollectionChanged != null)
				OnColumnHeaderCollectionChanged(this, new ThreadedListViewColumnHeaderChangedEventArgs(c,  a));
		}

	}

	#region helper classes
	/// <summary>
	/// Helper class for faster column key to index lookup.
	/// Can be used to create new items/subitems according to the current column keys.
	/// </summary>
	public class ColumnKeyIndexMap: Hashtable {
		public ColumnKeyIndexMap(): base() {}
		public ColumnKeyIndexMap(int capacity): base(capacity) {}
		public ColumnKeyIndexMap(IDictionary d): base(d) {}

		/// <summary>
		/// Set/Get the index of a column key
		/// </summary>
		public int this[string key] {
			get {
				return (int)base[key];
			}
			set {
				base[key] = value;
			}
		}

		public void Add(string key, int value) {
			base.Add (key, value);
		}
		public bool Contains(string key) {
			return base.Contains (key);
		}
		public bool ContainsKey(string key) {
			return base.ContainsKey (key);
		}
		public bool ContainsValue(int value) {
			return base.ContainsValue (value);
		}
	}
	#endregion

}

#region CVS Version Log
/*
 * $Log: ThreadedListViewColumnHeaderCollection.cs,v $
 * Revision 1.10  2006/11/21 06:59:16  t_rendelmann
 * fixed: surrounded the small diffs between CLR 2.0 and CLR 1.1 with conditional compile defs.
 *
 * Revision 1.9  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 */
#endregion