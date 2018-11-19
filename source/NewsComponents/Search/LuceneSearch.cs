
#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Lucene.Net.Analysis;
#if LUCENE_1_9
using Lucene.Net.Analysis.Br;
using Lucene.Net.Analysis.CJK;
using Lucene.Net.Analysis.Cn;
using Lucene.Net.Analysis.DE;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.Nl;
using Lucene.Net.Analysis.RU;
using Lucene.Net.Analysis.Sp;
using Lucene.Net.Search.Regex;
#else// lucene 2.0:
using Lucene.Net.Analysis.Snowball;
#endif
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.Resources;
using NewsComponents.Utils;
using RssBandit.Common.Logging;
using Directory=System.IO.Directory;

#endregion

namespace NewsComponents.Search
{

	/// <summary>
	/// Manages the process (instantiating the objects and hooking them together, 
	/// both for indexing and for searching), selecting the data files, 
	/// parsing the data files, getting the search string from the user 
	/// and displaying/providing the search results to the user 	
	/// </summary>
	/// <remarks>See also: 
	/// Project home page(s):
	/// http://incubator.apache.org/lucene.net/ (current project site)
	/// http://www.dotlucene.net/	(formerly project site)
	/// 
	/// Documentations:
	/// http://www.dotlucene.net/documentation/ToolforAnalyzingLuceneInd.html
	/// http://www.lucenebook.com/
	/// 
	/// Downloads:
	/// http://incubator.apache.org/lucene.net/download/
	/// </remarks>
	public sealed class LuceneSearch: IDisposable
	{
		/// <summary>
		/// Used in exceptions as the help link
		/// </summary>
		public const string HelpLink = "http://www.rssbandit.org/docs/search_query_syntax.htm";
		
		/// <summary>
		/// Raised on Indexing progress
		/// </summary>
		public event LuceneIndexingProgressEventHandler IndexingProgress;

		/// <summary>
		/// Raised, if the Indexing process finished.
		/// </summary>
		public event EventHandler IndexingFinished;

		/// <summary>
		/// Gets the default language used by lucene search and indexers
		/// </summary>
		public static string DefaultLanguage = "en";

		/// <summary>
		/// Defines all index relevant NewsFeed properties, 
		/// that are part of the lucene search index. On any change
		/// of these NewsFeed properties, that feed requires to be re-indexed!
		/// </summary>
		private readonly NewsFeedProperty indexRelevantPropertyChanges = 
			NewsFeedProperty.FeedLink |
			NewsFeedProperty.FeedUrl |
			NewsFeedProperty.FeedTitle |
			NewsFeedProperty.FeedCategory |
			NewsFeedProperty.FeedDescription |
			NewsFeedProperty.FeedType |
			NewsFeedProperty.FeedItemsDeleteUndelete |
			NewsFeedProperty.FeedAdded |
			NewsFeedProperty.FeedRemoved;
		
		private LuceneIndexModifier _indexModifier;
		private readonly LuceneSettings _settings;
        private readonly List<FeedSource> _feedSources = new List<FeedSource>(); 

		/// <summary>
		/// Is true, if we have to initially index all feeds
		/// </summary>
		private bool startIndexAll;

		// logging/tracing:
		private static readonly log4net.ILog _log = Log.GetLogger(typeof(LuceneSearch));

		#region ctor's

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <exception cref="ArgumentNullException">if newsHandler or configuration are null</exception>
		/// <exception cref="IOException">On indexPath directory creation failures</exception>
		/// <exception cref="SecurityException">On indexPath directory creation failures</exception>
		public LuceneSearch(INewsComponentsConfiguration configuration)
		{
			
			if (configuration.SearchIndexBehavior != SearchIndexBehavior.NoIndexing)
			{
				this._settings = new LuceneSettings(configuration);
				
				startIndexAll = (this._settings.IsRAMBasedSearch ||
					IsIndexCorrupted(this._settings.GetIndexDirectory()) ||
					!IndexReader.IndexExists(this._settings.GetIndexDirectory()));
				
				this._indexModifier = new LuceneIndexModifier(this._settings);
			}
			
		}

		#endregion
		
		#region public members

		/// <summary>
		/// Gets the settings.
		/// </summary>
		/// <value>The settings.</value>
		internal LuceneSettings Settings {
			get { return this._settings; }
		}

        /// <summary>
        /// The list of NewsHandlers being handled by this SearchHandler
        /// </summary>
        internal IList<FeedSource> FeedSources {
            get { return this._feedSources; }
        }

        /// <summary>
        /// Adds a FeedSource to the list of NewsHandlers being processed by this Search handler
        /// </summary>
        /// <param name="handler"></param>
        public void AddNewsHandler(FeedSource handler) {
            if (handler != null)
            {
                _feedSources.Add(handler);
            }
        }


