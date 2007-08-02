#region CVS Version Header
/*
 * $Id: RssHelper.cs,v 1.12 2005/04/02 20:01:14 haacked Exp $
 * Last modified by $Author: haacked $
 * Last modified at $Date: 2005/04/02 20:01:14 $
 * $Revision: 1.12 $
 */
#endregion

using System;
using System.Collections;
using System.Xml;

namespace NewsComponents.Utils
{


	/// <summary>
	/// Enumeration used to tell which field a NewsItemComparer should use when comparing NewsItem objects
	/// </summary>
	public enum NewsItemSortField{
		Title,
		Date,
		Subject,
		Author,
		FeedTitle,
		CommentCount,
		Enclosure,
		Flag
	}

	/// <summary>
	/// RssHelper
	/// </summary>
	public sealed class RssHelper {

		private RssHelper() {}

		/// <summary>
		/// </summary>
		public const string NsSlashModules = "http://purl.org/rss/1.0/modules/slash/";
		
		/// <summary>
		/// For spec. see http://dublincore.org/2003/03/24/dces#
		/// </summary>
		public const string NsDcElements    = "http://purl.org/dc/elements/1.1/";

		/// <summary>
		/// For spec. see http://wellformedweb.org/news/wfw_namespace_elements/
		/// </summary>
		public const string NsCommentAPI = "http://wellformedweb.org/CommentAPI/";

		// object needed to create XmlElement's, etc.
		private static XmlDocument elementCreator = new XmlDocument();


		/// <summary>
		/// Returns a array of DateTime with values to be used as initial values 
		/// for last retrieved property to reduce the initial payload of
		/// bulk requests that can be initiated by a large OPML import.
		/// </summary>
		/// <param name="expectedAmountOfBulkRequests">int. Number of expected requests.</param>
		/// <param name="defaultRefreshRateInMSecs">int. Base for the algorithm.</param>
		/// <returns>DateTime[]</returns>
		/// <remarks>
		/// The algorithm currently works this way:
		/// 1. If we get less than 100, we assume to handle them all at once 
		///   and return an array with DateTime.MinValue's
		/// 2. If we have more, we always initialize an array of 103 (magic) entries and set each entry
		///   to a DateTime we calculate starting with DateTime.Now reduced by defaultRefreshRateInMSecs.
		///   Then we add a calculated step determined by the index of the array entry. So we get an array with DateTimes spreading
		///   the whole Time from Now to Now - defaultRefreshRateInMSecs.
		/// 3. Now we overwrite some of them dependent on expectedAmountOfBulkRequests to be
		///   of DateTime.MinValue to force a request: the more expectedAmountOfBulkRequests we
		///   have, the fewer entries are overwritten with MinValue. E.g. for expectedAmountOfBulkRequests=1234 each
		///   19. entry gets the DateTime.MinValue.
		/// </remarks>
		public static DateTime[] InitialLastRetrievedSettings(int expectedAmountOfBulkRequests, int defaultRefreshRateInMSecs) {
			if (expectedAmountOfBulkRequests <= 100)
				return new DateTime[]{DateTime.MinValue, DateTime.MinValue, DateTime.MinValue};

			double defaultRefreshRate = 60 * 60 * 1000;	// one hour
			if (defaultRefreshRateInMSecs > (10*60*1000))		// if at least 10 minutes, take it:
				defaultRefreshRate = defaultRefreshRateInMSecs;

			int max = 103;
			double step = defaultRefreshRate / (max + 1);
			DateTime[] vals = new DateTime[max];

			DateTime startDate = DateTime.Now.AddMilliseconds(-defaultRefreshRate);
			int overwrite = (expectedAmountOfBulkRequests >> 6);
			for (int i = 0; i < max; i++) {	// init the array
				vals[i] = startDate.AddMilliseconds(i*step);
				if ((i % overwrite) == 0)
					vals[i] = DateTime.MinValue;
			}
			
			return vals;
		}

