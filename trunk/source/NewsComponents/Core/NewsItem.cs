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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.News;
using NewsComponents.RelationCosmos;
using NewsComponents.Utils;

namespace NewsComponents
{
    /// <summary>
    /// Represents an RSS enclosure
    /// </summary>
    public class Enclosure : IEnclosure
    {
        private readonly string mimeType;
        private readonly long length;
        private readonly string url;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mimeType">The MIME type of the enclosure</param>
        /// <param name="length">The length of the enclosure in bytes</param>
        /// <param name="url">The URL of the enclosure</param>
        /// <param name="description">The description.</param>
        public Enclosure(string mimeType, long length, string url, string description)
        {
            this.length = length;
            this.mimeType = mimeType;
            this.url = url;
            Description = description;
            Duration = TimeSpan.MinValue;
        }

        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        public string MimeType
        {
            get { return mimeType; }
        }

        /// <summary>
        /// The length of the enclosure in bytes
        /// </summary>
        public long Length
        {
            get { return length; }
        }

        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        public string Url
        {
            get { return url; }
        }

        /// <summary>
        /// The description associated with the item obtained via itunes:subtitle or media:title
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether this enclosure has already been downloaded or not.
        /// </summary>
        public bool Downloaded { get; set; }

        /// <summary>
        /// Gets the playing time of the enclosure. 
        /// </summary>
        public TimeSpan Duration { get; set; }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
        public bool Equals(IEnclosure other)
        {
            return Equals(other as Enclosure);
        }

