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

		public FeedSourceEntry() { }

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

		[XmlAttribute("id")]
		public int ID { get; set; }

		private FeedSourceType _serializedSourceType = FeedSourceType.Unknown;
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

	[Serializable]
	public class StringProperties: KeyItemCollection<string, string>
	{
		public StringProperties() {}
		public StringProperties(int capacity): base(capacity) {}
		protected StringProperties(SerializationInfo info, StreamingContext context) : 
			base(info, context) { }
	}

	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	[XmlRoot("feedsources", Namespace = NamespaceCore.Feeds_vCurrent, IsNullable = false)]
	public class SerializableFeedSources
	{
		/// <remarks/>
		[XmlElement("source", Type = typeof(FeedSourceEntry), IsNullable = false)]
		//public ArrayList List = new ArrayList();
		public List<FeedSourceEntry> List = new List<FeedSourceEntry>();

	}
	#endregion

	/// <summary>
	/// Manages a list of feed sources
	/// </summary>
	public class FeedSourceManager
	{
		/// <summary>
		/// Gets the keys of the common FeedSource properties dictionary
		/// </summary>
		public static class PropertyKey
		{
			public const string Domain = "domain";
			public const string UserName = "user";
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
		/// FeedSource.
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
		public FeedSourceEntry Add(FeedSource source, string name)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
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
		public FeedSourceEntry Add(string name, FeedSourceType type, IDictionary properties)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
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
		/// Clears this instance (removing all feedsources).
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
        /// <exception cref="KeyNotFoundException">if the name is not found in the FeedSourceManager</exception>
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
		/// <exception cref="KeyNotFoundException">if the name is not found in the FeedSourceManager</exception>
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
			if (_feedSources.ContainsKey(source.ID))
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
		/// <returns>Null, if newsFeed is null, else the requested extension instance</returns>
		/// <exception cref="InvalidCastException">If extension is not implemented by the newsFeed source</exception>
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
				foreach (FeedSourceEntry fs in mySources.List)
				{
					_feedSources.Add(fs.ID, fs);
					fs.Source = FeedSource.CreateFeedSource(fs.SourceType, CreateSubscriptionLocation(fs.ID, fs.SourceType, fs.Properties));
				}
			}
		}

		public int UniqueKey
		{
			get
			{
				int id = CryptHelper.GenerateShortKey();
				while (_feedSources.ContainsKey(id))
					id = CryptHelper.GenerateShortKey();
				return id;
			}
		}

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
		
		public void SaveFeedSources(string feedSourcesUrl)
		{
			var serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableFeedSources));
			var sources = new SerializableFeedSources();

			if (_feedSources != null)
			{
				foreach (var f in _feedSources.Values)
				{
					sources.List.Add(f);
					f.Properties = BuildProperties(f);
				}
				using (TextWriter writer = new StreamWriter(FileHelper.OpenForWrite(feedSourcesUrl)))
				{
					serializer.Serialize(writer, sources);
				}
			}
		}
	}
}
