#region CVS Version Header
/*
 * $Id: ThreadedListViewItemComparer.cs,v 1.2 2005/01/15 17:11:39 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/01/15 17:11:39 $
 * $Revision: 1.2 $
 */
#endregion

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.ThListView.Sorting
{
	/// <summary>
	/// Provides simple text sorting (case sensitive)
	/// </summary>
	public class ThreadedListViewItemComparer: IComparer {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sortColumn">Column to be sorted</param>
		/// <param name="ascending">true, if ascending order, false otherwise</param>
		public ThreadedListViewItemComparer(int sortColumn, bool ascending) {
			_column = sortColumn;
			_ascending = ascending;
		}

		/// <summary>
		/// Implementation of IComparer.Compare
		/// </summary>
		/// <param name="lhs">First Object to compare</param>
		/// <param name="rhs">Second Object to compare</param>
		/// <returns>Less that zero if lhs is less than rhs. Greater than zero if lhs greater that rhs. Zero if they are equal</returns>
		public int Compare(object lhs, object rhs) {
			ThreadedListViewItem lhsLvi = lhs as ThreadedListViewItem;
			ThreadedListViewItem rhsLvi = rhs as ThreadedListViewItem;

			if(lhsLvi == null || rhsLvi == null)    // We only know how to sort ListViewItems, so return equal
				return 0;

			if (Object.ReferenceEquals(lhsLvi, rhsLvi))
				return 0;

			int result = 0;

			if (lhsLvi.IndentLevel != rhsLvi.IndentLevel)  {

				if (lhsLvi.IndentLevel < rhsLvi.IndentLevel) {
					
					if (rhsLvi.Parent != null && rhsLvi.Parent.Equals(lhsLvi)) 	// my thread
						return -1;
					return this.Compare(lhsLvi, rhsLvi.Parent);

				} else {

					if (lhsLvi.Parent != null && lhsLvi.Parent.Equals(rhsLvi)) 	// my thread
						return 1;
					return this.Compare(lhsLvi.Parent, rhsLvi);
				}

			} else {	// equal indentLevel

				if (lhsLvi.Parent != null && !lhsLvi.Parent.Equals(rhsLvi.Parent))
					return this.Compare(lhsLvi.Parent, rhsLvi.Parent);	
			}

			ListViewItem.ListViewSubItemCollection lhsItems = lhsLvi.SubItems;
			ListViewItem.ListViewSubItemCollection rhsItems = rhsLvi.SubItems;

			string lhsText = (lhsItems.Count > _column) ? lhsItems[_column].Text : String.Empty;
			string rhsText = (rhsItems.Count > _column) ? rhsItems[_column].Text : String.Empty;

			if(lhsText.Length == 0 || rhsText.Length == 0)
				result = lhsText.CompareTo(rhsText);

			else
				result = OnCompare(lhsText, rhsText);

			if(!_ascending)
				result = -result;

			return result;
		}

		/// <summary>
		/// Overridden to do type-specific comparision.
		/// </summary>
		/// <param name="lhs">First Object to compare</param>
		/// <param name="rhs">Second Object to compare</param>
		/// <returns>Less that zero if lhs is less than rhs. Greater than zero if lhs greater that rhs. Zero if they are equal</returns>
		protected virtual int OnCompare(string lhs, string rhs) {
			return String.Compare(lhs, rhs, false);
		}

		private int _column;
		private bool _ascending;
	}

	#region TextItemComparer
	/// <summary>
	/// Provides text sorting (case sensitive)
	/// </summary>
	public class ThreadedListViewTextItemComparer: ThreadedListViewItemComparer {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sortColumn">Column to be sorted</param>
		/// <param name="ascending">true, if ascending order, false otherwise</param>
		public ThreadedListViewTextItemComparer(Int32 sortColumn, Boolean ascending):
			base(sortColumn, ascending) {
		}
	}

	/// <summary>
	/// Provides text sorting (case in-sensitive)
	/// </summary>
	public class ThreadedListViewCaseInsensitiveTextItemComparer: ThreadedListViewItemComparer {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sortColumn">Column to be sorted</param>
		/// <param name="ascending">true, if ascending order, false otherwise</param>
		public ThreadedListViewCaseInsensitiveTextItemComparer(Int32 sortColumn, Boolean ascending):
			base(sortColumn, ascending) {
		}

		/// <summary>
		/// Case-insensitive compare
		/// </summary>
		protected override Int32 OnCompare(String lhs, String rhs) {
			return String.Compare(lhs, rhs, true);
		}
	}
	#endregion

	#region DateTimeItemComparer
	/// <summary>
	/// Provides date sorting
	/// </summary>
	public class ThreadedListViewDateTimeItemComparer: ThreadedListViewItemComparer {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sortColumn">Column to be sorted</param>
		/// <param name="ascending">true, if ascending order, false otherwise</param>
		public ThreadedListViewDateTimeItemComparer(Int32 sortColumn, Boolean ascending):
			base(sortColumn, ascending) {
		}

		/// <summary>
		/// Date compare
		/// </summary>
		protected override Int32 OnCompare(String lhs, String rhs) {
			return DateTime.Parse(lhs).CompareTo(DateTime.Parse(rhs));
		}
	}
	#endregion

	#region IntXXItemComparer
	/// <summary>
	/// Provides integer (32 bits) sorting
	/// </summary>
	public class ThreadedListViewInt32ItemComparer: ThreadedListViewItemComparer {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sortColumn">Column to be sorted</param>
		/// <param name="ascending">true, if ascending order, false otherwise</param>
		public ThreadedListViewInt32ItemComparer(Int32 sortColumn, Boolean ascending):
			base(sortColumn, ascending) {
		}

		/// <summary>
		/// Integer compare
		/// </summary>
		protected override Int32 OnCompare(String lhs, String rhs) {
			return Int32.Parse(lhs, NumberStyles.Number) - Int32.Parse(rhs, NumberStyles.Number);
		}
	}

	/// <summary>
	/// Provides integer (64 bits) sorting
	/// </summary>
	public class ThreadedListViewInt64ItemComparer: ThreadedListViewItemComparer  {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sortColumn">Column to be sorted</param>
		/// <param name="ascending">true, if ascending order, false otherwise</param>
		public ThreadedListViewInt64ItemComparer(Int32 sortColumn, Boolean ascending):
			base(sortColumn, ascending) {
		}

		/// <summary>
		/// Integer compare
		/// </summary>
		protected override Int32 OnCompare(String lhs, String rhs) {
			return (Int32)(Int64.Parse(lhs, NumberStyles.Number) - Int64.Parse(rhs, NumberStyles.Number));
		}
	}
	#endregion

	#region DoubleItemComparer
	/// <summary>
	/// Provides floating-point sorting
	/// </summary>
	public class ThreadedListViewDoubleItemComparer: ThreadedListViewItemComparer {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sortColumn">Column to be sorted</param>
		/// <param name="ascending">true, if ascending order, false otherwise</param>
		public ThreadedListViewDoubleItemComparer(Int32 sortColumn, Boolean ascending):
			base(sortColumn, ascending) {
		}

		/// <summary>
		/// Floating-point compare
		/// </summary>
		protected override Int32 OnCompare(String lhs, String rhs) {
			Double result = Double.Parse(lhs) - Double.Parse(rhs);

			if(result > 0)
				return 1;

			else if(result < 0)
				return -1;

			else
				return 0;
		}
	}
	#endregion

}
