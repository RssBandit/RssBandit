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
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml.XPath;
using NewsComponents.Feed;
using NewsComponents.Search.BooleanSearch;
using NewsComponents.Utils;

namespace NewsComponents.Search
{
	/// <summary>
	/// Interface to abstract a search criteria implementation
	/// </summary>
	[XmlInclude(typeof(SearchCriteriaAge)), 
	XmlInclude(typeof(SearchCriteriaString)),
	XmlInclude(typeof(SearchCriteriaProperty))]
	public abstract class ISearchCriteria {
		/// <summary>
		/// Method to implement for a single NewsItem compare. Should return true,
		/// if the criteria match that item.
		/// </summary>
		/// <param name="item">NewsItem to comapre (work on)</param>
		/// <returns>true, if this criteria match that item</returns>
		public abstract bool Match(INewsItem item);
		/// <summary>
		/// Method to implement for a feed compare. Should return true,
		/// if the criteria match that feed.
		/// </summary>
		/// <param name="feed">FeedInfo to compare (work on)</param>
		/// <returns>true, if the feed match</returns>
		public abstract bool Match(FeedInfo feed);
	}

	/// <summary>
	/// Defines the properties (where) to search on an NewsItem.
	/// </summary>
	[Flags]
	public enum SearchStringElement {
		/// <summary>
		/// None, undefined state
		/// </summary>
		Undefined = 0,
		/// <summary>
		/// Search within an NewsItem title.
		/// </summary>
		Title = 1, 
		/// <summary>
		/// Search within an NewsItem content (description).
		/// </summary>
		Content = 2, 
		/// <summary>
		/// Search within an NewsItem subject (topic or category).
		/// </summary>
		Subject = 4,
		/// <summary>
		/// Search within an NewsItem link
		/// </summary>
		Link = 8, 
		/// <summary>
		/// Search within an NewsItem author
		/// </summary>
		Author = 16, 
		/// <summary>
		/// Search within all (title and content and subject and link)
		/// </summary>
		All = Title | Content | Subject | Link | Author
	}
	
	/// <summary>
	/// Defines the kind of the search expression
	/// </summary>
	public enum StringExpressionKind {
		/// <summary>Simple text expression</summary>
		Text,
		/// <summary>Lucene search expression</summary>
		LuceneExpression,
		/// <summary>Regular text expression</summary>
		RegularExpression,
		/// <summary>XPath expression</summary>
		XPathExpression
	}
	/// <summary>
	/// Defines the Date comparison operator to be used.
	/// </summary>
	public enum DateExpressionKind {
		/// <summary>Dates of NewsItem's (Time is ignored) have to be equal</summary>
		Equal,
		/// <summary>Dates of NewsItem's (Time is ignored) have to be older than the specified</summary>
		OlderThan,
		/// <summary>Dates of NewsItem's (Time is ignored) have to be newer than the specified</summary>
		NewerThan,
	}
	/// <summary>
	/// Defines, what other properties of an NewsItem should be considered in a search
	/// </summary>
	public enum PropertyExpressionKind {
		/// <summary>Consider read state on NewsItem's</summary>
		Unread,
		/// <summary>Consider flag state on NewsItem's</summary>
		Flagged
	}

	/// <summary>
	/// Defines how to handle the NewsItem read state
	/// </summary>
	public enum ItemReadState {
		Ignore,
		BeenRead,
		Unread,
	}

	/// <summary>
	/// Implements a collection of ISearchCriteria's
	/// </summary>
	[Serializable]
	public class SearchCriteriaCollection: ICollection {

		ArrayList criteriaList = new ArrayList(1); // list of ISearchCriteria instances
		/// <summary>
		/// Method applies the match call to all contained search criteria's for the
		/// specified NewsItem. It will only return true, if ALL criteria match on that item!
		/// </summary>
		/// <param name="item">NewsItem</param>
		/// <returns>true, if ALL criteria matches that item, else false</returns>
		public bool Match(INewsItem item) { 
			if (criteriaList.Count == 0)
				return false;
			//iterate all criteria and return true if all match ...(AND)
			foreach(ISearchCriteria sc in criteriaList){
				if(!sc.Match(item))
					return false;
			}
			return true;
		}
		/// <summary>
		/// Method applies the match call to all contained search criteria's for the
		/// specified feed. It will only return true, if ALL criteria match on that feed!
		/// </summary>
		/// <param name="feed">FeedInfo</param>
		/// <returns>true, if ALL criteria matches that feed, else false</returns>
		public bool Match(FeedInfo feed) { 
			if (criteriaList.Count == 0)
				return false;
			foreach(ISearchCriteria sc in criteriaList){
				if(!sc.Match(feed))
					return false;
			}
			return true;
		}
		/// <summary>
		/// Add a criteria to the list
		/// </summary>
		/// <param name="criteria">ISearchCriteria to add</param>
		public void Add(ISearchCriteria criteria) { 
			criteriaList.Add(criteria);
		}
		/// <summary>
		/// Removes a criteria from the list
		/// </summary>
		/// <param name="criteria">ISearchCriteria to remove</param>
		public void Remove(ISearchCriteria criteria) {
			criteriaList.Remove(criteria);
		}

