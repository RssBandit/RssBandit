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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

using System.Runtime.InteropServices;
using Microsoft.Feeds.Interop;
using NewsComponents.Core;
using RssBandit.Common;
using RssBandit.Common.Logging;

using log4net;

using NewsComponents.Collections;
using NewsComponents.Net;
using NewsComponents.RelationCosmos;
using NewsComponents.Utils;

namespace NewsComponents.Feed
{

    #region IWindowsRssFeedSource

    /// <summary>
	/// public <see cref="FeedSource"/> extension offered by NewsGator Feed Source
	/// </summary>
    public interface IWindowsRssFeedSource
    {

    }

    #endregion 


    #region WindowsRssPlatformException

    /// <summary>
    /// Indicates that an exception occurred in the Windows RSS platform and the feed list must be reloaded. 
    /// </summary>
    public class WindowsRssPlatformException : Exception {

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowsRssPlatformException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
        public WindowsRssPlatformException(string message) : base(message) { }
    }

    #endregion 

    #region WindowsRssFeedSource

    /// <summary>
    /// A FeedSource that retrieves user subscriptions and feeds from the Windows RSS platform. 
    /// </summary>
    class WindowsRssFeedSource : FeedSource, IFeedFolderEvents, IWindowsRssFeedSource
    {


        #region constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedSource"/> class.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <param name="location">The location infos.</param>
		public WindowsRssFeedSource(INewsComponentsConfiguration configuration, SubscriptionLocation location)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = FeedSource.DefaultConfiguration;

            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);
			
			this.Configuration.PropertyChanged += OnConfigurationPropertyChanged;
         	ApplyRefreshRate(this.Configuration.RefreshRate);

            this.location = location; 