		/// <summary>
		/// Converts a list of NewsItem's to a list of NewsItem.Link's
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		static public IList ItemListToHRefList(IList items) {
			ArrayList retList = new ArrayList(items.Count);
			foreach (NewsItem item in items) {
				if (!StringHelper.EmptyOrNull(item.Link))
					retList.Add(item.Link);
			}
			return retList;
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="item"></param>
		/// <param name="elementName"></param>
		/// <returns></returns>
		static public string GetDcElementValue(NewsItem item, string elementName) {
			return GetOptionalElementValue(item, elementName, NsDcElements);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		static public string GetHashCode(NewsItem item) {
			if (item == null) return null;
			System.Text.StringBuilder newHash = new System.Text.StringBuilder();
			
			if (item.Feed != null && item.Feed.link != null && item.Feed.link.Length > 0)
				newHash.Append(item.Feed.link.GetHashCode().ToString());

			if (item.Link != null && item.Link.Length > 0)
				newHash.Append(item.Link.GetHashCode().ToString());
			
			if (item.Title != null && item.Title.Length > 0)
				newHash.Append(item.Title.GetHashCode().ToString());

			if (item.HasContent)
				newHash.Append(item.Content.GetHashCode().ToString());
			
			return newHash.ToString();
		}

		static private string GetOptionalElementValue(NewsItem item, string elementName, string elementNamespace) {
			if (item.OptionalElements == null || item.OptionalElements.Count == 0)
				return null;

			string retStr = null;
			XmlElement elem = GetOptionalElement(item, elementName, elementNamespace);
			if (elem != null)
				retStr = elem.InnerText; 

			return retStr;
		}

		/// <summary>
		/// Returns the optional XmlElement if found within the NewsItem.OptionalElements list,
		/// else null.
		/// </summary>
		/// <param name="item">NewsItem</param>
		/// <param name="elementName">String</param>
		/// <param name="elementNamespace">String</param>
		/// <returns></returns>
		static public XmlElement GetOptionalElement(NewsItem item, string elementName, string elementNamespace) {
			if (item == null || item.OptionalElements == null)
				return null;
			return GetOptionalElement(item.OptionalElements, elementName, elementNamespace);
		}
		
		/// <summary>
		/// Returns the optional XmlElement if found within the NewsItem.OptionalElements list,
		/// else null.
		/// </summary>
		/// <param name="item">NewsItem</param>
		/// <param name="qName">XmlQualifiedName</param>
		/// <returns>XmlElement</returns>
		static public XmlElement GetOptionalElement(NewsItem item, XmlQualifiedName qName) {
			if (item == null || item.OptionalElements == null || qName == null)
				return null;
			return GetOptionalElement(item.OptionalElements, qName.Name, qName.Namespace);
		}

		/// <summary>
		/// Returns the optional XmlElement if found within the provided list,
		/// else null.
		/// </summary>
		/// <param name="elements"></param>
		/// <param name="elementName"></param>
		/// <param name="elementNamespace"></param>
		/// <returns></returns>
		static public XmlElement GetOptionalElement(IDictionary elements, string elementName, string elementNamespace) {
			if (elements == null || elements.Count == 0)
				return null;
			
			XmlQualifiedName qname = GetOptionalElementKey(elements, elementName, elementNamespace);
			if (qname != null) {
				XmlElement elem  = (XmlElement) elements[qname];
				if(elem != null) {
					return elem; 
				}
			}
			return null;
		}
		/// <summary>
		/// Returns the optional XmlElement if found within the provided list,
		/// else null.
		/// </summary>
		/// <param name="elements"></param>
		/// <param name="qName">XmlQualifiedName</param>
		/// <returns>XmlQualifiedName</returns>
		static public XmlQualifiedName GetOptionalElementKey(IDictionary elements, XmlQualifiedName qName) {
			if (elements == null || elements.Count == 0 || qName == null)
				return null;
			return GetOptionalElementKey(elements, qName.Name, qName.Namespace);
		}
		
		/// <summary>
		/// Returns the optional XmlElement if found within the provided list,
		/// else null.
		/// </summary>
		/// <param name="elements"></param>
		/// <param name="elementName"></param>
		/// <param name="elementNamespace"></param>
		/// <returns>XmlQualifiedName</returns>
		static public XmlQualifiedName GetOptionalElementKey(IDictionary elements, string elementName, string elementNamespace) {
			if (elements == null || elements.Count == 0)
				return null;

			//have to do this because XmlQualifiedName.Equals() is stupid
			foreach(XmlQualifiedName qname in elements.Keys){						
				if (qname.Namespace.Equals(elementNamespace) && qname.Name.IndexOf(elementName) >= 0) {
					return qname;
				}
			}
			return null;
		}

		/// <summary>
		/// Creates and returns a new XmlElement with the specified
		/// prefix, elementName, elementNamespace and value.
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="elementName">String</param>
		/// <param name="elementNamespace">String</param>
		/// <param name="value">String</param>
		/// <returns>XmlElement</returns>
		static public XmlElement CreateXmlElement(string prefix, string elementName, string elementNamespace, string value) {
			XmlElement e = elementCreator.CreateElement(prefix, elementName, elementNamespace); 
			e.InnerText  = value; 
			return e;
		}

		/// <summary>
		/// Returns the default NewsItem comparer.
		/// </summary>
		/// <returns>Ascending sorting NewsItemComparer (by Date)</returns>
		static public IComparer GetComparer() {
			return new NewsItemComparer();
		}
		/// <summary>
		/// Returns a NewsItem comparer.
		/// </summary>
		/// <param name="sortDescending">Set to False, if it should sort ascending (by date), 
		/// else true</param>
		/// <returns></returns>
		static public IComparer GetComparer(bool sortDescending) {
			return new NewsItemComparer(sortDescending);
		}


		/// <summary>
		/// Returns a NewsItem comparer.
		/// </summary>
		/// <param name="sortDescending">Set to False, if it should sort ascending, 
		/// else true</param>
		/// <param name="sortField">indicates which field on the NewsItem object should be used for 
		/// sorting</param>
		/// <returns></returns>
		static public IComparer GetComparer(bool sortDescending, NewsItemSortField sortField) {
			return new NewsItemComparer(sortDescending, sortField);
		}

		internal class NewsItemComparer: IComparer {
		
			#region private fields

			private bool sortDescending;
			private NewsItemSortField sortField;
			 
			#endregion 

			#region Constructors 

			public NewsItemComparer():this(false, NewsItemSortField.Date)  {}

			public NewsItemComparer(bool sortDescending):this(sortDescending, NewsItemSortField.Date){}
			
			public NewsItemComparer(NewsItemSortField sortField):this(false, sortField) {}

			public NewsItemComparer(bool sortDescending, NewsItemSortField sortField) {
				this.sortField = sortField;
				this.sortDescending = sortDescending;
			}

			#endregion

			#region Implementation of IComparer
			public int Compare(object x, object y) {
				
				NewsItem ri1 = x as NewsItem, ri2 = y as NewsItem;
				
				if (ri1 == null || ri2 == null)
					return 0;
				
				int reverse = (this.sortDescending ? 1: -1);

				switch(this.sortField){
					case NewsItemSortField.Date:
						return reverse * DateTime.Compare(ri2.Date, ri1.Date);

					case NewsItemSortField.Author:
						return reverse * String.Compare(ri2.Author, ri1.Author); 

					case NewsItemSortField.CommentCount:
						return  reverse * ri2.CommentCount.CompareTo(ri1.CommentCount); 

					case NewsItemSortField.Subject:
						return reverse * String.Compare(ri2.Subject, ri1.Subject); 

					case NewsItemSortField.Title:
						return reverse * String.Compare(ri2.Title, ri1.Title); 
					
					case NewsItemSortField.FeedTitle:
						return reverse * String.Compare(ri2.Feed.title, ri1.Feed.title);

					case NewsItemSortField.Flag:
						return reverse * ri2.FlagStatus.CompareTo(ri1.FlagStatus);

					case NewsItemSortField.Enclosure:
						XmlElement x1 = RssHelper.GetOptionalElement(ri1, "enclosure", String.Empty);
						XmlElement x2 = RssHelper.GetOptionalElement(ri2, "enclosure", String.Empty);
						if (x1 == null && x2 == null) return 0;
						if (x2 != null) 
							return reverse * 1;
						else
							return reverse * -1;

					default: 
						return 0; 
				}
			}
		
			#endregion
		}

	
	}

}
