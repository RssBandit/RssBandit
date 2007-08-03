#region CVS Version Header
/*
 * $Id: IFeedDetails.cs,v 1.2 2006/08/10 17:46:53 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/08/10 17:46:53 $
 * $Revision: 1.2 $
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
		Hashtable OptionalElements { get; }
		/// <summary>
		/// Gets the type of the FeedDetails info
		/// </summary>
		FeedType Type { get; }
		/// <summary>? TO DISCUSS ...</summary>	  
		/* TimeSpan MaxItemAge { get; set; } */ 
	}
}