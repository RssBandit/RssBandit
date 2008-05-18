#region Version Info Header
/*
 * $Id: GoogleReaderFeedSource.cs 462 2008-04-07 00:19:34Z Carnage4Life $
 * $HeadURL: https://rssbandit.svn.sourceforge.net/svnroot/rssbandit/trunk/source/NewsComponents/Feed/GoogleReaderFeedSource.cs $
 * Last modified by $Author: Carnage4Life $
 * Last modified at $Date: 2008-04-06 17:19:34 -0700 (Sun, 06 Apr 2008) $
 * $Revision: 462 $
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

using log4net;

using NewsComponents.Collections;
using NewsComponents.Net;
using NewsComponents.Utils;

using RssBandit.Common;
using RssBandit.Common.Logging;

namespace NewsComponents.Feed
{

    #region GoogleReaderFeedSource

    /// <summary>
    /// A FeedSource that retrieves user subscriptions and feeds from Google Reader. 
    /// </summary>
    public class GoogleReaderFeedSource : FeedSource
    {

        #region private fields

        // logging/tracing:
        private static readonly ILog _log = Log.GetLogger(typeof(GoogleReaderFeedSource));

        /// <summary>
        /// The URL for authenticating a Google user.
        /// </summary>
        private static readonly string authUrl  = @"https://www.google.com/accounts/ClientLogin?continue=http://www.google.com&service=reader&source=RssBandit&Email={0}&Passwd={1}";

        /// <summary>
        /// The first part of the URL to a feed stored in the Google Reader service
        /// </summary>
        private static readonly string feedUrlPrefix = @"http://www.google.com/reader/atom/";

        /// <summary>
        /// The first part of the URL to various Google Reader API end points
        /// </summary>
        private static readonly string apiUrlPrefix = @"http://www.google.com/reader/api/0/";

        /// <summary>
        /// Qname for the google reader continuation token found in a Google Reader Atom feed. 
        /// </summary>
        private static readonly XmlQualifiedName continuationQName = new XmlQualifiedName("continuation", "http://www.google.com/schemas/reader/atom/");

        /// <summary>
        /// This is the most recently retrieved edit token from the Google Reader service. 
        /// </summary>
        private static string MostRecentGoogleEditToken = null; 

        /// <summary>
        /// Authentication token which identifies the user. 
        /// </summary>
        private string SID = String.Empty;

      
        private string googleUserId = String.Empty;

        /// <summary>
        /// Updates Google Reader in a background thread.
        /// </summary>
        private static GoogleReaderModifier googleReaderUpdater;

        #endregion 

          #region constructor

        /// <summary>
        /// Shouldn't be able to create one of these without specifying a location. 
        /// </summary>
        private GoogleReaderFeedSource() { ; }

           /// <summary>
        /// Initializes a new instance of the <see cref="FeedSource"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="location">The user credentials</param>
        internal GoogleReaderFeedSource(INewsComponentsConfiguration configuration, SubscriptionLocation location)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = FeedSource.DefaultConfiguration;

            this.location = location;

            //register with background thread for updating Google Reader
            GoogleReaderUpdater.RegisterFeedSource(this);

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
        /// Returns the Google username associated with this feed source
        /// </summary>
        public string GoogleUserName
        {
            get { return this.location.Credentials.UserName; }
        }

        /// <summary>
        /// The Google User ID of the user. 
        /// </summary>
        public string GoogleUserId
        {
            get { return googleUserId; }
            private set
            {
                this.googleUserId = value;
            }
        }

        /// <summary>
        /// Gets or sets the Google Reader modifier
        /// </summary>
        /// <value>The GoogleReaderModifier instance used by all instances of this class.</value>
        internal GoogleReaderModifier GoogleReaderUpdater
        {
            get
            {
                if (googleReaderUpdater == null)
                {
                    googleReaderUpdater = new GoogleReaderModifier(this.Configuration.UserApplicationDataPath);
                }
                return googleReaderUpdater;
            }
            set
            {
                googleReaderUpdater = value;
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
            GoogleReaderUpdater.SavePendingOperations(); 
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

            //load feed list from Google Reader and use settings from subscriptions.xml
            this.BootstrapAndLoadFeedlist(myFeeds);
            GoogleReaderUpdater.StartBackgroundThread(); 
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

            if (this.Offline)
            {
                foreach (NewsFeed ff in feedlist.feed) { this.feedsTable.Add(ff.link, ff); }
                foreach (category cc in feedlist.categories) { this.categories.Add(cc.Value, cc); }
            }
            else
            {
                //matchup feeds from GoogleReader with 
                IEnumerable<GoogleReaderSubscription> gReaderFeeds = this.LoadFeedlistFromGoogleReader();

                foreach (GoogleReaderSubscription gfeed in gReaderFeeds)
                {
                    NewsFeed feed = null;
                    this.feedsTable.Add(gfeed.FeedUrl, new GoogleReaderNewsFeed(gfeed, (bootstrapFeeds.TryGetValue(gfeed.FeedUrl, out feed) ? feed : null), this));
                }

                IEnumerable<string> gReaderLabels = this.LoadTaglistFromGoogleReader();

                string labelPrefix = "user/" + this.GoogleUserId + "/label/";
                foreach (string gLabel in gReaderLabels)
                {
                    string label = gLabel.Replace("/", FeedSource.CategorySeparator);
                    category cat = null;
                    bootstrapCategories.TryGetValue(label, out cat);
                    this.categories.Add(gLabel, cat ?? new category(label));

                }
            }

            /* copy over list view layouts */
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
            }

        }

         /// <summary>
        /// Loads the user's category list from Google Reader. 
        /// </summary>
        private IEnumerable<string> LoadTaglistFromGoogleReader()
        {
            string taglistUrl = apiUrlPrefix + "tag/list";

            //load tag list XML
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(AsyncWebRequest.GetSyncResponseStream(taglistUrl, null, this.Proxy, MakeGoogleCookie(this.SID))));

            string temp = doc.SelectSingleNode("/object/list/object/string[contains(string(.), 'state/com.google/starred')]").InnerText;
            this.GoogleUserId = temp.Replace("/state/com.google/starred", "").Substring(5);

            var taglist = from XmlNode node in doc.SelectNodes("/object/list[@name='tags']/object/string[@name='id']")
                          where node.InnerText.IndexOf("/com.google/") == -1
                          select node.InnerText.Replace("user/" + this.GoogleUserId + "/label/", ""); 

            return taglist;          
        }


        /// <summary>
        /// Loads the user's feed list from Google Reader. 
        /// </summary>
        private IEnumerable<GoogleReaderSubscription> LoadFeedlistFromGoogleReader()
        {
            string feedlistUrl = apiUrlPrefix + "subscription/list";

            //get the user's SID
            this.AuthenticateUser();
            
            //load feed list XML
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(AsyncWebRequest.GetSyncResponseStream(feedlistUrl, null, this.Proxy, MakeGoogleCookie(this.SID))));

            var feedlist = from XmlNode node in doc.SelectNodes("/object/list[@name='subscriptions']/object")
                           select MakeSubscription(node);
            
            return feedlist; 
        }


        /// <summary>
        /// Helper function which creates a Google Reader subscription node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static GoogleReaderSubscription MakeSubscription(XmlNode node)
        {
            XmlNode id_node = node.SelectSingleNode("string[@name='id']");
            string feedid = (id_node == null ? String.Empty : id_node.InnerText);
            XmlNode title_node = node.SelectSingleNode("string[@name='title']");
            string title = (title_node == null ? String.Empty : title_node.InnerText);
            XmlNode fim_node = node.SelectSingleNode("string[@name='firstitemmsec']");
            long firstitemmsec = (fim_node == null ? 0 : Int64.Parse(fim_node.InnerText));
            List<GoogleReaderLabel> categories = MakeLabelList(node.SelectNodes("list[@name='categories']/object"));

            return new GoogleReaderSubscription(feedid, title, categories, firstitemmsec);         
        }


        /// <summary>
        /// Helper function which makes a list of Google Reader labels
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static List<GoogleReaderLabel> MakeLabelList(XmlNodeList nodes)
        {
            List<GoogleReaderLabel> labels = new List<GoogleReaderLabel>(); 

            foreach (XmlNode node in nodes) {
                XmlNode id_node = node.SelectSingleNode("string[@name='id']");
                string catid = (id_node == null ? String.Empty : id_node.InnerText);
                XmlNode label_node = node.SelectSingleNode("string[@name='label']");
                string label = (label_node == null ? String.Empty : label_node.InnerText); 
                labels.Add(new GoogleReaderLabel(label, catid));                 
            }

            return labels; 
        }

        #endregion

        #region user authentication methods

        /// <summary>
        /// Authenticates a user to Google's services and obtains their SID
        /// </summary>
        private void AuthenticateUser()
        {
            string requestUrl = String.Format(authUrl, location.Credentials.UserName, location.Credentials.Password); 
            WebRequest req = HttpWebRequest.Create(requestUrl); 
            
            WebResponse resp = req.GetResponse(); 
            StreamReader reader = new StreamReader(AsyncWebRequest.GetSyncResponseStream(requestUrl, null, this.Proxy));
            string[] response = reader.ReadToEnd().Split('\n');

            foreach(string s in response){
                if(s.StartsWith("SID=",StringComparison.Ordinal)){
                    this.SID = s.Substring(4);
                    return;
                }
            }

            throw new WebException("Could not authenticate user to Google Reader because no SID provided in response", WebExceptionStatus.UnknownError);
        }

		/// <summary>
		/// Returns a cookie with the specified SID
		/// </summary>
		/// <param name="sid">The user's SID.</param>
		/// <returns>
		/// The Google cookie created from the SID
		/// </returns>
        private static Cookie MakeGoogleCookie(string sid)
        {
           Cookie cookie = new Cookie("SID", sid, "/", ".google.com");
           cookie.Expires = DateTime.Now + new TimeSpan(7,0,0,0);           
           return cookie; 
        }

        /// <summary>
        /// Gets an edit token which is needed for any edit operations using the Google Reader API
        /// </summary>
        /// <param name="sid">The user's SID</param>
        /// <returns>The edit token</returns>
        private static string GetGoogleEditToken(string sid)
        {
            string tokenUrl = apiUrlPrefix + "token";
            HttpWebRequest request = HttpWebRequest.Create(tokenUrl) as HttpWebRequest;
            request.Timeout        = 5 * 1000; //5 second time out

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(MakeGoogleCookie(sid));

            try
            {
                StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream());
                MostRecentGoogleEditToken = reader.ReadToEnd();
            } catch (WebException we){
                if (we.Status != WebExceptionStatus.Timeout)
                {
                    throw;
                }
            }

            return MostRecentGoogleEditToken; 
        }

        #endregion 

        #region feed downloading methods


        /// <summary>
        /// Returns the Google Reader URL from which to download the Atom feed for the GoogleReaderNewsFeed object. 
        /// </summary>
        /// <param name="feed">The target feed</param>
       
        /// <returns></returns>
        private static string CreateDownloadUrl(GoogleReaderNewsFeed feed)
        {
            //either download all items since last retrieved or last 3 months of stuff if never fetched items from feed
            TimeSpan maxItemAge = (feed.lastretrievedSpecified ? DateTime.Now - feed.lastretrieved : new TimeSpan(90, 0, 0, 0));
            string feedUrl = feedUrlPrefix + Uri.EscapeDataString(feed.GoogleReaderFeedId) + "?n=50&r=o&ot=" + Convert.ToInt32(maxItemAge.TotalSeconds);          

            return feedUrl; 
        }

        /// <summary>
        /// Appends the continuation token to the Google Reader download URL for a particular feed
        /// </summary>
        /// <param name="requestUri">The URL to download a feed in Google Reader</param>
        /// <param name="continuationToken">The token that indicates what "page" of the feed to return</param>
        /// <returns></returns>
        private static Uri CreateContinuedDownloadUrl(Uri requestUri, string continuationToken)
        {
            string feedUrl = requestUri.CanonicalizedUri();
            int index = feedUrl.IndexOf("&c=");

            if (index == -1)
            {
                feedUrl = feedUrl + "&c=" + continuationToken;
            }
            else
            {
                feedUrl = feedUrl.Substring(0, index) + "&c=" + continuationToken;
            }

            return new Uri(feedUrl); 
        }

        /// <summary>
        /// Helper function which converts the URI to the Google Reader Atom feed for a URL into the original feed URI 
        /// </summary>
        /// <param name="downloadUri">The URI to the Atom feed in Google Reader </param>
        /// <returns>The feed URI</returns>
        private static Uri CreateFeedUriFromDownloadUri(Uri downloadUri)
        {
            if(downloadUri == null)
                throw new ArgumentNullException("downloadUri");

            string downloadUrl = downloadUri.AbsoluteUri;
            int startIndex = feedUrlPrefix.Length + "feed/".Length;
            int endIndex = downloadUrl.IndexOf("?n=50");

            return new Uri(Uri.UnescapeDataString(downloadUrl.Substring(startIndex, endIndex - startIndex))); 
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
           return this.AsyncGetItemsForFeed(feedUrl, forceDownload, manual, null); 
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
        /// <param name="continuationToken">Token that indicates what "page" of the feed should be downloaded. If this
        /// value is not null then the 'feedUrl' parameter is should actually be the download URL for the feed.</param>
        /// <exception cref="ApplicationException">If the RSS feed is not version 0.91, 1.0 or 2.0</exception>
        /// <exception cref="XmlException">If an error occured parsing the RSS feed</exception>
        /// <exception cref="ArgumentNullException">If feedUrl is a null reference</exception>
        /// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
        /// <returns>true, if the request really was queued up</returns>
        /// <remarks>Result arraylist is returned by OnUpdatedFeed event within UpdatedFeedEventArgs</remarks>		
        //	[MethodImpl(MethodImplOptions.Synchronized)]
        private bool AsyncGetItemsForFeed(string feedUrl, bool forceDownload, bool manual, string continuationToken)
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
            if (continuationToken != null)
            {
                feedUri = CreateFeedUriFromDownloadUri(reqUri);
                reqUri = CreateContinuedDownloadUrl(reqUri, continuationToken); 
            }

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
                GoogleReaderNewsFeed theFeed = null;
                if (feedsTable.ContainsKey(feedUri.CanonicalizedUri()))
                    theFeed = feedsTable[feedUri.CanonicalizedUri()] as GoogleReaderNewsFeed;

                if (theFeed == null)
                    return false;

                if (continuationToken == null)
                {
                    reqUri = new Uri(CreateDownloadUrl(theFeed));
                }

                // only if we "real" go over the wire for an update:
                RaiseOnUpdateFeedStarted(feedUri, forceDownload, priority);

                //DateTime lastRetrieved = DateTime.MinValue; 
                DateTime lastModified = DateTime.MinValue;

                if (itemsTable.ContainsKey(feedUrl))
                {
                    etag = theFeed.etag;
                    lastModified = (theFeed.lastretrievedSpecified ? theFeed.lastretrieved : theFeed.lastmodified);
                }

                ICredentials c = null;

                RequestParameter reqParam =
                    RequestParameter.Create(reqUri, this.UserAgent, this.Proxy, c, lastModified, etag);
                // global cookie handling:
                reqParam.SetCookies = false;
                reqParam.Cookies = new CookieCollection();
                reqParam.Cookies.Add(MakeGoogleCookie(this.SID)); 

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
            Trace("AsyncRequest.OnRequestStart('{0}') downloading", requestUri.ToString());
            this.RaiseBeforeDownloadFeedStarted(CreateFeedUriFromDownloadUri(requestUri), ref cancel);
            if (!cancel)
                cancel = this.Offline;
        }


        /// <summary>
        /// Called when an exception occurs while downloading a feed.
        /// </summary>
        /// <param name="requestUri">The URI of the feed</param>
        /// <param name="e">The exception</param>
        /// <param name="priority">The priority of the request</param>
        protected override void OnRequestException(Uri requestUri, Exception e, int priority)
        {
            Trace("AsyncRequst.OnRequestException() fetching '{0}': {1}", requestUri.ToString(), e.ToString());

            string key = CreateFeedUriFromDownloadUri(requestUri).CanonicalizedUri();
            if (feedsTable.ContainsKey(key))
            {
                Trace("AsyncRequest.OnRequestException() '{0}' found in feedsTable.", requestUri.ToString());
                INewsFeed f = feedsTable[key];
                // now we set this within causedException prop.
                //f.lastretrieved = DateTime.Now; 
                //f.lastretrievedSpecified = true; 
                f.causedException = true;
            }
            else
            {
                Trace("AsyncRequst.OnRequestException() '{0}' NOT found in feedsTable.", requestUri.ToString());
            }

            RaiseOnUpdateFeedException(key, e, priority);
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
            Trace("AsyncRequest.OnRequestComplete: '{0}': {1}", requestUri.ToString(), result);
            if (newUri != null)
                Trace("AsyncRequest.OnRequestComplete: perma redirect of '{0}' to '{1}'.", requestUri.ToString(),
                      newUri.ToString());

            IList<INewsItem> itemsForFeed;
            
            //BUGBUG: This value is now incorrectly returned if feed has more than 50 items 
            bool firstSuccessfulDownload = false; 
            
            //we download feeds 50 items at a time, so it may take multiple requests to get all items
            bool feedDownloadComplete = true;

            Uri feedUri = CreateFeedUriFromDownloadUri(requestUri); 

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

                string feedUrl = theFeed.link;
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

                    //see if there are still more items to download for the feed
                    if (fi.OptionalElements.ContainsKey(continuationQName))
                    {
                        XmlNode continuationNode = RssHelper.GetOptionalElement(fi.OptionalElements, continuationQName.Name, continuationQName.Namespace);
                        this.AsyncGetItemsForFeed(requestUri.CanonicalizedUri(), true, true, continuationNode.InnerText);

                        feedDownloadComplete = false;
                    }
                    else
                    { //we're done downloading items from the feed
                        theFeed.lastretrieved = new DateTime(DateTime.Now.Ticks);
                        theFeed.lastretrievedSpecified = true;
                    }
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

        #region subscription editing methods 


        /// <summary>
        /// Deletes a feed from the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to delete</param>
        internal void DeleteFeedFromGoogleReader(string feedUrl)
        {
            if (!StringHelper.EmptyTrimOrNull(feedUrl))
            {
                string subscribeUrl = apiUrlPrefix + "subscription/edit";
                string feedId = "feed/" + feedUrl;               

                string body = "s=" + Uri.EscapeDataString(feedId) + "&T=" + GetGoogleEditToken(this.SID) + "&ac=unsubscribe&i=null" /* + "&t" + Uri.EscapeDataString(f.title) */ ; 
                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(subscribeUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }


                base.DeleteFeed(feedUrl);
            }
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
            GoogleReaderUpdater.DeleteFeedFromGoogleReader(this.GoogleUserName, feedUrl);  
        }


         /// <summary>
        /// Adds a feed to the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to add</param>
        internal void AddFeedInGoogleReader(string feedUrl)
        {
            this.AddFeedInGoogleReader(feedUrl, null); 
        }

        /// <summary>
        /// Adds a feed to the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to add</param>
        /// <param name="label">The label to apply to the feed. If no label is provided, then it is obtained from 
        /// the INewsFeed object in the feeds table that has the same feed URL.</param>
        private void AddFeedInGoogleReader(string feedUrl, string label)
        {
            if (!StringHelper.EmptyTrimOrNull(feedUrl) && (feedsTable.ContainsKey(feedUrl) || label != null))
            {
                
                INewsFeed f = null;

                if (label == null)
                {
                    f = feedsTable[feedUrl];
                }

                string subscribeUrl = apiUrlPrefix + "subscription/edit";
                string feedId = "feed/" + feedUrl;
                string labelParam = String.Empty;
              
                if (label == null)
                {
                    foreach (string category in f.categories)
                    {
                        GoogleReaderLabel grl = new GoogleReaderLabel(category, "user/" + this.GoogleUserId + "/label/" + category);                      
                        labelParam += "&a=" + Uri.EscapeDataString(grl.Id);
                    }

                }else{
                    labelParam = "&a=" + Uri.EscapeDataString("user/" + this.GoogleUserId + "/label/" + label); 
                }

                string body = "s=" + Uri.EscapeDataString(feedId) + "&T=" + GetGoogleEditToken(this.SID) + "&ac=subscribe" + labelParam;
                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(subscribeUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }
            }      
        
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
            feed = base.AddFeed(feed, feedInfo);

            string feedId = "feed/" + feed.link;
            List<GoogleReaderLabel> labels = new List<GoogleReaderLabel>();

            foreach (string category in feed.categories)
            {
                GoogleReaderLabel label = new GoogleReaderLabel(category, "user/" + this.GoogleUserId + "/label/" + category);
                labels.Add(label);
            }

            GoogleReaderSubscription sub = new GoogleReaderSubscription(feedId, feed.title, labels, 0);

            //Replace NewsFeed instance with GoogleReaderNewsFeed
            feedsTable.Remove(feed.link);
            feed = new GoogleReaderNewsFeed(sub, feed, this);
            feedsTable.Add(feed.link, feed);

            GoogleReaderUpdater.AddFeedInGoogleReader(this.GoogleUserName, feed.link); 

            return feed; 
        }


        /// <summary>
        /// Changes the title of a subscribed feed in Google Reader
        /// </summary>
        /// <remarks>This method does nothing if the new title is empty or null</remarks>
        /// <param name="url">The feed URL</param>
        /// <param name="title">The new title</param>
        internal void RenameFeedInGoogleReader(string url, string title)
        {            
            if (StringHelper.EmptyTrimOrNull(title))
            {
                return; 
            }

            if(!StringHelper.EmptyTrimOrNull(url) && feedsTable.ContainsKey(url)){

                GoogleReaderNewsFeed f = feedsTable[url] as GoogleReaderNewsFeed;

                if (f != null)
                {
                    string apiUrl = apiUrlPrefix + "subscription/edit";
                    string body = "ac=edit&i=null&T=" + GetGoogleEditToken(this.SID) + "&t=" + Uri.EscapeDataString(title) + "&s=" + Uri.EscapeDataString(f.GoogleReaderFeedId);

                    HttpWebResponse response = AsyncWebRequest.PostSyncResponse(apiUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException(response.StatusDescription); 
                    }
                }

            }// if(!StringHelper.EmptyTrimOrNull(url) && feedsTable.ContainsKey(url)){

        }

        #endregion 

        #region news item manipulation methods

        /// <summary>
        /// Invoked when a NewsItem owned by this FeedSource changes in a way that 
        /// needs to be communicated to Google Reader. 
        /// </summary>
        /// <param name="sender">the NewsItem</param>
        /// <param name="e">information on the property that changed</param>
        protected override void OnNewsItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NewsItem item = sender as NewsItem; 

            if(item == null){ 
                return; 
            }

            switch (e.PropertyName)
            {
                case "BeenRead":
                    if (item.Feed != null)
                    {
                        GoogleReaderNewsFeed f = item.Feed as GoogleReaderNewsFeed;
                        GoogleReaderUpdater.ChangeItemReadStateInGoogleReader(this.GoogleUserName, f.GoogleReaderFeedId, item.Id,  item.BeenRead);
                    }
                    break;

                case "FlagStatus":
                    if (item.Feed != null)
                    {
                        GoogleReaderNewsFeed f = item.Feed as GoogleReaderNewsFeed;
                        GoogleReaderUpdater.ChangeItemTaggedStateInGoogleReader(this.GoogleUserName, f.GoogleReaderFeedId, item.Id, "starred", item.FlagStatus != Flagged.None);
                    }
                    break;
            }
        }


        /// <summary>
        /// Marks an item as shared or unshared in Google Reader
        /// </summary>
        ///<param name="item">The item to share. </param>
        /// <param name="tagged">Indicates whether the item was shared or unshared</param>       
        public void ShareNewsItem(INewsItem item, bool shared)
        {
            if (item != null)
            {
                GoogleReaderNewsFeed f = item.Feed as GoogleReaderNewsFeed;
                if (f != null)
                {
                    GoogleReaderUpdater.ChangeItemTaggedStateInGoogleReader(this.GoogleUserName, f.GoogleReaderFeedId, item.Id, "broadcast", shared);
                }
            }
        }

        /// <summary>
        /// Marks an item as tagged or unstarred in Google Reader
        /// </summary>
        /// <param name="feedId">The ID of the parent feed in Google Reader</param>
        /// <param name="itemId">The atom:id of the news item</param>        
        /// <param name="tag">The tag to apply or remove</param>
        /// <param name="tagged">Indicates whether the item was tagged or untagged</param>
        internal void ChangeItemTaggedStateInGoogleReader(string feedId, string itemId, string tag, bool tagged)
        {
            string itemReadUrl = apiUrlPrefix + "edit-tag";
            string op = (tagged ? "&a=" : "&r="); //are we adding or removing read label?
            string tagLabel = Uri.EscapeDataString("user/" + this.GoogleUserId + "/state/com.google/" + tag);

            string body = "s=" + Uri.EscapeDataString(feedId) + "&i=" + Uri.EscapeDataString(itemId) + "&ac=edit-tags" + op + tagLabel + "&async=true&T=" + GetGoogleEditToken(this.SID);

            HttpWebResponse response = AsyncWebRequest.PostSyncResponse(itemReadUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new WebException(response.StatusDescription);
            }
        }

        /// <summary>
        /// Marks an item as read or unread in Google Reader
        /// </summary>
        /// <param name="feedId">The ID of the parent feed in Google Reader</param>
        /// <param name="itemId">The atom:id of the news item</param>        
        /// <param name="beenRead">Indicates whether the item was marked as read or unread</param>
        internal void ChangeItemReadStateInGoogleReader(string feedId, string itemId, bool beenRead)
        {
            string itemReadUrl = apiUrlPrefix + "edit-tag";
            string op = (beenRead ? "&a=" : "&r="); //are we adding or removing read label?
            string readLabel = Uri.EscapeDataString("user/" + this.GoogleUserId + "/state/com.google/read");

            string body = "s=" + Uri.EscapeDataString(feedId) + "&i=" + Uri.EscapeDataString(itemId) + "&ac=edit-tags" + op + readLabel + "&async=true&T=" + GetGoogleEditToken(this.SID);
           
            HttpWebResponse response = AsyncWebRequest.PostSyncResponse(itemReadUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new WebException(response.StatusDescription);
            }
        }

        /// <summary>
        /// Marks all items older than the the specified date as read in Google Reader
        /// </summary>
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="olderThan">The date from which to mark all items older than that date as read</param>
        internal void MarkAllItemsAsReadInGoogleReader(string feedUrl, DateTime olderThan)
        {
            if (!StringHelper.EmptyTrimOrNull(feedUrl) && feedsTable.ContainsKey(feedUrl))
            {
                GoogleReaderNewsFeed f = feedsTable[feedUrl] as GoogleReaderNewsFeed;
                DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                string markReadUrl = apiUrlPrefix + "mark-all-as-read";

                string body = "T=" + GetGoogleEditToken(this.SID) + "&ts=" + Convert.ToInt32((olderThan.ToUniversalTime() - UnixEpoch).TotalSeconds) + "&s=" + Uri.EscapeDataString(f.GoogleReaderFeedId);
                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(markReadUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }
            }

        }

        /// <summary>
        /// Marks all items stored in the internal cache of RSS items and in Google Reader as read
        /// for a particular feed.
        /// </summary>        
        /// <param name="feed">The RSS feed</param>
        public override void MarkAllCachedItemsAsRead(INewsFeed feed)
        {
           
            DateTime newestItemAge = DateTime.MinValue;

            if (feed != null && feed.containsNewMessages && !string.IsNullOrEmpty(feed.link) && itemsTable.ContainsKey(feed.link))
            {
                IFeedDetails fi = itemsTable[feed.link] as IFeedDetails;

                if (fi != null)
                {

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
                GoogleReaderUpdater.MarkAllItemsAsReadInGoogleReader(this.GoogleUserName, feed.link, newestItemAge); 
            }
        }

    

        #endregion 

        #region category related methods 

        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The category to add</param>
        /// <returns>The INewsFeedCategory instance that will actually be used to represent the category</returns>
        /// <exception cref="NotSupportedException">If the category is a subcategory. Google Reader doesn't support 
        /// nested categories</exception>
        public override INewsFeedCategory AddCategory(INewsFeedCategory cat)
        {
            if (cat == null)
                throw new ArgumentNullException("cat");

            if (cat.Value.IndexOf(FeedSource.CategorySeparator) != -1)
                throw new NotSupportedException("Google Reader does not support nested categories");

            if (this.categories.ContainsKey(cat.Value))
                return this.categories[cat.Value];

            this.categories.Add(cat.Value, cat);
            readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);

            GoogleReaderUpdater.AddCategoryInGoogleReader(this.GoogleUserName, cat.Value);
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

            if (cat.IndexOf(FeedSource.CategorySeparator)!= -1)
                throw new NotSupportedException("Google Reader does not support nested categories"); 

            if (this.categories.ContainsKey(cat))
                return this.categories[cat];

            category c = new category(cat);
            this.categories.Add(cat, c);
            readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);

            GoogleReaderUpdater.AddCategoryInGoogleReader(this.GoogleUserName, cat); 
            return c; 
        }

        /// <summary>
        /// Adds the category in Google Reader
        /// </summary>
        /// <param name="name">The name of the category to add</param>
        internal void AddCategoryInGoogleReader(string name)
        {
            /* 
             * Since Google Reader doesn't support an explicit 'create folder' we will create a new subscription
             * and place it in the new category then delete the subscription
             */ 
            string dummyFeed = "http://rss.netflix.com/QueueRSS?id=P2792793912689011005960561087208982";
            this.AddFeedInGoogleReader(dummyFeed, name);
            this.DeleteFeedFromGoogleReader(dummyFeed); 
        }

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
                IList<string> categories2remove = this.GetChildCategories(cat);
                categories2remove.Add(cat);

                //remove category and all its subcategories
                lock (this.categories)
                {
                    foreach (string c in categories2remove)
                    {
                        this.categories.Remove(c);
                        GoogleReaderUpdater.DeleteCategoryInGoogleReader(this.GoogleUserName, c); 
                    }
                }

                //remove feeds in deleted categories and subcategories
                var feeds2delete =
                   from f in this.feedsTable.Values
                   where categories2remove.Contains(f.category)
                   select f.link.ToString();

                string[] feeds2remove = feeds2delete.ToArray<string>();

                lock (this.feedsTable)
                {
                    foreach (var feedUrl in feeds2remove)
                    {
                        this.feedsTable.Remove(feedUrl);
                        GoogleReaderUpdater.DeleteFeedFromGoogleReader(this.GoogleUserName, feedUrl); 
                    }
                }

                readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);
            }// if (!StringHelper.EmptyTrimOrNull(cat) && categories.ContainsKey(cat))
        }

        /// <summary>
        /// Deletes the category in Google Reader
        /// </summary>
        /// <param name="name">The name of the category to delete</param>
        internal void DeleteCategoryInGoogleReader(string name)
        {
            if (!StringHelper.EmptyTrimOrNull(name))
            {
                string labelUrl = apiUrlPrefix + "disable-tag";
                string labelParams = "&s=" + "user/" + this.GoogleUserId + "/label/" + Uri.EscapeDataString(name) + "&t=" + Uri.EscapeDataString(name);              

                string body = "ac=disable-tags&i=null&T=" + GetGoogleEditToken(this.SID) + labelParams;
                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(labelUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }
            }
        }



         /// <summary>
        /// Renames the specified category
        /// </summary>        
        /// <remarks>This method assumes that the caller will rename categories on INewsFeed instances directly instead
        /// of having this method do it automatically.</remarks>
        /// <param name="oldName">The old name of the category</param>
        /// <param name="newName">The new name of the category</param>        
        internal void RenameCategoryInGoogleReader(string oldName, string newName)
        {
            //if no feed with category as label (e.g. newly created category), we need to create label in Google Reader 
            if (!feedsTable.Any(x => x.Value.categories.Contains(oldName)))
            {
                this.AddCategoryInGoogleReader(newName);
            }

            this.DeleteCategoryInGoogleReader(oldName); 
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
            if(( StringHelper.EmptyTrimOrNull(oldName) || StringHelper.EmptyTrimOrNull(newName) )
                || oldName.Equals(newName) ){
                return; 
            }

            //rename object in category table
            base.RenameCategory(oldName, newName);
            GoogleReaderUpdater.RenameCategoryInGoogleReader(this.GoogleUserName, oldName, newName);                       
        }

        /// <summary>
        /// Changes the category of a feed in Google Reader
        /// </summary>
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="newCategory">The new category for the feed</param>
        /// <param name="oldCategory">The old category of the feed.</param>
        internal void ChangeCategoryInGoogleReader(string feedUrl, string newCategory, string oldCategory)
        {
            if (!StringHelper.EmptyTrimOrNull(feedUrl) && feedsTable.ContainsKey(feedUrl))
            {
                GoogleReaderNewsFeed f = feedsTable[feedUrl] as GoogleReaderNewsFeed;
                string labelUrl = apiUrlPrefix + "subscription/edit";
                string labelParams = String.Empty;

                if (oldCategory != null)
                {
                    labelParams = "&r=" + "user/" + this.GoogleUserId + "/label/" + Uri.EscapeDataString(oldCategory); 
                }

                if (newCategory != null)
                {
                    labelParams += "&a=" + "user/" + this.GoogleUserId + "/label/" + Uri.EscapeDataString(newCategory); 
                }

                string body = "ac=edit&i=null&T=" + GetGoogleEditToken(this.SID) + "&t=" + Uri.EscapeDataString(f.title) + "&s=" + Uri.EscapeDataString(f.GoogleReaderFeedId) + labelParams;
                HttpWebResponse response = AsyncWebRequest.PostSyncResponse(labelUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }
            }
        }           

        #endregion 

        #endregion

    }

    #endregion 

    #region GoogleReaderSubscription 

    /// <summary>
    /// A class which represents a subscription obtained from the Google Reader feed list.  
    /// </summary>
    internal class GoogleReaderSubscription
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<GoogleReaderLabel> Categories { get; set; }
        public long FirstItemMSec { get; set; }
        
        public string FeedUrl
        {
            get
            {
                string url = Id.Substring("feed/".Length);
                Uri uri = null;
                if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    return uri.CanonicalizedUri(); 
                }
                else
                {
                    return url;
                }
            }
        }

        internal GoogleReaderSubscription(string id, string title, List<GoogleReaderLabel> categories, long firstitemmsec)
        {
            this.Id = id;
            this.Title = title;
            this.Categories = categories;
            this.FirstItemMSec = firstitemmsec; 
        }
    }

    #endregion 

    #region GoogleReaderLabel

    /// <summary>
    /// Represents a label in Google Reader
    /// </summary>
    internal class GoogleReaderLabel
    {
        public string Label { get; set; }
        public string Id { get; set; }

        internal GoogleReaderLabel(string label, string id){
            this.Label = label; 
            this.Id = id; 
        }
    }

    #endregion 

    #region GoogleReaderNewsFeed 

    /// <summary>
    /// Represents a news feed subscribed to in Google Reader. 
    /// </summary>
    [XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
    public class GoogleReaderNewsFeed : NewsFeed
    {
        #region constructors 

        /// <summary>
        /// We always want an associated GoogleReaderSubscription instance
        /// </summary>
        private GoogleReaderNewsFeed() { ;}

        /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="subscription">The GoogleReaderSubscription instance that this object will wrap</param>
        /// <param name="banditfeed">The object that contains the settings that will be used to initialize this class</param>
        /// <param name="owner">This object's owner</param>       
        internal GoogleReaderNewsFeed(GoogleReaderSubscription subscription, INewsFeed banditfeed, object owner)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");
            this.mysubscription = subscription;

            foreach (GoogleReaderLabel label in subscription.Categories)
            {
                this._categories.Add(label.Label);
            }

            if (banditfeed != null)
            {
                this.lastretrievedSpecified = banditfeed.lastretrievedSpecified;
                this.lastretrieved = banditfeed.lastretrieved;
                this.lastmodifiedSpecified = banditfeed.lastmodifiedSpecified;
                this.lastmodified = banditfeed.lastmodified;
                this.id = banditfeed.id;
                this.enclosurefolder = banditfeed.enclosurefolder;
                this.deletedstories = banditfeed.deletedstories;
                this.storiesrecentlyviewed = banditfeed.storiesrecentlyviewed;
                this.refreshrate = banditfeed.refreshrate;
                this.refreshrateSpecified = banditfeed.refreshrateSpecified;
                this.maxitemage = banditfeed.maxitemage;
                this.markitemsreadonexit = banditfeed.markitemsreadonexit;
                this.markitemsreadonexitSpecified = banditfeed.markitemsreadonexitSpecified;
                this.listviewlayout = banditfeed.listviewlayout;
                this.favicon = banditfeed.favicon;
                this.stylesheet = banditfeed.stylesheet;
                this.cacheurl = banditfeed.cacheurl;
                this.enclosurealert = banditfeed.enclosurealert;
                this.enclosurealertSpecified = banditfeed.enclosurealertSpecified;
                this.alertEnabled = banditfeed.alertEnabled;
                this.alertEnabledSpecified = banditfeed.alertEnabledSpecified;
                this.Any = banditfeed.Any;
                //this.AnyAttr = banditfeed.AnyAttr; don't copy over since it causes issues with xsi:type attribute
            }

            if (owner is GoogleReaderFeedSource)
            {
                this.owner = owner; 
            }
        }

        #endregion 

        #region private fields


        /// <summary>
        /// The subscription that this object wraps. 
        /// </summary>
        GoogleReaderSubscription mysubscription = null; 

        #endregion 

        #region public properties 

        /// <summary>
        /// The feed's ID in Google Reader 
        /// </summary>
        public string GoogleReaderFeedId
        {
            get { return this.mysubscription.Id; }
        }

        #endregion 

        #region INewsFeed implementation

        #region INewsFeed properties

        [XmlElement(DataType = "anyURI")]
        public override string link
        {
            get
            {

                return (mysubscription == null ? this._link : mysubscription.FeedUrl);
            }
            set
            {
                this._link = value; 
            }
        }


        public override string title
        {
            get
            {
                return mysubscription.Title;
            }
            set
            {
                GoogleReaderFeedSource myowner = owner as GoogleReaderFeedSource;

                if (myowner != null && !StringHelper.EmptyTrimOrNull(value) && !mysubscription.Title.Equals(value))
                {
                    myowner.GoogleReaderUpdater.RenameFeedInGoogleReader(myowner.GoogleUserName, this.link, value);
                    mysubscription.Title = value;
                }
            }
        }

        [XmlAttribute]
        public override string category
        {
            get
            {
                return base.category;
            }
            set
            {
                GoogleReaderFeedSource myowner = owner as GoogleReaderFeedSource;
               
                if (myowner != null && 
                    ((base.category == null && value != null) || !base.category.Equals(value)) )
                {
                    myowner.GoogleReaderUpdater.ChangeCategoryInGoogleReader(myowner.GoogleUserName, this.link, value, base.category);
                }
                base.category = value;                
            }
        }
        
        public override List<string> categories
        {
            get
            {
                return this._categories;
            }
            set
            {
                //TODO: Make this change the tags in Google Reader
            }
        }

        #endregion

        #region INewsFeed methods

        #endregion

        #endregion

    }

    #endregion 

}
