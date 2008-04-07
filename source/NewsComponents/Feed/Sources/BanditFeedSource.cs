#region Version Info Header
/*
 * $Id: BanditFeedSource.cs 463 2008-04-07 13:09:49Z t_rendelmann $
 * $HeadURL: https://rssbandit.svn.sourceforge.net/svnroot/rssbandit/trunk/source/NewsComponents/Feed/BanditFeedSource.cs $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2008-04-07 06:09:49 -0700 (Mon, 07 Apr 2008) $
 * $Revision: 463 $
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
    /// A FeedSource that directly accesses RSS/Atom feeds via HTTP or HTTPS 
    /// and newsgroups via NNTP. 
    /// </summary>
    class BanditFeedSource : FeedSource
    {

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSource"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="location">The feedlist URL</param>
        internal BanditFeedSource(INewsComponentsConfiguration configuration, SubscriptionLocation location)
        {
            this.p_configuration = configuration;
            if (this.p_configuration == null)
                this.p_configuration = FeedSource.DefaultConfiguration;

            this.location = location;

            // check for programmers error in configuration:
            ValidateAndThrow(this.Configuration);

            this.LoadFeedlistSchema();

            this.rssParser = new RssParser(this);


            // initialize (later on loaded from feedlist):
            this.PodcastFolder = this.Configuration.DownloadedFilesDataPath;

            if (!String.IsNullOrEmpty(EnclosureFolder))
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

        	// reset migration properties dictionary:
			FeedSource.MigrationProperties.Clear();

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

			/* 
			 * props. set by configuration/static, but required for migration:
			 */ 
                //if refresh rate in imported feed then use that
            if (myFeeds.refreshrateSpecified)
            {
				FeedSource.MigrationProperties.Add("RefreshRate", myFeeds.refreshrate);
            }
			
			//if stylesheet specified in imported feed then use that
            if (!string.IsNullOrEmpty(myFeeds.stylesheet))
            {
				FeedSource.MigrationProperties.Add("Stylesheet", myFeeds.stylesheet);
                //this.stylesheet = myFeeds.stylesheet;
            }
			
                //if download enclosures specified in imported feed then use that
            if (myFeeds.downloadenclosuresSpecified)
            {
				FeedSource.MigrationProperties.Add("DownloadEnclosures", myFeeds.downloadenclosures);
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
            	FeedSource.MigrationProperties.Add("EnclosureAlert", myFeeds.enclosurealert);
                //this.enclosurealert = myFeeds.enclosurealert;
            }

                //if create subfolders for enclosures specified in imported feed then use that
            if (myFeeds.createsubfoldersforenclosuresSpecified)
            {
                this.createsubfoldersforenclosures = myFeeds.createsubfoldersforenclosures;
            }

			
                //if marking items as read on exit specified in imported feed then use that
            if (myFeeds.markitemsreadonexitSpecified)
            {
				FeedSource.MigrationProperties.Add("MarkItemsReadOnExit", myFeeds.markitemsreadonexit);
                //this.markitemsreadonexit = myFeeds.markitemsreadonexit;
            }
			
			//if enclosure folder specified in imported feed then use that
            if (!string.IsNullOrEmpty(myFeeds.enclosurefolder))
            {
				FeedSource.MigrationProperties.Add("EnclosureFolder", myFeeds.enclosurefolder);
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
					FeedSource.MigrationProperties.Add("MaxItemAge", myFeeds.maxitemage);
                    //this.maxitemage = XmlConvert.ToTimeSpan(myFeeds.maxitemage);
                }
            }
            catch (FormatException fe)
            {
                Trace("Error occured while parsing maximum item age from feed list: {0}", fe.ToString());
            }
            
        }



        #endregion


        #region public methods

        #region feed and category management 
           /// <summary>
        /// Deletes all subscribed feeds and categories 
        /// </summary>
        public override void DeleteAllFeedsAndCategories()
        {
            base.DeleteAllFeedsAndCategories();
            this.ClearItemsCache();
        }

        #endregion 

        #region feed list methods

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
            this.LoadFeedlist(location.Location, FeedSource.ValidationCallbackOne);

        }

        /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this FeedSource</param>
        public override void BootstrapAndLoadFeedlist(feeds feedlist)
        {
            this.ImportFeedlist(feedlist, null, true, false);
        }

#endregion

        #region feed downloading methods

  
        #endregion 

        #endregion


    }

}
