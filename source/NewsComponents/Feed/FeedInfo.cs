#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NewsComponents.Utils;

namespace NewsComponents.Feed
{
    /// <summary>
    /// represents information about a particular RSS feed. 
    /// </summary>
    public class FeedInfo : IInternalFeedDetails, ISizeInfo
    {
        /// <summary>
        /// Gets the empty feed info instance.
        /// </summary>
        public static readonly FeedInfo Empty = new FeedInfo(String.Empty, String.Empty, new List<INewsItem>(),
                                                             String.Empty, String.Empty, String.Empty,
                                                             new Dictionary<XmlQualifiedName, string>(0), String.Empty);


        private string _id;


        /// <summary>Gets/sets the id of this feed</summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }


        private string _feedLocation; //location in the cache not on the WWW

        /// <summary>
        /// Gets or sets the feed location.
        /// </summary>
        /// <value>The feed location.</value>
        public string FeedLocation
        {
            get { return _feedLocation; }
            set { _feedLocation = value; }
        }

        private readonly ObservableCollection<INewsItem> _itemsList = new ObservableCollection<INewsItem>();

        /// <summary>
        /// The list of news items belonging to the feed
        /// </summary>
        /// <value></value>
        public ReadOnlyObservableCollection<INewsItem> ItemsList
        {
            get;
            private set;

        }

        private readonly Dictionary<XmlQualifiedName, string> _optionalElements;


        /// <summary>
        /// Creates a FeedInfo initialized from the specified IFeedDetails object.
        /// It also take over the IFeedDetails.ItemsList entries.
        /// </summary>
        /// <param name="ifd">The object to copy from</param>
        public FeedInfo(IFeedDetails ifd)
            : this(ifd, ifd.ItemsList)
        {
        }

        /// <summary>
        /// Creates a FeedInfo initialized from the specified IFeedDetails object
        /// </summary>
        /// <param name="ifd">The object to copy from</param>
        /// <param name="itemsList">The items list to use.</param>
        public FeedInfo(IFeedDetails ifd, IEnumerable<INewsItem> itemsList)
            : this(ifd.Id, String.Empty, new List<INewsItem>(itemsList),
                   ifd.Title, ifd.Link, ifd.Description,
                   new Dictionary<XmlQualifiedName, string>(ifd.OptionalElements), ifd.Language)
        {
        }

        /// <summary>
        /// Overloaded. Initializer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="feedLocation"></param>
        /// <param name="itemsList"></param>
        public FeedInfo(string id, string feedLocation, IEnumerable<INewsItem> itemsList)
        {
            this._id = id;
            this._feedLocation = feedLocation;
            ItemsList = new ReadOnlyObservableCollection<INewsItem>(this._itemsList);
            if (itemsList != null)
            {
                foreach (var item in itemsList)
                    this._itemsList.Add(item);
            }
        }

        /// <summary>
        /// Overloaded. Initializer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="feedLocation"></param>
        /// <param name="itemsList"></param>
        /// <param name="title"></param>
        /// <param name="link"></param>
        /// <param name="description"></param>
        public FeedInfo(string id, string feedLocation, IEnumerable<INewsItem> itemsList, string title, string link,
                        string description)
            : this(
                id, feedLocation, itemsList, title, link, description, new Dictionary<XmlQualifiedName, string>(),
                String.Empty)
        {
        }

        /// <summary>
        /// Overloaded. Initializer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="feedLocation"></param>
        /// <param name="itemsList"></param>
        /// <param name="title"></param>
        /// <param name="link"></param>
        /// <param name="description"></param>
        /// <param name="optionalElements"></param>
        /// <param name="language"></param>
        public FeedInfo(string id, string feedLocation, IEnumerable<INewsItem> itemsList, string title, string link,
                        string description, IDictionary<XmlQualifiedName, string> optionalElements, string language)
        {
            this._id = id;
            this._feedLocation = feedLocation;
            this.ItemsList = new ReadOnlyObservableCollection<INewsItem>(this._itemsList);

            if (itemsList != null)
            {
                foreach (var item in itemsList)
                    this._itemsList.Add(item);
            }

            this._title = title;
            this._link = link;
            this._description = description;
            this._optionalElements = new Dictionary<XmlQualifiedName, string>(optionalElements);
            this._language = language;

            _type = RssHelper.IsNntpUrl(link) ? FeedType.Nntp : FeedType.Rss;
        }



