#region CVS Version Header
/*
 * $Id: CollapsiblePanelCollection.cs,v 1.1 2004/01/20 17:15:43 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2004/01/20 17:15:43 $
 * $Revision: 1.1 $
 */
#endregion

using System;
using System.Runtime.InteropServices;

namespace RssBandit.WinGui.Controls.CollapsiblePanels {

	/// <summary>
	/// TypeSafe Collection holding CollapsiblePanels
	/// </summary>
	public class CollapsiblePanelCollection : System.Collections.CollectionBase
	{

		/// <summary>
		/// Add a CollapsiblePanel
		/// </summary>
		public void Add(CollapsiblePanel panel)
		{
			this.List.Add(panel);
		}
		/// <summary>
		/// Remove the CollapsiblePanel at the index
		/// </summary>
		public void Remove(int index)
		{
			// Ensure the supplied index is valid
			if((index >= this.Count) || (index < 0))
			{
				throw new IndexOutOfRangeException("The supplied index is out of range");
			}
			this.List.RemoveAt(index);
		}

		/// <summary>
		/// Indexer
		/// </summary>
		public CollapsiblePanel Item(int index)
		{
			// Ensure the supplied index is valid
			if((index >= this.Count) || (index < 0))
			{
				throw new IndexOutOfRangeException("The supplied index is out of range");
			}
			return (CollapsiblePanel)this.List[index];
		}

		/// <summary>
		/// .
		/// </summary>
		public void Insert(int index, CollapsiblePanel panel)
		{
			this.List.Insert(index, panel);
		}

		/// <summary>
		/// .
		/// </summary>
		public void CopyTo(System.Array array, System.Int32 index)
		{
			this.List.CopyTo(array, index);
		}

		/// <summary>
		/// .
		/// </summary>
		public int IndexOf(CollapsiblePanel panel)
		{
			return this.List.IndexOf(panel);
		}
	}
}
