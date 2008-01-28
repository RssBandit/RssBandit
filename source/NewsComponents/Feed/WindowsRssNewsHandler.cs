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

        #endregion 

        #region public methods

        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The category to add</param>
        public override INewsFeedCategory AddCategory(INewsFeedCategory cat)
        {
            return null;
        }

        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The name of the category</param>
        public override INewsFeedCategory AddCategory(string cat)
        {
            return null;
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
                if (!FeedsTable.ContainsKey(f.link))
                {
                    FeedsTable.Add(f.link, f);
                }
            }
            else
            {

            }

            return f;
        }

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
            IFeedFolder root = feedManager.RootFolder as IFeedFolder;
           
            if (root != null)
            {
                IFeedsEnum rootFeeds = root.Feeds as IFeedsEnum;
                IFeedsEnum rootFolders = root.Subfolders as IFeedsEnum;

                if (rootFeeds.Count > 0)
                {
                    foreach (IFeed feed in rootFeeds)
                    {

                        this.AddFeed(new WindowsRssNewsFeed(feed), null);
                    }
                }

                if (rootFolders.Count > 0)
                {
                    foreach (IFeedFolder folder in rootFolders)
                    {
                        this.AddCategory(new WindowsRssNewsFeedCategory(folder));
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

        }


        /* 
        /// <summary>
        /// Loads the RSS feedlist from the given URL and validates it against the schema. 
        /// </summary>
        /// <param name="feedListUrl">The URL of the feedlist</param>
        /// <param name="veh">The event handler that should be invoked on the client if validation errors occur</param>
        /// <exception cref="XmlException">XmlException thrown if XML is not well-formed</exception>
        public void LoadFeedlist(string feedListUrl, ValidationEventHandler veh)
        {
            // LoadFeedlist(AsyncWebRequest.GetSyncResponseStream(feedListUrl, null, this.UserAgent, this.Proxy), veh); 

            IFeeds fs = new FeedsClass();
            IFeedFolder root = fs.Subscriptions;

            if (root.Feeds.Count > 0)
            {
                foreach (IFeed f in root.Feeds)
                {
                    this.AddFeed(f, null, null);
                }
            }

            if (root.Subfolders.Count > 0)
            {
                foreach (IFeedFolder f in root.Subfolders)
                {
                    this.AddFolder(f, null, null, null);
                }
            }

        }

        /// <summary>
        /// Grabs the common feedlist and populates the input collections
        /// </summary>
        /// <param name="f"></param>
        /// <param name="c"></param>
        public void GetCurrentCommonFeedList(FeedsCollection feedList, CategoriesCollection catList)
        {

            IFeeds fs = new FeedsClass();
            IFeedFolder root = fs.Subscriptions;

            if (root.Feeds.Count > 0)
            {
                foreach (IFeed f in root.Feeds)
                {
                    this.AddFeed(f, null, feedList);
                }
            }

            if (root.Subfolders.Count > 0)
            {
                foreach (IFeedFolder f in root.Subfolders)
                {
                    this.AddFolder(f, null, feedList, catList);
                }
            }


        }

        /// <summary>
        /// Adds an IFeed instance to the subscription list
        /// </summary>
        /// <param name="feed">The feed to add</param>
        /// <param name="category">it's category</param>
        public void AddFeed(IFeed feed, string category, FeedsCollection feedList)
        {

            feedList = (feedList == null ? _feedsTable : feedList);

            feedsFeed f = new feedsFeed();
            f.link = feed.Url;
            f.title = feed.Name;
            f.category = category;

            if (feedList.Contains(f.link) == false)
            {
                feedList.Add(f.link, f);
            }
        }

        /// <summary>
        /// Adds an IFeed instance to the common feed list
        /// </summary>
        /// <param name="feed">The feed to add</param>
        /// <param name="category">it's category</param>
        public void AddFeed2CommonFeedList(feedsFeed feed, string category)
        {

            IFeeds fs = new FeedsClass();
            IFeedFolder folder = fs.Subscriptions;

            if (category != null)
            {
                string[] categoryPath = category.Split(new char[] { '\\' });

                foreach (string c in categoryPath)
                {
                    folder = folder.GetSubfolder(c);
                }
            }

            folder.CreateFeed(feed.title, feed.link);
        }

        /// Adds an IFeed instance to the common feed list
        /// </summary>
        /// <param name="feed">The feed to add</param>
        /// <param name="category">it's category</param>
        public void DeleteFeedFromCommonFeedList(feedsFeed feed, string category)
        {

            IFeeds fs = new FeedsClass();
            IFeedFolder folder = fs.Subscriptions;

            if (category != null)
            {
                string[] categoryPath = category.Split(new char[] { '\\' });

                foreach (string c in categoryPath)
                {
                    folder = folder.GetSubfolder(c);
                }
            }

            folder.GetFeed(feed.title).Delete();
        }

        /// <summary>
        /// Add a folder to the feedlist 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="category">The path to the folder</param>
        public void AddFolder(IFeedFolder folder, string path, FeedsCollection feedList, CategoriesCollection catList)
        {

            feedList = (feedList == null ? _feedsTable : feedList);
            catList = (catList == null ? this.categories : catList);

            string category = (path == null ? folder.Name : path + "\\" + folder.Name);

            if (folder.Feeds.Count > 0)
            {

                foreach (IFeed f in folder.Feeds)
                {
                    this.AddFeed(f, category, feedList);
                }

                if (!catList.ContainsKey(category))
                {
                    catList.Add(category);
                }
            }

            if (folder.Subfolders.Count > 0)
            {
                foreach (IFeedFolder f in folder.Subfolders)
                {
                    this.AddFolder(f, category, feedList, catList);
                }
            }
        }

*/ 
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
        public ReadOnlyICollection<string> storiesrecentlyviewed
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
                return new ReadOnlyICollection<string>(_storiesrecentlyviewed);
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
        public ReadOnlyICollection<string> deletedstories
        {
            get
            {
                return new ReadOnlyICollection<string>(_deletedstories);
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
                if (!StringHelper.EmptyTrimOrNull(value))
                {

                    if (owner.Categories.ContainsKey(value))
                    {
                        /* 
                         * STEP 1: Find corresponding category/folder 
                         * STEP 2: If it exists then move this IFeeds to that folder
                         */ 
                    }
                    else { //we need to create the folder 
                    
                        /* 
                         * STEP 1: Create the folder 
                         * STEP 2: Add category object to categories object in owner
                         * STEP 3: Move IFeed object to the new  folder
                         */ 
                    }
                }//if(!StringBuilder.EmptryTrimOrNull(..))
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