        private readonly string _title;

        /// <summary></summary>
        public string Title
        {
            get { return _title; }
        }

        private readonly string _description;

        /// <summary></summary>
        public string Description
        {
            get { return _description; }
        }

        private readonly string _link;

        /// <summary></summary>
        public string Link
        {
            get { return _link; }
        }

        private readonly string _language;

        /// <summary></summary>
        public string Language
        {
            get { return _language; }
        }

        /// <summary>
        /// Table of optional feed elements.
        /// </summary>
        public Dictionary<XmlQualifiedName, string> OptionalElements
        {
            get { return _optionalElements; }
        }

        private readonly FeedType _type;

        /// <summary>
        /// Gets the type of the FeedDetails
        /// </summary>
        public FeedType Type
        {
            get { return _type; }
        }

        public void AddItem(INewsItem item)
        {
            _itemsList.Add(item);
        }
        
        public void ReplaceItems(IEnumerable<INewsItem> newItems)
        {
            _itemsList.Clear();
            _itemsList.AddRange(newItems);
        }
        
        public void RemoveItem(INewsItem item)
        {
            _itemsList.Remove(item);
        }
        
        public void RemoveItemAt(int index)
        {
            _itemsList.RemoveAt(index);
        }

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(XmlWriter writer)
        {
            WriteTo(writer, NewsItemSerializationFormat.RssFeed, true);
        }

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>
        public void WriteTo(XmlWriter writer, bool noDescriptions)
        {
            WriteTo(writer, NewsItemSerializationFormat.RssFeed, true, noDescriptions);
        }

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
        public void WriteTo(XmlWriter writer, NewsItemSerializationFormat format)
        {
            WriteTo(writer, format, true, false);
        }

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>				
        public void WriteTo(XmlWriter writer, NewsItemSerializationFormat format, bool useGMTDate)
        {
            WriteTo(writer, format, useGMTDate, false);
        }


        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>				
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>
        public void WriteTo(XmlWriter writer, NewsItemSerializationFormat format, bool useGMTDate, bool noDescriptions)
        {
            //writer.WriteStartDocument(); 

            if (format == NewsItemSerializationFormat.NewsPaper)
            {
                //<newspaper type="channel">
                writer.WriteStartElement("newspaper");
                writer.WriteAttributeString("type", "channel");
                // NamespaceCore.Feeds_v2003 == "http://www.25hoursaday.com/2003/RSSBandit/feeds/"
                writer.WriteAttributeString("xmlns", NamespaceCore.BanditPrefix, null, NamespaceCore.Feeds_v2003);
                writer.WriteElementString("title", _title);
            }
            else if (format != NewsItemSerializationFormat.Channel)
            {
                //<rss version="2.0">
                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");
                writer.WriteAttributeString("xmlns", NamespaceCore.BanditPrefix, null, NamespaceCore.Feeds_vCurrent);
            }

            /* These are here because so many people cut & paste into blogs from Microsoft Word 
            writer.WriteAttributeString("xmlns","v",null,"urn:schemas-microsoft-com:office:vml");
            writer.WriteAttributeString("xmlns","x",null,"urn:schemas-microsoft-com:office:excel");
            writer.WriteAttributeString("xmlns","o",null,"urn:schemas-microsoft-com:office:office");
            writer.WriteAttributeString("xmlns","w",null,"urn:schemas-microsoft-com:office:word");
            writer.WriteAttributeString("xmlns","st1",null,"urn:schemas-microsoft-com:office:smarttags");
            writer.WriteAttributeString("xmlns","st2",null,"urn:schemas-microsoft-com:office:smarttags");
            writer.WriteAttributeString("xmlns","asp",null,"http://www.example.com/asp");
            */

            //<channel>
            writer.WriteStartElement("channel");

            //<title />
            writer.WriteElementString("title", Title);

            //<link /> 
            writer.WriteElementString("link", Link);

            //<description /> 
            writer.WriteElementString("description", Description);

            //other stuff
            string[] optionals = new string[_optionalElements.Count];
            _optionalElements.Values.CopyTo(optionals, 0); 
            foreach (var s in optionals)
            {
                writer.WriteRaw(s);
            }

            //<item />
            var items = _itemsList.ToArray();

            foreach (var item in items)
            {
                writer.WriteRaw(item.ToString(NewsItemSerializationFormat.RssItem, useGMTDate, noDescriptions));
            }

            writer.WriteEndElement();

            if (format != NewsItemSerializationFormat.Channel)
            {
                writer.WriteEndElement();
            }

            //writer.WriteEndDocument(); 
        }

