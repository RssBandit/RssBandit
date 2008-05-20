#region Version Info Header

/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

#region framework usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using log4net;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.News;
using NewsComponents.RelationCosmos;
using NewsComponents.Resources;
using NewsComponents.Search;
using NewsComponents.Storage;
using NewsComponents.Threading;
using NewsComponents.Utils;
using RssBandit.AppServices.Core;
using RssBandit.Common;
using RssBandit.Common.Logging;

#endregion

#region project usings

#endregion

namespace NewsComponents
{
    /// <summary>
    /// Supported Feedlist Formats (import/export).
    /// </summary>
    public enum FeedListFormat
    {
        /// <summary>
        /// Open Content Syndication. See http://internetalchemy.org/ocs/
        /// </summary>
        OCS,
        /// <summary>
        /// Outline Processor Markup Language, see http://opml.scripting.com/spec
        /// </summary>
        OPML,
        /// <summary>
        /// Native FeedSource format
        /// </summary>
        NewsHandler,
        /// <summary>
        /// Native reduced/light FeedSource format
        /// </summary>
        NewsHandlerLite,
    }

    /// <summary>
    /// Enumeration that describes the source of the feeds that are being processed
    /// by a particular FeedSource
    /// </summary>
    public enum FeedSourceType
    {
		/// <summary>
		/// The default for unititialized instances: not a known source
		/// </summary>
		Unknown = 0, 
        /// <summary>
        /// The feeds are sourced from Google Reader.
        /// </summary>
        Google,

        /// <summary>
        /// The feeds are sourced from NewsGator Online.
        /// </summary>
        NewsGator,

        /// <summary>
        /// The feeds are sourced from the Windows RSS platform.
        /// </summary>
        WindowsRSS,

        /// <summary>
        /// The feeds are directly accessed by RSS Bandit.
        /// </summary>
        DirectAccess
    }


    /// <summary>
    /// Provides the location of a subscription location.
    /// </summary>
    public class SubscriptionLocation
    {
        /// <summary>
        /// Initializes the subscription location
        /// </summary>
        /// <param name="location">The path or identifier to the list of subscriptions</param>
        /// <param name="credentials">The credentials required to access the location if any</param>
        public SubscriptionLocation(string location, NetworkCredential credentials)
        {
            Location = location;
            Credentials = credentials;
        }

        /// <summary>
        /// Initializes the subscription location
        /// </summary>
        /// <param name="location">The path or identifier to the list of subscriptions</param>        
        public SubscriptionLocation(string location)
        {
            Location = location;
            Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        /// <summary>
        /// The path or identifier to the list of subscriptions
        /// </summary>
        public string Location { get; set; }


        /// <summary>
        /// The credentials required to access the location if any
        /// </summary>
        public NetworkCredential Credentials { get; set; }
    }

    /// <summary>
    /// Class for managing News feeds. This class is NOT thread-safe.
    /// </summary>
    //TODO: just there to make it compile while refactoring. MUST BE REMOVED ON RELEASE
#if DEBUG
    [CLSCompliant(false)]
#endif
    public abstract class FeedSource : ISharedProperty //, IDisposable 
    {
        #region ctor's

        /// <summary>
        /// Initialize the userAgent template
        /// </summary>
        static FeedSource()
        {
            var sb = new StringBuilder(200);
            sb.Append("{0}"); // userAgent filled in later
            sb.Append(" (.NET CLR ");
            sb.Append(Environment.Version);
            sb.Append("; ");
            sb.Append(Environment.OSVersion.ToString().Replace("Microsoft Windows ", "Win"));
            sb.Append("; http://www.rssbandit.org");
            sb.Append(")");

            userAgentTemplate = sb.ToString();
            // TODO: REMOVE
            //LoadCachedTopStoryTitles();
            EnclosureFolder = String.Empty;
            NumEnclosuresToDownloadOnNewFeed = DefaultNumEnclosuresToDownloadOnNewFeed;
        }

        protected FeedSource()
        {
            MaxItemAge = new TimeSpan(90, 0, 0, 0);
        }


        /// <summary>
        /// Creates the appropriate FeedSource subtype based on the supplied FeedSourceType using
        /// the default configuration
        /// </summary>
        /// <seealso cref="DefaultConfiguration"/>
        /// <param name="handlerType">The type of FeedSource to create</param>
        /// <param name="location">The location of the subscriptions</param>
        /// <returns>A new FeedSource</returns>
        public static FeedSource CreateFeedSource(FeedSourceType handlerType, SubscriptionLocation location)
        {
            return CreateFeedSource(handlerType, location, DefaultConfiguration);
        }

        /// <summary>
        /// Creates the appropriate FeedSource subtype based on the supplied FeedSourceType
        /// </summary>
        /// <param name="handlerType">The type of FeedSource to create</param>
        /// <param name="location">The location of the subscriptions</param>
        /// <param name="configuration"></param>
        /// <returns>A new FeedSource</returns>
        public static FeedSource CreateFeedSource(FeedSourceType handlerType, SubscriptionLocation location,
                                                  INewsComponentsConfiguration configuration)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (String.IsNullOrEmpty(location.Location))
				throw new ArgumentException("Parameter property location.Location cannot be null", "location");

            FeedSource handler = null;

            switch (handlerType)
            {
                case FeedSourceType.DirectAccess:
                    handler = new BanditFeedSource(configuration, location);
                    break;
                case FeedSourceType.WindowsRSS:
                    handler = new WindowsRssFeedSource(configuration);
                    break;
                case FeedSourceType.Google:
                    handler = new GoogleReaderFeedSource(configuration, location);
                    break;
                case FeedSourceType.NewsGator:
                    handler = new NewsGatorFeedSource(configuration, location);
                    break;
                default:
                    break;
            }

			//Add the FeedSource to the list of NewsHandlers known by the SearchHandler
			if (handler != null)
			{
				handler.sourceType = handlerType;

				if (handler.Configuration.SearchIndexBehavior != SearchIndexBehavior.NoIndexing &&
					handler.Configuration.SearchIndexBehavior == DefaultConfiguration.SearchIndexBehavior)
				{
					SearchHandler.AddNewsHandler(handler);
				}
			}

            return handler;
        }

        #endregion

        #region static properties 

        private static INewsComponentsConfiguration defaultConfiguration;

        /// <summary>
        /// Gets or sets the default NewsComponents configuration.
        /// </summary>
        /// <value>The default configuration.</value>
        public static INewsComponentsConfiguration DefaultConfiguration
        {
            get { return defaultConfiguration ?? NewsComponentsConfiguration.Default; }
            set { defaultConfiguration = value; }
        }

        #endregion

        /// <summary>
        /// Defines all cache relevant NewsFeed properties, 
        /// that requires we have to (re-)write the cached file. 
        /// </summary>
        private const NewsFeedProperty cacheRelevantPropertyChanges =
            NewsFeedProperty.FeedItemFlag |
            NewsFeedProperty.FeedItemReadState |
            NewsFeedProperty.FeedItemCommentCount |
            NewsFeedProperty.FeedItemNewCommentsRead |
            NewsFeedProperty.FeedItemWatchComments |
            NewsFeedProperty.FeedCredentials;

        /// <summary>
        /// Indicates the default maximum amount of space that enclosures and 
        /// podcasts can use on disk. Currently this is Int32.MaxValue
        /// </summary>
        public const int DefaultEnclosureCacheSize = Int32.MaxValue;

        /// <summary>
        /// Indicates the default number of enclosures which should be downloaded 
        /// automatically from a newly subscribed feed.
        /// Currently this is Int32.MaxValue.
        /// </summary>
        public const int DefaultNumEnclosuresToDownloadOnNewFeed = Int32.MaxValue;

        private const int maxItemsPerSearchResult = 10;

        /// <summary>
        /// Defines all subscription relevant NewsFeed properties, 
        /// that requires we have to (re-)write the subscription file. 
        /// </summary>
        private const NewsFeedProperty subscriptionRelevantPropertyChanges =
            NewsFeedProperty.FeedLink |
            NewsFeedProperty.FeedTitle |
            NewsFeedProperty.FeedCategory |
            NewsFeedProperty.FeedItemsDeleteUndelete |
            NewsFeedProperty.FeedItemReadState |
            NewsFeedProperty.FeedMaxItemAge |
            NewsFeedProperty.FeedRefreshRate |
            NewsFeedProperty.FeedCacheUrl |
            NewsFeedProperty.FeedAdded |
            NewsFeedProperty.FeedRemoved |
            NewsFeedProperty.FeedCategoryAdded |
            NewsFeedProperty.FeedCategoryRemoved |
            NewsFeedProperty.FeedAlertOnNewItemsReceived |
            NewsFeedProperty.FeedMarkItemsReadOnExit |
            NewsFeedProperty.General;

        private static readonly ILog _log = Log.GetLogger(typeof (FeedSource));

        /// <summary>
        /// Indicates when the application first started
        /// </summary>
        private static readonly DateTime ApplicationStartTime = DateTime.Now;

        private static readonly byte[] bmp_magic = new byte[] {0x42, 0x4d};
        private static readonly int bmp_magic_len = bmp_magic.Length;

        /// <summary>
        /// Gets a empty item list.
        /// </summary>
        public static readonly List<INewsItem> EmptyItemList = new List<INewsItem>(0);

        private static readonly byte[] gif_magic = new byte[] {0x47, 0x49, 0x46};
        private static readonly int gif_magic_len = gif_magic.Length;
        private static readonly byte[] ico_magic = new byte[] {0, 0, 1, 0};
        private static readonly int ico_magic_len = ico_magic.Length;
        private static readonly byte[] jpg_magic = new byte[] {0xff, 0xd8};
        private static readonly int jpg_magic_len = jpg_magic.Length;

        /// <summary>
        /// Gets the dictionary of (old) properties to migrate in a newer version
        /// </summary>
        public static readonly Dictionary<string, object> MigrationProperties = new Dictionary<string, object>();

        private static readonly byte[] png_magic = new byte[] {0x89, 0x50, 0x4e, 0x47};
        private static readonly int png_magic_len = png_magic.Length;

        /// <summary>
        /// Manage the channel processors working on received items and feeds
        /// </summary>
        private static readonly NewsChannelServices receivingNewsChannel = new NewsChannelServices();

        /// <summary>
        /// Manage the NewsItem relations
        /// </summary>
        private static readonly IRelationCosmos relationCosmos = RelationCosmosFactory.Create();

        /// <summary>
        /// Indicates whether the relationship cosmos should be built for incoming news items. 
        /// </summary>
        internal static bool buildRelationCosmos = true;

        /// <summary>
        /// The string used to build categories hierarchy
        /// </summary>
        public static string CategorySeparator = @"\";

        /// <summary>
        /// Gets the default refresh rate in millisecs (1 hour).
        /// Means: how often feeds are refreshed by default if no specific rate specified 
        /// by the configuration.
        /// </summary>
        public static int DefaultRefreshRate = 60*60*1000;

        /// <summary>
        /// Indicates the maximum amount of space that enclosures and 
        /// podcasts can use on disk.
        /// </summary>
        private static int enclosurecachesize = DefaultEnclosureCacheSize;

        /// <summary>
        /// Manage the lucene search 
        /// </summary>
        protected static LuceneSearch p_searchHandler;

        /// <summary>
        /// Indicates whether the cookies from IE should be taken over for our own requests. 
        /// Default is true.
        /// </summary>
        private static bool setCookies = true;

        private static bool unconditionalCommentRss;

        ///<summary>
        ///Internal flag used to track whether the XML in the feed list validated against the schema. 
        ///</summary>
        public static bool validationErrorOccured;

        /// <summary>
        /// Hashtable representing downloaded feed items
        /// </summary>
        protected readonly Dictionary<string, IFeedDetails> itemsTable =
            new Dictionary<string, IFeedDetails>();

        /// <summary>
        /// The file extensions of enclosures that should be treated as podcasts. 
        /// </summary>
        protected readonly ArrayList podcastfileextensions = new ArrayList();

        /// <summary>
        /// The Application Name or ID that uses the component. This will be used to 
        /// initialize the user path to store the feeds file and cached items.
        /// </summary>
        internal string applicationName = "NewsComponents";

        /// <summary>
        /// Used for making asynchronous Web requests
        /// </summary>
        protected AsyncWebRequest AsyncWebRequest;

        /// <summary>
        /// Represents the list of available categories for feeds. 
        /// </summary>
        protected IDictionary<string, INewsFeedCategory> categories = new SortedDictionary<string, INewsFeedCategory>();

        /// <summary>
        /// Downloads enclosures/podcasts in the background using BITS. 
        /// </summary>
        protected BackgroundDownloadManager enclosureDownloader;

        /// <summary>
        /// FeedsCollection representing subscribed feeds list
        /// </summary>
        protected IDictionary<string, INewsFeed> feedsTable = new SortedDictionary<string, INewsFeed>(UriHelper.Comparer);

        /// <summary>
        /// Collection contains UserIdentity objects.
        /// Keys are the UserIdentity.Name's
        /// </summary>
        private IDictionary<string, UserIdentity> identities = new Dictionary<string, UserIdentity>();

        /// <summary>
        /// Indicates whether the application is offline or not. 
        /// </summary>
        protected bool isOffline;

        /// <summary>
        /// Represents the list of available feed column layouts for feeds. 
        /// </summary>
        protected FeedColumnLayoutCollection layouts = new FeedColumnLayoutCollection();

        /// <summary>
        /// Gets the location of the feed
        /// </summary>
        protected SubscriptionLocation location;

        /// <summary>
        /// Collection contains NntpServerDefinition objects.
        /// Keys are the account name(s) - friendly names for the news server def.:
        /// NntpServerDefinition.Name's
        /// </summary>
        private IDictionary<string, INntpServerDefinition> nntpServers = new Dictionary<string, INntpServerDefinition>();

        /// <summary>
        /// Configuration provider
        /// </summary>
        protected INewsComponentsConfiguration p_configuration;

        /// <summary>
        /// This is the object that is returned when returning the list of categories in GetCategories()
        /// </summary>
        /// <seealso cref="GetCategories"/>
        protected ReadOnlyDictionary<string, INewsFeedCategory> readonly_categories;

        /// <summary>
        /// This is the object that is returned when returning the list of feeds in GetFeeds()
        /// </summary>
        /// <seealso cref="GetFeeds"/>
        protected ReadOnlyDictionary<string, INewsFeed> readonly_feedsTable;

        /// <summary>
        /// Manages the FeedType.Rss 
        /// </summary>
        protected RssParser rssParser;

        #region delegates/events/argument classes

        #region Delegates