		/// <summary>
		/// Executes a search.
		/// </summary>
		/// <param name="criteria">The criteria.</param>
		/// <param name="scope">The scope.</param>
		/// <param name="feedSources">The news handlers.</param>
		/// <param name="cultureName">Name of the culture.</param>
		/// <returns></returns>
		public Result ExecuteSearch(SearchCriteriaCollection criteria, INewsFeed[] scope, IEnumerable<FeedSource> feedSources, string cultureName) 
		{
			if (!UseIndex)
				return null;
			
			Query q = BuildLuceneQuery(criteria, scope, GetAnalyzer(cultureName));
			if (q == null)	// not validated
				return new Result(0, 0, GetList<SearchHitNewsItem>.Empty, GetArrayList.Empty);

			//TODO: to be fixed -
			// next line causes issues with concurrent thread access to the search index:
			IndexSearcher searcher = new IndexSearcher(this._settings.GetIndexDirectory());
			Hits hits = null;
			
			while (hits == null)
			{
				try {
					DateTime start = DateTime.Now;
					hits = searcher.Search(q, Sort.RELEVANCE);
					TimeSpan timeRequired = TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks);
					_log.Info(String.Format("Found {0} document(s) that matched query '{1}' (time required: {2})", hits.Length(), q,timeRequired));
				} catch (BooleanQuery.TooManyClauses) {
					BooleanQuery.SetMaxClauseCount(BooleanQuery.GetMaxClauseCount()*2);
					_log.Info(String.Format("Search failed with error 'BooleanQuery.TooManyClauses'. Retry with BooleanQuery.MaxClauseCount == {0}", BooleanQuery.GetMaxClauseCount()));
				}
			}

			List<SearchHitNewsItem> items = new List<SearchHitNewsItem>(hits.Length());
			HybridDictionary matchedFeeds = new HybridDictionary();

			
			for (int i = 0; i < hits.Length(); i++) {
				Document doc = hits.Doc(i);

				INewsFeed f = null;
				string feedLink = doc.Get(Keyword.FeedLink);
				if (matchedFeeds.Contains(feedLink))
					f = (INewsFeed) matchedFeeds[feedLink];

                if (f == null){
                    foreach (FeedSource h in feedSources)
                    {
                        if (h.IsSubscribed(feedLink))
                        {
                            f = h.GetFeeds()[feedLink];
                            break;
                        }
                    }
                }

				if (f == null) continue;
				SearchHitNewsItem item = new SearchHitNewsItem(f, 
					doc.Get(Keyword.ItemTitle),
					doc.Get(Keyword.ItemLink),
					doc.Get(IndexDocument.ItemSummary),
					doc.Get(Keyword.ItemAuthor),
					new DateTime(DateTools.StringToTime(doc.Get(Keyword.ItemDate))),
					LuceneNewsItemSearch.NewsItemIDFromUID(doc.Get(IndexDocument.ItemID)));
				
				items.Add(item);
				if (!matchedFeeds.Contains(feedLink))
					matchedFeeds.Add(feedLink, f);
				
			}
			
			
			return new Result(items.Count, matchedFeeds.Count, items, new ArrayList(matchedFeeds.Values));
			
		}