		/// <summary>
		/// Equalses the specified enclosure.
		/// </summary>
		/// <param name="enclosure">The enclosure.</param>
		/// <returns></returns>
        public bool Equals(Enclosure enclosure)
        {
            if (enclosure == null) return false;
            return Equals(url, enclosure.url);
        }

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as Enclosure);
        }

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
        public override int GetHashCode()
        {
            return url != null ? url.GetHashCode() : 0;
        }
    }

    /// <summary>
    /// Represents an item in an RSS feed
    /// </summary>
    public class NewsItem : RelationBase<INewsItem>, INewsItem, ISizeInfo, IEquatable<NewsItem>, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the Feed Link (Source Url)
        /// </summary>
        public string FeedLink
        {
            [DebuggerStepThrough]
            get
            {
                if (p_feed != null)
                    return p_feed.link;
                return null;
            }
        }

        /// <summary>
        /// The link to the item.
        /// </summary>
        public string Link
        {
            [DebuggerStepThrough]
            get { return HRef; }
        }

        /// <summary>
        /// The date the article or blog entry was made. 
        /// </summary>
        public DateTime Date
        {
            get { return aPointInTime; }
            set { base.PointInTime = value; }
        }


        //		private string p_id;				// used to link to stories read
        //		/// <summary>
        //		/// The unique identifier.
        //		/// </summary>
        //		public string Id { get { return p_id; } }

        protected INewsFeed p_feed; // owner

        /// <summary>
        /// Returns the feed object to which this item belongs
        /// </summary>
        public INewsFeed Feed
        {
            get { return p_feed; }
        }

        private string p_title;

        private readonly string p_parentId;

        /// <summary>
        /// The unique identifier of the parent.
        /// </summary>
        public string ParentId
        {
            get { return p_parentId; }
        }

        private string p_author;


        private byte[] p_content;

        /// <summary>
        /// The content of the article or blog entry. 
        /// </summary>
        public string Content
        {
            get { return p_content != null ? Encoding.UTF8.GetString(p_content) : null; }
        }

        void INewsItem.SetContent(string newContent, ContentType contentType)
        {
            SetContent(newContent, contentType);
        }

        internal byte[] GetContent()
        {
            return p_content;
        }

        internal void SetContent(byte[] newContent, ContentType contentType)
        {
            if (newContent != null)
            {
                p_content = newContent;
                p_contentType = contentType;
                return;
            }
        }

        internal void SetContent(string newContent, ContentType contentType)
        {
            if (string.IsNullOrEmpty(newContent))
            {
                p_content = null;
                p_contentType = ContentType.None;
                return;
            }

            p_content = Encoding.UTF8.GetBytes(newContent);
            p_contentType = contentType;
        }

        /// <summary>
        /// Returns true, if Content contains something; else false.
        /// </summary>
        /// <remarks>Should be used instead of testing 
        /// (Content != null &amp;&amp; Content.Length > 0) and is equivalent to 
        /// .ContentType == ContentType.None
        /// </remarks>
        public bool HasContent
        {
            get { return (p_contentType != ContentType.None); }
        }


        protected ContentType p_contentType = ContentType.None;

        /// <summary>
        /// Indicates whether the description on this feed is text, HTML or XHTML. 
        /// </summary>
        public ContentType ContentType
        {
            get { return p_contentType; }
            set { p_contentType = value; }
        }

        protected bool p_beenRead;

        /// <summary>
        /// Indicates whether the story has been read or not. 
        /// </summary>
        public bool BeenRead
        {
            get { return p_beenRead; }
            set
            {
                p_beenRead = value;
                OnPropertyChanged("BeenRead");
            }
        }

        protected IFeedDetails feedInfo;

        /// <summary>
        /// Returns an object implementing the FeedDetails interface to which this item belongs
        /// </summary>
        public IFeedDetails FeedDetails
        {
            get { return feedInfo; }
            set { feedInfo = value; }
        }

        protected Flagged p_flagStatus = Flagged.None;

        /// <summary>
        /// Indicates whether the item has been flagged for follow up or not. 
        /// </summary>
        public Flagged FlagStatus
        {
            get { return p_flagStatus; }
            set
            {
                p_flagStatus = value;
                OnPropertyChanged("FlagStatus");
            }
        }

        protected bool p_hasNewComments;

        /// <summary>
        /// Indicates that there are new comments to this item. 
        /// </summary>
        public bool HasNewComments
        {
            get { return p_hasNewComments; }
            set { p_hasNewComments = value; }
        }

        protected bool p_watchComments;

        /// <summary>
        /// Indicates that comments to this item are being watched. 
        /// </summary>
        public bool WatchComments
        {
            get { return p_watchComments; }
            set { p_watchComments = value; }
        }

        protected string p_language;

        /// <summary>
        /// Gets or sets the language of the entry.
        /// Format of the corresponfing attribute as defined in
        /// http://www.w3.org/TR/REC-xml/#sec-lang-tag;
        /// Format of the language string: 
        /// see http://www.ietf.org/rfc/rfc3066.txt
        /// </summary>
        /// <value>The language.</value>
        public string Language
        {
            get
            {
                if (string.IsNullOrEmpty(p_language) && (feedInfo != null))
                {
                    return feedInfo.Language;
                }
                return p_language;
            }
            set { p_language = value; }
        }


        /// <summary>
        /// An RSS item must always be initialized on construction.
        /// </summary>
        protected NewsItem()
        {
        }


        /// <summary>
        /// Creates a new NewsItem which is initialized from the input NewsItem and has it's owner feed as the 
        /// input INewsFeed. 
        /// </summary>
        /// <returns>A copy of this NewsItem</returns>
        public NewsItem(INewsFeed parent, INewsItem item)
            : this(parent, item.Title, item.Link, null, item.Date, item.Subject, item.Id, item.ParentId)
        {
            OptionalElements = new Dictionary<XmlQualifiedName, string>(OptionalElements);
            p_beenRead = item.BeenRead;
            p_author = item.Author;
            p_flagStatus = item.FlagStatus;
            SetContent(item.Content, item.ContentType);
            p_contentType = item.ContentType;
            commentUrl = item.CommentUrl;
            commentRssUrl = item.CommentRssUrl;
            commentCount = item.CommentCount;
            commentStyle = item.CommentStyle;
            p_watchComments = item.WatchComments;
            p_hasNewComments = item.HasNewComments;
        }

        /// <summary>
        /// Initializes an object representation of an RSS item. 
        /// </summary>
        /// <param name="feed">The RSS feed object this item belongs to.</param>
        /// <param name="title">The title of the article or blog entry.</param>
        /// <param name="link">A link to the article or blog entry.</param>
        /// <param name="content">A description of the article or blog entry. This parameter may 
        /// contain markup. </param>
        /// <param name="date">The date the article or blog entry was written or when it was fetched.</param>		
        /// <param name="subject">The topic of the article or blog entry.</param>
        public NewsItem(INewsFeed feed, string title, string link, string content, DateTime date, string subject)
            :
                this(
                feed, title, link, content, date, subject, ContentType.Unknown,
                new Dictionary<XmlQualifiedName, string>(), link, null)
        {
        }


        /// <summary>
        /// Initializes an object representation of an NNTP item
        /// </summary>
        /// <param name="feed">The NNTP feed object this item belongs to</param>
        /// <param name="title">The title of the newsgroup message</param>
        /// <param name="link">A link to the article or blog entry.</param>		
        /// <param name="content">The content of the newsgroup message</param>
        /// <param name="author">The author of the newsgroup message</param>
        /// <param name="date">Date of the newsgroup message</param>
        /// <param name="id">The unique identifier for the item</param>
        /// <param name="parentId">The ID of the message this one is in response to</param>
        public NewsItem(INewsFeed feed, string title, string link, string content, string author, DateTime date,
                        string id, string parentId)
            :
                this(feed, title, link, content, date, null, ContentType.Text, null, id, parentId)
        {
            p_author = author;
        }

        /// <summary>
        /// Initializes an object representation of an RSS item. 
        /// </summary>
        /// <param name="feed">The RSS feed object this item belongs to.</param>
        /// <param name="title">The title of the article or blog entry.</param>
        /// <param name="link">A link to the article or blog entry.</param>
        /// <param name="content">A description of the article or blog entry. This parameter may 
        /// contain markup. </param>
        /// <param name="date">The date the article or blog entry was written or when it was fetched.</param>		
        /// <param name="subject">The topic of the article or blog entry.</param>
        /// <param name="id">The unique identifier for the item</param>
        /// <param name="parentId">The ID of the message this one is in response to</param>		
        public NewsItem(INewsFeed feed, string title, string link, string content, DateTime date, string subject,
                        string id, string parentId)
            :
                this(
                feed, title, link, content, date, subject, ContentType.Unknown,
                new Dictionary<XmlQualifiedName, string>(), id, parentId)
        {
        }

        /// <summary>
        /// Initializes an object representation of an RSS item. 
        /// </summary>
        /// <param name="feed">The RSS feed object this item belongs to.</param>
        /// <param name="title">The title of the article or blog entry.</param>
        /// <param name="link">A link to the article or blog entry.</param>
        /// <param name="content">The content of the blog entry</param>
        /// <param name="date">The date the article or blog entry was written or when it was fetched.</param>
        /// <param name="subject">The topic of the article or blog entry.</param>
        /// <param name="ctype">Indicates whether the description parameter contains Text, encoded HTML or XHTML </param>
        /// <param name="otherElements">Hashtable containing  QName/XmlNode pairs that represent RSS elements that 
        /// don't map to properties on this class.</param>
        /// <param name="id">The unique identifier for the item</param>
        /// <param name="parentId">The unique identifier of the parent of this item</param>
        public NewsItem(INewsFeed feed, string title, string link, string content, DateTime date, string subject,
                        ContentType ctype, Dictionary<XmlQualifiedName, string> otherElements, string id,
                        string parentId)
            :
                this(feed, title, link, content, date, subject, ctype, otherElements, id, parentId, link)
        {
        }

        /// <summary>
        /// Initializes an object representation of an RSS item. 
        /// </summary>
        /// <param name="feed">The RSS feed object this item belongs to.</param>
        /// <param name="title">The title of the article or blog entry.</param>
        /// <param name="link">A link to the article or blog entry.</param>
        /// <param name="content">The content of the blog entry</param>
        /// <param name="date">The date the article or blog entry was written or when it was fetched.</param>
        /// <param name="subject">The topic of the article or blog entry.</param>
        /// <param name="ctype">Indicates whether the description parameter contains Text, encoded HTML or XHTML </param>
        /// <param name="otherElements">Hashtable containing  QName/XmlNode pairs that represent RSS elements that 
        /// don't map to properties on this class.</param>
        /// <param name="id">The unique identifier for the item</param>
        /// <param name="parentId">The unique identifier of the parent of this item</param>		
        /// <param name="baseUrl">The base URL used for resolving relative links in the content of the NewsItem</param>
        public NewsItem(INewsFeed feed, string title, string link, string content, DateTime date, string subject,
                        ContentType ctype, Dictionary<XmlQualifiedName, string> otherElements, string id,
                        string parentId, string baseUrl) :
                            this(
                            feed, title, link, content, date, subject, ctype, otherElements, id, parentId, baseUrl, null
                            )
        {
        }

        /// <summary>
        /// Initializes an object representation of an RSS item. 
        /// </summary>
        /// <param name="feed">The RSS feed object this item belongs to.</param>
        /// <param name="title">The title of the article or blog entry.</param>
        /// <param name="link">A link to the article or blog entry.</param>
        /// <param name="content">The content of the blog entry</param>
        /// <param name="date">The date the article or blog entry was written or when it was fetched.</param>
        /// <param name="subject">The topic of the article or blog entry.</param>
        /// <param name="ctype">Indicates whether the description parameter contains Text, encoded HTML or XHTML </param>
        /// <param name="otherElements">Hashtable containing  QName/XmlNode pairs that represent RSS elements that 
        /// don't map to properties on this class.</param>
        /// <param name="id">The unique identifier for the item</param>
        /// <param name="parentId">The unique identifier of the parent of this item</param>		
        /// <param name="baseUrl">The base URL used for resolving relative links in the content of the NewsItem</param>
        /// <param name="outgoingLinks">Outgoing hyperlinks from the HTML content of this item</param>
        public NewsItem(INewsFeed feed, string title, string link, string content, DateTime date, string subject,
                        ContentType ctype, Dictionary<XmlQualifiedName, string> otherElements, string id,
                        string parentId, string baseUrl,
                        List<string> outgoingLinks)
        {
            OptionalElements = otherElements;

            p_feed = feed;
            p_title = title;
            HRef = (link != null ? link.Trim() : null);
            // fix relative item link url:
            HRef = HtmlHelper.ConvertToAbsoluteUrl(HRef, baseUrl);

            //escape commonly occuring entities and remove CDATA sections					      
            if (content != null)
            {
                if (content.StartsWith("<![CDATA["))
                {
                    // true, if it is loaded from cache
                    content = content.Replace("<![CDATA[", String.Empty).Replace("]]>", String.Empty);
                }

                // now done via DisplayingNewsChannelProcessor before we display (not this general way):
                //content = HtmlHelper.StripBadTags(content);

                //remove invalid XML inserted by MS Word 				
                //description = description.Replace("<?xml:namespace", "<p").Replace("&lt;?xml:namespace", "&lt;p"); 
                //description = description.Replace("<?XML:NAMESPACE", "<p").Replace("&lt;?XML:NAMESPACE", "&lt;p"); 
                //description = description.Replace("o:", "").Replace("st1:", "").Replace("st2:", "");  
            }

            //make sure we have a title
            ProcessTitle(content);

            content = HtmlHelper.ExpandRelativeUrls(content, baseUrl);
            SetContent(content, ctype);

            p_id = id;

            // now check for a valid identifier (needed to remember read stories)
            if (p_id == null)
            {
                int hc = (p_title != null ? p_title.GetHashCode() : 0) +
                         (HasContent ? Content.GetHashCode() : 0);
                p_id = hc.ToString();
            }

            p_parentId = parentId;

            base.PointInTime = date;
            this.subject = subject;

            if (outgoingLinks != null)
                OutGoingLinks = outgoingLinks;

            if (FeedSource.buildRelationCosmos)
            {
                if (outgoingLinks == null)
                {
                    ProcessOutGoingLinks(content);
                }

                bool idEqHref = ReferenceEquals(HRef, p_id);

                if (null != HRef)
                    HRef = RelationCosmos.RelationCosmos.UrlTable.Add(HRef);

                p_id = idEqHref ? HRef : RelationCosmos.RelationCosmos.UrlTable.Add(p_id);

                if (!string.IsNullOrEmpty(p_parentId))
                {
                    // dealing with the relationcosmos string comparer (references only!):
                    string p_parentIdUrl = RelationCosmos.RelationCosmos.UrlTable.Add(
                        NntpParser.CreateGoogleUrlFromID(p_parentId));

                    if (ReferenceEquals(outgoingRelationships, GetList<string>.Empty))
                    {
                        outgoingRelationships = new List<string>(1);
                    }

                    outgoingRelationships.Add(p_parentIdUrl);
                }
            }
        }


        private string subject;
        private SupportedCommentStyle commentStyle;
        private string commentUrl;
        private string commentRssUrl;
        private int commentCount = NoComments;
        private Dictionary<XmlQualifiedName, string> optionalElements;
        private List<IEnclosure> enclosures;

        ///<summary>
        ///numeric value that indicates that no comments exist for an item
        ///</summary>
        public static int NoComments = Int32.MinValue;

        /// <summary>
        /// Overrides the default impl. of RelationBase. We return true, if we have a
        /// valid commentRssUrl and the commentCount is greater than zero.
        /// CommentCount is only be considered, if FeedSource.UnconditionalCommentRss
        /// is false (default).
        /// </summary>
        public override bool HasExternalRelations
        {
            get
            {
                if (!string.IsNullOrEmpty(commentRssUrl))
                {
                    if (FeedSource.UnconditionalCommentRss)
                        return true;
                    if (commentCount > 0)
                        return true;
                }
                return base.HasExternalRelations;
            }
        }

        /// <summary>
        /// Overrides the default impl. of RelationBase. External Relations are the comments
        /// of a NewsItem.
        /// </summary>
        /// <param name="relations"></param>
        public override void SetExternalRelations<T>(IList<T> relations)
        {
            if (base.GetExternalRelations().Count > 0)
            {
                FeedSource.RelationCosmosRemoveRange(relations);
            }
            FeedSource.RelationCosmosAddRange(relations);

            base.SetExternalRelations(relations);
        }

        /// <summary>
        /// The author of the article or blog entry 
        /// </summary>
        public string Author
        {
            get
            {
                if (FeedDetails.Type == FeedType.Rss)
                {
                    // TorstenR: expand HTML entities in title (property is accessed by the GUI)
                    // internally we use the unexpanded version to work with
                    string t = HtmlHelper.StripAnyTags(p_author);
                    if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0)
                    {
                        //t = System.Web.HttpUtility.HtmlDecode(t);
                        t = HtmlHelper.HtmlDecode(t);
                    }
                    return t;
                }

                return p_author;
            }
            set { p_author = value; }
        }


        /// <summary>
        /// The title of the article or blog entry. 
        /// </summary>
        public string Title
        {
            get
            {
                // TorstenR: expand HTML entities in title (property is accessed by the GUI)
                // internally we use the unexpanded version to work with
                string t = HtmlHelper.StripAnyTags(p_title);
                if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0)
                {
                    //t = System.Web.HttpUtility.HtmlDecode(t);
                    t = HtmlHelper.HtmlDecode(t);
                }
                return t.Trim();
            }
            set { p_title = (value ?? String.Empty); }
        }

        /// <summary>
        /// The subject of the article or blog entry. 
        /// </summary>
        public string Subject
        {
            get
            {
                // TorstenR: expand HTML entities in title (property is accessed by the GUI)
                // internally we use the unexpanded version to work with
                string t = HtmlHelper.StripAnyTags(subject);
                if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0)
                {
                    //t = System.Web.HttpUtility.HtmlDecode(t);
                    t = HtmlHelper.HtmlDecode(t);
                }
                return t;
            }
            set { subject = (value ?? String.Empty); }
        }

        /// <summary>
        /// Indicates whether this item supports posting comments. 
        /// </summary>
        public SupportedCommentStyle CommentStyle
        {
            get { return commentStyle; }
            set { commentStyle = value; }
        }

        /// <summary>
        /// Returns the amount of comments attached.
        /// </summary>
        public int CommentCount
        {
            get { return commentCount; }
            set { commentCount = value; }
        }

        /// <summary>the URL to post comments to</summary>
        public string CommentUrl
        {
            get { return commentUrl; }
            set { commentUrl = value; }
        }

        /// <summary>the URL to get an RSS feed of comments from</summary>
        public string CommentRssUrl
        {
            get { return commentRssUrl; }
            set { commentRssUrl = value; }
        }


        /// <summary>
        /// Container of enclosures on the item. If there are no enclosures on the item
        /// then this value is null. 
        /// </summary>
        public List<IEnclosure> Enclosures
        {
            get { return enclosures; }
            set { enclosures = value; }
        }

        /// <summary>
        /// Container for all the optional RSS elements for an item. Also 
        /// holds information from RSS modules. The keys in the hashtable 
        /// are instances of XmlQualifiedName while the values are instances 
        /// of XmlNode. 
        /// </summary>
        public Dictionary<XmlQualifiedName, string> OptionalElements
        {
            get
            {
                if (optionalElements == null)
                {
                    optionalElements = new Dictionary<XmlQualifiedName, string>();
                }
                return optionalElements;
            }

            set
            {
                optionalElements = value;
                /*
                if(value != null){
                    this.ProcessOptionalElements(); 
                } */
            }
        }

        /// <summary>
        /// Returns a collection of strings representing URIs to outgoing links in a feed. 
        /// </summary>
        public List<string> OutGoingLinks
        {
            get { return outgoingRelationships; }
            internal set { outgoingRelationships = value; }
        }

        /// <summary>
        /// Returns a copy of this NewsItem. The OptionalElements is only a shallow copy.
        /// </summary>
        /// <returns>A copy of this NewsItem</returns>
        public object Clone()
        {
            var item = new NewsItem(p_feed, p_title, HRef, null, Date, subject, p_id, p_parentId);
            item.OptionalElements = new Dictionary<XmlQualifiedName, string>(OptionalElements);
            item.p_beenRead = p_beenRead;
            item.p_author = p_author;
            item.p_flagStatus = p_flagStatus;
            item.p_content = p_content; //save performance costs of converting to & from UTF-8
            item.p_contentType = p_contentType;
            item.commentUrl = commentUrl;
            item.commentRssUrl = commentRssUrl;
            item.commentCount = commentCount;
            item.commentStyle = commentStyle;
            item.p_watchComments = p_watchComments;
            item.p_hasNewComments = p_hasNewComments;
            return item;
        }

        /// <summary>
        /// Copies the item (clone) and set the new parent to the provided feed 
        /// </summary>
        /// <param name="f">NewsFeed</param>
        /// <returns></returns>
        public INewsItem Clone(INewsFeed f)
        {
            var newItem = (NewsItem) Clone();
            newItem.p_feed = f;
            return newItem;
        }

        /// <summary>
        /// Helper function used by ToString(bool). 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="useGMTDate">Indicates whether the time should be written out as GMT or local time</param>
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>						
        public void WriteItem(XmlWriter writer, bool useGMTDate, bool noDescriptions)
        {
            //<item>
            writer.WriteStartElement("item");

            // xml:lang attribute
            if (!string.IsNullOrEmpty(p_language))
            {
                writer.WriteAttributeString("xml", "lang", null, p_language);
            }

            // <title />
            if (!string.IsNullOrEmpty(p_title))
            {
                writer.WriteElementString("title", p_title);
            }

            // <link /> 
            if (!string.IsNullOrEmpty(HRef))
            {
                writer.WriteElementString("link", HRef);
            }

            // <pubDate /> 			we write it with InvariantInfo to get them stored in a non-localized format
            if (useGMTDate)
            {
                writer.WriteElementString("pubDate", Date.ToString("r", DateTimeFormatInfo.InvariantInfo));
            }
            else
            {
                writer.WriteElementString("pubDate", Date.ToLocalTime().ToString("F", DateTimeFormatInfo.InvariantInfo));
            }

            // <category />
            if (!string.IsNullOrEmpty(subject))
            {
                writer.WriteElementString("category", subject);
            }

            //<guid>
            if (!string.IsNullOrEmpty(p_id) && (p_id.Equals(HRef) == false))
            {
                writer.WriteStartElement("guid");
                writer.WriteAttributeString("isPermaLink", "false");
                writer.WriteString(p_id);
                writer.WriteEndElement();
            }

            //<dc:creator>
            if (!string.IsNullOrEmpty(p_author))
            {
                writer.WriteElementString("creator", "http://purl.org/dc/elements/1.1/", p_author);
            }

            //<annotate:reference>
            if (!string.IsNullOrEmpty(p_parentId))
            {
                writer.WriteStartElement("annotate", "reference", "http://purl.org/rss/1.0/modules/annotate/");
                writer.WriteAttributeString("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#", p_parentId);
                writer.WriteEndElement();
            }

            // Always prefer <description> 
            if (!noDescriptions && HasContent)
            {
                /* if(this.ContentType != ContentType.Xhtml){ */
                writer.WriteStartElement("description");
                writer.WriteCData(Content);
                writer.WriteEndElement();
                /* }else // if(this.contentType == ContentType.Xhtml)  { 
                    writer.WriteStartElement("xhtml", "body",  "http://www.w3.org/1999/xhtml");
                    writer.WriteRaw(this.Content); 
                    writer.WriteEndElement();
                } */
            }

            //<wfw:comment />
            if (!string.IsNullOrEmpty(commentUrl))
            {
                if (commentStyle == SupportedCommentStyle.CommentAPI)
                {
                    writer.WriteStartElement("wfw", "comment", RssHelper.NsCommentAPI);
                    writer.WriteString(commentUrl);
                    writer.WriteEndElement();
                }
            }

            //<wfw:commentRss />
            if (!string.IsNullOrEmpty(commentRssUrl))
            {
                writer.WriteStartElement("wfw", "commentRss", RssHelper.NsCommentAPI);
                writer.WriteString(commentRssUrl);
                writer.WriteEndElement();
            }

            //<slash:comments>
            if (commentCount != NoComments)
            {
                writer.WriteStartElement("slash", "comments", "http://purl.org/rss/1.0/modules/slash/");
                writer.WriteString(commentCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }


            //	if(format == NewsItemSerializationFormat.NewsPaper){

            writer.WriteStartElement("fd", "state", "http://www.bradsoft.com/feeddemon/xmlns/1.0/");
            writer.WriteAttributeString("read", BeenRead ? "1" : "0");
            writer.WriteAttributeString("flagged", FlagStatus == Flagged.None ? "0" : "1");
            writer.WriteEndElement();

            //	} else { 
            //<rssbandit:flag-status />
            if (FlagStatus != Flagged.None)
            {
                //TODO: check: why we don't use the v2004/vCurrent namespace?
                writer.WriteElementString("flag-status", NamespaceCore.Feeds_v2003, FlagStatus.ToString());
            }
            //	}


            if (p_watchComments)
            {
                //TODO: check: why we don't use the v2004/vCurrent namespace?
                writer.WriteElementString("watch-comments", NamespaceCore.Feeds_v2003, "1");
            }

            if (HasNewComments)
            {
                //TODO: check: why we don't use the v2004/vCurrent namespace?
                writer.WriteElementString("has-new-comments", NamespaceCore.Feeds_v2003, "1");
            }

            //<enclosure />
            if (enclosures != null)
            {
                foreach (Enclosure enc in enclosures)
                {
                    writer.WriteStartElement("enclosure");
                    writer.WriteAttributeString("url", enc.Url);
                    writer.WriteAttributeString("type", enc.MimeType);
                    writer.WriteAttributeString("length", enc.Length.ToString(CultureInfo.InvariantCulture));
                    if (enc.Downloaded)
                    {
                        writer.WriteAttributeString("downloaded", "1");
                    }
                    if (enc.Duration != TimeSpan.MinValue)
                    {
                        writer.WriteAttributeString("duration", enc.Duration.ToString());
                    }
                    writer.WriteEndElement();
                }
            }

            //<rssbandit:outgoing-links />            
            writer.WriteStartElement("outgoing-links", NamespaceCore.Feeds_v2003);
            foreach (var outgoingLink in OutGoingLinks)
            {
                writer.WriteElementString("link", NamespaceCore.Feeds_v2003, outgoingLink);
            }
            writer.WriteEndElement();

            /* everything else */
            foreach (var s in OptionalElements.Values)
            {
                writer.WriteRaw(s);
            }

            //end </item> 
            writer.WriteEndElement();
        }

        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item. 
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message.  </param>
        /// <returns></returns>
        public String ToString(NewsItemSerializationFormat format)
        {
            return ToString(format, true, false);
        }

        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item. 
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message. </param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <returns>A string representation of this news item</returns>		
        public String ToString(NewsItemSerializationFormat format, bool useGMTDate)
        {
            return ToString(format, useGMTDate, false);
        }


        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item. 
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message. </param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>		
        /// <returns>A string representation of this news item</returns>		
        public String ToString(NewsItemSerializationFormat format, bool useGMTDate, bool noDescriptions)
        {
            string toReturn;

            switch (format)
            {
                case NewsItemSerializationFormat.NewsPaper:
                case NewsItemSerializationFormat.RssFeed:
                case NewsItemSerializationFormat.RssItem:
                    toReturn = ToRssFeedOrItem(format, useGMTDate, noDescriptions);
                    break;
                case NewsItemSerializationFormat.NntpMessage:
                    toReturn = ToNntpMessage();
                    break;
                default:
                    throw new NotSupportedException(format.ToString());
            }


            return toReturn;
        }

        /// <summary>
        /// Converts the NewsItem to an XML representation of an 
        /// RSS item element is returned or an entire RSS feed with this item as its 
        /// sole item.
        /// </summary>
        /// <param name="format">Indicates whether an XML representation of an 
        /// RSS item element is returned, an entire RSS feed with this item as its 
        /// sole item or an NNTP message. </param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>		
        /// <param name="noDescriptions">Indicates whether the contents of RSS items should 
        /// be written out or not.</param>				
        /// <returns>An RSS item or RSS feed</returns>
        public String ToRssFeedOrItem(NewsItemSerializationFormat format, bool useGMTDate, bool noDescriptions)
        {
            var sb = new StringBuilder("");
            var writer = new XmlTextWriter(new StringWriter(sb))
                             {
                                 Formatting = Formatting.Indented
                             };

            if (format == NewsItemSerializationFormat.RssFeed || format == NewsItemSerializationFormat.NewsPaper)
            {
                if (format == NewsItemSerializationFormat.NewsPaper)
                {
                    writer.WriteStartElement("newspaper");
                    writer.WriteAttributeString("type", "newsitem");
                }
                else
                {
                    writer.WriteStartElement("rss");
                    writer.WriteAttributeString("version", "2.0");
                }

                writer.WriteStartElement("channel");
                writer.WriteElementString("title", FeedDetails.Title);
                writer.WriteElementString("link", FeedDetails.Link);
                writer.WriteElementString("description", FeedDetails.Description);

                foreach (var s in FeedDetails.OptionalElements.Values)
                {
                    writer.WriteRaw(s);
                }
            }

            WriteItem(writer, useGMTDate, noDescriptions);

            if (format == NewsItemSerializationFormat.RssFeed || format == NewsItemSerializationFormat.NewsPaper)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts the object to an XML string containing an RSS 2.0 item.  
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return ToString(NewsItemSerializationFormat.RssItem);
        }

        /*
        /// <summary>
        /// Processes the hashtable of XmlQualifiedName/XmlNode pairs and uses those to populate 
        /// read-only properties of the class. 
        /// </summary>
        private void ProcessOptionalElements(){
            bool wfwCommentSeen = false; string wfwCommentRssSeen = null; 

            foreach(XmlNode node in this.OptionalElements.Values){

                if(node.NamespaceURI.Equals(RssHelper.NsCommentAPI)){

                    if(node.LocalName.Equals("comment")){
                        this.commentUrl   = node.InnerText; 
                        this.commentStyle = SupportedCommentStyle.CommentAPI; 								
                        wfwCommentSeen    = true;
                    }else if(StringHelper.AreEqualCaseInsensitive(node.LocalName, "commentrss")){
                        this.commentRssUrl = node.InnerText.Trim(); 
                        wfwCommentRssSeen  = node.LocalName; 
                    }
                }
            }//foreach

            if(wfwCommentSeen){			
                OptionalElements.Remove(new XmlQualifiedName("comment", RssHelper.NsCommentAPI)); 
            }

            if(wfwCommentRssSeen != null){			
                OptionalElements.Remove(new XmlQualifiedName(wfwCommentRssSeen, RssHelper.NsCommentAPI)); 
            }
        } */

        /// <summary>
        /// Makes sure the title isn't null or empty string as well as strips markup. 
        /// </summary>
        private void ProcessTitle(string content)
        {
            //if no title provided then use first 8 words of description						
            if (string.IsNullOrEmpty(p_title))
            {
                p_title = GetFirstWords(HtmlHelper.StripAnyTags(content), 8);
            }

            //Get around issues with titles that are one big CDATA section. This 
            //should no longer be needed once we move to .NET 2.0 can use ReadContentAsString()
            //in RssParser.cs 
            if (p_title.StartsWith("<![CDATA["))
            {
                p_title = p_title.Replace("<![CDATA[", String.Empty).Replace("]]>", String.Empty);
            }

            // remove carriage return and line feed in title
            // Fixed to use Enviroment.NewLine so that \r\n combos 
            // in windows are replaced with only one slash.
            // Still replace \r and \n individually afterwards in case of 
            // content produced on non-windows machines have these individually.
            p_title = p_title.Replace(Environment.NewLine, " ").Replace("\r", " ").Replace("\n", " ");
        }


        /// <summary>
        /// Processes the <paramref name="content"/> for outgoing links and populate 
        /// the outgoing links property. 
        /// </summary>
        private void ProcessOutGoingLinks(string content)
        {
            if (FeedSource.BuildRelationCosmos)
            {
                outgoingRelationships = HtmlHelper.RetrieveLinks(content);
            }
            else
            {
                outgoingRelationships = new List<string>();
            }
        }


        /// <summary>
        /// Gets the first n number of words from the provided string. 
        /// </summary>
        /// <param name="text">The target string</param>
        /// <param name="wordCount">The number of words to pick</param>
        /// <returns>The firs</returns>
        private static string GetFirstWords(string text, int wordCount)
        {
            if (text == null)
                return String.Empty;

            return StringHelper.GetFirstWords(text, wordCount) + "(...)";
        }

        /// <summary>
        /// Creates an XPathNavigator over the XML representation of this object. This method
        /// is equivalent to calling CreateNavigator(false)
        /// </summary>
        /// <returns>An XPathNavigator</returns>
        public XPathNavigator CreateNavigator()
        {
            return CreateNavigator(false);
        }

        /// <summary>
        /// Creates an XPathNavigator over the XML representation of this object.
        /// </summary>
        /// <param name="standalone">Indicates whether the navigator will be over the single 
        /// RSS item or over a representation of the parent RSS feed with this item 
        /// as the single item within it. When this parameter is true then the navigator 
        /// works over just an RSS item</param>
        /// <returns>An XPathNavigator</returns>
        public XPathNavigator CreateNavigator(bool standalone)
        {
            return CreateNavigator(standalone, true);
        }

        /// <summary>
        /// Creates an XPathNavigator over the XML representation of this object.
        /// </summary>
        /// <param name="standalone">Indicates whether the navigator will be over the single 
        /// RSS item or over a representation of the parent RSS feed with this item 
        /// as the single item within it. When this parameter is true then the navigator 
        /// works over just an RSS item</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <returns>An XPathNavigator</returns>
        public XPathNavigator CreateNavigator(bool standalone, bool useGMTDate)
        {
            NewsItemSerializationFormat format = (standalone
                                                      ? NewsItemSerializationFormat.RssItem
                                                      : NewsItemSerializationFormat.RssFeed);
            var doc =
                new XPathDocument(new XmlTextReader(new StringReader(ToString(format, useGMTDate))), XmlSpace.Preserve);
            return doc.CreateNavigator();
        }

        /// <summary>
        /// Get the hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        /// <summary>
        /// Compares to see if two NewsItems are identical. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as NewsItem);
        }

        public bool Equals(INewsItem other)
        {
            return Equals(other as NewsItem);
        }

        public bool Equals(NewsItem other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
                return false;

            if (Id.Equals(other.Id))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Creates a text based version of this object as NNTP message suitable for posting to a news server
        /// </summary>
        /// <returns>the NewsItem as an NNTP message string</returns>
        private string ToNntpMessage()
        {
            var sb = new StringBuilder();


            sb.Append("From: ");
            sb.Append(p_author);

            try
            {
                var newsgroupUri = new Uri(FeedDetails.Link);
                sb.Append("\r\nNewsgroups: ");
                sb.Append(newsgroupUri.AbsolutePath.Substring(1));
            }
            catch (UriFormatException)
            {
            }

            sb.Append("\r\nX-Newsreader: ");
            sb.Append(FeedSource.GlobalUserAgentString);

            sb.Append("\r\nSubject: ");
            sb.Append(p_title);

            if (!string.IsNullOrEmpty(p_parentId))
            {
                sb.Append("\r\nReferences: ");
                sb.Append(p_parentId);
            }

            /*
            if(this.headers.Count != 0){
			
                foreach(string header in this.headers.Keys){
                    sb.Append("\r\n" + header + ": ");				
                    sb.Append(this.headers[header]); 
                }
            }
            */

            sb.Append("\r\n\r\n");
            sb.Append(Content);
            sb.Append("\r\n.\r\n");

            return sb.ToString();
        }

        #region ISizeInfo Members

        /// <summary>
        /// [used to measure mem]
        /// </summary>
        /// <returns></returns>
        public int GetSize()
        {
            int l = StringHelper.SizeOfStr(p_id);
            l += StringHelper.SizeOfStr(Link);
            if (HasContent)
                l += p_content.Length;
            l += StringHelper.SizeOfStr(subject);
            l += StringHelper.SizeOfStr(commentUrl);

            return l;
        }

        /// <summary>
        /// [used to measure mem]
        /// </summary>
        public string GetSizeDetails()
        {
            return GetSize().ToString();
        }

        #endregion

        #region INotifyPropertyChanged implementation

        /// <summary>
        ///  Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fired whenever a property is changed. 
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(DataBindingHelper.GetPropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies listeners that a property has changed. 
        /// </summary>
        /// <param name="e">Details on the property change event</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, e);
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a NewsItem that shows up in the context of search results over local RSS feeds. 
    /// </summary>
    public class SearchHitNewsItem : NewsItem
    {
        #region Constructors

        /* new NewsItem(f, 
                    doc.Get(LuceneSearch.Keyword.ItemTitle),
                    doc.Get(LuceneSearch.Keyword.ItemLink),
                    doc.Get(LuceneSearch.IndexDocument.ItemSummary),
                    doc.Get(LuceneSearch.Keyword.ItemAuthor),
                    new DateTime(DateTools.StringToTime(doc.Get(LuceneSearch.Keyword.ItemDate))),
                    LuceneNewsItemSearch.NewsItemIDFromUID(doc.Get(LuceneSearch.IndexDocument.ItemID)), null);
        */

        /// <summary>
        /// Main Constructor. 
        /// </summary>
        /// <param name="feed"></param>
        /// <param name="title"></param>
        /// <param name="link"></param>
        /// <param name="summary"></param>
        /// <param name="author"></param>
        /// <param name="date"></param>
        /// <param name="id"></param>
        public SearchHitNewsItem(INewsFeed feed, string title, string link, string summary, string author, DateTime date,
                                 string id)
            :
                base(feed, title, link, null, author, date, id, null)
        {
            Enclosures = GetList<IEnclosure>.Empty;
            Summary = summary;
        }


        /// <summary>
        /// Initialize with a NewsItem 
        /// </summary>
        /// <param name="item"></param>
        public SearchHitNewsItem(INewsItem item)
            :
                this(item.Feed, item.Title, item.Link, item.Content, item.Author, item.Date, item.Id)
        {
        }

        #endregion

        #region Properties and Fields

        private string summary;

        /// <summary>
        /// A text snippet from the actual content of the RSS item. 
        /// </summary>
        public string Summary
        {
            get { return summary; }

            set
            {
                summary = value;
                /* add to OptionalElements hashtable and make sure XML is properly escaped 
                 * by using XmlTextWriter 
                 */

                try
                {
                    var qname = new XmlQualifiedName("summary", "http://www.w3.org/2005/Atom");

                    using (var sw = new StringWriter())
                    {
                        var xtw = new XmlTextWriter(sw);
                        xtw.WriteElementString(qname.Name, qname.Namespace, value);
                        xtw.Close();

                        OptionalElements.Remove(qname);
                        OptionalElements.Add(qname, sw.ToString());
                    }
                }
                catch (Exception)
                {
                    /* XML exception if summary contains invalid XML content */
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a NewsItem with informations and help about 
    /// a exception occured
    /// </summary>
    public class ExceptionalNewsItem : SearchHitNewsItem
    {
        public ExceptionalNewsItem(INewsFeed feed, string title, string link, string summary, string author,
                                   DateTime date, string id)
            :
                base(feed, title, link, summary, author, date, id)
        {
            SetContent(summary, ContentType.Html);
        }
    }

    internal class RankedNewsItem
    {
        private readonly INewsItem item;
        private readonly float score;

        internal RankedNewsItem(INewsItem item, float score)
        {
            this.score = score;
            this.item = item;
        }

        internal float Score
        {
            get { return score; }
        }

        internal INewsItem Item
        {
            get { return item; }
        }
    }
}