#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Windows.Forms;

namespace RssBandit.WinGui.Controls.ThListView
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
#if CLR_11		
		public object Tag;
#endif
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

#region CVS Version Log
/*
 * $Log: ThreadedListViewColumnHeader.cs,v $
 * Revision 1.5  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 */
#endregion