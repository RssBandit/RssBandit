#region CVS Version Header
/*
 * $Id: FileCacheManager.cs,v 1.14 2005/04/04 03:53:01 haacked Exp $
 * Last modified by $Author: haacked $
 * Last modified at $Date: 2005/04/04 03:53:01 $
 * $Revision: 1.14 $
 */
#endregion

using System;
using System.IO;
using System.Xml;
using NewsComponents.Feed;
using NewsComponents.Utils;

namespace NewsComponents.Storage {	

	/// <summary>
	/// An implementation of the CacheManager that uses the file system as a cache. 
	/// </summary>
	public class FileCacheManager : CacheManager{		

		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(FileCacheManager));

		private string cacheDirectory = null; 		

		/// <summary>
		/// Default constructor initializes class. 
		/// </summary>
		private FileCacheManager(){;}

		/// <summary>
		/// Constructor initializes class and sets the cache directory
		/// </summary>
		/// <param name="cacheDirectory">The cache directory</param>
		/// <exception cref="IOException">If the directory doesn't exist</exception>
		public FileCacheManager(string cacheDirectory){
			if(Directory.Exists(cacheDirectory)){
				this.cacheDirectory = cacheDirectory; 
			}else{
				throw new IOException(Resource.Manager["RES_ExceptionDirectoryNotExistsMessage", cacheDirectory]); 
			}
		}		
		
		
		/* 
		/// <summary>
		///  Sets the maximum amount of time an item should be kept in the 
		/// cache for a particular feed. This overrides the value of the 
		/// maxItemAge property. 
		/// </summary>
		/// <remarks>This method should be thread-safe</remarks>
		/// <param name="feed">The feed</param>
		/// <param name="age">The maximum amount of time items should be kept for the 
		/// specified feed.</param>
		/// <exception cref="IOException">If an error occurs while saving the feed</exception>
		[MethodImpl(MethodImplOptions.Synchronized)]
		[Obsolete("Should live in feedlist.xml")]
		public override void SetMaxItemAge(feedsFeed feed, TimeSpan age){
			FeedDetailsInternal fd = this.GetFeed(feed);
			if(fd != null){ //perhaps should throw exception if null? Not much for caller to do though. 
				fd.MaxItemAge = age; 
				feed.cacheurl = this.SaveFeed(fd); 
			}
		} 

		/// <summary>
		/// Gets the maximum amount of time an item is kept in the 
		/// cache for a particular feed. 
		/// </summary>
		/// <param name="feed">The feed identifier</param>
		/// <exception cref="IOException">If an error occurs while retrieving the feed</exception>
		[Obsolete("Should live in feedlist.xml")]
		public override TimeSpan GetMaxItemAge(feedsFeed feed){
			 
			 //TODO: Implement more efficiently by using XmlReader to scan XML on disk
			 FeedDetails fd   = this.GetFeed(feed);
			 return (fd == null ? this.maxItemAge : fd.MaxItemAge); 
		} */

		/// <summary>
		/// Returns an RSS feed. 
		/// </summary>
		/// <param name="feed">The feed whose FeedInfo is required.</param>
		/// <returns>The requested feed or null if it doesn't exist</returns>
		internal override FeedDetailsInternal GetFeed(feedsFeed feed){
			
			if (null == feed || null == feed.cacheurl)
				return null;

			FeedDetailsInternal fi = null; 

			string cachelocation = Path.Combine(this.cacheDirectory, feed.cacheurl);

			if(File.Exists(cachelocation)){
				
				using (Stream feedStream = FileHelper.OpenForRead(cachelocation)) {
					fi = RssParser.GetItemsForFeed(feed, feedStream, true); 
				}				  			  				

			}

			return fi; 			
		} 

		/// <summary>
		/// Saves a particular RSS feed.
		/// </summary>
		/// <remarks>This method should be thread-safe</remarks>
		/// <param name="feed">The feed to save. This is an identifier
		/// and not used to actually fetch the feed from the WWW.</param>
		/// <returns>An identifier for the saved feed. </returns>		
		internal override string SaveFeed(FeedDetailsInternal feed){
			
			string feedLocation = null;

			if (feed.Type == FeedType.Rss) {

				lock(this){

					if((feed.FeedLocation == null) || (feed.FeedLocation.Length == 0)){
						feed.FeedLocation = GetCacheUrlName(new Uri(feed.Link)); 
					}

					feedLocation= feed.FeedLocation;
				}				
			
				// RemoveExpiredItems(feed.ItemsList, maxItemAge);
			
				using (MemoryStream stream = new MemoryStream()) {
					XmlTextWriter writer = new XmlTextWriter(new StreamWriter(stream )); 
					feed.WriteTo(writer); 
					writer.Flush(); 
					FileHelper.WriteStreamWithRename(Path.Combine(this.cacheDirectory,feedLocation), stream);
				}
			
			} else if (feed.Type == FeedType.Nntp) {

				throw new NotImplementedException("NntpInfo class impl. missing");
				

			} else {
				throw new InvalidOperationException("Unknown/unhandled FeedDetails impl.");
			}

			return feedLocation;
		} 
		

		/// <summary>
		/// Removes a feed from the cache
		/// </summary>
		/// <param name="feed">The feed to remove</param>
		public override void RemoveFeed(feedsFeed feed){

			string cachelocation = Path.Combine(this.cacheDirectory, feed.cacheurl);
		
			try{ 

			if(File.Exists(cachelocation)){
				FileHelper.Delete(cachelocation); 
			}

			}catch(IOException iox){ 
				_log.Debug("RemoveFeed: Could not delete " + cachelocation, iox); 
			}
			
		}


		/// <summary>
		/// Removes every item in the cache. 
		/// </summary>
		public override void ClearCache(){

			try{ 

				lock(this){ 
			
					string [] fileEntries = Directory.GetFiles(cacheDirectory);
				
					foreach(string fileName in fileEntries){
						FileHelper.Delete(fileName); 
					}
				}
			}catch(IOException ioe){
				_log.Debug("Error Clearing Cache", ioe); 
			} 
		
		}

		/// <summary>
		/// Tests whether a feed with the given ID exists in the cache
		/// </summary>
		/// <param name="feed">The feed</param>
		/// <returns>True if a feed with that ID exists in the cache</returns>
		public override bool FeedExists(feedsFeed feed){	
	
			string cachelocation = Path.Combine(this.cacheDirectory, feed.cacheurl);
			return File.Exists(cachelocation); 
		}

		/// <summary>
		/// Get a file name that the file can be cached.
		/// </summary>
		/// <param name="uri">The uri of the rss document.</param>
		/// <returns>a filename that may be used to save the cached file.</returns>
		private static string GetCacheUrlName(Uri uri) {
			string path = null;
			if (uri.IsFile || uri.IsUnc) {
				path = uri.GetHashCode() + "." + System.Guid.NewGuid() + ".xml";
			}else{
				path = uri.Host + "." + uri.Port + "." + uri.GetHashCode() + "." + System.Guid.NewGuid() + ".xml";
			}
			return path.Replace("-","");
		}
	
	}

}
