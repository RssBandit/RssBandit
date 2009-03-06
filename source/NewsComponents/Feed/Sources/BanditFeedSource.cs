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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using log4net;
using NewsComponents.Net;
using NewsComponents.News;
using NewsComponents.Storage;
using NewsComponents.Utils;
using RssBandit.Common;
using RssBandit.Common.Logging;

// required to make it compile with the Uri extension methods.
// DO NOT REMOVE IT, also if Resharper complains about (not used!?)...

namespace NewsComponents.Feed
{
	#region IBanditFeedSource interface: public FeedSource extensions
	/// <summary>
	/// public FeedSource extension offered by Bandit Feed Source
	/// </summary>
	public interface IBanditFeedSource
	{
		#region NNTP Support

		/// <summary>
		/// Gets the NNTP server definitions.
		/// </summary>
		/// <value>The NNTP servers.</value>
		IDictionary<string, INntpServerDefinition> NntpServers { get; }

		/// <summary>
		/// Saves the NNTP server definitions.
		/// </summary>
		void SaveNntpServers();

		/// <summary>
		/// Gets the feed credentials for a INntpServerDefinition.
		/// </summary>
		/// <param name="sd">The sd.</param>
		/// <returns></returns>
		ICredentials GetFeedCredentials(INntpServerDefinition sd);

		#endregion
	}

	#endregion

	/// <summary>
    /// A FeedSource that directly accesses RSS/Atom feeds via HTTP or HTTPS 
    /// and newsgroups via NNTP. 
    /// </summary>
	internal class BanditFeedSource : FeedSource, IBanditFeedSource
    {
        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSource"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="location">The feedlist URL</param>
        internal BanditFeedSource(INewsComponentsConfiguration configuration, SubscriptionLocation location)
        {
            p_configuration = configuration;
            if (p_configuration == null)
                p_configuration = DefaultConfiguration;

            this.location = location;

            // check for programmers error in configuration:
            ValidateAndThrow(Configuration);

            LoadFeedlistSchema();

            rssParser = new RssParser(this);

            if (!String.IsNullOrEmpty(EnclosureFolder))
            {
                enclosureDownloader = new BackgroundDownloadManager(this);
                enclosureDownloader.DownloadCompleted += OnEnclosureDownloadComplete;
            }

            AsyncWebRequest = new AsyncWebRequest();
            AsyncWebRequest.OnAllRequestsComplete += OnAllRequestsComplete;
        }

        #endregion

        #region private fields

		private static readonly ILog _log = Log.GetLogger(typeof(BanditFeedSource));

        /// <summary>
        /// Collection contains NntpServerDefinition objects.
        /// Keys are the account name(s) - friendly names for the news server def.:
        /// NntpServerDefinition.Name's
        /// </summary>
        /// <remarks>We use null here: this flag the data as NOT yet loaded</remarks>
        private IDictionary<string, INntpServerDefinition> nntpServers;

        /// <summary>
        /// Collection contains UserIdentity objects.
        /// Keys are the UserIdentity.Name's
        /// </summary>
        private readonly IDictionary<string, UserIdentity> identities = new Dictionary<string, UserIdentity>();


        /// <summary>
        /// The schema for the RSS feed list format
        /// </summary>
        private XmlSchema feedsSchema;

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
            LoadFeedlist(AsyncWebRequest.GetSyncResponseStream(feedListUrl, null, UserAgent, Proxy), veh);
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
            var context =
                new XmlParserContext(null, new RssBanditXmlNamespaceResolver(), null, XmlSpace.None);
            XmlReader reader = new RssBanditXmlReader(xmlStream, XmlNodeType.Document, context);
            validationErrorOccured = false;

            //convert XML to objects
			XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(feeds));
		    var myFeeds = (feeds) serializer.Deserialize(reader);
            reader.Close();

            // reset migration properties dictionary:
            MigrationProperties.Clear();

            //copy over category info if we are importing a new feed
            if (myFeeds.categories != null)
            {
                foreach (var cat in myFeeds.categories)
                {
                    string cat_trimmed = cat.Value.Trim();
                    if (!categories.ContainsKey(cat_trimmed))
                    {
                        cat.Value = cat_trimmed;
                        categories.Add(cat_trimmed, cat);
                    }
                }
            }