		/// <summary>
		/// Validates the search criteria.
		/// </summary>
		/// <param name="criteria">The criteria.</param>
		/// <param name="cultureName">Name of the culture.</param>
		/// <param name="validationException">The validation exception.</param>
		/// <returns></returns>
		public bool ValidateSearchCriteria(SearchCriteriaCollection criteria, string cultureName, out Exception validationException) {
			validationException = null;
			
			if (criteria == null || criteria.Count == 0) {
				validationException = new SearchException(ComponentsText.ExceptionSearchNoSearchCriteria);
				return false;
			}

			SearchCriteriaString criteriaProperty;
			foreach (ISearchCriteria sc in criteria)
			{
				criteriaProperty = sc as SearchCriteriaString;
				if (criteriaProperty != null)
				{
					if (StringExpressionKind.RegularExpression == criteriaProperty.WhatKind) {
						validationException = new SearchException(String.Format(ComponentsText.ExceptionLuceneSearchKindNotSupported, criteriaProperty.WhatKind));
						break;	// yet a warning
					}
					if (StringExpressionKind.XPathExpression == criteriaProperty.WhatKind) {
						validationException = new SearchException(String.Format(ComponentsText.ExceptionLuceneSearchKindNotSupported, criteriaProperty.WhatKind));
						return false;	// error
					}
				}
			}

			try {
				if (null == BuildLuceneQuery(criteria, null, GetAnalyzer(cultureName))) {
					validationException = new SearchException(ComponentsText.ExceptionSearchQueryBuilder);
					return false;
				}
			} catch (Exception ex) {
				validationException = new SearchException(String.Format(ComponentsText.ExceptionSearchQueryBuilderFatal, ex.Message), ex);
				return false;
			}
			
			return true;
		}
		
		
		private static Query BuildLuceneQuery(SearchCriteriaCollection criteria, INewsFeed[] scope, Analyzer analyzer) 
		{
			BooleanQuery masterQuery = null;
			BooleanQuery bTerms = new BooleanQuery();
			BooleanQuery bRanges = new BooleanQuery();

			for (int i=0; criteria != null && i< criteria.Count; i++) 
			{
				ISearchCriteria sc = criteria[i];
				if (sc is SearchCriteriaString) {
					SearchCriteriaString c = (SearchCriteriaString) sc;
				
					if (string.IsNullOrEmpty(c.What))
						continue;
					
					if (c.Where == SearchStringElement.Undefined) {
						AddBooleanClauseShould(bTerms, QueryFromStringExpression(c, IndexDocument.ItemContent, analyzer)); 
					} else {
						if((c.Where & SearchStringElement.Title) > 0){
							AddBooleanClauseShould(bTerms, QueryFromStringExpression(c, Keyword.ItemTitle, analyzer)); 
						}

						if((c.Where & SearchStringElement.Link) > 0){
							AddBooleanClauseShould(bTerms, QueryFromStringExpression(c, Keyword.ItemLink, analyzer)); 
						}

						if((c.Where & SearchStringElement.Content) > 0){
							AddBooleanClauseShould(bTerms, QueryFromStringExpression(c, IndexDocument.ItemContent, analyzer)); 
						}

						if((c.Where & SearchStringElement.Subject) > 0){
							AddBooleanClauseShould(bTerms, QueryFromStringExpression(c, Keyword.ItemTopic, analyzer)); 
						}

						if((c.Where & SearchStringElement.Author) > 0){
							AddBooleanClauseShould(bTerms, QueryFromStringExpression(c, Keyword.ItemAuthor, analyzer)); 
						}
					}
					
				} 
				else if (sc is SearchCriteriaAge) 
				{
					SearchCriteriaAge c = (SearchCriteriaAge) sc;
					Term left, right; 
					string pastDate = "19900101",
					       pastDateTime = "199001010001";
					string futureDate =DateTime.Now.AddYears(20).DateToInteger().ToString(NumberFormatInfo.InvariantInfo),
						   futureDateTime = DateTime.Now.AddYears(20).DateToInteger().ToString(NumberFormatInfo.InvariantInfo) + "0001";
					
					if (c.WhatRelativeToToday.CompareTo(TimeSpan.Zero) == 0) {
						// compare date only:
						//TODO: validate provided date(s) to be in the allowed ranges (pastDate, futureDate)!
						switch(c.WhatKind){	
							case DateExpressionKind.Equal:
								AddBooleanClauseMust(bRanges, new PrefixQuery(new Term(Keyword.ItemDate, c.WhatAsIntDateOnly.ToString(NumberFormatInfo.InvariantInfo))));  //itemDate == whatYearOnly;
								break;
							case DateExpressionKind.OlderThan:
								left = new Term(Keyword.ItemDate, pastDate);
								right = new Term(Keyword.ItemDate, c.What.DateToInteger().ToString(NumberFormatInfo.InvariantInfo));
								AddBooleanClauseMust(bRanges, new RangeQuery(left, right, true)); // return itemDate < whatYearOnly;
								break;
							case DateExpressionKind.NewerThan:
								left = new Term(Keyword.ItemDate, c.What.DateToInteger().ToString(NumberFormatInfo.InvariantInfo));
								right = new Term(Keyword.ItemDate, futureDate);
								AddBooleanClauseMust(bRanges, new RangeQuery(left, right, true)); // return itemDate > whatYearOnly;
								break;
								
							default:
								break; 
						}
					} else {
						DateTime dt = DateTime.Now.ToUniversalTime().Subtract(c.WhatRelativeToToday);
						switch(c.WhatKind){	
							case DateExpressionKind.OlderThan:
								left = new Term(Keyword.ItemDate, pastDateTime);
								right = new Term(Keyword.ItemDate, DateTools.TimeToString(dt.Ticks, DateTools.Resolution.MINUTE));
								AddBooleanClauseMust(bRanges, new RangeQuery(left, right, true));
								break;
							case DateExpressionKind.NewerThan:
								left = new Term(Keyword.ItemDate, DateTools.TimeToString(dt.Ticks, DateTools.Resolution.MINUTE));
								right = new Term(Keyword.ItemDate, futureDateTime);
								AddBooleanClauseMust(bRanges, new RangeQuery(left, right, true));
								break;
								
							default:
								break; 
						}
					}
					 
				}
				else if (sc is SearchCriteriaDateRange)
				{
					SearchCriteriaDateRange  c = (SearchCriteriaDateRange) sc;
					
					Term left = new Term(Keyword.ItemDate, c.Bottom.DateToInteger().ToString(NumberFormatInfo.InvariantInfo));
					Term right = new Term(Keyword.ItemDate, c.Top.DateToInteger().ToString(NumberFormatInfo.InvariantInfo));
					AddBooleanClauseMust(bRanges, new RangeQuery(left, right, true)); // return itemDate > whatYearOnly;
				}	
			}

			// now we build: +(terms...) +ranges
			if (bTerms.GetClauses().Length > 0) {
				masterQuery = new BooleanQuery();
				AddBooleanClauseMust(masterQuery, bTerms);
			}

			if (bRanges.GetClauses().Length > 0) {
				if (masterQuery != null)
					AddBooleanClauseMust(masterQuery, bRanges); // AND
				else
					masterQuery = bRanges;
			}

			// +scope
			if (scope != null && scope.Length > 0 && masterQuery != null)
			{
				StringBuilder scopeQuery = new StringBuilder("(");
				for (int i = 0; i < scope.Length; i++) {
					scopeQuery.Append(scope[i].id);
					scopeQuery.Append(" ");
				}
				scopeQuery[scopeQuery.Length-1] = ')';
				// AND
				AddBooleanClauseMust(masterQuery, QueryFromStringExpression(scopeQuery.ToString(), IndexDocument.FeedID, analyzer));
			}
			
			return masterQuery;
		}
		
#if LUCENE_1_9
		private static Query QueryFromStringExpression(SearchCriteriaString c, string field, Analyzer a) {
			if (c.WhatKind == StringExpressionKind.RegularExpression) {
				return new RegexQuery(new Term(field, c.What));
			} else {
				return QueryFromStringExpression(c.What, field, a);
			}
		}
		