        /// <summary>
        /// Provides the XML representation of the feed as an RSS 2.0 feed. 
        /// </summary>
        /// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
        /// <returns>the feed as an XML string</returns>
        public string ToString(NewsItemSerializationFormat format)
        {
            return ToString(format, true);
        }

        /// <summary>
        /// Provides the XML representation of the feed as an RSS 2.0 feed. 
        /// </summary>
        /// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <returns>the feed as an XML string</returns>
        public string ToString(NewsItemSerializationFormat format, bool useGMTDate)
        {
            var sb = new StringBuilder("");
            var writer = new XmlTextWriter(new StringWriter(sb));

            WriteTo(writer, format, useGMTDate);

            writer.Flush();
            writer.Close();

            return sb.ToString();
        }


        /// <summary>
        /// Provides the XML representation of the feed as an RSS 2.0 feed. 
        /// </summary>
        /// <returns>the feed as an XML string</returns>
        public override string ToString()
        {
            return ToString(NewsItemSerializationFormat.RssFeed);
        }

        /// <summary>
        /// Returns a copy of this FeedInfo. The OptionalElements and ItemsList are only a shallow copies.
        /// </summary>
        /// <param name="includeNewsItems">if set to <c>true</c> the item list is cloned (shallow). If false, it is the empty list</param>
        /// <returns>A copy of this FeedInfo</returns>
        public FeedInfo Clone(bool includeNewsItems)
        {
            var toReturn = new FeedInfo(_id, _feedLocation,
                                        (includeNewsItems ? new List<INewsItem>(_itemsList) : new List<INewsItem>()),
                                        _title, _link, _description,
                                        new Dictionary<XmlQualifiedName, string>(_optionalElements), _language);

            return toReturn;
        }

        /// <summary>
        /// Returns a copy of this FeedInfo. The OptionalElements and ItemsList are only a shallow copies.
        /// </summary>
        /// <returns>A copy of this FeedInfo</returns>
        public object Clone()
        {
            return Clone(true);
        }


        /// <summary>
        /// Writes the union of the distinct item IDs and contents of the NewsItems contained 
        /// in the input reader to the specified binary writer. 
        /// </summary>
        /// <param name="reader">A reader positioned over the old descriptions file</param>
        /// <param name="writer"></param>
        public void WriteItemContents(BinaryReader reader, BinaryWriter writer)
        {
            var inMemoryDescriptions = new StringCollection();
            //string id;

            foreach (NewsItem item in _itemsList)
            {
                if (item.HasContent)
                {
                    writer.Write(item.Id);
                    byte[] tempContent = item.GetContent();
                    writer.Write(tempContent.Length);
                    writer.Write(tempContent);
                    inMemoryDescriptions.Add(item.Id);
                }
            } //foreach(NewsItem...)

            if (reader != null)
            {
                _id = reader.ReadString();

                while (!_id.Equals(FileHelper.EndOfBinaryFileMarker) && !string.IsNullOrEmpty(_id))
                {
                    int count = reader.ReadInt32();
                    byte[] content = reader.ReadBytes(count);

                    if (!inMemoryDescriptions.Contains(_id) && ContainsItemWithId(_id))
                    {
                        writer.Write(_id);
                        writer.Write(count);
                        writer.Write(content);
                    }
                    _id = reader.ReadString();
                } //while(!id.Equals(...))
            } //if(reader!= null) 
        }


        /// <summary>
        /// Determines whether a NewsItem with the specified ID is contained in this FeedInfo
        /// </summary>
        /// <param name="id">The ID of the NewsItem</param>
        /// <returns></returns>
        private bool ContainsItemWithId(string id)
        {
            foreach (NewsItem item in ItemsList.ToArray())
            {
                if (item.Id.Equals(id))
                {
                    return true;
                }
            }
            return false;
        }

        #region ISizeInfo Members

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <returns></returns>
        public int GetSize()
        {
            int iSize = StringHelper.SizeOfStr(_link);
            iSize += StringHelper.SizeOfStr(_title);
            iSize += StringHelper.SizeOfStr(_title);
            iSize += StringHelper.SizeOfStr(_description);
            //iSize += this.SizeOf(this.optionalElements);
            return iSize;
        }

