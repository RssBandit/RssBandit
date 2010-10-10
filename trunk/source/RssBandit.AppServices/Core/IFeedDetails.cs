#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml; 


namespace NewsComponents
{
    /// <summary>
    /// Interface represents information about a particular feed
    /// </summary>
    public interface IFeedDetails: ICloneable{
        
        /// <summary>Gets the Feed Language</summary>
        string Language{ get; } 		
        /// <summary>Gets the Feed Title</summary>
        string Title{ get; } 
        /// <summary>Gets the Feed Homepage Link</summary>
        string Link{ get; }
        /// <summary>Gets the Feed Description</summary>
        string Description{ get; }
        /// <summary>Gets the optional elements found at Feed level</summary>	  
        Dictionary<XmlQualifiedName, string> OptionalElements { get; }
        /// <summary>
        /// Gets the type of the FeedDetails info
        /// </summary>
        FeedType Type { get; }
        /// <summary>
        /// The list of news items belonging to the feed
        /// </summary>
        ReadOnlyObservableCollection<INewsItem> ItemsList { get; }

        void AddItem(INewsItem item);
        void ReplaceItems(IEnumerable<INewsItem> newItems);
        void RemoveItem(INewsItem item);
        void RemoveItemAt(int index);


        /// <summary>
        /// The unique identifier for the feed
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Writes the feed to the provided XML writer
        /// </summary>
        /// <param name="writer"></param>
        void WriteTo(XmlWriter writer);        

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
        void WriteTo(XmlWriter writer, NewsItemSerializationFormat format);

        /// <summary>
        /// Writes this object as an RSS 2.0 feed to the specified writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>				
        void WriteTo(XmlWriter writer, NewsItemSerializationFormat format, bool useGMTDate);

        /// <summary>
        /// Provides the XML representation of the feed as an RSS 2.0 feed. 
        /// </summary>
        /// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
        /// <returns>the feed as an XML string</returns>
        string ToString(NewsItemSerializationFormat format);

        /// <summary>
        /// Provides the XML representation of the feed as an RSS 2.0 feed. 
        /// </summary>
        /// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
        /// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
        /// <returns>the feed as an XML string</returns>
        string ToString(NewsItemSerializationFormat format, bool useGMTDate);
            
    }
}