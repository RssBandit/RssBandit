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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NewsComponents.Feed;
using NewsComponents.Utils;

namespace NewsComponents.Storage {	

	/// <summary>
	/// An implementation of the DataService that uses the file system as a cache. 
	/// </summary>
	internal class FileStorageDataService : DataServiceBase
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
		/// Initializes the data service with the specified initialization data.
		/// </summary>
		/// <param name="initData">The initialization data.
		/// Here it is a file path base, used as a cache;
		/// depending on the implementation of the data service</param>
		public override void Initialize(string initData)
		{
			initData.ExceptionIfNullOrEmpty("initData");
			base.Initialize(initData);
			
			if(!Directory.Exists(CacheLocation))
				Directory.CreateDirectory(CacheLocation); 
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
		public override string SaveFeed(IInternalFeedDetails feed)
		{

			string feedLocation = feed.FeedLocation;

			lock (this)
			{
				if (string.IsNullOrEmpty(feed.FeedLocation))
				{
					feed.FeedLocation = feedLocation = GetCacheUrlName(feed.Id, new Uri(feed.Link));
				}
			}

			//get location of binary file containing RSS item contents 
			string feedContentLocation = feedLocation.Substring(0, feedLocation.Length - 4) + ".bin";

			//write main RSS feed
			var tempFile = Path.GetTempFileName();
			using (FileStream stream = FileHelper.OpenForWrite(tempFile))
			{
				XmlTextWriter writer = new XmlTextWriter(new StreamWriter(stream));
				feed.WriteTo(writer, true);
				writer.Flush();
			}
			FileHelper.MoveFile(tempFile, Path.Combine(CacheLocation, feedLocation), MoveFileFlag.ReplaceExisting);

			//write binary file containing RSS item contents 
			tempFile = Path.GetTempFileName();
			using (FileStream stream = FileHelper.OpenForWrite(tempFile))
			{
				var writer = new BinaryWriter(stream);
				
				FileStream fs = null;
				BinaryReader reader = null;
				
				try
				{
					if (File.Exists(Path.Combine(CacheLocation, feedContentLocation)))
					{
						fs = new FileStream(Path.Combine(CacheLocation, feedContentLocation), FileMode.OpenOrCreate);
						reader = new BinaryReader(fs);
					}

					feed.WriteItemContents(reader, writer);
					writer.Write(FileHelper.EndOfBinaryFileMarker);
					writer.Flush();
				}
				finally
				{
					if (fs != null)
					{
						fs.Close();
					}
				}
				
			}//using(...) 

			FileHelper.MoveFile(tempFile, Path.Combine(CacheLocation, feedContentLocation), MoveFileFlag.ReplaceExisting);
			
			return feedLocation;
		} 
		

		/// <summary>
		/// Removes a feed from the cache
		/// </summary>
		/// <param name="feed">The feed to remove</param>
		public override void RemoveFeed(INewsFeed feed)
		{

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
		public override void LoadItemContent(INewsItem target)
		{
			NewsItem item = target as NewsItem; 
			FileStream fs = null;

			// item.Feed.cacheurl == null could happen on a search result item,
			// e.g. returned by a websearch rss:
			if (item == null || item.Feed.cacheurl == null)
            {
                return;
            }

			try
			{ 
				string feedContentLocation = item.Feed.cacheurl.Substring(0, item.Feed.cacheurl.Length - 4) + ".bin";
				string fileName = Path.Combine(CacheLocation, feedContentLocation);
				
				if(File.Exists(fileName))
				{
					fs = FileHelper.OpenForRead(fileName);					
					var reader = new BinaryReader(fs); 

					string id = reader.ReadString(); 
				
					while(!id.Equals(FileHelper.EndOfBinaryFileMarker))
					{
						int count = reader.ReadInt32();
						byte[] content = reader.ReadBytes(count);
				
						if(item.Id.Equals(id))
						{							
							item.SetContent(content, ContentType.Html); 							
							break; 
						}
						id = reader.ReadString(); 
					}//while(!id.Equals(...))
				}
			}
			finally
			{
				if(fs != null)
				{
					fs.Close(); 
				}			
			}
		}


		/// <summary>
		/// Loads the content of the unread NewsItems from the binary file 
		/// where item contents are contained. This is a memory-saving performance 
		/// optimization so we only have the content of items that are unread on load.  
		/// </summary>
		/// <param name="fi"></param>
		private void LoadItemContent(IInternalFeedDetails fi)
		{
            if (fi == null)
            {
                return;
            }
					
			Hashtable unreadItems = new Hashtable(); 
			FileStream fs = null; 
			BinaryReader reader = null; 


			//get list of unread items 
			foreach(NewsItem item in fi.ItemsList)
			{
				if(!item.BeenRead)
				{
					try
					{
						unreadItems.Add(item.Id, item); 
					}
					catch(ArgumentException) 
					{
						/* we don't test using ContainsKey() before Add() for performance reasons */
					}
				}
			}

			try
			{ 
				string feedContentLocation = fi.FeedLocation.Substring(0, fi.FeedLocation.Length - 4) + ".bin";
				string fileName = Path.Combine(CacheLocation, feedContentLocation);
				if(File.Exists(fileName))
				{
					fs = FileHelper.OpenForRead(fileName);
					reader = new BinaryReader(fs); 

					string id = reader.ReadString(); 
				
					while(!string.IsNullOrEmpty(id) && !id.Equals(FileHelper.EndOfBinaryFileMarker))
					{
						int count = reader.ReadInt32();
						byte[] content = reader.ReadBytes(count);
				
						if(unreadItems.Contains(id))
						{
							NewsItem ni = (NewsItem) unreadItems[id];  
							ni.SetContent(content, ContentType.Html); 
						}
						id = reader.ReadString(); 
					}//while(!id.Equals(...))
				}
			}
			finally
			{
				if(fs != null)
				{
					fs.Close(); 
				}			
			}
		}

        /// <summary>
        /// Saves the content of the binary data.
        /// </summary>
        /// <param name="contentId">The content id.</param>
        /// <param name="data">The data.</param>
        /// <returns>The location Uri for embedding purposes</returns>
        public override Uri SaveBinaryContent(string contentId, byte[] data)
		{
			string fileName = Path.Combine(CacheLocation, contentId);
			using (FileStream fs = FileHelper.OpenForWrite(fileName))
			{
				var bw = new BinaryWriter(fs);
				bw.Write(data);
				bw.Flush();
			}
            return new Uri(fileName);
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

		#region NntpServerDefinitions

		/// <summary>
		/// Saves the NNTP server definitions.
		/// </summary>
		/// <param name="nntpServerDefinitions">The NNTP server definitions.</param>
		public override void SaveNntpServerDefinitions(List<NntpServerDefinition> nntpServerDefinitions)
		{
			string fileName = NntpServerDefsFileName;
			if (nntpServerDefinitions == null || nntpServerDefinitions.Count == 0)
            {
				if (File.Exists(fileName)) 
					FileHelper.Delete(fileName);
            	return;
            }  
				
			XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableNntpServerDefinitions));
            using (Stream s = FileHelper.OpenForWrite(fileName))
            {
				SerializableNntpServerDefinitions root = new SerializableNntpServerDefinitions();
            	root.List = nntpServerDefinitions;
            	serializer.Serialize(s, root);
            }
		}

		/// <summary>
		/// Loads the NNTP server definitions.
		/// </summary>
		/// <returns></returns>
		public override List<NntpServerDefinition> LoadNntpServerDefinitions()
		{
			string fileName = NntpServerDefsFileName;
			if (File.Exists(fileName))
			{
				XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableNntpServerDefinitions));
				using (Stream s = FileHelper.OpenForRead(fileName))
				{
					SerializableNntpServerDefinitions root = (SerializableNntpServerDefinitions)serializer.Deserialize(s);
					return root.List;
				}
			}
			return null;
		}
		
		private string NntpServerDefsFileName
		{
			get
			{
				return Path.Combine(CacheLocation,
					"nntp-server-definitions.xml");
			}
		}

		#endregion

		/// <summary>
		/// Gets the used user data file names.
		/// </summary>
		/// <returns></returns>
		public override string[] GetUserDataFileNames()
		{
			return new string[]{NntpServerDefsFileName};
		}

		public override DataEntityName SetContentForDataFile(string dataFileName, Stream content)
		{
			string fileName = Path.GetFileName(NntpServerDefsFileName);
			if (String.Equals(dataFileName, fileName, StringComparison.OrdinalIgnoreCase))
			{
				FileHelper.WriteStreamWithBackup(NntpServerDefsFileName, content);
				return DataEntityName.NntpServerDefinitions;
			}

			return DataEntityName.None;
		}

		/// <summary>
		/// Get a file name that the file can be cached.
		/// </summary>
		/// <param name="id">Id to be used to build</param>
		/// <param name="uri">The uri of the rss document.</param>
		/// <returns>a filename that may be used to save the cached file.</returns>
		private static string GetCacheUrlName(string id, Uri uri) {
			string path;
			if (uri.IsFile || uri.IsUnc) {
				path = uri.GetHashCode() + "." + id + ".xml";
			}else{
				path = uri.Host + "." + uri.Port + "." + uri.GetHashCode() + "." + id + ".xml";
			}
			return path.Replace("-","");
		}

	}

	#region SerializableNntpServerDefinitions

	/// <summary>
	/// NNTP Server Definition serializable root class
	/// </summary>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	[XmlRoot("nntp-servers", Namespace = NamespaceCore.Feeds_vCurrent, IsNullable = false)]
	public class SerializableNntpServerDefinitions
	{
		/// <remarks/>
		[XmlElement("server", Type = typeof(NntpServerDefinition), IsNullable = false)]
		//public ArrayList List = new ArrayList();
		public List<NntpServerDefinition> List = new List<NntpServerDefinition>();
	}
	#endregion

}
