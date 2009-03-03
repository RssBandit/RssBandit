#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region using


#endregion using

namespace RssBandit.Xml
{
	
	/// <summary>
	/// Provides public constants for wellknown XML namespaces.
	/// </summary>
	public sealed class RssBanditNamespace {
		#region Ctor

		private RssBanditNamespace() {}

		#endregion Ctor

		#region Public Constants

		/// <summary>
		/// Namespace for BrowserTabState XML serialization
		/// </summary>
		public const string BrowserTabState = "http://www.25hoursaday.com/RSSBandit/browsertabstate/1.0";
		

		/// <summary>
		/// Namespace for treeState XML serialization
		/// </summary>
		public const string TreeState = "http://www.25hoursaday.com/RSSBandit/treestate/1.0";
		
		/// <summary>
		/// Namespace for Search Engine Configuration XML serialization
		/// </summary>
		public const string SearchConfiguration = "http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/";
		#endregion
	}
	
}

#region CVS Version Log
/*
 * $Log: Namespaces.cs,v $
 * Revision 1.3  2007/01/30 21:17:43  carnage4life
 * Added support for remembering browser tab state on restart
 *
 * Revision 1.2  2006/10/05 16:07:14  t_rendelmann
 * now using a const for the namespace
 *
 * Revision 1.1  2006/10/05 14:45:06  t_rendelmann
 * added usage of the XmlSerializerCache to prevent the Xml Serializer leak for the new
 * feature: persist the subscription tree state (expansion, selection)
 *
 */
#endregion
