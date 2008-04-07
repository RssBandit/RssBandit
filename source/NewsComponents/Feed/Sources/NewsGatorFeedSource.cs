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
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using NewsComponents.Net;
using NewsComponents.Search;

namespace NewsComponents.Feed {
    /// <summary>
    /// A FeedSource that retrieves user subscriptions and feeds from NewsGator Online. 
    /// </summary>
    class NewsGatorFeedSource : FeedSource
    {

        #region private fields


        /// <summary>
		/// The Newsgator Online sync token.
		/// </summary>
        private string NgosSyncToken; 
     
        /// <summary>
        /// The NewsGator API key for RSS Bandit
        /// </summary>
        private static readonly string NgosProductKey = "7AF62582A5334A9CADF967818E734558";

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

        #endregion 

        #region constructor

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
            /* GoogleReaderUpdater.RegisterFeedSource(this); */ 

           
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

        #endregion 

        #region public methods

        #region feed list methods

        /// <summary>
        /// Loads the user's feed list from NewsGator Online. In addition, sets the current NgosSyncToken property 
        /// </summary>
        /// <returns>The feed list from NewsGator Online</returns>
        private feeds LoadFeedlistFromNewsGatorOnline()
        {
            /*
            string feedlistUrl = apiUrlPrefix + "subscription/list";

            //get the user's SID
            this.AuthenticateUser();

            //load feed list XML
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(AsyncWebRequest.GetSyncResponseStream(feedlistUrl, null, this.Proxy, MakeGoogleCookie(this.SID))));

            var feedlist = from XmlNode node in doc.SelectNodes("/object/list[@name='subscriptions']/object")
                           select MakeSubscription(node);

            return feedlist;
             */
            return null; 
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
        }

        /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this FeedSource</param>
        public override void BootstrapAndLoadFeedlist(feeds feedlist)
        {

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
}

