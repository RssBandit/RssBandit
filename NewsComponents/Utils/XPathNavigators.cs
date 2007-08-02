using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using NewsComponents;


namespace NewsComponents.Utils{

	/*

	#region FeedDetailsNavigator

	/// <summary>
	/// This class implements an XPathNavigator over a FeedDetails object 
	/// </summary>
	internal class FeedDetailsNavigator : XPathNavigator{

	}

	#endregion 

#region NewsItemNavigator

	/// <summary>
	/// This class implements an XPathNavigator over a NewsItem object 
	/// </summary>
	internal class NewsItemNavigator : XPathNavigator{

		enum CurrentPosition{
		Root, 
		Rss, 
		Channel, 
		ChannelTitle,
		ChannelLink,
		ChannelDesc, 


		}

		#region Private fields
			
			/// <summary>
			/// The type of XPathNode currently positioned on
			/// </summary>
			private XPathNodeType nodeType; 

			/// <summary>
			/// Used for hashing names for optimized access
			/// </summary>
			private XmlNameTable nt = new NameTable();

			/// <summary>
			/// Indicates whether the navigator will be over the single 
			/// RSS item or over a representation of the parent RSS feed with this item 
			/// as a single item within it. 
			/// </summary>
			private NewsItemSerializationFormat serializationFormat; 

			/// <summary>
			/// Indicates whether the date should be GMT or local time
			/// </summary>
			private bool useGMTDate;

			/// <summary>
			/// The NewsItem object this XPathnavigator is positioned over 
			/// </summary>
			private NewsItem myItem; 


			/// <summary>
			/// This is the XPathNavigator over the feed this NewsItem belongs to
			/// </summary>
			private FeedDetailsNavigator feedNav;
			

		#endregion 

		#region Public properties 
		#endregion 


		#region Constructors 
		

			

		/// <summary>
		/// Initializes the NewsItemNavigator over the NewsItem. This assumes it is positioned
		/// over a NewsItem being treated as an RSS feed and dates are primted as GMT. 
		/// </summary>
		/// <param name="item">The NewsItem this XPathNavigator is positioned over</param>
		/// <param name="nav">The XPathNavigator over the parent RSS feed, if any</param>
		internal NewsItemNavigator(XmlNameTable nameTable, NewsItem item, FeedDetailsNavigator nav): this(nameTable, item, nav, NewsItemSerializationFormat.RssFeed, true){}

		/// <summary>
		/// Initializes the NewsItemNavigator. Dates are printed as GMT. 
		/// </summary>
		/// <param name="item">The NewsItem this XPathNavigator is positioned over</param>
		/// <param name="nav">The XPathNavigator over the parent RSS feed, if any</param>
		/// <param name="format">The XML format to serialize the document as</param>
		internal NewsItemNavigator(XmlNameTable nameTable, NewsItem item, FeedDetailsNavigator nav, NewsItemSerializationFormat format): this(nameTable, item, nav, format, true){}


		/// <summary>
		/// Initializes the NewsItemNavigator
		/// </summary>
		/// <param name="nameTable">The XML nametable</param>
		/// <param name="item">The NewsItem this XPathNavigator is positioned over</param>
		/// <param name="nav">The XPathNavigator over the parent RSS feed, if any</param>
		/// <param name="format">The XML format to serialize the document as</param>
		/// <param name="useGMTDate">indicates whether dates should be local time or GMT</param>
		internal NewsItemNavigator(XmlNameTable nameTable, NewsItem item, FeedDetailsNavigator nav, NewsItemSerializationFormat format, bool useGMTDate){
			this.nt = nameTable;
			this.myItem = item; 
			this.feedNav = nav; 
			this.serializationFormat = format; 
			this.useGMTDate = useGMTDate; 		
		}

		#endregion 

		#region Private methods 
		#endregion 

		#region Public methods 
		#endregion 
	}

	#endregion 


	#region ArrayListNavigator

	/// <summary>
	/// This class implements an XPathNavigator over an ArrayList of IXPathNavigable objects
	/// </summary>
	internal class ArrayListNavigator : XPathNavigator{

	}	

	#endregion 
	
	*/
}
