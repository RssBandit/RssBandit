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
  
}
