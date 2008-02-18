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
using System.Xml.XPath;

using NewsComponents.Feed;
using NewsComponents.RelationCosmos;

namespace NewsComponents
{

    /// <summary>
    /// Supported comment styles.
    /// </summary>
    public enum SupportedCommentStyle
    {
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
    /// Item flag states
    /// </summary>
    public enum Flagged
    {
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

	public interface INewsItem : IRelation, ICloneable, IXPathNavigable, IEquatable<INewsItem>
	{

		/// <summary>
		/// Gets the feed link (source the feed is requested from) the item belongs to.
		/// </summary>
		string FeedLink { get; }

		/// <summary>
		/// The link to the item.
		/// </summary>
		string Link { get; }

		/// <summary>
		/// The date the article or blog entry was made. 
		/// </summary>
		DateTime Date { get; set; }		

		/// <summary>
		/// The unique identifier of the parent.
		/// </summary>
		string ParentId { get; }

		/// <summary>
		/// The content of the article or blog entry. 
		/// </summary>
		string Content { get; }

		/// <summary>
		/// Returns true, if Content contains something; else false.
		/// </summary>
		/// <remarks>Should be used instead of testing 
		/// (Content != null &amp;&amp; Content.Length > 0) and is equivalent to 
		/// .ContentType == ContentType.None
		/// </remarks>
		bool HasContent { get; }

		/// <summary>
		/// Set new content of the article or blog entry.
		/// </summary>
		/// <param name="newContent">string</param>
		/// <param name="contentType">ContentType</param>
		void SetContent(string newContent, ContentType contentType) ;

		/// <summary>
		/// Indicates whether the description on this feed is text, HTML or XHTML. 
		/// </summary>
		ContentType ContentType { get; set; }

		/// <summary>
		/// Indicates whether the story has been read or not. 
		/// </summary>
		bool BeenRead { get; set; }

		/// <summary>
		/// Returns an object implementing the FeedDetails interface to which this item belongs
		/// </summary>
		IFeedDetails FeedDetails { get; set; }

		/// <summary>
		/// The author of the article or blog entry 
		/// </summary>
		string Author { get; set; }

		/// <summary>
		/// The title of the article or blog entry. 
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// The subject of the article or blog entry. 
		/// </summary>
		string Subject { get; set; }

		/// <summary>
		/// Returns the amount of comments attached.
		/// </summary>
		int CommentCount { get; set; }

		/// <summary>the URL to post comments to</summary>
		string CommentUrl { get; }

		/// <summary>the URL to get an RSS feed of comments from</summary>
		string CommentRssUrl { get; }

		/// <summary>
		/// Container for all the optional RSS elements for an item. Also 
		/// holds information from RSS modules. The keys in the hashtable 
		/// are instances of XmlQualifiedName while the values are instances 
		/// of XmlNode. 
		/// </summary>
		Hashtable OptionalElements { get; set; }

        /// <summary>
        /// Returns a collection of strings representing URIs to outgoing links in a feed. 
        /// </summary>
        List<string> OutGoingLinks { get; }

        /// <summary>
        /// Returns the feed object to which this item belongs
        /// </summary>
        INewsFeed Feed { get; }

        /// <summary>
        /// Indicates whether the item has been flagged for follow up or not. 
        /// </summary>
        Flagged FlagStatus { get; set;  }

        /// <summary>
        /// Indicates that comments to this item are being watched. 
        /// </summary>
        bool WatchComments { get; set;  }

        /// <summary>
        /// Indicates that there are new comments to this item. 
        /// </summary>
        bool HasNewComments { get; set; }

        /// <summary>
        /// Indicates whether this item supports posting comments. 
        /// </summary>
        SupportedCommentStyle CommentStyle { get; set;  }

         /// <summary>
        /// Gets or sets the language of the entry.
        /// Format of the corresponfing attribute as defined in
        /// http://www.w3.org/TR/REC-xml/#sec-lang-tag;
        /// Format of the language string: 
        /// see http://www.ietf.org/rfc/rfc3066.txt
        /// </summary>
        /// <value>The language.</value>
        string Language { get; }

        /// <summary>
        /// Container of enclosures on the item. If there are no enclosures on the item
        /// then this value is null. 
        /// </summary>
        List<IEnclosure> Enclosures { get; set; }

        /// <summary>
        /// Creates a clone of the INewsItem whose Feed is the provided INewsFeed instance. 
        /// </summary>
        /// <param name="newParent">The parent feed of the cloned item</param>
        /// <returns></returns>
        INewsItem Clone(INewsFeed newParent); 
	}

    /// <summary>
    /// Represents an RSS enclosure
    /// </summary>
    public interface IEnclosure: IEquatable<IEnclosure>
    {      
        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        string MimeType { get; }

        /// <summary>
        /// The length of the enclosure in bytes
        /// </summary>
        long Length { get; }

        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        string Url { get; }

        /// <summary>
        /// The description associated with the item obtained via itunes:subtitle or media:title
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Indicates whether this enclosure has already been downloaded or not.
        /// </summary>
        bool Downloaded { get; set; }

        /// <summary>
        /// Gets the playing time of the enclosure. 
        /// </summary>
        TimeSpan Duration { get; set; }

      
    }

}