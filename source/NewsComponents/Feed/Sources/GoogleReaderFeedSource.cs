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
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

using log4net;

using NewsComponents.Collections;
using NewsComponents.Net;
using NewsComponents.Threading;
using NewsComponents.Utils;

using RssBandit.Common;
using RssBandit.Common.Logging;
using NewsComponents.Resources;

namespace NewsComponents.Feed
{
	#region IGoogleReaderFeedSource interface: public FeedSource extensions
	/// <summary>
	/// public FeedSource extension offered by Google Reader Feed Source
	/// </summary>
	public interface IGoogleReaderFeedSource
	{
		/// <summary>
		/// Marks an item as shared or unshared in Google Reader
		/// </summary>
		///<param name="item">The item to share. </param>   
		void ShareNewsItem(INewsItem item);

		/// <summary>
		/// Gets true, if the item is shared, else false.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		bool NewsItemShared(INewsItem item);
	}
	#endregion

	#region GoogleReaderFeedSource

	/// <summary>
    /// A FeedSource that retrieves user subscriptions and feeds from Google Reader. 
    /// </summary>
	internal class GoogleReaderFeedSource : FeedSource, IGoogleReaderFeedSource
    {

        #region private fields

        // logging/tracing:
        private static readonly ILog _log = Log.GetLogger(typeof(GoogleReaderFeedSource));

        /// <summary>
        /// The URL for authenticating a Google user.
        /// </summary>
        private static readonly string authUrl = @"https://www.google.com/accounts/ClientLogin"; 

        /// <summary>
        /// The body of the request that will authenticate the Google user. 
        /// </summary>
        private static readonly string authBody = @"accountType=GOOGLE&service=reader&source=RssBandit&Email={0}&Passwd={1}";

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
        /// Qname for the crawl timestamp of a feed item that indicates the age of the news item to Google Reader
        /// </summary>
        private static readonly XmlQualifiedName crawltimestampQname = new XmlQualifiedName("crawl-timestamp-msec", "http://www.google.com/schemas/reader/atom/");

        /// <summary>
        /// This is the most recently retrieved edit token from the Google Reader service. 
        /// </summary>
        private static string MostRecentGoogleEditToken = null; 