        /// <summary>
        /// Callback delegate used on event OnDeletedCategory.
        /// </summary>
        public delegate void AddedCategoryCallback(object sender, CategoryEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnAddedFeed.
        /// </summary>
        public delegate void AddedFeedCallback(object sender, FeedChangedEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnDeletedCategory.
        /// </summary>
        public delegate void DeletedCategoryCallback(object sender, CategoryEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnDeletedFeed.
        /// </summary>
        public delegate void DeletedFeedCallback(object sender, FeedDeletedEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnDownloadedEnclosure.
        /// </summary>
        public delegate void DownloadedEnclosureCallback(object sender, DownloadItemEventArgs e);

        /// <summary>
        /// The callback used within the BeforeDownloadFeedStarted event.
        /// </summary>
        public delegate void DownloadFeedStartedCallback(object sender, DownloadFeedCancelEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnMovedCategory.
        /// </summary>
        public delegate void MovedCategoryCallback(object sender, CategoryChangedEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnMovedFeed.
        /// </summary>
        public delegate void MovedFeedCallback(object sender, FeedMovedEventArgs e);

        /// <summary>Signature for <see cref="NewsItemSearchResult">NewsItemSearchResult</see>  event</summary>
        public delegate void NewsItemSearchResultEventHandler(object sender, NewsItemSearchResultEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnRenamedCategory.
        /// </summary>
        public delegate void RenamedCategoryCallback(object sender, CategoryChangedEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnRenamedFeed.
        /// </summary>
        public delegate void RenamedFeedCallback(object sender, FeedRenamedEventArgs e);

        /// <summary>Signature for <see cref="SearchFinished">SearchFinished</see>  event</summary>
        public delegate void SearchFinishedEventHandler(object sender, SearchFinishedEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnUpdatedFavicon.
        /// </summary>
        public delegate void UpdatedFaviconCallback(object sender, UpdatedFaviconEventArgs e);

        /// <summary>
        /// Callback delegate used on event OnUpdatedFeed.
        /// </summary>
        public delegate void UpdatedFeedCallback(object sender, UpdatedFeedEventArgs e);

        /// <summary>
        /// Callback delegate used for event OnUpdateFeedException
        /// </summary>
        public delegate void UpdateFeedExceptionCallback(object sender, UpdateFeedExceptionEventArgs e);

        /// <summary>
        /// Delegate used for UpdateFeedsStarted event.
        /// </summary>
        public delegate void UpdateFeedsStartedHandler(object sender, UpdateFeedsEventArgs e);

        /// <summary>
        /// Delegate used for UpdateFeedStarted event.
        /// </summary>
        public delegate void UpdateFeedStartedHandler(object sender, UpdateFeedEventArgs e);

        #endregion

        /// <summary>
        /// The event that will be invoked on clients to notify them that 
        /// when a feed starts to be downloaded (AsyncWebRequest). 
        /// </summary>
        public event DownloadFeedStartedCallback BeforeDownloadFeedStarted = null;

        /// <summary>
        /// Event called on every updated feed.
        /// </summary>
        public event UpdatedFeedCallback OnUpdatedFeed = null;


        /// <summary>
        /// Event called on every deleted category.
        /// </summary>
        public event DeletedCategoryCallback OnDeletedCategory = null;

        /// <summary>
        /// Event called on every deleted category.
        /// </summary>
        public event AddedCategoryCallback OnAddedCategory = null;

        /// <summary>
        /// Event called on every renamed category.
        /// </summary>
        public event RenamedCategoryCallback OnRenamedCategory = null;

        /// <summary>
        /// Event called on every moved category.
        /// </summary>
        public event MovedCategoryCallback OnMovedCategory = null;

        /// <summary>
        /// Event called on every added feed.
        /// </summary>
        public event AddedFeedCallback OnAddedFeed = null;

        /// <summary>
        /// Event called on every deleted feed.
        /// </summary>
        public event DeletedFeedCallback OnDeletedFeed = null;

        /// <summary>
        /// Event called on every renamed feed.
        /// </summary>
        public event RenamedFeedCallback OnRenamedFeed = null;

        /// <summary>
        /// Event called on every moved feed.
        /// </summary>
        public event MovedFeedCallback OnMovedFeed = null;

        /// <summary>
        /// Event called on every completed enclosure download. 
        /// </summary>
        public event DownloadedEnclosureCallback OnDownloadedEnclosure = null;

        /// <summary>
        /// Event called on every updated favicon.
        /// </summary>
        public event UpdatedFaviconCallback OnUpdatedFavicon = null;


        /// <summary>
        /// Event called, if the WebRequest fails with any exception.
        /// </summary>
        public event UpdateFeedExceptionCallback OnUpdateFeedException = null;

        /// <summary>
        /// Called if RefreshFeeds() was initiated (all feeds).
        /// </summary>
        public event UpdateFeedsStartedHandler UpdateFeedsStarted = null;

        /// <summary>
        /// Called as each individual feed start to refresh
        /// </summary>
        public event UpdateFeedStartedHandler UpdateFeedStarted = null;

        /// <summary>
        /// Called if all async. requests are done.
        /// </summary>
        public event EventHandler OnAllAsyncRequestsCompleted = null;

        //Search	impl. 

        /// <summary>Called if NewsItems are found, that match the search criteria(s)</summary>
        public event NewsItemSearchResultEventHandler NewsItemSearchResult;

        /// <summary>Called on a search finished</summary>
        public event SearchFinishedEventHandler SearchFinished;

        #region Nested type: CategoryChangedEventArgs

        /// <summary>
        /// Category event argument class.
        /// </summary>
        public class CategoryChangedEventArgs : CategoryEventArgs
        {
            /// <summary>
            /// Provides information on the category event
            /// </summary>
            /// <param name="categoryName">The name of the affected category</param>
            /// <param name="newCategoryName">New name of the category.</param>
            public CategoryChangedEventArgs(string categoryName, string newCategoryName)
                : base(categoryName)
            {
                NewCategoryName = newCategoryName;
            }

            public string NewCategoryName { get; set; }
        }

        #endregion

        #region Nested type: CategoryEventArgs

        /// <summary>
        /// Category event argument class.
        /// </summary>
        public class CategoryEventArgs : EventArgs
        {
            /// <summary>
            /// Provides information on the category event
            /// </summary>
            /// <param name="categoryName">The name of the affected category</param>
            public CategoryEventArgs(string categoryName)
            {
                CategoryName = categoryName;
            }

            public string CategoryName { get; set; }
        }

        #endregion

        #region Nested type: DownloadFeedCancelEventArgs

        /// <summary>
        /// BeforeDownloadFeedStarted event argument class.
        /// </summary>
        [ComVisible(false)]
        public class DownloadFeedCancelEventArgs : CancelEventArgs
        {
            private readonly Uri feedUri;

            /// <summary>
            /// Class initializer.
            /// </summary>
            /// <param name="feed">feed Uri</param>
            /// <param name="cancel">bool, set to true, if you want to cancel further processing</param>
            public DownloadFeedCancelEventArgs(Uri feed, bool cancel) : base(cancel)
            {
                feedUri = feed;
            }

            /// <summary>
            /// The related feed Uri.
            /// </summary>
            public Uri FeedUri
            {
                get { return feedUri; }
            }
        }

        #endregion

        #region Nested type: FeedChangedEventArgs

        public class FeedChangedEventArgs : EventArgs
        {
            public FeedChangedEventArgs(string feedUrl)
            {
                FeedUrl = feedUrl;
            }

            public string FeedUrl { get; set; }
        }

        #endregion

        #region Nested type: FeedDeletedEventArgs

        public class FeedDeletedEventArgs : FeedChangedEventArgs
        {
            public FeedDeletedEventArgs(string feedUrl, string title) : base(feedUrl)
            {
                Title = title;
            }

            public string Title { get; set; }
        }

        #endregion

        #region Nested type: FeedMovedEventArgs

        public class FeedMovedEventArgs : FeedChangedEventArgs
        {
            public FeedMovedEventArgs(string feedUrl, string newCategory) : base(feedUrl)
            {
                NewCategory = newCategory;
            }

            public string NewCategory { get; set; }
        }

        #endregion

        #region Nested type: FeedRenamedEventArgs

        public class FeedRenamedEventArgs : FeedChangedEventArgs
        {
            public FeedRenamedEventArgs(string feedUrl, string newName) : base(feedUrl)
            {
                NewName = newName;
            }

            public string NewName { get; set; }
        }

        #endregion

        #region Nested type: FeedSearchResultEventArgs

        /// <summary>
        /// Contains the search result, if NewsFeed's are found. Used on FeedSearchResult event.
        /// </summary>
        [ComVisible(false)]
        public class FeedSearchResultEventArgs : CancelEventArgs
        {
            /// <summary>
            /// NewsFeed.
            /// </summary>
            public NewsFeed Feed;

            /// <summary>
            /// Object used by the caller only
            /// </summary>
            public object Tag;

            /// <summary>
            /// Initializer
            /// </summary>
            /// <param name="f">NewsFeed</param>
            /// <param name="tag">object, used by the caller only</param>
            /// <param name="cancel">true, if the search request should be cancelled</param>
            public FeedSearchResultEventArgs(
                NewsFeed f, object tag, bool cancel) : base(cancel)
            {
                Feed = f;
                Tag = tag;
            }
        }

        #endregion

        #region Nested type: NewsItemSearchResultEventArgs

        /// <summary>
        /// Contains the search result, if NewsItem's are found. Used on NewsItemSearchResult event.
        /// </summary>
        [ComVisible(false)]
        public class NewsItemSearchResultEventArgs : CancelEventArgs
        {
            /// <summary>
            /// NewsItem list
            /// </summary>
            public List<INewsItem> NewsItems;

            /// <summary>
            /// Object used by caller
            /// </summary>
            public object Tag;

            /// <summary>
            /// Initializer
            /// </summary>
            /// <param name="items">ArrayList of NewsItems</param>
            /// <param name="tag">Object used by caller</param>
            /// <param name="cancel"></param>
            public NewsItemSearchResultEventArgs(
                List<INewsItem> items, object tag, bool cancel) : base(cancel)
            {
                NewsItems = items;
                Tag = tag;
            }
        }

        #endregion

        #region Nested type: SearchFinishedEventArgs

        /// <summary>
        /// Provide informations about a finished search. Used on SearchFinished event.
        /// </summary>
        public class SearchFinishedEventArgs : EventArgs
        {
            /// <summary></summary>
            public readonly FeedInfoList MatchingFeeds;

            /// <summary></summary>
            public readonly int MatchingFeedsCount;

            /// <summary></summary>
            public readonly List<INewsItem> MatchingItems;

            /// <summary></summary>
            public readonly int MatchingItemsCount;

            /// <summary></summary>
            public readonly object Tag;

            /// <summary>
            /// Initializer
            /// </summary>
            /// <remarks>This modifies the input FeedInfoList by replacing its NewsItem contents 
            /// with SearchHitNewsItems</remarks>
            /// <param name="tag">Object used by caller</param>
            /// <param name="matchingFeeds"></param>
            /// <param name="matchingFeedsCount">integer stores the count of matching feeds</param>
            /// <param name="matchingItemsCount">integer stores the count of matching NewsItem's (over all feeds)</param>
            public SearchFinishedEventArgs(
                object tag, FeedInfoList matchingFeeds, int matchingFeedsCount, int matchingItemsCount) :
                    this(tag, matchingFeeds, new List<INewsItem>(), matchingFeedsCount, matchingItemsCount)
            {
                var temp = new List<INewsItem>();

                foreach (FeedInfo fi in matchingFeeds)
                {
                    foreach (var ni in fi.ItemsList)
                    {
                        if (ni is SearchHitNewsItem)
                            temp.Add(ni);
                        else
                            temp.Add(new SearchHitNewsItem(ni));
                    }
                    fi.ItemsList.Clear();
                    fi.ItemsList.AddRange(temp);
                    MatchingItems.AddRange(temp);
                    temp.Clear();
                } //foreach
            }

            /// <summary>
            /// Initializer
            /// </summary>
            /// <param name="tag">Object used by caller</param>
            /// <param name="matchingFeeds">The matching feeds.</param>
            /// <param name="matchingNewsItems">The matching news items.</param>
            /// <param name="matchingFeedsCount">integer stores the count of matching feeds</param>
            /// <param name="matchingItemsCount">integer stores the count of matching NewsItem's (over all feeds)</param>
            public SearchFinishedEventArgs(
                object tag, FeedInfoList matchingFeeds, IEnumerable<INewsItem> matchingNewsItems, int matchingFeedsCount,
                int matchingItemsCount)
            {
                MatchingFeedsCount = matchingFeedsCount;
                MatchingItemsCount = matchingItemsCount;
                MatchingFeeds = matchingFeeds;
                MatchingItems = new List<INewsItem>(matchingNewsItems);
                Tag = tag;
            }
        }

        #endregion

        #region Nested type: UpdatedFaviconEventArgs

        /// <summary>
        /// OnUpdatedFavicon event argument class.
        /// </summary>
        public class UpdatedFaviconEventArgs : EventArgs
        {
            private readonly string favicon;

            private readonly StringCollection feedUrls;

            /// <summary>
            /// Called on every updated favicon.
            /// </summary>
            /// <param name="favicon"> The name of the favicon file</param> 
            /// <param name="feedUrls">The list of URLs that will utilize this favicon</param>		
            public UpdatedFaviconEventArgs(string favicon, StringCollection feedUrls)
            {
                this.favicon = favicon;
                this.feedUrls = feedUrls;
            }

            /// <summary>
            /// The name of the favicon file. 
            /// </summary>
            public string Favicon
            {
                get { return favicon; }
            }

            /// <summary>
            /// The URLs of the feeds that will utilize this favicon. 
            /// </summary>
            public StringCollection FeedUrls
            {
                get { return feedUrls; }
            }
        }

        #endregion

        #region Nested type: UpdatedFeedEventArgs

        /// <summary>
        /// OnUpdatedFeed event argument class.
        /// </summary>
        public class UpdatedFeedEventArgs : EventArgs
        {
            private readonly bool firstSuccessfulDownload;
            private readonly Uri newUri;
            private readonly int priority;
            private readonly Uri requestUri;
            private readonly RequestResult result;

            /// <summary>
            /// Called on every updated feed.
            /// </summary>
            /// <param name="requestUri">Original requested Uri of the feed</param>
            /// <param name="newUri">The (maybe) new feed location. This could be set on a redirect or other mechanism.
            /// If the location was not changed, this parameter is left null</param>
            /// <param name="result">If result is <c>NotModified</c>, the conditional GET succeeds and no items are returned.</param>
            /// <param name="priority">Priority of the request</param>
            /// <param name="firstSuccessfulDownload">Indicates whether this is the first time the feed has been successfully downloaded
            /// to the cache</param>
            public UpdatedFeedEventArgs(Uri requestUri, Uri newUri, RequestResult result, int priority,
                                        bool firstSuccessfulDownload)
            {
                this.requestUri = requestUri;
                this.newUri = newUri;
                this.result = result;
                this.priority = priority;
                this.firstSuccessfulDownload = firstSuccessfulDownload;
            }

            /// <summary>
            /// Uri of the feed, that was updated
            /// </summary>
            public Uri UpdatedFeedUri
            {
                get { return requestUri; }
            } // should return Clone() ?
            /// <summary>
            /// Uri of the feed, if it was moved on the Web to a new location.
            /// </summary>
            public Uri NewFeedUri
            {
                get { return newUri; }
            } // should return Clone() ?

            /// <summary>
            /// RequestResult: OK or NotModified
            /// </summary>
            public RequestResult UpdateState
            {
                get { return result; }
            }

            /// <summary>
            /// Gets the queued priority
            /// </summary>
            public int Priority
            {
                get { return priority; }
            }

            /// <summary>
            /// Indicates whether this is the first time the feed has been downloaded to 
            /// the cache. 
            /// </summary>
            public bool FirstSuccessfulDownload
            {
                get { return firstSuccessfulDownload; }
            }
        }

        #endregion

        #region Nested type: UpdateFeedEventArgs

        /// <summary>
        /// UpdateFeedStarted event argument class. Single feed update.
        /// </summary>
        public class UpdateFeedEventArgs : UpdateFeedsEventArgs
        {
            private readonly Uri feedUri;

            private readonly int priority;

            /// <summary>
            /// Initializer
            /// </summary>
            /// <param name="feed">feed Uri</param>
            /// <param name="forced">true, if it was a forced (manually initiated) request</param>
            /// <param name="priority">Priority of the request</param>
            public UpdateFeedEventArgs(Uri feed, bool forced, int priority) : base(forced)
            {
                feedUri = feed;
                this.priority = priority;
            }

            /// <summary>
            /// Feed Uri.
            /// </summary>
            public Uri FeedUri
            {
                get { return feedUri; }
            }

            /// <summary>
            /// Gets the queued priority
            /// </summary>
            public int Priority
            {
                get { return priority; }
            }
        }

        #endregion

        #region Nested type: UpdateFeedExceptionEventArgs

        /// <summary>
        /// Event argument class used in OnUpdateFeedException.
        /// </summary>
        public class UpdateFeedExceptionEventArgs : EventArgs
        {
            private readonly Exception exception;
            private readonly int priority;
            private readonly string requestUri;

            /// <summary>
            /// Initializer
            /// </summary>
            /// <param name="requestUri">feed Uri, that was requested</param>
            /// <param name="e">Exception caused by the request</param>
            /// <param name="priority">int</param>
            public UpdateFeedExceptionEventArgs(string requestUri, Exception e, int priority)
            {
                this.requestUri = requestUri;
                exception = e;
                this.priority = priority;
            }

            /// <summary>
            /// feed Uri.
            /// </summary>
            public string FeedUri
            {
                get { return requestUri; }
            }

            /// <summary>
            /// caused exception
            /// </summary>
            public Exception ExceptionThrown
            {
                get { return exception; }
            }

            /// <summary>
            /// Gets the queued priority
            /// </summary>
            public int Priority
            {
                get { return priority; }
            }
        }

        #endregion

        #region Nested type: UpdateFeedsEventArgs

        /// <summary>
        /// UpdateFeedsStarted event argument class. Multiple feeds update.
        /// </summary>
        public class UpdateFeedsEventArgs : EventArgs
        {
            private readonly bool forced;

            /// <summary>
            /// Initializer
            /// </summary>
            /// <param name="forced">true, if it was a forced (manually initiated) request</param>
            public UpdateFeedsEventArgs(bool forced)
            {
                this.forced = forced;
            }

            /// <summary>
            /// True, if it was a manually forced request
            /// </summary>
            public bool ForcedRefresh
            {
                get { return forced; }
            }
        }

        #endregion

        #endregion

        #region Proxy handling

        #region ProxyWrapper class

        private class ProxyWrapper
        {
            private IWebProxy _proxy;

            public IWebProxy Proxy
            {
                get
                {
                    if (_proxy == null)
                        return WebRequest.DefaultWebProxy;
                    return _proxy;
                }
                set { _proxy = value; }
            }

            public void ResetProxy()
            {
                _proxy = null;
            }
        }

        #endregion

        private static readonly ProxyWrapper globalProxy = new ProxyWrapper();

        /// <summary>
        /// Gets or sets the global proxy used by all FeedSource instances.
        /// </summary>
        /// <value>The global proxy.</value>
        /// <remarks>This property is thread safe.</remarks>
        public static IWebProxy GlobalProxy
        {
            get
            {
                lock (globalProxy)
                    return globalProxy.Proxy;
            }
            set
            {
                lock (globalProxy)
                    globalProxy.Proxy = value;
            }
        }

        /// <summary>
        /// Proxy server information used for connections when fetching feeds. 
        /// </summary>
        /// <remarks>
        /// There are other components that use a instance reference/interface of
        /// FeedSource, so we provide the proxy also as a instance property here.
        /// </remarks>
        public IWebProxy Proxy
        {
            get { return GlobalProxy; }
        }

        /// <summary>
        /// Call to uses the default proxy. The default proxy is the value
        /// returned by WebRequest.DefaultWebProxy - so ensure to NOT
        /// modify that by calls to GlobalProxySelection.Select = new Proxy()
        /// or WebRequest.DefaultWebProxy = new Proxy() calls!
        /// Please use the <see cref="GlobalProxy"/> property to assign a
        /// user customized proxy or one that take over IE settings, then
        /// you are always able to switch back to use the default system proxy
        /// calling this method!
        /// </summary>
        /// <remarks>This method is thread safe.</remarks>
        public static void UseDefaultProxy()
        {
            lock (globalProxy)
                globalProxy.ResetProxy();
        }

        #endregion

    	private FeedSourceType sourceType;
		/// <summary>
		/// Gets the type of the source.
		/// </summary>
		/// <value>The type.</value>
    	public FeedSourceType Type
    	{
			get { return sourceType; }
    	}

        /// <summary>
        /// Gets the NewsComponents configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public INewsComponentsConfiguration Configuration
        {
            get { return p_configuration; }
        }

        /// <summary>
        /// Gets the cache manager.
        /// </summary>
        /// <value>The cache manager.</value>
        internal CacheManager CacheHandler
        {
            get { return p_configuration.CacheManager; }
        }

        /// <summary>
        /// Indicates whether the download interval has been reached. We should not start downloading feeds or favicons
        /// until this property is true. 
        /// </summary>
        public bool DownloadIntervalReached
        {
            get { return (DateTime.Now - ApplicationStartTime).TotalMilliseconds >= RefreshRate; }
        }

//		/// <summary>
//		/// Manages the cache. 
//		/// </summary>
//		[Obsolete("Use CacheHandler property")]
//		private CacheManager cacheHandler; 


        /// <summary>
        /// The location where feed items are cached.
        /// </summary>
        internal string CacheLocation
        {
            get { return CacheHandler.CacheLocation; }
        }

        /// <summary>
        /// Provide access to the RssParser for Rss specific tasks
        /// </summary>
        internal RssParser RssParserInstance
        {
            get { return rssParser; }
        }

        /// <summary>
        /// Gets or sets the search index handler.
        /// </summary>
        /// <value>The search handler.</value>
        public static LuceneSearch SearchHandler
        {
            get
            {
                if (p_searchHandler == null)
                    p_searchHandler = new LuceneSearch(DefaultConfiguration);

                return p_searchHandler;
            }
            set { p_searchHandler = value; }
        }

        /// <summary>
        /// Indicates whether the cookies from IE should be taken over for our own requests. 
        /// Default is true.
        /// </summary>
        public static bool SetCookies
        {
            set { setCookies = value; }
            get { return setCookies; }
        }

        /// <summary>
        /// Indicates whether the relationship cosmos should be built for incoming news items. 
        /// </summary>
        public static bool BuildRelationCosmos
        {
            set
            {
                buildRelationCosmos = value;
                if (buildRelationCosmos == false)
                    relationCosmos.Clear();
            }
            get { return buildRelationCosmos; }
        }


        /// <summary>
        /// Indicates whether the application is offline or not. 
        /// </summary>
        public bool Offline
        {
            set
            {
                isOffline = value;
                if (RssParserInstance != null)
                    RssParserInstance.Offline = value;
            }
            get { return isOffline; }
        }

        /// <summary>
        /// Boolean flag indicates whether the commentCount should be considered
        /// for NewsItem.HasExternalRelations() tests.
        ///	 Default is false and will test both the CommentRssUrl as a non-empty string
        ///	 and commentCount > 0 (zero)
        /// </summary>
        public static bool UnconditionalCommentRss
        {
            set { unconditionalCommentRss = value; }
            get { return unconditionalCommentRss; }
        }

        /// <summary>
        /// Gets or sets the maximum amount of time an item should be kept in the 
        /// cache. This value is used for all feeds unless one is specified on 
        /// the particular feed or its category
        /// </summary>
        public TimeSpan MaxItemAge { get; set; }

        /// <summary>
        /// Gets or sets the stylesheet for displaying feeds
        /// </summary>
        public static string Stylesheet { get; set; }

        /// <summary>
        /// Gets or sets the folder for downloading enclosures
        /// </summary>
        public static string EnclosureFolder { get; set; }


        /// <summary>
        /// Gets the list of file extensions of enclosures that should be treated as podcasts
        /// as a string. 
        /// </summary>
        public string PodcastFileExtensionsAsString
        {
            get
            {
                var toReturn = new StringBuilder();

                foreach (string s in podcastfileextensions)
                {
                    if (!StringHelper.EmptyTrimOrNull(s))
                    {
                        toReturn.Append(s);
                        toReturn.Append(";");
                    }
                }

                return toReturn.ToString();
            }

            set
            {
                var fileexts = value.Split(new[] {';', ' '});
                podcastfileextensions.Clear();

                foreach (var s in fileexts)
                {
                    podcastfileextensions.Add(s);
                }
            }
        }

        /// <summary>
        /// Gets or sets the folder for downloading podcasts
        /// </summary>
        public string PodcastFolder { get; set; }

        /// <summary>
        /// Gets or sets whether items in the feed should be marked as read on exiting
        /// the feed in the UI
        /// </summary>
        public static bool MarkItemsReadOnExit { get; set; }


        /// <summary>
        /// Indicates the maximum amount of space that enclosures and 
        /// podcasts can use on disk.
        /// </summary>
        public static int EnclosureCacheSize
        {
            get { return enclosurecachesize; }

            set { enclosurecachesize = value; }
        }

        /// <summary>
        /// Indicates the number of enclosures which should be downloaded automatically from a newly subscribed feed.
        /// </summary>
        public static int NumEnclosuresToDownloadOnNewFeed { get; set; }


        /// <summary>
        /// Gets or sets whether  podcasts and enclosures should be downloaded to a folder 
        /// named after the feed
        /// </summary>
        public static bool CreateSubfoldersForEnclosures { get; set; }

        /// <summary>
        /// Gets or sets whether a toast windows should be displayed on a successful download
        /// of an enclosure.
        /// </summary>
        public static bool EnclosureAlert { get; set; }

        /// <summary>
        /// Gets or sets wwhich properties of a NewsItem should be made columns in the RSS Bandit listview
        /// </summary>
        public string FeedColumnLayout { get; set; }

        /// <summary>
        /// Accesses the list of user specified layouts (currently listview only) 
        /// </summary>
        public FeedColumnLayoutCollection ColumnLayouts
        {
            get
            {
                if (layouts == null)
                {
                    layouts = new FeedColumnLayoutCollection();
                }

                return layouts;
            }
        }

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
                    nntpServers = new Dictionary<string, INntpServerDefinition>();
                }

                return nntpServers;
            }
        }

        /// <summary>
        /// Accesses the list of UserIdentity objects.
        /// Keys are the UserIdentity.Name's
        /// </summary>
        public IDictionary<string, UserIdentity> UserIdentity
        {
            [DebuggerStepThrough]
            get
            {
                if (identities == null)
                {
                    identities = new Dictionary<string, UserIdentity>();
                }

                return identities;
            }
        }

        /// <summary>
        ///  How often feeds are refreshed by default if no specific rate specified by the feed. 
        ///  Setting this property resets the refresh rate for all feeds. 
        /// </summary>
        /// <remarks>If set to a negative value then the old value remains. Setting the 
        /// value to zero means feeds are no longer updated.</remarks>
        public virtual int RefreshRate
        {
            //set
            //{
            //    if (value >= 0)
            //    {
            //        this.refreshrate = value;
            //    }
            /* 
				 * moved to ResetAllRefreshRateSettings():
				 * 
                string[] keys;

                lock (feedsTable)
                {
                    keys = new string[feedsTable.Count];
                    if (feedsTable.Count > 0)
                        feedsTable.Keys.CopyTo(keys, 0);
                }

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    INewsFeed f = null;
                    if (feedsTable.TryGetValue(keys[i], out f))
                    {
                        f.refreshrate = this.refreshrate;
                        f.refreshrateSpecified = true;
                    }
                }
				*/
            //}

            get { return p_configuration.RefreshRate; }
        }

        /// <summary>
        /// Boolean flag indicates whether the feeds list was loaded 
        /// successfully during the last call to LoadFeedlist()
        /// </summary>
        public bool FeedsListOK
        {
            get { return !validationErrorOccured; }
        }

        #region HTTP UserAgent 

        /// <summary>
        /// Our default short HTTP user agent string
        /// </summary>
        public const string DefaultUserAgent = "RssBandit/2.x";

        /// <summary>
        /// A template string to assamble a unified user agent string.
        /// </summary>
        private static readonly string userAgentTemplate;

        /// <summary>
        /// global long HTTP user agent string
        /// </summary>
        private static string globalLongUserAgent;

        /// <summary>
        /// The short HTTP user agent string used when requesting feeds
        /// and the property was not set via 
        /// </summary>
        private string useragent;

        /// <summary>
        /// Returns a global long HTTP user agent string build from the
        /// instance setting. 
        /// To be used by sub-components that do not have a instance variable 
        /// of the FeedSource.
        /// </summary>
        public static string GlobalUserAgentString
        {
            get
            {
                if (null == globalLongUserAgent)
                    globalLongUserAgent = UserAgentString(DefaultUserAgent);
                return globalLongUserAgent;
            }
        }

        /// <summary>
        /// The short HTTP user agent string used when requesting feeds. 
        /// </summary>
        public string UserAgent
        {
            get
            {
                if (String.IsNullOrEmpty(useragent))
                {
                    if (Configuration != null)
                    {
                        useragent = String.Format("{0}/{1}", Configuration.ApplicationID,
                                                  Configuration.ApplicationVersion);
                        globalLongUserAgent = UserAgentString(useragent);
                    }
                    else
                    {
                        // wait for a valid configuration, but return a usable value:
                        return DefaultUserAgent;
                    }
                }
                return useragent;
            }
            set
            {
                useragent = value;
                globalLongUserAgent = UserAgentString(useragent);
            }
        }

        /// <summary>
        /// The long HTTP user agent string used when requesting feeds. 
        /// </summary>
        public string FullUserAgent
        {
            get { return UserAgentString(UserAgent); }
        }

        /// <summary>
        /// Build a full user agent string incl. OS and .NET version 
        /// from the provided userAgent
        /// </summary>
        /// <param name="userAgent">string</param>
        /// <returns>The long HTTP user agent string</returns>
        public static string UserAgentString(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return GlobalUserAgentString;
            return String.Format(userAgentTemplate, userAgent);
        }

        #endregion

        #region Top Stories related

        /// <summary>
        /// This is a table of mappings of URLs to story titles for the top stories that have been returned by 
        /// GetTopStories()
        /// </summary>
        /// <seealso cref="GetTopStories"/>
        private static readonly Dictionary<string, storyNdate> TopStoryTitles = new Dictionary<string, storyNdate>();

        private static bool topStoriesModified;

        public static bool TopStoriesModified
        {
            get { return topStoriesModified; }
        }


        private class storyNdate
        {
            public readonly DateTime firstSeen;
            public readonly string storyTitle;

            public storyNdate(string title, DateTime date)
            {
                storyTitle = title;
                firstSeen = date;
            }
        }

        #endregion

        #region Feed Credentials handling

        /// <summary>
        /// Creates the credentials from a feed.
        /// </summary>
        /// <param name="feed">The feed</param>
        /// <returns>ICredentials</returns>
        public static ICredentials CreateCredentialsFrom(INewsFeed feed)
        {
            if (feed != null && !string.IsNullOrEmpty(feed.authUser))
            {
                string u = null, p = null;
                GetFeedCredentials(feed, ref u, ref p);
                return CreateCredentialsFrom(feed.link, u, p);
            }
            return null;
        }

        /// <summary>
        /// Creates the credentials from an url.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="domainUser">The domain user.</param>
        /// <param name="password">The password.</param>
        /// <returns>ICredentials</returns>
        public static ICredentials CreateCredentialsFrom(string url, string domainUser, string password)
        {
            ICredentials c = null;

            if (!string.IsNullOrEmpty(domainUser))
            {
                var credentials = CreateCredentialsFrom(domainUser, password);
                try
                {
                    var feedUri = new Uri(url);
                    var cc = new CredentialCache
                                 {
                                     {feedUri, "Basic", credentials},
                                     {feedUri, "Digest", credentials},
                                     {feedUri, "NTLM", credentials}
                                 };
                    c = cc;
                }
                catch (UriFormatException)
                {
                    c = credentials;
                }
            }
            return c;
        }

        /// <summary>
        /// Create and return a ICredentials object with the provided informations.
        /// </summary>
        /// <param name="domainUser">username and optional a domain: DOMAIN\user</param>
        /// <param name="password">the pwd</param>
        /// <returns>NetworkCredential</returns>
        public static NetworkCredential CreateCredentialsFrom(string domainUser, string password)
        {
            NetworkCredential c = null;
            if (domainUser != null)
            {
                NetworkCredential credentials;
                var aDomainUser = domainUser.Split(new[] {'\\'});
                if (aDomainUser.GetLength(0) > 1) // Domain specified: e.g. Domain\UserName
                    credentials = new NetworkCredential(aDomainUser[1], password, aDomainUser[0]);
                else
                    credentials = new NetworkCredential(aDomainUser[0], password);

                c = credentials;
            }
            return c;
        }

        /// <summary>
        /// Set the authorization credentials for a feed.
        /// </summary>
        /// <param name="feed">NewsFeed to be modified</param>
        /// <param name="user">username, identifier</param>
        /// <param name="pwd">password</param>
        public static void SetFeedCredentials(INewsFeed feed, string user, string pwd)
        {
            if (feed == null) return;
            feed.authPassword = CryptHelper.EncryptB(pwd);
            feed.authUser = user;
        }

        /// <summary>
        /// Get the authorization credentials for a feed.
        /// </summary>
        /// <param name="feed">NewsFeed, where the credentials are taken from</param>
        /// <param name="user">String return parameter containing the username</param>
        /// <param name="pwd">String return parameter, containing the password</param>
        public static void GetFeedCredentials(INewsFeed feed, ref string user, ref string pwd)
        {
            if (feed == null) return;
            pwd = CryptHelper.Decrypt(feed.authPassword);
            user = feed.authUser;
        }


        /// <summary>
        /// Return ICredentials of a feed. 
        /// </summary>
        /// <param name="feedUrl">url of the feed</param>
        /// <returns>null in the case the feed does not have credentials</returns>
        public ICredentials GetFeedCredentials(string feedUrl)
        {
            if (feedUrl != null && feedsTable.ContainsKey(feedUrl))
                return GetFeedCredentials(feedsTable[feedUrl]);
            return null;
        }

        /// <summary>
        /// Return ICredentials of a feed. 
        /// </summary>
        /// <param name="feed">NewsFeed</param>
        /// <returns>null in the case the feed does not have credentials</returns>
        public static ICredentials GetFeedCredentials(INewsFeed feed)
        {
            ICredentials c = null;
            if (feed != null && feed.authUser != null)
            {
                return CreateCredentialsFrom(feed);
                //				string u = null, p = null;
                //				GetFeedCredentials(f, ref u, ref p);
                //				c = CreateCredentialsFrom(u, p);
            }
            return c;
        }

        #endregion

        #region NntpServerDefinition Credentials handling

        /// <summary>
        /// Set the authorization credentials for a Nntp Server.
        /// </summary>
        /// <param name="sd">NntpServerDefinition to be modified</param>
        /// <param name="user">username, identifier</param>
        /// <param name="pwd">password</param>
        public static void SetNntpServerCredentials(INntpServerDefinition sd, string user, string pwd)
        {
            var server = (NntpServerDefinition) sd;
            if (server == null) return;
            server.AuthPassword = CryptHelper.EncryptB(pwd);
            server.AuthUser = user;
        }

        /// <summary>
        /// Get the authorization credentials for a feed.
        /// </summary>
        /// <param name="sd">NntpServerDefinition, where the credentials are taken from</param>
        /// <param name="user">String return parameter containing the username</param>
        /// <param name="pwd">String return parameter, containing the password</param>
        public static void GetNntpServerCredentials(INntpServerDefinition sd, ref string user, ref string pwd)
        {
            var server = (NntpServerDefinition) sd;
            if (server == null) return;
            pwd = (server.AuthPassword != null ? CryptHelper.Decrypt(server.AuthPassword) : null);
            user = server.AuthUser;
        }


        /// <summary>
        /// Return ICredentials of a nntp server. 
        /// </summary>
        /// <param name="serverAccountName">account name of the server</param>
        /// <returns>null in the case the server does not have credentials</returns>
        public ICredentials GetNntpServerCredentials(string serverAccountName)
        {
            if (serverAccountName != null && nntpServers.ContainsKey(serverAccountName))
                return GetFeedCredentials(nntpServers[serverAccountName]);
            return null;
        }

        /// <summary>
        /// Gets the NNTP server credentials for a feed.
        /// </summary>
        /// <param name="f">The feed.</param>
        /// <returns>ICredentials</returns>
        internal ICredentials GetNntpServerCredentials(INewsFeed f)
        {
            ICredentials c = null;
            if (f == null || ! RssHelper.IsNntpUrl(f.link))
                return c;

            try
            {
                var feedUri = new Uri(f.link);

                foreach (NntpServerDefinition nsd  in nntpServers.Values)
                {
                    if (nsd.Server.Equals(feedUri.Authority))
                    {
                        c = GetNntpServerCredentials(nsd.Name);
                        break;
                    }
                }
            }
            catch (UriFormatException)
            {
            }
            return c;
        }

        /// <summary>
        /// Return ICredentials of a feed. 
        /// </summary>
        /// <param name="sd">NntpServerDefinition</param>
        /// <returns>null in the case the nntp server does not have credentials</returns>
        public static ICredentials GetFeedCredentials(INntpServerDefinition sd)
        {
            ICredentials c = null;
            if (sd.AuthUser != null)
            {
                string u = null, p = null;
                GetNntpServerCredentials(sd, ref u, ref p);
                c = CreateCredentialsFrom(u, p);
            }
            return c;
        }

        #endregion

        #region Trace support

        protected static bool p_traceMode;

        /// <summary>
        /// Boolean flag indicates whether errors should be written to a logfile 
        ///	using Trace.Write(); 
        /// </summary>
        public static bool TraceMode
        {
            set { p_traceMode = value; }
            get { return p_traceMode; }
        }

        protected static void Trace(string formatString, params object[] paramArray)
        {
            if (p_traceMode)
                _log.Info(String.Format(formatString, paramArray));
        }

        #endregion

        #region ISharedProperty Members

        /// <summary>
        /// Gets or sets the maximum item age.
        /// </summary>
        /// <value>The max. item age.</value>
        string ISharedProperty.maxitemage
        {
            get { return XmlConvert.ToString(MaxItemAge); }
            set { MaxItemAge = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Gets or sets the refresh rate.
        /// </summary>
        /// <value>The refreshrate.</value>
        int ISharedProperty.refreshrate
        {
            get { return RefreshRate; }
            set
            {
                /* ignored at top level */
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [refresh rate is specified].
        /// </summary>
        /// <value><c>true</c> if [refresh rate specified]; otherwise, <c>false</c>.</value>
        bool ISharedProperty.refreshrateSpecified
        {
            get { return true; }
            set
            {
                /* ignored */
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ISharedProperty"/> should download enclosures.
        /// </summary>
        /// <value><c>true</c> if download enclosures; otherwise, <c>false</c>.</value>
        bool ISharedProperty.downloadenclosures
        {
            get { return p_configuration.DownloadEnclosures; }
            set
            {
                /* ignored at top level */
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [download enclosures is specified].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [download enclosures specified]; otherwise, <c>false</c>.
        /// </value>
        bool ISharedProperty.downloadenclosuresSpecified
        {
            get { return true; }
            set
            {
                /* ignore */
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ISharedProperty"/> should alert on enclosure downloads.
        /// </summary>
        /// <value><c>true</c> if enclosurealert; otherwise, <c>false</c>.</value>
        bool ISharedProperty.enclosurealert
        {
            get { return EnclosureAlert; }
            set
            {
                /* ignore at top level */
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enclosure alert specified].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [enclosurealert specified]; otherwise, <c>false</c>.
        /// </value>
        bool ISharedProperty.enclosurealertSpecified
        {
            get { return true; }
            set
            {
                /* ignore */
            }
        }

        /// <summary>
        /// Gets or sets the enclosure folder.
        /// </summary>
        /// <value>The enclosure folder.</value>
        string ISharedProperty.enclosurefolder
        {
            get { return EnclosureFolder; }
            set { EnclosureFolder = value; }
        }

        /// <summary>
        /// Gets or sets the listview layout.
        /// </summary>
        /// <value>The listview layout.</value>
        string ISharedProperty.listviewlayout
        {
            get { return FeedColumnLayout; }
            set { FeedColumnLayout = value; }
        }

        /// <summary>
        /// Gets or sets the stylesheet to render the feed/items.
        /// </summary>
        /// <value>The stylesheet.</value>
        string ISharedProperty.stylesheet
        {
            get { return Stylesheet; }
            set { Stylesheet = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ISharedProperty"/> should mark items read on exit.
        /// </summary>
        /// <value><c>true</c> if markitemsreadonexit; otherwise, <c>false</c>.</value>
        bool ISharedProperty.markitemsreadonexit
        {
            get { return MarkItemsReadOnExit; }
            set { MarkItemsReadOnExit = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [mark items read on exit specified].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [mark items read on exit specified]; otherwise, <c>false</c>.
        /// </value>
        bool ISharedProperty.markitemsreadonexitSpecified
        {
            get { return true; }
            set
            {
                /* ignore */
            }
        }

        #endregion

        /// <summary>
        /// Validates the configuration and throw on errors (required settings).
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        protected static void ValidateAndThrow(INewsComponentsConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            if (string.IsNullOrEmpty(configuration.ApplicationID))
                throw new InvalidOperationException(
                    "INewsComponentsConfiguration.ApplicationID cannot be null or empty.");
            if (configuration.CacheManager == null)
                throw new InvalidOperationException("INewsComponentsConfiguration.CacheManager cannot be null.");
            if (configuration.PersistedSettings == null)
                throw new InvalidOperationException("INewsComponentsConfiguration.PersistedSettings cannot be null.");
            if (string.IsNullOrEmpty(configuration.UserApplicationDataPath))
                throw new InvalidOperationException(
                    "INewsComponentsConfiguration.UserApplicationDataPath cannot be null or empty.");
            if (string.IsNullOrEmpty(configuration.UserLocalApplicationDataPath))
                throw new InvalidOperationException(
                    "INewsComponentsConfiguration.UserLocalApplicationDataPath cannot be null or empty.");
        }

        /// <summary>
        /// Clears all individual max-item-age settings on 
        /// feeds and categories.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ResetAllMaxItemAgeSettings()
        {
            string[] keys;

            // handle feeds:
            lock (feedsTable)
            {
                keys = new string[feedsTable.Count];
                if (feedsTable.Count > 0)
                    feedsTable.Keys.CopyTo(keys, 0);
            }

            for (int i = 0, len = keys.Length; i < len; i++)
            {
                INewsFeed f;
                if (feedsTable.TryGetValue(keys[i], out f))
                {
                    f.maxitemage = null;
                }
            }

            // handle categories:
            //DISCUSS: do we need to lock here? 
            foreach (var c in categories.Values)
            {
                c.maxitemage = null;
            }
        }


        private List<INewsItem> SearchNewsItemsHelper(IEnumerable<INewsItem> prevMatchItems,
                                                      SearchCriteriaCollection criteria, IFeedDetails fi,
                                                      IFeedDetails fiMatchedItems, ref int itemmatches,
                                                      ref int feedmatches, object tag)
        {
            var matchItems = new List<INewsItem>(maxItemsPerSearchResult);
            matchItems.AddRange(prevMatchItems);
            var cancel = false;
            var feedmatch = false;

            foreach (NewsItem item in fi.ItemsList)
            {
                if (criteria.Match(item))
                {
                    //_log.Info("MATCH FOUND: " + item.Title);  
                    feedmatch = true;
                    matchItems.Add(item);
                    fiMatchedItems.ItemsList.Add(item);
                    itemmatches++;
                    if ((itemmatches%50) == 0)
                    {
                        //Caller return results On the last feed we found results 
                        cancel = RaiseNewsItemSearchResultEvent(matchItems, tag);
                        matchItems.Clear();
                    }
                    if (cancel) throw new InvalidOperationException("SEARCH CANCELLED");
                }
            } //foreach(NewsItem...)

            if (feedmatch) feedmatches++;

            return matchItems;
        }

        /// <summary>
        /// Search for NewsItems, that match a provided criteria collection within a optional search scope.
        /// </summary>
        /// <param name="criteria">SearchCriteriaCollection containing the defined search criteria</param>
        /// <param name="scope">Search scope: an array of NewsFeed</param>
        /// <param name="tag">optional object to be used by the caller to identify this search</param>
        /// <param name="cultureName">Name of the culture.</param>
        /// <param name="returnFullItemText">if set to <c>true</c>, full item texts are returned instead of the summery.</param>
        public void SearchNewsItems(SearchCriteriaCollection criteria, INewsFeed[] scope, object tag, string cultureName,
                                    bool returnFullItemText)
        {
            // if scope is an empty array: search all, else search only in spec. feeds
            var feedmatches = 0;
            var itemmatches = 0;

            IList<INewsItem> unreturnedMatchItems = new List<INewsItem>();
            var fiList = new FeedInfoList(String.Empty);

            Exception ex;
            var valid = SearchHandler.ValidateSearchCriteria(criteria, cultureName, out ex);

            if (ex != null) // report always any error (warnings)
            {
                // render the error in-line (search result):
                fiList.Add(CreateHelpNewsItemFromException(ex).FeedDetails);
                feedmatches = fiList.Count;
                unreturnedMatchItems = fiList.GetAllNewsItems();
                itemmatches = unreturnedMatchItems.Count;
            }

            if (valid)
            {
                try
                {
                    // do the search (using lucene):
                    var r = SearchHandler.ExecuteSearch(criteria, scope, new List<FeedSource>
                                                                             {
                                                                                 this
                                                                             }, cultureName);

                    // we iterate r.ItemsMatched to build a
                    // NewsItemIdentifier and ArrayList list with items, that
                    // match the read status (if this was a search criteria)
                    // then call FindNewsItems(NewsItemIdentifier[]) to get also
                    // the FeedInfoList.
                    // Raise ONE event, instead of two to return all (counters, lists)

                    SearchCriteriaProperty criteriaProperty = null;
                    foreach (ISearchCriteria sc in criteria)
                    {
                        criteriaProperty = sc as SearchCriteriaProperty;
                        if (criteriaProperty != null &&
                            PropertyExpressionKind.Unread == criteriaProperty.WhatKind)
                            break;
                    }


                    var readState = ItemReadState.Ignore;
                    if (criteriaProperty != null)
                    {
                        readState = criteriaProperty.BeenRead ? ItemReadState.BeenRead : ItemReadState.Unread;
                    }


                    if (r != null && r.ItemMatchCount > 0)
                    {
                        // append results

                        var nids = new SearchHitNewsItem[r.ItemsMatched.Count];
                        r.ItemsMatched.CopyTo(nids, 0);
                        fiList.AddRange(FindNewsItems(nids, readState, returnFullItemText));
                        feedmatches = fiList.Count;

                        unreturnedMatchItems = fiList.GetAllNewsItems();
                        itemmatches = unreturnedMatchItems.Count;
                    }
                }
                catch (Exception searchEx)
                {
                    // render the error in-line (search result):
                    fiList.Add(CreateHelpNewsItemFromException(searchEx).FeedDetails);
                    feedmatches = fiList.Count;
                    unreturnedMatchItems = fiList.GetAllNewsItems();
                    itemmatches = unreturnedMatchItems.Count;
                }
            }

            RaiseSearchFinishedEvent(tag, fiList, unreturnedMatchItems, feedmatches, itemmatches);
        }

        /// <summary>
        /// Builds a ExceptionalNewsItem from a exception.
        /// This way it can be displayed in-line with a search result or
        /// a normal feed to get the user the hint in the news item list.
        /// to provide help about the error.
        /// </summary>
        /// <param name="e">Exception</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If e is null</exception>
        private static ExceptionalNewsItem CreateHelpNewsItemFromException(Exception e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            var f = new NewsFeed
                        {
                            link = "http://www.rssbandit.org/docs/",
                            title = ComponentsText.ExceptionHelpFeedTitle
                        };

            var newsItem =
                new ExceptionalNewsItem(f, String.Format(ComponentsText.ExceptionHelpFeedItemTitle, e.GetType().Name),
                                        (e.HelpLink ?? "http://www.rssbandit.org/docs/"),
                                        e.Message, e.Source, DateTime.Now.ToUniversalTime(), Guid.NewGuid().ToString())
                    {
                        Subject = e.GetType().Name,
                        CommentStyle = SupportedCommentStyle.None,
                        Enclosures = GetList<IEnclosure>.Empty,
                        WatchComments = false,
                        Language = CultureInfo.CurrentUICulture.Name,
                        HasNewComments = false
                    };

            var fi = new FeedInfo(f.id, f.cacheurl, new List<INewsItem>(new NewsItem[] {newsItem}),
                                  f.title, f.link, ComponentsText.ExceptionHelpFeedDesc,
                                  new Dictionary<XmlQualifiedName, string>(1), newsItem.Language);
            newsItem.FeedDetails = fi;
            return newsItem;
        }

        /// <summary>
        /// Search for NewsItems, that match a provided criteria collection within a optional search scope.
        /// </summary>
        /// <param name="criteria">SearchCriteriaCollection containing the defined search criteria</param>
        /// <param name="scope">Search scope: an array of NewsFeed</param>
        /// <param name="tag">optional object to be used by the caller to identify this search</param>
        public void SearchNewsItems(SearchCriteriaCollection criteria, NewsFeed[] scope, object tag)
        {
            // if scope is an empty array: search all, else search only in spec. feeds
            var feedmatches = 0;
            var itemmatches = 0;
            var feedcounter = 0;

            var unreturnedMatchItems = new List<INewsItem>();
            var fiList = new FeedInfoList(String.Empty);

            try
            {
                FeedInfo[] feedInfos;
                if (scope.Length == 0)
                {
                    // we search a copy of the current content to prevent the lock(itemsTable)
                    // while we do the more time consuming search ops. New received items are
                    // automatically recognized to be searched as they are float into the system.
                    lock (itemsTable)
                    {
                        feedInfos = new FeedInfo[itemsTable.Count];
                        itemsTable.Values.CopyTo(feedInfos, 0);
                    }
                    foreach (var fi in feedInfos)
                    {
                        var fiClone = fi.Clone(false);
                        //fiClone.ItemsList.Clear(); 

                        unreturnedMatchItems =
                            SearchNewsItemsHelper(unreturnedMatchItems, criteria, fi, fiClone, ref itemmatches,
                                                  ref feedmatches, tag);
                        feedcounter++;

                        if ((feedcounter%5) == 0)
                        {
                            // to shorten search if user want to cancel. Above modulo will only stop if it founds at least 100 matches...
                            var cancel = RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);
                            unreturnedMatchItems.Clear();
                            if (cancel)
                                break;
                        }

                        if (fiClone.ItemsList.Count != 0)
                        {
                            fiList.Add(fiClone);
                        }
                    } //foreach(FeedInfo...)
                }
                else
                {
                    lock (itemsTable)
                    {
                        feedInfos = new FeedInfo[scope.Length];
                        for (var i = 0; i < scope.Length; i++)
                        {
                            feedInfos[i] = (FeedInfo) itemsTable[scope[i].link];
                        }
                    }

                    foreach (var fi in feedInfos)
                    {
                        if (fi != null)
                        {
                            var fiClone = fi.Clone(false);
                            //fiClone.ItemsList.Clear();

                            unreturnedMatchItems =
                                SearchNewsItemsHelper(unreturnedMatchItems, criteria, fi, fiClone, ref itemmatches,
                                                      ref feedmatches, tag);
                            feedcounter++;

                            if ((feedcounter%5) == 0)
                            {
                                // to shorten search if user want to cancel. Above modulo will only stop if it founds at least 100 matches...
                                var cancel = RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);
                                unreturnedMatchItems.Clear();
                                if (cancel)
                                    break;
                            }

                            if (fiClone.ItemsList.Count != 0)
                            {
                                fiList.Add(fiClone);
                            }
                        }
                    }
                }

                if (unreturnedMatchItems.Count > 0)
                {
                    RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);
                }
            }
            catch (InvalidOperationException ioe)
            {
// New feeds added to FeedsTable from another thread  
                Trace("SearchNewsItems() casued InvalidOperationException: {0}", ioe);
            }

            RaiseSearchFinishedEvent(tag, fiList, feedmatches, itemmatches);
        }

        /// <summary>
        /// Initiate a remote (web) search using the engine incl. search expression specified
        /// by searchFeedUrl. We assume, the specified Url will return a RSS feed.
        /// This can be used e.g. to get a RSS search result from feedster.
        /// </summary>
        /// <param name="searchFeedUrl">Complete Url of the search engine incl. search expression</param>
        /// <param name="tag">optional, can be used by the caller</param>
        public void SearchRemoteFeed(string searchFeedUrl, object tag)
        {
            var unreturnedMatchItems = RssParser.DownloadItemsFromFeed(searchFeedUrl);
            RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);

            var feedmatches = 1;
            var itemmatches = unreturnedMatchItems.Count;
            var fi =
                new FeedInfo(String.Empty, String.Empty, unreturnedMatchItems, String.Empty, String.Empty, String.Empty,
                             new Dictionary<XmlQualifiedName, string>(), String.Empty);
            var fil = new FeedInfoList(String.Empty)
                          {
                              fi
                          };
            RaiseSearchFinishedEvent(tag, fil, feedmatches, itemmatches);
        }

        /// <summary>
        /// [To be provided]
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="scope"></param>
        /// <param name="tag"></param>
        public void SearchFeeds(SearchCriteriaCollection criteria, NewsFeed[] scope, object tag)
        {
            // if scope is an empty array: search all, else search only in spec. feeds
            // pseudo code:
            /* int matches = 0;
			foreach (NewsFeed f in feedsTable) {
				if (criteria.Match(f)) {
					matches++;
					if (RaiseFeedSearchResultEvent(f, tag))
					  break;
				}
			}
			RaiseSearchFinishedEvent(tag, matches, 0); */

            throw new NotSupportedException();
        }

        private bool RaiseNewsItemSearchResultEvent(IEnumerable<INewsItem> matchItems, object tag)
        {
            try
            {
                if (NewsItemSearchResult != null)
                {
                    var ea =
                        new NewsItemSearchResultEventArgs(new List<INewsItem>(matchItems), tag, false);
                    NewsItemSearchResult(this, ea);
                    return ea.Cancel;
                }
            }
            catch
            {
            }
            return false;
        }

        private void RaiseSearchFinishedEvent(object tag, FeedInfoList matchingFeeds, int matchingFeedsCount,
                                              int matchingItemsCount)
        {
            try
            {
                if (SearchFinished != null)
                {
                    SearchFinished(this,
                                   new SearchFinishedEventArgs(tag, matchingFeeds, matchingFeedsCount,
                                                               matchingItemsCount));
                }
            }
            catch (Exception e)
            {
                Trace("SearchFinished() event code raises exception: {0}", e);
            }
        }

        private void RaiseSearchFinishedEvent(object tag, FeedInfoList matchingFeeds,
                                              IEnumerable<INewsItem> matchingItems,
                                              int matchingFeedsCount, int matchingItemsCount)
        {
            try
            {
                if (SearchFinished != null)
                {
                    SearchFinished(this,
                                   new SearchFinishedEventArgs(tag, matchingFeeds, matchingItems, matchingFeedsCount,
                                                               matchingItemsCount));
                }
            }
            catch (Exception e)
            {
                Trace("SearchFinished() event code raises exception: {0}", e);
            }
        }

        /// <summary>
        /// Retrieves a specified NewsItem given the identifying feed URL and Item ID
        /// </summary>
        /// <param name="nid">The value used to identify the NewsItem</param>
        /// <returns>The NewsItem or null if it could not be found</returns>
        public INewsItem FindNewsItem(SearchHitNewsItem nid)
        {
            if (nid != null)
            {
                var fi = itemsTable[nid.FeedLink] as FeedInfo;

                if (fi != null)
                {
                    var items = new List<INewsItem>(fi.ItemsList);

                    foreach (var ni in items)
                    {
                        if (ni.Id.Equals(nid.Id))
                        {
                            return ni;
                        }
                    } //foreach
                } //if(fi != null)
            } //if(nid != null)

            return null;
        }


        /// <summary>
        /// Retrieves a list of NewsItems and their FeedInfo objects
        /// not regarding their read states.
        /// </summary>
        /// <param name="nids">The values used to identify the NewsItems</param>
        /// <returns>The list of FeedInfo objects containing the NewsItems (content summaries)</returns>
        public FeedInfoList FindNewsItems(SearchHitNewsItem[] nids)
        {
            return FindNewsItems(nids, ItemReadState.Ignore, false);
        }


        /// <summary>
        /// Retrieves a list of NewsItems and their FeedInfo objects
        /// </summary>
        /// <param name="nids">The values used to identify the NewsItems</param>
        /// <param name="readState">Indicates how to interpret read state of NewsItems to return</param>
        /// <param name="returnFullItemText">if set to <c>true</c> we load/return full item texts.</param>
        /// <returns>
        /// The list of FeedInfo objects containing the NewsItems
        /// </returns>
        public FeedInfoList FindNewsItems(SearchHitNewsItem[] nids, ItemReadState readState, bool returnFullItemText)
        {
            var fiList = new FeedInfoList(String.Empty);
            var matchedFeeds = new Dictionary<string, FeedInfo>();
            var itemlists = new Dictionary<string, List<INewsItem>>();

            foreach (var nid in nids)
            {
                IFeedDetails fdi;
                FeedInfo fi, originalfi = null; // this.itemsTable[nid.FeedLink] as FeedInfo; 
                if (itemsTable.TryGetValue(nid.FeedLink, out fdi))
                    originalfi = fdi as FeedInfo;

                if (originalfi != null)
                {
                    List<INewsItem> items;
                    if (matchedFeeds.ContainsKey(nid.FeedLink))
                    {
                        fi = matchedFeeds[nid.FeedLink];
                        items = itemlists[nid.FeedLink];
                    }
                    else
                    {
                        fi = originalfi.Clone(false);
                        items = new List<INewsItem>(originalfi.ItemsList);
                        matchedFeeds.Add(nid.FeedLink, fi);
                        itemlists.Add(nid.FeedLink, items);
                    }

                    var beenRead = (readState == ItemReadState.BeenRead);
                    foreach (NewsItem ni in items)
                    {
                        if (ni.Id.Equals(nid.Id))
                        {
                            if (readState == ItemReadState.Ignore ||
                                ni.BeenRead == beenRead)
                            {
                                nid.BeenRead = ni.BeenRead; //copy over read state
                                if (returnFullItemText && !nid.HasContent)
                                    GetCachedContentForItem(nid);
                                fi.ItemsList.Add(nid);
                                nid.FeedDetails = fi;
                            }
                            break;
                        }
                    } //foreach
                } //if(fi != null)
            }

            foreach (var f in matchedFeeds.Values)
            {
                //Ensure that we actually matched items from the feed before adding it. 
                //This can happen if search index has items that are no longer in RSS 
                //feed cache. 
                if (f.ItemsList.Count > 0)
                {
                    fiList.Add(f);
                }
            }

            return fiList;
        }


        /// <summary>
        /// Resets all refresh rate settings at feeds and categories.
        /// </summary>
        public void ResetAllRefreshRateSettings()
        {
            string[] keys;

            lock (feedsTable)
            {
                keys = new string[feedsTable.Count];
                if (feedsTable.Count > 0)
                    feedsTable.Keys.CopyTo(keys, 0);
            }

            for (int i = 0, len = keys.Length; i < len; i++)
            {
                INewsFeed f;
                if (feedsTable.TryGetValue(keys[i], out f))
                {
                    f.refreshrate = 0;
                    f.refreshrateSpecified = false;
                }
            }
            // handle categories:
            //DISCUSS: do we need to lock here? 
            foreach (var c in categories.Values)
            {
                c.refreshrate = 0;
                c.refreshrateSpecified = false;
            }
        }


        /// <summary>
        /// Helper method which retrieves the list of Keys in the FeedsTable object using the CopyTo method. 
        /// </summary>
        /// <returns>An array containing the "keys" of the FeedsTable</returns>
        protected string[] GetFeedsTableKeys()
        {
            string[] keys;

            lock (feedsTable)
            {
                keys = new string[feedsTable.Count];
                if (feedsTable.Count > 0)
                    feedsTable.Keys.CopyTo(keys, 0);
            }

            return keys;
        }


        /// <summary>
        /// Retrieves the stories with the most weighted links for a givern date range. 
        /// </summary>
        /// <param name="since">The start of the date range </param>
        /// <param name="numStories">The number of stories to return</param>
        /// <remarks>The score of the story is adjusted in a weighted manner so that 
        /// more recent posts are weighted higher than older posts. So a newly popular 
        /// item with 3 or 4 links posted yesterday ends up ranking higher than an 
        /// item with 6 to 10 posts about it from five days ago.
        /// </remarks>
        /// <returns>A sorted list (descending order) of RelationHrefEntry objects that 
        /// correspond to the most popular item from the date range starting with the 
        /// since parameter and ending with today.</returns>
        public IList<RelationHRefEntry> GetTopStories(TimeSpan since, int numStories)
        {
            var keys = GetFeedsTableKeys();
            var allLinks =
                new Dictionary<RelationHRefEntry, List<RankedNewsItem>>();

            for (var i = 0; i < keys.Length; i++)
            {
                if (!itemsTable.ContainsKey(keys[i]))
                {
                    continue;
                }

                var fi = (FeedInfo) itemsTable[keys[i]];

                //get all news items that fall within the date range
                var items =
                    fi.ItemsList.FindAll(item => (DateTime.Now - item.Date) < since);

                foreach (var item in items)
                {
                    //create score and ranked news item that represents a weighted link to a URL
                    var score = 1.0f - (DateTime.Now.Ticks - item.Date.Ticks)*1.0f/since.Ticks;
                    var rni = new RankedNewsItem(item, score);

                    /* 
                    //add a score for the permalink for the item 
                    //DON'T DO THIS BECAUSE WE HAVE TO THEN FILTER OUT ITEMS THAT ONLY HAVE THEMSELVES AS VOTES
                    if (!allLinks.ContainsKey(href)) {
                        allLinks[href] = new List<RankedNewsItem>(); 
                    }
                    allLinks[href].Add(rni);
                     */

                    //add vote to each URL linked from the item
                    foreach (var url in item.OutGoingLinks)
                    {
                        var href = new RelationHRefEntry(url, null, 0.0f);
                        if (!allLinks.ContainsKey(href))
                        {
                            allLinks[href] = new List<RankedNewsItem>();
                        }
                        allLinks[href].Add(rni);
                    }
                } //foreach(NewsItem item in items){
            } //for(int i; i < keys.Length; i++){

            //tally the votes, only 1 vote counts per feed
            var weightedLinks = new List<RelationHRefEntry>();

            foreach (var linkNvotes in allLinks)
            {
                var votesPerFeed = new Dictionary<string, float>();

                //pick the lower vote if multiple links from a particular feed
                foreach (var voteItem in linkNvotes.Value)
                {
                    var feedLink = voteItem.Item.FeedLink;

                    if (votesPerFeed.ContainsKey(feedLink))
                    {
                        votesPerFeed[feedLink] = Math.Min(votesPerFeed[feedLink], voteItem.Score);
                    }
                    else
                    {
                        votesPerFeed.Add(feedLink, voteItem.Score);
                        linkNvotes.Key.References.Add(voteItem.Item);
                    }
                }
                var totalScore = 0.0f;

                foreach (var value in votesPerFeed.Values)
                {
                    totalScore += value;
                }
                linkNvotes.Key.Score = totalScore;
                weightedLinks.Add(linkNvotes.Key);
            }

            weightedLinks.Sort((x, y) => y.Score.CompareTo(x.Score));
            weightedLinks = weightedLinks.GetRange(0, Math.Min(numStories, weightedLinks.Count));

            //fetch titles from HTML page

            // The number of HTML titles left to download by the anon. threaded delegate 	
            var numTitlesToDownload = Math.Min(numStories, weightedLinks.Count);
            //in number of weighted links less than numStories 

            var eventX = new ManualResetEvent(false);
            try
            {
                foreach (var rhf in weightedLinks)
                {
                    if (TopStoryTitles.ContainsKey(rhf.HRef))
                    {
                        rhf.Text = TopStoryTitles[rhf.HRef].storyTitle;
                        Interlocked.Decrement(ref numTitlesToDownload);
                    }
                    else
                    {
                        var weightedLink = rhf;

                        PriorityThreadPool.QueueUserWorkItem(
                            delegate
                                {
                                    try
                                    {
                                        /* NOTE: Default link text is URL */
                                        var title =
                                            HtmlHelper.FindTitle(weightedLink.HRef, weightedLink.HRef, Proxy,
                                                                 CredentialCache.DefaultCredentials);
                                        weightedLink.Text = title;
                                        if (!title.Equals(weightedLink.HRef) &&
                                            !TopStoryTitles.ContainsKey(weightedLink.HRef))
                                        {
                                            TopStoryTitles.Add(weightedLink.HRef, new storyNdate(title, DateTime.Now));
                                            topStoriesModified = true;
                                        }
                                    }
                                    finally
                                    {
                                        Interlocked.Decrement(ref numTitlesToDownload);
                                        if (numTitlesToDownload <= 0)
                                        {
                                            if (eventX != null)
                                                eventX.Set();
                                        }
                                    }
                                },
                            weightedLink,
                            (int) ThreadPriority.Normal);
                    }
                }

                if (numTitlesToDownload > 0)
                {
                    eventX.WaitOne(Timeout.Infinite, true);
                }
                return weightedLinks;
            }
            finally
            {
                IDisposable disposable = eventX;
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Retrieves all non-internet feed URLs (e.g. intranet and local feeds)
        /// </summary>
        /// <returns>A feeds table with the non-internet feeds</returns>
        public IEnumerable<NewsFeed> GetNonInternetFeeds()
        {
            var toReturn = new List<NewsFeed>();

            if (feedsTable.Count == 0)
                return toReturn;

            var keys = new string[feedsTable.Keys.Count];
            feedsTable.Keys.CopyTo(keys, 0);

            foreach (var url in keys)
            {
                try
                {
                    var uri = new Uri(url);
                    if (uri.IsFile || uri.IsUnc || !uri.Authority.Contains("."))
                    {
                        INewsFeed f;
                        if (feedsTable.TryGetValue(url, out f))
                        {
                            toReturn.Add(new NewsFeed(f));
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error("Exception in GetNonInternetFeeds()", e);
                }
            }

            return toReturn;
        }


        /// <summary>
        /// Loads the cache of {url:page_title} pairs so we don't have to go to the Web if we've previously 
        /// determined the title of a top story. 
        /// </summary>
        /// <seealso cref="TopStoryTitles"/>
        private void LoadCachedTopStoryTitles()
        {
            try
            {
                var topStories = Path.Combine(Configuration.UserApplicationDataPath, "top-stories.xml");
                if (File.Exists(topStories))
                {
					XmlDocument doc = new XmlDocument();
                    doc.Load(topStories);

                    foreach (XmlElement story in doc.SelectNodes("//story"))
                    {
                        TopStoryTitles.Add(story.Attributes["url"].Value,
                                           new storyNdate(story.Attributes["title"].Value,
                                                          XmlConvert.ToDateTime(story.Attributes["firstSeen"].Value,
                                                                                XmlDateTimeSerializationMode.Utc))
                            );
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Error in LoadCachedTopStoryTitles()", e);
            }
        }

        /// <summary>
        /// Saves the cached list of titles for top stories. 
        /// </summary>
        /// <seealso cref="TopStoryTitles"/>
        public static void SaveCachedTopStoryTitles()
        {
            var TwoWeeksAgo = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));
            topStoriesModified = false;

            try
            {
				XmlWriter writer =
                    XmlWriter.Create(Path.Combine(DefaultConfiguration.UserApplicationDataPath, "top-stories.xml"));
                writer.WriteStartDocument();
                writer.WriteStartElement("stories");
                foreach (var story in TopStoryTitles)
                {
                    if (story.Value.firstSeen > TwoWeeksAgo)
                    {
                        //filter out top stories older than two weeks
                        writer.WriteStartElement("story");
                        writer.WriteAttributeString("url", story.Key);
                        writer.WriteAttributeString("title", story.Value.storyTitle);
                        writer.WriteAttributeString("firstSeen",
                                                    XmlConvert.ToString(story.Value.firstSeen,
                                                                        XmlDateTimeSerializationMode.Utc));
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                _log.Error("Error in SaveCachedTopStoryTitles()", e);
            }
        }

        /// <summary>
        /// Specifies that a feed should be ignored when RefreshFeeds() is called by 
        /// setting its refresh rate to zero. The feed can still be refreshed manually by 
        /// calling GetItemsForFeed(). 
        /// </summary>
        /// <remarks>If no feed with that URL exists then nothing is done.</remarks>
        /// <param name="feedUrl">The URL of the feed to ignore. </param>
        public void DisableFeed(string feedUrl)
        {
            if (!feedsTable.ContainsKey(feedUrl))
            {
                return;
            }

            var f = feedsTable[feedUrl];
            f.refreshrate = 0;
            f.refreshrateSpecified = true;
        }


        /// <summary>
        /// Removes all information related to an item from the FeedSource. 
        /// </summary>
        /// <remarks>If the item doesn't exist in the FeedSource then nothing is done</remarks>
        /// <param name="item">the item to delete</param>
        public virtual void DeleteItem(INewsItem item)
        {
            if (item.Feed != null && !string.IsNullOrEmpty(item.Feed.link))
            {
                /* 
				 * There is no attempt to load feed from disk because it is 
				 * assumed that for this to be called the feed was already loaded
				 * since we have an item from the feed */

                var fi = itemsTable[item.Feed.link] as FeedInfo;

                if (fi != null)
                {
                    lock (fi.itemsList)
                    {
                        item.Feed.AddDeletedStory(item.Id);
                        fi.itemsList.Remove(item);
                    }
                } //if(fi != null)
            } //if(item.Feed != null) 
        }

        /// <summary>
        /// Deletes all the items in a feed
        /// </summary>
        /// <param name="feed">the feed</param>
        public void DeleteAllItemsInFeed(INewsFeed feed)
        {
            if (feed != null && !string.IsNullOrEmpty(feed.link) && feedsTable.ContainsKey(feed.link))
            {
                var fi = itemsTable[feed.link] as FeedInfo;

                //load feed from disk 
                if (fi == null)
                {
                    fi = (FeedInfo) GetFeed(feed);
                }

                if (fi != null)
                {
                    lock (fi.itemsList)
                    {
                        foreach (var item in fi.itemsList)
                        {
                            feed.AddDeletedStory(item.Id);
                        }
                        fi.itemsList.Clear();
                    }
                } //if(fi != null)		

                SearchHandler.IndexRemove(feed.id);
            } //if (feed != null && !string.IsNullOrEmpty( feed.link ) && feedsTable.ContainsKey(feed.link)) {
        }

        /// <summary>
        /// Deletes all items in a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        public void DeleteAllItemsInFeed(string feedUrl)
        {
            if (feedsTable.ContainsKey(feedUrl))
            {
                DeleteAllItemsInFeed(feedsTable[feedUrl]);
            }
        }

        /// <summary>
        /// Undeletes a deleted item
        /// </summary>
        /// <remarks>if the parent feed has been deleted then this does nothing</remarks>
        /// <param name="item">the utem to restore</param>
        public void RestoreDeletedItem(INewsItem item)
        {
            if (item.Feed != null && !string.IsNullOrEmpty(item.Feed.link) && feedsTable.ContainsKey(item.Feed.link))
            {
                var fi = itemsTable[item.Feed.link] as FeedInfo;

                //load feed from disk 
                if (fi == null)
                {
                    fi = (FeedInfo) GetFeed(item.Feed);
                }

                if (fi != null)
                {
                    lock (fi.itemsList)
                    {
                        item.Feed.RemoveDeletedStory(item.Id);
                        fi.itemsList.Add(item);
                    }
                } //if(fi != null)

                SearchHandler.IndexAdd(item);
            } //if(item.Feed != null) 
        }

        /// <summary>
        /// Undeletes all the deleted items in the list
        /// </summary>
        /// <remarks>if the parent feed has been deleted then this does nothing</remarks>
        /// <param name="deletedItems">the list of items to restore</param>
        public void RestoreDeletedItem(IList<INewsItem> deletedItems)
        {
            foreach (var item in deletedItems)
            {
                RestoreDeletedItem(item);
            }

            SearchHandler.IndexAdd(deletedItems);
        }


        /// <summary>
        /// Saves the feed list to the specified stream. The feed is written in 
        /// the RSS Bandit feed file format as described in feeds.xsd
        /// </summary>
        /// <param name="feedStream">The stream to save the feed list to</param>
        public virtual void SaveFeedList(Stream feedStream)
        {
            SaveFeedList(feedStream, FeedListFormat.NewsHandler);
        }


        /// <summary>
        /// Helper method used for constructing OPML file. It traverses down the tree on the 
        /// path defined by 'category' starting with 'startNode'. 
        /// </summary>
        /// <param name="startNode">Node to start with</param>
        /// <param name="category">A category path, e.g. 'Category1\SubCategory1'.</param>
        /// <returns>The leaf category node.</returns>
        /// <remarks>If one category in the path is not found, it will be created.</remarks>
        private static XmlElement CreateCategoryHive(XmlElement startNode, string category)
        {
            if (string.IsNullOrEmpty(category) || startNode == null) return startNode;

            var catHives = category.Split(CategorySeparator.ToCharArray());
            XmlElement n;
            var wasNew = false;

            foreach (var catHive in catHives)
            {
                if (!wasNew)
                {
                    var xpath = "child::outline[@title=" + buildXPathString(catHive) + " and (count(@*)= 1)]";
                    n = (XmlElement) startNode.SelectSingleNode(xpath);
                }
                else
                {
                    n = null;
                }

                if (n == null)
                {
                    n = startNode.OwnerDocument.CreateElement("outline");
                    n.SetAttribute("title", catHive);
                    startNode.AppendChild(n);
                    wasNew = true; // shorten search
                }

                startNode = n;
            } //foreach

            return startNode;
        }


        /// <summary>
        /// Helper function that gets the listview layout with the specified ID from the
        /// Arraylist
        /// </summary>
        /// <param name="id"></param>
        /// <param name="layouts"></param>
        /// <returns></returns>
        private static listviewLayout FindLayout(IEquatable<string> id, IEnumerable<listviewLayout> layouts)
        {
            foreach (var layout in layouts)
            {
                if (id.Equals(layout.ID))
                    return layout;
            }
            return null;
        }

        /// <summary>
        /// Helper function breaks up a string containing quote characters into 
        ///	a series of XPath concat() calls. 
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>broken up string</returns>
        public static string buildXPathString(string input)
        {
            var components = input.Split(new[] {'\''});
            var result = "";
            result += "concat(''";
            for (var i = 0; i < components.Length; i++)
            {
                result += ", '" + components[i] + "'";
                if (i < components.Length - 1)
                {
                    result += ", \"'\"";
                }
            }
            result += ")";
            Console.WriteLine(result);
            return result;
        }

        /// <summary>
        /// Saves the whole feed list incl. empty categories to the specified stream
        /// </summary>
        /// <param name="feedStream">The feedStream to save the feed list to</param>
        /// <param name="format">The format to save the stream as. </param>
        /// <exception cref="InvalidOperationException">If anything wrong goes on with XmlSerializer</exception>
        /// <exception cref="ArgumentNullException">If feedStream is null</exception>
        public virtual void SaveFeedList(Stream feedStream, FeedListFormat format)
        {
            SaveFeedList(feedStream, format, feedsTable, true);
        }

        /// <summary>
        /// Saves the provided feed list to the specified stream
        /// </summary>
        /// <param name="feedStream">The feedStream to save the feed list to</param>
        /// <param name="format">The format to save the stream as. </param>
        /// <param name="feeds">FeedsCollection containing the feeds to save. 
        /// Can contain a subset of the owned feeds collection</param>
        /// <param name="includeEmptyCategories">Set to true, if categories without a contained feed should be included</param>
        /// <exception cref="InvalidOperationException">If anything wrong goes on with XmlSerializer</exception>
        /// <exception cref="ArgumentNullException">If feedStream is null</exception>
        public virtual void SaveFeedList(Stream feedStream, FeedListFormat format, IDictionary<string, INewsFeed> feeds,
                                         bool includeEmptyCategories)
        {
            if (feedStream == null)
                throw new ArgumentNullException("feedStream");

            if (format.Equals(FeedListFormat.OPML))
            {
                var opmlDoc = new XmlDocument();
                opmlDoc.LoadXml("<opml version='1.0'><head /><body /></opml>");

                var categoryTable = new Dictionary<string, XmlElement>(categories.Count);

                foreach (NewsFeed f in feeds.Values)
                {
                    var outline = opmlDoc.CreateElement("outline");
                    outline.SetAttribute("title", f.title);
                    outline.SetAttribute("xmlUrl", f.link);
                    outline.SetAttribute("type", "rss");
                    outline.SetAttribute("text", f.title);

                    IFeedDetails fi;
                    var success = itemsTable.TryGetValue(f.link, out fi);

                    if (success)
                    {
                        outline.SetAttribute("htmlUrl", fi.Link);
                        outline.SetAttribute("description", fi.Description);
                    }

                    var category = (f.category ?? String.Empty);

                    XmlElement catnode;
                    if (categoryTable.ContainsKey(category))
                        catnode = categoryTable[category];
                    else
                    {
                        catnode = CreateCategoryHive((XmlElement) opmlDoc.DocumentElement.ChildNodes[1], category);
                        categoryTable.Add(category, catnode);
                    }

                    catnode.AppendChild(outline);
                }

                if (includeEmptyCategories)
                {
                    //add categories, we don't already have
                    foreach (var category in categories.Keys)
                    {
                        CreateCategoryHive((XmlElement) opmlDoc.DocumentElement.ChildNodes[1], category);
                    }
                }

                var opmlWriter = new XmlTextWriter(feedStream, Encoding.UTF8);
                opmlWriter.Formatting = Formatting.Indented;
                opmlDoc.Save(opmlWriter);
            }
            else if (format.Equals(FeedListFormat.NewsHandler) || format.Equals(FeedListFormat.NewsHandlerLite))
            {
                var serializer = XmlHelper.SerializerCache.GetSerializer(typeof (feeds));
                var feedlist = new feeds();

                if (feeds != null)
                {
                    /* 
					 * not anymore required to store that in feedlist.
					 * now provided via config/static:
					 * 
                    feedlist.refreshrate = this.refreshrate;
                    feedlist.refreshrateSpecified = true;
                    feedlist.downloadenclosures = this.downloadenclosures;
                    feedlist.downloadenclosuresSpecified = true;
                    feedlist.enclosurealert = this.enclosurealert;
                    feedlist.enclosurealertSpecified = true;

                    feedlist.createsubfoldersforenclosures = this.createsubfoldersforenclosures;
                    feedlist.createsubfoldersforenclosuresSpecified = true;

                    feedlist.numtodownloadonnewfeed = this.numtodownloadonnewfeed;
                    feedlist.numtodownloadonnewfeedSpecified = true;

                    feedlist.enclosurecachesize = this.enclosurecachesize;
                    feedlist.enclosurecachesizeSpecified = true;
*/
                    //feedlist.maxitemage = XmlConvert.ToString(this.maxitemage);
                    feedlist.listviewlayout = FeedColumnLayout;
                    /* not anymore required to store that in feedlist/now provided static:
                    feedlist.stylesheet = this.stylesheet;
					feedlist.enclosurefolder = this.EnclosureFolder;
					 */
                    feedlist.podcastfolder = PodcastFolder;
                    feedlist.podcastfileexts = PodcastFileExtensionsAsString;
                    /* not anymore required to store that in feedlist/now provided static:
                    feedlist.markitemsreadonexit = this.markitemsreadonexit;
                    feedlist.markitemsreadonexitSpecified = true;
					*/

                    // refactored props that do not need anymore stored in feedlist:
                    feedlist.markitemsreadonexitSpecified = false;
                    feedlist.downloadenclosuresSpecified = false;
                    feedlist.enclosurealertSpecified = false;
                    feedlist.refreshrateSpecified = false;
                    feedlist.createsubfoldersforenclosuresSpecified = false;
                    feedlist.numtodownloadonnewfeedSpecified = false;
                    feedlist.enclosurecachesizeSpecified = false;

                    foreach (var f in feeds.Values)
                    {
                        if (f is NewsFeed)
                            feedlist.feed.Add((NewsFeed) f);
                        else
                            feedlist.feed.Add(new NewsFeed(f));

                        if (itemsTable.ContainsKey(f.link))
                        {
                            IList<INewsItem> items = itemsTable[f.link].ItemsList;

                            // Taken out because it meant that when we sync we lose information
                            // about stuff we've read from other instances of RSS Bandit synced from 
                            // if its cache is older than this one. 
                            /* f.storiesrecentlyviewed.Clear(); */


                            if (!format.Equals(FeedListFormat.NewsHandlerLite))
                            {
                                foreach (var ri in items)
                                {
                                    if (ri.BeenRead && !f.storiesrecentlyviewed.Contains(ri.Id))
                                    {
                                        //THIS MAY BE SLOW
                                        f.AddViewedStory(ri.Id);
                                    }
                                }
                            } //foreach
                        } //if
                    } //foreach
                } //if(feeds != null) 


                var c = new List<category>(categories.Count);
                /* sometimes we get nulls in the arraylist */
                foreach (var cat in categories.Values)
                {
                    if (!StringHelper.EmptyTrimOrNull(cat.Value))
                    {
                        c.Add(new category(cat));
                    }
                }

                //we don't want to write out empty <categories /> into the schema. 				
                feedlist.categories = c.Count == 0 ? null : c;


                var lvl = new List<listviewLayout>(layouts.Count);
                /* sometimes we get nulls in the arraylist, remove them */
                for (var i = 0; i < layouts.Count; i++)
                {
                    var s = layouts[i];
                    if (s.Value == null)
                    {
                        layouts.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        lvl.Add(new listviewLayout(s.Key, s.Value));
                    }
                }

                //we don't want to write out empty <listview-layouts /> into the schema. 				
                feedlist.listviewLayouts = lvl.Count == 0 ? null : lvl;


                var nntps = new List<NntpServerDefinition>(nntpServers.Values.Count);
                foreach (var val in nntpServers.Values)
                    nntps.Add((NntpServerDefinition) val);

                //we don't want to write out empty <nntp-servers /> into the schema. 				
                feedlist.nntpservers = nntps.Count == 0 ? null : nntps;

                var ids = new List<UserIdentity>(identities.Values);

                //we don't want to write out empty <user-identities /> into the schema. 				
                feedlist.identities = ids.Count == 0 ? null : ids;


                TextWriter writer = new StreamWriter(feedStream);
                serializer.Serialize(writer, feedlist);
                //writer.Close(); DON'T CLOSE STREAM
            }
        }


        /// <summary>
        /// Used to clear the information about when last the feed was downloaded. This allows
        /// us to refetch the feed without sending If-Modified-Since or If-None-Match header
        /// information and thus force a download. 
        /// </summary>
        /// <param name="f">The feed to mark for download</param>
        public void MarkForDownload(INewsFeed f)
        {
            f.etag = null;
            f.lastretrievedSpecified = false;
            f.lastretrieved = DateTime.MinValue;
            f.lastmodified = DateTime.MinValue;
        }


        /// <summary>
        /// Used to clear the information about when last the feeds downloaded. This allows
        /// us to refetch the feed without sending If-Modified-Since or If-None-Match header
        /// information and thus force a download. 
        /// </summary>		
        public void MarkForDownload()
        {
            if (FeedsListOK)
            {
                foreach (NewsFeed f in feedsTable.Values)
                {
                    MarkForDownload(f);
                }
            }
        }

        /// <summary>
        /// Removes all the RSS items cached in-memory and on-disk for all feeds. 
        /// </summary>
        public void ClearItemsCache()
        {
            itemsTable.Clear();
            CacheHandler.ClearCache();
        }


        /// <summary>
        /// Marks all items stored in the internal cache of RSS items as read.
        /// </summary>
        public void MarkAllCachedItemsAsRead()
        {
            foreach (var f in feedsTable.Values)
            {
                MarkAllCachedItemsAsRead(f);
            }
        }


        /// <summary>
        /// Marks all items stored in the internal cache of RSS items as read
        /// for a particular category.
        /// </summary>
        /// <param name="category">The category the feeds belong to</param>
        public void MarkAllCachedCategoryItemsAsRead(string category)
        {
            if (FeedsListOK)
            {
                if (categories.ContainsKey(category))
                {
                    foreach (NewsFeed f in feedsTable.Values)
                    {
                        if ((f.category != null) && f.category.Equals(category))
                        {
                            MarkAllCachedItemsAsRead(f);
                        }
                    }
                }
                else if (category == null /* the default category */)
                {
                    foreach (NewsFeed f in feedsTable.Values)
                    {
                        if (f.category == null)
                        {
                            MarkAllCachedItemsAsRead(f);
                        }
                    }
                }
            } //if(FeedsListOK)
        }

        /// <summary>
        /// Marks all items stored in the internal cache of RSS items as read
        /// for a particular feed.
        /// </summary>
        /// <param name="feedUrl">The URL of the RSS feed</param>
        public virtual void MarkAllCachedItemsAsRead(string feedUrl)
        {
            if (!string.IsNullOrEmpty(feedUrl))
            {
                INewsFeed feed;
                if (feedsTable.TryGetValue(feedUrl, out feed))
                {
                    MarkAllCachedItemsAsRead(feed);
                }
            }
        }

        /// <summary>
        /// Marks all items stored in the internal cache of RSS items as read
        /// for a particular feed.
        /// </summary>
        /// <param name="feed">The RSS feed</param>
        public virtual void MarkAllCachedItemsAsRead(INewsFeed feed)
        {
            if (feed != null && !string.IsNullOrEmpty(feed.link) && itemsTable.ContainsKey(feed.link))
            {
                var fi = itemsTable[feed.link];

                if (fi != null)
                {
                    foreach (var ri in fi.ItemsList)
                    {
                        ri.BeenRead = true;
                    }
                }

                feed.containsNewMessages = false;
            }
        }

        /// <summary>
        /// Determines whether the changed specified properties 
        /// are cache relevant changes (feed cache file have to be (re-)written.
        /// </summary>
        /// <param name="changedProperty">The changed property or properties.</param>
        /// <returns>
        /// 	<c>true</c> if it is a cache relevant change; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCacheRelevantChange(NewsFeedProperty changedProperty)
        {
            return (cacheRelevantPropertyChanges & changedProperty) != NewsFeedProperty.None;
        }

        /// <summary>
        /// Determines whether the changed specified properties 
        /// are subscription relevant changes (subscription file have to be (re-)written.
        /// </summary>
        /// <param name="changedProperty">The changed property or properties.</param>
        /// <returns>
        /// 	<c>true</c> if it is a subscription relevant change; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSubscriptionRelevantChange(NewsFeedProperty changedProperty)
        {
            return (subscriptionRelevantPropertyChanges & changedProperty) != NewsFeedProperty.None;
        }

        /// <summary>
        /// Do apply any internal work needed after some feed or feed item properties 
        /// or content was changed outside.
        /// </summary>
        /// <param name="feedUrl">The feed to update</param>
        /// <exception cref="ArgumentNullException">If feedUrl is null or empty</exception>
        public void ApplyFeedModifications(string feedUrl)
        {
            if (string.IsNullOrEmpty(feedUrl))
                throw new ArgumentNullException("feedUrl");

            IFeedDetails fi = null;
            INewsFeed f = null;
            if (itemsTable.ContainsKey(feedUrl))
            {
                fi = itemsTable[feedUrl];
            }
            if (feedsTable.ContainsKey(feedUrl))
            {
                f = feedsTable[feedUrl];
            }
            if (fi != null && f != null)
            {
                try
                {
                    f.cacheurl = SaveFeed(f);
                }
                catch (Exception ex)
                {
                    Trace("ApplyFeedModifications() cause exception while saving feed '{0}'to cache: {1}", feedUrl,
                          ex.Message);
                }
            }
        }


        /// <summary>
        /// Tests whether a particular propery value is set
        /// </summary>
        /// <param name="value">the value to test</param>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="owner">the object which the property comes from</param>
        /// <returns>true if it is set and false otherwise</returns>
        private static bool IsPropertyValueSet(object value, string propertyName, ISharedProperty owner)
        {
            if (value == null)
            {
                return false;
            }

            if (value is string)
            {
                var isSet = !string.IsNullOrEmpty((string) value);

                if (propertyName.Equals("maxitemage") && isSet)
                {
                    isSet = !value.Equals(XmlConvert.ToString(TimeSpan.MaxValue));
                }

                return isSet;
            }


            return (bool) GetSharedPropertyValue(owner, propertyName + "Specified");
            //return (bool) owner.GetType().GetProperty(propertyName + "Specified").GetValue(owner, null);
        }


        /// <summary>
        /// Gets the value of a feed's property. This does not inherit the properties of parent
        /// categories. 
        /// </summary>
        /// <param name="feedUrl">the feed URL</param>
        /// <param name="propertyName">the name of the property</param>		
        /// <returns>the value of the property</returns>
        private object GetFeedProperty(string feedUrl, string propertyName)
        {
            return GetFeedProperty(feedUrl, propertyName, false);
        }

        /// <summary>
        /// Gets the value of a feed's property
        /// </summary>
        /// <param name="feedUrl">the feed URL</param>
        /// <param name="propertyName">the name of the property</param>
        /// <param name="inheritCategory">indicates whether the settings from the parent category should be inherited or not</param>
        /// <returns>the value of the property</returns>
        private object GetFeedProperty(string feedUrl, string propertyName, bool inheritCategory)
        {
            var value = GetSharedPropertyValue(this, propertyName);
            //this.GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            if (propertyName.Equals("maxitemage"))
            {
                value = XmlConvert.ToTimeSpan((string) value);
            }

            if (feedsTable.ContainsKey(feedUrl))
            {
                var f = feedsTable[feedUrl];
                var f_value = GetSharedPropertyValue(f, propertyName);
                // f.GetType().GetProperty(propertyName).GetValue(f, null);

                if (IsPropertyValueSet(f_value, propertyName, f))
                {
                    if (propertyName.Equals("maxitemage"))
                    {
                        f_value = XmlConvert.ToTimeSpan((string) f_value);
                    }

                    value = f_value;
                }
                else if (inheritCategory && !string.IsNullOrEmpty(f.category))
                {
                    INewsFeedCategory c;
                    categories.TryGetValue(f.category, out c);

                    while (c != null)
                    {
                        var c_value = GetSharedPropertyValue(c, propertyName);
                        // c.GetType().GetProperty(propertyName).GetValue(c, null);

                        if (IsPropertyValueSet(c_value, propertyName, c))
                        {
                            if (propertyName.Equals("maxitemage"))
                            {
                                c_value = XmlConvert.ToTimeSpan((string) c_value);
                            }
                            value = c_value;
                            break;
                        }
                        else
                        {
                            c = c.parent;
                        }
                    } //while
                } //else if(!string.IsNullOrEmpty(f.category))
            } //if(feedsTable.ContainsKey(feedUrl)){


            return value;
        }

        /// <summary>
        /// Sets the value of a feed property.
        /// </summary>
        /// <param name="feedUrl"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        private void SetFeedProperty(string feedUrl, string propertyName, object value)
        {
            //TODO: Make this code more efficient

            if (feedsTable.ContainsKey(feedUrl))
            {
                var f = feedsTable[feedUrl];

                if (value is TimeSpan)
                {
                    value = XmlConvert.ToString((TimeSpan) value);
                }
                SetSharedPropertyValue(f, propertyName, value);
                //f.GetType().GetProperty(propertyName).SetValue(f, value, null);

                if ((value != null) && !(value is string))
                {
                    SetSharedPropertyValue(f, propertyName + "Specified", true);
                    //f.GetType().GetProperty(propertyName + "Specified").SetValue(f, true, null);
                }
            }
        }

        /// <summary>
        ///  Sets the maximum amount of time an item should be kept in the 
        /// cache for a particular feed. This overrides the value of the 
        /// maxItemAge property. 
        /// </summary>
        /// <remarks>If the feed URL is not found in the FeedsTable then nothing happens</remarks>
        /// <param name="feedUrl">The feed</param>
        /// <param name="age">The maximum amount of time items should be kept for the 
        /// specified feed.</param>
        public void SetMaxItemAge(string feedUrl, TimeSpan age)
        {
            SetFeedProperty(feedUrl, "maxitemage", age);
        }

        /// <summary>
        /// Gets the maximum amount of time an item is kept in the 
        /// cache for a particular feed. 
        /// </summary>
        /// <param name="feedUrl">The feed identifier</param>
        /// <exception cref="FormatException">if an error occurs while converting the max item age value to a TimeSpan</exception>
        public TimeSpan GetMaxItemAge(string feedUrl)
        {
            return (TimeSpan) GetFeedProperty(feedUrl, "maxitemage", true);
        }


        /// <summary>
        /// Sets the refresh rate for a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="refreshRate">the new refresh rate</param>
        public void SetRefreshRate(string feedUrl, int refreshRate)
        {
            SetFeedProperty(feedUrl, "refreshrate", refreshRate);
        }

        /// <summary>
        /// Gets the refresh rate for a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <returns>the refresh rate</returns>
        public int GetRefreshRate(string feedUrl)
        {
            return (int) GetFeedProperty(feedUrl, "refreshrate", true);
        }

        /// <summary>
        /// Sets the stylesheet for a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="style">the new stylesheet</param>
        public void SetStyleSheet(string feedUrl, string style)
        {
            SetFeedProperty(feedUrl, "stylesheet", style);
        }

        /// <summary>
        /// Gets the stylesheet for a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <returns>the stylesheet</returns>
        public string GetStyleSheet(string feedUrl)
        {
            return (string) GetFeedProperty(feedUrl, "stylesheet");
        }


        /// <summary>
        /// Sets the enclosure folder for a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="folder">the new enclosure folder </param>
        public void SetEnclosureFolder(string feedUrl, string folder)
        {
            SetFeedProperty(feedUrl, "enclosurefolder", folder);
        }

        /// <summary>
        /// Gets the target folder to download enclosures from a feed. The folder returned 
        /// may change depending on whether the item is a podcast (i.e. is in the 
        /// podcastfileextensions ArrayList)
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="filename">The name of the file</param>
        /// <returns>the enclosure folder</returns>
        public string GetEnclosureFolder(string feedUrl, string filename)
        {
            var folderName = (IsPodcast(filename) ? PodcastFolder : EnclosureFolder);

            if (CreateSubfoldersForEnclosures && feedsTable.ContainsKey(feedUrl))
            {
                var f = feedsTable[feedUrl];
                folderName = Path.Combine(folderName, FileHelper.CreateValidFileName(f.title));
            }

            return folderName;
        }


        /// <summary>
        /// Sets the listview layout for a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="layout">the new listview layout </param>
        public void SetFeedColumnLayout(string feedUrl, string layout)
        {
            SetFeedProperty(feedUrl, "listviewlayout", layout);
        }

        /// <summary>
        /// Gets the listview layout for a feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <returns>the listview layout</returns>
        public string GetFeedColumnLayout(string feedUrl)
        {
            return (string) GetFeedProperty(feedUrl, "listviewlayout");
        }


        /// <summary>
        /// Sets whether to mark items as read on exiting the feed in the UI
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="markitemsread">the new value for markitemsreadonexit</param>
        public void SetMarkItemsReadOnExit(string feedUrl, bool markitemsread)
        {
            SetFeedProperty(feedUrl, "markitemsreadonexit", markitemsread);
        }

        /// <summary>
        /// Gets whether to mark items as read on exiting the feed in the UI
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <returns>whether to mark items as read on exit</returns>
        public bool GetMarkItemsReadOnExit(string feedUrl)
        {
            return (bool) GetFeedProperty(feedUrl, "markitemsreadonexit");
        }

        /// <summary>
        /// Sets whether to download enclosures for this feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="download">the new value for downloadenclosures</param>
        public void SetDownloadEnclosures(string feedUrl, bool download)
        {
            SetFeedProperty(feedUrl, "downloadenclosures", download);
        }

        /// <summary>
        /// Gets whether to download enclosures for this feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <returns>hether to download enclosures for this feed</returns>
        public bool GetDownloadEnclosures(string feedUrl)
        {
            return (bool) GetFeedProperty(feedUrl, "downloadenclosures");
        }


        /// <summary>
        /// Sets whether to display an alert when an enclosure is successfully
        /// downloaded for this feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <param name="alert">if set to <c>true</c> [enclosurealert].</param>
        public void SetEnclosureAlert(string feedUrl, bool alert)
        {
            SetFeedProperty(feedUrl, "enclosurealert", alert);
        }

        /// <summary>
        /// Gets whether to display an alert when an enclosure is successfully 
        /// downloaded for this feed
        /// </summary>
        /// <param name="feedUrl">the URL of the feed</param>
        /// <returns>hether to download enclosures for this feed</returns>
        public bool GetEnclosureAlert(string feedUrl)
        {
            return (bool) GetFeedProperty(feedUrl, "enclosurealert");
        }

        /// <summary>
        /// Gets the value of a category's property
        /// </summary>
        /// <param name="category">the category name</param>
        /// <param name="propertyName">the name of the property</param>
        /// <returns>the value of the property</returns>
        private object GetCategoryProperty(string category, string propertyName)
        {
            var value = GetSharedPropertyValue(this, propertyName);
            //this.GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            if (propertyName.Equals("maxitemage"))
            {
                value = XmlConvert.ToTimeSpan((string) value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                INewsFeedCategory c;
                categories.TryGetValue(category, out c);

                while (c != null)
                {
                    var c_value = GetSharedPropertyValue(c, propertyName);
                    //c.GetType().GetProperty(propertyName).GetValue(c, null);

                    if (IsPropertyValueSet(c_value, propertyName, c))
                    {
                        if (propertyName.Equals("maxitemage"))
                        {
                            c_value = XmlConvert.ToTimeSpan((string) c_value);
                        }
                        value = c_value;
                        break;
                    }
                    else
                    {
                        c = c.parent;
                    }
                } //while
            } //if(!string.IsNullOrEmpty(category))


            return value;
        }

        /// <summary>
        /// Sets the value of a category's property.
        /// </summary>
        /// <param name="category">the category's name</param>
        /// <param name="propertyName">the name of the property</param>
        /// <param name="value">the new value</param>
        private void SetCategoryProperty(string category, string propertyName, object value)
        {
            //TODO: Make this code more efficient

            if (!string.IsNullOrEmpty(category))
            {
                //category c = this.Categories.GetByKey(category);

                foreach (category c in categories.Values)
                {
                    //if(c!= null){			

                    if (c.Value.Equals(category) || c.Value.StartsWith(category + CategorySeparator))
                    {
                        if (value is TimeSpan)
                        {
                            value = XmlConvert.ToString((TimeSpan) value);
                        }

                        SetSharedPropertyValue(c, propertyName, value);
                        //c.GetType().GetProperty(propertyName).SetValue(c, value, null);

                        if ((value != null) && !(value is string))
                        {
                            SetSharedPropertyValue(c, propertyName + "Specified", true);
                            //c.GetType().GetProperty(propertyName + "Specified").SetValue(c, true, null);
                        }

                        break;
                    } //if(c!= null) 
                } //foreach
            } //	if(!string.IsNullOrEmpty(category)){
        }


        /// <summary>
        ///  Sets the maximum amount of time an item should be kept in the 
        /// cache for a particular category. This overrides the value of the 
        /// maxItemAge property. 
        /// </summary>
        /// <remarks>If the feed URL is not found in the FeedsTable then nothing happens</remarks>
        /// <param name="category">The feed</param>
        /// <param name="age">The maximum amount of time items should be kept for the 
        /// specified feed.</param>
        public void SetCategoryMaxItemAge(string category, TimeSpan age)
        {
            SetCategoryProperty(category, "maxitemage", age);
        }

        /// <summary>
        /// Gets the maximum amount of time an item is kept in the 
        /// cache for a particular feed. 
        /// </summary>
        /// <param name="category">The name of the category</param>
        /// <exception cref="FormatException">if an error occurs while converting the max item age value to a TimeSpan</exception>
        public TimeSpan GetCategoryMaxItemAge(string category)
        {
            return (TimeSpan) GetCategoryProperty(category, "maxitemage");
        }


        /// <summary>
        /// Sets the refresh rate for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <param name="refreshRate">the new refresh rate</param>
        public void SetCategoryRefreshRate(string category, int refreshRate)
        {
            SetCategoryProperty(category, "refreshrate", refreshRate);
        }

        /// <summary>
        /// Gets the refresh rate for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <returns>the refresh rate</returns>
        public int GetCategoryRefreshRate(string category)
        {
            return (int) GetCategoryProperty(category, "refreshrate");
        }

        /// <summary>
        /// Sets the stylesheet for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <param name="style">the new stylesheet</param>
        public void SetCategoryStyleSheet(string category, string style)
        {
            SetCategoryProperty(category, "stylesheet", style);
        }

        /// <summary>
        /// Gets the stylesheet for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <returns>the stylesheet</returns>
        public string GetCategoryStyleSheet(string category)
        {
            return (string) GetCategoryProperty(category, "stylesheet");
        }


        /// <summary>
        /// Sets the enclosure folder for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <param name="folder">the new enclosure folder </param>
        public void SetCategoryEnclosureFolder(string category, string folder)
        {
            SetCategoryProperty(category, "enclosurefolder", folder);
        }

        /// <summary>
        /// Gets the enclosure folder for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <returns>the enclosure folder</returns>
        public string GetCategoryEnclosureFolder(string category)
        {
            return (string) GetCategoryProperty(category, "enclosurefolder");
        }


        /// <summary>
        /// Sets the listview layout for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <param name="layout">the new listview layout </param>
        public void SetCategoryFeedColumnLayout(string category, string layout)
        {
            SetCategoryProperty(category, "listviewlayout", layout);
        }

        /// <summary>
        /// Gets the listview layout for a category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <returns>the listview layout</returns>
        public string GetCategoryFeedColumnLayout(string category)
        {
            return (string) GetCategoryProperty(category, "listviewlayout");
        }


        /// <summary>
        /// Sets whether to mark items as read on exiting the feed in the UI
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <param name="markitemsread">the new value for markitemsreadonexit</param>
        public void SetCategoryMarkItemsReadOnExit(string category, bool markitemsread)
        {
            SetCategoryProperty(category, "markitemsreadonexit", markitemsread);
        }

        /// <summary>
        /// Gets whether to mark items as read on exiting the feed in the UI
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <returns>whether to mark items as read on exit</returns>
        public bool GetCategoryMarkItemsReadOnExit(string category)
        {
            return (bool) GetCategoryProperty(category, "markitemsreadonexit");
        }

        /// <summary>
        /// Sets whether to download enclosures for this category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <param name="download">the new value for downloadenclosures</param>
        public void SetCategoryDownloadEnclosures(string category, bool download)
        {
            SetCategoryProperty(category, "downloadenclosures", download);
        }

        /// <summary>
        /// Gets whether to download enclosures for this category
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <returns>the refresh rate</returns>
        public bool GetCategoryDownloadEnclosures(string category)
        {
            return (bool) GetCategoryProperty(category, "downloadenclosures");
        }


        /// <summary>
        /// Sets whether to display an alert when an enclosure is successfully downloaded
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <param name="alert">if set to <c>true</c> [enclosurealert].</param>
        public void SetCategoryEnclosureAlert(string category, bool alert)
        {
            SetCategoryProperty(category, "enclosurealert", alert);
        }

        /// <summary>
        /// Gets whether to display an alert when an enclosure is successfully downloaded
        /// </summary>
        /// <param name="category">the name of the category</param>
        /// <returns>the refresh rate</returns>
        public bool GetCategoryEnclosureAlert(string category)
        {
            return (bool) GetCategoryProperty(category, "enclosurealert");
        }

        /// <summary>
        /// Returns the FeedDetails of a feed.
        /// </summary>
        /// <param name="feedUrl">string feed's Url</param>
        /// <returns>FeedInfo or null, if feed was removed or parameter is invalid</returns>
        public virtual IFeedDetails GetFeedDetails(string feedUrl)
        {
            return GetFeedInfo(feedUrl, null);
        }

        /// <summary>
        /// Returns the FeedDetails of a feed.
        /// </summary>
        /// <param name="feedUrl">string feed's Url</param>
        /// <param name="credentials">ICredentials, optional. Can be null</param>
        /// <returns>FeedInfo or null, if feed was removed or parameter is invalid</returns>
        public IFeedDetails GetFeedInfo(string feedUrl, ICredentials credentials)
        {
            if (string.IsNullOrEmpty(feedUrl))
                return null;

            IFeedDetails fd = null;

            if (!itemsTable.ContainsKey(feedUrl))
            {
                var theFeed = feedsTable[feedUrl];

                if (theFeed == null)
                {
//external feed?

                    using (
                        var mem =
                            AsyncWebRequest.GetSyncResponseStream(feedUrl, credentials, UserAgent, Proxy))
                    {
                        var f = new NewsFeed
                                    {
                                        link = feedUrl
                                    };
                        if (RssParser.CanProcessUrl(feedUrl))
                        {
                            fd = RssParser.GetItemsForFeed(f, mem, false);
                        }
                        //TODO: NntpHandler.CanProcessUrl()
                    }
                    return fd;
                }

                fd = GetFeed(theFeed);
                lock (itemsTable)
                {
                    //if feed was in cache but not in itemsTable we load it into itemsTable
                    if (!itemsTable.ContainsKey(feedUrl) && (fd != null))
                    {
                        itemsTable.Add(feedUrl, fd);
                    }
                }
            }
            else
            {
                fd = itemsTable[feedUrl];
            }

            return fd;
        }


        /// <summary>
        /// Retrieves the RSS feed for a particular subscription then converts 
        /// the blog posts or articles to an arraylist of items. 
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to download</param>
        /// <param name="force_download">Flag indicates whether cached feed items 
        /// can be returned or whether the application must fetch resources from 
        /// the web</param>
        /// <exception cref="ApplicationException">If the RSS feed is not 
        /// version 0.91, 1.0 or 2.0</exception>
        /// <exception cref="XmlException">If an error occured parsing the 
        /// RSS feed</exception>
        /// <exception cref="WebException">If an error occurs while attempting to download from the URL</exception>
        /// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
        /// <returns>An arraylist of News items (i.e. instances of the NewsItem class)</returns>		
        //	[MethodImpl(MethodImplOptions.Synchronized)]
        public virtual IList<INewsItem> GetItemsForFeed(string feedUrl, bool force_download)
        {
            //REM gets called from Bandit
            var url2Access = feedUrl;

            if (((!force_download) || isOffline) && itemsTable.ContainsKey(feedUrl))
            {
                return itemsTable[feedUrl].ItemsList;
            }

            //We need a reference to the feed so we can see if a cached object exists
            INewsFeed theFeed = null;
            if (feedsTable.ContainsKey(feedUrl))
                theFeed = feedsTable[feedUrl];

            if (theFeed == null) // not anymore in feedTable
                return EmptyItemList;

            try
            {
                if (((!force_download) || isOffline) && (!itemsTable.ContainsKey(feedUrl)) &&
                    (!string.IsNullOrEmpty(theFeed.cacheurl) &&
                     (CacheHandler.FeedExists(theFeed))))
                {
                    bool getFromCache;
                    lock (itemsTable)
                    {
                        getFromCache = !itemsTable.ContainsKey(feedUrl);
                    }
                    if (getFromCache)
                    {
                        // do not call from within a lock:
                        var fi = GetFeed(theFeed);
                        if (fi != null)
                        {
                            lock (itemsTable)
                            {
                                if (!itemsTable.ContainsKey(feedUrl))
                                    itemsTable.Add(feedUrl, fi);
                            }
                        }
                    }

                    return itemsTable[feedUrl].ItemsList;
                }
            }
            catch (Exception ex)
            {
                Trace("Error retrieving feed '{0}' from cache: {1}", feedUrl, ex.ToString());
            }


            if (isOffline)
            {
                //we are in offline mode and don't have the feed cached. 
                return EmptyItemList;
            }

            try
            {
                new Uri(url2Access);
            }
            catch (UriFormatException ufex)
            {
                Trace("Uri format exception on '{0}': {1}", url2Access, ufex.Message);
                throw;
            }


            AsyncGetItemsForFeed(feedUrl, true, true);
            return EmptyItemList; //we just return this for now, the async call will return real results 
        }


        /// <summary>
        /// Returns the number of pending async. requests in the queue.
        /// </summary>
        /// <returns></returns>
        public int AsyncRequestsPending()
        {
            return AsyncWebRequest.PendingRequests;
        }


        /// <summary>
        /// Creates a copy of the specified NewsItem with the specified NewsFeed as its owner 
        /// </summary>
        /// <param name="item">The item to copy</param>
        /// <param name="f">The owner feed</param>
        /// <returns>A copy of the specified news item</returns>
        public INewsItem CopyNewsItemTo(INewsItem item, INewsFeed f)
        {
            //load item content from disk if not in memory, to get a full clone later on
            if (!item.HasContent)
                GetCachedContentForItem(item);

            // now create a full copy (including item content)
            var n = new NewsItem(f, item);
            return n;
        }

        /// <summary>
        /// Loads the content of the NewsItem from the binary file containing 
        /// item content from disk. 
        /// </summary>
        /// <remarks>This should be called when a user clicks on an item which 
        /// had previously been read and thus wasn't loaded from disk on startup. </remarks>
        /// <param name="item"></param>
        public void GetCachedContentForItem(INewsItem item)
        {
            CacheHandler.LoadItemContent(item);
        }

        /// <summary>
        /// Invoked when a NewsItem owned by this FeedSource changes in a way that 
        /// needs to be communicated to the underlying feed source. 
        /// </summary>
        /// <param name="sender">the NewsItem</param>
        /// <param name="e">information on the property that changed</param>
        protected virtual void OnNewsItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Does nothing by default
        }


        /// <summary>
        /// Retrieves items from local cache. 
        /// </summary>
        /// <param name="feedUrl"></param>
        /// <returns>A ArrayList of NewsItem objects</returns>
        public virtual IList<INewsItem> GetCachedItemsForFeed(string feedUrl)
        {
            lock (itemsTable)
            {
                if (itemsTable.ContainsKey(feedUrl))
                {
                    return itemsTable[feedUrl].ItemsList;
                }
            }

            //We need a reference to the feed so we can see if a cached object exists
            INewsFeed theFeed = null;

            try
            {
                if (feedsTable.TryGetValue(feedUrl, out theFeed))
                {
                    if ((theFeed.cacheurl != null) && (theFeed.cacheurl.Trim().Length > 0) &&
                        (CacheHandler.FeedExists(theFeed)))
                    {
                        bool getFromCache;
                        lock (itemsTable)
                        {
                            getFromCache = !itemsTable.ContainsKey(feedUrl);
                        }
                        if (getFromCache)
                        {
                            var fi = GetFeed(theFeed);
                            if (fi != null)
                            {
                                lock (itemsTable)
                                {
                                    if (!itemsTable.ContainsKey(feedUrl))
                                        itemsTable.Add(feedUrl, fi);
                                }
                            }
                        }
                        return itemsTable[feedUrl].ItemsList;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // may be deleted in the middle of Test for Exists and GetFeed()
                // ignore
            }
            catch (XmlException xe)
            {
                //cached file is not well-formed so we remove it from cache. 	
                Trace("Xml Error retrieving feed '{0}' from cache: {1}", feedUrl, xe.ToString());
                CacheHandler.RemoveFeed(theFeed);
            }
            catch (Exception ex)
            {
                Trace("Error retrieving feed '{0}' from cache: {1}", feedUrl, ex.ToString());
                if (theFeed != null && !theFeed.causedException)
                {
                    theFeed.causedException = true;
                    RaiseOnUpdateFeedException(feedUrl,
                                               new Exception(
                                                   "Error retrieving feed {" + feedUrl + "} from cache: " + ex.Message,
                                                   ex), 11);
                }
            }

            return EmptyItemList;
        }

        /// <summary>
        /// Retrieves the RSS feed for a particular subscription then converts
        /// the blog posts or articles to an arraylist of items. The http requests are async calls.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to download</param>
        /// <param name="forceDownload">if set to <c>true</c> [force download].</param>
        /// <returns>
        /// true, if the request really was queued up
        /// </returns>
        /// <exception cref="ApplicationException">If the RSS feed is not
        /// version 0.91, 1.0 or 2.0</exception>
        /// <exception cref="XmlException">If an error occured parsing the
        /// RSS feed</exception>
        /// <exception cref="ArgumentNullException">If feedUrl is a null reference</exception>
        /// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
        /// <remarks>Result arraylist is returned by OnUpdatedFeed event within UpdatedFeedEventArgs</remarks>
        //	[MethodImpl(MethodImplOptions.Synchronized)]
        public bool AsyncGetItemsForFeed(string feedUrl, bool forceDownload)
        {
            return AsyncGetItemsForFeed(feedUrl, forceDownload, false);
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
        public virtual bool AsyncGetItemsForFeed(string feedUrl, bool forceDownload, bool manual)
        {
            if (feedUrl == null || feedUrl.Trim().Length == 0)
                throw new ArgumentNullException("feedUrl");

            string etag = null;
            var requestQueued = false;

            var priority = 10;
            if (forceDownload)
                priority += 100;
            if (manual)
                priority += 1000;


            try
            {
                var reqUri = new Uri(feedUrl);

                try
                {
                    if ((!forceDownload) || isOffline)
                    {
                        GetCachedItemsForFeed(feedUrl); //load feed into itemsTable
                        RaiseOnUpdatedFeed(reqUri, null, RequestResult.NotModified, priority, false);
                        return false;
                    }
                }
                catch (XmlException xe)
                {
                    //cache file is corrupt
                    Trace("Unexpected error retrieving cached feed '{0}': {1}", feedUrl, xe.ToString());
                }

                //We need a reference to the feed so we can see if a cached object exists
                INewsFeed theFeed = null;
                if (feedsTable.ContainsKey(feedUrl))
                    theFeed = feedsTable[feedUrl];

                if (theFeed == null)
                    return false;


                // only if we "real" go over the wire for an update:
                RaiseOnUpdateFeedStarted(reqUri, forceDownload, priority);

                //DateTime lastRetrieved = DateTime.MinValue; 
                var lastModified = DateTime.MinValue;

                if (itemsTable.ContainsKey(feedUrl))
                {
                    etag = theFeed.etag;
                    lastModified = (theFeed.lastretrievedSpecified ? theFeed.lastretrieved : theFeed.lastmodified);
                }


                //get credentials from server definition if this is a newsgroup subscription
                var c = RssHelper.IsNntpUrl(theFeed.link) ? GetNntpServerCredentials(theFeed) : CreateCredentialsFrom(theFeed);

                var reqParam =
                    RequestParameter.Create(reqUri, UserAgent, Proxy, c, lastModified, etag);
                // global cookie handling:
                reqParam.SetCookies = SetCookies;

                AsyncWebRequest.QueueRequest(reqParam,
                                             null,
                                             OnRequestStart,
                                             OnRequestComplete,
                                             OnRequestException, priority);

                requestQueued = true;
            }
            catch (Exception e)
            {
                Trace("Unexpected error on QueueRequest(), processing feed '{0}': {1}", feedUrl, e.ToString());
                RaiseOnUpdateFeedException(feedUrl, e, priority);
            }

            return requestQueued;
        }

        //no really used,...:
        //		private void OnRequestQueued(Uri requestUri, int priority) {
        //			//Trace.WriteLineIf(TraceMode, "Queued: '"+ requestUri.ToString() + "', with Priority '" + priority.ToString()+ "'...", "AsyncRequest");
        //		}

        /// <summary>
        /// Called when a network request has been made to start downloading a feed. 
        /// </summary>
        /// <param name="requestUri">The URL being requested</param>
        /// <param name="cancel">Whether the request is to be cancelled</param>
        protected virtual void OnRequestStart(Uri requestUri, ref bool cancel)
        {
            Trace("AsyncRequest.OnRequestStart('{0}') downloading", requestUri.ToString());
            RaiseBeforeDownloadFeedStarted(requestUri, ref cancel);
            if (!cancel)
                cancel = Offline;
        }

        /// <summary>
        /// Called when an exception occurs while downloading a feed.
        /// </summary>
        /// <param name="requestUri">The URI of the feed</param>
        /// <param name="e">The exception</param>
        /// <param name="priority">The priority of the request</param>
        protected virtual void OnRequestException(Uri requestUri, Exception e, int priority)
        {
            Trace("AsyncRequst.OnRequestException() fetching '{0}': {1}", requestUri.ToString(), e.ToString());

            var key = requestUri.CanonicalizedUri();
            if (feedsTable.ContainsKey(key))
            {
                Trace("AsyncRequest.OnRequestException() '{0}' found in feedsTable.", requestUri.ToString());
                var f = feedsTable[key];
                // now we set this within causedException prop.
                //f.lastretrieved = DateTime.Now; 
                //f.lastretrievedSpecified = true; 
                f.causedException = true;
            }
            else
            {
                Trace("AsyncRequst.OnRequestException() '{0}' NOT found in feedsTable.", requestUri.ToString());
            }

            RaiseOnUpdateFeedException(requestUri.CanonicalizedUri(), e, priority);
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
        protected virtual void OnRequestComplete(Uri requestUri, Stream response, Uri newUri, string eTag,
                                                 DateTime lastModified,
                                                 RequestResult result, int priority)
        {
            Trace("AsyncRequest.OnRequestComplete: '{0}': {1}", requestUri.ToString(), result);
            if (newUri != null)
                Trace("AsyncRequest.OnRequestComplete: perma redirect of '{0}' to '{1}'.", requestUri.ToString(),
                      newUri.ToString());

            IList<INewsItem> itemsForFeed;
            var firstSuccessfulDownload = false;

            //grab items from feed, then save stream to cache. 
            try
            {
                //We need a reference to the feed so we can see if a cached object exists
                INewsFeed theFeed;

                if (!feedsTable.TryGetValue(requestUri.CanonicalizedUri(), out theFeed))
                {
                    Trace("ATTENTION! FeedsTable[requestUri] as NewsFeed returns null for: '{0}'",
                          requestUri.ToString());
                    return;
                }

                var feedUrl = theFeed.link;
                if (true)
                {
                    if (String.Compare(feedUrl, requestUri.CanonicalizedUri(), true) != 0)
                        Trace("feed.link != requestUri: \r\n'{0}'\r\n'{1}'", feedUrl, requestUri.CanonicalizedUri());
                }

                if (newUri != null)
                {
                    // Uri changed/moved permanently

                    feedsTable.Remove(feedUrl);
                    theFeed.link = newUri.CanonicalizedUri();
                    feedsTable.Add(theFeed.link, theFeed);

                    lock (itemsTable)
                    {
                        if (itemsTable.ContainsKey(feedUrl))
                        {
                            var FI = itemsTable[feedUrl];
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

                    FeedDetailsInternal fi;

                    if ((requestUri.Scheme == NntpWebRequest.NntpUriScheme) ||
                        (requestUri.Scheme == NntpWebRequest.NewsUriScheme))
                    {
                        fi = NntpParser.GetItemsForNewsGroup(theFeed, response, false);
                    }
                    else
                    {
                        fi = RssParser.GetItemsForFeed(theFeed, response, false);
                    }

                    FeedDetailsInternal fiFromCache = null;

                    // Sometimes we may not have loaded feed from cache. So ensure it is 
                    // loaded into memory if cached. We don't lock here because loading from
                    // disk is too long a time to hold a lock.  
                    try
                    {
                        if (!itemsTable.ContainsKey(feedUrl))
                        {
                            fiFromCache = GetFeed(theFeed);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace("this.GetFeed(theFeed) caused exception: {0}", ex.ToString());
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
                            var fi2 = itemsTable[feedUrl];

                            if (RssParser.CanProcessUrl(feedUrl))
                            {
                                fi.ItemsList = MergeAndPurgeItems(fi2.ItemsList, fi.ItemsList, theFeed.deletedstories,
                                                                  out newReceivedItems, theFeed.replaceitemsonrefresh,
                                                                  true /* respectOldItemState */);
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
                                foreach (FeedDetailsInternal fdi in itemsTable.Values)
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

                    theFeed.lastretrieved = new DateTime(DateTime.Now.Ticks);
                    theFeed.lastretrievedSpecified = true;

                    theFeed.cacheurl = SaveFeed(theFeed);
                    SearchHandler.IndexAdd(newReceivedItems); // may require theFeed.cacheurl !

                    theFeed.causedException = false;
                    itemsForFeed = fi.ItemsList;

                    /* download podcasts from items we just received if downloadenclosures == true */
                    if (GetDownloadEnclosures(theFeed.link))
                    {
                        var numDownloaded = 0;
                        var maxDownloads = (firstSuccessfulDownload
                                                ? NumEnclosuresToDownloadOnNewFeed
                                                : DefaultNumEnclosuresToDownloadOnNewFeed);

                        if (newReceivedItems != null)
                            foreach (NewsItem ni in newReceivedItems)
                            {
                                //ensure that we don't attempt to download these enclosures at a later date
                                if (numDownloaded >= maxDownloads)
                                {
                                    MarkEnclosuresDownloaded(ni);
                                    continue;
                                }

                                try
                                {
                                    numDownloaded += DownloadEnclosure(ni, maxDownloads - numDownloaded);
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
                    }


                    if (itemsForFeed.Count > theFeed.storiesrecentlyviewed.Count)
                    {
                        theFeed.containsNewMessages = true;
                    }
                }
                else if (result == RequestResult.NotModified)
                {
                    // expected behavior: response == null, if not modified !!!
                    theFeed.lastretrieved = new DateTime(DateTime.Now.Ticks);
                    theFeed.lastretrievedSpecified = true;
                    theFeed.causedException = false;

                    //FeedDetailsInternal feedInfo = itemsTable[feedUrl];
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

                RaiseOnUpdatedFeed(requestUri, newUri, result, priority, firstSuccessfulDownload);
            }
            catch (Exception e)
            {
                var key = requestUri.CanonicalizedUri();
                if (feedsTable.ContainsKey(key))
                {
                    Trace("AsyncRequest.OnRequestComplete('{0}') Exception: ", requestUri.ToString(), e.StackTrace);
                    var f = feedsTable[key];
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

        protected void OnAllRequestsComplete()
        {
            RaiseOnAllAsyncRequestsCompleted();
        }


        protected void OnEnclosureDownloadComplete(object sender, DownloadItemEventArgs e)
        {
            if (OnDownloadedEnclosure != null)
            {
                try
                {
                    OnDownloadedEnclosure(sender, e);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        // see http://www.iana.org/assignments/media-types/image/vnd.microsoft.icon


        /// <summary>
        /// Gets the file extension for a detected image 
        /// </summary>
        /// <param name="bytes">Not null and length > 4!</param>
        /// <returns></returns>
        private static string GetExtensionForDetectedImage(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            int i, len = bytes.Length;

            //check for jpg magic: 
            for (i = 0; i < jpg_magic_len && i < len; i++)
            {
                if (bytes[i] != jpg_magic[i]) break;
            }
            if (i == jpg_magic_len) return ".jpg";

            // check for ico magic:
            for (i = 0; i < ico_magic_len && i < len; i++)
            {
                if (bytes[i] != ico_magic[i]) break;
            }
            if (i == ico_magic_len) return ".ico";

            // check for png magic:
            for (i = 0; i < png_magic_len && i < len; i++)
            {
                if (bytes[i] != png_magic[i]) break;
            }
            if (i == png_magic_len) return ".png";

            // check for gif magic:
            for (i = 0; i < gif_magic_len && i < len; i++)
            {
                if (bytes[i] != gif_magic[i]) break;
            }
            if (i == gif_magic_len) return ".gif";

            // check for bmp magic:
            for (i = 0; i < bmp_magic_len && i < len; i++)
            {
                if (bytes[i] != bmp_magic[i]) break;
            }
            if (i == bmp_magic_len) return ".bmp";

            // not supported, or <HTML> reporting a failure:
            return null;
        }

        private void OnFaviconRequestComplete(Uri requestUri, Stream response, Uri newUri, string eTag,
                                              DateTime lastModified, RequestResult result, int priority)
        {
            Trace("AsyncRequest.OnFaviconRequestComplete: '{0}': {1}", requestUri.ToString(), result);
            if (newUri != null)
                Trace("AsyncRequest.OnFaviconRequestComplete: perma redirect of '{0}' to '{1}'.", requestUri.ToString(),
                      newUri.ToString());

            try
            {
                var feedUrls = new StringCollection();
                string favicon = null;

                if (result == RequestResult.OK)
                {
                    //write favicon to feed cache location 
                    var br = new BinaryReader(response);
                    var bytes = new byte[response.Length];
                    // don't write null length files:
                    if (bytes.Length > 0)
                    {
                        bytes = br.ReadBytes((int) response.Length);
                        // check for some known common image formats:
                        var ext = GetExtensionForDetectedImage(bytes);
                        if (ext != null)
                        {
                            favicon = GenerateFaviconUrl(requestUri, ext);
                            var filelocation = Path.Combine(CacheHandler.CacheLocation, favicon);

                            using (var fs = FileHelper.OpenForWrite(filelocation))
                            {
                                var bw = new BinaryWriter(fs);
                                bw.Write(bytes);
                                bw.Flush();
                            }
                        }
                    }
                    else
                    {
                        // favicon == null; reset
                    }

                    // The "CopyTo()" construct prevents against InvalidOpExceptions/ArgumentOutOfRange
                    // exceptions and keep the loop alive if FeedsTable gets modified from other thread(s)
                    string[] keys;

                    lock (feedsTable)
                    {
                        keys = new string[feedsTable.Count];
                        if (feedsTable.Count > 0)
                            feedsTable.Keys.CopyTo(keys, 0);
                    }

                    //get all feeds that should use the returned favicon
                    foreach (var feedUrl in keys)
                    {
                        if (itemsTable.ContainsKey(feedUrl))
                        {
                            var websiteUrl = ((FeedInfo) itemsTable[feedUrl]).Link;

                            Uri uri = null;
                            try
                            {
                                uri = new Uri(websiteUrl);
                            }
                            catch (Exception)
                            {
                            }

                            if ((uri != null) && uri.Authority.Equals(requestUri.Authority))
                            {
                                feedUrls.Add(feedUrl);
                                var f = feedsTable[feedUrl];
                                f.favicon = favicon;
                            }
                        }
                    } //foreach
                }

                if (favicon != null)
                {
                    RaiseOnUpdatedFavicon(favicon, feedUrls);
                }
            }
            catch (Exception e)
            {
                Trace("AsyncRequest.OnFaviconRequestComplete('{0}') Exception on fetching favicon at: ",
                      requestUri.ToString(), e.StackTrace);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        protected void RaiseBeforeDownloadFeedStarted(Uri requestUri, ref bool cancel)
        {
            if (BeforeDownloadFeedStarted != null)
            {
                try
                {
                    var ea = new DownloadFeedCancelEventArgs(requestUri, cancel);
                    BeforeDownloadFeedStarted(this, ea);
                    cancel = ea.Cancel;
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnMovedFeed(FeedMovedEventArgs fmea)
        {
            if (OnMovedFeed != null)
            {
                try
                {
                    OnMovedFeed(this, fmea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        private void RaiseOnUpdatedFavicon(string favicon, StringCollection feedUrls)
        {
            if (OnUpdatedFavicon != null)
            {
                try
                {
                    OnUpdatedFavicon(this, new UpdatedFaviconEventArgs(favicon, feedUrls));
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }


        protected void RaiseOnUpdatedFeed(Uri requestUri, Uri newUri, RequestResult result, int priority,
                                          bool firstSuccessfulDownload)
        {
            if (OnUpdatedFeed != null)
            {
                try
                {
                    OnUpdatedFeed(this,
                                  new UpdatedFeedEventArgs(requestUri, newUri, result, priority, firstSuccessfulDownload));
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        /* private void RaiseOnUpdateFeedException(Uri requestUri, Exception e, int priority) {
			if (OnUpdateFeedException != null) {
				try {
					if (requestUri != null && RssParser.CanProcessUrl(requestUri.ToString()))
						e = new FeedRequestException(e.Message, e, this.GetFailureContext(requestUri)); 
					OnUpdateFeedException(this, new UpdateFeedExceptionEventArgs(requestUri, e, priority));
				} catch { /* ignore ex. thrown by callback   }
			}
		} */


        /// <summary>
        /// Inform the caller that an exception has occured while refreshing a feed
        /// </summary>
        /// <param name="requestUri">The URI of the feed</param>
        /// <param name="e">The exception</param>
        /// <param name="priority">The priority of the request</param>
        protected virtual void RaiseOnUpdateFeedException(string requestUri, Exception e, int priority)
        {
            if (OnUpdateFeedException != null)
            {
                try
                {
                    if (requestUri != null && RssParser.CanProcessUrl(requestUri))
                        e = new FeedRequestException(e.Message, e, GetFailureContext(requestUri));
                    OnUpdateFeedException(this, new UpdateFeedExceptionEventArgs(requestUri, e, priority));
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnAllAsyncRequestsCompleted()
        {
            if (OnAllAsyncRequestsCompleted != null)
            {
                try
                {
                    OnAllAsyncRequestsCompleted(this, new EventArgs());
                }
                catch
                {
/* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnAddedCategory(CategoryEventArgs cea)
        {
            if (OnAddedCategory != null)
            {
                try
                {
                    OnAddedCategory(this, cea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnDeletedCategory(CategoryEventArgs cea)
        {
            if (OnDeletedCategory != null)
            {
                try
                {
                    OnDeletedCategory(this, cea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnRenamedCategory(CategoryChangedEventArgs ccea)
        {
            if (OnRenamedCategory != null)
            {
                try
                {
                    OnRenamedCategory(this, ccea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnDeletedFeed(FeedDeletedEventArgs fdea)
        {
            if (OnDeletedFeed != null)
            {
                try
                {
                    OnDeletedFeed(this, fdea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnRenamedFeed(FeedRenamedEventArgs frea)
        {
            if (OnRenamedFeed != null)
            {
                try
                {
                    OnRenamedFeed(this, frea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnAddedFeed(FeedChangedEventArgs fcea)
        {
            if (OnAddedFeed != null)
            {
                try
                {
                    OnAddedFeed(this, fcea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnMovedCategory(CategoryChangedEventArgs ccea)
        {
            if (OnMovedCategory != null)
            {
                try
                {
                    OnMovedCategory(this, ccea);
                }
                catch
                {
                    /* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnUpdateFeedsStarted(bool forced)
        {
            if (UpdateFeedsStarted != null)
            {
                try
                {
                    UpdateFeedsStarted(this, new UpdateFeedsEventArgs(forced));
                }
                catch
                {
/* ignore ex. thrown by callback */
                }
            }
        }

        protected void RaiseOnUpdateFeedStarted(Uri feedUri, bool forced, int priority)
        {
            if (UpdateFeedStarted != null)
            {
                try
                {
                    UpdateFeedStarted(this, new UpdateFeedEventArgs(feedUri, forced, priority));
                }
                catch
                {
/* ignore ex. thrown by callback */
                }
            }
        }

        /// <summary>
        /// Uses a deterministic algorithm to generate a name for a favicon file from
        /// the domain name of the site that it belongs to.
        /// </summary>
        /// <param name="uri">The URL to the favicon</param>
        /// <param name="extension">The file extension.</param>
        /// <returns>A name for the favicon file</returns>
        private static string GenerateFaviconUrl(Uri uri, string extension)
        {
            return uri.Authority.Replace(".", "-") + extension;
        }


        /// <summary>
        /// Determines whether the file should be treated as a podcast or just as a regular enclosure.
        /// </summary>
        /// <param name="filename">The name of the file</param>
        /// <returns>Returns true if the file extension is one of those in the podcastfileextensions ArrayList</returns>
        public bool IsPodcast(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return false;
            }

            var fileext = Path.GetExtension(filename);

            if (fileext.Length > 1)
            {
                fileext = fileext.Substring(1);

                foreach (string podcastExt in podcastfileextensions)
                {
                    if (fileext.ToLower().Equals(podcastExt.ToLower()))
                    {
                        return true;
                    }
                } //foreach
            }

            return false;
        }

        /// <summary>
        /// Helper function that marks all of an items enclosures as downloaded. 
        /// </summary>
        /// <param name="item"></param>
        internal static void MarkEnclosuresDownloaded(INewsItem item)
        {
            if (item == null)
            {
                return;
            }

            foreach (Enclosure enc in item.Enclosures)
            {
                enc.Downloaded = true;
            }
        }

        /// <summary>
        /// Downloads all the enclosures associated with the specified NewsItem
        /// </summary>
        /// <param name="item">The newsitem whose enclosures are being downloaded</param>
        /// <param name="maxNumToDownload">The maximum number of enclosures that can be downloaded from this item</param>
        /// <returns>The number of downloaded enclosures</returns>
        protected int DownloadEnclosure(INewsItem item, int maxNumToDownload)
        {
            var numDownloaded = 0;

            if ((maxNumToDownload > 0) && (item != null) && (item.Enclosures.Count > 0))
            {
                foreach (Enclosure enc in item.Enclosures)
                {
                    var di = new DownloadItem(item.Feed.link, item.Id, enc, enclosureDownloader);

                    if (!enc.Downloaded)
                    {
                        enclosureDownloader.BeginDownload(di);
                        enc.Downloaded = true;
                        numDownloaded++;
                    }
                    if (numDownloaded >= maxNumToDownload) break;
                }
            } //if

            if (item != null && numDownloaded < item.Enclosures.Count)
            {
                MarkEnclosuresDownloaded(item);
            }

            return numDownloaded;
        }


        /// <summary>
        /// Downloads all the enclosures associated with the specified NewsItem
        /// </summary>
        /// <param name="item">The newsitem whose enclosures are being downloaded</param>
        public void DownloadEnclosure(INewsItem item)
        {
            DownloadEnclosure(item, Int32.MaxValue);
        }

        /// <summary>
        /// Download the specified enclosure associated with the specified NewsItem. 
        /// </summary>
        /// <remarks>The enclosure will be downloaded ONLY IF it is found as the Url 
        /// field of one of the Enclosure objects in the Enclosures collection of the specified NewsItem</remarks>
        /// <param name="item"></param>
        /// <param name="fileName">The name of the enclosure file to download</param>
        public void DownloadEnclosure(INewsItem item, string fileName)
        {
            if ((item != null) && (item.Enclosures.Count > 0))
            {
                foreach (Enclosure enc in item.Enclosures)
                {
                    if (enc.Url.EndsWith(fileName))
                    {
                        var di = new DownloadItem(item.Feed.link, item.Id, enc, enclosureDownloader);
                        enclosureDownloader.BeginDownload(di);
                        enc.Downloaded = true;
                        break;
                    }
                } //foreach										
            } //if(item != null && ...)
        }

        /// <summary>
        /// Resumes pending BITS downloads from a if any exist. 
        /// </summary>
        public virtual void ResumePendingDownloads()
        {
            if (enclosureDownloader != null)
                enclosureDownloader.ResumePendingDownloads();
        }

        /// <summary>
        /// Downloads the favicons for the various feeds. 
        /// </summary>
        public void RefreshFavicons()
        {
            if ((FeedsListOK == false) || isOffline)
            {
                //we don't have a feed list
                return;
            }

            var websites = new StringCollection();

            try
            {
                var keys = GetFeedsTableKeys();

                //foreach(string sKey in FeedsTable.Keys){
                //  NewsFeed current = FeedsTable[sKey];	

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    if (!itemsTable.ContainsKey(keys[i]))
                    {
                        continue;
                    }

                    var fi = (FeedInfo) itemsTable[keys[i]];

                    Uri webSiteUrl = null;
                    try
                    {
                        webSiteUrl = new Uri(fi.link);
                    }
                    catch (Exception)
                    {
                    }

                    if (webSiteUrl == null || !webSiteUrl.Scheme.ToLower().Equals("http"))
                    {
                        continue;
                    }

                    if (!websites.Contains(webSiteUrl.Authority))
                    {
                        var reqUri = new UriBuilder("http", webSiteUrl.Authority)
                                         {
                                             Path = "favicon.ico"
                                         };

                        try
                        {
                            webSiteUrl = reqUri.Uri;
                        }
                        catch (UriFormatException)
                        {
                            /* probably a local machine feed */
                            _log.ErrorFormat("Error creating URL '{0}/{1}' in RefreshFavicons", webSiteUrl,
                                             "favicon.ico");
                            continue;
                        }

                        var reqParam = RequestParameter.Create(webSiteUrl, UserAgent, Proxy,
                                                               /* ICredentials */ null,
                                                               /* lastModified */ DateTime.MinValue,
                                                               /* etag */ null);
                        // global cookie handling:
                        reqParam.SetCookies = SetCookies;

                        AsyncWebRequest.QueueRequest(reqParam,
                                                     null /* new RequestQueuedCallback(this.OnRequestQueued) */,
                                                     null /* new RequestStartCallback(this.OnRequestStart) */,
                                                     OnFaviconRequestComplete,
                                                     null /* new RequestExceptionCallback(this.OnRequestException) */,
                                                     100 /* priority*/);

                        websites.Add(webSiteUrl.Authority);
                    } //if(!websites.Contains(webSiteUrl.Authority)){					
                } //foreach(FeedInfo fi in itemsTable.Values){
            }
            catch (InvalidOperationException ioe)
            {
// New feeds added to FeedsTable from another thread  

                Trace("RefreshFavicons() InvalidOperationException: {0}", ioe.ToString());
            }
        }


        /// <summary>
        /// Downloads every feed that has either never been downloaded before or 
        /// whose elapsed time since last download indicates a fresh attempt should be made. 
        /// </summary>
        /// <param name="force_download">A flag that indicates whether download attempts should be made 
        /// or whether the cache can be used.</param>
        /// <remarks>This method uses the cache friendly If-None-Match and If-modified-Since
        /// HTTP headers when downloading feeds.</remarks>	
        public virtual void RefreshFeeds(bool force_download)
        {
            if (FeedsListOK == false)
            {
                //we don't have a feed list
                return;
            }

            var anyRequestQueued = false;

            try
            {
                RaiseOnUpdateFeedsStarted(force_download);

                var keys = GetFeedsTableKeys();

                //foreach(string sKey in FeedsTable.Keys){
                //  NewsFeed current = FeedsTable[sKey];	

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    if (!feedsTable.ContainsKey(keys[i])) // may have been redirected/removed meanwhile
                        continue;

                    var current = feedsTable[keys[i]];

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
                                var timeSinceLastDownload =
                                    DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
                                //fix: now consider refreshrate inherited by categories:
                                var refreshRate = GetRefreshRate(current.link);

                                if (!DownloadIntervalReached || (timeSinceLastDownload < refreshRate))
                                {
                                    continue; //no need to download 
                                }
                            } //if(current.lastretrievedSpecified...) 


                            if (AsyncGetItemsForFeed(current.link, true, false))
                                anyRequestQueued = true;
                        }
                        else
                        {
                            // not yet loaded, so not loaded from cache, new subscribed or imported
                            if ((!force_download) && current.lastretrievedSpecified &&
                                string.IsNullOrEmpty(current.cacheurl))
                            {
                                // imported may have lastretrievedSpecified set to reduce the initial payload
                                var timeSinceLastDownload =
                                    DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
                                //fix: now consider refreshrate inherited by categories:
                                var refreshRate = GetRefreshRate(current.link);

                                if (!DownloadIntervalReached || (timeSinceLastDownload < refreshRate))
                                {
                                    continue; //no need to download 
                                }
                            }

                            if (!force_download)
                            {
                                // not in itemsTable, cacheurl set - but no cache file anymore?
                                if (!string.IsNullOrEmpty(current.cacheurl) &&
                                    !CacheHandler.FeedExists(current))
                                    force_download = true;
                            }

                            if (AsyncGetItemsForFeed(current.link, force_download, false))
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
        public virtual void RefreshFeeds(string category, bool force_download)
        {
            if (FeedsListOK == false)
            {
                //we don't have a feed list
                return;
            }

            var anyRequestQueued = false;

            try
            {
                RaiseOnUpdateFeedsStarted(force_download);

                var keys = GetFeedsTableKeys();

                //foreach(string sKey in FeedsTable.Keys){
                //  NewsFeed current = FeedsTable[sKey];	

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    if (!feedsTable.ContainsKey(keys[i])) // may have been redirected/removed meanwhile
                        continue;

                    var current = feedsTable[keys[i]];

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
                                var timeSinceLastDownload =
                                    DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
                                //fix: now consider refreshrate inherited by categories:
                                var refreshRate = GetRefreshRate(current.link);

                                if (!DownloadIntervalReached || (timeSinceLastDownload < refreshRate))
                                {
                                    continue; //no need to download 
                                }
                            } //if(current.lastretrievedSpecified...) 


                            if (current.category != null && IsChildOrSameCategory(category, current.category))
                            {
                                if (AsyncGetItemsForFeed(current.link, true, false))
                                    anyRequestQueued = true;
                            }
                        }
                        else
                        {
                            if (current.category != null && IsChildOrSameCategory(category, current.category))
                            {
                                if (AsyncGetItemsForFeed(current.link, force_download, false))
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

        /// <summary>
        /// Determines whether two categories are the same or are whether 
        /// </summary>
        /// <param name="category">The category we are testing against</param>
        /// <param name="testCategory">The category being tested</param>
        /// <returns></returns>
        protected static bool IsChildOrSameCategory(string category, string testCategory)
        {
            if (testCategory.Equals(category) || testCategory.StartsWith(category + CategorySeparator))
                return true;
            
            return false;
        }

        /// <summary>
        /// Converts the input XML document from OCS, OPML or SIAM to the RSS Bandit feed list 
        /// format. 
        /// </summary>
        /// <param name="doc">The input feed list</param>
        /// <returns>The converted feed list</returns>
        /// <exception cref="ApplicationException">if the feed list format is unknown</exception>
        public XmlDocument ConvertFeedList(XmlDocument doc)
        {
            var importFilter = new ImportFilter(doc);

            var transform = importFilter.GetImportXsl();

            if (transform != null)
            {
                // We have a format other than Bandit
                // Apply the import filter (transform)
                var temp = new XmlDocument();
                temp.Load(transform.Transform(doc, null));
                doc = temp;
            }
            else
            {
                // see if we have a Bandit format
                if (importFilter.Format == ImportFeedFormat.Bandit)
                {
                    // load and validate the Bandit feed file
                    //validate document 
                    var context =
                        new XmlParserContext(null, new RssBanditXmlNamespaceResolver(), null, XmlSpace.None);
                    XmlReader vr = new RssBanditXmlReader(doc.OuterXml, XmlNodeType.Document, context);
                    doc.Load(vr);
                    vr.Close();
                }
                else
                {
                    // We have an unknown format
                    throw new ApplicationException("Unknown Feed Format.", null);
                }
            }

            return doc;
        }


        /// <summary>
        /// Replaces the existing list of feeds used by the application with the list of 
        /// feeds in the specified XML document. The file must be an RSS Bandit feed list
        /// or a SIAM file. 
        /// </summary>
        /// <param name="feedlist">The list of feeds</param>
        /// <exception cref="ApplicationException">If the file is not a SIAM, OPML or RSS bandit feedlist</exception>		
        public void ReplaceFeedlist(Stream feedlist)
        {
            ImportFeedlist(feedlist, String.Empty, true);
        }


        /// <summary>
        /// Replaces or imports the existing list of feeds used by the application with the list of 
        /// feeds in the specified XML document. The file must be an RSS Bandit feed list
        /// or a SIAM file. 
        /// </summary>
        /// <param name="feedlist">The list of feeds</param>
        /// <param name="category">The category to import the feeds into</param>
        /// <param name="replace">Indicates whether the feedlist should be replaced or not</param>
        /// <exception cref="ApplicationException">If the file is not a SIAM, OPML or RSS bandit feedlist</exception>		
        public void ImportFeedlist(Stream feedlist, string category, bool replace)
        {
            var doc = new XmlDocument();
            doc.Load(feedlist);

            //convert feed list to RSS Bandit format
            doc = ConvertFeedList(doc);

            //load up 
            var reader = new XmlNodeReader(doc);
            var serializer = XmlHelper.SerializerCache.GetSerializer(typeof (feeds));
            var myFeeds = (feeds) serializer.Deserialize(reader);
            reader.Close();

            var keepLocalSettings = true;
            ImportFeedlist(myFeeds, category, replace, keepLocalSettings);
        }


        /// <summary>
        /// Replaces or imports the existing list of feeds used by the application with the list of 
        /// feeds in the specified XML document. The file must be an RSS Bandit feed list
        /// or a SIAM file. 
        /// </summary>
        /// <param name="myFeeds">The list of feeds</param>
        /// <param name="category">The category to import the feeds into</param>
        /// <param name="replace">Indicates whether the feedlist should be replaced or not</param>
        /// <param name="keepLocalSettings">Indicates that the local feed specific settings should not be overwritten 
        /// by the imported settings</param>
        /// <exception cref="ApplicationException">If the file is not a SIAM, OPML or RSS bandit feedlist</exception>		
        public void ImportFeedlist(feeds myFeeds, string category, bool replace, bool keepLocalSettings)
        {
            //feedListImported = true; 
            /* TODO: Sync category settings */

            IDictionary<string, INewsFeedCategory> cats = new Dictionary<string, INewsFeedCategory>();
            var colLayouts = new FeedColumnLayoutCollection();

            IDictionary<string, INewsFeed> syncedfeeds = new SortedDictionary<string, INewsFeed>();

            // InitialHTTPLastModifiedSettings used to reduce the initial payload
            // for the first request of imported feeds.
            // HTTP endpoints considering also/only the ETag header will influence 
            // if a 200 OK is returned onrequest or not.
            // HTTP endpoints not considering the Last Modified header will not be affected.
            var dta = RssHelper.InitialLastRetrievedSettings(myFeeds.feed.Count, RefreshRate);
            int dtaCount = dta.Length, count = 0;

            while (myFeeds.feed.Count != 0)
            {
                INewsFeed f1 = myFeeds.feed[0];

                var isBadUri = false;
                try
                {
                    new Uri(f1.link);
                }
                catch (Exception)
                {
                    isBadUri = true;
                }

                if (isBadUri)
                {
                    myFeeds.feed.RemoveAt(0);
                    continue;
                }

                if (replace && feedsTable.ContainsKey(f1.link))
                {
                    //copy category information over
                    var f2 = feedsTable[f1.link];

                    if (!keepLocalSettings)
                    {
                        f2.category = f1.category;

                        if ((f2.category != null) && !cats.ContainsKey(f2.category))
                        {
                            cats.Add(f2.category, new category(f2.category));
                        }

                        //copy listview layout information over
                        if ((f1.listviewlayout != null) && !colLayouts.ContainsKey(f1.listviewlayout))
                        {
                            var layout = FindLayout(f1.listviewlayout, myFeeds.listviewLayouts);

                            if (layout != null)
                                colLayouts.Add(f1.listviewlayout, layout.FeedColumnLayout);
                            else
                                f1.listviewlayout = null;
                        }
                        f2.listviewlayout = (f1.listviewlayout ?? f2.listviewlayout);


                        //copy title information over 
                        f2.title = f1.title;


                        //copy various settings over			
                        f2.markitemsreadonexitSpecified = f1.markitemsreadonexitSpecified;
                        if (f1.markitemsreadonexitSpecified)
                        {
                            f2.markitemsreadonexit = f1.markitemsreadonexit;
                        }

                        f2.stylesheet = (f1.stylesheet ?? f2.stylesheet);
                        f2.maxitemage = (f1.maxitemage ?? f2.maxitemage);
                        f2.alertEnabledSpecified = f1.alertEnabledSpecified;
                        f2.alertEnabled = (f1.alertEnabledSpecified ? f1.alertEnabled : f2.alertEnabled);
                        f2.refreshrateSpecified = f1.refreshrateSpecified;
                        f2.refreshrate = (f1.refreshrateSpecified ? f1.refreshrate : f2.refreshrate);

                        //DISCUSS
                        //f2.downloadenclosures ?

                        // save to sync.: key is generated the same on every machine, IV seems to have no influence 
                        f2.authPassword = f1.authPassword;
                        f2.authUser = f1.authUser;
                    } //if(!keepLocalSettings)

                    //copy over deleted stories
                    foreach (var story in f1.deletedstories)
                    {
                        if (!f2.deletedstories.Contains(story))
                        {
                            f2.AddDeletedStory(story);
                        }
                    } //foreach

                    //copy over read stories
                    foreach (var story in f1.storiesrecentlyviewed)
                    {
                        if (!f2.storiesrecentlyviewed.Contains(story))
                        {
                            f2.AddViewedStory(story);
                        }
                    } //foreach					

                    if (itemsTable.ContainsKey(f2.link))
                    {
                        var items = ((FeedInfo) itemsTable[f2.link]).itemsList;

                        foreach (var item in items)
                        {
                            if (f2.storiesrecentlyviewed.Contains(item.Id))
                            {
                                item.BeenRead = true;
                            }
                        }
                    }

                    f2.owner = this;
                    syncedfeeds.Add(f2.link, f2);
                }
                else
                {
                    if (replace)
                    {
                        if ((f1.category != null) && !cats.ContainsKey(f1.category))
                        {
                            cats.Add(f1.category, new category(f1.category));
                        }

                        if ((f1.listviewlayout != null) && !colLayouts.ContainsKey(f1.listviewlayout))
                        {
                            var layout = FindLayout(f1.listviewlayout, myFeeds.listviewLayouts);

                            if (layout != null)
                                colLayouts.Add(f1.listviewlayout, layout.FeedColumnLayout);
                            else
                                f1.listviewlayout = null;
                        }

                        if (!syncedfeeds.ContainsKey(f1.link))
                        {
                            syncedfeeds.Add(f1.link, f1);
                        }
                    }
                    else
                    {
                        if (!StringHelper.EmptyTrimOrNull(category))
                        {
                            f1.category = (f1.category == null ? category : category + CategorySeparator + f1.category);
                        }
                        //f1.category = (category  == String.Empty ? f1.category : category + FeedSource.CategorySeparator + f1.category); 
                        if (!feedsTable.ContainsKey(f1.link))
                        {
                            f1.lastretrievedSpecified = true;
                            f1.lastretrieved = dta[count%dtaCount];
                            this.AddFeed(f1);
                        }
                    }
                }

                myFeeds.feed.RemoveAt(0);
                count++;
            }


            IDictionary<string, INntpServerDefinition> serverList = new Dictionary<string, INntpServerDefinition>();
            IDictionary<string, UserIdentity> identityList = new Dictionary<string, UserIdentity>();

            /* copy over user identity information */
            foreach (var identity in myFeeds.identities)
            {
                if (replace)
                {
                    identityList.Add(identity.Name, identity);
                }
                else if (!identities.ContainsKey(identity.Name))
                {
                    identities.Add(identity.Name, identity);
                }
            } //foreach


            /* copy over newsgroup information */
            foreach (var server in myFeeds.nntpservers)
            {
                if (replace)
                {
                    serverList.Add(server.Name, server);
                }
                else if (!identities.ContainsKey(server.Name))
                {
                    nntpServers.Add(server.Name, server);
                }
            }

            // copy over layout information 
            foreach (var layout in myFeeds.listviewLayouts)
            {
                if (replace)
                {
                    if (layout.FeedColumnLayout.LayoutType == LayoutType.GlobalFeedLayout ||
                        layout.FeedColumnLayout.LayoutType == LayoutType.GlobalCategoryLayout ||
                        layout.FeedColumnLayout.LayoutType == LayoutType.SearchFolderLayout ||
                        layout.FeedColumnLayout.LayoutType == LayoutType.SpecialFeedsLayout)
                        colLayouts.Add(layout.ID, layout.FeedColumnLayout);
                }
                else if (!layouts.ContainsKey(layout.ID))
                {
                    //don't replace layouts on import
                    if (layout.FeedColumnLayout.LayoutType != LayoutType.GlobalFeedLayout ||
                        layout.FeedColumnLayout.LayoutType != LayoutType.GlobalCategoryLayout ||
                        layout.FeedColumnLayout.LayoutType != LayoutType.SearchFolderLayout ||
                        layout.FeedColumnLayout.LayoutType != LayoutType.SpecialFeedsLayout)
                        layouts.Add(layout.ID, layout.FeedColumnLayout);
                }
            }


            if (replace)
            {
                /* update feeds table */
                feedsTable = syncedfeeds;
                /* update category information */
                categories = cats;
                /* update identities */
                identities = identityList;
                /* update servers */
                nntpServers = serverList;
                /* update layouts */
                layouts = colLayouts;
            }
            else
            {
                if (myFeeds.categories.Count == 0)
                {
                    //no new subcategories
                    if (!StringHelper.EmptyTrimOrNull(category) && categories.ContainsKey(category) == false)
                    {
                        AddCategory(category);
                    }
                }
                else
                {
                    foreach (var cat in myFeeds.categories)
                    {
                        var cat2 = (StringHelper.EmptyTrimOrNull(category) ? cat.Value : category + CategorySeparator + cat.Value);

                        if (categories.ContainsKey(cat2) == false)
                        {
                            AddCategory(cat2);
                        }
                    }
                }
            }
            
            readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(categories);
            readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(feedsTable); 

            //if original feed list was invalid then reset error indication	
            if (validationErrorOccured)
            {
                validationErrorOccured = false;
            }
        }


        /// <summary>
        /// Merges the list of feeds in the specified XML document with that currently 
        /// used by the application. The file can either be an RSS Bandit feed list or an 
        /// OPML file. 
        /// </summary>
        /// <param name="feedlist">The list of feeds</param>
        /// <exception cref="ApplicationException">If the file is neither an OPML file or RSS bandit feedlist</exception>		
        public void ImportFeedlist(Stream feedlist)
        {
            ImportFeedlist(feedlist, String.Empty, false);
        }


        /// <summary>
        /// Merges the list of feeds in the specified XML document with that currently 
        /// used by the application. The file can either be an RSS Bandit feed list or an 
        /// OPML file. 
        /// </summary>
        /// <param name="feedlist">The list of feeds</param>
        /// <param name="category">The category to import the feeds into</param>
        /// <exception cref="ApplicationException">If the file is neither an OPML file or RSS bandit feedlist</exception>		
        public void ImportFeedlist(Stream feedlist, string category)
        {
            try
            {
                ImportFeedlist(feedlist, category, false);               
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }

        /// <summary>
        /// Handles errors that occur during schema validation of RSS feed list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void ValidationCallbackOne(object sender,
                                                 ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                Trace("ValidationCallbackOne() message: {0}", args.Message);

                /* In some cases we corrupt feedlist.xml by not putting all referenced
				 * categories in <category> elements. This is not a fatal error. 
				 * 
				 * Also we sometimes corrupt subscriptions.xml by putting multiple entries for the same category.
				 */
                var xse = args.Exception;
                if (xse != null)
                {
                    var xseType = xse.GetType();
                    var resFieldInfo = xseType.GetField("res", BindingFlags.NonPublic | BindingFlags.Instance);

                    var errorType = (string) resFieldInfo.GetValue(xse);

                    if (!errorType.Equals("Sch_UnresolvedKeyref") && !errorType.Equals("Sch_DuplicateKey"))
                    {
                        validationErrorOccured = true;
                    }
                    else
                    {
                        //  categoryMismatch = true;
                    }
                } //if(xse != null) 
            } //if(args.Severity...)	
        }


        /// <summary>
        /// Saves a particular RSS feed.
        /// </summary>
        /// <remarks>This method should be thread-safe</remarks>
        /// <param name="feed">The the feed to save. This is an identifier
        /// and not used to actually fetch the feed from the WWW.</param>
        /// <returns>An identifier for the saved feed. </returns>		
        protected string SaveFeed(INewsFeed feed)
        {
            var maxItemAge = GetMaxItemAge(feed.link);
            var fi = itemsTable[feed.link] as FeedDetailsInternal;
            IList<INewsItem> items = fi.ItemsList;

            /* remove items that have expired according to users cache requirements */
            if (maxItemAge != TimeSpan.MinValue)
            {
                /* check if feed set to never delete items */

                lock (items)
                {
                    for (int i = 0, count = items.Count; i < count; i++)
                    {
                        var item = items[i];

                        if (feed.deletedstories.Contains(item.Id) || ((DateTime.Now - item.Date) >= maxItemAge))
                        {
                            items.Remove(item);
                            RelationCosmosRemove(item);
                            SearchHandler.IndexRemove(item);
                            count--;
                            i--;
                        } //if
                    } //for
                } //lock
            } //if(maxItemAge != TimeSpan.MinValue)						


            return CacheHandler.SaveFeed(fi);
        }

        /// <summary>
        /// Returns an RSS feed. 
        /// </summary>
        /// <param name="feed">The feed whose FeedInfo is required.</param>
        /// <returns>The requested feed or null if it doesn't exist</returns>
        internal FeedDetailsInternal GetFeed(INewsFeed feed)
        {
            var fi = CacheHandler.GetFeed(feed);

            if (fi != null)
            {
                /* remove items that have expired according to users cache requirements */
                var maxItemAge = GetMaxItemAge(feed.link);

                var readItems = 0;

                IList<INewsItem> items = fi.ItemsList;
                lock (items)
                {
                    /* check if feed set to never delete items */
                    var keepAll = (maxItemAge == TimeSpan.MinValue) && (feed.deletedstories.Count == 0);

                    //since we are going to use this value for calculation we should change it 
                    //from TimeSpan.MinValue which is used to indicate 'keep indefinitely' to TimeSpan.MaxValue
                    maxItemAge = (maxItemAge == TimeSpan.MinValue ? TimeSpan.MaxValue : maxItemAge);

                    for (int i = 0, count = items.Count; i < count; i++)
                    {
                        var item = items[i];

                        if ((!keepAll) && ((DateTime.Now - item.Date) >= maxItemAge) ||
                            feed.deletedstories.Contains(item.Id))
                        {
                            //items.Remove(item);  // calls internal IndexOf() and RemoveAt()	
                            items.RemoveAt(i);
                            RelationCosmosRemove(item);
                            i--;
                            count--;
                        }
                        else if (item.BeenRead)
                        {
                            readItems++;
                        }

                        //add read/flag state event handler. 
                        var inpc = item as INotifyPropertyChanged;
                        if (inpc != null)
                        {
                            inpc.PropertyChanged -= OnNewsItemPropertyChanged;
                            //remove it first, in case we already have it attached
                            inpc.PropertyChanged += OnNewsItemPropertyChanged;
                        }
                    }
                }

                if (readItems == items.Count)
                {
                    feed.containsNewMessages = false;
                }
                else
                {
                    feed.containsNewMessages = true;
                }
            } //if(fi != null)

            return fi;
        }

        /// <summary>
        /// Merge and purge items.
        /// </summary>
        /// <param name="oldItems">List with the old items</param>
        /// <param name="newItems">List with the new items</param>
        /// <param name="deletedItems">List with the IDs of deleted items</param>
        /// <param name="receivedNewItems">IList with the really new (received) items.</param>
        /// <param name="onlyKeepNewItems">Indicates that we only want the items from newItems to be kept. If this value is true 
        /// then this method merely copies over item state of any oldItems that are in newItems then returns newItems</param>
        /// <param name="respectOldItemState">Indicates that read and flag status from the old items should be respected</param>
        /// <returns>IList merge/purge result</returns>
        public static List<INewsItem> MergeAndPurgeItems(List<INewsItem> oldItems, List<INewsItem> newItems,
                                                         ICollection<string> deletedItems,
                                                         out List<INewsItem> receivedNewItems,
                                                         bool onlyKeepNewItems, bool respectOldItemState)
        {
            receivedNewItems = new List<INewsItem>();
            //ArrayList removedOldItems = new ArrayList(); 

            lock (oldItems)
            {
                foreach (NewsItem newitem in newItems)
                {
                    var index = oldItems.IndexOf(newitem);
                    if (index == -1)
                    {
                        if (!deletedItems.Contains(newitem.Id))
                        {
                            receivedNewItems.Add(newitem);
                            oldItems.Add(newitem);
                            //perform whatever processing is needed
                            ReceivingNewsChannelServices.ProcessItem(newitem);
                        }
                    }
                    else
                    {
                        var olditem = oldItems[index];

                        if (respectOldItemState)
                        {
                            newitem.BeenRead = olditem.BeenRead;
                        }

                        /*
						COMMENTED OUT BECAUSE WE WON'T SAVE NEWLY DOWNLOADED TEXT IF THE 
						FEED IS UPDATED WITH THE CODE BELOW. 
						
						//We don't need strings in memory if we've read it. However we have to 
						//account for the edge case where the feed list was imported and this was 
						//read but hasn't yet been saved to the cache. 
						//
						if(!feedListImported && newitem.BeenRead){ 
							newitem.SetContent((string) null, newitem.ContentType); 
						} */
                        newitem.Date = olditem.Date; //so the date is from when it was first fetched

                        if (respectOldItemState)
                        {
                            newitem.FlagStatus = olditem.FlagStatus;
                        }

                        if (olditem.WatchComments)
                        {
                            newitem.WatchComments = true;

                            if ((olditem.HasNewComments) || (olditem.CommentCount < newitem.CommentCount))
                            {
                                newitem.HasNewComments = true;
                            }
                        } //if(olditem.WatchComments) 

                        //feed doesn't support <slash:comments>, so we use the existing comment count 
                        //in case we previously obtained it by fetching the CommentRssUrl
                        if (newitem.CommentCount == NewsItem.NoComments)
                        {
                            newitem.CommentCount = olditem.CommentCount;
                        }

                        //see if we've downloaded any of the enclosures on the old item
                        if (olditem.Enclosures.Count > 0)
                        {
                            foreach (Enclosure enc in olditem.Enclosures)
                            {
                                var j = newitem.Enclosures.IndexOf(enc);

                                if (j != -1)
                                {
                                    var oldEnc = newitem.Enclosures[j];
                                    enc.Downloaded = oldEnc.Downloaded;
                                }
                                else
                                {
                                    if (ReferenceEquals(newitem.Enclosures, GetList<IEnclosure>.Empty))
                                    {
                                        newitem.Enclosures = new List<IEnclosure>();
                                    }
                                    newitem.Enclosures.Add(enc);
                                }
                            }
                        }

                        oldItems.RemoveAt(index);
                        oldItems.Add(newitem);
                        RelationCosmosRemove(olditem);
                        //	removedOldItems.Add(olditem); 
                    }
                } //foreach

                //remove old objects from relation cosmos and add newly downloaded items to relationcosmos
                //FeedSource.RelationCosmosRemoveRange(removedOldItems); 
                RelationCosmosAddRange(receivedNewItems);
            } //lock

            if (onlyKeepNewItems)
            {
                return newItems;
            }
            
            return oldItems;
        }

        /// <summary>
        /// Posts a comment in reply to an item using either NNTP or the CommentAPI 
        /// </summary>
        /// <param name="url">The URL to post the comment to</param>
        /// <param name="item2post">An RSS item that will be posted to the website</param>
        /// <param name="inReply2item">An RSS item that is the post parent</param>		
        /// <exception cref="WebException">If an error occurs when the POSTing the 
        /// comment</exception>
        public void PostComment(string url, INewsItem item2post, INewsItem inReply2item)
        {
            if (inReply2item.CommentStyle == SupportedCommentStyle.CommentAPI)
            {
                RssParserInstance.PostCommentViaCommentAPI(url, item2post, inReply2item,
                                                           GetFeedCredentials(inReply2item.Feed));
            }
            else if (inReply2item.CommentStyle == SupportedCommentStyle.NNTP)
            {
                NntpParser.PostCommentViaNntp(item2post, inReply2item, GetNntpServerCredentials(inReply2item.Feed));
            }
        }

        /// <summary>
        /// Posts a new item to a feed (currently only NNTP feeds) 
        /// </summary>
        /// <remarks>How about Atom feed posting?</remarks>
        /// <param name="item2post">An RSS item that will be posted to the website/NNTP Group</param>
        /// <param name="postTarget">An NewsFeed as the post target</param>		
        /// <exception cref="WebException">If an error occurs when the POSTing the 
        /// comment</exception>
        public void PostComment(INewsItem item2post, INewsFeed postTarget)
        {
            if (item2post.CommentStyle == SupportedCommentStyle.NNTP)
            {
                NntpParser.PostCommentViaNntp(item2post, postTarget, GetNntpServerCredentials(postTarget));
            }
        }

        private static object GetSharedPropertyValue(ISharedProperty instance, string propertyName)
        {
            switch (propertyName)
            {
                case "maxitemage":
                    return instance.maxitemage;
                case "downloadenclosures":
                    return instance.downloadenclosures;
                case "downloadenclosuresSpecified":
                    return instance.downloadenclosuresSpecified;
                case "enclosurealert":
                    return instance.enclosurealert;
                case "enclosurealertSpecified":
                    return instance.enclosurealertSpecified;
                case "enclosurefolder":
                    return instance.enclosurefolder;
                case "listviewlayout":
                    return instance.listviewlayout;
                case "markitemsreadonexit":
                    return instance.markitemsreadonexit;
                case "markitemsreadonexitSpecified":
                    return instance.markitemsreadonexitSpecified;
                case "refreshrate":
                    return instance.refreshrate;
                case "refreshrateSpecified":
                    return instance.refreshrateSpecified;
                case "stylesheet":
                    return instance.stylesheet;
                default:
                    Debug.Assert(true, "unknown shared property name: " + propertyName);
                    break;
            }
            return null;
        }

        private static void SetSharedPropertyValue(ISharedProperty instance, string propertyName, object value)
        {
            switch (propertyName)
            {
                case "maxitemage":
                    instance.maxitemage = value as string;
                    break;
                case "downloadenclosures":
                    instance.downloadenclosures = (bool) value;
                    break;
                case "downloadenclosuresSpecified":
                    instance.downloadenclosuresSpecified = (bool) value;
                    break;
                case "enclosurealert":
                    instance.enclosurealert = (bool) value;
                    break;
                case "enclosurealertSpecified":
                    instance.enclosurealertSpecified = (bool) value;
                    break;
                case "enclosurefolder":
                    instance.enclosurefolder = value as string;
                    break;
                case "listviewlayout":
                    instance.listviewlayout = value as string;
                    break;
                case "markitemsreadonexit":
                    instance.markitemsreadonexit = (bool) value;
                    break;
                case "markitemsreadonexitSpecified":
                    instance.markitemsreadonexitSpecified = (bool) value;
                    break;
                case "refreshrate":
                    instance.refreshrate = (int) value;
                    break;
                case "refreshrateSpecified":
                    instance.refreshrateSpecified = (bool) value;
                    break;
                case "stylesheet":
                    instance.stylesheet = value as string;
                    break;
                default:
                    Debug.Assert(true, "unknown shared property name: " + propertyName);
                    break;
            }
        }

        #region RelationCosmos management

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="excludeItemsList"></param>
        /// <returns></returns>
        public ICollection<INewsItem> GetItemsWithIncomingLinks(INewsItem item, IList<INewsItem> excludeItemsList)
        {
            if (buildRelationCosmos)
                return relationCosmos.GetIncoming(item, excludeItemsList);
            
            return new List<INewsItem>();
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="since"></param>
        /// <returns></returns>
        public IList<INewsItem> GetItemsWithIncomingLinks(string url, DateTime since)
        {
            //make sure we are using the interned string for lookup
            url = RelationCosmos.RelationCosmos.UrlTable.Add(url);

            if (buildRelationCosmos)
                return relationCosmos.GetIncoming<INewsItem>(url, since);
            
            return new List<INewsItem>();
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="excludeItemsList"></param>
        /// <returns></returns>
        public ICollection<INewsItem> GetItemsFromOutGoingLinks(INewsItem item, IList<INewsItem> excludeItemsList)
        {
            if (buildRelationCosmos)
                return relationCosmos.GetOutgoing(item, excludeItemsList);
            
            return new List<INewsItem>();
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="excludeItemsList"></param>
        /// <returns></returns>
        public bool HasItemAnyRelations(INewsItem item, IList<INewsItem> excludeItemsList)
        {
            if (buildRelationCosmos)
                return relationCosmos.HasIncomingOrOutgoing(item, excludeItemsList);
            
            return false;
        }

        /// <summary>
        /// Internal used accessor
        /// </summary>
        /// <param name="relation"></param>
        internal static void RelationCosmosAdd<T>(T relation)
            where T : IRelation
        {
            if (buildRelationCosmos)
                relationCosmos.Add(relation);
            else
                return;
        }

        internal static void RelationCosmosAddRange<T>(IEnumerable<T> relations)
            where T : IRelation
        {
            if (buildRelationCosmos)
                relationCosmos.AddRange(relations);
            else
                return;
        }

        internal static void RelationCosmosRemove<T>(T relation)
            where T : IRelation
        {
            if (buildRelationCosmos)
                relationCosmos.Remove(relation);
            else
                return;
        }

        internal static void RelationCosmosRemoveRange<T>(IList<T> relations)
            where T : IRelation
        {
            if (buildRelationCosmos)
                relationCosmos.RemoveRange(relations);
            else
                return;
        }

        #endregion

        #region ReceivingNewsChannel Manangement		

        /// <summary>
        /// Gets the receiving news channel.
        /// </summary>
        /// <value>The receiving news channel services.</value>
        internal static NewsChannelServices ReceivingNewsChannelServices
        {
            get { return receivingNewsChannel; }
        }

        /// <summary>
        /// Register INewsChannel processing services 
        /// </summary>
        public void RegisterReceivingNewsChannel(INewsChannel channel)
        {
            // We use an instance method to register services.
            // So we are able to change later the internal processing to a non-static
            // class/instance if required.
            receivingNewsChannel.RegisterNewsChannel(channel);
        }

        /// <summary>
        /// Unregister INewsChannel processing services 
        /// </summary>
        public void UnregisterReceivingNewsChannel(INewsChannel channel)
        {
            // We use an instance method to register services.
            // So we are able to change later the internal processing to a non-static
            // class/instance if required.
            receivingNewsChannel.UnregisterNewsChannel(channel);
        }

        #endregion

        #region GetFailureContext()

        /// <summary>
        /// Populates a hashtable with additional feed infos 
        /// we need to provide useful error infos to a user.
        /// It is only fully populated, if we have it allready read from cache.
        /// </summary>
        /// <remarks>
        /// Currently we populate the following keys:
        /// * TECH_CONTACT	(opt.; mail address from: 'webMaster' (RSS) or 'errorReportsTo' (Atom) )
        /// * PUBLISHER			(opt.; mail address from: 'managingEditor' (RSS)
        /// * PUBLISHER_HOMEPAGE	(opt.; additional info link)
        /// * GENERATOR			(opt.; generator software)
        /// * FULL_TITLE			(allways there; category and title as it is used in the UI)
        /// * FAILURE_OBJECT 	(allways there; NewsFeed | nntpFeed)
        /// </remarks>
        /// <param name="feedUri">Uri</param>
        /// <returns>Hashtable</returns>
        public Hashtable GetFailureContext(Uri feedUri)
        {
            INewsFeed f;
            if (feedUri == null || !feedsTable.TryGetValue(feedUri.CanonicalizedUri(), out f))
                return new Hashtable();
            return GetFailureContext(f);
        }


        /// <summary>
        /// Overloaded.
        /// </summary>
        /// <param name="feedUri">The feed URI.</param>
        /// <returns></returns>
        public Hashtable GetFailureContext(string feedUri)
        {
            if (feedUri == null)
                return new Hashtable();
            if (feedsTable.ContainsKey(feedUri))
                return GetFailureContext(feedsTable[feedUri]);
            
            return new Hashtable();
        }

        /// <summary>
        /// Overloaded.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Hashtable GetFailureContext(INewsFeed f)
        {
            if (f == null)
            {
                // how about nntpFeeds? They are within the FeedsTable (with different class type)?
                return new Hashtable();
            }

            FeedInfo fi = null;
            lock (itemsTable)
            {
                if (itemsTable.ContainsKey(f.link))
                {
                    fi = itemsTable[f.link] as FeedInfo;
                }
            }

            return GetFailureContext(f, fi);
        }

        /// <summary>
        /// Overloaded.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static Hashtable GetFailureContext(INewsFeed f, IFeedDetails fi)
        {
            var context = new Hashtable();

            if (f == null)
            {
                return context;
            }

            context.Add("FULL_TITLE", (f.category ?? String.Empty) + CategorySeparator + f.title);
            context.Add("FAILURE_OBJECT", f);

            if (fi == null)
                return context;

            context.Add("PUBLISHER_HOMEPAGE", fi.Link);

            var xe = RssHelper.GetOptionalElement(fi.OptionalElements, "managingEditor", String.Empty);
            if (xe != null)
                context.Add("PUBLISHER", xe.InnerText);

            xe = RssHelper.GetOptionalElement(fi.OptionalElements, "webMaster", String.Empty);
            if (xe != null)
            {
                context.Add("TECH_CONTACT", xe.InnerText);
            }
            else
            {
                xe = RssHelper.GetOptionalElement(fi.OptionalElements, "errorReportsTo", "http://webns.net/mvcb/");
                if (xe != null && xe.Attributes["resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"] != null)
                    context.Add("TECH_CONTACT",
                                xe.Attributes["resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"].InnerText);
            }

            xe = RssHelper.GetOptionalElement(fi.OptionalElements, "generator", String.Empty);
            if (xe != null)
                context.Add("GENERATOR", xe.InnerText);

            return context;
        }

        #endregion

        #region category manipulation methods 

        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The name of the category</param>
        /// <returns>The INewsFeedCategory instance that will actually be used to represent the category</returns>
        public virtual INewsFeedCategory AddCategory(string cat)
        {
            if (StringHelper.EmptyTrimOrNull(cat))
                return null;

            if (categories.ContainsKey(cat))
                return categories[cat];

            var ancestors = category.GetAncestors(cat);

            //create rest of category hierarchy if it doesn't exist
            for (var i = ancestors.Count; i-- > 0;)
            {
                INewsFeedCategory c;

                if (!categories.TryGetValue(ancestors[i], out c))
                {
                    categories.Add(ancestors[i], new category(ancestors[i]));
                }
            }

            INewsFeedCategory newCategory = new category(cat)
                                                {
                                                    parent = (ancestors.Count == 0 ? null : categories[ancestors[ancestors.Count - 1]])
                                                };

            categories.Add(cat, newCategory);
            readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(categories);

            return newCategory;
        }

        /// <summary>
        /// Adds a category to the list of feed categories known by this feed handler
        /// </summary>
        /// <param name="cat">The category to add</param>
        /// <returns>The INewsFeedCategory instance that will actually be used to represent the category</returns>
        public virtual INewsFeedCategory AddCategory(INewsFeedCategory cat)
        {
            if (categories.ContainsKey(cat.Value))
            {
                return categories[cat.Value];
            }
            
            categories.Add(cat.Value, cat);
            return cat;
        }


        /// <summary>
        /// Tests whether this category name exists in the FeedSource. 
        /// </summary>
        /// <param name="cat">The name of the category</param>
        /// <returns>True if this category is used by the FeedSource</returns>
        public virtual bool HasCategory(string cat)
        {
            if (cat == null)
            {
                return false;
            }

            return categories.ContainsKey(cat);
        }


        /// <summary>
        /// Returns a ReadOnlyDictionary containing the list of categories used by the FeedSource
        /// </summary>
        /// <returns>A read-only dictionary of categories</returns>
        public virtual ReadOnlyDictionary<string, INewsFeedCategory> GetCategories()
        {
            readonly_categories = readonly_categories ?? new ReadOnlyDictionary<string, INewsFeedCategory>(categories);
            return readonly_categories;
        }

        /// <summary>
        /// Deletes a category from the FeedSource. This process includes deleting all subcategories and the 
        /// corresponding feeds. 
        /// </summary>
        /// <remarks>Note that this does not fix up the references to this category in the feed list nor does it 
        /// fix up the references to this category in its parent and child categories.</remarks>
        /// <param name="cat"></param>
        public virtual void DeleteCategory(string cat)
        {
            if (!StringHelper.EmptyTrimOrNull(cat) && categories.ContainsKey(cat))
            {
                IList<string> categories2remove = GetChildCategories(cat);
                categories2remove.Add(cat);

                //remove category and all its subcategories
                lock (categories)
                {
                    foreach (var c in categories2remove)
                    {
                        categories.Remove(c);
                    }
                }

                //remove feeds in deleted categories and subcategories
                var feeds2delete =
                    from f in feedsTable.Values
                    where categories2remove.Contains(f.category)
                    select f.link;

                var feeds2remove = feeds2delete.ToArray();

                lock (feedsTable)
                {
                    foreach (var feedUrl in feeds2remove)
                    {
                        feedsTable.Remove(feedUrl);
                    }
                }

                readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(categories);
            } // if (!StringHelper.EmptyTrimOrNull(cat) && categories.ContainsKey(cat))
        }


           /// <summary>
        /// Changes the category of a particular INewsFeedCategory. This method should be used when moving a category. Also 
        /// changes the category of call child feeds and categories. 
        /// </summary>        
        /// <param name="cat">The category whose parent category to change</param>
        /// <param name="parent">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized. If this parameter is null then the parent is considered to be the root node.</param>
        public virtual void ChangeCategory(INewsFeedCategory cat, INewsFeedCategory parent)
        {
            if (cat == null)
                throw new ArgumentNullException("cat");

            if (this.categories.ContainsKey(cat.Value))
            {
                string parentPath = parent == null ? String.Empty : parent.Value;
                int index = cat.Value.LastIndexOf(FeedSource.CategorySeparator);
                index = (index == -1 ? 0 : index + 1);

                List<INewsFeedCategory> categories2move = this.GetDescendantCategories(cat);
                categories2move.Add(cat);

                foreach (INewsFeedCategory c in categories2move)
                {
                    var feeds2move = from f in this.feedsTable.Values
                                     where c.Value.Equals(f.category)
                                     select f;
                    
                    string newCategory = parentPath + (parentPath.Equals(String.Empty) ? String.Empty : FeedSource.CategorySeparator)
                            + c.Value.Substring(index);

                    if (feeds2move.Count() > 0)
                    {
                        foreach (INewsFeed feed in feeds2move)
                        {
                            feed.category = newCategory;
                        }
                    }

                    this.categories.Remove(c.Value);                 
                    c.Value = newCategory;                     
                    this.categories.Add(c.Value, c); 

                }//foreach(string c...)
            }//if (this.categories.ContainsKey(cat.Value))
        }

        /// <summary>
        /// Changes the category of a particular INewsFeed. This method should be used instead of setting
        /// the category property of the INewsFeed instance. 
        /// </summary>
        /// <param name="feed">The newsfeed whose category to change</param>
        /// <param name="cat">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized</param>
        public virtual void ChangeCategory(INewsFeed feed, string cat)
        {
            if (feed == null)
                throw new ArgumentNullException("feed");

            feed.category = cat;
        }


        /// <summary>
        /// Changes the category of a particular INewsFeed. This method should be used instead of setting
        /// the category property of the INewsFeed instance. 
        /// </summary>
        /// <param name="feed">The newsfeed whose category to change</param>
        /// <param name="cat">The new category for the feed. If this value is null then the feed is no longer 
        /// categorized</param>
        public virtual void ChangeCategory(INewsFeed feed, INewsFeedCategory cat)
        {
            if (feed == null)
                throw new ArgumentNullException("feed");

            feed.category = cat != null ? cat.Value : null;
        }

        /// <summary>
        /// Renames the specified category. This method also renames the subcategories and the categories on the 
        /// INewsFeed instances that are in the hierarchy.
        /// </summary>        
        /// <remarks>This method assumes that the caller will rename categories on INewsFeed instances directly instead
        /// of having this method do it automatically.</remarks>
        /// <param name="oldName">The old name of the category</param>
        /// <param name="newName">The new name of the category</param>        
        public virtual void RenameCategory(string oldName, string newName)
        {
            if (StringHelper.EmptyTrimOrNull(oldName))
                throw new ArgumentNullException("oldName");

            if (StringHelper.EmptyTrimOrNull(newName))
                throw new ArgumentNullException("newName");

            if (this.categories.ContainsKey(oldName))
            {
                INewsFeedCategory cat = categories[oldName];
                List<INewsFeedCategory> categories2rename = this.GetDescendantCategories(cat);
                categories2rename.Add(cat);

                foreach (INewsFeedCategory c in categories2rename)
                {
                    var feeds2rename = from f in this.feedsTable.Values
                                     where c.Value.Equals(f.category)
                                     select f;

                    if (feeds2rename.Count() > 0)
                    {                        
                        foreach (INewsFeed feed in feeds2rename)
                        {
                            feed.category = newName + (c.Value.Equals(oldName) ? String.Empty : 
                                FeedSource.CategorySeparator + c.Value.Substring(oldName.Length + 1) ) ;                          
                        }
                    }

                    this.categories.Remove(c.Value);
                    c.Value = newName + (c.Value.Equals(oldName) ? String.Empty :
                                FeedSource.CategorySeparator + c.Value.Substring(oldName.Length + 1));                   
                    this.categories.Add(c.Value, c);
                }//foreach(string c...)
            }//if (this.categories.ContainsKey(oldName))
        }

        /// <summary>
        /// Helper function that gets the parent category object of the named category
        /// </summary>
        /// <param name="category">The name of the category</param>
        /// <returns>The parent category of the specified category</returns>
        private INewsFeedCategory GetParentCategory(string category)
        {
            var index = category.LastIndexOf(CategorySeparator);
            INewsFeedCategory c = null;

            if (index != -1)
            {
                var parentName = category.Substring(0, index);
                categories.TryGetValue(parentName, out c);
            }

            return c;
        }

        /// <summary>
        /// Helper function that gets the child categories of the named category
        /// </summary>
        /// <param name="name">The name of the category</param>
        /// <returns>The list of child categories</returns>
        protected List<string> GetChildCategories(string name)
        {
            var list = new List<string>();

            foreach (var c in categories.Values)
            {
                if (c.Value.StartsWith(name + CategorySeparator))
                {
                    list.Add(c.Value);
                }
            }

            return list;
        }


        /// <summary>
        /// Helper function that gets the descendant categories of the named category
        /// </summary>
        /// <param name="parent">The parent category</param>
        /// <returns>The list of descendant categories</returns>
        protected List<INewsFeedCategory> GetDescendantCategories(INewsFeedCategory parent)
        {
            var list = new List<INewsFeedCategory>();

            foreach (var c in categories.Values)
            {
                if (c.Value.StartsWith(parent.Value + CategorySeparator))
                {
                    list.Add(c);
                }
            }

            return list;
        }

        #endregion

        #region Feed manipulation methods


        /// <summary>
        /// Helper function that gets the descendant feeds of the named category
        /// </summary>
        /// <param name="cat">The category</param>
        /// <returns>The list of descendant INewsFeed</returns>
        public IEnumerable<INewsFeed> GetDescendantFeeds(INewsFeedCategory cat)
        {
            if (cat == null)
            {
                return new List<INewsFeed>();
            }

            var feeds2return = from f in feedsTable.Values
                                   where f.category != null && (f.category.Equals(cat.Value) || f.category.StartsWith(cat.Value + FeedSource.CategorySeparator))
                                   select f;
            
            return new List<INewsFeed>(feeds2return); 
        }


        /// <summary>
        /// Invoked when a NewsFeed owned by this FeedSource changes in a way that 
        /// needs to be communicated to NewsGator Online. 
        /// </summary>
        /// <param name="sender">the NewsFeed</param>
        /// <param name="e">information on the property that changed</param>
        protected virtual void OnNewsFeedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //DOES NOTHING BY DEFAULT
        }

        /// <summary>
        /// Adds a feed and associated FeedInfo object to the FeedsTable and itemsTable. 
        /// Any existing feed objects are replaced by the new objects. 
        /// </summary>
        /// <param name="f">The NewsFeed object </param>
        /// <returns>The actual INewsFeed instance that will be used to represent this feed subscription</returns>
        public virtual INewsFeed AddFeed(INewsFeed f)
        {
            return AddFeed(f, null);
        }

        /// <summary>
        /// Adds a feed and associated FeedInfo object to the FeedsTable and itemsTable. 
        /// Any existing feed objects are replaced by the new objects. 
        /// </summary>
        /// <param name="feed">The NewsFeed object </param>
        /// <param name="feedInfo">The FeedInfo object</param>
        /// <returns>The actual INewsFeed instance that will be used to represent this feed subscription</returns>
        public virtual INewsFeed AddFeed(INewsFeed feed, FeedInfo feedInfo)
        {
            if (feed != null)
            {
                lock (feedsTable)
                {
                    if (feedsTable.ContainsKey(feed.link))
                    {
                        feedsTable.Remove(feed.link);
                    }
                    feed.owner = this;
                    feedsTable.Add(feed.link, feed);
                }
            }

            if (feedInfo != null && feed != null)
            {
                lock (itemsTable)
                {
                    if (itemsTable.ContainsKey(feed.link))
                    {
                        itemsTable.Remove(feed.link);
                    }
                    itemsTable.Add(feed.link, feedInfo);
                }
            }
            readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(feedsTable);
            return feed;
        }

        /// <summary>
        /// Removes all information related to a feed from the FeedSource.   
        /// </summary>
        /// <remarks>If no feed with that URL exists then nothing is done.</remarks>
        /// <param name="feedUrl">The URL of the feed to delete. </param>
        /// <exception cref="ApplicationException">If an error occured while 
        /// attempting to delete the cached feed. Examine the InnerException property 
        /// for details</exception>
        public virtual void DeleteFeed(string feedUrl)
        {
            if (!feedsTable.ContainsKey(feedUrl))
            {
                return;
            }

            var f = feedsTable[feedUrl];
            feedsTable.Remove(feedUrl);

            if (itemsTable.ContainsKey(feedUrl))
            {
                itemsTable.Remove(feedUrl);
            }

            SearchHandler.IndexRemove(f.id);
            if (enclosureDownloader != null)
                enclosureDownloader.CancelPendingDownloads(feedUrl);

            try
            {
                CacheHandler.RemoveFeed(f);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message, e);
            }
            readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(feedsTable);
        }

        /// <summary>
        /// Returns a read-only dictionary of feeds managed by this FeedSource
        /// </summary>
        /// <returns></returns>
        public ReadOnlyDictionary<string, INewsFeed> GetFeeds()
        {
            readonly_feedsTable = readonly_feedsTable ?? new ReadOnlyDictionary<string, INewsFeed>(feedsTable);
            return readonly_feedsTable;
        }

        /// <summary>
        /// Tests whether this feed is currently subscribed to. 
        /// </summary>
        /// <param name="feedUrl">The URL of the feed</param>
        /// <returns>True if this feed is used by the FeedSource</returns>
        public virtual bool IsSubscribed(string feedUrl)
        {
            if (StringHelper.EmptyTrimOrNull(feedUrl))
            {
                return false;
            }

            return feedsTable.ContainsKey(feedUrl);
        }

        /// <summary>
        /// Deletes all subscribed feeds and categories 
        /// </summary>
        public virtual void DeleteAllFeedsAndCategories()
        {
            feedsTable.Clear();
            categories.Clear();
            readonly_categories = new ReadOnlyDictionary<string, INewsFeedCategory>(categories);
            readonly_feedsTable = new ReadOnlyDictionary<string, INewsFeed>(feedsTable);
        }

        #endregion

        #region abstract methods 

        /// <summary>
        /// Loads the feedlist from the FeedLocation. 
        ///</summary>
        public abstract void LoadFeedlist();

        /// <summary>
        /// Loads the feedlist from the feedlocation and use the input feedlist to bootstrap the settings. The input feedlist
        /// is also used as a fallback in case the FeedLocation is inaccessible (e.g. we are in offline mode and the feed location
        /// is on the Web). 
        /// </summary>
        /// <param name="feedlist">The feed list to provide the settings for the feeds downloaded by this FeedSource</param>
        public abstract void BootstrapAndLoadFeedlist(feeds feedlist);

        #endregion
    }

    #region NewsFeedProperty enum

    /// <summary>
    /// Defines all storage relevant NewsFeed properties. On any change
    /// of a NewsFeed property, that feed requires to be saved with the
    /// subscriptions list, to the cache or re-indexed!
    /// </summary>
    [Flags]
    public enum NewsFeedProperty
    {
        None = 0,
        /// <summary>Requires subscriptions update/save, re-index</summary>
        FeedLink = 0x1,
        /// <summary>Requires re-index</summary>
        FeedUrl = 0x2,
        /// <summary>Requires subscriptions update/save, re-index</summary>
        FeedTitle = 0x4,
        /// <summary>Requires subscriptions update/save, re-index</summary>
        FeedCategory = 0x8,
        /// <summary>Requires re-index</summary>
        FeedDescription = 0x10,
        /// <summary>Requires cache update/save, re-index</summary>
        FeedType = 0x20,
        /// <summary>Requires subscriptions update/save, re-index</summary>
        FeedItemsDeleteUndelete = 0x40,
        /// <summary>Requires cache update/save</summary>
        FeedItemFlag = 0x80,
        /// <summary>Requires subscriptions and cache update/save</summary>
        FeedItemReadState = 0x100,
        /// <summary>Requires cache update/save</summary>
        FeedItemCommentCount = 0x200,
        /// <summary>Requires subscriptions update/save</summary>
        FeedMaxItemAge = 0x400,
        /// <summary>Requires cache update/save</summary>
        FeedItemWatchComments = 0x800,
        /// <summary>Requires subscriptions update/save</summary>
        FeedRefreshRate = 0x1000,
        /// <summary>Requires subscriptions update/save</summary>
        FeedCacheUrl = 0x2000,
        /// <summary>Requires subscriptions update/save</summary>
        FeedAdded = 0x4000,
        /// <summary>Requires subscriptions update/save</summary>
        FeedRemoved = 0x8000,
        /// <summary>Requires subscriptions update/save</summary>
        FeedCategoryRemoved = 0x10000,
        /// <summary>Requires subscriptions update/save</summary>
        FeedCategoryAdded = 0x20000,
        /// <summary>Requires cache update/save </summary>
        FeedCredentials = 0x40000,
        /// <summary>Requires subscriptions update/save </summary>
        FeedAlertOnNewItemsReceived = 0x80000,
        /// <summary>Requires subscriptions update/save </summary>
        FeedMarkItemsReadOnExit = 0x100000,
        /// <summary>Requires subscriptions update/save </summary>
        FeedStylesheet = 0x200000,
        /// <summary>Requires cache update/save</summary>
        FeedItemNewCommentsRead = 0x400000,
        /// <summary> General change, requires subscriptions update/save</summary>
        General = 0x8000000,
    }

    //	/// <summary>
    //	/// Defines all index relevant NewsItem properties, 
    //	/// that are part of the lucene search index. On any change
    //	/// of a NewsItem property, that NewsItem requires to be re-indexed!
    //	/// </summary>
    //	public enum NewsItemProperty {
    //		ItemAuthor,
    //		ItemTitle,
    //		ItemLink,
    //		ItemDate,
    //		ItemTopic,
    //		Other,
    //	}

    #endregion

    /// <summary>
    /// Interface represents extended information about a particular feed
    /// (internal use only)
    /// </summary>
    internal interface FeedDetailsInternal : IFeedDetails
    {
        /* new Dictionary<XmlQualifiedName, string> OptionalElements { get; }
        List<INewsItem> ItemsList { get; set; }
        string Id { get; set; }
        void WriteTo(XmlWriter writer);
        void WriteTo(XmlWriter writer, bool noDescriptions); */

        string FeedLocation { get; set; }
        void WriteItemContents(BinaryReader reader, BinaryWriter writer);
        void WriteTo(XmlWriter writer, bool noDescriptions);
    }

    /// <summary>
    /// Get informations about the size of an object or item
    /// </summary>
    public interface ISizeInfo
    {
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <returns></returns>
        int GetSize();

        /// <summary>
        /// Gets the size details.
        /// </summary>
        /// <returns></returns>
        string GetSizeDetails();
    }

    #region RssBanditXmlNamespaceResolver 

    /// <summary>
    /// Helper class used for treating v1.2.* RSS Bandit feedlist.xml files as RSS Bandit v1.3.* 
    /// subscriptions.xml files
    /// </summary>
    internal class RssBanditXmlNamespaceResolver : XmlNamespaceManager
    {
        public RssBanditXmlNamespaceResolver() : base(new NameTable())
        {
        }

        public override void AddNamespace(string prefix, string uri)
        {
            if (uri == NamespaceCore.Feeds_v2003)
            {
                uri = NamespaceCore.Feeds_vCurrent;
            }
            base.AddNamespace(prefix, uri);
        }
    }

    #endregion

    #region RssBanditXmlValidatingReader 

    /// <summary>
    /// Helper class used for treating v1.2.* RSS Bandit feedlist.xml files as RSS Bandit v1.3.* 
    /// subscriptions.xml files
    /// </summary>
    internal class RssBanditXmlReader : XmlTextReader
    {
        public RssBanditXmlReader(Stream s, XmlNodeType nodeType, XmlParserContext context) : base(s, nodeType, context)
        {
        }

        public RssBanditXmlReader(string s, XmlNodeType nodeType, XmlParserContext context) : base(s, nodeType, context)
        {
        }

        public override string Value
        {
            get
            {
                if ((NodeType == XmlNodeType.Attribute) &&
                    (base.Value == NamespaceCore.Feeds_v2003))
                {
                    return NamespaceCore.Feeds_vCurrent;
                }

                return base.Value;
            }
        }
    } //class 

    #endregion
}