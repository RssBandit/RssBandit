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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using log4net;

using RssBandit.Common;
using RssBandit.Common.Logging;

using NewsComponents.Collections;
using NewsComponents.Net;
using NewsComponents.Utils;

namespace NewsComponents.Feed
{
    #region INewsGatorFeedSource interface: public FeedSource extensions
    /// <summary>
    /// public FeedSource extension offered by NewsGator Feed Source
    /// </summary>
    public interface INewsGatorFeedSource
    {
        /// <summary>
        /// Marks an item as clipped or unclipped in NewsGator Online
        /// </summary>
        ///<param name="item">The item to clip. </param>      
        void ClipNewsItem(INewsItem item);

        /// <summary>
        /// Gets true, if the item is clipped, else false.
        /// </summary>
        /// <param name="item">The news item.</param>
        /// <returns>bool</returns>
        bool NewsItemClipped(INewsItem item);
    }
    #endregion

    #region NewsGatorFlagStatus 

    /// <summary>
    /// Uses same values as PR_FLAG_ICON enumeration used by Outlook 
    /// </summary>
    public enum NewsGatorFlagStatus
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// Red 
        /// </summary>
        FollowUp = 6, 
        /// <summary>
        /// Blue
        /// </summary>
        Forward = 5,  
        /// <summary>
        /// Green
        /// </summary>
        Read = 3,   
        /// <summary>
        /// Yellow
        /// </summary>
        Review = 4,
        /// <summary>
        /// Purple
        /// </summary>
        Reply = 1, 
        /// <summary>
        /// 
        /// </summary>
        Complete = 1000
    }

    #endregion 

    
    #region NewsGatorItemState

    /// <summary>
    /// Indicates the state of a news item in NewsGator Online
    /// </summary>
    internal enum NewsGatorItemState
    {
        Read, Unread, Deleted
    }

    #endregion 

    #region NewsGatorFeedSource

    /// <summary>
    /// A FeedSource that retrieves user subscriptions and feeds from NewsGator Online. 
    /// </summary>
    internal class NewsGatorFeedSource : FeedSource, INewsGatorFeedSource
    {

        #region private fields


        // logging/tracing:
        private static readonly ILog _log = DefaultLog.GetLogger(typeof(NewsGatorFeedSource));


        /// <summary>
        /// The Newsgator Online sync token.
        /// </summary>
        private string NgosSyncToken; 
     
        /// <summary>
        /// Place holder that is provided as RSS feed link when no URL found in NewsGator Online feed list for a feed 
        /// </summary>
        private static readonly string NoXmlUrlFoundInOpml = "http://www.example.com/no-url-for-rss-feed-provided-in-imported-opml";

        /// <summary>
        /// The NewsGator API key for RSS Bandit
        /// </summary>
        private static readonly string NgosProductKey = "7AF62582A5334A9CADF967818E734558";

        /// <summary>
        /// HTTP header containing the NewsGator API key. Needed for every Web request to NewsGator Online. 
        /// </summary>
        private static readonly WebHeaderCollection NgosTokenHeader = new WebHeaderCollection(); 

        /// <summary>
        /// The name for this location. Used for identifying synchronization endpoints in the NewsGator Online UI
        /// </summary>
        private static readonly string NgosLocationName = "RssBandit-" + Environment.MachineName;
        
        /// <summary>
        /// Service end point for the NewsGator Location REST API
        /// </summary>
        private static readonly string LocationApiUrl = "http://services.newsgator.com/ngws/svc/Location.aspx";

        /// <summary>
        /// Service end point for the NewsGator Subscription REST API
        /// </summary>
        private static readonly string SubscriptionApiUrl = "http://services.newsgator.com/ngws/svc/Subscription.aspx";

        /// <summary>
        /// Service end point for the NewsGator Feed REST API
        /// </summary>
       private static readonly string FeedApiUrl =  "http://services.newsgator.com/ngws/svc/Feed.aspx"; 

        /// <summary>
        /// Service end point for the NewsGator PostItem REST API
        /// </summary>
       private static readonly string PostItemApiUrl = "http://services.newsgator.com/ngws/svc/PostItem.aspx";

       /// <summary>
       /// Service end point for the NewsGator Folder REST API
       /// </summary>
       private static readonly string FolderApiUrl = "http://services.newsgator.com/ngws/svc/Folder.aspx";

       /// <summary>
       /// The XML namespace for NewsGator RSS extensions
       /// </summary>
       private static readonly string NewsGatorRssNS = "http://newsgator.com/schema/extensions";

       /// <summary>
       /// The XML namespace for NewsGator OPML extensions
       /// </summary>
       private static readonly string NewsGatorOpmlNS = "http://newsgator.com/schema/opml"; 

       /// <summary>
       /// Updates NewsGator Online in a background thread.
       /// </summary>
       private static NewsGatorModifier newsGatorUpdater;


        #endregion 

        #region constructors

        /// <summary>
        /// Static constructor
        /// </summary>
       static NewsGatorFeedSource()
       {
           NgosTokenHeader.Add("X-NGAPIToken", NgosProductKey); 
       }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSource"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="location">The user credentials</param>
        internal NewsGatorFeedSource(INewsComponentsConfiguration configuration, SubscriptionLocation location)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = FeedSource.DefaultConfiguration;

            this.location = location;

            //register with background thread for updating Newsgator Online
            NewsGatorUpdater.RegisterFeedSource(this);
           
            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);

            this.rssParser = new RssParser(this);
 
            if (String.IsNullOrEmpty(EnclosureFolder))
            {
                this.enclosureDownloader = new BackgroundDownloadManager(this);
                this.enclosureDownloader.DownloadCompleted += this.OnEnclosureDownloadComplete;
            }

            this.AsyncWebRequest = new AsyncWebRequest();
            this.AsyncWebRequest.OnAllRequestsComplete += this.OnAllRequestsComplete;                   
        }
               

        #endregion 


        #region public properties

        /// <summary>
        /// Returns the NewsGator username associated with this feed source
        /// </summary>
        public string NewsGatorUserName
        {
            get { return this.location.Credentials.UserName; }
        }


        /// <summary>
        /// Gets or sets the NewsGator modifier modifier
        /// </summary>
        /// <value>The NewsGatorModifier instance used by all instances of this class.</value>
        internal NewsGatorModifier NewsGatorUpdater
        {
            get
            {
                if (newsGatorUpdater == null)
                {
                    newsGatorUpdater = new NewsGatorModifier(this.Configuration.UserApplicationDataPath);
                }
                return newsGatorUpdater;
            }
            set
            {
                newsGatorUpdater = value;
            }
        }

        #endregion 

        #region public methods

        #region feed list methods




        /// <summary>
        /// Saves the feed list to the specified stream. The feed is written in 
        /// the RSS Bandit feed file format as described in feeds.xsd
        /// </summary>
        public override void SaveFeedList()
        {
            base.SaveFeedList();
            NewsGatorUpdater.SavePendingOperations();
        }        

        /// <summary>
        /// Transfers settings from a local RSS Bandit feed to the input NewsGator feed
        /// </summary>
        /// <param name="ngFeed">The NewsGator feed</param>
        /// <param name="banditfeed">The RSS Bandit feed</param>
        /// <returns>The modified NewsGator feed</returns>
        private static NewsFeed TransferSettings(NewsFeed ngFeed, NewsFeed banditfeed)
        {
            if (banditfeed != null)
            {
                ngFeed.lastretrievedSpecified = banditfeed.lastretrievedSpecified;
                ngFeed.lastretrieved = banditfeed.lastretrieved;
                ngFeed.lastmodifiedSpecified = banditfeed.lastmodifiedSpecified;
                ngFeed.lastmodified = banditfeed.lastmodified;
                ngFeed.id = banditfeed.id;
                ngFeed.enclosurefolder = banditfeed.enclosurefolder;
                ngFeed.deletedstories = banditfeed.deletedstories;
                ngFeed.storiesrecentlyviewed = banditfeed.storiesrecentlyviewed;
                ngFeed.refreshrate = banditfeed.refreshrate;
                ngFeed.refreshrateSpecified = banditfeed.refreshrateSpecified;
                ngFeed.maxitemage = banditfeed.maxitemage;
                ngFeed.markitemsreadonexit = banditfeed.markitemsreadonexit;
                ngFeed.markitemsreadonexitSpecified = banditfeed.markitemsreadonexitSpecified;
                ngFeed.listviewlayout = banditfeed.listviewlayout;
                ngFeed.favicon = banditfeed.favicon;
                ngFeed.stylesheet = banditfeed.stylesheet;
                ngFeed.cacheurl = banditfeed.cacheurl;
                ngFeed.enclosurealert = banditfeed.enclosurealert;
                ngFeed.enclosurealertSpecified = banditfeed.enclosurealertSpecified;
                ngFeed.alertEnabled = banditfeed.alertEnabled;
                ngFeed.alertEnabledSpecified = banditfeed.alertEnabledSpecified; 
            }

            return ngFeed;
        }

        /// <summary>
        /// Creates the location for this synchronization endpoint in NewsGator Online
        /// </summary>
        private void CreateLocation()
        {
            opml location = new opml();
            location.body = new opmloutline[1];
            opmloutline outline = new opmloutline();
            outline.text = NgosLocationName;
            location.body[0] = outline;

            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(opml));
            StringBuilder sb = new StringBuilder();

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;

            serializer.Serialize(XmlWriter.Create(sb, xws), location);

            try
            {
                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(LocationApiUrl, sb.ToString(), this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _log.Debug(String.Format("Error occured when creating location in NewsGator Online: {0}-{1}", response.StatusCode, response.StatusDescription));
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }
            }
            catch (Exception e)
            {
                _log.Error("Error occured when creating location in NewsGator Online:", e);
            }
        }


        /// <summary>
        /// Loads the user's feed list from NewsGator Online. In addition, sets the current NgosSyncToken property 
        /// </summary>
        /// <returns>The feed list from NewsGator Online</returns>
        private feeds LoadFeedlistFromNewsGatorOnline()
        {
            //first step is to create Location, we do this in case this is first time syncing from here or it got deleted somehow
            this.CreateLocation();

            string feedlistUrl = SubscriptionApiUrl + "/" + NgosLocationName;

            //load feed list XML
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(AsyncWebRequest.GetSyncResponseStream(feedlistUrl, this.location.Credentials, this.Proxy, NgosTokenHeader)));

            XmlNode tokenNode = doc.DocumentElement.Attributes.GetNamedItem("token", "http://newsgator.com/schema/opml");
            if (tokenNode != null)
            {
                NgosSyncToken = tokenNode.Value;
            }

            /* convert OPML feeds list to RSS Bandit data structures */          
            XmlNodeReader reader = new XmlNodeReader(this.ConvertFeedList(doc));
            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(feeds));
            feeds ngFeeds = (feeds)serializer.Deserialize(reader);
            reader.Close();

            return ngFeeds;
        }

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
              feeds myFeeds = null;
              if (File.Exists(this.location.Location))
              {
                  //load Bandit subscriptions.xml document into memory
                  XmlReader reader = XmlReader.Create(this.location.Location);
                  XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(feeds));
                  myFeeds = (feeds)serializer.Deserialize(reader);
                  reader.Close();
              }
              else
              {
                  myFeeds = new feeds(); 
              }

            //load feed list from NewsGator Online and use settings from subscriptions.xml
            this.BootstrapAndLoadFeedlist(myFeeds);
            NewsGatorUpdater.StartBackgroundThread(); 
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
            Dictionary<string, category> bootstrapCategories = new Dictionary<string, category>();
           
            foreach (NewsFeed f in feedlist.feed)
            {
                bootstrapFeeds.Add(f.link, f);
            }

            foreach (category c in feedlist.categories)
            {
                bootstrapCategories.Add(c.Value, c);
            }

            //matchup feeds from NewsGator Online with local feeds
            feeds ngFeeds = (Offline ? feedlist : this.LoadFeedlistFromNewsGatorOnline());

            foreach (NewsFeed ngFeed in ngFeeds.feed)
            {
                if (!ngFeed.link.Equals(NoXmlUrlFoundInOpml) && !feedsTable.ContainsKey(ngFeed.link))
                {
                    NewsFeed feed = null;
                    bootstrapFeeds.TryGetValue(ngFeed.link, out feed);
                    feed = TransferSettings(ngFeed, feed);
                    feed.owner = this; 

                    feed.PropertyChanged += this.OnNewsFeedPropertyChanged; 

                    this.feedsTable.Add(ngFeed.link, feed);
                }
            }

            foreach (category ngCategory in ngFeeds.categories)
            {
                category cat = null;
                bootstrapCategories.TryGetValue(ngCategory.Value, out cat);
                cat = cat ?? ngCategory;
                cat.AnyAttr = ngCategory.AnyAttr; //make sure we get ng:folderId from NewsGator Online
                this.categories.Add(ngCategory.Value, cat);
            }

            // copy over list view layouts 
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
            //}//if(feedlist.listviewLayouts != null)

        }


        /// <summary>
        /// This method fetches the feed list from Google Reader, figures out the differences between it and our local feed list, 
        /// then applies the changes. 
        /// </summary>
        private void UpdateFeedList()
        {
            bool feedlistModified = false, categoriesModified = false;

            feeds ngFeeds = this.LoadFeedlistFromNewsGatorOnline();

            var addedFeeds = from feed in ngFeeds.feed
                             where !feedsTable.ContainsKey(feed.link)
                             select feed;

            foreach (NewsFeed newFeed in addedFeeds)
            {
                if (!newFeed.link.Equals(NoXmlUrlFoundInOpml))
                {                    
                    newFeed.owner = this;
                    newFeed.PropertyChanged += this.OnNewsFeedPropertyChanged;
                   
                    this.feedsTable.Add(newFeed.link, newFeed);
                    RaiseOnAddedFeed(new FeedChangedEventArgs(newFeed.link));
                    feedlistModified = true;
                }
              
            }

            var removedFeeds = from url in feedsTable.Keys
                               where !ngFeeds.feed.Any(ngf => ngf.link == url)
                               select url;

            string[] deletedFeeds = removedFeeds.ToArray(); //prevent InvalidOperationException due to modifying feedsTable

            foreach (string deletedFeed in deletedFeeds)
            {
                INewsFeed f = null;
                if (feedsTable.TryGetValue(deletedFeed, out f))
                {
                    this.feedsTable.Remove(deletedFeed);
                    RaiseOnDeletedFeed(new FeedDeletedEventArgs(f.link, f.title));
                    feedlistModified = true;
                }
            }

            var movedFeeds = from ngfeed in ngFeeds.feed
                             where feedsTable.ContainsKey(ngfeed.link) 
                                && !ngfeed.categories.Any(c => String.Compare(c, feedsTable[ngfeed.link].category, true /* ignoreCase */) == 0)
                             select ngfeed;

            foreach (NewsFeed relocatedFeed in movedFeeds)
            {
                INewsFeed f = null;
                if (feedsTable.TryGetValue(relocatedFeed.link, out f))
                {
                    f.categories.Clear();
                    f.categories.AddRange(relocatedFeed.categories);
                    RaiseOnMovedFeed(new FeedMovedEventArgs(f.link, f.category));
                    categoriesModified = true;
                }
            }

            if (feedlistModified)
            {
                readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(feedsTable);
            }

            if (categoriesModified)
            {
                readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(categories);
            }
        }

        #endregion 

        #region feed downloading methods


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
            if (!Offline)
            {
                try
                {
                    this.UpdateFeedList();
                }
                catch (WebException we)
                {
                    _log.Error(we.Message, we);
                }
            }
            base.RefreshFeeds(force_download);
        }

        /// <summary>
        /// Retrieves the RSS feed for a particular subscription then converts 
        /// the blog posts or articles to an arraylist of items. The http requests are async calls.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to download or the Google Reader URL of the feed (if the 
        /// continuationToken parameter is set)</param>
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

            string etag = null;
            bool requestQueued = false;

            int priority = 10;
            if (forceDownload)
                priority += 100;
            if (manual)
                priority += 1000;

            Uri feedUri = new Uri(feedUrl);
            Uri reqUri = feedUri;

            //is this a follow on request to a feed download.            

            try
            {
                try
                {
                    if ((!forceDownload) || Offline)
                    {
                        GetCachedItemsForFeed(feedUri.CanonicalizedUri()); //load feed into itemsTable
                        RaiseOnUpdatedFeed(feedUri, null, RequestResult.NotModified, priority, false);
                        return false;
                    }
                }
                catch (XmlException xe)
                {
                    //cache file is corrupt
                    Trace("Unexpected error retrieving cached feed '{0}': {1}", feedUrl, xe.ToDescriptiveString());
                }

                //We need a reference to the feed so we can see if a cached object exists
                NewsFeed theFeed = null;
                if (feedsTable.ContainsKey(feedUri.CanonicalizedUri()))
                    theFeed = feedsTable[feedUri.CanonicalizedUri()] as NewsFeed;

                if (theFeed == null || theFeed.Any == null)
                    return false;

                XmlElement syncXmlUrl = theFeed.Any.FirstOrDefault(elem => elem.LocalName == "syncXmlUrl");

                if (syncXmlUrl == null) //newly added feed? 
                {
                    return false;
                }

                string requestUrl = syncXmlUrl.InnerText;
                reqUri = new Uri(requestUrl + "?unread=False"); 

                // only if we "real" go over the wire for an update:
                RaiseOnUpdateFeedStarted(feedUri, forceDownload, priority);

                //DateTime lastRetrieved = DateTime.MinValue; 
                DateTime lastModified = DateTime.MinValue;

                if (itemsTable.ContainsKey(feedUrl) && !manual)
                {
                    etag = theFeed.etag;
                    lastModified = (theFeed.lastretrievedSpecified ? theFeed.lastretrieved : theFeed.lastmodified);
                }

                ICredentials c = location.Credentials;

                RequestParameter reqParam =
                    RequestParameter.Create(reqUri, this.UserAgent, this.Proxy, c, lastModified, etag);
                // global cookie handling:
                reqParam.SetCookies = false;
                //set X-NGAPIToken header
                reqParam.Headers = NgosTokenHeader; 

                AsyncWebRequest.QueueRequest(reqParam,
                                             OnRequestStart,
                                             OnRequestComplete,
                                             OnRequestException, priority);

                requestQueued = true;
            }
            catch (Exception e)
            {
                Trace("Unexpected error on QueueRequest(), processing feed '{0}': {1}", feedUrl, e.ToDescriptiveString());
                RaiseOnUpdateFeedException(feedUrl, e, priority);
            }

            return requestQueued;
        }


        /// <summary>
        /// Called when a network request has been made to start downloading a feed. 
        /// </summary>
        /// <param name="requestUri">The URL being requested</param>
        /// <param name="cancel">Whether the request is to be cancelled</param>
        protected override void OnRequestStart(Uri requestUri, ref bool cancel)
        {
            //find the feed that has the requestUri as it's syncXmlUrl
            string syncUrl = requestUri.CanonicalizedUri().Substring(0, requestUri.CanonicalizedUri().LastIndexOf("?unread=False"));
            string feedUrl = feedsTable.FirstOrDefault(kvp =>
                                kvp.Value.Any.FirstOrDefault(elem => elem.LocalName == "syncXmlUrl").InnerText.Equals(syncUrl)).Key;

            if (!StringHelper.EmptyTrimOrNull(feedUrl))
            {
                Uri feedUri = new Uri(feedUrl);
                base.OnRequestStart(feedUri, ref cancel);
            }
        }

         /// <summary>
        /// Called when an exception occurs while downloading a feed.
        /// </summary>
        /// <param name="requestUri">The URI of the feed</param>
        /// <param name="e">The exception</param>
        /// <param name="priority">The priority of the request</param>
        protected override void OnRequestException(Uri requestUri, Exception e, int priority)
        {
            //find the feed that has the requestUri as it's syncXmlUrl
            string syncUrl = requestUri.CanonicalizedUri().Substring(0, requestUri.CanonicalizedUri().LastIndexOf("?unread=False"));
            string feedUrl = feedsTable.FirstOrDefault(kvp =>
                                kvp.Value.Any.FirstOrDefault(elem => elem.LocalName == "syncXmlUrl").InnerText.Equals(syncUrl)).Key;

            if (!StringHelper.EmptyTrimOrNull(feedUrl))
            {
                Uri feedUri = new Uri(feedUrl);
                base.OnRequestException(feedUri, e, priority);
            }
        }

        /// <summary>
        /// Called on successful completion of a Web request for a feed
        /// </summary>
        /// <param name="requestUri">The request URI</param>
        /// <param name="responseStream">The response stream.</param>
        /// <param name="response">The original Response</param>
        /// <param name="newUri">The new URI of a 3xx HTTP response was originally received</param>
        /// <param name="eTag">The etag</param>
        /// <param name="lastModified">The last modified date of the result</param>
        /// <param name="result">The HTTP result</param>
        /// <param name="priority">The priority of the request</param>
        protected override void OnRequestComplete(Uri requestUri, Stream responseStream, WebResponse response, Uri newUri, string eTag, DateTime lastModified,
                                    RequestResult result, int priority)
        {
            //find the feed that has the requestUri as it's syncXmlUrl
            string syncUrl = requestUri.CanonicalizedUri().Substring(0, requestUri.CanonicalizedUri().LastIndexOf("?unread=False")); 
            string feedUrl = feedsTable.FirstOrDefault(kvp =>
                                kvp.Value.Any.FirstOrDefault(elem => elem.LocalName == "syncXmlUrl").InnerText.Equals(syncUrl)).Key;
            Uri feedUri = new Uri(feedUrl);

          

            Trace("AsyncRequest.OnRequestComplete: '{0}': {1}", requestUri.ToString(), result);
            if (newUri != null)
                Trace("AsyncRequest.OnRequestComplete: perma redirect of '{0}' to '{1}'.", requestUri.ToString(),
                      newUri.ToString());

            IList<INewsItem> itemsForFeed;

            //BUGBUG: This value is now incorrectly returned if feed has more than 50 items 
            bool firstSuccessfulDownload = false;

            //we download feeds 50 items at a time, so it may take multiple requests to get all items
            bool feedDownloadComplete = true;            

            //grab items from feed, then save stream to cache. 
            try
            {
                //We need a reference to the feed so we can see if a cached object exists
                INewsFeed theFeed = null;

                if (!feedsTable.TryGetValue(feedUri.CanonicalizedUri(), out theFeed))
                {
                    Trace("ATTENTION! FeedsTable[requestUri] as NewsFeed returns null for: '{0}'",
                          requestUri.ToString());
                    return;
                }

                feedUrl = theFeed.link;
                if (true)
                {
                    if (String.Compare(feedUrl, feedUri.CanonicalizedUri(), true) != 0)
                        Trace("feed.link != requestUri: \r\n'{0}'\r\n'{1}'", feedUrl, requestUri.CanonicalizedUri());
                }

                if (newUri != null)
                {
                    // Uri changed/moved permanently

                    lock (feedsTable)
                    {
                        feedsTable.Remove(feedUrl);
                        theFeed.link = newUri.CanonicalizedUri();
                        this.feedsTable.Add(theFeed.link, theFeed);
                    }

                    lock (itemsTable)
                    {
                        if (itemsTable.ContainsKey(feedUrl))
                        {
                            IFeedDetails FI = itemsTable[feedUrl];
                            itemsTable.Remove(feedUrl);
                            itemsTable.Remove(theFeed.link); //remove any old cached versions of redirected link
                            itemsTable.Add(theFeed.link, FI);
                        }
                    }

                    feedUrl = theFeed.link;
                } // newUri

                if (result == RequestResult.OK)
                {
                    //Update our recently read stories. This is very necessary for 
                    //dynamically generated feeds which always return 200(OK) even if unchanged							

                    IInternalFeedDetails fi = RssParser.GetItemsForFeed(theFeed, responseStream, false /* cachedStream */, false /* markitemsread */);
                    IInternalFeedDetails fiFromCache = null;

                    // Sometimes we may not have loaded feed from cache. So ensure it is 
                    // loaded into memory if cached. We don't lock here because loading from
                    // disk is too long a time to hold a lock.  
                    try
                    {
                        if (!itemsTable.ContainsKey(feedUrl))
                        {
                            fiFromCache = this.GetFeed(theFeed);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace("this.GetFeed(theFeed) caused exception: {0}", ex.ToDescriptiveString());
                        /* the cache file may be corrupt or an IO exception 
                         * not much we can do so just ignore it 
                         */
                    }

                    IList<INewsItem> newReceivedItems = null;

                    //Merge items list from cached copy of feed with this newly fetched feed. 
                    //Thus if a feed removes old entries (such as a news site with daily updates) we 
                    //don't lose them in the aggregator. 
                    lock (itemsTable)
                    {
                        //TODO: resolve time consuming lock to hold only a short time!!!

                        //if feed was in cache but not in itemsTable we load it into itemsTable
                        if (!itemsTable.ContainsKey(feedUrl) && (fiFromCache != null))
                        {
                            itemsTable.Add(feedUrl, fiFromCache);
                        }

                        if (itemsTable.ContainsKey(feedUrl))
                        {
                            IFeedDetails fi2 = itemsTable[feedUrl];

                            if (RssParser.CanProcessUrl(feedUrl))
                            {
                                var newItems = MergeAndPurgeItems(fi2.ItemsList, fi.ItemsList, theFeed.deletedstories,
                                                                  out newReceivedItems, theFeed.replaceitemsonrefresh,
                                                                  false /* respectOldItemState */);

                                fi.ReplaceItems(newItems);
                            }

                            /*
                             * HACK: We have an issue that OnRequestComplete is sometimes passed a response Stream 
                             * that doesn't match the requestUri. We insert a test here to see if this has occured
                             * and if so we return from this method.  
                             * 
                             * We are careful here to ensure we don't treat a case of the feed or website being moved 
                             * as an instance of this bug. We do this by (1) test to see if website URL in feed just 
                             * downloaded matches the site URL in the feed from the cache AND (2) if all the items in
                             * the feed we just downloaded were never in the cache AND (3) the site URL is the same for
                             * the site URL for another feed we have in the cache. 
                             */
                            if ((String.Compare(fi2.Link, fi.Link, true) != 0) &&
                                (newReceivedItems.Count == fi.ItemsList.Count))
                            {
                                foreach (IInternalFeedDetails fdi in itemsTable.Values)
                                {
                                    if (String.Compare(fdi.Link, fi.Link, true) == 0)
                                    {
                                        RaiseOnUpdatedFeed(requestUri, null, RequestResult.NotModified, priority, false);
                                        _log.Error(
                                            String.Format(
                                                "Feed mixup encountered when downloading {2} because fi2.link != fi.link: {0}!= {1}",
                                                fi2.Link, fi.Link, requestUri.CanonicalizedUri()));
                                        return;
                                    }
                                } //foreach
                            }

                            itemsTable.Remove(feedUrl);
                        }
                        else
                        {
                            //if(itemsTable.ContainsKey(feedUrl)){ means this is a newly downloaded feed
                            firstSuccessfulDownload = true;
                            newReceivedItems = fi.ItemsList;
                            RelationCosmosAddRange(newReceivedItems);
                        }

                        itemsTable.Add(feedUrl, fi);
                    } //lock(itemsTable)					    

                    //if(eTag != null){	// why we did not store the null?
                    theFeed.etag = eTag;
                    //}

                    if (lastModified > theFeed.lastmodified)
                    {
                        theFeed.lastmodified = lastModified;
                    }

                    theFeed.cacheurl = this.SaveFeed(theFeed);
                    SearchHandler.IndexAdd(newReceivedItems); // may require theFeed.cacheurl !

                    theFeed.causedException = false;
                    itemsForFeed = fi.ItemsList;

                    /* download podcasts from items we just received if downloadenclosures == true */
                    if (this.GetDownloadEnclosures(theFeed.link))
                    {
                        int numDownloaded = 0;
                        int maxDownloads = (firstSuccessfulDownload
                                                ? NumEnclosuresToDownloadOnNewFeed
                                                : DefaultNumEnclosuresToDownloadOnNewFeed);

                        if (newReceivedItems != null)
                            foreach (NewsItem ni in newReceivedItems)
                            {
                                //ensure that we don't attempt to download these enclosures at a later date
                                if (numDownloaded >= maxDownloads)
                                {
                                    MarkEnclosuresDownloaded(ni);
                                    continue;
                                }

                                try
                                {
                                    numDownloaded += this.DownloadEnclosure(ni, maxDownloads - numDownloaded);
                                }
                                catch (DownloaderException de)
                                {
                                    _log.Error("Error occured when downloading enclosures in OnRequestComplete():", de);
                                }
                            }
                    }

                    /* Make sure read stories are accurately calculated */
                    theFeed.containsNewMessages = false;
                    theFeed.storiesrecentlyviewed.Clear();

                    foreach (NewsItem ri in itemsForFeed)
                    {
                        if (ri.BeenRead)
                        {
                            theFeed.AddViewedStory(ri.Id);
                        }

                        if (ri.HasNewComments)
                        {
                            theFeed.containsNewComments = true;
                        }

                        /* Set event handler for handling read state changes. 
                         * First unset it on old items so we don't add same event handler twice. 
                         */
                        ri.PropertyChanged -= this.OnNewsItemPropertyChanged;
                        ri.PropertyChanged += this.OnNewsItemPropertyChanged;
                    }


                    if (itemsForFeed.Count > theFeed.storiesrecentlyviewed.Count)
                    {
                        theFeed.containsNewMessages = true;
                    }

                    //we're done downloading items from the feed
                    theFeed.lastretrieved = new DateTime(DateTime.Now.Ticks);
                    theFeed.lastretrievedSpecified = true;

                }
                else if (result == RequestResult.NotModified)
                {
                    // expected behavior: response == null, if not modified !!!
                    theFeed.lastretrieved = new DateTime(DateTime.Now.Ticks);
                    theFeed.lastretrievedSpecified = true;
                    theFeed.causedException = false;

                    //IInternalFeedDetails feedInfo = itemsTable[feedUrl];
                    //if (feedInfo != null)
                    //    itemsForFeed = feedInfo.ItemsList;
                    //else
                    //    itemsForFeed = EmptyItemList;
                    // itemsForFeed wasn't used anywhere else
                }
                else
                {
                    throw new NotImplementedException("Unhandled RequestResult: " + result);
                }

                //only alert UI when we have downloaded all items from Google Reader which may take multiple downloads of the
                //Atom feed since we download 50 items at a time. 
                if (feedDownloadComplete)
                {
                    RaiseOnUpdatedFeed(feedUri, newUri, result, priority, firstSuccessfulDownload);
                }
            }
            catch (Exception e)
            {
                string key = requestUri.CanonicalizedUri();
                if (feedsTable.ContainsKey(key))
                {
                    Trace("AsyncRequest.OnRequestComplete('{0}') Exception: ", requestUri.ToString(), e.StackTrace);
                    INewsFeed f = feedsTable[key];
                    // now we set this within causedException prop.:
                    //f.lastretrieved = DateTime.Now; 
                    //f.lastretrievedSpecified = true; 
                    f.causedException = true;
                }
                else
                {
                    Trace("AsyncRequest.OnRequestComplete('{0}') Exception on feed not contained in FeedsTable: ",
                          requestUri.ToString(), e.StackTrace);
                }

                RaiseOnUpdateFeedException(requestUri.CanonicalizedUri(), e, priority);
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Close();
            }
        }

        #endregion 

        #region news item manipulation methods


        /// <summary>
        /// Marks an item as clipped or unclipped in NewsGator Online
        /// </summary>
        ///<param name="item">The item to clip or unclip</param>
       public void ClipNewsItem(INewsItem item)
        {
            if (item != null)
            {

                bool clipped = true;
                XmlQualifiedName qname = new XmlQualifiedName("clipped", "http://newsgator.com/schema/extensions");
                XmlElement clippedElem = RssHelper.GetOptionalElement(item, qname);
                if (clippedElem == null)
                {
                    clippedElem = RssHelper.CreateXmlElement("ng", qname, "True");
                }
                else
                {
                    clipped = clippedElem.InnerText != "True";
                    clippedElem.InnerText = clippedElem.InnerText == "True" ? "False" : "True";
                    item.OptionalElements.Remove(qname); 
                }

                item.OptionalElements.Add(qname, clippedElem.OuterXml);               
                XmlElement idElem = RssHelper.GetOptionalElement(item, "postId", NewsGatorRssNS);
                if (idElem != null)
                {
                    NewsGatorUpdater.ChangeItemClippedStateInNewsGatorOnline(this.NewsGatorUserName, idElem.InnerText, clipped);

                }
            }
        }

        /// <summary>
        /// Gets true, if the item is clipped, else false.
        /// </summary>
        /// <param name="item">The news item.</param>
        /// <returns>bool</returns>
        public bool NewsItemClipped(INewsItem item)
        {
            bool clipped = false;

            if (item != null)
            {
                XmlQualifiedName qname = new XmlQualifiedName("clipped", "http://newsgator.com/schema/extensions");
                XmlElement clippedElem = RssHelper.GetOptionalElement(item, qname);
                if (clippedElem != null)
                {
                    clipped = clippedElem.InnerText == "True";
                }
            }

            return clipped; 
        }

        /// <summary>
        /// Used to clip or unclip a post in NewsGator Online
        /// </summary>
        /// <param name="itemId">The ID of the news item to clip or unclip</param>
        /// <param name="clipped">Indicates whether the item is being clipped or unclipped</param>
        internal void ChangeItemClippedStateInNewsGatorOnline(string itemId, bool clipped)
        {
            if (!StringHelper.EmptyTrimOrNull(itemId))
            {

                string clipApiUrl = PostItemApiUrl + (clipped ? "/clipposts" : "/unclipposts");

                string bodyTemplate = "<clippings> <item><postid>{0}</postid>{1}</item> </clippings>";
                string clippingFolder = "<folderid>0</folderid>";
                string body = String.Format(bodyTemplate, itemId, clipped ? clippingFolder : String.Empty);

                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(clipApiUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }
            }

        }


         /// <summary>
        /// Invoked when a NewsFeed owned by this FeedSource changes in a way that 
        /// needs to be communicated to NewsGator Online. 
        /// </summary>
        /// <param name="sender">the NewsFeed</param>
        /// <param name="e">information on the property that changed</param>
        protected override void OnNewsFeedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NewsFeed feed = sender as NewsFeed;

            if (feed == null)
            {
                return;
            }


            switch (e.PropertyName)
            {
                case "title":
                    NewsGatorUpdater.RenameFeedInNewsGatorOnline(this.NewsGatorUserName, feed.link, feed.title);
                    break;               
            }

        }

        /// <summary>
        /// Invoked when a NewsItem owned by this FeedSource changes in a way that 
        /// needs to be communicated to NewsGator Online. 
        /// </summary>
        /// <param name="sender">the NewsItem</param>
        /// <param name="e">information on the property that changed</param>
        protected override void OnNewsItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NewsItem item = sender as NewsItem;

            if (item == null)
            {
                return;
            }

            XmlElement elem = RssHelper.GetOptionalElement(item, "postId", NewsGatorRssNS);
            if (elem != null)
            {
                switch (e.PropertyName)
                {
                    case "BeenRead":
                        NewsGatorUpdater.ChangeItemStateInNewsGatorOnline(this.NewsGatorUserName, elem.InnerText,
                            item.BeenRead ? NewsGatorItemState.Read : NewsGatorItemState.Unread);
                        break;

                    case "FlagStatus":
                        NewsGatorUpdater.ChangeItemStateInNewsGatorOnline(this.NewsGatorUserName, elem.InnerText, 
                            item.Feed.link, (NewsGatorFlagStatus) Enum.Parse(typeof(NewsGatorFlagStatus), item.FlagStatus.ToString())
                            ); 
                        break;

                }
            }
        }



        /// <summary>
        /// Marks an item as flagged or unflagged in NewsGator Online
        /// </summary>
        /// <param name="itemId">The NewsGator ID of the news item</param>        
        /// <param name="feedUrl">The URL of the feed the item belongs to</param>
        /// <param name="state">Indicates the flag status of the item</param>
        internal void ChangeItemStateInNewsGatorOnline(string itemId, string feedUrl, NewsGatorFlagStatus state)
        {
            if (!StringHelper.EmptyTrimOrNull(itemId) && !StringHelper.EmptyTrimOrNull(feedUrl) && feedsTable.ContainsKey(feedUrl))
            {
                INewsFeed f = feedsTable[feedUrl];
                string feedId = f.Any.FirstOrDefault(elem => elem.LocalName == "id").InnerText; 

                string flagItemApiUrl = PostItemApiUrl + "/updatepostmetadata"; 

                NewsGatorItemMetaData body = new NewsGatorItemMetaData();
                body.updatepostmetadata = new bodyUpdatepostmetadata();
                body.updatepostmetadata.location = NgosLocationName;
                body.updatepostmetadata.synctoken = NgosSyncToken;
                body.updatepostmetadata.newstates = new bodyUpdatepostmetadataNewstates();
                body.updatepostmetadata.newstates.feedmetadata = new bodyUpdatepostmetadataNewstatesFeedmetadata();
                body.updatepostmetadata.newstates.feedmetadata.feedid = feedId;
                body.updatepostmetadata.newstates.feedmetadata.postmetadata = new bodyUpdatepostmetadataNewstatesFeedmetadataPostmetadata();
                body.updatepostmetadata.newstates.feedmetadata.postmetadata.flagstate = (int) state;
                body.updatepostmetadata.newstates.feedmetadata.postmetadata.flagstatespecified = true;
                body.updatepostmetadata.newstates.feedmetadata.postmetadata.postid = itemId;
                body.updatepostmetadata.newstates.feedmetadata.postmetadata.statespecified = false;

                XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(NewsGatorItemMetaData));
                StringBuilder sb = new StringBuilder();

                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                serializer.Serialize(XmlWriter.Create(sb, xws), body);

                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(flagItemApiUrl, sb.ToString(), this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }
            }            

        }



         /// <summary>
        /// Marks an item as read, unread or deleted in NewsGator Online
        /// </summary>
        /// <param name="itemId">The NewsGator ID of the news item</param>        
        /// <param name="state">Indicates whether the item was marked as read, unread or deleted</param>
        internal void ChangeItemStateInNewsGatorOnline(string itemId, NewsGatorItemState state)
        {
            if(!StringHelper.EmptyTrimOrNull(itemId)){
                string body = "loc=" + NgosLocationName + "&";

                switch (state)
                {
                    case NewsGatorItemState.Deleted:
                        body += "d=" + itemId;
                        break;
                    case NewsGatorItemState.Read:
                        body += "r=" + itemId;
                        break;
                    case NewsGatorItemState.Unread:
                        body += "u=" + itemId;
                        break; 
                }

            HttpWebResponse response = AsyncWebRequest.PostSyncResponse(PostItemApiUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

             if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }

             /* close the response stream to prevent threadpool deadlocks and resource leaks */
             try
             {
                 response.Close();
             }
             catch { }
            }
        }

        /// <summary>
        /// Marks all items older than the the specified date as read in NewsGatorOnline
        /// </summary>
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="syncToken">The synchronization token that identifies which items should be marked as read</param>
        internal void MarkAllItemsAsReadInNewsGatorOnline(string feedUrl, string syncToken)
        {
            if (!StringHelper.EmptyTrimOrNull(feedUrl) && feedsTable.ContainsKey(feedUrl))
            {
                NewsFeed f = feedsTable[feedUrl] as NewsFeed;
                string feedId = f.Any.FirstOrDefault(elem => elem.LocalName == "id").InnerText;
            
                string markReadUrl = FeedApiUrl + "/" + feedId;
                string body = "tok=" + syncToken + "&read=true";

                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(markReadUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }
            }

        }


         /// <summary>
        /// Removes all information related to an item from the FeedSource. 
        /// </summary>
        /// <remarks>If the item doesn't exist in the FeedSource then nothing is done</remarks>
        /// <param name="item">the item to delete</param>
        public override void DeleteItem(INewsItem item)
        {
            XmlElement elem = RssHelper.GetOptionalElement(item, "postId", NewsGatorRssNS);
            if (elem != null)
            {
                NewsGatorUpdater.ChangeItemStateInNewsGatorOnline(this.NewsGatorUserName, elem.InnerText, NewsGatorItemState.Deleted); 
            }

            base.DeleteItem(item); 
        }

        /// <summary>        
        /// Marks all items stored in the internal cache of RSS items and in NewsGator Online as read
        /// for a particular feed.
        /// </summary>        
        /// <param name="feed">The RSS feed</param>
        public override void MarkAllCachedItemsAsRead(INewsFeed feed)
        {
            DateTime newestItemAge = DateTime.MinValue;
            string syncToken = null; 

            if (feed != null && feed.containsNewMessages && !string.IsNullOrEmpty(feed.link) && itemsTable.ContainsKey(feed.link))
            {
                IFeedDetails fi = itemsTable[feed.link] as IFeedDetails;

                if (fi != null)
                {
                    //get sync token from last time feed was fetched
                    XmlElement elem = RssHelper.GetOptionalElement(fi.OptionalElements, "token",NewsGatorRssNS);  
                    
                    if(elem != null){
                        syncToken = elem.InnerText; 
                    }

                    //mark each cached NewsItem as read
                    foreach (NewsItem ri in fi.ItemsList)
                    {
                        ri.PropertyChanged -= this.OnNewsItemPropertyChanged;
                        ri.BeenRead = true;
                        newestItemAge = (ri.Date > newestItemAge ? ri.Date : newestItemAge);
                        ri.PropertyChanged += this.OnNewsItemPropertyChanged;
                    }
                }

                feed.containsNewMessages = false;
            }

            if (newestItemAge != DateTime.MinValue)
            {
                
                NewsGatorUpdater.MarkAllItemsAsReadInNewsGatorOnline(this.NewsGatorUserName, feed.link, syncToken);                 
            }
        }

        #endregion

        #region category/folder manipulation methods 

        /// <summary>
        /// Deletes a category from the FeedSource. This process includes deleting all subcategories and the 
        /// corresponding feeds. 
        /// </summary>
        /// <remarks>Note that this does not fix up the references to this category in the feed list nor does it 
        /// fix up the references to this category in its parent and child categories.</remarks>
        /// <param name="cat"></param>
        public override void DeleteCategory(string cat)
        {
            if (!StringHelper.EmptyTrimOrNull(cat) && categories.ContainsKey(cat))
            {
                INewsFeedCategory c = categories[cat];
                XmlAttribute idNode = c.AnyAttr.FirstOrDefault(attr => attr.LocalName == "id");

                if (idNode != null)
                {
                    string id = idNode.Value;
                    NewsGatorUpdater.DeleteFolderFromNewsGatorOnline(this.NewsGatorUserName, id);
                }
                base.DeleteCategory(cat); 
            }
        }

        /// <summary>
        /// Deletes the folder in NewsGator Online
        /// </summary>
        /// <param name="folderId">The ID of the folder to delete</param>
        internal void DeleteFolderFromNewsGatorOnline(string folderId)
        {
            if (!StringHelper.EmptyTrimOrNull(folderId))
            {

                string folderDeleteUrl = FolderApiUrl + "/delete"; 
                string body = "fld=" + folderId;

                HttpWebResponse response = AsyncWebRequest.DeleteSyncResponse(folderDeleteUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// Moves a folder in NewsGator Online
        /// </summary>
        /// <param name="folderId">The ID of the folder to move</param>
        /// <param name="parentId">The ID of the new parent folder</param>
        internal void MoveFolderInNewsGatorOnline(string folderId, string parentId)
        {
            string folderMoveUrl = FolderApiUrl + "/move";
            string body = "fld=" + folderId + "&parentId=" + parentId;

            HttpWebResponse response = AsyncWebRequest.PostSyncResponse(folderMoveUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new WebException(response.StatusDescription);
            }

            /* close the response stream to prevent threadpool deadlocks and resource leaks */
            try
            {
                response.Close();
            }
            catch { }
        }

          /// <summary>
        /// Changes the category of a particular INewsFeedCategory. This method should be used when moving a category. Also 
        /// changes the category of call child feeds and categories. 
        /// </summary>        
        /// <param name="cat">The category whose parent category to change</param>
        /// <param name="parent">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized. If this parameter is null then the parent is considered to be the root node.</param>
        public override void ChangeCategory(INewsFeedCategory cat, INewsFeedCategory parent)
        {
            if (this.categories.ContainsKey(cat.Value))
            {
                string folderId  = cat.AnyAttr.FirstOrDefault(a => a.LocalName == "id").Value;
                string parentId  = parent == null ? "0" : parent.AnyAttr.FirstOrDefault(a => a.LocalName == "id").Value; 
                
                base.ChangeCategory(cat, parent);
                NewsGatorUpdater.MoveFolderInNewsGatorOnline(this.NewsGatorUserName, folderId, parentId); 
            }
        }

        /// <summary>
        /// Renames the specified folder in NewsGator Online
        /// </summary>
        /// <param name="oldName">The old name of the category</param>
        /// <param name="newName">The new name of the category</param>        
        internal void RenameFolderInNewsGatorOnline(string oldName, string newName)
        {
            INewsFeedCategory cat = null;

            if (this.categories.TryGetValue(newName, out cat))
            {
                string folderRenameUrl = FolderApiUrl + "/rename";
                string body = "fld=" + cat.AnyAttr.FirstOrDefault(a => a.LocalName == "id").Value + "&name="
                    + Uri.EscapeDataString(newName);

                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(folderRenameUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }
            }

        }

        /// <summary>
        /// Renames the specified category
        /// </summary>        
        /// <remarks>This method assumes that the caller will rename categories on INewsFeed instances directly instead
        /// of having this method do it automatically.</remarks>
        /// <param name="oldName">The old name of the category</param>
        /// <param name="newName">The new name of the category</param>        
        public override void RenameCategory(string oldName, string newName)
        {
            if ((StringHelper.EmptyTrimOrNull(oldName) || StringHelper.EmptyTrimOrNull(newName))
                || oldName.Equals(newName))
            {
                return;
            }

            //rename object in category table
            base.RenameCategory(oldName, newName);
            NewsGatorUpdater.RenameFolderInNewsGatorOnline(this.NewsGatorUserName, oldName, newName);
        }


        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The category to add</param>
        /// <returns>The INewsFeedCategory instance that will actually be used to represent the category</returns>
        public override INewsFeedCategory AddCategory(INewsFeedCategory cat)
        {
            if (cat == null)
                throw new ArgumentNullException("cat");

            if (this.categories.ContainsKey(cat.Value))
                return this.categories[cat.Value];

            this.categories.Add(cat.Value, cat);
            readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);

            NewsGatorUpdater.AddFolderInNewsGatorOnline(this.NewsGatorUserName, cat.Value);
            return cat;   
        }

         /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The name of the category</param>
        /// <returns>The INewsFeedCategory instance that will actually be used to represent the category</returns>
        /// <exception cref="NotSupportedException">If the category is a subcategory. Google Reader doesn't support 
        /// nested categories</exception>
        public override INewsFeedCategory AddCategory(string cat)
        {
            if (StringHelper.EmptyTrimOrNull(cat))
                return null;

            return this.AddCategory(new category(cat)); 
        }
        
        /// <summary>
        /// Adds the folder in NewsGator Online
        /// </summary>
        /// <param name="name">The name of the folder to add</param>
        internal void AddFolderInNewsGatorOnline(string name)
        {
            if (StringHelper.EmptyTrimOrNull(name))
                return;

            INewsFeedCategory cat;
            this.categories.TryGetValue(name, out cat);

            if (cat == null) //this shouldn't happen unless app shutdown before category added in NewsGator
            {
                cat = new category(name);
                this.categories.Add(name, cat);
            }

            //check if we already have the category in NewsGator Online
            if (cat.AnyAttr != null && cat.AnyAttr.FirstOrDefault( attr => attr.LocalName == "id" )!= null)
                return; 

            List<string> ancestors = category.GetAncestors(name);
            
            //create rest of category hierarchy if it doesn't exist in NewsGator Online
            for (int i = ancestors.Count; i-- > 0; )
            {
                INewsFeedCategory c;
                if (!this.categories.TryGetValue(ancestors[i], out c)) //this shouldn't happen
                {
                    c = new category(ancestors[i]);
                    this.categories.Add(ancestors[i], c);
                }

                if (c.AnyAttr==null || c.AnyAttr.FirstOrDefault(attr => attr.LocalName == "id") == null)
                {
                    this.AddFolderInNewsGatorOnline(ancestors[i]);
                    if (c.Value.Contains(FeedSource.CategorySeparator))
                    {
                        c.parent = this.categories[ancestors[i - 1]];
                    }
                }                
            }//for
            
            //make sure the new category has its parent set
            cat.parent = (ancestors.Count == 0 ? null : this.categories[ancestors[ancestors.Count - 1]]);
            int index = cat.Value.LastIndexOf(FeedSource.CategorySeparator); 
            string folderName =  (index == -1 ? cat.Value : cat.Value.Substring(index + 1)); 

            string folderCreateUrl = FolderApiUrl + "/create";
            string body = "parentid=" + (cat.parent == null ? "0" : cat.parent.AnyAttr.FirstOrDefault(a => a.LocalName == "id").Value) 
                + "&name=" + Uri.EscapeDataString(folderName) + "&root=MYF";            
           
                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(folderCreateUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

                try
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(response.GetResponseStream());

                        XmlElement outlineElem = doc.SelectSingleNode("//outline[@text=" + buildXPathString(folderName) + "]") as XmlElement;

                        if (outlineElem != null)
                        {
                            cat.AnyAttr = new XmlAttribute[1];

                            XmlAttribute idNode = outlineElem.Attributes["id", NewsGatorOpmlNS];
                            if (idNode != null)
                            {
                                cat.AnyAttr[0] = idNode;
                            }
                        }

                    }
                    else
                    {
                        throw new WebException(response.StatusDescription);
                    }
                }
                finally
                {
                    /* close the response stream to prevent threadpool deadlocks and resource leaks */
                    try
                    {
                        response.Close();
                    }
                    catch { }
                }        
        }
        #endregion 

        #region feed manipulation methods

         /// <summary>
        /// Changes the title of a subscribed feed in NewsGator Online
        /// </summary>
        /// <remarks>This method does nothing if the new title is empty or null</remarks>
        /// <param name="url">The feed URL</param>
        /// <param name="title">The new title</param>
        internal void RenameFeedInNewsGatorOnline(string url, string title)
        {
            if (StringHelper.EmptyTrimOrNull(title))
            {
                return;
            }

            if (!StringHelper.EmptyTrimOrNull(url) && feedsTable.ContainsKey(url))
            {
                INewsFeed f = feedsTable[url];

                opml location = new opml();
                location.body = new opmloutline[1];
                opmloutline outline = new opmloutline();
                outline.xmlUrl = f.link;
                outline.text = title;
                outline.id = f.Any.FirstOrDefault(elem => elem.LocalName == "id").InnerText; 


                if (!StringHelper.EmptyTrimOrNull(f.category))
                {
                    string[] catHives = f.category.Split(CategorySeparator.ToCharArray());

                    foreach (string cat in catHives.Reverse())
                    {
                        opmloutline folder = new opmloutline();
                        folder.text = cat;
                        folder.outline = new opmloutline[] { outline };
                        outline = folder;
                    }
                }

                location.body[0] = outline;

                XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(opml));
                StringBuilder sb = new StringBuilder();

                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                serializer.Serialize(XmlWriter.Create(sb, xws), location);

                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(SubscriptionApiUrl, sb.ToString(), this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)               
                {
                    throw new WebException(response.StatusDescription);
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }
            }

        }

        /// <summary>
        /// Adds the specified feed in NewsGator Online
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to add</param>
        /// <param name="cat">The name of the category</param>
        internal void ChangeFolderInNewsGatorOnline(string feedUrl, string cat)
        {

            if (!StringHelper.EmptyTrimOrNull(feedUrl) && feedsTable.ContainsKey(feedUrl)
                && !StringHelper.EmptyTrimOrNull(cat) && categories.ContainsKey(cat) )
            {
                INewsFeed f = feedsTable[feedUrl];
                string feedId = f.Any.FirstOrDefault(elem => elem.LocalName == "id").InnerText;

                INewsFeedCategory c = categories[cat];
                string folderId = c.AnyAttr.FirstOrDefault(attr => attr.LocalName == "id").Value;

                string moveApiUrl = SubscriptionApiUrl + "/" + NgosLocationName + "/movesubscription/" + feedId;
                string body = "tofolderid=" + folderId;

                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(moveApiUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);
                
                try
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException(response.StatusDescription);
                    }
                }
                finally
                {
                    /* close the response stream to prevent threadpool deadlocks and resource leaks */
                    try
                    {
                        response.Close();
                    }
                    catch { }
                }

                //update NewsGator folder ID of feed
                XmlElement folderIdNode = f.Any.FirstOrDefault(elem => elem.LocalName == "folderId");
                folderIdNode.InnerText = folderId; 
            }
        }

         /// <summary>
        /// Changes the category of a particular INewsFeed. This method should be used instead of setting
        /// the category property of the INewsFeed instance. 
        /// </summary>
        /// <param name="feed">The newsfeed whose category to change</param>
        /// <param name="cat">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized</param>
        public override void ChangeCategory(INewsFeed feed, string cat)
        {
            base.ChangeCategory(feed, cat);
            NewsGatorUpdater.ChangeFolderInNewsGatorOnline(this.NewsGatorUserName, feed.link, cat); 
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
            base.ChangeCategory(feed, cat);
            NewsGatorUpdater.ChangeFolderInNewsGatorOnline(this.NewsGatorUserName, feed.link, cat.Value);
        }

        /// <summary>
        /// Adds a feed and associated FeedInfo object to the FeedsTable and itemsTable. 
        /// Any existing feed objects are replaced by the new objects. 
        /// </summary>
        /// <param name="feed">The NewsFeed object </param>
        /// <param name="feedInfo">The FeedInfo object</param>
        /// <returns>The actual INewsFeed instance that will be used to represent this feed subscription</returns>
        public override INewsFeed AddFeed(INewsFeed feed, FeedInfo feedInfo)
        {
            feed.owner = this; 
            feed = base.AddFeed(feed, feedInfo);
            NewsGatorUpdater.AddFeedInNewsGatorOnline(this.NewsGatorUserName, feed.link);         
            feed.PropertyChanged += this.OnNewsFeedPropertyChanged;

            return feed; 
        }

        /// <summary>
        /// Adds the specified feed in NewsGator Online
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to add</param>
        internal void AddFeedInNewsGatorOnline(string feedUrl)
        {
            if (!StringHelper.EmptyTrimOrNull(feedUrl) && feedsTable.ContainsKey(feedUrl))
            {
                INewsFeed f = feedsTable[feedUrl];

                    opml location = new opml();
                    location.body = new opmloutline[1];
                    opmloutline outline = new opmloutline();
                    outline.xmlUrl = f.link;
                    outline.text = f.title;


                    if (!StringHelper.EmptyTrimOrNull(f.category))
                    {
                        string[] catHives = f.category.Split(CategorySeparator.ToCharArray());

                        foreach (string cat in catHives.Reverse())
                        {
                            opmloutline folder = new opmloutline();
                            folder.text = cat;
                            folder.outline = new opmloutline[] { outline };
                            outline = folder; 
                        }
                    }

                    location.body[0] = outline;

                    XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(opml));
                    StringBuilder sb = new StringBuilder();

                    XmlWriterSettings xws = new XmlWriterSettings();
                    xws.OmitXmlDeclaration = true;

                    serializer.Serialize(XmlWriter.Create(sb, xws), location);                

                    HttpWebResponse response = AsyncWebRequest.PostSyncResponse(SubscriptionApiUrl, sb.ToString(), this.location.Credentials, this.Proxy, NgosTokenHeader);

                    try
                    {

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(response.GetResponseStream());

                            XmlElement outlineElem = doc.SelectSingleNode("//outline[@xmlUrl='" + f.link + "']") as XmlElement;

                            if (outlineElem != null)
                            {
                                f.Any = new XmlElement[2];

                                string id = outlineElem.GetAttribute("id", NewsGatorOpmlNS);
                                if (!StringHelper.EmptyTrimOrNull(id))
                                {
                                    XmlElement idNode = doc.CreateElement("ng", "id", NewsGatorRssNS);
                                    idNode.InnerText = id;
                                    f.Any[0] = idNode;
                                }

                                string syncXmlUrl = outlineElem.GetAttribute("syncXmlUrl", NewsGatorOpmlNS);
                                if (!StringHelper.EmptyTrimOrNull(syncXmlUrl))
                                {
                                    XmlElement syncXmlUrlNode = doc.CreateElement("ng", "syncXmlUrl", NewsGatorRssNS);
                                    syncXmlUrlNode.InnerText = syncXmlUrl;
                                    f.Any[1] = syncXmlUrlNode;
                                }
                            }

                        }
                        else
                        {
                            throw new WebException(response.StatusDescription);
                        }
                    }
                    finally
                    {
                        /* close the response stream to prevent threadpool deadlocks and resource leaks */
                        try
                        {
                            response.Close();
                        }
                        catch { }
                    }
                
            }//if(StringHelper...)
        }


        /// <summary>
        /// Deletes all subscribed feeds and categories 
        /// </summary>
        /// <param name="deleteFromSource">Indicates whether the feeds should also be deleted from the feed source</param>
        public override void DeleteAllFeedsAndCategories(bool deleteFromSource)
        {
            if (deleteFromSource)
            {
                NewsGatorUpdater.CancelPendingOperations();
            }
            else
            {
                NewsGatorUpdater.UnregisterFeedSource(this); 
            }
            base.DeleteAllFeedsAndCategories(deleteFromSource);
        }

        /// <summary>
        /// Removes all information related to a feed from the FeedSource.   
        /// </summary>
        /// <remarks>If no feed with that URL exists then nothing is done.</remarks>       
        /// <exception cref="ApplicationException">If an error occured while 
        /// attempting to delete the cached feed. Examine the InnerException property 
        /// for details</exception>
        public override void DeleteFeed(string feedUrl)
        {
            NewsGatorUpdater.DeleteFeedFromNewsGatorOnline(this.NewsGatorUserName, feedUrl);
        }

        /// <summary>
        /// Deletes a feed from the list of user's subscriptions in NewsGator Online
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to delete. </param>
        internal void DeleteFeedFromNewsGatorOnline(string feedUrl)
        {

            if (!StringHelper.EmptyTrimOrNull(feedUrl) && feedsTable.ContainsKey(feedUrl))
            {
                INewsFeed f = feedsTable[feedUrl];
                string feedId = f.Any.FirstOrDefault(elem => elem.LocalName == "id").InnerText; 
                opml location = new opml();
                location.body = new opmloutline[1];
                opmloutline outline = new opmloutline();
                outline.id = feedId;
                location.body[0] = outline;

                XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(opml));
                StringBuilder sb = new StringBuilder();
               
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                               
                serializer.Serialize(XmlWriter.Create(sb, xws), location); 

                HttpWebResponse response = AsyncWebRequest.DeleteSyncResponse(SubscriptionApiUrl, sb.ToString(), this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }

                /* close the response stream to prevent threadpool deadlocks and resource leaks */
                try
                {
                    response.Close();
                }
                catch { }

                base.DeleteFeed(feedUrl); 
            }
        }

        #endregion 

        #endregion
    }