        /// <summary>
        /// Authentication token which identifies the user. 
        /// </summary>
        private string AuthToken = String.Empty;

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
        /// Saves the feed list to the SubscriptionLocation.Location. The feed is written in
        /// the RSS Bandit feed file format as described in feeds.xsd
        /// </summary>
        public override void SaveFeedList()
        {
            base.SaveFeedList();
            GoogleReaderUpdater.SavePendingOperations(); 
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
			
			//TR: gr is dead now
			
            ////load feed list from Google Reader and use settings from subscriptions.xml
            //this.BootstrapAndLoadFeedlist(myFeeds);
            //GoogleReaderUpdater.StartBackgroundThread(); 
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

            if (Offline)
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
                    if (!feedsTable.ContainsKey(gfeed.FeedUrl))
                    {
                        this.feedsTable.Add(gfeed.FeedUrl, new GoogleReaderNewsFeed(gfeed, (bootstrapFeeds.TryGetValue(gfeed.FeedUrl, out feed) ? feed : null), this));
                    }
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
        /// Loads the user's category list from Google Reader. 
        /// </summary>
        private IEnumerable<string> LoadTaglistFromGoogleReader()
        {
            string taglistUrl = apiUrlPrefix + "tag/list";

            //load tag list XML
            XmlDocument doc = new XmlDocument();
			doc.Load(XmlReader.Create(SyncWebRequest.GetResponseStream(taglistUrl, null, this.Proxy, MakeGoogleAuthHeader(this.AuthToken))));

            string temp = doc.SelectSingleNode("/object/list/object/string[contains(string(.), 'state/com.google/starred')]").InnerText;
            this.GoogleUserId = temp.Replace("/state/com.google/starred", "").Substring(5);

            var taglist = from XmlNode node in doc.SelectNodes("/object/list[@name='tags']/object/string[@name='id']")
                          where node.InnerText.IndexOf("/com.google/") == -1 && node.InnerText.IndexOf("/com.blogger/") == -1
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
			var doc = new XPathDocument(XmlReader.Create(SyncWebRequest.GetResponseStream(feedlistUrl, null, this.Proxy, MakeGoogleAuthHeader(this.AuthToken))));
            var nav = doc.CreateNavigator(); 

            var feedlist = from XPathNavigator node in nav.Select("/object/list[@name='subscriptions']/object")
                           select MakeSubscription(node);
            
            return feedlist; 
        }


        /// <summary>
        /// This method fetches the feed list from Google Reader, figures out the differences between it and our local feed list, 
        /// then applies the changes. 
        /// </summary>
        private void UpdateFeedList()
        {
			//TR: gr is dead now
			return;

            bool feedlistModified = false, categoriesModified = false; 

            IEnumerable<GoogleReaderSubscription> gReaderFeeds = this.LoadFeedlistFromGoogleReader();
            var addedFeeds = from feed in gReaderFeeds
                             where !feedsTable.ContainsKey(feed.FeedUrl)
                             select feed;

            foreach (GoogleReaderSubscription newFeed in addedFeeds)
            {
                this.feedsTable.Add(newFeed.FeedUrl, new GoogleReaderNewsFeed(newFeed,null, this));
                RaiseOnAddedFeed(new FeedChangedEventArgs(newFeed.FeedUrl));
                feedlistModified = true; 
            }
            
            var removedFeeds = from url in feedsTable.Keys
                               where !gReaderFeeds.Any(gf => gf.FeedUrl == url)
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
            
            var movedFeeds   = from gfeed in gReaderFeeds
                               where feedsTable.ContainsKey(gfeed.FeedUrl) && !gfeed.Categories.Any(l => l.Label.Equals(feedsTable[gfeed.FeedUrl].category))
                               select gfeed;

            foreach (GoogleReaderSubscription relocatedFeed in movedFeeds)
            {
                INewsFeed f = null;
                if (feedsTable.TryGetValue(relocatedFeed.FeedUrl, out f))
                {
                    f.categories.Clear();
                    f.categories.AddRange(relocatedFeed.Categories.Select(l => l.Label));
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

        /// <summary>
        /// Helper function which creates a Google Reader subscription node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static GoogleReaderSubscription MakeSubscription(XPathNavigator node)
        {
            string feedid = null, title = null;
            long firstitemmsec = 0; 

            //node starts positioned on <object>
            node.MoveToFirstChild(); //move to first <string> 

            while (node.Name == "string") //iterate through each <string>
            {
                node.MoveToAttribute("name", String.Empty);
                switch (node.Value)
                {
                    case "id":
                        node.MoveToParent();
                        feedid = node.Value;
                        break; 
                    case "title":
                        node.MoveToParent();
                        title = node.Value;
                        break;      
                    default:
                        node.MoveToParent();
                        break; 
                }

                node.MoveToNext(XPathNodeType.Element); 
            }

            node.MoveToParent(); //go back to <object>
            List<GoogleReaderLabel> categories = MakeLabelList(node.Select("list[@name='categories']/object").Cast<XPathNavigator>());

            return new GoogleReaderSubscription(feedid, title, categories, firstitemmsec);         
        }


        /// <summary>
        /// Helper function which makes a list of Google Reader labels
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static List<GoogleReaderLabel> MakeLabelList(IEnumerable<XPathNavigator> nodes)
        {
            List<GoogleReaderLabel> labels = new List<GoogleReaderLabel>(); 

            foreach (XPathNavigator node in nodes) {
                string catid = null, label = null;

                //node starts positioned on <object>
                node.MoveToFirstChild(); //move to first <string> 

                while (node.Name == "string") //iterate through each <string>
                {
                    node.MoveToAttribute("name", String.Empty);
                    switch (node.Value)
                    {
                        case "id":
                            node.MoveToParent();
                            catid = node.Value;
                            break;
                        case "label":
                            node.MoveToParent();
                            label = node.Value;
                            break;
                        default:
                            node.MoveToParent();
                            break; 
                    }

                    if (!node.MoveToNext(XPathNodeType.Element)) break; 
                }//while

                labels.Add(new GoogleReaderLabel(label, catid));                 
            }//foreach

            return labels; 
        }

        #endregion

        #region user authentication methods

        /// <summary>
        /// Authenticates a user to Google's services and obtains their SID
        /// </summary>
        private void AuthenticateUser()
        {
            string body = String.Format(authBody, location.Credentials.UserName, location.Credentials.Password);
            try
            {
				StreamReader reader = new StreamReader(SyncWebRequest.PostResponseStream(authUrl, body, null, null, this.Proxy));
                string[] response = reader.ReadToEnd().Split('\n');

                foreach (string s in response)
                {
                    if (s.StartsWith("Auth=", StringComparison.Ordinal))
                    {
                        this.AuthToken = s.Substring(5);
                        return;
                    }
                }
            }
            catch (ClientCertificateRequiredException) //Google returns a 403 instead of a 401 on invalid password
            {
                throw new ResourceAuthorizationException(); 
            }

            throw new WebException("Could not authenticate user to Google Reader because no authentication token provided in response", WebExceptionStatus.UnknownError);
        }

		/// <summary>
		/// Returns an HTTP authorization header with specified auth token
		/// </summary>
		/// <param name="sid">The user's SID.</param>
		/// <returns>
        /// an HTTP authorization header created from the auth token
		/// </returns>
        private static WebHeaderCollection MakeGoogleAuthHeader(string authToken)
        {
           var header = new WebHeaderCollection();
           header.Add("Authorization", "GoogleLogin auth=" + authToken);            
           return header; 
        }

        /// <summary>
        /// Gets an edit token which is needed for any edit operations using the Google Reader API
        /// </summary>
        /// <param name="sid">The user's auth token</param>
        /// <returns>The edit token</returns>
        private static string GetGoogleEditToken(string authToken)
        {
            string tokenUrl = apiUrlPrefix + "token";
            HttpWebRequest request = HttpWebRequest.Create(tokenUrl) as HttpWebRequest;
            request.Timeout        = 5 * 1000; //5 second time out
            request.Headers = MakeGoogleAuthHeader(authToken); 

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
        /// <param name="fetchAll">Indicates that all items from the feed should be fetched not just the items since 
        /// the last time the feed was retrieved</param>
        /// <returns></returns>
        private static string CreateDownloadUrl(GoogleReaderNewsFeed feed, bool fetchAll)
        {
            //either download all items since last retrieved or last 1 month of stuff (default if value to low) if never fetched items from feed
            TimeSpan maxItemAge = (feed.lastretrievedSpecified && !fetchAll ? feed.lastretrieved - UnixEpoch : new TimeSpan(0,0,1));
            string feedUrl = feedUrlPrefix + Uri.EscapeDataString(feed.GoogleReaderFeedId) + "?n=50&r=o&ot=" + Convert.ToInt64(maxItemAge.TotalSeconds);          

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
        /// Downloads every feed that has either never been downloaded before or 
        /// whose elapsed time since last download indicates a fresh attempt should be made. 
        /// </summary>
        /// <param name="forceDownload">A flag that indicates whether download attempts should be made 
        /// or whether the cache can be used.</param>
        /// <remarks>This method uses the cache friendly If-None-Match and If-modified-Since
        /// HTTP headers when downloading feeds.</remarks>	
        public override void RefreshFeeds(bool forceDownload)
        {
            if (!Offline)
            {
                try
                {
                    var eventX = new System.Threading.ManualResetEvent(false);
                    //perform task in background thread and timeout because for some reason sync HTTP web request hangs in certain cases
                    PriorityThreadPool.QueueUserWorkItem(
                                delegate
                                {
                                    this.UpdateFeedList();
                                    eventX.Set();
                                }
                                ,(int)ThreadPriority.Normal
                                );                    

                    //wait 2 minutes for above task to complete then move on
                    eventX.WaitOne(120000, true); 
                }
                catch (WebException we)
                {
                    _log.Error(we.Message, we);
                }
            }
            base.RefreshFeeds(forceDownload); 
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
                GoogleReaderNewsFeed theFeed = null;
                if (feedsTable.ContainsKey(feedUri.CanonicalizedUri()))
                    theFeed = feedsTable[feedUri.CanonicalizedUri()] as GoogleReaderNewsFeed;

                if (theFeed == null)
                    return false;

                if (continuationToken == null)
                {
                    reqUri = new Uri(CreateDownloadUrl(theFeed, manual));
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
                reqParam.Headers = MakeGoogleAuthHeader(this.AuthToken); 

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
            Trace("AsyncRequest.OnRequestComplete: '{0}': {1}", requestUri.ToString(), result);
            if (newUri != null)
                Trace("AsyncRequest.OnRequestComplete: perma redirect of '{0}' to '{1}'.", requestUri.ToString(),
                      newUri.ToString());

            IList<INewsItem> itemsForFeed;            
            Uri feedUri = CreateFeedUriFromDownloadUri(requestUri);
                       
            //we download feeds 50 items at a time, so it may take multiple requests to get all items
            bool feedDownloadComplete = true;

             //we download feed immediately after added in UI so it would not have been added to Google Reader yet
            bool notInGoogleReaderYet = GoogleReaderUpdater.IsPendingSubscription(feedUri.CanonicalizedUri());

            //is this the first time we've downloaded the feed on this machine? 
            bool firstSuccessfulDownload = notInGoogleReaderYet; 

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
                        if (notInGoogleReaderYet) //news items will come marked as read if not in our subscriptions
                        {
                            ri.BeenRead = false;
                        }
                        else if (ri.BeenRead)
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

        #region subscription editing methods 


        /// <summary>
        /// Deletes a feed from the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to delete</param>
        /// <param name="feedTitle">The title of the feed to delete</param>
        internal void DeleteFeedFromGoogleReader(string feedUrl, string feedTitle)
        {
            if (!string.IsNullOrWhiteSpace(feedUrl))
            {
                string subscribeUrl = apiUrlPrefix + "subscription/edit";
                string feedId = "feed/" + feedUrl;
                string feedTitleParam = string.IsNullOrWhiteSpace(feedTitle) ? String.Empty : "&t=" + Uri.EscapeDataString(feedTitle); 

                string body = "s=" + Uri.EscapeDataString(feedId) + "&T=" + GetGoogleEditToken(this.AuthToken) + "&ac=unsubscribe&i=null" + feedTitleParam ;
				HttpWebResponse response = SyncWebRequest.PostResponse(subscribeUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException(response.StatusDescription);
                }


                base.DeleteFeed(feedUrl);

                /* close the response stream to prevent threadpool deadlocks and resource leaks */ 
                try 
                {
                    response.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// Deletes all subscribed feeds and categories 
        /// </summary>
        /// <param name="deleteFromSource">Indicates whether the feeds should also be deleted from the feed source</param>
        public override void DeleteAllFeedsAndCategories(bool deleteFromSource)
        {
            if (deleteFromSource)
            {
                GoogleReaderUpdater.CancelPendingOperations();
            }
            else
            {
                GoogleReaderUpdater.UnregisterFeedSource(this); 
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
            INewsFeed f = null;
            if (feedsTable.TryGetValue(feedUrl, out f))
            {
                GoogleReaderUpdater.DeleteFeedFromGoogleReader(this.GoogleUserName, feedUrl, f.title);
                base.DeleteFeed(feedUrl); 
            }
        }


         /// <summary>
        /// Adds a feed to the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to add</param>
        /// <param name="title">The title of the new subscription</param>
        internal void AddFeedInGoogleReader(string feedUrl,string title)
        {
            this.AddFeedInGoogleReader(feedUrl, title, null); 
        }

        /// <summary>
        /// Adds a feed to the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to add</param>
        /// <param name="title">The title of the new subscription</param>
        /// <param name="label">The label to apply to the feed. If no label is provided, then it is obtained from 
        /// the INewsFeed object in the feeds table that has the same feed URL.</param>
        private void AddFeedInGoogleReader(string feedUrl, string title, string label)
        {
            if (!string.IsNullOrWhiteSpace(feedUrl) && (feedsTable.ContainsKey(feedUrl) || label != null))
            {               

                /* first add the feed */
                string subscribeUrl = apiUrlPrefix + "subscription/quickadd";
               
                string body = "quickadd=" + Uri.EscapeDataString(feedUrl) + "&T=" + GetGoogleEditToken(this.AuthToken);
				HttpWebResponse response = SyncWebRequest.PostResponse(subscribeUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

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
                    response.Close();
                }

                /* specify the title and label */
                INewsFeed f = null;
                string editUrl = apiUrlPrefix + "subscription/edit";
                string feedId = "feed/" + feedUrl;
                string labelParam = String.Empty;

                if (label == null)
                {
                    f = feedsTable[feedUrl];

                    foreach (string category in f.categories)
                    {
                        GoogleReaderLabel grl = new GoogleReaderLabel(category, "user/" + this.GoogleUserId + "/label/" + category);
                        labelParam += "&a=" + Uri.EscapeDataString(grl.Id);
                    }

                }
                else
                {
                    labelParam = "&a=" + Uri.EscapeDataString("user/" + this.GoogleUserId + "/label/" + label);
                }

                body = "s=" + Uri.EscapeDataString(feedId) + "&t=" + Uri.EscapeDataString(title) + "&T=" + GetGoogleEditToken(this.AuthToken) + "&ac=edit" + labelParam;
				response = SyncWebRequest.PostResponse(editUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

                try
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException(response.StatusDescription);
                    }
                }
                finally
                {
                    response.Close();
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
            if (feed != null)
            {
                if (feed.category != null)
                {
                    if (feed.category.IndexOf(FeedSource.CategorySeparator) != -1)
                        throw new NotSupportedException("Google Reader does not support nested categories");
                }

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
                lock (feedsTable)
                {
                    feedsTable.Remove(feed.link);
                    feed = new GoogleReaderNewsFeed(sub, feed, this);
                    feedsTable.Add(feed.link, feed);
                }
                GoogleReaderUpdater.AddFeedInGoogleReader(this.GoogleUserName, feed.link, feed.title);
            }
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
            if (string.IsNullOrWhiteSpace(title))
            {
                return; 
            }

            if(!string.IsNullOrWhiteSpace(url) && feedsTable.ContainsKey(url)){

                GoogleReaderNewsFeed f = feedsTable[url] as GoogleReaderNewsFeed;

                if (f != null)
                {
                    string apiUrl = apiUrlPrefix + "subscription/edit";
                    string body = "ac=edit&i=null&T=" + GetGoogleEditToken(this.AuthToken) + "&t=" + Uri.EscapeDataString(title) + "&s=" + Uri.EscapeDataString(f.GoogleReaderFeedId);

					HttpWebResponse response = SyncWebRequest.PostResponse(apiUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

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

            }// if(!string.IsNullOrWhiteSpace(url) && feedsTable.ContainsKey(url)){

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
                FeedInfo fi = this.GetFeedDetails(feed.link) as FeedInfo;
                this.DeleteFeed(feed.link);

                feed = new NewsFeed(feed); 
                feed.link = newUrl;
                feed = this.AddFeed(feed, fi);
            }

            return feed;
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
        ///<param name="item">The item to share or unshare. </param>
		 public void ShareNewsItem(INewsItem item)
        {
            if (item != null)
            {                
                GoogleReaderNewsFeed f = item.Feed as GoogleReaderNewsFeed;
                if (f != null)
                {
                    bool shared = true;
                    XmlQualifiedName qname = new XmlQualifiedName("broadcast", "http://www.google.com/reader/");
                    XmlElement elem = RssHelper.GetOptionalElement(item, qname);
                    if (elem == null)
                    {
                        elem = RssHelper.CreateXmlElement("gr", qname, "1");
                    }
                    else
                    {
                        shared = elem.InnerText != "1";
                        elem.InnerText = elem.InnerText == "1" ? "0" : "1";
                        item.OptionalElements.Remove(qname);
                    }
                    item.OptionalElements.Add(qname, elem.OuterXml);                   
                    GoogleReaderUpdater.ChangeItemTaggedStateInGoogleReader(this.GoogleUserName, f.GoogleReaderFeedId, item.Id, "broadcast", shared);
                }
            }
        }

		/// <summary>
		/// Gets true, if the item is shared, else false.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public bool NewsItemShared(INewsItem item)
		{
            bool shared = false;

            if (item != null)
            {
                XmlQualifiedName qname = new XmlQualifiedName("broadcast", "http://www.google.com/reader/");
                XmlElement elem = RssHelper.GetOptionalElement(item, qname);
                if (elem != null)
                {
                    shared = elem.InnerText == "1";
                }
            }

            return shared; 
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

            string body = "s=" + Uri.EscapeDataString(feedId) + "&i=" + Uri.EscapeDataString(itemId) + "&ac=edit-tags" + op + tagLabel + "&async=true&T=" + GetGoogleEditToken(this.AuthToken);

			HttpWebResponse response = SyncWebRequest.PostResponse(itemReadUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

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

            string body = "s=" + Uri.EscapeDataString(feedId) + "&i=" + Uri.EscapeDataString(itemId) + "&ac=edit-tags" + op + readLabel + "&async=true&T=" + GetGoogleEditToken(this.AuthToken);
           
            HttpWebResponse response = SyncWebRequest.PostResponse(itemReadUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

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
        /// Marks all items older than the the specified date as read in Google Reader
        /// </summary>
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="olderThan">The date from which to mark all items older than that date as read</param>
        internal void MarkAllItemsAsReadInGoogleReader(string feedUrl, string olderThan)
        {
            if (!string.IsNullOrWhiteSpace(feedUrl) && feedsTable.ContainsKey(feedUrl))
            {
                GoogleReaderNewsFeed f = feedsTable[feedUrl] as GoogleReaderNewsFeed;                
                string markReadUrl = apiUrlPrefix + "mark-all-as-read";

                string body = "T=" + GetGoogleEditToken(this.AuthToken) + "&ts=" + olderThan
                               + "&s=" + Uri.EscapeDataString(f.GoogleReaderFeedId) + "&t=" + Uri.EscapeDataString(f.title); 
                HttpWebResponse response = SyncWebRequest.PostResponse(markReadUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

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
        /// Marks all items stored in the internal cache of RSS items and in Google Reader as read
        /// for a particular feed.
        /// </summary>        
        /// <param name="feed">The RSS feed</param>
        public override void MarkAllCachedItemsAsRead(INewsFeed feed)
        {           
            //we need to use the time stamp from the newest news item (in microseconds) to indicate which items should be marked read
            DateTime newestItemAge = DateTime.MinValue;
            string newestItemTimeStamp = ( (DateTime.Now.Ticks - UnixEpoch.Ticks) / 10).ToString();  //default value is right now

            if (feed != null && feed.containsNewMessages && !string.IsNullOrEmpty(feed.link) && itemsTable.ContainsKey(feed.link))
            {
                IFeedDetails fi = itemsTable[feed.link] as IFeedDetails;

                if (fi != null)
                {

                    foreach (NewsItem ri in fi.ItemsList)
                    {
                        ri.PropertyChanged -= this.OnNewsItemPropertyChanged;
                        ri.BeenRead = true;

                        if (ri.Date > newestItemAge)
                        {
                            newestItemAge = ri.Date;
                            if (ri.OptionalElements.ContainsKey(crawltimestampQname))
                            {
                                newestItemTimeStamp = RssHelper.GetOptionalElement(ri, crawltimestampQname).InnerText + "999";
                            }
                        }
                        
                        ri.PropertyChanged += this.OnNewsItemPropertyChanged;
                    }
                }

                feed.containsNewMessages = false;
            }

            if (newestItemAge != DateTime.MinValue)
            {
                GoogleReaderUpdater.MarkAllItemsAsReadInGoogleReader(this.GoogleUserName, feed.link, newestItemTimeStamp); 
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
            if (string.IsNullOrWhiteSpace(cat))
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
            this.DeleteFeedFromGoogleReader(dummyFeed, String.Empty); 
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
            if (!string.IsNullOrWhiteSpace(cat) && categories.ContainsKey(cat))
            {
                IList<string> categories2remove = this.GetChildCategories(cat);
                categories2remove.Add(cat);
              

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
                        this.DeleteFeed(feedUrl);                         
                        /*  this.feedsTable.Remove(feedUrl);
                        * GoogleReaderUpdater.DeleteFeedFromGoogleReader(this.GoogleUserName, feedUrl); 
                        */ 
                    }
                }

                //remove category and all its subcategories
                lock (this.categories)
                {
                    foreach (string c in categories2remove)
                    {
                        this.categories.Remove(c);
                        GoogleReaderUpdater.DeleteCategoryInGoogleReader(this.GoogleUserName, c);
                    }
                }

                readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(this.feedsTable); 
                readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(this.categories);
            }// if (!string.IsNullOrWhiteSpace(cat) && categories.ContainsKey(cat))
        }

        /// <summary>
        /// Deletes the category in Google Reader
        /// </summary>
        /// <param name="name">The name of the category to delete</param>
        internal void DeleteCategoryInGoogleReader(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string labelUrl = apiUrlPrefix + "disable-tag";
                string labelParams = "&s=" + "user/" + this.GoogleUserId + "/label/" + Uri.EscapeDataString(name) + "&t=" + Uri.EscapeDataString(name);              

                string body = "ac=disable-tags&i=null&T=" + GetGoogleEditToken(this.AuthToken) + labelParams;
                HttpWebResponse response = SyncWebRequest.PostResponse(labelUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

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
            if(( string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName) )
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
            if (!string.IsNullOrWhiteSpace(feedUrl) && feedsTable.ContainsKey(feedUrl))
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

                string body = "ac=edit&i=null&T=" + GetGoogleEditToken(this.AuthToken) + "&t=" + Uri.EscapeDataString(f.title) + "&s=" + Uri.EscapeDataString(f.GoogleReaderFeedId) + labelParams;
                HttpWebResponse response = SyncWebRequest.PostResponse(labelUrl, body, MakeGoogleAuthHeader(this.AuthToken), null, this.Proxy);

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
                return url;                
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
                this.p_categories.Add(label.Label);
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

        #region public methods 

        /// <summary>
        /// Sets the GoogleReaderSubscription object represented by this object
        /// </summary>
        /// <param name="sub">The GoogleReaderSubscription instance</param>
        internal void SetSubscription(GoogleReaderSubscription sub)
        {
            if (sub != null)
            {
                lock (this)
                {
                    mysubscription = sub; 
                }
            }
        }

        #endregion 

        #region INewsFeed implementation

        #region INewsFeed properties

        [XmlElement(DataType = "anyURI")]
        public override string link
        {
            get
            {

                return (mysubscription == null ? this.p_link : mysubscription.FeedUrl);
            }
            set
            {
                this.p_link = value; 
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

                if (myowner != null && !string.IsNullOrWhiteSpace(value) && !mysubscription.Title.Equals(value))
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
                return this.p_categories;
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
