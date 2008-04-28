using System;
using System.Collections.Generic;
using System.Linq;

namespace NewsComponents.Utils
{
    public static class ListExtension
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
                list.Add(item);
        }
    }
}
