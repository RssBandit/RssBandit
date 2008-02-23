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
        List<INewsItem> ItemsList { get; set; }        

        /// <summary>
        /// The unique identifier for the feed
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Writes the feed to the provided XML writer
        /// </summary>
        /// <param name="writer"></param>
        void WriteTo(XmlWriter writer);        
	}
}