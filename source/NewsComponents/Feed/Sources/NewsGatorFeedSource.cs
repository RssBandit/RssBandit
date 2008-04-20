#region Version Info Header
/*
 * $Id: NewsGatorFeedSource.cs 359 2008-02-24 13:36:21Z carnage4life $
 * $HeadURL: https://rssbandit.svn.sourceforge.net/svnroot/rssbandit/trunk/source/NewsComponents/Feed/NewsGatorFeedSource.cs $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2008-02-24 05:36:21 -0800 (Sun, 24 Feb 2008) $
 * $Revision: 359 $
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using log4net;

using RssBandit.Common;
using RssBandit.Common.Logging;

using NewsComponents.Net;
using NewsComponents.Search;
using NewsComponents.Utils;

namespace NewsComponents.Feed {

#region NewsGatorFeedSource


    /// <summary>
    /// Indicates the state of a news item in NewsGator Online
    /// </summary>
    internal enum NewsGatorItemState
    {
        Read, Unread, Deleted
    }

    /// <summary>
    /// A FeedSource that retrieves user subscriptions and feeds from NewsGator Online. 
    /// </summary>
    class NewsGatorFeedSource : FeedSource
    {

        #region private fields


        // logging/tracing:
        private static readonly ILog _log = Log.GetLogger(typeof(NewsGatorFeedSource));


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

            // initialize (later on loaded from feedlist):
            this.PodcastFolder = this.Configuration.DownloadedFilesDataPath;
            
            if (String.IsNullOrEmpty(EnclosureFolder))
            {
                this.enclosureDownloader = new BackgroundDownloadManager(this.Configuration, this);
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
        public NewsGatorModifier NewsGatorUpdater
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
        /// <param name="feedStream">The stream to save the feed list to</param>
        public override void SaveFeedList(Stream feedStream)
        {
            base.SaveFeedList(feedStream);
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

            HttpWebResponse response = AsyncWebRequest.PostSyncResponse(LocationApiUrl, sb.ToString(), this.location.Credentials , this.Proxy, NgosTokenHeader);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _log.Debug(String.Format("Error occured when creating location in NewsGator Online: {0}-{1}", response.StatusCode, response.StatusDescription));
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
            //load Bandit subscriptions.xml document into memory
            XmlReader reader = XmlReader.Create(this.location.Location);
            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(feeds));
            feeds myFeeds = (feeds)serializer.Deserialize(reader);
            reader.Close();

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
            feeds ngFeeds = (this.Offline ? feedlist : this.LoadFeedlistFromNewsGatorOnline());

            foreach (NewsFeed ngFeed in ngFeeds.feed)
            {
                if (!ngFeed.link.Equals(NoXmlUrlFoundInOpml))
                {
                    NewsFeed feed = null;
                    bootstrapFeeds.TryGetValue(ngFeed.link, out feed);
                    this.feedsTable.Add(ngFeed.link, TransferSettings(ngFeed, feed));
                }
            }

            foreach (category ngCategory in ngFeeds.categories)
            {
                category cat = null;
                bootstrapCategories.TryGetValue(ngCategory.Value, out cat);
                this.categories.Add(ngCategory.Value, cat ?? ngCategory);
            }

            // copy over list view layouts 
            if (feedlist.listviewLayouts != null)
            {
                foreach (listviewLayout layout in feedlist.listviewLayouts)
                {
                    string layout_trimmed = layout.ID.Trim();
                    if (!this.layouts.ContainsKey(layout_trimmed))
                    {
                        this.layouts.Add(layout_trimmed, layout.FeedColumnLayout);
                    }
                }
            }//if(feedlist.listviewLayouts != null)

        }

        #endregion 

        #region feed downloading methods

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
                    if ((!forceDownload) || this.isOffline)
                    {
                        GetCachedItemsForFeed(feedUri.CanonicalizedUri()); //load feed into itemsTable
                        RaiseOnUpdatedFeed(feedUri, null, RequestResult.NotModified, priority, false);
                        return false;
                    }
                }
                catch (XmlException xe)
                {
                    //cache file is corrupt
                    Trace("Unexpected error retrieving cached feed '{0}': {1}", feedUrl, xe.ToString());
                }

                //We need a reference to the feed so we can see if a cached object exists
                NewsFeed theFeed = null;
                if (feedsTable.ContainsKey(feedUri.CanonicalizedUri()))
                    theFeed = feedsTable[feedUri.CanonicalizedUri()] as NewsFeed;

                if (theFeed == null || theFeed.Any == null)
                    return false;

                XmlElement syncXmlUrl = theFeed.Any.First(elem => elem.LocalName == "syncXmlUrl");

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

                if (itemsTable.ContainsKey(feedUrl))
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
                                             null,
                                             OnRequestStart,
                                             OnRequestComplete,
                                             OnRequestException, priority);

                requestQueued = true;
            }
            catch (Exception e)
            {
                Trace("Unexpected error on QueueRequest(), processing feed '{0}': {1}", feedUrl, e.ToString());
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
            string feedUrl = feedsTable.First(kvp =>
                                kvp.Value.Any.First(elem => elem.LocalName == "syncXmlUrl").InnerText.Equals(syncUrl)).Key;

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
            string feedUrl = feedsTable.First(kvp =>
                                kvp.Value.Any.First(elem => elem.LocalName == "syncXmlUrl").InnerText.Equals(syncUrl)).Key;

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
        /// <param name="response">The Response as a stream</param>
        /// <param name="newUri">The new URI of a 3xx HTTP response was originally received</param>
        /// <param name="eTag">The etag</param>
        /// <param name="lastModified">The last modified date of the result</param>
        /// <param name="result">The HTTP result</param>
        /// <param name="priority">The priority of the request</param>
        protected override void OnRequestComplete(Uri requestUri, Stream response, Uri newUri, string eTag, DateTime lastModified,
                                    RequestResult result, int priority)
        {
            //find the feed that has the requestUri as it's syncXmlUrl
            string syncUrl = requestUri.CanonicalizedUri().Substring(0, requestUri.CanonicalizedUri().LastIndexOf("?unread=False")); 
            string feedUrl = feedsTable.First(kvp =>
                                kvp.Value.Any.First(elem => elem.LocalName == "syncXmlUrl").InnerText.Equals(syncUrl)).Key;
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

                    feedsTable.Remove(feedUrl);
                    theFeed.link = newUri.CanonicalizedUri();
                    this.feedsTable.Add(theFeed.link, theFeed);

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

                    FeedDetailsInternal fi = RssParser.GetItemsForFeed(theFeed, response, false);
                    FeedDetailsInternal fiFromCache = null;

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
                        Trace("this.GetFeed(theFeed) caused exception: {0}", ex.ToString());
                        /* the cache file may be corrupt or an IO exception 
						 * not much we can do so just ignore it 
						 */
                    }

                    List<INewsItem> newReceivedItems = null;

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
                                fi.ItemsList = MergeAndPurgeItems(fi2.ItemsList, fi.ItemsList, theFeed.deletedstories,
                                                                  out newReceivedItems, theFeed.replaceitemsonrefresh,
                                                                  false /* respectOldItemState */);
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
                                foreach (FeedDetailsInternal fdi in itemsTable.Values)
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

                    //FeedDetailsInternal feedInfo = itemsTable[feedUrl];
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
                if (response != null)
                    response.Close();
            }
        }

        #endregion 

        #region news item manipulation methods

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
            }

        }

        /// <summary>
        /// Invoked when a NewsItem owned by this FeedSource changes in a way that 
        /// needs to be communicated to Google Reader. 
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
                        NewsGatorUpdater.ChangeItemClippedStateInNewsGatorOnline(this.NewsGatorUserName, elem.InnerText, 
                            item.FlagStatus != Flagged.None);
                        break;
                }
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
                string feedId = f.Any.First(elem => elem.LocalName == "id").InnerText;
            
                string markReadUrl = FeedApiUrl + "/" + feedId;
                string body = "tok=" + syncToken + "&read=true";

                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(markReadUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }              
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

            if (feed != null && !string.IsNullOrEmpty(feed.link) && itemsTable.ContainsKey(feed.link))
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

        #region feed manipulation methods 


        /// <summary>
        /// Adds a feed and associated FeedInfo object to the FeedsTable and itemsTable. 
        /// Any existing feed objects are replaced by the new objects. 
        /// </summary>
        /// <param name="feed">The NewsFeed object </param>
        /// <param name="feedInfo">The FeedInfo object</param>
        /// <returns>The actual INewsFeed instance that will be used to represent this feed subscription</returns>
        public override INewsFeed AddFeed(INewsFeed feed, FeedInfo feedInfo)
        {
            NewsGatorUpdater.AddFeedInNewsGatorOnline(this.NewsGatorUserName, feed.link);
            return base.AddFeed(feed, feedInfo);
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

                using (MemoryStream ms = new MemoryStream())
                {
                    this.SaveFeedList(ms, FeedListFormat.OPML);
                    ms.Seek(0, SeekOrigin.Begin); 
                    string body = new StreamReader(ms).ReadToEnd();
                    
                    #region old code that doesn't work 
                    /*
                     POSTing OPML TO SUBSCRIPTION URL RESULTS IN HTTP 500 ERRORS. NEWSGATOR ONLINE PROBABLY HAS BUGS HERE. 
                 
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
                
                     */
                    #endregion 

                    HttpWebResponse response = AsyncWebRequest.PutSyncResponse(SubscriptionApiUrl, body, this.location.Credentials, this.Proxy, NgosTokenHeader);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(response.GetResponseStream());

                        XmlElement outline = doc.SelectSingleNode("//outline[@xmlUrl='" + f.link + "']") as XmlElement;
                        
                        if (outline != null)
                        {
                            f.Any = new XmlElement[2];

                            string id = outline.GetAttribute("id", NewsGatorOpmlNS);
                            if (!StringHelper.EmptyTrimOrNull(id))
                            {
                                XmlElement idNode = doc.CreateElement("ng", "id", NewsGatorRssNS);
                                idNode.InnerText = id;
                                f.Any[0] = idNode; 
                            }

                            string syncXmlUrl = outline.GetAttribute("syncXmlUrl", NewsGatorOpmlNS);
                            if (!StringHelper.EmptyTrimOrNull(syncXmlUrl))
                            {
                                XmlElement syncXmlUrlNode = doc.CreateElement("ng", "syncXmlUrl", NewsGatorRssNS);
                                syncXmlUrlNode.InnerText = syncXmlUrl;
                                f.Any[1] = syncXmlUrlNode;
                            }
                        }
                        
                        //make sure INewsFeed is still in the feeds table in case user deleted it since method call started
                        if (!feedsTable.ContainsKey(f.link))
                        {
                            feedsTable.Add(f.link, f); 
                        }

                    }else
                    {
                        throw new WebException(response.StatusDescription);
                    }

                }//using(MemoryStream ms...)
            }//if(StringHelper...)
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
                string feedId = f.Any.First(elem => elem.LocalName == "id").InnerText; 
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

                base.DeleteFeed(feedUrl); 
            }
        }

        #endregion 

        #endregion
    }

#endregion


}

