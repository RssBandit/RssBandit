using System;
using System.Collections.Generic;
using System.Text;

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
        public static Converter<T, U> UpCast<T, U>() where T : U {
            return delegate(T item) { return (U)item; };
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
