#region CVS Version Header
/*
 * $Id: NewsItem.cs,v 1.29 2005/06/10 18:25:59 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/06/10 18:25:59 $
 * $Revision: 1.29 $
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using NewsComponents.Collections;
using NewsComponents.RelationCosmos;
using NewsComponents.Utils;
using NewsComponents.Feed;


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
		public DateTime Date{ get {return base.aPointInTime; } }
		internal void SetDate(DateTime newDate) { base.PointInTime = newDate; }


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
		/// (Content != null && Content.Length > 0) and is equivalent to 
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

		/// <summary>
		/// An RSS item must always be initialized on construction.
		/// </summary>
		private NewsItem(){;}


		/// <summary>
		/// Initializes an object representation of an RSS item. 
		/// </summary>
		/// <param name="feed">The RSS feed object this item belongs to.</param>
		/// <param name="title">The title of the article or blog entry.</param>
		/// <param name="link">A link to the article or blog entry.</param>
		/// <param name="content">A description of the article or blog entry. This parameter may 
		/// contain markup. </param>
		/// <param name="date">The date the article or blog entry was written or when it was fetched.</param>		
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
		public NewsItem(feedsFeed feed, string title, string link, string content, DateTime date, string subject, ContentType ctype,  Hashtable otherElements, string id, string parentId){

			this.OptionalElements = otherElements; 		
      
			p_feed  = feed; 
			p_title = title; 
			base.hReference = ( link != null ? link.Trim() : null) ;
			/* if (null != base.href)  String.Intern(base.href); BOGUS CALL, DOES NOTHING */ 

			if (null != base.hReference)  
				base.hReference = RelationCosmos.RelationCosmos.UrlTable.Add(base.hReference);

			//escape commonly occuring entities and remove CDATA sections					 
      
			if(content != null){
	
				if(content.StartsWith("<![CDATA[")){			// true, if it is loaded from cache
					content = content.Replace("<![CDATA[",String.Empty).Replace("]]>",String.Empty); 
				}

				content = HtmlHelper.StripBadTags(content);
				//remove invalid XML inserted by MS Word 				
				//description = description.Replace("<?xml:namespace", "<p").Replace("&lt;?xml:namespace", "&lt;p"); 
				//description = description.Replace("<?XML:NAMESPACE", "<p").Replace("&lt;?XML:NAMESPACE", "&lt;p"); 
				//description = description.Replace("o:", "").Replace("st1:", "").Replace("st2:", "");  

			}
					
			//make sure we have a title
			this.ProcessTitle(content); 

			this.ProcessOutGoingLinks(content); 
			this.SetContent(content, ctype);

			p_id = id; 

			// now check for a valid identifier (needed to remember read stories)
			if (p_id == null){			
				int hc = (p_title != null ? p_title.GetHashCode() : 0) + 
					(this.HasContent ? this.Content.GetHashCode() : 0);
				p_id = hc.ToString();
			}
		
			/* String.Intern(p_id); BOGUS CALL, DOES NOTHING */ 
			p_id = RelationCosmos.RelationCosmos.UrlTable.Add(p_id);

			p_parentId = parentId; 
			if (null != p_parentId) 
			{	// dealing with the relationcosmos string comparer (references only!):
				p_parentId = RelationCosmos.RelationCosmos.UrlTable.Add(p_parentId);
				outgoingRelationships.Add(p_parentId, 
					new RelationHRefEntry(p_parentId, String.Empty, outgoingRelationships.Count));
			}
			base.PointInTime = date; 
			this.subject = subject; 

	
		}

	
		private string subject; 
		private SupportedCommentStyle commentStyle; 
		private string commentUrl = null; 
		private string commentRssUrl = null; 
		private int commentCount  = NoComments; 
		private Hashtable optionalElements; 

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
			if (base.GetExternalRelations() != RelationCosmos.RelationCosmos.EmptyRelationList) {
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
				// TorstenR: expand HTML entities in title (property is accessed by the GUI)
				// internally we use the unexpanded version to work with
				string t = HtmlHelper.StripAnyTags(p_author);
				if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
					//t = System.Web.HttpUtility.HtmlDecode(t);
					t = HtmlHelper.HtmlDecode(t);
				}
				return t; 
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
				string t = HtmlHelper.StripAnyTags(p_title);
				if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
					//t = System.Web.HttpUtility.HtmlDecode(t);
					t = HtmlHelper.HtmlDecode(t);
				}
				return t; 
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
		}

		/// <summary>the URL to get an RSS feed of comments from</summary>
		public string CommentRssUrl {

			get { return this.commentRssUrl; }			
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

				if(value != null){
					this.ProcessOptionalElements(); 
				}
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
			return item; 
		}

		/// <summary>
		/// Copies the item (clone) and set the new parent to the provided feed 
		/// </summary>
		/// <param name="f">feedsFeed</param>
		/// <returns></returns>
		public NewsItem CopyTo(feedsFeed f) {
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
			WriteItem(writer, useGMTDate, NewsItemSerializationFormat.RssItem); 
		}

		/// <summary>
		/// Helper function used by ToString(bool). 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="useGMTDate">Indicates whether the time should be written out as GMT or local time</param>
		/// <param name="format">Indicates whether the item is being serialized as part of a FeedDemon newspaper view</param>
		private void WriteItem(XmlWriter writer, bool useGMTDate, NewsItemSerializationFormat format){
		
			//<item>
			writer.WriteStartElement("item"); 

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
			if(this.HasContent){ 
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


			if(format == NewsItemSerializationFormat.NewsPaper){

		
				writer.WriteStartElement("fd", "state", "http://www.bradsoft.com/feeddemon/xmlns/1.0/"); 
				writer.WriteStartAttribute("read", this.BeenRead ? "1" : "0"); 
				writer.WriteStartAttribute("flagged", FlagStatus == Flagged.None ? "0" : "1"); 
				writer.WriteEndElement(); 

			} else { 
				//<rssbandit:flag-status />
				if(FlagStatus != Flagged.None){
					writer.WriteElementString("flag-status", "http://www.25hoursaday.com/2003/RSSBandit/feeds/", FlagStatus.ToString()); 
				}
			}


			/* everything else */ 
			foreach(XmlNode xn in this.OptionalElements.Values){
			
				writer.WriteRaw(xn.OuterXml); 
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
			return ToString(format, true); 
		}

		/// <summary>
		/// Converts the object to an XML string containing an RSS 2.0 item. 
		/// </summary>
		/// <param name="format">Indicates whether an XML representation of an 
		/// RSS item element is returned, an entire RSS feed with this item as its 
		/// sole item or an NNTP message. </param>
		/// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
		/// <returns></returns>
		public String ToString(NewsItemSerializationFormat format, bool useGMTDate){

			string toReturn; 

			switch(format){
				case NewsItemSerializationFormat.NewsPaper:
				case NewsItemSerializationFormat.RssFeed:
				case NewsItemSerializationFormat.RssItem:
					toReturn = this.ToRssFeedOrItem(format, useGMTDate);
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
		/// <returns>An RSS item or RSS feed</returns>
		private String ToRssFeedOrItem(NewsItemSerializationFormat format,bool useGMTDate ){
			
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

				foreach(XmlNode node in this.FeedDetails.OptionalElements.Values){
					writer.WriteRaw(node.OuterXml); 	  
				}
			}

			this.WriteItem(writer,useGMTDate); 

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
		}

		/// <summary>
		/// Makes sure the title isn't null or empty string as well as strips markup. 
		/// </summary>
		private void ProcessTitle(string content){
	  
			//if no title provided then use first 8 words of description
			if((p_title == null) || (p_title.Length == 0)){			
				p_title = GetFirstWords(HtmlHelper.StripAnyTags(content), 8); 
			}

			// remove carriage return and line feed in title
			// Fixed to use Enviroment.NewLine so that \r\n combos 
			// in windows are replaced with only one slash.
			// Still replace \r and \n individually afterwards in case of 
			// content produced on non-windows machines have these individually.
			p_title = p_title.Replace(Environment.NewLine, " ").Replace("\r", " ").Replace("\n", " "); 
		}
		
		/// <summary>
		/// Processes the description property for outgoing links and populate 
		/// the outgoing links property. 
		/// </summary>
		private void ProcessOutGoingLinks(string content){
		     	     
			string baseUrl = (StringHelper.EmptyOrNull(base.hReference) ? p_feed.link : base.hReference);
			base.outgoingRelationships = HtmlHelper.RetrieveLinks(content, baseUrl);
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
				l += p_content.Length / 2;	
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
}