            //copy feeds over if we are importing a new feed  
            if (myFeeds.feed != null)
            {
                foreach (var f in myFeeds.feed)
                {
                    if (feedsTable.ContainsKey(f.link) == false)
                    {
                        Uri uri;
						if (Uri.TryCreate(f.link, UriKind.Absolute, out uri))
                        {
                            // CLR 2.0 Uri does not like "news:" scheme, so we 
                            // switch it to "nntp:" (see http://msdn2.microsoft.com/en-us/library/system.uri.scheme.aspx)
                            if (NntpWebRequest.NewsUriScheme.Equals(uri.Scheme))
                            {
                                f.link = NntpWebRequest.NntpUriScheme +
                                         uri.CanonicalizedUri().Substring(uri.Scheme.Length);
                            }
                            else
                            {
                                f.link = uri.CanonicalizedUri();
                            }
                        }
                        else
                        {
							// bad uri:
                        	continue;
						}

						// test again: we may have changed to Uri above:
						if (feedsTable.ContainsKey(f.link) == false)
						{
							f.owner = this;
							feedsTable.Add(f.link, f);

							//add category if needed
							if (f.category != null)
							{
								string cat_trimmed = f.category = f.category.Trim();

								if (!categories.ContainsKey(cat_trimmed))
								{
									AddCategory(cat_trimmed);
								}
							}
						}
					}
                }
            }


			////copy over layout info if we are importing a new feed
			//if (myFeeds.listviewLayouts != null)
			//{
			//    foreach (var layout in myFeeds.listviewLayouts)
			//    {
			//        string layout_trimmed = layout.ID.Trim();
			//        if (!layouts.ContainsKey(layout_trimmed))
			//        {
			//            layouts.Add(layout_trimmed, layout.FeedColumnLayout);
			//        }
			//    }
			//}

            

            /* 
			 * props. set by configuration/static, but required for migration:
			 */

