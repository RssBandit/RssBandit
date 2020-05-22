using System;
using System.Text;
using Lucene.Net.Documents;
using NewsComponents.Utils;

#pragma warning disable CS0618 // Type or member is obsolete

namespace NewsComponents.Search
{ 
	/// <summary>
	/// A utility class for making Lucene Documents from a NewsItem.
	/// </summary>	
	public class LuceneNewsItemSearch {

		private LuceneNewsItemSearch(){}

		/// <summary>
		/// Creates a document for a NewsItem.
		/// </summary>
		internal static Document Document(INewsItem item) {
			
			// make a new, empty document
			Document doc = new Document();
			
			// NOTE:
			// Field.Keyword() are added and indexed (searchable), but not tokenized
			// Field.Text() are added, indexed (searchable) and tokenized  
			// Field.UnIndexed() are added only

			#region NewsItem fields/keywords

			// Add the uid and feed ID as a field, so that index can be 
			// incrementally maintained. This fields is stored with document,
			// are indexed, but not tokenized prior to indexing.
			// uid can be used to remove/refresh a single feed item,
			// while feedID can be used to remove the indexed docs for a feed
			// at once:
			doc.Add(Field.Keyword(LuceneSearch.IndexDocument.ItemID, UID(item)));
			doc.Add(Field.Keyword(LuceneSearch.IndexDocument.FeedID, item.Feed.id));
			
			doc.Add(Field.Keyword(LuceneSearch.Keyword.ItemLink, CheckNull(item.Link)));
			//TODO: check, if we cannot simpy use the DateTime overload of the Keyword() method
			// remember, the item.Date is UTC here
			//doc.Add(Field.Keyword(LuceneSearch.Keyword.ItemDate, DateToString(item.Date)));
			//doc.Add(Field.Keyword(LuceneSearch.Keyword.ItemDate, item.Date));
			doc.Add(Field.Keyword(LuceneSearch.Keyword.ItemDate,
				DateTools.TimeToString(item.Date.Ticks, DateTools.Resolution.MINUTE)));
			doc.Add(Field.Text(LuceneSearch.Keyword.ItemTitle, CheckNull(item.Title)));
			doc.Add(Field.Text(LuceneSearch.Keyword.ItemAuthor, CheckNull(item.Author)));
			doc.Add(Field.Text(LuceneSearch.Keyword.ItemTopic, CheckNull(item.Subject)));

			if (item.HasContent) 
			{
				StringBuilder content = new StringBuilder(HtmlHelper.StripAnyTags(item.Content));

				// Add the summary as an UnIndexed field, so that it is stored and returned
				// with hit documents for display.
				doc.Add(Field.UnIndexed(LuceneSearch.IndexDocument.ItemSummary, StringHelper.GetFirstWords(content.ToString(), 50)));

				// append the previous stripped outgoing links to get indexed/tokenized:
				foreach (var link in item.OutGoingLinks)
					content.AppendFormat(" {0}", link.Url);	

				// simple text content:
				doc.Add(Field.Text(LuceneSearch.IndexDocument.ItemContent, content.ToString()));

			} else {
				doc.Add(Field.Text(LuceneSearch.IndexDocument.ItemContent, CheckNull(item.Title)));
				doc.Add(Field.UnIndexed(LuceneSearch.IndexDocument.ItemSummary, StringHelper.GetFirstWords(item.Title, 50)));
			}

			#endregion

			#region Feed fields/keywords

			doc.Add(Field.Keyword(LuceneSearch.Keyword.FeedLink, CheckNull(item.FeedLink)));
			doc.Add(Field.Keyword(LuceneSearch.Keyword.FeedUrl, CheckNull(item.FeedDetails.Link)));
			doc.Add(Field.Text(LuceneSearch.Keyword.FeedTitle, CheckNull(item.FeedDetails.Title)));
			doc.Add(Field.Keyword(LuceneSearch.Keyword.FeedType, item.FeedDetails.Type.ToString()));
			// is not really required (thought to be used for scoped searches)
			//doc.Add(Field.Keyword(LuceneSearch.Keyword.FeedCategory, CheckNull(item.Feed.category)));
			doc.Add(Field.Text(LuceneSearch.Keyword.FeedDescription, CheckNull(item.FeedDetails.Description)));

			#endregion

			// return the document
			return doc;
		}
		
#if LUCENE_1_9		
#else // lucene 2.0:
		class Field
		{
			/// <summary>
			/// Field.Keyword() are added and indexed (searchable), but not tokenized
			/// </summary>
			/// <param name="name">The name.</param>
			/// <param name="value">The value.</param>
			/// <returns></returns>
			public static Lucene.Net.Documents.Field Keyword(string name, string value) {
				//return new Lucene.Net.Documents.Field(name, value, true, true, false);
				return new Lucene.Net.Documents.Field(name, value,
				    Lucene.Net.Documents.Field.Store.YES,
				    Lucene.Net.Documents.Field.Index.UN_TOKENIZED);
			}
			/// <summary>
			/// Field.Text() are added, indexed (searchable) and tokenized  
			/// </summary>
			/// <param name="name">The name.</param>
			/// <param name="value">The value.</param>
			/// <returns></returns>
			public static Lucene.Net.Documents.Field Text(string name, string value) {
				return new Lucene.Net.Documents.Field(name, value,
					Lucene.Net.Documents.Field.Store.YES,
					Lucene.Net.Documents.Field.Index.TOKENIZED);
			}
			/// <summary>
			/// Field.UnIndexed() are added only
			/// </summary>
			/// <param name="name">The name.</param>
			/// <param name="value">The value.</param>
			/// <returns></returns>
			public static Lucene.Net.Documents.Field UnIndexed(string name, string value) {
				return new Lucene.Net.Documents.Field(name, value,
					Lucene.Net.Documents.Field.Store.YES,
					Lucene.Net.Documents.Field.Index.NO);
			}
		}
#endif
		
		/// <summary> 
		/// Converts a Date to a string suitable for indexing.
		/// </summary>
		/// <throws>  RuntimeException if the date specified in the 
		/// method argument is before 1970</throws>
		/// <remarks>Check, if we really have to subtract the TimeZone.UtcOffset,
		/// because the date is already UTC!</remarks>
		public static System.String DateToString(System.DateTime date) {
			TimeSpan ts = date.Subtract(new DateTime(1970, 1, 1));
			//TODO: Check, if this have to be removed:
			ts = ts.Subtract(TimeZone.CurrentTimeZone.GetUtcOffset(date));
			return DateField.TimeToString(ts.Ticks / TimeSpan.TicksPerMillisecond);
		}
		
		private static string CheckNull(string s) {
			if (s == null) return String.Empty;
			return s;
		}

		internal static char UrlPathSeparator = '/';
		internal static char UnicodeNullChar = '\u0000';
		
		public static string UID(INewsItem item) {
			string s = String.Concat(item.Feed.id, UnicodeNullChar, item.Id.Replace(UrlPathSeparator, UnicodeNullChar));
			return s;
		}
		
		public static string NewsItemIDFromUID(string uid) {
			return uid.Substring(1+uid.IndexOf(UnicodeNullChar)).Replace(UnicodeNullChar, UrlPathSeparator);
		}
		public static string FeedIDFromUID(string uid) {
			return uid.Substring(0, uid.IndexOf(UnicodeNullChar));
		}
	}
}

#pragma warning restore CS0618 // Type or member is obsolete
