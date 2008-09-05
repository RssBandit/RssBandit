#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;

namespace RssBandit
{
	internal static class OptionalItemElement
	{
		#region Const defs.

		/// <summary>
		/// Used for migration to version 1.7.x and higher
		/// </summary>
		internal static class OldVersion
		{
			/// <summary>
			/// Gets the common old name space used for RSS Bandit extensions
			/// </summary>
			public const string Namespace = NamespaceCore.Feeds_v2003;

			/// <summary>
			/// Gets the former common name space extension prefix
			/// </summary>
			public const string OldElementPrefix = "bandit";

			/// <summary>
			/// Gets the common name space extension prefix
			/// </summary>
			public const string ElementPrefix = "rssbandit";

			/// <summary>
			/// Gets the element name used to store flag state of new items
			/// </summary>
			public const string FeedUrlElementName = "feed-url";

			/// <summary>
			/// Gets the element name used to keep the feed URL of deleted news items 
			/// </summary>
			public const string ContainerUrlElementName = "container-url";

			/// <summary>
			/// Gets the element name used to keep the feed URL of failed feeds 
			/// (messages stored as news items) 
			/// </summary>
			public const string ErrorElementName = "failed-url";

		}
		
		/// <summary>
		/// Gets the current RssBandit name space extension prefix
		/// </summary>
		public static string Prefix { get { return NamespaceCore.BanditPrefix; } }

		/// <summary>
		/// Gets the element name used to store the original feed reference of a news item (link back to original owner feed and source)
		/// </summary>
		const string OriginalFeedRef = "feed-ref";
		
		/// <summary>
		/// Gets the attribute name used to store the feed source id (link back to original owner feed source)
		/// </summary>
		const string OriginalSourceID = "source-id";

		#endregion
		/// <summary>
		/// Adds the or replace the optional feed reference element.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="sourceID">The source ID.</param>
		/// <returns></returns>
		public static XmlElement AddOrReplaceOriginalFeedReference(INewsItem newsItem, string feedUrl, int sourceID)
		{
			if (newsItem == null)
				throw new ArgumentNullException("newsItem");

			XmlQualifiedName key = AdditionalElements.GetQualifiedName(OriginalFeedRef);
			XmlQualifiedName attrKey = AdditionalElements.GetQualifiedName(OriginalSourceID);
			
			if (newsItem.OptionalElements == null)
				newsItem.OptionalElements = new Dictionary<XmlQualifiedName, string>(2);

			if (newsItem.OptionalElements.ContainsKey(key))
				newsItem.OptionalElements.Remove(key);

			XmlElement element = RssHelper.CreateXmlElement(Prefix, key, feedUrl);
			element.Attributes.Append(RssHelper.CreateXmlAttribute(String.Empty, attrKey, sourceID.ToString()));
			newsItem.OptionalElements.Add(key, element.OuterXml);
			
			return element;
		}

		/// <summary>
		/// Returns the feed URL and source ID of the optional feed reference element for this <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <param name="sourceID">out: The source ID. -1 in case no source ID is available</param>
		/// <returns>
		/// The feed URL of the source feed if a pointer to it exists and NULL otherwise.
		/// </returns>
		public static string GetOriginalFeedReference(INewsItem newsItem, out int sourceID)
		{
			if (newsItem == null)
				throw new ArgumentNullException("newsItem");
			
			sourceID = -1;
			string feedUrl = null;
			XmlQualifiedName key = AdditionalElements.GetQualifiedName(OriginalFeedRef);

			if (key != null && newsItem.OptionalElements.ContainsKey(key))
			{
				string str = newsItem.OptionalElements[key];

				int startIndex = str.IndexOf(">") + 1;
				int endIndex = str.LastIndexOf("<");
				feedUrl = str.Substring(startIndex, endIndex - startIndex);

				endIndex = startIndex;
				startIndex = str.IndexOf(OriginalSourceID + "=", 0, endIndex);
				if (startIndex > 0)
				{
					startIndex = startIndex + (OriginalSourceID + "=").Length + 1;
					endIndex = str.IndexOf("\"", startIndex);
					string fs = str.Substring(startIndex, endIndex - startIndex);
					if (!String.IsNullOrEmpty(fs))
						Int32.TryParse(fs, out sourceID);
				}
			}

			return feedUrl;
		}

		/// <summary>
		/// Returns the feed URL and source ID of the optional feed reference element for this <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsFeed">The news feed.</param>
		/// <param name="sourceID">out: The source ID. -1 in case no source ID is available</param>
		/// <returns>
		/// The feed URL of the source feed if a pointer to it exists and NULL otherwise.
		/// </returns>
		public static string GetOriginalFeedReference(INewsFeed newsFeed, out int sourceID)
		{
			if (newsFeed == null)
				throw new ArgumentNullException("newsFeed");

			sourceID = -1;
			string feedUrl = null;
			XmlQualifiedName key = AdditionalElements.GetQualifiedName(OriginalFeedRef);
			XmlQualifiedName attrKey = AdditionalElements.GetQualifiedName(OriginalSourceID);
			
			if (key != null && newsFeed.Any != null && newsFeed.Any.Length > 0)
			{
				XmlElement origin = newsFeed.Any[0];
				feedUrl = origin.InnerText;

				if (origin.Attributes.Count > 0)
				{
					XmlNode a = origin.Attributes.GetNamedItem(attrKey.Name, attrKey.Namespace);
					if (a != null)
					{
						string fs = a.InnerText;
						if (!String.IsNullOrEmpty(fs))
							Int32.TryParse(fs, out sourceID);
					}
				}
			}

			return feedUrl;
		}
		/// <summary>
		/// Removes the optional feed reference element from <paramref name="newsItem"/>'s optional elements.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		public static void RemoveOriginalFeedReference(INewsItem newsItem)
		{
			if (newsItem == null)
				throw new ArgumentNullException("newsItem");

			XmlQualifiedName key = AdditionalElements.GetQualifiedName(OriginalFeedRef);

			if (key != null && newsItem.OptionalElements.ContainsKey(key))
			{
				newsItem.OptionalElements.Remove(key);
			}
		}

	}

