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
using System.Runtime.Serialization; 

using log4net; 

using RssBandit.Common;
using RssBandit.Common.Logging;

using NewsComponents.Collections;
using NewsComponents.Net;
using NewsComponents.Threading;
using NewsComponents.Utils;
using NewsComponents.Resources;

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

         /// <summary>
        /// Retrieves a new session key using the current auth token. 
        /// </summary>
        void GetSessionKey();

        /// <summary>
        /// Retrieves the list of comments on the specified item from Facebook 
        /// </summary>
        /// <param name="item">The target item</param>
        /// <returns>A list of comments on the item</returns>
        List<INewsItem> GetCommentsForItem(INewsItem item);

        /// <summary>
        /// Gets the user's profile information from Facebook
        /// </summary>
        /// <returns>The user's profile information as a UserIdentity object</returns>
        UserIdentity GetUserIdentity();

        /// <summary>
        /// Tests whether the user has granted the 'stream_publish' permission. 
        /// </summary>
        /// <returns>True if the 'stream_publish' permission has been granted to RSS Bandit and false otherwise</returns>
        bool CanPublishToStream();
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
        /// The URL to the default image used when a profile pic cannot be discovered for a user. 
        /// </summary>
        private static readonly string DefaultFacebookProfilePic = "http://static.ak.fbcdn.net/pics/q_silhouette.gif";

        /// <summary>
        /// The URL to the default image used when a profile URL cannot be discovered for a user. 
        /// </summary>
        private static readonly string DefaultFacebookProfileUrl = "http://www.facebook.com"; 


        /// <summary>
        /// The base URL from which to retrieve a user's news feed as an ActivityStreams feed.
        /// </summary>
        public static readonly string FacebookApiUrl = "http://api.facebook.com/restserver.php";  

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
        /// Indicates whether the user has granted the 'publish_stream' extended permission
        /// </summary>
        private bool canPublishToStream = false; 

        /// <summary>
        /// The Facebook application ID for RSS Bandit
        /// </summary>
        private static string ApplicationId = "15028810303";

        /// <summary>
        /// The Facebook application key for RSS Bandit
        /// </summary>
        private static string ApplicationKey ="2d8ab36a639b61dd7a1a9dab4f7a0a5a";
        
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
            f.link = FacebookApiUrl;
            f.title = ComponentsText.FacebookNewsFeedTitle;
            f.refreshrateSpecified = true;
            f.refreshrate = 1000 * 60 * 5; //refresh every five minutes
            f.stylesheet = ComponentsText.FacebookStyleSheet;
            f.markitemsreadonexit = f.markitemsreadonexitSpecified = true; 

            return f; 
        }


        /// <summary>
        /// Retrieves a new session key using the current auth token. 
        /// </summary>
        public void GetSessionKey()
        {
            if (!Offline && !String.IsNullOrEmpty(authToken))
            { //http://www.facebook.com/login.php?api_key=2d8ab36a639b61dd7a1a9dab4f7a0a5a&&v=1.0&auth_token={1}&popup
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
        /// Converts a dictionary of parameter values into a sequential list. 
        /// </summary>
        /// <param name="parameterDictionary">The dictionary to convert</param>
        /// <returns>List containing the parameter values</returns>
        private static List<string> ParameterDictionaryToList(IEnumerable<KeyValuePair<string, string>> parameterDictionary)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> pair in parameterDictionary)
            {
                list.Add(string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { pair.Key }));
            }
            return list;
        }

        /// <summary>
        /// Converts a dictionary of query string parameters to an HTTP GET query string.
        /// </summary>
        /// <remarks>This also adds the standard parameters for all Facebook API calls</remarks>
        /// <param name="parameterList">The list of parameters for the method call</param>
        /// <param name="useJson">Dictates whether to use XML or JSON as the output format</param>
        /// <returns>The parameters as an HTTP GET query string</returns>
        private string CreateHTTPParameterList(IDictionary<string, string> parameterList, bool useJson)
        {
            StringBuilder builder = new StringBuilder();
            parameterList.Add("api_key", ApplicationKey);
            parameterList.Add("v", "1.0");
            parameterList.Add("format", useJson ? "JSON" : "XML");
            parameterList.Add("call_id", DateTime.Now.Ticks.ToString("x", CultureInfo.InvariantCulture));
            parameterList.Add("sig", this.GenerateSignature(parameterList));
            foreach (KeyValuePair<string, string> pair in parameterList)
            {
                builder.Append(pair.Key);
                builder.Append("=");
                builder.Append(Uri.EscapeDataString(pair.Value));
                builder.Append("&");
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        /// <summary>
        /// Generates a signature for use in Facebook API calls. 
        /// </summary>
        /// <param name="parameters">The parameters of the current request</param>
        /// <returns>The MD5 signature of the provided parameters</returns>
        private string GenerateSignature(IDictionary<string, string> parameters)
        {
            StringBuilder builder = new StringBuilder();
            List<string> list = ParameterDictionaryToList(parameters);
            list.Sort();
            foreach (string str in list)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}={1}", new object[] { str, parameters[str] }));
            }
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
            Uri feedUri = CreateFeedUriFromDownloadUri(requestUri);
            base.OnRequestException(feedUri, e, priority);             
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
            Uri feedUri = CreateFeedUriFromDownloadUri(requestUri);
            base.OnRequestComplete(feedUri, responseStream, response, newUri, eTag, lastModified, result, priority); 
        }

        /// <summary>
        /// Helper method which converts a Unix timestamp to a DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            return unixEpoch.AddSeconds(timestamp);
        }


        /// <summary>
        /// Helper function for making API call to Facebook. 
        /// </summary>
        /// <param name="parameters">The request parameters</param>
        /// <param name="useJson">Indicates whether to return response as XML or JSON</param>
        /// <returns>The HttpWebResponse</returns>
        private HttpWebResponse MakeRequest(Dictionary<string, string> parameters, bool useJson)
        {
            string reqUrl = FacebookApiUrl + "?" + CreateHTTPParameterList(parameters, useJson);
            HttpWebRequest request = WebRequest.Create(reqUrl) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return response; 
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
                feedlist.feed[0].owner = this; 
            }
            else
            {
                throw new ApplicationException(ComponentsText.ExceptionFacebookFeedlistCorrupted); 
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

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("viewer_id", this.facebookUserId); 
            parameters.Add("session_key", this.sessionKey);
            parameters.Add("method", "stream.get");

            /* FQL
            string query = String.Format("?query=select post_id, source_id, created_time, actor_id, target_id, app_id, message, attachment, comments, likes, permalink, attribution, type from stream where filter_key in (select filter_key FROM stream_filter where uid = {0} and type = 'newsfeed') and created_time >= {1} order by created_time desc limit 50", facebookUserId, updatedTime);
            parameters = "&v=1.0&method=fql.query&format=JSON&call_id={0}&session_key={1}&api_key={2}&sig={3}";
            */

            string reqUrl = feedUrl + "?" + CreateHTTPParameterList(parameters, false /* useJson */);

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
                reqParam = RequestParameter.Create(false, reqParam);

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

        /// <summary>
        /// Retrieves the list of comments on the specified item from Facebook 
        /// </summary>
        /// <param name="item">The target item</param>
        /// <returns>A list of comments on the item</returns>
        public List<INewsItem> GetCommentsForItem(INewsItem item)
        {
            //get commenters
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("post_id", item.Id);
            parameters.Add("session_key", this.sessionKey);
            parameters.Add("method", "stream.getComments");

            HttpWebResponse response = MakeRequest(parameters, true /* useJson */ );             

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<FacebookComment>));
            List<INewsItem> comments = new List<INewsItem>(); 
            List<FacebookComment> jsonComments = serializer.ReadObject(response.GetResponseStream()) as List<FacebookComment>;
            response.Close(); 
           
            //get info on commenters
            string uids   = String.Join(",", jsonComments.Select(comment => comment.fromid.ToString()).ToArray()); 
            string fields = "uid,first_name,last_name,pic_square,profile_url";

            parameters = new Dictionary<string, string>();
            parameters.Add("uids", uids);
            parameters.Add("fields", fields);
            parameters.Add("session_key", this.sessionKey);
            parameters.Add("method", "users.getInfo");

            response = MakeRequest(parameters, true /* useJson */ );             

            serializer = new DataContractJsonSerializer(typeof(List<FacebookUser>));
            List<FacebookUser> jsonUsers = serializer.ReadObject(response.GetResponseStream()) as List<FacebookUser>;
            response.Close(); 

            return CreateCommentNewsItems(item.Feed, item.FeedDetails, jsonComments, jsonUsers); 
        }


        /// <summary>
        /// Converts the input comments and list of users into a collection of INewsItem objects. 
        /// </summary>
        /// <param name="feed">The parent news feed of the item whose comments are being generated</param>
        /// <param name="feedDetails">The parent feed details object for the comments being generated</param>
        /// <param name="comments">The comments from Facebook</param>
        /// <param name="users">The list of users who posted the comments </param>
        /// <returns>A list of news items representing the Facebook comments</returns>
        private static List<INewsItem> CreateCommentNewsItems(INewsFeed feed, IFeedDetails feedDetails, List<FacebookComment> comments, List<FacebookUser> users){

           string htmlBody = @"<div class='comment_box'><div class='ufi_section'>
                                <div class='comment_profile_pic'>
                                 <a title='{0}' href='{1}'>
                                  <span class='UIRoundedImage UIRoundedImage_Small'><img class='UIRoundedImage_Image' src='{2}' /></span>
                                 </a>
                                </div>
                                <div class='comment_content'>
                                 <div class='comment_actions'>
                                  <a href='{1}'>{0}</a> - <span class='comment_meta_data'>{3}</span>
                                 </div>
                                 <div class='comment_text'><div class='comment_actual_text'>{4}</div>
                                </div>
                               </div>";

           List<INewsItem> items = new List<INewsItem>();
           
           foreach (FacebookComment c in comments)
           {
               FacebookUser u = users.FirstOrDefault(user => user.uid == c.fromid.ToString()); 
               DateTime pubdate = ConvertFromUnixTimestamp(c.time); 
               
               //handle situations where all nulls returned because commenter isn't viewer's friend or in their network
               string name = String.IsNullOrEmpty(u.firstname) && String.IsNullOrEmpty(u.lastname) ?
                                ComponentsText.FacebookUnknownUser : u.firstname + " " + u.lastname;
               u.picsquare = u.picsquare ?? DefaultFacebookProfilePic;
               u.profileurl = u.profileurl ?? DefaultFacebookProfileUrl;
               string content = String.Format(htmlBody, name, u.profileurl, u.picsquare, pubdate.ToString("h:mmtt MMM dd"), c.text);
               
               NewsItem n = new NewsItem(feed, String.Empty, String.Empty, content, pubdate, String.Empty);
               n.Author = name;
               n.FeedDetails = feedDetails;
               n.Id = c.id; 

               items.Add(n); 
           }

           return items; 
        }

        /// <summary>
        /// Gets the user's profile information from Facebook
        /// </summary>
        /// <returns>The user's profile information as a UserIdentity object</returns>
        public UserIdentity GetUserIdentity()
        {
            if (this.UserIdentity.ContainsKey(this.facebookUserId))
            {
                return this.UserIdentity[this.facebookUserId];
            }
            else
            {
                UserIdentity ui = null;
                string fields = "uid,first_name,last_name,pic_square,profile_url";

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("uids", this.facebookUserId);
                parameters.Add("fields", fields);
                parameters.Add("session_key", this.sessionKey);
                parameters.Add("method", "users.getInfo");

                HttpWebResponse response = MakeRequest(parameters, true /* useJson */ );             

                var serializer = new DataContractJsonSerializer(typeof(List<FacebookUser>));
                FacebookUser fbUser = (serializer.ReadObject(response.GetResponseStream()) as List<FacebookUser>).First();
                ui = new UserIdentity()
                {
                    RealName = fbUser.firstname + " " + fbUser.lastname,
                    Name = "Facebook",
                    ReferrerUrl = fbUser.profileurl
                };
                this.UserIdentity[this.facebookUserId] = ui;

                response.Close(); 
                return ui; 
            }
        }

        /// <summary>
        /// Tests whether the user has granted the 'stream_publish' permission. 
        /// </summary>
        /// <returns>True if the 'stream_publish' permission has been granted to RSS Bandit and false otherwise</returns>
        public bool CanPublishToStream()
        {
            //assume that if user has previously granted permission that we still have it now
            if (canPublishToStream)
                return true;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("ext_perm", "publish_stream");
            parameters.Add("session_key", this.sessionKey);
            parameters.Add("method", "users.hasAppPermission");

            HttpWebResponse response = MakeRequest(parameters, true /* useJson */ );             
            string responseValue = new StreamReader(response.GetResponseStream()).ReadToEnd();            
            response.Close();

            canPublishToStream = responseValue.Contains("1");
            return canPublishToStream; 
        }


        /// <summary>
        /// Posts a comment in reply to an item 
        /// </summary>
        /// <param name="url">The URL to post the comment to</param>
        /// <param name="item2post">An RSS item that will be posted to the website</param>
        /// <param name="inReply2item">An RSS item that is the post parent</param>		
        /// <exception cref="WebException">If an error occurs when the POSTing the 
        /// comment</exception>
        public override void PostComment(string url, INewsItem item2post, INewsItem inReply2item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("post_id", inReply2item.Id);
            parameters.Add("session_key", this.sessionKey);
            parameters.Add("method", "stream.addComment");
            parameters.Add("comment", item2post.Content);

            HttpWebResponse response = MakeRequest(parameters, true /* useJson */ );             
            response.Close();
        }

        #endregion 

    }

    #endregion


    #region FacebookComment

    /// <summary>
    /// Represents a comment made on a news feed item.
    /// </summary>
    [Serializable]
    public struct FacebookComment
    {
        public long fromid;
        public long time;
        public string text;
        public string id;
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

    #region FacebookUser

    /// <summary>
    /// Represents a Facebook user
    /// </summary>
    [DataContract]
    public struct FacebookUser
    {
        [DataMember]
        public string uid;
        [DataMember(Name="first_name")]
        public string firstname;
        [DataMember(Name = "last_name")]
        public string lastname;
        [DataMember(Name = "pic_square")]
        public string picsquare;
        [DataMember(Name = "profile_url")]
        public string profileurl;
    }

    #endregion 


    #region FacebookException 

    /// <summary>
    /// Represents an exception when retrieving or publishing data to or from Facebook 
    /// </summary>
    [Serializable]
    public class FacebookException : ApplicationException
    {
        /// <summary>
        /// The error code that was returned by Facebook. 
        /// </summary>
        public int ErrorCode { get; private set; }

        /// <summary>
        /// Always requires an error code
        /// </summary>
        private FacebookException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RssParserException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public FacebookException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RssParserException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected FacebookException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion 
}
