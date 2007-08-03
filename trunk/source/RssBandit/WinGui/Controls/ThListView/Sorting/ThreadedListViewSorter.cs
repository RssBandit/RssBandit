#region CVS Version Header
/*
 * $Id: ThreadedListViewSorter.cs,v 1.5 2005/03/19 21:43:15 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/19 21:43:15 $
 * $Revision: 1.5 $
 */
#endregion

using System;
using System.Collections;
using System.Windows.Forms;

namespace System.Windows.Forms.ThListView.Sorting
{

	/// <summary>
	/// Summary description for ThreadedListViewSorter.
	/// </summary>
	public class ThreadedListViewSorter
	{

		public event EventHandler BeforeSort;
		public event EventHandler AfterSort;

		private ThreadedListView _list;
		private int _columnIndex;
		private SortOrder _sortOrder;
		private Hashtable _typeSorter;

		internal ThreadedListViewSorter(ThreadedListView list)
		{
			this._typeSorter = new Hashtable(5);
			this._typeSorter.Add(typeof(string), typeof(ThreadedListViewTextItemComparer));
			this._typeSorter.Add(typeof(int), typeof(ThreadedListViewInt32ItemComparer));
			this._typeSorter.Add(typeof(long), typeof(ThreadedListViewInt64ItemComparer));
			this._typeSorter.Add(typeof(double), typeof(ThreadedListViewDoubleItemComparer));
			this._typeSorter.Add(typeof(DateTime), typeof(ThreadedListViewDateTimeItemComparer));

			this._columnIndex = -1;
			this._sortOrder = SortOrder.None;
			this._list = list;
			this._list.ColumnClick += new ColumnClickEventHandler(this.OnListColumnClick);
			this._list.Columns.OnColumnHeaderCollectionChanged += new ColumnHeaderCollectionChangedHandler(this.OnColumnHeaderCollectionChanged);
		}

		public SortOrder SortOrder {
			get { return this._sortOrder; }
			set { this._sortOrder = value; }
		}

		public int SortColumnIndex {
			get { return this._columnIndex; }
			set { this._columnIndex = value; }
		}

		/// <summary>
		/// Sorts the rows based on the given column and the current sort order
		/// </summary>
		/// <param name="column">Column to be sorted</param>
		public void Sort(int column) {
			SortOrder order = SortOrder.Ascending;
			
			if(column == _columnIndex)
				order = (_sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;

			this.Sort(column, order);
		}

		/// <summary>
		/// Sorts the rows based on the given column and sort order
		/// </summary>
		/// <param name="column">Column to be sorted</param>
		/// <param name="order">Sort order</param>
		public void Sort(int column, SortOrder order) {
			if(column < 0 || column >= _list.Columns.Count)
				throw new IndexOutOfRangeException();

			_columnIndex = column;
			_sortOrder = order;

			if (BeforeSort != null)
				BeforeSort(this, EventArgs.Empty);

			if (_list.Items.Count > 1) {
//				lock(_list.Items) {
//					ThreadedListViewItem[] a = new ThreadedListViewItem[_list.Items.Count];
//					_list.Items.CopyTo(a, 0);
//					_list.BeginUpdate();
//					_list.Items.Clear();
//					Array.Sort(a, this.GetComparer());
//					_list.Items.AddRange(a); 
//					_list.EndUpdate();
//				}

				// may destroys Header control !!!
				IComparer comp = this.GetComparer();
				_list.ListViewItemSorter = comp;
				if (comp != null) {
					_list.Sort();
					_list.ListViewItemSorter = null;
				}
			}

			_list.RefreshSortMarks(_columnIndex, _sortOrder);

			if (AfterSort != null)
				AfterSort(this, EventArgs.Empty);

		}

		public void RefreshSortMarks() {
			_list.RefreshSortMarks(_columnIndex, _sortOrder);
		}

		/// <summary>
		/// Get the comparer matching the current SortOrder and Column of the attached listview.
		/// If SortOrder is SortOrder.None, null is returned.
		/// </summary>
		/// <returns>IComparer</returns>
		public IComparer GetComparer() {
			if(_sortOrder != SortOrder.None) {
				ThreadedListViewColumnHeader ch = _list.Columns[_columnIndex] as ThreadedListViewColumnHeader;
				if (ch != null && _typeSorter.ContainsKey(ch.ColumnValueType)) {
					return(ThreadedListViewItemComparer) Activator.CreateInstance((Type)_typeSorter[ch.ColumnValueType], new Object[] { _columnIndex, _sortOrder == SortOrder.Ascending } );
				} else {
					return new ThreadedListViewItemComparer( _columnIndex, _sortOrder == SortOrder.Ascending );
				}
			}
			return null;
		}

		private void OnListColumnClick(object sender, ColumnClickEventArgs e) {
			this.Sort(e.Column);
		}

		private void OnColumnHeaderCollectionChanged(object sender, ThreadedListViewColumnHeaderChangedEventArgs e) {
			if (e.Action == ThreadedListViewColumnHeaderChangedAction.Remove) {
				ThreadedListViewColumnHeaderCollection c = sender as ThreadedListViewColumnHeaderCollection;
				if (c != null && c.Count <= this._columnIndex) {
					if (c.Count > 0 && e.Columns.Length == 1) {
						this._columnIndex = c.Count - 1;
					} else {
						this._columnIndex = 0;
						this._sortOrder = SortOrder.None;
					}
				}
			}
		}
	}
}
