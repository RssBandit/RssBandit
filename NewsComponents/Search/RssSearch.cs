#region CVS Version Header
/*
 * $Id: RssSearch.cs,v 1.7 2005/05/12 14:10:47 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/12 14:10:47 $
 * $Revision: 1.7 $
 */
#endregion

using System;
using System.Collections;
using NewsComponents.Feed;
using System.Text.RegularExpressions;
using System.Xml.XPath; 
using System.Xml.Serialization;

using FeedInfo = NewsComponents.Feed.FeedInfo; 
using NewsComponents.Search.BooleanSearch;

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
		public abstract bool Match(NewsItem item);
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
		/// Search within all (title and content and subject and link)
		/// </summary>
		All = Title | Content | Subject | Link
	}
	
	/// <summary>
	/// Defines the kind of the search expression
	/// </summary>
	public enum StringExpressionKind {
		/// <summary>Simple text expression</summary>
		Text,
		/// <summary>Works like "SQL like" expression</summary>
		LikeExpression,
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
		NewerThan
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
		public bool Match(NewsItem item) { 
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
		[System.Xml.Serialization.XmlElementAttribute("WhatRelativeToToday")]
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
		[System.Xml.Serialization.XmlIgnoreAttribute()]
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
				whatYearOnly = what.Year *10000 + what.Month * 100 + what.Day; 
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
		public override bool Match(NewsItem item) { 
			if (this.WhatRelativeToToday.CompareTo(TimeSpan.Zero) == 0) {
				// compare dates only
				int itemDate = item.Date.Year *10000 + item.Date.Month * 100 + item.Date.Day;
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
	/// Implements ISearchCriteria to compare specific NewsItem properties
	/// </summary>
	[Serializable]
	public class SearchCriteriaProperty: ISearchCriteria {
		public bool BeenRead;
		public Flagged Flags;  //as defined in RssParser.cs
		public PropertyExpressionKind WhatKind;

		public SearchCriteriaProperty() {}
		
		// interface impl.
		public override bool Match(NewsItem item) { 
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
		public class SearchRssDocument:BooleanSearch.IDocument 
		{
			private NewsItem item;
			private SearchStringElement Where;
			private Regex htmlRegex = SearchCriteriaString.htmlRegex;

			public SearchRssDocument(NewsItem item,SearchStringElement where)
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
		public override bool Match(NewsItem item) 
		{ 
	
			switch(WhatKind){
									
				case StringExpressionKind.Text: 
					#region BRIAN ADD
						// http://www.devx.com/dotnet/Article/20650/0/page/3
						BooleanSearch.QueryBuilder builder = new BooleanSearch.QueryBuilder(What);
						if (builder.Validate())
						{
							// Query is valid, so build a tree
							BooleanSearch.QueryTree tree = builder.BuildTree();
							// Build list of resources to index
							BooleanSearch.IDocument[] docs = new BooleanSearch.IDocument[1];
							docs[0]= new SearchRssDocument(item,Where);
							// Retrieve matching documents
							BooleanSearch.IDocument[] matches = tree.GetMatches(docs);
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

					XPathDocument doc  = new XPathDocument(new System.IO.StringReader(item.ToString(NewsItemSerializationFormat.RssItem))); 
					XPathNavigator nav = doc.CreateNavigator(); 

					if((bool)nav.Evaluate("boolean(" + What + ")")){
						return true;
					}else{
						return false; 
					}
			
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