		private static Query QueryFromStringExpression(string expression, string field, Analyzer a) {
			return QueryParser.Parse(c.What, field, a);
		}
		private static void AddBooleanClauseMust(BooleanQuery bq, Query q) {
			bq.Add(q, true, false);
		}
		private static void AddBooleanClauseShould(BooleanQuery bq, Query q) {
			bq.Add(q, false, false);
		}

#else // lucene 2.0:
		
		private static Query QueryFromStringExpression(SearchCriteriaString c, string field, Analyzer a)
		{
			if (c.WhatKind == StringExpressionKind.RegularExpression) {
				throw new NotSupportedException(String.Format(ComponentsText.ExceptionLuceneSearchKindNotSupported, c.WhatKind));
			}
			return QueryFromStringExpression(c.What, field, a);
		}

		private static Query QueryFromStringExpression(string expression, string field, Analyzer a) {
			return new QueryParser(field, a).Parse(expression);
		}
		private static void AddBooleanClauseMust(BooleanQuery bq, Query q) {
			bq.Add(q, BooleanClause.Occur.MUST);
		}
		private static void AddBooleanClauseShould(BooleanQuery bq, Query q) {
			bq.Add(q, BooleanClause.Occur.SHOULD);
		}
#endif
		
		/// <summary>
		/// Determines whether the changed specified properties 
		/// are index relevant changes.
		/// </summary>
		/// <param name="changedProperty">The changed property or properties.</param>
		/// <returns>
		/// 	<c>true</c> if it is a index relevant change; otherwise, <c>false</c>.
		/// </returns>
		public bool IsIndexRelevantChange(NewsFeedProperty changedProperty) {
			return (indexRelevantPropertyChanges & changedProperty) != NewsFeedProperty.None;
		}

		#region CheckIndex

		/// <summary>
		/// Checks the index. If it does not exists, this will start
		/// creating a new index. This process is recoverable/restartable and 
		/// can/should be called each time the Search is expected to be used.
		/// </summary>
		public void CheckIndex() {
			CheckIndex(false);
		}