		/// <summary>
		/// Get/Set a specific criteria at the defined position
		/// </summary>
		public ISearchCriteria this[int criteria] {
			get {	return (ISearchCriteria)criteriaList[criteria];	}
			set {	criteriaList[criteria] = value;		}
		}

		/// <summary>
		/// Returns the count of contained criteria
		/// </summary>
		public int Count {
			get { return criteriaList.Count; }
		}
		#region ICollection Members

		public bool IsSynchronized { get {	return criteriaList.IsSynchronized;}	}
		public void CopyTo(Array array, int index) { criteriaList.CopyTo(array, index);	}
		public object SyncRoot { get {	return criteriaList.SyncRoot;	}	}

		#endregion

		#region IEnumerable Members
		public IEnumerator GetEnumerator() {	return criteriaList.GetEnumerator();	}
		#endregion
	}

	/// <summary>
	/// Class impl. ISearchCriteria to compare item ages (or: dates)
	/// </summary>
	/// <remarks>Either <c>What</c> or <c>WhatRelativeToToday</c> have to be
	/// specified!</remarks>
	[Serializable]
	public class SearchCriteriaAge: ISearchCriteria {
		
		/// <summary>
		/// This is a string version of the WhatRelativeToToday property because instances of 
		/// System.TimeSpan cannot be serialized by the XmlSerializer. 
		/// </summary>
		[XmlElement("WhatRelativeToToday")]
		public string WhatRelativeToTodayString{
			get { 
				return WhatRelativeToToday.ToString(); 
			}
			set { 
				WhatRelativeToToday = TimeSpan.Parse(value); 
			}		
		}

		/// <summary>
		/// TimeSpan specifies a relative date or time to compare an NewsItem published date/time to.
		/// </summary>
		[XmlIgnore()]
		public TimeSpan WhatRelativeToToday {
			get {
				return whatRelativeToToday; 
			}
			set { 
				whatRelativeToToday = value; 
				if (whatRelativeToToday > TimeSpan.Zero) {
					what = DateTime.MinValue;
					whatYearOnly = 0; 
				}
			}
		}
		
		internal int WhatAsIntDateOnly {
			get { return whatYearOnly; }
		}
		
		/// <summary>
		/// Defines, how the comparison works
		/// </summary>
		public DateExpressionKind WhatKind;

		/// <summary>
		///  Defines the Date to compare to.
		/// </summary>
		public DateTime What {
			get { 
				return what; 
			}
			set { 
				what = value; 
				whatYearOnly = DateTimeExt.DateAsInteger(what); 
				if (what > DateTime.MinValue)
					whatRelativeToToday = TimeSpan.Zero;
			}
		}
		private TimeSpan whatRelativeToToday;
		private int whatYearOnly;	// store the pre-calced year only integer
		private DateTime what;

		/// <summary>
		/// Default initializer
		/// </summary>
		public SearchCriteriaAge() {
			this.WhatRelativeToToday = TimeSpan.Zero;
			this.What = DateTime.MinValue;
		}
		/// <summary>
		/// Overloaded initializer
		/// </summary>
		/// <param name="whatKind"></param>
		public SearchCriteriaAge(DateExpressionKind whatKind):this(){		
			this.WhatKind = whatKind; 
		}
		/// <summary>
		/// Overloaded initializer
		/// </summary>
		/// <param name="what"></param>
		/// <param name="whatKind"></param>
		public SearchCriteriaAge(DateTime what, DateExpressionKind whatKind):this(whatKind){		
			this.What = what; 
		}
		/// <summary>
		/// Overloaded initializer
		/// </summary>
		/// <param name="whatRelative"></param>
		/// <param name="whatKind"></param>
		public SearchCriteriaAge(TimeSpan whatRelative, DateExpressionKind whatKind):this(whatKind){		
			this.WhatRelativeToToday = whatRelative; 
		}


		
		/// <summary>
		/// interface impl. ISearchCriteria
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool Match(INewsItem item) { 
			if (this.WhatRelativeToToday.CompareTo(TimeSpan.Zero) == 0) {
				// compare dates only
				int itemDate = DateTimeExt.DateAsInteger(item.Date);
				switch(this.WhatKind){	// we don't consider the date as UTC!?
					case DateExpressionKind.Equal:
						return itemDate == whatYearOnly;
					case DateExpressionKind.OlderThan:
						return itemDate < whatYearOnly;
					case DateExpressionKind.NewerThan:
						return itemDate > whatYearOnly;
					default:
						return false; 
				}
			} else {
				DateTime dt = DateTime.Now.ToUniversalTime().Subtract(this.WhatRelativeToToday);
				switch(this.WhatKind){	
					case DateExpressionKind.OlderThan:
						return item.Date <= dt; 
					case DateExpressionKind.NewerThan:
						return item.Date >= dt;
					default:
						return false; 
				}
			}
		
		}  
		/// <summary>
		/// [Not yet implemented]
		/// </summary>
		/// <param name="feed"></param>
		/// <returns></returns>
		public override bool Match(FeedInfo feed) { 
					
			return false;
		} 
	}
	
