using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace NewsComponents.Utils
{

    /// <summary>
    /// Encapsulates data about a change to a collection
    /// </summary>
    class CollectionChangedEventArgs : PropertyChangedEventArgs {

		/// <summary>
		/// Initializes the event arguments
		/// </summary>
		/// <param name="collectionName">The name of the property that is a collection</param>
		/// <param name="action">The action.</param>
		/// <param name="value">The value that was added or removed from the collection</param>
        public CollectionChangedEventArgs(string collectionName, CollectionChangeAction action, object value)
            : base(collectionName) 
        {
            this.Value = value;
            this.Action = action;
        }

        /// <summary>
        /// The value that was either added or removed from the collection. 
        /// </summary>
        public object Value;

        /// <summary>
        /// The modification that was made to the collection
        /// </summary>
        public CollectionChangeAction Action;
    }

    /// <summary>
    /// Helper class with methods needed for WPF data binding. 
    /// </summary>
    class DataBindingHelper
    {
        private static Object syncRoot = new Object(); 

        /// <summary>
        /// Cache of PropertyChangedEventArgs instances. 
        /// </summary>
        private static IDictionary<string, PropertyChangedEventArgs> propertyChangedArgsCache = new Dictionary<string, PropertyChangedEventArgs>();


        /// <summary>
        /// Returns an instance of PropertyChangedEventArgs for 
        /// the specified property name.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to create event args for.
        /// </param>		
        public static PropertyChangedEventArgs
            GetPropertyChangedEventArgs(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException(
                    "propertyName cannot be null or empty.");

            PropertyChangedEventArgs args;

            // Get the event args from the cache, creating them
            // and adding to the cache if necessary.
            lock (syncRoot)
            {
                if (!propertyChangedArgsCache.TryGetValue(propertyName, out args))
                {
                    args = new PropertyChangedEventArgs(propertyName); 
                    propertyChangedArgsCache.Add(propertyName, args);
                }
            }

            return args;
        }
    }
}