#endregion

    #region NewsGator post metadata related classes


    /// <remarks/>
    [System.Xml.Serialization.XmlRoot(ElementName="body")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class NewsGatorItemMetaData
    {

        private bodyUpdatepostmetadata updatepostmetadataField;

        /// <remarks/>
        public bodyUpdatepostmetadata updatepostmetadata
        {
            get
            {
                return this.updatepostmetadataField;
            }
            set
            {
                this.updatepostmetadataField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class bodyUpdatepostmetadata
    {

        private string locationField;

        private string synctokenField;

        private bodyUpdatepostmetadataNewstates newstatesField;

        /// <remarks/>
        public string location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }

        /// <remarks/>
        public string synctoken
        {
            get
            {
                return this.synctokenField;
            }
            set
            {
                this.synctokenField = value;
            }
        }

        /// <remarks/>
        public bodyUpdatepostmetadataNewstates newstates
        {
            get
            {
                return this.newstatesField;
            }
            set
            {
                this.newstatesField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class bodyUpdatepostmetadataNewstates
    {

        private bodyUpdatepostmetadataNewstatesFeedmetadata feedmetadataField;

        /// <remarks/>
        public bodyUpdatepostmetadataNewstatesFeedmetadata feedmetadata
        {
            get
            {
                return this.feedmetadataField;
            }
            set
            {
                this.feedmetadataField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class bodyUpdatepostmetadataNewstatesFeedmetadata
    {

        private string feedidField;

        private bodyUpdatepostmetadataNewstatesFeedmetadataPostmetadata postmetadataField;

        /// <remarks/>
        public string feedid
        {
            get
            {
                return this.feedidField;
            }
            set
            {
                this.feedidField = value;
            }
        }

        /// <remarks/>
        public bodyUpdatepostmetadataNewstatesFeedmetadataPostmetadata postmetadata
        {
            get
            {
                return this.postmetadataField;
            }
            set
            {
                this.postmetadataField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class bodyUpdatepostmetadataNewstatesFeedmetadataPostmetadata
    {

        private string postidField;

        private int stateField;

        private bool statespecifiedField;

        private int flagstateField;

        private bool flagstatespecifiedField;

        /// <remarks/>
        public string postid
        {
            get
            {
                return this.postidField;
            }
            set
            {
                this.postidField = value;
            }
        }

        /// <remarks/>
        public int state
        {
            get
            {
                return this.stateField;
            }
            set
            {
                this.stateField = value;
            }
        }

        /// <remarks/>
        public bool statespecified
        {
            get
            {
                return this.statespecifiedField;
            }
            set
            {
                this.statespecifiedField = value;
            }
        }

        /// <remarks/>
        public int flagstate
        {
            get
            {
                return this.flagstateField;
            }
            set
            {
                this.flagstateField = value;
            }
        }

        /// <remarks/>
        public bool flagstatespecified
        {
            get
            {
                return this.flagstatespecifiedField;
            }
            set
            {
                this.flagstatespecifiedField = value;
            }
        }
    }

    #endregion 

}

