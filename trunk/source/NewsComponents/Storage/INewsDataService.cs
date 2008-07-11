#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Collections.Generic;
using NewsComponents.Feed;

namespace NewsComponents.Storage
{
	/// <summary>
	/// News Components Data Service interface
	/// </summary>
	public interface INewsDataService
	{
		/// <summary>
		/// Returns an RSS feed as an XmlReader. 
		/// </summary>
		/// <param name="feed">The feed whose FeedInfo is required.</param>
		/// <returns>The requested feed</returns>
		IInternalFeedDetails GetFeed(INewsFeed feed);

		/// <summary>
		/// Saves a particular RSS feed.
		/// </summary>
		/// <remarks>This method should be thread-safe</remarks>
		/// <param name="feed">The feed to save. This is an identifier
		/// and not used to actually fetch the feed from the WWW.</param>
		/// <returns>The feed ID</returns>
		string SaveFeed(IInternalFeedDetails feed);

		/// <summary>
		/// Removes a feed from the cache
		/// </summary>
		/// <param name="feed">The feed to remove</param>
		void RemoveFeed(INewsFeed feed);

		/// <summary>
		/// Tests whether a feed with the given ID exists in the cache
		/// </summary>
		/// <param name="feed">The feed being searched for</param>
		/// <returns>True if a feed with that ID exists in the cache</returns>
		bool FeedExists(INewsFeed feed);

		/// <summary>
		/// Removes every item in the store. 
		/// </summary>
		void RemoveAllNewsItems();

		/// <summary>
		/// Loads the contents of the item from the cache. The provided INewsItem must have 
		/// non-null value for its Id property. 
		/// </summary>
		/// <param name="item"></param>
		void LoadItemContent(INewsItem item);

		#region BinaryContent

		/// <summary>
		/// Saves the content of the binary data.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		/// <param name="data">The data.</param>
		void SaveBinaryContent(string contentId, byte[] data);

		/// <summary>
		/// Gets the binary content, if any.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		/// <returns></returns>
		byte[] GetBinaryContent(string contentId);

		/// <summary>
		/// Deletes the binary content.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		void DeleteBinaryContent(string contentId);

		#endregion

		#region NntpServerDefinitions

		/// <summary>
		/// Saves the NNTP server definitions.
		/// </summary>
		/// <param name="nntpServerDefinitions">The NNTP server definitions.</param>
		void SaveNntpServerDefinitions(List<NntpServerDefinition> nntpServerDefinitions);

		/// <summary>
		/// Loads the NNTP server definitions.
		/// </summary>
		/// <returns></returns>
		List<NntpServerDefinition> LoadNntpServerDefinitions();

		#endregion

	}
}