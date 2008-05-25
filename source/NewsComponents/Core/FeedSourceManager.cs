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
		[XmlIgnore]
		public Guid ID { get { return _id; } }
		private Guid _id;

		FeedSource _source;
		[XmlIgnore]
		public FeedSource Source
		{
			get { return _source; }
			internal set { _source = value; }
		}

		public FeedSourceEntry() { }

		public FeedSourceEntry(FeedSource source, string name, int ordinal)
		{
			_id = Guid.NewGuid();
			Name = name;
			Ordinal = ordinal;
			_source = source;
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
		public string SerializableID
		{
			get { return ID.ToString(); }
			set { 
				if (value != null) 
					_id = new Guid(value);
			}
		}

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

		private XmlSerializableHashtable _serializedSourceProperties = new XmlSerializableHashtable();
		[XmlElement("properties")]
		public XmlSerializableHashtable Properties
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
	public class StringPropertiesBag : XmlSerializableHashtable
	{
		
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
		readonly Dictionary<Guid, FeedSourceEntry> _feedSources = new Dictionary<Guid, FeedSourceEntry>();

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
			FeedSourceEntry fs = new FeedSourceEntry(source, name, _feedSources.Count);
			_feedSources.Add(fs.ID, fs);
			return fs;
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
        /// Indexer which returns feed source keyed by name
        /// </summary>
        /// <param name="name">The name of the feed source</param>
        /// <returns>The requested feed source</returns>
        /// <exception cref="KeyNotFoundException">if the name is not found in the FeedSourceManager</exception>
        public FeedSourceEntry this[string name]
        {
            get {
                FeedSourceEntry fsid = _feedSources.Values.First(fs => fs.Name == name);
                if (fsid != null)
                {
                    return fsid;
                }
            	throw new KeyNotFoundException(name);
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
                FeedSourceEntry fsid = _feedSources.Values.First(fs => fs.Name == name);
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
		/// Gets the source of a newsfeed.
		/// </summary>
		/// <param name="newsFeed">The news feed.</param>
		/// <returns></returns>
		public FeedSourceEntry SourceOf(INewsFeed newsFeed)
		{
			if (newsFeed == null)
				return null;
			foreach (FeedSourceEntry id in _feedSources.Values)
				if (id.Source == newsFeed.owner)
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
					fs.Source = FeedSource.CreateFeedSource(fs.SourceType, CreateSubscriptionLocation(fs.Name, fs.SourceType, fs.Properties.InnerHashtable));
				}
			}
		}

		static SubscriptionLocation CreateSubscriptionLocation(string name, FeedSourceType type, IDictionary properties)
		{
			switch (type)
			{
				case FeedSourceType.DirectAccess:
					return new SubscriptionLocation(BuildSubscriptionName(name));
				case FeedSourceType.Google:
					return new SubscriptionLocation(BuildSubscriptionName(name),
						BuildCredentials(properties));
						
				case FeedSourceType.NewsGator:
					return new SubscriptionLocation(BuildSubscriptionName(name),
						BuildCredentials(properties));
				case FeedSourceType.WindowsRSS:
					return new SubscriptionLocation(BuildSubscriptionName(name));
				default:
					throw new InvalidOperationException("FeedSourceType not supported:" + type);
			}

		}

		static string BuildSubscriptionName(string name)
		{
			//TODO: check name for invalid file name chars
			string path = FeedSource.DefaultConfiguration.UserApplicationDataPath;
			return Path.Combine(path, String.Format("subscriptions.{0}.xml", name));
		}

		static NetworkCredential BuildCredentials(IDictionary properties)
		{
			return new NetworkCredential((string)properties["user"],
				CryptHelper.Decrypt((string)properties["pwd"]));
		}

		static Hashtable BuildProperties(FeedSourceEntry f)
		{
			Hashtable h = new Hashtable();
			if (f.Source.SubscriptionLocation.Credentials != null)
			{
				h.Add("user", f.Source.SubscriptionLocation.Credentials.UserName);
				h.Add("pwd", CryptHelper.Encrypt(f.Source.SubscriptionLocation.Credentials.Password));
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
					f.Properties.InnerHashtable = BuildProperties(f);
				}
				using (TextWriter writer = new StreamWriter(FileHelper.OpenForWrite(feedSourcesUrl)))
				{
					serializer.Serialize(writer, sources);
				}
			}
		}
	}
}