        /// <summary>
        /// Gets the size details.
        /// </summary>
        /// <returns></returns>
        public string GetSizeDetails()
        {
            return GetSize().ToString();
        }

        #endregion
    }


    /// <summary>
    /// Represents a list of FeedInfo objects. This is primarily used for generating newspaper views of multiple feeds.
    /// </summary>
    public class FeedInfoList : ICollection<IFeedDetails>
    {
        #region Private Members

        /// <summary>
        /// The list of feeds
        /// </summary>
        private readonly List<IFeedDetails> _feeds = new List<IFeedDetails>();

        /// <summary>
        /// The title of this list when displayed in a newspaper view
        /// </summary>
        private readonly string _title;

        #endregion

        #region Constructors 

        /// <summary>
        /// Creates a list with the specified title
        /// </summary>
        /// <param name="title">The name of the list</param>
        public FeedInfoList(string title)
        {
            this._title = title;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Returns the name of the list
        /// </summary>
        public string Title
        {
            get { return _title; }
        }

        #endregion

        #region Public methods 

        /// <summary>
        /// Returns all the NewsItems contained within this FeedInfoList in an ArrayList
        /// </summary>
        /// <returns>a list of all the NewsItems in this FeedInfoList</returns>
        public IList<INewsItem> GetAllNewsItems()
        {
            var allItems = new List<INewsItem>();

            foreach (FeedInfo fi in _feeds)
            {
                allItems.InsertRange(0, fi.ItemsList);
            }

            return allItems;
        }

        /// <summary>
        /// Adds a new Feed to the list
        /// </summary>
        /// <param name="feed">The FeedInfo object to add</param>
        /// <returns>The position into which the new feed was inserted</returns>
        public void Add(IFeedDetails feed)
        {
            _feeds.Add(feed);
        }

        /// <summary>
        /// Adds the range of feeds (FeedInfo collection).
        /// </summary>
        /// <param name="feedCollection">The feed collection.</param>
        public void AddRange(IEnumerable<IFeedDetails> feedCollection)
        {
            _feeds.AddRange(feedCollection);
        }

        /// <summary>
        /// Removes a Feed from the list
        /// </summary>
        /// <param name="feed">The IFeedDetails object to remove</param>
        public bool Remove(IFeedDetails feed)
        {
            return _feeds.Remove(feed);
        }

        /// <summary>
        /// Tests to see if the specified feed is in the list
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public bool Contains(IFeedDetails feed)
        {
            return _feeds.Contains(feed);
        }

        /// <summary>
        /// Removes all FeedInfo objects from the list
        /// </summary>
        public void Clear()
        {
            _feeds.Clear();
        }

        /// <summary>
        /// Gets the amount of FeedInfo objects.
        /// </summary>
        /// <value>The count.</value>
        public int NewsItemCount
        {
            get
            {
                int count = 0;

                foreach (FeedInfo fi in _feeds)
                {
                    count += fi.ItemsList.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the total amount of NewsItem objects in the FeedInfo objects held by this list.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return _feeds.Count; }
        }

        /// <summary>
        /// Provides the XML representation of the list as a FeedDemon newspaper 
        /// </summary>
        /// <returns>the feed list as an XML string</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("");
            var writer = new XmlTextWriter(new StringWriter(sb));

            WriteTo(writer);

            writer.Flush();
            writer.Close();

            return sb.ToString();
        }


        /// <summary>
        /// Writes this object as a FeedDemon group newspaper to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("newspaper");
            writer.WriteAttributeString("type", "group");
            writer.WriteElementString("title", _title);

            foreach (var feed in _feeds)
            {
                feed.WriteTo(writer, NewsItemSerializationFormat.Channel, false);
            }

            writer.WriteEndElement();
        }


        /// <summary>
        /// Returns an enumerator used to iterate over the FeedInfo objects in the list
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _feeds.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator used to iterate over the FeedInfo objects in the list
        /// </summary>
        /// <returns></returns>
        IEnumerator<IFeedDetails> IEnumerable<IFeedDetails>.GetEnumerator()
        {
            return _feeds.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">array is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
        /// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
        public void CopyTo(IFeedDetails[] array, int index)
        {
            _feeds.CopyTo(array, index);
        }


        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}
