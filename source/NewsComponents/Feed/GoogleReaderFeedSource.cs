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
using System.Xml;

using NewsComponents.Net;
using NewsComponents.Search;

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
            Dictionary<string, INewsFeedCategory> bootstrapCategories = new Dictionary<string, INewsFeedCategory>();

            foreach (NewsFeed f in feedlist.feed)
            {
                bootstrapFeeds.Add(f.link, f);
            }

            foreach (category c in feedlist.categories)
            {
                bootstrapCategories.Add(c.Value, c);
            }

            this.LoadFeedlistFromGoogleReader(); 
        }

        /// <summary>
        /// Loads the user's feed list from Google Reader. 
        /// </summary>
        private void LoadFeedlistFromGoogleReader()
        {
            string feedlistUrl = apiUrlPrefix + "subscription/list";

            //get the user's SID
            this.AuthenticateUser();
            
            //load feed list XML
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(AsyncWebRequest.GetSyncResponseStream(feedlistUrl, null, this.Proxy, MakeGoogleCookie(this.SID))));

            var feedlist = from XmlNode node in doc.SelectNodes("/object/list[@name='subscriptions']/object")
                           select MakeSubscription(node);
               
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
            XmlNode title_node = node.SelectSingleNode("string[name='title']");
            string title = (title_node == null ? String.Empty : title_node.InnerText);
            XmlNode fim_node = node.SelectSingleNode("string[@name='firstitemmsec']");
            long firstitemmsec = (id_node == null ? 0 : Int64.Parse(fim_node.InnerText));
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
        /// <param name="SID">a SID which identifies a Google user</param>
        /// <returns>A cookie collection with the Google cookie created from the SID</returns>
        private static CookieCollection MakeGoogleCookie(string sid)
        {
           Cookie cookie = new Cookie("SID", sid, "/", ".google.com");
           cookie.Expires = DateTime.Now + new TimeSpan(365,0,0,0); //set cookie to expire in 1 year
           CookieCollection collection = new CookieCollection(); 
           collection.Add(cookie); 

           return collection; 
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
}
