using System;
using System.Collections.Generic;
using System.Linq;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Extensions for IListlt;Tgt;
	/// </summary>
    public static class ListExtension
    {
		/// <summary>
		/// Adds the range of items.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <param name="items">The items.</param>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
                list.Add(item);
        }
    }
}
