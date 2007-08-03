#region CVS Version Header
/*
 * $Id: NewsItem.cs,v 1.64 2007/07/08 07:14:45 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2007/07/08 07:14:45 $
 * $Revision: 1.64 $
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using NewsComponents.Collections;
using NewsComponents.News;
using NewsComponents.RelationCosmos;
using NewsComponents.Utils;
using NewsComponents.Feed;

using RC = NewsComponents.RelationCosmos;

namespace NewsComponents
{

	/// <summary>
	/// Item flag states
	/// </summary>
	public enum Flagged{
		/// <summary>Not flagged</summary>
		None, 
		/// <summary>Flagged for follow up</summary>
		FollowUp, 
		/// <summary>Flagged for read (again). This is NOT the "been read" flag!</summary>
		Read, 
		/// <summary>Flagged for review</summary>
		Review, 
		/// <summary>Flagged for forward</summary>
		Forward,
		/// <summary>Flagged for reply. Is also set if you replied to an item</summary>
		Reply,
		/// <summary>Flagged for complete</summary>
		Complete
	}

	/// <summary>
	/// Supported comment styles.
	/// </summary>
	[Flags]//TODO: Dare, why is this a Flags enum?
	public enum SupportedCommentStyle{
		/// <summary>
		/// Undefined or None.
		/// </summary>
		None = 0, 
		/// <summary>
		/// Comment API
		/// </summary>
		CommentAPI = 1, 

		/// <summary>
		/// NNTP posting
		/// </summary>
		NNTP = 2 
	}


	/// <summary>
	/// Used on the overloaded ToString() method of the NewsItem to indicate what 
	/// format the NewsItem should be written out as when converted to a string.
	/// </summary>
	public enum NewsItemSerializationFormat{
		/// <summary>
		/// Indicates that the NewsItem should be written out as an Rss item element. 
		/// </summary>
		RssItem,
		/// <summary>
		/// Indicates that the NewsItem should be written out as a single item element within 
		/// an RSS feed
		/// </summary>
		RssFeed,
		/// <summary>
		/// Indicates that the NewsItem should be written out as an NNTP message
		/// </summary>
		NntpMessage, 
		/// <summary>
		/// Indicates that the NewsItem should be written out as a single item element within 
		/// an RSS feed that is itself within a FeedDemon newspaper element 
		/// </summary>
		NewsPaper, 
		/// <summary>
		/// Indicates that the NewsItem should be written out as a single item element within 
		/// an RSS 'channel' element 
		/// </summary>
		Channel
	}

	/// <summary>
	/// Represents an RSS enclosure
	/// </summary>
	public class Enclosure{ 

		private string mimeType; 
		private long length;
		private TimeSpan duration; 
		private string url; 
		private string description;
		private bool downloaded;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mimeType">The MIME type of the enclosure</param>
		/// <param name="length">The length of the enclosure in bytes</param>
		/// <param name="url">The URL of the enclosure</param>
		/// <param name="description">The description.</param>
		public Enclosure(string mimeType, long length, string url, string description){
			this.length = length; 
			this.mimeType = mimeType; 
			this.url = url; 			
			this.description = description;
			this.downloaded = false;
			this.Duration = TimeSpan.MinValue;
		}	
		
		/// <summary>
		/// The MIME type of the enclosure
		/// </summary>
		public string MimeType{
			get {return mimeType;} 
		} 

		/// <summary>
		/// The length of the enclosure in bytes
		/// </summary>
		public long Length{
			get {return length;} 
		} 

		/// <summary>
		/// The MIME type of the enclosure
		/// </summary>
		public string Url{
			get {return url;} 
		} 

		/// <summary>
		/// The description associated with the item obtained via itunes:subtitle or media:title
		/// </summary>
		public string Description{
			get { return description;}
			set { description = value; }
		}

		/// <summary>
		/// Indicates whether this enclosure has already been downloaded or not.
		/// </summary>
		public bool Downloaded{
			get { return downloaded;}
			set { downloaded = value; }
		}

		/// <summary>
		/// Gets the playing time of the enclosure. 
		/// </summary>
		public TimeSpan Duration{
			get { return duration; }
			set { duration = value; }
		}


		/// <summary>
		/// Compares to see if two Enclosures are identical. Identity just checks to see if they have 
		/// the same link, 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(Object  obj){

			if(Object.ReferenceEquals(this, obj)){ return true; }

			try{
				Enclosure item = (Enclosure) obj; 
			
				if (String.Compare(this.Url, item.Url)==0) {
					return true;
				}
			}catch(InvalidCastException){;}

			return false; 									
		}

		/// <summary>
		/// Get the hash code of the object
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode(){				

			if(StringHelper.EmptyOrNull(this.Url)){
				return String.Empty.GetHashCode();
			}else{
				return this.Url.GetHashCode();
			}
		}

	}

	/// <summary>
	/// Represents an item in an RSS feed
	/// </summary>
	public class NewsItem: RelationBase, INewsItem, ISizeInfo {
	

		/// <summary>
		/// Gets the Feed Link (Source Url)
		/// </summary>
		public string FeedLink { 
			[System.Diagnostics.DebuggerStepThrough()]
			get {
				if (p_feed != null)
					return p_feed.link;
				return null;
			}	
		} 
		/// <summary>
		/// The link to the item.
		/// </summary>
		public string Link {
			[System.Diagnostics.DebuggerStepThrough()]
			get { return base.hReference; } 
		}

		/// <summary>
		/// The date the article or blog entry was made. 
		/// </summary>
		public DateTime Date{ 
			get {return base.aPointInTime; } 
			set { base.PointInTime = value; }
		}


//		private string p_id;				// used to link to stories read
//		/// <summary>
//		/// The unique identifier.
//		/// </summary>
//		public string Id { get { return p_id; } }

		protected feedsFeed p_feed;		// owner
		/// <summary>
		/// Returns the feed object to which this item belongs
		/// </summary>
		public feedsFeed Feed{ get { return p_feed;} }

		private  string p_title;	
		
		private  string p_parentId; 
		/// <summary>
		/// The unique identifier of the parent.
		/// </summary>
		public string ParentId { get { return p_parentId; } }

		private  string p_author;	
	
		
		private byte[] p_content;	
		/// <summary>
		/// The content of the article or blog entry. 
		/// </summary>
		public string Content { 
			get { 
				return p_content != null ? Encoding.UTF8.GetString(p_content): null ; 
			
			} 
		}
		
		void INewsItem.SetContent(string newContent, ContentType contentType) {
			this.SetContent(newContent, contentType);
		}

		internal byte[] GetContent(){
			return p_content; 
		}

		internal void SetContent(byte[] newContent, ContentType contentType) {
			if (newContent!= null) {
				p_content = newContent;
				p_contentType = contentType;
				return;
			}
		}

		internal void SetContent(string newContent, ContentType contentType) {
			if (StringHelper.EmptyOrNull(newContent)) {
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
		public bool HasContent {
			get { return (p_contentType != ContentType.None); }
		}
	

		protected ContentType p_contentType = ContentType.None; 
		/// <summary>
		/// Indicates whether the description on this feed is text, HTML or XHTML. 
		/// </summary>
		public ContentType ContentType { 
			get { return p_contentType; }
			set { p_contentType = value; }
		}
		
		protected bool p_beenRead = false; 
		/// <summary>
		/// Indicates whether the story has been read or not. 
		/// </summary>
		public bool BeenRead{ 
			get { return p_beenRead; } 
			set { p_beenRead = value; }		
		}

		protected IFeedDetails feedInfo; 

		/// <summary>
		/// Returns an object implementing the FeedDetails interface to which this item belongs
		/// </summary>
		public IFeedDetails FeedDetails{
		
			get { return feedInfo;} 
			set { feedInfo = value; }
		}

		protected Flagged p_flagStatus = Flagged.None; 
		/// <summary>
		/// Indicates whether the item has been flagged for follow up or not. 
		/// </summary>
		public Flagged FlagStatus{ 
			get { return p_flagStatus; } 
			set {p_flagStatus = value; }		
		}

		protected bool p_hasNewComments; 

		/// <summary>
		/// Indicates that there are new comments to this item. 
		/// </summary>
		public bool HasNewComments{
			get { return p_hasNewComments; }
			set {p_hasNewComments = value; }
		}

		protected bool p_watchComments; 

		/// <summary>
		/// Indicates that comments to this item are being watched. 
		/// </summary>
		public bool WatchComments{
			get { return p_watchComments; }
			set {p_watchComments = value; }
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
		public string Language {
			get { 
				if(StringHelper.EmptyOrNull(p_language) && (this.feedInfo != null)){					
					return this.feedInfo.Language; 
				}else{ 				
					return p_language; 
				}			
			}	
			set { p_language = value; }
		}		
		

		/// <summary>
		/// An RSS item must always be initialized on construction.
		/// </summary>
		protected NewsItem(){;}


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
		public NewsItem(feedsFeed feed, string title, string link, string content, DateTime date, string subject):
			this(feed, title, link, content, date, subject, ContentType.Unknown, new Hashtable(), link, null){		    			
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
		public NewsItem(feedsFeed feed, string title, string link, string content, string author, DateTime date, string id, string parentId):
			this(feed, title, link, content, date, null, ContentType.Text, null, id, parentId){		    		
			
			this.p_author = author; 
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
		public NewsItem(feedsFeed feed, string title, string link, string content, DateTime date, string subject,  string id, string parentId):
			this(feed, title, link, content, date, subject, ContentType.Unknown, new Hashtable(), id, parentId){		    			
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
		public NewsItem(feedsFeed feed, string title, string link, string content, DateTime date, string subject, ContentType ctype,  Hashtable otherElements, string id, string parentId):
		this(feed, title, link, content, date, subject, ctype, otherElements, id, parentId, link){
			
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
		public NewsItem(feedsFeed feed, string title, string link, string content, DateTime date, string subject, ContentType ctype,  Hashtable otherElements, string id, string parentId, string baseUrl){

			this.OptionalElements = otherElements; 		
      
			p_feed  = feed; 
			p_title = title; 
			base.hReference = ( link != null ? link.Trim() : null) ;
			
			//escape commonly occuring entities and remove CDATA sections					      
			if(content != null){
	
				if(content.StartsWith("<![CDATA[")){			// true, if it is loaded from cache
					content = content.Replace("<![CDATA[",String.Empty).Replace("]]>",String.Empty); 
				}

				// now done via DisplayingNewsChannelProcessor before we display (not this general way):
				//content = HtmlHelper.StripBadTags(content);
				
				//remove invalid XML inserted by MS Word 				
				//description = description.Replace("<?xml:namespace", "<p").Replace("&lt;?xml:namespace", "&lt;p"); 
				//description = description.Replace("<?XML:NAMESPACE", "<p").Replace("&lt;?XML:NAMESPACE", "&lt;p"); 
				//description = description.Replace("o:", "").Replace("st1:", "").Replace("st2:", "");  

			}
					
			//make sure we have a title
			this.ProcessTitle(content); 								

			content = HtmlHelper.ExpandRelativeUrls(content, baseUrl);
			this.SetContent(content, ctype);

			p_id = id; 

			// now check for a valid identifier (needed to remember read stories)
			if (p_id == null){			
				int hc = (p_title != null ? p_title.GetHashCode() : 0) + 
					(this.HasContent ? this.Content.GetHashCode() : 0);
				p_id = hc.ToString();
			}
								
			p_parentId = parentId; 			

			base.PointInTime = date; 
			this.subject = subject; 

			if(NewsHandler.buildRelationCosmos){

				this.ProcessOutGoingLinks(content, baseUrl); 

				bool idEqHref = Object.ReferenceEquals(base.hReference, p_id);
			
				if(null != base.hReference)
					base.hReference = RC.RelationCosmos.UrlTable.Add(base.hReference);											
							
				if(idEqHref)
					p_id = base.hReference; 
				else
					p_id = RC.RelationCosmos.UrlTable.Add(p_id);

				if (null != p_parentId && p_parentId.Length > 0) { 
					// dealing with the relationcosmos string comparer (references only!):
					string p_parentIdUrl = RC.RelationCosmos.UrlTable.Add(
						NntpParser.CreateGoogleUrlFromID(p_parentId));

					if (outgoingRelationships.IsReadOnly)
						outgoingRelationships = new RelationHRefDictionary(1);

					outgoingRelationships.Add(p_parentIdUrl, 
						new RelationHRefEntry(p_parentIdUrl, String.Empty, outgoingRelationships.Count));
				}
				
			}

		}

	
		private string subject; 
		private SupportedCommentStyle commentStyle; 
		private string commentUrl = null; 
		private string commentRssUrl = null; 
		private int commentCount  = NoComments; 
		private Hashtable optionalElements; 
		private ArrayList enclosures = null; 

		///<summary>
		///numeric value that indicates that no comments exist for an item
		///</summary>
		public static int NoComments = Int32.MinValue; 

		/// <summary>
		/// Overrides the default impl. of RelationBase. We return true, if we have a
		/// valid commentRssUrl and the commentCount is greater than zero.
		/// CommentCount is only be considered, if NewsHandler.UnconditionalCommentRss
		/// is false (default).
		/// </summary>
		public override bool HasExternalRelations {
			get {
				if (!StringHelper.EmptyOrNull(this.commentRssUrl)) {
					if (NewsHandler.UnconditionalCommentRss)
						return true;
					if (this.commentCount > 0)
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
		public override void SetExternalRelations(RelationList relations) {
			if (base.GetExternalRelations() != RelationList.Empty) {
				NewsHandler.RelationCosmosRemoveRange(relations);
			}
			NewsHandler.RelationCosmosAddRange(relations);
			base.SetExternalRelations(relations);
		}

		/// <summary>
		/// The author of the article or blog entry 
		/// </summary>
		public  string Author{
			get { 
				if (this.FeedDetails.Type == FeedType.Rss) {
					// TorstenR: expand HTML entities in title (property is accessed by the GUI)
					// internally we use the unexpanded version to work with
					string t = HtmlHelper.StripAnyTags(p_author);
					if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
						//t = System.Web.HttpUtility.HtmlDecode(t);
						t = HtmlHelper.HtmlDecode(t);
					}
					return t; 
				
				} else {
					return p_author;
				}
			} 
			set {p_author = value ; }		
		}


		/// <summary>
		/// The title of the article or blog entry. 
		/// </summary>
		public  string Title {
			get { 
				// TorstenR: expand HTML entities in title (property is accessed by the GUI)
				// internally we use the unexpanded version to work with
				string t = HtmlHelper.StripAnyTags(this.p_title);
				if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
					//t = System.Web.HttpUtility.HtmlDecode(t);
					t = HtmlHelper.HtmlDecode(t);
				}
				return t.Trim(); 
			}
			set {
				this.p_title = (value == null ? String.Empty: value);
			}
		}

		/// <summary>
		/// The subject of the article or blog entry. 
		/// </summary>
		public string Subject{ 			
			get { 
				// TorstenR: expand HTML entities in title (property is accessed by the GUI)
				// internally we use the unexpanded version to work with
				string t = HtmlHelper.StripAnyTags(this.subject);
				if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
					//t = System.Web.HttpUtility.HtmlDecode(t);
					t = HtmlHelper.HtmlDecode(t);
				}
				return t; 
			} 
			set {
				this.subject = (value == null ? String.Empty: value);
			}
		}

		/// <summary>
		/// Indicates whether this item supports posting comments. 
		/// </summary>
		public SupportedCommentStyle CommentStyle{
		
			get { return this.commentStyle; } 
			set { this.commentStyle = value; }
		}
		/// <summary>
		/// Returns the amount of comments attached.
		/// </summary>
		public int CommentCount { 
			get {return this.commentCount; }
			set {this.commentCount = value; }
		}

		/// <summary>the URL to post comments to</summary>
		public string CommentUrl{			
			get { return this.commentUrl; }			
			set { this.commentUrl = value; }
		}

		/// <summary>the URL to get an RSS feed of comments from</summary>
		public string CommentRssUrl {		
			get { return this.commentRssUrl; }			
			set { this.commentRssUrl = value; }
		}


		/// <summary>
		/// Container of enclosures on the item. If there are no enclosures on the item
		/// then this value is null. 
		/// </summary>
		public IList Enclosures{
			get { return this.enclosures;  }
			set { this.enclosures = (ArrayList) value; }
		}

		/// <summary>
		/// Container for all the optional RSS elements for an item. Also 
		/// holds information from RSS modules. The keys in the hashtable 
		/// are instances of XmlQualifiedName while the values are instances 
		/// of XmlNode. 
		/// </summary>
		/// <remarks>Setting this field may have the side effect of setting certain read-only 
		/// properties such as CommentUrl and CommentStyle depending on whether CommentAPI 
		/// elements are contained in the table.</remarks>
		public Hashtable OptionalElements{
		
			get { 
				if(optionalElements == null){
					this.optionalElements = new Hashtable(); 
				}
				return this.optionalElements; 
			}

			set { 
				this.optionalElements = value; 
				/*
				if(value != null){
					this.ProcessOptionalElements(); 
				} */
			}

		} 

		/// <summary>
		/// Returns a collection of strings representing URIs to outgoing links in a feed. 
		/// </summary>
		public IStringCollection OutGoingLinks{
		
			get { return base.outgoingRelationships.Keys; }
		}

		/// <summary>
		/// Returns a copy of this NewsItem. The OptionalElements is only a shallow copy.
		/// </summary>
		/// <returns>A copy of this NewsItem</returns>
		public object Clone(){
		
			NewsItem item = new NewsItem(p_feed, p_title, base.hReference, null, Date, subject, p_id, p_parentId); 		
			item.OptionalElements = (Hashtable) OptionalElements.Clone(); 
			item.p_beenRead = p_beenRead; 
			item.p_author   = p_author; 
			item.p_flagStatus = p_flagStatus; 
			item.p_content = p_content; //save performance costs of converting to & from UTF-8
			item.p_contentType = p_contentType;
			item.commentUrl  = commentUrl; 
			item.commentRssUrl = commentRssUrl; 
			item.commentCount  = commentCount; 
			item.commentStyle  = commentStyle; 
			item.p_watchComments = p_watchComments; 
			item.p_hasNewComments = p_hasNewComments;
			return item; 
		}

		/// <summary>
		/// Copies the item (clone) and set the new parent to the provided feed 
		/// </summary>
		/// <param name="f">feedsFeed</param>
		/// <returns></returns>
		internal NewsItem CopyTo(feedsFeed f) {
			NewsItem newItem = (NewsItem)this.Clone();
			newItem.p_feed = f;
			return newItem;
		}

		/// <summary>
		/// Helper function used by ToString(bool). 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="useGMTDate">Indicates whether the time should be written out as GMT or local time</param>
		private void WriteItem(XmlWriter writer, bool useGMTDate){
			WriteItem(writer, useGMTDate, NewsItemSerializationFormat.RssItem, false); 
		}

		/// <summary>
		/// Helper function used by ToString(bool). 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="useGMTDate">Indicates whether the time should be written out as GMT or local time</param>
		/// <param name="format">Indicates whether the item is being serialized as part of a FeedDemon newspaper view</param>
		private void WriteItem(XmlWriter writer, bool useGMTDate, NewsItemSerializationFormat format){
			WriteItem(writer, useGMTDate, format, false); 			
		}

		/// <summary>
		/// Helper function used by ToString(bool). 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="useGMTDate">Indicates whether the time should be written out as GMT or local time</param>
		/// <param name="format">Indicates whether the item is being serialized as part of a FeedDemon newspaper view</param>
		/// <param name="noDescriptions">Indicates whether the contents of RSS items should 
		/// be written out or not.</param>						
		private void WriteItem(XmlWriter writer, bool useGMTDate, NewsItemSerializationFormat format, bool noDescriptions){
		
			//<item>
			writer.WriteStartElement("item"); 

			// xml:lang attribute
			if ((p_language != null) && (p_language.Length != 0)) {
				writer.WriteAttributeString("xml", "lang", null, p_language);
			}

			// <title />
			if((p_title != null) && (p_title.Length != 0)){ 
				writer.WriteElementString("title", p_title); 				
			}

			// <link /> 
			if((base.hReference != null) && (base.hReference.Length != 0)){ 
				writer.WriteElementString("link", base.hReference); 
			}

			// <pubDate /> 			we write it with InvariantInfo to get them stored in a non-localized format
			if(useGMTDate){
				writer.WriteElementString("pubDate", this.Date.ToString("r", System.Globalization.DateTimeFormatInfo.InvariantInfo	)); 				
			}else{
				writer.WriteElementString("pubDate", this.Date.ToLocalTime().ToString("F", System.Globalization.DateTimeFormatInfo.InvariantInfo)); 	
			}

			// <category />
			if((this.subject != null) && (this.subject.Length != 0)){ 
				writer.WriteElementString("category", this.subject); 	
			}
				
			//<guid>
			if((p_id != null) && (p_id.Length != 0) && (p_id.Equals(base.hReference) == false)){ 
				writer.WriteStartElement("guid"); 				
				writer.WriteAttributeString("isPermaLink", "false");
				writer.WriteString(p_id); 
				writer.WriteEndElement();
			}

			//<dc:creator>
			if((this.p_author != null) && (this.p_author.Length != 0)){ 
				writer.WriteElementString("creator", "http://purl.org/dc/elements/1.1/", this.p_author);  	
			}

			//<annotate:reference>
			if((p_parentId != null) && (p_parentId.Length != 0)){ 
				writer.WriteStartElement("annotate", "reference", "http://purl.org/rss/1.0/modules/annotate/");  				
				writer.WriteAttributeString("rdf", "resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#", p_parentId); 
				writer.WriteEndElement();
			}

			// Always prefer <description> 
			if(!noDescriptions && this.HasContent){ 
				/* if(this.ContentType != ContentType.Xhtml){ */ 
				writer.WriteStartElement("description"); 
				writer.WriteCData(this.Content);
				writer.WriteEndElement(); 
				/* }else // if(this.contentType == ContentType.Xhtml)  { 
					writer.WriteStartElement("xhtml", "body",  "http://www.w3.org/1999/xhtml");
					writer.WriteRaw(this.Content); 
					writer.WriteEndElement();
				} */ 

			}

			//<wfw:comment />
			if((this.commentUrl != null) && (this.commentUrl.Length != 0)){ 

				if(this.commentStyle == SupportedCommentStyle.CommentAPI){
					writer.WriteStartElement("wfw", "comment", RssHelper.NsCommentAPI); 
					writer.WriteString(this.commentUrl); 
					writer.WriteEndElement();
				}
			}

			//<wfw:commentRss />
			if((this.commentRssUrl != null) && (this.commentRssUrl.Length != 0)){ 				
				writer.WriteStartElement("wfw", "commentRss", RssHelper.NsCommentAPI); 
				writer.WriteString(this.commentRssUrl); 
				writer.WriteEndElement();				
			}

			//<slash:comments>
			if(this.commentCount != NoComments){
			
				writer.WriteStartElement("slash", "comments", "http://purl.org/rss/1.0/modules/slash/"); 
				writer.WriteString(this.commentCount.ToString()); 
				writer.WriteEndElement(); 
			}


		//	if(format == NewsItemSerializationFormat.NewsPaper){
		
				writer.WriteStartElement("fd", "state", "http://www.bradsoft.com/feeddemon/xmlns/1.0/"); 
				writer.WriteAttributeString("read", this.BeenRead ? "1" : "0"); 
				writer.WriteAttributeString("flagged", FlagStatus == Flagged.None ? "0" : "1"); 
				writer.WriteEndElement(); 

		//	} else { 
				//<rssbandit:flag-status />
				if(FlagStatus != Flagged.None){
					//TODO: check: why we don't use the v2004/vCurrent namespace?
					writer.WriteElementString("flag-status", NamespaceCore.Feeds_v2003, FlagStatus.ToString()); 
				}
		//	}


			if(this.p_watchComments){
				//TODO: check: why we don't use the v2004/vCurrent namespace?
				writer.WriteElementString("watch-comments", NamespaceCore.Feeds_v2003, "1"); 
			}

			if(this.HasNewComments){
				//TODO: check: why we don't use the v2004/vCurrent namespace?
				writer.WriteElementString("has-new-comments", NamespaceCore.Feeds_v2003, "1"); 
			}

			//<enclosure />
			if(this.enclosures != null){
				foreach(Enclosure enc in this.enclosures){
					writer.WriteStartElement("enclosure"); 
					writer.WriteAttributeString("url", enc.Url); 
					writer.WriteAttributeString("type", enc.MimeType);
					writer.WriteAttributeString("length", enc.Length.ToString());
					if(enc.Downloaded){
						writer.WriteAttributeString("downloaded", "1");
					}
					if(enc.Duration !=  TimeSpan.MinValue){
						writer.WriteAttributeString("duration", enc.Duration.ToString()); 
					}
					writer.WriteEndElement(); 
				}
			}

			/* everything else */ 
			foreach(string s in this.OptionalElements.Values){
			
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
		public String ToString(NewsItemSerializationFormat format){
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
		public String ToString(NewsItemSerializationFormat format, bool useGMTDate){
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
		public String ToString(NewsItemSerializationFormat format, bool useGMTDate, bool noDescriptions){

			string toReturn; 

			switch(format){
				case NewsItemSerializationFormat.NewsPaper:
				case NewsItemSerializationFormat.RssFeed:
				case NewsItemSerializationFormat.RssItem:
					toReturn = this.ToRssFeedOrItem(format, useGMTDate, noDescriptions);
					break;
				case NewsItemSerializationFormat.NntpMessage:
					toReturn = this.ToNntpMessage();
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
		private String ToRssFeedOrItem(NewsItemSerializationFormat format,bool useGMTDate, bool noDescriptions ){
			
			StringBuilder sb = new StringBuilder(""); 
			XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb)); 
			writer.Formatting = Formatting.Indented; 
			
			if(format == NewsItemSerializationFormat.RssFeed || format == NewsItemSerializationFormat.NewsPaper ){
			
				if(format == NewsItemSerializationFormat.NewsPaper){
					writer.WriteStartElement("newspaper"); 
					writer.WriteAttributeString("type", "newsitem"); 
				}else{

					writer.WriteStartElement("rss"); 
					writer.WriteAttributeString("version", "2.0"); 
				}

				writer.WriteStartElement("channel"); 
				writer.WriteElementString("title", this.FeedDetails.Title); 
				writer.WriteElementString("link", this.FeedDetails.Link); 
				writer.WriteElementString("description", this.FeedDetails.Description); 

				foreach(string s in this.FeedDetails.OptionalElements.Values){
					writer.WriteRaw(s); 	  
				}
			}

			this.WriteItem(writer,useGMTDate, format, noDescriptions); 

			if(format == NewsItemSerializationFormat.RssFeed || format == NewsItemSerializationFormat.NewsPaper ){
			
				writer.WriteEndElement(); 
				writer.WriteEndElement(); 			
			}

			return sb.ToString(); 
		}

		/// <summary>
		/// Converts the object to an XML string containing an RSS 2.0 item.  
		/// </summary>
		/// <returns></returns>
		public override String ToString(){
		
			return this.ToString(NewsItemSerializationFormat.RssItem); 			
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
		private void ProcessTitle(string content){	  			

			//if no title provided then use first 8 words of description						
			if((p_title == null) || (p_title.Length == 0)){													
				p_title = GetFirstWords(HtmlHelper.StripAnyTags(content), 8); 
			}

			//Get around issues with titles that are one big CDATA section. This 
			//should no longer be needed once we move to .NET 2.0 can use ReadContentAsString()
			//in RssParser.cs 
			if(p_title.StartsWith("<![CDATA[")){	
				p_title = p_title.Replace("<![CDATA[",String.Empty).Replace("]]>",String.Empty); 
			}

			// remove carriage return and line feed in title
			// Fixed to use Enviroment.NewLine so that \r\n combos 
			// in windows are replaced with only one slash.
			// Still replace \r and \n individually afterwards in case of 
			// content produced on non-windows machines have these individually.
			p_title = p_title.Replace(Environment.NewLine, " ").Replace("\r", " ").Replace("\n", " "); 
		}
		
		/// <summary>
		/// Initiate to process the out going links from the 
		/// own content property field and refresh the RelationCosmos
		/// (outgoing/incoming link infos).
		/// Call to this function is required on dynamical late load of
		/// the item content.
		/// </summary>
		internal void RefreshRelationCosmos(){
			if (NewsHandler.BuildRelationCosmos) {
				ProcessOutGoingLinks(this.Content, this.Link);
				NewsHandler.RelationCosmosAdd(this);
			}
		}
		
		/// <summary>
		/// Processes the <paramref name="content"/> for outgoing links and populate 
		/// the outgoing links property. 
		/// </summary>
		private void ProcessOutGoingLinks(string content, string baseUrl){

			if (NewsHandler.BuildRelationCosmos) {
				base.outgoingRelationships = HtmlHelper.RetrieveLinks(content, baseUrl);
			} else {
				base.outgoingRelationships = RelationHRefDictionary.Empty;
			}
		}

	
		/// <summary>
		/// Gets the first n number of words from the provided string. 
		/// </summary>
		/// <param name="text">The target string</param>
		/// <param name="wordCount">The number of words to pick</param>
		/// <returns>The firs</returns>
		private static string GetFirstWords(string text, int wordCount) {
		
			if (text == null) 
				return String.Empty;

			return StringHelper.GetFirstWords(text, wordCount) + "(...)";
		}

		/// <summary>
		/// Creates an XPathNavigator over the XML representation of this object. This method
		/// is equivalent to calling CreateNavigator(false)
		/// </summary>
		/// <returns>An XPathNavigator</returns>
		public XPathNavigator CreateNavigator(){					
			return this.CreateNavigator(false); 
		}

		/// <summary>
		/// Creates an XPathNavigator over the XML representation of this object.
		/// </summary>
		/// <param name="standalone">Indicates whether the navigator will be over the single 
		/// RSS item or over a representation of the parent RSS feed with this item 
		/// as the single item within it. When this parameter is true then the navigator 
		/// works over just an RSS item</param>
		/// <returns>An XPathNavigator</returns>
		public XPathNavigator CreateNavigator(bool standalone){
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
		public XPathNavigator CreateNavigator(bool standalone, bool useGMTDate){
		
			NewsItemSerializationFormat format = (standalone == true ? NewsItemSerializationFormat.RssItem : NewsItemSerializationFormat.RssFeed); 
			XPathDocument doc = new XPathDocument(new XmlTextReader(new StringReader(this.ToString(format, useGMTDate))), XmlSpace.Preserve); 
			return doc.CreateNavigator(); 
		}

		/// <summary>
		/// Get the hash code of the object
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode(){				

			if(!StringHelper.EmptyOrNull(base.hReference)){
				return base.hReference.GetHashCode(); 
			}else if(HasContent){
				return this.Content.GetHashCode(); 
			}

			return this.Date.GetHashCode(); //fallback 
		}


		/// <summary>
		/// Compares to see if two NewsItems are identical. Identity just checks to see if they have 
		/// the same link, if both have no link then checks to see if they have the same description
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object  obj){

			if(Object.ReferenceEquals(this, obj)){ return true; }

			NewsItem item = obj as NewsItem; 

			if(item == null) { return false; }
			
			if (this.Id.Equals(item.Id)) {
				return true;
			}

			return false; 									
		}


		/// <summary>
		/// Creates a text based version of this object as NNTP message suitable for posting to a news server
		/// </summary>
		/// <returns>the NewsItem as an NNTP message string</returns>
		private string ToNntpMessage() {
			StringBuilder sb = new StringBuilder();
						

			sb.Append("From: "); 
			sb.Append(p_author); 
		
			try{
				Uri newsgroupUri = new Uri(this.FeedDetails.Link); 
				sb.Append("\r\nNewsgroups: "); 				
				sb.Append(newsgroupUri.AbsolutePath.Substring(1)); 
			}catch(UriFormatException){}

			sb.Append("\r\nX-Newsreader: "); 
			sb.Append(NewsHandler.GlobalUserAgentString); 
			
			sb.Append("\r\nSubject: "); 
			sb.Append(p_title); 

			if((p_parentId != null) && (p_parentId.Length != 0)){
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
			sb.Append(this.Content); 
			sb.Append("\r\n.\r\n"); 

			return sb.ToString ();
		}

		#region ISizeInfo Members

		/// <summary>
		/// [used to measure mem]
		/// </summary>
		/// <returns></returns>
		public int GetSize() {
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
		public string GetSizeDetails() {
			return this.GetSize().ToString();
		}

		#endregion
	}

	/// <summary>
	/// Represents a NewsItem that shows up in the context of search results over local RSS feeds. 
	/// </summary>
	public class SearchHitNewsItem: NewsItem{
	

		#region Constructors 
		
		/// <summary>
		/// Default constructor is private. 
		/// </summary>
		private SearchHitNewsItem(){;}


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
		public SearchHitNewsItem(feedsFeed feed, string title, string link, string summary, string author, DateTime date, string id):
			base(feed, title, link, null, author, date, id, null){
			this.Enclosures = GetArrayList.Empty;
			this.Summary = summary; 
		}


		/// <summary>
		/// Initialize with a NewsItem 
		/// </summary>
		/// <param name="item"></param>
		public SearchHitNewsItem(NewsItem item): 
			this(item.Feed, item.Title, item.Link, item.Content, item.Author, item.Date, item.Id){
		}
		
		#endregion 

		#region Properties and Fields
		private string summary = null; 
		
		/// <summary>
		/// A text snippet from the actual content of the RSS item. 
		/// </summary>
		public string Summary{
			get { return summary;} 
			
			set { 
				summary = value; 
				/* add to OptionalElements hashtable and make sure XML is properly escaped 
				 * by using XmlTextWriter 
				 */ 
				
				try{
					XmlQualifiedName qname = new XmlQualifiedName("summary", "http://www.w3.org/2005/Atom"); 
					
					using(StringWriter sw = new StringWriter()){
						XmlTextWriter xtw = new XmlTextWriter(sw); 
						xtw.WriteElementString(qname.Name, qname.Namespace, value); 
						xtw.Close(); 					

						this.OptionalElements.Remove(qname); 
						this.OptionalElements.Add(qname, sw.ToString()); 
					}
				}catch(Exception){ /* XML exception if summary contains invalid XML content */	} 
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
		public ExceptionalNewsItem(feedsFeed feed, string title, string link, string summary, string author, DateTime date, string id)
			:
			base(feed, title, link, summary, author, date, id)
		{
			this.SetContent(summary, NewsComponents.ContentType.Html);
		}
	}
}

#region CVS Version Log
/*
 * $Log: NewsItem.cs,v $
 * Revision 1.64  2007/07/08 07:14:45  carnage4life
 * Images don't show up on certain items when clicking on feed or category view if the feed uses relative links such as http://www.tbray.org/ongoing/ongoing.atom
 *
 * Revision 1.63  2007/06/09 18:32:48  carnage4life
 * No results displayed when performing Web searches with Feedster or other search engines that return results as RSS feeds
 *
 * Revision 1.62  2007/03/03 19:05:30  carnage4life
 * Made changes to show duration for podcasts in newspaper view
 *
 * Revision 1.61  2007/02/18 15:24:06  t_rendelmann
 * fixed: null ref. exception on Enclosure property
 *
 * Revision 1.60  2007/02/17 14:45:52  t_rendelmann
 * switched: Resource.Manager indexer usage to strongly typed resources (StringResourceTool)
 *
 * Revision 1.59  2007/02/17 12:34:33  t_rendelmann
 * fixed: p_parentID can also be the empty string
 *
 * Revision 1.58  2007/02/15 16:37:49  t_rendelmann
 * changed: persisted searches now return full item texts;
 * fixed: we do now show the error of not supported search kinds to the user;
 *
 * Revision 1.57  2007/01/17 19:26:38  carnage4life
 * Added initial support for custom newspaper view for search results
 *
 * Revision 1.56  2006/12/19 04:39:51  carnage4life
 * Made changes to AsyncRequest and RequestThread to become instance based instead of static
 *
 * Revision 1.55  2006/12/16 22:26:51  carnage4life
 * Added CopyItemTo method that copies a NewsItem to a specific feedsFeed and does the logic to load item content from disk if needed
 *
 * Revision 1.54  2006/12/09 22:57:03  carnage4life
 * Added support for specifying how many podcasts downloaded from new feeds
 *
 * Revision 1.53  2006/12/03 01:20:13  carnage4life
 * Made changes to support Watched Items feed showing when new comments found
 *
 * Revision 1.52  2006/11/24 17:11:00  carnage4life
 * Items with new comments not remembered on restart
 *
 * Revision 1.51  2006/10/17 10:42:56  t_rendelmann
 * fixed: not all HTML entity encoding handled (like that of SGML without the trailing ";")
 * fixed: now trim the NewsItem.Title to get rid of tabs and spaces
 *
 * Revision 1.50  2006/10/10 12:42:04  carnage4life
 * Fixed some minor issues
 *
 * Revision 1.49  2006/10/05 08:00:13  t_rendelmann
 * refactored: use string constants for our XML namespaces
 *
 */
#endregion
