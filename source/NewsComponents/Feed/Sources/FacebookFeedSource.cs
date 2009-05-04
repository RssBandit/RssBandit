using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NewsComponents;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using System.Runtime.Serialization.Json;
using System.Net;
using System.Globalization;
using System.Security.Cryptography;

using log4net; 

using RssBandit.Common;
using RssBandit.Common.Logging;

using NewsComponents.Collections;
using NewsComponents.Net;
using NewsComponents.Threading;
using NewsComponents.Utils;

namespace NewsComponents.Feed
{

    #region IFacebookFeedSource interface: public FeedSource extensions
    /// <summary>
    /// public FeedSource extension offered by Facebook Feed Source
    /// </summary>
    public interface IFacebookFeedSource
    {      
        /// <summary>
        /// Sets the current auth token to be used for Facebook API requests. 
        /// </summary>
        ///<param name="item">The Facebook API application auth token</param>   
        void SetAuthToken(string authToken);   
    }

    #endregion

    #region FacebookFeedSource

    /// <summary>
    /// A FeedSource that retrieves user's stream and associated comments from Facebook. 
    /// </summary>
    internal class FacebookFeedSource : FeedSource, IFacebookFeedSource
    {

        #region private fields 

        /// <summary>
        /// The base URL from which to retrieve a user's news feed as an ActivityStreams feed.
        /// </summary>
        private static string ActivityStreamUrl = "http://www.facebook.com/activitystreams/feed.php"; 

        /// <summary>
        /// The Facebook user ID of the current user. 
        /// </summary>
        private string facebookUserId = String.Empty;

        /// <summary>
        /// The current session key for making Facebook API requests. 
        /// </summary>
        private string sessionKey = String.Empty;

        /// <summary>
        /// The client secret key that should be used when making requests. 
        /// </summary>
        private string clientSecret = String.Empty; 

        /// <summary>
        /// The current auth token for making Facebook API requests. 
        /// </summary>
        private string authToken = String.Empty; 

        /// <summary>
        /// The Facebook application ID for RSS Bandit
        /// </summary>
        private static string ApplicationId = "2d8ab36a639b61dd7a1a9dab4f7a0a5a";

        /// <summary>
        /// The start of the Unix epoch. Used to calculate If-Modified-Since semantics when fetching feeds. 
        /// </summary>
        private static DateTime unixEpoch = new DateTime(1970, 1, 1);

        // logging/tracing:
        private static readonly ILog _log = Log.GetLogger(typeof(GoogleReaderFeedSource));

        #endregion 

        #region Constructors

        /// <summary>
        /// Shouldn't be able to create one of these without specifying a location. 
        /// </summary>
        private FacebookFeedSource() { ; }

           /// <summary>
        /// Initializes a new instance of the <see cref="FeedSource"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="location">The user credentials</param>        
        internal FacebookFeedSource(INewsComponentsConfiguration configuration, SubscriptionLocation location)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = FeedSource.DefaultConfiguration;

            this.location = location;

            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);

            //If only Domain is set then we have the auth token otherwise we have secret + session key + uid
            if (String.IsNullOrEmpty(location.Credentials.UserName) && String.IsNullOrEmpty(location.Credentials.Password)
                && !String.IsNullOrEmpty(location.Credentials.Domain))
            {
                SetAuthToken(location.Credentials.Domain);
                GetSessionKey();
            }
            else
            {
                this.facebookUserId = location.Credentials.UserName; 
                this.clientSecret = location.Credentials.Password;
                this.sessionKey = location.Credentials.Domain; 
            }
           
