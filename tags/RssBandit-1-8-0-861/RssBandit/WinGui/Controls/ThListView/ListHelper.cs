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

namespace RssBandit.WinGui.Controls.ThListView
{
	/// <summary>
	/// ListHelper contains static helper functions.
	/// </summary>
	internal sealed class ListHelper
	{

		public static ArrayList CopyAndInsert(ArrayList list, int index, object obj) {
			ArrayList newList = new ArrayList((list.Count + 1));
			if (index == 0) {
				newList.Add(obj);
				newList.AddRange(list);
			} else {
				if (index == list.Count) {
					newList.AddRange(list);
					newList.Add(obj);
					return newList;
				}
				newList.AddRange(list.GetRange(0, index));
				newList.Add(obj);
				newList.AddRange(list.GetRange(index, (list.Count - index)));
			}
			return newList;
		}

		public static string[] StripEmptyEntries(string[] list) {
			ArrayList a = new ArrayList();
			foreach(string entry in list) {
				if (entry != null && entry.Length > 0) {
					a.Add(entry);
				}
			}
			if (a.Count > 0)
				return (string[])a.ToArray(typeof(string));
			return new string[]{};
		}

		private ListHelper() {}
	}
}
