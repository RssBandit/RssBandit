#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

namespace RssBandit.Common.Utils {
    /// <summary>
    /// Helper class that deals with the stupidity of generics and collections. 
    /// </summary>
    public class TypeConverter {

        /// <summary>
        /// Returns a delegate that can be used to cast a subtype back to its base type. 
        /// </summary>
        /// <typeparam name="T">The derived type</typeparam>
        /// <typeparam name="U">The base type</typeparam>
        /// <returns>Delegate that can be used to cast a subtype back to its base type. </returns>
        public static Converter<U, T> UpCast<U, T>() where U : T {
            return delegate(U item) { return (T)item; };
        }

        /// <summary>
        /// Returns a delegate that can be used to cast a base to the specified derived type. 
        /// </summary>
        /// <typeparam name="T">The derived type</typeparam>
        /// <typeparam name="U">The base type</typeparam>
        /// <returns>Delegate that can be used to cast a base to the specified derived type.  </returns>
        public static Converter<U, T> DownCast<U, T>() where T : U {
            return delegate(U item) { return (T)item; };
        }	
    }
}
