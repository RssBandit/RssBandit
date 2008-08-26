using System;

namespace NewsComponents
{

	public enum FeedType {
		/// <summary>
		/// RSS Feed (RSS1/2.x, Atom). Handled by RssParser
		/// </summary>
		Rss,
		/// <summary>
		/// NNTP Feed. Handled by NntpHandler
		/// </summary>
		Nntp
	}

	/// <summary>
	/// NewsItem description content types.
	/// </summary>
	public enum ContentType{		
		/// <summary>No content available</summary>
		None,
		/// <summary>Unknown or not supported</summary>
		Unknown,
		/// <summary>Simple text</summary>
		Text, 
		/// <summary>HTML formated text</summary>
		Html, 
		/// <summary>XHTML formated</summary>
		Xhtml
	}


	/// <summary>
	/// Channel Processing types
	/// </summary>
	public enum ChannelProcessingType
	{
		/// <summary>
		/// Process NewsItem's
		/// </summary>
		NewsItem,

		/// <summary>
		/// Process feeds (RSS, Atom)
		/// </summary>
		Feed,
		/* for future support:
		/// <summary>
		/// Process nntp
		/// </summary>
		Nntp,
		*/
	}

	/// <summary>
	/// indicates the sort order a feed should be displayed in.
	/// Used to use the same (int) values than system.Windows.Forms.SortOrder enum
	/// </summary>
	public enum SortOrder{
		None,
		Ascending, 
		Descending, 
	}
	
	///// <summary>
	///// Indicates whether a column layout is global, category-wide or feed specific.
	///// </summary>
	//public enum LayoutType {
	//    IndividualLayout, 
	//    GlobalFeedLayout, 
	//    GlobalCategoryLayout, 
	//    SearchFolderLayout, 
	//    SpecialFeedsLayout, 
	//}
}