		/// <summary>
		/// Checks the index. If it does not exists, this will start
		/// creating a new index. This process is recoverable/restartable and
		/// can/should be called each time the Search is expected to be used.
		/// </summary>
		/// <param name="force">if set to <c>true</c> a re-indexing of all
		/// items is started.</param>
		public void CheckIndex(bool force) 
		{
			if (!UseIndex) return;
			
			bool restartIndexing = false;
			bool fileBasedIndex = this._settings.IndexPath != null &&
			                      Directory.Exists(this._settings.IndexPath);
			string indexingStateFile = this.RestartIndexingStateFile;
			DateTime indexModifiedAt = DateTime.MinValue;

			if (fileBasedIndex)
			{
				
				indexModifiedAt = Directory.GetLastWriteTimeUtc(this._settings.IndexPath);
				bool indexStateFileExists = (indexingStateFile != null && File.Exists(indexingStateFile));
				
				// if the index have to be (re-)created, we don't want to rely on old restart infos:
				if ((force || startIndexAll) && indexStateFileExists)
					FileHelper.Delete(indexingStateFile);
				// restart indexing: read state and go on with the next non-indexed feed	
				restartIndexing = (! startIndexAll && indexStateFileExists); 

			} else {
				startIndexAll = true;
			}

			if (force || restartIndexing || startIndexAll) {
				IDictionary restartInfo = null;
				DictionaryEntry lastIndexed = new DictionaryEntry();
				if (restartIndexing) {
					// read indexState into restartInfo (persisted feed urls/id, that are yet indexed)
					restartInfo = ReadIndexingRestartStateFileContent(indexingStateFile, out lastIndexed);
				}
				if (restartInfo == null)
					restartInfo = new HybridDictionary();
				
				if (startIndexAll) 
					this._indexModifier.CreateIndex();
				
				LuceneIndexer indexer = GetIndexer();
				indexer.IndexingFinished += OnIndexingFinished;
				indexer.IndexingProgress += OnIndexingProgress;
				indexer.IndexAll(restartInfo, lastIndexed);
			} 
			else if (fileBasedIndex)
			{
				// check, if we have to call OptimizeIndex():
				DateTime lastIndexModification = this._settings.LastIndexOptimization;
				int compareResult = indexModifiedAt.CompareTo(lastIndexModification);
				// indexModifiedAt is greater than lastIndexModification, hence index was modified:
				if (compareResult > 0) {
					// attach event to get optimize index finished notification:
					this._indexModifier.FinishedIndexOperation += OnIndexModifierFinishedIndexOperation;
					this.IndexOptimize();
				} else if (compareResult != 0) {
					// correct the bad persisted entry:
					this._settings.LastIndexOptimization = Directory.GetLastWriteTimeUtc(this._settings.IndexPath);
				}
			}
			
		}
		
		private void OnIndexModifierFinishedIndexOperation(object sender, FinishedIndexOperationEventArgs e) {
			if (e.Operation.Action == IndexOperation.OptimizeIndex) {
				this._indexModifier.FinishedIndexOperation -= OnIndexModifierFinishedIndexOperation;
				this._settings.LastIndexOptimization = Directory.GetLastWriteTimeUtc(this._settings.IndexPath);
			}
		}
		
		#endregion

		#region IndexAdd

		/// <summary>
		/// Add the list of NewsItems to the lucene search index.
		/// </summary>
		/// <param name="newsItems">The news items list.</param>
		public void IndexAdd(IList<INewsItem> newsItems) {
			if (!UseIndex || newsItems == null) return;
			try {
				LuceneIndexer indexer = GetIndexer();
				indexer.IndexNewsItems(newsItems);

			} catch (Exception ex) {
				Log.Error("Failure while add item(s) to search index.", ex);
			}
		}

		/// <summary>
		/// Add the NewsItem to the lucene search index.
		/// </summary>
		/// <param name="item">The news item.</param>
		public void IndexAdd(INewsItem item) {
			if (!UseIndex || item == null) return;
			this.IndexAdd(new[]{item});
		}

		#endregion

		#region IndexRemove

		/// <summary>
		/// Remove the list of NewsItems from the lucene search index.
		/// </summary>
		/// <param name="newsItems">The news items list.</param>
		public void IndexRemove(INewsItem[] newsItems) {
			if (!UseIndex || newsItems == null) return;
			try {
				LuceneIndexer indexer = GetIndexer();
				indexer.RemoveNewsItems(newsItems);
			} catch (Exception ex) {
				Log.Error("Failure while remove item(s) from search index.", ex);
			}
		}
		
		/// <summary>
		/// Remove the list of NewsItems from the lucene search index.
		/// </summary>
		/// <param name="newsItem">The news items list.</param>
		public void IndexRemove(INewsItem newsItem) {
			if (!UseIndex || newsItem == null) return;
			this.IndexRemove(new[] {newsItem});
		}
		/// <summary>
		/// Remove the feed (and all it's items) from the lucene search index.
		/// </summary>
		/// <param name="feedID">The feed url.</param>
		public void IndexRemove(string feedID) {
			if (!UseIndex || string.IsNullOrEmpty(feedID)) return;
			try {
				LuceneIndexer indexer = GetIndexer();
				indexer.RemoveFeed(feedID);
			} catch (Exception ex) {
				Log.Error("Failure while remove item(s) from search index.", ex);
			}
			
		}

		/// <summary>
		/// Remove all indexed feeds (and all it's items) from the lucene search index.
		/// </summary>
		public void IndexRemoveAll() {
			if (!UseIndex) return;
			try {
				this._indexModifier.ResetIndex();
			} catch (Exception ex) {
				Log.Error("Failure while reset the whole index.", ex);
			}
			
		}
		#endregion

		#region ReIndex
//		/// <summary>
//		/// Re-Index a feed. First, the feed gets removed completely from index,
//		/// then the items are added to index again.
//		/// </summary>
//		/// <param name="feedID">The feed URL.</param>
//		/// <param name="newsItems">The news items.</param>
//		public void ReIndex(string feedID, IList newsItems) {
//			if (!UseIndex) return;
//			//WASTODO: way too general, we should optimize that:
//			// re-indexing only the really new items, and remove only
//			// the purged items:
//			this.IndexRemove(feedID);
//			this.IndexAdd(newsItems);
//		}

