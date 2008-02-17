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
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using System.Runtime.InteropServices;
using Microsoft.Feeds.Interop;

using RssBandit.Common;

using NewsComponents.Collections;
using NewsComponents.Net;
using NewsComponents.Search;
using NewsComponents.Utils;

namespace NewsComponents.Feed {
    /// <summary>
    /// A NewsHandler that retrieves user subscriptions and feeds from the Windows RSS platform. 
    /// </summary>
    class WindowsRssNewsHandler : NewsHandler
    {


        #region constructor

         /// <summary>
        /// Initializes a new instance of the <see cref="NewsHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public WindowsRssNewsHandler(INewsComponentsConfiguration configuration)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = NewsHandler.DefaultConfiguration;

            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);
           
        }    

        #endregion 


        #region private fields and properties 

        /// <summary>
        /// The Windows RSS platform feed manager
        /// </summary>
        private IFeedsManager feedManager = new FeedsManagerClass();

        #endregion 

        #region private methods

        /// <summary>
        /// Add a folder to the Windows RSS common feed list
        /// </summary>
        /// <param name="path">The path to the folder</param>
        public IFeedFolder AddFolder(string path)
        {
            IFeedFolder folder = feedManager.RootFolder as IFeedFolder;

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
                        if (!folder.Path.Equals(path) && !categories.ContainsKey(folder.Path)) {
                            this.categories.Add(folder.Path, new WindowsRssNewsFeedCategory(folder));
                        }
                    }
                }
            }// if (!StringHelper.EmptyTrimOrNull(category))           

            return folder;
        }


        #endregion 

        #region public methods

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
        /// Changes the category of a particular INewsFeed. This method should be used instead of setting
        /// the category property of the INewsFeed instance. 
        /// </summary>
        /// <param name="feed">The newsfeed whose category to change</param>
        /// <param name="cat">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized</param>
        public override void ChangeCategory(INewsFeed feed, INewsFeedCategory cat)
        {
            if (feed == null)
                throw new ArgumentNullException("feed");

            if (cat == null)
                throw new ArgumentNullException("cat");

            WindowsRssNewsFeed f = feed as WindowsRssNewsFeed; 

            if (f != null && feedsTable.ContainsKey(f.link))
            {
                IFeedFolder folder = String.IsNullOrEmpty(f.category) ? feedManager.RootFolder as IFeedFolder 
                                                                      : feedManager.GetFolder(f.category) as IFeedFolder;
                IFeed ifeed        = folder.GetFeed(f.title) as IFeed;
                ifeed.Move(cat.Value);                                 
            }
        }

        /// <summary>
        /// Renames the specified category
        /// </summary>        
        /// <param name="oldName">The old name of the category</param>
        /// <param name="newName">The new name of the category</param>        
        public override void RenameCategory(string oldName, string newName)
        {
            if (StringHelper.EmptyTrimOrNull(oldName))
                throw new ArgumentNullException("oldName");

            if (StringHelper.EmptyTrimOrNull(newName))
                throw new ArgumentNullException("newName");

            if (this.categories.ContainsKey(oldName))
            {
                WindowsRssNewsFeedCategory cat = this.categories[oldName] as WindowsRssNewsFeedCategory;
                

                IFeedFolder folder = feedManager.GetFolder(oldName) as IFeedFolder;
                if (folder != null)
                {
                    folder.Rename(newName);
                    this.categories.Remove(oldName);
                    categories.Add(newName, new WindowsRssNewsFeedCategory(folder, cat));
                }
            }
        }

        /// <summary>
        /// Adds a feed and associated FeedInfo object to the FeedsTable and itemsTable. 
        /// Any existing feed objects are replaced by the new objects. 
        /// </summary>
        /// <param name="f">The NewsFeed object </param>
        /// <param name="fi">The FeedInfo object</param>
        public override INewsFeed AddFeed(INewsFeed f, FeedInfo fi)
        {
            if (f is WindowsRssNewsFeed)
            {
                if (!feedsTable.ContainsKey(f.link))
                {
                    feedsTable.Add(f.link, f);
                }
            }
            else
            {
                if (!feedManager.ExistsFolder(f.category))
                {
                    this.AddCategory(f.category);
                }

                IFeedFolder folder = feedManager.GetFolder(f.category) as IFeedFolder;
                IFeed newFeed = folder.CreateFeed(f.title, f.link) as IFeed;
                f = new WindowsRssNewsFeed(newFeed, f);
                feedsTable.Add(f.link, f);
            }

            return f;
        }

        /// <summary>
        /// Deletes all subscribed feeds and categories 
        /// </summary>
        public override void DeleteAllFeedsAndCategories()
        {
            string[] keys = new string[categories.Count];
            this.categories.Keys.CopyTo(keys, 0);
            foreach (string categoryName in keys)
            {
                this.DeleteCategory(categoryName);
            }

            keys = new string[feedsTable.Count];
            this.feedsTable.Keys.CopyTo(keys, 0);
            foreach (string feedUrl in keys) {
                this.DeleteFeed(feedUrl);             
            }          

            base.DeleteAllFeedsAndCategories();             
        }

        /// <summary>
        /// Removes all information related to a feed from the NewsHandler.   
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
                IFeed feed = feedManager.GetFeedByUrl(feedUrl) as IFeed;
                
                if (feed != null)
                {
                    feed.Delete();
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
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
            this.BootstrapAndLoadFeedlist(new feeds()); 
        }


        /// <summary>
        /// Used to recursively load a folder and its children (feeds and subfolders) into the FeedsTable and Categories collections. 
        /// </summary>
        /// <param name="folder2load">The folder to load</param>
        /// <param name="bootstrapFeeds">The RSS Bandit metadata about the feeds being loaded</param>
        /// <param name="bootstrapCategories">The RSS Bandit metadata about the folders/categories being loaded</param>
        private void LoadFolder(IFeedFolder folder2load, Dictionary<string, NewsFeed> bootstrapFeeds, Dictionary<string, INewsFeedCategory> bootstrapCategories)
        {

            if (folder2load != null)
            {
                IFeedsEnum Feeds = folder2load.Feeds as IFeedsEnum;
                IFeedsEnum Subfolders = folder2load.Subfolders as IFeedsEnum;

                if (Feeds.Count > 0)
                {
                    foreach (IFeed feed in Feeds)
                    {
                        Uri uri = null;

                        try
                        {
                            uri = new Uri(feed.DownloadUrl);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        string feedUrl = uri.CanonicalizedUri();
                        INewsFeed bootstrapFeed = (bootstrapFeeds.ContainsKey(feedUrl) ? bootstrapFeeds[feedUrl] : null);
                        this.feedsTable.Add(feedUrl, new WindowsRssNewsFeed(feed, bootstrapFeed));

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
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this NewsHandler</param>
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
                         
            feedManager.BackgroundSync(FEEDS_BACKGROUNDSYNC_ACTION.FBSA_ENABLE); 
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
            if (force_download)
            {
                this.feedManager.BackgroundSync(FEEDS_BACKGROUNDSYNC_ACTION.FBSA_RUNNOW);
            }
        }

        #endregion

    }

    #region WindowsRssNewsFeed 

    /// <summary>
    /// Represents a NewsFeed obtained from the Windows RSS platform
    /// </summary>
    class WindowsRssNewsFeed : INewsFeed, IDisposable
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
        public WindowsRssNewsFeed(IFeed feed) {
            if (feed == null) throw new ArgumentNullException("feed"); 
            this.myfeed = feed; 
        }

        /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="feed">The IFeed instance that this object will wrap</param>
        /// <param name="banditfeed">The object that contains the settings that will be used to initialize this class</param>
        public WindowsRssNewsFeed(IFeed feed, INewsFeed banditfeed): this(feed)
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

            if (!disposed)
            {
                Dispose(true);
            }

        }

        /// <summary>
        /// Disposes of the class
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
            lock (this)
            {                
                if (myfeed != null)
                {
                    Marshal.ReleaseComObject(myfeed);
                }
                System.GC.SuppressFinalize(this);
                disposed = true; 
            }
        }

        #endregion


        #region private fields

        /// <summary>
        /// Indicates that the object has been disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The actual IFeed instance that this object is wrapping
        /// </summary>
        private IFeed myfeed = null; 

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
                }
            }        
        }

        #endregion 

        #region INewsFeed implementation

        public string title
        {
            get { return myfeed.Title; }
          
            set
            {
              if(!StringHelper.EmptyTrimOrNull(value))
              { 
                  myfeed.Rename(value);
                  OnPropertyChanged("title");
              }
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlElement(DataType = "anyURI")]
        public string link
        {
            get { return myfeed.DownloadUrl; }

            set
            {
                /* can't set IFeed.DownloadUrl */
            }
        }

        /// <summary>
        /// Setting this value does nothing. 
        /// </summary>
        [XmlAttribute]
        public string id
        {
            get { return myfeed.LocalId; }

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


        /// <remarks/>
        [XmlElement("refresh-rate")]
        public int refreshrate { get; set; }
     
        /// <remarks/>
        [XmlIgnore]
        public bool refreshrateSpecified { get; set; }

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
                _storiesrecentlyviewed.Clear(); 
                IFeedsEnum items = myfeed.Items as IFeedsEnum;
                foreach (IFeedItem item in items)
                {
                    if (item.IsRead)
                    {
                        _storiesrecentlyviewed.Add(item.LocalId.ToString());
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




        private bool _downloadenclosures;
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
                return myfeed.IsList;
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


        //TODO: Make this a collection
        /// <remarks/>
        [XmlAttribute]
        public string category {

            get
            {
                IFeedFolder myfolder = myfeed.Parent as IFeedFolder;

                if (myfolder != null)
                {
                    return myfolder.Path;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (!StringHelper.EmptyTrimOrNull(value) && !value.Equals(this.category))
                {
                    owner.ChangeCategory(this, owner.AddCategory(value)); 
                }
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

        private bool _containsNewMessages;
        /// <remarks/>
        [XmlIgnore]
        public bool containsNewMessages
        {
            get
            {
                return _containsNewMessages;
            }

            set
            {
                if (!_containsNewMessages.Equals(value))
                {
                    _containsNewMessages = value;
                    this.OnPropertyChanged("containsNewMessages");
                }
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

        /// <remarks />                
        [XmlIgnore]
        public NewsHandler owner { get; set; }

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
    }

    #endregion

    #region WindowsRssNewsFeedCategory

    public class WindowsRssNewsFeedCategory : INewsFeedCategory, IDisposable 
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
        public WindowsRssNewsFeedCategory(IFeedFolder folder, INewsFeedCategory category) : this(folder)
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

        #endregion 

        #region private fields

        /// <summary>
        /// Indicates that the object has been disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The actual IFeedFolder instance that this object is wrapping
        /// </summary>
        private IFeedFolder myfolder = null;

        #endregion

        #region destructor and IDisposable implementation 

        /// <summary>
        /// Releases the associated COM objects
        /// </summary>
        /// <seealso cref="myfeed"/>
        ~WindowsRssNewsFeedCategory()
        {
            Dispose(false);           
        }

        /// <summary>
        /// Disposes of the class
        /// </summary>
        public void Dispose()
        {

            if (!disposed)
            {
                Dispose(true);
            }

        }

        /// <summary>
        /// Disposes of the class
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
            lock (this)
            {                
                if (myfolder != null)
                {
                    Marshal.ReleaseComObject(myfolder);
                }
                System.GC.SuppressFinalize(this);
                disposed = true; 
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
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            WindowsRssNewsFeedCategory c = obj as WindowsRssNewsFeedCategory;

            if (c == null)
            {
                return false;
            }

            return this.myfolder.Path.Equals(c.myfolder.Path);
        }

        /// <summary>
        /// Returns a hashcode for a category object. 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.myfolder.Path.GetHashCode();
        }

        #endregion
    }

    #endregion 
}