	/// <summary>
	/// AdditionalElements contains the RSS Bandit owned
	/// additional feed XML elements/attributes and name spaces
	/// used to annotate, keep relations between feeds etc.
	/// </summary>
	internal static class AdditionalElements
	{
		#region Const defs.

		/// <summary>
		/// Gets the common current name space used for RSS Bandit extensions
		/// </summary>
		public const string Namespace = NamespaceCore.Feeds_vCurrent;
		
		
		#endregion

		
		#region public members

		/// <summary>
		/// Returns the value of a optional element for the specified <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="newsItem">The news item.</param>
		/// <returns>
		/// The value (content) of the optional element if it exists and NULL otherwise.
		/// </returns>
		public static string GetElementValue(XmlQualifiedName key, INewsItem newsItem)
		{
			string val = null;
			
			if (key != null && newsItem.OptionalElements != null && newsItem.OptionalElements.ContainsKey(key))
			{
				string str = newsItem.OptionalElements[key];

				int startIndex = str.IndexOf(">") + 1;
				int endIndex = str.LastIndexOf("<");
				val = str.Substring(startIndex, endIndex - startIndex);
			}

			return val;
		}

		/// <summary>
		/// Removes the optional element for the specified <paramref name="newsItem"/> from the optional elements.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="newsItem">The news item.</param>
		public static void RemoveElement(XmlQualifiedName key, INewsItem newsItem)
		{
			if (key != null && newsItem.OptionalElements != null && newsItem.OptionalElements.ContainsKey(key))
			{
				newsItem.OptionalElements.Remove(key);
			}
		}


		///// <summary>
		///// Gets the <see cref="XmlQualifiedName"/> used to reference 
		///// the original feed of flagged items.
		///// </summary>
		///// <value>XmlQualifiedName</value>
		//public static XmlQualifiedName OriginalFeedOfFlaggedItem 
		//{
		//    get { return GetQualifiedName(FeedUrlElementName); }
		//}
		///// <summary>
		///// Gets the <see cref="XmlQualifiedName"/> used to reference 
		///// the original feed of sent items.
		///// </summary>
		///// <value>XmlQualifiedName</value>
		//public static XmlQualifiedName OriginalFeedOfSentItem
		//{
		//    get { return GetQualifiedName(FeedUrlElementName); }
		//}  
		///// <summary>
		///// Gets the <see cref="XmlQualifiedName"/> used to reference 
		///// the original feed of deleted items.
		///// </summary>
		///// <value>XmlQualifiedName</value>
		//public static XmlQualifiedName OriginalFeedOfDeletedItem 
		//{
		//    get { return GetQualifiedName(ContainerUrlElementName); }
		//} 


		///// <summary>
		///// Gets the <see cref="XmlQualifiedName"/> used to reference 
		///// the original feed of watched items.
		///// </summary>
		///// <value>XmlQualifiedName</value>
		//public static XmlQualifiedName OriginalFeedOfWatchedItem 
		//{
		//    get { return GetQualifiedName(FeedUrlElementName); }
		//} 

		///// <summary>
		///// Gets the <see cref="XmlQualifiedName"/> used to reference 
		///// the original feed of deleted items.
		///// </summary>
		///// <value>XmlQualifiedName</value>
		//public static XmlQualifiedName OriginalFeedOfErrorItem
		//{
		//    get { return GetQualifiedName(ErrorElementName); }
		//}

		#endregion

		#region private members

		/// <summary>
		/// Qualified names cache
		/// </summary>
		private static readonly Dictionary<string, XmlQualifiedName> _names = new Dictionary<string, XmlQualifiedName>();
		private static readonly object creationLock = new object();

		/// <summary>
		/// Gets the name of the qualified.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		internal static XmlQualifiedName GetQualifiedName(string key)
		{
			if (key == null) return null;
			return GetQualifiedName(key, Namespace);
		}

		/// <summary>
		/// Gets the name of the qualified.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="xmlNamespace">The XML name space.</param>
		/// <returns></returns>
		internal static XmlQualifiedName GetQualifiedName(string key, string xmlNamespace)
		{
			if (key == null) return null;

			XmlQualifiedName name;
			if (!_names.TryGetValue(key, out name))
			{
				lock (creationLock)
				{
					if (!_names.TryGetValue(key, out name))
					{
						name = new XmlQualifiedName(key, xmlNamespace);
						_names.Add(key, name);
					}
				}
			}
			return name;
		}
		
		#endregion

	}
}

