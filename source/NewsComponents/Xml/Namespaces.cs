#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using NewsComponents.Xml.Serialization;

namespace NewsComponents
{
	/// <summary>
	/// NewsComponents Namespaces.
	/// </summary>
	public static class NamespaceCore
	{
		/// <summary>
		/// Historical namespace (required for migration, etc.)
		/// </summary>
		public const string Feeds_v2003 = "http://www.25hoursaday.com/2003/RSSBandit/feeds/";
		/// <summary>
		/// The newest namespace in use
		/// </summary>
		public const string Feeds_v2004 = "http://www.25hoursaday.com/2004/RSSBandit/feeds/";
		/// <summary>
		/// Currently used namespace
		/// </summary>
		public const string Feeds_vCurrent = Feeds_v2004;
	}

	#region NamespaceXml

	/// <summary>
	/// Provides public constants for wellknown XML namespaces.
	/// </summary>
	/// <remarks>Author: Daniel Cazzulino, kzu.net@gmail.com</remarks>
	public static class NamespaceXml {

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
	public static class XmlHelper
	{
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
