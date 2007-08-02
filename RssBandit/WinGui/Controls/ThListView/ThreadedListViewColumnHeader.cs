#region CVS Version Header
/*
 * $Id: ThreadedListViewColumnHeader.cs,v 1.4 2005/01/15 17:11:39 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/01/15 17:11:39 $
 * $Revision: 1.4 $
 */
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms.ThListView
{
	/// <summary>
	/// ThreadedListViewColumnHeader.
	/// </summary>
	public class ThreadedListViewColumnHeader:ColumnHeader
	{
		private string _id;
		private Type _colValueType;
		private static int keyNumber = 0;

		public ThreadedListViewColumnHeader():base() {
			this._id = "col" + keyNumber.ToString();
			RaiseKeyNumber();
			this._colValueType = typeof(String);
		}
		public ThreadedListViewColumnHeader(string columnID, Type valueType):base()
		{
			if (null != columnID && columnID.Length > 0)
				this._id = columnID;
			if (null != valueType )
				this._colValueType = valueType;
		}

		public string Key { get { return _id; } set { _id = value; } }
		public Type ColumnValueType { get { return _colValueType; } set { _colValueType = value; } }
		public object Tag;

		public virtual new object Clone() {
			ThreadedListViewColumnHeader nh = new ThreadedListViewColumnHeader(this._id, this._colValueType);
			nh.Text = this.Text;
			nh.Tag = this.Tag;
			nh.TextAlign = this.TextAlign;
			nh.Width = this.Width;
			return nh;
		}

		internal static void ResetKeyNumber() {
			System.Threading.Interlocked.Exchange(ref keyNumber, 0);
		}
		// used to ensure a unique column id
		internal protected void RaiseKeyNumber() {
			try {
				checked {
					System.Threading.Interlocked.Increment(ref keyNumber);
				}
			} catch (OverflowException) {
				ResetKeyNumber();
			}
		}

	}
}
