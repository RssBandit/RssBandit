#region CVS Version Header
/*
 * $Id: ListHelper.cs,v 1.2 2005/01/15 17:11:39 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/01/15 17:11:39 $
 * $Revision: 1.2 $
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

		
		private ListHelper() {}
	}
}
