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
using System.Collections.Generic;
using System.Linq;
using NewsComponents;
using NewsComponents.Feed;

namespace NewsComponents
{

	#region FeedSourceID
	/// <summary>
	/// Class used to store the values for visual representation:
	/// name and position in the tree/sources list.
	/// </summary>
	public class FeedSourceID : IComparable<FeedSourceID>
	{
		public readonly Guid ID;
		public readonly FeedSource Source;

		public FeedSourceID(FeedSource source, string name, int ordinal)
		{
			ID = new Guid();
			Name = name;
			Ordinal = ordinal;
			Source = source;
		}

		/// <summary>
		/// Gets or sets the name of the source.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the ordinal position.
		/// </summary>
		/// <value>The ordinal.</value>
		public int Ordinal { get; set; }

		#region IComparable<FeedSourceID> Members

		int IComparable<FeedSourceID>.CompareTo(FeedSourceID other)
		{
			if (other == null) return 1;
			if (ReferenceEquals(other, this) ||
				Ordinal == other.Ordinal) return 0;
			if (Ordinal > other.Ordinal) return 1;
			return -1;
		}

		#endregion
	} 
	#endregion

	/// <summary>
	/// Manages a list of feed sources
	/// </summary>
	public class FeedSourceManager
	{
		readonly Dictionary<Guid, FeedSourceID> _feedSources = new Dictionary<Guid, FeedSourceID>();

		/// <summary>
		/// Gets the ordered feed sources. Used
		/// to build the initial tree root entries.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<FeedSourceID> Sources
		{
			get {
				foreach (FeedSourceID entry in _feedSources.Values)
				{
					yield return entry;
				}
			}
		}

		/// <summary>
		/// Gets the ordered feed sources. Used
		/// to build the initial tree root entries.
		/// </summary>
		/// <returns></returns>
		public List<FeedSourceID> GetOrderedFeedSources() {
			List<FeedSourceID> sources = new List<FeedSourceID>(_feedSources.Values);
			sources.Sort(Comparer<FeedSourceID>.Default);
			return sources;
		}

		/// <summary>
		/// Can be used to call methods or set properties on each
		/// FeedSource.
		/// </summary>
		public void ForEach(Action<FeedSource> action)
		{
			foreach (FeedSourceID entry in _feedSources.Values) {
				action(entry.Source);
			}
		}

		/// <summary>
		/// Adds the specified new source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public FeedSourceID Add(FeedSource source, string name)
		{
			FeedSourceID fs = new FeedSourceID(source, name, _feedSources.Count);
			_feedSources.Add(fs.ID, fs);
			return fs;
		}


        /// <summary>
        /// Determines whether there is a Feed Source with the specified name
        /// </summary>
        /// <param name="name">The name of the feed source</param>
        /// <returns>True if a feed source has been provided with the specified name</returns>
        public bool Contains(string name)
        {
            return _feedSources.Values.Any(fs => fs.Name == name); 
        }

        /// <summary>
        /// Indexer which returns feed source keyed by name
        /// </summary>
        /// <param name="name">The name of the feed source</param>
        /// <returns>The requested feed source</returns>
        /// <exception cref="KeyNotFoundException">if the name is not found in the FeedSourceManager</exception>
        public FeedSourceID this[string name]
        {
            get {
                FeedSourceID fsid = _feedSources.Values.First(fs => fs.Name == name);
                if (fsid != null)
                {
                    return fsid;
                }
            	throw new KeyNotFoundException(name);
            }          
        }

        /// <summary>
        /// Returns feed source keyed by name
        /// </summary>
        /// <param name="name">The name of the feed source</param>
        /// <param name="value">out parameter for storing feed source</param>
        /// <returns>The requested feed source or null if not found</returns>
        public void TryGetValue(string name, out FeedSourceID value)
        {            
            value = null;
            if (!string.IsNullOrEmpty(name))
            {
                FeedSourceID fsid = _feedSources.Values.First(fs => fs.Name == name);
                if (fsid != null)
                {
                    value = fsid;
                }
            }
        }

		/// <summary>
		/// Removes the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		public void Remove(FeedSourceID source)
		{
			if (_feedSources.ContainsKey(source.ID))
				_feedSources.Remove(source.ID);
		}

		/// <summary>
		/// Gets the source of a newsfeed.
		/// </summary>
		/// <param name="newsFeed">The news feed.</param>
		/// <returns></returns>
		public FeedSourceID SourceOf(INewsFeed newsFeed)
		{
			if (newsFeed == null)
				return null;
			foreach (FeedSourceID id in _feedSources.Values)
				if (id.Source == newsFeed.owner)
					return id;
			return null;
		}

		/// <summary>
		/// Gets the source type of an item.
		/// </summary>
		/// <param name="newsFeed">The news feed.</param>
		/// <returns></returns>
		public FeedSourceType SourceTypeOf(INewsFeed newsFeed)
		{
			FeedSourceID sid = SourceOf(newsFeed);
			return sid != null ? sid.Source.Type : FeedSourceType.Unknown;
		}

		/// <summary>
		/// Gets the source extension.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="newsFeed">The news feed.</param>
		/// <returns>Null, if newsFeed is null, else the requested extension instance</returns>
		/// <exception cref="InvalidCastException">If extension is not implemented by the newsFeed source</exception>
		public T GetSourceExtension<T>(INewsFeed newsFeed)
		{
			FeedSourceID sid = SourceOf(newsFeed);
			if (sid != null)
			{
				object t = sid.Source;
				return (T)t;
			}
			return default(T);
		}
	}
}
