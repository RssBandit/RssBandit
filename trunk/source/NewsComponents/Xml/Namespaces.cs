#region CVS Version Header
/*
 * $Id: Namespaces.cs,v 1.1 2006/10/05 14:43:43 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/10/05 14:43:43 $
 * $Revision: 1.1 $
 */
#endregion

using NewsComponents.Xml.Serialization;

namespace NewsComponents
{
	/// <summary>
	/// NewsComponents Namespaces.
	/// </summary>
	public sealed class NamespaceCore
	{
		public const string Feeds_v2003 = "http://www.25hoursaday.com/2003/RSSBandit/feeds/";
		public const string Feeds_v2004 = "http://www.25hoursaday.com/2004/RSSBandit/feeds/";
		public const string Feeds_vCurrent = Feeds_v2004;
		
		private NamespaceCore(){}
	}

	#region NamespaceXml

	/// <summary>
	/// Provides public constants for wellknown XML namespaces.
	/// </summary>
	/// <remarks>Author: Daniel Cazzulino, kzu.net@gmail.com</remarks>
	public sealed class NamespaceXml {

		#region Ctor

		private NamespaceXml() {}

		#endregion Ctor

		#region Public Constants

		/// <summary>
		/// The public XML 1.0 namespace. 
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/2004/REC-xml-20040204/</remarks>
		public const string Xml = "http://www.w3.org/XML/1998/namespace";

		/// <summary>
		/// Public Xml Namespaces specification namespace. 
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/REC-xml-names/</remarks>
		public const string XmlNs = "http://www.w3.org/2000/xmlns/";

		/// <summary>
		/// Public Xml Namespaces prefix. 
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/REC-xml-names/</remarks>
		public const string XmlNsPrefix = "xmlns";

		/// <summary>
		/// XML Schema instance namespace.
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/xmlschema-1/</remarks>
		public const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";

		/// <summary>
		/// XML 1.0 Schema namespace.
		/// </summary>
		/// <remarks>See http://www.w3.org/TR/xmlschema-1/</remarks>
		public const string Xsd = "http://www.w3.org/2001/XMLSchema";

		#endregion Public Constants
	}

	#endregion

	
	/// <summary>
	/// Provides a instance of the XmlSerializerCache 
	/// </summary>
	public sealed class XmlHelper
	{
		private XmlHelper() {}
				
		/// <summary>
		/// Returns a instance of XmlSerializerCache.
		/// </summary>
		/// <returns>Instance of the XmlSerializerCache class</returns>
		public static XmlSerializerCache SerializerCache {
			get { return InstanceHelper.instance; }
		}

		/// <summary>
		/// Private instance helper class to impl. Singleton
		/// </summary>
		private class InstanceHelper {
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static InstanceHelper() {;}
			internal static readonly XmlSerializerCache instance = new XmlSerializerCache();
		}
	}
}

#region CVS Version Log
/*
 * $Log: Namespaces.cs,v $
 * Revision 1.1  2006/10/05 14:43:43  t_rendelmann
 * added the XmlSerializerCache code from the Mvp.Xml project (to prevent the Xml Serializer leak)
 *
 * Revision 1.1  2006/10/05 08:00:13  t_rendelmann
 * refactored: use string constants for our XML namespaces
 *
 */
#endregion
