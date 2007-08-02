#region CVS Version Header
/*
 * $Id: IFeedDetails.cs,v 1.1 2005/05/08 17:03:39 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/08 17:03:39 $
 * $Revision: 1.1 $
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