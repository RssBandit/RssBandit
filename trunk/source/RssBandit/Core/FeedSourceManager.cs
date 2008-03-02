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

		public ItemID(Guid sourceID, string itemID)
		{
			this.SourceID = sourceID;
			this._itemID = itemID;
		}

		public string FeedUrl
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
			_name = name;
			_ordinal = ordinal;
			Source = source;
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		private string _name;

		public int Ordinal
		{
			get { return _ordinal; }
			set { _ordinal = value; }
		}
		private int _ordinal;

		#region IComparable<FeedSourceID> Members

		int IComparable<FeedSourceID>.CompareTo(FeedSourceID other)
		{
			if (other == null) return 1;
			if (Object.ReferenceEquals(other, this) ||
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
		/// Gets the feed sources.
		/// </summary>
		/// <value>The feed sources.</value>
		private Dictionary<Guid, FeedSourceID> FeedSources {
			get { return _feedSources;  }
		}

		/// <summary>
		/// Gets the ordered feed sources. Used
		/// to build the initial tree root entries.
		/// </summary>
		/// <returns></returns>
		public List<FeedSourceID> GetOrderedFeedSources() {
			List<FeedSourceID> sources = new List<FeedSourceID>(FeedSources.Values);
			sources.Sort(Comparer<FeedSourceID>.Default);
			return sources;
		}

		/// <summary>
		/// Adds the specified new source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public FeedSourceID Add(FeedSource source, string name)
		{
			FeedSourceID fs = new FeedSourceID(source, name, FeedSources.Count);
			FeedSources.Add(fs.ID, fs);
			return fs;
		}

		/// <summary>
		/// Removes the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		public void Remove(FeedSourceID source)
		{
			if (FeedSources.ContainsKey(source.ID))
				FeedSources.Remove(source.ID);
		}

		/// <summary>
		/// Gets the source of a item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public FeedSourceID GetSourceOf(ItemID item) {
			return FeedSources[item.SourceID];
		}

	}
}