		/// <summary>
		/// Re-Index a feed. First, the feed gets removed completely from index,
		/// then the items are added to index again.
		/// </summary>
		/// <param name="feed">The feed.</param>
        /// <param name="items">The NewsItems belonging to the feed</param>
		public void ReIndex(INewsFeed feed, IList<INewsItem> items) {
			if (!UseIndex || feed == null) return;
			try {
				LuceneIndexer indexer = GetIndexer();
				indexer.RemoveNewsItems(feed.id); 
				indexer.IndexNewsItems(items);
			} catch (Exception ex) {
				Log.Error("Failure while ReIndex item(s) in search index.", ex);
			}
			//this.IndexRemove(feed.id);
			//this.IndexAdd(newsHandler.GetCachedItemsForFeed(feed.link));
		}

		#endregion

		#region IndexOptimize

		/// <summary>
		/// Optimize the search index.
		/// </summary>
		public void IndexOptimize() {
			if (!UseIndex) return;
			try {
				this._indexModifier.Optimize();
			} catch (Exception ex) {
				Log.Error("Failure while optimizing search index.", ex);
			}
		}

		#endregion

		/// <summary>
		/// Stops the indexer (background thread),
		/// performs all pending operations on the index and 
		/// flushes all pending I/O writes to disk. 
		/// </summary>
		public void StopIndexer() {
			if (!UseIndex) return;
			this._indexModifier.StopIndexer();
		}

		/// <summary> 
		/// Make sure all changes are written to disk (pending operations
		/// and index).
		/// </summary>
		public void Flush()
		{
			if (!UseIndex) return;
			this._indexModifier.Flush(false);
		}

		#endregion

		#region internal members
		
