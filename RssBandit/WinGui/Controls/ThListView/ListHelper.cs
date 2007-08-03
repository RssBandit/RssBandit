#region CVS Version Header
/*
 * $Id: ListHelper.cs,v 1.3 2005/09/30 13:12:11 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/09/30 13:12:11 $
 * $Revision: 1.3 $
 */
#endregion

using System;
using System.Collections;

namespace System.Windows.Forms.ThListView
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
