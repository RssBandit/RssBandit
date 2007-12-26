#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

using NewsComponents.Feed;

namespace NewsComponents.Storage {
	/// <summary>
	/// Abstract base class for classes that manage the cache.  
	/// A Cache can be managed by any backing store.  A file based 
	/// cache manager is included as <see cref="FileCacheManager"/>
	/// </summary>
	public abstract class CacheManager
	{

		/// <summary>
		/// Returns the location of the cache. The format of the location is dependent 
		/// on the CacheManager implementation. 
		/// </summary>
		internal abstract string CacheLocation {get;}

		/// <summary>
		/// Returns an RSS feed as an XmlReader. 
		/// </summary>
		/// <param name="feed">The feed whose FeedInfo is required.</param>
		/// <returns>The requested feed</returns>
		internal abstract FeedDetailsInternal GetFeed(feedsFeed feed); 

		/// <summary>
		/// Saves a particular RSS feed.
		/// </summary>
		/// <remarks>This method should be thread-safe</remarks>
		/// <param name="feed">The feed to save. This is an identifier
		/// and not used to actually fetch the feed from the WWW.</param>
		/// <returns>The feed ID</returns>
		internal abstract string SaveFeed(FeedDetailsInternal feed); 
		

		/// <summary>
		/// Removes a feed from the cache
		/// </summary>
		/// <param name="feed">The feed to remove</param>
		public abstract void RemoveFeed(feedsFeed feed); 

		/// <summary>
		/// Tests whether a feed with the given ID exists in the cache
		/// </summary>
		/// <param name="feed">The feed being searched for</param>
		/// <returns>True if a feed with that ID exists in the cache</returns>
		public abstract bool FeedExists(feedsFeed feed); 


		/// <summary>
		/// Removes every item in the cache. 
		/// </summary>
		public abstract void ClearCache(); 

		/// <summary>
		/// Loads the contents of the NewsItem from the cache. The provided NewsItem must have 
		/// non-null value for its Id property. 
		/// </summary>
		/// <param name="item"></param>
		public abstract void LoadItemContent(NewsItem item); 
	}
}