		/// <summary>
		/// Gets the analyzer matching the item.Language.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		internal static Analyzer GetAnalyzer(NewsItem item) {
			if (item == null)
				return new StandardAnalyzer();
			return GetAnalyzer(item.Language);
		} 

		/// <summary>
		/// Base method to get the analyzer.
		/// TODO: have to be changed as soon Lucene fully integrates
		/// the Snowball Analyzer using different language stemmers.
		/// See also: https://svn.apache.org/repos/asf/incubator/lucene.net/trunk/C%23/contrib/Snowball.Net/Snowball.Net/Lucene.Net/Analysis/Snowball/
		/// </summary>
		/// <param name="culture">The language.</param>
		/// <remarks>To get language support we use a custom build of Lucene.Net.dll
		/// including the various language packs. Italian was there, but did not compiled
		/// and so it is not supported here.</remarks>
		/// <returns></returns>
		internal static Analyzer GetAnalyzer(string culture) {
			culture = NormalizeCulture(culture);
			switch (culture) {
#if LUCENE_1_9
				case "de":	return new GermanAnalyzer();
				case "ru":	return new RussianAnalyzer();
				case "pt-br": return new BrazilianAnalyzer();
				case "nl-nl": return new DutchAnalyzer();
				case "fr":	return new FrenchAnalyzer();
				case "zh":	return new ChineseAnalyzer();
				case "ja":	return new CJKAnalyzer();	// use the Chinese/Japanese/Korean Analyzer
				case "ko":	return new CJKAnalyzer();	// use the Chinese/Japanese/Korean Analyzer
				case "es":	return new SpanishAnalyzer();
				default:	return new StandardAnalyzer();
#endif
				// Snowball/lucene 2.0:
				// Available stemmers are listed in {@link SF.Snowball.Ext}.  The name of a
				// stemmer is the part of the class name before "Stemmer", e.g., the stemmer in
				// {@link EnglishStemmer} is named "English".
				case "en":		return new SnowballAnalyzer("English");
				//TODO: review on next lucene update; Danish stemmer cause IndexOutOfRange exception in lucene.net 2.0 b004. Check in newer version(s) for a fix
				//case "da":		return new SnowballAnalyzer("Danish");
				case "de":		return new SnowballAnalyzer("German");
				case "es":		return new SnowballAnalyzer("Spanish");
				//TODO: review on next lucene update; also cause exceptions. See http://sourceforge.net/tracker/index.php?func=detail&aid=1859508&group_id=96589&atid=615248
				//case "fi":		return new SnowballAnalyzer("Finnish");
				case "fr":		return new SnowballAnalyzer("French");
				case "it":		return new SnowballAnalyzer("Italian");
				case "nl-nl":	return new SnowballAnalyzer("Dutch");
				case "no":		return new SnowballAnalyzer("Norwegian");
				case "pt":		return new SnowballAnalyzer("Portuguese");
				case "ru":		return new SnowballAnalyzer("Russian");
				case "sv":		return new SnowballAnalyzer("Swedish");
				default:		return new StandardAnalyzer();
			}
		}

		/// <summary>
		/// Normalizes the culture provided and returns 
		/// a culture string supported by the LuceneSearch 
		/// infrastructure (that can be mapped to a Analyzer class).
		/// </summary>
		/// <param name="culture">The language.</param>
		/// <returns></returns>
		internal static string NormalizeCulture(string culture) {
			if (string.IsNullOrEmpty(culture))
				return DefaultLanguage;
			culture = culture.ToLower(CultureInfo.InvariantCulture);
			if (culture == "en" || culture.StartsWith("en-")) return "en";
			if (culture == "de" || culture.StartsWith("de-")) return "de";
			if (culture == "da" || culture.StartsWith("da-")) return "da";
			if (culture == "es" || culture.StartsWith("es-")) return "es";
			if (culture == "fi" || culture.StartsWith("fi-")) return "fi";
			if (culture == "fr" || culture.StartsWith("fr-")) return "fr";
			if (culture == "it" || culture.StartsWith("it-")) return "it";
			if (culture == "ja" || culture.StartsWith("ja-")) return "ja";
			if (culture == "ko" || culture.StartsWith("ko-")) return "ko";
			if (culture == "nl-nl") return culture;
			if (culture == "no" || culture.StartsWith("nb-") || culture.StartsWith("nn-")) return "no";
			if (culture == "pt" || culture.StartsWith("pt-")) return "pt";
			if (culture == "ru" || culture.StartsWith("ru-")) return "ru";
			if (culture == "sv" || culture.StartsWith("sv-")) return "sv";
			if (culture == "zh" || culture.StartsWith("zh-")) return "zh";

			return DefaultLanguage;
		}
		#endregion

		#region private members

		private bool IsIndexCorrupted(Lucene.Net.Store.Directory directory)
		{
			if (directory is FSDirectory && IndexReader.IndexExists(directory))
			{
				bool unexpectedProcessTermination = IndexReader.IsLocked(directory);
				if (unexpectedProcessTermination) try
					{
						_log.Error("Try to cleanup index after unexpected Process Termination...");
						Directory.Delete(_settings.IndexPath, true);
						Directory.CreateDirectory(_settings.IndexPath);
						return true;
					}
					catch (Exception accessEx)
					{
						_log.Error("Could not cleanup index after unexpected Process Termination. Try simply unlock now.", accessEx);
						try
						{
							IndexReader.Unlock(directory);
						} 
						catch (Exception ex)
						{
							_log.Error("Could not unlock index after unexpected Process Termination. Giving up now.", ex);
						
						}
					}
			}
			return false;
		}

		private LuceneIndexer _indexer;
		/// <summary>
		/// Creates the indexer.
		/// </summary>
		/// <returns></returns>
		private LuceneIndexer GetIndexer() 
		{
			if (_indexer == null)
				_indexer = new LuceneIndexer(this._indexModifier, _feedSources);
			return _indexer;
		}

		private bool UseIndex {
			get { 
				return this._settings != null && (
				       this._settings.SearchIndexBehavior != SearchIndexBehavior.NoIndexing ||
					this._indexModifier != null); 
			}
		}

		private bool RaiseIndexingProgress(LuceneIndexingProgressCancelEventArgs e) {
			bool toReturn = false;
			if (IndexingProgress != null) {
				IndexingProgress(this, e);
				return e.Cancel;
			}
			return toReturn;
		}
		
		private void RaiseIndexingFinished() {
			if (IndexingFinished != null)
				try { IndexingFinished(this, EventArgs.Empty); } catch {}
		}

		private static IDictionary ReadIndexingRestartStateFileContent(string indexStateFile, out DictionaryEntry lastIndexed) {
			HybridDictionary toReturn = new HybridDictionary();
			lastIndexed = new DictionaryEntry();

			if (File.Exists(indexStateFile)) 
			{
				using(StreamReader reader = File.OpenText(indexStateFile)) 
				{
					while (reader.Peek() >= 0) 
					{
						DictionaryEntry de = ReadIndexingState(reader);
							
						// feed Url is the key:
						if (!string.IsNullOrEmpty((string)de.Key) && 
							!string.IsNullOrEmpty((string)de.Value)) 
						{
							// remember last indexed:
							lastIndexed = de;
							if (! toReturn.Contains(de.Key))
								toReturn.Add(de.Key, de.Value);
						}
					}
				}
			}
			return toReturn;
		}

		

		private static DictionaryEntry ReadIndexingState(TextReader reader) {
			string line = reader.ReadLine();	
			if (!StringHelper.EmptyTrimOrNull(line)) 
			{
				string[] fields = line.Split(new[]{'\t'});
				if (fields.Length > 1) {
					// feed Url is the key:
					return new DictionaryEntry(fields[1], fields[0]);
				}
			}
			return new DictionaryEntry();
		}

		private static void WriteIndexingState(TextWriter writer, LuceneIndexingProgressCancelEventArgs e) {
			writer.Write(e.YetIndexedFeedID);
			writer.Write("\t");
			writer.WriteLine(e.YetIndexedFeedUrl);
			writer.Flush();
		}
		/// <summary>
		/// Gets the restart indexing state file path.
		/// </summary>
		/// <value>The index state file.</value>
		private string RestartIndexingStateFile {
			get {
				if (this._settings.IndexPath != null)
					return Path.Combine(this._settings.IndexPath, "index.state");
				return null;
			}
		}

		private void OnIndexingFinished(object sender, EventArgs e) {
			try {
				if (File.Exists(this.RestartIndexingStateFile))
					FileHelper.Delete(this.RestartIndexingStateFile);
			} catch (Exception ex) {
				Log.Error("Cannot delete '" + this.RestartIndexingStateFile + "': " + ex.Message, ex);
			}
			
			_indexModifier.Flush(false);
			RaiseIndexingFinished();
		}

		private void OnIndexingProgress(object sender, LuceneIndexingProgressCancelEventArgs e) {

			if (File.Exists(this.RestartIndexingStateFile)) {
				try {
					using (StreamWriter writer = File.AppendText(this.RestartIndexingStateFile) ) {
						WriteIndexingState(writer, e);
					}
				} catch (Exception ex) {
					Log.Error("Cannot append to '" + this.RestartIndexingStateFile + "': " + ex.Message, ex);
				}
			} else {
				try {
					using (StreamWriter writer = File.CreateText(this.RestartIndexingStateFile) ) {
						WriteIndexingState(writer, e);
					}
				} catch (Exception ex) {
					Log.Error("Cannot create to '" + this.RestartIndexingStateFile + "': " + ex.Message, ex);
				}
			}
			
			e.Cancel = this.RaiseIndexingProgress(e);
		}
		

		#endregion


		/// <summary>
		/// Container for our known lucense search key words
		/// </summary>
		public sealed class Keyword {
			// feed item keywords
			public const string ItemAuthor = "author";
			public const string ItemTitle = "title";
			public const string ItemLink = "link";
			public const string ItemDate = "date";
			public const string ItemTopic = "topic";

			// feed keywords
			public const string FeedLink = "feedlink";	// required to post-process a search for scope
			public const string FeedUrl = "feedurl";
			public const string FeedTitle = "feedtitle";
			//not used. Way too dynamic (user can change it, drag feeds around)
			//public const string FeedCategory = "feedcategory";
			public const string FeedDescription = "feeddescription";
			public const string FeedType = "feedtype";	// RSS or NNTP
		}
	
		/// <summary>
		/// Used internal only to identify index documents
		/// for maintenance (remove/refresh), define the major
		/// search field and a summery to display.
		/// </summary>
		internal sealed class IndexDocument
		{
			internal const string ItemID = "iid";
			internal const string FeedID = "fid";
			internal const string ItemContent = "content";
			internal const string ItemSummary = "summery";
		}
		
		/// <summary>
		/// Container of a search result
		/// </summary>
		public class Result
		{
			public readonly int ItemMatchCount;
			public readonly int FeedMatchCount;
			public IList<SearchHitNewsItem> ItemsMatched;
			public IList FeedsMatched;

			/// <summary>
			/// Initializes a new instance of the <see cref="Result"/> class.
			/// </summary>
			/// <param name="itemMatches">The item matches.</param>
			/// <param name="feedMatches">The feed matches.</param>
			/// <param name="itemsMatched">The items matched.</param>
			/// <param name="feedsMatched">The feeds matched.</param>
			public Result(int itemMatches, int feedMatches, IList<SearchHitNewsItem> itemsMatched, IList feedsMatched)
			{
				this.ItemMatchCount = itemMatches;
				this.FeedMatchCount = feedMatches;
				this.ItemsMatched = itemsMatched;
				this.FeedsMatched = feedsMatched;
			}
		}

		/// <summary>
		/// Search Exception. May be caused on search expression validation
		/// or other search specific errors.
		/// </summary>
		[ComVisible(false),Serializable]
		public class SearchException : ApplicationException
		{
			/// <summary></summary>
			public SearchException() : 
				this(ComponentsText.ExceptionSearchFatal, null) {
			}
			/// <summary></summary>
			public SearchException(string message) : 
				this(message, null) {
			}
			/// <summary></summary>
			public SearchException(string message, Exception innerException) : 
				base(message, innerException) {
				base.HelpLink = LuceneSearch.HelpLink;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_indexModifier!= null)
					_indexModifier.Dispose();
				_indexModifier = null;
			}
		}
	}

    internal static class DateTimeExtensions
    {
        /// <summary>
        /// Returns the date part as integer in the format YYYYMMDD.
        /// </summary>
        /// <param name="dateTime">DateTime (only date part is relevant)</param>
        /// <returns>Int32</returns>
        public static int DateToInteger(this DateTime dateTime)
        {
            return dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        }

    }

}