            this.AsyncWebRequest = new AsyncWebRequest();
            this.AsyncWebRequest.OnAllRequestsComplete += this.OnAllRequestsComplete;              
        }

        #endregion 

        #region private methods

        /// <summary>
        /// Creates the news feed entry that represents a user's Facebook news feed. 
        /// </summary>
        /// <returns>The NewsFeed object that represents the Facebook news feed.</returns>
        private static NewsFeed CreateDefaultFacebookNewsFeed()
        {

            NewsFeed f = new NewsFeed();
            f.link = ActivityStreamUrl;
            f.title = SR.FacebookNewsFeedTitle;
            f.refreshrateSpecified = true;
            f.refreshrate = 1000 * 60 * 5; //refresh every five minutes
            f.stylesheet = SR.FacebookStyleSheet;

            return f; 
        }


        /// <summary>
        /// Retrieves a new session key using the current auth token. 
        /// </summary>
        private void GetSessionKey()
        {
            if (!Offline && !String.IsNullOrEmpty(authToken))
            {
                string SessionKeyUrl = String.Format("http://www.25hoursaday.com/weblog/CreateFBtoken.aspx?getsessionfor={0}", authToken);

                HttpWebRequest request = WebRequest.Create(SessionKeyUrl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FacebookSession));

                FacebookSession sessionInfo = (FacebookSession)serializer.ReadObject(response.GetResponseStream());
                location.Credentials.UserName = this.facebookUserId = sessionInfo.uid;
                location.Credentials.Domain   = this.sessionKey = sessionInfo.sessionkey;
                location.Credentials.Password = this.clientSecret = sessionInfo.secret;

                response.Close();
            }
        }

        /// <summary>
        /// Generates an MD5 hash of the parameters to the Facebook API call to retrieve the news feed
        /// </summary>
        /// <param name="facebookUserId">The user's Facebook ID</param>
        /// <param name="applicationId">The RSS Bandit application ID</param>
        /// <param name="sessionKey">The session key</param>
        /// <param name="clientSecret">The secret key of the client</param>
        /// <returns></returns>
        internal string GenerateSignature(string facebookUserId, string applicationId, string sessionKey, string clientSecret)
        {
            StringBuilder builder = new StringBuilder();            
            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}={1}", "app_id", applicationId));
            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}={1}", "session_key", sessionKey));
            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}={1}", "source_id", facebookUserId)); 
            builder.Append(this.clientSecret);

            byte[] buffer = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(builder.ToString().Trim()));
            builder = new StringBuilder();
            foreach (byte num in buffer)
            { 
                builder.Append(num.ToString("x2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }


        /// <summary>
        /// Helper function which converts the URI to the Facebook News Feed to a human readable form 
        /// </summary>
        /// <param name="downloadUri">The URI to download the Facebook news feed as an ActivityStream </param>
        /// <returns>The feed URI</returns>
        private static Uri CreateFeedUriFromDownloadUri(Uri downloadUri)
        {
            if (downloadUri == null)
                throw new ArgumentNullException("downloadUri");          
            return new Uri(downloadUri.AbsoluteUri.Replace(downloadUri.Query, String.Empty));
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
                cancel = Offline;
        }

        /// <summary>
        /// Called when an exception occurs while downloading a feed.
        /// </summary>
        /// <param name="requestUri">The URI of the feed</param>
        /// <param name="e">The exception</param>
        /// <param name="priority">The priority of the request</param>
        protected override void OnRequestException(Uri requestUri, Exception e, int priority)
        {
            Trace("AsyncRequst.OnRequestException() fetching '{0}': {1}", requestUri.ToString(), e.ToDescriptiveString());

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
            Uri feedUri = CreateFeedUriFromDownloadUri(requestUri);
           
            //is this the first time we've downloaded the feed on this machine? 
            bool firstSuccessfulDownload = false;

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

                    IInternalFeedDetails fi = RssParser.GetItemsForFeed(theFeed, response, false /* cachedStream */, false /* markitemsread */);
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

                    if (newReceivedItems.Count > 0)
                    {
                        theFeed.cacheurl = this.SaveFeed(theFeed);
                        SearchHandler.IndexAdd(newReceivedItems); // may require theFeed.cacheurl !
                    }

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
                            foreach (INewsItem ni in newReceivedItems)
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
                }
                else
                {
                    throw new NotImplementedException("Unhandled RequestResult: " + result);
                }
                
                    RaiseOnUpdatedFeed(feedUri, newUri, result, priority, firstSuccessfulDownload);                
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



        #region public methods

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
                NewsFeed f = CreateDefaultFacebookNewsFeed();
                myFeeds.feed.Add(f); 
            }

            //load news feed from Facebook using settings from subscriptions.xml
            this.BootstrapAndLoadFeedlist(myFeeds);            
        }
        

            /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this FeedSource</param>
        public override void BootstrapAndLoadFeedlist(feeds feedlist)
        {
            if (feedlist.feed.Count == 1)
            {
                this.feedsTable.Add(feedlist.feed[0].link, feedlist.feed[0]);
            }
            else
            {
                throw new ApplicationException(SR.ExceptionFacebookFeedlistCorrupted); 
            }
        }
        
        /// <summary>
        /// Sets the current application auth token to be used for Facebook API requests. 
        /// </summary>
        ///<param name="item">The Facebook API auth token</param>   
        public void SetAuthToken(string authToken)
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                this.authToken = authToken;
            }
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

            string etag = null;
            bool requestQueued = false;

            int priority = 10;
            if (forceDownload)
                priority += 100;
            if (manual)
                priority += 1000;

            Uri feedUri = new Uri(feedUrl);           

            NewsFeed theFeed = null; 

            //see if we've retrieved the news feed before 
             if (feedsTable.ContainsKey(feedUri.CanonicalizedUri()))
                    theFeed = feedsTable[feedUri.CanonicalizedUri()] as NewsFeed;

                if (theFeed == null) //feed list is corrupted 
                    return false;               

                // raise event only if we "really" go over the wire for an update:
                RaiseOnUpdateFeedStarted(feedUri, forceDownload, priority);

                //DateTime lastRetrieved = DateTime.MinValue; 
                DateTime lastModified = DateTime.MinValue;

                if (itemsTable.ContainsKey(feedUrl))
                {
                    etag = theFeed.etag;
                    lastModified = (theFeed.lastretrievedSpecified ? theFeed.lastretrieved : theFeed.lastmodified);
                }
            
            //generate request URL with appropriate parameters
            string parameters  = "?source_id={0}&app_id={1}&session_key={2}&sig={3}&v=0.7&read&updated_time={4}";
            DateTime lastRetrieved = theFeed.lastretrievedSpecified ? theFeed.lastretrieved : DateTime.Now - new TimeSpan(1, 0, 0, 0); //default is 1 day
            string updatedTime = ( (lastRetrieved.Ticks - unixEpoch.Ticks) / 10).ToString();

            string reqUrl = String.Format(feedUrl + parameters, this.facebookUserId, ApplicationId, this.sessionKey, GenerateSignature(this.facebookUserId, ApplicationId, sessionKey, this.clientSecret), updatedTime); 
            Uri reqUri = new Uri(reqUrl);

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
              
                ICredentials c = null;

                RequestParameter reqParam =
                    RequestParameter.Create(reqUri, this.UserAgent, this.Proxy, c, lastModified, etag);
               
                AsyncWebRequest.QueueRequest(reqParam,
                                             null,
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


        #endregion 

    }

    #endregion

    #region FacebookSession

    /// <summary>
    /// Encapsulates data required to initiate a Facebook API session
    /// </summary>
    [Serializable]
    public struct FacebookSession
    {
        public string sessionkey;
        public string uid;
        public string expires;
        public string secret; 
    }

    #endregion 
}
