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
using System.Xml.XPath;

namespace NewsComponents
{

	public interface INewsItem : ICloneable, IXPathNavigable, IEquatable<INewsItem>
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
		/// The unique identifier.
		/// </summary>
		string Id { get; }

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

	}

}