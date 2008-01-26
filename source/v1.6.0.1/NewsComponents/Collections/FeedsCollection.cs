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
using System.Collections.Generic;


namespace NewsComponents.Collections {


    #region EmptyList<T>
    /// <summary>
    /// Generic class to return an empty List instance
    /// </summary>
    public sealed class GetList<T> : List<T> {
        private readonly static List<T> _empty = new List<T>(0);
        /// <summary>
        /// Gets the empty/readonly single List instance.
        /// </summary>
        public static List<T> Empty { get { return _empty; } }
    }


    #endregion

    #region EmptyArrayList
    /// <summary>
	/// Helper class to return a empty ArrayList instance
	/// </summary>
	public sealed class GetArrayList:ArrayList
	{
		private static readonly ArrayList _empty = ReadOnly(new ArrayList(0));
		/// <summary>
		/// Gets the empty/readonly single ArrayList instance.
		/// </summary>
		public static ArrayList Empty { get { return _empty; } }	
	}
	#endregion

    public static class FeedsCollectionExtenstion
    {
        public static string KeyFromUri<V>(IDictionary<string, V> source, Uri uri)
        {
            if (ReferenceEquals(uri, null))
                throw new ArgumentNullException("uri");

            string feedUrl = uri.AbsoluteUri;
            if (!source.ContainsKey(feedUrl) && (uri.IsFile || uri.IsUnc))
            {
                feedUrl = uri.LocalPath;
            }

            return feedUrl;
        }
    }
}
