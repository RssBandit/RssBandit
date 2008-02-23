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
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;


using NewsComponents.Net;
using NewsComponents.News;
// required to make it compile with the Uri extension methods.
// DO NOT REMOVE IT, also if Resharper complains about (not used!?)...
using RssBandit.Common;

namespace NewsComponents.Feed
{

    

    /// <summary>
    /// A NewsHandler that directly accesses RSS/Atom feeds via HTTP or HTTPS 
    /// and newsgroups via NNTP. 
    /// </summary>
    class BanditNewsHandler : NewsHandler
    {

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="location">The feedlist URL</param>
        internal BanditNewsHandler(INewsComponentsConfiguration configuration, SubscriptionLocation location)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = NewsHandler.DefaultConfiguration;

            this.location = location;

            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);

            this.LoadFeedlistSchema();

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


        #region private fields

        /// <summary>
        /// Collection contains NntpServerDefinition objects.
        /// Keys are the account name(s) - friendly names for the news server def.:
        /// NntpServerDefinition.Name's
        /// </summary>
        private IDictionary<string, INntpServerDefinition> nntpServers = new Dictionary<string, INntpServerDefinition>();

        /// <summary>
        /// Collection contains UserIdentity objects.
        /// Keys are the UserIdentity.Name's
        /// </summary>
        private IDictionary<string, UserIdentity> identities = new Dictionary<string, UserIdentity>();


        /// <summary>
        /// The schema for the RSS feed list format
        /// </summary>
        private XmlSchema feedsSchema = null;
       

        #endregion


        #region private methods

        ///<summary>Loads the schema for a feedlist into an XmlSchema object. 
        ///<seealso cref="feedsSchema"/></summary>		
        private void LoadFeedlistSchema()
        {
            using (Stream xsdStream = Resource.Manager.GetStream("Resources.feedListSchema.xsd"))
            {
                feedsSchema = XmlSchema.Read(xsdStream, null);
            }
        }


        /// <summary>
        /// Loads the RSS feedlist from the given URL and validates it against the schema. 
        /// </summary>
        /// <param name="feedListUrl">The URL of the feedlist</param>
        /// <param name="veh">The event handler that should be invoked on the client if validation errors occur</param>
        /// <exception cref="XmlException">XmlException thrown if XML is not well-formed</exception>
        private void LoadFeedlist(string feedListUrl, ValidationEventHandler veh)
        {
            LoadFeedlist(AsyncWebRequest.GetSyncResponseStream(feedListUrl, null, this.UserAgent, this.Proxy), veh);
            SearchHandler.CheckIndex();
        }

        /// <summary>
        /// Loads the RSS feedlist from the given URL and validates it against the schema. 
        /// </summary>
        /// <param name="xmlStream">The XML Stream of a feedlist to load</param>
        /// <param name="veh">The event handler that should be invoked on the client if validation errors occur. NOT USED.</param>
        /// <exception cref="XmlException">XmlException thrown if XML is not well-formed</exception>
        private void LoadFeedlist(Stream xmlStream, ValidationEventHandler veh)
        {
            XmlParserContext context =
                new XmlParserContext(null, new RssBanditXmlNamespaceResolver(), null, XmlSpace.None);
            XmlReader reader = new RssBanditXmlReader(xmlStream, XmlNodeType.Document, context);
            validationErrorOccured = false;

            //convert XML to objects
            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(feeds));
            feeds myFeeds = (feeds)serializer.Deserialize(reader);
            reader.Close();
           

            //copy over category info if we are importing a new feed
            if (myFeeds.categories != null)
            {
                foreach (category cat in myFeeds.categories)
                {
                    string cat_trimmed = cat.Value.Trim();
                    if (!this.categories.ContainsKey(cat_trimmed))
                    {
                        cat.Value = cat_trimmed;
                        this.categories.Add(cat_trimmed, cat);
                    }
                }
            }

            //copy feeds over if we are importing a new feed  
            if (myFeeds.feed != null)
            {
                foreach (NewsFeed f in myFeeds.feed)
                {
                    if (feedsTable.ContainsKey(f.link) == false)
                    {
                        bool isBadUri = false;
                        try
                        {
                            Uri uri = new Uri(f.link);
                            // CLR 2.0 Uri does not like "news:" scheme, so we 
                            // switch it to "nntp:" (see http://msdn2.microsoft.com/en-us/library/system.uri.scheme.aspx)
                            if (NntpWebRequest.NewsUriScheme.Equals(uri.Scheme))
                            {
                                f.link = NntpWebRequest.NntpUriScheme + uri.CanonicalizedUri().Substring(uri.Scheme.Length);
                            }
                            else
                            {
                                f.link = uri.CanonicalizedUri();
                            }
                        }
                        catch (Exception)
                        {
                            isBadUri = true;
                        }

                        if (isBadUri)
                        {
                            continue;
                        }
                        else
                        {
                            // test again: we may have changed to Uri above:
                            if (feedsTable.ContainsKey(f.link) == false)
                            {
                                f.owner = this;
                                this.feedsTable.Add(f.link, f);

                                //add category if needed
                                if (f.category != null)
                                {
                                    string cat_trimmed = f.category = f.category.Trim();

                                    if (!this.categories.ContainsKey(cat_trimmed))
                                    {
                                        this.AddCategory(cat_trimmed);
                                    }
                                }
                            }
                        }
                    }
                }
            }

           
            //copy over layout info if we are importing a new feed
            if (myFeeds.listviewLayouts != null)
            {
                foreach (listviewLayout layout in myFeeds.listviewLayouts)
                {
                    string layout_trimmed = layout.ID.Trim();
                    if (!this.layouts.ContainsKey(layout_trimmed))
                    {
                        this.layouts.Add(layout_trimmed, layout.FeedColumnLayout);
                    }
                }
            }

