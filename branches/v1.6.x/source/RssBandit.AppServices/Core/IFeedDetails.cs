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
		IDictionary OptionalElements { get; }
		/// <summary>
		/// Gets the type of the FeedDetails info
		/// </summary>
		FeedType Type { get; }
		/// <summary>? TO DISCUSS ...</summary>	  
		/* TimeSpan MaxItemAge { get; set; } */ 
        
        /* HOW COME NO ITEMS COLLECTION? */ 
	}
}