	/// <summary>
	/// Class impl. ISearchCriteria to compare item ages (or: dates)
	/// </summary>
	/// <remarks>Both <c>Bottom</c> and <c>Top</c> have to be
	/// specified!</remarks>
	[Serializable]
	public class SearchCriteriaDateRange: ISearchCriteria {
		
		/// <summary>
		/// Defines the minimum range value: Jan., 1. 1980
		/// </summary>
		public static readonly DateTime MinValue = new DateTime(1980, 1, 1);
		
		/// <summary>
		///  Defines the lower Date of the range.
		/// </summary>
		public DateTime Bottom {
			get { 
				return lowDate; 
			}
			set { 
				lowDate = value; 
				lowDateOnly = DateTimeExt.DateAsInteger(lowDate); 
			}
		}
		
		private int lowDateOnly;	// store the pre-calced year only integer
		private DateTime lowDate;

		/// <summary>
		///  Defines the upper Date of the range.
		/// </summary>
		public DateTime Top {
			get { 
				return highDate; 
			}
			set { 
				highDate = value; 
				highDateOnly = DateTimeExt.DateAsInteger(highDate); 
			}
		}
		
		private int highDateOnly;	// store the pre-calced year only integer
		private DateTime highDate;
		
		/// <summary>
		/// Default initializer
		/// </summary>
		public SearchCriteriaDateRange() {
			this.Bottom = DateTime.MinValue;
			this.Top = DateTime.Now;
		}
		
		/// <summary>
		/// Overloaded initializer
		/// </summary>
		/// <param name="bottom"></param>
		/// <param name="top"></param>
		public SearchCriteriaDateRange(DateTime bottom, DateTime top):this(){		
			this.Bottom = bottom; 
			this.Top = top;
		}
		
		
		/// <summary>
		/// interface impl. ISearchCriteria
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool Match(INewsItem item) { 
			// compare dates only
			int itemDate = DateTimeExt.DateAsInteger(item.Date);
			return itemDate > lowDateOnly && itemDate < highDateOnly;
		}  
		/// <summary>
		/// [Not yet implemented]
		/// </summary>
		/// <param name="feed"></param>
		/// <returns></returns>
		public override bool Match(FeedInfo feed) { 
					
			return false;
		} 
	}

	/// <summary>
	/// Implements ISearchCriteria to compare specific NewsItem properties
	/// </summary>
	[Serializable]
	public class SearchCriteriaProperty: ISearchCriteria {
		public bool BeenRead;
		public Flagged Flags;  //as defined in RssParser.cs
		public PropertyExpressionKind WhatKind;

		public SearchCriteriaProperty() {}
		
		// interface impl.
		public override bool Match(INewsItem item) { 
			switch(WhatKind){ 
			
				case PropertyExpressionKind.Flagged:
					return item.FlagStatus != Flagged.None;
	
				case PropertyExpressionKind.Unread:
					return item.BeenRead == this.BeenRead; 

				default:
					return false;
			}
		}  
		public override bool  Match(FeedInfo feed) { 
			//compare as defined ...
			return false;
		} 
	}
	#region BRIAN ADD
		/// <summary>
		/// Impl. BooleanSearch.IDocument to use more complicated queries
		/// </summary>
		public class SearchRssDocument:IDocument 
		{
			private INewsItem item;
			private SearchStringElement Where;
			private Regex htmlRegex = SearchCriteriaString.htmlRegex;

			public SearchRssDocument(INewsItem item,SearchStringElement where)
			{
				this.item=item;
				this.Where=where;
			}
			public bool Find(string What)
			{
				// moved from switch statement in Match(NewsItem item) function
				string lowerWhat = What.ToLower(); 
				if(((Where & SearchStringElement.Title) > 0) && item.Title != null)
				{
					if(item.Title.ToLower().IndexOf(lowerWhat)!= -1) return true;						
				}

				if(((Where & SearchStringElement.Link) > 0) && item.Link != null)
				{
					if(item.Link.ToLower().IndexOf(lowerWhat)!= -1) return true;						
				}

				if(((Where & SearchStringElement.Content) > 0) && item.Content != null)
				{								 						
					string strippedxhtml = htmlRegex.Replace(item.Content, String.Empty); 
					if(strippedxhtml.ToLower().IndexOf(lowerWhat)!= -1) return true;						
				}

				if(((Where & SearchStringElement.Subject) > 0) && item.Subject != null)
				{
					// BRIAN updated bug: What->lowerWhat
					if(item.Subject.ToLower().IndexOf(lowerWhat)!= -1) return true;						
				}
				return false;
			}

