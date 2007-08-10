#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region CVS Version Log
/*
 * $Log: FileCacheManager.cs,v $
 * Revision 1.29  2007/02/17 14:45:53  t_rendelmann
 * switched: Resource.Manager indexer usage to strongly typed resources (StringResourceTool)
 *
 * Revision 1.28  2007/02/10 17:22:50  carnage4life
 * Added code to handle FileNotFoundException in LuceneIndexModifier. We now reset the index when this occurs because it indicates that the search index has been corrupted.
 *
 * Revision 1.27  2006/09/29 00:50:48  carnage4life
 * Switched to using FileHelper for loading .bin files
 *
 * Revision 1.26  2006/09/03 19:08:50  carnage4life
 * Added support for favicons
 *
 * Revision 1.25  2006/08/18 19:10:57  t_rendelmann
 * added an "id" XML attribute to the feedsFeed. We need it to make the feed items (feeditem.id + feed.id) unique to enable progressive indexing (lucene)
 *
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Xml;
using NewsComponents.Feed;
using NewsComponents.Resources;
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
				throw new IOException(ComponentsText.ExceptionDirectoryNotExistsMessage(cacheDirectory)); 
			}
		}				
		

		/// <summary>
		/// Returns the directory path to the Cache.
		/// </summary>
		internal override string CacheLocation {
			get{ 
				return this.cacheDirectory; 
			}		
		}



		/// <summary>
		/// Returns an RSS feed. 
		/// </summary>
		/// <param name="feed">The feed whose FeedInfo is required.</param>
		/// <returns>The requested feed or null if it doesn't exist</returns>
		internal override FeedDetailsInternal GetFeed(feedsFeed feed){
			
			if (null == feed || null == feed.cacheurl)
				return null;

			FeedInfo fi = null; 

			string cachelocation = Path.Combine(this.cacheDirectory, feed.cacheurl);

			if(File.Exists(cachelocation)){
				
				using (Stream feedStream = FileHelper.OpenForRead(cachelocation)) {
					fi = RssParser.GetItemsForFeed(feed, feedStream, true); 
				}				  			  				
				this.LoadItemContent(fi); 
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
			
			string feedLocation = feed.FeedLocation;
			
//			if (feed.Type == FeedType.Rss) {

				lock(this){

					if((feed.FeedLocation == null) || (feed.FeedLocation.Length == 0)){
						feed.FeedLocation = feedLocation = GetCacheUrlName(feed.Id, new Uri(feed.Link)); 
					}					
				}				
		
				//get location of binary file containing RSS item contents 
				string feedContentLocation = feedLocation.Substring(0, feedLocation.Length - 4) + ".bin";								

				//write main RSS feed
				using (MemoryStream stream = new MemoryStream()) {
					XmlTextWriter writer = new XmlTextWriter(new StreamWriter(stream )); 
					feed.WriteTo(writer,true); 
					writer.Flush(); 
					FileHelper.WriteStreamWithRename(Path.Combine(this.cacheDirectory,feedLocation), stream);
				}
			
				//write binary file containing RSS item contents 
				using (MemoryStream stream = new MemoryStream()) {
					FileStream fs = null; 
					BinaryReader reader = null; 
					BinaryWriter writer = new BinaryWriter(stream);
					
					try{ 
						if(File.Exists(Path.Combine(this.cacheDirectory, feedContentLocation))){
							fs = new FileStream(Path.Combine(this.cacheDirectory, feedContentLocation), FileMode.OpenOrCreate);
							reader = new BinaryReader(fs);
						}					 
						feed.WriteItemContents(reader, writer); 												
						writer.Write(FileHelper.EndOfBinaryFileMarker); 
						writer.Flush(); 						
					}finally{
						if(reader != null){
							reader.Close(); 
							fs.Close(); 
						}
					}

					FileHelper.WriteStreamWithRename(Path.Combine(this.cacheDirectory,feedContentLocation), stream);									
				}//using(MemoryStream...) {

//			} else if (feed.Type == FeedType.Nntp) {
//
//				throw new NotImplementedException("NntpInfo class impl. missing");
//				
//
//			} else {
//				throw new InvalidOperationException("Unknown/unhandled FeedDetails impl.");
//			}								

			return feedLocation;
		} 
		

		/// <summary>
		/// Removes a feed from the cache
		/// </summary>
		/// <param name="feed">The feed to remove</param>
		public override void RemoveFeed(feedsFeed feed){

			if (feed == null || feed.cacheurl == null)
				return;

			string cachelocation = Path.Combine(this.cacheDirectory, feed.cacheurl);
			string feedContentLocation = Path.Combine(this.cacheDirectory, 
											feed.cacheurl.Substring(0, feed.cacheurl.Length - 4) + ".bin");											
				
			try{ 

				if(File.Exists(cachelocation)){
					FileHelper.Delete(cachelocation); 
				}

				if(File.Exists(feedContentLocation)){
					FileHelper.Delete(feedContentLocation); 
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
		/// Loads the contents of the NewsItem from the cache. The provided NewsItem must have 
		/// non-null value for its Id property. 
		/// </summary>
		/// <param name="item"></param>
		public override void LoadItemContent(NewsItem item){		

			FileStream fs = null; 
			BinaryReader reader = null; 

			try{ 

				string feedContentLocation = item.Feed.cacheurl.Substring(0, item.Feed.cacheurl.Length - 4) + ".bin";											

				if(File.Exists(Path.Combine(this.cacheDirectory, feedContentLocation))){

					fs = FileHelper.OpenForRead(Path.Combine(this.cacheDirectory, feedContentLocation));					
					reader = new BinaryReader(fs); 

					string id = reader.ReadString(); 
				
					while(!id.Equals(FileHelper.EndOfBinaryFileMarker)){
						int count = reader.ReadInt32();
						byte[] content = reader.ReadBytes(count);
				
						if(item.Id.Equals(id)){							
							item.SetContent(content, ContentType.Html); 
							item.RefreshRelationCosmos();
							break; 
						}
						id = reader.ReadString(); 
					}//while(!id.Equals(...))
				}
			
			}finally{
				if(reader != null) {
					reader.Close(); 
				}

				if(fs != null){
					fs.Close(); 
				}			
			}//finally 

		}


		/// <summary>
		/// Loads the content of the unread NewsItems from the binary file 
		/// where item contents are contained. This is a memory-saving performance 
		/// optimization so we only have the content of items that are unread on load.  
		/// </summary>
		/// <param name="fi"></param>
		private void LoadItemContent(FeedInfo fi){
					
			Hashtable unreadItems = new Hashtable(); 
			FileStream fs = null; 
			BinaryReader reader = null; 


			//get list of unread items 
			foreach(NewsItem item in fi.itemsList){
				if(!item.BeenRead){
					try{
						unreadItems.Add(item.Id, item); 
					}catch(ArgumentException){
						/* we don't test using ContainsKey() before Add() for performance reasons */
					}
				}
			}

			try{ 

				string feedContentLocation = fi.feedLocation.Substring(0, fi.feedLocation.Length - 4) + ".bin";											

				if(File.Exists(Path.Combine(this.cacheDirectory, feedContentLocation))){

					fs = FileHelper.OpenForRead(Path.Combine(this.cacheDirectory, feedContentLocation));
					reader = new BinaryReader(fs); 

					string id = reader.ReadString(); 
				
					while(!id.Equals(FileHelper.EndOfBinaryFileMarker)){
						int count = reader.ReadInt32();
						byte[] content = reader.ReadBytes(count);
				
						if(unreadItems.Contains(id)){
							NewsItem ni = (NewsItem) unreadItems[id];  
							ni.SetContent(content, ContentType.Html); 
							ni.RefreshRelationCosmos();
						}
						id = reader.ReadString(); 
					}//while(!id.Equals(...))
				}
			
			}finally{
				if(reader != null) {
					reader.Close(); 
				}

				if(fs != null){
					fs.Close(); 
				}			
			}//finally 
			 		
		}	
		

		/// <summary>
		/// Get a file name that the file can be cached.
		/// </summary>
		/// <param name="id">Id to be used to build</param>
		/// <param name="uri">The uri of the rss document.</param>
		/// <returns>a filename that may be used to save the cached file.</returns>
		private static string GetCacheUrlName(string id, Uri uri) {
			string path = null;
			if (uri.IsFile || uri.IsUnc) {
				path = uri.GetHashCode() + "." + id + ".xml";
			}else{
				path = uri.Host + "." + uri.Port + "." + uri.GetHashCode() + "." + id + ".xml";
			}
			return path.Replace("-","");
		}
	
	}

}
