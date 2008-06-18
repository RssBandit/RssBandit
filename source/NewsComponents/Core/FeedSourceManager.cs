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
using NewsComponents;
using NewsComponents.Collections;
using NewsComponents.Feed;
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
			foreach (FeedSourceEntry entry in _feedSources.Values) {
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

			FeedSourceEntry fs = new FeedSourceEntry(UniqueKey, source, name, _feedSources.Count);
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
			FeedSource source = FeedSource.CreateFeedSource(type, CreateSubscriptionLocation(id, type, properties));
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
			
			XmlParserContext context =
				new XmlParserContext(null, new RssBanditXmlNamespaceResolver(), null, XmlSpace.None);
			using (XmlReader reader = new RssBanditXmlReader(FileHelper.OpenForRead(feedSourcesUrl), XmlNodeType.Document, context))
			{
				
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
					fs.Source = FeedSource.CreateFeedSource(fs.SourceType, CreateSubscriptionLocation(fs.ID, fs.SourceType, fs.Properties));
				}
				if (maxUsedKey > lastUsedKey)
					lastUsedKey = maxUsedKey;
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

			using (TextWriter writer = new StreamWriter(FileHelper.OpenForWrite(feedSourcesUrl)))
			{
				serializer.Serialize(writer, sources);
			}
		}
	}
}
