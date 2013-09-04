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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.Search; 
using NewsComponents.Utils;

namespace NewsComponents
{

	#region FeedSourceEntry
	/// <summary>
	/// Class used to store the values for visual representation:
	/// name and position in the tree/sources list.
	/// </summary>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	public class FeedSourceEntry : IComparable<FeedSourceEntry>
	{
		/// <summary>
		/// Gets or (internal) sets the source.
		/// </summary>
		/// <value>The source.</value>
		[XmlIgnore]
		public FeedSource Source { get; internal set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedSourceEntry"/> class.
		/// </summary>
		public FeedSourceEntry() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedSourceEntry"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="source">The source.</param>
		/// <param name="name">The name.</param>
		/// <param name="ordinal">The ordinal.</param>
		public FeedSourceEntry(int id, FeedSource source, string name, int ordinal)
		{
			ID = id;
			Name = name;
			Ordinal = ordinal;
			Source = source;
		}

        /// <summary>
        /// Gets or sets whether favicons have been downloaded for the source. 
        /// </summary>
        [XmlIgnore]
        public bool FaviconsDownloaded { get; set; }

		/// <summary>
		/// Gets or sets the name of the source.
		/// </summary>
		/// <value>The name.</value>
		[XmlAttribute("name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the ordinal position.
		/// </summary>
		/// <value>The ordinal.</value>
		[XmlAttribute("ordinal")]
		public int Ordinal { get; set; }

		/// <summary>
		/// Gets or sets the ID.
		/// </summary>
		/// <value>The ID.</value>
		[XmlAttribute("id")]
		public int ID { get; set; }

		private FeedSourceType _serializedSourceType = FeedSourceType.Unknown;
		/// <summary>
		/// Gets or sets the type of the source.
		/// </summary>
		/// <value>The type of the source.</value>
		[XmlAttribute("type")]
		public FeedSourceType SourceType
		{
			get
			{
				if (Source != null)
					return Source.Type;
				return _serializedSourceType;
			}
			set { _serializedSourceType = value; }
		}

        /// <summary>
        /// The identifier for the list view layout used for the unread items folder associated with this feed source. 
        /// </summary>
        [XmlAttribute("unread-items-column-layout")]
        public string UnreadItemsColumnLayoutId{ get; set; }

		private StringProperties _serializedSourceProperties = new StringProperties();
		/// <summary>
		/// Gets or sets the properties collection.
		/// </summary>
		/// <value>The properties.</value>
		[XmlElement("properties")]
		public StringProperties Properties
		{
			get
			{
				return _serializedSourceProperties;
			}
			set
			{
				_serializedSourceProperties = value;
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return Name;
		}

		#region IComparable<FeedSourceEntry> Members

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		int IComparable<FeedSourceEntry>.CompareTo(FeedSourceEntry other)
		{
			if (other == null) return 1;
			if (ReferenceEquals(other, this) ||
				Ordinal == other.Ordinal) return 0;
			if (Ordinal > other.Ordinal) return 1;
			return -1;
		}

		#endregion
		
	}

	/// <summary>
	/// String property container
	/// </summary>
	[Serializable]
	public class StringProperties: KeyItemCollection<string, string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StringProperties"/> class.
		/// </summary>
		public StringProperties() {}
		/// <summary>
		/// Initializes a new instance of the <see cref="StringProperties"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public StringProperties(int capacity): base(capacity) {}
		/// <summary>
		/// Initializes a new instance of the <see cref="StringProperties"/> class.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <param name="context">The context.</param>
		protected StringProperties(SerializationInfo info, StreamingContext context) : 
			base(info, context) { }
	}

	/// <summary>
	/// Feed sources serializable root class
	/// </summary>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	[XmlRoot("feedsources", Namespace = NamespaceCore.Feeds_vCurrent, IsNullable = false)]
	public class SerializableFeedSources
	{
		/// <remarks/>
		[XmlElement("source", Type = typeof(FeedSourceEntry), IsNullable = false)]
		//public ArrayList List = new ArrayList();
		public List<FeedSourceEntry> List = new List<FeedSourceEntry>();

		/// <summary>
		/// Gets or sets the last ID.
		/// </summary>
		/// <value>The last used ID.</value>
		[XmlAttribute("last-id")]
		public int LastID { get; set; }
	}
	#endregion

	/// <summary>
	/// Manages a list of feed sources
	/// </summary>
	public class FeedSourceManager
	{


        // logging/tracing:
        private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(FeedSourceManager));

        #region Nested type: PropertyKey

        /// <summary>
		/// Gets the keys of the common <see cref="FeedSource"/> properties dictionary
		/// </summary>
		public static class PropertyKey
		{
			/// <summary>
			/// Property key for the domain part of credentials
			/// </summary>
			public const string Domain = "domain";
			/// <summary>
			/// Property key for the user name part of credentials
			/// </summary>
			public const string UserName = "user";
			/// <summary>
			/// Property key for the password part of credentials
			/// </summary>
			public const string Password = "pwd";
        }

        #endregion 

        readonly Dictionary<int, FeedSourceEntry> _feedSources = new Dictionary<int, FeedSourceEntry>();

		/// <summary>
		/// Gets the ordered feed sources. Used
		/// to build the initial tree root entries.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<FeedSourceEntry> Sources
		{
			get {
				foreach (FeedSourceEntry entry in _feedSources.Values)
				{
					yield return entry;
				}
			}
		}

		/// <summary>
		/// Gets the ordered feed sources. Used
		/// to build the initial tree root entries.
		/// </summary>
		/// <returns></returns>
		public List<FeedSourceEntry> GetOrderedFeedSources() {
			List<FeedSourceEntry> sources = new List<FeedSourceEntry>(_feedSources.Values);
			sources.Sort(Comparer<FeedSourceEntry>.Default);
			return sources;
		}

		/// <summary>
		/// Can be used to call methods or set properties on each
		/// <see cref="FeedSource"/>.
		/// </summary>
		public void ForEach(Action<FeedSource> action)
		{
            //copy to array so we don't get collection modified exception if an new source is added or one is removed
            FeedSourceEntry[] entries = _feedSources.Values.ToArray(); 

			foreach (FeedSourceEntry entry in entries) { 
				action(entry.Source);
			}
		}

		/// <summary>
		/// Adds the specified new source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">If source or name are null</exception>
		/// <exception cref="ArgumentOutOfRangeException">If name is empty</exception>
		/// <exception cref="InvalidOperationException">If source with provided name yet exists</exception>
		public FeedSourceEntry Add(FeedSource source, string name)
		{
			source.ExceptionIfNull("source");
			name.ExceptionIfNullOrEmpty("name");
			
			if (Contains(name))
				throw new InvalidOperationException("Entry with name '" + name + "' already exists");

			source.sourceID = UniqueKey;
			FeedSourceEntry fs = new FeedSourceEntry(source.sourceID, source, name, _feedSources.Count);
			_feedSources.Add(fs.ID, fs);
			return fs;
		}

		/// <summary>
		/// Adds a new source with the specified name and properties.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="properties">The properties.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">If name is null</exception>
		/// <exception cref="ArgumentOutOfRangeException">If name is empty</exception>
		/// <exception cref="InvalidOperationException">If source with provided name yet exists</exception>
		public FeedSourceEntry Add(string name, FeedSourceType type, IDictionary properties)
		{
			name.ExceptionIfNullOrEmpty("name");
			
			if (Contains(name))
				throw new InvalidOperationException("Entry with name '" + name + "' already exists");
			
			int id = UniqueKey;
			FeedSource source = FeedSource.CreateFeedSource(id, type, 
				CreateSubscriptionLocation(id, type, properties));
			FeedSourceEntry fse = new FeedSourceEntry(id, source, name, _feedSources.Count);
			//fse.Properties =
			_feedSources.Add(fse.ID, fse);
			return fse;
		}

		/// <summary>
		/// Clears this instance (removing all feed sources).
		/// </summary>
		public void Clear()
		{
			_feedSources.Clear();
		}

        /// <summary>
        /// Determines whether there is a Feed Source with the specified name
        /// </summary>
        /// <param name="name">The name of the feed source</param>
        /// <returns>True if a feed source has been provided with the specified name</returns>
        public bool Contains(string name)
        {
            return _feedSources.Values.Any(fs => fs.Name == name); 
        }

		/// <summary>
		/// Determines whether there is a Feed Source with the specified id
		/// </summary>
		/// <param name="id">The id of the feed source</param>
		/// <returns>True if a feed source has been provided with the specified name</returns>
		public bool ContainsKey(int id)
		{
			return _feedSources.ContainsKey(id);
		}
		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get { return _feedSources.Count; }
		}

        /// <summary>
        /// Indexer which returns feed source keyed by name
        /// </summary>
        /// <param name="name">The name of the feed source</param>
        /// <returns>The requested feed source</returns>
        /// <exception cref="KeyNotFoundException">if the name is not found in the <see cref="FeedSourceManager"/></exception>
        public FeedSourceEntry this[string name]
        {
            get {
                FeedSourceEntry fsid = _feedSources.Values.FirstOrDefault(fs => fs.Name == name);
                if (fsid != null)
                {
                    return fsid;
                }
            	throw new KeyNotFoundException(name);
            }          
        }
		/// <summary>
		/// Indexer which returns feed source keyed by name
		/// </summary>
		/// <param name="id">The id of the feed source</param>
		/// <returns>The requested feed source</returns>
		/// <exception cref="KeyNotFoundException">if the name is not found in the <see cref="FeedSourceManager"/></exception>
		public FeedSourceEntry this[int id]
		{
			get
			{
				return _feedSources[id];
			}
		}

		/// <summary>
		/// Returns feed source keyed by name
		/// </summary>
		/// <param name="name">The name of the feed source</param>
		/// <param name="value">out parameter for storing feed source</param>
		/// <returns>
		/// The requested feed source (true) or null if not found (false)
		/// </returns>
        public bool TryGetValue(string name, out FeedSourceEntry value)
        {            
            value = null;
            if (!string.IsNullOrEmpty(name))
            {
                FeedSourceEntry fsid = _feedSources.Values.FirstOrDefault(fs => fs.Name == name);
                if (fsid != null)
                {
                    value = fsid;
                	return true;
                }
            }
        	return false;
        }

		/// <summary>
		/// Removes the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		public void Remove(FeedSourceEntry source)
		{
			if (source != null && _feedSources.ContainsKey(source.ID))
				_feedSources.Remove(source.ID);
		}

		/// <summary>
		/// Gets the source entry of a <paramref name="sourceInstance"/>.
		/// </summary>
		/// <param name="sourceInstance">The feed source instance.</param>
		/// <returns></returns>
		public FeedSourceEntry SourceOf(FeedSource sourceInstance)
		{
			if (sourceInstance == null)
				return null;
			foreach (FeedSourceEntry id in _feedSources.Values)
				if (ReferenceEquals(id.Source, sourceInstance))
					return id;
			return null;
		}

		/// <summary>
		/// Gets the source of a <paramref name="newsFeed"/>.
		/// </summary>
		/// <param name="newsFeed">The news feed.</param>
		/// <returns></returns>
		public FeedSourceEntry SourceOf(INewsFeed newsFeed)
		{
			if (newsFeed == null)
				return null;
			foreach (FeedSourceEntry id in _feedSources.Values)
				if (ReferenceEquals(id.Source, newsFeed.owner))
					return id;
			return null;
		}
		/// <summary>
		/// Gets the source of a <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <returns></returns>
		public FeedSourceEntry SourceOf(INewsItem newsItem)
		{
			if (newsItem == null)
				return null;
			return SourceOf(newsItem.Feed);
		}

		/// <summary>
		/// Gets the source type of an item.
		/// </summary>
		/// <param name="newsFeed">The news feed.</param>
		/// <returns></returns>
		public FeedSourceType SourceTypeOf(INewsFeed newsFeed)
		{
			FeedSourceEntry sid = SourceOf(newsFeed);
			return sid != null ? sid.Source.Type : FeedSourceType.Unknown;
		}

		/// <summary>
		/// Gets the source extension.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="newsFeed">The news feed.</param>
		/// <returns>Null, if <paramref name="newsFeed"/> is null, else the requested extension instance</returns>
		/// <exception cref="InvalidCastException">If extension is not implemented by the <c>newsFeed</c> source</exception>
		public T GetSourceExtension<T>(INewsFeed newsFeed)
		{
			FeedSourceEntry sid = SourceOf(newsFeed);
			if (sid != null)
			{
				object t = sid.Source;
				return (T)t;
			}
			return default(T);
		}

		/// <summary>
		/// Loads the feed sources.
		/// </summary>
		/// <param name="feedSourcesUrl">The feed sources URL.</param>
		public void LoadFeedSources(string feedSourcesUrl)
		{
			if (String.IsNullOrEmpty(feedSourcesUrl))
				return;
			if (!File.Exists(feedSourcesUrl))
				return;

			using (var stream = FileHelper.OpenForRead(feedSourcesUrl))
			{
				LoadFeedSources(stream);
			}
		}

		/// <summary>
		/// Loads the feed sources.
		/// </summary>
		/// <param name="feedSources">The feed sources stream.</param>
		public void LoadFeedSources(Stream feedSources)
		{
			if (feedSources== null)
				return;

			try
			{
				XmlParserContext context =
					new XmlParserContext(null, new RssBanditXmlNamespaceResolver(), null, XmlSpace.None);
				XmlReader reader = new RssBanditXmlReader(feedSources, XmlNodeType.Document, context);

				//convert XML to objects
				XmlSerializer serializer =
					XmlHelper.SerializerCache.GetSerializer(typeof(SerializableFeedSources));
				SerializableFeedSources mySources = (SerializableFeedSources)serializer.Deserialize(reader);

				_feedSources.Clear();

				lastUsedKey = mySources.LastID;
				int maxUsedKey = 0;
				foreach (FeedSourceEntry fs in mySources.List)
				{
					if (maxUsedKey < fs.ID)
						maxUsedKey = fs.ID;
					_feedSources.Add(fs.ID, fs);
					fs.Source = FeedSource.CreateFeedSource(fs.ID, fs.SourceType,
						CreateSubscriptionLocation(fs.ID, fs.SourceType, fs.Properties));
				}
				if (maxUsedKey > lastUsedKey)
					lastUsedKey = maxUsedKey;

			}
			catch (Exception e)
            {
                _log.Error("Error on deserializing feed source",e);                
            }
		}

		/// <summary>
		/// Gets a unique key for a new feed source.
		/// </summary>
		/// <value>The unique key.</value>
		public int UniqueKey
		{
			get
			{
				if (lastUsedKey <= 0)
					lastUsedKey = CryptHelper.GenerateShortKey();
				int id = lastUsedKey + 1;
				while (_feedSources.ContainsKey(id))
					id++;
				lastUsedKey = id;
				return id;
			}
		}

		private int lastUsedKey;

		static SubscriptionLocation CreateSubscriptionLocation(int id, FeedSourceType type, IDictionary properties)
		{
			switch (type)
			{
				case FeedSourceType.DirectAccess:
					return new SubscriptionLocation(BuildSubscriptionName(id, type));
				case FeedSourceType.Google:
					return new SubscriptionLocation(BuildSubscriptionName(id, type),
						BuildCredentials(properties));				
				case FeedSourceType.NewsGator:
					return new SubscriptionLocation(BuildSubscriptionName(id, type),
						BuildCredentials(properties));
				case FeedSourceType.WindowsRSS:
					return new SubscriptionLocation(BuildSubscriptionName(id, type));
                case FeedSourceType.Facebook:
                    return new SubscriptionLocation(BuildSubscriptionName(id, type), BuildCredentials(properties));
				default:
					throw new InvalidOperationException("FeedSourceType not supported:" + type);
			}

		}

		/// <summary>
		/// Builds the name of the subscription (file name).
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static string BuildSubscriptionName(int id, FeedSourceType type)
		{
			//TODO: check name for invalid file name chars
			string path = FeedSource.DefaultConfiguration.UserApplicationDataPath;
			return Path.Combine(path, String.Format("{0}.{1}.subscription", type, id));
		}

		static NetworkCredential BuildCredentials(IDictionary properties)
		{
			if (properties == null)
				return null;
			string u = null, d = null, p = null;
			if (properties.Contains(PropertyKey.UserName))
				u =  Convert.ToString(properties[PropertyKey.UserName]);
			if (properties.Contains(PropertyKey.Domain))
				d = Convert.ToString(properties[PropertyKey.Domain]);
			if (properties.Contains(PropertyKey.Password))
				p = Convert.ToString(properties[PropertyKey.Password]);
			if (!String.IsNullOrEmpty(p))
				p = CryptHelper.Decrypt(p);
			return new NetworkCredential(u,p,d);
		}

		static StringProperties BuildProperties(FeedSourceEntry f)
		{
			StringProperties h = new StringProperties();
			if (f.Source.SubscriptionLocation.Credentials != null &&
				!String.IsNullOrEmpty(f.Source.SubscriptionLocation.Credentials.UserName))
			{
				h.Add(PropertyKey.Domain, f.Source.SubscriptionLocation.Credentials.Domain);
				h.Add(PropertyKey.UserName, f.Source.SubscriptionLocation.Credentials.UserName);
				h.Add(PropertyKey.Password, CryptHelper.Encrypt(f.Source.SubscriptionLocation.Credentials.Password));                
			}
			return h;
		}

		/// <summary>
		/// Saves the feed sources.
		/// </summary>
		/// <param name="feedSourcesUrl">The feed sources URL.</param>
		public void SaveFeedSources(string feedSourcesUrl)
		{
			var serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableFeedSources));
			var sources = new SerializableFeedSources();
			sources.LastID = lastUsedKey;

			if (_feedSources != null && _feedSources.Count > 0)
			{
				foreach (var f in _feedSources.Values)
				{
					sources.List.Add(f);
					f.Properties = BuildProperties(f);
				}	
			}

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, sources);
                FileHelper.WriteStreamWithBackup(feedSourcesUrl, stream);
            }			
        }

        #region search related 

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
            /// <remarks>This modifies the input FeedInfoList by replacing its INewsItem contents 
            /// with SearchHitNewsItems</remarks>
            /// <param name="tag">Object used by caller</param>
            /// <param name="matchingFeeds"></param>
            /// <param name="matchingFeedsCount">integer stores the count of matching feeds</param>
            /// <param name="matchingItemsCount">integer stores the count of matching INewsItem's (over all feeds)</param>
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
            /// <param name="matchingItemsCount">integer stores the count of matching INewsItem's (over all feeds)</param>
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

        /// <summary>Signature for <see cref="SearchFinished">SearchFinished</see>  event</summary>
        public delegate void SearchFinishedEventHandler(object sender, SearchFinishedEventArgs e);


        /// <summary>Called on a search finished</summary>
        public event SearchFinishedEventHandler SearchFinished;

        /// <summary>
        /// Manage the lucene search 
        /// </summary>
        protected static LuceneSearch p_searchHandler;


        /// <summary>
        /// Gets or sets the search index handler.
        /// </summary>
        /// <value>The search handler.</value>
        public static LuceneSearch SearchHandler
        {
            get
            {
                if (p_searchHandler == null)
                {
                    try
                    {
                        /*  We need to handle issues with FIPS security policy on Vista as reported at 
                         *  http://sourceforge.net/tracker/index.php?func=detail&aid=1960767&group_id=96589&atid=615248
                         * 
                         * TODO: Find a way to notify the user that search is disabled. 
                         */
                        p_searchHandler = new LuceneSearch(FeedSource.DefaultConfiguration);
                    }
                    catch (TypeInitializationException tex)
                    {
						_log.Error("Because of a lucene search initialization failure, search was turned off.", tex);
                        NewsComponentsConfiguration noIndexingConfig = new NewsComponentsConfiguration();
                        noIndexingConfig.SearchIndexBehavior = SearchIndexBehavior.NoIndexing;
                        p_searchHandler = new LuceneSearch(noIndexingConfig);
                    }
                }

                return p_searchHandler;
            }
            set { p_searchHandler = value; }
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
            int feedmatches = 0;
            int itemmatches = 0;

            IList<INewsItem> unreturnedMatchItems = new List<INewsItem>();
            var fiList = new FeedInfoList(String.Empty);

            Exception ex;
            bool valid = SearchHandler.ValidateSearchCriteria(criteria, cultureName, out ex);

            if (ex != null) // report always any error (warnings)
            {
                // render the error in-line (search result):
                fiList.Add(FeedSource.CreateHelpNewsItemFromException(ex).FeedDetails);
                feedmatches = fiList.Count;
                unreturnedMatchItems = fiList.GetAllNewsItems();
                itemmatches = unreturnedMatchItems.Count;
            }

            if (valid)
            {
                try
                {
                    // do the search (using lucene):
                    LuceneSearch.Result r = SearchHandler.ExecuteSearch(criteria, scope, 
                                                                        this.Sources.Select(entry => entry.Source), cultureName);

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


                    ItemReadState readState = ItemReadState.Ignore;
                    if (criteriaProperty != null)
                    {
                        readState = criteriaProperty.BeenRead ? ItemReadState.BeenRead : ItemReadState.Unread;
                    }


                    if (r != null && r.ItemMatchCount > 0)
                    {
                        /* append results */ 
                        var nids = new SearchHitNewsItem[r.ItemsMatched.Count];
                        r.ItemsMatched.CopyTo(nids, 0);
                        
                        //look in every feed source to find source feed for matching news items
                        IEnumerable<FeedInfoList> results = Sources.Select(entry => entry.Source.FindNewsItems(nids, readState, returnFullItemText));
                        foreach (FeedInfoList fil in results)
                        {
                            fiList.AddRange(fil);
                        }

                        feedmatches = fiList.Count;
                        unreturnedMatchItems = fiList.GetAllNewsItems();
                        itemmatches = unreturnedMatchItems.Count;                       
                    }
                }
                catch (Exception searchEx)
                {
                    // render the error in-line (search result):
                    fiList.Add(FeedSource.CreateHelpNewsItemFromException(searchEx).FeedDetails);
                    feedmatches = fiList.Count;
                    unreturnedMatchItems = fiList.GetAllNewsItems();
                    itemmatches = unreturnedMatchItems.Count;
                }
            }

            RaiseSearchFinishedEvent(tag, fiList, unreturnedMatchItems, feedmatches, itemmatches);
        }


        /// <summary>
        /// <summary>
        /// Notify the user interface that the search has finished
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="matchingFeeds">The feeds that matched the search</param>
        /// <param name="matchingFeedsCount">Number of feeds matched by the search</param>
        /// <param name="matchingItemsCount">Number of items matched by the search</param>
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
                  _log.ErrorFormat("SearchFinished() event code raises exception: {0}", e);
              }
          }


        /// <summary>
        /// Notify the user interface that the search has finished
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="matchingFeeds">The feeds that matched the search</param>
        /// <param name="matchingItems">The items that matched the search</param>
        /// <param name="matchingFeedsCount">Number of feeds matched by the search</param>
        /// <param name="matchingItemsCount">Number of items matched by the search</param>
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
                _log.ErrorFormat("SearchFinished() event code raises exception: {0}", e);
            }
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
            var unreturnedMatchItems = new List<INewsItem>(1);
            try
            {
                unreturnedMatchItems = RssParser.DownloadItemsFromFeed(searchFeedUrl);
            }
            catch (Exception remoteSearchException)
            {
                unreturnedMatchItems.Add(FeedSource.CreateHelpNewsItemFromException(remoteSearchException));
            }

            int feedmatches = 1;
            int itemmatches = unreturnedMatchItems.Count;
            var fi =
                new FeedInfo(String.Empty, String.Empty, unreturnedMatchItems, String.Empty, String.Empty, String.Empty,
                             new Dictionary<XmlQualifiedName, string>(), String.Empty);
            var fil = new FeedInfoList(String.Empty)
                          {
                              fi
                          };
            RaiseSearchFinishedEvent(tag, fil, feedmatches, itemmatches);
        }

        #endregion 
    }
}
