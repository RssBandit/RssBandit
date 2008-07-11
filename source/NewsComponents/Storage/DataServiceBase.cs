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
using NewsComponents.Feed;

namespace NewsComponents.Storage 
{
	/// <summary>
	/// Abstract service base class for classes that manage the data.  
	/// A storage can be managed by any backing store.  A file based 
	/// data store manager is included as <see cref="FileStorageDataService"/>
	/// </summary>
	public abstract class DataServiceBase : IUserCacheDataService, IInitDataService,
		IUserDataService, IUserRoamingDataService
	{

		/// <summary>
		/// Returns an RSS feed as an XmlReader. 
		/// </summary>
		/// <param name="feed">The feed whose FeedInfo is required.</param>
		/// <returns>The requested feed</returns>
		public abstract IInternalFeedDetails GetFeed(INewsFeed feed); 

		/// <summary>
		/// Saves a particular RSS feed.
		/// </summary>
		/// <remarks>This method should be thread-safe</remarks>
		/// <param name="feed">The feed to save. This is an identifier
		/// and not used to actually fetch the feed from the WWW.</param>
		/// <returns>The feed ID</returns>
		public abstract string SaveFeed(IInternalFeedDetails feed); 
		

		/// <summary>
		/// Removes a feed from the cache
		/// </summary>
		/// <param name="feed">The feed to remove</param>
		public abstract void RemoveFeed(INewsFeed feed); 

		/// <summary>
		/// Tests whether a feed with the given ID exists in the cache
		/// </summary>
		/// <param name="feed">The feed being searched for</param>
		/// <returns>True if a feed with that ID exists in the cache</returns>
		public abstract bool FeedExists(INewsFeed feed); 


		/// <summary>
		/// Removes every item in the cache. 
		/// </summary>
		public abstract void RemoveAllNewsItems(); 

		/// <summary>
		/// Loads the contents of the NewsItem from the cache. The provided NewsItem must have 
		/// non-null value for its Id property. 
		/// </summary>
		/// <param name="item"></param>
		public abstract void LoadItemContent(INewsItem item);

		/// <summary>
		/// Saves the content of the binary data.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		/// <param name="data">The data.</param>
		public abstract void SaveBinaryContent(string contentId, byte[] data);

		/// <summary>
		/// Gets the binary content, if any.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		/// <returns></returns>
		public abstract byte[] GetBinaryContent(string contentId);

		/// <summary>
		/// Deletes the binary content.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		public abstract void DeleteBinaryContent(string contentId);

		#region IUserDataService

		/// <summary>
		/// Saves the NNTP server definitions.
		/// </summary>
		/// <param name="nntpServerDefinitions">The NNTP server definitions.</param>
		public abstract void SaveNntpServerDefinitions(List<NntpServerDefinition> nntpServerDefinitions);

		/// <summary>
		/// Loads the NNTP server definitions.
		/// </summary>
		/// <returns></returns>
		public abstract List<NntpServerDefinition> LoadNntpServerDefinitions();

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
		#region Implementation of IInitDataService

		/// <summary>
		/// The init data
		/// </summary>
		protected string initializationData;
		
		/// <summary>
		/// Initializes the data service with the specified initialization data.
		/// </summary>
		/// <param name="initData">The initialization data.
		/// Can be a connection string, or a file path;
		/// depending on the implementation of the data service</param>
		public virtual void Initialize(string initData)
		{
			initializationData = initData;
		}

		#endregion
	}
}