			public string Name()
			{
				return item.ToString();
			}

		}
	#endregion
	/// <summary>
	/// Impl. ISearchCriteria to compare an expression to specific NewsItems fields,
	/// that contains usually strings.
	/// </summary>
	[Serializable] 
	public class SearchCriteriaString:ISearchCriteria {
		
		public static Regex htmlRegex           = new Regex("</?[^>]+>"); 
		public static Regex placeholderRegex    = new Regex("\\$\\!\\$");

		public SearchCriteriaString() {}
		public SearchCriteriaString(string what, SearchStringElement where, StringExpressionKind kind) {
			this.What = what;
			this.Where = where;
			this.WhatKind = kind;
		}

		private SearchStringElement where;
		private string what;
		private StringExpressionKind whatKind;

		public SearchStringElement Where {
			get { return where; }
			set { where = value; currentExpressionRegex = null; }
		}
		public string What {
			get { return what; }
			set { what = value; currentExpressionRegex = null; }
		}
		public StringExpressionKind WhatKind {
			get { return whatKind; }
			set { whatKind = value; currentExpressionRegex = null; }
		}

		private Regex currentExpressionRegex = null;	// store the compiled Regex, if Kind is Regex
			

		// interface impl.
		public override bool Match(INewsItem item) 
		{ 
	
			switch(WhatKind){
									
				case StringExpressionKind.Text: 
					#region lucene
//					IndexSearcher searcher = new IndexSearcher(@"C:\your\index\directory");
//					Query q = QueryParser.Parse(What, LuceneSearch.IndexDocument.ItemContent, LuceneSearch.GetAnalyzer(CultureInfo.CurrentUICulture.Name));
//					Hits hits = searcher.Search(q);
					#endregion
					
					#region BRIAN ADD
						// http://www.devx.com/dotnet/Article/20650/0/page/3
						QueryBuilder builder = new QueryBuilder(What);
						if (builder.Validate())
						{
							// Query is valid, so build a tree
							QueryTree tree = builder.BuildTree();
							// Build list of resources to index
							IDocument[] docs = new IDocument[1];
							docs[0]= new SearchRssDocument(item,Where);
							// Retrieve matching documents
							IDocument[] matches = tree.GetMatches(docs);
							return matches.Length > 0;
						}
						else
						{
							// not valid
							return false;
						}
					#endregion

				case StringExpressionKind.RegularExpression:

					if (currentExpressionRegex == null) {	// create once, then hold as long What/Where/Kind does not change
						RegexOptions opts = RegexOptions.IgnoreCase | RegexOptions.Compiled;
						if ((Where & SearchStringElement.Content) > 0) {
							opts |= RegexOptions.Multiline;
						}
						currentExpressionRegex = new Regex(What, opts);
					}
					
					if(((Where & SearchStringElement.Title) > 0) && item.Title != null){
						if(currentExpressionRegex.Match(item.Title).Success)  return true;						
					}

					if(((Where & SearchStringElement.Link) > 0) && item.Link != null){
						if(currentExpressionRegex.Match(item.Link).Success)  return true;							
					}

					if(((Where & SearchStringElement.Content) > 0) && item.Content != null){
						string strippedxhtml = htmlRegex.Replace(item.Content, String.Empty); 						
						if(currentExpressionRegex.Match(strippedxhtml).Success)  return true;							
					}

					if(((Where & SearchStringElement.Subject) > 0) && item.Subject != null){
						if(currentExpressionRegex.Match(item.Subject).Success)  return true;							
					}

					return false; 

				case StringExpressionKind.XPathExpression:

                    //TODO: Disable XPath option in the UI
                    /* 
					XPathDocument doc  = new XPathDocument(new StringReader(item.ToString(NewsItemSerializationFormat.RssItem))); 
					XPathNavigator nav = doc.CreateNavigator(); 

					if((bool)nav.Evaluate("boolean(" + What + ")")){
						return true;
					}else{
						return false; 
					}
                     */ 
                    return false; 
				
				case StringExpressionKind.LuceneExpression:
					return true;	// strings are yet handled
					
				default: 
					return false; 
			}
			
		}  
		public override bool  Match(FeedInfo feed) { 
			//compare as defined ...
			return false;
		} 
	}
}
