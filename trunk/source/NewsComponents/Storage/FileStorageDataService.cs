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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using NewsComponents.Feed;
using NewsComponents.Resources;
using NewsComponents.Utils;

namespace NewsComponents.Storage {	

	/// <summary>
	/// An implementation of the DataService that uses the file system as a cache. 
	/// </summary>
	public class FileStorageDataService : NewsDataService
	{		

		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(FileStorageDataService));

		/// <summary>
		/// Returns the location of the cache. The format of the location is dependent 
		/// on the DataService implementation. 
		/// </summary>
		string CacheLocation
		{
			[DebuggerStepThrough]
			get { return initializationData; }
		}


		/// <summary>
		/// Constructor initializes class and sets the cache directory
		/// </summary>
		/// <param name="cacheDirectory">The cache directory</param>
		/// <exception cref="IOException">If the directory doesn't exist</exception>
		public FileStorageDataService(string cacheDirectory){
			if(Directory.Exists(cacheDirectory)){
				this.initializationData = cacheDirectory; 
			}else{
				throw new IOException(String.Format(ComponentsText.ExceptionDirectoryNotExistsMessage, cacheDirectory)); 
			}
		}				
		
		/// <summary>
		/// Returns an RSS feed. 
		/// </summary>
		/// <param name="feed">The feed whose FeedInfo is required.</param>
		/// <returns>The requested feed or null if it doesn't exist</returns>
		public override IInternalFeedDetails GetFeed(INewsFeed feed){
			
			if (null == feed || null == feed.cacheurl)
				return null;

            IInternalFeedDetails fi = null;

			string cachelocation = Path.Combine(CacheLocation, feed.cacheurl);

			if(File.Exists(cachelocation)){
				
				using (Stream feedStream = FileHelper.OpenForRead(cachelocation)) {
					fi = RssParser.GetItemsForFeed(feed, feedStream, true) as IInternalFeedDetails; 
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
		public override string SaveFeed(IInternalFeedDetails feed){
			
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
				//using (MemoryStream stream = new MemoryStream()) {
                using(FileStream stream = FileHelper.OpenForWrite(Path.GetTempFileName())){
					XmlTextWriter writer = new XmlTextWriter(new StreamWriter(stream )); 
					feed.WriteTo(writer,true); 
					writer.Flush();                   
                    writer.Close(); 
					//FileHelper.WriteStreamWithRename(Path.Combine(this.cacheDirectory,feedLocation), stream);
					FileHelper.MoveFile(stream.Name, Path.Combine(CacheLocation, feedLocation), MoveFileFlag.ReplaceExisting);                                      
				}
			
				//write binary file containing RSS item contents 
				//using (MemoryStream stream = new MemoryStream()) {
                using (FileStream stream = FileHelper.OpenForWrite(Path.GetTempFileName())){				
					FileStream fs = null; 
					BinaryReader reader = null; 
					BinaryWriter writer = new BinaryWriter(stream);
					
					try{
						if (File.Exists(Path.Combine(CacheLocation, feedContentLocation)))
						{
							fs = new FileStream(Path.Combine(CacheLocation, feedContentLocation), FileMode.OpenOrCreate);
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
                    writer.Close(); 
				 //	FileHelper.WriteStreamWithRename(Path.Combine(this.cacheDirectory,feedContentLocation), stream);									
					FileHelper.MoveFile(stream.Name, Path.Combine(CacheLocation, feedContentLocation), MoveFileFlag.ReplaceExisting);                    
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
		public override void RemoveFeed(INewsFeed feed){

			if (feed == null || feed.cacheurl == null)
				return;

			string cachelocation = Path.Combine(CacheLocation, feed.cacheurl);
			string feedContentLocation = Path.Combine(CacheLocation, 
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
		public override void RemoveAllNewsItems(){

			try{ 

				lock(this){

					string[] fileEntries = Directory.GetFiles(CacheLocation);
				
					foreach(string fileName in fileEntries){
                        try {
                            FileHelper.Delete(fileName);
                        } catch (IOException ioe) {
                            _log.Debug("Error deleting " + fileName + " while clearing cache: ", ioe);
                        } 
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
		public override bool FeedExists(INewsFeed feed){	
	
			string cachelocation = Path.Combine(CacheLocation, feed.cacheurl);
			return File.Exists(cachelocation); 
		}


		/// <summary>
		/// Loads the contents of the NewsItem from the cache. The provided NewsItem must have
		/// non-null value for its Id property.
		/// </summary>
		/// <param name="target">The target.</param>
		public override void LoadItemContent(INewsItem target){

            NewsItem item = target as NewsItem; 
			FileStream fs = null; 
			BinaryReader reader = null;

			// item.Feed.cacheurl == null could happen on a search result item,
			// e.g. returned by a websearch rss:
			if (item == null || item.Feed.cacheurl == null)
            {
                return;
            }

			try{ 

				string feedContentLocation = item.Feed.cacheurl.Substring(0, item.Feed.cacheurl.Length - 4) + ".bin";
				string fileName = Path.Combine(CacheLocation, feedContentLocation);
				
				if(File.Exists(fileName)){

					fs = FileHelper.OpenForRead(fileName);					
					reader = new BinaryReader(fs); 

					string id = reader.ReadString(); 
				
					while(!id.Equals(FileHelper.EndOfBinaryFileMarker)){
						int count = reader.ReadInt32();
						byte[] content = reader.ReadBytes(count);
				
						if(item.Id.Equals(id)){							
							item.SetContent(content, ContentType.Html); 							
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
		private void LoadItemContent(IInternalFeedDetails fi){

            if (fi == null)
            {
                return;
            }
					
			Hashtable unreadItems = new Hashtable(); 
			FileStream fs = null; 
			BinaryReader reader = null; 


			//get list of unread items 
			foreach(NewsItem item in fi.ItemsList){
				if(!item.BeenRead){
					try{
						unreadItems.Add(item.Id, item); 
					}catch(ArgumentException){
						/* we don't test using ContainsKey() before Add() for performance reasons */
					}
				}
			}

			try{ 

				string feedContentLocation = fi.FeedLocation.Substring(0, fi.FeedLocation.Length - 4) + ".bin";
				string fileName = Path.Combine(CacheLocation, feedContentLocation);
				if(File.Exists(fileName)){

					fs = FileHelper.OpenForRead(fileName);
					reader = new BinaryReader(fs); 

					string id = reader.ReadString(); 
				
					while(!string.IsNullOrEmpty(id) && !id.Equals(FileHelper.EndOfBinaryFileMarker)){
						int count = reader.ReadInt32();
						byte[] content = reader.ReadBytes(count);
				
						if(unreadItems.Contains(id)){
							NewsItem ni = (NewsItem) unreadItems[id];  
							ni.SetContent(content, ContentType.Html); 
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
		/// Saves the content of the binary data.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		/// <param name="data">The data.</param>
		public override void SaveBinaryContent(string contentId, byte[] data)
		{
			string fileName = Path.Combine(CacheLocation, contentId);
			using (FileStream fs = FileHelper.OpenForWrite(fileName))
			{
				var bw = new BinaryWriter(fs);
				bw.Write(data);
				bw.Flush();
			}
		}
		/// <summary>
		/// Gets the binary content, if any.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		/// <returns></returns>
		public override byte[] GetBinaryContent(string contentId)
		{
			string fileName = Path.Combine(CacheLocation, contentId);
			if (File.Exists(fileName))
			{
				using (FileStream fs = FileHelper.OpenForRead(fileName))
				{
					long length = fs.Length;
					if (length == 0)
						return null;
					if (length > Int32.MaxValue)
						throw new InvalidOperationException("Binary blob too large");
					var bw = new BinaryReader(fs);
					return bw.ReadBytes((int)length);
				}
			}
			return null;
		}

		/// <summary>
		/// Deletes the binary content.
		/// </summary>
		/// <param name="contentId">The content id.</param>
		public override void DeleteBinaryContent(string contentId)
		{
			string fileName = Path.Combine(CacheLocation, contentId);
			if (File.Exists(fileName))
			{
				FileHelper.Delete(fileName);
			}
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
