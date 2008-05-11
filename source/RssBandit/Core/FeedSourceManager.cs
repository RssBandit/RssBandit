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
using NewsComponents;
using NewsComponents.Feed;

namespace RssBandit
{
	#region ItemID
	/// <summary>
	/// Used to map a feed to a source, but also used
	/// to map a category/tag to a source.
	/// The kind of an item will be detected by the FeedNodeType
	/// currently.
	/// </summary>
	internal class ItemID
	{
		public readonly Guid SourceID;

		/// <summary>
		/// Initializes a new instance of the <see cref="ItemID"/> class.
		/// </summary>
		/// <param name="sourceID">The source ID.</param>
		/// <param name="itemID">The item ID (the feed.id or category.id).</param>
		public ItemID(Guid sourceID, string itemID)
		{
			SourceID = sourceID;
			_itemID = itemID;
		}

		/// <summary>
		/// Gets or sets the item ID.
		/// </summary>
		/// <value>The item ID.</value>
		public string ID
		{
			get { return _itemID; }
			set { _itemID = value; }
		}

		private string _itemID;

		public override string ToString()
		{
			return String.Format("{0} ({1})", _itemID, SourceID);
		}
	} 
	#endregion

	#region FeedSourceID
	/// <summary>
	/// Class used to store the values for visual representation:
	/// name and position in the tree/sources list.
	/// </summary>
	internal class FeedSourceID : IComparable<FeedSourceID>
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

	internal class FeedSourceManager
	{
		readonly Dictionary<Guid, FeedSourceID> _feedSources = new Dictionary<Guid, FeedSourceID>();

		/// <summary>
		/// Gets the ordered feed sources. Used
		/// to build the initial tree root entries.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<FeedSource> Sources
		{
			get {
				foreach (FeedSourceID entry in _feedSources.Values)
				{
					yield return entry.Source;
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
		/// Removes the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		public void Remove(FeedSourceID source)
		{
			if (_feedSources.ContainsKey(source.ID))
				_feedSources.Remove(source.ID);
		}

		/// <summary>
		/// Gets the source of a item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public FeedSourceID GetSourceOf(ItemID item) {
			return _feedSources[item.SourceID];
		}

		/// <summary>
		/// Gets the source of a newsfeed.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public FeedSourceID GetSourceOf(INewsFeed item)
		{
			foreach (FeedSourceID id in _feedSources.Values)
				if (id.Source == item.owner)
					return id;
			return null;
		}
	}
}
