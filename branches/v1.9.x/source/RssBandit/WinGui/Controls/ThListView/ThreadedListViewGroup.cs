#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.ComponentModel; 

namespace RssBandit.WinGui.Controls.ThListView
{
	/// <summary>
	/// Summary description for ThreadedListViewGroup.
	/// </summary>
	[TypeConverter(typeof(ThreadedListViewGroupConverter))] 
	public class ThreadedListViewGroup { 
		private string _text; 
		private int _index; 

		public ThreadedListViewGroup() { 
		} 

		public ThreadedListViewGroup(string text, int index) { 
			_text = text; 
			_index = index; 
		} 

		public ThreadedListViewGroup(string text) { 
			_text = text; 
		} 

		public string GroupText { 
			get { 
				return _text; 
			} 
			set { 
				_text = value; 
			} 
		} 

		public int GroupIndex { 
			get { 
				return _index; 
			} 
			set { 
				_index = value; 
			} 
		} 

		public override string ToString() { 
			return _text; 
		} 
	}
}
