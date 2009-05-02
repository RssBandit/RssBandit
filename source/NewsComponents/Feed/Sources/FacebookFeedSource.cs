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

using RssBandit.Common;

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

            //set application auth token
            SetAuthToken(location.Credentials.Domain);
            GetSessionKey();
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
            string SessionKeyUrl = String.Format("http://www.25hoursaday.com/weblog/CreateFBtoken.aspx?getsessionfor={0}", authToken);

            HttpWebRequest request = WebRequest.Create(SessionKeyUrl) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse; 

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FacebookSession));

            FacebookSession sessionInfo = (FacebookSession) serializer.ReadObject(response.GetResponseStream());                        
            this.facebookUserId = sessionInfo.uid;
            this.sessionKey = sessionInfo.sessionkey;
            this.clientSecret = sessionInfo.secret;

            response.Close();
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
