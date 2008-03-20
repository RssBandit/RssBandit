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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

using NewsComponents.Net;
using NewsComponents.Search;
using NewsComponents.Utils;

using RssBandit.Common;

namespace NewsComponents.Feed
{

    #region GoogleReaderFeedSource

    /// <summary>
    /// A FeedSource that retrieves user subscriptions and feeds from Google Reader. 
    /// </summary>
    class GoogleReaderFeedSource : FeedSource
    {

        #region private fields

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
        /// Authentication token which identifies the user. 
        /// </summary>
        private string SID = String.Empty;

        /// <summary>
        /// The Google User ID of the user. 
        /// </summary>
        private string GoogleUserId = String.Empty; 

        #endregion 

          #region constructor

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

            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);

            this.rssParser = new RssParser(this);


            // initialize (later on loaded from feedlist):
            this.PodcastFolder = this.Configuration.DownloadedFilesDataPath;
            this.EnclosureFolder = this.Configuration.DownloadedFilesDataPath;

            if (this.EnclosureFolder != null)
            {
                this.enclosureDownloader = new BackgroundDownloadManager(this.Configuration, this);
                this.enclosureDownloader.DownloadCompleted += this.OnEnclosureDownloadComplete;
            }

            this.AsyncWebRequest = new AsyncWebRequest();
            this.AsyncWebRequest.OnAllRequestsComplete += this.OnAllRequestsComplete;       
        }
               

        #endregion 

        #region public methods

        #region feed list methods

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
            Dictionary<string, category> bootstrapCategories = new Dictionary<string, category>();

            foreach (NewsFeed f in feedlist.feed)
            {
                bootstrapFeeds.Add(f.link, f);
            }

            foreach (category c in feedlist.categories)
            {
                bootstrapCategories.Add(c.Value, c);
            }

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
                this.categories.Add(gLabel, new GoogleReaderCategory(label, (bootstrapCategories.TryGetValue(label, out cat) ? cat : null), this));
  
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
		/// Returns a CookieCollection containing a cookie with the specified SID
		/// </summary>
		/// <param name="sid">The user's SID.</param>
		/// <returns>
		/// A cookie collection with the Google cookie created from the SID
		/// </returns>
        private static CookieCollection MakeGoogleCookie(string sid)
        {
           Cookie cookie = new Cookie("SID", sid, "/", ".google.com");
           cookie.Expires = DateTime.Now + new TimeSpan(365,0,0,0); //set cookie to expire in 1 year
           CookieCollection collection = new CookieCollection(); 
           collection.Add(cookie); 

           return collection; 
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

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(MakeGoogleCookie(sid));

            StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream());          
            return reader.ReadToEnd();             
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
            if (this.FeedsListOK == false)
            {
                //we don't have a feed list
                return;
            }

            bool anyRequestQueued = false;

            try
            {
                RaiseOnUpdateFeedsStarted(force_download);

                string[] keys = GetFeedsTableKeys();

                //foreach(string sKey in FeedsTable.Keys){
                //  NewsFeed current = FeedsTable[sKey];	

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    if (!feedsTable.ContainsKey(keys[i])) // may have been redirected/removed meanwhile
                        continue;

                    INewsFeed current = feedsTable[keys[i]];

                    try
                    {
                        // new: giving up after ten unsuccessfull requests
                        if (!force_download && current.causedExceptionCount >= 10)
                        {
                            continue;
                        }

                        if (current.refreshrateSpecified && (current.refreshrate == 0))
                        {
                            continue;
                        }

                        if (itemsTable.ContainsKey(current.link))
                        {
                            //check if feed downloaded in the past

                            //check if enough time has elapsed as to require a download attempt
                            if ((!force_download) && current.lastretrievedSpecified)
                            {
                                double timeSinceLastDownload =
                                    DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
								//fix: now consider refreshrate inherited by categories:
								int refreshRate = this.GetRefreshRate(current.link);

                                if (!DownloadIntervalReached || (timeSinceLastDownload < refreshRate))
                                {
                                    continue; //no need to download 
                                }
                            } //if(current.lastretrievedSpecified...) 


                            if (this.AsyncGetItemsForFeed(current.link, true, false))
                                anyRequestQueued = true;
                        }
                        else
                        {
                            // not yet loaded, so not loaded from cache, new subscribed or imported
                            if ((!force_download) && current.lastretrievedSpecified && string.IsNullOrEmpty(current.cacheurl))
                            {
                                // imported may have lastretrievedSpecified set to reduce the initial payload
                                double timeSinceLastDownload =
                                    DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
								//fix: now consider refreshrate inherited by categories:
								int refreshRate = this.GetRefreshRate(current.link);

                                if (!DownloadIntervalReached || (timeSinceLastDownload < refreshRate))
                                {
                                    continue; //no need to download 
                                }
                            }

                            if (!force_download)
                            {
                                // not in itemsTable, cacheurl set - but no cache file anymore?
                                if (!string.IsNullOrEmpty(current.cacheurl) &&
                                    !this.CacheHandler.FeedExists(current))
                                    force_download = true;
                            }

                            if (this.AsyncGetItemsForFeed(current.link, force_download, false))
                                anyRequestQueued = true;
                        }

                        Thread.Sleep(15); // force a context switches
                    }
                    catch (Exception e)
                    {
                        Trace("RefreshFeeds(bool) unexpected error processing feed '{0}': {1}", keys[i], e.ToString());
                    }
                } //for(i)
            }
            catch (InvalidOperationException ioe)
            {
                // New feeds added to FeedsTable from another thread  

                Trace("RefreshFeeds(bool) InvalidOperationException: {0}", ioe.ToString());
            }
            finally
            {
                if (isOffline || !anyRequestQueued)
                    RaiseOnAllAsyncRequestsCompleted();
            }
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
            if (this.FeedsListOK == false)
            {
                //we don't have a feed list
                return;
            }

            bool anyRequestQueued = false;

            try
            {
                RaiseOnUpdateFeedsStarted(force_download);

                string[] keys = GetFeedsTableKeys();

                //foreach(string sKey in FeedsTable.Keys){
                //  NewsFeed current = FeedsTable[sKey];	

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    if (!feedsTable.ContainsKey(keys[i])) // may have been redirected/removed meanwhile
                        continue;

                    INewsFeed current = feedsTable[keys[i]];

                    try
                    {
                        // new: giving up after three unsuccessfull requests
                        if (!force_download && current.causedExceptionCount >= 3)
                        {
                            continue;
                        }

                        if (current.refreshrateSpecified && (current.refreshrate == 0))
                        {
                            continue;
                        }

                        if (itemsTable.ContainsKey(current.link))
                        {
                            //check if feed downloaded in the past

                            //check if enough time has elapsed as to require a download attempt
                            if ((!force_download) && current.lastretrievedSpecified)
                            {
                                double timeSinceLastDownload =
                                    DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
								//fix: now consider refreshrate inherited by categories:
								int refreshRate = this.GetRefreshRate(current.link);

                                if (!DownloadIntervalReached || (timeSinceLastDownload < refreshRate))
                                {
                                    continue; //no need to download 
                                }
                            } //if(current.lastretrievedSpecified...) 


                            if (current.category != null && IsChildOrSameCategory(category, current.category))
                            {
                                if (this.AsyncGetItemsForFeed(current.link, true, false))
                                    anyRequestQueued = true;
                            }
                        }
                        else
                        {
                            if (current.category != null && IsChildOrSameCategory(category, current.category))
                            {
                                if (this.AsyncGetItemsForFeed(current.link, force_download, false))
                                    anyRequestQueued = true;
                            }
                        }

                        Thread.Sleep(15); // force a context switches
                    }
                    catch (Exception e)
                    {
                        Trace("RefreshFeeds(string,bool) unexpected error processing feed '{0}': {1}", current.link,
                              e.ToString());
                    }
                } //for(i)
            }
            catch (InvalidOperationException ioe)
            {
                // New feeds added to FeedsTable from another thread  

                Trace("RefreshFeeds(string,bool) InvalidOperationException: {0}", ioe.ToString());
            }
            finally
            {
                if (isOffline || !anyRequestQueued)
                    RaiseOnAllAsyncRequestsCompleted();
            }
        }


        #endregion

        #region subscription editing methods 

        /// <summary>
        /// Changes the title of a subscribed feed in Google Reader
        /// </summary>
        /// <remarks>This method does nothing if the new title is empty or null</remarks>
        /// <param name="url">The feed URL</param>
        /// <param name="title">The new title</param>
        public void RenameFeedInGoogleReader(string url, string title)
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
                    string body = "ac=edit&i=null&T=" + GetGoogleEditToken(this.SID) + "&t=" + HttpUtility.HtmlEncode(title) + "&s=" + f.GoogleReaderFeedId;
                    HttpWebResponse response = AsyncWebRequest.PostSyncResponse(apiUrl, body, MakeGoogleCookie(this.SID), null, this.Proxy);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException(response.StatusDescription); 
                    }
                }

            }// if(!StringHelper.EmptyTrimOrNull(url) && feedsTable.ContainsKey(url)){

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

        public override string link
        {
            get
            {
                return mysubscription.FeedUrl;
            }
            set
            {
                /* cannot set this field */ 
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
                
                if (myowner != null && !StringHelper.EmptyTrimOrNull(value))
                {
                    myowner.RenameFeedInGoogleReader(this.link, value);
                    mysubscription.Title = value;
                }
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

    #region GoogleReaderCategory

    /// <summary>
    /// Holds the preferences for a label/category in Google Reader. 
    /// </summary>
    internal class GoogleReaderCategory : category
    {

        #region constructors 

        // <summary>
        /// A category must always have a name
        /// </summary>
        private GoogleReaderCategory() { ;} 

         /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="label">The name of the category</param>
        public GoogleReaderCategory(string label)
        {
            if (label == null) throw new ArgumentNullException("label");
            this.Value = label; 
        }
       
        
        /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="label">The name of the category</param>
        /// <param name="category">A category instance from which this object shall obtain the values for it's INewsFeedCategory properties</param>
        public GoogleReaderCategory(string label, INewsFeedCategory category, GoogleReaderFeedSource owner)
            : this(label)
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

            if (owner is GoogleReaderFeedSource)
            {
                this.owner = owner as GoogleReaderFeedSource;
            }
        }

        #endregion 

        #region private fields 

        /// <summary>
        /// The actual GoogleReaderLabel instance that this object is wrapping
        /// </summary>
        private GoogleReaderLabel mylabel = null;

        /// <summary>
        /// The GoogleReaderFeedSource instance used to make Web requests to manage this label
        /// </summary>
        private GoogleReaderFeedSource owner = null; 

        #endregion 
    }

    #endregion 
}