                //copy nntp-server defs. over if we are importing  
            if (myFeeds.nntpservers != null)
            {
                foreach (NntpServerDefinition sd in myFeeds.nntpservers)
                {
                    if (nntpServers.ContainsKey(sd.Name) == false)
                    {
                        nntpServers.Add(sd.Name, sd);
                    }
                }
            }

                //copy user-identities over if we are importing  
            if (myFeeds.identities != null)
            {
                foreach (UserIdentity ui in myFeeds.identities)
                {
                    if (identities.ContainsKey(ui.Name) == false)
                    {
                        identities.Add(ui.Name, ui);
                    }
                }
            }

                //if refresh rate in imported feed then use that
            if (myFeeds.refreshrateSpecified)
            {
                this.refreshrate = myFeeds.refreshrate;
            }

                //if stylesheet specified in imported feed then use that
            if (!string.IsNullOrEmpty(myFeeds.stylesheet))
            {
                this.stylesheet = myFeeds.stylesheet;
            }

                //if download enclosures specified in imported feed then use that
            if (myFeeds.downloadenclosuresSpecified)
            {
                this.downloadenclosures = myFeeds.downloadenclosures;
            }

                //if maximum enclosure cache size specified in imported feed then use that
            if (myFeeds.enclosurecachesizeSpecified)
            {
                this.enclosurecachesize = myFeeds.enclosurecachesize;
            }

                //if maximum number of enclosures to download on a new feed specified in imported feed then use that
            if (myFeeds.numtodownloadonnewfeedSpecified)
            {
                this.numtodownloadonnewfeed = myFeeds.numtodownloadonnewfeed;
            }

                //if cause alert on enclosures specified in imported feed then use that
            if (myFeeds.enclosurealertSpecified)
            {
                this.enclosurealert = myFeeds.enclosurealert;
            }

                //if create subfolders for enclosures specified in imported feed then use that
            if (myFeeds.createsubfoldersforenclosuresSpecified)
            {
                this.createsubfoldersforenclosures = myFeeds.createsubfoldersforenclosures;
            }


                //if marking items as read on exit specified in imported feed then use that
            if (myFeeds.markitemsreadonexitSpecified)
            {
                this.markitemsreadonexit = myFeeds.markitemsreadonexit;
            }

                //if enclosure folder specified in imported feed then use that
            if (!string.IsNullOrEmpty(myFeeds.enclosurefolder))
            {
                this.EnclosureFolder = myFeeds.enclosurefolder;
            }

                //if podcast folder specified in imported feed then use that
            if (!string.IsNullOrEmpty(myFeeds.podcastfolder))
            {
                this.PodcastFolder = myFeeds.podcastfolder;
            }

                //if podcast file extensions specified in imported feed then use that
            if (!string.IsNullOrEmpty(myFeeds.podcastfileexts))
            {
                this.PodcastFileExtensionsAsString = myFeeds.podcastfileexts;
            }


                //if listview layout specified in imported feed then use that
            if (!string.IsNullOrEmpty(myFeeds.listviewlayout))
            {
                this.listviewlayout = myFeeds.listviewlayout;
            }

                //if max item age in imported feed then use that
            try
            {
                if (!string.IsNullOrEmpty(myFeeds.maxitemage))
                {
                    this.maxitemage = XmlConvert.ToTimeSpan(myFeeds.maxitemage);
                }
            }
            catch (FormatException fe)
            {
                Trace("Error occured while parsing maximum item age from feed list: {0}", fe.ToString());
            }
            
        }



        #endregion


        #region public methods


           /// <summary>
        /// Deletes all subscribed feeds and categories 
        /// </summary>
        public override void DeleteAllFeedsAndCategories()
        {
            base.DeleteAllFeedsAndCategories();
            this.ClearItemsCache();
        }

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
            this.LoadFeedlist(location.Location, NewsHandler.ValidationCallbackOne);

        }

        /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this NewsHandler</param>
        public override void BootstrapAndLoadFeedlist(feeds feedlist)
        {
            this.ImportFeedlist(feedlist, null, true, false); 
        }

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
                                int refreshRate = current.refreshrateSpecified ? current.refreshrate : this.RefreshRate;

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
                            if (current.lastretrievedSpecified && string.IsNullOrEmpty(current.cacheurl))
                            {
                                // imported may have lastretrievedSpecified set to reduce the initial payload
                                double timeSinceLastDownload =
                                    DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
                                int refreshRate = current.refreshrateSpecified ? current.refreshrate : this.RefreshRate;

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
                if (offline || !anyRequestQueued)
                    RaiseOnAllAsyncRequestsCompleted();
            }
        }

        #endregion


    }

}