            this.AttachEventHandlers();
            try
            {
                feedManager.BackgroundSync(FEEDS_BACKGROUNDSYNC_ACTION.FBSA_ENABLE);
            }
            catch (ArgumentException) { /* weird error */ }
            
        }    

        #endregion 


        #region private fields and properties 

        /// <summary>
        /// The Windows RSS platform feed manager
        /// </summary>
        private readonly IFeedsManager feedManager = new FeedsManagerClass();

        /// <summary>
        /// Needed for event handling support with Windows RSS platform
        /// </summary>
        private IFeedFolderEvents_Event fw;


        /// <summary>
        /// Indicates whether a folder renamed event received from the Windows RSS platform was caused by RSS Bandit
        /// </summary>
        internal static bool folder_renamed_event_caused_by_rssbandit;

        /// <summary>
        /// Synchronization point for folder_renamed_event_caused_by_rssbandit
        /// </summary>
        /// <seealso cref="event_caused_by_rssbandit"/>
        internal static Object folder_renamed_event_caused_by_rssbandit_syncroot = new Object(); 



        /// <summary>
        /// Indicates whether a folder moved event received from the Windows RSS platform was caused by RSS Bandit
        /// </summary>
        internal static bool folder_moved_event_caused_by_rssbandit;

        /// <summary>
        /// Synchronization point for folder_moved_event_caused_by_rssbandit
        /// </summary>
        /// <seealso cref="event_caused_by_rssbandit"/>
        internal static Object folder_moved_event_caused_by_rssbandit_syncroot = new Object(); 


        /// <summary>
        /// Indicates whether an event received from the Windows RSS platform was caused by RSS Bandit
        /// </summary>
        internal static bool event_caused_by_rssbandit;

        /// <summary>
		/// Synchronization point for event_caused_by_rssbandit
        /// </summary>
		/// <seealso cref="event_caused_by_rssbandit"/>
        internal static Object event_caused_by_rssbandit_syncroot = new Object(); 

         private static readonly ILog _log = Log.GetLogger(typeof (WindowsRssFeedSource));


        #endregion 

         #region public fields and properties 

		/// <summary>
		/// Enables or disables automatic polling of currently subscribed feeds
		/// </summary>
		/// <param name="enabled">Indicates whether automatic polling of feeds should be enabled or disabled</param>
         internal void ToggleFeedPolling(bool enabled)
         {
             string[] keys = this.GetFeedsTableKeys();

             foreach (string key in keys)
             {
                 INewsFeed f;
                 feedsTable.TryGetValue(key, out f);
                 WindowsRssNewsFeed feed = f as WindowsRssNewsFeed;

                 if (feed != null)
                 {
                     if (enabled){
                         feed.EnableFeedPolling();
                     }else{
                         feed.DisableFeedPolling();
                     }
                 }
             }//foreach
         }

         /// <summary>
         ///  How often feeds are refreshed by default if no specific rate specified by the feed. 
         ///  Setting this property resets the refresh rate for all feeds. Value is in milliseconds.
         /// </summary>
         /// <remarks>If set to a value less than 15 minutes then the old value remains. Setting the 
         /// value to zero means feeds are no longer updated.</remarks>
         public override int RefreshRate
         {
			 //set
			 //{
			 //    if (value >= 15 * 60000)
			 //    {
			 //        this.feedManager.DefaultInterval = value / 60000;
			 //    }
			 //}

             get
             {
				 // base impl. gets the configuration refreshrate. 
                 return  (feedManager.DefaultInterval == Int32.MaxValue ? 0 :
                     feedManager.DefaultInterval * 60000 /* convert to milliseconds */ ); 
             }
         }

         #endregion

         #region private methods




         /// <summary>
         /// Applies the refresh rate to the WinRSS platform feedManager.
         /// </summary>
         /// <param name="value">The value.</param>
         internal void ApplyRefreshRate(int value)
         {
             //Dare: the code is copied from RefreshRate{set;}
             // Missing: handling the case with value == 0
             // Missing: initialize with a default 
             if (value >= 15 * 60000)
             {
                 this.feedManager.DefaultInterval = value / 60000;
             }
             else if (value == 0)
             {
                 this.feedManager.DefaultInterval = int.MaxValue;
                 this.ToggleFeedPolling(false); 
             }
         }


		 /// <summary>
		 /// Attaches event handlers to the root IFeedFolder
		 /// </summary>
        internal void AttachEventHandlers()
        {
			IFeedFolder folder = feedManager.RootFolder as IFeedFolder;
		 	if (folder != null) 
			{
		 		fw = (IFeedFolderEvents_Event)folder.GetWatcher(
		 		    FEEDS_EVENTS_SCOPE.FES_ALL, FEEDS_EVENTS_MASK.FEM_FOLDEREVENTS);

				fw.Error += Error;
				fw.FeedAdded += FeedAdded;
				fw.FeedDeleted += FeedDeleted;
				fw.FeedDownloadCompleted += FeedDownloadCompleted;
				fw.FeedDownloading += FeedDownloading;
				fw.FeedItemCountChanged += FeedItemCountChanged;
				fw.FeedMovedFrom += FeedMovedFrom;
				fw.FeedMovedTo += FeedMovedTo;
				fw.FeedRenamed += FeedRenamed;
				fw.FeedUrlChanged += FeedUrlChanged;
				fw.FolderAdded += FolderAdded;
				fw.FolderDeleted += FolderDeleted;
				fw.FolderItemCountChanged += FolderItemCountChanged;
				fw.FolderMovedFrom += FolderMovedFrom;
				fw.FolderMovedTo += FolderMovedTo;
				fw.FolderRenamed += FolderRenamed;
			}
        }

		void OnConfigurationPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "RefreshRate") {
				ApplyRefreshRate(this.Configuration.RefreshRate);
			}
		}


		/// <summary>
		/// Detaches event handlers from the root IFeedFolder
		/// </summary>
        internal void DetachEventHandlers()
        {
            IFeedFolder folder = feedManager.RootFolder as IFeedFolder;
			if (folder != null) {
				fw = (IFeedFolderEvents_Event)folder.GetWatcher(
					FEEDS_EVENTS_SCOPE.FES_ALL, FEEDS_EVENTS_MASK.FEM_FOLDEREVENTS);

				fw.Error -= Error;
				fw.FeedAdded -= FeedAdded;
				fw.FeedDeleted -= FeedDeleted;
				fw.FeedDownloadCompleted -= FeedDownloadCompleted;
				fw.FeedDownloading -= FeedDownloading;
				fw.FeedItemCountChanged -= FeedItemCountChanged;
				fw.FeedMovedFrom -= FeedMovedFrom;
				fw.FeedMovedTo -= FeedMovedTo;
				fw.FeedRenamed -= FeedRenamed;
				fw.FeedUrlChanged -= FeedUrlChanged;
				fw.FolderAdded -= FolderAdded;
				fw.FolderDeleted -= FolderDeleted;
				fw.FolderItemCountChanged -= FolderItemCountChanged;
				fw.FolderMovedFrom -= FolderMovedFrom;
				fw.FolderMovedTo -= FolderMovedTo;
				fw.FolderRenamed -= FolderRenamed;
			}
        }
        /// <summary>
        /// Add a folder to the Windows RSS common feed list
        /// </summary>
        /// <param name="path">The path to the folder</param>
        public IFeedFolder AddFolder(string path)
        {

            IFeedFolder folder = feedManager.RootFolder as IFeedFolder;

            lock (event_caused_by_rssbandit_syncroot)
            {
                if (!StringHelper.EmptyTrimOrNull(path))
                {
                    string[] categoryPath = path.Split(new char[] { '\\' });

                    foreach (string c in categoryPath)
                    {
                        if (folder.ExistsSubfolder(c))
                        {
                            folder = folder.GetSubfolder(c) as IFeedFolder;
                        }
                        else
                        {
                            folder = folder.CreateSubfolder(c) as IFeedFolder;
                            if (!folder.Path.Equals(path) && !categories.ContainsKey(folder.Path))
                            {
                                this.categories.Add(folder.Path, new WindowsRssNewsFeedCategory(folder));
                            }
                        }
                    }
                }// if (!StringHelper.EmptyTrimOrNull(category))           

                event_caused_by_rssbandit = true;
            }
            return folder;
        }


        #endregion 

        #region public methods      


         /// <summary>
        /// Resumes pending BITS downloads from a if any exist. 
        /// </summary>
        public override void ResumePendingDownloads()
        {
            /* Do nothing here. This is handled by the Windows RSS platform automatically */ 
        }


        /// <summary>
        /// Returns the FeedDetails of a feed.
        /// </summary>
        /// <param name="feedUrl">string feed's Url</param>
        /// <returns>FeedInfo or null, if feed was removed or parameter is invalid</returns>
        /* TODO: Why does this lead to InvalidComException later on? */ 
         public override IFeedDetails GetFeedDetails(string feedUrl)
        {
            INewsFeed f;
            feedsTable.TryGetValue(feedUrl, out f);
            return f as IFeedDetails; 
        } 

         /// <summary>
        /// Retrieves items from local cache. 
        /// </summary>
        /// <param name="feedUrl"></param>
        /// <returns>A List of NewsItem objects</returns>
        public override IList<INewsItem> GetCachedItemsForFeed(string feedUrl)
        {
            return this.GetItemsForFeed(feedUrl, false); 
        }

        /// <summary>
        /// Marks all items stored in the internal cache of RSS items as read
        /// for a particular feed.
        /// </summary>
        /// <param name="feed">The RSS feed</param>
        public override void MarkAllCachedItemsAsRead(INewsFeed feed)
        {
            WindowsRssNewsFeed f = feed as WindowsRssNewsFeed;
            this.DetachEventHandlers();
            if (f != null && !string.IsNullOrEmpty(f.link))
            {
                f.MarkAllItemsAsRead();
            }
            //Thread.Sleep(1000); /* give events time to finish firing */ 
            this.AttachEventHandlers();

        }
                    

        /// <summary>
        /// Retrieves the RSS feed for a particular subscription then converts 
        /// the blog posts or articles to an list of items. 
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to download</param>
        /// <param name="force_download">Flag indicates whether cached feed items 
        /// can be returned or whether the application must fetch resources from 
        /// the web</param>
        /// <exception cref="ApplicationException">If the RSS feed is not 
        /// version 0.91, 1.0 or 2.0</exception>
        /// <exception cref="XmlException">If an error occured parsing the 
        /// RSS feed</exception>
        /// <exception cref="WebException">If an error occurs while attempting to download from the URL</exception>
        /// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
        /// <returns>An list of News items (i.e. instances of the NewsItem class)</returns>		
        //	[MethodImpl(MethodImplOptions.Synchronized)]
        public override IList<INewsItem> GetItemsForFeed(string feedUrl, bool force_download)
        {          
            //We need a reference to the feed so we can see if a cached object exists
        	INewsFeed f;
            feedsTable.TryGetValue(feedUrl, out f);                

            if (f == null) // not anymore in feedTable
                return EmptyItemList;
        	
			WindowsRssNewsFeed theFeed = f as WindowsRssNewsFeed;

			if (theFeed != null)
        	try
            {
                if (force_download)
                {
                    theFeed.RefreshFeed();
                }

                return theFeed.ItemsList;
            }
            catch (Exception ex)
            {
                Trace("Error retrieving feed '{0}' from cache: {1}", feedUrl, ex.ToDescriptiveString());
            }

            return EmptyItemList; 
        }

        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The category to add</param>
        public override INewsFeedCategory AddCategory(INewsFeedCategory cat)
        {
            if (cat is WindowsRssNewsFeedCategory)
            {
                if (!categories.ContainsKey(cat.Value))
                {
                    categories.Add(cat.Value, cat);
                }
            }
            else
            {
                if (!categories.ContainsKey(cat.Value))
                {
                    IFeedFolder folder = this.AddFolder(cat.Value);
                    cat = new WindowsRssNewsFeedCategory(folder, cat);
                    this.categories.Add(cat.Value, cat);
                }
                else
                {
                    cat = categories[cat.Value];
                }

            }
            
            return cat;
        }

        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The name of the category</param>
        public override INewsFeedCategory AddCategory(string cat)
        {
            INewsFeedCategory toReturn; 

            if (!this.categories.ContainsKey(cat))
            {                
                    IFeedFolder folder = this.AddFolder(cat);
                    toReturn = new WindowsRssNewsFeedCategory(folder);
                    this.categories.Add(cat, toReturn);
            }else{ 
                toReturn = categories[cat];
            }

            return toReturn;
        }



        /// <summary>
        /// Changes the category of a particular INewsFeedCategory. This method should be when moving a category. Also 
        /// changes the category of call child feeds and categories.   
        /// </summary>        
        /// <param name="cat">The category whose parent category to change</param>
        /// <param name="parent">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized. If this parameter is null then the parent is considered to be the root node.</param>
        public override void ChangeCategory(INewsFeedCategory cat, INewsFeedCategory parent)
        {
            this.ChangeCategory(cat, parent, true); 
        }

            /// <summary>
        /// Changes the category of a particular INewsFeedCategory. This method should be when moving a category. Also 
        /// changes the category of call child feeds and categories.   
        /// </summary>        
        /// <param name="category">The category whose parent category to change</param>
        /// <param name="parent">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized. If this parameter is null then the parent is considered to be the root node.</param>
        /// <param name="moveFolder">Indicates whether the underlying folder in the Windows RSS platform should be moved.</param>
        private void ChangeCategory(INewsFeedCategory category, INewsFeedCategory parent, bool moveFolder)
        {
			category.ExceptionIfNull("category");
            
            WindowsRssNewsFeedCategory c = category as WindowsRssNewsFeedCategory;

            if (c != null)
            {
                string parentPath = parent == null ? String.Empty : parent.Value;
                string originalCategory = c.Value;
                int index = originalCategory.LastIndexOf(FeedSource.CategorySeparator);
                index = (index == -1 ? 0 : index + 1);

                List<string> oldCategories = new List<string>();
                oldCategories.AddRange(from oc in this.GetDescendantCategories(c) select oc.Value);

                if (moveFolder)
                {
                    IFeedFolder folder2move = feedManager.GetFolder(c.Value) as IFeedFolder;
                    folder2move.Move(parentPath);
                    WindowsRssFeedSource.folder_moved_event_caused_by_rssbandit = true;
                    c.SetIFeedFolder(folder2move);
                }

                /* fix up IFeedFolder references */
                foreach (string str in oldCategories)
                {
                    WindowsRssNewsFeedCategory movedCat = this.categories[str] as WindowsRssNewsFeedCategory;
                    string newName = parentPath + (parentPath.Equals(String.Empty) ? String.Empty : FeedSource.CategorySeparator)
                                     + str.Substring(index);
                    movedCat.SetIFeedFolder(feedManager.GetFolder(newName) as IFeedFolder);
                    this.categories.Remove(str);
                    this.categories.Add(newName, movedCat);
                }

                /* fix up all IFeed references */
                oldCategories.Add(originalCategory);

                var feeds2update = from f in this.feedsTable.Values
                                   where oldCategories.Contains(f.category ?? String.Empty)
                                   select f;

                if (feeds2update.Count() > 0)
                {
                    foreach (WindowsRssNewsFeed feed in feeds2update)
                    {
                        IFeed ifeed = feedManager.GetFeedByUrl(feed.link) as IFeed;
                        feed.SetIFeed(ifeed);
                    }//foreach(INewsFeed...)
                }//if(feeds2update...)

                this.categories.Remove(originalCategory);
                this.categories.Add(c.Value, c);
                this.readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);

            }//if(c!= null)

        }

          

        /// <summary>
        /// Changes the category of a particular INewsFeed. This method should be used instead of setting
        /// the category property of the INewsFeed instance. 
        /// </summary>
        /// <param name="feed">The newsfeed whose category to change</param>
        /// <param name="cat">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized</param>
        public override void ChangeCategory(INewsFeed feed, INewsFeedCategory cat)
        {
			feed.ExceptionIfNull("feed");
        
            WindowsRssNewsFeed f = feed as WindowsRssNewsFeed; 

            if (f != null && feedsTable.ContainsKey(f.link))
            {
                IFeedFolder folder = cat == null ? feedManager.RootFolder as IFeedFolder 
                                                                      : feedManager.GetFolder(cat.Value) as IFeedFolder;
                IFeed ifeed = feedManager.GetFeedByUrl(f.link) as IFeed;

                if (!folder.Path.Equals(((IFeedFolder)(ifeed.Parent)).Path) ) 
                {
                    ifeed.Move(folder.Path);
                    f.SetIFeed(ifeed); 
                    WindowsRssFeedSource.event_caused_by_rssbandit = true;
                }
            }
        }

        /// <summary>
        /// Renames the specified category
        /// </summary>        
        /// <param name="oldName">The old name of the category</param>
        /// <param name="newName">The new name of the category</param>        
        public override void RenameCategory(string oldName, string newName)
        {
			oldName.ExceptionIfNullOrEmpty("oldName");
			newName.ExceptionIfNullOrEmpty("newName");

            if (this.categories.ContainsKey(oldName))
            {
                WindowsRssNewsFeedCategory c = this.categories[oldName] as WindowsRssNewsFeedCategory;
                List<string> oldCategories = new List<string>();
                oldCategories.AddRange(from oc in this.GetDescendantCategories(c) select oc.Value);

                int o_index = oldName.LastIndexOf(FeedSource.CategorySeparator);
                o_index = (o_index == -1 ? 0 : o_index + 1);
               
                IFeedFolder folder = feedManager.GetFolder(oldName) as IFeedFolder;

                if (folder != null)
                {
                    int n_index = newName.LastIndexOf(CategorySeparator) == -1 ?
                        0 : newName.LastIndexOf(CategorySeparator) + 1;
                    folder.Rename(newName.Substring(n_index));
                    folder_renamed_event_caused_by_rssbandit = true;
                    c.SetIFeedFolder(folder);

                    this.categories.Remove(oldName);                     
                    categories.Add(newName, c);

                    /* fix up IFeedFolder references */
                    foreach (string str in oldCategories)
                    {
                        WindowsRssNewsFeedCategory movedCat = this.categories[str] as WindowsRssNewsFeedCategory;
                        string name = newName + CategorySeparator + str.Substring(oldName.Length + 1);
                        movedCat.SetIFeedFolder(feedManager.GetFolder(name) as IFeedFolder);
                        this.categories.Remove(str);
                        this.categories.Add(name, movedCat);
                    }

                    /* fix up all IFeed references */
                    oldCategories.Add(oldName);

                    var feeds2update = from f in this.feedsTable.Values
                                       where oldCategories.Contains(f.category ?? String.Empty)
                                       select f;

                    if (feeds2update.Count() > 0)
                    {
                        foreach (WindowsRssNewsFeed feed in feeds2update)
                        {
                            IFeed ifeed = feedManager.GetFeedByUrl(feed.link) as IFeed;
                            feed.SetIFeed(ifeed);
                        }//foreach(INewsFeed...)
                    }//if(feeds2update...)

                 
                    this.readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);                    
                }//if (folder != null)
            }// if (this.categories.ContainsKey(oldName))         
                         
        }

        /// <summary>
        /// Changes the URL of the specified feed if it is contained in this feed source
        /// </summary>
        /// <param name="feed">The feed whose URL is being changed</param>
        /// <param name="newUrl">The new URL for the feed</param>
        /// <returns>The feed with the changed URL</returns>
        public override INewsFeed ChangeFeedUrl(INewsFeed feed, string newUrl)
        {
            if (feed != null && this.feedsTable.ContainsKey(feed.link))
            {
                this.DeleteFeed(feed.link);

                feed = new NewsFeed(feed);
                feed.link = newUrl;
                feed = this.AddFeed(feed);
            }

            return feed;
        }

        /// <summary>
        /// Adds a feed and associated FeedInfo object to the FeedsTable and itemsTable. 
        /// Any existing feed objects are replaced by the new objects. 
        /// </summary>
        /// <param name="feed">The NewsFeed object </param>
        /// <param name="feedInfo">The FeedInfo object</param>
        public override INewsFeed AddFeed(INewsFeed feed, FeedInfo feedInfo)
        {
            lock (WindowsRssFeedSource.event_caused_by_rssbandit_syncroot)
            {

                if (feed is WindowsRssNewsFeed)
                {
                    if (!feedsTable.ContainsKey(feed.link))
                    {
                        feedsTable.Add(feed.link, feed);
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(feed.category) && !feedManager.ExistsFolder(feed.category))
                    {
                        this.AddCategory(feed.category);
                    }

                    IFeedFolder folder = String.IsNullOrEmpty(feed.category) ? feedManager.RootFolder as IFeedFolder:
                        feedManager.GetFolder(feed.category) as IFeedFolder;
                    IFeed newFeed = folder.CreateFeed(feed.title, feed.link) as IFeed;
                    feed = new WindowsRssNewsFeed(newFeed, feed, this);
                    feedsTable.Add(feed.link, feed);
                }

                //handle case where refresh rate is 0 for all feeds
                if (feedManager.DefaultInterval == Int32.MaxValue)
                {
                    ((WindowsRssNewsFeed)feed).DisableFeedPolling(); 
                }

                WindowsRssFeedSource.event_caused_by_rssbandit = true;
            }
            return feed;
        }

        /// <summary>
        /// Deletes all subscribed feeds and categories 
        /// </summary>
        public override void DeleteAllFeedsAndCategories(bool deleteFromSource)
        {
            if (deleteFromSource)
            {
                string[] keys = new string[categories.Count];
                this.categories.Keys.CopyTo(keys, 0);
                foreach (string categoryName in keys)
                {
                    this.DeleteCategory(categoryName);
                }

                keys = new string[feedsTable.Count];
                this.feedsTable.Keys.CopyTo(keys, 0);
                foreach (string feedUrl in keys)
                {
                    this.DeleteFeed(feedUrl);
                }
            }

                base.DeleteAllFeedsAndCategories(deleteFromSource);            
          
        }

        /// <summary>
        /// Removes all information related to a feed from the FeedSource.   
        /// </summary>
        /// <remarks>If no feed with that URL exists then nothing is done.</remarks>
        /// <param name="feedUrl">The URL of the feed to delete. </param>
        /// <exception cref="ApplicationException">If an error occured while 
        /// attempting to delete the cached feed. Examine the InnerException property 
        /// for details</exception>
        public override void DeleteFeed(string feedUrl)
        {
            if (feedsTable.ContainsKey(feedUrl))
            {
                WindowsRssNewsFeed f = feedsTable[feedUrl] as WindowsRssNewsFeed;
                this.feedsTable.Remove(f.link);
                try
                {
                    IFeed feed = feedManager.GetFeedByUrl(feedUrl) as IFeed;

                    if (feed != null)
                    {
                        feed.Delete();
                    }
                }
                catch(Exception e)
                {
                    _log.Debug("Exception on deleting feed: " + feedUrl, e); 
                }
            }
        }

        /// <summary>
        /// Deletes a category from the Categories collection. 
        /// </summary>
        /// <remarks>This also deletes the corresponding folder from the underlying Windows RSS platform.</remarks>
        /// <param name="cat"></param>
        public override void DeleteCategory(string cat)
        {
            base.DeleteCategory(cat);

            IFeedFolder folder = feedManager.GetFolder(cat) as IFeedFolder;
            if (folder != null)
            {
                folder.Delete();
            }
        }

      


        /// <summary>
        /// Used to recursively load a folder and its children (feeds and subfolders) into the FeedsTable and Categories collections. 
        /// </summary>
        /// <param name="folder2load">The folder to load</param>
        /// <param name="bootstrapFeeds">The RSS Bandit metadata about the feeds being loaded</param>
        /// <param name="bootstrapCategories">The RSS Bandit metadata about the folders/categories being loaded</param>
        private void LoadFolder(IFeedFolder folder2load, IDictionary<string, NewsFeed> bootstrapFeeds, Dictionary<string, INewsFeedCategory> bootstrapCategories)
        {

            if (folder2load != null)
            {                
                IFeedsEnum Feeds = folder2load.Feeds as IFeedsEnum;
                IFeedsEnum Subfolders = folder2load.Subfolders as IFeedsEnum;

                if (Feeds.Count > 0)
                {
                    foreach (IFeed feed in Feeds)
                    {
                        Uri uri;
						bool isBadUrl; 

                        try
                        {
                           isBadUrl =  !Uri.TryCreate(feed.DownloadUrl, UriKind.Absolute, out uri);
                        }
                        catch (COMException) //DownloadUrl errors if the feed has never been downloaded
                        {
                           isBadUrl = !Uri.TryCreate(feed.Url, UriKind.Absolute, out uri);                         
                        }

                        if(isBadUrl) continue; 

                        string feedUrl = uri.CanonicalizedUri();
                        NewsFeed bootstrapFeed;
						bootstrapFeeds.TryGetValue(feedUrl, out bootstrapFeed);

                        if (this.feedsTable.ContainsKey(feedUrl)) //duplicate subscription
                        {
                            try { 
                                feed.Delete(); 
                            }catch (Exception e){
                                _log.Debug("Exception deleting duplicate feed " + feedUrl, e);
                            }
                        }
                        else
                        {
                            this.feedsTable.Add(feedUrl, new WindowsRssNewsFeed(feed, bootstrapFeed, this));
                        }
                    }//foreach(IFeed feed in ...)
                }

                if (Subfolders.Count > 0)
                {
                    foreach (IFeedFolder folder in Subfolders)
                    {
                        string categoryName = folder.Path;
                        INewsFeedCategory bootstrapCategory = (bootstrapCategories.ContainsKey(categoryName) ? bootstrapCategories[categoryName] : null);
                        this.categories.Add(folder.Path, new WindowsRssNewsFeedCategory(folder, bootstrapCategory));
                        LoadFolder(folder, bootstrapFeeds, bootstrapCategories);  
                    }
                }
            }

        }

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
            this.BootstrapAndLoadFeedlist(new feeds());
        }

        /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this FeedSource</param>
        public override void BootstrapAndLoadFeedlist(feeds feedlist)
        {
            Dictionary<string, NewsFeed> bootstrapFeeds = new Dictionary<string, NewsFeed>();
            Dictionary<string, INewsFeedCategory> bootstrapCategories = new Dictionary<string, INewsFeedCategory>();

            foreach (NewsFeed f in feedlist.feed) 
            {
                bootstrapFeeds.Add(f.link, f); 
            }

            foreach (category c in feedlist.categories) 
            {
                bootstrapCategories.Add(c.Value, c);   
            }

            IFeedFolder root = feedManager.RootFolder as IFeedFolder;
            LoadFolder(root, bootstrapFeeds, bootstrapCategories);

            /* copy over list view layouts */
			//if (feedlist.listviewLayouts != null)
			//{
			//    foreach (listviewLayout layout in feedlist.listviewLayouts)
			//    {
			//        string layout_trimmed = layout.ID.Trim();
			//        if (!this.layouts.ContainsKey(layout_trimmed))
			//        {
			//            this.layouts.Add(layout_trimmed, layout.FeedColumnLayout);
			//        }
			//    }
			//}             
        }


        /// <summary>
        /// Retrieves the RSS feed for a particular subscription then converts 
        /// the blog posts or articles to an arraylist of items. The http requests are async calls.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to download</param>
        /// <param name="forceDownload">Flag indicates whether cached feed items 
        /// can be returned or whether the application must fetch resources from 
        /// the web</param>
        /// <param name="manual">Flag indicates whether the call was initiated by user (true), or
        /// by automatic refresh timer (false)</param>
        /// <exception cref="ApplicationException">If the RSS feed is not version 0.91, 1.0 or 2.0</exception>
        /// <exception cref="XmlException">If an error occured parsing the RSS feed</exception>
        /// <exception cref="ArgumentNullException">If feedUrl is a null reference</exception>
        /// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
        /// <returns>true, if the request really was queued up</returns>
        /// <remarks>Result arraylist is returned by OnUpdatedFeed event within UpdatedFeedEventArgs</remarks>		
        //	[MethodImpl(MethodImplOptions.Synchronized)]
        public override bool AsyncGetItemsForFeed(string feedUrl, bool forceDownload, bool manual)
        {
            if (feedUrl == null || feedUrl.Trim().Length == 0)
                throw new ArgumentNullException("feedUrl");

            INewsFeed f = null;
            feedsTable.TryGetValue(feedUrl, out f);
            WindowsRssNewsFeed f2 = f as WindowsRssNewsFeed;

            if (f2 != null)
            {                
                f2.RefreshFeed(true); 
            }

            return true;
        }

        /// <summary>
        /// Downloads every feed that has either never been downloaded before or 
        /// whose elapsed time since last download indicates a fresh attempt should be made. 
        /// </summary>
        /// <param name="force_download">A flag that indicates whether download attempts should be made 
        /// or whether the cache can be used.</param>
        /// <remarks>This method uses the cache friendly If-None-Match and If-modified-Since
        /// HTTP headers when downloading feeds.</remarks>	
        public override void RefreshFeeds(bool force_download)
        {
            GetFeedsTableKeys();
            this.feedManager.AsyncSyncAll();            
            this.RaiseOnAllAsyncRequestsCompleted();
        }

        /// <summary>
        /// Downloads every feed that has either never been downloaded before or 
        /// whose elapsed time since last download indicates a fresh attempt should be made. 
        /// </summary>
        /// <param name="category">Refresh all feeds, that are part of the category</param>
        /// <param name="force_download">A flag that indicates whether download attempts should be made 
        /// or whether the cache can be used.</param>
        /// <remarks>This method uses the cache friendly If-None-Match and If-modified-Since
        /// HTTP headers when downloading feeds.</remarks>	
        public override void RefreshFeeds(string category, bool force_download)
        {
            //RaiseOnUpdateFeedsStarted(force_download);
            string[] keys = GetFeedsTableKeys();

            for (int i = 0, len = keys.Length; i < len; i++)
            {
                if (!feedsTable.ContainsKey(keys[i])) // may have been redirected/removed meanwhile
                    continue;

                WindowsRssNewsFeed current = feedsTable[keys[i]] as WindowsRssNewsFeed;

				if (current != null && current.category != null && IsChildOrSameCategory(category, current.category))
                {
                    current.RefreshFeed(true);
                }

                Thread.Sleep(15); // force a context switches
            } //for(i)

        }

        #endregion

        #region IFeedFolderEvents implementation

        /// <summary>
        /// Occurs when a feed event error occurs.
        /// </summary>
        /// <remarks>The advice in documentation for when this happens is that the application must assume that some events have 
        /// not been raised, and should recover by rereading the feed subscription list as if running for the first time. 
        /// </remarks>
        public void Error()
        {
            throw new WindowsRssPlatformException("Windows RSS platform has raised an error. Please reload the Windows RSS feed list"); 
        }

        /// <summary>
        /// A subfolder was added.
        /// </summary>
        /// <param name="Path">The path to the folder</param>
        public void FolderAdded(string Path)
        {
            this.categories.Add(Path, new WindowsRssNewsFeedCategory(feedManager.GetFolder(Path) as IFeedFolder));
            this.readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);
            RaiseOnAddedCategory(new CategoryEventArgs(Path));
        }

        /// <summary>
        /// A subfolder was added.
        /// </summary>
        /// <param name="Path">The path to the folder</param>
        public void FolderDeleted(string Path)
        {
            this.categories.Remove(Path);
            this.readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);
            RaiseOnDeletedCategory(new CategoryEventArgs(Path));
        }

        /// <summary>
        /// A subfolder was moved from this folder to another folder.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="oldPath"></param>
        public void FolderMovedFrom(string Path, string oldPath)
        {
            Console.WriteLine("Folder moved from {0} to {1}", oldPath, Path); 
         
         /* Do nothing since we get the same event repeated in FolderMoveTo */  
        }
        
        /// <summary>
        /// A subfolder was moved into this folder.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="oldPath"></param>
        public void FolderMovedTo(string Path, string oldPath)
        {
            lock (WindowsRssFeedSource.folder_moved_event_caused_by_rssbandit_syncroot)
            {
                if (WindowsRssFeedSource.folder_moved_event_caused_by_rssbandit)
                {
                    WindowsRssFeedSource.folder_moved_event_caused_by_rssbandit = false;
                    return;
                }
            }


            INewsFeedCategory cat = this.categories[oldPath];
            category c = new category(Path);
            this.ChangeCategory(cat, c, false); 
         
            RaiseOnMovedCategory(new CategoryChangedEventArgs(oldPath, Path));
        }


        /// <summary>
        /// A subfolder was renamed.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="oldPath"></param>
        public void FolderRenamed(string Path, string oldPath)
        {
            lock (WindowsRssFeedSource.folder_renamed_event_caused_by_rssbandit_syncroot)
            {
                if (WindowsRssFeedSource.folder_renamed_event_caused_by_rssbandit)
                {
                    WindowsRssFeedSource.folder_renamed_event_caused_by_rssbandit = false;
                    return;
                }
            }

            INewsFeedCategory cat = this.categories[oldPath];
            this.categories.Remove(oldPath);
            this.categories.Add(Path, new WindowsRssNewsFeedCategory(feedManager.GetFolder(Path) as IFeedFolder, cat));
            this.readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);

            RaiseOnRenamedCategory(new CategoryChangedEventArgs(oldPath, Path));
        }

        /// <summary>
        /// Occurs when the aggregated item count of a feed folder changes.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="itemCountType"></param>
        public void FolderItemCountChanged(string Path, int itemCountType)
        {
            /* Do nothing since we also get events from FeedItemCountChanged */ 
        }

        /// <summary>
        /// Occurs when a feed is added to the folder.
        /// </summary>
        /// <param name="Path"></param>
        public void FeedAdded(string Path)
        {
            lock (event_caused_by_rssbandit_syncroot)
            {
                if (event_caused_by_rssbandit)
                {
                    event_caused_by_rssbandit = false;
                    return; 
                }
            }

            IFeed ifeed = feedManager.GetFeed(Path) as IFeed;
            this.feedsTable.Add(ifeed.DownloadUrl, new WindowsRssNewsFeed(ifeed));
            this.readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(this.feedsTable);

            //handle case where refresh rate is 0 for all feeds
            if (feedManager.DefaultInterval == Int32.MaxValue)
            {
                ifeed.SyncSetting = FEEDS_SYNC_SETTING.FSS_MANUAL;
            }

            RaiseOnAddedFeed(new FeedChangedEventArgs(ifeed.DownloadUrl));
        }

        /// <summary>
        /// A feed was deleted.
        /// </summary>
        /// <param name="Path"></param>
        public void FeedDeleted(string Path)
        {
            lock (event_caused_by_rssbandit_syncroot)
            {
                if (event_caused_by_rssbandit)
                {
                    event_caused_by_rssbandit = false;
                    return;
                }
            }

            int index = Path.LastIndexOf(CategorySeparator);
            string categoryName = null, title;

            if (index == -1)
            {
                title = Path;
            }
            else
            {
                categoryName = Path.Substring(0, index);
                title = Path.Substring(index + 1);
            }

            string[] keys = GetFeedsTableKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                INewsFeed f;
                feedsTable.TryGetValue(keys[i], out f);

                if (f != null)
                {
                    if (f.title.Equals(title) && (Equals(f.category, categoryName)))
                    {
                        this.feedsTable.Remove(f.link); 
                        this.readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(this.feedsTable);

                        RaiseOnDeletedFeed(new FeedDeletedEventArgs(f.link, f.title));
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// A feed was renamed.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="oldPath"></param>
        public void FeedRenamed(string Path, string oldPath)
        {
            lock (event_caused_by_rssbandit_syncroot)
            {
                if (event_caused_by_rssbandit)
                {
                    event_caused_by_rssbandit = false;
                    return;
                }
            }

            int index = oldPath.LastIndexOf(CategorySeparator);
            string categoryName = null, title;

            if (index == -1)
            {
                title = oldPath;
            }
            else
            {
                categoryName = oldPath.Substring(0, index);
                title = oldPath.Substring(index + 1);
            }

            string[] keys = GetFeedsTableKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                INewsFeed f;
                feedsTable.TryGetValue(keys[i], out f);

                if (f != null)
                {
                    if (f.title.Equals(title) && (Equals(f.category, categoryName)))
                    {
                        index = Path.LastIndexOf(CategorySeparator);
                        string newTitle = (index == -1 ? Path : Path.Substring(index + 1));

                        RaiseOnRenamedFeed(new FeedRenamedEventArgs(f.link, newTitle));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// A feed was moved from this folder to another folder.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="oldPath"></param>
        public void FeedMovedFrom(string Path, string oldPath)
        {
            Console.WriteLine("Feed moved from {0} to {1}", oldPath, Path); 
            /* Do nothing since we get the same event repeated in FeedMoveTo */
        }

        /// <summary>
        /// A feed was moved to this folder.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="oldPath"></param>
        public void FeedMovedTo(string Path, string oldPath)
        {
            lock (event_caused_by_rssbandit_syncroot)
            {
                if (event_caused_by_rssbandit)
                {
                    event_caused_by_rssbandit = false;
                    return;
                }
            }
            
            int index = oldPath.LastIndexOf(CategorySeparator);
            string categoryName = null, title;

            if (index == -1)
            {
                title = oldPath;
            }
            else
            {
                categoryName = oldPath.Substring(0, index);
                title = oldPath.Substring(index + 1);
            }

            string[] keys = GetFeedsTableKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                INewsFeed f;
                feedsTable.TryGetValue(keys[i], out f);

                if (f != null)
                {
                    if (f.title.Equals(title) && (Equals(f.category, categoryName)))
                    {
                        //we need to get the new IFeed instance for the moved feed
                        ((WindowsRssNewsFeed)f).SetIFeed(feedManager.GetFeed(Path) as IFeed); 

                        RaiseOnMovedFeed(new FeedMovedEventArgs(f.link, f.category));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// The URL of a feed changed.
        /// </summary>
        /// <param name="Path"></param>
        public void FeedUrlChanged(string Path)
        {
            lock (event_caused_by_rssbandit_syncroot)
            {
                if (event_caused_by_rssbandit)
                {
                    event_caused_by_rssbandit = false;
                    return;
                }
            }

            IFeed ifeed = feedManager.GetFeed(Path) as IFeed;
            int index = Path.LastIndexOf(CategorySeparator);
            string categoryName = null, title;

            if (index == -1)
            {
                title = Path;
            }
            else
            {
                categoryName = Path.Substring(0, index);
                title = Path.Substring(index + 1);
            }

            string[] keys = GetFeedsTableKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                INewsFeed f;
                feedsTable.TryGetValue(keys[i], out f);

                if (f != null)
                {
                    if (f.title.Equals(title) && (Equals(f.category, categoryName)))
                    {
                        Uri requestUri = new Uri(f.link);
                        Uri newUri = new Uri(ifeed.DownloadUrl);
                        RaiseOnUpdatedFeed(requestUri, newUri, RequestResult.NotModified, 1110, false);
                        break;
                    }
                }
            }//for

        }


        /// <summary>
        /// The number of items or unread items in a feed changed.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="itemCountType"></param>
        public void FeedItemCountChanged(string Path, int itemCountType)
        {
            //Do nothing because we can't distinguish events caused by our application (e.g. marking lots of items as read) 
            //from events created by changing read state in another application like Outlook or IE. So instead we will ONLY
            //sync read state on successful feed download events. 

            /* 
            lock (WindowsRssFeedSource.event_caused_by_rssbandit_syncroot)
            {
                if (WindowsRssFeedSource.event_caused_by_rssbandit)
                {
                    WindowsRssFeedSource.event_caused_by_rssbandit = false;
                    return;
                }
            }

            IFeed ifeed = feedManager.GetFeed(Path) as IFeed;
            Uri requestUri = new Uri(ifeed.DownloadUrl);
            RaiseOnUpdatedFeed(requestUri, null, RequestResult.OK, 1110, false);
             */ 
        }


        /// <summary>
        /// A feed has started downloading.
        /// </summary>
        /// <param name="Path"></param>
        public void FeedDownloading(string Path)
        {
            lock (event_caused_by_rssbandit_syncroot)
            {
                if (event_caused_by_rssbandit)
                {
                    event_caused_by_rssbandit = false;
                    return;
                }
            }

            IFeed ifeed = feedManager.GetFeed(Path) as IFeed;
            Uri requestUri = new Uri(ifeed.DownloadUrl);
            bool cancel = false; 
            RaiseBeforeDownloadFeedStarted(requestUri, ref cancel);
            if (cancel)
            {
                ifeed.CancelAsyncDownload(); 
            }
        }       
         

        /// <summary>
        /// A feed has completed downloading (success or error).
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Error"></param>
        public void FeedDownloadCompleted(string Path, FEEDS_DOWNLOAD_ERROR Error)
        {
            lock (event_caused_by_rssbandit_syncroot)
            {
                if (event_caused_by_rssbandit)
                {
                    event_caused_by_rssbandit = false;
                    return;
                }
            }

			IFeed ifeed = (IFeed)feedManager.GetFeed(Path);
            Uri requestUri = new Uri(ifeed.DownloadUrl);

            if (Error == FEEDS_DOWNLOAD_ERROR.FDE_NONE)
            {
                RaiseOnUpdatedFeed(requestUri, null, RequestResult.OK, 1110, false);
            }
            else
            {
                INewsFeed f;
                feedsTable.TryGetValue(ifeed.DownloadUrl, out f);
                WindowsRssNewsFeed wf = f as WindowsRssNewsFeed;

                if (wf == null)
                {
                    Exception e = new FeedRequestException(Error.ToString(), new WebException(Error.ToString()), GetFailureContext(wf, wf));
                    RaiseOnUpdateFeedException(ifeed.DownloadUrl, e, 1100);
                }

            }
        }

        #endregion

        #region IDisposable pattern

       /// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DetachEventHandlers(); //we don't want to get events from Windows RSS platform once object is no longer needed
			}
		}    
         
        #endregion 
    }

    #endregion

    #region WindowsRssNewsItem


    /// <summary>
    /// Represents a NewsItem obtained from the Windows RSS platform
    /// </summary>
    public class WindowsRssNewsItem : INewsItem, IDisposable
    {

        #region constructors 

        /// <summary>
        /// We always want an associated IFeedItem instance
        /// </summary>
        private WindowsRssNewsItem() { ;}

		/// <summary>
		/// Initializes the class
		/// </summary>
		/// <param name="item">The IFeedItem instance that this object will wrap</param>
		/// <param name="owner">The owner.</param>
        internal WindowsRssNewsItem(IFeedItem item, WindowsRssNewsFeed owner)
        {
            if (item == null) throw new ArgumentNullException("item"); 
            this.myitem = item;
            this.myfeed = owner;
            /* do this here because COM interop is too slow to check it each time property is accessed */
            this._id = String.IsNullOrEmpty(myitem.Guid)
                ? (String.IsNullOrEmpty(myitem.Link) ? myitem.Title.GetHashCode().ToString() : myitem.Link)
                : myitem.Guid; 
            this._beenRead = myitem.IsRead;

            //TODO: RelationCosmos and outgoing links processing? 
        }


        #endregion 

        #region private fields

        /// <summary>
        /// Indicates that the object has been disposed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The actual IFeedItem instance that this object is wrapping
        /// </summary>
        private IFeedItem myitem;

        /// <summary>
        /// The INewsFeed instance which this item belongs to
        /// </summary>
        private WindowsRssNewsFeed myfeed;

        /// <summary>
        /// Used for logging. 
        /// </summary>
        private static readonly ILog _log = Log.GetLogger(typeof(WindowsRssNewsItem));

        /// <summary>
        /// This is the default DateTime used by the Windows RSS platform to indicate that no pubdate was found on the item. 
        /// </summary>
        private static DateTime NoPubDate = new DateTime(1899, 12, 30); 

        #endregion 

          #region destructor and IDisposable implementation 

        /// <summary>
        /// Releases the associated COM objects
        /// </summary>
        /// <seealso cref="myitem"/>
        ~WindowsRssNewsItem() {
            Dispose(false);           
        }

        /// <summary>
        /// Disposes of the class
        /// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        /// <summary>
        /// Disposes of the class
        /// </summary>
        /// <param name="disposing"></param>
		public void Dispose(bool disposing)
		{
			if (!disposed)
				lock (this)
				{
					if (!disposed && myitem != null)
					{
						Marshal.ReleaseComObject(myitem);
						myitem = null;
					}

					disposed = true;
				}
		}

        #endregion


        #region INewsItem implementation 

        /// <summary>
        /// Container of enclosures on the item. If there are no enclosures on the item
        /// then this value is null. 
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public List<IEnclosure> Enclosures
        {
            get
            {
            	if (myitem.Enclosure != null)
                {
                    IEnclosure enc = new WindowsRssEnclosure(myitem.Enclosure as IFeedEnclosure);
                    return new List<IEnclosure>() { enc };
                }
            	return GetList<IEnclosure>.Empty;
            }
        	set
            {
                /* Can't set IFeedItem.Enclosure */ 
            }
        }

        private bool p_hasNewComments;
        /// <summary>
        /// Indicates that there are new comments to this item. 
        /// </summary>
        public bool HasNewComments
        {
            get
            {
                return p_hasNewComments;
            }
            set
            {
                p_hasNewComments = value;
            }
        }

        private bool p_watchComments;
        /// <summary>
        /// Indicates that comments to this item are being watched. 
        /// </summary>
        public bool WatchComments
        {
            get
            {
                return p_watchComments;
            }
            set
            {
                p_watchComments = value;
            }
        }

        private Flagged p_flagStatus = Flagged.None;
        /// <summary>
        /// Indicates whether the item has been flagged for follow up or not. 
        /// </summary>
        public Flagged FlagStatus
        {
            get
            {
                return p_flagStatus;
            }
            set
            {
                p_flagStatus = value;
            }
        }

        /// <summary>
        /// Indicates whether this item supports posting comments. 
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public SupportedCommentStyle CommentStyle
        {
            get
            {
                return SupportedCommentStyle.None;
            }
            set
            {
                /* do nothing */ 
            }
        }

         /// <summary>
        /// Gets or sets the language of the entry.
        /// Format of the corresponfing attribute as defined in
        /// http://www.w3.org/TR/REC-xml/#sec-lang-tag;
        /// Format of the language string: 
        /// see http://www.ietf.org/rfc/rfc3066.txt
        /// </summary>
        /// <value>The language.</value>
        public string Language
        {
            get { return myfeed.Language; }
        }

        /// <summary>
        /// Gets the feed link (source the feed is requested from) the item belongs to.
        /// </summary>
        public string FeedLink
        {
            get { return (myitem.Parent as IFeed).DownloadUrl; }
        }

        /// <summary>
        /// The link to the item.
        /// </summary>
        public string Link
        {
            get
            {
                try
                {
                    return myitem.Link;
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// The date the article or blog entry was made. 
        /// </summary>
        /// <remarks>This field is read only </remarks>
        public DateTime Date
        {
            get
            {
                try
                {
                    return myitem.PubDate.Equals(NoPubDate) ? myitem.LastDownloadTime : myitem.PubDate;
                }
                catch (Exception e) /* thrown if Windows RSS platform can't parse the date */
                {
                    _log.Error("Exception in WindowsRssNewsItem.Date on attempting to read IFeedItem.PubDate:", e); 
                    return DateTime.Now;                    
                }
            }
            set { /* can't set IFeedItem.PubDate */ }
        }

        private string _id; 
        /// <summary>
        /// The unique identifier.
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public string Id
        {
            get { return _id; }
            set { /* Can't set IFeedItem.LocalId */ }
        }

        /// <summary>
        /// The unique identifier of the parent.
        /// </summary>
        public string ParentId
        {
            get { return null; }
        }

        /// <summary>
        /// The content of the article or blog entry. 
        /// </summary>
        public string Content
        {
            get { return myitem.Description; }
        }

        /// <summary>
        /// Returns true, if Content contains something; else false.
        /// </summary>
        /// <remarks>Should be used instead of testing 
        /// (Content != null &amp;&amp; Content.Length > 0) and is equivalent to 
        /// .ContentType == ContentType.None
        /// </remarks>
        public bool HasContent
        { 
            get { return true; } 
        }

        /// <summary>
        /// Set new content of the article or blog entry.
        /// </summary>
        /// <remarks>WARNING: This method does nothing.</remarks>
        /// <param name="newContent">string</param>
        /// <param name="contentType">ContentType</param>
        public void SetContent(string newContent, ContentType contentType)
        {
            /* Can't set IFeedItem.Description */ 
        }

        /// <summary>
        /// Indicates whether the description on this feed is text, HTML or XHTML. 
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public ContentType ContentType
        {
            get { return ContentType.Html; }
            set { } 
        }

		// hold for speed (COM interop is slow to query often...)
        private bool _beenRead;
        /// <summary>
        /// Indicates whether the story has been read or not. 
        /// </summary>
        public bool BeenRead
        {
            get 
            {
                return _beenRead;
            }
            set {
            	_beenRead = value;
                lock (WindowsRssFeedSource.event_caused_by_rssbandit_syncroot)
                {
                    WindowsRssFeedSource.event_caused_by_rssbandit = true;
					Access.Apply(myitem, delegate
					{
						myitem.IsRead = value;
					});
                }
            }
        }

        /// <summary>
        /// Returns an object implementing the FeedDetails interface to which this item belongs
        /// </summary>
        public IFeedDetails FeedDetails {
            get { return this.myfeed; }
            set
            {
                if (value is WindowsRssNewsFeed)
                    this.myfeed = value as WindowsRssNewsFeed;
            }
        }

        /// <summary>
        /// The author of the article or blog entry 
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public string Author {
            get { return this.myitem.Author; }
            set { /* Can't set IFeedItem.Author */ }
        }

        /// <summary>
        /// The title of the article or blog entry. 
        /// </summary>
        /// <remarks>This property is read only</remarks>
       public string Title { 
            get { return Access.Get(myitem, m => { return m.Title; }); }
            set { /* Can't set IFeedItem.Title */ } 
       }

        /// <summary>
        /// The subject of the article or blog entry. 
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public string Subject {
            get { return null; }
            set { /* not supported */ }
        }

        /// <summary>
        /// Returns the amount of comments attached.
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public int CommentCount
        {
            get { return 0; }
            set { /* */ }
        }

        /// <summary>the URL to post comments to</summary>
        public string CommentUrl
        {
            get{ return null;}
        }

        /// <summary>the URL to get an RSS feed of comments from</summary>
        /// <remarks>This is not exposed in the Windows RSS platform data model</remarks>
        public string CommentRssUrl {
            get { return null; }
            set { /* can't be set */ }
        }

        private Dictionary<XmlQualifiedName, string> _optionalElements = new Dictionary<XmlQualifiedName, string>(); 
        /// <summary>
        /// Container for all the optional RSS elements for an item. Also 
        /// holds information from RSS modules. The keys in the hashtable 
        /// are instances of XmlQualifiedName while the values are instances 
        /// of XmlNode. 
        /// </summary>
        /// <remarks>Setting this field may have the side effect of setting certain read-only 
        /// properties such as CommentUrl and CommentStyle depending on whether CommentAPI 
        /// elements are contained in the table.</remarks>
        public Dictionary<XmlQualifiedName, string> OptionalElements {
            get { return _optionalElements; }
            set { _optionalElements = null; }
        }


        private List<string> outgoingRelationships = new List<string>(); 
        /// <summary>
        /// Returns a collection of strings representing URIs to outgoing links in a feed. 
        /// </summary>
        public List<string> OutGoingLinks
        {
            get
            {
                return outgoingRelationships;
            }
            internal set
            {
                outgoingRelationships = value;
            }
        }



        /// <summary>
        /// Returns the feed object to which this item belongs
        /// </summary>
        public INewsFeed Feed
        {
            get
            {
                return this.myfeed;
            }
        }

        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item. 
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message.  </param>
        /// <returns></returns>
        public String ToString(NewsItemSerializationFormat format)
        {
            return ToString(format, true, false);
        }

        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item. 
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message. </param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <returns>A string representation of this news item</returns>		
        public String ToString(NewsItemSerializationFormat format, bool useGMTDate)
        {
            return ToString(format, useGMTDate, false);
        }


        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item. 
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message. </param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>		
        /// <returns>A string representation of this news item</returns>		
        public String ToString(NewsItemSerializationFormat format, bool useGMTDate, bool noDescriptions)
        {
            string toReturn;

            switch (format)
            {
                case NewsItemSerializationFormat.NewsPaper:
                case NewsItemSerializationFormat.RssFeed:
                case NewsItemSerializationFormat.RssItem:
                    toReturn = ToRssFeedOrItem(format, useGMTDate, noDescriptions);
                    break;
                case NewsItemSerializationFormat.NntpMessage:
                    throw new NotSupportedException(format.ToString());
        
                default:
                    throw new NotSupportedException(format.ToString());
            }


            return toReturn;
        }

        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item.  
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return ToString(NewsItemSerializationFormat.RssItem);
        }

        /// <summary>
        /// Converts the NewsItem to an XML representation of an 
        /// RSS item element is returned or an entire RSS feed with this item as its 
        /// sole item.
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message. </param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>		
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>				
        /// <returns>An RSS item or RSS feed</returns>
        public String ToRssFeedOrItem(NewsItemSerializationFormat format, bool useGMTDate, bool noDescriptions)
        {
            StringBuilder sb = new StringBuilder("");
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));
            writer.Formatting = Formatting.Indented;

            if (format == NewsItemSerializationFormat.RssFeed || format == NewsItemSerializationFormat.NewsPaper)
            {
                if (format == NewsItemSerializationFormat.NewsPaper)
                {
                    writer.WriteStartElement("newspaper");
                    writer.WriteAttributeString("type", "newsitem");
                }
                else
                {
                    writer.WriteStartElement("rss");
                    writer.WriteAttributeString("version", "2.0");
                }

                writer.WriteStartElement("channel");
                writer.WriteElementString("title", FeedDetails.Title);
                writer.WriteElementString("link", FeedDetails.Link);
                writer.WriteElementString("description", FeedDetails.Description);

                foreach (string s in FeedDetails.OptionalElements.Values)
                {
                    writer.WriteRaw(s);
                }
            }

            WriteItem(writer, useGMTDate, noDescriptions);

            if (format == NewsItemSerializationFormat.RssFeed || format == NewsItemSerializationFormat.NewsPaper)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Helper function used by ToString(bool). 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="useGMTDate">Indicates whether the time should be written out as GMT or local time</param>
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>						
        public void WriteItem(XmlWriter writer, bool useGMTDate, bool noDescriptions)
        {
            //<item>
            writer.WriteStartElement("item");

            // xml:lang attribute
            if (!string.IsNullOrEmpty(Language))
            {
                writer.WriteAttributeString("xml", "lang", null, Language);
            }

            // <title />
            if (!string.IsNullOrEmpty(Title))
            {
                writer.WriteElementString("title", Title);
            }

            // <link /> 
            if (!string.IsNullOrEmpty(HRef))
            {
                writer.WriteElementString("link", HRef);
            }

            // <pubDate /> 			we write it with InvariantInfo to get them stored in a non-localized format
            if (useGMTDate)
            {
                writer.WriteElementString("pubDate", Date.ToString("r", DateTimeFormatInfo.InvariantInfo));
            }
            else
            {
                writer.WriteElementString("pubDate", Date.ToLocalTime().ToString("F", DateTimeFormatInfo.InvariantInfo));
            }

            // <category />
            if (!string.IsNullOrEmpty(Subject))
            {
                writer.WriteElementString("category", Subject);
            }

            //<guid>
            if (!string.IsNullOrEmpty(Id) && (Id.Equals(HRef) == false))
            {
                writer.WriteStartElement("guid");
                writer.WriteAttributeString("isPermaLink", "false");
                writer.WriteString(Id);
                writer.WriteEndElement();
            }

            //<dc:creator>
            if (!string.IsNullOrEmpty(Author))
            {
                writer.WriteElementString("creator", "http://purl.org/dc/elements/1.1/", Author);
            }

            //<annotate:reference>
            if (!string.IsNullOrEmpty(ParentId))
            {
                writer.WriteStartElement("annotate", "reference", "http://purl.org/rss/1.0/modules/annotate/");
                writer.WriteAttributeString("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#", ParentId);
                writer.WriteEndElement();
            }

            // Always prefer <description> 
            if (!noDescriptions && HasContent)
            {
                /* if(this.ContentType != ContentType.Xhtml){ */
                writer.WriteStartElement("description");
                writer.WriteCData(Content);
                writer.WriteEndElement();              
            }

            //<wfw:comment />
            if (!string.IsNullOrEmpty(CommentUrl))
            {
                if (CommentStyle == SupportedCommentStyle.CommentAPI)
                {
                    writer.WriteStartElement("wfw", "comment", RssHelper.NsCommentAPI);
                    writer.WriteString(CommentUrl);
                    writer.WriteEndElement();
                }
            }

            //<wfw:commentRss />
            if (!string.IsNullOrEmpty(CommentRssUrl))
            {
                writer.WriteStartElement("wfw", "commentRss", RssHelper.NsCommentAPI);
                writer.WriteString(CommentRssUrl);
                writer.WriteEndElement();
            }

            //<slash:comments>
            if (CommentCount != NewsItem.NoComments)
            {
                writer.WriteStartElement("slash", "comments", "http://purl.org/rss/1.0/modules/slash/");
                writer.WriteString(CommentCount.ToString());
                writer.WriteEndElement();
            }


            //	if(format == NewsItemSerializationFormat.NewsPaper){

            writer.WriteStartElement("fd", "state", "http://www.bradsoft.com/feeddemon/xmlns/1.0/");
            writer.WriteAttributeString("read", BeenRead ? "1" : "0");
            writer.WriteAttributeString("flagged", FlagStatus == Flagged.None ? "0" : "1");
            writer.WriteEndElement();

            //	} else { 
            //<rssbandit:flag-status />
            if (FlagStatus != Flagged.None)
            {
                //TODO: check: why we don't use the v2004/vCurrent namespace?
                writer.WriteElementString("flag-status", NamespaceCore.Feeds_v2003, FlagStatus.ToString());
            }
            //	}


            if (p_watchComments)
            {
                //TODO: check: why we don't use the v2004/vCurrent namespace?
                writer.WriteElementString("watch-comments", NamespaceCore.Feeds_v2003, "1");
            }

            if (HasNewComments)
            {
                //TODO: check: why we don't use the v2004/vCurrent namespace?
                writer.WriteElementString("has-new-comments", NamespaceCore.Feeds_v2003, "1");
            }

            //<enclosure />
            if (Enclosures != null)
            {
                foreach (IEnclosure enc in Enclosures)
                {
                    writer.WriteStartElement("enclosure");
                    writer.WriteAttributeString("url", enc.Url);
                    writer.WriteAttributeString("type", enc.MimeType);
                    writer.WriteAttributeString("length", enc.Length.ToString());
                    if (enc.Downloaded)
                    {
                        writer.WriteAttributeString("downloaded", "1");
                    }
                    if (enc.Duration != TimeSpan.MinValue)
                    {
                        writer.WriteAttributeString("duration", enc.Duration.ToString());
                    }
                    writer.WriteEndElement();
                }
            }

            //<rssbandit:outgoing-links />            
            writer.WriteStartElement("outgoing-links", NamespaceCore.Feeds_v2003);
            foreach (string outgoingLink in OutGoingLinks)
            {
                writer.WriteElementString("link", NamespaceCore.Feeds_v2003, outgoingLink);
            }
            writer.WriteEndElement();

            /* everything else */
            foreach (string s in OptionalElements.Values)
            {
                writer.WriteRaw(s);
            }

            //end </item> 
            writer.WriteEndElement();
        }


        #endregion 

        #region IRelation implementation 

        /// <summary>
        /// Return a web reference, a resource ID, mail/message ID, NNTP post ID.
        /// </summary>
        public string HRef
        {
            get { return this.Link; }
        }

        /// <summary>
        /// Return a list of outgoing Relation objects, e.g. 
        /// links the current relation resource points to.
        /// </summary>
        public IList<string> OutgoingRelations
        {
            get
            {
                return outgoingRelationships;
            }
        }

        /// <summary>
        /// The DateTime the item was published/updated. It should be specified as UTC.
        /// </summary>
        /// <remarks>This property is read only</remarks>
        public virtual DateTime PointInTime
        {
            get { return this.Date; }
            set { /* can't set IFeedItem.PubDate */ }
        }

        /// <summary>
        /// Return true, if the Relation has some external relations (that are not part
        /// of the RelationCosmos). Default is false;
        /// </summary>
        public virtual bool HasExternalRelations { get { return false; } }

        /// <summary>
        /// Gets called if <see cref="HasExternalRelations">HasExternalRelations</see>
        /// returns true to retrieve the external Relation resource(s).
        /// Default return is the RelationCosmos.EmptyRelationList.
        /// </summary>
        public virtual IList<IRelation> GetExternalRelations()
        {
                return GetList<IRelation>.Empty;
        }
        /// <summary>
        /// Should be overridden. Stores a collection of external Relations related
        /// to this RelationBase.
        /// </summary>
        public virtual void SetExternalRelations<T>(IList<T> relations) where T: IRelation
        {
            /* not supported for Windows RSS items */ 
        }

        #endregion 


        #region ICloneable implementation

        /// <summary>
        /// Returns a copy of this object
        /// </summary>
        /// <returns>A copy of this object</returns>
        public object Clone()
        {
            return new WindowsRssNewsItem(this.myitem, this.myfeed); 
        }

        /// <summary>
        /// Copies the item (clone) and set the new parent to the provided feed 
        /// </summary>
        /// <param name="f">NewsFeed</param>
        /// <returns></returns>
        public INewsItem Clone(INewsFeed f)
        {
            //BUGBUG: This will throw exceptions when used as part of flagging or watching items. Instead return NewsItem instance
            return new WindowsRssNewsItem(this.myitem, f as WindowsRssNewsFeed); 
        }

        #endregion 

        #region IEquatable implementation 

        /// <summary>
        /// Compares to see if two WindowsRssNewsItems are identical. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as INewsItem);
        }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
        public bool Equals(INewsItem other)
        {
            return Equals(other as WindowsRssNewsItem);
        }

        /// <summary>
        /// Tests if this item is the same as another. The item to test. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Equals(WindowsRssNewsItem item) {
            if (item == null)
                return false;

            return item.myfeed.id.Equals(this.myfeed.id) && item.Id.Equals(this.Id); 
        }

        /// <summary>
        /// Returns a hash code for the given item
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #endregion 

        #region IComparable implementation

        /// <summary>
        /// Impl. IComparable.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return CompareTo(obj as WindowsRssNewsItem);
        }

		/// <summary>
		/// Compares to another instance.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
        public int CompareTo(WindowsRssNewsItem other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            if (ReferenceEquals(other, null))
                return 1;

            return this.Date.CompareTo(other.Date);
        }

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
        public int CompareTo(IRelation other)
        {
            return CompareTo(other as WindowsRssNewsItem);
        }

        #endregion

        #region IXPathNavigable implementation 

        /// <summary>
        /// Creates an XPathNavigator over this object
        /// </summary>
        /// <returns></returns>
        public XPathNavigator CreateNavigator()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(this.myitem.Xml(FEEDS_XML_INCLUDE_FLAGS.FXIF_NONE));
            return doc.CreateNavigator(); 
        }

        #endregion 

    }


	/// <summary>
	/// Helper to access a IFeedItem instance and handle/ignore
	/// some of the wired exceptions.
	/// </summary>
	static class Access
	{
		/// <summary>
		/// Applies the action to the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="action">The action.</param>
		public static void Apply(IFeedItem item, Action action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			if (item == null)
				return;

			try
			{
				action();
			}
			catch (FileNotFoundException) { /* ignore */ }
			catch (COMException comEx)
			{
				if (comEx.ErrorCode == -2147023728)	// Element not found. (Exception from HRESULT: 0x80070490)
					return;
				throw;
			}
		}

		/// <summary>
		/// Gets anything out of the specified item using the <paramref name="actionWithResult"/> delegate.
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="item">The item.</param>
		/// <param name="actionWithResult">The action with result.</param>
		/// <returns></returns>
		public static TResult Get<TResult>(IFeedItem item, Func<IFeedItem, TResult> actionWithResult)
		{
			if (actionWithResult == null)
				throw new ArgumentNullException("actionWithResult");

			if (item == null)
				return default(TResult); 

			try
			{
				return actionWithResult(item);
			}
			catch (FileNotFoundException) {/* ignore */ }
			catch (COMException comEx)
			{
				if (comEx.ErrorCode == -2147023728)	// Element not found. (Exception from HRESULT: 0x80070490)
					return default(TResult);
				throw;
			} 
			return default(TResult);
		}
	}
    #endregion 

    #region WindowsRssEnclosure

    /// <summary>
    /// Represents an enclosure from the Windows RSS platform
    /// </summary>
    public class WindowsRssEnclosure : BindableObject, IEnclosure
    {

        #region constructors 

           /// <summary>
        /// We always want an associated IFeedItem instance
        /// </summary>
        private WindowsRssEnclosure() { ;}

          /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="enclosure">The IFeedEnclosure instance that this object will wrap</param>
        internal WindowsRssEnclosure(IFeedEnclosure enclosure)
        {
            if (enclosure == null) throw new ArgumentNullException("enclosure");
            this.myenclosure = enclosure;
          
            //TODO: RelationCosmos and outgoing links processing? 
        }

        #endregion 

        #region private fields

        /// <summary>
        /// The IFeedEnclosure instance wrapped by this object
        /// </summary>
        private IFeedEnclosure myenclosure; 

        #endregion 



        #region IEnclosure implementation 

        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        public string MimeType
        {
            get { return myenclosure.Type; }
        }

        /// <summary>
        /// The length of the enclosure in bytes
        /// </summary>
        public long Length
        {
            get { return myenclosure.Length; }
        }

        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        public string Url
        {
            get { return myenclosure.DownloadUrl; }
        }

        /// <summary>
        /// The description associated with the item obtained via itunes:subtitle or media:title
        /// </summary>
        /// <remarks>This field is read only</remarks>
        public string Description {
            get { return null; }
            set { ; }
        }

        private bool _downloaded;
        /// <summary>
        /// Indicates whether this enclosure has already been downloaded or not.
        /// </summary>
        public bool Downloaded
        {
            get { return _downloaded; }
            set
            {
                _downloaded = value;
                RaisePropertyChanged("Downloaded");
            }
        }


        /// <summary>
        /// Gets the playing time of the enclosure. 
        /// </summary>
        /// <remarks>This field is read only</remarks>
        public TimeSpan Duration
        {
            get { return TimeSpan.MinValue; }
            set { /* */ }
        }

        #endregion 

        #region IEquatable implementation 

        /// <summary>
        /// Compares to see if two Enclosures are identical. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as WindowsRssEnclosure);
        }

		/// <summary>
		/// Compares to see if two WindowsRssEnclosure are identical. Identity just checks to see if they have
		/// the same link,
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
        public bool Equals(IEnclosure item)
        {
            if (item == null)
            {
                return false;
            }

            if (ReferenceEquals(this, item))
            {
                return true;
            }

            if (String.Compare(Url, item.Url) == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
        	if (string.IsNullOrEmpty(Url))
            {
                return String.Empty.GetHashCode();
            }
        	return Url.GetHashCode();
        }

    	#endregion
    }

    #endregion 

    #region WindowsRssNewsFeed

    /// <summary>
    /// Represents a NewsFeed obtained from the Windows RSS platform
    /// </summary>
    [DebuggerDisplay("Title = {title}, Uri = {link}")]
    internal class WindowsRssNewsFeed : INewsFeed, IDisposable, IFeedDetails
    {

        #region constructors 

        /// <summary>
        /// We always want an associated IFeed instance
        /// </summary>
        private WindowsRssNewsFeed() { ;}

        /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="feed">The IFeed instance that this object will wrap</param>
        internal WindowsRssNewsFeed(IFeed feed) {
            if (feed == null) throw new ArgumentNullException("feed"); 
            this.myfeed = feed;
            
            /* do this here because COM interop is too slow to check it each time property is accessed */
            this._id = myfeed.LocalId; 
            
            //make sure we have a list of items ready to go
            this.LoadItemsList();             
        }

        /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="feed">The IFeed instance that this object will wrap</param>
        /// <param name="banditfeed">The object that contains the settings that will be used to initialize this class</param>
        /// <param name="owner">This object's owner</param>
        internal WindowsRssNewsFeed(IFeed feed, INewsFeed banditfeed, object owner): this(feed)
        {
            if (banditfeed != null)
            {
                this.refreshrate = banditfeed.refreshrate;
                this.refreshrateSpecified = banditfeed.refreshrateSpecified;
                this.maxitemage = banditfeed.maxitemage;
                this.markitemsreadonexit = banditfeed.markitemsreadonexit;
                this.markitemsreadonexitSpecified = banditfeed.markitemsreadonexitSpecified;
                this.listviewlayout = banditfeed.listviewlayout;
                this.favicon = banditfeed.favicon;
                this.stylesheet = banditfeed.stylesheet;
                this.enclosurealert = banditfeed.enclosurealert;
                this.enclosurealertSpecified = banditfeed.enclosurealertSpecified;
                this.alertEnabled = banditfeed.alertEnabled;
                this.alertEnabledSpecified = banditfeed.alertEnabledSpecified;
                this.Any = banditfeed.Any;
                this.AnyAttr = banditfeed.AnyAttr;
            }
            
            if (owner is WindowsRssFeedSource)
            {
                this.owner = owner;
            }
        }

        #endregion 

        #region destructor and IDisposable implementation 

        /// <summary>
        /// Releases the associated COM objects
        /// </summary>
        /// <seealso cref="myfeed"/>
        ~WindowsRssNewsFeed() {
            Dispose(false);           
        }

        /// <summary>
        /// Disposes of the class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
			GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the class
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
				lock (this)
				{
					if (!disposed && myfeed != null)
					{
						Marshal.ReleaseComObject(myfeed);
						myfeed = null;
					}

					disposed = true;
				}
		}

        #endregion


        #region private fields

        /// <summary>
        /// Indicates that the object has been disposed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The actual IFeed instance that this object is wrapping
        /// </summary>
        private IFeed myfeed;

        /// <summary>
        /// The list of WindowsRssNewsItem instances contained by this feed. 
        /// </summary>
        private readonly List<INewsItem> items = new List<INewsItem>(); 


        private static readonly ILog _log = Log.GetLogger(typeof (WindowsRssNewsFeed));


        #endregion

        #region private methods

        /// <summary>
        /// Loads items from the underlying Windows RSS platform into the ItemsList property
        /// </summary>
        /// <seealso cref="ItemsList"/>
        internal void LoadItemsList()
        {
            this.items.Clear();
            IFeedsEnum feedItems = this.myfeed.Items as IFeedsEnum;

			if (feedItems != null)
				foreach (IFeedItem item in feedItems)
				{
					this.items.Add(new WindowsRssNewsItem(item, this));
				}
            _log.DebugFormat("LOAD_ITEMS_LIST:'{0}' loaded {1} item(s)", myfeed.Path, items.Count); 
        }

        #endregion 

        #region public methods

        /// <summary>
        /// Sets the IFeed object represented by this object
        /// </summary>
        /// <param name="feed">The IFeed instance</param>
        internal void SetIFeed(IFeed feed) {
            if (feed != null)
            {
                lock (this)
                {
                    if (myfeed != null)
                    {
                        Marshal.ReleaseComObject(myfeed);
                    }
                    myfeed = feed;
                    this.LoadItemsList(); //fixup object references to news items 
                }
            }        
        }

        /// <summary>
        /// Marks all items in the feed as read
        /// </summary>
        public void MarkAllItemsAsRead()
        {
            if (myfeed != null)
            {
                lock (WindowsRssFeedSource.event_caused_by_rssbandit_syncroot)
                {
                    WindowsRssFeedSource.event_caused_by_rssbandit = true;
                    myfeed.MarkAllItemsRead(); 
                }                
            }
        }

        /// <summary>
        /// Disables automatically updating the feed. Feed will only be updated when RefreshFeed() is called. 
        /// </summary>
        public void DisableFeedPolling()
        {
            this.myfeed.SyncSetting = FEEDS_SYNC_SETTING.FSS_MANUAL; 
        }


        /// <summary>
        /// Enables automatically updating the feed.  
        /// </summary>
        public void EnableFeedPolling()
        {
            this.myfeed.SyncSetting = FEEDS_SYNC_SETTING.FSS_INTERVAL;
        }


        #endregion 

        #region INewsFeed implementation

        public string title
        {
            get { return myfeed.Name; }

            set
            {
                lock (WindowsRssFeedSource.event_caused_by_rssbandit_syncroot)
                {
                    if (!StringHelper.EmptyTrimOrNull(value))
                    {
                        myfeed.Rename(value);
                        OnPropertyChanged("title");
                    }
                }
                
                WindowsRssFeedSource.event_caused_by_rssbandit = true;
            }
        }

        private string _link; 
        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlElement(DataType = "anyURI")]
        public string link
        {
            get
            {
                try
                {
                    _link = myfeed.DownloadUrl;
                }
                catch (Exception e) /* thrown if the feed has never been downloaded */ 
                {
                    _log.Debug("Exception on accessing IFeed.DownloadUrl: " + _link, e); 

                    try
                    {
                        _link = myfeed.Url;
                    }
                    catch (Exception e2)  /* thrown if the feed has been deleted. */
                    { 
                        _log.Debug("Exception on accessing IFeed.Url: " + _link, e2); 
                    } 
                }

                return _link; /* we should get the last known link for the IFeed if one was ever obtained */ 
            }

            set
            {
                /* can't set IFeed.DownloadUrl */
            }
        }

        private string _id; 
        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlAttribute]
        public string id
        {
            get { return _id; }

            set
            {
              /* can't set IFeed.LocalId */
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlElement("last-retrieved")]
        public DateTime lastretrieved
        {
            get { return myfeed.LastDownloadTime; }
            set
            {
                /* can't set myfeed.LastDownloadTime */
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlIgnore]
        public bool lastretrievedSpecified
        {
            get
            {
                return true;
            }

            set
            { 
                /* it should always be set */
            }
        }


        /// <remarks>This property does not apply to this object </remarks>
        public string etag { get { return null; } set { /* not applicable to this type */ } }

        /// <remarks>This property does not apply to this object </remarks>
        [XmlElement(DataType = "anyURI")]
        public string cacheurl { get { return null; } set { /* not applicable to this type */ } }

        /// <remarks/>
        [XmlElement("max-item-age", DataType = "duration")]
        public string maxitemage { get; set; }


        private List<string> _storiesrecentlyviewed = new List<string>();
        /// <remarks/>
        [XmlArray(ElementName = "stories-recently-viewed", IsNullable = false)]
        [XmlArrayItem("story", Type = typeof(String), IsNullable = false)]
        public List<string> storiesrecentlyviewed
        {
            get
            {
                //TODO: Can we make this less expensive. Current implementation tries to be careful in case
                // items marked as read outside RSS Bandit               
                foreach (INewsItem item in items)
                {
                    if (item.BeenRead)
                    {
                        _storiesrecentlyviewed.Add(item.Id);
                    }
                } 
                return _storiesrecentlyviewed;
            }
            set
            {
                _storiesrecentlyviewed = new List<string>(value);
            }
        }

        private List<string> _deletedstories = new List<string>();
        /// <remarks/>
        [XmlArray(ElementName = "deleted-stories", IsNullable = false)]
        [XmlArrayItem("story", Type = typeof(String), IsNullable = false)]
        public List<string> deletedstories
        {
            get
            {
                return _deletedstories;
            }
            set
            {
                _deletedstories = new List<string>(value);
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlElement("if-modified-since")]
        public DateTime lastmodified {
            get
            {
                return myfeed.LastWriteTime;
            }
            set
            {
                /* can't set IFeed.LastWriteTime */
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlIgnore]
        public bool lastmodifiedSpecified { get { return true; } set { } }

		/// <remarks>Not supported by this object</remarks>
		[XmlElement("certificate-id")]
		public string certificateId { get { return null; } set { } }

        /// <remarks>Not supported by this object</remarks>
        [XmlElement("auth-user")]
        public string authUser { get { return null; } set { } }

        /// <remarks>Not supported by this object</remarks>       
        [XmlElement("auth-password", DataType = "base64Binary")]
        public Byte[] authPassword { get { return null; } set { } }

        /// <remarks/>
        [XmlElement("listview-layout")]
        public string listviewlayout { get; set; }

        private string _favicon;
        /// <remarks/>
        public string favicon
        {
            get
            {
                return _favicon;
            }

            set
            {
                if (String.IsNullOrEmpty(_favicon) || !_favicon.Equals(value))
                {
                    _favicon = value;
                    this.OnPropertyChanged("favicon");
                }
            }
        }




        /// <remarks/>
        [XmlElement("download-enclosures")]
        public bool downloadenclosures
        {
            get
            {
                return myfeed.DownloadEnclosuresAutomatically;
            }

            set
            {
                myfeed.DownloadEnclosuresAutomatically = value;
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlIgnore]
        public bool downloadenclosuresSpecified { get { return true; } set { /* it is always set */ } }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlElement("enclosure-folder")]
        public string enclosurefolder
        {
            get
            {
                return myfeed.LocalEnclosurePath;
            }

            set
            {
             /* IFeed.LocalEnclosurePath can't be set */ 
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlAttribute("replace-items-on-refresh")]
        public bool replaceitemsonrefresh
        {
            get
            {
                try
                {
                    return myfeed.IsList;
                }
                catch
                {
                    return false; 
                }
            }
            set
            {
                /* IFeed.IsList can't be set */ 
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlIgnore]
        public bool replaceitemsonrefreshSpecified { get { return true; } set { } }

      
        public string stylesheet { get; set; }

        /// <remarks>Reference the corresponding NntpServerDefinition</remarks>
        [XmlElement("news-account")]
        public string newsaccount { get; set; }

        /// <remarks/>
        [XmlElement("mark-items-read-on-exit")]
        public bool markitemsreadonexit { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool markitemsreadonexitSpecified { get; set; }

        /// <remarks/>
        [XmlAnyElement]
        public XmlElement[] Any { get; set; }


        /// <remarks/>
        [XmlAttribute("alert"), DefaultValue(false)]
        public bool alertEnabled { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool alertEnabledSpecified { get; set; }


        /// <remarks/>
        [XmlAttribute("enclosure-alert"), DefaultValue(false)]
        public bool enclosurealert { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool enclosurealertSpecified { get; set; }

        
        /// <remarks/>
        [XmlAttribute]
        public string category {

            get
            {
                string categoryName = null, path  = this.myfeed.Path;
                int index = path.LastIndexOf(FeedSource.CategorySeparator);
             
                if (index != -1)
                {                 
                    categoryName = path.Substring(0, index);                   
                }

                return categoryName; 
            }

            set
            {
                if (!StringHelper.EmptyTrimOrNull(value) && !value.Equals(this.category))
                {
                    WindowsRssFeedSource handler = owner as WindowsRssFeedSource;
                    handler.ChangeCategory(this, handler.AddCategory(value)); 
                }
            }
        
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual List<string> categories
        {
            get { return new List<string>(new string[]{this.category}); }
            set
            {
               /* Setting this value does nothing */ 
            }
        }

        /// <remarks/>
        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttr { get; set; }

        /// <remarks>True, if the feed caused an exception on request to prevent sequenced
        /// error reports on every automatic download</remarks>
        [XmlIgnore]
        public bool causedException
        {
            get
            {
                return causedExceptionCount != 0;
            }
            set
            {
                if (value)
                {
                    causedExceptionCount++; // raise counter
                    lastretrievedSpecified = true;
                    lastretrieved = new DateTime(DateTime.Now.Ticks);
                }
                else
                    causedExceptionCount = 0; // reset
            }
        }

        /// <remarks>Number of exceptions caused on requests</remarks>
        [XmlIgnore]
        public int causedExceptionCount { get; set; }

        /// <remarks>Can be used to store any attached data</remarks>
        [XmlIgnore]
        public object Tag { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool containsNewMessages
        {
            get
            {
                bool hasUnread = items.Any(item => item.BeenRead == false);
                return hasUnread; 
            }

            set
            {
                /* This value is always correct */ 
            }
        }

        private bool _containsNewComments;
        /// <remarks/>
        [XmlIgnore]
        public bool containsNewComments
        {
            get
            {
                return _containsNewComments;
            }

            set
            {
                if (!_containsNewComments.Equals(value))
                {
                    _containsNewComments = value;
                    this.OnPropertyChanged("containsNewComments");
                }
            }
        }


        /// <remarks/>
        [XmlIgnore]
        public virtual bool refreshrateSpecified { 
            get { return myfeed.Interval > 0; }
            set { myfeed.Interval = 0; }
            }


        /// <remarks/>
        [XmlElement("refresh-rate")]
        public virtual int refreshrate
        {
            get
            {
                return myfeed.Interval * 60 * 1000; //convert minutes to milliseconds
            }

            set
            {
                if (value <= 0) return; /* Windows RSS platform doesn't support this */
                value = value / (60 * 1000); //convert to minutes

                if (!myfeed.Interval.Equals(value))
                {
                    myfeed.Interval = value;                  
                }
            }
        }

        /// <remarks />                
        [XmlIgnore]
        public object owner { get; set; }

        /// <summary>
        /// Gets the value of a particular wildcard element. If the element is not found then 
        /// null is returned
        /// </summary>
        /// <param name="namespaceUri"></param>
        /// <param name="localName"></param>
        /// <returns>The value of the wildcard element obtained by calling XmlElement.InnerText
        /// or null if the element is not found. </returns>
        public string GetElementWildCardValue(string namespaceUri, string localName)
        {
            foreach (XmlElement element in Any)
            {
                if (element.LocalName == localName && element.NamespaceURI == namespaceUri)
                    return element.InnerText;
            }
            return null;
        }

        /// <summary>
        /// Removes an entry from the storiesrecentlyviewed collection
        /// </summary>
        /// <seealso cref="storiesrecentlyviewed"/>
        /// <param name="storyid">The ID to add</param>
        public void AddViewedStory(string storyid)
        {
            if (!_storiesrecentlyviewed.Contains(storyid))
            {
                _storiesrecentlyviewed.Add(storyid);
                if (null != PropertyChanged)
                {
                    this.OnPropertyChanged(new CollectionChangedEventArgs("storiesrecentlyviewed", CollectionChangeAction.Add, storyid));
                }
            }
        }

        /// <summary>
        /// Adds an entry to the storiesrecentlyviewed collection
        /// </summary>
        /// <seealso cref="storiesrecentlyviewed"/>
        /// <param name="storyid">The ID to remove</param>
        public void RemoveViewedStory(string storyid)
        {
            if (_storiesrecentlyviewed.Contains(storyid))
            {
                _storiesrecentlyviewed.Remove(storyid);
                if (null != PropertyChanged)
                {
                    this.OnPropertyChanged(new CollectionChangedEventArgs("storiesrecentlyviewed", CollectionChangeAction.Remove, storyid));
                }
            }
        }

        /// <summary>
        /// Removes an entry from the deletedstories collection
        /// </summary>
        /// <seealso cref="deletedstories"/>
        /// <param name="storyid">The ID to add</param>
        public void AddDeletedStory(string storyid)
        {
            if (!_deletedstories.Contains(storyid))
            {
                _deletedstories.Add(storyid);
                if (null != PropertyChanged)
                {
                    this.OnPropertyChanged(new CollectionChangedEventArgs("deletedstories", CollectionChangeAction.Add, storyid));
                }
            }
        }

        /// <summary>
        /// Adds an entry to the deletedstories collection
        /// </summary>
        /// <seealso cref="deletedstories"/>
        /// <param name="storyid">The ID to remove</param>
        public void RemoveDeletedStory(string storyid)
        {
            if (_deletedstories.Contains(storyid))
            {
                _deletedstories.Remove(storyid);
                if (null != PropertyChanged)
                {
                    this.OnPropertyChanged(new CollectionChangedEventArgs("deletedstories", CollectionChangeAction.Remove, storyid));
                }
            }

        }

        /// <summary>
        /// Adds a category to the categories collection
        /// </summary>
        /// <seealso cref="categories"/>
        /// <param name="name">The category to add</param>
        public virtual void AddCategory(string name)
        {
           //DareO: Not clear what this method should do here. 
        }

        /// <summary>
        /// Removes a category from the categories collection
        /// </summary>
        /// <seealso cref="categories"/>
        /// <param name="name">The category to remove</param>
        public virtual void RemoveCategory(string name)
        {
            //DareO: Not clear what this method should do here. 
        }

        #endregion 

        #region IFeedDetails implementation 

        /// <summary>Gets the Feed Language</summary>
        public string Language { 
            get { return this.myfeed.Language; }
        }

        /// <summary>Gets the Feed Title</summary>
        public string Title {
            get { return this.myfeed.Title; }
        }

        /// <summary>Gets the Feed Homepage Link</summary>
        public string Link {
            get { return this.myfeed.Link; }
        }

        /// <summary>Gets the Feed Description</summary>
        public string Description {
            get { return this.myfeed.Description; }
        }


        /// <summary>
        /// The list of news items belonging to the feed
        /// </summary>
        public List<INewsItem> ItemsList
        {

            get
            {
                lock (this.items)
                {
                    LoadItemsList(); 
                }

                return this.items;
            }
            set
            {
                /* Can't set IFeed.Items */
            }

        }

        private Dictionary<XmlQualifiedName, string> _optionalElements = new Dictionary<XmlQualifiedName, string>(); 
        /// <summary>Gets the optional elements found at Feed level</summary>	  
        public Dictionary<XmlQualifiedName, string> OptionalElements {
            get { return this._optionalElements; } 
        }

        /// <summary>
        /// Gets the type of the FeedDetails info
        /// </summary>
        public FeedType Type {
            get { return FeedType.Rss; }
        }

        /// <summary>
        /// The unique identifier for the feed
        /// </summary>
        string IFeedDetails.Id
        {
            get { return this.id; }

            set
            {
                /* can't set IFeed.LocalId */
            }
        }

        /// <summary>
        /// Returns a copy of this object
        /// </summary>
        /// <returns>A copy of this object</returns>
        public object Clone()
        {
            return new WindowsRssNewsFeed(myfeed);
        }

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(XmlWriter writer)
        {
            this.WriteTo(writer, NewsItemSerializationFormat.RssFeed, true);
        }


        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
        public void WriteTo(XmlWriter writer, NewsItemSerializationFormat format)
        {
            this.WriteTo(writer, format, true);
        }

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>				
        public void WriteTo(XmlWriter writer, NewsItemSerializationFormat format, bool useGMTDate)
        {
            //writer.WriteStartDocument(); 

            if (format == NewsItemSerializationFormat.NewsPaper)
            {
                //<newspaper type="channel">
                writer.WriteStartElement("newspaper");
                writer.WriteAttributeString("type", "channel");
                writer.WriteElementString("title", this.title);
            }
            else if (format != NewsItemSerializationFormat.Channel)
            {
                //<rss version="2.0">
                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");
            }
            
            //<channel>
            writer.WriteStartElement("channel");

            //<title />
            writer.WriteElementString("title", this.Title);

            //<link /> 
            writer.WriteElementString("link", this.Link);

            //<description /> 
            writer.WriteElementString("description", this.Description);

            //other stuff
            foreach (string s in this.OptionalElements.Values)
            {
                writer.WriteRaw(s);
            }

            //<item />
            foreach (INewsItem item in this.ItemsList)
            {
                writer.WriteRaw(item.ToString(NewsItemSerializationFormat.RssItem, true));
            }

            writer.WriteEndElement();

            if (format != NewsItemSerializationFormat.Channel)
            {
                writer.WriteEndElement();
            }

            //writer.WriteEndDocument(); 
        }

        /// <summary>
        /// Provides the XML representation of the feed as an RSS 2.0 feed. 
        /// </summary>
        /// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
        /// <returns>the feed as an XML string</returns>
        public string ToString(NewsItemSerializationFormat format)
        {
            return this.ToString(format, true);
        }

        /// <summary>
        /// Provides the XML representation of the feed as an RSS 2.0 feed. 
        /// </summary>
        /// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <returns>the feed as an XML string</returns>
        public string ToString(NewsItemSerializationFormat format, bool useGMTDate)
        {

            StringBuilder sb = new StringBuilder("");
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));

            this.WriteTo(writer, format, useGMTDate);

            writer.Flush();
            writer.Close();

            return sb.ToString();

        }


        #endregion 

        #region INotifyPropertyChanged implementation

        /// <summary>
        ///  Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fired whenever a property is changed. 
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(DataBindingHelper.GetPropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies listeners that a property has changed. 
        /// </summary>
        /// <param name="e">Details on the property change event</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, e);
            }
        }


        #endregion 

        #region public methods

        /// <summary>
        /// Synchronously downloads the feed using the Windows RSS platform
        /// </summary>
        public void RefreshFeed()
        {
            this.RefreshFeed(false);
        }

        /// <summary>
        /// Asynchronously downloads the feed using the Windows RSS platform
        /// </summary>
        /// <param name="async">Determines whether the download should happen asynchronously</param>
        public void RefreshFeed(bool async)
        {
            try
            {
                if (async)
                {
                    this.myfeed.AsyncDownload();
                }
                else
                {
                    this.myfeed.Download();
                }
            }
            catch (Exception e)
            {
                _log.Debug("Exception in WindowsRssNewsFeed.RefreshFeed()", e); 
            }
        }

        #endregion 
    }

    #endregion

    #region WindowsRssNewsFeedCategory
    [DebuggerDisplay("Category = {Value}")]
    internal class WindowsRssNewsFeedCategory : INewsFeedCategory, IDisposable, IEquatable<WindowsRssNewsFeedCategory>
    {

        #region constructor 

        /// <summary>
        /// This class must always represent an instance of IFeedFolder 
        /// </summary>
        private WindowsRssNewsFeedCategory() { ;} 

         /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="folder">The IFeed instance that this object will wrap</param>
        public WindowsRssNewsFeedCategory(IFeedFolder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder"); 
            this.myfolder = folder; 
        }
       
        
        /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="folder">The IFeed instance that this object will wrap</param>
        /// <param name="category">A category instance from which this object shall obtain the values for it's INewsFeedCategory properties</param>
        public WindowsRssNewsFeedCategory(IFeedFolder folder, INewsFeedCategory category)
            : this(folder)
        {
            if (category != null)
            {
                this.AnyAttr = category.AnyAttr;
                this.downloadenclosures = category.downloadenclosures;
                this.downloadenclosuresSpecified = category.downloadenclosuresSpecified;
                this.enclosurealert = category.enclosurealert;
                this.enclosurealertSpecified = category.enclosurealertSpecified;
                this.listviewlayout = category.listviewlayout;
                this.markitemsreadonexit = category.markitemsreadonexit;
                this.markitemsreadonexitSpecified = category.markitemsreadonexitSpecified;
                this.maxitemage = category.maxitemage;
                this.refreshrate = category.refreshrate;
                this.refreshrateSpecified = category.refreshrateSpecified;
                this.stylesheet = category.stylesheet;
            }
        }

        #endregion 

        #region private fields

        /// <summary>
        /// Indicates that the object has been disposed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The actual IFeedFolder instance that this object is wrapping
        /// </summary>
        private IFeedFolder myfolder;

        #endregion

        #region destructor and IDisposable implementation 

		/// <summary>
		/// Releases the associated COM objects
		/// </summary>
		~WindowsRssNewsFeedCategory()
        {
            Dispose(false);           
        }

        /// <summary>
        /// Disposes of the class
        /// </summary>
        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        /// <summary>
        /// Disposes of the class
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
			if (!disposed)
            lock (this)
            {
				if (!disposed && myfolder != null)
                {
                    Marshal.ReleaseComObject(myfolder);
                	myfolder = null;
                }
                disposed = true; 
            }
        }

        #endregion

        #region public methods 

        /// <summary>
        /// Sets the IFeedFolder object represented by this object
        /// </summary>
        /// <param name="folder">The IFeedFolder instance</param>
        internal void SetIFeedFolder(IFeedFolder folder)
        {
            if (folder != null)
            {
                lock (this)
                {
                    if (myfolder != null)
                    {
                        Marshal.ReleaseComObject(myfolder);
                    }
                    myfolder = folder;
                }
            }
        }

        #endregion 

        #region INewsFeedCategory implementation

        /// <remarks/>
        [XmlAttribute("mark-items-read-on-exit")]
        public bool markitemsreadonexit { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool markitemsreadonexitSpecified { get; set; }

        /// <remarks/>
        [XmlAttribute("download-enclosures")]
        public bool downloadenclosures { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool downloadenclosuresSpecified { get; set; }

        /// <remarks>This property is not supported by this object</remarks>
        [XmlAttribute("enclosure-folder")]
        public string enclosurefolder { get { return null; } set { /* */} }

        ///<summary>ID to an FeedColumnLayout</summary>
        /// <remarks/>
        [XmlAttribute("listview-layout")]
        public string listviewlayout { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public string stylesheet { get; set; }

        /// <remarks/>
        [XmlAttribute("refresh-rate")]
        public int refreshrate { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool refreshrateSpecified { get; set; }

        /// <remarks/>
        [XmlAttribute("max-item-age", DataType = "duration")]
        public string maxitemage { get; set; }

        /// <remarks/>
        [XmlText]
        public string Value
        {
            get
            {
                return myfolder.Path;
            }
            set 
            {
                if (!StringHelper.EmptyTrimOrNull(value))
                {
                    myfolder.Rename(value);
                }
            }
        }

        /// <remarks/>
        [XmlIgnore]
        public INewsFeedCategory parent { get; set; }

        /// <remarks/>
        [XmlAttribute("enclosure-alert"), DefaultValue(false)]
        public bool enclosurealert { get; set; }

        /// <remarks/>
        [XmlIgnore]
        public bool enclosurealertSpecified { get; set; }

        /// <remarks/>
        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttr { get; set; }


        #endregion 

        #region Equality methods

        /// <summary>
        /// Tests to see if two category objects represent the same feed. 
        /// </summary>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (WindowsRssNewsFeedCategory)) return false;
            return Equals((WindowsRssNewsFeedCategory) obj);
        }

        /// <summary>
        /// Returns a hashcode for a category object. 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (stylesheet != null ? stylesheet.GetHashCode() : 0);
        }

        #endregion

        public bool Equals(WindowsRssNewsFeedCategory obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.Value, Value);
        }

        public bool Equals(INewsFeedCategory other)
        {
            return Equals(other as WindowsRssNewsFeedCategory);
        }
    }

    #endregion 
}