            if (MigrateProperties)
			{
				//copy user-identities over if we are migrating  
				if (myFeeds.identities != null)
				{
					MigrationProperties.Add("UserIdentity", myFeeds.identities);
				}

				//copy nntp-server defs. over if we are migrating  
				if (myFeeds.nntpservers != null)
				{
					foreach (var sd in myFeeds.nntpservers)
					{
						// using the public property will initiate a load:
						if (NntpServers.ContainsKey(sd.Name) == false)
						{
							NntpServers.Add(sd.Name, sd);
						}
					}
				}

                //if refresh rate in imported feed then use that
                if (myFeeds.refreshrateSpecified)
                {
                    MigrationProperties.Add("RefreshRate", myFeeds.refreshrate);
                }

                //if stylesheet specified in imported feed then use that
                if (!string.IsNullOrEmpty(myFeeds.stylesheet))
                {
                    MigrationProperties.Add("Stylesheet", myFeeds.stylesheet);
                    //this.stylesheet = myFeeds.stylesheet;
                }

                //if download enclosures specified in imported feed then use that
                if (myFeeds.downloadenclosuresSpecified)
                {
                    MigrationProperties.Add("DownloadEnclosures", myFeeds.downloadenclosures);
                }

                //if maximum enclosure cache size specified in imported feed then use that
                if (myFeeds.enclosurecachesizeSpecified)
                {
                    MigrationProperties.Add("EnclosureCacheSize", myFeeds.enclosurecachesize);
                    //this.enclosurecachesize = myFeeds.enclosurecachesize;
                }

                //if maximum number of enclosures to download on a new feed specified in imported feed then use that
                if (myFeeds.numtodownloadonnewfeedSpecified)
                {
                    MigrationProperties.Add("NumEnclosuresToDownloadOnNewFeed", myFeeds.numtodownloadonnewfeed);
                    //this.numtodownloadonnewfeed = myFeeds.numtodownloadonnewfeed;
                }

                //if cause alert on enclosures specified in imported feed then use that
                if (myFeeds.enclosurealertSpecified)
                {
                    MigrationProperties.Add("EnclosureAlert", myFeeds.enclosurealert);
                    //this.enclosurealert = myFeeds.enclosurealert;
                }

                //if create subfolders for enclosures specified in imported feed then use that
                if (myFeeds.createsubfoldersforenclosuresSpecified)
                {
                    MigrationProperties.Add("CreateSubfoldersForEnclosures", myFeeds.createsubfoldersforenclosures);
                    //this.createsubfoldersforenclosures = myFeeds.createsubfoldersforenclosures;
                }


                //if marking items as read on exit specified in imported feed then use that
                if (myFeeds.markitemsreadonexitSpecified)
                {
                    MigrationProperties.Add("MarkItemsReadOnExit", myFeeds.markitemsreadonexit);
                    //this.markitemsreadonexit = myFeeds.markitemsreadonexit;
                }

                //if enclosure folder specified in imported feed then use that
                if (!string.IsNullOrEmpty(myFeeds.enclosurefolder))
                {
                    MigrationProperties.Add("EnclosureFolder", myFeeds.enclosurefolder);
                }

                //if podcast folder specified in imported feed then use that
                if (!string.IsNullOrEmpty(myFeeds.podcastfolder))
                {
					MigrationProperties.Add("PodcastFolder", myFeeds.podcastfolder);
                }

                //if podcast file extensions specified in imported feed then use that
                if (!string.IsNullOrEmpty(myFeeds.podcastfileexts))
                {
					MigrationProperties.Add("PodcastFileExtensions", myFeeds.podcastfileexts);
                }

				////if listview layout specified in imported feed then use that
				//if (!string.IsNullOrEmpty(myFeeds.listviewlayout))
				//{
				//    FeedColumnLayout = myFeeds.listviewlayout;
				//}

                //if max item age in imported feed then use that
                try
                {
                    if (!string.IsNullOrEmpty(myFeeds.maxitemage))
                    {
                        MigrationProperties.Add("MaxItemAge", myFeeds.maxitemage);
                        //this.maxitemage = XmlConvert.ToTimeSpan(myFeeds.maxitemage);
                    }
                }
                catch (FormatException fe)
                {
                    Trace("Error occured while parsing maximum item age from feed list: {0}", fe.ToDescriptiveString());
                }
            } //if(FeedSource.MigrateProperties){
        }

        #endregion

        #region public methods

        #region feed and category management        

        #endregion

        #region feed list methods

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public override void LoadFeedlist()
        {
            LoadFeedlist(location.Location, ValidationCallbackOne);
        }

        /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this FeedSource</param>
        public override void BootstrapAndLoadFeedlist(feeds feedlist)
        {
            ImportFeedlist(feedlist, null, true, false);
        }

        #endregion

        #region feed downloading methods

        #endregion

		/// <summary>
		/// Accesses the list of NntpServerDefinition objects 
		/// Keys are the account name(s) - friendly names for the news server def.:
		/// NewsServerDefinition.Name's
		/// </summary>
		public IDictionary<string, INntpServerDefinition> NntpServers
		{
			[DebuggerStepThrough]
			get
			{
				if (nntpServers == null)
				{
					try
					{
						nntpServers = LoadNntpServers();
					} 
					catch (Exception ex)
					{
						_log.Error("Failed to load NNTP server definitions", ex);
					}
				}

				return nntpServers;
			}
		}

		public void SaveNntpServers()
		{
			if (nntpServers == null)
				return;
			List<NntpServerDefinition> list = new List<NntpServerDefinition>(nntpServers.Count);
			foreach (NntpServerDefinition sd in nntpServers.Values)
			{
				list.Add(sd);
			}
			try
			{
				this.UserDataService.SaveNntpServerDefinitions(list);
			}
			catch (Exception ex)
			{
				_log.Error("Failed to save NNTP server definitions", ex);
			}
		}

		private Dictionary<string, INntpServerDefinition> LoadNntpServers()
		{
			Dictionary<string, INntpServerDefinition> loaded = new Dictionary<string, INntpServerDefinition>();
			List<NntpServerDefinition> list = this.UserDataService.LoadNntpServerDefinitions();
			if (list == null || list.Count == 0)
				return loaded;
			foreach (NntpServerDefinition sd in list)
			{
				loaded.Add(sd.Name, sd);
			}
			return loaded;
		}

		/// <summary>
		/// Gets the data service files used by each data service.
		/// </summary>
		/// <returns></returns>
		public override string[] GetDataServiceFiles()
		{
			// currently only the IUserDataService has relevant files:
			IUserDataService service = UserDataService;
			return service.GetUserDataFileNames();
		}

		protected override void ReplaceDataWithContent(string dataFileName, Stream content)
		{
			// my IUserDataService has relevant files, reset ivar(s):
			if (DataEntityName.NntpServerDefinitions == UserDataService.SetContentForDataFile(dataFileName, content))
				nntpServers = null;
		}

		#region NntpServerDefinition Credentials handling

		/// <summary>
		/// Return ICredentials of a feed. 
		/// </summary>
		/// <param name="sd">NntpServerDefinition</param>
		/// <returns>null in the case the nntp server does not have credentials</returns>
		public ICredentials GetFeedCredentials(INntpServerDefinition sd)
		{
			ICredentials c = null;
			if (sd.AuthUser != null)
			{
				string u, p;
				GetNntpServerCredentials(sd, out u, out p);
				c = CreateCredentialsFrom(u, p);
			}
			return c;
		}

		/// <summary>
		/// Set the authorization credentials for a Nntp Server.
		/// </summary>
		/// <param name="sd">NntpServerDefinition to be modified</param>
		/// <param name="user">username, identifier</param>
		/// <param name="pwd">password</param>
		public static void SetNntpServerCredentials(NntpServerDefinition sd, string user, string pwd)
		{
			if (sd == null) return;
			sd.AuthPassword = CryptHelper.EncryptB(pwd);
			sd.AuthUser = user;
		}

		/// <summary>
		/// Get the authorization credentials for a feed.
		/// </summary>
		/// <param name="sd">NntpServerDefinition, where the credentials are taken from</param>
		/// <param name="user">String return parameter containing the username</param>
		/// <param name="pwd">String return parameter, containing the password</param>
		public static void GetNntpServerCredentials(NntpServerDefinition sd, out string user, out string pwd)
		{
			pwd = user = null;
			if (sd == null) return;
			pwd = (sd.AuthPassword != null ? CryptHelper.Decrypt(sd.AuthPassword) : null);
			user = sd.AuthUser;
		}
		#endregion

		#endregion
	}
}